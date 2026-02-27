using MdCsg.Api;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 03: Volume identities: A∪B+A∩B=VA+VB, De Morgan volume checks.</summary>
public class CsgVolumeInvariantTests
{
    private const double Tol = 0.5; // generous tolerance for mesh approximations

    private static Solid Cube1() => Primitives.Cube(Vec3.Zero, 2.0);
    private static Solid Cube2() => Primitives.Cube(new Vec3(0.8, 0, 0), 2.0);
    private static Solid Sphere1() => Primitives.Sphere(Vec3.Zero, 1.0, 2);
    private static Solid Sphere2() => Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 2);

    [Fact]
    public void InclusionExclusion_TwoCubes()
    {
        var a = Cube1(); var b = Cube2();
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        var expected = a.Volume() + b.Volume();
        Assert.True(System.Math.Abs(union + inter - expected) < Tol);
    }

    [Fact]
    public void InclusionExclusion_TwoSpheres()
    {
        var a = Sphere1(); var b = Sphere2();
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        var expected = a.Volume() + b.Volume();
        Assert.True(System.Math.Abs(union + inter - expected) < Tol);
    }

    [Fact]
    public void InclusionExclusion_CubeAndSphere()
    {
        var a = Cube1(); var b = Sphere1();
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        var expected = a.Volume() + b.Volume();
        Assert.True(System.Math.Abs(union + inter - expected) < Tol);
    }

    [Fact]
    public void DifferenceVolume_EqualsDifferenceOfVolumes()
    {
        var a = Cube1(); var b = Sphere2();
        var diff = new Solid(Csg.Difference(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        // V(A\B) = V(A) - V(A∩B)
        Assert.True(System.Math.Abs(diff - (a.Volume() - inter)) < Tol);
    }

    [Fact]
    public void UnionVolume_GreaterThanOrEqualToEach()
    {
        var a = Cube1(); var b = Cube2();
        var unionVol = new Solid(Csg.Union(a, b).Mesh).Volume();
        Assert.True(unionVol >= a.Volume() - Tol);
        Assert.True(unionVol >= b.Volume() - Tol);
    }

    [Fact]
    public void IntersectionVolume_LessThanOrEqualToEach()
    {
        var a = Cube1(); var b = Cube2();
        var interVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        Assert.True(interVol <= a.Volume() + Tol);
        Assert.True(interVol <= b.Volume() + Tol);
    }

    [Fact]
    public void DifferenceVolume_LessThanOrEqualToMinuend()
    {
        var a = Cube1(); var b = Sphere2();
        var diffVol = new Solid(Csg.Difference(a, b).Mesh).Volume();
        Assert.True(diffVol <= a.Volume() + Tol);
        Assert.True(diffVol > 0);
    }

    [Fact]
    public void SymmetricDifference_ViaUnionAndIntersection()
    {
        var a = Cube1(); var b = Cube2();
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        var diffAB = new Solid(Csg.Difference(a, b).Mesh).Volume();
        var diffBA = new Solid(Csg.Difference(b, a).Mesh).Volume();
        // V(A△B) = V(A\B) + V(B\A) = V(A∪B) - V(A∩B)
        Assert.True(System.Math.Abs((diffAB + diffBA) - (union - inter)) < Tol);
    }

    [Fact]
    public void ContainedSolid_IntersectionEqualsSmaller()
    {
        var big = Primitives.Cube(Vec3.Zero, 4.0);
        var small = Primitives.Cube(Vec3.Zero, 1.0);
        var inter = new Solid(Csg.Intersect(big, small).Mesh).Volume();
        Assert.True(System.Math.Abs(inter - small.Volume()) < Tol);
    }

    [Fact]
    public void ContainedSolid_UnionEqualsBigger()
    {
        var big = Primitives.Cube(Vec3.Zero, 4.0);
        var small = Primitives.Cube(Vec3.Zero, 1.0);
        var union = new Solid(Csg.Union(big, small).Mesh).Volume();
        Assert.True(System.Math.Abs(union - big.Volume()) < Tol);
    }

    [Fact]
    public void ContainedSolid_DifferenceRemovesSmaller()
    {
        var big = Primitives.Cube(Vec3.Zero, 4.0);
        var small = Primitives.Cube(Vec3.Zero, 1.0);
        var diff = new Solid(Csg.Difference(big, small).Mesh).Volume();
        Assert.True(System.Math.Abs(diff - (big.Volume() - small.Volume())) < Tol);
    }

    [Fact]
    public void IdenticalSolids_UnionEqualsEither()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var unionVol = new Solid(Csg.Union(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(unionVol - a.Volume()) < Tol);
    }

    [Fact]
    public void IdenticalSolids_IntersectionEqualsEither()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var interVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(interVol - a.Volume()) < Tol);
    }

    [Fact]
    public void VolumePositive_AllOperations()
    {
        var a = Cube1(); var b = Cube2();
        Assert.True(new Solid(Csg.Union(a, b).Mesh).Volume() > 0);
        Assert.True(new Solid(Csg.Intersect(a, b).Mesh).Volume() > 0);
        Assert.True(new Solid(Csg.Difference(a, b).Mesh).Volume() > 0);
    }

    [Fact]
    public void PartitionIdentity_DiffPlusIntersect()
    {
        var a = Cube1(); var b = Cube2();
        var diff = new Solid(Csg.Difference(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        // V(A\B) + V(A∩B) = V(A)
        Assert.True(System.Math.Abs(diff + inter - a.Volume()) < Tol);
    }

    [Fact]
    public void OffsetSpheres_VolumeConsistency()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        Assert.True(union > a.Volume());
        Assert.True(union < 2 * a.Volume() + Tol);
    }

    [Fact]
    public void VolumeMonotonicity_AddingMaterial()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(1.5, 0, 0), 0.5, 2);
        var unionVol = new Solid(Csg.Union(a, b).Mesh).Volume();
        Assert.True(unionVol >= a.Volume() - 0.01);
    }

    [Fact]
    public void VolumeMonotonicity_RemovingMaterial()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var diffVol = new Solid(Csg.Difference(a, b).Mesh).Volume();
        Assert.True(diffVol <= a.Volume() + 0.01);
    }

    [Fact]
    public void LargeOverlap_InclusionExclusionHolds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.1, 0.1, 0.1), 2.0);
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(union + inter - a.Volume() - b.Volume()) < Tol);
    }

    [Fact]
    public void SmallOverlap_InclusionExclusionHolds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1.5, 0, 0), 2.0);
        var union = new Solid(Csg.Union(a, b).Mesh).Volume();
        var inter = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(union + inter - a.Volume() - b.Volume()) < Tol);
    }
}
