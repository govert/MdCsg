using System.IO;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Showcase;

namespace MdCsg.Showcase.ContractTests;

public sealed class ShowcaseCsgContractTests : IDisposable
{
    public ShowcaseCsgContractTests()
    {
        ResetRuntimeOptions();
    }

    public void Dispose()
    {
        ResetRuntimeOptions();
    }

    [Fact]
    public void Step3_StrictNoFailover_ThrowsWithContractMessage()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;

        (Solid step2, Solid cylY) = BuildStep2Input();

        var ex = Assert.Throws<InvalidOperationException>(() => ShowcaseCsg.Difference(step2, cylY));
        Assert.Contains("strict mode", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("closureAttempt=1", ex.Message, StringComparison.Ordinal);
        Assert.Contains("--allow-legacy-failover", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Step3_StrictRobustOnly_DisablesClosureAttempt_InContractMessage()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;
        ShowcaseRuntimeOptions.UseClosureAttemptRobust = false;

        (Solid step2, Solid cylY) = BuildStep2Input();

        var ex = Assert.Throws<InvalidOperationException>(() => ShowcaseCsg.Difference(step2, cylY));
        Assert.Contains("closureAttempt=0", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Step3_ExplicitFailover_ReturnsLegacyResult_AndLogsFailover()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = true;

        using var output = new StringWriter();
        TextWriter previous = Console.Out;
        Console.SetOut(output);
        try
        {
            (Solid step2, Solid cylY) = BuildStep2Input();
            Solid step3 = ShowcaseCsg.Difference(step2, cylY);

            Assert.True(step3.Mesh.Faces.Count > 0);
            Assert.Contains("using explicit legacy failover", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(previous);
        }
    }

    [Fact]
    public void Step3_LegacyMode_BypassesRobustFailurePath()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = true;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;

        (Solid step2, Solid cylY) = BuildStep2Input();
        Solid step3 = ShowcaseCsg.Difference(step2, cylY);
        Assert.True(step3.Mesh.Faces.Count > 0);
    }

    private static (Solid Step2, Solid CylY) BuildStep2Input()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = ShowcaseCsg.Intersect(sphere, box);
        var step2 = ShowcaseCsg.Difference(step1, cylX);
        return (step2, cylY);
    }

    private static void ResetRuntimeOptions()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;
        ShowcaseRuntimeOptions.UseClosureAttemptRobust = true;
    }
}
