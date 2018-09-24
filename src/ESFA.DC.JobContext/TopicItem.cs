using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.JobContext
{
    public sealed class TopicItem : ITopicItem
    {
        public TopicItem(string subscriptionName, string subscriptionSqlFilterValue, IReadOnlyList<ITaskItem> tasks)
        {
            SubscriptionName = subscriptionName;
            SubscriptionSqlFilterValue = subscriptionSqlFilterValue;
            Tasks = tasks;
        }

        public string SubscriptionName { get; }

        public string SubscriptionSqlFilterValue { get; }

        public IReadOnlyList<ITaskItem> Tasks { get; set; }
    }
}
