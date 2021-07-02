using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using System.Collections;

namespace ACHR.Screen
{
    class frm_SalAdj : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.EditText txtEmployeeID, txtEmployeeName, txtDepartment, txtDesignation, txtBasicSalary, txtGrossSalary, txOpS, txAmount, txDesc;
        SAPbouiCOM.CheckBox chkAct;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, Description, Amount, Active;     
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix grdOpenSalDetail;
        SAPbouiCOM.Item itxtGrossSalary, ilblGrossSalary;
        SAPbouiCOM.Button btnOk, btnCancel;
        Decimal empBasicSalary = 0;
        Decimal empGrossSalary = 0;
        MstEmployee oEmployee = null;

        #endregion


        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Initialize Form Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    SaveRecords();
                    break;
                case "btPick":
                    doFind();
                    break;               
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmployeeID == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmployeeID.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }
        }

        #endregion

        #region Helper Methods

        private void InitiallizeForm()
        {

            try
            {
                txtEmployeeID = oForm.Items.Item("txEmpid").Specific;
                oForm.DataSources.UserDataSources.Add("txEmpid", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmployeeID.DataBind.SetBound(true, "", "txEmpid");

                txtEmployeeName = oForm.Items.Item("txName").Specific;
                oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtEmployeeName.DataBind.SetBound(true, "", "txName");

                txtDepartment = oForm.Items.Item("txDept").Specific;
                oForm.DataSources.UserDataSources.Add("txDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtDepartment.DataBind.SetBound(true, "", "txDept");

                txtDesignation = oForm.Items.Item("txDesi").Specific;
                oForm.DataSources.UserDataSources.Add("txDesi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDesignation.DataBind.SetBound(true, "", "txDesi");

                txtBasicSalary = oForm.Items.Item("txBSal").Specific;
                oForm.DataSources.UserDataSources.Add("txBSal", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txBSal");

                txOpS = oForm.Items.Item("txOpS").Specific;
                oForm.DataSources.UserDataSources.Add("txOpS", SAPbouiCOM.BoDataType.dt_SUM);
                txOpS.DataBind.SetBound(true, "", "txOpS");

                txtGrossSalary = oForm.Items.Item("txGS").Specific;
                itxtGrossSalary = oForm.Items.Item("txGS");
                oForm.DataSources.UserDataSources.Add("txGS", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossSalary.DataBind.SetBound(true, "", "txGS");

                txAmount = oForm.Items.Item("txAmount").Specific;
                oForm.DataSources.UserDataSources.Add("txAmount", SAPbouiCOM.BoDataType.dt_SUM);
                txAmount.DataBind.SetBound(true, "", "txAmount");

                txDesc = oForm.Items.Item("txDesc").Specific;
                oForm.DataSources.UserDataSources.Add("txDesc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txDesc.DataBind.SetBound(true, "", "txDesc");
          

                chkAct = oForm.Items.Item("chkAct").Specific;
                oForm.DataSources.UserDataSources.Add("chkAct", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkAct.DataBind.SetBound(true, "", "chkAct");
                chkAct.Checked = false;


               
                btnOk = oForm.Items.Item("1").Specific;

                InitiallizegridMatrix();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtMain = oForm.DataSources.DataTables.Add("OpeningSalary");
                dtMain.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("Description", SAPbouiCOM.BoFieldsType.ft_Text);
                dtMain.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Text);
                dtMain.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text);     

                grdOpenSalDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("mtDetail").Specific;
                oColumns = (SAPbouiCOM.Columns)grdOpenSalDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("OpeningSalary", "No");

                oColumn = oColumns.Item("Desc");
                Description = oColumn;
                oColumn.DataBind.Bind("OpeningSalary", "Description");

                oColumn = oColumns.Item("Amount");
                Amount = oColumn;
                oColumn.DataBind.Bind("OpeningSalary", "Amount");


                oColumn = oColumns.Item("Active");
                Active = oColumn;
                oColumn.DataBind.Bind("OpeningSalary", "Active");


                

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void doFind()
        {
            OpenNewSearchForm();
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empMaster";
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
                oApplication.StatusBar.SetText("Function : OpenNewSearchForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {

                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == Program.EmpID select a).FirstOrDefault();
                    if (oEmp != null)
                    {
                        oEmployee = oEmp;
                        txtEmployeeID.Value = oEmp.EmpID;
                        txtEmployeeName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                        txtDepartment.Value = oEmp.DepartmentName;
                        txtDesignation.Value = oEmp.DesignationName;


                        txtBasicSalary.Value = Convert.ToString(oEmp.BasicSalary);
                        empBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                        empGrossSalary = ds.getEmpGross(oEmp);
                        txtGrossSalary.Value = Convert.ToString(empGrossSalary);

                        var OpeninigSalary = dbHrPayroll.TrnsOBSalary.Where(d => d.EmpID == oEmp.ID).GroupBy(d => d.EmpID).Select(a => new { Amount = a.Sum(b => b.SalaryBalance) }).OrderByDescending(a => a.Amount).ToList();
                        if (OpeninigSalary != null && OpeninigSalary.Count > 0)
                        {
                            txOpS.Value = Convert.ToString(OpeninigSalary.FirstOrDefault().Amount);
                        }

                        GetHistory(oEmp.ID);
                        //addEmptyRowbyName();
                        //TrnsQuarterTaxAdj oTA = (from a in dbHrPayroll.TrnsQuarterTaxAdj where a.EmpID == oEmp.ID select a).FirstOrDefault();
                        //if (oTA != null)
                        //{
                        //    FillRecord(Convert.ToInt32(oTA.ID));

                        //}
                        //else
                        //{
                        //    //AddEmptyRow();
                        //    addEmptyRowbyName();
                        //    btnOk.Caption = "Add";
                        //}
                    }

                    oApplication.StatusBar.SetText("Employee Set Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void addEmptyRowbyName()
        {
            
            if (dtMain.Rows.Count == 0)
            {
                dtMain.Rows.Add(1);
                dtMain.SetValue("No", 0, "1");
                dtMain.SetValue("Description", 0, "");
                dtMain.SetValue("Amount", 0, "0");
                dtMain.SetValue("Active", 0, "1");
                grdOpenSalDetail.AddRow(1, grdOpenSalDetail.RowCount + 1);
            }
            //else
            //{
            //    if (dtMain.GetValue("pid", dtMain.Rows.Count - 1) == "")
            //    {
            //    }
            //    else
            //    {
            //        dtMain.Rows.Add(1);
            //        dtMain.SetValue("pid", 0, "-1");
            //        dtMain.SetValue("amt", 0, "0");
            //        dtMain.SetValue("rem_cur", 0, "-1");
            //        grdOpenSalDetail.AddRow(1, grdOpenSalDetail.RowCount + 1);
            //    }

            //}
            grdOpenSalDetail.LoadFromDataSource();
        }

        private void GetHistory(int intEmpID)
        {            
            Int16 i = 0;
            try
            {
                var Data = dbHrPayroll.TrnsObSalaryAdj.Where(adv => adv.EmpId == intEmpID).ToList();                
                if (Data.Count == 0)
                {
                    dtMain.Rows.Clear();
                    grdOpenSalDetail.LoadFromDataSource();
                    return;
                }
                else if (Data != null && Data.Count > 0)
                {
                    decimal ReceiveAmount = 0;
                    dtMain.Rows.Clear();
                    dtMain.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        dtMain.SetValue("No", i, i + 1);
                        dtMain.SetValue("Description", i, WD.Description);
                        dtMain.SetValue("Amount", i, String.Format("{0:0.00}", WD.Amount));
                        dtMain.SetValue("Active", i, WD.FlgActive == null ? false : WD.FlgActive.Value);                       
                        i++;
                    }
                    grdOpenSalDetail.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }

        private void SaveRecords()
        {
            TrnsObSalaryAdj objAdjustment = null;
            MstEmployee emp=dbHrPayroll.MstEmployee.Where(e=>e.EmpID==txtEmployeeID.Value).FirstOrDefault();
            if(emp==null)
            {
                 oApplication.StatusBar.SetText("Error : Please Select Valid Employee ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            try
            {
                objAdjustment = dbHrPayroll.TrnsObSalaryAdj.Where(d => d.EmpId == emp.ID).FirstOrDefault();
                if (objAdjustment == null)
                {
                    objAdjustment = new TrnsObSalaryAdj();
                    dbHrPayroll.TrnsObSalaryAdj.InsertOnSubmit(objAdjustment);
                    objAdjustment.EmpId = emp.ID;
                    objAdjustment.CreatedBy = oCompany.UserName;
                    objAdjustment.CreatedDate = DateTime.Now;
                }
                objAdjustment.Description = txDesc.Value;
                objAdjustment.Amount = Convert.ToDecimal(txAmount.Value);
                objAdjustment.FlgActive = chkAct.Checked;

                dbHrPayroll.SubmitChanges();

                GetHistory(emp.ID);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        #endregion

    }
}
