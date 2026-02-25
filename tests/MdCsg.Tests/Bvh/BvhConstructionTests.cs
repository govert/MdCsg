using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH construction tests — SAH, node structure, leaf properties</summary>
public class BvhConstructionTests
{
    [Fact]
    public void Build_Cube_NonZeroNodes()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Build_Cube_MeshPreserved()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Same(mesh, bvh.Mesh);
    }

    [Fact]
    public void Build_Cube_RootBoundsContainAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            Assert.True(rootBounds.Contains(a), $"Root doesn't contain vertex {a}");
            Assert.True(rootBounds.Contains(b), $"Root doesn't contain vertex {b}");
            Assert.True(rootBounds.Contains(c), $"Root doesn't contain vertex {c}");
        }
    }

    [Fact]
    public void Build_Cube_LeafNodesHavePrimitives()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        int leafCount = 0;
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            ref readonly var node = ref bvh.Nodes[i];
            if (node.IsLeaf)
            {
                leafCount++;
                totalPrimitives += node.PrimitiveCount;
                Assert.True(node.PrimitiveCount > 0);
                Assert.True(node.PrimitiveCount <= 4); // max leaf size
            }
        }
        Assert.True(leafCount > 0);
        Assert.Equal(mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void Build_Cube_InteriorNodesHaveZeroPrimCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            ref readonly var node = ref bvh.Nodes[i];
            if (!node.IsLeaf)
            {
                Assert.Equal(0, node.PrimitiveCount);
                Assert.True(node.LeftOrStart >= 0 && node.LeftOrStart < bvh.NodeCount);
                Assert.True(node.Right >= 0 && node.Right < bvh.NodeCount);
            }
        }
    }

    [Fact]
    public void Build_Sphere_MoreNodesThanCube()
    {
        var cubeMesh = MeshFactory.CreateCube().Mesh;
        var sphereMesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvhCube = BvhTree.Build(cubeMesh);
        var bvhSphere = BvhTree.Build(sphereMesh);
        Assert.True(bvhSphere.NodeCount > bvhCube.NodeCount);
    }

    [Fact]
    public void Build_Sphere_AllFacesReachable()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var reachedFaces = new HashSet<int>();
        CollectLeafFaces(bvh, 0, reachedFaces);
        Assert.Equal(mesh.Faces.Count, reachedFaces.Count);
    }

    private static void CollectLeafFaces(BvhTree bvh, int nodeIdx, HashSet<int> faces)
    {
        ref readonly var node = ref bvh.Nodes[nodeIdx];
        if (node.IsLeaf)
        {
            for (int i = 0; i < node.PrimitiveCount; i++)
                faces.Add(bvh.GetFaceIndex(node.LeftOrStart + i));
        }
        else
        {
            CollectLeafFaces(bvh, node.LeftOrStart, faces);
            CollectLeafFaces(bvh, node.Right, faces);
        }
    }

    [Fact]
    public void Build_Tetrahedron_SmallTree()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        // 4 faces → should fit in one leaf
        Assert.True(bvh.NodeCount >= 1);
        Assert.True(bvh.Nodes[0].IsLeaf); // Only 4 primitives → single leaf
        Assert.Equal(4, bvh.Nodes[0].PrimitiveCount);
    }

    [Fact]
    public void Build_FaceIndices_CoverAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var faceIndices = new HashSet<int>();
        for (int i = 0; i < mesh.Faces.Count; i++)
            faceIndices.Add(bvh.GetFaceIndex(i));
        Assert.Equal(mesh.Faces.Count, faceIndices.Count);
    }

    [Fact]
    public void Build_FaceIndices_AllValid()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            int fi = bvh.GetFaceIndex(i);
            Assert.True(fi >= 0 && fi < mesh.Faces.Count, $"Invalid face index {fi}");
        }
    }

    [Fact]
    public void Build_OffsetMesh_RootBoundsOffset()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        Assert.True(rootBounds.Min.X >= 99);
        Assert.True(rootBounds.Max.X <= 102);
        Assert.True(rootBounds.Min.Y >= 199);
        Assert.True(rootBounds.Max.Y <= 202);
    }

    [Fact]
    public void BvhNode_IsLeaf_WhenPrimitiveCountPositive()
    {
        var node = new BvhNode { PrimitiveCount = 3, LeftOrStart = 0 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_NotLeaf_WhenPrimitiveCountZero()
    {
        var node = new BvhNode { PrimitiveCount = 0, LeftOrStart = 1, Right = 2 };
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void Build_SphereSubdiv3_MoreNodes()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount > 10); // Sufficient tree depth
    }
}
