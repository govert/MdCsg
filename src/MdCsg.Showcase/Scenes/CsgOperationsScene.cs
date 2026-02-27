using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class CsgOperationsScene : IScene
{
    private (MeshBuffer Buf, Vector3 Offset, Vector4 Color, string Label)[] _items = [];

    public string Name => "CSG Operations";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        var union = new Solid(Csg.Union(a, b).Mesh);
        var inter = new Solid(Csg.Intersect(a, b).Mesh);
        var diff = new Solid(Csg.Difference(a, b).Mesh);

        var entries = new (Solid s, Vector3 off, Vector4 col, string lbl)[]
        {
            (a, new(-6, 0, 0), new(0.3f, 0.55f, 0.85f, 1), "A (Sphere)"),
            (b, new(-3, 0, 0), new(0.85f, 0.3f, 0.3f, 1), "B (Box)"),
            (union, new(0, 0, 0), new(0.6f, 0.2f, 0.7f, 1), "A ∪ B"),
            (inter, new(3, 0, 0), new(0.2f, 0.7f, 0.3f, 1), "A ∩ B"),
            (diff, new(6, 0, 0), new(0.85f, 0.6f, 0.2f, 1), "A - B"),
        };

        _items = new (MeshBuffer, Vector3, Vector4, string)[entries.Length];
        int tv = 0, tf = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            var buf = renderer.CreateFlatBuffer(entries[i].s.Mesh);
            _items[i] = (buf, entries[i].off, entries[i].col, entries[i].lbl);
            tv += (int)buf.VertexCount;
            tf += buf.FaceCount;
        }
        VertexCount = tv;
        FaceCount = tf;
    }

    public void Update(float dt) { }

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new(0.3f, -1f, 0.2f));

        foreach (var (buf, off, col, _) in _items)
        {
            var w = Matrix4x4.CreateTranslation(off);
            renderer.UpdateConstants(w * view * proj, w, camera.Eye, light, col);
            renderer.DrawMesh(buf);
        }
    }

    public void Dispose()
    {
        foreach (var (buf, _, _, _) in _items) buf.Dispose();
        _items = [];
    }
}
