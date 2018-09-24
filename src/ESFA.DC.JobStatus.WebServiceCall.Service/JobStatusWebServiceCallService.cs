using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.JobStatus.WebServiceCall.Service
{
    public sealed class JobStatusWebServiceCallService<T> : BaseWebServiceCallService, IJobStatusWebServiceCallService<T>
        where T : JobStatusDto, new()
    {
        private readonly IQueueSubscriptionService<T> _queueSubscriptionService;

        public JobStatusWebServiceCallService(
            IJobStatusWebServiceCallServiceConfig jobStatusWebServiceCallServiceConfig,
            IQueueSubscriptionService<T> queueSubscriptionService,
            ISerializationService serializationService,
            ILogger logger)
            : base(jobStatusWebServiceCallServiceConfig, serializationService, logger)
        {
            _queueSubscriptionService = queueSubscriptionService;
        }

        public void Subscribe()
        {
            _queueSubscriptionService.Subscribe((dto, props, token) => ProcessMessageAsync(dto, token), CancellationToken.None);
        }

        private async Task<IQueueCallbackResult> ProcessMessageAsync(T jobStatusDto, CancellationToken cancellationToken)
        {
            try
            {
                await SendStatusAsync(jobStatusDto.JobId, jobStatusDto.JobStatus, cancellationToken, jobStatusDto.NumberOfLearners);
                return new QueueCallbackResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to post job status message", ex);
                return new QueueCallbackResult(false, ex);
            }
        }
    }
}
