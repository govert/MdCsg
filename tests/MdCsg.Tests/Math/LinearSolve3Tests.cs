using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class LinearSolve3Tests
{
    // =========================================================================
    // Identity system: Ax=b where A=I → x=b
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(1, 2, 3)]
    [InlineData(-1, -2, -3)]
    [InlineData(100, 200, 300)]
    [InlineData(0.001, 0.002, 0.003)]
    public void Identity_SolvesExactly(double bx, double by, double bz)
    {
        var b = new Vec3(bx, by, bz);
        bool ok = LinearSolve3.Solve(Mat3.Identity, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, b) < 1e-10,
            $"Expected x={b}, got {x}");
    }

    // =========================================================================
    // Known 3x3 systems with exact solutions
    // =========================================================================

    [Fact]
    public void KnownSystem_ExactSolution()
    {
        // [1 2 3; 0 1 4; 5 6 0] * x = [1; 2; 3]
        // det = 1 (verified), solution: x=(-24, -5, 2) ... let's verify inline
        var a = new Mat3(1, 2, 3, 0, 1, 4, 5, 6, 0);
        var expected = new Vec3(6, -5, 2); // computed by hand: back-subst
        var b = a * expected; // produce the RHS from known answer
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-8,
            $"Expected {expected}, got {x}");
    }

    [Fact]
    public void SimpleScaling_SolvesCorrectly()
    {
        // diag(2, 3, 4) * x = (6, 9, 12) → x = (3, 3, 3)
        var a = new Mat3(2, 0, 0, 0, 3, 0, 0, 0, 4);
        var b = new Vec3(6, 9, 12);
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, new Vec3(3, 3, 3)) < 1e-10);
    }

    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0, 0, 0, 1)]  // I
    [InlineData(2, 0, 0, 0, 2, 0, 0, 0, 2)]  // 2I
    [InlineData(1, 1, 0, 0, 1, 1, 0, 0, 1)]  // upper triangular
    [InlineData(1, 0, 0, 1, 1, 0, 1, 1, 1)]  // lower triangular
    public void VariousNonSingular_SolvesRoundtrip(
        double a00, double a01, double a02,
        double a10, double a11, double a12,
        double a20, double a21, double a22)
    {
        var a = new Mat3(a00, a01, a02, a10, a11, a12, a20, a21, a22);
        var expected = new Vec3(1, 2, 3);
        var b = a * expected;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok, "System should be solvable");
        Assert.True(Vec3.Distance(x, expected) < 1e-8,
            $"Expected {expected}, got {x}");
    }

    // =========================================================================
    // Symmetric positive-definite systems
    // =========================================================================

    [Fact]
    public void SymmetricPositiveDefinite_SolvesCorrectly()
    {
        // SPD: [4 2 1; 2 5 3; 1 3 6]
        var a = Mat3.Symmetric(4, 2, 1, 5, 3, 6);
        var expected = new Vec3(1, -1, 2);
        var b = a * expected;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-8);
    }

    [Theory]
    [InlineData(1, 0, 0, 1, 0, 1)]
    [InlineData(10, 1, 1, 10, 1, 10)]
    [InlineData(100, 0, 0, 100, 0, 100)]
    public void SymmetricDiagonalDominant_Solves(
        double a00, double a01, double a02,
        double a11, double a12, double a22)
    {
        var a = Mat3.Symmetric(a00, a01, a02, a11, a12, a22);
        var expected = new Vec3(1, 2, 3);
        var b = a * expected;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-6);
    }

    // =========================================================================
    // Singular systems → returns false
    // =========================================================================

    [Fact]
    public void ZeroMatrix_ReturnsFalse()
    {
        bool ok = LinearSolve3.Solve(Mat3.Zero, new Vec3(1, 2, 3), out _);
        Assert.False(ok);
    }

    [Fact]
    public void RowsAreIdentical_ReturnsFalse()
    {
        var a = new Mat3(1, 2, 3, 1, 2, 3, 1, 2, 3);
        bool ok = LinearSolve3.Solve(a, new Vec3(1, 2, 3), out _);
        Assert.False(ok);
    }

    [Fact]
    public void ThirdRowIsSum_ReturnsFalse()
    {
        // row2 = row0 + row1 → singular
        var a = new Mat3(1, 0, 0, 0, 1, 0, 1, 1, 0);
        bool ok = LinearSolve3.Solve(a, new Vec3(1, 2, 3), out _);
        Assert.False(ok);
    }

    [Fact]
    public void TwoColumnsIdentical_ReturnsFalse()
    {
        var a = new Mat3(1, 1, 0, 2, 2, 0, 3, 3, 0);
        bool ok = LinearSolve3.Solve(a, Vec3.UnitX, out _);
        Assert.False(ok);
    }

    // =========================================================================
    // Zero RHS → zero solution
    // =========================================================================

    [Fact]
    public void ZeroRhs_ZeroSolution()
    {
        var a = new Mat3(1, 2, 3, 4, 5, 6, 7, 8, 10);
        bool ok = LinearSolve3.Solve(a, Vec3.Zero, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, Vec3.Zero) < 1e-10);
    }

    // =========================================================================
    // Large coefficient magnitudes
    // =========================================================================

    [Theory]
    [InlineData(1e6)]
    [InlineData(1e10)]
    [InlineData(1e-6)]
    [InlineData(1e-3)]
    public void ScaledIdentity_SolvesCorrectly(double scale)
    {
        var a = Mat3.Identity * scale;
        var expected = new Vec3(1, 2, 3);
        var b = expected * scale;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-4,
            $"scale={scale}: expected {expected}, got {x}");
    }

    // =========================================================================
    // Permutation matrices
    // =========================================================================

    [Fact]
    public void PermutationXYZ_to_YZX()
    {
        // [0 1 0; 0 0 1; 1 0 0] maps (x,y,z) to (y,z,x)
        var a = new Mat3(0, 1, 0, 0, 0, 1, 1, 0, 0);
        var expected = new Vec3(3, 1, 2);
        var b = a * expected; // (1, 2, 3)
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-10);
    }

    [Fact]
    public void PermutationXYZ_to_ZXY()
    {
        var a = new Mat3(0, 0, 1, 1, 0, 0, 0, 1, 0);
        var expected = new Vec3(2, 3, 1);
        var b = a * expected;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-10);
    }

    // =========================================================================
    // Diagonal matrices with varying scales
    // =========================================================================

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(0.01, 1, 100)]
    [InlineData(1e-4, 1, 1e4)]
    [InlineData(3.14, 2.72, 1.41)]
    public void Diagonal_VariousScales(double d0, double d1, double d2)
    {
        var a = new Mat3(d0, 0, 0, 0, d1, 0, 0, 0, d2);
        var expected = new Vec3(1, 2, 3);
        var b = new Vec3(d0 * 1, d1 * 2, d2 * 3);
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-6,
            $"diag({d0},{d1},{d2}): expected {expected}, got {x}");
    }

    // =========================================================================
    // Negative coefficients
    // =========================================================================

    [Fact]
    public void NegativeIdentity_Solves()
    {
        var a = Mat3.Identity * -1.0;
        var b = new Vec3(-1, -2, -3);
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, new Vec3(1, 2, 3)) < 1e-10);
    }

    [Fact]
    public void MixedSigns_SolvesCorrectly()
    {
        var a = new Mat3(-1, 2, -3, 4, -5, 6, -7, 8, -9);
        // This matrix might be singular; use a non-singular mixed-sign one
        var a2 = new Mat3(-1, 2, -3, 4, -5, 6, -7, 8, -10);
        var expected = new Vec3(1, -1, 2);
        var b = a2 * expected;
        bool ok = LinearSolve3.Solve(a2, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-8);
    }

    // =========================================================================
    // Rotation-like matrices
    // =========================================================================

    [Fact]
    public void RotationMatrix_SolvesCorrectly()
    {
        // 90 degree rotation about Z
        var a = new Mat3(0, -1, 0, 1, 0, 0, 0, 0, 1);
        var expected = new Vec3(1, 2, 3);
        var b = a * expected;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-10);
    }

    [Fact]
    public void Rotation45Z_SolvesCorrectly()
    {
        double c = System.Math.Cos(System.Math.PI / 4);
        double s = System.Math.Sin(System.Math.PI / 4);
        var a = new Mat3(c, -s, 0, s, c, 0, 0, 0, 1);
        var expected = new Vec3(1, 0, 5);
        var b = a * expected;
        bool ok = LinearSolve3.Solve(a, b, out var x);
        Assert.True(ok);
        Assert.True(Vec3.Distance(x, expected) < 1e-10);
    }
}
