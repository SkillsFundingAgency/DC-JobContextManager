using System;
using System.Collections.Generic;
using System.Linq;
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
            topicSubscriptionServiceMock.Setup(s => s.UnsubscribeAsync()).Verifiable();

            NewManager<string>(topicSubscriptionService: topicSubscriptionServiceMock.Object, logger: loggerMock.Object).CloseAsync();

            loggerMock.Verify();
            topicSubscriptionServiceMock.Verify();
        }

        private JobContextManager<T> NewManager<T>(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService = null,
            ITopicPublishService<JobContextDto> topicPublishService = null,
            IAuditor auditor = null,
            IMapper<JobContextMessage, T> mapper = null,
            IJobStatus jobStatus = null,
            ILogger logger = null,
            IMessageHandler<T> messageHandler = null)
            where T : class
        {
            return new JobContextManager<T>(topicSubscriptionService, topicPublishService, auditor, mapper, jobStatus, logger, messageHandler);
        }
    }
}
