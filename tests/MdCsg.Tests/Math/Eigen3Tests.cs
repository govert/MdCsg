using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Eigen3Tests
{
    // =========================================================================
    // Identity matrix
    // =========================================================================

    [Fact]
    public void Identity_AllEigenvaluesAreOne()
    {
        var (eigenvalues, _) = Eigen3.Decompose(Mat3.Identity);
        Assert.True(System.Math.Abs(eigenvalues.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void Identity_EigenvectorsAreOrthonormal()
    {
        var (_, V) = Eigen3.Decompose(Mat3.Identity);
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Diagonal matrices (various orderings)
    // =========================================================================

    [Fact]
    public void Diagonal_EigenvaluesMatchDiagonal()
    {
        var m = new Mat3(3, 0, 0, 0, 1, 0, 0, 0, 2);
        var (eigenvalues, _) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Y - 2.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - 3.0) < 1e-10);
    }

    [Fact]
    public void Diagonal_EigenvectorsAreAxisAligned()
    {
        var m = new Mat3(3, 0, 0, 0, 1, 0, 0, 0, 2);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        var v0 = V.Column(0);
        var v1 = V.Column(1);
        var v2 = V.Column(2);

        Assert.True(System.Math.Abs(System.Math.Abs(Vec3.Dot(v0, Vec3.UnitY)) - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(System.Math.Abs(Vec3.Dot(v1, Vec3.UnitZ)) - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(System.Math.Abs(Vec3.Dot(v2, Vec3.UnitX)) - 1.0) < 1e-10);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 1, 3)]
    [InlineData(10, 20, 30)]
    [InlineData(0.1, 0.2, 0.3)]
    public void Diagonal_VariousValues_SortedAscending(double d0, double d1, double d2)
    {
        var m = new Mat3(d0, 0, 0, 0, d1, 0, 0, 0, d2);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        var sorted = new[] { d0, d1, d2 };
        System.Array.Sort(sorted);

        Assert.True(System.Math.Abs(eigenvalues.X - sorted[0]) < 1e-8,
            $"λ0: expected {sorted[0]}, got {eigenvalues.X}");
        Assert.True(System.Math.Abs(eigenvalues.Y - sorted[1]) < 1e-8,
            $"λ1: expected {sorted[1]}, got {eigenvalues.Y}");
        Assert.True(System.Math.Abs(eigenvalues.Z - sorted[2]) < 1e-8,
            $"λ2: expected {sorted[2]}, got {eigenvalues.Z}");
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Known symmetric matrix
    // =========================================================================

    [Fact]
    public void KnownSymmetric_CorrectEigenvalues()
    {
        var m = Mat3.Symmetric(2, 1, 0, 3, 1, 2);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - 1.0) < 1e-8,
            $"Expected eigenvalue ~1.0, got {eigenvalues.X}");
        Assert.True(System.Math.Abs(eigenvalues.Y - 2.0) < 1e-8,
            $"Expected eigenvalue ~2.0, got {eigenvalues.Y}");
        Assert.True(System.Math.Abs(eigenvalues.Z - 4.0) < 1e-8,
            $"Expected eigenvalue ~4.0, got {eigenvalues.Z}");

        AssertOrthonormal(V);
    }

    [Fact]
    public void KnownSymmetric_EigenvectorsSatisfyEquation()
    {
        var m = Mat3.Symmetric(2, 1, 0, 3, 1, 2);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        for (int i = 0; i < 3; i++)
        {
            double lambda = eigenvalues[i];
            var v = V.Column(i);
            var mv = m * v;
            var lv = v * lambda;
            Assert.True(Vec3.Distance(mv, lv) < 1e-8,
                $"Eigenvector {i}: M*v != lambda*v (distance={Vec3.Distance(mv, lv)})");
        }
    }

    // =========================================================================
    // Repeated eigenvalues
    // =========================================================================

    [Fact]
    public void TripleRepeated_AllEigenvaluesEqual()
    {
        var m = Mat3.Identity * 2.0;
        var (eigenvalues, V) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - 2.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Y - 2.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - 2.0) < 1e-10);
        AssertOrthonormal(V);
    }

    [Fact]
    public void TwoRepeatedEigenvalues_Converges()
    {
        var m = new Mat3(5, 0, 0, 0, 5, 0, 0, 0, 1);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Y - 5.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - 5.0) < 1e-10);
        AssertOrthonormal(V);
    }

    [Theory]
    [InlineData(1, 1, 5)]
    [InlineData(3, 3, 7)]
    [InlineData(10, 10, 1)]
    public void TwoRepeated_Various(double a, double b, double c)
    {
        var m = new Mat3(a, 0, 0, 0, b, 0, 0, 0, c);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        var sorted = new[] { a, b, c };
        System.Array.Sort(sorted);

        Assert.True(System.Math.Abs(eigenvalues.X - sorted[0]) < 1e-8);
        Assert.True(System.Math.Abs(eigenvalues.Y - sorted[1]) < 1e-8);
        Assert.True(System.Math.Abs(eigenvalues.Z - sorted[2]) < 1e-8);
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Nearly repeated eigenvalues
    // =========================================================================

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(1e-8)]
    public void NearlyRepeated_StillConverges(double delta)
    {
        var m = new Mat3(1, 0, 0, 0, 1 + delta, 0, 0, 0, 2);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - 1.0) < delta + 1e-8);
        Assert.True(System.Math.Abs(eigenvalues.Y - (1.0 + delta)) < delta + 1e-8);
        Assert.True(System.Math.Abs(eigenvalues.Z - 2.0) < 1e-8);
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Large eigenvalue ratios
    // =========================================================================

    [Theory]
    [InlineData(1, 100)]
    [InlineData(1, 10000)]
    [InlineData(0.001, 1000)]
    public void LargeEigenvalueRatio(double small, double large)
    {
        var m = new Mat3(small, 0, 0, 0, 1, 0, 0, 0, large);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        var sorted = new[] { small, 1.0, large };
        System.Array.Sort(sorted);

        Assert.True(System.Math.Abs(eigenvalues.X - sorted[0]) < sorted[0] * 1e-6 + 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - sorted[2]) < sorted[2] * 1e-6 + 1e-10);
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Negative eigenvalues
    // =========================================================================

    [Fact]
    public void NegativeEigenvalues_Diagonal()
    {
        var m = new Mat3(-3, 0, 0, 0, -1, 0, 0, 0, -2);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - (-3.0)) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Y - (-2.0)) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - (-1.0)) < 1e-10);
        AssertOrthonormal(V);
    }

    [Fact]
    public void MixedSignEigenvalues()
    {
        var m = new Mat3(-2, 0, 0, 0, 1, 0, 0, 0, 3);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        Assert.True(System.Math.Abs(eigenvalues.X - (-2.0)) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(eigenvalues.Z - 3.0) < 1e-10);
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Eigenvector orthogonality verification
    // =========================================================================

    [Theory]
    [InlineData(2, 1, 0, 3, 1, 2)]
    [InlineData(5, 2, 1, 4, 0, 3)]
    [InlineData(10, 1, 1, 10, 1, 10)]
    [InlineData(1, 0, 0, 1, 0, 1)]
    public void VariousSymmetric_EigenvectorsOrthonormal(
        double a00, double a01, double a02, double a11, double a12, double a22)
    {
        var m = Mat3.Symmetric(a00, a01, a02, a11, a12, a22);
        var (_, V) = Eigen3.Decompose(m);
        AssertOrthonormal(V);
    }

    // =========================================================================
    // Reconstruction: V * diag(λ) * V^T ≈ A
    // =========================================================================

    [Theory]
    [InlineData(2, 1, 0, 3, 1, 2)]
    [InlineData(5, 2, 1, 4, 0, 3)]
    [InlineData(10, 0, 0, 10, 0, 10)]
    public void Reconstruction_VDiagVT_EqualsOriginal(
        double a00, double a01, double a02, double a11, double a12, double a22)
    {
        var m = Mat3.Symmetric(a00, a01, a02, a11, a12, a22);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        // Reconstruct: sum over i: lambda_i * v_i * v_i^T
        var reconstructed = Mat3.Zero;
        for (int i = 0; i < 3; i++)
        {
            double lambda = eigenvalues[i];
            var vi = V.Column(i);
            // Outer product: vi * vi^T
            var outer = new Mat3(
                vi.X * vi.X, vi.X * vi.Y, vi.X * vi.Z,
                vi.Y * vi.X, vi.Y * vi.Y, vi.Y * vi.Z,
                vi.Z * vi.X, vi.Z * vi.Y, vi.Z * vi.Z);
            reconstructed = reconstructed + outer * lambda;
        }

        // Compare element by element
        Assert.True(System.Math.Abs(reconstructed.M00 - m.M00) < 1e-6, $"M00: {reconstructed.M00} vs {m.M00}");
        Assert.True(System.Math.Abs(reconstructed.M01 - m.M01) < 1e-6, $"M01");
        Assert.True(System.Math.Abs(reconstructed.M02 - m.M02) < 1e-6, $"M02");
        Assert.True(System.Math.Abs(reconstructed.M11 - m.M11) < 1e-6, $"M11");
        Assert.True(System.Math.Abs(reconstructed.M12 - m.M12) < 1e-6, $"M12");
        Assert.True(System.Math.Abs(reconstructed.M22 - m.M22) < 1e-6, $"M22");
    }

    // =========================================================================
    // Eigenvalues sorted ascending
    // =========================================================================

    [Fact]
    public void EigenvaluesAreSortedAscending()
    {
        var m = Mat3.Symmetric(10, 1, 2, 5, 0.5, 3);
        var (eigenvalues, _) = Eigen3.Decompose(m);

        Assert.True(eigenvalues.X <= eigenvalues.Y,
            $"Eigenvalues not ascending: {eigenvalues.X} > {eigenvalues.Y}");
        Assert.True(eigenvalues.Y <= eigenvalues.Z,
            $"Eigenvalues not ascending: {eigenvalues.Y} > {eigenvalues.Z}");
    }

    [Theory]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(100, 0, 0, 1, 0, 0.01)]
    [InlineData(5, -1, 2, 3, 1, 7)]
    public void VariousMatrices_EigenvaluesSortedAscending(
        double a00, double a01, double a02, double a11, double a12, double a22)
    {
        var m = Mat3.Symmetric(a00, a01, a02, a11, a12, a22);
        var (eigenvalues, _) = Eigen3.Decompose(m);

        Assert.True(eigenvalues.X <= eigenvalues.Y + 1e-10,
            $"λ0={eigenvalues.X} > λ1={eigenvalues.Y}");
        Assert.True(eigenvalues.Y <= eigenvalues.Z + 1e-10,
            $"λ1={eigenvalues.Y} > λ2={eigenvalues.Z}");
    }

    // =========================================================================
    // Stress: M*v = λ*v for all eigenpairs
    // =========================================================================

    [Theory]
    [InlineData(3, 1, 0, 2, 1, 4)]
    [InlineData(7, 2, 3, 5, 1, 8)]
    [InlineData(1, 0.5, 0.3, 2, 0.1, 1.5)]
    [InlineData(100, 10, 5, 50, 3, 30)]
    public void EigenpairsSatisfyEquation(
        double a00, double a01, double a02, double a11, double a12, double a22)
    {
        var m = Mat3.Symmetric(a00, a01, a02, a11, a12, a22);
        var (eigenvalues, V) = Eigen3.Decompose(m);

        for (int i = 0; i < 3; i++)
        {
            double lambda = eigenvalues[i];
            var v = V.Column(i);
            var mv = m * v;
            var lv = v * lambda;
            Assert.True(Vec3.Distance(mv, lv) < 1e-6,
                $"({a00},{a01},{a02},{a11},{a12},{a22}): eigenpair {i}, dist={Vec3.Distance(mv, lv)}");
        }
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static void AssertOrthonormal(Mat3 V)
    {
        var c0 = V.Column(0);
        var c1 = V.Column(1);
        var c2 = V.Column(2);

        Assert.True(System.Math.Abs(c0.Length - 1.0) < 1e-8, $"Column 0 length: {c0.Length}");
        Assert.True(System.Math.Abs(c1.Length - 1.0) < 1e-8, $"Column 1 length: {c1.Length}");
        Assert.True(System.Math.Abs(c2.Length - 1.0) < 1e-8, $"Column 2 length: {c2.Length}");

        Assert.True(System.Math.Abs(Vec3.Dot(c0, c1)) < 1e-8, $"c0.c1 = {Vec3.Dot(c0, c1)}");
        Assert.True(System.Math.Abs(Vec3.Dot(c0, c2)) < 1e-8, $"c0.c2 = {Vec3.Dot(c0, c2)}");
        Assert.True(System.Math.Abs(Vec3.Dot(c1, c2)) < 1e-8, $"c1.c2 = {Vec3.Dot(c1, c2)}");
    }
}
