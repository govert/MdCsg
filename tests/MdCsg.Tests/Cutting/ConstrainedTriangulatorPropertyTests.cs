using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator - Projection, ear-clipping, constrained insertion, cavity re-triangulation</summary>
public class ConstrainedTriangulatorPropertyTests
{
    // --- GetDominantAxis ---

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
    public void GetDominantAxis_NegativeNormal()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, -10, 1)));
    }

    [Fact]
    public void GetDominantAxis_UnitZ()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitZ));
    }

    // --- ProjectTo2D ---

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var p = new Vec3(5, 3, 7);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 0);
        Assert.Equal(new Vec2(3, 7), result);
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var p = new Vec3(5, 3, 7);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 1);
        Assert.Equal(new Vec2(5, 7), result);
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var p = new Vec3(5, 3, 7);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 2);
        Assert.Equal(new Vec2(5, 3), result);
    }

    // --- Triangulate (no constraints) ---

    [Fact]
    public void Triangulate_ThreeVertices_SingleTriangle()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Single(result);
        var (a, b, c) = result[0];
        Assert.Contains(0, new[] { a, b, c });
        Assert.Contains(1, new[] { a, b, c });
        Assert.Contains(2, new[] { a, b, c });
    }

    [Fact]
    public void Triangulate_LessThanThreeVertices_Empty()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_FourVertices_NoConstraints_TwoTriangles()
    {
        // Square in the XY plane
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_AllIndicesValid()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.InRange(a, 0, verts.Length - 1);
            Assert.InRange(b, 0, verts.Length - 1);
            Assert.InRange(c, 0, verts.Length - 1);
        }
    }

    [Fact]
    public void Triangulate_FiveVertices_ThreeTriangles()
    {
        // Pentagon-like in XY plane
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(2, 0, 0), new(3, 1, 0), new(1, 2, 0), new(-1, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Equal(3, result.Count);
    }

    // --- Triangulate with constraints ---

    [Fact]
    public void Triangulate_WithConstraint_ProducesTriangles()
    {
        // Triangle with an interior point and a constraint from vertex 0 to vertex 3
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(2, 4, 0), new(2, 1, 0) // interior point
        };
        var constraints = new[] { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void Triangulate_WithConstraint_ConstraintEdgeExists()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(2, 4, 0), new(2, 1, 0)
        };
        var constraints = new[] { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);

        // The constraint edge (0,3) should exist in the triangulation
        bool found = result.Any(t =>
            HasEdge(t.A, t.B, t.C, 0, 3));
        Assert.True(found);
    }

    [Fact]
    public void Triangulate_WithConstraint_AllIndicesValid()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(2, 4, 0), new(2, 1, 0)
        };
        var constraints = new[] { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.InRange(a, 0, verts.Length - 1);
            Assert.InRange(b, 0, verts.Length - 1);
            Assert.InRange(c, 0, verts.Length - 1);
        }
    }

    [Fact]
    public void Triangulate_MultipleConstraints_AllExist()
    {
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(6, 0, 0), new(3, 6, 0),
            new(2, 1, 0), new(4, 1, 0) // two interior points
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);

        bool found = result.Any(t => HasEdge(t.A, t.B, t.C, 3, 4));
        Assert.True(found);
    }

    // --- Different normals (different projection axes) ---

    [Fact]
    public void Triangulate_YZPlane_Works()
    {
        // Triangle in YZ plane (normal along X)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitX);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_XZPlane_Works()
    {
        // Triangle in XZ plane (normal along Y)
        var verts = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitY);
        Assert.Single(result);
    }

    private static bool HasEdge(int a, int b, int c, int s, int e)
    {
        return (a == s && b == e) || (b == s && a == e) ||
               (b == s && c == e) || (c == s && b == e) ||
               (c == s && a == e) || (a == s && c == e);
    }
}
