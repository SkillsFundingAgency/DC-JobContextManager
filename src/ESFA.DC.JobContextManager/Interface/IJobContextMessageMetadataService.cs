using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobContextManager.Interface
{
    public interface IJobContextMessageMetadataService
    {
        JobStatusDto BuildJobStatusDto(IJobContextMessage jobContextMessage, JobStatusType jobStatusType);

        AuditingDto BuildAuditingDto(IJobContextMessage jobContextMessage, AuditEventType auditEventType, string extraInfo = null);

        bool PointerIsFirstTopic(IJobContextMessage jobContextMessage);

        bool PointerIsLastTopic(IJobContextMessage jobContextMessage);

        bool JobShouldPauseWhenFinished(IJobContextMessage jobContextMessage);
    }
}
