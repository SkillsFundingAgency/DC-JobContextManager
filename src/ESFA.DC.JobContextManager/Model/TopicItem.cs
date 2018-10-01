using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;
using ITaskItem = ESFA.DC.JobContextManager.Model.Interface.ITaskItem;
using ITopicItem = ESFA.DC.JobContextManager.Model.Interface.ITopicItem;

namespace ESFA.DC.JobContextManager.Model
{
    public sealed class TopicItem : ITopicItem
    {
        public TopicItem()
        {
        }

        public TopicItem(string subscriptionName, IReadOnlyList<ITaskItem> tasks)
        {
            SubscriptionName = subscriptionName;
            Tasks = tasks;
        }

        public string SubscriptionName { get; set; }

        public string SubscriptionSqlFilterValue { get; set; }

        public IReadOnlyList<ITaskItem> Tasks { get; set; }
    }
}
