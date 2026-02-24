using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Cutting;

/// <summary>
/// Cuts an entire mesh along an intersection graph, producing a new mesh with sub-triangles.
/// </summary>
public static class MeshCutter
{
    /// <summary>
    /// Result of cutting a mesh: new mesh + mapping from new faces to original faces.
    /// </summary>
    /// <param name="Mesh">The cut mesh with sub-triangles.</param>
    /// <param name="SubTriangles">All sub-triangles with original face indices and intersection edge flags.</param>
    public record CutResult(HalfEdgeMesh Mesh, IReadOnlyList<FaceCutter.SubTriangle> SubTriangles);

    /// <summary>
    /// Cuts a mesh along the intersection segments, producing sub-triangles.
    /// </summary>
    /// <param name="mesh">The source mesh.</param>
    /// <param name="faceSegments">Map from face index to intersection segments that cross it.</param>
    /// <returns>The cut mesh with sub-triangle information.</returns>
    public static CutResult Cut(HalfEdgeMesh mesh, IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments)
    {
        var allSubTriangles = new List<FaceCutter.SubTriangle>();

        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var tri = new Triangle3(va, vb, vc);

            if (faceSegments.TryGetValue(faceIdx, out var segments) && segments.Count > 0)
            {
                var subTris = FaceCutter.CutFace(tri, faceIdx, segments);
                allSubTriangles.AddRange(subTris);
            }
            else
            {
                allSubTriangles.Add(new FaceCutter.SubTriangle(tri.A, tri.B, tri.C, faceIdx, false));
            }
        }

        // Build new mesh from sub-triangles
        var triangles = new Triangle3[allSubTriangles.Count];
        for (int i = 0; i < allSubTriangles.Count; i++)
        {
            var st = allSubTriangles[i];
            triangles[i] = new Triangle3(st.A, st.B, st.C);
        }

        var builder = new MeshBuilder();
        var newMesh = builder.Build(triangles);

        // Transfer original face IDs
        for (int i = 0; i < System.Math.Min(newMesh.Faces.Count, allSubTriangles.Count); i++)
        {
            newMesh.Faces[i].OriginalFaceId = allSubTriangles[i].OriginalFaceIndex;
        }

        return new CutResult(newMesh, allSubTriangles);
    }
}
