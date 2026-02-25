using MdCsg.Bvh;
using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies points against a mesh-backed solid using its BVH.
/// Delegates to <see cref="RayCastClassifier"/> or <see cref="WindingNumberClassifier"/>
/// for inside/outside, and uses BVH traversal for distance-to-surface.
/// </summary>
public class BvhPointClassifier : IPointClassifier
{
    private readonly BvhTree _bvh;
    private readonly bool _useWindingNumber;

    /// <summary>
    /// Creates a classifier backed by a BVH tree.
    /// </summary>
    /// <param name="bvh">The BVH of the other solid's mesh.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting.</param>
    public BvhPointClassifier(BvhTree bvh, bool useWindingNumber = false)
    {
        _bvh = bvh;
        _useWindingNumber = useWindingNumber;
    }

    /// <inheritdoc />
    public SolidClassification Classify(Vec3 point) =>
        _useWindingNumber
            ? WindingNumberClassifier.Classify(point, _bvh)
            : RayCastClassifier.Classify(point, _bvh);

    /// <inheritdoc />
    public double DistanceToSurface(Vec3 point) =>
        ConfidentPoint.ComputeMinDistanceToMesh(point, _bvh);
}
