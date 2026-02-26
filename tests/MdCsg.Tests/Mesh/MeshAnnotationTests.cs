using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshAnnotationTests
{
    [Fact]
    public void GetSet_StoresValues()
    {
        var ann = new MeshAnnotation<double>();
        ann[0] = 3.14;
        ann[5] = 2.72;
        Assert.Equal(3.14, ann[0]);
        Assert.Equal(2.72, ann[5]);
    }

    [Fact]
    public void Get_UnsetIndex_ReturnsDefault()
    {
        var ann = new MeshAnnotation<int>();
        Assert.Equal(0, ann[0]);
        Assert.Equal(0, ann[100]);
    }

    [Fact]
    public void AutoGrow_LargeIndex()
    {
        var ann = new MeshAnnotation<string>(4);
        ann[1000] = "hello";
        Assert.Equal("hello", ann[1000]);
    }

    [Fact]
    public void ForVertices_CorrectCapacity()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var ann = MeshAnnotation<double>.ForVertices(cube.Mesh);
        // Should be able to index all vertices without growing
        for (int i = 0; i < cube.Mesh.Vertices.Count; i++)
            ann[i] = i * 1.0;

        for (int i = 0; i < cube.Mesh.Vertices.Count; i++)
            Assert.Equal(i * 1.0, ann[i]);
    }

    [Fact]
    public void ForFaces_CorrectCapacity()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var ann = MeshAnnotation<int>.ForFaces(cube.Mesh);
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
            ann[i] = i * 10;

        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
            Assert.Equal(i * 10, ann[i]);
    }

    [Fact]
    public void ForHalfEdges_CorrectCapacity()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var ann = MeshAnnotation<bool>.ForHalfEdges(cube.Mesh);
        ann[0] = true;
        Assert.True(ann[0]);
        Assert.False(ann[1]);
    }

    [Fact]
    public void Overwrite_ReplacesValue()
    {
        var ann = new MeshAnnotation<double>();
        ann[3] = 1.0;
        ann[3] = 2.0;
        Assert.Equal(2.0, ann[3]);
    }

    [Fact]
    public void StringAnnotation_NullDefault()
    {
        var ann = new MeshAnnotation<string>();
        Assert.Null(ann[0]);
    }
}
