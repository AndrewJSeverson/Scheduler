namespace Scheduler.Models
{
    public class SchedulerStats
    {
        public double CpuUtilization { get; set; }

        public double WaitingTime { get; set; }

        public double AverageWaitingTime { get; set; }

        public double AverageTurnAroundTime { get; set; }

        public double NormailizedTurnAroundTime { get; set; }
    }
}