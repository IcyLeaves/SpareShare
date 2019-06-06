using SpareShare.Models;
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
            using (SSDBEntities db = new SSDBEntities())
            {
                int cntUsers = db.Users.Count();//用户数量
                int cntThings = db.Things.Count();//捐赠物品数量
                ViewBag.cntUsers = cntUsers;
                ViewBag.cntThings = cntThings;
            }
            return View();
        }
    }
}