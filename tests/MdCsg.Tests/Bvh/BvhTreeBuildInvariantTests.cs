using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree.Build — structural invariants, node bounds containment, leaf primitive counts</summary>
public class BvhTreeBuildInvariantTests
{
    [Fact]
    public void EmptyMesh_ZeroNodes()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }

    [Fact]
    public void SingleTriangle_SingleLeaf()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount >= 1);
        Assert.True(bvh.Nodes[0].IsLeaf);
        Assert.Equal(1, bvh.Nodes[0].PrimitiveCount);
    }

    [Fact]
    public void Cube_RootBoundsContainAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
        {
            cube.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            Assert.True(rootBounds.Contains(a), $"Root should contain vertex A of face {i}");
            Assert.True(rootBounds.Contains(b), $"Root should contain vertex B of face {i}");
            Assert.True(rootBounds.Contains(c), $"Root should contain vertex C of face {i}");
        }
    }

    [Fact]
    public void Cube_InteriorNodeChildBoundsContainedByParent()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf) continue;
            var left = bvh.Nodes[node.LeftOrStart];
            var right = bvh.Nodes[node.Right];
            // Parent bounds should contain both children
            Assert.True(node.Bounds.Contains(left.Bounds.Min),
                $"Parent {i} should contain left child min");
            Assert.True(node.Bounds.Contains(left.Bounds.Max),
                $"Parent {i} should contain left child max");
            Assert.True(node.Bounds.Contains(right.Bounds.Min),
                $"Parent {i} should contain right child min");
            Assert.True(node.Bounds.Contains(right.Bounds.Max),
                $"Parent {i} should contain right child max");
        }
    }

    [Fact]
    public void Sphere_InteriorNodeChildBoundsContainedByParent()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf) continue;
            var left = bvh.Nodes[node.LeftOrStart];
            var right = bvh.Nodes[node.Right];
            Assert.True(node.Bounds.Contains(left.Bounds.Min));
            Assert.True(node.Bounds.Contains(left.Bounds.Max));
            Assert.True(node.Bounds.Contains(right.Bounds.Min));
            Assert.True(node.Bounds.Contains(right.Bounds.Max));
        }
    }

    [Fact]
    public void Cube_LeafPrimitiveCountsPositive()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                Assert.True(bvh.Nodes[i].PrimitiveCount > 0,
                    $"Leaf {i} should have positive primitive count");
        }
    }

    [Fact]
    public void Cube_LeafPrimitiveCountsAtMost4()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                Assert.True(bvh.Nodes[i].PrimitiveCount <= 4,
                    $"Leaf {i} has {bvh.Nodes[i].PrimitiveCount} primitives, max is 4");
        }
    }

    [Fact]
    public void Sphere_TotalLeafPrimitiveCount_EqualsFaceCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
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
    public void Cube_TotalLeafPrimitiveCount_EqualsFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                totalPrimitives += bvh.Nodes[i].PrimitiveCount;
        }
        Assert.Equal(cube.Mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void Tetrahedron_TotalLeafPrimitiveCount_EqualsFaceCount()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var bvh = BvhTree.Build(tet.Mesh);
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (bvh.Nodes[i].IsLeaf)
                totalPrimitives += bvh.Nodes[i].PrimitiveCount;
        }
        Assert.Equal(tet.Mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void FaceIndicesSpan_ContainsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var indices = bvh.FaceIndices;
        var set = new HashSet<int>();
        for (int i = 0; i < indices.Length; i++)
            set.Add(indices[i]);
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
            Assert.Contains(i, set);
    }

    [Fact]
    public void FaceIndicesSpan_Length_EqualsFaceCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var bvh = BvhTree.Build(sphere.Mesh);
        Assert.Equal(sphere.Mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void InteriorNodes_HaveZeroPrimitiveCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
                Assert.Equal(0, node.PrimitiveCount);
        }
    }

    [Fact]
    public void InteriorNodes_ChildIndicesInRange()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf) continue;
            Assert.True(node.LeftOrStart >= 0 && node.LeftOrStart < bvh.NodeCount,
                $"Left child index {node.LeftOrStart} out of range for node {i}");
            Assert.True(node.Right >= 0 && node.Right < bvh.NodeCount,
                $"Right child index {node.Right} out of range for node {i}");
        }
    }

    [Fact]
    public void LeafNodes_FaceIndicesInRange()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf) continue;
            for (int j = 0; j < node.PrimitiveCount; j++)
            {
                int faceIdx = bvh.GetFaceIndex(node.LeftOrStart + j);
                Assert.True(faceIdx >= 0 && faceIdx < cube.Mesh.Faces.Count,
                    $"Face index {faceIdx} out of range in leaf {i}");
            }
        }
    }

    [Fact]
    public void NodeCount_GreaterThanZero_ForNonEmptyMesh()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.True(bvh.NodeCount > 0);
    }
}
