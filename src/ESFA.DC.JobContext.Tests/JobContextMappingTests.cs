using System;
using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.JobContext.Tests
{
    public sealed class JobContextMappingTests
    {
        [Fact]
        public void TestJobContextMappingDtoToInterface()
        {
            DateTime now = DateTime.UtcNow;

            JobContextDto jobContextDto = new JobContextDto
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

            JobContextMapper mapper = new JobContextMapper();
            JobContextMessage message = mapper.MapTo(jobContextDto);

            message.SubmissionDateTimeUtc.Should().Be(now);
            message.JobId.Should().Be(12);
            message.KeyValuePairs.Should().BeEquivalentTo(jobContextDto.KeyValuePairs);
            message.Topics.Should().BeEquivalentTo(jobContextDto.Topics);
            message.TopicPointer.Should().Be(3);
        }

        [Fact]
        public void TestJobContextMappingInterfaceToDto()
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
            var topicItem = new TopicItem("Topic A", "SqlFilter A", tasks);

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

            JobContextMapper mapper = new JobContextMapper();
            JobContextDto dto = mapper.MapFrom(jobContextMessage);

            dto.SubmissionDateTimeUtc.Should().Be(now);
            dto.JobId.Should().Be(12);
            dto.KeyValuePairs.Should().BeEquivalentTo(jobContextMessage.KeyValuePairs);
            dto.Topics.Should().BeEquivalentTo(jobContextMessage.Topics);
            dto.TopicPointer.Should().Be(3);
        }
    }
}
