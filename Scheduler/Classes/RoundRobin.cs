using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public class RoundRobin : Scheduler
    {
        private List<ProcessItem> processItems;
        
        //The four queues this implementation of feedback requires
        private List<KimProcessItem> queue = new List<KimProcessItem>();


        //Return list of cpuProcesses
        private List<Process> cpuProcesses = new List<Process>();
        //Return list of scheduled io processes
        private List<Process> ioProcesses = new List<Process>();

        //The current cpu time
        private int currentTime;
        //The current io time
        private int ioTime;

        private int quantum;

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
            // CALCULATE A RANDOM ASS TIME QUANTUM
            Random r = new Random();
            quantum = r.Next(1, 20);

            //Initialize Process Dictonary for the process wait times
            processorsWaitTimes = processes.ToDictionary(p => p.Name, p => 0);

            //Sort the list by arrivial times and pick the first one to start.
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));

            processItems = processes;

            queue = processes.Select(l => new KimProcessItem
                {
                    process = new ProcessItem()
                        {
                            ArrivalTime = l.ArrivalTime,
                            BurstArray = (int[]) l.BurstArray.Clone(),
                            Name = l.Name
                        },
                    burstArrayIndex = 0
                }).ToList();

            while (queue.Any())
            {
               
                    queue.Sort((x, y) => x.process.ArrivalTime.CompareTo(y.process.ArrivalTime));

  
                    this.scheduleProcess();
               
            }

            return new SchedulerResult
                {
                    CpuProcesses = cpuProcesses,
                    IoProcesses = ioProcesses,
                    SchedulerStats = calculateStats(processes.Count), 
                    Quantum = quantum
                };
        }

        private SchedulerStats calculateStats(int numProcesses)
        {
            return new SchedulerStats
                {
                    AverageTurnAroundTime = ((double) turnAroundTime)/numProcesses,
                    CpuUtilization = ((double) currentTime - cpuDownTime)/currentTime,
                    AverageWaitingTime = ((double) waitingTime)/numProcesses,
                    ProcessWaitTimes = processorsWaitTimes, 
                };
        }

        private void scheduleProcess()
        {
            //We always take the first item because its first come first serve
            KimProcessItem nextItem = queue[0];
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
                processorsWaitTimes[nextItem.process.Name] += currentTime - arivialTime;
            }

            int currentBurst = nextItem.process.BurstArray[nextIdx];

            if (currentBurst > quantum)
            {
                //Schedule the process it only runs for the current queues time
                cpuProcesses.Add(new Process
                    {
                        Duration = quantum,
                        Name = nextItem.process.Name,
                        StartTime = currentTime
                    });

                //increment current time
                currentTime += quantum;

                //Change the remaing burst time.
                nextItem.process.BurstArray[nextIdx] -= quantum;

                //Set the arrival time to right now
                nextItem.process.ArrivalTime = currentTime;

                //Add this to the next queue
                queue.Add(nextItem);

                //Remove the item from queue 
                queue.RemoveAt(0);
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
                queue.RemoveAt(0);
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
                queue.Add(nextItem);
            }
            else
            {
                //find out when the process started by searching the original list and calculate the turn around time.
                //this process has now finished.
                turnAroundTime += (nextItem.process.ArrivalTime -
                                   processItems.First(p => p.Name.Equals(nextItem.process.Name)).ArrivalTime);
            }

        }
    }
}
