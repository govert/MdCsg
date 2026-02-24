using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Intersection;

/// <summary>
/// Computes all intersection segments between two meshes using BVH-accelerated
/// dual-tree traversal.
/// </summary>
public class IntersectionGraph
{
    /// <summary>All intersection segments between the two meshes.</summary>
    public IReadOnlyList<IntersectionSegment> Segments { get; }

    /// <summary>Maps a face index in mesh A to its intersection segments.</summary>
    public IReadOnlyDictionary<int, List<IntersectionSegment>> FaceSegmentsA { get; }

    /// <summary>Maps a face index in mesh B to its intersection segments.</summary>
    public IReadOnlyDictionary<int, List<IntersectionSegment>> FaceSegmentsB { get; }

    /// <summary>
    /// Coplanar face pairs: maps face index (from either mesh) to normal agreement info.
    /// True = normals agree in direction.
    /// </summary>
    public IReadOnlyDictionary<int, bool> CoplanarFacesA { get; }

    /// <summary>Coplanar face pairs for mesh B.</summary>
    public IReadOnlyDictionary<int, bool> CoplanarFacesB { get; }

    private IntersectionGraph(
        List<IntersectionSegment> segments,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsA,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsB,
        Dictionary<int, bool> coplanarFacesA,
        Dictionary<int, bool> coplanarFacesB)
    {
        Segments = segments;
        FaceSegmentsA = faceSegmentsA;
        FaceSegmentsB = faceSegmentsB;
        CoplanarFacesA = coplanarFacesA;
        CoplanarFacesB = coplanarFacesB;
    }

    /// <summary>
    /// Computes the intersection graph between two meshes.
    /// </summary>
    public static IntersectionGraph Compute(HalfEdgeMesh meshA, HalfEdgeMesh meshB, double gridSize = MathUtil.DefaultGridSize)
    {
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);

        var overlappingPairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);

        var segments = new List<IntersectionSegment>();
        var faceSegmentsA = new Dictionary<int, List<IntersectionSegment>>();
        var faceSegmentsB = new Dictionary<int, List<IntersectionSegment>>();
        var coplanarFacesA = new Dictionary<int, bool>();
        var coplanarFacesB = new Dictionary<int, bool>();

        foreach (var (faceA, faceB) in overlappingPairs)
        {
            var triA = GetTriangle(meshA, faceA);
            var triB = GetTriangle(meshB, faceB);

            if (TriTriIntersection.Intersect(triA, triB, out var seg))
            {
                seg = new IntersectionSegment(
                    SnapRounding.Snap(seg.Start, gridSize),
                    SnapRounding.Snap(seg.End, gridSize),
                    faceA,
                    faceB);

                if (!seg.IsDegenerate)
                {
                    segments.Add(seg);
                    AddToDict(faceSegmentsA, faceA, seg);
                    AddToDict(faceSegmentsB, faceB, seg);
                }
            }
            else if (TriTriIntersection.AreCoplanar(triA, triB) &&
                     TriTriIntersection.IntersectCoplanar(triA, triB, out var segsForA, out var segsForB, out bool normalsAgree))
            {
                // Track which faces are coplanar
                coplanarFacesA[faceA] = normalsAgree;
                coplanarFacesB[faceB] = normalsAgree;

                // Add clipping segments for face A (edges of triB clipped to triA)
                foreach (var (start, end) in segsForA)
                {
                    var snapped = new IntersectionSegment(
                        SnapRounding.Snap(start, gridSize),
                        SnapRounding.Snap(end, gridSize),
                        faceA, faceB);
                    if (!snapped.IsDegenerate)
                    {
                        segments.Add(snapped);
                        AddToDict(faceSegmentsA, faceA, snapped);
                    }
                }

                // Add clipping segments for face B (edges of triA clipped to triB)
                foreach (var (start, end) in segsForB)
                {
                    var snapped = new IntersectionSegment(
                        SnapRounding.Snap(start, gridSize),
                        SnapRounding.Snap(end, gridSize),
                        faceA, faceB);
                    if (!snapped.IsDegenerate)
                    {
                        segments.Add(snapped);
                        AddToDict(faceSegmentsB, faceB, snapped);
                    }
                }
            }
        }

        return new IntersectionGraph(segments, faceSegmentsA, faceSegmentsB, coplanarFacesA, coplanarFacesB);
    }

    private static void AddToDict(Dictionary<int, List<IntersectionSegment>> dict, int faceIdx, IntersectionSegment seg)
    {
        if (!dict.TryGetValue(faceIdx, out var list))
        {
            list = [];
            dict[faceIdx] = list;
        }
        list.Add(seg);
    }

    private static Triangle3 GetTriangle(HalfEdgeMesh mesh, int faceIndex)
    {
        var face = mesh.Faces[faceIndex];
        var verts = face.GetVertices();
        return new Triangle3(verts[0].Position, verts[1].Position, verts[2].Position);
    }
}
