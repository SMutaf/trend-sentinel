import os
from dotenv import load_dotenv

# .env dosyasını yükle
load_dotenv()

class Config:
    # API Anahtarını al, yoksa hata ver
    GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
    
    # Backend adresini al, yoksa varsayılanı kullan
    BACKEND_URL = os.getenv("BACKEND_URL", "https://localhost:7001/api")
    
    # Dakika ayarını al (Sayıya çevir)
    CHECK_INTERVAL_MINUTES = int(os.getenv("CHECK_INTERVAL_MINUTES", 15))

    if not GEMINI_API_KEY:
        print("UYARI: GEMINI_API_KEY .env dosyasında bulunamadı!")