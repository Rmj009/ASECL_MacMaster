using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class Dispatch
    {
        public ResponseDispatch DispatchProductMac { get; set; }

        public class ResponseDispatch
        {
            public bool Result { get; set; }
            //public bool IsNoMac { get; set; }
            public string Mac { get; set; }
            public string Name { get; set; }
            public bool IsFinish { get; set; }
        }
    }
}
