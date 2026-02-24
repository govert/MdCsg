using MdCsg.Bvh;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Classification;

/// <summary>
/// Selects the "most confident" point within a patch for classification.
/// The confident point is the sub-triangle centroid with the maximum minimum distance
/// to any face of the other solid. This is the key insight of the Patch-Confident CSG algorithm:
/// classifying at maximum-margin points makes double-precision arithmetic provably correct.
/// </summary>
public static class ConfidentPoint
{
    /// <summary>
    /// Finds the confident point for a patch by selecting the sub-triangle centroid
    /// with the largest distance to the nearest face of the other solid.
    /// </summary>
    /// <param name="patch">The patch to find a confident point for.</param>
    /// <param name="subTriangles">All sub-triangles.</param>
    /// <param name="otherBvh">BVH of the other solid's mesh.</param>
    /// <returns>The confident point and its margin (distance to nearest face).</returns>
    public static (Vec3 Point, double Margin) FindConfidentPoint(
        Patch patch,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh)
    {
        Vec3 bestPoint = Vec3.Zero;
        double bestMargin = -1;

        foreach (int triIdx in patch.SubTriangleIndices)
        {
            var tri = subTriangles[triIdx];
            var centroid = (tri.A + tri.B + tri.C) / 3.0;

            double margin = ComputeMinDistanceToMesh(centroid, otherBvh);

            if (margin > bestMargin)
            {
                bestMargin = margin;
                bestPoint = centroid;
            }
        }

        return (bestPoint, bestMargin);
    }

    /// <summary>
    /// Computes the minimum distance from a point to the nearest face of a mesh via BVH.
    /// </summary>
    private static double ComputeMinDistanceToMesh(Vec3 point, BvhTree bvh)
    {
        if (bvh.NodeCount == 0)
            return double.MaxValue;

        double minDistSq = double.MaxValue;
        ComputeMinDistRecursive(bvh, 0, point, ref minDistSq);
        return System.Math.Sqrt(minDistSq);
    }

    private static void ComputeMinDistRecursive(BvhTree bvh, int nodeIndex, Vec3 point, ref double minDistSq)
    {
        var nodes = bvh.Nodes;
        ref readonly var node = ref nodes[nodeIndex];

        // Early exit if node AABB is too far
        double boxDistSq = AabbDistanceSq(point, node.Bounds);
        if (boxDistSq >= minDistSq)
            return;

        if (node.IsLeaf)
        {
            for (int i = 0; i < node.PrimitiveCount; i++)
            {
                int faceIdx = bvh.GetFaceIndex(node.LeftOrStart + i);
                var face = bvh.Mesh.Faces[faceIdx];
                var verts = face.GetVertices();
                double dist = PointTriangleDistanceSq(point, verts[0].Position, verts[1].Position, verts[2].Position);
                if (dist < minDistSq)
                    minDistSq = dist;
            }
        }
        else
        {
            ComputeMinDistRecursive(bvh, node.LeftOrStart, point, ref minDistSq);
            ComputeMinDistRecursive(bvh, node.Right, point, ref minDistSq);
        }
    }

    private static double AabbDistanceSq(Vec3 point, Aabb box)
    {
        double dx = System.Math.Max(box.Min.X - point.X, System.Math.Max(0, point.X - box.Max.X));
        double dy = System.Math.Max(box.Min.Y - point.Y, System.Math.Max(0, point.Y - box.Max.Y));
        double dz = System.Math.Max(box.Min.Z - point.Z, System.Math.Max(0, point.Z - box.Max.Z));
        return dx * dx + dy * dy + dz * dz;
    }

    /// <summary>
    /// Computes the squared distance from a point to a triangle.
    /// </summary>
    public static double PointTriangleDistanceSq(Vec3 p, Vec3 a, Vec3 b, Vec3 c)
    {
        var ab = b - a;
        var ac = c - a;
        var ap = p - a;

        double d1 = Vec3.Dot(ab, ap);
        double d2 = Vec3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0) return Vec3.DistanceSquared(p, a);

        var bp = p - b;
        double d3 = Vec3.Dot(ab, bp);
        double d4 = Vec3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3) return Vec3.DistanceSquared(p, b);

        var cp = p - c;
        double d5 = Vec3.Dot(ab, cp);
        double d6 = Vec3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6) return Vec3.DistanceSquared(p, c);

        double vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0)
        {
            double v = d1 / (d1 - d3);
            var proj = a + ab * v;
            return Vec3.DistanceSquared(p, proj);
        }

        double vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0)
        {
            double w = d2 / (d2 - d6);
            var proj = a + ac * w;
            return Vec3.DistanceSquared(p, proj);
        }

        double va = d3 * d6 - d5 * d4;
        if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
        {
            double w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            var proj = b + (c - b) * w;
            return Vec3.DistanceSquared(p, proj);
        }

        double denom = 1.0 / (va + vb + vc);
        double v2 = vb * denom;
        double w2 = vc * denom;
        var closest = a + ab * v2 + ac * w2;
        return Vec3.DistanceSquared(p, closest);
    }
}
