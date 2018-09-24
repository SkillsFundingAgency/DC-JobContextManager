using ESFA.DC.JobStatus.Interface;

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
