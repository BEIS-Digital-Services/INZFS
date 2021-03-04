using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INZFS.MVC.Controllers
{
    public class ApplicationController : Controller
    {
        public IActionResult Index()
        {
            var contentType = "Widget";
            return View();
        }

        [HttpPost]
        public IActionResult Handle()
        {
            var form = Request.Form;
            var nextContentId = Request.Form["nextContent"].ToString();
            var redirectUrl = @$"../Contents/ContentItems/{nextContentId}";
            return Redirect(redirectUrl);
        }
    }
}
