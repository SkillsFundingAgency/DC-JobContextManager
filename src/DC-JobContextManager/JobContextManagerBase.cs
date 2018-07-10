using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobContextManager
{
    public class JobContextManagerBase<T>
    {
        protected readonly IAuditor _auditor;

        protected readonly ILogger _logger;

        private readonly Func<T, CancellationToken, Task<bool>> _callback;

        private readonly ITopicPublishService<JobContextDto> _topicPublishService;

        private readonly IMapper<JobContextMessage, T> _mapper;

        private readonly IJobStatus _jobStatus;

        private readonly JobContextMapper _jobContextMapper;

        protected JobContextManagerBase(
            ITopicPublishService<JobContextDto> topicPublishService,
            IAuditor auditor,
            IMapper<JobContextMessage, T> mapper,
            IJobStatus jobStatus,
            ILogger logger,
            Func<T, CancellationToken, Task<bool>> callback)
        {
            _topicPublishService = topicPublishService;
            _auditor = auditor;
            _mapper = mapper;
            _jobStatus = jobStatus;
            _logger = logger;
            _callback = callback;
            _jobContextMapper = new JobContextMapper();
        }

        protected async Task<IQueueCallbackResult> Callback(JobContextDto jobContextDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            JobContextMessage jobContextMessage = _jobContextMapper.MapTo(jobContextDto);
            JobContextMessage jobContextMessageConst = _jobContextMapper.MapTo(jobContextDto);

            try
            {
                await _auditor.AuditStartAsync(jobContextMessageConst);
                if (jobContextMessage.TopicPointer == 0)
                {
                    await _jobStatus.JobStartedAsync(jobContextMessage.JobId);
                }

                T obj = _mapper.MapTo(jobContextMessage);
                if (!await _callback.Invoke(obj, cancellationToken))
                {
                    await _auditor.AuditJobFailAsync(jobContextMessageConst);
                    return new QueueCallbackResult(false, null);
                }

                jobContextMessage = _mapper.MapFrom(obj);

                await _auditor.AuditEndAsync(jobContextMessageConst);

                jobContextMessage.TopicPointer++;
                if (jobContextMessage.TopicPointer >= jobContextMessage.Topics.Count)
                {
                    int numLearners = GetNumberOfLearners(jobContextMessage.JobId, jobContextMessage.KeyValuePairs);

                    if (jobContextMessage.KeyValuePairs.ContainsKey(JobContextMessageKey.PauseWhenFinished))
                    {
                        await _jobStatus.JobAwaitingActionAsync(jobContextMessage.JobId, numLearners);
                    }
                    else
                    {
                        await _jobStatus.JobFinishedAsync(jobContextMessage.JobId);
                    }

                    return new QueueCallbackResult(true, null);
                }

                // get the next subscription name
                string nextTopicSubscriptionName =
                    jobContextMessage.Topics[jobContextMessage.TopicPointer].SubscriptionName;

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

        private int GetNumberOfLearners(long jobId, IDictionary<JobContextMessageKey, object> keyValuePairs)
        {
            int ret = 0;
            try
            {
                if (keyValuePairs.ContainsKey(JobContextMessageKey.ValidLearnRefNumbersCount))
                {
                    ret = Convert.ToInt32(keyValuePairs[JobContextMessageKey.ValidLearnRefNumbersCount]);
                }

                if (keyValuePairs.ContainsKey(JobContextMessageKey.InvalidLearnRefNumbersCount))
                {
                    ret = ret + Convert.ToInt32(keyValuePairs[JobContextMessageKey.InvalidLearnRefNumbersCount]);
                }

                _logger.LogInfo($"Found {ret} total learners for job : {jobId}");
                return ret == 0 ? -1 : ret;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception thrown in JobContextManager callback - GetNumberOfLearners", ex, new object[] { jobId });
            }

            return ret;
        }
    }
}
