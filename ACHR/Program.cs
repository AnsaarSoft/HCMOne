using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Cryptography;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DIHRMS;

namespace ACHR
{
    static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static UDClass objHrmsUI;
        public static string sboLanguage;
        public static String HashKey = "";
        public static String AppType = "";
        public static DateTime StartTime, EndTime;        
        public static string EmpID = "";
        //public static string BatchID = "";
        public static string ExtendendEmpID = "";
        public static string AttachEmpID = "";
        public static string sqlString = "";
        public static string FromEmpId = "";
        public static string ToEmpId = "";
        public static string EmpBasicSalary = "";
        public static CfgPayrollBasicInitialization systemInfo;
        public static string ConStrHRMS = string.Empty;
        public static string BatchID = "";
        public static Boolean flgFinalSettelment = false;
        public struct SapItems
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
        }

        public class ElementList
        {
            public string ElementName { get; set; }
            public decimal ElementAmount { get; set; }
        }

        public class PresentOTSlab
        {
            public string EmpCode { get; set; }
            public int OTmins { get; set; }
            public int PresentDays { get; set; }
        }

        public static List<SapItems> oSapItems = new List<SapItems>();

        public static List<PresentOTSlab> oOTSlabs = new List<PresentOTSlab>();
        public static bool WebReporting=false ;
        public static string ReportPrams="";
        public static string WebViewerUrl;

        [STAThread]
        static void Main()
        {
            string sConnectionString = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";
            try
            {
                sConnectionString = Environment.GetCommandLineArgs().GetValue(1).ToString();
            }
            catch { }

            objHrmsUI = new UDClass(sConnectionString);
            SetWebViewer();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run();
        }

        public static void SetWebViewer()
        {
            try
            {
                if (objHrmsUI.dbHr.CfgReportViewer.Any())
                {
                    var firstRec = objHrmsUI.dbHr.CfgReportViewer.FirstOrDefault(o => o.Id == 1);
                    Program.WebReporting = firstRec.EnableWebReport ?? false;
                    Program.WebViewerUrl = firstRec.WebReportServerURL;
                }
            }
            catch (Exception ex ) 
            {
                Program.WebReporting = false;
                
            }            

        }
        
        public static String mfmLICEncryption(String pType,String pDate)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(pType);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        
        public static void SetReportOriginal(ReportDocument rep)
        {
            // rep.SetDatabaseLogon(

            rep.SetDatabaseLogon(objHrmsUI.HRMSDBuid, objHrmsUI.HRMSDbPwd, objHrmsUI.HRMSDbServer, objHrmsUI.HRMSDbName, true);

            foreach (CrystalDecisions.CrystalReports.Engine.Table Table in rep.Database.Tables)
            {
                CrystalDecisions.Shared.TableLogOnInfo Logon;
                Logon = Table.LogOnInfo;
                Logon.ConnectionInfo.DatabaseName = objHrmsUI.HRMSDbName;
                Logon.ConnectionInfo.ServerName = objHrmsUI.HRMSDbServer;
                Logon.ConnectionInfo.Password = Program.objHrmsUI.HRMSDbPwd;
                Logon.ConnectionInfo.UserID = Program.objHrmsUI.HRMSDBuid;
                Table.ApplyLogOnInfo(Logon);
            }

            foreach (ReportDocument rpt in rep.Subreports )
            {
                rpt.SetDatabaseLogon(objHrmsUI.HRMSDBuid, objHrmsUI.HRMSDbPwd, objHrmsUI.HRMSDbServer, objHrmsUI.HRMSDbName, true);

                foreach (CrystalDecisions.CrystalReports.Engine.Table Table in rpt.Database.Tables)
                {
                    CrystalDecisions.Shared.TableLogOnInfo Logon;
                    Logon = Table.LogOnInfo;
                    Logon.ConnectionInfo.DatabaseName = objHrmsUI.HRMSDbName;
                    Logon.ConnectionInfo.ServerName = objHrmsUI.HRMSDbServer;
                    Logon.ConnectionInfo.Password = Program.objHrmsUI.HRMSDbPwd;
                    Logon.ConnectionInfo.UserID = Program.objHrmsUI.HRMSDBuid;
                    Table.ApplyLogOnInfo(Logon);
                }
            }           
        }

        public static void SetReport(ReportDocument rep)
        {   
            foreach (CrystalDecisions.CrystalReports.Engine.Table Table in rep.Database.Tables)
            {
                #region Dynamic Report Credentials
                CrystalDecisions.Shared.ConnectionInfo coninfo = new CrystalDecisions.Shared.ConnectionInfo();
                coninfo.ServerName = objHrmsUI.HRMSDbServer;
                coninfo.UserID = Program.objHrmsUI.HRMSDBuid;
                coninfo.Password = Program.objHrmsUI.HRMSDbPwd;
                coninfo.DatabaseName = objHrmsUI.HRMSDbName;
                CrystalDecisions.Shared.TableLogOnInfo info = new CrystalDecisions.Shared.TableLogOnInfo();
                
                info.ConnectionInfo = coninfo;
                for (int i = 0; i < rep.Database.Tables.Count; i++)
                {
                    rep.Database.Tables[i].ApplyLogOnInfo(info);
                    rep.Refresh();
                }
                #endregion               
            }

            foreach (ReportDocument rpt in rep.Subreports)
            {   
                foreach (CrystalDecisions.CrystalReports.Engine.Table Table in rpt.Database.Tables)
                {
                    #region Dynamic Report Credentials
                    CrystalDecisions.Shared.ConnectionInfo coninfo = new CrystalDecisions.Shared.ConnectionInfo();
                    coninfo.ServerName = objHrmsUI.HRMSDbServer;
                    coninfo.UserID = Program.objHrmsUI.HRMSDBuid;
                    coninfo.Password = Program.objHrmsUI.HRMSDbPwd;
                    coninfo.DatabaseName = objHrmsUI.HRMSDbName;
                    CrystalDecisions.Shared.TableLogOnInfo info = new CrystalDecisions.Shared.TableLogOnInfo();
                    
                    info.ConnectionInfo = coninfo;
                    for (int i = 0; i < rep.Database.Tables.Count; i++)
                    {
                        rep.Database.Tables[i].ApplyLogOnInfo(info);
                        rep.Refresh();
                    }
                    #endregion
                   
                }
            }
        }
    }
}
