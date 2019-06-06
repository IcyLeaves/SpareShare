using SpareShare.Filter;
using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SpareShare.Controllers
{
    [MyAuthorize]
    public class AdminController : Controller
    {
        int numPerPage = 12;//每页显示条目
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: 查看待审核的捐赠物品列表
        // 修改时间: 2019年5月3日 15点06分
        public ActionResult CheckThings(int currentPage=1)
        {
            //新建视图模型列表
            var res = new List<ThingsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //所有待审核的捐赠物品
                var ts = db.Things.Where(x => x.Status == "正在审核中");
                //1.分页
                int totalThings = ts.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                ts = ts.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
                ViewBag.currentPage = currentPage;
                //给视图模型赋值
                foreach (var t in ts)
                {
                    //新建一个视图模型并赋值
                    var tmp = new ThingsListViewModel();
                    tmp.Thing = t;
                    //添加到列表中
                    res.Add(tmp);
                }
            }
            return View(res);
        }

        // GET: 查看待审核的受助请求列表
        // 修改时间: 2019年5月3日 15点06分
        public ActionResult CheckQuests(int currentPage=1)
        {
            //新建视图模型列表
            var res = new List<QuestsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //所有待审核的受助请求
                var qs = db.Quests.Where(x => x.Status == "正在审核中");
                //1.分页
                int totalThings = qs.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                qs = qs.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
                ViewBag.currentPage = currentPage;
                //给视图模型赋值
                foreach (var q in qs)
                {
                    //新建一个视图模型并赋值
                    var tmp = new QuestsListViewModel();
                    tmp.Quest = q;
                    //添加到列表中
                    res.Add(tmp);
                }
            }
            return View(res);
        }

        // GET: 显示审核捐赠物品界面
        // 修改时间: 2019年5月3日 14点55分
        public ActionResult ThingsDetail(int id)
        {
            //新建视图模型
            DetailsViewModel res = new DetailsViewModel();
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的捐赠物品
                Things t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                res.Thing = t;

                //找出对应的捐赠者
                Users tu = db.Users.Where(x => x.UserId == t.DonatorId).FirstOrDefault();
                res.Donator = tu;
                return View(res);
            }
        }

        // POST: 提交捐赠物品的审核信息表单
        // 修改时间: 2019年5月6日 14点55分
        [HttpPost]
        public ActionResult ThingsDetail(int id, string action,string reason)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            using (SSDBEntities db = new SSDBEntities())
            {
                switch (action)
                {
                    case "通过":
                        //新建Check元组
                        Checks c1 = new Checks();
                        c1.AdminId = usrId;
                        c1.CheckedTime = DateTime.Now;
                        c1.RefuseReason = "";
                        c1.Result = 1;
                        db.Checks.Add(c1);
                        db.SaveChanges();

                        //找出对应id的捐赠物品
                        var t1 = db.Things.Where(x => x.Id == id).FirstOrDefault();
                        //修改捐赠物品的状态
                        t1.Status = "物品闲置中";
                        t1.CheckId = c1.Id;
                        db.Entry(t1).State = EntityState.Modified;


                        //修改捐赠者的信誉评价
                        var u1 = db.Assess.Where(x => x.UserId == t1.DonatorId).FirstOrDefault();
                        u1.CreditPoint += 2;//信誉点数+2
                        u1.CreditLevel = CreditFunction.GetCreditLevel(u1.CreditPoint.Value);
                        u1.CheckedNum++;
                        db.Entry(u1).State = EntityState.Modified;
                        db.SaveChanges();


                        break;
                    case "提交":
                        //新建Check元组
                        Checks c2 = new Checks();
                        c2.AdminId = usrId;
                        c2.CheckedTime = DateTime.Now;
                        c2.RefuseReason = reason;
                        c2.Result = 0;
                        db.Checks.Add(c2);
                        db.SaveChanges();

                        //找出对应id的捐赠物品
                        var t2 = db.Things.Where(x => x.Id == id).FirstOrDefault();
                        //修改捐赠物品的状态
                        t2.Status = "审核未通过";
                        t2.CheckId = c2.Id;
                        db.Entry(t2).State = EntityState.Modified;

                        //修改捐赠者的信誉评价
                        var u2 = db.Assess.Where(x => x.UserId == t2.DonatorId).FirstOrDefault();
                        u2.CreditPoint -= 2;//信誉点数-2
                        u2.CreditLevel = CreditFunction.GetCreditLevel(u2.CreditPoint.Value);
                        db.Entry(u2).State = EntityState.Modified;
                        db.SaveChanges();

                        break;
                    case "取消":
                        break;
                }
            }
            //跳转至物品列表
            return RedirectToAction("CheckThings");
        }

        // GET: 显示审核受助请求界面
        // 修改时间: 2019年5月3日 14点57分
        public ActionResult QuestsDetail(int id)
        {
            //新建视图模型
            DetailsViewModel res = new DetailsViewModel();
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的受助请求
                Quests q = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                res.Quest = q;

                //找出对应的受助方
                Users qu = db.Users.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
                res.Receiver = qu;
                return View(res);
            }
        }

        // POST: 提交受助请求的审核信息表单
        // 修改时间: 2019年5月6日 14点56分
        [HttpPost]
        public ActionResult QuestsDetail(int id, string action,string reason)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            using (SSDBEntities db = new SSDBEntities())
            {
                switch (action)
                {
                    case "通过":
                        //新建Check元组
                        Checks c1 = new Checks();
                        c1.AdminId = usrId;
                        c1.CheckedTime = DateTime.Now;
                        c1.RefuseReason = "";
                        c1.Result = 1;
                        db.Checks.Add(c1);
                        db.SaveChanges();
                        //找出对应id的受助请求
                        var q1 = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                        //修改受助请求的状态
                        q1.Status = "等待受助中";
                        q1.CheckId = c1.Id;
                        db.Entry(q1).State = EntityState.Modified;
                        
                        //修改受助方的信誉评价
                        var u1 = db.Assess.Where(x => x.UserId == q1.ReceiverId).FirstOrDefault();
                        u1.CreditPoint += 2;//信誉点数+2
                        u1.CreditLevel = CreditFunction.GetCreditLevel(u1.CreditPoint.Value);
                        u1.CheckedNum++;
                        db.Entry(u1).State = EntityState.Modified;
                        db.SaveChanges();


                        break;
                    case "提交":
                        //新建Check元组
                        Checks c2 = new Checks();
                        c2.AdminId = usrId;
                        c2.CheckedTime = DateTime.Now;
                        c2.RefuseReason = reason;
                        c2.Result = 0;
                        db.Checks.Add(c2);
                        db.SaveChanges();
                        //找出对应id的受助请求
                        var q2 = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                        //修改受助请求的状态
                        q2.Status = "审核未通过";
                        q2.CheckId = c2.Id;
                        db.Entry(q2).State = EntityState.Modified;

                        //修改受助方的信誉评价
                        var u2 = db.Assess.Where(x => x.UserId == q2.ReceiverId).FirstOrDefault();
                        u2.CreditPoint -= 2;//信誉点数-2
                        u2.CreditLevel = CreditFunction.GetCreditLevel(u2.CreditPoint.Value);
                        db.Entry(u2).State = EntityState.Modified;
                        db.SaveChanges();

                        break;
                    case "取消":
                        break;
                }
            }
            //跳转至请求列表
            return RedirectToAction("CheckQuests");
        }

        //GET: SSAdministor/AdminCharts
        public ActionResult AdminCharts()
        {
            ViewBag.resData = "[]";
            return View();
        }

        //POST: SSAdministor/AdminCharts
        [HttpPost]
        public ActionResult AdminCharts(AdminChartsViewModel model)
        {
            if (model.StartTime > model.EndTime)
            {
                ModelState.AddModelError("", "起始日期需早于结束日期");
                ViewBag.resData = "[]";
                return View(model);
            }
            using (SSDBEntities db = new SSDBEntities())
            {
                var endtime = model.EndTime.AddDays(1);
                var lists = db.ThingsQuests.Where(tq => tq.Time >= model.StartTime && tq.Time <= endtime)
                    .Join(db.Things.Select(x => new { x.Id, x.DonatorId, x.Type, x.Name }), tq => tq.ThingId, t => t.Id, (tq, t) => new { tq, t })
                    .Join(db.Quests.Select(y => new { y.Id, y.ReceiverId, y.Type, y.Name }), tq_t => tq_t.tq.QuestId, q => q.Id, (tq_t, q) => new { tq_t, q })
                    .Join(db.Users.Select(gz => new { gz.UserId, gz.Username }), tq_t_q => tq_t_q.tq_t.t.DonatorId, gu => gu.UserId, (tq_t_q, gu) => new { tq_t_q, gu })
                    .Join(db.Users.Select(rz => new { rz.UserId, rz.Username }), tq_t_q_gu => tq_t_q_gu.tq_t_q.q.ReceiverId, ru => ru.UserId, (tq_t_q_gu, ru) => new { tq_t_q_gu, ru })
                    .Select(m => new
                    {
                        ThingName = m.tq_t_q_gu.tq_t_q.tq_t.t.Name,
                        ThingType = m.tq_t_q_gu.tq_t_q.tq_t.t.Type,
                        GiverName = m.tq_t_q_gu.gu.Username,
                        QuestName = m.tq_t_q_gu.tq_t_q.q.Name,
                        QuestType = m.tq_t_q_gu.tq_t_q.q.Type,
                        ReceiverName = m.ru.Username,
                        DealTime = m.tq_t_q_gu.tq_t_q.tq_t.tq.Time
                    })
                    .OrderBy(mx => mx.DealTime);
                var res = lists.ToList();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var resJson = serializer.Serialize(res);
                ViewBag.resData = resJson;
                ViewBag.Start = localeDateTime(model.StartTime);
                ViewBag.End = localeDateTime(model.EndTime);
                return View(model);
            }
        }
        string localeDateTime(DateTime t)
        {
            string res = "";
            res += t.Year;
            res += " ";
            string months = "";
            switch (t.Month)
            {
                case 1:
                    months = "一月";
                    break;
                case 2:
                    months = "二月";
                    break;
                case 3:
                    months = "三月";
                    break;
                case 4:
                    months = "四月";
                    break;
                case 5:
                    months = "五月";
                    break;
                case 6:
                    months = "六月";
                    break;
                case 7:
                    months = "七月";
                    break;
                case 8:
                    months = "八月";
                    break;
                case 9:
                    months = "九月";
                    break;
                case 10:
                    months = "十月";
                    break;
                case 11:
                    months = "十一月";
                    break;
                case 12:
                    months = "十二月";
                    break;
            }
            res += months;
            res += " ";
            if (t.Day < 10) res += "0";
            res += t.Day;
            return res;
        }

    }
}