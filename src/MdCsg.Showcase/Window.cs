using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.CS;
using static TerraFX.Interop.Windows.WS;
using static TerraFX.Interop.Windows.VK;
using static TerraFX.Interop.Windows.PM;
using static TerraFX.Interop.Windows.SW;
using static TerraFX.Interop.Windows.IDC;

namespace MdCsg.Showcase;

/// <summary>Win32 window with input tracking.</summary>
internal static unsafe class AppWindow
{
    private static HWND s_hwnd;
    private static int s_width, s_height;
    private static bool s_resized;

    public static int MouseX, MouseY;
    public static int PrevMouseX, PrevMouseY;
    public static bool LeftButtonDown, RightButtonDown;
    public static int ScrollDelta;
    public static readonly bool[] KeyPressed = new bool[256];

    public static HWND Hwnd => s_hwnd;
    public static int Width => s_width;
    public static int Height => s_height;

    public static bool WasResized
    {
        get { var r = s_resized; s_resized = false; return r; }
    }

    public static void Create(int width, int height, string title)
    {
        s_width = width;
        s_height = height;
        var hModule = GetModuleHandleW((char*)null);
        var hInstance = (HINSTANCE)(void*)hModule;

        fixed (char* className = "MdCsgShowcase")
        fixed (char* windowName = title)
        {
            var wc = new WNDCLASSEXW
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = &WndProc,
                hInstance = hInstance,
                hCursor = LoadCursorW(HINSTANCE.NULL, IDC_ARROW),
                lpszClassName = className,
            };
            RegisterClassExW(&wc);

            s_hwnd = CreateWindowExW(
                0, className, windowName,
                WS_OVERLAPPEDWINDOW,
                CW_USEDEFAULT, CW_USEDEFAULT,
                width, height,
                HWND.NULL, HMENU.NULL,
                hInstance, null);

            _ = ShowWindow(s_hwnd, SW_SHOWDEFAULT);
        }
    }

    public static void SetTitle(string title)
    {
        fixed (char* p = title)
            SetWindowTextW(s_hwnd, p);
    }

    public static bool ProcessMessages()
    {
        ScrollDelta = 0;
        Array.Clear(KeyPressed);
        PrevMouseX = MouseX;
        PrevMouseY = MouseY;

        MSG msg;
        while (PeekMessageW(&msg, HWND.NULL, 0, 0, PM_REMOVE) != 0)
        {
            if (msg.message == WM_QUIT)
                return false;
            _ = TranslateMessage(&msg);
            _ = DispatchMessageW(&msg);
        }
        return true;
    }

    [UnmanagedCallersOnly]
    private static LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case WM_DESTROY:
                PostQuitMessage(0);
                return default;

            case WM_SIZE:
                s_width = (int)(lParam.Value & 0xFFFF);
                s_height = (int)((lParam.Value >> 16) & 0xFFFF);
                s_resized = true;
                return default;

            case WM_MOUSEMOVE:
                MouseX = (short)(lParam.Value & 0xFFFF);
                MouseY = (short)((lParam.Value >> 16) & 0xFFFF);
                return default;

            case WM_LBUTTONDOWN:
                LeftButtonDown = true;
                _ = SetCapture(hWnd);
                return default;

            case WM_LBUTTONUP:
                LeftButtonDown = false;
                _ = ReleaseCapture();
                return default;

            case WM_RBUTTONDOWN:
                RightButtonDown = true;
                _ = SetCapture(hWnd);
                return default;

            case WM_RBUTTONUP:
                RightButtonDown = false;
                _ = ReleaseCapture();
                return default;

            case WM_MOUSEWHEEL:
                ScrollDelta += (short)((wParam.Value >> 16) & 0xFFFF);
                return default;

            case WM_KEYDOWN:
                var vk = (int)(wParam.Value & 0xFF);
                if (vk < 256) KeyPressed[vk] = true;
                if (vk == VK_ESCAPE) PostQuitMessage(0);
                return default;
        }
        return DefWindowProcW(hWnd, msg, wParam, lParam);
    }
}
