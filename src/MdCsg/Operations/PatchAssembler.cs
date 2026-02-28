using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Operations;

/// <summary>
/// Selects which patches to keep based on the CSG operation and their classification.
/// </summary>
public static class PatchAssembler
{
    private readonly record struct CoplanarPatchDecision(bool Keep, bool FlipNormals);

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
            bool keep;
            bool flip = false;
            if (patch.CoplanarNormalsAgree.HasValue && !patch.HasConfidentPoint)
            {
                var decision = ResolveCoplanarPatchDecision(
                    fromMeshA: true,
                    patch.CoplanarNormalsAgree.Value,
                    operation);
                keep = decision.Keep;
                flip = decision.FlipNormals;
            }
            else
            {
                keep = ShouldKeepPatchFromA(patch.IsInside ?? false, operation);
            }
            if (!keep) continue;

            foreach (int triIdx in patch.SubTriangleIndices)
            {
                var st = subTrianglesA[triIdx];
                if (flip)
                    triangles.Add(new Triangle3(st.A, st.C, st.B));
                else
                    triangles.Add(new Triangle3(st.A, st.B, st.C));
                flipNormals.Add(flip);
            }
        }

        // Select patches from mesh B
        foreach (var patch in patchesB)
        {
            if (patch.CoplanarNormalsAgree.HasValue && !patch.HasConfidentPoint)
            {
                var decision = ResolveCoplanarPatchDecision(
                    fromMeshA: false,
                    patch.CoplanarNormalsAgree.Value,
                    operation);
                if (!decision.Keep)
                    continue;

                foreach (int triIdx in patch.SubTriangleIndices)
                {
                    var st = subTrianglesB[triIdx];
                    if (decision.FlipNormals)
                        triangles.Add(new Triangle3(st.A, st.C, st.B));
                    else
                        triangles.Add(new Triangle3(st.A, st.B, st.C));
                    flipNormals.Add(decision.FlipNormals);
                }

                continue;
            }

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

    /// <summary>
    /// Resolves coplanar patch inclusion for each source mesh and operation.
    /// The policy is explicit and deterministic; mesh A remains authoritative when
    /// both surfaces are coplanar and ambiguous.
    /// </summary>
    private static CoplanarPatchDecision ResolveCoplanarPatchDecision(
        bool fromMeshA,
        bool normalsAgree,
        CsgOperation operation)
    {
        if (fromMeshA)
        {
            return operation switch
            {
                CsgOperation.Union => normalsAgree
                    ? new CoplanarPatchDecision(Keep: true, FlipNormals: false)
                    : new CoplanarPatchDecision(Keep: false, FlipNormals: false),
                CsgOperation.Intersection => normalsAgree
                    ? new CoplanarPatchDecision(Keep: true, FlipNormals: false)
                    : new CoplanarPatchDecision(Keep: false, FlipNormals: false),
                CsgOperation.Difference => normalsAgree
                    ? new CoplanarPatchDecision(Keep: false, FlipNormals: false)
                    : new CoplanarPatchDecision(Keep: true, FlipNormals: false),
                _ => new CoplanarPatchDecision(Keep: false, FlipNormals: false)
            };
        }

        // Mesh B coplanar policy: A-side selection remains authoritative for
        // unresolved coplanar regions so we do not duplicate shared surfaces.
        return new CoplanarPatchDecision(Keep: false, FlipNormals: false);
    }
}
