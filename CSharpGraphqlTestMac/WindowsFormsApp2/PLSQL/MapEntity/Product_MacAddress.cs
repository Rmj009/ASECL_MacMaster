using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Product_MacAddress
    {
        private long ID;
        private String LotCode;
        private String PO;
        private String Name;
        private String Address;
        private long AddressDecimal;
        private MacStatus MacStatus;
        private MacType MacType;
        private Product_TestConfiguration Product_TestConfiguration;
        private String SipSerialName;
        private String SipLicense;
        private DateTime CreatedTime;       //Timestamp
    }
}