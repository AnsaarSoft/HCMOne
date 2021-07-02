using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_LeaveEncash : HRMSBaseForm
    {
        #region Variable


        SAPbouiCOM.Button btSave, btCancel;
        SAPbouiCOM.CheckBox chkStandard;
        SAPbouiCOM.EditText txtEmpCode, txtEmpName, txtAmount, txtFromDate, txtsal, txtToDate, txtTotal, txtBF, txtEntitled, txtTotalAvb, txtUsed, txtRequeted, txtApproved, txtBalance;

        SAPbouiCOM.DataTable dtTest;
        private SAPbouiCOM.ComboBox cbLeaveType, cbPayrollPeriod;
        SAPbouiCOM.Item icbLeaveType, icbPayrollPeriod, ichkStandard;
        TrnsEmployeeElement empEle;
        TrnsEmployeeElementDetail trntEle;



        #endregion

        #region "B1 Form Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillLeaveTypeCombo();
            oForm.Freeze(false);
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                //case "txtFdt":
                //case "txtTdt":
                case "cbLTyp":
                    if (cbLeaveType != null && oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                        FindLeaveBalance();
                    break;
                case "txtTotl":
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                        CalculateTotalRequstedLeaves();
                    break;

                default:
                    break;
            }

        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                        {
                            if (!AddValidationUAE())
                            {
                                BubbleEvent = false;
                            }
                        }
                        else
                        {
                            if (!AddValidation())
                            {
                                BubbleEvent = false;
                            }
                        }
                        break;
                }

            }
            catch (Exception Ex)
            {


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
                        AddLeaveEncashmentElemntValueNEW();
                        break;
                    case "2":
                        break;
                    //case "chkOpt":
                    //    //if (btnOption.Caption == "Standard")
                    //    //{
                    //    //    btnOption.Caption = "Non Standard";
                    //    //}
                    //    //else
                    //    //{
                    //    //    btnOption.Caption = "Standard";
                    //    //}
                    //    break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LeaveRequest Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
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
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }

        #endregion

        #region "Local Methods"

        private void FillPayrollPeriods(int payrollID)
        {
            try
            {
                if (cbPayrollPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPayrollPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPayrollPeriod.ValidValues.Remove(cbPayrollPeriod.ValidValues.Item(k).Value);
                    }
                }
                cbPayrollPeriod.ValidValues.Add("-1", "[Select One]");
                var Data = from v in dbHrPayroll.CfgPeriodDates where v.PayrollId == payrollID && v.FlgLocked != true select v;
                foreach (var v in Data)
                {
                    cbPayrollPeriod.ValidValues.Add(v.ID.ToString(), v.PeriodName.ToString());
                    //cbPayrollPeriod.ValidValues.Add(v.PayrollId.ToString(), v.PeriodName.ToString());
                }
                cbPayrollPeriod.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillPayrollPeriods Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("2").Specific;
                oForm.DataSources.UserDataSources.Add("chkStn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkStandard = oForm.Items.Item("chkStn").Specific;
                ichkStandard = oForm.Items.Item("chkStn");
                chkStandard.DataBind.SetBound(true, "", "chkStn");
                               

                //cbLeaveType = oForm.Items.Item("cbLTyp").Specific;
                //cbPayrollPeriod = oForm.Items.Item("cbPeriod").Specific;
                cbLeaveType = oForm.Items.Item("cbLTyp").Specific;
                oForm.DataSources.UserDataSources.Add("cbLTyp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLeaveType.DataBind.SetBound(true, "", "cbLTyp");
                icbLeaveType = oForm.Items.Item("cbLTyp");

                cbPayrollPeriod = oForm.Items.Item("cbPeriod").Specific;
                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPayrollPeriod.DataBind.SetBound(true, "", "cbPeriod");
                icbPayrollPeriod = oForm.Items.Item("cbPeriod");


                // Initialize TextBoxes

                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;
                txtEmpCode.DataBind.SetBound(true, "", "txtEmpC");

                oForm.DataSources.UserDataSources.Add("txtamnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtAmount = oForm.Items.Item("txtamnt").Specific;
                txtAmount.DataBind.SetBound(true, "", "txtamnt");

                oForm.DataSources.UserDataSources.Add("txtsal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtsal = oForm.Items.Item("txtsal").Specific;
                txtsal.DataBind.SetBound(true, "", "txtsal");

                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpName = oForm.Items.Item("txtEmpN").Specific;
                txtEmpName.DataBind.SetBound(true, "", "txtEmpN");


                //oForm.DataSources.UserDataSources.Add("txtFdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                //txtFromDate = oForm.Items.Item("txtFdt").Specific;
                //txtFromDate.DataBind.SetBound(true, "", "txtFdt");

                //oForm.DataSources.UserDataSources.Add("txtTdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                //txtToDate = oForm.Items.Item("txtTdt").Specific;
                //txtToDate.DataBind.SetBound(true, "", "txtTdt");


                oForm.DataSources.UserDataSources.Add("txtTotl", SAPbouiCOM.BoDataType.dt_SUM);
                txtTotal = oForm.Items.Item("txtTotl").Specific;
                txtTotal.DataBind.SetBound(true, "", "txtTotl");



                /*---------------------------------*/

                oForm.DataSources.UserDataSources.Add("txtBF", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtBF = oForm.Items.Item("txtBF").Specific;
                txtBF.DataBind.SetBound(true, "", "txtBF");

                oForm.DataSources.UserDataSources.Add("txtEnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtEntitled = oForm.Items.Item("txtEnt").Specific;
                txtEntitled.DataBind.SetBound(true, "", "txtEnt");

                oForm.DataSources.UserDataSources.Add("txtTavb", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtTotalAvb = oForm.Items.Item("txtTavb").Specific;
                txtTotalAvb.DataBind.SetBound(true, "", "txtTavb");

                oForm.DataSources.UserDataSources.Add("txtUsed", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtUsed = oForm.Items.Item("txtUsed").Specific;
                txtUsed.DataBind.SetBound(true, "", "txtUsed");

                oForm.DataSources.UserDataSources.Add("txtReq", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtRequeted = oForm.Items.Item("txtReq").Specific;
                txtRequeted.DataBind.SetBound(true, "", "txtReq");

                oForm.DataSources.UserDataSources.Add("txtapp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtApproved = oForm.Items.Item("txtapp").Specific;
                txtApproved.DataBind.SetBound(true, "", "txtapp");

                oForm.DataSources.UserDataSources.Add("txtBal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtBalance = oForm.Items.Item("txtBal").Specific;
                txtBalance.DataBind.SetBound(true, "", "txtBal");


                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLeaveTypeCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLeaveType);
                var Data = (from v in dbHrPayroll.MstLeaveType
                            where v.Encash == true
                            select v).ToList();
                cbLeaveType.ValidValues.Add("-1", "[Select One]");
                foreach (var v in Data)
                {
                    cbLeaveType.ValidValues.Add(v.Code, v.Description);
                }
                cbLeaveType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FindLeaveBalance()
        {
            try
            {
                String iApprovedCode = "LV0006", iDraftCode = "LV0005";
                decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0;
                string strEmpCode = txtEmpCode.Value;
                string strLeaveType = "";
                if (!string.IsNullOrEmpty(cbLeaveType.Value) && cbLeaveType.Value != "-1")
                {
                    strLeaveType = cbLeaveType.Value;
                }
                if (!string.IsNullOrEmpty(strEmpCode) && !string.IsNullOrEmpty(strLeaveType))
                {
                    var oEMP = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();
                    //int intLeaveID = dbHrPayroll.MstLeaveType.Where(l => l.Code == strLeaveType).FirstOrDefault().ID;
                    var OintLeaveID = dbHrPayroll.MstLeaveType.Where(l => l.Code == strLeaveType).FirstOrDefault();
                    int intLeaveID = 0;
                    if (OintLeaveID != null)
                    {
                        intLeaveID = OintLeaveID.ID;
                    }
                    else
                    {
                        return;
                    }
                    //Enchanced Version MFM
                    MstCalendar oCal = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();
                    if (oCal == null)
                    {
                        oApplication.StatusBar.SetText("Select default calendar.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    MstEmployeeLeaves oEmpLeave = oEMP.MstEmployeeLeaves.Where(el => el.LeaveType == intLeaveID && el.FromDt == oCal.StartDate && el.ToDt == oCal.EndDate).FirstOrDefault();
                    //End of Enchancement
                    //MstEmployeeLeaves oEmpLeave = oEMP.MstEmployeeLeaves.Where(el => el.LeaveType == intLeaveID).FirstOrDefault();
                    if (oEmpLeave != null)
                    {
                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == iDraftCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                        {
                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                        }
                        //var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == iApprovedCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                        var ApprovedLeavesRecords = (from a in dbHrPayroll.TrnsLeavesRequest
                                                     where a.EmpID == oEMP.ID && a.LeaveType == intLeaveID
                                                     && a.DocAprStatus == iApprovedCode && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate
                                                     select a).ToList();
                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                        {
                            ApprovedLeaves = ApprovedLeavesRecords.Sum(a => a.TotalCount);
                        }
                        if (oEmpLeave.LeavesCarryForward != null)
                        {
                            LeaveCarryForward = oEmpLeave.LeavesCarryForward;
                        }
                        if (oEmpLeave.LeavesEntitled != null)
                        {
                            LeaveEntitled = oEmpLeave.LeavesEntitled;
                        }
                        if (LeaveCarryForward != null && LeaveEntitled != null)
                        {
                            TotalAvailable = LeaveCarryForward + LeaveEntitled;
                        }
                        if (oEmpLeave.LeavesUsed != null)
                        {
                            LeaveUsed = oEmpLeave.LeavesUsed;
                        }
                    }
                }
                txtBF.Value = String.Format("{0:0.00}", LeaveCarryForward);// Convert.ToString(LeaveCarryForward);
                txtEntitled.Value = String.Format("{0:0.00}", LeaveEntitled);// Convert.ToString(LeaveEntitled);
                txtTotalAvb.Value = String.Format("{0:0.00}", TotalAvailable);// Convert.ToString(TotalAvailable);
                txtUsed.Value = String.Format("{0:0.00}", LeaveUsed);
                txtApproved.Value = String.Format("{0:0.00}", ApprovedLeaves);
                txtRequeted.Value = String.Format("{0:0.00}", RequestedLeaves);
                deductedLeaves = RequestedLeaves + ApprovedLeaves + LeaveUsed;
                txtBalance.Value = String.Format("{0:0.00}", TotalAvailable - deductedLeaves);// Convert.ToString(TotalAvailable - deductedLeaves);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                //Program.sqlString = "empLeaveReq";
                //string comName = "Search";
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
                    txtEmpCode.Value = Program.EmpID;
                    MstEmployee EmpRecord = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpCode.Value.Trim() select a).FirstOrDefault();
                    if (EmpRecord != null)
                    {
                        txtEmpName.Value = EmpRecord.FirstName + " " + EmpRecord.MiddleName + " " + EmpRecord.LastName;
                        FillPayrollPeriods(EmpRecord.PayrollID.Value);
                        txtsal.Value = String.Format("{0:0.00}", EmpRecord.BasicSalary);
                        cbLeaveType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    }
                    //LoadSelectedData(txtEmpCode.Value);                   
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void CalculateTotalRequstedLeaves()
        {
            try
            {
                double LeaveDays = Convert.ToDouble(txtTotal.Value.Trim());
                string dayName = "";
                //if (string.IsNullOrEmpty(txtFromDate.Value) || string.IsNullOrEmpty(txtToDate.Value))
                //{
                //    return;
                //}
                //else
                //{
                //    DateTime dtFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //    DateTime dtTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //    LeaveDays = ((dtTo.Subtract(dtFrom)).TotalDays + 1);
                if (LeaveDays > 0)
                {
                    txtTotal.Value = Convert.ToString(LeaveDays);
                    EncashAmount();
                }
                //}
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddLeaveEncashmentElemntValue()
        {
            try
            {
                string leaveType = cbLeaveType.Value;
                string days = txtTotal.Value;
                decimal ReqLeaves = 0, decBalance = 0;
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                ReqLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);

                int perioddays = 0;
                decimal salary = 0.0m;

                string PayrollPeriod = cbPayrollPeriod.Value.Trim();
                if (string.IsNullOrEmpty(PayrollPeriod))
                {
                    oApplication.StatusBar.SetText("Please select Valid Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(leaveType))
                {
                    oApplication.StatusBar.SetText("Please select Valid Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves > decBalance)
                {
                    oApplication.StatusBar.SetText("Requested Leave(s) can't be greater than Balance Leaves", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves <= 0)
                {
                    oApplication.StatusBar.SetText("Requested Leave(s) can't be less than equal to 0", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                CfgPeriodDates LeaveFromPeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                if (LeaveFromPeriod != null)
                {
                    if (perioddays < 1)
                    {
                        perioddays = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                    }
                    else
                    {
                        perioddays = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                    }
                }
                var LeaveType = dbHrPayroll.MstLeaveType.Where(l => l.Code == leaveType).FirstOrDefault();
                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault();
                empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == txtEmpCode.Value.ToString() select p).Single();

                var ElementID = dbHrPayroll.MstElements.Where(e => e.ElementName == LeaveType.EncashElement).FirstOrDefault();


                if (ElementID != null)
                {
                    var trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.EmpElmtId == empEle.Id && p.ElementId == ElementID.Id select p).FirstOrDefault();
                    if (trntEle.ValueType == "POB")
                    {
                        salary = EmpRecord.BasicSalary.Value;
                    }
                    else if (trntEle.ValueType == "POG")
                    {
                        salary = (decimal)ds.getEmpGross(EmpRecord);
                    }
                    else if (trntEle.ValueType == "FIX")
                    {
                        salary = (decimal)trntEle.Value;
                    }
                    //string calcAmount = String.Format("{0:0}", CalculateAmount(salary, trntEle.Value.Value, Convert.ToInt32(days), perioddays));
                    trntEle.Amount = Convert.ToDecimal(txtAmount.Value);
                    trntEle.PeriodId = LeaveFromPeriod.ID;
                    trntEle.FlgOneTimeConsumed = false;
                    var EmpLeaves = dbHrPayroll.MstEmployeeLeaves.Where(l => l.EmpID == EmpRecord.ID && l.LeaveType == LeaveType.ID).FirstOrDefault();
                    decimal LeaveUsed = EmpLeaves.LeavesUsed;
                    LeaveUsed = LeaveUsed + ReqLeaves;
                    EmpLeaves.LeavesUsed = LeaveUsed;
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception Ex)
            {

            }
        }

        private Boolean AddValidation()
        {
            try
            {
                string previosperiodid = "";
                string selectedperiodid = "";
                DateTime StartDate, EndDate;
                //if (string.IsNullOrEmpty(txtFromDate.Value))
                //{
                //    oApplication.StatusBar.SetText("From Date is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return false;
                //}
                //if (string.IsNullOrEmpty(txtToDate.Value))
                //{
                //    oApplication.StatusBar.SetText("To Date is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return false;
                //}
                //StartDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //if (StartDate > EndDate)
                //{
                //    oApplication.StatusBar.SetText("From date could not be greater then to date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return false;
                //}
                if (string.IsNullOrEmpty(txtAmount.Value))
                {
                    oApplication.StatusBar.SetText("Document can't with zero amount.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                oApplication.StatusBar.SetText("AddValidation().", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return false;
            }
        }

        private Boolean AddValidationUAE()
        {
            try
            {
                string previosperiodid = "";
                string selectedperiodid = "";
                string PayrollPeriod = cbPayrollPeriod.Value.Trim();
                
                var oEmployee = (from a in dbHrPayroll.MstEmployee 
                                 where a.EmpID == Convert.ToString(txtEmpCode.Value) 
                                 select a).FirstOrDefault();

                var oLeaveEncashment = (from a in dbHrPayroll.TrnsleaveEncashment 
                                        where a.PeriodID == Convert.ToInt32(PayrollPeriod) 
                                        && a.EmpID == oEmployee.ID 
                                        select a).FirstOrDefault();

                if (oLeaveEncashment != null)
                {
                    oApplication.StatusBar.SetText("Multiple leave encashments can't be entered in current period: '" + oLeaveEncashment.PeriodName + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                var OPayrollCode = (from a in dbHrPayroll.CfgPeriodDates 
                                    where a.ID == Convert.ToInt32(PayrollPeriod) 
                                    select a).FirstOrDefault();

                if (OPayrollCode != null)
                {

                    selectedperiodid =  OPayrollCode.CalCode;
                    previosperiodid = "";// OPayrollCode.CalCode;
                    var OPayrollCalendar = (from a in dbHrPayroll.MstCalendar select a).ToList();
                    int OPayrollCalendarCount = (from a in dbHrPayroll.MstCalendar select a).Count();
                    if (OPayrollCalendar != null)
                    {
                        for (int j = 0; j < OPayrollCalendar.Count; j++)
                        {
                            if (selectedperiodid == OPayrollCalendar[j].Code)
                            {
                                if (OPayrollCalendarCount > 1)
                                {
                                    previosperiodid = OPayrollCalendar[j - 1].Code;
                                }
                                else
                                {
                                    selectedperiodid = OPayrollCalendar[j].Code;
                                }
                            }
                        }
                    }

                    var oPayrollCurrent = (from a in dbHrPayroll.CfgPeriodDates 
                                           where a.CalCode == selectedperiodid 
                                           select a).FirstOrDefault();

                    var oPayrollPrevious = (from a in dbHrPayroll.CfgPeriodDates
                                            where a.CalCode == previosperiodid 
                                            select a).FirstOrDefault();
                    if (chkStandard.Checked == false)
                    {
                        if (oPayrollPrevious != null)
                        {
                            oApplication.StatusBar.SetText("Leave Encashment Avail In Previous Year.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                oApplication.StatusBar.SetText("AddValidationUAE().", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return false;
            }
        }

        private void AddLeaveEncashmentToTable()
        {
            try
            {

                string leaveType = cbLeaveType.Value;
                string days = txtTotal.Value;
                decimal ReqLeaves = 0, decBalance = 0;
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                ReqLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);
                string strLeaveType = cbLeaveType.Value;
                string PayrollPeriod = cbPayrollPeriod.Value.Trim();

                //Previous
                CfgPeriodDates oCfgPeriods = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                var oLeaveType = dbHrPayroll.MstLeaveType.Where(l => l.Code == leaveType).FirstOrDefault();
                var oEmployee = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault();
                empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == txtEmpCode.Value.ToString() select p).Single();

                var ElementID = dbHrPayroll.MstElements.Where(e => e.ElementName == oLeaveType.EncashElement).FirstOrDefault();
                if (ElementID != null)
                {
                    var ObjLeaveEncashment = (from p in dbHrPayroll.TrnsleaveEncashment where p.EmpID == empEle.Id && p.ElementID == ElementID.Id select p).FirstOrDefault();

                    if (ObjLeaveEncashment == null)
                    {

                        ObjLeaveEncashment = new TrnsleaveEncashment();
                        dbHrPayroll.TrnsleaveEncashment.InsertOnSubmit(ObjLeaveEncashment);


                        ObjLeaveEncashment.EmpID = oEmployee.ID;
                        ObjLeaveEncashment.EmpName = txtEmpName.Value.Trim();
                        ObjLeaveEncashment.BasicSalary = oEmployee.BasicSalary;
                        // var oCfgPeriods = dbHrPayroll.CfgPeriodDates.Where(a => a.ID == Convert.ToInt32(cbPayrollPeriod.Value.Trim())).FirstOrDefault();
                        ObjLeaveEncashment.PeriodID = oCfgPeriods.ID;
                        ObjLeaveEncashment.PeriodName = oCfgPeriods.PeriodName;
                        //var oLeaveType = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == strLeaveType).Single();
                        ObjLeaveEncashment.LeaveID = oLeaveType.ID;
                        //ObjLeaveEncashment.FromDt = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        //ObjLeaveEncashment.ToDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        ObjLeaveEncashment.TotalLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);// Convert.ToDecimal(txtTotal.Value.Trim());
                        ObjLeaveEncashment.TotalAmount = string.IsNullOrEmpty(txtAmount.Value) ? 0 : Convert.ToDecimal(txtAmount.Value.Trim());
                        ObjLeaveEncashment.BalanceBF = string.IsNullOrEmpty(txtBF.Value) ? 0 : Convert.ToDecimal(txtBF.Value.Trim());
                        ObjLeaveEncashment.Entitled = string.IsNullOrEmpty(txtEntitled.Value) ? 0 : Convert.ToDecimal(txtEntitled.Value.Trim());
                        ObjLeaveEncashment.TotalAvailable = string.IsNullOrEmpty(txtTotalAvb.Value) ? 0 : Convert.ToDecimal(txtTotalAvb.Value.Trim());
                        ObjLeaveEncashment.LeaveUsed = string.IsNullOrEmpty(txtUsed.Value) ? 0 : Convert.ToDecimal(txtUsed.Value.Trim());
                        ObjLeaveEncashment.Requested = string.IsNullOrEmpty(txtRequeted.Value) ? 0 : Convert.ToDecimal(txtRequeted.Value.Trim());
                        ObjLeaveEncashment.Approved = string.IsNullOrEmpty(txtApproved.Value) ? 0 : Convert.ToDecimal(txtApproved.Value.Trim());
                        ObjLeaveEncashment.Balance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value.Trim());
                        ObjLeaveEncashment.CreatedDt = DateTime.Now;
                        ObjLeaveEncashment.CreatedBy = oCompany.UserName;


                        dbHrPayroll.SubmitChanges();

                        oApplication.StatusBar.SetText("Leave Encashment Record saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        ClearFieldRecords();
                    }
                }
            }
            catch (Exception Ex)
            {

            }
        }

        private void AddLeaveEncashmentElemntValueNEW()
        {
            try
            {
                //AddLeaveEncashmentToTable();
                string leaveType = cbLeaveType.Value.Trim();
                string days = txtTotal.Value;
                decimal ReqLeaves = 0, decBalance = 0;
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                ReqLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);

                int perioddays = 0;
                decimal salary = 0.0m;

                string PayrollPeriod = cbPayrollPeriod.Value.Trim();
                if (string.IsNullOrEmpty(PayrollPeriod))
                {
                    oApplication.StatusBar.SetText("Please select Valid Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(leaveType))
                {
                    oApplication.StatusBar.SetText("Please select Valid Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves > decBalance)
                {
                    oApplication.StatusBar.SetText("Requested Leave(s) can't be greater than Balance Leaves", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves <= 0)
                {
                    oApplication.StatusBar.SetText("Requested Leave(s) can't be less than equal to 0", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                CfgPeriodDates LeaveFromPeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                var LeaveType = dbHrPayroll.MstLeaveType.Where(l => l.Code == leaveType).FirstOrDefault();
                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault();
                empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == txtEmpCode.Value.ToString() select p).Single();

                var ElementID = dbHrPayroll.MstElements.Where(e => e.ElementName == LeaveType.EncashElement).FirstOrDefault();
                if (ElementID != null)
                {
                    var trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.EmpElmtId == empEle.Id && p.ElementId == ElementID.Id select p).FirstOrDefault();

                    if (trntEle == null)
                    {
                        trntEle = new TrnsEmployeeElementDetail();
                        empEle.TrnsEmployeeElementDetail.Add(trntEle);
                    }
                    trntEle.Amount = Convert.ToDecimal(txtAmount.Value);
                    trntEle.PeriodId = LeaveFromPeriod.ID;
                    trntEle.FlgOneTimeConsumed = false;
                    trntEle.FlgActive = true;
                    var EmpLeaves = dbHrPayroll.MstEmployeeLeaves.Where(l => l.EmpID == EmpRecord.ID && l.LeaveType == LeaveType.ID).FirstOrDefault();
                    decimal LeaveUsed = EmpLeaves.LeavesUsed;
                    LeaveUsed = LeaveUsed + ReqLeaves;
                    EmpLeaves.LeavesUsed = LeaveUsed;


                    //dbHrPayroll.SubmitChanges();

                    //oApplication.StatusBar.SetText("Leave Encashment Record saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    AddLeaveEncashmentToTable();

                    //ClearFieldRecords();
                }
            }
            catch (Exception Ex)
            {

            }
        }

        private decimal CalculateAmount(decimal salry, decimal value, decimal days, decimal perioddays)
        {
            try
            {
                decimal amount = 0;
                amount = (salry / perioddays) * (value / 100) * days;
                amount = Math.Round(amount);
                return amount;
            }
            catch (Exception Ex)
            {
                return 0.0M;
            }
        }

        private decimal CalculateAmountBaseofYearly(decimal salary, int daysinYear, int Months, int days)
        {
            try
            {
                decimal perdaySalary = 0;
                decimal amount = 0;
                perdaySalary = salary * Months / daysinYear;
                amount = perdaySalary * days;
                return amount;
            }
            catch (Exception Ex)
            {
                return 0.0M;
            }
        }

        private void EncashAmount()
        {
            try
            {
                int perioddays = 0;
                decimal salary = 0.0m;
                decimal ReqLeaves = 0, decBalance = 0;
                int intdaysInYear = 0, intMonths = 0;
                string PayrollPeriod = cbPayrollPeriod.Value.Trim();
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                ReqLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);
                string leaveType = cbLeaveType.Value.Trim();
                if (string.IsNullOrEmpty(PayrollPeriod))
                {
                    oApplication.StatusBar.SetText("Please select valid payroll period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(leaveType))
                {
                    oApplication.StatusBar.SetText("Please select valid leave type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves > decBalance)
                {
                    oApplication.StatusBar.SetText("Requested leave(s) can't be greater than balance leaves", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves <= 0)
                {
                    oApplication.StatusBar.SetText("Requested Leave(s) can't be less than equal to 0", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                CfgPeriodDates LeaveFromPeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                if (LeaveFromPeriod != null)
                {
                    if (perioddays < 1)
                    {
                        //perioddays = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                        double Days = (LeaveFromPeriod.EndDate.Value - LeaveFromPeriod.StartDate.Value).TotalDays;
                        perioddays = Convert.ToInt32(Days);
                        perioddays = perioddays + 1;
                    }
                    else
                    {
                        perioddays = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                    }
                }
                var LeaveType = dbHrPayroll.MstLeaveType.Where(l => l.Code == leaveType).FirstOrDefault();
                if (leaveType != null)
                {
                    intdaysInYear = LeaveType.DaysinYear == null ? 0 : LeaveType.DaysinYear.Value;
                    intMonths = LeaveType.Months == null ? 0 : LeaveType.Months.Value;
                }
                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault();
                empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == txtEmpCode.Value.ToString() select p).Single();

                var ElementID = dbHrPayroll.MstElements.Where(e => e.ElementName == LeaveType.EncashElement).FirstOrDefault();
                if (ElementID != null)
                {
                    var trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.EmpElmtId == empEle.Id && p.ElementId == ElementID.Id select p).FirstOrDefault();
                    if (trntEle == null)
                    {
                        oApplication.StatusBar.SetText("Please attach element : " + ElementID.Description + " to selected Employee in employee element transaction.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    //if (trntEle.ValueType == "POB")
                    if (trntEle.MstElements.MstElementEarning[0].ValueType == "POB")
                    {
                        salary = EmpRecord.BasicSalary.Value;
                    }
                    //else if (trntEle.ValueType == "POG")
                    else if (trntEle.MstElements.MstElementEarning[0].ValueType == "POG")
                    {
                        salary = (decimal)ds.getEmpGross(EmpRecord);
                    }
                    //else if (trntEle.ValueType == "FIX")
                    else if (trntEle.MstElements.MstElementEarning[0].ValueType == "FIX")
                    {
                        salary = (decimal)trntEle.Value;
                    }
                    if (intdaysInYear > 0 && intMonths > 0)
                    {
                        decimal decAmount = CalculateAmountBaseofYearly(salary, intdaysInYear, intMonths, Convert.ToInt32(ReqLeaves));
                        string calcAmount = string.Format("{0:0}", decAmount);
                        txtAmount.Value = calcAmount;
                    }
                    else
                    {
                        //string calcAmount = String.Format("{0:0}", CalculateAmount(salary, trntEle.Value.Value, Convert.ToInt32(ReqLeaves), perioddays));
                        string calcAmount = String.Format("{0:0}", CalculateAmount(salary, Convert.ToDecimal(trntEle.MstElements.MstElementEarning[0].Value), Convert.ToDecimal(ReqLeaves), Convert.ToDecimal(perioddays)));
                        //string calcAmount = String.Format("{0:0}", CalculateAmount(salary, Convert.ToDecimal(trntEle.MstElements.MstElementEarning[0].Value), Convert.ToInt32(ReqLeaves), perioddays));
                        txtAmount.Value = calcAmount;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ClearFieldRecords()
        {
            try
            {
                txtEmpCode.Value = string.Empty;
                txtEmpName.Value = string.Empty;
                txtsal.Value = string.Empty;
                cbLeaveType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbPayrollPeriod.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //txtFromDate.Value = string.Empty;
                //txtToDate.Value = string.Empty;               
                txtTotal.Value = string.Empty;
                txtAmount.Value = string.Empty;
                // Balance
                txtBF.Value = string.Empty;
                txtEntitled.Value = string.Empty;
                txtTotalAvb.Value = string.Empty;
                txtUsed.Value = string.Empty;
                txtRequeted.Value = string.Empty;
                txtApproved.Value = string.Empty;
                txtBalance.Value = string.Empty;


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

    }
}
