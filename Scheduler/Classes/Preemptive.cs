using System;
using System.Collections.Generic;
using System.Web;
using Scheduler.Models;
using System.Linq;


namespace Scheduler.Classes
{
    public class Preemptive
    {

        public SchedulerResult Run(List<ProcessItem> processes)
        {
            //sort
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));
            
            //return lists
            var cpuproc = new List<Process>();
            var ioproc = new List<Process>();

            //arrays to keep track of wait and turnaround times
            var turnarounds = new int[processes.Count];
            var waits = new int[processes.Count];

            //create separate entry list
            var entrylist = new List<ProcessItem>();
            foreach(ProcessItem proc in processes){
                entrylist.Add(proc);
            }

            //ready queues
            var cpuready = new List<ProcessItem>();
            var ioready = new List<ProcessItem>();

            //initialize
            cpuready.Add(processes.First<ProcessItem>());
            processes.RemoveAt(0);
            int clock = processes[0].ArrivalTime - 1;

            while (ioready.Count != 0 || cpuready.Count != 0)
            {
                clock++;

            }

            return new SchedulerResult();
        }
    }
}