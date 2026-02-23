import yfinance as yf
import pandas as pd
from datetime import datetime, date

class TechnicalEngine:
    def __init__(self):
        # Önbellek yapısı: { 'BTC-USD': { 'date': '2026-02-23', 'data': DataFrame } }
        self.daily_candle_cache = {}

    # GÜNLÜK VERİ + İNDİKATÖR ÖNBELLEĞİ
    def _get_or_build_daily_data(self, ticker: str):
        today = date.today().isoformat()

        # Önbellekte bugünün verisi var mı?
        if (
            ticker in self.daily_candle_cache and
            self.daily_candle_cache[ticker]["date"] == today
        ):
            return self.daily_candle_cache[ticker]["data"].copy()

        print(f"[Cache Miss] {ticker} daily candles + indicators hesaplanıyor...")

        # Yahoo'dan veriyi çek
        stock = yf.Ticker(ticker)
        # Hacim ortalaması için en az 20-30 güne ihtiyacımız var, garanti olsun diye 1 yıllık çekiyoruz
        df = stock.history(period="1y", interval="1d")

        if df.empty or len(df) < 35:
            return pd.DataFrame()

        # İndikatörleri Hesapla
        df = self._calculate_indicators(df)

        # Önbelleğe at
        self.daily_candle_cache[ticker] = {
            "date": today,
            "data": df
        }

        return df.copy()

    # İNDİKATÖR HESAPLAMALARI 
    def _calculate_indicators(self, df: pd.DataFrame):
        close_prices = df["Close"]
        volume = df["Volume"]

        # -------- MACD --------
        ema_12 = close_prices.ewm(span=12, adjust=False).mean()
        ema_26 = close_prices.ewm(span=26, adjust=False).mean()
        df["MACD_Line"] = ema_12 - ema_26
        df["MACD_Signal"] = df["MACD_Line"].ewm(span=9, adjust=False).mean()

        # -------- RSI --------
        delta = close_prices.diff()
        gain = delta.where(delta > 0, 0.0)
        loss = -delta.where(delta < 0, 0.0)
        avg_gain = gain.ewm(alpha=1/14, adjust=False).mean()
        avg_loss = loss.ewm(alpha=1/14, adjust=False).mean()
        avg_loss = avg_loss.replace(0, 1e-10) # Sıfıra bölünme hatasını önle
        rs = avg_gain / avg_loss
        df["RSI"] = 100 - (100 / (1 + rs))

        # -------- HACİM ANALİZİ  --------
        # 1. Son 20 Günlük Hacim Ortalaması (SMA 20)
        df["Vol_SMA20"] = volume.rolling(window=20).mean()

        # 2. Hacim Oranı (Bugünkü Hacim / Ortalama)
        # Eğer ortalama 0 ise hatayı önlemek için 1 kabul et
        df["Vol_Ratio"] = volume / df["Vol_SMA20"].replace(0, 1)

        # 3. Son 5 günde kaç gün ortalamanın üzerindeydi?
        # True/False serisi: Hacim > Ortalama mı?
        df["Vol_Above_Avg"] = df["Volume"] > df["Vol_SMA20"]
        
        # Son 5 gündeki "True" sayısını topla
        df["Above_Avg_Days_Last5"] = df["Vol_Above_Avg"].rolling(window=5).sum()

        return df

    # SNAPSHOT MOTORU
    def evaluate_technicals(self, ticker: str, llm_direction: str):
        print(f"[Technical Engine] {ticker} event snapshot hazırlanıyor...")

        try:
            df = self._get_or_build_daily_data(ticker)
            if df.empty: return self._default_snapshot()

            # ANLIK (INTRADAY) GÜNCELLEME
            # Günlük veri dün kapanış olabilir, o anki canlı fiyatı ve hacmi alıp son satırı güncelliyoruz.
            try:
                intraday = yf.Ticker(ticker).history(period="1d", interval="1m")
                if not intraday.empty:
                    latest_tick = intraday.iloc[-1]
                    
                    # Son satırdaki Fiyatları Güncelle
                    df.loc[df.index[-1], "Close"] = latest_tick["Close"]
                    df.loc[df.index[-1], "High"] = max(df.iloc[-1]["High"], latest_tick["High"])
                    df.loc[df.index[-1], "Low"] = min(df.iloc[-1]["Low"], latest_tick["Low"])
                    
                    # Eğer gün içi kümülatif hacim yoksa bu veri biraz düşük görünebilir ama trendi yakalar.
                    current_vol = latest_tick["Volume"]
                    if current_vol > 0:
                        df.loc[df.index[-1], "Volume"] = current_vol

                    snapshot_date = latest_tick.name.isoformat()
                    
                    # Veriler değiştiği için indikatörleri (RSI, Hacim Ortalaması vb.) tekrar hesaplatıyoruz
                    df = self._calculate_indicators(df)
                else:
                    snapshot_date = datetime.now().isoformat()
            except Exception:
                snapshot_date = datetime.now().isoformat()

            # SON DEĞERLERİ AL
            latest = df.iloc[-1]

            # RSI & MACD
            rsi_value = round(float(latest.get("RSI", 50.0)), 2)
            if pd.isna(rsi_value): rsi_value = 50.0
            
            macd_line = float(latest.get("MACD_Line", 0.0))
            macd_signal = float(latest.get("MACD_Signal", 0.0))
            is_macd_bullish = macd_line > macd_signal
            macd_state = "Bullish" if is_macd_bullish else "Bearish"

            # HACİM METRİKLERİ 
            vol_ratio = round(float(latest.get("Vol_Ratio", 0.0)), 2)
            above_avg_days = int(latest.get("Above_Avg_Days_Last5", 0))

            # Volume Trend Karar Mekanizması
            # Eğer hacim ortalamanın 1.5 katıysa veya son 3 gün ortalama üstündeyse "Güçlü"
            volume_trend = "Weak"
            if vol_ratio > 1.5 or above_avg_days >= 3:
                volume_trend = "Strong"
            elif vol_ratio > 1.0 or above_avg_days >= 2:
                volume_trend = "Moderate"

            # TEKNİK PUANLAMA (Volume Etkisi Eklendi)
            score = 0
            is_overextended = False

            # RSI Kuralları
            if llm_direction == "Up" and rsi_value < 65: score += 2
            if llm_direction == "Up" and rsi_value > 75: 
                score -= 2
                is_overextended = True # RSI çok şişmiş, düzeltme gelebilir
            if llm_direction == "Down" and rsi_value < 30: 
                score -= 2
                is_overextended = True

            # MACD Kuralları
            if llm_direction == "Up" and is_macd_bullish: score += 1
            elif llm_direction == "Down" and not is_macd_bullish: score += 1
            
            # HACİM KURALI 🚀
            # Eğer yön yukarı ve hacim "Strong" ise trend çok sağlamdır. +1 Puan.
            if llm_direction == "Up" and volume_trend == "Strong":
                score += 1

            # Fiyat Paketi
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
                "vol_ratio": vol_ratio,       
                "vol_trend": volume_trend,    
                "above_avg_days": above_avg_days, 
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
            "vol_ratio": 0.0,
            "vol_trend": "Weak",
            "above_avg_days": 0,
            "tech_score": 0,
            "is_overextended": False,
            "price_snapshot": None
        }