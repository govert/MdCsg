using MdCsg.Bvh;
using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies a point as inside or outside a solid using ray casting.
/// A point is inside if an odd number of ray-surface intersections occur.
/// Uses slightly perturbed ray directions to avoid hitting edges/vertices.
/// </summary>
public static class RayCastClassifier
{
    /// <summary>
    /// Classifies a point relative to a solid mesh using ray casting.
    /// Shoots rays along multiple perturbed directions for robustness.
    /// </summary>
    public static SolidClassification Classify(Vec3 point, BvhTree bvh)
    {
        // Use slightly perturbed directions to avoid hitting triangle edges/vertices
        var directions = new Vec3[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized,
        };

        int insideVotes = 0;
        foreach (var dir in directions)
        {
            var ray = new Ray(point, dir);
            int hits = bvh.RayCastCount(ray);
            if (hits % 2 == 1) insideVotes++;
        }

        return insideVotes >= 2 ? SolidClassification.Inside : SolidClassification.Outside;
    }
}
