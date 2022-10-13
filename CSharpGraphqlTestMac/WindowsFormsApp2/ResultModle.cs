using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class ResponseModel
    {
        public ResponseResult CreateMacAddress { get; set; }

        public class ResponseResult
        {
            public bool Result { get; set; }
           
            //public bool IsNoMac { get; set; }
            //public string Mac { get; set; }
            //public string Name { get; set; }
            //public bool IsFinish { get; set; }
        }
    }
}
