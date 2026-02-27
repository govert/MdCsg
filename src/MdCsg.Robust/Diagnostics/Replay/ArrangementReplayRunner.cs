using MdCsg.Robust.Kernel.Arrangement;

namespace MdCsg.Robust.Diagnostics.Replay;

public static class ArrangementReplayRunner
{
    public static ArrangementGraph BuildArrangement(
        ArrangementReplayCase replayCase,
        bool parallel = false)
    {
        var (meshA, meshB) = ArrangementReplayCodec.DecodeMeshes(replayCase);
        return ArrangementBuilder.Build(meshA, meshB, replayCase.GridSize, parallel);
    }

    public static ArrangementAnalysis AnalyzeArrangement(
        ArrangementReplayCase replayCase,
        bool parallel = false)
    {
        var graph = BuildArrangement(replayCase, parallel);
        return ArrangementAnalyzer.Analyze(graph);
    }
}
