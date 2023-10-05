using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using static MT8872AFIXTURE.LCS5.WifiObjects;
//using EventHandle;
//using WNC.API;

namespace MT8872AFIXTURE.LCS5
{
    internal class Telents
    {

        public class Telnet
        {
            public static event EventLogHandler Message;
            private string ip = string.Empty;
            private enum Verbs
            {
                WILL = 251,
                WONT = 252,
                DO = 253,
                DONT = 254,
                IAC = 255
            }

            private enum Options
            {
                SGA = 3
            }

            private string eth = string.Empty;
            public string Eth
            {
                get
                {
                    return this.eth;
                }
            }

            public string IP
            {
                get
                {
                    //DisplayMsg(LogType.Log, "IPIPIPIP" + this.IP);
                    return this.ip;
                }
            }

            private List<string> ipList = new List<string>();
            public List<string> IpList
            {
                get
                {
                    return this.ipList;
                }
            }

            private int port = 23;
            public int Port
            {
                get
                {
                    return this.port;
                }
            }

            private string login = string.Empty;
            public string Login
            {
                get
                {
                    return this.login;
                }
            }

            private string password = string.Empty;
            public string Password
            {
                get
                {
                    return this.password;
                }
            }

            private bool isConnect = false;
            public bool IsConnect
            {
                get
                {
                    return this.isConnect;
                }
            }


            private NetworkStream nwkStr;
            private TcpClient client = null;

            public double pingTimeS = -99;

            protected virtual void OnMessageDisplay(EventLogArgs e)
            {
                if (Message != null)
                    Message(e);
            }
            
            private void DisplayMsg(LogType type, string message)
            {
                EventLogArgs eLog = new EventLogArgs("[ " + type.ToString() + " ]  " + message);
                OnMessageDisplay(eLog);
            }

            public bool LoginTelnet(string keyword, int timeOutMs)
            {
                try
                {
                    string res = string.Empty;
                    //DisplayMsg(LogType.Log, "====Debuger6===");

                    if (!isConnect)
                    {
                        Initial();
                    }

                    if (this.login.Length == 0)
                    {
                        goto Exit;
                    }

                    if (ChkResponse("login", ref res, timeOutMs))
                    {
                        return false;
                    }

                    Transmit(this.login);

                    if (ChkResponse("Password", ref res, timeOutMs))
                    {
                        return false;
                    }

                    Transmit(this.password);

                Exit:
                    if (ChkResponse(keyword, ref res, timeOutMs))
                    {
                        this.isConnect = true;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                    return false;
                }
            }

            public bool Initial()
            {
                try
                {
                    DisplayMsg(LogType.Telnet, "Initial telnet");

                    if (this.eth == string.Empty)
                    {
                        this.client = new TcpClient();
                    }
                    else
                    {
                        this.client = new TcpClient(new IPEndPoint(IPAddress.Parse(eth), 0));
                    }
                    this.client.SendTimeout = 5;
                    this.client.Connect(this.ip, this.port);
                    this.nwkStr = client.GetStream();
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                    return false;
                }
            }

            public bool Dispose()
            {
                try
                {
                    DisplayMsg(LogType.Telnet, "Dispose telnet");

                    if (this.nwkStr != null)
                    {
                        this.nwkStr.Close();
                        this.nwkStr.Dispose();
                    }

                    if (client != null)
                    {
                        this.client.Close();
                    }

                    this.isConnect = false;
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Log, "SHOW_THE_ISSUE");
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                    return false;
                }
            }

            public bool ClearArp()
            {
                try
                {
                    DisplayMsg(LogType.Ping, "Clear ARP..");
                    Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = string.Empty;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.StandardInput.WriteLine("arp -a");
                    process.StandardInput.WriteLine("arp -d");
                    process.StandardInput.WriteLine("arp -a");
                    process.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    return false;
                }
            }

            public bool Ping(string sIp, int timeOutMs)
            {
                try
                {
                    ClearArp();

                    PingReply pingReply;
                    PingOptions pingOption = new PingOptions();
                    System.Net.NetworkInformation.Ping sensor = new System.Net.NetworkInformation.Ping();
                    Process process = new Process();
                    process.StartInfo.FileName = "arp";
                    process.StartInfo.Arguments = "-d";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    System.Threading.Thread.Sleep(100);
                    pingOption.DontFragment = true;

                    string data = "aaaaaaaaaaaaaaaaaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);


                    DateTime dt;
                    TimeSpan ts;
                    dt = DateTime.Now;

                    while (true)
                    {
                        ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                        if (ts.TotalMilliseconds > timeOutMs)
                        {
                            DisplayMsg(LogType.Ping, "Ping " + sIp + " fail !!");
                            return false;
                        }

                        DisplayMsg(LogType.Ping, "Ping " + sIp + " ...");
                        pingReply = sensor.Send(sIp, 1 * 1000, buffer, pingOption);

                        if (pingReply.Status == IPStatus.Success)
                        {
                            DisplayMsg(LogType.Ping, "Ping " + sIp + " success.");
                            DisplayMsg(LogType.Ping, "Time (s) :" + ts.TotalSeconds.ToString());
                            pingTimeS = ts.TotalSeconds;
                            return true;
                        }

                        System.Threading.Thread.Sleep(500);
                    }

                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Ping, "Ping " + sIp + " fail !!");
                    DisplayMsg(LogType.Exception, ex.Message);
                    return false;
                }
            }

            public bool Ping(string sEth, string sIp, int timeOutMs)
            {
                try
                {
                    ClearArp();

                    DateTime dt;
                    TimeSpan ts;
                    string cmd = "ping -S " + sEth + " " + sIp;
                    string res = string.Empty;
                    string msg = string.Empty;
                    string ctrlC = Convert.ToString((char)0x03);

                    Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    DisplayMsg(LogType.Ping, "cmd : " + cmd);
                    process.StandardInput.WriteLine(cmd);

                    dt = DateTime.Now;

                    while (true)
                    {
                        ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                        if (ts.TotalMilliseconds > timeOutMs)
                        {
                            DisplayMsg(LogType.Ping, "Ping " + sIp + " fail !!");
                            return false;
                        }

                        res = process.StandardOutput.ReadLine();
                        DisplayMsg(LogType.Log, res);
                        msg += res;

                        if ((msg.Contains("Reply from " + sIp) || msg.Contains("回覆自 " + sIp)))
                        {
                            if (msg.Contains("TTL"))
                            {
                                DisplayMsg(LogType.Ping, "Ping " + sIp + " success.");
                                DisplayMsg(LogType.Ping, "Time (s) :" + ts.TotalSeconds.ToString());
                                pingTimeS = ts.TotalSeconds;
                                break;
                            }
                        }

                        System.Threading.Thread.Sleep(50);

                        if (msg.Contains("%"))
                        {
                            msg = string.Empty;
                            process.StandardInput.WriteLine(cmd);
                        }
                    }

                    process.Kill();
                    process.Dispose();
                    process.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Ping, "Ping " + sIp + " fail !!");
                    DisplayMsg(LogType.Exception, ex.Message);
                    return false;
                }
            }

            //public bool PingList(int timeOutMs)
            //{
            //    try
            //    {
            //        foreach (string sIp in ipList)
            //        {
            //            if (!Ping(sIp, timeOutMs))
            //            {
            //                return false;
            //            }
            //        }

            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        DisplayMsg(LogType.Ping, "Ping " + ip + " fail !!");
            //        DisplayMsg(LogType.Exception, ex.Message);
            //        return false;
            //    }
            //}

            public bool PingList(string sEth, int timeOutMs)
            {
                try
                {
                    foreach (string sIp in ipList)
                    {
                        if (!Ping(sEth, sIp, timeOutMs))
                        {
                            return false;
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Ping, "Ping " + ip + " fail !!");
                    DisplayMsg(LogType.Exception, ex.Message);
                    return false;
                }
            }

            public bool Transmit(string cmd)
            {
                try
                {
                    if (!this.client.Connected)
                    {
                        this.isConnect = false;
                        return false;
                    }

                    byte[] data = System.Text.Encoding.ASCII.GetBytes(cmd + "\n");
                    this.nwkStr.Write(data, 0, data.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                    return false;
                }
            }

            public bool Transmit(byte[] value)
            {
                try
                {
                    if (!this.client.Connected)
                    {
                        this.isConnect = false;
                        return false;
                    }

                    this.nwkStr.Write(value, 0, 1);
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                    return false;
                }
            }

            public bool ChkResponse(string keyword, ref string res, int timeOutMs)
            {
                res = string.Empty;
                try
                {
                    if (!this.client.Connected)
                    {
                        this.isConnect = false;
                        return false;
                    }

                    StringBuilder sbuilder = new StringBuilder();
                    DateTime dt;
                    TimeSpan ts;

                    dt = DateTime.Now;
                    while (true)
                    {
                        ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                        if (ts.TotalMilliseconds > timeOutMs)
                        {
                            DisplayMsg(LogType.Telnet, res);
                            DisplayMsg(LogType.Error, "Check '" + keyword + "' timeout !!");
                            return false;
                        }

                        System.Threading.Thread.Sleep(50);
                        //DisplayMsg(LogType.Log, "====Debuger7===");

                        ParseTelnet(sbuilder);
                        //DisplayMsg(LogType.Log, "====Debuger8===");

                        res = sbuilder.ToString();

                        if (res.ToUpper().Contains(keyword.ToUpper()))
                        {
                            break;
                        }

                        if (keyword.Length == 0)
                        {
                            break;
                        }
                    }

                    DisplayMsg(LogType.Telnet, res);
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                    return false;
                }
            }

            private void ParseTelnet(StringBuilder sbuilder)
            {
                try
                {
                    while (this.client.Available > 0)
                    {
                        int option = this.client.GetStream().ReadByte();
                        int verb = -1;

                        switch (option)
                        {
                            case -1:
                                break;
                            case (int)Verbs.IAC:
                                #region Verbs.IAC 255
                                verb = this.client.GetStream().ReadByte();
                                if (verb == -1)
                                {
                                    break;
                                }
                                switch (verb)
                                {
                                    case (int)Verbs.IAC:
                                        sbuilder.Append(verb);
                                        break;
                                    case (int)Verbs.DO:
                                    case (int)Verbs.DONT:
                                    case (int)Verbs.WILL:
                                    case (int)Verbs.WONT:
                                        int _option = this.client.GetStream().ReadByte();
                                        if (_option == -1)
                                        {
                                            break;
                                        }
                                        this.client.GetStream().WriteByte((byte)Verbs.IAC);
                                        if (_option == (int)Options.SGA)
                                        {
                                            this.client.GetStream().WriteByte(verb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                        }
                                        else
                                        {
                                            this.client.GetStream().WriteByte(verb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                        }
                                        this.client.GetStream().WriteByte((byte)_option);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            #endregion
                            default:
                                sbuilder.Append((char)option);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Exception, ex.Message);
                    this.isConnect = false;
                }
            }

            //public Telnet(string section)
            //{
            //    string[] ipArray = Func.ReadINI("Setting", section, "IP", string.Empty).Split(';');

            //    this.ipList = new List<string>();
            //    for (int i = 0; i < ipArray.Length; i++)
            //    {
            //        if (ipArray[i].Contains("."))
            //        {
            //            this.ipList.Add(ipArray[i]);
            //        }
            //    }

            //    this.isConnect = false;
            //    this.login = Func.ReadINI("Setting", section, "Login", string.Empty);
            //    this.password = Func.ReadINI("Setting", section, "Password", string.Empty);
            //    this.eth = Func.ReadINI("Setting", section, "EthIP", string.Empty);
            //    this.ip = Func.ReadINI("Setting", section, "IP", string.Empty);
            //    this.port = Convert.ToInt16(Func.ReadINI("Setting", section, "Port", "23"));

            //    if (this.eth == string.Empty)
            //    {
            //        this.client = new TcpClient();
            //    }
            //    //else
            //    //{
            //    //    this.client = new TcpClient(new IPEndPoint(IPAddress.Parse(eth), 0));
            //    //}
            //}

            public Telnet(string sLogin, string sPassword, string sEth, string sIp, int iPort)
            {
                this.isConnect = false;
                this.login = sLogin;
                this.password = sPassword;
                this.ip = sIp;
                this.port = iPort;
                this.eth = sEth;
                this.client = new TcpClient(new IPEndPoint(IPAddress.Parse(eth), 0));
            }
        }
    }

}
