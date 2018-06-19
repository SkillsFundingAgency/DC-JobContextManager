using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace DC.JobContextManager.Interface
{
    public interface IJobContextManager<T> : ICommunicationListener
        where T : class
    {
    }
}
