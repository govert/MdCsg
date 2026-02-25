using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH SAH properties — node bounds containment, leaf counts, root bounds, interior/leaf invariants</summary>
public class BvhSahPropertyTests
{
    [Fact]
    public void RootBounds_ContainsAllVertices_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        foreach (var v in mesh.Vertices)
        {
            Assert.True(rootBounds.Contains(v.Position),
                $"Root bounds should contain vertex {v.Position}");
        }
    }

    [Fact]
    public void RootBounds_ContainsAllVertices_Sphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        foreach (var v in mesh.Vertices)
        {
            Assert.True(rootBounds.Contains(v.Position));
        }
    }

    [Fact]
    public void AllLeafFaceIndices_InBounds()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
            {
                for (int j = 0; j < node.PrimitiveCount; j++)
                {
                    int faceIdx = bvh.FaceIndices[node.LeftOrStart + j];
                    Assert.True(faceIdx >= 0 && faceIdx < mesh.Faces.Count,
                        $"Face index {faceIdx} out of range [0, {mesh.Faces.Count})");
                }
            }
        }
    }

    [Fact]
    public void InteriorNodes_HaveZeroPrimitiveCount()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
            {
                Assert.Equal(0, node.PrimitiveCount);
                Assert.True(node.LeftOrStart >= 0 && node.LeftOrStart < bvh.NodeCount);
                Assert.True(node.Right >= 0 && node.Right < bvh.NodeCount);
            }
        }
    }

    [Fact]
    public void LeafNodes_HavePositivePrimitiveCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        int leafCount = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
            {
                Assert.True(bvh.Nodes[i].PrimitiveCount > 0);
                leafCount++;
            }
        }
        Assert.True(leafCount > 0, "BVH should have at least one leaf");
    }

    [Fact]
    public void TotalPrimitivesInLeaves_EqualsFaceCount()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                totalPrimitives += bvh.Nodes[i].PrimitiveCount;
        }
        Assert.Equal(mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void ChildBounds_ContainedInParent()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
            {
                var left = bvh.Nodes[node.LeftOrStart];
                var right = bvh.Nodes[node.Right];
                // Child bounds should be contained in (or equal to) parent bounds
                Assert.True(node.Bounds.Min.X <= left.Bounds.Min.X + 1e-10);
                Assert.True(node.Bounds.Min.Y <= left.Bounds.Min.Y + 1e-10);
                Assert.True(node.Bounds.Min.Z <= left.Bounds.Min.Z + 1e-10);
                Assert.True(node.Bounds.Max.X >= left.Bounds.Max.X - 1e-10);
                Assert.True(node.Bounds.Max.Y >= left.Bounds.Max.Y - 1e-10);
                Assert.True(node.Bounds.Max.Z >= left.Bounds.Max.Z - 1e-10);

                Assert.True(node.Bounds.Min.X <= right.Bounds.Min.X + 1e-10);
                Assert.True(node.Bounds.Max.X >= right.Bounds.Max.X - 1e-10);
            }
        }
    }

    [Fact]
    public void EmptyMesh_NoBvhNodes()
    {
        var mesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }

    [Fact]
    public void SingleTriangleMesh_OneNode()
    {
        var tris = new List<Triangle3>
        {
            new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var mesh = new MdCsg.Mesh.MeshBuilder().Build(tris);
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount >= 1);
        Assert.True(bvh.Nodes[0].IsLeaf);
        Assert.Equal(1, bvh.Nodes[0].PrimitiveCount);
    }

    [Fact]
    public void NodeCount_ReasonableForCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // 12 faces, BVH should have a bounded number of nodes
        // At most 2*n-1 interior nodes for n leaves
        Assert.True(bvh.NodeCount <= 2 * mesh.Faces.Count);
        Assert.True(bvh.NodeCount >= 1);
    }

    [Fact]
    public void FaceIndices_IsPermutation()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Face indices should be a permutation of [0, faceCount)
        var seen = new HashSet<int>();
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            seen.Add(bvh.FaceIndices[i]);
        }
        Assert.Equal(mesh.Faces.Count, seen.Count);
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            Assert.Contains(i, seen);
        }
    }

    [Fact]
    public void Tetrahedron_BvhBuilds()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Sphere_BvhNodeCount_IncreasesWithFaces()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh;
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh1 = BvhTree.Build(s1);
        var bvh2 = BvhTree.Build(s2);
        Assert.True(bvh2.NodeCount > bvh1.NodeCount);
    }
}
