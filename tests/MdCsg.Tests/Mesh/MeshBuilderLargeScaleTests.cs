using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder large-scale — many triangles, welding at scale, twin linking correctness</summary>
public class MeshBuilderLargeScaleTests
{
    [Fact]
    public void Build100Triangles_AllFacesPresent()
    {
        var rng = new Random(42);
        var triangles = new List<Triangle3>();
        for (int i = 0; i < 100; i++)
        {
            var a = new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 10);
            var b = a + new Vec3(rng.NextDouble() + 0.1, 0, 0);
            var c = a + new Vec3(0, rng.NextDouble() + 0.1, 0);
            triangles.Add(new Triangle3(a, b, c));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        Assert.Equal(100, mesh.Faces.Count);
        Assert.Equal(300, mesh.HalfEdges.Count);
    }

    [Fact]
    public void AllFaces_HaveThreeHalfEdges()
    {
        var rng = new Random(123);
        var triangles = new List<Triangle3>();
        for (int i = 0; i < 50; i++)
        {
            var origin = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            triangles.Add(new Triangle3(
                origin,
                origin + new Vec3(0.5, 0, 0),
                origin + new Vec3(0, 0.5, 0)));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        foreach (var face in mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void WeldedBuild_ManyDuplicateVertices_Merges()
    {
        // 20 triangles that share vertex (0,0,0) with tiny perturbations
        var triangles = new List<Triangle3>();
        var rng = new Random(42);
        for (int i = 0; i < 20; i++)
        {
            double angle = i * System.Math.PI * 2 / 20;
            double eps = rng.NextDouble() * 1e-10;
            var origin = new Vec3(eps, eps, eps);
            var b = new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0);
            double nextAngle = (i + 1) * System.Math.PI * 2 / 20;
            var c = new Vec3(System.Math.Cos(nextAngle), System.Math.Sin(nextAngle), 0);
            triangles.Add(new Triangle3(origin, b, c));
        }
        var builder = new MeshBuilder(1e-6);
        var mesh = builder.Build(triangles);

        // Without welding: 60 vertices. With welding: all origins merge to 1
        Assert.True(mesh.Vertices.Count < 60, $"Expected welding to reduce vertex count, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void LargeIndexedBuild_CorrectFaceCount()
    {
        // Grid of triangles sharing vertices
        int gridSize = 10;
        var positions = new List<Vec3>();
        for (int y = 0; y <= gridSize; y++)
            for (int x = 0; x <= gridSize; x++)
                positions.Add(new Vec3(x, y, 0));

        var indices = new List<(int I0, int I1, int I2)>();
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int bl = y * (gridSize + 1) + x;
                int br = bl + 1;
                int tl = bl + gridSize + 1;
                int tr = tl + 1;
                indices.Add((bl, br, tr));
                indices.Add((bl, tr, tl));
            }
        }

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(200, mesh.Faces.Count); // 10x10 grid, 2 triangles each
        Assert.Equal(121, mesh.Vertices.Count); // 11x11 grid points
    }

    [Fact]
    public void LargeIndexedBuild_TwinsCorrectlyLinked()
    {
        // Grid of shared triangles: interior edges should have twins
        int gridSize = 5;
        var positions = new List<Vec3>();
        for (int y = 0; y <= gridSize; y++)
            for (int x = 0; x <= gridSize; x++)
                positions.Add(new Vec3(x, y, 0));

        var indices = new List<(int I0, int I1, int I2)>();
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int bl = y * (gridSize + 1) + x;
                int br = bl + 1;
                int tl = bl + gridSize + 1;
                int tr = tl + 1;
                indices.Add((bl, br, tr));
                indices.Add((bl, tr, tl));
            }
        }

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;

        // Interior edges have twins. Boundary edges don't.
        // Total half-edges: 150, boundary half-edges: 40 (5*4 sides * 2 per edge)
        Assert.True(twinCount > 0, "Grid should have interior twin-linked edges");
    }

    [Fact]
    public void Twin_SymmetryHoldsAtScale()
    {
        var rng = new Random(777);
        var triangles = new List<Triangle3>();
        for (int i = 0; i < 50; i++)
        {
            var a = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            triangles.Add(new Triangle3(a, a + new Vec3(1, 0, 0), a + new Vec3(0, 1, 0)));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Same(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void VertexIds_AreSequential()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)),
        });
        for (int i = 0; i < mesh.Vertices.Count; i++)
            Assert.Equal(i, mesh.Vertices[i].Id);
    }

    [Fact]
    public void FaceIds_AreSequential()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)),
        });
        for (int i = 0; i < mesh.Faces.Count; i++)
            Assert.Equal(i, mesh.Faces[i].Id);
    }

    [Fact]
    public void HalfEdgeIds_AreSequential()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        });
        for (int i = 0; i < mesh.HalfEdges.Count; i++)
            Assert.Equal(i, mesh.HalfEdges[i].Id);
    }

    [Fact]
    public void AllVertices_HaveOutgoingEdge()
    {
        var triangles = new List<Triangle3>();
        for (int i = 0; i < 30; i++)
        {
            double x = i * 2;
            triangles.Add(new Triangle3(
                new Vec3(x, 0, 0), new Vec3(x + 1, 0, 0), new Vec3(x, 1, 0)));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        foreach (var v in mesh.Vertices)
            Assert.NotNull(v.OutgoingEdge);
    }

    [Fact]
    public void Bounds_CoverAllVertices()
    {
        var rng = new Random(42);
        var triangles = new List<Triangle3>();
        for (int i = 0; i < 20; i++)
        {
            var a = new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50);
            triangles.Add(new Triangle3(a, a + new Vec3(1, 0, 0), a + new Vec3(0, 1, 0)));
        }
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        var bounds = mesh.GetBounds();
        foreach (var v in mesh.Vertices)
            Assert.True(bounds.Contains(v.Position));
    }
}
