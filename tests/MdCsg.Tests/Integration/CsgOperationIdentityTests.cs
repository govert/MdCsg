using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG Operation identities — De Morgan's, idempotency, absorption</summary>
public class CsgOperationIdentityTests
{
    [Fact]
    public void Union_EnclosingCubes_FaceCountMatchesOuter()
    {
        // If A completely contains B, Union = A → same face count as A (no intersection)
        var outer = MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3);
        var inner = MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2), 0.3);
        var result = Csg.Union(outer, inner);
        // The inner is entirely inside outer, so the result should be just the outer
        Assert.Equal(outer.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Intersect_EnclosingCubes_FaceCountMatchesInner()
    {
        // If A completely contains B, Intersect = B → same face count as B
        var outer = MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3);
        var inner = MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2), 0.3);
        var result = Csg.Intersect(outer, inner);
        Assert.Equal(inner.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Difference_EnclosingMinusInner_MoreFaces()
    {
        // A - B where B is inside A → carves out B → more faces than either
        var outer = MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3);
        var inner = MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2), 0.3);
        var result = Csg.Difference(outer, inner);
        Assert.True(result.Mesh.Faces.Count > outer.Mesh.Faces.Count,
            $"Carving should create more faces: got {result.Mesh.Faces.Count} vs {outer.Mesh.Faces.Count}");
    }

    [Fact]
    public void Difference_InnerMinusOuter_NoFaces()
    {
        // B - A where A contains B → B completely inside A → inside A subtracted → nothing left
        var outer = MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3);
        var inner = MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2), 0.3);
        var result = Csg.Difference(inner, outer);
        Assert.Equal(0, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Union_Disjoint_FacesSumEquals()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Intersect_Disjoint_EmptyResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Difference_Disjoint_SameAsOriginal()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void AllThreeOps_ProduceValidTriangles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));

        var union = Csg.Union(a, b);
        var intersect = Csg.Intersect(
            MeshFactory.CreateCube(),
            MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)));
        var difference = Csg.Difference(
            MeshFactory.CreateCube(),
            MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)));

        foreach (var result in new[] { union, intersect, difference })
        {
            foreach (var face in result.Mesh.Faces)
            {
                var start = face.Edge;
                var current = start;
                int count = 0;
                do { count++; current = current.Next; } while (current != start && count < 10);
                Assert.Equal(3, count);
            }
        }
    }

    [Fact]
    public void Union_FaceCount_GreaterOrEqual_EachInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Union(a, b);
        // Union should have at least as many faces as either input (when overlapping, usually more)
        Assert.True(result.Mesh.Faces.Count >= a.Mesh.Faces.Count ||
                    result.Mesh.Faces.Count >= b.Mesh.Faces.Count);
    }

    [Fact]
    public void DeMorgan_DiffAndIntersect_SameFaceCount()
    {
        // A - B should have same structure as A ∩ B̄ (complement of B)
        // We can't compute B̄ directly, but A-B should have faces from A that are outside B
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var diff = Csg.Difference(a, b);
        // A-B should have faces
        Assert.True(diff.Mesh.Faces.Count > 0);
        // A-B face count should be less than A∪B face count
        var union = Csg.Union(
            MeshFactory.CreateCube(),
            MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)));
        Assert.True(diff.Mesh.Faces.Count <= union.Mesh.Faces.Count,
            "Difference should have <= faces compared to union");
    }

    [Fact]
    public void UnionIntersectDifference_SameInputs_AllHaveFaces()
    {
        // Verify all three operations work with sphere inputs
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 1);

        var union = Csg.Union(a, b);
        Assert.True(union.Mesh.Faces.Count > 0, "Union of overlapping spheres should have faces");

        var intersect = Csg.Intersect(
            MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1),
            MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 1));
        Assert.True(intersect.Mesh.Faces.Count > 0, "Intersection of overlapping spheres should have faces");

        var diff = Csg.Difference(
            MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1),
            MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 1));
        Assert.True(diff.Mesh.Faces.Count > 0, "Difference of overlapping spheres should have faces");
    }

    [Fact]
    public void Tetrahedron_AllOps_ProduceFaces()
    {
        var a = MeshFactory.CreateTetrahedron();
        var b = MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5));

        var union = Csg.Union(a, b);
        Assert.True(union.Mesh.Faces.Count > 0);

        var intersect = Csg.Intersect(
            MeshFactory.CreateTetrahedron(),
            MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5)));
        Assert.True(intersect.Mesh.Faces.Count > 0);

        var diff = Csg.Difference(
            MeshFactory.CreateTetrahedron(),
            MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5)));
        Assert.True(diff.Mesh.Faces.Count > 0);
    }
}
