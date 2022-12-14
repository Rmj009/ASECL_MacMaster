using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class SyncPOResponse
    {
        public ResponseModelContent SyncPo { get; set; }
        public class ResponseModelContent
        {
            public bool Result { get; set; }
        }
    }
}
