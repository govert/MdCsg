using System.Globalization;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust;
using MdCsg.Robust.Diagnostics.Replay;
using MdCsg.Robust.Kernel.Arrangement;

string outputDir = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..",
        "tests", "MdCsg.Robust.Conformance", "ReplayCorpus", "arrangement"));
Directory.CreateDirectory(outputDir);

var strict = new RobustOperationOptions
{
    Mode = RobustMode.Strict,
    Deterministic = true,
    UseRobustTriangulationKernel = true
};

var cases = new List<(string FileName, Solid A, Solid B)>
{
    (
        "showcase-offset-sphere-cube.txt",
        Primitives.Sphere(Vec3.Zero, 1.2, 3),
        Primitives.Cube(new Vec3(0.6, 0, 0), 1.5)
    ),
    (
        "coplanar-shared-face-cubes.txt",
        Primitives.Cube(Vec3.Zero, 2.0),
        Primitives.Cube(new Vec3(2, 0, 0), 2.0)
    ),
    (
        "stable-overlap-cubes.txt",
        Primitives.Cube(Vec3.Zero, 2.0),
        Primitives.Cube(new Vec3(0.75, 0, 0), 2.0)
    ),
    (
        "kissing-spheres.txt",
        Primitives.Sphere(Vec3.Zero, 1.0, 3),
        Primitives.Sphere(new Vec3(2.0, 0, 0), 1.0, 3)
    )
};

// Chained showcase step-3 input pair (step2 mesh vs cylY).
{
    var y = new Vec3(0, 1, 0);
    var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
    var box = Primitives.Cube(Vec3.Zero, 1.8);
    var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
    var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

    var step1 = RobustCsg.Intersect(sphere, box, strict);
    if (!step1.Succeeded || step1.Result is null)
        throw new InvalidOperationException("Failed to produce chained-step1 corpus mesh.");

    var step2 = RobustCsg.Difference(new Solid(step1.Result.Mesh), cylX, strict);
    if (!step2.Succeeded || step2.Result is null)
        throw new InvalidOperationException("Failed to produce chained-step2 corpus mesh.");

    cases.Add(("showcase-chained-step3-input.txt", new Solid(step2.Result.Mesh), cylY));
}

var manifestLines = new List<string>
{
    "case_file,arrangement_vertices,arrangement_edges,endpoint_vertices,connected_components,coplanar_face_a,coplanar_face_b,coplanar_oppose"
};

foreach (var (fileName, a, b) in cases)
{
    var replay = ArrangementReplayCodec.Capture(a.Mesh, b.Mesh);
    string casePath = Path.Combine(outputDir, fileName);
    ArrangementReplayCodec.Save(casePath, replay);

    var graph = ArrangementReplayRunner.BuildArrangement(replay);
    var analysis = ArrangementReplayRunner.AnalyzeArrangement(replay);
    manifestLines.Add(string.Join(",",
        fileName,
        graph.Vertices.Count.ToString(CultureInfo.InvariantCulture),
        graph.Edges.Count.ToString(CultureInfo.InvariantCulture),
        analysis.EndpointVertexCount.ToString(CultureInfo.InvariantCulture),
        analysis.ConnectedComponentCount.ToString(CultureInfo.InvariantCulture),
        graph.CoplanarFaceCountA.ToString(CultureInfo.InvariantCulture),
        graph.CoplanarFaceCountB.ToString(CultureInfo.InvariantCulture),
        graph.CoplanarPairNormalsOpposeCount.ToString(CultureInfo.InvariantCulture)));
}

File.WriteAllLines(Path.Combine(outputDir, "manifest.csv"), manifestLines);
Console.WriteLine($"Wrote {cases.Count} replay cases to {outputDir}");
