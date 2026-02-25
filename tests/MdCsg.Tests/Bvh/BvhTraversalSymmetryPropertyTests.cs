using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal — pair symmetry, self-overlap, empty tree, transitivity</summary>
public class BvhTraversalSymmetryPropertyTests
{
    [Fact]
    public void OverlappingPairs_Symmetric()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh);

        var pairsAB = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        var pairsBa = BvhTraversal.FindOverlappingPairs(treeB, treeA);

        // Same number of pairs (with swapped indices)
        Assert.Equal(pairsAB.Count, pairsBa.Count);

        // Every (a,b) in AB should have corresponding (b,a) in BA
        var setAB = new HashSet<(int, int)>(pairsAB);
        var setBA = new HashSet<(int, int)>(pairsBa);
        foreach (var (a, b) in setAB)
            Assert.True(setBA.Contains((b, a)), $"Pair ({a},{b}) missing as ({b},{a}) in reverse");
    }

    [Fact]
    public void DisjointMeshes_NoPairs()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void SelfOverlap_AllPairsValid()
    {
        var tree = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(tree, tree);
        // Self-overlap should find overlapping face pairs
        Assert.True(pairs.Count > 0, "Self-overlap should find pairs");
        foreach (var (a, b) in pairs)
        {
            Assert.True(a >= 0 && a < tree.Mesh.Faces.Count);
            Assert.True(b >= 0 && b < tree.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void OverlappingCubes_HasManyPairs()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        Assert.True(pairs.Count > 5, $"Expected many overlapping pairs, got {pairs.Count}");
    }

    [Fact]
    public void OverlappingPairs_FaceIndicesInRange()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 1).Mesh;
        var treeA = BvhTree.Build(meshA);
        var treeB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        foreach (var (a, b) in pairs)
        {
            Assert.True(a >= 0 && a < meshA.Faces.Count, $"FaceA {a} out of range [0, {meshA.Faces.Count})");
            Assert.True(b >= 0 && b < meshB.Faces.Count, $"FaceB {b} out of range [0, {meshB.Faces.Count})");
        }
    }

    [Fact]
    public void SphereSphere_OverlappingPairs()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        Assert.True(pairs.Count > 10, $"Expected many pairs for overlapping spheres, got {pairs.Count}");
    }

    [Fact]
    public void ContainedCube_NoPairs_WhenFullyInside()
    {
        // Small cube fully inside large cube — face AABBs don't overlap
        // because the small cube faces are entirely within the large cube volume
        var treeA = BvhTree.Build(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 4).Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        // No face-level AABB overlap when fully contained (faces don't cross)
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void OverlappingPairs_NoDuplicates()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        var set = new HashSet<(int, int)>(pairs);
        Assert.Equal(pairs.Count, set.Count); // No duplicates
    }

    [Fact]
    public void BarelySeparated_NoPairs()
    {
        // Two cubes separated by just enough to not overlap
        var treeA = BvhTree.Build(MeshFactory.CreateCube().Mesh); // [0,1]^3
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(1.1, 0, 0)).Mesh); // [1.1, 2.1] x [0,1]^2
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void Deterministic_SameInputSameOutput()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh);

        var pairs1 = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        var pairs2 = BvhTraversal.FindOverlappingPairs(treeA, treeB);

        Assert.Equal(pairs1.Count, pairs2.Count);
        for (int i = 0; i < pairs1.Count; i++)
        {
            Assert.Equal(pairs1[i].FaceA, pairs2[i].FaceA);
            Assert.Equal(pairs1[i].FaceB, pairs2[i].FaceB);
        }
    }

    [Fact]
    public void TetrahedronCube_Overlap()
    {
        var treeA = BvhTree.Build(MeshFactory.CreateTetrahedron().Mesh);
        var treeB = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pairs = BvhTraversal.FindOverlappingPairs(treeA, treeB);
        // Both shapes overlap at origin area
        Assert.True(pairs.Count > 0, "Tetrahedron and unit cube should have overlapping face AABBs");
    }
}
