using SpareShareMVC.Models;
using SpareShareMVC.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SpareShareMVC.Controllers
{
    [MyAuthorize]
    public class SSUserController : Controller
    {
        Entities db = new Entities();
        public string[] Provinces = new string[] { "北京", "天津", "河北", "山西", "内蒙古", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东","河南",
                                                    "湖北","湖南","广东","广西","海南","重庆","四川","贵州","云南","西藏","陕西","甘肃","青海","宁夏","新疆","香港","澳门","台湾"};

        // GET: SSUser
        public ActionResult Index()
        {
            return View();
        }

        // GET: SSUser/Manage 个人信息
        public ActionResult Manage()
        {
            int usrId = (int)HttpContext.Session["usrId"];
            return View(db.Users.Where(x => x.UserId == usrId).FirstOrDefault());
        }

        // GET: SSUser/EditManage 编辑个人信息（显示表单） id=RPID
        public ActionResult EditManage(int id)
        {
            Users u = db.Users.Where(x => x.UserId == id).FirstOrDefault();
            ViewBag.User = u;
            List<SelectListItem> selectList = new List<SelectListItem>();
            foreach (var p in Provinces)
            {
                selectList.Add(new SelectListItem { Value = p, Text = p });
            }
            ViewBag.selectProvince = selectList;
            return View();
        }

        // POST: SSUser/EditManage 编辑受助方个人信息（提交表单） model=表单数据 id=RPID
        [HttpPost]
        public ActionResult EditManage(SSEditManageViewModel model, int id)
        {
            if(!ModelState.IsValid)
            {
                ModelState.AddModelError("Email", "请输入正确格式的邮箱");
                return View(model);
            }
            Users u = db.Users.Where(x => x.UserId == id).FirstOrDefault();
            u.Sex = model.Sex;
            u.Email = model.Email;
            u.QQ = model.QQ;
            u.Province = model.Province;
            u.School = model.School;
            db.Entry(u).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Manage");
        }

        // GET: SSUser/Credit/id 个人信誉评价 id=RPID
        public ActionResult Credit(int? id)
        {
            if (id == null)
            {
                id = (int)HttpContext.Session["usrId"];
            }
            Assess a = db.Assess.Where(x => x.UserId == id).FirstOrDefault();

            Users u = db.Users.Where(x => x.UserId == id).FirstOrDefault();
            ViewBag.QQ = u.QQ;
            ViewBag.Email = u.Email;
            return View(a);
        }
    }
}