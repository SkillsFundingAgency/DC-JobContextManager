using System.Threading.Tasks;

namespace ESFA.DC.JobStatus.Interface
{
    public interface IJobStatus
    {
        Task JobStartedAsync(long jobId);

        Task JobFinishedAsync(long jobId, int numOfLearners = -1);

        Task JobFailedIrrecoverablyAsync(long jobId);

        Task JobFailedRecoverablyAsync(long jobId);

        Task JobAwaitingActionAsync(long jobId, int numOfLearners = -1);
    }
}
