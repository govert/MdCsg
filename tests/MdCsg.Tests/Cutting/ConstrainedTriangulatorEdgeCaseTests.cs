using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator edge cases</summary>
public class ConstrainedTriangulatorEdgeCaseTests
{
    [Fact]
    public void GetDominantAxis_X()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(5, 1, 2)));
    }

    [Fact]
    public void GetDominantAxis_Y()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 5, 2)));
    }

    [Fact]
    public void GetDominantAxis_Z()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 2, 5)));
    }

    [Fact]
    public void GetDominantAxis_NegativeComponents()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, -5, 2)));
    }

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(10, 20, 30), 0);
        Assert.Equal(20, result.X);
        Assert.Equal(30, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(10, 20, 30), 1);
        Assert.Equal(10, result.X);
        Assert.Equal(30, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(10, 20, 30), 2);
        Assert.Equal(10, result.X);
        Assert.Equal(20, result.Y);
    }

    [Fact]
    public void Triangulate_ExactlyThreeVertices_NoConstraints()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.Single(result);
        Assert.Equal((0, 1, 2), result[0]);
    }

    [Fact]
    public void Triangulate_FourVertices_NoConstraints()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.3, 0.3, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        // Should produce triangulation with all 4 vertices
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_WithConstraint()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.5, 0, 0), new(0, 0.5, 0)
        };
        var constraints = new List<(int, int)> { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void Triangulate_ConstraintPreserved()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.5, 0, 0), new(0, 0.5, 0)
        };
        var constraints = new List<(int, int)> { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        // The constraint edge (3,4) should appear in the triangulation
        bool foundConstraint = result.Any(t =>
            (t.A == 3 && t.B == 4) || (t.B == 3 && t.C == 4) || (t.C == 3 && t.A == 4) ||
            (t.A == 4 && t.B == 3) || (t.B == 4 && t.C == 3) || (t.C == 4 && t.A == 3));
        Assert.True(foundConstraint, "Constraint edge (3,4) should be in triangulation");
    }

    [Fact]
    public void Triangulate_LessThan3Vertices_Empty()
    {
        var verts = new List<Vec3> { new(0, 0, 0), new(1, 0, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_AllTrianglesUseValidIndices()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.3, 0.3, 0), new(0.7, 0.1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Count);
            Assert.True(b >= 0 && b < verts.Count);
            Assert.True(c >= 0 && c < verts.Count);
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(a, c);
        }
    }

    [Fact]
    public void Triangulate_MultipleConstraints()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(2, 0, 0), new(0, 2, 0),
            new(1, 0, 0), new(0, 1, 0), new(0.5, 0.5, 0)
        };
        var constraints = new List<(int, int)> { (3, 4), (3, 5) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 2, $"Expected >= 2, got {result.Count}");
    }

    [Fact]
    public void Triangulate_XYPlane_Normal_Z()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.5, 0.5, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_XZPlane_Normal_Y()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1), new(0.5, 0, 0.5)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 1, 0));
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_YZPlane_Normal_X()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(0, 1, 0), new(0, 0, 1), new(0, 0.5, 0.5)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(1, 0, 0));
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_VertexOnEdge()
    {
        // Point on edge A-B of original triangle
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.5, 0, 0) // on A-B
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_VertexAtCorner()
    {
        // Point very close to vertex A (should be handled without crash)
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1e-12, 1e-12, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], new Vec3(0, 0, 1));
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void Triangulate_SixVertices_MultipleConstraints()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(3, 0, 0), new(0, 3, 0),
            new(1, 0, 0), new(2, 0, 0),
            new(0, 1, 0)
        };
        var constraints = new List<(int, int)> { (3, 5), (4, 5) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 4);
    }
}
