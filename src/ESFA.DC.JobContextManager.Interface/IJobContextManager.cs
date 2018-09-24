using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace ESFA.DC.JobContextManager.Interface
{
    public interface IJobContextManager<T> : ICommunicationListener
        where T : class
    {
    }
}
