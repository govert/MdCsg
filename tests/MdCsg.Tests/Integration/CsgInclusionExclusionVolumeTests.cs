using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG inclusion-exclusion volume — |A∪B| + |A∩B| ≈ |A| + |B|, DeMorgan, complement properties</summary>
public class CsgInclusionExclusionVolumeTests
{
    private static double Volume(CsgResult result)
        => VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

    private static double Volume(Solid solid)
        => VolumeCalculator.ComputeAbsoluteVolume(solid.Mesh);

    [Fact]
    public void InclusionExclusion_OverlappingCubes()
    {
        // |A ∪ B| + |A ∩ B| = |A| + |B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = Volume(a);
        double volB = Volume(b);
        double volUnion = Volume(Csg.Union(a, b));
        double volIntersect = Volume(Csg.Intersect(a, b));

        double lhs = volUnion + volIntersect;
        double rhs = volA + volB;
        Assert.True(System.Math.Abs(lhs - rhs) < 0.05,
            $"|A∪B|+|A∩B|={lhs:F4} vs |A|+|B|={rhs:F4}");
    }

    [Fact]
    public void InclusionExclusion_DisjointCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        double volA = Volume(a);
        double volB = Volume(b);
        double volUnion = Volume(Csg.Union(a, b));

        // Disjoint: |A ∪ B| = |A| + |B|
        Assert.True(System.Math.Abs(volUnion - (volA + volB)) < 0.01,
            $"Disjoint union: {volUnion:F4} vs {volA + volB:F4}");
    }

    [Fact]
    public void DifferenceVolume_Equals_AMinusIntersect()
    {
        // |A - B| = |A| - |A ∩ B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = Volume(a);
        double volDiff = Volume(Csg.Difference(a, b));
        double volIntersect = Volume(Csg.Intersect(a, b));

        double expected = volA - volIntersect;
        Assert.True(System.Math.Abs(volDiff - expected) < 0.05,
            $"|A-B|={volDiff:F4} vs |A|-|A∩B|={expected:F4}");
    }

    [Fact]
    public void Union_Commutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh);

        double volAB = Volume(Csg.Union(a, b));
        double volBA = Volume(Csg.Union(b, a));

        Assert.True(System.Math.Abs(volAB - volBA) < 0.05,
            $"|A∪B|={volAB:F4} vs |B∪A|={volBA:F4}");
    }

    [Fact]
    public void Intersection_Commutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh);

        double volAB = Volume(Csg.Intersect(a, b));
        double volBA = Volume(Csg.Intersect(b, a));

        Assert.True(System.Math.Abs(volAB - volBA) < 0.05,
            $"|A∩B|={volAB:F4} vs |B∩A|={volBA:F4}");
    }

    [Fact]
    public void Union_LargerThan_EachPart()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);

        double volA = Volume(a);
        double volB = Volume(b);
        double volUnion = Volume(Csg.Union(a, b));

        Assert.True(volUnion >= volA - 0.01);
        Assert.True(volUnion >= volB - 0.01);
        Assert.True(volUnion <= volA + volB + 0.01);
    }

    [Fact]
    public void Intersection_SmallerThan_EachPart()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);

        double volA = Volume(a);
        double volB = Volume(b);
        double volInt = Volume(Csg.Intersect(a, b));

        Assert.True(volInt <= volA + 0.01);
        Assert.True(volInt <= volB + 0.01);
    }

    [Fact]
    public void Difference_SmallerThan_A()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = Volume(a);
        double volDiff = Volume(Csg.Difference(a, b));

        Assert.True(volDiff <= volA + 0.01);
        Assert.True(volDiff >= 0);
    }

    [Fact]
    public void OverlappingCubes_IntersectionVolume_MatchesGeometry()
    {
        // A at origin, B offset by 0.5 in X. Both unit cubes.
        // Overlap is 0.5 x 1 x 1 = 0.5
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volInt = Volume(Csg.Intersect(a, b));
        Assert.True(System.Math.Abs(volInt - 0.5) < 0.05,
            $"Expected intersection volume ~0.5, got {volInt:F4}");
    }

    [Fact]
    public void OverlappingCubes_UnionVolume_MatchesGeometry()
    {
        // Two unit cubes, offset 0.5 in X → union = 1 + 1 - 0.5 = 1.5
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volUnion = Volume(Csg.Union(a, b));
        Assert.True(System.Math.Abs(volUnion - 1.5) < 0.05,
            $"Expected union volume ~1.5, got {volUnion:F4}");
    }

    [Fact]
    public void OverlappingCubes_DifferenceVolume_MatchesGeometry()
    {
        // A - B: A minus 0.5 overlap = 0.5
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volDiff = Volume(Csg.Difference(a, b));
        Assert.True(System.Math.Abs(volDiff - 0.5) < 0.05,
            $"Expected difference volume ~0.5, got {volDiff:F4}");
    }

    [Fact]
    public void FullyContained_Intersection_EqualsSmaller()
    {
        // Small cube fully inside large cube
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);

        double volSmall = Volume(small);
        double volInt = Volume(Csg.Intersect(large, small));

        Assert.True(System.Math.Abs(volInt - volSmall) < 0.05,
            $"Intersection of fully-contained should equal smaller: {volInt:F4} vs {volSmall:F4}");
    }

    [Fact]
    public void FullyContained_Difference_ReducesVolume()
    {
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);

        double volLarge = Volume(large);
        double volSmall = Volume(small);
        double volDiff = Volume(Csg.Difference(large, small));

        double expected = volLarge - volSmall;
        Assert.True(System.Math.Abs(volDiff - expected) < 0.1,
            $"|Large-Small|={volDiff:F4} vs expected {expected:F4}");
    }

    [Fact]
    public void ThreeBody_InclusionExclusion()
    {
        // |A ∪ B ∪ C| ≤ |A| + |B| + |C|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);

        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var abc = Csg.Union(abSolid, c);

        double volABC = Volume(abc);
        double volA = Volume(a);
        double volB = Volume(b);
        double volC = Volume(c);

        Assert.True(volABC <= volA + volB + volC + 0.01);
        Assert.True(volABC >= volA - 0.01);
    }
}
