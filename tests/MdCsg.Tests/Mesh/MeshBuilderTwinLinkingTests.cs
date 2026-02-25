using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder twin linking — half-edge twin pairs, shared edges, indexed mode</summary>
public class MeshBuilderTwinLinkingTests
{
    [Fact]
    public void SingleTriangle_NoTwins()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        foreach (var he in mesh.HalfEdges)
            Assert.Null(he.Twin);
    }

    [Fact]
    public void TwoTriangles_SharedEdge_HasTwins()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1))
        });
        int twinnedCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinnedCount++;
        Assert.True(twinnedCount >= 2);
    }

    [Fact]
    public void Twin_IsSymmetric()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Same(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void Cube_AllEdges_HaveTwins()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Tetrahedron_AllEdges_HaveTwins()
    {
        var mesh = TestHelpers.MeshFactory.CreateTetrahedron().Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void NextPrevCycle_Length3()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var he = face.Edge;
            Assert.Same(he, he.Next.Next.Next);
            Assert.Same(he, he.Prev.Prev.Prev);
        }
    }

    [Fact]
    public void NextPrev_AreInverse()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void EmptyInput_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(System.Array.Empty<Triangle3>());
        Assert.Empty(mesh.Vertices);
        Assert.Empty(mesh.Faces);
        Assert.Empty(mesh.HalfEdges);
    }

    [Fact]
    public void BuildIndexed_NoWelding_6Vertices()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)
        };
        var indices = new (int I0, int I1, int I2)[] { (0, 1, 2), (3, 4, 5) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void WeldedBuild_MergesNearby()
    {
        var builder = new MeshBuilder(1e-6);
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1.0000001, 0, 0), new Vec3(1, 1, 0), new Vec3(0.0000001, 1, 0))
        });
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Sphere_AllEdges_HaveTwins()
    {
        var mesh = TestHelpers.MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }
}
