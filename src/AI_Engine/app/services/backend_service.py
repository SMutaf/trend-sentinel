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
            if response.status_code == 200:
                return response.json()
            else:
                print(f"C# API Okuma Hatası (Kod: {response.status_code})")
                return []
        except Exception as e:
            print(f"C# API'ye Ulaşılamıyor (Visual Studio Kapalı Olabilir!): {e}")
            return []

    def create_company(self, ticker, name, sector_id=0):
        url = f"{self.base_url}/Companies"
        payload = {"name": name, "tickerSymbol": ticker, "sector": sector_id}
        try:
            response = requests.post(url, json=payload, verify=False, timeout=5)
            if response.status_code in [200, 201]:
                return response.json()
            else:
                print(f"Şirket C#'a Kaydedilemedi (Kod: {response.status_code}) - {response.text}")
                return None
        except Exception as e:
            print(f"Şirket Kayıt Hatası (Visual Studio Kapalı Olabilir!): {e}")
            return None

    def get_recent_logs(self, company_id, limit=5):
        url = f"{self.base_url}/NewsLogs/{company_id}" 
        try:
            response = requests.get(url, verify=False, timeout=5)
            if response.status_code == 200:
                logs = response.json()
                return logs[:limit]
            return []
        except Exception as e:
            print(f"Geçmiş loglar alınamadı: {e}")
            return []

    def send_log(self, log_data):
        url = f"{self.base_url}/NewsLogs"
        try:
            response = requests.post(url, json=log_data, verify=False, timeout=5)
            if response.status_code not in [200, 201]:
                 print(f"Haber Kaydedilemedi (Kod: {response.status_code})")
            return response.status_code in [200, 201]
        except Exception as e:
            print(f"Haber Kayıt Hatası: {e}")
            return False