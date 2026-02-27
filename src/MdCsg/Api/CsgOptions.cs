using MdCsg.Classification;
using MdCsg.Cutting;

namespace MdCsg.Api;

/// <summary>
/// Configuration options for CSG operations.
/// </summary>
public class CsgOptions
{
    /// <summary>
    /// Optional strategy for patch classification. When null, uses the default CPU implementation.
    /// Set this to a GPU-accelerated strategy for large meshes.
    /// </summary>
    public IPatchClassificationStrategy? ClassificationStrategy { get; set; }

    /// <summary>
    /// The snap grid size for intersection point rounding.
    /// Smaller values give more precise results but may cause topological issues
    /// with very close intersection points.
    /// </summary>
    public double GridSize { get; set; } = 1e-8;

    /// <summary>
    /// If true, use the generalized winding number classifier instead of ray casting.
    /// Winding numbers are more robust for non-manifold or open meshes,
    /// but slightly slower.
    /// </summary>
    public bool UseWindingNumber { get; set; } = false;

    /// <summary>
    /// Vertex welding tolerance for the output mesh.
    /// </summary>
    public double WeldTolerance { get; set; } = 1e-8;

    /// <summary>
    /// Whether to use parallel execution for independent pipeline stages.
    /// Defaults to true. Disable for debugging or single-threaded environments.
    /// </summary>
    public bool Parallel { get; set; } = true;

    /// <summary>
    /// Optional constrained triangulation kernel used by face cutting.
    /// When null, the default triangulator is used.
    /// </summary>
    public ConstrainedTriangulationKernel? TriangulationKernel { get; set; }
}
