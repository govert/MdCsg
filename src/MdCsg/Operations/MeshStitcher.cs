using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Operations;

/// <summary>
/// Stitches selected triangles into a manifold output mesh.
/// </summary>
public static class MeshStitcher
{
    /// <summary>
    /// Creates a HalfEdgeMesh from a list of triangles.
    /// </summary>
    public static HalfEdgeMesh Stitch(IReadOnlyList<Triangle3> triangles, double weldTolerance = 1e-8)
    {
        var builder = new MeshBuilder(weldTolerance);
        return builder.Build(triangles);
    }
}
