using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpareShare.Models
{
    public class UploadThingsViewModel
    {
        public Quests Quest;

        public Users Receiver;

        [Required(ErrorMessage = "请输入物品名称")]
        [Display(Name = "物品名称")]
        public string tName { get; set; }

        [Required(ErrorMessage = "请输入物品类别")]
        [Display(Name = "物品类别")]
        public string tType { get; set; }

        [Required(ErrorMessage = "请输入物品描述")]
        [Display(Name = "物品描述")]
        public string tDetail { get; set; }

        [Display(Name ="实物图")]
        public IEnumerable<HttpPostedFileBase> Image { get; set; }
    }

    public class ThingsListViewModel
    {
        public Things Thing { get; set; }

        public int prior { get; set; }

        public double similar { get; set; }
    }

    public class DetailsViewModel
    {
        public Things Thing { get; set; }

        public Users Donator { get; set; }

        public Quests Quest { get; set; }

        public Users Receiver { get; set; }

        public Comments Comment { get; set; }

        public Checks Check { get; set; }
    }
}