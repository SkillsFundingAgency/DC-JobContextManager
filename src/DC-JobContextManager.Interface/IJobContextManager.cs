using System.Threading.Tasks;
using ESFA.DC.JobContext.Interface;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace DC.JobContextManager.Interface
{
    public interface IJobContextManager : ICommunicationListener
    {
        Task FinishSuccessfully(IJobContextMessage jobContextMessage);

        Task FinishError(IJobContextMessage jobContextMessage);
    }
}
