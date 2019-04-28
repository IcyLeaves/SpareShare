using SpareShareMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShareMVC.Controllers
{
    public class SSHomeController : Controller
    {
        Entities db = new Entities();
        // GET: SSHome
        public ActionResult Index()
        {
            //if (HttpContext.Session["usrId"] != null)
            //    return RedirectToAction("Index", "SSUser");
            int cntUsers = db.Users.Count();
            int cntThings = db.Things.Count();
            ViewBag.cntUsers = cntUsers;
            ViewBag.cntThings = cntThings;
            return View();
        }
    }
}