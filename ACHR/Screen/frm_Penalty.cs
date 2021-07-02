using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;


namespace ACHR.Screen
{
    class frm_Penalty : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btnCancel, btnSave;
        SAPbouiCOM.DataTable dtPenaltyRules;
        SAPbouiCOM.Matrix grdPenaltyRules;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clCode, clRuleDesc, clDays, clPenalty,clLeaveType;       

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
                oApplication.StatusBar.SetText("Form: frm_Penalty Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    case "btnassg":
                        updateStdElements();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_Penalty Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btnSave = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                InitiallizegridMatrix();
                FillLeaveTypeInCombo();
                GetPealtyRulesRecords();
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
                dtPenaltyRules = oForm.DataSources.DataTables.Add("penltyRules");
                dtPenaltyRules.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtPenaltyRules.Columns.Add("RuleCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPenaltyRules.Columns.Add("RuleDesc", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPenaltyRules.Columns.Add("Days", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPenaltyRules.Columns.Add("Penalty", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPenaltyRules.Columns.Add("LeaveType", SAPbouiCOM.BoFieldsType.ft_Text); 

                grdPenaltyRules = (SAPbouiCOM.Matrix)oForm.Items.Item("grdRule").Specific;
                oColumns = (SAPbouiCOM.Columns)grdPenaltyRules.Columns;


                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("penltyRules", "No");

                oColumn = oColumns.Item("clCode");
                clCode = oColumn;
                oColumn.DataBind.Bind("penltyRules", "RuleCode");

                oColumn = oColumns.Item("clDesc");
                clRuleDesc = oColumn;
                oColumn.DataBind.Bind("penltyRules", "RuleDesc");

                oColumn = oColumns.Item("clday");
                clDays = oColumn;
                oColumn.DataBind.Bind("penltyRules", "Days");

                oColumn = oColumns.Item("clPen");
                clPenalty = oColumn;
                oColumn.DataBind.Bind("penltyRules", "Penalty");

                oColumn = oColumns.Item("clLeave");
                clLeaveType = oColumn;
                oColumn.DataBind.Bind("penltyRules", "LeaveType");
                clLeaveType.Visible = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetPealtyRulesRecords()
        {
            Int16 i = 0;
            try
            {
                var AttRules = dbHrPayroll.MstPenaltyRules.ToList();
                if (AttRules != null && AttRules.Count > 0)
                {
                    dtPenaltyRules.Rows.Clear();
                    dtPenaltyRules.Rows.Add(AttRules.Count());
                    foreach (var Rule in AttRules)
                    {
                        dtPenaltyRules.SetValue("No", i, i + 1);
                        dtPenaltyRules.SetValue("RuleCode", i, Rule.Code);
                        dtPenaltyRules.SetValue("RuleDesc", i, Rule.Description);
                        dtPenaltyRules.SetValue("Days", i, Rule.Days);
                        dtPenaltyRules.SetValue("Penalty", i, Rule.PenaltyDays);
                        if (Rule.LeaveType != null)
                        {
                            dtPenaltyRules.SetValue("LeaveType", i, Rule.LeaveType);
                        }
                                         
                        i++;
                    }
                    grdPenaltyRules.LoadFromDataSource();
                }
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
                var LeaveType = from a in dbHrPayroll.MstLeaveType select a;
                clLeaveType.ValidValues.Add(Convert.ToString(0), Convert.ToString("NONE"));
                foreach (MstLeaveType empLeaveType in LeaveType)
                {
                    clLeaveType.ValidValues.Add(Convert.ToString(empLeaveType.ID), Convert.ToString(empLeaveType.Description));
                }
                clLeaveType.DisplayDesc = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_Penalty Function: FillLeaveTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ValidateSaveRecords()
        {
            try
            {
                string Code, cldays, clpenaltydays, clLaeaveTypeX;                
                if (dtPenaltyRules != null && dtPenaltyRules.Rows.Count > 0)
                {
                    for (int i = 1; i <= grdPenaltyRules.RowCount; i++)
                    {
                        Code = (grdPenaltyRules.Columns.Item("clCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        var oOld = dbHrPayroll.MstPenaltyRules.Where(atr => atr.Code == Code).FirstOrDefault();
                        if (oOld != null)
                        {
                            cldays = (grdPenaltyRules.Columns.Item("clday").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            clpenaltydays = (grdPenaltyRules.Columns.Item("clPen").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            //clLaeaveTypeX = (grdPenaltyRules.Columns.Item("clLeave").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                            if (!string.IsNullOrEmpty(cldays))
                            {
                                oOld.Days = Convert.ToInt32(cldays);
                            }
                            if (!string.IsNullOrEmpty(clpenaltydays))
                            {
                                oOld.PenaltyDays = Convert.ToInt32(clpenaltydays);
                            }
                            //if (!string.IsNullOrEmpty(clLaeaveTypeX) && clLaeaveTypeX != "0")
                            //{
                            //    oOld.LeaveType = Convert.ToInt32(clLaeaveTypeX);
                            //}
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

        public void updateStdElements()
        {
            int confirm = oApplication.MessageBox("Are you sure you want to Assign Penalty(s)? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3)
            {
                return;
            }
            SAPbouiCOM.ProgressBar prog = null;
            try
            {
                IEnumerable<MstEmployee> emps = from p in dbHrPayroll.MstEmployee where p.FlgActive == true select p;
                int totalEmps = emps.Count();

                prog = oApplication.StatusBar.CreateProgressBar("Assign Employee Penalty", totalEmps, false);
                prog.Value = 0;

                foreach (MstEmployee emp in emps)
                {
                    System.Windows.Forms.Application.DoEvents();
                    //ds.updateStandardElements(emp, chUpdate.Checked);
                    AssignStandardPenalty(emp);
                    prog.Value += 1;
                }

            }

            catch (Exception ex)
            {

                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
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

        public void AssignStandardPenalty(MstEmployee instance)
        {
            IEnumerable<MstPenaltyRules> prules = from p in dbHrPayroll.MstPenaltyRules select p;
            foreach (MstPenaltyRules empprules in prules)
            {
                //throw new NotImplementedException();
                if (instance.PayrollID != null)
                {
                    int i = 0;
                    TrnsEmployeePenalty empEle;
                    MstPenaltyRules penalty = dbHrPayroll.MstPenaltyRules.Where(r => r.ID == empprules.ID).FirstOrDefault();
                    int cnt = (from p in dbHrPayroll.TrnsEmployeePenalty where p.EmpId == instance.ID && p.PenaltyId == empprules.ID select p).Count();
                    if (cnt == 0)
                    {
                        empEle = new TrnsEmployeePenalty();
                        empEle.PenaltyId = empprules.ID;
                        empEle.MstEmployee = instance;
                        empEle.Days = penalty.Days;
                        empEle.PenaltyDays = penalty.PenaltyDays;
                        dbHrPayroll.TrnsEmployeePenalty.InsertOnSubmit(empEle);
                    }
                    else
                    {
                        empEle = (from p in dbHrPayroll.TrnsEmployeePenalty where p.EmpId == instance.ID && p.PenaltyId == empprules.ID select p).FirstOrDefault();
                        empEle.Days = penalty.Days;
                        empEle.PenaltyDays = penalty.PenaltyDays;
                    }                    
                    dbHrPayroll.SubmitChanges();
                }
            }
        }

        #endregion
    }
}
