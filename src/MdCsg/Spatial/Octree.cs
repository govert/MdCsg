using MdCsg.Math;

namespace MdCsg.Spatial;

/// <summary>
/// An octree for 3D spatial indexing.
/// Items are inserted at positions and can be queried by AABB or sphere region.
/// </summary>
/// <typeparam name="T">The type of items stored in the octree.</typeparam>
public class Octree<T>
{
    private readonly OctreeNode _root;
    private readonly int _maxDepth;
    private readonly int _maxItems;

    /// <summary>
    /// Creates a new octree with the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounding box of the entire octree.</param>
    /// <param name="maxDepth">Maximum subdivision depth.</param>
    /// <param name="maxItems">Maximum items per leaf before subdividing.</param>
    public Octree(Aabb bounds, int maxDepth = 8, int maxItems = 16)
    {
        _maxDepth = maxDepth;
        _maxItems = maxItems;
        _root = new OctreeNode(bounds);
    }

    /// <summary>The bounding box of the octree.</summary>
    public Aabb Bounds => _root.Bounds;

    /// <summary>
    /// Inserts an item at the given position.
    /// </summary>
    /// <param name="position">The 3D position of the item.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(Vec3 position, T item)
    {
        Insert(_root, position, item, 0);
    }

    /// <summary>
    /// Queries all items within the given axis-aligned bounding box.
    /// </summary>
    /// <param name="queryBounds">The query AABB.</param>
    /// <returns>A list of items whose positions fall within the AABB.</returns>
    public List<T> Query(Aabb queryBounds)
    {
        var result = new List<T>();
        Query(_root, queryBounds, result);
        return result;
    }

    /// <summary>
    /// Queries all items within a sphere.
    /// </summary>
    /// <param name="center">Center of the query sphere.</param>
    /// <param name="radius">Radius of the query sphere.</param>
    /// <returns>A list of items within the sphere.</returns>
    public List<T> QuerySphere(Vec3 center, double radius)
    {
        var result = new List<T>();
        double r2 = radius * radius;
        // First use AABB query to narrow down, then filter by distance
        var queryBounds = new Aabb(
            new Vec3(center.X - radius, center.Y - radius, center.Z - radius),
            new Vec3(center.X + radius, center.Y + radius, center.Z + radius));
        QuerySphere(_root, queryBounds, center, r2, result);
        return result;
    }

    private void Insert(OctreeNode node, Vec3 position, T item, int depth)
    {
        if (!node.Bounds.Contains(position))
            return;

        if (node.Children != null)
        {
            // Internal node: recurse into the appropriate child
            int idx = ChildIndex(node.Bounds, position);
            Insert(node.Children[idx], position, item, depth + 1);
            return;
        }

        // Leaf node
        node.Items.Add((position, item));

        // Subdivide if over capacity and not at max depth
        if (node.Items.Count > _maxItems && depth < _maxDepth)
        {
            Subdivide(node, depth);
        }
    }

    private void Subdivide(OctreeNode node, int depth)
    {
        var b = node.Bounds;
        var mid = (b.Min + b.Max) * 0.5;

        node.Children = new OctreeNode[8];
        node.Children[0] = new OctreeNode(new Aabb(b.Min, mid));
        node.Children[1] = new OctreeNode(new Aabb(new Vec3(mid.X, b.Min.Y, b.Min.Z), new Vec3(b.Max.X, mid.Y, mid.Z)));
        node.Children[2] = new OctreeNode(new Aabb(new Vec3(b.Min.X, mid.Y, b.Min.Z), new Vec3(mid.X, b.Max.Y, mid.Z)));
        node.Children[3] = new OctreeNode(new Aabb(new Vec3(mid.X, mid.Y, b.Min.Z), new Vec3(b.Max.X, b.Max.Y, mid.Z)));
        node.Children[4] = new OctreeNode(new Aabb(new Vec3(b.Min.X, b.Min.Y, mid.Z), new Vec3(mid.X, mid.Y, b.Max.Z)));
        node.Children[5] = new OctreeNode(new Aabb(new Vec3(mid.X, b.Min.Y, mid.Z), new Vec3(b.Max.X, mid.Y, b.Max.Z)));
        node.Children[6] = new OctreeNode(new Aabb(new Vec3(b.Min.X, mid.Y, mid.Z), new Vec3(mid.X, b.Max.Y, b.Max.Z)));
        node.Children[7] = new OctreeNode(new Aabb(mid, b.Max));

        // Redistribute items
        var items = node.Items;
        node.Items = new List<(Vec3, T)>();
        for (int i = 0; i < items.Count; i++)
        {
            int idx = ChildIndex(b, items[i].Item1);
            Insert(node.Children[idx], items[i].Item1, items[i].Item2, depth + 1);
        }
    }

    private void Query(OctreeNode node, Aabb queryBounds, List<T> result)
    {
        if (!node.Bounds.Overlaps(queryBounds))
            return;

        if (node.Children != null)
        {
            for (int i = 0; i < 8; i++)
                Query(node.Children[i], queryBounds, result);
            return;
        }

        for (int i = 0; i < node.Items.Count; i++)
        {
            if (queryBounds.Contains(node.Items[i].Item1))
                result.Add(node.Items[i].Item2);
        }
    }

    private void QuerySphere(OctreeNode node, Aabb queryBounds, Vec3 center, double r2, List<T> result)
    {
        if (!node.Bounds.Overlaps(queryBounds))
            return;

        if (node.Children != null)
        {
            for (int i = 0; i < 8; i++)
                QuerySphere(node.Children[i], queryBounds, center, r2, result);
            return;
        }

        for (int i = 0; i < node.Items.Count; i++)
        {
            if ((node.Items[i].Item1 - center).LengthSquared <= r2)
                result.Add(node.Items[i].Item2);
        }
    }

    private static int ChildIndex(Aabb bounds, Vec3 position)
    {
        var mid = (bounds.Min + bounds.Max) * 0.5;
        int idx = 0;
        if (position.X >= mid.X) idx |= 1;
        if (position.Y >= mid.Y) idx |= 2;
        if (position.Z >= mid.Z) idx |= 4;
        return idx;
    }

    private class OctreeNode
    {
        public Aabb Bounds;
        public List<(Vec3 Position, T Item)> Items = new();
        public OctreeNode[]? Children;

        public OctreeNode(Aabb bounds) => Bounds = bounds;
    }
}
