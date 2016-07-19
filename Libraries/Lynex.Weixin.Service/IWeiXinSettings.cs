using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lynex.Weixin.Service
{
    public interface IWeiXinSettings
    {
        string AppId { get; set; }

        string MchId { get; set; }

        string AppSecret { get; set; }
    }
}
