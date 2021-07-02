using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using System.Globalization;

namespace ACHR.Screen
{
    class frm_ShftSchTex : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btnSave, btnSerch, btCancel, btnClear;
        SAPbouiCOM.EditText txtEmpIdFrom, txtEmpIdTo, txtFromDate, txtToDate;
        SAPbouiCOM.ComboBox cb_Location, cb_depart, cb_deignation, cb_shift;
        SAPbouiCOM.DataTable dtEmployees;
        SAPbouiCOM.Matrix grdEmployees;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, EmpCode, EmpName, Desig, Depart, Location, isSel;

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
                FillShiftsInCombo();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                        //picEmpFrom();
                        OpenNewSearchForm();
                        break;
                    case "btId2":
                        //picEmpTo();
                        OpenNewSearchFormTo();
                        break;
                    case "btnSerc":
                        PopulateGridWithFilterExpression();
                        break;
                    case "btnClear":
                        ClearControls();
                        break;
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                            SaveRecord();
                        break;
                    case "grd_Emp":
                        if (pVal.ColUID == "isSel" && pVal.Row == 0)
                        {
                            selectAllProcess();
                        }
                        break;
                    case "2":

                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            SetEmpValues();
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btnSerch = oForm.Items.Item("btnSerc").Specific;
                btnClear = oForm.Items.Item("btnClear").Specific;
                btnSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("2").Specific;

                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("empfrm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpIdFrom = oForm.Items.Item("empfrm").Specific;
                txtEmpIdFrom.DataBind.SetBound(true, "", "empfrm");

                oForm.DataSources.UserDataSources.Add("empTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpIdTo = oForm.Items.Item("empTo").Specific;
                txtEmpIdTo.DataBind.SetBound(true, "", "empTo");

                cb_depart = oForm.Items.Item("cb_dpt").Specific;
                FillDepartmentInCombo();
                cb_deignation = oForm.Items.Item("cb_desg").Specific;
                FillDesignationInCombo();
                cb_Location = oForm.Items.Item("cb_loc").Specific;
                FillEmpLocationInCombo();
                cb_shift = oForm.Items.Item("cb_shft").Specific;
                //Initializing Date Fields

                oForm.DataSources.UserDataSources.Add("frmdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate = oForm.Items.Item("frmdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "frmdt");

                oForm.DataSources.UserDataSources.Add("todt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate = oForm.Items.Item("todt").Specific;
                txtToDate.DataBind.SetBound(true, "", "todt");

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
                dtEmployees = oForm.DataSources.DataTables.Add("Employees");
                dtEmployees.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmployees.Columns.Add("EmpCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("Designation", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("Department", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("Location", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("isSel", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);

                grdEmployees = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Emp").Specific;
                oColumns = (SAPbouiCOM.Columns)grdEmployees.Columns;


                oColumn = oColumns.Item("No");
                clNo = oColumn;
                oColumn.DataBind.Bind("Employees", "No");

                oColumn = oColumns.Item("EmpCode");
                EmpCode = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpCode");

                oColumn = oColumns.Item("EmpName");
                EmpName = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpName");

                oColumn = oColumns.Item("Desig");
                Desig = oColumn;
                oColumn.DataBind.Bind("Employees", "Designation");

                oColumn = oColumns.Item("Depart");
                Depart = oColumn;
                oColumn.DataBind.Bind("Employees", "Department");

                oColumn = oColumns.Item("Location");
                Location = oColumn;
                oColumn.DataBind.Bind("Employees", "Location");

                oColumn = oColumns.Item("isSel");
                isSel = oColumn;
                oColumn.DataBind.Bind("Employees", "isSel");



            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillDepartmentInCombo()
        {
            try
            {
                var Departments = from a in dbHrPayroll.MstDepartment select a;
                cb_depart.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstDepartment Dept in Departments)
                {
                    cb_depart.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
                cb_depart.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillDepartmentInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillDesignationInCombo()
        {
            try
            {
                var Designation = from a in dbHrPayroll.MstDesignation select a;
                cb_deignation.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstDesignation Desig in Designation)
                {
                    cb_deignation.ValidValues.Add(Convert.ToString(Desig.Id), Convert.ToString(Desig.Name));
                }
                cb_deignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillDesignationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillEmpLocationInCombo()
        {
            try
            {
                var EmpLocation = from a in dbHrPayroll.MstLocation select a;
                cb_Location.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstLocation empLocation in EmpLocation)
                {
                    cb_Location.ValidValues.Add(Convert.ToString(empLocation.Id), Convert.ToString(empLocation.Name));
                }
                cb_Location.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillEmpLocationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void FillShiftsInCombo()
        {
            try
            {
                cb_shift.ValidValues.Add("-1", "[Select One]");
                var Shifts = dbHrPayroll.MstShifts.Where(s => s.StatusShift == true).ToList();
                if (Shifts != null && Shifts.Count > 0)
                {
                    foreach (MstShifts empShift in Shifts)
                    {
                        cb_shift.ValidValues.Add(Convert.ToString(empShift.Id), Convert.ToString(empShift.Description));
                    }
                }
                cb_shift.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillShiftsInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void PopulateGridWithFilterExpression()
        {
            Int16 i = 0;
            try
            {

                var Data = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID > 0).ToList();


                if (txtEmpIdFrom.Value != string.Empty && txtEmpIdTo.Value != string.Empty)
                {
                    var EMPFrom = dbHrPayroll.MstEmployee.Where(emp => emp.EmpID == txtEmpIdFrom.Value).FirstOrDefault();
                    var EmpTo = dbHrPayroll.MstEmployee.Where(emp => emp.EmpID == txtEmpIdTo.Value).FirstOrDefault();
                    if (EMPFrom == null || EmpTo == null)
                    {
                        oApplication.StatusBar.SetText("Please enter valid EmpID", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    int intEmpIdFrom = EMPFrom.ID;
                    int intEmpIdTo = EmpTo.ID;
                    Data = Data.Where(e => e.ID >= intEmpIdFrom && e.ID <= intEmpIdTo).ToList();
                }
                if (cb_Location.Value != "0" && cb_Location.Value != string.Empty)
                {
                    Data = Data.Where(e => e.Location == Convert.ToInt32(cb_Location.Value)).ToList();
                }
                if (cb_depart.Value != "0" && cb_depart.Value != string.Empty)
                {
                    Data = Data.Where(e => e.DepartmentID == Convert.ToInt32(cb_depart.Value)).ToList();
                }
                if (cb_deignation.Value != "0" && cb_deignation.Value != string.Empty)
                {
                    Data = Data.Where(e => e.DesignationID == Convert.ToInt32(cb_deignation.Value)).ToList();
                }
                if (Data != null && Data.Count > 0)
                {
                    dtEmployees.Rows.Clear();
                    dtEmployees.Rows.Add(Data.Count());
                    foreach (var EMP in Data)
                    {
                        dtEmployees.SetValue("No", i, i + 1);
                        dtEmployees.SetValue("EmpCode", i, EMP.EmpID);
                        dtEmployees.SetValue("EmpName", i, EMP.FirstName + " " + EMP.MiddleName + " " + EMP.LastName);
                        dtEmployees.SetValue("Designation", i, string.IsNullOrEmpty(EMP.DesignationName) ? "" : EMP.DesignationName);
                        dtEmployees.SetValue("Department", i, string.IsNullOrEmpty(EMP.DepartmentName) ? "" : EMP.DepartmentName);
                        dtEmployees.SetValue("Location", i, string.IsNullOrEmpty(EMP.LocationName) ? "" : EMP.LocationName);
                        i++;
                    }
                    grdEmployees.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: PopulateGridWithFilterExpression Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void ClearControls()
        {
            try
            {
                txtEmpIdFrom.Value = string.Empty;
                txtEmpIdTo.Value = string.Empty;
                cb_deignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cb_depart.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cb_Location.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cb_shift.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtEmployees.Rows.Clear();
                grdEmployees.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void SaveRecord()
        {
            double ShiftDays = 0;
            SAPbouiCOM.ProgressBar prog = null;
            try
            {
                if (dtEmployees != null && dtEmployees.Rows.Count > 0)
                {
                    string strEMPcode = "";
                    int intEmpID = 0;
                    int? PayrollID = 0;
                    DateTime startDate = DateTime.MinValue;
                    DateTime EndDate = DateTime.MinValue;
                    string shiftHours = "";
                    int intPeriodId = 0;
                    if (string.IsNullOrEmpty(txtFromDate.Value) && string.IsNullOrEmpty(txtToDate.Value))
                    {
                        oApplication.StatusBar.SetText("Please Select Shift Schedule From and To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (string.IsNullOrEmpty(cb_shift.Value))
                    {
                        oApplication.StatusBar.SetText("Please Select Shift", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (!string.IsNullOrEmpty(cb_shift.Value) && Convert.ToInt32(cb_shift.Value) < 1)
                    {
                        oApplication.StatusBar.SetText("Please Select Shift", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                    {
                        DateTime dtFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime dtTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        ShiftDays = ((dtTo.Subtract(dtFrom)).TotalDays + 1);
                        if (ShiftDays <= 0)
                        {
                            oApplication.StatusBar.SetText("Shift From date can't be greater than shift To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    if (txtFromDate.Value != string.Empty && txtToDate.Value != string.Empty)
                    {
                        int totalEmps = dtEmployees.Rows.Count;
                        if (totalEmps > 0)
                        {
                            int TotalRecord = Convert.ToInt32(totalEmps);
                            prog = oApplication.StatusBar.CreateProgressBar("Scheduling Shift(s) for Selected Employee(s)", TotalRecord, false);
                            prog.Value = 0;
                        }
                        startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        for (int i = 0; i < dtEmployees.Rows.Count; i++)
                        {
                            System.Windows.Forms.Application.DoEvents();
                            prog.Value += 1;

                            bool sel2 = (grdEmployees.Columns.Item("isSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                            if (sel2)
                            {
                                strEMPcode = Convert.ToString(dtEmployees.GetValue("EmpCode", i));
                                var EmpDATA = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEMPcode).FirstOrDefault();
                                if (EmpDATA == null)
                                {
                                    oApplication.StatusBar.SetText("Employee Record(s) can't be found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                else
                                {
                                    intEmpID = EmpDATA.ID;
                                    string EmpPayroll = Convert.ToString(EmpDATA.PayrollID);
                                    if (string.IsNullOrEmpty(EmpPayroll))
                                    {
                                        oApplication.StatusBar.SetText("Please Attach Payroll To Employee " + EmpDATA.EmpID + " Then Process Shift Schedualr", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        return;
                                    }
                                    PayrollID = EmpDATA.PayrollID;
                                }
                                if (intEmpID > 0)
                                {
                                    for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                    {
                                        string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                        var ShiftDetail = dbHrPayroll.MstShiftDetails.Where(S => S.Day == dayofWeeks && S.ShiftID == Convert.ToInt32(cb_shift.Value)).FirstOrDefault();
                                        if (ShiftDetail != null)
                                        {
                                            shiftHours = ShiftDetail.Duration;
                                        }
                                        var PeriodId = dbHrPayroll.CfgPeriodDates.Where(pd => pd.StartDate <= x && x <= pd.EndDate && pd.PayrollId == PayrollID).FirstOrDefault();
                                        if (PeriodId != null)
                                        {
                                            intPeriodId = PeriodId.ID;
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("PayrollID for Selected Date Range Can't be found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            return;
                                        }
                                        TrnsTextileGroupAttendanceReg attRegOld = dbHrPayroll.TrnsTextileGroupAttendanceReg.Where(atr => atr.EmpID == intEmpID && atr.Date == x).FirstOrDefault();
                                        if (attRegOld != null && attRegOld.FlgProcessed == true)
                                        {
                                            continue;
                                        }
                                        if (attRegOld != null && attRegOld.FlgOffDay == true)
                                        {
                                            continue;
                                        }
                                        if (attRegOld != null)
                                        {
                                            attRegOld.PeriodID = intPeriodId;
                                            attRegOld.ShiftID = Convert.ToInt32(cb_shift.Value);
                                            attRegOld.UpdatedDate = DateTime.Now;
                                            attRegOld.FlgOffDay = false;
                                            attRegOld.ShiftHours = shiftHours;
                                            attRegOld.DayName = dayofWeeks;
                                        }
                                        else
                                        {                                            
                                            TrnsTextileGroupAttendanceReg attendance = new TrnsTextileGroupAttendanceReg();
                                            attendance.EmpID = intEmpID;
                                            attendance.PeriodID = intPeriodId;
                                            attendance.DayName = dayofWeeks;
                                            attendance.Date = x;
                                            attendance.ShiftID = Convert.ToInt32(cb_shift.Value);
                                            attendance.ShiftHours = shiftHours;
                                            attendance.CreatedDate = DateTime.Now;
                                            attendance.CreatedBy = oCompany.UserName;
                                            attendance.FlgProcessed = false;
                                            attendance.FlgPosted = false;
                                            attendance.FlgOffDay = false;
                                            dbHrPayroll.TrnsTextileGroupAttendanceReg.InsertOnSubmit(attendance);
                                        }
                                    }
                                    dbHrPayroll.SubmitChanges();
                                }
                            }
                        }
                        ClearControls();
                        txtFromDate.Value = "";
                        txtToDate.Value = "";
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Please Select Employee(s) First", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: SaveRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                if (prog != null)
                {
                    prog.Stop();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                }
                prog = null;
            }
        }
        private void picEmpFrom()
        {
            string strSql = sqlString.getSql("empAdvance", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Shift Schedular");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpIdFrom.Value = st.Rows[0][0].ToString();
            }
        }
        private void picEmpTo()
        {
            string strSql = sqlString.getSql("empAdvance", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Shift Schedular");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpIdTo.Value = st.Rows[0][0].ToString();
            }
        }
        private void selectAllProcess()
        {
            try
            {

                oForm.Freeze(true);
                SAPbouiCOM.Column col = grdEmployees.Columns.Item("isSel");

                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {

                        dtEmployees.SetValue("isSel", i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {
                        dtEmployees.SetValue("isSel", i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                grdEmployees.LoadFromDataSource();
                oForm.Freeze(false);
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.FromEmpId = "";
                string comName = "fromSrch";
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
        private void OpenNewSearchFormTo()
        {
            try
            {
                Program.ToEmpId = "";
                string comName = "ToSearch";
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
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.FromEmpId))
                {
                    txtEmpIdFrom.Value = Program.FromEmpId;
                }
                if (!string.IsNullOrEmpty(Program.ToEmpId))
                {
                    txtEmpIdTo.Value = Program.ToEmpId;
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
