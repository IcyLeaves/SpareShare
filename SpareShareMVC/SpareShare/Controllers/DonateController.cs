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

        // GET: 显示用户捐赠物品列表
        // 修改时间: 2019年5月1日 13点29分
        public ActionResult MyThingsList()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<MyThingsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找该用户发布的所有捐赠物品
                var ts = db.Things.Where(x => x.DonatorId == usrId).ToList();
                //给视图模型赋值
                foreach (var t in ts)
                {
                    //新建一个视图模型并赋值
                    var tmp = new MyThingsListViewModel();
                    tmp.Name = t.Name;
                    tmp.Type = t.Type;
                    tmp.Detail = t.Detail;
                    tmp.ThingId = t.Id;
                    //添加到列表中
                    res.Add(tmp);
                }
            }
            return View(res);
        }

        // GET: 显示捐赠物品详细信息
        // 修改时间: 2019年5月3日 13点00分
        public ActionResult ThingsDetail(int id)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                var t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                return View(t);
            }
        }

        
    }
}