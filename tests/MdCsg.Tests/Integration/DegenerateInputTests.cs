using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

public class DegenerateInputTests
{
    [Fact]
    public void IdenticalCubes_Union_ProducesResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube();

        // Identical cubes: coplanar faces, this is a degenerate case
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void IdenticalCubes_Intersection_ProducesResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube();

        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SharedEdge_Union_Works()
    {
        // Two cubes sharing an edge (touching but not overlapping)
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(1, 1, 0));

        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }
}
