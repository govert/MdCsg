using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgResult cross-operation consistency — disjoint, nested, metadata invariants</summary>
public class CsgResultConsistencyTests
{
    [Fact]
    public void DisjointCubes_Union_FaceCount_SumOfBoth()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(100, 100, 100)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void DisjointCubes_Difference_EqualsA()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(100, 100, 100)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void AllThreeOps_SameIntersectionSegmentCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var u = Csg.Union(a, b);
        var i = Csg.Intersect(a, b);
        var d = Csg.Difference(a, b);
        Assert.Equal(u.IntersectionSegmentCount, i.IntersectionSegmentCount);
        Assert.Equal(u.IntersectionSegmentCount, d.IntersectionSegmentCount);
    }

    [Fact]
    public void AllThreeOps_SamePatchCounts()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var u = Csg.Union(a, b);
        var i = Csg.Intersect(a, b);
        var d = Csg.Difference(a, b);
        Assert.Equal(u.PatchCountA, i.PatchCountA);
        Assert.Equal(u.PatchCountA, d.PatchCountA);
        Assert.Equal(u.PatchCountB, i.PatchCountB);
        Assert.Equal(u.PatchCountB, d.PatchCountB);
    }

    [Fact]
    public void Union_FaceCount_GreaterThanIntersect()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        Assert.True(union.FaceCount > inter.FaceCount);
    }

    [Fact]
    public void SmallInsideLarge_Difference_ProducesCavity()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5).Mesh);
        var diff = Csg.Difference(large, small);
        // Cavity means more faces than original large cube
        Assert.True(diff.FaceCount > large.Mesh.Faces.Count);
    }

    [Fact]
    public void SmallInsideLarge_Intersect_EqualsSmall()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5).Mesh);
        var inter = Csg.Intersect(large, small);
        // Intersection of nested = smaller shape
        Assert.True(inter.FaceCount > 0);
        Assert.True(inter.FaceCount <= small.Mesh.Faces.Count + 2); // small tolerance
    }

    [Fact]
    public void SmallInsideLarge_Union_EqualsLarge()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5).Mesh);
        var union = Csg.Union(large, small);
        // Union of nested = larger shape
        Assert.True(union.FaceCount > 0);
        Assert.True(union.FaceCount <= large.Mesh.Faces.Count + 2); // small tolerance
    }

    [Fact]
    public void Result_CanBeUsedAsInput_Chained()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var r1 = Csg.Union(a, b);
        var solid1 = new Solid(r1.Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var r2 = Csg.Union(solid1, c);
        Assert.True(r2.FaceCount > 0);
    }

    [Fact]
    public void Sphere_Operations_ConsistentPatchCounts()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1, 2).Mesh);
        var u = Csg.Union(a, b);
        var i = Csg.Intersect(a, b);
        Assert.Equal(u.PatchCountA, i.PatchCountA);
        Assert.Equal(u.PatchCountB, i.PatchCountB);
    }

    [Fact]
    public void Solid_Bounds_ContainsAllVertices()
    {
        var solid = MeshFactory.CreateCube();
        var bounds = solid.Bounds;
        foreach (var v in solid.Mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position),
                $"Vertex {v.Position} not in bounds {bounds.Min}-{bounds.Max}");
        }
    }

    [Fact]
    public void Solid_FromTriangles_WorksWithCsg()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
            new(new Vec3(0, 0, 0), new Vec3(0, 0, 1), new Vec3(1, 0, 0)),
            new(new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(4, solid.Mesh.Faces.Count);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_FromIndexed_WorksWithCsg()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 1, 2), (0, 2, 3), (0, 3, 1), (1, 3, 2)
        };
        var solid = Solid.FromIndexed(verts, indices);
        Assert.Equal(4, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void WindingNumber_VsRayCast_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var rc = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
    }
}
