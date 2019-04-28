using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//namespace SpareShareMVC.Controllers
//{
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
//    public class MyAuthorizeAttribute :FilterAttribute, IAuthorizationFilter
//    {
//        public void OnAuthorization(AuthorizationContext filterContext)
//        {
//            var loginUser = filterContext.HttpContext.Session["usrId"];
//            //When user has not login yet
//            if (loginUser == null)
//            {
//                var redirectUrl = ConstantProvider.LoginURL + "?RedirectPath=" + filterContext.HttpContext.Request.Url;
//                filterContext.Result = new RedirectResult(redirectUrl);
//                return;
//            }

//        }
//    }
//}