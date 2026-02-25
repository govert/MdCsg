using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator.Triangulate — output validity, vertex coverage, constraint enforcement</summary>
public class ConstrainedTriangulatorOutputPropertyTests
{
    [Fact]
    public void ThreeVertices_SingleTriangle()
    {
        var verts = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        Assert.Single(result);
        var (a, b, c) = result[0];
        Assert.True(a >= 0 && a <= 2);
        Assert.True(b >= 0 && b <= 2);
        Assert.True(c >= 0 && c <= 2);
    }

    [Fact]
    public void FourVertices_TwoTriangles()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FiveVertices_ThreeTriangles()
    {
        var verts = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(2, 2, 0),
            new Vec3(1, 1, 0), new Vec3(0, 2, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void AllIndices_AreValid()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(2, 0, 0), new Vec3(2, 2, 0),
            new Vec3(0, 2, 0), new Vec3(1, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Length, $"Index A={a} out of range");
            Assert.True(b >= 0 && b < verts.Length, $"Index B={b} out of range");
            Assert.True(c >= 0 && c < verts.Length, $"Index C={c} out of range");
        }
    }

    [Fact]
    public void AllTriangles_HaveDistinctVertices()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(2, 0, 0), new Vec3(2, 2, 0),
            new Vec3(0, 2, 0), new Vec3(1, 0.5, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(a, c);
        }
    }

    [Fact]
    public void WithConstraint_ConstraintAppears()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(2, 0, 0), new Vec3(1, 2, 0),
            new Vec3(1, 1, 0) // interior point
        };
        var constraints = new List<(int, int)> { (0, 3) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        // The constraint edge (0,3) should appear as an edge in some triangle
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
    public void LessThanThreeVertices_EmptyResult()
    {
        var verts = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        Assert.Empty(result);
    }

    [Fact]
    public void ThreeVertices_NoConstraints_SingleTriangleCoversAll()
    {
        var verts = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0) };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        Assert.Single(result);
        var indices = new HashSet<int> { result[0].A, result[0].B, result[0].C };
        Assert.Contains(0, indices);
        Assert.Contains(1, indices);
        Assert.Contains(2, indices);
    }

    [Fact]
    public void RegularPentagon_ThreeTriangles()
    {
        var verts = new Vec3[5];
        for (int i = 0; i < 5; i++)
        {
            double angle = 2 * System.Math.PI * i / 5;
            verts[i] = new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0);
        }
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void TriangulationCount_ForNVertices_IsNMinus2()
    {
        // For a convex polygon with n vertices, triangulation should give n-2 triangles
        for (int n = 3; n <= 7; n++)
        {
            var verts = new Vec3[n];
            for (int i = 0; i < n; i++)
            {
                double angle = 2 * System.Math.PI * i / n;
                verts[i] = new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0);
            }
            var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitZ);
            Assert.Equal(n - 2, result.Count);
        }
    }

    [Fact]
    public void DifferentNormals_StillWorks()
    {
        // XZ plane
        var verts = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1) };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitY);
        Assert.Single(result);
    }

    [Fact]
    public void YZPlane_Triangulation()
    {
        var verts = new Vec3[] { Vec3.Zero, new Vec3(0, 1, 0), new Vec3(0, 0, 1) };
        var result = ConstrainedTriangulator.Triangulate(verts, [], Vec3.UnitX);
        Assert.Single(result);
    }
}
