using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Api;

/// <summary>
/// A solid object represented by a closed triangle mesh.
/// Wraps a <see cref="HalfEdgeMesh"/> with a pre-built BVH for spatial queries.
/// </summary>
public class Solid
{
    /// <summary>The underlying half-edge mesh.</summary>
    public HalfEdgeMesh Mesh { get; }

    /// <summary>The BVH built from the mesh faces.</summary>
    public BvhTree Bvh { get; }

    /// <summary>The axis-aligned bounding box of the solid.</summary>
    public Aabb Bounds => Mesh.GetBounds();

    /// <summary>
    /// Creates a solid from a pre-built half-edge mesh.
    /// </summary>
    public Solid(HalfEdgeMesh mesh)
    {
        Mesh = mesh;
        Bvh = BvhTree.Build(mesh);
    }

    /// <summary>
    /// Creates a solid from triangle soup.
    /// </summary>
    public static Solid FromTriangles(IReadOnlyList<Triangle3> triangles, double weldTolerance = 1e-10)
    {
        var builder = new MeshBuilder(weldTolerance);
        var mesh = builder.Build(triangles);
        return new Solid(mesh);
    }

    /// <summary>
    /// Creates a solid from vertex positions and triangle indices.
    /// </summary>
    public static Solid FromIndexed(IReadOnlyList<Vec3> positions, IReadOnlyList<(int I0, int I1, int I2)> triangles, double weldTolerance = 1e-10)
    {
        var builder = new MeshBuilder(weldTolerance);
        var mesh = builder.Build(positions, triangles);
        return new Solid(mesh);
    }
}
