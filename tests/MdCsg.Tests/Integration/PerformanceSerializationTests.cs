using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 47: Timed: serialize/deserialize large meshes.</summary>
public class PerformanceSerializationTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceSerializationTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void SerializeCube_Under1Second()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(cube.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize cube: {sw.ElapsedMilliseconds}ms, {ms.Length} bytes");
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void DeserializeCube_Under1Second()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            MeshSerializer.Serialize(cube.Mesh, writer);
        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        var sw = Stopwatch.StartNew();
        var mesh = MeshSerializer.Deserialize(reader);
        sw.Stop();
        _output.WriteLine($"Deserialize cube: {sw.ElapsedMilliseconds}ms, {mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void SerializeSphere_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(sphere.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize sphere(3): {sw.ElapsedMilliseconds}ms, {ms.Length} bytes, {sphere.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void SerializeHighPolySphere_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(sphere.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize sphere(4): {sw.ElapsedMilliseconds}ms, {ms.Length} bytes, {sphere.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void DeserializeHighPolySphere_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            MeshSerializer.Serialize(sphere.Mesh, writer);
        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        var sw = Stopwatch.StartNew();
        var mesh = MeshSerializer.Deserialize(reader);
        sw.Stop();
        _output.WriteLine($"Deserialize sphere(4): {sw.ElapsedMilliseconds}ms, {mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void RoundtripSphere_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        using var ms = new MemoryStream();
        var sw = Stopwatch.StartNew();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            MeshSerializer.Serialize(sphere.Mesh, writer);
        ms.Position = 0;
        using (var reader = new BinaryReader(ms))
            MeshSerializer.Deserialize(reader);
        sw.Stop();
        _output.WriteLine($"Roundtrip sphere(3): {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SerializeTorus_Under5Seconds()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 3.0, 1.0, 48, 24);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(torus.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize torus(48,24): {sw.ElapsedMilliseconds}ms, {ms.Length} bytes, {torus.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SerializeCsgResult_Under5Seconds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.2);
        var result = new Solid(Csg.Intersect(a, b).Mesh);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(result.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize CSG result: {sw.ElapsedMilliseconds}ms, {ms.Length} bytes, {result.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void MultipleSerializations_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            MeshSerializer.Serialize(cube.Mesh, writer);
        }
        sw.Stop();
        _output.WriteLine($"100 cube serializations: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void MultipleDeserializations_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            MeshSerializer.Serialize(cube.Mesh, writer);
        var bytes = ms.ToArray();
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            using var readMs = new MemoryStream(bytes);
            using var reader = new BinaryReader(readMs);
            MeshSerializer.Deserialize(reader);
        }
        sw.Stop();
        _output.WriteLine($"100 cube deserializations: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SerializeCylinder_Under2Seconds()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 48);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(cyl.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize cylinder(48): {sw.ElapsedMilliseconds}ms, {ms.Length} bytes");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void RoundtripCsgResult_Under10Seconds()
    {
        var a = Primitives.Sphere(Vec3.Zero, 2.0, 3);
        var b = Primitives.Cube(Vec3.Zero, 1.5);
        var result = new Solid(Csg.Intersect(a, b).Mesh);
        using var ms = new MemoryStream();
        var sw = Stopwatch.StartNew();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            MeshSerializer.Serialize(result.Mesh, writer);
        ms.Position = 0;
        HalfEdgeMesh deserialized;
        using (var reader = new BinaryReader(ms))
            deserialized = MeshSerializer.Deserialize(reader);
        sw.Stop();
        _output.WriteLine($"Roundtrip CSG: {sw.ElapsedMilliseconds}ms, {deserialized.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 10000);
        Assert.Equal(result.Mesh.Faces.Count, deserialized.Faces.Count);
    }

    [Fact]
    public void SerializeSize_CubeIsSmall()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        MeshSerializer.Serialize(cube.Mesh, writer);
        _output.WriteLine($"Cube size: {ms.Length} bytes");
        Assert.True(ms.Length < 10000);
    }

    [Fact]
    public void SerializeSize_SphereReasonable()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        MeshSerializer.Serialize(sphere.Mesh, writer);
        _output.WriteLine($"Sphere(3) size: {ms.Length} bytes, {sphere.Mesh.Faces.Count} faces");
        Assert.True(ms.Length < 1_000_000);
    }

    [Fact]
    public void DeserializeCsgResult_VolumePreserved()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var result = new Solid(Csg.Difference(a, b).Mesh);
        var originalVol = result.Volume();

        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            MeshSerializer.Serialize(result.Mesh, writer);
        ms.Position = 0;
        HalfEdgeMesh deserialized;
        using (var reader = new BinaryReader(ms))
            deserialized = MeshSerializer.Deserialize(reader);

        var roundtripSolid = new Solid(deserialized);
        var roundtripVol = roundtripSolid.Volume();
        _output.WriteLine($"Original vol={originalVol:F4}, roundtrip vol={roundtripVol:F4}");
        Assert.True(System.Math.Abs(originalVol - roundtripVol) < 0.01);
    }

    [Fact]
    public void SerializeCone_Under2Seconds()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 48);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var sw = Stopwatch.StartNew();
        MeshSerializer.Serialize(cone.Mesh, writer);
        sw.Stop();
        _output.WriteLine($"Serialize cone(48): {sw.ElapsedMilliseconds}ms, {ms.Length} bytes");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void RoundtripMultiplePrimitives_Under10Seconds()
    {
        var primitives = new[] {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0),
            Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0),
            Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0),
            Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5)
        };
        var sw = Stopwatch.StartNew();
        foreach (var prim in primitives)
        {
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
                MeshSerializer.Serialize(prim.Mesh, writer);
            ms.Position = 0;
            using (var reader = new BinaryReader(ms))
                MeshSerializer.Deserialize(reader);
        }
        sw.Stop();
        _output.WriteLine($"5 primitive roundtrips: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }
}
