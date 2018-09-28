using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.JobContext
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

        public IReadOnlyList<ITaskItem> Tasks { get; set; }

        public string SubscriptionSqlFilterValue => throw new System.NotImplementedException();
    }
}
