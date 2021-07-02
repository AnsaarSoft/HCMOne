using System;


using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;

using System.Diagnostics;
using System.Threading;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Management;
using Microsoft.CSharp;
using System.Security.Cryptography;
using System.Text;

using System.Reflection;
using System.IO;
using SAPbobsCOM;
using SAPbouiCOM;

using System.Resources;
using DIHRMS;
using mfmLicensing;
using UFFU;


namespace ACHR
{
    public class UDClass
    {
        long v_RetVal;
        int v_ErrCode;
        string v_ErrMsg = "";
        public SAPbobsCOM.Company oCompany;
        public SAPbobsCOM.Company oDiCompany;
        //Mera Kaam..
        string FileName;
        string defFileName;
        public Hashtable StringMessages = new Hashtable();
        public System.Data.DataTable LOVs = new System.Data.DataTable();
        public System.Data.DataTable AllLovs = new System.Data.DataTable();
        public SAPbouiCOM.Application oApplication;
        public dbHRMS dbHr;
        public string HRMSDbName = "";
        public string HRMSDbServer = "";
        public string HRMSDBuid = "";
        public string HRMSDbPwd = "";
        public string HRMSLicHash = "";
        public string hrConstr = "";
        public string HRMServerType = "";
        public string BranchName = "";
        public string JeSeries = "";
        public bool CalculateTax = false;
        public bool isDIConnected = false;
        public string showReportCode = "";
        public bool isSystemReport = false;
        public string rptCritaria = "";
        public string rptDateParameter = "";
        public string rptCP = "";
        public string B1ClientName = "";
        public bool isSuperUser = false;
        public string EmployeeFilterValues = "";
        public string salarySlipIDs = "";
        //MFM Build....
        public mFm logger = null;
        private const string HKey = "mfm11khi04RA2012";

        public string AppVersion = "930.03.84";
        private const string DBVersion = "1026";

        public void KillPreviousProcess(string processName = "ACHR")
        {
            if (!string.IsNullOrWhiteSpace(processName))
            {
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var processList = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (var process in processList)
                {
                    if (!(currentProcess.Id == process.Id))
                    {
                        process.Kill();
                    }
                }
            }
        }

        public UDClass(string connectString)
        {
            string errmsg = "";
            try
            {
                SboGuiApi sboApi = new SboGuiApi();
                //MessageBox.Show("after new sboguiapi");

                if (connectString == "")
                {
                    MessageBox.Show("Add-on must be run from SAP Business One.");
                }

                sboApi.Connect(connectString);
                //MessageBox.Show("the value in connection" + connectString);
                oApplication = sboApi.GetApplication();
                
                //MessageBox.Show("sboapi.getapplication success.");
                //oApplication.StatusBar.SetText("getdicompany", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                oCompany = oApplication.Company.GetDICompany();
                
                //oApplication.StatusBar.SetText("get dicompany success", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                // oCompany = new SAPbobsCOM.Company();
                //string sCookies = oCompany.GetContextCookie();
                //string conStr = oApplication.Company.GetConnectionContext(sCookies);
                //int ret = oCompany.SetSboLoginContext(conStr);
                int ret2 = 0; //oCompany.Connect();
                //int ret2 = oCompany.Connect();
                if (ret2 == 0)
                {
                    //oApplication.StatusBar.SetText("inside the check", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    oApplication.StatusBar.SetText("Addon Payroll Connected Successfully.!", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    oApplication.MenuEvent += new _IApplicationEvents_MenuEventEventHandler(oApplication_MenuEvent);
                    oApplication.AppEvent += new _IApplicationEvents_AppEventEventHandler(oApplication_appEvent);
                    string newTitle = oApplication.Desktop.Title;
                    Process oProcess = Process.GetCurrentProcess();
                    B1ClientName = newTitle + " " + oProcess.Id.ToString();
                    oApplication.Desktop.Title = B1ClientName;
                    // oApplication.FormDataEvent +=new _IApplicationEvents_FormDataEventEventHandler(oApplication_FormDataEvent);
                    logger = new mFm(System.Windows.Forms.Application.StartupPath, true, false);
                    string lang = oApplication.Language.ToString();
                    Program.sboLanguage = lang;
                    try
                    {
                        int langnum = Convert.ToInt16(oApplication.Language.ToString());
                        lang = "_" + lang;
                    }
                    catch
                    {

                    }

                    if (lang.Contains("English"))
                    {
                        lang = "ln_English";
                    }
                    createConfigTbl();
                    string strCon = HrmsConstr();
                    Program.ConStrHRMS = strCon;
                    Program.sboLanguage = lang;
                    loadMenu(lang);
                    Program.sboLanguage = lang;


                    try
                    {
                        ResXResourceReader rsxr = new ResXResourceReader("Msgs.res");
                        IDictionaryEnumerator id = rsxr.GetEnumerator();
                        StringMessages.Clear();
                        // Iterate through the resources and display the contents to the console.
                        foreach (DictionaryEntry d in rsxr)
                        {
                            StringMessages.Add(d.Key.ToString(), d.Value.ToString());
                        }

                        regSBOFms();
                        dbHr = new dbHRMS(strCon);
                        Program.systemInfo = (from p in dbHr.CfgPayrollBasicInitialization where p.Id == 1 select p).FirstOrDefault();
                        //Here App Version Check Will Held.
                        mfmAppVersioning();
                        if (Program.systemInfo.SAPB1Integration == true)
                        {
                            string companyDb = oCompany.CompanyDB;
                            string UserName = Program.systemInfo.SboUID;
                            string Password = Program.systemInfo.SboPwd;
                            string DBUserName = oCompany.DbUserName;
                            string DbPassword = oCompany.DbPassword;
                            oDiCompany = new SAPbobsCOM.Company();
                            oDiCompany.CompanyDB = companyDb;
                            oDiCompany.UserName = UserName;
                            oDiCompany.Password = Password;
                            oDiCompany.DbUserName = HRMSDBuid;
                            oDiCompany.DbPassword = HRMSDbPwd;
                            //Program.oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                            if (HRMServerType.Trim() == "2005")
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                            }
                            else if (HRMServerType.Trim() == "2008")
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                            }
                            else if (HRMServerType.Trim() == "2012")
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                            }
                            else if (HRMServerType.Trim().ToUpper() == "2014")
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                            }
                            else if (HRMServerType.Trim().ToUpper() == "2016")
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
                            }
                            else if (HRMServerType.Trim().ToUpper() == "HANA")
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
                            }
                            else
                            {
                                oDiCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                            }
                            oDiCompany.Server = HRMSDbServer;
                            //Try to connect
                            int lRetCode = 0; // oDiCompany.Connect();
                            oDiCompany = oApplication.Company.GetDICompany();
                            if (oDiCompany.Connected)
                            {
                                lRetCode = 0;
                            }
                            else
                            {
                                lRetCode = oApplication.Company.GetDICompany();
                            }

                            int errCode = 0;
                            string errMsg = "";
                            if (lRetCode != 0) // if the connection failed
                            {
                                oDiCompany.GetLastError(out errCode, out errMsg);
                                MessageBox.Show("Unable to connect for intigration!\n" + errMsg);
                                isDIConnected = false;
                            }
                            else
                            {
                                oApplication.SetStatusBarMessage("Connected to DI for intigration!", BoMessageTime.bmt_Short, false);
                                isDIConnected = true;
                            }
                        }
                        GetLogInUserStatus();
                    }
                    catch (Exception ex)
                    {
                        oApplication.SetStatusBarMessage(ex.Message + " " + errmsg);
                    }

                    // oApplication.SetStatusBarMessage("Applying Patch", BoMessageTime.bmt_Short, false);
                    // applyPatch("37");
                    try
                    {
                        applyAuthorization2nd();
                    }
                    catch (Exception ex) { oApplication.SetStatusBarMessage(ex.Message); }
                }

                else
                {
                    MessageBox.Show(oCompany.GetLastErrorDescription());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect company!" + ex.Message);

                Environment.Exit(0);
            }

        }

        private void loadMenu(string lang)
        {
            SAPbouiCOM.Menus mnus = oApplication.Menus;
            if (mnus.Exists("hcmone_H1"))
            {
                mnus.RemoveEx("hcmone_H1");
            }
            string strMenuFile = "";
            if (Program.AppType == "PN")
            {
                if (DateTime.Now.Date >= Program.StartTime && DateTime.Now.Date <= Program.EndTime)
                {
                    strMenuFile = "ACHR.XMLScreen." + lang + ".PayrollMenu.xml";
                    mfmLicWarnings();
                }
                else
                {
                    oApplication.SetStatusBarMessage("You Can't Use this Addon Contact AbacusConsulting for License. support.sapb1@abacus-global.com", BoMessageTime.bmt_Short, true);
                    strMenuFile = "ACHR.XMLScreen." + lang + ".PayrollMenuNoLic.xml";
                }
            }
            else if (Program.AppType == "PH")
            {
                if (DateTime.Now.Date >= Program.StartTime && DateTime.Now.Date <= Program.EndTime)
                {
                    strMenuFile = "ACHR.XMLScreen." + lang + ".PayrollMenuHR.xml";
                    mfmLicWarnings();
                }
                else
                {
                    oApplication.SetStatusBarMessage("You Can't Use this Addon Contact AbacusConsulting for License. support.sapb1@abacus-global.com", BoMessageTime.bmt_Short, true);
                    strMenuFile = "ACHR.XMLScreen." + lang + ".PayrollMenuNoLic.xml";
                }
            }
            else
            {
                oApplication.SetStatusBarMessage("You Can't Use this Addon Contact AbacusConsulting for License. support.sapb1@abacus-global.com", BoMessageTime.bmt_Short, true);
                strMenuFile = "ACHR.XMLScreen." + lang + ".PayrollMenuNoLic.xml";
            }

            LoadMenuFromXML(strMenuFile, "");
        }

        private void oApplication_appEvent(BoAppEventTypes EventType)
        {

            switch (EventType)
            {
                case BoAppEventTypes.aet_CompanyChanged:
                    System.Windows.Forms.Application.Exit();
                    break;
                case BoAppEventTypes.aet_ShutDown:
                    System.Windows.Forms.Application.Exit();
                    break;
                //case BoAppEventTypes.aet_ServerTerminition:
                //    System.Windows.Forms.Application.Exit();
                //    break;
                case BoAppEventTypes.aet_LanguageChanged:
                    string lang = oApplication.Language.ToString();
                    Program.sboLanguage = lang;
                    try
                    {
                        int langnum = Convert.ToInt16(oApplication.Language.ToString());
                        lang = "_" + lang;
                    }
                    catch
                    {

                    }
                    loadMenu(lang);
                    applyAuthorization2nd();
                    break;
            }
        }

        public string getAcctName(string strAcctCode)
        {
            string strOut = "";

            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

            //oRecSet.DoQuery("select acctname from oact where oact.acctcode='" + strAcctCode + "'"); //Old Normal
            oRecSet.DoQuery("select \"AcctName\" from OACT where \"AcctCode\"='" + strAcctCode + "'"); //For hana & Normal
            if (oRecSet.EoF)
            {
                strOut = "Not Found";
                return strOut;
            }
            strOut = oRecSet.Fields.Item("AcctName").Value;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecSet);
            oRecSet = null;


            return strOut;
        }

        public string HrmsConstr()
        {
            string strOut = "";
            try
            {
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT *  FROM \"@ACHR_CONFIG\" Where \"Code\" = 'CN16'");
                //oRecSet.DoQuery("SELECT *  FROM \"@ACHR_CONFIG\"");
                if (oRecSet.EoF)
                {
                    oApplication.StatusBar.SetText("Configure HRMS before further proceeding!");
                    return strOut;
                }
                strOut = oRecSet.Fields.Item("u_server").Value;
                strOut = "Data Source=" + oRecSet.Fields.Item("u_server").Value + ";Initial Catalog=" + oRecSet.Fields.Item("U_db").Value + ";User ID=" + oRecSet.Fields.Item("U_uid").Value + ";Password=" + oRecSet.Fields.Item("U_pwd").Value + ";" + "MultipleActiveResultSets=True";
                //  HRMSDbName = "[" + oRecSet.Fields.Item("U_db").Value + "]";
                HRMSDbName = oRecSet.Fields.Item("U_db").Value;
                HRMSDbServer = oRecSet.Fields.Item("U_server").Value;
                HRMSDBuid = oRecSet.Fields.Item("U_uid").Value;
                HRMSDbPwd = oRecSet.Fields.Item("U_pwd").Value;
                HRMServerType = oRecSet.Fields.Item("U_SvrType").Value;
                HRMSLicHash = oRecSet.Fields.Item("U_LicKey").Value;
                BranchName = oRecSet.Fields.Item("U_BranchName").Value;
                JeSeries = oRecSet.Fields.Item("U_JES").Value;
                if (!string.IsNullOrEmpty(HRMSLicHash))
                {
                    Program.HashKey = HRMSLicHash;
                }
                oRecSet = null;
                hrConstr = strOut;
                mfmLICBreakUP();

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("HrmsConstr Function Exception Error : " + Ex.Message, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);
                strOut = "";
            }
            return strOut;
        }

        public void createConfigTbl()
        {


            //addQuery("select * from " + HRMSDbName + ".dbo.mstElements", "Elements");
            try
            {
                AddTable("ACHR_CONFIG", "HRMS Configuration", SAPbobsCOM.BoUTBTableType.bott_NoObject);
                AddColumns("@ACHR_CONFIG", "server", "Server", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "uid", "Login ID", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "pwd", "Password", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "db", "HRMS DB", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "SvrType", "Server Type", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "LicKey", "License Key", SAPbobsCOM.BoFieldTypes.db_Alpha, 250, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "BranchName", "Branch Name", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("@ACHR_CONFIG", "JES", "JE Series", SAPbobsCOM.BoFieldTypes.db_Alpha, 50, SAPbobsCOM.BoFldSubTypes.st_None, "");

                AddColumns("OHEM", "HrmsEmpId", "HRMS Employee ID", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("OUSR", "PayrollType", "Payroll Type", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("OUSR", "SuperUser", "Super User", SAPbobsCOM.BoFieldTypes.db_Alpha, 10, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("JDT1", "EmpCode", "HRMS Employee ID", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("JDT1", "EmpName", "HRMS Employee Name", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("OITM", "PerPieceItem", "PerPiece Item", SAPbobsCOM.BoFieldTypes.db_Alpha, 2, SAPbobsCOM.BoFldSubTypes.st_None, "");

                AddColumns("RCT4", "DocNumber", "DocNumber", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("RCT4", "EmpID", "Employee ID", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("RCT4", "Type", "Type", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
                AddColumns("RCT4", "Installment", "Installment", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, SAPbobsCOM.BoFldSubTypes.st_None, "");
            }
            catch
            {
                oApplication.StatusBar.SetText("Didn't create UDT & UDF successfully. Contact SAP Support to manaully create them.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }

        }

        public void regSBOFms()
        {
            addQryCategor("HRMS Payroll");
            addFms("frm_PayrollSetup", "txCost", "-1", "select prcCode,prcName from oprc");
            addFms("frm_ApprStage", "mtAuth", "cUsr", "SELECT   t0.UserID, t1.DepartmentName FROM     " + HRMSDbName + ".dbo.MstUsers t0 INNER JOIN " + HRMSDbName + ".dbo.MstEmployee t1 ON t0.ID = t1.UserCode");
            addFms("frm_ApprTamp", "mtOrig", "cUsr", "SELECT  t0.UserID, t1.DepartmentName FROM     " + HRMSDbName + ".dbo.MstUsers t0 INNER JOIN " + HRMSDbName + ".dbo.MstEmployee t1 ON t0.ID = t1.UserCode");
            addFms("frm_ApprTamp", "mtStages", "cStage", "SELECT     StageName, StageDescription, ApprovalsNo, RejectionsNo FROM  " + HRMSDbName + ".dbo.CfgApprovalStage");
            addFms("frm_GLAcctDetLoc", "txLocation", "-1", "select Name,Description from " + HRMSDbName + ".dbo.mstlocation");
            addFms("frm_GLAcctDetDept", "txDept", "-1", "select Code,DeptName from " + HRMSDbName + ".dbo.MstDepartment");
            addFms("frm_EmpElem", "mtElement", "Element", "select Description , ElementName from " + HRMSDbName + ".dbo.mstElements where ElementName = $[dtHead.prId]");
            addFms("frm_empOverTime", "mtOT", "Code", "SELECT     Code,Description FROM      " + HRMSDbName + ".dbo.MstOverTime");
            addFms("frm_RetroSet", "mtElement", "Code", "select   ElementName,Description from " + HRMSDbName + ".dbo.mstElements ");
            addFms("frm_BtchCrea", "txElCode", "-1", "select   ElementName,Description from " + HRMSDbName + ".dbo.mstElements");
            addFms("frm_EmpElem", "txHRMSId", "-1", "SELECT     EmpID, FirstName, MiddleName, LastName FROM " + HRMSDbName + ".dbo.MstEmployee");
            //MFM Addition
            //***************



            //***************
        }

        private void applyAuthorization()
        {
            string UserCode = "";
            if (Program.systemInfo.TaxSetup == false)
            {
                SAPbouiCOM.Menus mnus = oApplication.Menus;
                UserCode = Convert.ToString(oCompany.UserSignature);
                if (!string.IsNullOrEmpty(UserCode))
                {
                    var EmployeeID = dbHr.MstEmployee.Where(e => e.SBOEmpCode == UserCode).FirstOrDefault();
                    if (EmployeeID != null)
                    {
                        var UserId = dbHr.MstUsers.Where(u => u.Empid == EmployeeID.ID).FirstOrDefault();
                        if (UserId != null)
                        {
                            var RemoveMenus = dbHr.MstUsersAuth.Where(a => a.UserID == UserId.ID && a.UserRights == "0").ToList();
                            if (RemoveMenus != null && RemoveMenus.Count > 0)
                            {
                                int i = 0;
                                foreach (var v in RemoveMenus)
                                {
                                    mnus.RemoveEx(v.MstUserFunctions.MenuID);
                                    i += 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SAPbouiCOM.Menus mnus = oApplication.Menus;
                UserCode = Convert.ToString(oCompany.UserSignature);
                if (!string.IsNullOrEmpty(UserCode))
                {
                    var EmployeeID = dbHr.MstEmployee.Where(e => e.SBOEmpCode == UserCode).FirstOrDefault();
                    if (EmployeeID != null)
                    {
                        var UserId = dbHr.MstUsers.Where(u => u.Empid == EmployeeID.ID).FirstOrDefault();
                        if (UserId != null)
                        {
                            var RemoveMenus = dbHr.MstUsersAuth.Where(a => a.UserID == UserId.ID && a.UserRights == "0").ToList();
                            if (RemoveMenus != null && RemoveMenus.Count > 0)
                            {
                                int i = 0;
                                foreach (var v in RemoveMenus)
                                {
                                    mnus.RemoveEx(v.MstUserFunctions.MenuID);
                                    i += 1;
                                }
                            }
                        }
                    }
                }
            }

        }

        private void applyAuthorization2nd()
        {
            string UserCode = "";
            if (Program.systemInfo.TaxSetup == false)
            {
                SAPbouiCOM.Menus mnus = oApplication.Menus;
                UserCode = Convert.ToString(oCompany.UserSignature);
                int intUserID = Convert.ToInt32(UserCode);
                if (!string.IsNullOrEmpty(UserCode))
                {
                    var RemoveMenus = dbHr.MstUsersAuth.Where(a => a.UserID == intUserID && a.UserRights == "0").ToList();
                    if (RemoveMenus != null && RemoveMenus.Count > 0)
                    {
                        int i = 0;
                        foreach (var v in RemoveMenus)
                        {
                            mnus.RemoveEx(v.MstUserFunctions.MenuID.Trim());
                            i += 1;
                        }
                    }
                }
            }
            else
            {
                SAPbouiCOM.Menus mnus = oApplication.Menus;
                UserCode = Convert.ToString(oCompany.UserSignature);
                int intUserID = Convert.ToInt32(UserCode);
                if (!string.IsNullOrEmpty(UserCode))
                {
                    var RemoveMenus = dbHr.MstUsersAuth.Where(a => a.UserID == intUserID && a.UserRights == "0").ToList();
                    if (RemoveMenus != null && RemoveMenus.Count > 0)
                    {
                        int i = 0;
                        foreach (var v in RemoveMenus)
                        {
                            mnus.RemoveEx(v.MstUserFunctions.MenuID.Trim());
                            i += 1;
                        }
                    }
                }
            }

        }

        public string getStrMsg(string strKey)
        {
            string outStr = "Un-Known Message";

            try
            {
                outStr = StringMessages[strKey].ToString();
            }
            catch { }

            return outStr;
        }

        public void oApplication_FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, out bool BubbleEvent)
        {
            BubbleEvent = true;
            oApplication.SetStatusBarMessage("Form data event fired");
        }

        public void oApplication_MenuEvent(ref MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            string mnuFrm = pVal.MenuUID;
            if (mnuFrm.Substring(0, 3) != "mnu") return;
            if (DateTime.Now.Date >= Program.StartTime && DateTime.Now.Date <= Program.EndTime)
            {

            }
            else
            {
                oApplication.StatusBar.SetText("Add-On Expired Contact AbacusConsultings @ support.sapb1@abacus-global.com", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return;

            }
            try
            {
                if (!pVal.BeforeAction)
                {
                    string strLang = oApplication.Language.ToString();
                    try
                    {
                        int langnum = Convert.ToInt16(oApplication.Language.ToString());
                        strLang = "_" + strLang;
                    }
                    catch
                    {

                    }
                    if (strLang.Contains("English"))
                    {
                        strLang = "ln_English";
                    }
                    string comName = pVal.MenuUID.Replace("mnu_", "");
                    try
                    {
                        oApplication.Forms.Item("frm_" + comName).Select();
                    }
                    catch
                    {
                        mfmLicWarnings();
                        Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                        Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                        objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);

                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Unhandeled Exception Caught at General Class:" + ex.Message);
            }
        }

        public UDClass(ref SAPbobsCOM.Company comp, ref SAPbouiCOM.Application app)
        {
            oCompany = comp;
            oApplication = app;
        }

        public bool ColumnExists(string TableName, string FieldID)
        {
            bool oFlag = true;
            try
            {
                SAPbobsCOM.Recordset rsetField = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                //string s = "Select 1 from [CUFD] Where TableID='" + TableName.Trim() + "' and AliasID='" + FieldID.Trim() + "'"; // Old Normal
                //string s = "Select 1 from CUFD Where \"TableID\"='" + TableName.Trim() + "' and \"AliasID\"='" + FieldID.Trim() + "'"; // Hana & Normal
                string s = "SELECT 1 FROM \"CUFD\" WHERE \"TableID\" = '" + TableName.Trim() + "' AND \"AliasID\" = '" + FieldID.Trim() + "'"; // Hana & Normal verified.
                rsetField.DoQuery(s);
                if (rsetField.EoF)
                    oFlag = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rsetField);
                rsetField = null;
                GC.Collect();
                return oFlag;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to Column Exists : " + ex.Message);
            }
            finally
            {
            }
            return oFlag;
        }

        public void ExecQuery(string sql, string CallerRef)
        {
            SAPbobsCOM.Recordset rs = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            try
            {
                rs.DoQuery(sql);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed in Exec Query on " + CallerRef + " : " + ex.Message);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                rs = null;

            }
        }

        public void ExecFileQuery(string filePath, string callerRef)
        {

            try
            {

                System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
                StreamReader reader = new StreamReader(stream);

                string strSql = reader.ReadToEnd();

                ExecQuery(strSql, callerRef);


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to execute pat" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            finally
            {
            }
        }

        public bool AddColumns(string TableName, string Name, string Description, SAPbobsCOM.BoFieldTypes Type, int Size = 0, SAPbobsCOM.BoFldSubTypes SubType = SAPbobsCOM.BoFldSubTypes.st_None, string LinkedTable = "", string[,] LOV = null, string DefV = "")
        {
            bool outResult = false;
            try
            {
                SAPbobsCOM.UserFieldsMD v_UserField = default(SAPbobsCOM.UserFieldsMD);

                if (TableName.StartsWith("@") == true)
                {
                    if (!ColumnExists(TableName, Name))
                    {
                        v_UserField = (SAPbobsCOM.UserFieldsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                        v_UserField.TableName = TableName;
                        v_UserField.Name = Name;
                        if (!string.IsNullOrEmpty(DefV))
                        {
                            v_UserField.DefaultValue = DefV;
                        }

                        if (LOV == null)
                        {
                        }
                        else
                        {
                            for (int k = 0; k <= LOV.Length - 1; k++)
                            {
                                v_UserField.ValidValues.Value = LOV[k, 0];
                                v_UserField.ValidValues.Value = LOV[k, 1];
                                v_UserField.ValidValues.Add();
                            }

                        }

                        v_UserField.Description = Description;
                        v_UserField.Type = Type;
                        if (Type != SAPbobsCOM.BoFieldTypes.db_Date)
                        {
                            if (Size != 0)
                            {
                                v_UserField.Size = Convert.ToInt16(Size);
                                v_UserField.EditSize = Convert.ToInt16(Size);
                            }
                        }
                        if (SubType != SAPbobsCOM.BoFldSubTypes.st_None)
                        {
                            v_UserField.SubType = SubType;
                        }
                        if (!string.IsNullOrEmpty(LinkedTable))
                            v_UserField.LinkedTable = LinkedTable;
                        v_RetVal = v_UserField.Add();
                        if (v_RetVal != 0)
                        {
                            oCompany.GetLastError(out v_ErrCode, out v_ErrMsg);
                            oApplication.StatusBar.SetText("Failed to add UserField " + Description + " - " + v_ErrCode + " " + v_ErrMsg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("[@" + TableName + "] - " + Description + " added successfully!!!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            outResult = true;
                            return true;
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(v_UserField);
                        v_UserField = null;
                    }
                    else
                    {
                        return false;
                    }
                }


                if (TableName.StartsWith("@") == false)
                {
                    if (!UDFExists(TableName, Name))
                    {
                        v_UserField = (SAPbobsCOM.UserFieldsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                        v_UserField.TableName = TableName;
                        v_UserField.Name = Name;
                        if (!string.IsNullOrEmpty(DefV))
                        {
                            v_UserField.DefaultValue = DefV;
                        }

                        if (LOV == null)
                        {
                        }
                        else
                        {
                            for (int k = 0; k <= LOV.Length / 2 - 1; k++)
                            {
                                v_UserField.ValidValues.Value = LOV[k, 0];
                                v_UserField.ValidValues.Description = LOV[k, 1];
                                v_UserField.ValidValues.Add();
                            }

                        }
                        v_UserField.Description = Description;
                        v_UserField.Type = Type;
                        if (Type != SAPbobsCOM.BoFieldTypes.db_Date)
                        {
                            if (Size != 0)
                            {
                                v_UserField.Size = Size;
                                v_UserField.EditSize = Size;
                            }
                        }
                        if (SubType != SAPbobsCOM.BoFldSubTypes.st_None)
                        {
                            v_UserField.SubType = SubType;
                        }
                        if (!string.IsNullOrEmpty(LinkedTable))
                            v_UserField.LinkedTable = LinkedTable;
                        v_RetVal = v_UserField.Add();
                        if (v_RetVal != 0)
                        {
                            oCompany.GetLastError(out v_ErrCode, out v_ErrMsg);
                            oApplication.StatusBar.SetText("Failed to add UserField " + Description + " - " + v_ErrCode + " " + v_ErrMsg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("[@" + TableName + "] - " + Description + " added successfully!!!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            outResult = true;
                            return true;
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(v_UserField);
                        v_UserField = null;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to Add Columns : " + ex.Message);
            }
            finally
            {
            }
            return outResult;
        }

        public void AddXML(string pathstr)
        {
            try
            {
                System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
                System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(pathstr);
                System.IO.StreamReader streamreader = new System.IO.StreamReader(stream, true);
                xmldoc.LoadXml(streamreader.ReadToEnd());
                streamreader.Close();
                oApplication.LoadBatchActions(xmldoc.InnerXml);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to Load XML,AddXMl Method Failed" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            finally
            {
            }
        }

        public void CopyStream(ref Stream input, ref Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8193];
            int bytesRead = 1;
            while ((bytesRead > 0))
            {
                bytesRead = input.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }

            }
        }

        public void DownloadEmbFile(string pathstr)
        {
            try
            {
                string strFileName = SaveFile(pathstr);


                if (!string.IsNullOrEmpty(strFileName))
                {

                    System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(pathstr);

                    byte[] buf = new byte[stream.Length + 1];
                    stream.Read(buf, 0, buf.Length);
                    File.WriteAllBytes(strFileName, buf);
                    oApplication.MessageBox("File saved successfully !");
                    //streamwriter.WriteLine(streamreader.ReadLine())
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to Load XML,AddXMl Method Failed" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            finally
            {
            }
        }

        public bool UDOExists(string code)
        {
            bool outResult = false;
            try
            {
                SAPbobsCOM.UserObjectsMD v_UDOMD = default(SAPbobsCOM.UserObjectsMD);
                bool v_ReturnCode = false;

                GC.Collect();
                v_UDOMD = (SAPbobsCOM.UserObjectsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                v_ReturnCode = v_UDOMD.GetByKey(code);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(v_UDOMD);
                v_UDOMD = null;
                outResult = v_ReturnCode;
                return v_ReturnCode;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to UDO Exists : " + ex.Message);
            }
            finally
            {
            }
            return outResult;
        }

        public bool registerUDO(string UDOCode, string UDOName, SAPbobsCOM.BoUDOObjType UDOType, string[,] findAliasNDescription, string parentTableName, string childTable1 = "", string childTable2 = "", string childTable3 = "", string childTable4 = "", SAPbobsCOM.BoYesNoEnum LogOption = SAPbobsCOM.BoYesNoEnum.tNO, string MenuId = "", int parrentId = 0)
        {
            bool functionReturnValue = false;

            try
            {
                bool actionSuccess = false;
                SAPbobsCOM.UserObjectsMD v_udoMD = default(SAPbobsCOM.UserObjectsMD);

                functionReturnValue = false;
                v_udoMD = (SAPbobsCOM.UserObjectsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                v_udoMD.CanCancel = SAPbobsCOM.BoYesNoEnum.tYES;
                v_udoMD.CanClose = SAPbobsCOM.BoYesNoEnum.tYES;
                v_udoMD.CanCreateDefaultForm = SAPbobsCOM.BoYesNoEnum.tNO;
                v_udoMD.CanDelete = SAPbobsCOM.BoYesNoEnum.tYES;
                v_udoMD.CanFind = SAPbobsCOM.BoYesNoEnum.tYES;
                v_udoMD.CanLog = SAPbobsCOM.BoYesNoEnum.tNO;
                v_udoMD.CanYearTransfer = SAPbobsCOM.BoYesNoEnum.tYES;
                v_udoMD.ManageSeries = SAPbobsCOM.BoYesNoEnum.tYES;
                if (!string.IsNullOrEmpty(MenuId))
                {
                    v_udoMD.CanCreateDefaultForm = SAPbobsCOM.BoYesNoEnum.tYES;
                    //v_udoMD.RebuildEnhancedForm = SAPbobsCOM.BoYesNoEnum.tYES
                    v_udoMD.MenuItem = SAPbobsCOM.BoYesNoEnum.tYES;

                    v_udoMD.MenuUID = MenuId;
                    v_udoMD.MenuCaption = UDOName;
                    // v_udoMD.EnableEnhancedForm = SAPbobsCOM.BoYesNoEnum.tYES
                    v_udoMD.FatherMenuID = parrentId;
                    v_udoMD.Position = 2;
                }

                v_udoMD.Code = UDOCode;
                v_udoMD.Name = UDOName;
                v_udoMD.TableName = parentTableName;
                if (LogOption == SAPbobsCOM.BoYesNoEnum.tYES)
                {
                    v_udoMD.CanLog = SAPbobsCOM.BoYesNoEnum.tYES;
                    v_udoMD.LogTableName = "A" + parentTableName;
                }
                v_udoMD.ObjectType = UDOType;
                for (Int16 i = 0; i <= findAliasNDescription.GetLength(0) - 1; i++)
                {
                    if (i > 0)
                        v_udoMD.FindColumns.Add();
                    v_udoMD.FindColumns.ColumnAlias = findAliasNDescription[i, 0];
                    v_udoMD.FindColumns.ColumnDescription = findAliasNDescription[i, 1];
                }
                if (!string.IsNullOrEmpty(childTable1))
                {
                    v_udoMD.ChildTables.TableName = childTable1;
                    v_udoMD.ChildTables.Add();
                }
                if (!string.IsNullOrEmpty(childTable2))
                {
                    v_udoMD.ChildTables.TableName = childTable2;
                    v_udoMD.ChildTables.Add();
                }
                if (!string.IsNullOrEmpty(childTable3))
                {
                    v_udoMD.ChildTables.TableName = childTable3;
                    v_udoMD.ChildTables.Add();
                }
                if (!string.IsNullOrEmpty(childTable4))
                {
                    v_udoMD.ChildTables.TableName = childTable4;
                    v_udoMD.ChildTables.Add();
                }
                if (v_udoMD.Add() == 0)
                {
                    functionReturnValue = true;
                    oApplication.StatusBar.SetText("Successfully Registered UDO >" + UDOCode.ToString() + ">" + UDOName.ToString() + " >" + oCompany.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    oApplication.StatusBar.SetText("Failed to Register UDO >" + UDOCode + ">" + UDOName + " >" + oCompany.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    functionReturnValue = false;
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(v_udoMD);
                v_udoMD = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to UDO Register : " + ex.Message);
            }
            finally
            {
            }
            return functionReturnValue;
        }

        public bool TableExists(string TableName)
        {
            bool outResult = false;
            try
            {
                SAPbobsCOM.UserTablesMD oTables = default(SAPbobsCOM.UserTablesMD);
                bool oFlag = false;

                oTables = (SAPbobsCOM.UserTablesMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
                oFlag = oTables.GetByKey(TableName);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oTables);
                outResult = oFlag;
                return oFlag;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to Table Exists : " + ex.Message);
            }
            finally
            {
            }
            return outResult;
        }

        public void addQryCategor(string catName)
        {
            try
            {
                SAPbobsCOM.QueryCategories qCat = default(SAPbobsCOM.QueryCategories);
                qCat = (SAPbobsCOM.QueryCategories)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQueryCategories);
                qCat.Name = catName;
                qCat.Add();

            }
            catch { }


        }

        public int addQuery(string strQuery, string queryName)
        {
            int queryId = 0;
            int catId = 0;

            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            //oRecSet.DoQuery("select CategoryId from OQCN where CatName='HRMS Payroll'"); // Old Normal
            oRecSet.DoQuery("select \"CategoryId\" from OQCN where \"CatName\"='HRMS Payroll'"); // Hana & Normal
            if (!oRecSet.EoF)
            {
                catId = oRecSet.Fields.Item("CategoryId").Value;


            }
            else
            {
                addQryCategor("HRMS Payroll");
                //oRecSet.DoQuery("select CategoryId from OQCN where CatName='HRMS Payroll'"); // Old Normal
                oRecSet.DoQuery("select \"CategoryId\" from OQCN where \"CatName\" = 'HRMS Payroll'"); // Hana & Normal
                if (!oRecSet.EoF)
                {
                    catId = oRecSet.Fields.Item("CategoryId").Value;
                }
            }

            //oRecSet.DoQuery("select intrnalKey as qId from ouqr where QName ='" + queryName + "'"); // Old Normal
            oRecSet.DoQuery("select \"IntrnalKey\" as qId from ouqr where \"QName\" ='" + queryName + "'"); // Hana & Normal
            if (!oRecSet.EoF)
            {
                queryId = Convert.ToInt32(oRecSet.Fields.Item("qId").Value);
            }
            else
            {
                //oRecSet.DoQuery("select isnull(max(intrnalKey),0) +1 as newId from ouqr"); // Old Normal
                oRecSet.DoQuery("select max(\"IntrnalKey\") +1 as newId from ouqr"); // Hana & Normal
                queryId = Convert.ToInt32(oRecSet.Fields.Item("newId").Value);
                //string sQuery = " insert into ouqr ([IntrnalKey] ,[QCategory] ,[QName] ,[QString] ,[QType] ) "; // Old Normal
                string sQuery = " insert into ouqr (\"IntrnalKey\" ,\"QCategory\" ,\"QName\" ,\"QString\" ,\"QType\" ) "; // Hana & Normal
                sQuery += " values ('" + queryId.ToString() + "','" + catId.ToString() + "','" + queryName + "','" + strQuery + "','W')";
                oRecSet.DoQuery(sQuery);
            }
            oRecSet = null;

            return queryId;
        }

        public void addFms(string frmId, string itmId, string colID, string query)
        {

            int queryId = 0;
            int fmsId = 0;

            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                ////oRecSet.DoQuery("select QueryId,IndexID from CSHS where formId='" + frmId + "' and ItemId='" + itmId + "' and colID='" + colID + "'"); // Old Normal
                //oRecSet.DoQuery("select \"QueryId\",\"IndexID\" from CSHS where \"FormID\"='" + frmId + "' and \"ItemID\"='" + itmId + "' and \"ColID\"='" + colID + "'"); // Hana & Normal
                //if (!oRecSet.EoF)
                //{
                //    queryId = Convert.ToInt32(oRecSet.Fields.Item("QueryId").Value);
                //    fmsId = Convert.ToInt32(oRecSet.Fields.Item("IndexID").Value);
                //    oRecSet.DoQuery("update \"OUQR\" set \"qString\"='" + query + "' where \"intrnalKey\"='" + queryId.ToString() + "'");

                //}
                //else
                //{
                //    //oRecSet.DoQuery("select isnull(max(IndexID),0) +1 as fmsId from CSHS"); // Old Normal
                //    oRecSet.DoQuery("update OUQR set \"QString\"='" + query + "' where \"IntrnalKey\"='" + queryId.ToString() + "'"); // Hana & Normal
                //    fmsId = Convert.ToInt32(oRecSet.Fields.Item("fmsId").Value);
                //    queryId = addQuery(query, "Fms_" + frmId + "_" + itmId + "_" + colID);

                //    //string strS = "INSERT into [CSHS] ([FormID] ,[ItemID] ,[ColID] ,[ActionT] ,[QueryId] ,[IndexID] ,[Refresh]  ,[FrceRfrsh] ,[ByField]) "; // Old Normal
                //    string strS = "INSERT into \"CSHS\" (\"FormID\" ,\"ItemID\" ,\"ColID\" ,\"ActionT\" ,\"QueryId\" ,\"IndexID\" ,\"Refresh\"  ,\"FrceRfrsh\" ,\"ByField\") "; // Hana & Normal
                //    strS += " Values ('" + frmId + "','" + itmId + "','" + colID + "','2','" + queryId.ToString() + "','" + fmsId.ToString() + "','N','N','N')";
                //    oRecSet.DoQuery(strS);
                //}

                oRecSet = null;
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in creating formatted search" + "Fms_" + frmId + "_" + itmId + "_" + colID + ex.Message);
            }

        }

        public bool AddTable(string TableName, string TableDescription, SAPbobsCOM.BoUTBTableType TableType)
        {
            bool outResult = false;
            try
            {
                SAPbobsCOM.UserTablesMD v_UserTableMD = default(SAPbobsCOM.UserTablesMD);
                GC.Collect();
                if (!TableExists(TableName))
                {
                    oApplication.StatusBar.SetText("Creating Table " + TableName + " ...................", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    v_UserTableMD = (SAPbobsCOM.UserTablesMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
                    v_UserTableMD.TableName = TableName;
                    v_UserTableMD.TableDescription = TableDescription;
                    v_UserTableMD.TableType = TableType;
                    v_RetVal = v_UserTableMD.Add();
                    if (v_RetVal != 0)
                    {
                        oCompany.GetLastError(out v_ErrCode, out v_ErrMsg);
                        oApplication.StatusBar.SetText("Failed to Create Table " + TableName + v_ErrCode + " " + v_ErrMsg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(v_UserTableMD);
                        v_UserTableMD = null;
                        GC.Collect();
                        return false;
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("[@" + TableName + "] - " + TableDescription + " created successfully!!!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(v_UserTableMD);
                        v_UserTableMD = null;
                        outResult = true;
                        GC.Collect();
                        return true;
                    }
                }
                else
                {
                    GC.Collect();
                    return false;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to Add Table : " + ex.Message);
            }
            finally
            {
            }
            return outResult;
        }

        public bool UDFExists(string TableName, string FieldID)
        {
            bool outResult = false;
            try
            {
                SAPbobsCOM.Recordset rsetUDF = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                bool oFlag = true;
                //rsetUDF.DoQuery("Select 1 from [CUFD] Where TableID='" + TableName.Trim() + "' and AliasID='" + FieldID.Trim() + "'"); // Old Normal 
                //rsetUDF.DoQuery("Select 1 from CUFD Where \"TableID\"='" + TableName.Trim() + "' and \"AliasID\"='" + FieldID.Trim() + "'"); // Hana & Normal
                string s = "SELECT 1 FROM \"CUFD\" WHERE \"TableID\" = '" + TableName.Trim() + "' AND \"AliasID\" = '" + FieldID.Trim() + "'"; // Hana & Normal verified.
                rsetUDF.DoQuery(s);
                if (rsetUDF.EoF)
                    oFlag = false;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rsetUDF);
                rsetUDF = null;
                outResult = oFlag;
                GC.Collect();
                return oFlag;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Failed to UDF Exisits : " + ex.Message);
            }
            finally
            {
            }
            return outResult;
        }

        public class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            private IntPtr _hwnd;
            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }
            public System.IntPtr Handle
            {
                get { return _hwnd; }
            }
        }

        public void showRpt()
        {
            try
            {
                if (showReportCode != "")
                {
                    WinForms.frmRptViewer rptv = new WinForms.frmRptViewer();
                    rptv.Critaria = rptCritaria;
                    rptv.rptCode = showReportCode;
                    rptv.isSystem = isSystemReport;
                    rptv.DateParameter = rptDateParameter;
                    rptv.mfmcp = rptCP;
                    rptv.ShowDialog();
                }
            }
            catch(Exception ex)
            {
                if (logger != null)
                {
                    logger.LogException("showRpt", ex);
                }
            }
        }

        public void emailRpt()
        {
            try
            {
                if (showReportCode != "")
                {
                    WinForms.frmExportAndMail rptv = new WinForms.frmExportAndMail();
                    rptv.salaySlipIDs = salarySlipIDs;
                    rptv.rptCode = showReportCode;
                    rptv.isSystem = isSystemReport;
                    rptv.ShowDialog();
                }
            }
            catch(Exception ex)
            {
                if (logger != null)
                {
                    logger.LogException("emailRpt", ex);
                }
            }
        }

        private void OpenWbReportForm()
        {
            try
            {

                string comName = "WebView";
                Program.sqlString = "empElement";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void printRpt(string reportCode, bool isSystemRpt, string Critaria, string CurrentPeriod)
        {
            if (Program.WebReporting)
            {
                Program.ReportPrams = "Code=" + reportCode + "&Criteria=" + Critaria + "&rptCP=" + CurrentPeriod;
                OpenWbReportForm();
                return;
            }
            rptCritaria = Critaria;
            rptDateParameter = "";
            showReportCode = reportCode;
            isSystemReport = isSystemRpt;
            rptCP = CurrentPeriod;
            System.Threading.Thread ShowFolderBrowserThread = null;
            try
            {
                ShowFolderBrowserThread = new System.Threading.Thread(showRpt);
                if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    ShowFolderBrowserThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    ShowFolderBrowserThread.Start();
                }
                else if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    ShowFolderBrowserThread.Start();
                    ShowFolderBrowserThread.Join();

                }
                Thread.Sleep(5000);
                while (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogException("printRpt", ex);
                }
            }
        }

        public void printRpt(string reportCode, bool isSystemRpt, string Critaria, string CurrentPeriod, string pDateParameter)
        {

            if (Program.WebReporting)
            {
                Program.ReportPrams = "Code=" + reportCode + "&Criteria=" + Critaria + "&rptCP=" + CurrentPeriod + "&rptDateParameter=" + pDateParameter;
                OpenWbReportForm();
                return;
            }
            rptCritaria = Critaria;
            rptDateParameter = pDateParameter;
            showReportCode = reportCode;
            isSystemReport = isSystemRpt;
            rptCP = CurrentPeriod;

            System.Threading.Thread ShowFolderBrowserThread = null;
            try
            {
                ShowFolderBrowserThread = new System.Threading.Thread(showRpt);
                if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    ShowFolderBrowserThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    ShowFolderBrowserThread.Start();
                }
                else if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    ShowFolderBrowserThread.Start();
                    ShowFolderBrowserThread.Join();

                }
                Thread.Sleep(5000);
                while (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                oApplication.MessageBox("print rpt" + ex.Message);
            }

        }

        public void emailRpt(string reportCode, bool isSystemRpt, string slipIDs)
        {
            salarySlipIDs = slipIDs;
            showReportCode = reportCode;
            isSystemReport = isSystemRpt;
            System.Threading.Thread emailerThread = null;
            try
            {
                emailerThread = new System.Threading.Thread(emailRpt);
                if (emailerThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    emailerThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    emailerThread.Start();
                }
                else if (emailerThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    emailerThread.Start();
                    emailerThread.Join();

                }
                Thread.Sleep(5000);
                while (emailerThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    System.Windows.Forms.Application.DoEvents();
                }



            }
            catch (Exception ex)
            {

                oApplication.MessageBox("print rpt" + ex.Message);
            }
        }

        public string FindFile()
        {
            System.Threading.Thread ShowFolderBrowserThread = null;
            try
            {
                ShowFolderBrowserThread = new System.Threading.Thread(ShowFolderBrowser);
                if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    ShowFolderBrowserThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    ShowFolderBrowserThread.Start();
                }
                else if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    ShowFolderBrowserThread.Start();
                    ShowFolderBrowserThread.Join();

                }
                Thread.Sleep(5000);
                while (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    System.Windows.Forms.Application.DoEvents();
                }


                if (!string.IsNullOrEmpty(FileName))
                {
                    return FileName;
                }
            }
            catch (Exception ex)
            {
                oApplication.MessageBox("FileFile" + ex.Message);
            }

            return "";

        }

        public string SaveFile(string defName)
        {

            defFileName = defName;
            System.Threading.Thread ShowFolderBrowserThread = null;
            try
            {
                ShowFolderBrowserThread = new System.Threading.Thread(SaveFileBrowser);

                if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    ShowFolderBrowserThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    ShowFolderBrowserThread.Start();
                }
                else if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    ShowFolderBrowserThread.Start();
                    ShowFolderBrowserThread.Join();

                }
                Thread.Sleep(5000);

                while (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                if (!string.IsNullOrEmpty(FileName))
                {
                    return FileName;
                }
            }
            catch (Exception ex)
            {
                oApplication.MessageBox("FileFile" + ex.Message);
            }

            return "";

        }

        public void ShowFolderBrowser()
        {
            System.Diagnostics.Process[] MyProcs = null;
            dynamic UserName = Environment.UserName;
            int CallingWindowSAP = 0;
            FileName = "";
            OpenFileDialog OpenFile = new OpenFileDialog();

            try
            {
                OpenFile.Multiselect = false;
                OpenFile.Filter = "All files(*.)|*.*";
                int filterindex = 0;
                try
                {
                    filterindex = 0;
                }
                catch (Exception ex)
                {
                }

                OpenFile.FilterIndex = filterindex;

                OpenFile.RestoreDirectory = true;
                MyProcs = System.Diagnostics.Process.GetProcessesByName("SAP Business One");
                //MyProcs = System.Diagnostics.Process.GetProcessesByName(oApplication.Desktop.Title);
                for (int i = 0; i <= MyProcs.GetLength(0); i++)
                {
                    if (MyProcs[i].MainWindowTitle == B1ClientName)
                    {
                        CallingWindowSAP = i;
                        goto NEXT_STEP;
                    }
                    //if (GetProcessUserName(MyProcs[i]) == UserName)
                    //{
                    //    goto NEXT_STEP;
                    //}

                }
                oApplication.MessageBox("Unable to determine Running processes by UserName!");
                OpenFile.Dispose();
                GC.Collect();
                return;
                NEXT_STEP:
                if (MyProcs.Length == 1)
                {
                    for (int i = 0; i <= MyProcs.Length - 1; i++)
                    {
                        WindowWrapper MyWindow = new WindowWrapper(MyProcs[i].MainWindowHandle);
                        DialogResult ret = OpenFile.ShowDialog(MyWindow);

                        if (ret == DialogResult.OK)
                        {
                            FileName = OpenFile.FileName;
                            OpenFile.Dispose();
                        }
                        else
                        {
                            System.Windows.Forms.Application.ExitThread();
                        }
                    }
                }
                else if (MyProcs.Length > 1)
                {
                    for (int i = 0; i <= MyProcs.Length - 1; i++)
                    {
                        if (CallingWindowSAP == i)
                        {
                            WindowWrapper MyWindow = new WindowWrapper(MyProcs[i].MainWindowHandle);
                            DialogResult ret = OpenFile.ShowDialog(MyWindow);

                            if (ret == DialogResult.OK)
                            {
                                FileName = OpenFile.FileName;
                                OpenFile.Dispose();
                            }
                            else
                            {
                                System.Windows.Forms.Application.ExitThread();
                            }
                        }
                    }
                }
                else
                {
                    oApplication.MessageBox("More than 1 SAP B1 is started!");
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message);
                FileName = "";
            }
            finally
            {
                OpenFile.Dispose();
                GC.Collect();
            }

        }

        public void SaveFileBrowser()
        {
            System.Diagnostics.Process[] MyProcs = null;
            dynamic UserName = Environment.UserName;

            FileName = "";
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = defFileName;
            try
            {
                MyProcs = System.Diagnostics.Process.GetProcessesByName("SAP Business One");

                for (int i = 0; i <= MyProcs.GetLength(1); i++)
                {
                    if (GetProcessUserName(MyProcs[i]) == UserName)
                    {
                        goto NEXT_STEP;
                    }
                }
                oApplication.MessageBox("Unable to determine Running processes by UserName!");
                saveFile.Dispose();
                GC.Collect();
                return;
                NEXT_STEP:
                if (MyProcs.Length == 1)
                {

                    for (int i = 0; i <= MyProcs.Length - 1; i++)
                    {
                        WindowWrapper MyWindow = new WindowWrapper(MyProcs[i].MainWindowHandle);
                        DialogResult ret = saveFile.ShowDialog(MyWindow);

                        if (ret == DialogResult.OK)
                        {
                            FileName = saveFile.FileName;
                            saveFile.Dispose();
                        }
                        else
                        {
                            System.Windows.Forms.Application.ExitThread();
                        }
                    }
                }
                else
                {
                    oApplication.MessageBox("More than 1 SAP B1 is started!");
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message);
                FileName = "";
            }
            finally
            {
                saveFile.Dispose();
                GC.Collect();
            }

        }

        private string GetProcessUserName(System.Diagnostics.Process Process)
        {
            string strResult = "";
            ObjectQuery sq = new ObjectQuery("Select * from Win32_Process Where ProcessID = '" + Process.Id + "'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(sq);


            if (searcher.Get().Count == 0)
                return null;

            foreach (ManagementObject oReturn in searcher.Get())
            {
                string[] o = new string[2];

                //Invoke the method and populate the o var with the user name and domain                         
                oReturn.InvokeMethod("GetOwner", (object[])o);
                strResult = o[0];
                return o[0];
            }
            return strResult;


        }

        private void LoadMenuFromXML(string FileName, string iconPath)
        {
            try
            {
                string sPath = null;
                System.Reflection.Assembly thisExe = null;
                thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.Stream file = thisExe.GetManifestResourceStream(FileName);
                string xml = null;

                // Using 
                System.IO.StreamReader sr = new System.IO.StreamReader(file);

                try
                {
                    xml = sr.ReadToEnd();

                }
                catch (Exception EX)
                {
                }
                finally
                {
                    ((IDisposable)sr).Dispose();
                }

                sPath = System.Windows.Forms.Application.StartupPath + "\\";
                xml = xml.Replace("Payroll.bmp", sPath + "Payroll.bmp");
                //'// load the form to the SBO application in one batch
                oApplication.LoadBatchActions(xml);
                sPath = oApplication.GetLastBatchResults();
                oApplication.StatusBar.SetText("***", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_None);
                oApplication.StatusBar.SetText(sPath, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_None);
                oApplication.StatusBar.SetText("***", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_None);
            }
            catch (Exception ex)
            {
                oApplication.MessageBox(ex.Message + " " + FileName);
            }
        }

        public void doHrmsQuery(string strSql)
        {

            SqlConnection con = (SqlConnection)dbHr.Connection;
            try
            {

                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                //Replace(“\t”,”  “).Rplace(“\n”,” “); 
                cmd.CommandText = strSql; // strSql.Replace("\r","").Replace("\n","");
                int outResult = cmd.ExecuteNonQuery();



            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        public void applyPatch(string patchNum)
        {
            try
            {
                string filePath = "ACHR.Scripts.s" + patchNum + ".sql";
                System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
                StreamReader reader = new StreamReader(stream);
                string strSql = reader.ReadToEnd();
                stream.Close();
                doHrmsQuery(strSql);

                oApplication.SetStatusBarMessage("Patch Applied successfully!", BoMessageTime.bmt_Short, false);
            }
            catch
            {
                oApplication.SetStatusBarMessage("Patch could not be applied");
            }
        }

        public void loadHrmsEmps(SAPbouiCOM.ChooseFromList oCFL)
        {
            SAPbouiCOM.Conditions oConds;

            SAPbouiCOM.Conditions oEmptyConds = new SAPbouiCOM.Conditions();
            //oCFL.ObjectType = "171";
            oCFL.SetConditions(oEmptyConds);

            IEnumerable<MstEmployee> emps = from p in dbHr.MstEmployee where p.SBOEmpCode != null && p.SBOEmpCode != "" select p;
            oConds = oCFL.GetConditions();
            int i = 1;
            int j = 0;

            foreach (MstEmployee emp in emps)
            {
                SAPbouiCOM.Condition oCond;

                oCond = oConds.Add();
                oCond.Alias = "empID";
                oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL;
                oCond.CondVal = emp.SBOEmpCode.Trim().ToString();
                if (i != emps.Count())
                {
                    oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_OR;

                }
                i++;
            }

            oCFL.SetConditions(oConds);
        }

        public string postJe(long sourceId, DateTime pPostingDate)
        {
            string outStr = "";
            SAPbobsCOM.JournalEntries vJE = (SAPbobsCOM.JournalEntries)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
            TrnsJE je = (from p in dbHr.TrnsJE where p.ID == sourceId select p).FirstOrDefault();
            if (je == null) return "Error : Journal entry can't be retrived.";
            string Remarks = je.Memo;
            
            DateTime postingDate = pPostingDate;

            try
            {
                vJE.ReferenceDate = postingDate;
                vJE.TaxDate = postingDate;
                vJE.DueDate = postingDate;
                if (!string.IsNullOrEmpty(JeSeries))
                {
                    vJE.Series = Convert.ToInt32(JeSeries);
                }
                if (Remarks.Length > 50)
                {
                    vJE.Memo = Remarks.Substring(0, 50); // dgSales[3, i].Value.ToString().Substring(0, 20);
                }
                else
                {
                    vJE.Memo = Remarks;
                }
                vJE.Reference = "Payroll JE Period " + je.CfgPeriodDates.PeriodName;
                vJE.Reference2 = je.ID.ToString();

                foreach (TrnsJEDetail acct in je.TrnsJEDetail)
                {
                    vJE.Lines.ShortName = acct.AcctCode;
                    vJE.Lines.DueDate = postingDate;
                    vJE.Lines.ReferenceDate1 = postingDate;
                    vJE.Lines.TaxDate = postingDate;
                    vJE.Lines.Reference1 = acct.ID.ToString();

                    #region Foriegn Currency
                    if (Convert.ToBoolean(Program.systemInfo.FlgJECurrency))
                    {
                        vJE.Lines.FCCredit = Convert.ToDouble(acct.Credit);
                        vJE.Lines.FCDebit = Convert.ToDouble(acct.Debit);
                        vJE.Lines.FCCurrency = acct.FCurrency.Trim();
                    }
                    else
                    {
                        vJE.Lines.Credit = Convert.ToDouble(acct.Credit);
                        vJE.Lines.Debit = Convert.ToDouble(acct.Debit);
                    }
                    #endregion

                    #region CostCenter
                    if (!String.IsNullOrEmpty(acct.CostCenter))
                    {
                        String DimCodes = "0";
                        String CCValue = acct.CostCenter.Trim();
                        //String strQuery = "SELECT DimCode FROM dbo.OPRC WHERE PrcCode = '" + CCValue + "'";
                        //String strQuery = "SELECT T0.\"DimCode\" FROM OPRC T0 WHERE T0.\"PrcCode\" = '" + CCValue + "'";
                        string strQuery = "SELECT \"DimCode\" FROM OOCR WHERE \"OcrCode\" = '" + CCValue + "'";
                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oRecSet.DoQuery(strQuery);
                        if (oRecSet.EoF)
                        {
                            outStr = "Error : CostCenter unable to retrive.";
                        }
                        DimCodes = Convert.ToString(oRecSet.Fields.Item("DimCode").Value);
                        if (DimCodes == "1")
                        {
                            vJE.Lines.CostingCode = CCValue;
                        }
                        if (DimCodes == "2")
                        {
                            vJE.Lines.CostingCode2 = CCValue;
                        }
                        if (DimCodes == "3")
                        {
                            vJE.Lines.CostingCode3 = CCValue;
                        }
                        if (DimCodes == "4")
                        {
                            vJE.Lines.CostingCode4 = CCValue;
                        }
                        if (DimCodes == "5")
                        {
                            vJE.Lines.CostingCode5 = CCValue;
                        }
                    }
                    #endregion

                    #region Branches
                    //Branches Integeration
                    if (!String.IsNullOrEmpty(acct.BranchName))
                    {
                        //vJE.Lines.BPLID = acct.BranchName;
                        String BranchIDFromSAP = "0";
                        String BBValue = acct.BranchName.Trim();
                        //String strQuery = "SELECT dbo.OBPL.BPLId As BPLId FROM dbo.OBPL WHERE dbo.OBPL.BPLName = '" + BBValue + "'";
                        String strQuery = "SELECT T0.\"BPLId\" FROM OBPL T0 WHERE T0.\"BPLName\" = '" + BBValue + "'";
                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oRecSet.DoQuery(strQuery);
                        if (oRecSet.EoF)
                        {
                            outStr = "Error : BranchName unable to retrive.";
                        }
                        BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);
                        if (BranchIDFromSAP != "0")
                        {
                            vJE.Lines.BPLID = Convert.ToInt32(BranchIDFromSAP);
                        }
                    }
                    #endregion

                    #region Dimensions
                    if (Convert.ToBoolean(Program.systemInfo.FlgMultipleDimension))
                    {
                        if (!string.IsNullOrEmpty(acct.Dimension1))
                        {
                            vJE.Lines.CostingCode = acct.Dimension1.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension2))
                        {
                            vJE.Lines.CostingCode2 = acct.Dimension2.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension3))
                        {
                            vJE.Lines.CostingCode3 = acct.Dimension3.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension4))
                        {
                            vJE.Lines.CostingCode4 = acct.Dimension4.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension5))
                        {
                            vJE.Lines.CostingCode5 = acct.Dimension5.Trim();
                        }
                    }
                    #endregion

                    #region Branch Posting CompanyDBWise SSL
                    if (!String.IsNullOrEmpty(BranchName))
                    {
                        //vJE.Lines.BPLID = acct.BranchName;
                        String BranchIDFromSAP = "0";
                        String BBValue = BranchName.Trim();
                        //String strQuery = "SELECT dbo.OBPL.BPLId As BPLId FROM dbo.OBPL WHERE dbo.OBPL.BPLName = '" + BBValue + "'";
                        String strQuery = "SELECT T0.\"BPLId\" FROM OBPL T0 WHERE T0.\"BPLFrName\" = '" + BBValue + "'";
                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oRecSet.DoQuery(strQuery);
                        if (oRecSet.EoF)
                        {
                            outStr = "Error : BranchName unable to retrive.";
                        }
                        BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);
                        if (BranchIDFromSAP != "0")
                        {
                            vJE.Lines.BPLID = Convert.ToInt32(BranchIDFromSAP);
                        }
                    }
                    #endregion

                    #region Projects
                    //Projects Integeration
                    if (Program.systemInfo.FlgProject == true)
                    {
                        if (!String.IsNullOrEmpty(acct.Project))
                        {
                            String ProjectCodeFromSAP = "0";
                            String BBValue = acct.Project.Trim();
                            String strQuery = "SELECT \"PrjCode\" FROM OPRJ WHERE \"PrjCode\" = '" + BBValue + "'";
                            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                            oRecSet.DoQuery(strQuery);
                            if (oRecSet.EoF)
                            {
                                outStr = "Error : Project Name unable to retrive.";
                            }
                            ProjectCodeFromSAP = Convert.ToString(oRecSet.Fields.Item("PrjCode").Value);
                            if (ProjectCodeFromSAP != "0")
                            {
                                vJE.Lines.ProjectCode = ProjectCodeFromSAP;
                            }
                        }
                    }
                    #endregion

                    vJE.Lines.Add();
                }
            }
            catch (Exception ex)
            {
                outStr = "Error : " + ex.Message;
            }
            int testnum = vJE.Add();
            if (testnum != 0)
            {
                int erroCode = 0;
                string errDescr = "";
                oCompany.GetLastError(out erroCode, out errDescr);
                outStr = "Error : " + errDescr + outStr;
            }
            else
            {
                outStr = Convert.ToString(oCompany.GetNewObjectKey());
            }
            return outStr;
        }

        public string postJe(long sourceId)
        {
            string outStr = "";
            SAPbobsCOM.JournalEntries vJE = (SAPbobsCOM.JournalEntries)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
            TrnsJE je = (from p in dbHr.TrnsJE where p.ID == sourceId select p).FirstOrDefault();
            if (je == null) return "Error : Journal entry can't be retrived.";
            string Remarks = je.Memo;

            DateTime postingDate = Convert.ToDateTime(je.JEPostingDate);

            try
            {
                vJE.ReferenceDate = postingDate;
                vJE.TaxDate = postingDate;
                vJE.DueDate = postingDate;
                if (!string.IsNullOrEmpty(JeSeries))
                {
                    vJE.Series = Convert.ToInt32(JeSeries);
                }
                if (Remarks.Length > 50)
                {
                    vJE.Memo = Remarks.Substring(0, 50); // dgSales[3, i].Value.ToString().Substring(0, 20);
                }
                else
                {
                    vJE.Memo = Remarks;
                }
                vJE.Reference = "Payroll JE Period " + je.CfgPeriodDates.PeriodName;
                vJE.Reference2 = je.ID.ToString();

                foreach (TrnsJEDetail acct in je.TrnsJEDetail)
                {
                    vJE.Lines.ShortName = acct.AcctCode;
                    vJE.Lines.DueDate = postingDate;
                    vJE.Lines.ReferenceDate1 = postingDate;
                    vJE.Lines.TaxDate = postingDate;
                    vJE.Lines.Reference1 = acct.ID.ToString();

                    #region Foriegn Currency
                    if (Convert.ToBoolean(Program.systemInfo.FlgJECurrency))
                    {
                        vJE.Lines.FCCredit = Convert.ToDouble(acct.Credit);
                        vJE.Lines.FCDebit = Convert.ToDouble(acct.Debit);
                        vJE.Lines.FCCurrency = acct.FCurrency.Trim();
                    }
                    else
                    {
                        vJE.Lines.Credit = Convert.ToDouble(acct.Credit);
                        vJE.Lines.Debit = Convert.ToDouble(acct.Debit);
                    }
                    #endregion

                    #region CostCenter
                    if (!String.IsNullOrEmpty(acct.CostCenter))
                    {
                        String DimCodes = "0";
                        String CCValue = acct.CostCenter.Trim();
                        //String strQuery = "SELECT DimCode FROM dbo.OPRC WHERE PrcCode = '" + CCValue + "'";
                        //String strQuery = "SELECT T0.\"DimCode\" FROM OPRC T0 WHERE T0.\"PrcCode\" = '" + CCValue + "'";
                        string strQuery = "SELECT \"DimCode\" FROM OOCR WHERE \"OcrCode\" = '" + CCValue + "'";
                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oRecSet.DoQuery(strQuery);
                        if (oRecSet.EoF)
                        {
                            outStr = "Error : CostCenter unable to retrive.";
                        }
                        DimCodes = Convert.ToString(oRecSet.Fields.Item("DimCode").Value);
                        if (DimCodes == "1")
                        {
                            vJE.Lines.CostingCode = CCValue;
                        }
                        if (DimCodes == "2")
                        {
                            vJE.Lines.CostingCode2 = CCValue;
                        }
                        if (DimCodes == "3")
                        {
                            vJE.Lines.CostingCode3 = CCValue;
                        }
                        if (DimCodes == "4")
                        {
                            vJE.Lines.CostingCode4 = CCValue;
                        }
                        if (DimCodes == "5")
                        {
                            vJE.Lines.CostingCode5 = CCValue;
                        }
                    }
                    #endregion

                    #region Branches
                    //Branches Integeration
                    if (!String.IsNullOrEmpty(acct.BranchName))
                    {
                        //vJE.Lines.BPLID = acct.BranchName;
                        String BranchIDFromSAP = "0";
                        String BBValue = acct.BranchName.Trim();
                        //String strQuery = "SELECT dbo.OBPL.BPLId As BPLId FROM dbo.OBPL WHERE dbo.OBPL.BPLName = '" + BBValue + "'";
                        String strQuery = "SELECT T0.\"BPLId\" FROM OBPL T0 WHERE T0.\"BPLName\" = '" + BBValue + "'";
                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oRecSet.DoQuery(strQuery);
                        if (oRecSet.EoF)
                        {
                            outStr = "Error : BranchName unable to retrive.";
                        }
                        BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);
                        if (BranchIDFromSAP != "0")
                        {
                            vJE.Lines.BPLID = Convert.ToInt32(BranchIDFromSAP);
                        }
                    }
                    #endregion

                    #region Dimensions
                    if (Convert.ToBoolean(Program.systemInfo.FlgMultipleDimension))
                    {
                        if (!string.IsNullOrEmpty(acct.Dimension1))
                        {
                            vJE.Lines.CostingCode = acct.Dimension1.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension2))
                        {
                            vJE.Lines.CostingCode2 = acct.Dimension2.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension3))
                        {
                            vJE.Lines.CostingCode3 = acct.Dimension3.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension4))
                        {
                            vJE.Lines.CostingCode4 = acct.Dimension4.Trim();
                        }
                        if (!string.IsNullOrEmpty(acct.Dimension5))
                        {
                            vJE.Lines.CostingCode5 = acct.Dimension5.Trim();
                        }
                    }
                    #endregion

                    #region Branch Posting CompanyDBWise SSL
                    if (!String.IsNullOrEmpty(BranchName))
                    {
                        //vJE.Lines.BPLID = acct.BranchName;
                        String BranchIDFromSAP = "0";
                        String BBValue = BranchName.Trim();
                        //String strQuery = "SELECT dbo.OBPL.BPLId As BPLId FROM dbo.OBPL WHERE dbo.OBPL.BPLName = '" + BBValue + "'";
                        String strQuery = "SELECT T0.\"BPLId\" FROM OBPL T0 WHERE T0.\"BPLFrName\" = '" + BBValue + "'";
                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oRecSet.DoQuery(strQuery);
                        if (oRecSet.EoF)
                        {
                            outStr = "Error : BranchName unable to retrive.";
                        }
                        BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);
                        if (BranchIDFromSAP != "0")
                        {
                            vJE.Lines.BPLID = Convert.ToInt32(BranchIDFromSAP);
                        }
                    }
                    #endregion

                    #region Projects
                    //Projects Integeration
                    if (Program.systemInfo.FlgProject == true)
                    {
                        if (!String.IsNullOrEmpty(acct.Project))
                        {
                            String ProjectCodeFromSAP = "0";
                            String BBValue = acct.Project.Trim();
                            String strQuery = "SELECT \"PrjCode\" FROM OPRJ WHERE \"PrjCode\" = '" + BBValue + "'";
                            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                            oRecSet.DoQuery(strQuery);
                            if (oRecSet.EoF)
                            {
                                outStr = "Error : Project Name unable to retrive.";
                            }
                            ProjectCodeFromSAP = Convert.ToString(oRecSet.Fields.Item("PrjCode").Value);
                            if (ProjectCodeFromSAP != "0")
                            {
                                vJE.Lines.ProjectCode = ProjectCodeFromSAP;
                            }
                        }
                    }
                    #endregion

                    vJE.Lines.Add();
                }
            }
            catch (Exception ex)
            {
                outStr = "Error : " + ex.Message;
            }
            int testnum = vJE.Add();
            if (testnum != 0)
            {
                int erroCode = 0;
                string errDescr = "";
                oCompany.GetLastError(out erroCode, out errDescr);
                outStr = "Error : " + errDescr + outStr;
            }
            else
            {
                outStr = Convert.ToString(oCompany.GetNewObjectKey());
            }
            return outStr;
        }

        private void mfmLICBreakUP()
        {
            try
            {
                if (!String.IsNullOrEmpty(Program.HashKey))
                {
                    if (Program.HashKey.Length == 44)
                    {
                        String Decoded, StTime, EnTime;
                        //HKey = 
                        Decoded = mfmLicensingMain.Decrypt(Program.HashKey, HKey);
                        Program.AppType = Decoded.Substring(0, 2);
                        StTime = Decoded.Substring(3, 8);
                        //StTime = "20140101";
                        Program.StartTime = DateTime.ParseExact(StTime, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        EnTime = Decoded.Substring(12, 8);
                        //EnTime = "20141231";
                        Program.EndTime = DateTime.ParseExact(EnTime, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("mfmLICBreakUP Exception : " + ex.Message, BoMessageTime.bmt_Short, true);
            }
        }

        private void mfmLicWarnings()
        {
            try
            {
                if (DateTime.Now.Date >= Program.StartTime && DateTime.Now.Date <= Program.EndTime)
                {
                    double DaysToLeft = Math.Abs((DateTime.Now.Date - Program.EndTime.Date).TotalDays);
                    if (DaysToLeft <= 30.0)
                    {

                        oApplication.StatusBar.SetText("Conctact AbacusConsultings @ support.sapb1@abacus-global.com before your License Expired. " + DaysToLeft.ToString() + " Days Left.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    }
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void mfmAppVersioning()
        {
            try
            {
                if (dbHr != null)
                {
                    string dbAppVersion = string.Empty;
                    string dbDBVersion = string.Empty;
                    var oLine = (from a in dbHr.MstCompany select a).FirstOrDefault();
                    if (oLine != null)
                    {
                        dbAppVersion = oLine.AppVersion;
                        dbDBVersion = oLine.DBVersion;
                        if (AppVersion != dbAppVersion)
                        {
                            oLine.AppVersion = AppVersion;
                            oLine.UpdateDate = DateTime.Now;
                            oLine.UpdatedBy = "MFM";
                            dbHr.SubmitChanges();
                        }
                    }
                    else
                    {
                        MstCompany oCom = new MstCompany();
                        dbHr.MstCompany.InsertOnSubmit(oCom);
                        oCom.CompanyName = "AbacusConsultings";
                        oCom.CompType = "F";
                        oCom.AppVersion = AppVersion;
                        oCom.DBVersion = "";
                        oCom.CreateDate = DateTime.Now;
                        oCom.UserId = "MFM";
                        oCom.UpdateDate = DateTime.Now;
                        oCom.UpdatedBy = "MFM";
                        dbHr.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("" + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
        }

        private void GetLogInUserStatus()
        {
            string strOut = string.Empty, strFilterEmployee = string.Empty;
            string strSql = "SELECT \"U_SuperUser\" , \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'";
            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecSet.DoQuery(strSql);
            strOut = Convert.ToString(oRecSet.Fields.Item("U_SuperUser").Value);
            strFilterEmployee = Convert.ToString(oRecSet.Fields.Item("U_PayrollType").Value);
            if (!string.IsNullOrEmpty(strOut) && strOut.ToLower() == "no")
            {
                isSuperUser = false;
            }
            else
            {
                isSuperUser = true;
            }
            if (string.IsNullOrEmpty(strFilterEmployee))
            {
                EmployeeFilterValues = "";
            }
            else
            {
                EmployeeFilterValues = strFilterEmployee;
            }
        }

    }
}
