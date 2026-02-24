namespace MdCsg.Operations;

/// <summary>
/// The type of CSG boolean operation.
/// </summary>
public enum CsgOperation
{
    /// <summary>A ∪ B: combine both solids.</summary>
    Union,

    /// <summary>A ∩ B: keep only the overlapping volume.</summary>
    Intersection,

    /// <summary>A \ B: subtract B from A.</summary>
    Difference
}
