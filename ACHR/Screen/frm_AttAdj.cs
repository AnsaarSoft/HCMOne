using System;
using System.Data;
using System.IO;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_AttAdj : HRMSBaseForm
    {
        public IEnumerable<AttSummary> batchs;
        public int elementId = 0;

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
           
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    submitForm();
                    break;
                
                case "btpick":
                    picElement();
                    break;
                case "mtEmp":
                    if (pVal.ColUID == "pick" && pVal.Row <= dtEmps.Rows.Count)
                    {
                        pickemps(pVal.Row );
                    }
                    break;

                case "btPick":
                    getFileName();
                    break;

            }
        }
        private void picElement()
        {
            
        }
        private void pickemps(int rowNum)
        {
            SearchKeyVal.Clear();
            SearchKeyVal.Add("emp.PayrollID", cbProll.Value.Trim());
            string strSql = sqlString.getSql("PayrollEmps", SearchKeyVal);
            strSql = strSql + " Order by FirstName";
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Days Hour Adjustment");
            pic = null;
            if (st.Rows.Count > 0)
            {
                string strCode = st.Rows[0][0].ToString();
                string strname = st.Rows[0][1].ToString() + " " +  st.Rows[0][2].ToString();
                var oldRecord = dbHrPayroll.AttSummary.Where(p => p.PayrollId.ToString() == cbProll.Value.Trim()).FirstOrDefault();
                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == strCode).FirstOrDefault();               
                dtEmps.SetValue("id", rowNum - 1, "0");
                dtEmps.SetValue("empId", rowNum - 1, strCode);
                dtEmps.SetValue("hrmsId", rowNum - 1,strCode);
                dtEmps.SetValue("EmpName", rowNum - 1, strname);
                if (oldRecord != null)
                {
                    var oldRecordDetail = oldRecord.AttSummaryDetail.Where(s => s.EmpId == EmpRecord.ID).FirstOrDefault();
                    if (oldRecordDetail != null)
                    {
                        dtEmps.SetValue("adjDays", rowNum-1, oldRecordDetail.AdjDays.ToString());
                        dtEmps.SetValue("adjHrs", rowNum-1, oldRecordDetail.AdjHrs.ToString());
                        dtEmps.SetValue("hrRate", rowNum-1, oldRecordDetail.HrsRate.ToString());
                    }
                }
                addEmptyRow();

            }

            mtEmp.LoadFromDataSource();



        }
        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        { 
            base.etAfterCfl(ref pVal, ref BubbleEvent);
            SAPbouiCOM.ChooseFromListEvent ocfl = (SAPbouiCOM.ChooseFromListEvent)pVal;
            string itemId = pVal.ItemUID;
            SAPbouiCOM.Item cflItem = oForm.Items.Item(itemId);
            SAPbouiCOM.DataTable oDT = ocfl.SelectedObjects;
            if (oDT != null)
            {
                int i = 0;
                int rowNum = pVal.Row;
                for (i = 0; i < oDT.Rows.Count; i++)
                {
                    string hrmsid = Convert.ToString(oDT.GetValue("U_HrmsEmpId", i));
                    string empIdSelected = Convert.ToString(oDT.GetValue("empID", i));
                   
                   


                    if (hrmsid.Trim() != "")
                    {
                        dtEmps.SetValue("id", rowNum - 1,"0");
                        dtEmps.SetValue("empId", rowNum - 1, Convert.ToString(oDT.GetValue("empID", i)));
                        dtEmps.SetValue("hrmsId", rowNum - 1, Convert.ToString(oDT.GetValue("U_HrmsEmpId", i)));
                        dtEmps.SetValue("EmpName", rowNum - 1, oDT.GetValue("firstName", i));
                       

                        

                        rowNum++;
                        if (rowNum > dtEmps.Rows.Count)
                        {
                            /*
                            dtEmps.Rows.Add(1);
                            mtEmp.AddRow(1, mtEmp.RowCount + 1);
                             * */
                            addEmptyRow();

                        }
                    }
                }
                mtEmp.LoadFromDataSource();

            }

        }
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            fillCbs();
            IniContrls();
           mtEmp.Columns.Item("id").Visible = false;
           AddNewRecord();
            
        }
        private void fillCbs()
        {
            int i = 0;
            string selId = "0";
            IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
            foreach (CfgPayrollDefination pr in prs)
            {
                cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                i++;
            }
            cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            FillPeriod(cbProll.Value);
            //fillCombo("btchStatus", cbStatus);

            cbStatus.ValidValues.Add("MAN", "Mannual");
            cbStatus.ValidValues.Add("VL", "Vacation Leave");


            
            

        }
        private void FillPeriod(string payroll)
        {
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
            int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Count();
            if (cnt > 0)
            {
                CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Single();
                foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                {
                    cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());

                    if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                    {
                        selId = pd.ID.ToString();
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
        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);               
                if (pVal.ItemUID == "cbProll")
                {
                    FillPeriod(cbProll.Value.Trim());
                }

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }

        }

        private void IniContrls()
        {

            oForm.DataSources.UserDataSources.Item("cbStatus").ValueEx = "MAN";
                   

            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            oForm.Update();
            oForm.Refresh();
            getData();
            long nextId = ds.getNextId("AttSummary", "attSummaryID");
            txDocNum.Value = nextId.ToString();
            cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            oForm.DataSources.UserDataSources.Item("txDocDate").ValueEx = DateTime.Now.ToString("yyyyMMdd");
            dtEmps.Rows.Clear();
            oForm.Items.Item("btPick").Enabled = true;

        }
        private void getData()
        {
            CodeIndex.Clear();
            batchs = from p in dbHrPayroll.AttSummary select p;
            int i = 0;
            foreach (AttSummary ele in batchs)
            {
                CodeIndex.Add(ele.AttSummaryID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        public override void fillFields()
        {
            base.fillFields();
            _fillFields();
        }
       
        private void _fillFields()
        {
            oForm.Freeze(true);
            try
            {
                if (currentRecord >= 0)
                {


                    AttSummary record = batchs.ElementAt<AttSummary>(currentRecord);
                   
                    txDocNum.Value = record.AttSummaryID.ToString();

                    oForm.Items.Item("btPick").Enabled = false;
                    dtEmps.Rows.Clear();
                    int rowNum = 0;
                    try
                    {

                        oForm.DataSources.UserDataSources.Item("txDocDate").ValueEx = Convert.ToDateTime(record.DocDate).ToString("yyyyMMdd");
                        if (record.DocDate != null)
                        {
                            oForm.DataSources.UserDataSources.Item("txDocDate").ValueEx = Convert.ToDateTime(record.DocDate).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txDocDate.Value = "";
                        }
                    }
                    catch { }
                    oForm.DataSources.UserDataSources.Item("cbProll").ValueEx = record.PayrollId.ToString().Trim();
                    FillPeriod(cbProll.Value);
                    oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = record.PeriodId.ToString().Trim();                    
                    oForm.DataSources.UserDataSources.Item("cbStatus").ValueEx = record.SourceType == null ? "" : record.SourceType.ToString().Trim();
                    txSourceId.Value = record.SourceId.ToString();
                    foreach (AttSummaryDetail btd in record.AttSummaryDetail)
                    {
                        // MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.ID == btd.EmployeeID select p).Single();

                        dtEmps.Rows.Add(1);
                        dtEmps.SetValue("id", rowNum, btd.AttSumDetailId.ToString());
                        dtEmps.SetValue("pick", rowNum, strCfl);
                        dtEmps.SetValue("hrmsId", rowNum, btd.MstEmployee.EmpID);
                        dtEmps.SetValue("EmpName", rowNum, btd.MstEmployee.FirstName + " " + btd.MstEmployee.MiddleName + " " + btd.MstEmployee.LastName);
                        dtEmps.SetValue("adjDays", rowNum, btd.AdjDays.ToString());
                        dtEmps.SetValue("adjHrs", rowNum, btd.AdjHrs.ToString());
                        dtEmps.SetValue("hrRate", rowNum, btd.HrsRate.ToString());
                        dtEmps.SetValue("Active", rowNum, btd.FlgActive == true ? "Y" : "N");

                       
                        rowNum++;

                    }
                    addEmptyRow();
                    mtEmp.LoadFromDataSource();


                }

                oForm.Freeze(false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in loading Record!" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                oForm.Freeze(false);

            }
        }
        private void addEmptyRow()
        {


            if (dtEmps.Rows.Count == 0)
            {
                dtEmps.Rows.Add(1);
                dtEmps.SetValue("id", 0, "0");
                dtEmps.SetValue("pick", 0, strCfl);
                dtEmps.SetValue("hrmsId", 0, "");
                dtEmps.SetValue("EmpName", 0, "");
                dtEmps.SetValue("adjDays", 0, "0.00");
                dtEmps.SetValue("adjHrs", 0, "0.00");
                dtEmps.SetValue("hrRate", 0, "0.00");
                dtEmps.SetValue("Active", 0,"Y");
                mtEmp.AddRow(1, mtEmp.RowCount + 1);


               

            }
            else
            {
                if (dtEmps.GetValue("empId", dtEmps.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtEmps.Rows.Add(1);
                    dtEmps.SetValue("id", dtEmps.Rows.Count - 1, "0");
                    dtEmps.SetValue("pick", dtEmps.Rows.Count - 1, strCfl);
                    dtEmps.SetValue("hrmsId", dtEmps.Rows.Count - 1, "");
                    dtEmps.SetValue("EmpName", dtEmps.Rows.Count - 1, "");
                    dtEmps.SetValue("adjDays", dtEmps.Rows.Count - 1, "0.00");
                    dtEmps.SetValue("adjHrs", dtEmps.Rows.Count - 1, "0.00");
                    dtEmps.SetValue("hrRate", 0, "0.00");
                    dtEmps.SetValue("Active", dtEmps.Rows.Count - 1, "Y");
                    mtEmp.AddRow(1, mtEmp.RowCount + 1);
                }

            }


        }
        public override void AddNewRecord()
        {
            base.AddNewRecord();
            IniContrls();
            addEmptyRow();
        }

        private void submitForm()
        {
            mtEmp.FlushToDataSource();
            string id = "";
            string code = "";
            string isnew = "";


            AttSummary eleAttAdj;
            int cnt = (from p in dbHrPayroll.AttSummary where p.AttSummaryID.ToString() == txDocNum.Value.ToString()  select p).Count();
            int cntOne = dbHrPayroll.AttSummary.Where(d => d.PeriodId == Convert.ToInt32(cbPeriod.Value.Trim()) && d.PayrollId.ToString() == cbProll.Value.Trim()).Count();
            if (cnt > 0)
            {
                eleAttAdj = (from p in dbHrPayroll.AttSummary where p.AttSummaryID.ToString() == txDocNum.Value.ToString() select p).Single();
                eleAttAdj.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.Trim() select p).Single();
                eleAttAdj.PayrollId = Convert.ToInt32(cbProll.Value);

            }
            else if (cntOne > 0)
            {
                eleAttAdj = dbHrPayroll.AttSummary.Where(d => d.PeriodId == Convert.ToInt32(cbPeriod.Value.Trim()) && d.PayrollId.ToString() == cbProll.Value.Trim()).FirstOrDefault();
                eleAttAdj.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.Trim() select p).Single();
                eleAttAdj.PayrollId = Convert.ToInt32(cbProll.Value);
            }
            else
            {
                eleAttAdj = new AttSummary();
                eleAttAdj.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.Trim() select p).Single();
                eleAttAdj.CreateDt = DateTime.Now;
                eleAttAdj.CreateBy = oCompany.UserName;
                eleAttAdj.PayrollId = Convert.ToInt32(cbProll.Value);
                dbHrPayroll.AttSummary.InsertOnSubmit(eleAttAdj);
            }

            eleAttAdj.SourceType = cbStatus.Value.ToString().Trim();

            if (txDocDate.Value != "")
            {
                eleAttAdj.DocDate = DateTime.ParseExact(txDocDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            }
            else
            {
               // payEle.EndDate = null;
            }

               eleAttAdj.UpdateDt = DateTime.Now;
            eleAttAdj.UpdateBy = oCompany.UserName;
           
            for (int i = 0; i < dtEmps.Rows.Count; i++)
            {

                code = Convert.ToString(dtEmps.GetValue("hrmsId", i));
                code = code.Trim();
                if (code != "")
                {
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == code select p).FirstOrDefault();
                    AttSummaryDetail attSummaryDetail = null;
                    int detailId = Convert.ToInt32(dtEmps.GetValue("id", i));
                    if (detailId > 0)
                    {
                        attSummaryDetail = (from p in dbHrPayroll.AttSummaryDetail where p.AttSumDetailId.ToString() == detailId.ToString() select p).Single();

                    }
                    else if (eleAttAdj != null && attSummaryDetail == null && emp != null)
                    {
                        attSummaryDetail = dbHrPayroll.AttSummaryDetail.Where(p => p.AttSumDetailId.ToString() == eleAttAdj.AttSummaryID.ToString() && p.EmpId == emp.ID).FirstOrDefault();
                        if (attSummaryDetail == null)
                        {
                            attSummaryDetail = new AttSummaryDetail();
                            eleAttAdj.AttSummaryDetail.Add(attSummaryDetail);
                        }
                    }
                    else if(emp!=null)
                    {
                        attSummaryDetail = new AttSummaryDetail();
                        eleAttAdj.AttSummaryDetail.Add(attSummaryDetail);
                    }


                    //MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == code select p).FirstOrDefault();
                    if (emp != null)
                    {
                        attSummaryDetail.EmpId = emp.ID;
                        attSummaryDetail.AdjDays = Convert.ToDecimal(dtEmps.GetValue("adjDays", i));
                        attSummaryDetail.AdjHrs = Convert.ToDecimal(dtEmps.GetValue("adjHrs", i));
                        attSummaryDetail.HrsRate = Convert.ToDecimal(dtEmps.GetValue("hrRate", i));
                        attSummaryDetail.FlgActive = Convert.ToString(dtEmps.GetValue("Active", i)) == "Y" ? true : false;
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
                _fillFields();
            }
           
           
        }
        private void getFileName()
        {
            string fileName = Program.objHrmsUI.FindFile();
            if (String.IsNullOrEmpty(fileName))
            {
                oApplication.SetStatusBarMessage("Select a template file");
                return;
            }
            txFilenam.Value = fileName;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            int rowNum = 1;
            dtEmps.Rows.Clear();
            mtEmp.LoadFromDataSource();
            if (dt.Rows.Count > 0)
            {
                dtEmps.Rows.Add(dt.Rows.Count);

                foreach (DataRow dr in dt.Rows)
                {

                    if (dr["EmpCode"].ToString() != "")
                    {
                        dtEmps.SetValue("id", rowNum - 1, "0");
                        dtEmps.SetValue("hrmsId", rowNum - 1, dr["EmpCode"].ToString());
                        dtEmps.SetValue("EmpName", rowNum - 1, dr["EmpName"].ToString());
                        dtEmps.SetValue("adjDays", rowNum - 1, dr["AdjDays"].ToString());
                        dtEmps.SetValue("adjHrs", rowNum - 1, dr["AdjHrs"].ToString());
                        dtEmps.SetValue("hrRate", rowNum - 1, dr["HrsRate"].ToString());
                        dtEmps.SetValue("Active", rowNum - 1, dr["Active"].ToString() == "1" ? "Y" : "N");
                        rowNum++;
                    }
                    else
                    {
                        break;
                    }

                }
                mtEmp.LoadFromDataSource();
               // addEmptyRow();
            }
        }
        private void fillDtFromTemplate(DataTable dt)
        {
            string fileName = txFilenam.Value.Trim();
            using (StreamReader file = new StreamReader(fileName))
            {
                string line = "";
                string[] pastrts;
                string strTemplateName = file.ReadLine();
                if (strTemplateName == null || !strTemplateName.Contains("HRMS Template"))
                {
                    oApplication.SetStatusBarMessage("Incorrect Template File");
                    return;
                }
                line = file.ReadLine();
                if (line == null)
                {
                    oApplication.SetStatusBarMessage("Incorrect Template File");
                    return;
                }
                pastrts = line.Split('\t');
                foreach (string colName in pastrts)
                {
                    dt.Columns.Add(colName);
                }
                while ("a" == "a")
                {
                    line = file.ReadLine();
                    if (line == null) break;
                    pastrts = line.Split('\t');
                    dt.Rows.Add(pastrts);
                    // dt.Rows.Add(pastrts(0), pastrts(1), pastrts(2), pastrts(3), pastrts(4), pastrts(5), pastrts(6), pastrts(7), pastrts(8), pastrts(9), pastrts(10), pastrts(11))
                }
            }
        }
    }
}
