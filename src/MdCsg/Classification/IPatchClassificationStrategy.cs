using MdCsg.Bvh;
using MdCsg.Cutting;
using MdCsg.Patches;

namespace MdCsg.Classification;

/// <summary>
/// Strategy interface for classifying patches as inside or outside the other solid.
/// Allows swapping CPU and GPU implementations.
/// </summary>
public interface IPatchClassificationStrategy
{
    /// <summary>
    /// Classifies all patches of a cut mesh against the other solid's BVH.
    /// </summary>
    /// <param name="patches">Patches to classify.</param>
    /// <param name="subTriangles">All sub-triangles from the cut mesh.</param>
    /// <param name="otherBvh">BVH of the other solid.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting.</param>
    /// <returns>Number of degenerate patches that could not be confidently classified.</returns>
    int ClassifyAll(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber);
}
