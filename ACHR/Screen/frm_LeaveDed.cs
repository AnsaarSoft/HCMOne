using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_LeaveDed : HRMSBaseForm
    {
        #region "Global Variable Area"

        private SAPbouiCOM.Button btnOk;
        private SAPbouiCOM.Item ibtnOk;
        private SAPbouiCOM.Matrix mtMain;
        private SAPbouiCOM.Column Code, Description, LeaveType, Active, isNew, Id, Serial,Value;
        private SAPbouiCOM.DataTable dtLeaveDeduction;
            
        #endregion

        #region "Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                        UpdateLeaveDeduction();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            //EachItemshould be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the controll object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */
            try
            {
                mtMain = oForm.Items.Item("mtLevDed").Specific;
                isNew = mtMain.Columns.Item("isnew");
                isNew.Visible = false;
                Id = mtMain.Columns.Item("id");
                Id.Visible = false;
                Code = mtMain.Columns.Item("code");
                Description = mtMain.Columns.Item("desc");
                LeaveType = mtMain.Columns.Item("levtype");
                Value = mtMain.Columns.Item("Value");
                Active = mtMain.Columns.Item("active");
                Serial = mtMain.Columns.Item("serial");
                
                dtLeaveDeduction = oForm.DataSources.DataTables.Item("dtMain");
                btnOk = oForm.Items.Item("1").Specific;
                ibtnOk = oForm.Items.Item("1");
                dtLeaveDeduction.Rows.Clear();

                fillColumCombo("Val_Type", LeaveType);

                FillLeaveDeduction();
                AddEmptyRow();


            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: InitiallizeForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtLeaveDeduction.Rows.Count == 0)
            {
                dtLeaveDeduction.Rows.Add(1);
                RowValue = dtLeaveDeduction.Rows.Count;
                dtLeaveDeduction.SetValue(isNew.DataBind.Alias, RowValue - 1, "Y");
                dtLeaveDeduction.SetValue("id", RowValue - 1, "0");
                dtLeaveDeduction.SetValue("code", RowValue - 1, "");
                dtLeaveDeduction.SetValue("desc", RowValue - 1, "");
                dtLeaveDeduction.SetValue("levtype", RowValue - 1, "");
                dtLeaveDeduction.SetValue("active", RowValue - 1, "");
                dtLeaveDeduction.SetValue("Value", RowValue - 1, "0.00");
                dtLeaveDeduction.SetValue("serial", RowValue - 1, RowValue);
                mtMain.AddRow(1, RowValue + 1);
            }
            else
            {
                if ( dtLeaveDeduction.GetValue("code", dtLeaveDeduction.Rows.Count -1 ) == "" )
                {
                }
                else
                {

                    dtLeaveDeduction.Rows.Add(1);
                    RowValue = dtLeaveDeduction.Rows.Count;
                    dtLeaveDeduction.SetValue("isnew", RowValue - 1, "Y");
                    dtLeaveDeduction.SetValue("id", RowValue - 1, "0");
                    dtLeaveDeduction.SetValue("code", RowValue - 1, "");
                    dtLeaveDeduction.SetValue("desc", RowValue - 1, "");
                    dtLeaveDeduction.SetValue("levtype", RowValue - 1, "");
                    dtLeaveDeduction.SetValue("Value", RowValue - 1, "0.00");
                    dtLeaveDeduction.SetValue("active", RowValue - 1, "");
                    dtLeaveDeduction.SetValue("serial", RowValue - 1, RowValue);
                    mtMain.AddRow(1, mtMain.RowCount + 1);
                }
            }
            mtMain.LoadFromDataSource();
        }

        private void UpdateLeaveDeductionOld()
        {
            try
            {
                mtMain.FlushToDataSource();
                String LevCode, LevDesc, IsNewValue, LevId, LevType, StrActive,Value;
                Boolean ActiveValue = false;
                for (int i = 0; i < dtLeaveDeduction.Rows.Count; i++)
                {
                    LevCode = Convert.ToString(dtLeaveDeduction.GetValue("code", i));
                    LevDesc = Convert.ToString( dtLeaveDeduction.GetValue("desc",i) );
                    IsNewValue = Convert.ToString( dtLeaveDeduction.GetValue("isnew",i) );
                    LevId = Convert.ToString(dtLeaveDeduction.GetValue("id",i));
                    LevType = Convert.ToString(dtLeaveDeduction.GetValue("levtype", i));
                    StrActive = Convert.ToString(dtLeaveDeduction.GetValue("active", i));
                    Value = Convert.ToString(dtLeaveDeduction.GetValue("Value", i));
                    if (StrActive == "Y" )
                    {
                        ActiveValue = true;
                    }
                    else
                    {
                        ActiveValue = false;
                    }
                    var codecheck = (from a in dbHrPayroll.MstLeaveDeduction where a.Code == LevCode select a).Count();
                    if (codecheck > 0 && IsNewValue.Trim().ToLower() == "y")
                    {
                        oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    else if (codecheck > 1 && IsNewValue.Trim().ToLower() == "n")
                    {
                        oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (IsNewValue.Trim().ToLower() == "y" && LevDesc == "")
                    {
                        oApplication.StatusBar.SetText("Description cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (IsNewValue.Trim().ToLower() == "y" && LevType == "")
                    {
                        oApplication.StatusBar.SetText("Type cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if ((Value == "") || (Value == "0"))
                    {
                        oApplication.StatusBar.SetText("Zero value cannot be entered.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    if (Value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive Value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                   
                    if (LevCode != "")
                    {
                        if (IsNewValue == "Y")
                        {
                            var LeaveDed = dbHrPayroll.MstLeaveDeduction.Where(e => e.Code == LevCode).FirstOrDefault();
                            if (LeaveDed != null)
                            {
                                oApplication.StatusBar.SetText("Duplication in leave deduction code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }

                            MstLeaveDeduction oNew = new MstLeaveDeduction();
                            oNew.Code = LevCode;
                            oNew.Description = LevDesc;
                            oNew.TypeofDeduction = LevType;
                            oNew.DeductionStatus = ActiveValue;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UserId = "manager";
                            oNew.DeductionValue = Convert.ToDecimal(Value);


                            dbHrPayroll.MstLeaveDeduction.InsertOnSubmit(oNew);
                        }
                        else if (IsNewValue == "N")
                        {
                            var oOld = (from a in dbHrPayroll.MstLeaveDeduction where a.Id == Convert.ToInt32(LevId) select a).FirstOrDefault();
                            var LeaveUsedDeductionCode = dbHrPayroll.TrnsLeavesRequest.Where(le => le.DeductId == LevCode).ToList();
                            if (LeaveUsedDeductionCode != null && LeaveUsedDeductionCode.Count > 0)
                            {
                                if (oOld.DeductionStatus != ActiveValue)
                                {
                                    oApplication.StatusBar.SetText("Deduction code can't be updated already used with Leave(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                if (oOld.Code != LevCode)
                                {
                                    oApplication.StatusBar.SetText("Deduction code can't be updated already used with Leave(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }

                            }
                            else
                            {
                                int confirm = oApplication.MessageBox("Are you sure you want to Active / deactive leave? ", 3, "Yes", "No", "Cancel");
                                if (confirm == 2 || confirm == 3)
                                {
                                    return;
                                }
                                else
                                {
                                    oOld.Description = LevDesc;
                                    oOld.DeductionStatus = ActiveValue;
                                    oOld.TypeofDeduction = LevType;
                                    oOld.DeductionValue = Convert.ToDecimal(Value);
                                    oOld.UpdateDate = DateTime.Now;
                                    oOld.UpdateBy = "manager";
                                    dbHrPayroll.SubmitChanges();
                                    AddEmptyRow();
                                }
                            }
                            //var LeaveDed = dbHrPayroll.MstLeaveType.Where(e => e.DeductionCode == LevCode).FirstOrDefault();
                            //if (LeaveDed != null)
                            //{
                            //    oApplication.StatusBar.SetText("Leave code already attached with leave type.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    return;
                            //}
                            if (LevType == "")
                            {
                                oApplication.StatusBar.SetText("Type cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                            if (Value.StartsWith("-") || (Value == "") || (Value.StartsWith("0.0")))
                            {
                                oApplication.StatusBar.SetText("Value cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                                
                            oOld.Description = LevDesc;
                            oOld.DeductionStatus = ActiveValue;
                            oOld.TypeofDeduction = LevType;
                            oOld.DeductionValue = Convert.ToDecimal(Value);
                            oOld.UpdateDate = DateTime.Now;
                            oOld.UpdateBy = "manager";
                        }

                        dbHrPayroll.SubmitChanges();
                        AddEmptyRow();
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: UpdateLeaveDeduction Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateLeaveDeduction()
        {
            try
            {
                mtMain.FlushToDataSource();
                String LevCode, LevDesc, IsNewValue, LevId, LevType, StrActive, Value;
                Boolean ActiveValue = false;
                for (int i = 0; i < dtLeaveDeduction.Rows.Count; i++)
                {
                    LevCode = Convert.ToString(dtLeaveDeduction.GetValue("code", i));
                    LevDesc = Convert.ToString(dtLeaveDeduction.GetValue("desc", i));
                    IsNewValue = Convert.ToString(dtLeaveDeduction.GetValue("isnew", i));
                    LevId = Convert.ToString(dtLeaveDeduction.GetValue("id", i));
                    LevType = Convert.ToString(dtLeaveDeduction.GetValue("levtype", i));
                    StrActive = Convert.ToString(dtLeaveDeduction.GetValue("active", i));
                    Value = Convert.ToString(dtLeaveDeduction.GetValue("Value", i));
                    if (StrActive == "Y")
                    {
                        ActiveValue = true;
                    }
                    else
                    {
                        ActiveValue = false;
                    }                   

                    if (LevCode != "")
                    {
                        if (IsNewValue == "Y")
                        {
                            var LeaveDed = dbHrPayroll.MstLeaveDeduction.Where(e => e.Code == LevCode).FirstOrDefault();
                            if (LeaveDed != null)
                            {
                                oApplication.StatusBar.SetText("Duplication in leave deduction code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }

                            MstLeaveDeduction oNew = new MstLeaveDeduction();
                            oNew.Code = LevCode;
                            oNew.Description = LevDesc;
                            oNew.TypeofDeduction = LevType;
                            oNew.DeductionStatus = ActiveValue;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UserId = "manager";
                            oNew.DeductionValue = Convert.ToDecimal(Value);


                            dbHrPayroll.MstLeaveDeduction.InsertOnSubmit(oNew);
                        }
                        else if (IsNewValue == "N")
                        {
                            var oOld = (from a in dbHrPayroll.MstLeaveDeduction where a.Id == Convert.ToInt32(LevId) select a).FirstOrDefault();
                            var LeaveUsedDeductionCode = dbHrPayroll.TrnsLeavesRequest.Where(le => le.DeductId == LevCode).ToList();
                            if (LeaveUsedDeductionCode != null && LeaveUsedDeductionCode.Count > 0)
                            {
                                if (oOld.DeductionStatus != ActiveValue)
                                {
                                    oApplication.StatusBar.SetText("Deduction code can't be updated already used with Leave(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                if (oOld.Code != LevCode)
                                {
                                    oApplication.StatusBar.SetText("Deduction code can't be updated already used with Leave(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }

                            }
                            else
                            {
                                int confirm = oApplication.MessageBox("Are you sure you want to Active / deactive leave? ", 3, "Yes", "No", "Cancel");
                                if (confirm == 2 || confirm == 3)
                                {
                                    return;
                                }
                                else
                                {
                                    oOld.Description = LevDesc;
                                    oOld.DeductionStatus = ActiveValue;
                                    oOld.TypeofDeduction = LevType;
                                    oOld.DeductionValue = Convert.ToDecimal(Value);
                                    oOld.UpdateDate = DateTime.Now;
                                    oOld.UpdateBy = "manager";
                                    dbHrPayroll.SubmitChanges();
                                    AddEmptyRow();
                                }
                            }
                            //var LeaveDed = dbHrPayroll.MstLeaveType.Where(e => e.DeductionCode == LevCode).FirstOrDefault();
                            //if (LeaveDed != null)
                            //{
                            //    oApplication.StatusBar.SetText("Leave code already attached with leave type.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    return;
                            //}
                            //if (LevType == "")
                            //{
                            //    oApplication.StatusBar.SetText("Type cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    return;
                            //}
                            //if (Value.StartsWith("-") || (Value == "") || (Value.StartsWith("0.0")))
                            //{
                            //    oApplication.StatusBar.SetText("Value cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    return;
                            //}
                            if ((Value == "") || (Value == "0"))
                            {
                                oApplication.StatusBar.SetText("Zero value cannot be entered.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return ;
                            }
                            if (Value.StartsWith("-"))
                            {
                                oApplication.StatusBar.SetText("Negetive Value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return ;
                            }
                            oOld.Description = LevDesc;
                            oOld.DeductionStatus = ActiveValue;
                            oOld.TypeofDeduction = LevType;
                            oOld.DeductionValue = Convert.ToDecimal(Value);
                            oOld.UpdateDate = DateTime.Now;
                            oOld.UpdateBy = "manager";
                        }

                        dbHrPayroll.SubmitChanges();
                        AddEmptyRow();
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: UpdateLeaveDeduction Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private Boolean ValidateForm()
        {
            try
            {
                //chek loan activity.
                mtMain.FlushToDataSource();
                for (int i = 0; i < dtLeaveDeduction.Rows.Count; i++)
                {
                    string OTcode = dtLeaveDeduction.GetValue(Code.DataBind.Alias, i);
                    string LevDesc = dtLeaveDeduction.GetValue(Description.DataBind.Alias, i);
                    string LevType = dtLeaveDeduction.GetValue(LeaveType.DataBind.Alias, i);
                    string OTValue = dtLeaveDeduction.GetValue(Value.DataBind.Alias, i);
                    string OTstatus = dtLeaveDeduction.GetValue(Active.DataBind.Alias, i);
                    string IsNewValue = dtLeaveDeduction.GetValue(isNew.DataBind.Alias, i);
                    if (!string.IsNullOrEmpty(OTcode) && !string.IsNullOrEmpty(OTstatus))
                    {
                        Boolean flgActive = false;
                        if (OTstatus.Trim().ToLower() == "y")
                        {
                            flgActive = true;
                        }
                        else
                        {
                            flgActive = false;
                        }
                        //for duplicate code
                        var codecheck = (from a in dbHrPayroll.MstLeaveDeduction where a.Code == OTcode select a).Count();
                        if (codecheck > 0 && IsNewValue.Trim().ToLower() == "y")
                        {
                            oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        else if (codecheck > 1 && IsNewValue.Trim().ToLower() == "n")
                        {
                            oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }                        
                        
                        if (IsNewValue.Trim().ToLower() == "y" && LevDesc == "")
                        {
                            oApplication.StatusBar.SetText("Description cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (IsNewValue.Trim().ToLower() == "y" && LevType == "")
                        {
                            oApplication.StatusBar.SetText("Type cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if ((OTValue == "") || (OTValue == "0"))
                        {
                            oApplication.StatusBar.SetText("Zero value cannot be entered.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (OTValue.StartsWith("-"))
                        {
                            oApplication.StatusBar.SetText("Negetive Value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        //if (Value.StartsWith("-") || (Value == "") || (Value.StartsWith("0.0")))
                        //{
                        //    oApplication.StatusBar.SetText("Value cannot be empty.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return;
                        //}
                        
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void FillLeaveDeduction()
        {
            try
            {
                var LeaveDeductions = from a in dbHrPayroll.MstLeaveDeduction select a;
                Int16 i = 0;
                if (LeaveDeductions.Count() == 0)
                {
                    return;
                }
                dtLeaveDeduction.Rows.Clear();
                dtLeaveDeduction.Rows.Add(LeaveDeductions.Count());
                foreach (var LD in LeaveDeductions)
                {
                    dtLeaveDeduction.SetValue("isnew", i, "N");
                    dtLeaveDeduction.SetValue("id", i, LD.Id);
                    dtLeaveDeduction.SetValue("code", i, LD.Code);
                    dtLeaveDeduction.SetValue("desc", i, LD.Description);
                    dtLeaveDeduction.SetValue("levtype", i, LD.TypeofDeduction);
                    dtLeaveDeduction.SetValue("Value", i, LD.DeductionValue.ToString());
                    if (Convert.ToBoolean(LD.DeductionStatus))
                    {
                        dtLeaveDeduction.SetValue("active", i, "Y");
                    }
                    else
                    { 
                        dtLeaveDeduction.SetValue("active", i, "N");
                    }
                    dtLeaveDeduction.SetValue("serial", i, i+1);
                    i++;
                }
                
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LeaveDeduction Function: FillLeaveDeduction Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
