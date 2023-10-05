using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MT8872AFIXTURE.LCS5.ConnectInterface;
using static MT8872AFIXTURE.LCS5.Telents;
//using WNC.API;
//using WNC.Equipment;
//using AtsHelper.CommandConsole;
//using DataSystemCtrl;
//using EventHandle;
//using Powersupply3323;

namespace MT8872AFIXTURE.LCS5
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
    }
    public enum LogType
    {
        Log,
        Cmd,
        Eqp,
        Error,
        Exception,
        Warning,
        Retry,
        PS,
        DAQ,
        Camera,
        Uart,
        SA,
        SG,
        Telnet,
        Ping,
        Golden,
        Curl,
        StressMarked,
        Empty,
        Scanner,
        GOLDEN_UART_6G_BT,
        GOLDEN_UART_2G_5G,
        GOLDEN_TELNET_6G_BT,
        GOLDEN_TELNET_2G_5G,
        JS7K,
    }

    public enum CHK
    {
        Enable,
        Disable,
        PASS,
        FAIL,
        Pass,
        Fail,
        OK,
    }

    public enum CTRL
    {
        ON,
        OFF,
        Enable,
        Disable,
    }

    public struct DataSystemConfig
    {
        public string DB;
        public string FTP_Username;
        public string FTP_Password;
        public string FTP_IP;
        public string UseDataSystem;
        public string Station;
        public string SFCSIP;
        public string ATSLogMode;
    }

    public partial class frmMain //: StatusUI2.FrmATS
    {
        /*********************************************
        *         Version Build
        ********************************************/

        private const string AtsStation = "NONE";
        private const string AtsVersion = "V1.0.0";
        private Action<string, UInt32> mLogCallback = null;

        /*********************************************
        *         Global Variable
        ********************************************/

        private System.Threading.Thread test;
        private System.Threading.Thread fixThread;

        private string warning = string.Empty;
        private string station = string.Empty;
        private const string station0 = "OTA";
        private const string station1 = "PCBA";
        private const string station2 = "RF_Test";
        private const string station3 = "Final";
        private const string station4 = "FWSwitch";
        private const string station5 = "FQC_OTA";
        private const string station6 = "FQC_Final";
        private const string station7 = "CustomerToMFG";
        private const string station8 = "OnlineBox";
        private const string station9 = "CheckPing";
        private string bu = "NH";
        private string product = "Product";
        private string project = "LCS5";
        private const string NG = "NG";
        private const string PASS = "PASS";
        private string sCtrlC = Convert.ToString((char)0x03);
        private int isLoop = 0;
        private bool forHQtest = false; //Rena_20221014 add for HQ test

        private DataSystemConfig dataSystemConfig;
        private static object lockDS = new object();
        private bool isFirstTest = true;
        private bool inSFCS = false;
        private ATS_Result atsResult;
        private int ngCount = 0;
        private int needRetestCount = 0;
        //EasyLibrary.CommandConsole myCC = new EasyLibrary.CommandConsole();
        private ATS_Template.SFCS_ATS_2_0.ATS sfcs = new ATS_Template.SFCS_ATS_2_0.ATS();
        //CtrlIOBOARD IO_Board_Control1;
        //CtrlIOBOARD IO_Board_Control2;
        private PS_3615 _ps3615;
        private PS_3323 _ps3323;
        public bool usePS_3615 = false;
        public bool usePS_3323 = false;
        public bool isGolden = false;
        public bool usecamera = false;
        public string buffer = "";
        public string RFPI = "";
        string sExeDirectory = "";
        /*********************************************
        *         Initial Function
        ********************************************/
        ATS_Template.YNConfirm frmYN = new ATS_Template.YNConfirm(ATS_Template.YNConfirm.Type.YESNO);
        ATS_Template.YNConfirm frmOK = new ATS_Template.YNConfirm(ATS_Template.YNConfirm.Type.OK);
        ATS_Template.YNConfirm frmMessage = new ATS_Template.YNConfirm(ATS_Template.YNConfirm.Type.NOBUTTON);
        public frmMain()
        {
            InitializeComponent();
        }

        public void Save_LOG_data(string sTtestResult, bool isTitle = false, bool isCustom = false, bool isError = false)
        {
            uint type = isTitle ? MT8872AFIXTURE.Util.MSG.TITLE : MT8872AFIXTURE.Util.MSG.NORMAL;
            if (type == MT8872AFIXTURE.Util.MSG.TITLE)
            {
                sTtestResult = "*** " + sTtestResult + " ***";
            }
            type = isCustom ? MT8872AFIXTURE.Util.MSG.CUSTOM : type;
            type = isError ? MT8872AFIXTURE.Util.MSG.ERROR : type;

            this.mLogCallback(sTtestResult, type);
        }
        private void frmMain_Load(object sender, EventArgs e)
        {

            //Telnet.Message += new EventHandle.EventLogHandler(Message);
            //PS_3615.Message += new EventHandle.EventLogHandler(Message);
            //Fixture.Message += new EventHandle.EventLogHandler(Message);
            //Fixture.Click += new System.EventHandler(status_ATS.buStart_Click);
            //RS232.Message += new EventHandle.EventLogHandler(Message);
            //LitePoint.Message += new EventHandle.EventLogHandler(Message);
            //WiFiCollection.Message += new EventHandle.EventLogHandler(Message);

            Initial_StatusUI();
            Initial_TestParameter();
            Initial_EQP();

            #region Run IQ_GetSN
            try
            {
                if (!File.Exists(".\\Setting.ini"))
                {
                    DisplayMsg(LogType.Log, "Setting file not exist!");
                    return;
                }
                if (Func.ReadINI("Setting", "IQ_GetSN", "Use", "0") == "1")
                {
                    if (!Directory.Exists(".\\IQ_GetSN"))
                    {
                        Directory.CreateDirectory(".\\IQ_GetSN");
                    }
                    File.Copy(".\\Setting.ini", ".\\IQ_GetSN\\Setting.ini", true);
                    DisplayMsg(LogType.Log, "Copy Setting.ini to IQ_GetSN");
                    Thread.Sleep(500);
                    Process.Start(".\\IQ_GetSN\\IQ_GetSN.exe");
                    DisplayMsg(LogType.Log, "Start IQ_GetSN.exe");
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, ex.ToString());
                return;
            }

            #endregion


            if (useShield && Func.ReadINI("Setting", "Setting", "AutoStart", "1") == "1")
            {
                SetButton(status_ATS.buStart, "A U T O", false);
                fixThread = new Thread(new ThreadStart(fixture.ListenFixtureStatus));
                fixThread.Start();
            }
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {

        }

        private void status_ATS_Click(object sender, EventArgs e)
        {
            test = new Thread(new ThreadStart(Test));
            test.Start();
        }

        private void status_ATS_Load(object sender, EventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillTaskProcess("camera");
            KillTaskProcess("BarCodeCam");
        }

        private void status_ATS_Start_Before()
        {
            if (String.Compare(Func.ReadINI("Setting", "BarCodeScanner", "BarCodeScanner", "Disable").ToUpper(), CHK.Enable.ToString().ToUpper(), true) == 0)
            {
                SetTextBox(status_ATS.txtPSN, "NONE");
            }
            if (String.Compare(Func.ReadINI("Setting", "BarCodeScanner2", "BarCodeScanner2", "Disable").ToUpper(), CHK.Enable.ToString().ToUpper(), true) == 0)
            {
                SetTextBox(status_ATS.txtSP, "NONE");
            }
            if (station == "RF_Test" || station == "OTA")
            {
                SetTextBox(status_ATS.txtPSN, "NONE");
                SetTextBox(status_ATS.txtSP, "NONE");
            }
            if (station == "Final")
            {
                SetTextBox(status_ATS.txtPSN, "NONE");
            }
        }

        private void status_ATS_End_Before()
        {

        }

        private void AddDefaultData()
        {
            //status_ATS.AddDataDefault("Item1","Unit");
            //status_ATS.AddDataDefault("Item2","Unit");
        }

        private void Initial_StatusUI()
        {
            #region Setting Test Stataion
            //Directory.SetCurrentDirectory(Application.StartupPath);//jed for work around
            DisplayMsg(LogType.Log, "Initial StatusUI");
            this.Save_LOG_data("Initial StatusUI");
            station = Func.ReadINI("Setting", "Setting", "Station", "NONE");
            #endregion

            #region Initial StatusUI
            string date = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            string versionRelease = "v" + myFileVersionInfo.ProductMajorPart + "." + myFileVersionInfo.ProductMinorPart + "." + myFileVersionInfo.ProductBuildPart;
            #endregion

            DateTime dt = ATS.BuildDateTime.GetBuildDateTime(System.Reflection.Assembly.GetExecutingAssembly());

            status_ATS.Setting.Encryption = false;      // None_encryption (Read SPEC.ini file not SPEC.dat file)
            status_ATS.SetVersion(station, bu, product, project, AtsVersion + " - " + dt.ToString("yyyy/MM/dd HH:mm:ss"), dt.ToString("yyyy/MM/dd"));
            //status_ATS.SetVersion(AtsStation, bu, product, project, AtsVersion + " - " + dt.ToString("yyyy/MM/dd HH:mm:ss"), dt.ToString("yyyy/MM/dd"));
            //status_ATS.SetVersion(station, bu, product, project, versionRelease, date);
            status_ATS.SetUIMode(StatusUI2.StatusUI.UIMode.Manufacture_Mode);
            status_ATS.SetSFCS_Data_Format(StatusUI2.StatusUI.SFCSDataFormat.Generally);
            //AddDefaultData();
            SetTextBox(status_ATS.txtFix, Func.ReadINI("Setting", "Setting", "FixtureNumber", "NONE"));
            status_ATS.txtPSN.Select();
        }

        private void Initial_TestParameter()
        {
            DataSystemParameter();

            if (station == "OTA" || station == "FQC_OTA")
            {
                WiFiParameter();
            }

            UartTestParameter();

            TelnetParameter();

            if (station == "RF_Test")
            {
                string path = @"C:\Program Files (x86)\Qualcomm\QPST\bin\";
                KillTaskProcess("QPSTConfig");
                KillTaskProcess("QPSTServer");
                if (!CheckToolExist(Path.GetDirectoryName(path) + "QPSTConfig.exe"))
                    if (!OpenTestTool(Path.GetDirectoryName(path), "QPSTConfig.exe", "", 3000))
                    {
                        status_ATS.Write_Warning("Open QPSTConfig.exe error", StatusUI2.StatusUI.StatusProc.Warning);
                    }
            }

            sExeDirectory = WNC.API.Func.ReadINI("Setting", "Camera", "ExeDir", "D:\\camera");
            if (Func.ReadINI("Setting", "Camera", "Camera", "0") == "1")
            {
                StartupCamera();
                usecamera = true;
            }
        }

        private void Initial_EQP()
        {
            if (Func.ReadINI("Setting", "IO_Board_Control", "IO_Control_1", "0") == "1")
            {
                #region IO_Board_Control
                try
                {
                    string rev_message = "";
                    string Comport1 = Func.ReadINI("Setting", "IO_Board_Control", "Comport1", "COM1");
                    string Baudrate = Func.ReadINI("Setting", "IO_Board_Control", "Baudrate", "9600");
                    IO_Board_Control1 = new CtrlIOBOARD(Comport1.Trim(), Baudrate.Trim(), ref rev_message);

                    DisplayMsg(LogType.Log, rev_message);
                    if (!IO_Board_Control1.bInit)
                    {
                        MessageBox.Show("IO_Board Com Port Fail");
                        Environment.Exit(0);
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "IO_Board Com Port OK");
                        for (int io1 = 0; io1 < 8; io1++)
                        {
                            //if (io1 != Golden_IOBoard_OTA && Station == "OTA")
                            //{
                            //    status_ATS.AddLog("8进8出IO_Board的Y" + io1 + " Off...");
                            //    IO_Board_Control1.ConTrolIOPort_write(io1, "2", ref rev_message);
                            //}
                            //else
                            //{
                            status_ATS.AddLog("IO_Board_Y" + io1 + " Off...");
                            IO_Board_Control1.ConTrolIOPort_write(io1, "2", ref rev_message);
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    status_ATS.AddLog("IO_Board_Control_1 has an error：" + ex.ToString());
                    MessageBox.Show(ex.Message);
                    return;
                }
                #endregion
            }

            if (Func.ReadINI("Setting", "IO_Board_Control2", "IO_Control_2", "0") == "1")
            {
                #region IO_Board_Control
                try
                {
                    string rev_message = "";
                    string Comport1 = Func.ReadINI("Setting", "IO_Board_Control2", "Comport1", "COM1");
                    string Baudrate = Func.ReadINI("Setting", "IO_Board_Control2", "Baudrate", "9600");
                    IO_Board_Control2 = new CtrlIOBOARD(Comport1.Trim(), Baudrate.Trim(), ref rev_message);

                    DisplayMsg(LogType.Log, rev_message);
                    if (!IO_Board_Control2.bInit)
                    {
                        MessageBox.Show("IO_Board Com Port Fail");
                        Environment.Exit(0);
                    }
                    else
                    {
                        DisplayMsg(LogType.Log, "IO_Board Com Port OK");
                        for (int io1 = 0; io1 < 8; io1++)
                        {
                            //if (io1 != Golden_IOBoard_OTA && Station == "OTA")
                            //{
                            //    status_ATS.AddLog("8进8出IO_Board的Y" + io1 + " Off...");
                            //    IO_Board_Control1.ConTrolIOPort_write(io1, "2", ref rev_message);
                            //}
                            //else
                            //{
                            status_ATS.AddLog("IO_Board_Y" + io1 + " Off...");
                            IO_Board_Control2.ConTrolIOPort_write(io1, "2", ref rev_message);
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    status_ATS.AddLog("IO_Board_Control_1 has an error：" + ex.ToString());
                    MessageBox.Show(ex.Message);
                    return;
                }
                #endregion
            }

            if (Func.ReadINI("Setting", "BarCodeScanner", "BarCodeScanner", "Disable") == "Enable")
            {
                #region BarCodeScanner
                try
                {

                    string Comport1 = Func.ReadINI("Setting", "BarCodeScanner", "Comport1", "COM1");
                    string Baudrate = Func.ReadINI("Setting", "BarCodeScanner", "Baudrate", "9600");
                    barcodeUart = new SerialPort(Comport1.Trim(), Convert.ToInt32(Baudrate), Parity.None, 8, StopBits.One);

                    if (!barcodeUart.IsOpen)
                    {
                        barcodeUart.Open();
                    }
                    DisplayMsg(LogType.Log, $"Initial barcode uart {Comport1} ok");
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Log, ex.ToString());
                    MessageBox.Show("Initial barcode uart error");
                    Application.Exit();
                }
                #endregion

            }

            if (Func.ReadINI("Setting", "BarCodeScanner2", "BarCodeScanner2", "Disable") == "Enable")
            {
                #region BarCodeScanner
                try
                {
                    string rev_message = "";
                    string Comport1 = Func.ReadINI("Setting", "BarCodeScanner2", "Comport1", "COM1");
                    string Baudrate = Func.ReadINI("Setting", "BarCodeScanner2", "Baudrate", "9600");
                    barcodeUart2 = new SerialPort(Comport1.Trim(), Convert.ToInt32(Baudrate), Parity.None, 8, StopBits.One);


                    if (!barcodeUart2.IsOpen)
                    {
                        barcodeUart2.Open();
                    }
                    DisplayMsg(LogType.Log, $"Initial barcode uart {Comport1} ok");
                }
                catch (Exception ex)
                {
                    DisplayMsg(LogType.Log, ex.ToString());
                    MessageBox.Show("Initial barcode uart error");
                    Application.Exit();
                }
                #endregion

            }

            #region Power Supply
            if (WNC.API.Func.ReadINI("Setting", "PowerSupply", "PowerSupply", "0") == "1")
            {
                if (WNC.API.Func.ReadINI("Setting", "PowerSupply", "PS3323", "0") == "0")
                {
                    #region 3615
                    try
                    {
                        //string sPSCom = WNC.API.Func.ReadINI("Setting", "PowerSupply", "3323_COM", "COM1");
                        _ps3615 = new PS_3615("PowerSupply");
                        _ps3615.PowerSupplyPowerOff();
                        usePS_3615 = true;
                        status_ATS.AddLog("Initial Power supply OK");
                    }
                    catch (Exception ex)
                    {

                        DisplayMsg(LogType.Exception, "Inital PowerSupply Error： " + ex.Message);
                        MessageBox.Show("Inital PowerSupply Error：" + ex.ToString());
                        Environment.Exit(0);
                    }
                    #endregion
                }
                else
                {
                    #region 3323
                    DisplayMsg(LogType.Log, "====== Power Supply ======");
                    string psCom = Func.ReadINI("Setting", "PowerSupply", "RS232", "COM1");
                    int sChannel_3323 = Convert.ToInt16(Func.ReadINI("Setting", "PowerSupply", "CH1_Channel", "1"));
                    double dVolt_3323 = Convert.ToDouble(Func.ReadINI("Setting", "PowerSupply", "CH1_Volt", "0.0"));
                    double dCurr_3323 = Convert.ToDouble(Func.ReadINI("Setting", "PowerSupply", "CH1_Current", "0.0"));
                    _ps3323 = new PS_3323(psCom);
                    DisplayMsg(LogType.Log, "DUT CH : " + sChannel_3323.ToString());
                    DisplayMsg(LogType.Log, "DUT Current : " + dVolt_3323.ToString() + " (V)");
                    DisplayMsg(LogType.Log, "DUT Voltage : " + dCurr_3323.ToString() + " (A)");

                    _ps3323.OVPSET(sChannel_3323, 13);
                    _ps3323.SET_Volt(sChannel_3323, (float)dVolt_3323);
                    _ps3323.SET_Curr(sChannel_3323, (float)dCurr_3323);
                    _ps3323.Power_OFF();
                    status_ATS.AddLog("Initial Power supply RS232 OK");

                    #endregion
                    usePS_3323 = true;
                }
            }
            #endregion

            if (Func.ReadINI("Setting", "Camera", "Barcode", "0") == "1")
            {

                if (!CheckTaskProcess("BarCodeCam"))
                {
                    try
                    {
                        string sReceive = "";
                        Net.NetPort newNetPort = new Net.NetPort();
                        string NewCameraPath = Func.ReadINI("Setting", "Camera", "BarcodePath", "D:/Barcode");
                        newNetPort.ExecuteDOSCommand(NewCameraPath + @".\BarCodeCam.exe", "", false, ref sReceive, 10, true);
                        status_ATS.AddLog("Open camera path：" + NewCameraPath);
                    }
                    catch (Exception ex)
                    {
                        Directory.SetCurrentDirectory(Application.StartupPath);
                        status_ATS.AddLog("Camera Open Exception:" + ex.Message);
                    }
                }
            }

            if (!fixture.ChkState(true))
            {
                fixture.Open();
            }
        }

        private void DataSystemParameter()
        {
            DisplayMsg(LogType.Log, "====== Data System ======");

            DataSystem.Message += Message;
            dataSystemConfig.UseDataSystem = Func.ReadINI("Setting", "DataSystem", "UseDataSystem", "1");
            dataSystemConfig.DB = Func.ReadINI("Setting", "DataSystem", "DB", "ats");
            dataSystemConfig.FTP_Username = Func.ReadINI("Setting", "DataSystem", "FTPUsername", "V1-ATSlogFTP");
            dataSystemConfig.FTP_Password = Func.ReadINI("Setting", "DataSystem", "FTPPassword", "V1-ATSlogFTP");
            dataSystemConfig.FTP_IP = Func.ReadINI("Setting", "DataSystem", "FTPIP", "10.169.98.45");
            dataSystemConfig.Station = station;
            dataSystemConfig.SFCSIP = GetNetCardIP("SFCS");
            dataSystemConfig.ATSLogMode = Func.ReadINI("Setting", "Setting", "ATSLogMode", "");

            if (String.Compare(dataSystemConfig.UseDataSystem, "0", true) == 0)
            {
                DisplayMsg(LogType.Warning, "Disable DataSystem !!");
            }
            else
            {
                DisplayMsg(LogType.Log, "Enable DataSystem");
                DisplayMsg(LogType.Log, "DB : " + dataSystemConfig.DB);
                DisplayMsg(LogType.Log, "Station : " + dataSystemConfig.Station);
                DisplayMsg(LogType.Log, "SFCS : " + dataSystemConfig.SFCSIP);
            }
        }


        /*********************************************
        *         Test Function
        ********************************************/

        private void Test()
        {
            //isLoop = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "Setting", "Loop", "0"));
            //int retry = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "Setting", "RetryTimes", "0"));
            //int loopDelay = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "Setting", "LoopDelayMs", "5000"));
            //int loopCount = Convert.ToInt32(WNC.API.Func.ReadINI("Setting", "Setting", "LoopCount", "100"));

            int count = 0;
            int passCount = 0;
            int ngCount = 0;

            do
            {
            Re:
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Test_Initial();

                Test_Function();
                sw.Stop();

                count++;
                if (isLoop == 0 && !CheckGoNoGo())
                {
                    if (count < retry)
                    {
                        status_ATS.Write_Retest_Data();
                        status_ATS.ListDataSFCS.Clear();
                        RemoveFailedItem();
                        if (useShield)
                            fixture.Open();
                        goto Re;
                    }
                }

                if (isLoop != 0)
                {
                    if (CheckGoNoGo())
                        passCount++;
                    else
                        ngCount++;

                    DisplayMsg(LogType.StressMarked, "END MARKED");
                    DisplayMsg(LogType.StressMarked, "Test time:" + sw.ElapsedMilliseconds);
                    DisplayMsg(LogType.Log, "PASS : " + passCount.ToString());
                    DisplayMsg(LogType.Log, "NG : " + ngCount.ToString());

                    warning = string.Empty;
                    RemoveFailedItem();

                    if (count >= loopCount)
                        break;
                    if (station == "RF_Test")
                        fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.OFF);
                    if (useShield)
                        fixture.Open();

                    DisplayMsg(LogType.Log, "Delay " + loopDelay + " (ms)..");
                    Thread.Sleep(loopDelay);
                    if (useShield)
                        fixture.Close();
                }

            } while (isLoop != 0);

            //status_ATS.AddData("RetryTimes", "", 0, 2, count, "000000");
            //Test_Discharge();

        }

        private void Test_Initial()
        {
            warning = string.Empty;
            isGolden = false;
            if (useShield)
                fixture.Close();

            if (!ScanBarCode())
            {
                warning = "Scan Barcode error.";
                return;
            }
            if (!ScanBarCode2())
            {
                warning = "Scan Barcode error.";
                return;
            }
        }

        private void Test_Function()
        {
            #region Check Station
            if (station == "NONE")
            {
                warning = "Wrong station";
                return;
            }
            #endregion

            try
            {
                #region Main Flow
                DisplayMsg(LogType.Log, "========= " + station + " =========");
                switch (station)
                {
                    //case station0:
                    //    this.OTA();
                    //    break;
                    //case station1:
                    //    this.PCBA();
                    //    break;
                    //case station2:
                    //    this.RF_Test();
                    //    break;
                    //case station3:
                    //    this.Final();
                    //    break;
                    //case station4:
                    //    this.FWSwitch();
                    //    break;
                    //case station5:
                    //    this.FQC_OTA();
                    //    break;
                    //case station6:
                    //    this.FQC_Final();
                    //    break;
                    //default:
                    //    warning = "Wrong station";
                    //    return;
                }
                #endregion

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                warning = "Test_Function Exception";
            }
        }

        private void Test_Discharge()
        {
            if (useShield)
            {
                if (station == "RF_Test")
                    fixture.ControlIO(Fixture.FixtureIO.IO_6, CTRL.OFF);
                fixture.Open();
            }
            string temp = status_ATS.txtPSN.Text;

            CheckTestItemCount();

            if (status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
                inSFCS = false;
            else
                inSFCS = true;

            if (station == "OnlineBox" && !CheckGoNoGo())
            {
                status_ATS.Write_Warning("NG", StatusUI2.StatusUI.StatusProc.NG);
            }
            else
                CheckChangeFixture(temp);

            if (useShield && Func.ReadINI("Setting", "Setting", "AutoStart", "1") == "1")
            {
                SetButton(status_ATS.buStart, "A U T O", false);
                fixThread = new Thread(new ThreadStart(fixture.ListenFixtureStatus));
                fixThread.Start();
            }

            //UploadDataToDataSystem();
        }

        private void CheckChangeFixture(string psn)
        {
            if (warning == "" || warning == null)
            {
                if (isGolden || status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
                {
                    if (!CheckGoNoGo())
                    {
                        atsResult = ATS_Result.NG;
                    }
                    else
                    {
                        atsResult = ATS_Result.PASS;
                    }
                    status_ATS.WriteToSFCS();
                }
                else
                {
                    #region 得到產品SFCS內記錄
                    status_ATS.AddLog("Retest:" + psn + "," + dataSystemConfig.SFCSIP);
                    string sErr = "";
                    ATS_Template.SFCS_ATS_2_0.ATS a = new ATS_Template.SFCS_ATS_2_0.ATS();

                    //status_ATS.SFCS_Data.PSN
                    if (!a.ReTestControl_Query(psn, dataSystemConfig.SFCSIP, ref sErr))
                    {
                        atsResult = ATS_Result.WARNING;
                        status_ATS.Write_Warning(sErr, StatusUI2.StatusUI.StatusProc.Warning);
                        status_ATS.AddLog("sError=" + sErr);
                        //status_ATS.WriteToSFCS();
                        return;
                    }
                    status_ATS.AddLog("Retest control query error:" + sErr);
                    #endregion

                    if (sErr == "")
                    {
                        #region 記錄為空(0)
                        if (CheckGoNoGo())
                        {
                            atsResult = ATS_Result.PASS;
                            status_ATS.WriteToSFCS();
                        }
                        else
                        {
                            atsResult = ATS_Result.WARNING;
                            a.ReTestControl_Insert(psn, dataSystemConfig.SFCSIP, "0002", ref sErr);
                            status_ATS.Write_Retest_Data();
                            status_ATS.Write_Warning("RETEST", StatusUI2.StatusUI.StatusProc.Warning);
                        }
                        #endregion
                    }
                    else
                    {
                        if (CheckGoNoGo())
                        {
                            atsResult = ATS_Result.PASS;
                            a.ReTestControl_Delete(psn, ref sErr);
                            status_ATS.WriteToSFCS();
                        }
                        else
                        {
                            atsResult = ATS_Result.NG;
                            a.ReTestControl_Delete(psn, ref sErr);
                            status_ATS.WriteToSFCS();
                        }
                    }
                }
            }
            else
            {
                atsResult = ATS_Result.WARNING;
                status_ATS.Write_Warning(warning, StatusUI2.StatusUI.StatusProc.Warning);
            }
        }
        private void CheckTestItemCount()
        {
            int testCountATS = status_ATS.CheckListDataAll().Count;
            int testCountSetting = Convert.ToInt32(Func.ReadINI("Setting", "Setting", "ItemCount", "-1"));
            DisplayMsg(LogType.Log, "Test item count in Setting:" + testCountSetting);
            DisplayMsg(LogType.Log, "Test item count in ATS:" + testCountATS);

            if (status_ATS._testMode != StatusUI2.StatusUI.TestMode.EngMode && !isGolden && CheckGoNoGo())
            {
                if (testCountATS != testCountSetting)
                {
                    warning = "Test items count not same";
                    //int c = 0;
                    //foreach (var item in status_ATS.CheckListDataAll())
                    //{
                    //    c++;
                    //    DisplayMsg(LogType.Log, c + "." + item);
                    //}
                    return;
                }
            }

        }

        private bool CheckGoNoGo()
        {
            if (status_ATS.CheckListData().Count == 0 && warning == "")
            {
                return true;
            }
            else
            {
                status_ATS.AddLog("NG item count：" + status_ATS.CheckListData().Count + "，sWarning：" + warning);
                if (status_ATS.CheckListData().Count != 0)
                {
                    status_ATS.AddLog("NG item name：" + status_ATS.CheckListData()[0].ToString());
                }
                return false;
            }
        }

        /*********************************************
        *         SFCS Function
        ********************************************/

        public void DisplayMsg(LogType type, string message)
        {
            if (String.Compare(type.ToString(), LogType.Empty.ToString(), true) == 0)
            {
                status_ATS.AddLog(message);
            }
            else
            {
                status_ATS.AddLog("[ " + type.ToString() + " ]  " + message);
            }
        }

        private void Message(EventHandle.EventLogArgs e)
        {
            status_ATS.AddLog(e._S_LOG);
        }

        private void SetCombineLine(string number)
        {
            SetTextBox(status_ATS.txtPSN, number);
            status_ATS.SFCS_Data.PSN = number;
            status_ATS.SFCS_Data.First_Line = number;
        }

        private bool ChkStation(string number)
        {
            string myIp = LocalIpAddress();
            string[] ipArray = null;
            string error = string.Empty;

            if (Stri/*ng.Compare(Func.ReadINI("Setting", "Setting", "CHK_STAGE", "Enable"), CHK.Disable.ToString(), true) == 0 || isGolde*/n)
            {
                return true;
            }

            DisplayMsg(LogType.Log, "--- Check Station ---");

            ipArray = myIp.Split('@');

            for (int i = 0; i < ipArray.Length - 1; i++)
            {
                if (sfcs.check_SFCS_Stage_new(number, ipArray[i], ref error))
                {
                    return true;
                }
                status_ATS.AddLog("Error : " + error);
            }

            //status_ATS.AddLog(" Wrong Station !! : " + number);
            warning = "Wrong Station";
            return false;
        }

        private void ChkRetryStatus(string psn)
        {
            DisplayMsg(LogType.Log, "NG Item : " + FetchNgItemName());
            int reCount = 0;

            if (status_ATS.CheckListData().Count == 0 && warning == string.Empty)
            {
                atsResult = ATS_Result.PASS;
                ngCount = 0;
                return;
            }

            ngCount++;

            if (warning != string.Empty)
            {
                DisplayMsg(LogType.Log, "Warning: " + warning.ToString());
                atsResult = ATS_Result.WARNING;
                return;
            }

            atsResult = ATS_Result.NG;

            //ChkRetryCount(psn, ref reCount);

            //if (reCount < needRetestCount)
            //{
            //    atsResult = ATS_Result.RETEST;
            //    warning = "ReTest";
            //}
            //else
            //{
            //    atsResult = ATS_Result.NG;
            //}
        }

        private bool CheckSn(string sn)
        {
            if (status_ATS.CheckListData().Count != 0 || warning != string.Empty)
            {
                return false;
            }

            string errorItem = "CheckSn";
            try
            {
                int timeOutMs = 10 * 1000;
                int delayMs = 0;
                string res = string.Empty;
                string SN_FAC = string.Empty;

                DisplayMsg(LogType.Log, "=============== Write and Check Serial Number ===============");

                DisplayMsg(LogType.Log, "Input serial number:" + sn);

                SendAndChk(errorItem, PortType.DUT_UART, "fw_setenv serial_number " + sn, "#", delayMs, timeOutMs);
                SendAndChk(errorItem, PortType.DUT_UART, "sync", "#", delayMs, timeOutMs);
                SendCommand(PortType.DUT_UART, "fw_printenv serial_number", delayMs);
                ChkResponse(PortType.DUT_UART, ITEM.NONE, "#", out res, timeOutMs);
                if (!res.Contains("serial_number=" + sn))
                {
                    warning = "check sn fail!";
                    return false;
                }
                // fw_printenv SerialNumber    // SerialNumber=
                //SendAndChk(errorItem, PortType.LS04_UART, "fw_printenv SerialNumber", sn, 100, timeOutMs);    // fw_printenv SerialNumber    // SerialNumber=

                if (status_ATS.CheckListData().Count != 0 || warning != string.Empty)
                {
                    return false;
                }
                status_ATS.AddDataLog(errorItem, PASS);
                //status_ATS.AddDataRaw("SN", status.Sn, sn, "A00000");

                return true;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                status_ATS.AddDataLog(errorItem, NG);
                return false;
            }
        }

        private void ChkRetryCount(string sn, ref int retestCount)
        {
            try
            {
                retestCount = 0;

                if (String.Compare(Func.ReadINI("Setting", "ReTest", "CheckRetest", "Disable"), CHK.Enable.ToString(), true) != 0)
                {
                    DisplayMsg(LogType.Warning, "Not check retest");
                    retestCount = 3;
                    return;
                }

                if (isGolden)
                {
                    return;
                }


                DisplayMsg(LogType.Log, "====== Check Re-Test ======");

                string ip = GetNetCardIP("SFCS");
                string msg = string.Empty;
                string error = string.Empty;
                string url = string.Empty;
                string pn = GetPartNumber(sn);

                int retestNum = 0;
                int ngNum = 0;

                char[] splitChar = { ',', ':' };

                DisplayMsg(LogType.Log, "SN : " + sn);
                DisplayMsg(LogType.Log, "IP : " + ip);

                GetSfcs_2_0_Url(pn, out url);

                sfcs.Url = url;

                msg = sfcs.chkRetestNG(ip, sn, ref error);

                DisplayMsg(LogType.Log, msg);

                if (string.IsNullOrEmpty(error))
                {
                    retestNum = Convert.ToInt16(msg.Split(splitChar)[1]);
                    ngNum = Convert.ToInt16(msg.Split(splitChar)[5]);

                    retestCount = retestNum - ngNum;
                    DisplayMsg(LogType.Log, "Re-Test Count : " + retestCount.ToString());
                }
                else
                {
                    DisplayMsg(LogType.Log, "Check re-test number NG : " + error);
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                retestCount = 3;
            }
        }

        private void GetFromSfcs(string name, out string component)
        {
            try
            {
                DisplayMsg(LogType.Log, "Get " + name + " on SFCS");
                component = string.Empty;
                string psn = status_ATS.txtPSN.Text;
                DisplayMsg(LogType.Log, "PSN : " + psn);
                //??????
                //string SQLstr = "SELECT * FROM(SELECT H.*, G.PN FROM SPN_TABL G JOIN (SELECT * FROM (SELECT A.* FROM (SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START  WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                //    + psn + "'  or A.prod_ssn='" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN=PROD_SSN UNION SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                //    + psn + "' or a.prod_ssn = '" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN=CSN) A UNION SELECT 99 AS LV, B.SSN, '' AS CSN, '' AS CPN, A.CSN AS PROD_SSN FROM SN_ASSE B JOIN (SELECT * FROM (SELECT A.* FROM (SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                //    + psn + "' or A.prod_ssn='" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN=PROD_SSN UNION SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                //    + psn + "' or A.prod_ssn='" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN=CSN) A) WHERE CPN LIKE 'N%') A ON  A.CSN = B.PROD_SSN) ORDER BY LV) H ON G.SPN = SUBSTR(H.SSN,1,4)) WHERE CSN NOT LIKE '%=RWK%' OR CSN IS NULL ";

                string FUNCstr = "LC";
                string SQLstr = string.Empty;
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                DataColumn col1 = new DataColumn("SQL");
                col1.DataType = System.Type.GetType("System.String");
                col1.AllowDBNull = false;
                col1.Caption = "SQL";
                col1.DefaultValue = string.Empty;
                System.Data.DataColumn col2 = new DataColumn("FUNC");
                col2.DataType = System.Type.GetType("System.String");
                col2.AllowDBNull = false;
                col2.Caption = "FUNC";
                col2.DefaultValue = string.Empty;
                System.Data.DataRow row = dt.NewRow();
                dt.Columns.Add(col1);
                dt.Columns.Add(col2);

                row["SQL"] = SQLstr;

                row["FUNC"] = FUNCstr;
                dt.Rows.Add(row);
                ds.Tables.Add(dt);

                string error = string.Empty;
                DataSet ds1 = sfcs.ASSP_V001(ds, ref error);
                string csn = string.Empty;
                string cpn = string.Empty;
                if (ds1.Tables[0].Rows.Count != 0)
                {
                    for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
                    {
                        csn = ds1.Tables[0].Rows[i].ItemArray[ds1.Tables[0].Columns.IndexOf("CSN")].ToString();
                        cpn = ds1.Tables[0].Rows[i].ItemArray[ds1.Tables[0].Columns.IndexOf("CPN")].ToString();
                        if (string.Compare(name, cpn, true) == 0)
                        {
                            component = csn;
                            break;
                        }
                    }
                }

                if (component == string.Empty)
                {
                    DisplayMsg(LogType.Log, "Dut not have " + name + " on SFCS");
                }

            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                component = string.Empty;
            }
        }

        private void GetSfcs_2_0_Url(string pn, out string url)
        {
            url = string.Empty;

            try
            {
                string errMsg = string.Empty;
                url = sfcs.GetWebServiceURL(Func.ReadINI("Setting", "Setting", "Webservices", @"http://v2haproxy01.wneweb.com.tw:100/Sfcs_Webservice/ats.asmx"), pn, ref errMsg);
                DisplayMsg(LogType.Log, "URL : " + url);
                DisplayMsg(LogType.Log, "Error : " + errMsg);
                var op = sfcs.getOP_ID(string.Empty, ref errMsg);
                DisplayMsg(LogType.Log, "Error : " + errMsg);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }

        private void RemoveFailedItem()
        {
            foreach (string s in status_ATS.CheckListData())
            {
                DisplayMsg(LogType.Log, "================>RemoveFailedItem : " + s);
                status_ATS.RemoveTestItem(s);
            }
        }

        private void RemoveFailedItem(string item)
        {
            ArrayList failedItem = status_ATS.CheckListData();

            for (int i = 0; i < failedItem.Count; i++)
            {
                StatusUI2.Data data = (StatusUI2.Data)failedItem[i];
                if (String.Compare(item, data.TestItem, false) == 0)
                {
                    DisplayMsg(LogType.Warning, "Remove failed item : " + data.TestItem);
                    status_ATS.RemoveTestItem(data.TestItem);
                    return;
                }
            }
        }

        private string LocalIpAddress()
        {
            IPHostEntry host;
            string localIP = string.Empty;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    DisplayMsg(LogType.Log, "IP : " + ip.ToString());
                    localIP += ip.ToString() + "@";
                }
            }
            return localIP;
        }

        private void KillTaskProcess(string taskName)
        {
            try
            {
                Process[] localAll = Process.GetProcesses();

                foreach (Process i in localAll)
                {
                    Regex r;
                    Match m;
                    string strEscape;

                    strEscape = Regex.Escape(taskName);
                    r = new Regex(@strEscape, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    m = r.Match(i.ProcessName.ToString());

                    if (m.Success)
                    {
                        DisplayMsg(LogType.Log, "Kill task : " + taskName);
                        Process[] killProcess = Process.GetProcessesByName(i.ProcessName.ToString());
                        killProcess[0].Kill();
                        m.NextMatch();
                        DisplayMsg(LogType.Log, "Kill task complete");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
                //MessageBox.Show(ex.Message);
            }
        }

        delegate void _SetTextBox(TextBox tb, string text);
        private void SetTextBox(TextBox tb, string text)
        {
            try
            {
                if (tb.InvokeRequired)
                {
                    _SetTextBox d = new _SetTextBox(SetTextBox);
                    tb.Invoke(d, new object[] { tb, text });
                }
                else
                {
                    tb.Text = text;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }

        delegate void _SetButton(Button button, string text, bool ctrl);
        private void SetButton(Button button, string text, bool ctrl)
        {
            try
            {
                if (button.InvokeRequired)
                {
                    _SetButton d = new _SetButton(SetButton);
                    button.Invoke(d, new object[] { button, text, ctrl });
                }
                else
                {
                    button.Text = text;
                    button.Enabled = ctrl;
                }
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Exception, ex.Message);
            }
        }

        /*********************************************
        *         Data System
        ********************************************/

        public string GetNgItem(ArrayList data)
        {
            if (data.Count <= 0)
            {
                return string.Empty;
            }

            if (data[0].GetType() == typeof(StatusUI2.Data))
            {
                return ((StatusUI2.Data)(data[0])).TestItem;
            }
            else if (data[0].GetType() == typeof(string))
            {
                return data[0].ToString();
            }
            else
            {
                return "TYPE_ERROR";
            }
        }

        public string GetAtsLogPath()
        {
            bool logByResult = (dataSystemConfig.ATSLogMode.ToUpper() == "BYRESULT");
            string resPath = "";
            string logPath = status_ATS.Log_S.File_Name.LogPath;//.LogPath;
            string logFileName = status_ATS.Log_S.File_Name.Log;//.LogFile;
            string logFileName_ByResult = status_ATS.Log_S.File_Name.ResultPath;

            if (logByResult && (atsResult == ATS_Result.PASS || atsResult == ATS_Result.NG))
            {
                string resultFolder = (status_ATS.CheckListData().Count != 0) ? "NG" : "Pass";
                //resPath = @System.IO.Path.Combine(logPath, resultFolder, logFileName).Replace("\\\\", "\\");
                //resPath = @System.IO.Path.Combine(logPath, resultFolder + "\\" + logFileName).Replace("\\\\", "\\");
                resPath = logFileName_ByResult.Replace("\\\\", "\\");
            }
            else
            {
                //resPath = @System.IO.Path.Combine(logPath, logFileName).Replace("\\\\", "\\");
                //resPath = @System.IO.Path.Combine(logPath, logFileName).Replace("\\\\", "\\");
                resPath = logFileName.Replace("\\\\", "\\");
            }

            return resPath;
        }

        public void UploadDataToDataSystem()
        {
            if (dataSystemConfig.UseDataSystem != "1" || status_ATS._testMode == StatusUI2.StatusUI.TestMode.EngMode)
            {
                return;
            }

            DisplayMsg(LogType.Log, "Upload Data to DataSystem");
            DataSystem dataSystem = new DataSystem();

            string error = string.Empty;
            string sn = status_ATS.SFCS_Data.PSN;
            string partNumber = string.Empty;
            string woNumber = string.Empty;

            woNumber = GetManufacturingNumber(sn);
            partNumber = GetPartNumber(sn);
            //_PartNumber = Func.ReadINI("Setting", "DataSystem", "PN", "");

            try
            {
                lock (lockDS)
                {
                    /*
                 *  驗證中的ATS，如果沒有正確的SN或是此SN取不到料號跟MO，建議可改成下列:
                 *  string PN = "PN_自己的專案名稱_Verify";
                 *  string PN = "MO_自己的專案名稱_Verify";
                 *  以上PN跟MO不可以加特殊符號(-、"'*+=%$@!都不可以用)，只能加下底線
                */

                    dataSystem.Connection(Func.ReadINI("Setting", "DataSystem", "ServerIP", "10.169.99.47"),
                                          Func.ReadINI("Setting", "DataSystem", "ServerAccount", "root"),
                                          Func.ReadINI("Setting", "DataSystem", "ServerPassword", ""), dataSystemConfig.DB);

                    string FTPPath = dataSystem.UploadLogToFTP(new UploadFTPData()
                    {
                        FTP_IP = dataSystemConfig.FTP_IP,
                        FTP_Username = dataSystemConfig.FTP_Username,
                        FTP_Password = dataSystemConfig.FTP_Password,
                        ATSLogPath = GetAtsLogPath(),
                        PN = partNumber,
                        MO = woNumber,
                        Station = dataSystemConfig.Station,
                        TestPass = (status_ATS.CheckListData().Count == 0)
                    });

                    dataSystem.WriteSQLData(new WriteSQLData()
                    {
                        FirstTest = isFirstTest,
                        InSFCS = true,
                        GoldenSample = isGolden,
                        PN = partNumber,
                        SFCSIP = dataSystemConfig.SFCSIP,
                        Fixture = status_ATS.SFCS_Data.Fixture,
                        SN = sn,
                        Result = atsResult.ToString(),
                        FTPLog = FTPPath,
                        NGItem = GetNgItem(status_ATS.CheckListData()),
                        ErrorCode = ((atsResult == ATS_Result.WARNING || atsResult == ATS_Result.PASS) ? string.Empty : status_ATS.SFCS_Data.ErrorCode),
                        Station = dataSystemConfig.Station,
                        Warning = warning,
                        TestTime = Convert.ToDouble(status_ATS.SFCS_Data.TestTime),
                        ListDataSFCS = status_ATS.ListDataSFCS
                    });
                }

            }
            catch (Exception ex)
            {
                status_ATS.AddLog("[frmMain][UploadDataToDataSystem][Exception] --> " + ex.Message);
                dataSystem.Disconnection();
            }
        }

        public string GetNetCardIP(string interfaceName)
        {
            string ip = string.Empty;
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.Name.ToLower().IndexOf(interfaceName.ToLower()) >= 0)
                {
                    IPInterfaceProperties ipProperties = adapter.GetIPProperties();
                    if (ipProperties.UnicastAddresses.Count > 0)
                    {
                        PhysicalAddress mac = adapter.GetPhysicalAddress();
                        string name = adapter.Name;
                        string description = adapter.Description;
                        ip = ipProperties.UnicastAddresses[0].Address.ToString();
                        if (ip.Contains(":") && ipProperties.UnicastAddresses.Count > 1)
                        {
                            ip = ipProperties.UnicastAddresses[1].Address.ToString();
                        }
                        string netmask = ipProperties.UnicastAddresses[0].IPv4Mask.ToString();
                    }
                }
            }
            return ip;
        }

        private string FetchNgItemName()
        {
            try
            {
                ArrayList defaultData = status_ATS.CheckDataDefualtIsComplete();
                ArrayList listData = status_ATS.CheckListData();
                string returnString = string.Empty;
                // Status UI 會先顯示default data缺少的測項,default data的優先權大於list data
                if (defaultData.Count > 0)
                {
                    StatusUI2.Data data = (StatusUI2.Data)defaultData[0];//只取第一個ng 的測項
                    returnString = " , NG item : " + data.TestItem;
                }
                else if (listData.Count > 0)
                {
                    returnString = " , NG item : " + listData[0].ToString();
                }
                return returnString;
            }
            catch (Exception ex)
            {
                status_ATS.AddLog("Exception :" + ex.ToString());
                return "Exception";
            }
        }

        private string GetManufacturingNumber(string sn)
        {
            string number = string.Empty;
            string error = string.Empty;

            number = sfcs.GetWO_BySN(sn, ref error);

            if (!string.IsNullOrEmpty(error))
            {
                number = "getWoNG";
                DisplayMsg(LogType.Error, String.Format("Get WO number NG : {0}", error));
            }

            DisplayMsg(LogType.Log, String.Format("WO number = {0}", number));
            return number;
        }

        private string GetPartNumber(string sn)
        {
            string partNumber = string.Empty;
            string error = string.Empty;
            try
            {
                partNumber = sfcs.GetPN_BySN(sn, ref error);

                if (!string.IsNullOrEmpty(error))
                {
                    partNumber = "getPnNG";
                    DisplayMsg(LogType.Error, String.Format("Get PartNumber NG : {0}", error));
                }

                DisplayMsg(LogType.Log, String.Format("PartNumber = {0}", partNumber));
                return partNumber;
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, String.Format("Exception : {0}", ex.ToString()));
                return "getPnNG";
            }
        }
        private void SentPsnForGetMAC(string SMT)
        {
            int n = 0;
        Retry:
            try
            {
                string sFileName = "PSN_" + SMT + ".txt";
                if (!Directory.Exists(@"D:\SFCS\"))
                {
                    Directory.CreateDirectory(@"D:\SFCS\");
                }

                FileStream fileStream = File.Open(@"D:\SFCS\" + sFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                StreamWriter writer = new StreamWriter(fileStream);
                //writer.WriteLine();

                string content = Environment.NewLine + status_ATS.txtPSN.Text + "," + status_ATS.txtSP.Text + Environment.NewLine;
                writer.Write(content);
                writer.Close();
                fileStream.Close();

                File.Copy(@"D:\SFCS\" + sFileName, @"C:\SFCS\" + sFileName, true);
                File.Delete(@"D:\SFCS\" + sFileName);

                status_ATS.AddLog("[LCS]Sent PSN File to SFCS time: " + DateTime.Now);
            }
            catch (Exception E)
            {
                if (n < 3)
                {
                    status_ATS.AddLog("Remove file Fail, Retry .. ");
                    n++;
                    goto Retry;
                }
                else
                {
                    warning = E.ToString() + " Remove file Fail...";
                }
            }
        }
        private void AddData(string AddDataTestItem, double AddDataVal)
        {
            if (AddDataTestItem == "")
            {
                return;
            }
            //string AddDataErrorCode = WNC.API.Func.ReadINI(Application.StartupPath, "SPEC", "Error_Code", AddDataTestItem, "XXXXXX");
            //double AddDataLSL = Convert.ToDouble(WNC.API.Func.ReadINI(Application.StartupPath, "SPEC", "SPEC", AddDataTestItem + "_LSL", "0"));
            //double AddDataUSL = Convert.ToDouble(WNC.API.Func.ReadINI(Application.StartupPath, "SPEC", "SPEC", AddDataTestItem + "_USL", "0"));
            //status_ATS.AddData(AddDataTestItem, "", AddDataLSL, AddDataUSL, AddDataVal, AddDataErrorCode);
        }
        private void SetCurrentFolder(string Folder)
        {
            if (!string.IsNullOrEmpty(Folder))
            {
                Directory.SetCurrentDirectory(Folder);
                DisplayMsg(LogType.Log, "Set Current Directory : " + Folder + " .");
            }
        }
        private bool CheckToolExist(string path)
        {
            Process[] prcAppNameAll = Process.GetProcesses();
            foreach (Process appName in prcAppNameAll)
            {
                string execName = Path.GetFileNameWithoutExtension(path);

                if (execName == appName.ProcessName)
                {
                    return true;
                }
            }
            return false;
        }
        private bool OpenTestTool(string dir, string exeName, string arg, int timeOutMs)
        {
            string _ATS_Folder = System.IO.Directory.GetCurrentDirectory();
            SetCurrentFolder(dir);
            try
            {
                Process testTool = new Process();
                testTool.StartInfo.WorkingDirectory = dir;
                testTool.StartInfo.FileName = exeName;
                testTool.StartInfo.Arguments = arg;

                DisplayMsg(LogType.Log, "Execute : " + dir + "\\" + exeName + " " + arg);
                DisplayMsg(LogType.Log, "TimeOut (ms) : " + timeOutMs.ToString());
                testTool.Start();
                testTool.WaitForExit(timeOutMs);
            }
            catch (Exception ex)
            {
                DisplayMsg(LogType.Log, "Open Test Tool erro: " + ex.ToString());
                return false;
            }
            SetCurrentFolder(_ATS_Folder);
            return true;
        }
    }

}
