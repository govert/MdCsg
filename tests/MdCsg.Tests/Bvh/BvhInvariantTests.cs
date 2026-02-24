using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH structural invariants and traversal correctness</summary>
public class BvhInvariantTests
{
    [Fact]
    public void BvhTree_RootBounds_ContainsAllFaces()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        for (int i = 0; i < bvh.Mesh.Faces.Count; i++)
        {
            bvh.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            Assert.True(rootBounds.Contains(a), $"Vertex {a} not in root bounds");
            Assert.True(rootBounds.Contains(b), $"Vertex {b} not in root bounds");
            Assert.True(rootBounds.Contains(c), $"Vertex {c} not in root bounds");
        }
    }

    [Fact]
    public void BvhTree_AllFaceIndices_ValidRange()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        for (int i = 0; i < bvh.FaceIndices.Length; i++)
        {
            int idx = bvh.GetFaceIndex(i);
            Assert.True(idx >= 0 && idx < bvh.Mesh.Faces.Count,
                $"Face index {idx} out of range [0, {bvh.Mesh.Faces.Count})");
        }
    }

    [Fact]
    public void BvhTree_AllFaceIndices_Unique()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var seen = new HashSet<int>();
        for (int i = 0; i < bvh.FaceIndices.Length; i++)
        {
            Assert.True(seen.Add(bvh.GetFaceIndex(i)), $"Duplicate face index at position {i}");
        }
    }

    [Fact]
    public void BvhTree_AllFaces_Represented()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void BvhTree_LeafNodes_HavePrimitives()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].PrimitiveCount > 0);
                Assert.True(nodes[i].PrimitiveCount <= 4); // max leaf size
            }
        }
    }

    [Fact]
    public void BvhTree_InternalNodes_HaveChildren()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].LeftOrStart >= 0 && nodes[i].LeftOrStart < nodes.Length);
                Assert.True(nodes[i].Right > 0 && nodes[i].Right < nodes.Length);
            }
        }
    }

    [Fact]
    public void BvhTree_ChildBounds_InsideParent()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                var parent = nodes[i].Bounds;
                var left = nodes[nodes[i].LeftOrStart].Bounds;
                var right = nodes[nodes[i].Right].Bounds;
                // Child bounds should be contained within parent (with epsilon for float errors)
                var expanded = parent.Expand(1e-10);
                Assert.True(expanded.Contains(left.Min), $"Left min {left.Min} outside parent");
                Assert.True(expanded.Contains(left.Max), $"Left max {left.Max} outside parent");
                Assert.True(expanded.Contains(right.Min), $"Right min {right.Min} outside parent");
                Assert.True(expanded.Contains(right.Max), $"Right max {right.Max} outside parent");
            }
        }
    }

    [Fact]
    public void BvhTree_Query_FullBox_ReturnsAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        Assert.Equal(mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void BvhTree_Query_SmallBox_ReturnsSubset()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var results = new List<int>();
        // Query near the surface of the sphere (radius 1) to ensure hits
        bvh.Query(new Aabb(new Vec3(0.8, -0.3, -0.3), new Vec3(1.2, 0.3, 0.3)), results);
        Assert.True(results.Count > 0, "Expected some faces near sphere surface");
        Assert.True(results.Count < bvh.Mesh.Faces.Count, "Expected subset, not all faces");
    }

    [Fact]
    public void BvhTraversal_OverlappingPairs_AllValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        foreach (var (fA, fB) in pairs)
        {
            Assert.True(fA >= 0 && fA < meshA.Faces.Count);
            Assert.True(fB >= 0 && fB < meshB.Faces.Count);
        }
    }

    [Fact]
    public void BvhTraversal_OverlappingPairs_ActuallyOverlap()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        foreach (var (fA, fB) in pairs)
        {
            meshA.Faces[fA].GetTrianglePositions(out var a1, out var b1, out var c1);
            meshB.Faces[fB].GetTrianglePositions(out var a2, out var b2, out var c2);
            var boundsA = Aabb.FromTriangle(a1, b1, c1);
            var boundsB = Aabb.FromTriangle(a2, b2, c2);
            Assert.True(boundsA.Overlaps(boundsB), "Reported overlapping pair doesn't overlap");
        }
    }

    [Fact]
    public void BvhTree_Sphere_LargerTree_CorrectStructure()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh);
        Assert.True(bvh.NodeCount > 10); // non-trivial tree
        Assert.True(bvh.FaceIndices.Length > 50); // many faces
    }

    [Fact]
    public void RayCast_Sphere_AllDirections_HitOrMiss()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var dirs = new[]
        {
            Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ,
            -Vec3.UnitX, -Vec3.UnitY, -Vec3.UnitZ,
        };
        foreach (var dir in dirs)
        {
            var ray = new Ray(dir * -5, dir);
            int hits = bvh.RayCastCount(ray);
            Assert.True(hits >= 2 && hits % 2 == 0,
                $"Sphere ray dir {dir}: expected even hits >= 2, got {hits}");
        }
    }
}
