using MdCsg.Math;

namespace MdCsg.Intersection;

/// <summary>
/// Snap rounding for intersection points to ensure topological consistency.
/// </summary>
public static class SnapRounding
{
    /// <summary>
    /// Snaps an intersection point to the nearest grid point.
    /// </summary>
    public static Vec3 Snap(Vec3 point, double gridSize = MathUtil.DefaultGridSize) =>
        point.SnapToGrid(gridSize);

    /// <summary>
    /// Snaps an intersection segment's endpoints and merges near-coincident points.
    /// </summary>
    public static IntersectionSegment SnapSegment(IntersectionSegment seg, double gridSize = MathUtil.DefaultGridSize)
    {
        var start = Snap(seg.Start, gridSize);
        var end = Snap(seg.End, gridSize);
        return new IntersectionSegment(start, end, seg.FaceIndexA, seg.FaceIndexB);
    }

    /// <summary>
    /// Merges near-coincident points in a list, returning indices to canonical points.
    /// </summary>
    public static (List<Vec3> UniquePoints, Dictionary<int, int> OriginalToUnique) MergePoints(
        IReadOnlyList<Vec3> points, double tolerance = MathUtil.DefaultGridSize)
    {
        var unique = new List<Vec3>();
        var mapping = new Dictionary<int, int>();
        var toleranceSq = tolerance * tolerance;

        for (int i = 0; i < points.Count; i++)
        {
            int found = -1;
            for (int j = 0; j < unique.Count; j++)
            {
                if (Vec3.DistanceSquared(points[i], unique[j]) < toleranceSq)
                {
                    found = j;
                    break;
                }
            }
            if (found >= 0)
            {
                mapping[i] = found;
            }
            else
            {
                mapping[i] = unique.Count;
                unique.Add(points[i]);
            }
        }

        return (unique, mapping);
    }
}
