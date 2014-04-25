using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scheduler.Models;

namespace Scheduler.Classes
{
    public class RoundRobin : Scheduler
    {
        public class EamonProcess : Process
        {
            // We need this in order to preempt processes if they exceed the time quantum.
            // This way, we can return to the process and run as much of it as we can within
            // the quantum restraints. 
            public int BurstRemaining;
        }

        public override SchedulerResult Run(List<ProcessItem> processes)
        {
            // CALCULATE A RANDOM ASS TIME QUANTUM
            Random r = new Random();
            int quantum = r.Next(1, 20);

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
            var cpuProcesses = new List<EamonProcess>();

            // List of IO Processes
            var ioProcesses = new List<EamonProcess>();

            // Information about where we are when accessing process data, etc. 
            var processData = new int[processes.Count, 4];
            for (int i = 0; i < processes.Count; i++)
            {
                processData[i, 0] = 0;  // Current index of 'processes'
                processData[i, 1] = processes[i].BurstArray.Length - 1; // Maximum length of the burst array for a particular process
                processData[i, 2] = 0; // Current burst array index of the particular process we're looking at.
                processData[i, 3] = 1;
            }

            var availableCPUs = new List<EamonProcess>();
            var availableIOs = new List<EamonProcess>();

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

                    if (currentBurstIndex % 2 == 0)
                    {
                        availableCPUs.Add(
                            new EamonProcess
                            {
                                Name = processes[i].Name,
                                Duration = processes[i].BurstArray[currentBurstIndex],
                                ProcessIndex = i,
                                StartTime = cpuTime,
                                BurstRemaining = processes[i].BurstArray[currentBurstIndex]
                            }
                            );
                    }


                    if (currentBurstIndex % 2 > 0)
                    {
                        availableIOs.Add(
                            new EamonProcess
                            {
                                Name = processes[i].Name,
                                Duration = processes[i].BurstArray[currentBurstIndex],
                                ProcessIndex = i,
                                StartTime = cpuTime,
                                BurstRemaining = processes[i].BurstArray[currentBurstIndex]
                            });
                    }


                    // Increment the current burst index for the process we're looking at
                    currentBurstIndex = currentBurstIndex + 1;

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

            currentTime = 0;
            EamonProcess CurrentCPUExec = null;
            EamonProcess CurrentIOExec = null;
            int currentQuantum = quantum;
            // a different approach.
            while (availableCPUs.Count > 0 || availableIOs.Count > 0)
            {
                // The quantum has expired. Time to be done with some processes!
                if (currentQuantum == 0)
                {
                    // If we have a cpu process executing when the quantum expires
                    if (CurrentCPUExec != null)
                    {
                        // If there is work left to do, put the process back into the available queue, but at the end.
                        if (CurrentCPUExec.BurstRemaining - quantum > 0)
                        {
                            CurrentCPUExec.BurstRemaining = CurrentCPUExec.BurstRemaining - quantum;
                            availableCPUs.Insert(availableCPUs.Count, new EamonProcess
                                {
                                    BurstRemaining = CurrentCPUExec.BurstRemaining,
                                    Duration = CurrentCPUExec.BurstRemaining,
                                    Name = CurrentCPUExec.Name,
                                    ProcessIndex = CurrentCPUExec.ProcessIndex,
                                    StartTime = 0
                                });
                            CurrentCPUExec = null;
                        }
                            // There is no more work to do
                        else
                        {
                            CurrentCPUExec.BurstRemaining = CurrentCPUExec.BurstRemaining - quantum;
                            CurrentCPUExec = null;
                        }
                    }

                    // if we have an io process executing when the quantum expires
                    if (CurrentIOExec != null)
                    {
                        // if there is work left to do, put the proces sback into the available queue, but at the end.
                        if (CurrentIOExec.BurstRemaining - quantum > 0)
                        {
                            CurrentIOExec.BurstRemaining = CurrentIOExec.BurstRemaining - quantum;
                            availableIOs.Insert(availableIOs.Count, new EamonProcess
                                {
                                    BurstRemaining = CurrentIOExec.BurstRemaining,
                                    Duration = CurrentIOExec.BurstRemaining,
                                    Name = CurrentIOExec.Name,
                                    ProcessIndex = CurrentIOExec.ProcessIndex,
                                    StartTime = 0
                                }
                                );
                            CurrentIOExec = null;
                        }
                            // there is no more work to do
                        else
                        {
                            CurrentIOExec.BurstRemaining = CurrentIOExec.BurstRemaining - quantum;
                            CurrentIOExec = null;
                        }
                    }
                    // reset the quantum
                    currentQuantum = quantum;
                }


                EamonProcess CPUProc;
                try
                {
                    if (CurrentIOExec != null)
                    {
                        CPUProc = availableCPUs.First(x => x.ProcessIndex != CurrentIOExec.ProcessIndex);
                    }
                    else
                    {
                        CPUProc = availableCPUs.First();
                    }
                }
                catch (InvalidOperationException e)
                {
                    cpuWaiting = true;
                    CPUProc = null;
                }

                EamonProcess IOProc;
                try
                {
                    if (CurrentCPUExec != null && CPUProc != null)
                    {
                        IOProc =
                            availableIOs.First(
                                x =>
                                x.ProcessIndex != CPUProc.ProcessIndex && x.ProcessIndex != CurrentCPUExec.ProcessIndex);
                    }
                    else if (CurrentCPUExec == null && CPUProc != null)
                    {
                        IOProc = availableIOs.First(x => x.ProcessIndex != CPUProc.ProcessIndex);
                    } else if (CurrentCPUExec != null & CPUProc == null)
                    {
                        IOProc = availableIOs.First(x => x.ProcessIndex != CurrentCPUExec.ProcessIndex);
                    }
                    else
                    {
                        IOProc = availableIOs.First();
                    }
                }
                catch (InvalidOperationException e)
                {
                    // If we get here, that means there doesn't exist an IO operation that is not the same as a CPU opeation.
                    // So.... we're waiting.
                    ioWaiting = true;
                    IOProc = null;
                }

                // If there is no work to be done for the IO
                if (IOProc == null)
                {
                    if (CurrentIOExec != null)
                    {
                        if (CurrentIOExec.StartTime + CurrentIOExec.Duration == currentTime)
                        {
                            CurrentIOExec = null;
                        }
                    }
                }

                // If there i sno work to be done for the CPU
                if (CPUProc == null)
                {
                    if (CurrentCPUExec != null)
                    {
                        if (CurrentCPUExec.StartTime + CurrentCPUExec.Duration == currentTime)
                        {
                            CurrentCPUExec = null;
                        }
                    }
                }


                // If both are waiting, continue.
                if (CPUProc != null && IOProc != null && processes[CPUProc.ProcessIndex].ArrivalTime > currentTime &&
                    processes[IOProc.ProcessIndex].ArrivalTime > currentTime)
                {
                    ioWaiting = true;
                    cpuWaiting = true;
                    currentTime++;
                    currentQuantum--;
                    foreach (ProcessItem x in processes.Where(x => x.ArrivalTime <= arrivalTime))
                    {
                        waitTimes[processes.IndexOf(x)] = waitTimes[processes.IndexOf(x)] + 1;
                    }
                    continue;
                }


                // Nothing is currently being processed, but something CAN begin execution
                if (CPUProc != null && CurrentCPUExec == null && processes[CPUProc.ProcessIndex].ArrivalTime <= currentTime)
                {
                    // Set the currently executing process
                    CurrentCPUExec = CPUProc;
                    if (CurrentCPUExec.Duration > quantum)
                    {
                        CurrentCPUExec.BurstRemaining = CurrentCPUExec.Duration;
                        CurrentCPUExec.Duration = quantum;
                    }
                    // Set the start time of the currently executing process
                    CurrentCPUExec.StartTime = currentTime;
                    // Add the process to the cpuProcesses queue
                    cpuProcesses.Add(CurrentCPUExec);
                    // Remove it from the available queue
                    availableCPUs.RemoveAt(availableCPUs.IndexOf(CurrentCPUExec));
                }
                // Meaning we have a proces currently executing, and we have one ready for execution as well.
                else if (CPUProc != null && CurrentCPUExec != null && processes[CPUProc.ProcessIndex].ArrivalTime <= currentTime)
                {
                    // meaning our current process just finished execution
                    if (CurrentCPUExec.StartTime + CurrentCPUExec.Duration == currentTime)
                    {
                        CurrentCPUExec = null;
                        CurrentCPUExec = CPUProc;
                        CurrentCPUExec.StartTime = currentTime;
                        if (CurrentCPUExec.Duration > quantum)
                        {
                            CurrentCPUExec.BurstRemaining = CurrentCPUExec.Duration;
                            CurrentCPUExec.Duration = quantum;
                        }
                        cpuProcesses.Add(CurrentCPUExec);
                        availableCPUs.RemoveAt(availableCPUs.IndexOf(CurrentCPUExec));
                    }
                }

                // Nothing is currently being processed, but something CAN begin execution
                if (IOProc != null && CurrentIOExec == null && processes[IOProc.ProcessIndex].ArrivalTime <= currentTime)
                {
                    // SEt the currently executing process
                    CurrentIOExec = IOProc;
                    // Set the start time of the currently executing process
                    CurrentIOExec.StartTime = currentTime;
                    if (CurrentIOExec.Duration > quantum)
                    {
                        CurrentIOExec.BurstRemaining = CurrentIOExec.Duration;
                        CurrentIOExec.Duration = quantum;
                    }
                    // Add the process to the ioProcesses queue
                    ioProcesses.Add(CurrentIOExec);
                    // Remove it from the available queue
                    availableIOs.RemoveAt(availableIOs.IndexOf(CurrentIOExec));
                }
                // meaning we have a process currently executing, and we have one ready for execution as well.
                else if (IOProc != null && CurrentIOExec != null && processes[IOProc.ProcessIndex].ArrivalTime <= currentTime)
                {
                    // meaning our current process just finished execution
                    if (CurrentIOExec.StartTime + CurrentIOExec.Duration == currentTime)
                    {
                        EamonProcess CopyOfCurrent = CurrentIOExec;
                        CurrentIOExec = null;
                        CurrentIOExec = IOProc;
                        if (CurrentIOExec.Duration > quantum)
                        {
                            CurrentIOExec.BurstRemaining = CurrentIOExec.Duration;
                            CurrentIOExec.Duration = quantum;
                        }
                        CurrentIOExec.StartTime = currentTime;
                        ioProcesses.Add(CurrentIOExec);
                        availableIOs.RemoveAt(availableIOs.IndexOf(CurrentIOExec));
                    }
                }
                currentTime++;
                currentQuantum--;
            }


            //// If there is still processor work to do, do it.
            //while (availableCPUs.Count > 0)
            //{
            //    Process CPUProc = availableCPUs.First();
            //    cpuTime = cpuTime + CPUProc.Duration;
            //    cpuProcesses.Add(CPUProc);
            //    availableCPUs.RemoveAt(0);

            //    ioWait = cpuTime - currentTime;
            //    currentTime = ioTime = cpuTime;
            //}

            //while (availableIOs.Count > 0)
            //{
            //    Process IOProc = availableIOs.First();
            //    ioTime = ioTime + IOProc.Duration;
            //    ioProcesses.Add(IOProc);
            //    availableIOs.RemoveAt(0);

            //    cpuWait = ioTime - currentTime;
            //    currentTime = cpuTime = ioTime;
            //}

            List<Process> RealCPUProcesses = new List<Process>();
            List<Process> RealIOProcesses = new List<Process>();

            foreach (EamonProcess x in cpuProcesses)
            {
                RealCPUProcesses.Add(
                    new Process
                        {
                            Duration = x.Duration,
                            Name = x.Name,
                            ProcessIndex = x.ProcessIndex,
                            StartTime = x.StartTime
                        });
            }

            foreach (EamonProcess x in ioProcesses)
            {
                RealIOProcesses.Add(
                    new Process
                        {
                            Duration = x.Duration,
                            Name = x.Name,
                            ProcessIndex = x.ProcessIndex,
                            StartTime = x.StartTime
                        });
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
                CpuProcesses = RealCPUProcesses,
                IoProcesses = RealIOProcesses,
                SchedulerStats = calculateStats(processes.Count, waitingTime, processorsWaitTimes),
                Quantum = quantum
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
