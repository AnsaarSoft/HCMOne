using System;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_FS : HRMSBaseForm
    {
        #region Local Variable Area
        SAPbouiCOM.EditText txtEmployeeID, txtEmployeeName, txtArrears, txtContributions, txtNetPayable, txtBasicSalary, txtGrossSalary;
        SAPbouiCOM.EditText txtDepartment, txtDesignation, txtJoiningDate, txtResignationDate;
        SAPbouiCOM.EditText txtAdjustment, txtTerminationDate, txtRemarks;
        SAPbouiCOM.Button btnProcess, btnPick, btnVoid;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix mtMain;

        IEnumerable<TrnsResignation> oCollection = null;
        MstEmployee EmployeeCode = null ;
        Int32 NoOfDays = 0, PayrollID = 0, ResignPeriodID = 0;
        DateTime JoiningDate, ResignDate, TerminationDate;
        String ResignPeriodName = "";

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
                case "btProcess":
                    Processed();
                    break;
                case "btpick":
                    doFind();
                    break;
                case "btvoid":
                    VoidEOSClick();
                    break;
                case "btok":
                    oForm.Close();
                    break;
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
        }

        public override void fillFields()
        {
            base.fillFields();
            SelectEmployee();
        }

        #endregion
        
        #region Local Functions

        private void InitiallizeForm()
        {

            try
            {
                //oForm.DataSources.UserDataSources.Add("txthfname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                //txtFirstName = oForm.Items.Item("txthfname").Specific;
                //itxtFirstName = oForm.Items.Item("txthfname");
                //txtFirstName.DataBind.SetBound(true, "", "txthfname");



                txtEmployeeID = oForm.Items.Item("txemp").Specific;
                oForm.DataSources.UserDataSources.Add("txemp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtEmployeeID.DataBind.SetBound(true, "", "txemp");
                

                txtEmployeeName = oForm.Items.Item("txempname").Specific;
                oForm.DataSources.UserDataSources.Add("txempname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtEmployeeName.DataBind.SetBound(true, "", "txempname");

                txtBasicSalary = oForm.Items.Item("txbasic").Specific;
                oForm.DataSources.UserDataSources.Add("txbasic", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txbasic");

                txtGrossSalary = oForm.Items.Item("txgrslry").Specific;
                oForm.DataSources.UserDataSources.Add("txgrslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossSalary.DataBind.SetBound(true, "", "txgrslry");

                txtArrears = oForm.Items.Item("txarr").Specific;
                oForm.DataSources.UserDataSources.Add("txarr", SAPbouiCOM.BoDataType.dt_SUM);
                txtArrears.DataBind.SetBound(true, "", "txarr");


                txtContributions = oForm.Items.Item("txcon").Specific;
                oForm.DataSources.UserDataSources.Add("txcon", SAPbouiCOM.BoDataType.dt_SUM);
                txtContributions.DataBind.SetBound(true, "", "txcon");
                

                txtNetPayable = oForm.Items.Item("txnetpay").Specific;
                oForm.DataSources.UserDataSources.Add("txnetpay", SAPbouiCOM.BoDataType.dt_SUM);
                txtNetPayable.DataBind.SetBound(true, "", "txnetpay");

                txtAdjustment = oForm.Items.Item("txAdj").Specific;
                oForm.DataSources.UserDataSources.Add("txAdj", SAPbouiCOM.BoDataType.dt_SUM);
                txtAdjustment.DataBind.SetBound(true, "", "txAdj");

                txtRemarks = oForm.Items.Item("txremarks").Specific;
                oForm.DataSources.UserDataSources.Add("txremarks", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtRemarks.DataBind.SetBound(true, "", "txremarks");

                txtDepartment = oForm.Items.Item("txdept").Specific;
                oForm.DataSources.UserDataSources.Add("txdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDepartment.DataBind.SetBound(true, "", "txdept");

                txtDesignation = oForm.Items.Item("txdesig").Specific;
                oForm.DataSources.UserDataSources.Add("txdesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDesignation.DataBind.SetBound(true, "", "txdesig");

                txtJoiningDate = oForm.Items.Item("txjdate").Specific;
                oForm.DataSources.UserDataSources.Add("txjdate", SAPbouiCOM.BoDataType.dt_DATE);
                txtJoiningDate.DataBind.SetBound(true, "", "txjdate");

                txtResignationDate = oForm.Items.Item("txrdate").Specific;
                oForm.DataSources.UserDataSources.Add("txrdate", SAPbouiCOM.BoDataType.dt_DATE);
                txtResignationDate.DataBind.SetBound(true, "", "txrdate");

                txtTerminationDate = oForm.Items.Item("txterdate").Specific;
                oForm.DataSources.UserDataSources.Add("txterdate", SAPbouiCOM.BoDataType.dt_DATE);
                txtTerminationDate.DataBind.SetBound(true, "", "txterdate");

                mtMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtdetail");

                btnProcess = oForm.Items.Item("btProcess").Specific;
                btnVoid = oForm.Items.Item("btvoid").Specific;
                btnPick = oForm.Items.Item("btpick").Specific;

                //mtOthElements = oForm.Items.Item("mtOEle").Specific;
                //dtOthElements = oForm.DataSources.DataTables.Item("dtOtherEle");
                

                GetData();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void doFind()
        {

            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("FinalSettlement", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select  Employee");
            pic = null;
            if (st.Rows.Count > 0)
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                currentObjId = st.Rows[0][0].ToString();
                getRecord(currentObjId);

            }
        }

        private void GetData()
        {

            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.TrnsResignation select a;
            Int32 i = 0;
            foreach (TrnsResignation One in oCollection)
            {
                CodeIndex.Add(One.Id.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void SelectEmployee()
        {
            try
            {
                DateTime FromDate, ToDate;
                TrnsResignation oDoc = oCollection.ElementAt<TrnsResignation>(currentRecord);
                txtEmployeeID.Value = oDoc.MstEmployee.EmpID;
                txtEmployeeName.Value = oDoc.MstEmployee.FirstName + " " + oDoc.MstEmployee.LastName;
                txtDepartment.Value = oDoc.MstEmployee.MstDepartment.DeptName;
                txtDesignation.Value = oDoc.MstEmployee.MstDesignation.Name;
                txtJoiningDate.Value = Convert.ToDateTime(oDoc.DateOfJoining).ToString("yyyyMMdd"); 
                txtResignationDate.Value = Convert.ToDateTime(oDoc.ResignDate).ToString("yyyyMMdd");
                txtTerminationDate.Value = Convert.ToDateTime(oDoc.MstEmployee.TerminationDate).ToString("yyyyMMdd");
                
                //Get the Period
                TrnsSalaryProcessRegister One = (from a in dbHrPayroll.TrnsSalaryProcessRegister
                                                 where a.MstEmployee.EmpID.Contains(oDoc.MstEmployee.EmpID)
                                                 orderby a.Id descending
                                                 select a).FirstOrDefault();
                if (One != null)
                {
                    CfgPeriodDates PeriodDates = (from a in dbHrPayroll.CfgPeriodDates
                                                  where a.PeriodName.Contains(One.PeriodName) && a.PayrollId == One.PayrollID
                                                  select a).FirstOrDefault();
                    FromDate = Convert.ToDateTime(PeriodDates.EndDate);
                    FromDate.AddDays(1);
                }
                else
                {
                    FromDate = Convert.ToDateTime(oDoc.DateOfJoining);
                }
                ToDate = Convert.ToDateTime(oDoc.ResignDate);

                TimeSpan Duration = FromDate - ToDate;

                CfgPeriodDates ResigPeriodDate = (from a in dbHrPayroll.CfgPeriodDates
                                                  where a.PayrollId == oDoc.MstEmployee.PayrollID
                                                  && a.StartDate <= oDoc.ResignDate 
                                                  && a.EndDate >= oDoc.ResignDate
                                                  select a).FirstOrDefault();

                EmployeeCode = (from a in dbHrPayroll.MstEmployee where a.ID == oDoc.MstEmployee.ID select a).FirstOrDefault();
                ResignDate = Convert.ToDateTime(oDoc.CreateDate);
                TerminationDate = Convert.ToDateTime(oDoc.ResignDate);
                JoiningDate = Convert.ToDateTime(oDoc.DateOfJoining);
                NoOfDays = Math.Abs(Convert.ToInt32(Duration.Days));
                PayrollID = Convert.ToInt32(oDoc.MstEmployee.PayrollID);
                ResignPeriodID = Convert.ToInt32(ResigPeriodDate.ID);
                ResignPeriodName = ResigPeriodDate.PeriodName;
                Int32 cnt = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                             where a.MstEmployee.ID == EmployeeCode.ID
                             select a.Id).Count();
                if (cnt > 0)
                {
                    TrnsFinalSettelmentRegister Two = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                                                             where a.MstEmployee.ID == EmployeeCode.ID
                                                             select a).FirstOrDefault();
                    FillEOSDetails(Convert.ToString(Two.Id));
                    btnProcess.Caption = "Update";
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SelectEmployee Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        //private void PrcossesEOS(String pEmpID,Int32 pDaysCount, Int32 pPayrollId, Int32 pPeriodID)
        //{
        //    string strProcessing = "";
        //    int totalCnt = 0;
        //    SAPbouiCOM.ProgressBar prog = oApplication.StatusBar.CreateProgressBar("Processing Salary", totalCnt, false);
        //    try
        //    {
                
        //        Hashtable elementGls = new Hashtable();
                

        //        //SAPbouiCOM.ProgressBar prog = oApplication.StatusBar.CreateProgressBar("Processing Salary", totalCnt, false);
        //        prog.Value = 0;
        //        CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID == pPayrollId select p).Single();
        //        CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID == pPeriodID select p).Single();
        //        int periodDays = 0;
        //        periodDays = Convert.ToInt16(payroll.WorkDays);
        //        decimal empBasicSalary = 0;
        //        decimal empGrossSalary = 0;

        //        try
        //        {
                    

        //            decimal amnt = 0.0M;



        //            string sel = "Y";
        //            if (sel == "Y")
        //            {
        //                prog.Value += 1;
        //                //if (i == 89)
        //                //{
        //                //    string strBreak = "break";
        //                //}
        //                string empid = pEmpID;
        //                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID.Contains(empid) select p).Single();
        //                MstGLDetermination glDetr = ds.getEmpGl(emp);
        //                if (glDetr == null)
        //                {
        //                    //continue;
        //                    //break;
        //                }
        //                empBasicSalary = (decimal)emp.BasicSalary;
        //                if (emp.EmployeeContractType == "DWGS")
        //                {
        //                    empGrossSalary = ds.getEmpGross(emp, payrollperiod.ID);
        //                }
        //                else
        //                {
        //                    empGrossSalary = ds.getEmpGross(emp);
        //                }
        //                prog.Text = "(" + prog.Value.ToString() + " of " + totalCnt.ToString() + " ) Processing Salary --> " + emp.FirstName + " " + emp.LastName;
        //                strProcessing = "Error in Processing Salary --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + " (" + "" + " of " + totalCnt.ToString() + " ) ";
        //                Application.DoEvents();
        //                int DaysCnt = 0;

        //                decimal spTaxbleAmnt = 0.00M;

        //                //DaysCnt = ds.getDaysCnt(emp, payrollperiod);
        //                DaysCnt = pDaysCount;
        //                decimal employeeRemainingSalary = 0.00M;
        //                try
        //                {
        //                    employeeRemainingSalary = Math.Round((decimal)emp.BasicSalary, 0);
        //                }
        //                catch { }
        //                TrnsFinalSettelmentRegister reg = new TrnsFinalSettelmentRegister();
        //                reg.MstEmployee = emp;
        //                reg.CfgPayrollDefination = payroll;
        //                reg.CfgPeriodDates = payrollperiod;
        //                reg.EmpBasic = Math.Round(Convert.ToDecimal(emp.BasicSalary), 0);
        //                reg.EmpGross = empGrossSalary;
        //                reg.CreateDate = DateTime.Now;
        //                reg.UpdateDate = DateTime.Now;
        //                reg.UserId = oCompany.UserName;
        //                reg.UpdateBy = oCompany.UserName;
        //                reg.PeriodName = payrollperiod.PeriodName;
        //                reg.PayrollName = payroll.PayrollName;
        //                reg.EmpName = emp.FirstName + " " + emp.LastName;


        //                /// Basic Salary ////
        //                /// ************////
        //                TrnsFinalSettelmentRegisterDetail spdHeadRow = new TrnsFinalSettelmentRegisterDetail();
        //                spdHeadRow.LineType = "BS";
        //                spdHeadRow.LineSubType = "Basic Salary";
        //                spdHeadRow.LineValue = Math.Round(employeeRemainingSalary, 0);
        //                spdHeadRow.LineMemo = "Basic Salary ";
        //                spdHeadRow.DebitAccount = glDetr.BasicSalary;
        //                spdHeadRow.CreditAccount = glDetr.BSPayable;
        //                spdHeadRow.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
        //                spdHeadRow.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
        //                spdHeadRow.LineBaseEntry = emp.ID;
        //                spdHeadRow.BaseValueCalculatedOn = employeeRemainingSalary;
        //                spdHeadRow.BaseValue = employeeRemainingSalary;
        //                spdHeadRow.BaseValueType = "FIX";
        //                spdHeadRow.CreateDate = DateTime.Now;
        //                spdHeadRow.UpdateDate = DateTime.Now;
        //                spdHeadRow.UserId = oCompany.UserName;
        //                spdHeadRow.UpdatedBy = oCompany.UserName;
        //                spdHeadRow.NoOfDay = Convert.ToInt16(DaysCnt);
        //                spdHeadRow.TaxableAmount = employeeRemainingSalary;
        //                spTaxbleAmnt += employeeRemainingSalary;
        //                employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
        //                reg.TrnsFinalSettelmentRegisterDetail.Add(spdHeadRow);


        //                //* Payroll elements assigned to employee ***Employee Elements ****** 
        //                //*******************************************************************
        //                //TODO: Need to check if it works with FinalSettlement 
        //                DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, payrollperiod, DaysCnt, empGrossSalary, glDetr);
        //                foreach (DataRow dr in dtSalPrlElements.Rows)
        //                {
        //                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
        //                    {
        //                        TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
        //                        spdetail.LineType = dr["LineType"].ToString();
        //                        spdetail.LineSubType = dr["LineSubType"].ToString();
        //                        spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                        spdetail.LineMemo = dr["LineMemo"].ToString();
        //                        spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                        spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                        spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                        spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                        spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                        spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                        spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                        spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                        spdetail.CreateDate = DateTime.Now;
        //                        spdetail.UpdateDate = DateTime.Now;
        //                        spdetail.UserId = oCompany.UserName;
        //                        spdetail.UpdatedBy = oCompany.UserName;
        //                        spdetail.NoOfDay = Convert.ToInt16(DaysCnt);
        //                        spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                        spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                        employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                        reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
        //                    }
        //                }
        //                //******************** End of Elements *********************************



        //                //////Over time ////
        //                //**************////
        //                //TODO: Need to chech if it works with final settlement
        //                DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, payrollperiod, empGrossSalary);
        //                foreach (DataRow dr in dtSalOverTimes.Rows)
        //                {
        //                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                    spdetail.NoOfDay = Convert.ToInt16(DaysCnt);
        //                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
        //                }

        //                //* AbsentDeductions,Reimbursement

        //                //////Absents ////
        //                //**************////
        //                //TODO: Need to chech if it works with final settlement
        //                DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross);
        //                foreach (DataRow dr in dtAbsentDeduction.Rows)
        //                {
        //                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                    spdetail.NoOfDay = Convert.ToInt16(DaysCnt);
        //                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
        //                }

        //                //* End of Leave Deductions


        //                // * ************Advance Recovery Processing **************
        //                //*******************************************************
        //                //TODO: NEED TO CHECK FOR EOS
        //                DataTable dtAdvance = ds.salaryProcessingAdvance(emp, employeeRemainingSalary);

        //                foreach (DataRow dr in dtAdvance.Rows)
        //                {
        //                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();

        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.TaxableAmount = 0.00M;
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;


        //                    spdetail.NoOfDay = Convert.ToInt16(DaysCnt);
        //                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
        //                }



        //                // * ************Loan Recovery Processing **************
        //                //TODO: NEED TO CHECK FOR EOS
        //                DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary);

        //                foreach (DataRow dr in dtLoands.Rows)
        //                {
        //                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.NoOfDay = Convert.ToInt16(DaysCnt);
        //                    spdetail.TaxableAmount = 0.00M;
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;

        //                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
        //                }
        //                reg.EmpTaxblTotal = spTaxbleAmnt;

        //                //********************Taxable Amount***********************************
        //                //TODO: NEED TO CHECK FOR EOS
        //                if (Program.systemInfo.TaxSetup == true)
        //                {
        //                    decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxbleAmnt, empGrossSalary);
        //                    reg.EmpTotalTax = TotalTax;

        //                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
        //                    spdetail.LineType = "Tax";
        //                    spdetail.LineSubType = "Tax";
        //                    spdetail.LineValue = -Math.Round(TotalTax, 0);
        //                    spdetail.LineMemo = "Tax Deduction";
        //                    spdetail.DebitAccount = glDetr.IncomeTaxExpense;
        //                    spdetail.CreditAccount = glDetr.IncomeTaxPayable;
        //                    spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
        //                    spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
        //                    spdetail.LineBaseEntry = 0;
        //                    spdetail.BaseValueCalculatedOn = spTaxbleAmnt;
        //                    spdetail.BaseValue = spTaxbleAmnt;
        //                    spdetail.BaseValueType = "FIX";
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.NoOfDay = Convert.ToInt16(DaysCnt);
        //                    spdetail.TaxableAmount = 0.00M;
        //                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);


        //                }
        //                reg.FSStatus = 0;//Salary Processed
        //                dbHrPayroll.TrnsFinalSettelmentRegister.InsertOnSubmit(reg);


        //            }

                    
        //            dbHrPayroll.SubmitChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            oApplication.SetStatusBarMessage(strProcessing + ":" + ex.Message);
        //        }
        //        prog.Stop();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);

        //        prog = null;
                
        //    }
        //    catch (Exception ex)
        //    {
        //        oApplication.SetStatusBarMessage(ex.Message);
        //        prog.Stop();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);

        //        prog = null;
        //    }

        //}
        
        private void ProcessEOS(MstEmployee pEmpID, Int32 pDaysCount, DateTime JoiningDate, DateTime ResignationDate)
        {
            String EmpName = "";
            Int16 LenghtName = 0;
            try
            {
                TrnsFinalSettelmentRegister oDoc = new TrnsFinalSettelmentRegister();

                oDoc.DaysPaid = Convert.ToInt16(pDaysCount);
                oDoc.EmpID = Convert.ToInt32(pEmpID.ID);
                EmpName = pEmpID.FirstName + " " + pEmpID.MiddleName + " " + pEmpID.LastName;
                if (EmpName.Length >= 50)
                {
                    LenghtName = 49;
                }
                else
                {
                    LenghtName = Convert.ToInt16(EmpName.Length);
                }
                oDoc.EmpName = EmpName.Substring(0, LenghtName-1);
                oDoc.EmpBasic = Convert.ToDecimal(pEmpID.BasicSalary);
                oDoc.EmpGross = Convert.ToDecimal(ds.getEmpGross(pEmpID));
                oDoc.PayrollID = PayrollID;
                oDoc.PayrollName = pEmpID.CfgPayrollDefination.PayrollName;
                oDoc.PayrollPeriodID = ResignPeriodID;
                oDoc.PeriodName = ResignPeriodName;
                oDoc.CreateDate = DateTime.Now;
                oDoc.UpdateDate = DateTime.Now;
                oDoc.UserId = oCompany.UserName;
                oDoc.UpdateBy = oCompany.UserName;
                //Get Value of Arrears

                //* Final Settlement Calculate Arrears for Selected Employee ******** 
                //*******************************************************************
                // 
                DataTable dtArrears = ds.EOSArrear(pEmpID.ID.ToString());
                foreach (DataRow dr in dtArrears.Rows)
                {
                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                    {
                        TrnsFinalSettelmentRegisterDetail ArrearDetail = new TrnsFinalSettelmentRegisterDetail();
                        ArrearDetail.LineType = dr["LineType"].ToString();
                        ArrearDetail.LineSubType = dr["LineSubType"].ToString();
                        ArrearDetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        ArrearDetail.LineMemo = dr["LineMemo"].ToString();
                        ArrearDetail.DebitAccount = dr["DebitAccount"].ToString();
                        ArrearDetail.CreditAccount = dr["CreditAccount"].ToString();
                        ArrearDetail.DebitAccountName = dr["DebitAccountName"].ToString();
                        ArrearDetail.CreditAccountName = dr["CreditAccountName"].ToString();
                        ArrearDetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                        ArrearDetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                        ArrearDetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                        ArrearDetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                        ArrearDetail.CreateDate = DateTime.Now;
                        ArrearDetail.UpdateDate = DateTime.Now;
                        ArrearDetail.UserId = oCompany.UserName;
                        ArrearDetail.UpdatedBy = oCompany.UserName;
                        ArrearDetail.NoOfDay = Convert.ToInt16(NoOfDays);
                        ArrearDetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);

                        //employeeRemainingSalary += (decimal)ConDetail.LineValue;
                        oDoc.TrnsFinalSettelmentRegisterDetail.Add(ArrearDetail);
                    }
                }
                //******************** End of Employee Arrears *********************************

                //* Final Settlement Calculate Contribution for Selected Employee+Employer *** 
                //*******************************************************************
                // 
                DataTable dtEOSContribution = ds.EOSContribution(pEmpID.ID.ToString());
                foreach (DataRow dr in dtEOSContribution.Rows)
                {
                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                    {
                        TrnsFinalSettelmentRegisterDetail ConDetail = new TrnsFinalSettelmentRegisterDetail();
                        ConDetail.LineType = dr["LineType"].ToString();
                        ConDetail.LineSubType = dr["LineSubType"].ToString();
                        ConDetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        ConDetail.LineMemo = dr["LineMemo"].ToString();
                        ConDetail.DebitAccount = dr["DebitAccount"].ToString();
                        ConDetail.CreditAccount = dr["CreditAccount"].ToString();
                        ConDetail.DebitAccountName = dr["DebitAccountName"].ToString();
                        ConDetail.CreditAccountName = dr["CreditAccountName"].ToString();
                        ConDetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                        ConDetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                        ConDetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                        ConDetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                        ConDetail.CreateDate = DateTime.Now;
                        ConDetail.UpdateDate = DateTime.Now;
                        ConDetail.UserId = oCompany.UserName;
                        ConDetail.UpdatedBy = oCompany.UserName;
                        ConDetail.NoOfDay = Convert.ToInt16(NoOfDays);
                        ConDetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                        
                        //employeeRemainingSalary += (decimal)ConDetail.LineValue;
                        oDoc.TrnsFinalSettelmentRegisterDetail.Add(ConDetail);
                    }
                }
                //******************** End of Employee+Employer Contribution *********************************

                //Calculate Gratuity

                //DataTable dtGratuity = //ds.GratuityEOS(pEmpID);
                //foreach (DataRow dr in dtGratuity.Rows)
                //{
                //    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                //    {
                //        TrnsFinalSettelmentRegisterDetail grDetail = new TrnsFinalSettelmentRegisterDetail();
                //        grDetail.LineType = dr["LineType"].ToString();
                //        grDetail.LineSubType = dr["LineSubType"].ToString();
                //        grDetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                //        grDetail.LineMemo = dr["LineMemo"].ToString();
                //        grDetail.DebitAccount = dr["DebitAccount"].ToString();
                //        grDetail.CreditAccount = dr["CreditAccount"].ToString();
                //        grDetail.DebitAccountName = dr["DebitAccountName"].ToString();
                //        grDetail.CreditAccountName = dr["CreditAccountName"].ToString();
                //        grDetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                //        grDetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                //        grDetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                //        grDetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                //        grDetail.CreateDate = DateTime.Now;
                //        grDetail.UpdateDate = DateTime.Now;
                //        grDetail.UserId = oCompany.UserName;
                //        grDetail.UpdatedBy = oCompany.UserName;
                //        grDetail.NoOfDay = Convert.ToInt16(NoOfDays);
                //        grDetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);

                //        //employeeRemainingSalary += (decimal)ConDetail.LineValue;
                //        oDoc.TrnsFinalSettelmentRegisterDetail.Add(grDetail);
                //    }
                //}

                // * ************Advance Recovery Processing **************
                //*******************************************************
                DataTable dtAdvance = ds.EOSAdvanceRecovery(pEmpID);

                foreach (DataRow dr in dtAdvance.Rows)
                {
                    TrnsFinalSettelmentRegisterDetail AdvanceLine = new TrnsFinalSettelmentRegisterDetail();
                    AdvanceLine.LineType = dr["LineType"].ToString();
                    AdvanceLine.LineSubType = dr["LineSubType"].ToString();
                    AdvanceLine.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                    AdvanceLine.LineMemo = dr["LineMemo"].ToString();
                    AdvanceLine.DebitAccount = dr["DebitAccount"].ToString();
                    AdvanceLine.CreditAccount = dr["CreditAccount"].ToString();
                    AdvanceLine.DebitAccountName = dr["DebitAccountName"].ToString();
                    AdvanceLine.CreditAccountName = dr["CreditAccountName"].ToString();
                    AdvanceLine.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                    AdvanceLine.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                    AdvanceLine.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                    AdvanceLine.BaseValueType = dr["BaseValueType"].ToString(); ;
                    AdvanceLine.CreateDate = DateTime.Now;
                    AdvanceLine.UpdateDate = DateTime.Now;
                    AdvanceLine.UserId = oCompany.UserName;
                    AdvanceLine.UpdatedBy = oCompany.UserName;
                    AdvanceLine.TaxableAmount = 0.00M;
                    oDoc.TrnsFinalSettelmentRegisterDetail.Add(AdvanceLine);
                }

                // * ************Loan Recovery Processing **************
                //******************************************************
                DataTable dtLoands = ds.EOSLoanRecovery(pEmpID);

                foreach (DataRow dr in dtLoands.Rows)
                {
                    //TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                    TrnsFinalSettelmentRegisterDetail LoanLine = new TrnsFinalSettelmentRegisterDetail();
                    LoanLine.LineType = dr["LineType"].ToString();
                    LoanLine.LineSubType = dr["LineSubType"].ToString();
                    LoanLine.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                    LoanLine.LineMemo = dr["LineMemo"].ToString();
                    LoanLine.DebitAccount = dr["DebitAccount"].ToString();
                    LoanLine.CreditAccount = dr["CreditAccount"].ToString();
                    LoanLine.DebitAccountName = dr["DebitAccountName"].ToString();
                    LoanLine.CreditAccountName = dr["CreditAccountName"].ToString();
                    LoanLine.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                    LoanLine.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                    LoanLine.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                    LoanLine.BaseValueType = dr["BaseValueType"].ToString(); ;
                    LoanLine.CreateDate = DateTime.Now;
                    LoanLine.UpdateDate = DateTime.Now;
                    LoanLine.UserId = oCompany.UserName;
                    LoanLine.UpdatedBy = oCompany.UserName;
                    LoanLine.NoOfDay = Convert.ToInt16(pDaysCount);
                    LoanLine.TaxableAmount = 0.00M;
                    //employeeRemainingSalary += (decimal)spdetail.LineValue;
                    //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                    oDoc.TrnsFinalSettelmentRegisterDetail.Add(LoanLine);
                }


                //End of Calculation SAve EOS

                dbHrPayroll.TrnsFinalSettelmentRegister.InsertOnSubmit(oDoc);
                dbHrPayroll.SubmitChanges();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Process EOS Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddAdjustment(String pEOSId)
        {
            try
            {

                TrnsFinalSettelmentRegister oDoc = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                                                    where a.Id.ToString() == pEOSId
                                                    select a).FirstOrDefault();
                if (oDoc == null)
                {
                    return;
                }
                else
                {
                    //if (oDoc.FSStatus == 1)
                    //{
                    //    oApplication.StatusBar.SetText("Can't Update Approved Final Settlement.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //    return;
                    //}
                }

                //* Add the Adjustment Line in Final Settelment ******** 
                //******************************************************
                //******************************************************

                TrnsFinalSettelmentRegisterDetail AdjustmentDetail = null;

                AdjustmentDetail = (from a in dbHrPayroll.TrnsFinalSettelmentRegisterDetail
                                    where a.FSID == oDoc.Id && a.LineType.Contains("Adjustment")
                                    select a).FirstOrDefault();
                if (AdjustmentDetail == null)
                {
                    AdjustmentDetail = new TrnsFinalSettelmentRegisterDetail();
                    oDoc.TrnsFinalSettelmentRegisterDetail.Add(AdjustmentDetail);
                }

                AdjustmentDetail.LineType = "Adjustment";
                AdjustmentDetail.LineSubType = "Adjustment";
                AdjustmentDetail.LineValue = Math.Round(Convert.ToDecimal(txtAdjustment.Value.Trim()), 0);
                AdjustmentDetail.LineMemo = Convert.ToString(txtRemarks.Value.Trim());
                AdjustmentDetail.DebitAccount = "";
                AdjustmentDetail.CreditAccount = "";
                AdjustmentDetail.DebitAccountName = "";
                AdjustmentDetail.CreditAccountName = "";
                AdjustmentDetail.LineBaseEntry = 0;
                AdjustmentDetail.BaseValueCalculatedOn = 0.0M;
                AdjustmentDetail.BaseValue = 0.0M;
                AdjustmentDetail.BaseValueType = "";
                AdjustmentDetail.CreateDate = DateTime.Now;
                AdjustmentDetail.UpdateDate = DateTime.Now;
                AdjustmentDetail.UserId = oCompany.UserName;
                AdjustmentDetail.UpdatedBy = oCompany.UserName;
                AdjustmentDetail.NoOfDay = Convert.ToInt16(NoOfDays);
                AdjustmentDetail.TaxableAmount = 0.0M;

                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddAdjustment Error : " + ex.Message);
            }
        }

        private void VoidEOS(String pEmpID, Int32 pPayrollID)
        {
            try
            {
                
                TrnsFinalSettelmentRegister oEntry = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                                                      where a.MstEmployee.EmpID.Contains(pEmpID)
                                                      && a.CfgPayrollDefination.ID == pPayrollID
                                                      select a).FirstOrDefault();

                //IEnumerable<TrnsEmployeeElementDetail> nonRecuringElements = from p in dbHrPayroll.TrnsEmployeeElementDetail 
                //                                                             where p.TrnsEmployeeElement.MstEmployee.EmpID.Contains(pEmpID)
                //                                                             && p.PeriodId.ToString() == cbPeriod.Value.ToString() 
                //                                                             select p;
                //foreach (TrnsEmployeeElementDetail ele in nonRecuringElements)
                //{
                //    ele.FlgOneTimeConsumed = false;
                //}
                if (oEntry != null)
                {
                    //if (oEntry.FSStatus != 0)
                    //    oApplication.StatusBar.SetText("Can't Void An Approved Final Settlement", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //    return;
                }
                dbHrPayroll.TrnsFinalSettelmentRegister.DeleteOnSubmit(oEntry);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidEOS Error :" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void Processed()
        {
            try
            {
                TrnsFinalSettelmentRegister oDoc = null;
                Int32 cnt = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                             where a.MstEmployee.ID == EmployeeCode.ID
                             select a.Id).Count();
                if (cnt == 0 && btnProcess.Caption == "Process")
                {
                    ProcessEOS(EmployeeCode, NoOfDays, JoiningDate, TerminationDate);
                    oApplication.StatusBar.SetText("Successfuly Posted EOS of EmpID : " + EmployeeCode.EmpID, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else if (cnt != 0 && btnProcess.Caption == "Update")
                {
                    oDoc = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                            where a.MstEmployee.EmpID.Contains(EmployeeCode.EmpID)
                            select a).FirstOrDefault();
                    AddAdjustment(oDoc.Id.ToString());
                    oApplication.StatusBar.SetText("Record Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                cnt = 0;
                cnt = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                       where a.MstEmployee.ID == EmployeeCode.ID
                       select a.Id).Count();
                if (cnt > 0)
                {
                    oDoc = null;
                    oDoc = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                                                       where a.MstEmployee.ID == EmployeeCode.ID
                                                       select a).FirstOrDefault();
                    FillEOSDetails(Convert.ToString(oDoc.Id));
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Processed Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void VoidEOSClick()
        {
            try
            {
                if (EmployeeCode == null)
                    return;
                
                VoidEOS(EmployeeCode.EmpID, PayrollID);
                InitializeEOSDetail();
                oApplication.StatusBar.SetText("Successfully Void Salary of EmpID : " + EmployeeCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidEOSClick Error : "+ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillEOSDetails(String pEOSID)
        {
            
            InitializeEOSDetail();
            //dtElements.Rows.Clear();
            //dtOthElements.Rows.Clear();
            decimal Arrear = 0.0M;
            decimal Contribution = 0.00M;
            decimal BasicSalary = 0.0M;
            decimal Adjustment = 0.0M;
            decimal Others = 0.0M;
            string Remarks = "";
            decimal GrossSalary = 0.0M;
            int i = 0;
            int cnt = (from p in dbHrPayroll.TrnsFinalSettelmentRegister where p.Id.ToString() == pEOSID select p).Count();
            if (cnt > 0)
            {
                TrnsFinalSettelmentRegister salms = (from p in dbHrPayroll.TrnsFinalSettelmentRegister where p.Id.ToString() == pEOSID select p).Single();
                BasicSalary = Convert.ToDecimal(salms.EmpBasic.ToString());
                GrossSalary = Convert.ToDecimal(salms.EmpGross.ToString());
                string strSql = @"
                                    SELECT 
	                                    LineType,
	                                    LineSubType,
	                                    ISNULL(LineValue,0) AS LineValue,
                                        LineMemo
                                    FROM 
	                                    dbo.TrnsFinalSettelmentRegisterDetail
                                    WHERE 
	                                    FSID = '"+ pEOSID +@"'
                                 ";
                DataTable dtEle = ds.getDataTable(strSql);
                foreach (DataRow dr in dtEle.Rows)
                {
                    dtMain.Rows.Add(1);
                    dtMain.SetValue("desc", i, dr["LineSubType"].ToString());
                    dtMain.SetValue("Amount", i, dr["LineValue"].ToString());
                    if ( dr["LineType"].ToString() == "Arrear")
                    {
                        Arrear = Convert.ToDecimal(dr["LineValue"].ToString());
                    }
                    if (dr["LineType"].ToString() == "Contribution")
                    {
                        Contribution += Convert.ToDecimal(dr["LineValue"].ToString());
                    }
                    if (dr["LineType"].ToString() == "Adjustment")
                    {
                        Adjustment = Convert.ToDecimal(dr["LineValue"].ToString());
                        Remarks = Convert.ToString(dr["LineMemo"].ToString());
                    }
                    if (dr["LineType"].ToString() != "Arrear" && dr["LineType"].ToString() != "Contribution" && dr["LineType"].ToString() != "Adjustment")
                    {
                        Others += Convert.ToDecimal(dr["LineValue"].ToString());
                    }
                    i++;
                }
                
                txtBasicSalary.Value = BasicSalary.ToString();
                txtGrossSalary.Value = GrossSalary.ToString();
                txtAdjustment.Value = Adjustment.ToString();
                txtRemarks.Value = Remarks;
                txtArrears.Value = Arrear.ToString();
                txtContributions.Value = Contribution.ToString();
                txtNetPayable.Value = Convert.ToString(Arrear + Contribution + Adjustment + Others);
                
               
            }
            mtMain.LoadFromDataSource();
            
        }

        private void InitializeEOSDetail()
        {
            txtBasicSalary.Value = "0.00";
            txtGrossSalary.Value = "0.00";
            txtAdjustment.Value = "0.00";
            txtArrears.Value = "0.00";
            txtContributions.Value = "0.00";
            txtNetPayable.Value = "0.00";
            dtMain.Rows.Clear();
            
        }

        #endregion

    }
}
