using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using OpenCvSharp;
using System.Collections;
using SpareShare.Filter;

namespace SpareShare.Controllers
{
    [MyAuthorize]
    public class DonateController : Controller
    {
        int numPerPage = 12;//每页显示条目数
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
                    //查找受助方
                    Users qu = db.Users.Where(x => x.UserId == q.ReceiverId).FirstOrDefault();
                    //新建视图模型
                    UploadThingsViewModel res = new UploadThingsViewModel();
                    
                    //给视图模型赋值
                    res.Quest = q;
                    res.Receiver = qu;
                    res.tName = q.Name;
                    res.tType = q.Type;
                    return View(res);
                }
            }
            return View();
        }

        // POST: 提交表单，上传捐赠物品信息
        // 修改时间: 2019年5月21日 15点45分
        [HttpPost]
        public ActionResult UploadThings(UploadThingsViewModel model,int? qId)
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
                t.ReleaseTime = DateTime.Now;
                t.Status = qId != null ? "已接受捐赠" : "正在审核中";//如果当前有匹配受助请求，则跳过审核
                //文件存储到目录下
                foreach (var imgFile in model.Image)
                {
                    if (imgFile != null && imgFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(imgFile.FileName);
                        if(extension==".jpg" || extension == ".png" || extension == ".gif")
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
                            t.ImageHash = MainHash(filePath);
                            //保存提交的图片URL至数据库
                            t.ImageUrl = fileName;
                        }
                        else
                        {
                            ModelState.AddModelError("Image", "请上传正确格式的图片(.jpg .png .gif)");
                            return View(model);
                        }
                    }
                }
                //向数据库新建元组，并获取新Things的id
                using (SSDBEntities db = new SSDBEntities())
                {
                    db.Things.Add(t);
                    db.SaveChanges();
                    tId = t.Id;
                }
                //如果当前有匹配受助请求，再新建一条匹配元组
                //并改变受助请求状态
                if (qId != null)
                {
                    //新建ThingsQuests
                    ThingsQuests tq = new ThingsQuests();
                    tq.ThingId = tId;
                    tq.QuestId = qId.Value;
                    tq.Time = DateTime.Now;
                    //向数据库新建元组，改变受助请求状态
                    using (SSDBEntities db = new SSDBEntities())
                    {
                        db.ThingsQuests.Add(tq);

                        Quests q = db.Quests.Where(x => x.Id == qId.Value).FirstOrDefault();
                        q.Status = "已接受受助";
                        db.Entry(q).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                //提交成功，跳转至用户主页
                return RedirectToAction("MyThingsList");
            }
            //模型报错，返回当前页面
            return RedirectToAction("UploadThings", new { id = qId});
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
            Mat image1 = new Mat(src.Height, src.Width,MatType.CV_8UC1);
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
            double[] dIdex=new double[64];
            double mean = 0.0;
            int k = 0;
            for (int i=0;i<8;i++)
            {
                for(int j=0;j<8;j++)
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
                    resStr+= '1';
                }
                else
                {
                    resStr+= '0';
                }
            }
            return resStr;
        }
        


        // GET: 显示用户捐赠物品列表
        // 修改时间: 2019年5月1日 13点29分
        public ActionResult MyThingsList(string keywords,string status,int currentPage=1)
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<ThingsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找该用户发布的所有捐赠物品
                var ts = db.Things.Where(x => x.DonatorId == usrId);
                //1.按照关键字搜索
                if (keywords != null)
                {
                    ts = ts.Where(x => x.Name.Contains(keywords));
                }
                //2.按照状态筛选
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
                //3.分页
                int totalThings = ts.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                ts = ts.OrderByDescending(x=>x.ReleaseTime).Skip(start).Take(numPerPage);
                ViewBag.searchString = keywords;
                ViewBag.Status = status;
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

        // GET: 显示捐赠物品详细信息
        // 修改时间: 2019年5月3日 15点10分
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
                //审核未通过的话还要获取审核信息
                if (t.Status == "审核未通过")
                {
                    Checks check = db.Checks.Where(x => x.Id == t.CheckId).FirstOrDefault();
                    res.Check = check;
                }
                //捐赠物品对应的受助请求..
                var qs = db.Quests.Where(q =>
            (db.ThingsQuests.Where(tr => tr.ThingId == id)).Select(trx => trx.QuestId).Contains(q.Id));
                //..如果有的话
                if (qs.FirstOrDefault() != null)
                {
                    //受助请求信息
                    Quests data = qs.FirstOrDefault();
                    res.Quest = data;

                    //受助方信息
                    Users qu = db.Users.Where(x => x.UserId == data.ReceiverId).FirstOrDefault();
                    res.Receiver = qu;

                    //评论信息
                    Comments c = db.Comments.Where(x => x.ThingId == id).FirstOrDefault();
                    res.Comment = c;
                }
                return View(res);
            }
        }

        // POST: 提交对捐赠物品的操作
        // 修改时间: 2019年5月12日 13点58分
        [HttpPost]
        public ActionResult ThingsDetail(int id, string action)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的捐赠物品
                Things t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                switch (action)
                {
                    case "删除":
                        //修改捐赠者信誉评价
                        if (t.Status == "等待受助中")
                        {
                            var tu = db.Assess.Where(x => x.UserId == t.DonatorId).FirstOrDefault();
                            tu.CreditPoint -= 2;
                            tu.CreditLevel = CreditFunction.GetCreditLevel(tu.CreditPoint.Value);
                            db.Entry(tu).State = EntityState.Modified;
                        }
                        //移除Things元组
                        db.Things.Remove(t);
                        db.SaveChanges();
                        return RedirectToAction("MyThingsList");
                    case "返回":
                        //跳转至“我的捐赠物品”
                        return RedirectToAction("MyThingsList"); ;
                }
                return View(t);
            }
        }

        // GET: 显示受助请求详细信息
        // 修改时间: 2019年5月3日 15点10分
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
        // 修改时间: 2019年5月21日 14点58分
        public ActionResult SearchQuests(string keywords,int currentPage=1)
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];

            List<string> sWords = new List<string>();//近义词
            List<string> oWords = new List<string>();//原词

            using (SSDBEntities db = new SSDBEntities())
            {
                //查找[其他用户的][闲置的]请求
                var qs = db.Quests.Where(x => x.ReceiverId != usrId && x.Status == "等待受助中");
                //1.地域置顶
                var user = db.Users.Where(x => x.UserId == usrId).FirstOrDefault();
                var pro = user.Province;
                var sch = user.School;
                var qs_u = qs.Join(db.Users.Select(x => new { x.UserId, x.Province, x.School }), q => q.ReceiverId, u => u.UserId, (q, u) => new { q, u });
                var qsu1 = qs_u.Where(o => o.u.Province.Contains(pro) && o.u.School.Contains(sch)).Select(n => new QuestsListViewModel { Quest = n.q, prior = 300 }).ToList();//同省同校
                var qsu2 = qs_u.Where(o => o.u.Province.Contains(pro) && !o.u.School.Contains(sch)).Select(n => new QuestsListViewModel { Quest = n.q, prior = 200 }).ToList();//同校不同省
                var qsu3 = qs_u.Where(o => !o.u.Province.Contains(pro)).Select(n => new QuestsListViewModel { Quest = n.q, prior = 100 }).ToList();//不同省
                var qs_place = qsu1.Union(qsu2).Union(qsu3);//已转化为list的视图模型，之后关键字直接foreach匹配

                //2.获取同义分词 AI
                if (!string.IsNullOrEmpty(keywords))
                {
                    string data = TencentAI.tencentAI(keywords);
                    oWords = TencentAI.oriWords(data);
                    sWords = TencentAI.synWords(data);
                    //qs = qs.Where(x => x.Name.Contains(searchString));
                    //关键字匹配
                    foreach (var l in qs_place)
                    {
                        if (l.Quest.Name.Contains(keywords))
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
                //3.分页
                int totalThings = qs.Count();//总共条目数目
                ViewBag.totalPages = (int)Math.Ceiling(totalThings / (double)numPerPage);//总共页数
                int start = (currentPage - 1) * numPerPage;//开始的条目
                qs_place = qs_place.OrderByDescending(x => x.prior).Skip(start).Take(numPerPage);
                ViewBag.searchString = keywords;
                ViewBag.currentPage = currentPage;
                return View(qs_place);
            }
            
        }
        
    }
}