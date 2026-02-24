namespace MdCsg.Api;

/// <summary>
/// Configuration options for CSG operations.
/// </summary>
public class CsgOptions
{
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
    public bool UseWindingNumber { get; set; }

    /// <summary>
    /// Vertex welding tolerance for the output mesh.
    /// </summary>
    public double WeldTolerance { get; set; } = 1e-8;
}
