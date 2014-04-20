using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Models;

namespace Scheduler.Classes
{
    /// <summary>
    /// Takes in a list of processes and processes them with a shortest job first, non-preemtive algorithm.
    /// </summary>
    public class SJFNonPreemtive : Scheduler
    {
        public override SchedulerResult Run(List<ProcessItem> processes)
        {
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));

            //Return list of cpu processes
            var cpuProcesses = new List<Process>();
            //Return list of io processes
            var ioProcesses = new List<Process>();

            //Tracks various data about processes
            var processData = new int[processes.Count, 4];
            for (int i = 0; i < processData.Length; i++)
            {
                processData[i, 0] = 0; //Current index in im on in a process
                processData[i, 1] = processes[i].BurstArray.Length-1; //Max index of a process
                processData[i, 2] = 0; //Wait time of a particular process
            }

            //Add first process to list of current processes, since we have them ordered by arrival time
            int currentTime, cpuTime, ioTime, waitingTime;
            currentTime = cpuTime = ioTime = waitingTime= processes.First().ArrivalTime;

            while (processes.Count > 0)
            {
                //List of cpus/ios available to add to "queue"
                var availableCPUs = new List<Process>();
                var availableIOs = new List<Process>();

                for (int i = 0; i < processes.Count; i++)
                {
                    int cur = processData[i, 0];
                    //CPU burst, even indexes, processes has to have arrived
                    if (cur % 2 == 0 && processes[i].ArrivalTime <= currentTime && cpuTime <= currentTime)
                    {
                        availableCPUs.Add(
                            new Process
                            {
                                Name = processes[i].Name,
                                StartTime = cpuTime,
                                Duration = processes[i].BurstArray[cur],
                                ProcessIndex = i
                            }
                        );
                    }
                    //IO burst, odd indxes, process has to have arrived
                    else if (processes[i].ArrivalTime <= currentTime && ioTime <= currentTime)
                    {
                        availableIOs.Add(
                            new Process
                            {
                                Name = processes[i].Name,
                                StartTime = ioTime,
                                Duration = processes[i].BurstArray[cur],
                                ProcessIndex = i
                            }
                        );
                    }
                    //Increase position in process burst array
                    processData[i, 0]++;
                    //Check if you've reached the last "burst" of this process, if so remove it
                    int end = processData[i, 1];
                    if (cur == end)
                    {
                        processes.RemoveAt(i);
                    }
                }

                //If any CPU bursts are available, sort them and add the shortest one to our lists
                bool cpuWaiting = false;
                if (availableCPUs.Count > 0)
                {
                    availableCPUs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    Process first = availableCPUs.First();
                    cpuProcesses.Add(first);
                    cpuTime = (first.StartTime + first.Duration);
                    availableCPUs.RemoveAt(0);

                    //If any remain, we have to iterate over them and add to their wait times
                    if (availableCPUs.Count > 0)
                    {
                        foreach (var p in availableCPUs)
                        {
                            //Added wait time is same as what we added to cpuTime
                            processData[p.ProcessIndex, 2] += first.Duration;
                        }
                    }
                }
                else
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

                    //If any remain, we have to iterate over them and add to their wait times
                    if (availableIOs.Count > 0)
                    {
                        foreach (var p in availableIOs)
                        {
                            //Added wait time is same as what we added to cpuTime
                            processData[p.ProcessIndex, 2] += first.Duration;
                        }
                    }
                }
                //Nothing to put on IO, so it's waiting
                else
                {
                    ioWaiting = true;
                }
                
                if (cpuWaiting && ioWaiting)//TODO need to commit
                {
                    //TODO currently unhandled..and it's a rare case. I presume I would have to simply "reset" all the variables to the next closest IO/CPU
                }else if (cpuWaiting)
                {
                    waitingTime += ioTime - cpuTime; //TODO this logic may be bad? Not sure if CPU can be waiting
                    cpuTime = ioTime;//Because IO pushed time forward
                }else if (ioWaiting)
                {
                    ioTime = cpuTime;//Because CPU pushed time forward
                }

                //Set current time to minimum of cpu and io time
                currentTime = cpuTime > ioTime ? ioTime : cpuTime; 
            }


            /*
             CALCULATE STATS AND RETURN 'EM
            */

            //Grab processes wait times from my process data and put it into the dictionary....Kim already had this method setup, so bleh
            var processorsWaitTimes = new Dictionary<string, int>();
            for (int i = 0; i < processData.Length; i++)
            {
                processorsWaitTimes.Add(processes[i].Name, processData[i, 2]);
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