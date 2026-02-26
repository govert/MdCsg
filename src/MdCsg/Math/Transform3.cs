namespace MdCsg.Math;

/// <summary>
/// An affine 3D transform consisting of a rotation (3x3 matrix) and a translation.
/// </summary>
/// <param name="Rotation">The 3x3 rotation/scale matrix.</param>
/// <param name="Translation">The translation vector.</param>
public readonly record struct Transform3(Mat3 Rotation, Vec3 Translation)
{
    /// <summary>The identity transform (no rotation, no translation).</summary>
    public static readonly Transform3 Identity = new(Mat3.Identity, Vec3.Zero);

    /// <summary>Creates a pure translation transform.</summary>
    /// <param name="offset">The translation offset.</param>
    /// <returns>A translation-only transform.</returns>
    public static Transform3 FromTranslation(Vec3 offset) =>
        new(Mat3.Identity, offset);

    /// <summary>Creates a rotation around the X axis.</summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>The rotation transform.</returns>
    public static Transform3 FromRotationX(double radians)
    {
        double c = System.Math.Cos(radians);
        double s = System.Math.Sin(radians);
        return new(new Mat3(1, 0, 0, 0, c, -s, 0, s, c), Vec3.Zero);
    }

    /// <summary>Creates a rotation around the Y axis.</summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>The rotation transform.</returns>
    public static Transform3 FromRotationY(double radians)
    {
        double c = System.Math.Cos(radians);
        double s = System.Math.Sin(radians);
        return new(new Mat3(c, 0, s, 0, 1, 0, -s, 0, c), Vec3.Zero);
    }

    /// <summary>Creates a rotation around the Z axis.</summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>The rotation transform.</returns>
    public static Transform3 FromRotationZ(double radians)
    {
        double c = System.Math.Cos(radians);
        double s = System.Math.Sin(radians);
        return new(new Mat3(c, -s, 0, s, c, 0, 0, 0, 1), Vec3.Zero);
    }

    /// <summary>
    /// Creates a rotation around an arbitrary axis using the Rodrigues formula.
    /// </summary>
    /// <param name="axis">The rotation axis (will be normalized).</param>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>The rotation transform.</returns>
    public static Transform3 FromAxisAngle(Vec3 axis, double radians)
    {
        var n = axis.Normalized;
        double c = System.Math.Cos(radians);
        double s = System.Math.Sin(radians);
        double t = 1 - c;

        var rot = new Mat3(
            t * n.X * n.X + c, t * n.X * n.Y - s * n.Z, t * n.X * n.Z + s * n.Y,
            t * n.X * n.Y + s * n.Z, t * n.Y * n.Y + c, t * n.Y * n.Z - s * n.X,
            t * n.X * n.Z - s * n.Y, t * n.Y * n.Z + s * n.X, t * n.Z * n.Z + c);

        return new(rot, Vec3.Zero);
    }

    /// <summary>
    /// Creates a rotation that aligns the Z axis with the given forward direction
    /// and the Y axis as close to up as possible.
    /// </summary>
    /// <param name="forward">The desired forward (Z) direction.</param>
    /// <param name="up">The desired up hint (need not be orthogonal to forward).</param>
    /// <returns>The rotation transform.</returns>
    public static Transform3 LookRotation(Vec3 forward, Vec3 up)
    {
        var w = forward.Normalized;
        var u = Vec3.Cross(up, w).Normalized;
        var v = Vec3.Cross(w, u);
        return new(Mat3.FromColumns(u, v, w), Vec3.Zero);
    }

    /// <summary>
    /// Creates a rotation from Euler angles applied in X-Y-Z order (intrinsic).
    /// </summary>
    /// <param name="x">Rotation around X in radians.</param>
    /// <param name="y">Rotation around Y in radians.</param>
    /// <param name="z">Rotation around Z in radians.</param>
    /// <returns>The combined rotation transform.</returns>
    public static Transform3 FromEulerXYZ(double x, double y, double z) =>
        FromRotationX(x) * FromRotationY(y) * FromRotationZ(z);

    /// <summary>
    /// Creates a rotation from Euler angles applied in Z-Y-X order (intrinsic).
    /// </summary>
    /// <param name="z">Rotation around Z in radians.</param>
    /// <param name="y">Rotation around Y in radians.</param>
    /// <param name="x">Rotation around X in radians.</param>
    /// <returns>The combined rotation transform.</returns>
    public static Transform3 FromEulerZYX(double z, double y, double x) =>
        FromRotationZ(z) * FromRotationY(y) * FromRotationX(x);

    /// <summary>
    /// Extracts Euler angles in X-Y-Z order from this transform's rotation.
    /// Assumes the rotation is a proper rotation (orthonormal with det=+1).
    /// </summary>
    /// <returns>Euler angles (x, y, z) in radians.</returns>
    public (double X, double Y, double Z) ToEulerXYZ()
    {
        var r = Rotation;
        double sy = r.M02;
        double y = System.Math.Asin(MathUtil.Clamp(sy, -1, 1));

        double cosY = System.Math.Cos(y);
        double x, z;
        if (System.Math.Abs(cosY) > 1e-12)
        {
            x = System.Math.Atan2(-r.M12, r.M22);
            z = System.Math.Atan2(-r.M01, r.M00);
        }
        else
        {
            // Gimbal lock
            x = System.Math.Atan2(r.M21, r.M11);
            z = 0;
        }
        return (x, y, z);
    }

    /// <summary>
    /// Transforms a point: Rotation * point + Translation.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public Vec3 TransformPoint(Vec3 point) =>
        Rotation.Multiply(point) + Translation;

    /// <summary>
    /// Transforms a direction vector (rotation only, no translation).
    /// </summary>
    /// <param name="direction">The direction to transform.</param>
    /// <returns>The rotated direction.</returns>
    public Vec3 TransformDirection(Vec3 direction) =>
        Rotation.Multiply(direction);

    /// <summary>
    /// Returns the inverse of this transform. Assumes rotation is orthonormal.
    /// </summary>
    /// <returns>The inverse transform.</returns>
    public Transform3 Inverse()
    {
        var invRot = Rotation.Transposed;
        return new(invRot, invRot.Multiply(-Translation));
    }

    /// <summary>
    /// Composes two transforms: first applies <paramref name="a"/>, then <paramref name="b"/>.
    /// Equivalent to b * a in matrix notation.
    /// </summary>
    /// <param name="a">First transform to apply.</param>
    /// <param name="b">Second transform to apply.</param>
    /// <returns>The composed transform.</returns>
    public static Transform3 Compose(Transform3 a, Transform3 b) =>
        new(b.Rotation * a.Rotation, b.Rotation.Multiply(a.Translation) + b.Translation);

    /// <summary>
    /// Composition operator: applies <paramref name="right"/> first, then <paramref name="left"/>.
    /// </summary>
    public static Transform3 operator *(Transform3 left, Transform3 right) =>
        Compose(right, left);

    /// <summary>
    /// Transforms a point using the * operator.
    /// </summary>
    public static Vec3 operator *(Transform3 transform, Vec3 point) =>
        transform.TransformPoint(point);
}
