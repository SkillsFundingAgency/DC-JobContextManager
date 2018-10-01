using System.Collections.Generic;

namespace ESFA.DC.JobContextManager.Model.Interface
{
    public interface ITopicItem
    {
        string SubscriptionName { get; }

        IReadOnlyList<ITaskItem> Tasks { get; }
    }
}
