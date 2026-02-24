namespace MdCsg.Math;

/// <summary>
/// A triangle in 3D space defined by three vertices.
/// </summary>
/// <param name="A">First vertex.</param>
/// <param name="B">Second vertex.</param>
/// <param name="C">Third vertex.</param>
public readonly record struct Triangle3(Vec3 A, Vec3 B, Vec3 C)
{
    /// <summary>Returns the (unnormalized) normal via cross product.</summary>
    public Vec3 Normal => Vec3.Cross(B - A, C - A);

    /// <summary>Returns the unit normal.</summary>
    public Vec3 UnitNormal => Normal.Normalized;

    /// <summary>Returns the plane containing this triangle.</summary>
    public Plane Plane => Plane.FromPoints(A, B, C);

    /// <summary>Returns the centroid (average of vertices).</summary>
    public Vec3 Centroid => (A + B + C) / 3.0;

    /// <summary>Returns the axis-aligned bounding box.</summary>
    public Aabb Bounds => Aabb.FromTriangle(A, B, C);

    /// <summary>Returns twice the area (magnitude of the cross product).</summary>
    public double DoubleArea => Normal.Length;

    /// <summary>Returns the area of the triangle.</summary>
    public double Area => DoubleArea * 0.5;

    /// <summary>Returns the vertex at the given index (0=A, 1=B, 2=C).</summary>
    public Vec3 this[int index] => index switch
    {
        0 => A,
        1 => B,
        2 => C,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}
