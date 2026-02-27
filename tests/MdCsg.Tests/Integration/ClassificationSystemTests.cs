using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 23: All classifiers: Bvh, Sphere, Cylinder, Plane, RayCast vs Winding.</summary>
public class ClassificationSystemTests
{
    [Fact]
    public void BvhClassifier_CubeCenter_Inside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cls = new BvhPointClassifier(cube.Bvh);
        Assert.Equal(SolidClassification.Inside, cls.Classify(Vec3.Zero));
    }

    [Fact]
    public void BvhClassifier_OutsidePoint_Outside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cls = new BvhPointClassifier(cube.Bvh);
        Assert.Equal(SolidClassification.Outside, cls.Classify(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void BvhClassifier_WindingNumber_CubeCenter_Inside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cls = new BvhPointClassifier(cube.Bvh, useWindingNumber: true);
        Assert.Equal(SolidClassification.Inside, cls.Classify(Vec3.Zero));
    }

    [Fact]
    public void BvhClassifier_WindingNumber_OutsidePoint_Outside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cls = new BvhPointClassifier(cube.Bvh, useWindingNumber: true);
        Assert.Equal(SolidClassification.Outside, cls.Classify(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void SphereClassifier_Inside()
    {
        var cls = new SpherePointClassifier(Vec3.Zero, 1.0);
        Assert.Equal(SolidClassification.Inside, cls.Classify(Vec3.Zero));
    }

    [Fact]
    public void SphereClassifier_Outside()
    {
        var cls = new SpherePointClassifier(Vec3.Zero, 1.0);
        Assert.Equal(SolidClassification.Outside, cls.Classify(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void SphereClassifier_Distance()
    {
        var cls = new SpherePointClassifier(Vec3.Zero, 1.0);
        Assert.True(System.Math.Abs(cls.DistanceToSurface(Vec3.Zero) - 1.0) < 0.01);
    }

    [Fact]
    public void CylinderClassifier_Inside()
    {
        var cls = new CylinderPointClassifier(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0);
        Assert.Equal(SolidClassification.Inside, cls.Classify(Vec3.Zero));
    }

    [Fact]
    public void CylinderClassifier_Outside()
    {
        var cls = new CylinderPointClassifier(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0);
        Assert.Equal(SolidClassification.Outside, cls.Classify(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void PlaneClassifier_PositiveSide()
    {
        var plane = Plane.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var cls = new PlanePointClassifier(plane);
        Assert.Equal(SolidClassification.Inside, cls.Classify(new Vec3(0, 0, 1)));
    }

    [Fact]
    public void PlaneClassifier_NegativeSide()
    {
        var plane = Plane.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var cls = new PlanePointClassifier(plane);
        Assert.Equal(SolidClassification.Outside, cls.Classify(new Vec3(0, 0, -1)));
    }

    [Fact]
    public void RayCastVsWinding_AgreeOnCube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rayCast = new BvhPointClassifier(cube.Bvh, useWindingNumber: false);
        var winding = new BvhPointClassifier(cube.Bvh, useWindingNumber: true);

        var testPoints = new[]
        {
            Vec3.Zero, new Vec3(0.5, 0.5, 0.5),
            new Vec3(5, 0, 0), new Vec3(0, 5, 0),
        };
        foreach (var p in testPoints)
            Assert.Equal(rayCast.Classify(p), winding.Classify(p));
    }

    [Fact]
    public void RayCastVsWinding_AgreeOnSphere()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var rayCast = new BvhPointClassifier(sphere.Bvh, useWindingNumber: false);
        var winding = new BvhPointClassifier(sphere.Bvh, useWindingNumber: true);

        var testPoints = new[]
        {
            Vec3.Zero, new Vec3(0.5, 0, 0),
            new Vec3(5, 0, 0), new Vec3(-5, 0, 0),
        };
        foreach (var p in testPoints)
            Assert.Equal(rayCast.Classify(p), winding.Classify(p));
    }

    [Fact]
    public void CsgWithWindingNumber_ProducesValidMesh()
    {
        var opts = new CsgOptions { UseWindingNumber = true };
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = Csg.Difference(a, b, opts);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void CsgWithRayCast_ProducesValidMesh()
    {
        var opts = new CsgOptions { UseWindingNumber = false };
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = Csg.Difference(a, b, opts);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void DistanceToSurface_IsNonNegative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cls = new BvhPointClassifier(cube.Bvh);
        var testPoints = new[] { Vec3.Zero, new Vec3(5, 0, 0), new Vec3(0.5, 0.5, 0.5) };
        foreach (var p in testPoints)
            Assert.True(cls.DistanceToSurface(p) >= 0);
    }

    [Fact]
    public void SphereClassifier_BoundaryPoint()
    {
        var cls = new SpherePointClassifier(Vec3.Zero, 1.0);
        double dist = cls.DistanceToSurface(new Vec3(1, 0, 0));
        Assert.True(dist < 0.01);
    }

    [Fact]
    public void CylinderClassifier_BoundaryPoint()
    {
        var cls = new CylinderPointClassifier(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0);
        double dist = cls.DistanceToSurface(new Vec3(1, 0, 0));
        Assert.True(dist < 0.01);
    }

    [Fact]
    public void ClassifierConsistency_MultiplePrimitives()
    {
        var shapes = new[]
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0, 2),
            Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16),
        };
        foreach (var s in shapes)
        {
            var cls = new BvhPointClassifier(s.Bvh);
            Assert.Equal(SolidClassification.Inside, cls.Classify(Vec3.Zero));
            Assert.Equal(SolidClassification.Outside, cls.Classify(new Vec3(10, 0, 0)));
        }
    }
}
