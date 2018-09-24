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
        public void TestJobContextSerialise()
        {
            DateTime dateTime = DateTime.UtcNow;

            var taskItems = new List<ITaskItem>
            {
                new TaskItem(
                    new List<string>
                    {
                        "Task 1",
                        "Task 2"
                    },
                    true)
            };

            IReadOnlyList<ITopicItem> topics = new List<ITopicItem>
            {
                new TopicItem(
                    "Subscription A",
                    "SqlFilter A",
                    taskItems)
            };

            var jobContextMessage = new JobContextMessage(
                9999, topics, "UkPrn", "Container", "Filename", "Username", 12, dateTime);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            var serialised = JsonConvert.SerializeObject(jobContextMessage, settings);

            var deserialised = JsonConvert.DeserializeObject<JobContextMessage>(serialised, settings);

            deserialised.JobId.Should().Be(9999);
            deserialised.SubmissionDateTimeUtc.Should().Be(dateTime);
            deserialised.TopicPointer.Should().Be(12);
            deserialised.Topics.Should().BeEquivalentTo(topics);
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
