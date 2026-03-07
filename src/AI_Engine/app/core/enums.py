from enum import Enum

class SentimentType(str, Enum):
    POSITIVE = "Positive"
    NEGATIVE = "Negative"
    NEUTRAL = "Neutral"

class DirectionType(str, Enum):
    UP = "Up"
    DOWN = "Down"
    UNCERTAIN = "Uncertain"

class TimeHorizonType(str, Enum):
    INTRADAY = "Intraday"
    SHORT_TERM = "ShortTerm"
    LONG_TERM = "LongTerm"

class NewsEventType(str, Enum):
    EARNINGS = "Earnings"
    FDA_APPROVAL = "FDAApproval"
    UPGRADE = "Upgrade"
    DOWNGRADE = "Downgrade"
    HYPE = "Hype"
    PARTNERSHIP = "Partnership"
    PRODUCT_LAUNCH = "ProductLaunch"
    REGULATORY = "Regulatory"
    MARKET_MOVE = "MarketMove"
    OTHER = "Other"
    GENERIC_NEWS = "Generic News"

class SectorType(int, Enum):
    OTHER = 0
    HEALTHCARE = 1
    TECHNOLOGY = 2
    ENERGY = 3
    FINANCE = 4
    AUTOMOTIVE = 5

# Yardımcı fonksiyonlar
def normalize_sentiment(value: str) -> str:
    """Sentiment değerini enum formatına çevirir"""
    if not value:
        return SentimentType.NEUTRAL.value
    
    value_lower = value.lower()
    if value_lower in ["positive", "pos", "bullish"]:
        return SentimentType.POSITIVE.value
    elif value_lower in ["negative", "neg", "bearish"]:
        return SentimentType.NEGATIVE.value
    else:
        return SentimentType.NEUTRAL.value

def normalize_direction(value: str) -> str:
    """Direction değerini enum formatına çevirir"""
    if not value:
        return DirectionType.UNCERTAIN.value
    
    value_lower = value.lower()
    if value_lower in ["up", "bullish", "long"]:
        return DirectionType.UP.value
    elif value_lower in ["down", "bearish", "short"]:
        return DirectionType.DOWN.value
    else:
        return DirectionType.UNCERTAIN.value

def normalize_time_horizon(value: str) -> str:
    """Time horizon değerini enum formatına çevirir"""
    if not value:
        return TimeHorizonType.SHORT_TERM.value
    
    value_lower = value.lower().replace(" ", "_").replace("-", "_")
    if value_lower in ["intraday", "day", "1d"]:
        return TimeHorizonType.INTRADAY.value
    elif value_lower in ["shortterm", "short_term", "short", "1w", "1m"]:
        return TimeHorizonType.SHORT_TERM.value
    elif value_lower in ["longterm", "long_term", "long", "3m", "6m", "1y"]:
        return TimeHorizonType.LONG_TERM.value
    else:
        return TimeHorizonType.SHORT_TERM.value

def normalize_event_type(value: str) -> str:
    """Event type değerini enum formatına çevirir"""
    if not value:
        return NewsEventType.OTHER.value
    
    # Önce tam eşleşmeleri kontrol et
    for event_type in NewsEventType:
        if value.lower() == event_type.value.lower():
            return event_type.value
    
    # Kısmi eşleşmeleri kontrol et
    value_lower = value.lower()
    if "earning" in value_lower:
        return NewsEventType.EARNINGS.value
    elif "fda" in value_lower or "approval" in value_lower:
        return NewsEventType.FDA_APPROVAL.value
    elif "upgrade" in value_lower:
        return NewsEventType.UPGRADE.value
    elif "downgrade" in value_lower:
        return NewsEventType.DOWNGRADE.value
    elif "hype" in value_lower or "viral" in value_lower:
        return NewsEventType.HYPE.value
    elif "partnership" in value_lower or "partner" in value_lower:
        return NewsEventType.PARTNERSHIP.value
    elif "product" in value_lower or "launch" in value_lower:
        return NewsEventType.PRODUCT_LAUNCH.value
    elif "regulatory" in value_lower or "regulation" in value_lower:
        return NewsEventType.REGULATORY.value
    elif "market" in value_lower:
        return NewsEventType.MARKET_MOVE.value
    else:
        return NewsEventType.OTHER.value