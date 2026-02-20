import google.generativeai as genai
import json
from app.core.config import Config

class GeminiService:
    def __init__(self):
        if not Config.GEMINI_API_KEY:
            raise ValueError("Gemini API Key eksik!")
        genai.configure(api_key=Config.GEMINI_API_KEY)
        self.model = genai.GenerativeModel('gemma-3-27b-it')

    def analyze_news_with_history(self, current_news_title, current_news_summary, current_news_pub_date, history_logs):
        """
        Geçmiş haberleri de dikkate alarak analiz yapar.
        history_logs: Veritabanından gelen eski haberlerin listesi.
        """
        
        history_text = "ÖNCEKİ HABERLER (Eskiden yeniye):\n"
        if history_logs:
            for log in history_logs:
                history_text += f"- {log.get('createdDate')}: {log.get('title')} (Analiz: {log.get('trendSummary', '')})\n"
        else:
            history_text += "Bu şirket için kayıtlı geçmiş haber yok.\n"

        prompt = f"""
        Sen Wall Street'in en acımasız ve şüpheci algoritmik trade (quant) analistisin.

        {history_text}

        ---
        YENİ GELEN HABER:
        Yayınlanma Zamanı: {current_news_pub_date}
        Başlık: {current_news_title}
        Özet: {current_news_summary}
        ---

        GÖREVİN: Haberi fiyat hareketi perspektifinden acımasızca değerlendir.

        DEĞERLENDİRME KRİTERLERİ:
        1. Bu haber gerçek bir fiyat katalizörü mü?
        2. Etki kısa vadeli mi uzun vadeli mi?
        3. Haber zaten fiyatlanmış olabilir mi?
        4. Bu haber "momentum continuation" mı yoksa "mean reversion risk" mi?

        YANIT FORMATI (Aşağıdaki anahtarlara sahip SADECE geçerli bir JSON dön):
        {{
            "shouldSave": true,
            "eventType": "Earnings / FDA Approval / Upgrade / Hype / Generic News vb.",
            "impactStrength": 4,
            "expectedDirection": "Up",
            "timeHorizon": "ShortTerm",
            "overextendedRisk": false,
            "confidenceScore": 85,
            "sectorId": 2,
            "trendSummary": "Haberin fiyat etkisini açıklayan net ve kısa yorum."
        }}
        """
        
        try:
            response = self.model.generate_content(prompt)
            cleaned_text = response.text.replace('```json', '').replace('```', '').strip()
            return json.loads(cleaned_text)
        except Exception as e:
            print(f"AI Context Hatası: {e}")
            # C#'ı çökertmemek için güvenli varsayılan değerler dönüyoruz
            return {
                "shouldSave": False, 
                "eventType": "Error", 
                "impactStrength": 1, 
                "expectedDirection": "Uncertain", 
                "timeHorizon": "Intraday", 
                "overextendedRisk": False, 
                "confidenceScore": 0, 
                "sectorId": 0, 
                "trendSummary": "API Hatası"
            }