using SpareShare.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SpareShare
{
    static public class PublicFunction
    {
        static public string[] Provinces = new string[] { "北京", "天津", "河北", "山西", "内蒙古", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东","河南",
                                                    "湖北","湖南","广东","广西","海南","重庆","四川","贵州","云南","西藏","陕西","甘肃","青海","宁夏","新疆","香港","澳门","台湾"};


        static string root = "/Upload/Images/";
        //获取主机物理路径
        static public string GetPhicyPath()
        {
            //主机存储图片的文件目录
            var phicyPath = HostingEnvironment.MapPath("~"+root);
            return phicyPath;
        }
        //网页所有图片src调用的函数
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
        //根据不同的状态返回[受助请求进度]html
        static public string GetQuestStatus(string status)
        {
            switch(status)
            {
                case "正在审核中":
                    return "<span style='color:Orange;float:right;'>" + status + "</span>";
                case "等待受助中":
                    return  "<span style='color:cadetblue;float:right;'>" + status + "</span>";
                case "已接受受助":
                    return "<span style='color:forestgreen;float:right;'>" + status + "</span>"; ;
                case "审核未通过":
                    return "<span style='color:firebrick;float:right;'>" + status + "</span>";
                case "受助已成功":
                    return "<span style='color:mediumPurple;float:right;'>" + status + "</span>";
            }
            return "";
        }
        //根据不同的状态返回[捐赠物品进度]html
        static public string GetThingStatus(string status)
        {
            switch (status)
            {
                case "正在审核中":
                    return "<span style='color:Orange;'>" + status + "</span>";
                case "物品闲置中":
                    return "<span style='color:cadetblue;'>" + status + "</span>";
                case "已接受捐赠":
                    return "<span style='color:forestgreen;'>" + status + "</span>"; ;
                case "审核未通过":
                    return "<span style='color:firebrick;'>" + status + "</span>";
                case "捐赠已成功":
                    return "<span style='color:mediumPurple;'>" + status + "</span>";
            }
            return "";
        }
        //TimeSpan的转换
        static public string TransTime(DateTime time)
        {
            TimeSpan span = (TimeSpan)(DateTime.Now - time);
            var result = "";
            if (span.TotalDays > 60)
            {
                result = time.ToShortDateString();
            }
            else if (span.TotalDays > 30)
            {
                result = "1个月前";
            }
            else if (span.TotalDays > 14)
            {
                result = "2周前";
            }
            else if (span.TotalDays > 7)
            {
                result = "1周前";
            }
            else if (span.TotalDays > 1)
            {
                result = string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
            }
            else if (span.TotalHours > 1)
            {
                result = string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
            }
            else if (span.TotalMinutes > 1)
            {
                result = string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
            }
            else if (span.TotalSeconds >= 1)
            {
                result = string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
            }
            else
            {
                result = "1秒前";
            }
            return result;
        }
        //根据不同的状态返回[同校]html
        static public string GetSameSch(int prior)
        {
            if (prior<300) { return ""; }
            else return "<span class='badge sameschool'>同校</span>";
        }
        //根据不同的状态返回[同省]html
        static public string GetSamePro(int prior)
        {
            if (prior < 200) { return ""; }
            else return "<span class='badge sameprovince'>同省</span>";
        }
        //获取省份下拉框
        static public List<SelectListItem> GetProvinces()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            foreach (var p in Provinces)
            {
                selectList.Add(new SelectListItem { Value = p, Text = p });
            }
            return selectList;
        }
        //获取相似度html
        static public string GetSimilar(double similar)
        {
            return "<span style='color:Orange;'>" + string.Format("{0:F}%",similar)+ "相似度</span>";
        }
    }
    static public class CreditFunction
    {
        static public int GetCreditLevel(int exp)
        {
            if (0 <= exp && exp < 5)
            {
                return 1;
            }
            if (5 <= exp && exp < 20)
            {
                return 2;
            }
            if (20 <= exp && exp < 50)
            {
                return 3;
            }
            if (0 <= exp && exp < 100)
            {
                return 4;
            }
            if (0 <= exp && exp < 200)
            {
                return 5;
            }
            return 6;
        }
        static public int ExpInProgress(int exp)
        {
            if (exp < 5)
            {
                return 5 - exp;
            }
            if (5 <= exp && exp < 20)
            {
                return 20 - exp;
            }
            if (20 <= exp && exp < 50)
            {
                return 50 - exp;
            }
            if (0 <= exp && exp < 100)
            {
                return 100 - exp;
            }
            if (0 <= exp && exp < 200)
            {
                return 200 - exp;
            }
            return -1;
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