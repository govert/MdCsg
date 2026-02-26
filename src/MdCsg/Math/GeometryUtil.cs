namespace MdCsg.Math;

/// <summary>
/// Utility methods for common geometric computations.
/// </summary>
public static class GeometryUtil
{
    /// <summary>
    /// Returns the nearest point on triangle (a,b,c) to point p.
    /// Uses the Voronoi region algorithm (same structure as ConfidentPoint.PointTriangleDistanceSq).
    /// </summary>
    /// <param name="p">The query point.</param>
    /// <param name="a">First triangle vertex.</param>
    /// <param name="b">Second triangle vertex.</param>
    /// <param name="c">Third triangle vertex.</param>
    /// <returns>The closest point on the triangle to p.</returns>
    public static Vec3 NearestPointOnTriangle(Vec3 p, Vec3 a, Vec3 b, Vec3 c)
    {
        var ab = b - a;
        var ac = c - a;
        var ap = p - a;

        double d1 = Vec3.Dot(ab, ap);
        double d2 = Vec3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0) return a;

        var bp = p - b;
        double d3 = Vec3.Dot(ab, bp);
        double d4 = Vec3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3) return b;

        var cp = p - c;
        double d5 = Vec3.Dot(ab, cp);
        double d6 = Vec3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6) return c;

        double vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0)
        {
            double v = d1 / (d1 - d3);
            return a + ab * v;
        }

        double vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0)
        {
            double w = d2 / (d2 - d6);
            return a + ac * w;
        }

        double va = d3 * d6 - d5 * d4;
        if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
        {
            double w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + (c - b) * w;
        }

        double denom = 1.0 / (va + vb + vc);
        double v2 = vb * denom;
        double w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }

    /// <summary>
    /// Computes the circumcircle of triangle (a,b,c) in 3D.
    /// Uses 2x2 Cramer's rule in the triangle's plane.
    /// </summary>
    /// <param name="a">First triangle vertex.</param>
    /// <param name="b">Second triangle vertex.</param>
    /// <param name="c">Third triangle vertex.</param>
    /// <returns>Center and radius of the circumscribed circle.</returns>
    public static (Vec3 Center, double Radius) Circumcircle(Vec3 a, Vec3 b, Vec3 c)
    {
        var ab = b - a;
        var ac = c - a;

        double d00 = Vec3.Dot(ab, ab);
        double d01 = Vec3.Dot(ab, ac);
        double d11 = Vec3.Dot(ac, ac);

        double det = d00 * d11 - d01 * d01;
        if (System.Math.Abs(det) < 1e-30)
        {
            // Degenerate triangle — return centroid with zero radius
            var centroid = (a + b + c) / 3.0;
            return (centroid, 0);
        }

        double invDet = 1.0 / det;
        double u = (d11 * d00 - d01 * d01) * 0.5 * invDet;
        double v = (d00 * d11 - d01 * d01) * 0.5 * invDet;

        // Actually use correct Cramer's formula:
        // u * ab + v * ac = circumcenter offset from a
        // The system is:
        // d00 * u + d01 * v = d00/2
        // d01 * u + d11 * v = d11/2
        double rhs0 = d00 * 0.5;
        double rhs1 = d11 * 0.5;
        u = (rhs0 * d11 - rhs1 * d01) * invDet;
        v = (d00 * rhs1 - d01 * rhs0) * invDet;

        var center = a + ab * u + ac * v;
        double radius = Vec3.Distance(center, a);
        return (center, radius);
    }

    /// <summary>
    /// Computes the circumsphere of tetrahedron (a,b,c,d).
    /// Solves the 3x3 system via <see cref="LinearSolve3"/>.
    /// </summary>
    /// <param name="a">First vertex.</param>
    /// <param name="b">Second vertex.</param>
    /// <param name="c">Third vertex.</param>
    /// <param name="d">Fourth vertex.</param>
    /// <returns>Center and radius of the circumscribed sphere.</returns>
    public static (Vec3 Center, double Radius) Circumsphere(Vec3 a, Vec3 b, Vec3 c, Vec3 d)
    {
        // Translate so a is at origin
        var ab = b - a;
        var ac = c - a;
        var ad = d - a;

        // Solve:
        // 2*(bx*x + by*y + bz*z) = |b|^2
        // 2*(cx*x + cy*y + cz*z) = |c|^2
        // 2*(dx*x + dy*y + dz*z) = |d|^2
        var mat = new Mat3(
            2 * ab.X, 2 * ab.Y, 2 * ab.Z,
            2 * ac.X, 2 * ac.Y, 2 * ac.Z,
            2 * ad.X, 2 * ad.Y, 2 * ad.Z);

        var rhs = new Vec3(ab.LengthSquared, ac.LengthSquared, ad.LengthSquared);

        if (!LinearSolve3.Solve(mat, rhs, out var offset))
        {
            // Degenerate — return centroid with zero radius
            var centroid = (a + b + c + d) * 0.25;
            return (centroid, 0);
        }

        var center = a + offset;
        double radius = offset.Length;
        return (center, radius);
    }
}
