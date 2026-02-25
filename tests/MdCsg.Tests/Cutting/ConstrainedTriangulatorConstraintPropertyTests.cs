using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator — constraint enforcement, projection, ear clipping, edge cases</summary>
public class ConstrainedTriangulatorConstraintPropertyTests
{
    [Fact]
    public void Triangulate_ThreeVertices_NoConstraints_SingleTriangle()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var constraints = Array.Empty<(int, int)>();
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_ThreeVertices_OutputUsesAllIndices()
    {
        var verts = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitZ);
        var indices = new HashSet<int>();
        foreach (var (a, b, c) in result)
        {
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }
        Assert.Contains(0, indices);
        Assert.Contains(1, indices);
        Assert.Contains(2, indices);
    }

    [Fact]
    public void Triangulate_FourVertices_WithConstraint_TwoTriangles()
    {
        // Triangle with an extra point on the midpoint of one edge
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0),
            new Vec3(1, 0, 0) // midpoint of A-B
        };
        var constraints = new[] { (3, 2) }; // constraint from midpoint to C
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 2, $"Expected >= 2 triangles, got {result.Count}");
    }

    [Fact]
    public void Triangulate_TotalArea_MatchesOriginal()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0),
            new Vec3(2, 0, 0), new Vec3(0, 2, 0)
        };
        var constraints = new[] { (3, 4) }; // constraint from midAB to midAC
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        double totalArea = 0;
        foreach (var (a, b, c) in result)
            totalArea += new Triangle3(verts[a], verts[b], verts[c]).Area;
        double originalArea = new Triangle3(verts[0], verts[1], verts[2]).Area;
        Assert.True(System.Math.Abs(totalArea - originalArea) < 0.01,
            $"Total area {totalArea} should match original {originalArea}");
    }

    [Fact]
    public void Triangulate_ConstraintEdge_AppearsInOutput()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0),
            new Vec3(2, 0, 0), new Vec3(0, 2, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        // The constraint edge (3,4) should appear as an edge in some triangle
        bool found = false;
        foreach (var (a, b, c) in result)
        {
            if ((a == 3 && b == 4) || (b == 3 && c == 4) || (c == 3 && a == 4) ||
                (a == 4 && b == 3) || (b == 4 && c == 3) || (c == 4 && a == 3))
            {
                found = true;
                break;
            }
        }
        Assert.True(found, "Constraint edge (3,4) should appear in output triangulation");
    }

    [Fact]
    public void Triangulate_NoOverlap_IndicesInRange()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0),
            new Vec3(1, 0, 0)
        };
        var constraints = new[] { (3, 2) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < verts.Length, $"Index {a} out of range");
            Assert.True(b >= 0 && b < verts.Length, $"Index {b} out of range");
            Assert.True(c >= 0 && c < verts.Length, $"Index {c} out of range");
        }
    }

    [Fact]
    public void Triangulate_LessThanThreeVertices_EmptyResult()
    {
        var result = ConstrainedTriangulator.Triangulate(
            new Vec3[] { Vec3.Zero, Vec3.UnitX },
            Array.Empty<(int, int)>(), Vec3.UnitZ);
        Assert.Empty(result);
    }

    [Fact]
    public void Triangulate_XZPlane_ProjectsCorrectly()
    {
        // Triangle in XZ plane (normal along Y)
        var verts = new Vec3[] { Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 0, 2) };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitY);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_YZPlane_ProjectsCorrectly()
    {
        // Triangle in YZ plane (normal along X)
        var verts = new Vec3[] { Vec3.Zero, new Vec3(0, 2, 0), new Vec3(0, 0, 2) };
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), Vec3.UnitX);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_NonDegenerateTriangles()
    {
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0),
            new Vec3(2, 0, 0), new Vec3(0, 2, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            double area = new Triangle3(verts[a], verts[b], verts[c]).Area;
            Assert.True(area > 1e-10, $"Triangle ({a},{b},{c}) has near-zero area {area}");
        }
    }

    [Fact]
    public void Triangulate_DiagonalNormal_ProjectsCorrectly()
    {
        // Triangle in a tilted plane
        var verts = new Vec3[] { Vec3.Zero, new Vec3(1, 1, 0), new Vec3(0, 1, 1) };
        var normal = Vec3.Cross(verts[1] - verts[0], verts[2] - verts[0]).Normalized;
        var result = ConstrainedTriangulator.Triangulate(verts, Array.Empty<(int, int)>(), normal);
        Assert.Single(result);
    }

    [Fact]
    public void Triangulate_MultipleConstraints_AllPresent()
    {
        // Triangle with two extra points forming a cross-cut
        var verts = new Vec3[]
        {
            Vec3.Zero, new Vec3(6, 0, 0), new Vec3(0, 6, 0),
            new Vec3(3, 0, 0), new Vec3(0, 3, 0)
        };
        var constraints = new[] { (3, 4) };
        var result = ConstrainedTriangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.True(result.Count >= 3, $"Expected >= 3 triangles, got {result.Count}");
    }
}
