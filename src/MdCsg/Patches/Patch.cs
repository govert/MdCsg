using MdCsg.Math;

namespace MdCsg.Patches;

/// <summary>
/// A patch is a connected set of sub-triangles bounded by intersection edges.
/// Each patch will be classified as Inside or Outside the other solid.
/// </summary>
public class Patch
{
    /// <summary>Unique patch identifier.</summary>
    public int Id { get; }

    /// <summary>Indices of sub-triangles belonging to this patch.</summary>
    public List<int> SubTriangleIndices { get; } = [];

    /// <summary>The "most confident" point for classification (max margin from other solid's faces).</summary>
    public Vec3 ConfidentPoint { get; set; }

    /// <summary>Classification result: true = inside the other solid.</summary>
    public bool? IsInside { get; set; }

    /// <summary>Whether this patch has a valid confident point.</summary>
    public bool HasConfidentPoint { get; set; }

    /// <summary>The original mesh this patch belongs to (0 = mesh A, 1 = mesh B).</summary>
    public int SourceMesh { get; set; }

    /// <summary>
    /// Creates a new patch.
    /// </summary>
    public Patch(int id)
    {
        Id = id;
    }
}
