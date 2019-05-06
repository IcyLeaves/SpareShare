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
        // 修改时间: 2019年5月3日 14点05分
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
                t.Status = "正在审核中";
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
            var res = new List<ThingsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找该用户发布的所有捐赠物品
                var ts = db.Things.Where(x => x.DonatorId == usrId).ToList();
                //给视图模型赋值
                foreach (var t in ts)
                {
                    //新建一个视图模型并赋值
                    var tmp = new ThingsListViewModel();
                    tmp.Name = t.Name;
                    tmp.Type = t.Type;
                    tmp.Detail = t.Detail;
                    tmp.ThingId = t.Id;
                    tmp.Status = t.Status;
                    //添加到列表中
                    res.Add(tmp);
                }
            }
            return View(res);
        }

        // GET: 显示捐赠物品详细信息
        // 修改时间: 2019年5月3日 15点10分
        public ActionResult ThingsDetail(int id)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的捐赠物品
                var t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                return View(t);
            }
        }

        // GET: 显示受助请求详细信息
        // 修改时间: 2019年5月3日 15点10分
        public ActionResult QuestsDetail(int id)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的受助请求
                var q = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                return View(q);
            }
        }

        // GET: 查询其他用户的受助请求
        // 修改时间: 2019年5月6日 15点22分
        public ActionResult SearchQuests()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<QuestsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找[其他用户的][闲置的]请求
                var qs = db.Quests.Where(x => x.Id != usrId && x.Status == "等待受助中");
                //给视图模型赋值
                foreach (var q in qs)
                {
                    //新建一个视图模型并赋值
                    var tmp = new QuestsListViewModel();
                    tmp.Name = q.Name;
                    tmp.Type = q.Type;
                    tmp.Detail = q.Detail;
                    tmp.QuestId =q.Id;
                    tmp.Status = q.Status;
                    //添加到列表中
                    res.Add(tmp);
                }
            }
            return View(res);
        }
    }
}