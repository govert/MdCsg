using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

public class PlanePointClassifierTests
{
    [Fact]
    public void Classify_PointAbovePlane_Inside()
    {
        var plane = new Plane(Vec3.UnitZ, 0);
        var classifier = new PlanePointClassifier(plane);
        Assert.Equal(SolidClassification.Inside, classifier.Classify(new Vec3(0, 0, 1)));
    }

    [Fact]
    public void Classify_PointBelowPlane_Outside()
    {
        var plane = new Plane(Vec3.UnitZ, 0);
        var classifier = new PlanePointClassifier(plane);
        Assert.Equal(SolidClassification.Outside, classifier.Classify(new Vec3(0, 0, -1)));
    }

    [Fact]
    public void Classify_PointOnPlane_Outside()
    {
        // Exactly zero → not strictly > 0, so Outside
        var plane = new Plane(Vec3.UnitZ, 0);
        var classifier = new PlanePointClassifier(plane);
        Assert.Equal(SolidClassification.Outside, classifier.Classify(Vec3.Zero));
    }

    [Fact]
    public void DistanceToSurface_AlwaysPositive()
    {
        var plane = new Plane(Vec3.UnitZ, 0);
        var classifier = new PlanePointClassifier(plane);

        Assert.True(classifier.DistanceToSurface(new Vec3(0, 0, 3)) > 0);
        Assert.True(classifier.DistanceToSurface(new Vec3(0, 0, -3)) > 0);
    }

    [Fact]
    public void DistanceToSurface_PointOnPlane_Zero()
    {
        var plane = new Plane(Vec3.UnitZ, 0);
        var classifier = new PlanePointClassifier(plane);
        Assert.Equal(0.0, classifier.DistanceToSurface(Vec3.Zero), 1e-12);
    }

    [Fact]
    public void DistanceToSurface_ReturnsCorrectDistance()
    {
        var plane = new Plane(Vec3.UnitZ, 5.0);
        var classifier = new PlanePointClassifier(plane);
        Assert.Equal(3.0, classifier.DistanceToSurface(new Vec3(10, 20, 8)), 1e-12);
        Assert.Equal(7.0, classifier.DistanceToSurface(new Vec3(10, 20, -2)), 1e-12);
    }

    [Fact]
    public void Classify_OffsetPlane()
    {
        // Plane at z=5, normal +Z
        var plane = new Plane(Vec3.UnitZ, 5.0);
        var classifier = new PlanePointClassifier(plane);

        Assert.Equal(SolidClassification.Inside, classifier.Classify(new Vec3(0, 0, 10)));
        Assert.Equal(SolidClassification.Outside, classifier.Classify(new Vec3(0, 0, 0)));
    }
}
