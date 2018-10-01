using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;
using ITaskItem = ESFA.DC.JobContextManager.Model.Interface.ITaskItem;

namespace ESFA.DC.JobContextManager.Model
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
