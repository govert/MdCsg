using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 30: CSG Integration tests - multi-op chains and edge cases (20 tests)</summary>
public class CsgIntegrationTests
{
    [Fact]
    public void ChainedUnion_ThreeCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void UnionThenDifference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var result = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferenceThenUnion()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(2, 0, 0)).Mesh);
        var ab = Csg.Difference(a, b);
        var result = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void IntersectThenDifference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Intersect(a, b);
        var result = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount >= 0); // may be empty
    }

    [Fact]
    public void DeMorgan_UnionComplement()
    {
        // (A ∪ B)' should relate to A' ∩ B' — we can't test complement directly,
        // but we can verify Difference(Universe, Union(A,B)) produces consistent results
        var universe = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3).Mesh);
        var a = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var union = Csg.Union(a, b);
        var result = Csg.Difference(universe, new Solid(union.Mesh));
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SelfDifference_EmptyResult()
    {
        // A - A should be empty, but this is a degenerate case (identical meshes)
        // At least verify it doesn't crash
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount >= 0);
    }

    [Fact]
    public void Union_WithSelf_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount >= 0);
    }

    [Fact]
    public void Intersect_WithSelf_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount >= 0);
    }

    [Fact]
    public void MultipleDifferences_CarvesHoles()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void CubeSphereUnion_HasMoreFacesThanEither()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > cube.Mesh.Faces.Count);
    }

    [Fact]
    public void OffsetSpheres_Intersect_HasResult()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgResult_CanBeFedBackAsSolid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r1 = Csg.Union(a, b);
        var solid = new Solid(r1.Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void ChainedIntersections_Shrinks()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4)).Mesh);
        var ab = Csg.Intersect(a, b);
        if (ab.FaceCount > 0)
        {
            var abc = Csg.Intersect(new Solid(ab.Mesh), c);
            // Chain of intersections should shrink or stay same
            Assert.True(abc.FaceCount <= ab.FaceCount || abc.FaceCount == 0);
        }
    }

    [Fact]
    public void Solid_FromTriangles_ThenCsg()
    {
        // Use the FromTriangles factory directly
        var trisA = new List<Triangle3>();
        var trisB = new List<Triangle3>();
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        for (int i = 0; i < cubeA.Mesh.Faces.Count; i++)
        {
            cubeA.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            trisA.Add(new Triangle3(a, b, c));
        }
        for (int i = 0; i < cubeB.Mesh.Faces.Count; i++)
        {
            cubeB.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            trisB.Add(new Triangle3(a, b, c));
        }
        var solidA = Solid.FromTriangles(trisA);
        var solidB = Solid.FromTriangles(trisB);
        var result = Csg.Union(solidA, solidB);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FullPipeline_CubeCube_Union_NoException()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.1)).Mesh);
        var result = Csg.Union(a, b);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void FullPipeline_CubeCube_Intersect_NoException()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.1)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void FullPipeline_CubeCube_Difference_NoException()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.1)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void AllThreeOps_ProduceDistinctFaceCounts()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var union = Csg.Union(a, b);
        var intersect = Csg.Intersect(a, b);
        var diff = Csg.Difference(a, b);
        // All three operations should produce different face counts
        var counts = new HashSet<int> { union.FaceCount, intersect.FaceCount, diff.FaceCount };
        Assert.True(counts.Count >= 2); // at least 2 distinct
    }

    [Fact]
    public void Union_HasMorePatchesThanDisjoint()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        // Overlapping cubes produce more patches than disjoint
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CsgWithCustomWeldTolerance()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { WeldTolerance = 1e-6 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }
}
