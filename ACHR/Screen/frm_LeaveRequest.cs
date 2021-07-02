using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_LeaveRequest : HRMSBaseForm
    {

        #region Variable

        // private SAPbouiCOM.Application oApplication;

        SAPbouiCOM.Button btSave, btCancel;
        SAPbouiCOM.EditText txtDocNum, txtEmpCode, txtEmpName, txtFromDate, txtToDate, txtTotal, txtApprocalStatus, txtDocStatus, txtBF, txtEntitled, txtTotalAvb, txtTotalAccured, txtUsed, txtRequeted, txtApproved, txtBalance, txtRemarks, txtLeaveAdjustmentDate;
        SAPbouiCOM.EditText txtUnitsQty;
        SAPbouiCOM.Item ItxtUnitsQty, IcbLaeveType, IcbUnits;
        private SAPbouiCOM.ComboBox cbLaeveType, cbUnits;
        SAPbouiCOM.CheckBox cbweek;

        int UnitValue = 1;
        int UnitsInRequest = 0;

        public class LeavesCalculated
        {
            public string LeaveType;
            public decimal LeaveCount;
            public decimal DocumentNo;
            public DateTime FromDate;
            public DateTime ToDate;
        }

        List<LeavesCalculated> oLeavesToEnter = new List<LeavesCalculated>();

        #endregion

        #region B1 Form Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            //oForm.EnableMenu("1282", false);  // Add New Record
            oForm.EnableMenu("1288", false);  // Next Record
            oForm.EnableMenu("1289", false);  // Pevious Record
            oForm.EnableMenu("1290", false);  // First Record
            oForm.EnableMenu("1291", false);  // Last record 
            InitiallizeForm();
            FillLeaveTypeCombo();
            FillLeaveUnitsCombo();
            oForm.Freeze(false);
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "cbUnits":
                case "txtFdt":
                case "txtTdt":
                    if (cbUnits != null && oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        FillUnitsValues();
                        RequestedLeaveCount();
                    }
                    break;
                case "txunits":
                    RequestedLeaveCount();
                    if (Program.systemInfo.FlgArabic == true)
                    {
                        if (cbUnits.Value != "-1")
                        {
                            if (cbLaeveType.Selected != null)
                            {
                                if (cbLaeveType.Selected.Value.ToLower().StartsWith("an"))
                                {
                                    //ValidationAccured();
                                }
                            }
                        }
                    }
                    break;
                case "cbLTyp":
                    if (cbLaeveType != null && oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                        if (Program.systemInfo.FlgLeaveCalendar.GetValueOrDefault() == true)
                        {
                            FindLeaveBalanceByLC();
                        }
                        else
                        {
                            FindLeaveBalance();
                        }
                    break;

                default:
                    break;
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
                            //AddLeaveRequest();
                            if (Program.systemInfo.FlgArabic == true)
                            {
                                string vale = cbLaeveType.Selected.Value;
                                if (cbLaeveType.Selected.Value.ToLower().StartsWith("sl"))
                                {
                                    AddLeaveRequestMFM();
                                }
                                else
                                {
                                    AddLeaveRequest();
                                }
                            }
                            else
                            {
                                AddLeaveRequest();
                            }
                        break;
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LeaveRequest Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (Program.systemInfo.FlgArabic == true)
                    {
                        if (cbLaeveType.Selected.Value.ToLower().StartsWith("sl"))
                        {
                            if (!ValidationAddUAE())
                            {
                                BubbleEvent = false;
                            }
                        }
                        else
                        {
                            if (!ValidationAccured())
                            {
                                BubbleEvent = false;
                            }
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

        #region Local Methods

        private void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("2").Specific;

                //cbPayrollPeriod = oForm.Items.Item("cbPeriod").Specific;
                //oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                //cbPayrollPeriod.DataBind.SetBound(true, "", "cbPeriod");
                //IcbPayrollPeriod = oForm.Items.Item("cbPeriod");

                cbLaeveType = oForm.Items.Item("cbLTyp").Specific;
                oForm.DataSources.UserDataSources.Add("cbLTyp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                cbLaeveType.DataBind.SetBound(true, "", "cbLTyp");
                IcbLaeveType = oForm.Items.Item("cbLTyp");


                cbUnits = oForm.Items.Item("cbUnits").Specific;
                oForm.DataSources.UserDataSources.Add("cbUnits", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                cbUnits.DataBind.SetBound(true, "", "cbUnits");
                IcbUnits = oForm.Items.Item("cbUnits");


                oForm.DataSources.UserDataSources.Add("cbweek", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbweek = oForm.Items.Item("cbweek").Specific;
                cbweek.DataBind.SetBound(true, "", "cbweek");


                // Initialize TextBoxes

                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;
                txtEmpCode.DataBind.SetBound(true, "", "txtEmpC");

                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpName = oForm.Items.Item("txtEmpN").Specific;
                txtEmpName.DataBind.SetBound(true, "", "txtEmpN");

                oForm.DataSources.UserDataSources.Add("txtDNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtDocNum = oForm.Items.Item("txtDNum").Specific;
                txtDocNum.DataBind.SetBound(true, "", "txtDNum");

                oForm.DataSources.UserDataSources.Add("txtFdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate = oForm.Items.Item("txtFdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "txtFdt");

                oForm.DataSources.UserDataSources.Add("txtTdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate = oForm.Items.Item("txtTdt").Specific;
                txtToDate.DataBind.SetBound(true, "", "txtTdt");



                oForm.DataSources.UserDataSources.Add("txtTotl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtTotal = oForm.Items.Item("txtTotl").Specific;
                txtTotal.DataBind.SetBound(true, "", "txtTotl");

                oForm.DataSources.UserDataSources.Add("txtRSta", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtApprocalStatus = oForm.Items.Item("txtRSta").Specific;
                txtApprocalStatus.DataBind.SetBound(true, "", "txtRSta");

                oForm.DataSources.UserDataSources.Add("txtCStat", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtDocStatus = oForm.Items.Item("txtCStat").Specific;
                txtDocStatus.DataBind.SetBound(true, "", "txtCStat");

                oForm.DataSources.UserDataSources.Add("txunits", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtUnitsQty = oForm.Items.Item("txunits").Specific;
                txtUnitsQty.DataBind.SetBound(true, "", "txunits");
                ItxtUnitsQty = oForm.Items.Item("txunits");


                /*---------------------------------*/

                oForm.DataSources.UserDataSources.Add("txtBF", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtBF = oForm.Items.Item("txtBF").Specific;
                txtBF.DataBind.SetBound(true, "", "txtBF");

                oForm.DataSources.UserDataSources.Add("txtEnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEntitled = oForm.Items.Item("txtEnt").Specific;
                txtEntitled.DataBind.SetBound(true, "", "txtEnt");

                oForm.DataSources.UserDataSources.Add("txtTavb", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtTotalAvb = oForm.Items.Item("txtTavb").Specific;
                txtTotalAvb.DataBind.SetBound(true, "", "txtTavb");

                oForm.DataSources.UserDataSources.Add("txtTacc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtTotalAccured = oForm.Items.Item("txtTacc").Specific;
                txtTotalAccured.DataBind.SetBound(true, "", "txtTacc");

                oForm.DataSources.UserDataSources.Add("txtUsed", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtUsed = oForm.Items.Item("txtUsed").Specific;
                txtUsed.DataBind.SetBound(true, "", "txtUsed");

                oForm.DataSources.UserDataSources.Add("txtReq", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtRequeted = oForm.Items.Item("txtReq").Specific;
                txtRequeted.DataBind.SetBound(true, "", "txtReq");

                oForm.DataSources.UserDataSources.Add("txtapp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtApproved = oForm.Items.Item("txtapp").Specific;
                txtApproved.DataBind.SetBound(true, "", "txtapp");

                oForm.DataSources.UserDataSources.Add("txtBal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtBalance = oForm.Items.Item("txtBal").Specific;
                txtBalance.DataBind.SetBound(true, "", "txtBal");

                //oForm.DataSources.UserDataSources.Add("txtrem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                //txtRemarks = oForm.Items.Item("txtrem").Specific;
                //txtRemarks.DataBind.SetBound(true, "", "txtrem");

                //oForm.DataSources.UserDataSources.Add("txtaddt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                //txtLeaveAdjustmentDate = oForm.Items.Item("txtaddt").Specific;
                //txtLeaveAdjustmentDate.DataBind.SetBound(true, "", "txtaddt");

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadSelectedData(String pCode)
        {
            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID.Trim() == pCode.Trim() select a).FirstOrDefault();
                    if (getEmp == null)
                    {
                        oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (getEmp != null)
                    {
                        txtEmpName.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        //int intIdt = dbHrPayroll.TrnsLeavesRequest.Max(u => u.ID);
                        //int? intIdt = dbHrPayroll.TrnsLeavesRequest.Max(u => (int?)u.ID);
                        int? intIdt = dbHrPayroll.TrnsLeavesRequest.Max(u => (int?)u.ID);
                        //int DocCount = dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                        intIdt = intIdt == null ? 1 : intIdt + 1;
                        txtDocNum.Value = Convert.ToString(intIdt);
                        cbLaeveType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        cbUnits.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LeaveRequest Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLeaveTypeCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLeaveType);

                var Data = (from v in dbHrPayroll.MstLeaveType where v.Active == true && (v.FlgConditionalProcessing != true || v.FlgConditionalProcessing == null) select v).ToList();
                cbLaeveType.ValidValues.Add("-1", "[Select One]");
                foreach (var v in Data)
                {
                    cbLaeveType.ValidValues.Add(v.Code, v.Description);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLeaveUnitsCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLOVE);
                cbUnits.ValidValues.Add("-1", "[Select One]");
                var Data = dbHrPayroll.MstLOVE.Where(LU => LU.Type == "LeaveUnits").ToList();
                foreach (var v in Data)
                {
                    cbUnits.ValidValues.Add(v.Code, v.Value);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillLeaveUnitsCombo" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void RequestedLeaveCount()
        {
            try
            {
                double LeaveDays = 0, Weekends = 0, decLeaveHours = 0;
                string dayName = "";
                string strshiftHours = "", strLeaveHours = "";
                bool IsWeekendInclude = cbweek.Checked;
                string strUnit = cbUnits.Value.Trim();
                string strApprovalStatus = "LV0005", strDocStatus = "LV0001";
                if (string.IsNullOrEmpty(txtFromDate.Value.Trim()) || string.IsNullOrEmpty(txtToDate.Value.Trim()) || string.IsNullOrEmpty(strUnit))
                {
                    return;
                }
                if (strUnit.ToUpper() == "DAY" || strUnit.ToUpper() == "HALFDAY")
                {
                    if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                    {
                        var oEmployee = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpCode.Value.Trim() select a).FirstOrDefault();
                        if (oEmployee == null)
                        {
                            return;
                        }
                        DateTime dtFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime dtTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        //LeaveDays = ((dtTo.Subtract(dtFrom)).TotalDays + 1);
                        for (DateTime x = dtFrom; x <= dtTo; x = x.AddDays(1))
                        {
                            dayName = x.DayOfWeek.ToString();
                            var ShiftDay = (from a in dbHrPayroll.TrnsAttendanceRegister
                                            where a.EmpID == oEmployee.ID && a.Date == x
                                            select a).FirstOrDefault();
                            //var ShiftDayTS = (from a in dbHrPayroll.TrnsAttendanceRegisterTS
                            //                  where a.EmpID == oEmployee.ID && a.Date == x
                            //                  select a).FirstOrDefault();
                            if (ShiftDay == null)
                            {
                                oApplication.StatusBar.SetText("Please assign Shift to selected employee", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                            int ShiftID = 0;
                            //if (ShiftDay == null && ShiftDayTS != null)
                            //{
                            //    ShiftID = Convert.ToInt32(ShiftDayTS.ShiftID);
                            //}
                            //else
                            if (ShiftDay != null)
                            {
                                ShiftID = Convert.ToInt32(ShiftDay.ShiftID);
                            }
                            if (ShiftID == 0)
                            {
                                LeaveDays = 0;
                                oApplication.StatusBar.SetText("You are violating term of attendance processing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                            var ShiftRecord = dbHrPayroll.MstShifts.Where(s => s.Id == ShiftID).FirstOrDefault();
                            var ShiftDetail = ShiftRecord.MstShiftDetails.Where(sd => sd.Day == dayName).FirstOrDefault();
                            if (ShiftDetail == null)
                            {
                                LeaveDays = 0;
                                oApplication.StatusBar.SetText("Shifts Detail record not found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                            else if (string.IsNullOrEmpty(ShiftDetail.Duration))
                            {

                            }
                            else if (ShiftDetail.Duration == "00:00")
                            {
                                Weekends++;
                            }
                            else if (!string.IsNullOrEmpty(oEmployee.EmpCalender))
                            {
                                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                                string SQLHolidays = "SELECT HldCode,Rmrks FROM dbo.HLD1 WHERE HldCode = '" + oEmployee.EmpCalender + "' AND StrDate <= '" + x + "' AND EndDate >= '" + x + "'";
                                oRecSet.DoQuery(SQLHolidays);
                                if (oRecSet.RecordCount > 0)
                                {
                                    Weekends++;
                                }
                                else
                                {
                                    LeaveDays++;
                                }
                            }
                            else
                            {
                                LeaveDays++;
                            }
                        }
                        if (LeaveDays > 0)
                        {
                            if (strUnit.ToUpper() == "HALFDAY")
                            {
                                LeaveDays = LeaveDays / 2;
                            }
                            if (IsWeekendInclude)
                            {
                                LeaveDays = ((dtTo.Subtract(dtFrom)).TotalDays + 1);
                                //LeaveDays += Weekends;
                            }
                            txtTotal.Value = Convert.ToString(LeaveDays);
                            txtDocStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == strDocStatus).FirstOrDefault().Value;
                            txtApprocalStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == strApprovalStatus).FirstOrDefault().Value;
                        }
                        else
                        {
                            //oApplication.MessageBox(Program.objHrmsUI.getStrMsg("InvalidLeaveRequest"));
                            // oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("InvalidLeaveRequest"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            txtTotal.Value = "0";
                        }
                    }
                }
                else if (strUnit.ToUpper() == "HOUR")
                {
                    decimal TotalCount = 0M;
                    var oEmployee = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpCode.Value.Trim() select a).FirstOrDefault();
                    if (oEmployee == null) { return; }
                    DateTime dtFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime dtTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    for (DateTime x = dtFrom; x <= dtTo; x = x.AddDays(1))
                    {
                        dayName = x.DayOfWeek.ToString();
                        var ShiftDay = (from a in dbHrPayroll.TrnsAttendanceRegister
                                        where a.EmpID == oEmployee.ID && a.Date == x
                                        select a).FirstOrDefault();
                        if (ShiftDay == null)
                        {
                            oApplication.StatusBar.SetText("Please Assighn Shift to Selected Employee", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        var ShiftRecord = dbHrPayroll.MstShifts.Where(s => s.Id == ShiftDay.ShiftID).FirstOrDefault();
                        var ShiftDetail = ShiftRecord.MstShiftDetails.Where(sd => sd.Day == dayName).FirstOrDefault();
                        if (ShiftDetail == null)
                        {
                            LeaveDays = 0;
                            oApplication.StatusBar.SetText("Shifts Detail record not found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        else if (!string.IsNullOrEmpty(ShiftDetail.Duration))
                        {
                            strshiftHours = ShiftDetail.Duration;
                        }
                    }
                    decLeaveHours = Convert.ToDouble(txtUnitsQty.Value);
                    TimeSpan ShortDuration = TimeSpan.FromHours(decLeaveHours);
                    string output = ShortDuration.ToString("h\\:mm");
                    int hrs = ShortDuration.Hours;
                    int mint = ShortDuration.Minutes;
                    strLeaveHours = string.Format("{0:00}", hrs) + ':' + string.Format("{0:00}", mint);
                    TotalCount = GetLeaveCountOnMinLeaves(strLeaveHours, strshiftHours);
                    txtTotal.Value = string.Format("{0:0.000}", TotalCount);
                    txtDocStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == strDocStatus).FirstOrDefault().Value;
                    txtApprocalStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == strApprovalStatus).FirstOrDefault().Value;
                }

                else if (strUnit.ToUpper() == "MIN")
                {

                    var oEmployee = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpCode.Value.Trim() select a).FirstOrDefault();
                    if (oEmployee == null) { return; }
                    DateTime dtFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime dtTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    for (DateTime x = dtFrom; x <= dtTo; x = x.AddDays(1))
                    {
                        dayName = x.DayOfWeek.ToString();
                        var ShiftDay = (from a in dbHrPayroll.TrnsAttendanceRegister
                                        where a.EmpID == oEmployee.ID && a.Date == x
                                        select a).FirstOrDefault();
                        if (ShiftDay == null)
                        {
                            oApplication.StatusBar.SetText("Please Assighn Shift to Selected Employee", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        var ShiftRecord = dbHrPayroll.MstShifts.Where(s => s.Id == ShiftDay.ShiftID).FirstOrDefault();
                        var ShiftDetail = ShiftRecord.MstShiftDetails.Where(sd => sd.Day == dayName).FirstOrDefault();
                        if (ShiftDetail == null)
                        {
                            LeaveDays = 0;
                            oApplication.StatusBar.SetText("Shifts Detail record not found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        else if (string.IsNullOrEmpty(ShiftDetail.Duration))
                        {

                        }
                        else if (ShiftDetail.Duration == "00:00")
                        {
                            Weekends++;
                        }
                        else
                        {
                            LeaveDays++;
                        }
                    }
                    if (LeaveDays > 0)
                    {
                        int UnitsInDay = 0;
                        int WorkHourPayroll = 0;
                        int DefinedUnits = 0;
                        int CalculatedUnits = 0;
                        decimal TotalCount = 0M;
                        WorkHourPayroll = Convert.ToInt32(oEmployee.CfgPayrollDefination.WorkHours);
                        UnitsInDay = (WorkHourPayroll * 60) / UnitValue;
                        DefinedUnits = Convert.ToInt32(txtUnitsQty.Value.Trim()) * UnitValue;
                        //if (!IsWeekendInclude)
                        //{
                        //    CalculatedUnits = Convert.ToInt32(DefinedUnits * LeaveDays);
                        //    decimal temp1 = CalculatedUnits / 60;
                        //    TotalCount = temp1 / WorkHourPayroll;
                        //}
                        //else
                        //{
                        //    CalculatedUnits = Convert.ToInt32(DefinedUnits * (LeaveDays + Weekends));
                        //    decimal temp1 = CalculatedUnits / 60;
                        //    TotalCount = temp1 / WorkHourPayroll;
                        //}
                        TotalCount = DefinedUnits;
                        txtTotal.Value = string.Format("{0:0.000}", TotalCount);
                        UnitsInRequest = CalculatedUnits;
                        txtDocStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == strDocStatus).FirstOrDefault().Value;
                        txtApprocalStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == strApprovalStatus).FirstOrDefault().Value;
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Leave Count is Zero", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }
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
                decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0, AllowedAccured = 0;
                string strEmpCode = txtEmpCode.Value;
                string strLeaveType = "";
                if (!string.IsNullOrEmpty(cbLaeveType.Value) && cbLaeveType.Value != "-1")
                {
                    strLeaveType = cbLaeveType.Value.Trim();
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
                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == iDraftCode && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                        {
                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                        }
                        var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == iApprovedCode && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                        {
                            ApprovedLeaves = ApprovedLeavesRecords.FirstOrDefault().Amount;
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
                            LeaveUsed = ApprovedLeaves;
                        }
                    }
                }
                txtBF.Value = String.Format("{0:0.00}", LeaveCarryForward);// Convert.ToString(LeaveCarryForward);
                txtEntitled.Value = String.Format("{0:0.00}", LeaveEntitled);// Convert.ToString(LeaveEntitled);
                txtTotalAvb.Value = String.Format("{0:0.00}", TotalAvailable);// Convert.ToString(TotalAvailable);
                txtUsed.Value = String.Format("{0:0.00}", LeaveUsed);
                txtApproved.Value = String.Format("{0:0.00}", ApprovedLeaves);
                txtRequeted.Value = String.Format("{0:0.00}", RequestedLeaves);
                txtTotalAccured.Value = String.Format("{0:0.00}", AllowedAccured);
                //deductedLeaves = RequestedLeaves + ApprovedLeaves + LeaveUsed;
                deductedLeaves = RequestedLeaves + ApprovedLeaves;
                txtBalance.Value = String.Format("{0:0.00}", TotalAvailable - deductedLeaves);// Convert.ToString(TotalAvailable - deductedLeaves);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FindLeaveBalanceByLC()
        {
            try
            {
                String iApprovedCode = "LV0006", iDraftCode = "LV0005";
                decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0, AllowedAccured = 0;
                string strEmpCode = txtEmpCode.Value;
                string strLeaveType = "";
                if (!string.IsNullOrEmpty(cbLaeveType.Value) && cbLaeveType.Value != "-1")
                {
                    strLeaveType = cbLaeveType.Value.Trim();
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
                    MstLeaveCalendar oCal = (from a in dbHrPayroll.MstLeaveCalendar where a.FlgActive == true select a).FirstOrDefault();
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
                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == iDraftCode && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                        {
                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                        }
                        var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == iApprovedCode && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                        {
                            ApprovedLeaves = ApprovedLeavesRecords.FirstOrDefault().Amount;
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
                            LeaveUsed = ApprovedLeaves;
                        }
                    }
                }
                txtBF.Value = String.Format("{0:0.00}", LeaveCarryForward);// Convert.ToString(LeaveCarryForward);
                txtEntitled.Value = String.Format("{0:0.00}", LeaveEntitled);// Convert.ToString(LeaveEntitled);
                txtTotalAvb.Value = String.Format("{0:0.00}", TotalAvailable);// Convert.ToString(TotalAvailable);
                txtUsed.Value = String.Format("{0:0.00}", LeaveUsed);
                txtApproved.Value = String.Format("{0:0.00}", ApprovedLeaves);
                txtRequeted.Value = String.Format("{0:0.00}", RequestedLeaves);
                txtTotalAccured.Value = String.Format("{0:0.00}", AllowedAccured);
                //deductedLeaves = RequestedLeaves + ApprovedLeaves + LeaveUsed;
                deductedLeaves = RequestedLeaves + ApprovedLeaves;
                txtBalance.Value = String.Format("{0:0.00}", TotalAvailable - deductedLeaves);// Convert.ToString(TotalAvailable - deductedLeaves);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddLeaveRequest()
        {
            DateTime startDate = DateTime.MinValue;
            DateTime EndDate = DateTime.MinValue;
            try
            {

                string strEmpCode = txtEmpCode.Value;
                string strLeaveType = "";
                if (!string.IsNullOrEmpty(cbLaeveType.Value.Trim()) && cbLaeveType.Value.Trim() != "-1")
                {
                    strLeaveType = cbLaeveType.Value.Trim();
                }
                //else
                //{
                //    oApplication.StatusBar.SetText("Please select Valid Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return;
                //}
                decimal ReqLeaves = 0, decBalance = 0;
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                ReqLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);
                //int? intIdt = dbHrPayroll.TrnsLeavesRequest.Max(u => (int?)u.ID);
                //intIdt = intIdt == null ? 1 : intIdt + 1;
                //int DocNum =dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                int DocNum = string.IsNullOrEmpty(txtDocNum.Value) ? 1 : Convert.ToInt32(txtDocNum.Value);//dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                var Emp = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();
                //if (Emp == null)
                //{
                //    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return;
                //}
                if (ReqLeaves > 0)
                {
                    if (!string.IsNullOrEmpty(strLeaveType))
                    {
                        var LeaveType = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == strLeaveType).Single();
                        if (Emp != null && LeaveType != null)
                        {
                            if (ReqLeaves <= decBalance)
                            {
                                var ChechkForLeave = dbHrPayroll.TrnsLeavesRequest.Where(lr => lr.EmpID == Emp.ID).ToList();
                                if (ChechkForLeave != null && ChechkForLeave.Count > 0)
                                {
                                    startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                    {
                                        TrnsLeavesRequest AlreadyEnteredLeave = (from a in dbHrPayroll.TrnsLeavesRequest
                                                                                 where a.LeaveFrom <= x && a.LeaveTo >= x && a.MstEmployee.EmpID == strEmpCode
                                                                                 select a).FirstOrDefault();
                                        if (AlreadyEnteredLeave != null)
                                        {
                                            oApplication.StatusBar.SetText("Leave already entered for date " + x.Date.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            return;
                                        }
                                    }
                                }

                                //int confirm = oApplication.MessageBox("Are you sure you want to Add Leave(s) for Selected Employee? ", 3, "Yes", "No", "Cancel");
                                //if (confirm == 2 || confirm == 3)
                                //{
                                //    return;
                                //} 
                                TrnsLeavesRequest LeaveRequest = new TrnsLeavesRequest();
                                //LeaveRequest.DocNum = DocNum;
                                LeaveRequest.Series = -1;
                                LeaveRequest.EmpID = Emp.ID;
                                LeaveRequest.EmpName = txtEmpName.Value;
                                if (!string.IsNullOrEmpty(cbUnits.Value.Trim()) && cbUnits.Value.Trim() != "0")
                                {
                                    LeaveRequest.UnitsID = cbUnits.Value.Trim();
                                }
                                else
                                {
                                    LeaveRequest.UnitsID = "Day";
                                }

                                LeaveRequest.UnitsLOVType = "LeaveUnits";
                                if (!string.IsNullOrEmpty(txtFromDate.Value))
                                {
                                    LeaveRequest.LeaveFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                }
                                if (!string.IsNullOrEmpty(txtToDate.Value))
                                {
                                    LeaveRequest.LeaveTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                }
                                LeaveRequest.TotalCount = Convert.ToDecimal(txtTotal.Value);
                                LeaveRequest.Units = Convert.ToInt32(UnitsInRequest);
                                LeaveRequest.LeaveType = LeaveType.ID;
                                LeaveRequest.LeaveDescription = LeaveType.Description;
                                LeaveRequest.FlgPaid = false;
                                LeaveRequest.DocDate = DateTime.Now;
                                LeaveRequest.CreateDate = DateTime.Now;
                                LeaveRequest.CreatedBy = oCompany.UserName;
                                dbHrPayroll.TrnsLeavesRequest.InsertOnSubmit(LeaveRequest);
                                dbHrPayroll.SubmitChanges();
                                var oEmpLeave = (from a in dbHrPayroll.MstEmployeeLeaves
                                                 where a.MstEmployee.EmpID == Emp.EmpID && a.MstLeaveType.Code == LeaveType.Code
                                                 select a).FirstOrDefault();
                                if (oEmpLeave != null)
                                {
                                    decimal TotalCount = Convert.ToDecimal(txtTotal.Value);
                                    decimal UsedLeaves = Convert.ToDecimal(txtUsed.Value);
                                    oEmpLeave.LeavesUsed = (TotalCount + UsedLeaves);
                                }
                                dbHrPayroll.SubmitChanges();
                                LeaveRequest.DocNum = LeaveRequest.ID;
                                dbHrPayroll.SubmitChanges();
                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                ClearRecords();
                                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("You don't have enough leave(s). Please recheck your leave balance", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Invalid leave request. Required leave(s) must be greater Than 0.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddLeaveRequestAR()
        {
            DateTime startDate = DateTime.MinValue;
            DateTime EndDate = DateTime.MinValue;
            try
            {

                string strEmpCode = txtEmpCode.Value;
                string strLeaveType = "";
                string selectedSlab = "";
                if (!string.IsNullOrEmpty(cbLaeveType.Value) && cbLaeveType.Value != "-1")
                {
                    strLeaveType = cbLaeveType.Value;
                }
                else
                {
                    oApplication.StatusBar.SetText("Please select Valid Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                decimal ReqLeaves = 0, decBalance = 0;
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                ReqLeaves = string.IsNullOrEmpty(txtTotal.Value) ? 0 : Convert.ToDecimal(txtTotal.Value);
                //int? intIdt = dbHrPayroll.TrnsLeavesRequest.Max(u => (int?)u.ID);
                //intIdt = intIdt == null ? 1 : intIdt + 1;
                //int DocNum =dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                int DocNum = string.IsNullOrEmpty(txtDocNum.Value) ? 1 : Convert.ToInt32(txtDocNum.Value);//dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                var Emp = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).Single();
                if (Emp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (ReqLeaves > 0)
                {
                    if (!string.IsNullOrEmpty(strLeaveType))
                    {
                        var Data = dbHrPayroll.TrnsLeavesRequest.Where(pd => pd.EmpID == Emp.ID).FirstOrDefault();
                        var LeaveType = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == strLeaveType).Single();

                        if (Emp != null && LeaveType != null)
                        {
                            if (ReqLeaves <= decBalance)
                            {
                                decimal slabOne = 0;
                                decimal slabTwo = 0;
                                decimal slabThree = 0;
                                TrnsLeavesRequest LeaveRequest = new TrnsLeavesRequest();
                                var ChechkForLeave = dbHrPayroll.TrnsLeavesRequest.Where(lr => lr.EmpID == Emp.ID).ToList();
                                if (ChechkForLeave != null && ChechkForLeave.Count > 0)
                                {
                                    startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                                    {
                                        TrnsLeavesRequest AlreadyEnteredLeave = (from a in dbHrPayroll.TrnsLeavesRequest
                                                                                 where a.LeaveFrom <= x && a.LeaveTo >= x && a.MstEmployee.EmpID == strEmpCode
                                                                                 select a).FirstOrDefault();
                                        if (AlreadyEnteredLeave != null)
                                        {
                                            oApplication.StatusBar.SetText("Leave Already Entered For Date " + x.Date.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            return;
                                        }
                                    }
                                }
                                var checkProbotion = dbHrPayroll.MstEmployee.Where(a => a.ID == Emp.ID & a.EmployeeContractType == "PROB").FirstOrDefault();
                                {
                                    if (LeaveType.Code.StartsWith("SL"))
                                    {
                                        if (checkProbotion != null)
                                        {
                                            oApplication.StatusBar.SetText("Sick Leaves Not Allowed In Probation Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            return;
                                        }
                                        else
                                        {
                                            decimal LeavesUsed = 0;
                                            decimal LeaveApply = 0;
                                            decimal TotalLeaves = 0;
                                            Int32 previosslabID = 0;
                                            Int32 nextslabID = 0;
                                            LeaveApply = Convert.ToDecimal(txtTotal.Value.Trim());
                                            LeavesUsed = (from a in dbHrPayroll.TrnsLeavesRequest
                                                          where a.EmpID == Emp.ID && a.LeaveType == Data.LeaveType
                                                          select a.TotalCount).Sum() ?? 0M;

                                            TotalLeaves = LeavesUsed + LeaveApply;

                                            var oSlab = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.LeaveCount > TotalLeaves select a).FirstOrDefault();
                                            selectedSlab = Convert.ToString(oSlab.ID);

                                            previosslabID = Convert.ToInt32(oSlab.Periorty) - 1;
                                            nextslabID = Convert.ToInt32(oSlab.Periorty) + 1;
                                            var PreviousSlab = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.Periorty == previosslabID select a).FirstOrDefault();
                                            var NextSlab = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.Periorty == nextslabID select a).FirstOrDefault();

                                            decimal CurrentSlab = 0;
                                            decimal RemaingLeaves = 0;

                                            CurrentSlab = Convert.ToDecimal(oSlab.LeaveCount);
                                            if (oSlab.Periorty > 1)
                                            {
                                                if (LeavesUsed > PreviousSlab.LeaveCount)
                                                {
                                                    RemaingLeaves = Convert.ToDecimal(PreviousSlab.LeaveCount) - CurrentSlab;
                                                    LeaveRequest.TotalCount = RemaingLeaves;
                                                }
                                                if (CurrentSlab > TotalLeaves)
                                                {
                                                    RemaingLeaves = Convert.ToDecimal(PreviousSlab.LeaveCount) - CurrentSlab;
                                                    LeaveRequest.TotalCount = RemaingLeaves;
                                                }
                                            }

                                        }
                                    }

                                }
                                //int confirm = oApplication.MessageBox("Are you sure you want to Add Leave(s) for Selected Employee? ", 3, "Yes", "No", "Cancel");
                                //if (confirm == 2 || confirm == 3)
                                //{
                                //    return;
                                //} 

                                LeaveRequest.DocNum = DocNum;
                                LeaveRequest.Series = -1;
                                LeaveRequest.EmpID = Emp.ID;
                                LeaveRequest.EmpName = txtEmpName.Value;
                                if (!string.IsNullOrEmpty(cbUnits.Value) && cbUnits.Value != "0")
                                {
                                    LeaveRequest.UnitsID = cbUnits.Value;
                                }
                                else
                                {
                                    LeaveRequest.UnitsID = "Day";
                                }

                                LeaveRequest.UnitsLOVType = "LeaveUnits";
                                if (!string.IsNullOrEmpty(txtFromDate.Value))
                                {
                                    LeaveRequest.LeaveFrom = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                }
                                if (!string.IsNullOrEmpty(txtToDate.Value))
                                {
                                    LeaveRequest.LeaveTo = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                }
                                LeaveRequest.TotalCount = Convert.ToDecimal(txtTotal.Value);
                                LeaveRequest.Units = Convert.ToInt32(UnitsInRequest);
                                LeaveRequest.LeaveType = LeaveType.ID;
                                LeaveRequest.LeaveDescription = LeaveType.Description;
                                LeaveRequest.FlgPaid = false;
                                LeaveRequest.DocDate = DateTime.Now;
                                LeaveRequest.CreateDate = DateTime.Now;
                                LeaveRequest.CreatedBy = oCompany.UserName;

                                dbHrPayroll.TrnsLeavesRequest.InsertOnSubmit(LeaveRequest);
                                //dbHrPayroll.SubmitChanges();
                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                ClearRecords();
                                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("You don't have enough Leave(s). Please recheck your Leave Balance", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Invalid Leave Request. Required Leave(s) Must be greater Than 0.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private Boolean ValidationAccured()
        {
            try
            {
                DateTime FromDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DateTime ToDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


                string empcode = txtEmpCode.Value.Trim();
                string leavetype = cbLaeveType.Value.Trim();
                var oLeaveType = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == leavetype).FirstOrDefault();
                var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                DateTime Joindate = Convert.ToDateTime(oEmp.JoiningDate);
                var diffMonths = (ToDate.Month + ToDate.Year * 12) - (Joindate.Month + FromDate.Year * 12);
                string check = Convert.ToString(diffMonths);


                var CurrentPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= FromDate && a.EndDate >= FromDate select a).FirstOrDefault();
                var CurrentPeriod2 = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= ToDate && a.EndDate >= ToDate select a).FirstOrDefault();
                string checkPeriodTodate = "";
                string checkPeriodTodate2 = "";
                checkPeriodTodate = CurrentPeriod.PeriodName;
                checkPeriodTodate2 = CurrentPeriod2.PeriodName;
                if (CurrentPeriod != CurrentPeriod2)
                {
                    oApplication.StatusBar.SetText("Leave Document range can not fall on multiple Periods.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //if (diffMonths >= 6 && diffMonths <= 12)
                //{

                decimal LeavesAvailable = 0;
                decimal chkLeavesEntitled = 0;
                decimal chkLeavesCarryForward = 0;
                chkLeavesEntitled = (decimal)(from a in dbHrPayroll.MstEmployeeLeaves
                                              where a.EmpID == oEmp.ID && a.LeaveType == oLeaveType.ID
                                              select a.LeavesEntitled).FirstOrDefault();

                chkLeavesCarryForward = (decimal)(from a in dbHrPayroll.MstEmployeeLeaves
                                                  where a.EmpID == oEmp.ID && a.LeaveType == oLeaveType.ID
                                                  select a.LeavesCarryForward).FirstOrDefault();
                LeavesAvailable = chkLeavesEntitled + chkLeavesCarryForward;


                decimal LeavesUsed = 0;
                LeavesUsed = (from a in dbHrPayroll.TrnsLeavesRequest
                              where a.EmpID == oEmp.ID && a.LeaveType == oLeaveType.ID
                              select a.TotalCount).Sum() ?? 0M;


                var CurrentCalander = dbHrPayroll.MstCalendar.Where(pr => pr.StartDate <= ToDate & pr.EndDate >= ToDate & pr.FlgActive == true).FirstOrDefault();
                string CurrentPeriodName = "";
                if (CurrentCalander != null)
                {
                    CurrentPeriodName = CurrentCalander.Code;
                }
                var CurrentCalanderPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.CalCode == Convert.ToString(CurrentCalander.Code) && a.StartDate >= ToDate select a).FirstOrDefault();
                var oPeriodList = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == oEmp.CfgPayrollDefination.ID && a.CalCode == CurrentCalander.Code select a).ToList();
                int i = 1;
                int TotalCalanderMonths = oPeriodList.Count;
                foreach (var line in oPeriodList)
                {
                    if (CurrentCalanderPeriod.PeriodName == line.PeriodName)
                    {
                        break;
                    }
                    i++;
                }

                decimal PerMonthAnnualLeavs = 0;
                PerMonthAnnualLeavs = LeavesAvailable / TotalCalanderMonths;
                if (leavetype.ToLower().StartsWith("an") & oLeaveType.AccumulativeCount > 0)
                {
                    string GetCurrentPeriod = "";
                    DateTime GetCurrentPeridDate;

                    GetCurrentPeriod = CurrentCalanderPeriod.PeriodName;
                    GetCurrentPeridDate = Convert.ToDateTime(CurrentCalanderPeriod.StartDate);

                    decimal Monthnumber = 0;
                    Monthnumber = i;

                    decimal AllowedAccuredLeave = 0;
                    AllowedAccuredLeave = (PerMonthAnnualLeavs * Monthnumber) - LeavesUsed;
                    txtTotalAccured.Value = String.Format("{0:0.00}", AllowedAccuredLeave);
                    if (Convert.ToDecimal(txtTotal.Value.Trim()) > Convert.ToDecimal(txtTotalAccured.Value.Trim()))
                    {
                        oApplication.StatusBar.SetText("You don't have enough Accured Leave Balance .", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }

                }
                //}
                return true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ValidationAdd : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }

        }

        private Boolean ValidationAddUAE()
        {

            try
            {
                DateTime FromDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DateTime ToDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                decimal LeaveCount = 0M;
                if (!string.IsNullOrEmpty(txtTotal.Value))
                {
                    LeaveCount = Convert.ToDecimal(txtTotal.Value.Trim());
                }
                string empcode = txtEmpCode.Value.Trim();
                string leavetype = cbLaeveType.Value.Trim();
                var oLeaveType = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == leavetype).FirstOrDefault();

                int DocNum = string.IsNullOrEmpty(txtDocNum.Value) ? 1 : Convert.ToInt32(txtDocNum.Value);//dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                                                                                                          //var CurrentPeriod = dbHrPayroll.CfgPeriodDates.Where(pr => pr.StartDate >= ToDate ).FirstOrDefault();


                //CurrentSystemLeaves = (from a in dbHrPayroll.TrnsLeavesRequest where a.MstEmployee.EmpID == oEmp.EmpID && a.MstLeaveType.Code == leavetype select a.TotalCount).Sum() ?? 0M;
                var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                if (oEmp == null)
                {
                    oApplication.StatusBar.SetText("select employee.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(leavetype))
                {
                    oApplication.StatusBar.SetText("select leave type.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                for (DateTime x = FromDate; x <= ToDate; x = x.AddDays(1))
                {
                    TrnsLeavesRequest AlreadyEnteredLeave = (from a in dbHrPayroll.TrnsLeavesRequest
                                                             where a.LeaveFrom <= x && a.LeaveTo >= x && a.MstEmployee.EmpID == oEmp.EmpID
                                                             select a).FirstOrDefault();
                    if (AlreadyEnteredLeave != null)
                    {
                        oApplication.StatusBar.SetText("Leave Already Entered For Date " + x.Date.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }

                if (oEmp.EmployeeContractType.ToLower() == "prob")
                {
                    if (leavetype.ToLower().Contains("sl"))
                    {
                        oApplication.StatusBar.SetText("Sick leave not allowed on probation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                //Check Leave Balance
                decimal TotalAvailableLeaves = 0, decBalance = 0;
                decBalance = string.IsNullOrEmpty(txtBalance.Value) ? 0 : Convert.ToDecimal(txtBalance.Value);
                TotalAvailableLeaves = string.IsNullOrEmpty(txtTotalAvb.Value) ? 0 : Convert.ToDecimal(txtTotalAvb.Value);
                if ((leavetype.ToLower().Contains("sl")) && (TotalAvailableLeaves == 0))
                {
                    oApplication.StatusBar.SetText("Please assign sick leave to selected employee code # " + oEmp.EmpID, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                var CurrentPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= FromDate && a.EndDate >= FromDate select a).FirstOrDefault();
                var CurrentPeriod2 = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= ToDate && a.EndDate >= ToDate select a).FirstOrDefault();
                string checkPeriodTodate = "";
                string checkPeriodTodate2 = "";
                checkPeriodTodate = CurrentPeriod.PeriodName;
                checkPeriodTodate2 = CurrentPeriod2.PeriodName;
                if (CurrentPeriod != CurrentPeriod2)
                {
                    oApplication.StatusBar.SetText("Leave Document range can not fall on multiple Periods.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                decimal CurrentSystemLeaves = 0;
                decimal AppliedLeaves = 0;
                decimal TotalAfterApplied = 0;
                oLeavesToEnter.Clear();

                CurrentSystemLeaves = (from a in dbHrPayroll.TrnsLeavesRequest where a.MstEmployee.EmpID == oEmp.EmpID && a.MstLeaveType.Code == leavetype select a.TotalCount).Sum() ?? 0M;
                AppliedLeaves = LeaveCount;
                TotalAfterApplied = CurrentSystemLeaves + AppliedLeaves;

                var oSlab1 = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.Periorty == 1 select a).FirstOrDefault();
                var oSlab2 = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.Periorty == 2 select a).FirstOrDefault();
                var oSlab3 = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.Periorty == 3 select a).FirstOrDefault();
                if (TotalAfterApplied <= oSlab1.LeaveCount)
                {
                    LeavesCalculated obj0 = new LeavesCalculated();
                    obj0.LeaveType = leavetype;
                    obj0.DocumentNo = DocNum;
                    obj0.LeaveCount = Convert.ToDecimal(txtTotal.Value);
                    obj0.FromDate = FromDate;
                    obj0.ToDate = ToDate;
                    oLeavesToEnter.Add(obj0);
                }
                else if ((TotalAfterApplied > oSlab1.LeaveCount) && (TotalAfterApplied <= (oSlab2.LeaveCount + oSlab1.LeaveCount)))
                {
                    decimal ExpectedLeaveCount = 0;

                    if (CurrentSystemLeaves > oSlab1.LeaveCount)
                    {
                        ExpectedLeaveCount = TotalAfterApplied - CurrentSystemLeaves;
                    }
                    else
                    {
                        ExpectedLeaveCount = TotalAfterApplied - Convert.ToDecimal(oSlab1.LeaveCount);
                    }
                    if (ExpectedLeaveCount == AppliedLeaves)
                    {
                        LeavesCalculated obj = new LeavesCalculated();
                        obj.DocumentNo = DocNum;
                        obj.LeaveType = oSlab2.DeductableLeave;
                        obj.LeaveCount = ExpectedLeaveCount;
                        obj.FromDate = FromDate;
                        obj.ToDate = FromDate.AddDays(Convert.ToDouble(ExpectedLeaveCount - 1));

                        oLeavesToEnter.Add(obj);
                    }
                    else
                    {
                        decimal RemainingLeaveCount = 0;

                        RemainingLeaveCount = AppliedLeaves - ExpectedLeaveCount;

                        LeavesCalculated obj1 = new LeavesCalculated();
                        obj1.DocumentNo = DocNum;
                        obj1.LeaveType = oSlab1.NonDeductableLeave;
                        obj1.LeaveCount = RemainingLeaveCount;
                        obj1.FromDate = FromDate;
                        obj1.ToDate = FromDate.AddDays(Convert.ToDouble(RemainingLeaveCount - 1));
                        oLeavesToEnter.Add(obj1);

                        LeavesCalculated obj = new LeavesCalculated();
                        obj.DocumentNo = DocNum + 1;
                        obj.LeaveType = oSlab2.DeductableLeave;
                        obj.LeaveCount = ExpectedLeaveCount;
                        obj.FromDate = FromDate.AddDays(Convert.ToDouble(RemainingLeaveCount));
                        obj.ToDate = FromDate.AddDays(Convert.ToDouble(ExpectedLeaveCount + RemainingLeaveCount - 1));
                        oLeavesToEnter.Add(obj);
                    }
                }
                else
                {
                    decimal ExpectedLeaveCount = 0;

                    if (CurrentSystemLeaves > oSlab2.LeaveCount)
                    {
                        ExpectedLeaveCount = TotalAfterApplied - CurrentSystemLeaves;
                    }
                    else
                    {
                        ExpectedLeaveCount = TotalAfterApplied - Convert.ToDecimal(oSlab2.LeaveCount);
                    }
                    if (ExpectedLeaveCount == AppliedLeaves)
                    {
                        LeavesCalculated obj = new LeavesCalculated();
                        obj.DocumentNo = DocNum;
                        obj.LeaveType = oSlab3.DeductableLeave;
                        obj.LeaveCount = ExpectedLeaveCount;
                        obj.FromDate = FromDate;
                        obj.ToDate = FromDate.AddDays(Convert.ToDouble(ExpectedLeaveCount - 1));

                        oLeavesToEnter.Add(obj);
                    }
                    else
                    {
                        //System Leave is Zero
                        if (CurrentSystemLeaves == 0)
                        {
                            decimal RemainingLeaveCount = 0;
                            decimal Slab3LeaveCount = 0, Slab1LeaveCount = 0;
                            RemainingLeaveCount = AppliedLeaves - ExpectedLeaveCount;
                            Slab3LeaveCount = ExpectedLeaveCount - Convert.ToDecimal(oSlab1.LeaveCount);
                            Slab1LeaveCount = ExpectedLeaveCount - Slab3LeaveCount;
                            //Slab1
                            LeavesCalculated obj1 = new LeavesCalculated();
                            obj1.DocumentNo = DocNum;
                            obj1.LeaveType = oSlab1.NonDeductableLeave;
                            obj1.LeaveCount = Slab1LeaveCount;
                            obj1.FromDate = FromDate;
                            obj1.ToDate = FromDate.AddDays(Convert.ToDouble(Slab1LeaveCount - 1));
                            oLeavesToEnter.Add(obj1);
                            //Slab2
                            LeavesCalculated obj2 = new LeavesCalculated();
                            obj2.DocumentNo = DocNum + 1;
                            obj2.LeaveType = oSlab2.DeductableLeave;
                            obj2.LeaveCount = RemainingLeaveCount;
                            obj2.FromDate = FromDate.AddDays(Convert.ToDouble(Slab1LeaveCount));
                            obj2.ToDate = FromDate.AddDays(Convert.ToDouble((RemainingLeaveCount + Slab1LeaveCount) - 1));
                            oLeavesToEnter.Add(obj2);
                            //Slab3
                            LeavesCalculated obj3 = new LeavesCalculated();
                            obj3.DocumentNo = DocNum + 2;
                            obj3.LeaveType = oSlab3.DeductableLeave;
                            obj3.LeaveCount = Slab3LeaveCount;
                            obj3.FromDate = FromDate.AddDays(Convert.ToDouble((RemainingLeaveCount + Slab1LeaveCount)));
                            obj3.ToDate = FromDate.AddDays(Convert.ToDouble((RemainingLeaveCount + Slab1LeaveCount + Slab3LeaveCount) - 1));
                            oLeavesToEnter.Add(obj3);
                        }
                        else // When system current leave has a value
                        {
                            decimal RemainingLeaveCount = 0;
                            decimal Slab2LeaveCount = 0, Slab3LeaveCount = 0, Slab1LeaveCount = 0;
                            RemainingLeaveCount = AppliedLeaves - ExpectedLeaveCount;
                            Slab3LeaveCount = ExpectedLeaveCount - Convert.ToDecimal(oSlab1.LeaveCount);
                            Slab1LeaveCount = ExpectedLeaveCount - Slab3LeaveCount - CurrentSystemLeaves;
                            Slab2LeaveCount = RemainingLeaveCount + CurrentSystemLeaves;
                            //Slab1
                            LeavesCalculated obj1 = new LeavesCalculated();
                            obj1.DocumentNo = DocNum;
                            obj1.LeaveType = oSlab1.NonDeductableLeave;
                            obj1.LeaveCount = Slab1LeaveCount;
                            obj1.FromDate = FromDate;
                            obj1.ToDate = FromDate.AddDays(Convert.ToDouble(Slab1LeaveCount - 1));
                            oLeavesToEnter.Add(obj1);
                            //Slab2
                            LeavesCalculated obj2 = new LeavesCalculated();
                            obj2.DocumentNo = DocNum + 1;
                            obj2.LeaveType = oSlab2.DeductableLeave;
                            obj2.LeaveCount = Slab2LeaveCount;
                            obj2.FromDate = FromDate.AddDays(Convert.ToDouble(Slab1LeaveCount));
                            obj2.ToDate = FromDate.AddDays(Convert.ToDouble((Slab2LeaveCount + Slab1LeaveCount) - 1));
                            oLeavesToEnter.Add(obj2);
                            //Slab3
                            LeavesCalculated obj3 = new LeavesCalculated();
                            obj3.DocumentNo = DocNum + 2;
                            obj3.LeaveType = oSlab3.DeductableLeave;
                            obj3.LeaveCount = Slab3LeaveCount;
                            obj3.FromDate = FromDate.AddDays(Convert.ToDouble((Slab2LeaveCount + Slab1LeaveCount)));
                            obj3.ToDate = FromDate.AddDays(Convert.ToDouble((Slab2LeaveCount + Slab1LeaveCount + Slab3LeaveCount) - 1));
                            oLeavesToEnter.Add(obj3);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ValidationAdd : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
        }

        private Boolean AddValidation()
        {
            try
            {
                string strEmpCode = txtEmpCode.Value.Trim();
                string strLeaveType = "";
                DateTime StartDate, EndDate;
                StartDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == strEmpCode select a).FirstOrDefault();
                var LockedPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == oEmp.PayrollID && a.StartDate <= StartDate && a.EndDate >= StartDate && a.FlgLocked == true select a).FirstOrDefault();
                var CurrentPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == oEmp.PayrollID && a.StartDate <= StartDate && a.EndDate >= StartDate && a.FlgLocked == false select a).FirstOrDefault();
                var CurrentPeriod2 = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == oEmp.PayrollID && a.StartDate <= EndDate && a.EndDate >= EndDate && a.FlgLocked == false select a).FirstOrDefault();
                string checkPeriodTodate = "";
                string checkPeriodTodate2 = "";
                if (CurrentPeriod != null && CurrentPeriod2 != null)
                {
                    checkPeriodTodate = CurrentPeriod.PeriodName;
                    checkPeriodTodate2 = CurrentPeriod2.PeriodName;
                }

                if (string.IsNullOrEmpty(txtEmpCode.Value.Trim()))
                {
                    oApplication.StatusBar.SetText("Employee ID is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (!string.IsNullOrEmpty(cbLaeveType.Value) && cbLaeveType.Value != "-1")
                {
                    strLeaveType = cbLaeveType.Value.Trim();
                }
                else
                {
                    oApplication.StatusBar.SetText("Please select valid leave type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                int DocNum = string.IsNullOrEmpty(txtDocNum.Value) ? 1 : Convert.ToInt32(txtDocNum.Value);//dbHrPayroll.TrnsLeavesRequest.Count() + 1;
                var Emp = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();
                if (Emp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                if (Emp.JoiningDate > StartDate)
                {
                    oApplication.StatusBar.SetText("From date could not be greater then to Join date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (Emp.JoiningDate > EndDate)
                {
                    oApplication.StatusBar.SetText("End date could not be greater then to Join date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtFromDate.Value))
                {
                    oApplication.StatusBar.SetText("From Date is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtToDate.Value))
                {
                    oApplication.StatusBar.SetText("To Date is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                if (StartDate > EndDate)
                {
                    oApplication.StatusBar.SetText("From date could not be greater then to date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                if (CurrentPeriod != CurrentPeriod2)
                {
                    oApplication.StatusBar.SetText("Leave Document range can not fall on multiple Periods.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (LockedPeriod != null)
                {
                    oApplication.StatusBar.SetText("Leave not allowed on locked Periods.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
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

        private Boolean UpdateValidation()
        {
            return false;
        }

        private void AddLeaveRequestMFM()
        {
            try
            {

                string strEmpCode = txtEmpCode.Value;
                string strLeaveType = cbLaeveType.Value;
                var Emp = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();

                int DocNum = string.IsNullOrEmpty(txtDocNum.Value) ? 1 : Convert.ToInt32(txtDocNum.Value);
                foreach (var one in oLeavesToEnter)
                {
                    TrnsLeavesRequest oDoc = new TrnsLeavesRequest();
                    dbHrPayroll.TrnsLeavesRequest.InsertOnSubmit(oDoc);

                    oDoc.LeaveFrom = one.FromDate;
                    oDoc.LeaveTo = one.ToDate;
                    oDoc.TotalCount = one.LeaveCount;
                    //oDoc.DocNum = Convert.ToInt32(one.DocumentNo);
                    oDoc.Series = -1;
                    oDoc.EmpID = Emp.ID;
                    oDoc.EmpName = txtEmpName.Value;
                    if (!string.IsNullOrEmpty(cbUnits.Value) && cbUnits.Value != "0")
                    {
                        oDoc.UnitsID = cbUnits.Value;
                    }
                    else
                    {
                        oDoc.UnitsID = "Day";
                    }

                    oDoc.UnitsLOVType = "LeaveUnits";

                    oDoc.Units = Convert.ToInt32(UnitsInRequest);
                    var oLeaveType = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == one.LeaveType).FirstOrDefault();
                    oDoc.LeaveType = oLeaveType.ID;
                    oDoc.LeaveDescription = oLeaveType.Description;
                    oDoc.FlgPaid = false;
                    oDoc.DocDate = DateTime.Now;
                    oDoc.CreateDate = DateTime.Now;
                    oDoc.CreatedBy = oCompany.UserName;
                    dbHrPayroll.SubmitChanges();
                    oDoc.DocNum = oDoc.ID;
                    dbHrPayroll.SubmitChanges();
                }

                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                ClearRecords();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("AddLeaveRequestMFM : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ClearRecords()
        {
            try
            {
                txtEmpCode.Value = string.Empty;
                txtEmpName.Value = string.Empty;
                txtDocNum.Value = string.Empty;
                cbLaeveType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtFromDate.Value = string.Empty;
                txtToDate.Value = string.Empty;
                cbUnits.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtTotal.Value = string.Empty;
                txtUnitsQty.Value = string.Empty;
                txtApprocalStatus.Value = string.Empty;
                txtDocStatus.Value = string.Empty;
                // Balance
                txtBF.Value = string.Empty;
                txtEntitled.Value = string.Empty;
                txtTotalAvb.Value = string.Empty;
                txtUsed.Value = string.Empty;
                txtRequeted.Value = string.Empty;
                txtApproved.Value = string.Empty;
                txtBalance.Value = string.Empty;
                txtTotalAccured.Value = string.Empty;
                UnitsInRequest = 0;

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("empid", txtEmpCode.Value.ToString());
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
                    LoadSelectedData(txtEmpCode.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void FillUnitsValues()
        {
            try
            {
                var oEmployee = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpCode.Value.Trim() select a).FirstOrDefault();
                if (oEmployee == null) return;
                decimal HourInPayroll = 0;
                int UnitsInOneDay = 0;
                //if (oEmployee.CfgPayrollDefination != null)
                //{
                //    HourInPayroll = Convert.ToInt32(oEmployee.CfgPayrollDefination.WorkHours);
                //}                
                if (!String.IsNullOrEmpty(txtFromDate.Value) && !String.IsNullOrEmpty(txtToDate.Value))
                {
                    DateTime date = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    var ShiftDay = (from c in dbHrPayroll.TrnsAttendanceRegister
                                    where c.EmpID == oEmployee.ID && c.Date == date
                                    select c).FirstOrDefault();
                    if (ShiftDay == null)
                    {
                        oApplication.StatusBar.SetText("Please Assighn Shift to Selected Employee", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    var ShiftDetail = (from s in dbHrPayroll.MstShiftDetails where s.Day == ShiftDay.DateDay && s.ShiftID == ShiftDay.ShiftID select s).FirstOrDefault();
                    if (ShiftDetail != null)
                    {
                        HourInPayroll = Convert.ToDecimal(TimeSpan.Parse(ShiftDetail.Duration).TotalHours);
                    }
                }
                if (cbUnits.Value.Trim().ToUpper() == "DAY" || cbUnits.Value.Trim().ToUpper() == "MIN")
                {
                    UnitsInOneDay = Convert.ToInt16((HourInPayroll * 60) / UnitValue);
                }
                if (cbUnits.Value.Trim().ToUpper() == "HALFDAY")
                {
                    UnitsInOneDay = (Convert.ToInt16((HourInPayroll * 60) / UnitValue)) / 2;
                }
                txtUnitsQty.Value = Convert.ToString(UnitsInOneDay);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillUnitsValues : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private decimal GetLeaveCountOnMinLeaves(string pLeaveHour, string pShiftHour)
        {
            decimal retValue = 0;
            try
            {
                string[] arrLeaveHour = pLeaveHour.Split(':');
                int LeaveHour = (Convert.ToInt32(arrLeaveHour[0]) * 60) + (Convert.ToInt32(arrLeaveHour[1]));
                string[] arrShiftHour = pShiftHour.Split(':');
                int ShiftHour = (Convert.ToInt32(arrShiftHour[0]) * 60) + (Convert.ToInt32(arrShiftHour[1]));
                decimal LeaveCount = (ShiftHour - LeaveHour) / Convert.ToDecimal(ShiftHour);
                retValue = 1 - LeaveCount;
            }
            catch (Exception ex)
            {
                logger(ex);
                retValue = 0;
            }
            return retValue;
        }

        #endregion

    }
}
