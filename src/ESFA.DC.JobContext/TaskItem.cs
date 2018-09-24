using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.JobContext
{
    public sealed class TaskItem : ITaskItem
    {
        public TaskItem()
        {
        }

        public TaskItem(IReadOnlyList<string> tasks, bool supportsParallelExecution)
        {
            Tasks = tasks;
            SupportsParallelExecution = supportsParallelExecution;
        }

        public IReadOnlyList<string> Tasks { get; set; }

        public bool SupportsParallelExecution { get; set; }
    }
}
