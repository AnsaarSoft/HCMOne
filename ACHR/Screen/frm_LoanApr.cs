using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Globalization;

namespace ACHR.Screen
{
    class frm_LoanApr : HRMSBaseForm
    {
        #region "Global Variable Area"
        
        SAPbouiCOM.Button btApprove,btnReject, btCancel;
        SAPbouiCOM.EditText txReqBy, txEmpCode, txdocNum, txManager, txdoj, txdesig, txtSalary, txtOriginator, txtReAmnt, txtInstall, txtReqDt, txtAprIns, txtAprAm;
        SAPbouiCOM.ComboBox cb_LnTyp;
        SAPbouiCOM.DataTable dtLoanRequest;       
        SAPbouiCOM.Matrix grdLoanDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, loanType, Amount, RecToDate, RemToDate, Installment;
        SAPbouiCOM.Button btId;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                FillParentLoanTypeCombo();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanRequest Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
       
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btnApprv":
                        ApproveLoan();
                        break;
                    case "btId":
                        picDoc();
                        break;
                    case "btnRej":
                        RejectLoan();
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
                btApprove = oForm.Items.Item("btnApprv").Specific;
                btnReject = oForm.Items.Item("btnRej").Specific;
                btCancel = oForm.Items.Item("2").Specific;

                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("tbRby", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txReqBy = oForm.Items.Item("tbRby").Specific;
                txReqBy.DataBind.SetBound(true, "", "tbRby");

                oForm.DataSources.UserDataSources.Add("tbEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txEmpCode = oForm.Items.Item("tbEmpC").Specific;
                txEmpCode.DataBind.SetBound(true, "", "tbEmpC");

                cb_LnTyp = oForm.Items.Item("cb_LnTyp").Specific;

                //Initializing ComboBxes
                oForm.DataSources.UserDataSources.Add("tbDocNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txdocNum = oForm.Items.Item("tbDocNum").Specific;
                txdocNum.DataBind.SetBound(true, "", "tbDocNum");


                oForm.DataSources.UserDataSources.Add("tbManagr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txManager = oForm.Items.Item("tbManagr").Specific;
                txManager.DataBind.SetBound(true, "", "tbManagr");

                oForm.DataSources.UserDataSources.Add("tbdtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txdoj = oForm.Items.Item("tbdtJoin").Specific;
                txdoj.DataBind.SetBound(true, "", "tbdtJoin");

                oForm.DataSources.UserDataSources.Add("tbDesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txdesig = oForm.Items.Item("tbDesig").Specific;
                txdesig.DataBind.SetBound(true, "", "tbDesig");

                oForm.DataSources.UserDataSources.Add("tbSalry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtSalary = oForm.Items.Item("tbSalry").Specific;
                txtSalary.DataBind.SetBound(true, "", "tbSalry");

                oForm.DataSources.UserDataSources.Add("tbOrig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtOriginator = oForm.Items.Item("tbOrig").Specific;
                txtOriginator.DataBind.SetBound(true, "", "tbOrig");

                oForm.DataSources.UserDataSources.Add("txtReAmnt", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtReAmnt = oForm.Items.Item("txtReAmnt").Specific;
                txtReAmnt.DataBind.SetBound(true, "", "txtReAmnt");

                oForm.DataSources.UserDataSources.Add("txtInstall", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtInstall = oForm.Items.Item("txtInstall").Specific;
                txtInstall.DataBind.SetBound(true, "", "txtInstall");

                oForm.DataSources.UserDataSources.Add("txtReqDt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtReqDt = oForm.Items.Item("txtReqDt").Specific;
                txtReqDt.DataBind.SetBound(true, "", "txtReqDt");

                oForm.DataSources.UserDataSources.Add("txtAprIns", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtAprIns = oForm.Items.Item("txtAprIns").Specific;
                txtAprIns.DataBind.SetBound(true, "", "txtAprIns");

                oForm.DataSources.UserDataSources.Add("txtAprAm", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtAprAm = oForm.Items.Item("txtAprAm").Specific;
                txtAprAm.DataBind.SetBound(true, "", "txtAprAm");

                InitiallizegridMatrix();               

                string loginUserId = oCompany.UserName;                             
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
                dtLoanRequest = oForm.DataSources.DataTables.Add("LoanRequest");
                dtLoanRequest.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtLoanRequest.Columns.Add("LoanType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("RecToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("RemToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Installment", SAPbouiCOM.BoFieldsType.ft_Text);

                grdLoanDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grdLDet").Specific;
                oColumns = (SAPbouiCOM.Columns)grdLoanDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "No");

                oColumn = oColumns.Item("loanType");
                loanType = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "LoanType");

                oColumn = oColumns.Item("Amount");
                Amount = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "Amount");

                oColumn = oColumns.Item("cl_RecTD");
                RecToDate = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "RecToDate");

                oColumn = oColumns.Item("RemToDate");
                RemToDate = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "RemToDate");

                oColumn = oColumns.Item("cl_Inst");
                Installment = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "Installment");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
     
        private void LoadSelectedDatabyDocNum(string docNum)
        {
            try
            {
                if (!string.IsNullOrEmpty(docNum))
                {
                    int docNumber = Convert.ToInt32(docNum);
                    var getEmpLoan = (from a in dbHrPayroll.TrnsLoan
                                      where a.DocNum == docNumber && a.DocStatus == "LV0001" && a.DocAprStatus == "LV0005"
                                      select a).FirstOrDefault();
                    var getLoanDetail = getEmpLoan.TrnsLoanDetail.FirstOrDefault();
                    if (getEmpLoan != null && getLoanDetail != null)
                    {                        
                        txReqBy.Value = getEmpLoan.EmpName;
                        txEmpCode.Value = getEmpLoan.MstEmployee.EmpID;
                        txManager.Value = getEmpLoan.ManagerName;
                        txdoj.Value = getEmpLoan.DateOfJoining == null ? "" : Convert.ToDateTime(getEmpLoan.DateOfJoining).ToString("yyyyMMdd");
                        txdesig.Value = getEmpLoan.Designation;
                        txtSalary.Value = String.Format("{0:0.00}", getEmpLoan.Salary);
                        txtOriginator.Value = Convert.ToString(getEmpLoan.OriginatorName);
                        string strLoanTypeCode = Convert.ToString(dbHrPayroll.MstLoans.Where(a => a.Id == getLoanDetail.LoanType).FirstOrDefault().Code);
                        cb_LnTyp.Select(strLoanTypeCode, SAPbouiCOM.BoSearchKey.psk_ByValue);
                        txtReAmnt.Value = Convert.ToString(getLoanDetail.RequestedAmount);
                        txtAprAm.Value = String.Format("{0:0.00}", getLoanDetail.RequestedAmount);
                        txtAprIns.Value = String.Format("{0:0.00}", getLoanDetail.Installments);
                        txtInstall.Value = Convert.ToString(getLoanDetail.Installments);
                        txtReqDt.Value = Convert.ToDateTime(getLoanDetail.RequiredDate).ToString("yyyyMMdd");
                        GetLoanHistory(getEmpLoan.MstEmployee.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanRequest Function: LoadSelectedDatabyDocNum Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }        

        private void FillParentLoanTypeCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLoans);
                var Data = from v in dbHrPayroll.MstLoans select v;
                foreach (var v in Data)
                {
                    cb_LnTyp.ValidValues.Add(v.Code, v.Description);
                }                               
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ApproveLoan()
        {
            try
            {
                int intdocNum = Convert.ToInt32(txdocNum.Value);
                if (!String.IsNullOrEmpty(txEmpCode.Value))
                {
                    string loginEmpID = oCompany.UserName;                    
                    var aapLoanRecord = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == intdocNum && a.EmpID == loginEmpID && a.DocType == 11 && a.FlgActive == true).FirstOrDefault();
                    int empID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txEmpCode.Value).FirstOrDefault().ID;
                    var LoanRecordID = dbHrPayroll.TrnsLoan.Where(a => a.DocNum == intdocNum && a.DocType == 11 && a.EmpID == empID).FirstOrDefault();
                    var LoanDetail = LoanRecordID.TrnsLoanDetail.FirstOrDefault();
                    if (aapLoanRecord != null && LoanDetail != null)
                    {
                        LoanDetail.ApprovedAmount = string.IsNullOrEmpty(txtAprAm.Value) ? 0 : Convert.ToDecimal(txtAprAm.Value);
                        LoanDetail.ApprovedInstallment = string.IsNullOrEmpty(txtAprIns.Value) ? 0 : Convert.ToDecimal(txtAprIns.Value);
                        LoanDetail.UpdateDate = DateTime.Now;
                        aapLoanRecord.LineStatusID = "LV0006"; //Loan Approved
                        aapLoanRecord.Remarks = "Loan Approved";
                        aapLoanRecord.UpdateDt = DateTime.Now;
                    }
                    else
                    {
                        LoanDetail.ApprovedAmount = string.IsNullOrEmpty(txtAprAm.Value) ? 0 : Convert.ToDecimal(txtAprAm.Value);
                        LoanDetail.ApprovedInstallment = string.IsNullOrEmpty(txtAprIns.Value) ? 0 : Convert.ToDecimal(txtAprIns.Value);
                        LoanDetail.UpdateDate = DateTime.Now;
                        LoanRecordID.DocAprStatus = "LV0006";
                        LoanRecordID.DocStatus = "LV0002";
                    }
                    dbHrPayroll.SubmitChanges();
                    ClearControls();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    //oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    oApplication.StatusBar.SetText("Record Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);                    
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void RejectLoan()
        {
            try
            {
                int intdocNum = Convert.ToInt32(txdocNum.Value);
                
                if (!String.IsNullOrEmpty(txEmpCode.Value))
                {
                    string loginEmpID = oCompany.UserName;
                    int empID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txEmpCode.Value).FirstOrDefault().ID;
                    var LoanRecordID = dbHrPayroll.TrnsLoan.Where(a => a.DocNum == intdocNum && a.DocType == 11 && a.EmpID == empID).FirstOrDefault();
                    var LoanDetail = LoanRecordID.TrnsLoanDetail.FirstOrDefault();
                    //EMID b Where Clause main Dalni hai.
                    var aapLoanRecord = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == intdocNum && a.EmpID == loginEmpID && a.DocType == 11 && a.FlgActive == true).FirstOrDefault();
                    if (aapLoanRecord != null)
                    {
                        aapLoanRecord.LineStatusID = "LV0007"; //Loan Rejected
                        aapLoanRecord.UpdateDt = DateTime.Now;
                        aapLoanRecord.Remarks = "Loan has been Rejected";
                    }
                    else
                    {
                        LoanDetail.ApprovedAmount = string.IsNullOrEmpty(txtAprAm.Value) ? 0 : Convert.ToDecimal(txtAprAm.Value);
                        LoanDetail.ApprovedInstallment = string.IsNullOrEmpty(txtAprIns.Value) ? 0 : Convert.ToDecimal(txtAprIns.Value);
                        LoanDetail.UpdateDate = DateTime.Now;
                        LoanRecordID.DocAprStatus = "LV0007";
                        LoanRecordID.DocStatus = "LV0002";                             
                    }
                    dbHrPayroll.SubmitChanges();
                    ClearControls();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    //oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    oApplication.StatusBar.SetText("Record Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);                    
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ClearControls()
        {
            try
            {
                txReqBy.Value = string.Empty;
                txEmpCode.Value = string.Empty;
                txdocNum.Value = string.Empty;
                txManager.Value = string.Empty;
                txdoj.Value = string.Empty;
                txdesig.Value = string.Empty;
                txtSalary.Value = string.Empty;
                txtOriginator.Value = string.Empty;
                txtAprAm.Value = string.Empty;
                txtAprIns.Value = string.Empty;
                cb_LnTyp.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtReAmnt.Value = string.Empty;
                txtInstall.Value = string.Empty;
                txtReqDt.Value = string.Empty;
                dtLoanRequest.Rows.Clear();
                grdLoanDetail.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetLoanHistory(int intEmpID)
        {
            string strDocStatus = "LV0003", strApprovalStatus = "LV0006";
            try
            {
                var Data = (from n in dbHrPayroll.TrnsLoanDetail
                            join e in dbHrPayroll.TrnsLoan on n.LnAID equals e.ID
                            where e.EmpID == intEmpID && n.TrnsLoan.DocAprStatus == strApprovalStatus
                            select new
                            {
                                LoanID = n.MstLoans.Id,
                                LoanType = n.MstLoans.Description,
                                ApprovedAmmount = n.RequestedAmount,
                                EmpID = e.EmpID,
                                //ReceivedAmount=dbHrPayroll.TrnsLoanRegister.Where(LR=>LR.LoanID==LoanID)
                                Installment = n.Installments
                            }).ToList();
                Int16 i = 0;
                if (Data.Count() == 0)
                {
                    dtLoanRequest.Rows.Clear();
                    grdLoanDetail.LoadFromDataSource();
                    return;
                }
                else if (Data != null && Data.Count > 0)
                {
                    dtLoanRequest.Rows.Clear();
                    dtLoanRequest.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        dtLoanRequest.SetValue("No", i, i + 1);
                        dtLoanRequest.SetValue("LoanType", i, WD.LoanType);
                        dtLoanRequest.SetValue("Amount", i, WD.ApprovedAmmount.ToString());
                        //dtLoanRequest.SetValue("RecToDate", i, WD.EmpName);
                        dtLoanRequest.SetValue("RecToDate", i, "0");
                        dtLoanRequest.SetValue("RemToDate", i, WD.ApprovedAmmount.ToString());
                        dtLoanRequest.SetValue("Installment", i, WD.Installment.ToString());
                        i++;
                    }
                    grdLoanDetail.LoadFromDataSource();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }

        private void picDoc()
        {
            string strSql = sqlString.getSql("LoanApprovalDoc", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select DocNumber", "Select Employee for Loan Approval");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txdocNum.Value = st.Rows[0][0].ToString();
                LoadSelectedDatabyDocNum(txdocNum.Value);
            }
        }

        #endregion
    }
}
