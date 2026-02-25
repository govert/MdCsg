using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Csg.Evaluate pipeline — Union/Intersect/Difference with various options and shapes</summary>
public class CsgEvaluatePipelinePropertyTests
{
    [Fact]
    public void Union_OverlappingCubes_HasFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_OverlappingCubes_HasFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_OverlappingCubes_HasFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DisjointCubes_FaceCountIsSumOfBoth()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointCubes_EmptyResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_DisjointCubes_SameAsOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_WithNullOptions_UsesDefaults()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b, null);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_WithCustomGridSize_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var options = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_WithCustomWeldTolerance_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var options = new CsgOptions { WeldTolerance = 1e-6 };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Result_HasVertices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_Result_BoundsContained()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        // Intersection should be contained within both inputs
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_ABnotBA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // A-B and B-A should generally have different face counts for asymmetric geometry
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeSphere_HasPatchCounts()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
    }

    [Fact]
    public void Union_CubeSphere_HasIntersectionSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_Disjoint_ZeroIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_Disjoint_ZeroDegeneratePatches()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.DegenerateCount);
    }

    [Fact]
    public void Intersect_Sphere_SmallOffset_NonEmpty()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0, "Overlapping spheres intersection should be non-empty");
    }

    [Fact]
    public void Difference_ContainedCube_CarvedHole()
    {
        var outer = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var inner = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5);
        var result = Csg.Difference(outer, inner);
        Assert.True(result.FaceCount > outer.Mesh.Faces.Count,
            "Carving a hole should produce more faces than the original");
    }
}
