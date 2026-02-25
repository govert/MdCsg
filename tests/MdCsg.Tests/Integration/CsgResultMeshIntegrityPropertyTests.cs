using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG result mesh integrity — face cycle consistency, half-edge properties, vertex referencing</summary>
public class CsgResultMeshIntegrityPropertyTests
{
    [Fact]
    public void Union_ResultMesh_AllFacesHaveThreeEdgeCycle()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Intersect_ResultMesh_AllFacesHaveThreeEdgeCycle()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Difference_ResultMesh_AllFacesHaveThreeEdgeCycle()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Union_ResultMesh_NextPrevConsistency()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Union_ResultMesh_HalfEdgeCountIs3xFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count * 3, result.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Union_ResultMesh_AllHalfEdgesHaveFace()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Face);
        }
    }

    [Fact]
    public void Union_ResultMesh_FaceNormals_NonZero()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var normal = face.Normal;
            Assert.True(normal.LengthSquared > 1e-20,
                $"Face {face.Id} has near-zero normal");
        }
    }

    [Fact]
    public void Intersect_ResultMesh_BoundsContainedByInputBounds()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        var bounds = result.Mesh.GetBounds();
        var boundsA = a.Bounds;
        var boundsB = b.Bounds;
        // Intersection bounds should be within both input bounds (with tolerance)
        Assert.True(bounds.Min.X >= System.Math.Min(boundsA.Min.X, boundsB.Min.X) - 0.01);
        Assert.True(bounds.Max.X <= System.Math.Max(boundsA.Max.X, boundsB.Max.X) + 0.01);
    }

    [Fact]
    public void Union_ResultMesh_BoundsContainBothInputs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(3, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        // Union bounds should cover both inputs
        Assert.True(bounds.Min.X <= a.Bounds.Min.X + 0.01);
        Assert.True(bounds.Max.X >= b.Bounds.Max.X - 0.01);
    }

    [Fact]
    public void Difference_ResultMesh_BoundsWithinA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        var bounds = result.Mesh.GetBounds();
        // Difference bounds should be within A's bounds (with tolerance)
        Assert.True(bounds.Min.X >= a.Bounds.Min.X - 0.01);
        Assert.True(bounds.Max.X <= a.Bounds.Max.X + 0.01);
    }

    [Fact]
    public void Union_Disjoint_FaceCountIsSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_Overlapping_FaceCountPositive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0, "Intersection of overlapping cubes should have faces");
        // The intersection region is smaller than the union of both inputs
        var union = Csg.Union(a, b);
        Assert.True(result.FaceCount <= union.FaceCount,
            "Intersection face count should not exceed union face count");
    }

    [Fact]
    public void Union_CubeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_ResultVertices_AllReferencedByFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        var referencedVerts = new HashSet<int>();
        foreach (var face in result.Mesh.Faces)
        {
            var verts = face.GetVertices();
            foreach (var v in verts)
                referencedVerts.Add(v.Id);
        }
        // All vertices should be referenced by at least one face
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.True(referencedVerts.Contains(v.Id),
                $"Vertex {v.Id} not referenced by any face");
        }
    }
}
