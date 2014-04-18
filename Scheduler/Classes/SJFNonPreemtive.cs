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

            //Tracks which process index i'm on for each process. Not sure if i'll need this yet
            var processData = new int[processes.Count, 2];
            for (int i = 0; i < processData.Length; i++)
            {
                processData[i, 0] = 0;
                processData[i, 1] = processes[i].BurstArray.Length-1;
            }

            //Add first process to list of current processes, since we have them ordered by arrival time
            List<ProcessItem> currentProcesses = new List<ProcessItem> { processes.First() };
            int currentTime, cpuTime, ioTime, waitingTime;
            currentTime = cpuTime = ioTime = processes.First().ArrivalTime;
            waitingTime = 0;
            processes.RemoveAt(0);

            while (currentProcesses.Count > 0)
            {
                //Check if any other processes have arrived, if so add them to currentProcesses
                for (int i = 0; i < processes.Count; i++)
                {
                    if (processes[i].ArrivalTime <= currentTime)
                    {
                        currentProcesses.Add(processes[i]);
                        processes.RemoveAt(i);
                    }
                }

                //List of cpus/ios available to add to "queue"
                var availableCPUs = new List<Process>();
                var availableIOs = new List<Process>();
                for (int i = 0; i < currentProcesses.Count; i++)
                {
                    int cur = processData[i, 0];
                    //CPU burst
                    if (cur % 2 == 0 && cpuTime <= currentTime)
                    {
                        availableCPUs.Add(
                            new Process
                            {
                                Name = currentProcesses[i].Name,
                                StartTime = cpuTime,
                                Duration = currentProcesses[i].BurstArray[cur]
                            }
                        );
                    }
                    //IO burst
                    else if (ioTime <= currentTime)
                    {
                        availableIOs.Add(
                            new Process
                            {
                                Name = currentProcesses[i].Name,
                                StartTime = ioTime,
                                Duration = currentProcesses[i].BurstArray[cur]
                            }
                        );
                    }
                    //Increase position in process burst array
                    processData[i, 0]++;
                    //Check if you've reached the last "burst" of this process, if so remove it
                    int end = processData[i, 1];
                    if ((cur+1) > end)//+1 because cur is 1 less then the "updated" value
                    {
                        currentProcesses.RemoveAt(i);
                    }
                }

                //If any are available, sort them and add the shortest one to our lists
                if (availableCPUs.Count > 0 || availableIOs.Count > 0)
                {
                    availableCPUs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    Process first = availableCPUs.First();
                    cpuProcesses.Add(first);
                    cpuTime = first.StartTime + first.Duration;
                }
                //Nothing to put on CPU, so it's waiting
                else
                {
                    
                }
                if (availableIOs.Count > 0)
                {
                    availableIOs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    Process first = availableIOs.First();
                    ioProcesses.Add(first);
                    ioTime = first.StartTime + first.Duration;
                }
                //Nothing to put on IO, so it's waiting
                else
                {
                    
                }

                //Set current time to minimum of cpu and io time
                currentTime = cpuTime > ioTime ? ioTime : cpuTime; 

                //TODO Need to calculate wait time somewhere around here
                //TODO Need to handle following case (e.g. current time got to 8, and current io is sitting at 10, but there's no more CPUs to process until some IOs are. In this case "wait time" will be added, and current time will be brought to the io time).
            }

            return new SchedulerResult
            {
                CpuProcesses = cpuProcesses,
                IoProcesses = ioProcesses
                //SchedulerStats = calculateStats(processes.Count)
            };
        }
    }
}