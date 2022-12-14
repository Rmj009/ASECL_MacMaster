using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class MacAddress
    {
        public Response QueryLastBTMacAddressInMP { get; set; }

        public class Response
        {
            //public bool Result { get; set; }
            //public bool IsNoMac { get; set; }
            //public string Mac { get; set; }
            public string Address { get; set; }
            //public bool IsFinish { get; set; }
        }
    }
}
