namespace MdCsg.Robust.Kernel.Predicates;

public sealed class PredicateTelemetryCounter
{
    public int DoubleCount { get; private set; }

    public int ExpansionCount { get; private set; }

    public int ExactCount { get; private set; }

    public int EscalationCount => ExpansionCount + ExactCount;

    public void Add(PredicatePrecisionTier tier)
    {
        switch (tier)
        {
            case PredicatePrecisionTier.Double:
                DoubleCount++;
                break;
            case PredicatePrecisionTier.Expansion:
                ExpansionCount++;
                break;
            case PredicatePrecisionTier.Exact:
                ExactCount++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tier), tier, null);
        }
    }

    public void Merge(PredicateTelemetryCounter other)
    {
        DoubleCount += other.DoubleCount;
        ExpansionCount += other.ExpansionCount;
        ExactCount += other.ExactCount;
    }
}
