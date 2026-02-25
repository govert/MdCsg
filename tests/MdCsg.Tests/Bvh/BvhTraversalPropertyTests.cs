using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal — FindOverlappingPairs dual-tree traversal</summary>
public class BvhTraversalPropertyTests
{
    [Fact]
    public void FindOverlappingPairs_DisjointCubes_NoPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FindOverlappingPairs_OverlappingCubes_HasPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_IdenticalCubes_MaxPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
        // Each face of A can overlap with multiple faces of B
    }

    [Fact]
    public void FindOverlappingPairs_ContainedCube_HasPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_FaceIndicesValid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        foreach (var (faceA, faceB) in pairs)
        {
            Assert.True(faceA >= 0 && faceA < a.Mesh.Faces.Count);
            Assert.True(faceB >= 0 && faceB < b.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void FindOverlappingPairs_EmptyBvh_NoPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, empty.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FindOverlappingPairs_BothEmpty_NoPairs()
    {
        var a = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var b = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FindOverlappingPairs_CubeSphere_HasPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(Vec3.Zero, 1.5, 2);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_Symmetric()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var pairsAB = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        var pairsBA = BvhTraversal.FindOverlappingPairs(b.Bvh, a.Bvh);
        // Same number of pairs regardless of order (swapped indices)
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void FindOverlappingPairs_FarApart_NoPairs()
    {
        var a = MeshFactory.CreateCube(new Vec3(-1000, 0, 0), 1.0);
        var b = MeshFactory.CreateCube(new Vec3(1000, 0, 0), 1.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void FindOverlappingPairs_SphereSphere_HasPairs()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
    }
}
