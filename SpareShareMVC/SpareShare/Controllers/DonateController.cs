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
        // 修改时间: 2019年5月21日 15点45分
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
                //文件存储到目录下
                var imgFile = model.Image;
                if (imgFile != null && imgFile.ContentLength > 0)
                {
                    var root = "~/Upload/Images/";
                    //主机存储图片的文件目录
                    var phicyPath = HostingEnvironment.MapPath(root);
                    //创建Upload文件夹，如果有则不创建
                    Directory.CreateDirectory(phicyPath);
                    //文件名与路径
                    var fileName = imgFile.FileName;
                    var filePath = phicyPath + fileName;
                    //防止文件重名
                    while (System.IO.File.Exists(filePath))
                    {
                        Random rand = new Random();
                        fileName = rand.Next().ToString() + "-" + imgFile.FileName;
                        filePath = phicyPath + fileName;
                    }
                    //存储文件到文件目录
                    imgFile.SaveAs(filePath);
                    //保存提交的图片URL至数据库
                    t.ImageUrl = fileName;
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
        ////计算哈希值
        //// 修改时间: 2019年5月16日 22点08分
        //void MainHash()
        //{
        //    Mat image1 = Cv2.ImRead("example1.jpg",ImreadModes.Color);
        //    Mat image2 = Cv2.ImRead("example3.jpg",ImreadModes.Color);
        //    string imgPrint1 = ImageHashValue(image1);
        //    string imgPrint2 = ImageHashValue(image2);
        //    double similarity = ImageSimilarity(imgPrint1, imgPrint2);
        //    //cout << "The similarity of two images is " << similarity * 100 << "%" << endl;
        //    //if (similarity >= 0.9) cout << "The two images are extremely similar." << endl;
        //    //else if (similarity >= 0.8 && similarity < 0.9) cout << "The two images are pretty similar." << endl;
        //    //else if (similarity >= 0.7 && similarity < 0.8) cout << "The two images are a little similar." << endl;
        //    //else if (similarity < 0.7) cout << "The two image are not similar." << endl; cout << endl;
        //    Cv2.WaitKey(0);
        //}
        ////计算图片的指纹信息
        //string ImageHashValue(Mat src)
        //{
        //    string resStr=new string('\0',64);
        //    Mat image = new Mat(src.Height, src.Width,MatType.CV_8UC3);
        //    //1.灰度化
        //    if(src.Channels()==3)
        //    {
        //        Cv2.CvtColor(src, image, ColorConversionCodes.BGR2GRAY);
        //    }
        //    else
        //    {
        //        src.CopyTo(image);
        //    }
        //    //2.缩小尺寸 8*8
        //    Mat temp = new Mat(8, 8, MatType.CV_8UC1);
        //    Cv2.Resize(image,temp,new Size(8,8));
        //    ////3.简化色彩
        //    //ArrayList pData = new ArrayList();
        //    //pData.Capacity = 64;
        //    //for(int i=0;i<temp.Height;i++)
        //    //{
        //    //    for(int j=0;j<temp.Width;j++)
        //    //    {
        //    //        pData
        //    //    }
        //    //    temp.GetArray(i,)
        //    //    pData = (temp.GetArray + i * temp.widthStep);
        //    //    for(int j=0;j<temp.Width;j++)
        //    //    {
        //    //        pData[j] = (int)pData[j] / 4;
        //    //    }
        //    //}
        //    //3.计算平均灰度值
        //    double average = Cv2.Mean(temp).Val0;
        //    //4.计算哈希值
        //    int index = 0;
        //    for(int i=0;i<temp.Height;i++)
        //    {
        //        pData = (temp.imageData + i * temp.widthStep);
        //        for(int j=0;j<temp.Width;j++)
        //        {
        //            if(pData[j]>=average)
        //            {
        //                resStr[index++] = '1';
        //            }
        //            else
        //            {
        //                resStr[index++] = '0';
        //            }
        //        }
        //    }
        //    return resStr;
        //}
        ////根据指纹信息计算两幅图像的相似度
        //double ImageSimilarity(string str1,string str2)
        //{
        //    double similarity = 1.0;
        //    for(int i=1;i<64;i++)
        //    {
        //        if(str1[i]!=str2[i])
        //        {
        //            similarity -= 1.0 / 64;
        //        }
        //    }
        //    return similarity;
        //}


        // GET: 显示用户捐赠物品列表
        // 修改时间: 2019年5月1日 13点29分
        public ActionResult MyThingsList(string keywords,string status)
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
                ViewBag.Status = status;//保存status以便再传回这里
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
        // 修改时间: 2019年5月21日 14点58分
        public ActionResult SearchQuests(string keywords)
        {
            //获取当前用户id
            int usrId = (int)HttpContext.Session["usrId"];
            //新建视图模型列表
            var res = new List<QuestsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //查找[其他用户的][闲置的]请求
                var qs = db.Quests.Where(x => x.ReceiverId != usrId && x.Status == "等待受助中");
                //1.按照关键字搜索
                if(keywords!=null)
                {
                    qs = qs.Where(x => x.Name.Contains(keywords));
                }
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