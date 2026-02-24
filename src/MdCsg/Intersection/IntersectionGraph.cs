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

    private IntersectionGraph(
        List<IntersectionSegment> segments,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsA,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsB)
    {
        Segments = segments;
        FaceSegmentsA = faceSegmentsA;
        FaceSegmentsB = faceSegmentsB;
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

                    if (!faceSegmentsA.TryGetValue(faceA, out var listA))
                    {
                        listA = [];
                        faceSegmentsA[faceA] = listA;
                    }
                    listA.Add(seg);

                    if (!faceSegmentsB.TryGetValue(faceB, out var listB))
                    {
                        listB = [];
                        faceSegmentsB[faceB] = listB;
                    }
                    listB.Add(seg);
                }
            }
        }

        return new IntersectionGraph(segments, faceSegmentsA, faceSegmentsB);
    }

    private static Triangle3 GetTriangle(HalfEdgeMesh mesh, int faceIndex)
    {
        var face = mesh.Faces[faceIndex];
        var verts = face.GetVertices();
        return new Triangle3(verts[0].Position, verts[1].Position, verts[2].Position);
    }
}
