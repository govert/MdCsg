using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler — Assemble with different operations, normal flipping, coplanar handling</summary>
public class PatchAssemblerPropertyTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int faceIdx = 0)
        => new(a, b, c, faceIdx, false);

    [Fact]
    public void Union_KeepsOutsideA_OutsideB()
    {
        var patchA = new Patch(0) { IsInside = false };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = false };
        patchB.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var subB = new[] { MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subA, subB, CsgOperation.Union);

        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Union_DiscardsInsideA_InsideB()
    {
        var patchA = new Patch(0) { IsInside = true };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = true };
        patchB.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var subB = new[] { MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subA, subB, CsgOperation.Union);

        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Intersection_KeepsInsideA_InsideB()
    {
        var patchA = new Patch(0) { IsInside = true };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = true };
        patchB.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var subB = new[] { MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subA, subB, CsgOperation.Intersection);

        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Intersection_DiscardsOutside()
    {
        var patchA = new Patch(0) { IsInside = false };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = false };
        patchB.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var subB = new[] { MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subA, subB, CsgOperation.Intersection);

        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Difference_KeepsOutsideA_InsideB()
    {
        var patchA = new Patch(0) { IsInside = false };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = true };
        patchB.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var subB = new[] { MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subA, subB, CsgOperation.Difference);

        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Difference_BPatchFlipsNormals()
    {
        var patchB = new Patch(0) { IsInside = true };
        patchB.SubTriangleIndices.Add(0);

        var subB = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };

        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, Array.Empty<FaceCutter.SubTriangle>(), subB, CsgOperation.Difference);

        Assert.Single(result.Triangles);
        Assert.True(result.FlipNormals[0]);
        // Flipped winding: A, C, B instead of A, B, C
        var tri = result.Triangles[0];
        Assert.Equal(new Vec3(0, 1, 0), tri.B);
        Assert.Equal(new Vec3(1, 0, 0), tri.C);
    }

    [Fact]
    public void Union_NoFlipNormals()
    {
        var patchA = new Patch(0) { IsInside = false };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = false };
        patchB.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var subB = new[] { MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subA, subB, CsgOperation.Union);

        Assert.All(result.FlipNormals, f => Assert.False(f));
    }

    [Fact]
    public void Assemble_EmptyPatches_EmptyResult()
    {
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), Array.Empty<Patch>(),
            Array.Empty<FaceCutter.SubTriangle>(), Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);

        Assert.Empty(result.Triangles);
        Assert.Empty(result.FlipNormals);
    }

    [Fact]
    public void Assemble_MultiplePatchesMixed()
    {
        var pA1 = new Patch(0) { IsInside = false };
        pA1.SubTriangleIndices.Add(0);
        var pA2 = new Patch(1) { IsInside = true };
        pA2.SubTriangleIndices.Add(1);

        var subA = new[] {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0))
        };

        var result = PatchAssembler.Assemble(
            new[] { pA1, pA2 }, Array.Empty<Patch>(), subA, Array.Empty<FaceCutter.SubTriangle>(), CsgOperation.Union);

        Assert.Single(result.Triangles);
    }

    [Fact]
    public void AssemblyResult_TrianglesAndFlipNormalsMatchCount()
    {
        var patchA = new Patch(0) { IsInside = false };
        patchA.SubTriangleIndices.Add(0);
        patchA.SubTriangleIndices.Add(1);

        var subA = new[] {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0))
        };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subA, Array.Empty<FaceCutter.SubTriangle>(), CsgOperation.Union);

        Assert.Equal(result.Triangles.Count, result.FlipNormals.Count);
    }

    [Fact]
    public void CoplanarPatch_SameNormal_KeptForUnion()
    {
        var patch = new Patch(0) { CoplanarNormalsAgree = true };
        patch.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patch }, Array.Empty<Patch>(), subA, Array.Empty<FaceCutter.SubTriangle>(), CsgOperation.Union);

        Assert.Single(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_OppositeNormal_DiscardedForUnion()
    {
        var patch = new Patch(0) { CoplanarNormalsAgree = false };
        patch.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patch }, Array.Empty<Patch>(), subA, Array.Empty<FaceCutter.SubTriangle>(), CsgOperation.Union);

        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_SameNormal_DiscardedForDifference()
    {
        var patch = new Patch(0) { CoplanarNormalsAgree = true };
        patch.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patch }, Array.Empty<Patch>(), subA, Array.Empty<FaceCutter.SubTriangle>(), CsgOperation.Difference);

        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_OppositeNormal_KeptForDifference()
    {
        var patch = new Patch(0) { CoplanarNormalsAgree = false };
        patch.SubTriangleIndices.Add(0);

        var subA = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };

        var result = PatchAssembler.Assemble(
            new[] { patch }, Array.Empty<Patch>(), subA, Array.Empty<FaceCutter.SubTriangle>(), CsgOperation.Difference);

        Assert.Single(result.Triangles);
    }

    [Fact]
    public void CoplanarPatchFromB_AlwaysDiscarded()
    {
        var patchB = new Patch(0) { CoplanarNormalsAgree = true };
        patchB.SubTriangleIndices.Add(0);

        var subB = new[] { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };

        var unionResult = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, Array.Empty<FaceCutter.SubTriangle>(), subB, CsgOperation.Union);
        var intResult = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, Array.Empty<FaceCutter.SubTriangle>(), subB, CsgOperation.Intersection);
        var diffResult = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, Array.Empty<FaceCutter.SubTriangle>(), subB, CsgOperation.Difference);

        Assert.Empty(unionResult.Triangles);
        Assert.Empty(intResult.Triangles);
        Assert.Empty(diffResult.Triangles);
    }
}
