using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.JobContextManager.Interface
{
    public interface IJobContextManager<T>
        where T : class
    {
        void OpenAsync(CancellationToken cancellationToken);

        Task CloseAsync();
    }
}
