using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using System.Collections;

namespace ACHR.Screen
{
    class frm_TaxAdj : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.EditText txtEmployeeID, txtEmployeeName, txtBasicSalary, txtGrossSalary;
        SAPbouiCOM.EditText txtDepartment, txtDesignation, txtLocation, txtBranch;
        SAPbouiCOM.EditText txtTaxableAmountperYear, txtTaxableAmountperMonth;
        SAPbouiCOM.EditText txtTApaCurrent, txtTApmCurrent;
        SAPbouiCOM.Button btnOk, btnCancel;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix mtMain;
        SAPbouiCOM.Column cDesc, cAmount, cID, cActive, cMonthly;
        SAPbouiCOM.Item ibtnOk, ibtnCancel;

        Boolean flgAddSuccess = false;
        Boolean flgLoadedRecord = false;
        Int32 DocId = 0;
        Decimal empBasicSalary = 0;
        Decimal empGrossSalary = 0;
        Decimal ExpectedTaxYear = 0;
        Decimal ExpectedTaxMonth = 0;
        Decimal CurrentTaxYear = 0;
        Decimal CurrentTaxMonth = 0;
        MstEmployee oEmployee = null;
        IEnumerable<MstEmployee> oEmployees = null;
        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Initialize Form Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    //doSomething();
                    break;
                case "btPick":
                    doFind();
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    doSomething();
                    break;
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmployeeID == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmployeeID.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            } 
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "mtMain" && pVal.ColUID == "clActive")
            {
                oForm.Freeze(true);
                mtMain.FlushToDataSource();
                AddEmptyRow();
                oForm.Freeze(false);
            }
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

        }
        #endregion

        #region Functions

        private void InitiallizeForm()
        {

            try
            {
                //oForm.DataSources.UserDataSources.Add("txthfname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                //txtFirstName = oForm.Items.Item("txthfname").Specific;
                //itxtFirstName = oForm.Items.Item("txthfname");
                //txtFirstName.DataBind.SetBound(true, "", "txthfname");

                txtEmployeeID = oForm.Items.Item("txEmpid").Specific;
                oForm.DataSources.UserDataSources.Add("txEmpid", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmployeeID.DataBind.SetBound(true, "", "txEmpid");


                txtEmployeeName = oForm.Items.Item("txName").Specific;
                oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtEmployeeName.DataBind.SetBound(true, "", "txName");

                txtDepartment = oForm.Items.Item("txDept").Specific;
                oForm.DataSources.UserDataSources.Add("txDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDepartment.DataBind.SetBound(true, "", "txDept");

                txtDesignation = oForm.Items.Item("txDesig").Specific;
                oForm.DataSources.UserDataSources.Add("txDesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDesignation.DataBind.SetBound(true, "", "txDesig");

                txtLocation = oForm.Items.Item("txLoc").Specific;
                oForm.DataSources.UserDataSources.Add("txLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtLocation.DataBind.SetBound(true, "", "txLoc");

                txtBranch = oForm.Items.Item("txBrnch").Specific;
                oForm.DataSources.UserDataSources.Add("txBrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtBranch.DataBind.SetBound(true, "", "txBrnch");

                txtBasicSalary = oForm.Items.Item("txBS").Specific;
                oForm.DataSources.UserDataSources.Add("txBS", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txBS");

                txtGrossSalary = oForm.Items.Item("txGS").Specific;
                oForm.DataSources.UserDataSources.Add("txGS", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossSalary.DataBind.SetBound(true, "", "txGS");

                txtTaxableAmountperYear = oForm.Items.Item("txTAY").Specific;
                oForm.DataSources.UserDataSources.Add("txTAY", SAPbouiCOM.BoDataType.dt_SUM);
                txtTaxableAmountperYear.DataBind.SetBound(true, "", "txTAY");

                txtTaxableAmountperMonth = oForm.Items.Item("txTAM").Specific;
                oForm.DataSources.UserDataSources.Add("txTAM", SAPbouiCOM.BoDataType.dt_SUM);
                txtTaxableAmountperMonth.DataBind.SetBound(true, "", "txTAM");

                txtTApaCurrent = oForm.Items.Item("txTAYC").Specific;
                oForm.DataSources.UserDataSources.Add("txTAYC", SAPbouiCOM.BoDataType.dt_SUM);
                txtTApaCurrent.DataBind.SetBound(true, "", "txTAYC");

                txtTApmCurrent = oForm.Items.Item("txTAMC").Specific;
                oForm.DataSources.UserDataSources.Add("txTAMC", SAPbouiCOM.BoDataType.dt_SUM);
                txtTApmCurrent.DataBind.SetBound(true, "", "txTAMC");

                mtMain = oForm.Items.Item("mtMain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtDetail");
                cDesc = mtMain.Columns.Item("clDesc");
                cDesc.Width = 300;
                cAmount = mtMain.Columns.Item("clAmount");
                cAmount.Width = 80;
                cID = mtMain.Columns.Item("clID");
                cID.Visible = false;
                cActive = mtMain.Columns.Item("clActive");
                cActive.Width = 80;
                cMonthly = mtMain.Columns.Item("clMon");
                cMonthly.Width = 80;
                btnOk = oForm.Items.Item("1").Specific;
                //ibtnOk = oForm.Items.Item("btProcess");
                btnCancel = oForm.Items.Item("2").Specific;
                

                //mtOthElements = oForm.Items.Item("mtOEle").Specific;
                //dtOthElements = oForm.DataSources.DataTables.Item("dtOtherEle");


                //GetData();
                GetDataFilterData();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
        }

        private void doFind()
        {
            OpenNewSearchForm();
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
                oApplication.StatusBar.SetText("Function : OpenNewSearchForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == Program.EmpID select a).FirstOrDefault();
                    if (oEmp != null)
                    {
                        oEmployee = oEmp;
                        txtEmployeeID.Value = oEmp.EmpID;
                        txtEmployeeName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                        txtDepartment.Value = oEmp.DepartmentName;
                        txtDesignation.Value = oEmp.DesignationName;
                        txtLocation.Value = oEmp.LocationName;
                        txtBranch.Value = oEmp.BranchName;

                        txtBasicSalary.Value = Convert.ToString(oEmp.BasicSalary);
                        empBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                        empGrossSalary = ds.getEmpGross(oEmp);
                        txtGrossSalary.Value = Convert.ToString(empGrossSalary);

                        //Check for record
                        TrnsTaxAdjustment oTA = (from a in dbHrPayroll.TrnsTaxAdjustment where a.EmpID == oEmp.ID && a.FlgActive == true select a).FirstOrDefault();
                        if (oTA != null)
                        {
                            FillRecord(oTA);                            
                        }
                        else
                        {
                            AddEmptyRow();
                            btnOk.Caption = "Add";
                        }
                        oApplication.StatusBar.SetText("Employee Set Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                                      
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void doSomething()
        {
            try
            {
                if (btnOk.Caption == "Add")
                {
                    int confirm = oApplication.MessageBox("Are you sure you want to post Tax Adjustment? ", 2, "Yes", "No");
                    if (confirm == 2) return;
                    if (AddRecord())
                    {
                        ClearRecord();
                    }
                }
                if (btnOk.Caption == "Update")
                {
                    int confirm = oApplication.MessageBox("Are you sure you want to post Tax Adjustment? ", 2, "Yes", "No");
                    if (confirm == 2) return;
                    if (UpdateRecord())
                    {
                        ClearRecord();
                    }
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private Boolean AddRecord()
        {
            Boolean flgReturn = true;
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                if (oEmp != null)
                {
                    TrnsTaxAdjustment oTA = new TrnsTaxAdjustment();

                    oTA.MstEmployee = oEmp;
                    oTA.FlgActive = true;
                    oTA.CreatedBy = oCompany.UserName;
                    oTA.CreateDt = DateTime.Now;
                    //oTA.UpdatedBy = oCompany.UserName;
                    //oTA.UpdateDt = DateTime.Now;

                    dtMain.Rows.Clear();
                    mtMain.FlushToDataSource();
                    for (Int16 i = 0; i < dtMain.Rows.Count; i++)
                    {
                        if (String.IsNullOrEmpty(dtMain.GetValue(cDesc.DataBind.Alias, i)))
                        {
                            continue;
                        }
                        else
                        {

                            String Description = "";
                            String MonthlyValue = "N";
                            Decimal Amount = 0;
                            Boolean flgCheck = true;
                            Boolean flgMonthly = false;

                            TrnsTaxAdjustmentDetails oDetail = new TrnsTaxAdjustmentDetails();

                            Description = Convert.ToString(dtMain.GetValue(cDesc.DataBind.Alias, i));
                            Amount = Convert.ToDecimal(dtMain.GetValue(cAmount.DataBind.Alias, i));
                            MonthlyValue = Convert.ToString(dtMain.GetValue(cMonthly.DataBind.Alias,i));

                            if (MonthlyValue == "Y")
                                flgMonthly = true;
                            else
                                flgMonthly = false;

                            oDetail.Description = Description;
                            oDetail.Amount = Amount;
                            oDetail.CreatedBy = oCompany.UserName;
                            oDetail.UpdatedBy = oCompany.UserName;
                            oDetail.CreateDt = DateTime.Now;
                            oDetail.UpdateDt = DateTime.Now;
                            oDetail.FlgActive = flgCheck;
                            oDetail.FlgMonthly = flgMonthly;
                            oTA.TrnsTaxAdjustmentDetails.Add(oDetail);
                        }
                    }
                    dbHrPayroll.TrnsTaxAdjustment.InsertOnSubmit(oTA);
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Record Added Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    flgReturn = false;
                    oApplication.StatusBar.SetText("Employee not found." , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Record didn't added. error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                flgReturn = false;
            }
            return flgReturn;
        }

        private Boolean UpdateRecord()
        {
            Boolean flgReturn = true;
            try
            {
                if (!(flgLoadedRecord == true) && !(DocId > 0))
                {
                    flgReturn = false;
                    return flgReturn;
                }
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                if (oEmp != null)
                {
                    TrnsTaxAdjustment oTA = (from a in dbHrPayroll.TrnsTaxAdjustment where a.ID == DocId select a).FirstOrDefault();

                    if (oTA != null)
                    {
                        oTA.FlgActive = true;
                        oTA.UpdatedBy = oCompany.UserName;
                        oTA.UpdateDt = DateTime.Now;

                        dtMain.Rows.Clear();
                        mtMain.FlushToDataSource();
                        for (Int16 i = 0; i < dtMain.Rows.Count; i++)
                        {
                            if (String.IsNullOrEmpty(dtMain.GetValue(cDesc.DataBind.Alias, i)))
                            {
                                continue;
                            }
                            else
                            {

                                String Description = "";
                                String CheckValue = "N";
                                String MonthlyValue = "N";
                                Int32 ID = 0;
                                Decimal Amount = 0;
                                Boolean flgCheck = true;
                                Boolean flgMonthly = false;
                                

                                Description = Convert.ToString(dtMain.GetValue(cDesc.DataBind.Alias, i));
                                Amount = Convert.ToDecimal(dtMain.GetValue(cAmount.DataBind.Alias, i));
                                ID = Convert.ToInt32(dtMain.GetValue(cID.DataBind.Alias, i));
                                CheckValue = Convert.ToString(dtMain.GetValue(cActive.DataBind.Alias, i));
                                MonthlyValue = Convert.ToString(dtMain.GetValue(cMonthly.DataBind.Alias, i));

                                if (CheckValue == "Y")
                                    flgCheck = true;
                                else
                                    flgCheck = false;

                                if (MonthlyValue == "Y")
                                    flgMonthly = true;
                                else
                                    flgMonthly = false;

                                if (ID == 0)
                                {
                                    TrnsTaxAdjustmentDetails oDetail = new TrnsTaxAdjustmentDetails();
                                    oDetail.Description = Description;
                                    oDetail.Amount = Amount;
                                    oDetail.CreatedBy = oCompany.UserName;
                                    oDetail.UpdatedBy = oCompany.UserName;
                                    oDetail.CreateDt = DateTime.Now;
                                    oDetail.UpdateDt = DateTime.Now;
                                    oDetail.FlgActive = flgCheck;
                                    oDetail.FlgMonthly = flgMonthly;
                                    oTA.TrnsTaxAdjustmentDetails.Add(oDetail);
                                }
                                else
                                {
                                    TrnsTaxAdjustmentDetails oDetail = (from a in dbHrPayroll.TrnsTaxAdjustmentDetails where a.ID == ID select a).FirstOrDefault();
                                    oDetail.Description = Description;
                                    oDetail.Amount = Amount;
                                    oDetail.UpdatedBy = oCompany.UserName;
                                    oDetail.UpdateDt = DateTime.Now;
                                    oDetail.FlgActive = flgCheck;
                                    //Can't unable or change status once yearly ya monthly line was entered.
                                    //oDetail.FlgMonthly = flgMonthly; 
                                }
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        oApplication.StatusBar.SetText("Record Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                }
            }
            catch (Exception Ex)
            {
                flgReturn = false;
                oApplication.StatusBar.SetText("Record didn't updated. error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            return flgReturn;
        }

        private void FillRecord(TrnsTaxAdjustment pTA)
        {
            try
            {
                if (pTA != null)
                {
                    mtMain.FlushToDataSource();
                    dtMain.Rows.Clear();
                    Int32 RowCounts = 0;
                    foreach (TrnsTaxAdjustmentDetails OneRec in pTA.TrnsTaxAdjustmentDetails)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(cDesc.DataBind.Alias, RowCounts, OneRec.Description);
                        dtMain.SetValue(cAmount.DataBind.Alias, RowCounts, Convert.ToString(OneRec.Amount));
                        dtMain.SetValue(cActive.DataBind.Alias, RowCounts, OneRec.FlgActive == true ? "Y" : "N");
                        dtMain.SetValue(cMonthly.DataBind.Alias, RowCounts, OneRec.FlgMonthly == true ? "Y" : "N");
                        dtMain.SetValue(cID.DataBind.Alias, RowCounts, OneRec.ID);
                        RowCounts++;
                    }
                    mtMain.LoadFromDataSourceEx(true);
                    DocId = pTA.ID;
                    flgLoadedRecord = true;
                    btnOk.Caption = "Update";
                    AddEmptyRow();
                    CalculateEmployeeTax();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtMain.Rows.Count == 0)
            {
                dtMain.Rows.Add(1);
                RowValue = dtMain.Rows.Count;
                dtMain.SetValue(cDesc.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(cAmount.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(cActive.DataBind.Alias, RowValue - 1, "Y");
                dtMain.SetValue(cMonthly.DataBind.Alias, RowValue - 1, "Y");
                dtMain.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                mtMain.AddRow(1, 0);
            }
            else
            {
                if (dtMain.GetValue(cDesc.DataBind.Alias, dtMain.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(cDesc.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(cAmount.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(cActive.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(cMonthly.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                    mtMain.AddRow(1, mtMain.RowCount);
                }
            }
            mtMain.LoadFromDataSource();
        }

        private void ClearRecord()
        {
            try
            {
                Program.EmpID = "";
                txtEmployeeID.Value = "";
                txtEmployeeName.Value = "";
                txtDesignation.Value = "";
                txtDepartment.Value = "";
                txtLocation.Value = "";
                txtBranch.Value = "";
                txtBasicSalary.Value = "0";
                txtGrossSalary.Value = "0";
                txtTApaCurrent.Value = "0";
                txtTApmCurrent.Value = "0";
                txtTaxableAmountperYear.Value = "0";
                txtTaxableAmountperMonth.Value = "0";
                flgLoadedRecord = false;
                DocId = 0;
                dtMain.Rows.Clear();
                mtMain.LoadFromDataSource();
                AddEmptyRow();
            }
            catch (Exception Ex)
            {
            }
        }

        private void CalculateEmployeeTax()
        {
            try
            {
                #region Variable

                Decimal AllRowValue = 0;
                Decimal ExpectedYearlySalary = 0;
                
                Decimal temp = 0;
                #endregion

                mtMain.FlushToDataSource();
                for (Int32 i = 0; i < dtMain.Rows.Count; i++)
                {
                    string CheckValue = Convert.ToString(dtMain.GetValue(cActive.DataBind.Alias, i));
                    if (CheckValue == "Y")
                    {
                        AllRowValue += Convert.ToDecimal(dtMain.GetValue(cAmount.DataBind.Alias, i));
                    }
                }

                CfgPayrollDefination oPayroll = (from a in dbHrPayroll.CfgPayrollDefination where a.ID == oEmployee.PayrollID select a).FirstOrDefault();
                CfgPeriodDates oPeriadDate = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == oPayroll.ID && a.StartDate <= DateTime.Now && a.EndDate > DateTime.Now select a).FirstOrDefault();

                ExpectedYearlySalary = (empGrossSalary * 12);
                Int32 cnt = (from p in dbHrPayroll.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == oPeriadDate.CalCode where p.MinAmount <= ExpectedYearlySalary && p.MaxAmount >= ExpectedYearlySalary select p).Count();
                if (cnt > 0)
                {
                    CfgTaxDetail taxLine = (from p in dbHrPayroll.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == oPeriadDate.CalCode where p.MinAmount <= ExpectedYearlySalary && p.MaxAmount >= ExpectedYearlySalary select p).Single();
                    ExpectedTaxYear = (decimal)taxLine.FixTerm + (decimal)(ExpectedYearlySalary - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;
                    ExpectedTaxMonth = ExpectedTaxYear / 12;
                    CurrentTaxYear = (ExpectedTaxYear + AllRowValue);
                    CurrentTaxMonth = CurrentTaxYear / 12;
                    txtTApaCurrent.Value = Convert.ToString(CurrentTaxYear);
                    txtTApmCurrent.Value = Convert.ToString(CurrentTaxMonth);
                    txtTaxableAmountperYear.Value = Convert.ToString(ExpectedTaxYear);
                    txtTaxableAmountperMonth.Value = Convert.ToString(ExpectedTaxMonth);
                }

                //oApplication.StatusBar.SetText("error : " + Convert.ToString(CurrentTaxYear), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            catch (Exception Ex)
            {
            }
        }

        private void ProcessSalary(Int32 PayrollID, Int32 PeriodID, String EmpID )
        {
            string strProcessing = "";
            try
            {
                IEnumerable<MstEmployee> emps = from p in dbHrPayroll.MstEmployee select p;
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, emps);
                Hashtable elementGls = new Hashtable();

                int totalCnt = 0;


                CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID == PayrollID select p).Single();
                CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID == PeriodID select p).Single();
                int periodDays = 0;
                periodDays = Convert.ToInt16(payroll.WorkDays);
                decimal empBasicSalary = 0;
                decimal empGrossSalary = 0;

                try
                {
                    #region Basic Salary
                    decimal amnt = 0.0M;
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == EmpID select p).FirstOrDefault();
                    MstGLDetermination glDetr = ds.getEmpGl(emp);
                    if (glDetr == null)
                    {
                        return;
                    }
                    empBasicSalary = (decimal)emp.BasicSalary;
                    decimal spTaxbleAmnt = 0.00M;
                    decimal spTaxableAmntOT = 0.00M;
                    decimal spTaxableAmntLWOP = 0.00M;
                    decimal DaysCnt = 0;
                    decimal payDays = 0.00M;
                    decimal leaveDays = 0.00M;
                    decimal monthDays = 0.00M;
                    decimal nonRecurringTaxable = 0.00M;
                    decimal payRatio = 1.00M;
                    decimal payRatioWithLeaves = 1.00M;


                    DaysCnt = ds.getDaysCnt(emp, payrollperiod, out payDays, out leaveDays, out monthDays);
                    decimal employeeRemainingSalary = 0.00M;
                    payRatio = payDays / monthDays;
                    payRatioWithLeaves = (payDays - leaveDays) / monthDays;

                    if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
                    {
                        empGrossSalary = ds.getEmpGross(emp, payrollperiod.ID);
                        empBasicSalary = empGrossSalary;

                    }
                    else
                    {
                        empGrossSalary = ds.getEmpGross(emp);
                    }
                    

                    try
                    {
                        employeeRemainingSalary = Math.Round((decimal)empBasicSalary * payRatio, 0);
                    }
                    catch { }
                    TrnsSalaryProcessRegister reg = new TrnsSalaryProcessRegister();
                    reg.MstEmployee = emp;
                    reg.CfgPayrollDefination = payroll;
                    reg.CfgPeriodDates = payrollperiod;
                    reg.EmpBasic = employeeRemainingSalary;//Math.Round(Convert.ToDecimal(empBasicSalary * payRatio), 0);
                    reg.EmpGross = empGrossSalary;
                    reg.CreateDate = DateTime.Now;
                    reg.UpdateDate = DateTime.Now;
                    reg.UserId = oCompany.UserName;
                    reg.UpdatedBy = oCompany.UserName;
                    reg.PeriodName = payrollperiod.PeriodName;
                    reg.PayrollName = payroll.PayrollName;
                    reg.EmpName = emp.FirstName + " " + emp.LastName;
                    //reg.DaysPaid = Convert.ToInt16( payDays);
                    reg.DaysPaid = Convert.ToDecimal(DaysCnt);
                    reg.MonthDays = Convert.ToInt32(monthDays);

                    #endregion

                    #region Processing

                            /// Basic Salary ////
                            /// ************////
                            TrnsSalaryProcessRegisterDetail spdHeadRow = new TrnsSalaryProcessRegisterDetail();
                            spdHeadRow.LineType = "BS";
                            spdHeadRow.LineSubType = "Basic Salary";
                            spdHeadRow.LineValue = Math.Round(employeeRemainingSalary, 0);
                            spdHeadRow.LineMemo = "Basic Salary ";
                            spdHeadRow.DebitAccount = glDetr.BasicSalary;
                            spdHeadRow.CreditAccount = glDetr.BSPayable;
                            spdHeadRow.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
                            spdHeadRow.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
                            spdHeadRow.LineBaseEntry = emp.ID;
                            spdHeadRow.BaseValueCalculatedOn = employeeRemainingSalary;
                            spdHeadRow.BaseValue = employeeRemainingSalary;
                            spdHeadRow.BaseValueType = "FIX";
                            spdHeadRow.CreateDate = DateTime.Now;
                            spdHeadRow.UpdateDate = DateTime.Now;
                            spdHeadRow.UserId = oCompany.UserName;
                            spdHeadRow.UpdatedBy = oCompany.UserName;
                            spdHeadRow.NoOfDay = Convert.ToDecimal(DaysCnt);
                            spdHeadRow.TaxableAmount = employeeRemainingSalary;
                            spTaxbleAmnt += employeeRemainingSalary;
                            // employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdHeadRow);





                            //* AbsentDeductions,Reimbursement

                            //////Absents ////
                            //**************////
                            decimal leaveCnt = 0.00M;
                            //DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt);

                            DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt, glDetr);
                            foreach (DataRow dr in dtAbsentDeduction.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                spTaxableAmntLWOP += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }

                            //DaysCnt -= leaveCnt;
                            //* End of Leave Deductions


                            //* Payroll elements assigned to employee ***Employee Elements ****** 
                            //*******************************************************************

                            DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, payrollperiod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays);
                            foreach (DataRow dr in dtSalPrlElements.Rows)
                            {
                                if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                {
                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = dr["LineType"].ToString();
                                    spdetail.LineSubType = dr["LineSubType"].ToString();
                                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                    spdetail.LineMemo = dr["LineMemo"].ToString();
                                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                    employeeRemainingSalary += (decimal)spdetail.LineValue;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            //******************** End of Elements *********************************



                            //////Over time ////
                            //**************////
                            Int32 otmin = 0;
                            DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, payrollperiod, empGrossSalary, out otmin);

                            //Code modified by Zeeshan

                            foreach (DataRow dr in dtSalOverTimes.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxableAmntOT += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }



                            // * ************Advance Recovery Processing **************
                            //*******************************************************
                            DataTable dtAdvance = ds.salaryProcessingAdvance(emp, employeeRemainingSalary, payrollperiod);

                            foreach (DataRow dr in dtAdvance.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = 0.00M;
                                employeeRemainingSalary += (decimal)spdetail.LineValue;


                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }



                            // * ************Loan Recovery Processing **************

                            DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary, payrollperiod);

                            foreach (DataRow dr in dtLoands.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                spdetail.TaxableAmount = 0.00M;
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }
                            reg.EmpTaxblTotal = spTaxbleAmnt;

                            // * ************TAX**************
                            if (Program.systemInfo.TaxSetup == true && emp.FlgTax == true)
                            {
                                //decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable);
                                decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, payRatio);
                                if (TotalTax >= 0)
                                {
                                    reg.EmpTotalTax = TotalTax;

                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = "Tax";
                                    spdetail.LineSubType = "Tax";
                                    spdetail.LineValue = -Math.Round(TotalTax, 0);
                                    spdetail.LineMemo = "Tax Deduction";
                                    spdetail.DebitAccount = glDetr.IncomeTaxExpense;
                                    spdetail.CreditAccount = glDetr.IncomeTaxPayable;
                                    spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
                                    spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
                                    spdetail.LineBaseEntry = 0;
                                    spdetail.BaseValueCalculatedOn = spTaxbleAmnt;
                                    spdetail.BaseValue = spTaxbleAmnt;
                                    spdetail.BaseValueType = "FIX";
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = 0.00M;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            

                            if (emp.CfgPayrollDefination.FlgGratuity == true)
                            {
                                int gratCnt = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).Count();
                                if (gratCnt > 0)
                                {
                                    MstGratuity empGrat = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).FirstOrDefault();

                                    try
                                    {
                                        int FromYr = Convert.ToInt16(empGrat.YearFrom) * 365;

                                        if ((Convert.ToDateTime(payrollperiod.StartDate) - Convert.ToDateTime(emp.JoiningDate)).Days > FromYr)
                                        {
                                            decimal gratProvision = 0.00M;
                                            decimal basedOnAmont = 0.00M;
                                            if (empGrat.BasedOn == "0")
                                            {
                                                basedOnAmont = empBasicSalary;

                                            }
                                            else
                                            {
                                                basedOnAmont = empGrossSalary;
                                            }

                                            gratProvision = (basedOnAmont * (decimal)empGrat.Factor / 100) / 12;
                                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                            spdetail.LineType = "Element";
                                            spdetail.LineSubType = "Empr Cont";
                                            spdetail.LineValue = Math.Round(gratProvision, 0);
                                            spdetail.LineMemo = "Gratuity";
                                            spdetail.DebitAccount = glDetr.GratuityExpense;
                                            spdetail.CreditAccount = glDetr.GratuityPayable;
                                            spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityExpense);
                                            spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityPayable);
                                            spdetail.LineBaseEntry = 0;
                                            spdetail.BaseValueCalculatedOn = empBasicSalary;
                                            spdetail.BaseValue = empBasicSalary;
                                            spdetail.BaseValueType = "FIX";
                                            spdetail.CreateDate = DateTime.Now;
                                            spdetail.UpdateDate = DateTime.Now;
                                            spdetail.UserId = oCompany.UserName;
                                            spdetail.UpdatedBy = oCompany.UserName;
                                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                            spdetail.TaxableAmount = 0.00M;
                                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }

                    #endregion  
                    reg.SalaryStatus = 0;//Salary Processed
                    dbHrPayroll.TrnsSalaryProcessRegister.InsertOnSubmit(reg);
                    //dbHrPayroll.SubmitChanges();
                }
                catch (Exception ex)
                {
                    oApplication.SetStatusBarMessage(strProcessing + ":" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }
        private void GetDataFilterData()
        {
            try
            {
                CodeIndex.Clear();
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {



                    string strOut = string.Empty;
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                    strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                    //IEnumerable<MstEmployee> oEmployees =(from e in dbHrPayroll.MstEmployee where Convert.ToString(e.PayrollID) == strOut  select e);
                    oEmployees = (from e in dbHrPayroll.MstEmployee where Convert.ToString(e.PayrollID) == strOut select e);
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {
                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;

                }
                else
                {
                    oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {

                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;
                }

            }


            //    IEnumerable<MstEmployee> oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
            //    Int32 i = 0;
            //    foreach (MstEmployee oEmp in oEmployees)
            //    {
            //        CodeIndex.Add(oEmp.ID.ToString(), i);
            //        i++;
            //    }
            //    totalRecord = i;
            //}
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }
        #endregion

    }
}
