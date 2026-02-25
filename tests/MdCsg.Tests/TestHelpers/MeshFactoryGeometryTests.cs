using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.TestHelpers;

/// <summary>Phase 6: MeshFactory geometry — cube/sphere/tetrahedron geometric properties, volume, bounds, normals</summary>
public class MeshFactoryGeometryTests
{
    [Fact]
    public void Cube_Volume_IsOne()
    {
        var solid = MeshFactory.CreateCube();
        double vol = VolumeCalculator.ComputeAbsoluteVolume(solid.Mesh);
        Assert.Equal(1.0, vol, 1e-10);
    }

    [Fact]
    public void Cube_Volume_ScalesWithSize()
    {
        var solid = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(solid.Mesh);
        Assert.Equal(8.0, vol, 1e-10);
    }

    [Fact]
    public void Cube_Bounds_CorrectForOffset()
    {
        var offset = new Vec3(3, 4, 5);
        var solid = MeshFactory.CreateCube(offset);
        var bounds = solid.Mesh.GetBounds();
        Assert.Equal(3, bounds.Min.X, 1e-10);
        Assert.Equal(4, bounds.Min.Y, 1e-10);
        Assert.Equal(5, bounds.Min.Z, 1e-10);
        Assert.Equal(4, bounds.Max.X, 1e-10);
        Assert.Equal(5, bounds.Max.Y, 1e-10);
        Assert.Equal(6, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void Cube_Has12Faces()
    {
        var solid = MeshFactory.CreateCube();
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Cube_Has8Vertices()
    {
        var solid = MeshFactory.CreateCube();
        Assert.Equal(8, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Cube_AllNormals_PointOutward()
    {
        var solid = MeshFactory.CreateCube();
        var center = new Vec3(0.5, 0.5, 0.5);
        foreach (var face in solid.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            var faceCentroid = tri.Centroid;
            var outward = faceCentroid - center;
            // Normal should point outward (positive dot with outward direction)
            Assert.True(Vec3.Dot(tri.Normal, outward) > 0,
                $"Face normal should point outward at {faceCentroid}");
        }
    }

    [Fact]
    public void Sphere_Volume_ApproximatesAnalytic()
    {
        double radius = 1.0;
        var solid = MeshFactory.CreateSphere(Vec3.Zero, radius, 3);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(solid.Mesh);
        double expected = (4.0 / 3.0) * System.Math.PI * radius * radius * radius;
        // Icosphere at subdiv 3 should approximate well
        Assert.True(System.Math.Abs(vol - expected) / expected < 0.02,
            $"Volume {vol} should approximate {expected} within 2%");
    }

    [Fact]
    public void Sphere_Bounds_Approximate()
    {
        double radius = 2.0;
        var center = new Vec3(1, 2, 3);
        var solid = MeshFactory.CreateSphere(center, radius, 2);
        var bounds = solid.Mesh.GetBounds();
        // Bounds should contain center ± radius (approximately)
        Assert.True(bounds.Min.X <= center.X - radius + 0.1);
        Assert.True(bounds.Max.X >= center.X + radius - 0.1);
    }

    [Fact]
    public void Sphere_AllVertices_OnSurface()
    {
        double radius = 1.5;
        var center = new Vec3(1, 2, 3);
        var solid = MeshFactory.CreateSphere(center, radius, 2);
        foreach (var v in solid.Mesh.Vertices)
        {
            double dist = Vec3.Distance(v.Position, center);
            Assert.Equal(radius, dist, 1e-10);
        }
    }

    [Fact]
    public void Sphere_FaceCount_Increases_WithSubdivision()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var s3 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.True(s2.Mesh.Faces.Count > s1.Mesh.Faces.Count);
        Assert.True(s3.Mesh.Faces.Count > s2.Mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_FaceCount_QuadruplesPerSubdiv()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        // Each subdivision quadruples faces: 20 -> 80 -> 320 -> 1280
        Assert.Equal(s1.Mesh.Faces.Count * 4, s2.Mesh.Faces.Count);
    }

    [Fact]
    public void Tetrahedron_Has4Faces()
    {
        var solid = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Tetrahedron_Has4Vertices()
    {
        var solid = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Tetrahedron_Volume_Positive()
    {
        var solid = MeshFactory.CreateTetrahedron();
        double vol = VolumeCalculator.ComputeAbsoluteVolume(solid.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var solid = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(solid.Mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Cube_Volume_WithOffset_Unchanged()
    {
        var v1 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube().Mesh);
        var v2 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(new Vec3(10, 20, 30)).Mesh);
        Assert.Equal(v1, v2, 1e-10);
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = MeshValidator.Validate(solid.Mesh);
        Assert.True(result.IsClosedManifold);
    }
}
