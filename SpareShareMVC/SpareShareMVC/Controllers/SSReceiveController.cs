using SpareShareMVC.Filter;
using SpareShareMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SpareShareMVC.Controllers
{
    [MyAuthorize]
    public class SSReceiveController : Controller
    {
        Entities db = new Entities();
        //var usrId = HttpContext.Session["usrId"];
        //var usrName = HttpContext.Session["usrName"];
        int numPerPage = 12;
        // 
        // GET: SSReceive 受助方首页
        public ActionResult Index()
        {
            return View();
        }

        // GET: SSReceive/SearchGivenThings 查询捐赠物品 searchString=查询字符串
        public ActionResult SearchGivenThings(string searchString,int currentPage=1)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            List<string> sWords = new List<string>();//近义词
            List<string> oWords = new List<string>();//原词


            var ts = db.Things.Where(x => x.Status == "物品闲置中" && x.GiverId != usrId);//基本结果
            //地域置顶
            var user = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
            var pro = user.Province;
            var sch = user.School;
            var ts_u = ts.Join(db.Users.Select(x => new { x.UserId, x.Province, x.School }), t => t.GiverId, u => u.UserId, (t, u) => new { t, u });
            var tsu1 = ts_u.Where(o => o.u.Province.Contains(pro) && o.u.School.Contains(sch)).Select(n => new SSSeachGivenThingsViewModel { Thing = n.t, prior = 300 }).ToList();//同省同校
            var tsu2 = ts_u.Where(o => o.u.Province.Contains(pro) && !o.u.School.Contains(sch)).Select(n => new SSSeachGivenThingsViewModel { Thing = n.t, prior = 200 }).ToList();//同校不同省
            var tsu3 = ts_u.Where(o => !o.u.Province.Contains(pro)).Select(n => new SSSeachGivenThingsViewModel { Thing = n.t, prior = 100 }).ToList();//不同省
            var ts_place = tsu1.Union(tsu2).Union(tsu3);//已转化为list的视图模型，之后关键字直接foreach匹配
            //获取同义分词 AI
            if (!string.IsNullOrEmpty(searchString))
            {
                string data = TencentAI.tencentAI(searchString);
                oWords = TencentAI.oriWords(data);
                sWords = TencentAI.synWords(data);
                //qs = qs.Where(x => x.Name.Contains(searchString));
                //关键字匹配
                foreach (var l in ts_place)
                {
                    if (l.Thing.Name.Contains(searchString))
                    {
                        l.prior += 10;
                        continue;
                    }
                    for (int i = 0; i < oWords.Count; i++)
                    {
                        if (l.Thing.Name.Contains(oWords[i]))
                        {
                            l.prior += 5;
                            break;
                        }
                    }
                    for (int j = 0; j < sWords.Count; j++)
                    {
                        if (l.Thing.Name.Contains(sWords[j]))
                        {
                            l.prior += 1;
                            break;
                        }
                    }
                    if(l.prior%100==0)
                    {
                        l.prior -= 1000;
                    }
                }
            }
            ts_place = ts_place.Where(x => x.prior >= 0);
            int totalThings = ts_place.Count();//总共条目数目
            ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
            int start = (currentPage - 1) * numPerPage;//开始的条目
            ts_place = ts_place.OrderByDescending(x => x.prior).ThenByDescending(x => x.Thing.ReleaseTime).Skip(start).Take(numPerPage);
            ViewBag.searchString = searchString;
            ViewBag.currentPage = currentPage;
            return View(ts_place);
        }

        // GET: SSReceive/DetailsGivenThings/id 捐赠物品详细信息页（查看信息） id=GTID
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
        // POST: SSReceive/DetailsGivenThings/id 捐赠物品详细信息页（接受捐赠或返回列表） id=GTID action={取消,接受}
        [HttpPost]
        public ActionResult DetailsGivenThings(int? id, string action)
        {
            if (action == "取消")
                return RedirectToAction("SearchGivenThings");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("UploadReceiveThings", new { id });
        }

        //// GET: SSReceive/ManageReceivePeople 受助方个人信息
        //public ActionResult ManageReceivePeople()
        //{
        //    int usrId = (int)HttpContext.Session["usrId"];
        //    return View(db.RP.Where(rp => rp.RPID == usrId).FirstOrDefault());
        //}

        //// GET: SSReceive/EditManageReceivePeople 编辑受助方个人信息（显示表单） id=RPID
        //public ActionResult EditManageReceivePeople(int id)
        //{
        //    return View();
        //}

        //// POST: SSReceive/EditManageReceivePeople 编辑受助方个人信息（提交表单） model=表单数据 id=RPID
        //[HttpPost]
        //public ActionResult EditManageReceivePeople(SSEditManageReceivePeopleViewModel model, int id)
        //{
        //    RP rp = db.RP.Where(r => r.RPID == id).FirstOrDefault();
        //    rp.Name = model.Name;
        //    rp.Sex = model.Sex;
        //    rp.Email = model.Email;
        //    rp.Tel = model.Tel;
        //    db.Entry(rp).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return RedirectToAction("ManageReceivePeople");
        //}

        //// GET: SSReceive/SelectHistoryThings 确认领取物品 searchString=查询字符串
        //public ActionResult SelectHistoryThings(string searchString)
        //{
        //    int usrId = (int)HttpContext.Session["usrId"];
        //    //查询所有自己的请求
        //    var qs = db.Quests.Where(x => x.ReceiverId == usrId).Select(q => q.QuestId);
        //    //查询这些请求所有的
        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        ts = ts.Where(x => x.Name.Contains(searchString));
        //    }
        //    return View(ts.ToList());
        //}

        // GET: SSReceive/UploadReceiveThings 发布受助请求（显示表单）
        public ActionResult UploadReceiveThings(int? id)
        {
            ViewBag.tId = id;
            if (id != null)
            {
                var t = db.Things.Where(x => x.ThingId == id).FirstOrDefault();
                ViewBag.tName = t.Name;
                ViewBag.tType = t.Type;
                ViewBag.tReason = t.Reason;
                ViewBag.tPhoto = t.PhotoURL;
                ViewBag.tTime = t.ReleaseTime;

                Users tu = db.Users.Where(x => x.UserId == t.GiverId).FirstOrDefault();
                ViewBag.tUserName = tu.Username;
                ViewBag.tUserId = tu.UserId;
            }
            return View();
        }
        // POST: SSReceive/UploadReceiveThings 发布受助请求（提交表单） model=表单数据
        [HttpPost]
        public ActionResult UploadReceiveThings(SSUploadReceiveThingsViewModel model, int? id)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            if (ModelState.IsValid)
            {
                Quests q = new Quests();
                q.ReceiverId = usrId;
                //q.CheckId=NULL;
                q.Name = model.Name;
                q.Reason = model.Reason;
                q.Type = model.Type;
                q.ReleaseTime = DateTime.Now;
                q.Status = id == null ? "正在审核中" : "已接受受助";

                int qId = 0;
                using (Entities db = new Entities())
                {
                    db.Quests.Add(q);
                    db.SaveChanges();
                    qId = q.QuestId;
                }
                if (id != null)
                {
                    Things t = db.Things.Where(x => x.ThingId == (int)id).FirstOrDefault();
                    t.Status = "已接受捐赠";
                    db.Entry(t).State = EntityState.Modified;

                    ThingsQuests tq = new ThingsQuests();
                    tq.QuestId = qId;
                    tq.ThingId = (int)id;
                    tq.Time = DateTime.Now;
                    db.ThingsQuests.Add(tq);
                    db.SaveChanges();
                    return RedirectToAction("CompleteReceiveThings", new { id = qId });
                }
                return RedirectToAction("SelectReceiveThings", "SSReceive");
            }
            return View(model);
        }

        //GET: SSReceive/SelectReceiveThings 查看我的受助 
        public ActionResult SelectReceiveThings(string searchString,string status,int currentPage=1)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            var qs = db.Quests.Where(x => x.ReceiverId == usrId);
            if (!string.IsNullOrEmpty(searchString))
            {
                qs = qs.Where(x => x.Name.Contains(searchString));
            }
            switch (status)
            {
                case "All":
                    break;
                case "1":
                    qs = qs.Where(x => x.Status == "正在审核中");
                    break;
                case "2":
                    qs = qs.Where(x => x.Status == "审核未通过");
                    break;
                case "3":
                    qs = qs.Where(x => x.Status == "等待受助中");
                    break;
                case "4":
                    qs = qs.Where(x => x.Status == "已接受受助");
                    break;
                case "5":
                    qs = qs.Where(x => x.Status == "受助已成功");
                    break;
            }
            int totalThings = qs.Count();//总共条目数目
            ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
            int start = (currentPage - 1) * numPerPage;//开始的条目
            qs = qs.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
            ViewBag.searchString = searchString;
            ViewBag.status = status;
            ViewBag.currentPage = currentPage;
            return View(qs.ToList());
        }

        // GET: SSReceive/DetailsReceiveThings/id 受助请求详细信息页（查看信息） id=RTID
        public ActionResult DetailsReceiveThings(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quests q = db.Quests.Where(x => x.QuestId == id).FirstOrDefault();
            Users qu = db.Users.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
            ViewBag.qUserName = qu.Username;
            ViewBag.qUserId = qu.UserId;
            if (q.Status == "审核未通过")
            {
                Checks check = db.Checks.Where(x => x.CheckId == q.CheckId).FirstOrDefault();
                ViewBag.RefuseReason = check.RefuseReason;
            }
            if (q.Status == "已接受受助")
            {
                return RedirectToAction("CompleteReceiveThings", new { id });
            }
            else if (q.Status == "受助已成功")
            {
                ThingsQuests tq = db.ThingsQuests.Where(x => x.QuestId == q.QuestId).FirstOrDefault();
                Things t = db.Things.Where(x => x.ThingId == tq.ThingId).FirstOrDefault();
                ViewBag.tId = t.ThingId;
                ViewBag.tName = t.Name;
                ViewBag.tType = t.Type;
                ViewBag.tReason = t.Reason;
                ViewBag.tPhoto = t.PhotoURL;
                ViewBag.tTime = t.ReleaseTime;

                Users tu = db.Users.Where(x => x.UserId == t.GiverId).FirstOrDefault();
                ViewBag.tUserName = tu.Username;
                ViewBag.tUserId = tu.UserId;

                Comments c = db.Comments.Where(x => x.ThingId == t.ThingId).FirstOrDefault();
                if(c!=null)
                {
                    ViewBag.cReceiverId = c.ReceiverId;
                    ViewBag.cNew = c.NewScore;
                    ViewBag.cSimilar = c.SimilarScore;
                    ViewBag.cUseful = c.UsefulScore;
                    ViewBag.cSpeed = c.SpeedScore;
                    ViewBag.cBeautiful = c.BeautifulScore;
                    ViewBag.cText = c.Text;
                    ViewBag.cTime = c.CommentTime;
                }
            }
            return View(q);
        }
        // POST: SSReceive/DetailsReceiveThings/id 受助请求详细信息页（删除请求或返回列表） id=RTID action={删除,返回到列表}
        [HttpPost]
        public ActionResult DetailsReceiveThings(int? id, string action)
        {
            switch (action)
            {
                case "评价物品":
                    ThingsQuests tq = db.ThingsQuests.Where(x => x.QuestId == id).FirstOrDefault();
                    Things t = db.Things.Where(x => x.ThingId == tq.ThingId).FirstOrDefault();
                    return RedirectToAction("UploadComment", new { id = t.ThingId });
                case "删除":
                    Quests q = db.Quests.Where(x => x.QuestId == id).FirstOrDefault();
                    db.Quests.Remove(q);

                    Checks c = db.Checks.Where(x => x.CheckId == q.ReceiverId).FirstOrDefault();
                    if(c!=null)
                    {
                        db.Checks.Remove(c);
                    }
                    Assess a = db.Assess.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
                    a.CreditPoint -= 2;
                    a.CreditLevel = CreditsCal.getLevel(a.CreditPoint.Value);
                    db.Entry(a).State = EntityState.Modified;
                    db.SaveChanges();
                    break;
                case "返回到列表":
                    return RedirectToAction("SelectReceiveThings");
            }
            
            return RedirectToAction("SelectReceiveThings");
        }

        // GET: SSReceive/CompleteReceiveThings/id 受助请求确认完成（查看信息） id=RTID
        public ActionResult CompleteReceiveThings(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var q = db.Quests.Where(x => x.QuestId == id).FirstOrDefault();

            Users qu = db.Users.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
            ViewBag.qUserId = qu.UserId;
            ViewBag.qUserName = qu.Username;
            var ts = db.Things.Where(t =>
            (db.ThingsQuests.Where(tq => tq.QuestId == id)).Select(qgx => qgx.ThingId).Contains(t.ThingId));
            if (ts.FirstOrDefault() != null)
            {
                Things data = ts.FirstOrDefault();
                ViewBag.tId = data.ThingId;
                ViewBag.tName = data.Name;
                ViewBag.tType = data.Type;
                ViewBag.tReason = data.Reason;
                ViewBag.tPhoto = data.PhotoURL;
                ViewBag.tTime = data.ReleaseTime;

                Users tu = db.Users.Where(x => x.UserId == data.GiverId).FirstOrDefault();
                ViewBag.tUserName = tu.Username;
                ViewBag.tUserId = tu.UserId;
            }
            return View(q);
        }
        // POST: SSReceive/CompleteReceiveThings/id 受助请求确认完成（确认送达或返回列表） id=RTID
        [HttpPost]
        public ActionResult CompleteReceiveThings(int? id, string action)
        {
            switch (action)
            {
                case "确认送达":
                    break;
                case "返回到列表":
                    return RedirectToAction("SelectReceiveThings");
            }
            Quests q = db.Quests.Where(x => x.QuestId == id).FirstOrDefault();
            q.Status = "受助已成功";
            db.Entry(q).State = EntityState.Modified;

            ThingsQuests tq = db.ThingsQuests.Where(x => x.QuestId == q.QuestId).FirstOrDefault();
            Things t = db.Things.Where(x => x.ThingId == tq.ThingId).FirstOrDefault();
            t.Status = "捐赠已成功";
            db.Entry(t).State = EntityState.Modified;

            Assess aq = db.Assess.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
            aq.ReceiveNum++;
            aq.CreditPoint += 1;
            aq.CreditLevel = CreditsCal.getLevel(aq.CreditPoint.Value);
            Assess ag = db.Assess.Where(x => x.UserId == t.GiverId).FirstOrDefault();
            ag.GiveNum++;
            ag.CreditPoint += 5;
            ag.CreditLevel = CreditsCal.getLevel(ag.CreditPoint.Value);
            db.Entry(aq).State = EntityState.Modified;
            db.Entry(ag).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("UploadComment",new { id=t.ThingId });
        }

        //// GET: SSReceive/CompleteGivenThings/id 捐赠物品确认完成（查看信息） id=GTID
        //public ActionResult CompleteGivenThings(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    return View(db.Things.Where(x => x.ThingId == id).FirstOrDefault());
        //}
        //// POST: SSReceive/CompleteGivenThings/id 捐赠物品确认完成（确认送达或放弃接受或返回列表） id=GTID action={"放弃接受","确认送达","返回到列表"}
        //[HttpPost]
        //public ActionResult CompleteGivenThings(int? id, string action)
        //{
        //    switch (action)
        //    {
        //        case "放弃接受":
        //            return RedirectToAction("CancelGivenThings");
        //        case "确认送达":
        //            break;
        //        case "返回到列表":
        //            return RedirectToAction("SelectHistoryThings");
        //    }
        //    Things t = db.Things.Where(x => x.ThingId == id).FirstOrDefault();
        //    t.Status = "捐赠已成功";
        //    db.Entry(t).State = EntityState.Modified;

        //    Assess a = db.Assess.Where(x => x.UserId == t.GiverId).FirstOrDefault();
        //    a.SucceedNum++;
        //    db.Entry(a).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return RedirectToAction("SelectHistoryThings");
        //}

        // GET: SSReceive/UploadComment/id 评论捐赠物品 id=物品id
        public ActionResult UploadComment(int id)
        {
            ViewBag.tId = id;
            var t = db.Things.Where(x => x.ThingId == id).FirstOrDefault();
            ViewBag.tName = t.Name;
            ViewBag.tType = t.Type;
            ViewBag.tReason = t.Reason;
            ViewBag.tPhoto = t.PhotoURL;
            ViewBag.tTime = t.ReleaseTime;

            Users tu = db.Users.Where(x => x.UserId == t.GiverId).FirstOrDefault();
            ViewBag.tUserName = tu.Username;
            ViewBag.tUserId = tu.UserId;

            return View();
        }
        // POST: SSReceive/UploadComment/id 评论捐赠物品 id=物品id
        [HttpPost]
        public ActionResult UploadComment(SSUploadCommentViewModel model,int id)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            if (ModelState.IsValid)
            {
                Comments c = new Comments();
                c.CommentTime = DateTime.Now;
                c.NewScore = model.NewScore;
                c.ReceiverId = usrId;
                c.SimilarScore = model.SimilarScore;
                c.Text = model.Text;
                c.ThingId = id;
                c.UsefulScore = model.UsefulScore;
                c.BeautifulScore = model.BeautifulScore;
                c.SpeedScore = model.SpeedScore;
                db.Comments.Add(c);

                Assess a = db.Assess.Where(x => x.UserId == usrId).FirstOrDefault();
                a.CreditPoint += 1;
                a.CreditLevel = CreditsCal.getLevel(a.CreditPoint.Value);
                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                return View(model);
            }
            return RedirectToAction("SelectReceiveThings");
        }

        //// GET: SSReceive/CancelGivenThings 取消接受物品 id=GTID
        //public ActionResult CancelGivenThings(int id)
        //{
        //    Things t = db.Things.Where(x => x.ThingId == (int)id).FirstOrDefault();
        //    t.Status = "物品闲置中";
        //    db.Entry(t).State = EntityState.Modified;

        //    int usrId = (int)HttpContext.Session["usrId"];
        //    ThingsQuests tq = db.ThingsQuests.Where(x => x.ReceiverId == usrId && x.ThingId == t.ThingId).FirstOrDefault();
        //    db.ThingsQuests.Remove(tq);
        //    db.SaveChanges();
        //    return RedirectToAction("SelectHistoryThings");
        //}

        //// GET: SSReceive/CreditReceivePeople/id 受助方个人信誉评价 id=RPID
        //public ActionResult CreditReceivePeople(int? id)
        //{
        //    if (id == null)
        //    {
        //        id = (int)HttpContext.Session["usrId"];
        //    }
        //    ARP arp = db.ARP.Where(ar => ar.RPID == id).FirstOrDefault();
        //    return View(arp);
        //}
    }
}