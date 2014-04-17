using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models
{
    public class processItem
    {
        public string name { get; set; }
        public int[] BurstArray { get; set; }
        public int arrivalTime { get; set; }
    }
}