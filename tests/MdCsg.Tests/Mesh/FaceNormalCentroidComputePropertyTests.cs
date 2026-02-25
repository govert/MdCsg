using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face — Normal, UnitNormal, Centroid computed properties on real meshes</summary>
public class FaceNormalCentroidComputePropertyTests
{
    [Fact]
    public void CubeFaces_AllHaveNonZeroNormal()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20,
                $"Face {face.Id} has near-zero normal");
        }
    }

    [Fact]
    public void CubeFaces_UnitNormals_AreUnitLength()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var un = face.UnitNormal;
            Assert.True(System.Math.Abs(un.Length - 1.0) < 1e-10,
                $"Face {face.Id} unit normal length is {un.Length}");
        }
    }

    [Fact]
    public void CubeFaces_CentroidInsideBounds()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bounds = cube.Bounds;
        foreach (var face in cube.Mesh.Faces)
        {
            var centroid = face.Centroid;
            Assert.True(centroid.X >= bounds.Min.X - 0.01 && centroid.X <= bounds.Max.X + 0.01);
            Assert.True(centroid.Y >= bounds.Min.Y - 0.01 && centroid.Y <= bounds.Max.Y + 0.01);
            Assert.True(centroid.Z >= bounds.Min.Z - 0.01 && centroid.Z <= bounds.Max.Z + 0.01);
        }
    }

    [Fact]
    public void CubeFaces_Normals_PointAwayFromCenter()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var center = new Vec3(1, 1, 1);
        foreach (var face in cube.Mesh.Faces)
        {
            var centroid = face.Centroid;
            var outward = centroid - center;
            var normal = face.Normal;
            // Normal should generally point away from center
            double dot = Vec3.Dot(normal, outward);
            Assert.True(dot > -0.1,
                $"Face {face.Id} normal ({normal}) may point inward (dot with outward = {dot})");
        }
    }

    [Fact]
    public void SphereFaces_AllHaveNonZeroNormal()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        foreach (var face in sphere.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20);
        }
    }

    [Fact]
    public void SphereFaces_CentroidsNearSurface()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
        {
            var centroid = face.Centroid;
            double dist = centroid.Length;
            Assert.True(dist > 0.8 && dist < 1.2,
                $"Face {face.Id} centroid distance from origin is {dist}, expected near 1.0");
        }
    }

    [Fact]
    public void Face_GetVertices_ThreeDistinctVertices()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
            Assert.NotEqual(verts[0].Id, verts[1].Id);
            Assert.NotEqual(verts[1].Id, verts[2].Id);
            Assert.NotEqual(verts[2].Id, verts[0].Id);
        }
    }

    [Fact]
    public void Face_GetTrianglePositions_MatchesVertices()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var verts = face.GetVertices();
            Assert.Equal(verts[0].Position, a);
            Assert.Equal(verts[1].Position, b);
            Assert.Equal(verts[2].Position, c);
        }
    }

    [Fact]
    public void Face_Centroid_IsAverageOfVertices()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var expected = (a + b + c) / 3.0;
            var actual = face.Centroid;
            Assert.True(Vec3.DistanceSquared(expected, actual) < 1e-20);
        }
    }

    [Fact]
    public void Face_Normal_IsCrossProduct()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var expected = Vec3.Cross(b - a, c - a);
            var actual = face.Normal;
            Assert.True(Vec3.DistanceSquared(expected, actual) < 1e-20);
        }
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultsToId()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.Equal(face.Id, face.OriginalFaceId);
        }
    }

    [Fact]
    public void Face_PatchId_DefaultsToNegativeOne()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.Equal(-1, face.PatchId);
        }
    }

    [Fact]
    public void TetrahedronFaces_UnitNormalsAreUnitLength()
    {
        var tet = MeshFactory.CreateTetrahedron();
        foreach (var face in tet.Mesh.Faces)
        {
            var un = face.UnitNormal;
            Assert.True(System.Math.Abs(un.Length - 1.0) < 1e-10);
        }
    }

    [Fact]
    public void Vertex_HasPosition()
    {
        var cube = MeshFactory.CreateCube(new Vec3(1, 2, 3), 2.0);
        foreach (var v in cube.Mesh.Vertices)
        {
            // All vertices should be within the cube bounds
            Assert.True(v.Position.X >= 0.99 && v.Position.X <= 3.01);
            Assert.True(v.Position.Y >= 1.99 && v.Position.Y <= 4.01);
            Assert.True(v.Position.Z >= 2.99 && v.Position.Z <= 5.01);
        }
    }

    [Fact]
    public void HalfEdge_Origin_DerivedFromPrev()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            // Origin should be Prev.Target
            Assert.Same(he.Prev.Target, he.Origin);
        }
    }
}
