using System.Collections.Generic;

namespace ESFA.DC.JobContext.Interface
{
    public sealed class TaskItemDto
    {
        public List<string> Tasks { get; set; }

        public bool SupportsParallelExecution { get; set; }
    }
}
