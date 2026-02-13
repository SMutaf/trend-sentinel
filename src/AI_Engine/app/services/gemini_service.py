import google.generativeai as genai
import json
from app.core.config import Config

class GeminiService:
    def __init__(self):
        # 1. API Anahtarını Yapılandır
        if not Config.GEMINI_API_KEY:
            raise ValueError("Gemini API Key bulunamadı! .env dosyasını kontrol et.")
            
        genai.configure(api_key=Config.GEMINI_API_KEY)
        
        # 2. LLM model seçimi
        self.model = genai.GenerativeModel('gemini-2.5-flash')

    def analyze_news(self, news_title, news_summary):
        """
        Haberi Gemini'ye gönderir ve JSON formatında analiz sonucunu döner.
        """
        # Yapay Zekaya Gönderilecek Emir (Prompt)
        prompt = f"""
        Sen uzman bir borsa analistisin. Aşağıdaki finans haberini analiz et.

        Haber Başlığı: {news_title}
        Haber Özeti: {news_summary}

        Kurallar:
        1. 'isTrendTriggered': Eğer bu haber hisse fiyatını ANLIK ve GÜÇLÜ etkileyecek bir trend başlatıyorsa 'true', yoksa 'false'.
        2. 'trendSummary': Trendin nedenini açıklayan tek cümlelik Türkçe özet.
        3. 'sentimentLabel': Haberin duygusu (Positive, Negative, Neutral).

        Yanıtı SADECE aşağıdaki saf JSON formatında ver (Markdown veya ```json kullanma):
        {{
            "isTrendTriggered": true,
            "trendSummary": "Yatırımcılar için risk oluşturuyor...",
            "sentimentLabel": "Negative"
        }}
        """
        
        try:
            # AI'dan yanıt al
            response = self.model.generate_content(prompt)
            
            # Yanıtı temizle (Bazen ```json etiketiyle gelir, onu siliyoruz)
            cleaned_text = response.text.replace('```json', '').replace('```', '').strip()
            
            # String'i JSON objesine çevir
            return json.loads(cleaned_text)
            
        except Exception as e:
            print(f"🧠 AI Analiz Hatası: {e}")
            # Hata olursa program patlamasın, nötr sonuç dönsün
            return {
                "isTrendTriggered": False, 
                "trendSummary": "AI Analizi Yapılamadı", 
                "sentimentLabel": "Neutral"
            }