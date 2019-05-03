using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: 查看待审核的捐赠物品列表
        // 修改时间: 2019年5月3日 15点06分
        public ActionResult CheckThings()
        {
            //新建视图模型列表
            var res = new List<ThingsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //所有待审核的捐赠物品
                var ts = db.Things.Where(x => x.Status == "正在审核中");
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
        public ActionResult CheckQuests()
        {
            //新建视图模型列表
            var res = new List<QuestsListViewModel>();
            using (SSDBEntities db = new SSDBEntities())
            {
                //所有待审核的受助请求
                var qs = db.Quests.Where(x => x.Status == "正在审核中");
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
    }
}