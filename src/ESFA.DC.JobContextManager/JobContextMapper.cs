using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Mapping.Interface;

namespace ESFA.DC.JobContextManager
{
    public sealed class JobContextMapper : IMapper<JobContextDto, JobContextMessage>
    {
        public JobContextDto MapFrom(JobContextMessage value)
        {
            return new JobContextDto()
            {
                JobId = value.JobId,
                KeyValuePairs = new Dictionary<string, object>(value.KeyValuePairs),
                SubmissionDateTimeUtc = value.SubmissionDateTimeUtc,
                TopicPointer = value.TopicPointer,
                Topics = value.Topics.Select(topic =>
                    new TopicItemDto()
                    {
                        SubscriptionName = topic.SubscriptionName,
                        SubscriptionSqlFilterValue = topic.SubscriptionSqlFilterValue,
                        Tasks = topic.Tasks.Select(task =>
                            new TaskItemDto()
                            {
                                SupportsParallelExecution = task.SupportsParallelExecution,
                                Tasks = task.Tasks.ToList()
                            }).ToList()
                    }).ToList()
            };
        }

        public JobContextMessage MapTo(JobContextDto value)
        {
            return new JobContextMessage()
            {
                JobId = value.JobId,
                TopicPointer = value.TopicPointer,
                SubmissionDateTimeUtc = value.SubmissionDateTimeUtc,
                KeyValuePairs = value.KeyValuePairs,
                Topics = value.Topics.Select(topic =>
                    new TopicItem(
                        topic.SubscriptionName,
                        topic.SubscriptionSqlFilterValue,
                        topic.Tasks.Select(task =>
                            new TaskItem()
                            {
                                SupportsParallelExecution = task.SupportsParallelExecution,
                                Tasks = task.Tasks.ToList()
                            }).ToList())).ToList()
            };
        }
    }
}
