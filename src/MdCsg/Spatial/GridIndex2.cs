using MdCsg.Math;

namespace MdCsg.Spatial;

/// <summary>
/// A 2D grid index for spatial lookups using XY projection.
/// Items can span multiple cells if their AABB covers more than one cell.
/// </summary>
/// <typeparam name="T">The type of items stored in the grid.</typeparam>
public class GridIndex2<T>
{
    private readonly double _cellSize;
    private readonly double _invCellSize;
    private readonly Vec2 _origin;
    private readonly Dictionary<(int, int), List<T>> _cells = new();

    /// <summary>
    /// Creates a new 2D grid index.
    /// </summary>
    /// <param name="cellSize">The size of each grid cell.</param>
    /// <param name="origin">The origin point for cell coordinate calculation.</param>
    public GridIndex2(double cellSize, Vec2 origin = default)
    {
        if (cellSize <= 0) throw new ArgumentException("Cell size must be positive.", nameof(cellSize));
        _cellSize = cellSize;
        _invCellSize = 1.0 / cellSize;
        _origin = origin;
    }

    /// <summary>The size of each grid cell.</summary>
    public double CellSize => _cellSize;

    /// <summary>The number of non-empty cells.</summary>
    public int CellCount => _cells.Count;

    /// <summary>
    /// Inserts an item with an AABB footprint. The item is added to all cells that overlap its XY projection.
    /// </summary>
    /// <param name="bounds">The 3D AABB of the item (XY projection is used).</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(Aabb bounds, T item)
    {
        int minX = CellX(bounds.Min.X);
        int maxX = CellX(bounds.Max.X);
        int minY = CellY(bounds.Min.Y);
        int maxY = CellY(bounds.Max.Y);

        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        {
            var key = (x, y);
            if (!_cells.TryGetValue(key, out var list))
            {
                list = new List<T>();
                _cells[key] = list;
            }
            list.Add(item);
        }
    }

    /// <summary>
    /// Queries all items in the cell containing the given 2D position.
    /// </summary>
    /// <param name="position">The 2D query position.</param>
    /// <returns>A list of items in the cell, or an empty list if empty.</returns>
    public List<T> Query(Vec2 position)
    {
        var key = (CellX(position.X), CellY(position.Y));
        if (_cells.TryGetValue(key, out var list))
            return new List<T>(list);
        return new List<T>();
    }

    /// <summary>
    /// Removes all items from the grid.
    /// </summary>
    public void Clear()
    {
        _cells.Clear();
    }

    private int CellX(double x) => (int)System.Math.Floor((x - _origin.X) * _invCellSize);
    private int CellY(double y) => (int)System.Math.Floor((y - _origin.Y) * _invCellSize);
}
