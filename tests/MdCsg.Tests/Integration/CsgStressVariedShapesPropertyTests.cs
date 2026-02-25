using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG stress with varied shapes — sphere+tetrahedron, different subdivisions, asymmetric ops</summary>
public class CsgStressVariedShapesPropertyTests
{
    [Fact]
    public void Union_SphereTetrahedron_ProducesFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.5, 1);
        var tet = MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(sphere, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SphereTetrahedron_ProducesFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 1);
        var tet = MeshFactory.CreateTetrahedron();
        var result = Csg.Intersect(sphere, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereTetrahedron_ProducesFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 1);
        var tet = MeshFactory.CreateTetrahedron();
        var result = Csg.Difference(sphere, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DifferentSubdivisionSpheres_ProducesFaces()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var s2 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(s1, s2);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_DifferentSubdivisionSpheres_ProducesFaces()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var s2 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(s1, s2);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeTetrahedron_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var tet = MeshFactory.CreateTetrahedron(new Vec3(1, 1, 1));
        var result = Csg.Union(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeTetrahedron_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var tet = MeshFactory.CreateTetrahedron(new Vec3(1, 1, 1));
        var result = Csg.Difference(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_SmallSphereInsideLargeCube_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 10.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 5, 5), 0.5, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_LargeSphereSmallCube_ProducesFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 10.0, 1);
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var result = Csg.Intersect(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_AllResultsFacesAreTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Difference_AllResultsFacesAreTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Difference(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Union_ResultNormals_NonZero()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20);
        }
    }

    [Fact]
    public void Difference_CubeFromSphere_ProducesFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var cube = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 1.0);
        var result = Csg.Difference(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }
}
