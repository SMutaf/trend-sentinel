import requests
import xml.etree.ElementTree as ET

class YahooFinanceScraper:
    def __init__(self):
        self.rss_url = "https://finance.yahoo.com/rss/headline?s={ticker}"
        # Yahoo'nun canlı trend hisselerini veren API'si
        self.trending_url = "https://query1.finance.yahoo.com/v1/finance/trending/US"
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }

    def get_trending_tickers(self):
        """
        Yahoo Finance üzerinden o anki en popüler (Trending) hisseleri canlı çeker.
        """
        try:
            response = requests.get(self.trending_url, headers=self.headers, timeout=10)
            if response.status_code == 200:
                data = response.json()
                quotes = data['finance']['result'][0]['quotes']
                # Sadece sembolleri al (Örn: ['NVDA', 'SMCI', 'BTC-USD'...])
                tickers = [quote['symbol'] for quote in quotes]
                return tickers[:10] # En popüler 10 hisseyi döndür
            return []
        except Exception as e:
            print(f"Trend Ticker Çekme Hatası: {e}")
            # Eğer API çökerse yedek liste (Fallback) daha sonra düzenlenicek
            return ["NVDA", "TSLA", "AAPL", "AMD", "PLTR"]

    def fetch_latest_news(self, ticker: str, limit: int = 1):
        url = self.rss_url.format(ticker=ticker)
        
        try:
            response = requests.get(url, headers=self.headers, timeout=10)
            
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