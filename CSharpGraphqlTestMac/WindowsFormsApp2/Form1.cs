using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private Mutex mMutex = new Mutex();
        private bool mIsFinish = false;

        public class NetworkTestItem
        {
            public NetworkTestItem()
            {
                IsNoMac = true;
                IsFinish = false;
                IsPassByHaveMac = false;
                Name = "";
                Mac = "";
                License = "";
                TestMode = "";
                FlashUUID = "";
                IsChangeMacLogName = false;
                NewLogBTMacName = "";
                TwoD = "";
                TwoD_Model = "";
                // QCCBTMac = "";
            }

            public string License { get; set; }

            public bool IsNoMac { get; set; }

            public string TwoD { get; set; }

            public string TwoD_Model { get; set; }

            public string Name { get; set; }

            public string Mac { get; set; }

            public string TestMode { get; set; }

            public bool IsChangeMacLogName { get; set; }

            public string NewLogBTMacName { get; set; }

            public string FlashUUID { get; set; }

            // public string QCCBTMac { get; set; }

            public bool IsFinish { get; set; }

            public bool IsPassByHaveMac { get; set; }
        }

        public class SyncData
        {
            public bool IsUploadFile { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string MacAddress { get; set; }
            public string ErrorCode { get; set; }
            public string Result { get; set; }
            public string ResultSummary { get; set; }
            public string LotNo { get; set; }
            public string ProductFamily { get; set; }
            public string ProductDevice { get; set; }
            public string MacType { get; set; }
            public string SipSerialName { get; set; }
        }

        private List<string> mUrls = new List<string>();
        private string mDNSPath = "";
        private string mCurrentURL = "";
        private string mGqlURL = @"http://192.168.1.230/simdc_project/graphql";

        public Form1()
        {
            InitializeComponent();
        }

        public class Global
        {
            public static int iVars = 0;
        }

        public void AddServerURL(string url)
        {
            this.mUrls.Add(url);
        }

        public void SetDNSPath(string dns)
        {
            this.mDNSPath = dns;
        }

        public void RefreshGraphql_URL()
        {
            if (this.mUrls.Count == 0)
            {
                throw new Exception("Not Set Any Server URL Error !!");
            }

            if (this.mCurrentURL.Length == 0)
            {
                this.mCurrentURL = this.mUrls[0].Trim();
            }
            else
            {
                int urlCount = this.mUrls.Count;
                int currentIndex = 0;
                for (int i = 0; i < urlCount; i++)
                {
                    if (this.mUrls[i].Trim().Equals(this.mCurrentURL))
                    {
                        currentIndex = i;
                        break;
                    }
                }

                if ((currentIndex + 1) >= urlCount)
                {
                    currentIndex = 0;
                }
                else
                {
                    currentIndex++;
                }

                this.mCurrentURL = this.mUrls[currentIndex].Trim();
            }

            if (this.mDNSPath.Length == 0)
            {
                this.mGqlURL = "http://" + this.mCurrentURL + "/graphql";
            }
            else
            {
                this.mGqlURL = "http://" + this.mCurrentURL + "/" + this.mDNSPath + "/graphql";
            }
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            this.SetDNSPath("simdc_project");
            this.AddServerURL("192.168.1.230");
            this.AddServerURL("192.168.1.231");
            this.RefreshGraphql_URL();
            await Try_Real();
        }

        #region TRY Fake Mac Dispatch

        private const int mFakeTotalCount = 1000;
        private const int mFakeTryPeople = 12;
        private const string mFakeLotCode = "112290";

        private async Task Try_Fake()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            mIsFinish = false;

            await this.Fake_CreateTestConfigurtaion(mFakeLotCode, mFakeTotalCount);
            Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "___" + "Create lot : " + mFakeLotCode + " OK!!!");

            List<Task> lists = new List<Task>();
            for (int i = 0; i < mFakeTryPeople; i++)
            {
                Task Task1 = this.Fake_StartToTest(mFakeLotCode);
                lists.Add(Task1);
            }

            await Task.WhenAll(lists.ToArray());

            sw.Stop();
            Console.WriteLine($"Total Spend {sw.ElapsedMilliseconds}ms");
        }

        private void Fake_setFinish(int remainCount)
        {
            this.mMutex.WaitOne();
            this.mIsFinish = remainCount <= 0;
            this.mMutex.ReleaseMutex();
        }

        private bool Fake_isFinish()
        {
            bool isFinish = false;
            this.mMutex.WaitOne();
            isFinish = this.mIsFinish;
            this.mMutex.ReleaseMutex();
            return isFinish;
        }

        private async Task Fake_StartToTest(string lotcode)
        {
            string serialNum = "a123";
            do
            {
                try
                {
                    Stopwatch ssw = new Stopwatch();

                    NetworkTestItem network = new NetworkTestItem();
                    network.TwoD = "1125245645";
                    network.TwoD_Model = "GGGYYY";
                    GraphQL.GraphQLResponse<DispatchMacResponse> r1 = await this.Fake_DispatchMac(lotcode, serialNum, network);
                    this.Fake_setFinish(r1.Data.DispatchMac.IsFinish ? 0 : 1000);

                    if (!this.Fake_isFinish() && r1.Data.DispatchMac.Mac.Trim().Length > 0)
                    {
                        Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "___" +
                           "Dispatch mac --> " + r1.Data.DispatchMac.Mac);
                        SyncData syncData = new SyncData();
                        syncData.LotNo = lotcode;
                        syncData.MacAddress = r1.Data.DispatchMac.Mac;
                        GraphQL.GraphQLResponse<UploadResponse> r2 = await this.Fake_SyncMACResultWithoutLog(serialNum, syncData);
                        this.Fake_setFinish(r2.Data.SyncMACResultWithoutLog.Unused);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fail --> " + ex.Message);
                }
            } while (!this.Fake_isFinish());

            Console.WriteLine("Dispatch Finish !!");
        }

        public async Task<GraphQL.GraphQLResponse<DispatchMacResponse>> Fake_DispatchMac(string lotCode, string serialNum, NetworkTestItem item)
        {
            string gqlstr = @"
			        mutation($input: MacDispatchInput!){
                        DispatchMac(input: $input){
                            Result
                            IsNoMac
                            Mac
                            Name
                            IsFinish
                        }
                    }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var token = new Newtonsoft.Json.Linq.JObject();
            token.Add("productFamily", "QCC3046");
            token.Add("productDevice", "SCL004141");
            token.Add("lotCode", lotCode);
            token.Add("dutPCName", "123456");
            token.Add("opId", "1111");
            token.Add("groupPC", "123456");
            token.Add("testFlow", "FT1");
            token.Add("testerEmplayeeId", "01978");
            token.Add("barcode", item.TwoD);
            token.Add("barcode_vendor", item.TwoD_Model);
            token.Add("sipserialname", serialNum);
            jObject.Add("input", token);

            return await this.GqlHttpRequest<DispatchMacResponse>(gqlstr, jObject);
        }

        public async Task<GraphQL.GraphQLResponse<UploadResponse>> Fake_SyncMACResultWithoutLog(string serialNum, SyncData data)
        {
            //Total Fail Pass Unused Using TrayName TrayX TrayY LastMac LastMacName
            string gqlstr = @"
			        mutation($input: SyncIput!){
                          SyncMACResultWithoutLog(input: $input){
                            Total
                            Fail
                            Pass
                            Unused
                            Using
                            TrayName
                            TrayX
                            TrayY
                            LastMac
                            LastMacName
                        }
                    }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var inputVar = new Newtonsoft.Json.Linq.JObject();
            inputVar.Add("uploadLog", 1);
            inputVar.Add("resultSummary", "{\"Result\":\"OK\"}");
            inputVar.Add("address", data.MacAddress);
            inputVar.Add("result", "Pass");
            inputVar.Add("errorCode", "0001");
            inputVar.Add("lotcode", data.LotNo);
            inputVar.Add("productFamily", "QCC3046");
            inputVar.Add("productDevice", "SCL004141");
            inputVar.Add("macType", "BT");
            inputVar.Add("sipserialname", serialNum);
            jObject.Add("input", inputVar);
            return await this.GqlHttpRequest<UploadResponse>(gqlstr, jObject);
        }

        public async Task<GraphQL.GraphQLResponse<CreateTestConfigurationResponse>> Fake_CreateTestConfigurtaion(String lotCode, int count)
        {
            string gqlstr = @"
			       mutation($input: TestConfigurationCreationInput!){
                    CreateTestConfiguration(input: $input){
                        Result
                        CurrentStatus
                        SwName
                        SwVersion
                        FwName
                        FwVersion
                    }
                  }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var token = new Newtonsoft.Json.Linq.JObject();
            var duts = new Newtonsoft.Json.Linq.JArray();
            token.Add("lotcode", lotCode);
            token.Add("testPGMName", "Test");
            token.Add("testLoadBoard", "Test");
            token.Add("testMode", "FT1");

            token.Add("testDutMode", "Production");
            token.Add("testFlow", "FT1");
            token.Add("pid", "0");
            //  token.Add("testFlow", OperatorSingleton.Instance.Flow);
            token.Add("productFamilyName", "QCC3046");
            token.Add("productDeviceName", "SCL004141");
            token.Add("groupPC", "123456");
            token.Add("customerName", "QCC3046");
            token.Add("macDispatchType", "Normal");
            token.Add("macName", "Test");
            token.Add("macType", "BT");
            token.Add("macCount", count);
            token.Add("autogen", 1);
            token.Add("ownerEmplayeeId", "01978");
            token.Add("opId", "1111");
            token.Add("fwName", "IamFW");
            token.Add("fwVersion", "0.0.1");
            token.Add("swName", "IamSW");
            token.Add("swVersion", "0.0.1");
            token.Add("logTitle", "{\"Title\": \"aaaaa\"}");
            token.Add("logLimitUpper", "{\"Title\": \"aaaaa\"}");
            token.Add("logLimitLower", "{\"Title\": \"aaaaa\"}");

            //string[] dutSplits = OperatorSingleton.Instance.NetDutComputers.Split(',');
            //foreach (var item in dutSplits)
            //{
            //    duts.Add(item.Trim());
            //}
            //token.Add("dutNames", duts);

            // -- tray
            jObject.Add("input", token);
            return await this.GqlHttpRequest<CreateTestConfigurationResponse>(gqlstr, jObject);
        }

        #endregion TRY Fake Mac Dispatch

        #region TRY Real Mac Distpach

        private const int mRealTotalCount = 256;
        private const int mRealTryPeople = 6;
        private const string mRealLotCode = "112235";
        private const string mRealPO = "1";
        private const string mRealSerialPrefix = mRealLotCode;
        private int mRealSerialIndex = 0;

        private bool mRealFinish = false;
        private Mutex mRealMutex = new Mutex();
        private Mutex mRealSerialMutex = new Mutex();

        private void Real_setFinish(int remainCount)
        {
            this.mRealMutex.WaitOne();
            this.mRealFinish = remainCount <= 0;
            this.mRealMutex.ReleaseMutex();
        }

        private bool Real_isFinish()
        {
            bool isFinish = false;
            this.mRealMutex.WaitOne();
            isFinish = this.mRealFinish;
            this.mRealMutex.ReleaseMutex();
            return isFinish;
        }

        private string Real_getSerialNum()
        {
            int loop = 0;
            this.mRealSerialMutex.WaitOne();
            loop = this.mRealSerialIndex;
            this.mRealSerialIndex++;
            this.mRealSerialMutex.ReleaseMutex();
            return mRealSerialPrefix + "_" + loop.ToString();
        }

        private async Task Real_StartToTest(string lotcode)
        {
            int loop = 0;
            do
            {
                try
                {
                    string serialNum = Real_getSerialNum();
                    Stopwatch ssw = new Stopwatch();
                    NetworkTestItem network = new NetworkTestItem();
                    network.TwoD = "1125245645";
                    network.TwoD_Model = "GGGYYY";
                    GraphQL.GraphQLResponse<DispatchProductMacResponse> r1 = await this.Real_DispatchProductMac(lotcode, serialNum, network);
                    this.Real_setFinish(r1.Data.DispatchProductMac.IsFinish ? 0 : 1000);

                    if (!this.Real_isFinish() && r1.Data.DispatchProductMac.Mac.Trim().Length > 0)
                    {
                        Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "___" +
                           "Real Dispatch mac --> " + r1.Data.DispatchProductMac.Mac);
                        SyncData syncData = new SyncData();
                        syncData.LotNo = lotcode;
                        syncData.MacAddress = r1.Data.DispatchProductMac.Mac;
                        GraphQL.GraphQLResponse<ProductUploadResponse> r2 = await this.Real_SyncProductMACResultWithoutLog(serialNum, syncData);
                        this.Fake_setFinish(r2.Data.SyncProductMACResultWithoutLog.Unused);
                    }
                    loop++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fail --> " + ex.Message);
                }
            } while (!this.Fake_isFinish());

            Console.WriteLine("Real Dispatch Finish !!");
        }

        private async Task Try_Real()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            mRealFinish = false;
            mRealSerialIndex = 0;
            Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "___ Ready to test !!");
            await this.Real_SyncPo(mRealLotCode, mRealPO, mRealTotalCount);
            await this.Real_CreateTestConfigurtaion(mRealLotCode, mRealPO, mRealTotalCount);
            Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "___" + "Create Real lot : " + mRealLotCode + " OK!!!");

            List<Task> lists = new List<Task>();
            for (int i = 0; i < mRealTryPeople; i++)
            {
                Task Task1 = this.Real_StartToTest(mRealLotCode);
                lists.Add(Task1);
            }

            await Task.WhenAll(lists.ToArray());

            sw.Stop();
            Console.WriteLine($"Real Total Spend {sw.ElapsedMilliseconds}ms");
        }

        public async Task<GraphQL.GraphQLResponse<SyncPOResponse>> Real_SyncPo(string lotCode, string po, int count)
        {
            string gqlstr = @"mutation($input: SyncPoInput!)
                            {
                                SyncPo(input: $input)
                                {
                                    Result
                                }
                            }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var token = new Newtonsoft.Json.Linq.JObject();
            token.Add("po", po);
            token.Add("lotcode", lotCode);
            token.Add("count", count);
            jObject.Add("input", token);

            return await this.GqlHttpRequest<SyncPOResponse>(gqlstr, jObject);
        }

        public async Task<GraphQL.GraphQLResponse<ProductCreateTestConfigurationResponse>> Real_CreateTestConfigurtaion(string lotCode, string po, int count)
        {
            string gqlstr = @"
			       mutation($input: Product_TestConfigurationCreationInput!){
                    CreateProductTestConfiguration(input: $input){
                        Result
                        CurrentStatus
                        SwName
                        SwVersion
                        FwName
                        FwVersion
                    }
                  }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var token = new Newtonsoft.Json.Linq.JObject();
            var duts = new Newtonsoft.Json.Linq.JArray();
            token.Add("testPGMName", "Test");
            token.Add("groupPC", "123456");
            token.Add("pid", "0");
            token.Add("lotcode", lotCode);
            token.Add("po", po);
            token.Add("productDeviceName", "SCL004141");
            token.Add("testLoadBoard", "Test");
            token.Add("testMode", "FT2");
            token.Add("testDutMode", "Product");
            token.Add("testFlow", "FT2");
            token.Add("productFamilyName", "QCC3046");
            token.Add("customerName", "QCC3046");
            token.Add("macDispatchType", "Normal");
            token.Add("macName", "Test");
            token.Add("macType", "BT");
            token.Add("macCount", count);
            token.Add("autogen", 1);
            token.Add("ownerEmplayeeId", "01978");
            token.Add("opId", "1111");
            token.Add("fwName", "IamFW");
            token.Add("fwVersion", "0.0.1");
            token.Add("swName", "IamSW");
            token.Add("swVersion", "0.0.1");
            token.Add("logTitle", "{\"Title\": \"aaaaa\"}");
            token.Add("logLimitUpper", "{\"Title\": \"aaaaa\"}");
            token.Add("logLimitLower", "{\"Title\": \"aaaaa\"}");
            jObject.Add("input", token);
            return await this.GqlHttpRequest<ProductCreateTestConfigurationResponse>(gqlstr, jObject);
        }

        public async Task<GraphQL.GraphQLResponse<DispatchProductMacResponse>> Real_DispatchProductMac(string lotCode, string serialNum, NetworkTestItem item)
        {
            string gqlstr = @"
			        mutation($input: MacDispatchInput!){
                        DispatchProductMac(input: $input){
                            Result
                            IsNoMac
                            IsFinish
                            Mac
                            Name
                            SipLicense
                        }
                    }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var token = new Newtonsoft.Json.Linq.JObject();

            token.Add("productFamily", "QCC3046");
            token.Add("productDevice", "SCL004141");
            token.Add("lotCode", lotCode);
            token.Add("dutPCName", "123456");
            token.Add("opId", "1111");
            token.Add("groupPC", "123456");
            token.Add("testFlow", "FT1");
            token.Add("testerEmplayeeId", "01978");
            token.Add("barcode", item.TwoD);
            token.Add("barcode_vendor", item.TwoD_Model);
            token.Add("sipserialname", serialNum);
            jObject.Add("input", token);

            return await this.GqlHttpRequest<DispatchProductMacResponse>(gqlstr, jObject);
        }

        public async Task<GraphQL.GraphQLResponse<ProductUploadResponse>> Real_SyncProductMACResultWithoutLog(string serialNum, SyncData data)
        {
            string gqlstr = @"
			        mutation($input: SyncIput!){
                          SyncProductMACResultWithoutLog(input: $input){
                            Total
                            Fail
                            Pass
                            Unused
                            Using
                            TrayName
                            TrayX
                            TrayY
                            LastMac
                            LastMacName
                        }
                    }";

            var jObject = new Newtonsoft.Json.Linq.JObject();
            var inputVar = new Newtonsoft.Json.Linq.JObject();

            inputVar.Add("uploadLog", 1);
            inputVar.Add("resultSummary", "{\"Result\":\"OK\"}");
            inputVar.Add("address", data.MacAddress);
            inputVar.Add("result", "Pass");
            inputVar.Add("errorCode", "0001");
            inputVar.Add("lotcode", data.LotNo);
            inputVar.Add("productFamily", "QCC3046");
            inputVar.Add("productDevice", "SCL004141");
            inputVar.Add("macType", "BT");
            inputVar.Add("sipserialname", serialNum);
            jObject.Add("input", inputVar);
            return await this.GqlHttpRequest<ProductUploadResponse>(gqlstr, jObject);
        }

        #endregion TRY Real Mac Distpach

        public async Task<GraphQL.GraphQLResponse<MGRStatusResponse>> MySQLMgrForceSingleMaster()
        {
            string gqlstr = @"
			        mutation{
                      MYSQL_MGR_Force_Single_Member_For_This_Host{
                        CHANNEL_NAME
                        MEMBER_ID
                        MEMBER_HOST
                        MEMBER_PORT
                        MEMBER_STATE
                        MEMBER_ROLE
                        MEMBER_VERSION
                      }
                    }";

            return await this.GqlHttpRequest<MGRStatusResponse>(gqlstr, null);
        }

        private async Task<GraphQL.GraphQLResponse<T>> GqlHttpRequest<T>(string gqlStr, Newtonsoft.Json.Linq.JObject jObject)
        {
            int retryCount = 0;
            GraphQLHttpClient graphQLClient = null;
            const int WebRetryCount = 3;
            int loopCount = WebRetryCount + 1;
            do
            {
                try
                {
                    //Console.WriteLine("Prepare to Connect Web Query : " + gqlStr + ", Param : " + jObject.ToString());
                    graphQLClient = new GraphQLHttpClient(this.mGqlURL, new NewtonsoftJsonSerializer());
                    var request = new GraphQLRequest
                    {
                        Query = gqlStr,
                        Variables = jObject
                    };

                    var graphQLResponse = await graphQLClient.SendQueryAsync<T>(request);
                    if (graphQLResponse.Errors != null && graphQLResponse.Errors.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var error in graphQLResponse.Errors)
                        {
                            sb.Append(error.Message + " ~ \r\n");
                        }
                        throw new Exception("Response Error : " + sb.ToString());
                        //throw new Exception("Response Error : " + sb.ToString());
                    }

                    if (graphQLResponse.Data == null)
                    {
                        throw new Exception("Http Unstable error !! ");
                    }

                    return graphQLResponse;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMsg = ("Connect error : " + ex.InnerException.InnerException.Message);
                        }
                        else
                        {
                            errorMsg = ("Connect error : " + ex.InnerException.Message);
                        }
                    }

                    Console.WriteLine("Web Error : " + errorMsg, true);

                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            //if(ex.InnerException.InnerException.)
                            try
                            {
                                SocketException w32ex = ex.InnerException.InnerException as SocketException;
                                if (w32ex.SocketErrorCode == SocketError.TimedOut)
                                {
                                    this.RefreshGraphql_URL();
                                    int retry = 0;

                                    do
                                    {
                                        try
                                        {
                                            Console.WriteLine("Start To Force MYSQL MGR Single Master !! ");
                                            GraphQL.GraphQLResponse<MGRStatusResponse> result = await this.MySQLMgrForceSingleMaster();
                                            Console.WriteLine("Force MYSQL MGR Single Master OK !! ");
                                        }
                                        catch (Exception exx)
                                        {
                                            string mgrError = exx.Message;
                                            if (exx.InnerException != null)
                                            {
                                                if (exx.InnerException.InnerException != null)
                                                {
                                                    mgrError = ("MGR error : " + exx.InnerException.InnerException.Message);
                                                }
                                                else
                                                {
                                                    mgrError = ("MGR error : " + exx.InnerException.Message);
                                                }
                                            }
                                            Console.WriteLine("Force MYSQL MGR Single Master Fail : " + mgrError);
                                        }
                                    } while (retry < 3);
                                }
                            }
                            catch { }
                        }
                    }

                    if (retryCount >= WebRetryCount)
                    {
                        throw new Exception(errorMsg);
                    }
                    else
                    {
                        retryCount++;
                        Console.WriteLine("*******************************************************");
                        Console.WriteLine("Prepare To Retry " + retryCount + " times !!");
                    }
                }
                finally
                {
                    graphQLClient.Dispose();
                    graphQLClient = null;
                }
            } while (retryCount < loopCount);

            throw new Exception("Http Unstable error !! ");
        }

        private async Task QueryTest()
        {
            try
            {
                var graphQLClient = new GraphQLHttpClient("http://localhost:8083/simdc_project/graphql", new NewtonsoftJsonSerializer());
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var token = new Newtonsoft.Json.Linq.JObject();
                token.Add("name", "QCC5121_EN");
                jObject.Add("input", token);
                var request = new GraphQLRequest
                {
                    Query = @"
			        query($input: ProductQueryInput){
                        QueryProduct(input: $input){
                            ID
                            Name
                            Remark
                            CreatedTime
                        }
                    }",
                    Variables = jObject
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<ProductQueryResponse>(request);
                Console.WriteLine("raw response:");
                System.Windows.Forms.MessageBox.Show(graphQLResponse.Data.QueryProduct[0].Remark);
                System.Windows.Forms.MessageBox.Show(System.Text.Json.JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    System.Windows.Forms.MessageBox.Show(ex.InnerException.Message);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
        }

        private async Task<uint> GetMacTest(uint count, string path)
        {
            GraphQLHttpClient graphQLClient = null;
            try
            {
                if (count != 1)
                {
                    if (await MacResult(count, "XX-XX-XX-XX-XX-XX", path, -1))
                    {
                        Console.WriteLine("+++++++++++++++++++END++++++++++++++++++++++++++++++");
                        File(path, "+++++++++++++++++++END++++++++++++++++++++++++++++++");
                        return 1;
                    }
                }

                graphQLClient = new GraphQLHttpClient("http://localhost:8081/graphql", new NewtonsoftJsonSerializer());
                string gqlstr = @"
			        mutation($input: SyncPoInput!){
                        SyncPo(input: $input){
                            Result
                        }
                    }";
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var token = new Newtonsoft.Json.Linq.JObject();
                token.Add("count", "8");
                token.Add("lotcode", "1");
                token.Add("po", "0");

                jObject.Add("input", token);

                var request = new GraphQLRequest
                {
                    Query = gqlstr,
                    Variables = jObject
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<ResponseMacResult>(request);
                if (graphQLResponse.Errors != null && graphQLResponse.Errors.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in graphQLResponse.Errors)
                    {
                        sb.Append(error.Message + " ~ \r\n");
                    }
                    throw new Exception("GetMacTest Response Error : " + sb.ToString());
                }

                if (!graphQLResponse.Data.SyncPo.Result)
                {
                    File(path, "GetMacTest Graphql http fail" + count);
                    Console.WriteLine("GetMacTest Graphql http fail", count);
                }
                else
                {
                    File(path, "GetMacTest Graphql http success" + count);
                    Console.WriteLine("GetMacTest Graphql http success" + count);
                }
                //StreamWriter writer = new StreamWriter(path, true, Encoding.Default);
                //writer.WriteLine("這是第一行!");
                //writer.WriteLine("這是第二行!");
                //writer.Close();

                if (await SetTestconfiguration(count, path) == 1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

                //System.Windows.Forms.MessageBox.Show(graphQLResponse.Data.SyncPo.Result.ToString());
                //System.Windows.Forms.MessageBox.Show(System.Text.Json.JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    File(path, count + " ~~ GetMacTest ~~ Error : " + ex.InnerException.Message);
                    Console.WriteLine(count + " ~~ GetMacTest ~~ Error : " + ex.InnerException.Message);
                }
                else
                {
                    File(path, count + " ~~ GetMacTest ~~ Error : " + ex.Message);
                    Console.WriteLine(count + " ~~ GetMacTest ~~ Error : " + ex.Message);
                }
            }
            finally
            {
                if (graphQLClient != null)
                {
                    graphQLClient.Dispose();
                }
                //graphQLClient.Dispose();
            }
            return 0;
        }

        private async Task<uint> SetTestconfiguration(uint count, string path)
        {
            GraphQLHttpClient graphQLClient = null;
            try
            {
                graphQLClient = new GraphQLHttpClient("http://localhost:8081/graphql", new NewtonsoftJsonSerializer());
                string gqlstr = @"
			        mutation($input: Product_TestConfigurationCreationInput!){
                        CreateProductTestConfiguration(input: $input){
                            Result
                        }
                    }";
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var token = new Newtonsoft.Json.Linq.JObject();
                var logTitle = new Newtonsoft.Json.Linq.JObject();
                var logLimitUpper = new Newtonsoft.Json.Linq.JObject();
                var logLimitLower = new Newtonsoft.Json.Linq.JObject();
                logTitle.Add("test", "12");
                logLimitUpper.Add("test", "12");
                logLimitLower.Add("test", "12");
                token.Add("testPGMName", "QCC5121_SCL0A02041_MPSink_S010101F010101_0_0_20200618");
                token.Add("groupPC", "agag");
                token.Add("pid", "gggg");
                token.Add("lotcode", "1");
                token.Add("po", "0");
                token.Add("productDeviceName", "12345");
                token.Add("testLoadBoard", "Test111");
                token.Add("testMode", "MP");
                token.Add("productFamilyName", "QCC5121");
                token.Add("customerName", "FOL");
                token.Add("macDispatchType", "Normal");
                token.Add("macName", "FT3518-D13B");
                token.Add("macType", "BT");
                token.Add("macCount", "8");
                token.Add("autogen", "1");
                token.Add("ownerEmplayeeId", "01978");
                token.Add("opId", "01978");
                token.Add("fwName", "MPSink");
                token.Add("fwVersion", "010101");
                token.Add("swName", "QCC5121_SCL0A02041_MPSink_S010101F010101_0_0_20200618");
                token.Add("swVersion", "010101");
                token.Add("logTitle", logTitle.ToString());
                token.Add("logLimitUpper", logLimitUpper.ToString());
                token.Add("logLimitLower", logLimitLower.ToString());

                jObject.Add("input", token);

                var request = new GraphQLRequest
                {
                    Query = gqlstr,
                    Variables = jObject
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<ResponseCreateTestConfigurationModel>(request);
                if (graphQLResponse.Errors != null && graphQLResponse.Errors.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in graphQLResponse.Errors)
                    {
                        sb.Append(error.Message + " ~ \r\n");
                    }
                    throw new Exception("SetTestconfiguration Response Error : " + sb.ToString());
                }

                if (!graphQLResponse.Data.CreateProductTestConfiguration.Result)
                {
                    Console.WriteLine("SetTestconfiguration Graphql http fail", count);
                    File(path, "SetTestconfiguration Graphql http fail " + count);
                }
                else
                {
                    Console.WriteLine("SetTestconfiguration Graphql http success" + count);
                    File(path, "SetTestconfiguration Graphql http success " + count);
                }

                if (await MacDispatch(count, path) == 1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

                //System.Windows.Forms.MessageBox.Show(graphQLResponse.Data.SyncPo.Result.ToString());
                //System.Windows.Forms.MessageBox.Show(System.Text.Json.JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    File(path, count + " ~~ SetTestconfiguration ~~ Error : " + ex.InnerException.Message);
                    Console.WriteLine(count + " ~~ SetTestconfiguration ~~ Error : " + ex.InnerException.Message);
                }
                else
                {
                    File(path, count + " ~~ SetTestconfiguration ~~ Error : " + ex.Message);
                    Console.WriteLine(count + " ~~ SetTestconfiguration ~~ Error : " + ex.Message);
                }
            }
            finally
            {
                graphQLClient.Dispose();
            }
            return 0;
        }

        private async Task<uint> MacDispatch(uint count, string path)
        {
            GraphQLHttpClient graphQLClient = null;
            try
            {
                graphQLClient = new GraphQLHttpClient("http://localhost:8081/graphql", new NewtonsoftJsonSerializer());
                string gqlstr = @"
			        mutation($input: MacDispatchInput!){
                        DispatchProductMac(input: $input){
                            Result
                            Mac
                            Name
                           IsNoMac
                           IsFinish
                        }
                    }";
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var token = new Newtonsoft.Json.Linq.JObject();
                token.Add("lotCode", "1");
                token.Add("testFlow", "FT1");
                token.Add("productFamily", "QCC5121");
                token.Add("groupPC", "MT8872_001_1");
                token.Add("productDevice", "12345");
                token.Add("opId", "1111");
                token.Add("dutPCName", "G1");
                token.Add("testerEmplayeeId", "01978");

                jObject.Add("input", token);

                var request = new GraphQLRequest
                {
                    Query = gqlstr,
                    Variables = jObject
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<Dispatch>(request);
                if (graphQLResponse.Errors != null && graphQLResponse.Errors.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in graphQLResponse.Errors)
                    {
                        sb.Append(error.Message + " ~ \r\n");
                    }
                    throw new Exception("MacDispatch Response Error : " + sb.ToString());
                }

                if (!graphQLResponse.Data.DispatchProductMac.Result)
                {
                    Console.WriteLine("MacDispatch Graphql http fail", count);
                    File(path, "MacDispatch Graphql http fail " + count);
                }
                else
                {
                    Console.WriteLine("MacDispatch Graphql http success" + count);
                    Console.WriteLine("MacDispatch MAC " + graphQLResponse.Data.DispatchProductMac.Mac);
                    File(path, "MacDispatch Graphql http success " + count + "\nMacDispatch MAC " + graphQLResponse.Data.DispatchProductMac.Mac);
                }

                if (await MacResult(count, graphQLResponse.Data.DispatchProductMac.Mac, path, 1))
                {
                    Console.WriteLine("+++++++++++++++++++END++++++++++++++++++++++++++++++");
                    File(path, "+++++++++++++++++++END++++++++++++++++++++++++++++++");
                    return 1;
                }
                else
                {
                    return 0;
                }

                //if (!graphQLResponse.Data.DispatchProductMac.IsFinish)
                //{
                //    if (await MacResult(count, graphQLResponse.Data.DispatchProductMac.Mac, path))
                //    {
                //        Console.WriteLine("+++++++++++++++++++END++++++++++++++++++++++++++++++");
                //        File(path, "+++++++++++++++++++END++++++++++++++++++++++++++++++");
                //        return 1;
                //    }
                //    else
                //    {
                //        return 0;

                //    }
                //}
                //else
                //{
                //    Console.WriteLine("+++++++++++++++++++END++++++++++++++++++++++++++++++");
                //    File(path, "+++++++++++++++++++END++++++++++++++++++++++++++++++");
                //    return 1;
                //}
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(count + " ~~ MacDispatch ~~ Error : " + ex.InnerException.Message);
                    File(path, count + " ~~ MacDispatch ~~ Error : " + ex.InnerException.Message);
                }
                else
                {
                    Console.WriteLine(count + " ~~MacDispatch ~~ Error : " + ex.Message);
                    File(path, count + " ~~MacDispatch ~~ Error : " + ex.Message);
                }
            }
            finally
            {
                graphQLClient.Dispose();
            }

            if (Global.iVars == 1)
            {
                Console.WriteLine("+++++++++++++++++++END++++++++++++++++++++++++++++++");
                File(path, "+++++++++++++++++++END++++++++++++++++++++++++++++++");
                return 1;
            }
            return 0;
        }

        private async Task<bool> MacResult(uint count, string mac, string path, int uplog)
        {
            GraphQLHttpClient graphQLClient = null;
            try
            {
                graphQLClient = new GraphQLHttpClient("http://localhost:8081/graphql", new NewtonsoftJsonSerializer());
                string gqlstr = @"
			        mutation($input: SyncIput!){
                        SyncProductMACResultWithoutLog(input: $input){
                            Total
                            Fail
                            Pass
                            Unused
                            Using
                            LastMac
                            LastMacName
                            TrayName
                            TrayX
                            TrayY
                        }
                    }";

                Random Rnd = new Random();
                var result = "pass";

                //    System.Windows.Forms.MessageBox.Show(String.Format("~~~~~{0}", res));
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var resultSummary = new Newtonsoft.Json.Linq.JObject();
                resultSummary.Add("test", "1");
                var token = new Newtonsoft.Json.Linq.JObject();
                token.Add("uploadLog", uplog.ToString());
                token.Add("lotcode", "1");
                token.Add("productFamily", "QCC5121");
                token.Add("productDevice", "12345");
                token.Add("errorCode", "fail1111");
                token.Add("macType", "BT");
                token.Add("address", mac.ToString());
                token.Add("sipserialname", mac.ToString());
                token.Add("result", result.ToString());
                token.Add("resultSummary", resultSummary.ToString());

                jObject.Add("input", token);

                var request = new GraphQLRequest
                {
                    Query = gqlstr,
                    Variables = jObject
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<Result>(request);
                if (graphQLResponse.Errors != null && graphQLResponse.Errors.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in graphQLResponse.Errors)
                    {
                        sb.Append(error.Message + " ~ \r\n");
                    }
                    throw new Exception("MacResult Response Error : " + sb.ToString());
                }
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog MAC {0}", mac.ToString());
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog Total {0}", graphQLResponse.Data.SyncProductMACResultWithoutLog.Total);
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog Using {0}", graphQLResponse.Data.SyncProductMACResultWithoutLog.Using);
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog Unused {0}", graphQLResponse.Data.SyncProductMACResultWithoutLog.Unused);
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog Pass {0}", graphQLResponse.Data.SyncProductMACResultWithoutLog.Pass);
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog Fail {0}", graphQLResponse.Data.SyncProductMACResultWithoutLog.Fail);
                Console.WriteLine("Graphql http SyncProductMACResultWithoutLog LastMac {0}", graphQLResponse.Data.SyncProductMACResultWithoutLog.LastMac);
                Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                File(path, "Graphql http SyncProductMACResultWithoutLog MAC：" + mac.ToString()
                    + "\nGraphql http SyncProductMACResultWithoutLog Total：" + graphQLResponse.Data.SyncProductMACResultWithoutLog.Total
                    + "\nGraphql http SyncProductMACResultWithoutLog Using：" + graphQLResponse.Data.SyncProductMACResultWithoutLog.Using
                    + "\nGraphql http SyncProductMACResultWithoutLog Unused：" + graphQLResponse.Data.SyncProductMACResultWithoutLog.Unused
                    + "\nGraphql http SyncProductMACResultWithoutLog Pass：" + graphQLResponse.Data.SyncProductMACResultWithoutLog.Pass
                    + "\nGraphql http SyncProductMACResultWithoutLog Fail：" + graphQLResponse.Data.SyncProductMACResultWithoutLog.Fail
                    + "\nGraphql http SyncProductMACResultWithoutLog LastMac：" + graphQLResponse.Data.SyncProductMACResultWithoutLog.LastMac
                    + "\n----------------------------------------------------------------------------------------------------------");
                //if (graphQLResponse.Data.SyncProductMACResultWithoutLog.Total == 1)
                //{
                //    Console.WriteLine("Graphql http fail", count);

                //}
                //else
                //{
                //    Console.WriteLine("Graphql http success" + count);
                //}
                if (graphQLResponse.Data.SyncProductMACResultWithoutLog.Unused == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(mac + " ~~MacResult  ~~ Error : " + ex.InnerException.Message);
                    File(path, count + " ~~MacResult ~~ Error : " + ex.InnerException.Message);
                }
                else
                {
                    Console.WriteLine(mac + " ~~MacResult ~~ Error : " + ex.Message);
                    File(path, count + " ~~MacResult ~~ Error : " + ex.Message);
                }
            }
            finally
            {
                graphQLClient.Dispose();
            }
            return false;
        }

        private void File(string path, string con)
        {
            // Create an instance of StreamWriter to write text to a file.
            // The using statement also closes the StreamWriter.
            using (StreamWriter sw = new StreamWriter(path, true))   //小寫TXT
            {
                // Add some text to the file.

                sw.WriteLine(con);
                sw.Close();
            }
        }

        private string SyncQueryMac()
        {
            try
            {
                Task<string> queryTask = this.QueryMac();
                Task.WaitAll(queryTask);
                return queryTask.Result;
            }
            catch
            {
                throw;
            }
        }

        private bool SyncMacCreate50000(string mac)
        {
            try
            {
                Task<bool> createTask = this.MacCreate50000(mac);
                Task.WaitAll(createTask);
                return createTask.Result;
            }
            catch
            {
                throw;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Thread thread = new Thread(() =>
                {
                    string mac = "50-38-ff-00-00-00";
                    int i = 1;
                    // SyncMacCreate50000(mac);
                    for (; i <= 2; i++)
                    {
                        //string lastmac = SyncQueryMac();

                        //Console.WriteLine("-------" + lastmac.Replace("-", ""));
                        //long chkNum = Convert.ToInt64(lastmac.Replace("-", ""), 16);
                        Console.WriteLine("-------" + mac.Replace("-", ""));
                        long chkNum = Convert.ToInt64(mac.Replace("-", ""), 16);
                        Console.WriteLine("-------" + chkNum);
                        chkNum += 1;

                        Console.WriteLine("-------" + Convert.ToString(chkNum, 16).ToString().Insert(2, "-").Insert(5, "-").Insert(8, "-").Insert(11, "-").Insert(14, "-"));
                        SyncMacCreate50000(Convert.ToString(chkNum, 16).ToString().Insert(2, "-").Insert(5, "-").Insert(8, "-").Insert(11, "-").Insert(14, "-"));
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine("err --> ", ex.InnerException.Message);
                }
                else
                {
                    Console.WriteLine("err --> ", ex.Message);
                }
            }
        }

        private async Task<bool> MacCreate50000(string mac)
        {
            GraphQLHttpClient graphQLClient = null;
            try
            {
                graphQLClient = new GraphQLHttpClient("http://localhost:8081/graphql", new NewtonsoftJsonSerializer());
                string gqlstr = @"
			        mutation($input: MacCreationInput!){
                        CreateMacAddress(input: $input){
                            Result
                        }
                    }";
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var token = new Newtonsoft.Json.Linq.JObject();
                token.Add("address", mac);
                token.Add("totalCount", "2");
                token.Add("macType", "BT");
                token.Add("increaseType", "Normal");
                token.Add("ownerEmplayeeId", "01978");
                jObject.Add("input", token);

                var request = new GraphQLRequest
                {
                    Query = gqlstr,
                    Variables = jObject
                };

                //Task<ResponseModel> checkFinishTask = this._syncWebInfo();
                //Task.WaitAll(checkFinishTask);

                var graphQLResponse = await graphQLClient.SendQueryAsync<ResponseModel>(request);
                if (graphQLResponse.Errors != null && graphQLResponse.Errors.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in graphQLResponse.Errors)
                    {
                        sb.Append(error.Message + " ~ \r\n");
                    }
                    throw new Exception("CreateMacAddress Response Error : " + sb.ToString());
                }

                if (!graphQLResponse.Data.CreateMacAddress.Result)
                {
                    Console.WriteLine("CreateMacAddress Graphql http fail");
                    throw new Exception("CreateMacAddress Graphql http fail");
                }
                else
                {
                    Console.WriteLine("CreateMacAddress Graphql http success");
                    Console.WriteLine("CreateMacAddress  " + graphQLResponse.Data.CreateMacAddress.Result);
                }
                return true;
            }
            catch (Exception ex)
            {
                //if (ex.InnerException != null)
                //{
                //    Console.WriteLine(" ~~ CreateMacAddress ~~ Error : " + ex.InnerException.Message);

                //}
                //else
                //{
                //    Console.WriteLine(" ~~CreateMacAddress ~~ Error : " + ex.Message);

                //}
                throw;
            }
            finally
            {
                graphQLClient.Dispose();
            }
        }

        private async Task<string> QueryMac()
        {
            GraphQLHttpClient graphQLClient = null;
            try
            {
                graphQLClient = new GraphQLHttpClient("http://localhost:8081/graphql", new NewtonsoftJsonSerializer());
                var jObject = new Newtonsoft.Json.Linq.JObject();
                var token = new Newtonsoft.Json.Linq.JObject();

                jObject.Add("input", null);
                var request = new GraphQLRequest
                {
                    Query = @"
			        query($input: MacQueryInput){
                        QueryLastBTMacAddressInMP(input: $input){
                            Address
                        }
                    }",
                    Variables = jObject
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<MacAddress>(request);
                Console.WriteLine("raw response:" + graphQLResponse.Data.QueryLastBTMacAddressInMP.Address);
                return graphQLResponse.Data.QueryLastBTMacAddressInMP.Address.ToString();
            }
            catch (Exception ex)
            {
                //if (ex.InnerException != null)
                //{
                //    System.Windows.Forms.MessageBox.Show(ex.InnerException.Message);
                //}
                //else
                //{
                //    System.Windows.Forms.MessageBox.Show(ex.Message);
                //}

                throw;
            }
            finally
            {
                graphQLClient.Dispose();
            }
            return "Error";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}