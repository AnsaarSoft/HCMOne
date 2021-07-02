using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;
using System.Collections.Generic;

namespace ACHR.Screen
{
    class frm_MstShift : HRMSBaseForm
    {

        #region "Global Variable Area"

        SAPbouiCOM.Button btnSave, btCancel, btId;
        SAPbouiCOM.Item IbtId, Icb_Overtime, IcmbDeductionRule, IcmbOffDayOverTime, IcmbHoliDayOverTime;
        SAPbouiCOM.CheckBox chkIsActive, flgOT, chkOtWrk, chkOffDayOverTime,chkHoliDayOverTime, chkWorkHrs;
        SAPbouiCOM.EditText txtShiftCode, txtDescription;
        SAPbouiCOM.ComboBox cb_Overtime, cmbDeductionRule, cmbOffDayOverTime,cmbHoliDayOverTime;
        SAPbouiCOM.DataTable dtShiftDetail;
        SAPbouiCOM.Matrix grdShiftDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, Day, StartTime, EndTime,BreakTime, Duration, InOverlap, OutOverlap, ebuff,sbuff, ExpectedIn, ExpectedOut;
        private Int32 CurrentRecord = 0, TotalRecords = 0;
        IEnumerable<MstShifts> oDocuments = null;

        private bool Validate;

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
                
                FillShiftDeatilGrid();
                FillComboDeductionRule();
                //FillOffDayOvertimeType();
                FillOvertimeType();
                FillOffDayOvertimeType();
                FillHoliDayOvertimeType();
                
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_MstShift Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (pVal.ColUID == "STime" || pVal.ColUID == "ETime")
                {
                    string[] StartDate = (grdShiftDetail.Columns.Item("STime").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
                    string[] EndDate = (grdShiftDetail.Columns.Item("ETime").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
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
                        (grdShiftDetail.Columns.Item("dura").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                    }
                }
                if(pVal.ColUID=="")
                {

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
                    case "STime":
                    case "ETime":
                        {
                            string Value = (grdShiftDetail.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
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

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":                       
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
                        {
                            //GetShiftByFilterExpresion();
                            //doFind();
                        }
                        else
                        {
                            ValidateAndSave();
                        }
                        break;
                    case "2":

                        break;
                    case "flgOT":
                        UpdateOTTypeWithStatus();
                        break;
                    case "btId":
                        picShiftCode();
                        break;
                    case "chOffDayOT":
                        UpdateOTTypeWithStatus();
                        break;
                    case "chHoliDyOT":
                        UpdateOTTypeWithStatus();

                        break;
                    default:
                        break;
            
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_MstShift Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);

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

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            ClearFields2();
            picShiftCode();
            IbtId.Enabled = true;
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {                
                btnSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("2").Specific;
                btId = oForm.Items.Item("btId").Specific;
                IbtId = oForm.Items.Item("btId");

                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("txtSCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtShiftCode = oForm.Items.Item("txtSCode").Specific;
                txtShiftCode.DataBind.SetBound(true, "", "txtSCode");

                oForm.DataSources.UserDataSources.Add("txtSDec", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtDescription = oForm.Items.Item("txtSDec").Specific;
                txtDescription.DataBind.SetBound(true, "", "txtSDec");

                cb_Overtime = oForm.Items.Item("cbOT").Specific;
                oForm.DataSources.UserDataSources.Add("cbOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cb_Overtime.DataBind.SetBound(true, "", "cbOT");
                Icb_Overtime = oForm.Items.Item("cbOT");

                chkIsActive = oForm.Items.Item("chkAct").Specific;
                oForm.DataSources.UserDataSources.Add("chkAct", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkIsActive.DataBind.SetBound(true, "", "chkAct");
                chkIsActive.Checked = true;

                flgOT = oForm.Items.Item("flgOT").Specific;
                oForm.DataSources.UserDataSources.Add("flgOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                flgOT.DataBind.SetBound(true, "", "flgOT");
                flgOT.Checked = true;

                chkOtWrk = oForm.Items.Item("chkOtWrk").Specific;
                oForm.DataSources.UserDataSources.Add("chkOtWrk", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkOtWrk.DataBind.SetBound(true, "", "chkOtWrk");
                chkOtWrk.Checked = false;

                

                cmbDeductionRule = oForm.Items.Item("cbdedrule").Specific;
                IcmbDeductionRule = oForm.Items.Item("cbdedrule");
                oForm.DataSources.UserDataSources.Add("cbdedrule", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbDeductionRule.DataBind.SetBound(true, "", "cbdedrule");


                cmbOffDayOverTime = oForm.Items.Item("cbOffDayOT").Specific;
                IcmbOffDayOverTime = oForm.Items.Item("cbOffDayOT");
                oForm.DataSources.UserDataSources.Add("cbOffDayOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbOffDayOverTime.DataBind.SetBound(true, "", "cbOffDayOT");

                chkOffDayOverTime = oForm.Items.Item("chOffDayOT").Specific;
                oForm.DataSources.UserDataSources.Add("chOffDayOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkOffDayOverTime.DataBind.SetBound(true, "", "chOffDayOT");
                chkOffDayOverTime.Checked = false;
                //cbHoliDyOT
                
                cmbHoliDayOverTime = oForm.Items.Item("cbHoliDyOT").Specific;
                IcmbHoliDayOverTime = oForm.Items.Item("cbHoliDyOT");
                oForm.DataSources.UserDataSources.Add("cbHoliDyOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbHoliDayOverTime.DataBind.SetBound(true, "", "cbHoliDyOT");

                chkHoliDayOverTime = oForm.Items.Item("chHoliDyOT").Specific;
                oForm.DataSources.UserDataSources.Add("chHoliDyOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkHoliDayOverTime.DataBind.SetBound(true, "", "chHoliDyOT");
                chkHoliDayOverTime.Checked = false;

                chkWorkHrs = oForm.Items.Item("chkWorkHrs").Specific;
                oForm.DataSources.UserDataSources.Add("chkWorkHrs", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkWorkHrs.DataBind.SetBound(true, "", "chkWorkHrs");
                chkWorkHrs.Checked = false;

                //chOffDayOT
                //cbOffDayOT
                InitiallizegridMatrix();
                AddNewRecord();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadToNewRecord()
        {
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            ClearFields();
            FillShiftDeatilGrid();
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtShiftDetail = oForm.DataSources.DataTables.Add("ShiftDetails");
                dtShiftDetail.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtShiftDetail.Columns.Add("Day", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("STime", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("ETime", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("BKTime", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("Duration", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("InOverlap", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("OutOverlap", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("ExpectIn", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("ExpectOut", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("sbuff", SAPbouiCOM.BoFieldsType.ft_Text);
                dtShiftDetail.Columns.Add("ebuff", SAPbouiCOM.BoFieldsType.ft_Text);

                grdShiftDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Shif").Specific;
                oColumns = (SAPbouiCOM.Columns)grdShiftDetail.Columns;


                oColumn = oColumns.Item("No");
                clNo = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "No");

                oColumn = oColumns.Item("clDay");
                Day = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "Day");

                oColumn = oColumns.Item("STime");
                StartTime = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "STime");

                oColumn = oColumns.Item("ETime");
                EndTime = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "ETime");

                oColumn = oColumns.Item("BKTime");
                BreakTime = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "BKTime");

                oColumn = oColumns.Item("InOvp");
                InOverlap = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "InOverlap");

                oColumn = oColumns.Item("Outovp");
                OutOverlap = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "OutOverlap");


                oColumn = oColumns.Item("dura");
                Duration = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "Duration");

                oColumn = oColumns.Item("ebuff");
                ebuff = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "ebuff");

                oColumn = oColumns.Item("sbuff");
                sbuff = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "sbuff");

                oColumn = oColumns.Item("expin");
                ExpectedIn = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "ExpectIn");

                oColumn = oColumns.Item("expout");
                ExpectedOut = oColumn;
                oColumn.DataBind.Bind("ShiftDetails", "ExpectOut");
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillOvertimeType()
        {
            try
            {
                var Data = dbHrPayroll.MstOverTime.Where(o => o.FlgActive == true).ToList();
                cb_Overtime.ValidValues.Add("-1", "[select one]");
                if (Data != null && Data.Count > 0)
                {
                    foreach (MstOverTime Overtime in Data)
                    {
                        cb_Overtime.ValidValues.Add(Convert.ToString(Overtime.ID), Convert.ToString(Overtime.Description));
                    }  
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillOffDayOvertimeType()
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.MstOverTime where a.FlgActive == true select a).ToList();
                cmbOffDayOverTime.ValidValues.Add("-1", "Select One");
                foreach (var one in ocollection)
                {
                    cmbOffDayOverTime.ValidValues.Add(Convert.ToString(one.ID), Convert.ToString(one.Description));
                }
                cmbOffDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillHoliDayOvertimeType()
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.MstOverTime where a.FlgActive == true select a).ToList();
                cmbHoliDayOverTime.ValidValues.Add("-1", "Select One");
                foreach (var one in ocollection)
                {
                    cmbHoliDayOverTime.ValidValues.Add(Convert.ToString(one.ID), Convert.ToString(one.Description));
                }
                cmbHoliDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboDeductionRule()
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.TrnsDeductionRules select a).ToList();
                cmbDeductionRule.ValidValues.Add("-1", "Select One");
                foreach (var one in ocollection)
                {
                    cmbDeductionRule.ValidValues.Add(Convert.ToString(one.ID), Convert.ToString(one.Code));
                }
                cmbDeductionRule.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillShiftDeatilGrid()
        {
            Int16 i = 0;
            try
            {
                var WeekDays = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.ToList();
                if (WeekDays != null && WeekDays.Count > 0)
                {
                    dtShiftDetail.Rows.Clear();
                    dtShiftDetail.Rows.Add(WeekDays.Count());
                    foreach (var Day in WeekDays)
                    {
                        dtShiftDetail.SetValue("No", i, i + 1);
                        dtShiftDetail.SetValue("Day", i, Day);                        
                        i++;
                    }
                    grdShiftDetail.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillOvertimeType Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ValidateAndSave()
        {            
            string TimeIn = "";
            string Day="";
            string TimeOut = "";
            string Duration = "";
            string startBuffer = "";
            string EndBuffer = "";
            string strBreakTime = "";
            bool flgInOverlap = false;
            bool flgOutOverlap = false;
            bool flgExpectedIn = false;
            bool flgExpectedOut = false;
            MstShifts ObjShift;
            MstShiftDetails objChild;
            try
            {
                if (string.IsNullOrEmpty(txtShiftCode.Value))
                {
                    oApplication.StatusBar.SetText("Please Enter Valid Shift Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);                      
                    return;
                }
                else if (string.IsNullOrEmpty(txtDescription.Value))
                {
                    oApplication.StatusBar.SetText("Please Enter Valid Shift Description", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);                     
                    return;
                }
                else
                {
                    ObjShift = (from p in dbHrPayroll.MstShifts where p.Code == txtShiftCode.Value select p).FirstOrDefault();
                    if (ObjShift == null)
                    {
                        ObjShift = new MstShifts();
                        dbHrPayroll.MstShifts.InsertOnSubmit(ObjShift);
                        ObjShift.Code = txtShiftCode.Value;
                    }
                    ObjShift.Description = txtDescription.Value;
                    ObjShift.CreateDate = DateTime.Now;
                    ObjShift.UserId = oCompany.UserName;
                    ObjShift.UpdateDate = DateTime.Now;
                    ObjShift.UpdatedBy = oCompany.UserName;

                    if (flgOT.Checked)
                    {
                        ObjShift.OverTime = true;
                        if (!string.IsNullOrEmpty(cb_Overtime.Value) && Convert.ToInt32(cb_Overtime.Value) > 0)
                        {
                            //Wrong way of assigning objects
                            //ObjShift.OverTimeID = Convert.ToInt32(cb_Overtime.Value);
                            MstOverTime oOT = (from a in dbHrPayroll.MstOverTime where a.ID.ToString() == cb_Overtime.Value.Trim() select a).FirstOrDefault();
                            if (oOT != null)
                            {
                                //ObjShift.OverTimeID = oOT.ID;
                                ObjShift.MstOverTime = oOT;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Please Select Valid OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    else
                    {
                        ObjShift.OverTime = false;
                    }
                    if (chkOffDayOverTime.Checked)
                    {
                        ObjShift.FlgOffDayOverTime = true;
                        if (!string.IsNullOrEmpty(cmbOffDayOverTime.Value) && Convert.ToInt32(cmbOffDayOverTime.Value) > 0)
                        {  
                            MstOverTime oOffDayOT = (from a in dbHrPayroll.MstOverTime where a.ID.ToString() == cmbOffDayOverTime.Value.Trim() select a).FirstOrDefault();
                            if (oOffDayOT != null)
                            {
                                ObjShift.OffDayOverTimeMstOverTime = oOffDayOT;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Please Select Valid Off Day OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    else
                    {
                        ObjShift.FlgOffDayOverTime = false;
                    }

                    if (chkHoliDayOverTime.Checked)
                    {
                        ObjShift.FlgHoliDayOverTime = true;
                        if (!string.IsNullOrEmpty(cmbHoliDayOverTime.Value) && Convert.ToInt32(cmbHoliDayOverTime.Value) > 0)
                        {
                            MstOverTime oOffDayOT = (from a in dbHrPayroll.MstOverTime where a.ID.ToString() == cmbHoliDayOverTime.Value.Trim() select a).FirstOrDefault();
                            if (oOffDayOT != null)
                            {
                                ObjShift.HoliDayOverTimeMstOverTime = oOffDayOT;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Please Select Valid Holi Day OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    else
                    {
                        ObjShift.FlgHoliDayOverTime = false;
                    }
                    int dedrulecheck = cmbDeductionRule.Value.Trim() == "-1" ? 0 : Convert.ToInt32(cmbDeductionRule.Value.Trim());
                    if (dedrulecheck == 0)
                    {
                        ObjShift.DeductionRuleID = null;
                    }
                    else
                    {
                        ObjShift.DeductionRuleID = dedrulecheck;
                    }
                    if (chkIsActive.Checked)
                    {
                        ObjShift.StatusShift = true;
                    }
                    else
                    {
                        ObjShift.StatusShift = false;
                    }
                    if (chkOtWrk.Checked)
                    {
                        ObjShift.FlgOTWrkHrs = true;
                    }
                    else
                    {
                        ObjShift.FlgOTWrkHrs = false;
                    }
                    if (chkWorkHrs.Checked)
                    {
                        ObjShift.FlgWorkingHoursOnMultipTimeInTimeOut = true;
                    }
                    else
                    {
                        ObjShift.FlgWorkingHoursOnMultipTimeInTimeOut= false;
                    }
                    if (dtShiftDetail != null && dtShiftDetail.Rows.Count > 0)
                    {
                        for (int i = 1; i <= grdShiftDetail.RowCount; i++)
                        {
                            Day = (grdShiftDetail.Columns.Item("clDay").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            TimeIn = (grdShiftDetail.Columns.Item("STime").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            TimeOut = (grdShiftDetail.Columns.Item("ETime").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            strBreakTime = (grdShiftDetail.Columns.Item("BKTime").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            Duration = (grdShiftDetail.Columns.Item("dura").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            startBuffer = (grdShiftDetail.Columns.Item("sbuff").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            EndBuffer = (grdShiftDetail.Columns.Item("ebuff").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            flgInOverlap = (grdShiftDetail.Columns.Item("InOvp").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            flgOutOverlap = (grdShiftDetail.Columns.Item("Outovp").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            flgExpectedIn = (grdShiftDetail.Columns.Item("expin").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            flgExpectedOut = (grdShiftDetail.Columns.Item("expout").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            if (!string.IsNullOrEmpty(Duration))
                            {
                                objChild = dbHrPayroll.MstShiftDetails.Where(Shft => Shft.ShiftID == ObjShift.Id && Shft.Day == Day).FirstOrDefault();
                                if (objChild == null)
                                {
                                    objChild = new MstShiftDetails();
                                    ObjShift.MstShiftDetails.Add(objChild);
                                }
                                objChild.Day = Day;
                                objChild.BufferStartTime = startBuffer;
                                objChild.BufferEndTime = EndBuffer;
                                objChild.StartTime = TimeIn;
                                objChild.EndTime = TimeOut;
                                objChild.BreakTime = strBreakTime;
                                objChild.Duration = Duration;
                                objChild.FlgInOverlap = flgInOverlap;
                                objChild.FlgOutOverlap = flgOutOverlap;
                                objChild.FlgExpectedIn = flgExpectedIn;
                                objChild.FlgExpectedOut = flgExpectedOut;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Please Enter Valid Shift Duration", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        ClearFields();
                        oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    }
                }               
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);              
            }
        }

        private void GetShiftByFilterExpresion(int id)
        {
            Int16 i = 0;
            try
            {
                var Shifts = dbHrPayroll.MstShifts.Where(s => s.Id == id).FirstOrDefault();
                var ShiftWisedeductionRules = dbHrPayroll.TrnsDeductionRules.Where(d => d.ID == Shifts.DeductionRuleID).FirstOrDefault();
                if (Shifts != null)
                {

                    txtShiftCode.Value = Shifts.Code;
                    txtDescription.Value = Shifts.Description;
                    bool isOvertime = Shifts.OverTime == null ? false : Shifts.OverTime.Value;
                    
                    flgOT.Checked = isOvertime;
                    if (Shifts.OverTime != null && isOvertime)
                    {
                        cb_Overtime.Select(Shifts.MstOverTime.Description);
                    }
                    else
                    {
                        cb_Overtime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    }

                    bool isOffdayOvertime = Shifts.FlgOffDayOverTime == null ? false : Shifts.FlgOffDayOverTime.Value;
                    if (Shifts.OffDayOverTime != null && isOffdayOvertime)
                    {
                        cmbOffDayOverTime.Select(Shifts.OffDayOverTimeMstOverTime.Description);
                    }
                    else
                    {
                        cmbOffDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    
                    }
                    if (isOffdayOvertime)
                    {
                        chkOffDayOverTime.Checked = true;
                    }
                    else
                    {
                        chkOffDayOverTime.Checked = false;
                    }
                    bool isHolidayOvertime = Shifts.FlgHoliDayOverTime.GetValueOrDefault();
                    if (Shifts.HoliDayOverTime != null && isHolidayOvertime)
                    {
                        cmbHoliDayOverTime.Select(Shifts.HoliDayOverTimeMstOverTime.Description);
                    }
                    else
                    {
                        cmbHoliDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    }
                    if (isHolidayOvertime)
                    {
                        chkHoliDayOverTime.Checked = true;
                    }
                    else
                    {
                        chkHoliDayOverTime.Checked = false;
                    }
                    //if (!string.IsNullOrEmpty(cmbDeductionRule.Value.Trim()) && cmbDeductionRule.Value.Trim()!="-1")
                    if (Shifts.DeductionRuleID != null)
                    {
                        cmbDeductionRule.Select(ShiftWisedeductionRules.Code);
                    }
                    else
                    {
                        cmbDeductionRule.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    }
                    
                    bool shiftStatus = Shifts.StatusShift == null ? false : Shifts.StatusShift.Value;
                    if (shiftStatus)
                    {
                        chkIsActive.Checked = true;
                    }
                    else
                    {
                        chkIsActive.Checked = false;
                    }
                    bool isOtonWrkHrs = Shifts.FlgOTWrkHrs == null ? false : Shifts.FlgOTWrkHrs.Value;
                    bool isWrkHrs = Shifts.FlgWorkingHoursOnMultipTimeInTimeOut == null ? false : Shifts.FlgWorkingHoursOnMultipTimeInTimeOut.Value;
                    chkOtWrk.Checked = isOtonWrkHrs;
                    chkWorkHrs.Checked = isWrkHrs;
                    var shiftDetail = Shifts.MstShiftDetails.ToList();
                    if (shiftDetail != null && shiftDetail.Count > 0)
                    {
                        dtShiftDetail.Rows.Clear();
                        dtShiftDetail.Rows.Add(shiftDetail.Count);
                        foreach (var EMP in shiftDetail)
                        {
                            dtShiftDetail.SetValue("No", i, i + 1);
                            dtShiftDetail.SetValue("Day", i, EMP.Day);
                            dtShiftDetail.SetValue("STime", i, string.IsNullOrEmpty(EMP.StartTime) ? "" : EMP.StartTime);
                            dtShiftDetail.SetValue("ETime", i, string.IsNullOrEmpty(EMP.EndTime) ? "" : EMP.EndTime);
                            dtShiftDetail.SetValue("BKTime", i, string.IsNullOrEmpty(EMP.BreakTime) ? "" : EMP.BreakTime);
                            dtShiftDetail.SetValue("Duration", i, string.IsNullOrEmpty(EMP.Duration) ? "" : EMP.Duration);
                            dtShiftDetail.SetValue("sbuff", i, string.IsNullOrEmpty(EMP.BufferStartTime) ? "" : EMP.BufferStartTime);
                            dtShiftDetail.SetValue("ebuff", i, string.IsNullOrEmpty(EMP.BufferEndTime) ? "" : EMP.BufferEndTime);
                            dtShiftDetail.SetValue("InOverlap", i, EMP.FlgInOverlap == true ? "Y" : "N");
                            dtShiftDetail.SetValue("OutOverlap", i, EMP.FlgOutOverlap == true ? "Y" : "N");
                            i++;
                        }
                        grdShiftDetail.LoadFromDataSource();
                    }
                   
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetNextRecord()
        {
            try
            {
                var ShiftsRecords = dbHrPayroll.MstShifts.ToList();
                if (ShiftsRecords != null && ShiftsRecords.Count > 0)
                {
                    TotalRecords = ShiftsRecords.Count;
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
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }   


        }

        private void GetPreviosRecord()
        {
            try
            {
                var ShiftsRecords = dbHrPayroll.MstShifts.ToList();
                if (ShiftsRecords != null && ShiftsRecords.Count > 0)
                {
                    TotalRecords = ShiftsRecords.Count;
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
            catch (Exception Ex)
            {
                 Console.WriteLine("{0}", Ex.Message);
            }    
        }

        private void FillDocument(Int32 DocumentID)
        {
            Int16 i = 0;
            try
            {
                            
                oDocuments = dbHrPayroll.MstShifts.ToList();
                MstShifts oDoc = oDocuments.ElementAt<MstShifts>(DocumentID);
                var ShiftWisedeductionRules = dbHrPayroll.TrnsDeductionRules.Where(d => d.ID == oDoc.DeductionRuleID).FirstOrDefault();
                txtShiftCode.Value = oDoc.Code;
                txtDescription.Value = oDoc.Description;
                bool isOvertime = oDoc.OverTime == null ? false : oDoc.OverTime.Value;
                flgOT.Checked = isOvertime;
                if (isOvertime)
                {
                    cb_Overtime.Select(oDoc.MstOverTime.Description);
                }
                else
                {
                    cb_Overtime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                bool shiftStatus = oDoc.StatusShift == null ? false : oDoc.StatusShift.Value;
                chkIsActive.Checked = shiftStatus;
                bool isOtonWrkHrs = oDoc.FlgOTWrkHrs == null ? false : oDoc.FlgOTWrkHrs.Value;
                bool isWorkHrs = oDoc.FlgWorkingHoursOnMultipTimeInTimeOut.GetValueOrDefault();
                chkOtWrk.Checked = isOtonWrkHrs;
                chkWorkHrs.Checked = isWorkHrs;
                if (oDoc.DeductionRuleID != null)
                {
                    cmbDeductionRule.Select(ShiftWisedeductionRules.Code);
                }
                else
                {
                    cmbDeductionRule.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                bool isOffdayOvertime = oDoc.FlgOffDayOverTime == null ? false : oDoc.FlgOffDayOverTime.Value;
                if (oDoc.OffDayOverTime != null && isOffdayOvertime)
                {
                    cmbOffDayOverTime.Select(oDoc.OffDayOverTimeMstOverTime.Description);
                }
                else
                {
                    //cmbOffDayOverTime.ValidValues.Add("","");
                    cmbOffDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    #region Remove Items From Combobox
                    //if (cmbOffDayOverTime.ValidValues.Count > 0)
                    //{
                    //    int vcnt = cmbOffDayOverTime.ValidValues.Count;
                    //    for (int k = vcnt - 1; k >= 0; k--)
                    //    {
                    //        cmbOffDayOverTime.ValidValues.Remove(cmbOffDayOverTime.ValidValues.Item(k).Value);
                    //    }
                    //}
                    #endregion

                }
                if (isOffdayOvertime)
                {
                    chkOffDayOverTime.Checked = true;
                }
                else
                {
                    chkOffDayOverTime.Checked = false;
                }

                bool isHolidayOvertime = oDoc.FlgHoliDayOverTime.GetValueOrDefault();
                if (oDoc.HoliDayOverTime != null && isHolidayOvertime)
                {
                    cmbHoliDayOverTime.Select(oDoc.HoliDayOverTimeMstOverTime.Description);
                }
                else
                {
                   
                    cmbHoliDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                   

                }
                if (isHolidayOvertime)
                {
                    chkHoliDayOverTime.Checked = true;
                }
                else
                {
                    chkHoliDayOverTime.Checked = false;
                }

                //string Overtime = Convert.ToString(oDoc.OverTime);
                //if (!string.IsNullOrEmpty(Overtime) && oDoc.OverTime == true)
                //{
                //    cb_Overtime.Select(oDoc.MstOverTime.Description);
                //    flgOT.Checked = true;
                //}
                //else
                //{
                //    cb_Overtime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //    flgOT.Checked = false;
                //}
                //if (oDoc.StatusShift.Value)
                //{
                //    chkIsActive.Checked = true;                   
                //}
                //else
                //{
                //    chkIsActive.Checked = false;
                //}
                var shiftDetail = oDoc.MstShiftDetails.ToList();
                if (shiftDetail != null && shiftDetail.Count > 0)
                {
                    dtShiftDetail.Rows.Clear();
                    dtShiftDetail.Rows.Add(shiftDetail.Count);
                    foreach (var EMP in shiftDetail)
                    {
                        dtShiftDetail.SetValue("No", i, i + 1);
                        dtShiftDetail.SetValue("Day", i, EMP.Day);
                        dtShiftDetail.SetValue("STime", i, string.IsNullOrEmpty(EMP.StartTime) ? "" : EMP.StartTime);
                        dtShiftDetail.SetValue("ETime", i, string.IsNullOrEmpty(EMP.EndTime) ? "" : EMP.EndTime);
                        dtShiftDetail.SetValue("BKTime", i, string.IsNullOrEmpty(EMP.BreakTime) ? "" : EMP.BreakTime);
                        dtShiftDetail.SetValue("Duration", i, string.IsNullOrEmpty(EMP.Duration) ? "" : EMP.Duration);
                        dtShiftDetail.SetValue("sbuff", i, string.IsNullOrEmpty(EMP.BufferStartTime) ? "" : EMP.BufferStartTime);
                        dtShiftDetail.SetValue("ebuff", i, string.IsNullOrEmpty(EMP.BufferEndTime) ? "" : EMP.BufferEndTime);
                        dtShiftDetail.SetValue("InOverlap", i, EMP.FlgInOverlap == true ? "Y" : "N");
                        dtShiftDetail.SetValue("OutOverlap", i, EMP.FlgOutOverlap == true ? "Y" : "N");
                        dtShiftDetail.SetValue("ExpectIn", i, EMP.FlgExpectedIn == true ? "Y" : "N");
                        dtShiftDetail.SetValue("ExpectOut", i, EMP.FlgExpectedOut == true ? "Y" : "N");
                        i++;
                    }
                    grdShiftDetail.LoadFromDataSource();
                    grdShiftDetail.AutoResizeColumns();
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;   
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        
        private void ClearFields()
        {
            try
            {
                txtShiftCode.Value = string.Empty;
                txtDescription.Value = string.Empty;
                cb_Overtime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbDeductionRule.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbOffDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbHoliDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                chkOffDayOverTime.Checked = false;
                chkIsActive.Checked = false;
                flgOT.Checked = false;
                chkOtWrk.Checked = false;
                chkHoliDayOverTime.Checked = false;
                dtShiftDetail.Rows.Clear();
                FillShiftDeatilGrid();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void UpdateOTTypeWithStatus()
        {
            try
            {
                if (!flgOT.Checked)
                {
                    cb_Overtime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                if (!chkOffDayOverTime.Checked)
                {
                    cmbOffDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                if (!chkHoliDayOverTime.Checked)
                {
                    cmbHoliDayOverTime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void doFind()
        {
            try
            {
                PrepareSearchKeyHash();
                string strSql = sqlString.getSql("MstShifts", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Shifts", "Select  Shifts");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    currentObjId = st.Rows[0][0].ToString();
                    //getRecord(currentObjId); 
                    int docID = Convert.ToInt32(currentObjId);
                    //FillDocument(docID);]
                    GetShiftByFilterExpresion(docID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            try
            {
                SearchKeyVal.Clear();
                if (txtShiftCode.Value.Trim() != "")
                {
                    SearchKeyVal.Add("Code", txtShiftCode.Value);
                }
                if (txtDescription.Value.Trim() != "")
                {
                    SearchKeyVal.Add("Description", txtDescription.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void picShiftCode()
        {
            string strSql = sqlString.getSql("MstShiftsMaster", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Shifts", "Select  Shifts");
            pic = null;
            if (st.Rows.Count > 0)
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                currentObjId = st.Rows[0][0].ToString();
                int docID = Convert.ToInt32(currentObjId);
                GetShiftByFilterExpresion(docID);
                //txtShiftCode.Value = st.Rows[0][1].ToString();
            }
        }

        private void ClearFields2()
        {
            try
            {
                txtShiftCode.Value = string.Empty;               
                txtDescription.Value = string.Empty;
                cb_Overtime.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //chkIsActive.Checked = false;
                //flgOT.Checked = false;
                dtShiftDetail.Rows.Clear();
                grdShiftDetail.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        #endregion

    }
}
