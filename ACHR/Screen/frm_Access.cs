using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using System.IO;
using System.Data.OleDb;

namespace ACHR.Screen
{
    class frm_Access : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btSave, btCancel, btnImp;
        SAPbouiCOM.EditText txtfileName;
        SAPbouiCOM.DataTable dtAttRecords;
        SAPbouiCOM.Matrix grdAttRecords;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, EmpId, Date, Time, In_Out;
        SAPbouiCOM.Button btPick;
        DataTable DtFile = new DataTable();
        string EmpCode, InOut, PunchDate, PunchTime, flgProcessed;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_Access Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                //var UnitFeature = dbHrPayroll.CfgPayrollBasicInitialization.Where(pi => pi.FlgUnitFeature == true).FirstOrDefault();
                switch (pVal.ItemUID)
                {
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)

                            if (Program.systemInfo.FlgUnitFeature == true)
                            {
                                AttendanceRecordImportMEPL();
                            }
                            else
                            {
                                AttendanceRecordImport();
                            }
                        break;
                    case "btPick":
                        getFileName();
                        break;
                    case "btnImp":
                        //AR

                        if (Program.systemInfo.FlgUnitFeature == true)
                        {
                            pullAttRecordFromFileMEPL();
                        }
                        else
                        {
                            pullAttRecordFromFile();
                        }

                        break;
                    case "2":
                        break;
                    case "btclear":
                        ClearAllRecords();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_Access Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("2").Specific;
                btPick = oForm.Items.Item("btPick").Specific;
                btnImp = oForm.Items.Item("btnImp").Specific;

                txtfileName = oForm.Items.Item("txtFLoc").Specific;

                InitiallizegridMatrix();


                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtAttRecords = oForm.DataSources.DataTables.Add("AttendRequest");
                dtAttRecords.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAttRecords.Columns.Add("EmpId", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttRecords.Columns.Add("PunchDate", SAPbouiCOM.BoFieldsType.ft_Date);
                dtAttRecords.Columns.Add("PunchTime", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttRecords.Columns.Add("In_Out", SAPbouiCOM.BoFieldsType.ft_Text);


                grdAttRecords = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_att").Specific;
                oColumns = (SAPbouiCOM.Columns)grdAttRecords.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("AttendRequest", "No");

                oColumn = oColumns.Item("cl_EmpId");
                EmpId = oColumn;
                oColumn.DataBind.Bind("AttendRequest", "EmpId");

                oColumn = oColumns.Item("cl_date");
                Date = oColumn;
                oColumn.DataBind.Bind("AttendRequest", "PunchDate");

                oColumn = oColumns.Item("cl_Time");
                Time = oColumn;
                oColumn.DataBind.Bind("AttendRequest", "PunchTime");

                oColumn = oColumns.Item("cl_type");
                In_Out = oColumn;
                oColumn.DataBind.Bind("AttendRequest", "In_Out");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void getFileName()
        {
            string fileName = Program.objHrmsUI.FindFile();
            txtfileName.Value = fileName;
        }

        private void pullAttRecordFromFile()
        {

            string fileName = txtfileName.Value.Trim();
            if (fileName == "") return;
            AttendanceRecordToGrid(fileName);
        }

        private void AttendanceRecordToGrid(string FilePath)
        {
            String OneLine;
            String[] OneLineParsed = new String[10];
            int LineNumber = 1;
            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("PunchDate", typeof(string));
                DtFile.Columns.Add("PunchTime");
                DtFile.Columns.Add("In_Out");
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0].ToString(), OneLineParsed[1].ToString(), OneLineParsed[2].ToString(), OneLineParsed[3].ToString());
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    grdAttRecords.Clear();
                    dtAttRecords.Rows.Clear();
                    
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtAttRecords.Rows.Add();

                        dtAttRecords.SetValue("No", LineNumber - 1, LineNumber);
                        dtAttRecords.SetValue("EmpId", LineNumber - 1, dr["EmpID"]);
                        dtAttRecords.SetValue("PunchDate", LineNumber - 1, Convert.ToDateTime(dr["PunchDate"]));
                        dtAttRecords.SetValue("PunchTime", LineNumber - 1, dr["PunchTime"]);
                        dtAttRecords.SetValue("In_Out", LineNumber - 1, dr["In_Out"]);

                        LineNumber++;
                    }
                    grdAttRecords.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Invalid attendance file.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void pullAttRecordFromFileMEPL()
        {

            string fileName = txtfileName.Value.Trim();
            if (fileName == "") return;
            AttendanceRecordToGridMEPL(fileName);

        }

        private void AttendanceRecordToGridMEPL(string FilePath)
        {
            String OneLine;
            String[] OneLineParsed = new String[4];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("PunchDate");
                DtFile.Columns.Add("PunchTime");
                DtFile.Columns.Add("In_Out");

                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[1], "1");
                                // DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    grdAttRecords.Clear();
                    dtAttRecords.Rows.Clear();
                    int LineNumber = 1;
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtAttRecords.Rows.Add();

                        dtAttRecords.SetValue("No", LineNumber - 1, LineNumber);
                        dtAttRecords.SetValue("EmpId", LineNumber - 1, dr["EmpID"]);
                        dtAttRecords.SetValue("PunchDate", LineNumber - 1, Convert.ToDateTime(dr["PunchDate"]));
                        dtAttRecords.SetValue("PunchTime", LineNumber - 1, Convert.ToDateTime(dr["PunchTime"]).ToString("HH:mm"));
                        //dtAttRecords.SetValue("PunchTime", LineNumber - 1, dr["PunchTime"]);
                        dtAttRecords.SetValue("In_Out", LineNumber - 1, dr["In_Out"]);


                        LineNumber++;
                    }
                    grdAttRecords.LoadFromDataSource();
                    //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AttendanceRecordToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AttendanceRecordImportMEPL()
        {
            try
            {
                oApplication.StatusBar.SetText("Importing of Attendance Started... Please wait.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                foreach (DataRow OneRow in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(OneRow["EmpID"]);
                    PunchDate = Convert.ToString(OneRow["PunchDate"]);
                    DateTime DataDate = Convert.ToDateTime(OneRow["PunchDate"]);
                    int chkTempAttandance = 0;
                    chkTempAttandance = (from a in dbHrPayroll.TrnsTempAttendance
                                         where a.EmpID == EmpCode && a.PunchedDate == DataDate
                                         select a).Count();
                    if (chkTempAttandance > 0)
                    {
                        var oCollection = (from a in dbHrPayroll.TrnsTempAttendance
                                           where a.EmpID == EmpCode && a.PunchedDate == DataDate
                                           select a).ToList();
                        foreach (var One in oCollection)
                        {
                            dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(One);
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                foreach (DataRow dr in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(dr["EmpID"]).Trim();
                    InOut = Convert.ToString(dr["In_Out"].ToString());
                    PunchDate = Convert.ToString(dr["PunchDate"]);
                    PunchTime = Convert.ToString(dr["PunchTime"].ToString());

                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == EmpCode
                                   select a).Count();
                    
                    if (chkEmployee != 0)
                    {
                        TrnsTempAttendance oInsert = new TrnsTempAttendance();
                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                            where a.EmpID == EmpCode
                                            select a).FirstOrDefault();
                        if (oEmp == null)
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Employee not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        oInsert.EmpID = Convert.ToString(oEmp.EmpID);
                        if (string.IsNullOrEmpty(PunchDate))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Date can't be found. Please Provide valid Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        if (string.IsNullOrEmpty(PunchTime))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " PunchTime can't be found. Please Provide valid PunchTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        oInsert.In_Out = InOut;
                        oInsert.PunchedDate = Convert.ToDateTime(PunchDate).Date;
                        oInsert.PunchedTime = Convert.ToDateTime(PunchTime.Trim()).ToString("HH:mm");
                        oInsert.FlgProcessed = false;
                        oInsert.CreatedDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;

                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(oInsert);
                    }
                }
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("All Records Successfully Imported.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                dtAttRecords.Rows.Clear();
                grdAttRecords.LoadFromDataSource();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AttendanceRecordImportMEPL : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AttendanceRecordImport1()
        {
            try
            {
                oApplication.StatusBar.SetText("Importing of Attendance Started... Please wait.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                foreach (DataRow OneRow in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(OneRow["EmpID"]);
                    PunchDate = Convert.ToString(OneRow["PunchDate"]);
                    DateTime DataDate = Convert.ToDateTime(OneRow["PunchDate"]);
                    
                    int chkTempAttandance = 0;
                    chkTempAttandance = (from a in dbHrPayroll.TrnsTempAttendance
                                         where a.EmpID == EmpCode && a.PunchedDate == DataDate
                                         select a).Count();
                    if (chkTempAttandance > 0)
                    {
                        var oCollection = (from a in dbHrPayroll.TrnsTempAttendance
                                           where a.EmpID == EmpCode && a.PunchedDate == DataDate
                                           select a).ToList();
                        foreach (var One in oCollection)
                        {
                            dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(One);
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                foreach (DataRow dr in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(dr["EmpID"]).Trim();
                    InOut = Convert.ToString(dr["In_Out"].ToString());
                    PunchDate = Convert.ToString(dr["PunchDate"]);
                    PunchTime = Convert.ToString(dr["PunchTime"].ToString());

                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == EmpCode
                                   select a).Count();

                    if (chkEmployee != 0)
                    {
                        TrnsTempAttendance oInsert = new TrnsTempAttendance();
                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                            where a.EmpID == EmpCode
                                            select a).FirstOrDefault();
                        if (oEmp == null)
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Employee not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            continue;
                        }
                        oInsert.EmpID = Convert.ToString(oEmp.EmpID);
                        if (string.IsNullOrEmpty(PunchDate))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Date can't be found. Please Provide valid Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        if (string.IsNullOrEmpty(PunchTime))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " PunchTime can't be found. Please Provide valid PunchTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        oInsert.In_Out = InOut;
                        oInsert.PunchedDate = Convert.ToDateTime(PunchDate).Date;
                        oInsert.PunchedTime = Convert.ToDateTime(PunchTime.Trim()).ToString("HH:mm");
                        oInsert.FlgProcessed = false;
                        oInsert.CreatedDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;

                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(oInsert);
                    }
                }
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("All Records Successfully Imported.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                dtAttRecords.Rows.Clear();
                grdAttRecords.LoadFromDataSource();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AttendanceRecordImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AttendanceRecordImport()
        {
            try
            {
                oApplication.StatusBar.SetText("Importing of Attendance Started... Please wait.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                foreach (DataRow OneRow in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(OneRow["EmpID"]);
                    PunchDate = Convert.ToString(OneRow["PunchDate"]);
                    DateTime DataDate = Convert.ToDateTime(OneRow["PunchDate"]);

                    var oAttendanceRegister = (from a in dbHrPayroll.TrnsAttendanceRegister
                                               where a.MstEmployee.EmpID == EmpCode && a.Date == DataDate
                                               select a).FirstOrDefault();
                    if (oAttendanceRegister != null)
                    {
                        if (!Convert.ToBoolean(oAttendanceRegister.FlgPosted))
                        {
                            oAttendanceRegister.Processed = false;
                        }
                        else
                        {
                            MsgWarning("Posted Attendance can't have Imported data.");
                        }
                    }

                    int chkTempAttandance = 0;
                    chkTempAttandance = (from a in dbHrPayroll.TrnsTempAttendance
                                         where a.EmpID == EmpCode && a.PunchedDate == DataDate
                                         select a).Count();
                    if (chkTempAttandance > 0)
                    {
                        var oCollection = (from a in dbHrPayroll.TrnsTempAttendance
                                           where a.EmpID == EmpCode && a.PunchedDate == DataDate
                                           select a).ToList();
                        foreach (var One in oCollection)
                        {
                            dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(One);
                        }
                    }
                    //int chkAttendanceRegister = 0;
                    
                }
                dbHrPayroll.SubmitChanges();
                foreach (DataRow dr in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(dr["EmpID"]).Trim();
                    InOut = Convert.ToString(dr["In_Out"].ToString());
                    PunchDate = Convert.ToString(dr["PunchDate"]);
                    PunchTime = Convert.ToString(dr["PunchTime"].ToString());

                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == EmpCode
                                   select a).Count();

                    if (chkEmployee != 0)
                    {
                        TrnsTempAttendance oInsert = new TrnsTempAttendance();
                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                            where a.EmpID == EmpCode
                                            select a).FirstOrDefault();
                        if (oEmp == null)
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Employee not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            continue;
                        }
                        oInsert.EmpID = Convert.ToString(oEmp.EmpID);
                        if (string.IsNullOrEmpty(PunchDate))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Date can't be found. Please Provide valid Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        if (string.IsNullOrEmpty(PunchTime))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " PunchTime can't be found. Please Provide valid PunchTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }

                        oInsert.In_Out = InOut;
                        oInsert.PunchedDate = Convert.ToDateTime(PunchDate).Date;
                        oInsert.PunchedTime = Convert.ToDateTime(PunchTime.Trim()).ToString("HH:mm");
                        oInsert.FlgProcessed = false;
                        oInsert.CreatedDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;

                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(oInsert);
                    }
                }
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("All Records Successfully Imported.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                dtAttRecords.Rows.Clear();
                grdAttRecords.LoadFromDataSource();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AttendanceRecordImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AttendanceRecordImportDuplicationofAttendance()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    EmpCode = Convert.ToString(dr["EmpID"]);
                    InOut = Convert.ToString(dr["In_Out"].ToString());
                    PunchDate = Convert.ToString(dr["PunchDate"]);
                    PunchTime = Convert.ToString(dr["PunchTime"].ToString());

                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == (dr["EmpID"].ToString())
                                   select a).Count();
                    if (chkEmployee != 0)
                    {
                        TrnsTempAttendance oInsert = new TrnsTempAttendance();
                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                            where a.EmpID == (dr["EmpID"].ToString().Trim())
                                            select a).FirstOrDefault();
                        if (oEmp == null)
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Employee not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        oInsert.EmpID = Convert.ToString(oEmp.EmpID);
                        if (string.IsNullOrEmpty(PunchDate))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " Date can't be found. Please Provide valid Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        if (string.IsNullOrEmpty(PunchTime))
                        {
                            oApplication.StatusBar.SetText(EmpCode + " PunchTime can't be found. Please Provide valid PunchTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        if (!string.IsNullOrEmpty(InOut))
                        {
                            InOut = InOut.Trim().ToUpper();
                            switch (InOut)
                            {
                                case "01":
                                case "1":
                                case "02":
                                case "2":
                                case "I":
                                case "U":
                                case "IN":
                                case "OUT":
                                    break;
                                default:
                                    oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode + " for Date " + PunchDate, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                            }
                        }
                        if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "01")
                        {
                            InOut = "1";
                        }
                        else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "02")
                        {
                            InOut = "2";
                        }
                        else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "I")
                        {
                            InOut = "1";
                        }
                        else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "U")
                        {
                            InOut = "2";
                        }
                        else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "IN")
                        {
                            InOut = "1";
                        }
                        else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "OUT")
                        {
                            InOut = "2";
                        }
                        oInsert.In_Out = InOut;
                        oInsert.PunchedDate = Convert.ToDateTime(PunchDate).Date;
                        oInsert.PunchedTime = Convert.ToDateTime(PunchTime.Trim()).ToString("HH:mm");

                        oInsert.CreatedDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;

                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AttendanceRecordImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ClearAllRecords()
        {
            try
            {
                txtfileName.Value = string.Empty;

                grdAttRecords.Clear();
                //dtAttRecords.LoadFromDataSource();
                var oCollection = (from a in dbHrPayroll.TrnsTempAttendance select a).ToList();
                foreach (var One in oCollection)
                {
                    dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(One);
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Clear All Records Msg : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
