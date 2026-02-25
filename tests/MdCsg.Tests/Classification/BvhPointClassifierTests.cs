using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

public class BvhPointClassifierTests
{
    [Fact]
    public void Classify_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh);

        Assert.Equal(SolidClassification.Inside, classifier.Classify(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Classify_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh);

        Assert.Equal(SolidClassification.Outside, classifier.Classify(new Vec3(5, 5, 5)));
    }

    [Fact]
    public void Classify_MatchesRayCastClassifier()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh, useWindingNumber: false);

        var testPoints = new[]
        {
            new Vec3(0.5, 0.5, 0.5),
            new Vec3(2, 2, 2),
            new Vec3(0.1, 0.1, 0.1),
            new Vec3(-1, -1, -1),
        };

        foreach (var p in testPoints)
        {
            var expected = RayCastClassifier.Classify(p, cube.Bvh);
            Assert.Equal(expected, classifier.Classify(p));
        }
    }

    [Fact]
    public void Classify_WithWindingNumber_MatchesWindingClassifier()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh, useWindingNumber: true);

        var inside = new Vec3(0.5, 0.5, 0.5);
        var outside = new Vec3(5, 5, 5);

        Assert.Equal(WindingNumberClassifier.Classify(inside, cube.Bvh), classifier.Classify(inside));
        Assert.Equal(WindingNumberClassifier.Classify(outside, cube.Bvh), classifier.Classify(outside));
    }

    [Fact]
    public void DistanceToSurface_InsideCube_Positive()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh);

        double dist = classifier.DistanceToSurface(new Vec3(0.5, 0.5, 0.5));
        Assert.True(dist > 0);
    }

    [Fact]
    public void DistanceToSurface_FarAway_Positive()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh);

        double dist = classifier.DistanceToSurface(new Vec3(10, 10, 10));
        Assert.True(dist > 0);
    }

    [Fact]
    public void DistanceToSurface_CenterOfCube_ApproxHalf()
    {
        var cube = MeshFactory.CreateCube();
        var classifier = new BvhPointClassifier(cube.Bvh);

        // Center of unit cube [0,1]^3 is (0.5, 0.5, 0.5), distance to nearest face = 0.5
        double dist = classifier.DistanceToSurface(new Vec3(0.5, 0.5, 0.5));
        Assert.Equal(0.5, dist, 1e-6);
    }
}
