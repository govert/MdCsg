using MdCsg.Math;

namespace MdCsg.Spatial;

/// <summary>
/// A spatial hash grid for 3D point queries.
/// Items are inserted at positions and can be queried by sphere region.
/// </summary>
/// <typeparam name="T">The type of items stored in the grid.</typeparam>
public class HashGrid3<T>
{
    private readonly double _cellSize;
    private readonly double _invCellSize;
    private readonly Dictionary<(int, int, int), List<(Vec3 Position, T Item)>> _cells = new();

    /// <summary>
    /// Creates a new hash grid with the specified cell size.
    /// </summary>
    /// <param name="cellSize">The size of each grid cell.</param>
    public HashGrid3(double cellSize)
    {
        if (cellSize <= 0) throw new ArgumentException("Cell size must be positive.", nameof(cellSize));
        _cellSize = cellSize;
        _invCellSize = 1.0 / cellSize;
    }

    /// <summary>The size of each grid cell.</summary>
    public double CellSize => _cellSize;

    /// <summary>The number of non-empty cells.</summary>
    public int CellCount => _cells.Count;

    /// <summary>
    /// Inserts an item at the given position.
    /// </summary>
    /// <param name="position">The 3D position of the item.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(Vec3 position, T item)
    {
        var key = CellKey(position);
        if (!_cells.TryGetValue(key, out var list))
        {
            list = new List<(Vec3, T)>();
            _cells[key] = list;
        }
        list.Add((position, item));
    }

    /// <summary>
    /// Queries all items within a sphere centered at the given position.
    /// </summary>
    /// <param name="center">Center of the query sphere.</param>
    /// <param name="radius">Radius of the query sphere.</param>
    /// <returns>A list of items within the sphere.</returns>
    public List<T> Query(Vec3 center, double radius)
    {
        var result = new List<T>();
        double r2 = radius * radius;

        int minX = (int)System.Math.Floor((center.X - radius) * _invCellSize);
        int maxX = (int)System.Math.Floor((center.X + radius) * _invCellSize);
        int minY = (int)System.Math.Floor((center.Y - radius) * _invCellSize);
        int maxY = (int)System.Math.Floor((center.Y + radius) * _invCellSize);
        int minZ = (int)System.Math.Floor((center.Z - radius) * _invCellSize);
        int maxZ = (int)System.Math.Floor((center.Z + radius) * _invCellSize);

        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        for (int z = minZ; z <= maxZ; z++)
        {
            if (_cells.TryGetValue((x, y, z), out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i].Position - center).LengthSquared <= r2)
                        result.Add(list[i].Item);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all items in the specified cell.
    /// </summary>
    /// <param name="cx">Cell X index.</param>
    /// <param name="cy">Cell Y index.</param>
    /// <param name="cz">Cell Z index.</param>
    /// <returns>A list of items in the cell, or an empty list if the cell is empty.</returns>
    public List<T> QueryCell(int cx, int cy, int cz)
    {
        if (_cells.TryGetValue((cx, cy, cz), out var list))
        {
            var result = new List<T>(list.Count);
            for (int i = 0; i < list.Count; i++)
                result.Add(list[i].Item);
            return result;
        }
        return new List<T>();
    }

    /// <summary>
    /// Removes all items from the grid.
    /// </summary>
    public void Clear()
    {
        _cells.Clear();
    }

    private (int, int, int) CellKey(Vec3 position) =>
        ((int)System.Math.Floor(position.X * _invCellSize),
         (int)System.Math.Floor(position.Y * _invCellSize),
         (int)System.Math.Floor(position.Z * _invCellSize));
}
