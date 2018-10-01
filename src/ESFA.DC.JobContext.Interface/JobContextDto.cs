using System;
using System.Collections.Generic;

namespace ESFA.DC.JobContext.Interface
{
    public sealed class JobContextDto
    {
        public long JobId { get; set; }

        public DateTime SubmissionDateTimeUtc { get; set; }

        public List<TopicItemDto> Topics { get; set; }

        public int TopicPointer { get; set; }

        public Dictionary<string, object> KeyValuePairs { get; set; }
    }
}
