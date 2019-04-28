using Newtonsoft.Json;
using SpareShareMVC.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpareShareMVC.Controllers
{
    public class SSChartsController : Controller
    {
        public string[] Types = new string[] { "书籍", "工具", "日用", "服饰","杂物","娱乐","配件","其他" };
        public string[] Scores = new string[] { "新旧","符合描述","实用程度","送达速度","美观程度"};
        public string[] Provinces = new string[] { "北京", "天津", "河北", "山西", "内蒙古", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东","河南",
                                                    "湖北","湖南","广东","广西","海南","重庆","四川","贵州","云南","西藏","陕西","甘肃","青海","宁夏","新疆","香港","澳门","台湾"};
        // GET: SSCharts
        public ActionResult Index()
        {
            return View();
        }

        //前21天每天发布的捐赠物品数
        public JsonResult NumbersStatis()
        {
            dynamic sp;
            List<dynamic> LL = new List<dynamic>();
            List<string> x = new List<string>();
            List<int> y = new List<int>();
            using (Entities db = new Entities())
            {
                for(int day=-20;day<=0;day++)
                {
                    var LastDay = DateTime.Now.AddDays(day);
                    var numOfThings = db.Things.Where(xt => xt.ReleaseTime.Value.Day==LastDay.Day).Count();
                    x.Add(DateTime.Now.AddDays(day).ToString("MM-dd"));
                    y.Add(numOfThings);
                }
                
            }
            sp = new ExpandoObject();
            sp.type = "line";
            sp.data = y.ToArray();
            LL.Add(sp);
            return Json(new
            {
                legend = new { data = new string[] { "物品数" } },
                xAxis = new { data = x.ToArray() },
                Series = JsonConvert.SerializeObject(LL)
            },
                JsonRequestBehavior.AllowGet);
        }

        //全站所有发布的捐赠物品饼图
        public JsonResult CategoryStatis()
        {
            dynamic sp;
            List<dynamic> LL = new List<dynamic>();
            List<string> legends = new List<string>();
            using (Entities db = new Entities())
            {
                var cates = db.Things
                    .GroupBy(x => x.Type)
                    .Select(gp => new
                    {
                        type = gp.Key,
                        count = gp.Count()
                    });
                foreach (var c in cates)
                {
                    sp = new ExpandoObject();
                    sp.value = c.count;
                    sp.name = c.type;
                    LL.Add(sp);
                    legends.Add(c.type);
                }
            }
            return Json(new
            {
                legend = new { data = legends },
                SeriesData = JsonConvert.SerializeObject(LL)
            },
                JsonRequestBehavior.AllowGet);
        }

        //单件物品的评价雷达图
        public JsonResult CommentStatis(int thingId,int receiverId)
        {
            dynamic indicator;
            List<dynamic> cators = new List<dynamic>();
            List<int> values = new List<int>();
            using (Entities db = new Entities())
            {
                var c = db.Comments.Where(x => x.ReceiverId == receiverId && x.ThingId == thingId).FirstOrDefault();
                values.Add(c.NewScore.Value);
                values.Add(c.SimilarScore.Value);
                values.Add(c.UsefulScore.Value);
                values.Add(c.SpeedScore.Value);
                values.Add(c.BeautifulScore.Value);
            }
            foreach(var s in Scores)
            {
                indicator = new ExpandoObject();
                indicator.name = s;
                indicator.max = 5;
                cators.Add(indicator);
            }

            return Json(new
            {
                Indicator = JsonConvert.SerializeObject(cators),
                SeriesData = values.ToArray()
            },
                JsonRequestBehavior.AllowGet);
        }

        //爱心地图
        public JsonResult MapStatis()
        {
            dynamic sp;
            List<dynamic> ps = new List<dynamic>();
            dynamic nums;
            List<dynamic> numses = new List<dynamic>();
            int maxNum;
            using (Entities db = new Entities())
            {
                var ts = db.Things.Join(db.Users, t => t.GiverId, u => u.UserId, (t, u) => new { t, u })
                    .GroupBy(x => x.u.Province)
                    .Select(gt => new
                    {
                        province = gt.Key,
                        count = gt.Count()
                    });
                foreach(var p in Provinces)
                {
                    sp = new ExpandoObject();
                    sp.name = p;
                    var data = ts.Where(x => x.province == p).FirstOrDefault();
                    if(data!=null)
                    {
                        sp.value = data.count;
                    }
                    else
                    {
                        sp.value = 0;
                    }
                    ps.Add(sp);
                }

                maxNum = ts.Max(x=>x.count);
            }
            int level = maxNum / 6 + 1;
            for(int i=0;i<6;i++)
            {
                nums = new ExpandoObject();
                nums.start = i * level;
                nums.end = i * level +level ;
                numses.Add(nums);
            }
            return Json(new
            {
                SeriesData = JsonConvert.SerializeObject(ps),
                Levels=JsonConvert.SerializeObject(numses)
            },
                JsonRequestBehavior.AllowGet);
        }
    }
}