using MacMaster.PLSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.MapEntity
{
    public class Login
    {
        private long ID;
        private User LoginUser;
        private String JwtToken;
        private LoginType LoginType;
        private DateTime LastModifyTime;        //Timestamp
        private DateTime CreatedTime;           //Timestamp
    }
}