using MySql.Data.MySqlClient;
using EventHandle;
using MT8872AFIXTURE.LCS5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacMaster.LCS5
{
    internal class DataSystemCtrl
    {
        public enum ATS_Result
        {
            NONE = 0,
            PASS = 1,
            NG = 2,
            WARNING = 3,
            RETEST = 4,
        }

        public struct ConfigureData
        {
            public string ATSLogMode;
            public string DataBase;
            public string FTP_Username;
            public string FTP_Password;
            public string FTP_IP;
            public string Station;
            public string SFCSIP;
        }

        public struct UploadFTPData
        {
            public string FTP_IP;
            public string FTP_Username;
            public string FTP_Password;
            public string ATSLogPath;
            public string PN;
            public string MO;
            public string Station;
            public bool TestPass;
        }

        public struct WriteSQLData
        {
            public bool FirstTest;
            public bool InSFCS;
            public bool GoldenSample;
            public string PN;
            public string SFCSIP;
            public string Fixture;
            public string SN;
            public string Result;
            public string FTPLog;
            public string NGItem;
            public string ErrorCode;
            public string Station;
            public string Warning;
            public double TestTime;
            public ArrayList ListDataSFCS;
        }
    }
    public class FtpWeb
        {
            public static event EventLogHandler Message;

            protected virtual void OnMessageDisplay(EventLogArgs e)
            {
                if (Message != null)
                {
                    Message(e);
                }
            }

            private void DisplayMsg(string message)
            {
                EventLogArgs eLog = new EventLogArgs(message);
                OnMessageDisplay(eLog);
            }

            public string ftpServerIP;
            string ftpRemotePath;
            string ftpUserID;
            string ftpPassword;
            public string ftpURI;

            /// <summary> 連接FTP </summary> 
            /// <param name="FtpServerIP">FTP Server IP</param> 
            /// <param name="FtpRemotePath">指定FTP連接成功後的當前目錄, 如果不指定預設為根目錄</param> 
            /// <param name="FtpUserID">帳號</param> 
            /// <param name="FtpPassword">密碼</param> 
            public FtpWeb(string FtpServerIP, string FtpRemotePath, string FtpUserID, string FtpPassword)
            {
                ftpServerIP = FtpServerIP;
                ftpRemotePath = FtpRemotePath;
                ftpUserID = FtpUserID;
                ftpPassword = FtpPassword;
                ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "";
            }

            /// <summary> 上傳 </summary> 
            /// <param name="filename"></param> 
            public void Upload(string filename)
            {
                FileInfo fileInf = new FileInfo(filename);
                string uri = ftpURI + fileInf.Name;
                FtpWebRequest reqFTP;

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.ContentLength = fileInf.Length;
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                FileStream fs = fileInf.OpenRead();

                try
                {
                    Stream strm = reqFTP.GetRequestStream();
                    contentLen = fs.Read(buff, 0, buffLength);

                    while (contentLen != 0)
                    {
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }

                    strm.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][Upload][Exception] --> " + ex.Message);
                }
            }

            public void Upload(string dec_fp, string scr_fp)
            {

                FileInfo fileInf = new FileInfo(scr_fp);
                FtpWebRequest reqFTP;

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(dec_fp));
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.ContentLength = fileInf.Length;
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                FileStream fs = File.Open(scr_fp, FileMode.Open);

                try
                {
                    Stream strm = reqFTP.GetRequestStream();
                    contentLen = fs.Read(buff, 0, buffLength);

                    while (contentLen != 0)
                    {
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }

                    strm.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][Upload][Exception] --> " + ex.Message);
                }
            }

            public bool Upload2(string dec_fp, string scr_fp)
            {
                try
                {
                    //string uri = ftpURI + dec_fp;
                    FileInfo fileInf = new FileInfo(scr_fp);
                    FtpWebRequest reqFTP;

                    reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(dec_fp));
                    reqFTP.Proxy = null;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    reqFTP.KeepAlive = false;
                    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                    reqFTP.UseBinary = true;
                    reqFTP.ContentLength = fileInf.Length;
                    int buffLength = 2048;
                    byte[] buff = new byte[buffLength];
                    int contentLen;
                    FileStream fs = File.Open(scr_fp, FileMode.Open);

                    Stream strm = reqFTP.GetRequestStream();
                    contentLen = fs.Read(buff, 0, buffLength);

                    while (contentLen != 0)
                    {
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }

                    strm.Close();
                    fs.Close();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary> 下載 </summary> 
            /// <param name="filePath"></param> 
            /// <param name="fileName"></param> 
            public void Download(string filePath, string fileName)
            {
                FtpWebRequest reqFTP;
                try
                {
                    FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);

                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + fileName));
                    reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    Stream ftpStream = response.GetResponseStream();
                    long cl = response.ContentLength;
                    int bufferSize = 2048;
                    int readCount;
                    byte[] buffer = new byte[bufferSize];

                    readCount = ftpStream.Read(buffer, 0, bufferSize);

                    while (readCount > 0)
                    {
                        outputStream.Write(buffer, 0, readCount);
                        readCount = ftpStream.Read(buffer, 0, bufferSize);
                    }

                    ftpStream.Close();
                    outputStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][Download][Exception] --> " + ex.Message);
                }
            }

            public void Download(string ftpFileFullName, string filePath, string fileName)
            {
                FtpWebRequest reqFTP;

                try
                {
                    FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);

                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpFileFullName));
                    reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    Stream ftpStream = response.GetResponseStream();
                    long cl = response.ContentLength;
                    int bufferSize = 2048;
                    int readCount;
                    byte[] buffer = new byte[bufferSize];

                    readCount = ftpStream.Read(buffer, 0, bufferSize);

                    while (readCount > 0)
                    {
                        outputStream.Write(buffer, 0, readCount);
                        readCount = ftpStream.Read(buffer, 0, bufferSize);
                    }

                    ftpStream.Close();
                    outputStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][Download][Exception] --> " + ex.Message);
                }
            }


            /// <summary> 刪除檔案 </summary> 
            /// <param name="fileName"></param> 
            public void Delete(string fileName)
            {
                try
                {
                    //string uri = ftpURI + fileName;
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(fileName));

                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    reqFTP.KeepAlive = false;
                    reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                    string result = String.Empty;
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    long size = response.ContentLength;
                    Stream datastream = response.GetResponseStream();
                    StreamReader sr = new StreamReader(datastream);
                    result = sr.ReadToEnd();
                    sr.Close();
                    datastream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][Delete][Exception] --> " + ex.Message + "  文件名:" + fileName);
                }
            }

            /// <summary> 刪除資料夾 </summary> 
            /// <param name="folderName"></param> 
            public void RemoveDirectory(string folderName)
            {
                try
                {
                    //string uri = ftpURI + folderName;
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(folderName));

                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    reqFTP.KeepAlive = false;
                    reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;

                    string result = String.Empty;
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    long size = response.ContentLength;
                    Stream datastream = response.GetResponseStream();
                    StreamReader sr = new StreamReader(datastream);
                    result = sr.ReadToEnd();
                    sr.Close();
                    datastream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][Delete][Exception] --> " + ex.Message + "  文件名:" + folderName);
                }
            }

            /// <summary> 取得當前目錄明細(包含檔案和資料夾) </summary> 
            /// <returns></returns> 
            public string[] GetFilesDetailList()
            {
                string[] downloadFiles;
                try
                {
                    StringBuilder result = new StringBuilder();
                    FtpWebRequest ftp;
                    ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
                    ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    WebResponse response = ftp.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                    string line = reader.ReadLine();

                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }

                    reader.Close();
                    response.Close();

                    if (result.Length != 0)
                        result.Remove(result.ToString().LastIndexOf("\n"), 1);
                    else
                        return null;

                    return result.ToString().Split('\n');
                }
                catch (Exception ex)
                {
                    downloadFiles = null;
                    DisplayMsg("[FtpWeb][GetFilesDetailList][Exception] --> " + ex.Message);
                    return downloadFiles;
                }
            }

            /// <summary> 取得當前目錄下檔案列表 </summary> 
            /// <returns></returns> 
            public string[] GetFileList(string mask)
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest reqFTP;

                try
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                    WebResponse response = reqFTP.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                    string line = reader.ReadLine();

                    while (line != null)
                    {
                        if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                        {

                            string mask_ = mask.Substring(mask.IndexOf("*") + 1, mask.Length - mask.IndexOf("*") - 1);

                            if (line.ToLower().Contains(mask_.ToLower()))
                            {
                                result.Append(line);
                                result.Append("\n");
                            }
                        }
                        else
                        {
                            result.Append(line);
                            result.Append("\n");
                        }
                        line = reader.ReadLine();
                    }
                    reader.Close();
                    response.Close();

                    if (result.Length != 0)
                        result.Remove(result.ToString().LastIndexOf('\n'), 1);
                    else
                        return null;

                    return result.ToString().Split('\n');
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][GetFileList][Exception] --> " + ex.Message.ToString());
                    return null;
                }
            }

            /// <summary> 取得當前目錄下所有的資料夾 </summary> 
            /// <returns></returns> 
            public string[] GetDirectoryList()
            {
                string[] drectory = GetFilesDetailList();
                string m = string.Empty;
                if (drectory != null)
                {
                    foreach (string str in drectory)
                    {
                        int dirPos = str.IndexOf("<DIR>");

                        if (dirPos > 0)
                        {
                            m += str.Substring(dirPos + 5).Trim() + "\n";
                        }
                        else if (str.Trim().Substring(0, 1).ToUpper() == "D")
                        {
                            string[] sSplit = str.Split(' ');
                            string dir = sSplit[sSplit.Length - 1].Trim();

                            if (dir != "." && dir != "..")
                            {
                                m += dir + "\n";
                            }
                        }
                    }

                    if (m != string.Empty)
                    {
                        m = m.Remove(m.ToString().LastIndexOf('\n'), 1);

                        return m.Split('\n');
                    }
                    else
                        return null;
                }
                else
                    return null;
            }

            /// <summary> 遍歷當前目錄所有子目錄、檔案 </summary> 
            /// <returns></returns> 
            public void GetSubDirectoryList(ref ArrayList AllFileList)
            {
                string[] files = GetFileList("*.txt");

                if (files != null)
                {
                    foreach (string file in files)
                    {
                        AllFileList.Add(file);
                    }
                }

                string[] directory = GetDirectoryList();

                if (directory != null)
                {
                    foreach (string str in directory)
                    {
                        GotoDirectory(str, false);
                        GetSubDirectoryList(ref AllFileList);
                        GotoDirectory(ftpURI.Replace("ftp://" + ftpServerIP + "/", "").Replace(str + "//", ""), true);
                    }
                }
            }

            public void GetSubDirectoryList(string mask, ref ArrayList AllFileList, ref ArrayList AllFileFullName)
            {
                string[] files = GetFileList(mask);

                if (files != null)
                {
                    foreach (string file in files)
                    {
                        AllFileList.Add(file);
                        AllFileFullName.Add(ftpURI + file);
                    }
                }

                string[] directory = GetDirectoryList();

                if (directory != null)
                {
                    foreach (string str in directory)
                    {
                        GotoDirectory(str, false);
                        GetSubDirectoryList(mask, ref AllFileList, ref AllFileFullName);
                        GotoDirectory(ftpURI.Replace("ftp://" + ftpServerIP + "/", "").Replace(str + "/", ""), true);
                    }
                }
            }

            /// <summary> 判斷當前目錄下指定的子目錄是否存在 </summary> 
            /// <param name="RemoteDirectoryName">指定的目錄名稱</param> 
            public bool DirectoryExist(string RemoteDirectoryName)
            {
                string[] dirList = GetDirectoryList();

                if (dirList != null)
                {
                    foreach (string str in dirList)
                    {
                        if (str.Trim() == RemoteDirectoryName.Trim())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary> 判斷當前目錄下指定的檔案是否存在 </summary> 
            /// <param name="RemoteFileName">遠端檔案名</param> 
            public bool FileExist(string RemoteFileName)
            {
                string[] fileList = GetFileList("*.*");

                if (fileList != null)
                {
                    foreach (string str in fileList)
                    {
                        if (str.Trim() == RemoteFileName.Trim())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary> 建立資料夾 </summary> 
            /// <param name="dirName"></param> 
            public void MakeDir(string dirName)
            {
                FtpWebRequest reqFTP;

                try
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(dirName));
                    reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    Stream ftpStream = response.GetResponseStream();

                    ftpStream.Close();
                    response.Close();
                }
                catch (Exception)
                {
                }
            }

            /// <summary> 取得指定檔案大小 </summary> 
            /// <param name="filename"></param> 
            /// <returns></returns> 
            public long GetFileSize(string filename)
            {
                FtpWebRequest reqFTP;
                long fileSize = 0;

                try
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + filename));
                    reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    Stream ftpStream = response.GetResponseStream();
                    fileSize = response.ContentLength;

                    ftpStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][GetFileSize][Exception] --> " + ex.Message);
                }
                return fileSize;
            }

            /// <summary> 取得指定檔案最後修改日期 </summary> 
            /// <param name="filename"></param> 
            /// <returns></returns> 
            public DateTime GetFileLastModified(string filename)
            {
                FtpWebRequest reqFTP;
                DateTime sLastModified = DateTime.Now;

                try
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(filename));
                    reqFTP.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    Stream ftpStream = response.GetResponseStream();
                    sLastModified = response.LastModified;

                    ftpStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][GetFileSize][Exception] --> " + ex.Message);
                }
                return sLastModified;
            }

            public string GetFileContent(string filename)
            {
                string content = "";
                FtpWebRequest reqFTP;

                try
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + filename));
                    reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                    reqFTP.UsePassive = false;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("gb2312"));
                    content = reader.ReadToEnd();

                    reader.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][GetFileContent][Exception] --> " + ex.Message);
                }
                return content;
            }

            /// <summary> 改名 </summary> 
            /// <param name="currentFilename"></param> 
            /// <param name="newFilename"></param> 
            public void ReName(string currentFilename, string newFilename)
            {
                FtpWebRequest reqFTP;

                try
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + currentFilename));
                    reqFTP.Method = WebRequestMethods.Ftp.Rename;
                    reqFTP.RenameTo = newFilename;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    Stream ftpStream = response.GetResponseStream();

                    ftpStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    DisplayMsg("[FtpWeb][ReName][Exception] --> " + ex.Message);
                }
            }

            /// <summary> 搬移檔案 </summary> 
            /// <param name="currentFilename"></param> 
            /// <param name="newFilename"></param> 
            public void MovieFile(string currentFilename, string newDirectory)
            {
                ReName(currentFilename, newDirectory);
            }

            /// <summary> 切換當前目錄 </summary> 
            /// <param name="DirectoryName"></param> 
            /// <param name="IsRoot">true 絕對路徑   false 相對路徑</param> 
            public void GotoDirectory(string DirectoryName, bool IsRoot)
            {
                if (IsRoot)
                {
                    ftpRemotePath = DirectoryName;
                }
                else
                {
                    ftpRemotePath += DirectoryName + "/";
                }
                ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "";
            }

            /// <summary> 刪除訂單目錄 </summary> 
            /// <param name="ftpServerIP">FTP Server IP</param> 
            /// <param name="folderToDelete">要刪除的目錄</param> 
            /// <param name="ftpUserID">FTP 帳號</param> 
            /// <param name="ftpPassword">FTP 密碼</param> 
            public static void DeleteOrderDirectory(string ftpServerIP, string folderToDelete, string ftpUserID, string ftpPassword)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ftpServerIP) && !string.IsNullOrEmpty(folderToDelete) && !string.IsNullOrEmpty(ftpUserID) && !string.IsNullOrEmpty(ftpPassword))
                    {
                        FtpWeb fw = new FtpWeb(ftpServerIP, folderToDelete, ftpUserID, ftpPassword);

                        fw.GotoDirectory(folderToDelete, true);

                        string[] folders = fw.GetDirectoryList();

                        foreach (string folder in folders)
                        {
                            if (!string.IsNullOrEmpty(folder) || folder != "")
                            {
                                string subFolder = folderToDelete + "/" + folder;
                                fw.GotoDirectory(subFolder, true);
                                string[] files = fw.GetFileList("*.*");

                                if (files != null)
                                {
                                    foreach (string file in files)
                                    {
                                        fw.Delete(file);
                                    }
                                }

                                fw.GotoDirectory(folderToDelete, true);
                                fw.RemoveDirectory(folder);
                            }
                        }

                        string parentFolder = folderToDelete.Remove(folderToDelete.LastIndexOf('/'));
                        string orderFolder = folderToDelete.Substring(folderToDelete.LastIndexOf('/') + 1);
                        fw.GotoDirectory(parentFolder, true);
                        fw.RemoveDirectory(orderFolder);
                    }
                    else
                    {
                        throw new Exception("FTP 路徑不能為空！");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("删除訂單錯誤：" + ex.Message);
                }
            }
        }
    public class DataSystem
        {
            public static event EventLogHandler Message;

            protected virtual void OnMessageDisplay(EventLogArgs e)
            {
                if (Message != null)
                {
                    Message(e);
                }
            }

            private void DisplayMsg(string message)
            {
                EventLogArgs eLog = new EventLogArgs(message);
                OnMessageDisplay(eLog);
            }

            public class _ConditionStruct
            {
                public string Field = "";
                public string Input = "";
                public string KeyWord = "";

                public _ConditionStruct(string field, string input, string keyword)
                {
                    Field = field;
                    Input = input;
                    KeyWord = keyword;
                }
            }

            private struct GlobalVariable
            {
                public string BU;
                public string ConnectStr;
                public MySqlConnection MySqlConn;

                public GlobalVariable(string bu, string connectstr, MySqlConnection connect)
                {
                    BU = bu;
                    ConnectStr = connectstr;
                    MySqlConn = connect;
                }
            }

            private GlobalVariable _globalVariable;


            private DataSet QueryID(string field, string table, string condition)
            {
                DataSet set = new DataSet();
                CheckConnection();

                if (_globalVariable.MySqlConn.State == ConnectionState.Open)
                {
                    string query = string.Format("Select {0} from {1} where", field, table);
                    List<_ConditionStruct> cond = new List<_ConditionStruct>();

                    foreach (var item in condition.Split(','))
                    {
                        string[] fav = item.Split('=');
                        string[] vak = fav[0].Split(' ');
                        cond.Add(new _ConditionStruct((vak.Length > 1 ? vak[1].Trim() : fav[0].Trim()), fav[1].Trim(), (vak.Length > 1 ? vak[0] : "")));
                    }

                    foreach (var item in cond)
                    {
                        query += " " + item.KeyWord + " " + item.Field + "=@" + item.Field;
                    }

                    MySqlCommand cmd = new MySqlCommand(query, _globalVariable.MySqlConn);

                    foreach (var item in cond)
                    {
                        cmd.Parameters.Add(new MySqlParameter(item.Field, item.Input));
                    }

                    MySqlDataReader DataReader = cmd.ExecuteReader();
                    set.Load(DataReader, LoadOption.OverwriteChanges, new string[] { table });
                    DataReader.Close();
                }
                return set;
            }

            private bool Update(string table, List<MySqlParameter> set, List<MySqlParameter> condition, string[] keyword)
            {
                string sql = string.Format("Update {0} set ", table);

                try
                {
                    MySqlCommand cmd = new MySqlCommand(sql, _globalVariable.MySqlConn);

                    foreach (var item in set)
                    {
                        sql += item.ParameterName.ToString().Substring(1) + "=" + item.ParameterName + ",";
                        cmd.Parameters.Add(item);
                    }

                    sql = sql.Substring(0, sql.Length - 1);
                    sql += " where";

                    for (int i = 0; i < condition.Count; ++i)
                    {
                        if ((i + 1) == condition.Count)
                        {
                            sql += " " + condition[i].ParameterName.ToString().Substring(1) + "=" + condition[i].ParameterName;
                        }
                        else
                        {
                            sql += " " + condition[i].ParameterName.ToString().Substring(1) + "=" + condition[i].ParameterName + keyword[i];
                        }

                        cmd.Parameters.Add(condition[i]);
                    }

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][Update][Exception] --> " + ex.Message);
                }
            }

            private bool Insert(string table, List<MySqlParameter> value)
            {
                try
                {
                    string sql = string.Format("Insert into {0} (", table);
                    MySqlCommand cmd = new MySqlCommand(sql, _globalVariable.MySqlConn);

                    foreach (var field in value)
                    {
                        sql += field.ParameterName.Substring(1) + ",";
                        cmd.Parameters.Add(field);
                    }

                    sql = sql.Substring(0, sql.Length - 1) + ")Value(";

                    foreach (var val in value)
                    {
                        sql += val.ParameterName + ",";
                    }

                    sql = sql.Substring(0, sql.Length - 1) + ")";

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][Insert][Exception] --> " + ex.Message);
                }

            }

            private string CreateTestTable(string pn, int year, int month)
            {
                try
                {
                    string TestTableName = pn.Replace('.', '_') + "_Test_YearMonth" + year + month;

                    string sql = "CREATE TABLE IF NOT EXISTS " + TestTableName + " (" +
                                    "`ID`  int(200) NOT NULL AUTO_INCREMENT ," +
                                    "`STATION`  text(50) NULL ," +
                                    "`GOLDENSIMPLE`  int(10) NULL ," +
                                    "`RESULT`  text(10) NULL ," +
                                    "`INSFCS`  int(10) NULL ," +
                                    "`SN`  text(200) NULL ," +
                                    "WARNING text(2000) NULL ," +
                                    "`PN`  text(50) NULL ," +
                                    "`IP`  text(50) NULL ," +
                                    "`FIXTURE`  text(100) NULL ," +
                                    "`TESTTIME`  double(50,0) NULL ," +
                                    "`CreateTime`  timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP ," +
                                    "`NGITEM`  text(300) NULL ," +
                                    "`ERRORCODE`  text(100) NULL ," +
                                    "`FTPLOG`  text(200) NULL ," +
                    "`FIRSTLOAD`  text(200) NULL ," +
                                    "PRIMARY KEY (`ID`))DEFAULT CHARACTER SET=utf8 COLLATE=utf8_general_ci";

                    MySqlCommand cmd = new MySqlCommand(sql, _globalVariable.MySqlConn);
                    cmd.ExecuteNonQuery();

                    return TestTableName;
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][CreateTestTable][Exception] --> " + ex.Message);
                }
            }

            private string CreateDataTable(string testtablename, string pn, string station, int year, int month)
            {
                try
                {
                    string DataTableName = pn.Replace('.', '_') + "_TestData_YearMonth" + year + month + "_" + station.Replace(' ', '_');

                    string sql = "CREATE TABLE IF NOT EXISTS  " + DataTableName + " (`ID`  int(100) NOT NULL AUTO_INCREMENT ,`TestID`  int(100) NOT NULL,PRIMARY KEY (`ID`),CONSTRAINT `" + DataTableName + "F` FOREIGN KEY (`TestID`) REFERENCES `" + testtablename + "` (`ID`) ON DELETE RESTRICT ON UPDATE RESTRICT)";
                    //DisplayMsg(sql);
                    MySqlCommand cmd = new MySqlCommand(sql, _globalVariable.MySqlConn);
                    cmd.ExecuteNonQuery();

                    return DataTableName;
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][CreateDataTable][Exception] --> " + ex.Message);
                }
            }

            private string QueryPNID(string pn, int itemp)
            {
                string PNID = "0";
                DataSet pnDataSet = QueryID("ID", "PN", "PN=" + pn);
                if (pnDataSet.Tables[0].Rows.Count > 0)
                {
                    Update("PN", new List<MySqlParameter>() { new MySqlParameter("@Temp", itemp.ToString()) }, new List<MySqlParameter>() { new MySqlParameter("@ID", PNID) }, new string[] { "" });
                }
                else
                {
                    Insert("PN", new List<MySqlParameter>() { new MySqlParameter("@PN", pn) });
                    pnDataSet.Clear();
                    pnDataSet = QueryID("ID", "PN", "PN=" + pn);
                }
                PNID = pnDataSet.Tables[0].Rows[0].ItemArray[0].ToString();
                return PNID;
            }

            private string QueryItemID(string itemname, string tablename, string condition, string conditionfield, int itemp)
            {
                string ID = "0";
                DataSet ds = QueryID("ID", tablename, (tablename + "=" + itemname + ",and " + conditionfield + "= " + condition));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    Update(tablename, new List<MySqlParameter>() { new MySqlParameter("@Temp", itemp.ToString()) }, new List<MySqlParameter>() { new MySqlParameter("@ID", ID) }, new string[] { "" });
                }
                else
                {
                    Insert(tablename, new List<MySqlParameter>() { new MySqlParameter("@" + tablename, itemname), new MySqlParameter("@" + conditionfield, condition) });
                    ds.Clear();
                    ds = QueryID("ID", tablename, (tablename + "=" + itemname + ",and " + conditionfield + "= " + condition));
                }

                ID = ds.Tables[0].Rows[0].ItemArray[0].ToString();

                return ID;
            }

            private void CheckConnection()
            {
                try
                {
                    if (_globalVariable.MySqlConn == null)
                    {
                        _globalVariable.MySqlConn = new MySqlConnection(_globalVariable.ConnectStr);
                        _globalVariable.MySqlConn.Open();
                    }
                    else if (_globalVariable.MySqlConn.State == ConnectionState.Closed)
                    {
                        _globalVariable.MySqlConn.Open();
                    }
                }
                catch (Exception ex)
                {
                    DisplayMsg("[DataSystem][CheckConnection][Exception] --> " + ex.Message);
                }
            }

            /// <summary> 連接DB </summary> 
            /// <param name="server">DB IP</param> 
            /// <param name="user">登錄帳號</param> 
            /// <param name="pwd">登錄密碼</param> 
            /// <param name="database">使用的DB</param> 
            public bool Connection(string server, string user, string pwd, string database)
            {
                try
                {
                    string connectStr = string.Format(@"server = {0};uid = {1};pwd = {2};DataBase = {3};Allow User Variables=true;Charset=utf8", server, user, pwd, database);

                    _globalVariable.BU = database;
                    _globalVariable.ConnectStr = connectStr;

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][Connection][Exception] --> " + ex.Message);
                }
            }

            /// <summary> 關閉DB連接 </summary> 
            public bool Disconnection()
            {
                try
                {
                    if (_globalVariable.MySqlConn.State == ConnectionState.Open)
                    {
                        _globalVariable.MySqlConn.Close();
                    }

                    if (_globalVariable.MySqlConn != null)
                    {
                        _globalVariable.MySqlConn.Dispose();
                    }

                    _globalVariable.MySqlConn = null;
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][Disconnection][Exception] --> " + ex.Message);
                }
            }

            /// <summary> 上拋資料到FTP </summary> 
            /// <param name = "data" > 上拋FTP所需要的資料 </ param >
            public string UploadLogToFTP(UploadFTPData data)
            {
                string outFTPPath = "";

                try
                {
                    //  有需要加上時間在說
                    /*
                    DateTime sDateTime = DateTime.Now;
                    string _Year = sDateTime.Year.ToString();
                    string _Month = sDateTime.Month.ToString();
                    string _Day = sDateTime.Day.ToString();
                    string sTime = sDateTime.Year.ToString() + sDateTime.Month.ToString("00") + sDateTime.Day.ToString("00") + "-" + sDateTime.Hour.ToString("00") + sDateTime.Minute.ToString("00") + sDateTime.Second.ToString("00");
                    */

                    FtpWeb mftp = null;

                    //if (!data.FTP_Username.Contains("NW"))
                    //{
                    //    mftp = new FtpWeb(data.FTP_IP, "Harman", data.FTP_Username, data.FTP_Password);
                    //    data.FTP_IP = data.FTP_IP + "/Harman";
                    //}
                    //else
                    //{
                    mftp = new FtpWeb(data.FTP_IP, "", data.FTP_Username, data.FTP_Password);
                    //}

                    // create ftp folder
                    string[] ftpFolderArray = { data.FTP_IP, _globalVariable.BU, data.PN, data.MO, data.Station, ((data.TestPass) ? "Pass" : "NG") };
                    outFTPPath += "ftp://";

                    foreach (string folder in ftpFolderArray)
                    {
                        string currectFolder = (folder == "") ? "Verification" : folder;

                        outFTPPath += currectFolder + "/";
                        mftp.MakeDir(outFTPPath);
                    }

                    outFTPPath += Path.GetFileName(data.ATSLogPath);
                    DisplayMsg("[DataSystem][UploadLogToFTP] --> Rermote log path: " + outFTPPath);
                    DisplayMsg("[DataSystem][UploadLogToFTP] --> Local log path: " + data.ATSLogPath);
                    mftp.Upload2(outFTPPath, data.ATSLogPath);
                    DisplayMsg("[DataSystem][UploadLogToFTP] --> Upload ATS log done.");
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][UploadLogToFTP][Exception] --> " + ex.Message);
                }

                return outFTPPath;
            }

            /// <summary> 寫資料進DataSystem </summary> 
            /// <param name = "datat" > 寫SQL需要的資料 </ param >
            public void WriteSQLData(WriteSQLData data)
            {
                try
                {
                    using (_globalVariable.MySqlConn = new MySqlConnection(_globalVariable.ConnectStr))
                    {
                        _globalVariable.MySqlConn.Open();

                        int goldSample = (data.GoldenSample) ? 1 : 0;
                        int inSFCS = (data.InSFCS) ? 1 : 0;
                        int Year = DateTime.Now.Year;
                        int Month = DateTime.Now.Month;
                        Random ran = new Random();
                        int iTemp = ran.Next(10);

                        string currectStation = Regex.Replace(data.Station, @"[\W_]+", "_");
                        string PNID = QueryPNID(data.PN, iTemp);
                        string StationID = QueryItemID(currectStation, "Station", PNID, "PNID", iTemp);
                        string FixtureID = QueryItemID(data.Fixture, "Fixture", StationID, "StationID", iTemp);
                        string NGItemID = QueryItemID(data.NGItem, "NGItem", StationID, "StationID", iTemp);
                        string WarningID = QueryItemID(data.Warning, "WARNING", StationID, "StationID", iTemp);
                        string TestTableName = CreateTestTable(data.PN, Year, Month);
                        string DataTableName = CreateDataTable(TestTableName, data.PN, currectStation, Year, Month);

                        DisplayMsg("[DataSystem][WriteSQLData] --> TestTableName: " + TestTableName);
                        DisplayMsg("[DataSystem][WriteSQLData] --> TestTableName: " + DataTableName);

                        List<string> mylist = new List<string>();

                        Insert(TestTableName, new List<MySqlParameter>() {
                             new MySqlParameter("@PN", data.PN),
                             new MySqlParameter("@IP", data.SFCSIP),
                             new MySqlParameter("@FIXTURE", data.Fixture),
                             new MySqlParameter("@SN", data.SN),
                             new MySqlParameter("@RESULT", data.Result),
                             new MySqlParameter("@TESTTIME", data.TestTime),
                             new MySqlParameter("@GOLDENSIMPLE", goldSample),
                             new MySqlParameter("@FTPLOG", data.FTPLog),
                             new MySqlParameter("@NGITEM", data.NGItem),
                             new MySqlParameter("@ERRORCODE", data.ErrorCode),
                             new MySqlParameter("@STATION", currectStation),
                            new MySqlParameter("@INSFCS", inSFCS),
                             new MySqlParameter("@WARNING", data.Warning),
                             new MySqlParameter("@FIRSTLOAD", (data.FirstTest ? 1 : 0))
                        });

                        DataSet ds = QueryID("max(id)", TestTableName, "Fixture=" + data.Fixture);
                        string GetInsertID = ds.Tables[0].Rows[0].ItemArray[0].ToString();
                        int TestID = Convert.ToInt32(GetInsertID);

                        DataSet columnDS = QueryID("column_name", "information_schema.`COLUMNS`", "table_name=" + DataTableName);

                        for (int i = 0; i < columnDS.Tables[0].Rows.Count; i++)
                        {
                            object[] aa = columnDS.Tables[0].Rows[i].ItemArray;

                            foreach (object a in aa)
                            {
                                mylist.Add(a.ToString().ToUpper());
                            }
                        }

                        string sSql = "alter table " + DataTableName;
                        string ValidData = "";
                        string ValidDataRaw = "";
                        string ValidData_DataLog = "";

                        //  增加欄位(欄位名用測試項目)
                        foreach (StatusUI2.Data sfcs_data in data.ListDataSFCS)
                        {
                            ValidData = Regex.Replace(sfcs_data.TestItem, @"[\W_]+", "_");
                            ValidData_DataLog = Regex.Replace(sfcs_data.TestItem, @"[\W_]+", "_") + "_DataLog";
                            ValidDataRaw = Regex.Replace(sfcs_data.TestItem, @"[\W_]+", "_").ToUpper() + "_Raw";

                            if (sfcs_data.DataType == StatusUI2.Data.data_type.Raw)
                            {
                                if (!mylist.Contains(ValidDataRaw.ToUpper()))
                                {
                                    sSql += " add " + ValidDataRaw + " text(100),";
                                }
                            }
                            else if (sfcs_data.DataType == StatusUI2.Data.data_type.Number)
                            {
                                if (!mylist.Contains(ValidData.ToUpper()))
                                {
                                    sSql += " add " + ValidData + " text(100),";
                                }
                            }
                            else if (sfcs_data.DataType == StatusUI2.Data.data_type.Log)
                            {
                                if (!mylist.Contains(ValidData_DataLog.ToUpper()))
                                {
                                    sSql += " add " + ValidData_DataLog + " text(100),";
                                }
                            }
                        }
                        sSql = sSql.TrimEnd(',');
                        DisplayMsg("[DataSystem][WriteSQLData] --> sSql(alert): " + sSql);

                        MySqlCommand sMYSqlCmd = new MySqlCommand(sSql, _globalVariable.MySqlConn);
                        sMYSqlCmd.ExecuteNonQuery();

                        Insert(DataTableName, new List<MySqlParameter>() {
                             new MySqlParameter("@TESTID", TestID)
                        });

                        //  將測試資料update到剛剛增加的欄位裡
                        if (data.ListDataSFCS.Count > 0)
                        {
                            sSql = "UPDATE " + DataTableName + " SET ";

                            foreach (StatusUI2.Data sfcs_data in data.ListDataSFCS)
                            {
                                ValidData = Regex.Replace(sfcs_data.TestItem, @"[\W_]+", "_");
                                ValidData_DataLog = Regex.Replace(sfcs_data.TestItem, @"[\W_]+", "_") + "_DataLog";
                                ValidDataRaw = Regex.Replace(sfcs_data.TestItem, @"[\W_]+", "_").ToUpper() + "_Raw";

                                if (sfcs_data.DataType == StatusUI2.Data.data_type.Number)
                                {
                                    sSql += ValidData + "='" + sfcs_data.Val + "',";
                                }
                                else if (sfcs_data.DataType == StatusUI2.Data.data_type.Log)
                                {
                                    if (sfcs_data.Status == 0)
                                    {
                                        sSql += ValidData_DataLog + "='PASS',";
                                    }
                                    else
                                    {
                                        sSql += ValidData_DataLog + "='NG',";
                                    }
                                }
                                else if (sfcs_data.DataType == StatusUI2.Data.data_type.Raw)
                                {
                                    sSql += ValidDataRaw + "='" + sfcs_data.Unit + "',";
                                }
                            }

                            sSql = sSql.TrimEnd(',');
                            sSql += " WHERE TESTID=" + TestID;
                            DisplayMsg("[DataSystem][WriteSQLData] --> sSql: " + sSql);
                            sMYSqlCmd = new MySqlCommand(sSql, _globalVariable.MySqlConn);
                            sMYSqlCmd.ExecuteNonQuery();
                        }

                        DisplayMsg("[DataSystem][WriteSQLData] --> Upload data to DataSystem done.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("[DataSystem][WriteSQLData][Exception] --> " + ex.Message);
                }
            }
        }
}
