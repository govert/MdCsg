using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Showcase.Scenes;

internal class SmoothingScene : IScene
{
    private MeshBuffer? _original;
    private MeshBuffer? _smoothed;

    public string Name => "Laplacian Smoothing";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        // CSG result has faceted edges - good candidate for smoothing
        var box = Primitives.Cube(Vec3.Zero, 2.0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.4, 3);
        var csg = new Solid(Csg.Intersect(box, sphere).Mesh);
        var mesh = csg.Mesh;

        var smoothed = MeshSmoothing.LaplacianSmooth(mesh, iterations: 5, lambda: 0.5);

        _original = renderer.CreateFlatBuffer(mesh);
        _smoothed = renderer.CreateFlatBuffer(smoothed);

        VertexCount = (int)(_original.VertexCount + _smoothed.VertexCount);
        FaceCount = _original.FaceCount + _smoothed.FaceCount;
    }

    public void Update(float dt) { }

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new(0.3f, -1f, 0.2f));

        var wL = Matrix4x4.CreateTranslation(-2.5f, 0, 0);
        renderer.UpdateConstants(wL * view * proj, wL, camera.Eye, light, new(0.6f, 0.6f, 0.6f, 1));
        renderer.DrawMesh(_original!);

        var wR = Matrix4x4.CreateTranslation(2.5f, 0, 0);
        renderer.UpdateConstants(wR * view * proj, wR, camera.Eye, light, new(0.3f, 0.75f, 0.9f, 1));
        renderer.DrawMesh(_smoothed!);
    }

    public void Dispose()
    {
        _original?.Dispose();
        _smoothed?.Dispose();
    }
}
