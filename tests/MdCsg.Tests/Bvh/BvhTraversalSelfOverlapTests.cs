using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal — self-overlap, empty trees, face-level granularity, symmetry</summary>
public class BvhTraversalSelfOverlapTests
{
    [Fact]
    public void EmptyTrees_ReturnsNoPairs()
    {
        var meshA = new HalfEdgeMesh();
        var meshB = new HalfEdgeMesh();
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void OneEmpty_ReturnsNoPairs()
    {
        var cube = MeshFactory.CreateCube();
        var empty = BvhTree.Build(new HalfEdgeMesh());
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, empty);
        Assert.Empty(pairs);
    }

    [Fact]
    public void SelfOverlap_Cube_FindsAllSelfPairs()
    {
        var cube = MeshFactory.CreateCube();
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, cube.Bvh);
        // Self-overlap: every face overlaps itself, plus adjacent face pairs
        Assert.True(pairs.Count >= cube.Mesh.Faces.Count,
            $"Self-overlap should find at least {cube.Mesh.Faces.Count} pairs, got {pairs.Count}");
    }

    [Fact]
    public void DisjointCubes_NoPairs()
    {
        var cubeA = MeshFactory.CreateCube(new Vec3(0, 0, 0));
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var pairs = BvhTraversal.FindOverlappingPairs(cubeA.Bvh, cubeB.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void OverlappingCubes_FindsPairs()
    {
        var cubeA = MeshFactory.CreateCube(new Vec3(0, 0, 0));
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var pairs = BvhTraversal.FindOverlappingPairs(cubeA.Bvh, cubeB.Bvh);
        Assert.True(pairs.Count > 0, "Overlapping cubes should have face pairs");
    }

    [Fact]
    public void OverlappingCubes_PairIndicesInRange()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var pairs = BvhTraversal.FindOverlappingPairs(cubeA.Bvh, cubeB.Bvh);
        foreach (var (faceA, faceB) in pairs)
        {
            Assert.True(faceA >= 0 && faceA < cubeA.Mesh.Faces.Count,
                $"FaceA {faceA} out of range [0, {cubeA.Mesh.Faces.Count})");
            Assert.True(faceB >= 0 && faceB < cubeB.Mesh.Faces.Count,
                $"FaceB {faceB} out of range [0, {cubeB.Mesh.Faces.Count})");
        }
    }

    [Fact]
    public void SymmetryCheck_SwappedTreesSameCount()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4));
        var pairsAB = BvhTraversal.FindOverlappingPairs(cubeA.Bvh, cubeB.Bvh);
        var pairsBA = BvhTraversal.FindOverlappingPairs(cubeB.Bvh, cubeA.Bvh);
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void SymmetryCheck_SwappedPairsMatch()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4));
        var pairsAB = BvhTraversal.FindOverlappingPairs(cubeA.Bvh, cubeB.Bvh);
        var pairsBA = BvhTraversal.FindOverlappingPairs(cubeB.Bvh, cubeA.Bvh);
        var setAB = new HashSet<(int, int)>(pairsAB);
        var setBA = new HashSet<(int, int)>(pairsBA.Select(p => (p.FaceB, p.FaceA)));
        Assert.Equal(setAB, setBA);
    }

    [Fact]
    public void SingleTriangleMeshes_Overlapping()
    {
        var meshA = new HalfEdgeMesh();
        var v0a = meshA.AddVertex(Vec3.Zero);
        var v1a = meshA.AddVertex(new Vec3(1, 0, 0));
        var v2a = meshA.AddVertex(new Vec3(0, 1, 0));
        meshA.AddFace(v0a, v1a, v2a);

        var meshB = new HalfEdgeMesh();
        var v0b = meshB.AddVertex(new Vec3(0.25, 0.25, -0.5));
        var v1b = meshB.AddVertex(new Vec3(0.75, 0.25, -0.5));
        var v2b = meshB.AddVertex(new Vec3(0.25, 0.25, 0.5));
        meshB.AddFace(v0b, v1b, v2b);

        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Single(pairs);
        Assert.Equal(0, pairs[0].FaceA);
        Assert.Equal(0, pairs[0].FaceB);
    }

    [Fact]
    public void SingleTriangleMeshes_NonOverlapping()
    {
        var meshA = new HalfEdgeMesh();
        var v0a = meshA.AddVertex(Vec3.Zero);
        var v1a = meshA.AddVertex(new Vec3(1, 0, 0));
        var v2a = meshA.AddVertex(new Vec3(0, 1, 0));
        meshA.AddFace(v0a, v1a, v2a);

        var meshB = new HalfEdgeMesh();
        var v0b = meshB.AddVertex(new Vec3(10, 10, 10));
        var v1b = meshB.AddVertex(new Vec3(11, 10, 10));
        var v2b = meshB.AddVertex(new Vec3(10, 11, 10));
        meshB.AddFace(v0b, v1b, v2b);

        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void SphereSphere_OverlappingFindsPairs()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var sphereB = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 1);
        var pairs = BvhTraversal.FindOverlappingPairs(sphereA.Bvh, sphereB.Bvh);
        Assert.True(pairs.Count > 0, "Overlapping spheres should have face pairs");
    }

    [Fact]
    public void SphereSphere_Disjoint_NoPairs()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1);
        var sphereB = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 0.5, 1);
        var pairs = BvhTraversal.FindOverlappingPairs(sphereA.Bvh, sphereB.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void CubeSphere_OverlappingFindsPairs()
    {
        var cube = MeshFactory.CreateCube();
        // Sphere penetrates cube surface so face AABBs actually overlap
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1);
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, sphere.Bvh);
        Assert.True(pairs.Count > 0, "Cube-sphere with overlapping surfaces should have face pairs");
    }

    [Fact]
    public void NoDuplicatePairs()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var pairs = BvhTraversal.FindOverlappingPairs(cubeA.Bvh, cubeB.Bvh);
        var set = new HashSet<(int, int)>(pairs);
        Assert.Equal(pairs.Count, set.Count);
    }
}
