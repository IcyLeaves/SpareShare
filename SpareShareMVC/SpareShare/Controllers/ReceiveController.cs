using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    public class ReceiveController : Controller
    {
        // GET: 显示发布请求的表单
        // 修改时间: 2019年5月8日 16点09分
        public ActionResult UploadQuests(int? id)
        {
            if (id != null)
            {
                using (SSDBEntities db = new SSDBEntities())
                {
                    //查找id对应的捐赠物品
                    Things t = db.Things.Where(x => x.Id == id.Value).FirstOrDefault();
                    //新建视图模型
                    UploadQuestsViewModel res = new UploadQuestsViewModel();
                    //给视图模型赋值
                    res.tId = t.Id;
                    res.tName = t.Name;
                    res.tType = t.Type;
                    res.tDetail = t.Detail;
                    res.qName = t.Name;
                    res.qType = t.Type;
                    return View(res);
                }
            }
            return View();
        }

        // POST: 提交表单，发布受助请求信息
        // 修改时间: 2019年5月8日 16点11分
        [HttpPost]
        public ActionResult UploadQuests(UploadQuestsViewModel model)
        {
            //获取当前用户的Id
            int usrId = (int)HttpContext.Session["usrId"];
            int qId = 0;
            if (ModelState.IsValid)
            {
                //新建Quests
                Quests q = new Quests();
                q.ReceiverId = usrId;
                q.Name = model.qName;
                q.Type = model.qType;
                q.Detail = model.qDetail;
                q.Status = model.tId > 0 ? "已接受受助" : "正在审核中";
                //向数据库新建元组，并获取新Quests的id
                using (SSDBEntities db = new SSDBEntities())
                {
                    db.Quests.Add(q);
                    db.SaveChanges();
                    qId = q.Id;
                }
                //如果当前有匹配捐赠物品，再新建一条匹配元组
                if (model.tId > 0)
                {
                    //新建ThingsQuests
                    ThingsQuests tq = new ThingsQuests();
                    tq.ThingId = model.tId;
                    tq.QuestId = qId;
                    //向数据库新建元组
                    using (SSDBEntities db = new SSDBEntities())
                    {
                        db.ThingsQuests.Add(tq);

                        Things t = db.Things.Where(x => x.Id == model.tId).FirstOrDefault();
                        t.Status = "已接受捐赠";
                        db.Entry(t).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                //提交成功，跳转至用户主页
                return RedirectToAction("Index", "Home");
            }
            //模型报错，返回当前页面
            return View(model);
        }

        // GET: 显示用户受助请求列表
        // 修改时间: 2019年5月3日 13点23分
        public ActionResult MyQuestsList()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<QuestsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找该用户发布的所有受助请求
                var qs = db.Quests.Where(x => x.ReceiverId == usrId).ToList();
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

        // GET: 显示受助请求详细信息
        // 修改时间: 2019年5月3日 13点02分
        public ActionResult QuestsDetail(int id)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的受助请求
                var q = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                return View(q);
            }
        }

        // POST: 提交对受助请求的操作
        // 修改时间: 2019年5月12日 13点55分
        [HttpPost]
        public ActionResult QuestsDetail(int id,string action)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                //找到id对应的Quests
                Quests q = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                switch (action)
                {
                    case "确认送达":
                        //找到匹配的ThingId
                        ThingsQuests tq = db.ThingsQuests.Where(x => x.QuestId == id).FirstOrDefault();
                        var tId = tq.ThingId;
                        //找到ThingId对应的Things
                        Things t = db.Things.Where(x => x.Id == tId).FirstOrDefault();
                        //修改Things状态
                        t.Status = "捐赠已完成";
                        db.Entry(t).State = EntityState.Modified;
                        //修改Quests状态
                        q.Status = "受助已完成";
                        db.Entry(q).State = EntityState.Modified;
                        //执行操作
                        db.SaveChanges();
                        return RedirectToAction("MyQuestsList");
                    case "删除":
                        //从数据库中移除Quests元组
                        db.Quests.Remove(q);
                        db.SaveChanges();
                        return RedirectToAction("MyQuestsList");
                    case "返回":
                        //跳转至“我的受助请求”
                        return RedirectToAction("MyQuestsList");
                }
                return View(q);
            }
        }

        // GET: 显示捐赠物品详细信息
        // 修改时间: 2019年5月6日 20点00分
        public ActionResult ThingsDetail(int id)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的捐赠物品
                var t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                return View(t);
            }
        }

        // POST: 接受其他用户的受助请求
        // 修改时间: 2019年5月8日 16点18分
        [HttpPost]
        public ActionResult ThingsDetail(int id, string action)
        {
            switch (action)
            {
                case "接受":
                    //跳转至匹配发布界面，id为对应请求的id
                    return RedirectToAction("UploadQuests", new { id = id });
                case "返回":
                    //跳转回请求列表
                    return RedirectToAction("SearchThings");
            }
            return View();
        }

        // GET: 查询其他用户的捐赠物品
        // 修改时间: 2019年5月9日 13点18分
        public ActionResult SearchThings()
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<ThingsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找[其他用户的][闲置的]物品
                var ts = db.Things.Where(x => x.DonatorId != usrId && x.Status == "物品闲置中");
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
    }
}