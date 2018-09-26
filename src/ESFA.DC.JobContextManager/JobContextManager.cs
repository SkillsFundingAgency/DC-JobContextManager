using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobContextManager
{
    public class JobContextManager<T> : IJobContextManager<T>
        where T : class
    {
        private readonly ITopicSubscriptionService<JobContextDto> _topicSubscriptionService;
        private readonly ITopicPublishService<JobContextDto> _topicPublishService;
        private readonly IAuditor _auditor;
        private readonly IMapper<JobContextMessage, T> _mapper;
        private readonly IQueuePublishService<JobStatusDto> _jobStatusDtoQueuePublishService;
        private readonly ILogger _logger;
        private readonly IMessageHandler<T> _messageHandler;
        private readonly IMapper<JobContextDto, JobContextMessage> _jobContextMapper;

        public JobContextManager(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IAuditor auditor,
            IMapper<JobContextMessage, T> mapper,
            IQueuePublishService<JobStatusDto> jobStatusDtoQueuePublishService,
            ILogger logger,
            IMessageHandler<T> messageHandler)
        {
            _topicSubscriptionService = topicSubscriptionService;
            _topicPublishService = topicPublishService;
            _auditor = auditor;
            _mapper = mapper;
            _jobStatusDtoQueuePublishService = jobStatusDtoQueuePublishService;
            _logger = logger;
            _messageHandler = messageHandler;
            _jobContextMapper = new JobContextMapper();
        }

        public void OpenAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Opening Job Context Manager method invoked, Topic Subscription Subscribing");
            _topicSubscriptionService.Subscribe(Callback, cancellationToken);
        }

        public async Task CloseAsync()
        {
            _logger.LogInfo("Closing Job Context Manager method invoked, Topic Subscription Unsubscribing");
            await _topicSubscriptionService.UnsubscribeAsync();
        }

        public JobStatusDto BuildJobStatusDto(long jobId, JobStatusType jobStatusType)
        {
            return new JobStatusDto(jobId, (int)jobStatusType);
        }

        private async Task<IQueueCallbackResult> Callback(JobContextDto jobContextDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            JobContextMessage jobContextMessage = _jobContextMapper.MapTo(jobContextDto);
            JobContextMessage jobContextMessageConst = _jobContextMapper.MapTo(jobContextDto);

            try
            {
                await _auditor.AuditStartAsync(jobContextMessageConst);
                if (jobContextMessage.TopicPointer == 0)
                {
                    await _jobStatusDtoQueuePublishService.PublishAsync(BuildJobStatusDto(jobContextMessage.JobId, JobStatusType.Processing));
                }

                T obj = _mapper.MapTo(jobContextMessage);

                if (!await _messageHandler.HandleAsync(obj, cancellationToken))
                {
                    await _auditor.AuditJobFailAsync(jobContextMessageConst);
                    return new QueueCallbackResult(false, null);
                }

                jobContextMessage = _mapper.MapFrom(obj);

                await _auditor.AuditEndAsync(jobContextMessageConst);

                jobContextMessage.TopicPointer++;
                if (jobContextMessage.TopicPointer >= jobContextMessage.Topics.Count)
                {
                    if (jobContextMessage.KeyValuePairs.ContainsKey(JobContextMessageKey.PauseWhenFinished))
                    {
                        await _jobStatusDtoQueuePublishService.PublishAsync(BuildJobStatusDto(jobContextMessage.JobId, JobStatusType.Waiting));
                    }
                    else
                    {
                        await _jobStatusDtoQueuePublishService.PublishAsync(BuildJobStatusDto(jobContextMessage.JobId, JobStatusType.Completed));
                    }

                    return new QueueCallbackResult(true, null);
                }

                // get the next subscription name
                string nextTopicSubscriptionName = jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName;

                // create properties for topic with sqlfilter
                var topicProperties = new Dictionary<string, object>
                {
                    { "To", nextTopicSubscriptionName }
                };

                jobContextDto = _jobContextMapper.MapFrom(jobContextMessage);
                await _topicPublishService.PublishAsync(jobContextDto, topicProperties, nextTopicSubscriptionName);

                return new QueueCallbackResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception thrown in JobContextManager callback", ex, new object[] { jobContextDto.JobId });
                await _auditor.AuditJobFailAsync(jobContextMessageConst);
                return new QueueCallbackResult(false, ex);
            }
        }
    }
}