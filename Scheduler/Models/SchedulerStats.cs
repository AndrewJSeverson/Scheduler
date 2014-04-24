using System.Collections.Generic;

namespace Scheduler.Models
{
    public class SchedulerStats
    {
        public double CpuUtilization { get; set; }

        public double AverageWaitingTime { get; set; }

        public double AverageTurnAroundTime { get; set; }

        public Dictionary<string, int> ProcessWaitTimes { get; set; }
        public Dictionary<string, double> ProcessTurnAroundTimes { get; set; }
    }
}