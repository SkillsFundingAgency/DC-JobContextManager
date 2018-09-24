using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.JobContext.Interface
{
    public interface ITaskItem
    {
        IReadOnlyList<string> Tasks { get; set; }

        bool SupportsParallelExecution { get; set; }
    }
}
