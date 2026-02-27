using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using MdCsg.Math;
using MdCsg.Mesh;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.DirectX;
using static TerraFX.Interop.DirectX.D3D11;
using static TerraFX.Interop.DirectX.DXGI;
using static TerraFX.Interop.Windows.Windows;

namespace MdCsg.Showcase;

[StructLayout(LayoutKind.Sequential)]
internal struct GpuConstants
{
    public Matrix4x4 WorldViewProj;
    public Matrix4x4 World;
    public Vector3 CameraPos;
    public float Pad0;
    public Vector3 LightDir;
    public float Pad1;
    public Vector4 MaterialColor;
}

/// <summary>Direct3D 11 rendering engine.</summary>
internal sealed unsafe class Renderer : IDisposable
{
    private ID3D11Device* _device;
    private ID3D11DeviceContext* _context;
    private IDXGISwapChain* _swapChain;
    private ID3D11RenderTargetView* _rtv;
    private ID3D11DepthStencilView* _dsv;
    private ID3D11Texture2D* _depthTexture;
    private ID3D11Buffer* _constantBuffer;
    private ID3D11RasterizerState* _solidRasterState;
    private ID3D11RasterizerState* _wireRasterState;
    private ShaderManager? _shaderManager;
    private int _width, _height;

    public bool WireframeMode;
    public bool FlatShading = true;
    public float AspectRatio => _width > 0 && _height > 0 ? (float)_width / _height : 1f;

    // Factory methods so scenes don't need unsafe context
    public MeshBuffer CreateFlatBuffer(HalfEdgeMesh mesh) => MeshBuffer.CreateFlat(_device, mesh);
    public MeshBuffer CreateSmoothBuffer(HalfEdgeMesh mesh) => MeshBuffer.CreateSmooth(_device, mesh);
    public MeshBuffer CreatePointBuffer(Vec3[] points) => MeshBuffer.CreatePoints(_device, points);

    public void Initialize(HWND hwnd, int width, int height)
    {
        _width = width;
        _height = height;

        var swapDesc = new DXGI_SWAP_CHAIN_DESC
        {
            BufferDesc = new DXGI_MODE_DESC
            {
                Width = (uint)width,
                Height = (uint)height,
                Format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
            },
            SampleDesc = new DXGI_SAMPLE_DESC { Count = 1 },
            BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
            BufferCount = 2,
            OutputWindow = hwnd,
            Windowed = TRUE,
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_DISCARD,
        };

        var featureLevel = D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0;
        IDXGISwapChain* sc;
        ID3D11Device* dev;
        ID3D11DeviceContext* ctx;
        D3DHelper.ThrowIfFailed(D3D11CreateDeviceAndSwapChain(
            (IDXGIAdapter*)null,
            D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
            HMODULE.NULL, 0,
            &featureLevel, 1, D3D11_SDK_VERSION,
            &swapDesc, &sc, &dev, null, &ctx));
        _swapChain = sc;
        _device = dev;
        _context = ctx;

        CreateRenderTargets();
        CreateConstantBuffer();
        CreateRasterizerStates();

        _shaderManager = new ShaderManager();
        _shaderManager.Initialize(_device, Path.Combine(AppContext.BaseDirectory, "Shaders"));
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        _width = width;
        _height = height;

        _context->OMSetRenderTargets(0, null, null);
        ReleaseRenderTargets();

        D3DHelper.ThrowIfFailed(_swapChain->ResizeBuffers(
            0, (uint)width, (uint)height, DXGI_FORMAT.DXGI_FORMAT_UNKNOWN, 0));
        CreateRenderTargets();
    }

    public void BeginFrame()
    {
        float* bg = stackalloc float[4] { 0.12f, 0.12f, 0.16f, 1f };
        _context->ClearRenderTargetView(_rtv, bg);
        _context->ClearDepthStencilView(_dsv, (uint)D3D11_CLEAR_FLAG.D3D11_CLEAR_DEPTH, 1f, 0);

        var vp = new D3D11_VIEWPORT { Width = _width, Height = _height, MaxDepth = 1f };
        _context->RSSetViewports(1, &vp);

        var rtv = _rtv;
        _context->OMSetRenderTargets(1, &rtv, _dsv);
    }

    public void EndFrame()
    {
        _swapChain->Present(1, 0);
    }

    public void UpdateConstants(Matrix4x4 worldViewProj, Matrix4x4 world,
        Vector3 cameraPos, Vector3 lightDir, Vector4 materialColor)
    {
        D3D11_MAPPED_SUBRESOURCE mapped;
        D3DHelper.ThrowIfFailed(_context->Map(
            (ID3D11Resource*)_constantBuffer, 0,
            D3D11_MAP.D3D11_MAP_WRITE_DISCARD, 0, &mapped));

        var cb = (GpuConstants*)mapped.pData;
        cb->WorldViewProj = worldViewProj;
        cb->World = world;
        cb->CameraPos = cameraPos;
        cb->LightDir = lightDir;
        cb->MaterialColor = materialColor;

        _context->Unmap((ID3D11Resource*)_constantBuffer, 0);

        var cbPtr = _constantBuffer;
        _context->VSSetConstantBuffers(0, 1, &cbPtr);
        _context->PSSetConstantBuffers(0, 1, &cbPtr);
    }

    public void DrawMesh(MeshBuffer buffer)
    {
        if (WireframeMode)
        {
            _shaderManager!.SetWireframePipeline(_context);
            _context->RSSetState(_wireRasterState);
        }
        else
        {
            _shaderManager!.SetMeshPipeline(_context);
            _context->RSSetState(_solidRasterState);
        }
        buffer.Bind(_context);
        buffer.Draw(_context);
    }

    public void DrawPoints(MeshBuffer buffer)
    {
        _shaderManager!.SetWireframePipeline(_context);
        _context->RSSetState(_solidRasterState);
        buffer.Bind(_context);
        buffer.Draw(_context, D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_POINTLIST);
    }

    public void SaveBackBuffer(string path)
    {
        // Get back buffer
        ID3D11Texture2D* backBuffer;
        var iid = __uuidof<ID3D11Texture2D>();
        D3DHelper.ThrowIfFailed(_swapChain->GetBuffer(0, iid, (void**)&backBuffer));

        // Create staging texture
        D3D11_TEXTURE2D_DESC bbDesc;
        backBuffer->GetDesc(&bbDesc);
        var stagingDesc = new D3D11_TEXTURE2D_DESC
        {
            Width = bbDesc.Width,
            Height = bbDesc.Height,
            MipLevels = 1,
            ArraySize = 1,
            Format = bbDesc.Format,
            SampleDesc = new DXGI_SAMPLE_DESC { Count = 1 },
            Usage = D3D11_USAGE.D3D11_USAGE_STAGING,
            CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_READ,
        };
        ID3D11Texture2D* staging;
        D3DHelper.ThrowIfFailed(_device->CreateTexture2D(&stagingDesc, null, &staging));

        _context->CopyResource((ID3D11Resource*)staging, (ID3D11Resource*)backBuffer);
        backBuffer->Release();

        // Map and read pixels
        D3D11_MAPPED_SUBRESOURCE mapped;
        D3DHelper.ThrowIfFailed(_context->Map((ID3D11Resource*)staging, 0,
            D3D11_MAP.D3D11_MAP_READ, 0, &mapped));

        int w = (int)bbDesc.Width, h = (int)bbDesc.Height;
        int rowPitch = (int)mapped.RowPitch;
        var pixels = new byte[w * h * 3];

        // Convert RGBA (top-to-bottom) to BGR (bottom-to-top) for BMP
        for (int y = 0; y < h; y++)
        {
            var src = (byte*)mapped.pData + y * rowPitch;
            int dstRow = (h - 1 - y) * w * 3;
            for (int x = 0; x < w; x++)
            {
                pixels[dstRow + x * 3 + 0] = src[x * 4 + 2]; // B
                pixels[dstRow + x * 3 + 1] = src[x * 4 + 1]; // G
                pixels[dstRow + x * 3 + 2] = src[x * 4 + 0]; // R
            }
        }

        _context->Unmap((ID3D11Resource*)staging, 0);
        ((IUnknown*)staging)->Release();

        // Write BMP
        int bmpRowSize = (w * 3 + 3) & ~3;
        int pixelDataSize = bmpRowSize * h;
        int fileSize = 14 + 40 + pixelDataSize;

        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        // File header
        bw.Write((byte)'B'); bw.Write((byte)'M');
        bw.Write(fileSize);
        bw.Write(0); // reserved
        bw.Write(14 + 40); // pixel data offset
        // DIB header (BITMAPINFOHEADER)
        bw.Write(40); // header size
        bw.Write(w);
        bw.Write(h);
        bw.Write((short)1); // planes
        bw.Write((short)24); // bpp
        bw.Write(0); // compression
        bw.Write(pixelDataSize);
        bw.Write(2835); // X pixels per meter
        bw.Write(2835); // Y pixels per meter
        bw.Write(0); // colors used
        bw.Write(0); // important colors
        // Pixel data (pad each row)
        int pad = bmpRowSize - w * 3;
        for (int y = 0; y < h; y++)
        {
            bw.Write(pixels, y * w * 3, w * 3);
            for (int p = 0; p < pad; p++) bw.Write((byte)0);
        }
    }

    public void Dispose()
    {
        _shaderManager?.Dispose();
        Release(ref _constantBuffer);
        Release(ref _wireRasterState);
        Release(ref _solidRasterState);
        ReleaseRenderTargets();
        Release(ref _swapChain);
        Release(ref _context);
        Release(ref _device);
    }

    private void CreateRenderTargets()
    {
        ID3D11Texture2D* backBuffer;
        var iid = __uuidof<ID3D11Texture2D>();
        D3DHelper.ThrowIfFailed(_swapChain->GetBuffer(0, iid, (void**)&backBuffer));

        ID3D11RenderTargetView* rtv;
        D3DHelper.ThrowIfFailed(_device->CreateRenderTargetView((ID3D11Resource*)backBuffer, null, &rtv));
        _rtv = rtv;
        backBuffer->Release();

        var depthDesc = new D3D11_TEXTURE2D_DESC
        {
            Width = (uint)_width,
            Height = (uint)_height,
            MipLevels = 1,
            ArraySize = 1,
            Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT,
            SampleDesc = new DXGI_SAMPLE_DESC { Count = 1 },
            Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT,
            BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL,
        };
        ID3D11Texture2D* dt;
        D3DHelper.ThrowIfFailed(_device->CreateTexture2D(&depthDesc, null, &dt));
        _depthTexture = dt;

        ID3D11DepthStencilView* dsv;
        D3DHelper.ThrowIfFailed(_device->CreateDepthStencilView(
            (ID3D11Resource*)_depthTexture, null, &dsv));
        _dsv = dsv;
    }

    private void ReleaseRenderTargets()
    {
        Release(ref _dsv);
        Release(ref _depthTexture);
        Release(ref _rtv);
    }

    private void CreateConstantBuffer()
    {
        var desc = new D3D11_BUFFER_DESC
        {
            ByteWidth = (uint)sizeof(GpuConstants),
            Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC,
            BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_CONSTANT_BUFFER,
            CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE,
        };
        ID3D11Buffer* buf;
        D3DHelper.ThrowIfFailed(_device->CreateBuffer(&desc, null, &buf));
        _constantBuffer = buf;
    }

    private void CreateRasterizerStates()
    {
        var solidDesc = new D3D11_RASTERIZER_DESC
        {
            FillMode = D3D11_FILL_MODE.D3D11_FILL_SOLID,
            CullMode = D3D11_CULL_MODE.D3D11_CULL_NONE,
            DepthClipEnable = TRUE,
        };
        ID3D11RasterizerState* rs;
        D3DHelper.ThrowIfFailed(_device->CreateRasterizerState(&solidDesc, &rs));
        _solidRasterState = rs;

        var wireDesc = new D3D11_RASTERIZER_DESC
        {
            FillMode = D3D11_FILL_MODE.D3D11_FILL_WIREFRAME,
            CullMode = D3D11_CULL_MODE.D3D11_CULL_NONE,
            DepthClipEnable = TRUE,
        };
        D3DHelper.ThrowIfFailed(_device->CreateRasterizerState(&wireDesc, &rs));
        _wireRasterState = rs;
    }

    private static void Release<T>(ref T* ptr) where T : unmanaged
    {
        if (ptr != null)
        {
            ((IUnknown*)ptr)->Release();
            ptr = null;
        }
    }
}
