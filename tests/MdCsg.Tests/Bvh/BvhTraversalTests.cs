using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH dual-tree traversal tests — overlapping pair detection, disjoint, contained</summary>
public class BvhTraversalTests
{
    [Fact]
    public void FindOverlappingPairs_OverlappingCubes_HasPairs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_DisjointCubes_NoPairs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FindOverlappingPairs_SamePosition_AllOverlap()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        // All faces overlap when cubes are at same position
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_PartialOverlap_HasPairs()
    {
        // Partial overlap ensures face AABBs actually overlap
        var meshA = MeshFactory.CreateCube(size: 2).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(1, 1, 1), size: 2).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_FaceIndicesValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        foreach (var (faceA, faceB) in pairs)
        {
            Assert.True(faceA >= 0 && faceA < meshA.Faces.Count);
            Assert.True(faceB >= 0 && faceB < meshB.Faces.Count);
        }
    }

    [Fact]
    public void FindOverlappingPairs_SphereCube_HasPairs()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_Symmetric()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairsAB = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        var pairsBA = BvhTraversal.FindOverlappingPairs(bvhB, bvhA);
        // Pair counts should match (A,B) <-> (B,A)
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void FindOverlappingPairs_TouchingFaces_HasPairs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        // Touching at a face → AABB overlap at boundary
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_MoreOverlap_MorePairs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var bvhA = BvhTree.Build(meshA);

        var meshB1 = MeshFactory.CreateCube(new Vec3(0.9, 0, 0)).Mesh;
        var meshB2 = MeshFactory.CreateCube(new Vec3(0.3, 0, 0)).Mesh;

        var pairs1 = BvhTraversal.FindOverlappingPairs(bvhA, BvhTree.Build(meshB1));
        var pairs2 = BvhTraversal.FindOverlappingPairs(bvhA, BvhTree.Build(meshB2));

        // More overlap → at least as many overlapping pairs
        Assert.True(pairs2.Count >= pairs1.Count,
            $"More overlap should mean more pairs: small={pairs1.Count}, large={pairs2.Count}");
    }
}
