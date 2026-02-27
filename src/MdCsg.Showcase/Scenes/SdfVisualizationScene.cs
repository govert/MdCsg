using System;
using System.Collections.Generic;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class SdfVisualizationScene : IScene
{
    private MeshBuffer? _mesh;
    private MeshBuffer? _insidePoints;
    private MeshBuffer? _outsidePoints;
    private MeshBuffer? _surfacePoints;

    public string Name => "SDF Visualization";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        // CSG object
        var sphere = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var cyl = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var solid = new Solid(Csg.Difference(sphere, cyl).Mesh);

        _mesh = renderer.CreateFlatBuffer(solid.Mesh);

        // Sample SDF on a grid in the XY plane (z=0)
        var sdf = new SignedDistanceField(solid);
        var inside = new List<Vec3>();
        var outside = new List<Vec3>();
        var surface = new List<Vec3>();

        int res = 50;
        double range = 2.0;
        for (int ix = 0; ix < res; ix++)
        for (int iy = 0; iy < res; iy++)
        {
            double x = -range + 2 * range * ix / (res - 1);
            double y = -range + 2 * range * iy / (res - 1);
            var p = new Vec3(x, y, 0);
            double d = sdf.SignedDistance(p);

            if (System.Math.Abs(d) < 0.05)
                surface.Add(p);
            else if (d < 0)
                inside.Add(p);
            else
                outside.Add(p);
        }

        if (inside.Count > 0) _insidePoints = renderer.CreatePointBuffer(inside.ToArray());
        if (outside.Count > 0) _outsidePoints = renderer.CreatePointBuffer(outside.ToArray());
        if (surface.Count > 0) _surfacePoints = renderer.CreatePointBuffer(surface.ToArray());

        VertexCount = (int)_mesh.VertexCount + inside.Count + outside.Count + surface.Count;
        FaceCount = _mesh.FaceCount;
    }

    public void Update(float dt) { }

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new(0.3f, -1f, 0.2f));

        // Draw mesh offset to side
        var wM = Matrix4x4.CreateTranslation(-3f, 0, 0);
        renderer.UpdateConstants(wM * view * proj, wM, camera.Eye, light, new(0.5f, 0.5f, 0.5f, 0.6f));
        renderer.DrawMesh(_mesh!);

        // Draw SDF points
        var wP = Matrix4x4.CreateTranslation(3f, 0, 0);
        if (_insidePoints != null)
        {
            renderer.UpdateConstants(wP * view * proj, wP, camera.Eye, light, new(0.9f, 0.2f, 0.2f, 1));
            renderer.DrawPoints(_insidePoints);
        }
        if (_outsidePoints != null)
        {
            renderer.UpdateConstants(wP * view * proj, wP, camera.Eye, light, new(0.2f, 0.4f, 0.9f, 1));
            renderer.DrawPoints(_outsidePoints);
        }
        if (_surfacePoints != null)
        {
            renderer.UpdateConstants(wP * view * proj, wP, camera.Eye, light, new(0.2f, 0.9f, 0.2f, 1));
            renderer.DrawPoints(_surfacePoints);
        }
    }

    public void Dispose()
    {
        _mesh?.Dispose();
        _insidePoints?.Dispose();
        _outsidePoints?.Dispose();
        _surfacePoints?.Dispose();
    }
}
