using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.TestHelpers;

/// <summary>
/// Computes the signed volume of a closed mesh using the divergence theorem.
/// </summary>
public static class VolumeCalculator
{
    /// <summary>
    /// Computes the signed volume of a closed mesh.
    /// The mesh must be consistently oriented with outward-facing normals.
    /// </summary>
    public static double ComputeVolume(HalfEdgeMesh mesh)
    {
        double volume = 0;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            if (verts.Count < 3) continue;

            var a = verts[0].Position;
            var b = verts[1].Position;
            var c = verts[2].Position;

            // Signed volume of tetrahedron formed by triangle and origin
            volume += Vec3.Dot(a, Vec3.Cross(b, c)) / 6.0;
        }
        return volume;
    }

    /// <summary>
    /// Computes the absolute volume (always positive).
    /// </summary>
    public static double ComputeAbsoluteVolume(HalfEdgeMesh mesh) =>
        System.Math.Abs(ComputeVolume(mesh));
}
