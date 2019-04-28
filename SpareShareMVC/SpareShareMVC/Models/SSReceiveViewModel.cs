using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SpareShareMVC.Models
{
    public class SSUploadReceiveThingsViewModel
    {
        [Required(ErrorMessage = "请输入物品名称")]
        [Display(Name = "物品名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "请输入物品类别")]
        [Display(Name = "物品类别")]
        public string Type { get; set; }

        [Required(ErrorMessage = "请输入受助理由")]
        [Display(Name = "受助理由")]
        public string Reason { get; set; }
    }
    public class SSUploadCommentViewModel
    {
        [Required(ErrorMessage = "请评价新旧程度")]
        [Display(Name = "新旧程度")]
        public int NewScore { get; set; }

        [Required(ErrorMessage = "请评价符合描述程度")]
        [Display(Name = "符合描述程度")]
        public int SimilarScore { get; set; }

        [Required(ErrorMessage = "请评价实用程度")]
        [Display(Name = "实用程度")]
        public int UsefulScore { get; set; }

        [Required(ErrorMessage = "请评价送达速度")]
        [Display(Name = "送达速度")]
        public int SpeedScore { get; set; }

        [Required(ErrorMessage = "请评价美观程度")]
        [Display(Name = "美观程度")]
        public int BeautifulScore { get; set; }

        [Required(ErrorMessage = "请发表一段评论")]
        [Display(Name = "评论")]
        public string Text { get; set; }
    }
    public class SSSeachGivenThingsViewModel
    {
        public Things Thing { get; set; }
        public int prior { get; set; }
    }

    //public class SSEditManageReceivePeopleViewModel
    //{
    //    [Required]
    //    [Display(Name = "姓名")]
    //    public string Name { get; set; }

    //    [Required]
    //    [Display(Name = "性别")]
    //    public string Sex { get; set; }

    //    [Required]
    //    [Display(Name = "邮箱")]
    //    public string Email { get; set; }

    //    [Required]
    //    [Display(Name = "电话")]
    //    public string Tel { get; set; }
    //}
}