using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal dual-tree properties — pair symmetry, face-level AABB filtering, empty tree cases</summary>
public class BvhDualTreePropertyTests
{
    [Fact]
    public void EmptyTreeA_NoPairs()
    {
        var emptyMesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var cube = MeshFactory.CreateCube().Mesh;
        var bvhEmpty = BvhTree.Build(emptyMesh);
        var bvhCube = BvhTree.Build(cube);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhEmpty, bvhCube);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void EmptyTreeB_NoPairs()
    {
        var emptyMesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var cube = MeshFactory.CreateCube().Mesh;
        var bvhCube = BvhTree.Build(cube);
        var bvhEmpty = BvhTree.Build(emptyMesh);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhCube, bvhEmpty);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void BothEmpty_NoPairs()
    {
        var emptyMesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var bvhA = BvhTree.Build(emptyMesh);
        var bvhB = BvhTree.Build(emptyMesh);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void DisjointCubes_NoPairs()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void OverlappingCubes_HasPairs()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void PairFaceIndices_InBounds()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        foreach (var (fA, fB) in pairs)
        {
            Assert.True(fA >= 0 && fA < cubeA.Faces.Count);
            Assert.True(fB >= 0 && fB < cubeB.Faces.Count);
        }
    }

    [Fact]
    public void IdenticalCubes_AllFacesPair()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube().Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        // Every face from A should overlap with at least one face from B
        var facesA = new HashSet<int>();
        foreach (var (fA, _) in pairs) facesA.Add(fA);
        // All 12 faces should appear
        Assert.Equal(cubeA.Faces.Count, facesA.Count);
    }

    [Fact]
    public void TouchingFaces_Detected()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        // Touching faces (x=1 plane) should be detected
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void SelfOverlap_AllFaces()
    {
        var cube = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(cube);
        var pairs = BvhTraversal.FindOverlappingPairs(bvh, bvh);
        // Self-overlap should find pairs
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void SpherePair_HasPairs()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var sphereB = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh;
        var bvhA = BvhTree.Build(sphereA);
        var bvhB = BvhTree.Build(sphereB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void DisjointSpheres_NoPairs()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh;
        var sphereB = MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 2).Mesh;
        var bvhA = BvhTree.Build(sphereA);
        var bvhB = BvhTree.Build(sphereB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void CubeSphere_PairsValid()
    {
        // Sphere centered at cube face — guaranteed to intersect
        var cube = MeshFactory.CreateCube().Mesh;
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.0), 0.5, 2).Mesh;
        var bvhCube = BvhTree.Build(cube);
        var bvhSphere = BvhTree.Build(sphere);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhCube, bvhSphere);
        Assert.True(pairs.Count > 0, "Intersecting cube-sphere should have overlapping face pairs");
        foreach (var (fA, fB) in pairs)
        {
            Assert.True(fA >= 0 && fA < cube.Faces.Count);
            Assert.True(fB >= 0 && fB < sphere.Faces.Count);
        }
    }

    [Fact]
    public void LargeOffset_NoPairs()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube(new Vec3(100, 100, 100)).Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void SmallOverlap_HasFewPairs()
    {
        var cubeA = MeshFactory.CreateCube().Mesh;
        var cubeB = MeshFactory.CreateCube(new Vec3(0.9, 0.9, 0.9)).Mesh;
        var bvhA = BvhTree.Build(cubeA);
        var bvhB = BvhTree.Build(cubeB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        // Small overlap should produce fewer pairs than large overlap
        var cubeC = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhC = BvhTree.Build(cubeC);
        var pairsLarge = BvhTraversal.FindOverlappingPairs(bvhA, bvhC);
        Assert.True(pairs.Count <= pairsLarge.Count,
            $"Small overlap ({pairs.Count} pairs) should have fewer/equal pairs than large overlap ({pairsLarge.Count})");
    }
}
