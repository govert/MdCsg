using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator stress and edge cases — large polygons, degenerate inputs, projection axes</summary>
public class ConstrainedTriangulatorStressTests
{
    [Fact]
    public void GetDominantAxis_XDominant()
    {
        // Normal mostly along X
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
    public void GetDominantAxis_NegativeX()
    {
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(new Vec3(-10, 1, 1)));
    }

    [Fact]
    public void GetDominantAxis_NegativeY()
    {
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, -10, 1)));
    }

    [Fact]
    public void GetDominantAxis_NegativeZ()
    {
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(new Vec3(1, 1, -10)));
    }

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var p = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 0);
        Assert.Equal(2, result.X); // Y
        Assert.Equal(3, result.Y); // Z
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var p = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 1);
        Assert.Equal(1, result.X); // X
        Assert.Equal(3, result.Y); // Z
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var p = new Vec3(1, 2, 3);
        var result = ConstrainedTriangulator.ProjectTo2D(p, 2);
        Assert.Equal(1, result.X); // X
        Assert.Equal(2, result.Y); // Y
    }

    [Fact]
    public void Triangulate_SquareWithDiagonalConstraint()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0),
            // Interior diagonal midpoint not needed; constraint from corner to corner
        };
        var constraints = new List<(int, int)> { (0, 2) }; // Diagonal
        var triangles = ConstrainedTriangulator.Triangulate(verts, constraints, normal);
        Assert.True(triangles.Count >= 2, "Square should produce at least 2 triangles");
    }

    [Fact]
    public void Triangulate_SimpleTriangle_OneOutput()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var constraints = new List<(int, int)>();
        var triangles = ConstrainedTriangulator.Triangulate(verts, constraints, normal);
        Assert.Equal(1, triangles.Count);
    }

    [Fact]
    public void Triangulate_Pentagon_ThreeTriangles()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>();
        for (int i = 0; i < 5; i++)
        {
            double angle = 2 * System.Math.PI * i / 5;
            verts.Add(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0));
        }
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        // n-2 = 3 triangles for a convex pentagon
        Assert.True(triangles.Count >= 3);
    }

    [Fact]
    public void Triangulate_Hexagon_FourTriangles()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            verts.Add(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0));
        }
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 4);
    }

    [Fact]
    public void Triangulate_RegularPolygon12_ProducesCorrectCount()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>();
        for (int i = 0; i < 12; i++)
        {
            double angle = 2 * System.Math.PI * i / 12;
            verts.Add(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0));
        }
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 10); // n-2 = 10
    }

    [Fact]
    public void Triangulate_AllOutputTriangles_HaveArea()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>();
        for (int i = 0; i < 8; i++)
        {
            double angle = 2 * System.Math.PI * i / 8;
            verts.Add(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0));
        }
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        foreach (var tri in triangles)
        {
            var a = verts[tri.A];
            var b = verts[tri.B];
            var c = verts[tri.C];
            var cross = Vec3.Cross(b - a, c - a);
            double area = cross.Length / 2;
            Assert.True(area > 1e-15, "Triangle should have non-zero area");
        }
    }

    [Fact]
    public void Triangulate_InXYPlane_WorksWithZNormal()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3> { new(0, 0, 5), new(1, 0, 5), new(1, 1, 5), new(0, 1, 5) };
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 2);
    }

    [Fact]
    public void Triangulate_InXZPlane_WorksWithYNormal()
    {
        var normal = new Vec3(0, 1, 0);
        var verts = new List<Vec3> { new(0, 5, 0), new(1, 5, 0), new(1, 5, 1), new(0, 5, 1) };
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 2);
    }

    [Fact]
    public void Triangulate_InYZPlane_WorksWithXNormal()
    {
        var normal = new Vec3(1, 0, 0);
        var verts = new List<Vec3> { new(5, 0, 0), new(5, 1, 0), new(5, 1, 1), new(5, 0, 1) };
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 2);
    }

    [Fact]
    public void Triangulate_WithConstraint_ProducesMultipleTriangles()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0), new(1, 1, 0)
        };
        var constraints = new List<(int, int)> { (0, 4), (2, 4) };
        var triangles = ConstrainedTriangulator.Triangulate(verts, constraints, normal);
        // 5 vertices with 2 constraints should produce at least 2 triangles
        Assert.True(triangles.Count >= 2, $"Expected >= 2 triangles, got {triangles.Count}");
        // All indices should be valid
        foreach (var tri in triangles)
        {
            Assert.True(tri.A >= 0 && tri.A < verts.Count);
            Assert.True(tri.B >= 0 && tri.B < verts.Count);
            Assert.True(tri.C >= 0 && tri.C < verts.Count);
        }
    }

    [Fact]
    public void Triangulate_OutputVertices_WithinInputConvexHull()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(3, 0, 0), new(3, 3, 0), new(0, 3, 0)
        };
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        foreach (var tri in triangles)
        {
            foreach (var idx in new[] { tri.A, tri.B, tri.C })
            {
                var v = verts[idx];
                Assert.True(v.X >= -1e-10 && v.X <= 3 + 1e-10);
                Assert.True(v.Y >= -1e-10 && v.Y <= 3 + 1e-10);
            }
        }
    }

    [Fact]
    public void Triangulate_NegativeNormal_StillWorks()
    {
        var normal = new Vec3(0, 0, -1);
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 2);
    }

    [Fact]
    public void Triangulate_LargePolygon_20Vertices()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3>();
        for (int i = 0; i < 20; i++)
        {
            double angle = 2 * System.Math.PI * i / 20;
            verts.Add(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0));
        }
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.True(triangles.Count >= 18); // n-2
    }

    [Fact]
    public void Triangulate_TwoVertices_Empty()
    {
        var normal = new Vec3(0, 0, 1);
        var verts = new List<Vec3> { new(0, 0, 0), new(1, 0, 0) };
        var triangles = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), normal);
        Assert.Equal(0, triangles.Count);
    }
}
