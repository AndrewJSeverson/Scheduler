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
            for (int i = 0; i < processes.Count; i++) turnarounds[i] = 0;
            var waits = new int[processes.Count];
            for (int i = 0; i < processes.Count; i++) waits[i] = 0;

            //create separate entry list
            var entrylist = new List<ProcessItem>();
            foreach(ProcessItem proc in processes){
                entrylist.Add(proc);
            }

            //ready queues
            var cpuready = new List<ProcessItem>();
            var ioready = new List<ProcessItem>();

            //setup for loop
            var currentcpuproc = entrylist.First();
            ProcessItem currentioproc = null;
            entrylist.RemoveAt(0);
            int clock = currentcpuproc.ArrivalTime;

            while (ioready.Count != 0 || cpuready.Count != 0 || entrylist.Count != 0)
            {
                clock++;
                bool preempt = false;

                //if a process arrives fresh to the cpu
                if (entrylist.First().ArrivalTime == clock)
                {
                    preempt = true;
                    cpuready.Add(entrylist.First());
                }
                //if the current cpu process ends
                if (currentcpuproc.BurstArray[0] + currentcpuproc.ArrivalTime == clock)
                {
                    preempt = true;
                    currentcpuproc = null;
                    cpuproc.Add(
                        new Process
                        {
                            Name = currentcpuproc.Name,
                            StartTime = currentcpuproc.ArrivalTime,
                            Duration = currentcpuproc.BurstArray[0]
                        }
                    );
                }
                //if the current io process ends
                if (currentioproc != null && currentioproc.BurstArray[0] + currentioproc.ArrivalTime == clock)
                {
                    preempt = true;
                    var temp = new ProcessItem
                    {
                        Name = currentioproc.Name,
                        ArrivalTime = clock
                    };
                    currentioproc.BurstArray.CopyTo(temp.BurstArray,1);
                    cpuready.Add(temp);
                    currentioproc = null;
                }

                //handling preemptions and stuff
                if (preempt)
                {
                    
                }
            }
 

            return new SchedulerResult();
        }
    }
}