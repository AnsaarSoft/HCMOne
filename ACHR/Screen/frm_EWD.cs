using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_EWD : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btOK, btCancel;
        SAPbouiCOM.EditText txEmpIdFrom;
        SAPbouiCOM.ComboBox cbPayrollName, cbEmpLocation, cbPayrollPeriod, cbDepartment, cbSalaryBase;
        SAPbouiCOM.Item icbPayrollPeriod, icbEmpLocation, icbDepartment, icbSalaryBase, ibtnVoid;
        SAPbouiCOM.DataTable dtEmpWD, dtHead, dtPeriods;
        SAPbouiCOM.Matrix grdEmpWorkDays;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, Id, clNo, EmpID, Name, WorkDays, PerdayIncome, NetIncome, IsSelected, PerDaySalary;
        SAPbouiCOM.Button btId, btId2, btnsearch, btnCalculateSalary, btnVoid;
        SAPbouiCOM.OptionBtn optNCalculated, optCalculated;
        public DateTime PeriodStartDate, PeriodEndDate;
        private bool oFormLoad = false;
        string CompanyName = "";
        TrnsEmployeeWorkDays oNewWD;
        TrnsEmployeeWDDetails OnewWDDetail;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                ibtnVoid.Enabled = false;
                //if (CompanyName.ToLower() == "pakola")
                //{
                icbSalaryBase.Enabled = true;
                //}
                //else
                //{
                //    icbSalaryBase.Enabled = false;
                //}
                oForm.Freeze(false);

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EWD Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (oFormLoad == false)
            {
                return;
            }
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        //if (cbSalaryBase.Value != "")
                        //{
                        ValidateAndSaveRecords();
                        //}
                        //else
                        //{
                        //    oApplication.StatusBar.SetText("Salary base is empty: ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return;
                        //}
                        break;
                    case "btId":
                        //picEmp();
                        OpenNewSearchForm();
                        break;
                    case "btsearch":
                        //    //LoadEmployeeRecords();
                        //    LoadEmployeeData();
                        break;
                    case "optNCal":
                        //LoadEmployeeData();
                        if (oFormLoad)
                        {
                            ibtnVoid.Enabled = false;
                            getNotCalcculatedEmployees();
                        }
                        break;
                    case "optCal":
                        if (oFormLoad)
                        {
                            ibtnVoid.Enabled = true;
                            getCalcculatedEmployees();
                        }
                        break;
                    case "btsalcal":
                        if (oFormLoad)
                        {
                            //if (cbSalaryBase.Value != "")
                            //{
                            CalculateNetSalary();
                            //}
                            //else
                            //{
                            //    oApplication.StatusBar.SetText("Salary base is empty: ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    return;
                            //}
                        }
                        break;
                    case "btVoid":
                        if (oFormLoad)
                        {
                            DWGVoidSalaryCalculations();
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
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                //case "cbPrPeriod":
                //     LoadEmployeeData();
                //    break;
                case "grdEWD":
                    //CalculateGrossSalary();
                    break;
                default:
                    break;
            }

        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            SetEmpValues();
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "cbPrName":
                case "cbPrPeriod":
                case "cbsalrybas":

                    refreshEmps();

                    break;
            }
            if (pVal.ItemUID == "cbPrName")
            {
                if (oFormLoad)
                {
                    FillPeriod(cbPayrollName.Value);
                }

            }
            if (pVal.ItemUID == "cbsalrybas")
            {
                if (oFormLoad)
                {
                    if (cbSalaryBase.Value.Trim() == "DWPD")
                    {
                        CalculatePerDaySalary();
                    }
                    else
                    {
                        CalculateMonthlySalarySalary();
                    }
                }

            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                CompanyName = string.IsNullOrEmpty(Program.systemInfo.CompanyName) ? "" : Program.systemInfo.CompanyName.Trim();
                //btnsearch = oForm.Items.Item("btsearch").Specific;
                btnCalculateSalary = oForm.Items.Item("btsalcal").Specific;
                btnVoid = oForm.Items.Item("btVoid").Specific;
                ibtnVoid = oForm.Items.Item("btVoid");
                btCancel = oForm.Items.Item("2").Specific;
                btOK = oForm.Items.Item("1").Specific;
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("txtEIDFrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txEmpIdFrom = oForm.Items.Item("txtEIDFrom").Specific;
                txEmpIdFrom.DataBind.SetBound(true, "", "txtEIDFrom");



                oForm.DataSources.UserDataSources.Add("optNCal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                optNCalculated = oForm.Items.Item("optNCal").Specific;
                optNCalculated.DataBind.SetBound(true, "", "optNCal");

                //optNCalculated.GroupWith("optCal");

                //optNCalculated.Selected = true;

                oForm.DataSources.UserDataSources.Add("optCal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                optCalculated = oForm.Items.Item("optCal").Specific;
                optCalculated.DataBind.SetBound(true, "", "optCal");


                optNCalculated.GroupWith(optCalculated.Item.UniqueID);
                optNCalculated.Selected = true;

                //Initializing ComboBxes

                cbPayrollName = oForm.Items.Item("cbPrName").Specific;
                oForm.DataSources.UserDataSources.Add("cbPrName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPayrollName.DataBind.SetBound(true, "", "cbPrName");
                icbPayrollPeriod = oForm.Items.Item("cbPrName");

                cbPayrollPeriod = oForm.Items.Item("cbPrPeriod").Specific;
                oForm.DataSources.UserDataSources.Add("cbPrPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPayrollPeriod.DataBind.SetBound(true, "", "cbPrPeriod");
                icbPayrollPeriod = oForm.Items.Item("cbPrPeriod");

                cbEmpLocation = oForm.Items.Item("cbELoc").Specific;
                oForm.DataSources.UserDataSources.Add("cbELoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbEmpLocation.DataBind.SetBound(true, "", "cbELoc");
                icbEmpLocation = oForm.Items.Item("cbELoc");

                cbDepartment = oForm.Items.Item("cbDpt").Specific;
                oForm.DataSources.UserDataSources.Add("cbDpt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDepartment.DataBind.SetBound(true, "", "cbDpt");
                icbDepartment = oForm.Items.Item("cbDpt");

                cbSalaryBase = oForm.Items.Item("cbsalrybas").Specific;
                oForm.DataSources.UserDataSources.Add("cbsalrybas", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbSalaryBase.DataBind.SetBound(true, "", "cbsalrybas");
                icbSalaryBase = oForm.Items.Item("cbsalrybas");

                dtPeriods = oForm.DataSources.DataTables.Item("dtPeriods");
                InitiallizegridMatrix();
                fillCbs();
                fillCombo("DW", cbSalaryBase);
                oFormLoad = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void fillCbs()
        {
            try
            {
                int i = 0;
                string selId = "0";
                IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                foreach (CfgPayrollDefination pr in prs)
                {
                    cbPayrollName.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                    i++;
                }

                cbPayrollName.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                FillPeriod(cbPayrollName.Value);

                IEnumerable<MstDepartment> depts = from p in dbHrPayroll.MstDepartment orderby p.DeptName ascending select p;
                cbDepartment.ValidValues.Add("0", "All");
                foreach (MstDepartment dept in depts)
                {
                    cbDepartment.ValidValues.Add(dept.ID.ToString(), dept.DeptName);

                }
                cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                cbEmpLocation.ValidValues.Add("0", "All");
                IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation orderby p.Description select p;

                foreach (MstLocation loc in locs)
                {
                    cbEmpLocation.ValidValues.Add(loc.Id.ToString(), loc.Description);

                }
                cbEmpLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
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
                Program.EmpID = "";
                Program.sqlString = "DailyWagers";
                string comName = "Search";
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
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txEmpIdFrom.Value = Program.EmpID;
                    var Emprecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txEmpIdFrom.Value).FirstOrDefault();
                    if (Emprecord != null)
                    {
                        string payrollname = Emprecord.PayrollName;
                        if (!string.IsNullOrEmpty(payrollname))
                        {
                            cbPayrollName.Select(payrollname, SAPbouiCOM.BoSearchKey.psk_ByDescription);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void CalculateNetSalary()
        {
            try
            {
                if (CompanyName.ToLower() == "pakola")
                {
                    if (cbSalaryBase.Value == "")
                    {
                        oApplication.StatusBar.SetText("Salary base is empty: ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                }
                grdEmpWorkDays.FlushToDataSource();
                String WorkDays, PerdayIncome;
                decimal intWorkDays;
                decimal decNetIncome, decPerdayIncome;
                CfgPeriodDates periods = (from p in dbHrPayroll.CfgPeriodDates
                                          where p.PayrollId.ToString() == cbPayrollName.Value
                                          && p.FlgLocked == false
                                          select p).FirstOrDefault();
                if (periods == null) return;
                CfgPeriodDates oPayrollPeriod = (from p in dbHrPayroll.CfgPeriodDates
                                                 where p.ID.ToString() == cbPayrollPeriod.Value.ToString()
                                                 select p).FirstOrDefault();

                for (int i = 0; i < dtEmpWD.Rows.Count; i++)
                {

                    string sel = dtEmpWD.GetValue("isSel", i);
                    if (sel == "N")
                    {
                        continue;
                    }
                    decimal TotalLeaveUsed = 0;
                    decimal PeriodDayCount = 0;
                    decimal PerDaySalary = 0;
                    WorkDays = Convert.ToString(dtEmpWD.GetValue("WorkDays", i));
                    PerdayIncome = Convert.ToString(dtEmpWD.GetValue("PerdayIncome", i));
                    string EmployeeCode = Convert.ToString(dtEmpWD.GetValue("EmpID", i));
                    if (!String.IsNullOrEmpty(WorkDays))
                    {
                        var oEmployee = (from a in dbHrPayroll.MstEmployee
                                         where a.FlgActive == true
                                         && a.EmpID == EmployeeCode
                                         //&& a.EmployeeContractType == "DWGS"
                                         select a).FirstOrDefault();
                        TotalLeaveUsed = (from a in dbHrPayroll.TrnsLeavesRequest
                                          where a.MstEmployee.EmpID == oEmployee.EmpID
                                          && periods.StartDate <= a.LeaveFrom
                                               && periods.EndDate >= a.LeaveFrom
                                               && a.LeaveType == a.MstLeaveType.ID
                                          //&& a.MstLeaveType.LeaveType == "Ded"
                                          select a.TotalCount).Sum() ?? 0;
                        if (oEmployee.JoiningDate >= oPayrollPeriod.StartDate && oEmployee.JoiningDate <= oPayrollPeriod.EndDate)
                        {
                            PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oEmployee.JoiningDate)).Days + 1;
                        }
                        else
                        {
                            PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oPayrollPeriod.StartDate)).Days + 1;
                        }
                        DateTime dtStartDate = Convert.ToDateTime(oPayrollPeriod.EndDate);
                        decimal MonthDays = DateTime.DaysInMonth(dtStartDate.Year, dtStartDate.Month);
                        if (cbSalaryBase.Value.Trim() == "DWPD")
                        {
                            intWorkDays = Convert.ToDecimal(WorkDays);
                            decPerdayIncome = Convert.ToDecimal(PerdayIncome);
                            decNetIncome = intWorkDays * decPerdayIncome;
                            dtEmpWD.SetValue("NetIncome", i, decNetIncome.ToString());

                        }
                        else
                        {
                            if (CompanyName.ToLower() == "pakola")
                            {
                                PerDaySalary = Convert.ToDecimal(PerdayIncome) / MonthDays;
                                //intWorkDays = PeriodDayCount;
                                decNetIncome = PerDaySalary * Convert.ToDecimal(WorkDays);
                                dtEmpWD.SetValue("NetIncome", i, decNetIncome.ToString());
                            }
                            else
                            {
                                intWorkDays = Convert.ToDecimal(WorkDays);
                                decPerdayIncome = Convert.ToDecimal(PerdayIncome);
                                decNetIncome = intWorkDays * decPerdayIncome;
                                dtEmpWD.SetValue("NetIncome", i, decNetIncome.ToString());
                            }
                        }
                    }
                }
                grdEmpWorkDays.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void CalculateNetSalaryForMonthlyDW()
        {
            try
            {
                grdEmpWorkDays.FlushToDataSource();
                String WorkDays, PerdayIncome;
                decimal intWorkDays;
                decimal decNetIncome, decPerdayIncome;
                for (int i = 0; i < dtEmpWD.Rows.Count; i++)
                {
                    string sel = dtEmpWD.GetValue("isSel", i);
                    if (sel == "N")
                    {
                        continue;
                    }
                    WorkDays = Convert.ToString(dtEmpWD.GetValue("WorkDays", i));
                    PerdayIncome = Convert.ToString(dtEmpWD.GetValue("PerdayIncome", i));
                    if (!String.IsNullOrEmpty(WorkDays))
                    {
                        intWorkDays = Convert.ToDecimal(WorkDays);
                        decPerdayIncome = Convert.ToDecimal(PerdayIncome);
                        decNetIncome = intWorkDays * decPerdayIncome;
                        dtEmpWD.SetValue("NetIncome", i, PerdayIncome.ToString());

                    }
                }
                grdEmpWorkDays.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void CalculatePerDaySalary()
        {
            try
            {
                grdEmpWorkDays.FlushToDataSource();
                String StrEmpID;
                decimal MonthlyIncome, decPerdayIncome;
                CfgPayrollDefination oPayroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cbPayrollName.Value.ToString() select p).FirstOrDefault();
                CfgPeriodDates oPayrollPeriod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPayrollPeriod.Value.ToString() select p).FirstOrDefault();
                for (int i = 0; i < dtEmpWD.Rows.Count; i++)
                {
                    //string sel = dtEmpWD.GetValue("isSel", i);
                    //if (sel == "N")
                    //{
                    //    continue;
                    //}
                    dtEmpWD.SetValue("NetIncome", i, 0);
                    StrEmpID = Convert.ToString(dtEmpWD.GetValue("EmpID", i));
                    if (StrEmpID != "")
                    {
                        Int32 PeriodDayCount = 0;
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID.ToString() == StrEmpID select p).FirstOrDefault();
                        if (emp == null) continue;
                        MonthlyIncome = Convert.ToDecimal(emp.BasicSalary);
                        if (emp.JoiningDate >= oPayrollPeriod.StartDate && emp.JoiningDate <= oPayrollPeriod.EndDate)
                        {
                            PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(emp.JoiningDate)).Days + 1;
                        }
                        else
                        {
                            PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oPayrollPeriod.StartDate)).Days + 1;
                        }
                        DateTime dtStartDate = Convert.ToDateTime(oPayrollPeriod.EndDate);
                        decimal MonthDays = DateTime.DaysInMonth(dtStartDate.Year, dtStartDate.Month);
                        decPerdayIncome = Convert.ToDecimal(MonthlyIncome) / MonthDays;
                        dtEmpWD.SetValue("PerdayIncome", i, MonthlyIncome.ToString());
                        dtEmpWD.SetValue("PerDay", i, MonthlyIncome.ToString());

                    }
                }
                grdEmpWorkDays.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void CalculateMonthlySalarySalary()
        {
            try
            {
                grdEmpWorkDays.FlushToDataSource();
                String StrEmpID;
                decimal MonthlyIncome, decPerdayIncome;
                CfgPayrollDefination oPayroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cbPayrollName.Value.ToString() select p).FirstOrDefault();
                CfgPeriodDates oPayrollPeriod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPayrollPeriod.Value.ToString() select p).FirstOrDefault();
                for (int i = 0; i < dtEmpWD.Rows.Count; i++)
                {
                    //string sel = dtEmpWD.GetValue("isSel", i);
                    //if (sel == "N")
                    //{
                    //    continue;
                    //}
                    dtEmpWD.SetValue("NetIncome", i, 0);
                    StrEmpID = Convert.ToString(dtEmpWD.GetValue("EmpID", i));
                    string strMontlySalary = Convert.ToString(dtEmpWD.GetValue("PerdayIncome", i));
                    if (StrEmpID != "")
                    {
                        Int32 PeriodDayCount = 0;
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID.ToString() == StrEmpID select p).FirstOrDefault();
                        if (emp == null) continue;
                        MonthlyIncome = Convert.ToDecimal(emp.BasicSalary);
                        if (emp.JoiningDate >= oPayrollPeriod.StartDate && emp.JoiningDate <= oPayrollPeriod.EndDate)
                        {
                            PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(emp.JoiningDate)).Days + 1;
                        }
                        else
                        {
                            PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oPayrollPeriod.StartDate)).Days + 1;
                        }
                        DateTime dtStartDate = Convert.ToDateTime(oPayrollPeriod.EndDate);
                        decimal MonthDays = DateTime.DaysInMonth(dtStartDate.Year, dtStartDate.Month);
                        decPerdayIncome = Convert.ToDecimal(MonthlyIncome) / MonthDays;
                        //decPerdayIncome = Convert.ToDecimal(strMontlySalary) * PeriodDayCount;
                        dtEmpWD.SetValue("PerdayIncome", i, MonthlyIncome.ToString());
                        dtEmpWD.SetValue("PerDay", i, decPerdayIncome.ToString());

                    }
                }
                grdEmpWorkDays.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtEmpWD = oForm.DataSources.DataTables.Add("Employees");
                dtEmpWD.Columns.Add("Id", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmpWD.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmpWD.Columns.Add("EmpID", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpWD.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpWD.Columns.Add("PerdayIncome", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtEmpWD.Columns.Add("PerDay", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtEmpWD.Columns.Add("WorkDays", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpWD.Columns.Add("NetIncome", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtEmpWD.Columns.Add("EmployeeID", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmpWD.Columns.Add("isSel", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                grdEmpWorkDays = (SAPbouiCOM.Matrix)oForm.Items.Item("grdEWD").Specific;
                oColumns = (SAPbouiCOM.Columns)grdEmpWorkDays.Columns;

                oColumn = oColumns.Item("Id");
                Id = oColumn;
                oColumn.DataBind.Bind("Employees", "Id");
                Id.Visible = false;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("Employees", "No");

                oColumn = oColumns.Item("cl_empID");
                EmpID = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpID");

                oColumn = oColumns.Item("cl_Name");
                Name = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpName");

                oColumn = oColumns.Item("cl_WD");
                WorkDays = oColumn;
                oColumn.DataBind.Bind("Employees", "WorkDays");

                oColumn = oColumns.Item("cl_pdIncm");
                PerdayIncome = oColumn;
                oColumn.DataBind.Bind("Employees", "PerdayIncome");

                oColumn = oColumns.Item("cl_pdSal");
                PerDaySalary = oColumn;
                oColumn.DataBind.Bind("Employees", "PerDay");

                oColumn = oColumns.Item("cl_NetI");
                NetIncome = oColumn;
                oColumn.DataBind.Bind("Employees", "NetIncome");

                oColumn = oColumns.Item("isSel");
                IsSelected = oColumn;
                oColumn.DataBind.Bind("Employees", "isSel");
                oColumn.TitleObject.Sortable = false;

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                dtPeriods.Rows.Clear();
                if (cbPayrollPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPayrollPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPayrollPeriod.ValidValues.Remove(cbPayrollPeriod.ValidValues.Item(k).Value);
                    }
                }
                int i = 0;
                string selId = "0";
                bool flgPrevios = false;
                bool flgHit = false;
                int count = 0;
                int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Count();
                if (cnt > 0)
                {
                    CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Single();
                    foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    {
                        if (pd.FlgVisible == null ? false : (bool)pd.FlgVisible && pd.FlgLocked != true)
                        {
                            cbPayrollPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        }
                        count++;
                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();
                        //if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                        //{
                        //    selId = pd.ID.ToString();
                        //}
                        if (Convert.ToBoolean(pd.FlgLocked))
                        {
                            selId = "0";
                            flgPrevios = true;
                        }
                        else
                        {
                            if (flgPrevios)
                            {
                                selId = pd.ID.ToString();
                                flgPrevios = false;
                            }
                        }

                        i++;
                    }
                    try
                    {
                        cbPayrollPeriod.Select(selId);
                        //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void refreshEmps()
        {
            try
            {
                if (cbPayrollName.Value.ToString() != "")
                {
                    CfgPeriodDates periods = (from p in dbHrPayroll.CfgPeriodDates
                                              where p.ID.ToString() == cbPayrollName.Value.ToString()
                                              && p.FlgLocked == false
                                              select p).FirstOrDefault();
                    if (periods != null)
                    {
                        PeriodStartDate = Convert.ToDateTime(periods.StartDate);
                        PeriodEndDate = Convert.ToDateTime(periods.EndDate);
                        if (cbPayrollName.Value != "" && periods != null)
                        {
                            if (optNCalculated.Selected)
                            {
                                getNotCalcculatedEmployees();
                            }
                            else
                            {
                                getCalcculatedEmployees();
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void getCalcculatedEmployees()
        {
            try
            {
                if (cbPayrollName.Value != "")
                {
                    CfgPeriodDates periods = (from p in dbHrPayroll.CfgPeriodDates
                                              where p.PayrollId.ToString() == cbPayrollName.Value
                                              && p.FlgLocked == false
                                              select p).FirstOrDefault();
                    if (periods == null) return;
                    PeriodStartDate = Convert.ToDateTime(periods.StartDate);
                    PeriodEndDate = Convert.ToDateTime(periods.EndDate);

                    string strSql = @"
                        SELECT
	                        dbo.MstEmployee.EmpID, dbo.MstEmployee.SBOEmpCode, 
	                        dbo.MstEmployee.FirstName + ' ' + ISNULL(dbo.MstEmployee.MiddleName, '')+ ' ' + ISNULL(dbo.MstEmployee.LastName, '') AS empName,  
	                        dbo.MstEmployee.DepartmentName, dbo.MstEmployee.BranchName,dbo.MstEmployee.LocationName, dbo.TrnsEmployeeWDDetails.Id,
                            MstEmployee.BasicSalary,TrnsEmployeeWDDetails.WorkDays,TrnsEmployeeWDDetails.NetIncome
	                          
                        FROM 
	                        dbo.MstEmployee 
	                        INNER JOIN dbo.TrnsEmployeeWDDetails ON dbo.MstEmployee.ID = dbo.TrnsEmployeeWDDetails.EmployeeID
							inner join TrnsEmployeeWorkDays on TrnsEmployeeWDDetails.EmpWDId=TrnsEmployeeWorkDays.Id
                        WHERE  
	                        
	                         (dbo.MstEmployee.EmployeeContractType='DWGS' AND dbo.TrnsEmployeeWorkDays.PayrollID = " + cbPayrollName.Value.Trim().ToString() + @")
	                        AND (dbo.TrnsEmployeeWorkDays.PayrollPeriodID = " + cbPayrollPeriod.Value.Trim().ToString() + @" )  
                        ";
                    if (txEmpIdFrom.Value.Trim() != "")
                    {
                        strSql += " AND EmpID = " + txEmpIdFrom.Value.Trim();
                    }
                    if (cbDepartment.Value.ToString().Trim() != "0" && cbDepartment.Value.ToString().Trim() != "")
                    {
                        strSql += " and departmentId = " + cbDepartment.Value.ToString();

                    }
                    if (cbEmpLocation.Value.ToString().Trim() != "0" && cbEmpLocation.Value.ToString().Trim() != "")
                    {
                        strSql += " and location = " + cbEmpLocation.Value.ToString();
                    }

                    strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                    DataTable dtEmp = ds.getDataTable(strSql);
                    dtEmpWD.Rows.Clear();
                    int i = 0;
                    foreach (DataRow dr in dtEmp.Rows)
                    {
                        dtEmpWD.Rows.Add(1);
                        dtEmpWD.SetValue("No", i, i + 1);
                        dtEmpWD.SetValue("Id", i, dr["ID"].ToString());
                        dtEmpWD.SetValue("isSel", i, "N");
                        dtEmpWD.SetValue("EmpID", i, dr["EmpID"].ToString());
                        dtEmpWD.SetValue("EmpName", i, dr["empName"].ToString());
                        dtEmpWD.SetValue("PerdayIncome", i, dr["BasicSalary"].ToString());
                        dtEmpWD.SetValue("WorkDays", i, dr["WorkDays"].ToString());
                        dtEmpWD.SetValue("NetIncome", i, dr["NetIncome"].ToString());
                        i++;

                    }
                    grdEmpWorkDays.LoadFromDataSource();
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //oApplication.SetStatusBarMessage("getCalcculatedEmployees :  " + ex.Message);
                logger(ex);
            }

        }

        private void getNotCalcculatedEmployees()
        {
            try
            {
                string PayrollName = cbPayrollName.Selected.Value.Trim();
                if (PayrollName == null)
                {
                    return;
                }
                else if (cbPayrollName.Value.Trim() != "")
                {
                    CfgPeriodDates periods = (from p in dbHrPayroll.CfgPeriodDates
                                              where p.PayrollId.ToString() == cbPayrollName.Value
                                              && p.FlgLocked == false
                                              select p).FirstOrDefault();
                    if (periods == null) return;
                    CfgPeriodDates oPayrollPeriod = (from p in dbHrPayroll.CfgPeriodDates
                                                     where p.ID.ToString() == cbPayrollPeriod.Value.ToString()
                                                     select p).FirstOrDefault();
                    PeriodStartDate = Convert.ToDateTime(periods.StartDate);
                    PeriodEndDate = Convert.ToDateTime(periods.EndDate);

                    string strSql = "SELECT  EmpID, ISNULL(SBOEmpCode,'') AS SBOEmpCode , ID, FirstName + ' ' + ISNULL(MiddleName, '')+ ' ' + LastName AS empName,BasicSalary, DepartmentName,BranchName, LocationName FROM dbo.MstEmployee";
                    strSql += " WHERE ISNULL(flgActive,0) <> 0 AND ISNULL(PayrollID, 0) = " + cbPayrollName.Value.Trim();
                    strSql += " AND JoiningDate <= '" + PeriodEndDate.ToString("MM/dd/yyyy") + "'";
                    strSql += " AND ResignDate IS NULL";
                    strSql += " AND ID NOT IN (SELECT A1.EmployeeID FROM dbo.TrnsEmployeeWDDetails A1 inner join TrnsEmployeeWorkDays A01 on A1.EmpWDId=A01.Id WHERE  A01.PayrollID =" + cbPayrollName.Value.Trim() + "  AND A01.PayrollPeriodID = " + cbPayrollPeriod.Value.Trim() + " ) ";

                    if (txEmpIdFrom.Value.Trim() != "")
                    {
                        strSql += " AND dbo.MstEmployee.EmpID = " + txEmpIdFrom.Value.Trim();
                    }
                    if (cbDepartment.Value.ToString().Trim() != "0" && cbDepartment.Value.ToString().Trim() != "")
                    {
                        strSql += " AND dbo.MstEmployee.DepartmentID = " + cbDepartment.Value.ToString();
                    }
                    if (cbEmpLocation.Value.ToString().Trim() != "0" && cbEmpLocation.Value.ToString().Trim() != "")
                    {
                        strSql += " AND dbo.MstEmployee.Location = " + cbEmpLocation.Value.ToString();
                    }

                    strSql += " AND dbo.MstEmployee.EmployeeContractType='DWGS' ORDER BY dbo.MstEmployee.SortOrder ASC";

                    DataTable dtEmp = ds.getDataTable(strSql);
                    dtEmpWD.Rows.Clear();
                    int i = 0;
                    foreach (DataRow dr in dtEmp.Rows)
                    {
                        decimal DailyWagerPresentDays = 0;
                        decimal TotalLeaveUsed = 0;
                        decimal decWorkingDaysfromPayroll = 0;
                        decWorkingDaysfromPayroll = Convert.ToInt32(periods.CfgPayrollDefination.WorkDays);
                        string EmployeeID = dr["ID"].ToString();
                        string EmployeeCode = dr["EmpID"].ToString();
                        if (EmployeeCode != "")
                        {
                            var oEmployee = (from a in dbHrPayroll.MstEmployee
                                             where a.FlgActive == true
                                             && a.EmpID == EmployeeCode
                                             //&& a.EmployeeContractType == "DWGS"
                                             select a).FirstOrDefault();
                            if (oEmployee != null)
                            {
                                dtEmpWD.Rows.Add(1);
                                dtEmpWD.SetValue("No", i, i + 1);
                                dtEmpWD.SetValue("Id", i, dr["ID"].ToString());
                                dtEmpWD.SetValue("isSel", i, "N");
                                dtEmpWD.SetValue("EmpID", i, dr["EmpID"].ToString());
                                dtEmpWD.SetValue("EmpName", i, dr["empName"].ToString());
                                dtEmpWD.SetValue("PerdayIncome", i, dr["BasicSalary"].ToString());
                                dtEmpWD.SetValue("PerDay", i, dr["BasicSalary"].ToString());

                                if (CompanyName.ToLower() == "pakola")
                                {
                                    #region GetWorkingDays
                                    decimal PeriodDayCount = 0;
                                    TotalLeaveUsed = (from a in dbHrPayroll.TrnsLeavesRequest
                                                      where a.MstEmployee.EmpID == oEmployee.EmpID
                                                      && periods.StartDate <= a.LeaveFrom
                                                           && periods.EndDate >= a.LeaveFrom
                                                           && a.LeaveType == a.MstLeaveType.ID
                                                      //&& a.MstLeaveType.LeaveType == "Ded"
                                                      select a.TotalCount).Sum() ?? 0;
                                    if (oEmployee.JoiningDate >= oPayrollPeriod.StartDate && oEmployee.JoiningDate <= oPayrollPeriod.EndDate)
                                    {
                                        PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oEmployee.JoiningDate)).Days + 1;
                                    }
                                    else
                                    {
                                        PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oPayrollPeriod.StartDate)).Days + 1;
                                    }
                                    if (Program.systemInfo.AttendanceSystem == null ? false : Program.systemInfo.AttendanceSystem == true)
                                    {
                                        if (decWorkingDaysfromPayroll > 0)
                                        {
                                            //decWorkingDaysfromPayroll = decWorkingDaysfromPayroll - TotalLeaveUsed;
                                            //dtEmpWD.Rows.Add(1);
                                            dtEmpWD.SetValue("WorkDays", i, decWorkingDaysfromPayroll.ToString());
                                        }
                                        else
                                        {
                                            DailyWagerPresentDays = (from a in dbHrPayroll.TrnsAttendanceRegister
                                                                     where a.FlgPosted == true
                                                                     && a.EmpID == oEmployee.ID
                                                                     select a).Count();

                                            if (DailyWagerPresentDays > 0)
                                            {
                                                //DailyWagerPresentDays = DailyWagerPresentDays - TotalLeaveUsed;
                                                //dtEmpWD.Rows.Add(1);
                                                dtEmpWD.SetValue("WorkDays", i, DailyWagerPresentDays.ToString());
                                                //WorkDays
                                            }
                                            else
                                            {
                                                oApplication.StatusBar.SetText("Attendance not posted of Empployee Code: " + EmployeeCode + "", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (decWorkingDaysfromPayroll > 0)
                                        {
                                            //decWorkingDaysfromPayroll = Convert.ToDecimal(decWorkingDaysfromPayroll - TotalLeaveUsed);
                                            //dtEmpWD.Rows.Add(1);
                                            dtEmpWD.SetValue("WorkDays", i, decWorkingDaysfromPayroll.ToString());
                                        }
                                        else
                                        {
                                            //PeriodDayCount = PeriodDayCount - TotalLeaveUsed;
                                            //dtEmpWD.Rows.Add(1);
                                            dtEmpWD.SetValue("WorkDays", i, PeriodDayCount.ToString());

                                        }
                                    }

                                    #endregion
                                }
                            }
                        }
                        i++;
                    }
                    grdEmpWorkDays.LoadFromDataSource();
                }

                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //oApplication.SetStatusBarMessage("getNotCalcculatedEmployees : " + ex.Message);
                logger(ex);
            }
        }

        private void ValidateAndSaveRecords()
        {
            try
            {
                if (CompanyName.ToLower() == "pakola")
                {
                    if (cbSalaryBase.Value == "")
                    {
                        oApplication.StatusBar.SetText("Salary base is empty: ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                }
                if (string.IsNullOrEmpty(cbPayrollName.Value))
                {
                    oApplication.StatusBar.SetText("Please Select valid Payroll", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (!string.IsNullOrEmpty(cbPayrollName.Value) && Convert.ToInt32(cbPayrollName.Value) < 0)
                {
                    oApplication.StatusBar.SetText("Please Select valid Payroll", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (string.IsNullOrEmpty(cbPayrollPeriod.Value))
                {
                    oApplication.StatusBar.SetText("Please Select valid Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (!string.IsNullOrEmpty(cbPayrollPeriod.Value) && Convert.ToInt32(cbPayrollPeriod.Value) < 0)
                {
                    oApplication.StatusBar.SetText("Please Select valid Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                oNewWD = dbHrPayroll.TrnsEmployeeWorkDays.Where(w => w.PayrollID == Convert.ToInt32(cbPayrollName.Value) && w.PayrollPeriodID == Convert.ToInt32(cbPayrollPeriod.Value)).FirstOrDefault();
                if (oNewWD == null)
                {
                    oNewWD = new TrnsEmployeeWorkDays();
                    dbHrPayroll.TrnsEmployeeWorkDays.InsertOnSubmit(oNewWD);
                }
                oNewWD.PayrollID = Convert.ToInt32(cbPayrollName.Value);
                oNewWD.PayrollPeriodID = Convert.ToInt32(cbPayrollPeriod.Value);

                for (int i = 0; i < dtEmpWD.Rows.Count; i++)
                {
                    string sel = dtEmpWD.GetValue("isSel", i);
                    if (sel == "N" || sel == "")
                    {
                        continue;
                    }
                    string empCode = Convert.ToString(dtEmpWD.GetValue("EmpID", i));
                    decimal decNetSalary = Convert.ToDecimal(dtEmpWD.GetValue("NetIncome", i));
                    if (decNetSalary > 0)
                    {
                        var EmpID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == empCode.Trim()).FirstOrDefault().ID;
                        OnewWDDetail = oNewWD.TrnsEmployeeWDDetails.Where(e => e.EmployeeID == EmpID).FirstOrDefault();
                        if (OnewWDDetail == null)
                        {
                            OnewWDDetail = new TrnsEmployeeWDDetails();
                            oNewWD.TrnsEmployeeWDDetails.Add(OnewWDDetail);
                        }
                        OnewWDDetail.EmployeeID = EmpID;
                        OnewWDDetail.WorkDays = Convert.ToDecimal(dtEmpWD.GetValue("WorkDays", i));
                        OnewWDDetail.PerDayIncome = Convert.ToDecimal(dtEmpWD.GetValue("PerdayIncome", i));
                        OnewWDDetail.NetIncome = Convert.ToDecimal(dtEmpWD.GetValue("NetIncome", i));
                        OnewWDDetail.CreateDate = DateTime.Now;
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Please calculate salary first of employee : '" + EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                }
                dbHrPayroll.SubmitChanges();
                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                dtEmpWD.Rows.Clear();
                grdEmpWorkDays.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void DWGVoidSalaryCalculations()
        {
            try
            {
                grdEmpWorkDays.FlushToDataSource();
                for (int i = 0; i < dtEmpWD.Rows.Count; i++)
                {
                    string sel = dtEmpWD.GetValue("isSel", i);
                    if (sel == "Y")
                    {
                        string empCode = Convert.ToString(dtEmpWD.GetValue("EmpID", i));
                        var EmployeeID = (from a in dbHrPayroll.MstEmployee
                                          where a.EmpID == empCode
                                          select a).FirstOrDefault();
                        if (EmployeeID != null)
                        {

                            TrnsEmployeeWDDetails reg = (from p in dbHrPayroll.TrnsEmployeeWDDetails
                                                         where p.EmployeeID == EmployeeID.ID
                                                         && p.EmpWDId == p.TrnsEmployeeWorkDays.Id
                                                         && p.TrnsEmployeeWorkDays.PayrollID.ToString() == cbPayrollName.Value.ToString()
                                                         && p.TrnsEmployeeWorkDays.PayrollPeriodID.ToString() == cbPayrollPeriod.Value.ToString()
                                                         select p).FirstOrDefault();
                            if (reg != null)
                            {
                                IEnumerable<TrnsEmployeeElementDetail> nonRecuringElements = from p in dbHrPayroll.TrnsEmployeeElementDetail
                                                                                             where p.TrnsEmployeeElement.MstEmployee.EmpID == empCode
                                                                                             && p.PeriodId.ToString() == cbPayrollPeriod.Value.ToString()
                                                                                             select p;
                                foreach (TrnsEmployeeElementDetail ele in nonRecuringElements)
                                {
                                    ele.FlgOneTimeConsumed = false;
                                }
                                if (reg != null)
                                {
                                    dbHrPayroll.TrnsEmployeeWDDetails.DeleteOnSubmit(reg);
                                }
                            }
                        }
                        //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, nonRecuringElements);                   
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Only Selected Employee can Void:", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                }
                dbHrPayroll.SubmitChanges();
                getCalcculatedEmployees();
                //getNotCalcculatedEmployees();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Void Salary : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }



        #endregion

    }
}
