using System.Collections.Generic;

namespace ESFA.DC.JobContextManager.Model.Interface
{
    public interface ITaskItem
    {
        IReadOnlyList<string> Tasks { get; set; }

        bool SupportsParallelExecution { get; set; }
    }
}
