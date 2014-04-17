using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scheduler.Models;

namespace Scheduler.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }
        public static List<processItem> getProcessData(int numProc)
        {
            var temp = new List<processItem>();
            for (var i = 0; i < numProc; i++)
            {
                var name = "P" + (i+1);
                var random = new Random();
                var randomNumber = random.Next(1, 4);
                var processItem = new processItem
                {
                    name = name,
                    BurstArray = new int[randomNumber * 2 + 1],
                    arrivalTime = random.Next(1,12);
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
