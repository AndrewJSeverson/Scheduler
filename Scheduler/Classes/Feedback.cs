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

        public override SchedulerResult Run(List<ProcessItem> processes)
        {
            currentTime = 0;
            ioTime = 0;

            //Sort the list by arrivial times and pick the first one to start.
            processes.Sort((x, y) => x.ArrivalTime.CompareTo(y.ArrivalTime));
            queues[0] = processes.Select(l => new KimProcessItem
                        {
                            process = l,
                            burstArrayIndex = 0
                        }).ToList();

            while (queues[0].Count + queues[1].Count + queues[2].Count + queues[3].Count > 0)
            {
                for (int i = 0; i < queues.Count; i++)
                {
                    if (queues[i].Count > 0)
                    {
                        this.scheduleProcess(i);
                        //Resort the queue by arrivial time.  This will ensure that when processes shouldn't start they won't
                        //As the algorithm finsihed this won't do much sorting cause the list will be sorted
                        queues[i].Sort((x, y) => x.process.ArrivalTime.CompareTo(y.process.ArrivalTime));
                        break;
                    }
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
            int startTime = currentTime < nextItem.process.ArrivalTime ? nextItem.process.ArrivalTime : currentTime;
            int currentBurst = nextItem.process.BurstArray[nextItem.burstArrayIndex];


            //This means that the process has terminated at the arrival time.  
            if (nextItem.burstArrayIndex >= nextItem.process.BurstArray.Count())
            {
                return;
            }

            //This is the last queue it is a special case the process finishes all remaing time here
            //This is because this queue has no quantam
            if (queueId == queues.Count - 1)
            {
                //Schedule the process
                cpuProcesses.Add(new Process
                {
                    Duration = currentBurst,
                    Name = nextItem.process.Name,
                    StartTime =  startTime
                });

                //Increment the current time by the duration of this burst
                currentTime += currentBurst;

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
                    StartTime = startTime
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
                    Duration = currentBurst,
                    Name = nextItem.process.Name,
                    StartTime = startTime
                });
                //increment the current time
                currentTime = nextItem.process.ArrivalTime + currentBurst;

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

                //increment the burst array
                nextItem.burstArrayIndex += 2;

                //Remove the item from the current queue
                queues[queueId].RemoveAt(0);

                //Set the arrivial time to right now this is the i/o time it must be after the i/o burst runs
                nextItem.process.ArrivalTime = ioTime;

                //Insert it at the end of queue 0 because
                queues[queueId].Add(nextItem);
            }
        }
    }
}