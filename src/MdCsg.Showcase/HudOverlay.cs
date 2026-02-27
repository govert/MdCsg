using System.Diagnostics;

namespace MdCsg.Showcase;

/// <summary>Tracks FPS and generates window title string.</summary>
internal class HudOverlay
{
    private int _frameCount;
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private double _fps;
    private double _lastUpdate;

    public double Fps => _fps;

    public void Update()
    {
        _frameCount++;
        var t = _sw.Elapsed.TotalSeconds;
        if (t - _lastUpdate >= 1.0)
        {
            _fps = _frameCount / (t - _lastUpdate);
            _frameCount = 0;
            _lastUpdate = t;
        }
    }

    public string GetTitle(string scene, int verts, int faces, bool wire, bool flat)
        => $"MdCsg Showcase - {scene} | {_fps:F0} FPS | {verts:N0} verts, {faces:N0} faces | {(wire ? "Wire" : "Solid")} | {(flat ? "Flat" : "Smooth")}";
}
