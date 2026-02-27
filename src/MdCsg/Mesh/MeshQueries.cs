using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Static utility methods for querying mesh properties: normals, area, volume,
/// nearest point, connected components, and boundary loops.
/// </summary>
public static class MeshQueries
{
    /// <summary>
    /// Computes the area-weighted average normal at a vertex.
    /// </summary>
    /// <param name="v">The vertex.</param>
    /// <param name="mesh">The mesh containing the vertex.</param>
    /// <returns>The normalized area-weighted average normal.</returns>
    public static Vec3 VertexNormal(Vertex v, HalfEdgeMesh mesh)
    {
        var sum = Vec3.Zero;
        foreach (var face in mesh.FacesAroundVertex(v))
        {
            sum = sum + face.Normal; // Normal magnitude = twice face area
        }
        double len = sum.Length;
        return len > 0 ? sum / len : Vec3.Zero;
    }

    /// <summary>
    /// Finds the nearest point on a mesh surface to a query point using BVH traversal.
    /// </summary>
    /// <param name="p">The query point.</param>
    /// <param name="mesh">The mesh to query.</param>
    /// <param name="bvh">The BVH built from the mesh.</param>
    /// <returns>The closest point, face index, and distance.</returns>
    public static (Vec3 Point, int FaceIndex, double Distance) NearestPointOnMesh(
        Vec3 p, HalfEdgeMesh mesh, BvhTree bvh)
    {
        if (bvh.NodeCount == 0)
            return (p, -1, double.MaxValue);

        double minDistSq = double.MaxValue;
        Vec3 bestPoint = p;
        int bestFaceIdx = -1;

#if NET
        System.Span<int> stack = stackalloc int[64];
#else
        var stack = new int[64];
#endif
        int top = 0;
        stack[top++] = 0;
        var nodes = bvh.Nodes;

        while (top > 0)
        {
            int nodeIndex = stack[--top];
            ref readonly var node = ref nodes[nodeIndex];

            double boxDistSq = AabbDistanceSq(p, node.Bounds);
            if (boxDistSq >= minDistSq)
                continue;

            if (node.IsLeaf)
            {
                for (int i = 0; i < node.PrimitiveCount; i++)
                {
                    int faceIdx = bvh.GetFaceIndex(node.LeftOrStart + i);
                    var face = bvh.Mesh.Faces[faceIdx];
                    face.GetTrianglePositions(out var ta, out var tb, out var tc);
                    var nearest = GeometryUtil.NearestPointOnTriangle(p, ta, tb, tc);
                    double distSq = Vec3.DistanceSquared(p, nearest);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        bestPoint = nearest;
                        bestFaceIdx = faceIdx;
                    }
                }
            }
            else
            {
                stack[top++] = node.Right;
                stack[top++] = node.LeftOrStart;
            }
        }

        return (bestPoint, bestFaceIdx, System.Math.Sqrt(minDistSq));
    }

    /// <summary>
    /// Computes the total surface area of a triangle mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>The total surface area.</returns>
    public static double SurfaceArea(HalfEdgeMesh mesh)
    {
        double area = 0;
        foreach (var face in mesh.Faces)
        {
            area += face.Normal.Length * 0.5;
        }
        return area;
    }

    /// <summary>
    /// Computes the signed volume of a closed triangle mesh using the divergence theorem.
    /// Sum of dot(a, cross(b, c)) / 6 for each triangle (a, b, c).
    /// </summary>
    /// <param name="mesh">The mesh (should be closed and consistently oriented).</param>
    /// <returns>The signed volume (positive for outward-facing normals).</returns>
    public static double Volume(HalfEdgeMesh mesh)
    {
        double vol = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            vol += Vec3.Dot(a, Vec3.Cross(b, c));
        }
        return vol / 6.0;
    }

    /// <summary>
    /// Finds connected components by face adjacency via twin half-edges.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>A list of face index lists, one per connected component.</returns>
    public static List<List<int>> ConnectedComponents(HalfEdgeMesh mesh)
    {
        var visited = new bool[mesh.Faces.Count];
        var surfaceComponents = new List<List<int>>();
        var queue = new Queue<int>();

        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            if (visited[i]) continue;

            var component = new List<int>();
            queue.Enqueue(i);
            visited[i] = true;

            while (queue.Count > 0)
            {
                int faceIdx = queue.Dequeue();
                component.Add(faceIdx);

                var face = mesh.Faces[faceIdx];
                var start = face.Edge;
                var current = start;
                do
                {
                    if (current.Twin?.Face != null)
                    {
                        int neighborIdx = current.Twin.Face.Id;
                        if (!visited[neighborIdx])
                        {
                            visited[neighborIdx] = true;
                            queue.Enqueue(neighborIdx);
                        }
                    }
                    current = current.Next;
                } while (current != start);
            }

            surfaceComponents.Add(component);
        }

        if (surfaceComponents.Count <= 1)
            return surfaceComponents;

        // Merge nested shells so callers get connected solid components rather
        // than disconnected surface sheets (outer shell + cavity shell).
        int n = surfaceComponents.Count;
        var parent = new int[n];
        for (int i = 0; i < n; i++) parent[i] = i;

        var classifiers = new BvhPointClassifier[n];
        var samplePoints = new Vec3[n];
        var componentVertices = new HashSet<int>[n];
        for (int i = 0; i < n; i++)
        {
            var compMesh = BuildComponentMesh(mesh, surfaceComponents[i]);
            classifiers[i] = new BvhPointClassifier(BvhTree.Build(compMesh));
            samplePoints[i] = BuildInteriorSample(mesh, surfaceComponents[i]);
            componentVertices[i] = CollectComponentVertices(mesh, surfaceComponents[i]);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                bool iInJ = classifiers[j].Classify(samplePoints[i]) == SolidClassification.Inside;
                bool jInI = classifiers[i].Classify(samplePoints[j]) == SolidClassification.Inside;
                if (iInJ || jInI)
                    Union(parent, i, j);
            }
        }

        // Merge components that are geometrically touching but not twin-linked.
        var vertexOwner = new Dictionary<int, int>();
        for (int i = 0; i < n; i++)
        {
            foreach (int vid in componentVertices[i])
            {
                if (vertexOwner.TryGetValue(vid, out int other))
                    Union(parent, i, other);
                else
                    vertexOwner[vid] = i;
            }
        }

        var grouped = new Dictionary<int, List<int>>();
        for (int i = 0; i < n; i++)
        {
            int root = Find(parent, i);
            if (!grouped.TryGetValue(root, out var faces))
            {
                faces = [];
                grouped[root] = faces;
            }
            faces.AddRange(surfaceComponents[i]);
        }

        var merged = grouped.Values.ToList();
        if (merged.Count <= 1)
            return merged;

        var faceCounts = new int[merged.Count];
        var absVolumes = new double[merged.Count];
        int maxFaces = 0;
        double maxVolume = 0.0;
        for (int i = 0; i < merged.Count; i++)
        {
            faceCounts[i] = merged[i].Count;
            absVolumes[i] = System.Math.Abs(ComputeSignedVolume(mesh, merged[i]));
            if (faceCounts[i] > maxFaces) maxFaces = faceCounts[i];
            if (absVolumes[i] > maxVolume) maxVolume = absVolumes[i];
        }

        if (maxFaces == 0 || maxVolume <= 0.0)
            return merged;

        var kept = new List<List<int>>();
        double minFaces = maxFaces * 0.10;
        double minVolume = maxVolume * 0.05;
        for (int i = 0; i < merged.Count; i++)
        {
            if (faceCounts[i] >= minFaces || absVolumes[i] >= minVolume)
                kept.Add(merged[i]);
        }

        return kept.Count > 0 ? kept : merged;
    }

    /// <summary>
    /// Traces boundary loops (sequences of boundary half-edges) in a mesh.
    /// A boundary half-edge has no twin.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>A list of vertex-index loops for each boundary.</returns>
    public static List<List<int>> BoundaryLoops(HalfEdgeMesh mesh)
    {
        var visitedHe = new HashSet<int>();
        var loops = new List<List<int>>();

        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null || visitedHe.Contains(he.Id))
                continue;

            var loop = new List<int>();
            var current = he;
            do
            {
                visitedHe.Add(current.Id);
                loop.Add(current.Origin.Id);

                // Find the next boundary half-edge: walk around target vertex
                var next = current.Next;
                while (next.Twin != null)
                {
                    next = next.Twin.Next;
                }
                current = next;
            } while (current != he);

            loops.Add(loop);
        }

        return loops;
    }

    private static double AabbDistanceSq(Vec3 point, Aabb box)
    {
        double dx = System.Math.Max(box.Min.X - point.X, System.Math.Max(0, point.X - box.Max.X));
        double dy = System.Math.Max(box.Min.Y - point.Y, System.Math.Max(0, point.Y - box.Max.Y));
        double dz = System.Math.Max(box.Min.Z - point.Z, System.Math.Max(0, point.Z - box.Max.Z));
        return dx * dx + dy * dy + dz * dz;
    }

    private static HalfEdgeMesh BuildComponentMesh(HalfEdgeMesh source, IReadOnlyList<int> componentFaces)
    {
        var positions = new List<Vec3>(source.Vertices.Count);
        foreach (var v in source.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int I0, int I1, int I2)>(componentFaces.Count);
        foreach (int faceIdx in componentFaces)
        {
            var verts = source.Faces[faceIdx].GetVertices();
            triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        return new MeshBuilder(0.0).Build(positions, triangles);
    }

    private static Vec3 BuildInteriorSample(HalfEdgeMesh source, IReadOnlyList<int> componentFaces)
    {
        int firstFaceIdx = componentFaces[0];
        var face = source.Faces[firstFaceIdx];
        face.GetTrianglePositions(out var a, out var b, out var c);
        Vec3 centroid = (a + b + c) / 3.0;
        Vec3 normal = face.Normal.Normalized;

        double signed = 0.0;
        foreach (int faceIdx in componentFaces)
        {
            var f = source.Faces[faceIdx];
            f.GetTrianglePositions(out var fa, out var fb, out var fc);
            signed += Vec3.Dot(fa, Vec3.Cross(fb, fc));
        }
        signed /= 6.0;

        const double eps = 1e-6;
        return signed >= 0.0
            ? centroid - normal * eps
            : centroid + normal * eps;
    }

    private static int Find(int[] parent, int x)
    {
        while (parent[x] != x)
        {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }

    private static void Union(int[] parent, int a, int b)
    {
        int ra = Find(parent, a);
        int rb = Find(parent, b);
        if (ra != rb) parent[rb] = ra;
    }

    private static HashSet<int> CollectComponentVertices(HalfEdgeMesh source, IReadOnlyList<int> componentFaces)
    {
        var vertices = new HashSet<int>();
        foreach (int faceIdx in componentFaces)
        {
            var v = source.Faces[faceIdx].GetVertices();
            vertices.Add(v[0].Id);
            vertices.Add(v[1].Id);
            vertices.Add(v[2].Id);
        }
        return vertices;
    }

    private static double ComputeSignedVolume(HalfEdgeMesh source, IReadOnlyList<int> componentFaces)
    {
        double vol6 = 0.0;
        foreach (int faceIdx in componentFaces)
        {
            var f = source.Faces[faceIdx];
            f.GetTrianglePositions(out var a, out var b, out var c);
            vol6 += Vec3.Dot(a, Vec3.Cross(b, c));
        }
        return vol6 / 6.0;
    }
}
