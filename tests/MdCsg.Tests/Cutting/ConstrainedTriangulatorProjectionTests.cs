using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: ConstrainedTriangulator projection — GetDominantAxis, ProjectTo2D for all axes</summary>
public class ConstrainedTriangulatorProjectionTests
{
    [Fact]
    public void GetDominantAxis_ZDominant()
    {
        var normal = new Vec3(0, 0, 1);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_YDominant()
    {
        var normal = new Vec3(0, 1, 0);
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_XDominant()
    {
        var normal = new Vec3(1, 0, 0);
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_NegativeZDominant()
    {
        var normal = new Vec3(0, 0, -5);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_DiagonalNormal()
    {
        // (1, 1, 3) → Z dominant
        var normal = new Vec3(1, 1, 3);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void GetDominantAxis_XYEqual_XChosen()
    {
        // (5, 5, 0) → X or Y (depends on tie-breaking)
        var normal = new Vec3(5, 5, 0);
        int axis = ConstrainedTriangulator.GetDominantAxis(normal);
        Assert.True(axis == 0 || axis == 1);
    }

    [Fact]
    public void ProjectTo2D_DropZ()
    {
        var point = new Vec3(1, 2, 3);
        var proj = ConstrainedTriangulator.ProjectTo2D(point, 2);
        Assert.Equal(1, proj.X, 1e-14);
        Assert.Equal(2, proj.Y, 1e-14);
    }

    [Fact]
    public void ProjectTo2D_DropY()
    {
        var point = new Vec3(1, 2, 3);
        var proj = ConstrainedTriangulator.ProjectTo2D(point, 1);
        Assert.Equal(1, proj.X, 1e-14);
        Assert.Equal(3, proj.Y, 1e-14);
    }

    [Fact]
    public void ProjectTo2D_DropX()
    {
        var point = new Vec3(1, 2, 3);
        var proj = ConstrainedTriangulator.ProjectTo2D(point, 0);
        Assert.Equal(2, proj.X, 1e-14);
        Assert.Equal(3, proj.Y, 1e-14);
    }

    [Fact]
    public void ProjectTo2D_Origin_IsZero()
    {
        var proj = ConstrainedTriangulator.ProjectTo2D(Vec3.Zero, 2);
        Assert.Equal(0, proj.X, 1e-14);
        Assert.Equal(0, proj.Y, 1e-14);
    }

    [Fact]
    public void Triangulate_SimpleTriangle_OneTriangle()
    {
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Triangulate_Square_TwoTriangles()
    {
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Triangulate_AllIndicesInBounds()
    {
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(2, 2, 0), new Vec3(0, 2, 0)
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
    public void Triangulate_NoDuplicateIndices()
    {
        var verts = new List<Vec3>
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0)
        };
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        foreach (var (a, b, c) in result)
        {
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, c);
            Assert.NotEqual(a, c);
        }
    }

    [Fact]
    public void Triangulate_Pentagon_ThreeTriangles()
    {
        // Regular pentagon approximation in XY plane
        var verts = new List<Vec3>();
        for (int i = 0; i < 5; i++)
        {
            double angle = 2 * System.Math.PI * i / 5;
            verts.Add(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0));
        }
        var result = ConstrainedTriangulator.Triangulate(verts, new List<(int, int)>(), new Vec3(0, 0, 1));
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetDominantAxis_LargeNormal()
    {
        var normal = new Vec3(1e6, 0, 0);
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(normal));
    }
}
