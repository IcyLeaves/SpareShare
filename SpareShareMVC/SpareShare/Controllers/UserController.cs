using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
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
            return View();
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
    }
}