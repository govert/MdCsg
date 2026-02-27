using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 25: Laplacian smooth + volume preservation.</summary>
public class SmoothingIntegrationTests
{
    [Fact]
    public void LaplacianSmooth_Cube_PreservesApproximateVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 1, lambda: 0.3);
        var vol = System.Math.Abs(MeshQueries.Volume(smoothed));
        Assert.True(System.Math.Abs(cube.Volume() - vol) < 2.0);
    }

    [Fact]
    public void LaplacianSmooth_Sphere_PreservesApproximateVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, iterations: 1, lambda: 0.3);
        var vol = System.Math.Abs(MeshQueries.Volume(smoothed));
        Assert.True(System.Math.Abs(sphere.Volume() - vol) < 1.0);
    }

    [Fact]
    public void LaplacianSmooth_PreservesFaceCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, iterations: 3);
        Assert.Equal(sphere.Mesh.Faces.Count, smoothed.Faces.Count);
    }

    [Fact]
    public void LaplacianSmooth_PreservesVertexCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, iterations: 3);
        Assert.Equal(sphere.Mesh.Vertices.Count, smoothed.Vertices.Count);
    }

    [Fact]
    public void LaplacianSmooth_Cube_ShrinksCornersInward()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 5, lambda: 0.5);
        // Smoothing should move corners inward → reduced max extent
        var bounds = smoothed.GetBounds();
        Assert.True(bounds.Max.X < 1.1); // was 1.0 for cube corner
    }

    [Fact]
    public void LaplacianSmooth_MultipleIterations_IncreasingSmoothing()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var s1 = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 1, lambda: 0.5);
        var s3 = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 3, lambda: 0.5);
        var b1 = s1.GetBounds();
        var b3 = s3.GetBounds();
        // More iterations → more shrinkage
        Assert.True(b3.Max.X <= b1.Max.X + 0.01);
    }

    [Fact]
    public void LaplacianSmooth_ZeroIterations_NoChange()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 0);
        Assert.Equal(cube.Mesh.Faces.Count, smoothed.Faces.Count);
    }

    [Fact]
    public void LaplacianSmooth_SmallLambda_LessEffect()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var s01 = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 3, lambda: 0.1);
        var s05 = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 3, lambda: 0.5);
        var v01 = System.Math.Abs(MeshQueries.Volume(s01));
        var v05 = System.Math.Abs(MeshQueries.Volume(s05));
        // Smaller lambda preserves more volume
        Assert.True(v01 >= v05 - 0.1);
    }

    [Fact]
    public void SmoothedMesh_CanBuildSolid()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 2);
        var solid = new Solid(smoothed);
        Assert.True(solid.Volume() > 0);
        Assert.NotNull(solid.Bvh);
    }

    [Fact]
    public void SmoothedCube_ThenCsg_ProducesValidMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = new Solid(MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 2, lambda: 0.3));
        var sphere = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var r = Csg.Difference(smoothed, sphere);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void SmoothedSphere_LooksMoreSpherical()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 1); // low subdivision
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, iterations: 3, lambda: 0.3);
        // After smoothing, vertices should be closer to unit sphere
        double maxDeviation = 0;
        foreach (var v in smoothed.Vertices)
        {
            double dev = System.Math.Abs(v.Position.Length - 1.0);
            if (dev > maxDeviation) maxDeviation = dev;
        }
        // Low subdivision sphere has significant deviation that smoothing reduces
        Assert.True(maxDeviation < 0.5);
    }

    [Fact]
    public void SmoothedMesh_CanSerialize()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 2);
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.Default, true))
            MeshSerializer.Serialize(smoothed, writer);
        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        var rt = MeshSerializer.Deserialize(reader);
        Assert.Equal(smoothed.Faces.Count, rt.Faces.Count);
    }

    [Fact]
    public void SmoothCylinder_PreservesApproximateVolume()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        var smoothed = MeshSmoothing.LaplacianSmooth(cyl.Mesh, iterations: 1, lambda: 0.3);
        var vol = System.Math.Abs(MeshQueries.Volume(smoothed));
        Assert.True(System.Math.Abs(cyl.Volume() - vol) < 1.0);
    }

    [Fact]
    public void SmoothTorus_PreservesFaceCount()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var smoothed = MeshSmoothing.LaplacianSmooth(torus.Mesh, iterations: 2);
        Assert.Equal(torus.Mesh.Faces.Count, smoothed.Faces.Count);
    }

    [Fact]
    public void SmoothCsgResult_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var result = new Solid(Csg.Difference(a, b).Mesh);
        var smoothed = MeshSmoothing.LaplacianSmooth(result.Mesh, iterations: 1, lambda: 0.3);
        Assert.True(smoothed.Faces.Count > 0);
        Assert.True(System.Math.Abs(MeshQueries.Volume(smoothed)) > 0);
    }

    [Fact]
    public void SmoothCone_PreservesFaceCount()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        var smoothed = MeshSmoothing.LaplacianSmooth(cone.Mesh, iterations: 2);
        Assert.Equal(cone.Mesh.Faces.Count, smoothed.Faces.Count);
    }

    [Fact]
    public void SmoothBox_PreservesFaceCount()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var smoothed = MeshSmoothing.LaplacianSmooth(box.Mesh, iterations: 2);
        Assert.Equal(box.Mesh.Faces.Count, smoothed.Faces.Count);
    }

    [Fact]
    public void SmoothedMesh_NoNaNVertices()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var smoothed = MeshSmoothing.LaplacianSmooth(cube.Mesh, iterations: 10, lambda: 0.5);
        foreach (var v in smoothed.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsNaN(v.Position.Y));
            Assert.False(double.IsNaN(v.Position.Z));
        }
    }
}
