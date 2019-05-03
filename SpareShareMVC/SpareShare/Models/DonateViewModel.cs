using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpareShare.Models
{
    public class UploadThingsViewModel
    {
        [Required(ErrorMessage = "请输入物品名称")]
        [Display(Name = "物品名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "请输入物品类别")]
        [Display(Name = "物品类别")]
        public string Type { get; set; }

        [Required(ErrorMessage = "请输入物品描述")]
        [Display(Name = "物品描述")]
        public string Detail { get; set; }
    }

    public class MyThingsListViewModel
    {
        public int ThingId { get; set; }

        [Display(Name = "物品名称")]
        public string Name { get; set; }

        [Display(Name = "物品类别")]
        public string Type { get; set; }

        [Display(Name = "物品描述")]
        public string Detail { get; set; }
    }
}