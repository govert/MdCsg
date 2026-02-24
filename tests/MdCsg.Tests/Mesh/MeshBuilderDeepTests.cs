using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder deep tests — welding, twin linking, degenerate geometry</summary>
public class MeshBuilderDeepTests
{
    [Fact]
    public void Build_SingleTriangle_3Vertices3HalfEdges()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Single(mesh.Faces);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_TwoTriangles_SharedEdge_WeldsVertices()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count); // 4 unique vertices
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_TwoTriangles_SharedEdge_TwinsLinked()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(tris);

        // The shared edge should have twins linked
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) twinCount++;
        }
        Assert.True(twinCount >= 2, "At least 2 half-edges should have twins (the shared edge)");
    }

    [Fact]
    public void Build_Cube_8Vertices_12Faces()
    {
        // Build cube manually from triangle soup
        var tris = MakeCubeTriangles();
        var builder = new MeshBuilder(weldTolerance: 1e-8);
        var mesh = builder.Build(tris);
        Assert.Equal(8, mesh.Vertices.Count);
        Assert.Equal(12, mesh.Faces.Count);
        Assert.Equal(36, mesh.HalfEdges.Count); // 12 faces * 3 half-edges
    }

    [Fact]
    public void Build_Cube_AllTwinsLinked()
    {
        var tris = MakeCubeTriangles();
        var builder = new MeshBuilder(weldTolerance: 1e-8);
        var mesh = builder.Build(tris);

        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
        }
    }

    [Fact]
    public void Build_WeldTolerance_VerticesWithinToleranceMerge()
    {
        double eps = 1e-12;
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1 + eps, 0, 0), new Vec3(0, 0, eps), new Vec3(0.5, -1, 0)),
        };
        var builder = new MeshBuilder(weldTolerance: 1e-10);
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count); // Should weld the close vertices
    }

    [Fact]
    public void Build_WeldTolerance_VerticesBeyondToleranceSeparate()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1.001, 0, 0), new Vec3(0.001, 0, 0), new Vec3(0.5, -1, 0)),
        };
        var builder = new MeshBuilder(weldTolerance: 1e-10);
        var mesh = builder.Build(tris);
        Assert.Equal(6, mesh.Vertices.Count); // Should NOT weld — too far apart
    }

    [Fact]
    public void Build_FromIndexed_NoWelding_TrustsIndices()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 2, 3) };

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_FromIndexed_TwinsLinked()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        // Two triangles sharing edge 0-2
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 2, 3) };

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2, "Shared edge should have twins");
    }

    [Fact]
    public void Build_DuplicateTriangles_HandledGracefully()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { tri, tri });
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_OpenMesh_SomeEdgesWithoutTwins()
    {
        // Single triangle is an open mesh
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(tris);

        // Open mesh — boundary edges have no twins
        int nullTwinCount = mesh.HalfEdges.Count(he => he.Twin == null);
        Assert.Equal(3, nullTwinCount);
    }

    [Fact]
    public void Build_FaceCycleValid_NextPrevConsistent()
    {
        var tris = MakeCubeTriangles();
        var builder = new MeshBuilder(weldTolerance: 1e-8);
        var mesh = builder.Build(tris);

        foreach (var face in mesh.Faces)
        {
            var he = face.Edge;
            var current = he;
            int count = 0;
            do
            {
                Assert.Equal(current, current.Next.Prev);
                Assert.Equal(face, current.Face);
                current = current.Next;
                count++;
            } while (current != he && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Build_VertexOutgoingEdge_OriginMatches()
    {
        var tris = MakeCubeTriangles();
        var builder = new MeshBuilder(weldTolerance: 1e-8);
        var mesh = builder.Build(tris);

        foreach (var v in mesh.Vertices)
        {
            Assert.NotNull(v.OutgoingEdge);
            Assert.Equal(v.Id, v.OutgoingEdge.Origin.Id);
        }
    }

    [Fact]
    public void Build_TwinSymmetry()
    {
        var tris = MakeCubeTriangles();
        var builder = new MeshBuilder(weldTolerance: 1e-8);
        var mesh = builder.Build(tris);

        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Equal(he, he.Twin.Twin);
                Assert.Equal(he.Origin.Id, he.Twin.Target.Id);
                Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
            }
        }
    }

    [Fact]
    public void MeshValidator_Validate_ClosedCube_AllChecksPass()
    {
        var tris = MakeCubeTriangles();
        var builder = new MeshBuilder(weldTolerance: 1e-8);
        var mesh = builder.Build(tris);

        var result = MeshValidator.Validate(mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.Equal(2, result.EulerCharacteristic);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidator_Validate_OpenMesh_NotClosed()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(tris);

        var result = MeshValidator.Validate(mesh);
        Assert.False(result.AllEdgesHaveTwins);
        Assert.False(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidator_Sphere_IsClosedManifold()
    {
        var mesh = TestHelpers.MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void MeshValidator_Tetrahedron_IsClosedManifold()
    {
        var mesh = TestHelpers.MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(6, result.EdgeCount);
        Assert.Equal(4, result.FaceCount);
    }

    [Fact]
    public void MeshValidator_CsgResult_IsClosedManifold()
    {
        var a = new MdCsg.Api.Solid(TestHelpers.MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(TestHelpers.MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
        Assert.True(validation.IsConsistentlyOriented);
    }

    private static IReadOnlyList<Triangle3> MakeCubeTriangles()
    {
        var p = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0),
            new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1),
        };
        return new Triangle3[]
        {
            new(p[0], p[2], p[1]), new(p[0], p[3], p[2]),   // bottom
            new(p[4], p[5], p[6]), new(p[4], p[6], p[7]),   // top
            new(p[0], p[1], p[5]), new(p[0], p[5], p[4]),   // front
            new(p[2], p[3], p[7]), new(p[2], p[7], p[6]),   // back
            new(p[0], p[4], p[7]), new(p[0], p[7], p[3]),   // left
            new(p[1], p[2], p[6]), new(p[1], p[6], p[5]),   // right
        };
    }
}
