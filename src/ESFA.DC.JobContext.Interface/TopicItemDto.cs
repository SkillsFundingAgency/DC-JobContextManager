using System.Collections.Generic;

namespace ESFA.DC.JobContext.Interface
{
    public sealed class TopicItemDto
    {
        public string SubscriptionName { get; set; }

        public List<TaskItemDto> Tasks { get; set; }
    }
}
