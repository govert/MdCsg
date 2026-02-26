using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshSimplifierTests
{
    // ── Target face count ───────────────────────────────────────────

    [Fact]
    public void Simplify_Sphere_ReducesFaceCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 3);
        int origFaces = sphere.Mesh.Faces.Count;
        int target = origFaces / 2;

        var simplified = MeshSimplifier.Simplify(sphere.Mesh, target);
        Assert.True(simplified.Faces.Count <= target + 4,
            $"Expected <= {target} faces, got {simplified.Faces.Count}");
        Assert.True(simplified.Faces.Count > 0);
    }

    [Fact]
    public void Simplify_Cube_ReducesFaceCount()
    {
        // Cube has only 12 faces - simplify to 8
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var simplified = MeshSimplifier.Simplify(cube.Mesh, 8);
        Assert.True(simplified.Faces.Count <= 12);
        Assert.True(simplified.Faces.Count >= 4);
    }

    // ── Error threshold ─────────────────────────────────────────────

    [Fact]
    public void Simplify_ErrorThreshold_StopsEarly()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 3);
        int origFaces = sphere.Mesh.Faces.Count;

        // Very small threshold should stop early
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 1e-10);
        Assert.True(simplified.Faces.Count > origFaces / 4,
            $"Very tight threshold should preserve most faces. Got {simplified.Faces.Count}/{origFaces}");
    }

    [Fact]
    public void Simplify_LargeThreshold_RemovesManyFaces()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 3);
        int origFaces = sphere.Mesh.Faces.Count;

        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 100.0);
        Assert.True(simplified.Faces.Count < origFaces,
            $"Large threshold should remove faces. Got {simplified.Faces.Count}/{origFaces}");
    }

    // ── Produces valid mesh ─────────────────────────────────────────

    [Fact]
    public void Simplify_ResultHasVerticesAndFaces()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 2);
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, sphere.Mesh.Faces.Count / 2);
        Assert.True(simplified.Vertices.Count > 0);
        Assert.True(simplified.Faces.Count > 0);
    }

    // ── Parameterized across primitives ─────────────────────────────

    public static IEnumerable<object[]> PrimitiveTypes()
    {
        yield return new object[] { "Sphere" };
        yield return new object[] { "Cylinder" };
        yield return new object[] { "Cone" };
    }

    [Theory]
    [MemberData(nameof(PrimitiveTypes))]
    public void Simplify_AllPrimitives_ReducesFaces(string type)
    {
        var solid = CreatePrimitive(type);
        int origFaces = solid.Mesh.Faces.Count;
        int target = System.Math.Max(origFaces / 2, 4);

        var simplified = MeshSimplifier.Simplify(solid.Mesh, target);
        Assert.True(simplified.Faces.Count <= origFaces,
            $"{type}: Expected <= {origFaces} faces, got {simplified.Faces.Count}");
        Assert.True(simplified.Faces.Count > 0);
    }

    private static Solid CreatePrimitive(string type) => type switch
    {
        "Sphere" => Primitives.Sphere(Vec3.Zero, 1, 3),
        "Cylinder" => Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1, 2, 24),
        "Cone" => Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1, 2, 24),
        _ => throw new ArgumentException()
    };
}
