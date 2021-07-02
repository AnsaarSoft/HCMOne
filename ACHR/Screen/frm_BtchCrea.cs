using System;
using System.Data;
using System.IO;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    partial class frm_BtchCrea : HRMSBaseForm
    {
        #region Local Variable Area

        public IEnumerable<TrnsBatches> batchs;
        public int elementId = 0;
        SAPbouiCOM.DataTable dtPeriods;
        Boolean flgValidCall = false, flgDocMode = false;

        string selEmpId = "";
        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            fillCbs();
            IniContrls();
            mtEmp.Columns.Item("id").Visible = false;
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    submitForm();
                    break;
                case "btProcess":
                    processBatch();
                    break;
                case "btpick":
                    picElement();
                    break;
                case "mtEmp":
                    if (pVal.ColUID == "pick" && pVal.Row <= dtEmps.Rows.Count)
                    {
                        pickemps(pVal.Row);
                    }
                    break;

                case "btPick":
                    getFileName();
                    break;
                case "btRefresh":
                    ExecuteFMSonEmpValue();
                    break;
            }
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
                    if (hrmsid.Trim() != "")
                    {
                        dtEmps.SetValue("id", rowNum - 1, "0");
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

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (!string.IsNullOrEmpty(Program.EmpID))
            {
                if (flgValidCall)
                    SetElementValues(Program.EmpID);
            }
        }

        public override void fillFields()
        {
            base.fillFields();
            _fillFields();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            OpenNewSearchWindow();
        }
        #endregion

        #region Local Functions

        private void picElement()
        {
            try
            {
                picker pic = new picker(oApplication, ds.getValidPrlElement(cbProll.Value.Trim(), cbPeriod.Value.Trim()));
                System.Data.DataTable st = pic.ShowInput("Select Element", "Select Element for Employee");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    string strRepeat = st.Rows[0][3].ToString();
                    string elementName = st.Rows[0][1].ToString();
                    string id = st.Rows[0][0].ToString();
                    txElCode.Value = st.Rows[0][1].ToString();
                    elementId = Convert.ToInt32(id);
                    setElementInfo(elementName, elementId);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function PicElemnt.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void pickemps(int rowNum)
        {
            try
            {
                SearchKeyVal.Clear();
                SearchKeyVal.Add("emp.PayrollID", cbProll.Value.Trim());
                string strSql = sqlString.getSql("PayrollEmps", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select OT", "Select over time");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    string strCode = st.Rows[0][0].ToString();
                    string strname = st.Rows[0][1].ToString() + " " + st.Rows[0][2].ToString();


                    dtEmps.SetValue("id", rowNum - 1, "0");
                    dtEmps.SetValue("empId", rowNum - 1, strCode);
                    dtEmps.SetValue("hrmsId", rowNum - 1, strCode);
                    dtEmps.SetValue("EmpName", rowNum - 1, strname);

                    addEmptyRow();

                }

                mtEmp.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function pickemps.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }



        }

        private void setElementInfo(string ele, int id)
        {
            try
            {
                int cnt = (from p in dbHrPayroll.MstElements where p.ElementName == ele && p.Id == id select p).Count();
                if (cnt > 0)
                {
                    MstElements element = (from p in dbHrPayroll.MstElements where p.ElementName == ele && p.Id == id select p).FirstOrDefault();
                    txElName.Value = element.Description;
                    txEleType.Value = element.ElmtType;
                    elementId = element.Id;
                    switch (element.ElmtType.ToString().Trim())
                    {
                        case "Ear":
                            oForm.PaneLevel = 1;
                            cbValType.Select(element.MstElementEarning[0].ValueType.Trim());
                            txValue.Value = element.MstElementEarning[0].Value.ToString();
                            break;
                        case "Ded":
                            oForm.PaneLevel = 1;
                            cbValType.Select(element.MstElementDeduction[0].ValueType.Trim());
                            txValue.Value = element.MstElementDeduction[0].Value.ToString();
                            break;
                        case "Con":
                            oForm.PaneLevel = 2;
                            break;
                    }
                    dtEmps.Rows.Clear();
                    mtEmp.LoadFromDataSource();
                    addEmptyRow();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function setElementInfo.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void fillCbs()
        {
            try
            {
                int i = 0;
                string selId = "0";
                //IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                //foreach (CfgPayrollDefination pr in prs)
                //{
                //    cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                //    i++;
                //}
                #region Fill Payroll
                string strOut = string.Empty;
                //string strSql = "SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"DimCode\" = 1";
                string strSql = "SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                //string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 1";
                oRecSet.DoQuery(strSql);
                //strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                strOut = Convert.ToString(oRecSet.Fields.Item("U_PayrollType").Value);
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    if (strOut != null && strOut != "")
                    {
                        //IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == strOut.Trim() select p;
                        //foreach (CfgPayrollDefination pr in prs)
                        //{
                        //    cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        //    i++;
                        //}
                        string strSql2 = sqlString.getSql("GetPayrollName", SearchKeyVal);
                        strSql2 = strSql2 + " where ID in (" + strOut + ")";
                        strSql2 += " ORDER BY ID Asc ";
                        System.Data.DataTable dt = ds.getDataTable(strSql2);
                        DataView dv = dt.DefaultView;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int l = 0; l < dt.Rows.Count; l++)
                            {
                                string strPayrollName = dt.Rows[l]["PayrollName"].ToString();
                                Int32 intPayrollID = Convert.ToInt32(dt.Rows[l]["ID"].ToString());
                                cbProll.ValidValues.Add(intPayrollID.ToString(), strPayrollName);

                            }
                        }
                        cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbProll.Value);
                    }
                    else
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbProll.Value);
                    }
                }
                else
                {
                    IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                    foreach (CfgPayrollDefination pr in prs)
                    {
                        cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        i++;
                    }

                    cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    FillPeriod(cbProll.Value);
                }
                //End Fill Payroll
                #endregion

                FillPeriod(cbProll.Value);
                fillCombo("btchStatus", cbStatus);
                fillCombo("Val_Type", cbValType);
                fillCombo("btchIfExist", cbIfExst);

                fillColumCombo("Val_Type", mtEmp.Columns.Item("ValType"));
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function fillCbs.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                        if (pd.FlgVisible == null ? false : (bool)pd.FlgVisible && pd.FlgLocked != true)
                        {
                            cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        }
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
                getData();
                long nextId = ds.getNextId("TrnsBatches", "ID");
                txDocNum.Value = nextId.ToString();
                txBchName.Value = "";
                txElCode.Value = "";
                txEleType.Value = "";
                txElName.Value = "";
                txEmpCont.Value = "";
                txEmprCont.Value = "0.00";
                txEff.Value = "";
                txValue.Value = "0.00";
                cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbIfExst.Select(2, SAPbouiCOM.BoSearchKey.psk_Index);

                oForm.DataSources.UserDataSources.Item("txEff").ValueEx = DateTime.Now.ToString("yyyyMMdd");
                dtEmps.Rows.Clear();
                txBchName.Active = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function IniContrls.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void getData()
        {
            try
            {
                CodeIndex.Clear();
                batchs = from p in dbHrPayroll.TrnsBatches select p;
                int i = 0;
                foreach (TrnsBatches ele in batchs)
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

        private void _fillFields()
        {
            oForm.Freeze(true);
            string strProcessing = "";
            try
            {
                if (currentRecord >= 0)
                {
                    TrnsBatches record = batchs.ElementAt<TrnsBatches>(currentRecord);
                    elementId = record.MstElements.Id;
                    txDocNum.Value = record.Id.ToString();
                    txBchName.Value = record.BatchName.ToString();
                    txElCode.Value = record.MstElements.ElementName.ToString();
                    txElName.Value = record.MstElements.Description;
                    txEleType.Value = record.ElmtType.ToString();
                    cbValType.Select(record.ValType.ToString());
                    cbProll.Select(record.PayrollName.ToString());
                    cbPeriod.Select(record.PayrollPeriodID.ToString());
                    cbStatus.Select(record.BatchStatus.ToString());
                    if (record.BatchStatus.ToString().Trim() == "0")
                    {
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        IbtProcess.Enabled = true;
                    }
                    else
                    {
                        IbtProcess.Enabled = false;

                        //b oForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
                    }
                    txValue.Value = record.Value.ToString();
                    txEmpCont.Value = record.Value.ToString();
                    txEmprCont.Value = record.Value.ToString();
                    switch (record.ElmtType.Trim())
                    {
                        case "Ear":
                        case "Ded":
                            oForm.PaneLevel = 1;
                            break;
                        case "Con":
                            oForm.PaneLevel = 2;
                            break;
                    }
                    dtEmps.Rows.Clear();
                    int rowNum = 0;
                    foreach (TrnsBatchesDetails btd in record.TrnsBatchesDetails)
                    {
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.ID == btd.EmployeeID select p).Single();
                        strProcessing = "Error in Setting Employee Record with Employee ID --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + "  ";

                        dtEmps.Rows.Add(1);
                        dtEmps.SetValue("id", rowNum, btd.Id.ToString());
                        dtEmps.SetValue("pick", rowNum, strCfl);
                        dtEmps.SetValue("hrmsId", rowNum, emp.EmpID);
                        dtEmps.SetValue("EmpName", rowNum, emp.FirstName);
                        dtEmps.SetValue("ValType", rowNum, btd.ValueType);
                        dtEmps.SetValue("Value", rowNum, btd.Value.ToString());
                        if (btd.EmplrCont == null)
                        {
                            dtEmps.SetValue("emprValue", rowNum, "0");
                        }
                        else
                        {
                            dtEmps.SetValue("emprValue", rowNum, btd.EmplrCont.ToString());
                        }
                        dtEmps.SetValue("Active", rowNum, btd.FlgActive == true ? "Y" : "N");
                        rowNum++;
                    }
                    addEmptyRow();
                    mtEmp.LoadFromDataSource();
                    oForm.Items.Item("btProcess").Enabled = true;
                }

                oForm.Freeze(false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(strProcessing + "Error in loading Record!" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                oForm.Freeze(false);

            }
        }

        private void SetElementValues(string pvalue)
        {
            try
            {
                string strProcessing = "";
                string payrollName = cbProll.Value.Trim();
                string PeriodName = cbPeriod.Value.Trim();
                if (!string.IsNullOrEmpty(pvalue))
                {
                    flgValidCall = false;
                    //var oPayroll = (from a in dbHrPayroll.CfgPayrollDefination where a.ID.ToString() == payrollName select a).FirstOrDefault();
                    //if (oPayroll == null) return;
                    //var oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == oPayroll.ID && a.ID.ToString() == PeriodName select a).FirstOrDefault();
                    //if (oPeriod == null) return;

                    //var oDoc = (from a in dbHrPayroll.TrnsBatches where a.Id.ToString() == pvalue && a.PayrollID == oPayroll.ID && a.PayrollPeriodID == oPeriod.ID select a).FirstOrDefault();
                    var oDoc = (from a in dbHrPayroll.TrnsBatches where a.Id.ToString() == pvalue select a).FirstOrDefault();
                    if (oDoc == null)
                    {
                        oApplication.StatusBar.SetText("Please Select Proper Payroll First", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    else
                    {
                        elementId = oDoc.MstElements.Id;
                        txDocNum.Value = Convert.ToString(oDoc.Id);
                        txBchName.Value = oDoc.BatchName.ToString();
                        txElCode.Value = oDoc.MstElements.ElementName.ToString();
                        txElName.Value = oDoc.MstElements.Description;
                        txEleType.Value = oDoc.ElmtType.ToString();
                        cbValType.Select(oDoc.ValType.ToString());
                        //
                        cbProll.Select(oDoc.PayrollID.ToString());
                        cbPeriod.Select(oDoc.PayrollPeriodID.ToString());

                        cbStatus.Select(oDoc.BatchStatus.ToString());
                        if (oDoc.BatchStatus.ToString().Trim() == "0")
                        {
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                            IbtProcess.Enabled = true;
                        }
                        else
                        {
                            IbtProcess.Enabled = false;

                            //b oForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
                        }
                        txValue.Value = oDoc.Value.ToString();
                        txEmpCont.Value = oDoc.Value.ToString();
                        txEmprCont.Value = oDoc.Value.ToString();
                        switch (oDoc.ElmtType.Trim())
                        {
                            case "Ear":
                            case "Ded":
                                oForm.PaneLevel = 1;
                                break;
                            case "Con":
                                oForm.PaneLevel = 2;
                                break;
                        }
                        dtEmps.Rows.Clear();
                        int rowNum = 0;

                        foreach (TrnsBatchesDetails btd in oDoc.TrnsBatchesDetails)
                        {
                            MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.ID == btd.EmployeeID select p).Single();
                            strProcessing = "Error in Setting Employee Record with Employee ID --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + "  ";

                            dtEmps.Rows.Add(1);
                            dtEmps.SetValue("id", rowNum, btd.Id.ToString());
                            dtEmps.SetValue("pick", rowNum, strCfl);
                            dtEmps.SetValue("hrmsId", rowNum, emp.EmpID);
                            dtEmps.SetValue("EmpName", rowNum, emp.FirstName);
                            dtEmps.SetValue("ValType", rowNum, btd.ValueType);
                            dtEmps.SetValue("Value", rowNum, btd.Value.ToString());
                            if (btd.EmplrCont == null)
                            {
                                dtEmps.SetValue("emprValue", rowNum, "0");
                            }
                            else
                            {
                                dtEmps.SetValue("emprValue", rowNum, btd.EmplrCont.ToString());
                            }
                            dtEmps.SetValue("Active", rowNum, btd.FlgActive == true ? "Y" : "N");
                            rowNum++;
                        }
                        addEmptyRow();
                        mtEmp.LoadFromDataSource();
                        //oForm.Items.Item("btProcess").Enabled = true;
                    }

                }
                oForm.Freeze(false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("fill record : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void addEmptyRow()
        {
            try
            {
                if (dtEmps.Rows.Count == 0)
                {
                    dtEmps.Rows.Add(1);
                    dtEmps.SetValue("id", 0, "0");
                    dtEmps.SetValue("pick", 0, strCfl);
                    dtEmps.SetValue("hrmsId", 0, "");
                    dtEmps.SetValue("EmpName", 0, "");
                    dtEmps.SetValue("ValType", 0, cbValType.Value.ToString());
                    dtEmps.SetValue("Value", 0, txValue.Value.ToString());
                    dtEmps.SetValue("Active", 0, "Y");
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
                        dtEmps.SetValue("ValType", dtEmps.Rows.Count - 1, cbValType.Value.ToString());
                        dtEmps.SetValue("Value", dtEmps.Rows.Count - 1, txValue.Value.ToString());
                        dtEmps.SetValue("Active", dtEmps.Rows.Count - 1, "Y");
                        mtEmp.AddRow(1, mtEmp.RowCount + 1);
                    }

                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function addEmptyRow.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }


        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            IniContrls();
        }

        private void submitForm()
        {
            string strProcessing = "";
            try
            {
                if (string.IsNullOrEmpty(cbPeriod.Value) || cbPeriod.Selected.Value == "0")
                {
                    oApplication.StatusBar.SetText("Please select valid Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txElCode.Value))
                {
                    oApplication.StatusBar.SetText("Please select valid Element", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                mtEmp.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                TrnsBatches eleBatch;
                int cnt = (from p in dbHrPayroll.TrnsBatches where p.Id.ToString() == txDocNum.Value.ToString() select p).Count();
                if (cnt > 0)
                {
                    eleBatch = (from p in dbHrPayroll.TrnsBatches where p.Id.ToString() == txDocNum.Value.ToString() select p).FirstOrDefault();
                }
                else
                {
                    eleBatch = new TrnsBatches();
                    eleBatch.MstElements = (from p in dbHrPayroll.MstElements where p.ElementName == txElCode.Value.ToString() && p.Id == elementId select p).FirstOrDefault();
                    eleBatch.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value select p).FirstOrDefault();
                    eleBatch.CreateDate = DateTime.Now;
                    eleBatch.UserId = oCompany.UserName;
                    eleBatch.PayrollID = Convert.ToInt32(cbProll.Value);
                    dbHrPayroll.TrnsBatches.InsertOnSubmit(eleBatch);
                }
                eleBatch.PayrollName = cbProll.Selected.Description;
                eleBatch.PayrollPeriodID = Convert.ToInt32(cbPeriod.Value);
                eleBatch.PayrollPeriod = cbPeriod.Selected.Description;
                eleBatch.BatchName = txBchName.Value;
                eleBatch.ElmtType = txEleType.Value;
                eleBatch.ValType = cbValType.Value.ToString();
                eleBatch.Value = Convert.ToDecimal(txValue.Value);
                eleBatch.EmplrCont = Convert.ToDecimal(txEmprCont.Value);
                eleBatch.BatchStatus = Convert.ToInt16(cbStatus.Value.ToString());
                eleBatch.UpdateDate = DateTime.Now;
                eleBatch.UpdatedBy = oCompany.UserName;
                for (int i = 0; i < dtEmps.Rows.Count; i++)
                {

                    code = Convert.ToString(dtEmps.GetValue("hrmsId", i));
                    code = code.Trim();
                    if (code != "")
                    {

                        TrnsBatchesDetails btchDetail;
                        int detailId = Convert.ToInt32(dtEmps.GetValue("id", i));
                        if (detailId > 0)
                        {
                            btchDetail = (from p in dbHrPayroll.TrnsBatchesDetails where p.Id.ToString() == detailId.ToString() select p).FirstOrDefault();
                        }
                        else
                        {
                            btchDetail = new TrnsBatchesDetails();
                            btchDetail.CreateDate = DateTime.Now;
                            btchDetail.UserId = oCompany.UserName;

                            eleBatch.TrnsBatchesDetails.Add(btchDetail);
                        }

                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == code select p).FirstOrDefault();
                        if (emp == null)
                        {
                            oApplication.StatusBar.SetText("Employee with EmpId " + code + " not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                        strProcessing = "Error in Setting Employee Record with Employee ID --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + "  ";

                        btchDetail.EmployeeID = emp.ID;
                        btchDetail.ValueType = dtEmps.GetValue("ValType", i);
                        btchDetail.Value = Convert.ToDecimal(dtEmps.GetValue("Value", i));
                        btchDetail.EmplrCont = Convert.ToDecimal(dtEmps.GetValue("emprValue", i));
                        btchDetail.UpdateDate = DateTime.Now;
                        btchDetail.UpdatedBy = oCompany.UserName;
                        btchDetail.FlgActive = Convert.ToString(dtEmps.GetValue("Active", i)) == "Y" ? true : false;

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
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(strProcessing + "Error in Function submitForm.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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

        private void UpdateBalanceAmount()
        {
            try
            {
                int i = 0;
                int empcnt = dtEmps.Rows.Count;
                for (i = 0; i < empcnt; i++)
                {
                    string strHrmsId = Convert.ToString(dtEmps.GetValue("hrmsId", i));
                    if (strHrmsId.Trim() != "")
                    {
                        MstEmployee oEmployee = (from a in dbHrPayroll.MstEmployee
                                                 where a.EmpID == strHrmsId
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
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);

            }

        }

        private void processBatch()
        {
            int confirm = oApplication.MessageBox("Selected template will impact salaries of employees, do you want to proceed?", 1, "Yes", "No");
            if (confirm == 2)
            {
                oApplication.StatusBar.SetText("Processing canceled", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return;
            }
            int i = 0;
            int empcnt = dtEmps.Rows.Count;
            string cbSel = cbIfExst.Value.Trim();
            SAPbouiCOM.ProgressBar prog = oApplication.StatusBar.CreateProgressBar("Updating Employee Elements", empcnt, false);
            prog.Value = 0;
            try
            {
                for (i = 0; i < empcnt; i++)
                {
                    string strHrmsId = Convert.ToString(dtEmps.GetValue("hrmsId", i));
                    if (strHrmsId.Trim() != "")
                    {
                        prog.Value += 1;
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == strHrmsId select p).FirstOrDefault();
                        if (elementId != 0)
                        {
                            TrnsEmployeeElement empElement;
                            int cnt = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID == emp.EmpID select p).Count();
                            if (cnt > 0)
                            {
                                empElement = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID == emp.EmpID select p).FirstOrDefault();
                            }
                            else
                            {
                                empElement = new TrnsEmployeeElement();
                                empElement.CreateDate = DateTime.Now;
                                empElement.MstEmployee = emp;
                                empElement.UpdateDate = DateTime.Now;
                                empElement.UserId = oCompany.UserName;
                            }
                            TrnsEmployeeElementDetail trntEle;
                            trntEle = new TrnsEmployeeElementDetail();

                            // mtElement.SetLineData(i+1);
                            MstElements mstele = (from p in dbHrPayroll.MstElements where p.ElementName == txElCode.Value.Trim() && p.Id == elementId select p).FirstOrDefault();
                            if (cbSel == "2")
                            {
                                int cntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == mstele.Id select p).Count();
                                if (cntEle > 0)
                                {
                                    trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == mstele.Id select p).FirstOrDefault();

                                }
                                else
                                {
                                    empElement.TrnsEmployeeElementDetail.Add(trntEle);
                                }
                            }
                            if (cbSel == "1")
                            {
                                empElement.TrnsEmployeeElementDetail.Add(trntEle);
                            }
                            trntEle.RetroAmount = Convert.ToDecimal(0.00);
                            trntEle.FlgRetro = false;
                            trntEle.FlgActive = dtEmps.GetValue("Active", i) == "Y" ? true : false;
                            if (mstele.Type == "Non-Rec")
                            {
                                trntEle.FlgOneTimeConsumed = false;
                                trntEle.PeriodId = Convert.ToInt32(cbPeriod.Value);
                            }
                            string strEmplrContr = Convert.ToString(dtEmps.GetValue("emprValue", i));

                            trntEle.ElementType = Convert.ToString(mstele.ElmtType);
                            trntEle.ValueType = Convert.ToString(dtEmps.GetValue("ValType", i));
                            trntEle.Value = Convert.ToDecimal(dtEmps.GetValue("Value", i));
                            trntEle.EmpContr = Convert.ToDecimal(dtEmps.GetValue("Value", i));

                            if (!string.IsNullOrEmpty(strEmplrContr))
                            {
                                trntEle.EmplrContr = Convert.ToDecimal(dtEmps.GetValue("emprValue", i));
                            }
                            //incase of Fix value.Amount will be same as value
                            trntEle.Amount = ds.getElementAmount(emp, trntEle.ValueType, (decimal)trntEle.Value);
                            if (trntEle.ValueType.Trim() == "FIX")
                            {
                                trntEle.Amount = Convert.ToDecimal(dtEmps.GetValue("Value", i));
                            }
                            trntEle.MstElements = mstele;

                        }
                    }

                }
                TrnsBatches record = batchs.ElementAt<TrnsBatches>(currentRecord);
                record.BatchStatus = 2;

                dbHrPayroll.SubmitChanges();
                UpdateBalanceAmount();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function processBatch.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            prog.Stop();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
            prog = null;
            _fillFields();
        }

        private void getFileName()
        {
            try
            {
                string fileName = Program.objHrmsUI.FindFile();
                if (String.IsNullOrEmpty(fileName))
                {
                    oApplication.SetStatusBarMessage("Select a template file");
                    return;
                }
                txFilenam.Value = fileName;
                System.Data.DataTable dt = new System.Data.DataTable();
                fillDtFromTemplate(dt);
                int rowNum = 1;
                dtEmps.Rows.Clear();
                if (dt.Rows.Count > 0)
                {
                    // dtEmps.Rows.Add(dt.Rows.Count);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strEmpCode = dr["EmpCode"].ToString();
                        string strEmpName = dr["EmpName"].ToString();
                        string strValueType = dr["ValType"].ToString();
                        string strEmprValue = dr["EmprVal"].ToString();
                        string strEmployeeValue = dr["EmpVal"].ToString();
                        string strActive = dr["Active"].ToString();
                        if (!string.IsNullOrEmpty(strEmpCode))
                        {
                            dtEmps.Rows.Add(1);
                            dtEmps.SetValue("id", rowNum - 1, "0");
                            dtEmps.SetValue("empId", rowNum - 1, strEmpCode);
                            dtEmps.SetValue("hrmsId", rowNum - 1, dr["EmpCode"].ToString());
                            dtEmps.SetValue("EmpName", rowNum - 1, strEmpName);
                            dtEmps.SetValue("ValType", rowNum - 1, dr["ValType"].ToString());
                            if (!string.IsNullOrEmpty(strEmployeeValue))
                            {
                                dtEmps.SetValue("Value", rowNum - 1, strEmployeeValue);
                            }
                            else
                            {
                                dtEmps.SetValue("Value", rowNum - 1, "0");
                            }
                            if (!string.IsNullOrEmpty(strEmprValue))
                            {
                                dtEmps.SetValue("emprValue", rowNum - 1, strEmprValue);
                            }
                            else
                            {
                                dtEmps.SetValue("emprValue", rowNum - 1, "0");
                            }
                            dtEmps.SetValue("Active", rowNum - 1, strActive == "1" ? "Y" : "N");
                        }
                        rowNum++;

                    }
                    mtEmp.LoadFromDataSource();
                    oApplication.StatusBar.SetText("Successfully loaded data.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    oForm.Items.Item("btProcess").Enabled = true;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Unable to read file, Invalid file.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void fillDtFromTemplate(System.Data.DataTable dt)
        {
            try
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
            catch
            {
                oApplication.StatusBar.SetText("Unable to read file, Invalid file.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ExecuteFMSonEmpValue()
        {
            oForm.Freeze(true);
            try
            {
                //string GetFixedQuery = "SELECT QString FROM dbo.OUQR WHERE QName = 'BatchTransaction Working'";
                string GetFixedQuery = "SELECT \"QString\" FROM \"OUQR\" WHERE \"QName\" = '" + ACHR.Properties.Settings.Default.EmpValueQuery + "'";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(GetFixedQuery);
                if (!oRecSet.EoF)
                {
                    string FixedQuery = oRecSet.Fields.Item("QString").Value;
                    for (int i = 1; i <= mtEmp.RowCount; i++)
                    {
                        string ReplaceParameter = ACHR.Properties.Settings.Default.EmpValueReplaceID;
                        string grdEmpCode = (mtEmp.Columns.Item("V_2").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value.Trim();
                        string FMSQuery = FixedQuery.Replace(ReplaceParameter, grdEmpCode);
                        SAPbobsCOM.Recordset oLineSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        oLineSet.DoQuery(FMSQuery);
                        if (!oLineSet.EoF)
                        {
                            (mtEmp.Columns.Item("Value").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = Convert.ToString(oLineSet.Fields.Item(0).Value);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ExecuteFMSonEmpValue : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void OpenNewSearchWindow()
        {
            try
            {

                flgValidCall = true;
                Program.EmpID = "";
                string comName = "BtchCreaSearch";
                Program.sqlString = "";
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
        #endregion
      
    }
}
