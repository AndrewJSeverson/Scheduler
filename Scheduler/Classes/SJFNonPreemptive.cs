using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public struct KodyProcessItem
    {
        public ProcessItem process { get; set; }
        public int burstArrayIndex { get; set; }
    }
    /// <summary>
    /// Takes in a list of processes and processes them with a shortest job first, non-preemtive algorithm.
    /// </summary>
    public class SJFNonPreemtive : Scheduler
    {
        public override SchedulerResult Run(List<ProcessItem> processes)
        {
            Dictionary<string, int> processorsWaitTimes = new Dictionary<string, int>();

            var queue = new List<KodyProcessItem>();
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));

            //Return list of cpu processes
            var cpuProcesses = new List<Process>();
            //Return list of io processes
            var ioProcesses = new List<Process>();

            //Add first process to list of current processes, since we have them ordered by arrival time
            int currentTime, cpuTime, ioTime, waitingTime;
            currentTime = cpuTime = ioTime = waitingTime = 0;
            queue = processes.Select(l => new KodyProcessItem
            {
                process = l,
                burstArrayIndex = 0
            }).ToList();

            while (queue.Count > 0)
            {
                //List of cpus/ios available to add to "queue"
                var availableCPUs = new List<Process>();
                var availableIOs = new List<Process>();

                var noneArrived = true;
                for (int i = 0; i < queue.Count; i++)
                {
                    int cur = queue[i].burstArrayIndex;
                    int arrivalTime = queue[i].process.ArrivalTime;
                    //If this process hasn't arrived, skip it
                    if (arrivalTime > currentTime)
                    {
                        continue;
                    }
                    //At least one has arrived
                    noneArrived = false;

                    if (cur%2 == 0) //It's a CPU process
                    {
                        //We only care about a CPU burst if it's up to/before current time
                        if (cpuTime <= currentTime)
                        {
                            availableCPUs.Add(
                                new Process
                                    {
                                        Name = queue[i].process.Name,
                                        StartTime = currentTime,
                                        Duration = queue[i].process.BurstArray[cur],
                                        ProcessIndex = i
                                    });
                        }
                    }
                    else //It's an IO process
                    {
                        if (ioTime <= currentTime)
                        {
                            availableIOs.Add(
                                new Process
                                {
                                    Name = queue[i].process.Name,
                                    StartTime = currentTime,
                                    Duration = queue[i].process.BurstArray[cur],
                                    ProcessIndex = i
                                });
                        }
                    }
                }

                //No process bursts have arrived, so we'll move currentTime to the next available arrival time, then restart the while!
                if (noneArrived)
                {
                    //Grab burst with next arrival time
                    KodyProcessItem next = queue[0];
                    for (int i = 1; i < queue.Count; i++)
                    {
                        if ( queue[i].process.ArrivalTime < next.process.ArrivalTime)
                        {
                            next = queue[i];
                        }
                    }
                    //Add to CPU wait time
                    waitingTime += (next.process.ArrivalTime - cpuTime);
                    //Set current time to the next arrival time
                    currentTime = next.process.ArrivalTime;
                    continue;
                }

                bool cpuWaiting = false;
                if (availableCPUs.Count > 0)
                {
                    //Sort bursts that are available
                    availableCPUs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    //Add burst with shortest time to CPU time and remove the burst from the availableCPUs
                    Process first = availableCPUs.First();
                    cpuProcesses.Add(first);
                    cpuTime = first.StartTime + first.Duration;
                    availableCPUs.RemoveAt(0);

                    //Need to update burst index and arrival time, need to use a temp of the structure because you can't directly modify a structure in an array (not sure why?)
                    int processIndex = first.ProcessIndex;
                    KodyProcessItem temp = queue[processIndex];
                    temp.burstArrayIndex++;
                    temp.process.ArrivalTime = temp.process.ArrivalTime + first.Duration;
                    queue[processIndex] = temp;
                    //If this was the last burst for this process, remove it from our queue
                    if (temp.burstArrayIndex >= queue[processIndex].process.BurstArray.Length)
                    {
                        queue.RemoveAt(processIndex);
                    }
                    //Increment each processes wait time that's still waiting in the available cpus
                    foreach (Process p in availableCPUs)
                    {
                        if (processorsWaitTimes.ContainsKey(p.Name))
                        {
                            processorsWaitTimes[p.Name] += first.Duration;
                        }
                        else
                        {
                            processorsWaitTimes.Add(p.Name, first.Duration);
                        }
                    }
                }
                else//Nothing to put on CPU, so it's waiting
                {
                    cpuWaiting = true;
                }

                //If any IO bursts are available, sort them and add the shortest one to our lists
                bool ioWaiting = false;
                if (availableIOs.Count > 0)
                {
                    availableIOs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    Process first = availableIOs.First();
                    ioProcesses.Add(first);
                    ioTime = first.StartTime + first.Duration;
                    availableIOs.RemoveAt(0);

                    //Need to update burst index and arrival time, need to use a temp of the structure because you can't directly modify a structure in an array (not sure why?)
                    int processIndex = first.ProcessIndex;
                    //int processIndex = queue.First(p => p.process.Name.Equals(first.Name)).burstArrayIndex;
                    KodyProcessItem temp = queue[processIndex];
                    temp.burstArrayIndex++;
                    temp.process.ArrivalTime = temp.process.ArrivalTime + first.Duration;
                    queue[processIndex] = temp;

                    //TODO If we care about single processes IO wait time, we would iterate over the rest of "availableIOs" and add to their wait times here
                }
                else//Nothing to put on IO, so it's waiting
                {
                    ioWaiting = true;
                }
                
                if (cpuWaiting)
                {
                    waitingTime += ioTime - cpuTime; //Add to total wait time of the CPU
                    cpuTime = ioTime;//Because IO moved time forward and we added wait time to CPU
                }else if (ioWaiting)
                {
                    //TODO if we care about IO wait time, we would track that here
                    ioTime = cpuTime;//Because CPU moved time forward
                }

                //Set current time to minimum of cpu and io time
                currentTime = cpuTime > ioTime ? ioTime : cpuTime; 
            }


            /*
             CALCULATE STATS AND RETURN 'EM
            */

            //Grab processes wait times from my process data and put it into the dictionary....Kim already had this method setup, so bleh
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
                AverageTurnAroundTime = 0.0,//((double)turnAroundTime) / numProcesses, each process end - start time
                CpuUtilization =0.0,//((double)currentTime - waitingTime) / currentTime,
                AverageWaitingTime = ((double)waitingTime) / numProcesses,
                ProcessWaitTimes = processorsWaitTimes
            };
        }
    }
}