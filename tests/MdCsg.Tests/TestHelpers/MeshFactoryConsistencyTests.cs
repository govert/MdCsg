using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.TestHelpers;

/// <summary>Phase 6: MeshFactory consistency tests — verifying test helper meshes are well-formed</summary>
public class MeshFactoryConsistencyTests
{
    [Fact]
    public void CreateCube_Default_UnitCube()
    {
        var solid = MeshFactory.CreateCube();
        Assert.Equal(12, solid.Mesh.Faces.Count); // 6 faces * 2 triangles
        Assert.Equal(8, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void CreateCube_Offset_SameFaceCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        Assert.Equal(a.Mesh.Faces.Count, b.Mesh.Faces.Count);
        Assert.Equal(a.Mesh.Vertices.Count, b.Mesh.Vertices.Count);
    }

    [Fact]
    public void CreateCube_CustomSize_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(size: 1);
        var b = MeshFactory.CreateCube(size: 5);
        Assert.Equal(a.Mesh.Faces.Count, b.Mesh.Faces.Count);
    }

    [Fact]
    public void CreateCube_AllFacesTriangles()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
            Assert.Equal(3, face.GetVertices().Count);
    }

    [Fact]
    public void CreateTetrahedron_4Faces_4Vertices()
    {
        var solid = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void CreateTetrahedron_Offset_SameTopology()
    {
        var a = MeshFactory.CreateTetrahedron();
        var b = MeshFactory.CreateTetrahedron(new Vec3(10, 10, 10));
        Assert.Equal(a.Mesh.Faces.Count, b.Mesh.Faces.Count);
        Assert.Equal(a.Mesh.Vertices.Count, b.Mesh.Vertices.Count);
    }

    [Fact]
    public void CreateSphere_Subdivisions_IncreaseFaceCount()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1, 1);
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var s3 = MeshFactory.CreateSphere(Vec3.Zero, 1, 3);
        Assert.True(s2.Mesh.Faces.Count > s1.Mesh.Faces.Count);
        Assert.True(s3.Mesh.Faces.Count > s2.Mesh.Faces.Count);
    }

    [Fact]
    public void CreateSphere_AllFacesTriangles()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
            Assert.Equal(3, face.GetVertices().Count);
    }

    [Fact]
    public void CreateSphere_VerticesOnSurface()
    {
        double radius = 2.0;
        var center = new Vec3(1, 2, 3);
        var mesh = MeshFactory.CreateSphere(center, radius, 2).Mesh;
        foreach (var v in mesh.Vertices)
        {
            double dist = Vec3.Distance(v.Position, center);
            Assert.True(System.Math.Abs(dist - radius) < 0.01,
                $"Vertex at {v.Position} has distance {dist} from center, expected {radius}");
        }
    }

    [Fact]
    public void CreateCube_BoundsCorrect()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
        Assert.True(bounds.Min.Y >= -0.01);
        Assert.True(bounds.Max.Y <= 1.01);
        Assert.True(bounds.Min.Z >= -0.01);
        Assert.True(bounds.Max.Z <= 1.01);
    }

    [Fact]
    public void CreateCube_LargeSize_BoundsCorrect()
    {
        var mesh = MeshFactory.CreateCube(size: 10).Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Size.X > 9);
        Assert.True(bounds.Size.X < 11);
    }

    [Fact]
    public void CreateSphere_HasBvh()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void CreateCube_HasBvh()
    {
        var solid = MeshFactory.CreateCube();
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }
}
