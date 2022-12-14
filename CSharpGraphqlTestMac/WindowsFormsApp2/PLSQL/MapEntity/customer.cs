using MacMaster.PLSQL.Entity;
using MacMaster.PLSQL.MapEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL
{
    internal class Customer
    {
        private long ID;
        private String Name;
        private String Phone;
        private String Remark;
        private User CreatedOwner;
        private List<TestConfiguration> TestConfigurations;
        private DateTime CreatedTime;               //Timestamp
    }
}