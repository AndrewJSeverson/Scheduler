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
        private List<int> QueueTimes = new List<int>
            {
                1,
                2,
                4
            };

        private List<List<KimProcessItem>> queues = new List<List<KimProcessItem>>
            {
                new List<KimProcessItem>(),
                new List<KimProcessItem>(),
                new List<KimProcessItem>(),
                new List<KimProcessItem>()
            };

        private List<Process> cpuProcesses = new List<Process>();
        private List<Process> ioProcesses = new List<Process>();
        private int currentTime;
        private int ioTime;

        public override SchedulerResult Run(List<ProcessItem> processes)
        {
            currentTime = 0;
            ioTime = 0;

            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));
            queues[0] = processes.Select(l => new KimProcessItem
                {
                    process = l, 
                    burstArrayIndex = 0
                }).ToList();


            while (queues[0].Count + queues[1].Count + queues[2].Count + queues[3].Count > 0)
            {
                if (queues[0].Count > 0)
                {
                    this.scheduleProcess(0);
                }else if (queues[1].Count > 0)
                {
                    this.scheduleProcess(1);
                }else if (queues[2].Count > 0)
                {
                    this.scheduleProcess(2);
                }else if (queues[3].Count > 0)
                {
                    this.scheduleProcess(3);
                }
            }
           
            return new SchedulerResult
                {
                    CpuProcesses = cpuProcesses, 
                    IoProcesses = ioProcesses, 
                    SchedulerStats = null
                };
        }

        private void scheduleProcess(int queueId)
        {
            //We always take the first item because its first come first serve
            KimProcessItem nextItem = queues[queueId][0];

            //This means that the process has terminated at the arrival time.  
            if (nextItem.burstArrayIndex >= nextItem.process.BurstArray.Count())
            {
                return;
            }

            int currentBurst = nextItem.process.BurstArray[nextItem.burstArrayIndex];
            
            //This is the last queue it is a special case the process finishes all remaing time here
            if (queueId == 3)
            {
                 //Schedule the process
                cpuProcesses.Add(new Process
                {
                    Duration = nextItem.process.BurstArray[nextItem.burstArrayIndex],
                    Name = nextItem.process.Name,
                    StartTime =  currentTime < nextItem.process.ArrivalTime ? nextItem.process.ArrivalTime : currentTime
                });

                //Increment the current time by the duration of this burst
                currentTime += nextItem.process.BurstArray[nextItem.burstArrayIndex];

                //Remove the item from queue 1
                queues[queueId].RemoveAt(0);
            }
            //If the next burst is less than the cpu cycle
            else if (currentBurst > QueueTimes[queueId])
            {
                //Schedule the process it only runs for the current queues time
                cpuProcesses.Add(new Process
                {
                    Duration = QueueTimes[queueId],
                    Name = nextItem.process.Name,
                    StartTime = currentTime < nextItem.process.ArrivalTime ? nextItem.process.ArrivalTime : currentTime
                });

                //increment current time
                currentTime +=  QueueTimes[queueId];

                //Change the remaing burst time.
                nextItem.process.BurstArray[nextItem.burstArrayIndex] -= QueueTimes[queueId];

                //Set the arrival time to right now
                nextItem.process.ArrivalTime = currentTime;

                //Add this to the next queue
                queues[queueId+1].Add(nextItem);
                
                //Remove the item from queue 1
                queues[queueId].RemoveAt(0);
            }
            else
            {
                //Schedule the process the length is of the next burst
                cpuProcesses.Add(new Process
                {
                    Duration = nextItem.process.BurstArray[nextItem.burstArrayIndex],
                    Name = nextItem.process.Name,
                    StartTime = currentTime < nextItem.process.ArrivalTime ? nextItem.process.ArrivalTime : currentTime
                });
                //increment the current time
                currentTime = nextItem.process.ArrivalTime + nextItem.process.BurstArray[nextItem.burstArrayIndex];

                //If the io time is less than the current time then there is nothing currently schedules so there was an idle time increment iotime
                if (ioTime < currentTime)
                {
                    ioTime = currentTime;
                }

                //Schedule the I/O Process
                ioProcesses.Add(new Process
                {
                    Duration = nextItem.process.BurstArray[nextItem.burstArrayIndex + 1],
                    Name = nextItem.process.Name,
                    StartTime = ioTime
                });

                //Increment io time
                ioTime += nextItem.process.BurstArray[nextItem.burstArrayIndex + 1];

                //if the process was scheduled move this one to the back of this queue again
                nextItem.burstArrayIndex += 2;

                //Remove the item from the current queue
                queues[queueId].RemoveAt(0);

                //Set the arrivial time to right now this is the i/o time it must be after the i/o burst runs
                nextItem.process.ArrivalTime = ioTime;

                //Insert it at the end of queue 0 becau
                queues[queueId].Add(nextItem);
            }
        }
    }
}