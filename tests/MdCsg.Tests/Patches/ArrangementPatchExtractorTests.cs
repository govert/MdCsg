using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

public class ArrangementPatchExtractorTests
{
    [Fact]
    public void SharedEdgeWithArrangementSegment_SplitsIntoTwoPatches_WithoutIntersectionFlags()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);

        var subTriangles = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 0, false, 0),
            new(b, d, c, 0, false, 0)
        };

        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            [0] =
            [
                new IntersectionSegment(b, c, 0, 0)
            ]
        };

        var patches = ArrangementPatchExtractor.Extract(subTriangles, faceSegments);

        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void SharedEdgeWithoutArrangementSegment_RemainsSinglePatch()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);

        var subTriangles = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 0, false, 0),
            new(b, d, c, 0, false, 0)
        };

        var faceSegments = new Dictionary<int, List<IntersectionSegment>>();
        var patches = ArrangementPatchExtractor.Extract(subTriangles, faceSegments);

        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void ArrangementPatchExtraction_IsDeterministic()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);
        var e = new Vec3(2, 0, 0);
        var f = new Vec3(2, 1, 0);

        var subTriangles = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 0, false, 0),
            new(b, d, c, 0, false, 0),
            new(b, e, d, 0, false, 0),
            new(e, f, d, 0, false, 0)
        };

        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            [0] =
            [
                new IntersectionSegment(b, c, 0, 0),
                new IntersectionSegment(b, d, 0, 0)
            ]
        };

        var first = ArrangementPatchExtractor.Extract(subTriangles, faceSegments);
        var second = ArrangementPatchExtractor.Extract(subTriangles, faceSegments);

        Assert.Equal(first.Count, second.Count);
        for (int i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].Id, second[i].Id);
            Assert.Equal(
                first[i].SubTriangleIndices.OrderBy(static x => x),
                second[i].SubTriangleIndices.OrderBy(static x => x));
        }
    }
}
