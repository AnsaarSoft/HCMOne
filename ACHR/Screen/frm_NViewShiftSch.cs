using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Globalization;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_NViewShiftSch : HRMSBaseForm
    {

        #region Variables

        SAPbouiCOM.Button btnSearch, btnCancel, btnSave, btnOK;
        SAPbouiCOM.EditText txReqBy, txEmpCode, txtFromDate, txtToDate;
        SAPbouiCOM.ComboBox cbDay;
        SAPbouiCOM.DataTable dtScheduledShift;
        SAPbouiCOM.Matrix grdSceduledShift;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clDate, clShiftName, clWorkHours, clIsOff, clDay;
        SAPbouiCOM.Button btId;
        SAPbouiCOM.Item IbtnOK;

        #endregion

        #region SAP Events

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
                        //if (Convert.ToBoolean(Program.systemInfo.FlgRetailRules1))
                        //{
                        GetScheduledShiftStandard();
                        //}
                        //else
                        //{
                        //    GetScheduledShift();
                        //}
                        break;
                    case "btnSave":
                        //if (Convert.ToBoolean(Program.systemInfo.FlgRetailRules1))
                        //{
                            SaveRecordsStandard();
                        //}
                        //else
                        //{
                        //    SaveRecords();
                        //}
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

        public override void etAfterLostFocus(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            try
            {
                if (pVal.ItemUID == "txtEmpC")
                {
                    if (!string.IsNullOrEmpty(txEmpCode.Value))
                    {
                        SetEmpValuesLostFocus();
                    }
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

        }
        #endregion

        #region Functions

        public void InitiallizeForm()
        {
            try
            {
                btnSearch = oForm.Items.Item("btSrch").Specific;
                btnSave = oForm.Items.Item("btnSave").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                btnOK = oForm.Items.Item("2").Specific;
                IbtnOK = oForm.Items.Item("1");
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txReqBy = oForm.Items.Item("txtEmpN").Specific;
                txReqBy.DataBind.SetBound(true, "", "txtEmpN");

                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txEmpCode = oForm.Items.Item("txtEmpC").Specific;
                txEmpCode.DataBind.SetBound(true, "", "txtEmpC");


                oForm.DataSources.UserDataSources.Add("cbDay", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDay = oForm.Items.Item("cbDay").Specific;
                cbDay.DataBind.SetBound(true, "", "cbDay");


                //Initializing ComboBxes              

                oForm.DataSources.UserDataSources.Add("txtFdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate = oForm.Items.Item("txtFdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "txtFdt");

                oForm.DataSources.UserDataSources.Add("txtTdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate = oForm.Items.Item("txtTdt").Specific;
                txtToDate.DataBind.SetBound(true, "", "txtTdt");

                InitiallizegridMatrix();

                FillDaysInCombo();

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
                dtScheduledShift.Columns.Add("WorkHours", SAPbouiCOM.BoFieldsType.ft_Text);
                dtScheduledShift.Columns.Add("flgIsOffDay", SAPbouiCOM.BoFieldsType.ft_Text);
                dtScheduledShift.Columns.Add("Day", SAPbouiCOM.BoFieldsType.ft_Text);

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

                oColumn = oColumns.Item("clWhrs");
                clWorkHours = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "WorkHours");

                oColumn = oColumns.Item("flgOff");
                clIsOff = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "flgIsOffDay");

                oColumn = oColumns.Item("clDay");
                clDay = oColumn;
                oColumn.DataBind.Bind("ShiftSchedule", "Day");

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
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                      //where a.EmpID == txEmpCode.Value
                                  where a.EmpID == txEmpCode.Value
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                    }
                }
            }
        }

        private void GetScheduledShift()
        {
            bool flgOffDay = false;
            string shiftHours = "";
            string shiftName = "";
            int RecordCounter = 0;
            try
            {
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
                if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                {
                    dtScheduledShift.Rows.Clear();
                    if (!string.IsNullOrEmpty(txEmpCode.Value))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                          //where a.EmpID.Contains(txEmpCode.Value)
                                      where a.EmpID == (txEmpCode.Value)
                                      select a).FirstOrDefault();
                        if (getEmp != null)
                        {
                            int EmpId = getEmp.ID;
                            if (EmpId > 0)
                            {
                                DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                {
                                    var AttendanceRegister = dbHrPayroll.TrnsAttendanceRegisterTS.Where(atr => atr.Date == x && atr.EmpID == EmpId).FirstOrDefault();
                                    if (AttendanceRegister != null)
                                    {
                                        string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                        shiftName = string.IsNullOrEmpty(AttendanceRegister.MstShifts.Description) ? "" : AttendanceRegister.MstShifts.Description;
                                        shiftHours = string.IsNullOrEmpty(AttendanceRegister.ShiftHours) ? "" : AttendanceRegister.ShiftHours;
                                        flgOffDay = AttendanceRegister.FlgOffDay.Value;
                                        if (!string.IsNullOrEmpty(cbDay.Value) && cbDay.Value.Trim() != "-1")
                                        {
                                            if (cbDay.Value.Trim() == dayofWeeks)
                                            {
                                                dtScheduledShift.Rows.Add(1);
                                                dtScheduledShift.SetValue("No", RecordCounter, RecordCounter + 1);
                                                dtScheduledShift.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                                dtScheduledShift.SetValue("Shiftname", RecordCounter, shiftName);
                                                dtScheduledShift.SetValue("WorkHours", RecordCounter, shiftHours);
                                                dtScheduledShift.SetValue("Day", RecordCounter, dayofWeeks);
                                                dtScheduledShift.SetValue("flgIsOffDay", RecordCounter, flgOffDay == true ? "Y" : "N");
                                                RecordCounter++;
                                            }
                                        }
                                        else
                                        {
                                            dtScheduledShift.Rows.Add(1);
                                            dtScheduledShift.SetValue("No", RecordCounter, RecordCounter + 1);
                                            dtScheduledShift.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                            dtScheduledShift.SetValue("Shiftname", RecordCounter, shiftName);
                                            dtScheduledShift.SetValue("WorkHours", RecordCounter, shiftHours);
                                            dtScheduledShift.SetValue("Day", RecordCounter, dayofWeeks);
                                            dtScheduledShift.SetValue("flgIsOffDay", RecordCounter, flgOffDay == true ? "Y" : "N");
                                            RecordCounter++;
                                        }
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

        private void GetScheduledShiftStandard()
        {
            bool flgOffDay = false;
            string shiftHours = "";
            string shiftName = "";
            int RecordCounter = 0;
            try
            {
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
                if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                {
                    dtScheduledShift.Rows.Clear();
                    if (!string.IsNullOrEmpty(txEmpCode.Value))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID == txEmpCode.Value.Trim()
                                      select a).FirstOrDefault();
                        if (getEmp != null)
                        {
                            int EmpId = getEmp.ID;
                            if (EmpId > 0)
                            {
                                DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                {
                                    var AttendanceRegister = (from a in dbHrPayroll.TrnsAttendanceRegister
                                                              where a.Date == x
                                                              && a.EmpID == EmpId
                                                              select a).FirstOrDefault();
                                    if (AttendanceRegister != null)
                                    {
                                        string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                        shiftName = string.IsNullOrEmpty(AttendanceRegister.MstShifts.Description) ? "" : AttendanceRegister.MstShifts.Description;
                                        shiftHours = string.IsNullOrEmpty(AttendanceRegister.WorkHour) ? "00:00" : AttendanceRegister.WorkHour;
                                        flgOffDay = Convert.ToBoolean(AttendanceRegister.FlgOffDay);
                                        if (!string.IsNullOrEmpty(cbDay.Value) && cbDay.Value.Trim() != "-1")
                                        {
                                            if (cbDay.Value.Trim() == dayofWeeks)
                                            {
                                                dtScheduledShift.Rows.Add(1);
                                                dtScheduledShift.SetValue("No", RecordCounter, RecordCounter + 1);
                                                dtScheduledShift.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                                dtScheduledShift.SetValue("Shiftname", RecordCounter, shiftName);
                                                dtScheduledShift.SetValue("WorkHours", RecordCounter, shiftHours);
                                                dtScheduledShift.SetValue("Day", RecordCounter, dayofWeeks);
                                                dtScheduledShift.SetValue("flgIsOffDay", RecordCounter, flgOffDay == true ? "Y" : "N");
                                                RecordCounter++;
                                            }
                                        }
                                        else
                                        {
                                            dtScheduledShift.Rows.Add(1);
                                            dtScheduledShift.SetValue("No", RecordCounter, RecordCounter + 1);
                                            dtScheduledShift.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                            dtScheduledShift.SetValue("Shiftname", RecordCounter, shiftName);
                                            dtScheduledShift.SetValue("WorkHours", RecordCounter, shiftHours);
                                            dtScheduledShift.SetValue("Day", RecordCounter, dayofWeeks);
                                            dtScheduledShift.SetValue("flgIsOffDay", RecordCounter, flgOffDay == true ? "Y" : "N");
                                            RecordCounter++;
                                        }
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
                logger(ex);
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
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                          //Wrong selection of employee due to CONTAINS
                                      where a.EmpID == txEmpCode.Value
                                      select a).FirstOrDefault();
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

        private void SetEmpValuesLostFocus()
        {
            try
            {
                if (!string.IsNullOrEmpty(txEmpCode.Value))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == txEmpCode.Value.Trim()
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void SaveRecords()
        {
            string strDate = "";
            bool IsOffDay = false;
            DateTime dtDate = DateTime.MinValue;
            var getEmp = (from a in dbHrPayroll.MstEmployee
                              //where a.EmpID.Contains(txEmpCode.Value)
                              //Due to not OFFDAY assigning change contain to Equal (02-Aug-16)
                          where a.EmpID == (txEmpCode.Value)
                          select a).FirstOrDefault();
            if (getEmp != null)
            {
                try
                {
                    for (int i = 1; i < grdSceduledShift.RowCount + 1; i++)
                    {
                        strDate = (grdSceduledShift.Columns.Item("cldt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        IsOffDay = (grdSceduledShift.Columns.Item("flgOff").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        dtDate = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        var DateRecord = dbHrPayroll.TrnsAttendanceRegisterTS.Where(a => a.EmpID == getEmp.ID && a.Date == dtDate).FirstOrDefault();
                        if (DateRecord != null)
                        {
                            DateRecord.FlgOffDay = IsOffDay;
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    dtScheduledShift.Rows.Clear();
                    grdSceduledShift.LoadFromDataSource();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
                catch (Exception Ex)
                {
                    oApplication.StatusBar.SetText("Form: frm_NViewShiftSch Function: SaveRecords Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
        }

        private void SaveRecordsStandard()
        {
            try
            {
                string strDate = "";
                bool IsOffDay = false;
                DateTime dtDate = DateTime.MinValue;
                var oEmp = (from a in dbHrPayroll.MstEmployee
                            //where a.EmpID.Contains(txEmpCode.Value)
                            //Due to not OFFDAY assigning change contain to Equal (02-Aug-16)
                            where a.EmpID == txEmpCode.Value.Trim()
                            select a).FirstOrDefault();
                if (oEmp != null)
                {
                    for (int i = 1; i < grdSceduledShift.RowCount + 1; i++)
                    {
                        strDate = (grdSceduledShift.Columns.Item("cldt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        IsOffDay = (grdSceduledShift.Columns.Item("flgOff").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        dtDate = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        var DateRecord = (from a in dbHrPayroll.TrnsAttendanceRegister
                                          where a.Date == dtDate
                                          && a.EmpID == oEmp.ID
                                          select a).FirstOrDefault();
                        if (DateRecord != null)
                        {
                            DateRecord.FlgOffDay = IsOffDay;
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    MsgSuccess("Records successfully updated.");
                    dtScheduledShift.Rows.Clear();
                    grdSceduledShift.LoadFromDataSource();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillDaysInCombo()
        {
            try
            {
                cbDay.ValidValues.Add("-1", "[Select One]");
                cbDay.ValidValues.Add("Sunday", "Sunday");
                cbDay.ValidValues.Add("Monday", "Monday");
                cbDay.ValidValues.Add("Tuesday", "Tuesday");
                cbDay.ValidValues.Add("Wednesday", "Wednesday");
                cbDay.ValidValues.Add("Thursday", "Thursday");
                cbDay.ValidValues.Add("Friday", "Friday");
                cbDay.ValidValues.Add("Saturday", "Saturday");
                //var Shifts = CultureInfo.CurrentCulture.DateTimeFormat.DayNames.ToList();
                //if (Shifts != null && Shifts.Count() > 0)
                //{
                //    foreach (MstShifts empShift in Shifts)
                //    {
                //        cbDay.ValidValues.Add(Convert.ToString(empShift.Id), Convert.ToString(empShift.Description));
                //    }
                //}
                cbDay.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillShiftsInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        #endregion

    }
}
