using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
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
        private readonly IMapper<JobContextMessage, T> _mapper;
        private readonly IQueuePublishService<JobStatusDto> _jobStatusDtoQueuePublishService;
        private readonly IQueuePublishService<AuditingDto> _auditingDtoQueuePublishService;
        private readonly ILogger _logger;
        private readonly IMessageHandler<T> _messageHandler;
        private readonly IMapper<JobContextDto, JobContextMessage> _jobContextDtoToMessageMapper;
        private readonly IJobContextMessageMetadataService _jobContextMessageMetadataService;

        public JobContextManager(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IMapper<JobContextMessage, T> mapper,
            IQueuePublishService<JobStatusDto> jobStatusDtoQueuePublishService,
            IQueuePublishService<AuditingDto> auditingDtoQueuePublishService,
            ILogger logger,
            IMessageHandler<T> messageHandler)
            : this (
                topicSubscriptionService,
                topicPublishService,
                mapper,
                jobStatusDtoQueuePublishService,
                auditingDtoQueuePublishService,
                logger,
                messageHandler,
                new JobContextDtoToMessageMapper(),
                new JobContextMessageMetadataService())
        {
        }

        public JobContextManager(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IMapper<JobContextMessage, T> mapper,
            IQueuePublishService<JobStatusDto> jobStatusDtoQueuePublishService,
            IQueuePublishService<AuditingDto> auditingDtoQueuePublishService,
            ILogger logger,
            IMessageHandler<T> messageHandler,
            IMapper<JobContextDto, JobContextMessage> jobContextDtoToMessageMapper,
            IJobContextMessageMetadataService jobContextMessageMetadataService)
        {
            _topicSubscriptionService = topicSubscriptionService;
            _topicPublishService = topicPublishService;
            _mapper = mapper;
            _jobStatusDtoQueuePublishService = jobStatusDtoQueuePublishService;
            _auditingDtoQueuePublishService = auditingDtoQueuePublishService;
            _logger = logger;
            _messageHandler = messageHandler;
            _jobContextDtoToMessageMapper = jobContextDtoToMessageMapper;
            _jobContextMessageMetadataService = jobContextMessageMetadataService;
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

        private async Task<IQueueCallbackResult> Callback(JobContextDto jobContextDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            JobContextMessage jobContextMessage = _jobContextDtoToMessageMapper.MapTo(jobContextDto);

            try
            {
                if (_jobContextMessageMetadataService.PointerIsFirstTopic(jobContextMessage))
                {
                    await _auditingDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildAuditingDto(jobContextMessage, AuditEventType.JobStarted));
                    await _jobStatusDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildJobStatusDto(jobContextMessage, JobStatusType.Processing));
                }

                await _auditingDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildAuditingDto(jobContextMessage, AuditEventType.ServiceStarted));

                T obj = _mapper.MapTo(jobContextMessage);

                if (!await _messageHandler.HandleAsync(obj, cancellationToken))
                {
                    await _auditingDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildAuditingDto(jobContextMessage, AuditEventType.JobFailed));
                    return new QueueCallbackResult(false, null);
                }

                jobContextMessage = _mapper.MapFrom(obj);

                await _auditingDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildAuditingDto(jobContextMessage, AuditEventType.ServiceFinished));

                if (_jobContextMessageMetadataService.PointerIsLastTopic(jobContextMessage))
                {
                    await _auditingDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildAuditingDto(jobContextMessage, AuditEventType.JobFinished));

                    if (_jobContextMessageMetadataService.JobShouldPauseWhenFinished(jobContextMessage))
                    {
                        await _jobStatusDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildJobStatusDto(jobContextMessage, JobStatusType.Waiting));
                    }
                    else
                    {
                        await _jobStatusDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildJobStatusDto(jobContextMessage, JobStatusType.Completed));
                    }
                }
                else
                {
                    jobContextMessage.TopicPointer++;

                    string nextTopicSubscriptionName = jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName;

                    var nextTopicProperties = new Dictionary<string, object>
                    {
                        { "To", nextTopicSubscriptionName }
                    };

                    var nextTopicJobContextDto = _jobContextDtoToMessageMapper.MapFrom(jobContextMessage);
                    await _topicPublishService.PublishAsync(nextTopicJobContextDto, nextTopicProperties, nextTopicSubscriptionName);
                }

                return new QueueCallbackResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception thrown in JobContextManager callback", ex, new object[] { jobContextDto.JobId });

                await _auditingDtoQueuePublishService.PublishAsync(_jobContextMessageMetadataService.BuildAuditingDto(jobContextMessage, AuditEventType.JobFailed, ex.ToString()));

                return new QueueCallbackResult(false, ex);
            }
        }
    }
}