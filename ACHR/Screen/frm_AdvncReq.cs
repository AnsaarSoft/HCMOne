using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Data.SqlClient;
using SAPbobsCOM;
using System.Collections;

namespace ACHR.Screen
{
    class frm_AdvncReq : HRMSBaseForm
    {
        #region "Variables"


        SAPbouiCOM.Button btSave, btCancel, btPay;
        SAPbouiCOM.EditText txtReqBy, txtGL, txtPE, txtPaidAccount, txtEmpCode, txtdocNum, txtManager, txtdoj, txtdesig, txtSalary, txtOriginator, txtRequestedAmount, txtReqDt, txtdocStatus, txtappStatus, txtAprAmount, txtExpSalary, txtPreAd;
        private SAPbouiCOM.ComboBox cmbAdvanceType, cbPT;
        SAPbouiCOM.DataTable dtPrevAdvance;
        SAPbouiCOM.Matrix grdAdvncDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, AdvanceType, Amount, RecToDate, RemToDate, clDate;
        private Int32 CurrentRecord = 0, TotalRecords = 0;
        IEnumerable<TrnsAdvance> oDocuments = null;
        SAPbouiCOM.Button btId, btPrint;
        SAPbouiCOM.Item IbtPay, ItxtPaidAccount, IbtSave, IcbAdvT, IcbPT;
        SAPbouiCOM.CheckBox flgStop;
        SAPbouiCOM.PictureBox pctBox;
        System.Data.DataTable dtError = new System.Data.DataTable();
        Boolean flgManager = false, flgReportTo = false;
        IEnumerable<MstEmployee> oEmployees = null;
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

                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    case "btPrint":
                        printReport();
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
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
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
            if (txtEmpCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmpCode.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }


        }

        public override void etAfterKeyDown(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterKeyDown(ref pVal, ref BubbleEvent);
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
            {
                if (pVal.ItemUID == "txtEmpC")
                {
                    if (pVal.CharPressed == 9)
                    {
                        if (string.IsNullOrEmpty(txtEmpCode.Value))
                        {
                            OpenNewSearchForm();
                        }
                    }
                }
                //if (pVal.CharPressed == 13)
                //{
                //    AddRecord();
                //}
            }
        }

        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
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

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "txtReqDt")
            {
                if (string.IsNullOrEmpty(txtEmpCode.Value)) return;
                var oEmp = (from a in dbHrPayroll.MstEmployee
                            where a.EmpID == txtEmpCode.Value.Trim()
                            select a).FirstOrDefault();
                if (oEmp != null)
                {
                    if (Program.systemInfo.FlgRetailRules1 == false)
                    {
                        txtExpSalary.Value = string.Format("{0:0.00}", GetSalaryTillDate(oEmp));
                        SetEmployeeSalaryConditional(oEmp);
                    }
                    else
                    {
                        txtExpSalary.Value = string.Format("{0:0.00}", GetSalaryTillDateISM(oEmp));
                        SetEmployeeSalaryConditional(oEmp);
                    }

                }
            }
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            txtEmpCode.Value = "";
            OpenNewSearchForm();

        }

        #endregion

        #region "Functions"

        public void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;
                IbtSave = oForm.Items.Item("1");
                btCancel = oForm.Items.Item("2").Specific;
                btPrint = oForm.Items.Item("btPrint").Specific;

                btPay = oForm.Items.Item("btPay").Specific;

                IbtPay = oForm.Items.Item("btPay");
                IbtPay.Enabled = false;

                txtdocStatus = oForm.Items.Item("txtdstat").Specific;
                txtappStatus = oForm.Items.Item("txtappst").Specific;

                txtGL = oForm.Items.Item("txtGL").Specific;

                oForm.DataSources.UserDataSources.Add("txtPaidA", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
                txtPaidAccount = oForm.Items.Item("txtPaidA").Specific;
                ItxtPaidAccount = oForm.Items.Item("txtPaidA");
                txtPaidAccount.DataBind.SetBound(true, "", "txtPaidA");
                ItxtPaidAccount.Enabled = false;

                txtPE = oForm.Items.Item("txtPE").Specific;

                pctBox = oForm.Items.Item("picbox").Specific;


                //Initializing Textboxes
                txtReqBy = oForm.Items.Item("txtRby").Specific;
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;

                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;

                oForm.DataSources.UserDataSources.Add("txtDNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtdocNum = oForm.Items.Item("txtDNum").Specific;
                txtdocNum.DataBind.SetBound(true, "", "txtDNum");

                txtManager = oForm.Items.Item("txtManagr").Specific;

                oForm.DataSources.UserDataSources.Add("dtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtdoj = oForm.Items.Item("dtJoin").Specific;
                txtdoj.DataBind.SetBound(true, "", "dtJoin");

                txtdesig = oForm.Items.Item("txtDesig").Specific;
                txtSalary = oForm.Items.Item("txtSalry").Specific;
                txtExpSalary = oForm.Items.Item("txtExpS").Specific;
                txtOriginator = oForm.Items.Item("txtOrig").Specific;

                cmbAdvanceType = oForm.Items.Item("cbAdvT").Specific;
                oForm.DataSources.UserDataSources.Add("cbAdvT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbAdvanceType.DataBind.SetBound(true, "", "cbAdvT");
                IcbAdvT = oForm.Items.Item("cbAdvT");

                FillParentAdvnTypeCombo();

                cbPT = oForm.Items.Item("cbPT").Specific;
                oForm.DataSources.UserDataSources.Add("cbPT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPT.DataBind.SetBound(true, "", "cbPT");

                FillPTypeCombo();
                IcbPT = oForm.Items.Item("cbPT");
                //IcbPT.Enabled = false;

                oForm.DataSources.UserDataSources.Add("txtRqAmnt", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtRequestedAmount = oForm.Items.Item("txtRqAmnt").Specific;
                txtRequestedAmount.DataBind.SetBound(true, "", "txtRqAmnt");

                oForm.DataSources.UserDataSources.Add("txtReqDt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtReqDt = oForm.Items.Item("txtReqDt").Specific;
                txtReqDt.DataBind.SetBound(true, "", "txtReqDt");

                oForm.DataSources.UserDataSources.Add("txtApram", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtAprAmount = oForm.Items.Item("txtApram").Specific;
                txtAprAmount.DataBind.SetBound(true, "", "txtApram");
                txtPreAd = oForm.Items.Item("txtPreAd").Specific;

                InitiallizegridMatrix();


                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oForm.ActiveItem = "txtEmpC";
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
                dtPrevAdvance = oForm.DataSources.DataTables.Add("AdvanceRequest");
                dtPrevAdvance.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtPrevAdvance.Columns.Add("AdvanceType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("RecToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("RemToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("clDate", SAPbouiCOM.BoFieldsType.ft_Date);

                grdAdvncDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grdAdvD").Specific;
                oColumns = (SAPbouiCOM.Columns)grdAdvncDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "No");

                oColumn = oColumns.Item("AdvType");
                AdvanceType = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "AdvanceType");

                oColumn = oColumns.Item("Amount");
                Amount = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "Amount");

                oColumn = oColumns.Item("cl_RecTD");
                RecToDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "RecToDate");

                oColumn = oColumns.Item("RemToDate");
                RemToDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "RemToDate");

                oColumn = oColumns.Item("clDate");
                clDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "clDate");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillParentAdvnTypeCombo()
        {
            try
            {
                cmbAdvanceType.ValidValues.Add("-1", "[Select One]");
                var Data = from v in dbHrPayroll.MstAdvance where v.FlgActive == true select v;
                foreach (var v in Data)
                {
                    cmbAdvanceType.ValidValues.Add(v.Id.ToString(), v.Description);
                }
                cmbAdvanceType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
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
                //var Data = from v in dbHrPayroll.MstAdvance where v.FlgActive == true select v;
                //foreach (var v in Data)
                //{
                //    cbAdvT.ValidValues.Add(v.Id.ToString(), v.Description);
                //}
                cbPT.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadSelectedData(String pCode)
        {

            try
            {
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    string strDocStatus = "LV0001", strApprovalStatus = "LV0005";
                    decimal decApprovedAmount = 0;
                    if (!String.IsNullOrEmpty(pCode))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID == pCode
                                      //where a.EmpID.Contains(pCode)
                                      select a).FirstOrDefault();
                        var GetUser = getEmp.MstUsers.FirstOrDefault();

                        if (getEmp != null)
                        {
                            if (Program.systemInfo.FlgRetailRules1 == false)
                            {
                                txtExpSalary.Value = string.Format("{0:0.00}", GetSalaryTillDate(getEmp));
                            }
                            else
                            {
                                txtExpSalary.Value = string.Format("{0:0.00}", GetSalaryTillDateISM(getEmp));
                            }
                            //txtdocNum.Value = Convert.ToString(dbHrPayroll.TrnsAdvance.Count() + 1);
                            txtdocNum.Value = Convert.ToString((from a in dbHrPayroll.TrnsAdvance select a.DocNum).Max() + 1);
                            if (GetUser != null)
                            {
                                txtOriginator.Value = GetUser.UserID;
                            }
                            txtReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                            txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                            txtdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                            txtdesig.Value = getEmp.DesignationName;
                            //txtSalary.Value = String.Format("{0:0.00}", ds.getEmpGross(getEmp)); 
                            //txtSalary.Value = String.Format("{0:0.00}", getEmp.BasicSalary);
                            SetEmployeeSalaryConditional(getEmp);
                            txtdocStatus.Value = dbHrPayroll.MstLOVE.Where(lv => lv.Code == strDocStatus).Single().Value;
                            txtappStatus.Value = dbHrPayroll.MstLOVE.Where(lv => lv.Code == strApprovalStatus).Single().Value;
                            txtAprAmount.Value = Convert.ToString(decApprovedAmount);
                            //cbAdvT.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                            var DefaultData = dbHrPayroll.MstAdvance.Where(a => a.FlgDefault == true).FirstOrDefault();
                            if (DefaultData != null)
                            {
                                cmbAdvanceType.Select(DefaultData.Id.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                            }
                            else
                            {
                                cmbAdvanceType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                            }
                            if (!String.IsNullOrEmpty(getEmp.ImgPath))
                            {
                                pctBox.Picture = getEmp.ImgPath;
                            }
                            else
                            {
                                pctBox.Picture = "";
                            }
                            txtReqDt.Value = DateTime.Now.ToString("yyyyMMdd");

                            GetAdvanceHistory(getEmp.ID);

                        }

                    }
                }
                else
                {

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetAdvanceHistory(int intEmpID)
        {
            string strDocStatus = "LV0003", strApprovalStatus = "LV0006";
            Int16 i = 0;
            try
            {
                var Data = dbHrPayroll.TrnsAdvance.Where(adv => adv.EmpID == intEmpID).ToList();
                if (Data != null && Data.Count > 0)
                {
                    Data = Data.Where(Ad => Ad.DocAprStatus == strApprovalStatus && Ad.RemainingAmount > 0 && (Ad.FlgStop != null ? Ad.FlgStop : false) == false).ToList();
                }
                if (Data.Count == 0)
                {
                    dtPrevAdvance.Rows.Clear();
                    grdAdvncDetail.LoadFromDataSource();
                    return;
                }
                else if (Data != null && Data.Count > 0)
                {
                    decimal ReceiveAmount = 0;
                    dtPrevAdvance.Rows.Clear();
                    dtPrevAdvance.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        var AdvanceType = dbHrPayroll.MstAdvance.Where(a => a.Id == WD.AdvanceType).FirstOrDefault();
                        dtPrevAdvance.SetValue("No", i, i + 1);
                        dtPrevAdvance.SetValue("AdvanceType", i, AdvanceType.Description);
                        dtPrevAdvance.SetValue("Amount", i, String.Format("{0:0.00}", WD.ApprovedAmount));
                        if (WD.ApprovedAmount > 0 && WD.RemainingAmount >= 0)
                        {
                            ReceiveAmount = WD.ApprovedAmount.Value - WD.RemainingAmount.Value;
                        }
                        if (WD.RequiredDate != null)
                        {
                            dtPrevAdvance.SetValue("clDate", i, WD.RequiredDate);
                        }
                        dtPrevAdvance.SetValue("RecToDate", i, String.Format("{0:0.00}", ReceiveAmount));
                        dtPrevAdvance.SetValue("RemToDate", i, String.Format("{0:0.00}", WD.RemainingAmount));
                        i++;
                    }
                    grdAdvncDetail.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }

        private Boolean ValidateRecord()
        {
            try
            {
                if (string.IsNullOrEmpty(txtEmpCode.Value))
                {
                    oApplication.StatusBar.SetText("Employee Code field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtReqDt.Value))
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
                    decimal valuecheck = Convert.ToDecimal(txtRequestedAmount.Value);
                    if (valuecheck <= 0)
                    {
                        oApplication.StatusBar.SetText("Requested Amount can't be zero or negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                if (string.IsNullOrEmpty(cmbAdvanceType.Value) || cmbAdvanceType.Value.Trim() == "-1")
                {
                    oApplication.StatusBar.SetText("Advance Type is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                var oEmp = (from a in dbHrPayroll.MstEmployee
                            where a.EmpID == txtEmpCode.Value.Trim()
                            select a).FirstOrDefault();
                if (Program.systemInfo.FlgRetailRules1 == true)
                {
                    decimal RequestedAmount = string.IsNullOrEmpty(txtRequestedAmount.Value) ? 0 : Convert.ToDecimal(txtRequestedAmount.Value);
                    decimal Salary = string.IsNullOrEmpty(txtSalary.Value) ? 0 : Convert.ToDecimal(txtSalary.Value);
                    decimal ExpectedSalary = string.IsNullOrEmpty(txtExpSalary.Value) ? 0 : Convert.ToDecimal(txtExpSalary.Value);
                    DateTime dtRequestDate = DateTime.ParseExact(txtReqDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //decimal salaryBalance = Salary + ExpectedSalary;
                    if (oEmp != null)
                    {
                        var oPeriod = (from a in dbHrPayroll.CfgPeriodDates
                                       where a.StartDate <= dtRequestDate
                                       && a.EndDate >= dtRequestDate
                                       && a.PayrollId == oEmp.PayrollID
                                       select a).FirstOrDefault();
                        Int32 DayCount = 0;
                        if (oEmp.JoiningDate > oPeriod.StartDate)
                        {
                            DayCount = Convert.ToInt32((dtRequestDate - Convert.ToDateTime(oEmp.JoiningDate)).TotalDays + 1d);
                        }
                        else
                        {
                            DayCount = Convert.ToInt32((dtRequestDate - Convert.ToDateTime(oPeriod.StartDate)).TotalDays + 1d);
                        }
                        Int32 SavedAttendanceCount = (from a in dbHrPayroll.TrnsAttendanceRegister
                                                      where a.EmpID == oEmp.ID
                                                      && a.PeriodID == oPeriod.ID
                                                      && (a.Processed == null ? false : Convert.ToBoolean(a.Processed)) == true
                                                      select a).Count();
                        if (SavedAttendanceCount < DayCount)
                        {
                            MsgWarning("Attendance was not saved, You're not allowed to enter advance request.");
                            return false;
                        }
                        if (oEmp.AllowedAdvance == null)
                        {
                            MsgWarning("You must define Allowed percentage for advance on employee master.");
                            return false;
                        }
                        else
                        {
                            decimal percentagevalue = Convert.ToDecimal(oEmp.AllowedAdvance);
                            if (RequestedAmount > (ExpectedSalary * (percentagevalue / 100)))
                            {
                                MsgWarning("Requested amount can't be higher than allowed advance range " + oEmp.EmpID + " : " + string.Format("{0:0.00}", oEmp.AllowedAdvance) + " Percent.");
                                return false;
                            }
                        }
                    }

                }
                else
                {
                    #region Get Period Days for Daily Wager Salary
                    int CountTotalPeriodDays = 0;
                    var GetCurrentMonthsPeriod = dbHrPayroll.CfgPeriodDates.Where(e => e.StartDate <= DateTime.Now
                        && e.EndDate >= DateTime.Now && e.PayrollId == oEmp.PayrollID).FirstOrDefault();
                    if (GetCurrentMonthsPeriod != null)
                    {
                        DateTime startDate = GetCurrentMonthsPeriod.StartDate.Value;
                        DateTime EndDate = GetCurrentMonthsPeriod.EndDate.Value;
                        DateTime TodayDate = DateTime.Now;
                        CountTotalPeriodDays = (EndDate - startDate).Days + 1;
                    }
                    #endregion
                    decimal RequestedAmount = string.IsNullOrEmpty(txtRequestedAmount.Value) ? 0 : Convert.ToDecimal(txtRequestedAmount.Value);
                    decimal Salary = string.IsNullOrEmpty(txtSalary.Value) ? 0 : Convert.ToDecimal(txtSalary.Value);
                    decimal PrevAdvance = string.IsNullOrEmpty(txtPreAd.Value) ? 0 : Convert.ToDecimal(txtPreAd.Value);
                    decimal salaryBalance = 0;
                    if (!string.IsNullOrEmpty(oEmp.EmployeeContractType) && oEmp.EmployeeContractType != "DWGS")
                    {
                        salaryBalance = Salary + PrevAdvance;
                    }
                    else
                    {
                        salaryBalance = (Salary * CountTotalPeriodDays) + PrevAdvance;
                    }
                    if (RequestedAmount > salaryBalance)
                    {
                        oApplication.StatusBar.SetText("Requested amount can't be greater than Salary", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddRecord()
        {
            try
            {
                int EmpID;
                String pCode = txtEmpCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                var getUser = getEmp.MstUsers.FirstOrDefault();
                if (getEmp != null && !string.IsNullOrEmpty(txtReqDt.Value) && !string.IsNullOrEmpty(txtRequestedAmount.Value))
                {
                    EmpID = getEmp.ID;
                    decimal RequestedAmount = string.IsNullOrEmpty(txtRequestedAmount.Value) ? 0 : Convert.ToDecimal(txtRequestedAmount.Value);
                    decimal Salary = string.IsNullOrEmpty(txtSalary.Value) ? 0 : Convert.ToDecimal(txtSalary.Value);
                    decimal PrevAdvance = string.IsNullOrEmpty(txtPreAd.Value) ? 0 : Convert.ToDecimal(txtPreAd.Value);
                    decimal salaryBalance = Salary + PrevAdvance;

                    //int confirm = oApplication.MessageBox("Are you sure you want to Add Advacne for Selected Employee? ", 3, "Yes", "No", "Cancel");
                    //if (confirm == 2 || confirm == 3)
                    //{
                    //    return;
                    //} 
                    TrnsAdvance oNewAdvnc = new TrnsAdvance();
                    Int32 DocNum = 0;
                    //DocumentNUmber = Convert.ToInt32((from a in dbHrPayroll.TrnsAdvance select a.DocNum).Max());
                    //int? dbvalue = dbHrPayroll.TrnsAdvance.Max(a => a.ID);
                    int? dbvalue = null;
                    if (dbHrPayroll.TrnsAdvance.Count() > 0)
                    {
                        dbvalue = (from a in dbHrPayroll.TrnsAdvance select a.ID).Max();
                    }
                    else
                    {
                        dbvalue = null;
                    }
                    if (dbvalue == null)
                    {
                        DocNum = 1;
                    }
                    else
                    {
                        DocNum = Convert.ToInt32(dbvalue);
                    }
                    if (DocNum > 0)
                    {
                        DocNum += 1;
                    }
                    else
                    {
                        DocNum = 1;
                    }
                    //oNewAdvnc.DocNum = Convert.ToInt32(DocumentNUmber);
                    oNewAdvnc.Series = -1;
                    oNewAdvnc.EmpID = EmpID;
                    oNewAdvnc.EmpName = txtReqBy.Value;
                    oNewAdvnc.FlgActive = true;
                    oNewAdvnc.FlgStop = false;
                    if (!string.IsNullOrEmpty(txtManager.Value))
                    {
                        oNewAdvnc.ManagerID = getEmp.Manager;
                        oNewAdvnc.ManagerName = txtManager.Value;
                    }
                    oNewAdvnc.UserId = oCompany.UserName;
                    if (!string.IsNullOrEmpty(txtdoj.Value))
                    {
                        oNewAdvnc.DateOfJoining = DateTime.ParseExact(txtdoj.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    oNewAdvnc.DesignationID = getEmp.DesignationID;
                    oNewAdvnc.Designation = txtdesig.Value;
                    if (string.IsNullOrEmpty(txtSalary.Value))
                    {
                        oApplication.StatusBar.SetText("Employee Salary Field Can't be Empty", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    oNewAdvnc.Salary = Convert.ToDecimal(txtSalary.Value);
                    if (getUser != null)
                    {
                        oNewAdvnc.OriginatorID = EmpID;
                        oNewAdvnc.OriginatorName = getUser.UserID;
                    }
                    else
                    {
                        oNewAdvnc.OriginatorID = EmpID;
                        oNewAdvnc.OriginatorName = txtReqBy.Value;
                    }
                    oNewAdvnc.CreateDate = DateTime.Now;
                    oNewAdvnc.UserId = oCompany.UserName;
                    oNewAdvnc.UpdateDate = DateTime.Now;
                    oNewAdvnc.UpdateBy = oCompany.UserName;

                    oNewAdvnc.AdvanceType = Convert.ToInt32(cmbAdvanceType.Value);
                    oNewAdvnc.RequestedAmount = Convert.ToDecimal(txtRequestedAmount.Value);
                    oNewAdvnc.RequiredDate = DateTime.ParseExact(txtReqDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    oNewAdvnc.ApprovedAmount = 0;
                    oNewAdvnc.MaturityDate = DateTime.ParseExact(txtReqDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    dbHrPayroll.TrnsAdvance.InsertOnSubmit(oNewAdvnc);
                    dbHrPayroll.SubmitChanges();
                    oNewAdvnc.DocNum = oNewAdvnc.ID;
                    dbHrPayroll.SubmitChanges();
                    //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    oForm.ActiveItem = "txtEmpC";
                    GetDataFilterData();
                    ClearControls();
                }
                else
                {
                    oApplication.StatusBar.SetText("Required Field(s) Missing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: InsertAdvanceRequest Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetNextRecord()
        {
            var AdvanceRecords = dbHrPayroll.TrnsAdvance.ToList();
            if (AdvanceRecords != null && AdvanceRecords.Count > 0)
            {
                TotalRecords = AdvanceRecords.Count;
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
            var AdvanceRecords = dbHrPayroll.TrnsAdvance.ToList();
            if (AdvanceRecords != null && AdvanceRecords.Count > 0)
            {
                TotalRecords = AdvanceRecords.Count;
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

                oDocuments = dbHrPayroll.TrnsAdvance.ToList();
                TrnsAdvance oDoc = oDocuments.ElementAt<TrnsAdvance>(DocumentID);
                if (!String.IsNullOrEmpty(oDoc.EmpName))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.ID == oDoc.EmpID
                                  select a).FirstOrDefault();
                    if (!String.IsNullOrEmpty(getEmp.ImgPath))
                    {
                        pctBox.Picture = getEmp.ImgPath;
                    }
                    else
                    {
                        pctBox.Picture = "";
                    }
                    var GetUser = getEmp.MstUsers.FirstOrDefault();
                    txtEmpCode.Value = getEmp.EmpID;
                    //txtOriginator.Value = GetUser.UserID;
                    txtReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                    txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                    txtdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                    txtdesig.Value = getEmp.DesignationName;
                    //txtSalary.Value = String.Format("{0:0.00}", getEmp.BasicSalary);
                    SetEmployeeSalaryConditional(getEmp);
                    txtPE.Value = Convert.ToString(oDoc.TransID);
                }
                txtdocNum.Value = Convert.ToString(oDoc.DocNum);
                if (oDoc.RemainingAmount == 0)
                {
                    txtdocStatus.Value = "Closed";
                }
                else
                {
                    txtdocStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == oDoc.DocStatus).Single().Value;
                }
                txtappStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == oDoc.DocAprStatus).Single().Value;
                txtRequestedAmount.Value = Convert.ToString(oDoc.RequestedAmount);
                txtReqDt.Value = Convert.ToDateTime(oDoc.RequiredDate).ToString("yyyyMMdd");
                txtAprAmount.Value = Convert.ToString(oDoc.ApprovedAmount);
                //txtGL.Value = "_SYS00000000003";
                //txtPaidA.Value = "_SYS00000000044";
                if (oDoc.AdvanceType > 0)
                {
                    cmbAdvanceType.Select(oDoc.AdvanceType.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                if (!string.IsNullOrEmpty(txtappStatus.Value))
                {
                    if (txtappStatus.Value.Trim() == "Approved")
                    {
                        if (string.IsNullOrEmpty(txtPE.Value))
                        {
                            IbtPay.Enabled = true;
                            ItxtPaidAccount.Enabled = true;
                        }
                        else
                        {
                            IbtPay.Enabled = false;
                            ItxtPaidAccount.Enabled = false;
                            IbtSave.Enabled = false;
                        }
                    }
                }
                else
                {
                    IbtPay.Enabled = false;
                    ItxtPaidAccount.Enabled = false;
                }
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
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
                txtEmpCode.Value = string.Empty;
                txtReqBy.Value = string.Empty;
                txtdocNum.Value = string.Empty;
                txtManager.Value = string.Empty;
                txtdoj.Value = string.Empty;
                txtdesig.Value = string.Empty;
                txtSalary.Value = string.Empty;
                txtOriginator.Value = string.Empty;
                txtRequestedAmount.Value = string.Empty;
                txtReqDt.Value = string.Empty;
                txtdocStatus.Value = string.Empty;
                txtappStatus.Value = string.Empty;
                txtAprAmount.Value = string.Empty;
                txtExpSalary.Value = string.Empty;
                txtPreAd.Value = string.Empty;
                cmbAdvanceType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtPrevAdvance.Rows.Clear();
                grdAdvncDetail.LoadFromDataSource();

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void doFind()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empAdvance", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Advace");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpCode.Value = st.Rows[0][0].ToString();
                LoadSelectedData(txtEmpCode.Value);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            if (!string.IsNullOrEmpty(txtEmpCode.Value))
            {
                SearchKeyVal.Add("EmpID", txtEmpCode.Value.ToString());
            }
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
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    //GetDataFilterData();
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmpCode.Value = Program.EmpID;
                    LoadSelectedData(txtEmpCode.Value);
                    oForm.ActiveItem = "txtRqAmnt";
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private decimal GetSalaryTillDate(MstEmployee objEmp)
        {
            try
            {

                decimal BasicTillDay = 0.0M;
                decimal leaveCnt = 0.00M;
                decimal payDays = 0.00M;
                decimal leaveDays = 0.00M;
                decimal monthDays = 0.00M;
                decimal getElemnts = 0.0M;
                decimal getAdvanceDeduction = 0.0M;
                decimal getLoanDeduction = 0.0M;
                decimal getLeaveDeduction = 0.0M;
                decimal payRatio = 1.00M;
                decimal getOT = 0.0M;
                decimal TotalSalary = 0.0M;
                MstGLDetermination glDetr = null;
                try
                {
                    glDetr = ds.getEmpGl(objEmp);
                    if (glDetr == null)
                    {
                        oApplication.StatusBar.SetText("EmpCode : " + objEmp.EmpID + " Doesn't have GL determination defined in respected Location or Deparment.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return 0M;
                    }
                }
                catch
                {
                    oApplication.StatusBar.SetText("GL Determination for Employee not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return 0M;
                }
                var GetCurrentMonthsPeriod = dbHrPayroll.CfgPeriodDates.Where(e => e.StartDate <= DateTime.Now && e.EndDate >= DateTime.Now && e.PayrollId == objEmp.PayrollID).FirstOrDefault();
                if (GetCurrentMonthsPeriod != null)
                {
                    DateTime startDate = GetCurrentMonthsPeriod.StartDate.Value;
                    DateTime EndDate = GetCurrentMonthsPeriod.EndDate.Value;
                    DateTime TodayDate = DateTime.Now;
                    int CountTotalPeriodDays = (EndDate - startDate).Days + 1;
                    int COuntTillTodayDays = (TodayDate - startDate).Days + 1;
                    payRatio = COuntTillTodayDays;
                    decimal getBasic = objEmp.BasicSalary.Value;
                    //Calculate Basic Salary
                    if (getBasic > 0)
                    {
                        if (!string.IsNullOrEmpty(objEmp.EmployeeContractType) && objEmp.EmployeeContractType != "DWGS")
                        {
                            BasicTillDay = (getBasic / CountTotalPeriodDays) * COuntTillTodayDays;
                        }
                        else
                        {
                            BasicTillDay = (getBasic * CountTotalPeriodDays);
                        }
                    }
                    //////Absents ////
                    System.Data.DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(objEmp, GetCurrentMonthsPeriod, (decimal)ds.getEmpGross(objEmp), out leaveCnt);
                    foreach (DataRow dr in dtAbsentDeduction.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getLeaveDeduction = getLeaveDeduction + RecordAmount;
                    }
                    //* Payroll elements assigned to employee ***Employee Elements ****** 
                    System.Data.DataTable dtSalPrlElements = ds.salaryProcessingElements(objEmp, GetCurrentMonthsPeriod, COuntTillTodayDays, (decimal)ds.getEmpGross(objEmp), glDetr, payRatio, 0, 0, 0);
                    foreach (DataRow dr in dtSalPrlElements.Rows)
                    {
                        if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                        {
                            decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            getElemnts = getElemnts + RecordAmount;
                        }
                    }
                    //////Over time ////
                    Int32 otminute = 0;
                    System.Data.DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(objEmp, GetCurrentMonthsPeriod, (decimal)ds.getEmpGross(objEmp), out otminute);
                    foreach (DataRow dr in dtSalOverTimes.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getOT = getOT + RecordAmount;
                    }
                    // * ************Advance Recovery Processing **************
                    System.Data.DataTable dtAdvance = ds.salaryProcessingAdvance(objEmp, (decimal)ds.getEmpGross(objEmp), GetCurrentMonthsPeriod);
                    foreach (DataRow dr in dtAdvance.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getAdvanceDeduction = getAdvanceDeduction + RecordAmount;
                    }
                    txtPreAd.Value = string.Format("{0:0.00}", getAdvanceDeduction);
                    // * ************Loan Recovery Processing **************
                    System.Data.DataTable dtLoands = ds.salaryProcessingLoans(objEmp, (decimal)ds.getEmpGross(objEmp), GetCurrentMonthsPeriod);
                    foreach (DataRow dr in dtLoands.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getLoanDeduction = getLoanDeduction + RecordAmount;
                    }

                    //Sum TOTAL

                    TotalSalary = BasicTillDay + getLeaveDeduction + getElemnts + getOT + getAdvanceDeduction + getLoanDeduction;



                }
                return TotalSalary;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0.0M;
            }
        }

        private decimal GetSalaryTillDateISM(MstEmployee oEmp)
        {
            try
            {
                decimal BasicTillDay = 0.0M;
                decimal leaveCnt = 0.00M;
                decimal payDays = 0.00M;
                decimal leaveDays = 0.00M;
                decimal monthDays = 0.00M;
                decimal getElemnts = 0.0M;
                decimal getAdvanceDeduction = 0.0M;
                decimal getLoanDeduction = 0.0M;
                decimal getLeaveDeduction = 0.0M;
                decimal payRatio = 1.00M;
                decimal getOT = 0.0M;
                decimal TotalSalary = 0.0M;
                MstGLDetermination glDetr = null;
                try
                {
                    glDetr = ds.getEmpGl(oEmp);
                }
                catch
                {
                    MsgWarning("GL Determination for employee not found.");
                    return 0M;
                }
                DateTime RequestDate = DateTime.MinValue;
                if (!string.IsNullOrEmpty(txtReqDt.Value))
                {
                    RequestDate = DateTime.ParseExact(txtReqDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                else
                {
                    return 0m;
                }
                //var GetPeriodFromRequestDate = dbHrPayroll.CfgPeriodDates.Where(e => e.StartDate <= DateTime.Now && e.EndDate >= DateTime.Now && e.PayrollId == oEmp.PayrollID).FirstOrDefault();
                var GetPeriodFromRequestDate = (from a in dbHrPayroll.CfgPeriodDates
                                                where a.StartDate <= RequestDate
                                                && a.EndDate >= RequestDate
                                                && a.PayrollId == oEmp.PayrollID
                                                select a).FirstOrDefault();
                if (GetPeriodFromRequestDate != null)
                {
                    DateTime startDate = GetPeriodFromRequestDate.StartDate.Value;
                    if (oEmp.JoiningDate > GetPeriodFromRequestDate.StartDate.Value)
                    {
                        startDate = Convert.ToDateTime(oEmp.JoiningDate);
                    }
                    DateTime EndDate = GetPeriodFromRequestDate.EndDate.Value;
                    DateTime TodayDate = RequestDate;
                    int CountTotalPeriodDays = (EndDate - startDate).Days + 1;
                    int CountTillDate = (TodayDate - startDate).Days + 1;
                    if (oEmp.JoiningDate > GetPeriodFromRequestDate.StartDate.Value)
                    {
                        payRatio = CountTillDate / CountTotalPeriodDays;
                    }
                    else
                    {
                        payRatio = 1;
                    }
                    decimal getBasic = oEmp.BasicSalary.Value;
                    //Calculate Basic Salary
                    if (getBasic > 0)
                    {
                        BasicTillDay = (getBasic / CountTotalPeriodDays) * CountTillDate;
                    }
                    //////Absents ////
                    System.Data.DataTable dtAbsentDeduction = ds.DynamicLeavesProcessing(oEmp, GetPeriodFromRequestDate, (decimal)ds.getEmpGross(oEmp), out leaveCnt, RequestDate);
                    foreach (DataRow dr in dtAbsentDeduction.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getLeaveDeduction = getLeaveDeduction + RecordAmount;
                    }
                    //* Payroll elements assigned to employee ***Employee Elements ****** 
                    System.Data.DataTable dtSalPrlElements = ds.salaryProcessingElements(oEmp, GetPeriodFromRequestDate, CountTillDate, (decimal)ds.getEmpGross(oEmp), glDetr, payRatio, 0, 0, 0);
                    foreach (DataRow dr in dtSalPrlElements.Rows)
                    {
                        if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                        {
                            decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            getElemnts = getElemnts + RecordAmount;                           
                        }
                    }

                    // * ************Advance Recovery Processing **************
                    System.Data.DataTable dtAdvance = ds.salaryProcessingAdvance(oEmp, (decimal)ds.getEmpGross(oEmp), GetPeriodFromRequestDate);
                    foreach (DataRow dr in dtAdvance.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getAdvanceDeduction = getAdvanceDeduction + RecordAmount;
                    }
                    txtPreAd.Value = string.Format("{0:0.00}", getAdvanceDeduction);
                    // * ************Loan Recovery Processing **************
                    System.Data.DataTable dtLoands = ds.salaryProcessingLoans(oEmp, (decimal)ds.getEmpGross(oEmp), GetPeriodFromRequestDate);
                    foreach (DataRow dr in dtLoands.Rows)
                    {
                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                        getLoanDeduction = getLoanDeduction + RecordAmount;

                    }

                    //Sum TOTAL

                    TotalSalary = BasicTillDay + getLeaveDeduction + getElemnts + getOT + getAdvanceDeduction + getLoanDeduction;



                }
                return TotalSalary;
            }
            catch (Exception ex)
            {
                //oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                logger(ex);
                return 0.0M;
            }
        }

        private void printReport()
        {
            try
            {
                string docNum = txtdocNum.Value;
                if (!string.IsNullOrEmpty(docNum))
                {
                    string cri = "   WHERE dbo.TrnsAdvance.DocNum = '" + docNum + "'";
                    Program.objHrmsUI.printRpt("AdvPrint", true, cri, "");
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in showing report. " + ex.Message);
            }
        }

        private void PostPaymentXXXX()
        {
            try
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

                oPays.Series = 10;

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
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostPaymentBank()
        {
            try
            {
                Hashtable elementGls = new Hashtable();
                string strDebitAccount = "";
                string strCreditAccount = "";

                String pCode = txtEmpCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                var advReq = dbHrPayroll.TrnsAdvance.Where(ad => ad.DocNum == Convert.ToInt32(txtdocNum.Value)).FirstOrDefault();

                MstAdvance adv = (from p in dbHrPayroll.MstAdvance
                                  where p.Id.ToString() == advReq.AdvanceType.ToString()
                                  select p).FirstOrDefault();

                if (adv != null)
                {
                    try
                    {
                        elementGls = getAdvGL(getEmp, adv);
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
                if (advReq == null)
                {
                    oApplication.StatusBar.SetText("Advance Request Not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (advReq != null)
                {
                    bool IsPaid = advReq.FlgPaid == null ? false : advReq.FlgPaid.Value;
                    if (IsPaid)
                    {
                        oApplication.StatusBar.SetText("Advance Already paid", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cmbAdvanceType.Value))
                {
                    oApplication.StatusBar.SetText("Please select Advance Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cmbAdvanceType.Value) && Convert.ToInt32(cmbAdvanceType.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Advance Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                if (string.IsNullOrEmpty(txtPaidAccount.Value))
                {
                    oApplication.StatusBar.SetText("Please select Account Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }


                SAPbobsCOM.Payments oPays = default(SAPbobsCOM.Payments);
                oPays = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                oPays.AccountPayments.AccountCode = strCreditAccount; //txtGL.Value.Trim();
                oPays.AccountPayments.Decription = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName; //"HRMS Payroll Manual";
                oPays.AccountPayments.SumPaid = Convert.ToDouble(txtAprAmount.Value);

                oPays.AccountPayments.UserFields.Fields.Item("U_DocNumber").Value = Convert.ToString(advReq.ID);
                oPays.AccountPayments.UserFields.Fields.Item("U_EmpID").Value = getEmp.EmpID;
                oPays.AccountPayments.UserFields.Fields.Item("U_Type").Value = "Advance";

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
                        //outStr = "Error : BranchName unable to retrive.";
                        oApplication.StatusBar.SetText("Error : BranchName unable to retrive.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);

                    oPays.BPLID = Convert.ToInt32(BranchIDFromSAP);
                }


                oPays.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                oPays.DocType = SAPbobsCOM.BoRcptTypes.rAccount;
                oPays.DocTypte = SAPbobsCOM.BoRcptTypes.rAccount;
                oPays.DocDate = DateTime.Now;
                oPays.JournalRemarks = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName; //"pag efect";
                oPays.TaxDate = DateTime.Now;
                oPays.TransferAccount = txtPaidAccount.Value.Trim(); //"_SYS00000000003"; //"_SYS00000000003";
                oPays.TransferDate = DateTime.Now;
                oPays.TransferReference = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName; //"ref01";
                oPays.TransferSum = Convert.ToDouble(txtAprAmount.Value);
                oPays.ApplyVAT = SAPbobsCOM.BoYesNoEnum.tNO;
                oPays.AccountPayments.Add();
                int paidDoc = oPays.Add();
                if (paidDoc != 0)
                {
                    int erroCode = 0;
                    string errDescr = "";
                    oCompany.GetLastError(out erroCode, out errDescr);
                    //dtError.Rows.Add(DateTime.Now.ToString(), "Not posted Error :" + errDescr);
                    oApplication.StatusBar.SetText("SAP Payment document not posted Error : " + errDescr, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    // outStr = "SBO Error:" + errDescr;
                }
                else
                {
                    string strPENumber = Convert.ToString(oCompany.GetNewObjectKey());
                    txtPE.Value = strPENumber;
                    var AdvRequest = dbHrPayroll.TrnsAdvance.Where(a => a.DocNum == Convert.ToInt32(txtdocNum.Value)).FirstOrDefault();
                    if (AdvRequest != null)
                    {
                        AdvRequest.TransID = Convert.ToInt32(strPENumber);
                        AdvRequest.FlgPaid = true;
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

                String pCode = txtEmpCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                var advReq = dbHrPayroll.TrnsAdvance.Where(ad => ad.DocNum == Convert.ToInt32(txtdocNum.Value)).FirstOrDefault();
                MstAdvance adv = (from p in dbHrPayroll.MstAdvance where p.Id.ToString() == advReq.AdvanceType.ToString() select p).Single();
                if (adv != null)
                {
                    try
                    {
                        elementGls = getAdvGL(getEmp, adv);
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
                if (advReq == null)
                {
                    oApplication.StatusBar.SetText("Advance Request Not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (advReq != null)
                {
                    bool IsPaid = advReq.FlgPaid == null ? false : advReq.FlgPaid.Value;
                    if (IsPaid)
                    {
                        oApplication.StatusBar.SetText("Advance Already paid", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cmbAdvanceType.Value))
                {
                    oApplication.StatusBar.SetText("Please select Advance Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cmbAdvanceType.Value) && Convert.ToInt32(cmbAdvanceType.Value) < 1)
                {
                    oApplication.StatusBar.SetText("Please select Advance Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                if (string.IsNullOrEmpty(txtPaidAccount.Value))
                {
                    oApplication.StatusBar.SetText("Please select Account Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }


                SAPbobsCOM.Payments oPays = default(SAPbobsCOM.Payments);
                oPays = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                oPays.AccountPayments.AccountCode = strCreditAccount; //txtGL.Value.Trim();
                oPays.AccountPayments.Decription = getEmp.EmpID + " : " + getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;// "HRMS Payroll Manual";
                oPays.AccountPayments.SumPaid = Convert.ToDouble(txtAprAmount.Value);

                oPays.AccountPayments.UserFields.Fields.Item("U_DocNumber").Value = Convert.ToString(advReq.ID);
                oPays.AccountPayments.UserFields.Fields.Item("U_EmpID").Value = getEmp.EmpID;
                oPays.AccountPayments.UserFields.Fields.Item("U_Type").Value = "Advance";

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
                        //outStr = "Error : BranchName unable to retrive.";
                        oApplication.StatusBar.SetText("Error : BranchName unable to retrive.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    BranchIDFromSAP = Convert.ToString(oRecSet.Fields.Item("BPLId").Value);

                    oPays.BPLID = Convert.ToInt32(BranchIDFromSAP);
                }

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


                oPays.CashAccount = txtPaidAccount.Value.Trim();
                oPays.CashSum = Convert.ToDouble(txtAprAmount.Value);


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
                    var AdvRequest = dbHrPayroll.TrnsAdvance.Where(a => a.DocNum == Convert.ToInt32(txtdocNum.Value)).FirstOrDefault();
                    if (AdvRequest != null)
                    {
                        AdvRequest.TransID = Convert.ToInt32(strPENumber);
                        AdvRequest.FlgPaid = true;
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

        public bool alreadyExist(string docType, string txtRef, out string objectKey)
        {
            bool result = false;
            objectKey = "";

            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(Environment.GetCommandLineArgs().GetValue(1).ToString());
            con.Open();
            SqlDataReader dr;
            cmd.Connection = con;



            if (docType == "GoodReciept")
            {
                try
                {

                    cmd.CommandText = "Select docnum from oign where u_CMSWeightId='" + txtRef + "'";
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        objectKey = dr[0].ToString();
                        result = true;
                    }

                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                }
            }


            if (docType == "JE")
            {
                try
                {
                    cmd.CommandText = "select transid from ojdt where u_CMSJENum='" + txtRef + "'";
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        objectKey = dr[0].ToString();
                        result = true;
                    }

                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                }
            }

            if (docType == "Payment")
            {
                try
                {

                    cmd.CommandText = "select  docnum from ovpm where U_CMSPaymentId = '" + txtRef + "'";
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        objectKey = dr[0].ToString();
                        result = true;
                    }

                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                }
            }
            if (docType == "Production")
            {
                try
                {

                    cmd.CommandText = "select docnum  from owor where U_CMSRecievingNum = '" + txtRef + "'";
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        objectKey = dr[0].ToString();
                        result = true;
                    }

                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open) con.Close();
                }
            }

            if (con.State == ConnectionState.Open) con.Close();
            return result;

        }

        public string getBankGL(string bankCode, string Branch, string acctNum)
        {
            string resultStr = "";
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(Environment.GetCommandLineArgs().GetValue(1).ToString());
            try
            {
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = "select GLAccount from dsc1 where BankCode='" + bankCode + "' and Branch='" + Branch + "' and Account='" + acctNum + "'";
                resultStr = cmd.ExecuteScalar().ToString();

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                resultStr = "Error";

            }

            return resultStr;
        }

        private void createDt()
        {
            try
            {

                dtError.Rows.Clear();
                dtError.Columns.Clear();
                dtError.Columns.Add("DT");
                dtError.Columns.Add("Error");
                //grdErrors.DataSource = dtError;
            }
            catch (Exception Ex)
            {
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

        public void postpayemntNewOne()
        {

            SAPbobsCOM.Payments oPays = default(SAPbobsCOM.Payments);

            oPays = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);



            oPays.AccountPayments.AccountCode = "_SYS00000000044";//"_SYS00000000044";

            //oPays.AccountPayments.AccountName = "Juan Perez"

            oPays.AccountPayments.Decription = "zeeshan Manual";

            oPays.AccountPayments.SumPaid = 255;



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

            oPays.TransferAccount = "_SYS00000000003"; //"_SYS00000000420";

            oPays.TransferDate = DateTime.Now;

            oPays.TransferReference = "ref01";

            oPays.TransferSum = 255;



            oPays.ApplyVAT = SAPbobsCOM.BoYesNoEnum.tNO;

            //oPays.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO

            //oPays.Proforma = SAPbobsCOM.BoYesNoEnum.tNO

            //oPays.DocNum = 3



            oPays.AccountPayments.Add();
            int paidDoc = oPays.Add();
            if (paidDoc != 0)
            {
                int erroCode = 0;
                string errDescr = "";
                oCompany.GetLastError(out erroCode, out errDescr);
                dtError.Rows.Add(DateTime.Now.ToString(), "Not posted Error :" + errDescr);

                // outStr = "SBO Error:" + errDescr;
            }
            else
            {
                //outStr = Convert.ToString(oCompany.GetNewObjectKey());
            }

            //if (oPays.Add() != 0)
            //{
            //    Interaction.MsgBox(oCompany.GetLastErrorDescription);


            //}            
        }

        public Hashtable getAdvGL(MstEmployee emp, MstAdvance adv)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in dbHrPayroll.MstGLDAdvanceDetail
                     where p.GLDId.ToString() == GlId.ToString() 
                     && p.AdvancesId.ToString() == adv.Id.ToString()
                     select p).Count();

            if (cntGl > 0)
            {
                MstGLDAdvanceDetail glAdv = (from p in dbHrPayroll.MstGLDAdvanceDetail
                                             where p.GLDId.ToString() == GlId.ToString() 
                                             && p.AdvancesId.ToString() == adv.Id.ToString()
                                             select p).FirstOrDefault();

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
            string GlType = emp.CfgPayrollDefination.GLType.Trim().ToString();

            try
            {

                if (GlType.Trim() == "LOC")
                {
                    detr = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "LOC" && p.GLValue == emp.Location select p).Single();
                }
                else if (GlType.Trim() == "DEPT")
                {
                    detr = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "DEPT" && p.GLValue == emp.DepartmentID select p).Single();
                }
                else if (GlType.Trim() == "COMP")
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

        private void SetEmployeeSalaryConditional(MstEmployee oEmp)
        {
            try
            {
                if (Convert.ToBoolean(Program.systemInfo.FlgRetailRules1))
                {
                    decimal TillDateSalary = 0M, AllowedSalary = 0M;
                    TillDateSalary = Convert.ToDecimal(txtExpSalary.Value);
                    AllowedSalary = ((TillDateSalary / 100) * Convert.ToDecimal(oEmp.AllowedAdvance));
                    txtSalary.Value = AllowedSalary.ToString();
                }
                else
                {
                    txtSalary.Value = String.Format("{0:0.00}", ds.getEmpGross(oEmp));
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        #endregion

    }
}
