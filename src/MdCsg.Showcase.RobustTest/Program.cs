using System;
using MdCsg.Showcase;

namespace MdCsg.Showcase.RobustTest;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        AppWindow.Create(1280, 720, "MdCsg Robust Intersection Test");

        using var renderer = new Renderer();
        renderer.Initialize(AppWindow.Hwnd, 1280, 720);

        var camera = new Camera
        {
            Distance = 8.0f,
            Azimuth = 0.55f,
            Elevation = 0.25f
        };

        using var scene = new IntersectionDebugScene(renderer);

        while (AppWindow.ProcessMessages())
        {
            ProcessInput(camera, renderer, scene);

            if (AppWindow.WasResized && AppWindow.Width > 0 && AppWindow.Height > 0)
                renderer.Resize(AppWindow.Width, AppWindow.Height);

            renderer.BeginFrame();
            scene.Render(renderer, camera);
            renderer.EndFrame();

            AppWindow.SetTitle(
                $"MdCsg Robust Intersection Test | {scene.ModeLabel} | {(renderer.WireframeMode ? "Wireframe" : "Shaded")} | {(renderer.FlatShading ? "Flat" : "Smooth")} | Keys: 1-5 views, W wire, N shading, Esc quit");
        }
    }

    private static void ProcessInput(Camera camera, Renderer renderer, IntersectionDebugScene scene)
    {
        if (AppWindow.LeftButtonDown)
        {
            int dx = AppWindow.MouseX - AppWindow.PrevMouseX;
            int dy = AppWindow.MouseY - AppWindow.PrevMouseY;
            camera.Rotate(dx * 0.005f, -dy * 0.005f);
        }

        if (AppWindow.RightButtonDown)
        {
            int dx = AppWindow.MouseX - AppWindow.PrevMouseX;
            int dy = AppWindow.MouseY - AppWindow.PrevMouseY;
            camera.Pan(-dx, dy);
        }

        if (AppWindow.ScrollDelta != 0)
            camera.Zoom(AppWindow.ScrollDelta / 120f);

        if (AppWindow.KeyPressed['1']) scene.SetMode(IntersectionViewMode.Inputs);
        if (AppWindow.KeyPressed['2']) scene.SetMode(IntersectionViewMode.IntersectionOnly);
        if (AppWindow.KeyPressed['3']) scene.SetMode(IntersectionViewMode.CubeMinusSphere);
        if (AppWindow.KeyPressed['4']) scene.SetMode(IntersectionViewMode.SphereMinusCube);
        if (AppWindow.KeyPressed['5']) scene.SetMode(IntersectionViewMode.ExplodedParts);

        if (AppWindow.KeyPressed['W']) renderer.WireframeMode = !renderer.WireframeMode;
        if (AppWindow.KeyPressed['N']) renderer.FlatShading = !renderer.FlatShading;
    }
}
