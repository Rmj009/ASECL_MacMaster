using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.Entity
{
    public class Software
    {
        private long ID;
        private String Name;
        private String Version;
        private User CreatedOwner;
        private DateTime CreatedTime;   //Timestamp
    }
}