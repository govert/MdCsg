using System.Threading;
using MdCsg.Bvh;
using MdCsg.Cutting;
using MdCsg.Patches;

namespace MdCsg.Classification;

/// <summary>
/// Classifies all patches of a cut mesh as inside or outside the other solid.
/// For each patch, selects the confident point (maximum margin) and classifies it.
/// </summary>
public static class PatchClassifier
{
    /// <summary>
    /// The minimum margin below which a patch is flagged as degenerate.
    /// </summary>
    public const double DegenerateMarginThreshold = 1e-10;

    /// <summary>
    /// Classifies all patches using an <see cref="IPointClassifier"/> for the other solid.
    /// </summary>
    /// <param name="patches">Patches to classify.</param>
    /// <param name="subTriangles">All sub-triangles.</param>
    /// <param name="classifier">Classifier for the other solid (BVH-backed or plane-backed).</param>
    /// <returns>Number of degenerate patches that could not be confidently classified.</returns>
    public static int ClassifyAll(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        IPointClassifier classifier)
    {
        int degenerateCount = 0;

        foreach (var patch in patches)
        {
            var (confidentPoint, margin) = ConfidentPoint.FindConfidentPoint(patch, subTriangles, classifier);
            patch.ConfidentPoint = confidentPoint;
            patch.HasConfidentPoint = margin > DegenerateMarginThreshold;

            if (!patch.HasConfidentPoint)
                degenerateCount++;

            patch.IsInside = classifier.Classify(confidentPoint) == SolidClassification.Inside;
        }

        return degenerateCount;
    }

    /// <summary>
    /// Classifies all patches, optionally in parallel.
    /// </summary>
    /// <param name="patches">Patches to classify.</param>
    /// <param name="subTriangles">All sub-triangles.</param>
    /// <param name="otherBvh">BVH of the other solid.</param>
    /// <param name="useWindingNumber">If true, use winding number instead of ray casting.</param>
    /// <param name="parallel">If true, classify patches in parallel.</param>
    /// <returns>Number of degenerate patches that could not be confidently classified.</returns>
    public static int ClassifyAll(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber = false,
        bool parallel = false)
    {
        if (parallel && patches.Count > 1)
            return ClassifyAllParallel(patches, subTriangles, otherBvh, useWindingNumber);

        return ClassifyAllSequential(patches, subTriangles, otherBvh, useWindingNumber);
    }

    private static int ClassifyAllSequential(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber)
    {
        int degenerateCount = 0;

        foreach (var patch in patches)
        {
            ClassifyPatch(patch, subTriangles, otherBvh, useWindingNumber);
            if (!patch.HasConfidentPoint)
                degenerateCount++;
        }

        return degenerateCount;
    }

    private static int ClassifyAllParallel(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber)
    {
        int degenerateCount = 0;

        System.Threading.Tasks.Parallel.For(0, patches.Count, () => 0, (i, _, localDeg) =>
        {
            var patch = patches[i];
            ClassifyPatch(patch, subTriangles, otherBvh, useWindingNumber);
            if (!patch.HasConfidentPoint)
                localDeg++;
            return localDeg;
        }, localDeg => Interlocked.Add(ref degenerateCount, localDeg));

        return degenerateCount;
    }

    private static void ClassifyPatch(
        Patch patch,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber)
    {
        var (confidentPoint, margin) = ConfidentPoint.FindConfidentPoint(patch, subTriangles, otherBvh);
        patch.ConfidentPoint = confidentPoint;
        patch.HasConfidentPoint = margin > DegenerateMarginThreshold;

        if (!patch.HasConfidentPoint)
        {
            // Use exact Rational ray casting for degenerate patches
            var exactClassification = ExactRayCastClassifier.Classify(confidentPoint, otherBvh);
            patch.IsInside = exactClassification == SolidClassification.Inside;
            return;
        }

        var classification = useWindingNumber
            ? WindingNumberClassifier.Classify(confidentPoint, otherBvh)
            : RayCastClassifier.Classify(confidentPoint, otherBvh);

        patch.IsInside = classification == SolidClassification.Inside;
    }
}

/// <summary>
/// CPU implementation of patch classification using the existing static logic.
/// </summary>
public class CpuPatchClassificationStrategy : IPatchClassificationStrategy
{
    private readonly bool _parallel;

    /// <summary>
    /// Creates a CPU patch classification strategy.
    /// </summary>
    /// <param name="parallel">If true, classify patches in parallel.</param>
    public CpuPatchClassificationStrategy(bool parallel = false)
    {
        _parallel = parallel;
    }

    /// <inheritdoc />
    public int ClassifyAll(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber)
    {
        return PatchClassifier.ClassifyAll(patches, subTriangles, otherBvh, useWindingNumber, _parallel);
    }
}
