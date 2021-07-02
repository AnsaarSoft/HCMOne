using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_FSN : HRMSBaseForm
    {
        #region Local Variable Area
        private int RoundingSet = 0;
        SAPbouiCOM.EditText txtEmployeeID, txtEmployeeName, txtContributions, txtNetPayable, txtBasicSalary, txtGrossSalary;
        SAPbouiCOM.EditText txtDepartment, txtDesignation, txtJoiningDate, txtResignationDate;
        SAPbouiCOM.EditText txtAdjustment, txtTerminationDate, txtRemarks;
        SAPbouiCOM.Button btnProcess, btnPick, btnVoid;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix mtMain;
        SAPbouiCOM.Column cDesc, cAmount;
        SAPbouiCOM.Item ibtnProcess, ibtnPick, ibtnVoid;

        IEnumerable<TrnsResignation> oCollection = null;
        MstEmployee oEmployee = null;
        Int32 NoOfDays = 0, PayrollID = 0, ResignPeriodID = 0;
        DateTime JoiningDate, ResignDate, TerminationDate;
        String ResignPeriodName = "";
        List<Int32> lstPeriodID = new List<Int32>();
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
                //case "btProcess":
                case "1":                    
                    Processed();
                    break;
                case "btpick":
                    doFind();
                    break;
                case "btvoid":
                    VoidEOSClick();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    btnProcess.Caption = "Process";
                    break;
                //case "btok":
                //    oForm.Close();
                //    break;
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

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            
        }

        public override void fillFields()
        {
            base.fillFields();
            //SelectEmployee(txtEmployeeID.Value);
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();           
            OpenNewSearchForm();

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

                if (ACHR.Properties.Settings.Default.RoundingValue == "Yes")
                {
                    RoundingSet = 1;
                }
                else
                {
                    RoundingSet = 0;
                }

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
                cDesc = mtMain.Columns.Item("desc");
                cAmount = mtMain.Columns.Item("Amount");

                //btnProcess = oForm.Items.Item("btProcess").Specific;
                //ibtnProcess = oForm.Items.Item("btProcess");

                btnProcess = oForm.Items.Item("1").Specific;
                ibtnProcess = oForm.Items.Item("1");

                btnVoid = oForm.Items.Item("btvoid").Specific;
                ibtnVoid = oForm.Items.Item("btvoid");
                btnPick = oForm.Items.Item("btpick").Specific;
                ibtnPick = oForm.Items.Item("btpick");

                //mtOthElements = oForm.Items.Item("mtOEle").Specific;
                //dtOthElements = oForm.DataSources.DataTables.Item("dtOtherEle");


                GetData();
                GetDataFilterData();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void doFind()
        {
            OpenNewSearchForm();
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

        private void SelectEmployee(String EmpID)
        {
            try
            {
                Boolean flgAbc = false;
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpID select a).FirstOrDefault();
                oEmployee = oEmp;
                lstPeriodID.Clear();
                //TrnsResignation oDoc = oCollection.ElementAt<TrnsResignation>(currentRecord);
                txtEmployeeID.Value = oEmp.EmpID;
                txtEmployeeName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                txtDepartment.Value = oEmp.MstDepartment.DeptName;
                if (!string.IsNullOrEmpty(oEmp.DesignationID.ToString()))
                {
                    txtDesignation.Value = oEmp.MstDesignation.Description;
                }
                txtJoiningDate.Value = Convert.ToDateTime(oEmp.JoiningDate).ToString("yyyyMMdd");
                txtResignationDate.Value = Convert.ToDateTime(oEmp.ResignDate).ToString("yyyyMMdd");
                txtTerminationDate.Value = Convert.ToDateTime(oEmp.TerminationDate).ToString("yyyyMMdd");
                txtBasicSalary.Value = Convert.ToString(oEmp.BasicSalary);
                txtGrossSalary.Value = ds.getEmpGross(oEmp).ToString();
                ResignDate = Convert.ToDateTime(oEmp.ResignDate);
                TerminationDate = Convert.ToDateTime(oEmp.TerminationDate);
                JoiningDate = Convert.ToDateTime(oEmp.JoiningDate);
                PayrollID = Convert.ToInt32(oEmp.PayrollID);

                Int32 PeriodCheckCount = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true && a.EndDate >= ResignDate && a.EndDate >= TerminationDate select a).Count();
                if (PeriodCheckCount == 0)
                {
                    oApplication.StatusBar.SetText("Resign and Termination should be under fiscal Year.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    ibtnProcess.Enabled = false;
                    return;
                }
                //Get the Periods
                for (DateTime x = ResignDate; x <= TerminationDate; x = x.AddDays(1))
                {
                    CfgPeriodDates oValue = (from a in dbHrPayroll.CfgPeriodDates
                                             where a.StartDate <= x && a.EndDate >= x && a.PayrollId == PayrollID
                                             select a).FirstOrDefault();
                    if (oValue != null)
                    {
                        if (lstPeriodID.Count > 0)
                        {
                            foreach (Int32 Value in lstPeriodID)
                            {
                                if (oValue.ID == Value)
                                {
                                    flgAbc = true;
                                }
                            }
                        }
                    }
                    if (flgAbc == false)
                    {
                        lstPeriodID.Add(oValue.ID);
                    }
                    flgAbc = false;
                }

                //Check Already Processed or Not

                TrnsFSHead oDocCount = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID && a.EosType == (oEmployee.TermCount == null ? 1 : Convert.ToInt32(oEmployee.TermCount)) select a).FirstOrDefault();
                if (oDocCount != null)
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    ibtnVoid.Enabled = true;
                    Int32 DoOcId = oDocCount.ID;
                    FillEOSDetails(DoOcId, 0);
                    btnProcess.Caption = "Update";
                    
                }
                else
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    ibtnVoid.Enabled = false;
                    ibtnProcess.Enabled = true;
                    btnProcess.Caption = "Process";
                }


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SelectEmployee Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

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
                oDoc.EmpName = EmpName.Substring(0, LenghtName - 1);
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

                ////Calculate Gratuity

                //DataTable dtGratuity = ds.GratuityEOS(pEmpID);
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

        private void ProcessEOS(Int32 pPeriodID)
        {
            try
            {
                String strFinalSattelment = Convert.ToString(Program.flgFinalSettelment);
                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID select a).FirstOrDefault();

                Hashtable elementGls = new Hashtable();
                CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID == PayrollID select p).FirstOrDefault();
                CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID == pPeriodID select p).Single();
                int periodDays = 0;
                periodDays = Convert.ToInt16(payroll.WorkDays);
                decimal empBasicSalary = 0;
                decimal empGrossSalary = 0;

                decimal amnt = 0.0M;

                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == oEmployee.EmpID select p).FirstOrDefault();
                MstGLDetermination glDetr = ds.getEmpGl(emp);

                if (glDetr == null)
                {
                    oApplication.StatusBar.SetText("GL Account not found for EMP Id : " + oEmployee.EmpID, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }

                empBasicSalary = (decimal)emp.BasicSalary;
                decimal spTaxbleAmnt = 0.00M;
                decimal spTaxableAmntOT = 0.00M;
                decimal spTaxableAmntLWOP = 0.00M;
                decimal DaysCnt = 0, VarDayCnt = 0;
                decimal payDays = 0.00M;
                decimal leaveDays = 0.00M;
                decimal monthDays = 0.00M;
                decimal nonRecurringTaxable = 0.00M;
                decimal payRatio = 1.00M;
                decimal payRatioWithLeaves = 1.00M;
                //**********************************
                Int32 MonthHour = 0;
                Int32 TotalMinutes = 0;
                Int32 PresentMinutes = 0;
                Int32 OTMinutes = 0;
                decimal LeaveMinutesTotal = 0;
                decimal AllowanceTriggerValue = 18 * 60;
                //**********************************

                DaysCnt = ds.getDaysCnt(emp, payrollperiod, out payDays, out leaveDays, out monthDays, out VarDayCnt);


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


                TrnsFinalSettelmentRegister reg = new TrnsFinalSettelmentRegister();

                reg.MstEmployee = emp;
                reg.CfgPayrollDefination = payroll;
                reg.CfgPeriodDates = payrollperiod;
                reg.EmpBasic = employeeRemainingSalary;//Math.Round(Convert.ToDecimal(empBasicSalary * payRatio), 0);
                reg.EmpGross = empGrossSalary;
                if (emp.MstDepartment != null)
                {
                    reg.EmpDepartment = emp.MstDepartment.DeptName;
                }
                else
                {
                    reg.EmpDepartment = "";
                }
                if (emp.MstDesignation != null)
                {
                    reg.EmpDesignation = emp.MstDesignation.Description;
                }
                else
                {
                    reg.EmpDesignation = "";
                }
                if (emp.MstLocation != null)
                {
                    reg.EmpLocation = emp.MstLocation.Description;
                }
                else
                {
                    reg.EmpLocation = "";
                }
                if (emp.MstBranches != null)
                {
                    reg.EmpBranch = emp.MstBranches.Description;
                }
                else
                {
                    reg.EmpBranch = "";
                }
                if (emp.MstPosition != null)
                {
                    reg.EmpPosition = emp.MstPosition.Description;
                }
                else
                {
                    reg.EmpPosition = "";
                }
                if (string.IsNullOrEmpty(emp.JobTitle))
                {
                    reg.EmpJobTitle = "";
                }
                else
                {
                    MstJobTitle oTitle = (from a in dbHrPayroll.MstJobTitle where a.Id.ToString() == emp.JobTitle select a).FirstOrDefault();
                    if (oTitle != null)
                    {
                        reg.EmpJobTitle = oTitle.Description;
                    }
                }
                reg.CreateDate = DateTime.Now;
                reg.UpdateDate = DateTime.Now;
                reg.UserId = oCompany.UserName;
                reg.UpdateBy = oCompany.UserName;
                reg.PeriodName = payrollperiod.PeriodName;
                reg.PayrollName = payroll.PayrollName;
                reg.EmpName = emp.FirstName + " " + emp.MiddleName + " " + emp.LastName;
                reg.DaysPaid = Convert.ToDecimal(DaysCnt);
                reg.MonthDays = Convert.ToInt32(monthDays);

                /// Basic Salary ////
                /// ************////
                TrnsFinalSettelmentRegisterDetail spdHeadRow = new TrnsFinalSettelmentRegisterDetail();
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
                reg.TrnsFinalSettelmentRegisterDetail.Add(spdHeadRow);


                //////Absents ////
                //**************////
                decimal leaveCnt = 0.00M;
                DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt);
                foreach (DataRow dr in dtAbsentDeduction.Rows)
                {
                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]);
                    spdetail.BaseValueType = dr["BaseValueType"].ToString();
                    spdetail.CreateDate = DateTime.Now;
                    spdetail.UpdateDate = DateTime.Now;
                    spdetail.UserId = oCompany.UserName;
                    spdetail.UpdatedBy = oCompany.UserName;
                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                    employeeRemainingSalary += (decimal)spdetail.LineValue;
                    spTaxableAmntLWOP += Convert.ToDecimal(dr["TaxbleAmnt"]);
                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                }
                #region Element Processing
                decimal Percent = 1;
                Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                string SQL = "SELECT TOP 1 \"percentage\" FROM \"FinalEval3\" WHERE \"emp_id\" = " + emp.EmpID + " AND \"year\" = " + Convert.ToDateTime(payrollperiod.StartDate).Year + "AND \"monthid\" = " + Convert.ToDateTime(payrollperiod.StartDate).Month;
                oRecSet.DoQuery(SQL);
                if (oRecSet.RecordCount > 0)
                {
                    Percent = Convert.ToDecimal(oRecSet.Fields.Item(0).Value) / 100;
                }
                //DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, payrollperiod, VarDayCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays);                
                DataTable dtSalPrlElements = ds.ElementsProcessionEarnings(emp, payrollperiod, payDays, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet, Percent);
                //DataTable dtEarnings = ds.ElementsProcessionEarnings(emp, oPayrollPeriod, VariableDayCount, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                foreach (DataRow dr in dtSalPrlElements.Rows)
                {
                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                    {
                        TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                        reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                    }
                }
                DataTable dtDeductions = ds.ElementsProcessingDeductions(emp, payrollperiod, VarDayCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                foreach (DataRow dr in dtDeductions.Rows)
                {
                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                    {
                        TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                        reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                    }
                }
                DataTable dtContributions = ds.ElementsProcessingContributions(emp, payrollperiod, VarDayCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                foreach (DataRow dr in dtContributions.Rows)
                {
                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                    {
                        TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                        reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                    }
                }
                #endregion
                //******************** End of Elements *********************************

                //////Over time ////
                //**************////

                DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, payrollperiod, empGrossSalary, out OTMinutes);

                //Code modified by Zeeshan

                foreach (DataRow dr in dtSalOverTimes.Rows)
                {
                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                }

                // * ************Loan Recovery Processing **************
                #region Problem Loan In Final Sattelment
                DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary, payrollperiod);

                foreach (DataRow dr in dtLoands.Rows)
                {
                    TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                    reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                }
                #endregion
                reg.EmpTaxblTotal = spTaxbleAmnt;

                // * ************TAX**************
                //if (Program.systemInfo.TaxSetup == true && emp.FlgTax == true)
                //{
                //    decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable,empGrossSalary, 1);
                //    if (TotalTax >= 0)
                //    {
                //        reg.EmpTotalTax = TotalTax;

                //        TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
                //        spdetail.LineType = "Tax";
                //        spdetail.LineSubType = "Tax";
                //        spdetail.LineValue = -Math.Round(TotalTax, 0);
                //        spdetail.LineMemo = "Tax Deduction";
                //        spdetail.DebitAccount = glDetr.IncomeTaxExpense;
                //        spdetail.CreditAccount = glDetr.IncomeTaxPayable;
                //        spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
                //        spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
                //        spdetail.LineBaseEntry = 0;
                //        spdetail.BaseValueCalculatedOn = spTaxbleAmnt;
                //        spdetail.BaseValue = spTaxbleAmnt;
                //        spdetail.BaseValueType = "FIX";
                //        spdetail.CreateDate = DateTime.Now;
                //        spdetail.UpdateDate = DateTime.Now;
                //        spdetail.UserId = oCompany.UserName;
                //        spdetail.UpdatedBy = oCompany.UserName;
                //        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                //        spdetail.TaxableAmount = 0.00M;

                //        reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                //    }
                //}

                // * ************Gratuity**************
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
                                TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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
                                reg.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                oHead.TrnsFinalSettelmentRegister.Add(reg);
                //dbHrPayroll.TrnsFSHead.InsertOnSubmit(oHead);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Process EOS Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void EndOfServiceDues(MstEmployee pEmpObject)
        {
            try
            {
                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead
                                    where a.MstEmployee.EmpID == pEmpObject.EmpID
                                    && a.EosType == (pEmpObject.TermCount == null ? 1 : Convert.ToInt32(pEmpObject.TermCount))
                                    select a).FirstOrDefault();

                TrnsFinalSettelmentRegister oFSRegister = (from a in dbHrPayroll.TrnsFinalSettelmentRegister where a.FSHeadID == oHead.ID orderby a.Id descending select a).FirstOrDefault();
                MstGLDetermination glDetr = ds.getEmpGl(oEmployee);

                //* Final Settlement Calculate Contribution for Selected Employee+Employer *** 
                //*******************************************************************
                // 
                DataTable dtEOSContribution = ds.EOSContribution(oEmployee.ID.ToString());
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
                        oFSRegister.TrnsFinalSettelmentRegisterDetail.Add(ConDetail);
                    }
                }

                //******** OB Value PF for Employee  *** 
                //*******************************************************************
                // 
                DataTable dtEmployeeOBPF = ds.EOSOBPFEmpValues(oEmployee, glDetr);
                foreach (DataRow dr in dtEmployeeOBPF.Rows)
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
                        oFSRegister.TrnsFinalSettelmentRegisterDetail.Add(ConDetail);
                    }
                }

                //******** OB Value PF for Employer  *** 
                //*******************************************************************
                // 
                DataTable dtEmployerOBPF = ds.EOSOBPFEmplrValues(oEmployee, glDetr);
                foreach (DataRow dr in dtEmployerOBPF.Rows)
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
                        oFSRegister.TrnsFinalSettelmentRegisterDetail.Add(ConDetail);
                    }
                }


                //Calculate Gratuity
                DataTable dtGratuity;
                if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                {
                    dtGratuity = ds.GratuityEOSUAESlabsWise(oEmployee, glDetr.GratuityExpense, glDetr.GratuityPayable, Program.objHrmsUI.getAcctName(glDetr.GratuityExpense), Program.objHrmsUI.getAcctName(glDetr.GratuityPayable));
                }
                else
                {
                    dtGratuity = ds.GratuityEOS(oEmployee, glDetr.GratuityExpense, glDetr.GratuityPayable, Program.objHrmsUI.getAcctName(glDetr.GratuityExpense), Program.objHrmsUI.getAcctName(glDetr.GratuityPayable));
                }
                foreach (DataRow dr in dtGratuity.Rows)
                {
                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                    {
                        TrnsFinalSettelmentRegisterDetail grDetail = new TrnsFinalSettelmentRegisterDetail();
                        grDetail.LineType = dr["LineType"].ToString();
                        grDetail.LineSubType = dr["LineSubType"].ToString();
                        grDetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        grDetail.LineMemo = dr["LineMemo"].ToString();
                        grDetail.DebitAccount = dr["DebitAccount"].ToString();
                        grDetail.CreditAccount = dr["CreditAccount"].ToString();
                        grDetail.DebitAccountName = dr["DebitAccountName"].ToString();
                        grDetail.CreditAccountName = dr["CreditAccountName"].ToString();
                        grDetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                        grDetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                        grDetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                        grDetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                        grDetail.CreateDate = DateTime.Now;
                        grDetail.UpdateDate = DateTime.Now;
                        grDetail.UserId = oCompany.UserName;
                        grDetail.UpdatedBy = oCompany.UserName;
                        grDetail.NoOfDay = Convert.ToInt16(NoOfDays);
                        grDetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);

                        //employeeRemainingSalary += (decimal)ConDetail.LineValue;
                        oFSRegister.TrnsFinalSettelmentRegisterDetail.Add(grDetail);
                    }
                }

                //*************Advance Recovery Processing **************
                //*******************************************************
                DataTable dtAdvance = ds.EOSAdvanceRecovery(oEmployee);

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
                    oFSRegister.TrnsFinalSettelmentRegisterDetail.Add(AdvanceLine);
                }

                // * ************Loan Recovery Processing **************
                //******************************************************
                DataTable dtLoands = ds.EOSLoanRecovery(oEmployee);

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
                    LoanLine.NoOfDay = 0.0M;
                    LoanLine.TaxableAmount = 0.00M;
                    //employeeRemainingSalary += (decimal)spdetail.LineValue;
                    //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                    oFSRegister.TrnsFinalSettelmentRegisterDetail.Add(LoanLine);
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Function : EndoFServiceDUes Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void EndOfServiceTaxation(MstEmployee pEmp, Int32 pIntPeriod, TrnsFSHead oHead, Decimal DaysCnt)
        {
            try
            {
                int remainingPeriodsCnt = 0;
                decimal outValue = 0.00M;
                decimal alreadyProcessedIncome = 0.00M;
                decimal alreadyProcessedTax = 0.00M;
                decimal projectedIncome = 0.00M;
                decimal currentYearExpectedIncome = 0.00M;
                decimal currentYearExpectedIncomeI = 0.00M;
                decimal currentYearExpectedTax = 0.00M;
                decimal currentYearExpectedTaxI = 0.00M;
                decimal perYearTax = 0.00M;
                Decimal TaxDiscountYearly = 0.0M;
                Decimal TaxDiscountMonthly = 0.0M;
                Decimal TaxQuarterlyComplete = 0.0M;
                decimal spTaxbleAmnt = 0.0M;
                decimal QuarterlyValue = 0.0M;
                Int32 TaxDetailID = 0;
                Decimal TaxValueIncentive = 0.0M;

                CfgPeriodDates pPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.ID == pIntPeriod select a).FirstOrDefault();


                //New Section For Tax Adjustment

                String strQuery = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '0' AND  
	                                A3.EmpID = '" + pEmp.EmpID + "'";


                TaxDiscountYearly = Convert.ToDecimal(getScallerValue(strQuery));

                String strQuery2 = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '1' AND  
	                                A3.EmpID = '" + pEmp.EmpID + "'";

                TaxDiscountMonthly = Convert.ToDecimal(getScallerValue(strQuery2));

                String strQuery3 = @"
                                SELECT 
	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
                                FROM 
	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
                                WHERE
	                                A1.empID = '" + pEmp.ID + @"'
	                                AND A2.PayrollPeriodID = '" + pIntPeriod + "'";

                TaxQuarterlyComplete = Convert.ToDecimal(getScallerValue(strQuery3));

                String strQuery4 = @"SELECT A2.ID FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE	A1.empID = '" + pEmp.ID + "' 	AND A2.PayrollPeriodID = '" + pIntPeriod + "'";
                TaxDetailID = Convert.ToInt32(getScallerValue(strQuery4));

                String strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE A1.empID = '" + pEmp.ID + "' AND A2.PayrollPeriodID < '" + pIntPeriod + "'";
                TaxValueIncentive = Convert.ToInt32(getScallerValue(strQuery5));


                if (TaxQuarterlyComplete > 0)
                {
                    QuarterlyValue = TaxQuarterlyComplete;
                }
                else
                {
                    QuarterlyValue = 0.0M;
                }

                //End Section

                string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
                strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
                strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
                strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
                strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + pEmp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + pPeriod.CalCode + "')";
                strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

                DataTable dtPreviousInfo = getDataTable(strPrevious);

                remainingPeriodsCnt = (from p in dbHrPayroll.CfgPeriodDates where p.ID >= pPeriod.ID && p.PayrollId == pEmp.PayrollID && p.CalCode == pPeriod.CalCode select p).Count();

                string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + pEmp.ID.ToString() + "'";
                decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));
                string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + pEmp.ID.ToString() + "'";
                decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));
                alreadyProcessedIncome = obIncome;
                alreadyProcessedTax = obTax;
                foreach (DataRow dr in dtPreviousInfo.Rows)
                {
                    alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                    alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

                }

                //TODO: For 1st month of calendar year for specific days
                if (false)
                {
                    //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * ((remainingPeriodsCnt-1)*empGrossSalary)) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                    //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * empGrossSalary)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                    //projectedIncome += TaxQuarterlyComplete;
                }
                else
                {
                    //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                    //projectedIncome += TaxQuarterlyComplete;
                    string strQueryFS = @"SELECT ISNULL(SUM(ISNULL(A1.EmpTaxblTotal,0)),0) AS TaxbleAmount, ISNULL(SUM(ISNULL(A1.EmpTotalTax,0)),0) AS PaidTax
                                            FROM dbo.TrnsFinalSettelmentRegister A1
                                            WHERE A1.EmpID = '" + pEmp.ID.ToString() + @"' 
                                            GROUP BY A1.EmpID";
                    DataTable dtFSInfo = getDataTable(strQueryFS);
                    foreach (DataRow dr in dtFSInfo.Rows)
                    {
                        projectedIncome += Convert.ToDecimal(dtFSInfo.Rows[0]["TaxbleAmount"]);
                        //alreadyProcessedTax += Convert.ToDecimal(dtFSInfo.Rows[0]["PaidTax"]);

                    }
                }

                if (TaxDiscountYearly == 0)
                {
                    currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;
                }
                else
                {
                    currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome + TaxDiscountYearly;
                }

                //Quarter Tax Adjustment 
                if (TaxQuarterlyComplete > 0)
                {
                    currentYearExpectedIncomeI = alreadyProcessedIncome + (projectedIncome + TaxQuarterlyComplete);
                    int cnt1 = (from p in dbHrPayroll.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == pPeriod.CalCode where p.MinAmount <= currentYearExpectedIncomeI && p.MaxAmount >= currentYearExpectedIncomeI select p).Count();
                    if (cnt1 > 0)
                    {
                        CfgTaxDetail taxLine = (from p in dbHrPayroll.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == pPeriod.CalCode where p.MinAmount <= currentYearExpectedIncomeI && p.MaxAmount >= currentYearExpectedIncomeI select p).Single();

                        currentYearExpectedTaxI = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncomeI - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;

                    }
                }

                int cnt = (from p in dbHrPayroll.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == pPeriod.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Count();
                if (cnt > 0)
                {
                    CfgTaxDetail taxLine = (from p in dbHrPayroll.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == pPeriod.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Single();

                    currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;

                    //if (TaxDiscountYearly != 0)
                    //{
                    //    currentYearExpectedTax -= TaxDiscountYearly;
                    //}

                    outValue = (currentYearExpectedTax - (alreadyProcessedTax - TaxValueIncentive)) / remainingPeriodsCnt;

                    if (TaxDiscountMonthly != 0)
                    {
                        outValue += TaxDiscountMonthly;
                    }

                    if (TaxQuarterlyComplete > 0)
                    {
                        decimal incentiveValue = Math.Abs(currentYearExpectedTax - currentYearExpectedTaxI);
                        outValue += incentiveValue;
                        if (incentiveValue > 0 && TaxDetailID != 0)
                        {
                            QuarterlyValue = incentiveValue;
                            //string strQuery6 = "UPDATE dbo.TrnsQuarterTaxAdjDetail SET TaxableAmount = '" + incentiveValue.ToString() + "' WHERE dbo.TrnsQuarterTaxAdjDetail.ID = '"+ TaxDetailID.ToString() +"'";
                            //ExecuteQueries(strQuery6);
                        }
                        else
                        {
                            QuarterlyValue = 0.0M;
                        }
                    }
                }

                // * ************TAX**************
                if (Program.systemInfo.TaxSetup == true && pEmp.FlgTax == true && outValue > 0)
                {
                    //decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, 1);
                    MstGLDetermination glDetr = ds.getEmpGl(oEmployee);
                    decimal TotalTax = outValue;
                    if (TotalTax > 0)
                    {

                        TrnsFinalSettelmentRegister oHeadFS = (from a in dbHrPayroll.TrnsFinalSettelmentRegister where a.FSHeadID == oHead.ID select a).FirstOrDefault();
                        oHeadFS.EmpTotalTax = TotalTax;
                        TrnsFinalSettelmentRegisterDetail spdetail = new TrnsFinalSettelmentRegisterDetail();
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

                        oHeadFS.TrnsFinalSettelmentRegisterDetail.Add(spdetail);
                    }
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void AddAdjustment(Int32 pEmployeeID)
        {
            try
            {
                TrnsFinalSettelmentRegister oDoc;
                MstGLDetermination glDetr = ds.getEmpGl(oEmployee);
                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead
                                    where a.InternalEmpID == pEmployeeID
                                    && a.EosType == (oEmployee.TermCount == null ? 1 : Convert.ToInt32(oEmployee.TermCount))
                                    select a).FirstOrDefault();
                if (oHead != null)
                {
                    oDoc = (from a in dbHrPayroll.TrnsFinalSettelmentRegister where a.FSHeadID == oHead.ID orderby a.Id select a).FirstOrDefault();
                }
                else
                {
                    return;
                }

                if (oDoc == null)
                {
                    return;
                }
                else
                {
                    if (oHead.DocStatus == "Posted")
                    {
                        oApplication.StatusBar.SetText("Can't Update Approved Final Settlement.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
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

                AdjustmentDetail.LineType = "FSAdjustment";
                AdjustmentDetail.LineSubType = "Adjustment";
                AdjustmentDetail.LineValue = Math.Round(Convert.ToDecimal(txtAdjustment.Value.Trim()), 0);
                AdjustmentDetail.LineMemo = Convert.ToString(txtRemarks.Value.Trim());
                AdjustmentDetail.DebitAccount = glDetr.DiffDRCR;
                AdjustmentDetail.CreditAccount = glDetr.BSPayable;
                AdjustmentDetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.DiffDRCR);
                AdjustmentDetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
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
                TrnsFSHead oEntry = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID && a.PayrollID == oEmployee.PayrollID select a).FirstOrDefault();
                if (oEntry == null)
                {
                    oApplication.StatusBar.SetText("No Final Settlement Found.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (oEntry != null)
                {
                    if (oEntry.DocStatus == "Posted")
                    {
                        oApplication.StatusBar.SetText("Can't Void An Approved Final Settlement", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                }
                //dbHrPayroll.TrnsFinalSettelmentRegister.DeleteOnSubmit(oEntry);
                foreach (TrnsFinalSettelmentRegister One in oEntry.TrnsFinalSettelmentRegister)
                {
                    IEnumerable<TrnsEmployeeElementDetail> nonRecuringElements = from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.MstEmployee.EmpID == One.MstEmployee.EmpID && p.PeriodId.ToString() == One.PayrollPeriodID.ToString() select p;
                    foreach (var recDetail in nonRecuringElements)
                    {
                        recDetail.FlgOneTimeConsumed = false;
                    }
                }
                dbHrPayroll.TrnsFSHead.DeleteOnSubmit(oEntry);
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
                Program.flgFinalSettelment = true;
                TrnsFSHead oMainDoc;
                oMainDoc = (from a in dbHrPayroll.TrnsFSHead 
                            where a.InternalEmpID == oEmployee.ID 
                            && a.EosType == (oEmployee.TermCount == null ? 1 : Convert.ToInt32(oEmployee.TermCount)) 
                            select a).FirstOrDefault();
                //if (oMainDoc == null && btnProcess.Caption == "Process")
                if (oMainDoc == null && (oForm.Mode==SAPbouiCOM.BoFormMode.fm_ADD_MODE))
                {
                    oMainDoc = new TrnsFSHead();
                    oMainDoc.MstEmployee = oEmployee;
                    oMainDoc.PayrollID = PayrollID;
                    oMainDoc.PeriodCounts = lstPeriodID.Count;
                    oMainDoc.DocStatus = "Draft";
                    oMainDoc.EosType = oEmployee.TermCount == null ? 1 : Convert.ToInt32(oEmployee.TermCount);
                    oMainDoc.JournalEntry = 0;
                    oMainDoc.CreateDt = DateTime.Now;
                    //oMainDoc.UpdateDt = DateTime.Now;
                    oMainDoc.CreatedBy = oCompany.UserName;
                    //oMainDoc.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.TrnsFSHead.InsertOnSubmit(oMainDoc);
                    dbHrPayroll.SubmitChanges();

                    foreach (Int32 Periods in lstPeriodID)
                    {
                        ProcessEOS(Periods);
                    }
                    EndOfServiceDues(oEmployee);
                    DateTime ResignDate, TerminationDate, Diff;

                    ResignDate = Convert.ToDateTime(oEmployee.ResignDate);
                    TerminationDate = Convert.ToDateTime(oEmployee.TerminationDate);
                    Decimal DaysCount = Math.Abs((ResignDate - TerminationDate).Days);
                    EndOfServiceTaxation(oEmployee, lstPeriodID[0], oMainDoc, DaysCount);
                    FillEOSDetails(oEmployee.ID, 1);
                    //btnProcess.Caption = "Ok";
                    ibtnVoid.Enabled = true;
                    oApplication.StatusBar.SetText("Successfuly Posted EOS of EmpID : " + oEmployee.EmpID, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    GetDataFilterData();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
                //else if (oMainDoc != null && btnProcess.Caption == "Update")
                else if (oMainDoc != null)
                {
                    AddAdjustment(Convert.ToInt32(oEmployee.ID));                   
                    oApplication.StatusBar.SetText("Record Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    FillEOSDetails(oEmployee.ID, 1);
                    GetDataFilterData();
                }
                //else if (btnProcess.Caption == "Ok")
                //else if (oForm.Mode==SAPbouiCOM.BoFormMode.fm_OK_MODE)
                //{
                //    oForm.Close();
                //}                

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
                if (oEmployee == null)
                    return;
                Program.flgFinalSettelment = false;
                VoidEOS(oEmployee.EmpID, PayrollID);
                InitializeEOSDetail();
                oApplication.StatusBar.SetText("Successfully Void Salary of EmpID : " + oEmployee, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidEOSClick Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillEOSDetails(Int32 pEOSID, Int32 pType)
        {
            try
            {
                //InitializeEOSDetail();
                Decimal totalPayable = 0.0M;
                Decimal contributionTotal = 0.0M;
                dtMain.Rows.Clear();
                TrnsFSHead oDoc = null;
                if (pType == 0)
                {
                    oDoc = (from p in dbHrPayroll.TrnsFSHead where p.ID == pEOSID select p).FirstOrDefault();
                }
                if (pType == 1)
                {
                    oDoc = (from p in dbHrPayroll.TrnsFSHead where p.InternalEmpID == pEOSID select p).FirstOrDefault();
                }
                if (oDoc != null)
                {
                    Int32 i = 0;
                    String strQuery = @"
                                        SELECT M3.LineType, M3.LineSubType, SUM(ISNULL(M3.LineValue,0)) AS LineValue
                                        FROM 
	                                        dbo.TrnsFSHead M1
	                                        INNER JOIN dbo.TrnsFinalSettelmentRegister M2 ON M1.ID = M2.FSHeadID
	                                        INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail M3 ON M2.Id = M3.FSID
                                        WHERE 
	                                        M1.ID = '" + oDoc.ID + @"'
                                            And M3.LineSubType <> 'Empr Cont'
                                        GROUP BY M3.LineType, M3.LineSubType
                                       ";
                    DataTable dtGrab = ds.getDataTable(strQuery);
                    foreach (DataRow oLine in dtGrab.Rows)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(cDesc.DataBind.Alias, i, oLine["LineSubType"].ToString());
                        dtMain.SetValue(cAmount.DataBind.Alias, i, oLine["LineValue"].ToString());
                        totalPayable += Convert.ToDecimal(oLine["LineValue"]);
                        if (oLine["LineType"].ToString() == "FSContribution")
                        {
                            contributionTotal += Convert.ToDecimal(oLine["LineValue"]);
                        }
                        i++;
                    }

                }
                mtMain.LoadFromDataSource();
                txtNetPayable.Value = totalPayable.ToString();
                txtContributions.Value = contributionTotal.ToString();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Function: FillEOSDetails Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitializeEOSDetail()
        {
            //txtBasicSalary.Value = "0.00";
            //txtGrossSalary.Value = "0.00";
            txtAdjustment.Value = "0.00";
            txtContributions.Value = "0.00";
            txtNetPayable.Value = "0.00";
            dtMain.Rows.Clear();
            mtMain.LoadFromDataSource();
            btnProcess.Caption = "Process";
            ibtnVoid.Enabled = false;
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empFSN";
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
                    txtEmployeeID.Value = Program.EmpID;
                    SelectEmployee(Program.EmpID);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public object getScallerValue(string strSql)
        {
            object outResult = new object();
            SqlConnection con = (SqlConnection)dbHrPayroll.Connection;
            try
            {

                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = strSql;
                outResult = cmd.ExecuteScalar();

            }
            catch
            {
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }

            return outResult;
        }

        public DataTable getDataTable(string strsql)
        {
            DataTable dt = new DataTable();
            SqlConnection con = (SqlConnection)dbHrPayroll.Connection;
            try
            {

                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = strsql;



                SqlDataReader dr = cmd.ExecuteReader();


                dt.Clear();
                dt.Rows.Clear();
                dt.Load(dr);

                dr.Close();
            }
            catch (Exception ex)
            {
                string tempvalue = ex.Message;
                tempvalue = "";
            }
            finally
            {
                // if (con.State == ConnectionState.Open) con.Close();
            }



            return dt;


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
