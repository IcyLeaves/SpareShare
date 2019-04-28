using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpareShareMVC.Models;
using System.Data.Entity;

namespace SpareShareMVC.Controllers
{
    public class SSAccountController : Controller
    {
        Entities db=new Entities();
        public string[] Provinces = new string[] { "北京", "天津", "河北", "山西", "内蒙古", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东","河南",
                                                    "湖北","湖南","广东","广西","海南","重庆","四川","贵州","云南","西藏","陕西","甘肃","青海","宁夏","新疆","香港","澳门","台湾"};


        // GET: SSAccount/Login
        public ActionResult Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // POST: SSAccount/Login
        [HttpPost]
        public ActionResult Login(SSLoginViewModel model, string ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string username = model.Username;
            string password = model.Password;
            //查询用户名和密码
            var User = db.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();
            if (User == null)
            {
                ModelState.AddModelError("", "登录失败");
                return View(model);
            }
            else
            {
                HttpContext.Session["usrName"] = model.Username;
                HttpContext.Session["usrId"] = User.UserId;
                HttpContext.Session["isAdmin"] = User.IsAdmin;
            }
            return RedirectToAction("Index", "SSUser");
        }

        //GET: SSAccount/Register
        public ActionResult Register()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            foreach(var p in Provinces)
            {
                selectList.Add(new SelectListItem { Value = p, Text = p });
            }
            ViewBag.selectProvince = selectList;
            return View();
        }

        //POST: SSAccount/Register
        [HttpPost]
        public ActionResult Register(SSRegisterViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            string username = model.Username;
            string password = model.Password;
            if(db.Users.Where(x=>x.Username==username).FirstOrDefault()!=null)
            {
                ModelState.AddModelError("", "用户名已注册");
                return View(model);
            }
            var u = new Users();
            int uId = 0;
            u.RegTime = DateTime.Now;
            u.Username = username;
            u.Password = password;
            u.Sex = model.Sex;
            u.Email = model.Email;
            u.QQ = model.QQ;
            u.Province = model.Province;
            u.School = model.School;
            u.IsAdmin = model.isAdmin;
            using (Entities db = new Entities())
            {
                db.Users.Add(u);
                db.SaveChanges();
                uId = u.UserId;
            }

            var a = new Assess();
            a.UserId = uId;
            a.GiveNum = 0;
            a.ReceiveNum = 0;
            a.CheckedNum = 0;
            a.CreditPoint = 0;
            a.CreditLevel = 1;
            db.Assess.Add(a);
            db.SaveChanges();
            return RedirectToAction("Login");
        }

        public ActionResult ToManagePage()
        {
            string usrOp=(string)HttpContext.Session["usrOp"];
            switch (usrOp)
            {
                case "A":
                    return RedirectToAction("ManageAdministor", "SSAdministor");
                case "G":
                    return RedirectToAction("ManageGivenPeople", "SSGiven");
                case "R":
                    return RedirectToAction("ManageReceivePeople", "SSReceive");
            }
            return RedirectToAction("Index", "SSHome");
        }

        //
        [HttpPost]
        public ActionResult LogOff()
        {
            HttpContext.Session["usrName"] = null;
            HttpContext.Session["usrId"] = null;
            HttpContext.Session["isAdmin"] = null;
            return RedirectToAction("Index", "SSHome");
        }
    }
}