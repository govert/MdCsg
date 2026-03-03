using System;
using System.Linq;
using System.Numerics;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust;

namespace MdCsg.Showcase.RobustTest;

internal enum IntersectionViewMode
{
    Inputs = 0,
    IntersectionOnly = 1,
    CubeMinusSphere = 2,
    SphereMinusCube = 3,
    ExplodedParts = 4
}

internal sealed class IntersectionDebugScene : IDisposable
{
    private sealed class PartBuffers : IDisposable
    {
        public required MeshBuffer Flat;
        public required MeshBuffer Smooth;
        public required Vector4 Color;
        public required Vector3 Offset;
        public required string Label;

        public MeshBuffer Buffer(bool flatShading) => flatShading ? Flat : Smooth;

        public void Dispose()
        {
            Flat.Dispose();
            Smooth.Dispose();
        }
    }

    private static readonly RobustOperationOptions RobustOnlyOptions = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true,
        AttemptResidualDegenerateClosure = true
    };

    private readonly PartBuffers[] _inputs;
    private readonly PartBuffers[] _parts;
    private IntersectionViewMode _mode = IntersectionViewMode.ExplodedParts;

    public IntersectionDebugScene(Renderer renderer)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.2);
        var sphere = Primitives.Sphere(new Vec3(0.45, 0.2, 0), 1.45, 2);

        var intersection = RequireRobustSuccess("Intersect(cube, sphere)", RobustCsg.Intersect(cube, sphere, RobustOnlyOptions));
        var cubeMinusSphere = RequireRobustSuccess("Difference(cube, sphere)", RobustCsg.Difference(cube, sphere, RobustOnlyOptions));
        var sphereMinusCube = RequireRobustSuccess("Difference(sphere, cube)", RobustCsg.Difference(sphere, cube, RobustOnlyOptions));

        _inputs =
        [
            BuildPart(renderer, cube, new Vector4(0.20f, 0.50f, 0.85f, 1f), new Vector3(-2.2f, 0f, 0f), "Cube"),
            BuildPart(renderer, sphere, new Vector4(0.95f, 0.62f, 0.20f, 1f), new Vector3(2.2f, 0f, 0f), "Rough Sphere")
        ];

        _parts =
        [
            BuildPart(renderer, new Solid(intersection.Mesh), new Vector4(0.20f, 0.80f, 0.28f, 1f), Vector3.Zero, "Intersection"),
            BuildPart(renderer, new Solid(cubeMinusSphere.Mesh), new Vector4(0.30f, 0.58f, 0.95f, 1f), new Vector3(-3.4f, 0f, 0f), "Cube - Sphere"),
            BuildPart(renderer, new Solid(sphereMinusCube.Mesh), new Vector4(0.96f, 0.66f, 0.25f, 1f), new Vector3(3.4f, 0f, 0f), "Sphere - Cube")
        ];
    }

    public void SetMode(IntersectionViewMode mode) => _mode = mode;

    public string ModeLabel => _mode switch
    {
        IntersectionViewMode.Inputs => "Inputs (1)",
        IntersectionViewMode.IntersectionOnly => "Intersection (2)",
        IntersectionViewMode.CubeMinusSphere => "Cube Minus Sphere (3)",
        IntersectionViewMode.SphereMinusCube => "Sphere Minus Cube (4)",
        _ => "Exploded Parts (5)"
    };

    public void Render(Renderer renderer, Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix(renderer.AspectRatio);
        var light = Vector3.Normalize(new Vector3(0.35f, -1f, 0.2f));

        switch (_mode)
        {
            case IntersectionViewMode.Inputs:
                DrawParts(renderer, camera, view, proj, light, _inputs, true);
                break;
            case IntersectionViewMode.IntersectionOnly:
                DrawParts(renderer, camera, view, proj, light, [_parts[0]], false);
                break;
            case IntersectionViewMode.CubeMinusSphere:
                DrawParts(renderer, camera, view, proj, light, [_parts[1]], false);
                break;
            case IntersectionViewMode.SphereMinusCube:
                DrawParts(renderer, camera, view, proj, light, [_parts[2]], false);
                break;
            default:
                DrawParts(renderer, camera, view, proj, light, _parts, true);
                break;
        }
    }

    public void Dispose()
    {
        foreach (var part in _inputs)
            part.Dispose();
        foreach (var part in _parts)
            part.Dispose();
    }

    private static void DrawParts(
        Renderer renderer,
        Camera camera,
        Matrix4x4 view,
        Matrix4x4 proj,
        Vector3 light,
        PartBuffers[] parts,
        bool useOffsets)
    {
        foreach (var part in parts)
        {
            var world = useOffsets
                ? Matrix4x4.CreateTranslation(part.Offset)
                : Matrix4x4.Identity;
            renderer.UpdateConstants(world * view * proj, world, camera.Eye, light, part.Color);
            renderer.DrawMesh(part.Buffer(renderer.FlatShading));
        }
    }

    private static PartBuffers BuildPart(
        Renderer renderer,
        Solid solid,
        Vector4 color,
        Vector3 offset,
        string label)
        => new()
        {
            Flat = renderer.CreateFlatBuffer(solid.Mesh),
            Smooth = renderer.CreateSmoothBuffer(solid.Mesh),
            Color = color,
            Offset = offset,
            Label = label
        };

    private static Solid RequireRobustSuccess(string opName, RobustCsgResult result)
    {
        if (result.Succeeded && result.Result is not null)
            return new Solid(result.Result.Mesh);

        string issueSummary = result.Issues.Count == 0
            ? "<none>"
            : string.Join(
                " | ",
                result.Issues.Select(static i => $"{i.Severity}:{i.Code}:{i.Message}"));
        throw new InvalidOperationException(
            $"Robust-only kernel failed for {opName}. Issues: {issueSummary}");
    }
}
