using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    public class ReceiveController : Controller
    {
        // GET: 显示发布请求的表单
        // 修改时间: 2019年5月1日 15点07分
        public ActionResult UploadQuests()
        {
            return View();
        }

        // POST: 提交表单，发布受助请求信息
        // 修改时间: 2019年5月3日 14点06分
        [HttpPost]
        public ActionResult UploadQuests(UploadQuestsViewModel model)
        {
            //获取当前用户的Id
            int usrId = (int)HttpContext.Session["usrId"];
            if (ModelState.IsValid)
            {
                //新建Quests
                Quests q = new Quests();
                q.ReceiverId = usrId;
                q.Name = model.Name;
                q.Type = model.Type;
                q.Detail = model.Detail;
                q.Status = "正在审核中";
                //向数据库新建元组
                using (SSDBEntities db = new SSDBEntities())
                {
                    db.Quests.Add(q);
                    db.SaveChanges();
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
    }
}