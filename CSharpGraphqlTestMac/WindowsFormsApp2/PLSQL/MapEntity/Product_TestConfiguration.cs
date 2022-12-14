using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Product_TestConfiguration
    {
        private long ID;
        private String LotCode;
        private String PID;
        private String PO;
        private TestConfigurationStatus Status;
        private ProductDevice ProductDevice;
        private List<Product_TestConfiguration_SW_FW_Binding> FwSwBinding;
        private Customer Customer;
        private List<DutDevice> DutDevices;
        private String ForceStopOPId;
        private String ForceStopRemark;

        //private JSONObject ExtraJson;
        //private JSONObject LogTitle;
        //private JSONObject LogLimitUpper;
        //private JSONObject LogLimitLower;
        private int TrayMode;

        private DateTime FinishDate;            //Timestamp
        private DateTime CreatedTime;           //Timestamp
    }
}