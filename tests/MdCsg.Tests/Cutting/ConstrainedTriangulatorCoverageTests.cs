using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Code coverage: ConstrainedTriangulator internal paths</summary>
public class ConstrainedTriangulatorCoverageTests
{
    [Fact]
    public void GetDominantAxis_X()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(10, 1, 1)));
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(-10, 1, 1)));
    }

    [Fact]
    public void GetDominantAxis_Y()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 10, 1)));
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, -10, 1)));
    }

    [Fact]
    public void GetDominantAxis_Z()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, 10)));
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, -10)));
    }

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var p = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 0);
        Assert.Equal(2, result.X);
        Assert.Equal(3, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var p = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 1);
        Assert.Equal(1, result.X);
        Assert.Equal(3, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var p = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 2);
        Assert.Equal(1, result.X);
        Assert.Equal(2, result.Y);
    }

    [Fact]
    public void Triangulate_LessThan3Vertices_ReturnsEmpty()
    {
        var result = ConstrainedTriangulator.Triangulate(
            new List<Vec3> { new Vec3(0, 0, 0), new Vec3(1, 0, 0) },
            new List<(int, int)>(),
            Vec3.UnitZ);
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_ExactlyThreeVertices_SingleTriangle()
    {
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), Vec3.UnitZ);
        Assert.Single(result);
        Assert.Equal((0, 1, 2), result[0]);
    }

    [Fact]
    public void Triangulate_FourVertices_NoConstraints_EarClip()
    {
        // Square: 4 vertices, no constraints → ear clipping
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), Vec3.UnitZ);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_FourVertices_WithConstraint()
    {
        // Triangle with an interior point on edge
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(0, 2, 0),
            new Vec3(1, 0, 0) // midpoint of AB
        };
        var constraints = new List<(int, int)> { (3, 2) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_ReversedWinding_StillWorks()
    {
        // CW winding in XY → should detect and flip
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0, 1, 0),
            new Vec3(1, 0, 0),
            new Vec3(0.5, 0.5, 0) // interior point
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_VertexOnEdge_SplitsCorrectly()
    {
        // Original triangle plus a point on edge BC
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(4, 0, 0),
            new Vec3(0, 4, 0),
            new Vec3(2, 2, 0) // on edge BC
        };
        var constraints = new List<(int, int)> { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_MultipleConstraints()
    {
        // Triangle with two intersection segments
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(4, 0, 0),
            new Vec3(0, 4, 0),
            new Vec3(1, 0, 0), // on edge AB
            new Vec3(0, 1, 0), // on edge AC
            new Vec3(3, 0, 0), // on edge AB
            new Vec3(0, 3, 0), // on edge AC
        };
        var constraints = new List<(int, int)> { (3, 4), (5, 6) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 4);
    }

    [Fact]
    public void Triangulate_ProjectAlongYAxis()
    {
        // Triangle in XZ plane (Y-dominant normal)
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), Vec3.UnitY);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_ProjectAlongXAxis()
    {
        // Triangle in YZ plane (X-dominant normal)
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0, 1, 0),
            new Vec3(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), Vec3.UnitX);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_FiveVertices_EarClip()
    {
        // Pentagon-like shape
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(0, 2, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), Vec3.UnitZ);
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void Triangulate_ConstraintAlreadyExists()
    {
        // Constraint that is already an edge of the triangulation
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(0, 2, 0),
            new Vec3(1, 1, 0) // interior
        };
        // Constraint 0-1 is already the edge of the initial triangle
        var constraints = new List<(int, int)> { (0, 1) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }
}
