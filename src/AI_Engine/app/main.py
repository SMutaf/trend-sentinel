import time
import schedule
from datetime import datetime
from app.core.config import Config
from app.core.enums import (
    SentimentType,
    DirectionType,
    TimeHorizonType,
    NewsEventType,
    SectorType,
    normalize_sentiment,
    normalize_direction,
    normalize_time_horizon,
    normalize_event_type
)
from app.scrapers.yahoo_scraper import YahooFinanceScraper
from app.engines.llm_engine import LLMEngine
from app.engines.technical_engine import TechnicalEngine
from app.services.backend_service import BackendService
import urllib3

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

scraper = YahooFinanceScraper()
llm_brain = LLMEngine()
tech_engine = TechnicalEngine()
backend = BackendService()

def job():
    print(f"\n--- ZİNCİRLEME ANALİZ MODU: {datetime.now().strftime('%H:%M:%S')} ---")
    
    existing_companies = backend.get_companies()
    existing_tickers = {c['tickerSymbol']: c['id'] for c in existing_companies}
    target_tickers = scraper.get_trending_tickers()
    
    for ticker in target_tickers:
        news_list = scraper.fetch_latest_news(ticker, limit=1)
        if not news_list:
            continue
        
        news = news_list[0]
        print(f"\n{ticker} İnceleniyor: '{news['title'][:40]}...'")
        
        company_id = existing_tickers.get(ticker)
        history_logs = []
        if company_id:
            history_logs = backend.get_recent_logs(company_id, limit=5)
            print(f"    Hafıza: {len(history_logs)} geçmiş haber bulundu.")
        
        # --- MÜKERRER KONTROLÜ ---
        is_duplicate = any(log.get('url') == news['link'] for log in history_logs)
        if is_duplicate:
            print(f"    Bu haberi daha önce işlemişiz, pas geçiliyor.")
            continue
        
        news_pub_date = news.get('pubDate', 'Bilinmiyor')
        
        # 1. Aşama: YAPAY ZEKA TEMEL ANALİZ (Haber ne diyor?)
        analysis = llm_brain.analyze_news_with_history(
            news['title'],
            news['summary'],
            news_pub_date,
            history_logs
        )
        
        should_save = analysis.get('shouldSave', False)
        impact = analysis.get('impactStrength', 1)
        confidence = analysis.get('confidenceScore', 0)
        direction = analysis.get('expectedDirection', DirectionType.UNCERTAIN.value)
        detected_sector = analysis.get('sectorId', SectorType.OTHER.value)
        
        # Sentiment'i direction'dan türet (enum formatında)
        if direction == DirectionType.UP.value:
            sentiment = SentimentType.POSITIVE.value
        elif direction == DirectionType.DOWN.value:
            sentiment = SentimentType.NEGATIVE.value
        else:
            sentiment = SentimentType.NEUTRAL.value
        
        # 2. Aşama: TEKNİK ANALİZ (Fiyat/Grafik ne diyor?)
        tech_data = tech_engine.evaluate_technicals(ticker, direction)
        tech_score = tech_data.get('tech_score', 0)
        vol_trend = tech_data.get('vol_trend', 'Weak')
        vol_ratio = tech_data.get('vol_ratio', 0.0)
        
        # STRATEJİ 1: BREAKOUT (Yukarı Patlama)
        # Çok güçlü haber + Hacim patlaması + Yüksek güven
        is_breakout = (
            direction == DirectionType.UP.value and
            impact >= 4 and
            confidence >= 75 and
            vol_ratio >= 1.5 and
            vol_trend == "Strong"
        )
        
        # STRATEJİ 2: SLOW TREND (İstikrarlı Yükseliş)
        # İyi haber + Teknik puan iyi + Hacim destekliyor
        is_slow_trend = (
            direction == DirectionType.UP.value and
            impact >= 3 and
            confidence >= 70 and
            tech_score >= 2 and
            vol_trend in ["Strong", "Moderate"]
        )
        
        # STRATEJİ 3: BREAKDOWN (Aşağı Kırılım)
        # Güçlü düşüş haberi + Teknik onay + Hacim baskısı
        is_breakdown = (
            direction == DirectionType.DOWN.value and
            impact >= 4 and
            confidence >= 75 and
            tech_score >= 2 and
            vol_trend in ["Strong", "Moderate"]
        )
        
        # Ana Trend Kararı
        is_trend = is_breakout or is_slow_trend or is_breakdown
        trend_reason = ""
        if is_breakout: trend_reason = "BREAKOUT (Hacim Patlaması)"
        elif is_slow_trend: trend_reason = "SLOW TREND (Teknik+Hacim)"
        elif is_breakdown: trend_reason = "BREAKDOWN (Aşağı Kırılım)"
        
        if should_save or is_trend:
            print(f"    Önemli Gelişme! (Trend: {is_trend}, Yön: {direction})")
            print(f"      Detay: LLM:{impact}/5, Güven:%{confidence}, Tech:{tech_score}, Hacim:{vol_trend} (x{vol_ratio})")
            if is_trend:
                print(f"      Strateji: {trend_reason}")
            
            # Şirket yoksa önce şirketi C#'a kaydet
            if not company_id:
                print(f"    Yeni Takip Başlatılıyor: {ticker} (Sektör ID: {detected_sector})")
                new_company = backend.create_company(ticker, ticker, detected_sector)
                if new_company:
                    company_id = new_company['id']
                    existing_tickers[ticker] = company_id
            
            # 3. Aşama: VERİTABANINA ZİNCİRLEME KAYIT
            if company_id:
                log_payload = {
                    "companyId": company_id,
                    "title": news['title'],
                    "url": news['link'],
                    "summary": news['summary'][:500],
                    "isTrendTriggered": is_trend,
                    "trendSummary": analysis.get('trendSummary', '')[:500],
                    "sentimentLabel": sentiment,  # Enum formatında
                    "publishedDate": datetime.now().isoformat(),
                    "eventType": analysis.get('eventType', NewsEventType.OTHER.value),  # Enum formatında
                    "impactStrength": impact,
                    "expectedDirection": direction,  # Enum formatında
                    "timeHorizon": analysis.get('timeHorizon', TimeHorizonType.SHORT_TERM.value),  # Enum formatında
                    "overextendedRisk": tech_data['is_overextended'],
                    "confidenceScore": confidence,
                    "sectorId": detected_sector,
                }
                
                saved_log = backend.send_log(log_payload)
                if saved_log and 'id' in saved_log:
                    news_log_id = saved_log['id']
                    print(f"    Haber veritabanına işlendi. (ID: {news_log_id[:8]}...)")
                    
                    # Fiyat snapshot
                    price_snapshot = tech_data.get('price_snapshot')
                    if price_snapshot:
                        price_payload = {
                            "newsLogId": news_log_id,
                            "date": price_snapshot['date'],
                            "open": price_snapshot['open'],
                            "high": price_snapshot['high'],
                            "low": price_snapshot['low'],
                            "close": price_snapshot['close'],
                            "volume": price_snapshot['volume']
                        }
                        backend.send_price_history(price_payload)
                    
                    # Teknik snapshot
                    technical_payload = {
                        "newsLogId": news_log_id,
                        "rsiValue": tech_data['rsi'],
                        "macdState": tech_data['macd_state'],
                        "techScore": tech_data['tech_score'],
                        "isOverextended": tech_data['is_overextended'],
                        "volRatio": tech_data['vol_ratio'],
                        "volTrend": tech_data['vol_trend'],
                        "aboveAvgDaysLast5": tech_data['above_avg_days']
                    }
                    backend.send_technical_snapshot(technical_payload)
                    print(f"    Fiyat & Teknik Snapshot Kaydedildi! (RSI: {tech_data['rsi']}, MACD: {tech_data['macd_state']})")
                else:
                    print(f"    Haber kaydedilemedi, Snapshot'lar iptal edildi.")
            else:
                print("    Haber kaydedilemedi, Snapshot'lar iptal edildi.")
        else:
            print("    Önemsiz haber, pas geçildi.")
        
        time.sleep(10)
    
    print(f"\nTur tamamlandı. {Config.CHECK_INTERVAL_MINUTES} dakika bekleniyor...")

def start():
    print("--- TREND SENTINEL V5: HEDGE FUND META MODEL ---")
    job()
    schedule.every(Config.CHECK_INTERVAL_MINUTES).minutes.do(job)
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == "__main__":
    start()