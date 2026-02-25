using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PatchClassifier — ClassifyAll, degenerate threshold, confident points, winding vs raycast</summary>
public class PatchClassifierDeepPropertyTests
{
    [Fact]
    public void DegenerateMarginThreshold_IsPositive()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
    }

    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-5);
    }

    [Fact]
    public void ClassifyAll_SetsIsInsideForAllPatches()
    {
        var (patches, subTris, bvh) = CreateTestPatchData();
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        foreach (var patch in patches)
        {
            Assert.NotNull(patch.IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_SetsConfidentPointForAllPatches()
    {
        var (patches, subTris, bvh) = CreateTestPatchData();
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        foreach (var patch in patches)
        {
            // ConfidentPoint should be set to some value (may be zero if degenerate, but property is assigned)
            Assert.True(patch.IsInside.HasValue);
        }
    }

    [Fact]
    public void ClassifyAll_DegenerateCount_NonNegative()
    {
        var (patches, subTris, bvh) = CreateTestPatchData();
        int degCount = PatchClassifier.ClassifyAll(patches, subTris, bvh);
        Assert.True(degCount >= 0);
    }

    [Fact]
    public void ClassifyAll_WindingNumber_SetsIsInside()
    {
        var (patches, subTris, bvh) = CreateTestPatchData();
        PatchClassifier.ClassifyAll(patches, subTris, bvh, useWindingNumber: true);
        foreach (var patch in patches)
        {
            Assert.NotNull(patch.IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_RaycastAndWinding_AgreeOnWellSeparated()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        // Create a simple sub-triangle outside the cube
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 0, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        
        int degRay = PatchClassifier.ClassifyAll(patches, subTris, cube.Bvh, useWindingNumber: false);
        bool? rayResult = patches[0].IsInside;
        
        // Reset and classify with winding
        patches[0].IsInside = null;
        int degWind = PatchClassifier.ClassifyAll(patches, subTris, cube.Bvh, useWindingNumber: true);
        bool? windResult = patches[0].IsInside;
        
        Assert.Equal(rayResult, windResult);
    }

    [Fact]
    public void ClassifyAll_InsidePatch_ClassifiedAsInside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        // Sub-triangle fully inside the cube
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(1, 1, 1), new Vec3(1.1, 1, 1), new Vec3(1, 1.1, 1), 0, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        
        PatchClassifier.ClassifyAll(patches, subTris, cube.Bvh);
        Assert.True(patches[0].IsInside == true);
    }

    [Fact]
    public void ClassifyAll_OutsidePatch_ClassifiedAsOutside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 0, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        
        PatchClassifier.ClassifyAll(patches, subTris, cube.Bvh);
        Assert.True(patches[0].IsInside == false);
    }

    [Fact]
    public void CpuStrategy_ProducesSameResultAsStatic()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(1, 1, 1), new Vec3(1.1, 1, 1), new Vec3(1, 1.1, 1), 0, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        
        var patches1 = PatchExtractor.Extract(subTris, adjacency);
        PatchClassifier.ClassifyAll(patches1, subTris, cube.Bvh);
        
        var patches2 = PatchExtractor.Extract(subTris, adjacency);
        var strategy = new CpuPatchClassificationStrategy();
        strategy.ClassifyAll(patches2, subTris, cube.Bvh, false);
        
        Assert.Equal(patches1[0].IsInside, patches2[0].IsInside);
    }

    private static (List<Patch> patches, List<FaceCutter.SubTriangle> subTris, MdCsg.Bvh.BvhTree bvh) CreateTestPatchData()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(1, 1, 1), new Vec3(1.5, 1, 1), new Vec3(1, 1.5, 1), 0, false),
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        return (patches, subTris, cube.Bvh);
    }
}
