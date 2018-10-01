﻿using System.Collections.Generic;
using System.Linq;
using ESFA.DC.JobContext;
using ESFA.DC.Mapping.Interface;
using JobContextMessage = ESFA.DC.JobContextManager.Model.JobContextMessage;
using TaskItem = ESFA.DC.JobContextManager.Model.TaskItem;
using TopicItem = ESFA.DC.JobContextManager.Model.TopicItem;

namespace ESFA.DC.JobContextManager
{
    public sealed class JobContextDtoToMessageMapper : IMapper<JobContextDto, JobContextMessage>
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
                    new TopicItem()
                    {
                        SubscriptionName = topic.SubscriptionName,
                        Tasks = topic.Tasks.Select(task =>
                            new TaskItem()
                            {
                                SupportsParallelExecution = task.SupportsParallelExecution,
                                Tasks = task.Tasks.ToList()
                            }).ToList()
                    }).ToList()
            };
        }
    }
}