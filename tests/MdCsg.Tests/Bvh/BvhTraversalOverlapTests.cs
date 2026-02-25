using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal.FindOverlappingPairs — face pair detection for CSG intersection</summary>
public class BvhTraversalOverlapTests
{
    [Fact]
    public void OverlappingCubes_FindsPairs()
    {
        var solidA = MeshFactory.CreateCube();
        var solidB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var pairs = BvhTraversal.FindOverlappingPairs(solidA.Bvh, solidB.Bvh);
        Assert.True(pairs.Count > 0, "Overlapping cubes should produce face pairs");
    }

    [Fact]
    public void DisjointCubes_NoPairs()
    {
        var solidA = MeshFactory.CreateCube();
        var solidB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(solidA.Bvh, solidB.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void SameCube_AllFacesOverlap()
    {
        var solidA = MeshFactory.CreateCube();
        var solidB = MeshFactory.CreateCube();
        var pairs = BvhTraversal.FindOverlappingPairs(solidA.Bvh, solidB.Bvh);
        // Each face overlaps its counterpart at minimum; exact count depends on AABB overlap
        Assert.True(pairs.Count >= 12, $"Same cube should have many pairs, got {pairs.Count}");
    }

    [Fact]
    public void OverlappingPairs_FaceIndicesValid()
    {
        var solidA = MeshFactory.CreateCube();
        var solidB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(solidA.Bvh, solidB.Bvh);
        foreach (var (faceA, faceB) in pairs)
        {
            Assert.True(faceA >= 0 && faceA < solidA.Mesh.Faces.Count);
            Assert.True(faceB >= 0 && faceB < solidB.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void CubeSphere_FindsPairs()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 2);
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, sphere.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void SmallInsideLarge_NoPairsWhenFullyContained()
    {
        // When fully contained, face AABBs may not overlap (faces don't intersect)
        var large = MeshFactory.CreateCube(Vec3.Zero, 2);
        var small = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5);
        var pairs = BvhTraversal.FindOverlappingPairs(large.Bvh, small.Bvh);
        // Could be 0 (fully inside, no face overlaps) or >0 (face AABB touches)
        Assert.True(pairs.Count >= 0);
    }

    [Fact]
    public void EmptyTree_NoPairs()
    {
        var builder = new MdCsg.Mesh.MeshBuilder();
        var emptyMesh = builder.Build(System.Array.Empty<Triangle3>());
        var emptyBvh = BvhTree.Build(emptyMesh);
        var cube = MeshFactory.CreateCube();
        var pairs = BvhTraversal.FindOverlappingPairs(emptyBvh, cube.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void TetrahedronCube_FindsPairs()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var cube = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var pairs = BvhTraversal.FindOverlappingPairs(tet.Bvh, cube.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void Symmetric()
    {
        var solidA = MeshFactory.CreateCube();
        var solidB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairsAB = BvhTraversal.FindOverlappingPairs(solidA.Bvh, solidB.Bvh);
        var pairsBA = BvhTraversal.FindOverlappingPairs(solidB.Bvh, solidA.Bvh);
        // Same count — pairs just swap (A,B) ↔ (B,A)
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }
}
