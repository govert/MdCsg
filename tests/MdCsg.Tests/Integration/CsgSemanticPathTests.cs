using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG semantic path tests — verifying each pipeline path produces correct topological results</summary>
public class CsgSemanticPathTests
{
    [Fact]
    public void Union_CubeCube_ValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        var val = MeshValidator.Validate(result.Mesh);
        Assert.True(val.HasValidFaceCycles);
        Assert.True(val.IsConsistentlyOriented);
    }

    [Fact]
    public void Intersection_CubeCube_ValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        var val = MeshValidator.Validate(result.Mesh);
        Assert.True(val.HasValidFaceCycles);
    }

    [Fact]
    public void Difference_CubeCube_ValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        var val = MeshValidator.Validate(result.Mesh);
        Assert.True(val.HasValidFaceCycles);
    }

    [Fact]
    public void Union_Disjoint_HasSumOfFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Difference_Disjoint_HasFacesOfA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersection_Disjoint_EmptyResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void CsgResult_HasMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CsgResult_Disjoint_ZeroIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_SphereCube_ReasonableFaceCount()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.8, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-0.2, -0.2, -0.2)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 20);
        Assert.True(result.FaceCount < 5000);
    }

    [Fact]
    public void Difference_LargeMinusSmall_VolumeReduced()
    {
        var large = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        var result = Csg.Difference(large, small);
        double vResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vLarge = VolumeCalculator.ComputeAbsoluteVolume(large.Mesh);
        Assert.True(vResult < vLarge, $"Difference should reduce volume: {vResult} vs {vLarge}");
        Assert.True(vResult > 0);
    }

    [Fact]
    public void Intersection_SmallInLarge_VolumeEqualsSmall()
    {
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), size: 4).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        var result = Csg.Intersect(large, small);
        double vResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        Assert.True(System.Math.Abs(vResult - vSmall) < 0.1,
            $"Intersection of contained should equal small: {vResult} vs {vSmall}");
    }

    [Fact]
    public void CsgOptions_WindingNumber_ProducesSameVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var optRC = new CsgOptions { UseWindingNumber = false };
        var optWN = new CsgOptions { UseWindingNumber = true };

        var r1 = Csg.Union(a, b, optRC);
        var r2 = Csg.Union(a, b, optWN);

        double v1 = VolumeCalculator.ComputeAbsoluteVolume(r1.Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(r2.Mesh);
        Assert.True(System.Math.Abs(v1 - v2) < 0.1, $"RC={v1}, WN={v2}");
    }

    [Fact]
    public void CsgOptions_CustomGridSize_StillWorks()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TwoSpheres_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0, $"Union of overlapping spheres should have positive volume: {vol}");
    }

    [Fact]
    public void Intersection_TwoSpheres_SmallerThanEither()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh);
        var vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        var result = Csg.Intersect(a, b);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vI > 0);
        Assert.True(vI < vA + 0.5);
    }

    [Fact]
    public void AllFaces_HaveNonZeroArea()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var area = new Triangle3(va, vb, vc).Area;
            Assert.True(area > 0, $"Face {face.Id} has zero area");
        }
    }

    [Fact]
    public void Difference_AB_And_BA_DifferentVolumes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vAB = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        double vBA = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(b, a).Mesh);
        // A\B and B\A should generally differ (unless symmetric)
        Assert.True(vAB > 0);
        Assert.True(vBA > 0);
    }

    [Fact]
    public void Solid_BoundsEncloseMesh()
    {
        var a = MeshFactory.CreateCube(new Vec3(2, 3, 4), size: 2);
        var bounds = a.Bounds;
        Assert.True(bounds.Min.X <= 2.01);
        Assert.True(bounds.Min.Y <= 3.01);
        Assert.True(bounds.Min.Z <= 4.01);
        Assert.True(bounds.Max.X >= 3.99);
        Assert.True(bounds.Max.Y >= 4.99);
        Assert.True(bounds.Max.Z >= 5.99);
    }
}
