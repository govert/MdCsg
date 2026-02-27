using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Showcase.Scenes;

internal class SimplificationScene : IScene
{
    private MeshBuffer? _original;
    private MeshBuffer? _simplified;
    private int _origFaces, _simpFaces;

    public string Name => "Simplification";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.5, 4);
        var mesh = sphere.Mesh;
        _origFaces = mesh.Faces.Count;

        var simplified = MeshSimplifier.Simplify(mesh, mesh.Faces.Count / 4);
        _simpFaces = simplified.Faces.Count;

        _original = renderer.CreateFlatBuffer(mesh);
        _simplified = renderer.CreateFlatBuffer(simplified);

        VertexCount = (int)(_original.VertexCount + _simplified.VertexCount);
        FaceCount = _original.FaceCount + _simplified.FaceCount;
    }

    public void Update(float dt) { }

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new(0.3f, -1f, 0.2f));

        var wL = Matrix4x4.CreateTranslation(-2.5f, 0, 0);
        renderer.UpdateConstants(wL * view * proj, wL, camera.Eye, light, new(0.4f, 0.6f, 0.85f, 1));
        renderer.DrawMesh(_original!);

        var wR = Matrix4x4.CreateTranslation(2.5f, 0, 0);
        renderer.UpdateConstants(wR * view * proj, wR, camera.Eye, light, new(0.85f, 0.5f, 0.3f, 1));
        renderer.DrawMesh(_simplified!);
    }

    public void Dispose()
    {
        _original?.Dispose();
        _simplified?.Dispose();
    }
}
