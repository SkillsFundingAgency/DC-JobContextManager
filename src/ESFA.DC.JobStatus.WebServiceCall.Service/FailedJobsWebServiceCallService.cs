using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.JobStatus.WebServiceCall.Service
{
    public sealed class FailedJobsWebServiceCallService : BaseWebServiceCallService, IJobStatusWebServiceCallService<JobContextDto>
    {
        private readonly IQueueSubscriptionService<JobContextDto> _queueSubscriptionService;

        public FailedJobsWebServiceCallService(
            IJobStatusWebServiceCallServiceConfig jobStatusWebServiceCallServiceConfig,
            IQueueSubscriptionService<JobContextDto> queueSubscriptionService,
            ISerializationService serializationService,
            ILogger logger)
            : base(jobStatusWebServiceCallServiceConfig, serializationService, logger)
        {
            _queueSubscriptionService = queueSubscriptionService;
        }

        public void Subscribe()
        {
            _queueSubscriptionService.Subscribe(ProcessDeadLetterMessageAsync, CancellationToken.None);
        }

        private async Task<IQueueCallbackResult> ProcessDeadLetterMessageAsync(JobContextDto jobContextDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            try
            {
                bool irrecoverable = false;
                if (messageProperties.TryGetValue("Exceptions", out object exceptions))
                {
                    string[] exceptionList = exceptions.ToString().Split(':');
                    if (exceptionList.Contains("NullReferenceException"))
                    {
                        irrecoverable = true;
                    }
                }

                if (irrecoverable)
                {
                    await SendStatusAsync(jobContextDto.JobId, (int)JobStatusType.Failed, cancellationToken);
                }
                else
                {
                    await SendStatusAsync(jobContextDto.JobId, (int)JobStatusType.FailedRetry, cancellationToken);
                }

                return new QueueCallbackResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to post dead letter job status message", ex);
                return new QueueCallbackResult(false, ex);
            }
        }
    }
}
