﻿using System;
using System.Collections.Generic;
using System.Linq;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public struct KimProcessItem 
    {
        public ProcessItem process { get; set; }
        public int burstArrayIndex { get; set; }
    }

    public class Feedback : Scheduler
    {
        private List<ProcessItem> processItems; 

        //Hard coded queue times based on the assignment description
        private List<int> QueueTimes = new List<int>
            {
                1,
                2,
                4
            };

        //The four queues this implementation of feedback requires
        private List<List<KimProcessItem>> queues = new List<List<KimProcessItem>>
            {
                new List<KimProcessItem>(),
                new List<KimProcessItem>(),
                new List<KimProcessItem>(),
                new List<KimProcessItem>()//This one is first come first serve
            };

        //Return list of cpuProcesses
        private List<Process> cpuProcesses = new List<Process>();
        //Return list of scheduled io processes
        private List<Process> ioProcesses = new List<Process>();

        //The current cpu time
        private int currentTime;
        //The current io time
        private int ioTime;

        //Used to calculate cpu utilization
        private int cpuDownTime;
        //Used to calculate averate wait time
        private int waitingTime;
        //Used to calculate average turnaround time
        private int turnAroundTime;

        private Dictionary<string, int> processorsWaitTimes = new Dictionary<string, int>();

        public override SchedulerResult Run(List<ProcessItem> processes)
        {
            currentTime = 0;
            ioTime = 0;
            cpuDownTime = 0;
            waitingTime = 0;
            
            //Initialize Process Dictonary for the process wait times
            processorsWaitTimes = processes.ToDictionary(p => p.Name, p => 0);

            //Sort the list by arrivial times and pick the first one to start.
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));

            processItems = processes;

            queues[0] = processes.Select(l => new KimProcessItem
                        {
                            process = new ProcessItem()
                                {
                                    ArrivalTime = l.ArrivalTime, 
                                    BurstArray = (int [])l.BurstArray.Clone(),
                                    Name = l.Name
                                },
                            burstArrayIndex = 0
                        }).ToList();

            while (queues[0].Any() || queues[1].Count + queues[2].Count + queues[3].Count > 0)
            {
                foreach (List<KimProcessItem> queue in queues)
                {
                    queue.Sort((x, y) => x.process.ArrivalTime.CompareTo(y.process.ArrivalTime));
                }
                
                if (queues[0].Any(p => p.process.ArrivalTime <= currentTime))
                {
                    this.scheduleProcess(0);
                }
                else if (queues[1].Any(p => p.process.ArrivalTime <= currentTime))
                {
                    this.scheduleProcess(1);
                }
                else if (queues[2].Any(p => p.process.ArrivalTime <= currentTime))
                {
                    this.scheduleProcess(2);
                }
                else if (queues[3].Any(p => p.process.ArrivalTime <= currentTime))
                {
                    this.scheduleProcess(3);
                }
                else
                {
                    //If none of the processes start before the cureent time pick the one that gets there first
                    int minStart = int.MaxValue;
                    int? minIdx = null;
                    for (int i = 0; i < queues.Count; i++)
                    {
                        if (queues[i].Any())
                        {
                            int nextStart = queues[i].First().process.ArrivalTime;
                            if (nextStart <= currentTime && minStart > nextStart)
                            {
                                minStart = nextStart;
                                minIdx = i;
                            }
                        }
                    }

                    this.scheduleProcess(minIdx ?? 0);
                }
            }
           
            return new SchedulerResult
                {
                    CpuProcesses = cpuProcesses, 
                    IoProcesses = ioProcesses, 
                    SchedulerStats = calculateStats(processes.Count)
                };
        }

        private SchedulerStats calculateStats(int numProcesses)
        {
            return new SchedulerStats
                {
                    AverageTurnAroundTime = ((double)turnAroundTime) / numProcesses,
                    CpuUtilization = ((double)currentTime - cpuDownTime) / currentTime,
                    AverageWaitingTime = ((double)waitingTime) / numProcesses,
                    ProcessWaitTimes = processorsWaitTimes
                };
        }

        private void scheduleProcess(int queueId)
        {
            //We always take the first item because its first come first serve
            KimProcessItem nextItem = queues[queueId][0];
            int arivialTime = nextItem.process.ArrivalTime;
            int nextIdx = nextItem.burstArrayIndex;

            if (currentTime < arivialTime)
            {
                //Used for calculating cpu utilization
                cpuDownTime += (arivialTime - currentTime);
                currentTime = arivialTime;
            }
            else
            {
                //Used to calculates avg wait times
                waitingTime += (currentTime - arivialTime);

                //Display Processor Wait Times
                processorsWaitTimes[nextItem.process.Name]+= currentTime - arivialTime;
            }

            int currentBurst = nextItem.process.BurstArray[nextIdx];

            //This is the last queue it is a special case the process finishes all remaing time here
            //This is because this queue has no quantam
            if (queueId == queues.Count - 1)
            {
                //Schedule the process
                cpuProcesses.Add(new Process
                {
                    Duration = currentBurst,
                    Name = nextItem.process.Name,
                    StartTime =  currentTime
                });

                //Increment the current time by the duration of this burst
                currentTime += currentBurst;

                //Set the arrival time to right now
                nextItem.process.ArrivalTime = currentTime;

                //Remove the item from queue 1
                queues[queueId].RemoveAt(0);

                //Schedule IO event move to next burst
                scheduleIOEvent(nextItem, nextIdx);
            }
            //If the next burst is less than the cpu cycle
            else if (currentBurst > QueueTimes[queueId])
            {
                //Schedule the process it only runs for the current queues time
                cpuProcesses.Add(new Process
                {
                    Duration = QueueTimes[queueId],
                    Name = nextItem.process.Name,
                    StartTime = currentTime
                });

                //increment current time
                currentTime +=  QueueTimes[queueId];

                //Change the remaing burst time.
                nextItem.process.BurstArray[nextIdx] -= QueueTimes[queueId];

                //Set the arrival time to right now
                nextItem.process.ArrivalTime = currentTime;

                //Add this to the next queue
                queues[queueId+1].Add(nextItem);
                
                //Remove the item from queue 
                queues[queueId].RemoveAt(0);
            }
            else
            {
                //Schedule the process the length is of the next burst
                cpuProcesses.Add(new Process
                {
                    Duration = currentBurst,
                    Name = nextItem.process.Name,
                    StartTime = currentTime
                });
                //increment the current time
                currentTime += currentBurst;

                //Set the arrival time of this process
                nextItem.process.ArrivalTime = currentTime;

              scheduleIOEvent(nextItem, nextIdx);

              //Remove the item from the current queue
              queues[queueId].RemoveAt(0);
        }
    }

    private void scheduleIOEvent(KimProcessItem nextItem, int nextIdx)
        {

            //increment the burst array
            nextItem.burstArrayIndex += 2;
          //If the io time is less than the current time then there is nothing currently schedules so there was an idle time increment iotime
                if (ioTime < currentTime)
                {
                    ioTime = currentTime;
                }

                if (nextIdx + 1 < nextItem.process.BurstArray.Length)
                {
                    //Schedule the I/O Process
                    ioProcesses.Add(new Process
                        {
                            Duration = nextItem.process.BurstArray[nextIdx + 1],
                            Name = nextItem.process.Name,
                            StartTime = ioTime
                        });

                    //Increment io time
                    ioTime += nextItem.process.BurstArray[nextIdx + 1];

                    //Set the arrivial time to right now this is the i/o time it must be after the i/o burst runs
                    nextItem.process.ArrivalTime = ioTime;

                    //Insert it at the end of queue 0 because it shoudl always start over at queue 0 when a process finishes
                    queues[0].Add(nextItem);
                }
                else
                {
                    //find out when the process started by searching the original list and calculate the turn around time.
                    //this process has now finished.
                    turnAroundTime += (nextItem.process.ArrivalTime - processItems.First(p => p.Name.Equals(nextItem.process.Name)).ArrivalTime);
                }

            }
    }
}