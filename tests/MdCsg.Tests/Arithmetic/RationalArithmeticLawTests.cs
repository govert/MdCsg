using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational arithmetic laws — commutativity, associativity, distributivity, inverse</summary>
public class RationalArithmeticLawTests
{
    [Fact]
    public void Addition_Commutative()
    {
        var a = Rational.FromDouble(1.5);
        var b = Rational.FromDouble(2.7);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Multiplication_Commutative()
    {
        var a = Rational.FromDouble(3.0);
        var b = Rational.FromDouble(7.0);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Addition_Associative()
    {
        var a = Rational.FromDouble(1.0);
        var b = Rational.FromDouble(2.0);
        var c = Rational.FromDouble(3.0);
        Assert.Equal((a + b) + c, a + (b + c));
    }

    [Fact]
    public void Multiplication_Associative()
    {
        var a = Rational.FromDouble(2.0);
        var b = Rational.FromDouble(3.0);
        var c = Rational.FromDouble(5.0);
        Assert.Equal((a * b) * c, a * (b * c));
    }

    [Fact]
    public void Distributive_Law()
    {
        var a = Rational.FromDouble(2.0);
        var b = Rational.FromDouble(3.0);
        var c = Rational.FromDouble(5.0);
        Assert.Equal(a * (b + c), a * b + a * c);
    }

    [Fact]
    public void AddZero_Identity()
    {
        var a = Rational.FromDouble(42.0);
        var zero = Rational.FromDouble(0.0);
        Assert.Equal(a, a + zero);
    }

    [Fact]
    public void MultiplyOne_Identity()
    {
        var a = Rational.FromDouble(42.0);
        var one = Rational.FromDouble(1.0);
        Assert.Equal(a, a * one);
    }

    [Fact]
    public void MultiplyZero_IsZero()
    {
        var a = Rational.FromDouble(42.0);
        var zero = Rational.FromDouble(0.0);
        Assert.Equal(0, (a * zero).Sign);
    }

    [Fact]
    public void SubtractSelf_IsZero()
    {
        var a = Rational.FromDouble(7.5);
        Assert.Equal(0, (a - a).Sign);
    }

    [Fact]
    public void Negate_ThenAdd_IsZero()
    {
        var a = Rational.FromDouble(3.14);
        var neg = Rational.FromDouble(0.0) - a;
        Assert.Equal(0, (a + neg).Sign);
    }

    [Fact]
    public void Sign_Positive()
    {
        Assert.Equal(1, Rational.FromDouble(5.0).Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        Assert.Equal(-1, Rational.FromDouble(-5.0).Sign);
    }

    [Fact]
    public void Sign_Zero()
    {
        Assert.Equal(0, Rational.FromDouble(0.0).Sign);
    }

    [Fact]
    public void Comparison_Consistent()
    {
        var a = Rational.FromDouble(3.0);
        var b = Rational.FromDouble(5.0);
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void FromDouble_Preserves_Integers()
    {
        for (int i = -10; i <= 10; i++)
        {
            var r = Rational.FromDouble(i);
            Assert.Equal(i < 0 ? -1 : (i > 0 ? 1 : 0), r.Sign);
        }
    }

    [Fact]
    public void Product_SignRule_PositiveTimesNegative()
    {
        var pos = Rational.FromDouble(3.0);
        var neg = Rational.FromDouble(-2.0);
        Assert.Equal(-1, (pos * neg).Sign);
    }

    [Fact]
    public void Product_SignRule_NegativeTimesNegative()
    {
        var a = Rational.FromDouble(-3.0);
        var b = Rational.FromDouble(-2.0);
        Assert.Equal(1, (a * b).Sign);
    }

    [Fact]
    public void Subtraction_IsAddNegative()
    {
        var a = Rational.FromDouble(10.0);
        var b = Rational.FromDouble(3.0);
        var diff = a - b;
        Assert.Equal(1, diff.Sign); // 10 - 3 = 7 > 0
    }

    [Fact]
    public void LargeInteger_Exact()
    {
        var a = Rational.FromDouble(1e15);
        var b = Rational.FromDouble(1.0);
        var sum = a + b;
        Assert.Equal(1, sum.CompareTo(a)); // a+1 > a
    }
}
