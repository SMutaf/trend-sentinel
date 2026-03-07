import google.generativeai as genai
import json
from app.core.config import Config
from app.core.enums import (
    SentimentType, 
    DirectionType, 
    TimeHorizonType, 
    NewsEventType,
    SectorType,
    normalize_direction,
    normalize_time_horizon,
    normalize_event_type
)

class LLMEngine:
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

SEKTÖR ID KARŞILIĞI (sectorId için SADECE bu değerleri kullan):
0 = Other (Diğer / Bilinmiyor)
1 = Healthcare (Sağlık, İlaç, Biyoteknoloji)
2 = Technology (Yazılım, Yarı İletken, AI, SaaS)
3 = Energy (Petrol, Gaz, Yenilenebilir Enerji)
4 = Finance (Bankacılık, Kripto, Borsa, Fintech)
5 = Automotive (Otomotiv, Elektrikli Araç)

OLASI EVENT TİPLERİ (eventType için bunlardan birini seç):
- Earnings (Kazanç Açıklaması)
- FDAApproval (FDA Onayı)
- Upgrade (Not Artırımı)
- Downgrade (Not Düşürme)
- Hype (Viral/Sosyal Medya)
- Partnership (Ortaklık)
- ProductLaunch (Ürün Lansmanı)
- Regulatory (Düzenleyici Karar)
- MarketMove (Piyasa Hareketi)
- Other (Diğer)

ZAMAN UFUKLARI (timeHorizon için bunlardan birini seç):
- Intraday (Gün içi)
- ShortTerm (Kısa vadeli: 1 gün - 1 hafta)
- LongTerm (Uzun vadeli: 1 ay+)

BEKLENEN YÖNLER (expectedDirection için bunlardan birini seç):
- Up (Yukarı)
- Down (Aşağı)
- Uncertain (Belirsiz)

YANIT FORMATI (Aşağıdaki anahtarlara sahip SADECE geçerli bir JSON dön):
{{
    "shouldSave": true,
    "eventType": "Earnings",
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
            result = json.loads(cleaned_text)
            
            # Enum değerlerini normalize et (güvenlik için)
            result['expectedDirection'] = normalize_direction(result.get('expectedDirection', 'Uncertain'))
            result['timeHorizon'] = normalize_time_horizon(result.get('timeHorizon', 'ShortTerm'))
            result['eventType'] = normalize_event_type(result.get('eventType', 'Other'))
            
            # Sector ID'yi validasyon
            sector_id = result.get('sectorId', 0)
            if sector_id not in [s.value for s in SectorType]:
                result['sectorId'] = SectorType.OTHER.value
            
            # Impact strength validasyonu (1-5 arası)
            impact = result.get('impactStrength', 1)
            result['impactStrength'] = max(1, min(5, impact))
            
            # Confidence score validasyonu (0-100 arası)
            confidence = result.get('confidenceScore', 0)
            result['confidenceScore'] = max(0, min(100, confidence))
            
            return result
            
        except Exception as e:
            print(f"AI Context Hatası: {e}")
            # C#'ı çökertmemek için güvenli varsayılan değerler dönüyoruz
            return {
                "shouldSave": False,
                "eventType": NewsEventType.OTHER.value,
                "impactStrength": 1,
                "expectedDirection": DirectionType.UNCERTAIN.value,
                "timeHorizon": TimeHorizonType.INTRADAY.value,
                "overextendedRisk": False,
                "confidenceScore": 0,
                "sectorId": SectorType.OTHER.value,
                "trendSummary": "API Hatası"
            }