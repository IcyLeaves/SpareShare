using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        // 修改时间: 2019年5月8日 16点03分
        public ActionResult UploadThings(int? id)
        {
            if (id != null)
            {
                using (SSDBEntities db = new SSDBEntities())
                {
                    //查找id对应的受助请求
                    Quests q = db.Quests.Where(x => x.Id == id.Value).FirstOrDefault();
                    //新建视图模型
                    UploadThingsViewModel res = new UploadThingsViewModel();
                    //给视图模型赋值
                    res.qId = q.Id;
                    res.qName = q.Name;
                    res.qType = q.Type;
                    res.qDetail = q.Detail;
                    res.tName = q.Name;
                    res.tType = q.Type;
                    return View(res);
                }
            }
            return View();
        }

        // POST: 提交表单，上传捐赠物品信息
        // 修改时间: 2019年5月8日 18点23分
        [HttpPost]
        public ActionResult UploadThings(UploadThingsViewModel model)
        {
            //获取当前用户的Id
            int usrId = (int)HttpContext.Session["usrId"];
            int tId = 0;
            if (ModelState.IsValid)
            {
                //新建Things
                Things t = new Things();
                t.DonatorId = usrId;
                t.Name = model.tName;
                t.Type = model.tType;
                t.Detail = model.tDetail;
                t.Status = model.qId > 0 ? "已接受捐赠" : "正在审核中";//如果当前有匹配受助请求，则跳过审核
                //向数据库新建元组，并获取新Things的id
                using (SSDBEntities db = new SSDBEntities())
                {
                    db.Things.Add(t);
                    db.SaveChanges();
                    tId = t.Id;
                }
                //如果当前有匹配受助请求，再新建一条匹配元组
                //并改变受助请求状态
                if (model.qId > 0)
                {
                    //新建ThingsQuests
                    ThingsQuests tq = new ThingsQuests();
                    tq.ThingId = tId;
                    tq.QuestId = model.qId;
                    //向数据库新建元组，改变受助请求状态
                    using (SSDBEntities db = new SSDBEntities())
                    {
                        db.ThingsQuests.Add(tq);

                        Quests q = db.Quests.Where(x => x.Id == model.qId).FirstOrDefault();
                        q.Status = "已接受受助";
                        db.Entry(q).State = EntityState.Modified;
                        db.SaveChanges();
                    }
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

        // POST: 接受其他用户的受助请求
        // 修改时间: 2019年5月8日 13点58分
        [HttpPost]
        public ActionResult QuestsDetail(int id, string action)
        {
            switch (action)
            {
                case "接受":
                    //跳转至匹配发布界面，id为对应请求的id
                    return RedirectToAction("UploadThings", new { id = id });
                case "返回":
                    //跳转回请求列表
                    return RedirectToAction("SearchQuests");
            }
            return View();
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
                    tmp.QuestId = q.Id;
                    tmp.Status = q.Status;
                    //添加到列表中
                    res.Add(tmp);
                }
            }
            return View(res);
        }
    }
}