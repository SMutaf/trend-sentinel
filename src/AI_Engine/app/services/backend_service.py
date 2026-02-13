import requests
import urllib3
from app.core.config import Config

# Localhost'ta çalıştığı için SSL sertifika uyarılarını (HTTPS) gizliyoruz.
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

class BackendService:
    def __init__(self):
        self.base_url = Config.BACKEND_URL

    def get_companies(self):
        """
        API'den takip edilecek şirketlerin listesini çeker.
        """
        url = f"{self.base_url}/Companies"
        try:
            # verify=False: Localhost sertifikasına güvenmesi için
            response = requests.get(url, verify=False, timeout=5)
            
            if response.status_code == 200:
                return response.json()
            else:
                print(f"API Hatası (Şirketler alınamadı): {response.status_code}")
                return []
        except Exception as e:
            print(f"API Bağlantı Hatası: {e}")
            return []

    def send_log(self, log_data):
        """
        Analiz edilen haberi ve sonucu API'ye gönderir.
        """
        url = f"{self.base_url}/NewsLogs"
        try:
            # verify=False: Localhost sertifikasına güvenmesi için
            response = requests.post(url, json=log_data, verify=False, timeout=5)
            
            if response.status_code in [200, 201]:
                print("Başarılı: Analiz Backend'e ve Veritabanına kaydedildi!")
                return True
            else:
                print(f"Kayıt Başarısız: {response.status_code} - {response.text}")
                return False
        except Exception as e:
            print(f"API Gönderim Hatası: {e}")
            return False