using System;

namespace MdCsg.Showcase;

/// <summary>A renderable demo scene.</summary>
internal interface IScene : IDisposable
{
    string Name { get; }
    int VertexCount { get; }
    int FaceCount { get; }
    void Initialize(Renderer renderer);
    void Update(float deltaTime);
    void Render(Renderer renderer, Camera camera);
}
