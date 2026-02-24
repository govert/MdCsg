using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Batch 46: BVH deep tests (20 tests)</summary>
public class BvhDeepTests
{
    [Fact]
    public void Cube_BvhNodeCount_Positive()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Cube_BvhNodeCount_OddNumber()
    {
        // Binary tree with N leaves has 2N-1 nodes
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.True(bvh.NodeCount % 2 == 1 || bvh.NodeCount == 1);
    }

    [Fact]
    public void Cube_BvhRootBounds_ContainsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        var meshBounds = cube.Mesh.GetBounds();
        Assert.True(rootBounds.Contains(meshBounds.Min) ||
            Vec3.Distance(rootBounds.Min, meshBounds.Min) < 0.01);
        Assert.True(rootBounds.Contains(meshBounds.Max) ||
            Vec3.Distance(rootBounds.Max, meshBounds.Max) < 0.01);
    }

    [Fact]
    public void Cube_BvhRoot_IsNotLeafForMultipleFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // With 12 faces, root should be interior
        Assert.False(bvh.Nodes[0].IsLeaf);
    }

    [Fact]
    public void Cube_Leaves_CoverAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
                totalPrimitives += node.PrimitiveCount;
        }
        Assert.Equal(12, totalPrimitives); // 12 triangle faces
    }

    [Fact]
    public void Tetrahedron_BvhBuilds()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var bvh = BvhTree.Build(tet.Mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Tetrahedron_BvhLeaves_Cover4Faces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var bvh = BvhTree.Build(tet.Mesh);
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                totalPrimitives += bvh.Nodes[i].PrimitiveCount;
        }
        Assert.Equal(4, totalPrimitives);
    }

    [Fact]
    public void Sphere_Sub2_BvhBuilds()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Sphere_Sub2_BvhLeavesCoverAllFaces()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                totalPrimitives += bvh.Nodes[i].PrimitiveCount;
        }
        Assert.Equal(sphere.Mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void BvhNode_IsLeaf_TrueForPositivePrimitiveCount()
    {
        var node = new BvhNode { PrimitiveCount = 1 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_IsLeaf_FalseForZeroPrimitiveCount()
    {
        var node = new BvhNode { PrimitiveCount = 0 };
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void Cube_FaceIndices_Length_EqualsFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.Equal(12, bvh.FaceIndices.Length);
    }

    [Fact]
    public void Cube_FaceIndices_ContainsAllIndices()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var indices = new HashSet<int>();
        for (int i = 0; i < bvh.FaceIndices.Length; i++)
            indices.Add(bvh.FaceIndices[i]);
        for (int i = 0; i < 12; i++)
            Assert.Contains(i, indices);
    }

    [Fact]
    public void BvhTree_Mesh_Property()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.Same(cube.Mesh, bvh.Mesh);
    }

    [Fact]
    public void Cube_Interior_HasChildren()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var root = bvh.Nodes[0];
        if (!root.IsLeaf)
        {
            Assert.True(root.LeftOrStart >= 0 && root.LeftOrStart < bvh.NodeCount);
            Assert.True(root.Right >= 0 && root.Right < bvh.NodeCount);
        }
    }

    [Fact]
    public void OffsetCube_BvhBounds_MatchMeshBounds()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var bvh = BvhTree.Build(cube.Mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        Assert.True(rootBounds.Min.X >= 9.99);
        Assert.True(rootBounds.Min.Y >= 19.99);
        Assert.True(rootBounds.Max.X <= 11.01);
    }

    [Fact]
    public void LargeCube_BvhBuilds()
    {
        var cube = MeshFactory.CreateCube(size: 100);
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.True(bvh.NodeCount > 0);
        Assert.True(bvh.Nodes[0].Bounds.Size.X > 99);
    }

    [Fact]
    public void SmallCube_BvhBuilds()
    {
        var cube = MeshFactory.CreateCube(size: 0.001);
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.True(bvh.NodeCount > 0);
        Assert.True(bvh.Nodes[0].Bounds.Size.X < 0.002);
    }

    [Fact]
    public void Leaf_PrimitiveCountPositive()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
                Assert.True(node.PrimitiveCount > 0);
        }
    }

    [Fact]
    public void InteriorNode_HasZeroPrimitiveCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
                Assert.Equal(0, node.PrimitiveCount);
        }
    }
}
