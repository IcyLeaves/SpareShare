using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SpareShareMVC.Models
{
    public class SSEditManageAdministorViewModel
    {
        [Required]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "性别")]
        public string Sex { get; set; }
    }

    public class SSAdminChartsViewModel
    {
        [Display(Name ="起始日期")]
        public DateTime StartTime { get; set; }

        [Display(Name = "结束日期")]
        public DateTime EndTime { get; set; }
    }
}