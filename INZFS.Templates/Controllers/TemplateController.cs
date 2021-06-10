using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace INZFS.Templates.Controllers
{
    public class TemplateController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
