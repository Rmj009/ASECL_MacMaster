using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class ProductFamily
    {
        private long ID;
        private String Name;
        private String Remark;
        private List<ProductDevice> productDevices;
        private User CreatedOwner;
        private DateTime CreatedTime;       //Timestamp
    }
}