using System.Collections.Generic;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public abstract class Scheduler
    {
        //Generates the information needed for the ui to display the scheduler results.
        public abstract SchedulerResult Run(List<ProcessItem> processes);
    }
}