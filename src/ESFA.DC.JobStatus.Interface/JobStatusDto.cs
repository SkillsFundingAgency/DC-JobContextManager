namespace ESFA.DC.JobStatus.Interface
{
    /// <summary>
    /// The Job Status DTO transported across queues and Web API, must be serialisable.
    /// </summary>
    public class JobStatusDto
    {
        public JobStatusDto()
        {
        }

        public JobStatusDto(long jobId, int jobStatus)
        {
            JobId = jobId;
            JobStatus = jobStatus;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public long JobId { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public int JobStatus { get; set; }
    }
}
