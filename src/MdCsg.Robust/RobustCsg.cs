using MdCsg.Api;

namespace MdCsg.Robust;

public static class RobustCsg
{
    private static IRobustCsgEngine _engine = new LegacyBridgedRobustCsgEngine();

    public static IRobustCsgEngine Engine
    {
        get => _engine;
        set => _engine = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static RobustCsgResult Execute(
        Solid a,
        Solid b,
        RobustCsgOperation operation,
        RobustOperationOptions? options = null)
        => _engine.Execute(a, b, operation, options);

    public static RobustCsgResult Union(
        Solid a,
        Solid b,
        RobustOperationOptions? options = null)
        => Execute(a, b, RobustCsgOperation.Union, options);

    public static RobustCsgResult Intersect(
        Solid a,
        Solid b,
        RobustOperationOptions? options = null)
        => Execute(a, b, RobustCsgOperation.Intersection, options);

    public static RobustCsgResult Difference(
        Solid a,
        Solid b,
        RobustOperationOptions? options = null)
        => Execute(a, b, RobustCsgOperation.Difference, options);
}
