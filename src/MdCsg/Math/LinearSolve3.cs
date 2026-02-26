namespace MdCsg.Math;

/// <summary>
/// Solves 3x3 linear systems using Gaussian elimination with partial pivoting.
/// </summary>
internal static class LinearSolve3
{
    /// <summary>
    /// Solves Ax = b for a 3x3 system. Returns false if the system is singular.
    /// </summary>
    internal static bool Solve(Mat3 a, Vec3 b, out Vec3 x)
    {
        // Augmented matrix [A|b] stored as 3 rows of 4 elements
        double r00 = a.M00, r01 = a.M01, r02 = a.M02, r03 = b.X;
        double r10 = a.M10, r11 = a.M11, r12 = a.M12, r13 = b.Y;
        double r20 = a.M20, r21 = a.M21, r22 = a.M22, r23 = b.Z;

        // Column 0: partial pivot
        double abs0 = System.Math.Abs(r00);
        double abs1 = System.Math.Abs(r10);
        double abs2 = System.Math.Abs(r20);
        if (abs1 > abs0 && abs1 >= abs2)
        {
            SwapRows(ref r00, ref r01, ref r02, ref r03, ref r10, ref r11, ref r12, ref r13);
        }
        else if (abs2 > abs0)
        {
            SwapRows(ref r00, ref r01, ref r02, ref r03, ref r20, ref r21, ref r22, ref r23);
        }

        if (System.Math.Abs(r00) < 1e-30) { x = Vec3.Zero; return false; }

        // Eliminate column 0 from rows 1,2
        double f1 = r10 / r00;
        r10 = 0; r11 -= f1 * r01; r12 -= f1 * r02; r13 -= f1 * r03;
        double f2 = r20 / r00;
        r20 = 0; r21 -= f2 * r01; r22 -= f2 * r02; r23 -= f2 * r03;

        // Column 1: partial pivot rows 1,2
        if (System.Math.Abs(r21) > System.Math.Abs(r11))
        {
            SwapRows(ref r10, ref r11, ref r12, ref r13, ref r20, ref r21, ref r22, ref r23);
        }

        if (System.Math.Abs(r11) < 1e-30) { x = Vec3.Zero; return false; }

        // Eliminate column 1 from row 2
        double f3 = r21 / r11;
        r21 = 0; r22 -= f3 * r12; r23 -= f3 * r13;

        if (System.Math.Abs(r22) < 1e-30) { x = Vec3.Zero; return false; }

        // Back-substitution
        double z = r23 / r22;
        double y = (r13 - r12 * z) / r11;
        double xv = (r03 - r01 * y - r02 * z) / r00;

        x = new Vec3(xv, y, z);
        return true;
    }

    private static void SwapRows(
        ref double a0, ref double a1, ref double a2, ref double a3,
        ref double b0, ref double b1, ref double b2, ref double b3)
    {
        double t;
        t = a0; a0 = b0; b0 = t;
        t = a1; a1 = b1; b1 = t;
        t = a2; a2 = b2; b2 = t;
        t = a3; a3 = b3; b3 = t;
    }
}
