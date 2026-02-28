using MdCsg.Mesh;

namespace MdCsg.Api;

/// <summary>
/// Result of a CSG operation.
/// </summary>
public class CsgResult
{
    /// <summary>The resulting mesh.</summary>
    public required HalfEdgeMesh Mesh { get; init; }

    /// <summary>The number of patches in mesh A.</summary>
    public int PatchCountA { get; init; }

    /// <summary>The number of patches in mesh B.</summary>
    public int PatchCountB { get; init; }

    /// <summary>The number of degenerate patches (low confidence classification).</summary>
    public int DegenerateCount { get; init; }

    /// <summary>The number of intersection segments found.</summary>
    public int IntersectionSegmentCount { get; init; }

    /// <summary>
    /// The patch extraction mode used by the operation when available.
    /// </summary>
    public PatchExtractionMode? SelectedPatchExtractionMode { get; init; }

    /// <summary>
    /// Boundary edge count of the stitched assembly candidate selected before final post-processing.
    /// </summary>
    public int? SelectedPatchExtractionBoundaryEdgeCount { get; init; }

    /// <summary>
    /// Whether the stitched assembly candidate selected before final post-processing is edge-manifold.
    /// </summary>
    public bool? SelectedPatchExtractionIsEdgeManifold { get; init; }

    /// <summary>
    /// Connected component count of the stitched assembly candidate selected before final post-processing.
    /// </summary>
    public int? SelectedPatchExtractionConnectedComponentCount { get; init; }

    /// <summary>Total number of faces in the result.</summary>
    public int FaceCount => Mesh.Faces.Count;

    /// <summary>Total number of vertices in the result.</summary>
    public int VertexCount => Mesh.Vertices.Count;
}
