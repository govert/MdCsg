using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Mat3Tests
{
    // =========================================================================
    // Identity / Zero
    // =========================================================================

    [Fact]
    public void Identity_DiagonalIsOne()
    {
        var m = Mat3.Identity;
        Assert.Equal(1.0, m.M00);
        Assert.Equal(1.0, m.M11);
        Assert.Equal(1.0, m.M22);
        Assert.Equal(0.0, m.M01);
        Assert.Equal(0.0, m.M12);
    }

    [Fact]
    public void Zero_AllElementsZero()
    {
        var m = Mat3.Zero;
        Assert.Equal(0.0, m.M00);
        Assert.Equal(0.0, m.M11);
        Assert.Equal(0.0, m.M22);
    }

    // =========================================================================
    // Determinant
    // =========================================================================

    [Fact]
    public void Identity_DeterminantIsOne()
    {
        Assert.Equal(1.0, Mat3.Identity.Determinant, 5);
    }

    [Fact]
    public void Zero_DeterminantIsZero()
    {
        Assert.Equal(0.0, Mat3.Zero.Determinant, 5);
    }

    [Fact]
    public void KnownMatrix_CorrectDeterminant()
    {
        var m = new Mat3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        Assert.Equal(1.0, m.Determinant, 10);
    }

    [Fact]
    public void Diagonal_DeterminantIsProduct()
    {
        var m = new Mat3(2, 0, 0, 0, 3, 0, 0, 0, 5);
        Assert.Equal(30.0, m.Determinant, 10);
    }

    [Fact]
    public void DeterminantOfProduct_IsProductOfDeterminants()
    {
        var a = new Mat3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        var b = new Mat3(2, 0, 1, 1, 3, 0, 0, 1, 2);

        double detAB = (a * b).Determinant;
        double detA_detB = a.Determinant * b.Determinant;

        Assert.True(System.Math.Abs(detAB - detA_detB) < 1e-8,
            $"det(AB)={detAB}, det(A)*det(B)={detA_detB}");
    }

    [Fact]
    public void NegativeDeterminant_Matrix()
    {
        // A permutation matrix with odd parity has det = -1
        var m = new Mat3(0, 1, 0, 1, 0, 0, 0, 0, 1); // swap rows 0,1
        Assert.True(System.Math.Abs(m.Determinant - (-1.0)) < 1e-10);
    }

    // =========================================================================
    // Transpose
    // =========================================================================

    [Fact]
    public void Transpose_SwapsRowsAndColumns()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var t = m.Transposed;
        Assert.Equal(1.0, t.M00);
        Assert.Equal(4.0, t.M01);
        Assert.Equal(7.0, t.M02);
        Assert.Equal(2.0, t.M10);
        Assert.Equal(5.0, t.M11);
        Assert.Equal(8.0, t.M12);
    }

    [Fact]
    public void Transpose_Symmetric_IsSelf()
    {
        var m = Mat3.Symmetric(1, 2, 3, 4, 5, 6);
        var t = m.Transposed;
        Assert.Equal(m, t);
    }

    [Fact]
    public void Transpose_DoubleTranspose_IsOriginal()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        Assert.Equal(m, m.Transposed.Transposed);
    }

    [Fact]
    public void Transpose_OfProduct_IsReverseProduct()
    {
        var a = new Mat3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        var b = new Mat3(2, 0, 1, 1, 3, 0, 0, 1, 2);

        var abT = (a * b).Transposed;
        var bTaT = b.Transposed * a.Transposed;

        // Check element-wise
        Assert.True(System.Math.Abs(abT.M00 - bTaT.M00) < 1e-10);
        Assert.True(System.Math.Abs(abT.M01 - bTaT.M01) < 1e-10);
        Assert.True(System.Math.Abs(abT.M02 - bTaT.M02) < 1e-10);
        Assert.True(System.Math.Abs(abT.M10 - bTaT.M10) < 1e-10);
        Assert.True(System.Math.Abs(abT.M11 - bTaT.M11) < 1e-10);
        Assert.True(System.Math.Abs(abT.M12 - bTaT.M12) < 1e-10);
        Assert.True(System.Math.Abs(abT.M20 - bTaT.M20) < 1e-10);
        Assert.True(System.Math.Abs(abT.M21 - bTaT.M21) < 1e-10);
        Assert.True(System.Math.Abs(abT.M22 - bTaT.M22) < 1e-10);
    }

    // =========================================================================
    // Trace
    // =========================================================================

    [Fact]
    public void Identity_TraceIsThree()
    {
        Assert.Equal(3.0, Mat3.Identity.Trace, 5);
    }

    [Fact]
    public void Zero_TraceIsZero()
    {
        Assert.Equal(0.0, Mat3.Zero.Trace, 5);
    }

    [Fact]
    public void Diagonal_TraceIsSumOfDiagonal()
    {
        var m = new Mat3(2, 0, 0, 0, 5, 0, 0, 0, 7);
        Assert.Equal(14.0, m.Trace, 10);
    }

    // =========================================================================
    // Multiply: matrix * vector
    // =========================================================================

    [Fact]
    public void Multiply_Identity_ReturnsSameVector()
    {
        var v = new Vec3(1, 2, 3);
        var result = Mat3.Identity * v;
        Assert.Equal(v, result);
    }

    [Fact]
    public void Multiply_Scale_ScalesVector()
    {
        var scale = new Mat3(2, 0, 0, 0, 3, 0, 0, 0, 4);
        var v = new Vec3(1, 1, 1);
        var result = scale * v;
        Assert.Equal(new Vec3(2, 3, 4), result);
    }

    [Fact]
    public void Multiply_Zero_ReturnsZero()
    {
        var v = new Vec3(1, 2, 3);
        var result = Mat3.Zero * v;
        Assert.Equal(Vec3.Zero, result);
    }

    [Fact]
    public void MatrixMultiply_KnownResult()
    {
        var a = new Mat3(1, 0, 0, 0, 0, -1, 0, 1, 0); // 90-degree rotation about X
        var v = new Vec3(0, 1, 0);
        var result = a * v;
        Assert.True(System.Math.Abs(result.X) < 1e-10);
        Assert.True(System.Math.Abs(result.Y) < 1e-10);
        Assert.True(System.Math.Abs(result.Z - 1.0) < 1e-10);
    }

    // =========================================================================
    // Multiply: matrix * matrix
    // =========================================================================

    [Fact]
    public void MatrixMultiply_IdentityTimesAny_ReturnsSame()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = Mat3.Identity * m;
        Assert.Equal(m, result);
    }

    [Fact]
    public void MatrixMultiply_AnyTimesIdentity_ReturnsSame()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = m * Mat3.Identity;
        Assert.Equal(m, result);
    }

    [Fact]
    public void MatrixMultiply_Associative()
    {
        var a = new Mat3(1, 2, 0, 0, 1, 3, 1, 0, 1);
        var b = new Mat3(2, 0, 1, 1, 3, 0, 0, 1, 2);
        var c = new Mat3(0, 1, 2, 1, 0, 1, 2, 1, 0);

        var ab_c = (a * b) * c;
        var a_bc = a * (b * c);

        for (int i = 0; i < 3; i++)
        {
            var r1 = ab_c.Row(i);
            var r2 = a_bc.Row(i);
            Assert.True(Vec3.Distance(r1, r2) < 1e-8,
                $"Row {i}: (AB)C != A(BC)");
        }
    }

    // =========================================================================
    // Arithmetic
    // =========================================================================

    [Fact]
    public void Addition_ElementWise()
    {
        var a = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = a + Mat3.Identity;
        Assert.Equal(2.0, result.M00);
        Assert.Equal(2.0, result.M01);
        Assert.Equal(6.0, result.M11);
    }

    [Fact]
    public void Addition_Commutative()
    {
        var a = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var b = new Mat3(9, 8, 7, 6, 5, 4, 3, 2, 1);
        var ab = a + b;
        var ba = b + a;
        Assert.Equal(ab, ba);
    }

    [Fact]
    public void Subtraction_ElementWise()
    {
        var result = Mat3.Identity - Mat3.Identity;
        Assert.Equal(Mat3.Zero, result);
    }

    [Fact]
    public void ScalarMultiply_Doubles()
    {
        var result = Mat3.Identity * 2.0;
        Assert.Equal(2.0, result.M00);
        Assert.Equal(0.0, result.M01);
        Assert.Equal(2.0, result.M22);
    }

    [Fact]
    public void ScalarMultiply_Zero_GivesZero()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var result = m * 0.0;
        Assert.Equal(Mat3.Zero, result);
    }

    [Fact]
    public void ScalarMultiply_Negative()
    {
        var m = Mat3.Identity;
        var result = m * -1.0;
        Assert.Equal(-1.0, result.M00);
        Assert.Equal(-1.0, result.M11);
        Assert.Equal(0.0, result.M01);
    }

    // =========================================================================
    // Row / Column accessors
    // =========================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Row_AllIndices(int row)
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var r = m.Row(row);
        double expected0 = 1 + row * 3;
        Assert.Equal(expected0, r.X);
        Assert.Equal(expected0 + 1, r.Y);
        Assert.Equal(expected0 + 2, r.Z);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Column_AllIndices(int col)
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var c = m.Column(col);
        Assert.Equal(1.0 + col, c.X);
        Assert.Equal(4.0 + col, c.Y);
        Assert.Equal(7.0 + col, c.Z);
    }

    [Fact]
    public void Row_ReturnsCorrectVector()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        Assert.Equal(new Vec3(4, 5, 6), m.Row(1));
    }

    [Fact]
    public void Column_ReturnsCorrectVector()
    {
        var m = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 9);
        Assert.Equal(new Vec3(2, 5, 8), m.Column(1));
    }

    // =========================================================================
    // FromRows / FromColumns
    // =========================================================================

    [Fact]
    public void FromRows_RoundTrips()
    {
        var r0 = new Vec3(1, 2, 3);
        var r1 = new Vec3(4, 5, 6);
        var r2 = new Vec3(7, 8, 9);
        var m = Mat3.FromRows(r0, r1, r2);
        Assert.Equal(r0, m.Row(0));
        Assert.Equal(r1, m.Row(1));
        Assert.Equal(r2, m.Row(2));
    }

    [Fact]
    public void FromColumns_RoundTrips()
    {
        var c0 = new Vec3(1, 4, 7);
        var c1 = new Vec3(2, 5, 8);
        var c2 = new Vec3(3, 6, 9);
        var m = Mat3.FromColumns(c0, c1, c2);
        Assert.Equal(c0, m.Column(0));
        Assert.Equal(c1, m.Column(1));
        Assert.Equal(c2, m.Column(2));
    }

    [Fact]
    public void FromRows_FromColumns_AreTransposeOfEachOther()
    {
        var v0 = new Vec3(1, 2, 3);
        var v1 = new Vec3(4, 5, 6);
        var v2 = new Vec3(7, 8, 9);

        var fromRows = Mat3.FromRows(v0, v1, v2);
        var fromCols = Mat3.FromColumns(v0, v1, v2);

        Assert.Equal(fromRows, fromCols.Transposed);
    }

    // =========================================================================
    // Symmetric factory
    // =========================================================================

    [Fact]
    public void Symmetric_IsSymmetric()
    {
        var m = Mat3.Symmetric(1, 2, 3, 4, 5, 6);
        Assert.Equal(m.M01, m.M10);
        Assert.Equal(m.M02, m.M20);
        Assert.Equal(m.M12, m.M21);
    }

    [Fact]
    public void Symmetric_DiagonalCorrect()
    {
        var m = Mat3.Symmetric(1, 2, 3, 4, 5, 6);
        Assert.Equal(1.0, m.M00);
        Assert.Equal(4.0, m.M11);
        Assert.Equal(6.0, m.M22);
    }

    [Fact]
    public void Symmetric_OffDiagonalCorrect()
    {
        var m = Mat3.Symmetric(1, 2, 3, 4, 5, 6);
        Assert.Equal(2.0, m.M01);
        Assert.Equal(3.0, m.M02);
        Assert.Equal(5.0, m.M12);
    }

    // =========================================================================
    // Distributivity: A*(B+C) = A*B + A*C
    // =========================================================================

    [Fact]
    public void Distributivity_MatrixMultiplyOverAdd()
    {
        var a = new Mat3(1, 2, 0, 0, 1, 3, 1, 0, 1);
        var b = new Mat3(2, 0, 1, 1, 3, 0, 0, 1, 2);
        var c = new Mat3(0, 1, 2, 1, 0, 1, 2, 1, 0);

        var lhs = a * (b + c);
        var rhs = a * b + a * c;

        for (int i = 0; i < 3; i++)
        {
            var r1 = lhs.Row(i);
            var r2 = rhs.Row(i);
            Assert.True(Vec3.Distance(r1, r2) < 1e-8,
                $"Row {i}: A(B+C) != AB+AC");
        }
    }

    // =========================================================================
    // Large/small values (numerical stability)
    // =========================================================================

    [Theory]
    [InlineData(1e10)]
    [InlineData(1e-10)]
    public void ScaledMatrix_DeterminantScalesCubed(double scale)
    {
        var m = new Mat3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        var scaled = m * scale;
        double expectedDet = m.Determinant * scale * scale * scale;
        Assert.True(System.Math.Abs(scaled.Determinant - expectedDet) < System.Math.Abs(expectedDet) * 1e-6,
            $"scale={scale}: det={scaled.Determinant}, expected={expectedDet}");
    }
}
