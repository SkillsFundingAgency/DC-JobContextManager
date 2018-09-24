using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.JobStatus.WebServiceCall.Service
{
    public class BaseWebServiceCallService
    {
        protected readonly ILogger _logger;

        private readonly ISerializationService _serializationService;

        private readonly string _endPointUrl;

        private readonly HttpClient client = new HttpClient();

        protected BaseWebServiceCallService(
            IJobStatusWebServiceCallServiceConfig jobStatusWebServiceCallServiceConfig,
            ISerializationService serializationService,
            ILogger logger)
        {
            _serializationService = serializationService;
            _logger = logger;
            _endPointUrl = jobStatusWebServiceCallServiceConfig.EndPointUrl;
            _endPointUrl = !_endPointUrl.EndsWith("/") ? $"{_endPointUrl}/Job/Status" : $"{_endPointUrl}Job/Status";
        }

        protected async Task SendStatusAsync(long jobId, int status, CancellationToken cancellationToken, int numOfLearners = -1)
        {
            await client.PostAsync(
                _endPointUrl,
                new StringContent(_serializationService.Serialize(new JobStatusDto(jobId, status, numOfLearners)), Encoding.UTF8, "application/json"),
                cancellationToken);
        }
    }
}
