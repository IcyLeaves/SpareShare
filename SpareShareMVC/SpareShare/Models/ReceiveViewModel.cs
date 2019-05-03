using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpareShare.Models
{
    public class UploadQuestsViewModel
    {
        [Required(ErrorMessage = "请输入请求名称")]
        [Display(Name = "请求名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "请输入请求类别")]
        [Display(Name = "请求类别")]
        public string Type { get; set; }

        [Required(ErrorMessage = "请输入请求描述")]
        [Display(Name = "请求描述")]
        public string Detail { get; set; }
    }

    public class MyQuestsListViewModel
    {
        public int QuestId { get; set; }

        [Display(Name = "请求名称")]
        public string Name { get; set; }

        [Display(Name = "请求类别")]
        public string Type { get; set; }

        [Display(Name = "请求描述")]
        public string Detail { get; set; }
    }
}