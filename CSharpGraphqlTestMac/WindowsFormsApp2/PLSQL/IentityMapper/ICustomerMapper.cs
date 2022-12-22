using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.IentityMapper
{
    public interface CustomerMapper : BaseMapper<Task>
    {
        CustomerMapper GetByName(String name);

    }
}
