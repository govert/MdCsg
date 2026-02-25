using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree ray-AABB and ray-triangle — Moller-Trumbore, slab test, count semantics</summary>
public class BvhRayAabbTraversalTests
{
    [Fact]
    public void RayCastCount_EmptyBvh_ReturnsZero()
    {
        var mesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCastCount_RayThroughCube_EvenHits()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // Ray from outside through cube center (may hit multiple triangles per face)
        var ray = new Ray(new Vec3(-1, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        // Should hit even number of faces (entry + exit)
        Assert.True(count > 0);
        Assert.True(count % 2 == 0, $"Expected even count, got {count}");
    }

    [Fact]
    public void RayCastCount_RayMissesCube_ZeroHits()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // Ray passing above the cube
        var ray = new Ray(new Vec3(-1, 5, 0.5), new Vec3(1, 0, 0));
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCastCount_RayFromInside_OddCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // Ray from inside cube
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, "Inside point should produce odd intersection count");
    }

    [Fact]
    public void RayCastCount_RayFromOutside_EvenCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // Ray from far outside, through cube
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, "Outside point should produce even intersection count");
    }

    [Fact]
    public void RayCastCount_BackwardRay_NoHits()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // Ray pointing away from cube
        var ray = new Ray(new Vec3(-1, 0.5, 0.5), new Vec3(-1, 0, 0));
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCastCount_TetrahedronFromInside_OddCount()
    {
        var tetra = MeshFactory.CreateTetrahedron();
        var bvh = BvhTree.Build(tetra.Mesh);
        // Centroid is inside tetrahedron
        var centroid = Vec3.Zero;
        foreach (var v in tetra.Mesh.Vertices)
            centroid = centroid + v.Position;
        centroid = centroid / tetra.Mesh.Vertices.Count;
        var ray = new Ray(centroid, new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1);
    }

    [Fact]
    public void RayCastCount_Sphere_InsideOdd()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1);
    }

    [Fact]
    public void Query_EmptyBvh_EmptyResult()
    {
        var mesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(Vec3.Zero, new Vec3(1, 1, 1)), results);
        Assert.Equal(0, results.Count);
    }

    [Fact]
    public void Query_OverlappingBox_FindsFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(0.1, 0.1, 0.1), new Vec3(0.9, 0.9, 0.9)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void Query_NonOverlappingBox_NoFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6)), results);
        Assert.Equal(0, results.Count);
    }

    [Fact]
    public void Query_FullyCoveringBox_FindsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        Assert.Equal(cube.Mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Query_ResultsAreValidFaceIndices()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        foreach (int idx in results)
        {
            Assert.True(idx >= 0 && idx < cube.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void RayCastCount_MultipleDirections_Consistent()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var origin = new Vec3(0.5, 0.5, 0.5);
        // Use perturbed directions to avoid edge/vertex hits on triangulated cube
        var directions = new[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized,
            new Vec3(-1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, -1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, -1).Normalized,
        };
        foreach (var dir in directions)
        {
            var ray = new Ray(origin, dir);
            int count = bvh.RayCastCount(ray);
            Assert.True(count % 2 == 1, $"Inside cube should have odd count for direction {dir}, got {count}");
        }
    }

    [Fact]
    public void GetFaceIndex_ValidIndicesForLeaves()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].IsLeaf)
            {
                for (int j = 0; j < nodes[i].PrimitiveCount; j++)
                {
                    int faceIdx = bvh.GetFaceIndex(nodes[i].LeftOrStart + j);
                    Assert.True(faceIdx >= 0 && faceIdx < cube.Mesh.Faces.Count);
                }
            }
        }
    }

    [Fact]
    public void FaceIndices_CoverAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var covered = new HashSet<int>();
        var nodes = bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].IsLeaf)
            {
                for (int j = 0; j < nodes[i].PrimitiveCount; j++)
                    covered.Add(bvh.GetFaceIndex(nodes[i].LeftOrStart + j));
            }
        }
        Assert.Equal(cube.Mesh.Faces.Count, covered.Count);
    }

    [Fact]
    public void RayCastCount_OffsetCube_CorrectParity()
    {
        var cube = MeshFactory.CreateCube(new Vec3(5, 10, 15));
        var bvh = BvhTree.Build(cube.Mesh);
        // Inside
        var insideRay = new Ray(new Vec3(5.5, 10.5, 15.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        Assert.True(bvh.RayCastCount(insideRay) % 2 == 1);
        // Outside
        var outsideRay = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0.00013, 0.00017).Normalized);
        Assert.True(bvh.RayCastCount(outsideRay) % 2 == 0);
    }

    [Fact]
    public void Query_PartialOverlap_FindsSomeFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        // Query box overlaps only the top half
        bvh.Query(new Aabb(new Vec3(-0.5, 0.5, -0.5), new Vec3(1.5, 1.5, 1.5)), results);
        Assert.True(results.Count > 0);
        Assert.True(results.Count <= cube.Mesh.Faces.Count);
    }
}
