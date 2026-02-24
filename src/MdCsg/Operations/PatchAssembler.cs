using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Operations;

/// <summary>
/// Selects which patches to keep based on the CSG operation and their classification.
/// </summary>
public static class PatchAssembler
{
    /// <summary>
    /// Result of patch assembly: selected sub-triangles with normal direction info.
    /// </summary>
    /// <param name="Triangles">Selected triangles.</param>
    /// <param name="FlipNormals">Whether normals should be flipped for each triangle.</param>
    public record AssemblyResult(List<Triangle3> Triangles, List<bool> FlipNormals);

    /// <summary>
    /// Selects patches from both meshes based on the CSG operation.
    /// </summary>
    /// <param name="patchesA">Patches from mesh A.</param>
    /// <param name="patchesB">Patches from mesh B.</param>
    /// <param name="subTrianglesA">Sub-triangles from mesh A.</param>
    /// <param name="subTrianglesB">Sub-triangles from mesh B.</param>
    /// <param name="operation">The CSG operation.</param>
    public static AssemblyResult Assemble(
        IReadOnlyList<Patch> patchesA,
        IReadOnlyList<Patch> patchesB,
        IReadOnlyList<FaceCutter.SubTriangle> subTrianglesA,
        IReadOnlyList<FaceCutter.SubTriangle> subTrianglesB,
        CsgOperation operation)
    {
        var triangles = new List<Triangle3>();
        var flipNormals = new List<bool>();

        // Select patches from mesh A
        foreach (var patch in patchesA)
        {
            bool keep = ShouldKeepPatchFromA(patch.IsInside ?? false, operation);
            if (!keep) continue;

            foreach (int triIdx in patch.SubTriangleIndices)
            {
                var st = subTrianglesA[triIdx];
                triangles.Add(new Triangle3(st.A, st.B, st.C));
                flipNormals.Add(false); // A's normals stay as-is
            }
        }

        // Select patches from mesh B
        foreach (var patch in patchesB)
        {
            bool keep = ShouldKeepPatchFromB(patch.IsInside ?? false, operation);
            bool flip = ShouldFlipNormalsFromB(patch.IsInside ?? false, operation);
            if (!keep) continue;

            foreach (int triIdx in patch.SubTriangleIndices)
            {
                var st = subTrianglesB[triIdx];
                if (flip)
                    triangles.Add(new Triangle3(st.A, st.C, st.B)); // reversed winding = flipped normal
                else
                    triangles.Add(new Triangle3(st.A, st.B, st.C));
                flipNormals.Add(flip);
            }
        }

        return new AssemblyResult(triangles, flipNormals);
    }

    /// <summary>
    /// For mesh A patches: determines if a patch should be kept.
    /// </summary>
    private static bool ShouldKeepPatchFromA(bool isInsideB, CsgOperation operation) => operation switch
    {
        // Union: keep parts of A that are outside B
        CsgOperation.Union => !isInsideB,
        // Intersection: keep parts of A that are inside B
        CsgOperation.Intersection => isInsideB,
        // Difference: keep parts of A that are outside B
        CsgOperation.Difference => !isInsideB,
        _ => false
    };

    /// <summary>
    /// For mesh B patches: determines if a patch should be kept.
    /// </summary>
    private static bool ShouldKeepPatchFromB(bool isInsideA, CsgOperation operation) => operation switch
    {
        // Union: keep parts of B that are outside A
        CsgOperation.Union => !isInsideA,
        // Intersection: keep parts of B that are inside A
        CsgOperation.Intersection => isInsideA,
        // Difference: keep parts of B that are inside A (with flipped normals)
        CsgOperation.Difference => isInsideA,
        _ => false
    };

    /// <summary>
    /// For mesh B patches: determines if normals should be flipped.
    /// </summary>
    private static bool ShouldFlipNormalsFromB(bool isInsideA, CsgOperation operation) => operation switch
    {
        // Difference: B's inside patches get flipped normals
        CsgOperation.Difference => isInsideA,
        _ => false
    };
}
