using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduler.Models
{
    public class KodyProcess
    {
        public string Name { get; set; }
        public int StartTime { get; set; }
        public int Duration { get; set; }
    }
}