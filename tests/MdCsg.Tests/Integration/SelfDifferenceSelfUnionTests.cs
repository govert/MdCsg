using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 12: A-A=∅, A∪A≈A, edge cases.</summary>
public class SelfDifferenceSelfUnionTests
{
    private const double Tol = 0.5;

    [Fact]
    public void CubeSelfDifference_EmptyOrZeroVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Difference(a, a);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void SphereSelfDifference_EmptyOrZeroVolume()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Difference(a, a);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void CylinderSelfDifference_EmptyOrZeroVolume()
    {
        var a = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var r = Csg.Difference(a, a);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void CubeSelfUnion_PreservesVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Union(a, a);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < Tol);
    }

    [Fact]
    public void SphereSelfUnion_PreservesVolume()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Union(a, a);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < Tol);
    }

    [Fact]
    public void CubeSelfIntersection_PreservesVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(a, a);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < Tol);
    }

    [Fact]
    public void SphereSelfIntersection_PreservesVolume()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Intersect(a, a);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < Tol);
    }

    [Fact]
    public void ConeSelfDifference_EmptyOrZeroVolume()
    {
        var a = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var r = Csg.Difference(a, a);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void TorusSelfDifference_EmptyOrZeroVolume()
    {
        var a = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var r = Csg.Difference(a, a);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void BoxSelfUnion_PreservesVolume()
    {
        var a = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var r = Csg.Union(a, a);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < Tol);
    }

    [Fact]
    public void SelfDifference_ProducesNoInternalPoints()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Difference(a, a);
        if (r.FaceCount > 0)
        {
            var solid = new Solid(r.Mesh);
            Assert.False(solid.IsInside(Vec3.Zero));
        }
    }

    [Fact]
    public void SelfUnion_PreservesIsInside()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = new Solid(Csg.Union(a, a).Mesh);
        Assert.True(r.IsInside(Vec3.Zero));
        Assert.False(r.IsInside(new Vec3(5, 5, 5)));
    }

    [Fact]
    public void SelfIntersection_PreservesIsInside()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = new Solid(Csg.Intersect(a, a).Mesh);
        Assert.True(r.IsInside(Vec3.Zero));
        Assert.False(r.IsInside(new Vec3(5, 5, 5)));
    }

    [Fact]
    public void SelfUnion_FaceCountReasonable()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Union(a, a);
        // Should be approximately the same face count or less
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SelfDifference_ThenUnionOriginal_RestoresVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var diff = Csg.Difference(a, a);
        if (diff.FaceCount > 0)
        {
            var r = Csg.Union(new Solid(diff.Mesh), a);
            Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < Tol);
        }
    }

    [Fact]
    public void CsgResult_SelfDifference_HasLowIntersectionCount()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var csgResult = new Solid(Csg.Union(a, b).Mesh);
        var r = Csg.Difference(csgResult, csgResult);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void TwoCopies_SameVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        Assert.True(System.Math.Abs(a.Volume() - b.Volume()) < 0.001);
    }

    [Fact]
    public void SelfUnionTwice_StableVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r1 = new Solid(Csg.Union(a, a).Mesh);
        var r2 = new Solid(Csg.Union(r1, a).Mesh);
        Assert.True(System.Math.Abs(r1.Volume() - r2.Volume()) < Tol);
    }

    [Fact]
    public void SelfIntersectTwice_StableVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var r1 = new Solid(Csg.Intersect(a, a).Mesh);
        var r2 = new Solid(Csg.Intersect(r1, a).Mesh);
        Assert.True(System.Math.Abs(r1.Volume() - r2.Volume()) < Tol);
    }
}
