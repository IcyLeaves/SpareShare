using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SpareShareMVC.Models
{
    public class SSEditManageViewModel
    { 

        [Required(ErrorMessage = "请选择性别")]
        [Display(Name = "性别")]
        public string Sex { get; set; }
        
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "请输入正确格式的邮箱")]
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入QQ")]
        [RegularExpression(@"[0-9]*", ErrorMessage = "请输入正确格式的QQ")]
        [Display(Name = "QQ")]
        public string QQ { get; set; }

        [Required(ErrorMessage = "请选择省份")]
        [Display(Name = "省份")]
        public string Province { get; set; }

        [Required(ErrorMessage = "请输入学校")]
        [Display(Name = "学校")]
        public string School { get; set; }
    }

}