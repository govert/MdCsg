using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class ImplicitScene : IScene
{
    private (MeshBuffer Buf, Vector3 Offset, Vector4 Color)[] _items = [];

    public string Name => "Implicit Cuts";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);

        var sphere = new ImplicitSphere(Vec3.Zero, 1.3);
        var cylinder = new ImplicitCylinder(new Vec3(0, -1.5, 0), new Vec3(0, 1, 0), 0.6, 3.0);

        var sphereCut = new Solid(Csg.Intersect(cube, sphere).Mesh);
        var cylDiff = new Solid(Csg.Difference(cube, cylinder).Mesh);

        var entries = new (Solid s, Vector3 off, Vector4 col)[]
        {
            (cube, new(-4, 0, 0), new(0.5f, 0.5f, 0.5f, 1)),
            (sphereCut, new(0, 0, 0), new(0.3f, 0.6f, 0.9f, 1)),
            (cylDiff, new(4, 0, 0), new(0.9f, 0.4f, 0.3f, 1)),
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
