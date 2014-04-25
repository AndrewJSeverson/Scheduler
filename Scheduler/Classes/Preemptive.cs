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
            for (int i = 0; i < processes.Count; i++){
                turnarounds[i] = 0;
                waits[i] = 0;
            }
            var cpuwait = 0;

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
            int clock = -1;

            while (ioready.Count != 0 || cpuready.Count != 0 || entrylist.Count != 0 || currentioproc != null || currentcpuproc != null)
            {
                clock++;
                bool preempt = false;
                if (currentcpuproc == null || currentioproc == null)
                    preempt = true;

                //if a process arrives fresh to the cpu
                while (entrylist.Count != 0 && entrylist[0].StartTime == clock)
                {
                    preempt = true;
                    cpuready.Add(entrylist[0]);
                    entrylist.RemoveAt(0);
                }

                //if the current cpu process ends
                if (currentcpuproc != null && currentcpuproc.Duration + currentcpuproc.StartTime == clock)
                {
                    preempt = true;
                    var tempint = currentcpuproc.ProcessIndex;
                    burstindex[tempint]++;
                    if (burstindex[tempint] < processes[tempint].BurstArray.Count())
                        ioready.Add(
                            new Process
                            {
                                Name = currentcpuproc.Name,
                                StartTime = clock,
                                Duration = processes[tempint].BurstArray[burstindex[tempint]],
                                ProcessIndex = currentcpuproc.ProcessIndex
                            }
                        );
                    else
                        turnarounds[currentcpuproc.ProcessIndex] = clock - processes[currentcpuproc.ProcessIndex].ArrivalTime;
                    cpuproc.Add(currentcpuproc);
                    currentcpuproc = null;
                }

                //if the current io process ends
                if (currentioproc != null && currentioproc.Duration + currentioproc.StartTime == clock)
                {
                    preempt = true;
                    var tempint = currentioproc.ProcessIndex;
                    burstindex[tempint]++;
                    cpuready.Add(
                        new Process
                        {
                            Name = currentioproc.Name,
                            StartTime = clock,
                            Duration = processes[tempint].BurstArray[burstindex[tempint]],
                            ProcessIndex = currentioproc.ProcessIndex
                        }
                    );
                    ioproc.Add(currentioproc);
                    currentioproc = null;
                }

                //handling preemptions and stuff
                if (preempt)
                {
                    if (currentcpuproc != null)
                    {
                        cpuready.Add(
                            new Process
                            {
                                Name = currentcpuproc.Name,
                                Duration = currentcpuproc.Duration - (clock - currentcpuproc.StartTime),
                                StartTime = clock,
                                ProcessIndex = currentcpuproc.ProcessIndex
                            }
                        );
                        cpuready.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                        if (cpuready[0].ProcessIndex != currentcpuproc.ProcessIndex)
                        {
                            cpuproc.Add(
                                new Process
                                {
                                    Name = currentcpuproc.Name,
                                    StartTime = currentcpuproc.StartTime,
                                    Duration = clock - currentcpuproc.StartTime,
                                    ProcessIndex = currentcpuproc.ProcessIndex
                                }
                            );
                            currentcpuproc = cpuready[0];
                            cpuready.RemoveAt(0);
                        }
                        else
                        {
                            cpuready.RemoveAt(0);
                        }
                    }
                    else if (cpuready.Count != 0)
                    {
                        cpuready.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                        currentcpuproc = cpuready[0];
                        if (currentcpuproc.StartTime < clock)
                        {
                            waits[currentcpuproc.ProcessIndex] += clock - currentcpuproc.StartTime;
                            currentcpuproc.StartTime = clock;
                        }
                        cpuready.RemoveAt(0);
                    }
                    else
                        cpuwait++;
                }

                if (currentioproc == null && ioready.Count != 0){
                    currentioproc = ioready[0];
                    currentioproc.StartTime = clock;
                    ioready.RemoveAt(0);
                }
            }

            var procwait = new Dictionary<string,int>();
            var turnaround = new Dictionary<string,double>();
            for (int i = 0; i < processes.Count; i++){
                procwait.Add(processes[i].Name,waits[i]);
                turnaround.Add(processes[i].Name,(double)turnarounds[i]);
            }

            return new SchedulerResult{
                SchedulerStats = calculateStats(processes.Count,cpuwait,waits.Sum(),clock,turnarounds.Sum(),procwait,turnaround),
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