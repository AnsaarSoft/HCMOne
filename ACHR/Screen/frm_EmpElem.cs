using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_EmpElem : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.Matrix mtElement, mtLoan, mtAdvance;
        SAPbouiCOM.Column isNew, id;
        private SAPbouiCOM.DataTable dtAdvance, dtElements, dtLoans, dtHead;
        SAPbouiCOM.EditText txCode, txName, txHRMSId, txBS, txGross;
        SAPbouiCOM.Button btId;
        SAPbouiCOM.Item ItxCode, ItxName, ItxHRMSId, ItxBS, ItxGross, ibtId;
        System.Data.DataTable dtSearch = new System.Data.DataTable();
        SAPbouiCOM.OptionBtn optbtn;
        private int RoundingSet = 0;
        SAPbouiCOM.ChooseFromList oCfl;
        MstEmployee emp;
        string selEmpId = "";


        public IEnumerable<MstAdvance> advances;
        public IEnumerable<MstLoans> loans;
        public IEnumerable<MstElements> elements;
        public string SelectedEmp = "";
        decimal grossSalary = 0.00M;
        decimal basicSalary = 0.00M;

        #endregion

        #region B1 Event

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);

            InitiallizeForm();
            oForm.EnableMenu("1282", false);  // Add New Record
            oForm.EnableMenu("1288", false);  // Next Record
            oForm.EnableMenu("1289", false);  // Pevious Record
            oForm.EnableMenu("1290", false);  // First Record
            oForm.EnableMenu("1291", false);  // Last record 

            oForm.Freeze(false);

        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txHRMSId == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txHRMSId.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }
        }

        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {

            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            oForm.Freeze(true);
            switch (pVal.ItemUID)
            {
                case "1":
                    doSubmit();
                    break;
                case "btId":
                    OpenNewSearchForm();
                    break;
                case "mtElement":
                    if (pVal.ColUID == "pickEle")
                    {
                        int rowNum = pVal.Row;
                        if (rowNum <= dtElements.Rows.Count)
                        {
                            picElement(rowNum);
                        }
                    }
                    break;
                case "optR":
                    fillMat(selEmpId);
                    break;
                case "optNR":
                    fillMat(selEmpId);
                    break;
            }
            oForm.Freeze(false);
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();

            txHRMSId.Value = "";
            txName.Value = "";
            txCode.Value = "";
            ItxHRMSId.Enabled = true;
            ItxName.Enabled = true;
            txHRMSId.Active = true;
            OpenNewSearchForm();
        }
        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            //EachItemshould be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the controll object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */

            //, , , , txGrtA, txITE, txBasicP, txArrP, txLvP, txEOSP, txGrtP, txITP;
            //string ReplaceParameter = ACHR.Properties.Settings.Default.EmpValueReplaceID;
            if (ACHR.Properties.Settings.Default.RoundingValue == "Yes")
            {
                RoundingSet = 1;
            }
            else
            {
                RoundingSet = 0;
            }

            oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txCode = oForm.Items.Item("txCode").Specific;
            ItxCode = oForm.Items.Item("txCode");
            txCode.DataBind.SetBound(true, "", "txCode");

            oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80); // Days of Month
            txName = oForm.Items.Item("txName").Specific;
            ItxName = oForm.Items.Item("txName");
            txName.DataBind.SetBound(true, "", "txName");


            oForm.DataSources.UserDataSources.Add("txHRMSId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txHRMSId = oForm.Items.Item("txHRMSId").Specific;
            ItxHRMSId = oForm.Items.Item("txHRMSId");
            txHRMSId.DataBind.SetBound(true, "", "txHRMSId");

            oForm.DataSources.UserDataSources.Add("txGross", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txGross = oForm.Items.Item("txGross").Specific;
            ItxGross = oForm.Items.Item("txGross");
            txGross.DataBind.SetBound(true, "", "txGross");

            oForm.DataSources.UserDataSources.Add("txBS", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txBS = oForm.Items.Item("txBS").Specific;
            ItxBS = oForm.Items.Item("txBS");
            txBS.DataBind.SetBound(true, "", "txBS");


            mtAdvance = oForm.Items.Item("mtAdvance").Specific;
            isNew = mtAdvance.Columns.Item("isNew");
            id = mtAdvance.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtLoan = oForm.Items.Item("mtLoan").Specific;
            isNew = mtLoan.Columns.Item("isNew");
            id = mtLoan.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtElement = oForm.Items.Item("mtElement").Specific;


            isNew = mtElement.Columns.Item("isNew");
            id = mtElement.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;



            dtAdvance = oForm.DataSources.DataTables.Item("dtAdvance");
            dtAdvance.Rows.Clear();

            dtElements = oForm.DataSources.DataTables.Item("dtElements");
            dtElements.Rows.Clear();

            SAPbouiCOM.Column pickEle = mtElement.Columns.Item("pickEle");

            dtLoans = oForm.DataSources.DataTables.Item("dtLoans");
            dtLoans.Rows.Clear();
            fillColumCombo("Val_Type", mtElement.Columns.Item("ValType"));

            //oCfl = oForm.ChooseFromLists.Item("OHEM");
            dtHead = oForm.DataSources.DataTables.Item("dtHead");
            dtHead.Rows.Add(1);
            optbtn = oForm.Items.Item("optR").Specific;
            optbtn.GroupWith("optNR");
            dtHead.SetValue("optRecurring", 0, "Y");
            optbtn.Selected = true;
            //, , , , , , 
            /*
            fillMat();
            _fillFields();
            */
            oForm.DefButton = "1";

            oForm.PaneLevel = 1;
            optbtn.Selected = true;
            // Program.objHrmsUI.loadHrmsEmps(oCfl);
            oForm.Freeze(false);

        }

        private void picEmp()
        {
            try
            {
                PrepareSearchKeyHash();
                string strSql = sqlString.getSql("empElement", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for elements");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    txHRMSId.Value = st.Rows[0][0].ToString();
                    selEmpId = txHRMSId.Value.ToString().Trim();
                    fillMat(Convert.ToString(txHRMSId.Value.ToString()));
                }
            }


            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txHRMSId.Value = Program.EmpID;
                    selEmpId = txHRMSId.Value.ToString().Trim();
                    fillMat(Convert.ToString(txHRMSId.Value.ToString()));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void fillPeriods()
        {
            try
            {
                CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == emp.PayrollID.ToString() select p).FirstOrDefault();

                if (pr != null)
                {
                    SAPbouiCOM.Column cbCol = mtElement.Columns.Item("cpayroll");
                    if (cbCol.ValidValues.Count > 0)
                    {
                        int vcnt = cbCol.ValidValues.Count;
                        for (int k = vcnt - 1; k >= 0; k--)
                        {
                            cbCol.ValidValues.Remove(cbCol.ValidValues.Item(k).Value);
                        }
                    }

                    cbCol.ValidValues.Add("-1", "");
                    foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    {
                        cbCol.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                    }
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void picElement(int rowNum)
        {

            picker pic = new picker(oApplication, ds.getValidEmpElement(txHRMSId.Value, ""));
            System.Data.DataTable st = pic.ShowInput("Select Element", "Select Element for Employee");
            pic = null;
            if (st.Rows.Count > 0)
            {
                string strRepeat = st.Rows[0][4].ToString();
                string elementName = st.Rows[0][1].ToString();
                string id = st.Rows[0][0].ToString();

                if (strRepeat == "False" && alreadyAssignedElement(elementName))
                {
                    oApplication.SetStatusBarMessage("Element can not be repeated");
                    return;
                }
                setElementInfo(elementName, rowNum - 1, id);
            }
        }

        private bool alreadyAssignedElement(string element)
        {
            bool exist = false;
            int cnt = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.MstElements.ElementName == element && p.TrnsEmployeeElement.MstEmployee.EmpID == txHRMSId.Value.ToString().Trim() select p).Count();
            if (cnt > 0) exist = true; else exist = false;
            return exist;

        }

        private System.Data.DataTable getValidEmpElement(string empid, string periodId)
        {
            string strSql = "SELECT    MstElementLink.ID, dbo.MstElements.ElementName, dbo.MstElements.Description, dbo.MstElements.Type , dbo.MstElementEarning.flgMultipleEntryAllowed AS 'Allowed Multiple' ";
            strSql += "  FROM         dbo.MstEmployee INNER JOIN ";
            strSql += "                   dbo.CfgPayrollDefination ON dbo.MstEmployee.PayrollID = dbo.CfgPayrollDefination.ID INNER JOIN ";
            strSql += "                     dbo.MstElementLink ON dbo.CfgPayrollDefination.ID = dbo.MstElementLink.PayrollID INNER JOIN";
            strSql += "                    dbo.MstElements ON dbo.MstElementLink.ElementID = dbo.MstElements.Id LEFT OUTER JOIN ";
            strSql += "                    dbo.MstElementEarning ON dbo.MstElements.Id = dbo.MstElementEarning.ElementID  ";
            strSql += " WHERE      (dbo.MstEmployee.EmpID = '" + empid + "')";
            System.Data.DataTable dtValidElement = ds.getDataTable(strSql);
            return dtValidElement;
        }

        private void setElementInfo(string ele, int rowNum, string linkId)
        {
            if (emp == null) return;
            int cnt = (from p in dbHrPayroll.MstElements where p.ElementName == ele select p).Count();
            if (cnt > 0)
            {

                string strImg = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\CFL.BMP";

                MstElementLink empelement = (from p in dbHrPayroll.MstElementLink where p.ID.ToString() == linkId.ToString() && p.MstElements.ElementName == ele && p.PayrollID == emp.PayrollID select p).Single();
                // dtElements.SetValue("isNew", rowNum, "Y");
                dtElements.SetValue("id", rowNum, empelement.MstElements.Id);
                dtElements.SetValue("Element", rowNum, empelement.MstElements.ElementName);
                dtElements.SetValue("Desc", rowNum, empelement.MstElements.Description);
                dtElements.SetValue("Classif", rowNum, empelement.MstElements.ElmtType);
                dtElements.SetValue("EffeDt", rowNum, Convert.ToDateTime(empelement.MstElements.StartDate).ToString("yyyyMMdd"));
                //dtElements.SetValue("Active", rowNum, empelement.FlgActive);

                DIHRMS.Custom.clsElement eleinfo = new DIHRMS.Custom.clsElement(dbHrPayroll, empelement.MstElements);

                dtElements.SetValue("ValType", rowNum, eleinfo.ValueType.ToString());
                dtElements.SetValue("Value", rowNum, eleinfo.Value.ToString());
                //dtElements.SetValue("Amount", rowNum, eleinfo.Amount.ToString());
                if (empelement.MstElements.FlgRemainingAmount == true)
                {
                    decimal BalanceElementAmoun = BalanceAmount(emp.EmpID);
                    dtElements.SetValue("Value", rowNum, Convert.ToString(BalanceElementAmoun));
                    dtElements.SetValue("Amount", rowNum, Convert.ToString(BalanceElementAmoun));
                }
                else
                {
                    dtElements.SetValue("Value", rowNum, eleinfo.Value.ToString());

                    dtElements.SetValue("Amount", rowNum, eleinfo.Amount.ToString());
                }

                try
                {
                    dtElements.SetValue("EndDt", rowNum, Convert.ToDateTime(empelement.MstElements.EndDate).ToString("yyyyMMdd"));
                }
                catch { }
                dtElements.SetValue("RetroAmnt", rowNum, "0.00");
                dtElements.SetValue("RetActive", rowNum, "N");
                if (empelement.FlgActive == true)
                {

                    dtElements.SetValue("Active", rowNum, "Y");
                }
                else
                {
                    dtElements.SetValue("Active", rowNum, "N");
                }
                mtElement.SetLineData(rowNum + 1);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                addEmptyElement();

            }

        }

        private decimal BalanceAmount(string empid)
        {

            var oEmp = (from a in dbHrPayroll.MstEmployee
                        where a.EmpID == empid
                        && a.FlgActive == true
                        select a).FirstOrDefault();

            decimal EmployeeBasicSalary = oEmp.BasicSalary ?? 0;

            decimal EmployeeGrossSalary = oEmp.GrossSalary ?? 0;

            if (EmployeeGrossSalary == 0M)
            {
                oApplication.StatusBar.SetText("Please update Selected Employee '" + empid + "' Gross Salary ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            decimal TotalEarningElementAmount = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                                 where a.TrnsEmployeeElement.EmployeeId == oEmp.ID
                                                 && a.TrnsEmployeeElement.Id == a.EmpElmtId
                                                 && a.ElementId == a.MstElements.Id
                                                 && a.MstElements.FlgRemainingAmount.GetValueOrDefault() == false
                                                 && a.MstElements.FlgEffectOnGross.GetValueOrDefault() == true
                                                 && a.ElementType == "Ear"
                                                 && a.FlgActive == true
                                                 select a.Amount).Sum() ?? 0;
            decimal BalanceElementAmoun = 0M;
            if (TotalEarningElementAmount > 0 && EmployeeGrossSalary > 0)
            {
                BalanceElementAmoun = EmployeeGrossSalary - (TotalEarningElementAmount + EmployeeBasicSalary);
            }
            return BalanceElementAmoun;
        }

        private decimal getElementAmount(MstEmployee emp, int rowNum)
        {
            decimal outValue = Convert.ToDecimal(0.00);
            string valType = dtElements.GetValue("ValType", rowNum);
            decimal Value = Convert.ToDecimal(dtElements.GetValue("Value", rowNum));

            switch (valType.Trim())
            {

                case "POB":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.BasicSalary;
                    break;
                case "POG":
                    //outValue = Convert.ToDecimal(Value) / 100 * (decimal)ds.getEmpGross(emp);
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GrossSalary.GetValueOrDefault();
                    break;
                case "FIX":
                    outValue = Convert.ToDecimal(Value);
                    break;
                case "FGosi%":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalary;
                    break;
                case "VGosi%":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalaryV;
                    break;
            }
            return outValue;
        }

        private void fillMat(string empId)
        {

            try
            {
                if (empId != "")
                {
                    MstEmployee oEmployee = (from a in dbHrPayroll.MstEmployee
                                             where a.EmpID == empId
                                             select a).FirstOrDefault();

                    MstElements oElement = (from a in dbHrPayroll.MstElements
                                            where a.FlgRemainingAmount == true
                                            select a).FirstOrDefault();
                    if (oElement != null)
                    {
                        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail
                                                             where p.TrnsEmployeeElement.EmployeeId == oEmployee.ID
                                                             && p.ElementId == oElement.Id
                                                             select p).FirstOrDefault();
                        if (trntEle != null)
                        {
                            if (trntEle.MstElements.FlgRemainingAmount == true)
                            {
                                decimal BalanceElementAmoun = BalanceAmount(oEmployee.EmpID);
                                trntEle.Value = Convert.ToDecimal(BalanceElementAmoun);
                                trntEle.Amount = Convert.ToDecimal(BalanceElementAmoun);
                                trntEle.UpdateDate = DateTime.Now;
                                trntEle.UpdatedBy = oCompany.UserName;
                            }
                            dbHrPayroll.SubmitChanges();
                        }

                    }
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                txHRMSId.Active = true;
                ItxHRMSId.Enabled = true;
                txName.Active = false;
                SAPbouiCOM.Button bt = oForm.Items.Item("1").Specific;
                dtElements.Rows.Clear();
                ItxName.Enabled = false;

                int cnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == selEmpId select p).Count();
                if (cnt == 0) return;
                emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == selEmpId select p).Single();
                dtHead.SetValue("prId", 0, emp.PayrollID.ToString());
                fillPeriods();
                oForm.DataSources.UserDataSources.Item("txName").ValueEx = emp.FirstName + " " + emp.LastName;
                txHRMSId.Value = emp.EmpID.ToString();
                txName.Value = emp.FirstName + " " + emp.MiddleName + " " + emp.LastName;
                mtElement.Columns.Item("RetroAmnt").Visible = false;
                mtElement.Columns.Item("RetActive").Visible = false;
                if (optbtn.Selected)
                {
                    mtElement.Columns.Item("cpayroll").Visible = false;
                    mtElement.Columns.Item("consumed").Visible = false;
                }
                else
                {
                    mtElement.Columns.Item("cpayroll").Visible = true;
                    mtElement.Columns.Item("consumed").Visible = true;
                }

                txBS.Value = emp.BasicSalary.GetValueOrDefault().ToString();

                decimal Percent = 1;
                //Anas k bhund
                if (false)
                {
                    Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    string SQL = "SELECT TOP 1 \"percentage\" FROM \"FinalEval3\" WHERE \"emp_id\" = " + emp.EmpID + " AND \"year\" = " + DateTime.Now.Year + "AND \"monthid\" = " + DateTime.Now.Month;
                    oRecSet.DoQuery(SQL);
                    if (oRecSet.RecordCount > 0)
                    {
                        Percent = Convert.ToDecimal(oRecSet.Fields.Item(0).Value) / 100;
                    }
                }
                //End of Anas k bhud
                Decimal empGrossSalary = ds.getEmpGross(emp, RoundingSet, 0, Percent);
                //emp.GrossSalary = empGrossSalary;
                txGross.Value = Convert.ToString(empGrossSalary);
                cnt = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empId.ToString() select p).Count();

                if (cnt > 0)
                {
                    TrnsEmployeeElement empEle;
                    empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empId.ToString() select p).Single();
                    txCode.Value = empEle.Id.ToString();
                    dtElements.Rows.Clear();
                    int i = 0;
                    foreach (TrnsEmployeeElementDetail empelement in empEle.TrnsEmployeeElementDetail)
                    {

                        if (optbtn.Selected && empelement.MstElements.Type != "Rec") continue;
                        if (!optbtn.Selected && empelement.MstElements.Type == "Rec") continue;

                        dtElements.Rows.Add(1);
                        dtElements.SetValue("isNew", i, "N");
                        dtElements.SetValue("id", i, empelement.Id.ToString());
                        dtElements.SetValue("Element", i, empelement.MstElements.ElementName);
                        dtElements.SetValue("pick", i, strCfl);
                        dtElements.SetValue("Desc", i, empelement.MstElements.Description);
                        dtElements.SetValue("Classif", i, empelement.MstElements.ElmtType);
                        dtElements.SetValue("EffeDt", i, Convert.ToDateTime(empelement.StartDate).ToString("yyyyMMdd"));
                        dtElements.SetValue("Active", i, empelement.FlgActive == true ? "Y" : "N");
                        dtElements.SetValue("cpayroll", i, empelement.PeriodId == null ? "-1" : empelement.PeriodId.ToString());
                        dtElements.SetValue("consumed", i, empelement.FlgOneTimeConsumed == true ? "Y" : "N");

                        //DIHRMS.Custom.clsElement elementinfo = new DIHRMS.Custom.clsElement(dbHrPayroll, empelement, empelement.TrnsEmployeeElement.MstEmployee);
                        //DIHRMS.Custom.clsElement elementinfo = new DIHRMS.Custom.clsElement(dbHrPayroll, empelement, empelement.TrnsEmployeeElement.MstEmployee, empGrossSalary, 1);
                        //DIHRMS.Custom.clsElement elementinfo = new DIHRMS.Custom.clsElement(dbHrPayroll, empelement, empelement.TrnsEmployeeElement.MstEmployee, (emp.GrossSalary == null ? empGrossSalary : (decimal)emp.GrossSalary), 1);
                        DIHRMS.Custom.clsElement elementinfo = new DIHRMS.Custom.clsElement(dbHrPayroll, empelement, empelement.TrnsEmployeeElement.MstEmployee, ds.getEmpGross(emp, 1, 0, Percent), 1);

                        dtElements.SetValue("ValType", dtElements.Rows.Count - 1, empelement.ValueType);
                        dtElements.SetValue("Value", dtElements.Rows.Count - 1, empelement.Value.ToString());
                        //dtElements.SetValue("Amount", i, elementinfo.Amount.ToString());
                        //dtElements.SetValue("Amount", i, empelement.Amount.ToString());
                        //decimal decAmount = empelement.Amount == null ? 0 : empelement.Amount.Value;
                        decimal decAmount = empelement.Amount != elementinfo.Amount ? elementinfo.Amount : empelement.Amount.Value;
                        decAmount = empelement.MstElements.FlgGradeDep == true ? decAmount * Percent : decAmount;
                        dtElements.SetValue("Amount", i, decAmount.ToString());
                        var Element = dbHrPayroll.MstElements.Where(e => e.Id == empelement.ElementId).FirstOrDefault();
                        if (Element != null)
                        {
                            if (Element.MstElementEarning.FirstOrDefault().FlgLeaveEncashment.Value)
                            {
                                if (empelement.Amount != null)
                                {
                                    dtElements.SetValue("Amount", i, Convert.ToString(empelement.Amount * Percent));
                                }
                                else
                                {
                                    dtElements.SetValue("Amount", i, "0.0");
                                }
                            }
                        }

                        try
                        {
                            dtElements.SetValue("EndDt", i, Convert.ToDateTime(empelement.EndDate).ToString("yyyyMMdd"));
                        }
                        catch { }

                        i++;

                    }
                    i = 0;

                    //IEnumerable<TrnsLoan> empLoans = from p in dbHrPayroll.TrnsLoan where p.EmpID == empEle.EmployeeId select p;

                    var empLoans = (from p in dbHrPayroll.TrnsLoanDetail 
                                    where p.TrnsLoan.EmpID == empEle.EmployeeId
                                    && p.LnAID==p.TrnsLoan.ID                                    
                                    select p).ToList();

                    dtLoans.Rows.Clear();
                    foreach (TrnsLoanDetail empLoan in empLoans)
                    {
                        dtLoans.Rows.Add(1);
                        dtLoans.SetValue("id", i, empLoan.ID.ToString());
                        dtLoans.SetValue("LoanId", i, empLoan.MstLoans.Code);
                        dtLoans.SetValue("Descr", i, empLoan.MstLoans.Description);
                        dtLoans.SetValue("dtEffect", i, Convert.ToDateTime(empLoan.RequiredDate).ToString("yyyyMMdd"));
                        dtLoans.SetValue("LoanAmt", i, empLoan.ApprovedAmount == null ? "0.00" : empLoan.ApprovedAmount.ToString());                        
                        dtLoans.SetValue("TotalAmt", i, empLoan.ApprovedAmount == null ? "0.00" : empLoan.ApprovedAmount.ToString());
                        dtLoans.SetValue("InstAmt", i, empLoan.Installments.ToString());
                        dtLoans.SetValue("Active", i, "Y");
                        dtLoans.SetValue("Stop", i, empLoan.FlgStopRecovery == true ? "Y" : "N");

                        i++;

                    }

                    IEnumerable<TrnsAdvance> empAdvances = from p in dbHrPayroll.TrnsAdvance where p.EmpID == empEle.EmployeeId && p.RemainingAmount > 0 select p;
                    i = 0;
                    dtAdvance.Rows.Clear();
                    foreach (TrnsAdvance empAdv in empAdvances)
                    {
                        MstAdvance adv = (from p in dbHrPayroll.MstAdvance where p.Id.ToString() == empAdv.AdvanceType.ToString() select p).Single();
                        dtAdvance.Rows.Add(1);
                        dtAdvance.SetValue("AdvId", i, adv.AllowanceId);
                        dtAdvance.SetValue("Descr", i, adv.Description);
                        dtAdvance.SetValue("EffFrom", i, Convert.ToDateTime(empAdv.RequiredDate).ToString("yyyyMMdd"));
                        dtAdvance.SetValue("AdvAmt", i, empAdv.ApprovedAmount.ToString());
                        dtAdvance.SetValue("instAmt", i, empAdv.RemainingAmount.ToString());
                        dtAdvance.SetValue("Active", i, "Y");
                        i++;

                    }

                }

                addEmptyElement();
                mtElement.LoadFromDataSource();
                mtLoan.LoadFromDataSource();
                mtAdvance.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);

            }

        }

        private void addEmptyElement()
        {
            if (dtElements.Rows.Count == 0)
            {
                dtElements.Rows.Add(1);
                dtElements.SetValue("isNew", 0, "Y");
                dtElements.SetValue("id", 0, "");
                dtElements.SetValue("Element", 0, "");
                dtElements.SetValue("pick", 0, strCfl);
                dtElements.SetValue("Desc", 0, "");
                dtElements.SetValue("Classif", 0, "");
                dtElements.SetValue("EffeDt", 0, "");
                dtElements.SetValue("ValType", 0, "");
                dtElements.SetValue("Value", 0, "0.00");
                dtElements.SetValue("RetroAmnt", 0, 0.00);
                dtElements.SetValue("RetActive", 0, "N");
                // mtElement.AddRow(1, mtElement.RowCount + 1);
            }
            else
            {
                if (dtElements.GetValue("Element", dtElements.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtElements.Rows.Add(1);
                    dtElements.SetValue("isNew", dtElements.Rows.Count - 1, "Y");
                    dtElements.SetValue("id", dtElements.Rows.Count - 1, "");
                    dtElements.SetValue("Element", dtElements.Rows.Count - 1, "");
                    dtElements.SetValue("pick", dtElements.Rows.Count - 1, strCfl);

                    dtElements.SetValue("Desc", dtElements.Rows.Count - 1, "");
                    dtElements.SetValue("Classif", dtElements.Rows.Count - 1, "");
                    dtElements.SetValue("EffeDt", dtElements.Rows.Count - 1, "");
                    dtElements.SetValue("ValType", dtElements.Rows.Count - 1, "");
                    dtElements.SetValue("Value", dtElements.Rows.Count - 1, "0.00");
                    dtElements.SetValue("RetroAmnt", dtElements.Rows.Count - 1, 0.00);
                    dtElements.SetValue("RetActive", dtElements.Rows.Count - 1, "N");
                    //  mtElement.AddRow(1, mtElement.RowCount + 1);

                }

            }
            mtElement.LoadFromDataSource();



        }

        private void addEmptyRow()
        {


            if (dtAdvance.Rows.Count == 0)
            {
                dtAdvance.Rows.Add(1);
                dtAdvance.SetValue("isNew", 0, "Y");
                dtAdvance.SetValue("id", 0, 0);
                dtAdvance.SetValue("advCode", 0, "");
                dtAdvance.SetValue("Desc", 0, "");
                dtAdvance.SetValue("Active", 0, "N");
                mtAdvance.AddRow(1, mtAdvance.RowCount + 1);
            }
            else
            {
                if (dtAdvance.GetValue("AdvId", dtAdvance.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtAdvance.Rows.Add(1);
                    dtAdvance.SetValue("isNew", dtAdvance.Rows.Count - 1, "Y");
                    dtAdvance.SetValue("advCode", dtAdvance.Rows.Count - 1, "");
                    dtAdvance.SetValue("Desc", dtAdvance.Rows.Count - 1, "");
                    dtAdvance.SetValue("Active", dtAdvance.Rows.Count - 1, "N");
                    mtAdvance.AddRow(1, mtAdvance.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();

        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empElement";
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

        public void _fillFields()
        {


            int cnt = (from p in dbHrPayroll.TrnsEmployeeElement where p.EmployeeId.ToString() == txCode.Value.ToString() select p).Count();

        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("empid", txHRMSId.Value.ToString());
            //SearchKeyVal.Add("FirstName + ' ' + LastName", txName.Value.ToString());


        }

        private void doSubmit()
        {
            try
            {
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
                {
                    doFind();
                }
                else
                {
                    submitChanges();
                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void doFind()
        {
            try
            {

                PrepareSearchKeyHash();
                string strSql = sqlString.getSql("empElement", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for elements");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    //txHRMSId.Value = st.Rows[0][0].ToString();
                    //fillMat(Convert.ToString(txHRMSId.Value.ToString()));
                    txHRMSId.Value = st.Rows[0][0].ToString();
                    selEmpId = txHRMSId.Value.ToString().Trim();
                    fillMat(Convert.ToString(txHRMSId.Value.ToString()));
                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void submitChanges()
        {

            try
            {
                //int confirm = oApplication.MessageBox("Are you sure you want to Update Element(s) for Selected Employee? ", 3, "Yes", "No", "Cancel");
                //if (confirm == 2 || confirm == 3)
                //{
                //    return;
                //} 
                TrnsEmployeeElement empEle;
                mtElement.FlushToDataSource();
                int cnt = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == txHRMSId.Value.ToString() select p).Count();

                if (cnt > 0)
                {
                    empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == txHRMSId.Value.ToString() select p).FirstOrDefault();
                    empEle.UpdateDate = DateTime.Now;
                    empEle.UpdatedBy = oCompany.UserName;
                }
                else
                {
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txHRMSId.Value select p).Single();
                    empEle = new TrnsEmployeeElement();
                    empEle.CreateDate = DateTime.Now;
                    empEle.UserId = oCompany.UserName;
                    empEle.MstEmployee = emp;
                    dbHrPayroll.TrnsEmployeeElement.InsertOnSubmit(empEle);
                }
                for (int i = 0; i < dtElements.Rows.Count; i++)
                {
                    string strEmpCode = (mtElement.Columns.Item("id").Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value;

                    string elId = Convert.ToString(dtElements.GetValue("id", i));
                    string eleName = Convert.ToString(dtElements.GetValue("Element", i));
                    string isNew = Convert.ToString(dtElements.GetValue("isNew", i));
                    string isActive = dtElements.GetValue("Active", i);

                    if (elId != "")
                    {
                        TrnsEmployeeElementDetail trntEle;
                        if (elId != "0" && elId != "" && isNew == "Y")
                        {
                            trntEle = new TrnsEmployeeElementDetail();
                            empEle.TrnsEmployeeElementDetail.Add(trntEle);
                            trntEle.FlgOneTimeConsumed = false;
                            trntEle.CreateDate = DateTime.Now;
                            trntEle.UserId = oCompany.UserName;
                        }
                        else
                        {
                            trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.Id.ToString() == elId select p).Single();
                            trntEle.UpdateDate = DateTime.Now;
                            trntEle.UpdatedBy = oCompany.UserName;
                        }
                        // mtElement.SetLineData(i+1);
                        MstElements mstele = (from p in dbHrPayroll.MstElements where p.ElementName == eleName select p).FirstOrDefault();

                        bool isNewLeave = (mtElement.Columns.Item("Active").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                        string value = (mtElement.Columns.Item("Value").Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value;
                        string PPayroll = (mtElement.Columns.Item("cpayroll").Cells.Item(i + 1).Specific as SAPbouiCOM.ComboBox).Value;
                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                        trntEle.FlgRetro = false;
                        trntEle.ElementType = Convert.ToString(dtElements.GetValue("Classif", i));
                        trntEle.ValueType = Convert.ToString(dtElements.GetValue("ValType", i));
                        trntEle.Value = Convert.ToDecimal(value);//Convert.ToDecimal(dtElements.GetValue("Value", i));
                        //Code Modified by Zeeshan
                        if (mstele.MstElementEarning[0].FlgLeaveEncashment.Value)
                        {
                            if (elId != "0" && elId != "" && isNew == "Y")
                            {
                                trntEle.Amount = 0.0M;
                            }
                        }
                        else
                        {
                            trntEle.Amount = getElementAmount(empEle.MstEmployee, i);
                        }
                        //trntEle.FlgActive = Convert.ToString(dtElements.GetValue("Active", i)) == "Y" ? true : false;
                        trntEle.FlgActive = isNewLeave == true ? true : false;
                        trntEle.MstElements = mstele;
                        if (!string.IsNullOrEmpty(PPayroll) && PPayroll != "-1")
                        {
                            if (trntEle.FlgOneTimeConsumed != true)
                            {
                                var Payrolx = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PPayroll)).FirstOrDefault();
                                if (Payrolx != null)
                                {
                                    trntEle.PeriodId = Payrolx.ID;
                                }
                            }
                            else
                            {

                            }
                        }

                    }
                    //UpdateLoans();
                }
                dbHrPayroll.SubmitChanges();
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
            fillMat(txHRMSId.Value.ToString());
        }

        private void UpdateLoans()
        {
            try
            {
                if (dtLoans != null && dtLoans.Rows.Count > 0)
                {
                    for (int i = 0; i < dtLoans.Rows.Count; i++)
                    {
                        int loanId = Convert.ToInt32((mtLoan.Columns.Item("ID").Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value);
                        string stoprecovery = (mtLoan.Columns.Item("Stop").Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value;
                        var LoansData = dbHrPayroll.TrnsLoan.Where(l => l.ID == loanId).FirstOrDefault();
                        if (LoansData != null)
                        {
                            LoansData.TrnsLoanDetail[0].FlgStopRecovery = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        #endregion

    }
}
