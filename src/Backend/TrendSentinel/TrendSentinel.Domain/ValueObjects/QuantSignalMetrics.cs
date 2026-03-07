using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Domain.ValueObjects
{
    public class QuantSignalMetrics
    {
        public NewsEventType EventType { get; private set; }
        public int ImpactStrength { get; private set; }
        public DirectionType ExpectedDirection { get; private set; }
        public TimeHorizonType TimeHorizon { get; private set; }
        public bool OverextendedRisk { get; private set; }

        public bool IsStrongSignal() => ImpactStrength >= 4 && !OverextendedRisk;
        public bool IsShortTerm() => TimeHorizon == TimeHorizonType.Intraday || TimeHorizon == TimeHorizonType.ShortTerm;

        private QuantSignalMetrics() { }

        public QuantSignalMetrics(NewsEventType eventType, int impactStrength,
            DirectionType direction, TimeHorizonType horizon, bool overextendedRisk)
        {
            EventType = eventType;
            ImpactStrength = impactStrength;
            ExpectedDirection = direction;
            TimeHorizon = horizon;
            OverextendedRisk = overextendedRisk;
        }

        public static QuantSignalMetrics Create(NewsEventType eventType, int impactStrength,
            DirectionType direction, TimeHorizonType horizon, bool overextendedRisk)
        {
            return new QuantSignalMetrics(eventType, impactStrength, direction, horizon, overextendedRisk);
        }
    }
}