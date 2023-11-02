using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacMaster.LCS5
{
    internal class SFCSQuery
    {
        public class SFCS_Query
    {
        public string _MAC = string.Empty;

        public string _SSN = string.Empty;

        public string Get_MAC_SSN(string sSMT, int index)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            //string sIP = "172.21.30.208";


            //dr[0] = "select a.prod_ssn as SSN, b.csn as MAC  from sn_csn a, sn_csn b where a.csn='" + sSMT + "' and a.ssn=b.ssn and b.cpn='@MAC'";

            string sSQL = "";

            sSQL += "SELECT prod_ssn, CSN, CPN FROM SN_CSN WHERE SSN IN (SELECT A.SSN FROM ";

            sSQL += "(SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN = PROD_SSN ";
            sSQL += "UNION ";
            sSQL += "SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN = CSN) A ";
            sSQL += "where a.cpn like '%MAC'and a.csn not like '%RWK%') ";

            dr[0] = sSQL;

            dr[1] = "MAC";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;
            if (nCount == 0)
                return "";
            else
            {
                _MAC = dt_back.Rows[index]["CSN"].ToString();
            }
            return _MAC;
        }

        public string Get_MAC_SSN(string sSMT, string sCheckString)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "";

            sSQL += "SELECT prod_ssn, CSN, CPN FROM SN_CSN WHERE SSN IN (SELECT A.SSN FROM ";

            sSQL += "(SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN = PROD_SSN ";
            sSQL += "UNION ";
            sSQL += "SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN = CSN) A ";
            sSQL += "where a.cpn like '%MAC'and a.csn not like '%RWK%') ";

            dr[0] = sSQL;

            dr[1] = "MAC";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;
            if (nCount == 0)
                return "";
            else
            {
                _MAC = "";
                for (int n = 0; n < nCount; n++)
                {
                    string str = dt_back.Rows[n]["CSN"].ToString().Trim();
                    if (str.StartsWith(sCheckString))
                    {
                        _MAC = str;
                        break;
                    }

                }

            }
            return _MAC;
        }

        public string Get_MAC_SSN1(string sSMT, string sCheckString, int nLength)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "";

            sSQL += "SELECT prod_ssn, CSN, CPN FROM SN_CSN WHERE SSN IN (SELECT A.SSN FROM ";

            sSQL += "(SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN = PROD_SSN ";
            sSQL += "UNION ";
            sSQL += "SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN = CSN) A ";
            sSQL += "where a.cpn like '%MAC'and a.csn not like '%RWK%') ";

            dr[0] = sSQL;

            dr[1] = "MAC";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;
            if (nCount == 0)
                return "";
            else
            {
                _MAC = "";
                for (int n = 0; n < nCount; n++)
                {
                    string str = dt_back.Rows[n]["CSN"].ToString().Trim();
                    if (str.StartsWith(sCheckString) && str.Length == nLength)
                    {
                        _MAC = str;
                        break;
                    }

                }

            }
            return _MAC;
        }

        public string GetSSIDFromSFCS(string SN, string Item)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string SQL = "";

            //SQL = SQL + "SELECT VALUE FROM SN_FLEXFIELD WHERE SSN IN (SELECT SSN FROM SN_CSN WHERE CSN=\'" + SN + "\') AND QUALIFIER=\'" + Item + "\'";
            SQL += "SELECT B.VALUE ";
            SQL += "FROM SN_FLEXFIELD B ";
            SQL += "WHERE SSN IN ";
            SQL += "(SELECT DISTINCT A.SSN as SN";
            SQL += "  FROM SN_CSN A";
            SQL += "  START WITH A.SSN = (SELECT SSN";
            SQL += " FROM (SELECT *";
            SQL += " FROM SN_CSN";
            SQL += " START WITH CSN =\'" + SN + "\'";
            SQL += " CONNECT BY PRIOR PROD_SSN = CSN";
            SQL += " ORDER BY CREATED_DATE DESC)";
            SQL += " WHERE ROWNUM = 1)";
            SQL += " CONNECT BY PRIOR A.CSN = A.PROD_SSN)";
            SQL += " AND QUALIFIER =\'" + Item + "\'";

            dr[0] = SQL;

            dr[1] = "CSN";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;

            if (nCount == 0)
                return "mistake";
            else
                Item = dt_back.Rows[0]["VALUE"].ToString();
            //string ss = dt_back.Rows[1]["VALUE"].ToString();
            return Item;
        }

        public void Get_StationByCSN_1(string CSN, ref string CurrStation, ref string NextStation)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            string _Stage = "NONE";
            DataSet set = new DataSet();
            set = ats.getSN_ForATS(CSN, ref _sError);
            DataTable table2 = new DataTable();
            table2 = set.Tables[0];
            if (table2.Rows.Count != 0)
            {
                //_Stage = table2.Rows[0][1].ToString();
                CurrStation = table2.Rows[0][1].ToString();
                NextStation = table2.Rows[0][2].ToString();
            }
            else
            {
                CurrStation = "NONE";
                NextStation = "NONE";
            }

        }

        public void Get_StationByCSN(string CSN, ref string CurrStation, ref string NextStation)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "";
            sSQL = "select curr_stage,next_stage From sn_asse a,sn_csn b " +
                   "where a.ssn=b.ssn "
                   + "and b.csn='" + CSN + "'";

            dr[0] = sSQL;

            dr[1] = "MAC";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            CurrStation = dt_back.Rows[0]["CURR_STAGE"].ToString().Trim().ToUpper();
            NextStation = dt_back.Rows[0]["NEXT_STAGE"].ToString().Trim().ToUpper();
        }

        public void Get_StationByCSN2(string CSN, ref string CurrStation)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "";
            sSQL = "SELECT A.SSN,'" + CSN + "' AS SN_INPUT,A.CURR_STAGE,B.DESCRIPTION" +
            "FROM(SELECT * FROM(SELECT * FROM SN_ASSE WHERE PROD_SSN IN (SELECT A.PROD_SSN" +
            "FROM SN_CSN A START WITH A.CSN = '" + CSN + "' CONNECT BY     PRIOR A.PROD_SSN = A.CSN" +
            "AND INSTR(A.CSN, '=RWK') = 0) AND(IS_REWORK <> 'R') ORDER BY UPDATED_DATE DESC)" +
            "WHERE ROWNUM = 1) A, STAGE B WHERE A.CURR_STAGE = B.STAGE";


            dr[0] = sSQL;

            dr[1] = "CSN";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            CurrStation = dt_back.Rows[0]["DESCRIPTION"].ToString().Trim().ToUpper();
        }

        public string Get_MAC_SSN(string sSMT, string sCheckString, string Start, int nLength)
        {
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "";

            sSQL += "   select csn from sn_csn where ssn in (select ssn from sn_csn start with ssn = (select distinct ssn from SN_CSN where csn = '" + sSMT + "') connect by prior prod_ssn = csn) and cpn = '" + sCheckString + "' and instr('csn', 'RWK') = 0";

            //sSQL += "(SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            //sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            //sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            //sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN = PROD_SSN ";
            //sSQL += "UNION ";
            //sSQL += "SELECT level as lv, SSN, CSN, CPN, PROD_SSN FROM SN_CSN START WITH SSN in ";
            //sSQL += "(select SSN from SN_ASSE where ssn = (select ssn from (select level, A.SSN ";
            //sSQL += "from sn_csn A start with A.csn = '" + sSMT + "' connect by prior A.prod_ssn = A.csn ";
            //sSQL += "order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN = CSN) A ";
            //sSQL += "where a.cpn like '%MAC'and a.csn not like '%RWK%') ";

            dr[0] = sSQL;

            dr[1] = "MAC";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;
            if (nCount == 0)
                return "";
            else
            {
                _MAC = "";
                for (int n = 0; n < nCount; n++)
                {
                    string str = dt_back.Rows[n]["CSN"].ToString().Trim();
                    if (str.StartsWith(Start) && str.Length == nLength)
                    {
                        _MAC = str;
                        break;
                    }

                }

            }
            return _MAC;
        }

        public bool Get15Data(string MAC, string KEY, ref string sMAC_R0)
        {
            try
            {
                string error = "";
                ATS_Template.SFCS_ATS_2_0.ATS _sfcsATS = new ATS_Template.SFCS_ATS_2_0.ATS();

                DataTable _dtTableOut = new DataTable();
                DataSet _dsOut = new DataSet();
                _dsOut = _sfcsATS.getFLEXFIELD_ByCSN(MAC, KEY, ref error);
                _dtTableOut = _dsOut.Tables[0];
                int nCount = _dtTableOut.Rows.Count;
                if (nCount == 0)
                {
                    return false;
                }
                else
                {
                    sMAC_R0 = _dtTableOut.Rows[0]["VALUE"].ToString();
                    return true;
                }
            }
            catch (Exception EX)
            {
                sMAC_R0 = EX.ToString();
                return false;
            }
        }

        public int Get_SSNBy15Line(string s15Line, string sCheckString, string AllPN, int nLength)
        {
            string _sError = "";
            //string[] AllPNs = AllPN.Split(new char[] { ',' });
            string PN = AllPN;

            //for (int i = 0; i < AllPNs.Length; i++)
            //{
            //    if (i == AllPNs.Length - 1)
            //    {
            //        PN += "'" + AllPNs[i] + "'";
            //    }
            //    else
            //    {
            //        PN += "'" + AllPNs[i] + "',";
            //    }
            //}

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "";

            sSQL += "select csn";
            sSQL += " from sn_csn a, sn_flexfield b, spn_tabl c";
            sSQL += " where c.pn in (" + PN;
            sSQL += ") and";
            sSQL += " b.qualifier = '" + sCheckString + "' and value='" + s15Line + "' and substr(b.ssn,1,4)=c.spn and a.cpn like'@%'";
            sSQL += " and a.ssn = b.ssn";


            dr[0] = sSQL;
            //MessageBox.Show(sSQL);
            dr[1] = "CSN";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;
            if (nCount == 0)
                return 0;
            else
            {
                _MAC = "";
                for (int n = 0; n < nCount; n++)
                {
                    string str = dt_back.Rows[n]["CSN"].ToString().Trim();
                    //if (str.StartsWith(Start) && str.Length == nLength)
                    //{
                    _MAC = str;
                    //break;
                    //}

                }

            }
            return nCount;
        }

        public string GetSSNbySMT(string SMT, string Item)
        {
            //string temp=Get_MAC_SSN(SMT,2);
            string _sError = "";

            ATS_Template.SFCS_ATS_2_0.ATS ats = new ATS_Template.SFCS_ATS_2_0.ATS();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("SQL");
            DataColumn dc2 = new DataColumn("FUNC");

            DataRow dr = dt.NewRow();
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            string sSQL = "select c.* from (SELECT ssn FROM SN_CSN a START WITH A.cSN = '" + SMT + "' CONNECT BY PRIOR A.PROD_SSN=A.cSN ) b,sn_csn c where b.ssn=c.ssn";

            sSQL += "   ";

            dr[0] = sSQL;

            dr[1] = "@J60_SN";

            dt.Rows.Add(dr);
            ds.Tables.Add(dt);

            DataSet ds_back = new DataSet();
            ds_back = ats.ASSP_V001(ds, ref _sError);

            DataTable dt_back = new DataTable();
            dt_back = ds_back.Tables[0];

            int nCount = dt_back.Rows.Count;
            if (nCount == 0)
                return "";
            else
            {
                _SSN = "";
                for (int n = 0; n < nCount; n++)
                {
                    string str = dt_back.Rows[n]["CSN"].ToString().Trim();
                    if (str.StartsWith("H") && str.Length == 14)
                    {
                        _SSN = str;
                        break;
                    }

                }

            }
            return _SSN;

        }

        public string GetFromSfcs(string psn, string name)//, out string component)
        {
            string component = string.Empty;

            try
            {
                string SQLstr = "SELECT * FROM(SELECT H.*, G.PN FROM SPN_TABL G JOIN (SELECT * FROM (SELECT A.* FROM (SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START  WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                    + psn + "'  or A.prod_ssn='" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN=PROD_SSN UNION SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                    + psn + "' or a.prod_ssn = '" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN=CSN) A UNION SELECT 99 AS LV, B.SSN, '' AS CSN, '' AS CPN, A.CSN AS PROD_SSN FROM SN_ASSE B JOIN (SELECT * FROM (SELECT A.* FROM (SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                    + psn + "' or A.prod_ssn='" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR CSN=PROD_SSN UNION SELECT level as lv, SSN,CSN,CPN,PROD_SSN FROM SN_CSN START WITH SSN in (select SSN from SN_ASSE where ssn= (select ssn from (select level,A.SSN from sn_csn A start with A.csn='"
                    + psn + "' or A.prod_ssn='" + psn + "' connect by prior A.prod_ssn=A.csn order by level desc) where rownum < 2)) CONNECT BY PRIOR PROD_SSN=CSN) A) WHERE CPN LIKE 'N%') A ON  A.CSN = B.PROD_SSN) ORDER BY LV) H ON G.SPN = SUBSTR(H.SSN,1,4)) WHERE CSN NOT LIKE '%=RWK%' OR CSN IS NULL ";

                string FUNCstr = "LC";
                ATS_Template.SFCS_ATS_2_0.ATS sfcs = new ATS_Template.SFCS_ATS_2_0.ATS();

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
                            return component;
                            //break;
                        }
                    }
                }

                if (component == string.Empty)
                {
                    component = "Dut not have " + name + " on SFCS";
                }
                return component;
            }
            catch (Exception ex)
            {
                component = ex.ToString();
                return component;// = string.Empty;
            }
        }

        public string GetProdSN(string csn, int lenght, ref string error)
        {
            try
            {
                error = "";
                ATS_Template.SFCS_ATS_2_0.ATS sfcs = new ATS_Template.SFCS_ATS_2_0.ATS();

                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                ds = sfcs.getProdSSN_ByCSN(csn, ref error);
                dt = ds.Tables[0];
                int nCount = dt.Rows.Count;
                if (nCount == 0)
                    return "";
                else
                {
                    _MAC = "";
                    for (int n = 0; n < nCount; n++)
                    {
                        string str = dt.Rows[n][1].ToString().Trim();
                        if (str.Length == lenght)
                        {
                            _MAC = str;
                            break;
                        }
                    }

                }
                return _MAC;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
    }
}


