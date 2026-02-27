using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Predicates;
using MdCsg.Robust.Kernel.Predicates;

namespace MdCsg.Robust.Conformance;

public class CertifiedPredicatesTests
{
    [Fact]
    public void Orient2D_NonDegenerate_UsesDoubleTier()
    {
        var result = CertifiedPredicates.Orient2D(
            new Vec2(0, 0),
            new Vec2(1, 0),
            new Vec2(0, 1));

        Assert.Equal(PredicateSign.Positive, result.Sign);
        Assert.Equal(PredicatePrecisionTier.Double, result.Tier);
    }

    [Fact]
    public void Orient2D_Collinear_EscalatesToExact()
    {
        var result = CertifiedPredicates.Orient2D(
            new Vec2(0, 0),
            new Vec2(1, 1),
            new Vec2(2, 2));

        Assert.Equal(PredicateSign.Zero, result.Sign);
        Assert.Equal(PredicatePrecisionTier.Exact, result.Tier);
    }

    [Fact]
    public void Orient3D_CollinearPlane_EscalatesToExact()
    {
        var result = CertifiedPredicates.Orient3D(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0),
            new Vec3(0.5, 0.5, 0));

        Assert.Equal(PredicateSign.Zero, result.Sign);
        Assert.Equal(PredicatePrecisionTier.Exact, result.Tier);
    }

    [Fact]
    public void StrictMode_DegenerateInput_ReportsIssueAndPredicateTelemetry()
    {
        var degenerate = Solid.FromTriangles(
        [
            new Triangle3(
                new Vec3(0, 0, 0),
                new Vec3(1, 1, 1),
                new Vec3(2, 2, 2))
        ]);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);

        var result = RobustCsg.Union(degenerate, cube, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            ValidateInput = true,
            FailOnValidationError = true
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputMeshHasDegenerateFaces);
        Assert.True(result.Diagnostics.PredicateEscalationCount > 0);
        Assert.True(result.Diagnostics.PredicateExactCount > 0);
    }
}
