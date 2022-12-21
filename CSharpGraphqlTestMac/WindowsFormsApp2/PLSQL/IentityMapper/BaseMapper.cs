using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.IentityMapper
{
    public interface BaseMapper<T>
    {
        void insert(T type);

        List<T> getAll();

        int getTotalCount();

        void update(T type);

        void deleteByID(long id);
    }
}
