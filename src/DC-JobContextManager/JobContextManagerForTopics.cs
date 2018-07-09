using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobContextManager
{
    public sealed class JobContextManagerForTopics<T> : JobContextManagerBase<T>, IJobContextManager<T>
        where T : class
    {
        private readonly ITopicSubscriptionService<JobContextDto> _topicSubscriptionService;

        public JobContextManagerForTopics(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IAuditor auditor,
            IMapper<JobContextMessage, T> mapper,
            IJobStatus jobStatus,
            ILogger logger,
            Func<T, CancellationToken, Task<bool>> callback)
            : base(topicPublishService, auditor, mapper, jobStatus, logger, callback)
        {
            _topicSubscriptionService = topicSubscriptionService;
        }

        public async Task FinishSuccessfully(IJobContextMessage jobContextMessage)
        {
            await _auditor.AuditEndAsync(jobContextMessage);
        }

        public async Task FinishError(IJobContextMessage jobContextMessage)
        {
            await _auditor.AuditJobFailAsync(jobContextMessage);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _topicSubscriptionService.Subscribe(Callback);
            return Task.FromResult("EndPoint");
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Closed Async method invoked");
            await _topicSubscriptionService.UnsubscribeAsync();
        }

        public void Abort()
        {
            _logger.LogInfo("Abort method invoked");
            _topicSubscriptionService.UnsubscribeAsync().Wait();
        }
    }
}