import google.generativeai as genai
import json
from app.core.config import Config

class GeminiService:
    def __init__(self):
        if not Config.GEMINI_API_KEY:
            raise ValueError("Gemini API Key eksik!")
        genai.configure(api_key=Config.GEMINI_API_KEY)
        self.model = genai.GenerativeModel('gemma-3-27b-it')

    def analyze_news_with_history(self, current_news_title, current_news_summary, history_logs):
        """
        Geçmiş haberleri de dikkate alarak analiz yapar.
        history_logs: Veritabanından gelen eski haberlerin listesi.
        """
        
        # Geçmiş haberleri metne dök
        history_text = "ÖNCEKİ HABERLER (Eskiden yeniye):\n"
        if history_logs:
            for log in history_logs:
                history_text += f"- {log.get('createdDate')}: {log.get('title')} (Analiz: {log.get('trendSummary')})\n"
        else:
            history_text += "Bu şirket için kayıtlı geçmiş haber yok.\n"

        prompt = f"""
        Sen uzman bir finansal stratejistsin. Bir şirketin haber akışını takip ediyorsun.
        
        {history_text}
        
        ---
        YENİ GELEN HABER:
        Başlık: {current_news_title}
        Özet: {current_news_summary}
        ---

        GÖREVİN:
        Yeni haberi, geçmiş haberlerle BİRLEŞTİREREK analiz et.
        Örneğin: Geçmişte "FDA Başvurusu" varsa ve şimdi "FDA Onayı" geldiyse bu devasa bir trenddir.
        
        YANIT FORMATI (JSON):
        1. 'shouldSave': Bu haber şirket için "önemli bir gelişme" veya "hazırlık" aşaması mı? (Çöp haberleri kaydetme).
        2. 'isTrendTriggered': (TRUE/FALSE) Geçmiş ve şimdiki haber birleşince hissede BÜYÜK bir patlama yaratır mı?
        3. 'trendSummary': Neden böyle düşündüğünü açıklayan Türkçe cümle.
        4. 'sentimentLabel': Positive, Negative, Neutral.

        SADECE JSON DÖN:
        {{
            "shouldSave": true,
            "isTrendTriggered": true,
            "trendSummary": "Geçmişteki başvuru haberi, bugünkü onay ile tamamlandı ve güçlü alım sinyali oluştu.",
            "sentimentLabel": "Positive"
        }}
        """
        
        try:
            response = self.model.generate_content(prompt)
            cleaned_text = response.text.replace('```json', '').replace('```', '').strip()
            return json.loads(cleaned_text)
        except Exception as e:
            print(f"AI Context Hatası: {e}")
            return {"shouldSave": True, "isTrendTriggered": False, "trendSummary": "Hata", "sentimentLabel": "Neutral"}