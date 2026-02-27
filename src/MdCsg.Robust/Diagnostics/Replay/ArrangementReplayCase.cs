namespace MdCsg.Robust.Diagnostics.Replay;

public sealed class ArrangementReplayCase
{
    public ArrangementReplayCase(double gridSize, byte[] meshAData, byte[] meshBData)
    {
        GridSize = gridSize;
        MeshAData = meshAData is null
            ? throw new ArgumentNullException(nameof(meshAData))
            : (byte[])meshAData.Clone();
        MeshBData = meshBData is null
            ? throw new ArgumentNullException(nameof(meshBData))
            : (byte[])meshBData.Clone();
    }

    public double GridSize { get; }

    public byte[] MeshAData { get; }

    public byte[] MeshBData { get; }
}
