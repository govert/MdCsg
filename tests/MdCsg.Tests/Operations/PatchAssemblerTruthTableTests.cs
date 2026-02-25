using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler decision function truth tables — exhaustive keep/flip/coplanar logic</summary>
public class PatchAssemblerTruthTableTests
{
    // Helper to create sub-triangles
    private static List<FaceCutter.SubTriangle> MakeSubs(int count)
    {
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < count; i++)
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(i, 0, 0), new Vec3(i + 1, 0, 0), new Vec3(i, 1, 0), 0, false, 0));
        return subs;
    }

    private static Patch MakePatch(int id, int subIndex, bool? isInside, bool? coplanarNormalsAgree = null, bool hasConfidentPoint = true)
    {
        var p = new Patch(id);
        p.SubTriangleIndices.Add(subIndex);
        p.IsInside = isInside;
        p.CoplanarNormalsAgree = coplanarNormalsAgree;
        p.HasConfidentPoint = hasConfidentPoint;
        return p;
    }

    // ===== ShouldKeepPatchFromA truth table =====

    [Fact]
    public void A_Union_Inside_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void A_Union_Outside_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void A_Intersection_Inside_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Intersection);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void A_Intersection_Outside_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Intersection);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void A_Difference_Inside_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Difference);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void A_Difference_Outside_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    // ===== ShouldKeepPatchFromB truth table =====

    [Fact]
    public void B_Union_Inside_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void B_Union_Outside_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void B_Intersection_Inside_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Intersection);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void B_Intersection_Outside_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Intersection);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void B_Difference_Inside_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void B_Difference_Outside_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Difference);
        Assert.Equal(0, result.Triangles.Count);
    }

    // ===== ShouldFlipNormalsFromB truth table =====

    [Fact]
    public void B_Union_Outside_NotFlipped()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Union);
        Assert.Equal(1, result.FlipNormals.Count);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void B_Intersection_Inside_NotFlipped()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Intersection);
        Assert.Equal(1, result.FlipNormals.Count);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void B_Difference_Inside_Flipped()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Difference);
        Assert.Equal(1, result.FlipNormals.Count);
        Assert.True(result.FlipNormals[0]);
    }

    [Fact]
    public void B_Difference_Flipped_WindingReversed()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var subs = new List<FaceCutter.SubTriangle> { new(a, b, c, 0, false, 0) };
        var patch = MakePatch(0, 0, isInside: true);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Difference);
        // Flipped winding: A, C, B (not A, B, C)
        Assert.Equal(a, result.Triangles[0].A);
        Assert.Equal(c, result.Triangles[0].B);
        Assert.Equal(b, result.Triangles[0].C);
    }

    // ===== ShouldKeepCoplanarPatchFromA truth table =====

    [Fact]
    public void A_Coplanar_SameNormal_Union_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: true, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void A_Coplanar_OppositeNormal_Union_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: false, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void A_Coplanar_SameNormal_Intersection_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: true, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Intersection);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void A_Coplanar_OppositeNormal_Intersection_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: false, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Intersection);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void A_Coplanar_SameNormal_Difference_Discarded()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: true, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Difference);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void A_Coplanar_OppositeNormal_Difference_Kept()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: false, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    // ===== Coplanar B patches always discarded =====

    [Fact]
    public void B_Coplanar_SameNormal_AlwaysDiscarded_Union()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: true, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void B_Coplanar_OppositeNormal_AlwaysDiscarded_Intersection()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: false, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Intersection);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void B_Coplanar_SameNormal_AlwaysDiscarded_Difference()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: true, coplanarNormalsAgree: true, hasConfidentPoint: false);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Difference);
        Assert.Equal(0, result.Triangles.Count);
    }

    // ===== Coplanar with HasConfidentPoint falls back to normal path =====

    [Fact]
    public void A_Coplanar_WithConfidentPoint_UsesNormalPath()
    {
        var subs = MakeSubs(1);
        // HasConfidentPoint=true means coplanar override is bypassed, uses normal IsInside logic
        var patch = MakePatch(0, 0, isInside: true, coplanarNormalsAgree: true, hasConfidentPoint: true);
        // Union + Inside = discarded (normal path)
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void B_Coplanar_WithConfidentPoint_UsesNormalPath()
    {
        var subs = MakeSubs(1);
        // HasConfidentPoint=true means coplanar override is bypassed
        var patch = MakePatch(0, 0, isInside: false, coplanarNormalsAgree: true, hasConfidentPoint: true);
        // Union + Outside = kept (normal B path)
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    // ===== Multiple patches mixed =====

    [Fact]
    public void Multiple_A_Patches_Mixed_InsideOutside()
    {
        var subs = MakeSubs(3);
        var pInside = MakePatch(0, 0, isInside: true);
        var pOutside = MakePatch(1, 1, isInside: false);
        var pOutside2 = MakePatch(2, 2, isInside: false);
        var result = PatchAssembler.Assemble(new[] { pInside, pOutside, pOutside2 }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(2, result.Triangles.Count); // Only outside kept
    }

    [Fact]
    public void Empty_Patches_ProducesEmptyResult()
    {
        var subs = MakeSubs(1);
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
        Assert.Equal(0, result.FlipNormals.Count);
    }

    [Fact]
    public void A_NullIsInside_TreatedAsFalse()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: null); // null coalesces to false
        // Union + Outside(false) = kept
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void B_NullIsInside_TreatedAsFalse()
    {
        var subs = MakeSubs(1);
        var patch = MakePatch(0, 0, isInside: null); // null coalesces to false
        // Difference + Outside(false) = discarded
        var result = PatchAssembler.Assemble(Array.Empty<Patch>(), new[] { patch }, subs, subs, CsgOperation.Difference);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void MultiSubTriangle_Patch_AllIncluded()
    {
        var subs = MakeSubs(5);
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(1);
        patch.SubTriangleIndices.Add(2);
        patch.SubTriangleIndices.Add(3);
        patch.SubTriangleIndices.Add(4);
        patch.IsInside = false;
        patch.HasConfidentPoint = true;
        var result = PatchAssembler.Assemble(new[] { patch }, Array.Empty<Patch>(), subs, subs, CsgOperation.Union);
        Assert.Equal(5, result.Triangles.Count);
        Assert.All(result.FlipNormals, flip => Assert.False(flip));
    }

    [Fact]
    public void TrianglesAndFlipNormals_SameLength()
    {
        var subs = MakeSubs(3);
        var pA = MakePatch(0, 0, isInside: false);
        var pB = MakePatch(1, 1, isInside: true);
        var result = PatchAssembler.Assemble(new[] { pA }, new[] { pB }, subs, subs, CsgOperation.Difference);
        // A outside kept + B inside kept (flipped)
        Assert.Equal(2, result.Triangles.Count);
        Assert.Equal(2, result.FlipNormals.Count);
        Assert.False(result.FlipNormals[0]); // A never flipped
        Assert.True(result.FlipNormals[1]); // B inside difference is flipped
    }
}
