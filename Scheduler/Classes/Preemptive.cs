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

           //array to track which burst each process is working on
            var burstindex = new int[processes.Count];
            for (int i = 0; i < processes.Count; i++){
                burstindex[i] = 0;
            }

            //create initial entry list
            var entrylist = new List<Process>();
            for (int i = 0; i < processes.Count; i++){
                entrylist.Add(
                    new Process{
                        Name = processes[i].Name,
                        StartTime = processes[i].ArrivalTime,
                        Duration = processes[i].BurstArray[0],
                        ProcessIndex = i
                    }
                );
            }

            //ready queues
            var cpuready = new List<Process>();
            var ioready = new List<Process>();

            //setup for loop
            Process currentcpuproc = null;
            Process currentioproc = null;
            entrylist.RemoveAt(0);
            int clock = currentcpuproc.StartTime;

            while (ioready.Count != 0 || cpuready.Count != 0 || entrylist.Count != 0)
            {
                clock++;
                bool preempt = false;

                //if a process arrives fresh to the cpu
                if (entrylist[0].StartTime == clock)
                {
                    preempt = true;
                    cpuready.Add(entrylist[0]);
                    entrylist.RemoveAt(0);
                }

                //if the current cpu process ends
                if (currentcpuproc.Duration + currentcpuproc.StartTime == clock)
                {
                    preempt = true;
                    burstindex[currentcpuproc.ProcessIndex]++;
                    var tempint = currentcpuproc.ProcessIndex;
                    if(burstindex[tempint] < processes[tempint].BurstArray.Count())
                        ioready.Add(
                            new Process{
                                Name = currentcpuproc.Name,
                                StartTime = clock,
                                Duration = processes[tempint].BurstArray[burstindex[tempint]],
                                ProcessIndex = currentcpuproc.ProcessIndex
                            }
                        );
                    cpuproc.Add(currentcpuproc);
                    currentcpuproc = null;
                }
                //if the current io process ends
                if (currentioproc != null && currentioproc.Duration + currentioproc.StartTime == clock)
                {
                    preempt = true;
                    burstindex[currentioproc.ProcessIndex]++;
                    var tempint = currentioproc.ProcessIndex;
                    cpuready.Add(
                        new Process{
                            Name = currentioproc.Name,
                            StartTime = clock,
                            Duration = processes[tempint].BurstArray[burstindex[tempint]]
                        }
                    );
                    ioproc.Add(currentioproc);
                    currentioproc = null;
                }

                //handling preemptions and stuff
                if (preempt)
                {
                    //TODO
                }

                if (currentioproc == null){
                    currentioproc = ioready[0];
                    ioready.RemoveAt(0);
                }
            }

            return new SchedulerResult{
                SchedulerStats = null, //TODO
                CpuProcesses = cpuproc,
                IoProcesses = ioproc
            };
        }

        private SchedulerStats calculateStats(int numProcesses, int cpuWaitingTime, int processWaitTime, int currentTime, int turnAround, Dictionary<string, int> processorsWaitTimes, Dictionary<string, double> processTurnAroundTimes)
        {
            return new SchedulerStats
            {
                AverageTurnAroundTime = ((double)turnAround) / numProcesses,
                CpuUtilization = ((double)currentTime - cpuWaitingTime) / currentTime,
                AverageWaitingTime = ((double)processWaitTime) / numProcesses,
                ProcessWaitTimes = processorsWaitTimes,
                ProcessTurnAroundTimes = processTurnAroundTimes
            };
        }
    }
}