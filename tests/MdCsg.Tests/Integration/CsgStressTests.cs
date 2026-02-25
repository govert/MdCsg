using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG stress tests — larger meshes, sphere-sphere, multiple operations</summary>
public class CsgStressTests
{
    [Fact]
    public void Union_SphereSubdiv3_Cube()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 3).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(sphere, cube);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_SphereSubdiv3_Cube()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 3).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Intersect(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereSubdiv3_Cube()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 3).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Difference(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TwoSpheres_Overlapping()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_TwoSpheres_Overlapping()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_TwoSpheres_Overlapping()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ChainedUnion_ThreeCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var result = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ChainedDifference_TwiceFromCube()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size: 0.3).Mesh);
        var ab = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 1.0, 1.0), size: 0.3).Mesh);
        var result = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Tetrahedron_Cube()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.2, 0.2, 0.2), 0.5).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(tet, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_Tetrahedron_Cube()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.2, 0.2, 0.2), 0.5).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Intersect(tet, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Result_Mesh_IsValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        // Check all faces have 3 vertices
        foreach (var face in result.Mesh.Faces)
        {
            Assert.Equal(3, face.GetVertices().Count);
        }
    }

    [Fact]
    public void Result_NoNaNVertices()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X), "NaN X");
            Assert.False(double.IsNaN(v.Position.Y), "NaN Y");
            Assert.False(double.IsNaN(v.Position.Z), "NaN Z");
        }
    }

    [Fact]
    public void CsgResult_Metadata_Populated()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void CsgResult_AllOperations_HaveMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        foreach (var result in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            Assert.True(result.FaceCount > 0);
            Assert.True(result.VertexCount > 0);
        }
    }
}
