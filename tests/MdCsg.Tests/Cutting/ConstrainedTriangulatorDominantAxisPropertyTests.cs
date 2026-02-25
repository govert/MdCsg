using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator — GetDominantAxis, ProjectTo2D, basic triangulation invariants</summary>
public class ConstrainedTriangulatorDominantAxisPropertyTests
{
    [Fact]
    public void GetDominantAxis_ZNormal_ReturnsZ()
    {
        var normal = new Vec3(0, 0, 1);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_NegativeZNormal_ReturnsZ()
    {
        var normal = new Vec3(0, 0, -5);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_XNormal_ReturnsX()
    {
        var normal = new Vec3(10, 0, 0);
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_YNormal_ReturnsY()
    {
        var normal = new Vec3(0, 7, 0);
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_DiagonalNormal_ReturnsLargest()
    {
        // (1, 2, 3) → Z is largest
        var normal = new Vec3(1, 2, 3);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void ProjectTo2D_ZAxis_DropsZ()
    {
        var point = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(point, 2);
        Assert.Equal(1.0, result.X);
        Assert.Equal(2.0, result.Y);
    }

    [Fact]
    public void ProjectTo2D_XAxis_DropsX()
    {
        var point = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(point, 0);
        Assert.Equal(2.0, result.X);
        Assert.Equal(3.0, result.Y);
    }

    [Fact]
    public void ProjectTo2D_YAxis_DropsY()
    {
        var point = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(point, 1);
        Assert.Equal(1.0, result.X);
        Assert.Equal(3.0, result.Y);
    }

    [Fact]
    public void Triangulate_SingleTriangle_ReturnsOneTriangle()
    {
        var vertices = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(vertices, new List<(int, int)>(), Vec3.UnitZ);
        Assert.Single(result);
        var (a, b, c) = result[0];
        Assert.True(a >= 0 && a < 3);
        Assert.True(b >= 0 && b < 3);
        Assert.True(c >= 0 && c < 3);
        Assert.NotEqual(a, b);
        Assert.NotEqual(b, c);
        Assert.NotEqual(c, a);
    }

    [Fact]
    public void Triangulate_QuadWithDiagonal_ReturnsTwoTriangles()
    {
        var vertices = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)
        };
        // Constraint along diagonal
        var constraints = new List<(int, int)> { (0, 2) };
        var result = ConstrainedTriangulator.Triangulate(vertices, constraints, Vec3.UnitZ);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_AllIndicesInRange()
    {
        var vertices = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(2, 2, 0),
            new Vec3(0, 2, 0), new Vec3(1, 1, 0)
        };
        var constraints = new List<(int, int)> { (0, 4), (4, 2) };
        var result = ConstrainedTriangulator.Triangulate(vertices, constraints, Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.True(a >= 0 && a < vertices.Count);
            Assert.True(b >= 0 && b < vertices.Count);
            Assert.True(c >= 0 && c < vertices.Count);
        }
    }

    [Fact]
    public void Triangulate_NoConstraints_ProducesTriangles()
    {
        var vertices = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(3, 3, 0),
            new Vec3(0, 3, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(vertices, new List<(int, int)>(), Vec3.UnitZ);
        Assert.True(result.Count >= 2, "Quad should produce at least 2 triangles");
    }

    [Fact]
    public void Triangulate_TriangleIndices_NoDuplicatesWithin()
    {
        var vertices = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(vertices, new List<(int, int)>(), Vec3.UnitZ);
        foreach (var (a, b, c) in result)
        {
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(c, a);
        }
    }
}
