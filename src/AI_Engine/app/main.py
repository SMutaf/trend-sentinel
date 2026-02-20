import time
import schedule
from datetime import datetime
from app.core.config import Config
from app.scrapers.yahoo_scraper import YahooFinanceScraper
from app.services.gemini_service import GeminiService
from app.services.backend_service import BackendService

scraper = YahooFinanceScraper()
ai_brain = GeminiService()
backend = BackendService()

def job():
    print(f"\n--- ZİNCİRLEME ANALİZ MODU: {datetime.now().strftime('%H:%M:%S')} ---")
    
    existing_companies = backend.get_companies()
    existing_tickers = {c['tickerSymbol']: c['id'] for c in existing_companies}
    target_tickers = scraper.get_trending_tickers() 
    
    for ticker in target_tickers:
        news_list = scraper.fetch_latest_news(ticker, limit=1)
        if not news_list: continue
        
        news = news_list[0]
        print(f"{ticker} İnceleniyor: '{news['title'][:30]}...'")

        company_id = existing_tickers.get(ticker)
        history_logs = []

        if company_id:
            history_logs = backend.get_recent_logs(company_id)
            print(f"Hafıza: {len(history_logs)} geçmiş haber bulundu.")
            
            # --- 1. YENİ EKLENEN: MÜKERRER KONTROLÜ (KOTAYI KURTARIR!) ---
            is_duplicate = any(log.get('url') == news['link'] for log in history_logs)
            if is_duplicate:
                print(f" ⏭️ Bu haberi daha önce işlemişiz, pas geçiliyor.")
                continue # Döngüyü atla, Gemini'ye hiç sorma!
            # -------------------------------------------------------------
        
        # --- ADIM B: AI KARAR MEKANİZMASI ---
        analysis = ai_brain.analyze_news_with_history(news['title'], news['summary'], history_logs)
        
        should_save = analysis.get('shouldSave', False)
        is_trend = analysis.get('isTrendTriggered', False)
        
        if should_save or is_trend:
            print(f"    Önemli Gelişme Tespit Edildi! (Trend: {is_trend})")
            
            if not company_id:
                print(f"   ➕ Yeni Takip Başlatılıyor: {ticker}")
                new_company = backend.create_company(ticker, ticker)
                if new_company:
                    company_id = new_company['id']
                    existing_tickers[ticker] = company_id
            
            if company_id:
                payload = {
                    "companyId": company_id,
                    "title": news['title'],
                    "url": news['link'],
                    "summary": news['summary'][:500],
                    "isTrendTriggered": is_trend,
                    "trendSummary": analysis.get('trendSummary', ''),
                    "sentimentLabel": analysis.get('sentimentLabel', 'Neutral'),
                    "publishedDate": datetime.now().isoformat()
                }
                backend.send_log(payload)
                print(f"   ✅ Veritabanına işlendi.")
        else:
            print("     Önemsiz haber, pas geçildi.")
            
        # --- 2. DEĞİŞEN: BEKLEME SÜRESİNİ UZAT ---
        # "Dakikada Maksimum" hatasına takılmamak için 2 saniye yerine 10 saniye yapıyoruz.
        time.sleep(10) 

    print(f"Tur tamamlandı. {Config.CHECK_INTERVAL_MINUTES} dakika bekleniyor...")

def start():
    print("""--- TREND SENTINEL V3: CONTEXT AWARE MODE ---""")
    job()
    schedule.every(Config.CHECK_INTERVAL_MINUTES).minutes.do(job)
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == "__main__":
    start()