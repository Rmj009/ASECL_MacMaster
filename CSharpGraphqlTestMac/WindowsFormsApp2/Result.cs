using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class Result
    {
        public ResponseResult SyncProductMACResultWithoutLog { get; set; }

        public class ResponseResult
        {
            public int Total { get; set; }
            public int Fail { get; set; }
            public int Pass { get; set; }
            public int Unused { get; set; }
            public int Using { get; set; }
            public String LastMac { get; set; }
            public String LastMacName { get; set; }
            //public bool IsNoMac { get; set; }
            //public string Mac { get; set; }
            //public string Name { get; set; }
            //public bool IsFinish { get; set; }
        }
    }
}
