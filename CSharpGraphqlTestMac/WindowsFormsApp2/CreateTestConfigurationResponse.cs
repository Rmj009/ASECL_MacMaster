using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class CreateTestConfigurationResponse
    {
        public CreateTestConfigurationRes CreateTestConfiguration { get; set; }
        public class CreateTestConfigurationRes
        {
            public bool Result { get; set; }

            public string CurrentStatus { get; set; }

            public string SwName { get; set; }

            public string SwVersion { get; set; }

            public string FwName { get; set; }

            public string FwVersion { get; set; }
        }
    }
}
