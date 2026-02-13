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
    print(f"\n--- DÖNGÜ BAŞLADI: {datetime.now().strftime('%H:%M:%S')} ---")
    
    # 1. Takip Edilecek Şirketleri Getir
    companies = backend.get_companies()
    
    if not companies:
        print("⚠️ Hata: Takip edilecek şirket bulunamadı veya API kapalı.")
        return

    print(f"📊 Takip Listesi: {len(companies)} şirket var.")

    for company in companies:
        company_name = company.get('name')
        ticker = company.get('tickerSymbol')
        company_id = company.get('id')
        
        # 2. Haberleri Çek (Her şirket için son 1 haber yeterli şimdilik)
        news_list = scraper.fetch_latest_news(ticker, limit=1)
        
        if not news_list:
            print(f"   -> {ticker}: Haber yok.")
            continue
            
        for news in news_list:
            print(f"   -> {ticker}: Haber bulundu! '{news['title'][:30]}...'")
            print(f"      🧠 Gemini Analiz Ediyor...")

            # 3. AI Analizi Yap
            analysis = ai_brain.analyze_news(news['title'], news['summary'])
            
            # 4. Sonucu Hazırla
            payload = {
                "companyId": company_id,
                "title": news['title'],
                "url": news['link'],
                "summary": news['summary'][:500],
                
                # AI Sonuçları
                "isTrendTriggered": analysis.get('isTrendTriggered', False),
                "trendSummary": analysis.get('trendSummary', 'Analiz Yok'),
                "sentimentLabel": analysis.get('sentimentLabel', 'Neutral'),
                
                "publishedDate": datetime.now().isoformat()
            }
            
            # 5. Backend'e Gönder (Telegram otomatik tetiklenecek)
            backend.send_log(payload)
            
            # API'yi boğmamak timeout
            time.sleep(2) 

    print(f"Döngü bitti. {Config.CHECK_INTERVAL_MINUTES} dakika bekleniyor...")

def start():
    print("""--- TREND SENTINEL AI MOTORU BAŞLATILDI ---""")
    print(f"Kontrol Aralığı: {Config.CHECK_INTERVAL_MINUTES} dakika")
    
    # İlk açılışta beklemeden hemen bir kez çalıştır
    job()
    
    # Zamanlayıcıyı kur
    schedule.every(Config.CHECK_INTERVAL_MINUTES).minutes.do(job)
    
    # Sonsuz döngü (Programı açık tutar)
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == "__main__":
    start()