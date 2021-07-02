using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_EmpPN : HRMSBaseForm
    {
        #region "Global Variable Area"

        private bool Validate;
        SAPbouiCOM.Button btnSerch, btnSave, btnID, btnCancel, btnOK;
        SAPbouiCOM.EditText txtEmpId;
        SAPbouiCOM.ComboBox cb_Location, cb_depart, cb_deignation,cb_payroll,cb_Branch;
        SAPbouiCOM.Item Icb_Location, Icb_depart, Icb_deignation, Icb_payroll, Icb_Branch, ibtnOK;
        
        SAPbouiCOM.DataTable dtEmployees, dtPenalty;
        SAPbouiCOM.Matrix grdEmployees, grdPenalty;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, EmpCode, EmpName, Desig, Depart, Location, isSel, clId, clPenaltyCode, clFDate, clTDate, clDays, clPenalty;

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
                FillEmpBranchInCombo();
                FillEmpPayrollInCombo();
                FillPenaltyTypesInCombo();
                ibtnOK.Visible = false;
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    case "btnSerc":
                        PopulateGridWithFilterExpression();
                        break;
                    case "btnSave":
                        SaveRecords();
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
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmpId == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmpId.Value)
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

        }
        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                oForm.PaneLevel = 1;
                btnSerch = oForm.Items.Item("btnSerc").Specific;
                btnSave = oForm.Items.Item("btnSave").Specific;            
                btnID = oForm.Items.Item("btId").Specific;
                btnOK = oForm.Items.Item("1").Specific;
                ibtnOK = oForm.Items.Item("1");
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpId = oForm.Items.Item("txtEmpC").Specific;
                txtEmpId.DataBind.SetBound(true, "", "txtEmpC");


                
                

                cb_depart = oForm.Items.Item("cb_dpt").Specific;
                oForm.DataSources.UserDataSources.Add("cb_dpt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cb_depart.DataBind.SetBound(true, "", "cb_dpt");
                Icb_depart = oForm.Items.Item("cb_dpt");

                cb_deignation = oForm.Items.Item("cb_desg").Specific;
                oForm.DataSources.UserDataSources.Add("cb_desg", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cb_deignation.DataBind.SetBound(true, "", "cb_desg");
                Icb_deignation = oForm.Items.Item("cb_desg");

                cb_Location = oForm.Items.Item("cb_loc").Specific;
                oForm.DataSources.UserDataSources.Add("cb_loc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cb_Location.DataBind.SetBound(true, "", "cb_loc");
                Icb_Location = oForm.Items.Item("cb_loc");

                cb_Branch = oForm.Items.Item("cb_Brnc").Specific;
                oForm.DataSources.UserDataSources.Add("cb_Brnc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cb_Branch.DataBind.SetBound(true, "", "cb_Brnc");
                Icb_Branch = oForm.Items.Item("cb_Brnc");

                cb_payroll = oForm.Items.Item("cb_Prl").Specific;
                oForm.DataSources.UserDataSources.Add("cb_Prl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cb_payroll.DataBind.SetBound(true, "", "cb_Prl");
                Icb_payroll = oForm.Items.Item("cb_Prl");

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
                dtEmployees.Columns.Add("Branch", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmployees.Columns.Add("isSel", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);

                grdEmployees = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Emp").Specific;             
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

                oColumn = oColumns.Item("brnch");
                Location = oColumn;
                oColumn.DataBind.Bind("Employees", "Branch");

                oColumn = oColumns.Item("Loc");
                Location = oColumn;
                oColumn.DataBind.Bind("Employees", "Location");

                oColumn = oColumns.Item("isSel");
                isSel = oColumn;
                oColumn.DataBind.Bind("Employees", "isSel");

                // Second Grid Initialization

                dtPenalty = oForm.DataSources.DataTables.Add("penltyRules");
                dtPenalty.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtPenalty.Columns.Add("RuleCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPenalty.Columns.Add("clFDate", SAPbouiCOM.BoFieldsType.ft_Date);
                dtPenalty.Columns.Add("clTDate", SAPbouiCOM.BoFieldsType.ft_Date);
                dtPenalty.Columns.Add("Days", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPenalty.Columns.Add("Penalty", SAPbouiCOM.BoFieldsType.ft_Text);


                grdPenalty = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Pn").Specific;
                oColumns = (SAPbouiCOM.Columns)grdPenalty.Columns;
              

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("penltyRules", "No");

                oColumn = oColumns.Item("clCode");
                clPenaltyCode = oColumn;
                oColumn.DataBind.Bind("penltyRules", "RuleCode");

                oColumn = oColumns.Item("clFDate");
                clFDate = oColumn;
                oColumn.DataBind.Bind("penltyRules", "clFDate");

                oColumn = oColumns.Item("clTDate");
                clTDate = oColumn;
                oColumn.DataBind.Bind("penltyRules", "clTDate");

                oColumn = oColumns.Item("clday");
                clDays = oColumn;
                oColumn.DataBind.Bind("penltyRules", "Days");

                oColumn = oColumns.Item("clPen");
                clPenalty = oColumn;
                oColumn.DataBind.Bind("penltyRules", "Penalty");

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
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: FillDepartmentInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: FillDesignationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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

        private void FillEmpBranchInCombo()
        {
            try
            {
                var EmpLocation = from a in dbHrPayroll.MstBranches select a;
                cb_Branch.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstBranches empBranches in EmpLocation)
                {
                    cb_Branch.ValidValues.Add(Convert.ToString(empBranches.Id), Convert.ToString(empBranches.Name));
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: FillEmpBranchInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void FillEmpPayrollInCombo()
        {
            try
            {
                var EmpPayroll = from a in dbHrPayroll.CfgPayrollDefination select a;
                cb_payroll.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (CfgPayrollDefination empPayroll in EmpPayroll)
                {
                    cb_payroll.ValidValues.Add(Convert.ToString(empPayroll.ID), Convert.ToString(empPayroll.PayrollName));
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: FillEmpPayrollInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void FillPenaltyTypesInCombo()
        {
            try
            {
                var penaltyType = from a in dbHrPayroll.MstPenaltyRules select a;
                clPenaltyCode.ValidValues.Add("-1", "");
                foreach (MstPenaltyRules empPenaltyType in penaltyType)
                {
                    clPenaltyCode.ValidValues.Add(Convert.ToString(empPenaltyType.ID), Convert.ToString(empPenaltyType.Description));
                }
                clPenaltyCode.DisplayDesc = true;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: FillLeaveTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void PopulateGridWithFilterExpression()
        {
            Int16 i = 0;

            var Data = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID > 0).ToList();

            if (txtEmpId.Value != string.Empty)
            {
                int intEmpIdFrom = dbHrPayroll.MstEmployee.Where(emp => emp.EmpID == txtEmpId.Value).FirstOrDefault().ID;                
                Data = Data.Where(e => e.ID == intEmpIdFrom).ToList();
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
                dtPenalty.Rows.Clear();
                grdPenalty.LoadFromDataSource();
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
                    dtEmployees.SetValue("Branch", i, !String.IsNullOrEmpty(EMP.BranchName) ? EMP.BranchName.ToString() : "");
                    dtEmployees.SetValue("Branch", i, !String.IsNullOrEmpty(EMP.BranchName) ? EMP.BranchName.ToString() : "");
                    i++;
                }
                grdEmployees.LoadFromDataSource();
            }
            else
            {
                dtEmployees.Rows.Clear();
                grdEmployees.LoadFromDataSource();
                dtPenalty.Rows.Clear();
                grdPenalty.LoadFromDataSource();
            }
            //GetPealtyRulesRecords();
            addEmptyRow();
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
            }

        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
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
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
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
                    txtEmpId.Value = Program.EmpID;
                    LoadSelectedData(txtEmpId.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadSelectedData(String pCode)
        {

            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID.Contains(pCode)
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txtEmpId.Value = getEmp.EmpID;
                    }
                }                
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpPN Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetPealtyRulesRecords()
        {
            Int16 i = 0;
            try
            {
                dtPenalty.Rows.Add(1);
                dtPenalty.SetValue("No", i, i + 1);
                dtPenalty.SetValue("RuleCode", i, "-1");
                dtPenalty.SetValue("clFDate", i, "");
                dtPenalty.SetValue("clTDate", i, "");
                dtPenalty.SetValue("Days", i, "0");
                dtPenalty.SetValue("Penalty", i, "0");
                grdPenalty.LoadFromDataSource();
                //var AttRules = dbHrPayroll.MstPenaltyRules.ToList();
                //if (AttRules != null && AttRules.Count > 0)
                //{
                //    dtPenalty.Rows.Clear();
                //    dtPenalty.Rows.Add(AttRules.Count());
                //    foreach (var Rule in AttRules)
                //    {
                //        dtPenalty.SetValue("No", i, i + 1);
                //        dtPenalty.SetValue("RuleCode", i, Rule.Code);
                //        dtPenalty.SetValue("RuleDesc", i, Rule.Description);
                //        dtPenalty.SetValue("Days", i, Rule.Days);
                //        dtPenalty.SetValue("Penalty", i, Rule.PenaltyDays);
                //        i++;
                //    }
                //    grdPenalty.LoadFromDataSource();
                //}
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void addEmptyRow()
        {


            if (dtPenalty.Rows.Count == 0)
            {
                dtPenalty.Rows.Add(1);               
                dtPenalty.SetValue("No", 0,  1);
                dtPenalty.SetValue("RuleCode", 0, "-1");
                dtPenalty.SetValue("clFDate", 0, "");
                dtPenalty.SetValue("clTDate", 0, "");
                dtPenalty.SetValue("Days", 0, "0");
                dtPenalty.SetValue("Penalty", 0, "0");
                grdPenalty.AddRow(1, grdPenalty.RowCount + 1);
            }
            else
            {
                //dtPenalty.Rows.Add(1);
                //dtPenalty.SetValue("No", dtPenalty.Rows.Count - 1, dtPenalty.Rows.Count);
                //dtPenalty.SetValue("RuleCode", dtPenalty.Rows.Count - 1, "-1");
                //dtPenalty.SetValue("clFDate", dtPenalty.Rows.Count - 1, "");
                //dtPenalty.SetValue("clTDate", dtPenalty.Rows.Count - 1, "");
                //dtPenalty.SetValue("Days", dtPenalty.Rows.Count - 1, "0");
                //dtPenalty.SetValue("Penalty", dtPenalty.Rows.Count - 1, "0");
                //grdPenalty.AddRow(1, grdPenalty.RowCount + 1);
                if (dtPenalty.GetValue("RuleCode", dtPenalty.Rows.Count - 1) == "-1")
                {
                }
                else
                {
                    dtPenalty.Rows.Add(1);
                    dtPenalty.SetValue("No", dtPenalty.Rows.Count - 1, dtPenalty.Rows.Count);
                    dtPenalty.SetValue("RuleCode", dtPenalty.Rows.Count - 1, "-1");
                    dtPenalty.SetValue("clFDate", dtPenalty.Rows.Count - 1, "");
                    dtPenalty.SetValue("clTDate", dtPenalty.Rows.Count - 1, "");
                    dtPenalty.SetValue("Days", dtPenalty.Rows.Count - 1, "0");
                    dtPenalty.SetValue("Penalty", dtPenalty.Rows.Count - 1, "0");
                    grdPenalty.AddRow(1, grdPenalty.RowCount + 1);
                }

            }
            grdPenalty.LoadFromDataSource();
        }

        private void SaveRecords()
        {
            string strEMPcode = "";
            string strPenaltyCode = "";
            string strEvalDays = "";
            string strPenaltyDays = "";
            string strFromDate = "";
            DateTime FromDate = DateTime.MinValue;
            string strToDate = "";
            DateTime ToDate = DateTime.MinValue;
            //grdEmployees.FlushToDataSource();
            try
            {
                if (dtEmployees != null && dtEmployees.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {
                        bool sel2 = (grdEmployees.Columns.Item("isSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                        if (sel2)
                        {
                            strEMPcode = Convert.ToString(dtEmployees.GetValue("EmpCode", i));
                            var EmpDATA = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strEMPcode).FirstOrDefault();
                            if (EmpDATA == null)
                            {
                                oApplication.StatusBar.SetText("Employee Record(s) can't be found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                            for (int k = 0; k < dtPenalty.Rows.Count; k++)
                            {
                                strFromDate = (grdPenalty.Columns.Item("clFDate").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                                FromDate = DateTime.ParseExact(strFromDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                strToDate = (grdPenalty.Columns.Item("clTDate").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                                ToDate = DateTime.ParseExact(strToDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                strPenaltyCode = (grdPenalty.Columns.Item("clCode").Cells.Item(k + 1).Specific as SAPbouiCOM.ComboBox).Value;
                                strEvalDays = (grdPenalty.Columns.Item("clday").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;                            
                                strPenaltyDays = (grdPenalty.Columns.Item("clPen").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                                //strPenaltyDays = Convert.ToString(dtPenalty.GetValue("Penalty", k));
                                dtPenalty.SetValue("No", k, k + 1);
                                dtPenalty.SetValue("RuleCode", k, strPenaltyCode);
                                dtPenalty.SetValue("clFDate", k, FromDate);
                                dtPenalty.SetValue("clTDate", k, ToDate);
                                dtPenalty.SetValue("Days", k, strEvalDays);
                                dtPenalty.SetValue("Penalty", k, strPenaltyDays);
                                var EmpPenaltyMaster = dbHrPayroll.MstPenaltyRules.Where(c => c.ID == Convert.ToInt32(strPenaltyCode)).FirstOrDefault();
                                if (EmpPenaltyMaster != null)
                                {
                                    var EmpPenRecord = dbHrPayroll.TrnsEmployeePenalty.Where(p => p.PenaltyId == EmpPenaltyMaster.ID && p.EmpId == EmpDATA.ID).FirstOrDefault();
                                    if (EmpPenRecord != null)
                                    {
                                        EmpPenRecord.Days = Convert.ToInt32(strEvalDays);
                                        EmpPenRecord.PenaltyDays = Convert.ToInt32(strPenaltyDays);
                                        EmpPenRecord.FromDate = FromDate;
                                        EmpPenRecord.ToDate = ToDate;
                                        dbHrPayroll.SubmitChanges();
                                    }
                                    else
                                    {
                                        TrnsEmployeePenalty ObjPenalty = new TrnsEmployeePenalty();
                                        ObjPenalty.PenaltyId = Convert.ToInt32(strPenaltyCode);
                                        ObjPenalty.EmpId = EmpDATA.ID;
                                        ObjPenalty.FromDate = FromDate;
                                        ObjPenalty.ToDate = ToDate;
                                        ObjPenalty.Days = Convert.ToInt32(strEvalDays);
                                        ObjPenalty.PenaltyDays = Convert.ToInt32(strPenaltyDays);
                                      //  ObjPenalty.FlgActive = true;
                                        dbHrPayroll.TrnsEmployeePenalty.InsertOnSubmit(ObjPenalty);
                                        dbHrPayroll.SubmitChanges();
                                    }
                                }
                            }                            
                        }
                    }
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    addEmptyRow();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AttProcess Function: SaveRecords Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
