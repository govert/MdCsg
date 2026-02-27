using System;
using System.Collections.Generic;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class PointCloudScene : IScene
{
    private MeshBuffer? _mesh;
    private MeshBuffer? _insidePoints;
    private MeshBuffer? _outsidePoints;

    public string Name => "Point Cloud Classification";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var solid = Primitives.Torus(Vec3.Zero, new Vec3(0, 1, 0), 1.2, 0.4, 20, 10);
        _mesh = renderer.CreateFlatBuffer(solid.Mesh);

        // Generate random points and classify
        var rng = new Random(42);
        var inside = new List<Vec3>();
        var outside = new List<Vec3>();
        double r = 2.0;

        for (int i = 0; i < 5000; i++)
        {
            var p = new Vec3(
                rng.NextDouble() * 2 * r - r,
                rng.NextDouble() * 2 * r - r,
                rng.NextDouble() * 2 * r - r);

            if (solid.IsInside(p))
                inside.Add(p);
            else
                outside.Add(p);
        }

        if (inside.Count > 0) _insidePoints = renderer.CreatePointBuffer(inside.ToArray());
        if (outside.Count > 0) _outsidePoints = renderer.CreatePointBuffer(outside.ToArray());

        VertexCount = (int)_mesh.VertexCount + inside.Count + outside.Count;
        FaceCount = _mesh.FaceCount;
    }

    public void Update(float dt) { }

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new(0.3f, -1f, 0.2f));
        var w = Matrix4x4.Identity;

        // Draw translucent mesh
        renderer.UpdateConstants(w * view * proj, w, camera.Eye, light, new(0.5f, 0.5f, 0.5f, 0.4f));
        renderer.DrawMesh(_mesh!);

        // Inside points (red)
        if (_insidePoints != null)
        {
            renderer.UpdateConstants(w * view * proj, w, camera.Eye, light, new(0.9f, 0.15f, 0.15f, 1));
            renderer.DrawPoints(_insidePoints);
        }

        // Outside points (blue)
        if (_outsidePoints != null)
        {
            renderer.UpdateConstants(w * view * proj, w, camera.Eye, light, new(0.15f, 0.3f, 0.9f, 1));
            renderer.DrawPoints(_outsidePoints);
        }
    }

    public void Dispose()
    {
        _mesh?.Dispose();
        _insidePoints?.Dispose();
        _outsidePoints?.Dispose();
    }
}
