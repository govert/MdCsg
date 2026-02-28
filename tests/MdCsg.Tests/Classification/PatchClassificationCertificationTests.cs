using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Classification;

public class PatchClassificationCertificationTests
{
    [Fact]
    public void ClassifyAll_TracksCertificationAndFallbackCounts()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.3, 0), 2.0);

        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjacency = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjacency);

        int fallbackCount = PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, b.Bvh);

        int uncertifiedCount = patches.Count(p => !p.IsClassificationCertified);
        Assert.Equal(uncertifiedCount, fallbackCount);
        Assert.All(patches, p => Assert.True(p.ClassificationErrorBound > 0));

        foreach (var patch in patches)
        {
            if (patch.IsClassificationCertified)
            {
                Assert.True(patch.HasConfidentPoint);
                Assert.True(patch.ClassificationMargin > patch.ClassificationErrorBound);
            }
            else
            {
                Assert.False(patch.HasConfidentPoint);
                Assert.True(patch.ClassificationMargin <= patch.ClassificationErrorBound);
            }

            Assert.NotNull(patch.IsInside);
        }
    }
}
