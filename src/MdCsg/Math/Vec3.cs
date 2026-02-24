#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace MdCsg.Math;

/// <summary>
/// Immutable 3D vector with double-precision components.
/// </summary>
/// <param name="X">The X component.</param>
/// <param name="Y">The Y component.</param>
/// <param name="Z">The Z component.</param>
public readonly record struct Vec3(double X, double Y, double Z)
#if NET7_0_OR_GREATER
    : IAdditionOperators<Vec3, Vec3, Vec3>,
      ISubtractionOperators<Vec3, Vec3, Vec3>,
      IUnaryNegationOperators<Vec3, Vec3>,
      IMultiplyOperators<Vec3, double, Vec3>
#endif
{
    /// <summary>The zero vector.</summary>
    public static readonly Vec3 Zero = new(0, 0, 0);

    /// <summary>Unit X axis.</summary>
    public static readonly Vec3 UnitX = new(1, 0, 0);

    /// <summary>Unit Y axis.</summary>
    public static readonly Vec3 UnitY = new(0, 1, 0);

    /// <summary>Unit Z axis.</summary>
    public static readonly Vec3 UnitZ = new(0, 0, 1);

    /// <summary>Returns the dot product of two vectors.</summary>
    public static double Dot(Vec3 a, Vec3 b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    /// <summary>Returns the cross product of two vectors.</summary>
    public static Vec3 Cross(Vec3 a, Vec3 b) =>
        new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

    /// <summary>Returns the squared length of this vector.</summary>
    public double LengthSquared => X * X + Y * Y + Z * Z;

    /// <summary>Returns the length of this vector.</summary>
    public double Length => System.Math.Sqrt(LengthSquared);

    /// <summary>Returns a unit-length vector in the same direction.</summary>
    public Vec3 Normalized
    {
        get
        {
            var len = Length;
            return new Vec3(X / len, Y / len, Z / len);
        }
    }

    /// <summary>Snaps each component to the nearest grid point.</summary>
    public Vec3 SnapToGrid(double gridSize = MathUtil.DefaultGridSize) =>
        new(
            MathUtil.SnapToGrid(X, gridSize),
            MathUtil.SnapToGrid(Y, gridSize),
            MathUtil.SnapToGrid(Z, gridSize));

    /// <summary>Returns the component-wise minimum.</summary>
    public static Vec3 Min(Vec3 a, Vec3 b) =>
        new(System.Math.Min(a.X, b.X), System.Math.Min(a.Y, b.Y), System.Math.Min(a.Z, b.Z));

    /// <summary>Returns the component-wise maximum.</summary>
    public static Vec3 Max(Vec3 a, Vec3 b) =>
        new(System.Math.Max(a.X, b.X), System.Math.Max(a.Y, b.Y), System.Math.Max(a.Z, b.Z));

    /// <summary>Returns the distance between two points.</summary>
    public static double Distance(Vec3 a, Vec3 b) => (a - b).Length;

    /// <summary>Returns the squared distance between two points.</summary>
    public static double DistanceSquared(Vec3 a, Vec3 b) => (a - b).LengthSquared;

    /// <inheritdoc />
    public static Vec3 operator +(Vec3 left, Vec3 right) =>
        new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    /// <inheritdoc />
    public static Vec3 operator -(Vec3 left, Vec3 right) =>
        new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    /// <inheritdoc />
    public static Vec3 operator -(Vec3 value) =>
        new(-value.X, -value.Y, -value.Z);

    /// <inheritdoc />
    public static Vec3 operator *(Vec3 left, double right) =>
        new(left.X * right, left.Y * right, left.Z * right);

    /// <summary>Scalar multiplication (scalar on the left).</summary>
    public static Vec3 operator *(double left, Vec3 right) =>
        new(left * right.X, left * right.Y, left * right.Z);

    /// <summary>Component-wise division by a scalar.</summary>
    public static Vec3 operator /(Vec3 left, double right) =>
        new(left.X / right, left.Y / right, left.Z / right);

    /// <summary>Indexes into the vector components (0=X, 1=Y, 2=Z).</summary>
    public double this[int index] => index switch
    {
        0 => X,
        1 => Y,
        2 => Z,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    /// <inheritdoc />
    public override string ToString() => $"({X}, {Y}, {Z})";
}
