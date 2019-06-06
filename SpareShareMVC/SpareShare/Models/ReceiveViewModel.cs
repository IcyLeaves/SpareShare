using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpareShare.Models
{
    public class UploadQuestsViewModel
    {
        public Things Thing { get; set; }
        public Users Donator { get; set; }

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
        public Quests Quest { get; set; }
        public int prior { get; set; }
    }

    public class UploadCommentViewModel
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

        public Things Thing { get; set; }

        public Users Donator { get; set; }
    }
}