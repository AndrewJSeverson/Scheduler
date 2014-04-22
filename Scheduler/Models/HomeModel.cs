using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Classes;

namespace Scheduler.Models
{
    public class HomeModel
    {
        public SchedulerResult Feedback { get; set; }
        public SchedulerResult RR { get; set; }
        public SchedulerResult FCFS { get; set; }
        public SchedulerResult SPN { get; set; }
        public SchedulerResult SRT { get; set; }
        public int NumProcess { get; set; }
        public List<ProcessItem> ProcessItems { get; set; }
    }
}