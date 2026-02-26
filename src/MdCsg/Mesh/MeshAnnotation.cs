namespace MdCsg.Mesh;

/// <summary>
/// An auto-growing array that maps mesh element IDs to annotation values.
/// </summary>
/// <typeparam name="T">The annotation value type.</typeparam>
public class MeshAnnotation<T>
{
    private T[] _data;

    /// <summary>
    /// Creates a mesh annotation with the given initial capacity.
    /// </summary>
    /// <param name="initialCapacity">Initial array size.</param>
    public MeshAnnotation(int initialCapacity = 16)
    {
        _data = new T[initialCapacity];
    }

    /// <summary>
    /// Gets or sets the annotation for the element with the given ID.
    /// Automatically grows the internal array as needed.
    /// </summary>
    /// <param name="id">The element ID.</param>
    public T this[int id]
    {
        get => id < _data.Length ? _data[id] : default!;
        set
        {
            EnsureCapacity(id + 1);
            _data[id] = value;
        }
    }

    /// <summary>
    /// Creates an annotation sized for the vertices of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>A new annotation with capacity for all vertices.</returns>
    public static MeshAnnotation<T> ForVertices(HalfEdgeMesh mesh) =>
        new(mesh.Vertices.Count);

    /// <summary>
    /// Creates an annotation sized for the faces of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>A new annotation with capacity for all faces.</returns>
    public static MeshAnnotation<T> ForFaces(HalfEdgeMesh mesh) =>
        new(mesh.Faces.Count);

    /// <summary>
    /// Creates an annotation sized for the half-edges of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>A new annotation with capacity for all half-edges.</returns>
    public static MeshAnnotation<T> ForHalfEdges(HalfEdgeMesh mesh) =>
        new(mesh.HalfEdges.Count);

    private void EnsureCapacity(int required)
    {
        if (required <= _data.Length) return;
        int newSize = _data.Length;
        while (newSize < required) newSize *= 2;
        var newData = new T[newSize];
        _data.CopyTo(newData, 0);
        _data = newData;
    }
}
