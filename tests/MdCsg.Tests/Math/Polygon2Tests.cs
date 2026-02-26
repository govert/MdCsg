using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Polygon2Tests
{
    private const double Tol = 1e-10;

    private static Polygon2 UnitSquareCCW() => new(new[]
    {
        new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1)
    });

    private static Polygon2 UnitSquareCW() => new(new[]
    {
        new Vec2(0, 0), new Vec2(0, 1), new Vec2(1, 1), new Vec2(1, 0)
    });

    private static Polygon2 Triangle() => new(new[]
    {
        new Vec2(0, 0), new Vec2(4, 0), new Vec2(0, 3)
    });

    // ── SignedArea ───────────────────────────────────────────────────

    [Fact]
    public void SignedArea_CCWSquare_Positive()
    {
        Assert.True(System.Math.Abs(UnitSquareCCW().SignedArea - 1.0) < Tol);
    }

    [Fact]
    public void SignedArea_CWSquare_Negative()
    {
        Assert.True(System.Math.Abs(UnitSquareCW().SignedArea + 1.0) < Tol);
    }

    [Fact]
    public void SignedArea_Triangle_HalfBaseTimesHeight()
    {
        Assert.True(System.Math.Abs(Triangle().SignedArea - 6.0) < Tol);
    }

    // ── Area ────────────────────────────────────────────────────────

    [Fact]
    public void Area_AlwaysPositive()
    {
        Assert.True(UnitSquareCCW().Area > 0);
        Assert.True(UnitSquareCW().Area > 0);
    }

    [Fact]
    public void Area_CWAndCCW_SameValue()
    {
        Assert.True(System.Math.Abs(UnitSquareCCW().Area - UnitSquareCW().Area) < Tol);
    }

    // ── IsClockwise ─────────────────────────────────────────────────

    [Fact]
    public void IsClockwise_CW_True()
    {
        Assert.True(UnitSquareCW().IsClockwise);
    }

    [Fact]
    public void IsClockwise_CCW_False()
    {
        Assert.False(UnitSquareCCW().IsClockwise);
    }

    // ── Reversed ────────────────────────────────────────────────────

    [Fact]
    public void Reversed_FlipsWinding()
    {
        var ccw = UnitSquareCCW();
        var reversed = ccw.Reversed();
        Assert.True(reversed.IsClockwise);
        Assert.True(System.Math.Abs(reversed.Area - ccw.Area) < Tol);
    }

    [Fact]
    public void Reversed_DoubleReverse_SameVertices()
    {
        var orig = UnitSquareCCW();
        var doubled = orig.Reversed().Reversed();
        for (int i = 0; i < orig.Vertices.Length; i++)
        {
            Assert.True(System.Math.Abs(orig.Vertices[i].X - doubled.Vertices[i].X) < Tol);
            Assert.True(System.Math.Abs(orig.Vertices[i].Y - doubled.Vertices[i].Y) < Tol);
        }
    }

    // ── Contains ────────────────────────────────────────────────────

    [Fact]
    public void Contains_InsideSquare_True()
    {
        Assert.True(UnitSquareCCW().Contains(new Vec2(0.5, 0.5)));
    }

    [Fact]
    public void Contains_OutsideSquare_False()
    {
        Assert.False(UnitSquareCCW().Contains(new Vec2(2, 2)));
        Assert.False(UnitSquareCCW().Contains(new Vec2(-1, 0.5)));
    }

    [Fact]
    public void Contains_InsideTriangle_True()
    {
        Assert.True(Triangle().Contains(new Vec2(0.5, 0.5)));
    }

    [Fact]
    public void Contains_OutsideTriangle_False()
    {
        Assert.False(Triangle().Contains(new Vec2(3, 3)));
    }

    [Fact]
    public void Contains_CW_StillWorks()
    {
        Assert.True(UnitSquareCW().Contains(new Vec2(0.5, 0.5)));
        Assert.False(UnitSquareCW().Contains(new Vec2(2, 2)));
    }

    [Fact]
    public void Contains_ConcavePolygon()
    {
        // L-shaped polygon
        var poly = new Polygon2(new[]
        {
            new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 1),
            new Vec2(1, 1), new Vec2(1, 2), new Vec2(0, 2)
        });
        Assert.True(poly.Contains(new Vec2(0.5, 0.5)));   // inside bottom
        Assert.True(poly.Contains(new Vec2(0.5, 1.5)));   // inside left arm
        Assert.False(poly.Contains(new Vec2(1.5, 1.5)));  // in the notch
    }

    // ── Offset ──────────────────────────────────────────────────────

    [Fact]
    public void Offset_ExpandsSquare()
    {
        var sq = UnitSquareCCW();
        var expanded = sq.Offset(0.5);
        // Expanded square should have area > original
        Assert.True(expanded.Area > sq.Area);
    }

    [Fact]
    public void Offset_Zero_SameArea()
    {
        var sq = UnitSquareCCW();
        var same = sq.Offset(0);
        Assert.True(System.Math.Abs(same.Area - sq.Area) < 1e-8);
    }

    [Fact]
    public void Offset_NegativeContracts()
    {
        var sq = UnitSquareCCW();
        var contracted = sq.Offset(-0.1);
        Assert.True(contracted.Area < sq.Area);
    }

    [Fact]
    public void Offset_SquareByD_AreaIncreasesBy4D()
    {
        // For a unit square offset by d, area = (1+2d)^2
        double d = 0.25;
        var sq = UnitSquareCCW();
        var expanded = sq.Offset(d);
        double expectedArea = (1 + 2 * d) * (1 + 2 * d);
        Assert.True(System.Math.Abs(expanded.Area - expectedArea) < 1e-8,
            $"Expected area {expectedArea}, got {expanded.Area}");
    }
}
