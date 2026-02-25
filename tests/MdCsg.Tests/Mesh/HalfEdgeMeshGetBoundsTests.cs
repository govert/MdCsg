using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh.GetBounds — bounds computation, empty mesh, single vertex</summary>
public class HalfEdgeMeshGetBoundsTests
{
    [Fact]
    public void EmptyMesh_ReturnsEmptyBounds()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        Assert.Equal(Aabb.Empty, bounds);
    }

    [Fact]
    public void SingleVertex_PointBounds()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(1, 2, 3));
        var bounds = mesh.GetBounds();
        Assert.Equal(new Vec3(1, 2, 3), bounds.Min);
        Assert.Equal(new Vec3(1, 2, 3), bounds.Max);
    }

    [Fact]
    public void TwoVertices_SpansBoth()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(1, 2, 3));
        mesh.AddVertex(new Vec3(4, 5, 6));
        var bounds = mesh.GetBounds();
        Assert.Equal(new Vec3(1, 2, 3), bounds.Min);
        Assert.Equal(new Vec3(4, 5, 6), bounds.Max);
    }

    [Fact]
    public void Cube_BoundsContainAllVertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        foreach (var v in mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position),
                $"Bounds should contain vertex at {v.Position}");
        }
    }

    [Fact]
    public void Sphere_BoundsContainAllVertices()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bounds = mesh.GetBounds();
        foreach (var v in mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position),
                $"Bounds should contain vertex at {v.Position}");
        }
    }

    [Fact]
    public void Cube_BoundsApproximatelyUnitCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        var size = bounds.Size;
        // Cube should be approximately 1x1x1
        Assert.True(size.X > 0.5 && size.X < 2.0, $"X size {size.X} unexpected");
        Assert.True(size.Y > 0.5 && size.Y < 2.0, $"Y size {size.Y} unexpected");
        Assert.True(size.Z > 0.5 && size.Z < 2.0, $"Z size {size.Z} unexpected");
    }

    [Fact]
    public void Sphere_BoundsSurroundsRadius()
    {
        double r = 1.0;
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, r, 3).Mesh;
        var bounds = mesh.GetBounds();
        // All vertices should be within radius of origin, so bounds should be around [-1, 1]
        Assert.True(bounds.Min.X >= -r - 0.01 && bounds.Max.X <= r + 0.01);
        Assert.True(bounds.Min.Y >= -r - 0.01 && bounds.Max.Y <= r + 0.01);
        Assert.True(bounds.Min.Z >= -r - 0.01 && bounds.Max.Z <= r + 0.01);
    }

    [Fact]
    public void OffsetCube_BoundsReflectOffset()
    {
        var offset = new Vec3(5, 3, 2);
        var mesh = MeshFactory.CreateCube(offset).Mesh;
        var bounds = mesh.GetBounds();
        // Center should be near offset + (0.5, 0.5, 0.5) for a unit cube starting at offset
        Assert.True(bounds.Center.X > 4, $"Center X {bounds.Center.X} should reflect offset");
        Assert.True(bounds.Center.Y > 2, $"Center Y {bounds.Center.Y} should reflect offset");
        Assert.True(bounds.Center.Z > 1, $"Center Z {bounds.Center.Z} should reflect offset");
    }

    [Fact]
    public void NegativeCoordinates_HandledCorrectly()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(-5, -3, -1));
        mesh.AddVertex(new Vec3(5, 3, 1));
        var bounds = mesh.GetBounds();
        Assert.Equal(-5.0, bounds.Min.X, 15);
        Assert.Equal(-3.0, bounds.Min.Y, 15);
        Assert.Equal(-1.0, bounds.Min.Z, 15);
        Assert.Equal(5.0, bounds.Max.X, 15);
        Assert.Equal(3.0, bounds.Max.Y, 15);
        Assert.Equal(1.0, bounds.Max.Z, 15);
    }

    [Fact]
    public void Tetrahedron_HasPositiveSizeBounds()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Size.X > 0);
        Assert.True(bounds.Size.Y > 0);
        Assert.True(bounds.Size.Z > 0);
    }
}
