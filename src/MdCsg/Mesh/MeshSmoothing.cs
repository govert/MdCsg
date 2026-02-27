using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Laplacian mesh smoothing.
/// </summary>
public static class MeshSmoothing
{
    /// <summary>
    /// Applies Laplacian smoothing by blending each vertex toward the average
    /// of its neighbors. Rebuilds the mesh after smoothing.
    /// </summary>
    /// <param name="mesh">The input mesh.</param>
    /// <param name="iterations">Number of smoothing passes.</param>
    /// <param name="lambda">Blending factor in [0, 1]. 0 = no change, 1 = move fully to average.</param>
    /// <returns>A new smoothed mesh.</returns>
    public static HalfEdgeMesh LaplacianSmooth(HalfEdgeMesh mesh, int iterations = 1, double lambda = 0.5)
    {
        if (iterations <= 0)
            return CloneMesh(mesh);

        lambda = System.Math.Max(0.0, System.Math.Min(1.0, lambda));

        // Build adjacency: for each vertex, collect neighbor vertex ids
        var adjacency = BuildAdjacency(mesh);

        // Work on mutable positions
        var positions = new Vec3[mesh.Vertices.Count];
        for (int i = 0; i < mesh.Vertices.Count; i++)
            positions[i] = mesh.Vertices[i].Position;

        for (int iter = 0; iter < iterations; iter++)
        {
            var newPositions = new Vec3[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                var neighbors = adjacency[i];
                if (neighbors.Count == 0)
                {
                    newPositions[i] = positions[i];
                    continue;
                }

                var avg = Vec3.Zero;
                foreach (int neighborId in neighbors)
                    avg = avg + positions[neighborId];
                avg = avg / neighbors.Count;

                newPositions[i] = Vec3.Lerp(positions[i], avg, lambda);
            }
            positions = newPositions;
        }

        // Basic volume compensation to counter strong Laplacian shrinkage.
        // This keeps smoothing useful while preserving solid-scale behavior.
        double originalAbsVolume = System.Math.Abs(MeshQueries.Volume(mesh));
        if (originalAbsVolume > 1e-18)
        {
            double smoothedAbsVolume = System.Math.Abs(ComputeVolume(positions, mesh));
            if (smoothedAbsVolume > 1e-18)
            {
                double targetScale = System.Math.Pow(originalAbsVolume / smoothedAbsVolume, 1.0 / 3.0);
                double correctionStrength = 0.7 / System.Math.Sqrt(System.Math.Max(1, iterations));
                correctionStrength = System.Math.Max(0.0, System.Math.Min(1.0, correctionStrength));
                double scale = 1.0 + (targetScale - 1.0) * correctionStrength;
                Vec3 center = ComputeCentroid(positions);
                for (int i = 0; i < positions.Length; i++)
                {
                    var delta = positions[i] - center;
                    positions[i] = center + delta * scale;
                }
            }
        }

        // Rebuild mesh with smoothed positions
        var positionList = new List<Vec3>(positions);
        var triangles = new List<(int I0, int I1, int I2)>(mesh.Faces.Count);
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        var builder = new MeshBuilder(1e-12);
        return builder.Build(positionList, triangles);
    }

    private static double ComputeVolume(IReadOnlyList<Vec3> positions, HalfEdgeMesh mesh)
    {
        double vol6 = 0.0;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            var a = positions[verts[0].Id];
            var b = positions[verts[1].Id];
            var c = positions[verts[2].Id];
            vol6 += Vec3.Dot(a, Vec3.Cross(b, c));
        }
        return vol6 / 6.0;
    }

    private static Vec3 ComputeCentroid(IReadOnlyList<Vec3> positions)
    {
        if (positions.Count == 0)
            return Vec3.Zero;

        var sum = Vec3.Zero;
        for (int i = 0; i < positions.Count; i++)
            sum += positions[i];
        return sum / positions.Count;
    }

    private static List<HashSet<int>> BuildAdjacency(HalfEdgeMesh mesh)
    {
        var adj = new List<HashSet<int>>(mesh.Vertices.Count);
        for (int i = 0; i < mesh.Vertices.Count; i++)
            adj.Add(new HashSet<int>());

        foreach (var he in mesh.HalfEdges)
        {
            int origin = he.Origin.Id;
            int target = he.Target.Id;
            adj[origin].Add(target);
            adj[target].Add(origin);
        }

        return adj;
    }

    private static HalfEdgeMesh CloneMesh(HalfEdgeMesh mesh)
    {
        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int I0, int I1, int I2)>(mesh.Faces.Count);
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        var builder = new MeshBuilder(1e-12);
        return builder.Build(positions, triangles);
    }
}
