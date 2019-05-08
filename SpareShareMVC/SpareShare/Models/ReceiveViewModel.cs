using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpareShare.Models
{
    public class UploadQuestsViewModel
    {
        public int tId { set; get; }

        [Display(Name = "物品名称")]
        public string tName { get; set; }

        [Display(Name = "物品类别")]
        public string tType { get; set; }
        
        [Display(Name = "物品描述")]
        public string tDetail { get; set; }

        [Required(ErrorMessage = "请输入请求名称")]
        [Display(Name = "请求名称")]
        public string qName { get; set; }

        [Required(ErrorMessage = "请输入请求类别")]
        [Display(Name = "请求类别")]
        public string qType { get; set; }

        [Required(ErrorMessage = "请输入请求描述")]
        [Display(Name = "请求描述")]
        public string qDetail { get; set; }
    }

    public class QuestsListViewModel
    {
        public int QuestId { get; set; }

        [Display(Name = "请求名称")]
        public string Name { get; set; }

        [Display(Name = "请求类别")]
        public string Type { get; set; }

        [Display(Name = "请求描述")]
        public string Detail { get; set; }

        [Display(Name = "请求状态")]
        public string Status { get; set; }
    }
}