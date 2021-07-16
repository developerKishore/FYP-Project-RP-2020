using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FYP_Project.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult GenerateView()
        {
            return View("Generate");
        }

        public IActionResult Reports()
        {
            return View("GenerateReport");
        }



    }
}