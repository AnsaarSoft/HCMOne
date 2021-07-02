using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_DeductionRules : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btnCancel, btnSave;
        SAPbouiCOM.DataTable dtRules;
        SAPbouiCOM.Matrix grdRules;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clCode, clValue, clRangeFrom, clRangeTo, clflgDeduct, clLeaveType, clGracePeriod, clLeaveCount;

        private bool Validate;

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
                        ValidateSaveRecords();
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
        
        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                btnSave = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                InitiallizegridMatrix();
                grdRules.AutoResizeColumns();
                FillLeaveTypeInCombo();
                GetRulesRecords();
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
        
        private void ValidateSaveRecords()
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
                            else
                            {
                                leaveCode = 0;
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

        
      

        #endregion
    }
}
