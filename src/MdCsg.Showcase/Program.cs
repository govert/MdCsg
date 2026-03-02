using System;
using System.Diagnostics;
using System.IO;
using MdCsg.Showcase;
using MdCsg.Showcase.Scenes;
using static TerraFX.Interop.Windows.Windows;

static unsafe class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        bool screenshotMode = args.Length > 0 && args[0] == "--screenshot";
        string? screenshotDir = screenshotMode && args.Length > 1 ? args[1] : null;
        bool legacyCsgMode = Array.Exists(
            args,
            static a => string.Equals(a, "--legacy-csg", StringComparison.OrdinalIgnoreCase));
        bool legacyFailoverMode = Array.Exists(
            args,
            static a => string.Equals(a, "--allow-legacy-failover", StringComparison.OrdinalIgnoreCase));
        bool strictRobustOnlyMode = Array.Exists(
            args,
            static a => string.Equals(a, "--strict-robust-only", StringComparison.OrdinalIgnoreCase));
        ShowcaseRuntimeOptions.UseLegacyCsg = legacyCsgMode;
        ShowcaseRuntimeOptions.AllowLegacyFailover = legacyFailoverMode;
        ShowcaseRuntimeOptions.UseClosureAttemptRobust = !strictRobustOnlyMode;
        if (legacyCsgMode)
        {
            Console.WriteLine("[showcase] CSG mode: legacy (explicit opt-out)");
        }
        else if (legacyFailoverMode)
        {
            Console.WriteLine(
                $"[showcase] CSG mode: robust-strict (closureAttempt={(ShowcaseRuntimeOptions.UseClosureAttemptRobust ? 1 : 0)}) with explicit legacy failover");
        }
        else
        {
            Console.WriteLine(
                $"[showcase] CSG mode: robust-strict (closureAttempt={(ShowcaseRuntimeOptions.UseClosureAttemptRobust ? 1 : 0)}, no automatic fallback)");
        }

        AppWindow.Create(1280, 720, "MdCsg Showcase - Loading...");

        using var renderer = new Renderer();
        renderer.Initialize(AppWindow.Hwnd, 1280, 720);

        var camera = new Camera { Distance = 12f };
        var scenes = new SceneManager();

        scenes.AddScene(new PrimitivesScene(), renderer);
        scenes.AddScene(new CsgOperationsScene(), renderer);
        scenes.AddScene(new ImplicitScene(), renderer);
        scenes.AddScene(new HalfSpaceScene(), renderer);
        scenes.AddScene(new ChainedCsgScene(), renderer);
        scenes.AddScene(new SimplificationScene(), renderer);
        scenes.AddScene(new SmoothingScene(), renderer);
        scenes.AddScene(new SdfVisualizationScene(), renderer);
        scenes.AddScene(new PointCloudScene(), renderer);
        scenes.AddScene(new FittingScene(), renderer);

        if (screenshotMode)
        {
            var outDir = screenshotDir ?? Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(outDir);

            for (int i = 0; i < 10; i++)
            {
                scenes.SwitchScene(i);
                renderer.BeginFrame();
                scenes.Render(renderer, camera);
                renderer.EndFrame();

                // Render a second frame to ensure Present completes
                renderer.BeginFrame();
                scenes.Render(renderer, camera);
                // Don't Present yet - save the back buffer before Present
                var sceneName = scenes.ActiveScene?.Name ?? $"scene_{i}";
                var safeName = string.Join("_", sceneName.Split(Path.GetInvalidFileNameChars()));
                var filePath = Path.Combine(outDir, $"{i + 1:D2}_{safeName}.bmp");
                renderer.SaveBackBuffer(filePath);
                renderer.EndFrame();

                Console.WriteLine($"Saved: {filePath}");
            }
            Console.WriteLine($"Screenshots saved to: {outDir}");
            scenes.Dispose();
            return;
        }

        var hud = new HudOverlay();
        var sw = Stopwatch.StartNew();
        var lastTime = 0.0;

        while (AppWindow.ProcessMessages())
        {
            var now = sw.Elapsed.TotalSeconds;
            var dt = (float)(now - lastTime);
            lastTime = now;

            ProcessInput(camera, scenes, renderer);
            scenes.Update(dt);
            hud.Update();

            if (AppWindow.WasResized && AppWindow.Width > 0 && AppWindow.Height > 0)
                renderer.Resize(AppWindow.Width, AppWindow.Height);

            renderer.BeginFrame();
            scenes.Render(renderer, camera);
            renderer.EndFrame();

            var s = scenes.ActiveScene;
            if (s != null)
                AppWindow.SetTitle(hud.GetTitle(s.Name, s.VertexCount, s.FaceCount,
                    renderer.WireframeMode, renderer.FlatShading));
        }

        scenes.Dispose();
    }

    static void ProcessInput(Camera camera, SceneManager scenes, Renderer renderer)
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

        for (int i = 0; i < 10; i++)
        {
            int key = i < 9 ? '1' + i : '0';
            if (AppWindow.KeyPressed[key])
            {
                scenes.SwitchScene(i);
                break;
            }
        }

        if (AppWindow.KeyPressed['W']) renderer.WireframeMode = !renderer.WireframeMode;
        if (AppWindow.KeyPressed['N']) renderer.FlatShading = !renderer.FlatShading;
    }
}
