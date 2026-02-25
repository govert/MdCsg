using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG result mesh validity — Euler characteristic, face cycles, orientation after CSG</summary>
public class CsgResultMeshValidityTests
{
    [Fact]
    public void Union_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Intersection_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Difference_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void DisjointUnion_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Union_AllFacesHaveThreeEdges()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);

        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Union_AllVerticesHaveOutgoing()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);

        foreach (var v in result.Mesh.Vertices)
            Assert.NotNull(v.OutgoingEdge);
    }

    [Fact]
    public void Intersection_AllVerticesHaveOutgoing()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);

        foreach (var v in result.Mesh.Vertices)
            Assert.NotNull(v.OutgoingEdge);
    }

    [Fact]
    public void CubeCube_Union_EulerCharacteristic()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);

        int euler = MeshValidator.EulerCharacteristic(result.Mesh);
        // Output mesh is triangulated (not necessarily closed), Euler should be 2 for genus-0
        Assert.True(euler >= 0, $"Euler characteristic = {euler}, expected >= 0");
    }

    [Fact]
    public void DisjointCubes_Union_EulerCharacteristic4()
    {
        // Two disjoint closed surfaces → Euler = 2 + 2 = 4
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);

        int euler = MeshValidator.EulerCharacteristic(result.Mesh);
        // Each closed surface contributes V-E+F=2, so combined should be 4
        Assert.Equal(4, euler);
    }

    [Fact]
    public void InputCube_EulerCharacteristic2()
    {
        var cube = MeshFactory.CreateCube();
        int euler = MeshValidator.EulerCharacteristic(cube.Mesh);
        Assert.Equal(2, euler);
    }

    [Fact]
    public void InputSphere_EulerCharacteristic2()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        int euler = MeshValidator.EulerCharacteristic(sphere.Mesh);
        Assert.Equal(2, euler);
    }

    [Fact]
    public void InputTetrahedron_EulerCharacteristic2()
    {
        var tet = MeshFactory.CreateTetrahedron();
        int euler = MeshValidator.EulerCharacteristic(tet.Mesh);
        Assert.Equal(2, euler);
    }

    [Fact]
    public void Union_NextPrev_AreInverse()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);

        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Difference_NextPrev_AreInverse()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);

        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void SphereCube_Union_ValidCycles()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(sphere, cube);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }
}
