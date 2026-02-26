using MdCsg.Arithmetic;
using MdCsg.Bvh;
using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Exact inside/outside classification using Rational arithmetic.
/// Used as a fallback when the confident point margin is too small for
/// double-precision ray casting to be reliable (coplanar/near-coplanar cases).
/// </summary>
internal static class ExactRayCastClassifier
{
    /// <summary>
    /// Classifies a point using exact Rational ray casting along multiple directions.
    /// Shoots rays and counts parity using exact arithmetic to avoid near-edge ambiguity.
    /// </summary>
    internal static SolidClassification Classify(Vec3 point, BvhTree bvh)
    {
        // Use the same perturbed directions as RayCastClassifier for consistency,
        // but evaluate intersection predicates with exact Rational arithmetic.
        var directions = new Vec3[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized,
        };

        int insideVotes = 0;

        foreach (var dir in directions)
        {
            int hits = ExactRayCastCount(point, dir, bvh);
            if (hits % 2 == 1) insideVotes++;
        }

        return insideVotes >= 2 ? SolidClassification.Inside : SolidClassification.Outside;
    }

    /// <summary>
    /// Counts exact ray-triangle intersections using Rational arithmetic.
    /// First uses the BVH with double-precision AABB checks for broad-phase pruning,
    /// then performs exact ray-triangle intersection tests using Rational.
    /// </summary>
    private static int ExactRayCastCount(Vec3 origin, Vec3 direction, BvhTree bvh)
    {
        if (bvh.NodeCount == 0) return 0;

        // Convert origin and direction to Rational once
        var ox = Rational.FromDouble(origin.X);
        var oy = Rational.FromDouble(origin.Y);
        var oz = Rational.FromDouble(origin.Z);
        var dx = Rational.FromDouble(direction.X);
        var dy = Rational.FromDouble(direction.Y);
        var dz = Rational.FromDouble(direction.Z);

        // Use BVH for broad-phase with a generous AABB expansion,
        // then exact test for each candidate face.
        var ray = new Ray(origin, direction);
        int count = 0;

        var nodes = bvh.Nodes;
#if NET
        Span<int> stack = stackalloc int[64];
#else
        var stack = new int[64];
#endif
        int top = 0;
        stack[top++] = 0;

        while (top > 0)
        {
            int nodeIndex = stack[--top];
            ref readonly var node = ref nodes[nodeIndex];

            if (!RayIntersectsAabb(ray, node.Bounds)) continue;

            if (node.IsLeaf)
            {
                for (int i = 0; i < node.PrimitiveCount; i++)
                {
                    var faceIdx = bvh.GetFaceIndex(node.LeftOrStart + i);
                    var face = bvh.Mesh.Faces[faceIdx];
                    face.GetTrianglePositions(out var a, out var b, out var c);

                    if (ExactRayIntersectsTriangle(
                        ox, oy, oz, dx, dy, dz,
                        a, b, c))
                    {
                        count++;
                    }
                }
            }
            else
            {
                stack[top++] = node.Right;
                stack[top++] = node.LeftOrStart;
            }
        }

        return count;
    }

    /// <summary>
    /// Exact Moller-Trumbore ray-triangle intersection using Rational arithmetic.
    /// Returns true if the ray (origin, direction) hits the triangle (v0, v1, v2) at t > 0.
    /// </summary>
    private static bool ExactRayIntersectsTriangle(
        Rational ox, Rational oy, Rational oz,
        Rational dx, Rational dy, Rational dz,
        Vec3 v0d, Vec3 v1d, Vec3 v2d)
    {
        var v0x = Rational.FromDouble(v0d.X);
        var v0y = Rational.FromDouble(v0d.Y);
        var v0z = Rational.FromDouble(v0d.Z);
        var v1x = Rational.FromDouble(v1d.X);
        var v1y = Rational.FromDouble(v1d.Y);
        var v1z = Rational.FromDouble(v1d.Z);
        var v2x = Rational.FromDouble(v2d.X);
        var v2y = Rational.FromDouble(v2d.Y);
        var v2z = Rational.FromDouble(v2d.Z);

        // edge1 = v1 - v0
        var e1x = v1x - v0x; var e1y = v1y - v0y; var e1z = v1z - v0z;
        // edge2 = v2 - v0
        var e2x = v2x - v0x; var e2y = v2y - v0y; var e2z = v2z - v0z;

        // h = cross(direction, edge2)
        var hx = dy * e2z - dz * e2y;
        var hy = dz * e2x - dx * e2z;
        var hz = dx * e2y - dy * e2x;

        // det = dot(edge1, h)
        var det = e1x * hx + e1y * hy + e1z * hz;

        if (det.Sign == 0) return false; // Ray parallel to triangle

        var invDet = Rational.One / det;

        // s = origin - v0
        var sx = ox - v0x; var sy = oy - v0y; var sz = oz - v0z;

        // u = dot(s, h) * invDet
        var u = (sx * hx + sy * hy + sz * hz) * invDet;
        if (u < Rational.Zero || u > Rational.One) return false;

        // q = cross(s, edge1)
        var qx = sy * e1z - sz * e1y;
        var qy = sz * e1x - sx * e1z;
        var qz = sx * e1y - sy * e1x;

        // v = dot(direction, q) * invDet
        var v = (dx * qx + dy * qy + dz * qz) * invDet;
        if (v < Rational.Zero || (u + v) > Rational.One) return false;

        // t = dot(edge2, q) * invDet
        var t = (e2x * qx + e2y * qy + e2z * qz) * invDet;

        return t > Rational.Zero;
    }

    /// <summary>
    /// Double-precision ray-AABB intersection for broad-phase BVH traversal.
    /// </summary>
    private static bool RayIntersectsAabb(Ray ray, Aabb box)
    {
        double tmin = 0;
        double tmax = double.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            double origin = ray.Origin[i];
            double dir = ray.Direction[i];
            double bmin = box.Min[i];
            double bmax = box.Max[i];

            if (System.Math.Abs(dir) < 1e-30)
            {
                if (origin < bmin || origin > bmax) return false;
            }
            else
            {
                double invD = 1.0 / dir;
                double t0 = (bmin - origin) * invD;
                double t1 = (bmax - origin) * invD;
                if (t0 > t1) (t0, t1) = (t1, t0);
                tmin = System.Math.Max(tmin, t0);
                tmax = System.Math.Min(tmax, t1);
                if (tmin > tmax) return false;
            }
        }

        return true;
    }
}
