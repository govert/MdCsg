#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace MdCsg.Math;

/// <summary>
/// Immutable 2D vector with double-precision components.
/// </summary>
/// <param name="X">The X component.</param>
/// <param name="Y">The Y component.</param>
public readonly record struct Vec2(double X, double Y)
#if NET7_0_OR_GREATER
    : IAdditionOperators<Vec2, Vec2, Vec2>,
      ISubtractionOperators<Vec2, Vec2, Vec2>,
      IUnaryNegationOperators<Vec2, Vec2>,
      IMultiplyOperators<Vec2, double, Vec2>
#endif
{
    /// <summary>The zero vector.</summary>
    public static readonly Vec2 Zero = new(0, 0);

    /// <summary>Returns the dot product of two vectors.</summary>
    public static double Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

    /// <summary>Returns the 2D cross product (scalar z-component of the 3D cross).</summary>
    public static double Cross(Vec2 a, Vec2 b) => a.X * b.Y - a.Y * b.X;

    /// <summary>Returns the squared length of this vector.</summary>
    public double LengthSquared => X * X + Y * Y;

    /// <summary>Returns the length of this vector.</summary>
    public double Length => System.Math.Sqrt(LengthSquared);

    /// <summary>Returns a unit-length vector in the same direction.</summary>
    public Vec2 Normalized
    {
        get
        {
            var len = Length;
            return new Vec2(X / len, Y / len);
        }
    }

    /// <inheritdoc />
    public static Vec2 operator +(Vec2 left, Vec2 right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <inheritdoc />
    public static Vec2 operator -(Vec2 left, Vec2 right) =>
        new(left.X - right.X, left.Y - right.Y);

    /// <inheritdoc />
    public static Vec2 operator -(Vec2 value) =>
        new(-value.X, -value.Y);

    /// <inheritdoc />
    public static Vec2 operator *(Vec2 left, double right) =>
        new(left.X * right, left.Y * right);

    /// <summary>Scalar multiplication (scalar on the left).</summary>
    public static Vec2 operator *(double left, Vec2 right) =>
        new(left * right.X, left * right.Y);

    /// <summary>Component-wise division by a scalar.</summary>
    public static Vec2 operator /(Vec2 left, double right) =>
        new(left.X / right, left.Y / right);

    /// <inheritdoc />
    public override string ToString() => $"({X}, {Y})";
}
