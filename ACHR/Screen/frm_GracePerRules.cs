using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_GracePerRules : HRMSBaseForm
    {
        
        #region "Global Variable Area"

        SAPbouiCOM.CheckBox flgGracePeriodActive, flgLateIN, flgEarlyOut, flgEarlyIN, flgLateOut, flgconsecutive, flgWOPOverflow, flgLateInTriger;
        SAPbouiCOM.CheckBox flgShortLeaveTriger, flgTimeBasedeductionRules, flgSandwichLeave;
        SAPbouiCOM.EditText txBWTS, txAWTS, txBWTE, txAWTE, txLateInCount, txtShortLeaveCount;
        SAPbouiCOM.Button btnSave, btnCancel;
        SAPbouiCOM.ComboBox cbLeaveType, cbLWOPType;

        SAPbouiCOM.Item IcbLeaveType, IcbLWOPType;
        private bool Validate;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                
                oForm.Freeze(false);
                FillLeaveTypeCombo();
                FillLeaveWOPTypeCombo();
                GetRulesRecords();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_GracePerRules Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etBeforeValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {            
            try
            {
                BubbleEvent = true;
                Validate = false;
                switch (pVal.ItemUID)
                {                                                         
                    case "txAWTE":
                        {
                           Validate = ValidateTextBoxesValues(txBWTS.Value);                    
                        }
                        break;
                    case "txBWTE":
                        {
                            Validate = ValidateTextBoxesValues(txBWTE.Value);   
                        }
                        break;
                    case "txAWTS":
                        {
                            Validate = ValidateTextBoxesValues(txAWTS.Value);
                        }
                        break;
                    case "txBWTS":
                        {
                            Validate = ValidateTextBoxesValues(txBWTS.Value);
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
                oApplication.StatusBar.SetText("Form: Frm_GracePerRules Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                //Initializing Button
                btnSave = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                //Initializing Text Boxes
                txBWTS = oForm.Items.Item("txBWTS").Specific;
                txAWTS = oForm.Items.Item("txAWTS").Specific;
                txBWTE = oForm.Items.Item("txBWTE").Specific;
                txAWTE = oForm.Items.Item("txAWTE").Specific;
                txLateInCount = oForm.Items.Item("txtLTInCnt").Specific;
                //Initializing CheckBoxes
                oForm.DataSources.UserDataSources.Add("flgGP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgGracePeriodActive = oForm.Items.Item("flgGP").Specific;
                flgGracePeriodActive.DataBind.SetBound(true, "", "flgGP");

                oForm.DataSources.UserDataSources.Add("flgLtIN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgLateIN = oForm.Items.Item("flgLtIN").Specific;
                flgLateIN.DataBind.SetBound(true, "", "flgLtIN");

                oForm.DataSources.UserDataSources.Add("flgEOut", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgEarlyOut = oForm.Items.Item("flgEOut").Specific;
                flgEarlyOut.DataBind.SetBound(true, "", "flgEOut");

                oForm.DataSources.UserDataSources.Add("flgEIN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgEarlyIN = oForm.Items.Item("flgEIN").Specific;
                flgEarlyIN.DataBind.SetBound(true, "", "flgEIN");

                oForm.DataSources.UserDataSources.Add("flgLOUT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgLateOut = oForm.Items.Item("flgLOUT").Specific;
                flgLateOut.DataBind.SetBound(true, "", "flgLOUT");

                oForm.DataSources.UserDataSources.Add("flgCon", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgconsecutive = oForm.Items.Item("flgCon").Specific;
                flgconsecutive.DataBind.SetBound(true, "", "flgCon");

                oForm.DataSources.UserDataSources.Add("chsl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgSandwichLeave = oForm.Items.Item("chsl").Specific;
                flgSandwichLeave.DataBind.SetBound(true, "", "chsl");

                oForm.DataSources.UserDataSources.Add("flgwop", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgWOPOverflow = oForm.Items.Item("flgwop").Specific;
                flgWOPOverflow.DataBind.SetBound(true, "", "flgwop");

                oForm.DataSources.UserDataSources.Add("flgShrtLve", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgShortLeaveTriger = oForm.Items.Item("flgShrtLve").Specific;
                flgShortLeaveTriger.DataBind.SetBound(true, "", "flgShrtLve");

                oForm.DataSources.UserDataSources.Add("flgDedTime", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgTimeBasedeductionRules = oForm.Items.Item("flgDedTime").Specific;
                flgTimeBasedeductionRules.DataBind.SetBound(true, "", "flgDedTime");

                txtShortLeaveCount = oForm.Items.Item("txtCounter").Specific;

                oForm.DataSources.UserDataSources.Add("flgLateIn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgLateInTriger = oForm.Items.Item("flgLateIn").Specific;
                flgLateInTriger.DataBind.SetBound(true, "", "flgLateIn");

                oForm.DataSources.UserDataSources.Add("cbLeave", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbLeaveType = oForm.Items.Item("cbLeave").Specific;
                cbLeaveType.DataBind.SetBound(true, "", "cbLeave");

                oForm.DataSources.UserDataSources.Add("cbLWOP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbLWOPType = oForm.Items.Item("cbLWOP").Specific;
                cbLWOPType.DataBind.SetBound(true, "", "cbLWOP");



                //flgCon

                //oForm.DataSources.UserDataSources.Add("flgOVT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); 
                //flgOvertime = oForm.Items.Item("flgOVT").Specific;
                //flgOvertime.DataBind.SetBound(true, "", "flgOVT");

                //oForm.DataSources.UserDataSources.Add("flgEOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                //flgEarlyOT = oForm.Items.Item("flgEOT").Specific;
                //flgEarlyOT.DataBind.SetBound(true, "", "flgEOT");

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetRulesRecords()
        {
            try
            {
                var AttRules = dbHrPayroll.MstAttendanceRule.FirstOrDefault();

                if (AttRules != null)
                {
                    txAWTE.Value = AttRules.GpAfterTimeEnd;
                    txAWTS.Value = AttRules.GpAfterStartTime;
                    txBWTE.Value = AttRules.GpBeforeTimeEnd;
                    txBWTS.Value = AttRules.GpBeforeStartTime;
                    txLateInCount.Value = Convert.ToString(AttRules.LateInCountTriger);
                    txtShortLeaveCount.Value = Convert.ToString(AttRules.ShortLeaveCount);
                    var LeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == AttRules.LateInCountLeaveType && a.Active == true select a).FirstOrDefault();
                    if (LeaveType != null)
                    {
                        //cbLeaveType.Select(LeaveType.Code != null ? LeaveType.Code.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                        cbLeaveType.Select(LeaveType.Code.Trim());
                    }
                    var LeaveWOP = (from a in dbHrPayroll.MstLeaveType where a.Code == AttRules.LeaveTypeWOP && a.Active == true select a).FirstOrDefault();
                    if (LeaveWOP != null)
                    {
                        cbLWOPType.Select(LeaveWOP.Code.Trim());
                    }

                    if (Convert.ToBoolean(AttRules.FlgGpActive))
                    {
                        flgGracePeriodActive.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgEarlyIn))
                    {
                        flgEarlyIN.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgEarlyOut))
                    {
                        flgEarlyOut.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgLateIn))
                    {
                        flgLateIN.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgLateOut))
                    {
                        flgLateOut.Checked = true;
                    }

                    if (Convert.ToBoolean(AttRules.FlgConsecutiveLeave))
                    {
                        flgconsecutive.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgWOPOverFlow))
                    {
                        flgWOPOverflow.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgLateInTriger))
                    {
                        flgLateInTriger.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgShortLeave))
                    {
                        flgShortLeaveTriger.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgTimeBaseDeductionRules))
                    {
                        flgTimeBasedeductionRules.Checked = true;
                    }
                    if (Convert.ToBoolean(AttRules.FlgSandwichLeaves))
                    {
                        flgSandwichLeave.Checked = true;
                    }
                    //if (AttRules.FlgEarlyInOT)
                    //{
                    //    flgEarlyOT.Checked = true;
                    //}
                    //if (AttRules.FlgOvertime)
                    //{
                    //    flgOvertime.Checked = true;
                    //}                
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private bool ValidateTextBoxesValues(string Value)
        {
            try
            {
                Validate = true;                
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
                        return Validate;
                    }
                }
                return Validate;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
        }

        private void ValidateSaveRecords()
        {
            try
            {
                var oOld = dbHrPayroll.MstAttendanceRule.FirstOrDefault();
                if (oOld != null)
                {
                    if (flgGracePeriodActive.Checked)
                    {
                        oOld.FlgGpActive = true;
                    }
                    else
                    {
                        oOld.FlgGpActive = false;
                    }
                    if (flgEarlyIN.Checked)
                    {
                        oOld.FlgEarlyIn = true;
                    }
                    else
                    {
                        oOld.FlgEarlyIn = false;
                    }
                    if (flgEarlyOut.Checked)
                    {
                        oOld.FlgEarlyOut = true;
                    }
                    else
                    {
                        oOld.FlgEarlyOut = false;
                    }
                    if (flgLateIN.Checked)
                    {
                        oOld.FlgLateIn = true;
                    }
                    else
                    {
                        oOld.FlgLateIn = false;
                    }
                    if (flgLateOut.Checked)
                    {
                        oOld.FlgLateOut = true;
                    }
                    else
                    {
                        oOld.FlgLateOut = false;
                    }

                    if (flgconsecutive.Checked)
                    {
                        oOld.FlgConsecutiveLeave = true;
                    }
                    else
                    {
                        oOld.FlgConsecutiveLeave = false;
                    }
                    if (flgWOPOverflow.Checked)
                    {
                        oOld.FlgWOPOverFlow = true;
                    }
                    else
                    {
                        oOld.FlgWOPOverFlow = false;
                    }
                    if (flgLateInTriger.Checked)
                    {
                        oOld.FlgLateInTriger = true;
                    }
                    else
                    {
                        oOld.FlgLateInTriger = false;
                    }
                    if (flgShortLeaveTriger.Checked)
                    {
                        oOld.FlgShortLeave = true;
                    }
                    else
                    {
                        oOld.FlgShortLeave = false;
                    }
                    if (flgTimeBasedeductionRules.Checked)
                    {
                        oOld.FlgTimeBaseDeductionRules = true;
                    }
                    else
                    {
                        oOld.FlgTimeBaseDeductionRules = false;
                    }
                    if (flgSandwichLeave.Checked)
                    {
                        oOld.FlgSandwichLeaves = true;
                    }
                    else
                    {
                        oOld.FlgSandwichLeaves = false;
                    }
                    oOld.GpBeforeStartTime = txBWTS.Value;
                    oOld.GpBeforeTimeEnd = txBWTE.Value;
                    oOld.GpAfterStartTime = txAWTS.Value;
                    oOld.GpAfterTimeEnd = txAWTE.Value;
                    if (!string.IsNullOrEmpty(txLateInCount.Value))
                    {
                        oOld.LateInCountTriger = Convert.ToInt32(txLateInCount.Value);
                    }
                    if (!string.IsNullOrEmpty(txtShortLeaveCount.Value))
                    {
                        oOld.ShortLeaveCount = Convert.ToInt32(txtShortLeaveCount.Value);
                    }
                    if (!string.IsNullOrEmpty(cbLeaveType.Value))
                    {
                        oOld.LateInCountLeaveType = cbLeaveType.Value.Trim();
                    }
                    if (!string.IsNullOrEmpty(cbLWOPType.Value))
                    {
                        oOld.LeaveTypeWOP = cbLWOPType.Value.Trim();
                    }
                    dbHrPayroll.SubmitChanges();

                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void FillLeaveTypeCombo()
        {
            try
            {
                //Clear Combo Records
                if (cbLeaveType.ValidValues.Count > 0)
                {
                    int vcnt = cbLeaveType.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbLeaveType.ValidValues.Remove(cbLeaveType.ValidValues.Item(k).Value);
                    }
                }
                //
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLeaveType);
                
                var Data = from v in dbHrPayroll.MstLeaveType
                           where v.Active == true && v.LeaveType != "Ded" && (v.FlgConditionalProcessing != true || v.FlgConditionalProcessing == null)
                           select v;

                cbLeaveType.ValidValues.Add("-1", "[Select One]");
                foreach (var v in Data)
                {
                    cbLeaveType.ValidValues.Add(v.Code, v.Description);
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLeaveWOPTypeCombo()
        {
            try
            {
                //Clear Combo Records
                if (cbLWOPType.ValidValues.Count > 0)
                {
                    int vcnt = cbLeaveType.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbLWOPType.ValidValues.Remove(cbLeaveType.ValidValues.Item(k).Value);
                    }
                }
                //
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLeaveType);
                
                var Data = from v in dbHrPayroll.MstLeaveType                
                           where v.Active == true && v.LeaveType == "Ded" && (v.FlgConditionalProcessing != true || v.FlgConditionalProcessing == null)
                           select v;

                cbLWOPType.ValidValues.Add("-1", "[Select One]");
                foreach (var v in Data)
                {
                    cbLWOPType.ValidValues.Add(v.Code, v.Description);
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
