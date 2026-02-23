import yfinance as yf
import pandas as pd
from datetime import datetime, date


class TechnicalEngine:
    def __init__(self):
        """
        Cache format:
        {
            'AAPL': {
                'date': '2026-02-22',
                'data': DataFrame (INDICATORS INCLUDED)
            }
        }
        """
        self.daily_candle_cache = {}

    # DAILY DATA + INDICATOR CACHE
    def _get_or_build_daily_data(self, ticker: str):
        today = date.today().isoformat()

        # Cache hit
        if (
            ticker in self.daily_candle_cache and
            self.daily_candle_cache[ticker]["date"] == today
        ):
            return self.daily_candle_cache[ticker]["data"].copy()

        # Cache miss
        print(f"[Cache Miss] {ticker} daily candles + indicators hesaplanıyor...")

        stock = yf.Ticker(ticker)
        df = stock.history(period="1y", interval="1d")

        if df.empty or len(df) < 35:
            return pd.DataFrame()

        df = self._calculate_indicators(df)

        self.daily_candle_cache[ticker] = {
            "date": today,
            "data": df
        }

        return df.copy()

    # INDICATOR CALCULATION
    def _calculate_indicators(self, df: pd.DataFrame):

        close_prices = df["Close"]

        # -------- MACD --------
        ema_12 = close_prices.ewm(span=12, adjust=False).mean()
        ema_26 = close_prices.ewm(span=26, adjust=False).mean()

        df["MACD_Line"] = ema_12 - ema_26
        df["MACD_Signal"] = df["MACD_Line"].ewm(span=9, adjust=False).mean()

        # -------- RSI (Wilder) --------
        delta = close_prices.diff()
        gain = delta.where(delta > 0, 0.0)
        loss = -delta.where(delta < 0, 0.0)

        avg_gain = gain.ewm(alpha=1/14, adjust=False).mean()
        avg_loss = loss.ewm(alpha=1/14, adjust=False).mean()

        # Zero division protection
        avg_loss = avg_loss.replace(0, 1e-10)

        rs = avg_gain / avg_loss
        df["RSI"] = 100 - (100 / (1 + rs))

        return df

    # EVENT SNAPSHOT ENGINE
    def evaluate_technicals(self, ticker: str, llm_direction: str):

        print(f"[Technical Engine] {ticker} event snapshot hazırlanıyor...")

        try:
            df = self._get_or_build_daily_data(ticker)

            if df.empty:
                return self._default_snapshot()

            # INTRADAY UPDATE (1m latest price)
            try:
                intraday = yf.Ticker(ticker).history(period="1d", interval="1m")

                if not intraday.empty:
                    latest_tick = intraday.iloc[-1]

                    # Update price fields only
                    df.loc[df.index[-1], "Close"] = latest_tick["Close"]
                    df.loc[df.index[-1], "High"] = max(
                        df.iloc[-1]["High"],
                        latest_tick["High"]
                    )
                    df.loc[df.index[-1], "Low"] = min(
                        df.iloc[-1]["Low"],
                        latest_tick["Low"]
                    )

                    # Volume overwrite (double counting engellendi)
                    df.loc[df.index[-1], "Volume"] = latest_tick["Volume"]

                    snapshot_date = latest_tick.name.isoformat()

                    # Fiyat değişti → indikatörleri yeniden hesapla
                    df = self._calculate_indicators(df)

                else:
                    snapshot_date = datetime.now().isoformat()

            except Exception:
                snapshot_date = datetime.now().isoformat()

            # FINAL VALUES
            latest = df.iloc[-1]

            rsi_value = round(float(latest.get("RSI", 50.0)), 2)
            if pd.isna(rsi_value):
                rsi_value = 50.0

            macd_line = float(latest.get("MACD_Line", 0.0))
            macd_signal = float(latest.get("MACD_Signal", 0.0))

            is_macd_bullish = macd_line > macd_signal
            macd_state = "Bullish" if is_macd_bullish else "Bearish"

            # TECH ALIGNMENT SCORE
            score = 0
            is_overextended = False

            if llm_direction == "Up" and rsi_value < 65:
                score += 2

            if llm_direction == "Up" and rsi_value > 75:
                score -= 2
                is_overextended = True

            if llm_direction == "Down" and rsi_value < 30:
                score -= 2
                is_overextended = True

            if llm_direction == "Up" and is_macd_bullish:
                score += 1
            elif llm_direction == "Down" and not is_macd_bullish:
                score += 1

            # SNAPSHOT PAYLOAD
            price_snapshot = {
                "date": snapshot_date,
                "open": float(latest.get("Open", 0.0)),
                "high": float(latest.get("High", 0.0)),
                "low": float(latest.get("Low", 0.0)),
                "close": float(latest.get("Close", 0.0)),
                "volume": int(latest.get("Volume", 0))
            }

            return {
                "rsi": rsi_value,
                "macd_state": macd_state,
                "tech_score": score,
                "is_overextended": is_overextended,
                "price_snapshot": price_snapshot
            }

        except Exception as e:
            print(f"[Hata] Teknik analiz başarısız ({ticker}): {e}")
            return self._default_snapshot()

    def _default_snapshot(self):
        return {
            "rsi": 50.0,
            "macd_state": "Neutral",
            "tech_score": 0,
            "is_overextended": False,
            "price_snapshot": None
        }