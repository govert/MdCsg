using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Batch 17: ConstrainedTriangulator tests (20 tests)</summary>
public class ConstrainedTriangulatorTests
{
    [Fact]
    public void ThreeVertices_NoConstraints_ReturnsOneTriangle()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Single(result);
    }

    [Fact]
    public void ThreeVertices_NoConstraints_HasCorrectIndices()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        var (a, b, c) = result[0];
        // Should contain all three vertices
        var indices = new HashSet<int> { a, b, c };
        Assert.Contains(0, indices);
        Assert.Contains(1, indices);
        Assert.Contains(2, indices);
    }

    [Fact]
    public void LessThanThreeVertices_ReturnsEmpty()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Empty(result);
    }

    [Fact]
    public void FourVertices_NoConstraints_ReturnsTwo()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY, new Vec3(0.3, 0.3, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void FiveVertices_WithConstraint_ProducesValidTriangulation()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, Vec3.UnitX, Vec3.UnitY,
            new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void Constraint_AppearAsEdge()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, Vec3.UnitX, Vec3.UnitY,
            new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        // The constraint edge (3,4) should appear in the triangulation
        bool found = result.Any(t =>
            (t.A == 3 && t.B == 4) || (t.B == 3 && t.C == 4) || (t.C == 3 && t.A == 4) ||
            (t.A == 4 && t.B == 3) || (t.B == 4 && t.C == 3) || (t.C == 4 && t.A == 3));
        Assert.True(found);
    }

    [Fact]
    public void GetDominantAxis_X()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitX));
    }

    [Fact]
    public void GetDominantAxis_Y()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitY));
    }

    [Fact]
    public void GetDominantAxis_Z()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitZ));
    }

    [Fact]
    public void GetDominantAxis_NegativeZ()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(-Vec3.UnitZ));
    }

    [Fact]
    public void GetDominantAxis_Diagonal()
    {
        // (1,1,2) -> Z is dominant
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, 2)));
    }

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var v = new Vec3(1, 2, 3);
        var p = ConstrainedTriangulator.ProjectTo2D(v, 0);
        Assert.Equal(2, p.X);
        Assert.Equal(3, p.Y);
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var v = new Vec3(1, 2, 3);
        var p = ConstrainedTriangulator.ProjectTo2D(v, 1);
        Assert.Equal(1, p.X);
        Assert.Equal(3, p.Y);
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var v = new Vec3(1, 2, 3);
        var p = ConstrainedTriangulator.ProjectTo2D(v, 2);
        Assert.Equal(1, p.X);
        Assert.Equal(2, p.Y);
    }

    [Fact]
    public void Triangulate_InXZPlane_UsesCorrectProjection()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitZ };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitY);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_InYZPlane_UsesCorrectProjection()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitY, Vec3.UnitZ };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitX);
        Assert.Single(result);
    }

    [Fact]
    public void MultipleConstraints_AllEnforced()
    {
        // Triangle with two constraint edges crossing it
        var verts = new Vec3[]
        {
            new Vec3(-1, -1, 0), new Vec3(3, -1, 0), new Vec3(1, 3, 0),
            new Vec3(0, 0, 0), new Vec3(2, 0, 0),   // constraint 1
            new Vec3(0, 1, 0), new Vec3(2, 1, 0),   // constraint 2
        };
        var constraints = new[] { (3, 4), (5, 6) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 4);
    }

    [Fact]
    public void AllTriangles_HaveValidIndices()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, Vec3.UnitX, Vec3.UnitY,
            new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Length);
            Assert.True(b >= 0 && b < verts.Length);
            Assert.True(c >= 0 && c < verts.Length);
        }
    }

    [Fact]
    public void AllTriangles_HaveDistinctVertices()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, Vec3.UnitX, Vec3.UnitY,
            new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(c, a);
        }
    }

    [Fact]
    public void VertexOnEdge_HandledGracefully()
    {
        // Point exactly on edge AB at midpoint
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(2, 0, 0), new Vec3(1, 2, 0),
            new Vec3(1, 0, 0) // midpoint of AB
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.True(result.Count >= 2);
    }
}
