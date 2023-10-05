using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT8872AFIXTURE.LCS5
{
    #region Event Define
    /// <summary>
    /// 委派宣告.
    /// </summary>
    /// <param name="o">觸發Event的元作</param>
    /// <param name="e">Event的訊息</param>
    public delegate void EventLogHandler(EventLogArgs e);

    /// <summary>
    /// Event 參數覆寫 新增_S_LOG,_DT_CURR_DATETIME,_OBJ_TEST_RESULT
    /// </summary>
    public class EventLogArgs : EventArgs
    {
        public string _S_LOG = string.Empty;
        public DateTime _DT_CURR_DATETIME = DateTime.Now;


        public EventLogArgs()
        {
            _DT_CURR_DATETIME = DateTime.Now;
        }

        public EventLogArgs(string sLog)
        {
            _DT_CURR_DATETIME = DateTime.Now;
            _S_LOG = sLog;
        }

    }
    #endregion Event Define
}