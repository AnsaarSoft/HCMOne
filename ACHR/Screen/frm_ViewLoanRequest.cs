using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_ViewLoanRequest : HRMSBaseForm
    {
        #region "Global Variable Area"
        
        SAPbouiCOM.EditText txReqBy, txEmpCode, txManager, txdoj, txdesig, tbSalary;     
        SAPbouiCOM.DataTable dtLoanRequest;
        SAPbouiCOM.Matrix grdLoanDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clID, clDocNum, loanType, clDate, ReqAmount, Amount, RecToDate, RemToDate, Installment, cl_status, cl_Stop;    
        SAPbouiCOM.Button btId;

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
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "1":
                        //if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                            UpdateLoanStatus();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LoanRequest Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txEmpCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txEmpCode.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            } 
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }

        #endregion

        #region "Local Methods"

        private void InitiallizegridMatrix()
        {
            try
            {
                dtLoanRequest = oForm.DataSources.DataTables.Add("LoanRequest");
                dtLoanRequest.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtLoanRequest.Columns.Add("DocID", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("DocNo", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("LoanType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Date", SAPbouiCOM.BoFieldsType.ft_Date);
                dtLoanRequest.Columns.Add("ReqAmt", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Installment", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("RecToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("RemToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Status", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLoanRequest.Columns.Add("Stop", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
               

                grdLoanDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grdLDet").Specific;
                oColumns = (SAPbouiCOM.Columns)grdLoanDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "No");

                oColumn = oColumns.Item("clID");
                clID = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "DocID");
                
                oColumn = oColumns.Item("docNum");
                clDocNum = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "DocNo");

                oColumn = oColumns.Item("loanType");
                loanType = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "LoanType");

                oColumn = oColumns.Item("cl_date");
                clDate = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "Date");


                oColumn = oColumns.Item("ReqAmt");
                ReqAmount = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "ReqAmt");

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

                oColumn = oColumns.Item("cl_status");
                cl_status = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "Status");

                oColumn = oColumns.Item("cl_Stop");
                cl_Stop = oColumn;
                oColumn.DataBind.Bind("LoanRequest", "Stop");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public void InitiallizeForm()
        {
            try
            {                
                btId = oForm.Items.Item("btId").Specific;
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("tbRby", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80);
                txReqBy = oForm.Items.Item("tbRby").Specific;
                txReqBy.DataBind.SetBound(true, "", "tbRby");

                txEmpCode = oForm.Items.Item("tbEmpC").Specific;                                        

                oForm.DataSources.UserDataSources.Add("tbManagr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80);
                txManager = oForm.Items.Item("tbManagr").Specific;
                txManager.DataBind.SetBound(true, "", "tbManagr");

                oForm.DataSources.UserDataSources.Add("tbdtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txdoj = oForm.Items.Item("tbdtJoin").Specific;
                txdoj.DataBind.SetBound(true, "", "tbdtJoin");

                oForm.DataSources.UserDataSources.Add("tbDesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txdesig = oForm.Items.Item("tbDesig").Specific;
                txdesig.DataBind.SetBound(true, "", "tbDesig");

                oForm.DataSources.UserDataSources.Add("tbSalry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                tbSalary = oForm.Items.Item("tbSalry").Specific;
                tbSalary.DataBind.SetBound(true, "", "tbSalry");
                                                       
                InitiallizegridMatrix();                
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void picEmp()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empLoan", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Loan");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txEmpCode.Value = st.Rows[0][0].ToString();
                LoadSelectedData(txEmpCode.Value);
            }
        }
        
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("empid", txEmpCode.Value.ToString());          
        }
        
        private void LoadSelectedData(String pCode)
        {
            try
            {                               
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == pCode
                                  select a).FirstOrDefault();                    

                    if (getEmp != null)
                    {                                               
                        txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        txManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                        txdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                        txdesig.Value = getEmp.DesignationName;
                        tbSalary.Value = getEmp.BasicSalary != null ? String.Format("{0:0.00}", getEmp.BasicSalary) : "";                        
                        GetLoanHistory(getEmp.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanRequest Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void GetLoanHistoryOld(int intEmpID)
        {            
            try
            {
                var Data = (from n in dbHrPayroll.TrnsLoanDetail
                            join e in dbHrPayroll.TrnsLoan on n.LnAID equals e.ID
                            where e.EmpID == intEmpID
                            select new
                            {
                                LId = e.DocNum,
                                LoanID = e.ID,
                                LoanType = n.MstLoans.Description,
                                RequestedAmmount = n.RequestedAmount,
                                ApprovedAmmount = n.ApprovedAmount,
                                
                                RequiredDate=n.RequiredDate,
                                EmpID = e.EmpID,                               
                                //Installment = n.Installments,
                                Installment = n.Installments.GetValueOrDefault(),
                                //approvedInstallment=n.ApprovedInstallment,
                                approvedInstallment = n.ApprovedInstallment.GetValueOrDefault(),
                                ApprovalStatus=n.TrnsLoan.DocAprStatus,
                                RecAmount = n.RecoveredAmount.GetValueOrDefault(),
                                //RecAmount =n.RecoveredAmount,
                                Stop=n.FlgStopRecovery
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
                    decimal RemaingAmount = 0;
                    decimal approvedInstallment=0;

                    dtLoanRequest.Rows.Clear();
                    dtLoanRequest.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        var DueAmount = dbHrPayroll.TrnsLoanRegister.Where(LR => LR.LoanID == WD.LId).FirstOrDefault();
                        var ApprovalStatusDetail = dbHrPayroll.MstLOVE.Where(LV => LV.Code == WD.ApprovalStatus).FirstOrDefault();
                        dtLoanRequest.SetValue("No", i, i + 1);
                        dtLoanRequest.SetValue("DocID", i, WD.LoanID);
                        dtLoanRequest.SetValue("DocNo", i, WD.LId);
                        dtLoanRequest.SetValue("LoanType", i, WD.LoanType);
                        dtLoanRequest.SetValue("Date", i, WD.RequiredDate);
                        if (WD.Stop.Value)
                        {
                            dtLoanRequest.SetValue("Stop", i, "Y");
                        }
                        else
                        {
                            dtLoanRequest.SetValue("Stop", i, "N");
                        }
                        dtLoanRequest.SetValue("ReqAmt", i, WD.RequestedAmmount.ToString());
                        dtLoanRequest.SetValue("Amount", i, WD.ApprovedAmmount.ToString());
                        if (ApprovalStatusDetail != null)
                        {
                            dtLoanRequest.SetValue("Status", i, ApprovalStatusDetail.Value);
                        }
                        if (DueAmount != null)
                        {
                            //dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", DueAmount.RecoveredAmount));
                            dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", DueAmount.RecoveredAmount.GetValueOrDefault()));

                            RemaingAmount = WD.ApprovedAmmount.Value - DueAmount.RecoveredAmount.Value;
                        }
                        else
                        {
                            dtLoanRequest.SetValue("RecToDate", i, "0");
                        }
                        
                        dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", WD.RecAmount));
                        RemaingAmount = WD.ApprovedAmmount.Value - WD.RecAmount;

                        dtLoanRequest.SetValue("RemToDate", i, RemaingAmount.ToString());
                        
                        approvedInstallment=Convert.ToDecimal(WD.approvedInstallment);

                        if (approvedInstallment < 1)
                        {
                            dtLoanRequest.SetValue("Installment", i, WD.Installment.ToString());
                        }
                        else
                        {
                            dtLoanRequest.SetValue("Installment", i, WD.approvedInstallment.ToString());
                        }

                        i++;
                    }
                    grdLoanDetail.LoadFromDataSource();
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }
        private void GetLoanHistory(int intEmpID)
        {
            try
            {
                var Data = (from n in dbHrPayroll.TrnsLoanDetail
                            join e in dbHrPayroll.TrnsLoan on n.LnAID equals e.ID
                            where e.EmpID == intEmpID
                            select new
                            {
                                LId = e.DocNum,
                                LoanID = e.ID,
                                LoanType = n.MstLoans.Description,
                                RequestedAmmount = n.RequestedAmount,
                                ApprovedAmmount = n.ApprovedAmount,

                                RequiredDate = n.RequiredDate,
                                EmpID = e.EmpID,
                                //Installment = n.Installments,
                                Installment = n.Installments.GetValueOrDefault(),
                                //approvedInstallment=n.ApprovedInstallment,
                                approvedInstallment = n.ApprovedInstallment.GetValueOrDefault(),
                                ApprovalStatus = n.TrnsLoan.DocAprStatus,
                                RecAmount = n.RecoveredAmount.GetValueOrDefault(),
                                //RecAmount =n.RecoveredAmount,
                                Stop = n.FlgStopRecovery
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
                    decimal RemaingAmount = 0;
                    decimal approvedInstallment = 0;

                    dtLoanRequest.Rows.Clear();
                    dtLoanRequest.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        var DueAmount = dbHrPayroll.TrnsLoanRegister.Where(LR => LR.LoanID == WD.LId).FirstOrDefault();
                        var ApprovalStatusDetail = dbHrPayroll.MstLOVE.Where(LV => LV.Code == WD.ApprovalStatus).FirstOrDefault();
                        dtLoanRequest.SetValue("No", i, i + 1);
                        dtLoanRequest.SetValue("DocID", i, WD.LoanID);
                        dtLoanRequest.SetValue("DocNo", i, WD.LId);
                        dtLoanRequest.SetValue("LoanType", i, WD.LoanType);
                        dtLoanRequest.SetValue("Date", i, WD.RequiredDate);
                        if (WD.Stop.Value)
                        {
                            dtLoanRequest.SetValue("Stop", i, "Y");
                        }
                        else
                        {
                            dtLoanRequest.SetValue("Stop", i, "N");
                        }
                        dtLoanRequest.SetValue("ReqAmt", i, WD.RequestedAmmount.ToString());
                        dtLoanRequest.SetValue("Amount", i, WD.ApprovedAmmount.ToString());
                        if (ApprovalStatusDetail != null)
                        {
                            dtLoanRequest.SetValue("Status", i, ApprovalStatusDetail.Value);
                        }
                        if (DueAmount != null)
                        {
                            //dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", DueAmount.RecoveredAmount));
                            dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", DueAmount.RecoveredAmount.GetValueOrDefault()));

                            RemaingAmount = WD.ApprovedAmmount.Value - DueAmount.RecoveredAmount.Value;
                        }
                        else
                        {
                            dtLoanRequest.SetValue("RecToDate", i, "0");
                        }

                        dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", WD.RecAmount));
                        RemaingAmount = WD.ApprovedAmmount.Value - WD.RecAmount;

                        dtLoanRequest.SetValue("RemToDate", i, RemaingAmount.ToString());

                        approvedInstallment = Convert.ToDecimal(WD.approvedInstallment);

                        if (approvedInstallment < 1)
                        {
                            dtLoanRequest.SetValue("Installment", i, WD.Installment.ToString());
                        }
                        else
                        {
                            dtLoanRequest.SetValue("Installment", i, WD.approvedInstallment.ToString());
                        }

                        i++;
                    }
                    grdLoanDetail.LoadFromDataSource();
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }
        
        
        private void UpdateLoanStatus()
        {
            string strDocNum = "", strDocID = "";
            string Intallmentamount = "";
            bool isStop = false;
            try
            {
                for (int i = 1; i < grdLoanDetail.RowCount + 1; i++)
                {
                    strDocNum = (grdLoanDetail.Columns.Item("docNum").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strDocID = (grdLoanDetail.Columns.Item("clID").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Intallmentamount = (grdLoanDetail.Columns.Item("cl_Inst").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Intallmentamount = string.Format("{0:0.00}", Intallmentamount);
                    isStop = (grdLoanDetail.Columns.Item("cl_Stop").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    var LoanRecord = dbHrPayroll.TrnsLoan.Where(a => a.ID == Convert.ToInt32(strDocID)).FirstOrDefault();
                    if (LoanRecord != null)
                    {
                        var loanrecorddetail = dbHrPayroll.TrnsLoanDetail.Where(d => d.LnAID == LoanRecord.ID).FirstOrDefault();
                        if (loanrecorddetail != null)
                        {
                            if (isStop)
                            {
                                loanrecorddetail.FlgStopRecovery = true;
                            }
                            else
                            {
                                loanrecorddetail.FlgStopRecovery = false;
                            }
                            loanrecorddetail.ApprovedInstallment = Convert.ToDecimal(Intallmentamount);
                            loanrecorddetail.Installments = Convert.ToDecimal(Intallmentamount);
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                }                
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: UpdateLoanStatus Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
        
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txEmpCode.Value = Program.EmpID;
                    LoadSelectedData(txEmpCode.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

    }
}
