using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Collections;

using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_LoanRequest : HRMSBaseForm
    {
        #region "Variable"

        SAPbouiCOM.Button btSave, btCancel, btnStop, btPay;
        SAPbouiCOM.EditText txReqBy, txtEmployeeCode, txtPE, txtPaidA, txdocNum, txManager, txdoj, txdesig, tbSalary, tbOriginator, txtRequestedAmount, txtIntallmentAmount, txtRequestedDate, txtdocStatus, txtappStatus, txtApprovedAmount;
        SAPbouiCOM.ComboBox cmbLoanType, cbPT;
        SAPbouiCOM.CheckBox flgStop;
        SAPbouiCOM.DataTable dtLoanRequest;
        SAPbouiCOM.Matrix grdLoanDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, loanType, Amount, RecToDate, RemToDate, Installment;
        private Int32 CurrentRecord = 0, TotalRecords = 0;
        IEnumerable<TrnsLoan> oDocuments = null;
        SAPbouiCOM.Button btId;
        SAPbouiCOM.Item IbtPay, ItxtPaidA, IcbPT, IbtSave, Icb_LnTyp;
        IEnumerable<MstEmployee> oEmployees = null;

        bool flgNegativeLoan = false;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
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
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                            AddRecord();
                        break;
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "btnStop":
                        UpdateLoanStatus();
                        break;
                    case "btPay":
                        PostPayment();
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

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (!ValidateRecord())
                    {
                        BubbleEvent = false;
                    }
                    break;
            }
        }

        public override void etAfterCfl(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCfl(ref pVal, ref BubbleEvent);
            base.etAfterCfl(ref pVal, ref BubbleEvent);

            string itemId = pVal.ItemUID;
            SAPbouiCOM.ChooseFromListEvent ocfl = (SAPbouiCOM.ChooseFromListEvent)pVal;
            SAPbouiCOM.Item cflItem = oForm.Items.Item(itemId);
            SAPbouiCOM.DataTable oDT = ocfl.SelectedObjects;

            if (cflItem.Type.ToString() == "it_EDIT")
            {
                SAPbouiCOM.EditText txt = oForm.Items.Item(itemId).Specific;
                oForm.DataSources.UserDataSources.Item(itemId).ValueEx = oDT.GetValue("AcctCode", 0);
            }
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            GetNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            GetPreviosRecord();
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            LoadToNewRecord();
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmployeeCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmployeeCode.Value)
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
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
        }
        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                if (ACHR.Properties.Settings.Default.LoanNegative == "Yes")
                {
                    flgNegativeLoan = true;
                }
                else
                {
                    flgNegativeLoan = false;
                }
                btSave = oForm.Items.Item("1").Specific;
                IbtSave = oForm.Items.Item("1");
                btCancel = oForm.Items.Item("2").Specific;
                btnStop = oForm.Items.Item("btnStop").Specific;
                btId = oForm.Items.Item("btId").Specific;
                IbtPay = oForm.Items.Item("btPay");
                IbtPay.Enabled = false;

                oForm.DataSources.UserDataSources.Add("txtPaidA", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
                txtPaidA = oForm.Items.Item("txtPaidA").Specific;
                ItxtPaidA = oForm.Items.Item("txtPaidA");
                txtPaidA.DataBind.SetBound(true, "", "txtPaidA");
                ItxtPaidA.Enabled = false;

                txtPE = oForm.Items.Item("txtPE").Specific;
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("tbRby", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80);
                txReqBy = oForm.Items.Item("tbRby").Specific;
                txReqBy.DataBind.SetBound(true, "", "tbRby");

                txtEmployeeCode = oForm.Items.Item("tbEmpC").Specific;
                oForm.DataSources.UserDataSources.Add("tbEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmployeeCode.DataBind.SetBound(true, "", "tbEmpC");

                cmbLoanType = oForm.Items.Item("cn_LnTyp").Specific;
                oForm.DataSources.UserDataSources.Add("cn_LnTyp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbLoanType.DataBind.SetBound(true, "", "cn_LnTyp");
                Icb_LnTyp = oForm.Items.Item("cn_LnTyp");

                txtdocStatus = oForm.Items.Item("txtdstat").Specific;
                oForm.DataSources.UserDataSources.Add("txtdstat", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtdocStatus.DataBind.SetBound(true, "", "txtdstat");

                txtappStatus = oForm.Items.Item("txtappst").Specific;
                oForm.DataSources.UserDataSources.Add("txtappst", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtappStatus.DataBind.SetBound(true, "", "txtappst");


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
                tbSalary = oForm.Items.Item("tbSalry").Specific;
                tbSalary.DataBind.SetBound(true, "", "tbSalry");

                oForm.DataSources.UserDataSources.Add("tbOrig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                tbOriginator = oForm.Items.Item("tbOrig").Specific;
                tbOriginator.DataBind.SetBound(true, "", "tbOrig");

                oForm.DataSources.UserDataSources.Add("txtReAmnt", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtRequestedAmount = oForm.Items.Item("txtReAmnt").Specific;
                txtRequestedAmount.DataBind.SetBound(true, "", "txtReAmnt");

                oForm.DataSources.UserDataSources.Add("txtInstall", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtIntallmentAmount = oForm.Items.Item("txtInstall").Specific;
                txtIntallmentAmount.DataBind.SetBound(true, "", "txtInstall");

                oForm.DataSources.UserDataSources.Add("txtReqDt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtRequestedDate = oForm.Items.Item("txtReqDt").Specific;
                txtRequestedDate.DataBind.SetBound(true, "", "txtReqDt");

                oForm.DataSources.UserDataSources.Add("txtApAm", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtApprovedAmount = oForm.Items.Item("txtApAm").Specific;
                txtApprovedAmount.DataBind.SetBound(true, "", "txtApAm");

                //
                //oForm.DataSources.UserDataSources.Add("txtPaidA", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
                //txtPaidAccount = oForm.Items.Item("txtPaidA").Specific;
                //ItxtPaidAccount = oForm.Items.Item("txtPaidA");
                //txtPaidAccount.DataBind.SetBound(true, "", "txtPaidA");
                //ItxtPaidAccount.Enabled = false;

                flgStop = oForm.Items.Item("flgStop").Specific;
                oForm.DataSources.UserDataSources.Add("flgStop", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgStop.DataBind.SetBound(true, "", "flgStop");
                flgStop.Checked = false;

                cbPT = oForm.Items.Item("cbPT").Specific;
                oForm.DataSources.UserDataSources.Add("cbPT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPT.DataBind.SetBound(true, "", "cbPT");

                FillPTypeCombo();
                IcbPT = oForm.Items.Item("cbPT");

                InitiallizegridMatrix();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                GetDataFilterData();

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

        private void FillPTypeCombo()
        {
            try
            {
                cbPT.ValidValues.Add("-1", "[Select One]");
                cbPT.ValidValues.Add("1", "Bank");
                cbPT.ValidValues.Add("2", "Cash");
                cbPT.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillParentLoanTypeCombo()
        {
            try
            {
                cmbLoanType.ValidValues.Add("-1", "[Select One]");
                var Data = from v in dbHrPayroll.MstLoans where v.FlgActive == true select v;
                foreach (var v in Data)
                {
                    cmbLoanType.ValidValues.Add(v.Id.ToString(), v.Description);
                }
                cmbLoanType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public int GetNextDocnum()
        {
            try
            {
                //int MaxDocnum = Convert.ToInt32(dbHrPayroll.TrnsLoan.Max(x => x.DocNum));
                int MaxDocnum = Convert.ToInt32(dbHrPayroll.TrnsLoan.Max(x => x.ID));
                return MaxDocnum + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private void LoadSelectedData(String pCode)
        {
            try
            {
                string strDocStatus = "LV0001", strApprovalStatus = "LV0005";
                decimal decApprovedAmount = 0;
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == pCode
                                  select a).FirstOrDefault();
                    var GetUser = getEmp.MstUsers.FirstOrDefault();

                    if (getEmp != null)
                    {
                        txdocNum.Value = Convert.ToString(GetNextDocnum());
                        if (GetUser != null)
                        {
                            tbOriginator.Value = GetUser.UserID;
                        }
                        txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        txManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                        txdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                        txdesig.Value = getEmp.DesignationName;

                        tbSalary.Value = String.Format("{0:0.00}", ds.getEmpGross(getEmp));

                        //tbSalary.Value = getEmp.BasicSalary != null ? String.Format("{0:0.00}", getEmp.BasicSalary) : "";
                        txtdocStatus.Value = dbHrPayroll.MstLOVE.Where(lv => lv.Code == strDocStatus).Single().Value;
                        txtappStatus.Value = dbHrPayroll.MstLOVE.Where(lv => lv.Code == strApprovalStatus).Single().Value;
                        txtApprovedAmount.Value = Convert.ToString(decApprovedAmount);
                        //cb_LnTyp.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        GetLoanHistory(getEmp.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanRequest Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                            && n.RecoveredAmount > 0
                            select new
                            {
                                LId = e.ID,
                                LoanID = n.MstLoans.Id,
                                LoanType = n.MstLoans.Description,
                                ApprovedAmmount = n.RequestedAmount,
                                EmpID = e.EmpID,
                                ///DueAmount=dbHrPayroll.TrnsLoanRegister.Where(LR=>LR.LoanID==LoanID)
                                Installment = n.Installments,
                                //RecAmount = n.RecoveredAmount
                                RecAmount = n.RecoveredAmount.GetValueOrDefault()
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

                    dtLoanRequest.Rows.Clear();
                    dtLoanRequest.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        var DueAmount = dbHrPayroll.TrnsLoanRegister.Where(LR => LR.LoanID == WD.LId).FirstOrDefault();
                        dtLoanRequest.SetValue("No", i, i + 1);
                        dtLoanRequest.SetValue("LoanType", i, WD.LoanType);
                        dtLoanRequest.SetValue("Amount", i, WD.ApprovedAmmount.ToString());
                        if (DueAmount != null)
                        {
                            dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", DueAmount.RecoveredAmount));
                            RemaingAmount = WD.ApprovedAmmount.Value - DueAmount.RecoveredAmount.Value;
                        }
                        else
                        {
                            dtLoanRequest.SetValue("RecToDate", i, "0");
                        }

                        dtLoanRequest.SetValue("RecToDate", i, string.Format("{0:0.00}", WD.RecAmount));
                        RemaingAmount = WD.ApprovedAmmount.Value - WD.RecAmount;


                        dtLoanRequest.SetValue("RemToDate", i, RemaingAmount.ToString());
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

        private void AddRecord()
        {
            try
            {
                String pCode = txtEmployeeCode.Value;

                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                var getUser = getEmp.MstUsers.FirstOrDefault();
                if (string.IsNullOrEmpty(cmbLoanType.Value))
                {
                    oApplication.StatusBar.SetText("Please select Loan Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cmbLoanType.Value) && Convert.ToInt32(cmbLoanType.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Loan Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtRequestedDate.Value))
                {
                    oApplication.StatusBar.SetText("Please Enter Required Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (getEmp != null && !string.IsNullOrEmpty(txtRequestedDate.Value) && !string.IsNullOrEmpty(txtRequestedAmount.Value))
                {
                    TrnsLoan oNewLoan = new TrnsLoan();
                    TrnsLoanDetail oChild = new TrnsLoanDetail();

                    //oNewLoan.DocNum = Convert.ToInt32(txdocNum.Value);
                    oNewLoan.Series = -1;
                    oNewLoan.EmpID = getEmp.ID;
                    oNewLoan.EmpName = txReqBy.Value;
                    if (!string.IsNullOrEmpty(txManager.Value))
                    {
                        oNewLoan.ManagerID = getEmp.Manager;
                        oNewLoan.ManagerName = txManager.Value;
                    }
                    if (!string.IsNullOrEmpty(txdoj.Value))
                    {
                        oNewLoan.DateOfJoining = DateTime.ParseExact(txdoj.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    oNewLoan.UserId = oCompany.UserName;
                    oNewLoan.DesignationID = getEmp.DesignationID;
                    oNewLoan.Designation = txdesig.Value;
                    if (string.IsNullOrEmpty(tbSalary.Value))
                    {
                        oApplication.StatusBar.SetText("Employee Salary Field Can't be Empty", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    oNewLoan.Salary = Convert.ToDecimal(tbSalary.Value);
                    if (getUser != null)
                    {
                        oNewLoan.OriginatorID = getEmp.ID;
                        oNewLoan.OriginatorName = getUser.UserID;
                    }
                    else
                    {
                        oNewLoan.OriginatorID = getEmp.ID;
                        oNewLoan.OriginatorName = txReqBy.Value;
                    }
                    oNewLoan.CreateDate = DateTime.Now;
                    // Inserting Child Record
                    oChild.LoanType = Convert.ToInt32(cmbLoanType.Value);
                    oChild.RequestedAmount = Convert.ToDecimal(txtRequestedAmount.Value);
                    oChild.Installments = Convert.ToDecimal(txtIntallmentAmount.Value);
                    oChild.FlgActive = true;
                    oChild.FlgStopRecovery = false;
                    oChild.ApprovedAmount = 0;
                    oChild.ApprovedInstallment = 0;
                    oChild.RequiredDate = DateTime.ParseExact(txtRequestedDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oChild.MaturityDate = DateTime.ParseExact(txtRequestedDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oChild.CreateDate = DateTime.Now;
                    oChild.UserID = oCompany.UserName;
                    dbHrPayroll.TrnsLoan.InsertOnSubmit(oNewLoan);
                    oNewLoan.TrnsLoanDetail.Add(oChild);

                    dbHrPayroll.SubmitChanges();
                    oNewLoan.DocNum = oNewLoan.ID;
                    dbHrPayroll.SubmitChanges();
                    ClearControls();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    GetDataFilterData();
                }
                else
                {
                    oApplication.SetStatusBarMessage("Required Field(s) Missing", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LoanRequest Function: InsertLoanRequest Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private Boolean ValidateRecord()
        {
            try
            {
                if (string.IsNullOrEmpty(txtEmployeeCode.Value))
                {
                    oApplication.StatusBar.SetText("Employee Code field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtRequestedDate.Value))
                {
                    oApplication.StatusBar.SetText("Requested date field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtRequestedAmount.Value))
                {
                    oApplication.StatusBar.SetText("Requested Amount field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                else
                {
                    if (!flgNegativeLoan)
                    {
                        decimal valuecheck = Convert.ToDecimal(txtRequestedAmount.Value);
                        if (valuecheck <= 0)
                        {
                            oApplication.StatusBar.SetText("Requested Amount can't be zero or negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(txtIntallmentAmount.Value))
                {
                    oApplication.StatusBar.SetText("Installment Amount field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                else
                {
                    if (!flgNegativeLoan)
                    {
                        decimal valuecheck = Convert.ToDecimal(txtIntallmentAmount.Value);
                        if (valuecheck <= 0)
                        {
                            oApplication.StatusBar.SetText("Installment Amount can't be zero or negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(cmbLoanType.Value) || cmbLoanType.Value.Trim() == "-1")
                {
                    oApplication.StatusBar.SetText("Loan Type is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                var oEmp = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmployeeCode.Value).FirstOrDefault();
                
                if (!flgNegativeLoan)
                {
                    decimal RequestedAmount = Convert.ToDecimal(txtRequestedAmount.Value);
                    decimal Salary = Convert.ToDecimal(tbSalary.Value);
                    decimal Install = Convert.ToDecimal(txtIntallmentAmount.Value);
                    if (!string.IsNullOrEmpty(oEmp.EmployeeContractType) && oEmp.EmployeeContractType != "DWGS")
                    {
                        if (Install > Salary)
                        {
                            oApplication.StatusBar.SetText("Installent amount can't be greater than Salary", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (Install > RequestedAmount)
                        {
                            oApplication.StatusBar.SetText("Installent amount can't be greater than Requested Amount", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                    else
                    {
                        if (Install > RequestedAmount)
                        {
                            oApplication.StatusBar.SetText("Installent amount can't be greater than Requested Amount", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                    
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GetNextRecord()
        {
            var LoanRecords = dbHrPayroll.TrnsLoan.ToList();
            if (LoanRecords != null && LoanRecords.Count > 0)
            {
                TotalRecords = LoanRecords.Count;
                if (CurrentRecord + 1 >= TotalRecords)
                {
                    CurrentRecord = 0;
                }
                else
                {
                    CurrentRecord++;
                }
                FillDocument(CurrentRecord);
            }
            else
            {
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("NoRecordFound"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetPreviosRecord()
        {
            var LoanRecords = dbHrPayroll.TrnsLoan.ToList();
            if (LoanRecords != null && LoanRecords.Count > 0)
            {
                TotalRecords = LoanRecords.Count;
                if (CurrentRecord - 1 < 0)
                {
                    CurrentRecord = TotalRecords - 1;
                }
                else
                {
                    CurrentRecord--;
                }
                FillDocument(CurrentRecord);
            }
            else
            {
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("NoRecordFound"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDocument(Int32 DocumentID)
        {
            try
            {

                oDocuments = dbHrPayroll.TrnsLoan.ToList();
                TrnsLoan oDoc = oDocuments.ElementAt<TrnsLoan>(DocumentID);
                if (!String.IsNullOrEmpty(oDoc.EmpName))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.ID == oDoc.EmpID
                                  select a).FirstOrDefault();
                    var GetUser = getEmp.MstUsers.FirstOrDefault();
                    tbOriginator.Value = GetUser.UserID;
                    txtEmployeeCode.Value = getEmp.EmpID;
                    txReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                    txManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                    txdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                    txdesig.Value = getEmp.DesignationName;
                    //tbSalary.Value = String.Format("{0:0.00}",  getEmp.BasicSalary);
                    tbSalary.Value = String.Format("{0:0.00}", ds.getEmpGross(getEmp));
                    txtPE.Value = Convert.ToString(oDoc.TransID);
                }
                txdocNum.Value = Convert.ToString(oDoc.DocNum);
                if (Convert.ToDecimal(oDoc.TrnsLoanDetail[0].RecoveredAmount) > 0)
                {
                    txtdocStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == oDoc.DocStatus).Single().Value;
                }
                else
                {
                    txtdocStatus.Value = "Closed";
                }
                txtappStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == oDoc.DocAprStatus).Single().Value;
                txtRequestedAmount.Value = Convert.ToString(oDoc.TrnsLoanDetail.FirstOrDefault().RequestedAmount);
                txtIntallmentAmount.Value = Convert.ToString(oDoc.TrnsLoanDetail.FirstOrDefault().Installments);
                txtRequestedDate.Value = Convert.ToDateTime(oDoc.TrnsLoanDetail.FirstOrDefault().RequiredDate).ToString("yyyyMMdd");
                txtApprovedAmount.Value = Convert.ToString(oDoc.TrnsLoanDetail.FirstOrDefault().ApprovedAmount);
                if (oDoc.TrnsLoanDetail.FirstOrDefault().FlgStopRecovery != null && oDoc.TrnsLoanDetail.FirstOrDefault().FlgStopRecovery == true)
                {
                    flgStop.Checked = true;
                }
                else
                {
                    flgStop.Checked = false;
                }
                var LoanTypeDetailType = dbHrPayroll.MstLoans.Where(l => l.Id == oDoc.TrnsLoanDetail.Single().LoanType).FirstOrDefault();
                if (LoanTypeDetailType != null)
                {
                    cmbLoanType.Select(LoanTypeDetailType.Id.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cmbLoanType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                if (!string.IsNullOrEmpty(txtappStatus.Value))
                {
                    if (txtappStatus.Value.Trim() == "Approved")
                    {
                        if (string.IsNullOrEmpty(txtPE.Value))
                        {
                            IbtPay.Enabled = true;
                            ItxtPaidA.Enabled = true;
                        }
                        else
                        {
                            IbtPay.Enabled = false;
                            ItxtPaidA.Enabled = false;
                            IbtSave.Enabled = false;
                        }
                    }
                }
                else
                {
                    IbtPay.Enabled = false;
                    ItxtPaidA.Enabled = false;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void ClearControls()
        {
            try
            {
                GetDataFilterData();
                txtEmployeeCode.Value = string.Empty;
                txReqBy.Value = string.Empty;
                txdocNum.Value = string.Empty;
                txManager.Value = string.Empty;
                txdoj.Value = string.Empty;
                txdesig.Value = string.Empty;
                tbSalary.Value = string.Empty;
                tbOriginator.Value = string.Empty;
                txtRequestedAmount.Value = string.Empty;
                txtRequestedDate.Value = string.Empty;
                txtdocStatus.Value = string.Empty;
                txtappStatus.Value = string.Empty;
                txtIntallmentAmount.Value = string.Empty;
                txtApprovedAmount.Value = string.Empty;
                cmbLoanType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtLoanRequest.Rows.Clear();
                grdLoanDetail.LoadFromDataSource();

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                txtEmployeeCode.Value = st.Rows[0][0].ToString();
                LoadSelectedData(txtEmployeeCode.Value);
            }
        }

        private void UpdateLoanStatus()
        {
            try
            {
                if (string.IsNullOrEmpty(txdocNum.Value))
                {
                    oApplication.StatusBar.SetText("Please select Loan Record First", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                var LoanRecord = dbHrPayroll.TrnsLoan.Where(a => a.DocNum == Convert.ToInt32(txdocNum.Value)).FirstOrDefault();
                var loanDetail = LoanRecord.TrnsLoanDetail.FirstOrDefault();
                if (LoanRecord != null && loanDetail != null)
                {
                    if (flgStop.Checked)
                    {
                        loanDetail.FlgStopRecovery = true;
                    }
                    else
                    {
                        loanDetail.FlgStopRecovery = false;
                    }
                    dbHrPayroll.SubmitChanges();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    //oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: UpdateLoanStatus Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("empid", txtEmployeeCode.Value.ToString());
            // SearchKeyVal.Add("FirstName + ' ' + LastName", txName.Value.ToString());
        }

        private void LoadToNewRecord()
        {
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            ClearControls();
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
                    txtEmployeeCode.Value = Program.EmpID;
                    LoadSelectedData(txtEmployeeCode.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void PostPaymentBank()
        {
            try
            {
                Hashtable elementGls = new Hashtable();
                string strDebitAccount = "";
                string strCreditAccount = "";

                String pCode = txtEmployeeCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                var LoanReq = dbHrPayroll.TrnsLoan.Where(ad => ad.DocNum == Convert.ToInt32(txdocNum.Value)).FirstOrDefault();
                TrnsLoanDetail recLoan = LoanReq.TrnsLoanDetail.FirstOrDefault();
                MstLoans loan = recLoan.MstLoans; //(from p in dbHrPayroll.MstLoans where p.Id.ToString() == recLoan.MstLoans.ToString() select p).Single();
                if (loan != null)
                {
                    try
                    {
                        elementGls = getLoanGL(getEmp, loan);
                        strDebitAccount = elementGls["DrAcct"].ToString();
                        strCreditAccount = elementGls["CrAcct"].ToString();
                        if (string.IsNullOrEmpty(strDebitAccount) || string.IsNullOrEmpty(strCreditAccount))
                        {
                            oApplication.StatusBar.SetText("Employee's Department/Location GL not define.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    catch
                    {
                        oApplication.StatusBar.SetText("Employee's Department/Location GL not define.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                if (LoanReq == null)
                {
                    oApplication.StatusBar.SetText("Loan Request Not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (LoanReq != null)
                {
                    bool IsPaid = LoanReq.FlgPaid == null ? false : LoanReq.FlgPaid.Value;
                    if (IsPaid)
                    {
                        oApplication.StatusBar.SetText("Loan Already paid", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cmbLoanType.Value))
                {
                    oApplication.StatusBar.SetText("Please select Loan Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cmbLoanType.Value) && Convert.ToInt32(cmbLoanType.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Loan Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cbPT.Value))
                {
                    oApplication.StatusBar.SetText("Please select Payment Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cbPT.Value) && Convert.ToInt32(cbPT.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Payment Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtPaidA.Value))
                {
                    oApplication.StatusBar.SetText("Please select Account Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }


                SAPbobsCOM.Payments oPays = default(SAPbobsCOM.Payments);
                oPays = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                oPays.AccountPayments.AccountCode = strCreditAccount; //txtGL.Value.Trim();
                oPays.AccountPayments.Decription = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;// "HRMS Payroll Manual";
                oPays.AccountPayments.SumPaid = Convert.ToDouble(txtApprovedAmount.Value);
                if (Convert.ToBoolean(Program.systemInfo.FlgBranches))
                {
                    //Add Here Branch COndition
                    String BBValue = getEmp.BranchName;
                    //String strQuery = "SELECT dbo.OBPL.BPLId As BPLId FROM dbo.OBPL WHERE dbo.OBPL.BPLName = '" + BBValue + "'";
                    String strQuery = "SELECT T0.\"BPLId\" FROM OBPL T0 WHERE T0.\"BPLName\" = '" + BBValue + "'";
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    oRecSet.DoQuery(strQuery);
                    string outStr = string.Empty;
                    string BranchIDFromSAP = string.Empty;
                    if (oRecSet.EoF)
                    {
                        outStr = "Error : BranchName unable to retrive.";
                    }
                    BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);

                    oPays.BPLID = Convert.ToInt32(BranchIDFromSAP);
                }

                //oPays.AccountPayments.UserFields.Fields.Item("U_EmpID").Value = "1";

                oPays.AccountPayments.UserFields.Fields.Item("U_DocNumber").Value = Convert.ToString(LoanReq.ID);
                oPays.AccountPayments.UserFields.Fields.Item("U_EmpID").Value = getEmp.EmpID;
                oPays.AccountPayments.UserFields.Fields.Item("U_Type").Value = "Loan";
                string strInstallment = txtIntallmentAmount.Value;
                strInstallment = string.Format("{0:0.0}", strInstallment);
                oPays.AccountPayments.UserFields.Fields.Item("U_Installment").Value = strInstallment;

                //oPays.AccountPayments.UserFields.Fields.Item("U_Installment").Value = string.Format("{0:0.00}", txtInstall.Value);// Convert.ToString(txtInstall.Value);

                oPays.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                oPays.DocType = SAPbobsCOM.BoRcptTypes.rAccount;
                oPays.DocTypte = SAPbobsCOM.BoRcptTypes.rAccount;
                oPays.DocDate = DateTime.Now;
                oPays.JournalRemarks = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;// "pag efect";
                oPays.TaxDate = DateTime.Now;
                oPays.TransferAccount = txtPaidA.Value.Trim(); //"_SYS00000000003"; //"_SYS00000000003";
                oPays.TransferDate = DateTime.Now;
                oPays.TransferReference = "Payroll Entry"; //getEmp.EmpID;// +" : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;// "ref01";
                oPays.TransferSum = Convert.ToDouble(txtApprovedAmount.Value);
                oPays.ApplyVAT = SAPbobsCOM.BoYesNoEnum.tNO;
                oPays.AccountPayments.Add();
                int paidDoc = oPays.Add();
                if (paidDoc != 0)
                {
                    int erroCode = 0;
                    string errDescr = "";
                    oCompany.GetLastError(out erroCode, out errDescr);
                    //dtError.Rows.Add(DateTime.Now.ToString(), "Not posted Error :" + errDescr);
                    oApplication.StatusBar.SetText("SAP B1 error : " + errDescr, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    // outStr = "SBO Error:" + errDescr;
                }
                else
                {
                    string strPENumber = Convert.ToString(oCompany.GetNewObjectKey());
                    txtPE.Value = strPENumber;
                    var LoanRequest = dbHrPayroll.TrnsLoan.Where(a => a.DocNum == Convert.ToInt32(txdocNum.Value)).FirstOrDefault();
                    if (LoanRequest != null)
                    {
                        LoanRequest.TransID = Convert.ToInt32(strPENumber);
                        LoanRequest.FlgPaid = true;
                        dbHrPayroll.SubmitChanges();
                    }
                    //outStr = Convert.ToString(oCompany.GetNewObjectKey());
                    oApplication.StatusBar.SetText("Payment has been Made Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in Posting Payment.");
            }
        }

        private void PostPaymentCASH()
        {
            try
            {
                Hashtable elementGls = new Hashtable();
                string strDebitAccount = "";
                string strCreditAccount = "";

                String pCode = txtEmployeeCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                var LoanReq = dbHrPayroll.TrnsLoan.Where(ad => ad.DocNum == Convert.ToInt32(txdocNum.Value)).FirstOrDefault();
                TrnsLoanDetail recLoan = LoanReq.TrnsLoanDetail.FirstOrDefault();
                MstLoans loan = recLoan.MstLoans;
                if (loan != null)
                {
                    try
                    {
                        elementGls = getLoanGL(getEmp, loan);
                        strDebitAccount = elementGls["DrAcct"].ToString();
                        strCreditAccount = elementGls["CrAcct"].ToString();
                        if (string.IsNullOrEmpty(strDebitAccount) || string.IsNullOrEmpty(strCreditAccount))
                        {
                            oApplication.StatusBar.SetText("Employee's Department/Location GL not define.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    catch
                    {
                        oApplication.StatusBar.SetText("Employee's Department/Location GL not define.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                if (LoanReq == null)
                {
                    oApplication.StatusBar.SetText("Loan Request Not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (LoanReq != null)
                {
                    bool IsPaid = LoanReq.FlgPaid == null ? false : LoanReq.FlgPaid.Value;
                    if (IsPaid)
                    {
                        oApplication.StatusBar.SetText("Loan Already paid", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cmbLoanType.Value))
                {
                    oApplication.StatusBar.SetText("Please select Loan Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cmbLoanType.Value) && Convert.ToInt32(cmbLoanType.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Loan Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cbPT.Value))
                {
                    oApplication.StatusBar.SetText("Please select Payment Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cbPT.Value) && Convert.ToInt32(cbPT.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Payment Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtPaidA.Value))
                {
                    oApplication.StatusBar.SetText("Please select Account Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }


                SAPbobsCOM.Payments oPays = default(SAPbobsCOM.Payments);
                oPays = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                oPays.AccountPayments.AccountCode = strCreditAccount; //txtGL.Value.Trim();
                oPays.AccountPayments.Decription = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;// "HRMS Payroll Manual";
                oPays.AccountPayments.SumPaid = Convert.ToDouble(txtApprovedAmount.Value);
                if (Convert.ToBoolean(Program.systemInfo.FlgBranches))
                {
                    //Add Here Branch COndition
                    String BBValue = getEmp.BranchName;
                    //String strQuery = "SELECT dbo.OBPL.BPLId As BPLId FROM dbo.OBPL WHERE dbo.OBPL.BPLName = '" + BBValue + "'";
                    String strQuery = "SELECT T0.\"BPLId\" FROM OBPL T0 WHERE T0.\"BPLName\" = '" + BBValue + "'";
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    oRecSet.DoQuery(strQuery);
                    string outStr = string.Empty;
                    string BranchIDFromSAP = string.Empty;
                    if (oRecSet.EoF)
                    {
                        outStr = "Error : BranchName unable to retrive.";
                    }
                    BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);

                    oPays.BPLID = Convert.ToInt32(BranchIDFromSAP);
                }

                oPays.AccountPayments.UserFields.Fields.Item("U_DocNumber").Value = Convert.ToString(LoanReq.ID);
                oPays.AccountPayments.UserFields.Fields.Item("U_EmpID").Value = getEmp.EmpID;
                oPays.AccountPayments.UserFields.Fields.Item("U_Type").Value = "Loan";
                string strInstallment = txtIntallmentAmount.Value;
                strInstallment = string.Format("{0:0.0}", strInstallment);
                oPays.AccountPayments.UserFields.Fields.Item("U_Installment").Value = strInstallment;

                oPays.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                oPays.DocType = SAPbobsCOM.BoRcptTypes.rAccount;
                oPays.DocTypte = SAPbobsCOM.BoRcptTypes.rAccount;
                oPays.DocDate = DateTime.Now;
                oPays.JournalRemarks = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;// "pag efect";
                oPays.TaxDate = DateTime.Now;

                //oPays.TransferAccount = txtPaidA.Value.Trim(); //"_SYS00000000003"; //"_SYS00000000003";
                //oPays.TransferDate = DateTime.Now;
                //oPays.TransferReference = "ref01";
                //oPays.TransferSum = Convert.ToDouble(txtAprAmount.Value);


                oPays.CashAccount = txtPaidA.Value.Trim();
                oPays.CashSum = Convert.ToDouble(txtApprovedAmount.Value);

                oPays.ApplyVAT = SAPbobsCOM.BoYesNoEnum.tNO;
                oPays.AccountPayments.Add();
                int paidDoc = oPays.Add();
                if (paidDoc != 0)
                {
                    int erroCode = 0;
                    string errDescr = "";
                    oCompany.GetLastError(out erroCode, out errDescr);
                    //dtError.Rows.Add(DateTime.Now.ToString(), "Not posted Error :" + errDescr);
                    oApplication.StatusBar.SetText(errDescr, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    // outStr = "SBO Error:" + errDescr;
                }
                else
                {
                    string strPENumber = Convert.ToString(oCompany.GetNewObjectKey());
                    txtPE.Value = strPENumber;
                    var LoanRequest = dbHrPayroll.TrnsLoan.Where(a => a.DocNum == Convert.ToInt32(txdocNum.Value)).FirstOrDefault();
                    if (LoanRequest != null)
                    {
                        LoanRequest.TransID = Convert.ToInt32(strPENumber);
                        LoanRequest.FlgPaid = true;
                        dbHrPayroll.SubmitChanges();
                    }
                    //outStr = Convert.ToString(oCompany.GetNewObjectKey());
                    oApplication.StatusBar.SetText("Payment has been Made Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in Posting Payment " + ex.Message);
            }
        }

        public void PostPayment()
        {
            try
            {

                int confirm = oApplication.MessageBox("outgoing payment is irr-reversable. Are you sure you want to post Payment? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                string strPostPaymentType = cbPT.Value.Trim();
                if (!string.IsNullOrEmpty(strPostPaymentType))
                {
                    switch (strPostPaymentType)
                    {
                        case "1":
                            PostPaymentBank();
                            return;
                        case "2":
                            PostPaymentCASH();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Please select Payment Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in Posting Payment " + ex.Message);
            }
        }

        public Hashtable getAdvGL(MstEmployee emp, MstAdvance adv)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in dbHrPayroll.MstGLDAdvanceDetail where p.GLDId.ToString() == GlId.ToString() && p.AdvancesId.ToString() == adv.Id.ToString() select p).Count();
            if (cntGl > 0)
            {
                MstGLDAdvanceDetail glAdv = (from p in dbHrPayroll.MstGLDAdvanceDetail where p.GLDId.ToString() == GlId.ToString() && p.AdvancesId.ToString() == adv.Id.ToString() select p).Single();
                gls.Add("DrAcct", glAdv.CostAccount);
                gls.Add("CrAcct", glAdv.BalancingAccount);
                gls.Add("DrAcctName", glAdv.CostAcctDisplay);
                gls.Add("CrAcctName", glAdv.BalancingAcctDisplay);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
            }

            return gls;

        }

        public MstGLDetermination getEmpGl(MstEmployee emp)
        {
            MstGLDetermination detr = null;
            string GlType = emp.CfgPayrollDefination.GLType.ToString();

            try
            {

                if (GlType.Trim().ToUpper() == "LOC")
                {
                    detr = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "LOC" && p.GLValue == emp.Location select p).Single();
                }
                else if (GlType.Trim().ToUpper() == "DEPT")
                {
                    detr = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "DEPT" && p.GLValue == emp.DepartmentID select p).Single();
                }
                else if (GlType.Trim().ToUpper() == "COMP")
                {
                    detr = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "COMP" select p).Single();
                }
                else
                {
                    detr = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "COMP" select p).Single();
                }

            }
            catch (Exception ex)
            {

            }
            return detr;
        }

        public Hashtable getLoanGL(MstEmployee emp, MstLoans loan)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in dbHrPayroll.MstGLDLoansDetails where p.GLDId.ToString() == GlId.ToString() && p.LoanId.ToString() == loan.Id.ToString() select p).Count();
            if (cntGl > 0)
            {
                MstGLDLoansDetails glloan = (from p in dbHrPayroll.MstGLDLoansDetails where p.GLDId.ToString() == GlId.ToString() && p.LoanId.ToString() == loan.Id.ToString() select p).Single();
                gls.Add("DrAcct", glloan.CostAccount);
                gls.Add("CrAcct", glloan.BalancingAccount);
                gls.Add("DrAcctName", glloan.CostAcctDisplay);
                gls.Add("CrAcctName", glloan.BalancingAcctDisplay);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
            }

            return gls;

        }

        private void GetDataFilterData()
        {
            try
            {
                CodeIndex.Clear();
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {

                    string strOut = string.Empty;
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
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
