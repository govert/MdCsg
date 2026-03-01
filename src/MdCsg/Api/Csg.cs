using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Api;

/// <summary>
/// Static entry point for CSG (Constructive Solid Geometry) boolean operations.
/// Implements the Patch-Confident CSG algorithm.
/// </summary>
public static class Csg
{
    // ── Mesh × Mesh ──────────────────────────────────────────────

    /// <summary>
    /// Computes the union A ∪ B.
    /// </summary>
    public static CsgResult Union(Solid a, Solid b, CsgOptions? options = null) =>
        EvaluateWithComplements(a, b, CsgOperation.Union, options ?? new CsgOptions());

    /// <summary>
    /// Computes the intersection A ∩ B.
    /// </summary>
    public static CsgResult Intersect(Solid a, Solid b, CsgOptions? options = null) =>
        EvaluateWithComplements(a, b, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>
    /// Computes the difference A \ B.
    /// </summary>
    public static CsgResult Difference(Solid a, Solid b, CsgOptions? options = null) =>
        EvaluateWithComplements(a, b, CsgOperation.Difference, options ?? new CsgOptions());

    // ── Mesh × HalfSpace ────────────────────────────────────────

    /// <summary>Union of a mesh and a half-space produces an infinite result.</summary>
    public static CsgResult Union(Solid a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union of a mesh and a half-space produces an infinite result.");

    /// <summary>Clips mesh A to the interior of half-space B.</summary>
    public static CsgResult Intersect(Solid a, HalfSpace b, CsgOptions? options = null) =>
        CsgHalfSpace.Evaluate(a, b, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracts a half-space from mesh A, keeping the part outside the half-space.</summary>
    public static CsgResult Difference(Solid a, HalfSpace b, CsgOptions? options = null) =>
        CsgHalfSpace.Evaluate(a, b, CsgOperation.Difference, options ?? new CsgOptions());

    // ── HalfSpace × Mesh ────────────────────────────────────────

    /// <summary>Union of a half-space and a mesh produces an infinite result.</summary>
    public static CsgResult Union(HalfSpace a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union of a half-space and a mesh produces an infinite result.");

    /// <summary>Clips mesh B to the interior of half-space A (commutative with Intersect(Solid, HalfSpace)).</summary>
    public static CsgResult Intersect(HalfSpace a, Solid b, CsgOptions? options = null) =>
        CsgHalfSpace.Evaluate(b, a, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracting a mesh from a half-space produces an infinite result.</summary>
    public static CsgResult Difference(HalfSpace a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Subtracting a mesh from a half-space produces an infinite result.");

    // ── Mesh × ImplicitSolid ─────────────────────────────────────

    /// <summary>Union of a mesh and an implicit solid requires tessellating the implicit surface.</summary>
    public static CsgResult Union(Solid a, ImplicitSolid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union with an implicit solid requires tessellating its surface. Use a meshed primitive instead.");

    /// <summary>Clips mesh A to the interior of implicit solid B.</summary>
    public static CsgResult Intersect(Solid a, ImplicitSolid b, CsgOptions? options = null) =>
        CsgImplicit.Evaluate(a, b, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracts an implicit solid from mesh A, keeping the part outside the implicit surface.</summary>
    public static CsgResult Difference(Solid a, ImplicitSolid b, CsgOptions? options = null) =>
        CsgImplicit.Evaluate(a, b, CsgOperation.Difference, options ?? new CsgOptions());

    // ── ImplicitSolid × Mesh ─────────────────────────────────────

    /// <summary>Union of an implicit solid and a mesh requires tessellating the implicit surface.</summary>
    public static CsgResult Union(ImplicitSolid a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union with an implicit solid requires tessellating its surface. Use a meshed primitive instead.");

    /// <summary>Clips mesh B to the interior of implicit solid A (commutative with Intersect(Solid, ImplicitSolid)).</summary>
    public static CsgResult Intersect(ImplicitSolid a, Solid b, CsgOptions? options = null) =>
        CsgImplicit.Evaluate(b, a, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracting a mesh from an implicit solid requires tessellating the implicit surface.</summary>
    public static CsgResult Difference(ImplicitSolid a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Subtracting a mesh from an implicit solid requires tessellating its surface. Use a meshed primitive instead.");

    // ── HalfSpace × HalfSpace ───────────────────────────────────

    /// <summary>HalfSpace-HalfSpace operations are not supported.</summary>
    public static CsgResult Union(HalfSpace a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("CSG operations between two half-spaces are not supported.");

    /// <summary>HalfSpace-HalfSpace operations are not supported.</summary>
    public static CsgResult Intersect(HalfSpace a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("CSG operations between two half-spaces are not supported.");

    /// <summary>HalfSpace-HalfSpace operations are not supported.</summary>
    public static CsgResult Difference(HalfSpace a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("CSG operations between two half-spaces are not supported.");

    /// <summary>
    /// Evaluates a CSG boolean operation between two solids.
    /// </summary>
    private static CsgResult Evaluate(Solid a, Solid b, CsgOperation operation, CsgOptions options)
    {
        // Step 1: Find all intersections between the two meshes
        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, options.Parallel);

        // Step 2: Cut both meshes along intersection curves
        var cutA = MeshCutter.Cut(
            a.Mesh,
            intersections.FaceSegmentsA,
            options.Parallel,
            options.GridSize,
            useEdgeSplitConstraints: true,
            triangulationKernel: options.TriangulationKernel);
        var cutB = MeshCutter.Cut(
            b.Mesh,
            intersections.FaceSegmentsB,
            options.Parallel,
            options.GridSize,
            useEdgeSplitConstraints: true,
            triangulationKernel: options.TriangulationKernel);

        // Step 3: Build adjacency
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        // Step 4: Build extraction candidate(s), classify, and assemble.
        var classifier = options.ClassificationStrategy ?? new CpuPatchClassificationStrategy(options.Parallel);
        var extractionMode = ResolveExtractionMode(options.PatchExtractionMode, intersections.Segments.Count > 0);

        AssemblyCandidate chosen;
        IReadOnlyList<string> candidateSignatures;
        if (options.PatchExtractionMode == PatchExtractionMode.Auto
            && options.PreferTopologyPreservingPatchExtraction
            && intersections.Segments.Count > 0)
        {
            var intra = BuildAssemblyCandidate(
                PatchExtractionMode.IntraFace,
                classifier,
                cutA,
                cutB,
                adjA,
                adjB,
                intersections,
                a,
                b,
                operation,
                options);
            var global = BuildAssemblyCandidate(
                PatchExtractionMode.Global,
                classifier,
                cutA,
                cutB,
                adjA,
                adjB,
                intersections,
                a,
                b,
                operation,
                options);
            var arrangement = BuildAssemblyCandidate(
                PatchExtractionMode.Arrangement,
                classifier,
                cutA,
                cutB,
                adjA,
                adjB,
                intersections,
                a,
                b,
                operation,
                options);

            var candidates = new[] { intra, global, arrangement };
            if (options.ForceArrangementPatchExtraction)
            {
                chosen = arrangement;
            }
            else
            {
                chosen = intra;
                if (IsBetterTopologyQuality(global.TopologyQuality, chosen.TopologyQuality))
                    chosen = global;
                if (IsBetterTopologyQuality(arrangement.TopologyQuality, chosen.TopologyQuality))
                    chosen = arrangement;
            }

            candidateSignatures = BuildCandidateSignatures(candidates);
        }
        else
        {
            chosen = BuildAssemblyCandidate(
                extractionMode,
                classifier,
                cutA,
                cutB,
                adjA,
                adjB,
                intersections,
                a,
                b,
                operation,
                options);

            candidateSignatures = BuildCandidateSignatures([chosen]);
        }

        // Step 6: Stitch into output mesh
        var resultMesh = MeshStitcher.Stitch(chosen.Assembly.Triangles, options.WeldTolerance);
        if (intersections.Segments.Count > 0)
        {
            if (MeshValidator.CountBoundaryEdges(resultMesh) > 0)
            {
                double repairTolerance = System.Math.Max(options.WeldTolerance * 2.0, options.GridSize * 8.0);
                MeshStitcher.RepairBoundary(resultMesh, repairTolerance);
            }

            if (MeshValidator.CountBoundaryEdges(resultMesh) > 0)
                MeshStitcher.CloseBoundaryLoops(resultMesh);

            resultMesh = PruneFragmentComponents(resultMesh, options.WeldTolerance);
        }

        return new CsgResult
        {
            Mesh = resultMesh,
            PatchCountA = chosen.PatchCountA,
            PatchCountB = chosen.PatchCountB,
            DegenerateCount = chosen.DegenerateCountA + chosen.DegenerateCountB,
            IntersectionSegmentCount = intersections.Segments.Count,
            SelectedPatchExtractionMode = chosen.ExtractionMode,
            SelectedPatchExtractionBoundaryEdgeCount = chosen.TopologyQuality.BoundaryEdgeCount,
            SelectedPatchExtractionIsEdgeManifold = chosen.TopologyQuality.IsEdgeManifold,
            SelectedPatchExtractionConnectedComponentCount = chosen.TopologyQuality.ConnectedComponentCount,
            SelectedPatchBoundaryAuthority = GetBoundaryAuthorityForMode(chosen.ExtractionMode),
            SelectedAssemblyTrianglesFromA = chosen.Assembly.TrianglesFromMeshA,
            SelectedAssemblyTrianglesFromB = chosen.Assembly.TrianglesFromMeshB,
            SelectedAssemblyFlippedTrianglesFromB = chosen.Assembly.FlippedTrianglesFromMeshB,
            PatchExtractionCandidateSignatures = candidateSignatures,
            SelectedCertifiedPatchCount = chosen.ClassificationEvidence.CertifiedCount,
            SelectedUncertifiedPatchCount = chosen.ClassificationEvidence.UncertifiedCount,
            SelectedClassificationEvidenceFingerprint = chosen.ClassificationEvidence.Fingerprint,
            AuthoritativeBoundary = new ReconstructionBoundaryContract(
                ExtractionMode: chosen.ExtractionMode,
                Authority: GetBoundaryAuthorityForMode(chosen.ExtractionMode),
                BoundaryEdgeCount: chosen.TopologyQuality.BoundaryEdgeCount,
                IsEdgeManifold: chosen.TopologyQuality.IsEdgeManifold,
                ConnectedComponentCount: chosen.TopologyQuality.ConnectedComponentCount)
        };
    }

    private static CsgResult EvaluateWithComplements(Solid a, Solid b, CsgOperation operation, CsgOptions options)
    {
        bool compA = a.IsComplemented;
        bool compB = b.IsComplemented;
        if (!compA && !compB)
            return Evaluate(a, b, operation, options);

        var baseA = compA ? a.Complement() : a;
        var baseB = compB ? b.Complement() : b;

        CsgResult result;
        bool complementResult;

        switch (operation)
        {
            case CsgOperation.Union:
                if (compA && compB)
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Intersection, options);
                    complementResult = true; // ~A ∪ ~B = ~(A ∩ B)
                }
                else if (compA)
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Difference, options);
                    complementResult = true; // ~A ∪ B = ~(A \ B)
                }
                else
                {
                    result = Evaluate(baseB, baseA, CsgOperation.Difference, options);
                    complementResult = true; // A ∪ ~B = ~(B \ A)
                }
                break;

            case CsgOperation.Intersection:
                if (compA && compB)
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Union, options);
                    complementResult = true; // ~A ∩ ~B = ~(A ∪ B)
                }
                else if (compA)
                {
                    result = Evaluate(baseB, baseA, CsgOperation.Difference, options);
                    complementResult = false; // ~A ∩ B = B \ A
                }
                else
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Difference, options);
                    complementResult = false; // A ∩ ~B = A \ B
                }
                break;

            case CsgOperation.Difference:
                if (compA && compB)
                {
                    result = Evaluate(baseB, baseA, CsgOperation.Difference, options);
                    complementResult = false; // ~A \ ~B = B \ A
                }
                else if (compA)
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Union, options);
                    complementResult = true; // ~A \ B = ~(A ∪ B)
                }
                else if (compB)
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Intersection, options);
                    complementResult = false; // A \ ~B = A ∩ B
                }
                else
                {
                    result = Evaluate(baseA, baseB, CsgOperation.Difference, options);
                    complementResult = false;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }

        result.Mesh.IsComplemented = complementResult;
        return result;
    }

    // ── Complement ──────────────────────────────────────────────────

    /// <summary>
    /// Returns the set-theoretic complement of a solid.
    /// </summary>
    /// <param name="solid">The solid to complement.</param>
    /// <returns>A new solid marked as complemented with reversed winding.</returns>
    public static Solid Complement(Solid solid)
    {
        var positions = new List<Vec3>(solid.Mesh.Vertices.Count);
        foreach (var v in solid.Mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int, int, int)>(solid.Mesh.Faces.Count);
        foreach (var face in solid.Mesh.Faces)
        {
            var verts = face.GetVertices();
            triangles.Add((verts[0].Id, verts[2].Id, verts[1].Id));
        }

        var builder = new MeshBuilder(1e-12);
        var mesh = builder.Build(positions, triangles);
        mesh.IsComplemented = !solid.IsComplemented;
        return new Solid(mesh);
    }

    // ── Bulk Operations ─────────────────────────────────────────────

    /// <summary>
    /// Computes the union of multiple solids via left-fold.
    /// </summary>
    /// <param name="solids">The solids to unite.</param>
    /// <param name="options">Optional CSG options.</param>
    /// <returns>The result of uniting all solids.</returns>
    public static CsgResult Union(IEnumerable<Solid> solids, CsgOptions? options = null)
    {
        return FoldSolids(solids, CsgOperation.Union, options);
    }

    /// <summary>
    /// Computes the intersection of multiple solids via left-fold.
    /// </summary>
    /// <param name="solids">The solids to intersect.</param>
    /// <param name="options">Optional CSG options.</param>
    /// <returns>The result of intersecting all solids.</returns>
    public static CsgResult Intersect(IEnumerable<Solid> solids, CsgOptions? options = null)
    {
        return FoldSolids(solids, CsgOperation.Intersection, options);
    }

    private static CsgResult FoldSolids(IEnumerable<Solid> solids, CsgOperation op, CsgOptions? options)
    {
        Solid? accumulator = null;
        CsgResult? lastResult = null;

        foreach (var solid in solids)
        {
            if (accumulator == null)
            {
                accumulator = solid;
                continue;
            }

            lastResult = op switch
            {
                CsgOperation.Union => Union(accumulator, solid, options),
                CsgOperation.Intersection => Intersect(accumulator, solid, options),
                CsgOperation.Difference => Difference(accumulator, solid, options),
                _ => throw new ArgumentOutOfRangeException()
            };
            accumulator = new Solid(lastResult.Mesh);
        }

        if (lastResult != null)
            return lastResult;

        if (accumulator != null)
        {
            return new CsgResult
            {
                Mesh = accumulator.Mesh,
                PatchCountA = 0,
                PatchCountB = 0,
                DegenerateCount = 0,
                IntersectionSegmentCount = 0
            };
        }

        throw new ArgumentException("At least one solid is required.", nameof(solids));
    }

    /// <summary>
    /// Marks patches whose sub-triangles come from coplanar face regions.
    /// A patch is coplanar if any of its original faces are in the coplanar map.
    /// </summary>
    private static void MarkCoplanarPatches(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        IReadOnlyDictionary<int, bool> coplanarFaces)
    {
        if (coplanarFaces.Count == 0) return;

        foreach (var patch in patches)
        {
            // Check if any sub-triangle in this patch has a coplanar original face
            // and the patch has low margin (indicating it's in the overlap region)
            foreach (int triIdx in patch.SubTriangleIndices)
            {
                int origFace = subTriangles[triIdx].OriginalFaceIndex;
                if (coplanarFaces.TryGetValue(origFace, out bool normalsAgree))
                {
                    patch.CoplanarNormalsAgree = normalsAgree;
                    break;
                }
            }
        }
    }

    private static PatchExtractionMode ResolveExtractionMode(PatchExtractionMode configured, bool hasIntersections)
    {
        if (configured != PatchExtractionMode.Auto)
            return configured;

        return hasIntersections
            ? PatchExtractionMode.IntraFace
            : PatchExtractionMode.Global;
    }

    private readonly record struct AssemblyCandidate(
        PatchExtractionMode ExtractionMode,
        PatchAssembler.AssemblyResult Assembly,
        int PatchCountA,
        int PatchCountB,
        int DegenerateCountA,
        int DegenerateCountB,
        AssemblyTopologyQuality TopologyQuality,
        ClassificationEvidence ClassificationEvidence);

    private readonly record struct AssemblyTopologyQuality(
        int BoundaryEdgeCount,
        bool IsEdgeManifold,
        int ConnectedComponentCount)
    {
        public bool IsClosedManifold => BoundaryEdgeCount == 0 && IsEdgeManifold;
    }

    private readonly record struct ClassificationEvidence(
        int CertifiedCount,
        int UncertifiedCount,
        string Fingerprint);

    private static AssemblyCandidate BuildAssemblyCandidate(
        PatchExtractionMode extractionMode,
        IPatchClassificationStrategy classifier,
        MeshCutter.CutResult cutA,
        MeshCutter.CutResult cutB,
        SubTriangleAdjacency adjA,
        SubTriangleAdjacency adjB,
        IntersectionGraph intersections,
        Solid a,
        Solid b,
        CsgOperation operation,
        CsgOptions options)
    {
        List<Patch> patchesA;
        List<Patch> patchesB;
        switch (extractionMode)
        {
            case PatchExtractionMode.IntraFace:
                patchesA = IntraFacePatchExtractor.Extract(cutA.SubTriangles);
                patchesB = IntraFacePatchExtractor.Extract(cutB.SubTriangles);
                break;
            case PatchExtractionMode.Global:
                patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
                patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
                break;
            case PatchExtractionMode.Arrangement:
                patchesA = ArrangementPatchExtractor.Extract(
                    cutA.SubTriangles,
                    intersections.FaceSegmentsA,
                    options.GridSize);
                patchesB = ArrangementPatchExtractor.Extract(
                    cutB.SubTriangles,
                    intersections.FaceSegmentsB,
                    options.GridSize);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(extractionMode), extractionMode, null);
        }

        MarkCoplanarPatches(patchesA, cutA.SubTriangles, intersections.CoplanarFacesA);
        MarkCoplanarPatches(patchesB, cutB.SubTriangles, intersections.CoplanarFacesB);

        int degA = classifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh, options.UseWindingNumber);
        int degB = classifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh, options.UseWindingNumber);

        var assembly = PatchAssembler.Assemble(
            patchesA,
            patchesB,
            cutA.SubTriangles,
            cutB.SubTriangles,
            operation);

        var quality = EvaluateAssemblyTopologyQuality(assembly, options.WeldTolerance);
        var evidence = BuildClassificationEvidence(patchesA, patchesB);
        return new AssemblyCandidate(
            extractionMode,
            assembly,
            patchesA.Count,
            patchesB.Count,
            degA,
            degB,
            quality,
            evidence);
    }

    private static AssemblyTopologyQuality EvaluateAssemblyTopologyQuality(
        PatchAssembler.AssemblyResult assembly,
        double weldTolerance)
    {
        if (assembly.Triangles.Count == 0)
            return new AssemblyTopologyQuality(
                BoundaryEdgeCount: 0,
                IsEdgeManifold: true,
                ConnectedComponentCount: 0);

        var mesh = MeshStitcher.Stitch(assembly.Triangles, weldTolerance);
        return new AssemblyTopologyQuality(
            BoundaryEdgeCount: MeshValidator.CountBoundaryEdges(mesh),
            IsEdgeManifold: MeshValidator.IsEdgeManifold(mesh),
            ConnectedComponentCount: MeshQueries.ConnectedComponents(mesh).Count);
    }

    private static bool IsBetterTopologyQuality(AssemblyTopologyQuality a, AssemblyTopologyQuality b)
    {
        if (a.IsClosedManifold != b.IsClosedManifold)
            return a.IsClosedManifold;

        if (a.BoundaryEdgeCount != b.BoundaryEdgeCount)
            return a.BoundaryEdgeCount < b.BoundaryEdgeCount;

        if (a.IsEdgeManifold != b.IsEdgeManifold)
            return a.IsEdgeManifold;

        if (a.ConnectedComponentCount != b.ConnectedComponentCount)
            return a.ConnectedComponentCount < b.ConnectedComponentCount;

        return false;
    }

    private static ClassificationEvidence BuildClassificationEvidence(
        IReadOnlyList<Patch> patchesA,
        IReadOnlyList<Patch> patchesB)
    {
        int certified = 0;
        int uncertified = 0;
        var tokens = new List<string>(patchesA.Count + patchesB.Count);

        AddTokens(patchesA, sourceMesh: "A");
        AddTokens(patchesB, sourceMesh: "B");
        tokens.Sort(StringComparer.Ordinal);

        return new ClassificationEvidence(
            CertifiedCount: certified,
            UncertifiedCount: uncertified,
            Fingerprint: string.Join("|", tokens));

        void AddTokens(IReadOnlyList<Patch> patches, string sourceMesh)
        {
            for (int i = 0; i < patches.Count; i++)
            {
                var patch = patches[i];
                bool isCertified = patch.IsClassificationCertified;
                if (isCertified)
                    certified++;
                else
                    uncertified++;

                int inside = patch.IsInside switch
                {
                    true => 1,
                    false => 0,
                    _ => -1
                };
                int coplanar = patch.CoplanarNormalsAgree switch
                {
                    true => 1,
                    false => 0,
                    _ => -1
                };

                tokens.Add(
                    $"{sourceMesh}:{patch.StableId}:{(isCertified ? 1 : 0)}:{inside}:{coplanar}:{patch.SubTriangleIndices.Count}:{(int)patch.BoundaryAuthority}");
            }
        }
    }

    private static IReadOnlyList<string> BuildCandidateSignatures(
        IReadOnlyList<AssemblyCandidate> candidates)
    {
        var ordered = new List<AssemblyCandidate>(candidates.Count);
        for (int i = 0; i < candidates.Count; i++)
            ordered.Add(candidates[i]);
        ordered.Sort(static (x, y) => x.ExtractionMode.CompareTo(y.ExtractionMode));

        var signatures = new string[ordered.Count];
        for (int i = 0; i < ordered.Count; i++)
            signatures[i] = BuildCandidateSignature(ordered[i]);
        return signatures;
    }

    private static string BuildCandidateSignature(AssemblyCandidate candidate)
    {
        int degenerateTotal = candidate.DegenerateCountA + candidate.DegenerateCountB;
        return $"{candidate.ExtractionMode}:"
            + $"authority={GetBoundaryAuthorityForMode(candidate.ExtractionMode)};"
            + $"boundary={candidate.TopologyQuality.BoundaryEdgeCount};"
            + $"manifold={(candidate.TopologyQuality.IsEdgeManifold ? 1 : 0)};"
            + $"components={candidate.TopologyQuality.ConnectedComponentCount};"
            + $"patchA={candidate.PatchCountA};"
            + $"patchB={candidate.PatchCountB};"
            + $"deg={degenerateTotal};"
            + $"tri={candidate.Assembly.Triangles.Count}";
    }

    private static PatchBoundaryAuthority GetBoundaryAuthorityForMode(PatchExtractionMode mode)
        => mode == PatchExtractionMode.Arrangement
            ? PatchBoundaryAuthority.Arrangement
            : PatchBoundaryAuthority.IntersectionFlags;

    private static HalfEdgeMesh PruneFragmentComponents(HalfEdgeMesh mesh, double weldTolerance)
    {
        var components = MeshQueries.ConnectedComponents(mesh);
        if (components.Count <= 1)
            return mesh;

        var keep = new bool[components.Count];
        var faceCounts = new int[components.Count];
        var absVolumes = new double[components.Count];
        for (int ci = 0; ci < components.Count; ci++)
        {
            keep[ci] = true;
            faceCounts[ci] = components[ci].Count;
            absVolumes[ci] = System.Math.Abs(ComputeSignedVolume(mesh, components[ci]));

            // A closed triangular surface component cannot have fewer than 4 faces.
            if (components[ci].Count < 4)
            {
                keep[ci] = false;
                continue;
            }

            var stats = GetComponentEdgeStats(mesh, components[ci]);
            if (components[ci].Count <= 8 && (stats.BoundaryEdgeCount > 0 || stats.NonManifoldEdgeCount > 0))
            {
                keep[ci] = false;
            }
        }

        int maxFaces = 0;
        double maxAbsVolume = 0.0;
        for (int ci = 0; ci < components.Count; ci++)
        {
            if (!keep[ci]) continue;
            if (faceCounts[ci] > maxFaces) maxFaces = faceCounts[ci];
            if (absVolumes[ci] > maxAbsVolume) maxAbsVolume = absVolumes[ci];
        }

        if (maxFaces > 0 && maxAbsVolume > 0.0)
        {
            const double relFaceThreshold = 0.05;
            const double relVolumeThreshold = 0.01;
            double minFaces = maxFaces * relFaceThreshold;
            double minVolume = maxAbsVolume * relVolumeThreshold;

            for (int ci = 0; ci < components.Count; ci++)
            {
                if (!keep[ci]) continue;
                if (faceCounts[ci] < minFaces && absVolumes[ci] < minVolume)
                    keep[ci] = false;
            }
        }

        int totalKeptFaces = 0;
        for (int ci = 0; ci < components.Count; ci++)
        {
            if (keep[ci]) totalKeptFaces += faceCounts[ci];
        }

        if (totalKeptFaces > 0 && maxFaces >= totalKeptFaces * 0.70)
        {
            double minFaces = maxFaces * 0.20;
            double minVolume = maxAbsVolume * 0.20;
            for (int ci = 0; ci < components.Count; ci++)
            {
                if (!keep[ci]) continue;
                if (faceCounts[ci] < minFaces && absVolumes[ci] < minVolume)
                    keep[ci] = false;
            }
        }

        int largestIdx = -1;
        int largestFaces = 0;
        int secondLargestFaces = 0;
        for (int ci = 0; ci < components.Count; ci++)
        {
            if (!keep[ci]) continue;
            int f = faceCounts[ci];
            if (f > largestFaces)
            {
                secondLargestFaces = largestFaces;
                largestFaces = f;
                largestIdx = ci;
            }
            else if (f > secondLargestFaces)
            {
                secondLargestFaces = f;
            }
        }

        if (largestIdx >= 0 && secondLargestFaces > 0 && largestFaces >= secondLargestFaces * 10)
        {
            for (int ci = 0; ci < components.Count; ci++)
                keep[ci] = ci == largestIdx;
        }

        int keepCount = 0;
        for (int i = 0; i < keep.Length; i++)
            if (keep[i]) keepCount++;

        if (keepCount == 0)
            return mesh;

        if (keepCount == components.Count)
            return mesh;

        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int I0, int I1, int I2)>();
        for (int ci = 0; ci < components.Count; ci++)
        {
            if (!keep[ci]) continue;
            foreach (int faceIdx in components[ci])
            {
                var verts = mesh.Faces[faceIdx].GetVertices();
                triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
            }
        }

        var rebuilt = new MeshBuilder(0.0).Build(positions, triangles);
        rebuilt.IsComplemented = mesh.IsComplemented;
        return rebuilt;
    }

    private readonly struct ComponentEdgeStats
    {
        public int BoundaryEdgeCount { get; }
        public int NonManifoldEdgeCount { get; }

        public ComponentEdgeStats(int boundaryEdgeCount, int nonManifoldEdgeCount)
        {
            BoundaryEdgeCount = boundaryEdgeCount;
            NonManifoldEdgeCount = nonManifoldEdgeCount;
        }
    }

    private static ComponentEdgeStats GetComponentEdgeStats(HalfEdgeMesh mesh, IReadOnlyList<int> componentFaces)
    {
        var edgeUse = new Dictionary<long, int>(componentFaces.Count * 3);
        foreach (int faceIdx in componentFaces)
        {
            var verts = mesh.Faces[faceIdx].GetVertices();
            CountUndirectedEdge(edgeUse, verts[0].Id, verts[1].Id);
            CountUndirectedEdge(edgeUse, verts[1].Id, verts[2].Id);
            CountUndirectedEdge(edgeUse, verts[2].Id, verts[0].Id);
        }

        int boundary = 0;
        int nonManifold = 0;
        foreach (int count in edgeUse.Values)
        {
            if (count == 1) boundary++;
            else if (count > 2) nonManifold++;
        }

        return new ComponentEdgeStats(boundary, nonManifold);
    }

    private static double ComputeSignedVolume(HalfEdgeMesh mesh, IReadOnlyList<int> componentFaces)
    {
        double vol6 = 0.0;
        foreach (int faceIdx in componentFaces)
        {
            var face = mesh.Faces[faceIdx];
            face.GetTrianglePositions(out var a, out var b, out var c);
            vol6 += Vec3.Dot(a, Vec3.Cross(b, c));
        }
        return vol6 / 6.0;
    }

    private static void CountUndirectedEdge(Dictionary<long, int> edgeUse, int i0, int i1)
    {
        int lo = i0 < i1 ? i0 : i1;
        int hi = i0 < i1 ? i1 : i0;
        long key = ((long)lo << 32) | (uint)hi;
        edgeUse.TryGetValue(key, out int count);
        edgeUse[key] = count + 1;
    }

    private static HalfEdgeMesh KeepLargestComponent(HalfEdgeMesh mesh)
    {
        var components = MeshQueries.ConnectedComponents(mesh);
        if (components.Count <= 1)
            return mesh;

        int largestIdx = 0;
        int largestCount = components[0].Count;
        for (int i = 1; i < components.Count; i++)
        {
            int count = components[i].Count;
            if (count > largestCount)
            {
                largestCount = count;
                largestIdx = i;
            }
        }

        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int I0, int I1, int I2)>(components[largestIdx].Count);
        foreach (int faceIdx in components[largestIdx])
        {
            var verts = mesh.Faces[faceIdx].GetVertices();
            triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        var rebuilt = new MeshBuilder(0.0).Build(positions, triangles);
        rebuilt.IsComplemented = mesh.IsComplemented;
        return rebuilt;
    }
}
