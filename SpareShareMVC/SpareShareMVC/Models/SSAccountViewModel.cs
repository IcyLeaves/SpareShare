using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SpareShareMVC.Models
{
    public class SSLoginViewModel
    {
        [Required(ErrorMessage ="请输入用户名")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }
    }

    public class SSRegisterViewModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

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
        [Display(Name ="省份")]
        public string Province { get; set; }

        [Required(ErrorMessage = "请输入学校")]
        [Display(Name ="学校")]
        public string School { get; set; }

        [Display(Name ="是否为管理员")]
        public string isAdmin { get; set; }

        [RegularExpression(@"admin", ErrorMessage = "管理员密码错误")]
        [Display(Name ="管理员密码")]
        public string AdminPassword { get; set; }
    }
}