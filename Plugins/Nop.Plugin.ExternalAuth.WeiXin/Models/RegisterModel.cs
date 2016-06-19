
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.ExternalAuth.WeiXin.Models
{
    public class RegisterModel
    {
        [NopResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Fields.Password")]
        [AllowHtml]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Fields.ConfirmPassword")]
        [AllowHtml]
        public string ConfirmPassword { get; set; }
    }
}
