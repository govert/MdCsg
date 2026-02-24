namespace MdCsg.Math;

/// <summary>
/// Axis-aligned bounding box.
/// </summary>
/// <param name="Min">The minimum corner.</param>
/// <param name="Max">The maximum corner.</param>
public readonly record struct Aabb(Vec3 Min, Vec3 Max)
{
    /// <summary>An invalid (empty) bounding box.</summary>
    public static readonly Aabb Empty = new(
        new Vec3(double.MaxValue, double.MaxValue, double.MaxValue),
        new Vec3(double.MinValue, double.MinValue, double.MinValue));

    /// <summary>Returns the size of the box along each axis.</summary>
    public Vec3 Size => Max - Min;

    /// <summary>Returns the center of the box.</summary>
    public Vec3 Center => (Min + Max) * 0.5;

    /// <summary>Returns the surface area of the box.</summary>
    public double SurfaceArea
    {
        get
        {
            var s = Size;
            return 2.0 * (s.X * s.Y + s.Y * s.Z + s.Z * s.X);
        }
    }

    /// <summary>Tests whether this box overlaps another.</summary>
    public bool Overlaps(Aabb other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Tests whether this box contains a point.</summary>
    public bool Contains(Vec3 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;

    /// <summary>Returns the union of two bounding boxes.</summary>
    public static Aabb Union(Aabb a, Aabb b) =>
        new(Vec3.Min(a.Min, b.Min), Vec3.Max(a.Max, b.Max));

    /// <summary>Grows this box to include a point.</summary>
    public Aabb ExpandToInclude(Vec3 point) =>
        new(Vec3.Min(Min, point), Vec3.Max(Max, point));

    /// <summary>Creates an AABB enclosing a triangle.</summary>
    public static Aabb FromTriangle(Vec3 a, Vec3 b, Vec3 c) =>
        new(Vec3.Min(Vec3.Min(a, b), c), Vec3.Max(Vec3.Max(a, b), c));

    /// <summary>Creates an AABB from a single point.</summary>
    public static Aabb FromPoint(Vec3 point) => new(point, point);

    /// <summary>Returns the box expanded by a uniform margin.</summary>
    public Aabb Expand(double margin)
    {
        var m = new Vec3(margin, margin, margin);
        return new Aabb(Min - m, Max + m);
    }
}
