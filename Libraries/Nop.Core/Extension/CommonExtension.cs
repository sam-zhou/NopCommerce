using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Extension
{
    public static class CommonExtension
    {
        public static long GetCurrentTimeStamp()
        {
            long unixTimestamp = DateTime.Now.AddHours(8).Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            return unixTimestamp;
        }
    }
}
