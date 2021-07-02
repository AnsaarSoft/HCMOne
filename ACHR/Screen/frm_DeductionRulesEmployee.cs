using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_DeductionRulesEmployee : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btnCancel, btnSave;
        SAPbouiCOM.DataTable dtRules;
        SAPbouiCOM.Matrix grdRules;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clCode, clValue, clRangeFrom, clRangeTo, clflgDeduct, clLeaveType, clGracePeriod, clLeaveCount;
        SAPbouiCOM.EditText txtDocNum, txtDeductionCode;
        SAPbouiCOM.Item ItxtDocNum, ItxtDeductionCode;
        private bool Validate;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1288", true);  // Next Record
                oForm.EnableMenu("1289", true);  // Pevious Record
                oForm.EnableMenu("1290", true);  // First Record
                oForm.EnableMenu("1291", true);  // Last record 
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_DeductionRules Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                        SaveRecords();
                        break;
                    case "2":

                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_DeductionRules Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    case "clRngFrm":
                    case "clRngTo":
                        {
                            string Value = (grdRules.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
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
        
        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ColUID == "clRngTo")
            {
                string RangeFrom = (grdRules.Columns.Item("clRngFrm").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                string RangeTo = (grdRules.Columns.Item("clRngTo").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                if (!string.IsNullOrEmpty(RangeFrom) && (!string.IsNullOrEmpty(RangeTo)))
                {
                    double checkRangeFrom = TimeSpan.Parse(RangeFrom).TotalHours;
                    double checkRangeTo = TimeSpan.Parse(RangeTo).TotalHours;

                    if (checkRangeFrom > checkRangeTo)
                    {
                        oApplication.StatusBar.SetText("Range from is greater then range to", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                }

            }

        }
        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();            
            
        }

        public override void fillFields()
        {
            base.fillFields();
            oForm.Freeze(true);
            InitiallizeDocument();
            FillRecord();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            oForm.Freeze(false);
        }
        public override void AddNewRecord()
        {
            base.AddNewRecord();
            InitiallizeDocument();
        }
        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                oForm.DataSources.UserDataSources.Add("txtDocNo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txtDocNum = oForm.Items.Item("txtDocNo").Specific;
                ItxtDocNum = oForm.Items.Item("txtDocNo");
                txtDocNum.DataBind.SetBound(true, "", "txtDocNo");

                oForm.DataSources.UserDataSources.Add("txtCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txtDeductionCode = oForm.Items.Item("txtCode").Specific;
                ItxtDeductionCode = oForm.Items.Item("txtCode");
                txtDeductionCode.DataBind.SetBound(true, "", "txtCode");

                btnSave = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                InitiallizegridMatrix();
                grdRules.AutoResizeColumns();
                FillLeaveTypeInCombo();
                GetData();
                InitiallizeDocument();
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
                dtRules = oForm.DataSources.DataTables.Add("AttRules");
                dtRules.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtRules.Columns.Add("RuleCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("Value", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("RangeFrom", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("RangeTo", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("flgDeduct", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("LeaveType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("GracePeriod", SAPbouiCOM.BoFieldsType.ft_Text);
                dtRules.Columns.Add("LeaveCount", SAPbouiCOM.BoFieldsType.ft_Text);

                grdRules = (SAPbouiCOM.Matrix)oForm.Items.Item("grdRule").Specific;
                oColumns = (SAPbouiCOM.Columns)grdRules.Columns;


                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("AttRules", "No");

                oColumn = oColumns.Item("clCode");
                clCode = oColumn;
                oColumn.DataBind.Bind("AttRules", "RuleCode");

                oColumn = oColumns.Item("clValue");
                clValue = oColumn;
                oColumn.DataBind.Bind("AttRules", "Value");

                oColumn = oColumns.Item("clRngFrm");
                clRangeFrom = oColumn;
                oColumn.DataBind.Bind("AttRules", "RangeFrom");

                oColumn = oColumns.Item("clRngTo");
                clRangeTo = oColumn;
                oColumn.DataBind.Bind("AttRules", "RangeTo");

                oColumn = oColumns.Item("chkded");
                clflgDeduct = oColumn;
                oColumn.DataBind.Bind("AttRules", "flgDeduct");

                oColumn = oColumns.Item("cbLType");
                clLeaveType = oColumn;
                oColumn.DataBind.Bind("AttRules", "LeaveType");

                oColumn = oColumns.Item("clGP");
                clGracePeriod = oColumn;
                oColumn.DataBind.Bind("AttRules", "GracePeriod");

                oColumn = oColumns.Item("clLC");
                clLeaveCount = oColumn;
                oColumn.DataBind.Bind("AttRules", "LeaveCount");
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLeaveTypeInCombo()
        {
            try
            {
                var LeaveType = (from a in dbHrPayroll.MstLeaveType select a).ToList();
                clLeaveType.ValidValues.Add(Convert.ToString(0), Convert.ToString("NONE"));
                foreach (MstLeaveType empLeaveType in LeaveType)
                {
                    clLeaveType.ValidValues.Add(Convert.ToString(empLeaveType.Code), Convert.ToString(empLeaveType.Description));
                }
                clLeaveType.DisplayDesc = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttendanceRules Function: FillLeaveTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                txtDeductionCode.Value = "";

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                GetDocumentNo();
                var AttendanceRules = (from a in dbHrPayroll.MstAttendanceRule select a).FirstOrDefault();
                if (AttendanceRules != null && AttendanceRules.FlgTimeBaseDeductionRules == true)
                {
                    GetTimeBaseDeductionRules();
                }
                else
                {
                    GetRulesRecords();
                }
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeDocument : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetRulesRecords()
        {
            Int16 i = 0;
            try
            {
                var AttRules = dbHrPayroll.MstDeductionRules.ToList();
                if (AttRules != null && AttRules.Count > 0)
                {
                    dtRules.Rows.Clear();
                    dtRules.Rows.Add(AttRules.Count());
                    foreach (var Rule in AttRules)
                    {
                        dtRules.SetValue("No", i, i + 1);
                        dtRules.SetValue("RuleCode", i, Rule.Code);
                        dtRules.SetValue("Value", i, Rule.Value);
                        dtRules.SetValue("RangeFrom", i, Rule.RangeFrom);
                        dtRules.SetValue("RangeTo", i, Rule.RangeTo);
                        dtRules.SetValue("flgDeduct", i, Rule.Deduction == true ? "Y" : "N");
                        //if (Rule.Deduction == true)
                        if (true)
                        {
                            var LeaveTypeID = dbHrPayroll.MstLeaveType.Where(lt => lt.ID == Rule.LeaveType).FirstOrDefault();
                            if (LeaveTypeID != null)
                            {
                                dtRules.SetValue("LeaveType", i, LeaveTypeID.Code);
                            }
                            else
                            {
                                dtRules.SetValue("LeaveType", i, 0);
                                dtRules.SetValue("flgDeduct", i, "N");
                            }
                        }
                        else
                        {
                            dtRules.SetValue("LeaveType", i, 0);
                        }
                        dtRules.SetValue(clGracePeriod.DataBind.Alias, i, Rule.GracePeriod != null ? Convert.ToString(Rule.GracePeriod) : "0");
                        dtRules.SetValue(clLeaveCount.DataBind.Alias, i, Rule.LeaveCount != null ? string.Format("{0:0.00}", Convert.ToDecimal(Rule.LeaveCount)) : "0");
                        i++;
                    }
                    grdRules.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetTimeBaseDeductionRules()
        {
            Int16 i = 0;
            try
            {
                var AttRules = dbHrPayroll.MstLOVE.Where(a => a.Value == "Shift").ToList();
                if (AttRules != null && AttRules.Count > 0)
                {
                    dtRules.Rows.Clear();
                    dtRules.Rows.Add(AttRules.Count());
                    foreach (var Rule in AttRules)
                    {
                        dtRules.SetValue("No", i, i + 1);
                        dtRules.SetValue("RuleCode", i, Rule.Code);
                        dtRules.SetValue("Value", i, Rule.Value);
                        dtRules.SetValue("RangeFrom", i, "00:00");
                        dtRules.SetValue("RangeTo", i, "00:00");
                        //dtRules.SetValue("flgDeduct", i, Rule.Deduction == true ? "Y" : "N");
                        dtRules.SetValue("flgDeduct", i, "N");
                        dtRules.SetValue(clGracePeriod.DataBind.Alias, i, "0");
                        dtRules.SetValue(clLeaveCount.DataBind.Alias, i, "0");

                        //if (true)
                        //{
                        //    var LeaveTypeID = dbHrPayroll.MstLeaveType.Where(lt => lt.ID == Rule.LeaveType).FirstOrDefault();
                        //    if (LeaveTypeID != null)
                        //    {
                        //        dtRules.SetValue("LeaveType", i, LeaveTypeID.Code);
                        //    }
                        //    else
                        //    {
                        //        dtRules.SetValue("LeaveType", i, 0);
                        //        dtRules.SetValue("flgDeduct", i, "N");
                        //    }
                        //}
                        //else
                        //{
                        //    dtRules.SetValue("LeaveType", i, 0);
                        //}
                        //dtRules.SetValue(clGracePeriod.DataBind.Alias, i, Rule.GracePeriod != null ? Convert.ToString(Rule.GracePeriod) : "0");
                        //dtRules.SetValue(clLeaveCount.DataBind.Alias, i, Rule.LeaveCount != null ? string.Format("{0:0.00}", Convert.ToDecimal(Rule.LeaveCount)) : "0");
                        i++;
                    }
                    grdRules.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ValidateSaveRecords1()
        {
            try
            {
                string Code, leavetypeCode;
                int leaveCode = 0;
                if (dtRules != null && dtRules.Rows.Count > 0)
                {
                    for (int i = 1; i <= grdRules.RowCount; i++)
                    {
                        Code = (grdRules.Columns.Item("clCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        var oOld = dbHrPayroll.MstDeductionRules.Where(atr => atr.Code == Code).FirstOrDefault();
                        if (oOld != null)
                        {
                            oOld.RangeFrom = (grdRules.Columns.Item("clRngFrm").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            oOld.RangeTo = (grdRules.Columns.Item("clRngTo").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            oOld.Deduction = (grdRules.Columns.Item("chkded").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            leavetypeCode = (grdRules.Columns.Item("cbLType").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                            if (leavetypeCode != "0")
                            {
                                leaveCode = dbHrPayroll.MstLeaveType.Where(lt => lt.Code == leavetypeCode).FirstOrDefault().ID;
                            }
                            oOld.LeaveType = leaveCode;
                            oOld.GracePeriod = Convert.ToInt32((grdRules.Columns.Item("clGP").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                            oOld.LeaveCount = Convert.ToDecimal((grdRules.Columns.Item("clLC").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                        }
                    }
                    oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SaveRecords()
        {
            try
            {
                string Code, leavetypeCode;
                int leaveCode = 0, DocNum;
                DocNum = Convert.ToInt32(txtDocNum.Value.Trim());
                var oDoc = (from a in dbHrPayroll.TrnsDeductionRules
                            where a.DocNo == DocNum
                            select a).FirstOrDefault();
                if (oDoc == null)
                {
                    TrnsDeductionRules oNew = new TrnsDeductionRules();
                    dbHrPayroll.TrnsDeductionRules.InsertOnSubmit(oNew);
                    oNew.DocNo = DocNum;
                    oNew.Code = txtDeductionCode.Value.Trim();
                    oNew.CreateDate = DateTime.Now;
                    oNew.CreatedBy = oCompany.UserName;
                    for (int i = 1; i <= grdRules.RowCount; i++)
                    {
                        string code, value, fromtime, totime, leavetype, gracepoint, leavecount;
                        bool isdeduction;
                        code = (grdRules.Columns.Item("clCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        value = (grdRules.Columns.Item("clValue").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        fromtime = (grdRules.Columns.Item("clRngFrm").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        totime = (grdRules.Columns.Item("clRngTo").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        isdeduction = (grdRules.Columns.Item("chkded").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        leavetype = (grdRules.Columns.Item("cbLType").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value; ;
                        gracepoint = (grdRules.Columns.Item("clGP").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        leavecount = (grdRules.Columns.Item("clLC").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;

                        TrnsDeductionRulesDetail oDetail = new TrnsDeductionRulesDetail();
                        oNew.TrnsDeductionRulesDetail.Add(oDetail);
                        oDetail.Code = code;
                        oDetail.Value = value;
                        oDetail.RangeFrom = fromtime;
                        oDetail.RangeTo = totime;
                        oDetail.Deduction = isdeduction;
                        oDetail.LeaveCount = Convert.ToDecimal(leavecount);
                        oDetail.GracePeriod = Convert.ToInt32(gracepoint);
                        var oLeaveType = (from a in dbHrPayroll.MstLeaveType
                                          where a.Code == leavetype
                                          select a).FirstOrDefault();
                        if (oLeaveType != null)
                        {
                            oDetail.LeaveType = oLeaveType.ID;
                        }
                    }
                }
                else
                {
                    oDoc.UpdateDate = DateTime.Now;
                    oDoc.UpdatedBy = oCompany.UserName;
                    for (int i = 1; i <= grdRules.RowCount; i++)
                    {
                        string code, value, fromtime, totime, leavetype, gracepoint, leavecount;
                        bool isdeduction;
                        code = (grdRules.Columns.Item("clCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        value = (grdRules.Columns.Item("clValue").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        fromtime = (grdRules.Columns.Item("clRngFrm").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        totime = (grdRules.Columns.Item("clRngTo").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        isdeduction = (grdRules.Columns.Item("chkded").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        leavetype = (grdRules.Columns.Item("cbLType").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value; ;
                        gracepoint = (grdRules.Columns.Item("clGP").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        leavecount = (grdRules.Columns.Item("clLC").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;

                        TrnsDeductionRulesDetail oDetail = oDoc.TrnsDeductionRulesDetail[i - 1];
                        oDetail.RangeFrom = fromtime;
                        oDetail.RangeTo = totime;
                        oDetail.Deduction = isdeduction;
                        oDetail.LeaveCount = Convert.ToDecimal(leavecount);
                        oDetail.GracePeriod = Convert.ToInt32(gracepoint);
                        var oLeaveType = (from a in dbHrPayroll.MstLeaveType
                                          where a.Code == leavetype
                                          select a).FirstOrDefault();
                        if (oLeaveType != null)
                        {
                            oDetail.LeaveType = oLeaveType.ID;
                        }
                        else
                        {
                            oDetail.LeaveType = 0;
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                MsgSuccess("Record Updated Successfully.");
                GetData();
                InitiallizeDocument();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void GetDocumentNo()
        {
            int? intIdt = dbHrPayroll.TrnsDeductionRules.Max(u => (int?)u.DocNo);
            //int DocCount = dbHrPayroll.TrnsLeavesRequest.Count() + 1;
            intIdt = intIdt == null ? 1 : intIdt + 1;
            txtDocNum.Value = Convert.ToString(intIdt);
        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                var oDocuments = (from a in dbHrPayroll.TrnsDeductionRules select a).ToList();
                Int32 i = 0;
                foreach (var oDoc in oDocuments)
                {
                    CodeIndex.Add(i, oDoc.DocNo);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
            }
        }

        private void FillRecord()
        {
            Int16 i = 0;
            try
            {
                if (CodeIndex.Count == 0) return;
                string value = CodeIndex[currentRecord].ToString();
                if (!string.IsNullOrEmpty(value))
                {

                    var oDoc = (from a in dbHrPayroll.TrnsDeductionRules
                                where a.DocNo.ToString() == value
                                select a).FirstOrDefault();
                    if (oDoc == null) return;
                    txtDocNum.Value = Convert.ToString(oDoc.DocNo);
                    txtDeductionCode.Value = oDoc.Code;
                    var AttRules = oDoc.TrnsDeductionRulesDetail.ToList();
                    if (AttRules != null && AttRules.Count > 0)
                    {
                        dtRules.Rows.Clear();
                        dtRules.Rows.Add(AttRules.Count());
                        foreach (var Rule in AttRules)
                        {
                            dtRules.SetValue("No", i, i + 1);
                            dtRules.SetValue("RuleCode", i, Rule.Code);
                            dtRules.SetValue("Value", i, Rule.Value);
                            dtRules.SetValue("RangeFrom", i, Rule.RangeFrom);
                            dtRules.SetValue("RangeTo", i, Rule.RangeTo);
                            dtRules.SetValue("flgDeduct", i, Rule.Deduction == true ? "Y" : "N");
                            //if (Rule.Deduction == true)
                            if (true)
                            {
                                var LeaveTypeID = dbHrPayroll.MstLeaveType.Where(lt => lt.ID == Rule.LeaveType).FirstOrDefault();
                                if (LeaveTypeID != null)
                                {
                                    dtRules.SetValue("LeaveType", i, LeaveTypeID.Code);
                                }
                                else
                                {
                                    dtRules.SetValue("LeaveType", i, 0);
                                    dtRules.SetValue("flgDeduct", i, "N");
                                }
                            }
                            else
                            {
                                dtRules.SetValue("LeaveType", i, 0);
                            }
                            dtRules.SetValue(clGracePeriod.DataBind.Alias, i, Rule.GracePeriod != null ? Convert.ToString(Rule.GracePeriod) : "0");
                            dtRules.SetValue(clLeaveCount.DataBind.Alias, i, Rule.LeaveCount != null ? string.Format("{0:0.00}", Convert.ToDecimal(Rule.LeaveCount)) : "0");
                            i++;
                        }
                        grdRules.LoadFromDataSource();
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
