using System;
using System.Collections.Generic;
using ESFA.DC.JobContext;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.JobContextManager.Tests
{
    public class JobContextMappingTests
    {
        [Fact]
        public void DtoToMessage()
        {
            DateTime now = DateTime.UtcNow;

            var jobContextDto = new JobContextDto
            {
                SubmissionDateTimeUtc = now,
                JobId = 12,
                KeyValuePairs = new Dictionary<string, object>
                {
                    { JobContextMessageKey.UkPrn, 12345 }
                },
                TopicPointer = 3,
                Topics = new List<TopicItemDto>
                {
                    new TopicItemDto
                    {
                        Tasks = new List<TaskItemDto>
                        {
                            new TaskItemDto
                            {
                                Tasks = new List<string>
                                {
                                    "Task A"
                                },
                                SupportsParallelExecution = true
                            }
                        },
                        SubscriptionName = "Subscription A"
                    }
                }
            };

            var mapper = new JobContextDtoToMessageMapper();
            var message = mapper.MapTo(jobContextDto);

            message.SubmissionDateTimeUtc.Should().Be(now);
            message.JobId.Should().Be(12);
            message.KeyValuePairs.Should().BeEquivalentTo(jobContextDto.KeyValuePairs);
            message.Topics.Should().BeEquivalentTo(jobContextDto.Topics);
            message.TopicPointer.Should().Be(3);
        }

        [Fact]
        public void MessageToDto()
        {
            DateTime now = DateTime.UtcNow;

            List<ITaskItem> tasks = new List<ITaskItem>()
            {
                new TaskItem
                {
                    Tasks = new List<string>
                    {
                        "Task A"
                    },
                    SupportsParallelExecution = true
                }
            };
            var topicItem = new TopicItem("Topic A", tasks);

            JobContextMessage jobContextMessage = new JobContextMessage
            {
                JobId = 12,
                KeyValuePairs = new Dictionary<string, object>
                {
                    { JobContextMessageKey.UkPrn, 12345 }
                },
                SubmissionDateTimeUtc = now,
                TopicPointer = 3,
                Topics = new List<ITopicItem>
                {
                    topicItem
                }
            };

            JobContextDtoToMessageMapper mapper = new JobContextDtoToMessageMapper();
            JobContextDto dto = mapper.MapFrom(jobContextMessage);

            dto.SubmissionDateTimeUtc.Should().Be(now);
            dto.JobId.Should().Be(12);
            dto.KeyValuePairs.Should().BeEquivalentTo(jobContextMessage.KeyValuePairs);
            dto.Topics.Should().BeEquivalentTo(jobContextMessage.Topics);
            dto.TopicPointer.Should().Be(3);
        }
    }
}
