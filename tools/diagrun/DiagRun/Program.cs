using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Fitting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;

static void PrintCase(string name, Solid a, Solid b, CsgOperation op)
{
    var options = new CsgOptions { Parallel = false };
    var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
    var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, false, options.GridSize, useEdgeSplitConstraints: true);
    var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB, false, options.GridSize, useEdgeSplitConstraints: true);

    var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
    var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
    var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
    var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);

    int aTriWithInt = 0;
    int bTriWithInt = 0;
    foreach (var st in cutA.SubTriangles) if (st.HasIntersectionEdge) aTriWithInt++;
    foreach (var st in cutB.SubTriangles) if (st.HasIntersectionEdge) bTriWithInt++;

    int aAdjInt = 0;
    int bAdjInt = 0;
    for (int i = 0; i < adjA.Count; i++)
        foreach (var n in adjA.GetNeighbors(i))
            if (n.IsIntersectionEdge) aAdjInt++;
    for (int i = 0; i < adjB.Count; i++)
        foreach (var n in adjB.GetNeighbors(i))
            if (n.IsIntersectionEdge) bAdjInt++;

    var classifier = new CpuPatchClassificationStrategy(false);
    classifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh, options.UseWindingNumber);
    classifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh, options.UseWindingNumber);

    int aInside = 0;
    int aOutside = 0;
    int bInside = 0;
    int bOutside = 0;
    foreach (var p in patchesA)
    {
        if (p.IsInside == true) aInside++; else aOutside++;
    }
    foreach (var p in patchesB)
    {
        if (p.IsInside == true) bInside++; else bOutside++;
    }

    var result = op switch
    {
        CsgOperation.Union => Csg.Union(a, b, options),
        CsgOperation.Intersection => Csg.Intersect(a, b, options),
        CsgOperation.Difference => Csg.Difference(a, b, options),
        _ => throw new ArgumentOutOfRangeException(nameof(op))
    };

    var mesh = result.Mesh;
    int components = MeshQueries.ConnectedComponents(mesh).Count;
    var comps = MeshQueries.ConnectedComponents(mesh);
    int boundaries = MeshQueries.BoundaryLoops(mesh).Count;
    int degenerateFaces = 0;
    foreach (var f in mesh.Faces)
    {
        if (f.Normal.LengthSquared < 1e-20)
            degenerateFaces++;
    }

    Console.WriteLine($"=== {name} / {op} ===");
    Console.WriteLine($"segments={intersections.Segments.Count}");
    Console.WriteLine($"cutA tris={cutA.SubTriangles.Count} intTri={aTriWithInt} adjInt={aAdjInt} patches={patchesA.Count} in={aInside} out={aOutside}");
    Console.WriteLine($"cutB tris={cutB.SubTriangles.Count} intTri={bTriWithInt} adjInt={bAdjInt} patches={patchesB.Count} in={bInside} out={bOutside}");
    Console.WriteLine($"result faces={result.FaceCount} verts={result.VertexCount} comp={components} boundaryLoops={boundaries} degenFaces={degenerateFaces}");
    for (int ci = 0; ci < comps.Count && ci < 5; ci++)
    {
        double v = 0;
        foreach (int fi in comps[ci])
        {
            var face = mesh.Faces[fi];
            face.GetTrianglePositions(out var a0, out var b0, out var c0);
            v += Vec3.Dot(a0, Vec3.Cross(b0, c0));
        }
        v /= 6.0;
        Console.WriteLine($"  comp[{ci}] faces={comps[ci].Count} signedVol={v:E4}");
    }
}

var s2a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
var s2b1 = Primitives.Sphere(new Vec3(1, 0, 0), 1.0, 2);
var s2b05 = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 2);
var cube = Primitives.Cube(Vec3.Zero, 2.0);
var sphere15 = Primitives.Sphere(Vec3.Zero, 1.5, 2);
var sphere05 = Primitives.Sphere(Vec3.Zero, 0.5, 2);

PrintCase("sphere2@1.0", s2a, s2b1, CsgOperation.Intersection);
PrintCase("sphere2@1.0", s2a, s2b1, CsgOperation.Union);
PrintCase("sphere2@0.5", s2a, s2b05, CsgOperation.Intersection);
PrintCase("cube2 - sphere0.5", cube, sphere05, CsgOperation.Difference);
PrintCase("cube2 ∩ sphere1.5", cube, sphere15, CsgOperation.Intersection);

var cubeComp = cube.Complement();
var p0 = Vec3.Zero;
Console.WriteLine("=== complement classify ===");
Console.WriteLine($"cube inside(ray)={new BvhPointClassifier(cube.Bvh, useWindingNumber: false).Classify(p0)}");
Console.WriteLine($"cube inside(wind)={new BvhPointClassifier(cube.Bvh, useWindingNumber: true).Classify(p0)}");
Console.WriteLine($"comp inside(ray)={new BvhPointClassifier(cubeComp.Bvh, useWindingNumber: false).Classify(p0)}");
Console.WriteLine($"comp inside(wind)={new BvhPointClassifier(cubeComp.Bvh, useWindingNumber: true).Classify(p0)}");
Console.WriteLine($"cube.IsInside={cube.IsInside(p0)} comp.IsInside={cubeComp.IsInside(p0)}");
var sph = Primitives.Sphere(Vec3.Zero, 1.0, 2);
var sphComp = sph.Complement();
Console.WriteLine($"sphere inside(ray)={new BvhPointClassifier(sph.Bvh, useWindingNumber: false).Classify(p0)}");
Console.WriteLine($"sphereComp inside(ray)={new BvhPointClassifier(sphComp.Bvh, useWindingNumber: false).Classify(p0)}");
Console.WriteLine($"sphere.IsInside={sph.IsInside(p0)} sphereComp.IsInside={sphComp.IsInside(p0)}");

var topHalf = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
var hsCyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
var hsSphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
var hsCube = Primitives.Cube(Vec3.Zero, 2.0);
var hsR1 = Csg.Intersect(hsCyl, topHalf);
var hsR2 = Csg.Intersect(hsSphere, topHalf);
var hsR3 = Csg.Difference(hsSphere, topHalf);
var hsDiag = Csg.Intersect(hsCube, HalfSpace.FromPointAndNormal(Vec3.Zero, new Vec3(1, 1, 0).Normalized));
Console.WriteLine("=== halfspace quick ===");
Console.WriteLine($"cyl∩hs faces={hsR1.FaceCount} vol={new Solid(hsR1.Mesh).Volume():F6}");
Console.WriteLine($"sphere∩hs faces={hsR2.FaceCount} vol={new Solid(hsR2.Mesh).Volume():F6}");
Console.WriteLine($"sphere-hs faces={hsR3.FaceCount} vol={new Solid(hsR3.Mesh).Volume():F6}");
Console.WriteLine($"cube∩diaghs faces={hsDiag.FaceCount} vol={new Solid(hsDiag.Mesh).Volume():F6}");
var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.01), Vec3.UnitZ);
var bot = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.01), -Vec3.UnitZ);
var slab1 = new Solid(Csg.Intersect(hsCube, top).Mesh);
var slab2 = Csg.Intersect(slab1, bot);
Console.WriteLine($"thin slab step1 vol={slab1.Volume():F6} faces={slab1.Mesh.Faces.Count}");
Console.WriteLine($"thin slab step2 vol={new Solid(slab2.Mesh).Volume():F6} faces={slab2.FaceCount}");

var ss = Primitives.Sphere(Vec3.Zero, 1.0, 3);
var ssSimpMesh = MeshSimplifier.Simplify(ss.Mesh, ss.Mesh.Faces.Count / 2);
var ssSimp = new Solid(ssSimpMesh);
Console.WriteLine("=== simplification quick ===");
Console.WriteLine($"sphere orig faces={ss.Mesh.Faces.Count} vol={ss.Volume():F6} boundaries={MeshQueries.BoundaryLoops(ss.Mesh).Count}");
Console.WriteLine($"sphere simp faces={ssSimp.Mesh.Faces.Count} vol={ssSimp.Volume():F6} signed={MeshQueries.Volume(ssSimp.Mesh):F6} boundaries={MeshQueries.BoundaryLoops(ssSimp.Mesh).Count}");
Console.WriteLine($"sphere simp isInside(0)={ssSimp.IsInside(Vec3.Zero)}");

var cubeSm = Primitives.Cube(Vec3.Zero, 2.0);
var cubeSmoothed = MeshSmoothing.LaplacianSmooth(cubeSm.Mesh, iterations: 1, lambda: 0.3);
Console.WriteLine("=== smoothing quick ===");
Console.WriteLine($"cube orig vol={cubeSm.Volume():F6}, smoothed absVol={System.Math.Abs(MeshQueries.Volume(cubeSmoothed)):F6}, signed={MeshQueries.Volume(cubeSmoothed):F6}, boundaries={MeshQueries.BoundaryLoops(cubeSmoothed).Count}");

Solid UnitCube(Vec3 offset)
{
    var positions = new Vec3[]
    {
        new Vec3(0, 0, 0) + offset,
        new Vec3(1, 0, 0) + offset,
        new Vec3(1, 1, 0) + offset,
        new Vec3(0, 1, 0) + offset,
        new Vec3(0, 0, 1) + offset,
        new Vec3(1, 0, 1) + offset,
        new Vec3(1, 1, 1) + offset,
        new Vec3(0, 1, 1) + offset,
    };
    var triangles = new (int, int, int)[]
    {
        (0, 2, 1), (0, 3, 2),
        (4, 5, 6), (4, 6, 7),
        (0, 1, 5), (0, 5, 4),
        (2, 3, 7), (2, 7, 6),
        (0, 4, 7), (0, 7, 3),
        (1, 2, 6), (1, 6, 5),
    };
    return Solid.FromIndexed(positions, triangles);
}

var cA = UnitCube(Vec3.Zero);
var cSmall = UnitCube(new Vec3(0.9, 0.1, 0.1));
var cLarge = UnitCube(new Vec3(0.1, 0.1, 0.1));
var cRs = Csg.Intersect(cA, cSmall);
var cRl = Csg.Intersect(cA, cLarge);
Console.WriteLine("=== intersect monotonic ===");
Console.WriteLine($"small faces={cRs.FaceCount} vol={new Solid(cRs.Mesh).Volume():F6}");
Console.WriteLine($"large faces={cRl.FaceCount} vol={new Solid(cRl.Mesh).Volume():F6}");

var ciA = Primitives.Cube(Vec3.Zero, 2.0);
var ciB = Primitives.Sphere(Vec3.Zero, 1.2, 2);
var ciC = Primitives.Box(new Vec3(0.2, 0.2, 0), new Vec3(1.6, 1.6, 1.6));
var ciR1 = new Solid(Csg.Intersect(ciA, ciB).Mesh);
var ciR2 = Csg.Intersect(ciR1, ciC);
Console.WriteLine("=== chained intersections ===");
Console.WriteLine($"r1 faces={ciR1.Mesh.Faces.Count} vol={ciR1.Volume():F6}");
Console.WriteLine($"r2 faces={ciR2.FaceCount} vol={new Solid(ciR2.Mesh).Volume():F6}");

var nm = NelderMead.Minimize(x => (x[0] + 5) * (x[0] + 5), new[] { -10.0 });
Console.WriteLine("=== nelder mead ===");
Console.WriteLine($"solution={nm.Solution[0]:F12} value={nm.Value:E6} iter={nm.Iterations}");
