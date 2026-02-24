using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 35: Robustness tests - precision, scaled, rotated (20 tests)</summary>
public class RobustnessTests
{
    [Fact]
    public void ScaledUp_Union_Works()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0), 10).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(3, 3, 3), 10).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ScaledDown_Union_Works()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0), 0.001).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.0003, 0.0003, 0.0003), 0.001).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-12 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FarFromOrigin_Union_Works()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(1000, 1000, 1000)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1000.3, 1000.3, 1000.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FarFromOrigin_Intersect_Works()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(1000, 1000, 1000)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1000.3, 1000.3, 1000.3)).Mesh);
        var result = Csg.Intersect(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FarFromOrigin_Difference_Works()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(1000, 1000, 1000)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1000.3, 1000.3, 1000.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void NegativeCoordinates_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-1.7, -1.7, -1.7)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void MixedSign_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferentSizes_Union()
    {
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0), 0.5).Mesh);
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-0.25, -0.25, -0.25), 2).Mesh);
        var result = Csg.Union(small, large);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferentSizes_Intersect()
    {
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), 0.5).Mesh);
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-0.25, -0.25, -0.25), 2).Mesh);
        var result = Csg.Intersect(small, large);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferentSizes_Difference()
    {
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-0.25, -0.25, -0.25), 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), 0.5).Mesh);
        var result = Csg.Difference(large, small);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_DifferentScales()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 1).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronSphere_Union()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 1.5).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh);
        var result = Csg.Union(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ConsecutiveOperations_Stability()
    {
        var current = new Solid(MeshFactory.CreateCube().Mesh);
        for (int i = 1; i <= 3; i++)
        {
            var next = new Solid(MeshFactory.CreateCube(new Vec3(i * 0.3, i * 0.3, i * 0.3)).Mesh);
            var result = Csg.Union(current, next);
            current = new Solid(result.Mesh);
        }
        Assert.True(current.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void WindingNumber_MatchesRayCast_ForUnion()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var rc = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
    }

    [Fact]
    public void WindingNumber_MatchesRayCast_ForIntersect()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var rc = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
    }

    [Fact]
    public void WindingNumber_MatchesRayCast_ForDifference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var rc = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
    }

    [Fact]
    public void GridSize_1e6_StillWorks()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void GridSize_1e10_StillWorks()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-10 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WeldTolerance_1e6_StillWorks()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { WeldTolerance = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOptions_Combined()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions
        {
            GridSize = 1e-6,
            WeldTolerance = 1e-6,
            UseWindingNumber = true,
            ClassificationStrategy = new MdCsg.Classification.CpuPatchClassificationStrategy()
        };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }
}
