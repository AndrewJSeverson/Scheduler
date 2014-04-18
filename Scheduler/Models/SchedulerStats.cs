namespace Scheduler.Models
{
    public class SchedulerStats
    {
        public decimal CpuUtilization { get; set; }

        public decimal WaitingTime { get; set; }

        public decimal AverageWaitingTime { get; set; }

        public decimal MeanTurnAroundTime { get; set; }

        public decimal NormailizedTurnAroundTime { get; set; }
    }
}