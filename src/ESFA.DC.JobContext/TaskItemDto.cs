using System.Collections.Generic;

namespace ESFA.DC.JobContext
{
    public sealed class TaskItemDto
    {
        public TaskItemDto()
        {
        }

        public List<string> Tasks { get; set; }

        public bool SupportsParallelExecution { get; set; }
    }
}
