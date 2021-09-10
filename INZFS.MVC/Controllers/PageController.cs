﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INZFS.MVC.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Accessibility()
        {
            return View();
        }
        public IActionResult PrivacyNotice()
        {
            return View();
        }
        public IActionResult Help()
        {
            return View();
        }
        public IActionResult CookiePolicy()
        {
            return View();
        }
    }
}
