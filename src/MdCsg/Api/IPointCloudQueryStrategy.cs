using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Api;

/// <summary>
/// Strategy interface for executing batch point cloud queries.
/// Implementations can target CPU or GPU.
/// </summary>
public interface IPointCloudQueryStrategy
{
    /// <summary>
    /// Classifies each point as inside or outside the solid.
    /// </summary>
    /// <param name="points">The query points.</param>
    /// <param name="results">Output classifications (same length as points).</param>
    /// <param name="bvh">The BVH of the solid mesh.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting.</param>
    void Classify(Vec3[] points, SolidClassification[] results, BvhTree bvh, bool useWindingNumber);

    /// <summary>
    /// Computes the unsigned distance from each point to the nearest surface.
    /// </summary>
    /// <param name="points">The query points.</param>
    /// <param name="results">Output distances (same length as points).</param>
    /// <param name="bvh">The BVH of the solid mesh.</param>
    void Distances(Vec3[] points, double[] results, BvhTree bvh);

    /// <summary>
    /// Computes the signed distance from each point to the surface.
    /// Negative values indicate inside, positive outside.
    /// </summary>
    /// <param name="points">The query points.</param>
    /// <param name="results">Output signed distances (same length as points).</param>
    /// <param name="bvh">The BVH of the solid mesh.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting.</param>
    void SignedDistances(Vec3[] points, double[] results, BvhTree bvh, bool useWindingNumber);
}
