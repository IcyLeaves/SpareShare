using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShareMVC.Filter
{
    public class MyAdmin : AuthorizeAttribute
    {
        /// <summary>
        /// 提供一个入口用于自定义授权检查
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool pass = false;
            var isAdmin = HttpContext.Current.Session["isAdmin"];
            if ((string)isAdmin == "N")
            {
                httpContext.Response.StatusCode = 401;
                pass = false;
            }
            else
            {
                pass = true;
            }
            return pass;
        }

        /// <summary>
        /// 处理未能授权的Http请求
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            filterContext.HttpContext.Response.Write(filterContext.HttpContext.Response.StatusCode);
            if (filterContext.HttpContext.Response.StatusCode == 401)
            {
                //跳转到登录界面
                filterContext.Result = new RedirectResult("/SSUser/Index");
            }
        }
    }

}