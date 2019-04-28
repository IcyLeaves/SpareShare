using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpareShareMVC
{
    public class ValidateFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            int MaxContentLength = 1024 * 1024 * 4;
            string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png",".jpeg"};

            var files = value as IEnumerable<HttpPostedFileBase>;
            var file = files.FirstOrDefault();
            if (file == null)
                return true;
            else if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
            {
                ErrorMessage = "请上传你的图片类型: " + string.Join(", ", AllowedFileExtensions);
                return false;
            }
            else if (file.ContentLength > MaxContentLength)
            {
                ErrorMessage = "上传图片过大，不能超过4MB";
                return false;
            }
            else
                return true;
        }
    }
}