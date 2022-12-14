using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Product_TrayPosition
    {
        private long ID;
        private Product_TestConfiguration Product_TestConfiguration;
        private Product_MacAddress_ResultBinding Product_MacAddress_ResultBinding;
        private Product_Tray Product_Tray;
        private int MatrixIndex;
        private int X;
        private int Y;
        private String ErrorCode;
        private DateTime CreatedTime;               //Timestamp
    }
}