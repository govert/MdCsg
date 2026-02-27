using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 08: ImplicitCylinder intersect/diff with all primitives.</summary>
public class ImplicitCylinderIntegrationTests
{
    private static readonly ImplicitCylinder Cyl = new(new Vec3(0, 0, -1), Vec3.UnitZ, 0.8, 2.0);

    [Fact]
    public void IntersectCubeAndImplicitCylinder_ProducesValidMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, Cyl);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void DifferenceCubeMinusImplicitCylinder_ProducesValidMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Difference(cube, Cyl);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectBoxAndImplicitCylinder_ProducesValidMesh()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 3));
        var r = Csg.Intersect(box, Cyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceBoxMinusImplicitCylinder_ProducesValidMesh()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 3));
        var r = Csg.Difference(box, Cyl);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectSphereAndImplicitCylinder_ProducesValidMesh()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var r = Csg.Intersect(sphere, Cyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceSphereMinusImplicitCylinder_ProducesValidMesh()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var r = Csg.Difference(sphere, Cyl);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectCylinderAndImplicitCylinder_ProducesValidMesh()
    {
        var meshCyl = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 1.5, 4.0, 16);
        var r = Csg.Intersect(meshCyl, Cyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceCylinderMinusImplicitCylinder_ProducesValidMesh()
    {
        var meshCyl = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 1.5, 4.0, 16);
        var r = Csg.Difference(meshCyl, Cyl);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectConeAndImplicitCylinder_ProducesValidMesh()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.5, 2.0, 16);
        var r = Csg.Intersect(cone, Cyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceConeMinusImplicitCylinder_ProducesValidMesh()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.5, 2.0, 16);
        var r = Csg.Difference(cone, Cyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void IntersectImplicitCylinderAndCube_Commutative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(Cyl, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ImplicitCylinderIntersect_VolumeReasonable()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, Cyl);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol > 0);
        Assert.True(vol < cube.Volume());
    }

    [Fact]
    public void ImplicitCylinderDifference_ReducesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Difference(cube, Cyl);
        Assert.True(new Solid(r.Mesh).Volume() < cube.Volume());
    }

    [Fact]
    public void OffsetImplicitCylinder_StillWorks()
    {
        var offCyl = new ImplicitCylinder(new Vec3(0.5, 0, -1), Vec3.UnitZ, 0.5, 2.0);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, offCyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void HorizontalImplicitCylinder_Intersects()
    {
        var hCyl = new ImplicitCylinder(new Vec3(-1, 0, 0), Vec3.UnitX, 0.5, 2.0);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, hCyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void TiltedImplicitCylinder_Intersects()
    {
        var tCyl = new ImplicitCylinder(new Vec3(0, 0, -1), new Vec3(1, 0, 1).Normalized, 0.5, 2.0);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, tCyl);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ImplicitCylinder_ClassifierWorks()
    {
        var classifier = Cyl.CreateClassifier();
        Assert.Equal(MdCsg.Classification.SolidClassification.Inside,
            classifier.Classify(Vec3.Zero));
        Assert.Equal(MdCsg.Classification.SolidClassification.Outside,
            classifier.Classify(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void ImplicitCylinder_SurfaceNormal_Lateral()
    {
        var n = Cyl.SurfaceNormal(new Vec3(0.8, 0, 0));
        Assert.True(System.Math.Abs(n.Z) < 0.1); // approximately radial
    }

    [Fact]
    public void ImplicitCylinder_SurfaceNormal_TopCap()
    {
        var n = Cyl.SurfaceNormal(new Vec3(0, 0, 2));
        Assert.True(n.Z > 0.9);
    }

    [Fact]
    public void ImplicitCylinder_SurfaceNormal_BottomCap()
    {
        var n = Cyl.SurfaceNormal(new Vec3(0, 0, -2));
        Assert.True(n.Z < -0.9);
    }
}
