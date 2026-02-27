namespace MdCsg.Robust.Kernel.Arrangement;

public readonly record struct ArrangementEdge(
    int Id,
    int StartVertexId,
    int EndVertexId,
    int FaceIndexA,
    int FaceIndexB,
    bool IsDegenerate);
