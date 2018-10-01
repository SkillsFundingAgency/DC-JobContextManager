using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobContextManager
{
    public class JobContextMessageMetadataService : IJobContextMessageMetadataService
    {
        public JobStatusDto BuildJobStatusDto(IJobContextMessage jobContextMessage, JobStatusType jobStatusType)
        {
            return new JobStatusDto(jobContextMessage.JobId, (int)jobStatusType);
        }

        public AuditingDto BuildAuditingDto(IJobContextMessage jobContextMessage, AuditEventType auditEventType, string extraInfo = null)
        {
            return new AuditingDto()
            {
                EventType = (int)auditEventType,
                ExtraInfo = extraInfo,
                Filename = jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename].ToString(),
                JobId = jobContextMessage.JobId,
                Source = jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName,
                UkPrn = jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString(),
                UserId = jobContextMessage.KeyValuePairs[JobContextMessageKey.Username].ToString(),
            };
        }

        public bool PointerIsFirstTopic(IJobContextMessage jobContextMessage)
        {
            return jobContextMessage.TopicPointer == 0;
        }

        public bool PointerIsLastTopic(IJobContextMessage jobContextMessage)
        {
            return jobContextMessage.TopicPointer >= jobContextMessage.Topics.Count - 1;
        }

        public bool JobShouldPauseWhenFinished(IJobContextMessage jobContextMessage)
        {
            return jobContextMessage.KeyValuePairs.ContainsKey(JobContextMessageKey.PauseWhenFinished);
        }
    }
}
