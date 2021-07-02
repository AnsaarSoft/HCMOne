using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using System.Globalization;


namespace ACHR.Screen
{
    class frm_ShftSch : HRMSBaseForm
    {
        #region "Variable"

        SAPbouiCOM.Button btnSave, btnSerch, btCancel, btnClear;
        SAPbouiCOM.EditText txtEmpFrom, txtEmpTo, txtFromDate, txtToDate;
        SAPbouiCOM.ComboBox cbLocation, cbDepartment, cbDesignation, cbShift, cbBranch;
        SAPbouiCOM.Item IcbLocation, IcbDepartment, IcbDesignation, IcbShift, IcbBranch;
        SAPbouiCOM.DataTable dtEmployees;
        SAPbouiCOM.Matrix grdEmployees;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, EmpCode, EmpName, Desig, Depart, Location, Branch, isSel;
        SAPbouiCOM.Item itxtEmpFrom, itxtEmpTo, itxtFromDate, itxtToDate;

        Boolean flgEmpFrom, flgEmpTo;

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
                        flgEmpTo = false;
                        flgEmpFrom = true;
                        OpenNewSearchFormFrom();
                        break;
                    case "btId2":
                        flgEmpTo = true;
                        flgEmpFrom = false;
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
            if (flgEmpTo && !flgEmpFrom)
            {
                txtEmpTo.Value = Program.EmpID;
                flgEmpFrom = false;
                flgEmpTo = false;
            }
            if (!flgEmpTo && flgEmpFrom)
            {
                txtEmpFrom.Value = Program.EmpID;
                flgEmpFrom = false;
                flgEmpTo = false;
            }
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
                txtEmpFrom = oForm.Items.Item("empfrm").Specific;
                itxtEmpFrom = oForm.Items.Item("empfrm");
                txtEmpFrom.DataBind.SetBound(true, "", "empfrm");
                txtEmpFrom.TabOrder = 1;

                oForm.DataSources.UserDataSources.Add("empTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpTo = oForm.Items.Item("empTo").Specific;
                itxtEmpTo = oForm.Items.Item("empTo");
                txtEmpTo.DataBind.SetBound(true, "", "empTo");
                txtEmpTo.TabOrder = 2;

                cbLocation = oForm.Items.Item("cb_loc").Specific;
                oForm.DataSources.UserDataSources.Add("cb_loc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLocation.DataBind.SetBound(true, "", "cb_loc");
                IcbLocation = oForm.Items.Item("cb_loc");
                cbLocation.TabOrder = 3;

                oForm.DataSources.UserDataSources.Add("cbBrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbBranch = oForm.Items.Item("cbBrnch").Specific;
                cbBranch.DataBind.SetBound(true, "", "cbBrnch");
                IcbBranch = oForm.Items.Item("cbBrnch");
                cbBranch.TabOrder = 4;

                FillEmpLocationInCombo();
                FillEmpBranchInCombo();

                cbDepartment = oForm.Items.Item("cb_dpt").Specific;
                oForm.DataSources.UserDataSources.Add("cb_dpt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDepartment.DataBind.SetBound(true, "", "cb_dpt");
                IcbDepartment = oForm.Items.Item("cb_dpt");
                cbDepartment.TabOrder = 4;
                FillDepartmentInCombo();

                cbDesignation = oForm.Items.Item("cb_desg").Specific;
                oForm.DataSources.UserDataSources.Add("cb_desg", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDesignation.DataBind.SetBound(true, "", "cb_desg");
                IcbDesignation = oForm.Items.Item("cb_desg");
                cbDesignation.TabOrder = 5;
                FillDesignationInCombo();

                oForm.DataSources.UserDataSources.Add("frmdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate = oForm.Items.Item("frmdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "frmdt");
                txtFromDate.TabOrder = 6;

                oForm.DataSources.UserDataSources.Add("todt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate = oForm.Items.Item("todt").Specific;
                txtToDate.DataBind.SetBound(true, "", "todt");
                txtToDate.TabOrder = 7;

                cbShift = oForm.Items.Item("cb_shft").Specific;
                oForm.DataSources.UserDataSources.Add("cb_shft", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbShift.DataBind.SetBound(true, "", "cb_shft");
                IcbShift = oForm.Items.Item("cb_shft");
                cbShift.TabOrder = 8;

                InitiallizegridMatrix();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                itxtEmpFrom.Click();
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
                var Departments = from a in dbHrPayroll.MstDepartment orderby a.DeptName ascending select a;
                cbDepartment.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstDepartment Dept in Departments)
                {
                    cbDepartment.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
                cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
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
                var Designation = from a in dbHrPayroll.MstDesignation orderby a.Name select a;
                cbDesignation.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstDesignation Desig in Designation)
                {
                    cbDesignation.ValidValues.Add(Convert.ToString(Desig.Id), Convert.ToString(Desig.Name));
                }
                cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
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
                var EmpLocation = from a in dbHrPayroll.MstLocation orderby a.Name ascending select a;
                cbLocation.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstLocation empLocation in EmpLocation)
                {
                    cbLocation.ValidValues.Add(Convert.ToString(empLocation.Id), Convert.ToString(empLocation.Name));
                }
                cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillEmpLocationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void FillEmpBranchInCombo()
        {
            try
            {
                var EmpBranchs = from a in dbHrPayroll.MstBranches orderby a.Name ascending select a;
                cbBranch.ValidValues.Add(Convert.ToString(0), Convert.ToString("[ALL]"));
                foreach (MstBranches empBranch in EmpBranchs)
                {
                    cbBranch.ValidValues.Add(Convert.ToString(empBranch.Id), Convert.ToString(empBranch.Description));
                }
                cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: FillEmpBranchInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void FillShiftsInCombo()
        {
            try
            {
                cbShift.ValidValues.Add("-1", "[Select One]");
                var Shifts = dbHrPayroll.MstShifts.Where(s => s.StatusShift == true).ToList();
                if (Shifts != null && Shifts.Count > 0)
                {
                    foreach (MstShifts empShift in Shifts)
                    {
                        cbShift.ValidValues.Add(Convert.ToString(empShift.Id), Convert.ToString(empShift.Description));
                    }
                }
                cbShift.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
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
                //var Data = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID > 0).ToList();
                var Data = (from e in dbHrPayroll.MstEmployee
                            where e.FlgActive == true
                            && e.PayrollID > 0
                            orderby e.SortOrder
                            ascending
                            select e).ToList();

                if (txtEmpFrom.Value != string.Empty && txtEmpTo.Value != string.Empty)
                {
                    int? intEmpIdFrom = (from a in dbHrPayroll.MstEmployee
                                         where a.EmpID == txtEmpFrom.Value.Trim()
                                         select a.SortOrder).FirstOrDefault();

                    int? intEmpIdTo = (from a in dbHrPayroll.MstEmployee
                                       where a.EmpID == txtEmpTo.Value.Trim()
                                       select a.SortOrder).FirstOrDefault();

                    if (intEmpIdFrom == null) intEmpIdFrom = 0;
                    if (intEmpIdTo == null) intEmpIdTo = 100000;
                    Data = Data.Where(e => e.SortOrder >= intEmpIdFrom && e.SortOrder <= intEmpIdTo).ToList();
                }
                if (cbLocation.Value.Trim() != "0" && cbLocation.Value.Trim() != string.Empty)
                {
                    Data = Data.Where(e => e.Location == Convert.ToInt32(cbLocation.Value)).ToList();
                }
                if (cbDepartment.Value.Trim() != "0" && cbDepartment.Value.Trim() != string.Empty)
                {
                    Data = Data.Where(e => e.DepartmentID == Convert.ToInt32(cbDepartment.Value)).ToList();
                }
                if (cbDesignation.Value.Trim() != "0" && cbDesignation.Value.Trim() != string.Empty)
                {
                    Data = Data.Where(e => e.DesignationID == Convert.ToInt32(cbDesignation.Value)).ToList();
                }
                if (cbBranch.Value.Trim() != "0" && cbBranch.Value.Trim() != string.Empty)
                {
                    Data = Data.Where(e => e.BranchID == Convert.ToInt32(cbBranch.Value)).ToList();
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
                txtEmpFrom.Value = string.Empty;
                txtEmpTo.Value = string.Empty;
                cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbShift.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtEmployees.Rows.Clear();
                grdEmployees.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ShftSch Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SaveRecord1()
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
                    int intPeriodId = 0;
                    if (string.IsNullOrEmpty(txtFromDate.Value) && string.IsNullOrEmpty(txtToDate.Value))
                    {
                        oApplication.StatusBar.SetText("Please Select Shift Schedule From and To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (string.IsNullOrEmpty(cbShift.Value))
                    {
                        oApplication.StatusBar.SetText("Please Select Shift", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (!string.IsNullOrEmpty(cbShift.Value) && Convert.ToInt32(cbShift.Value) < 1)
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
                            oApplication.StatusBar.SetText("Shift from date could not be greater then to date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                                        if (EmpDATA.JoiningDate > x) continue;
                                        var PeriodId = dbHrPayroll.CfgPeriodDates.Where(pd => pd.StartDate <= x && x <= pd.EndDate && pd.PayrollId == PayrollID).FirstOrDefault();
                                        if (PeriodId != null)
                                        {
                                            intPeriodId = PeriodId.ID;
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("Period for Selected Date Range Can't be found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            return;
                                        }
                                        TrnsAttendanceRegister attRegOld = dbHrPayroll.TrnsAttendanceRegister.Where(atr => atr.EmpID == intEmpID && atr.Date == x).FirstOrDefault();
                                        if (attRegOld != null && attRegOld.Processed == true)
                                        {
                                            continue;
                                        }
                                        if (attRegOld != null)
                                        {
                                            attRegOld.PeriodID = intPeriodId;
                                            attRegOld.ShiftID = Convert.ToInt32(cbShift.Value);
                                            attRegOld.UpdateDate = DateTime.Now;
                                            attRegOld.UpdatedBy = oCompany.UserName;
                                        }
                                        else
                                        {

                                            TrnsAttendanceRegister attendance = new TrnsAttendanceRegister();
                                            attendance.EmpID = intEmpID;
                                            attendance.PeriodID = intPeriodId;
                                            attendance.Date = x;
                                            attendance.ShiftID = Convert.ToInt32(cbShift.Value);
                                            attendance.CreateDate = DateTime.Now;
                                            attendance.UserId = oCompany.UserName;
                                            attendance.Processed = false;

                                            dbHrPayroll.TrnsAttendanceRegister.InsertOnSubmit(attendance);
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
                    int intPeriodId = 0;
                    if (string.IsNullOrEmpty(txtFromDate.Value) && string.IsNullOrEmpty(txtToDate.Value))
                    {
                        oApplication.StatusBar.SetText("Please Select Shift Schedule From and To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (string.IsNullOrEmpty(cbShift.Value))
                    {
                        oApplication.StatusBar.SetText("Please Select Shift", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (!string.IsNullOrEmpty(cbShift.Value) && Convert.ToInt32(cbShift.Value) < 1)
                    {
                        oApplication.StatusBar.SetText("Please Select Shift", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                    {
                        DateTime dtFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime dtTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        ShiftDays = ((dtTo.Subtract(dtFrom)).TotalDays + 1);
                        if (ShiftDays <= 0)
                        {
                            oApplication.StatusBar.SetText("Shift from date could not be greater then to date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
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
                                var oEmp = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEMPcode).FirstOrDefault();
                                if (oEmp == null)
                                {
                                    oApplication.StatusBar.SetText("Employee Record(s) can't be found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                else
                                {
                                    intEmpID = oEmp.ID;
                                    string EmpPayroll = Convert.ToString(oEmp.PayrollID);
                                    if (string.IsNullOrEmpty(EmpPayroll))
                                    {
                                        oApplication.StatusBar.SetText("Please Attach Payroll To Employee " + oEmp.EmpID + " Then Process Shift Schedualr", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        return;
                                    }
                                    PayrollID = oEmp.PayrollID;
                                }
                                if (intEmpID > 0)
                                {
                                    for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                    {
                                        var PeriodId = dbHrPayroll.CfgPeriodDates.Where(pd => pd.StartDate <= x && x <= pd.EndDate && pd.PayrollId == PayrollID).FirstOrDefault();
                                        if (PeriodId != null)
                                        {
                                            intPeriodId = PeriodId.ID;
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("Period for Selected Date Range Can't be found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            return;
                                        }
                                        if (oEmp.JoiningDate > x) continue;
                                        string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                        TrnsAttendanceRegister attRegOld = dbHrPayroll.TrnsAttendanceRegister.Where(atr => atr.EmpID == intEmpID
                                        && atr.Date == x).FirstOrDefault();
                                        if (attRegOld != null && attRegOld.Processed == true)
                                        {
                                            oApplication.StatusBar.SetText("Shift can not be Changed Employee : " + oEmp.EmpID + " has Attendance Processed on Date:" + x.ToString("MM/dd/yyyy") + "", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            continue;
                                        }
                                        if (attRegOld != null && attRegOld.Processed == true && attRegOld.FlgPosted == true)
                                        {
                                            oApplication.StatusBar.SetText("Shift can not be Changed Employee : " + oEmp.EmpID + " has Attendance Posted on Date:" + x.ToString("MM/dd/yyyy") + "", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            continue;
                                        }
                                        var oShiftdetails = (from a in dbHrPayroll.MstShiftDetails
                                                             where a.ShiftID == Convert.ToInt32(cbShift.Value)
                                                             && a.Day == dayofWeeks.ToUpper()
                                                             select a).FirstOrDefault();
                                        if (attRegOld != null)
                                        {

                                            attRegOld.PeriodID = intPeriodId;
                                            attRegOld.ShiftID = Convert.ToInt32(cbShift.Value);
                                            attRegOld.DateDay = dayofWeeks;
                                            if ((String.IsNullOrEmpty(oShiftdetails.StartTime) || oShiftdetails.StartTime == "00:00")
                                                && (String.IsNullOrEmpty(oShiftdetails.EndTime) || oShiftdetails.EndTime == "00:00"))
                                            {
                                                attRegOld.FlgOffDay = true;
                                            }
                                            else if (!string.IsNullOrEmpty(oEmp.DefaultOffDay))
                                            {
                                                if (oEmp.DefaultOffDay == dayofWeeks.ToUpper())
                                                {
                                                    attRegOld.FlgOffDay = true;
                                                }
                                                else
                                                {
                                                    attRegOld.FlgOffDay = false;
                                                }
                                            }
                                            attRegOld.UpdateDate = DateTime.Now;
                                            attRegOld.UpdatedBy = oCompany.UserName;

                                        }
                                        else
                                        {
                                            TrnsAttendanceRegister attendance = new TrnsAttendanceRegister();
                                            attendance.EmpID = intEmpID;
                                            attendance.PeriodID = intPeriodId;
                                            attendance.Date = x;
                                            attendance.DateDay = dayofWeeks;
                                            attendance.ShiftID = Convert.ToInt32(cbShift.Value);
                                            attendance.CreateDate = DateTime.Now;
                                            attendance.UserId = oCompany.UserName;
                                            attendance.Processed = false;
                                            if ((String.IsNullOrEmpty(oShiftdetails.StartTime) || oShiftdetails.StartTime == "00:00")
                                               && (String.IsNullOrEmpty(oShiftdetails.EndTime) || oShiftdetails.EndTime == "00:00"))
                                            {
                                                attendance.FlgOffDay = true;
                                            }
                                            else if (!string.IsNullOrEmpty(oEmp.DefaultOffDay))
                                            {
                                                if (oEmp.DefaultOffDay.ToUpper() == dayofWeeks.ToUpper())
                                                {
                                                    attendance.FlgOffDay = true;
                                                }
                                                else
                                                {
                                                    attendance.FlgOffDay = false;
                                                }
                                            }
                                            dbHrPayroll.TrnsAttendanceRegister.InsertOnSubmit(attendance);
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
                txtEmpFrom.Value = st.Rows[0][0].ToString();
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
                txtEmpTo.Value = st.Rows[0][0].ToString();
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

        private void OpenNewSearchFormFrom()
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
                    txtEmpFrom.Value = Program.FromEmpId;
                }
                if (!string.IsNullOrEmpty(Program.ToEmpId))
                {
                    txtEmpTo.Value = Program.ToEmpId;
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

    }
}
