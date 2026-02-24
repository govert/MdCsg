namespace MdCsg.Arithmetic;

/// <summary>
/// Adaptive precision determinant evaluation with escalation strategy:
/// double → expansion → Rational.
/// </summary>
public static class AdaptivePrecision
{
    /// <summary>
    /// Returns the sign of a 2×2 determinant: |a b; c d| = ad - bc.
    /// Uses adaptive precision escalation.
    /// </summary>
    public static int Det2x2Sign(double a, double b, double c, double d)
    {
        // Fast path: double precision
        var det = a * d - b * c;
        var errBound = (System.Math.Abs(a * d) + System.Math.Abs(b * c)) * ErrorBound.Orient2DErrorBoundA;
        if (System.Math.Abs(det) > errBound)
            return System.Math.Sign(det);

        // Medium path: expansion arithmetic
        Span<double> ad = stackalloc double[2];
        Span<double> bc = stackalloc double[2];
        var (adProd, adErr) = ExpansionArithmetic.TwoProduct(a, d);
        ad[0] = adErr; ad[1] = adProd;
        var (bcProd, bcErr) = ExpansionArithmetic.TwoProduct(b, c);
        bc[0] = bcErr; bc[1] = bcProd;

        // ad - bc
        ExpansionArithmetic.Negate(bc);
        Span<double> result = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(ad, bc, result);
        int sign = ExpansionArithmetic.Sign(result[..len]);
        if (sign != 0)
            return sign;

        // Exact path: Rational
        var ra = Rational.FromDouble(a);
        var rb = Rational.FromDouble(b);
        var rc = Rational.FromDouble(c);
        var rd = Rational.FromDouble(d);
        return (ra * rd - rb * rc).Sign;
    }

    /// <summary>
    /// Returns the sign of a 3×3 determinant using cofactor expansion along the first row.
    /// | a b c |
    /// | d e f |
    /// | g h i |
    /// = a(ei-fh) - b(di-fg) + c(dh-eg)
    /// </summary>
    public static int Det3x3Sign(
        double a, double b, double c,
        double d, double e, double f,
        double g, double h, double i)
    {
        // Fast path
        var ei_fh = e * i - f * h;
        var di_fg = d * i - f * g;
        var dh_eg = d * h - e * g;
        var det = a * ei_fh - b * di_fg + c * dh_eg;

        var permanent = System.Math.Abs(a) * (System.Math.Abs(e * i) + System.Math.Abs(f * h))
                      + System.Math.Abs(b) * (System.Math.Abs(d * i) + System.Math.Abs(f * g))
                      + System.Math.Abs(c) * (System.Math.Abs(d * h) + System.Math.Abs(e * g));
        var errBound = permanent * ErrorBound.Orient3DErrorBoundA;

        if (System.Math.Abs(det) > errBound)
            return System.Math.Sign(det);

        // Exact path: Rational (skip expansion for 3x3 — go straight to exact)
        var ra = Rational.FromDouble(a); var rb = Rational.FromDouble(b); var rc = Rational.FromDouble(c);
        var rd = Rational.FromDouble(d); var re = Rational.FromDouble(e); var rf = Rational.FromDouble(f);
        var rg = Rational.FromDouble(g); var rh = Rational.FromDouble(h); var ri = Rational.FromDouble(i);

        var result = ra * (re * ri - rf * rh)
                   - rb * (rd * ri - rf * rg)
                   + rc * (rd * rh - re * rg);
        return result.Sign;
    }

    /// <summary>
    /// Returns the sign of a 4×4 determinant using cofactor expansion.
    /// </summary>
    public static int Det4x4Sign(
        double a00, double a01, double a02, double a03,
        double a10, double a11, double a12, double a13,
        double a20, double a21, double a22, double a23,
        double a30, double a31, double a32, double a33)
    {
        // Direct Rational computation for 4x4 (expansion would be complex)
        var r = new Rational[4, 4];
        r[0, 0] = Rational.FromDouble(a00); r[0, 1] = Rational.FromDouble(a01);
        r[0, 2] = Rational.FromDouble(a02); r[0, 3] = Rational.FromDouble(a03);
        r[1, 0] = Rational.FromDouble(a10); r[1, 1] = Rational.FromDouble(a11);
        r[1, 2] = Rational.FromDouble(a12); r[1, 3] = Rational.FromDouble(a13);
        r[2, 0] = Rational.FromDouble(a20); r[2, 1] = Rational.FromDouble(a21);
        r[2, 2] = Rational.FromDouble(a22); r[2, 3] = Rational.FromDouble(a23);
        r[3, 0] = Rational.FromDouble(a30); r[3, 1] = Rational.FromDouble(a31);
        r[3, 2] = Rational.FromDouble(a32); r[3, 3] = Rational.FromDouble(a33);

        // Fast double path first
        var det = a00 * Minor3x3(a11, a12, a13, a21, a22, a23, a31, a32, a33)
                - a01 * Minor3x3(a10, a12, a13, a20, a22, a23, a30, a32, a33)
                + a02 * Minor3x3(a10, a11, a13, a20, a21, a23, a30, a31, a33)
                - a03 * Minor3x3(a10, a11, a12, a20, a21, a22, a30, a31, a32);

        // Rough error bound
        var permanent = System.Math.Abs(a00) * PermanentMinor3x3(a11, a12, a13, a21, a22, a23, a31, a32, a33)
                      + System.Math.Abs(a01) * PermanentMinor3x3(a10, a12, a13, a20, a22, a23, a30, a32, a33)
                      + System.Math.Abs(a02) * PermanentMinor3x3(a10, a11, a13, a20, a21, a23, a30, a31, a33)
                      + System.Math.Abs(a03) * PermanentMinor3x3(a10, a11, a12, a20, a21, a22, a30, a31, a32);
        var errBound = permanent * ErrorBound.InCircleErrorBoundA;

        if (System.Math.Abs(det) > errBound)
            return System.Math.Sign(det);

        // Exact Rational computation
        var result = r[0, 0] * Minor3x3R(r, 1, 2, 3, 1, 2, 3)
                   - r[0, 1] * Minor3x3R(r, 1, 2, 3, 0, 2, 3)
                   + r[0, 2] * Minor3x3R(r, 1, 2, 3, 0, 1, 3)
                   - r[0, 3] * Minor3x3R(r, 1, 2, 3, 0, 1, 2);
        return result.Sign;
    }

    private static double Minor3x3(
        double a, double b, double c,
        double d, double e, double f,
        double g, double h, double i) =>
        a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);

    private static double PermanentMinor3x3(
        double a, double b, double c,
        double d, double e, double f,
        double g, double h, double i) =>
        System.Math.Abs(a) * (System.Math.Abs(e * i) + System.Math.Abs(f * h))
      + System.Math.Abs(b) * (System.Math.Abs(d * i) + System.Math.Abs(f * g))
      + System.Math.Abs(c) * (System.Math.Abs(d * h) + System.Math.Abs(e * g));

    private static Rational Minor3x3R(Rational[,] m, int r0, int r1, int r2, int c0, int c1, int c2) =>
        m[r0, c0] * (m[r1, c1] * m[r2, c2] - m[r1, c2] * m[r2, c1])
      - m[r0, c1] * (m[r1, c0] * m[r2, c2] - m[r1, c2] * m[r2, c0])
      + m[r0, c2] * (m[r1, c0] * m[r2, c1] - m[r1, c1] * m[r2, c0]);
}
