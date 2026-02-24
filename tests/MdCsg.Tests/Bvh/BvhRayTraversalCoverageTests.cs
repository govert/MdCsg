using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Code coverage: BVH ray intersection, query, and traversal edge cases</summary>
public class BvhRayTraversalCoverageTests
{
    [Fact]
    public void RayCastCount_ThroughCube_HitsEvenCount()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        // Perturbed origin avoids hitting triangle edges/vertices
        var ray = new Ray(new Vec3(0.3, 0.4, -1), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0, $"Expected even hits >= 2, got {hits}");
    }

    [Fact]
    public void RayCastCount_MissesCube_Zero()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var ray = new Ray(new Vec3(5, 5, -1), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.Equal(0, hits);
    }

    [Fact]
    public void RayCastCount_InsidePoint_OddHits()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 1, $"Inside point should have odd hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_OutsidePoint_EvenHits()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var ray = new Ray(new Vec3(5, 5, 5), new Vec3(1, 0.00013, 0.00017));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0, $"Outside point should have even hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_ThroughSphere()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var ray = new Ray(new Vec3(0, 0, -5), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0);
    }

    [Fact]
    public void RayCastCount_MissSphere()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var ray = new Ray(new Vec3(5, 0, -5), new Vec3(0, 0, 1));
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCastCount_EmptyMesh()
    {
        var bvh = BvhTree.Build(new HalfEdgeMesh());
        var ray = new Ray(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void Query_OverlappingBox_FindsFaces()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(0.4, 0.4, 0.4), new Vec3(0.6, 0.6, 0.6)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void Query_DisjointBox_FindsNothing()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void Query_FullEnclosingBox_FindsAllFaces()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        Assert.Equal(12, results.Count);
    }

    [Fact]
    public void Query_EmptyMesh()
    {
        var bvh = BvhTree.Build(new HalfEdgeMesh());
        var results = new List<int>();
        bvh.Query(new Aabb(Vec3.Zero, Vec3.UnitX), results);
        Assert.Empty(results);
    }

    [Fact]
    public void FindOverlappingPairs_OverlappingCubes()
    {
        var bvhA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var bvhB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_DisjointCubes_Empty()
    {
        var bvhA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var bvhB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FindOverlappingPairs_Symmetric()
    {
        var bvhA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var bvhB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var pairsAB = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        var pairsBA = BvhTraversal.FindOverlappingPairs(bvhB, bvhA);
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void GetFaceIndex_ValidIndices()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        for (int i = 0; i < bvh.FaceIndices.Length; i++)
        {
            int faceIdx = bvh.GetFaceIndex(i);
            Assert.True(faceIdx >= 0 && faceIdx < 12);
        }
    }

    [Fact]
    public void RayCastCount_PerturbedDirections()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var dirs = new[]
        {
            new Vec3(1, 0.00013, 0.00017),
            new Vec3(0.00019, 1, 0.00023),
            new Vec3(0.00029, 0.00031, 1),
        };
        foreach (var dir in dirs)
        {
            var ray = new Ray(new Vec3(0.5, 0.5, 0.5), dir);
            int hits = bvh.RayCastCount(ray);
            Assert.True(hits % 2 == 1, $"Inside cube with dir {dir}: {hits} hits");
        }
    }

    [Fact]
    public void RayCastCount_NegativeDirection()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        // Perturbed origin to avoid edge/vertex hits
        var ray = new Ray(new Vec3(0.3, 0.4, 2), new Vec3(0, 0, -1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0, $"Expected even hits >= 2, got {hits}");
    }

    [Fact]
    public void Query_PartialOverlap_FindsSome()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(0.8, 0.8, 0.8), new Vec3(1.2, 1.2, 1.2)), results);
        Assert.True(results.Count > 0 && results.Count < 12);
    }
}
