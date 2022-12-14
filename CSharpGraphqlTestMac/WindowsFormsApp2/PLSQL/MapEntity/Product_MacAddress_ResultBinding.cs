using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Product_MacAddress_ResultBinding
    {
        private long ID;
        private Product_MacAddress Mac_ID;
        private String OPId;
        private String TestFlow;
        private DutDevice dutDevice;
        private TestResultStatus ResultStatus_ID;

        //private JSONObject ResultSummary;
        private String Path;

        private String Barcode;
        private String Barcode_Vendor;
        private Product_TestConfiguration Product_TestConfiguration_ID;
        private DateTime CreatedTime;       //Timestamp
    }
}