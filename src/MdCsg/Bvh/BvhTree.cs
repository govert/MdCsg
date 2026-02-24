using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Bvh;

/// <summary>
/// A BVH (bounding volume hierarchy) built from mesh faces using the Surface Area Heuristic.
/// Uses a flat array layout for cache-friendly traversal.
/// </summary>
public class BvhTree
{
    private readonly BvhNode[] _nodes;
    private readonly int[] _faceIndices;
    private readonly HalfEdgeMesh _mesh;

    /// <summary>The flat node array.</summary>
    public ReadOnlySpan<BvhNode> Nodes => _nodes;

    /// <summary>The face index array (maps leaf primitive slots to mesh face indices).</summary>
    public ReadOnlySpan<int> FaceIndices => _faceIndices;

    /// <summary>Number of nodes in the tree.</summary>
    public int NodeCount => _nodes.Length;

    /// <summary>The source mesh.</summary>
    public HalfEdgeMesh Mesh => _mesh;

    private BvhTree(BvhNode[] nodes, int[] faceIndices, HalfEdgeMesh mesh)
    {
        _nodes = nodes;
        _faceIndices = faceIndices;
        _mesh = mesh;
    }

    /// <summary>
    /// Builds a BVH from the faces of a half-edge mesh using SAH.
    /// </summary>
    public static BvhTree Build(HalfEdgeMesh mesh)
    {
        var faceCount = mesh.Faces.Count;
        if (faceCount == 0)
            return new BvhTree([], [], mesh);

        var faceIndices = new int[faceCount];
        var centroids = new Vec3[faceCount];
        var bounds = new Aabb[faceCount];

        for (int i = 0; i < faceCount; i++)
        {
            faceIndices[i] = i;
            var face = mesh.Faces[i];
            face.GetTrianglePositions(out var a, out var b, out var c);
            bounds[i] = Aabb.FromTriangle(a, b, c);
            centroids[i] = (a + b + c) / 3.0;
        }

        var nodes = new List<BvhNode>();
        BuildRecursive(nodes, faceIndices, centroids, bounds, 0, faceCount);
        return new BvhTree(nodes.ToArray(), faceIndices, mesh);
    }

    private static int BuildRecursive(
        List<BvhNode> nodes, int[] indices, Vec3[] centroids, Aabb[] bounds,
        int start, int end)
    {
        int nodeIndex = nodes.Count;
        nodes.Add(default); // placeholder

        // Compute bounds for this range
        var nodeBounds = bounds[indices[start]];
        for (int i = start + 1; i < end; i++)
            nodeBounds = Aabb.Union(nodeBounds, bounds[indices[i]]);

        int count = end - start;
        if (count <= 4)
        {
            // Leaf
            nodes[nodeIndex] = new BvhNode
            {
                Bounds = nodeBounds,
                LeftOrStart = start,
                PrimitiveCount = count
            };
            return nodeIndex;
        }

        // Find best split using SAH
        int bestAxis = 0;
        int bestSplit = start + count / 2;
        double bestCost = double.MaxValue;

        var centroidBounds = Aabb.FromPoint(centroids[indices[start]]);
        for (int i = start + 1; i < end; i++)
            centroidBounds = centroidBounds.ExpandToInclude(centroids[indices[i]]);

        var centroidSize = centroidBounds.Size;

        for (int axis = 0; axis < 3; axis++)
        {
            if (centroidSize[axis] < 1e-30) continue;

            const int numBins = 12;
            var binBounds = new Aabb[numBins];
            var binCounts = new int[numBins];
            for (int i = 0; i < numBins; i++)
                binBounds[i] = Aabb.Empty;

            double axisMin = centroidBounds.Min[axis];
            double axisRange = centroidSize[axis];

            for (int i = start; i < end; i++)
            {
                int bin = System.Math.Min(numBins - 1,
                    (int)(numBins * (centroids[indices[i]][axis] - axisMin) / axisRange));
                binBounds[bin] = Aabb.Union(binBounds[bin], bounds[indices[i]]);
                binCounts[bin]++;
            }

            // Sweep to find best split
            var leftBounds = new Aabb[numBins - 1];
            var leftCounts = new int[numBins - 1];
            var accumBounds = Aabb.Empty;
            int accumCount = 0;
            for (int i = 0; i < numBins - 1; i++)
            {
                accumBounds = Aabb.Union(accumBounds, binBounds[i]);
                accumCount += binCounts[i];
                leftBounds[i] = accumBounds;
                leftCounts[i] = accumCount;
            }

            accumBounds = Aabb.Empty;
            accumCount = 0;
            for (int i = numBins - 1; i >= 1; i--)
            {
                accumBounds = Aabb.Union(accumBounds, binBounds[i]);
                accumCount += binCounts[i];

                if (leftCounts[i - 1] == 0 || accumCount == 0) continue;

                double cost = leftCounts[i - 1] * SafeSurfaceArea(leftBounds[i - 1])
                            + accumCount * SafeSurfaceArea(accumBounds);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestAxis = axis;
                    bestSplit = start + leftCounts[i - 1];
                }
            }
        }

        // Partition indices by best axis using nth_element partitioning (O(n) vs O(n log n))
        double splitRange = centroidSize[bestAxis];
        if (splitRange < 1e-30)
        {
            bestSplit = start + count / 2;
        }
        else
        {
            if (bestSplit <= start) bestSplit = start + 1;
            if (bestSplit >= end) bestSplit = end - 1;
            NthElement(indices, centroids, bestAxis, start, end, bestSplit);
        }

        // Build children
        int leftChild = BuildRecursive(nodes, indices, centroids, bounds, start, bestSplit);
        int rightChild = BuildRecursive(nodes, indices, centroids, bounds, bestSplit, end);

        nodes[nodeIndex] = new BvhNode
        {
            Bounds = nodeBounds,
            LeftOrStart = leftChild,
            Right = rightChild,
            PrimitiveCount = 0
        };

        return nodeIndex;
    }

    /// <summary>
    /// Partitions so elements before nth are less-or-equal and after nth are greater-or-equal.
    /// O(n) expected time via quickselect.
    /// </summary>
    private static void NthElement(int[] indices, Vec3[] centroids, int axis, int start, int end, int nth)
    {
        while (start < end - 1)
        {
            // Choose pivot as median of three
            int mid = start + (end - start) / 2;
            double vStart = centroids[indices[start]][axis];
            double vMid = centroids[indices[mid]][axis];
            double vEnd = centroids[indices[end - 1]][axis];

            int pivotIdx;
            if ((vStart <= vMid && vMid <= vEnd) || (vEnd <= vMid && vMid <= vStart))
                pivotIdx = mid;
            else if ((vMid <= vStart && vStart <= vEnd) || (vEnd <= vStart && vStart <= vMid))
                pivotIdx = start;
            else
                pivotIdx = end - 1;

            // Move pivot to end
            (indices[pivotIdx], indices[end - 1]) = (indices[end - 1], indices[pivotIdx]);
            double pivotVal = centroids[indices[end - 1]][axis];

            int store = start;
            for (int i = start; i < end - 1; i++)
            {
                if (centroids[indices[i]][axis] <= pivotVal)
                {
                    (indices[store], indices[i]) = (indices[i], indices[store]);
                    store++;
                }
            }
            (indices[store], indices[end - 1]) = (indices[end - 1], indices[store]);

            if (store == nth) return;
            if (store < nth) start = store + 1;
            else end = store;
        }
    }

    private static double SafeSurfaceArea(Aabb box)
    {
        var sa = box.SurfaceArea;
        return sa > 0 ? sa : 1e-30;
    }

    /// <summary>
    /// Queries all face indices whose AABBs overlap the given query box.
    /// </summary>
    public void Query(Aabb queryBox, List<int> results)
    {
        if (_nodes.Length == 0) return;
        QueryRecursive(0, queryBox, results);
    }

    private void QueryRecursive(int nodeIndex, Aabb queryBox, List<int> results)
    {
        ref readonly var node = ref _nodes[nodeIndex];
        if (!node.Bounds.Overlaps(queryBox)) return;

        if (node.IsLeaf)
        {
            for (int i = 0; i < node.PrimitiveCount; i++)
                results.Add(_faceIndices[node.LeftOrStart + i]);
        }
        else
        {
            QueryRecursive(node.LeftOrStart, queryBox, results);
            QueryRecursive(node.Right, queryBox, results);
        }
    }

    /// <summary>
    /// Counts the number of ray-face intersections (for inside/outside testing).
    /// Uses iterative traversal with an explicit stack for performance.
    /// </summary>
    public int RayCastCount(Ray ray)
    {
        if (_nodes.Length == 0) return 0;

        int count = 0;
        Span<int> stack = stackalloc int[64];
        int top = 0;
        stack[top++] = 0;

        while (top > 0)
        {
            int nodeIndex = stack[--top];
            ref readonly var node = ref _nodes[nodeIndex];

            if (!RayIntersectsAabb(ray, node.Bounds)) continue;

            if (node.IsLeaf)
            {
                for (int i = 0; i < node.PrimitiveCount; i++)
                {
                    var faceIdx = _faceIndices[node.LeftOrStart + i];
                    if (RayIntersectsTriangle(ray, _mesh.Faces[faceIdx]))
                        count++;
                }
            }
            else
            {
                stack[top++] = node.Right;
                stack[top++] = node.LeftOrStart;
            }
        }

        return count;
    }

    private static bool RayIntersectsAabb(Ray ray, Aabb box)
    {
        double tmin = double.NegativeInfinity;
        double tmax = double.PositiveInfinity;

        for (int i = 0; i < 3; i++)
        {
            double invD = 1.0 / ray.Direction[i];
            double t0 = (box.Min[i] - ray.Origin[i]) * invD;
            double t1 = (box.Max[i] - ray.Origin[i]) * invD;
            if (invD < 0) (t0, t1) = (t1, t0);
            tmin = t0 > tmin ? t0 : tmin;
            tmax = t1 < tmax ? t1 : tmax;
            if (tmax < tmin) return false;
        }
        return tmax >= 0;
    }

    private static bool RayIntersectsTriangle(Ray ray, Face face)
    {
        face.GetTrianglePositions(out var v0, out var v1, out var v2);

        // Moller-Trumbore
        var edge1 = v1 - v0;
        var edge2 = v2 - v0;
        var h = Vec3.Cross(ray.Direction, edge2);
        var a = Vec3.Dot(edge1, h);
        if (System.Math.Abs(a) < 1e-15) return false;

        var f = 1.0 / a;
        var s = ray.Origin - v0;
        var u = f * Vec3.Dot(s, h);
        if (u < 0 || u > 1) return false;

        var q = Vec3.Cross(s, edge1);
        var v = f * Vec3.Dot(ray.Direction, q);
        if (v < 0 || u + v > 1) return false;

        var t = f * Vec3.Dot(edge2, q);
        return t > 1e-15; // only forward hits
    }

    /// <summary>
    /// Returns the face index for a leaf node's primitive.
    /// </summary>
    public int GetFaceIndex(int primitiveIndex) => _faceIndices[primitiveIndex];
}
