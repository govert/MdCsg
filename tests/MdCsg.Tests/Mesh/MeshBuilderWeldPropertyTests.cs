using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — welding tolerance, indexed build, twin verification</summary>
public class MeshBuilderWeldPropertyTests
{
    [Fact]
    public void VertexWelding_MergesCloseVertices()
    {
        var builder = new MeshBuilder(1e-6);
        var triangles = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            // Second triangle shares edge but vertices are slightly offset
            new Triangle3(new Vec3(1.0000001, 0, 0), new Vec3(0.0000001, 0, 0), new Vec3(0, 0, 1)),
        };
        var mesh = builder.Build(triangles);

        // With welding, shared vertices should be merged
        Assert.True(mesh.Vertices.Count <= 5,
            $"Welding should merge close vertices, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void NoWelding_DistantVerticesStaySeparate()
    {
        var builder = new MeshBuilder(1e-10);
        var triangles = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0)),
        };
        var mesh = builder.Build(triangles);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void IndexedBuild_VertexCountMatches()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new[] { (0, 1, 2), (1, 3, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(6, mesh.HalfEdges.Count);
    }

    [Fact]
    public void IndexedBuild_SharedEdge_HasTwin()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new[] { (0, 1, 2), (1, 3, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.True(twinCount >= 2, $"Shared edge should have twins, got {twinCount}");
    }

    [Fact]
    public void SingleTriangle_NoTwins()
    {
        var builder = new MeshBuilder();
        var triangles = new[] { new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var mesh = builder.Build(triangles);

        foreach (var he in mesh.HalfEdges)
            Assert.Null(he.Twin);
    }

    [Fact]
    public void Cube_AllTwinsLinked()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Cube_TwinSymmetry()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
            Assert.Same(he, he.Twin!.Twin);
        }
    }

    [Fact]
    public void Cube_TwinPointsOpposite()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null) continue;
            Assert.Equal(he.Origin.Id, he.Twin.Target.Id);
            Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
        }
    }

    [Fact]
    public void Build_FaceCyclesAreLength3()
    {
        var builder = new MeshBuilder();
        var triangles = new[]
        {
            new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        };
        var mesh = builder.Build(triangles);

        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do { count++; current = current.Next; } while (current != start && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Build_MultipleTriangles_CorrectFaceCount()
    {
        var builder = new MeshBuilder();
        var triangles = new[]
        {
            new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
        };
        var mesh = builder.Build(triangles);
        Assert.Equal(3, mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_AllTwinsLinked()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }
}
