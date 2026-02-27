using System;
using MdCsg.Math;
using MdCsg.Mesh;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;
using static TerraFX.Interop.DirectX.D3D_PRIMITIVE_TOPOLOGY;

namespace MdCsg.Showcase;

/// <summary>GPU vertex/index buffers created from MdCsg meshes.</summary>
internal sealed unsafe class MeshBuffer : IDisposable
{
    private ID3D11Buffer* _vertexBuffer;
    private ID3D11Buffer* _indexBuffer;
    private uint _vertexCount;
    private uint _indexCount;
    private bool _hasIndices;

    public uint VertexCount => _vertexCount;
    public uint IndexCount => _indexCount;
    public int FaceCount => (int)(_hasIndices ? _indexCount / 3 : _vertexCount / 3);

    private MeshBuffer() { }

    /// <summary>Flat shading: duplicated vertices with per-face normals.</summary>
    public static MeshBuffer CreateFlat(ID3D11Device* device, HalfEdgeMesh mesh)
    {
        var fc = mesh.Faces.Count;
        var verts = new GpuVertex[fc * 3];
        var indices = new uint[fc * 3];
        int vi = 0;

        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var n = face.UnitNormal;
            float nx = (float)n.X, ny = (float)n.Y, nz = (float)n.Z;

            verts[vi] = new GpuVertex { PX = (float)a.X, PY = (float)a.Y, PZ = (float)a.Z, NX = nx, NY = ny, NZ = nz };
            indices[vi] = (uint)vi; vi++;
            verts[vi] = new GpuVertex { PX = (float)b.X, PY = (float)b.Y, PZ = (float)b.Z, NX = nx, NY = ny, NZ = nz };
            indices[vi] = (uint)vi; vi++;
            verts[vi] = new GpuVertex { PX = (float)c.X, PY = (float)c.Y, PZ = (float)c.Z, NX = nx, NY = ny, NZ = nz };
            indices[vi] = (uint)vi; vi++;
        }

        var buf = new MeshBuffer { _vertexCount = (uint)vi, _indexCount = (uint)vi, _hasIndices = true };
        CreateGpuBuffer(device, verts, (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER, out buf._vertexBuffer);
        CreateGpuBuffer(device, indices, (uint)D3D11_BIND_FLAG.D3D11_BIND_INDEX_BUFFER, out buf._indexBuffer);
        return buf;
    }

    /// <summary>Smooth shading: shared vertices with averaged normals.</summary>
    public static MeshBuffer CreateSmooth(ID3D11Device* device, HalfEdgeMesh mesh)
    {
        var vc = mesh.Vertices.Count;
        var verts = new GpuVertex[vc];

        for (int i = 0; i < vc; i++)
        {
            var v = mesh.Vertices[i];
            var p = v.Position;
            var n = MeshQueries.VertexNormal(v, mesh);
            verts[i] = new GpuVertex
            {
                PX = (float)p.X, PY = (float)p.Y, PZ = (float)p.Z,
                NX = (float)n.X, NY = (float)n.Y, NZ = (float)n.Z
            };
        }

        var fc = mesh.Faces.Count;
        var indices = new uint[fc * 3];
        int ii = 0;
        foreach (var face in mesh.Faces)
        {
            var fv = face.GetVertices();
            indices[ii++] = (uint)fv[0].Id;
            indices[ii++] = (uint)fv[1].Id;
            indices[ii++] = (uint)fv[2].Id;
        }

        var buf = new MeshBuffer { _vertexCount = (uint)vc, _indexCount = (uint)ii, _hasIndices = true };
        CreateGpuBuffer(device, verts, (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER, out buf._vertexBuffer);
        CreateGpuBuffer(device, indices, (uint)D3D11_BIND_FLAG.D3D11_BIND_INDEX_BUFFER, out buf._indexBuffer);
        return buf;
    }

    /// <summary>Point cloud: position only, no index buffer.</summary>
    public static MeshBuffer CreatePoints(ID3D11Device* device, Vec3[] points)
    {
        var verts = new GpuVertex[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            verts[i] = new GpuVertex { PX = (float)p.X, PY = (float)p.Y, PZ = (float)p.Z };
        }

        var buf = new MeshBuffer { _vertexCount = (uint)points.Length, _hasIndices = false };
        CreateGpuBuffer(device, verts, (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER, out buf._vertexBuffer);
        return buf;
    }

    public void Bind(ID3D11DeviceContext* ctx)
    {
        var stride = (uint)sizeof(GpuVertex);
        uint offset = 0;
        var vb = _vertexBuffer;
        ctx->IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        if (_hasIndices)
            ctx->IASetIndexBuffer(_indexBuffer, DXGI_FORMAT_R32_UINT, 0);
    }

    public void Draw(ID3D11DeviceContext* ctx, D3D_PRIMITIVE_TOPOLOGY topology = D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST)
    {
        ctx->IASetPrimitiveTopology(topology);
        if (_hasIndices)
            ctx->DrawIndexed(_indexCount, 0, 0);
        else
            ctx->Draw(_vertexCount, 0);
    }

    public void Dispose()
    {
        if (_vertexBuffer != null) { _vertexBuffer->Release(); _vertexBuffer = null; }
        if (_indexBuffer != null) { _indexBuffer->Release(); _indexBuffer = null; }
    }

    private static void CreateGpuBuffer<T>(ID3D11Device* device, T[] data, uint bindFlags, out ID3D11Buffer* buffer) where T : unmanaged
    {
        fixed (T* pData = data)
        {
            var desc = new D3D11_BUFFER_DESC
            {
                ByteWidth = (uint)(data.Length * sizeof(T)),
                Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT,
                BindFlags = bindFlags,
            };
            var init = new D3D11_SUBRESOURCE_DATA { pSysMem = pData };
            ID3D11Buffer* buf;
            D3DHelper.ThrowIfFailed(device->CreateBuffer(&desc, &init, &buf));
            buffer = buf;
        }
    }
}
