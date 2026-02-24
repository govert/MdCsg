using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Batch 24: PatchAssembler tests (20 tests)</summary>
public class PatchAssemblerTests
{
    private static (List<Patch> PatchesA, List<Patch> PatchesB, List<FaceCutter.SubTriangle> SubTrisA, List<FaceCutter.SubTriangle> SubTrisB) MakeTestData()
    {
        var stA = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),  // idx 0 - outside B
            new(new Vec3(0.1, 0.1, 0), new Vec3(0.4, 0.1, 0), new Vec3(0.2, 0.4, 0), 1, false),  // idx 1 - inside B
        };
        var stB = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0), 0, false),  // idx 0 - outside A
            new(new Vec3(0.2, 0.2, 0), new Vec3(0.3, 0.2, 0), new Vec3(0.25, 0.3, 0), 1, false), // idx 1 - inside A
        };

        var patchA_outside = new Patch(0);
        patchA_outside.SubTriangleIndices.Add(0);
        patchA_outside.IsInside = false;
        patchA_outside.HasConfidentPoint = true;

        var patchA_inside = new Patch(1);
        patchA_inside.SubTriangleIndices.Add(1);
        patchA_inside.IsInside = true;
        patchA_inside.HasConfidentPoint = true;

        var patchB_outside = new Patch(0);
        patchB_outside.SubTriangleIndices.Add(0);
        patchB_outside.IsInside = false;
        patchB_outside.HasConfidentPoint = true;

        var patchB_inside = new Patch(1);
        patchB_inside.SubTriangleIndices.Add(1);
        patchB_inside.IsInside = true;
        patchB_inside.HasConfidentPoint = true;

        return (new List<Patch> { patchA_outside, patchA_inside },
                new List<Patch> { patchB_outside, patchB_inside },
                stA, stB);
    }

    [Fact]
    public void Union_KeepsAOutsideB()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        // A outside B should be kept
        Assert.True(result.Triangles.Count >= 1);
    }

    [Fact]
    public void Union_KeepsBOutsideA()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        // Should keep A-outside (0) and B-outside (0) = 2 total
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Union_DoesNotKeepInsideParts()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        // Inside patches from A and B should be excluded
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Intersection_KeepsAInsideB()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        // A inside B (1) and B inside A (1) = 2 total
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Intersection_DoesNotKeepOutsideParts()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        // Only inside patches kept
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Difference_KeepsAOutsideB()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // A outside B (1) + B inside A with flipped normals (1) = 2 total
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Difference_FlipsBInsideNormals()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // B's inside-A patch should have flipped normals
        Assert.True(result.FlipNormals.Any(f => f));
    }

    [Fact]
    public void Union_NoFlippedNormals()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        Assert.True(result.FlipNormals.All(f => !f));
    }

    [Fact]
    public void Intersection_NoFlippedNormals()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.True(result.FlipNormals.All(f => !f));
    }

    [Fact]
    public void EmptyPatches_EmptyResult()
    {
        var result = PatchAssembler.Assemble(
            new List<Patch>(), new List<Patch>(),
            new List<FaceCutter.SubTriangle>(), new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_SameNormal_KeptForUnion()
    {
        var st = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.CoplanarNormalsAgree = true;
        patch.HasConfidentPoint = false;
        patch.IsInside = false;

        var result = PatchAssembler.Assemble(
            new List<Patch> { patch }, new List<Patch>(),
            st, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_OppositeNormal_DiscardedForUnion()
    {
        var st = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.CoplanarNormalsAgree = false;
        patch.HasConfidentPoint = false;

        var result = PatchAssembler.Assemble(
            new List<Patch> { patch }, new List<Patch>(),
            st, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_OppositeNormal_KeptForDifference()
    {
        var st = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.CoplanarNormalsAgree = false;
        patch.HasConfidentPoint = false;

        var result = PatchAssembler.Assemble(
            new List<Patch> { patch }, new List<Patch>(),
            st, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_SameNormal_DiscardedForDifference()
    {
        var st = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.CoplanarNormalsAgree = true;
        patch.HasConfidentPoint = false;

        var result = PatchAssembler.Assemble(
            new List<Patch> { patch }, new List<Patch>(),
            st, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatchB_AlwaysDiscarded()
    {
        var stB = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var patchB = new Patch(0);
        patchB.SubTriangleIndices.Add(0);
        patchB.CoplanarNormalsAgree = true;
        patchB.HasConfidentPoint = false;

        var result = PatchAssembler.Assemble(
            new List<Patch>(), new List<Patch> { patchB },
            new List<FaceCutter.SubTriangle>(), stB,
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void AssemblyResult_TrianglesAndFlipNormals_SameLength()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        Assert.Equal(result.Triangles.Count, result.FlipNormals.Count);
    }

    [Fact]
    public void CsgOperation_HasThreeValues()
    {
        Assert.Equal(0, (int)CsgOperation.Union);
        Assert.Equal(1, (int)CsgOperation.Intersection);
        Assert.Equal(2, (int)CsgOperation.Difference);
    }

    [Fact]
    public void Difference_FlippedTriangle_HasReversedWinding()
    {
        var (pA, pB, stA, stB) = MakeTestData();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // Find the flipped triangle from B
        for (int i = 0; i < result.FlipNormals.Count; i++)
        {
            if (result.FlipNormals[i])
            {
                // Original B[1] = (0.2,0.2,0), (0.3,0.2,0), (0.25,0.3,0)
                // Flipped = (0.2,0.2,0), (0.25,0.3,0), (0.3,0.2,0) - B and C swapped
                Assert.Equal(stB[1].A, result.Triangles[i].A);
                Assert.Equal(stB[1].C, result.Triangles[i].B);
                Assert.Equal(stB[1].B, result.Triangles[i].C);
            }
        }
    }

    [Fact]
    public void CoplanarPatch_SameNormal_KeptForIntersection()
    {
        var st = new List<FaceCutter.SubTriangle>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.CoplanarNormalsAgree = true;
        patch.HasConfidentPoint = false;

        var result = PatchAssembler.Assemble(
            new List<Patch> { patch }, new List<Patch>(),
            st, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }
}
