using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Models;

namespace SchedulerOS.Classes
{
    /// <summary>
    /// Takes in a list of processes and processes them with a shortest job first, non-preemtive algorithm.
    /// </summary>
    public class SJFNonPreemtive
    {
        //private readonly Queue<processItem> _processes;
        public SJFNonPreemtive(List<processItem> processList)
        {
            //Sort provided process list by arrival time and add it to queue
            processList.Sort((x, y) => x.arrivalTime.CompareTo(y.arrivalTime));
            //_processes = new Queue<processItem>(processList);
	    }


        public void Run(){
            var cpuPieces = new List<processItem>();
            var ioPieces = new List<processItem>();


            int currentTime = 0;



        }
    }
}