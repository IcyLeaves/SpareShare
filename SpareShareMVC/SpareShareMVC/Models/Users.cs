//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpareShareMVC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Users
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Nullable<System.DateTime> RegTime { get; set; }
        public string Province { get; set; }
        public string School { get; set; }
        public string Email { get; set; }
        public string QQ { get; set; }
        public string Sex { get; set; }
        public string IsAdmin { get; set; }
    }
}
