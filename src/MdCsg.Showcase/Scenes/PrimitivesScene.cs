using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class PrimitivesScene : IScene
{
    private (MeshBuffer Buf, Vector3 Offset, Vector4 Color)[] _items = [];

    public string Name => "Primitives";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var y = new Vec3(0, 1, 0);

        var profile = new Vec2[] { new(0, 0), new(1, 0), new(1, 0.4), new(0.4, 0.4), new(0.4, 1), new(0, 1) };
        var revProfile = new Vec2[] { new(0.5, 0), new(1.0, 0), new(0.8, 0.5), new(1.0, 1.0), new(0.5, 1.0) };

        var solids = new (Solid s, Vector3 off, Vector4 col)[]
        {
            (Primitives.Cube(Vec3.Zero, 1.5), new(-9, 0, 0), new(0.85f, 0.3f, 0.3f, 1)),
            (Primitives.Sphere(Vec3.Zero, 1.0, 3), new(-6, 0, 0), new(0.3f, 0.55f, 0.85f, 1)),
            (Primitives.Cylinder(new Vec3(0, -1, 0), y, 0.7, 2.0), new(-3, 0, 0), new(0.3f, 0.8f, 0.4f, 1)),
            (Primitives.Cone(new Vec3(0, -1, 0), y, 0.8, 2.0), new(0, 0, 0), new(0.85f, 0.7f, 0.2f, 1)),
            (Primitives.Torus(Vec3.Zero, y, 1.0, 0.35, 24, 12), new(3, 0, 0), new(0.7f, 0.3f, 0.75f, 1)),
            (Primitives.Extrude(profile, new Vec3(0, 0, 1), 1.0), new(6, 0, 0), new(0.2f, 0.7f, 0.75f, 1)),
            (Primitives.Revolve(revProfile, y, 24), new(9, 0, 0), new(0.9f, 0.5f, 0.2f, 1)),
        };

        _items = new (MeshBuffer, Vector3, Vector4)[solids.Length];
        int tv = 0, tf = 0;
        for (int i = 0; i < solids.Length; i++)
        {
            var buf = renderer.CreateFlatBuffer(solids[i].s.Mesh);
            _items[i] = (buf, solids[i].off, solids[i].col);
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
