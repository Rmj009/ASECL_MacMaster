using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class MacAddress
    {
        private long ID;
        private String Name;
        private String Address;
        private long AddressDecimal;
        private MacStatus MacStatus;
        private MacType MacType;
        private TestConfiguration TestConfiguration;
        private User TestUser;
        private User CreatedOwner;
        private DateTime CreatedTime;               //Timestamp
    }
}