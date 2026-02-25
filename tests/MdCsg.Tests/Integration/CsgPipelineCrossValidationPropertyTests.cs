using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG pipeline cross-validation — RayCast vs WindingNumber consistency, option effects, determinism</summary>
public class CsgPipelineCrossValidationPropertyTests
{
    [Fact]
    public void Union_RayCastVsWinding_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var rayCast = Csg.Union(a, b);
        var winding = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rayCast.FaceCount, winding.FaceCount);
    }

    [Fact]
    public void Intersect_RayCastVsWinding_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var rayCast = Csg.Intersect(a, b);
        var winding = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rayCast.FaceCount, winding.FaceCount);
    }

    [Fact]
    public void Difference_RayCastVsWinding_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var rayCast = Csg.Difference(a, b);
        var winding = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rayCast.FaceCount, winding.FaceCount);
    }

    [Fact]
    public void Union_Deterministic_SameResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
        Assert.Equal(r1.VertexCount, r2.VertexCount);
    }

    [Fact]
    public void Union_DisjointCubes_FaceCountIsSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointCubes_EmptyResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_DisjointCubes_PreservesOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_OverlappingCubes_HasCorrectSegmentCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0,
            "Overlapping cubes should produce intersection segments");
    }

    [Fact]
    public void Union_DisjointCubes_ZeroSegmentCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void CsgResult_PatchCounts_NonNegative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA >= 0);
        Assert.True(result.PatchCountB >= 0);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void Sphere_Union_WithWinding_HasFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgOptions_Default_UsesRayCast()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
    }

    [Fact]
    public void Intersect_OverlappingCubes_HasPatches()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA > 0 || result.PatchCountB > 0);
    }
}
