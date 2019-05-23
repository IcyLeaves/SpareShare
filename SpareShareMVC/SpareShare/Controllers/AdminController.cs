using SpareShare.Filter;
using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                ts = ts.OrderBy(x => x.Id).Skip(start).Take(numPerPage);
                ViewBag.currentPage = currentPage;
                //给视图模型赋值
                foreach (var t in ts)
                {
                    //新建一个视图模型并赋值
                    var tmp = new ThingsListViewModel();
                    tmp.Name = t.Name;
                    tmp.Type = t.Type;
                    tmp.Detail = t.Detail;
                    tmp.ThingId = t.Id;
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
                qs = qs.OrderBy(x => x.Id).Skip(start).Take(numPerPage);
                ViewBag.currentPage = currentPage;
                //给视图模型赋值
                foreach (var q in qs)
                {
                    //新建一个视图模型并赋值
                    var tmp = new QuestsListViewModel();
                    tmp.Name = q.Name;
                    tmp.Type = q.Type;
                    tmp.Detail = q.Detail;
                    tmp.QuestId = q.Id;
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
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的捐赠物品
                var t = db.Things.Where(x => x.Id == id).FirstOrDefault();
                return View(t);
            }
        }

        // POST: 提交捐赠物品的审核信息表单
        // 修改时间: 2019年5月6日 14点55分
        [HttpPost]
        public ActionResult ThingsDetail(int id, string action)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                switch (action)
                {
                    case "通过":
                        //找出对应id的捐赠物品
                        var t1 = db.Things.Where(x => x.Id == id).FirstOrDefault();
                        //修改捐赠物品的状态
                        t1.Status = "物品闲置中";
                        db.Entry(t1).State = EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "拒绝":
                        //找出对应id的捐赠物品
                        var t2 = db.Things.Where(x => x.Id == id).FirstOrDefault();
                        //修改捐赠物品的状态
                        t2.Status = "审核未通过";
                        db.Entry(t2).State = EntityState.Modified;
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
            using (SSDBEntities db = new SSDBEntities())
            {
                //找出对应id的受助请求
                var q = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                return View(q);
            }
        }

        // POST: 提交受助请求的审核信息表单
        // 修改时间: 2019年5月6日 14点56分
        [HttpPost]
        public ActionResult QuestsDetail(int id, string action)
        {
            using (SSDBEntities db = new SSDBEntities())
            {
                switch (action)
                {
                    case "通过":
                        //找出对应id的受助请求
                        var q1 = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                        //修改受助请求的状态
                        q1.Status = "等待受助中";
                        db.Entry(q1).State = EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "拒绝":
                        //找出对应id的受助请求
                        var q2 = db.Quests.Where(x => x.Id == id).FirstOrDefault();
                        //修改受助请求的状态
                        q2.Status = "审核未通过";
                        db.Entry(q2).State = EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "取消":
                        break;
                }
            }
            //跳转至请求列表
            return RedirectToAction("CheckQuests");
        }
    }
}