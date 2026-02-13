import requests
import xml.etree.ElementTree as ET

class YahooFinanceScraper:
    def __init__(self):
        # Yahoo Finance RSS adresi (s= sembol parametresi)
        self.base_url = "https://finance.yahoo.com/rss/headline?s={ticker}"

    def fetch_latest_news(self, ticker: str, limit: int = 3):
        """
        Belirtilen hisse (ticker) için son haberleri çeker.
        """
        url = self.base_url.format(ticker=ticker)
        print(f"{ticker} için haberler taranıyor...")
        
        try:
            # Bot  anlaşılmasın diye tarayıcı gibi davranma (user-agent)
            headers = {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
            }
            
            # İsteği gönder (10 saniye zaman aşımı)
            response = requests.get(url, headers=headers, timeout=10)
            
            if response.status_code != 200:
                print(f"Hata: Yahoo yanıt vermedi (Kod: {response.status_code})")
                return []

            # Gelen XML verisini işle
            root = ET.fromstring(response.content)
            news_list = []
            
            # XML içindeki <item> etiketlerini bul (Haberler burada)
            # Limit kadar haberi al (varsayılan 3)
            for item in root.findall('.//item')[:limit]:
                news_item = {
                    "title": item.find('title').text,
                    "link": item.find('link').text,
                    # Açıklama bazen boş gelebilir, kontrol ediyoruz
                    "summary": item.find('description').text if item.find('description') is not None else "Özet yok.",
                    "pubDate": item.find('pubDate').text
                }
                news_list.append(news_item)
                
            return news_list

        except Exception as e:
            print(f"Scraping Hatası ({ticker}): {e}")
            return []