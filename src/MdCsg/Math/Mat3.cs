namespace MdCsg.Math;

/// <summary>
/// Immutable 3x3 matrix with double-precision components, stored in row-major order.
/// </summary>
/// <param name="M00">Row 0, column 0.</param>
/// <param name="M01">Row 0, column 1.</param>
/// <param name="M02">Row 0, column 2.</param>
/// <param name="M10">Row 1, column 0.</param>
/// <param name="M11">Row 1, column 1.</param>
/// <param name="M12">Row 1, column 2.</param>
/// <param name="M20">Row 2, column 0.</param>
/// <param name="M21">Row 2, column 1.</param>
/// <param name="M22">Row 2, column 2.</param>
public readonly record struct Mat3(
    double M00, double M01, double M02,
    double M10, double M11, double M12,
    double M20, double M21, double M22)
{
    /// <summary>The 3x3 identity matrix.</summary>
    public static readonly Mat3 Identity = new(1, 0, 0, 0, 1, 0, 0, 0, 1);

    /// <summary>The 3x3 zero matrix.</summary>
    public static readonly Mat3 Zero = new(0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Creates a symmetric matrix from six unique elements.
    /// </summary>
    /// <param name="m00">Diagonal element (0,0).</param>
    /// <param name="m01">Off-diagonal element (0,1) = (1,0).</param>
    /// <param name="m02">Off-diagonal element (0,2) = (2,0).</param>
    /// <param name="m11">Diagonal element (1,1).</param>
    /// <param name="m12">Off-diagonal element (1,2) = (2,1).</param>
    /// <param name="m22">Diagonal element (2,2).</param>
    /// <returns>A symmetric 3x3 matrix.</returns>
    public static Mat3 Symmetric(double m00, double m01, double m02,
        double m11, double m12, double m22)
        => new(m00, m01, m02, m01, m11, m12, m02, m12, m22);

    /// <summary>
    /// Creates a matrix from three row vectors.
    /// </summary>
    /// <param name="row0">The first row.</param>
    /// <param name="row1">The second row.</param>
    /// <param name="row2">The third row.</param>
    /// <returns>A matrix whose rows are the given vectors.</returns>
    public static Mat3 FromRows(Vec3 row0, Vec3 row1, Vec3 row2)
        => new(row0.X, row0.Y, row0.Z, row1.X, row1.Y, row1.Z, row2.X, row2.Y, row2.Z);

    /// <summary>
    /// Creates a matrix from three column vectors.
    /// </summary>
    /// <param name="col0">The first column.</param>
    /// <param name="col1">The second column.</param>
    /// <param name="col2">The third column.</param>
    /// <returns>A matrix whose columns are the given vectors.</returns>
    public static Mat3 FromColumns(Vec3 col0, Vec3 col1, Vec3 col2)
        => new(col0.X, col1.X, col2.X, col0.Y, col1.Y, col2.Y, col0.Z, col1.Z, col2.Z);

    /// <summary>Returns the transpose of this matrix.</summary>
    public Mat3 Transposed => new(M00, M10, M20, M01, M11, M21, M02, M12, M22);

    /// <summary>Returns the determinant of this matrix.</summary>
    public double Determinant =>
        M00 * (M11 * M22 - M12 * M21) -
        M01 * (M10 * M22 - M12 * M20) +
        M02 * (M10 * M21 - M11 * M20);

    /// <summary>Returns the trace (sum of diagonal elements).</summary>
    public double Trace => M00 + M11 + M22;

    /// <summary>Returns row <paramref name="i"/> as a <see cref="Vec3"/>.</summary>
    /// <param name="i">Row index (0, 1, or 2).</param>
    /// <returns>The row vector.</returns>
    public Vec3 Row(int i) => i switch
    {
        0 => new Vec3(M00, M01, M02),
        1 => new Vec3(M10, M11, M12),
        2 => new Vec3(M20, M21, M22),
        _ => throw new ArgumentOutOfRangeException(nameof(i))
    };

    /// <summary>Returns column <paramref name="j"/> as a <see cref="Vec3"/>.</summary>
    /// <param name="j">Column index (0, 1, or 2).</param>
    /// <returns>The column vector.</returns>
    public Vec3 Column(int j) => j switch
    {
        0 => new Vec3(M00, M10, M20),
        1 => new Vec3(M01, M11, M21),
        2 => new Vec3(M02, M12, M22),
        _ => throw new ArgumentOutOfRangeException(nameof(j))
    };

    /// <summary>Multiplies this matrix by a vector (M * v).</summary>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The transformed vector.</returns>
    public Vec3 Multiply(Vec3 v) => new(
        M00 * v.X + M01 * v.Y + M02 * v.Z,
        M10 * v.X + M11 * v.Y + M12 * v.Z,
        M20 * v.X + M21 * v.Y + M22 * v.Z);

    /// <summary>Matrix-vector multiplication.</summary>
    public static Vec3 operator *(Mat3 m, Vec3 v) => m.Multiply(v);

    /// <summary>Matrix-matrix multiplication.</summary>
    public static Mat3 operator *(Mat3 a, Mat3 b) => new(
        a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20,
        a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21,
        a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22,
        a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20,
        a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21,
        a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22,
        a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20,
        a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21,
        a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22);

    /// <summary>Matrix addition.</summary>
    public static Mat3 operator +(Mat3 a, Mat3 b) => new(
        a.M00 + b.M00, a.M01 + b.M01, a.M02 + b.M02,
        a.M10 + b.M10, a.M11 + b.M11, a.M12 + b.M12,
        a.M20 + b.M20, a.M21 + b.M21, a.M22 + b.M22);

    /// <summary>Matrix subtraction.</summary>
    public static Mat3 operator -(Mat3 a, Mat3 b) => new(
        a.M00 - b.M00, a.M01 - b.M01, a.M02 - b.M02,
        a.M10 - b.M10, a.M11 - b.M11, a.M12 - b.M12,
        a.M20 - b.M20, a.M21 - b.M21, a.M22 - b.M22);

    /// <summary>Scalar multiplication.</summary>
    public static Mat3 operator *(Mat3 m, double s) => new(
        m.M00 * s, m.M01 * s, m.M02 * s,
        m.M10 * s, m.M11 * s, m.M12 * s,
        m.M20 * s, m.M21 * s, m.M22 * s);

    /// <summary>Scalar multiplication (scalar on the left).</summary>
    public static Mat3 operator *(double s, Mat3 m) => m * s;
}
