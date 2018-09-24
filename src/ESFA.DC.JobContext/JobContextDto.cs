using System;
using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.JobContext
{
    public sealed class JobContextDto
    {
        public JobContextDto()
        {
        }

        public long JobId { get; set; }

        public DateTime SubmissionDateTimeUtc { get; set; }

        public List<TopicItemDto> Topics { get; set; }

        public int TopicPointer { get; set; }

        public Dictionary<string, object> KeyValuePairs { get; set; }
    }
}
