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
    class frm_FSAppr : HRMSBaseForm
    {
        #region Local Variable Area
        
        SAPbouiCOM.EditText txtEmployeeID, txtEmployeeName, txtArrears, txtContributions, txtNetPayable, txtBasicSalary;
        SAPbouiCOM.EditText txtDepartment, txtDesignation, txtJoiningDate, txtResignationDate, txtJENumber;
        SAPbouiCOM.EditText txtAdjustment, txtTerminationDate, txtRemarks, txtGrossSalary, txtPostingDt;
        SAPbouiCOM.Button btnDecision, btnPick, btnPostJE, btnPostInSBO;
        SAPbouiCOM.Item ibtnDecision, ibtnPick, ibtnPostJE, ibtnPostInSBO;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column cDesc, cAmount;
        SAPbouiCOM.Matrix mtMain;


        IEnumerable<TrnsFSHead> oCollection = null;
        MstEmployee oEmployee = null;
        Int32 NoOfDays = 0, PayrollID = 0, ResignPeriodID = 0;
        DateTime JoiningDate, ResignDate, TerminationDate;
        String ResignPeriodName = "";
        List<Int32> lstPeriodID = new List<Int32>();

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
            if (pVal.BeforeAction == false)
            {
                switch (pVal.ItemUID)
                {
                    case "btdecision":
                        Approved();
                        break;
                    case "btpick":
                        doFind();
                        break;
                    case "btpost":
                        //PostFS();
                        if (btnPostJE.Caption == "Cancel JE")
                        {
                            VoidJEOnly();
                        }
                        else if (btnPostJE.Caption == "Post JE")
                        {
                            if (Convert.ToBoolean(Program.systemInfo.ProvidentFund))
                            {
                                PostFSNewATC();
                            }
                            else if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                            {
                                //PostFSWithUAE();
                                PostFSNew();
                            }
                            else
                            {
                                PostFSNew();
                            }
                        }
                        break;
                    case "btsboPost":
                        SBOPost();
                        break;
                }
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
            //SelectEmployee();
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

                txtJENumber = oForm.Items.Item("txJE").Specific;
                oForm.DataSources.UserDataSources.Add("txJE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtJENumber.DataBind.SetBound(true, "", "txJE");

                txtBasicSalary = oForm.Items.Item("txbasic").Specific;
                oForm.DataSources.UserDataSources.Add("txbasic", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txbasic");

                txtGrossSalary = oForm.Items.Item("txgrsslry").Specific;
                oForm.DataSources.UserDataSources.Add("txgrsslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossSalary.DataBind.SetBound(true, "", "txgrsslry");

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

                txtPostingDt = oForm.Items.Item("txpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txpdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtPostingDt.DataBind.SetBound(true, "", "txpdt");

                mtMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtdetail");
                cAmount = mtMain.Columns.Item("Amount");
                cDesc = mtMain.Columns.Item("desc");

                btnDecision = oForm.Items.Item("btdecision").Specific;
                ibtnDecision = oForm.Items.Item("btdecision");
                btnPostJE = oForm.Items.Item("btpost").Specific;
                ibtnPostJE = oForm.Items.Item("btpost");
                btnPick = oForm.Items.Item("btpick").Specific;
                ibtnPick = oForm.Items.Item("btpick");

                btnPostInSBO = oForm.Items.Item("btsboPost").Specific;
                ibtnPostInSBO = oForm.Items.Item("btsboPost");

                //mtOthElements = oForm.Items.Item("mtOEle").Specific;
                //dtOthElements = oForm.DataSources.DataTables.Item("dtOtherEle");
                btnPostJE.Caption = "Post JE";
                btnPostInSBO.Caption = "Post In SBO";
                btnDecision.Caption = "Approved";
                GetData();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : Initialize Form Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void doFind()
        {

            OpenNewSearchForm();
        }

        private void GetData()
        {

            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.TrnsFSHead select a;
            Int32 i = 0;
            foreach (TrnsFSHead One in oCollection)
            {
                CodeIndex.Add(One.ID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void SelectEmployee(String EmpID)
        {
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpID select a).FirstOrDefault();
                oEmployee = oEmp;
                //TrnsResignation oDoc = oCollection.ElementAt<TrnsResignation>(currentRecord);
                txtEmployeeID.Value = oEmp.EmpID;
                txtEmployeeName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                txtDepartment.Value = oEmp.MstDepartment.DeptName.ToString();
                txtDesignation.Value = oEmp.MstDesignation.Description.ToString();
                txtJoiningDate.Value = Convert.ToDateTime(oEmp.JoiningDate).ToString("yyyyMMdd");
                txtResignationDate.Value = Convert.ToDateTime(oEmp.ResignDate).ToString("yyyyMMdd");
                txtTerminationDate.Value = Convert.ToDateTime(oEmp.TerminationDate).ToString("yyyyMMdd");
                txtBasicSalary.Value = Convert.ToString(oEmp.BasicSalary);
                txtGrossSalary.Value = ds.getEmpGross(oEmp).ToString();
                ResignDate = Convert.ToDateTime(oEmp.ResignDate);
                TerminationDate = Convert.ToDateTime(oEmp.TerminationDate);
                JoiningDate = Convert.ToDateTime(oEmp.JoiningDate);
                PayrollID = Convert.ToInt32(oEmp.PayrollID);

                //Check Already Processed or Not

                TrnsFSHead oDocCount = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID && a.EosType == (oEmp.TermCount == null ? 1 : Convert.ToInt32(oEmp.TermCount)) select a).FirstOrDefault();
                if (oDocCount != null)
                {
                    //txtJENumber.Value = oDocCount.JournalEntry.ToString();
                    TrnsJE oJe = (from a in dbHrPayroll.TrnsJE where a.ID == oDocCount.JournalEntry select a).FirstOrDefault();
                    if (oJe != null)
                    {
                        if (oJe.SBOJeNum != null && oJe.SBOJeNum > 0)
                        {
                            txtJENumber.Value = Convert.ToString(oJe.ID) + " : " + Convert.ToString(oJe.SBOJeNum);
                        }
                        else
                        {
                            txtJENumber.Value = Convert.ToString(oJe.ID);
                        }
                    }
                    if (oDocCount.DocStatus == "Draft")
                    {
                        Int32 DoOcId = oDocCount.ID;
                        FillEOSDetails(DoOcId);
                        ibtnDecision.Enabled = true;
                        ibtnPostJE.Enabled = false;
                        ibtnPostInSBO.Enabled = false;
                    }
                    else if (oDocCount.DocStatus == "Posted")
                    {
                        Int32 DoOcId = oDocCount.ID;
                        FillEOSDetails(DoOcId);
                        ibtnDecision.Enabled = false;
                        if (oDocCount.JournalEntry == 0)
                        {
                            ibtnPostInSBO.Enabled = false;
                            ibtnPostJE.Enabled = true;
                            btnPostJE.Caption = "Post JE";
                        }
                        else if (oDocCount.JournalEntry > 0)
                        {
                            TrnsJE oJE = (from a in dbHrPayroll.TrnsJE where a.ID == oDocCount.JournalEntry select a).FirstOrDefault();
                            if (oJe == null)
                            {
                            }
                            else if (oJe.SBOJeNum != null)
                            {
                                ibtnPostInSBO.Enabled = false;
                                ibtnPostJE.Enabled = false;
                            }
                            else if (oJe.SBOJeNum == null)
                            {
                                ibtnPostInSBO.Enabled = true;
                                ibtnPostJE.Enabled = true;
                                btnPostJE.Caption = "Cancel JE";
                            }
                        }
                    }
                }
                else
                {
                    //ibtnVoid.Enabled = false;
                    //ibtnProcess.Enabled = true;
                    //btnDecision.Caption = "Approve";
                }


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SelectEmployee Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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

                //* Add the Adjustment Line in Final Settelment ******** 
                //******************************************************
                //
                TrnsFinalSettelmentRegisterDetail AdjustmentDetail = new TrnsFinalSettelmentRegisterDetail();
                AdjustmentDetail.LineType = "Adjustment";
                AdjustmentDetail.LineSubType = "Adjustment";
                AdjustmentDetail.LineValue = Math.Round(Convert.ToDecimal(txtAdjustment.Value.Trim()), 0);
                AdjustmentDetail.LineMemo = "Adjustment";
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

                //employeeRemainingSalary += (decimal)ConDetail.LineValue;
                oDoc.TrnsFinalSettelmentRegisterDetail.Add(AdjustmentDetail);

                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddAdjustment Error : " + ex.Message);
            }
        }

        private void Approved()
        {
            try
            {

                int confirm = oApplication.MessageBox("Approving FS is ir-reversable Process. Are you sure you want to post final settlement? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                TrnsFSHead oMain = (from a in dbHrPayroll.TrnsFSHead
                                    where a.MstEmployee.ID == oEmployee.ID
                                    && a.EosType == (oEmployee.TermCount == null ? 1 : Convert.ToInt32(oEmployee.TermCount))
                                    select a).FirstOrDefault();
                if (oMain != null && btnDecision.Caption == "Approved")
                {
                    //**************
                    foreach (TrnsFinalSettelmentRegister FSRegister in oMain.TrnsFinalSettelmentRegister)
                    {
                        foreach (TrnsFinalSettelmentRegisterDetail One in FSRegister.TrnsFinalSettelmentRegisterDetail)
                        {
                            //Post Loans 
                            if (One.LineType == "Loan Recovery")
                            {
                                TrnsLoan oLoanDoc = (from a in dbHrPayroll.TrnsLoan where a.ID.ToString() == One.LineBaseEntry.ToString() select a).FirstOrDefault();
                                //oLoanDoc.TrnsLoanDetail[0].RecoveredAmount = 0;
                            }
                            //Post Advances
                            if (One.LineType == "Advance Recovery")
                            {
                                TrnsAdvance oAdvanceDoc = (from a in dbHrPayroll.TrnsAdvance where a.ID.ToString() == One.LineBaseEntry.ToString() select a).FirstOrDefault();
                                //oAdvanceDoc.RemainingAmount = 0;
                            }
                        }
                    }
                    //**************
                    oMain.DocStatus = "Posted";
                    oEmployee.FlgActive = false;
                    oEmployee.EmployeeContractType = "RTRD";
                    dbHrPayroll.SubmitChanges();
                    ibtnDecision.Enabled = false;
                    ibtnPostJE.Enabled = true;
                    oApplication.StatusBar.SetText("Successfuly Posted Final Settlement of EmpID : " + oEmployee.EmpID, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Processed Error: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillEOSDetails(Int32 pEOSID)
        {
            try
            {
                //InitializeEOSDetail();
                Decimal totalPayable = 0.0M;
                Decimal contributionTotal = 0.0M;
                Decimal adjustment = 0.0M;
                dtMain.Rows.Clear();
                TrnsFSHead oDoc = (from p in dbHrPayroll.TrnsFSHead where p.ID == pEOSID select p).FirstOrDefault();
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
                        if (oLine["LineType"].ToString() == "FSAdjustment")
                        {
                            adjustment += Convert.ToDecimal(oLine["LineValue"]);
                        }
                        i++;
                    }
                }
                mtMain.LoadFromDataSource();
                txtNetPayable.Value = totalPayable.ToString();
                txtContributions.Value = contributionTotal.ToString();
                txtAdjustment.Value = adjustment.ToString();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Function: FillEOSDetails Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitializeEOSDetail()
        {
            txtBasicSalary.Value = "0.00";
            txtGrossSalary.Value = "0.00";
            txtAdjustment.Value = "0.00";
            txtRemarks.Value = "";
            txtArrears.Value = "0.00";
            txtContributions.Value = "0.00";
            txtNetPayable.Value = "0.00";
            dtMain.Rows.Clear();
            mtMain.LoadFromDataSource();
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "Search";
                Program.sqlString = "empFSApr";
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

        private void PostFS()
        {
            try
            {
                int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;

                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead
                                    where a.InternalEmpID == oEmployee.ID
                                    select a).FirstOrDefault();
                TrnsFinalSettelmentRegister oRegister = (from a in dbHrPayroll.TrnsFinalSettelmentRegister
                                                         where a.FSHeadID == oHead.ID
                                                         orderby a.Id descending
                                                         select a).FirstOrDefault();
                MstGLDetermination glDetr = ds.getEmpGl(oEmployee);

                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                string pdate = txtPostingDt.Value.Trim();
                if (string.IsNullOrEmpty(pdate))
                {
                    je.JEPostingDate = oRegister.CfgPeriodDates.EndDate;
                }
                else
                {
                    je.JEPostingDate = DateTime.ParseExact(pdate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                
                //je.PeriodId = payrollperiod.ID;
                je.PayrollID = oRegister.CfgPayrollDefination.ID;
                je.PeriodID = oRegister.CfgPeriodDates.ID;
                //je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;
                je.Memo = " Payroll JE for period " + oRegister.CfgPeriodDates.PeriodName;

                //Debit
                TrnsJEDetail jed = new TrnsJEDetail();
                jed.AcctCode = glDetr.EOSExpese;
                jed.AcctName = Program.objHrmsUI.getAcctName(glDetr.EOSExpese);
                jed.Debit = Convert.ToDecimal(txtNetPayable.Value);
                jed.Credit = Convert.ToDecimal(0.0M);
                if (!String.IsNullOrEmpty(oEmployee.CostCenter))
                {
                    jed.CostCenter = oEmployee.CostCenter;
                }
                je.TrnsJEDetail.Add(jed);

                //Credit
                TrnsJEDetail jec = new TrnsJEDetail();
                jec.AcctCode = glDetr.EOSPayable;
                jec.AcctName = Program.objHrmsUI.getAcctName(glDetr.EOSPayable);
                jec.Debit = Convert.ToDecimal(0.0M);
                jec.Credit = Convert.ToDecimal(txtNetPayable.Value);
                if (!String.IsNullOrEmpty(oEmployee.CostCenter))
                {
                    jec.CostCenter = oEmployee.CostCenter;
                }
                je.TrnsJEDetail.Add(jec);

                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();

                int jeNum = je.ID;

                oHead.JournalEntry = jeNum;

                //dbHrPayroll.SubmitChanges();

                txtJENumber.Value = jeNum.ToString();
                postIntoSbo(jeNum.ToString());
                ibtnPostJE.Enabled = false;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Fucntion : PostFS Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        //For Normal Condition
        private void PostFSNew()
        {
            try
            {
                int confirm = oApplication.MessageBox("EOS JE posting is irr-reversable. Are you sure you want to post salary? ", 2, "Yes", "No");
                if (confirm == 2) return;
                
                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID
                                    && a.EosType == (oEmployee.TermCount == null ? 1 : Convert.ToInt32(oEmployee.TermCount))
                                    select a).FirstOrDefault();
                
                SearchKeyVal.Clear();
                SearchKeyVal.Add("FSID", oHead.ID);
                string JeSql = sqlString.getSql("FSJEQuery", SearchKeyVal);

                
                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found" || string.IsNullOrEmpty(strCode))
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;               
                je.FlgCanceled = false;
                je.FlgPosted = false;
                string pdate = txtPostingDt.Value.Trim();
                if (string.IsNullOrEmpty(pdate))
                {
                    je.JEPostingDate = oHead.TrnsFinalSettelmentRegister[0].CfgPeriodDates.EndDate;
                }
                else
                {
                    je.JEPostingDate = DateTime.ParseExact(pdate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                //je.JEPostingDate = DateTime.Now;
                je.PayrollID = oHead.TrnsFinalSettelmentRegister[0].MstEmployee.CfgPayrollDefination.ID;
                je.PeriodID = oHead.TrnsFinalSettelmentRegister[0].PayrollPeriodID;
                je.Memo = " Payroll EOS JE for Emp : " + oEmployee.EmpID ; 

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.CostCenter = dr["CostCenter"].ToString();
                    if (Program.systemInfo.FlgBranches == true)
                    {
                        jed.BranchName = dr["Branches"].ToString();
                    }
                    if (Program.systemInfo.FlgProject == true)
                    {
                        jed.Project = dr["Project"].ToString();
                    }
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;
                
                oHead.JournalEntry = jeNum;

                dbHrPayroll.SubmitChanges();

                txtJENumber.Value = jeNum.ToString();
                //postIntoSbo(jeNum.ToString());
                ibtnPostJE.Enabled = false;
                
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        //for uae with gratuity changes
        private void PostFSWithUAE()
        {
            try
            {
                int confirm = oApplication.MessageBox("EOS JE posting is irr-reversable. Are you sure you want to post salary? ", 2, "Yes", "No");
                if (confirm == 2) return;

                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID select a).FirstOrDefault();

                SearchKeyVal.Clear();
                SearchKeyVal.Add("FSEmpID", oEmployee.EmpID);
                string JeSql = sqlString.getSql("FSUAEJEQuery", SearchKeyVal);


                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found" || string.IsNullOrEmpty(strCode))
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                string pdate = txtPostingDt.Value.Trim();
                if (string.IsNullOrEmpty(pdate))
                {
                    je.JEPostingDate = oHead.TrnsFinalSettelmentRegister[0].CfgPeriodDates.EndDate;
                }
                else
                {
                    je.JEPostingDate = DateTime.ParseExact(pdate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                //je.JEPostingDate = DateTime.Now;
                je.PayrollID = oHead.TrnsFinalSettelmentRegister[0].MstEmployee.CfgPayrollDefination.ID;
                je.PeriodID = oHead.TrnsFinalSettelmentRegister[0].PayrollPeriodID;
                je.Memo = " Payroll EOS JE for Emp : " + oEmployee.EmpID;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.CostCenter = dr["CostCenter"].ToString();
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                oHead.JournalEntry = jeNum;
                
                dbHrPayroll.SubmitChanges();

                txtJENumber.Value = jeNum.ToString();
                btnPostJE.Caption = "Cancel JE";
                ibtnPostInSBO.Enabled = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PostFSWithUAEGratuity : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        //FOr Atc with cfgpayrolldefination mai provident fund true ho to hi chalay ga
        private void PostFSNewATC()
        {
            try
            {
                int confirm = oApplication.MessageBox("EOS JE posting is irr-reversable. Are you sure you want to post salary? ", 2, "Yes", "No");
                if (confirm == 2) return;

                TrnsFSHead oHead = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmployee.ID select a).FirstOrDefault();

                SearchKeyVal.Clear();
                SearchKeyVal.Add("FSEmpID", oEmployee.EmpID);
                string JeSql = sqlString.getSql("FSJEQueryATC", SearchKeyVal);


                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found" || string.IsNullOrEmpty(strCode))
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                string pdate = txtPostingDt.Value.Trim();
                if (string.IsNullOrEmpty(pdate))
                {
                    je.JEPostingDate = oHead.TrnsFinalSettelmentRegister[0].CfgPeriodDates.EndDate;
                }
                else
                {
                    je.JEPostingDate = DateTime.ParseExact(pdate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                //je.JEPostingDate = DateTime.Now;
                je.PayrollID = oHead.TrnsFinalSettelmentRegister[0].MstEmployee.CfgPayrollDefination.ID;
                je.PeriodID = oHead.TrnsFinalSettelmentRegister[0].PayrollPeriodID;
                je.Memo = " Payroll EOS JE for Emp : " + oEmployee.EmpID;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.CostCenter = dr["CostCenter"].ToString();
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                oHead.JournalEntry = jeNum;

                dbHrPayroll.SubmitChanges();

                txtJENumber.Value = jeNum.ToString();
                postIntoSbo(jeNum.ToString());
                ibtnPostJE.Enabled = false;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void postIntoSbo(String selJe)
        {
            try
            {
                
                //int confirm = oApplication.MessageBox("Are you sure you want to post draft? ", 3, "Yes", "No", "Cancel");
                //if (confirm == 2 || confirm == 3) return;
                TrnsJE je = (from p in dbHrPayroll.TrnsJE where p.ID.ToString() == selJe select p).FirstOrDefault();
                
                string strResult = Program.objHrmsUI.postJe(je.ID);

                if (strResult.Contains("Error"))
                {
                    oApplication.SetStatusBarMessage(strResult);
                }
                else
                {
                    je.SBOJeNum = Convert.ToInt32(strResult);
                    dbHrPayroll.SubmitChanges();
                    ibtnPostInSBO.Enabled = false;
                    ibtnPostJE.Enabled = false;
                    ibtnDecision.Enabled = false;
                    oApplication.StatusBar.SetText("Successfully posted JE.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    //doPostingTransactions(Convert.ToInt32(selJe));
                    //getPEmployees();
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void VoidJEOnly()
        {
            try
            {
                string JeTextValue = "", EmployeeID = "";
                JeTextValue = txtJENumber.Value;
                EmployeeID = txtEmployeeID.Value;
                if (!string.IsNullOrEmpty(JeTextValue) && !string.IsNullOrEmpty(EmployeeID))
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeID select a).FirstOrDefault();
                    if (oEmp == null) return;
                    TrnsFSHead oFS = (from a in dbHrPayroll.TrnsFSHead where a.InternalEmpID == oEmp.ID && a.EosType == (oEmp.TermCount == null ? 1 : Convert.ToInt32(oEmp.TermCount)) select a).FirstOrDefault();
                    if (oFS == null) return;
                    TrnsJE oJE = (from a in dbHrPayroll.TrnsJE where a.ID.ToString() == JeTextValue.Trim() select a).FirstOrDefault();
                    if (oJE != null)
                    {
                        oFS.JournalEntry = 0;
                        dbHrPayroll.TrnsJE.DeleteOnSubmit(oJE);
                        dbHrPayroll.SubmitChanges();
                        txtJENumber.Value = "";
                        ibtnDecision.Enabled = false;
                        ibtnPostJE.Enabled = true;
                        btnPostJE.Caption = "Post JE";
                        ibtnPostInSBO.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void SBOPost()
        {
            try
            {
                string EmpValue = txtEmployeeID.Value;
                if (!string.IsNullOrEmpty(EmpValue))
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee 
                                        where a.EmpID == EmpValue 
                                        select a).FirstOrDefault();
                    if (oEmp == null) return;

                    TrnsFSHead oDoc = (from a in dbHrPayroll.TrnsFSHead 
                                       where a.InternalEmpID == oEmp.ID 
                                       //&& a.PeriodCounts == oEmp.TermCount 
                                       select a).FirstOrDefault();
                    if (oDoc == null) return;

                    postIntoSbo(oDoc.JournalEntry.ToString());
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

    }
}
