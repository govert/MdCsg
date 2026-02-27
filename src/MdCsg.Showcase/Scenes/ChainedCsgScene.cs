using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class ChainedCsgScene : IScene
{
    private (MeshBuffer Buf, Vector3 Offset, Vector4 Color)[] _items = [];

    public string Name => "Chained CSG";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var y = new Vec3(0, 1, 0);

        // Build complex object: (Sphere ∪ Box) - Cylinder_X - Cylinder_Y - Cylinder_Z
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);
        var cylZ = Primitives.Cylinder(new Vec3(0, 0, -1.5), new Vec3(0, 0, 1), 0.5, 3.0);

        var step1 = new Solid(Csg.Intersect(sphere, box).Mesh);
        var step2 = new Solid(Csg.Difference(step1, cylX).Mesh);
        var step3 = new Solid(Csg.Difference(step2, cylY).Mesh);
        var final = new Solid(Csg.Difference(step3, cylZ).Mesh);

        var entries = new (Solid s, Vector3 off, Vector4 col)[]
        {
            (step1, new(-5, 0, 0), new(0.5f, 0.5f, 0.5f, 1)),
            (step2, new(-1.5f, 0, 0), new(0.5f, 0.7f, 0.9f, 1)),
            (step3, new(1.5f, 0, 0), new(0.7f, 0.8f, 0.4f, 1)),
            (final, new(5, 0, 0), new(0.9f, 0.3f, 0.3f, 1)),
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
