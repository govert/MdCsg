using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree — Query correctness, RayCastCount validation, face index mapping, empty tree handling</summary>
public class BvhTreeQueryCorrectnessPropertyTests
{
    [Fact]
    public void Query_CubeWithContainingBox_ReturnsAllFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var queryBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(5, 5, 5));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Equal(12, results.Count); // cube has 12 faces
    }

    [Fact]
    public void Query_CubeWithDisjointBox_ReturnsNothing()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var queryBox = new Aabb(new Vec3(100, 100, 100), new Vec3(200, 200, 200));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Equal(0, results.Count);
    }

    [Fact]
    public void Query_CubeWithSmallBox_ReturnsSubset()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        // Small box touching one corner
        var queryBox = new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, 0.1, 0.1));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0, "Should find faces near corner");
        Assert.True(results.Count < 12, "Should not return all faces");
    }

    [Fact]
    public void Query_ResultFaceIndices_AreValid()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var queryBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(5, 5, 5));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        foreach (var idx in results)
        {
            Assert.True(idx >= 0 && idx < cube.Mesh.Faces.Count,
                $"Face index {idx} out of range [0, {cube.Mesh.Faces.Count})");
        }
    }

    [Fact]
    public void Query_NoFalseNegatives_BruteForceVerification()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var queryBox = new Aabb(new Vec3(0.5, 0.5, 0.5), new Vec3(1.5, 1.5, 1.5));

        var bvhResults = new List<int>();
        bvh.Query(queryBox, bvhResults);
        var bvhSet = new HashSet<int>(bvhResults);

        // Brute-force: check all faces
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
        {
            cube.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            var faceBounds = Aabb.FromTriangle(a, b, c);
            if (faceBounds.Overlaps(queryBox))
            {
                Assert.True(bvhSet.Contains(i),
                    $"BVH query missed face {i} which overlaps query box");
            }
        }
    }

    [Fact]
    public void RayCastCount_RayInside_Cube_IsOdd()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 1, $"Point inside cube should have odd hit count, got {hits}");
    }

    [Fact]
    public void RayCastCount_RayOutside_Cube_IsEven()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bvh = cube.Bvh;
        var ray = new Ray(new Vec3(10, 10, 10), new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0, $"Point outside cube should have even hit count, got {hits}");
    }

    [Fact]
    public void RayCastCount_EmptyMesh_ZeroHits()
    {
        var solid = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var bvh = solid.Bvh;
        var ray = new Ray(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void Query_EmptyMesh_ReturnsNothing()
    {
        var solid = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var bvh = solid.Bvh;
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10)), results);
        Assert.Equal(0, results.Count);
    }

    [Fact]
    public void NodeCount_Cube_GreaterThanZero()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(cube.Bvh.NodeCount > 0);
    }

    [Fact]
    public void NodeCount_EmptyMesh_IsZero()
    {
        var solid = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Equal(0, solid.Bvh.NodeCount);
    }

    [Fact]
    public void Mesh_Property_ReturnsSameMesh()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Same(cube.Mesh, cube.Bvh.Mesh);
    }

    [Fact]
    public void GetFaceIndex_ValidRange_ReturnsValidIndex()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        // Walk tree to find a leaf
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
            {
                for (int j = 0; j < nodes[i].PrimitiveCount; j++)
                {
                    int faceIdx = bvh.GetFaceIndex(nodes[i].LeftOrStart + j);
                    Assert.True(faceIdx >= 0 && faceIdx < cube.Mesh.Faces.Count);
                }
                return; // found and checked one leaf
            }
        }
    }

    [Fact]
    public void Query_SphereWithContainingBox_ReturnsAllFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var bvh = sphere.Bvh;
        var queryBox = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Equal(80, results.Count); // sphere subdivision 1 has 80 faces
    }

    [Fact]
    public void RayCastCount_MultipleRays_InsideSphere_AllOdd()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var bvh = sphere.Bvh;
        var directions = new[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized
        };
        foreach (var dir in directions)
        {
            var ray = new Ray(new Vec3(0.1, 0.1, 0.1), dir);
            int hits = bvh.RayCastCount(ray);
            Assert.True(hits % 2 == 1,
                $"Point inside sphere should have odd hit count for direction {dir}, got {hits}");
        }
    }

    [Fact]
    public void Query_RepeatedCalls_SameResults()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var queryBox = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));

        var results1 = new List<int>();
        var results2 = new List<int>();
        bvh.Query(queryBox, results1);
        bvh.Query(queryBox, results2);

        Assert.Equal(results1.Count, results2.Count);
        results1.Sort();
        results2.Sort();
        Assert.Equal(results1, results2);
    }

    [Fact]
    public void RayCastCount_Deterministic()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), Vec3.UnitX);
        int hits1 = bvh.RayCastCount(ray);
        int hits2 = bvh.RayCastCount(ray);
        Assert.Equal(hits1, hits2);
    }

    [Fact]
    public void FaceIndices_AllInRange()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var faceIndices = bvh.FaceIndices;
        for (int i = 0; i < faceIndices.Length; i++)
        {
            Assert.True(faceIndices[i] >= 0 && faceIndices[i] < cube.Mesh.Faces.Count,
                $"FaceIndex[{i}] = {faceIndices[i]} out of range");
        }
    }

    [Fact]
    public void FaceIndices_Length_EqualsFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(cube.Mesh.Faces.Count, cube.Bvh.FaceIndices.Length);
    }
}
