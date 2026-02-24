using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 34: Edge case and robustness tests (20 tests)</summary>
public class EdgeCaseTests
{
    [Fact]
    public void TouchingCubes_Union_DoesNotCrash()
    {
        // Cubes sharing a face plane at x=1
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount >= 0);
    }

    [Fact]
    public void TouchingCubes_Difference_DoesNotCrash()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount >= 0);
    }

    [Fact]
    public void NearlyTouching_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1.0001, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        // Nearly touching = disjoint
        Assert.Equal(24, result.FaceCount);
    }

    [Fact]
    public void TinyOverlap_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.999, 0.001, 0.001)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void LargeScaleCubes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 100).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(30, 30, 30), 100).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallScaleCubes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0), 0.01).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.003, 0.003, 0.003), 0.01).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ContainedCube_Union_KeepsOuter()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Union(outer, inner);
        // Union of contained object should be the outer
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 7.5, $"Expected ~8.0, got {vol}");
    }

    [Fact]
    public void ContainedCube_Difference_CarvesThroughSurface()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void Sphere_LowRes_Union()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Sphere_HighRes_Union()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Difference()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8).Mesh);
        var result = Csg.Difference(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_Difference()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeFromSphere_Difference()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Difference(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void MultipleOperations_DoNotAccumulateErrors()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r1 = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(2, 0, 0)).Mesh);
        var r2 = Csg.Union(new Solid(r1.Mesh), c);
        Assert.True(r2.FaceCount > 0);
        Assert.True(r2.DegenerateCount == 0);
    }

    [Fact]
    public void CustomClassificationStrategy_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions
        {
            ClassificationStrategy = new MdCsg.Classification.CpuPatchClassificationStrategy()
        };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AsymmetricOffsetCubes_AllOpsWork()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.7, 0.2, 0.4)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void Cube_WithNegativeOffset_AllOpsWork()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-0.3, -0.3, -0.3)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
    }

    [Fact]
    public void ThreeConsecutiveUnions()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.1)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(-0.3, 0.5, 0.2)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void UnionAndDifference_Combined()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Union(a, b);
        var result = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOperations_ProduceNonNullMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        Assert.NotNull(Csg.Union(a, b).Mesh);
        Assert.NotNull(Csg.Intersect(a, b).Mesh);
        Assert.NotNull(Csg.Difference(a, b).Mesh);
    }
}
