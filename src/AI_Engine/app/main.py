import time
import schedule
from datetime import datetime
from app.core.config import Config
from app.scrapers.yahoo_scraper import YahooFinanceScraper
from app.engines.llm_engine import LLMEngine 
from app.engines.technical_engine import TechnicalEngine # yeni eklenen engine
from app.services.backend_service import BackendService
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning) # logdaki uyarıları anlık olarak göstememek için

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
            history_logs = backend.get_recent_logs(company_id)
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
        direction = analysis.get('expectedDirection', 'Uncertain')
        
        # Sektör verisi
        detected_sector = analysis.get('sectorId', 0)

        if direction == "Up":
            sentiment = "Positive"
        elif direction == "Down":
            sentiment = "Negative"
        else:
            sentiment = "Neutral"

        # 2. Aşama: TEKNİK ANALİZ (Fiyat/Grafik ne diyor?)
        # Snapshot (Fiyat fotoğrafı) ve İndikatörleri (RSI, MACD) hesapla
        tech_data = tech_engine.evaluate_technicals(ticker, direction)
        
        # Yeni Kantitatif Formül: AI Güveni + Teknik Onay (RSI ucuz mu/şişmiş mi?)
        is_trend = (impact >= 4) and (confidence >= 75)
        
        if should_save or is_trend:
            print(f"    Önemli Gelişme Tespit Edildi! (Trend: {is_trend}, LLM Etki: {impact}/5, Tech Puanı: {tech_data['tech_score']})")
            
            # Şirket yoksa önce şirketi C#'a kaydet
            if not company_id:
                print(f"    Yeni Takip Başlatılıyor: {ticker} (Sektör ID: {detected_sector})")
                new_company = backend.create_company(ticker, ticker, detected_sector)
                if new_company:
                    company_id = new_company['id']
                    existing_tickers[ticker] = company_id
            
            # 3. Aşama: VERİTABANINA ZİNCİRLEME KAYIT
            if company_id:
                # Önce Haberi (NewsLog) yolla
                log_payload = {
                    "companyId": company_id,
                    "title": news['title'],
                    "url": news['link'],
                    "summary": news['summary'][:500],
                    "isTrendTriggered": is_trend,
                    "trendSummary": analysis.get('trendSummary', '')[:500],
                    "sentimentLabel": sentiment,
                    "publishedDate": datetime.now().isoformat(),
                    "eventType": analysis.get('eventType', 'Generic News'),
                    "impactStrength": impact,
                    "expectedDirection": direction,
                    "timeHorizon": analysis.get('timeHorizon', 'ShortTerm'),
                    "overextendedRisk": tech_data['is_overextended'], # Artık Teknik Motor Karar Veriyor !! burasının düzenlensmesi gereke
                    "confidenceScore": confidence
                }
                
                # C#'tan dönen t NewsLog'u (ve ID'sini) yakala
                saved_log = backend.send_log(log_payload)
                
                if saved_log and 'id' in saved_log:
                    news_log_id = saved_log['id']
                    print(f"    Haber veritabanına işlendi. (ID: {news_log_id[:8]}...)")
                    
                    # Eğer fiyata dair Snapshot (Olay Anı Fotoğrafı) alınabilmişse
                    price_snapshot = tech_data.get('price_snapshot')
                    if price_snapshot:
                        # SAF FİYATLARI (PriceHistory) yolla -> NewsLogId ile eşleştirerek
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
                        
                        # TEKNİK İNDİKATÖRLERİ (EventTechnicalSnapshot) yolla -> NewsLogId ile eşleştirerek
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
            print("    Önemsiz haber, pas geçildi.")
            
        time.sleep(10)

    print(f"\nTur tamamlandı. {Config.CHECK_INTERVAL_MINUTES} dakika bekleniyor...")

def start():
    print("""--- TREND SENTINEL V5: HEDGE FUND META MODEL ---""")
    job()
    schedule.every(Config.CHECK_INTERVAL_MINUTES).minutes.do(job)
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == "__main__":
    start()