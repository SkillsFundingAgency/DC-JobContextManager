using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.JobContextManager.Interface
{
    public interface IMessageHandler<in T>
    {
        Task<bool> HandleAsync(T message, CancellationToken cancellationToken);
    }
}
