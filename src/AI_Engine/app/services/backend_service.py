import requests
from app.core.config import Config

class BackendService:
    def __init__(self):
        self.base_url = Config.BACKEND_URL
    
    def get_companies(self):
        url = f"{self.base_url}/api/Companies" if not self.base_url.endswith("/api") else f"{self.base_url}/Companies"
        try:
            response = requests.get(url, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Şirket Listesi Çekme Hatası: {e}")
            return []
    
    def create_company(self, name, ticker, sector_id=0):
        url = f"{self.base_url}/api/Companies" if not self.base_url.endswith("/api") else f"{self.base_url}/Companies"
        payload = {"name": name, "tickerSymbol": ticker, "sectorId": sector_id}
        try:
            response = requests.post(url, json=payload, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Şirket Kayıt Hatası: {e}")
            return None
    
    def get_recent_logs(self, company_id):
        url = f"{self.base_url}/api/NewsLogs/{company_id}" if not self.base_url.endswith("/api") else f"{self.base_url}/NewsLogs/{company_id}"
        try:
            response = requests.get(url, timeout=10, verify=False)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            if e.response is not None and e.response.status_code == 404:
                return []
            print(f"Geçmiş Logları Çekme Hatası: {e}")
            return []
    
    def send_log(self, payload):
        """ Haberi kaydeder ve C# API'den dönen NewsLog nesnesini (ve ID'sini) geri döndürür. """
        url = f"{self.base_url}/api/NewsLogs" if not self.base_url.endswith("/api") else f"{self.base_url}/NewsLogs"
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
    
    def create_signal_track(self, news_log_id, entry_price, target_duration_days=10):
        """
        Trend sinyali için takip kaydı oluşturur (SignalTrack).
        Bu, hisseyi backtest izleme listesine alır.
        """
        endpoint = "/SignalTracks" if self.base_url.endswith("/api") else "/api/SignalTracks"
        url = f"{self.base_url}{endpoint}"
        
        payload = {
            "newsLogId": news_log_id,
            "entryPrice": entry_price,
            "targetDurationDays": target_duration_days
        }
        
        try:
            response = requests.post(url, json=payload, timeout=10, verify=False)
            response.raise_for_status()
            print(f"    ✅ SignalTrack oluşturuldu! (Entry: ${entry_price}, Süre: {target_duration_days} gün)")
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"    ⚠️ SignalTrack Oluşturma Hatası: {e}")
            if e.response is not None:
                print(f"    API Detayı: {e.response.text}")
            return None