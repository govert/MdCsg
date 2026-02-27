using MdCsg.Arithmetic;
using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Robust.Kernel.Predicates;

public static class CertifiedPredicates
{
    public static CertifiedPredicateResult Orient2D(Vec2 a, Vec2 b, Vec2 c)
    {
        double acx = a.X - c.X;
        double acy = a.Y - c.Y;
        double bcx = b.X - c.X;
        double bcy = b.Y - c.Y;

        double det = acx * bcy - acy * bcx;
        double errBound = (System.Math.Abs(acx * bcy) + System.Math.Abs(acy * bcx))
            * ErrorBound.Orient2DErrorBoundA;

        if (System.Math.Abs(det) > errBound)
        {
            return new CertifiedPredicateResult(
                ToSign(System.Math.Sign(det)),
                PredicatePrecisionTier.Double,
                det,
                errBound);
        }

        Span<double> ad = stackalloc double[2];
        Span<double> bc = stackalloc double[2];
        var (adProd, adErr) = ExpansionArithmetic.TwoProduct(acx, bcy);
        ad[0] = adErr;
        ad[1] = adProd;

        var (bcProd, bcErr) = ExpansionArithmetic.TwoProduct(acy, bcx);
        bc[0] = bcErr;
        bc[1] = bcProd;

        ExpansionArithmetic.Negate(bc);
        Span<double> result = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(ad, bc, result);
        int expansionSign = ExpansionArithmetic.Sign(result[..len]);
        if (expansionSign != 0)
        {
            return new CertifiedPredicateResult(
                ToSign(expansionSign),
                PredicatePrecisionTier.Expansion,
                det,
                errBound);
        }

        var ra = Rational.FromDouble(acx);
        var rb = Rational.FromDouble(acy);
        var rc = Rational.FromDouble(bcx);
        var rd = Rational.FromDouble(bcy);
        int exactSign = (ra * rd - rb * rc).Sign;

        return new CertifiedPredicateResult(
            ToSign(exactSign),
            PredicatePrecisionTier.Exact,
            det,
            errBound);
    }

    public static CertifiedPredicateResult Orient3D(Vec3 a, Vec3 b, Vec3 c, Vec3 d)
    {
        double bax = b.X - a.X;
        double bay = b.Y - a.Y;
        double baz = b.Z - a.Z;
        double cax = c.X - a.X;
        double cay = c.Y - a.Y;
        double caz = c.Z - a.Z;
        double dax = d.X - a.X;
        double day = d.Y - a.Y;
        double daz = d.Z - a.Z;

        double eiFh = cay * daz - caz * day;
        double diFg = cax * daz - caz * dax;
        double dhEg = cax * day - cay * dax;
        double det = bax * eiFh - bay * diFg + baz * dhEg;

        double permanent = System.Math.Abs(bax) * (System.Math.Abs(cay * daz) + System.Math.Abs(caz * day))
                         + System.Math.Abs(bay) * (System.Math.Abs(cax * daz) + System.Math.Abs(caz * dax))
                         + System.Math.Abs(baz) * (System.Math.Abs(cax * day) + System.Math.Abs(cay * dax));
        double errBound = permanent * ErrorBound.Orient3DErrorBoundA;

        if (System.Math.Abs(det) > errBound)
        {
            return new CertifiedPredicateResult(
                ToSign(System.Math.Sign(det)),
                PredicatePrecisionTier.Double,
                det,
                errBound);
        }

        var rBax = Rational.FromDouble(bax); var rBay = Rational.FromDouble(bay); var rBaz = Rational.FromDouble(baz);
        var rCax = Rational.FromDouble(cax); var rCay = Rational.FromDouble(cay); var rCaz = Rational.FromDouble(caz);
        var rDax = Rational.FromDouble(dax); var rDay = Rational.FromDouble(day); var rDaz = Rational.FromDouble(daz);

        var exact = rBax * (rCay * rDaz - rCaz * rDay)
                  - rBay * (rCax * rDaz - rCaz * rDax)
                  + rBaz * (rCax * rDay - rCay * rDax);

        return new CertifiedPredicateResult(
            ToSign(exact.Sign),
            PredicatePrecisionTier.Exact,
            det,
            errBound);
    }

    private static PredicateSign ToSign(int sign) => sign switch
    {
        < 0 => PredicateSign.Negative,
        > 0 => PredicateSign.Positive,
        _ => PredicateSign.Zero
    };
}
