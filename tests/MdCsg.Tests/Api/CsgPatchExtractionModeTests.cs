using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class CsgPatchExtractionModeTests
{
    [Fact]
    public void CsgOptions_DefaultPatchExtractionPolicy_IsAutoWithoutTopologyPreference()
    {
        var options = new CsgOptions();

        Assert.Equal(PatchExtractionMode.Auto, options.PatchExtractionMode);
        Assert.False(options.PreferTopologyPreservingPatchExtraction);
    }

    [Fact]
    public void AutoMode_WithIntersectingInputs_UsesIntraFaceWhenTopologyPreferenceIsDisabled()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.3, 0), 2.0);

        var auto = Csg.Union(a, b, new CsgOptions
        {
            PatchExtractionMode = PatchExtractionMode.Auto,
            PreferTopologyPreservingPatchExtraction = false
        });
        var intra = Csg.Union(a, b, new CsgOptions
        {
            PatchExtractionMode = PatchExtractionMode.IntraFace
        });

        Assert.True(auto.IntersectionSegmentCount > 0);
        Assert.Equal(PatchExtractionMode.IntraFace, auto.SelectedPatchExtractionMode);
        Assert.Equal(intra.FaceCount, auto.FaceCount);
        Assert.Equal(intra.PatchCountA, auto.PatchCountA);
        Assert.Equal(intra.PatchCountB, auto.PatchCountB);
    }

    [Fact]
    public void TopologyPreservingAutoSelection_FollowsDeterministicQualityOrder()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = Csg.Intersect(sphere, box, new CsgOptions { Parallel = false });
        var step2 = Csg.Difference(new Solid(step1.Mesh), cylX, new CsgOptions { Parallel = false });
        var step2Solid = new Solid(step2.Mesh);

        var intra = Csg.Difference(step2Solid, cylY, new CsgOptions
        {
            Parallel = false,
            PatchExtractionMode = PatchExtractionMode.IntraFace
        });
        var global = Csg.Difference(step2Solid, cylY, new CsgOptions
        {
            Parallel = false,
            PatchExtractionMode = PatchExtractionMode.Global
        });
        var auto = Csg.Difference(step2Solid, cylY, new CsgOptions
        {
            Parallel = false,
            PatchExtractionMode = PatchExtractionMode.Auto,
            PreferTopologyPreservingPatchExtraction = true
        });

        var expected = IsBetter(global, intra) ? global : intra;
        Assert.Equal(expected.SelectedPatchExtractionMode, auto.SelectedPatchExtractionMode);
        Assert.Equal(expected.SelectedPatchExtractionBoundaryEdgeCount, auto.SelectedPatchExtractionBoundaryEdgeCount);
        Assert.Equal(expected.SelectedPatchExtractionIsEdgeManifold, auto.SelectedPatchExtractionIsEdgeManifold);
        Assert.Equal(expected.SelectedPatchExtractionConnectedComponentCount, auto.SelectedPatchExtractionConnectedComponentCount);
    }

    [Fact]
    public void TopologyPreservingAutoSelection_EmitsDeterministicCandidateSignatures()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = Csg.Intersect(sphere, box, new CsgOptions { Parallel = false });
        var step2 = Csg.Difference(new Solid(step1.Mesh), cylX, new CsgOptions { Parallel = false });
        var step2Solid = new Solid(step2.Mesh);

        var baseline = Csg.Difference(step2Solid, cylY, new CsgOptions
        {
            Parallel = false,
            PatchExtractionMode = PatchExtractionMode.Auto,
            PreferTopologyPreservingPatchExtraction = true
        });

        Assert.Equal(3, baseline.PatchExtractionCandidateSignatures.Count);
        Assert.Contains(baseline.PatchExtractionCandidateSignatures, static s => s.StartsWith("IntraFace:", StringComparison.Ordinal));
        Assert.Contains(baseline.PatchExtractionCandidateSignatures, static s => s.StartsWith("Global:", StringComparison.Ordinal));
        Assert.Contains(baseline.PatchExtractionCandidateSignatures, static s => s.StartsWith("Arrangement:", StringComparison.Ordinal));
        Assert.Contains(
            baseline.PatchExtractionCandidateSignatures,
            s => s.StartsWith($"{baseline.SelectedPatchExtractionMode}:", StringComparison.Ordinal));

        for (int i = 0; i < 3; i++)
        {
            var next = Csg.Difference(step2Solid, cylY, new CsgOptions
            {
                Parallel = false,
                PatchExtractionMode = PatchExtractionMode.Auto,
                PreferTopologyPreservingPatchExtraction = true
            });

            Assert.Equal(baseline.PatchExtractionCandidateSignatures, next.PatchExtractionCandidateSignatures);
        }
    }

    [Fact]
    public void ExplicitArrangementMode_ReportsArrangementSelection()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.3, 0), 2.0);

        var result = Csg.Intersect(a, b, new CsgOptions
        {
            PatchExtractionMode = PatchExtractionMode.Arrangement,
            Parallel = false
        });

        Assert.True(result.IntersectionSegmentCount > 0);
        Assert.Equal(PatchExtractionMode.Arrangement, result.SelectedPatchExtractionMode);
    }

    private static bool IsBetter(CsgResult a, CsgResult b)
    {
        Assert.NotNull(a.SelectedPatchExtractionBoundaryEdgeCount);
        Assert.NotNull(a.SelectedPatchExtractionIsEdgeManifold);
        Assert.NotNull(a.SelectedPatchExtractionConnectedComponentCount);
        Assert.NotNull(b.SelectedPatchExtractionBoundaryEdgeCount);
        Assert.NotNull(b.SelectedPatchExtractionIsEdgeManifold);
        Assert.NotNull(b.SelectedPatchExtractionConnectedComponentCount);

        bool aClosed = a.SelectedPatchExtractionBoundaryEdgeCount == 0
                       && a.SelectedPatchExtractionIsEdgeManifold == true;
        bool bClosed = b.SelectedPatchExtractionBoundaryEdgeCount == 0
                       && b.SelectedPatchExtractionIsEdgeManifold == true;
        if (aClosed != bClosed)
            return aClosed;

        if (a.SelectedPatchExtractionBoundaryEdgeCount != b.SelectedPatchExtractionBoundaryEdgeCount)
            return a.SelectedPatchExtractionBoundaryEdgeCount < b.SelectedPatchExtractionBoundaryEdgeCount;

        if (a.SelectedPatchExtractionIsEdgeManifold != b.SelectedPatchExtractionIsEdgeManifold)
            return a.SelectedPatchExtractionIsEdgeManifold == true;

        if (a.SelectedPatchExtractionConnectedComponentCount != b.SelectedPatchExtractionConnectedComponentCount)
        {
            return a.SelectedPatchExtractionConnectedComponentCount
                   < b.SelectedPatchExtractionConnectedComponentCount;
        }

        return false;
    }
}
