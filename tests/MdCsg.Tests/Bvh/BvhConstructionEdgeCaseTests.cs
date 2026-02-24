using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH construction edge cases and query behavior</summary>
public class BvhConstructionEdgeCaseTests
{
    [Fact]
    public void Build_SingleTriangle_SingleNode()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount >= 1);
        Assert.Equal(1, bvh.FaceIndices.Length);
    }

    [Fact]
    public void Build_Cube_AllFacesIndexed()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void Build_Sphere_ValidStructure()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount > 1);
        Assert.Equal(mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void Query_EmptyBox_NoResults()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(5, 5, 5), new Vec3(5.01, 5.01, 5.01)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void Query_HugeBox_AllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-100, -100, -100), new Vec3(100, 100, 100)), results);
        Assert.Equal(mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Query_HalfCube_Subset()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        // Query box covers roughly half the cube
        bvh.Query(new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.5, 1.1, 1.1)), results);
        Assert.True(results.Count > 0 && results.Count <= mesh.Faces.Count);
    }

    [Fact]
    public void Query_ResultsAreValidFaceIndices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        foreach (int idx in results)
        {
            Assert.True(idx >= 0 && idx < mesh.Faces.Count);
        }
    }

    [Fact]
    public void RayCastCount_CubeFromOutside_EvenHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(0.3, 0.4, -5), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0, $"Expected even >= 2 hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_MissRay_ZeroHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray completely misses the cube
        var ray = new Ray(new Vec3(5, 5, -5), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.Equal(0, hits);
    }

    [Fact]
    public void RayCastCount_SphereFromOutside_EvenHits()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(0.1, 0.2, -5), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0, $"Expected even >= 2 hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_AlongX_ThroughCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(-5, 0.3, 0.4), new Vec3(1, 0, 0));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0, $"Expected even >= 2, got {hits}");
    }

    [Fact]
    public void RayCastCount_AlongY_ThroughCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(0.3, -5, 0.4), new Vec3(0, 1, 0));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0, $"Expected even >= 2, got {hits}");
    }

    [Fact]
    public void BvhTraversal_DisjointTrees_NoPairs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }

    [Fact]
    public void BvhTraversal_OverlappingTrees_HasPairs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void BvhTraversal_Symmetric()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairsAB = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        var pairsBA = BvhTraversal.FindOverlappingPairs(bvhB, bvhA);
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void GetFaceIndex_AllValid()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var seen = new HashSet<int>();
        for (int i = 0; i < bvh.FaceIndices.Length; i++)
        {
            int idx = bvh.GetFaceIndex(i);
            Assert.True(idx >= 0 && idx < mesh.Faces.Count);
            seen.Add(idx);
        }
        Assert.Equal(mesh.Faces.Count, seen.Count); // all faces represented
    }
}
