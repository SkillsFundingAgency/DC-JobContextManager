using System;
using System.Collections.Generic;

namespace ESFA.DC.JobContext.Interface
{
    public interface IJobContextMessage
    {
        long JobId { get; }

        DateTime SubmissionDateTimeUtc { get; }

        IReadOnlyList<ITopicItem> Topics { get; }

        int TopicPointer { get; }

        IDictionary<string, object> KeyValuePairs { get; }
    }
}
