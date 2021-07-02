using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;


namespace DIHRMS.Custom
{
    public class LOV : eMail
    {

        dbHRMS oDB = null;

        public LOV(dbHRMS pDB) : base(pDB)
        {
            oDB = pDB;
        }

        private DataTable GetLOVAllOldExcel()
        {
            DataTable dtLOVList = new DataTable();
            try
            {
                //Variables
                String vAssemblyPath = "DIHRMS.Resources.LOVList.xlsx";
                String vFilePath = @"c:\LOVList.xlsx";
                String connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + vFilePath + ";Extended Properties=Excel 12.0;";
                //Logic
                dtLOVList.Columns.Add("Code");
                dtLOVList.Columns.Add("Value");
                dtLOVList.Columns.Add("Type");
                Assembly oAssembly = Assembly.GetExecutingAssembly();
                Stream oFileStream = oAssembly.GetManifestResourceStream(vAssemblyPath);
                SaveStreamToFile(@"c:\LOVList.xlsx", oFileStream);
                OleDbConnection oOleDbConn = new System.Data.OleDb.OleDbConnection(connStr);
                OleDbDataAdapter oDataAdapter = new OleDbDataAdapter("Select * From [L1$]", oOleDbConn);
                oDataAdapter.Fill(dtLOVList);
                oOleDbConn.Close();
            }
            catch (Exception Ex)
            {

            }

            return dtLOVList;
        }

        private DataTable GetLOVTypeOldExcel(string pType)
        {
            DataTable dtLOVList = new DataTable();
            DataTable dtSelectedLOV = new DataTable();
            try
            {
                //Variables
                String vAssemblyPath = "DIHRMS.Resources.LOVList.xlsx";
                String vFilePath = @"c:\LOVList.xlsx";
                String connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + vFilePath + ";Extended Properties=Excel 12.0;";
                //Logic
                dtLOVList.Columns.Add("Code");
                dtLOVList.Columns.Add("Value");
                dtLOVList.Columns.Add("Type");
                Assembly oAssembly = Assembly.GetExecutingAssembly();
                Stream oFileStream = oAssembly.GetManifestResourceStream(vAssemblyPath);
                SaveStreamToFile(@"c:\LOVList.xlsx", oFileStream);
                OleDbConnection oOleDbConn = new System.Data.OleDb.OleDbConnection(connStr);
                OleDbDataAdapter oDataAdapter = new OleDbDataAdapter("Select * From [L1$]", oOleDbConn);
                oDataAdapter.Fill(dtLOVList);
                oOleDbConn.Close();

                var values = from t in dtLOVList.AsEnumerable()
                             where t.Field<String>("Type") == pType
                             select new
                             {
                                 Code = t.Field<String>("Code"),
                                 Value = t.Field<String>("Value"),
                                 Type = t.Field<String>("Type")
                             };
                dtSelectedLOV.Columns.Add("Code");
                dtSelectedLOV.Columns.Add("Value");
                dtSelectedLOV.Columns.Add("Type");
                foreach (var value in values)
                {
                    dtSelectedLOV.Rows.Add(value.Code, value.Value, value.Type);
                }

            }
            catch (Exception Ex)
            {

            }

            return dtSelectedLOV;
        }

        public DataTable GetAllLov(String pLanguage)
        {
            DataTable dtMain = new DataTable();
            dtMain.Columns.Add("ID");
            dtMain.Columns.Add("Code");
            dtMain.Columns.Add("Value");
            dtMain.Columns.Add("Type");
            dtMain.Columns.Add("Language");
            if (String.IsNullOrEmpty(pLanguage))
            {
                pLanguage = "ln_English";
            }

            var Records = from a in oDB.MstLOVE
                          where a.Language.Contains(pLanguage)
                          select a;

            foreach (var Record in Records)
            {
                dtMain.Rows.Add(Record.Id, Record.Code, Record.Value, Record.Type, Record.Language);
            }

            return dtMain;
        }

        public DataTable GetLovType(String pType, String pLanguage)
        {
            DataTable dtMain = new DataTable();
            dtMain.Columns.Add("ID");
            dtMain.Columns.Add("Code");
            dtMain.Columns.Add("Value");
            dtMain.Columns.Add("Type");
            dtMain.Columns.Add("Language");
            if (String.IsNullOrEmpty(pLanguage))
            {
                pLanguage = "ln_English";
            }

            var Records = from a in oDB.MstLOVE
                          where a.Language.Contains(pLanguage) && a.Type == pType
                          select a;

            foreach (var Record in Records)
            {
                dtMain.Rows.Add(Record.Id, Record.Code, Record.Value, Record.Type, Record.Language);
            }

            return dtMain;
        }

        private void SaveStreamToFile(string fileFullPath, Stream stream)
        {
            if (stream.Length == 0) return;
            // Create a FileStream object to write a stream to a file
            using (FileStream fileStream = System.IO.File.Create(fileFullPath, (int)stream.Length))
            {
                // Fill the bytes[] array with the stream data
                byte[] bytesInStream = new byte[stream.Length];
                stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                // Use FileStream object to write to the specified file
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
            }
        }

        public Int32 GetDocumentNumber(Int32 Series, Int16 DocType)
        {
            Int32 ReturnDocnum = 0;
            try
            {
                switch (DocType)
                {
                    case 11:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsLoan where a.Series == Series select a.DocNum).Max());
                        break;
                    case 12:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsResignation where a.Series == Series select a.DocNum).Max());
                        break;
                    case 13:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsLeavesRequest where a.Series == Series select a.DocNum).Max());
                        break;
                    case 14:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsHeadBudget where a.Series == Series select a.DocNum).Max());
                        break;
                    case 15:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsJobRequisition where a.Series == Series select a.DocNum).Max());
                        break;
                    case 16:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsCompetencyProfile where a.Series == Series select a.DocNum).Max());
                        break;
                    case 17:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsPerformanceAppraisal where a.Series == Series select a.DocNum).Max());
                        break;
                    case 18:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsPerformanceAppraisal360 where a.Series == Series select a.DocNum).Max());
                        break;
                    case 19:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsPromotionAdvice where a.Series == Series select a.DocNum).Max());
                        break;
                    case 20:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsAdvance where a.Series == Series select a.DocNum).Max());
                        break;
                    case 21:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsInterviewEAS where a.Series == Series select a.DocNum).Max());
                        break;
                    case 22:
                        //ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsInterviewCall where a.Series == Series select a.DocNum).Max());
                        break;
                    case 23:
                        ReturnDocnum = Convert.ToInt32((from a in oDB.TrnsEmployeeReHire select a.DocNo).Max());
                        break;
                    default:
                        ReturnDocnum = 0;
                        break;
                }

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
                ReturnDocnum = 0;
            }

            return ++ReturnDocnum;
        }

        public Int32 GetEmployeeNumber()
        {
            Int32 EmpID = 0;
            try
            {
                EmpID = Convert.ToInt32((from a in oDB.MstEmployee select a.ID).Max());

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
                EmpID = 0;
            }
            return ++EmpID;
        }

        public Int32 GetNextCadidate()
        {
            Int32 CandidateNo = 0;
            try
            {
                CandidateNo = Convert.ToInt32((from a in oDB.MstCandidate select a.CandidateNo).Max());
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
                CandidateNo = 0;
            }

            return ++CandidateNo;
        }


    }

}
