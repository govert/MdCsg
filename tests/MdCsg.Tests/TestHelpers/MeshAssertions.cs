using MdCsg.Mesh;

namespace MdCsg.Tests.TestHelpers;

/// <summary>
/// Assertion helpers for mesh validation in tests.
/// </summary>
public static class MeshAssertions
{
    /// <summary>
    /// Asserts that the mesh is a valid closed manifold with the given Euler characteristic.
    /// </summary>
    public static void AssertClosedManifold(HalfEdgeMesh mesh, int expectedEulerCharacteristic = 2)
    {
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.HasValidFaceCycles, "Face cycles are invalid.");
        Assert.True(result.IsConsistentlyOriented, "Mesh is not consistently oriented.");
        Assert.Equal(expectedEulerCharacteristic, result.EulerCharacteristic);
    }

    /// <summary>
    /// Asserts that the mesh has at least the expected number of faces.
    /// </summary>
    public static void AssertMinFaceCount(HalfEdgeMesh mesh, int minFaces)
    {
        Assert.True(mesh.Faces.Count >= minFaces,
            $"Expected at least {minFaces} faces, got {mesh.Faces.Count}.");
    }

    /// <summary>
    /// Asserts that the volume of the mesh is approximately the expected value.
    /// </summary>
    public static void AssertVolume(HalfEdgeMesh mesh, double expectedVolume, double tolerance = 0.1)
    {
        double actual = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.True(System.Math.Abs(actual - expectedVolume) < tolerance,
            $"Expected volume ~{expectedVolume}, got {actual}.");
    }
}
