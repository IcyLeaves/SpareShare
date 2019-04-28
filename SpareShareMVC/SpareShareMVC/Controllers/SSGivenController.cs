using SpareShareMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SpareShareMVC.Filter;
using System.Web.Hosting;


namespace SpareShareMVC.Controllers
{
    [MyAuthorize]
    public class SSGivenController : Controller
    {
        Entities db = new Entities();
        //var usrId = HttpContext.Session["usrId"];
        //var usrName = HttpContext.Session["usrName"];
        int numPerPage = 12;

        // GET: SSGiven
        public ActionResult Index()
        {
            return View();
        }

        // GET: SSGiven/UploadGivenThings
        public ActionResult UploadGivenThings(int? id)
        {
            ViewBag.qId = id;
            if (id != null)
            {
                Quests q = db.Quests.Where(x => x.QuestId == id).FirstOrDefault();
                ViewBag.qName = q.Name;
                ViewBag.qType = q.Type;
                ViewBag.qReason = q.Reason;
                ViewBag.qTime = q.ReleaseTime;

                Users qu = db.Users.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
                ViewBag.qUserId = qu.UserId;
                ViewBag.qUserName = qu.Username;
            }
            return View();
        }
        // POST: SSGiven/UploadGivenThings/id 匹配受助请求上传捐赠物品 id=QuestId
        [HttpPost]
        public ActionResult UploadGivenThings(SSUploadGivenThingsViewModel model, int? id)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            if (ModelState.IsValid)
            {
                Things t = new Things();
                t.GiverId = usrId;
                //t.CheckId=NULL;
                t.Name = model.Name;
                t.Reason = model.Reason;
                t.Type = model.Type;
                t.Status = id == null ? "正在审核中" : "已接受捐赠";
                t.ReleaseTime = DateTime.Now;
                foreach (var file in model.Image)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        t.PhotoURL = Save(file, "ThingsImages");
                    }
                }
                int tId = 0;
                using (Entities db = new Entities())
                {
                    db.Things.Add(t);
                    db.SaveChanges();
                    tId = t.ThingId;
                }
                if (id != null)
                {
                    Quests q = db.Quests.Where(x => x.QuestId == (int)id).FirstOrDefault();
                    q.Status = "已接受受助";
                    db.Entry(q).State = EntityState.Modified;

                    ThingsQuests qg = new ThingsQuests();
                    qg.QuestId = (int)id;
                    qg.ThingId = tId;
                    qg.Time = DateTime.Now;
                    db.ThingsQuests.Add(qg);
                    db.SaveChanges();
                    return RedirectToAction("DetailsGivenThings", new { id = tId });
                }
                return RedirectToAction("SelectGivenThings", "SSGiven");
            }
            return View(model);
        }
        private static string Save(HttpPostedFileBase file, string path)
        {
            var root = "~/Upload/" + path + "/";
            var phicyPath = HostingEnvironment.MapPath(root);
            Directory.CreateDirectory(phicyPath);
            var fileName = file.FileName;
            var filePath = phicyPath + fileName;
            while (System.IO.File.Exists(filePath))
            {
                Random rand = new Random();
                fileName = rand.Next().ToString() + "-" + file.FileName;
                filePath = phicyPath + fileName;
            }
            file.SaveAs(filePath);
            return fileName;
        }

        // GET: SSGiven/SelectGivenThings
        public ActionResult SelectGivenThings(string searchString, string status, int currentPage = 1)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            var ts = db.Things.Where(x => x.GiverId == usrId);
            if (!string.IsNullOrEmpty(searchString))
            {
                ts = ts.Where(x => x.Name.Contains(searchString));
            }
            switch (status)
            {
                case "All":
                    break;
                case "1":
                    ts = ts.Where(x => x.Status == "正在审核中");
                    break;
                case "2":
                    ts = ts.Where(x => x.Status == "审核未通过");
                    break;
                case "3":
                    ts = ts.Where(x => x.Status == "物品闲置中");
                    break;
                case "4":
                    ts = ts.Where(x => x.Status == "已接受捐赠");
                    break;
                case "5":
                    ts = ts.Where(x => x.Status == "捐赠已成功");
                    break;
            }
            int totalThings = ts.Count();//总共条目数目
            ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
            int start = (currentPage - 1) * numPerPage;//开始的条目
            ts = ts.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
            ViewBag.searchString = searchString;
            ViewBag.status = status;
            ViewBag.currentPage = currentPage;
            return View(ts.ToList());
        }

        // GET: SSGiven/DetailsGivenThings/id
        public ActionResult DetailsGivenThings(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var t = db.Things.Where(x => x.ThingId == id).FirstOrDefault();

            Users tu = db.Users.Where(x => x.UserId == t.GiverId).FirstOrDefault();
            ViewBag.tUserId = tu.UserId;
            ViewBag.tUserName = tu.Username;
            if (t.Status == "审核未通过")
            {
                Checks check = db.Checks.Where(x => x.CheckId == t.CheckId).FirstOrDefault();
                ViewBag.RefuseReason = check.RefuseReason;
            }
            var qs = db.Quests.Where(q =>
            (db.ThingsQuests.Where(tr => tr.ThingId == id)).Select(trx => trx.QuestId).Contains(q.QuestId));
            if (qs.FirstOrDefault() != null)
            {
                Quests data = qs.FirstOrDefault();
                ViewBag.qId = data.QuestId;
                ViewBag.qName = data.Name;
                ViewBag.qType = data.Type;
                ViewBag.qReason = data.Reason;
                ViewBag.qTime = data.ReleaseTime;

                Users qu = db.Users.Where(x => x.UserId == data.ReceiverId).FirstOrDefault();
                ViewBag.qUserId = qu.UserId;
                ViewBag.qUserName = qu.Username;

                Comments c = db.Comments.Where(x => x.ThingId == id).FirstOrDefault();
                if (c != null)
                {
                    ViewBag.cReceiverId = c.ReceiverId;
                    ViewBag.cNew = c.NewScore;
                    ViewBag.cSimilar = c.SimilarScore;
                    ViewBag.cUseful = c.UsefulScore;
                    ViewBag.cText = c.Text;
                    ViewBag.cTime = c.CommentTime;
                }
            }
            return View(t);
        }
        // POST: SSGiven/DetailsGivenThings/id
        [HttpPost]
        public ActionResult DetailsGivenThings(int? id, string action)
        {
            switch (action)
            {
                case "删除":
                    break;
                case "返回到列表":
                    return RedirectToAction("SelectGivenThings");
            }

            Things t = db.Things.Where(x => x.ThingId == id).FirstOrDefault();

            if (t.PhotoURL != null)
            {
                var filename = t.PhotoURL;
                var root = "~/Upload/ThingsImages/" + filename;
                var phicyPath = HostingEnvironment.MapPath(root);
                System.IO.File.Delete(phicyPath);
            }
            db.Things.Remove(t);

            Checks c = db.Checks.Where(x => x.CheckId == t.CheckId).FirstOrDefault();
            if (c != null)
            {
                db.Checks.Remove(c);
            }
            Assess a = db.Assess.Where(x => x.UserId == t.CheckId).FirstOrDefault();
            a.CreditPoint -= 2;
            a.CreditLevel = CreditsCal.getLevel(a.CreditPoint.Value);
            db.Entry(a).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("SelectGivenThings");
        }

        // GET: SSGiven/SearchReceiveThings 查询受助请求 searchString=查询字符串
        public ActionResult SearchReceiveThings(string searchString, int currentPage = 1)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            List<string> sWords=new List<string>();//近义词
            List<string> oWords = new List<string>();//原词


            var qs = db.Quests.Where(x => x.Status == "等待受助中" && x.ReceiverId != usrId);//基本结果
            //地域置顶
            var user = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
            var pro = user.Province;
            var sch = user.School;
            var qs_u = qs.Join(db.Users.Select(x => new { x.UserId, x.Province, x.School }), q => q.ReceiverId, u => u.UserId, (q, u) => new { q, u });
            var qsu1 = qs_u.Where(o => o.u.Province.Contains(pro) && o.u.School.Contains(sch)).Select(n=>new SSSearchReceiveThingsViewModel{ Quest=n.q, prior=300 }).ToList();//同省同校
            var qsu2 = qs_u.Where(o => o.u.Province.Contains(pro) && !o.u.School.Contains(sch)).Select(n => new SSSearchReceiveThingsViewModel { Quest = n.q, prior = 200}).ToList();//同校不同省
            var qsu3 = qs_u.Where(o => !o.u.Province.Contains(pro)).Select(n => new SSSearchReceiveThingsViewModel { Quest = n.q, prior = 100 }).ToList();//不同省
            var qs_place = qsu1.Union(qsu2).Union(qsu3);//已转化为list的视图模型，之后关键字直接foreach匹配
            //获取同义分词 AI
            if (!string.IsNullOrEmpty(searchString))
            {
                string data = TencentAI.tencentAI(searchString);
                oWords = TencentAI.oriWords(data);
                sWords = TencentAI.synWords(data);
                //qs = qs.Where(x => x.Name.Contains(searchString));
                //关键字匹配
                foreach (var l in qs_place)
                {
                    if (l.Quest.Name.Contains(searchString))
                    {
                        l.prior += 10;
                        continue;
                    }
                    for (int i = 0; i < oWords.Count; i++)
                    {
                        if (l.Quest.Name.Contains(oWords[i]))
                        {
                            l.prior += 5;
                            break;
                        }
                    }
                    for (int j = 0; j < sWords.Count; j++)
                    {
                        if (l.Quest.Name.Contains(sWords[j]))
                        {
                            l.prior += 1;
                            break;
                        }
                    }
                    if (l.prior % 100 == 0)
                    {
                        l.prior -= 1000;
                    }
                }
            }
            qs_place = qs_place.Where(x => x.prior >= 0);
            //分页
            int totalThings = qs_place.Count();//总共条目数目
            ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
            int start = (currentPage - 1) * numPerPage;//开始的条目
            qs_place = qs_place.OrderByDescending(x => x.prior).ThenByDescending(x=>x.Quest.ReleaseTime).Skip(start).Take(numPerPage);
            ViewBag.searchString = searchString;
            ViewBag.currentPage = currentPage;
            return View(qs_place);
        }

        // GET: SSGiven/DetailsReceiveThings/id 受助请求详细信息页（查看信息） id=RTID
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
        // POST: SSGiven/DetailsReceiveThings/id 受助请求详细信息页（接受请求或返回列表） id=RTID action={接受,返回到列表}
        [HttpPost]
        public ActionResult DetailsReceiveThings(int? id, string action)
        {
            switch (action)
            {
                case "接受":
                    break;
                case "返回到列表":
                    return RedirectToAction("SearchReceiveThings");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("UploadGivenThings", new { id });
        }

    }
}