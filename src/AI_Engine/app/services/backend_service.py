import requests
from app.core.config import Config

class BackendService:
    def __init__(self):
        self.base_url = Config.BACKEND_URL
    
    def get_companies(self):
        url = f"{self.base_url}/Companies" if self.base_url.endswith("/api") else f"{self.base_url}/api/Companies"
        try:
            response = requests.get(url, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Şirket Listesi Çekme Hatası: {e}")
            return []
    
    def create_company(self, name, ticker, sector_id=0):
        url = f"{self.base_url}/Companies" if self.base_url.endswith("/api") else f"{self.base_url}/api/Companies"
        payload = {"name": name, "tickerSymbol": ticker, "sector": sector_id}
        try:
            response = requests.post(url, json=payload, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Şirket Kayıt Hatası: {e}")
            return None
    
    def get_recent_logs(self, company_id, limit=50):
        """
        Veritabanından son haberleri çeker.
        Duplicate kontrolü için limit'i 50'ye çıkardık (daha güvenli)
        """
        url = f"{self.base_url}/NewsLogs/{company_id}" if self.base_url.endswith("/api") else f"{self.base_url}/api/NewsLogs/{company_id}"
        try:
            response = requests.get(
                url,
                params={"limit": limit},
                timeout=10,
                verify=False
            )
            
            # 404 = Henüz haber yok, bu bir hata değil → boş liste dön
            if response.status_code == 404:
                return []
            
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Geçmiş Logları Çekme Hatası: {e}")
            return []
    
    def send_log(self, payload):
        """ Haberi kaydeder ve C# API'den dönen NewsLog nesnesini (ve ID'sini) geri döndürür. """
        url = f"{self.base_url}/NewsLogs" if self.base_url.endswith("/api") else f"{self.base_url}/api/NewsLogs"
        try:
            response = requests.post(url, json=payload, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Haber Kayıt Hatası: {e}")
            if e.response is not None:
                print(f"API Detayı: {e.response.text}")
            return None
    
    def send_price_history(self, payload):
        """ Sadece SAF FİYAT verilerini (Open, High, Low, Close, Volume) NewsLogId ile kaydeder. """
        endpoint = "/PriceHistories" if self.base_url.endswith("/api") else "/api/PriceHistories"
        url = f"{self.base_url}{endpoint}"
        try:
            response = requests.post(url, json=payload, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"PriceHistory Kayıt Hatası: {e}")
            if e.response is not None:
                print(f"API Detayı: {e.response.text}")
            return None
    
    def send_technical_snapshot(self, payload):
        """ Sadece TEKNİK İNDİKATÖR verilerini (RSI, MACD, vs.) NewsLogId ile kaydeder. """
        endpoint = "/EventTechnicalSnapshots" if self.base_url.endswith("/api") else "/api/EventTechnicalSnapshots"
        url = f"{self.base_url}{endpoint}"
        try:
            response = requests.post(url, json=payload, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Technical Snapshot Kayıt Hatası: {e}")
            if e.response is not None:
                print(f"API Detayı: {e.response.text}")
            return None