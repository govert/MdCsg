namespace MdCsg.Patches;

/// <summary>
/// Indicates which source was used as authoritative patch-boundary ownership.
/// </summary>
public enum PatchBoundaryAuthority
{
    /// <summary>
    /// Patch boundaries are driven by intersection-edge flags on cut sub-triangles.
    /// </summary>
    IntersectionFlags = 0,

    /// <summary>
    /// Patch boundaries are driven by arrangement-owned segment ownership.
    /// </summary>
    Arrangement = 1
}
