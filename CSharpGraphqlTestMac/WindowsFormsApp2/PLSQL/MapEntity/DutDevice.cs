using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class DutDevice
    {
        private long ID;
        private String HostName;
        private String ProductDevice;
        private String Remark;
        private String GroupPC;
        private User CreatedOwner;
        private DateTime CreatedTime;           //Timestamp
    }
}