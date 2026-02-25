using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator — GetDominantAxis, ProjectTo2D, basic triangulation</summary>
public class ConstrainedTriangulatorHelperTests
{
    // ── GetDominantAxis ──────────────────────────────────────────────────

    [Fact]
    public void GetDominantAxis_XDominant()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(5, 1, 2)));
    }

    [Fact]
    public void GetDominantAxis_YDominant()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 5, 2)));
    }

    [Fact]
    public void GetDominantAxis_ZDominant()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 2, 5)));
    }

    [Fact]
    public void GetDominantAxis_NegativeComponents()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(-1, -5, -2)));
    }

    [Fact]
    public void GetDominantAxis_EqualXY_ReturnsX()
    {
        // When ax == ay, X wins (first check: ax >= ay && ax >= az)
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(3, 3, 1)));
    }

    [Fact]
    public void GetDominantAxis_EqualYZ_ReturnsY()
    {
        // When ay == az and ax < ay, Y wins
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 3, 3)));
    }

    [Fact]
    public void GetDominantAxis_AllEqual_ReturnsX()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, 1)));
    }

    [Fact]
    public void GetDominantAxis_UnitX()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 0, 0)));
    }

    [Fact]
    public void GetDominantAxis_UnitY()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 1, 0)));
    }

    [Fact]
    public void GetDominantAxis_UnitZ()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 0, 1)));
    }

    // ── ProjectTo2D ──────────────────────────────────────────────────────

    [Fact]
    public void ProjectTo2D_DropX_ReturnYZ()
    {
        var p = new Vec3(10, 20, 30);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 0);
        Assert.Equal(new Vec2(20, 30), result);
    }

    [Fact]
    public void ProjectTo2D_DropY_ReturnXZ()
    {
        var p = new Vec3(10, 20, 30);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 1);
        Assert.Equal(new Vec2(10, 30), result);
    }

    [Fact]
    public void ProjectTo2D_DropZ_ReturnXY()
    {
        var p = new Vec3(10, 20, 30);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 2);
        Assert.Equal(new Vec2(10, 20), result);
    }

    [Fact]
    public void ProjectTo2D_DropX_NegativeCoords()
    {
        var p = new Vec3(-5, -7, -3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 0);
        Assert.Equal(new Vec2(-7, -3), result);
    }

    [Fact]
    public void ProjectTo2D_DropZ_Zero()
    {
        var p = Vec3.Zero;
        var result = ConstrainedTriangulator.ProjectTo2D(p, 2);
        Assert.Equal(new Vec2(0, 0), result);
    }

    [Fact]
    public void ProjectTo2D_InvalidAxis_DefaultsToDropZ()
    {
        var p = new Vec3(1, 2, 3);
        // dropAxis >= 2 falls to default case, drops Z
        var result = ConstrainedTriangulator.ProjectTo2D(p, 99);
        Assert.Equal(new Vec2(1, 2), result);
    }

    // ── Triangulate (integration) ────────────────────────────────────────

    [Fact]
    public void Triangulate_SingleTriangle_ReturnsSingleTriangle()
    {
        var verts = new List<Vec3>
        {
            Vec3.Zero,
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0),
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Single(result);
        var (a, b, c) = result[0];
        Assert.True(a == 0 && b == 1 && c == 2);
    }

    [Fact]
    public void Triangulate_LessThan3Vertices_ReturnsEmpty()
    {
        var verts = new List<Vec3> { Vec3.Zero, new Vec3(1, 0, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_FourVertices_NoConstraints_ProducesTwo()
    {
        // Square polygon
        var verts = new List<Vec3>
        {
            Vec3.Zero,
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_WithConstraint_ProducesValidTriangles()
    {
        var verts = new List<Vec3>
        {
            Vec3.Zero,
            new Vec3(2, 0, 0),
            new Vec3(1, 2, 0),
            new Vec3(1, 0.5, 0), // interior point
        };
        var constraints = new List<(int, int)> { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, new Vec3(0, 0, 1));
        Assert.True(result.Count >= 2, $"Expected >= 2 triangles, got {result.Count}");
    }

    [Fact]
    public void Triangulate_AllIndicesValid()
    {
        var verts = new List<Vec3>
        {
            Vec3.Zero,
            new Vec3(1, 0, 0),
            new Vec3(0.5, 1, 0),
            new Vec3(0.5, 0.3, 0),
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Count, $"Index a={a} out of range");
            Assert.True(b >= 0 && b < verts.Count, $"Index b={b} out of range");
            Assert.True(c >= 0 && c < verts.Count, $"Index c={c} out of range");
        }
    }

    [Fact]
    public void Triangulate_FiveVertices_ProducesThree()
    {
        // Pentagon
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(2.5, 1.5, 0),
            new Vec3(1, 2.5, 0),
            new Vec3(-0.5, 1.5, 0),
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Triangulate_YDominant_ProjectsXZ()
    {
        // Triangle on XZ plane (normal along Y)
        var verts = new List<Vec3>
        {
            Vec3.Zero,
            new Vec3(1, 0, 0),
            new Vec3(0, 0, 1),
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), new Vec3(0, 1, 0));
        Assert.Single(result);
    }
}
