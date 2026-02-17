import requests
import urllib3
from app.core.config import Config

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

class BackendService:
    def __init__(self):
        self.base_url = Config.BACKEND_URL

    def get_companies(self):
        url = f"{self.base_url}/Companies"
        try:
            response = requests.get(url, verify=False, timeout=5)
            return response.json() if response.status_code == 200 else []
        except:
            return []

    def create_company(self, ticker, name):
        """Şirket yoksa oluşturur"""
        url = f"{self.base_url}/Companies"
        payload = {"name": name, "tickerSymbol": ticker, "sector": 0}
        try:
            response = requests.post(url, json=payload, verify=False, timeout=5)
            if response.status_code in [200, 201]:
                return response.json()
            return None
        except:
            return None

    # --- YENİ: GEÇMİŞ HABERLERİ ÇEKME ---
    def get_recent_logs(self, company_id, limit=5):
        """
        AI'ın hafızası olması için o şirkete ait son haberleri çeker.
        """
        url = f"{self.base_url}/NewsLogs/{company_id}" 
        try:
            response = requests.get(url, verify=False, timeout=5)
            if response.status_code == 200:
                logs = response.json()
                # Tarihe göre sırala
                return logs[:limit]
            return []
        except Exception as e:
            print(f"Geçmiş loglar alınamadı: {e}")
            return []

    def send_log(self, log_data):
        url = f"{self.base_url}/NewsLogs"
        try:
            response = requests.post(url, json=log_data, verify=False, timeout=5)
            return response.status_code in [200, 201]
        except:
            return False