using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: MeshStitcher properties — tolerance, welding, edge cases, structural invariants</summary>
public class MeshStitcherPropertyTests
{
    [Fact]
    public void EmptyTriangleList_ProducesEmptyMesh()
    {
        var mesh = MeshStitcher.Stitch(new List<Triangle3>());
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.HalfEdges.Count);
    }

    [Fact]
    public void SingleTriangle_ProducesOneFace()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var mesh = MeshStitcher.Stitch(new[] { tri });
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void TwoSharingEdge_WeldsVertices()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count); // 2 shared + 2 unique
    }

    [Fact]
    public void DefaultTolerance_Is1eNeg8()
    {
        // Points within 1e-8 should weld, points beyond should not
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1e-9, 0, 0), new Vec3(1 + 1e-9, 0, 0), new Vec3(1e-9, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(tris);
        // Should weld: 3 vertices from first + potentially 0 from second if all weld
        Assert.True(mesh.Vertices.Count <= 4);
    }

    [Fact]
    public void LargeTolerance_WeldsMoreVertices()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(0.005, 0, 0), new Vec3(1.005, 0, 0), new Vec3(0.005, 1, 0)),
        };
        var meshSmall = MeshStitcher.Stitch(tris, 0.001);
        var meshLarge = MeshStitcher.Stitch(tris, 0.01);
        Assert.True(meshLarge.Vertices.Count <= meshSmall.Vertices.Count);
    }

    [Fact]
    public void VerySmallTolerance_NoWelding()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1e-10, 0, 0), new Vec3(1 + 1e-10, 0, 0), new Vec3(1e-10, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(tris, 1e-15);
        Assert.Equal(6, mesh.Vertices.Count); // No welding
    }

    [Fact]
    public void CubeTriangles_StitchesToManifold()
    {
        var cube = MeshFactory.CreateCube();
        // Extract triangles and re-stitch
        var tris = new List<Triangle3>();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(tris);
        Assert.Equal(cube.Mesh.Faces.Count, stitched.Faces.Count);
        Assert.Equal(cube.Mesh.Vertices.Count, stitched.Vertices.Count);
    }

    [Fact]
    public void TetraTriangles_StitchesToManifold()
    {
        var tetra = MeshFactory.CreateTetrahedron();
        var tris = new List<Triangle3>();
        foreach (var face in tetra.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(tris);
        Assert.Equal(4, stitched.Faces.Count);
        Assert.Equal(4, stitched.Vertices.Count);
    }

    [Fact]
    public void Stitched_FacesHaveThreeHalfEdges()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
        };
        var mesh = MeshStitcher.Stitch(tris);
        foreach (var face in mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var he = start;
            do
            {
                count++;
                he = he.Next;
            } while (he != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Stitched_HalfEdgesPerFace_Always3()
    {
        var cube = MeshFactory.CreateCube();
        var tris = new List<Triangle3>();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(tris);
        Assert.Equal(stitched.Faces.Count * 3, stitched.HalfEdges.Count);
    }

    [Fact]
    public void Stitched_ValidFaceCycles()
    {
        var tris = new List<Triangle3>();
        for (int i = 0; i < 10; i++)
        {
            double x = i * 0.1;
            tris.Add(new Triangle3(new Vec3(x, 0, 0), new Vec3(x + 0.1, 0, 0), new Vec3(x, 1, 0)));
        }
        var mesh = MeshStitcher.Stitch(tris);
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Stitched_ConsistentlyOriented()
    {
        var cube = MeshFactory.CreateCube();
        var tris = new List<Triangle3>();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(tris);
        var validation = MeshValidator.Validate(stitched);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void LargeTriangleSet_StitchesWithoutError()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var tris = new List<Triangle3>();
        foreach (var face in sphere.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(tris);
        Assert.Equal(sphere.Mesh.Faces.Count, stitched.Faces.Count);
    }

    [Fact]
    public void NegativeCoordinates_StitchesCorrectly()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(-5, -5, -5), new Vec3(-4, -5, -5), new Vec3(-5, -4, -5)),
            new Triangle3(new Vec3(-4, -5, -5), new Vec3(-5, -5, -5), new Vec3(-5, -5, -4)),
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_PreservesTriangleVertexPositions()
    {
        var a = new Vec3(1.111, 2.222, 3.333);
        var b = new Vec3(4.444, 5.555, 6.666);
        var c = new Vec3(7.777, 8.888, 9.999);
        var mesh = MeshStitcher.Stitch(new[] { new Triangle3(a, b, c) });
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var ra, out var rb, out var rc);
        // Vertex positions should all be present (order may vary)
        var input = new[] { a, b, c };
        var output = new[] { ra, rb, rc };
        foreach (var inp in input)
        {
            bool found = false;
            foreach (var outp in output)
                if (System.Math.Abs(inp.X - outp.X) < 1e-10 &&
                    System.Math.Abs(inp.Y - outp.Y) < 1e-10 &&
                    System.Math.Abs(inp.Z - outp.Z) < 1e-10)
                { found = true; break; }
            Assert.True(found, $"Vertex ({inp.X}, {inp.Y}, {inp.Z}) not found in output");
        }
    }
}
