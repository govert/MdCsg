using System;
using System.Collections.Generic;
using MdCsg.Cutting;

namespace MdCsg.Patches;

internal static class PatchIdentityAssigner
{
    public static void Assign(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        PatchBoundaryAuthority boundaryAuthority)
    {
        foreach (var patch in patches)
        {
            patch.BoundaryAuthority = boundaryAuthority;
            patch.SourceFaceIndices = BuildSourceFaceIndices(patch, subTriangles);
            patch.StableId = ComputeStableId(patch, boundaryAuthority);
        }
    }

    private static IReadOnlyList<int> BuildSourceFaceIndices(
        Patch patch,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles)
    {
        var sourceFaces = new HashSet<int>();
        foreach (int triIdx in patch.SubTriangleIndices)
            sourceFaces.Add(subTriangles[triIdx].OriginalFaceIndex);

        var sorted = sourceFaces.ToArray();
        Array.Sort(sorted);
        return sorted;
    }

    private static ulong ComputeStableId(Patch patch, PatchBoundaryAuthority boundaryAuthority)
    {
        // FNV-1a 64-bit over deterministic extraction-local provenance.
        const ulong fnvOffset = 1469598103934665603UL;
        const ulong fnvPrime = 1099511628211UL;
        ulong hash = fnvOffset;

        AddInt((int)boundaryAuthority);
        foreach (int face in patch.SourceFaceIndices)
            AddInt(face);

        var tris = patch.SubTriangleIndices.ToArray();
        Array.Sort(tris);
        foreach (int tri in tris)
            AddInt(tri);

        return hash;

        void AddInt(int value)
        {
            unchecked
            {
                uint data = (uint)value;
                hash ^= data & 0xFFu;
                hash *= fnvPrime;
                hash ^= (data >> 8) & 0xFFu;
                hash *= fnvPrime;
                hash ^= (data >> 16) & 0xFFu;
                hash *= fnvPrime;
                hash ^= (data >> 24) & 0xFFu;
                hash *= fnvPrime;
            }
        }
    }
}
