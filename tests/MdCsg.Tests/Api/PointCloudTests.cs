using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class PointCloudTests
{
    // =========================================================================
    // Constructor basics
    // =========================================================================

    [Fact]
    public void Constructor_StoresPointsAndComputesBounds()
    {
        var pts = new[] { new Vec3(0, 0, 0), new Vec3(1, 2, 3) };
        var cloud = new PointCloud(pts);

        Assert.Equal(2, cloud.Count);
        Assert.Same(pts, cloud.Points);
        Assert.Null(cloud.Normals);
        Assert.Equal(0.0, cloud.Bounds.Min.X);
        Assert.Equal(1.0, cloud.Bounds.Max.X);
        Assert.Equal(3.0, cloud.Bounds.Max.Z);
    }

    [Fact]
    public void Constructor_WithNormals_StoresBoth()
    {
        var pts = new[] { Vec3.Zero, Vec3.UnitX };
        var nrm = new[] { Vec3.UnitZ, Vec3.UnitY };
        var cloud = new PointCloud(pts, nrm);

        Assert.NotNull(cloud.Normals);
        Assert.Equal(2, cloud.Normals!.Length);
    }

    [Fact]
    public void Constructor_MismatchedNormals_Throws()
    {
        var pts = new[] { Vec3.Zero };
        var nrm = new[] { Vec3.UnitZ, Vec3.UnitY };

        Assert.Throws<ArgumentException>(() => new PointCloud(pts, nrm));
    }

    [Fact]
    public void Empty_Cloud_HasEmptyBounds()
    {
        var cloud = new PointCloud(Array.Empty<Vec3>());
        Assert.Equal(0, cloud.Count);
    }

    [Fact]
    public void SinglePoint_Cloud()
    {
        var cloud = new PointCloud(new[] { new Vec3(5, 10, 15) });
        Assert.Equal(1, cloud.Count);
        Assert.Equal(5.0, cloud.Bounds.Min.X);
        Assert.Equal(5.0, cloud.Bounds.Max.X);
    }

    // =========================================================================
    // Bounds computation
    // =========================================================================

    [Fact]
    public void Bounds_CorrectForSpread()
    {
        var pts = new[]
        {
            new Vec3(-10, -20, -30),
            new Vec3(10, 20, 30),
            new Vec3(0, 0, 0),
        };
        var cloud = new PointCloud(pts);

        Assert.Equal(-10.0, cloud.Bounds.Min.X);
        Assert.Equal(-20.0, cloud.Bounds.Min.Y);
        Assert.Equal(-30.0, cloud.Bounds.Min.Z);
        Assert.Equal(10.0, cloud.Bounds.Max.X);
        Assert.Equal(20.0, cloud.Bounds.Max.Y);
        Assert.Equal(30.0, cloud.Bounds.Max.Z);
    }

    [Fact]
    public void Bounds_AllSamePoint_DegenerateBounds()
    {
        var pts = new[] { new Vec3(5, 5, 5), new Vec3(5, 5, 5), new Vec3(5, 5, 5) };
        var cloud = new PointCloud(pts);

        Assert.Equal(5.0, cloud.Bounds.Min.X);
        Assert.Equal(5.0, cloud.Bounds.Max.X);
    }

    // =========================================================================
    // Subset operations
    // =========================================================================

    [Fact]
    public void Subset_ReturnsSelectedPoints()
    {
        var pts = new[] { new Vec3(0, 0, 0), new Vec3(1, 1, 1), new Vec3(2, 2, 2), new Vec3(3, 3, 3) };
        var cloud = new PointCloud(pts);

        var sub = cloud.Subset(new[] { 1, 3 });
        Assert.Equal(2, sub.Count);
        Assert.Equal(new Vec3(1, 1, 1), sub.Points[0]);
        Assert.Equal(new Vec3(3, 3, 3), sub.Points[1]);
    }

    [Fact]
    public void Subset_WithNormals_PreservesNormals()
    {
        var pts = new[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var nrm = new[] { Vec3.UnitZ, Vec3.UnitX, Vec3.UnitY };
        var cloud = new PointCloud(pts, nrm);

        var sub = cloud.Subset(new[] { 2 });
        Assert.NotNull(sub.Normals);
        Assert.Equal(Vec3.UnitY, sub.Normals![0]);
    }

    [Fact]
    public void Subset_SingleIndex()
    {
        var pts = new[] { new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9) };
        var cloud = new PointCloud(pts);

        var sub = cloud.Subset(new[] { 0 });
        Assert.Equal(1, sub.Count);
        Assert.Equal(new Vec3(1, 2, 3), sub.Points[0]);
    }

    [Fact]
    public void Subset_AllIndices()
    {
        var pts = new[] { new Vec3(1, 2, 3), new Vec3(4, 5, 6) };
        var cloud = new PointCloud(pts);

        var sub = cloud.Subset(new[] { 0, 1 });
        Assert.Equal(2, sub.Count);
    }

    [Fact]
    public void Subset_EmptyIndices()
    {
        var cloud = new PointCloud(new[] { Vec3.Zero, Vec3.UnitX });
        var sub = cloud.Subset(Array.Empty<int>());
        Assert.Equal(0, sub.Count);
    }

    [Fact]
    public void Subset_ReversedOrder()
    {
        var pts = new[] { new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(3, 0, 0) };
        var cloud = new PointCloud(pts);

        var sub = cloud.Subset(new[] { 2, 0 });
        Assert.Equal(new Vec3(3, 0, 0), sub.Points[0]);
        Assert.Equal(new Vec3(1, 0, 0), sub.Points[1]);
    }

    // =========================================================================
    // Where operations
    // =========================================================================

    [Fact]
    public void Where_FiltersByMask()
    {
        var pts = new[] { new Vec3(0, 0, 0), new Vec3(1, 1, 1), new Vec3(2, 2, 2) };
        var cloud = new PointCloud(pts);

        var filtered = cloud.Where(new[] { true, false, true });
        Assert.Equal(2, filtered.Count);
        Assert.Equal(new Vec3(0, 0, 0), filtered.Points[0]);
        Assert.Equal(new Vec3(2, 2, 2), filtered.Points[1]);
    }

    [Fact]
    public void Where_AllTrue_ReturnsAll()
    {
        var pts = new[] { Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        var cloud = new PointCloud(pts);

        var filtered = cloud.Where(new[] { true, true, true });
        Assert.Equal(3, filtered.Count);
    }

    [Fact]
    public void Where_AllFalse_ReturnsEmpty()
    {
        var pts = new[] { Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        var cloud = new PointCloud(pts);

        var filtered = cloud.Where(new[] { false, false, false });
        Assert.Equal(0, filtered.Count);
    }

    [Fact]
    public void Where_Alternating()
    {
        var pts = new Vec3[6];
        for (int i = 0; i < 6; i++)
            pts[i] = new Vec3(i, 0, 0);
        var cloud = new PointCloud(pts);

        var mask = new[] { true, false, true, false, true, false };
        var filtered = cloud.Where(mask);
        Assert.Equal(3, filtered.Count);
        Assert.Equal(new Vec3(0, 0, 0), filtered.Points[0]);
        Assert.Equal(new Vec3(2, 0, 0), filtered.Points[1]);
        Assert.Equal(new Vec3(4, 0, 0), filtered.Points[2]);
    }

    [Fact]
    public void Where_WithNormals_PreservesNormals()
    {
        var pts = new[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var nrm = new[] { Vec3.UnitZ, Vec3.UnitX, Vec3.UnitY };
        var cloud = new PointCloud(pts, nrm);

        var filtered = cloud.Where(new[] { false, true, false });
        Assert.NotNull(filtered.Normals);
        Assert.Equal(1, filtered.Count);
        Assert.Equal(Vec3.UnitX, filtered.Normals![0]);
    }

    [Fact]
    public void Where_MismatchedMask_Throws()
    {
        var cloud = new PointCloud(new[] { Vec3.Zero });
        Assert.Throws<ArgumentException>(() => cloud.Where(new[] { true, false }));
    }

    // =========================================================================
    // Count property
    // =========================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Count_MatchesPointArrayLength(int count)
    {
        var pts = new Vec3[count];
        for (int i = 0; i < count; i++)
            pts[i] = new Vec3(i, 0, 0);
        var cloud = new PointCloud(pts);
        Assert.Equal(count, cloud.Count);
    }
}
