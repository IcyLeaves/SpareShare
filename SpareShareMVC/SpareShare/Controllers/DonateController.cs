using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    public class DonateController : Controller
    {
        // GET: Donate
        public ActionResult Index()
        {
            return View();
        }

        // GET: 显示上传物品的表单
        // 修改时间: 2019年4月30日 20点50分
        public ActionResult UploadThings()
        {
            return View();
        }

        // POST: 提交表单，上传捐赠物品信息
        // 修改时间: 2019年4月30日 21点22分
        [HttpPost]
        public ActionResult UploadThings(UploadThingsViewModel model)
        {
            //获取当前用户的Id
            int usrId = (int)HttpContext.Session["usrId"];
            if(ModelState.IsValid)
            {
                //新建Things
                Things t = new Things();
                t.DonatorId = usrId;
                t.Name = model.Name;
                t.Type = model.Type;
                t.Detail = model.Detail;
                //向数据库新建元组
                using (SSDBEntities db = new SSDBEntities())
                {
                    db.Things.Add(t);
                    db.SaveChanges();
                }
                //提交成功，跳转至用户主页
                return RedirectToAction("Index", "Home");
            }
            //模型报错，返回当前页面
            return View(model);
        }
    }
}