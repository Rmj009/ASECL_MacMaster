using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class MGRStatusResponse
    {
        public List<ResponseMGRStatusContent> ResponseMGRStatus { get; set; }

        public class ResponseMGRStatusContent
        {
            public String CHANNEL_NAME { get; set; }
            public String MEMBER_ID { get; set; }
            public String MEMBER_HOST { get; set; }
            public String MEMBER_PORT { get; set; }
            public String MEMBER_STATE { get; set; }
            public String MEMBER_ROLE { get; set; }
            public String MEMBER_VERSION { get; set; }
        }
    }
}
