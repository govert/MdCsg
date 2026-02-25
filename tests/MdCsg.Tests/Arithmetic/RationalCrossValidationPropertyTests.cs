using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — cross-validation with double, edge cases, algebraic properties</summary>
public class RationalCrossValidationPropertyTests
{
    [Fact]
    public void FromDouble_Zero_IsZero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(Rational.Zero, r);
    }

    [Fact]
    public void FromDouble_One_IsOne()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(Rational.One, r);
    }

    [Fact]
    public void FromDouble_NegativeOne_IsNegOne()
    {
        var r = Rational.FromDouble(-1.0);
        Assert.Equal(-Rational.One, r);
    }

    [Fact]
    public void FromDouble_Half_IsHalf()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(new Rational(1, 2), r);
    }

    [Fact]
    public void Add_OneAndOne_IsTwo()
    {
        Assert.Equal(new Rational(2, 1), Rational.One + Rational.One);
    }

    [Fact]
    public void Subtract_OneMinusOne_IsZero()
    {
        Assert.Equal(Rational.Zero, Rational.One - Rational.One);
    }

    [Fact]
    public void Multiply_TwoTimesThree_IsSix()
    {
        var two = new Rational(2, 1);
        var three = new Rational(3, 1);
        Assert.Equal(new Rational(6, 1), two * three);
    }

    [Fact]
    public void Divide_SixByTwo_IsThree()
    {
        var six = new Rational(6, 1);
        var two = new Rational(2, 1);
        Assert.Equal(new Rational(3, 1), six / two);
    }

    [Fact]
    public void Associativity_Add()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 7);
        var c = new Rational(1, 11);
        Assert.Equal((a + b) + c, a + (b + c));
    }

    [Fact]
    public void Commutativity_Add()
    {
        var a = new Rational(3, 5);
        var b = new Rational(7, 11);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Commutativity_Multiply()
    {
        var a = new Rational(3, 5);
        var b = new Rational(7, 11);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Distributivity()
    {
        var a = new Rational(2, 3);
        var b = new Rational(4, 5);
        var c = new Rational(6, 7);
        Assert.Equal(a * (b + c), a * b + a * c);
    }

    [Fact]
    public void MultiplicativeIdentity()
    {
        var r = new Rational(17, 13);
        Assert.Equal(r, r * Rational.One);
    }

    [Fact]
    public void AdditiveIdentity()
    {
        var r = new Rational(17, 13);
        Assert.Equal(r, r + Rational.Zero);
    }

    [Fact]
    public void Comparison_LessThan()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a < b);
        Assert.False(b < a);
    }

    [Fact]
    public void Comparison_GreaterThan()
    {
        var a = new Rational(2, 3);
        var b = new Rational(1, 3);
        Assert.True(a > b);
    }

    [Fact]
    public void Comparison_Equal()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Negate_DoubleNegate_IsOriginal()
    {
        var r = new Rational(7, 11);
        Assert.Equal(r, -(-r));
    }

    [Fact]
    public void FromDouble_RoundTrip_Integer()
    {
        for (int i = -10; i <= 10; i++)
        {
            var r = Rational.FromDouble(i);
            Assert.Equal(new Rational(i, 1), r);
        }
    }

    [Fact]
    public void FromDouble_PowerOfTwo_Exact()
    {
        var r = Rational.FromDouble(0.25);
        Assert.Equal(new Rational(1, 4), r);
    }
}
