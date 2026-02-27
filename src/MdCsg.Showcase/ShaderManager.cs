using System;
using System.IO;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.DirectX;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;
using static TerraFX.Interop.DirectX.D3D11_INPUT_CLASSIFICATION;

namespace MdCsg.Showcase;

/// <summary>Compiles HLSL shaders and manages pipeline state objects.</summary>
internal sealed unsafe class ShaderManager : IDisposable
{
    private ID3D11VertexShader* _meshVS;
    private ID3D11PixelShader* _meshPS;
    private ID3D11VertexShader* _wireVS;
    private ID3D11PixelShader* _wirePS;
    private ID3D11InputLayout* _inputLayout;

    public void Initialize(ID3D11Device* device, string shaderDir)
    {
        ID3DBlob* meshVsBlob = null, meshPsBlob = null, wireVsBlob = null, wirePsBlob = null;
        try
        {
            meshVsBlob = CompileFromFile(Path.Combine(shaderDir, "mesh_vs.hlsl"), "vs_5_0"u8);
            meshPsBlob = CompileFromFile(Path.Combine(shaderDir, "mesh_ps.hlsl"), "ps_5_0"u8);
            wireVsBlob = CompileFromFile(Path.Combine(shaderDir, "wireframe_vs.hlsl"), "vs_5_0"u8);
            wirePsBlob = CompileFromFile(Path.Combine(shaderDir, "wireframe_ps.hlsl"), "ps_5_0"u8);

            ID3D11VertexShader* vs;
            ID3D11PixelShader* ps;

            D3DHelper.ThrowIfFailed(device->CreateVertexShader(
                meshVsBlob->GetBufferPointer(), meshVsBlob->GetBufferSize(), null, &vs));
            _meshVS = vs;

            D3DHelper.ThrowIfFailed(device->CreatePixelShader(
                meshPsBlob->GetBufferPointer(), meshPsBlob->GetBufferSize(), null, &ps));
            _meshPS = ps;

            D3DHelper.ThrowIfFailed(device->CreateVertexShader(
                wireVsBlob->GetBufferPointer(), wireVsBlob->GetBufferSize(), null, &vs));
            _wireVS = vs;

            D3DHelper.ThrowIfFailed(device->CreatePixelShader(
                wirePsBlob->GetBufferPointer(), wirePsBlob->GetBufferSize(), null, &ps));
            _wirePS = ps;

            CreateInputLayout(device, meshVsBlob);
        }
        finally
        {
            if (meshVsBlob != null) meshVsBlob->Release();
            if (meshPsBlob != null) meshPsBlob->Release();
            if (wireVsBlob != null) wireVsBlob->Release();
            if (wirePsBlob != null) wirePsBlob->Release();
        }
    }

    public void SetMeshPipeline(ID3D11DeviceContext* ctx)
    {
        ctx->IASetInputLayout(_inputLayout);
        ctx->VSSetShader(_meshVS, null, 0);
        ctx->PSSetShader(_meshPS, null, 0);
    }

    public void SetWireframePipeline(ID3D11DeviceContext* ctx)
    {
        ctx->IASetInputLayout(_inputLayout);
        ctx->VSSetShader(_wireVS, null, 0);
        ctx->PSSetShader(_wirePS, null, 0);
    }

    public void Dispose()
    {
        if (_inputLayout != null) { _inputLayout->Release(); _inputLayout = null; }
        if (_meshVS != null) { _meshVS->Release(); _meshVS = null; }
        if (_meshPS != null) { _meshPS->Release(); _meshPS = null; }
        if (_wireVS != null) { _wireVS->Release(); _wireVS = null; }
        if (_wirePS != null) { _wirePS->Release(); _wirePS = null; }
    }

    private void CreateInputLayout(ID3D11Device* device, ID3DBlob* vsBlob)
    {
        fixed (byte* pPosition = "POSITION"u8)
        fixed (byte* pNormal = "NORMAL"u8)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[2];
            elements[0] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pPosition,
                SemanticIndex = 0,
                Format = DXGI_FORMAT_R32G32B32_FLOAT,
                InputSlot = 0,
                AlignedByteOffset = 0,
                InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA,
                InstanceDataStepRate = 0,
            };
            elements[1] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pNormal,
                SemanticIndex = 0,
                Format = DXGI_FORMAT_R32G32B32_FLOAT,
                InputSlot = 0,
                AlignedByteOffset = 12,
                InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA,
                InstanceDataStepRate = 0,
            };

            ID3D11InputLayout* il;
            D3DHelper.ThrowIfFailed(device->CreateInputLayout(
                elements, 2,
                vsBlob->GetBufferPointer(), vsBlob->GetBufferSize(),
                &il));
            _inputLayout = il;
        }
    }

    private static ID3DBlob* CompileFromFile(string path, ReadOnlySpan<byte> target)
    {
        var source = File.ReadAllBytes(path);
        fixed (byte* pSource = source)
        fixed (byte* pEntry = "main"u8)
        fixed (byte* pTarget = target)
        {
            ID3DBlob* code = null;
            ID3DBlob* errors = null;
            var hr = D3DCompile(
                pSource, (nuint)source.Length,
                (sbyte*)null, null, null,
                (sbyte*)pEntry, (sbyte*)pTarget,
                0, 0, &code, &errors);

            if (hr.Value < 0)
            {
                var msg = errors != null
                    ? new string((sbyte*)errors->GetBufferPointer())
                    : "Unknown error";
                if (errors != null) errors->Release();
                throw new Exception($"Shader compile error ({Path.GetFileName(path)}): {msg}");
            }
            if (errors != null) errors->Release();
            return code;
        }
    }
}
