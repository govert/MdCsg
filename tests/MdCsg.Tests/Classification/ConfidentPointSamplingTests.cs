using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Classification;

public class ConfidentPointSamplingTests
{
    [Fact]
    public void FindConfidentPoint_LargePatch_UsesBoundedDeterministicSampling()
    {
        var patch = new Patch(0);
        var subs = BuildSubTriangles(count: 500, patch);

        var classifierA = new CountingClassifier();
        var (pointA, marginA) = ConfidentPoint.FindConfidentPoint(patch, subs, classifierA);

        var classifierB = new CountingClassifier();
        var (pointB, marginB) = ConfidentPoint.FindConfidentPoint(patch, subs, classifierB);

        Assert.InRange(classifierA.DistanceCallCount, 1, 65);
        Assert.Equal(classifierA.DistanceCallCount, classifierB.DistanceCallCount);
        Assert.Equal(pointA, pointB);
        Assert.Equal(marginA, marginB);
    }

    [Fact]
    public void FindConfidentPoint_SmallPatch_EvaluatesAllSubTriangles()
    {
        var patch = new Patch(0);
        var subs = BuildSubTriangles(count: 12, patch);
        var classifier = new CountingClassifier();

        _ = ConfidentPoint.FindConfidentPoint(patch, subs, classifier);

        Assert.Equal(12, classifier.DistanceCallCount);
    }

    private static List<FaceCutter.SubTriangle> BuildSubTriangles(int count, Patch patch)
    {
        var subs = new List<FaceCutter.SubTriangle>(count);
        for (int i = 0; i < count; i++)
        {
            double x = i * 0.01;
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(x, 0, 0),
                new Vec3(x + 0.005, 0.01, 0),
                new Vec3(x + 0.01, 0, 0),
                OriginalFaceIndex: 0,
                HasIntersectionEdge: false));
            patch.SubTriangleIndices.Add(i);
        }

        return subs;
    }

    private sealed class CountingClassifier : IPointClassifier
    {
        public int ClassifyCallCount { get; private set; }
        public int DistanceCallCount { get; private set; }

        public SolidClassification Classify(Vec3 point)
        {
            ClassifyCallCount++;
            return SolidClassification.Outside;
        }

        public double DistanceToSurface(Vec3 point)
        {
            DistanceCallCount++;
            return point.X;
        }
    }
}
