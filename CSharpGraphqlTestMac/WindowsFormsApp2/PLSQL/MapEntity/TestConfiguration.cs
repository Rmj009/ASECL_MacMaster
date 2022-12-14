using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    internal class TestConfiguration
    {
        private long ID;
        private String LotCode;
        private String PID;
        private TestConfigurationStatus Status;
        private ProductDevice ProductDevice;
        private List<TestConfiguration_SW_FW_Binding> FwSwBinding;
        private MacDispatchType MacDispatchType;
        private Customer Customer;
        private List<DutDevice> DutDevices;
        private String ForceStopOPId;
        private User ForceStopUser;
        private String ForceStopRemark;

        //private JSONObject ExtraJson;
        //private JSONObject LogTitle;
        //private JSONObject LogLimitUpper;
        //private JSONObject LogLimitLower;
        private int TrayMode;

        private User CreatedOwner;
        private DateTime FinishDate;        //Timestamp
        private DateTime CreatedTime;               //Timestamp
    }
}