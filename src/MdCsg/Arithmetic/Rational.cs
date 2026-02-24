using System.Numerics;

namespace MdCsg.Arithmetic;

/// <summary>
/// Exact rational number using BigInteger numerator and denominator.
/// Used as the last-resort exact arithmetic fallback when expansion arithmetic
/// cannot resolve the sign.
/// </summary>
public readonly struct Rational : IEquatable<Rational>, IComparable<Rational>
{
    /// <summary>The numerator.</summary>
    public BigInteger Numerator { get; }

    /// <summary>The denominator (always positive after construction).</summary>
    public BigInteger Denominator { get; }

    /// <summary>Zero.</summary>
    public static readonly Rational Zero = new(BigInteger.Zero, BigInteger.One);

    /// <summary>One.</summary>
    public static readonly Rational One = new(BigInteger.One, BigInteger.One);

    /// <summary>
    /// Creates a rational number and normalizes it (positive denominator, reduced form).
    /// </summary>
    public Rational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero)
            throw new DivideByZeroException("Rational denominator cannot be zero.");

        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    /// <summary>
    /// Converts a double to an exact rational by decomposing the IEEE 754 representation.
    /// </summary>
    public static Rational FromDouble(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Cannot convert NaN or Infinity to Rational.", nameof(value));

        if (value == 0.0)
            return Zero;

        // Decompose IEEE 754 double
        long bits = BitConverter.DoubleToInt64Bits(value);
        int sign = (bits >> 63) != 0 ? -1 : 1;
        int exponent = (int)((bits >> 52) & 0x7FF) - 1023;
        long mantissa = bits & 0x000FFFFFFFFFFFFF;

        BigInteger num;
        BigInteger den;

        if (exponent == -1023)
        {
            // Subnormal: value = (-1)^sign * mantissa * 2^(-1074)
            num = sign * (BigInteger)mantissa;
            den = BigInteger.One << 1074;
        }
        else
        {
            // Normal: value = (-1)^sign * (1 + mantissa/2^52) * 2^exponent
            num = sign * ((BigInteger.One << 52) + mantissa);
            int shift = exponent - 52;
            if (shift >= 0)
            {
                num <<= shift;
                den = BigInteger.One;
            }
            else
            {
                den = BigInteger.One << (-shift);
            }
        }

        return new Rational(num, den);
    }

    /// <summary>Returns the sign: -1, 0, or +1.</summary>
    public int Sign => Numerator.Sign;

    /// <inheritdoc />
    public static Rational operator +(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator + b.Numerator * a.Denominator,
            a.Denominator * b.Denominator);

    /// <inheritdoc />
    public static Rational operator -(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator);

    /// <inheritdoc />
    public static Rational operator *(Rational a, Rational b) =>
        new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

    /// <inheritdoc />
    public static Rational operator /(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);

    /// <inheritdoc />
    public static Rational operator -(Rational a) =>
        new(-a.Numerator, a.Denominator);

    /// <inheritdoc />
    public static bool operator ==(Rational a, Rational b) => a.Equals(b);

    /// <inheritdoc />
    public static bool operator !=(Rational a, Rational b) => !a.Equals(b);

    /// <inheritdoc />
    public static bool operator <(Rational a, Rational b) => a.CompareTo(b) < 0;

    /// <inheritdoc />
    public static bool operator >(Rational a, Rational b) => a.CompareTo(b) > 0;

    /// <inheritdoc />
    public static bool operator <=(Rational a, Rational b) => a.CompareTo(b) <= 0;

    /// <inheritdoc />
    public static bool operator >=(Rational a, Rational b) => a.CompareTo(b) >= 0;

    /// <inheritdoc />
    public int CompareTo(Rational other) =>
        (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

    /// <inheritdoc />
    public bool Equals(Rational other) =>
        Numerator == other.Numerator && Denominator == other.Denominator;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Rational r && Equals(r);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    /// <inheritdoc />
    public override string ToString() =>
        Denominator == BigInteger.One ? $"{Numerator}" : $"{Numerator}/{Denominator}";
}
