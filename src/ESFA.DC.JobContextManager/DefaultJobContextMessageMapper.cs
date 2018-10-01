using ESFA.DC.JobContext;
using ESFA.DC.Mapping.Interface;
using JobContextMessage = ESFA.DC.JobContextManager.Model.JobContextMessage;

namespace ESFA.DC.JobContextManager
{
    public class DefaultJobContextMessageMapper<T> : IMapper<JobContextMessage, T>
        where T : class
    {
        public T MapTo(JobContextMessage value)
        {
            return value as T;
        }

        public JobContextMessage MapFrom(T value)
        {
            return value as JobContextMessage;
        }
    }
}
