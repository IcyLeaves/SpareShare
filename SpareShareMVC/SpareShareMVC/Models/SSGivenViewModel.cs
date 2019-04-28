using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SpareShareMVC.Models
{
    public class SSUploadGivenThingsViewModel
    {
        [Required(ErrorMessage = "请输入物品名称")]
        [Display(Name ="物品名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "请输入物品类别")]
        [Display(Name = "物品类别")]
        public string Type { get; set; }

        [Required(ErrorMessage = "请输入捐赠理由")]
        [Display(Name = "捐赠理由")]
        public string Reason { get; set; }
        
        [Display(Name ="实物图")]
        [ValidateFile]
        public IEnumerable<HttpPostedFileBase> Image { get; set; }
    }

    public class SSSearchReceiveThingsViewModel
    {
        public Quests Quest { get; set; }

        public int prior { get; set; }
    }
    //public class SSEditManageGivenPeopleViewModel
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