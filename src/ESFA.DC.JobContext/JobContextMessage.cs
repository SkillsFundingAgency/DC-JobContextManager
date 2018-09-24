using System;
using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.JobContext
{
    /// <summary>
    /// Job Context Message for use on Azure Service Bus. Must be serialisable.
    /// </summary>
    public sealed class JobContextMessage : IJobContextMessage
    {
        public JobContextMessage()
        {
        }

        public JobContextMessage(long jobId, IReadOnlyList<ITopicItem> topics, int topicPointer = 0, DateTime? submissionDateTime = null)
        {
            JobId = jobId;
            SubmissionDateTimeUtc = submissionDateTime ?? DateTime.UtcNow;
            Topics = topics;
            TopicPointer = topicPointer;
            KeyValuePairs = new Dictionary<string, object>();
        }

        public JobContextMessage(
            long jobId,
            IReadOnlyList<ITopicItem> topics,
            string ukPrn,
            string container,
            string filename,
            string username,
            int topicPointer = 0,
            DateTime? submissionDateTime = null)
            : this(jobId, topics, topicPointer, submissionDateTime)
        {
            KeyValuePairs[JobContextMessageKey.UkPrn] = ukPrn;
            KeyValuePairs[JobContextMessageKey.Container] = container;
            KeyValuePairs[JobContextMessageKey.Filename] = filename;
            KeyValuePairs[JobContextMessageKey.Username] = username;
        }

        public long JobId { get; set; }

        public DateTime SubmissionDateTimeUtc { get; set; }

        public IReadOnlyList<ITopicItem> Topics { get; set; }

        public int TopicPointer { get; set; }

        public IDictionary<string, object> KeyValuePairs { get; set; }
    }
}
