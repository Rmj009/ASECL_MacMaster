using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class TrayPosition
    {
        private long ID;
        private TestConfiguration TestConfiguration;
        private MacAddress_ResultBinding ResultBinding;
        private Tray Tray;
        private int MatrixIndex;
        private int X;
        private int Y;
        private String ErrorCode;
        private DateTime CreatedTime;       //Timestamp
    }
}