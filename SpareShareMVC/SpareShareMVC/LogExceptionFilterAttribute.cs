using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShareMVC
{
    public class LogExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            // 添加记录日志代码
            Exception ex = filterContext.Exception;
            string filePath = System.Web.HttpContext.Current.Server.MapPath("/_error.txt");
            StreamWriter sw = System.IO.File.AppendText(filePath);
            sw.WriteLine("来源IP：" + System.Web.HttpContext.Current.Request.UserHostAddress);
            sw.WriteLine("异常时间：" + DateTime.Now.ToString());
            sw.WriteLine("异常页面：" + System.Web.HttpContext.Current.Request.Url);
            sw.WriteLine("异常信息：" + ex.Message);
            sw.WriteLine("异常来源：" + ex.Source);
            sw.WriteLine("堆栈信息：" + ex.StackTrace);
            sw.WriteLine("------------------------------");
            sw.WriteLine("");
            sw.Close();
            filterContext.ExceptionHandled = true;
            filterContext.Result = new RedirectResult("/Views/Shared/Error");
        }
    }
}