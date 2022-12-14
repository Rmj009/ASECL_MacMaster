using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.PLSQL.Entity
{
    public class User
    {
        private long ID;
        private String EmployeeID;
        private String RealName;
        private String NickName;
        private String EMail;
        private String Phone;
        private Role Role;
        private String Password;
        private User AgreeUser;
        private User DisagreeUser;
        private short IsActived;
        private String DisagreeReason;
        private DateTime LastActivedTime;                                            //Timestamp
        private DateTime LastDisagreeActiveTime;                           //Timestamp
    }
}