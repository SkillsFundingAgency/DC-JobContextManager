namespace ESFA.DC.JobStatus.Interface
{
    public enum JobStatusType
    {
        Ready = 1,
        MovedForProcessing = 2,
        Processing = 3,
        Completed = 4,
        FailedRetry = 5,
        Failed = 6,
        Paused = 7,
        Waiting = 8
    }
}
