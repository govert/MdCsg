using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Api;

/// <summary>
/// Executes batch point classification, distance, and signed distance queries
/// against a solid mesh using BVH acceleration.
/// </summary>
public class PointCloudQuery
{
    private readonly BvhTree _bvh;
    private readonly bool _useWindingNumber;
    private readonly IPointCloudQueryStrategy? _strategy;

    /// <summary>
    /// Creates a point cloud query engine for the given solid.
    /// </summary>
    /// <param name="solid">The solid to query against.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting for classification.</param>
    /// <param name="strategy">Optional strategy for GPU acceleration. If null, uses CPU parallelism.</param>
    public PointCloudQuery(Solid solid, bool useWindingNumber = false,
        IPointCloudQueryStrategy? strategy = null)
    {
        _bvh = solid.Bvh;
        _useWindingNumber = useWindingNumber;
        _strategy = strategy;
    }

    /// <summary>
    /// Classifies each point in the cloud as inside or outside the solid.
    /// </summary>
    /// <param name="cloud">The point cloud to classify.</param>
    /// <param name="parallel">If true, uses parallel execution on CPU (ignored when a strategy is provided).</param>
    /// <returns>An array of classifications for each point.</returns>
    public SolidClassification[] Classify(PointCloud cloud, bool parallel = true)
    {
        var results = new SolidClassification[cloud.Count];
        if (cloud.Count == 0) return results;

        if (_strategy != null)
        {
            _strategy.Classify(cloud.Points, results, _bvh, _useWindingNumber);
            return results;
        }

        ClassifyCpu(cloud.Points, results, parallel);
        return results;
    }

    /// <summary>
    /// Computes the unsigned distance from each point to the nearest mesh surface.
    /// </summary>
    /// <param name="cloud">The point cloud to query.</param>
    /// <param name="parallel">If true, uses parallel execution on CPU (ignored when a strategy is provided).</param>
    /// <returns>An array of unsigned distances for each point.</returns>
    public double[] Distances(PointCloud cloud, bool parallel = true)
    {
        var results = new double[cloud.Count];
        if (cloud.Count == 0) return results;

        if (_strategy != null)
        {
            _strategy.Distances(cloud.Points, results, _bvh);
            return results;
        }

        DistancesCpu(cloud.Points, results, parallel);
        return results;
    }

    /// <summary>
    /// Computes the signed distance from each point to the surface.
    /// Negative values indicate inside, positive values indicate outside.
    /// </summary>
    /// <param name="cloud">The point cloud to query.</param>
    /// <param name="parallel">If true, uses parallel execution on CPU (ignored when a strategy is provided).</param>
    /// <returns>An array of signed distances for each point.</returns>
    public double[] SignedDistances(PointCloud cloud, bool parallel = true)
    {
        var results = new double[cloud.Count];
        if (cloud.Count == 0) return results;

        if (_strategy != null)
        {
            _strategy.SignedDistances(cloud.Points, results, _bvh, _useWindingNumber);
            return results;
        }

        SignedDistancesCpu(cloud.Points, results, parallel);
        return results;
    }

    /// <summary>
    /// Classifies and computes unsigned distances in a single pass.
    /// </summary>
    /// <param name="cloud">The point cloud to query.</param>
    /// <param name="classifications">Output classifications (must be same length as cloud).</param>
    /// <param name="distances">Output unsigned distances (must be same length as cloud).</param>
    /// <param name="parallel">If true, uses parallel execution on CPU.</param>
    public void ClassifyWithDistances(PointCloud cloud,
        SolidClassification[] classifications, double[] distances, bool parallel = true)
    {
        if (cloud.Count == 0) return;

        if (_strategy != null)
        {
            _strategy.Classify(cloud.Points, classifications, _bvh, _useWindingNumber);
            _strategy.Distances(cloud.Points, distances, _bvh);
            return;
        }

        var points = cloud.Points;
        if (parallel)
        {
            Parallel.For(0, points.Length, i =>
            {
                classifications[i] = ClassifyPoint(points[i]);
                distances[i] = ConfidentPoint.ComputeMinDistanceToMesh(points[i], _bvh);
            });
        }
        else
        {
            for (int i = 0; i < points.Length; i++)
            {
                classifications[i] = ClassifyPoint(points[i]);
                distances[i] = ConfidentPoint.ComputeMinDistanceToMesh(points[i], _bvh);
            }
        }
    }

    /// <summary>
    /// Returns a new point cloud containing only the points classified as inside the solid.
    /// </summary>
    /// <param name="cloud">The point cloud to filter.</param>
    /// <param name="parallel">If true, uses parallel execution on CPU.</param>
    /// <returns>A new point cloud with only inside points.</returns>
    public PointCloud FilterInside(PointCloud cloud, bool parallel = true)
    {
        var cls = Classify(cloud, parallel);
        var mask = new bool[cloud.Count];
        for (int i = 0; i < cls.Length; i++)
            mask[i] = cls[i] == SolidClassification.Inside;
        return cloud.Where(mask);
    }

    /// <summary>
    /// Returns a new point cloud containing only the points classified as outside the solid.
    /// </summary>
    /// <param name="cloud">The point cloud to filter.</param>
    /// <param name="parallel">If true, uses parallel execution on CPU.</param>
    /// <returns>A new point cloud with only outside points.</returns>
    public PointCloud FilterOutside(PointCloud cloud, bool parallel = true)
    {
        var cls = Classify(cloud, parallel);
        var mask = new bool[cloud.Count];
        for (int i = 0; i < cls.Length; i++)
            mask[i] = cls[i] == SolidClassification.Outside;
        return cloud.Where(mask);
    }

    /// <summary>
    /// Returns a new point cloud containing only points within <paramref name="maxDistance"/> of the surface.
    /// </summary>
    /// <param name="cloud">The point cloud to filter.</param>
    /// <param name="maxDistance">Maximum distance from the mesh surface.</param>
    /// <param name="parallel">If true, uses parallel execution on CPU.</param>
    /// <returns>A new point cloud with only near-surface points.</returns>
    public PointCloud FilterNearSurface(PointCloud cloud, double maxDistance, bool parallel = true)
    {
        var dists = Distances(cloud, parallel);
        var mask = new bool[cloud.Count];
        for (int i = 0; i < dists.Length; i++)
            mask[i] = dists[i] <= maxDistance;
        return cloud.Where(mask);
    }

    private void ClassifyCpu(Vec3[] points, SolidClassification[] results, bool parallel)
    {
        if (parallel)
        {
            Parallel.For(0, points.Length, i =>
            {
                results[i] = ClassifyPoint(points[i]);
            });
        }
        else
        {
            for (int i = 0; i < points.Length; i++)
                results[i] = ClassifyPoint(points[i]);
        }
    }

    private void DistancesCpu(Vec3[] points, double[] results, bool parallel)
    {
        if (parallel)
        {
            Parallel.For(0, points.Length, i =>
            {
                results[i] = ConfidentPoint.ComputeMinDistanceToMesh(points[i], _bvh);
            });
        }
        else
        {
            for (int i = 0; i < points.Length; i++)
                results[i] = ConfidentPoint.ComputeMinDistanceToMesh(points[i], _bvh);
        }
    }

    private void SignedDistancesCpu(Vec3[] points, double[] results, bool parallel)
    {
        if (parallel)
        {
            Parallel.For(0, points.Length, i =>
            {
                double dist = ConfidentPoint.ComputeMinDistanceToMesh(points[i], _bvh);
                var cls = ClassifyPoint(points[i]);
                results[i] = cls == SolidClassification.Inside ? -dist : dist;
            });
        }
        else
        {
            for (int i = 0; i < points.Length; i++)
            {
                double dist = ConfidentPoint.ComputeMinDistanceToMesh(points[i], _bvh);
                var cls = ClassifyPoint(points[i]);
                results[i] = cls == SolidClassification.Inside ? -dist : dist;
            }
        }
    }

    private SolidClassification ClassifyPoint(Vec3 point) =>
        _useWindingNumber
            ? WindingNumberClassifier.Classify(point, _bvh)
            : RayCastClassifier.Classify(point, _bvh);
}
