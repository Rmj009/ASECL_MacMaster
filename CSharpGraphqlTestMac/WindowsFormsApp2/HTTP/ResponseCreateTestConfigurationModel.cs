using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class ResponseCreateTestConfigurationModel
    {
        public ResponseCreateTestConfiguration CreateProductTestConfiguration { get; set; }

        public class ResponseCreateTestConfiguration
        {

            public bool Result { get; set; }
            //public bool IsNoMac { get; set; }
            public string CurrentStatus { get; set; }
            public string SwName { get; set; }
            public string SwVersion { get; set; }
            public string FwName { get; set; }
            public string FwVersion { get; set; }

        }
    }
}
