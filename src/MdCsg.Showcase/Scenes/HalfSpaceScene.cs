using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;
namespace MdCsg.Showcase.Scenes;

internal class HalfSpaceScene : IScene
{
    private (MeshBuffer Buf, Vector3 Offset, Vector4 Color)[] _items = [];

    public string Name => "HalfSpace Slicing";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.5, 3);

        var hs1 = new HalfSpace(new Vec3(0, 1, 0), 0.0);
        var hs2 = new HalfSpace(new Vec3(1, 0, 0), 0.3);
        var hs3 = new HalfSpace(new Vec3(0, 0, 1), -0.2);

        var cut1 = new Solid(Csg.Intersect(sphere, hs1).Mesh);
        var cut2 = new Solid(Csg.Intersect(cut1, hs2).Mesh);
        var cut3 = new Solid(Csg.Intersect(cut2, hs3).Mesh);

        var entries = new (Solid s, Vector3 off, Vector4 col)[]
        {
            (sphere, new(-6, 0, 0), new(0.5f, 0.5f, 0.5f, 1)),
            (cut1, new(-2, 0, 0), new(0.4f, 0.7f, 0.9f, 1)),
            (cut2, new(2, 0, 0), new(0.3f, 0.8f, 0.4f, 1)),
            (cut3, new(6, 0, 0), new(0.9f, 0.5f, 0.2f, 1)),
        };

        _items = new (MeshBuffer, Vector3, Vector4)[entries.Length];
        int tv = 0, tf = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            var buf = renderer.CreateFlatBuffer(entries[i].s.Mesh);
            _items[i] = (buf, entries[i].off, entries[i].col);
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

        foreach (var (buf, off, col) in _items)
        {
            var w = Matrix4x4.CreateTranslation(off);
            renderer.UpdateConstants(w * view * proj, w, camera.Eye, light, col);
            renderer.DrawMesh(buf);
        }
    }

    public void Dispose()
    {
        foreach (var (buf, _, _) in _items) buf.Dispose();
        _items = [];
    }
}
