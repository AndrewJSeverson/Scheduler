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

        public ActionResult Index()
        {
            Feedback f = new Feedback();
            SJFNonPreemtive s = new SJFNonPreemtive();
            //RoundRobin rr = new RoundRobin();
            List <ProcessItem> test = getProcessData(3);
            HomeModel model = new HomeModel
                {
                    Feedback = f.Run(test),
                    SRT = s.Run(test)
                    //RR = rr.Run(getProcessData(3))
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
                var randomNumber = random.Next(1, 4);
                var processItem = new ProcessItem
                {
                    Name = name,
                    BurstArray = new int[randomNumber * 2 + 1],
                    ArrivalTime = random.Next(0,12)
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
