using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models
{
    public class ProcessItem
    {
        public string Name { get; set; }
        public int[] BurstArray { get; set; }
        public int ArrivalTime { get; set; }
    }
}