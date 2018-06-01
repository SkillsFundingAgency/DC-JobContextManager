using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DC.JobContextManager.Interface;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;

namespace DC.JobContextManager
{
    public sealed class JobContextManager<T> : IJobContextManager
        where T : new()
    {
        private readonly ITopicPublishService<JobContextDto> _topicPublishService;

        private readonly IAuditor _auditor;

        private readonly IMapper<JobContextMessage, T> _mapper;

        private readonly Func<T, CancellationToken, Task<bool>> _callback;

        private readonly JobContextMapper _jobContextMapper;

        private readonly ILogger _logger;

        private readonly IQueueSubscriptionService<JobContextDto> _queueSubscriptionService;

        public JobContextManager(
            IQueueSubscriptionService<JobContextDto> queueSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IAuditor auditor,
            IMapper<JobContextMessage, T> mapper,
            Func<T, CancellationToken, Task<bool>> callback,
            ILogger logger)
        {
            _topicPublishService = topicPublishService;
            _auditor = auditor;
            _mapper = mapper;
            _callback = callback;
            _logger = logger;
            _queueSubscriptionService = queueSubscriptionService;
            _jobContextMapper = new JobContextMapper();
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

        private async Task<bool> Callback(JobContextDto jobContextDto, CancellationToken cancellationToken)
        {
            JobContextMessage jobContextMessage = _jobContextMapper.MapTo(jobContextDto);

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
                await _auditor.AuditEndAsync(jobContextMessage);

                jobContextMessage.TopicPointer++;
                if (jobContextMessage.TopicPointer >= jobContextMessage.Topics.Count)
                {
                    return true;
                }

                // get the next subscriptionName
                var subscriptionSqlFilterValue = jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionSqlFilterValue;
                string nextTopicSubscriptionName = jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName;

                // create properties for topic with sqlfilter
                var topicProperties = new Dictionary<string, object>()
                {
                    { "To", subscriptionSqlFilterValue }
                };

                jobContextDto = _jobContextMapper.MapFrom(jobContextMessage);
                await _topicPublishService.PublishAsync(jobContextDto, topicProperties, nextTopicSubscriptionName);
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
