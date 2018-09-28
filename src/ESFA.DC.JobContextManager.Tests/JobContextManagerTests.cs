using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.JobContextManager.Tests
{
    public class JobContextManagerTests
    {
        [Fact]
        public void OpenAsync()
        {
            var cancellationToken = CancellationToken.None;

            var loggerMock = new Mock<ILogger>();
            var topicSubscriptionServiceMock = new Mock<ITopicSubscriptionService<JobContextDto>>();

            loggerMock.Setup(l => l.LogInfo("Opening Job Context Manager method invoked, Topic Subscription Subscribing", It.IsAny<object[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Verifiable();
            topicSubscriptionServiceMock.Setup(s => s.Subscribe(It.IsAny<Func<JobContextDto, IDictionary<string, object>, CancellationToken, Task<IQueueCallbackResult>>>(), cancellationToken)).Verifiable();

            NewManager<string>(topicSubscriptionService: topicSubscriptionServiceMock.Object, logger: loggerMock.Object).OpenAsync(cancellationToken);

            loggerMock.Verify();
            topicSubscriptionServiceMock.Verify();
        }

        [Fact]
        public async Task CloseAsync()
        {
            var cancellationToken = CancellationToken.None;

            var loggerMock = new Mock<ILogger>();
            var topicSubscriptionServiceMock = new Mock<ITopicSubscriptionService<JobContextDto>>();

            loggerMock.Setup(l => l.LogInfo("Closing Job Context Manager method invoked, Topic Subscription Unsubscribing", It.IsAny<object[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Verifiable();
            topicSubscriptionServiceMock.Setup(s => s.UnsubscribeAsync()).Returns(Task.CompletedTask).Verifiable();


            var manager = NewManager<string>(topicSubscriptionService: topicSubscriptionServiceMock.Object, logger: loggerMock.Object);

            await manager.CloseAsync();

            loggerMock.VerifyAll();
            topicSubscriptionServiceMock.VerifyAll();
        }

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

            var jobStatusDto = NewManager<string>().BuildJobStatusDto(jobId, jobStatusType);

            jobStatusDto.JobId.Should().Be(jobId);
            jobStatusDto.JobStatus.Should().Be(jobStatusTypeInteger);
        }

        private JobContextManager<T> NewManager<T>(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService = null,
            ITopicPublishService<JobContextDto> topicPublishService = null,
            IMapper<JobContextMessage, T> mapper = null,
            IQueuePublishService<JobStatusDto> jobStatusDtoQueuePublishService = null,
            IQueuePublishService<AuditingDto> auditingDtoQueuePublishService = null,
            ILogger logger = null,
            IMessageHandler<T> messageHandler = null)
            where T : class
        {
            return new JobContextManager<T>(topicSubscriptionService, topicPublishService, mapper, jobStatusDtoQueuePublishService, auditingDtoQueuePublishService, logger, messageHandler);
        }
    }
}
