using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_VAttPost : HRMSBaseForm
    {
        #region "Global Variable Area"

        private bool Validate;
        SAPbouiCOM.Button btnNext, btnSerch, btnClear, btnBack, btnSave, btnID, btnId2;
        SAPbouiCOM.EditText txtEmpIdFrom, txtEmpIdTo, txtFromDate, txtToDate;
        SAPbouiCOM.ComboBox cb_Location, cb_depart, cb_deignation, cb_payroll, cbPeriod, cmbLType;
        SAPbouiCOM.DataTable dtEmployees, dtAttendance, dtPeriods;
        SAPbouiCOM.Matrix grdEmployees, grdAttendance;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clId, clLCount, clNo, EmpCode, EmpName, Desig, Depart, Location, clSToTalDays, clSLeaveDays, clsAbsentDays, clsWorkDays, clsLaTeDays, clsLatePen, clsSatSunPen, clsSPecialPen, clConOff, isSel, clSpdays, clssdays, clCnOff, clNormalLeave, clTotal, clLType, clsOTH, clsOTT;
        SAPbouiCOM.Item IgrdEmployees, IgrdAttendance, ItxtEmpIdFrom, ItxtEmpIdTo, IbtnID, IbtnId2, Icb_Location, Icb_depart, Icb_deignation, IbtnBack, IbtnSave, Icb_payroll, IcbPeriod, IcmbLType;

        #endregion

        #region "B1 Events"

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
                if (pVal.ItemUID == "cb_prl")
                {
                    FillPeriod(cb_payroll.Value);
                }

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }

        }
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
                FillPayrollTypeInCombo();
                //FillLeaveTypeInCombo();
                //FillOverTimeTypeInCombo();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                        //picEmpFrom();
                        OpenNewSearchForm();
                        break;
                    case "btId2":
                        //picEmpTo();
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
                        saveRecord();
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
            try
            {
                if (pVal.ColUID == "TmIn" || pVal.ColUID == "TmOut")
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
                        string TimeIn = (grdAttendance.Columns.Item("TmIn").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string ShiftTimeIn = (grdAttendance.Columns.Item("SfStart").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string TimeOut = (grdAttendance.Columns.Item("TmOut").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string shiftTimeOut = (grdAttendance.Columns.Item("SfEnd").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string shiftHours = (grdAttendance.Columns.Item("SfHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string ActualWorkingHours = (grdAttendance.Columns.Item("WHrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string ShiftName = (grdAttendance.Columns.Item("shft").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        string strEMPID = (grdAttendance.Columns.Item("EmpCode").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        DateTime shiftDateX = DateTime.MinValue;
                        string shftDate = (grdAttendance.Columns.Item("Date").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        shiftDateX = DateTime.ParseExact(shftDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime x = Convert.ToDateTime(shiftDateX);
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

                cmbLType = oForm.Items.Item("cmbLType").Specific;
                IcmbLType = oForm.Items.Item("cmbLType");

                cb_payroll = oForm.Items.Item("cb_prl").Specific;
                Icb_payroll = oForm.Items.Item("cb_prl");

                cbPeriod = oForm.Items.Item("cb_prd").Specific;
                IcbPeriod = oForm.Items.Item("cb_prd");

                //Initializing Date Fields                

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
                dtAttendance.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAttendance.Columns.Add("EmpCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("TotalDays", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("LeaveDays", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("WorkDays", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("LateDays", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("LatePenalty", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                dtAttendance.Columns.Add("clTotal", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                //dtAttendance.Columns.Add("clLType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAttendance.Columns.Add("clsOTH", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 5);
                //dtAttendance.Columns.Add("clsOTT", SAPbouiCOM.BoFieldsType.ft_Text);

                grdAttendance = (SAPbouiCOM.Matrix)oForm.Items.Item("grdAtt").Specific;
                IgrdAttendance = oForm.Items.Item("grdAtt");
                oColumns = (SAPbouiCOM.Columns)grdAttendance.Columns;


                oColumn = oColumns.Item("No");
                clNo = oColumn;
                oColumn.DataBind.Bind("Attendance", "No");

                oColumn = oColumns.Item("EmpCode");
                EmpCode = oColumn;
                oColumn.DataBind.Bind("Attendance", "EmpCode");

                oColumn = oColumns.Item("EmpName");
                EmpName = oColumn;
                oColumn.DataBind.Bind("Attendance", "EmpName");


                oColumn = oColumns.Item("TDays");
                clSToTalDays = oColumn;
                oColumn.DataBind.Bind("Attendance", "TotalDays");

                oColumn = oColumns.Item("LDays");
                clSLeaveDays = oColumn;
                oColumn.DataBind.Bind("Attendance", "LeaveDays");


                oColumn = oColumns.Item("WDays");
                clsWorkDays = oColumn;
                oColumn.DataBind.Bind("Attendance", "WorkDays");

                oColumn = oColumns.Item("LTDays");
                clsLaTeDays = oColumn;
                oColumn.DataBind.Bind("Attendance", "LateDays");

                oColumn = oColumns.Item("LPen");
                clsLatePen = oColumn;
                oColumn.DataBind.Bind("Attendance", "LatePenalty");

                oColumn = oColumns.Item("clTotal");
                clTotal = oColumn;
                oColumn.DataBind.Bind("Attendance", "clTotal");

                //oColumn = oColumns.Item("clLType");
                //clLType = oColumn;
                //oColumn.DataBind.Bind("Attendance", "clLType");

                oColumn = oColumns.Item("clsOTH");
                clsOTH = oColumn;
                oColumn.DataBind.Bind("Attendance", "clsOTH");

                //oColumn = oColumns.Item("clsOTT");
                //clsOTT = oColumn;
                //oColumn.DataBind.Bind("Attendance", "clsOTT");

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
                var Departments = from a in dbHrPayroll.MstDepartment where a.FlgActive == true select a;
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
        private void FillLeaveTypeInCombo()
        {
            try
            {
                var LeaveType = from a in dbHrPayroll.MstLeaveType where a.Active == true select a;
                clLType.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstLeaveType empLocation in LeaveType)
                {
                    clLType.ValidValues.Add(Convert.ToString(empLocation.ID), Convert.ToString(empLocation.Description));
                }


            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillEmpLocationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void FillOverTimeTypeInCombo()
        {
            try
            {
                var OTTType = from a in dbHrPayroll.MstOverTime where a.FlgActive == true select a;
                clsOTT.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstOverTime empLocation in OTTType)
                {
                    clsOTT.ValidValues.Add(Convert.ToString(empLocation.ID), Convert.ToString(empLocation.Description));
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillEmpLocationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void FillPayrollTypeInCombo()
        {
            try
            {
                var payrollDef = from a in dbHrPayroll.CfgPayrollDefination select a;
                cb_payroll.ValidValues.Add("-1", "");
                foreach (CfgPayrollDefination pr in payrollDef)
                {
                    cb_payroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);
                }
                var PayrollDefData = dbHrPayroll.CfgPayrollDefination.Where(p => p.FlgIsDefault == true).FirstOrDefault();
                if (PayrollDefData != null)
                {
                    cb_payroll.Select(PayrollDefData.ID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillOvertimeTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillPeriod(string payroll)
        {
            try
            {
                if (cbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPeriod.ValidValues.Remove(cbPeriod.ValidValues.Item(k).Value);
                    }
                }
                int i = 0;
                string selId = "0";
                int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Count();
                if (cnt > 0)
                {
                    CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Single();
                    IEnumerable<CfgPeriodDates> prdList = pr.CfgPeriodDates.Where(r => r.FlgLocked == false).ToList();
                    foreach (CfgPeriodDates pd in prdList)
                    {
                        cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());

                        if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                        {
                            selId = pd.ID.ToString();
                        }

                        i++;
                    }
                    try
                    {
                        cbPeriod.Select(selId);
                    }
                    catch { }

                    //foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    //{
                    //    cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());

                    //    if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                    //    {
                    //        selId = pd.ID.ToString();
                    //    }

                    //    i++;
                    //}
                    //try
                    //{
                    //    cbPeriod.Select(selId);
                    //}
                    //catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
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
            if (!string.IsNullOrEmpty(cb_payroll.Value) && cb_payroll.Value != "-1")
            {
                Data = Data.Where(e => e.PayrollID == Convert.ToInt32(cb_payroll.Value)).ToList();
            }
            else
            {
                Data = null;
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
                if (!string.IsNullOrEmpty(cbPeriod.Value))
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
        private int CalculatingLateComingPenalty(int lateDays, int empid)
        {
            try
            {
                int intLateComingID = 4;
                string strLateComingCode = "PR_04";
                int evaluationdays = 0;
                int penaltydays = 0;
                int actualdays = 0;

                var EmpPenaltyRecord = dbHrPayroll.TrnsEmployeePenalty.Where(e => e.EmpId == empid && e.PenaltyId == intLateComingID).FirstOrDefault();
                if (EmpPenaltyRecord != null)
                {
                    evaluationdays = EmpPenaltyRecord.Days.Value;
                    penaltydays = EmpPenaltyRecord.PenaltyDays.Value;
                    actualdays = lateDays / evaluationdays;
                    actualdays = actualdays * penaltydays;
                    return actualdays;
                }
                else
                {
                    //If Rule Not Found Then Mark 0 day for penalty
                    return 0;
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: CalculatingLateComingPenalty Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }
        }
        private int CalculatingSatSundayPenalty(int satSundayDays, int empid)
        {
            try
            {
                int intsatSundayID = 1;
                string strsatSundayCode = "PR_01";
                int evaluationdays = 0;
                int penaltydays = 0;
                int actualdays = 0;

                var EmpPenaltyRecord = dbHrPayroll.TrnsEmployeePenalty.Where(e => e.EmpId == empid && e.PenaltyId == intsatSundayID).FirstOrDefault();
                if (EmpPenaltyRecord != null)
                {
                    evaluationdays = EmpPenaltyRecord.Days.Value;
                    penaltydays = EmpPenaltyRecord.PenaltyDays.Value;
                    actualdays = satSundayDays / evaluationdays;
                    actualdays = actualdays * penaltydays;
                    return actualdays;
                }
                else
                {
                    return satSundayDays;
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: CalculatingLateComingPenalty Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }
        }
        private int CalculatingSpecialDayPenalty(int specialDays, int empid)
        {
            try
            {
                int intspecialID = 3;
                string strspecialCode = "PR_03";
                int evaluationdays = 0;
                int penaltydays = 0;
                int actualdays = 0;

                var EmpPenaltyRecord = dbHrPayroll.TrnsEmployeePenalty.Where(e => e.EmpId == empid && e.PenaltyId == intspecialID).FirstOrDefault();
                if (EmpPenaltyRecord != null)
                {
                    evaluationdays = EmpPenaltyRecord.Days.Value;
                    penaltydays = EmpPenaltyRecord.PenaltyDays.Value;
                    actualdays = evaluationdays / specialDays;
                    actualdays = actualdays * penaltydays;
                    return actualdays;
                }
                else
                {
                    return specialDays;
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: CalculatingSpecialDayPenalty Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }
        }
        private int CalculatingConsectiveoffDayPenalty(int consectiveDays, int empid)
        {
            try
            {
                int intspecialID = 2;
                string strspecialCode = "PR_02";
                int evaluationdays = 0;
                int penaltydays = 0;
                int actualdays = 0;

                var EmpPenaltyRecord = dbHrPayroll.TrnsEmployeePenalty.Where(e => e.EmpId == empid && e.PenaltyId == intspecialID).FirstOrDefault();
                if (EmpPenaltyRecord != null)
                {
                    evaluationdays = EmpPenaltyRecord.Days.Value;
                    penaltydays = EmpPenaltyRecord.PenaltyDays.Value;
                    actualdays = consectiveDays / evaluationdays;
                    actualdays = actualdays * penaltydays;
                    return actualdays;
                }
                else
                {
                    return consectiveDays;
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: CalculatingSpecialDayPenalty Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }
        }
        private int CalculatingConsectiveOffDays(int empid, int periodId)
        {
            try
            {
                int conOffCount = 0;
                var OffDaysRecord = dbHrPayroll.TrnsAttendanceRegisterTS.Where(r => r.PeriodID == periodId && r.FlgOffDay == true && r.EmpID == empid).ToList();
                if (OffDaysRecord != null)
                {
                    foreach (TrnsAttendanceRegisterTS item in OffDaysRecord)
                    {
                        DateTime dtWeekendDate = item.Date.Value;
                        DateTime dtDateAfterHoliday = dtWeekendDate.AddDays(1);
                        DateTime dtDateBeforeHoliday = dtWeekendDate.AddDays(-1);
                        var OneDayAfterHoliday = dbHrPayroll.TrnsAttendanceRegisterTS.Where(r => r.PeriodID == periodId && r.EmpID == empid && r.Date == dtDateAfterHoliday && r.OnAbsent == true).FirstOrDefault();
                        if (OneDayAfterHoliday != null)
                        {
                            conOffCount = conOffCount + 1;
                        }
                        var OneDayBeforeHoliday = dbHrPayroll.TrnsAttendanceRegisterTS.Where(r => r.PeriodID == periodId && r.EmpID == empid && r.Date == dtDateBeforeHoliday && r.OnAbsent == true).FirstOrDefault();
                        if (OneDayBeforeHoliday != null)
                        {
                            conOffCount = conOffCount + 1;
                        }
                    }
                }
                return conOffCount;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: CalculatingConsectiveOffDays Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
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

            string strDesc = "";

            string strTimeIn = "";
            string strTimeOut = "";
            string strLateInMinutes = "";
            string strStatus = "";

            try
            {
                DateTime startDate = DateTime.MinValue;
                DateTime EndDate = DateTime.MinValue;
                if (dtEmployees == null && dtEmployees.Rows.Count <= 0)
                {
                    oApplication.StatusBar.SetText("Please Select Employee(s) First", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cbPeriod.Value))
                {
                    oApplication.StatusBar.SetText("Please Select Attendance Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (!string.IsNullOrEmpty(cbPeriod.Value) && cbPeriod.Value != "0")
                {
                    var perioddates = dbHrPayroll.CfgPeriodDates.Where(d => d.ID == Convert.ToInt32(cbPeriod.Value)).FirstOrDefault();
                    if (perioddates != null)
                    {
                        startDate = perioddates.StartDate.Value;
                        EndDate = perioddates.EndDate.Value;
                    }
                    double totalEmps = ((EndDate.Subtract(startDate)).TotalDays + 1);
                    if (totalEmps > 0)
                    {
                        int TotalRecord = Convert.ToInt32(totalEmps);
                        prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", TotalRecord, false);
                        prog.Value = 0;
                    }
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        prog.Value += 1;

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
                            double TotalDays = ((EndDate.Subtract(startDate)).TotalDays + 1);
                            decimal OTHrs = 0.0M;
                            decimal AbsentDays = 0;
                            double LeaveDays = 0;
                            int LateDays = 0;
                            decimal decLeaveDays = 0;
                            decimal totalLeaves = 0;
                            double WorkDays = 0;
                            int lateCOmingPenalty = 0;
                            strDesc = string.Empty;
                            strTimeIn = string.Empty;
                            strTimeOut = string.Empty;
                            strWorkHours = string.Empty;
                            strLateInMinutes = string.Empty;
                            strStatus = string.Empty;
                            var AttendanceRegister = dbHrPayroll.TrnsTextileGroupAttendanceReg.Where(atr => atr.PeriodID == Convert.ToInt32(cbPeriod.Value) && atr.EmpID == intEmpID && (atr.FlgPosted == null || atr.FlgPosted == false)).ToList();
                            if (AttendanceRegister != null)
                            {

                                decimal sumLineTotal = AttendanceRegister.Sum(od => od.OTCount ?? 0);
                                OTHrs = sumLineTotal;
                                decLeaveDays = AttendanceRegister.Sum(od => od.LeaveCount ?? 0);
                                if (decLeaveDays > 0)
                                {
                                    LeaveDays = Convert.ToDouble(decLeaveDays);
                                }
                                LateDays = AttendanceRegister.Where(a => !string.IsNullOrEmpty(a.LateInMin) && a.LateInMin != "00:00" && a.FlgOffDay != true).Count();
                            }
                            //Calculating LateDays Penalty
                            lateCOmingPenalty = CalculatingLateComingPenalty(LateDays, intEmpID);
                            //Calculating Work Days
                            WorkDays = TotalDays - LeaveDays;
                            totalLeaves = decLeaveDays;

                            dtAttendance.Rows.Add(1);
                            dtAttendance.SetValue("No", RecordCounter, RecordCounter + 1);
                            dtAttendance.SetValue("EmpCode", RecordCounter, strEmpCode.Trim());
                            dtAttendance.SetValue("EmpName", RecordCounter, strEmpName.Trim());
                            dtAttendance.SetValue("TotalDays", RecordCounter, string.Format("{0:0.00}", TotalDays));
                            dtAttendance.SetValue("LeaveDays", RecordCounter, string.Format("{0:0.00}", LeaveDays));
                            dtAttendance.SetValue("WorkDays", RecordCounter, string.Format("{0:0.00}", WorkDays));
                            dtAttendance.SetValue("LateDays", RecordCounter, string.Format("{0:0.00}", LateDays));
                            dtAttendance.SetValue("clsOTH", RecordCounter, string.Format("{0:0.00}", OTHrs));
                            dtAttendance.SetValue("clTotal", RecordCounter, string.Format("{0:0.00}", totalLeaves));
                            //if (totalLeaves >= 0)
                            //{
                            //    var defaultleavetype = dbHrPayroll.MstLeaveType.Where(l => l.FlgDefault == true).FirstOrDefault();
                            //    if (defaultleavetype != null)
                            //    {
                            //        dtAttendance.SetValue("clLType", RecordCounter, defaultleavetype.ID.ToString());
                            //    }
                            //}
                            //if (OTHrs >= 0)
                            //{
                            //    var defaultOTtype = dbHrPayroll.MstOverTime.Where(l => l.FlgDefault == true).FirstOrDefault();
                            //    if (defaultOTtype != null)
                            //    {
                            //        dtAttendance.SetValue("clsOTT", RecordCounter, defaultOTtype.ID.ToString());
                            //    }
                            //}

                            RecordCounter++;
                        }
                    }

                    grdAttendance.LoadFromDataSource();
                }
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
        private decimal setRowAmnt(MstEmployee emp, decimal Overtimehours, string Periodid, string strOverTimeType)
        {
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValue = 0.0M;
            decimal daysinYear = 0.0M;
            decimal amount = 0.0M;
            decimal baseValue = 0.00M;
            decimal value = 0.00M;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            decimal monthHours = Convert.ToDecimal(30.00 * 8.00);
            try
            {
                string code = strOverTimeType; //cb.Value; //Convert.ToString(dtOT.GetValue("Code", rowNum));
                if (string.IsNullOrEmpty(code) || code == "-1")
                {
                    oApplication.StatusBar.SetText("Please select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return 0;
                }
                if (!string.IsNullOrEmpty(code))
                {
                    var OTTYpe = dbHrPayroll.MstOverTime.Where(o => o.ID.ToString() == code).FirstOrDefault();
                    if (OTTYpe != null)
                    {
                        value = Convert.ToDecimal(OTTYpe.Value.Value);
                        daysOT = string.IsNullOrEmpty(OTTYpe.Days) ? Convert.ToInt16(0) : Convert.ToInt16(OTTYpe.Days);
                        HoursOT = string.IsNullOrEmpty(OTTYpe.Hours) ? 0 : Convert.ToDecimal(OTTYpe.Hours);
                        fixValue = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        if (OTTYpe.ValueType == "POB")
                        {
                            baseValue = (decimal)emp.BasicSalary;
                        }
                        if (OTTYpe.ValueType == "POG")
                        {
                            baseValue = ds.getEmpGross(emp);
                        }
                        if (OTTYpe.ValueType == "Fix")
                        {
                            baseValue = OTTYpe.Value.Value;
                        }
                    }
                }
                if (HoursOT > 0)
                {
                    workhours = HoursOT;
                }
                if (daysOT > 0)
                {
                    days = daysOT;
                }
                if (daysOT <= 0)
                {
                    string PayrollPeriod = Periodid;// cbPeriod.Value.Trim();
                    if (!string.IsNullOrEmpty(PayrollPeriod))
                    {
                        CfgPeriodDates LeaveFromPeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                        if (LeaveFromPeriod != null)
                        {
                            if (days < 1)
                            {
                                days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                            }
                            else if (days < 1)
                            {
                                days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                            }
                        }
                    }
                }
                monthHours = Convert.ToDecimal(days * workhours);
                decimal hours = Overtimehours; //Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
                decimal baseAmoun = baseValue;  //Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
                decimal Val = value; //Convert.ToDecimal(dtOT.GetValue("Value", rowNum));
                if (fixValue > 0 && daysinYear > 0)
                {
                    baseAmoun = baseAmoun + fixValue;
                    baseAmoun = baseAmoun * 12;
                    baseAmoun = baseAmoun / daysinYear;
                    baseAmoun = baseAmoun / workhours;
                    //baseAmoun = baseAmoun * 2;  //2 Tiem of Noraml Working Hours
                    amount = ((baseAmoun) * Val / 100) * hours;
                    //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                }
                else
                {
                    amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                }
                //dtOT.SetValue("Amount", rowNum, amount.ToString());

                return amount;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function setRowAmnt.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }
        private void saveRecord_OLD()
        {
            string strEmpCode = "";
            string strLeaveCont = "";
            string strOTCount = "";
            int leaveTypeId = 0;
            int intLeaveCount = 0;
            int OverTimeType = 0;
            string strLeaveType = "";
            string strOTType = "";
            try
            {
                int periodId = Convert.ToInt32(cbPeriod.Value);
                int payrollIdd = Convert.ToInt32(cb_payroll.Value);
                //int leaveTypeId = Convert.ToInt32(cmbLType.Value);
                var PeriodDate = dbHrPayroll.CfgPeriodDates.Where(e => e.ID == periodId).FirstOrDefault();
                if (PeriodDate != null)
                {
                    DateTime dtStartDate = PeriodDate.StartDate.Value;

                    for (int i = 1; i < grdAttendance.RowCount + 1; i++)
                    {
                        strEmpCode = (grdAttendance.Columns.Item("EmpCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        strLeaveCont = (grdAttendance.Columns.Item("clTotal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        strOTCount = (grdAttendance.Columns.Item("clsOTH").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        //strLeaveType = (grdAttendance.Columns.Item("clLType").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        //strOTType = (grdAttendance.Columns.Item("clsOTT").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        if (!string.IsNullOrEmpty(strLeaveCont) && !string.IsNullOrEmpty(strOTCount))
                        {
                            decimal decLeaveCount = string.IsNullOrEmpty(strLeaveCont) ? 0 : Convert.ToInt32(strLeaveCont);
                            decimal decOverTimeCount = string.IsNullOrEmpty(strOTCount) ? 0 : Convert.ToDecimal(strOTCount);
                            //if (!string.IsNullOrEmpty(strLeaveCont))
                            //{
                            //    decimal leaveCount = string.IsNullOrEmpty(strLeaveCont) ? 0 : Convert.ToDecimal(strLeaveCont);
                            //    if (leaveCount > 0)
                            //    {
                            //        if (string.IsNullOrEmpty(strLeaveType))
                            //        {
                            //            oApplication.StatusBar.SetText("Please Select Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            //            return;
                            //        }
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(strOTCount))
                            //{
                            //    if (decOverTimeCount > 0)
                            //    {
                            //        if (string.IsNullOrEmpty(strOTType))
                            //        {
                            //            oApplication.StatusBar.SetText("Please Select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            //            return;
                            //        }
                            //    }
                            //    //decimal decOTCount = string.IsNullOrEmpty(strOTCount) ? 0 : Convert.ToDecimal(strOTCount);
                            //    //if (decOTCount > 0)
                            //    //{
                            //    //    oApplication.StatusBar.SetText("Please Select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            //    //    return;
                            //    //}
                            //}
                            //Only For Naheed Disbale here
                            //int intleavetype = Convert.ToInt32(strLeaveType);
                            //int intOvertimetype = Convert.ToInt32(strOTType);
                            //AddAdjustmentRequest(strEmpCode, periodId, payrollIdd, decLeaveCount, decOverTimeCount, intleavetype, intOvertimetype);
                            //Direct Hit Non-Naheed
                            //if (string.IsNullOrEmpty(strLeaveType))
                            //{
                            //    oApplication.StatusBar.SetText("Please Select Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            //    return;
                            //}                            
                            //=========================
                        }
                        if (!string.IsNullOrEmpty(strLeaveCont))
                        {
                            intLeaveCount = string.IsNullOrEmpty(strLeaveCont) ? 0 : Convert.ToInt32(strLeaveCont);
                            leaveTypeId = string.IsNullOrEmpty(strLeaveType) ? -1 : Convert.ToInt32(strLeaveType);
                            DateTime dtENdDate = dtStartDate.AddDays(intLeaveCount);
                            if (intLeaveCount > 0)
                            {
                                if (string.IsNullOrEmpty(strLeaveType))
                                {
                                    oApplication.StatusBar.SetText("Please Select Leave Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                //Enter Leave Request
                                AddNewLeaveRequest(strEmpCode, leaveTypeId, dtStartDate, dtENdDate, intLeaveCount);
                            }
                        }
                        //Direct Hit Non-Naheed Posting.
                        if (!string.IsNullOrEmpty(strOTCount))
                        {
                            decimal OverTimeCount = string.IsNullOrEmpty(strOTCount) ? 0 : Convert.ToDecimal(strOTCount);
                            if (OverTimeCount > 0)
                            {
                                if (string.IsNullOrEmpty(strOTType))
                                {
                                    oApplication.StatusBar.SetText("Please Select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                else
                                {
                                    OverTimeType = Convert.ToInt32(strOTType);
                                    if (OverTimeCount > 0)
                                    {
                                        //Enter OverTime Request
                                        AddNewOverTimeRequest(strEmpCode, periodId, OverTimeType, OverTimeCount, dtStartDate);
                                    }
                                }
                            }
                        }
                        //========================
                        var EmpR = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();
                        if (EmpR != null)
                        {
                            var AttendancePosted = dbHrPayroll.TrnsAttendanceRegisterTS.Where(r => r.EmpID == EmpR.ID && r.PeriodID == periodId).ToList();
                            if (AttendancePosted != null)
                            {
                                foreach (var item in AttendancePosted)
                                {
                                    item.Posted = true;
                                    item.PostedBy = oCompany.UserName;
                                    item.UpdatedDate = DateTime.Now;
                                }
                                dbHrPayroll.SubmitChanges();
                            }
                        }
                    }
                    dtAttendance.Rows.Clear();
                    grdAttendance.LoadFromDataSource();
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: saveRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void saveRecord()
        {
            string strEmpCode = "";
            string strLeaveCont = "";
            string strOTCount = "";
            int leaveTypeId = 0;
            int intLeaveCount = 0;
            int OverTimeType = 0;
            string strLeaveType = "0";
            string strOTType = "0";
            try
            {
                int periodId = Convert.ToInt32(cbPeriod.Value);
                int payrollIdd = Convert.ToInt32(cb_payroll.Value);
                //int leaveTypeId = Convert.ToInt32(cmbLType.Value);
                var PeriodDate = dbHrPayroll.CfgPeriodDates.Where(e => e.ID == periodId).FirstOrDefault();
                if (PeriodDate != null)
                {
                    DateTime dtStartDate = PeriodDate.StartDate.Value;

                    for (int i = 1; i < grdAttendance.RowCount + 1; i++)
                    {
                        strEmpCode = (grdAttendance.Columns.Item("EmpCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        strLeaveCont = (grdAttendance.Columns.Item("clTotal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        strOTCount = (grdAttendance.Columns.Item("clsOTH").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        //strLeaveType = (grdAttendance.Columns.Item("clLType").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        //strOTType = (grdAttendance.Columns.Item("clsOTT").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;

                        if (!string.IsNullOrEmpty(strLeaveCont) && !string.IsNullOrEmpty(strOTCount))
                        {
                            decimal decLeaveCount = Convert.ToDecimal(strLeaveCont);
                            decimal decOverTimeCount = Convert.ToDecimal(strOTCount);
                            int intleavetype = Convert.ToInt32(strLeaveType);
                            int intOvertimetype = Convert.ToInt32(strOTType);

                            AddAdjustmentRequest(strEmpCode, periodId, payrollIdd, decLeaveCount, decOverTimeCount, intleavetype, intOvertimetype);
                        }
                        var EmpR = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEmpCode).FirstOrDefault();
                        if (EmpR != null)
                        {
                            var AttendancePosted = dbHrPayroll.TrnsTextileGroupAttendanceReg.Where(r => r.EmpID == EmpR.ID && r.PeriodID == periodId).ToList();
                            if (AttendancePosted != null)
                            {
                                foreach (var item in AttendancePosted)
                                {
                                    item.FlgPosted = true;
                                    item.PostedBy = oCompany.UserName;
                                    item.UpdatedDate = DateTime.Now;
                                }
                                dbHrPayroll.SubmitChanges();
                            }
                        }
                    }
                    dtAttendance.Rows.Clear();
                    grdAttendance.LoadFromDataSource();
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: saveRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void AddNewLeaveRequest(string empid, int leaveType, DateTime leaveFromdate, DateTime leaveToDate, int leaveCount)
        {
            try
            {
                int? intIdt = dbHrPayroll.TrnsLeavesRequest.Max(u => (int?)u.ID);
                intIdt = intIdt == null ? 1 : intIdt + 1;

                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == empid).FirstOrDefault();
                TrnsLeavesRequest objLeaveRequest = new TrnsLeavesRequest();
                objLeaveRequest.EmpID = EmpRecord.ID;
                objLeaveRequest.DocNum = intIdt;
                objLeaveRequest.EmpName = EmpRecord.FirstName + " " + EmpRecord.MiddleName + " " + EmpRecord.LastName;
                objLeaveRequest.LeaveDescription = "Leave Deduction";
                objLeaveRequest.DocDate = DateTime.Now;
                objLeaveRequest.CreateDate = DateTime.Now;
                objLeaveRequest.CreatedBy = oCompany.UserName;
                objLeaveRequest.UnitsID = "Day";
                objLeaveRequest.UnitsLOVType = "LeaveUnits";
                objLeaveRequest.TotalCount = leaveCount;
                objLeaveRequest.LeaveType = leaveType;
                objLeaveRequest.LeaveFrom = leaveFromdate;
                objLeaveRequest.LeaveTo = leaveToDate;
                objLeaveRequest.DocType = 13;
                objLeaveRequest.Series = -1;
                objLeaveRequest.FlgPaid = false;

                dbHrPayroll.TrnsLeavesRequest.InsertOnSubmit(objLeaveRequest);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: AddNewLeaveRequest Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void AddNewOverTimeRequest(string empid, int PeriodID, int OverTimeType, decimal OTCountTotal, DateTime OTDate)
        {
            TrnsEmployeeOvertime EmpOverTime;
            try
            {
                decimal OtCount = 0;
                decimal amount = 0;
                decimal workhoursX = 0;
                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p).Single();
                if (!emp.FlgOTApplicable.Value)
                {
                    oApplication.StatusBar.SetText("Overtime not applicable to employee with empcode " + emp.EmpID, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                else
                {
                    //Insert Child Record
                    var OverTime = dbHrPayroll.MstOverTime.Where(O => O.ID == OverTimeType).FirstOrDefault();
                    if (OverTime != null)
                    {
                        var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == empid).FirstOrDefault();
                        OtCount = OTCountTotal;
                        amount = setRowAmnt(EmpRecord, OtCount, PeriodID.ToString(), OverTime.ID.ToString());
                        string strAmount = string.Format("{0:0.00}", amount);
                        if (EmpRecord != null)
                        {
                            EmpOverTime = dbHrPayroll.TrnsEmployeeOvertime.Where(o => o.EmployeeId == emp.ID && o.Period == PeriodID).FirstOrDefault();
                            if (EmpOverTime == null)
                            {
                                EmpOverTime = new TrnsEmployeeOvertime();
                                dbHrPayroll.TrnsEmployeeOvertime.InsertOnSubmit(EmpOverTime);
                            }
                            TrnsEmployeeOvertimeDetail EmpOverTimeDet = new TrnsEmployeeOvertimeDetail();
                            EmpOverTime.EmployeeId = emp.ID;
                            EmpOverTime.Period = PeriodID;
                            EmpOverTime.CreateDate = DateTime.Now;
                            EmpOverTime.UserId = oCompany.UserName;


                            EmpOverTimeDet.OvertimeID = OverTime.ID;
                            EmpOverTimeDet.ValueType = OverTime.ValueType;
                            EmpOverTimeDet.OTValue = OverTime.Value;
                            EmpOverTimeDet.OTDate = OTDate;
                            EmpOverTimeDet.FromTime = "";
                            EmpOverTimeDet.ToTime = "";
                            EmpOverTimeDet.OTHours = Convert.ToDecimal(OtCount);
                            EmpOverTimeDet.Amount = Convert.ToDecimal(strAmount);
                            EmpOverTimeDet.BasicSalary = EmpRecord.BasicSalary;
                            EmpOverTimeDet.FlgActive = true;
                            EmpOverTimeDet.CreateDate = DateTime.Now;
                            EmpOverTimeDet.UserId = oCompany.UserName;
                            EmpOverTime.TrnsEmployeeOvertimeDetail.Add(EmpOverTimeDet);
                            dbHrPayroll.SubmitChanges();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: AddNewOverTimeRequest Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void AddAdjustmentRequest(string empid, int PeriodID, int payrollId, decimal daysCount, decimal HoursCount, int leaveType, int OvertimeType)
        {
            AttSummary eleAttAdj;
            try
            {
                int cntOne = dbHrPayroll.AttSummary.Where(d => d.PeriodId == Convert.ToInt32(cbPeriod.Value.Trim()) && d.PayrollId == payrollId).Count();
                if (cntOne > 0)
                {
                    eleAttAdj = dbHrPayroll.AttSummary.Where(d => d.PeriodId == Convert.ToInt32(cbPeriod.Value.Trim()) && d.PayrollId == payrollId).FirstOrDefault();
                    eleAttAdj.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.Trim() select p).Single();
                    eleAttAdj.PayrollId = Convert.ToInt32(payrollId);
                    //eleAttAdj.LeaveId = leaveType;
                    //eleAttAdj.OvertimeId = OvertimeType;
                    eleAttAdj.UpdateDt = DateTime.Now;
                    eleAttAdj.UpdateBy = oCompany.UserName;
                }
                else
                {
                    eleAttAdj = new AttSummary();
                    eleAttAdj.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.Trim() select p).Single();
                    eleAttAdj.CreateDt = DateTime.Now;
                    eleAttAdj.CreateBy = oCompany.UserName;
                    //eleAttAdj.LeaveId = leaveType;
                    //eleAttAdj.OvertimeId = OvertimeType;
                    eleAttAdj.PayrollId = Convert.ToInt32(payrollId);
                    dbHrPayroll.AttSummary.InsertOnSubmit(eleAttAdj);
                }
                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p).FirstOrDefault();
                AttSummaryDetail attSummaryDetail = null;
                //attSummaryDetail = (from p in dbHrPayroll.AttSummaryDetail where p.AttSumDetailId.ToString() == detailId.ToString() select p).Single();
                if (eleAttAdj != null && emp != null)
                {
                    attSummaryDetail = dbHrPayroll.AttSummaryDetail.Where(p => p.AttSumDetailId.ToString() == eleAttAdj.AttSummaryID.ToString() && p.EmpId == emp.ID).FirstOrDefault();
                    if (attSummaryDetail == null)
                    {
                        attSummaryDetail = new AttSummaryDetail();
                        eleAttAdj.AttSummaryDetail.Add(attSummaryDetail);
                    }
                }
                else
                {
                    attSummaryDetail = new AttSummaryDetail();
                    eleAttAdj.AttSummaryDetail.Add(attSummaryDetail);
                }
                if (emp != null)
                {
                    attSummaryDetail.EmpId = emp.ID;
                    attSummaryDetail.AdjDays = Convert.ToDecimal(daysCount);
                    attSummaryDetail.AdjHrs = Convert.ToDecimal(HoursCount);
                    attSummaryDetail.HrsRate = 0;
                    attSummaryDetail.FlgActive = true;
                    attSummaryDetail.Remarks = "Posted From Attendance";
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_NAttPost Function: Add Days Hour Adjustment Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
