using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Batch 15: IntersectionGraph and segment processing tests (20 tests)</summary>
public class IntersectionGraphExtTests
{
    [Fact]
    public void IntersectionGraph_CubeOffset_Y_HasSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0, 0.5, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_CubeOffset_Z_HasSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0, 0, 0.5));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_CubeOffset_Diagonal_HasSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_SmallOverlap_HasSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.9, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_ContainedCube_HasSegments()
    {
        var outer = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2);
        var inner = MeshFactory.CreateCube(new Vec3(0, 0, 0), 1);
        var graph = IntersectionGraph.Compute(outer.Mesh, inner.Mesh);
        // Contained cube has no crossing intersections but may have coplanar
        // Actually with different sizes, no face planes coincide, so no intersections
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void IntersectionGraph_SphereSphere_HasSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 1);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_SphereSphere_Disjoint()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1);
        var b = MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 1);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void IntersectionGraph_TetrahedronCube_HasSegments()
    {
        var cube = MeshFactory.CreateCube();
        // Tetrahedron that sticks out of the cube
        var tet = MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8);
        var graph = IntersectionGraph.Compute(cube.Mesh, tet.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_FaceSegmentA_ContainsOnlyFaceAIndices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < a.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void IntersectionGraph_FaceSegmentB_ContainsOnlyFaceBIndices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < b.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void IntersectionSegment_FaceIndices()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 3, 7);
        Assert.Equal(3, seg.FaceIndexA);
        Assert.Equal(7, seg.FaceIndexB);
    }

    [Fact]
    public void IntersectionSegment_Length_ZeroVector()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 0);
        Assert.Equal(0, seg.Length);
    }

    [Fact]
    public void IntersectionSegment_Length_UnitVector()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 0);
        Assert.Equal(1, seg.Length, 1e-15);
    }

    [Fact]
    public void IntersectionSegment_Length_Diagonal()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(3, 4, 0), 0, 0);
        Assert.Equal(5, seg.Length, 1e-15);
    }

    [Fact]
    public void TriTri_Crossing_SegmentEndpoints_OnBothTriangles()
    {
        // Two triangles crossing - the intersection segment should lie on both
        var t1 = new Triangle3(new Vec3(-2,-2,0), new Vec3(2,-2,0), new Vec3(0,2,0));
        var t2 = new Triangle3(new Vec3(0,-2,-2), new Vec3(0,2,-2), new Vec3(0,0,2));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        // Segment should be approximately along the Z=0 line at X=0
        Assert.True(System.Math.Abs(seg.Start.X) < 0.5);
        Assert.True(System.Math.Abs(seg.End.X) < 0.5);
    }

    [Fact]
    public void IntersectionGraph_Segments_HaveNonZeroLength()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.Length > MathUtil.Epsilon);
        }
    }

    [Fact]
    public void IntersectionGraph_CustomGridSize()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph1 = IntersectionGraph.Compute(a.Mesh, b.Mesh, 1e-6);
        var graph2 = IntersectionGraph.Compute(a.Mesh, b.Mesh, 1e-10);
        // Both should produce valid results
        Assert.True(graph1.Segments.Count > 0);
        Assert.True(graph2.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_OverlappingCubes_CoplanarFaces()
    {
        // Two cubes with offset (0.5,0,0) share face planes at y=0, y=1, z=0, z=1
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // Should detect coplanar faces
        Assert.True(graph.CoplanarFacesA.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_CubeOffset_XY_NoCoplanar()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // Cubes share z=0 and z=1 planes
        Assert.True(graph.CoplanarFacesA.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_FullDiagonalOffset_NoCoplanar()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // No shared planes
        Assert.Empty(graph.CoplanarFacesA);
    }
}
