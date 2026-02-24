using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Bvh;

public class BvhTests
{
    private static HalfEdgeMesh BuildCubeMesh(Vec3 offset = default)
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0) + offset, new Vec3(1, 0, 0) + offset, new Vec3(1, 1, 0) + offset, new Vec3(0, 1, 0) + offset,
            new Vec3(0, 0, 1) + offset, new Vec3(1, 0, 1) + offset, new Vec3(1, 1, 1) + offset, new Vec3(0, 1, 1) + offset,
        };

        var triangles = new (int, int, int)[]
        {
            (0, 2, 1), (0, 3, 2),
            (4, 5, 6), (4, 6, 7),
            (0, 1, 5), (0, 5, 4),
            (2, 3, 7), (2, 7, 6),
            (0, 4, 7), (0, 7, 3),
            (1, 2, 6), (1, 6, 5),
        };

        return new MeshBuilder().Build(positions, triangles);
    }

    [Fact]
    public void Build_FromCube_HasCorrectNodeCount()
    {
        var mesh = BuildCubeMesh();
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void Query_OverlappingBox_FindsFaces()
    {
        var mesh = BuildCubeMesh();
        var bvh = BvhTree.Build(mesh);

        var queryBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2));
        var results = new List<int>();
        bvh.Query(queryBox, results);

        // All 12 faces should be found
        Assert.Equal(12, results.Count);
    }

    [Fact]
    public void Query_DisjointBox_FindsNothing()
    {
        var mesh = BuildCubeMesh();
        var bvh = BvhTree.Build(mesh);

        var queryBox = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        var results = new List<int>();
        bvh.Query(queryBox, results);

        Assert.Empty(results);
    }

    [Fact]
    public void RayCast_ThroughCube_HitsTwoFaces()
    {
        var mesh = BuildCubeMesh();
        var bvh = BvhTree.Build(mesh);

        // Ray from outside, through interior, exits other side
        // Offset from (0.5,0.5) to avoid hitting the diagonal edge between two triangles per face
        var ray = new Ray(new Vec3(0.3, 0.2, -1), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.Equal(2, hits); // enters and exits
    }

    [Fact]
    public void RayCast_MissesCube_HitsZero()
    {
        var mesh = BuildCubeMesh();
        var bvh = BvhTree.Build(mesh);

        var ray = new Ray(new Vec3(5, 5, 0), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.Equal(0, hits);
    }

    [Fact]
    public void DualTreeTraversal_OverlappingCubes_FindsPairs()
    {
        var meshA = BuildCubeMesh();
        var meshB = BuildCubeMesh(new Vec3(0.5, 0.5, 0.5));

        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);

        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void DualTreeTraversal_DisjointCubes_FindsNoPairs()
    {
        var meshA = BuildCubeMesh();
        var meshB = BuildCubeMesh(new Vec3(10, 10, 10));

        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);

        var pairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);
        Assert.Empty(pairs);
    }
}
