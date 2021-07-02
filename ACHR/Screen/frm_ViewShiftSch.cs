using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Globalization;


namespace ACHR.Screen
{
    class frm_ViewShiftSch : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btnSearch, btnCancel,btnOK;
        SAPbouiCOM.Item IbtnOK;
        SAPbouiCOM.EditText txReqBy, txEmpCode, txtFromDate, txtToDate;
        SAPbouiCOM.DataTable dtScheduledShift;
        SAPbouiCOM.Matrix grdSceduledShift;
        SAPbouiCOM.Columns oColumns;        
        SAPbouiCOM.Column oColumn, clNo, clDate, clShiftName, clTimeIn, clTimeOut, clWorkHours;
        SAPbouiCOM.Button btId;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1288", false);  // Next Record
                oForm.EnableMenu("1289", false);  // Pevious Record
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();
                IbtnOK.Visible = false;
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanRequest Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {                    
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "btSrch":
                        GetScheduledShift();
                        break; 
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txEmpCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txEmpCode.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            } 
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

        }
        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btnSearch = oForm.Items.Item("btSrch").Specific;               
                btnCancel = oForm.Items.Item("2").Specific;               
                btnOK = oForm.Items.Item("1").Specific;
                IbtnOK = oForm.Items.Item("1");

                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txReqBy = oForm.Items.Item("txtEmpN").Specific;
                txReqBy.DataBind.SetBound(true, "", "txtEmpN");

                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txEmpCode = oForm.Items.Item("txtEmpC").Specific;
                txEmpCode.DataBind.SetBound(true, "", "txtEmpC");
               

                //Initializing ComboBxes              

                oForm.DataSources.UserDataSources.Add("txtFdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate = oForm.Items.Item("txtFdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "txtFdt");

                oForm.DataSources.UserDataSources.Add("txtTdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate = oForm.Items.Item("txtTdt").Specific;
                txtToDate.DataBind.SetBound(true, "", "txtTdt");               

                InitiallizegridMatrix();

                string loginUserId = oCompany.UserName;
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
                dtScheduledShift = oForm.DataSources.DataTables.Add("ShiftSchedule");
                dtScheduledShift.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtScheduledShift.Columns.Add("Date", SAPbouiCOM.BoFieldsType.ft_Date);
                dtScheduledShift.Columns.Add("Shiftname", SAPbouiCOM.BoFieldsType.ft_Text);
                dtScheduledShift.Columns.Add("TimeIN", SAPbouiCOM.BoFieldsType.ft_Text);
                dtScheduledShift.Columns.Add("TimeOut", SAPbouiCOM.BoFieldsType.ft_Text);
                dtScheduledShift.Columns.Add("WorkHours", SAPbouiCOM.BoFieldsType.ft_Text);

                grdSceduledShift = (SAPbouiCOM.Matrix)oForm.Items.Item("grdShft").Specific;
                oColumns = (SAPbouiCOM.Columns)grdSceduledShift.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "No");

                oColumn = oColumns.Item("cldt");
                clDate = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "Date");

                oColumn = oColumns.Item("clshft");
                clShiftName = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "Shiftname");

                oColumn = oColumns.Item("clTin");
                clTimeIn = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "TimeIN");

                oColumn = oColumns.Item("clTOut");
                clTimeOut = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "TimeOut");

                oColumn = oColumns.Item("clWhrs");
                clWorkHours = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "WorkHours");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void picEmp()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empLoan", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for View Shift Schedule");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txEmpCode.Value = st.Rows[0][0].ToString();
                if (!string.IsNullOrEmpty(txEmpCode.Value))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txEmpCode.Value select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                    }
                }
            }
        }

        private void GetScheduledShift()
        {
            string shiftTimeIn = "";
            string shiftTimeOut = "";
            string shiftHours = "";
            string shiftName = "";
            int RecordCounter = 0;
            try
            {
                DateTime startDate = DateTime.MinValue;
                DateTime EndDate = DateTime.MinValue;
                startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
               
                if (string.IsNullOrEmpty(txEmpCode.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Employee(s) First", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtFromDate.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Shift From Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtToDate.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Shift To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (EndDate < startDate)
                {
                    oApplication.StatusBar.SetText("Please select Proper date range, end date is greater then start date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                {
                    dtScheduledShift.Rows.Clear();
                    if (!string.IsNullOrEmpty(txEmpCode.Value))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID ==txEmpCode.Value.Trim() select a).FirstOrDefault();
                        if (getEmp != null)
                        {
                            int EmpId = getEmp.ID;
                            if (EmpId > 0)
                            {
                                //DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                //DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                {
                                    var AttendanceRegister = dbHrPayroll.TrnsAttendanceRegister.Where(atr => atr.Date == x && atr.EmpID == EmpId).FirstOrDefault();
                                    if (AttendanceRegister != null)
                                    {
                                        string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                        shiftName = string.IsNullOrEmpty(AttendanceRegister.MstShifts.Description) ? "" : AttendanceRegister.MstShifts.Description;
                                        var ShiftDetail = dbHrPayroll.MstShiftDetails.Where(S => S.Day == dayofWeeks && S.ShiftID == AttendanceRegister.MstShifts.Id).FirstOrDefault();
                                        if (ShiftDetail != null)
                                        {
                                            shiftTimeIn = ShiftDetail.StartTime;
                                            shiftTimeOut = ShiftDetail.EndTime;
                                            shiftHours = ShiftDetail.Duration;
                                        }
                                        dtScheduledShift.Rows.Add(1);
                                        dtScheduledShift.SetValue("No", RecordCounter, RecordCounter + 1);
                                        dtScheduledShift.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                        dtScheduledShift.SetValue("Shiftname", RecordCounter, shiftName);
                                        dtScheduledShift.SetValue("TimeIN", RecordCounter, shiftTimeIn);
                                        dtScheduledShift.SetValue("TimeOut", RecordCounter, shiftTimeOut);
                                        dtScheduledShift.SetValue("WorkHours", RecordCounter, shiftHours);

                                        RecordCounter++;
                                    }                                   
                                }
                                if (dtScheduledShift.Rows.Count > 0)
                                {
                                    grdSceduledShift.LoadFromDataSource();
                                }
                                else
                                {
                                    dtScheduledShift.Rows.Clear();
                                    grdSceduledShift.LoadFromDataSource();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            if (!string.IsNullOrEmpty(txEmpCode.Value))
            {
                SearchKeyVal.Add("EmpID", txEmpCode.Value.ToString());
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empPick";
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
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txEmpCode.Value = Program.EmpID;
                    if (!string.IsNullOrEmpty(txEmpCode.Value))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txEmpCode.Value.Trim() select a).FirstOrDefault();
                        if (getEmp != null)
                        {
                            txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        }
                    }                  
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

    }
}
