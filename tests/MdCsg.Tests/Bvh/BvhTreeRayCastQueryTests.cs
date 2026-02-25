using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree — RayCastCount, Query, GetFaceIndex, FaceIndices property</summary>
public class BvhTreeRayCastQueryTests
{
    private static BvhTree BuildCubeBvh(Vec3 offset = default, double size = 1)
    {
        var cube = MeshFactory.CreateCube(offset, size);
        return BvhTree.Build(cube.Mesh);
    }

    [Fact]
    public void RayCastCount_RayThrough_ReturnsEven()
    {
        // Ray through cube should hit even number of faces (entry + exit, possibly on edges)
        var bvh = BuildCubeBvh();
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count >= 2 && count % 2 == 0, $"Expected even count >= 2, got {count}");
    }

    [Fact]
    public void RayCastCount_RayMisses_ReturnsZero()
    {
        var bvh = BuildCubeBvh();
        var ray = new Ray(new Vec3(-5, 5, 5), new Vec3(1, 0, 0).Normalized);
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCastCount_RayFromInside_ReturnsOdd()
    {
        var bvh = BuildCubeBvh();
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(1, count % 2); // odd = inside
    }

    [Fact]
    public void RayCastCount_RayFromOutside_ReturnsEven()
    {
        var bvh = BuildCubeBvh();
        var ray = new Ray(new Vec3(5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count % 2); // even = outside
    }

    [Fact]
    public void RayCastCount_EmptyBvh_ReturnsZero()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void Query_OverlappingBox_FindsFaces()
    {
        var bvh = BuildCubeBvh();
        var queryBox = new Aabb(new Vec3(0.4, 0.4, 0.4), new Vec3(0.6, 0.6, 0.6));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void Query_NonOverlappingBox_FindsNothing()
    {
        var bvh = BuildCubeBvh();
        var queryBox = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Empty(results);
    }

    [Fact]
    public void Query_FullyContainingBox_FindsAllFaces()
    {
        var bvh = BuildCubeBvh();
        var queryBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Equal(12, results.Count); // 6 faces * 2 triangles = 12
    }

    [Fact]
    public void Query_EmptyBvh_NoResults()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(Vec3.Zero, new Vec3(1, 1, 1)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void GetFaceIndex_ValidIndices_ReturnsMappedFace()
    {
        var bvh = BuildCubeBvh();
        // All face indices from 0..11 should be accessible
        var indices = new HashSet<int>();
        for (int i = 0; i < 12; i++)
            indices.Add(bvh.GetFaceIndex(i));
        // All 12 face indices should be present (permuted by BVH construction)
        Assert.Equal(12, indices.Count);
    }

    [Fact]
    public void FaceIndices_SpanLength_EqualsFaceCount()
    {
        var bvh = BuildCubeBvh();
        Assert.Equal(12, bvh.FaceIndices.Length);
    }

    [Fact]
    public void FaceIndices_AllInRange()
    {
        var bvh = BuildCubeBvh();
        var indices = bvh.FaceIndices;
        for (int i = 0; i < indices.Length; i++)
        {
            Assert.True(indices[i] >= 0 && indices[i] < 12);
        }
    }

    [Fact]
    public void NodeCount_Cube_Positive()
    {
        var bvh = BuildCubeBvh();
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Mesh_Property_ReturnsSameMesh()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.Same(cube.Mesh, bvh.Mesh);
    }

    [Fact]
    public void RayCastCount_MultipleDirections_ConsistentParity()
    {
        // From inside, all perturbed rays should give odd counts
        var bvh = BuildCubeBvh();
        var origin = new Vec3(0.5, 0.5, 0.5);

        var dirs = new[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized,
        };

        foreach (var dir in dirs)
        {
            int count = bvh.RayCastCount(new Ray(origin, dir));
            Assert.Equal(1, count % 2);
        }
    }

    [Fact]
    public void Query_PartialOverlap_FindsSubset()
    {
        var bvh = BuildCubeBvh();
        // Query just one corner — should find some but not all faces
        var queryBox = new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, 0.1, 0.1));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0);
        Assert.True(results.Count < 12);
    }

    [Fact]
    public void RayCastCount_SphereBvh_ConsistentFromOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        var ray = new Ray(new Vec3(-5, 0.00013, 0.00017), new Vec3(1, 0, 0).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count % 2); // even = entered and exited
        Assert.True(count >= 2); // at least entry + exit
    }
}
