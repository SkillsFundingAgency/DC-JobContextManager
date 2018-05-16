using System;
using System.Threading;
using System.Threading.Tasks;
using DC.JobContextManager.Interface;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace DC.JobContextManager
{
    public sealed class JobContextManager<T> : IJobContextManager
        where T : new()
    {
        private readonly IQueuePublishService<JobContextMessage> _queuePublishService;

        private readonly IAuditor _auditor;

        private readonly IMapper<JobContextMessage, T> _mapper;

        private readonly Func<T, CancellationToken, Task<bool>> _callback;

        private readonly ILogger _logger;
        private readonly IQueueSubscriptionService<JobContextMessage> _queueSubscriptionService;

        public JobContextManager(
            IQueueSubscriptionService<JobContextMessage> queueSubscriptionService,
            IQueuePublishService<JobContextMessage> queuePublishService,
            IAuditor auditor,
            IMapper<JobContextMessage, T> mapper,
            Func<T, CancellationToken, Task<bool>> callback,
            ILogger logger)
        {
            _queuePublishService = queuePublishService;
            _auditor = auditor;
            _mapper = mapper;
            _callback = callback;
            _logger = logger;
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

            return Task.FromResult("Something");
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Closed Async method invoked");
            await _queueSubscriptionService.UnsubscribeAsync();
        }

        public void Abort()
        {
            _logger.LogInfo("Abort method invoked");
            _queueSubscriptionService.UnsubscribeAsync();
        }

        private async Task<bool> Callback(JobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            try
            {
                await _auditor.AuditStartAsync(jobContextMessage);
                T obj = _mapper.MapTo(jobContextMessage);
                if (!await _callback.Invoke(obj, cancellationToken))
                {
                    await _auditor.AuditJobFailAsync(jobContextMessage);
                    return false;
                }

                jobContextMessage = _mapper.MapFrom(obj);
                jobContextMessage.TopicPointer++;
                await _auditor.AuditEndAsync(jobContextMessage);
                if (jobContextMessage.TopicPointer >= jobContextMessage.Topics.Count)
                {
                    return true;
                }

                await _queuePublishService.PublishAsync(jobContextMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception thrown in JobManager callback", ex);
                await _auditor.AuditJobFailAsync(jobContextMessage);
                return false;
            }

            return true;
        }
    }
}
