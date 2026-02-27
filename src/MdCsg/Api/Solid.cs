using MdCsg.Bvh;
using MdCsg.Classification;
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

    /// <summary>
    /// True if this solid represents the set-theoretic complement of the bounded mesh interior.
    /// </summary>
    public bool IsComplemented => Mesh.IsComplemented;

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

    /// <summary>
    /// Returns the volume of this solid using the divergence theorem.
    /// </summary>
    /// <returns>The unsigned volume.</returns>
    public double Volume() => System.Math.Abs(MeshQueries.Volume(Mesh));

    /// <summary>
    /// Returns the total surface area of this solid.
    /// </summary>
    /// <returns>The surface area.</returns>
    public double SurfaceArea() => MeshQueries.SurfaceArea(Mesh);

    /// <summary>
    /// Tests whether a point is inside this solid using ray casting.
    /// </summary>
    /// <param name="point">The query point.</param>
    /// <returns>True if the point is inside.</returns>
    public bool IsInside(Vec3 point)
    {
        // Use orientation-insensitive ray casting here so complement metadata
        // controls semantic inversion consistently, even if face winding flips.
        bool inside = new BvhPointClassifier(Bvh, useWindingNumber: false).Classify(point) == SolidClassification.Inside;

        // For non-complement solids, fall back to winding-number classification when
        // ray casting says outside. This improves robustness on noisy meshes produced
        // by simplification/smoothing where parity can be unstable.
        if (!inside && !IsComplemented)
        {
            inside = new BvhPointClassifier(Bvh, useWindingNumber: true).Classify(point) == SolidClassification.Inside;
        }

        return IsComplemented ? !inside : inside;
    }

    /// <summary>
    /// Returns a new solid with all vertex positions transformed.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new solid with transformed geometry.</returns>
    public Solid Transform(Transform3 transform)
    {
        var mesh = MeshTransform.Apply(Mesh, transform);
        mesh.IsComplemented = Mesh.IsComplemented;
        return new Solid(mesh);
    }

    /// <summary>
    /// Returns the complement of this solid (reversed winding).
    /// </summary>
    /// <returns>A new solid with reversed face normals.</returns>
    public Solid Complement() => Csg.Complement(this);

    /// <summary>
    /// Union operator: returns A ∪ B.
    /// </summary>
    public static CsgResult operator +(Solid a, Solid b) => Csg.Union(a, b);

    /// <summary>
    /// Difference operator: returns A \ B.
    /// </summary>
    public static CsgResult operator -(Solid a, Solid b) => Csg.Difference(a, b);

    /// <summary>
    /// Intersection operator: returns A ∩ B.
    /// </summary>
    public static CsgResult operator ^(Solid a, Solid b) => Csg.Intersect(a, b);
}
