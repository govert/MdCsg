using MdCsg.Api;

namespace MdCsg.Robust;

public interface IRobustCsgEngine
{
    RobustCsgResult Execute(
        Solid a,
        Solid b,
        RobustCsgOperation operation,
        RobustOperationOptions? options = null);
}
