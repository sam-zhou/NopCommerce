using System.ComponentModel.DataAnnotations;

using System.Web.Mvc;
using System.Web.Security;
using FluentValidation.Attributes;
using Nop.Web.Framework;

namespace Nop.Plugin.ExternalAuth.WeiXin.Models
{
    [Validator(typeof(RegisterValidator))]
    public class RegisterModel
    {
        [NopResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Fields.Password")]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Fields.ConfirmPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        
        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; set; }
    }
}
