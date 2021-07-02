using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.ComponentModel;


namespace ACHR.Screen
{
    class frm_ShowReports1 : HRMSBaseForm
    {
        #region Variable Area

        SAPbouiCOM.ComboBox cmbReport, cbPeriod, cbPayroll;
        SAPbouiCOM.Button btnShowReport, btnok;
        SAPbouiCOM.Matrix mtEmployee, mtDepartment, mtLocation,mtPeriod;
        SAPbouiCOM.Item imtEmployee, imtDepartment, imtLocation, itxtFromDate, itxtToDate, IcbPeriod, IcbPayroll, Ibtnok;
        SAPbouiCOM.Column eIsSelected, eEmpId, eName, dIsSelected, dCode, dDescription, lIsSelected, lCode, lDescription;
        SAPbouiCOM.DataTable dtEmployee, dtDepartment, dtLocation, dtPeriods;
        SAPbouiCOM.EditText txtFromDate, txtToDate;

        #endregion

        #region SAP Events Area

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                InitiallizeForm();
                Ibtnok.Visible = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception CreateForm ShowReport : "+ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btreport":
                        ShowReport();
                        break;                    
                    case "mtemp":
                        if (pVal.Row == 0 && pVal.ColUID == "sel")
                        {
                            SelectAllEmployee();
                        }
                        if (pVal.Row == 0 && pVal.ColUID == "clear")
                        {
                            FillEmployeeInGrid();
                        }
                        break;
                    case "mtdept":
                        if (pVal.Row == 0 && pVal.ColUID == "selected")
                        {
                            SelectAllDepartment();
                        }
                        if (pVal.Row == 0 && pVal.ColUID == "loademp")
                        {
                            FillEmployeeInGrid("Department");
                        }
                        break;
                    case "mtloc":
                        if (pVal.Row == 0 && pVal.ColUID == "selected")
                        {
                            SelectAllLocation();
                        }
                        if (pVal.Row == 0 && pVal.ColUID == "loademp")
                        {
                            FillEmployeeInGrid("Location");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception etAfterClick : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "cbreport":
                    SetParameterBasedOnSelection(cmbReport.Selected.Value);
                    break;
                case "cbPayroll":
                    if (pVal.ItemUID == "cbPayroll")
                    {
                        FillPeriod(cbPayroll.Value);               
                    }
                    break;
            }
        }

        #endregion

        #region Local Methods

        private void InitiallizeForm()
        {
            try
            {
                oForm.Freeze(true);

                btnShowReport = oForm.Items.Item("btreport").Specific;
                btnok = oForm.Items.Item("1").Specific;
                Ibtnok = oForm.Items.Item("1");
                
              

                //From Date

                txtFromDate = oForm.Items.Item("txdtfrom").Specific;
                itxtFromDate = oForm.Items.Item("txdtfrom");
                oForm.DataSources.UserDataSources.Add("txdtfrom", SAPbouiCOM.BoDataType.dt_DATE);
                txtFromDate.DataBind.SetBound(true, "", "txdtfrom");

                //ToDate

                txtToDate = oForm.Items.Item("txdtto").Specific;
                itxtToDate = oForm.Items.Item("txdtto");
                oForm.DataSources.UserDataSources.Add("txdtto", SAPbouiCOM.BoDataType.dt_DATE);
                txtToDate.DataBind.SetBound(true, "", "txdtto");
                
                //Report Combobxo 
                cmbReport = oForm.Items.Item("cbreport").Specific;
                oForm.DataSources.UserDataSources.Add("cbreport", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                cmbReport.DataBind.SetBound(true, "", "cbreport");

                //Payroll Combobox
                oForm.DataSources.UserDataSources.Add("cbPayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbPayroll = oForm.Items.Item("cbPayroll").Specific;
                IcbPayroll = oForm.Items.Item("cbPayroll");
                cbPayroll.DataBind.SetBound(true, "", "cbPayroll");
                //Periods Combobox
                cbPeriod = oForm.Items.Item("cbPeriod").Specific;
                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPeriod.DataBind.SetBound(true, "", "cbPeriod");
                IcbPeriod = oForm.Items.Item("cbPeriod");

                

                // Employee Matrix

                mtEmployee = oForm.Items.Item("mtemp").Specific;
                imtEmployee = oForm.Items.Item("mtemp");
                dtEmployee = oForm.DataSources.DataTables.Item("dtEmployee");
                eIsSelected = mtEmployee.Columns.Item("sel");
                eEmpId = mtEmployee.Columns.Item("empid");
                eName = mtEmployee.Columns.Item("name");
 
                // Department Matrix

                mtDepartment = oForm.Items.Item("mtdept").Specific;
                imtDepartment = oForm.Items.Item("mtdept");
                dtDepartment = oForm.DataSources.DataTables.Item("dtDept");
                dIsSelected = mtDepartment.Columns.Item("selected");
                dCode = mtDepartment.Columns.Item("code");
                dDescription = mtDepartment.Columns.Item("name");

                // Location Matrix

                mtLocation = oForm.Items.Item("mtloc").Specific;
                imtLocation = oForm.Items.Item("mtloc");
                dtLocation = oForm.DataSources.DataTables.Item("dtLocation");
                lIsSelected = mtLocation.Columns.Item("selected");
                lCode = mtLocation.Columns.Item("code");
                lDescription = mtLocation.Columns.Item("name");

                dtPeriods = oForm.DataSources.DataTables.Item("dtPeriods");
                
                

                //Periods Datatable
                
               // dtPeriods = oForm.DataSources.DataTables.Item("dtPeriods");                

                //Initialize ComboBox
                //fillCbs();
                fillPayroll(cbPayroll);

                FillReportComboBox(cmbReport);
                FillEmployeeInGrid();
                FillDepartmentInGrid();
                FillLocationInGrid();
                //btnok.Caption = "OK";
                oForm.Freeze(false);
               
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                oForm.Freeze(false);
            }
        }

        private void FillReportComboBox(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<TblRpts> oReports = from a in dbHrPayroll.TblRpts where a.FlgSystem == false select a;
                pCombo.ValidValues.Add("0", "Not Report Selected");
                foreach (TblRpts Report in oReports)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Report.ReportId), Convert.ToString(Report.ReportName));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillReportComboBox : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void fillPayroll(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                int i = 0;
                string selId = "0";
                #region Fill Payroll
                string strOut = string.Empty;
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'");
                strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    if (strOut != null && strOut != "")
                    {
                        string strSql = sqlString.getSql("GetPayrollName", SearchKeyVal);
                        strSql = strSql + " where ID in (" + strOut + ")";
                        strSql += " ORDER BY ID Asc ";
                        System.Data.DataTable dt = ds.getDataTable(strSql);
                        DataView dv = dt.DefaultView;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int l = 0; l < dt.Rows.Count; l++)
                            {
                                string strPayrollName = dt.Rows[l]["PayrollName"].ToString();
                                Int32 intPayrollID = Convert.ToInt32(dt.Rows[l]["ID"].ToString());
                                cbPayroll.ValidValues.Add(intPayrollID.ToString(), strPayrollName);

                            }
                        }
                        cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbPayroll.Value);
                    }
                    else
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbPayroll.Value);
                    }

                }
                else
                {
                    IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                    foreach (CfgPayrollDefination pr in prs)
                    {
                        cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        i++;
                    }

                    cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    FillPeriod(cbPayroll.Value);
                }
                //End Fill Payroll
                #endregion
                cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                FillPeriod(cbPayroll.Value);


            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured fillPayroll " + ex.Message);
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                dtPeriods.Rows.Clear();
                if (cbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPeriod.ValidValues.Remove(cbPeriod.ValidValues.Item(k).Value);
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
                            cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
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
                        cbPeriod.Select(selId);
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

        private void fillPayrollOld(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                int i = 0;
                string selId = "0";
                IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                pCombo.ValidValues.Add("0", "Not Payroll Selected");
                foreach (CfgPayrollDefination pr in prs)
                {
                    cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                    i++;
                }

                cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                FillPeriod(cbPayroll.Value);

                
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured fillPayroll " + ex.Message);
            }
        }

        private void FillPeriodOld(string payroll)
        {
            try
            {
                
                if (cbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPeriod.ValidValues.Remove(cbPeriod.ValidValues.Item(k).Value);
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
                        cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
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
                        //cbPeriod.Select(selId);
                        //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured FillPeriod" + ex.Message);
            }
        }

        private void FillEmployeeInGrid()
        {
            try
            {
                dtEmployee.Rows.Clear();
                //IEnumerable<MstEmployee> oEmpCollection = from a in dbHrPayroll.MstEmployee select a;
                if (!string.IsNullOrEmpty(Program.objHrmsUI.EmployeeFilterValues))
                {
                    string[] arr;
                    if (Program.objHrmsUI.EmployeeFilterValues.Contains(','))
                    {
                        arr = Program.objHrmsUI.EmployeeFilterValues.Split(',');
                        var oEmployees = (from a in dbHrPayroll.MstEmployee
                                      where arr.Contains((a.PayrollID != null ? a.PayrollID.ToString() : "0"))
                                      select a).ToList();
                        UInt16 i = 0;
                        foreach (MstEmployee One in oEmployees)
                        {
                            dtEmployee.Rows.Add(1);
                            dtEmployee.SetValue(eIsSelected.DataBind.Alias, i, "N");
                            dtEmployee.SetValue(eEmpId.DataBind.Alias, i, One.EmpID);
                            dtEmployee.SetValue(eName.DataBind.Alias, i, One.FirstName + " " + One.MiddleName + " " + One.LastName);
                            i++;
                        }
                        mtEmployee.LoadFromDataSource();
                    }
                    else
                    {
                        var oEmployees = (from a in dbHrPayroll.MstEmployee
                                      where a.PayrollID.ToString() == Program.objHrmsUI.EmployeeFilterValues
                                      select a).ToList();
                        UInt16 i = 0;
                        foreach (MstEmployee One in oEmployees)
                        {
                            dtEmployee.Rows.Add(1);
                            dtEmployee.SetValue(eIsSelected.DataBind.Alias, i, "N");
                            dtEmployee.SetValue(eEmpId.DataBind.Alias, i, One.EmpID);
                            dtEmployee.SetValue(eName.DataBind.Alias, i, One.FirstName + " " + One.MiddleName + " " + One.LastName);
                            i++;
                        }
                        mtEmployee.LoadFromDataSource();
                    }
                }
                else
                {
                    var oEmployees = (from a in dbHrPayroll.MstEmployee
                                      orderby (a.SortOrder != null ? a.SortOrder : a.ID) ascending
                                      select a).ToList();
                    UInt16 i = 0;
                    foreach (MstEmployee One in oEmployees)
                    {
                        dtEmployee.Rows.Add(1);
                        dtEmployee.SetValue(eIsSelected.DataBind.Alias, i, "N");
                        dtEmployee.SetValue(eEmpId.DataBind.Alias, i, One.EmpID);
                        dtEmployee.SetValue(eName.DataBind.Alias, i, One.FirstName + " " + One.MiddleName + " " + One.LastName);
                        i++;
                    }
                    mtEmployee.LoadFromDataSource();
                }
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillEmployeeInGrid : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillEmployeeInGrid(String pPara)
        {
            IEnumerable<MstEmployee> oEmpCollection = null;
            DataTable dtSelected = new DataTable();
            String DepartmentCrit = "", LocationCrit = "";
            dtSelected.Columns.Add("Code");
            try
            {
                oForm.Freeze(true);
                dtEmployee.Rows.Clear();
                if (pPara == "Department")
                {
                    String deptList=@"SELECT dbo.MstEmployee.*
                                      FROM dbo.MstEmployee INNER JOIN dbo.MstDepartment ON dbo.MstEmployee.DepartmentID = dbo.MstDepartment.ID
                                     ";
                    mtDepartment.FlushToDataSource();
                    dtSelected.Rows.Clear();
                    DepartmentCrit += " WHERE dbo.MstDepartment.Code IN ";
                    mtDepartment.FlushToDataSource();
                    for (int i = 0; i < dtDepartment.Rows.Count; i++)
                    {
                        String Selection = dtDepartment.GetValue(dIsSelected.DataBind.Alias, i);
                        String DeptCode = dtDepartment.GetValue(dCode.DataBind.Alias, i);
                        if (Selection == "Y")
                        {
                            DataRow drOne = dtSelected.NewRow();
                            drOne[0] = DeptCode;
                            dtSelected.Rows.Add(drOne);
                        }
                    }
                    int totalCount = dtSelected.Rows.Count;
                    for (int i = 0; i < totalCount; i++)
                    {
                        if (i == 0)
                        {
                            if (i == totalCount - 1)
                            {
                                DepartmentCrit += "('" + dtSelected.Rows[i][0].ToString() + "')";
                            }
                            else
                            {
                                DepartmentCrit += "('" + dtSelected.Rows[i][0].ToString() + "',";
                            }
                        }
                        if (i == totalCount - 1)
                        {
                            if (i == 0)
                            {
                            }
                            else
                            {
                                DepartmentCrit += "'" + dtSelected.Rows[i][0].ToString() + "')";
                            }
                        }
                        if (i != 0 && i != totalCount - 1)
                        {
                            DepartmentCrit += "'" + dtSelected.Rows[i][0].ToString() + "',";
                        }
                    }
                    if (totalCount > 0)
                    {
                        deptList += DepartmentCrit;
                    }
                    oEmpCollection = dbHrPayroll.ExecuteQuery<MstEmployee>(deptList);
                }
                if (pPara == "Location")
                {
                    String locList = "";
                    mtLocation.FlushToDataSource();
                    String locationList = @"SELECT dbo.MstEmployee.*
                                            FROM dbo.MstEmployee INNER JOIN dbo.MstLocation ON dbo.MstEmployee.Location = dbo.MstLocation.Id";
                    dtSelected.Rows.Clear();
                    LocationCrit += " WHERE dbo.MstLocation.Name IN ";
                    mtDepartment.FlushToDataSource();
                    for (int i = 0; i < dtLocation.Rows.Count; i++)
                    {
                        String Selection = dtLocation.GetValue(lIsSelected.DataBind.Alias, i);
                        String LocCode = dtLocation.GetValue(lCode.DataBind.Alias, i);
                        if (Selection == "Y")
                        {
                            DataRow drOne = dtSelected.NewRow();
                            drOne[0] = LocCode;
                            dtSelected.Rows.Add(drOne);
                        }
                    }
                    int totalCount = dtSelected.Rows.Count;
                    for (int i = 0; i < totalCount; i++)
                    {
                        if (i == 0)
                        {
                            if (i == totalCount - 1)
                            {
                                LocationCrit += "('" + dtSelected.Rows[i][0].ToString() + "')";
                            }
                            else
                            {
                                LocationCrit += "('" + dtSelected.Rows[i][0].ToString() + "',";
                            }
                        }
                        if (i == totalCount - 1)
                        {
                            if (i == 0)
                            {
                            }
                            else
                            {
                                LocationCrit += "'" + dtSelected.Rows[i][0].ToString() + "')";
                            }
                        }
                        if (i != 0 && i != totalCount - 1)
                        {
                            LocationCrit += "'" + dtSelected.Rows[i][0].ToString() + "',";
                        }
                    }
                    if (totalCount > 0)
                    {
                        locationList += LocationCrit;
                    }
                    oEmpCollection = dbHrPayroll.ExecuteQuery<MstEmployee>(locationList);
                }

                //oEmpCollection = from a in dbHrPayroll.MstEmployee select a;
                UInt16 k = 0;
                foreach (MstEmployee One in oEmpCollection)
                {
                    dtEmployee.Rows.Add(1);
                    dtEmployee.SetValue(eIsSelected.DataBind.Alias, k, "N");
                    dtEmployee.SetValue(eEmpId.DataBind.Alias, k, One.EmpID);
                    dtEmployee.SetValue(eName.DataBind.Alias, k, One.FirstName + " " + One.MiddleName + " " + One.LastName);
                    k++;
                }
                mtEmployee.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillEmployeeInGrid : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                oForm.Freeze(false);
            }
        }

        private void FillDepartmentInGrid()
        {
            try
            {
                dtDepartment.Rows.Clear();
                IEnumerable<MstDepartment> oCollection = from a in dbHrPayroll.MstDepartment select a;
                UInt16 i = 0;
                foreach (MstDepartment One in oCollection)
                {
                    dtDepartment.Rows.Add(1);
                    dtDepartment.SetValue(dIsSelected.DataBind.Alias, i, "N");
                    dtDepartment.SetValue(dCode.DataBind.Alias, i, One.Code);
                    dtDepartment.SetValue(dDescription.DataBind.Alias, i, One.DeptName);
                    i++;
                }
                mtDepartment.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillDepartmentInGrid : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLocationInGrid()
        {
            try
            {
                dtLocation.Rows.Clear();
                UInt16 i = 0;
                IEnumerable<MstLocation> oCollection = from a in dbHrPayroll.MstLocation select a;
                foreach (MstLocation One in oCollection)
                {
                    dtLocation.Rows.Add(1);
                    dtLocation.SetValue(lIsSelected.DataBind.Alias, i, "N");
                    dtLocation.SetValue(lCode.DataBind.Alias, i, One.Name);
                    dtLocation.SetValue(lDescription.DataBind.Alias, i, One.Description);
                    i++;
                }
                mtLocation.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillLocationInGrid : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ShowDeptAttendanceSummary()
        {
            try
            {
                DateTime dtFrom, dtTo;
                String FromDate = "", ToDate = "";
                String Critaria = "";
                string ReportCode = "DeptAttd";
                Critaria += " Where 1=1 ";
                if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                {
                    dtFrom = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    FromDate += "" + dtFrom.Month + "/" + dtFrom.Day + "/" + dtFrom.Year; 
                    dtTo = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    ToDate += "" + dtTo.Month + "/" + dtTo.Day + "/" + dtTo.Year; 

                    double LeaveDays = ((dtTo.Subtract(dtFrom)).TotalDays + 1);
                    if (LeaveDays != 1)
                    {
                        oApplication.StatusBar.SetText("DateFrom and DateTo Fields must be same", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    else
                    {
                        Critaria += " AND DateRange Between '" + FromDate + "' And '" + ToDate + "' AND ShiftDay=DATENAME(dw,'" + FromDate + "') ";
                        Program.objHrmsUI.printRpt(ReportCode, true, Critaria,"");

                    }
                }


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception ShowDeptAttendanceSummary : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ShowReport()
        {
            String ReportID = "";
            String Critaria = "";
            String CurrentPeriod="";
            String EmployeeCrit = "";
            String DepartmentCrit = "";
            String LocationCrit = "";
            String DateRangeCrit = "";
            String FromDate = "", ToDate = "";
            DateTime dtFrom, dtTo;
            String PeriodCrit = "";
            String Period = "";
            String PreviousPeriod = "";
            DataTable dtSelectedValues = new DataTable();
            dtSelectedValues.Columns.Add("Code");
            Int32 i = 0, totalCount= 0;
            try
            {
                 
                //Program.objHrmsUI.printRpt(rptCode, true, cri);
                //Select Report id
                ReportID = cmbReport.Selected.Value;
                if (ReportID == "0")
                {
                    oApplication.StatusBar.SetText("", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.ReportId.ToString() == ReportID select a).FirstOrDefault();

                //************************************
                //Creating The Criteria
                //************************************
                Critaria += " Where 1=1 ";
                //Employee
                if (Convert.ToBoolean(oReport.FlgEmployee))
                {

                    EmployeeCrit += " AND EmployeeCode IN ";
                    mtEmployee.FlushToDataSource();
                    for (i = 0; i < dtEmployee.Rows.Count; i++)
                    {
                        String Selection = dtEmployee.GetValue(eIsSelected.DataBind.Alias, i);
                        String EmpidToMerge = dtEmployee.GetValue(eEmpId.DataBind.Alias, i);
                        if (Selection == "Y")
                        {
                            DataRow drOne = dtSelectedValues.NewRow();
                            drOne[0] = EmpidToMerge;
                            dtSelectedValues.Rows.Add(drOne);
                        }
                    }
                    totalCount = dtSelectedValues.Rows.Count;
                    i = 0;
                    for (i = 0; i < totalCount; i++)
                    {
                        if (i== 0)
                        {
                            if (i == totalCount - 1)
                            {
                                EmployeeCrit += "('" + dtSelectedValues.Rows[i][0].ToString() + "')";
                            }
                            else
                            {
                                EmployeeCrit += "('" + dtSelectedValues.Rows[i][0].ToString() + "',";
                            }
                        }
                        if (i == totalCount - 1)
                        {
                            if (i == 0)
                            {
                            }
                            else
                            {
                                EmployeeCrit += "'" + dtSelectedValues.Rows[i][0].ToString() + "')";
                            }
                        }
                        if (i != 0 && i != totalCount - 1)
                        {
                            EmployeeCrit += "'" + dtSelectedValues.Rows[i][0].ToString() + "',";
                        }
                    }
                    if (totalCount > 0)
                    {
                        Critaria += EmployeeCrit;
                    }
                }

                //Department Section
                dtSelectedValues.Rows.Clear();
                if (Convert.ToBoolean(oReport.FlgDept))
                {
                    DepartmentCrit += " AND DepartmentCode IN ";
                    mtDepartment.FlushToDataSource();
                    for (i = 0; i < dtDepartment.Rows.Count; i++)
                    {
                        String Selection = dtDepartment.GetValue(dIsSelected.DataBind.Alias, i);
                        String DeptCode = dtDepartment.GetValue(dCode.DataBind.Alias, i);
                        if (Selection == "Y")
                        {
                            DataRow drOne = dtSelectedValues.NewRow();
                            drOne[0] = DeptCode;
                            dtSelectedValues.Rows.Add(drOne);
                        }
                    }
                    totalCount = dtSelectedValues.Rows.Count;
                    i = 0;
                    for (i = 0; i < totalCount; i++)
                    {
                        if (i == 0)
                        {
                            if (i == totalCount - 1)
                            {
                                DepartmentCrit += "('" + dtSelectedValues.Rows[i][0].ToString() + "')";
                            }
                            else
                            {
                                DepartmentCrit += "('" + dtSelectedValues.Rows[i][0].ToString() + "',";
                            }
                        }
                        if (i == totalCount - 1)
                        {
                            if (i == 0)
                            {
                            }
                            else
                            {
                                DepartmentCrit += "'" + dtSelectedValues.Rows[i][0].ToString() + "')";
                            }
                        }
                        if (i != 0 && i != totalCount - 1)
                        {
                            DepartmentCrit += "'" + dtSelectedValues.Rows[i][0].ToString() + "',";
                        }
                    }
                    if (totalCount > 0)
                    {
                        Critaria += DepartmentCrit;
                    }
                }
                //Location
                dtSelectedValues.Rows.Clear();
                if (Convert.ToBoolean(oReport.FlgLocation))
                {
                    LocationCrit += " AND LocationCode IN ";
                    mtLocation.FlushToDataSource();
                    for (i = 0; i < dtLocation.Rows.Count; i++)
                    {
                        String Selection = dtLocation.GetValue(lIsSelected.DataBind.Alias, i);
                        String LocationCode = dtLocation.GetValue(lCode.DataBind.Alias, i);
                        if (Selection == "Y")
                        {
                            DataRow drOne = dtSelectedValues.NewRow();
                            drOne[0] = LocationCode;
                            dtSelectedValues.Rows.Add(drOne);
                        }
                    }
                    totalCount = dtSelectedValues.Rows.Count;
                    i = 0;
                    for (i = 0; i < totalCount; i++)
                    {
                        if (i == 0)
                        {
                            if (i == totalCount - 1)
                            {
                                LocationCrit += "('" + dtSelectedValues.Rows[i][0].ToString() + "'";
                            }
                            else
                            {
                                LocationCrit += "('" + dtSelectedValues.Rows[i][0].ToString() + "',";
                            }
                        }
                        if (i == totalCount - 1)
                        {
                            if (i == 0)
                            {
                                LocationCrit += ")";
                            }
                            else
                            {
                                LocationCrit += "'" + dtSelectedValues.Rows[i][0].ToString() + "')";
                            }
                        }
                        if (i != 0 && i != totalCount - 1)
                        {
                            LocationCrit += "'" + dtSelectedValues.Rows[i][0].ToString() + "',";
                        }
                    }
                    if (totalCount > 0)
                    {
                        Critaria += LocationCrit;
                    }
                }

                //Date Range
                if (Convert.ToBoolean(oReport.FlgDateFrom) && !Convert.ToBoolean(oReport.FlgDateTo))
                {
                    dtFrom = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    FromDate += "" + dtFrom.Month + "/" + dtFrom.Day + "/" + dtFrom.Year;
                }
                if (Convert.ToBoolean(oReport.FlgDateFrom) && Convert.ToBoolean(oReport.FlgDateTo))
                {
                    if (!String.IsNullOrEmpty(txtFromDate.Value))
                    {
                        dtFrom = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        FromDate += "" + dtFrom.Month + "/" + dtFrom.Day + "/" + dtFrom.Year; 
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Select From Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (!String.IsNullOrEmpty(txtToDate.Value))
                    {
                        dtTo = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        ToDate += "" + dtTo.Month + "/" + dtTo.Day + "/" + dtTo.Year; 
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Select To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    DateRangeCrit += " AND DateRange Between '" + FromDate + "' And '" + ToDate + "' ";

                    Critaria += DateRangeCrit;
                }
                #region PeriodReports

                if (!String.IsNullOrEmpty(cbPayroll.Value))
                {
                    if (Convert.ToBoolean(oReport.FlgPreviousPeriod))
                    {
                        if (String.IsNullOrEmpty(cbPeriod.Value))
                        {
                            oApplication.StatusBar.SetText("Select Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        //cbPeriod.Selected.Value.IndexOf();
                        Int32 previosperiodid = 0;
                        Int32 selectedperiodid = Convert.ToInt32(cbPeriod.Selected.Value);
                        Int32 selectedpayroll = Convert.ToInt32(cbPayroll.Selected.Value);
                        var oPeriodsCollection = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == selectedpayroll select a).ToList();
                        for (int j = 0; j < oPeriodsCollection.Count; j++)
                        {
                            if (selectedperiodid == oPeriodsCollection[j].ID)
                            {
                                previosperiodid = oPeriodsCollection[j - 1].ID;
                            }
                        }

                        var oPeriodCurrent = (from a in dbHrPayroll.CfgPeriodDates where a.ID == selectedperiodid select a).FirstOrDefault();
                        var oPeriodPrevious = (from a in dbHrPayroll.CfgPeriodDates where a.ID == previosperiodid select a).FirstOrDefault();

                        PeriodCrit += " AND PeriodName IN ('" + oPeriodPrevious.PeriodName + "','" + oPeriodCurrent.PeriodName + "')";
                        

                        Critaria += PeriodCrit;
                    }
                    //Previous
                    
                    if (Convert.ToBoolean(oReport.FlgPeriod))
                    {
                        if (String.IsNullOrEmpty(cbPeriod.Value))
                        {
                            oApplication.StatusBar.SetText("Select Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        Period = cbPeriod.Selected.Description;
                        
                        PeriodCrit += " AND PeriodName = '" + Period + "'";                        

                        Critaria += PeriodCrit;
                    }
                    //
                    //WIth Out Parameters
                    if (Convert.ToBoolean(oReport.FlgCritaria))
                    {
                        if (String.IsNullOrEmpty(cbPeriod.Value))
                        {
                            oApplication.StatusBar.SetText("Select Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        Period = cbPeriod.Selected.Description;
                        CurrentPeriod = Period;
                        PeriodCrit += "";

                        Critaria += PeriodCrit;
                        Program.objHrmsUI.printRpt(oReport.RptCode, true, "", CurrentPeriod);
                    }
                    else
                    {
                        Program.objHrmsUI.printRpt(oReport.RptCode, true, Critaria, CurrentPeriod, FromDate);
                    }
                    //
                }
                else
                {
                    oApplication.StatusBar.SetText("Select Payroll Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                #endregion
                //************************************
                 // Program.objHrmsUI.printRpt(oReport.RptCode, true, Critaria, CurrentPeriod);
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception ShowReport : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetParameterBasedOnSelection(String pReportID)
        {
            try
            {
                TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.ReportId.ToString() == pReportID select a).FirstOrDefault();
                if (oReport != null)
                {
                    if (Convert.ToBoolean(oReport.FlgEmployee))
                    {
                        imtEmployee.Enabled = true;
                    }
                    else
                    {
                        imtEmployee.Enabled = false;
                    }
                    if (Convert.ToBoolean(oReport.FlgDept))
                    {
                        imtDepartment.Enabled = true;
                    }
                    else
                    {
                        imtDepartment.Enabled = false;
                    }
                    if (Convert.ToBoolean(oReport.FlgLocation))
                    {
                        imtLocation.Enabled = true;
                    }
                    else
                    {
                        imtLocation.Enabled = false;
                    }
                    if (Convert.ToBoolean(oReport.FlgDateFrom))
                    {
                        itxtFromDate.Enabled = true;
                    }
                    else
                    {
                        itxtFromDate.Enabled = false;
                    }
                    if (Convert.ToBoolean(oReport.FlgDateTo))
                    {
                        itxtToDate.Enabled = true;
                    }
                    else
                    {
                        itxtToDate.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        private void SelectAllEmployee()
        {
            try
            {
                oForm.Freeze(true);
                SAPbouiCOM.Column col = mtEmployee.Columns.Item("sel");

                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtEmployee.Rows.Count; i++)
                    {

                        dtEmployee.SetValue(eIsSelected.DataBind.Alias, i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmployee.Rows.Count; i++)
                    {
                        dtEmployee.SetValue(eIsSelected.DataBind.Alias, i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                mtEmployee.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
            }
        }

        private void SelectAllDepartment()
        {
            try
            {
                oForm.Freeze(true);
                if (dIsSelected.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtDepartment.Rows.Count; i++)
                    {

                        dtDepartment.SetValue(dIsSelected.DataBind.Alias, i, "N");
                        dIsSelected.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtDepartment.Rows.Count; i++)
                    {
                        dtDepartment.SetValue(dIsSelected.DataBind.Alias, i, "Y");
                        dIsSelected.TitleObject.Caption = "X";
                    }
                }
                mtDepartment.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
            }
        }
        
        private void SelectAllLocation()
        {
            try
            {
                oForm.Freeze(true);
                if (lIsSelected.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtLocation.Rows.Count; i++)
                    {

                        dtLocation.SetValue(lIsSelected.DataBind.Alias, i, "N");
                        lIsSelected.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtLocation.Rows.Count; i++)
                    {
                        dtLocation.SetValue(lIsSelected.DataBind.Alias, i, "Y");
                        lIsSelected.TitleObject.Caption = "X";
                    }
                }
                mtLocation.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
