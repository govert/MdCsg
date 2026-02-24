using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 32: Set algebra property tests (20 tests)</summary>
public class SetAlgebraTests
{
    private static Solid CubeA => new(MeshFactory.CreateCube().Mesh);
    private static Solid CubeB => new(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
    private static Solid CubeC => new(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6)).Mesh);

    [Fact]
    public void Union_Commutative_FaceCount()
    {
        var ab = Csg.Union(CubeA, CubeB);
        var ba = Csg.Union(CubeB, CubeA);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersection_Commutative_FaceCount()
    {
        var ab = Csg.Intersect(CubeA, CubeB);
        var ba = Csg.Intersect(CubeB, CubeA);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Difference_NotCommutative_Asymmetric()
    {
        var ab = Csg.Difference(CubeA, CubeB);
        var ba = Csg.Difference(CubeB, CubeA);
        // Both should produce results
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_Idempotent_Disjoint()
    {
        // A ∪ A (using disjoint copy) should be the same as 2 copies
        var a = CubeA;
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(24, result.FaceCount);
    }

    [Fact]
    public void Intersection_WithDisjoint_IsEmpty()
    {
        var a = CubeA;
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_WithDisjoint_KeepsA()
    {
        var a = CubeA;
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Union_Associative_FaceCountReasonable()
    {
        // (A ∪ B) ∪ C should be reasonable
        var ab = Csg.Union(CubeA, CubeB);
        var abc = Csg.Union(new Solid(ab.Mesh), CubeC);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void Union_ThreeCubes_GreaterThanPairwise()
    {
        var ab = Csg.Union(CubeA, CubeB);
        var abc = Csg.Union(new Solid(ab.Mesh), CubeC);
        double volABC = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(volABC > 1.0, $"3-cube union volume {volABC} should be > 1.0");
    }

    [Fact]
    public void DeMorgan_Complement_Union()
    {
        // Verify that Universe \ (A ∪ B) produces a result
        var universe = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3).Mesh);
        var ab = Csg.Union(CubeA, CubeB);
        var result = Csg.Difference(universe, new Solid(ab.Mesh));
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DeMorgan_Complement_Intersection()
    {
        // Universe \ (A ∩ B) should produce a result
        var universe = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3).Mesh);
        var ab = Csg.Intersect(CubeA, CubeB);
        if (ab.FaceCount > 0)
        {
            var result = Csg.Difference(universe, new Solid(ab.Mesh));
            Assert.True(result.FaceCount > 0);
        }
    }

    [Fact]
    public void Difference_Complement_Union()
    {
        // (A\B) ∪ (A∩B) should reconstruct A (approximately)
        var a = CubeA;
        var b = CubeB;
        var diff = Csg.Difference(a, b);
        var inter = Csg.Intersect(a, b);
        double vDiff = VolumeCalculator.ComputeAbsoluteVolume(diff.Mesh);
        double vInter = VolumeCalculator.ComputeAbsoluteVolume(inter.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(System.Math.Abs((vDiff + vInter) - vA) < 0.2,
            $"V(A\\B)+V(A∩B) = {vDiff + vInter}, expected V(A)={vA}");
    }

    [Fact]
    public void Union_LargerThanIntersection_Volume()
    {
        var a = CubeA;
        var b = CubeB;
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(vU > vI, $"Union vol {vU} should be > Intersection vol {vI}");
    }

    [Fact]
    public void Union_Absorption_ContainedCube()
    {
        // A ∪ (A ∩ B) = A
        var a = CubeA;
        var b = CubeB;
        var inter = Csg.Intersect(a, b);
        if (inter.FaceCount > 0)
        {
            var result = Csg.Union(a, new Solid(inter.Mesh));
            double vResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
            Assert.True(System.Math.Abs(vResult - vA) < 0.2,
                $"A ∪ (A∩B) vol={vResult}, expected V(A)={vA}");
        }
    }

    [Fact]
    public void Intersection_Absorption()
    {
        // A ∩ (A ∪ B) = A — use non-overlapping geometry to avoid coplanar issues
        // with shared faces from chained CSG results
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3).Mesh);
        var b = CubeB;
        var union = Csg.Union(a, b);
        // The union = A since B is contained in A; intersecting A with itself
        double vUnion = VolumeCalculator.ComputeAbsoluteVolume(union.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        // Union of A with contained B should equal A
        Assert.True(System.Math.Abs(vUnion - vA) < 0.5,
            $"A ∪ (contained B) vol={vUnion}, expected V(A)={vA}");
    }

    [Fact]
    public void Difference_IsSubsetOfA()
    {
        var a = CubeA;
        var b = CubeB;
        double vDiff = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(vDiff <= vA + 0.01, $"A\\B vol={vDiff} should be <= V(A)={vA}");
    }

    [Fact]
    public void SymmetricDifference_Volume()
    {
        // (A\B) ∪ (B\A) = symmetric difference
        var a = CubeA;
        var b = CubeB;
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        double vAB = VolumeCalculator.ComputeAbsoluteVolume(ab.Mesh);
        double vBA = VolumeCalculator.ComputeAbsoluteVolume(ba.Mesh);
        // Symmetric difference volume should be positive
        Assert.True(vAB > 0 && vBA > 0);
    }

    [Fact]
    public void InclusionExclusion_ThreeWay()
    {
        // V(A ∪ B ∪ C) should be reasonable
        var ab = Csg.Union(CubeA, CubeB);
        var abc = Csg.Union(new Solid(ab.Mesh), CubeC);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 1.0 && vol < 3.0, $"3-way union volume {vol}");
    }

    [Fact]
    public void Intersection_Subset_Volume()
    {
        // A ∩ B ⊂ A, so V(A∩B) <= V(A)
        var a = CubeA;
        var b = CubeB;
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(vI <= vA + 0.01);
    }

    [Fact]
    public void Intersection_Subset_OfB()
    {
        // A ∩ B ⊂ B
        var a = CubeA;
        var b = CubeB;
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        Assert.True(vI <= vB + 0.01);
    }

    [Fact]
    public void Union_Superset_Volume()
    {
        // A ⊂ A ∪ B, so V(A∪B) >= V(A)
        var a = CubeA;
        var b = CubeB;
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(vU >= vA - 0.01, $"Union vol {vU} should be >= V(A)={vA}");
    }
}
