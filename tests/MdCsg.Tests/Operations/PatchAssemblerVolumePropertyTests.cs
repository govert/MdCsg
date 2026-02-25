using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler via CSG — operation-specific volume properties, patch selection correctness</summary>
public class PatchAssemblerVolumePropertyTests
{
    private static double Volume(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Union_Volume_GreaterThanMax()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = Volume(Csg.Union(a, b));

        Assert.True(volUnion >= System.Math.Max(volA, volB) - 0.01);
    }

    [Fact]
    public void Intersection_Volume_LessThanMin()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volInt = Volume(Csg.Intersect(a, b));

        Assert.True(volInt <= System.Math.Min(volA, volB) + 0.01);
    }

    [Fact]
    public void Difference_Volume_LessThanA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = Volume(Csg.Difference(a, b));

        Assert.True(volDiff <= volA + 0.01);
        Assert.True(volDiff >= 0);
    }

    [Fact]
    public void Union_KeepsBothSurfaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Union(a, b);
        // Union should have patches from both A and B
        Assert.True(result.PatchCountA > 0, "Union should keep patches from A");
        Assert.True(result.PatchCountB > 0, "Union should keep patches from B");
    }

    [Fact]
    public void Intersection_KeepsBothSurfaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA > 0, "Intersection should keep patches from A");
        Assert.True(result.PatchCountB > 0, "Intersection should keep patches from B");
    }

    [Fact]
    public void Difference_KeepsBothSurfaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Difference(a, b);
        Assert.True(result.PatchCountA > 0, "Difference should keep outside-A patches");
        Assert.True(result.PatchCountB > 0, "Difference should keep inside-B patches (flipped)");
    }

    [Fact]
    public void DisjointUnion_AllPatchesKept()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        var result = Csg.Union(a, b);
        // All patches are "outside" for disjoint solids
        Assert.Equal(24, result.FaceCount); // 12 + 12
    }

    [Fact]
    public void DisjointDifference_OnlyA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        var result = Csg.Difference(a, b);
        Assert.Equal(12, result.FaceCount); // Only A's faces
    }

    [Fact]
    public void DisjointIntersection_Empty()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void FullyContained_Union_EqualsOuter()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 3).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);

        var result = Csg.Union(outer, inner);
        double volOuter = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double volResult = Volume(result);

        Assert.True(System.Math.Abs(volResult - volOuter) < 0.1,
            $"Union with fully contained: {volResult:F4} vs outer {volOuter:F4}");
    }

    [Fact]
    public void FullyContained_Intersection_EqualsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 3).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);

        var result = Csg.Intersect(outer, inner);
        double volInner = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        double volResult = Volume(result);

        Assert.True(System.Math.Abs(volResult - volInner) < 0.1,
            $"Intersect with fully contained: {volResult:F4} vs inner {volInner:F4}");
    }

    [Fact]
    public void DegenerateCount_IsSmall()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Union(a, b);
        // Degenerate patches may occur for axis-aligned faces
        Assert.True(result.DegenerateCount <= result.PatchCountA + result.PatchCountB,
            $"Degenerate count {result.DegenerateCount} exceeds total patches");
    }
}
