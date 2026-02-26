using MdCsg.Math;

namespace MdCsg.Api;

/// <summary>
/// A collection of 3D points with optional normals.
/// </summary>
public class PointCloud
{
    /// <summary>The point positions.</summary>
    public Vec3[] Points { get; }

    /// <summary>Optional per-point normals (same length as <see cref="Points"/>, or null).</summary>
    public Vec3[]? Normals { get; }

    /// <summary>The number of points.</summary>
    public int Count => Points.Length;

    /// <summary>The axis-aligned bounding box enclosing all points.</summary>
    public Aabb Bounds { get; }

    /// <summary>
    /// Creates a point cloud from an array of positions with optional normals.
    /// </summary>
    /// <param name="points">The point positions.</param>
    /// <param name="normals">Optional per-point normals (must be same length as points if provided).</param>
    public PointCloud(Vec3[] points, Vec3[]? normals = null)
    {
        if (normals != null && normals.Length != points.Length)
            throw new ArgumentException("Normals array must have same length as points.", nameof(normals));

        Points = points;
        Normals = normals;
        Bounds = ComputeBounds(points);
    }

    /// <summary>
    /// Returns a new point cloud containing only the points at the given indices.
    /// </summary>
    /// <param name="indices">Indices into this cloud's <see cref="Points"/> array.</param>
    /// <returns>A new point cloud with the selected subset.</returns>
    public PointCloud Subset(int[] indices)
    {
        var pts = new Vec3[indices.Length];
        Vec3[]? nrm = Normals != null ? new Vec3[indices.Length] : null;
        for (int i = 0; i < indices.Length; i++)
        {
            pts[i] = Points[indices[i]];
            if (nrm != null)
                nrm[i] = Normals![indices[i]];
        }
        return new PointCloud(pts, nrm);
    }

    /// <summary>
    /// Returns a new point cloud containing only the points where the mask is true.
    /// </summary>
    /// <param name="mask">Boolean mask (must be same length as <see cref="Points"/>).</param>
    /// <returns>A new point cloud with the filtered subset.</returns>
    public PointCloud Where(bool[] mask)
    {
        if (mask.Length != Points.Length)
            throw new ArgumentException("Mask must have same length as points.", nameof(mask));

        int count = 0;
        for (int i = 0; i < mask.Length; i++)
            if (mask[i]) count++;

        var pts = new Vec3[count];
        Vec3[]? nrm = Normals != null ? new Vec3[count] : null;
        int j = 0;
        for (int i = 0; i < mask.Length; i++)
        {
            if (mask[i])
            {
                pts[j] = Points[i];
                if (nrm != null)
                    nrm[j] = Normals![i];
                j++;
            }
        }
        return new PointCloud(pts, nrm);
    }

    private static Aabb ComputeBounds(Vec3[] points)
    {
        if (points.Length == 0)
            return Aabb.Empty;

        var bounds = Aabb.FromPoint(points[0]);
        for (int i = 1; i < points.Length; i++)
            bounds = bounds.ExpandToInclude(points[i]);
        return bounds;
    }
}
