using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator — GetDominantAxis, ProjectTo2D, Triangulate with/without constraints</summary>
public class ConstrainedTriangulatorPropertyTests
{
    [Fact]
    public void GetDominantAxis_XDominant()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(10, 1, 1)));
    }

    [Fact]
    public void GetDominantAxis_YDominant()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 10, 1)));
    }

    [Fact]
    public void GetDominantAxis_ZDominant()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, 10)));
    }

    [Fact]
    public void GetDominantAxis_NegativeValues()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 0, -5)));
    }

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 0);
        Assert.Equal(2, result.X);
        Assert.Equal(3, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 1);
        Assert.Equal(1, result.X);
        Assert.Equal(3, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 2);
        Assert.Equal(1, result.X);
        Assert.Equal(2, result.Y);
    }

    [Fact]
    public void Triangulate_SingleTriangle_ReturnsOneTriangle()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Single(result);
        Assert.Equal((0, 1, 2), result[0]);
    }

    [Fact]
    public void Triangulate_LessThanThree_ReturnsEmpty()
    {
        var verts = new List<Vec3> { new(0, 0, 0), new(1, 0, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_FourPoints_NoConstraints_TwoTriangles()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0.5, 0.3, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        // 4 points in 2D should produce 2 triangles
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_WithConstraint_ConstraintEdgeExists()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
            new(0.5, 0.0, 0), new(0.25, 0.5, 0)
        };
        var constraints = new List<(int, int)> { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));

        // The constraint edge (3->4 or 4->3) should appear in the triangulation
        bool found = false;
        foreach (var (a, b, c) in result)
        {
            if ((a == 3 && b == 4) || (b == 3 && a == 4) ||
                (b == 3 && c == 4) || (c == 3 && b == 4) ||
                (c == 3 && a == 4) || (a == 3 && c == 4))
            {
                found = true;
                break;
            }
        }
        Assert.True(found, "Constraint edge should appear in triangulation");
    }

    [Fact]
    public void Triangulate_AllTrianglesUseSameVertexPool()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(2, 0, 0), new(0, 2, 0),
            new(1, 0, 0), new(0.5, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));

        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Count);
            Assert.True(b >= 0 && b < verts.Count);
            Assert.True(c >= 0 && c < verts.Count);
        }
    }

    [Fact]
    public void Triangulate_VerticalFace_ZDominant_Works()
    {
        // Face in XZ plane (Y-dominant normal)
        var verts = new List<Vec3>
        {
            new(0, 5, 0), new(1, 5, 0), new(0, 5, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 1, 0));
        Assert.Single(result);
    }
}
