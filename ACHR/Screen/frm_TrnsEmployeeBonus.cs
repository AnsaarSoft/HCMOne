using System;
using System.Data;
using System.IO;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbouiCOM;
using System.Text.RegularExpressions;

namespace ACHR.Screen
{
    partial class frm_TrnsEmployeeBonus : HRMSBaseForm
    {
        #region Local Variable Area

        public IEnumerable<TrnsEmployeeBonus> BonusPaymentBatch;
        SAPbouiCOM.DataTable dtPeriods;
        Boolean flgEmpFrom = false, flgEmpTo = false, flgValidCall = false, flgDocMode = false, flgCalculateBonus = false;
        string selEmpId = "";

        SAPbouiCOM.EditText txDocNum, txFilenam, txtEmpFrom, txtEmpTo;
        SAPbouiCOM.ComboBox cbCalendar, cbPayroll, cbPeriod, cbStatus, cbElementType;
        SAPbouiCOM.ComboBox cbDepartment, cbLocation, cbDesignation, cbJobTitle;
        SAPbouiCOM.Matrix mtEmp;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clID, clEmployeeID, clEmployeeName, clBranch, clBasicSalary, clGrossSalary, clSlabCode, clSalaryRange, clPercentage, clNetSalary, clNetAmount, clActive;
        SAPbouiCOM.Item ItxDocNum, ItxDocDate, ItxApidAccount, ItxDurationFrom, ItxDurationTo, ItxFilenam, ItxEmpFrom, ItxEmpTo;
        SAPbouiCOM.Item IcbProll, IcbPeriod, IcbStatus, IcbElementType, IcbDepartment, IcbLocation, IcbDesignation, IcbJobTitle, IcbCalendar;
        SAPbouiCOM.Button btSave, btCalculateBonus;
        SAPbouiCOM.Item ImtEmp, IbtProcess, IbtSave, IbtCalculations;
        SAPbouiCOM.PictureBox pctBox;

        SAPbouiCOM.DataTable dtEmps;
        System.Data.DataTable DtFile = new System.Data.DataTable();
        private Int32 CurrentRecord = 0, TotalRecords = 0;
        private string CompanyName = "";
        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.EnableMenu("1290", false);  // First Record
            oForm.EnableMenu("1291", false);  // Last record 
            InitiallizeForm();
            InitiallizegridMatrix();
            fillCbs();
            IniContrls();
            IbtCalculations.Enabled = false;
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btProcess":
                        //if (!ValidateRecord())
                        //{
                        //    BubbleEvent = false;
                        //}
                        break;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btnSave":
                    SubmitForm();
                    break;
                case "btEmpFr":
                    flgEmpTo = false;
                    flgEmpFrom = true;
                    OpenNewSearchFormFrom();
                    break;
                case "btEmpTo":
                    flgEmpTo = true;
                    flgEmpFrom = false;
                    OpenNewSearchFormTo();
                    break;
                case "btGetEmp":
                    GetEmployees();
                    break;
                case "btPick":
                    LoadEmployeeToGrid();
                    break;
                case "btncalc":
                    if (CompanyName.ToLower() == "wahnobel")
                    {
                        CalculateBonusBasedonMonthly();
                    }
                    else
                    {
                        CalculateBonusBasedOnYearly();
                    }
                    break;
                case "btProcess":
                    var DocumentStatus = (from a in dbHrPayroll.TrnsEmployeeBonus
                                          where a.DocumentNo.ToString() == txDocNum.Value.Trim()
                                          select a).FirstOrDefault();
                    if (DocumentStatus != null)
                    {
                        if (DocumentStatus.Status == "0" || DocumentStatus.Status == "1")
                        {
                            Int32 DocNum = Convert.ToInt32(DocumentStatus.DocumentNo);

                            PostPayment(DocNum);
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Selected document already Posted", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                    }

                    break;

            }
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
                if (pVal.ItemUID == "cbProll")
                {
                    FillPeriod(cbPayroll.Value.Trim());
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (flgEmpTo && !flgEmpFrom)
            {
                txtEmpTo.Value = Program.EmpID;
                flgEmpFrom = false;
                flgEmpTo = false;
            }
            if (!flgEmpTo && flgEmpFrom)
            {
                txtEmpFrom.Value = Program.EmpID;
                flgEmpFrom = false;
                flgEmpTo = false;
            }
            //SetEmpValues();
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

        #endregion

        #region Local Functions

        private void InitiallizeForm()
        {
            try
            {
                CompanyName = string.IsNullOrEmpty(Program.systemInfo.CompanyName) ? "" : Program.systemInfo.CompanyName.Trim();
                oForm.Freeze(true);

                oForm.DataSources.UserDataSources.Add("txEmpFrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpFrom = oForm.Items.Item("txEmpFrom").Specific;
                ItxEmpFrom = oForm.Items.Item("txEmpFrom");
                txtEmpFrom.DataBind.SetBound(true, "", "txEmpFrom");

                oForm.DataSources.UserDataSources.Add("txEmpTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpTo = oForm.Items.Item("txEmpTo").Specific;
                ItxEmpTo = oForm.Items.Item("txEmpTo");
                txtEmpTo.DataBind.SetBound(true, "", "txEmpTo");

                oForm.DataSources.UserDataSources.Add("txDocNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txDocNum = oForm.Items.Item("txDocNum").Specific;
                ItxDocNum = oForm.Items.Item("txDocNum");
                txDocNum.DataBind.SetBound(true, "", "txDocNum");

                oForm.DataSources.UserDataSources.Add("txFilenam", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 150);
                txFilenam = oForm.Items.Item("txFilenam").Specific;
                ItxFilenam = oForm.Items.Item("txFilenam");
                txFilenam.DataBind.SetBound(true, "", "txFilenam");

                oForm.Items.Item("btProcess").Enabled = false;
                IbtProcess = oForm.Items.Item("btProcess");

                oForm.Items.Item("btnSave").Enabled = true;
                btSave = oForm.Items.Item("btnSave").Specific;
                IbtSave = oForm.Items.Item("btnSave");

                oForm.Items.Item("btncalc").Enabled = true;
                btCalculateBonus = oForm.Items.Item("btncalc").Specific;
                IbtCalculations = oForm.Items.Item("btncalc");

                #region CBS
                cbPayroll = oForm.Items.Item("cbProll").Specific;
                oForm.DataSources.UserDataSources.Add("cbProll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPayroll.DataBind.SetBound(true, "", "cbProll");

                cbPeriod = oForm.Items.Item("cbPeriod").Specific;
                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPeriod.DataBind.SetBound(true, "", "cbPeriod");

                cbStatus = oForm.Items.Item("cbStatus").Specific;
                oForm.DataSources.UserDataSources.Add("cbStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbStatus.DataBind.SetBound(true, "", "cbStatus");

                cbElementType = oForm.Items.Item("cbelmnt").Specific;
                oForm.DataSources.UserDataSources.Add("cbelmnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbElementType.DataBind.SetBound(true, "", "cbelmnt");

                oForm.DataSources.UserDataSources.Add("cbLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbLocation = oForm.Items.Item("cbLoc").Specific;
                IcbLocation = oForm.Items.Item("cbLoc");
                cbLocation.DataBind.SetBound(true, "", "cbLoc");

                oForm.DataSources.UserDataSources.Add("cbDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbDepartment = oForm.Items.Item("cbDept").Specific;
                IcbDepartment = oForm.Items.Item("cbDept");
                cbDepartment.DataBind.SetBound(true, "", "cbDept");

                oForm.DataSources.UserDataSources.Add("cbDes", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbDesignation = oForm.Items.Item("cbDes").Specific;
                IcbDesignation = oForm.Items.Item("cbDes");
                cbDesignation.DataBind.SetBound(true, "", "cbDes");

                oForm.DataSources.UserDataSources.Add("cbJob", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbJobTitle = oForm.Items.Item("cbJob").Specific;
                IcbJobTitle = oForm.Items.Item("cbJob");
                cbJobTitle.DataBind.SetBound(true, "", "cbJob");

                oForm.DataSources.UserDataSources.Add("cbCalendar", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
                cbCalendar = oForm.Items.Item("cbCalendar").Specific;
                IcbCalendar = oForm.Items.Item("cbCalendar");
                cbCalendar.DataBind.SetBound(true, "", "cbCalendar");



                #endregion

                oForm.Freeze(false);
            }
            catch (Exception ex)
            {

            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtEmps = oForm.DataSources.DataTables.Add("mtEmp");
                dtEmps.Columns.Add("id", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmps.Columns.Add("EmpID", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("EName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("BSal", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("GSal", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("SlabCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("SalaryRange", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("Percentage", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("NetSalary", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("NetAmount", SAPbouiCOM.BoFieldsType.ft_Text);

                dtEmps.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text);

                mtEmp = (SAPbouiCOM.Matrix)oForm.Items.Item("mtEmp").Specific;
                oColumns = (SAPbouiCOM.Columns)mtEmp.Columns;


                oColumn = oColumns.Item("id");
                clID = oColumn;
                oColumn.DataBind.Bind("mtEmp", "id");
                clID.Visible = false;
                oColumn = oColumns.Item("clEId");
                clEmployeeID = oColumn;
                oColumn.DataBind.Bind("mtEmp", "EmpID");

                oColumn = oColumns.Item("clEName");
                clEmployeeName = oColumn;
                oColumn.DataBind.Bind("mtEmp", "EName");

                oColumn = oColumns.Item("clBSal");
                clBasicSalary = oColumn;
                oColumn.DataBind.Bind("mtEmp", "BSal");

                oColumn = oColumns.Item("clGSal");
                clGrossSalary = oColumn;
                oColumn.DataBind.Bind("mtEmp", "GSal");

                oColumn = oColumns.Item("clSlabCode");
                clSlabCode = oColumn;
                oColumn.DataBind.Bind("mtEmp", "SlabCode");

                oColumn = oColumns.Item("clRange");
                clSalaryRange = oColumn;
                oColumn.DataBind.Bind("mtEmp", "SalaryRange");

                oColumn = oColumns.Item("clPerc");
                clPercentage = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Percentage");

                oColumn = oColumns.Item("clNetSal");
                clNetSalary = oColumn;
                oColumn.DataBind.Bind("mtEmp", "NetSalary");
                clNetSalary.Visible = false;
                oColumn = oColumns.Item("clNetAmt");
                clNetAmount = oColumn;
                oColumn.DataBind.Bind("mtEmp", "NetAmount");

                oColumn = oColumns.Item("clActive");
                clActive = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Active");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormFrom()
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
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormTo()
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
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.FromEmpId))
                {
                    txtEmpFrom.Value = Program.FromEmpId;
                }
                if (!string.IsNullOrEmpty(Program.ToEmpId))
                {
                    txtEmpTo.Value = Program.ToEmpId;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void fillCbs()
        {
            try
            {
                int i = 0;
                string selId = "0";

                #region Fill Payroll

                string strOut = string.Empty;
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'");
                strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    if (strOut != null && strOut != "")
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == strOut.Trim() select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbPayroll.Value);
                    }
                    else
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbPayroll.Value);
                    }

                }
                else
                {
                    IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                    foreach (CfgPayrollDefination pr in prs)
                    {
                        cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        i++;
                    }

                    cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    FillPeriod(cbPayroll.Value);
                }
                #endregion

                //cbCalendar.ValidValues.Add("0", "All");
                IEnumerable<MstCalendar> oCalendar = from c in dbHrPayroll.MstCalendar where c.FlgActive == true orderby c.Description ascending select c;

                foreach (MstCalendar calendar in oCalendar)
                {
                    cbCalendar.ValidValues.Add(calendar.Id.ToString(), calendar.Description);

                }
                cbCalendar.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                cbDepartment.ValidValues.Add("0", "All");
                IEnumerable<MstDepartment> depts = (from p in dbHrPayroll.MstDepartment orderby p.DeptName ascending select p);

                foreach (MstDepartment dept in depts)
                {
                    cbDepartment.ValidValues.Add(dept.ID.ToString(), dept.DeptName);

                }
                cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                cbLocation.ValidValues.Add("0", "All");
                IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation orderby p.Description ascending select p;

                foreach (MstLocation loc in locs)
                {
                    cbLocation.ValidValues.Add(loc.Id.ToString(), loc.Description);

                }
                cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                cbDesignation.ValidValues.Add("0", "All");
                IEnumerable<MstDesignation> designations = from p in dbHrPayroll.MstDesignation orderby p.Description ascending select p;

                foreach (MstDesignation des in designations)
                {
                    cbDesignation.ValidValues.Add(des.Id.ToString(), des.Description);

                }
                cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);


                cbJobTitle.ValidValues.Add("0", "All");
                IEnumerable<MstJobTitle> jobtitles = from p in dbHrPayroll.MstJobTitle orderby p.Description ascending select p;

                foreach (MstJobTitle jt in jobtitles)
                {
                    cbJobTitle.ValidValues.Add(jt.Id.ToString(), jt.Description);

                }
                cbJobTitle.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //End Fill Payroll


                FillPeriod(cbPayroll.Value);
                fillCombo("btchStatus", cbStatus);
                FillEmployeeElement();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function fillCbs.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillEmployeeElement()
        {
            try
            {
                cbElementType.ValidValues.Add("-1", "[Select One]");
                var Records = from v in dbHrPayroll.MstElements
                              where v.FlgEmployeeBonus == true                             
                              select v;
                foreach (var Record in Records)
                {
                    cbElementType.ValidValues.Add(Record.ElementName, Record.Description);
                }
                cbElementType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                // dtPeriods.Rows.Clear();
                if (cbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPeriod.ValidValues.Remove(cbPeriod.ValidValues.Item(k).Value);
                    }
                }
                int i = 0;
                string selId = "0";
                bool flgPrevios = false;
                bool flgHit = false;
                int count = 0;
                int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Count();
                if (cnt > 0)
                {
                    CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Single();
                    foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    {
                        //if (Convert.ToBoolean(pd.FlgVisible))
                        //{
                        cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        //}
                        count++;

                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();

                        if (Convert.ToBoolean(pd.FlgLocked))
                        {
                            selId = "0";
                            flgPrevios = true;
                        }
                        else
                        {
                            if (flgPrevios)
                            {
                                selId = pd.ID.ToString();
                                flgPrevios = false;
                            }
                        }

                        i++;
                    }
                    try
                    {
                        cbPeriod.Select(selId);
                        //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void IniContrls()
        {

            try
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oForm.Update();
                oForm.Refresh();
                //getData();
                //GetDocumentNo();
                long nextId = ds.getNextId("TrnsEmployeeBonus", "ID");
                txDocNum.Value = nextId.ToString();
                cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                dtEmps.Rows.Clear();
                AddEmptyRow();
                btSave.Caption = "Save";

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function IniContrls.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtEmps.Rows.Count == 0)
            {
                dtEmps.Rows.Add(1);
                RowValue = dtEmps.Rows.Count;
                dtEmps.SetValue(clEmployeeID.DataBind.Alias, RowValue - 1, "");
                dtEmps.SetValue(clEmployeeName.DataBind.Alias, RowValue - 1, "");
                dtEmps.SetValue(clBasicSalary.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clBasicSalary.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clGrossSalary.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clSlabCode.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clNetSalary.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clNetAmount.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clActive.DataBind.Alias, RowValue - 1, "N");
                //grdMain.AddRow(1, RowValue + 1);
                mtEmp.AddRow(1, 0);
            }
            else
            {
                if (dtEmps.GetValue(clEmployeeName.DataBind.Alias, dtEmps.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtEmps.Rows.Add(1);
                    RowValue = dtEmps.Rows.Count;
                    dtEmps.SetValue(clEmployeeID.DataBind.Alias, RowValue - 1, "");
                    dtEmps.SetValue(clEmployeeName.DataBind.Alias, RowValue - 1, "");
                    dtEmps.SetValue(clBasicSalary.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clBasicSalary.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clGrossSalary.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clSlabCode.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clNetSalary.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clNetAmount.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clActive.DataBind.Alias, RowValue - 1, "N");
                    mtEmp.AddRow(1, mtEmp.RowCount + 1);
                }
            }
            mtEmp.LoadFromDataSource();
        }

        private void getData()
        {
            try
            {
                CodeIndex.Clear();
                BonusPaymentBatch = from p in dbHrPayroll.TrnsEmployeeBonus select p;
                int i = 0;
                foreach (TrnsEmployeeBonus ele in BonusPaymentBatch)
                {
                    CodeIndex.Add(ele.Id.ToString(), i);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function getData.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetEmployees()
        {
            try
            {
                DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName, Program.objHrmsUI.logger);
                string PayrollID = cbPayroll.Selected.Value;
                var oPayroll = (from p in dbHrPayroll.CfgPayrollDefination 
                                where p.ID == Convert.ToInt32(PayrollID) 
                                select p).FirstOrDefault();
                string strSql = "SELECT EmpID, SBOEmpCode, ID,ISNULL(FirstName,'') + ' ' + ISNULL(MiddleName, '')+ ' ' + ISNULL(LastName, '') AS empName ,  DepartmentName, LocationName FROM         " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee where payrollId = " + cbPayroll.Value.ToString().Trim() + " and ResignDate IS NULL AND ISNULL(flgActive,'1') = 1 ";
                if (cbDepartment.Value.ToString().Trim() != "0")
                {
                    strSql += " and departmentId = " + cbDepartment.Value.ToString();
                }
                if (cbLocation.Value.ToString().Trim() != "0")
                {
                    strSql += " and location = " + cbLocation.Value.ToString().Trim();
                }

                if (cbDesignation.Value.ToString().Trim() != "0")
                {
                    strSql += " and DesignationID = '" + cbDesignation.Value.ToString().Trim() + "'";
                }

                if (cbJobTitle.Value.ToString().Trim() != "0")
                {
                    strSql += " and JobTitle = '" + cbJobTitle.Value.ToString().Trim() + "'";
                }

                if (!String.IsNullOrEmpty(txtEmpFrom.Value.Trim()) && !String.IsNullOrEmpty(txtEmpTo.Value.Trim()))
                {
                    Int32? FromEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault();
                    Int32? ToEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                    if (FromEmpID == null) FromEmpID = 0;
                    if (ToEmpID == null) ToEmpID = 100000000;
                    strSql += " and ISNULL(sortorder,0) between " + FromEmpID + " and " + ToEmpID + "";
                }
                System.Data.DataTable dtEmp = ds.getDataTable(strSql);
                dtEmps.Rows.Clear();
                int i = 0;
                MstEmployee emp;
                decimal empGross = 0.00M;
                foreach (DataRow dr in dtEmp.Rows)
                {
                    //emp = (from p in dbHrPayroll.MstEmployee 
                    //       where p.EmpID == dr["EmpID"].ToString()
                    //       select p).Single();
                    emp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == dr["EmpID"].ToString()
                                  && a.PayrollID == oPayroll.ID
                                  && a.BonusCode != null
                                  select a).FirstOrDefault();
                    if (emp != null)
                    {
                        dtEmps.Rows.Add(1);
                        dtEmps.SetValue("EmpID", i, dr["EmpID"].ToString());
                        dtEmps.SetValue("EName", i, dr["empName"].ToString());
                        dtEmps.SetValue("BSal", i, string.Format("{0:0.00}", emp.BasicSalary));
                        if (emp.GrossSalary > 0)
                        {

                            dtEmps.SetValue("GSal", i, string.Format("{0:0.00}", emp.GrossSalary));
                        }
                        else
                        {
                            dtEmps.SetValue("GSal", i, string.Format("{0:0.00}", ds.getEmpGross(emp)));
                        }
                        i++;
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Employee ID: '" + dr["EmpID"] + "' not found in payroll/Bonus not applicable: '" + oPayroll.PayrollName + "' ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                }

                mtEmp.LoadFromDataSource();
                IbtCalculations.Enabled = true;
            }
            catch (Exception ex)
            {

                oApplication.StatusBar.SetText("Error in Function getEmployees.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadEmployeeToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[2];
            Int16 counter = 0;
            Int16 LineNumber = 1;
            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("No");
                DtFile.Columns.Add("EmpID");
                string PayrollID = cbPayroll.Selected.Value;
                var oPayroll = (from p in dbHrPayroll.CfgPayrollDefination 
                                where p.ID == Convert.ToInt32(PayrollID) 
                                select p).FirstOrDefault();
                FilePath = Program.objHrmsUI.FindFile();
                if (String.IsNullOrEmpty(FilePath))
                {
                    oApplication.SetStatusBarMessage("Select a template file");
                    return;
                }
                txFilenam.Value = Convert.ToString(FilePath);
                if (!string.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();

                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {

                    mtEmp.Clear();
                    dtEmps.Rows.Clear();
                    foreach (DataRow dr1 in DtFile.Rows)
                    {
                        var oEmpID = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID == dr1["EmpID"].ToString()
                                      && a.PayrollID == oPayroll.ID
                                      && a.BonusCode!=null
                                      select a).FirstOrDefault();
                        if (oEmpID != null)
                        {
                            dtEmps.Rows.Add();
                            dtEmps.SetValue("id", LineNumber - 1, LineNumber);
                            dtEmps.SetValue("EmpID", LineNumber - 1, dr1["EmpID"]);

                            string stremp = oEmpID.EmpID;
                            dtEmps.SetValue("EName", LineNumber - 1, oEmpID.FirstName + ' ' + oEmpID.MiddleName + ' ' + oEmpID.LastName);
                            dtEmps.SetValue("BSal", LineNumber - 1, string.Format("{0:0.00}", oEmpID.BasicSalary));
                            if (oEmpID.GrossSalary > 0)
                            {
                                dtEmps.SetValue("GSal", LineNumber - 1, string.Format("{0:0.00}", oEmpID.GrossSalary));
                            }
                            else
                            {
                                dtEmps.SetValue("GSal", LineNumber - 1, string.Format("{0:0.00}", ds.getEmpGross(oEmpID)));
                            }

                            LineNumber++;
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Employee ID: '" + dr1["EmpID"] + "' not found in payroll/Bonus not applicable: '" + oPayroll.PayrollName + "' ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }


                    }
                    mtEmp.LoadFromDataSource();
                    IbtCalculations.Enabled = true;

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoadEmployeeToGrid : " + Ex.Message + counter + " : " + LineNumber.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void CalculateBonusOriginal()
        {

            string code = "";
            List<string> oSelectedEmployee = new List<string>();
            mtEmp.FlushToDataSource();
            try
            {
                if (cbStatus.Value == "Processed")
                {
                    oApplication.StatusBar.SetText("Bonus already calculated and Processed", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {

                    code = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    code = code.Trim();
                    if (code != "")
                    {
                        var oEmpID = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID == code.ToString()
                                      select a).FirstOrDefault();
                        if (oEmpID != null)
                        {

                            var oSlabs = (from s in dbHrPayroll.MstBonusYearly
                                          where s.DocCode == oEmpID.BonusCode
                                          select s).ToList();
                            #region Variables
                            string ActiveSlab = "";
                            string SlabCode = "";
                            decimal decGrossSalary = 0;
                            decimal decSalaryFrom = 0;
                            decimal decSalaryTo = 0;
                            decimal decPercentage = 0;
                            decimal decMinimumMonthsDuration = 0;
                            int ElementType = 0;
                            string Range = "";
                            decimal TotalNetSalary = 0;
                            decimal CalculatedNetAmount = 0;
                            #endregion

                            foreach (var OneSlab in oSlabs)
                            {
                                SlabCode = OneSlab.Code;
                                decGrossSalary = Convert.ToDecimal(oEmpID.GrossSalary);
                                decSalaryFrom = Convert.ToDecimal(OneSlab.SalaryFrom);
                                decSalaryTo = Convert.ToDecimal(OneSlab.SalaryTo);
                                ActiveSlab = Convert.ToString(OneSlab.FlgActive);
                                decPercentage = 0;
                                decMinimumMonthsDuration = 0;
                                ElementType = 0;

                                if (decGrossSalary >= decSalaryFrom && decGrossSalary <= decSalaryTo)
                                {
                                    SlabCode = OneSlab.Code;
                                    decPercentage = Convert.ToDecimal(OneSlab.BonusPercentage);
                                    decMinimumMonthsDuration = Convert.ToDecimal(OneSlab.MinimumMonthsDuration);
                                    ElementType = Convert.ToInt32(OneSlab.ElementType);
                                    Range = Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryFrom), 0)) + " - " + Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryTo), 0));
                                    break;
                                }
                            }
                            #region BonusCalculate

                            try
                            {
                                if (ActiveSlab != "False")
                                {
                                    var oCalendar = (from a in dbHrPayroll.MstCalendar
                                                     where a.FlgActive == true
                                                     select a).FirstOrDefault();

                                    int PostedSalaryPeriodCount = (from a in dbHrPayroll.TrnsSalaryProcessRegister
                                                                   join b in dbHrPayroll.TrnsJE on a.JENum equals b.ID
                                                                   where a.PayrollID == oEmpID.PayrollID
                                                                                    && a.PayrollPeriodID == a.CfgPeriodDates.ID
                                                                                    && a.CfgPeriodDates.CalCode == oCalendar.Code
                                                                                    && a.JENum == b.ID
                                                                                    && a.PayrollID == b.PayrollID
                                                                                    && a.EmpID == oEmpID.ID
                                                                                    && b.SBOJeNum != null
                                                                   select a).Count();

                                    if (decMinimumMonthsDuration >= PostedSalaryPeriodCount)
                                    {
                                        TotalNetSalary = 0;
                                        TotalNetSalary = (from a in dbHrPayroll.TrnsSalaryProcessRegisterDetail
                                                          where a.TrnsSalaryProcessRegister.PayrollID == oEmpID.PayrollID
                                                                   && a.TrnsSalaryProcessRegister.PayrollPeriodID == a.TrnsSalaryProcessRegister.CfgPeriodDates.ID
                                                                   && a.TrnsSalaryProcessRegister.CfgPeriodDates.CalCode == oCalendar.Code
                                                                   && a.TrnsSalaryProcessRegister.JENum != null
                                                                   && a.TrnsSalaryProcessRegister.EmpID == oEmpID.ID
                                                          select a.LineValue).Sum() ?? 0;

                                        decimal getBasic = oEmpID.BasicSalary.Value;

                                        CalculatedNetAmount = (TotalNetSalary / 100) * decPercentage;

                                        #region Set Value on Grid

                                        (mtEmp.Columns.Item("clSlabCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = SlabCode.ToString();
                                        (mtEmp.Columns.Item("clRange").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Range.ToString();
                                        (mtEmp.Columns.Item("clPerc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(decPercentage), 2).ToString();
                                        (mtEmp.Columns.Item("clNetSal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(TotalNetSalary), 2).ToString();
                                        (mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(CalculatedNetAmount), 2).ToString();


                                        Boolean flgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked = true;
                                        #endregion
                                    }
                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Fall in Slab: '" + SlabCode + "' but Slab is not Active. ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                            #endregion
                        }

                    }
                }
                IbtCalculations.Enabled = false;
            }
            catch (Exception ex)
            {

            }

        }

        private void CalculateBonusBasedOnYearly()
        {

            string code = "";
            List<string> oSelectedEmployee = new List<string>();
            mtEmp.FlushToDataSource();
            try
            {
                if (cbStatus.Value == "Processed")
                {
                    oApplication.StatusBar.SetText("Bonus already calculated and Processed", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {

                    code = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    code = code.Trim();
                    if (code != "")
                    {
                        var oEmp = (from a in dbHrPayroll.MstEmployee
                                    where a.EmpID == code.ToString()
                                    select a).FirstOrDefault();
                        if (oEmp != null)
                        {

                            var oSlabs = (from s in dbHrPayroll.MstBonusYearly
                                          where s.DocCode == oEmp.BonusCode
                                          select s).ToList();
                            #region Variables
                            string ActiveSlab = "", SlabCode = "", ValueType = "", Range = "";
                            decimal decGrossSalary = 0, decBasicSalary = 0, decSalaryFrom = 0, decSalaryTo = 0;
                            Int32 intScaleFrom = 0, intScaleTo = 0;
                            decimal decPercentage = 0, decMinimumMonthsDuration = 0, TotalNetSalary = 0;
                            int ElementType = 0;
                            decimal CalculatedNetAmount = 0, OutValue = 0;

                            #endregion

                            foreach (var OneSlab in oSlabs)
                            {
                                SlabCode = OneSlab.Code;
                                ValueType = Convert.ToString(OneSlab.ValueType);
                                decBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                                decGrossSalary = Convert.ToDecimal(oEmp.GrossSalary);
                                decSalaryFrom = Convert.ToDecimal(OneSlab.SalaryFrom);
                                decSalaryTo = Convert.ToDecimal(OneSlab.SalaryTo);
                                intScaleFrom = Convert.ToInt32(OneSlab.ScaleFrom);
                                intScaleTo = Convert.ToInt32(OneSlab.ScaleTo);
                                ActiveSlab = Convert.ToString(OneSlab.FlgActive);
                                decPercentage = 0;
                                decMinimumMonthsDuration = 0;
                                ElementType = 0;
                                if (ValueType == "POB")
                                {
                                    OutValue = (decimal)oEmp.BasicSalary.GetValueOrDefault();
                                }
                                if (ValueType == "POG")
                                {
                                    //Anas
                                    //OutValue = (decimal)oEmp.GrossSalary.GetValueOrDefault();
                                    OutValue = (decimal)ds.getEmpGross(oEmp);
                                }

                                if (OutValue >= decSalaryFrom && OutValue <= decSalaryTo)
                                {
                                    SlabCode = OneSlab.Code;
                                    decPercentage = Convert.ToDecimal(OneSlab.BonusPercentage);
                                    decMinimumMonthsDuration = Convert.ToDecimal(OneSlab.MinimumMonthsDuration);
                                    ElementType = Convert.ToInt32(OneSlab.ElementType);
                                    Range = Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryFrom), 0)) + " - " + Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryTo), 0));
                                    break;
                                }

                            }
                            #region BonusCalculate

                            try
                            {
                                if (ActiveSlab != "False")
                                {
                                    var oCalendar = (from a in dbHrPayroll.MstCalendar
                                                     where a.FlgActive == true
                                                     select a).FirstOrDefault();

                                    int PostedSalaryPeriodCount = (from a in dbHrPayroll.TrnsSalaryProcessRegister
                                                                   join b in dbHrPayroll.TrnsJE on a.JENum equals b.ID
                                                                   where a.PayrollID == oEmp.PayrollID
                                                                                    && a.PayrollPeriodID == a.CfgPeriodDates.ID
                                                                                    && a.CfgPeriodDates.CalCode == oCalendar.Code
                                                                                    && a.JENum == b.ID
                                                                                    && a.PayrollID == b.PayrollID
                                                                                    && a.EmpID == oEmp.ID
                                                                                    && b.SBOJeNum != null
                                                                   select a).Count();

                                    if (decMinimumMonthsDuration <= PostedSalaryPeriodCount)
                                    {
                                        TotalNetSalary = 0;
                                        //TotalNetSalary = (from a in dbHrPayroll.TrnsSalaryProcessRegisterDetail
                                        //                  where a.TrnsSalaryProcessRegister.PayrollID == oEmp.PayrollID
                                        //                           && a.TrnsSalaryProcessRegister.PayrollPeriodID == a.TrnsSalaryProcessRegister.CfgPeriodDates.ID
                                        //                           && a.TrnsSalaryProcessRegister.CfgPeriodDates.CalCode == oCalendar.Code
                                        //                           && a.TrnsSalaryProcessRegister.JENum != null
                                        //                           && a.TrnsSalaryProcessRegister.EmpID == oEmp.ID
                                        //                  select a.LineValue).Sum() ?? 0;

                                        TotalNetSalary = OutValue;

                                        decimal getBasic = oEmp.BasicSalary.Value;

                                        CalculatedNetAmount = (TotalNetSalary / 100) * decPercentage;

                                        #region Set Value on Grid

                                        (mtEmp.Columns.Item("clSlabCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = SlabCode.ToString();
                                        (mtEmp.Columns.Item("clRange").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Range.ToString();
                                        (mtEmp.Columns.Item("clPerc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(decPercentage), 2).ToString();
                                        (mtEmp.Columns.Item("clNetSal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(TotalNetSalary), 2).ToString();
                                        (mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(CalculatedNetAmount), 2).ToString();


                                        Boolean flgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked = true;
                                        #endregion
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Fall below than Min. Months Duration", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Fall in Slab: '" + SlabCode + "' but Slab is not Active. ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                            #endregion
                        }

                    }
                }
                IbtCalculations.Enabled = false;
            }
            catch (Exception ex)
            {

            }

        }

        private void CalculateBonusBasedonMonthly()
        {

            string code = "";
            List<string> oSelectedEmployee = new List<string>();
            mtEmp.FlushToDataSource();
            try
            {
                if (cbStatus.Value == "Processed")
                {
                    oApplication.StatusBar.SetText("Bonus already calculated and Processed", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {
                    decimal EffectiveMonths = 0.0M;
                    decimal GetPartialMonth = 0.0M;
                    code = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    code = code.Trim();
                    if (code != "")
                    {
                        var oEmp = (from a in dbHrPayroll.MstEmployee
                                    where a.EmpID == code.ToString()
                                    select a).FirstOrDefault();
                        if (oEmp != null)
                        {

                            var oSlabs = (from s in dbHrPayroll.MstBonusYearly
                                          where s.DocCode == oEmp.BonusCode
                                          select s).ToList();
                            #region Variables
                            string ActiveSlab = "", strScale = "", SlabCode = "", ValueType = "";
                            string Range = "", ScaleNumber = "";
                            decimal decGrossSalary = 0, decBasicSalary = 0, decSalaryFrom = 0, decSalaryTo = 0;
                            Int32 intScaleFrom = 0, intScaleTo = 0, EmployeeScale = 0;
                            decimal decPercentage = 0, decMinimumMonthsDuration = 0, TotalNetSalary = 0;
                            int ElementType = 0, periodDays = 0;
                            decimal CalculatedNetAmount = 0, OutValue = 0, DaysCount = 0, PresentDays = 0;
                            decimal PayDays = 0.00M, LeaveDays = 0.00M, MonthDays = 0.00M, EarnedSalary = 0;
                            decimal PresentEarnedSalary = 0;
                            #endregion

                            foreach (var OneSlab in oSlabs)
                            {
                                SlabCode = OneSlab.Code;
                                ValueType = Convert.ToString(OneSlab.ValueType);
                                decBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                                decGrossSalary = Convert.ToDecimal(oEmp.GrossSalary);
                                decSalaryFrom = Convert.ToDecimal(OneSlab.SalaryFrom);
                                decSalaryTo = Convert.ToDecimal(OneSlab.SalaryTo);
                                intScaleFrom = Convert.ToInt32(OneSlab.ScaleFrom);
                                intScaleTo = Convert.ToInt32(OneSlab.ScaleTo);
                                ActiveSlab = Convert.ToString(OneSlab.FlgActive);
                                decPercentage = 0;
                                decMinimumMonthsDuration = 0;
                                ElementType = 0;
                                if (ValueType == "POB")
                                {
                                    OutValue = (decimal)oEmp.BasicSalary.GetValueOrDefault();
                                }
                                if (ValueType == "POG")
                                {
                                    if (oEmp.GrossSalary > 0)
                                    {
                                        OutValue = (decimal)oEmp.GrossSalary.GetValueOrDefault();
                                    }
                                    else
                                    {
                                        OutValue = (decimal)ds.getEmpGross(oEmp);
                                    }
                                }


                                if (!String.IsNullOrEmpty(oEmp.PositionName))
                                {
                                    strScale = oEmp.PositionName;
                                    ScaleNumber = Regex.Replace(strScale, @"\D", "");
                                    EmployeeScale = Convert.ToInt32(ScaleNumber);

                                    if (decSalaryFrom > 0 && decSalaryTo > 0 && intScaleFrom > 0 && intScaleTo > 0)
                                    {
                                        if ((OutValue >= decSalaryFrom && OutValue <= decSalaryTo)
                                            &&
                                            (EmployeeScale >= intScaleFrom && EmployeeScale <= intScaleTo))
                                        {
                                            SlabCode = OneSlab.Code;
                                            decPercentage = Convert.ToDecimal(OneSlab.BonusPercentage);
                                            decMinimumMonthsDuration = Convert.ToDecimal(OneSlab.MinimumMonthsDuration);
                                            ElementType = Convert.ToInt32(OneSlab.ElementType);
                                            Range = Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryFrom), 0)) + " - " + Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryTo), 0));
                                            break;
                                        }
                                    }
                                    else if (decSalaryFrom == 0 && decSalaryTo == 0 && intScaleFrom > 0 && intScaleTo > 0)
                                    {
                                        if (EmployeeScale >= intScaleFrom && EmployeeScale <= intScaleTo)
                                        {
                                            SlabCode = OneSlab.Code;
                                            decPercentage = Convert.ToDecimal(OneSlab.BonusPercentage);
                                            decMinimumMonthsDuration = Convert.ToDecimal(OneSlab.MinimumMonthsDuration);
                                            ElementType = Convert.ToInt32(OneSlab.ElementType);
                                            Range = Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryFrom), 0)) + " - " + Convert.ToString(Math.Round(Convert.ToDecimal(OneSlab.SalaryTo), 0));
                                            break;
                                        }

                                    }
                                }

                            }

                            #region BonusCalculate

                            try
                            {
                                if (ActiveSlab != "False")
                                {
                                    #region Working Days Count
                                    var oPayroll = (from proll in dbHrPayroll.CfgPayrollDefination
                                                    where proll.ID.ToString() == cbPayroll.Value.ToString()
                                                    select proll).FirstOrDefault();

                                    var oPeriod = (from prds in dbHrPayroll.CfgPeriodDates
                                                   where prds.ID.ToString() == cbPeriod.Value.ToString()
                                                   select prds).FirstOrDefault();

                                    var oCalendar = (from a in dbHrPayroll.MstCalendar
                                                     where a.Code == oPeriod.CalCode
                                                     && a.FlgActive == true
                                                     select a).FirstOrDefault();
                                    DaysCount = ds.getDaysCnt(oEmp, oPeriod, out PayDays, out LeaveDays, out MonthDays);
                                    periodDays = Convert.ToInt16(oPayroll.WorkDays);
                                    #endregion

                                    #region Employee Service Duration
                                    DateTime EmployeeDateOfBirth = Convert.ToDateTime(oEmp.JoiningDate);
                                    DateTime effectiveTo = Convert.ToDateTime(oPeriod.StartDate);
                                    EffectiveMonths = ((effectiveTo.Year - EmployeeDateOfBirth.Year) * 12) + effectiveTo.Month - EmployeeDateOfBirth.Month;
                                    int month = EmployeeDateOfBirth.Month;
                                    int year = EmployeeDateOfBirth.Year;
                                    int Peridmonth = effectiveTo.Month;
                                    int Periodyear = effectiveTo.Year;
                                    int intdaysIneffectiveMonth = System.DateTime.DaysInMonth(year, month);
                                    int intdaysInPeriodMonth = System.DateTime.DaysInMonth(Periodyear, Peridmonth);
                                    DateTime monthEndDate = new DateTime(EmployeeDateOfBirth.Year, EmployeeDateOfBirth.Month, intdaysIneffectiveMonth);
                                    GetPartialMonth = (monthEndDate - EmployeeDateOfBirth).Days + 1;
                                    decimal decMonthCount = GetPartialMonth / intdaysIneffectiveMonth;
                                    EffectiveMonths = EffectiveMonths - 1;
                                    EffectiveMonths = EffectiveMonths + decMonthCount;
                                    #endregion

                                    if (periodDays > 0)
                                    {
                                        PresentDays = periodDays;
                                    }
                                    else
                                    {
                                        PresentDays = DaysCount;
                                    }
                                    if (decPercentage > 0)
                                    {
                                        PresentEarnedSalary = (OutValue / intdaysInPeriodMonth) * DaysCount;
                                        EarnedSalary = (PresentEarnedSalary / 100) * decPercentage;
                                    }
                                    else
                                    {
                                        EarnedSalary = (OutValue / intdaysInPeriodMonth) * DaysCount;
                                    }

                                    if (decMinimumMonthsDuration > 0)
                                    {
                                        if (EffectiveMonths >= decMinimumMonthsDuration)
                                        {
                                            decimal TotalAmount = 0M;
                                            foreach (TrnsEmployeeElementDetail ele in oEmp.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                                            {
                                                if (((bool)ele.MstElements.FlgEffectOnGross))
                                                {
                                                    string elementName = "";
                                                    decimal OriginalElementAmount = 0.0M;
                                                    decimal OneMonthElementAmount = 0.0M;
                                                    var EarningElement = (from a in dbHrPayroll.MstElementEarning where a.ElementID == ele.MstElements.Id && a.ValueType != "FIX" && ele.MstElements.Type != "Non-Rec" select a).FirstOrDefault();
                                                    if (ele.MstElements.ElmtType == "Ear" && EarningElement != null && ele.MstElements.FlgEmployeeBonus == true)
                                                    {
                                                        elementName = ele.MstElements.Description;
                                                        OriginalElementAmount = Convert.ToDecimal(ele.Amount);


                                                        if (OriginalElementAmount > 0 && decPercentage > 0)
                                                        {
                                                            if (EarningElement.FlgVariableValue == true)
                                                            {
                                                                decimal PeresentDaysAmount = (Convert.ToDecimal(OriginalElementAmount) / intdaysInPeriodMonth) * Convert.ToDecimal(DaysCount);
                                                                OneMonthElementAmount = (Convert.ToDecimal(PeresentDaysAmount) / 100) * Convert.ToDecimal(decPercentage);
                                                            }
                                                            else
                                                            {
                                                                OneMonthElementAmount = (Convert.ToDecimal(OriginalElementAmount) / 100) * Convert.ToDecimal(decPercentage);
                                                            }
                                                        }
                                                        else if (OriginalElementAmount > 0 && decPercentage == 0)
                                                        {
                                                            if (EarningElement.FlgVariableValue == true)
                                                            {
                                                                OneMonthElementAmount = (Convert.ToDecimal(OriginalElementAmount) / 100) * Convert.ToDecimal(DaysCount);
                                                            }
                                                            else
                                                            {
                                                                OneMonthElementAmount = Convert.ToDecimal(OriginalElementAmount);
                                                            }
                                                        }
                                                        if (OneMonthElementAmount > 0)
                                                        {
                                                            TotalAmount = TotalAmount + OneMonthElementAmount;
                                                        }
                                                    }
                                                }
                                            }
                                            decimal getBasic = oEmp.BasicSalary.Value;
                                            CalculatedNetAmount = EarnedSalary + TotalAmount;

                                            #region Set Value on Grid

                                            (mtEmp.Columns.Item("clSlabCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = SlabCode.ToString();
                                            (mtEmp.Columns.Item("clRange").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Range.ToString();
                                            (mtEmp.Columns.Item("clPerc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(decPercentage), 2).ToString();
                                            (mtEmp.Columns.Item("clNetSal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(PresentEarnedSalary), 2).ToString();
                                            (mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Math.Round(Convert.ToDecimal(CalculatedNetAmount), 2).ToString();

                                            Boolean flgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked = true;
                                            #endregion
                                            //}
                                        }
                                    }

                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Fall in Slab: '" + SlabCode + "' but Slab is not Active. ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                            #endregion
                        }

                    }
                }
                IbtCalculations.Enabled = false;
            }
            catch (Exception ex)
            {

            }

        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            IniContrls();
        }

        private void GetNextRecord()
        {
            try
            {
                var Records = dbHrPayroll.TrnsEmployeeBonus.ToList();
                if (Records != null && Records.Count > 0)
                {
                    TotalRecords = Records.Count;
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
                var Records = dbHrPayroll.TrnsEmployeeBonus.ToList();
                if (Records != null && Records.Count > 0)
                {
                    TotalRecords = Records.Count;
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
            btSave.Caption = "Update";
            try
            {
                if (cbStatus.Value.Trim() == "0")
                {
                    IbtCalculations.Enabled = true;
                }
                else
                {
                    IbtCalculations.Enabled = false;
                }

                BonusPaymentBatch = dbHrPayroll.TrnsEmployeeBonus.ToList();
                TrnsEmployeeBonus record = BonusPaymentBatch.ElementAt<TrnsEmployeeBonus>(DocumentID);
                txDocNum.Value = record.DocumentNo.ToString();
                cbCalendar.Select(record.CalendarID.ToString());
                cbPayroll.Select(record.PayrollID.ToString());
                cbPeriod.Select(record.PaysInPeriodID.ToString());
                var oElement = (from a in dbHrPayroll.MstElements where a.Id == record.ElementType select a).FirstOrDefault();
                cbElementType.Select(oElement.ElementName.ToString());
                cbStatus.Select(record.Status.ToString());

                if (record.Status.ToString().Trim() == "0")
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    btSave.Caption = "Update";
                    IbtProcess.Enabled = true;
                }
                else
                {
                    IbtProcess.Enabled = false;
                }

                dtEmps.Rows.Clear();

                var oDetail = record.TrnsEmployeeBonusDetail.ToList();
                if (oDetail != null && oDetail.Count > 0)
                {
                    dtEmps.Rows.Clear();
                    dtEmps.Rows.Add(oDetail.Count);
                    foreach (var OneValue in oDetail)
                    {
                        var oEmp = (from a in dbHrPayroll.MstEmployee where a.ID == OneValue.EmployeeID select a).FirstOrDefault();
                        dtEmps.SetValue("id", i, OneValue.Id.ToString());
                        dtEmps.SetValue("EmpID", i, oEmp.EmpID);
                        dtEmps.SetValue("EName", i, OneValue.EmployeeName);
                        dtEmps.SetValue("BSal", i, OneValue.BasicSalary.ToString());
                        dtEmps.SetValue("GSal", i, OneValue.GrossSalary.ToString());
                        dtEmps.SetValue("SlabCode", i, OneValue.SlabCode.ToString());
                        dtEmps.SetValue("SalaryRange", i, OneValue.SalaryRange.ToString());
                        dtEmps.SetValue("Percentage", i, OneValue.Percentage.ToString());
                        dtEmps.SetValue("NetSalary", i, OneValue.NetSalary.ToString());
                        dtEmps.SetValue("NetAmount", i, OneValue.CalculatedAmount.ToString());

                        dtEmps.SetValue("Active", i, OneValue.FlgActive == true ? "Y" : "N");

                        i++;
                    }
                    mtEmp.LoadFromDataSource();
                    mtEmp.AutoResizeColumns();
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillDocumentAfterSave(Int32 DocumentID)
        {
            Int16 i = 0;
            btSave.Caption = "Update";
            try
            {
                if (cbStatus.Value.Trim() == "0")
                {
                    IbtCalculations.Enabled = true;
                }
                else
                {
                    IbtCalculations.Enabled = false;
                }

                BonusPaymentBatch = dbHrPayroll.TrnsEmployeeBonus.ToList();

                var record = (from a in dbHrPayroll.TrnsEmployeeBonus where a.DocumentNo == DocumentID select a).FirstOrDefault();
                txDocNum.Value = record.DocumentNo.ToString();
                cbCalendar.Select(record.CalendarID.ToString());
                cbPayroll.Select(record.PayrollID.ToString());
                cbPeriod.Select(record.PaysInPeriodID.ToString());
                var oElement = (from a in dbHrPayroll.MstElements where a.Id == record.ElementType select a).FirstOrDefault();
                cbElementType.Select(oElement.ElementName.ToString());
                cbStatus.Select(record.Status.ToString());

                if (record.Status.ToString().Trim() == "0")
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    btSave.Caption = "Update";
                    IbtProcess.Enabled = true;
                }
                else
                {
                    IbtProcess.Enabled = false;
                }

                dtEmps.Rows.Clear();

                var oDetail = record.TrnsEmployeeBonusDetail.ToList();
                if (oDetail != null && oDetail.Count > 0)
                {
                    dtEmps.Rows.Clear();
                    dtEmps.Rows.Add(oDetail.Count);
                    foreach (var OneValue in oDetail)
                    {
                        var oEmp = (from a in dbHrPayroll.MstEmployee where a.ID == OneValue.EmployeeID select a).FirstOrDefault();
                        dtEmps.SetValue("id", i, OneValue.Id.ToString());
                        dtEmps.SetValue("EmpID", i, oEmp.EmpID);
                        dtEmps.SetValue("EName", i, OneValue.EmployeeName);
                        dtEmps.SetValue("BSal", i, OneValue.BasicSalary.ToString());
                        dtEmps.SetValue("GSal", i, OneValue.GrossSalary.ToString());
                        dtEmps.SetValue("SlabCode", i, OneValue.SlabCode.ToString());
                        dtEmps.SetValue("SalaryRange", i, OneValue.SalaryRange.ToString());
                        dtEmps.SetValue("Percentage", i, OneValue.Percentage.ToString());
                        dtEmps.SetValue("NetSalary", i, OneValue.NetSalary.ToString());
                        dtEmps.SetValue("NetAmount", i, OneValue.CalculatedAmount.ToString());

                        dtEmps.SetValue("Active", i, OneValue.FlgActive == true ? "Y" : "N");

                        i++;
                    }
                    mtEmp.LoadFromDataSource();
                    mtEmp.AutoResizeColumns();
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void SubmitForm()
        {
            string strProcessing = "";
            DateTime DocDate = DateTime.MinValue;
            DateTime DurationDate = DateTime.MinValue;

            try
            {
                if (string.IsNullOrEmpty(cbPeriod.Value.Trim()) || cbPeriod.Selected.Value.Trim() == "0")
                {
                    oApplication.StatusBar.SetText("Please select valid Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (string.IsNullOrEmpty(cbElementType.Value.Trim()) || cbElementType.Value.Trim() == "-1")
                {
                    oApplication.StatusBar.SetText("Please select Element Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                mtEmp.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                CfgPayrollDefination oPayroll = (from p in dbHrPayroll.CfgPayrollDefination
                                                 where p.ID.ToString() == cbPayroll.Value.ToString()
                                                 select p).FirstOrDefault();

                CfgPeriodDates oPayrollPeriod = (from prds in dbHrPayroll.CfgPeriodDates
                                                 where prds.ID.ToString() == cbPeriod.Value.ToString()
                                                 select prds).FirstOrDefault();

                MstCalendar oCalenar = (from c in dbHrPayroll.MstCalendar
                                        where c.Code == oPayrollPeriod.CalCode
                                        && c.FlgActive == true
                                        select c).FirstOrDefault();

                TrnsEmployeeBonus BonusPayment;
                int cnt = (from p in dbHrPayroll.TrnsEmployeeBonus
                           where p.DocumentNo.ToString() == txDocNum.Value.ToString()
                           select p).Count();
                if (cnt == 0)
                {
                    BonusPayment = new TrnsEmployeeBonus();
                    long nextId = 0;

                    nextId = ds.getNextId("TrnsEmployeeBonus", "ID");
                    BonusPayment.DocumentNo = Convert.ToInt32(nextId);
                    BonusPayment.CalendarID = Convert.ToInt32(oCalenar.Id);
                    BonusPayment.PayrollID = Convert.ToInt32(oPayroll.ID);
                    BonusPayment.PaysInPeriodID = Convert.ToInt32(oPayrollPeriod.ID);
                    var Records = (from v in dbHrPayroll.MstElements
                                   where v.FlgEmployeeBonus == true
                                   && v.ElementName == cbElementType.Value.Trim()
                                   //&& v.Type == "Non-Rec"
                                   select v).FirstOrDefault();
                    BonusPayment.ElementType = Convert.ToInt32(Records.Id);
                    BonusPayment.Status = cbStatus.Value.ToString().Trim();
                    BonusPayment.CreatedDate = DateTime.Now;
                    BonusPayment.CreatedBy = oCompany.UserName;

                    dbHrPayroll.TrnsEmployeeBonus.InsertOnSubmit(BonusPayment);

                }
                else
                {
                    BonusPayment = (from p in dbHrPayroll.TrnsEmployeeBonus
                                    where p.Id.ToString() == txDocNum.Value.ToString()
                                    select p).FirstOrDefault();
                    BonusPayment.UpdatedDate = DateTime.Now;
                    BonusPayment.UpdatedBy = oCompany.UserName;
                }

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {

                    code = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    code = code.Trim();
                    if (code != "")
                    {

                        Boolean flgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        decimal NetAmount = Convert.ToDecimal((mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                        if (flgActive == true && NetAmount > 0)
                        {

                            TrnsEmployeeBonusDetail BonusPaymentDettail;
                            //string strdetailId = (mtEmp.Columns.Item("id").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            //int detailId = Convert.ToInt32(strdetailId);
                            MstEmployee emp = (from p in dbHrPayroll.MstEmployee
                                               where p.EmpID == code
                                               && p.FlgActive == true
                                               && p.PayrollID == oPayroll.ID
                                               select p).FirstOrDefault();
                            if (emp == null)
                            {
                                oApplication.StatusBar.SetText("Employee with EmpId " + code + " not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                continue;
                            }
                            if (cnt == 0)
                            {
                                BonusPaymentDettail = new TrnsEmployeeBonusDetail();
                                BonusPayment.TrnsEmployeeBonusDetail.Add(BonusPaymentDettail);
                                BonusPaymentDettail.EmployeeID = emp.ID;
                                BonusPaymentDettail.EmployeeName = (mtEmp.Columns.Item("clEName").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value; //dtEmps.GetValue("EName", i);
                                BonusPaymentDettail.BasicSalary = emp.BasicSalary;
                                BonusPaymentDettail.GrossSalary = emp.GrossSalary;
                                BonusPaymentDettail.SlabCode = Convert.ToString((mtEmp.Columns.Item("clSlabCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                BonusPaymentDettail.SalaryRange = Convert.ToString((mtEmp.Columns.Item("clRange").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                BonusPaymentDettail.Percentage = Convert.ToDecimal((mtEmp.Columns.Item("clPerc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                BonusPaymentDettail.NetSalary = (int)Convert.ToDecimal((mtEmp.Columns.Item("clNetSal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                BonusPaymentDettail.CalculatedAmount = (int)Convert.ToDecimal((mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);

                                BonusPaymentDettail.FlgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                                BonusPaymentDettail.CreatedDate = DateTime.Now;
                                BonusPaymentDettail.CreatedBy = oCompany.UserName;
                            }
                            else
                            {
                                BonusPaymentDettail = (from a in dbHrPayroll.TrnsEmployeeBonusDetail
                                                       where a.FKID == BonusPayment.Id
                                                       && a.EmployeeID == emp.ID
                                                       && BonusPayment.PaysInPeriodID == Convert.ToInt32(cbPeriod.Value.Trim())
                                                       select a).FirstOrDefault();

                                if (BonusPaymentDettail != null)
                                {
                                    BonusPaymentDettail.EmployeeID = emp.ID;
                                    BonusPaymentDettail.EmployeeName = (mtEmp.Columns.Item("clEName").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value; //dtEmps.GetValue("EName", i);
                                    BonusPaymentDettail.BasicSalary = emp.BasicSalary;
                                    BonusPaymentDettail.GrossSalary = emp.GrossSalary;
                                    BonusPaymentDettail.SlabCode = Convert.ToString((mtEmp.Columns.Item("clSlabCode").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    BonusPaymentDettail.SalaryRange = Convert.ToString((mtEmp.Columns.Item("clRange").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    BonusPaymentDettail.Percentage = Convert.ToDecimal((mtEmp.Columns.Item("clPerc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    BonusPaymentDettail.NetSalary = (int)Convert.ToDecimal((mtEmp.Columns.Item("clNetSal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    BonusPaymentDettail.CalculatedAmount = (int)Convert.ToDecimal((mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);

                                    bool flgcheckActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                                    BonusPaymentDettail.FlgActive = flgcheckActive;
                                    BonusPaymentDettail.UpdatedDate = DateTime.Now;
                                    BonusPaymentDettail.UpdatedBy = oCompany.UserName;
                                }

                            }
                        }
                    }
                }

                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                dbHrPayroll.SubmitChanges();
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    AddNewRecord();
                }
                else
                {
                    //FillDocument(doc)
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(strProcessing + "Error in Function submitForm.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void PostPaymentOld(Int32 DocumentID)
        {
            DateTime DocDate = DateTime.MinValue;
            DateTime DurationDate = DateTime.MinValue;

            int confirm = oApplication.MessageBox("Do you want to Post selected employees?", 1, "Yes", "No");
            if (confirm == 2)
            {
                oApplication.StatusBar.SetText("Posting canceled", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return;
            }

            try
            {

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {

                    string strEmpID = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (strEmpID.Trim() != "")
                    {
                        MstEmployee oEmp = (from p in dbHrPayroll.MstEmployee
                                            where p.EmpID == strEmpID
                                            && p.FlgActive == true
                                            select p).FirstOrDefault();
                        if (oEmp == null)
                        {
                            oApplication.StatusBar.SetText("Employee with EmpId " + strEmpID + " not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            continue;
                        }
                        var getUser = oEmp.MstUsers.FirstOrDefault();
                        int oElement = (from e in dbHrPayroll.MstElements
                                        where e.FlgEmployeeBonus == true
                                        && e.Type == "Rec"
                                        select e).Count();
                        if (oElement != 0)
                        {
                            var TargetedDocuments = (from a in dbHrPayroll.TrnsEmployeeBonus where a.DocumentNo == DocumentID select a).FirstOrDefault();

                            var BonusPaymentDetails = (from p in dbHrPayroll.TrnsEmployeeBonusDetail
                                                       where p.FKID == TargetedDocuments.Id
                                                       && p.EmployeeID == oEmp.ID
                                                       && p.TrnsEmployeeBonus.CalendarID == Convert.ToInt32(cbCalendar.Value.Trim())
                                                       && p.TrnsEmployeeBonus.PayrollID == Convert.ToInt32(cbPayroll.Value.Trim())
                                                       && p.TrnsEmployeeBonus.PaysInPeriodID == Convert.ToInt32(cbPeriod.Value.Trim())

                                                       select p).FirstOrDefault();

                            Boolean flgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            decimal approvedBonus = Convert.ToDecimal((mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                            if (BonusPaymentDetails.FlgActive == true && approvedBonus > 0)
                            {

                                TrnsEmployeeElement oEmpEle = (from a in dbHrPayroll.TrnsEmployeeElement
                                                               where a.MstEmployee.EmpID == oEmp.EmpID
                                                               select a).FirstOrDefault();
                                if (oEmpEle == null) return;
                                MstElements oElementType = (from a in dbHrPayroll.MstElements
                                                            where a.Id == TargetedDocuments.ElementType
                                                            select a).FirstOrDefault();
                                if (oElementType == null) return;
                                MstElements oElementPaysThrough = (from a in dbHrPayroll.MstElements
                                                                   where a.Id == TargetedDocuments.ElementType
                                                                   select a).FirstOrDefault();
                                if (oElementPaysThrough == null) return;

                                int chkDetail = 0;
                                chkDetail = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                             where a.TrnsEmployeeElement.Id == oEmpEle.Id
                                             && a.PeriodId == TargetedDocuments.PaysInPeriodID
                                             && a.MstElements.Id == oElementPaysThrough.Id
                                             select a).Count();
                                TrnsEmployeeElementDetail oDoc = new TrnsEmployeeElementDetail();
                                var oElementEar = (from a in dbHrPayroll.MstElementEarning
                                                   where a.ElementID == oElementType.Id
                                                   select a).FirstOrDefault();
                                if (oElementEar == null) return;
                                string Elementname = oElementType.ElementName;
                                if (chkDetail == 0)
                                {
                                    oDoc = new TrnsEmployeeElementDetail();
                                    oEmpEle.TrnsEmployeeElementDetail.Add(oDoc);
                                }
                                else
                                {
                                    oDoc = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                            where a.TrnsEmployeeElement.Id == oEmpEle.Id
                                            && a.PeriodId == TargetedDocuments.PaysInPeriodID
                                            && a.MstElements.Id == oElementPaysThrough.Id
                                            select a).FirstOrDefault();
                                }
                                oDoc.MstElements = oElementPaysThrough;
                                oDoc.StartDate = oElementPaysThrough.StartDate;
                                oDoc.EndDate = oElementPaysThrough.EndDate;
                                oDoc.FlgRetro = false;
                                oDoc.RetroAmount = 0M;
                                oDoc.FlgActive = true;
                                oDoc.FlgOneTimeConsumed = false;
                                oDoc.PeriodId = TargetedDocuments.PaysInPeriodID;
                                oDoc.ElementType = oElementPaysThrough.ElmtType;
                                oDoc.ValueType = oElementPaysThrough.MstElementEarning[0].ValueType;
                                oElementPaysThrough.MstElementEarning[0].Value = BonusPaymentDetails.CalculatedAmount;
                                oDoc.Value = oElementPaysThrough.MstElementEarning[0].Value;
                                oDoc.Amount = 0M;
                                oDoc.EmpContr = 0M;
                                oDoc.EmplrContr = 0M;
                                dbHrPayroll.SubmitChanges();

                            }
                        }
                    }
                }
                var record = (from a in dbHrPayroll.TrnsEmployeeBonus where a.DocumentNo == DocumentID select a).FirstOrDefault();
                record.Status = "2";
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function processBatch.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            FillDocumentAfterSave(DocumentID);
        }

        private void PostPayment(Int32 DocumentID)
        {
            DateTime DocDate = DateTime.MinValue;
            DateTime DurationDate = DateTime.MinValue;

            int confirm = oApplication.MessageBox("Do you want to Post selected employees?", 1, "Yes", "No");
            if (confirm == 2)
            {
                oApplication.StatusBar.SetText("Posting canceled", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return;
            }

            try
            {

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {

                    string strEmpID = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (strEmpID.Trim() != "")
                    {
                        MstEmployee oEmp = (from p in dbHrPayroll.MstEmployee
                                            where p.EmpID == strEmpID
                                            && p.FlgActive == true
                                            select p).FirstOrDefault();
                        if (oEmp == null)
                        {
                            oApplication.StatusBar.SetText("Employee with EmpId " + strEmpID + " not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            continue;
                        }
                        var getUser = oEmp.MstUsers.FirstOrDefault();
                        int oElement = (from e in dbHrPayroll.MstElements
                                        where e.FlgEmployeeBonus == true                                        
                                        select e).Count();
                        if (oElement != 0)
                        {
                            var TargetedDocuments = (from a in dbHrPayroll.TrnsEmployeeBonus where a.DocumentNo == DocumentID select a).FirstOrDefault();

                            var BonusPaymentDetails = (from p in dbHrPayroll.TrnsEmployeeBonusDetail
                                                       where p.FKID == TargetedDocuments.Id
                                                       && p.EmployeeID == oEmp.ID
                                                       && p.TrnsEmployeeBonus.CalendarID == Convert.ToInt32(cbCalendar.Value.Trim())
                                                       && p.TrnsEmployeeBonus.PayrollID == Convert.ToInt32(cbPayroll.Value.Trim())
                                                       && p.TrnsEmployeeBonus.PaysInPeriodID == Convert.ToInt32(cbPeriod.Value.Trim())

                                                       select p).FirstOrDefault();

                            Boolean flgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                            decimal approvedBonus = Convert.ToDecimal((mtEmp.Columns.Item("clNetAmt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                            if (BonusPaymentDetails.FlgActive == true && approvedBonus > 0)
                            {

                                TrnsEmployeeElement oEmpEle = (from a in dbHrPayroll.TrnsEmployeeElement
                                                               where a.MstEmployee.EmpID == oEmp.EmpID
                                                               select a).FirstOrDefault();
                                if (oEmpEle == null) return;
                                MstElements oElementType = (from a in dbHrPayroll.MstElements
                                                            where a.Id == TargetedDocuments.ElementType
                                                            select a).FirstOrDefault();
                                if (oElementType == null) return;
                                MstElements oElementPaysThrough = (from a in dbHrPayroll.MstElements
                                                                   where a.Id == TargetedDocuments.ElementType
                                                                   select a).FirstOrDefault();
                                if (oElementPaysThrough == null) return;

                                int chkDetail = 0;
                                chkDetail = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                             where a.TrnsEmployeeElement.Id == oEmpEle.Id
                                             && a.PeriodId == TargetedDocuments.PaysInPeriodID
                                             && a.MstElements.Id == oElementPaysThrough.Id
                                             select a).Count();
                                TrnsEmployeeElementDetail oDoc = new TrnsEmployeeElementDetail();
                                var oElementEar = (from a in dbHrPayroll.MstElementEarning
                                                   where a.ElementID == oElementType.Id
                                                   select a).FirstOrDefault();
                                if (oElementEar == null) return;
                                string Elementname = oElementType.ElementName;
                                if (chkDetail == 0)
                                {
                                    oDoc = new TrnsEmployeeElementDetail();
                                    oEmpEle.TrnsEmployeeElementDetail.Add(oDoc);
                                }
                                else
                                {
                                    oDoc = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                            where a.TrnsEmployeeElement.Id == oEmpEle.Id
                                            && a.PeriodId == TargetedDocuments.PaysInPeriodID
                                            && a.MstElements.Id == oElementPaysThrough.Id
                                            select a).FirstOrDefault();
                                }
                                oDoc.MstElements = oElementPaysThrough;
                                oDoc.StartDate = oElementPaysThrough.StartDate;
                                oDoc.EndDate = oElementPaysThrough.EndDate;
                                oDoc.FlgRetro = false;
                                oDoc.RetroAmount = 0M;
                                oDoc.FlgActive = true;
                                if (oElementPaysThrough.Type == "Non-Rec")
                                {
                                    oDoc.FlgOneTimeConsumed = false;
                                    oDoc.PeriodId = TargetedDocuments.PaysInPeriodID;
                                }
                                
                                oDoc.ElementType = oElementPaysThrough.ElmtType;
                                oDoc.ValueType = oElementPaysThrough.MstElementEarning[0].ValueType;
                                oElementPaysThrough.MstElementEarning[0].Value = BonusPaymentDetails.CalculatedAmount;
                                oDoc.Value = oElementPaysThrough.MstElementEarning[0].Value;
                                oDoc.Amount = 0M;
                                oDoc.EmpContr = 0M;
                                oDoc.EmplrContr = 0M;
                                dbHrPayroll.SubmitChanges();

                            }
                        }
                    }
                }
                var record = (from a in dbHrPayroll.TrnsEmployeeBonus where a.DocumentNo == DocumentID select a).FirstOrDefault();
                record.Status = "2";
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function processBatch.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            FillDocumentAfterSave(DocumentID);
        }
        #endregion
    }

}
