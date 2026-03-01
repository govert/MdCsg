using MdCsg.Patches;

namespace MdCsg.Api;

/// <summary>
/// Normalized boundary authority contract for reconstruction-stage decisions.
/// </summary>
public sealed record ReconstructionBoundaryContract(
    PatchExtractionMode ExtractionMode,
    PatchBoundaryAuthority Authority,
    int BoundaryEdgeCount,
    bool IsEdgeManifold,
    int ConnectedComponentCount);
