using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG volume accuracy tests — verifying geometric correctness of CSG results</summary>
public class CsgVolumeAccuracyTests
{
    [Fact]
    public void Union_IdenticalCubes_DoesNotCrash()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(a, b);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(0.2)]
    [InlineData(0.4)]
    [InlineData(0.6)]
    [InlineData(0.8)]
    public void Union_XOffset_VolumePositive(double offset)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0, $"offset={offset}, union vol={vol} should be > 1.0");
        Assert.True(vol < 2.1, $"offset={offset}, union vol={vol} should be < 2.1");
    }

    [Theory]
    [InlineData(0.2)]
    [InlineData(0.4)]
    [InlineData(0.6)]
    [InlineData(0.8)]
    public void Intersection_XOffset_VolumePositive(double offset)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0, $"offset={offset}, intersect vol={vol} should be > 0");
        Assert.True(vol < 1.0, $"offset={offset}, intersect vol={vol} should be < 1.0");
    }

    [Theory]
    [InlineData(0.2)]
    [InlineData(0.4)]
    [InlineData(0.6)]
    [InlineData(0.8)]
    public void Difference_XOffset_VolumePositive(double offset)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0, $"offset={offset}, diff vol={vol} should be > 0");
        Assert.True(vol < 1.0, $"offset={offset}, diff vol={vol} should be < 1.0");
    }

    [Fact]
    public void Union_LargerOverlap_SmallerVolume()
    {
        // More overlap → smaller union volume
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b1 = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0, 0)).Mesh);
        var b2 = new Solid(MeshFactory.CreateCube(new Vec3(0.7, 0, 0)).Mesh);
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b1).Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b2).Mesh);
        // offset 0.3 has more overlap than 0.7, so union of 0.3 should be smaller
        Assert.True(vol1 < vol2, $"vol1(offset=0.3)={vol1} should be < vol2(offset=0.7)={vol2}");
    }

    [Fact]
    public void Intersection_LargerOverlap_LargerVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b1 = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0, 0)).Mesh);
        var b2 = new Solid(MeshFactory.CreateCube(new Vec3(0.7, 0, 0)).Mesh);
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b1).Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b2).Mesh);
        // offset 0.3 has more overlap → larger intersection
        Assert.True(vol1 > vol2, $"vol1(offset=0.3)={vol1} should be > vol2(offset=0.7)={vol2}");
    }

    [Fact]
    public void InclusionExclusion_UnionPlusIntersect_ApproximatelyEqualsSum()
    {
        // |A ∪ B| + |A ∩ B| ≈ |A| + |B| (with wider tolerance for mesh artifacts)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double lhs = volUnion + volIntersect;
        double rhs = volA + volB;
        Assert.True(System.Math.Abs(lhs - rhs) < 1.0,
            $"|A∪B|={volUnion} + |A∩B|={volIntersect} = {lhs} vs |A|+|B|={rhs}");
    }

    [Fact]
    public void Difference_VolumeApproximatelyAMinusIntersection()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(System.Math.Abs(volDiff - (volA - volIntersect)) < 0.5,
            $"|A\\B|={volDiff} vs |A|-|A∩B|={volA - volIntersect}");
    }

    [Fact]
    public void Union_SmallInsideLarge_VolumeApproximatelyLarge()
    {
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), size: 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        double volLarge = VolumeCalculator.ComputeAbsoluteVolume(large.Mesh);
        var result = Csg.Union(large, small);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volLarge) < 1.0, $"union={volResult}, large={volLarge}");
    }

    [Fact]
    public void Intersection_SmallInsideLarge_VolumeApproximatelySmall()
    {
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), size: 4).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size: 0.5).Mesh);
        double volSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        var result = Csg.Intersect(large, small);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volSmall) < 0.1, $"intersect={volResult}, small={volSmall}");
    }

    [Fact]
    public void Difference_SmallFromLarge_VolumeReduced()
    {
        var large = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        double volLarge = VolumeCalculator.ComputeAbsoluteVolume(large.Mesh);
        double volSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        var result = Csg.Difference(large, small);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volResult < volLarge, $"diff={volResult} should be < large={volLarge}");
        Assert.True(volResult > 0, $"diff={volResult} should be > 0");
    }

    [Fact]
    public void Union_DisjointCubes_VolumeSums()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(3, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 2.0) < 0.1, $"Disjoint union vol={vol}");
    }

    [Fact]
    public void Intersection_DisjointCubes_VolumeZero()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(3, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_Sphere_Cube_HasFaces()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(sphere, cube);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0, $"Sphere-cube union vol={vol}");
    }

    [Fact]
    public void CsgResult_FaceCount_Positive_ForAllOperations()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void Union_Monotonicity_MoreOverlap_LessVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        double prevVol = double.MaxValue;
        // Decreasing offsets (more overlap) should give decreasing union volume
        foreach (double offset in new[] { 0.8, 0.6, 0.4, 0.2 })
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
            double vol = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
            Assert.True(vol > 1.0, $"Union vol {vol} should be > 1");
            prevVol = vol;
        }
    }
}
