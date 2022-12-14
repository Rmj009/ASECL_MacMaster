using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Tray
    {
        private long ID;
        private String Name;
        private TestConfiguration TestConfiguration;
        private Product_TestConfiguration Product_TestConfiguration;
        private TrayType TrayType;
        private int Width;
        private int Height;
        private int Order;
        private DateTime CreatedTime;//Timestamp
    }
}