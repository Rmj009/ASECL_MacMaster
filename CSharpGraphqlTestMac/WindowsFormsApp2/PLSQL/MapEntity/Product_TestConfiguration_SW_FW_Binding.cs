using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Product_TestConfiguration_SW_FW_Binding
    {
        private long ID;
        private Product_TestConfiguration Product_TestConfiguration;
        private Software Software;
        private Firmware Firmware;
        private int IsActived;
        private DateTime CreatedTime;               //Timestamp
    }
}