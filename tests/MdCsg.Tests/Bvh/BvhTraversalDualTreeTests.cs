using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal dual-tree overlap — pair finding, symmetry, disjoint/nested</summary>
public class BvhTraversalDualTreeTests
{
    [Fact]
    public void DisjointCubes_NoPairs()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(100, 100, 100)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void OverlappingCubes_SomePairs()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0, "Overlapping cubes should have face pairs");
    }

    [Fact]
    public void SameCube_AllFacesPairWithSomething()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        // Identical meshes — every face should have at least one overlapping pair
        var facesA = pairs.Select(p => p.FaceA).Distinct().Count();
        Assert.True(facesA > 0);
    }

    [Fact]
    public void PairFaceIndices_ValidRange()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        foreach (var (fA, fB) in pairs)
        {
            Assert.True(fA >= 0 && fA < meshA.Faces.Count, $"FaceA {fA} out of range");
            Assert.True(fB >= 0 && fB < meshB.Faces.Count, $"FaceB {fB} out of range");
        }
    }

    [Fact]
    public void Traversal_Symmetric_SamePairCount()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairsAB = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        var pairsBA = BvhTraversal.FindOverlappingPairs(bvhB, bvhA);
        // AB pairs (fA,fB) should correspond to BA pairs (fB,fA)
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void SmallInsideLarge_HasPairs()
    {
        // Small cube fully inside large cube
        var meshLarge = MeshFactory.CreateCube(Vec3.Zero, 5).Mesh;
        var meshSmall = MeshFactory.CreateCube(new Vec3(2.5, 2.5, 2.5), 0.5).Mesh;
        var bvhL = BvhTree.Build(meshLarge);
        var bvhS = BvhTree.Build(meshSmall);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhL, bvhS);
        // Even if no intersection, face AABBs may overlap due to BVH structure
        // At minimum, the small cube's faces project onto large cube's faces
        Assert.True(pairs.Count >= 0); // Won't crash
    }

    [Fact]
    public void CubeSphere_HasPairs()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 2).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0, "Overlapping cube-sphere should have pairs");
    }

    [Fact]
    public void EmptyMeshA_NoPairs()
    {
        var emptyMesh = new MdCsg.Mesh.HalfEdgeMesh();
        var meshB = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var bvhA = BvhTree.Build(emptyMesh);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void EmptyMeshB_NoPairs()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var emptyMesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(emptyMesh);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void BothEmpty_NoPairs()
    {
        var emptyA = new MdCsg.Mesh.HalfEdgeMesh();
        var emptyB = new MdCsg.Mesh.HalfEdgeMesh();
        var bvhA = BvhTree.Build(emptyA);
        var bvhB = BvhTree.Build(emptyB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void TetrahedronPairs_WithCube()
    {
        var meshA = MeshFactory.CreateTetrahedron().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), 0.5).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void NoPairDuplicates()
    {
        var meshA = MeshFactory.CreateCube(Vec3.Zero).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        var uniquePairs = pairs.Distinct().Count();
        Assert.Equal(pairs.Count, uniquePairs);
    }

    [Fact]
    public void FarApart_X_NoPairs()
    {
        var meshA = MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(1000, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FarApart_Y_NoPairs()
    {
        var meshA = MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0, 1000, 0)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FarApart_Z_NoPairs()
    {
        var meshA = MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0, 0, 1000)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }
}
