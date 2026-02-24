namespace MdCsg.Predicates;

/// <summary>
/// Sign result from a geometric predicate.
/// </summary>
public enum PredicateSign
{
    /// <summary>The determinant is negative.</summary>
    Negative = -1,

    /// <summary>The determinant is exactly zero (degenerate configuration).</summary>
    Zero = 0,

    /// <summary>The determinant is positive.</summary>
    Positive = 1
}
