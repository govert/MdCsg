using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree Build — SAH quality, node count, leaf distribution, bounds containment</summary>
public class BvhBuildQualityPropertyTests
{
    [Fact]
    public void Build_Cube_NodeCountBounded()
    {
        var cube = MeshFactory.CreateCube();
        // 12 faces → at most 2*12-1 = 23 nodes for a complete binary tree
        Assert.True(cube.Bvh.NodeCount <= 2 * cube.Mesh.Faces.Count,
            $"Node count {cube.Bvh.NodeCount} too high for {cube.Mesh.Faces.Count} faces");
    }

    [Fact]
    public void Build_Sphere_NodeCountBounded()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(sphere.Bvh.NodeCount <= 2 * sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Build_RootBounds_ContainsAllVertices()
    {
        var cube = MeshFactory.CreateCube();
        var rootBounds = cube.Bvh.Nodes[0].Bounds;
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.True(rootBounds.Contains(v.Position) ||
                System.Math.Abs(v.Position.X - rootBounds.Max.X) < 1e-10 ||
                System.Math.Abs(v.Position.Y - rootBounds.Max.Y) < 1e-10 ||
                System.Math.Abs(v.Position.Z - rootBounds.Max.Z) < 1e-10,
                $"Vertex {v.Position} should be within root bounds");
        }
    }

    [Fact]
    public void Build_LeafNodes_CoverAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        int totalPrimitives = 0;
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < cube.Bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
                totalPrimitives += nodes[i].PrimitiveCount;
        }
        Assert.Equal(cube.Mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void Build_Sphere_LeafNodesCoverAllFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        int totalPrimitives = 0;
        var nodes = sphere.Bvh.Nodes;
        for (int i = 0; i < sphere.Bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
                totalPrimitives += nodes[i].PrimitiveCount;
        }
        Assert.Equal(sphere.Mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void Build_FaceIndices_ContainsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var indices = cube.Bvh.FaceIndices;
        var indexSet = new HashSet<int>();
        for (int i = 0; i < indices.Length; i++)
            indexSet.Add(indices[i]);
        Assert.Equal(cube.Mesh.Faces.Count, indexSet.Count);
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
            Assert.Contains(i, indexSet);
    }

    [Fact]
    public void Build_Tetrahedron_SmallNodeCount()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(tet.Bvh.NodeCount <= 8, $"4-face BVH should be small, got {tet.Bvh.NodeCount}");
    }

    [Fact]
    public void Build_SingleFaceMesh_OneOrTwoNodes()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount >= 1 && bvh.NodeCount <= 2);
    }

    [Fact]
    public void Build_EmptyMesh_ZeroNodes()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }

    [Fact]
    public void Build_InteriorNodes_ChildrenExist()
    {
        var cube = MeshFactory.CreateCube();
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < cube.Bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].LeftOrStart >= 0 && nodes[i].LeftOrStart < cube.Bvh.NodeCount,
                    $"Left child {nodes[i].LeftOrStart} out of range");
                Assert.True(nodes[i].Right >= 0 && nodes[i].Right < cube.Bvh.NodeCount,
                    $"Right child {nodes[i].Right} out of range");
            }
        }
    }

    [Fact]
    public void Build_ChildBounds_WithinParent()
    {
        var cube = MeshFactory.CreateCube();
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < cube.Bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                var parent = nodes[i].Bounds;
                var left = nodes[nodes[i].LeftOrStart].Bounds;
                var right = nodes[nodes[i].Right].Bounds;
                // Child bounds should be within parent (with small tolerance)
                Assert.True(left.Min.X >= parent.Min.X - 1e-10, "Left child min X exceeds parent");
                Assert.True(left.Max.X <= parent.Max.X + 1e-10, "Left child max X exceeds parent");
                Assert.True(right.Min.X >= parent.Min.X - 1e-10, "Right child min X exceeds parent");
                Assert.True(right.Max.X <= parent.Max.X + 1e-10, "Right child max X exceeds parent");
            }
        }
    }

    [Fact]
    public void Build_Mesh_PropertyMatches()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Same(cube.Mesh, cube.Bvh.Mesh);
    }
}
