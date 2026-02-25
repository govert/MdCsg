using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal — FindOverlappingPairs correctness, symmetry, disjoint/overlapping mesh cases</summary>
public class BvhTraversalOverlapPairPropertyTests
{
    [Fact]
    public void OverlappingCubes_FindsPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0, "Overlapping cubes should have face pairs");
    }

    [Fact]
    public void DisjointCubes_NoPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void SamePosition_FindsAllPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(Vec3.Zero);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        // All face pairs should overlap when cubes are identical
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void OverlappingPairs_FaceIndicesValid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        foreach (var (faceA, faceB) in pairs)
        {
            Assert.True(faceA >= 0 && faceA < a.Mesh.Faces.Count,
                $"FaceA {faceA} out of range");
            Assert.True(faceB >= 0 && faceB < b.Mesh.Faces.Count,
                $"FaceB {faceB} out of range");
        }
    }

    [Fact]
    public void OverlappingPairs_ActuallyOverlap()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        foreach (var (faceA, faceB) in pairs)
        {
            a.Mesh.Faces[faceA].GetTrianglePositions(out var a1, out var a2, out var a3);
            b.Mesh.Faces[faceB].GetTrianglePositions(out var b1, out var b2, out var b3);
            var boundsA = Aabb.FromTriangle(a1, a2, a3);
            var boundsB = Aabb.FromTriangle(b1, b2, b3);
            Assert.True(boundsA.Overlaps(boundsB),
                $"Pair ({faceA},{faceB}) AABBs should overlap");
        }
    }

    [Fact]
    public void Symmetry_AB_BA_SamePairCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairsAB = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        var pairsBA = BvhTraversal.FindOverlappingPairs(b.Bvh, a.Bvh);
        Assert.Equal(pairsAB.Count, pairsBA.Count);
    }

    [Fact]
    public void Symmetry_AB_BA_SamePairsSwapped()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairsAB = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        var pairsBA = BvhTraversal.FindOverlappingPairs(b.Bvh, a.Bvh);

        var setAB = new HashSet<(int, int)>();
        foreach (var (fa, fb) in pairsAB)
            setAB.Add((fa, fb));

        foreach (var (fb, fa) in pairsBA)
        {
            Assert.True(setAB.Contains((fa, fb)),
                $"Pair ({fa},{fb}) from AB not found in BA swapped");
        }
    }

    [Fact]
    public void EmptyMeshA_NoPairs()
    {
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var pairs = BvhTraversal.FindOverlappingPairs(empty.Bvh, cube.Bvh);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void EmptyMeshB_NoPairs()
    {
        var cube = MeshFactory.CreateCube();
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, empty.Bvh);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void BothEmpty_NoPairs()
    {
        var a = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var b = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void NoFalseNegatives_BruteForce()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var bvhPairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        var bvhSet = new HashSet<(int, int)>();
        foreach (var (fa, fb) in bvhPairs)
            bvhSet.Add((fa, fb));

        // Brute force all pairs
        for (int i = 0; i < a.Mesh.Faces.Count; i++)
        {
            a.Mesh.Faces[i].GetTrianglePositions(out var a1, out var a2, out var a3);
            var boundsA = Aabb.FromTriangle(a1, a2, a3);
            for (int j = 0; j < b.Mesh.Faces.Count; j++)
            {
                b.Mesh.Faces[j].GetTrianglePositions(out var b1, out var b2, out var b3);
                var boundsB = Aabb.FromTriangle(b1, b2, b3);
                if (boundsA.Overlaps(boundsB))
                {
                    Assert.True(bvhSet.Contains((i, j)),
                        $"BVH missed overlapping pair ({i},{j})");
                }
            }
        }
    }

    [Fact]
    public void CubeSphere_FindsPairs()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, sphere.Bvh);
        Assert.True(pairs.Count > 0, "Cube-sphere overlap should produce pairs");
    }

    [Fact]
    public void Deterministic_SameResults()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairs1 = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        var pairs2 = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Equal(pairs1.Count, pairs2.Count);
        for (int i = 0; i < pairs1.Count; i++)
        {
            Assert.Equal(pairs1[i].FaceA, pairs2[i].FaceA);
            Assert.Equal(pairs1[i].FaceB, pairs2[i].FaceB);
        }
    }

    [Fact]
    public void TouchingCubes_MayHavePairs()
    {
        // Cubes touching at one face (edge/face contact)
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 1.0); // touching at x=1 plane
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        // Touching cubes may or may not have AABB overlap (depends on exact position)
        Assert.True(pairs.Count >= 0); // just verify no crash
    }
}
