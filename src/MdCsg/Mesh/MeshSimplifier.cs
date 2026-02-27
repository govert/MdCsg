using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Mesh decimation using the Garland-Heckbert Quadric Error Metric (QEM) algorithm.
/// </summary>
public static class MeshSimplifier
{
    /// <summary>
    /// Simplifies a mesh to a target face count using QEM edge collapse.
    /// </summary>
    /// <param name="mesh">The input mesh.</param>
    /// <param name="targetFaceCount">The desired number of faces.</param>
    /// <returns>A new simplified mesh.</returns>
    public static HalfEdgeMesh Simplify(HalfEdgeMesh mesh, int targetFaceCount)
    {
        return SimplifyInternal(mesh, targetFaceCount, double.MaxValue);
    }

    /// <summary>
    /// Simplifies a mesh until the next collapse would exceed the given error threshold.
    /// </summary>
    /// <param name="mesh">The input mesh.</param>
    /// <param name="errorThreshold">Maximum allowed QEM error for a collapse.</param>
    /// <returns>A new simplified mesh.</returns>
    public static HalfEdgeMesh Simplify(HalfEdgeMesh mesh, double errorThreshold)
    {
        return SimplifyInternal(mesh, 4, errorThreshold); // At least 4 faces (tetrahedron)
    }

    private static HalfEdgeMesh SimplifyInternal(HalfEdgeMesh mesh, int targetFaceCount, double errorThreshold)
    {
        // Extract triangles
        int vertCount = mesh.Vertices.Count;
        int faceCount = mesh.Faces.Count;
        double originalAbsVolume = System.Math.Abs(MeshQueries.Volume(mesh));

        var positions = new Vec3[vertCount];
        for (int i = 0; i < vertCount; i++)
            positions[i] = mesh.Vertices[i].Position;
        Vec3 originalCentroid = ComputeCentroid(positions);

        var triangles = new List<int[]>(faceCount);
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            triangles.Add(new[] { verts[0].Id, verts[1].Id, verts[2].Id });
        }

        // Compute quadrics per vertex (10 doubles each representing symmetric 4x4)
        var quadrics = new double[vertCount][];
        for (int i = 0; i < vertCount; i++)
            quadrics[i] = new double[10];

        foreach (var tri in triangles)
        {
            var a = positions[tri[0]];
            var b = positions[tri[1]];
            var c = positions[tri[2]];
            var n = Vec3.Cross(b - a, c - a);
            double len = n.Length;
            if (len < 1e-30) continue;
            n = n / len;
            double d = -Vec3.Dot(n, a);
            // Quadric Q = pp^T where p = (nx, ny, nz, d)
            double[] q = ComputePlaneQuadric(n.X, n.Y, n.Z, d);
            for (int k = 0; k < 3; k++)
                AddQuadric(quadrics[tri[k]], q);
        }

        // Build edge set and priority queue
        var edgeSet = new HashSet<long>();
        var heap = new MinHeap();
        var alive = new bool[vertCount];
        var triAlive = new bool[faceCount];
        for (int i = 0; i < vertCount; i++) alive[i] = true;
        for (int i = 0; i < faceCount; i++) triAlive[i] = true;

        // Vertex -> merged target mapping (union-find)
        var parent = new int[vertCount];
        for (int i = 0; i < vertCount; i++) parent[i] = i;

        for (int ti = 0; ti < triangles.Count; ti++)
        {
            var tri = triangles[ti];
            for (int e = 0; e < 3; e++)
            {
                int va = tri[e], vb = tri[(e + 1) % 3];
                long key = EdgeKey(va, vb);
                if (edgeSet.Add(key))
                {
                    double cost = ComputeEdgeCost(quadrics, positions, va, vb, out _);
                    heap.Insert(cost, va, vb);
                }
            }
        }

        int currentFaceCount = faceCount;

        while (currentFaceCount > targetFaceCount && heap.Count > 0)
        {
            var (cost, eA, eB) = heap.ExtractMin();
            if (cost > errorThreshold) break;

            // Resolve to current representatives
            int a = Find(parent, eA);
            int b = Find(parent, eB);
            if (a == b || !alive[a] || !alive[b]) continue;
            if (!IsCollapseTopologicallySafe(triangles, triAlive, parent, a, b))
                continue;

            // Recompute cost with current quadrics
            double actualCost = ComputeEdgeCost(quadrics, positions, a, b, out Vec3 optimalPos);
            if (actualCost > errorThreshold) break;

            // Merge b into a
            positions[a] = optimalPos;
            AddQuadric(quadrics[a], quadrics[b]);
            parent[b] = a;
            alive[b] = false;

            // Kill triangles where a and b both appear
            for (int ti = 0; ti < triangles.Count; ti++)
            {
                if (!triAlive[ti]) continue;
                var tri = triangles[ti];
                // Resolve all verts
                tri[0] = Find(parent, tri[0]);
                tri[1] = Find(parent, tri[1]);
                tri[2] = Find(parent, tri[2]);
                if (tri[0] == tri[1] || tri[1] == tri[2] || tri[0] == tri[2])
                {
                    triAlive[ti] = false;
                    currentFaceCount--;
                }
            }

            // Re-insert edges from a's neighborhood
            for (int ti = 0; ti < triangles.Count; ti++)
            {
                if (!triAlive[ti]) continue;
                var tri = triangles[ti];
                for (int e = 0; e < 3; e++)
                {
                    if (tri[e] == a || tri[(e + 1) % 3] == a)
                    {
                        int va = tri[e], vb = tri[(e + 1) % 3];
                        if (va != vb)
                        {
                            double c2 = ComputeEdgeCost(quadrics, positions, va, vb, out _);
                            heap.Insert(c2, va, vb);
                        }
                    }
                }
            }
        }

        // Rebuild mesh from surviving triangles
        // Remap vertex IDs
        var idMap = new int[vertCount];
        var newPositions = new List<Vec3>();
        for (int i = 0; i < vertCount; i++) idMap[i] = -1;

        var newTriangles = new List<(int, int, int)>();
        for (int ti = 0; ti < triangles.Count; ti++)
        {
            if (!triAlive[ti]) continue;
            var tri = triangles[ti];
            int v0 = Find(parent, tri[0]);
            int v1 = Find(parent, tri[1]);
            int v2 = Find(parent, tri[2]);
            if (v0 == v1 || v1 == v2 || v0 == v2) continue;

            if (idMap[v0] < 0) { idMap[v0] = newPositions.Count; newPositions.Add(positions[v0]); }
            if (idMap[v1] < 0) { idMap[v1] = newPositions.Count; newPositions.Add(positions[v1]); }
            if (idMap[v2] < 0) { idMap[v2] = newPositions.Count; newPositions.Add(positions[v2]); }

            newTriangles.Add((idMap[v0], idMap[v1], idMap[v2]));
        }

        if (newPositions.Count > 0 && newTriangles.Count > 0 && originalAbsVolume > 1e-18)
        {
            double simplifiedAbsVolume = System.Math.Abs(ComputeVolume(newPositions, newTriangles));
            if (simplifiedAbsVolume > 1e-18)
            {
                double scale = System.Math.Pow(originalAbsVolume / simplifiedAbsVolume, 1.0 / 3.0);
                Vec3 simplifiedCentroid = ComputeCentroid(newPositions);
                for (int i = 0; i < newPositions.Count; i++)
                {
                    var delta = newPositions[i] - simplifiedCentroid;
                    newPositions[i] = originalCentroid + delta * scale;
                }
            }
        }

        var builder = new MeshBuilder(1e-12);
        return builder.Build(newPositions, newTriangles);
    }

    private static int Find(int[] parent, int i)
    {
        while (parent[i] != i)
        {
            parent[i] = parent[parent[i]]; // path compression
            i = parent[i];
        }
        return i;
    }

    private static long EdgeKey(int a, int b)
    {
        if (a > b) { int t = a; a = b; b = t; }
        return (long)a << 32 | (uint)b;
    }

    private static double[] ComputePlaneQuadric(double a, double b, double c, double d)
    {
        // Symmetric 4x4 stored as 10 elements: q[0..9] = {aa, ab, ac, ad, bb, bc, bd, cc, cd, dd}
        return new[]
        {
            a * a, a * b, a * c, a * d,
            b * b, b * c, b * d,
            c * c, c * d,
            d * d
        };
    }

    private static void AddQuadric(double[] target, double[] source)
    {
        for (int i = 0; i < 10; i++)
            target[i] += source[i];
    }

    private static double ComputeEdgeCost(double[][] quadrics, Vec3[] positions, int a, int b, out Vec3 optimalPos)
    {
        // Sum the quadrics
        var q = new double[10];
        for (int i = 0; i < 10; i++)
            q[i] = quadrics[a][i] + quadrics[b][i];

        var aPos = positions[a];
        var bPos = positions[b];
        var midpoint = (aPos + bPos) * 0.5;

        Vec3 bestPos = midpoint;
        double bestErr = EvaluateQuadric(q, midpoint);

        double errA = EvaluateQuadric(q, aPos);
        if (errA < bestErr)
        {
            bestErr = errA;
            bestPos = aPos;
        }

        double errB = EvaluateQuadric(q, bPos);
        if (errB < bestErr)
        {
            bestErr = errB;
            bestPos = bPos;
        }

        // Try unconstrained optimum, but only keep it when finite and close to the edge
        // segment to avoid large drift and volume collapse.
        var mat = new Mat3(q[0], q[1], q[2], q[1], q[4], q[5], q[2], q[5], q[7]);
        var rhs = new Vec3(-q[3], -q[6], -q[8]);
        if (LinearSolve3.Solve(mat, rhs, out var solved) && IsFinite(solved))
        {
            var edge = bPos - aPos;
            double edgeLenSq = edge.LengthSquared;
            if (edgeLenSq > 1e-30)
            {
                double t = Vec3.Dot(solved - aPos, edge) / edgeLenSq;
                if (t >= -0.25 && t <= 1.25)
                {
                    double errSolved = EvaluateQuadric(q, solved);
                    if (errSolved < bestErr)
                    {
                        bestErr = errSolved;
                        bestPos = solved;
                    }
                }
            }
        }

        optimalPos = bestPos;
        return bestErr;
    }

    private static bool IsFinite(Vec3 v) =>
        !(double.IsNaN(v.X) || double.IsInfinity(v.X) ||
          double.IsNaN(v.Y) || double.IsInfinity(v.Y) ||
          double.IsNaN(v.Z) || double.IsInfinity(v.Z));

    private static double EvaluateQuadric(double[] q, Vec3 p)
    {
        double x = p.X, y = p.Y, z = p.Z;
        double err = q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x
                   + q[4] * y * y + 2 * q[5] * y * z + 2 * q[6] * y
                   + q[7] * z * z + 2 * q[8] * z
                   + q[9];
        return System.Math.Max(err, 0);
    }

    private static double ComputeVolume(IReadOnlyList<Vec3> positions, IReadOnlyList<(int I0, int I1, int I2)> triangles)
    {
        double vol6 = 0.0;
        for (int i = 0; i < triangles.Count; i++)
        {
            var (i0, i1, i2) = triangles[i];
            var a = positions[i0];
            var b = positions[i1];
            var c = positions[i2];
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

    private static bool IsCollapseTopologicallySafe(
        IReadOnlyList<int[]> triangles,
        IReadOnlyList<bool> triAlive,
        int[] parent,
        int a,
        int b)
    {
        var neighborsA = new HashSet<int>();
        var neighborsB = new HashSet<int>();

        for (int ti = 0; ti < triangles.Count; ti++)
        {
            if (!triAlive[ti]) continue;
            var tri = triangles[ti];
            int v0 = Find(parent, tri[0]);
            int v1 = Find(parent, tri[1]);
            int v2 = Find(parent, tri[2]);

            bool hasA = v0 == a || v1 == a || v2 == a;
            bool hasB = v0 == b || v1 == b || v2 == b;

            if (hasA)
            {
                if (v0 != a) neighborsA.Add(v0);
                if (v1 != a) neighborsA.Add(v1);
                if (v2 != a) neighborsA.Add(v2);
            }

            if (hasB)
            {
                if (v0 != b) neighborsB.Add(v0);
                if (v1 != b) neighborsB.Add(v1);
                if (v2 != b) neighborsB.Add(v2);
            }
        }

        int common = 0;
        foreach (int n in neighborsA)
        {
            if (n != a && n != b && neighborsB.Contains(n))
                common++;
        }

        // Interior manifold edge collapse should preserve exactly two shared neighbors.
        return common == 2;
    }

    /// <summary>
    /// Simple min-heap for the edge-collapse priority queue.
    /// </summary>
    private class MinHeap
    {
        private readonly List<(double Cost, int A, int B)> _items = new();

        /// <summary>Number of items in the heap.</summary>
        public int Count => _items.Count;

        /// <summary>Inserts an edge with its cost.</summary>
        public void Insert(double cost, int a, int b)
        {
            _items.Add((cost, a, b));
            BubbleUp(_items.Count - 1);
        }

        /// <summary>Extracts the edge with the minimum cost.</summary>
        public (double Cost, int A, int B) ExtractMin()
        {
            var min = _items[0];
            int last = _items.Count - 1;
            _items[0] = _items[last];
            _items.RemoveAt(last);
            if (_items.Count > 0)
                BubbleDown(0);
            return min;
        }

        private void BubbleUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_items[i].Cost >= _items[parent].Cost) break;
                (_items[i], _items[parent]) = (_items[parent], _items[i]);
                i = parent;
            }
        }

        private void BubbleDown(int i)
        {
            int n = _items.Count;
            while (true)
            {
                int smallest = i;
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                if (left < n && _items[left].Cost < _items[smallest].Cost) smallest = left;
                if (right < n && _items[right].Cost < _items[smallest].Cost) smallest = right;
                if (smallest == i) break;
                (_items[i], _items[smallest]) = (_items[smallest], _items[i]);
                i = smallest;
            }
        }
    }
}
