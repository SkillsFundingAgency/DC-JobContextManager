using System.Collections.Generic;

namespace ESFA.DC.JobContext
{
    public sealed class TopicItemDto
    {
        public string SubscriptionName { get; set; }

        public string SubscriptionSqlFilterValue { get; set; }

        public List<TaskItemDto> Tasks { get; set; }
    }
}
