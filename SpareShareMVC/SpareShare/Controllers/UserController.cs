using SpareShare.Filter;
using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    [MyAuthorize]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: 查看个人资料
        // 修改时间: 2019年5月26日 18点11分
        public ActionResult Manage()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            using (SSDBEntities db = new SSDBEntities())
            {
                var u = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
                return View(u);
            }
        }

        // GET: 查看编辑资料表单
        // 修改时间: 2019年5月26日 20点44分
        public ActionResult EditManage()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            using (SSDBEntities db = new SSDBEntities())
            {
                Users u = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
                ViewBag.User = u;
                return View();
            }
        }

        // POST: 提交编辑资料信息
        // 修改时间: 2019年5月26日 20点56分
        [HttpPost]
        public ActionResult EditManage(EditManageViewModel model)
        {
            if(ModelState.IsValid)
            {
                //获取当前用户id
                int usrId = (int)HttpContext.Session["usrId"];
                using (SSDBEntities db = new SSDBEntities())
                {
                    var u = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
                    u.Province = model.Province;
                    u.School = model.School;
                    u.QQ = model.QQ;
                    u.Sex = model.Sex;
                    u.Email = model.Email;
                    db.Entry(u).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Manage");
            }
            return View(model);
        }

        // GET: 查看信誉评价
        public ActionResult Credit()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型
            CreditViewModel res = new CreditViewModel();
            using (SSDBEntities db = new SSDBEntities())
            {
                Users u = db.Users.Where(X => X.UserId == usrId).FirstOrDefault();
                Assess a = db.Assess.Where(x => x.UserId == usrId).FirstOrDefault();
                res.assess = a;
                res.QQ = u.QQ;
                res.Email = u.Email;
                return View(res);
            }
                
        }
    }
}