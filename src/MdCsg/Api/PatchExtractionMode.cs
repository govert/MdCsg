namespace MdCsg.Api;

/// <summary>
/// Controls patch extraction strategy after mesh cutting.
/// </summary>
public enum PatchExtractionMode
{
    /// <summary>
    /// Uses the built-in policy:
    /// - intersecting meshes: intra-face extraction,
    /// - non-intersecting meshes: global extraction.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Flood-fills patches independently per original face.
    /// </summary>
    IntraFace = 1,

    /// <summary>
    /// Flood-fills patches globally across sub-triangle adjacency.
    /// </summary>
    Global = 2,

    /// <summary>
    /// Flood-fills globally while using arrangement-segment ownership as the
    /// authoritative patch boundary classifier.
    /// </summary>
    Arrangement = 3
}
