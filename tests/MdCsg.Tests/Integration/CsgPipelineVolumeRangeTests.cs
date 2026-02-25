using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG pipeline — volume range guarantees across all operations</summary>
public class CsgPipelineVolumeRangeTests
{
    private static double Vol(Solid s) => VolumeCalculator.ComputeAbsoluteVolume(s.Mesh);
    private static double Vol(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Union_Volume_GreaterOrEqual_MaxInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        double vUnion = Vol(result);
        Assert.True(vUnion >= System.Math.Max(Vol(a), Vol(b)) - 0.1,
            $"Union volume {vUnion:F4} should be >= max({Vol(a):F4}, {Vol(b):F4})");
    }

    [Fact]
    public void Union_Volume_LessOrEqual_SumInputs()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        double vUnion = Vol(result);
        Assert.True(vUnion <= Vol(a) + Vol(b) + 0.1,
            $"Union volume {vUnion:F4} should be <= {Vol(a):F4} + {Vol(b):F4}");
    }

    [Fact]
    public void Intersection_Volume_LessOrEqual_MinInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Intersect(a, b);
        double vInt = Vol(result);
        Assert.True(vInt <= System.Math.Min(Vol(a), Vol(b)) + 0.1,
            $"Intersection volume {vInt:F4} should be <= min({Vol(a):F4}, {Vol(b):F4})");
    }

    [Fact]
    public void Intersection_Volume_GreaterOrEqual_Zero()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Intersect(a, b);
        Assert.True(Vol(result) >= -0.01, $"Intersection volume should be >= 0");
    }

    [Fact]
    public void Difference_Volume_LessOrEqual_FirstInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        Assert.True(Vol(result) <= Vol(a) + 0.1,
            $"Difference volume {Vol(result):F4} should be <= {Vol(a):F4}");
    }

    [Fact]
    public void Difference_Volume_GreaterOrEqual_Zero()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        Assert.True(Vol(result) >= -0.01, $"Difference volume should be >= 0");
    }

    [Fact]
    public void Disjoint_Union_Volume_EqualsSumInputs()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(System.Math.Abs(Vol(result) - (Vol(a) + Vol(b))) < 0.15,
            $"Disjoint union volume {Vol(result):F4} should be ~{Vol(a) + Vol(b):F4}");
    }

    [Fact]
    public void Disjoint_Intersection_Volume_NearZero()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.True(Vol(result) < 0.1, $"Disjoint intersection volume should be ~0, got {Vol(result):F4}");
    }

    [Fact]
    public void SphereCube_Union_HasPositiveVolume()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var result = Csg.Union(a, b);
        Assert.True(Vol(result) > 0.5, $"Sphere-cube union volume {Vol(result):F4} should be > 0.5");
    }

    [Fact]
    public void SphereCube_Intersection_Bounded()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var result = Csg.Intersect(a, b);
        Assert.True(Vol(result) <= System.Math.Min(Vol(a), Vol(b)) + 0.3,
            $"Intersection volume exceeds min input");
    }

    [Fact]
    public void Union_Intersection_InclusionExclusion()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        double lhs = Vol(union) + Vol(inter);
        double rhs = Vol(a) + Vol(b);
        Assert.True(System.Math.Abs(lhs - rhs) < 0.3,
            $"Inclusion-exclusion: {lhs:F4} vs {rhs:F4} (diff={System.Math.Abs(lhs - rhs):F4})");
    }
}
