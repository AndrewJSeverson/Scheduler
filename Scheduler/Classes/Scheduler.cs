using System.Collections.Generic;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public abstract class Scheduler
    {
        public SchedulerStats SchedulerStats { get; set; }

        public abstract List<Process> Run();
    }
}