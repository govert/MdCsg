using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator — ProjectTo2D, GetDominantAxis, basic triangulation, ear clipping</summary>
public class ConstrainedTriangulatorProjectionPropertyTests
{
    [Fact]
    public void GetDominantAxis_ZNormal_Returns2()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitZ));
    }

    [Fact]
    public void GetDominantAxis_XNormal_Returns0()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitX));
    }

    [Fact]
    public void GetDominantAxis_YNormal_Returns1()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(Vec3.UnitY));
    }

    [Fact]
    public void GetDominantAxis_DiagonalNormal_ReturnsLargest()
    {
        // (1, 2, 3) → Z is largest
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 2, 3)));
    }

    [Fact]
    public void GetDominantAxis_NegativeNormal_StillWorks()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(0, 0, -5)));
    }

    [Fact]
    public void ProjectTo2D_DropZ_ReturnsXY()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 2);
        Assert.Equal(1.0, result.X);
        Assert.Equal(2.0, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropX_ReturnsYZ()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 0);
        Assert.Equal(2.0, result.X);
        Assert.Equal(3.0, result.Y);
    }

    [Fact]
    public void ProjectTo2D_DropY_ReturnsXZ()
    {
        var result = ConstrainedTriangulator.ProjectTo2D(new Vec3(1, 2, 3), 1);
        Assert.Equal(1.0, result.X);
        Assert.Equal(3.0, result.Y);
    }

    [Fact]
    public void Triangulate_SingleTriangle_ReturnsOneTriangle()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var constraints = Array.Empty<(int, int)>();
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Triangulate_SingleTriangle_IndicesAre012()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var constraints = Array.Empty<(int, int)>();
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        var tri = result[0];
        var indices = new HashSet<int> { tri.A, tri.B, tri.C };
        Assert.Contains(0, indices);
        Assert.Contains(1, indices);
        Assert.Contains(2, indices);
    }

    [Fact]
    public void Triangulate_FourPoints_ProducesMultipleTriangles()
    {
        var verts = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(2, 0, 0),
            new Vec3(2, 2, 0), new Vec3(1, 1, 0) // point inside triangle
        };
        var constraints = Array.Empty<(int, int)>();
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2, $"4 points should produce 2+ triangles, got {result.Count}");
    }

    [Fact]
    public void Triangulate_WithConstraint_RespectsEdge()
    {
        // Triangle with an extra point and a constraint edge
        var verts = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(4, 0, 0),
            new Vec3(2, 4, 0), new Vec3(2, 1, 0) // inside point
        };
        var constraints = new (int, int)[] { (0, 3) }; // constraint from vertex 0 to vertex 3
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2);

        // Check that constraint edge exists in the triangulation
        bool found = false;
        foreach (var (a, b, c) in result)
        {
            if ((a == 0 && b == 3) || (b == 0 && c == 3) || (c == 0 && a == 3) ||
                (a == 3 && b == 0) || (b == 3 && c == 0) || (c == 3 && a == 0))
            {
                found = true;
                break;
            }
        }
        Assert.True(found, "Constraint edge (0,3) should appear in triangulation");
    }

    [Fact]
    public void Triangulate_LessThanThreeVertices_ReturnsEmpty()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void Triangulate_AllIndicesInRange()
    {
        var verts = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(4, 0, 0),
            new Vec3(2, 4, 0), new Vec3(2, 1, 0),
            new Vec3(1, 2, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Length, $"Index {a} out of range");
            Assert.True(b >= 0 && b < verts.Length, $"Index {b} out of range");
            Assert.True(c >= 0 && c < verts.Length, $"Index {c} out of range");
        }
    }

    [Fact]
    public void Triangulate_TriangleIndices_AllDistinct()
    {
        var verts = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(4, 0, 0),
            new Vec3(2, 4, 0), new Vec3(2, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(c, a);
        }
    }

    [Fact]
    public void Triangulate_NegativeZNormal_StillWorks()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), -Vec3.UnitZ);
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Triangulate_XFacingTriangle_ProjectsCorrectly()
    {
        // Triangle on YZ plane
        var verts = new Vec3[]
        {
            new Vec3(5, 0, 0), new Vec3(5, 1, 0), new Vec3(5, 0, 1)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitX);
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Triangulate_MultipleConstraints_ProducesValidOutput()
    {
        var verts = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(4, 0, 0),
            new Vec3(4, 4, 0), new Vec3(1, 1, 0),
            new Vec3(3, 1, 0)
        };
        var constraints = new (int, int)[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 3, $"5 points should produce 3+ triangles, got {result.Count}");
    }
}
