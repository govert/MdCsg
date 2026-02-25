using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — large mesh construction, vertex welding at scale, structural properties</summary>
public class MeshBuilderLargeMeshPropertyTests
{
    [Fact]
    public void Build_100Triangles_CorrectFaceCount()
    {
        var triangles = new Triangle3[100];
        for (int i = 0; i < 100; i++)
        {
            double x = i * 3.0;
            triangles[i] = new Triangle3(
                new Vec3(x, 0, 0),
                new Vec3(x + 1, 0, 0),
                new Vec3(x, 1, 0));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        Assert.Equal(100, mesh.Faces.Count);
    }

    [Fact]
    public void Build_100Triangles_AllFaceCyclesLength3()
    {
        var triangles = new Triangle3[100];
        for (int i = 0; i < 100; i++)
        {
            double x = i * 3.0;
            triangles[i] = new Triangle3(
                new Vec3(x, 0, 0),
                new Vec3(x + 1, 0, 0),
                new Vec3(x, 1, 0));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do { count++; current = current.Next; } while (current != start && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Build_GridMesh_TwinsLinked()
    {
        // Build a 5x5 grid of quads (each = 2 triangles) with shared edges
        var positions = new List<Vec3>();
        for (int y = 0; y <= 5; y++)
            for (int x = 0; x <= 5; x++)
                positions.Add(new Vec3(x, y, 0));

        var indices = new List<(int, int, int)>();
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                int i = y * 6 + x;
                indices.Add((i, i + 1, i + 6));
                indices.Add((i + 1, i + 7, i + 6));
            }
        }

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(50, mesh.Faces.Count);

        // Interior edges should have twins
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount > 0, "Grid mesh should have twin-linked edges");
    }

    [Fact]
    public void Build_Sphere_Subdiv2_CorrectFaceCount()
    {
        // Icosphere with 2 subdivisions: 20 * 4^2 = 320 faces
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(320, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Build_Sphere_Subdiv3_CorrectFaceCount()
    {
        // Icosphere with 3 subdivisions: 20 * 4^3 = 1280 faces
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.Equal(1280, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Build_WeldedStrip_FewerVertices()
    {
        // Triangle strip with shared edges — welding should merge shared vertices
        var triangles = new Triangle3[10];
        for (int i = 0; i < 10; i++)
        {
            if (i % 2 == 0)
                triangles[i] = new Triangle3(
                    new Vec3(i, 0, 0),
                    new Vec3(i + 1, 0, 0),
                    new Vec3(i, 1, 0));
            else
                triangles[i] = new Triangle3(
                    new Vec3(i, 0, 0),
                    new Vec3(i + 1, 0, 0),
                    new Vec3(i + 1, 1, 0));
        }
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(triangles);
        Assert.Equal(10, mesh.Faces.Count);
        // Without welding would be 30 vertices (3 per tri), with welding should be fewer
        Assert.True(mesh.Vertices.Count < 30,
            $"Welding should reduce vertices from 30, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void Build_IndexedCube_AllTwinsLinked()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Build_Indexed_LargeGrid_CorrectCounts()
    {
        int n = 10;
        var positions = new List<Vec3>();
        for (int y = 0; y <= n; y++)
            for (int x = 0; x <= n; x++)
                positions.Add(new Vec3(x, y, 0));

        var indices = new List<(int, int, int)>();
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                int i = y * (n + 1) + x;
                indices.Add((i, i + 1, i + n + 1));
                indices.Add((i + 1, i + n + 2, i + n + 1));
            }
        }

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal((n + 1) * (n + 1), mesh.Vertices.Count);
        Assert.Equal(n * n * 2, mesh.Faces.Count);
        Assert.Equal(n * n * 2 * 3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_HighSubdivSphere_AllTwinsLinked()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        foreach (var he in sphere.Mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Build_HalfEdgeCount_Is3TimesFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(cube.Mesh.Faces.Count * 3, cube.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_AllFaces_HaveNonNullEdge()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
            Assert.NotNull(face.Edge);
    }

    [Fact]
    public void Build_HalfEdge_NextCycle_ClosesForAllFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
        {
            var start = face.Edge;
            var current = start.Next.Next.Next;
            Assert.Same(start, current);
        }
    }
}
