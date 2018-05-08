using System.Threading.Tasks;
using ESFA.DC.JobContext.Interface;

namespace DC.JobContextManager.Interface
{
    public interface IJobContextManager
    {
        Task FinishSuccessfully(IJobContextMessage jobContextMessage);

        Task FinishError(IJobContextMessage jobContextMessage);
    }
}
