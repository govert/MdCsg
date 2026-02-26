namespace MdCsg.Math;

/// <summary>
/// Eigenvalue decomposition for 3x3 symmetric matrices using the Jacobi method.
/// </summary>
public static class Eigen3
{
    /// <summary>
    /// Computes the eigenvalues and eigenvectors of a 3x3 symmetric matrix.
    /// Returns eigenvalues in ascending order with corresponding eigenvectors.
    /// </summary>
    /// <param name="m">A symmetric 3x3 matrix.</param>
    /// <param name="maxIterations">Maximum number of sweep iterations.</param>
    /// <returns>
    /// Eigenvalues sorted ascending, and a matrix whose columns are the corresponding eigenvectors.
    /// </returns>
    public static (Vec3 Eigenvalues, Mat3 Eigenvectors) Decompose(Mat3 m, int maxIterations = 50)
    {
        // Working copy of the matrix (only upper triangle matters for symmetric)
        double a00 = m.M00, a01 = m.M01, a02 = m.M02;
        double a11 = m.M11, a12 = m.M12;
        double a22 = m.M22;

        // Eigenvector matrix starts as identity
        double v00 = 1, v01 = 0, v02 = 0;
        double v10 = 0, v11 = 1, v12 = 0;
        double v20 = 0, v21 = 0, v22 = 1;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Check convergence: sum of squares of off-diagonal elements
            double off = a01 * a01 + a02 * a02 + a12 * a12;
            if (off < 1e-30)
                break;

            // Rotate to zero the largest off-diagonal element
            double abs01 = System.Math.Abs(a01);
            double abs02 = System.Math.Abs(a02);
            double abs12 = System.Math.Abs(a12);

            int p, q;
            if (abs01 >= abs02 && abs01 >= abs12) { p = 0; q = 1; }
            else if (abs02 >= abs12) { p = 0; q = 2; }
            else { p = 1; q = 2; }

            // Get elements for the rotation
            double app = GetDiag(p, a00, a11, a22);
            double aqq = GetDiag(q, a00, a11, a22);
            double apq = GetOff(p, q, a01, a02, a12);

            if (System.Math.Abs(apq) < 1e-30)
                continue;

            // Compute Jacobi rotation angle
            double tau = (aqq - app) / (2.0 * apq);
            double t = (tau >= 0 ? 1.0 : -1.0) / (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
            double c = 1.0 / System.Math.Sqrt(1.0 + t * t);
            double s = t * c;

            // Apply Givens rotation to the matrix
            ApplyRotation(p, q, c, s,
                ref a00, ref a01, ref a02,
                ref a11, ref a12, ref a22);

            // Accumulate eigenvectors
            ApplyEigenvectorRotation(p, q, c, s,
                ref v00, ref v01, ref v02,
                ref v10, ref v11, ref v12,
                ref v20, ref v21, ref v22);
        }

        // Eigenvalues are the diagonal elements
        double e0 = a00, e1 = a11, e2 = a22;
        var col0 = new Vec3(v00, v10, v20);
        var col1 = new Vec3(v01, v11, v21);
        var col2 = new Vec3(v02, v12, v22);

        // Sort eigenvalues ascending with their eigenvectors
        SortAscending(ref e0, ref e1, ref e2, ref col0, ref col1, ref col2);

        return (new Vec3(e0, e1, e2), Mat3.FromColumns(col0, col1, col2));
    }

    private static double GetDiag(int i, double a00, double a11, double a22) =>
        i == 0 ? a00 : i == 1 ? a11 : a22;

    private static double GetOff(int p, int q, double a01, double a02, double a12)
    {
        if (p == 0 && q == 1) return a01;
        if (p == 0 && q == 2) return a02;
        return a12; // p==1, q==2
    }

    private static void ApplyRotation(int p, int q, double c, double s,
        ref double a00, ref double a01, ref double a02,
        ref double a11, ref double a12, ref double a22)
    {
        // Get the third index
        int r = 3 - p - q;

        double app = GetDiag(p, a00, a11, a22);
        double aqq = GetDiag(q, a00, a11, a22);
        double apq = GetOff(p, q, a01, a02, a12);

        double apr = GetElement(p, r, a00, a01, a02, a11, a12, a22);
        double aqr = GetElement(q, r, a00, a01, a02, a11, a12, a22);

        double newApp = c * c * app - 2 * s * c * apq + s * s * aqq;
        double newAqq = s * s * app + 2 * s * c * apq + c * c * aqq;
        double newApr = c * apr - s * aqr;
        double newAqr = s * apr + c * aqr;

        SetDiag(p, newApp, ref a00, ref a11, ref a22);
        SetDiag(q, newAqq, ref a00, ref a11, ref a22);
        SetElement(p, q, 0, ref a01, ref a02, ref a12); // zero out the target
        SetElement(p, r, newApr, ref a01, ref a02, ref a12);
        SetElement(q, r, newAqr, ref a01, ref a02, ref a12);
    }

    private static double GetElement(int i, int j, double a00, double a01, double a02,
        double a11, double a12, double a22)
    {
        if (i > j) { int tmp = i; i = j; j = tmp; }
        if (i == 0 && j == 0) return a00;
        if (i == 0 && j == 1) return a01;
        if (i == 0 && j == 2) return a02;
        if (i == 1 && j == 1) return a11;
        if (i == 1 && j == 2) return a12;
        return a22;
    }

    private static void SetDiag(int i, double val, ref double a00, ref double a11, ref double a22)
    {
        if (i == 0) a00 = val;
        else if (i == 1) a11 = val;
        else a22 = val;
    }

    private static void SetElement(int i, int j, double val, ref double a01, ref double a02, ref double a12)
    {
        if (i > j) { int tmp = i; i = j; j = tmp; }
        if (i == 0 && j == 1) a01 = val;
        else if (i == 0 && j == 2) a02 = val;
        else a12 = val; // i==1, j==2
    }

    private static void ApplyEigenvectorRotation(int p, int q, double c, double s,
        ref double v00, ref double v01, ref double v02,
        ref double v10, ref double v11, ref double v12,
        ref double v20, ref double v21, ref double v22)
    {
        // Rotate columns p and q of the eigenvector matrix
        for (int row = 0; row < 3; row++)
        {
            double vp = GetV(row, p, v00, v01, v02, v10, v11, v12, v20, v21, v22);
            double vq = GetV(row, q, v00, v01, v02, v10, v11, v12, v20, v21, v22);
            double newVp = c * vp - s * vq;
            double newVq = s * vp + c * vq;
            SetV(row, p, newVp, ref v00, ref v01, ref v02, ref v10, ref v11, ref v12, ref v20, ref v21, ref v22);
            SetV(row, q, newVq, ref v00, ref v01, ref v02, ref v10, ref v11, ref v12, ref v20, ref v21, ref v22);
        }
    }

    private static double GetV(int row, int col,
        double v00, double v01, double v02,
        double v10, double v11, double v12,
        double v20, double v21, double v22)
    {
        if (row == 0) return col == 0 ? v00 : col == 1 ? v01 : v02;
        if (row == 1) return col == 0 ? v10 : col == 1 ? v11 : v12;
        return col == 0 ? v20 : col == 1 ? v21 : v22;
    }

    private static void SetV(int row, int col, double val,
        ref double v00, ref double v01, ref double v02,
        ref double v10, ref double v11, ref double v12,
        ref double v20, ref double v21, ref double v22)
    {
        if (row == 0)
        {
            if (col == 0) v00 = val; else if (col == 1) v01 = val; else v02 = val;
        }
        else if (row == 1)
        {
            if (col == 0) v10 = val; else if (col == 1) v11 = val; else v12 = val;
        }
        else
        {
            if (col == 0) v20 = val; else if (col == 1) v21 = val; else v22 = val;
        }
    }

    private static void SortAscending(ref double e0, ref double e1, ref double e2,
        ref Vec3 v0, ref Vec3 v1, ref Vec3 v2)
    {
        // Simple bubble sort for 3 elements
        if (e0 > e1) { Swap(ref e0, ref e1); Swap(ref v0, ref v1); }
        if (e1 > e2) { Swap(ref e1, ref e2); Swap(ref v1, ref v2); }
        if (e0 > e1) { Swap(ref e0, ref e1); Swap(ref v0, ref v1); }
    }

    private static void Swap(ref double a, ref double b) { double t = a; a = b; b = t; }
    private static void Swap(ref Vec3 a, ref Vec3 b) { var t = a; a = b; b = t; }
}
