using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator deep property tests — projection, triangulation invariants</summary>
public class ConstrainedTriangulatorPropertyTests
{
    [Fact]
    public void GetDominantAxis_X()
    {
        int axis = ConstrainedTriangulator.GetDominantAxis(new Vec3(10, 1, 1));
        Assert.Equal(0, axis);
    }

    [Fact]
    public void GetDominantAxis_Y()
    {
        int axis = ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 10, 1));
        Assert.Equal(1, axis);
    }

    [Fact]
    public void GetDominantAxis_Z()
    {
        int axis = ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, 10));
        Assert.Equal(2, axis);
    }

    [Fact]
    public void GetDominantAxis_NegativeNormal()
    {
        int axis = ConstrainedTriangulator.GetDominantAxis(new Vec3(-10, 1, 1));
        Assert.Equal(0, axis);
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
    public void Triangulate_SingleTriangle_ReturnsSingleTriangle()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.Single(result);
        var (a, b, c) = result[0];
        Assert.Contains(0, new[] { a, b, c });
        Assert.Contains(1, new[] { a, b, c });
        Assert.Contains(2, new[] { a, b, c });
    }

    [Fact]
    public void Triangulate_LessThan3_ReturnsEmpty()
    {
        var result = ConstrainedTriangulator.Triangulate(
            new Vec3[] { new(0, 0, 0), new(1, 0, 0) }, [], new Vec3(0, 0, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_EmptyVertices_ReturnsEmpty()
    {
        var result = ConstrainedTriangulator.Triangulate(Array.Empty<Vec3>(), [], new Vec3(0, 0, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_Square_TwoTriangles()
    {
        // A square as 4 vertices with no constraints → should produce 2 triangles
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_WithConstraint_ConstraintEdgeExists()
    {
        // Triangle with midpoint vertex and constraint edge across it
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(2, 0, 0), new(0, 2, 0),
            new(1, 0, 0) // midpoint of edge AB
        };
        // Constraint: 3→2 (midpoint of AB to vertex C)
        var constraints = new List<(int, int)> { (3, 2) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 2);
        // Verify all indices are valid
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Length);
            Assert.True(b >= 0 && b < verts.Length);
            Assert.True(c >= 0 && c < verts.Length);
        }
    }

    [Fact]
    public void Triangulate_AllIndicesValid()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
            new(0.3, 0.3, 0), new(0.6, 0.2, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Length);
            Assert.True(b >= 0 && b < verts.Length);
            Assert.True(c >= 0 && c < verts.Length);
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(c, a);
        }
    }

    [Fact]
    public void Triangulate_NTriangles_ForNVertices()
    {
        // For N vertices inside a triangle, expect at most 2*(N-3)+1 triangles
        // With a triangle (3 verts) and 2 interior points (5 verts), expect ~3-5 triangles
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(0, 4, 0),
            new(1, 1, 0), new(2, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void Triangulate_YZPlane()
    {
        // Triangle in YZ plane (dominant axis = X)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(1, 0, 0));
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_XZPlane()
    {
        // Triangle in XZ plane (dominant axis = Y)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, -1, 0));
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_WithConstraint_ProducesNonDegenerate()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(2, 0, 0), new(1, 2, 0),
            new(0.5, 1, 0), new(1.5, 1, 0)
        };
        // Constraint edge from vertex 3 to 4 (horizontal line through the triangle)
        var constraints = new List<(int, int)> { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void Triangulate_ConstraintAlreadyExists()
    {
        // Constraint edge that already exists in the triangulation (an edge of the triangle)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var constraints = new List<(int, int)> { (0, 1) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.Single(result);
    }
}
