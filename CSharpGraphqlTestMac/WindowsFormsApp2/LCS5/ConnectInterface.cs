using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MT8872AFIXTURE.LCS5
{
    internal class ConnectInterface
    {
        private SerialPort UaRt;
        private SerialPort serialPort;
        private SerialPort JS7K;
        private SerialPort Golden2G_5G;
        private SerialPort Golden6G_BT;
        private SerialPort ShieldUaRt;


        public enum PortType
        {
            DUT_UART = 0,
            DIAG = 1,
            FIXTURE = 2,
            UART = 3,
            Telnet = 4,
            Telnet2 = 5,
            GOLDEN_Uart_2G_5G = 6,
            GOLEN_Uart_6G_BT = 7,
            GOLEN_Telnet_2G_5G_BT = 8,
            GOLEN_Telnet_6G_BT = 9,
            JS7X_GOLEN = 10,
            SSH = 11

        }
        public enum ITEM
        {
            SET_BOOTCMD,
            SET_SerialNumber,
            LED,
            BT_RSSI,
            //------------
            Wifi_2G_RSSI,
            Wifi_5G_RSSI,
            Wifi_6G_RSSI,

        }
        public bool ChkPort(string PortName)
        {
            bool IsPortTheSame = false;
            string[] PortLst = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string port in PortLst)
            {
                if (port.ToUpper() == PortName.ToUpper())
                {
                    IsPortTheSame = true;
                    break;
                }
            }
            return IsPortTheSame;
        }
        public bool ChkResponse(PortType portType, ITEM item, string keyword, int timeOutMs, out string getMsg)
        {
            getMsg = null;
            DateTime dt = DateTime.Now;
            TimeSpan timeSpan= TimeSpan.FromSeconds(timeOutMs);
            int count = 0;
            int delayMs = 50;
            
            
            string res = string.Empty;
            string log = string.Empty;
            try
            {
                while (true)
                {
                    timeSpan = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                    if (timeSpan.TotalMilliseconds > timeOutMs)
                    {
                        //Dispaly(LogType.Err, "Check timeout");
                    }
                    else
                    {
                        Thread.Sleep(delayMs);
                        switch(portType)
                        {
                            case PortType.UART:
                                {
                                    if (!ChkPort(UaRt.PortName))
                                    {
                                        //SAVE_DATA_LOG
                                        MessageBox.Show("Port_NOT_FOUND=>" + UaRt.PortName);
                                        return false;
                                    }
                                    if (true)
                                    {

                                    }
                                    break;
                                }
                            case PortType.DIAG:
                                {
                                    if (true)
                                    {
                                        break;
                                    }
                                }
                            case PortType.DUT_UART:
                                {
                                    if (true)
                                    {
                                        break;
                                    }
                                }
                            case PortType.GOLDEN_Uart_2G_5G:
                                {
                                    if (true)
                                    {
                                        break;
                                    }
                                }
                            case PortType.GOLEN_Telnet_2G_5G_BT:
                                {
                                    if (true)
                                    {
                                        break;
                                    }
                                }
                            case PortType.GOLEN_Telnet_6G_BT:
                                {
                                    if (true)
                                    {
                                        break;
                                    }
                                }
                        }
                    }
                }
                //return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool IsPingOK()
        {
            return false;
        }
    }
}
