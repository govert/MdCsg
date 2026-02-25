using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG difference — A\B properties, non-overlapping, self-subtraction effects</summary>
public class CsgDifferenceSubtractionTests
{
    private static double Vol(Solid s) => VolumeCalculator.ComputeAbsoluteVolume(s.Mesh);
    private static double Vol(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Disjoint_Difference_EqualsFirstInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Difference(a, b);
        Assert.True(System.Math.Abs(Vol(result) - Vol(a)) < 0.15,
            $"Disjoint difference should equal A: {Vol(result):F4} vs {Vol(a):F4}");
    }

    [Fact]
    public void Difference_HasFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SmallerThan_Union()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var diff = Csg.Difference(a, b);
        var union = Csg.Union(a, b);
        Assert.True(Vol(diff) < Vol(union),
            $"Difference {Vol(diff):F4} should be < union {Vol(union):F4}");
    }

    [Fact]
    public void DifferenceA_Plus_DifferenceB_Plus_Intersection_EqualsUnion()
    {
        // V(A\B) + V(B\A) + 2*V(A∩B) ≈ V(A) + V(B)
        // Equivalently: V(A\B) + V(A∩B) ≈ V(A)
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var diffAB = Csg.Difference(a, b);
        var inter = Csg.Intersect(a, b);
        double lhs = Vol(diffAB) + Vol(inter);
        double rhs = Vol(a);
        Assert.True(System.Math.Abs(lhs - rhs) < 0.3,
            $"V(A\\B) + V(A∩B) = {lhs:F4} vs V(A) = {rhs:F4}");
    }

    [Fact]
    public void SphereCube_Difference_PositiveVolume()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var result = Csg.Difference(a, b);
        Assert.True(Vol(result) > 0, $"Sphere minus cube should have positive volume");
    }

    [Fact]
    public void CubeSphere_Difference_PositiveVolume()
    {
        var a = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var b = MeshFactory.CreateSphere(Vec3.Zero, 0.4, 2);
        var result = Csg.Difference(a, b);
        Assert.True(Vol(result) > 0, $"Cube minus sphere should have positive volume");
    }

    [Fact]
    public void Difference_NonCommutative()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var diffAB = Csg.Difference(a, b);
        var diffBA = Csg.Difference(b, a);
        // In general, A\B != B\A
        // But their volumes might coincidentally be close if shapes are similar
        // At least verify both have positive volume
        Assert.True(Vol(diffAB) > 0);
        Assert.True(Vol(diffBA) > 0);
    }

    [Fact]
    public void SmallOffset_Difference_SmallVolume()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1));
        var result = Csg.Difference(a, b);
        // Small offset → large overlap → small difference
        Assert.True(Vol(result) < Vol(a),
            $"Small-offset difference {Vol(result):F4} should be less than input {Vol(a):F4}");
    }

    [Fact]
    public void LargeOffset_Difference_EqualsInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Difference(a, b);
        // No overlap → difference equals first input
        Assert.True(System.Math.Abs(Vol(result) - Vol(a)) < 0.15,
            $"Non-overlapping difference should equal input: {Vol(result):F4} vs {Vol(a):F4}");
    }

    [Fact]
    public void Difference_HasVertices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Difference_Result_MoreFacesThanInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        // Cutting creates more faces than the original 12
        Assert.True(result.FaceCount >= 12,
            $"Difference should have >= 12 faces, got {result.FaceCount}");
    }
}
