using System;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Showcase.Scenes;

internal class FittingScene : IScene
{
    private MeshBuffer? _originalMesh;
    private MeshBuffer? _fittedMesh;
    private MeshBuffer? _vertexPoints;

    public string Name => "Primitive Fitting";
    public int VertexCount { get; private set; }
    public int FaceCount { get; private set; }

    public void Initialize(Renderer renderer)
    {
        // Create a sphere and extract its vertices
        var sphere = Primitives.Sphere(new Vec3(0.1, -0.05, 0.15), 1.3, 3);
        var mesh = sphere.Mesh;

        var points = new Vec3[mesh.Vertices.Count];
        for (int i = 0; i < mesh.Vertices.Count; i++)
            points[i] = mesh.Vertices[i].Position;

        // Fit a sphere to the extracted vertices
        var fitted = SphereFitter.Fit(points);
        var fittedSolid = MeshFitting.ToSolid(fitted);

        _originalMesh = renderer.CreateFlatBuffer(mesh);
        _fittedMesh = renderer.CreateFlatBuffer(fittedSolid.Mesh);
        _vertexPoints = renderer.CreatePointBuffer(points);

        VertexCount = (int)(_originalMesh.VertexCount + _fittedMesh.VertexCount) + points.Length;
        FaceCount = _originalMesh.FaceCount + _fittedMesh.FaceCount;
    }

    public void Update(float dt) { }

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new(0.3f, -1f, 0.2f));

        // Original mesh (left)
        var wL = Matrix4x4.CreateTranslation(-3f, 0, 0);
        renderer.UpdateConstants(wL * view * proj, wL, camera.Eye, light, new(0.5f, 0.5f, 0.5f, 1));
        renderer.DrawMesh(_originalMesh!);

        // Vertex points (center)
        var wC = Matrix4x4.Identity;
        renderer.UpdateConstants(wC * view * proj, wC, camera.Eye, light, new(1f, 0.8f, 0.1f, 1));
        renderer.DrawPoints(_vertexPoints!);

        // Fitted sphere (right)
        var wR = Matrix4x4.CreateTranslation(3f, 0, 0);
        renderer.UpdateConstants(wR * view * proj, wR, camera.Eye, light, new(0.3f, 0.8f, 0.4f, 1));
        renderer.DrawMesh(_fittedMesh!);
    }

    public void Dispose()
    {
        _originalMesh?.Dispose();
        _fittedMesh?.Dispose();
        _vertexPoints?.Dispose();
    }
}
