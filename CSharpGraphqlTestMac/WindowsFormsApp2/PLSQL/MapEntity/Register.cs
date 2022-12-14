using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Register
    {
        private long ID;
        private User RegisterUser;
        private RegisterType RegisterType;
        private DateTime CreatedTime;
    }
}