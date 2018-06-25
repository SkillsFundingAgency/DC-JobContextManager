using System;
using System.Threading;
using System.Threading.Tasks;
using DC.JobContextManager.Interface;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;

namespace DC.JobContextManager
{
    public sealed class JobContextManagerForQueue<T> : JobContextManagerBase<T>, IJobContextManager
        where T : class
    {
        private readonly IQueueSubscriptionService<JobContextDto> _queueSubscriptionService;

        public JobContextManagerForQueue(
            IQueueSubscriptionService<JobContextDto> queueSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IAuditor auditor,
            IMapper<JobContextMessage, T> mapper,
            IJobStatus jobStatus,
            ILogger logger,
            Func<T, CancellationToken, Task<bool>> callback)
            : base(topicPublishService, auditor, mapper, jobStatus, logger, callback)
        {
            _queueSubscriptionService = queueSubscriptionService;
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
            _queueSubscriptionService.Subscribe(Callback);
            return Task.FromResult("EndPoint");
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Closed Async method invoked");
            await _queueSubscriptionService.UnsubscribeAsync();
        }

        public void Abort()
        {
            _logger.LogInfo("Abort method invoked");
            _queueSubscriptionService.UnsubscribeAsync().Wait();
        }
    }
}