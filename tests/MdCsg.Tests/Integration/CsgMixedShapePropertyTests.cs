using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG mixed-shape operations — cube-sphere, tetrahedron-sphere, cube-tetrahedron configurations</summary>
public class CsgMixedShapePropertyTests
{
    [Fact]
    public void Union_CubeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1.5, 0, 0), 1.0, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1.5, 0, 0), 1.0, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeMinusSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1.5, 0, 0), 1.0, 1);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereMinusCube_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1.5, 0, 0), 1.0, 1);
        var result = Csg.Difference(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DisjointCubeSphere_FaceCountIsSum()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(20, 0, 0), 1.0, 1);
        var result = Csg.Union(cube, sphere);
        Assert.Equal(cube.Mesh.Faces.Count + sphere.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointCubeSphere_Empty()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(20, 0, 0), 1.0, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_TetrahedronSphere_ProducesFaces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 0.5, 1);
        var result = Csg.Union(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_TetrahedronSphere_ProducesFaces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 0.5, 1);
        var result = Csg.Intersect(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_TetrahedronMinusSphere_ProducesFaces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 0.5, 1);
        var result = Csg.Difference(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeTetrahedron_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var tet = MeshFactory.CreateTetrahedron();
        var result = Csg.Union(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeSphere_AllFaceNormalsNonZero()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 1, 0), 1.0, 1);
        var result = Csg.Union(cube, sphere);
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20,
                $"Face {face.Id} has near-zero normal");
        }
    }

    [Fact]
    public void Union_CubeSphere_HasIntersectionSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_FewerFacesThanUnion()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var union = Csg.Union(cube, sphere);
        var intersect = Csg.Intersect(cube, sphere);
        Assert.True(intersect.FaceCount <= union.FaceCount,
            $"Intersection ({intersect.FaceCount}) should have <= faces than union ({union.FaceCount})");
    }

    [Fact]
    public void Union_WithWinding_CubeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(cube, sphere, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_ValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Intersect(cube, sphere);
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
}
