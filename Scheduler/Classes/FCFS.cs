using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Models;

// I blatantly stole this from Kody. The FCFS logic was already there, except you don't need to reorder the processes/
// based on job length. I removed those sections, so this should be generic FCFS. 

namespace Scheduler.Classes
{
    public class FCFS : Scheduler
    {
        public override SchedulerResult Run(List<Models.ProcessItem> processes)
        {
            int[] waitTimes = new int[processes.Count];
            int[] arrivalTimes = new int[processes.Count];
            for (int i = 0; i < processes.Count; i++)
            {
                arrivalTimes[i] = processes[i].ArrivalTime;
            }

            // Sort the processes in order of arrival time.
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));

            List<Models.ProcessItem> copyProcesses = processes;

            // List of CPU Processes
            var cpuProcesses = new List<Process>();

            // List of IO Processes
            var ioProcesses = new List<Process>();

            // Information about where we are when accessing process data, etc. 
            var processData = new int[processes.Count,4];
            for (int i = 0; i < processes.Count; i++)
            {
                processData[i, 0] = 0;  // Current index of 'processes'
                processData[i, 1] = processes[i].BurstArray.Length - 1; // Maximum length of the burst array for a particular process
                processData[i, 2] = 0; // Current burst array index of the particular process we're looking at.
                processData[i, 3] = 1;
            }

            var availableCPUs = new List<Process>();
            var availableIOs = new List<Process>();

            int currentTime = 0;
            int ioTime = 0;
            int waitingTime = 0;
            int cpuTime = 0;
            bool finished = false;
            while (!finished)
            {
                // Loop through the processes and create new things from them
                for (int i = 0; i < processes.Count; i++)
                {
                    // Sometimes we're done with processes and we don't need to get more from them. 
                    if (processData[i, 3] == 0)
                    {
                        continue;
                    }
                    int currentProc = processData[i, 0];
                    int currentBurstIndex = processData[i, 2];

                    if (currentBurstIndex%2 == 0)
                    {
                        availableCPUs.Add(
                            new Process
                                {
                                    Name = processes[i].Name,
                                    Duration = processes[i].BurstArray[currentBurstIndex],
                                    ProcessIndex = i,
                                    StartTime = cpuTime
                                }
                            );
                    }


                    if (currentBurstIndex%2 > 0)
                    {
                        availableIOs.Add(
                            new Process
                                {
                                    Name = processes[i].Name,
                                    Duration = processes[i].BurstArray[currentBurstIndex],
                                    ProcessIndex = i,
                                    StartTime = cpuTime
                                });
                    }


                    // Increment the current burst index for the process we're looking at
                    currentBurstIndex = currentBurstIndex+1;

                    // If the current burst index is larger than the max burst index, then we're done, pull it from the processes list.
                    if (currentBurstIndex > processData[i, 1])
                    {
                        processData[i, 3] = 0;
                    }
                    else
                    {
                        processData[i, 2] = currentBurstIndex;
                    }
                }
                finished = true;
                for (int j = 0; j < processes.Count; j++)
                {
                    if (processData[j, 3] == 1)
                    {
                        finished = false;
                    }
                }
            }
            
            // At this point, we should have two queues of processes, CPU and IO.
            // They are ordered in an FCFS fashion.
            // Now, we need to run through and schedule them.

            // We need to increment the waiting, current, io and cpu times 
            // to whatever the arrival time of the first process is, 
            // since we're just going to be sitting around waiting.
            int arrivalTime = processes.Min(x => x.ArrivalTime);
            cpuTime = waitingTime = ioTime = currentTime = arrivalTime;

            bool cpuWaiting = false;
            bool ioWaiting = false;

            int cpuWait = 0;
            int ioWait = 0;

            // While there is still work to be done in BOTH queues
            while (availableCPUs.Count > 0 && availableIOs.Count > 0)
            {
                // Grab the first two processes off the list.
                Process CPUProc = availableCPUs.First();
                Process IOProc;
                try
                {
                    IOProc = availableIOs.First(x => x.ProcessIndex != CPUProc.ProcessIndex);
                        // Grab the first IO Process that is not the same as the CPU process
                }
                catch (InvalidOperationException e)
                {
                    // If we get here, that means there doesn't exist an IO operation that is not the same as a CPU opeation.
                    // So.... we're waiting.
                    ioWaiting = true;
                    IOProc = null;
                }


                // Process CPU if and only if the process has already arrived. 
                if (processes[CPUProc.ProcessIndex].ArrivalTime >= arrivalTime && cpuTime == currentTime)
                {
                    cpuTime = cpuTime + CPUProc.Duration;
                    cpuProcesses.Add(CPUProc);
                    availableCPUs.RemoveAt(availableCPUs.IndexOf(CPUProc));
                }
                else
                {
                    cpuWaiting = true;
                }

                // Process IO if and only if the process has already arrived. 
                if (!ioWaiting && IOProc != null && processes[IOProc.ProcessIndex].ArrivalTime >= arrivalTime && ioTime == currentTime)
                {
                    ioTime = ioTime + IOProc.Duration;
                    ioProcesses.Add(IOProc);
                    availableIOs.RemoveAt(availableIOs.IndexOf(IOProc));
                }
                else
                {
                    ioWaiting = true;
                }



                foreach (ProcessItem x in processes.Where(x => x.ArrivalTime <= arrivalTime))
                {
                    waitTimes[processes.IndexOf(x)] += Math.Min(ioTime, cpuTime) - currentTime;
                }

                // Advance the current time to whatever the shortest process is.
                if (!ioWaiting && !cpuWaiting)
                {
                    currentTime = Math.Min(cpuTime, ioTime);
                }
                else if (ioWaiting && !cpuWaiting)
                {
                    ioWait += cpuTime - currentTime;
                    currentTime = cpuTime;
                } else if (cpuWaiting && !ioWaiting)
                {
                    cpuWait += ioTime - currentTime;
                    currentTime = ioTime;
                }
                else
                {
                    // This is a weird case that I'm not going to handle, because
                    // the CPU and IO should never be waiting at the same time.
                }
            }

            // If there is still processor work to do, do it.
            while (availableCPUs.Count > 0)
            {
                Process CPUProc = availableCPUs.First();
                cpuTime = cpuTime + CPUProc.Duration;
                cpuProcesses.Add(CPUProc);
                availableCPUs.RemoveAt(0);

                ioWait = cpuTime - currentTime;
                currentTime = ioTime = cpuTime;
            }

            while (availableIOs.Count > 0)
            {
                Process IOProc = availableIOs.First();
                ioTime = ioTime + IOProc.Duration;
                ioProcesses.Add(IOProc);
                availableIOs.RemoveAt(0);

                cpuWait = ioTime - currentTime;
                currentTime = cpuTime = ioTime;
            }

            /*
             CALCULATE STATS AND RETURN 'EM
            */

            //Grab processes wait times from my process data and put it into the dictionary....Kim already had this method setup, so bleh
            var processorsWaitTimes = new Dictionary<string, int>();
            for (int i = 0; i < processes.Count; i++)
            {
                processorsWaitTimes.Add(processes[i].Name, waitTimes[i]);
            }

            return new SchedulerResult
            {
                CpuProcesses = cpuProcesses,
                IoProcesses = ioProcesses,
                SchedulerStats = calculateStats(processes.Count, waitingTime, processorsWaitTimes)
            };
        }

        private SchedulerStats calculateStats(int numProcesses, int waitingTime, Dictionary<string, int> processorsWaitTimes)
        {


            return new SchedulerStats
            {
                AverageTurnAroundTime = 0.0,//((double)turnAroundTime) / numProcesses,
                CpuUtilization = 0.0,//((double)currentTime - cpuDownTime) / currentTime,
                AverageWaitingTime = ((double)waitingTime) / numProcesses,
                ProcessWaitTimes = processorsWaitTimes
            };
        }
    }
}