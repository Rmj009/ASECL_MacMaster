using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class ProductUploadResponse
    {
        public UploadContent SyncProductMACResultWithoutLog { get; set; }

        public class UploadContent
        {
            public int Total { get; set; }
            public int Fail { get; set; }
            public int Pass { get; set; }
            public int Unused { get; set; }
            public int Using { get; set; }
            public string TrayName { get; set; }
            public int TrayX { get; set; }
            public int TrayY { get; set; }
            public string LastMac { get; set; }
            public string LastMacName { get; set; }
        }
    }
}
