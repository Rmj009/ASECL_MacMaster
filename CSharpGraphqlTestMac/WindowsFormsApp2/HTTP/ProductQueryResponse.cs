using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class ProductQueryResponse
    {
        public List<ProductQueryContent> QueryProduct { get; set; }
        public class ProductQueryContent {
            public long ID { get; set; }
            public string Name { get; set; }
            public string Remark { get; set; }
            public String CreatedTime { get; set; }
        }


    }
}
