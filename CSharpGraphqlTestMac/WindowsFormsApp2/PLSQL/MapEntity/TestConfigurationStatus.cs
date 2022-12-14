using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class TestConfigurationStatus
    {
        private long ID;
        private String Name;
        private DateTime CreatedTime;

        public DateTime UnixEpoch()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public long ToMillisecondsSinceUnixEpoch(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(UnixEpoch()).TotalMilliseconds;
        }
    }
}