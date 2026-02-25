using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG Boolean algebra — absorption, distributivity, complement properties</summary>
public class CsgAbsorptionLawTests
{
    private static double Volume(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Union_Disjoint_Volume_Additive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = Volume(Csg.Union(a, b));

        Assert.True(System.Math.Abs(volUnion - (volA + volB)) < 0.01);
    }

    [Fact]
    public void Intersection_Disjoint_Empty()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_Disjoint_EqualsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = Volume(Csg.Difference(a, b));

        Assert.True(System.Math.Abs(volDiff - volA) < 0.01);
    }

    [Fact]
    public void Union_Chained_VolumeInBounds()
    {
        // (A ∪ B) ∪ C volume should be between max(|A|,|B|,|C|) and |A|+|B|+|C|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.5, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volC = VolumeCalculator.ComputeAbsoluteVolume(c.Mesh);

        var abResult = Csg.Union(a, b);
        var abSolid = new Solid(abResult.Mesh);
        double vol_ab_c = Volume(Csg.Union(abSolid, c));

        double maxSingle = System.Math.Max(volA, System.Math.Max(volB, volC));
        double sumAll = volA + volB + volC;

        Assert.True(vol_ab_c >= maxSingle - 0.2, $"Union too small: {vol_ab_c:F4} < {maxSingle:F4}");
        Assert.True(vol_ab_c <= sumAll + 0.2, $"Union too large: {vol_ab_c:F4} > {sumAll:F4}");
    }

    [Fact]
    public void Intersection_Chained_VolumeInBounds()
    {
        // (A ∩ B) ∩ C volume should be between 0 and min(|A|,|B|,|C|)
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 2).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 2).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volC = VolumeCalculator.ComputeAbsoluteVolume(c.Mesh);

        var abResult = Csg.Intersect(a, b);
        var abSolid = new Solid(abResult.Mesh);
        double vol_ab_c = Volume(Csg.Intersect(abSolid, c));

        double minSingle = System.Math.Min(volA, System.Math.Min(volB, volC));

        Assert.True(vol_ab_c >= 0, $"Intersection negative: {vol_ab_c:F4}");
        Assert.True(vol_ab_c <= minSingle + 0.5, $"Intersection too large: {vol_ab_c:F4} > {minSingle:F4}");
        Assert.True(vol_ab_c > 0, "Overlapping cubes should have positive intersection");
    }

    [Fact]
    public void DiffPlusDiff_EqualsUnionMinusIntersect()
    {
        // |A-B| + |B-A| = |A∪B| - |A∩B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volAminusB = Volume(Csg.Difference(a, b));
        double volBminusA = Volume(Csg.Difference(b, a));
        double volUnion = Volume(Csg.Union(a, b));
        double volIntersect = Volume(Csg.Intersect(a, b));

        double lhs = volAminusB + volBminusA;
        double rhs = volUnion - volIntersect;

        Assert.True(System.Math.Abs(lhs - rhs) < 0.1,
            $"|A-B|+|B-A|={lhs:F4} vs |A∪B|-|A∩B|={rhs:F4}");
    }

    [Fact]
    public void DifferenceVolume_Correct()
    {
        // |A - B| = |A| - |A ∩ B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volAminusB = Volume(Csg.Difference(a, b));
        double volAintB = Volume(Csg.Intersect(a, b));

        Assert.True(System.Math.Abs(volAminusB - (volA - volAintB)) < 0.05,
            $"|A-B|={volAminusB:F4} vs |A|-|A∩B|={volA - volAintB:F4}");
    }

    [Fact]
    public void SymmetricDifference_Volume()
    {
        // |A △ B| = |A ∪ B| - |A ∩ B| = |A - B| + |B - A|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volUnion = Volume(Csg.Union(a, b));
        double volIntersect = Volume(Csg.Intersect(a, b));
        double symDiff = volUnion - volIntersect;

        Assert.True(symDiff > 0);
        Assert.True(symDiff < volUnion);
    }

    [Fact]
    public void Union_IsBounded_BySum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = Volume(Csg.Union(a, b));

        Assert.True(volUnion <= volA + volB + 0.01);
        Assert.True(volUnion >= System.Math.Max(volA, volB) - 0.01);
    }

    [Fact]
    public void Intersection_IsBounded()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volInt = Volume(Csg.Intersect(a, b));

        Assert.True(volInt >= 0);
        Assert.True(volInt <= System.Math.Min(volA, volB) + 0.01);
    }
}
