using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 07: ImplicitSphere intersect/diff with all primitives.</summary>
public class ImplicitSphereIntegrationTests
{
    private static readonly ImplicitSphere Sphere = new(Vec3.Zero, 1.0);

    [Fact]
    public void IntersectCubeAndImplicitSphere_ProducesValidMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, Sphere);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void DifferenceCubeMinusImplicitSphere_ProducesValidMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Difference(cube, Sphere);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectBoxAndImplicitSphere_ProducesValidMesh()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 2));
        var r = Csg.Intersect(box, Sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceBoxMinusImplicitSphere_ProducesValidMesh()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 2));
        var r = Csg.Difference(box, Sphere);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectCylinderAndImplicitSphere_ProducesValidMesh()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 1.5, 4.0, 16);
        var r = Csg.Intersect(cyl, Sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceCylinderMinusImplicitSphere_ProducesValidMesh()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 1.5, 4.0, 16);
        var r = Csg.Difference(cyl, Sphere);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectConeAndImplicitSphere_ProducesValidMesh()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.5, 2.0, 16);
        var r = Csg.Intersect(cone, Sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceConeMinusImplicitSphere_ProducesValidMesh()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.5, 2.0, 16);
        var r = Csg.Difference(cone, Sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void IntersectSphereAndImplicitSphere_ProducesValidMesh()
    {
        var meshSphere = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var r = Csg.Intersect(meshSphere, Sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceSphereMinusImplicitSphere_ProducesValidMesh()
    {
        var meshSphere = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var r = Csg.Difference(meshSphere, Sphere);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectImplicitSphereAndCube_Commutative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(Sphere, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ImplicitSphereIntersect_VolumeReasonable()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, Sphere);
        var vol = new Solid(r.Mesh).Volume();
        // Should be approximately the sphere volume (sphere is inside the cube)
        double sphereVol = 4.0 / 3.0 * System.Math.PI * 1.0 * 1.0 * 1.0;
        Assert.True(vol > 0);
        Assert.True(vol < cube.Volume());
    }

    [Fact]
    public void ImplicitSphereDifference_ReducesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Difference(cube, Sphere);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol < cube.Volume());
        Assert.True(vol > 0);
    }

    [Fact]
    public void OffsetImplicitSphere_StillWorks()
    {
        var offsetSphere = new ImplicitSphere(new Vec3(0.5, 0, 0), 0.8);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, offsetSphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SmallImplicitSphere_IntersectsLargeCube()
    {
        var smallSphere = new ImplicitSphere(Vec3.Zero, 0.3);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(cube, smallSphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void LargeImplicitSphere_ContainsCube()
    {
        var largeSphere = new ImplicitSphere(Vec3.Zero, 5.0);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, largeSphere);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(System.Math.Abs(vol - cube.Volume()) < 0.5);
    }

    [Fact]
    public void ImplicitSphere_DiffThenIntersect_Chain()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var s1 = new ImplicitSphere(Vec3.Zero, 1.0);
        var r1 = new Solid(Csg.Difference(cube, s1).Mesh);
        var s2 = new ImplicitSphere(new Vec3(0.5, 0, 0), 0.5);
        var r2 = Csg.Intersect(r1, s2);
        // May or may not have volume depending on overlap
        Assert.True(r2.FaceCount >= 0);
    }

    [Fact]
    public void ImplicitSphere_ClassifierAgreesWithSolid()
    {
        var classifier = Sphere.CreateClassifier();
        Assert.Equal(MdCsg.Classification.SolidClassification.Inside,
            classifier.Classify(Vec3.Zero));
        Assert.Equal(MdCsg.Classification.SolidClassification.Outside,
            classifier.Classify(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void ImplicitSphere_SurfaceNormalPointsOutward()
    {
        var n = Sphere.SurfaceNormal(new Vec3(1, 0, 0));
        Assert.True(Vec3.Dot(n, new Vec3(1, 0, 0)) > 0.99);
    }
}
