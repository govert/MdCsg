using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Applies a <see cref="Transform3"/> to a mesh, producing a new mesh.
/// </summary>
public static class MeshTransform
{
    /// <summary>
    /// Creates a new mesh by transforming all vertex positions of the source mesh.
    /// Extracts triangles, transforms vertices, and rebuilds via <see cref="MeshBuilder"/>.
    /// </summary>
    /// <param name="source">The source mesh.</param>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new mesh with transformed vertices.</returns>
    public static HalfEdgeMesh Apply(HalfEdgeMesh source, Transform3 transform)
    {
        var positions = new List<Vec3>(source.Vertices.Count);
        foreach (var v in source.Vertices)
            positions.Add(transform.TransformPoint(v.Position));

        var triangles = new List<(int I0, int I1, int I2)>(source.Faces.Count);

        // Check if the transform has a negative determinant (mirror/reflection)
        bool flip = transform.Rotation.Determinant < 0;

        foreach (var face in source.Faces)
        {
            var verts = face.GetVertices();
            if (flip)
                triangles.Add((verts[0].Id, verts[2].Id, verts[1].Id));
            else
                triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        var builder = new MeshBuilder(1e-12);
        return builder.Build(positions, triangles);
    }
}
