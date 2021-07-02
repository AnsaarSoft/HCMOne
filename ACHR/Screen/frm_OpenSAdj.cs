using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using System.Collections;

namespace ACHR.Screen
{
    class frm_OpenSAdj : HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.EditText txtEmployeeID, txtEmployeeName, txtDepartment, txtDesignation, txtBasicSalary, txtGrossSalary, txtOpeningSalary;
        SAPbouiCOM.Column LineID, cAmount, cDescription, cSelected;
        SAPbouiCOM.ComboBox cPeroidID;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix mtMain;
        SAPbouiCOM.Item itxtGrossSalary, ilblGrossSalary;


        SAPbouiCOM.Button btnOk, btnCancel;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, ID, Fucntions, Rights;
        Decimal empBasicSalary = 0;
        Decimal empGrossSalary = 0;
        Decimal TaxableValue = 0;
        Int32 salaryProcessID = 0;

        MstEmployee oEmployee = null;
        //TrnsSalaryProcessRegister reg = null;

        Int32 DocId = 0;
        Boolean flgAddSuccess = false;
        Boolean flgLoadedRecord = false;
        IEnumerable<MstEmployee> oEmployees = null;

        #endregion

        #region B1 Events
        
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
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
                    doSomething();
                    break;
                case "btPick":
                    doFind();
                    break;                
                case "btRem":
                    DeleteProcess();
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
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

        }
        #endregion

        #region Functions

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

                txtOpeningSalary = oForm.Items.Item("txOpen").Specific;
                oForm.DataSources.UserDataSources.Add("txOpen", SAPbouiCOM.BoDataType.dt_SUM);
                txtOpeningSalary.DataBind.SetBound(true, "", "txOpen");

                txtGrossSalary = oForm.Items.Item("txGS").Specific;
                itxtGrossSalary = oForm.Items.Item("txGS");
                oForm.DataSources.UserDataSources.Add("txGS", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossSalary.DataBind.SetBound(true, "", "txGS");
                //itxtGrossSalary.Visible = false;

                ilblGrossSalary = oForm.Items.Item("17");
                //ilblGrossSalary.Visible = false;


                mtMain = oForm.Items.Item("mtMain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtDetail");


                LineID = mtMain.Columns.Item("cllid");
                LineID.Visible = false;               

                cAmount = mtMain.Columns.Item("clamt");
                //cAmount.Width = 120;                

                cDescription = mtMain.Columns.Item("clDesc");
                //cDescription.Width = 120;
                //cDescription.Editable = false;

                cSelected = mtMain.Columns.Item("clSel");
                //cSelected.Width = 80;

                btnOk = oForm.Items.Item("1").Specific;
                GetDataFilterData();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
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
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : OpenNewSearchForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void doSomething()
        {
            try
            {
                //if (btnOk.Caption == "Add")
                //{
                //    int confirm = oApplication.MessageBox("Are you sure you want to post Bonus Adjustment? ", 2, "Yes", "No");
                //    if (confirm == 2) return;
                //    UpdateRecords();
                //}
                //if (btnOk.Caption == "Update")
                //{
                //    int confirm = oApplication.MessageBox("Are you sure you want to post Bonus Adjustment? ", 2, "Yes", "No");
                //    if (confirm == 2) return;
                //    UpdateRecords();

                //}
                UpdateRecords();
                FillRecord(Convert.ToInt32(oEmployee.ID));
                //addEmptyRowbyName();
            }
            catch (Exception Ex)
            {
            }
        }

        private void FillRecord(int intEmpId)
        {
            try
            {
                IEnumerable<TrnsObSalaryAdj> pTA = (from a in dbHrPayroll.TrnsObSalaryAdj where a.EmpId == intEmpId select a).ToList();
                if (pTA != null)
                {                    
                    //mtMain.FlushToDataSource();
                    dtMain.Rows.Clear();
                    Int32 RowCounts = 0;

                    foreach (TrnsObSalaryAdj OneRec in pTA)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(LineID.DataBind.Alias, RowCounts, Convert.ToString(OneRec.ID));
                        dtMain.SetValue(cDescription.DataBind.Alias, RowCounts, Convert.ToString(OneRec.Description));
                        dtMain.SetValue(cAmount.DataBind.Alias, RowCounts, Convert.ToString(OneRec.Amount));
                        if (OneRec.FlgActive != null && OneRec.FlgActive.Value == true)
                        {
                            dtMain.SetValue(cSelected.DataBind.Alias, RowCounts, "Y");
                        }
                        else
                        {
                            dtMain.SetValue(cSelected.DataBind.Alias, RowCounts, "N");
                        }
                        RowCounts++;
                    }
                    //foreach (TrnsQuarterTaxAdjDetail OneRec in pTA)
                    //{
                    //    dtMain.Rows.Add(1);
                    //    dtMain.SetValue(LineID.DataBind.Alias, RowCounts, Convert.ToString(OneRec.ID));
                    //    dtMain.SetValue(PeroidID.DataBind.Alias, RowCounts, Convert.ToString(OneRec.PayrollPeriodID));
                    //    dtMain.SetValue(cAmount.DataBind.Alias, RowCounts, Convert.ToString(OneRec.Amount));
                    //    dtMain.SetValue(cRemCur.DataBind.Alias, RowCounts, Convert.ToString(OneRec.RemaiCurnt));
                    //    dtMain.SetValue(cTaxableAmount.DataBind.Alias, RowCounts, Convert.ToString(OneRec.TaxableAmount));
                    //    RowCounts++;
                    //}
                    dtMain.Rows.Add(1);
                    dtMain.SetValue(LineID.DataBind.Alias, RowCounts, "-1");
                    dtMain.SetValue(cDescription.DataBind.Alias, RowCounts, "");
                    dtMain.SetValue(cAmount.DataBind.Alias, RowCounts, "0");
                    dtMain.SetValue(cSelected.DataBind.Alias, RowCounts, "N");            
                    
                    mtMain.LoadFromDataSource();
                    //DocId = pTA.ID;                      
                    flgLoadedRecord = true;
                    btnOk.Caption = "Update";
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void addEmptyRowbyName()
        {
            try
            {
                mtMain.FlushToDataSource();
                if (dtMain.Rows.Count == 0)
                {
                    dtMain.Rows.Add(1);
                    dtMain.SetValue("ID", 0, "-1");
                    dtMain.SetValue("amt", 0, "0");
                    dtMain.SetValue("Desc", 0, "");
                    mtMain.AddRow(1, mtMain.RowCount + 1);
                }
                else
                {
                    if (dtMain.GetValue("ID", dtMain.Rows.Count - 1) == "-1")
                    {
                    }
                    else
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue("ID", 0, "-1");
                        dtMain.SetValue("amt", 0, "0");
                        dtMain.SetValue("Desc", 0, "");
                        mtMain.AddRow(1, mtMain.RowCount + 1);
                    }

                }
                mtMain.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("addEmptyRowbyName error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ClearRecord()
        {
            try
            {
                Program.EmpID = "";
                txtEmployeeID.Value = "";
                txtEmployeeName.Value = "";
                txtDesignation.Value = "";
                txtDepartment.Value = "";

                txtBasicSalary.Value = "0";
                txtGrossSalary.Value = "0";


                DocId = 0;
                dtMain.Rows.Clear();
                mtMain.LoadFromDataSource();
                //addEmptyRowbyName();
            }
            catch (Exception Ex)
            {
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
                            txtOpeningSalary.Value = Convert.ToString(OpeninigSalary.FirstOrDefault().Amount);
                        }
                        //FillPeriodsInColumn(PeroidID, oEmp.PayrollID.Value);
                        //FillCurrRemInColumn(cRemCur);
                      

                        IEnumerable< TrnsObSalaryAdj> oTA = (from a in dbHrPayroll.TrnsObSalaryAdj where a.EmpId == oEmp.ID select a).ToList();
                        if (oTA != null)
                        {
                            FillRecord(Convert.ToInt32(oEmp.ID));
                            //addEmptyRowbyName();
                        }
                        else
                        {
                            //AddEmptyRow();
                            addEmptyRowbyName();
                            btnOk.Caption = "Add";
                        }
                    }

                    oApplication.StatusBar.SetText("Employee Set Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void UpdateRecords()
        {
            string strDetailLineID = string.Empty;
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                if (oEmp != null)
                {
                    for (int k = 0; k < mtMain.RowCount; k++)
                    {
                        TrnsObSalaryAdj objAdjustment = null;

                        string amount = "", Description = "";
                        bool isActive = false;
                        strDetailLineID = (mtMain.Columns.Item("cllid").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                        amount = (mtMain.Columns.Item("clamt").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                        Description = (mtMain.Columns.Item("clDesc").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                        isActive = (mtMain.Columns.Item("clSel").Cells.Item(k + 1).Specific as SAPbouiCOM.CheckBox).Checked;

                        objAdjustment = dbHrPayroll.TrnsObSalaryAdj.Where(r => r.ID.ToString() == strDetailLineID).FirstOrDefault();
                        if (objAdjustment == null && !string.IsNullOrEmpty(amount) && !string.IsNullOrEmpty(Description))
                        {
                            objAdjustment = new TrnsObSalaryAdj();
                            dbHrPayroll.TrnsObSalaryAdj.InsertOnSubmit(objAdjustment);
                            objAdjustment.EmpId = oEmp.ID;
                            objAdjustment.Amount = Convert.ToDecimal(amount);
                            objAdjustment.Description = Description;
                            objAdjustment.CreatedBy = oCompany.UserName;
                            objAdjustment.FlgActive = isActive;
                            objAdjustment.CreatedDate = DateTime.Now;
                        }
                        else if (objAdjustment != null && !string.IsNullOrEmpty(amount) && !string.IsNullOrEmpty(Description))
                        {
                            objAdjustment.Description = Description;
                            objAdjustment.Amount = Convert.ToDecimal(amount);
                            objAdjustment.FlgActive = isActive;
                            objAdjustment.UpdatedBy = oCompany.UserName;
                            objAdjustment.UpdatedDate = DateTime.Now;
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Records Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Record didn't updated. error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void DeleteProcess()
        {
            //try
            //{
            //    if (DeleteRecord())
            //    {
            //        ClearRecord();
            //    }
            //}
            //catch (Exception Ex)
            //{
            //}
        }

        private void DeleteProcessedRecord()
        {
            try
            {
                TrnsSalaryProcessRegister objReg = null;
                //objReg = dbHrPayroll.TrnsSalaryProcessRegister.Where(d => d.PayrollPeriodID == periodid & d.EmpID == empid).FirstOrDefault();
                objReg = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.Id == salaryProcessID select a).FirstOrDefault();
                IEnumerable<TrnsEmployeeElementDetail> nonRecuringElements = from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.MstEmployee.EmpID == objReg.MstEmployee.EmpID && p.PeriodId.ToString() == objReg.CfgPeriodDates.ID.ToString() select p;
                foreach (TrnsEmployeeElementDetail ele in nonRecuringElements)
                {
                    ele.FlgOneTimeConsumed = false;
                }
                if (objReg != null)
                {
                    dbHrPayroll.TrnsSalaryProcessRegister.DeleteOnSubmit(objReg);
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void GetDataFilterData()
        {
            try
            {
                CodeIndex.Clear();
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {



                    string strOut = string.Empty;
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                    strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                    //IEnumerable<MstEmployee> oEmployees =(from e in dbHrPayroll.MstEmployee where Convert.ToString(e.PayrollID) == strOut  select e);
                    oEmployees = (from e in dbHrPayroll.MstEmployee where Convert.ToString(e.PayrollID) == strOut select e);
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {
                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;

                }
                else
                {
                    oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {

                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;
                }

            }


            //    IEnumerable<MstEmployee> oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
            //    Int32 i = 0;
            //    foreach (MstEmployee oEmp in oEmployees)
            //    {
            //        CodeIndex.Add(oEmp.ID.ToString(), i);
            //        i++;
            //    }
            //    totalRecord = i;
            //}
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }
        #endregion

    }
}
