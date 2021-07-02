using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Collections;
using System.Data.SqlClient;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_LoanPay : HRMSBaseForm
    {
        #region "Global Variable Area"

        //public IEnumerable<TrnsAdvance> Employees;
        SAPbouiCOM.Item itxtEmployeeCode,ItxCode;
        SAPbouiCOM.Button btSave, btCancel;
        SAPbouiCOM.EditText txtEmpCode, txtEmpName, txtRemarks, txtManager, txtdocNum, txtPostDt, txtMatDt, txtALNum, txtAmnt, txACode, txtAcName, txtAdLTyp, txtAdLT;
        SAPbouiCOM.ComboBox cb_LnAType, cbPfrom;
        public string SelectedAccount = "";
        string debitAccount, debitAccountName;
        Hashtable elementGls = new Hashtable();
        


        #endregion  

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                FillAdvanceNLoanTypeCombo();
                FillPaymentFromCombo();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LoanPay Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "cbLyA":
                     LoadDocuments();
                    break;
                case "txDocNum":
                    if (txtdocNum != null)
                    {
                        LoadSelectedData(txtdocNum.Value);
                    }
                    break;                                                   
                default:
                    break;
            }

        }
        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCfl(ref pVal, ref BubbleEvent);
            SAPbouiCOM.ChooseFromListEvent ocfl = (SAPbouiCOM.ChooseFromListEvent)pVal;
            string itemId = pVal.ItemUID;
            if (itemId == "txACode")
            {
                SAPbouiCOM.Item cflItem = oForm.Items.Item(itemId);
                SAPbouiCOM.DataTable oDT = ocfl.SelectedObjects;
                if (cflItem.Type.ToString() == "it_EDIT")
                {
                    if (SelectedAccount == Convert.ToString(oDT.GetValue("AcctCode", 0)))
                    {                     
                        return;
                    }
                    SAPbouiCOM.EditText txt = oForm.Items.Item(itemId).Specific;
                    SelectedAccount = Convert.ToString(oDT.GetValue("AcctCode", 0));
                    if (!string.IsNullOrEmpty(SelectedAccount))
                    {
                        //txACode.Value = SelectedAccount;
                        oForm.DataSources.UserDataSources.Item(itemId).ValueEx = Convert.ToString(oDT.GetValue("AcctCode", 0));
                        txtAcName.Value = Convert.ToString(oDT.GetValue("AcctName", 0));
                    }    
                }
            }          

        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        SavePaymentRecord();
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

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;                
                btCancel = oForm.Items.Item("2").Specific;                
                //Initializing Textboxes

                txtEmpName = oForm.Items.Item("txtEmpN").Specific;                
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;           
                itxtEmployeeCode = oForm.Items.Item("txtEmpC");

                txtRemarks = oForm.Items.Item("txtRemark").Specific;
                txtManager = oForm.Items.Item("txtManager").Specific;           
                txtALNum = oForm.Items.Item("txtReqNum").Specific;
                txtdocNum = oForm.Items.Item("txDocNum").Specific;

                cb_LnAType = oForm.Items.Item("cbLyA").Specific;
                cbPfrom = oForm.Items.Item("cbPfrom").Specific;
                                            
                oForm.DataSources.UserDataSources.Add("txtPDate", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtPostDt = oForm.Items.Item("txtPDate").Specific;
                txtPostDt.DataBind.SetBound(true, "", "txtPDate");

                oForm.DataSources.UserDataSources.Add("txtMDate", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtMatDt = oForm.Items.Item("txtMDate").Specific;
                txtMatDt.DataBind.SetBound(true, "", "txtMDate");

                oForm.DataSources.UserDataSources.Add("txtAmnt", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtAmnt = oForm.Items.Item("txtAmnt").Specific;
                txtAmnt.DataBind.SetBound(true, "", "txtAmnt");

                oForm.DataSources.UserDataSources.Add("txACode", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 30);
                txACode = oForm.Items.Item("txACode").Specific;
                txACode.DataBind.SetBound(true, "", "txACode");
                ItxCode = oForm.Items.Item("txACode");

                txtAcName = oForm.Items.Item("txtAcName").Specific;
                txtAdLTyp = oForm.Items.Item("txtAdLTyp").Specific;
                txtAdLT = oForm.Items.Item("txtAdLT").Specific;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public void FillAdvanceNLoanTypeCombo()
        {
            try
            {
                cb_LnAType.ValidValues.Add("None", "None");
                cb_LnAType.ValidValues.Add("LN", "Loan");
                cb_LnAType.ValidValues.Add("ADV", "Advance");               
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: FillAdvanceNLoanTypeCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }                       
        }
        public void FillPaymentFromCombo()
        {
            cbPfrom.ValidValues.Add("BNK", "Bank");
            cbPfrom.ValidValues.Add("ACNT", "Account");
        }
        private void LoadSelectedData(String pCode)
        {           
            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    int docNum = Convert.ToInt32(pCode);                   
                    string LAType = cb_LnAType.Value;
                    if (string.IsNullOrEmpty(LAType))
                    {
                        return;
                    }
                    else
                    {
                        if (LAType == "ADV")
                        {
                            var getEmpAdvanceDetail = dbHrPayroll.TrnsAdvance.Where(e => e.DocNum == docNum && e.DocStatus == "LV0002" && e.DocAprStatus == "LV0006").FirstOrDefault();                                               
                            if (getEmpAdvanceDetail != null)
                            {
                                txtEmpName.Value = getEmpAdvanceDetail.EmpName;
                                txtEmpCode.Value = getEmpAdvanceDetail.MstEmployee.EmpID;
                                txtManager.Value = getEmpAdvanceDetail.ManagerName;
                                txtAdLTyp.Value = Convert.ToString(getEmpAdvanceDetail.AdvanceType);
                                if (!string.IsNullOrEmpty(txtAdLTyp.Value))
                                {
                                    txtAdLT.Value = dbHrPayroll.MstAdvance.Where(adv => adv.Id == Convert.ToInt32(txtAdLTyp.Value)).FirstOrDefault().Description;
                                }                        
                                txtPostDt.Value = Convert.ToDateTime(getEmpAdvanceDetail.CreateDate).ToString("yyyyMMdd");
                                txtALNum.Value = Convert.ToString(getEmpAdvanceDetail.ID);
                                txtdocNum.Value = Convert.ToString(getEmpAdvanceDetail.DocNum);
                                txtAmnt.Value = String.Format("{0:0.00}", getEmpAdvanceDetail.ApprovedAmount);
                                txtMatDt.Value = getEmpAdvanceDetail.MaturityDate == null ? "" : Convert.ToDateTime(getEmpAdvanceDetail.MaturityDate).ToString("yyyyMMdd");
                                var Remarks = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == getEmpAdvanceDetail.DocNum && a.DocType == 20 && a.FlgActive == false).FirstOrDefault();
                                if (Remarks != null)
                                {
                                    txtRemarks.Value = Remarks.Remarks;
                                }
                                else
                                {
                                    txtRemarks.Value = "";
                                }
                            }
                        }
                        else if (LAType == "LN")
                        {
                            var getEmpLoanDetail = dbHrPayroll.TrnsLoan.Where(l => l.DocNum == docNum && l.DocStatus == "LV0002" && l.DocAprStatus == "LV0006").FirstOrDefault();                            
                            var LoanDet = dbHrPayroll.TrnsLoanDetail.Where(a => a.LnAID == getEmpLoanDetail.ID).FirstOrDefault();
                            if (getEmpLoanDetail != null && LoanDet != null)
                            {
                                txtEmpCode.Value = getEmpLoanDetail.MstEmployee.EmpID;
                                txtEmpName.Value = getEmpLoanDetail.EmpName;
                                txtManager.Value = getEmpLoanDetail.ManagerName;
                                txtPostDt.Value = Convert.ToDateTime(getEmpLoanDetail.CreateDate).ToString("yyyyMMdd");
                                txtALNum.Value = Convert.ToString(getEmpLoanDetail.ID);
                                txtAdLTyp.Value = Convert.ToString(LoanDet.LoanType);
                                if (!string.IsNullOrEmpty(txtAdLTyp.Value))
                                {
                                    txtAdLT.Value = dbHrPayroll.MstLoans.Where(loan => loan.Id == Convert.ToInt32(txtAdLTyp.Value)).FirstOrDefault().Description;
                                }                                  
                                txtdocNum.Value = Convert.ToString(getEmpLoanDetail.DocNum);
                                txtAmnt.Value = String.Format("{0:0.00}", LoanDet.ApprovedAmount);
                                txtMatDt.Value = LoanDet.MaturityDate == null ? "" : Convert.ToDateTime(LoanDet.MaturityDate).ToString("yyyyMMdd");
                                var Remarks = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == getEmpLoanDetail.DocNum && a.DocType == 11 && a.FlgActive == false).FirstOrDefault();
                                if (Remarks != null)
                                {
                                    txtRemarks.Value = Remarks.Remarks;
                                }
                                else
                                {
                                    txtRemarks.Value = "";
                                }

                            }                     
                        }
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void LoadDocuments()
        {
            string RecordType = cb_LnAType.Value;
            switch (RecordType)
            {
                case "ADV":

                    String query = @"SELECT ta.DocNum, ISNULL(EmpName,'''') AS EmpName
                                            FROM " + Program.objHrmsUI.HRMSDbName + ".dbo.TrnsAdvance ta INNER JOIN " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee te ON ta.EmpID = te.ID Where DocStatus=''LV0002'' AND DocAprStatus=''LV0006''";
                    Program.objHrmsUI.addFms("frm_LoanPay", "txDocNum", "-1", query);  
                  
                    break;
                case "LN":

                    String queryLN = @"SELECT ta.DocNum, ISNULL(EmpName,'''') AS EmpName
                                            FROM " + Program.objHrmsUI.HRMSDbName + ".dbo.TrnsLoan ta INNER JOIN " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee te ON ta.EmpID = te.ID  Where DocStatus=''LV0002'' AND DocAprStatus=''LV0006''";
                    Program.objHrmsUI.addFms("frm_LoanPay", "txDocNum", "-1", queryLN);

                    break;
                default:

                    break;
            }

        }
        private void SavePaymentRecord()
        {
            try
            {
                string TransactionType = cb_LnAType.Value;
                switch (TransactionType)
                {
                    case "ADV":
                        SaveAdvancePayment();
                        break;
                    case "LN":
                        //SaveLoanPayment();
                        //OutGoingPaymentEntry();
                        PostPost();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: SavePaymentRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void SaveAdvancePayment()
        {
            try
            {
                if (string.IsNullOrEmpty(cbPfrom.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Payment From", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txACode.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Valid Account", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

                TrnsLoanAndAdvancePayment loanadvPayment = new TrnsLoanAndAdvancePayment();
                TrnsLoanAndAdvancePaymentDetail oChild = new TrnsLoanAndAdvancePaymentDetail();
                MstEmployee mstEmployee=dbHrPayroll.MstEmployee.Where(e=>e.EmpID==txtEmpCode.Value).Single();
                MstAdvance mstAdvance = dbHrPayroll.MstAdvance.Where(a => a.Id == Convert.ToInt32(txtAdLTyp.Value)).Single();

                loanadvPayment.DocType = 20;
                loanadvPayment.Series = -1;
                loanadvPayment.EmpID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault().ID;
                if (txtMatDt.Value != "")
                {
                    loanadvPayment.MaturityDate = DateTime.ParseExact(txtMatDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                loanadvPayment.LAType = cb_LnAType.Value;
                loanadvPayment.LoadAdvanceID = Convert.ToInt32(txtALNum.Value);
                loanadvPayment.Manager = txtManager.Value;
                loanadvPayment.Amount = Convert.ToDecimal(txtAmnt.Value);
                loanadvPayment.CreateDate = DateTime.Now;
                loanadvPayment.UserId = oCompany.UserName; 
                if (cbPfrom.Value == "BNK")
                {
                    loanadvPayment.FlgBank = true;
                    loanadvPayment.FlgAccount = false;
                }
                else if (cbPfrom.Value == "ACNT")
                {
                    loanadvPayment.FlgAccount = true;
                    loanadvPayment.FlgBank = false;
                    if (mstAdvance != null && mstEmployee != null)
                    {
                        elementGls = ds.getAdvGL(mstEmployee, mstAdvance);
                        debitAccount = elementGls["DrAcct"].ToString();
                        debitAccountName = elementGls["DrAcctName"].ToString();
                    }     
                }                                                 
                if (!string.IsNullOrEmpty(debitAccount) && !string.IsNullOrEmpty(debitAccountName))
                {
                    oChild.GLAccount = debitAccount;
                    oChild.GLName = debitAccountName;
                    oChild.DocumentRemarks = txtRemarks.Value;
                    oChild.Amount = Convert.ToDecimal(txtAmnt.Value);
                    oChild.CreateDate = DateTime.Now;
                    oChild.UserId = oCompany.UserName;
                    loanadvPayment.TrnsLoanAndAdvancePaymentDetail.Add(oChild);                   
                }
                dbHrPayroll.TrnsLoanAndAdvancePayment.InsertOnSubmit(loanadvPayment);
                dbHrPayroll.SubmitChanges();
                var oOldAdvance = dbHrPayroll.TrnsAdvance.Where(a => a.ID == Convert.ToInt32(txtALNum.Value)).FirstOrDefault();
                if (oOldAdvance != null)
                {
                    oOldAdvance.DocStatus = "LV0003"; //Doc Closed                  
                }               
                dbHrPayroll.SubmitChanges();
                ClearFieldRecords();
                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: SaveAdvancePayment Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void SaveLoanPayment()
        {
            try
            {
                TrnsLoanAndAdvancePayment loanadvPayment = new TrnsLoanAndAdvancePayment();
                TrnsLoanAndAdvancePaymentDetail oChild = new TrnsLoanAndAdvancePaymentDetail();
                MstEmployee mstEmployee = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).Single();
                MstLoans mstLoan = dbHrPayroll.MstLoans.Where(a => a.Id == Convert.ToInt32(txtAdLTyp.Value)).Single();

                loanadvPayment.DocType = 11;
                loanadvPayment.Series = -1;
                loanadvPayment.EmpID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault().ID;
                if (!string.IsNullOrEmpty(txtMatDt.Value))
                {
                    loanadvPayment.MaturityDate = DateTime.ParseExact(txtMatDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                loanadvPayment.LAType = cb_LnAType.Value;
                loanadvPayment.LoadAdvanceID = Convert.ToInt32(txtALNum.Value);
                loanadvPayment.Manager = txtManager.Value;
                loanadvPayment.Amount = Convert.ToDecimal(txtAmnt.Value);
                loanadvPayment.CreateDate = DateTime.Now;
                loanadvPayment.UserId = oCompany.UserName;
                if (cbPfrom.Value == "BNK")
                {
                    loanadvPayment.FlgBank = true;
                    loanadvPayment.FlgAccount = false;
                }
                else if (cbPfrom.Value == "ACNT")
                {
                    loanadvPayment.FlgAccount = true;
                    loanadvPayment.FlgBank = false;
                    if (mstLoan != null && mstEmployee != null)
                    {
                        elementGls = ds.getLoanGL(mstEmployee, mstLoan);
                        debitAccount = elementGls["DrAcct"].ToString();
                        debitAccountName = elementGls["DrAcctName"].ToString();
                    }
                }
                dbHrPayroll.TrnsLoanAndAdvancePayment.InsertOnSubmit(loanadvPayment);
                if (!string.IsNullOrEmpty(debitAccount) && !string.IsNullOrEmpty(debitAccountName))
                {
                    oChild.GLAccount = debitAccount;
                    oChild.GLName = debitAccountName;
                    oChild.DocumentRemarks = txtRemarks.Value;
                    oChild.Amount = Convert.ToDecimal(txtAmnt.Value);
                    oChild.CreateDate = DateTime.Now;
                    oChild.UserId = oCompany.UserName;
                    loanadvPayment.TrnsLoanAndAdvancePaymentDetail.Add(oChild);
                }
               
                var oOldLoan = dbHrPayroll.TrnsLoan.Where(a => a.ID == Convert.ToInt32(txtALNum.Value)).FirstOrDefault();
                if (oOldLoan != null)
                {
                    oOldLoan.DocStatus = "LV0003"; //Doc Closed                   
                }
                dbHrPayroll.SubmitChanges();
                ClearFieldRecords();
                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: SaveLoanPayment Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void ClearFieldRecords()
        {
            try
            {
                cb_LnAType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtdocNum.Value = string.Empty;
                txtEmpCode.Value = string.Empty;
                txtEmpName.Value = string.Empty;
                txtRemarks.Value = string.Empty;
                cbPfrom.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txACode.Value = string.Empty;
                txtAcName.Value = string.Empty;
                txtPostDt.Value = string.Empty;
                txtMatDt.Value = string.Empty;
                txtALNum.Value = string.Empty;
                txtAmnt.Value = string.Empty;
                txtManager.Value = string.Empty;
                txtAdLT.Value = string.Empty;
                txtAdLTyp.Value = string.Empty;                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void PostPaymentEntry(long sourceId)
        {
            int jdtdocentry = 0;
            int jdtDocLine = 0;
            try
            {
                TrnsLoanAndAdvancePayment pmt = dbHrPayroll.TrnsLoanAndAdvancePayment.Where(p => p.Id == sourceId).FirstOrDefault();

                SAPbobsCOM.Payments vPay = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                IEnumerable<TrnsLoanAndAdvancePayment> pmts = from p in dbHrPayroll.TrnsLoanAndAdvancePayment where p.Id == sourceId select p;
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, pmts);
                string bankGl = "";
                vPay.CardCode = pmt.TrnsLoanAndAdvancePaymentDetail.FirstOrDefault().GLAccount;
                vPay.DocDate = (DateTime)pmt.TrnsLoanAndAdvancePaymentDetail.FirstOrDefault().CreateDate;

                vPay.JournalRemarks = "Loan/Advance Payment - " + pmt.Id.ToString();
                vPay.TaxDate = (DateTime)pmt.TrnsLoanAndAdvancePaymentDetail.FirstOrDefault().UpdateDate;
                vPay.UserFields.Fields.Item("U_CMSPaymentId").Value = pmt.Id.ToString();
                vPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PaymentAdvice;
                vPay.Invoices.DocEntry = jdtdocentry;
                vPay.Invoices.DocLine = jdtDocLine;
                vPay.Invoices.SumApplied = Convert.ToDouble(txtAmnt.Value);
                vPay.Invoices.Add();
            }
            catch (Exception Ex)
            {
               oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: PostPaymentEntry Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void OutGoingPaymentEntry()
        {
            try
            {
                MstEmployee mstEmployee = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault();
                MstLoans mstLoan = dbHrPayroll.MstLoans.Where(a => a.Id == Convert.ToInt32(txtAdLTyp.Value)).FirstOrDefault();
                if (mstLoan != null && mstEmployee != null)
                {
                    elementGls = ds.getLoanGL(mstEmployee, mstLoan);
                    debitAccount = elementGls["DrAcct"].ToString();
                    debitAccountName = elementGls["DrAcctName"].ToString();
                }
                //Payments _payment = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                ////_payment.DocNum = 1;
                //_payment.AccountPayments.AccountCode = txACode.Value;
                //_payment.AccountPayments.AccountName = txtAcName.Value;
                //_payment.AccountPayments.SumPaid = Convert.ToDouble(txtAmnt.Value);
                //_payment.AccountPayments.Decription = "Manual OutGoing Payment";
                //_payment.CardCode = debitAccount;
                //_payment.CardName = debitAccountName;

                //_payment.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                //_payment.DocType = SAPbobsCOM.BoRcptTypes.rAccount;
                //_payment.DocTypte = SAPbobsCOM.BoRcptTypes.rAccount;

                //_payment.TaxDate = DateTime.Now;
                //_payment.TransferAccount = "_SYS00000000420";
                //_payment.TransferDate = DateTime.Now;
                //_payment.TransferReference = "ref01";
                //_payment.TransferSum = Convert.ToDouble(txtAmnt.Value);
                //_payment.ApplyVAT = SAPbobsCOM.BoYesNoEnum.tNO;
                //_payment.AccountPayments.Add();
                //int errorCode = _payment.Add();
                //if (errorCode != 0)
                //{
                //    //Error In adding OutGoing Payment Entry
                //} 

                int jdtdocentry = 0;
                int jdtDocLine = 0;
                SAPbobsCOM.Payments vPay = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                vPay.CardCode = debitAccount;
                vPay.DocDate = DateTime.Now;
                vPay.JournalRemarks = "Loan Payment - " + txtdocNum.Value.ToString();
                vPay.TaxDate = DateTime.ParseExact(txtMatDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                vPay.UserFields.Fields.Item("U_EmpID").Value = txtEmpCode.Value;
                vPay.UserFields.Fields.Item("U_RecordID").Value = txtdocNum.Value;
                vPay.UserFields.Fields.Item("U_LnAdvType").Value = "Laon";


                vPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_JournalEntry;
                vPay.Invoices.DocEntry = jdtdocentry;//20
                vPay.Invoices.DocLine = jdtDocLine; //
                vPay.Invoices.SumApplied = Convert.ToDouble(txtAmnt.Value);
                vPay.Invoices.Add();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanPay Function: OutGoingPaymentEntry Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }


        //public string postPayment(long sourceId)
        //{

        //    SqlCommand cmd = new SqlCommand();
        //    //SqlConnection con = new SqlConnection(Program.constr);
        //    //con.Open();
        //   // cmd.Connection = con;
        //    SqlDataReader dr;
        //    string outStr = "";
        //    //IntPaymentSource pmt = (from p in db.IntPaymentSource where p.SourceId == sourceId select p).Single();
        //    //string objectKey = "";
        //    //if (alreadyExist("Payment", pmt.PmtId.ToString(), out objectKey))
        //    //{
        //    //    dtError.Rows.Add(DateTime.Now.ToString(), "Payment Already Exist");
        //    //    return objectKey;

        //    //}

        //    SAPbobsCOM.Payments vPay = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);

        //    //SAPbobsCOM.Payments vJE = (SAPbobsCOM.Payments)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);


        //    IEnumerable<IntPaymentSource> pmts = from p in db.IntPaymentSource where p.SourceId == sourceId select p;
        //    db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, pmts);
        //    string bankGl = "";
        //    dtError.Rows.Add(DateTime.Now.ToString(), "Checking Bank GL");

        //    IEnumerable<IntPaymentInvoicesSource> pmtinvoices = from p in db.IntPaymentInvoicesSource where p.PaymentId == pmt.PmtId orderby p.JeDetailNum select p;
        //    if (pmt.BranchName.Contains("Hold"))
        //    {
        //        bankGl = pmt.BankAcctNumber;
        //    }
        //    else
        //    {
        //        bankGl = getBankGL(pmt.BankCode, pmt.BranchName, pmt.BankAcctNumber);
        //    }
        //    if (!bankGl.Contains("_SYS"))
        //    {
        //        return "Error : GL Account of bank is not defined or Check Branch Field of SBO with Branch name field of House bank of CaneMs";
        //    }

        //    dtError.Rows.Add(DateTime.Now.ToString(), "Checking Bank GL");

        //    vPay.CardCode = pmt.BpCode;
        //    vPay.DocDate = (DateTime)pmt.PostingDate;

        //    vPay.JournalRemarks = "CaneMs Payment - " + pmt.PmtId.ToString();
        //    vPay.TaxDate = (DateTime)pmt.PostingDate;
        //    vPay.UserFields.Fields.Item("U_CMSPaymentId").Value = pmt.PmtId.ToString();
        //    if (caneDetail.PaymentSeries > 0)
        //    {
        //        vPay.Series = (int)caneDetail.PaymentSeries;
        //    }

        //    foreach (IntPaymentInvoicesSource inv in pmtinvoices)
        //    {
        //        int jdtdocentry = 0;
        //        int jdtDocLine = 0;
        //        try
        //        {
        //            cmd.CommandText = "select top 1 * from jdt1 inner join ojdt on ojdt.transid=jdt1.transid where jdt1.Ref1='" + inv.JeDetailNum.ToString() + "' and ojdt.ref1='From CMS' ";
        //            dr = cmd.ExecuteReader();
        //            if (dr.Read())
        //            {
        //                jdtdocentry = Convert.ToInt32(dr["TransId"]);
        //                jdtDocLine = Convert.ToInt32(dr["Line_ID"]);
        //            }
        //            dr.Close();
        //        }
        //        catch { }
        //        vPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_JournalEntry;
        //        vPay.Invoices.DocEntry = jdtdocentry;
        //        vPay.Invoices.DocLine = jdtDocLine;
        //        vPay.Invoices.SumApplied = Convert.ToDouble(inv.PaidAmnt);
        //        vPay.Invoices.Add();

        //    }
        //    if (pmt.CheckSum == 0)
        //    {

        //        long zeroJenum = postZeroPmtJe(bankGl, pmt);
        //        vPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_JournalEntry;
        //        vPay.Invoices.DocEntry = Convert.ToInt32(zeroJenum);
        //        vPay.Invoices.DocLine = 1;
        //        vPay.Invoices.SumApplied = 0.001;
        //        vPay.Invoices.Add();


        //    }

        //    if (con.State == ConnectionState.Open) con.Close();
        //    dtError.Rows.Add(DateTime.Now.ToString(), "Invoices selected");

        //    if (pmt.BranchName.Contains("Hold"))
        //    {
        //        vPay.CashAccount = pmt.AccountNumber;
        //        if (pmt.CheckSum > 0)
        //        {
        //            vPay.CashSum = Convert.ToDouble(pmt.CheckSum);
        //        }
        //        else
        //        {
        //            vPay.CashSum = 0.001;


        //        }

        //    }
        //    else
        //    {
        //        vPay.CashAccount = bankGl;

        //        vPay.Checks.AccounttNum = pmt.BankAcctNumber;
        //        vPay.Checks.CountryCode = "PK";
        //        vPay.Checks.BankCode = pmt.BankCode;
        //        vPay.Checks.Branch = pmt.Branch;
        //        //vPay.Checks.ManualCheck = SAPbobsCOM.BoYesNoEnum.tYES;
        //        vPay.Checks.CheckNumber = Convert.ToInt32(pmt.CheckNumber);
        //        if (pmt.CheckSum > 0)
        //        {
        //            vPay.Checks.CheckSum = Convert.ToDouble(pmt.CheckSum);
        //        }
        //        else
        //        {
        //            vPay.Checks.CheckSum = 0.001;


        //        }

        //        //vPay.Checks.Details = "test1";
        //        vPay.Checks.DueDate = (DateTime)pmt.DueDate;
        //        vPay.Checks.Trnsfrable = 0;
        //    }
        //    //vPay.Checks.Add();
        //    dtError.Rows.Add(DateTime.Now.ToString(), "Trying to post payment");

        //    int paidDoc = vPay.Add();
        //    if (paidDoc != 0)
        //    {
        //        int erroCode = 0;
        //        string errDescr = "";
        //        Program.oCompany.GetLastError(out erroCode, out errDescr);
        //        dtError.Rows.Add(DateTime.Now.ToString(), "Not posted Error :" + errDescr);

        //        outStr = "SBO Error:" + errDescr;
        //    }
        //    else
        //    {
        //        outStr = Convert.ToString(Program.oCompany.GetNewObjectKey());
        //    }


        //    return outStr;
        //}

        public void PostPost()
        {

            SAPbobsCOM.Payments oPays = default(SAPbobsCOM.Payments);

            oPays = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);



            oPays.AccountPayments.AccountCode = "_SYS00000000420";

            //oPays.AccountPayments.AccountName = "Juan Perez"

            oPays.AccountPayments.Decription = "Pago Manual";

            oPays.AccountPayments.SumPaid = 250;



            //oPays.CardCode = "ADA001-S" '"_SYS00000000031"

            //oPays.CardName = "Juan Perez"



            oPays.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;

            oPays.DocType = SAPbobsCOM.BoRcptTypes.rAccount;

            oPays.DocTypte = SAPbobsCOM.BoRcptTypes.rAccount;



            //oPays.DocCurrency = "USD"

            oPays.DocDate = DateTime.Now;

            //oPays.IsPayToBank = tNO

            oPays.JournalRemarks = "pag efect";

            //oPays.LocalCurrency = tNO

            //oPays.PaymentPriority = bopp_Priority_6

            //oPays.Series = 10

            oPays.TaxDate = DateTime.Now;

            oPays.TransferAccount = "_SYS00000000420";

            oPays.TransferDate = DateTime.Now;

            oPays.TransferReference = "ref01";

            oPays.TransferSum = 250;



            oPays.ApplyVAT = SAPbobsCOM.BoYesNoEnum.tNO;

            //oPays.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO

            //oPays.Proforma = SAPbobsCOM.BoYesNoEnum.tNO

            //oPays.DocNum = 3



            oPays.AccountPayments.Add();


            if (oPays.Add() != 0)
            {
                //Interaction.MsgBox(oCompany.GetLastErrorDescription);

                //oApplication.StatusBar.SetText(oCompany.GetLastErrorDescription, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);


            }
            // If oCompany.GetLastErrorCode <> 0 Then

            //Debug.Print(oCom.GetLastErrorDescription)
          

        }

        #endregion
    }
}
