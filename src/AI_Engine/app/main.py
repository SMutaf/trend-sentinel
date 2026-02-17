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
    
    # 1. Mevcut Şirketleri Önbelleğe Al (Cache)
    existing_companies = backend.get_companies()
    existing_tickers = {c['tickerSymbol']: c['id'] for c in existing_companies}
    
    # 2. Yahoo'dan Potansiyel Haberleri Çek (Trending Listesi)
    target_tickers = scraper.get_trending_tickers() # ["NVDA", "TSLA", "A_SIRKETI"...]
    
    for ticker in target_tickers:
        news_list = scraper.fetch_latest_news(ticker, limit=1)
        if not news_list: continue
        
        news = news_list[0]
        print(f"{ticker} İnceleniyor: '{news['title'][:30]}...'")

        # --- ADIM A: ŞİRKET BAĞLANTISI KUR ---
        company_id = existing_tickers.get(ticker)
        history_logs = []

        # Eğer şirket bizde kayıtlıysa, geçmişini (Hafızayı) getir
        if company_id:
            history_logs = backend.get_recent_logs(company_id)
            print(f"Hafıza: {len(history_logs)} geçmiş haber bulundu.")
        
        # --- ADIM B: AI KARAR MEKANİZMASI ---
        # Şirket yoksa bile analiz edilir. Önemli haber olma ihitmali var.
        analysis = ai_brain.analyze_news_with_history(news['title'], news['summary'], history_logs)
        
        should_save = analysis.get('shouldSave', False)
        is_trend = analysis.get('isTrendTriggered', False)
        
        if should_save or is_trend:
            print(f"    Önemli Gelişme Tespit Edildi! (Trend: {is_trend})")
            
            # Şirket yoksa şimdi oluştur (Çünkü kaydetmeye değer bir haber bulundu)
            if not company_id:
                print(f"   ➕ Yeni Takip Başlatılıyor: {ticker}")
                new_company = backend.create_company(ticker, ticker)
                if new_company:
                    company_id = new_company['id']
                    existing_tickers[ticker] = company_id
            
            # Haberi Kaydet
            if company_id:
                payload = {
                    "companyId": company_id,
                    "title": news['title'],
                    "url": news['link'],
                    "summary": news['summary'][:500],
                    "isTrendTriggered": is_trend, # Telegram sadece bu True ise ötecek
                    "trendSummary": analysis.get('trendSummary', ''),
                    "sentimentLabel": analysis.get('sentimentLabel', 'Neutral'),
                    "publishedDate": datetime.now().isoformat()
                }
                backend.send_log(payload)
                print(f"   ✅ Veritabanına işlendi.")
        else:
            print("     Önemsiz haber, pas geçildi.")
            
        time.sleep(2)

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