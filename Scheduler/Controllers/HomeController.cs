using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            //Preemptive p = new Preemptive();
            FCFS fcfs = new FCFS();
            List<ProcessItem> processItems = getProcessData(numProcess);
            HomeModel model = new HomeModel
                {
                NumProcess = numProcess,
                ProcessItems = processItems,
                Feedback = f.Run(processItems),
                //SPN = s.Run(processItems),
                //SRT = p.Run(processItems)
                //RR = rr.Run(processItems)
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
