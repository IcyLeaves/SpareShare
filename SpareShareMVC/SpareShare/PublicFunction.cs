using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SpareShare
{
    static public class PublicFunction
    {
        static string root = "/Upload/Images/";
        static public string GetPhicyPath()
        {
            //主机存储图片的文件目录
            var phicyPath = HostingEnvironment.MapPath("~"+root);
            return phicyPath;
        }
        static public string GetImgSrc(string url)
        {
            string webPath = root + url;
            string phicyPath = GetPhicyPath() + url;
            if (url!=null && File.Exists(phicyPath))
            {
                return webPath;
            }
            return "/Files/1.png";
        }
    }
}
namespace SpareShare.Filter
{
    public class MyAuthorize : AuthorizeAttribute
    {
        /// <summary>
        /// 提供一个入口用于自定义授权检查
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool pass = false;
            var loginId = HttpContext.Current.Session["usrId"];
            if (loginId == null)
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
                filterContext.Result = new RedirectResult("/Account/Login");
            }
        }
    }
}