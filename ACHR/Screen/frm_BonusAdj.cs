using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using System.Collections;

namespace ACHR.Screen
{
    class frm_BonusAdj : HRMSBaseForm
    {
       
        #region Variable

        SAPbouiCOM.EditText txtEmployeeID,txtEmployeeName, txtDepartment, txtDesignation,txtBasicSalary, txtGrossSalary;
        SAPbouiCOM.Column LineID, PeroidID,cAmount, cRemCur, cTaxableAmount, cSelected,cname,cdesig,cdept,cbsal,cgsal;
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
                    //doSomething();
                    break;
                case "btPick":
                    doFind();
                    break;
                case "btCal":
                    CalculateTax();
                    break;
                case "btRem":
                    DeleteProcess();
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    doSomething();
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

                txtDepartment  = oForm.Items.Item("txDept").Specific;
                oForm.DataSources.UserDataSources.Add("txDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtDepartment .DataBind.SetBound(true, "", "txDept");

                txtDesignation = oForm.Items.Item("txDesi").Specific;
                oForm.DataSources.UserDataSources.Add("txDesi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDesignation.DataBind.SetBound(true, "", "txDesi");

                txtBasicSalary = oForm.Items.Item("txBSal").Specific;
                oForm.DataSources.UserDataSources.Add("txBSal", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txBSal");

                txtGrossSalary = oForm.Items.Item("txGS").Specific;
                itxtGrossSalary = oForm.Items.Item("txGS");
                oForm.DataSources.UserDataSources.Add("txGS", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossSalary.DataBind.SetBound(true, "", "txGS");
                itxtGrossSalary.Visible = false;

                ilblGrossSalary = oForm.Items.Item("17");
                ilblGrossSalary.Visible = false;


                mtMain = oForm.Items.Item("mtMain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtDetail");


                LineID = mtMain.Columns.Item("cllid");
                LineID.Visible = false;
                PeroidID = mtMain.Columns.Item("clpid");

                cAmount = mtMain.Columns.Item("clamt");
                cAmount.Width = 120;
                cRemCur = mtMain.Columns.Item("clRmcr");
                cRemCur.Width = 120;
                cRemCur.Visible = true;
                cTaxableAmount = mtMain.Columns.Item("clTax");
                cTaxableAmount.Width = 120;
                cTaxableAmount.Editable = false;
                cSelected = mtMain.Columns.Item("clSel");
                cSelected.Width = 80;
                
                btnOk = oForm.Items.Item("1").Specific;
                //ibtnOk = oForm.Items.Item("btProcess");
                //btnCancel = oForm.Items.Item("2").Specific;

                //GetData();

                //InitiallizegridMatrix();

                GetDataFilterData();

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
                dtMain = oForm.DataSources.DataTables.Add("dtDetail");
                
                mtMain = (SAPbouiCOM.Matrix)oForm.Items.Item("mtMain").Specific;
                oColumns = (SAPbouiCOM.Columns)mtMain.Columns;

                
                oColumn = oColumns.Item("clpid");
                PeroidID = oColumn;
                oColumn.DataBind.Bind("dtDetail", "pid");

                oColumn = oColumns.Item("clamt");
                cAmount  = oColumn;
                oColumn.DataBind.Bind("dtDetail", "amt");

                oColumn = oColumns.Item("clRmcr");
                cRemCur  = oColumn;
                oColumn.DataBind.Bind("dtDetail", "rem_cur");

                

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillCurrRemInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
           {                
                OneColumn.ValidValues.Add("-1", "");
                OneColumn.ValidValues.Add("1", "Current");
                OneColumn.ValidValues.Add("2", "Remaining");               
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        
        private void FillPeriodsInColumn(SAPbouiCOM.Column OneColumn, int intpayrollId)
        {
            try
            {
                IEnumerable<CfgPeriodDates> objPerioddates = dbHrPayroll.CfgPeriodDates.Where(x => x.PayrollId == intpayrollId & x.FlgLocked==false).ToList();
               // IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (CfgPeriodDates singlePeriod in objPerioddates)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(singlePeriod.ID), Convert.ToString(singlePeriod.PeriodName));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
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
                if (btnOk.Caption == "Add")
                {
                    int confirm = oApplication.MessageBox("Are you sure you want to post Bonus Adjustment? ", 2, "Yes", "No");
                    if (confirm == 2) return;
                    if (AddRecord())
                    {
                       // ClearRecord();
                    }
                }
                if (btnOk.Caption == "Update")
                {
                    int confirm = oApplication.MessageBox("Are you sure you want to post Bonus Adjustment? ", 2, "Yes", "No");
                    if (confirm == 2) return;
                    //if (UpdateRecordsZee())
                    //{                        
                     
                    //}                   
                    UpdateRecords();
                   
                }
               
            }
            catch (Exception Ex)
            {
            }
        }
        
        private void FillRecord(Int32 pdocid)
        {
            try
            {
                TrnsQuarterTaxAdj pTA = (from a in dbHrPayroll.TrnsQuarterTaxAdj where a.ID == pdocid select a).FirstOrDefault();
                if (pTA != null)
                {
                    //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.KeepChanges, pTA);
                    mtMain.FlushToDataSource();
                    dtMain.Rows.Clear();
                    Int32 RowCounts = 0;

                    foreach (TrnsQuarterTaxAdjDetail  OneRec in pTA.TrnsQuarterTaxAdjDetail)
                    {
                        
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(LineID.DataBind.Alias, RowCounts, Convert.ToString(OneRec.ID));
                        dtMain.SetValue(PeroidID.DataBind.Alias, RowCounts, Convert.ToString(OneRec.PayrollPeriodID));
                        dtMain.SetValue(cAmount.DataBind.Alias, RowCounts, Convert.ToString(OneRec.Amount));
                        dtMain.SetValue(cRemCur.DataBind.Alias,RowCounts,Convert.ToString( OneRec.RemaiCurnt));
                        dtMain.SetValue(cTaxableAmount.DataBind.Alias, RowCounts, Convert.ToString(OneRec.TaxableAmount));
                        RowCounts++;
                    }
                    dtMain.Rows.Add(1);
                    dtMain.SetValue(PeroidID.DataBind.Alias, RowCounts, "-1");
                    dtMain.SetValue(cAmount.DataBind.Alias, RowCounts, "0");
                    dtMain.SetValue(cRemCur.DataBind.Alias, RowCounts, "-1");

                    //addEmptyRowbyName();
                    //mtMain.LoadFromDataSourceEx(true);
                    mtMain.LoadFromDataSource();
                    DocId = pTA.ID;
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

            //mtMain.FlushToDataSource();
            if (dtMain.Rows.Count == 0)
            {
                dtMain.Rows.Add(1);
                dtMain.SetValue("pid", 0, "-1");
                dtMain.SetValue("amt", 0, "0");
                dtMain.SetValue("rem_cur", 0, "-1");
                mtMain.AddRow(1, mtMain.RowCount + 1);
            }
            else
            {
                if (dtMain.GetValue("pid", dtMain.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtMain.Rows.Add(1);
                    dtMain.SetValue("pid", 0, "-1");
                    dtMain.SetValue("amt", 0, "0");
                    dtMain.SetValue("rem_cur", 0, "-1");
                    mtMain.AddRow(1, mtMain.RowCount + 1);
                }

            }
            mtMain.LoadFromDataSource();
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
                        FillPeriodsInColumn(PeroidID, oEmp.PayrollID.Value);
                        FillCurrRemInColumn(cRemCur);


                        TrnsQuarterTaxAdj oTA = (from a in dbHrPayroll.TrnsQuarterTaxAdj where a.EmpID == oEmp.ID select a).FirstOrDefault();
                        if (oTA != null)
                        {
                            FillRecord(Convert.ToInt32(oTA.ID));

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

        private bool DeleteRecord()
        {
            bool retValue = false;
            try
            {
                if (!string.IsNullOrEmpty(txtEmployeeID.Value))
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                       
                    TrnsQuarterTaxAdj oTA = (from a in dbHrPayroll.TrnsQuarterTaxAdj where a.EmpID == oEmp.ID select a).FirstOrDefault();               

                    if (oTA != null)
                    {
                        for (Int16 i = 0; i < dtMain.Rows.Count; i++)
                        {
                            if (String.IsNullOrEmpty(dtMain.GetValue(PeroidID.DataBind.Alias, i)))
                            {
                                continue;
                            }
                            else
                            {
                                Int32 PeriodID = 0;
                                Decimal Amount = 0;
                                PeriodID = Convert.ToInt32(dtMain.GetValue(PeroidID.DataBind.Alias, i));
                                Amount = Convert.ToDecimal(dtMain.GetValue(cAmount.DataBind.Alias, i));
                                
                                Boolean chkCheck = (mtMain.Columns.Item("clSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                                if (PeriodID != 0 && PeriodID != -1 && chkCheck)
                                {
                                    Int32 recCount = 0;
                                    recCount = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.EmpID == oTA.EmpID && a.PayrollPeriodID == PeriodID && (a.JENum == null ? 0 : Convert.ToInt32(a.JENum)) > 0 select a).Count();
                                    if (recCount == 0)
                                    {
                                        Int32 recProcessCount = 0;
                                        recProcessCount = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.EmpID == oTA.EmpID && a.PayrollPeriodID == PeriodID select a).Count();
                                        if (recProcessCount == 0)
                                        {
                                            TrnsQuarterTaxAdjDetail oDocLine = (from a in dbHrPayroll.TrnsQuarterTaxAdjDetail where a.PayrollPeriodID == PeriodID && a.QTAID == oTA.ID select a).FirstOrDefault();
                                        
                                            //oTA.TrnsQuarterTaxAdjDetail.Remove(oDocLine);
                                            dbHrPayroll.TrnsQuarterTaxAdjDetail.DeleteOnSubmit(oDocLine);
                                            dbHrPayroll.SubmitChanges();
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("Can't delete void process salary.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Can't delete Salary already posted.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                
                            }
                        }
                    }
                }
                retValue = true;
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }
        
        private Boolean AddRecord()
        {
            Boolean flgReturn = true;
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                if (oEmp != null)
                {
                    TrnsQuarterTaxAdj oTA = new TrnsQuarterTaxAdj();


                    oTA.EmpID = Convert.ToInt32(oEmp.ID); 
                    oTA.CreatedBy = oCompany.UserName;
                    oTA.CreateDt = DateTime.Now;
                    //oTA.UpdatedBy = oCompany.UserName;
                    //oTA.UpdateDt = DateTime.Now;

                    dtMain.Rows.Clear();
                    mtMain.FlushToDataSource();
                    for (Int16 i = 0; i < dtMain.Rows.Count; i++)
                    {
                        if (String.IsNullOrEmpty(dtMain.GetValue(PeroidID .DataBind.Alias, i)))
                        {
                            continue;
                        }
                        else
                        {

                            Int32  Payrollid = 0;
                            Decimal Amount = 0;
                            String Rmcr = "";
                          
                            TrnsQuarterTaxAdjDetail  oDetail = new TrnsQuarterTaxAdjDetail ();

                            Payrollid = Convert.ToInt32 (dtMain.GetValue(PeroidID.DataBind.Alias, i));                           
                            Amount = Convert.ToDecimal(dtMain.GetValue(cAmount.DataBind.Alias, i));
                            Rmcr = Convert.ToString(dtMain.GetValue(cRemCur.DataBind.Alias, i));
                            Rmcr = "1";
                            oDetail.PayrollPeriodID  = Payrollid;
                            oDetail.RemaiCurnt = Rmcr;
                            oDetail.Amount = Amount;
                            oDetail.CreatedBy = oCompany.UserName;
                            oDetail.UpdatedBy = oCompany.UserName;
                            oDetail.CreateDt = DateTime.Now;
                            oDetail.UpdateDt = DateTime.Now;
                            oTA.TrnsQuarterTaxAdjDetail.Add(oDetail);
                        }
                    }
                    dbHrPayroll.TrnsQuarterTaxAdj.InsertOnSubmit(oTA);   
                    //dbHrPayroll.TrnsTaxAdjustment.InsertOnSubmit(oTA);
                    dbHrPayroll.SubmitChanges();
                    
                    oApplication.StatusBar.SetText("Record Added Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    flgReturn = false;
                    oApplication.StatusBar.SetText("Employee not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Record didn't added. error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                flgReturn = false;
            }
            return flgReturn;
        }
        
        private void UpdateRecords()
        {

            //reg = null;
            string strPayrollId = string.Empty;
            string strDetailLineID = string.Empty;
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                if (oEmp != null)
                {
                    
                    TrnsQuarterTaxAdj oTA = (from a in dbHrPayroll.TrnsQuarterTaxAdj where a.EmpID == oEmp.ID select a).FirstOrDefault();
                    //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.KeepChanges, oTA);
                    //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.KeepCurrentValues, oTA);
                    //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, oTA);
                    if (oTA != null)
                    {
                        for (int k = 0; k < mtMain.RowCount ; k++)
                        {
                            string amount = "", taxvalue = "", strCurrRem = "";
                            strDetailLineID = (mtMain.Columns.Item("cllid").Cells.Item(k + 1).Specific as SAPbouiCOM.ComboBox).Value;
                            strPayrollId = (mtMain.Columns.Item("clpid").Cells.Item(k+1).Specific as SAPbouiCOM.ComboBox).Value;
                            amount = (mtMain.Columns.Item("clamt").Cells.Item(k+1).Specific as SAPbouiCOM.EditText).Value;
                            taxvalue = (mtMain.Columns.Item("clTax").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                            strCurrRem = (mtMain.Columns.Item("clRmcr").Cells.Item(k + 1).Specific as SAPbouiCOM.EditText).Value;
                            //var OTADetails = dbHrPayroll.TrnsQuarterTaxAdjDetail.Where(d => d.QTAID == oTA.ID && d.PayrollPeriodID.ToString() == strPayrollId).FirstOrDefault();
                            TrnsQuarterTaxAdjDetail OTADetails = (from a in dbHrPayroll.TrnsQuarterTaxAdjDetail where a.QTAID == oTA.ID && a.PayrollPeriodID.ToString() == strPayrollId.Trim() && a.ID.ToString() == strDetailLineID.Trim() select a).FirstOrDefault();
                            if (strPayrollId != "-1")
                            {

                                if (OTADetails == null)
                                {
                                    OTADetails = new TrnsQuarterTaxAdjDetail();
                                    oTA.TrnsQuarterTaxAdjDetail.Add(OTADetails);

                                    OTADetails.PayrollPeriodID = Convert.ToInt32(strPayrollId);
                                    OTADetails.Amount = Convert.ToDecimal(amount);
                                    OTADetails.RemaiCurnt = Convert.ToString(strCurrRem);
                                    OTADetails.RemaiCurnt = "1";
                                    OTADetails.CreatedBy = oCompany.UserName;
                                    OTADetails.UpdatedBy = oCompany.UserName;
                                    OTADetails.UpdateDt = DateTime.Now;
                                    //OTADetails.CreateDt = DateTime.Now;
                                } 
                                else
                                {

                                    //OTADetails.PayrollPeriodID = Convert.ToInt32(strPayrollId);
                                    OTADetails.Amount = Convert.ToDecimal(amount);
                                    OTADetails.RemaiCurnt = Convert.ToString(strCurrRem);
                                    OTADetails.TaxableAmount = string.IsNullOrEmpty(taxvalue) ? 0 : Convert.ToDecimal(taxvalue);
                                    //OTADetails.RemaiCurnt = "1";
                                    OTADetails.UpdatedBy = oCompany.UserName;
                                    OTADetails.UpdateDt = DateTime.Now;
                                }
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        //DeleteProcessedRecord(oEmp.ID, Convert.ToInt32(strPayrollId));
                        oApplication.StatusBar.SetText("Record Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        ClearRecord();
                        btnOk.Caption = "Ok";
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Record didn't updated. error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void CalculateTax()
        {
            try
            {
                MstEmployee oemp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeID.Value.Trim() select a).FirstOrDefault();
                CfgPeriodDates operiod = null;
                for (Int32 i = 0; i < dtMain.Rows.Count; i++)
                {
                    //string value = Convert.ToString(dtMain.GetValue(cSelected.DataBind.Alias, i));
                    Boolean value = (mtMain.Columns.Item("clSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                    //if (value.Trim() == "Y")
                    if (value)
                    {
                        //string periodid = Convert.ToString(dtMain.GetValue(cPeroidID.DataBind.Alias, i));
                        string periodid = (mtMain.Columns.Item("clpid").Cells.Item(i + 1).Specific as SAPbouiCOM.ComboBox).Value.Trim();
                        operiod = (from a in dbHrPayroll.CfgPeriodDates where a.ID.ToString() == periodid select a).FirstOrDefault();
                    }
                }
               
                if(oemp!=null && operiod!=null)
                
                {
                    ProcessSalary(Convert.ToInt16(oemp.ID),Convert.ToInt32(operiod.ID));
                    DeleteProcessedRecord();
                    //ProcessSalaryZeeshan(Convert.ToInt16(oemp.ID), Convert.ToInt32(operiod.ID));
                }
                for (Int32 i = 0; i < dtMain.Rows.Count; i++)
                {
                    //string value = Convert.ToString(dtMain.GetValue(cSelected.DataBind.Alias, i));
                    Boolean value = (mtMain.Columns.Item("clSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                    //if (value.Trim() == "Y")
                    if (value)
                    {
                        //string periodid = Convert.ToString(dtMain.GetValue(cPeroidID.DataBind.Alias, i));
                        (mtMain.Columns.Item("clTax").Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value = Convert.ToString(TaxableValue);
                        
                    }
                }
                oApplication.StatusBar.SetText("Calculated Tax Successfully.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //SetEmpValues();
               
            }
            catch (Exception Ex)
            {
            }
        }

        private void ProcessSalary(Int32 pempid, Int32 pperiodid )
        {
            string strProcessing = "";
            try
            {
                MstEmployee emp = (from a in dbHrPayroll.MstEmployee where a.ID == pempid select a).FirstOrDefault();
                CfgPeriodDates payrollperiod = (from a in dbHrPayroll.CfgPeriodDates where a.ID == pperiodid select a).FirstOrDefault();
                Hashtable elementGls = new Hashtable();
                CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == emp.PayrollID.ToString() select p).Single();
                //CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).Single();
                int periodDays = 0;
                periodDays = Convert.ToInt16(payroll.WorkDays);
                decimal empBasicSalary = 0;
                decimal empGrossSalary = 0;
                try
                {
                    #region Start Here

                    decimal amnt = 0.0M;
                    
                        //MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID.ToString() == empid select p).FirstOrDefault();
                        MstGLDetermination glDetr = ds.getEmpGl(emp);
                        if (glDetr == null)
                        {
                            oApplication.StatusBar.SetText("GL determination not set", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
                        empBasicSalary = (decimal)emp.BasicSalary;
                        decimal spTaxbleAmnt = 0.00M;
                        decimal spTaxableAmntOT = 0.00M;
                        decimal spTaxableAmntLWOP = 0.00M;
                        decimal DaysCnt = 0;
                        decimal payDays = 0.00M;
                        decimal leaveDays = 0.00M;
                        decimal monthDays = 0.00M;
                        decimal nonRecurringTaxable = 0.00M;
                        decimal payRatio = 1.00M;
                        decimal payRatioWithLeaves = 1.00M;

                        //**********************************
                        Int32 MonthHour = 0;
                        Int32 TotalMinutes = 0;
                        Int32 PresentMinutes = 0;
                        Int32 OTMinutes = 0;
                        decimal LeaveMinutesTotal = 0;
                        decimal AllowanceTriggerValue = 18 * 60;
                        //**********************************

                        DaysCnt = ds.getDaysCnt(emp, payrollperiod, out payDays, out leaveDays, out monthDays);
                        decimal employeeRemainingSalary = 0.00M;
                        payRatio = payDays / monthDays;
                        payRatioWithLeaves = (payDays - leaveDays) / monthDays;

                        if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
                        {
                            empGrossSalary = ds.getEmpGross(emp, payrollperiod.ID);
                            empBasicSalary = empGrossSalary;
                        }
                        else
                        {
                            empGrossSalary = ds.getEmpGross(emp);
                        }
                        

                        try
                        {
                            employeeRemainingSalary = Math.Round((decimal)empBasicSalary * payRatio, 0);
                        }
                        catch { }
                        TrnsSalaryProcessRegister reg = new TrnsSalaryProcessRegister();
                        reg = new TrnsSalaryProcessRegister();
                        reg.MstEmployee = emp;
                        reg.CfgPayrollDefination = payroll;
                        reg.CfgPeriodDates = payrollperiod;
                        reg.EmpBasic = employeeRemainingSalary;//Math.Round(Convert.ToDecimal(empBasicSalary * payRatio), 0);
                        reg.EmpGross = empGrossSalary;
                        reg.CreateDate = DateTime.Now;
                        reg.UpdateDate = DateTime.Now;
                        reg.UserId = oCompany.UserName;
                        reg.UpdatedBy = oCompany.UserName;
                        reg.PeriodName = payrollperiod.PeriodName;
                        reg.PayrollName = payroll.PayrollName;
                        reg.EmpName = emp.FirstName + " " + emp.LastName;
                        //reg.DaysPaid = Convert.ToInt16( payDays);
                        reg.DaysPaid = Convert.ToDecimal(DaysCnt);
                        reg.MonthDays = Convert.ToInt32(monthDays);

                        /// Basic Salary ////
                        /// ************////
                        TrnsSalaryProcessRegisterDetail spdHeadRow = new TrnsSalaryProcessRegisterDetail();
                        spdHeadRow.LineType = "BS";
                        spdHeadRow.LineSubType = "Basic Salary";
                        spdHeadRow.LineValue = Math.Round(employeeRemainingSalary, 0);
                        spdHeadRow.LineMemo = "Basic Salary ";
                        spdHeadRow.DebitAccount = glDetr.BasicSalary;
                        spdHeadRow.CreditAccount = glDetr.BSPayable;
                        spdHeadRow.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
                        spdHeadRow.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
                        spdHeadRow.LineBaseEntry = emp.ID;
                        spdHeadRow.BaseValueCalculatedOn = employeeRemainingSalary;
                        spdHeadRow.BaseValue = employeeRemainingSalary;
                        spdHeadRow.BaseValueType = "FIX";
                        spdHeadRow.CreateDate = DateTime.Now;
                        spdHeadRow.UpdateDate = DateTime.Now;
                        spdHeadRow.UserId = oCompany.UserName;
                        spdHeadRow.UpdatedBy = oCompany.UserName;
                        spdHeadRow.NoOfDay = Convert.ToDecimal(DaysCnt);
                        spdHeadRow.TaxableAmount = employeeRemainingSalary;
                        spTaxbleAmnt += employeeRemainingSalary;
                        employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
                        reg.TrnsSalaryProcessRegisterDetail.Add(spdHeadRow);





                        //* AbsentDeductions,Reimbursement

                        //////Absents ////
                        //**************////
                        decimal leaveCnt = 0.00M;
                        //DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt);

                        DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt, glDetr);
                        foreach (DataRow dr in dtAbsentDeduction.Rows)
                        {
                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            spdetail.LineType = dr["LineType"].ToString();
                            spdetail.LineSubType = dr["LineSubType"].ToString();
                            spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            spdetail.LineMemo = dr["LineMemo"].ToString();
                            spdetail.DebitAccount = dr["DebitAccount"].ToString();
                            spdetail.CreditAccount = dr["CreditAccount"].ToString();
                            spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                            spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                            spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                            spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                            spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                            spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                            spdetail.CreateDate = DateTime.Now;
                            spdetail.UpdateDate = DateTime.Now;
                            spdetail.UserId = oCompany.UserName;
                            spdetail.UpdatedBy = oCompany.UserName;
                            spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                            spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                            employeeRemainingSalary += (decimal)spdetail.LineValue;
                            spTaxableAmntLWOP += Convert.ToDecimal(dr["TaxbleAmnt"]);
                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                        }

                        //DaysCnt -= leaveCnt;
                        //* End of Leave Deductions


                        //* Payroll elements assigned to employee ***Employee Elements ****** 
                        //*******************************************************************

                        DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, payrollperiod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays);
                        foreach (DataRow dr in dtSalPrlElements.Rows)
                        {
                            if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }
                        }
                        //******************** End of Elements *********************************



                        //////Over time ////
                        //**************////
                        
                        DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, payrollperiod, empGrossSalary,out OTMinutes);

                        //Code modified by Zeeshan

                        foreach (DataRow dr in dtSalOverTimes.Rows)
                        {
                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            spdetail.LineType = dr["LineType"].ToString();
                            spdetail.LineSubType = dr["LineSubType"].ToString();
                            spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            spdetail.LineMemo = dr["LineMemo"].ToString();
                            spdetail.DebitAccount = dr["DebitAccount"].ToString();
                            spdetail.CreditAccount = dr["CreditAccount"].ToString();
                            spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                            spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                            spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                            spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                            spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                            spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                            spdetail.CreateDate = DateTime.Now;
                            spdetail.UpdateDate = DateTime.Now;
                            spdetail.UserId = oCompany.UserName;
                            spdetail.UpdatedBy = oCompany.UserName;
                            spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                            spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                            spTaxableAmntOT += Convert.ToDecimal(dr["TaxbleAmnt"]);
                            employeeRemainingSalary += (decimal)spdetail.LineValue;
                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                        }



                        // * ************Advance Recovery Processing **************
                        //*******************************************************
                        DataTable dtAdvance = ds.salaryProcessingAdvance(emp, employeeRemainingSalary, payrollperiod);

                        foreach (DataRow dr in dtAdvance.Rows)
                        {
                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            spdetail.LineType = dr["LineType"].ToString();
                            spdetail.LineSubType = dr["LineSubType"].ToString();
                            spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            spdetail.LineMemo = dr["LineMemo"].ToString();
                            spdetail.DebitAccount = dr["DebitAccount"].ToString();
                            spdetail.CreditAccount = dr["CreditAccount"].ToString();
                            spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                            spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                            spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                            spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                            spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                            spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                            spdetail.CreateDate = DateTime.Now;
                            spdetail.UpdateDate = DateTime.Now;
                            spdetail.UserId = oCompany.UserName;
                            spdetail.UpdatedBy = oCompany.UserName;
                            spdetail.TaxableAmount = 0.00M;
                            employeeRemainingSalary += (decimal)spdetail.LineValue;


                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                        }



                        // * ************Loan Recovery Processing **************

                        DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary, payrollperiod);

                        foreach (DataRow dr in dtLoands.Rows)
                        {
                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            spdetail.LineType = dr["LineType"].ToString();
                            spdetail.LineSubType = dr["LineSubType"].ToString();
                            spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            spdetail.LineMemo = dr["LineMemo"].ToString();
                            spdetail.DebitAccount = dr["DebitAccount"].ToString();
                            spdetail.CreditAccount = dr["CreditAccount"].ToString();
                            spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                            spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                            spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                            spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                            spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                            spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                            spdetail.CreateDate = DateTime.Now;
                            spdetail.UpdateDate = DateTime.Now;
                            spdetail.UserId = oCompany.UserName;
                            spdetail.UpdatedBy = oCompany.UserName;
                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                            spdetail.TaxableAmount = 0.00M;
                            employeeRemainingSalary += (decimal)spdetail.LineValue;
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                        }

                        reg.EmpTaxblTotal = spTaxbleAmnt;
                        // * ************TAX**************
                        Decimal QuaterlyTaxValueReturn = 0.0M;
                        if (Program.systemInfo.TaxSetup == true && emp.FlgTax == true)
                        {
                            //decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable);
                            decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, payRatio, out QuaterlyTaxValueReturn);
                            if (TotalTax >= 0)
                            {
                                reg.EmpTotalTax = TotalTax;

                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = "Tax";
                                spdetail.LineSubType = "Tax";
                                spdetail.LineValue = -Math.Round(TotalTax, 0);
                                spdetail.LineMemo = "Tax Deduction";
                                spdetail.DebitAccount = glDetr.IncomeTaxExpense;
                                spdetail.CreditAccount = glDetr.IncomeTaxPayable;
                                spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
                                spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
                                spdetail.LineBaseEntry = 0;
                                spdetail.BaseValueCalculatedOn = spTaxbleAmnt;
                                spdetail.BaseValue = spTaxbleAmnt;
                                spdetail.BaseValueType = "FIX";
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                spdetail.TaxableAmount = 0.00M;
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }
                        }
                        spTaxbleAmnt = spTaxbleAmnt + QuaterlyTaxValueReturn;
                        reg.EmpTaxblTotal = spTaxbleAmnt;
                        TaxableValue = QuaterlyTaxValueReturn;
                        //************************************************
                        //********** Gratuity Calculations ***************

                        if (emp.CfgPayrollDefination.FlgGratuity == true)
                        {
                            int gratCnt = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).Count();
                            if (gratCnt > 0)
                            {
                                MstGratuity empGrat = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).FirstOrDefault();

                                try
                                {
                                    int FromYr = Convert.ToInt16(empGrat.YearFrom) * 365;

                                    if ((Convert.ToDateTime(payrollperiod.StartDate) - Convert.ToDateTime(emp.JoiningDate)).Days > FromYr)
                                    {
                                        decimal gratProvision = 0.00M;
                                        decimal basedOnAmont = 0.00M;
                                        if (empGrat.BasedOn == "0")
                                        {
                                            basedOnAmont = empBasicSalary;
                                        }
                                        else
                                        {
                                            basedOnAmont = empGrossSalary;
                                        }

                                        gratProvision = (basedOnAmont * (decimal)empGrat.Factor / 100) / 12;
                                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                        spdetail.LineType = "Element";
                                        spdetail.LineSubType = "Empr Cont";
                                        spdetail.LineValue = Math.Round(gratProvision, 0);
                                        spdetail.LineMemo = "Gratuity";
                                        spdetail.DebitAccount = glDetr.GratuityExpense;
                                        spdetail.CreditAccount = glDetr.GratuityPayable;
                                        spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityExpense);
                                        spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityPayable);
                                        spdetail.LineBaseEntry = 0;
                                        spdetail.BaseValueCalculatedOn = empBasicSalary;
                                        spdetail.BaseValue = empBasicSalary;
                                        spdetail.BaseValueType = "FIX";
                                        spdetail.CreateDate = DateTime.Now;
                                        spdetail.UpdateDate = DateTime.Now;
                                        spdetail.UserId = oCompany.UserName;
                                        spdetail.UpdatedBy = oCompany.UserName;
                                        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                        spdetail.TaxableAmount = 0.00M;
                                        reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        reg.SalaryStatus = 0;//Salary Processed
                        if (emp.PaymentMode == "HOLD")
                        {
                            reg.FlgHoldPayment = true;
                        }
                        
                        dbHrPayroll.TrnsSalaryProcessRegister.InsertOnSubmit(reg);
                        dbHrPayroll.SubmitChanges();
                        salaryProcessID = reg.Id;
                    
                    #endregion
                }
                catch (Exception ex)
                {
                    oApplication.SetStatusBarMessage(strProcessing + ":" + ex.Message);
                }
                
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }

        //private void ProcessSalaryZeeshan(Int32 pempid, Int32 pperiodid)
        //{
        //    string strProcessing = "";
        //    try
        //    {
        //        TrnsSalaryProcessRegisterDetail spdHeadRow = null;
        //        MstEmployee emp = (from a in dbHrPayroll.MstEmployee where a.ID == pempid select a).FirstOrDefault();
        //        CfgPeriodDates payrollperiod = (from a in dbHrPayroll.CfgPeriodDates where a.ID == pperiodid select a).FirstOrDefault();
        //        Hashtable elementGls = new Hashtable();
        //        CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == emp.PayrollID.ToString() select p).Single();
        //        //CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).Single();
        //        int periodDays = 0;
        //        periodDays = Convert.ToInt16(payroll.WorkDays);
        //        decimal empBasicSalary = 0;
        //        decimal empGrossSalary = 0;
        //        try
        //        {
        //            #region Start
        //            decimal amnt = 0.0M;
        //            if (true)
        //            {
        //                //MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID.ToString() == empid select p).FirstOrDefault();
        //                MstGLDetermination glDetr = ds.getEmpGl(emp);
        //                if (glDetr == null)
        //                {
        //                    oApplication.StatusBar.SetText("GL determination not set", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //                }
        //                empBasicSalary = (decimal)emp.BasicSalary;
        //                decimal spTaxbleAmnt = 0.00M;
        //                decimal spTaxableAmntOT = 0.00M;
        //                decimal spTaxableAmntLWOP = 0.00M;
        //                decimal DaysCnt = 0;
        //                decimal payDays = 0.00M;
        //                decimal leaveDays = 0.00M;
        //                decimal monthDays = 0.00M;
        //                decimal nonRecurringTaxable = 0.00M;
        //                decimal payRatio = 1.00M;

        //                DaysCnt = ds.getDaysCnt(emp, payrollperiod, out payDays, out leaveDays, out monthDays);
        //                decimal employeeRemainingSalary = 0.00M;
        //                payRatio = payDays / monthDays;

        //                if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
        //                {
        //                    empGrossSalary = ds.getEmpGross(emp, payrollperiod.ID);
        //                    empBasicSalary = empGrossSalary;
        //                }
        //                else
        //                {
        //                    empGrossSalary = ds.getEmpGross(emp);
        //                }


        //                try
        //                {
        //                    employeeRemainingSalary = Math.Round((decimal)empBasicSalary * payRatio, 0);
        //                }
        //                catch { }
        //                //TrnsSalaryProcessRegister reg = new TrnsSalaryProcessRegister();
        //                reg = new TrnsSalaryProcessRegister();
        //                reg.MstEmployee = emp;
        //                reg.CfgPayrollDefination = payroll;
        //                reg.CfgPeriodDates = payrollperiod;
        //                reg.EmpBasic = employeeRemainingSalary;//Math.Round(Convert.ToDecimal(empBasicSalary * payRatio), 0);
        //                reg.EmpGross = empGrossSalary;
        //                reg.CreateDate = DateTime.Now;
        //                reg.UpdateDate = DateTime.Now;
        //                reg.UserId = oCompany.UserName;
        //                reg.UpdatedBy = oCompany.UserName;
        //                reg.PeriodName = payrollperiod.PeriodName;
        //                reg.PayrollName = payroll.PayrollName;
        //                reg.EmpName = emp.FirstName + " " + emp.LastName;
        //                //reg.DaysPaid = Convert.ToInt16( payDays);
        //                reg.DaysPaid = Convert.ToDecimal(DaysCnt);
        //                reg.MonthDays = Convert.ToInt32(monthDays);

        //                /// Basic Salary ////
        //                /// ************////
        //               spdHeadRow = new TrnsSalaryProcessRegisterDetail();
        //                spdHeadRow.LineType = "BS";
        //                spdHeadRow.LineSubType = "Basic Salary";
        //                spdHeadRow.LineValue = Math.Round(employeeRemainingSalary, 0);
        //                spdHeadRow.LineMemo = "Basic Salary ";
        //                spdHeadRow.DebitAccount = glDetr.BasicSalary;
        //                spdHeadRow.CreditAccount = glDetr.BSPayable;
        //                spdHeadRow.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
        //                spdHeadRow.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
        //                spdHeadRow.LineBaseEntry = emp.ID;
        //                spdHeadRow.BaseValueCalculatedOn = employeeRemainingSalary;
        //                spdHeadRow.BaseValue = employeeRemainingSalary;
        //                spdHeadRow.BaseValueType = "FIX";
        //                spdHeadRow.CreateDate = DateTime.Now;
        //                spdHeadRow.UpdateDate = DateTime.Now;
        //                spdHeadRow.UserId = oCompany.UserName;
        //                spdHeadRow.UpdatedBy = oCompany.UserName;
        //                spdHeadRow.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                spdHeadRow.TaxableAmount = employeeRemainingSalary;
        //                spTaxbleAmnt += employeeRemainingSalary;
        //                // employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
        //                //reg.TrnsSalaryProcessRegisterDetail.Add(spdHeadRow);





        //                //* AbsentDeductions,Reimbursement

        //                //////Absents ////
        //                //**************////
        //                decimal leaveCnt = 0.00M;
        //                //DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt);

        //                DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt, glDetr);
        //                foreach (DataRow dr in dtAbsentDeduction.Rows)
        //                {
        //                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                    spTaxableAmntLWOP += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                    //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                }

        //                //DaysCnt -= leaveCnt;
        //                //* End of Leave Deductions


        //                //* Payroll elements assigned to employee ***Employee Elements ****** 
        //                //*******************************************************************

        //                DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, payrollperiod, DaysCnt, empGrossSalary, glDetr, payRatio, 0, 0);
        //                foreach (DataRow dr in dtSalPrlElements.Rows)
        //                {
        //                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
        //                    {
        //                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                        spdetail.LineType = dr["LineType"].ToString();
        //                        spdetail.LineSubType = dr["LineSubType"].ToString();
        //                        spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                        spdetail.LineMemo = dr["LineMemo"].ToString();
        //                        spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                        spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                        spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                        spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                        spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                        spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                        spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                        spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                        spdetail.CreateDate = DateTime.Now;
        //                        spdetail.UpdateDate = DateTime.Now;
        //                        spdetail.UserId = oCompany.UserName;
        //                        spdetail.UpdatedBy = oCompany.UserName;
        //                        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                        spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                        spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                        nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
        //                        employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                        //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                    }
        //                }
        //                //******************** End of Elements *********************************



        //                //////Over time ////
        //                //**************////

        //                DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, payrollperiod, empGrossSalary);

        //                //Code modified by Zeeshan

        //                foreach (DataRow dr in dtSalOverTimes.Rows)
        //                {
        //                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    spTaxableAmntOT += Convert.ToDecimal(dr["TaxbleAmnt"]);
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                    //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                }



        //                // * ************Advance Recovery Processing **************
        //                //*******************************************************
        //                DataTable dtAdvance = ds.salaryProcessingAdvance(emp, employeeRemainingSalary, payrollperiod);

        //                foreach (DataRow dr in dtAdvance.Rows)
        //                {
        //                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.TaxableAmount = 0.00M;
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;


        //                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                    //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                }



        //                // * ************Loan Recovery Processing **************

        //                DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary, payrollperiod);

        //                foreach (DataRow dr in dtLoands.Rows)
        //                {
        //                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                    spdetail.LineType = dr["LineType"].ToString();
        //                    spdetail.LineSubType = dr["LineSubType"].ToString();
        //                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
        //                    spdetail.LineMemo = dr["LineMemo"].ToString();
        //                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
        //                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
        //                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
        //                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
        //                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
        //                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
        //                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
        //                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
        //                    spdetail.CreateDate = DateTime.Now;
        //                    spdetail.UpdateDate = DateTime.Now;
        //                    spdetail.UserId = oCompany.UserName;
        //                    spdetail.UpdatedBy = oCompany.UserName;
        //                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                    spdetail.TaxableAmount = 0.00M;
        //                    employeeRemainingSalary += (decimal)spdetail.LineValue;
        //                    //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                }

        //                reg.EmpTaxblTotal = spTaxbleAmnt;
        //                // * ************TAX**************
        //                Decimal QuaterlyTaxValueReturn = 0.0M;
        //                if (Program.systemInfo.TaxSetup == true && emp.FlgTax == true)
        //                {
        //                    //decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable);
        //                    decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, payRatio, out QuaterlyTaxValueReturn);
        //                    if (TotalTax >= 0)
        //                    {
        //                        reg.EmpTotalTax = TotalTax;

        //                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                        spdetail.LineType = "Tax";
        //                        spdetail.LineSubType = "Tax";
        //                        spdetail.LineValue = -Math.Round(TotalTax, 0);
        //                        spdetail.LineMemo = "Tax Deduction";
        //                        spdetail.DebitAccount = glDetr.IncomeTaxExpense;
        //                        spdetail.CreditAccount = glDetr.IncomeTaxPayable;
        //                        spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
        //                        spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
        //                        spdetail.LineBaseEntry = 0;
        //                        spdetail.BaseValueCalculatedOn = spTaxbleAmnt;
        //                        spdetail.BaseValue = spTaxbleAmnt;
        //                        spdetail.BaseValueType = "FIX";
        //                        spdetail.CreateDate = DateTime.Now;
        //                        spdetail.UpdateDate = DateTime.Now;
        //                        spdetail.UserId = oCompany.UserName;
        //                        spdetail.UpdatedBy = oCompany.UserName;
        //                        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                        spdetail.TaxableAmount = 0.00M;
        //                        //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                    }
        //                }
        //                spTaxbleAmnt = spTaxbleAmnt + QuaterlyTaxValueReturn;
        //                //reg.EmpTaxblTotal = spTaxbleAmnt;
        //                TaxableValue = QuaterlyTaxValueReturn;
        //                //************************************************
        //                //********** Gratuity Calculations ***************

        //                if (emp.CfgPayrollDefination.FlgGratuity == true)
        //                {
        //                    int gratCnt = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).Count();
        //                    if (gratCnt > 0)
        //                    {
        //                        MstGratuity empGrat = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).FirstOrDefault();

        //                        try
        //                        {
        //                            int FromYr = Convert.ToInt16(empGrat.YearFrom) * 365;

        //                            if ((Convert.ToDateTime(payrollperiod.StartDate) - Convert.ToDateTime(emp.JoiningDate)).Days > FromYr)
        //                            {
        //                                decimal gratProvision = 0.00M;
        //                                decimal basedOnAmont = 0.00M;
        //                                if (empGrat.BasedOn == "0")
        //                                {
        //                                    basedOnAmont = empBasicSalary;
        //                                }
        //                                else
        //                                {
        //                                    basedOnAmont = empGrossSalary;
        //                                }

        //                                gratProvision = (basedOnAmont * (decimal)empGrat.Factor / 100) / 12;
        //                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                                spdetail.LineType = "Element";
        //                                spdetail.LineSubType = "Empr Cont";
        //                                spdetail.LineValue = Math.Round(gratProvision, 0);
        //                                spdetail.LineMemo = "Gratuity";
        //                                spdetail.DebitAccount = glDetr.GratuityExpense;
        //                                spdetail.CreditAccount = glDetr.GratuityPayable;
        //                                spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityExpense);
        //                                spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityPayable);
        //                                spdetail.LineBaseEntry = 0;
        //                                spdetail.BaseValueCalculatedOn = empBasicSalary;
        //                                spdetail.BaseValue = empBasicSalary;
        //                                spdetail.BaseValueType = "FIX";
        //                                spdetail.CreateDate = DateTime.Now;
        //                                spdetail.UpdateDate = DateTime.Now;
        //                                spdetail.UserId = oCompany.UserName;
        //                                spdetail.UpdatedBy = oCompany.UserName;
        //                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
        //                                spdetail.TaxableAmount = 0.00M;
        //                                //reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {

        //                        }
        //                    }
        //                }
        //                reg.SalaryStatus = 0;//Salary Processed
        //                if (emp.PaymentMode == "HOLD")
        //                {
        //                    reg.FlgHoldPayment = true;
        //                }
        //                reg = null;
        //                spdHeadRow = null;
        //                //dbHrPayroll.TrnsSalaryProcessRegister.InsertOnSubmit(reg);
        //            }
        //            #endregion

        //            //dbHrPayroll.SubmitChanges();

        //            reg = null;
        //            spdHeadRow = null;
        //            dbHrPayroll.SubmitChanges();
        //            //spdetail = null;
        //        }
        //        catch (Exception ex)
        //        {
        //            oApplication.SetStatusBarMessage(strProcessing + ":" + ex.Message);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        oApplication.SetStatusBarMessage(ex.Message);
        //    }
        //}

        private void DeleteProcess()
        {
            try
            {
                if (DeleteRecord())
                {
                    ClearRecord();
                }
            }
            catch (Exception Ex)
            {
            }
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