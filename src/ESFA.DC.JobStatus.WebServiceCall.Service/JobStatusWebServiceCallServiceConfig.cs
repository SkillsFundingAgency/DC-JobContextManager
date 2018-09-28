using ESFA.DC.JobStatus.Interface;
using IJobStatusWebServiceCallServiceConfig = ESFA.DC.JobStatus.WebServiceCall.Service.Interface.IJobStatusWebServiceCallServiceConfig;

namespace ESFA.DC.JobStatus.WebServiceCall.Service
{
    public sealed class JobStatusWebServiceCallServiceConfig : IJobStatusWebServiceCallServiceConfig
    {
        public JobStatusWebServiceCallServiceConfig(string endPointUrl)
        {
            EndPointUrl = endPointUrl;
        }

        public string EndPointUrl { get; }
    }
}
