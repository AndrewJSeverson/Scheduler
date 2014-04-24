using System;

namespace Scheduler.Models
{
    [Serializable]
    public class ProcessItem
    {
        public string Name { get; set; }
        public int[] BurstArray { get; set; }
        public int ArrivalTime { get; set; }
    }
}