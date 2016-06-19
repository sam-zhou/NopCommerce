using System.ComponentModel.DataAnnotations;

using System.Web.Mvc;
using System.Web.Security;
using Nop.Web.Framework;

namespace Nop.Plugin.ExternalAuth.WeiXin.Models
{

    public class RegisterModel
    {
        [NopResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Fields.Password")]
        [AllowHtml]
        [Required]
        [DataType(DataType.Password)]
        [MembershipPassword()]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Fields.ConfirmPassword")]
        [AllowHtml]
        [Required]
        [DataType(DataType.Password)]
        [MembershipPassword()]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }

        
        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; set; }
    }
}
