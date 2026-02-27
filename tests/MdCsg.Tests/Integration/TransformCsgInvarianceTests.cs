using MdCsg.Api;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 04: Transform→CSG→inverse = direct CSG.</summary>
public class TransformCsgInvarianceTests
{
    private const double Tol = 0.5;

    [Fact]
    public void TranslatedUnion_VolumePreserved()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var directVol = new Solid(Csg.Union(a, b).Mesh).Volume();

        var t = Transform3.FromTranslation(new Vec3(5, 5, 5));
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Union(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void RotatedIntersection_VolumePreserved()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var directVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();

        var t = Transform3.FromRotationZ(0.5);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Intersect(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void RotatedDifference_VolumePreserved()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var directVol = new Solid(Csg.Difference(a, b).Mesh).Volume();

        var t = Transform3.FromRotationX(1.0);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Difference(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void TranslateAndInvert_CsgUnchanged()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var directVol = new Solid(Csg.Union(a, b).Mesh).Volume();

        var t = Transform3.FromTranslation(new Vec3(10, -3, 7));
        var inv = t.Inverse();
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var result = new Solid(Csg.Union(at, bt).Mesh).Transform(inv);
        var invVol = result.Volume();

        Assert.True(System.Math.Abs(directVol - invVol) < Tol);
    }

    [Fact]
    public void RotateXYZ_VolumeInvariant()
    {
        var a = Primitives.Box(Vec3.Zero, new Vec3(2, 1, 1));
        var b = Primitives.Sphere(Vec3.Zero, 0.6, 2);
        var directVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();

        var t = Transform3.FromEulerXYZ(0.3, 0.5, 0.7);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Intersect(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void AxisAngleRotation_VolumeInvariant()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 0.5, 2.0, 12);
        var directVol = new Solid(Csg.Difference(a, b).Mesh).Volume();

        var t = Transform3.FromAxisAngle(new Vec3(1, 1, 1).Normalized, 1.2);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Difference(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void ComposeTranslations_VolumeInvariant()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.5, 0), 2.0);
        var directVol = new Solid(Csg.Union(a, b).Mesh).Volume();

        var t1 = Transform3.FromTranslation(new Vec3(3, 0, 0));
        var t2 = Transform3.FromTranslation(new Vec3(0, 4, 0));
        var t = t1 * t2;
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Union(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void RotationThenTranslation_VolumeInvariant()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 1.5);
        var directVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();

        var rot = Transform3.FromRotationY(0.8);
        var trans = Transform3.FromTranslation(new Vec3(2, 3, 4));
        var t = trans * rot;
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Intersect(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void IdentityTransform_ExactVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var original = new Solid(Csg.Difference(a, b).Mesh).Volume();

        var t = Transform3.Identity;
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformed = new Solid(Csg.Difference(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(original - transformed) < 0.01);
    }

    [Fact]
    public void LargeTranslation_VolumePreserved()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var directVol = new Solid(Csg.Union(a, b).Mesh).Volume();

        var t = Transform3.FromTranslation(new Vec3(1000, 2000, 3000));
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Union(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void MultipleRotations_VolumePreserved()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.2, 2);
        const double rotationTol = 1.0;

        double vol0 = new Solid(Csg.Union(a, b).Mesh).Volume();
        for (int i = 1; i <= 3; i++)
        {
            var t = Transform3.FromRotationZ(i * System.Math.PI / 6);
            var at = a.Transform(t);
            var bt = b.Transform(t);
            double vol = new Solid(Csg.Union(at, bt).Mesh).Volume();
            Assert.True(System.Math.Abs(vol0 - vol) < rotationTol);
        }
    }

    [Fact]
    public void TransformInverse_RoundTrip()
    {
        var t = Transform3.FromEulerXYZ(0.3, 0.5, 0.7);
        var inv = t.Inverse();
        var composed = t * inv;
        var p = new Vec3(1, 2, 3);
        var result = composed.TransformPoint(p);
        Assert.True(Vec3.Distance(p, result) < 1e-10);
    }

    [Fact]
    public void TransformPreservesInsideOutside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var t = Transform3.FromTranslation(new Vec3(5, 0, 0));
        var moved = cube.Transform(t);
        Assert.True(moved.IsInside(new Vec3(5, 0, 0)));
        Assert.False(moved.IsInside(Vec3.Zero));
    }

    [Fact]
    public void RotatedCsg_IsInsideConsistent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var diff = new Solid(Csg.Difference(a, b).Mesh);

        var t = Transform3.FromRotationZ(System.Math.PI / 4);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var diffT = new Solid(Csg.Difference(at, bt).Mesh);

        var p = new Vec3(0.8, 0.8, 0);
        var pt = t.TransformPoint(p);
        Assert.Equal(diff.IsInside(p), diffT.IsInside(pt));
    }

    [Fact]
    public void NegativeTranslation_VolumeInvariant()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var directVol = new Solid(Csg.Difference(a, b).Mesh).Volume();

        var t = Transform3.FromTranslation(new Vec3(-10, -10, -10));
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Difference(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void RotateAroundY_DifferenceVolumeInvariant()
    {
        var a = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var directVol = new Solid(Csg.Difference(a, b).Mesh).Volume();

        var t = Transform3.FromRotationY(1.1);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Difference(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void CompoundTransform_VolumeInvariant()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.5);
        var directVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();

        var t = Transform3.FromAxisAngle(Vec3.UnitZ, 0.3)
              * Transform3.FromTranslation(new Vec3(1, 2, 3))
              * Transform3.FromRotationX(0.5);
        var at = a.Transform(t);
        var bt = b.Transform(t);
        var transformedVol = new Solid(Csg.Intersect(at, bt).Mesh).Volume();

        Assert.True(System.Math.Abs(directVol - transformedVol) < Tol);
    }

    [Fact]
    public void SurfaceArea_InvariantUnderRigidTransform()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var directArea = a.SurfaceArea();

        var t = Transform3.FromEulerXYZ(0.2, 0.4, 0.6) * Transform3.FromTranslation(new Vec3(3, 4, 5));
        var at = a.Transform(t);
        Assert.True(System.Math.Abs(directArea - at.SurfaceArea()) < 0.01);
    }
}
