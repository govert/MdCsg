using MdCsg.Math;

namespace MdCsg.Robust.Kernel.Triangulation;

public interface IRobustConstrainedTriangulator
{
    RobustTriangulationResult Triangulate(
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal,
        RobustTriangulationOptions? options = null);
}
