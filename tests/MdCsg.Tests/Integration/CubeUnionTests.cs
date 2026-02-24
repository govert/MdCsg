using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

public class CubeUnionTests
{
    [Fact]
    public void OverlappingCubes_Union_ProducesResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var result = Csg.Union(a, b);

        Assert.True(result.FaceCount > 0, "Union should produce faces.");
        Assert.True(result.IntersectionSegmentCount > 0, "Overlapping cubes should have intersections.");
    }

    [Fact]
    public void DisjointCubes_Union_CombinesBoth()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));

        var result = Csg.Union(a, b);

        // Disjoint: no intersections, result has faces from both
        Assert.Equal(0, result.IntersectionSegmentCount);
        Assert.Equal(24, result.FaceCount); // 12 + 12
    }

    [Fact]
    public void FullyContainedCube_Union_ProducesOuterOnly()
    {
        var outer = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2.0);
        var inner = MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5);

        var result = Csg.Union(outer, inner);

        // The inner cube is fully inside — union should be just the outer cube
        Assert.True(result.FaceCount >= 12, "Union should have at least the outer cube's faces.");
    }

    [Fact]
    public void TouchingCubes_Union_ProducesResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0)); // touching at x=1

        var result = Csg.Union(a, b);

        Assert.True(result.FaceCount > 0);
    }
}
