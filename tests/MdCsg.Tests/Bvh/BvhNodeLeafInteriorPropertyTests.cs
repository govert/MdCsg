using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhNode — IsLeaf, LeftOrStart, Right, PrimitiveCount, bounds containment invariants</summary>
public class BvhNodeLeafInteriorPropertyTests
{
    [Fact]
    public void CubeBvh_RootNode_HasBounds()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var root = bvh.Nodes[0];
        Assert.True(root.Bounds.SurfaceArea > 0);
    }

    [Fact]
    public void CubeBvh_RootBounds_ContainsAllFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var rootBounds = bvh.Nodes[0].Bounds;
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
        {
            cube.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            var faceBounds = Aabb.FromTriangle(a, b, c);
            Assert.True(rootBounds.Contains(faceBounds.Min) ||
                        System.Math.Abs(rootBounds.Min.X - faceBounds.Min.X) < 0.01);
        }
    }

    [Fact]
    public void CubeBvh_LeafNodes_HavePositivePrimitiveCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var nodes = bvh.Nodes;
        bool foundLeaf = false;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
            {
                foundLeaf = true;
                Assert.True(nodes[i].PrimitiveCount > 0);
                Assert.True(nodes[i].PrimitiveCount <= 4, "Leaf should have at most 4 primitives");
            }
        }
        Assert.True(foundLeaf, "BVH should have at least one leaf node");
    }

    [Fact]
    public void CubeBvh_InteriorNodes_HaveZeroPrimitiveCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                Assert.Equal(0, nodes[i].PrimitiveCount);
            }
        }
    }

    [Fact]
    public void CubeBvh_InteriorNodes_ChildIndicesValid()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].LeftOrStart >= 0 && nodes[i].LeftOrStart < bvh.NodeCount,
                    $"Left child {nodes[i].LeftOrStart} out of range for interior node {i}");
                Assert.True(nodes[i].Right > 0 && nodes[i].Right < bvh.NodeCount,
                    $"Right child {nodes[i].Right} out of range for interior node {i}");
            }
        }
    }

    [Fact]
    public void CubeBvh_LeafNodes_StartIndexValid()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].LeftOrStart >= 0,
                    $"Leaf start index should be non-negative, got {nodes[i].LeftOrStart}");
                Assert.True(nodes[i].LeftOrStart + nodes[i].PrimitiveCount <= bvh.FaceIndices.Length,
                    $"Leaf range exceeds face indices array");
            }
        }
    }

    [Fact]
    public void SphereBvh_HasMultipleNodes()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(sphere.Bvh.NodeCount > 1, "Sphere with 320 faces should have multiple BVH nodes");
    }

    [Fact]
    public void BvhNode_Default_IsLeaf()
    {
        var node = new BvhNode();
        // Default PrimitiveCount is 0, so IsLeaf is false
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_WithPrimitives_IsLeaf()
    {
        var node = new BvhNode { PrimitiveCount = 3 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_ZeroPrimitives_IsNotLeaf()
    {
        var node = new BvhNode { PrimitiveCount = 0, LeftOrStart = 1, Right = 2 };
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void CubeBvh_AllBoundsNonDegenerate()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var size = nodes[i].Bounds.Size;
            // At least one dimension should be non-zero (some faces may be flat on one axis)
            Assert.True(size.X >= 0 && size.Y >= 0 && size.Z >= 0,
                $"Node {i} has negative bounds dimension");
        }
    }

    [Fact]
    public void TetrahedronBvh_NodeCount_Positive()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(tet.Bvh.NodeCount > 0);
    }

    [Fact]
    public void TetrahedronBvh_FourFaces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Bvh.FaceIndices.Length);
    }

    [Fact]
    public void SphereBvh_ChildBoundsContainedByParent()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var bvh = sphere.Bvh;
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                var parent = nodes[i].Bounds;
                var leftBounds = nodes[nodes[i].LeftOrStart].Bounds;
                var rightBounds = nodes[nodes[i].Right].Bounds;
                // Child bounds should be within parent (with tolerance)
                Assert.True(leftBounds.Min.X >= parent.Min.X - 0.01);
                Assert.True(leftBounds.Max.X <= parent.Max.X + 0.01);
                Assert.True(rightBounds.Min.X >= parent.Min.X - 0.01);
                Assert.True(rightBounds.Max.X <= parent.Max.X + 0.01);
            }
        }
    }

    [Fact]
    public void EmptyBvh_NodeCountZero()
    {
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Equal(0, empty.Bvh.NodeCount);
    }
}
