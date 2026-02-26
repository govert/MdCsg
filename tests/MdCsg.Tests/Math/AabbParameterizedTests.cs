using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>
/// Comprehensive parameterized tests for Aabb operations.
/// </summary>
public class AabbParameterizedTests
{
    public static IEnumerable<object[]> Boxes => new[]
    {
        new object[] { 0.0, 0.0, 0.0, 1.0, 1.0, 1.0 },
        new object[] { -1.0, -1.0, -1.0, 1.0, 1.0, 1.0 },
        new object[] { 0.0, 0.0, 0.0, 10.0, 10.0, 10.0 },
        new object[] { -5.0, -5.0, -5.0, 5.0, 5.0, 5.0 },
        new object[] { 0.0, 0.0, 0.0, 0.001, 0.001, 0.001 },
        new object[] { 100.0, 200.0, 300.0, 101.0, 201.0, 301.0 },
        new object[] { -100.0, -100.0, -100.0, -99.0, -99.0, -99.0 },
    };

    public static IEnumerable<object[]> BoxPairs()
    {
        var boxes = new (Vec3 min, Vec3 max)[]
        {
            (new(0,0,0), new(1,1,1)),
            (new(-1,-1,-1), new(1,1,1)),
            (new(0.5,0.5,0.5), new(1.5,1.5,1.5)),
            (new(2,2,2), new(3,3,3)),
            (new(-2,-2,-2), new(-1,-1,-1)),
            (new(0,0,0), new(10,10,10)),
        };
        foreach (var a in boxes)
        foreach (var b in boxes)
            yield return new object[] { a.min.X, a.min.Y, a.min.Z, a.max.X, a.max.Y, a.max.Z,
                                        b.min.X, b.min.Y, b.min.Z, b.max.X, b.max.Y, b.max.Z };
    }

    public static IEnumerable<object[]> Points => new[]
    {
        new object[] { 0.5, 0.5, 0.5 },
        new object[] { 0.0, 0.0, 0.0 },
        new object[] { 1.0, 1.0, 1.0 },
        new object[] { -1.0, -1.0, -1.0 },
        new object[] { 5.0, 5.0, 5.0 },
        new object[] { -5.0, 0.0, 5.0 },
        new object[] { 100.0, 200.0, 300.0 },
        new object[] { 0.001, 0.002, 0.003 },
    };

    // =========================================================================
    // Size
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Size_EqualsMaxMinusMin(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        var size = box.Size;
        Assert.True(System.Math.Abs(size.X - (maxx - minx)) < 1e-10);
        Assert.True(System.Math.Abs(size.Y - (maxy - miny)) < 1e-10);
        Assert.True(System.Math.Abs(size.Z - (maxz - minz)) < 1e-10);
    }

    // =========================================================================
    // Center
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Center_IsMidpoint(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        var center = box.Center;
        Assert.True(System.Math.Abs(center.X - (minx + maxx) * 0.5) < 1e-10);
        Assert.True(System.Math.Abs(center.Y - (miny + maxy) * 0.5) < 1e-10);
        Assert.True(System.Math.Abs(center.Z - (minz + maxz) * 0.5) < 1e-10);
    }

    // =========================================================================
    // SurfaceArea
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void SurfaceArea_Correct(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        double dx = maxx - minx, dy = maxy - miny, dz = maxz - minz;
        double expected = 2.0 * (dx * dy + dy * dz + dz * dx);
        Assert.True(System.Math.Abs(box.SurfaceArea - expected) < 1e-8);
    }

    // =========================================================================
    // Contains: center always inside
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Contains_CenterAlwaysInside(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        Assert.True(box.Contains(box.Center));
    }

    // =========================================================================
    // Contains: Min inside
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Contains_MinInside(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        Assert.True(box.Contains(box.Min));
    }

    // =========================================================================
    // Contains: far point outside
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Contains_FarPointOutside(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        Assert.False(box.Contains(new Vec3(maxx + 100, maxy + 100, maxz + 100)));
    }

    // =========================================================================
    // Overlaps: self always overlaps
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Overlaps_Self(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        Assert.True(box.Overlaps(box));
    }

    // =========================================================================
    // Overlaps: symmetric
    // =========================================================================

    [Theory]
    [MemberData(nameof(BoxPairs))]
    public void Overlaps_Symmetric(double aminx, double aminy, double aminz, double amaxx, double amaxy, double amaxz,
                                    double bminx, double bminy, double bminz, double bmaxx, double bmaxy, double bmaxz)
    {
        var a = new Aabb(new Vec3(aminx, aminy, aminz), new Vec3(amaxx, amaxy, amaxz));
        var b = new Aabb(new Vec3(bminx, bminy, bminz), new Vec3(bmaxx, bmaxy, bmaxz));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    // =========================================================================
    // Union: commutative
    // =========================================================================

    [Theory]
    [MemberData(nameof(BoxPairs))]
    public void Union_Commutative(double aminx, double aminy, double aminz, double amaxx, double amaxy, double amaxz,
                                   double bminx, double bminy, double bminz, double bmaxx, double bmaxy, double bmaxz)
    {
        var a = new Aabb(new Vec3(aminx, aminy, aminz), new Vec3(amaxx, amaxy, amaxz));
        var b = new Aabb(new Vec3(bminx, bminy, bminz), new Vec3(bmaxx, bmaxy, bmaxz));
        Assert.Equal(Aabb.Union(a, b), Aabb.Union(b, a));
    }

    // =========================================================================
    // Union: contains both
    // =========================================================================

    [Theory]
    [MemberData(nameof(BoxPairs))]
    public void Union_ContainsBothCenters(double aminx, double aminy, double aminz, double amaxx, double amaxy, double amaxz,
                                          double bminx, double bminy, double bminz, double bmaxx, double bmaxy, double bmaxz)
    {
        var a = new Aabb(new Vec3(aminx, aminy, aminz), new Vec3(amaxx, amaxy, amaxz));
        var b = new Aabb(new Vec3(bminx, bminy, bminz), new Vec3(bmaxx, bmaxy, bmaxz));
        var u = Aabb.Union(a, b);
        Assert.True(u.Contains(a.Center));
        Assert.True(u.Contains(b.Center));
    }

    // =========================================================================
    // Union with self == self
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void Union_Self_IsSelf(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        Assert.Equal(box, Aabb.Union(box, box));
    }

    // =========================================================================
    // ExpandToInclude: result contains point
    // =========================================================================

    [Theory]
    [MemberData(nameof(Points))]
    public void ExpandToInclude_ContainsPoint(double px, double py, double pz)
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var point = new Vec3(px, py, pz);
        var expanded = box.ExpandToInclude(point);
        Assert.True(expanded.Contains(point));
    }

    // =========================================================================
    // ExpandToInclude: result contains original
    // =========================================================================

    [Theory]
    [MemberData(nameof(Points))]
    public void ExpandToInclude_ContainsOriginal(double px, double py, double pz)
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(px, py, pz));
        Assert.True(expanded.Contains(box.Center));
        Assert.True(expanded.Contains(box.Min));
    }

    // =========================================================================
    // FromTriangle: contains all three vertices
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1, 0, 0, 0, 1, 0)]
    [InlineData(0, 0, 0, 0, 0, 1, 0, 1, 0)]
    [InlineData(-1, -1, -1, 1, 1, 1, 0, 0, 0)]
    [InlineData(5, 5, 5, 6, 5, 5, 5, 6, 5)]
    [InlineData(-10, 0, 0, 10, 0, 0, 0, 10, 0)]
    public void FromTriangle_ContainsVertices(double ax, double ay, double az,
                                               double bx, double by, double bz,
                                               double cx, double cy, double cz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var c = new Vec3(cx, cy, cz);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    // =========================================================================
    // FromPoint: contains the point
    // =========================================================================

    [Theory]
    [MemberData(nameof(Points))]
    public void FromPoint_ContainsPoint(double x, double y, double z)
    {
        var p = new Vec3(x, y, z);
        var box = Aabb.FromPoint(p);
        Assert.True(box.Contains(p));
    }

    // =========================================================================
    // Overlaps: disjoint boxes don't overlap
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1, 1, 1, 5, 5, 5, 6, 6, 6)]
    [InlineData(0, 0, 0, 1, 1, 1, -5, -5, -5, -4, -4, -4)]
    [InlineData(0, 0, 0, 1, 1, 1, 2, 0, 0, 3, 1, 1)]
    [InlineData(0, 0, 0, 1, 1, 1, 0, 2, 0, 1, 3, 1)]
    [InlineData(0, 0, 0, 1, 1, 1, 0, 0, 2, 1, 1, 3)]
    public void Overlaps_DisjointBoxes_False(double aminx, double aminy, double aminz, double amaxx, double amaxy, double amaxz,
                                              double bminx, double bminy, double bminz, double bmaxx, double bmaxy, double bmaxz)
    {
        var a = new Aabb(new Vec3(aminx, aminy, aminz), new Vec3(amaxx, amaxy, amaxz));
        var b = new Aabb(new Vec3(bminx, bminy, bminz), new Vec3(bmaxx, bmaxy, bmaxz));
        Assert.False(a.Overlaps(b));
    }

    // =========================================================================
    // Overlaps: overlapping boxes
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 2, 2, 2, 1, 1, 1, 3, 3, 3)]
    [InlineData(-1, -1, -1, 1, 1, 1, 0, 0, 0, 2, 2, 2)]
    [InlineData(0, 0, 0, 10, 10, 10, 5, 5, 5, 15, 15, 15)]
    public void Overlaps_OverlappingBoxes_True(double aminx, double aminy, double aminz, double amaxx, double amaxy, double amaxz,
                                                double bminx, double bminy, double bminz, double bmaxx, double bmaxy, double bmaxz)
    {
        var a = new Aabb(new Vec3(aminx, aminy, aminz), new Vec3(amaxx, amaxy, amaxz));
        var b = new Aabb(new Vec3(bminx, bminy, bminz), new Vec3(bmaxx, bmaxy, bmaxz));
        Assert.True(a.Overlaps(b));
    }

    // =========================================================================
    // Expand: margin increases size
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1, 1, 1, 0.5)]
    [InlineData(-1, -1, -1, 1, 1, 1, 1.0)]
    [InlineData(0, 0, 0, 10, 10, 10, 0.1)]
    [InlineData(5, 5, 5, 6, 6, 6, 2.0)]
    public void Expand_IncreasesSize(double minx, double miny, double minz, double maxx, double maxy, double maxz, double margin)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        var expanded = box.Expand(margin);
        Assert.True(expanded.Size.X >= box.Size.X);
        Assert.True(expanded.Size.Y >= box.Size.Y);
        Assert.True(expanded.Size.Z >= box.Size.Z);
    }

    // =========================================================================
    // Expand: still contains original center
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1, 1, 1, 0.5)]
    [InlineData(-1, -1, -1, 1, 1, 1, 1.0)]
    [InlineData(0, 0, 0, 10, 10, 10, 0.1)]
    public void Expand_ContainsOriginalCenter(double minx, double miny, double minz, double maxx, double maxy, double maxz, double margin)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        var expanded = box.Expand(margin);
        Assert.True(expanded.Contains(box.Center));
    }

    // =========================================================================
    // SurfaceArea: unit cube == 6
    // =========================================================================

    [Fact]
    public void SurfaceArea_UnitCube_IsSix()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(System.Math.Abs(box.SurfaceArea - 6.0) < 1e-10);
    }

    // =========================================================================
    // SurfaceArea: non-negative
    // =========================================================================

    [Theory]
    [MemberData(nameof(Boxes))]
    public void SurfaceArea_NonNegative(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
        var box = new Aabb(new Vec3(minx, miny, minz), new Vec3(maxx, maxy, maxz));
        Assert.True(box.SurfaceArea >= 0);
    }
}
