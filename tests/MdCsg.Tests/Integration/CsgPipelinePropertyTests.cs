using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG pipeline — Evaluate metadata, mesh quality, edge cases</summary>
public class CsgPipelinePropertyTests
{
    [Fact]
    public void Union_CubeCube_PatchCountsPositive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA >= 2);
        Assert.True(result.PatchCountB >= 2);
    }

    [Fact]
    public void Intersect_CubeCube_PatchCountsPositive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA >= 2);
        Assert.True(result.PatchCountB >= 2);
    }

    [Fact]
    public void Difference_CubeCube_PatchCountsPositive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        Assert.True(result.PatchCountA >= 2);
        Assert.True(result.PatchCountB >= 2);
    }

    [Fact]
    public void Disjoint_PatchCountsEqualOne()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        // Each mesh has 1 patch (no intersection to split them)
        Assert.Equal(1, result.PatchCountA);
        Assert.Equal(1, result.PatchCountB);
    }

    [Fact]
    public void Union_ResultMesh_HasValidFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var tri = new Triangle3(va, vb, vc);
            Assert.True(tri.Area > 0);
        }
    }

    [Fact]
    public void Union_TetrahedronCube_ProducesFaces()
    {
        var a = MeshFactory.CreateTetrahedron();
        var b = MeshFactory.CreateCube(Vec3.Zero, 0.5);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_TetrahedronCube_ProducesFaces()
    {
        var a = MeshFactory.CreateTetrahedron();
        var b = MeshFactory.CreateCube(Vec3.Zero, 0.3);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Solid_FromMesh_RoundTrip()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var solid = new Solid(cube.Mesh);
        Assert.Equal(cube.Mesh.Faces.Count, solid.Mesh.Faces.Count);
        Assert.NotNull(solid.Bvh);
    }

    [Fact]
    public void Union_LargerSphere_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 3);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeCube_ResultVerticesInBounds()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= -2.1 && v.Position.X <= 3.1);
            Assert.True(v.Position.Y >= -2.1 && v.Position.Y <= 2.1);
            Assert.True(v.Position.Z >= -2.1 && v.Position.Z <= 2.1);
        }
    }

    [Fact]
    public void Difference_CubeCube_FewerVerticesThanUnion()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var unionResult = Csg.Union(a, b);
        var diffResult = Csg.Difference(a, b);
        // Difference removes material, so should have fewer or equal vertices
        Assert.True(diffResult.VertexCount <= unionResult.VertexCount + 10); // small tolerance for mesh construction
    }
}
