using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShare.Controllers
{
    public class AccountController : Controller
    {
        // GET: 打开登录界面
        // 修改时间：2019年4月28日 21点51分
        public ActionResult Login()
        {
            //跳转至Login视图
            return View();
        }

        // POST: 登录账号
        // 修改时间：2019年4月28日 22点16分
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            //模型错误返回错误模型的Login视图
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //获取登录表单的用户名和密码
            string username = model.Username;
            string password = model.Password;
            //查询用户名和密码
            using (SSDBEntities db = new SSDBEntities())
            {
                //第一个和username匹配的Users元组
                var User = db.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();
                if (User == null)//如果没找到对应Users,说明登录信息有误,返回登陆失败报错
                {
                    ModelState.AddModelError("", "登录失败");
                    return View(model);
                }
                else//如果找到Users,给Session赋值
                {
                    HttpContext.Session["usrName"] = model.Username;
                    HttpContext.Session["usrId"] = User.UserId;
                }
            }
            //登录成功后跳转至用户主页
            return RedirectToAction("Index", "Home");
        }

        // GET: 打开注册界面
        // 修改时间：2019年4月28日 21点51分
        public ActionResult Register()
        {
            return View();
        }

        // POST: 注册账号
        // 修改时间：2019年4月28日 22点22分
        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            //模型错误返回错误模型的Register视图
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //获取注册表单的用户名和密码
            string username = model.Username;
            string password = model.Password;
            using (SSDBEntities db = new SSDBEntities())
            {
                //若用户名重复,则返回用户名已注册报错
                if (db.Users.Where(x => x.Username == username).FirstOrDefault() != null)
                {
                    ModelState.AddModelError("", "用户名已注册");
                    return View(model);
                }
                //新建Users元组
                var u = new Users();
                u.Username = username;
                u.Password = password;
                db.Users.Add(u);
                db.SaveChanges();
            }
            //注册成功跳转至登录页面
            return RedirectToAction("Login");
        }
        // POST: 注销账号
        // 修改时间：2019年4月28日 22点24分
        [HttpPost]
        public ActionResult LogOff()
        {
            //取消Session值
            HttpContext.Session["usrName"] = null;
            HttpContext.Session["usrId"] = null;
            //注销成功后跳转至首页
            return RedirectToAction("Index", "Home");
        }
    }
}