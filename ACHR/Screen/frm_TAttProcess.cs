using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_TAttProcess : HRMSBaseForm
    {
        #region "Global Variable Area"

        private bool Validate;
        SAPbouiCOM.Button btnNext, btnSerch, btnClear, btnBack, btnSave, btnID, btnId2;
        SAPbouiCOM.EditText txtEmpIdFrom, txtEmpIdTo, txtFromDate, txtToDate;
        SAPbouiCOM.ComboBox cb_Location, cb_depart, cb_deignation;
        SAPbouiCOM.DataTable dtEmployees, dtAttendance;
        SAPbouiCOM.Matrix grdEmployees, grdAttendance;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clId, clLCount, clNo, EmpCode, clIsNewLeave, EmpName, Desig, Depart, Location, clSfStart, clSfEnd, clshft, clsdate, clsTimeIn, clsTimeOut, clsWHrs, clsOTT, clsOTH, clLT, clsLH, clssHiftHrs, clStatus, clDesc, isSel, clDTp;
        SAPbouiCOM.Item IgrdEmployees, IgrdAttendance, ItxtEmpIdFrom, ItxtEmpIdTo, IbtnID, IbtnId2, Icb_Location, Icb_depart, Icb_deignation, ItxtFromDate, ItxtToDate, IbtnBack, IbtnSave;

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
                FillDepartmentInCombo();
                FillDesignationInCombo();
                FillEmpLocationInCombo();
                FillDayTypeInCombo();                
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    case "btId2":              
                        OpenNewSearchFormTo();
                        break;
                    case "btnSerc":
                        PopulateGridWithFilterExpression();
                        break;
                    case "btnClear":
                        ClearControls();
                        break;
                    case "btnNext":
                        HideFirstVisibleNext();
                        break;
                    case "btnBack":
                        HideNextVisibleFirst();
                        break;
                    case "btnSave":
                        SaveAttendanceRecord();
                        break;
                    case "grd_Emp":
                        if (pVal.ColUID == "isSel" && pVal.Row == 0)
                        {
                            selectAllProcess();
                        }
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            bool isOverTimeApplicable = false;
            try
            {
                if (pVal.ColUID == "clDTp")
                {
                    string strDayType = (grdAttendance.Columns.Item("clDTp").Cells.Item(pVal.Row).Specific as SAPbouiCOM.ComboBox).Value;
                    if (strDayType != "WD")
                    {
                        (grdAttendance.Columns.Item("WHrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                        (grdAttendance.Columns.Item("TmIn").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                        (grdAttendance.Columns.Item("TmOut").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                        (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                        (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                        (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                    }
                }
                if (pVal.ColUID == "TmIn" || pVal.ColUID == "TmOut")
                {
                    string TimeIn = (grdAttendance.Columns.Item("TmIn").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    string TimeOut = (grdAttendance.Columns.Item("TmOut").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    string strDayType = (grdAttendance.Columns.Item("clDTp").Cells.Item(pVal.Row).Specific as SAPbouiCOM.ComboBox).Value;
                    if (string.IsNullOrEmpty(TimeIn) && string.IsNullOrEmpty(TimeOut))
                    {
                        (grdAttendance.Columns.Item("WHrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                        if (strDayType == "WD")
                        {
                            (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "Leave / Absent";
                            (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "1.00";
                            (grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.00";
                            (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                            (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";                           
                        }
                        else
                        {
                            (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "OFF Day";
                            (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.00";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TimeIn) || string.IsNullOrEmpty(TimeOut))
                        {
                            (grdAttendance.Columns.Item("WHrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                            if (strDayType == "WD")
                            {
                                (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "Leave / Absent";
                                (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "1.00";
                                (grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.00";
                                (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                                (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                               
                            }
                            else
                            {
                                (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                                (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.00";
                            }
                        }
                        else
                        {

                            string[] StartDate = (grdAttendance.Columns.Item("TmIn").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
                            string[] EndDate = (grdAttendance.Columns.Item("TmOut").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
                            if (StartDate.Length != 2 || EndDate.Length != 2)
                            {
                                return;
                            }
                            else
                            {
                                int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                                if (DurinMin < 0)
                                    DurinMin += 1440;
                                int HrsDur = DurinMin / 60;
                                int MinDur = DurinMin % 60;
                                (grdAttendance.Columns.Item("WHrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');

                                string ShiftTimeIn = (grdAttendance.Columns.Item("SfStart").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                string shiftTimeOut = (grdAttendance.Columns.Item("SfEnd").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                string shiftHours = (grdAttendance.Columns.Item("SfHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                string ActualWorkingHours = (grdAttendance.Columns.Item("WHrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                string ShiftName = (grdAttendance.Columns.Item("shft").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                string strEMPID = (grdAttendance.Columns.Item("EmpCode").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                DateTime shiftDateX = DateTime.MinValue;
                                string shftDate = (grdAttendance.Columns.Item("Date").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                shiftDateX = DateTime.ParseExact(shftDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                DateTime x = Convert.ToDateTime(shiftDateX);
                                //Calculate LateIn Minutes
                                if (!string.IsNullOrEmpty(TimeIn))
                                {
                                    string strLateInMinutes = CalculateLateInMinutes(ShiftTimeIn, TimeIn);
                                    //Buffer Period  Included Here
                                    var AttendanceRule = dbHrPayroll.MstAttendanceRule.Where(ru => ru.FlgGpActive == false).FirstOrDefault();
                                    if (AttendanceRule != null)
                                    {
                                        string BufferInTime = AttendanceRule.GpAfterStartTime;
                                        if (!string.IsNullOrEmpty(BufferInTime) && BufferInTime != "00:00" && !string.IsNullOrEmpty(strLateInMinutes) && strLateInMinutes != "00:00")
                                        {
                                            if (IsBufferApplicable(BufferInTime, strLateInMinutes))
                                            {
                                                (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = strLateInMinutes;
                                            }
                                            else
                                            {
                                                (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                                            }
                                        }
                                        else
                                        {
                                            (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                                        }
                                    }
                                }
                                //Calculate Early Out Minutes
                                if (!string.IsNullOrEmpty(TimeIn))
                                {
                                    string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                    var ShiftRecord = dbHrPayroll.MstShifts.Where(S => S.Description == ShiftName).FirstOrDefault();
                                    var ShiftDetail = dbHrPayroll.MstShiftDetails.Where(S => S.Day == dayofWeeks && S.ShiftID == ShiftRecord.Id).FirstOrDefault();
                                    bool OutflgOverlap = ShiftDetail.FlgOutOverlap == null ? false : ShiftDetail.FlgOutOverlap.Value;
                                    decimal decTimeOut = ConvertTimeToDecimal(TimeOut);
                                    decimal decTimeIn = 0.0M;
                                    if (TimeIn != null)
                                    {
                                        decTimeIn = ConvertTimeToDecimal(TimeIn);
                                    }
                                    if (decTimeIn > decTimeOut && OutflgOverlap == false)
                                    {
                                        //Do Nothing
                                        (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                                    }
                                    else
                                    {
                                        string strEarlyOutMinutes = CalculateEarlyOutMinutes(shiftTimeOut, TimeOut);
                                        var AttendanceRule = dbHrPayroll.MstAttendanceRule.Where(ru => ru.FlgGpActive == false).FirstOrDefault();
                                        if (AttendanceRule != null)
                                        {
                                            string BufferOutTime = AttendanceRule.GpBeforeTimeEnd;
                                            if (!string.IsNullOrEmpty(BufferOutTime) && BufferOutTime != "00:00" && !string.IsNullOrEmpty(strEarlyOutMinutes) && strEarlyOutMinutes != "00:00")
                                            {
                                                if (!IsBufferApplicable(BufferOutTime, strEarlyOutMinutes))
                                                {
                                                    (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                                                }
                                                else
                                                {
                                                    (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = strEarlyOutMinutes;
                                                }
                                            }
                                            else
                                            {
                                                (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "00:00";
                                            }
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(shiftHours) && shiftHours != "00:00")
                                {
                                    if (string.IsNullOrEmpty(TimeIn) && string.IsNullOrEmpty(TimeOut) && strDayType == "WD")
                                    {
                                        (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "Leave /Absent";
                                        (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "1.00";
                                        (grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.00";
                                    }
                                    else
                                    {
                                        (grdAttendance.Columns.Item("clDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                                        (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.00";
                                    }
                                }
                                if (!string.IsNullOrEmpty(ActualWorkingHours))
                                {
                                    string strStatus = "";
                                    string strLateInMinutes = (grdAttendance.Columns.Item("clLtMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                    string strEarlyOutMinutes = (grdAttendance.Columns.Item("clEOMin").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                    strStatus = GetAttendanceStatus_NEW(strLateInMinutes, strEarlyOutMinutes);
                                    (grdAttendance.Columns.Item("clStatus").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = strStatus;
                                }
                                else
                                {
                                    (grdAttendance.Columns.Item("clStatus").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                                }
                                //Calculating OverTime
                                if (!string.IsNullOrEmpty(ActualWorkingHours))
                                {
                                    decimal OTRatio = 0.0M;
                                    string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                    var ShiftRecord = dbHrPayroll.MstShifts.Where(S => S.Description == ShiftName).FirstOrDefault();
                                    var ShiftDetail = dbHrPayroll.MstShiftDetails.Where(S => S.Day == dayofWeeks && S.ShiftID == ShiftRecord.Id).FirstOrDefault();
                                    var EmpMst = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEMPID).FirstOrDefault();
                                    if (EmpMst != null)
                                    {
                                        isOverTimeApplicable = EmpMst.FlgOTApplicable == null ? false : EmpMst.FlgOTApplicable.Value;
                                    }
                                    string strInOverTime = "00:00";
                                    string strOutOverTime = "00:00";
                                    string strOverTimeHours = "00:00";
                                    string strOverTimeType = "";
                                    string shiftBefferTimeIn = ShiftDetail.BufferStartTime;
                                    string shiftBufferTimeOut = ShiftDetail.BufferEndTime;
                                    bool OutflgOverlap = ShiftDetail.FlgOutOverlap == null ? false : ShiftDetail.FlgOutOverlap.Value;
                                    if (!isOverTimeApplicable)
                                    {
                                        strOverTimeHours = "";
                                        strOverTimeType = "";
                                    }
                                    else
                                    {
                                        bool flgOtonWorkedHours = ShiftRecord.FlgOTWrkHrs == null ? false : ShiftRecord.FlgOTWrkHrs.Value;
                                        if (flgOtonWorkedHours)
                                        {
                                            if (shiftHours == "00:00")
                                            {
                                                strOverTimeHours = ActualWorkingHours;
                                            }
                                            else if (strDayType == "OD")
                                            {
                                                strOverTimeHours = ActualWorkingHours;
                                            }
                                            else
                                            {
                                                strOverTimeHours = CalculateOverTimeHours(shiftHours, ActualWorkingHours);
                                            }
                                            OTRatio = GetOTHrsRatio(shiftHours, strOverTimeHours);
                                           //(grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = strOverTimeHours;
                                            (grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = string.Format("{0:0.00}", OTRatio);
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(shiftBefferTimeIn))
                                            {
                                                shiftBefferTimeIn = ShiftTimeIn;
                                            }
                                            if (string.IsNullOrEmpty(shiftBufferTimeOut))
                                            {
                                                shiftBufferTimeOut = shiftTimeOut;
                                            }
                                            strInOverTime = IFInOvertimeApplicable(shiftBefferTimeIn, TimeIn);
                                            if (strInOverTime != "00:00")
                                            {
                                                //CalculateIN Overtime AccordingToShift
                                                strInOverTime = CalculateInOvertimeApplicable(ShiftTimeIn, TimeIn);
                                            }
                                            strOutOverTime = IFOutOvertimeApplicable(TimeOut, shiftBufferTimeOut, OutflgOverlap);  //strOutOverTime = IFOutOvertimeApplicable(TimeOut, shiftBufferTimeOut);
                                            if (strOutOverTime != "00:00")
                                            {
                                                //strOutOverTime = CalculateOutOvertimeApplicable(TimeOut, shiftTimeOut);
                                                strOutOverTime = CalculateOutOvertimeApplicable(TimeOut, shiftTimeOut, OutflgOverlap);
                                            }
                                            strOverTimeHours = CalculateOverTimeHoursInandOutTime(strInOverTime, strOutOverTime);
                                            //OverTime on Weekend
                                            if (shiftHours == "00:00")
                                            {
                                                strOverTimeHours = ActualWorkingHours;
                                            }
                                            if (strDayType == "OD")
                                            {
                                                strOverTimeHours = ActualWorkingHours;
                                            }
                                            OTRatio = GetOTHrsRatio(shiftHours, strOverTimeHours);
                                            // (grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = strOverTimeHours;
                                            (grdAttendance.Columns.Item("OTH").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = string.Format("{0:0.00}", OTRatio);
                                        }
                                    }
                                }
                                //Calculate Leave Hours if Person is Available
                                if (!string.IsNullOrEmpty(ActualWorkingHours))
                                {
                                    string TempLeaveHours = "";
                                    var EmpREcord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEMPID).FirstOrDefault();
                                    decimal LeaveCount = 0.0M;
                                    TempLeaveHours = CalculateLeaveHours(shiftHours, ActualWorkingHours);
                                    if (!string.IsNullOrEmpty(TempLeaveHours) && TempLeaveHours != "00:00")
                                    {
                                        bool flgIsSupervisor = EmpREcord.FlgSuperVisor == null ? false : EmpREcord.FlgSuperVisor.Value;
                                        LeaveCount = ReturnLeaveUnits(TempLeaveHours, flgIsSupervisor);
                                    }
                                    (grdAttendance.Columns.Item("lCnt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = string.Format("{0:0.00}", LeaveCount);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }
        public override void etBeforeValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                BubbleEvent = true;
                Validate = false;
                switch (pVal.ColUID)
                {
                    case "TimeIn":
                    case "TimeOut":
                        {
                            string Value = (grdAttendance.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                            for (int i = 0; i < Value.Length; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        if ((char)Value[0] >= '0' && (char)Value[0] <= '2') Validate = true;
                                        else Validate = false;
                                        break;
                                    case 1:
                                        if ((char)Value[0] != '2')
                                        {
                                            if ((char)Value[1] >= '0' && (char)Value[1] <= '9') Validate = true;
                                            else Validate = false;
                                        }
                                        else
                                        {
                                            if ((char)Value[1] >= '0' && (char)Value[1] <= '3') Validate = true;
                                            else Validate = false;
                                        }
                                        break;
                                    case 2:
                                        if ((char)Value[2] == ':') Validate = true;
                                        else Validate = false;
                                        break;
                                    case 3:
                                        if ((char)Value[3] >= '0' && (char)Value[3] <= '5') Validate = true;
                                        else Validate = false;
                                        break;

                                    case 4:
                                        if ((char)Value[4] >= '0' && (char)Value[4] <= '9') Validate = true;
                                        else Validate = false;
                                        break;

                                }
                                if (Validate == false || Value.Length != 5)
                                {
                                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_InvalidFormat"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    BubbleEvent = false;
                                    return;
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            SetEmpValues();
        }

        #endregion

        #region "Local Methods"

        private string CalculateWorkHours(string startTime, string endTime)
        {
            string strWorkHours = "";
            try
            {
                if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
                {
                    string[] StartDate = startTime.Split(':');
                    string[] EndDate = endTime.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                            DurinMin += 1440;
                        int HrsDur = DurinMin / 60;
                        int MinDur = DurinMin % 60;
                        strWorkHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                    }
                }
                return strWorkHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        public void InitiallizeForm()
        {
            try
            {
                oForm.PaneLevel = 1;
                btnSerch = oForm.Items.Item("btnSerc").Specific;
                btnClear = oForm.Items.Item("btnClear").Specific;
                btnNext = oForm.Items.Item("btnNext").Specific;
                btnBack = oForm.Items.Item("btnBack").Specific;
                IbtnBack = oForm.Items.Item("btnBack");
                btnSave = oForm.Items.Item("btnSave").Specific;
                IbtnSave = oForm.Items.Item("btnSave");
                btnID = oForm.Items.Item("btId").Specific;
                IbtnID = oForm.Items.Item("btId");
                btnId2 = oForm.Items.Item("btId2").Specific;
                IbtnId2 = oForm.Items.Item("btId2");
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("empfrm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpIdFrom = oForm.Items.Item("empfrm").Specific;
                ItxtEmpIdFrom = oForm.Items.Item("empfrm");
                txtEmpIdFrom.DataBind.SetBound(true, "", "empfrm");

                oForm.DataSources.UserDataSources.Add("empTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpIdTo = oForm.Items.Item("empTo").Specific;
                ItxtEmpIdTo = oForm.Items.Item("empTo");
                txtEmpIdTo.DataBind.SetBound(true, "", "empTo");

                cb_depart = oForm.Items.Item("cb_dpt").Specific;
                Icb_depart = oForm.Items.Item("cb_dpt");

                cb_deignation = oForm.Items.Item("cb_desg").Specific;
                Icb_deignation = oForm.Items.Item("cb_desg");

                cb_Location = oForm.Items.Item("cb_loc").Specific;
                Icb_Location = oForm.Items.Item("cb_loc");

                //Initializing Date Fields

                oForm.DataSources.UserDataSources.Add("frmdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate = oForm.Items.Item("frmdt").Specific;
                ItxtFromDate = oForm.Items.Item("frmdt");
                txtFromDate.DataBind.SetBound(true, "", "frmdt");

                oForm.DataSources.UserDataSources.Add("todt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate = oForm.Items.Item("todt").Specific;
                ItxtToDate = oForm.Items.Item("todt");
                txtToDate.DataBind.SetBound(true, "", "todt");

                InitiallizegridMatrix();
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
                dtEmployees = oForm.DataSources.DataTables.Add("Employees");
                dtEmployees.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmployees.Columns.Add("EmpCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("Designation", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("Department", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("Location", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("isSel", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);

                grdEmployees = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Emp").Specific;
                IgrdEmployees = oForm.Items.Item("grd_Emp");
                oColumns = (SAPbouiCOM.Columns)grdEmployees.Columns;


                oColumn = oColumns.Item("No");
                clNo = oColumn;
                oColumn.DataBind.Bind("Employees", "No");

                oColumn = oColumns.Item("EmpCode");
                EmpCode = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpCode");

                oColumn = oColumns.Item("EmpName");
                EmpName = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpName");

                oColumn = oColumns.Item("Desig");
                Desig = oColumn;
                oColumn.DataBind.Bind("Employees", "Designation");

                oColumn = oColumns.Item("Depart");
                Depart = oColumn;
                oColumn.DataBind.Bind("Employees", "Department");

                oColumn = oColumns.Item("Location");
                Location = oColumn;
                oColumn.DataBind.Bind("Employees", "Location");

                oColumn = oColumns.Item("isSel");
                isSel = oColumn;
                oColumn.DataBind.Bind("Employees", "isSel");

                // Second Grid Initialization

                dtAttendance = oForm.DataSources.DataTables.Add("Attendance");
                dtAttendance.Columns.Add("Id", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAttendance.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAttendance.Columns.Add("EmpCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("Date", SAPbouiCOM.BoFieldsType.ft_Date);
                dtAttendance.Columns.Add("Shift", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("SfStart", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("SfEnd", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("SfHours", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("TimeIn", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("TimeOut", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("WorkHours", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("LateInMin", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("EarlyOutMin", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);       
                dtAttendance.Columns.Add("OTHours", SAPbouiCOM.BoFieldsType.ft_Text);                          
                dtAttendance.Columns.Add("LevHours", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("clDesc", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("clDTp", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("Status", SAPbouiCOM.BoFieldsType.ft_Text); 
                //  dtAttendance.Columns.Add("IsNewLeave", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                dtAttendance.Columns.Add("LevCount", SAPbouiCOM.BoFieldsType.ft_Text, 6);

                grdAttendance = (SAPbouiCOM.Matrix)oForm.Items.Item("grdAtt").Specific;
                IgrdAttendance = oForm.Items.Item("grdAtt");
                oColumns = (SAPbouiCOM.Columns)grdAttendance.Columns;

                oColumn = oColumns.Item("Id");
                clId = oColumn;
                oColumn.DataBind.Bind("Attendance", "Id");
                clId.Visible = false;

                oColumn = oColumns.Item("No");
                clNo = oColumn;
                oColumn.DataBind.Bind("Attendance", "No");

                oColumn = oColumns.Item("EmpCode");
                EmpCode = oColumn;
                oColumn.DataBind.Bind("Attendance", "EmpCode");

                oColumn = oColumns.Item("EmpName");
                EmpName = oColumn;
                oColumn.DataBind.Bind("Attendance", "EmpName");


                oColumn = oColumns.Item("Date");
                clsdate = oColumn;
                oColumn.DataBind.Bind("Attendance", "Date");

                oColumn = oColumns.Item("shft");
                clshft = oColumn;
                oColumn.DataBind.Bind("Attendance", "Shift");

                oColumn = oColumns.Item("SfStart");
                clSfStart = oColumn;
                oColumn.DataBind.Bind("Attendance", "SfStart");

                oColumn = oColumns.Item("SfEnd");
                clSfEnd = oColumn;
                oColumn.DataBind.Bind("Attendance", "SfEnd");

                oColumn = oColumns.Item("SfHours");
                clssHiftHrs = oColumn;
                oColumn.DataBind.Bind("Attendance", "SfHours");

                oColumn = oColumns.Item("TmIn");
                clsTimeIn = oColumn;
                oColumn.DataBind.Bind("Attendance", "TimeIn");

                oColumn = oColumns.Item("TmOut");
                clsTimeOut = oColumn;
                oColumn.DataBind.Bind("Attendance", "TimeOut");

                oColumn = oColumns.Item("WHrs");
                clsWHrs = oColumn;
                oColumn.DataBind.Bind("Attendance", "WorkHours");

                oColumn = oColumns.Item("clDTp");
                clDTp = oColumn;
                oColumn.DataBind.Bind("Attendance", "clDTp");
                clDTp.Editable = false;


                oColumn = oColumns.Item("OTH");
                clsOTH = oColumn;
                oColumn.DataBind.Bind("Attendance", "OTHours");

                //oColumn = oColumns.Item("LT");
                //clLT = oColumn;
                //oColumn.DataBind.Bind("Attendance", "LevType");

                //oColumn = oColumns.Item("LH");
                //clsLH = oColumn;
                //oColumn.DataBind.Bind("Attendance", "LevHours");

                oColumn = oColumns.Item("clStatus");
                clStatus = oColumn;
                oColumn.DataBind.Bind("Attendance", "Status");
                //clStatus.Visible = false;

                oColumn = oColumns.Item("clLtMin");
                clStatus = oColumn;
                oColumn.DataBind.Bind("Attendance", "LateInMin");

                oColumn = oColumns.Item("clEOMin");
                clStatus = oColumn;
                oColumn.DataBind.Bind("Attendance", "EarlyOutMin");

                oColumn = oColumns.Item("lCnt");
                clLCount = oColumn;
                oColumn.DataBind.Bind("Attendance", "LevCount");

                //oColumn = oColumns.Item("IsNewL");
                //clIsNewLeave = oColumn;
                //oColumn.DataBind.Bind("Attendance", "IsNewLeave");
                //clIsNewLeave.Visible = true;

                oColumn = oColumns.Item("clDesc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("Attendance", "clDesc");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillDepartmentInCombo()
        {
            try
            {
                var Departments = from a in dbHrPayroll.MstDepartment select a;
                cb_depart.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstDepartment Dept in Departments)
                {
                    cb_depart.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillDepartmentInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillDesignationInCombo()
        {
            try
            {
                var Designation = from a in dbHrPayroll.MstDesignation select a;
                cb_deignation.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstDesignation Desig in Designation)
                {
                    cb_deignation.ValidValues.Add(Convert.ToString(Desig.Id), Convert.ToString(Desig.Name));
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillDesignationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillEmpLocationInCombo()
        {
            try
            {
                var EmpLocation = from a in dbHrPayroll.MstLocation select a;
                cb_Location.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstLocation empLocation in EmpLocation)
                {
                    cb_Location.ValidValues.Add(Convert.ToString(empLocation.Id), Convert.ToString(empLocation.Name));
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillEmpLocationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void FillOvertimeTypeInCombo()
        {
            try
            {
                var OverTime = from a in dbHrPayroll.MstOverTime select a;
                clsOTT.ValidValues.Add("-1", "");
                foreach (MstOverTime empOvertimeType in OverTime)
                {
                    clsOTT.ValidValues.Add(Convert.ToString(empOvertimeType.Code), Convert.ToString(empOvertimeType.Description));
                }
                clsOTT.DisplayDesc = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillOvertimeTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillLeaveTypeInCombo()
        {
            try
            {
                var LeaveType = from a in dbHrPayroll.MstLeaveType select a;
                clLT.ValidValues.Add("-1", "");
                foreach (MstLeaveType empLeaveType in LeaveType)
                {
                    clLT.ValidValues.Add(Convert.ToString(empLeaveType.Code), Convert.ToString(empLeaveType.Description));
                }
                clLT.DisplayDesc = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillLeaveTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void FillDayTypeInCombo()
        {
            try
            {
                var OverTime = from a in dbHrPayroll.MstLOVE
                               where a.Type == "DayTpe"
                               select a;
                clDTp.ValidValues.Add("-1", "");
                foreach (MstLOVE empOvertimeType in OverTime)
                {
                    clDTp.ValidValues.Add(Convert.ToString(empOvertimeType.Code), Convert.ToString(empOvertimeType.Value));
                }
                clDTp.DisplayDesc = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillOvertimeTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void PopulateGridWithFilterExpression()
        {
            Int16 i = 0;

            var Data = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID > 0).ToList();

            if (txtEmpIdFrom.Value != string.Empty && txtEmpIdTo.Value != string.Empty)
            {
                int intEmpIdFrom = dbHrPayroll.MstEmployee.Where(emp => emp.EmpID == txtEmpIdFrom.Value).FirstOrDefault().ID;
                int intEmpIdTo = dbHrPayroll.MstEmployee.Where(emp => emp.EmpID == txtEmpIdTo.Value).FirstOrDefault().ID;
                Data = Data.Where(e => e.ID >= intEmpIdFrom && e.ID <= intEmpIdTo).ToList();
            }
            if (cb_Location.Value != "0" && cb_Location.Value != string.Empty)
            {
                Data = Data.Where(e => e.Location == Convert.ToInt32(cb_Location.Value)).ToList();
            }
            if (cb_depart.Value != "0" && cb_depart.Value != string.Empty)
            {
                Data = Data.Where(e => e.DepartmentID == Convert.ToInt32(cb_depart.Value)).ToList();
            }
            if (cb_deignation.Value != "0" && cb_deignation.Value != string.Empty)
            {
                Data = Data.Where(e => e.DesignationID == Convert.ToInt32(cb_deignation.Value)).ToList();
            }
            if (Data != null && Data.Count > 0)
            {

                dtEmployees.Rows.Clear();
                dtEmployees.Rows.Add(Data.Count());
                foreach (var EMP in Data)
                {
                    dtEmployees.SetValue("No", i, i + 1);
                    dtEmployees.SetValue("EmpCode", i, EMP.EmpID);
                    dtEmployees.SetValue("EmpName", i, EMP.FirstName + " " + EMP.MiddleName + " " + EMP.LastName);
                    dtEmployees.SetValue("Designation", i, !String.IsNullOrEmpty(EMP.DesignationName) ? EMP.DesignationName.ToString() : "");
                    dtEmployees.SetValue("Department", i, !String.IsNullOrEmpty(EMP.DepartmentName) ? EMP.DepartmentName.ToString() : "");
                    dtEmployees.SetValue("Location", i, !String.IsNullOrEmpty(EMP.LocationName) ? EMP.LocationName.ToString() : "");
                    i++;
                }
                grdEmployees.LoadFromDataSource();
            }
            else
            {
                dtEmployees.Rows.Clear();
                grdEmployees.LoadFromDataSource();
            }
        }
        private void ClearControls()
        {
            try
            {
                txtEmpIdFrom.Value = string.Empty;
                txtEmpIdTo.Value = string.Empty;
                cb_deignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cb_depart.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cb_Location.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtEmployees.Rows.Clear();
                grdEmployees.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void HideFirstVisibleNext()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                {
                    if (dtEmployees != null && dtEmployees.Rows.Count > 0)
                    {
                        LoadEmployeeAttendanceRecordOrderByDate();
                        IgrdAttendance.Visible = true;
                        IbtnID.Visible = false;
                        IbtnId2.Visible = false;
                        IbtnBack.Visible = true;
                        IbtnSave.Visible = true;
                        oForm.PaneLevel = 2;
                    }
                    else
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("INF_SelectEmployee"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("INF_AttendanceDates"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: HideFirstVisibleNext Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void HideNextVisibleFirst()
        {
            try
            {
                dtAttendance.Rows.Clear();
                grdAttendance.LoadFromDataSource();
                IbtnID.Visible = true;
                IbtnId2.Visible = true;
                oForm.PaneLevel = 1;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: HideNextVisibleFirst Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private string CalculateOverTimeHours(string ShiftHours, string WorkedHours)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(ShiftHours) && !string.IsNullOrEmpty(WorkedHours))
                {
                    string[] StartDate = ShiftHours.Split(':');
                    string[] EndDate = WorkedHours.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string IFOutOvertimeApplicable(string ActualTimeOut, string BufferOutTime)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(BufferOutTime) && !string.IsNullOrEmpty(ActualTimeOut))
                {
                    string[] StartDate = BufferOutTime.Split(':');
                    string[] EndDate = ActualTimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {

                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            DurinMin += 1440;
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                            //strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string IFOutOvertimeApplicable(string ActualTimeOut, string BufferOutTime, bool flgOutOverLap)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(BufferOutTime) && !string.IsNullOrEmpty(ActualTimeOut))
                {
                    string[] StartDate = BufferOutTime.Split(':');
                    string[] EndDate = ActualTimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {

                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            if (!flgOutOverLap)
                            {
                                DurinMin += 1440;
                                int HrsDur = DurinMin / 60;
                                int MinDur = DurinMin % 60;
                                strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                            }
                            else
                            {
                                strOverTimeHours = "00:00";
                            }
                            //strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string IFOutOvertimeApplicable_OLDCODEBYZEESHAN(string ActualTimeOut, string BufferOutTime)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(BufferOutTime) && !string.IsNullOrEmpty(ActualTimeOut))
                {
                    string[] StartDate = BufferOutTime.Split(':');
                    string[] EndDate = ActualTimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string CalculateOutOvertimeApplicable(string ActualTimeOut, string shiftOutTime)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(shiftOutTime) && !string.IsNullOrEmpty(ActualTimeOut))
                {
                    //decimal decPunchTimeOUT = ConvertTimeToDecimal(ActualTimeOut);
                    double decPunchTimeOUT = TimeSpan.Parse(ActualTimeOut).TotalHours;
                    if (decPunchTimeOUT >= 0 && decPunchTimeOUT < 7.52)
                    {
                        decPunchTimeOUT = decPunchTimeOUT + 24;
                        //decimal hours = Math.Floor(decPunchTimeOUT); //take integral part
                        //decimal minutes = (decPunchTimeOUT - hours) * 60.0M;
                        double hours = Math.Floor(decPunchTimeOUT);
                        double minutes = (decPunchTimeOUT - hours) * 60;

                        int H = (int)Math.Floor(hours);
                        int M = (int)Math.Round(minutes);
                        ActualTimeOut = H.ToString().PadLeft(2, '0') + ":" + M.ToString().PadLeft(2, '0');
                    }

                    string[] StartDate = shiftOutTime.Split(':');
                    string[] EndDate = ActualTimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string CalculateOutOvertimeApplicable(string ActualTimeOut, string shiftOutTime, bool flgOutOverLap)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(shiftOutTime) && !string.IsNullOrEmpty(ActualTimeOut))
                {
                    if (!flgOutOverLap)
                    {
                        //decimal decPunchTimeOUT = ConvertTimeToDecimal(ActualTimeOut);
                        double decPunchTimeOUT = TimeSpan.Parse(ActualTimeOut).TotalHours;
                        if (decPunchTimeOUT >= 0 && decPunchTimeOUT < 7.52)
                        {
                            decPunchTimeOUT = decPunchTimeOUT + 24;
                            //decimal hours = Math.Floor(decPunchTimeOUT); //take integral part
                            //decimal minutes = (decPunchTimeOUT - hours) * 60.0M;
                            double hours = Math.Floor(decPunchTimeOUT);
                            double minutes = (decPunchTimeOUT - hours) * 60;

                            int H = (int)Math.Floor(hours);
                            int M = (int)Math.Round(minutes);
                            ActualTimeOut = H.ToString().PadLeft(2, '0') + ":" + M.ToString().PadLeft(2, '0');
                        }
                    }
                    string[] StartDate = shiftOutTime.Split(':');
                    string[] EndDate = ActualTimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string CalculateOutOvertimeApplicable_OLDCODEBYZEESHAN(string ActualTimeOut, string shiftOutTime)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(shiftOutTime) && !string.IsNullOrEmpty(ActualTimeOut))
                {
                    string[] StartDate = shiftOutTime.Split(':');
                    string[] EndDate = ActualTimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string IFInOvertimeApplicable(string BufferInTime, string ActualTimeIn)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(BufferInTime) && !string.IsNullOrEmpty(ActualTimeIn))
                {
                    string[] StartDate = ActualTimeIn.Split(':');
                    string[] EndDate = BufferInTime.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private string CalculateInOvertimeApplicable(string ShiftInTime, string ActualTimeIn)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(ShiftInTime) && !string.IsNullOrEmpty(ActualTimeIn))
                {
                    string[] StartDate = ActualTimeIn.Split(':');
                    string[] EndDate = ShiftInTime.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private decimal CalculateHourTimeCount(string ActualOTHours)
        {
            decimal OtHours = 0;
            try
            {
                if (!string.IsNullOrEmpty(ActualOTHours))
                {
                    string[] EndDate = ActualOTHours.Split(':');
                    if (EndDate.Length != 2)
                    {
                        return 0;
                    }
                    else
                    {
                        double decPunchTimeOUT = TimeSpan.Parse(ActualOTHours).TotalHours;
                        OtHours = Convert.ToDecimal(decPunchTimeOUT);
                        //int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        //OtHours = DurinMin / 60;
                        //decimal min = DurinMin % 60;
                        //min = decimal.Multiply(0.01M, min);
                        //OtHours = OtHours + min;
                    }
                }
                return OtHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }
        private string CalculateLateInMinutes(string ShiftTime, string TimIn)
        {
            string strLateInHours = "";
            try
            {
                if (ShiftTime == "00:00")
                {
                    strLateInHours = "00:00";
                    return strLateInHours;
                }
                if (TimIn == "00:00")
                {
                    strLateInHours = "00:00";
                    return strLateInHours;
                }
                if (!string.IsNullOrEmpty(ShiftTime) && !string.IsNullOrEmpty(TimIn))
                {
                    string[] StartDate = ShiftTime.Split(':');
                    string[] EndDate = TimIn.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strLateInHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strLateInHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }

                    }
                }
                return strLateInHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }

        }
        private string CalculateLeaveHours(string ShiftHours, string ActualWorkingHours)
        {
            string strLeaveHours = "";
            try
            {
                if (ShiftHours == "00:00")
                {
                    return "00:00";
                }
                if (!string.IsNullOrEmpty(ShiftHours) && !string.IsNullOrEmpty(ActualWorkingHours))
                {
                    string[] StartDate = ShiftHours.Split(':');
                    string[] EndDate = ActualWorkingHours.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1])) - ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        if (DurinMin < 0)
                        {
                            strLeaveHours = "00:00";
                        }
                        else
                        {
                            int ShiftMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strLeaveHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }

                    }
                }
                return strLeaveHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }

        }
        private string CalculateOverTimeHoursInandOutTime(string InTimeOT, string OutTimeOT)
        {
            string strOverTimeHours = "";
            try
            {
                if (!string.IsNullOrEmpty(InTimeOT) && !string.IsNullOrEmpty(OutTimeOT))
                {
                    string[] StartDate = InTimeOT.Split(':');
                    string[] EndDate = OutTimeOT.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) + ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strOverTimeHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strOverTimeHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }
                    }
                }
                return strOverTimeHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }
        }
        private bool IsBufferApplicable(string BufferTime, string DueTime)
        {
            string strLeaveHours = "";
            bool BufferApplicable = false;
            try
            {
                if (BufferTime == "00:00")
                {
                    return BufferApplicable;
                }
                if (!string.IsNullOrEmpty(BufferTime) && !string.IsNullOrEmpty(DueTime))
                {
                    string[] StartDate = DueTime.Split(':');
                    string[] EndDate = BufferTime.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1])) - ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        if (DurinMin <= 0)
                        {
                            //strLeaveHours = "00:00";
                            BufferApplicable = false;
                        }
                        else
                        {
                            strLeaveHours = DueTime;
                            BufferApplicable = true;
                        }

                    }
                }
                return BufferApplicable;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }

        }
        private void OpenNewSearchForm()
        {
            try
            {
                Program.FromEmpId = "";
                string comName = "fromSrch";
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
        private void OpenNewSearchFormTo()
        {
            try
            {
                Program.ToEmpId = "";
                string comName = "ToSearch";
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
                if (!string.IsNullOrEmpty(Program.FromEmpId))
                {
                    txtEmpIdFrom.Value = Program.FromEmpId;
                }
                if (!string.IsNullOrEmpty(Program.ToEmpId))
                {
                    txtEmpIdTo.Value = Program.ToEmpId;
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void selectAllProcess()
        {
            try
            {

                oForm.Freeze(true);
                SAPbouiCOM.Column col = grdEmployees.Columns.Item("isSel");

                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {

                        dtEmployees.SetValue("isSel", i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {
                        dtEmployees.SetValue("isSel", i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                grdEmployees.LoadFromDataSource();
                oForm.Freeze(false);
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }
        private string CalculateHalfShiftHours(string ShiftHours)
        {
            string strLeaveHours = "";
            try
            {
                if (ShiftHours == "00:00")
                {
                    return "00:00";
                }
                if (!string.IsNullOrEmpty(ShiftHours))
                {
                    string[] StartDate = ShiftHours.Split(':');

                    if (StartDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                        {
                            strLeaveHours = "00:00";
                        }
                        else
                        {
                            int ShiftMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                            int decShiftMin = ShiftMin / 2;

                            int HrsDur = decShiftMin / 60;
                            int MinDur = decShiftMin % 60;
                            strLeaveHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }

                    }
                }
                return strLeaveHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }

        }
        private decimal CalculateLeaveCount(string ShiftHours, string ActualWorkingHours)
        {
            decimal LeaveCountX = 0.0M;
            try
            {
                if (ShiftHours == "00:00")
                {
                    return LeaveCountX;
                }
                if (!string.IsNullOrEmpty(ShiftHours) && !string.IsNullOrEmpty(ActualWorkingHours))
                {


                    string[] StartDate = ShiftHours.Split(':');
                    string[] EndDate = ActualWorkingHours.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1])) - ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        if (DurinMin < 0)
                        {
                            //strLeaveHours = "00:00";
                        }
                        else
                        {
                            int ShiftMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                            decimal ShiftHoursX = ShiftMin / 60;
                            decimal LeaveHours = DurinMin / 60;
                            decimal Count = LeaveHours / ShiftHoursX;
                            decimal countMin = LeaveHours % ShiftHoursX;
                            countMin = decimal.Multiply(0.01M, countMin);
                            Count = Count + countMin;
                            LeaveCountX = Count;
                        }

                    }
                }
                return LeaveCountX;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }
        private string CalculateEarlyOutMinutes(string ShiftTime, string TimeOut)
        {
            string strLateInHours = "";
            try
            {
                if (ShiftTime == "00:00")
                {
                    strLateInHours = "00:00";
                    return strLateInHours;
                }
                if (TimeOut == "00:00")
                {
                    strLateInHours = "00:00";
                    return strLateInHours;
                }
                if (!string.IsNullOrEmpty(ShiftTime) && !string.IsNullOrEmpty(TimeOut))
                {
                    string[] StartDate = ShiftTime.Split(':');
                    string[] EndDate = TimeOut.Split(':');
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //return "";
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1])) - ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        if (DurinMin < 0)
                        {
                            strLateInHours = "00:00";
                        }
                        else
                        {
                            int HrsDur = DurinMin / 60;
                            int MinDur = DurinMin % 60;
                            strLateInHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        }

                    }
                }
                return strLateInHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return "";
            }

        }
        private string GetAttendanceStatus_NEW(string LateInMinutes, string EarlyOutMinutes)
        {
            try
            {
                string strStatus = "";
                if (!string.IsNullOrEmpty(LateInMinutes) && LateInMinutes != "00:00")
                {
                    strStatus = strStatus + " LateIn";
                }
                if (!string.IsNullOrEmpty(EarlyOutMinutes) && EarlyOutMinutes != "00:00")
                {
                    strStatus = strStatus + " Early Out";
                }
                return strStatus;
            }
            catch (Exception Ex)
            {
                return "";
            }

        }
        private void SaveAttendanceRecord()
        {
            bool isOnLeave = false;
            decimal ShiftHoursCount = 0.0M;
            bool isOnSpecialDayLeave = false;
            bool isOnAbsent = false;
            string strShiftHours = "";
            int recId = 0;
            string strTimeIn = "";
            string strTimeOut = "";
            string strWorkHours = "";
            //string strLeaveType = "";
            // string strLeaveHours = "";
            string strEmpCode = "";
            string strEmpName = "";
            int intEmpID = 0;
            string LeaveCountTotal = "";
            string strleaveDate = "";
            string strLateInMinutes = "";
            string strDayType = "";
            string strEarlyOutMinutes = "";
            string strOverTimeHours = "";
            //string strOverTimeType = "";
            //TrnsEmployeeOvertime EmpOverTime;

            DateTime leaveDate = DateTime.MinValue;
            var objPayrollInition = dbHrPayroll.CfgPayrollBasicInitialization.FirstOrDefault();
            if (objPayrollInition == null)
            {
                oApplication.StatusBar.SetText("Please Configure Payroll Initialization First ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return;
            }
            try
            {
                for (int i = 1; i < grdAttendance.RowCount + 1; i++)
                {
                    strEmpCode = (grdAttendance.Columns.Item("EmpCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strTimeIn = (grdAttendance.Columns.Item("TmIn").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strTimeOut = (grdAttendance.Columns.Item("TmOut").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strleaveDate = (grdAttendance.Columns.Item("Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;

                    if (!string.IsNullOrEmpty(strTimeIn) && string.IsNullOrEmpty(strTimeOut) && objPayrollInition.FlgAbsent != true)
                    {
                        oApplication.StatusBar.SetText("TimeOut missing for Employee with Code " + strEmpCode + " on Dated " + strleaveDate, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (string.IsNullOrEmpty(strTimeIn) && !string.IsNullOrEmpty(strTimeOut) && objPayrollInition.FlgAbsent != true)
                    {
                        oApplication.StatusBar.SetText("TimeIn missing for Employee with Code " + strEmpCode + " on Dated " + strleaveDate, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }

                for (int i = 1; i < grdAttendance.RowCount + 1; i++)
                {

                    recId = Convert.ToInt32((grdAttendance.Columns.Item("Id").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);          
                    strShiftHours = (grdAttendance.Columns.Item("SfHours").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strTimeIn = (grdAttendance.Columns.Item("TmIn").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strTimeOut = (grdAttendance.Columns.Item("TmOut").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strWorkHours = (grdAttendance.Columns.Item("WHrs").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strDayType = (grdAttendance.Columns.Item("clDTp").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    //strLeaveHours = (grdAttendance.Columns.Item("LH").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    LeaveCountTotal = (grdAttendance.Columns.Item("lCnt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strLateInMinutes = (grdAttendance.Columns.Item("clLtMin").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strEarlyOutMinutes = (grdAttendance.Columns.Item("clEOMin").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strEmpCode = (grdAttendance.Columns.Item("EmpCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strEmpName = (grdAttendance.Columns.Item("EmpName").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strOverTimeHours = (grdAttendance.Columns.Item("OTH").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;                  
                    intEmpID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault().ID;
                    strleaveDate = (grdAttendance.Columns.Item("Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    leaveDate = DateTime.ParseExact(strleaveDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    if (string.IsNullOrEmpty(strWorkHours) && strShiftHours != "00:00")
                    {
                        var Data = dbHrPayroll.TrnsLeavesRequest.Where(pd => pd.LeaveFrom <= leaveDate && leaveDate <= pd.LeaveTo && pd.EmpID == intEmpID).FirstOrDefault();
                        if (Data != null && Data.UnitsID == "Day")
                        {
                            isOnLeave = true;
                            isOnAbsent = false;
                            isOnSpecialDayLeave = false;
                        }
                        else
                        {                           
                            isOnLeave = false;
                            isOnAbsent = true;
                        }
                    }
                    else
                    {
                        isOnLeave = false;
                        isOnAbsent = false;                    
                    }

                    if (recId > 0)
                    {
                        TrnsTextileGroupAttendanceReg oOldVal = dbHrPayroll.TrnsTextileGroupAttendanceReg.Where(atr => atr.ID == recId).FirstOrDefault();                                              
                        oOldVal.TimeIn = strTimeIn.Trim();
                        oOldVal.TimeOut = strTimeOut.Trim();
                        oOldVal.WorkHours = strWorkHours.Trim();
                        oOldVal.LateInMin = strLateInMinutes.Trim();
                        oOldVal.EarlyOutMin = strEarlyOutMinutes.Trim();
                        oOldVal.OTCount = string.IsNullOrEmpty(strOverTimeHours) ? 0 : Convert.ToDecimal(strOverTimeHours);
                        //oOldVal.OTHours = strOverTimeHours.Trim();
                        oOldVal.FlgOnLeave = isOnLeave;
                        oOldVal.LeaveCount = string.IsNullOrEmpty(LeaveCountTotal) ? 0 : Convert.ToDecimal(LeaveCountTotal);
                        oOldVal.FlgOnAbsent = isOnAbsent;              
                        oOldVal.ShiftHours = strShiftHours.Trim();
                        oOldVal.UpdatedDate = DateTime.Now;
                        oOldVal.FlgProcessed = true;
                        //oOldVal.OTCount = ShiftHoursCount;
                        oOldVal.UpdatedBy = oCompany.UserName;
                        oOldVal.ProcessedBy = oCompany.UserName;                        
                    }
                    dbHrPayroll.SubmitChanges();
                }
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                dtAttendance.Rows.Clear();
                grdAttendance.LoadFromDataSource();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: SaveAttendanceRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void picEmpFrom()
        {
            SearchKeyVal.Clear();
            if (!string.IsNullOrEmpty(txtEmpIdFrom.Value))
            {
                SearchKeyVal.Add("EmpID", txtEmpIdFrom.Value.ToString());
            }
            string strSql = sqlString.getSql("empAttendanceFrom", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Attendance Process");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpIdFrom.Value = st.Rows[0][0].ToString();
            }
        }
        private void picEmpTo()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empAttendanceTo", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Attendance Process");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpIdTo.Value = st.Rows[0][0].ToString();
            }
        }
        private decimal ConvertTimeToDecimal(string ActualHours)
        {
            decimal OtHours = 0;
            try
            {
                if (!string.IsNullOrEmpty(ActualHours))
                {
                    string[] EndDate = ActualHours.Split(':');
                    if (EndDate.Length != 2)
                    {
                        return 0;
                    }
                    else
                    {
                        double decPunchTimeOUT = TimeSpan.Parse(ActualHours).TotalHours;
                        OtHours = Convert.ToDecimal(decPunchTimeOUT);
                        //int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        //OtHours = DurinMin / 60;
                        //decimal min = DurinMin % 60;
                        //min = decimal.Multiply(0.01M, min);
                        //OtHours = OtHours + min;
                    }
                }
                return OtHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }
        private decimal ReturnLeaveUnits(string TempLeaveHours,bool FlgSuperVisor)
        {
            decimal decLeaveCount = 0.00M;
            string SQL = "";
            try
            {
                if (FlgSuperVisor)
                {
                    SQL = "Select Code,Value,RangeFrom,RangeTo,Deduction,LeaveType From " + Program.objHrmsUI.HRMSDbName + ".dbo.MstDeductionRuleSup Where RangeFrom <= '" + TempLeaveHours + "' and RangeTo >= '" + TempLeaveHours + "'";
                }
                else
                {
                    SQL = "Select Code,Value,RangeFrom,RangeTo,Deduction,LeaveType From " + Program.objHrmsUI.HRMSDbName + ".dbo.MstDeductionRules Where RangeFrom <= '" + TempLeaveHours + "' and RangeTo >= '" + TempLeaveHours + "'";
                }
                DataTable dt = ds.getDataTable(SQL);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string DeductionUnit = Convert.ToString(dt.Rows[0]["Code"]);
                    bool Deduction = Convert.ToBoolean(dt.Rows[0]["Deduction"]);
                    switch (DeductionUnit)
                    {
                        case "DR_01":
                            decLeaveCount = 0.00M;
                            break;
                        case "DR_02":
                            decLeaveCount = 0.50M;
                            break;
                        case "DR_03":
                            decLeaveCount = 1.00M;
                            break;
                    }
                    if (Deduction != true)
                    {
                        decLeaveCount = 0.00M;
                    }
                }
                return decLeaveCount;
            }
            catch (Exception ex)
            {
                return 0.0M;
            }
        }
        private decimal GetOTHrsRatio(string strShiftHrs, string strOvertimeHrs)
        {
            decimal decOTActualRatio = 0.0M;
            try
            {
                decimal decShiftHrs = ConvertTimeToDecimal(strShiftHrs);
                decimal decWrkHrs = ConvertTimeToDecimal(strOvertimeHrs);
                if (decShiftHrs <= 0)
                {
                    decOTActualRatio = 1;
                }
                else if (decWrkHrs <= 0)
                {
                    decOTActualRatio = 0;
                }
                else if(decShiftHrs > 0 && decWrkHrs > 0)
                {
                    decOTActualRatio = decWrkHrs / decShiftHrs;
                }
                return decOTActualRatio;
            }
            catch (Exception ex)
            {

                return decOTActualRatio;
            }
        }

        private void LoadEmployeeAttendanceRecordOrderByDate()
        {
            SAPbouiCOM.ProgressBar prog = null;
            string strEmpCode = "";
            int intEmpID = 0;
            string strEmpName = "";
            string strWorkHours = "";
            int RecordCounter = 0;
            decimal LeaveCount = 0;
            decimal OTRatio = 0;
            string shiftName = "";
            string strDesc = "";
            string shiftTimeIn = "";
            string shiftBefferTimeIn = "";
            string shiftBufferTimeOut = "";
            string shiftTimeOut = "";
            string shiftHours = "";
            string strTimeIn = "";
            string strTimeOut = "";
            string strOverTimeHours = "";
            string strOverTimeType = "";
            string strLateInMinutes = "";
            string strEarlyOutMinutes = "";
            string strLeaveHours = "";
            string strStatus = "";
            string strLeaveType = "";
            string strLeaveTypeCode = "";
            string strDayType = "";
            bool InflgOverlap = false;
            bool OutflgOverlap = false;
            bool isOverTimeApplicable = false;
            try
            {
                DateTime startDate = DateTime.MinValue;
                DateTime EndDate = DateTime.MinValue;
                if (dtEmployees == null && dtEmployees.Rows.Count <= 0)
                {
                    oApplication.StatusBar.SetText("Please Select Employee(s) First", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtFromDate.Value) && string.IsNullOrEmpty(txtToDate.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Attendance Process From and To Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value))
                {
                    startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    double totalEmps = ((EndDate.Subtract(startDate)).TotalDays + 1);
                    if (totalEmps > 0)
                    {
                        int TotalRecord = Convert.ToInt32(totalEmps);
                        prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", TotalRecord, false);
                        prog.Value = 0;
                    }
                    for (DateTime x = startDate; x <= EndDate; x = x.AddDays(1))
                    {
                        System.Windows.Forms.Application.DoEvents();
                        prog.Value += 1;

                        for (int i = 0; i < dtEmployees.Rows.Count; i++)
                        {
                            bool sel2 = (grdEmployees.Columns.Item("isSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                            if (!sel2)
                            {
                                continue;
                            }
                            strEmpCode = Convert.ToString(dtEmployees.GetValue("EmpCode", i));
                            strEmpName = Convert.ToString(dtEmployees.GetValue("EmpName", i));
                            var EmpREcord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();
                            intEmpID = EmpREcord.ID;
                            if (intEmpID > 0)
                            {
                                bool IsHoliday = false;
                                strDesc = string.Empty;
                                strTimeIn = string.Empty;
                                strTimeOut = string.Empty;
                                strWorkHours = string.Empty;
                                shiftBefferTimeIn = string.Empty;
                                shiftBufferTimeOut = string.Empty;
                                strOverTimeHours = string.Empty;
                                strLateInMinutes = string.Empty;
                                strEarlyOutMinutes = string.Empty;
                                strStatus = string.Empty;
                                strLeaveHours = string.Empty;
                                strLeaveType = string.Empty;
                                strOverTimeType = string.Empty;
                                strDayType = string.Empty;
                                strLeaveTypeCode = string.Empty;
                                isOverTimeApplicable = false;
                                string EmpCalenderID = EmpREcord.EmpCalender;
                                var AttendanceRegister = dbHrPayroll.TrnsTextileGroupAttendanceReg.Where(atr => atr.Date == x.Date && atr.EmpID == intEmpID && (atr.FlgPosted == false || atr.FlgPosted == null)).FirstOrDefault();
                                string dayofWeeks = Convert.ToString(x.DayOfWeek);
                                if (AttendanceRegister != null && AttendanceRegister.FlgProcessed == true)
                                {
                                    shiftName = string.IsNullOrEmpty(AttendanceRegister.MstShifts.Description) ? "" : AttendanceRegister.MstShifts.Description;
                                    var ShiftDetail = dbHrPayroll.MstShiftDetails.Where(S => S.Day == dayofWeeks && S.ShiftID == AttendanceRegister.MstShifts.Id).FirstOrDefault();
                                    if (ShiftDetail != null)
                                    {
                                        shiftTimeIn = ShiftDetail.StartTime;
                                        shiftBefferTimeIn = ShiftDetail.BufferStartTime;
                                        shiftBufferTimeOut = ShiftDetail.BufferEndTime;
                                        shiftTimeOut = ShiftDetail.EndTime;
                                        shiftHours = ShiftDetail.Duration;
                                    }
                                    strTimeIn = AttendanceRegister.TimeIn.Trim();
                                    strTimeOut = AttendanceRegister.TimeOut.Trim();
                                    strLateInMinutes = AttendanceRegister.LateInMin.Trim();
                                    strEarlyOutMinutes = string.IsNullOrEmpty(AttendanceRegister.EarlyOutMin) ? "" : AttendanceRegister.EarlyOutMin.Trim();
                                    if (AttendanceRegister.FlgOffDay != true)
                                    {
                                        strDayType = "WD";
                                        strDesc = "WorkDay";
                                    }
                                    else
                                    {
                                        strDayType = "OD";
                                        strDesc = "Off Day";
                                    }
                                    if (AttendanceRegister.FlgOnAbsent == true)
                                    {
                                        strDesc = "Leave / Absent";
                                    }
                                    strWorkHours = AttendanceRegister.WorkHours.Trim();
                                    strOverTimeHours = Convert.ToString(AttendanceRegister.OTCount);

                                    dtAttendance.Rows.Add(1);
                                    dtAttendance.SetValue("Id", RecordCounter, AttendanceRegister.ID);
                                    dtAttendance.SetValue("No", RecordCounter, RecordCounter + 1);
                                    dtAttendance.SetValue("EmpCode", RecordCounter, strEmpCode.Trim());
                                    dtAttendance.SetValue("EmpName", RecordCounter, strEmpName.Trim());
                                    dtAttendance.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                    dtAttendance.SetValue("Shift", RecordCounter, shiftName.Trim());
                                    dtAttendance.SetValue("SfStart", RecordCounter, shiftTimeIn.Trim());
                                    dtAttendance.SetValue("SfEnd", RecordCounter, shiftTimeOut.Trim());
                                    dtAttendance.SetValue("SfHours", RecordCounter, shiftHours.Trim());
                                    dtAttendance.SetValue("TimeIn", RecordCounter, strTimeIn.Trim());
                                    dtAttendance.SetValue("TimeOut", RecordCounter, strTimeOut.Trim());
                                    dtAttendance.SetValue("LateInMin", RecordCounter, strLateInMinutes.Trim());
                                    dtAttendance.SetValue("clDTp", RecordCounter, strDayType.Trim());
                                    dtAttendance.SetValue("EarlyOutMin", RecordCounter, strEarlyOutMinutes.Trim());
                                    dtAttendance.SetValue("Status", RecordCounter, strStatus.Trim());
                                    dtAttendance.SetValue("WorkHours", RecordCounter, strWorkHours.Trim());
                                    dtAttendance.SetValue("OTHours", RecordCounter, strOverTimeHours.Trim());
                                    dtAttendance.SetValue("LevCount", RecordCounter, Convert.ToString(AttendanceRegister.LeaveCount));
                                    dtAttendance.SetValue("clDesc", RecordCounter, strDesc.Trim());

                                    RecordCounter++;
                                }
                                else if (AttendanceRegister != null && AttendanceRegister.FlgProcessed != true)
                                {
                                    shiftName = string.IsNullOrEmpty(AttendanceRegister.MstShifts.Description) ? "" : AttendanceRegister.MstShifts.Description;
                                    var ShiftDetail = dbHrPayroll.MstShiftDetails.Where(S => S.Day == dayofWeeks && S.ShiftID == AttendanceRegister.MstShifts.Id).FirstOrDefault();
                                    if (ShiftDetail != null)
                                    {
                                        shiftTimeIn = ShiftDetail.StartTime;
                                        shiftBefferTimeIn = ShiftDetail.BufferStartTime;
                                        shiftBufferTimeOut = ShiftDetail.BufferEndTime;
                                        shiftTimeOut = ShiftDetail.EndTime;
                                        shiftHours = ShiftDetail.Duration;
                                        InflgOverlap = ShiftDetail.FlgInOverlap.Value;
                                        OutflgOverlap = ShiftDetail.FlgOutOverlap.Value;
                                    }
                                    //Get Attendance Record of the Day
                                    if (!InflgOverlap && !OutflgOverlap)
                                    {
                                        DateTime dtx = x.AddDays(1);
                                        //var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID == intEmpID && tr.PunchedDate == x).ToList();
                                        var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID == EmpREcord.EmpID && tr.PunchedDate == x).ToList();
                                        var TempAttendanceNextDay = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID == EmpREcord.EmpID && tr.PunchedDate == dtx && tr.In_Out == "2").ToList();
                                        if (TempAttendance != null && TempAttendance.Count > 0)
                                        {
                                            strTimeIn = Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime));
                                            //strTimeOut = Convert.ToString(TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime));
                                            //Calculating Out Time with Same Day
                                            var OUTTempAttendance = TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").ToList();
                                            if (OUTTempAttendance != null)
                                            {
                                                OUTTempAttendance = OUTTempAttendance.OrderBy(d => d.PunchedTime).ToList();
                                            }
                                            foreach (TrnsTempAttendance item in OUTTempAttendance)
                                            {
                                                string PunchTime = item.PunchedTime;
                                                decimal decPunchTime = ConvertTimeToDecimal(PunchTime);

                                                if (decPunchTime >= 7.52M && decPunchTime <= 24)
                                                {
                                                    //decPunchTime = decPunchTime + 24;
                                                    strTimeOut = PunchTime;
                                                }
                                            }
                                            foreach (TrnsTempAttendance item in TempAttendanceNextDay)
                                            {
                                                string PunchTime = item.PunchedTime;
                                                decimal decPunchTime = ConvertTimeToDecimal(PunchTime);

                                                if (decPunchTime >= 0 && decPunchTime < 7.52M)
                                                {
                                                    //decPunchTime = decPunchTime + 24;
                                                    strTimeOut = PunchTime;
                                                }
                                            }
                                            strWorkHours = CalculateWorkHours(strTimeIn, strTimeOut);
                                        }
                                    }
                                    //if (!InflgOverlap && !OutflgOverlap)
                                    //{
                                    //    var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID.ToString() == EmpREcord.EmpID && tr.PunchedDate == x).ToList();
                                    //    //var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID.ToString() == EmpREcord.ID.ToString() && tr.PunchedDate == x).ToList();
                                    //    if (TempAttendance != null && TempAttendance.Count > 0)
                                    //    {
                                    //        strTimeIn = Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime));
                                    //        strTimeOut = Convert.ToString(TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime));
                                    //        strWorkHours = CalculateWorkHours(strTimeIn, strTimeOut);
                                    //    }
                                    //}
                                    else if (!InflgOverlap && OutflgOverlap)
                                    {
                                        var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID.ToString() == EmpREcord.EmpID && tr.PunchedDate.Value.Date == x.Date).ToList();
                                        //var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID.ToString() == EmpREcord.ID.ToString() && tr.PunchedDate.Value.Date == x.Date).ToList();
                                        if (TempAttendance != null && TempAttendance.Count > 0)
                                        {
                                            strTimeIn = Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime));
                                        }
                                        DateTime dtx = x.AddDays(1);
                                        var TempAttendanceOverlap = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID.ToString() == EmpREcord.EmpID && tr.PunchedDate == dtx).ToList();
                                        //var TempAttendanceOverlap = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID.ToString() == EmpREcord.ID.ToString() && tr.PunchedDate == dtx).ToList();
                                        if (TempAttendanceOverlap != null && TempAttendanceOverlap.Count > 0)
                                        {
                                            strTimeOut = Convert.ToString(TempAttendanceOverlap.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendanceOverlap.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime));
                                        }
                                        strWorkHours = CalculateWorkHours(strTimeIn, strTimeOut);
                                    }
                                    else if (InflgOverlap && OutflgOverlap)
                                    {
                                        DateTime dtx = x.AddDays(1);
                                        var TempAttendance = dbHrPayroll.TrnsTempAttendance.Where(tr => tr.EmpID == EmpREcord.EmpID && tr.PunchedDate == dtx).ToList();
                                        if (TempAttendance != null && TempAttendance.Count > 0)
                                        {
                                            strTimeIn = Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "01" || str.In_Out == "1").Min(y => y.PunchedTime));
                                            strTimeOut = Convert.ToString(TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime)) == null ? "" : Convert.ToString(TempAttendance.Where(str => str.In_Out == "02" || str.In_Out == "2").Max(y => y.PunchedTime));
                                            strWorkHours = CalculateWorkHours(strTimeIn, strTimeOut);
                                        }
                                    }
                                    //Weekend Calulation
                                    if (!string.IsNullOrEmpty(shiftHours) && shiftHours == "00:00")
                                    {
                                        strDesc = "OFF DAY";
                                        LeaveCount = 0.0M;
                                        strDayType = "OD";
                                    }
                                    if (AttendanceRegister.FlgOffDay == true)
                                    {
                                        strDesc = "OFF DAY";
                                        strDayType = "OD";
                                        LeaveCount = 0.0M;
                                    }
                                    else
                                    {
                                        strDayType = "WD";
                                    }
                                    //Public Holiday Calculation                                 
                                    if (!string.IsNullOrEmpty(EmpCalenderID))
                                    {
                                        SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                                        string SQLHolidays = "SELECT HldCode,Rmrks FROM dbo.HLD1 WHERE HldCode = '" + EmpCalenderID + "' AND StrDate <= '" + x + "' AND EndDate >= '" + x + "' ";
                                        oRecSet.DoQuery(SQLHolidays);
                                        if (oRecSet.RecordCount > 0)
                                        {
                                            IsHoliday = true;
                                            strDesc = oRecSet.Fields.Item(1).Value;
                                        }
                                        if (IsHoliday)
                                        {
                                            shiftTimeIn = "00:00";
                                            shiftTimeOut = "00:00";
                                            shiftHours = "00:00";
                                            LeaveCount = 0.0M;
                                        }
                                    }
                                    //Mark Absent If Not Available
                                    if (!string.IsNullOrEmpty(shiftHours) && shiftHours != "00:00")
                                    {
                                        if (strDayType == "WD" && (string.IsNullOrEmpty(strTimeIn) || string.IsNullOrEmpty(strTimeOut)))
                                        {
                                            strDesc = "Leave / Absent";
                                        }
                                    }
                                    //Calculate Leaves in diffrent Scenerio IF Person Not Available
                                    if (string.IsNullOrEmpty(strTimeIn) && string.IsNullOrEmpty(strTimeOut) && !string.IsNullOrEmpty(shiftHours) && shiftHours != "00:00")
                                    {
                                        if (AttendanceRegister.FlgOffDay.Value != true)
                                        {
                                            string TempLeaveHours = "";
                                            LeaveCount = 0.0M;
                                            var Data = dbHrPayroll.TrnsLeavesRequest.Where(pd => pd.LeaveFrom <= x && x <= pd.LeaveTo && pd.EmpID == intEmpID).FirstOrDefault();
                                            if (Data != null && Data.UnitsID == "Day")
                                            {
                                                LeaveCount = 0.0M;
                                            }
                                            else if (Data != null && Data.UnitsID == "HalfDay")
                                            {
                                                TempLeaveHours = CalculateHalfShiftHours(shiftHours);                                                
                                                if (!string.IsNullOrEmpty(TempLeaveHours) && TempLeaveHours != "00:00")
                                                {
                                                    bool flgIsSupervisor = EmpREcord.FlgSuperVisor == null ? false : EmpREcord.FlgSuperVisor.Value;
                                                    LeaveCount = ReturnLeaveUnits(TempLeaveHours, flgIsSupervisor);
                                                }                                                
                                            }
                                            else
                                            {
                                                TempLeaveHours = shiftHours;
                                                LeaveCount = 0.00M;
                                                if (!string.IsNullOrEmpty(TempLeaveHours) && TempLeaveHours != "00:00")
                                                {
                                                    bool flgIsSupervisor = EmpREcord.FlgSuperVisor == null ? false : EmpREcord.FlgSuperVisor.Value;
                                                    LeaveCount = ReturnLeaveUnits(TempLeaveHours, flgIsSupervisor);
                                                }
                                            }
                                        }
                                    }
                                    //Calculate LeaveHours If Person is Available in Office but Working Hours Differ From Shift Hours
                                    if (!string.IsNullOrEmpty(strWorkHours))
                                    {
                                        string TempLeaveHours = "";                                                                             
                                        LeaveCount = 0.0M;
                                        TempLeaveHours = CalculateLeaveHours(shiftHours, strWorkHours);
                                        if (!string.IsNullOrEmpty(TempLeaveHours) && TempLeaveHours != "00:00")
                                        {
                                             bool flgIsSupervisor = EmpREcord.FlgSuperVisor == null ? false : EmpREcord.FlgSuperVisor.Value;
                                             LeaveCount = ReturnLeaveUnits(TempLeaveHours, flgIsSupervisor);                                                                                       
                                        }
                                    }
                                    //Calculate OverTime Here
                                    if (!string.IsNullOrEmpty(strWorkHours))
                                    {
                                        string strInOverTime = "00:00";
                                        string strOutOverTime = "00:00";
                                        bool flgOTApplicable = AttendanceRegister.MstEmployee.FlgOTApplicable == null ? false : AttendanceRegister.MstEmployee.FlgOTApplicable.Value;
                                        bool flgOtonWorkedHours = AttendanceRegister.MstShifts.FlgOTWrkHrs == null ? false : AttendanceRegister.MstShifts.FlgOTWrkHrs.Value;
                                        isOverTimeApplicable = AttendanceRegister.MstShifts.OverTime == null ? false : AttendanceRegister.MstShifts.OverTime.Value;
                                        if (!isOverTimeApplicable)
                                        {
                                            strOverTimeHours = "";
                                            strOverTimeType = "";
                                        }
                                        else
                                        {
                                            if (!flgOTApplicable)
                                            {
                                                strOverTimeHours = "";
                                                strOverTimeType = "";
                                            }
                                            else
                                            {
                                                if (flgOtonWorkedHours)
                                                {
                                                    strOverTimeHours = CalculateOverTimeHours(shiftHours, strWorkHours);
                                                    if (!string.IsNullOrEmpty(strOverTimeHours) && strOverTimeHours != "00:00")
                                                    {
                                                        strOverTimeType = dbHrPayroll.MstOverTime.Where(O => O.ID == AttendanceRegister.MstShifts.OverTimeID.Value).FirstOrDefault().Code;
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(shiftBefferTimeIn))
                                                    {
                                                        shiftBefferTimeIn = shiftTimeIn;
                                                    }
                                                    if (string.IsNullOrEmpty(shiftBufferTimeOut))
                                                    {
                                                        shiftBufferTimeOut = shiftTimeOut;
                                                    }
                                                    strInOverTime = IFInOvertimeApplicable(shiftBefferTimeIn, strTimeIn);
                                                    if (strInOverTime != "00:00")
                                                    {
                                                        //CalculateIN Overtime AccordingToShift
                                                        strInOverTime = CalculateInOvertimeApplicable(shiftTimeIn, strTimeIn);
                                                    }
                                                    strOutOverTime = IFOutOvertimeApplicable(strTimeOut, shiftBufferTimeOut, OutflgOverlap);
                                                    //strOutOverTime = IFOutOvertimeApplicable(strTimeOut, shiftBufferTimeOut);
                                                    if (strOutOverTime != "00:00")
                                                    {
                                                        //strOutOverTime = CalculateOutOvertimeApplicable(strTimeOut, shiftTimeOut);
                                                        strOutOverTime = CalculateOutOvertimeApplicable(strTimeOut, shiftTimeOut, OutflgOverlap);
                                                    }
                                                    strOverTimeHours = CalculateOverTimeHoursInandOutTime(strInOverTime, strOutOverTime);
                                                    //OverTime on Weekend
                                                    if (shiftHours == "00:00")
                                                    {
                                                        strOverTimeHours = strWorkHours;
                                                    }
                                                    if (strDayType != "WD")
                                                    {
                                                        strOverTimeHours = strWorkHours;
                                                    }
                                                    if (!string.IsNullOrEmpty(strOverTimeHours) && strOverTimeHours != "00:00")
                                                    {
                                                        strOverTimeType = dbHrPayroll.MstOverTime.Where(O => O.ID == AttendanceRegister.MstShifts.OverTimeID.Value).FirstOrDefault().Code;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    //Calculate LateIn Minutes
                                    if (!string.IsNullOrEmpty(strTimeIn))
                                    {
                                        strLateInMinutes = CalculateLateInMinutes(shiftTimeIn, strTimeIn);
                                        //Buffer Period  Included Here
                                        var AttendanceRule = dbHrPayroll.MstAttendanceRule.Where(ru => ru.FlgGpActive == false).FirstOrDefault();
                                        if (AttendanceRule != null)
                                        {
                                            string BufferInTime = AttendanceRule.GpAfterStartTime;
                                            if (!string.IsNullOrEmpty(BufferInTime) && BufferInTime != "00:00" && !string.IsNullOrEmpty(strLateInMinutes) && strLateInMinutes != "00:00")
                                            {
                                                if (!IsBufferApplicable(BufferInTime, strLateInMinutes))
                                                {
                                                    strLateInMinutes = "00:00";
                                                }
                                            }
                                        }
                                    }
                                    //Calculate Earlyout Minutes
                                    if (!string.IsNullOrEmpty(strTimeOut))
                                    {
                                        decimal decTimeOut = ConvertTimeToDecimal(strTimeOut);
                                        decimal decTimeIn = 0.0M;
                                        if (strTimeIn != null)
                                        {
                                            decTimeIn = ConvertTimeToDecimal(strTimeIn);
                                        }

                                        if (decTimeIn > decTimeOut && OutflgOverlap == false)
                                        {
                                            //Do Nothing
                                            strEarlyOutMinutes = "00:00";
                                        }
                                        else
                                        {
                                            strEarlyOutMinutes = CalculateEarlyOutMinutes(shiftTimeOut, strTimeOut);
                                            var AttendanceRule = dbHrPayroll.MstAttendanceRule.Where(ru => ru.FlgGpActive == false).FirstOrDefault();
                                            if (AttendanceRule != null)
                                            {
                                                string BufferOutTime = AttendanceRule.GpBeforeTimeEnd;
                                                if (!string.IsNullOrEmpty(BufferOutTime) && BufferOutTime != "00:00" && !string.IsNullOrEmpty(strEarlyOutMinutes) && strEarlyOutMinutes != "00:00")
                                                {
                                                    if (!IsBufferApplicable(BufferOutTime, strEarlyOutMinutes))
                                                    {
                                                        strEarlyOutMinutes = "00:00";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Status is not in Use in Current Version
                                    if (!string.IsNullOrEmpty(strLateInMinutes) && !string.IsNullOrEmpty(strEarlyOutMinutes))
                                    {
                                        strStatus = GetAttendanceStatus_NEW(strLateInMinutes, strEarlyOutMinutes);
                                    }
                                    if (!string.IsNullOrEmpty(strOverTimeHours))
                                    {
                                        OTRatio = GetOTHrsRatio(shiftHours, strOverTimeHours);
                                    }
                                    dtAttendance.Rows.Add(1);
                                    dtAttendance.SetValue("Id", RecordCounter, AttendanceRegister.ID);
                                    dtAttendance.SetValue("No", RecordCounter, RecordCounter + 1);
                                    dtAttendance.SetValue("EmpCode", RecordCounter, strEmpCode.Trim());
                                    dtAttendance.SetValue("EmpName", RecordCounter, strEmpName.Trim());
                                    dtAttendance.SetValue("Date", RecordCounter, Convert.ToDateTime(x).ToString("yyyyMMdd"));
                                    dtAttendance.SetValue("Shift", RecordCounter, shiftName.Trim());
                                    dtAttendance.SetValue("SfStart", RecordCounter, shiftTimeIn.Trim());
                                    dtAttendance.SetValue("SfEnd", RecordCounter, shiftTimeOut.Trim());
                                    dtAttendance.SetValue("SfHours", RecordCounter, shiftHours.Trim());
                                    dtAttendance.SetValue("TimeIn", RecordCounter, strTimeIn.Trim());
                                    dtAttendance.SetValue("TimeOut", RecordCounter, strTimeOut.Trim());
                                    dtAttendance.SetValue("LateInMin", RecordCounter, strLateInMinutes.Trim());
                                    dtAttendance.SetValue("clDTp", RecordCounter, strDayType.Trim());
                                    dtAttendance.SetValue("EarlyOutMin", RecordCounter, strEarlyOutMinutes.Trim());
                                    dtAttendance.SetValue("Status", RecordCounter, strStatus.Trim());
                                    dtAttendance.SetValue("WorkHours", RecordCounter, strWorkHours.Trim());
                                    dtAttendance.SetValue("OTHours", RecordCounter, string.Format("{0:0.00}", OTRatio));
                                    dtAttendance.SetValue("LevCount", RecordCounter, Convert.ToString(LeaveCount));
                                    dtAttendance.SetValue("clDesc", RecordCounter, strDesc.Trim());

                                    RecordCounter++;
                                }
                            }

                        }
                    }
                }
                grdAttendance.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: LoadEmployeeAttendanceRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                if (prog != null)
                {
                    prog.Stop();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                }
                prog = null;
            }
        }
        

        #endregion
    }
}
