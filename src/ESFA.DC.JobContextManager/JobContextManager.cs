using System;
using System.Collections.Generic;
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
        private readonly IMapper<JobContextDto, JobContextMessage> _jobContextMapper;

        public JobContextManager(
            ITopicSubscriptionService<JobContextDto> topicSubscriptionService,
            ITopicPublishService<JobContextDto> topicPublishService,
            IMapper<JobContextMessage, T> mapper,
            IQueuePublishService<JobStatusDto> jobStatusDtoQueuePublishService,
            IQueuePublishService<AuditingDto> auditingDtoQueuePublishService,
            ILogger logger,
            IMessageHandler<T> messageHandler)
        {
            _topicSubscriptionService = topicSubscriptionService;
            _topicPublishService = topicPublishService;
            _mapper = mapper;
            _jobStatusDtoQueuePublishService = jobStatusDtoQueuePublishService;
            _auditingDtoQueuePublishService = auditingDtoQueuePublishService;
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

        public AuditingDto BuildAuditingDto(JobContextMessage jobContextMessage, AuditEventType auditEventType, string extraInfo = null)
        {
            return new AuditingDto()
            {
                EventType = (int)auditEventType,
                ExtraInfo = extraInfo,
                Filename = (string)jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename],
                JobId = jobContextMessage.JobId,
                Source = jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName,
                UkPrn = (string)jobContextMessage.KeyValuePairs[JobContextMessageKey.Username],
                UserId = (string)jobContextMessage.KeyValuePairs[JobContextMessageKey.Username],
            };
        }

        public bool PointerIsFirstTopic(JobContextMessage jobContextMessage)
        {
            return jobContextMessage.TopicPointer == 0;
        }

        public bool PointerIsLastTopic(JobContextMessage jobContextMessage)
        {
            return jobContextMessage.TopicPointer == jobContextMessage.Topics.Count - 1;
        }

        private async Task<IQueueCallbackResult> Callback(JobContextDto jobContextDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            JobContextMessage jobContextMessage = _jobContextMapper.MapTo(jobContextDto);
            JobContextMessage jobContextMessageConst = _jobContextMapper.MapTo(jobContextDto);

            try
            {
                if (PointerIsFirstTopic(jobContextMessageConst))
                {
                    await _auditingDtoQueuePublishService.PublishAsync(BuildAuditingDto(jobContextMessageConst, AuditEventType.JobStarted));
                }

                await _auditingDtoQueuePublishService.PublishAsync(BuildAuditingDto(jobContextMessageConst, AuditEventType.ServiceStarted));

                if (jobContextMessage.TopicPointer == 0)
                {
                    await _jobStatusDtoQueuePublishService.PublishAsync(BuildJobStatusDto(jobContextMessage.JobId, JobStatusType.Processing));
                }

                T obj = _mapper.MapTo(jobContextMessage);

                if (!await _messageHandler.HandleAsync(obj, cancellationToken))
                {
                    await _auditingDtoQueuePublishService.PublishAsync(BuildAuditingDto(jobContextMessageConst, AuditEventType.JobFailed));
                    return new QueueCallbackResult(false, null);
                }

                jobContextMessage = _mapper.MapFrom(obj);

                await _auditingDtoQueuePublishService.PublishAsync(BuildAuditingDto(jobContextMessageConst, AuditEventType.ServiceFinished));

                if (PointerIsLastTopic(jobContextMessageConst))
                {
                    await _auditingDtoQueuePublishService.PublishAsync(BuildAuditingDto(jobContextMessageConst, AuditEventType.JobFinished));
                }

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

                await _auditingDtoQueuePublishService.PublishAsync(BuildAuditingDto(jobContextMessageConst, AuditEventType.JobFailed, ex.ToString()));

                return new QueueCallbackResult(false, ex);
            }
        }
    }
}



//using System;
//using System.Threading.Tasks;
//using ESFA.DC.Auditing.Dto;
//using ESFA.DC.Auditing.Interface;
//using ESFA.DC.JobContext.Interface;
//using ESFA.DC.Queueing.Interface;

//namespace ESFA.DC.Auditing
//{
//    public sealed class Auditor : IAuditor
//    {
//        private readonly IQueuePublishService<AuditingDto> _queuePublishService;

//        public Auditor(IQueuePublishService<AuditingDto> queuePublishService)
//        {
//            _queuePublishService = queuePublishService;
//        }

//        public async Task AuditStartAsync(IJobContextMessage jobContextMessage)
//        {
//            if (jobContextMessage.TopicPointer == 0)
//            {
//                await AuditAsync(
//                    jobContextMessage,
//                    AuditEventType.JobStarted);
//            }

//            await AuditAsync(
//                jobContextMessage,
//                AuditEventType.ServiceStarted);
//        }

//        public async Task AuditServiceFailAsync(IJobContextMessage jobContextMessage, Exception ex)
//        {
//            await AuditAsync(
//                jobContextMessage,
//                AuditEventType.ServiceFailed,
//                ex.ToString());
//        }

//        public async Task AuditServiceFailAsync(IJobContextMessage jobContextMessage, string message)
//        {
//            await AuditAsync(
//                jobContextMessage,
//                AuditEventType.ServiceFailed,
//                message);
//        }

//        public async Task AuditJobFailAsync(IJobContextMessage jobContextMessage)
//        {
//            await AuditAsync(
//                jobContextMessage,
//                AuditEventType.JobFailed);
//        }

//        public async Task AuditEndAsync(IJobContextMessage jobContextMessage)
//        {
//            await AuditAsync(
//                jobContextMessage,
//                AuditEventType.ServiceFinished);

//            if (jobContextMessage.TopicPointer == jobContextMessage.Topics.Count - 1)
//            {
//                await AuditAsync(
//                    jobContextMessage,
//                    AuditEventType.JobFinished);
//            }
//        }

//        public async Task AuditAsync(
//            IJobContextMessage jobContextMessage,
//            AuditEventType eventType,
//            string extraInfo = null)
//        {
//            await AuditAsync(
//                jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName,
//                eventType,
//                (string)jobContextMessage.KeyValuePairs[JobContextMessageKey.Username],
//                jobContextMessage.JobId,
//                (string)jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename],
//                (string)jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn],
//                extraInfo);
//        }

//        public async Task AuditAsync(
//            string source,
//            AuditEventType eventType,
//            string userId,
//            long jobId = -1,
//            string filename = null,
//            string ukPrn = null,
//            string extraInfo = null)
//        {
//            await _queuePublishService.PublishAsync(new AuditingDto(source, (int)eventType, userId, jobId, filename, ukPrn, extraInfo));
//        }
//    }
//}
