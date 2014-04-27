using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Scheduler.Classes;
using Scheduler.Models;

namespace Scheduler.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index(int? id)
        {
            int numProcess = id ?? 3;
            Feedback f = new Feedback();
            SJFNonPreemtive s = new SJFNonPreemtive();
            RoundRobin rr = new RoundRobin();
            Preemptive p = new Preemptive();
            FCFS fcfs = new FCFS();
            List<ProcessItem> processItems = new List<ProcessItem>(); //= getProcessData(numProcess);

            ProcessItem p1 = new ProcessItem
            {
                Name = "P1",
                ArrivalTime = 0,
                BurstArray = new[]{6,17,3,11,16,10,13}
            };
            ProcessItem p2 = new ProcessItem
            {
                Name = "P2",
                ArrivalTime = 11,
                BurstArray = new[] { 6,2,5,13,8 }
            };
            ProcessItem p3 = new ProcessItem
            {
                Name = "P3",
                ArrivalTime = 5,
                BurstArray = new[] { 16,6,13,5,4,9,2 }
            };
            processItems.Add(p1);
            processItems.Add(p2);
            processItems.Add(p3);



            HomeModel model = new HomeModel
                {
                NumProcess = numProcess,
                ProcessItems = Helper.Clone(processItems),
                Feedback = f.Run(Helper.Clone(processItems)),
                SPN = s.Run(Helper.Clone(processItems)),
                SRT = p.Run(processItems),
                RR = rr.Run(processItems),
                FCFS = fcfs.Run(processItems)
                };
            return View(model);
        }
        public static List<ProcessItem> getProcessData(int numProc)
        {
            var temp = new List<ProcessItem>();
            for (var i = 0; i < numProc; i++)
            {
                var name = "P" + (i+1);
                var random = new Random();
                System.Threading.Thread.Sleep(random.Next(1,200));
                var randomNumber = random.Next(1, 4);
                var processItem = new ProcessItem
                {
                    Name = name,
                    BurstArray = new int[randomNumber * 2 + 1],
                    ArrivalTime = random.Next(0, 12)
                };
                for (var j = 0; j < processItem.BurstArray.Length; j++)
                {
                    var randTime = random.Next(1, 20);
                    processItem.BurstArray[j] = randTime;
                }
                temp.Add(processItem);
            }
            return temp;
        }

    }
}
