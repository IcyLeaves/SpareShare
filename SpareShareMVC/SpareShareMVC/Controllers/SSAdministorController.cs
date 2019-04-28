using Newtonsoft.Json;
using SpareShareMVC.Filter;
using SpareShareMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SpareShareMVC.Controllers
{
    [MyAuthorize]
    [MyAdmin]
    public class SSAdministorController : Controller
    {
        Entities db = new Entities();
        //var usrId = HttpContext.Session["usrId"];
        //var usrName = HttpContext.Session["usrName"];
        int numPerPage = 12;
        // 
        // GET: SSAdministor
        public ActionResult Index()
        {
            return View();
        }

        // GET: SSAdministor/AssessGivenThings
        public ActionResult AssessGivenThings(int currentPage = 1)
        {
            var ts = db.Things.Where(x => x.Status == "正在审核中");
            int totalThings = ts.Count();//总共条目数目
            ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
            int start = (currentPage - 1) * numPerPage;//开始的条目
            ts = ts.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
            ViewBag.currentPage = currentPage;
            return View(ts.ToList());
        }

        // GET: SSAdministor/DetailsGivenThings
        public ActionResult DetailsGivenThings(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Things t = db.Things.Where(x => x.ThingId == id).FirstOrDefault();

            Users tu = db.Users.Where(x => x.UserId == t.GiverId).FirstOrDefault();
            ViewBag.tUserName = tu.Username;
            ViewBag.tUserId = tu.UserId;
            return View(t);
        }
        // POST: SSAdministor/DetailsGivenThings
        [HttpPost]
        public ActionResult DetailsGivenThings(int? id, string action, string reason)
        {
            bool isAccept = false;
            switch (action)
            {
                case "通过":
                    isAccept = true;
                    break;
                case "拒绝":
                    isAccept = false;
                    break;
                case "取消":
                    return RedirectToAction("AssessGivenThings");
            }
            Checks c = new Checks();
            int cId = 0;
            c.AdminId = (int)HttpContext.Session["usrId"];
            c.Result = isAccept ? 1 : -1;
            c.RefuseReason = reason;
            c.CheckedTime = DateTime.Now;
            using (Entities db = new Entities())
            {
                db.Checks.Add(c);
                db.SaveChanges();
                cId = c.CheckId;
            }

            Things t = db.Things.Where(x => x.ThingId == (int)id).FirstOrDefault();
            t.Status = isAccept ? "物品闲置中" : "审核未通过";
            t.CheckId = cId;
            db.Entry(t).State = EntityState.Modified;

            Assess a = db.Assess.Where(x => x.UserId == t.GiverId).FirstOrDefault();
            a.CheckedNum++;
            a.CreditPoint += 2;
            a.CreditLevel = CreditsCal.getLevel(a.CreditPoint.Value);
            db.Entry(a).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("AssessGivenThings");
        }

        //// GET: SSAdministor/ManageAdministor
        //public ActionResult ManageAdministor()
        //{
        //    int usrId = (int)HttpContext.Session["usrId"];
        //    return View(db.A.Where(a => a.AID == usrId).FirstOrDefault());
        //}

        //public ActionResult EditManageAdministor(int id)
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult EditManageAdministor(SSEditManageAdministorViewModel model, int id)
        //{
        //    A a = db.A.Where(ap => ap.AID == id).FirstOrDefault();
        //    a.Name = model.Name;
        //    a.Sex = model.Sex;
        //    db.Entry(a).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return RedirectToAction("ManageAdministor");
        //}

        // GET: SSAdministor/AssessReceiveThings
        public ActionResult AssessReceiveThings(int currentPage = 1)
        {
            var qs = db.Quests.Where(x => x.Status == "正在审核中");
            int totalThings = qs.Count();//总共条目数目
            ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
            int start = (currentPage - 1) * numPerPage;//开始的条目
            qs = qs.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
            ViewBag.currentPage = currentPage;
            return View(qs.ToList());
        }

        // GET: SSAdministor/DetailsReceiveThings
        public ActionResult DetailsReceiveThings(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quests q = db.Quests.Where(x => x.QuestId == id).FirstOrDefault();

            Users qu = db.Users.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
            ViewBag.qUserId = qu.UserId;
            ViewBag.qUserName = qu.Username;
            return View(q);
        }
        // POST: SSAdministor/DetailsReceiveThings
        [HttpPost]
        public ActionResult DetailsReceiveThings(int? id, string action, string reason)
        {
            bool isAccept = false;
            switch (action)
            {
                case "通过":
                    isAccept = true;
                    break;
                case "拒绝":
                    isAccept = false;
                    break;
                case "取消":
                    return RedirectToAction("AssessReceiveThings");
            }
            Checks c = new Checks();
            int cId = 0;
            c.AdminId = (int)HttpContext.Session["usrId"];
            c.Result = isAccept ? 1 : -1;
            c.RefuseReason = reason;
            c.CheckedTime = DateTime.Now;
            using (Entities db = new Entities())
            {
                db.Checks.Add(c);
                db.SaveChanges();
                cId = c.CheckId;
            }

            Quests q = db.Quests.Where(x => x.QuestId == (int)id).FirstOrDefault();
            q.Status = isAccept ? "等待受助中" : "审核未通过";
            q.CheckId = cId;
            db.Entry(q).State = EntityState.Modified;

            Assess a = db.Assess.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
            a.CheckedNum++;
            a.CreditPoint += 2;
            a.CreditLevel = CreditsCal.getLevel(a.CreditPoint.Value);
            db.Entry(a).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("AssessReceiveThings");
        }

        //GET: SSAdministor/AdminCharts
        public ActionResult AdminCharts()
        {
            ViewBag.resData = "[]";
            return View();
        }

        //POST: SSAdministor/AdminCharts
        [HttpPost]
        public ActionResult AdminCharts(SSAdminChartsViewModel model)
        {
            if(model.StartTime>model.EndTime)
            {
                ModelState.AddModelError("", "起始日期需早于结束日期");
                ViewBag.resData = "[]";
                return View(model);
            }
            var endtime = model.EndTime.AddDays(1);
            var lists = db.ThingsQuests.Where(tq => tq.Time >= model.StartTime && tq.Time <= endtime)
                .Join(db.Things.Select(x => new { x.ThingId, x.GiverId, x.Type, x.Name }), tq => tq.ThingId, t => t.ThingId, (tq, t) => new { tq, t })
                .Join(db.Quests.Select(y => new { y.QuestId, y.ReceiverId, y.Type, y.Name }), tq_t => tq_t.tq.QuestId, q => q.QuestId, (tq_t, q) => new { tq_t, q })
                .Join(db.Users.Select(gz => new { gz.UserId, gz.Username }), tq_t_q => tq_t_q.tq_t.t.GiverId, gu => gu.UserId, (tq_t_q, gu) => new { tq_t_q, gu })
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
                .OrderBy(mx=>mx.DealTime);
            var res = lists.ToList();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var resJson=serializer.Serialize(res);
            ViewBag.resData = resJson;
            ViewBag.Start = localeDateTime(model.StartTime);
            ViewBag.End = localeDateTime(model.EndTime);
            return View(model);
        }
        string localeDateTime(DateTime t)
        {
            string res = "";
            res += t.Year;
            res += " ";
            string months = "";
            switch(t.Month)
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