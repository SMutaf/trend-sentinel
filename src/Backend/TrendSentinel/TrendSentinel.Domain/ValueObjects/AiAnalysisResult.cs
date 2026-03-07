using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Domain.ValueObjects
{
    public class AiAnalysisResult
    {
        public bool IsTrendTriggered { get; private set; }
        public string TrendSummary { get; private set; } = string.Empty;
        public SentimentType Sentiment { get; private set; }
        public int ConfidenceScore { get; private set; }

        public bool IsHighConfidence(int threshold = 80) => ConfidenceScore >= threshold;
        public bool IsPositiveSentiment() => Sentiment == SentimentType.Positive;
        public bool IsNegativeSentiment() => Sentiment == SentimentType.Negative;

        private AiAnalysisResult() { }

        public AiAnalysisResult(bool isTrendTriggered, string trendSummary, SentimentType sentiment, int confidenceScore)
        {
            IsTrendTriggered = isTrendTriggered;
            TrendSummary = trendSummary;
            Sentiment = sentiment;
            ConfidenceScore = confidenceScore;
        }

        public static AiAnalysisResult Create(bool isTrendTriggered, string trendSummary, SentimentType sentiment, int confidenceScore)
        {
            return new AiAnalysisResult(isTrendTriggered, trendSummary, sentiment, confidenceScore);
        }
    }
}