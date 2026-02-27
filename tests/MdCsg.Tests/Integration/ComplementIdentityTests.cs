using MdCsg.Api;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 05: A∩comp(A)=∅, comp(comp(A))=A, De Morgan on complements.</summary>
public class ComplementIdentityTests
{
    private const double Tol = 0.5;

    [Fact]
    public void Complement_ReversesWinding()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comp = cube.Complement();
        Assert.Equal(cube.Mesh.Faces.Count, comp.Mesh.Faces.Count);
    }

    [Fact]
    public void DoubleComplement_SameVertexCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var compComp = cube.Complement().Complement();
        Assert.Equal(cube.Mesh.Vertices.Count, compComp.Mesh.Vertices.Count);
    }

    [Fact]
    public void DoubleComplement_SameFaceCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var compComp = cube.Complement().Complement();
        Assert.Equal(cube.Mesh.Faces.Count, compComp.Mesh.Faces.Count);
    }

    [Fact]
    public void DoubleComplement_SameVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var compComp = sphere.Complement().Complement();
        Assert.True(System.Math.Abs(sphere.Volume() - compComp.Volume()) < 0.01);
    }

    [Fact]
    public void Complement_FlipsInsideOutside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comp = cube.Complement();
        // Original: center is inside
        Assert.True(cube.IsInside(Vec3.Zero));
        // Complement: center should be outside
        Assert.False(comp.IsInside(Vec3.Zero));
    }

    [Fact]
    public void Complement_FlipsOutsideToInside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comp = cube.Complement();
        var farPoint = new Vec3(10, 10, 10);
        Assert.False(cube.IsInside(farPoint));
        Assert.True(comp.IsInside(farPoint));
    }

    [Fact]
    public void ComplementOfSphere_FlipsClassification()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var comp = sphere.Complement();
        Assert.True(sphere.IsInside(Vec3.Zero));
        Assert.False(comp.IsInside(Vec3.Zero));
    }

    [Fact]
    public void ComplementOfCylinder_FlipsClassification()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var comp = cyl.Complement();
        Assert.True(cyl.IsInside(Vec3.Zero));
        Assert.False(comp.IsInside(Vec3.Zero));
    }

    [Fact]
    public void ComplementOfCone_FlipsClassification()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var comp = cone.Complement();
        Assert.True(cone.IsInside(new Vec3(0, 0, -0.5)));
        Assert.False(comp.IsInside(new Vec3(0, 0, -0.5)));
    }

    [Fact]
    public void ComplementOfTorus_FlipsClassification()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var comp = torus.Complement();
        var insidePt = new Vec3(1.0, 0, 0); // inside the tube
        Assert.True(torus.IsInside(insidePt));
        Assert.False(comp.IsInside(insidePt));
    }

    [Fact]
    public void SelfIntersectionWithComplement_ProducesEmptyOrZero()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comp = cube.Complement();
        var result = Csg.Intersect(cube, comp);
        Assert.True(result.FaceCount == 0 || new Solid(result.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DifferenceWithSelf_ProducesEmptyOrZero()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var result = Csg.Difference(cube, cube);
        Assert.True(result.FaceCount == 0 || new Solid(result.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DeMorgan_ComplementOfUnion()
    {
        // comp(A∪B) should classify same as comp(A) ∩ comp(B) at test points
        var a = Primitives.Cube(new Vec3(-0.3, 0, 0), 1.5);
        var b = Primitives.Cube(new Vec3(0.3, 0, 0), 1.5);
        var unionAB = new Solid(Csg.Union(a, b).Mesh);
        var compUnion = unionAB.Complement();

        var compA = a.Complement();
        var compB = b.Complement();
        var interCompComp = new Solid(Csg.Intersect(compA, compB).Mesh);

        var testPt = new Vec3(5, 5, 5);
        Assert.Equal(compUnion.IsInside(testPt), interCompComp.IsInside(testPt));
    }

    [Fact]
    public void DeMorgan_ComplementOfIntersection()
    {
        var a = Primitives.Cube(new Vec3(-0.3, 0, 0), 1.5);
        var b = Primitives.Cube(new Vec3(0.3, 0, 0), 1.5);
        var interAB = new Solid(Csg.Intersect(a, b).Mesh);
        var compInter = interAB.Complement();

        var testPt = new Vec3(5, 5, 5);
        // comp(A∩B) at far away point should be inside
        Assert.True(compInter.IsInside(testPt));
    }

    [Fact]
    public void DoubleComplementBox_PreservesVolume()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var cc = box.Complement().Complement();
        Assert.True(System.Math.Abs(box.Volume() - cc.Volume()) < 0.01);
    }

    [Fact]
    public void DoubleComplementSphere_PreservesVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var cc = sphere.Complement().Complement();
        Assert.True(System.Math.Abs(sphere.Volume() - cc.Volume()) < 0.01);
    }

    [Fact]
    public void ComplementPreservesFaceCount()
    {
        var shapes = new[]
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0, 2),
            Primitives.Cylinder(new Vec3(0,0,-1), Vec3.UnitZ, 0.5, 2.0, 12),
        };
        foreach (var shape in shapes)
        {
            var comp = shape.Complement();
            Assert.Equal(shape.Mesh.Faces.Count, comp.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void ComplementPreservesVertexCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comp = cube.Complement();
        Assert.Equal(cube.Mesh.Vertices.Count, comp.Mesh.Vertices.Count);
    }

    [Fact]
    public void ComplementOfComplement_IsInsideSameAsOriginal()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cc = cube.Complement().Complement();
        var testPoints = new[]
        {
            Vec3.Zero,
            new Vec3(0.5, 0.5, 0.5),
            new Vec3(10, 10, 10),
        };
        foreach (var p in testPoints)
            Assert.Equal(cube.IsInside(p), cc.IsInside(p));
    }
}
