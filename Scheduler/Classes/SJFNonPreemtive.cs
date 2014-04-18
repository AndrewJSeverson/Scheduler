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
    public class SJFNonPreemtive
    {
        private readonly List<processItem> _processes;

        public SJFNonPreemtive(List<processItem> processList)
        {
            //Sort provided process list by arrival time and add it to queue
            processList.Sort((x, y) => x.arrivalTime.CompareTo(y.arrivalTime));
            _processes = processList;
        }

        public void Run()
        {
            var cpu = new List<KodyProcess>();
            var io = new List<KodyProcess>();

            //Tracks which process index i'm on for each process. Not sure if i'll need this yet
            var processData = new int[_processes.Count, 2];
            for (int i = 0; i < processData.Length; i++)
            {
                processData[i, 0] = 0;
                processData[i, 1] = _processes[i].BurstArray.Length;
            }

            //Add first process to list of current processes, since we have them ordered by arrival time
            List<processItem> currentProcesses = new List<processItem> {_processes.First()};
            int currentTime, cpuTime, ioTime;
            currentTime = cpuTime = ioTime = _processes.First().arrivalTime;
            _processes.RemoveAt(0);

            while (currentProcesses.Count > 0)
            {
                //Check if any other processes have arrived, if so add them to currentProcesses
                for(int i=0; i < _processes.Count; i++)
                {
                    if (_processes[i].arrivalTime <= currentTime)
                    {
                        currentProcesses.Add(_processes[i]);
                        _processes.RemoveAt(i);
                    }
                }
                var availableCPUs = new List<KodyProcess>();
                var availableIOs = new List<KodyProcess>();
                var newCurrTime = 0;
                for (int i = 0; i < currentProcesses.Count; i++)
                {
                    int pos = processData[i, 0]; //TODO check if pos has reached "end" of processes task list (found in processData[i, 1]), if so remove it from currentProcesses list I think?
                    //CPU burst
                    if (pos % 2 == 0)
                    {
                        availableCPUs.Add(
                            new KodyProcess
                            {
                                Name = currentProcesses[i].name,
                                StartTime = cpuTime,
                                Duration = currentProcesses[i].BurstArray[pos]
                            }
                        );
                    }
                    //IO burst
                    else
                    {
                        availableIOs.Add(
                            new KodyProcess
                            {
                                Name = currentProcesses[i].name,
                                StartTime = ioTime,
                                Duration = currentProcesses[i].BurstArray[pos]
                            }
                        );
                    }
                    //Increase position in process burst array
                    processData[i, 0]++;
                }

                //If any are available, sort them and add the shortest one to our lists
                if (availableCPUs.Count > 0)
                {
                    availableCPUs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    KodyProcess first = availableCPUs.First();
                    cpu.Add(first);
                    cpuTime = first.StartTime + first.Duration;
                }
                if (availableIOs.Count > 0)
                {
                    availableIOs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    KodyProcess first = availableIOs.First();
                    io.Add(first);
                    ioTime = first.StartTime + first.Duration;
                }
                //Set current time to minimum of cpu and io time
                currentTime = cpuTime > ioTime ? ioTime : cpuTime;

                //TODO Need to calculate wait time somewhere around here
                //TODO Need to handle following case (e.g. current time got to 8, and current io is sitting at 10, but there's no more CPUs to process until some IOs are. In this case "wait time" will be added, and current time will be brought to the io time).
            }
        }
    }
}