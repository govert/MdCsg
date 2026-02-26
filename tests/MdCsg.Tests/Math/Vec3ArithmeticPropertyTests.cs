using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>
/// Comprehensive parameterized tests for Vec3 arithmetic, geometric, and algebraic properties.
/// </summary>
public class Vec3ArithmeticPropertyTests
{
    // =========================================================================
    // Test data
    // =========================================================================

    public static IEnumerable<object[]> Vectors => new[]
    {
        new object[] { 1.0, 0.0, 0.0 },
        new object[] { 0.0, 1.0, 0.0 },
        new object[] { 0.0, 0.0, 1.0 },
        new object[] { 1.0, 1.0, 1.0 },
        new object[] { -1.0, 2.0, -3.0 },
        new object[] { 0.5, -0.5, 0.5 },
        new object[] { 3.0, 4.0, 0.0 },
        new object[] { 0.0, 3.0, 4.0 },
        new object[] { 100.0, -200.0, 300.0 },
        new object[] { 0.001, 0.002, 0.003 },
    };

    public static IEnumerable<object[]> VectorPairs()
    {
        var vecs = new Vec3[]
        {
            new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
            new(1, 1, 1), new(-1, 2, -3), new(0.5, -0.5, 0.5),
            new(3, 4, 0), new(0, 3, 4), new(2, -1, 3),
        };
        foreach (var a in vecs)
        foreach (var b in vecs)
            yield return new object[] { a.X, a.Y, a.Z, b.X, b.Y, b.Z };
    }

    public static IEnumerable<object[]> VectorTriples()
    {
        var vecs = new Vec3[]
        {
            new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
            new(1, 1, 1), new(-1, 2, -3), new(2, -1, 3),
        };
        foreach (var a in vecs)
        foreach (var b in vecs)
        foreach (var c in vecs)
            yield return new object[] { a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z };
    }

    public static IEnumerable<object[]> Scalars => new[]
    {
        new object[] { 0.5 },
        new object[] { 1.0 },
        new object[] { 2.0 },
        new object[] { -1.0 },
        new object[] { -0.5 },
        new object[] { 0.1 },
        new object[] { 10.0 },
        new object[] { 100.0 },
    };

    // =========================================================================
    // Addition commutativity: a + b == b + a
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Addition_Commutative(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var ab = a + b;
        var ba = b + a;
        Assert.Equal(ab.X, ba.X, 10);
        Assert.Equal(ab.Y, ba.Y, 10);
        Assert.Equal(ab.Z, ba.Z, 10);
    }

    // =========================================================================
    // Addition associativity: (a + b) + c == a + (b + c)
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorTriples))]
    public void Addition_Associative(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var c = new Vec3(cx, cy, cz);
        var left = (a + b) + c;
        var right = a + (b + c);
        Assert.True(System.Math.Abs(left.X - right.X) < 1e-10);
        Assert.True(System.Math.Abs(left.Y - right.Y) < 1e-10);
        Assert.True(System.Math.Abs(left.Z - right.Z) < 1e-10);
    }

    // =========================================================================
    // Zero identity: a + Zero == a
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Addition_ZeroIdentity(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        var result = v + Vec3.Zero;
        Assert.Equal(v, result);
    }

    // =========================================================================
    // Negation: a + (-a) == Zero
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Negation_SumsToZero(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        var result = v + (-v);
        Assert.True(result.Length < 1e-10);
    }

    // =========================================================================
    // Subtraction: a - b == a + (-b)
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Subtraction_EqualsAddNegation(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var sub = a - b;
        var addNeg = a + (-b);
        Assert.Equal(sub.X, addNeg.X, 10);
        Assert.Equal(sub.Y, addNeg.Y, 10);
        Assert.Equal(sub.Z, addNeg.Z, 10);
    }

    // =========================================================================
    // Scalar multiplication distributivity: s * (a + b) == s*a + s*b
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0, 2.0)]
    [InlineData(1, 1, 1, -1, 2, -3, 0.5)]
    [InlineData(3, 4, 0, 0, 3, 4, -1.0)]
    [InlineData(0.5, -0.5, 0.5, 2, -1, 3, 10.0)]
    [InlineData(100, -200, 300, 0.001, 0.002, 0.003, 0.1)]
    public void ScalarMul_Distributive(double ax, double ay, double az, double bx, double by, double bz, double s)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var left = s * (a + b);
        var right = s * a + s * b;
        Assert.True(System.Math.Abs(left.X - right.X) < 1e-8);
        Assert.True(System.Math.Abs(left.Y - right.Y) < 1e-8);
        Assert.True(System.Math.Abs(left.Z - right.Z) < 1e-8);
    }

    // =========================================================================
    // Scalar mul commutativity: s * v == v * s
    // =========================================================================

    [Theory]
    [InlineData(1, 2, 3, 2.0)]
    [InlineData(-1, 0, 5, -3.0)]
    [InlineData(0, 0, 0, 100.0)]
    [InlineData(0.5, -0.5, 0.5, 0.1)]
    [InlineData(100, -200, 300, 0.001)]
    public void ScalarMul_Commutative(double x, double y, double z, double s)
    {
        var v = new Vec3(x, y, z);
        var sv = s * v;
        var vs = v * s;
        Assert.Equal(sv, vs);
    }

    // =========================================================================
    // Dot product commutativity: Dot(a, b) == Dot(b, a)
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Dot_Commutative(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a), 10);
    }

    // =========================================================================
    // Dot product linearity: Dot(s*a, b) == s * Dot(a, b)
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0, 2.0)]
    [InlineData(1, 1, 1, -1, 2, -3, 0.5)]
    [InlineData(3, 4, 0, 0, 3, 4, -1.0)]
    [InlineData(0.5, -0.5, 0.5, 2, -1, 3, 10.0)]
    public void Dot_Linear(double ax, double ay, double az, double bx, double by, double bz, double s)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        Assert.True(System.Math.Abs(Vec3.Dot(s * a, b) - s * Vec3.Dot(a, b)) < 1e-8);
    }

    // =========================================================================
    // Dot self == LengthSquared
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Dot_Self_EqualsLengthSquared(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.True(System.Math.Abs(Vec3.Dot(v, v) - v.LengthSquared) < 1e-10);
    }

    // =========================================================================
    // Cross product anti-commutativity: Cross(a, b) == -Cross(b, a)
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Cross_AntiCommutative(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.True(System.Math.Abs(ab.X + ba.X) < 1e-10);
        Assert.True(System.Math.Abs(ab.Y + ba.Y) < 1e-10);
        Assert.True(System.Math.Abs(ab.Z + ba.Z) < 1e-10);
    }

    // =========================================================================
    // Cross product orthogonality: Cross(a, b) · a == 0, Cross(a, b) · b == 0
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Cross_OrthogonalToBothInputs(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var c = Vec3.Cross(a, b);
        Assert.True(System.Math.Abs(Vec3.Dot(c, a)) < 1e-8);
        Assert.True(System.Math.Abs(Vec3.Dot(c, b)) < 1e-8);
    }

    // =========================================================================
    // Cross self == Zero
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Cross_Self_IsZero(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        var c = Vec3.Cross(v, v);
        Assert.True(c.Length < 1e-10);
    }

    // =========================================================================
    // Lagrange identity: |Cross(a,b)|^2 = |a|^2 * |b|^2 - (a·b)^2
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Cross_LagrangeIdentity(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var cross = Vec3.Cross(a, b);
        double crossSq = cross.LengthSquared;
        double dotAB = Vec3.Dot(a, b);
        double expected = a.LengthSquared * b.LengthSquared - dotAB * dotAB;
        Assert.True(System.Math.Abs(crossSq - expected) < 1e-6,
            $"|axb|^2={crossSq}, |a|^2*|b|^2-(a.b)^2={expected}");
    }

    // =========================================================================
    // Normalized length == 1
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Normalized_HasUnitLength(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        if (v.Length < 1e-15) return;
        Assert.True(System.Math.Abs(v.Normalized.Length - 1.0) < 1e-10);
    }

    // =========================================================================
    // Normalized direction preserved
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Normalized_DirectionPreserved(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        if (v.Length < 1e-15) return;
        var n = v.Normalized;
        double dot = Vec3.Dot(v, n);
        Assert.True(dot > 0, "Normalized vector should point same direction");
    }

    // =========================================================================
    // Distance symmetry
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Distance_Symmetric(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a), 10);
    }

    // =========================================================================
    // Distance self == 0
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Distance_Self_IsZero(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.Equal(0.0, Vec3.Distance(v, v), 10);
    }

    // =========================================================================
    // Distance via subtraction: Distance(a,b) == (a-b).Length
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Distance_EqualsSubtractionLength(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        Assert.True(System.Math.Abs(Vec3.Distance(a, b) - (a - b).Length) < 1e-10);
    }

    // =========================================================================
    // DistanceSquared == Distance^2
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void DistanceSquared_EqualsDistanceSquared(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        double d = Vec3.Distance(a, b);
        double dSq = Vec3.DistanceSquared(a, b);
        Assert.True(System.Math.Abs(dSq - d * d) < 1e-8);
    }

    // =========================================================================
    // Triangle inequality: Distance(a,c) <= Distance(a,b) + Distance(b,c)
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorTriples))]
    public void Distance_TriangleInequality(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var c = new Vec3(cx, cy, cz);
        double ac = Vec3.Distance(a, c);
        double ab = Vec3.Distance(a, b);
        double bc = Vec3.Distance(b, c);
        Assert.True(ac <= ab + bc + 1e-10,
            $"Triangle inequality: {ac} > {ab} + {bc}");
    }

    // =========================================================================
    // Indexer consistency
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Indexer_MatchesComponents(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.Equal(x, v[0]);
        Assert.Equal(y, v[1]);
        Assert.Equal(z, v[2]);
    }

    // =========================================================================
    // Min/Max component-wise
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Min_ComponentWise(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var min = Vec3.Min(a, b);
        Assert.Equal(System.Math.Min(ax, bx), min.X);
        Assert.Equal(System.Math.Min(ay, by), min.Y);
        Assert.Equal(System.Math.Min(az, bz), min.Z);
    }

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Max_ComponentWise(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var max = Vec3.Max(a, b);
        Assert.Equal(System.Math.Max(ax, bx), max.X);
        Assert.Equal(System.Math.Max(ay, by), max.Y);
        Assert.Equal(System.Math.Max(az, bz), max.Z);
    }

    // =========================================================================
    // Length non-negative
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Length_NonNegative(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.True(v.Length >= 0);
    }

    // =========================================================================
    // Length == sqrt(LengthSquared)
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Length_EqualsSqrtLengthSquared(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.True(System.Math.Abs(v.Length - System.Math.Sqrt(v.LengthSquared)) < 1e-10);
    }

    // =========================================================================
    // Scalar division: v / s == v * (1/s)
    // =========================================================================

    [Theory]
    [InlineData(1, 2, 3, 2.0)]
    [InlineData(-1, 0, 5, -3.0)]
    [InlineData(0.5, -0.5, 0.5, 0.1)]
    [InlineData(100, -200, 300, 10.0)]
    public void Division_EqualsMultiplyReciprocal(double x, double y, double z, double s)
    {
        var v = new Vec3(x, y, z);
        var div = v / s;
        var mulRecip = v * (1.0 / s);
        Assert.True(System.Math.Abs(div.X - mulRecip.X) < 1e-10);
        Assert.True(System.Math.Abs(div.Y - mulRecip.Y) < 1e-10);
        Assert.True(System.Math.Abs(div.Z - mulRecip.Z) < 1e-10);
    }

    // =========================================================================
    // Scalar mul by 1 is identity
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void ScalarMul_OneIsIdentity(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.Equal(v, v * 1.0);
    }

    // =========================================================================
    // Scalar mul by 0 is zero
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void ScalarMul_ZeroIsZero(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        var result = v * 0.0;
        Assert.Equal(Vec3.Zero, result);
    }

    // =========================================================================
    // Scalar mul by -1 is negation
    // =========================================================================

    [Theory]
    [MemberData(nameof(Vectors))]
    public void ScalarMul_NegOneIsNegation(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        Assert.Equal(-v, v * -1.0);
    }

    // =========================================================================
    // Unit vectors are orthonormal
    // =========================================================================

    [Fact]
    public void UnitVectors_AreOrthonormal()
    {
        Assert.Equal(1.0, Vec3.UnitX.Length, 10);
        Assert.Equal(1.0, Vec3.UnitY.Length, 10);
        Assert.Equal(1.0, Vec3.UnitZ.Length, 10);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY), 10);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitX, Vec3.UnitZ), 10);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ), 10);
    }

    // =========================================================================
    // Cross products of unit vectors
    // =========================================================================

    [Theory]
    [InlineData(0, 1, 2)]  // X x Y = Z
    [InlineData(1, 2, 0)]  // Y x Z = X
    [InlineData(2, 0, 1)]  // Z x X = Y
    public void UnitVectors_CrossProduct(int ai, int bi, int ci)
    {
        var units = new[] { Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        var cross = Vec3.Cross(units[ai], units[bi]);
        Assert.True(System.Math.Abs(cross.X - units[ci].X) < 1e-10);
        Assert.True(System.Math.Abs(cross.Y - units[ci].Y) < 1e-10);
        Assert.True(System.Math.Abs(cross.Z - units[ci].Z) < 1e-10);
    }

    // =========================================================================
    // Cauchy-Schwarz: |Dot(a,b)| <= |a| * |b|
    // =========================================================================

    [Theory]
    [MemberData(nameof(VectorPairs))]
    public void Dot_CauchySchwarz(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        Assert.True(System.Math.Abs(Vec3.Dot(a, b)) <= a.Length * b.Length + 1e-10);
    }

    // =========================================================================
    // Scaling preserves direction
    // =========================================================================

    [Theory]
    [InlineData(1, 2, 3, 2.0)]
    [InlineData(-1, 0, 5, 3.0)]
    [InlineData(0.5, -0.5, 0.5, 100.0)]
    public void Scaling_PreservesDirection(double x, double y, double z, double s)
    {
        var v = new Vec3(x, y, z);
        if (v.Length < 1e-15) return;
        var scaled = v * s;
        if (s > 0)
        {
            double dot = Vec3.Dot(v.Normalized, scaled.Normalized);
            Assert.True(System.Math.Abs(dot - 1.0) < 1e-10);
        }
    }

    // =========================================================================
    // Scaling scales length
    // =========================================================================

    [Theory]
    [InlineData(1, 2, 3, 2.0)]
    [InlineData(-1, 0, 5, 3.0)]
    [InlineData(0.5, -0.5, 0.5, 0.1)]
    [InlineData(100, -200, 300, 0.01)]
    public void Scaling_ScalesLength(double x, double y, double z, double s)
    {
        var v = new Vec3(x, y, z);
        var scaled = v * s;
        Assert.True(System.Math.Abs(scaled.Length - v.Length * System.Math.Abs(s)) < 1e-8);
    }
}
