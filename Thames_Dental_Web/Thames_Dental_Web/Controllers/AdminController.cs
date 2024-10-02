using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
