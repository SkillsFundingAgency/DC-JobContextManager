using System;
using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace ESFA.DC.JobContext.Tests
{
    public sealed class JobContextTests
    {
        [Fact]
        public void TestJobContextDtoSerialise()
        {
            DateTime dateTime = DateTime.UtcNow;

            var jobContextMessage = new JobContextDto()
            {
                JobId = 9999,
                SubmissionDateTimeUtc = dateTime,
                TopicPointer = 12,
                KeyValuePairs = new Dictionary<string, object>()
                {
                    { "UkPrn", "UkPrn" },
                    { "Container", "Container" },
                    { "Filename", "Filename" },
                    { "Username", "Username" }
                },
                Topics = new List<TopicItemDto>
                {
                    new TopicItemDto()
                    {
                        SubscriptionName = "Subscription A",
                        Tasks = new List<TaskItemDto>
                        {
                            new TaskItemDto()
                            {
                                SupportsParallelExecution = true,
                                Tasks = new List<string>
                                {
                                    "Task 1",
                                    "Task 2"
                                }
                            }
                        }
                    }
                }
            };

            var serialised = JsonConvert.SerializeObject(jobContextMessage);

            var deserialised = JsonConvert.DeserializeObject<JobContextDto>(serialised);

            deserialised.JobId.Should().Be(9999);
            deserialised.SubmissionDateTimeUtc.Should().Be(dateTime);
            deserialised.TopicPointer.Should().Be(12);
            deserialised.Topics.Should().BeEquivalentTo(deserialised.Topics);
            deserialised.KeyValuePairs.Should().ContainKey(JobContextMessageKey.UkPrn);
            deserialised.KeyValuePairs[JobContextMessageKey.UkPrn].Should().Be("UkPrn");
            deserialised.KeyValuePairs.Should().ContainKey(JobContextMessageKey.Filename);
            deserialised.KeyValuePairs[JobContextMessageKey.Filename].Should().Be("Filename");
            deserialised.KeyValuePairs.Should().ContainKey(JobContextMessageKey.Container);
            deserialised.KeyValuePairs[JobContextMessageKey.Container].Should().Be("Container");
            deserialised.KeyValuePairs.Should().ContainKey(JobContextMessageKey.Username);
            deserialised.KeyValuePairs[JobContextMessageKey.Username].Should().Be("Username");
        }
    }
}
