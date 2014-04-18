using System.Collections.Generic;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public class SchedulerResult
    {
        //List of processes scheduled on the CPU (in order! lowest time first)
        public List<Process> CpuProcesses { get; set; }

        //List of processes scheduled on the ID device (in order! lowest time first)
        public List<Process> IoProcesses { get; set; }

        //Statistics about the CPU using a given scheduling method
        public SchedulerStats SchedulerStats { get; set; }
    }
}