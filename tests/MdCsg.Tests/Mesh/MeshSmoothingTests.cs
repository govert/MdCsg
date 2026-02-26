using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshSmoothingTests
{
    // ── Zero iterations = identity ──────────────────────────────────

    [Fact]
    public void LaplacianSmooth_ZeroIterations_PreservesVertexPositions()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, 0);
        Assert.Equal(cube.Mesh.Vertices.Count, smoothed.Vertices.Count);
        for (int i = 0; i < cube.Mesh.Vertices.Count; i++)
        {
            Assert.True(Vec3.DistanceSquared(cube.Mesh.Vertices[i].Position, smoothed.Vertices[i].Position) < 1e-12);
        }
    }

    // ── Preserves mesh structure ────────────────────────────────────

    [Fact]
    public void LaplacianSmooth_PreservesFaceCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, 3);
        Assert.Equal(cube.Mesh.Faces.Count, smoothed.Faces.Count);
    }

    [Fact]
    public void LaplacianSmooth_PreservesVertexCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, 3);
        Assert.Equal(cube.Mesh.Vertices.Count, smoothed.Vertices.Count);
    }

    // ── Smoothing reduces roughness ─────────────────────────────────

    [Fact]
    public void LaplacianSmooth_Cube_ReducesEdgeLength()
    {
        // A cube has sharp corners — smoothing should pull vertices inward,
        // reducing the max distance from center
        var cube = Primitives.Cube(Vec3.Zero, 2);
        double origMaxDist = MaxDistFromCenter(cube.Mesh, Vec3.Zero);

        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, 5, 0.5);
        double smoothedMaxDist = MaxDistFromCenter(smoothed, Vec3.Zero);

        Assert.True(smoothedMaxDist < origMaxDist,
            $"Smoothing should reduce max distance: {origMaxDist} -> {smoothedMaxDist}");
    }

    // ── Volume roughly preserved ────────────────────────────────────

    [Fact]
    public void LaplacianSmooth_Sphere_VolumeRoughlyPreserved()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 2);
        double origVol = System.Math.Abs(MeshQueries.Volume(sphere.Mesh));

        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, 1, 0.3);
        double smoothedVol = System.Math.Abs(MeshQueries.Volume(smoothed));

        double relChange = System.Math.Abs(origVol - smoothedVol) / origVol;
        Assert.True(relChange < 0.2, $"Volume changed by {relChange:P}");
    }

    // ── Works across primitive types ────────────────────────────────

    public static IEnumerable<object[]> PrimitiveTypes()
    {
        yield return new object[] { "Cube" };
        yield return new object[] { "Sphere" };
        yield return new object[] { "Cylinder" };
    }

    [Theory]
    [MemberData(nameof(PrimitiveTypes))]
    public void LaplacianSmooth_AllPrimitives_NoException(string type)
    {
        var solid = CreatePrimitive(type);
        var smoothed = MeshSmoothing.LaplacianSmooth(solid.Mesh, 2, 0.5);
        Assert.True(smoothed.Faces.Count > 0);
    }

    // ── Lambda = 0 ──────────────────────────────────────────────────

    [Fact]
    public void LaplacianSmooth_LambdaZero_NoChange()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, 5, 0.0);
        for (int i = 0; i < cube.Mesh.Vertices.Count; i++)
        {
            Assert.True(Vec3.DistanceSquared(cube.Mesh.Vertices[i].Position, smoothed.Vertices[i].Position) < 1e-12);
        }
    }

    private static double MaxDistFromCenter(HalfEdgeMesh mesh, Vec3 center)
    {
        double maxDist = 0;
        foreach (var v in mesh.Vertices)
        {
            double d = Vec3.Distance(v.Position, center);
            if (d > maxDist) maxDist = d;
        }
        return maxDist;
    }

    private static Solid CreatePrimitive(string type) => type switch
    {
        "Cube" => Primitives.Cube(Vec3.Zero, 2),
        "Sphere" => Primitives.Sphere(Vec3.Zero, 1, 2),
        "Cylinder" => Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1, 2, 24),
        _ => throw new ArgumentException()
    };
}
