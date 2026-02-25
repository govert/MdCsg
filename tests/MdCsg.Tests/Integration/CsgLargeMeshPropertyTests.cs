using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with larger meshes — stress tests for the full pipeline</summary>
public class CsgLargeMeshPropertyTests
{
    [Fact]
    public void Union_HighSubdivisionSpheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, "Large sphere union should produce faces");
    }

    [Fact]
    public void Intersect_HighSubdivisionSpheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Intersect(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, "Large sphere intersection should produce faces");
    }

    [Fact]
    public void Difference_HighSubdivisionSpheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, "Large sphere difference should produce faces");
    }

    [Fact]
    public void Union_SphereCube_LargerSphere_ProducesFaces()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 3);
        var cube = MeshFactory.CreateCube();
        var result = Csg.Union(sphere, cube);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Difference_CubeMinusLargeSphere_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(size: 2);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 1, 1), 0.8, 3);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_LargeSpheres_FacesHaveValidCycles()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do { count++; current = current.Next; } while (current != start && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Intersect_LargeSpheres_AllFaceNormalsNonZero()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Intersect(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var normal = face.Normal;
            Assert.True(normal.Length > 0, $"Face {face.Id} should have non-zero normal");
        }
    }

    [Fact]
    public void Union_LargeSpheres_HasManyIntersectionSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 10,
            $"Large overlapping spheres should have many intersection segments, got {result.IntersectionSegmentCount}");
    }

    [Fact]
    public void Difference_TetrahedronMinusSphere_ProducesFaces()
    {
        var tet = MeshFactory.CreateTetrahedron(size: 2);
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2);
        var result = Csg.Difference(tet, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_ThreeOperationsSequential_ProducesFaces()
    {
        // A ∪ B, then result ∪ C — chain of operations
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Union(a, b);
        // Build a new Solid from the union result
        var abSolid = new Solid(ab.Mesh);
        var c = MeshFactory.CreateCube(new Vec3(0, 0.5, 0));
        var abc = Csg.Union(abSolid, c);
        Assert.True(abc.Mesh.Faces.Count > 0, "Chained union should produce faces");
    }

    [Fact]
    public void Intersect_SphereTetrahedron_HasFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.5, 2);
        var tet = MeshFactory.CreateTetrahedron();
        var result = Csg.Intersect(sphere, tet);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_LargeSpheres_ResultHasMoreFacesThanEitherInput()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var b = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > a.Mesh.Faces.Count,
            $"Union should have more faces ({result.Mesh.Faces.Count}) than A ({a.Mesh.Faces.Count})");
    }
}
