using MdCsg.Predicates;

namespace MdCsg.Robust.Kernel.Predicates;

public readonly record struct CertifiedPredicateResult(
    PredicateSign Sign,
    PredicatePrecisionTier Tier,
    double ApproximateDeterminant,
    double ErrorBound)
{
    public bool IsExact => Tier == PredicatePrecisionTier.Exact;

    public bool WasEscalated => Tier != PredicatePrecisionTier.Double;
}
