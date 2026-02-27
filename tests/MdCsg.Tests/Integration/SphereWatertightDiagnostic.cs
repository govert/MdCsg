using MdCsg.Api;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

public class SphereWatertightDiagnostic
{
    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseCutMeshBoundaryDistances()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, false, options.GridSize);

        var mesh = cutA.Mesh;
        var boundaryEdges = new List<HalfEdge>();
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null)
                boundaryEdges.Add(he);
        }

        // For each boundary half-edge, find closest opposite-direction boundary half-edge
        int within1e8 = 0, within1e6 = 0, within1e4 = 0, within1e2 = 0, beyond = 0;
        var distBuckets = new List<double>();

        foreach (var he in boundaryEdges)
        {
            var origin = he.Origin.Position;
            var target = he.Target.Position;

            double bestDist = double.MaxValue;
            foreach (var other in boundaryEdges)
            {
                if (other == he) continue;
                double d1 = Vec3.DistanceSquared(other.Origin.Position, target);
                double d2 = Vec3.DistanceSquared(other.Target.Position, origin);
                double dist = System.Math.Max(System.Math.Sqrt(d1), System.Math.Sqrt(d2));
                if (dist < bestDist) bestDist = dist;
            }

            distBuckets.Add(bestDist);
            if (bestDist < 1e-8) within1e8++;
            else if (bestDist < 1e-6) within1e6++;
            else if (bestDist < 1e-4) within1e4++;
            else if (bestDist < 1e-2) within1e2++;
            else beyond++;
        }

        distBuckets.Sort();
        var median = distBuckets.Count > 0 ? distBuckets[distBuckets.Count / 2] : 0;

        Assert.Fail(
            $"Cut mesh A boundary edge match distances ({boundaryEdges.Count} total):\n" +
            $"  < 1e-8 (exact): {within1e8}\n" +
            $"  < 1e-6: {within1e6}\n" +
            $"  < 1e-4: {within1e4}\n" +
            $"  < 1e-2: {within1e2}\n" +
            $"  > 1e-2: {beyond}\n" +
            $"  Median distance: {median:E3}\n" +
            $"  Min: {(distBuckets.Count > 0 ? distBuckets[0] : 0):E3}\n" +
            $"  Max: {(distBuckets.Count > 0 ? distBuckets[^1] : 0):E3}");
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseFaceAreaCoverage()
    {
        // Check if CDT sub-triangles fully cover each original face (no dropped triangles)
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);

        // Cut manually to get per-face results
        var mesh = a.Mesh;
        int faceCount = mesh.Faces.Count;
        var faceSegments = intersections.FaceSegmentsA;

        int facesWithAreaLoss = 0;
        double maxAreaLossPercent = 0;
        int cutFaces = 0;

        for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var originalTri = new Triangle3(va, vb, vc);
            double originalArea = Vec3.Cross(vb - va, vc - va).Length * 0.5;

            if (!faceSegments.TryGetValue(faceIdx, out var segs) || segs.Count == 0)
                continue;

            cutFaces++;
            var subTris = FaceCutter.CutFace(originalTri, faceIdx, segs);
            double totalSubArea = 0;
            foreach (var st in subTris)
            {
                double subArea = Vec3.Cross(st.B - st.A, st.C - st.A).Length * 0.5;
                totalSubArea += subArea;
            }

            double areaLoss = (originalArea - totalSubArea) / originalArea * 100;
            if (areaLoss > 0.01) // More than 0.01% area loss
            {
                facesWithAreaLoss++;
                maxAreaLossPercent = System.Math.Max(maxAreaLossPercent, areaLoss);
            }
        }

        // Also test with edge constraints (matching MeshCutter behavior)
        var edgeSplitMap = new Dictionary<long, List<Vec3>>();
        // Recompute edge splits like MeshCutter does
        double tolSq = options.GridSize * options.GridSize;
        for (int fi = 0; fi < faceCount; fi++)
        {
            if (!faceSegments.TryGetValue(fi, out var s) || s.Count == 0) continue;
            var f = mesh.Faces[fi];
            var h0 = f.Edge; var h1 = h0.Next; var h2 = h1.Next;
            var faceEdges = new (Vec3 P0, Vec3 P1, long Key)[]
            {
                (h0.Origin.Position, h0.Target.Position, PackEdgeKey(h0.Origin.Id, h0.Target.Id)),
                (h1.Origin.Position, h1.Target.Position, PackEdgeKey(h1.Origin.Id, h1.Target.Id)),
                (h2.Origin.Position, h2.Target.Position, PackEdgeKey(h2.Origin.Id, h2.Target.Id))
            };
            foreach (var sg in s)
            {
                RegisterOnEdge(sg.Start, faceEdges, edgeSplitMap, options.GridSize, tolSq);
                RegisterOnEdge(sg.End, faceEdges, edgeSplitMap, options.GridSize, tolSq);
            }
        }

        int cutFaces2 = 0, facesWithLoss2 = 0;
        double maxLoss2 = 0;
        var lossFaces = new List<string>();
        for (int fi = 0; fi < faceCount; fi++)
        {
            var f = mesh.Faces[fi];
            f.GetTrianglePositions(out var va2, out var vb2, out var vc2);
            double origArea2 = Vec3.Cross(vb2 - va2, vc2 - va2).Length * 0.5;

            bool hasSeg = faceSegments.TryGetValue(fi, out var segs2) && segs2.Count > 0;
            var edgeSplits = GetFaceEdgeSplitsLocal(mesh, fi, edgeSplitMap);
            if (!hasSeg && edgeSplits == null) continue;

            cutFaces2++;
            List<FaceCutter.SubTriangle> subs;
            if (hasSeg)
                subs = FaceCutter.CutFace(new Triangle3(va2, vb2, vc2), fi, segs2!, edgeSplits);
            else
                subs = FaceCutter.CutFace(new Triangle3(va2, vb2, vc2), fi,
                    System.Array.Empty<MdCsg.Intersection.IntersectionSegment>(), edgeSplits);

            double totalSub2 = 0;
            foreach (var st2 in subs)
                totalSub2 += Vec3.Cross(st2.B - st2.A, st2.C - st2.A).Length * 0.5;

            double loss2 = (origArea2 - totalSub2) / origArea2 * 100;
            if (loss2 > 0.01)
            {
                facesWithLoss2++;
                maxLoss2 = System.Math.Max(maxLoss2, loss2);
                lossFaces.Add($"Face {fi}: {segs2?.Count ?? 0} segs, {subs.Count} sub-tris, {loss2:F2}% loss, origArea={origArea2:E3}");
            }
        }

        Assert.Fail(
            $"Face area coverage WITHOUT edge constraints:\n" +
            $"  Total cut faces: {cutFaces}\n" +
            $"  Faces with area loss > 0.01%: {facesWithAreaLoss}\n" +
            $"  Max area loss: {maxAreaLossPercent:F4}%\n" +
            $"WITH edge constraints:\n" +
            $"  Total cut faces: {cutFaces2}\n" +
            $"  Faces with area loss > 0.01%: {facesWithLoss2}\n" +
            $"  Max area loss: {maxLoss2:F4}%\n" +
            $"Loss details:\n  " + string.Join("\n  ", lossFaces));
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseCsgResultBoundaryDistances()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var result = Csg.Union(a, b, options);
        var mesh = result.Mesh;

        var boundaryEdges = new List<HalfEdge>();
        foreach (var he in mesh.HalfEdges)
            if (he.Twin == null) boundaryEdges.Add(he);

        int within1e8 = 0, within1e6 = 0, within1e4 = 0, within1e2 = 0, beyond = 0;
        var distBuckets = new List<double>();

        foreach (var he in boundaryEdges)
        {
            var origin = he.Origin.Position;
            var target = he.Target.Position;

            double bestDist = double.MaxValue;
            foreach (var other in boundaryEdges)
            {
                if (other == he) continue;
                double d1 = Vec3.DistanceSquared(other.Origin.Position, target);
                double d2 = Vec3.DistanceSquared(other.Target.Position, origin);
                double dist = System.Math.Max(System.Math.Sqrt(d1), System.Math.Sqrt(d2));
                if (dist < bestDist) bestDist = dist;
            }

            distBuckets.Add(bestDist);
            if (bestDist < 1e-8) within1e8++;
            else if (bestDist < 1e-6) within1e6++;
            else if (bestDist < 1e-4) within1e4++;
            else if (bestDist < 1e-2) within1e2++;
            else beyond++;
        }

        distBuckets.Sort();
        var median = distBuckets.Count > 0 ? distBuckets[distBuckets.Count / 2] : 0;

        // Also count non-manifold
        var edgeCounts = new Dictionary<(int, int), int>();
        int nonManifold = 0;
        foreach (var he in mesh.HalfEdges)
        {
            var key = (he.Origin.Id, he.Target.Id);
            edgeCounts.TryGetValue(key, out int c);
            edgeCounts[key] = c + 1;
            if (c > 0) nonManifold++;
        }

        Assert.Fail(
            $"CSG result (SphereSphere Union) boundary distances ({boundaryEdges.Count} boundary, {nonManifold} non-manifold, {mesh.Faces.Count} faces, {mesh.Vertices.Count} vertices):\n" +
            $"  < 1e-8 (exact): {within1e8}\n" +
            $"  < 1e-6: {within1e6}\n" +
            $"  < 1e-4: {within1e4}\n" +
            $"  < 1e-2: {within1e2}\n" +
            $"  > 1e-2: {beyond}\n" +
            $"  Median: {median:E3}\n" +
            $"  Min: {(distBuckets.Count > 0 ? distBuckets[0] : 0):E3}\n" +
            $"  Max: {(distBuckets.Count > 0 ? distBuckets[^1] : 0):E3}");
    }

    private static long PackEdgeKey(int id0, int id1)
    {
        int lo = id0 < id1 ? id0 : id1;
        int hi = id0 < id1 ? id1 : id0;
        return ((long)lo << 32) | (uint)hi;
    }

    private static void RegisterOnEdge(Vec3 point, (Vec3 P0, Vec3 P1, long Key)[] faceEdges,
        Dictionary<long, List<Vec3>> map, double gridSize, double tolSq)
    {
        foreach (var (p0, p1, key) in faceEdges)
        {
            if (Vec3.DistanceSquared(point, p0) < tolSq) return;
            if (Vec3.DistanceSquared(point, p1) < tolSq) return;
            var edge = p1 - p0;
            double eLenSq = edge.LengthSquared;
            if (eLenSq < tolSq) continue;
            double t = Vec3.Dot(point - p0, edge) / eLenSq;
            if (t <= gridSize || t >= 1.0 - gridSize) continue;
            var proj = p0 + edge * t;
            if (Vec3.DistanceSquared(point, proj) < tolSq)
            {
                if (!map.TryGetValue(key, out var list)) { list = []; map[key] = list; }
                bool found = false;
                foreach (var v in list) if (Vec3.DistanceSquared(v, point) < tolSq) { found = true; break; }
                if (!found) list.Add(point);
                return;
            }
        }
    }

    private static List<Vec3>[]? GetFaceEdgeSplitsLocal(HalfEdgeMesh mesh, int faceIdx,
        Dictionary<long, List<Vec3>> edgeSplitMap)
    {
        var face = mesh.Faces[faceIdx];
        var he0 = face.Edge;
        var he1 = he0.Next;
        var he2 = he1.Next;
        var edgeIds = new (int, int)[]
        {
            (he1.Origin.Id, he1.Target.Id),
            (he2.Origin.Id, he2.Target.Id),
            (he0.Origin.Id, he0.Target.Id),
        };
        List<Vec3>[]? edgeSplits = null;
        for (int e = 0; e < 3; e++)
        {
            long key = PackEdgeKey(edgeIds[e].Item1, edgeIds[e].Item2);
            if (edgeSplitMap.TryGetValue(key, out var splits) && splits.Count > 0)
            {
                edgeSplits ??= new List<Vec3>[3];
                edgeSplits[e] = splits;
            }
        }
        return edgeSplits;
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseBoundaryEdgeOrigins()
    {
        // Determine whether cut mesh boundary edges are at shared original edges
        // or at internal (face-interior) edges
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, false, options.GridSize);

        var mesh = cutA.Mesh;

        // Build set of original edge vertex pairs
        var originalEdges = new HashSet<long>();
        foreach (var he in a.Mesh.HalfEdges)
        {
            int lo = System.Math.Min(he.Origin.Id, he.Target.Id);
            int hi = System.Math.Max(he.Origin.Id, he.Target.Id);
            originalEdges.Add(((long)lo << 32) | (uint)hi);
        }

        // For each boundary edge, check if it lies on an original edge
        // (both endpoints near the same original edge line)
        var boundaryEdges = new List<HalfEdge>();
        foreach (var he in mesh.HalfEdges)
            if (he.Twin == null) boundaryEdges.Add(he);

        int onOrigEdge = 0;
        int onIntersection = 0;
        int internal_ = 0;

        foreach (var he in boundaryEdges)
        {
            var org = he.Origin.Position;
            var tgt = he.Target.Position;

            // Check if this is an intersection edge
            bool isIntEdge = he.IsIntersectionEdge;

            // Check if both endpoints are near an original mesh edge
            bool nearOrigEdge = false;
            foreach (var origHe in a.Mesh.HalfEdges)
            {
                var p0 = origHe.Origin.Position;
                var p1 = origHe.Target.Position;
                var edgeDir = p1 - p0;
                double edgeLenSq = edgeDir.LengthSquared;
                if (edgeLenSq < 1e-16) continue;

                // Check org
                double t1 = Vec3.Dot(org - p0, edgeDir) / edgeLenSq;
                var proj1 = p0 + edgeDir * t1;
                double d1 = Vec3.DistanceSquared(org, proj1);

                double t2 = Vec3.Dot(tgt - p0, edgeDir) / edgeLenSq;
                var proj2 = p0 + edgeDir * t2;
                double d2 = Vec3.DistanceSquared(tgt, proj2);

                if (d1 < 1e-10 && d2 < 1e-10 && t1 >= -0.01 && t1 <= 1.01 && t2 >= -0.01 && t2 <= 1.01)
                {
                    nearOrigEdge = true;
                    break;
                }
            }

            if (isIntEdge) onIntersection++;
            else if (nearOrigEdge) onOrigEdge++;
            else internal_++;
        }

        // Also check: for boundary edges on original edges, are the adjacent faces cut?
        Assert.Fail(
            $"Boundary edge origins ({boundaryEdges.Count} total):\n" +
            $"  On intersection curve: {onIntersection}\n" +
            $"  On original mesh edge: {onOrigEdge}\n" +
            $"  Internal (face interior): {internal_}");
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnosePatchClassification()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, false, options.GridSize);
        var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB, false, options.GridSize);

        var patchesA = MdCsg.Patches.IntraFacePatchExtractor.Extract(cutA.SubTriangles);
        var patchesB = MdCsg.Patches.IntraFacePatchExtractor.Extract(cutB.SubTriangles);

        var classifier = new MdCsg.Classification.CpuPatchClassificationStrategy(false);
        int degA = classifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh, false);
        int degB = classifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh, false);

        int aInside = 0, aOutside = 0, aDegen = 0;
        int aInsideTris = 0, aOutsideTris = 0;
        foreach (var p in patchesA)
        {
            if (!p.HasConfidentPoint) { aDegen++; continue; }
            if (p.IsInside == true) { aInside++; aInsideTris += p.SubTriangleIndices.Count; }
            else { aOutside++; aOutsideTris += p.SubTriangleIndices.Count; }
        }

        int bInside = 0, bOutside = 0, bDegen = 0;
        int bInsideTris = 0, bOutsideTris = 0;
        foreach (var p in patchesB)
        {
            if (!p.HasConfidentPoint) { bDegen++; continue; }
            if (p.IsInside == true) { bInside++; bInsideTris += p.SubTriangleIndices.Count; }
            else { bOutside++; bOutsideTris += p.SubTriangleIndices.Count; }
        }

        // For Union: keep A's outside-B + B's outside-A
        int unionFaces = aOutsideTris + bOutsideTris;

        Assert.Fail(
            $"Patch classification (SphereSphere):\n" +
            $"  A: {patchesA.Count} patches ({aInside} inside/{aOutside} outside/{aDegen} degen), {cutA.SubTriangles.Count} tris\n" +
            $"  A inside: {aInsideTris} tris, outside: {aOutsideTris} tris\n" +
            $"  B: {patchesB.Count} patches ({bInside} inside/{bOutside} outside/{bDegen} degen), {cutB.SubTriangles.Count} tris\n" +
            $"  B inside: {bInsideTris} tris, outside: {bOutsideTris} tris\n" +
            $"  degA={degA}, degB={degB}\n" +
            $"  Union expected ~{unionFaces} tris, CSG result has 627 faces");
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseCutMeshNonManifold()
    {
        // Check for non-manifold edges in cut mesh
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, false, options.GridSize);

        var mesh = cutA.Mesh;

        // Check for non-manifold: half-edges with same (origin,target) pair
        var edgeCounts = new Dictionary<(int, int), int>();
        int totalHalfEdges = 0;
        int withTwin = 0;
        int withoutTwin = 0;
        int duplicateDirected = 0;

        foreach (var he in mesh.HalfEdges)
        {
            totalHalfEdges++;
            if (he.Twin != null) withTwin++;
            else withoutTwin++;

            var key = (he.Origin.Id, he.Target.Id);
            edgeCounts.TryGetValue(key, out int count);
            edgeCounts[key] = count + 1;
            if (count > 0) duplicateDirected++;
        }

        // Also check mesh B
        var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB, false, options.GridSize);
        var meshB = cutB.Mesh;
        int bBoundary = 0, bNonManifold = 0;
        var edgeCountsB = new Dictionary<(int, int), int>();
        foreach (var he in meshB.HalfEdges)
        {
            if (he.Twin == null) bBoundary++;
            var keyB = (he.Origin.Id, he.Target.Id);
            edgeCountsB.TryGetValue(keyB, out int countB);
            edgeCountsB[keyB] = countB + 1;
            if (countB > 0) bNonManifold++;
        }

        Assert.Fail(
            $"Cut mesh A topology:\n" +
            $"  Total half-edges: {totalHalfEdges}\n" +
            $"  With twin: {withTwin}\n" +
            $"  Without twin (boundary): {withoutTwin}\n" +
            $"  Duplicate directed edges (non-manifold): {duplicateDirected}\n" +
            $"  Unique vertices: {mesh.Vertices.Count}\n" +
            $"  Faces: {mesh.Faces.Count}\n" +
            $"Cut mesh B topology:\n" +
            $"  Boundary: {bBoundary}\n" +
            $"  Non-manifold: {bNonManifold}\n" +
            $"  Vertices: {meshB.Vertices.Count}\n" +
            $"  Faces: {meshB.Faces.Count}");
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseCdtBehaviorForFailingFaces()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
        var faceSegments = intersections.FaceSegmentsA;
        var meshObj = a.Mesh;
        int[] diagFaces = [176, 302];
        var lines = new List<string>();

        foreach (var fi in diagFaces)
        {
            if (!faceSegments.TryGetValue(fi, out var segs) || segs.Count == 0)
            {
                lines.Add($"Face {fi}: no segments");
                continue;
            }

            var face = meshObj.Faces[fi];
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var tri = new Triangle3(va, vb, vc);
            double origArea = Vec3.Cross(vb - va, vc - va).Length * 0.5;

            lines.Add($"Face {fi}: {segs.Count} segments, origArea={origArea:E6}");
            lines.Add($"  Triangle: A={va}, B={vb}, C={vc}");
            lines.Add($"  Normal: {tri.Normal}");

            // Build vertices and constraints like FaceCutter does
            var vertices = new List<Vec3> { va, vb, vc };
            var constraintPairs = new List<(int Start, int End)>();

            foreach (var seg in segs)
            {
                int startIdx = AddOrFindVertex(vertices, seg.Start);
                int endIdx = AddOrFindVertex(vertices, seg.End);
                lines.Add($"  Segment: {seg.Start} -> {seg.End}  (idx {startIdx} -> {endIdx})");
                if (startIdx != endIdx)
                    constraintPairs.Add((startIdx, endIdx));
            }

            lines.Add($"  Total vertices: {vertices.Count}");
            for (int i = 0; i < vertices.Count; i++)
                lines.Add($"    v[{i}] = {vertices[i]}");

            lines.Add($"  Constraints: {constraintPairs.Count}");
            for (int i = 0; i < constraintPairs.Count; i++)
                lines.Add($"    c[{i}] = ({constraintPairs[i].Start}, {constraintPairs[i].End})");

            // Call CDT
            var cdtResult = ConstrainedTriangulator.Triangulate(vertices, constraintPairs, tri.Normal);
            lines.Add($"  CDT output: {cdtResult.Count} triangles");

            double cdtArea = 0;
            foreach (var (ai, bi, ci) in cdtResult)
            {
                var pa = vertices[ai]; var pb = vertices[bi]; var pc = vertices[ci];
                double area3d = Vec3.Cross(pb - pa, pc - pa).Length * 0.5;
                cdtArea += area3d;
                lines.Add($"    tri({ai},{bi},{ci}): area3d={area3d:E4}");
            }

            double lossPercent = (origArea - cdtArea) / origArea * 100;
            lines.Add($"  Total CDT area: {cdtArea:E6}, loss: {lossPercent:F2}%");

            // Now check what FaceCutter produces (with its 3D area filter)
            var subTris = FaceCutter.CutFace(tri, fi, segs);
            double fcArea = 0;
            foreach (var st in subTris)
                fcArea += Vec3.Cross(st.B - st.A, st.C - st.A).Length * 0.5;
            double fcLoss = (origArea - fcArea) / origArea * 100;
            lines.Add($"  FaceCutter output: {subTris.Count} sub-tris, area={fcArea:E6}, loss={fcLoss:F2}%");
        }

        // Now check: are vertices coplanar? If not, that explains 3D area loss.
        foreach (var fi in diagFaces)
        {
            if (!faceSegments.TryGetValue(fi, out var segs) || segs.Count == 0) continue;

            var face = meshObj.Faces[fi];
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var tri = new Triangle3(va, vb, vc);
            var n = tri.Normal;
            double nLenSq = Vec3.Dot(n, n);

            var vertices = new List<Vec3> { va, vb, vc };
            foreach (var seg in segs)
            {
                AddOrFindVertex(vertices, seg.Start);
                AddOrFindVertex(vertices, seg.End);
            }

            lines.Add($"\nFace {fi} coplanarity:");
            for (int i = 3; i < vertices.Count; i++)
            {
                double dist = Vec3.Dot(vertices[i] - va, n) / System.Math.Sqrt(nLenSq);
                lines.Add($"  v[{i}] off-plane: {dist:E4}");
            }

            // Project vertices onto face plane and recompute area
            var projected = new List<Vec3>(vertices);
            for (int i = 3; i < projected.Count; i++)
            {
                double dist = Vec3.Dot(projected[i] - va, n);
                projected[i] = projected[i] - n * (dist / nLenSq);
            }

            var cdtProj = ConstrainedTriangulator.Triangulate(projected,
                new List<(int, int)>(), tri.Normal);
            // Just compute total area with projected vertices using the CDT indices from original
            var cdtOrig = ConstrainedTriangulator.Triangulate(vertices,
                new List<(int, int)>(), tri.Normal);
            double origArea = Vec3.Cross(vb - va, vc - va).Length * 0.5;
            double areaOrig3d = 0, areaProj3d = 0;
            foreach (var (ai, bi, ci) in cdtOrig)
            {
                areaOrig3d += Vec3.Cross(vertices[bi] - vertices[ai], vertices[ci] - vertices[ai]).Length * 0.5;
                areaProj3d += Vec3.Cross(projected[bi] - projected[ai], projected[ci] - projected[ai]).Length * 0.5;
            }
            lines.Add($"  No-constraint CDT: {cdtOrig.Count} tris, 3D area={areaOrig3d:E6}, projected={areaProj3d:E6}, orig={origArea:E6}");
        }

        Assert.Fail(string.Join("\n", lines));
    }

    [Fact(Skip = "Diagnostic-only investigation test; not part of pass/fail CI suite.")]
    public void DiagnoseIntersectionCurveEdgeMatching()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 1.0, 2);
        var options = new CsgOptions { Parallel = false };

        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, false);
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, false, options.GridSize);
        var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB, false, options.GridSize);

        var patchesA = MdCsg.Patches.IntraFacePatchExtractor.Extract(cutA.SubTriangles);
        var patchesB = MdCsg.Patches.IntraFacePatchExtractor.Extract(cutB.SubTriangles);

        var classifier = new MdCsg.Classification.CpuPatchClassificationStrategy(false);
        classifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh, false);
        classifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh, false);

        // Collect intersection edges from A-outside sub-triangles
        var aEdges = new List<(Vec3 Start, Vec3 End)>();
        foreach (var p in patchesA)
        {
            if (p.IsInside == true) continue; // keep outside for union
            foreach (int idx in p.SubTriangleIndices)
            {
                var st = cutA.SubTriangles[idx];
                if (!st.HasIntersectionEdge) continue;
                var verts = new[] { st.A, st.B, st.C };
                for (int e = 0; e < 3; e++)
                {
                    if (st.IsEdgeIntersection(e))
                        aEdges.Add((verts[e], verts[(e + 1) % 3]));
                }
            }
        }

        // Collect intersection edges from B-outside sub-triangles
        var bEdges = new List<(Vec3 Start, Vec3 End)>();
        foreach (var p in patchesB)
        {
            if (p.IsInside == true) continue; // keep outside for union
            foreach (int idx in p.SubTriangleIndices)
            {
                var st = cutB.SubTriangles[idx];
                if (!st.HasIntersectionEdge) continue;
                var verts = new[] { st.A, st.B, st.C };
                for (int e = 0; e < 3; e++)
                {
                    if (st.IsEdgeIntersection(e))
                        bEdges.Add((verts[e], verts[(e + 1) % 3]));
                }
            }
        }

        // Check matching: each A edge should have a reverse B edge
        double tolSq = 1e-12;
        int aMatched = 0, aUnmatched = 0;
        var unmatchedDists = new List<double>();
        var unmatchedSamples = new List<string>();

        foreach (var (aS, aE) in aEdges)
        {
            double bestDist = double.MaxValue;
            foreach (var (bS, bE) in bEdges)
            {
                // Check reverse: A(S→E) matches B(E→S)
                double d1 = Vec3.DistanceSquared(aS, bE);
                double d2 = Vec3.DistanceSquared(aE, bS);
                double d = System.Math.Max(d1, d2);
                if (d < bestDist) bestDist = d;
            }

            if (bestDist < tolSq)
                aMatched++;
            else
            {
                aUnmatched++;
                unmatchedDists.Add(System.Math.Sqrt(bestDist));
                if (unmatchedSamples.Count < 5)
                    unmatchedSamples.Add($"  A({aS} → {aE}), nearest B dist={System.Math.Sqrt(bestDist):E4}");
            }
        }

        // Also check: are there intersection segments that match between A and B?
        int segA = 0, segB = 0;
        foreach (var kvp in intersections.FaceSegmentsA) segA += kvp.Value.Count;
        foreach (var kvp in intersections.FaceSegmentsB) segB += kvp.Value.Count;

        // Check if segments are bit-identical between A and B
        var allSegsA = new List<(Vec3 S, Vec3 E)>();
        var allSegsB = new List<(Vec3 S, Vec3 E)>();
        foreach (var kvp in intersections.FaceSegmentsA)
            foreach (var s in kvp.Value) allSegsA.Add((s.Start, s.End));
        foreach (var kvp in intersections.FaceSegmentsB)
            foreach (var s in kvp.Value) allSegsB.Add((s.Start, s.End));

        int segMatches = 0;
        foreach (var (sA, eA) in allSegsA)
        {
            foreach (var (sB, eB) in allSegsB)
            {
                if ((sA == sB && eA == eB) || (sA == eB && eA == sB))
                {
                    segMatches++;
                    break;
                }
            }
        }

        unmatchedDists.Sort();

        Assert.Fail(
            $"Intersection curve edge matching:\n" +
            $"  Intersection segments: A={segA}, B={segB}, bit-identical={segMatches}\n" +
            $"  A-outside intersection edges: {aEdges.Count} ({aMatched} matched, {aUnmatched} unmatched)\n" +
            $"  B-outside intersection edges: {bEdges.Count}\n" +
            $"  Unmatched distance range: {(unmatchedDists.Count > 0 ? $"{unmatchedDists[0]:E4} to {unmatchedDists[^1]:E4}" : "N/A")}\n" +
            $"  Sample unmatched:\n" + string.Join("\n", unmatchedSamples));
    }

    private static int AddOrFindVertex(List<Vec3> vertices, Vec3 point, double tolerance = 1e-8)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vec3.DistanceSquared(vertices[i], point) < tolerance * tolerance)
                return i;
        }
        vertices.Add(point);
        return vertices.Count - 1;
    }
}
