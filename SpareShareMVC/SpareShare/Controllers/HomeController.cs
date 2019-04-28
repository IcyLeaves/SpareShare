using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    public class HomeController : Controller
    {
        // GET: 打开首页界面
        public ActionResult Index()
        {
            return View();
        }
    }
}