using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_EmpLev : HRMSBaseForm
    {
        #region Variables
        SAPbouiCOM.EditText txtEmpFrom, txtEmpTo, txtFromDate, txtToDate;
        SAPbouiCOM.ComboBox cbDepartment, cbLocation, cbDesignation, cbPeriod;

        SAPbouiCOM.Matrix mtLeaveType, mtEmpAssign;
        SAPbouiCOM.DataTable dtLeaveType, dtEmpAssign;

        SAPbouiCOM.Button btnMain, btnCancel, btnLoad, btnApply, btnAssign;
        SAPbouiCOM.Item ibtnMain, ibtnCancel, ibtnLoad, ibtnApply, ibtnAssign;

        Boolean flgApplied = false;
        Boolean flgEmpFrom, flgEmpTo;
        #endregion

        #region SAP Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            ibtnMain.Visible = false;
            oForm.Freeze(false);
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);

            switch (pVal.ItemUID)
            {
                case "btId":
                    flgEmpTo = false;
                    flgEmpFrom = true;
                    OpenNewSearchForm();
                    break;
                case "btId2":
                    flgEmpTo = true;
                    flgEmpFrom = false;
                    OpenNewSearchFormTo();
                    break;
                case "1":
                    oForm.Close();
                    break;
                case "btapply":
                    SearchEmployee();
                    break;
                case "btAssing":
                    AssignLeaveAllocation();
                    break;
                case "mtLevTypes":
                    if (pVal.ColUID == "appl" && pVal.Row == 0)
                    {
                        CUCLeaves();
                    }
                    break;
                case "mtLevEmp":
                    if (pVal.ColUID == "appl" && pVal.Row == 0)
                    {
                        CUCEmployees();
                    }
                    break;
                default:
                    break;
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
            //SetEmpValues();
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            BubbleEvent = true;
            if (pVal.ColUID == "noofleaves")
            {
                string value = (mtLeaveType.Columns.Item("noofleaves").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                if (value.StartsWith("-"))
                {
                    oApplication.StatusBar.SetText("Negetive leaves count not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    (mtLeaveType.Columns.Item("noofleaves").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.000";
                    BubbleEvent = false;
                }
            }
        }


        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                oForm.DataSources.UserDataSources.Add("txempfrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmpFrom = oForm.Items.Item("txempfrom").Specific;
                txtEmpFrom.DataBind.SetBound(true, "", "txempfrom");
                //txtEmpFrom.Value = "0";

                oForm.DataSources.UserDataSources.Add("txempto", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmpTo = oForm.Items.Item("txempto").Specific;
                txtEmpTo.DataBind.SetBound(true, "", "txempto");
                //txtEmpTo.Value = "0";

                //txtFromDate = oForm.Items.Item("txfromdt").Specific;
                //oForm.DataSources.UserDataSources.Add("txfromdt", SAPbouiCOM.BoDataType.dt_DATE, 30); // from date
                //txtFromDate.DataBind.SetBound(true, "", "txfromdt");

                //txtToDate = oForm.Items.Item("txtodt").Specific;
                //oForm.DataSources.UserDataSources.Add("txtodt", SAPbouiCOM.BoDataType.dt_DATE, 30); // to date
                //txtToDate.DataBind.SetBound(true, "", "txtodt");

                oForm.DataSources.UserDataSources.Add("cbdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDepartment = oForm.Items.Item("cbdept").Specific;
                cbDepartment.DataBind.SetBound(true, "", "cbdept");

                oForm.DataSources.UserDataSources.Add("cbdesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDesignation = oForm.Items.Item("cbdesig").Specific;
                cbDesignation.DataBind.SetBound(true, "", "cbdesig");

                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLocation = oForm.Items.Item("cbloc").Specific;
                cbLocation.DataBind.SetBound(true, "", "cbloc");

                oForm.DataSources.UserDataSources.Add("cbperiod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPeriod = oForm.Items.Item("cbperiod").Specific;
                cbPeriod.DataBind.SetBound(true, "", "cbperiod");

                FillDepartmentCombo(cbDepartment);
                FillDesignationCombo(cbDesignation);
                FillLocationCombo(cbLocation);
                if (Program.systemInfo.FlgLeaveCalendar == true)
                {
                    FillPeriodYearComboLeaveCalander(cbPeriod);
                }
                else
                {
                    FillPeriodYearCombo(cbPeriod);
                }

                mtLeaveType = oForm.Items.Item("mtLevTypes").Specific;
                dtLeaveType = oForm.DataSources.DataTables.Item("dtlevtype");

                //mtLeaveType = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Emp").Specific;
                //IgrdEmployees = oForm.Items.Item("grd_Emp");
                //oColumns = (SAPbouiCOM.Columns)grdEmployees.Columns;


                mtEmpAssign = oForm.Items.Item("mtLevEmp").Specific;
                dtEmpAssign = oForm.DataSources.DataTables.Item("dtemp");

                btnMain = oForm.Items.Item("1").Specific;
                ibtnMain = oForm.Items.Item("1");
                btnCancel = oForm.Items.Item("2").Specific;
                ibtnCancel = oForm.Items.Item("2");
                btnApply = oForm.Items.Item("btapply").Specific;
                ibtnApply = oForm.Items.Item("btapply");
                btnAssign = oForm.Items.Item("btAssing").Specific;
                ibtnAssign = oForm.Items.Item("btAssing");

                cbDepartment.Select("0", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbDesignation.Select("0", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbLocation.Select("0", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbPeriod.Select("0", SAPbouiCOM.BoSearchKey.psk_ByValue);
                FillLeaveTypes();
                btnApply.Caption = "Search Employees";
                flgEmpFrom = false;
                flgEmpTo = false;
                //AddEmptyRow();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDepartmentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            IEnumerable<MstDepartment> Departments = from a in dbHrPayroll.MstDepartment orderby a.DeptName ascending select a;
            foreach (var Dept in Departments)
            {
                pCombo.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
            }
            pCombo.ValidValues.Add(Convert.ToString("0"), Convert.ToString("None"));
        }

        private void FillDesignationCombo(SAPbouiCOM.ComboBox pCombo)
        {
            //var AllDepartment = from a in dbHrPayroll.MstDepartment select a;
            IEnumerable<MstDesignation> Designations = (from a in dbHrPayroll.MstDesignation orderby a.Name select a).Distinct();
            foreach (var Designation in Designations)
            {
                pCombo.ValidValues.Add(Convert.ToString(Designation.Id), Convert.ToString(Designation.Name));
            }
            pCombo.ValidValues.Add(Convert.ToString("0"), Convert.ToString("None"));
        }

        private void FillLocationCombo(SAPbouiCOM.ComboBox pCombo)
        {
            IEnumerable<MstLocation> Locations = from a in dbHrPayroll.MstLocation orderby a.Name ascending select a;
            foreach (MstLocation Location in Locations)
            {
                pCombo.ValidValues.Add(Convert.ToString(Location.Id), Convert.ToString(Location.Name));
            }
            pCombo.ValidValues.Add(Convert.ToString("0"), Convert.ToString("None"));
        }

        private void FillPeriodYearComboLeaveCalander(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLeaveCalendar> oCollection = from a in dbHrPayroll.MstLeaveCalendar where a.FlgActive == true select a;
                foreach (MstLeaveCalendar Collect in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Collect.Id), Convert.ToString(Collect.Code));
                }
                pCombo.ValidValues.Add(Convert.ToString("0"), Convert.ToString("None"));
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillPeriodYear Combo Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillPeriodYearCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstCalendar> oCollection = from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a;
                foreach (MstCalendar Collect in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Collect.Id), Convert.ToString(Collect.Code));
                }
                pCombo.ValidValues.Add(Convert.ToString("0"), Convert.ToString("None"));
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillPeriodYear Combo Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLeaveTypes()
        {
            IEnumerable<MstLeaveType> LeaveTypes = from a in dbHrPayroll.MstLeaveType where a.Active == true select a;
            try
            {
                Int32 i = 0;
                if (LeaveTypes.Count() == 0)
                {
                    return;
                }
                dtLeaveType.Rows.Clear();
                dtLeaveType.Rows.Add(LeaveTypes.Count());
                foreach (MstLeaveType LT in LeaveTypes)
                {
                    dtLeaveType.SetValue("serial", i, i + 1);
                    dtLeaveType.SetValue("appl", i, "Y");
                    dtLeaveType.SetValue("leave", i, LT.Code);
                    dtLeaveType.SetValue("desc", i, LT.Description);
                    dtLeaveType.SetValue("value", i, 0.0);
                    i++;
                }
                mtLeaveType.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillLeaveTypes Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0, Serial = 1;
            if (dtLeaveType.Rows.Count == 0)
            {
                dtLeaveType.Rows.Add(1);
                RowValue = dtLeaveType.Rows.Count;
                dtLeaveType.SetValue("serial", RowValue - 1, Serial);
                dtLeaveType.SetValue("leave", RowValue - 1, "");
                dtLeaveType.SetValue("value", RowValue - 1, "0.0");
                mtLeaveType.AddRow(1, RowValue + 1);
                Serial++;
            }
            else
            {
                if (dtLeaveType.GetValue("code", dtLeaveType.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtLeaveType.Rows.Add(1);
                    RowValue = dtLeaveType.Rows.Count;
                    dtLeaveType.SetValue("serial", RowValue - 1, Serial);
                    dtLeaveType.SetValue("leave", RowValue - 1, "");
                    dtLeaveType.SetValue("value", RowValue - 1, "0.0");
                    mtLeaveType.AddRow(1, RowValue + 1);
                    Serial++;
                }
            }
            mtLeaveType.LoadFromDataSource();
        }

        private void LoadEmployeesOnSelection()
        {
            //Varialbe
            String Department = "", Designation = "", Location = "";

            Int32 FromEmp = 0, ToEmp = 0;
            try
            {
                Department = cbDepartment.Value.Trim();
                Designation = cbDesignation.Value.Trim();
                Location = cbLocation.Value.Trim();
                if (txtEmpFrom.Value != string.Empty && txtEmpTo.Value != string.Empty)
                {
                    int? intEmpIdFrom = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault();
                    int? intEmpIdTo = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                    if (intEmpIdFrom == null) intEmpIdFrom = 0;
                    if (intEmpIdTo == null) intEmpIdTo = 100000;

                    //if (!String.IsNullOrEmpty(txtEmpFrom.Value.Trim()) && txtEmpFrom.Value.Trim() != "0")
                    //{
                    //    FromEmp = Convert.ToInt32( (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault());
                    //}
                    //if (!String.IsNullOrEmpty(txtEmpTo.Value.Trim()) && txtEmpTo.Value.Trim() != "0")
                    //{
                    //    ToEmp = Convert.ToInt32( (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault());
                    //}
                    //if (ToEmp == 0) ToEmp = 100000000;

                    if (intEmpIdFrom > intEmpIdTo)
                    {
                        oApplication.StatusBar.SetText("Searching criteria is not valid for selected range.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    var Employee = (from a in dbHrPayroll.MstEmployee
                                    where a.FlgActive == true
                                    && a.SortOrder >= intEmpIdFrom
                                    && a.SortOrder <= intEmpIdTo
                                    select a).ToList();

                    if (Department != "0")
                    {
                        Employee = Employee.Where(a => a.DepartmentID.ToString() == Department).ToList();
                    }
                    if (Location != "0")
                    {
                        Employee = Employee.Where(a => a.Location.ToString() == Location).ToList();
                    }
                    if (Designation != "0")
                    {
                        Employee = Employee.Where(a => a.DesignationID.ToString() == Designation).ToList();
                    }
                    if (Employee == null)
                    {
                        oApplication.StatusBar.SetText("No Employee selected on give search parameter.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }

                    if (Employee.Count() > 0)
                    {
                        dtEmpAssign.Rows.Clear();
                        dtEmpAssign.Rows.Add(Employee.Count());
                        Int32 Serial = 1, i = 0;
                        foreach (MstEmployee a in Employee)
                        {
                            dtEmpAssign.SetValue("serial", i, Serial);
                            dtEmpAssign.SetValue("empid", i, a.EmpID);
                            dtEmpAssign.SetValue("name", i, a.FirstName + " " + a.MiddleName + " " + a.LastName);
                            MstEmployeeLeaves One = (from b in dbHrPayroll.MstEmployeeLeaves where b.EmpID == a.ID && b.FlgActive == true select b).FirstOrDefault();
                            if (One == null)
                            {
                                dtEmpAssign.SetValue("appl", i, "N");
                            }
                            else
                            {
                                dtEmpAssign.SetValue("appl", i, "Y");
                            }
                            Serial++;
                            i++;
                        }
                        mtEmpAssign.LoadFromDataSource();
                    }
                }
                else
                {
                    var Data = (from e in dbHrPayroll.MstEmployee
                                where e.FlgActive == true
                                && e.PayrollID > 0
                                orderby e.SortOrder ascending
                                select e).ToList();

                    if (txtEmpFrom.Value != string.Empty && txtEmpTo.Value != string.Empty)
                    {
                        int? sortorderfrom = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault();
                        int? sortorderto = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                        if (sortorderfrom == null) sortorderfrom = 0;
                        if (sortorderto == null) sortorderto = 100000;
                        if (sortorderfrom > sortorderto)
                        {
                            //Data = Data.Where(e => e.SortOrder >= intEmpIdTo && e.SortOrder <= intEmpIdFrom).ToList();                        
                            oApplication.StatusBar.SetText("Searching criteria is not valid for selected range.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;

                        }
                        if (sortorderto >= sortorderfrom)
                        {
                            Data = Data.Where(e => e.SortOrder >= sortorderfrom && e.SortOrder <= sortorderto).ToList();
                        }
                    }
                    if (Department != "0")
                    {
                        Data = Data.Where(a => a.DepartmentID.ToString() == Department).ToList();
                    }
                    if (Location != "0")
                    {
                        Data = Data.Where(a => a.Location.ToString() == Location).ToList();
                    }
                    if (Designation != "0")
                    {
                        Data = Data.Where(a => a.DesignationID.ToString() == Designation).ToList();
                    }
                    if (Data == null)
                    {
                        oApplication.StatusBar.SetText("No Employee selected on give search parameter.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (Data.Count() > 0)
                    {
                        dtEmpAssign.Rows.Clear();
                        dtEmpAssign.Rows.Add(Data.Count());
                        Int32 Serial = 1, i = 0;
                        foreach (MstEmployee a in Data)
                        {
                            dtEmpAssign.SetValue("serial", i, Serial);
                            dtEmpAssign.SetValue("empid", i, a.EmpID);
                            dtEmpAssign.SetValue("name", i, a.FirstName + " " + a.MiddleName + " " + a.LastName);
                            MstEmployeeLeaves One = (from b in dbHrPayroll.MstEmployeeLeaves where b.EmpID == a.ID && b.FlgActive == true select b).FirstOrDefault();
                            if (One == null)
                            {
                                dtEmpAssign.SetValue("appl", i, "N");
                            }
                            else
                            {
                                dtEmpAssign.SetValue("appl", i, "Y");
                            }
                            Serial++;
                            i++;
                        }
                        mtEmpAssign.LoadFromDataSource();
                    }
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoadEmployeesOnSelection Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void CUCEmployees()
        {
            try
            {
                oForm.Freeze(true);
                SAPbouiCOM.Column col = mtEmpAssign.Columns.Item("appl");

                if (col.TitleObject.Caption == "Applied")
                {
                    for (int i = 0; i < dtEmpAssign.Rows.Count; i++)
                    {

                        dtEmpAssign.SetValue("appl", i, "N");
                        col.TitleObject.Caption = "Apply";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmpAssign.Rows.Count; i++)
                    {
                        dtEmpAssign.SetValue("appl", i, "Y");
                        col.TitleObject.Caption = "Applied";
                    }
                }
                mtEmpAssign.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CheckEmployeeAll Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void CUCLeaves()
        {
            try
            {
                oForm.Freeze(true);
                SAPbouiCOM.Column col = mtLeaveType.Columns.Item("appl");

                if (col.TitleObject.Caption == "Applied")
                {
                    for (int i = 0; i < dtLeaveType.Rows.Count; i++)
                    {

                        dtLeaveType.SetValue("appl", i, "N");
                        col.TitleObject.Caption = "Apply";
                    }
                }
                else
                {
                    for (int i = 0; i < dtLeaveType.Rows.Count; i++)
                    {
                        dtLeaveType.SetValue("appl", i, "Y");
                        col.TitleObject.Caption = "Applied";
                    }
                }
                mtLeaveType.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CUCLeave Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SearchEmployee()
        {
            try
            {

                LoadEmployeesOnSelection();
                return;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CheckMain Exception" + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AssignLeaveAllocation()
        {
            try
            {                
                if (cbPeriod.Value.Trim() != "0")
                {
                    if (Program.systemInfo.FlgLeaveCalendar == true)
                    {
                        AssignLeavesFromLeaveCalendar();
                        btnMain.Caption = "Ok";
                    }
                    else
                    {
                        AssignLeaves();
                        btnMain.Caption = "Ok";
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Selection of leave period is mandatory for leave allocation.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }               
                return;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CheckMain Exception" + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AssignLeaves()
        {
            try
            {
                IEnumerable<MstEmployeeLeaves> EmpLeaves = null;
                MstEmployee Employee = null;
                MstCalendar oCalendar = null;
                Int32 LeaveTypeId = 0; Int32 reply = 0;
                Decimal Entitled = 0M;
                DateTime FromDate = DateTime.MinValue, ToDate = DateTime.MinValue;
                String LeaveTypeCode = "";
                String EmpID = "", Status = "";
                mtEmpAssign.FlushToDataSource();
                mtLeaveType.FlushToDataSource();
                for (Int32 i = 0; i < dtEmpAssign.Rows.Count; i++)
                {
                    EmpID = dtEmpAssign.GetValue("empid", i);
                    Status = dtEmpAssign.GetValue("appl", i);
                    if (!String.IsNullOrEmpty(Status) && Status.Trim() == "Y")
                    {

                        Employee = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpID select a).FirstOrDefault();

                        oCalendar = (from a in dbHrPayroll.MstCalendar where a.Id.ToString() == cbPeriod.Value.Trim() select a).FirstOrDefault();
                        if (oCalendar != null)
                        {
                            EmpLeaves = from a in dbHrPayroll.MstEmployeeLeaves
                                        where a.MstEmployee.EmpID == EmpID
                                        && a.FromDt == oCalendar.StartDate
                                        && a.ToDt == oCalendar.EndDate
                                        select a;
                        }
                        else
                        {
                            return;
                        }
                        FromDate = Convert.ToDateTime(oCalendar.StartDate);
                        ToDate = Convert.ToDateTime(oCalendar.EndDate);
                        if (EmpLeaves.Count() == 0)
                        {
                            for (Int32 k = 0; k < dtLeaveType.Rows.Count; k++)
                            {
                                String StatusLeave = dtLeaveType.GetValue("appl", k);
                                if (StatusLeave == "Y")
                                {
                                    MstEmployeeLeaves oNew = new MstEmployeeLeaves();
                                    oNew.EmpID = Employee.ID;
                                    LeaveTypeCode = dtLeaveType.GetValue("leave", k);
                                    LeaveTypeId = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a.ID).FirstOrDefault();
                                    oNew.LeaveType = LeaveTypeId;
                                    if (Convert.ToDecimal(dtLeaveType.GetValue("value", k)) != 0.0M)
                                    {
                                        Entitled = Convert.ToDecimal(dtLeaveType.GetValue("value", k));
                                    }
                                    else
                                    {
                                        Entitled = 0.0M;
                                    }
                                    oNew.LeavesEntitled = Entitled;
                                    oNew.CreateDate = Convert.ToDateTime(DateTime.Now);
                                    oNew.FromDt = FromDate;
                                    oNew.ToDt = ToDate;
                                    oNew.UserId = oCompany.UserName;
                                    oNew.FlgActive = true;
                                    oNew.LeaveCalCode = oCalendar.Code;
                                    dbHrPayroll.MstEmployeeLeaves.InsertOnSubmit(oNew);
                                    dbHrPayroll.SubmitChanges();
                                }
                            }
                        }
                        else
                        {
                            if (reply != 3)
                            {
                                reply = oApplication.MessageBox("Your are Reassigning Leaves.", 1, "Proceed", "Skip", "Processed All");
                            }
                            if (reply == 2)
                            {
                                continue;
                            }
                            for (Int32 k = 0; k < dtLeaveType.Rows.Count; k++)
                            {
                                String StatusLeave = dtLeaveType.GetValue("appl", k);
                                if (StatusLeave == "Y")
                                {
                                    MstEmployeeLeaves oUpdate = null;
                                    LeaveTypeCode = dtLeaveType.GetValue("leave", k);
                                    oUpdate = (from a in dbHrPayroll.MstEmployeeLeaves
                                               where a.MstLeaveType.Code == LeaveTypeCode &&
                                               a.MstEmployee.EmpID == EmpID &&
                                               a.FromDt == oCalendar.StartDate &&
                                               a.ToDt == oCalendar.EndDate
                                               select a).FirstOrDefault();
                                    if (oUpdate != null)
                                    {
                                        if (Convert.ToDecimal(dtLeaveType.GetValue("value", k)) != 0.0M)
                                        {
                                            Entitled = Convert.ToDecimal(dtLeaveType.GetValue("value", k));
                                        }
                                        else
                                        {
                                            Entitled = 0.0M;
                                        }
                                        oUpdate.LeavesEntitled = Entitled;
                                        oUpdate.UpdateDate = Convert.ToDateTime(DateTime.Now);
                                        oUpdate.LeaveCalCode = oCalendar.Code;

                                        oUpdate.UserId = oCompany.UserName;
                                        dbHrPayroll.SubmitChanges();
                                    }
                                    else
                                    {
                                        MstEmployeeLeaves oNew = new MstEmployeeLeaves();
                                        oNew.EmpID = Employee.ID;
                                        LeaveTypeCode = dtLeaveType.GetValue("leave", k);
                                        LeaveTypeId = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a.ID).FirstOrDefault();
                                        oNew.LeaveType = LeaveTypeId;
                                        if (Convert.ToDecimal(dtLeaveType.GetValue("value", k)) != 0.0M)
                                        {
                                            Entitled = Convert.ToDecimal(dtLeaveType.GetValue("value", k));
                                        }
                                        else
                                        {
                                            Entitled = 0.0M;
                                        }
                                        oNew.LeavesEntitled = Entitled;
                                        oNew.CreateDate = Convert.ToDateTime(DateTime.Now);
                                        oNew.FromDt = FromDate;
                                        oNew.ToDt = ToDate;
                                        oNew.FlgActive = true;
                                        oNew.LeaveCalCode = oCalendar.Code;
                                        oNew.CreatedBy = oCompany.UserName;
                                        //oNew.UserId = oCompany.UserName;
                                        dbHrPayroll.MstEmployeeLeaves.InsertOnSubmit(oNew);
                                        dbHrPayroll.SubmitChanges();
                                        oApplication.StatusBar.SetText("Leaves Successfully Assign", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                    }
                                }
                            }
                        }
                    }
                }
                oApplication.StatusBar.SetText("Leaves Successfully Assign", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AssignLeaveToEmployee Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AssignLeavesFromLeaveCalendar()
        {
            try
            {
                IEnumerable<MstEmployeeLeaves> EmpLeaves = null;
                MstEmployee Employee = null;
                MstLeaveCalendar oCalendar = null;
                Int32 LeaveTypeId = 0; Int32 reply = 0;
                Decimal Entitled = 0M;
                DateTime FromDate = DateTime.MinValue, ToDate = DateTime.MinValue;
                String LeaveTypeCode = "";
                String EmpID = "", Status = "";
                mtEmpAssign.FlushToDataSource();
                mtLeaveType.FlushToDataSource();
                for (Int32 i = 0; i < dtEmpAssign.Rows.Count; i++)
                {
                    EmpID = dtEmpAssign.GetValue("empid", i);
                    Status = dtEmpAssign.GetValue("appl", i);
                    if (!String.IsNullOrEmpty(Status) && Status.Trim() == "Y")
                    {

                        Employee = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpID select a).FirstOrDefault();

                        oCalendar = (from a in dbHrPayroll.MstLeaveCalendar where a.Id.ToString() == cbPeriod.Value.Trim() select a).FirstOrDefault();
                        if (oCalendar != null)
                        {
                            EmpLeaves = from a in dbHrPayroll.MstEmployeeLeaves
                                        where a.MstEmployee.EmpID == EmpID
                                        && a.FromDt == oCalendar.StartDate && a.ToDt == oCalendar.EndDate
                                        select a;
                        }
                        else
                        {
                            return;
                        }
                        FromDate = Convert.ToDateTime(oCalendar.StartDate);
                        ToDate = Convert.ToDateTime(oCalendar.EndDate);
                        if (EmpLeaves.Count() == 0)
                        {
                            for (Int32 k = 0; k < dtLeaveType.Rows.Count; k++)
                            {
                                String StatusLeave = dtLeaveType.GetValue("appl", k);
                                if (StatusLeave == "Y")
                                {
                                    MstEmployeeLeaves oNew = new MstEmployeeLeaves();
                                    oNew.EmpID = Employee.ID;
                                    LeaveTypeCode = dtLeaveType.GetValue("leave", k);
                                    LeaveTypeId = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a.ID).FirstOrDefault();
                                    oNew.LeaveType = LeaveTypeId;
                                    if (Convert.ToDecimal(dtLeaveType.GetValue("value", k)) != 0.0M)
                                    {
                                        Entitled = Convert.ToDecimal(dtLeaveType.GetValue("value", k));
                                    }
                                    else
                                    {
                                        Entitled = 0.0M;
                                    }
                                    oNew.LeavesEntitled = Entitled;
                                    oNew.CreateDate = Convert.ToDateTime(DateTime.Now);
                                    oNew.FromDt = FromDate;
                                    oNew.ToDt = ToDate;
                                    oNew.LeaveCalCode = oCalendar.Code;
                                    oNew.UserId = oCompany.UserName;
                                    oNew.FlgActive = true;
                                    dbHrPayroll.MstEmployeeLeaves.InsertOnSubmit(oNew);
                                    dbHrPayroll.SubmitChanges();
                                }
                            }
                        }
                        else
                        {
                            if (reply != 3)
                            {
                                reply = oApplication.MessageBox("Your are Reassigning Leaves.", 1, "Proceed", "Skip", "Processed All");
                            }
                            if (reply == 2)
                            {
                                continue;
                            }
                            for (Int32 k = 0; k < dtLeaveType.Rows.Count; k++)
                            {
                                String StatusLeave = dtLeaveType.GetValue("appl", k);
                                if (StatusLeave == "Y")
                                {
                                    MstEmployeeLeaves oUpdate = null;
                                    LeaveTypeCode = dtLeaveType.GetValue("leave", k);
                                    oUpdate = (from a in dbHrPayroll.MstEmployeeLeaves
                                               where a.MstLeaveType.Code == LeaveTypeCode &&
                                               a.MstEmployee.EmpID == EmpID &&
                                               a.FromDt == oCalendar.StartDate &&
                                               a.ToDt == oCalendar.EndDate
                                               select a).FirstOrDefault();
                                    if (oUpdate != null)
                                    {
                                        if (Convert.ToDecimal(dtLeaveType.GetValue("value", k)) != 0.0M)
                                        {
                                            Entitled = Convert.ToDecimal(dtLeaveType.GetValue("value", k));
                                        }
                                        else
                                        {
                                            Entitled = 0.0M;
                                        }
                                        oUpdate.LeavesEntitled = Entitled;
                                        oUpdate.LeaveCalCode = oCalendar.Code;
                                        oUpdate.UpdateDate = Convert.ToDateTime(DateTime.Now);
                                        //AR
                                        oUpdate.UserId = oCompany.UserName;
                                        dbHrPayroll.SubmitChanges();
                                    }
                                    else
                                    {
                                        MstEmployeeLeaves oNew = new MstEmployeeLeaves();
                                        oNew.EmpID = Employee.ID;
                                        LeaveTypeCode = dtLeaveType.GetValue("leave", k);
                                        LeaveTypeId = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a.ID).FirstOrDefault();
                                        oNew.LeaveType = LeaveTypeId;
                                        if (Convert.ToDecimal(dtLeaveType.GetValue("value", k)) != 0.0M)
                                        {
                                            Entitled = Convert.ToDecimal(dtLeaveType.GetValue("value", k));
                                        }
                                        else
                                        {
                                            Entitled = 0.0M;
                                        }
                                        oNew.LeavesEntitled = Entitled;
                                        oNew.CreateDate = Convert.ToDateTime(DateTime.Now);
                                        oNew.FromDt = FromDate;
                                        oNew.ToDt = ToDate;
                                        oNew.LeaveCalCode = oCalendar.Code;
                                        oNew.FlgActive = true;
                                        oNew.CreatedBy = oCompany.UserName;
                                        //oNew.UserId = oCompany.UserName;
                                        dbHrPayroll.MstEmployeeLeaves.InsertOnSubmit(oNew);
                                        dbHrPayroll.SubmitChanges();
                                        oApplication.StatusBar.SetText("Leaves Successfully Assign", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                    }
                                }
                            }
                        }
                    }
                }
                oApplication.StatusBar.SetText("Leaves Successfully Assign", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("AssignLeaveToEmployee Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void picEmpFrom()
        {
            string strSql = sqlString.getSql("empAdvance", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Leave Assigment");
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
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Leave Assigment");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpTo.Value = st.Rows[0][0].ToString();
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
