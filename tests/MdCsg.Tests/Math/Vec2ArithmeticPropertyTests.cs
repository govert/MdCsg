using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>
/// Comprehensive parameterized tests for Vec2 arithmetic and geometric properties.
/// </summary>
public class Vec2ArithmeticPropertyTests
{
    public static IEnumerable<object[]> Vectors => new[]
    {
        new object[] { 1.0, 0.0 },
        new object[] { 0.0, 1.0 },
        new object[] { 1.0, 1.0 },
        new object[] { -1.0, 2.0 },
        new object[] { 0.5, -0.5 },
        new object[] { 3.0, 4.0 },
        new object[] { -3.0, 4.0 },
        new object[] { 100.0, -200.0 },
        new object[] { 0.001, 0.002 },
        new object[] { -7.0, -7.0 },
    };

    public static IEnumerable<object[]> VectorPairs()
    {
        var vecs = new Vec2[]
        {
            new(1, 0), new(0, 1), new(1, 1),
            new(-1, 2), new(0.5, -0.5), new(3, 4),
            new(-3, 4), new(2, -1), new(0.1, 0.2),
        };
        foreach (var a in vecs)
        foreach (var b in vecs)
            yield return new object[] { a.X, a.Y, b.X, b.Y };
    }

    public static IEnumerable<object[]> VectorTriples()
    {
        var vecs = new Vec2[]
        {
            new(1, 0), new(0, 1), new(1, 1),
            new(-1, 2), new(0.5, -0.5), new(3, 4),
        };
        foreach (var a in vecs)
        foreach (var b in vecs)
        foreach (var c in vecs)
            yield return new object[] { a.X, a.Y, b.X, b.Y, c.X, c.Y };
    }

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Addition_Commutative(double ax, double ay, double bx, double by)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        var ab = a + b;
        var ba = b + a;
        Assert.Equal(ab.X, ba.X, 10);
        Assert.Equal(ab.Y, ba.Y, 10);
    }

    [Theory]
    [MemberData(nameof(VectorTriples))]
    public void Addition_Associative(double ax, double ay, double bx, double by, double cx, double cy)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        var c = new Vec2(cx, cy);
        var left = (a + b) + c;
        var right = a + (b + c);
        Assert.True(System.Math.Abs(left.X - right.X) < 1e-10);
        Assert.True(System.Math.Abs(left.Y - right.Y) < 1e-10);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Addition_ZeroIdentity(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.Equal(v, v + Vec2.Zero);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Negation_SumsToZero(double x, double y)
    {
        var v = new Vec2(x, y);
        var result = v + (-v);
        Assert.True(result.Length < 1e-10);
    }

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Subtraction_EqualsAddNegation(double ax, double ay, double bx, double by)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        var sub = a - b;
        var addNeg = a + (-b);
        Assert.Equal(sub.X, addNeg.X, 10);
        Assert.Equal(sub.Y, addNeg.Y, 10);
    }

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Dot_Commutative(double ax, double ay, double bx, double by)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        Assert.Equal(Vec2.Dot(a, b), Vec2.Dot(b, a), 10);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Dot_Self_EqualsLengthSquared(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.True(System.Math.Abs(Vec2.Dot(v, v) - v.LengthSquared) < 1e-10);
    }

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Cross_AntiCommutative(double ax, double ay, double bx, double by)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        Assert.True(System.Math.Abs(Vec2.Cross(a, b) + Vec2.Cross(b, a)) < 1e-10);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Cross_Self_IsZero(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.True(System.Math.Abs(Vec2.Cross(v, v)) < 1e-10);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Normalized_HasUnitLength(double x, double y)
    {
        var v = new Vec2(x, y);
        if (v.Length < 1e-15) return;
        Assert.True(System.Math.Abs(v.Normalized.Length - 1.0) < 1e-10);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Length_NonNegative(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.True(v.Length >= 0);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Length_EqualsSqrtLengthSquared(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.True(System.Math.Abs(v.Length - System.Math.Sqrt(v.LengthSquared)) < 1e-10);
    }

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void CauchySchwarz_DotBounded(double ax, double ay, double bx, double by)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        Assert.True(System.Math.Abs(Vec2.Dot(a, b)) <= a.Length * b.Length + 1e-10);
    }

    [Theory]
    [InlineData(1, 2, 2.0)]
    [InlineData(-1, 0, -3.0)]
    [InlineData(0.5, -0.5, 0.1)]
    [InlineData(100, -200, 0.01)]
    public void Scaling_ScalesLength(double x, double y, double s)
    {
        var v = new Vec2(x, y);
        var scaled = v * s;
        Assert.True(System.Math.Abs(scaled.Length - v.Length * System.Math.Abs(s)) < 1e-8);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void ScalarMul_OneIsIdentity(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.Equal(v, v * 1.0);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void ScalarMul_ZeroIsZero(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.Equal(Vec2.Zero, v * 0.0);
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void ScalarMul_NegOneIsNegation(double x, double y)
    {
        var v = new Vec2(x, y);
        Assert.Equal(-v, v * -1.0);
    }

    [Theory]
    [InlineData(1, 2, 2.0)]
    [InlineData(-1, 0, -3.0)]
    [InlineData(0.5, -0.5, 0.1)]
    public void ScalarMul_Commutative(double x, double y, double s)
    {
        var v = new Vec2(x, y);
        Assert.Equal(s * v, v * s);
    }

    [Theory]
    [InlineData(1, 0, 0, 1, 2.0)]
    [InlineData(1, 1, -1, 2, 0.5)]
    [InlineData(3, 4, -3, 4, -1.0)]
    public void ScalarMul_Distributive(double ax, double ay, double bx, double by, double s)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        var left = s * (a + b);
        var right = s * a + s * b;
        Assert.True(System.Math.Abs(left.X - right.X) < 1e-8);
        Assert.True(System.Math.Abs(left.Y - right.Y) < 1e-8);
    }

    // Cross product = area of parallelogram
    [Theory]
    [InlineData(1, 0, 0, 1, 1.0)]   // unit square area
    [InlineData(2, 0, 0, 3, 6.0)]   // rectangle
    [InlineData(1, 0, 1, 1, 1.0)]   // parallelogram
    public void Cross_EqualsParallelogramArea(double ax, double ay, double bx, double by, double expectedArea)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        Assert.True(System.Math.Abs(Vec2.Cross(a, b) - expectedArea) < 1e-10);
    }

    // Lagrange identity 2D: Cross(a,b)^2 + Dot(a,b)^2 = |a|^2 * |b|^2
    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void LagrangeIdentity_2D(double ax, double ay, double bx, double by)
    {
        var a = new Vec2(ax, ay);
        var b = new Vec2(bx, by);
        double crossSq = Vec2.Cross(a, b) * Vec2.Cross(a, b);
        double dotSq = Vec2.Dot(a, b) * Vec2.Dot(a, b);
        double expected = a.LengthSquared * b.LengthSquared;
        Assert.True(System.Math.Abs(crossSq + dotSq - expected) < 1e-6);
    }

    [Theory]
    [InlineData(1, 2, 2.0)]
    [InlineData(-1, 0, -3.0)]
    [InlineData(0.5, -0.5, 0.1)]
    public void Division_EqualsMultiplyReciprocal(double x, double y, double s)
    {
        var v = new Vec2(x, y);
        var div = v / s;
        var mulRecip = v * (1.0 / s);
        Assert.True(System.Math.Abs(div.X - mulRecip.X) < 1e-10);
        Assert.True(System.Math.Abs(div.Y - mulRecip.Y) < 1e-10);
    }
}
