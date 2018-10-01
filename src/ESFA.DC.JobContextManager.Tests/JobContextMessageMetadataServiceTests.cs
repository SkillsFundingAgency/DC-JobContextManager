using System.Collections.Generic;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.JobStatus.Interface;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.JobContextManager.Tests
{
    public class JobContextMessageMetadataServiceTests
    {
        [Theory]
        [InlineData(1, JobStatusType.Ready)]
        [InlineData(2, JobStatusType.MovedForProcessing)]
        [InlineData(3, JobStatusType.Processing)]
        [InlineData(4, JobStatusType.Completed)]
        [InlineData(5, JobStatusType.FailedRetry)]
        [InlineData(6, JobStatusType.Failed)]
        [InlineData(7, JobStatusType.Paused)]
        [InlineData(8, JobStatusType.Waiting)]
        public void BuildJobStatusDto(int jobStatusTypeInteger, JobStatusType jobStatusType)
        {
            var jobId = 1;

            var jobContextMessage = new JobContextMessage()
            {
                JobId = jobId
            };

            var jobStatusDto = NewService().BuildJobStatusDto(jobContextMessage, jobStatusType);

            jobStatusDto.JobId.Should().Be(jobId);
            jobStatusDto.JobStatus.Should().Be(jobStatusTypeInteger);
        }

        [Theory]
        [InlineData(0, AuditEventType.JobSubmitted)]
        [InlineData(1, AuditEventType.JobStarted)]
        [InlineData(2, AuditEventType.ServiceStarted)]
        [InlineData(3, AuditEventType.ServiceFailed)]
        [InlineData(4, AuditEventType.ServiceFinished)]
        [InlineData(5, AuditEventType.JobFailed)]
        [InlineData(6, AuditEventType.JobFinished)]
        public void BuildAuditingDto(int auditEventTypeInt, AuditEventType auditEventType)
        {
            var jobId = 1;
            var extraInfo = "extra info";
            var fileName = "FileName";
            var topicPointer = 0;
            var subscriptionName = "SubscriptionName";
            var ukprn = "100000";
            var username = "Username";

            var topics = new List<ITopicItem>()
            {
                new TopicItem()
                {
                    SubscriptionName = subscriptionName
                }
            };

            var keyValuePairs = new Dictionary<string, object>()
            {
                { "Filename",  fileName },
                { "UkPrn", ukprn },
                { "Username", username }
            };

            var jobContextMessage = new JobContextMessage()
            {
                JobId = jobId,
                KeyValuePairs = keyValuePairs,
                TopicPointer = topicPointer,
                Topics = topics
            };

            var auditingDto = NewService().BuildAuditingDto(jobContextMessage, auditEventType, extraInfo);

            auditingDto.JobId.Should().Be(jobId);
            auditingDto.EventType.Should().Be(auditEventTypeInt);
            auditingDto.ExtraInfo.Should().Be(extraInfo);
            auditingDto.Filename.Should().Be(fileName);
            auditingDto.Source.Should().Be(subscriptionName);
            auditingDto.UkPrn.Should().Be(ukprn);
            auditingDto.UserId.Should().Be(username);
        }

        [Fact]
        public void PointerIsFirstTopic_True()
        {
            var jobContextMessage = new JobContextMessage()
            {
                TopicPointer = 0
            };

            NewService().PointerIsFirstTopic(jobContextMessage).Should().BeTrue();
        }

        [Fact]
        public void PointerIsFirstTopic_False()
        {
            var jobContextMessage = new JobContextMessage()
            {
                TopicPointer = 1
            };

            NewService().PointerIsFirstTopic(jobContextMessage).Should().BeFalse();
        }

        [Fact]
        public void PointerIsLastTopic_True()
        {
            var jobContextMessage = new JobContextMessage()
            {
                TopicPointer = 2,
                Topics = new List<ITopicItem>()
                {
                    new TopicItem(),
                    new TopicItem(),
                    new TopicItem(),
                }
            };

            NewService().PointerIsLastTopic(jobContextMessage).Should().BeTrue();
        }

        [Fact]
        public void PointerIsLastTopic_True_Exceeded()
        {
            var jobContextMessage = new JobContextMessage()
            {
                TopicPointer = 2,
                Topics = new List<ITopicItem>()
                {
                    new TopicItem(),
                    new TopicItem(),
                }
            };

            NewService().PointerIsLastTopic(jobContextMessage).Should().BeTrue();
        }

        [Fact]
        public void PointerIsLastTopic_False()
        {
            var jobContextMessage = new JobContextMessage()
            {
                TopicPointer = 1,
                Topics = new List<ITopicItem>()
                {
                    new TopicItem(),
                    new TopicItem(),
                    new TopicItem(),
                }
            };

            NewService().PointerIsLastTopic(jobContextMessage).Should().BeFalse();
        }

        [Fact]
        public void JobShouldPauseWhenFinished_True()
        {
            var jobContextMessage = new JobContextMessage()
            {
                KeyValuePairs = new Dictionary<string, object>()
                {
                    { "PauseWhenFinished", "Doesn't Matter" }
                }
            };

            NewService().JobShouldPauseWhenFinished(jobContextMessage).Should().BeTrue();
        }

        [Fact]
        public void JobShouldPauseWhenFinished_False()
        {

            var jobContextMessage = new JobContextMessage()
            {
                KeyValuePairs = new Dictionary<string, object>()
            };

            NewService().JobShouldPauseWhenFinished(jobContextMessage).Should().BeFalse();
        }

        private JobContextMessageMetadataService NewService()
        {
            return new JobContextMessageMetadataService();
        }
    }
}
