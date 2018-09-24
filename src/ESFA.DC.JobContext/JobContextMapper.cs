using System.Collections.Generic;
using System.Linq;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Mapping.Interface;

namespace ESFA.DC.JobContext
{
    public sealed class JobContextMapper : IMapper<JobContextDto, JobContextMessage>
    {
        public JobContextDto MapFrom(JobContextMessage value)
        {
            JobContextDto jobContextDto = new JobContextDto
            {
                JobId = value.JobId,
                KeyValuePairs = new Dictionary<string, object>(value.KeyValuePairs),
                SubmissionDateTimeUtc = value.SubmissionDateTimeUtc,
                TopicPointer = value.TopicPointer,
                Topics = new List<TopicItemDto>()
            };

            foreach (ITopicItem topic in value.Topics)
            {
                TopicItemDto topicItem = new TopicItemDto
                {
                    SubscriptionName = topic.SubscriptionName,
                    SubscriptionSqlFilterValue = topic.SubscriptionSqlFilterValue,
                    Tasks = new List<TaskItemDto>()
                };
                foreach (ITaskItem task in topic.Tasks)
                {
                    topicItem.Tasks.Add(new TaskItemDto
                    {
                        SupportsParallelExecution = task.SupportsParallelExecution,
                        Tasks = task.Tasks.ToList()
                    });
                }

                jobContextDto.Topics.Add(topicItem);
            }

            return jobContextDto;
        }

        public JobContextMessage MapTo(JobContextDto value)
        {
            List<TopicItem> topicItems = new List<TopicItem>();
            foreach (TopicItemDto topicItemDto in value.Topics)
            {
                List<TaskItem> taskItems = new List<TaskItem>();
                foreach (TaskItemDto taskItemDto in topicItemDto.Tasks)
                {
                    taskItems.Add(new TaskItem(taskItemDto.Tasks.ToList(), taskItemDto.SupportsParallelExecution));
                }

                var topicItem = new TopicItem(
                    topicItemDto.SubscriptionName,
                    topicItemDto.SubscriptionSqlFilterValue,
                    taskItems);

                topicItems.Add(topicItem);
            }

            JobContextMessage jobContextMessage =
                new JobContextMessage(value.JobId, topicItems, value.TopicPointer, value.SubmissionDateTimeUtc)
                {
                    KeyValuePairs = value.KeyValuePairs
                };

            return jobContextMessage;
        }
    }
}
