using System;
using System.Collections.Generic;

namespace MdCsg.Showcase;

/// <summary>Manages demo scenes and keyboard switching.</summary>
internal class SceneManager : IDisposable
{
    private readonly List<IScene> _scenes = new();
    private int _activeIndex;

    public IScene? ActiveScene => _activeIndex < _scenes.Count ? _scenes[_activeIndex] : null;

    public void AddScene(IScene scene, Renderer renderer)
    {
        scene.Initialize(renderer);
        _scenes.Add(scene);
    }

    public void SwitchScene(int index)
    {
        if (index >= 0 && index < _scenes.Count)
            _activeIndex = index;
    }

    public void Update(float dt) => ActiveScene?.Update(dt);

    public void Render(Renderer renderer, Camera camera) => ActiveScene?.Render(renderer, camera);

    public void Dispose()
    {
        foreach (var s in _scenes) s.Dispose();
        _scenes.Clear();
    }
}
