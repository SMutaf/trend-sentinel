import requests
import xml.etree.ElementTree as ET

class YahooFinanceScraper:
    def __init__(self):
        self.base_url = "https://finance.yahoo.com/rss/headline?s={ticker}"

    # --- EKSİK OLAN YENİ FONKSİYON BURASI ---
    def get_trending_tickers(self):
        """
        Piyasadaki popüler hisseleri manuel veya otomatik belirler.
        Şimdilik 'Avcı Modu' testi için popüler hisseleri dönüyoruz.
        """
        return ["NVDA", "TSLA", "AAPL", "AMD", "PLTR", "AMZN", "GOOGL", "MSFT", "META"]

    def fetch_latest_news(self, ticker: str, limit: int = 3):
        url = self.base_url.format(ticker=ticker)
        
        try:
            headers = {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
            }
            response = requests.get(url, headers=headers, timeout=10)
            
            if response.status_code != 200:
                return []

            root = ET.fromstring(response.content)
            news_list = []
            
            for item in root.findall('.//item')[:limit]:
                news_item = {
                    "title": item.find('title').text,
                    "link": item.find('link').text,
                    "summary": item.find('description').text if item.find('description') is not None else "Özet yok.",
                    "pubDate": item.find('pubDate').text
                }
                news_list.append(news_item)
                
            return news_list

        except Exception as e:
            return []