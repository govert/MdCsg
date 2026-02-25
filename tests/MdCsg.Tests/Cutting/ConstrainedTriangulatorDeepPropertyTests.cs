using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator — projection, ear-clipping, constraint enforcement, deeper tests</summary>
public class ConstrainedTriangulatorDeepPropertyTests
{
    [Fact]
    public void GetDominantAxis_ZNormal_Returns2()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 0, 1)));
    }

    [Fact]
    public void GetDominantAxis_XNormal_Returns0()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 0, 0)));
    }

    [Fact]
    public void GetDominantAxis_YNormal_Returns1()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 1, 0)));
    }

    [Fact]
    public void GetDominantAxis_NegativeZ_Returns2()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 0, -5)));
    }

    [Fact]
    public void GetDominantAxis_Diagonal_ReturnsLargest()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(0.1, 0.2, 0.9)));
    }

    [Fact]
    public void ProjectTo2D_DropX_GetsYZ()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 0);
        Assert.Equal(new Vec2(2, 3), result);
    }

    [Fact]
    public void ProjectTo2D_DropY_GetsXZ()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 1);
        Assert.Equal(new Vec2(1, 3), result);
    }

    [Fact]
    public void ProjectTo2D_DropZ_GetsXY()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 2);
        Assert.Equal(new Vec2(1, 2), result);
    }

    [Fact]
    public void Triangulate_ThreeVertices_OneTriangle()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var constraints = Array.Empty<(int, int)>();
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.Single(result);
        Assert.Equal((0, 1, 2), result[0]);
    }

    [Fact]
    public void Triangulate_LessThanThreeVertices_Empty()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitY);
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_FourVertices_TwoTriangles()
    {
        // Square in XY plane
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_FourVertices_AllIndicesValid()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < 4);
            Assert.True(b >= 0 && b < 4);
            Assert.True(c >= 0 && c < 4);
        }
    }

    [Fact]
    public void Triangulate_WithConstraint_ProducesTriangles()
    {
        // Triangle with a midpoint and constraint from edge to midpoint
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(0, 4, 0),
            new(2, 2, 0) // midpoint of hypotenuse
        };
        var constraints = new (int, int)[] { (0, 3) }; // Constraint from origin to midpoint
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 2, $"Should produce at least 2 triangles, got {result.Count}");
    }

    [Fact]
    public void Triangulate_WithConstraint_AllIndicesValid()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(0, 4, 0),
            new(2, 2, 0)
        };
        var constraints = new (int, int)[] { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < 4);
            Assert.True(b >= 0 && b < 4);
            Assert.True(c >= 0 && c < 4);
        }
    }

    [Fact]
    public void Triangulate_FiveVertices_ThreeTriangles()
    {
        // Pentagon-like shape
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(1, 3, 0), new(0, 2, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Triangulate_ReversedWinding_StillProducesTriangles()
    {
        // CW winding triangle — should handle flip
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitY, Vec3.UnitX }; // CW in XY plane
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_YZPlane_ProjectsCorrectly()
    {
        // Triangle in YZ plane (normal along X)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(1, 0, 0));
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_XZPlane_ProjectsCorrectly()
    {
        // Triangle in XZ plane (normal along Y)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 1, 0));
        Assert.Single(result);
    }
}
