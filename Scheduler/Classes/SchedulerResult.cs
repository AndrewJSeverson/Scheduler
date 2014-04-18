using System.Collections.Generic;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public class SchedulerResult
    {
        //List of processes scheduled (in order! lowest time first)
        public List<Process> Processes { get; set; } 

        //Statistics about the CPU using a given scheduling method
        public SchedulerStats SchedulerStats { get; set; }

        //List of the orignial process information
        public List<processItem> OriginalProcesses { get; set; } 
    }
}