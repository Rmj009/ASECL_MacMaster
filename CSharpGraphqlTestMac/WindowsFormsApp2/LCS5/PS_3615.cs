using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT8872AFIXTURE.LCS5
{
    internal class PS_3615
    {
        private enum IFACE
        {
            RS232,
            GPIB,
            NONE,
        }
    }
    public class RS232
    {
        public static event EventLogHandler Message;
        private StopBits stopBits = StopBits.None;
        private System.IO.Ports.SerialPort rs232;
        private string portName = string.Empty;
        private Parity parity = Parity.None;
        private bool rtsEnable = false;
        private int dataBits = -1;
        private int baudRate = -1;

        // ===========================================
        protected virtual void OnMessageDisplay(EventLogArgs e)
        {
            if (Message != null)
                Message(e);
        }
        public RS232(SerialPort port)
        {
            rs232 = new SerialPort();

            this.portName = port.PortName;
            this.baudRate = port.BaudRate;
            this.stopBits = port.StopBits;
            this.parity = port.Parity;
            this.dataBits = port.DataBits;
            this.rtsEnable = port.RtsEnable;

            this.rs232 = port;
        }
        public StopBits _StopBits
        {
            get
            {
                return this.stopBits;
            }
        }
        public Parity _Parity
        {
            get
            {
                return this.parity;
            }
        }
        private void DisplayMsg(LogType type, string message)
        {
            EventLogArgs eLog = new EventLogArgs("[ " + type.ToString() + " ]  " + message);
            OnMessageDisplay(eLog);
        }
        private void GcErase()
        {
            int count = 0;

        Re:
            try
            {
                try
                {
                    rs232.Dispose();
                    rs232 = null;
                }
                catch (Exception e)
                {
                    DisplayMsg(LogType.Exception, e.Message);
                }


                GC.Collect();
                GC.WaitForPendingFinalizers();
                DisplayMsg(LogType.Log, "Wait Erase Power Supply Port 1 s");
                System.Threading.Thread.Sleep(1000);

                DisplayMsg(LogType.Log, "Create Power Supply Port..");

                rs232 = new SerialPort();
                rs232.PortName = this.portName;
                rs232.BaudRate = this.baudRate;
                rs232.StopBits = this.stopBits;
                rs232.Parity = this.parity;
                rs232.DataBits = this.dataBits;
                rs232.RtsEnable = this.rtsEnable;

            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 2)
                {
                    DisplayMsg(LogType.Retry, "Retry count : " + count.ToString());
                    goto Re;
                }
            }
        }
        public bool Chk(string cmd, string keyword, int timeOutMs)
        {
            try
            {
                DateTime dt;
                TimeSpan ts;

                dt = DateTime.Now;
                string res = string.Empty;

                this.Write(cmd);

                while (true)
                {
                    ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (ts.TotalMilliseconds > timeOutMs)
                    {
                        Debug.Write("Check timeout");
                        DisplayMsg(LogType.Error, "Check timeout");
                        return false;
                    }

                    res = Read();

                    if (res.Contains(keyword))
                    {
                        return true;
                    }

                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                return false;
            }
        }
        public bool Write(string cmd, int delayMs)
        {
            int count = 0;

        Re:
            try
            {
                if (!this.Open())
                {
                    return false;
                }

                Debug.Write(cmd);
                rs232.Write(cmd + "\r");

                System.Threading.Thread.Sleep(delayMs);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 2)
                {
                    GcErase();
                    goto Re;
                }
                return false;
            }
        }
        public bool Write(string cmd)
        {
            int count = 0;

        Re:
            try
            {
                if (!this.Open())
                {
                    return false;
                }

                Debug.Write(cmd);
                rs232.Write(cmd + "\r\n");

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 2)
                {
                    GcErase();
                    goto Re;
                }
                return false;
            }
        }
        public bool _RtsEnable
        {
            get
            {
                return this.rtsEnable;
            }
        }
        public bool Close()
        {
            int count = 0;

        Re:
            try
            {
                if (rs232.IsOpen)
                {
                    Debug.Write("Close Power Supply uart");
                    rs232.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 2)
                {
                    GcErase();
                    goto Re;
                }
                return false;
            }
        }
        public bool Open()
        {
            int count = 0;

        Re:
            try
            {
                if (!rs232.IsOpen)
                {
                    Debug.Write("Open Power Supply uart");
                    rs232.Open();
                    System.Threading.Thread.Sleep(100);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 2)
                {
                    GcErase();
                    goto Re;
                }
                return false;
            }
        }
        public string _PortName
        {
            get
            {
                return this.portName;
            }
        }
        public string Read()
        {
            string res = string.Empty;
            int count = 0;

        Re:
            try
            {
                if (!this.Open())
                {
                    return "ERROR";
                }

                if (rs232.BytesToRead > 0)
                {
                    res = rs232.ReadExisting();
                    Debug.Write(res);
                }
                return res;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                if (count++ < 2)
                {
                    GcErase();
                    goto Re;
                }
                return "ERROR";
            }
        }
        public string Qury(string cmd, int delayMs)
        {
            try
            {
                this.Write(cmd, delayMs);

                return Read();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                DisplayMsg(LogType.Exception, ex.Message);
                return "ERROR";
            }
        }
        public int _BaudRate
        {
            get
            {
                return this.baudRate;
            }
        }
        public int _DataBits
        {
            get
            {
                return this.dataBits;
            }
        }

    }

}
