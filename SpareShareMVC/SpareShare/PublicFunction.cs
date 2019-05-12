using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpareShare
{
    static public class PublicFunction
    {
        static public string GetImgSrc(string url)
        {
            if(url!=null && System.IO.File.Exists(url))
            {
                return url;
            }
            return "/Files/1.png";
        }
    }
}