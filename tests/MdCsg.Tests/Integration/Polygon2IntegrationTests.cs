using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 38: Polygon operations: area, contains, offset.</summary>
public class Polygon2IntegrationTests
{
    [Fact]
    public void UnitSquare_AreaIsOne()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1)
        });
        Assert.True(System.Math.Abs(poly.Area - 1.0) < 1e-10);
    }

    [Fact]
    public void UnitSquare_SignedArea_CCW_Positive()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1)
        });
        Assert.True(poly.SignedArea > 0);
    }

    [Fact]
    public void UnitSquare_CW_SignedAreaNegative()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(0, 1), new Vec2(1, 1), new Vec2(1, 0)
        });
        Assert.True(poly.SignedArea < 0);
    }

    [Fact]
    public void Triangle_AreaIsHalfBaseTimesHeight()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(4, 0), new Vec2(0, 3)
        });
        Assert.True(System.Math.Abs(poly.Area - 6.0) < 1e-10);
    }

    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        Assert.True(poly.Contains(new Vec2(5, 5)));
    }

    [Fact]
    public void Contains_PointOutside_ReturnsFalse()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        Assert.False(poly.Contains(new Vec2(15, 5)));
    }

    [Fact]
    public void Contains_PointFarOutside_ReturnsFalse()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1)
        });
        Assert.False(poly.Contains(new Vec2(100, 100)));
    }

    [Fact]
    public void Reversed_FlipsSignedArea()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1)
        });
        var reversed = poly.Reversed();
        Assert.True(System.Math.Abs(poly.SignedArea + reversed.SignedArea) < 1e-10);
    }

    [Fact]
    public void IsClockwise_CW_ReturnsTrue()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(0, 1), new Vec2(1, 1), new Vec2(1, 0)
        });
        Assert.True(poly.IsClockwise);
    }

    [Fact]
    public void IsClockwise_CCW_ReturnsFalse()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1)
        });
        Assert.False(poly.IsClockwise);
    }

    [Fact]
    public void Offset_Positive_IncreasesArea()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        var expanded = poly.Offset(1.0);
        Assert.True(expanded.Area > poly.Area);
    }

    [Fact]
    public void Offset_Negative_DecreasesArea()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        var shrunk = poly.Offset(-1.0);
        Assert.True(shrunk.Area < poly.Area);
    }

    [Fact]
    public void Offset_Zero_PreservesArea()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        var same = poly.Offset(0.0);
        Assert.True(System.Math.Abs(same.Area - poly.Area) < 1e-8);
    }

    [Fact]
    public void LargePolygon_AreaIsCorrect()
    {
        // Regular hexagon with radius 10
        int n = 6;
        var verts = new Vec2[n];
        for (int i = 0; i < n; i++)
        {
            double angle = 2 * System.Math.PI * i / n;
            verts[i] = new Vec2(10 * System.Math.Cos(angle), 10 * System.Math.Sin(angle));
        }
        var poly = new Polygon2(verts);
        double expected = 3 * System.Math.Sqrt(3) / 2 * 100; // 3*sqrt(3)/2 * r^2
        Assert.True(System.Math.Abs(poly.Area - expected) < 0.1);
    }

    [Fact]
    public void Contains_ManyPointsInsideSquare()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        int insideCount = 0;
        for (int x = 1; x <= 9; x++)
            for (int y = 1; y <= 9; y++)
                if (poly.Contains(new Vec2(x, y)))
                    insideCount++;
        Assert.Equal(81, insideCount);
    }

    [Fact]
    public void Vertices_ArePreserved()
    {
        var verts = new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0.5, 1) };
        var poly = new Polygon2(verts);
        Assert.Equal(3, poly.Vertices.Length);
    }

    [Fact]
    public void Reversed_PreservesArea()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 3), new Vec2(0, 3)
        });
        var reversed = poly.Reversed();
        Assert.True(System.Math.Abs(poly.Area - reversed.Area) < 1e-10);
    }

    [Fact]
    public void ConcavePolygon_AreaCorrect()
    {
        // L-shape: 3x3 square minus 1.5x1.5 corner = 9 - 2.25 = 6.75
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 1.5),
            new Vec2(1.5, 1.5), new Vec2(1.5, 3), new Vec2(0, 3)
        });
        Assert.True(System.Math.Abs(poly.Area - 6.75) < 1e-10);
    }

    [Fact]
    public void Contains_TriangleInterior()
    {
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(5, 10)
        });
        Assert.True(poly.Contains(new Vec2(5, 3)));
        Assert.False(poly.Contains(new Vec2(0, 10)));
    }

    [Fact]
    public void Offset_SquareByOne_AreaIncreasesCorrectly()
    {
        // 10x10 square offset by 1 → ~12x12 with rounded corners
        var poly = new Polygon2(new[] {
            new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10)
        });
        var expanded = poly.Offset(1.0);
        // Area should be roughly (10+2)*(10+2) = 144, minus corner differences
        Assert.True(expanded.Area > 130);
        Assert.True(expanded.Area < 160);
    }
}
