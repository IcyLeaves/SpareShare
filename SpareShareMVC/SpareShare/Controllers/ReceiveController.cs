using OpenCvSharp;
using SpareShare.Filter;
using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    [MyAuthorize]
    public class ReceiveController : Controller
    {
        int numPerPage = 12;//每页显示条目
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
                    //查找捐赠者
                    Users tu = db.Users.Where(x => x.UserId == t.DonatorId).FirstOrDefault();
                    //新建视图模型
                    UploadQuestsViewModel res = new UploadQuestsViewModel();
                    
                    //给视图模型赋值
                    res.Thing = t;
                    res.Donator = tu;
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
        public ActionResult UploadQuests(UploadQuestsViewModel model, int? tId)
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
                q.ReleaseTime = DateTime.Now;
                q.Status = tId != null ? "已接受受助" : "正在审核中";
                //向数据库新建元组，并获取新Quests的id
                using (SSDBEntities db = new SSDBEntities())
                {
                    db.Quests.Add(q);
                    db.SaveChanges();
                    qId = q.Id;
                }
                //如果当前有匹配捐赠物品，再新建一条匹配元组
                if (tId != null)
                {
                    //新建ThingsQuests
                    ThingsQuests tq = new ThingsQuests();
                    tq.ThingId = tId.Value;
                    tq.QuestId = qId;
                    tq.Time = DateTime.Now;
                    //向数据库新建元组
                    using (SSDBEntities db = new SSDBEntities())
                    {
                        db.ThingsQuests.Add(tq);

                        Things t = db.Things.Where(x => x.Id == tId.Value).FirstOrDefault();
                        t.Status = "已接受捐赠";
                        db.Entry(t).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                //提交成功，跳转至用户主页
                return RedirectToAction("MyQuestsList");
            }
            //模型报错，返回当前页面
            return RedirectToAction("UploadQuests", new { id = tId });
        }

        // GET: 显示用户受助请求列表
        // 修改时间: 2019年5月23日 14点19分
        public ActionResult MyQuestsList(string keywords, string status, int currentPage = 1)
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<QuestsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找该用户发布的所有受助请求
                var qs = db.Quests.Where(x => x.ReceiverId == usrId);
                //1.按照关键字搜索
                if (keywords != null)
                {
                    qs = qs.Where(x => x.Name.Contains(keywords));
                }
                //2.按照状态筛选
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
                //3.分页
                int totalThings = qs.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                qs = qs.OrderByDescending(x => x.ReleaseTime).Skip(start).Take(numPerPage);
                ViewBag.searchString = keywords;
                ViewBag.Status = status;
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

        // GET: 显示受助请求详细信息
        // 修改时间: 2019年5月3日 13点02分
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
                //审核未通过的话还要获取审核信息
                if (q.Status == "审核未通过")
                {
                    Checks check = db.Checks.Where(x => x.Id == q.CheckId).FirstOrDefault();
                    res.Check = check;
                }
                //受助请求对应的捐赠物品..
                var ts = db.Things.Where(t =>
            (db.ThingsQuests.Where(qr => qr.QuestId == id)).Select(qrx => qrx.ThingId).Contains(t.Id));
                //..如果有的话
                if (ts.FirstOrDefault() != null)
                {
                    //捐赠物品信息
                    Things data = ts.FirstOrDefault();
                    res.Thing = data;

                    //捐赠者信息
                    Users tu = db.Users.Where(x => x.UserId == data.DonatorId).FirstOrDefault();
                    res.Donator = tu;

                    //评论信息
                    Comments c = db.Comments.Where(x => x.ThingId == data.Id).FirstOrDefault();
                    res.Comment = c;
                }
                return View(res);
            }
        }

        // POST: 提交对受助请求的操作
        // 修改时间: 2019年5月12日 13点55分
        [HttpPost]
        public ActionResult QuestsDetail(int id, string action, int ThingId = 0)
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
                        t.Status = "捐赠已成功";
                        db.Entry(t).State = EntityState.Modified;
                        //修改Quests状态
                        q.Status = "受助已成功";
                        db.Entry(q).State = EntityState.Modified;
                        //修改信誉评价
                        Assess tu = db.Assess.Where(x => x.UserId == t.DonatorId).FirstOrDefault();
                        Assess qu = db.Assess.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
                        tu.CreditPoint += 5;
                        tu.CreditLevel = CreditFunction.GetCreditLevel(tu.CreditPoint.Value);
                        tu.DonateNum++;
                        qu.CreditPoint += 1;
                        qu.CreditLevel = CreditFunction.GetCreditLevel(qu.CreditPoint.Value);
                        qu.ReceiveNum++;
                        //执行操作
                        db.SaveChanges();
                        return RedirectToAction("UploadComment",new { id = t.Id});
                    case "删除":
                        //修改受助方信誉评价
                        if (q.Status == "等待受助中")
                        {
                            var a = db.Assess.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
                            a.CreditPoint -= 2;
                            a.CreditLevel = CreditFunction.GetCreditLevel(a.CreditPoint.Value);
                            db.Entry(a).State = EntityState.Modified;
                        }
                        //从数据库中移除Quests元组
                        db.Quests.Remove(q);
                        db.SaveChanges();
                        return RedirectToAction("MyQuestsList");
                    case "评价物品":
                        return RedirectToAction("UploadComment", new { id = ThingId });
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
        // 修改时间: 2019年5月9日 15点04分
        public ActionResult SearchThings(string keywords, int currentPage = 1)
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];

            List<string> sWords = new List<string>();//近义词
            List<string> oWords = new List<string>();//原词

            using (SSDBEntities db = new SSDBEntities())
            {
                //查找[其他用户的][闲置的]物品
                var ts = db.Things.Where(x => x.DonatorId != usrId && x.Status == "物品闲置中");
                //1.地域置顶
                var user = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
                var pro = user.Province;
                var sch = user.School;
                var ts_u = ts.Join(db.Users.Select(x => new { x.UserId, x.Province, x.School }), t => t.DonatorId, u => u.UserId, (t, u) => new { t, u });
                var tsu1 = ts_u.Where(o => o.u.Province.Contains(pro) && o.u.School.Contains(sch)).Select(n => new ThingsListViewModel { Thing = n.t, prior = 300 }).ToList();//同省同校
                var tsu2 = ts_u.Where(o => o.u.Province.Contains(pro) && !o.u.School.Contains(sch)).Select(n => new ThingsListViewModel { Thing = n.t, prior = 200 }).ToList();//同校不同省
                var tsu3 = ts_u.Where(o => !o.u.Province.Contains(pro)).Select(n => new ThingsListViewModel { Thing = n.t, prior = 100 }).ToList();//不同省
                var ts_place = tsu1.Union(tsu2).Union(tsu3);//已转化为list的视图模型，之后关键字直接foreach匹配
                //2.获取同义分词 AI
                if (!string.IsNullOrEmpty(keywords))
                {
                    string data = TencentAI.tencentAI(keywords);
                    oWords = TencentAI.oriWords(data);
                    sWords = TencentAI.synWords(data);
                    //qs = qs.Where(x => x.Name.Contains(searchString));
                    //关键字匹配
                    foreach (var l in ts_place)
                    {
                        if (l.Thing.Name.Contains(keywords))
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
                        if (l.prior % 100 == 0)
                        {
                            l.prior -= 1000;
                        }
                    }
                }
                ts_place = ts_place.Where(x => x.prior >= 0);
                //3.分页
                int totalThings = ts.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                ts_place = ts_place.OrderByDescending(x => x.prior).Skip(start).Take(numPerPage);
                ViewBag.searchString = keywords;
                ViewBag.currentPage = currentPage;
                return View(ts_place);
            }
        }

        // POST: 以图搜图
        [HttpPost]
        public ActionResult SearchThings(IEnumerable<HttpPostedFileBase> Image)
        {
            string hash = "0";
            foreach (var imgFile in Image)
            {
                if (imgFile != null && imgFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(imgFile.FileName);
                    if (extension == ".jpg" || extension == ".png" || extension == ".gif")
                    {
                        string phicyPath = PublicFunction.GetPhicyPath();
                        //创建Upload文件夹，如果有则不创建
                        Directory.CreateDirectory(phicyPath);
                        //文件名与路径
                        var fileName = Path.GetFileName(imgFile.FileName);
                        var filePath = phicyPath + fileName;
                        //防止文件重名
                        while (System.IO.File.Exists(filePath))
                        {
                            Random rand = new Random();
                            fileName = rand.Next().ToString() + "-" + fileName;
                            filePath = phicyPath + fileName;
                        }
                        //存储文件到文件目录
                        imgFile.SaveAs(filePath);
                        //计算图片的哈希值
                        hash = MainHash(filePath);
                    }
                }
            }
            return RedirectToAction("SearchThingsByImage", new { hash = hash });
        }
        //计算哈希值
        //修改时间: 2019年5月16日 22点08分
        string MainHash(string path)
        {
            Mat image = Cv2.ImRead(path, ImreadModes.Color);
            return ImageHashValue(image);
        }
        //计算图片的指纹信息
        string ImageHashValue(Mat src)
        {
            Mat image1 = new Mat(src.Height, src.Width, MatType.CV_8UC1);
            Mat image = new Mat(src.Height, src.Width, MatType.CV_32FC1);
            //1.灰度化
            if (src.Channels() == 3)
            {
                Cv2.CvtColor(src, image1, ColorConversionCodes.BGR2GRAY);
                image1.ConvertTo(image, MatType.CV_32FC1, 1 / 225.0);
            }
            else
            {
                src.CopyTo(image1);
                image1.ConvertTo(image, MatType.CV_32FC1, 1 / 225.0);
            }
            //2.缩小尺寸 8 * 8
            Mat temp = new Mat(8, 8, MatType.CV_32F);
            Mat dst = new Mat(8, 8, MatType.CV_32F);
            Cv2.Resize(image, temp, new Size(8, 8));
            //3.DCT
            Cv2.Dct(temp, dst);
            //3.求取DCT系数均值（左上角8*8区块的DCT系数）
            double[] dIdex = new double[64];
            double mean = 0.0;
            int k = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    dIdex[k] = dst.At<double>(i, j);
                    mean += dst.At<double>(i, j) / 64;
                    ++k;
                }
            }
            //4.计算哈希值
            string resStr = "";
            for (int i = 0; i < 64; ++i)
            {
                if (dIdex[i] >= mean)
                {
                    resStr += '1';
                }
                else
                {
                    resStr += '0';
                }
            }
            return resStr;
        }

        // GET: 查看评论捐赠物品界面
        public ActionResult UploadComment(int id)
        {
            UploadCommentViewModel res = new UploadCommentViewModel();
            using (SSDBEntities db = new SSDBEntities())
            {
                Things t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                Users tu = db.Users.Where(x => x.UserId == t.DonatorId).FirstOrDefault();
                res.Thing = t;
                res.Donator = tu;
            }
            return View(res);
        }
        // POST: SSReceive/UploadComment/id 评论捐赠物品 id=物品id
        [HttpPost]
        public ActionResult UploadComment(UploadCommentViewModel model, int id)
        {
            int usrId = (int)HttpContext.Session["usrId"];
            if (ModelState.IsValid)
            {
                using (SSDBEntities db = new SSDBEntities())
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
                    a.CreditLevel = CreditFunction.GetCreditLevel(a.CreditPoint.Value);
                    db.Entry(a).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                return View(model);
            }
            return RedirectToAction("MyQuestsList");
        }

        // GET: 以图搜图
        public ActionResult SearchThingsByImage(string hash, int currentPage = 1)
        {
            int usrId = (int)HttpContext.Session["usrId"];

            using (SSDBEntities db = new SSDBEntities())
            {
                var ts = db.Things.Where(x => x.DonatorId != usrId && x.Status == "物品闲置中");
                //1.地域
                var user = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
                var pro = user.Province;
                var sch = user.School;
                var ts_u = ts.Join(db.Users.Select(x => new { x.UserId, x.Province, x.School }), t => t.DonatorId, u => u.UserId, (t, u) => new { t, u });
                var tsu1 = ts_u.Where(o => o.u.Province.Contains(pro) && o.u.School.Contains(sch)).Select(n => new ThingsListViewModel { Thing = n.t, prior = 300, similar = 0 }).ToList();//同省同校
                var tsu2 = ts_u.Where(o => o.u.Province.Contains(pro) && !o.u.School.Contains(sch)).Select(n => new ThingsListViewModel { Thing = n.t, prior = 200, similar = 0 }).ToList();//同校不同省
                var tsu3 = ts_u.Where(o => !o.u.Province.Contains(pro)).Select(n => new ThingsListViewModel { Thing = n.t, prior = 100, similar = 0 }).ToList();//不同省
                var ts_place = tsu1.Union(tsu2).Union(tsu3);//已转化为list的视图模型，之后关键字直接foreach匹配
                //2.相似度高置顶
                foreach (var t in ts_place)
                {
                    string tmp = t.Thing.ImageHash;
                    t.similar = ImageSimilarity(hash, tmp) * 100;
                }
                //3.分页
                int totalThings = ts.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                ts_place = ts_place.OrderByDescending(x => x.similar).Skip(start).Take(numPerPage);
                ViewBag.currentPage = currentPage;
                ViewBag.hash = hash;
                return View(ts_place);
            }
        }
        //根据指纹信息计算两幅图像的相似度
        double ImageSimilarity(string str1, string str2)
        {
            double similarity = 0.0;
            if(!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2))
            {
                for (int i = 1; i < 64; i++)
                {
                    if (str1[i] == str2[i])
                    {
                        similarity += 1.0 / 64;
                    }
                }
            }
            return similarity;
        }
    }
}