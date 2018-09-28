using System.Collections.Generic;

namespace ESFA.DC.JobContext.Interface
{
    public interface ITopicItem
    {
        string SubscriptionName { get; }

        IReadOnlyList<ITaskItem> Tasks { get; }
    }
}
