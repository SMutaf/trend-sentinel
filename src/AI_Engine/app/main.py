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
            
            # --- MÜKERRER KONTROLÜ ---
            is_duplicate = any(log.get('url') == news['link'] for log in history_logs)
            if is_duplicate:
                print(f"Bu haberi daha önce işlemişiz, pas geçiliyor.")
                continue # Döngüyü atla, Gemini'ye sorma
            # -------------------------------------------------------------
        
        # --- ADIM B: AI KARAR MEKANİZMASI ---
        # 1. Haberin yayın saatini alıyoruz
        news_pub_date = news.get('pubDate', 'Bilinmiyor')
        
        # 2. (saat bilgisi dahil) AI'ı çağırıyoruz
        analysis = ai_brain.analyze_news_with_history(news['title'], news['summary'], news_pub_date, history_logs)
        
        should_save = analysis.get('shouldSave', False)
        
        # --- MATEMATİKSEL TREND VE QUANT VERİLERİ ---
        impact = analysis.get('impactStrength', 1)
        confidence = analysis.get('confidenceScore', 0)
        
        # Formülümüz: Etkisi 4 veya 5 olacak VE yapay zeka en az %75 emin olacak
        is_trend = (impact >= 4) and (confidence >= 75)
        
        # Sentiment yönünü AI'ın 'expectedDirection' tahmininden belirliyoruz
        direction = analysis.get('expectedDirection', 'Uncertain')
        if direction == "Up":
            sentiment = "Positive"
        elif direction == "Down":
            sentiment = "Negative"
        else:
            sentiment = "Neutral"

        if should_save or is_trend:
            print(f"    Önemli Gelişme Tespit Edildi! (Trend: {is_trend}, Etki: {impact}/5, Güven: %{confidence})")
            
            if not company_id:
                detected_sector = analysis.get('sectorId', 0)
                print(f"Yeni Takip Başlatılıyor: {ticker} (Sektör: {detected_sector})")
                # sektörü de C#'a gönderiyoruz
                new_company = backend.create_company(ticker, ticker, detected_sector)
                if new_company:
                    company_id = new_company['id']
                    existing_tickers[ticker] = company_id
            
            if company_id:
                # C#'ın beklediği YENİ standart payload
                payload = {
                    "companyId": company_id,
                    "title": news['title'],
                    "url": news['link'],
                    "summary": news['summary'][:500],
                    "isTrendTriggered": is_trend,
                    "trendSummary": analysis.get('trendSummary', '')[:500],
                    "sentimentLabel": sentiment,
                    "publishedDate": datetime.now().isoformat(),
                    
                    # --- YENİ EKLENEN QUANT ALANLARI ---
                    "eventType": analysis.get('eventType', 'Generic News'),
                    "impactStrength": impact,
                    "expectedDirection": direction,
                    "timeHorizon": analysis.get('timeHorizon', 'ShortTerm'),
                    "overextendedRisk": analysis.get('overextendedRisk', False),
                    "confidenceScore": confidence
                }
                backend.send_log(payload)
                print(f"    Veritabanına işlendi.")
        else:
            print("     Önemsiz haber, pas geçildi.")
            
        # "Dakikada Maksimum" hatasına takılmamak için bekleme
        time.sleep(10) 

    print(f"Tur tamamlandı. {Config.CHECK_INTERVAL_MINUTES} dakika bekleniyor...")

def start():
    print("""--- TREND SENTINEL V4: QUANT ANALYST MODE ---""")
    job()
    schedule.every(Config.CHECK_INTERVAL_MINUTES).minutes.do(job)
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == "__main__":
    start()