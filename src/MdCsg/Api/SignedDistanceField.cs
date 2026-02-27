using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Api;

/// <summary>
/// Provides signed distance queries against a solid mesh.
/// The signed distance is negative inside the solid, positive outside, and zero on the surface.
/// </summary>
public class SignedDistanceField
{
    private readonly BvhPointClassifier _classifier;

    /// <summary>
    /// Creates a signed distance field from a solid.
    /// </summary>
    /// <param name="solid">The solid to query against.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting for classification.</param>
    public SignedDistanceField(Solid solid, bool useWindingNumber = false)
    {
        _classifier = new BvhPointClassifier(solid.Bvh, useWindingNumber);
    }

    /// <summary>
    /// Computes the signed distance from a point to the solid surface.
    /// Returns a negative value if the point is inside, positive if outside.
    /// </summary>
    /// <param name="point">The query point.</param>
    /// <returns>Signed distance: negative inside, positive outside.</returns>
    public double SignedDistance(Vec3 point)
    {
        double dist = _classifier.DistanceToSurface(point);
        if (dist <= 1e-15)
            dist = 1e-15;
        var cls = _classifier.Classify(point);
        return cls == SolidClassification.Inside ? -dist : dist;
    }

    /// <summary>
    /// Computes signed distances for a batch of points.
    /// </summary>
    /// <param name="points">Input points to query.</param>
    /// <param name="results">Output span receiving the signed distances (same length as points).</param>
    public void SignedDistances(ReadOnlySpan<Vec3> points, Span<double> results)
    {
        for (int i = 0; i < points.Length; i++)
            results[i] = SignedDistance(points[i]);
    }

    /// <summary>
    /// Samples the signed distance field on a regular grid within the given bounds.
    /// </summary>
    /// <param name="bounds">The axis-aligned bounding box defining the sampling volume.</param>
    /// <param name="nx">Number of samples along the X axis.</param>
    /// <param name="ny">Number of samples along the Y axis.</param>
    /// <param name="nz">Number of samples along the Z axis.</param>
    /// <returns>A 3D array of signed distances indexed by [x, y, z].</returns>
    public double[,,] SampleOnGrid(Aabb bounds, int nx, int ny, int nz)
    {
        var result = new double[nx, ny, nz];

        double dx = nx > 1 ? (bounds.Max.X - bounds.Min.X) / (nx - 1) : 0;
        double dy = ny > 1 ? (bounds.Max.Y - bounds.Min.Y) / (ny - 1) : 0;
        double dz = nz > 1 ? (bounds.Max.Z - bounds.Min.Z) / (nz - 1) : 0;

        for (int ix = 0; ix < nx; ix++)
        {
            double x = bounds.Min.X + ix * dx;
            for (int iy = 0; iy < ny; iy++)
            {
                double y = bounds.Min.Y + iy * dy;
                for (int iz = 0; iz < nz; iz++)
                {
                    double z = bounds.Min.Z + iz * dz;
                    result[ix, iy, iz] = SignedDistance(new Vec3(x, y, z));
                }
            }
        }

        return result;
    }
}
