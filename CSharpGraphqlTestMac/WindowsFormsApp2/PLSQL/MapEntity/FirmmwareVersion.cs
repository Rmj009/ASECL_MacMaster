using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class FirmwareVersion
    {
        private long ID;
        private String Version;
        private String Path;
        private String Remark;
        private String MD5;
        private User CreatedOwner;
        private DateTime CreatedTime;           //Timestamp
    }
}