using MdCsg.Math;

namespace MdCsg.Cutting;

/// <summary>
/// Per-face constrained triangulation callback used by face cutting.
/// Implementations must return triangle indices into <paramref name="vertices3D"/>.
/// </summary>
/// <param name="vertices3D">Face-local 3D vertices.</param>
/// <param name="constraints">Constraint edges as vertex index pairs.</param>
/// <param name="faceNormal">Face normal used for orientation/projection.</param>
/// <returns>Triangle index triples referencing <paramref name="vertices3D"/>.</returns>
public delegate IReadOnlyList<(int A, int B, int C)> ConstrainedTriangulationKernel(
    IReadOnlyList<Vec3> vertices3D,
    IReadOnlyList<(int Start, int End)> constraints,
    Vec3 faceNormal);
