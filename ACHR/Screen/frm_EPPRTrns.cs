using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;

namespace ACHR.Screen
{
    class frm_EPPRTrns : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.EditText txtEmployeeSelected, txtEmployeeCodeSelected, txtEmployeeSelectedP, txtEmployeeCodeSelectedP, txtNetPay;
        SAPbouiCOM.EditText txtDebitTotal, txtCreditTotal;
        SAPbouiCOM.ComboBox cmbPayroll, cmbLocation, cmbDepartment, cmbPeriod;
        SAPbouiCOM.Button btnEmpFrom, btnEmpTo, btnSearch, btnMain;
        SAPbouiCOM.Matrix grdNPEmp, grdPREmp, grdItems, grdItemsPr, grdJEHead, grdJEDetail;
        SAPbouiCOM.DataTable dtNPEmp, dtPREmp, dtItems, dtItemsPr, dtJEHead, dtJEDetail;
        SAPbouiCOM.Column ceEmpID, ceEmpName, ceSelect;
        SAPbouiCOM.Column cePEmpID, cePEmpName, cePSelect, cePID;
        SAPbouiCOM.Column ciItemCode, ciItemName, ciRate, ciQTY;
        SAPbouiCOM.Column ciPItemCode, ciPItemName, ciPRate, ciPQty, ciPTotalAmount;
        SAPbouiCOM.Column chID, chMemo, chSBO, chJENumber;
        SAPbouiCOM.Column cdAcctCode, cdAcctName, cdDebit, cdCredit;

        SAPbouiCOM.Item ItxtEmpFrom, ItxtEmpTo, ItxtEmployeeSelected, ItxtEmployeeCodeSelected, ItxtEmployeeSelectedP, ItxtEmployeeCodeSelectedP, ItxtNetPay;
        SAPbouiCOM.Item IcmbPayroll, IcmbLocation, IcmbDepartment, IcmbPeriod;
        SAPbouiCOM.Item ItxtDebitTotal, ItxtCreditTotal;
        public DateTime PeriodStartDate, PeriodEndDate;
        Boolean flgEmpFrom, flgEmpTo, flgItemRunOneTime;

        string NPSelectedEmployee, PRSelectedEmployee, SelectedJEDoc;

        public struct ItemSet
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public decimal PerPieceRate { get; set; }
            public decimal Qty { get; set; }
        }

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    #region MainButton
                    if (SaveRecord())
                    {
                        ClearItemsNP();
                    }
                    #endregion
                    break;
                case "mtEmp":
                    #region LoadGridNP
                    if (pVal.Row >= 1 && pVal.Row <= grdNPEmp.RowCount)
                    {
                        try
                        {
                            string empname = Convert.ToString(dtNPEmp.GetValue(ceEmpName.DataBind.Alias, pVal.Row - 1));
                            string empcode = Convert.ToString(dtNPEmp.GetValue(ceEmpID.DataBind.Alias, pVal.Row - 1));
                            txtEmployeeSelected.Value = empname;
                            txtEmployeeCodeSelected.Value = empcode;
                            NPSelectedEmployee = empcode;
                            FillSelectedEmployeeItemNP(empcode);                            
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        txtEmployeeSelected.Value = "";
                        txtEmployeeCodeSelected.Value = "";
                    }
                    #endregion
                    break;
                case "mtEmpPr":
                    #region LoadGridP
                    if (pVal.Row >= 1 && pVal.Row <= grdPREmp.RowCount)
                    {
                        try
                        {
                            string empcode, empname, totalamount;
                            empname = Convert.ToString(dtPREmp.GetValue(ceEmpName.DataBind.Alias, pVal.Row - 1));
                            empcode = Convert.ToString(dtPREmp.GetValue(ceEmpID.DataBind.Alias, pVal.Row - 1));
                            txtEmployeeSelectedP.Value = empname;
                            txtEmployeeCodeSelectedP.Value = empcode;
                            PRSelectedEmployee = empcode;
                            FillSelectedEmployeeItemProcessed(empcode);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        txtEmployeeSelectedP.Value = "";
                        txtEmployeeCodeSelectedP.Value = "";
                        PRSelectedEmployee = string.Empty;
                    }
                    #endregion
                    break;
                case "mtjedoc":
                    #region JEHeadClick
                    if (pVal.Row >= 1 && pVal.Row <= grdJEHead.RowCount)
                    {
                        try
                        {
                            string jeid;
                            jeid = Convert.ToString(dtJEHead.GetValue(chJENumber.DataBind.Alias, pVal.Row - 1));
                            SelectedJEDoc = jeid;
                            FillSelectedJEDocDetail(jeid);
                        }
                        catch
                        {
                            SelectedJEDoc = string.Empty;
                        }
                    }
                    else
                    {
                        SelectedJEDoc = string.Empty;
                    }
                    #endregion
                    break;
                case "btProces":
                    Processing();
                    break;
                case "btVoid":
                    #region Void Processed
                    VoidProcessing();
                    #endregion
                    break;
                case "btpost":
                    PostProcessed();
                    break;
                case "btjevoid":
                    VoidJEDoc();
                    break;
                case "btjepost":
                    PostInSBO();
                    break;
            }
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
                switch (pVal.ItemUID)
                {
                    case "cbPayroll":
                    case "cbDept":
                    case "cbLoc":
                    case "cbPeriod":
                       RefreshEmployeeDisplay();
                        break;
                }
                if (pVal.ItemUID == "cbPayroll")
                {
                    //FillPeriod(cmbPeriod.Value);
                    FillPeriod(cmbPayroll.Value);
                }

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }

        }

        #endregion

        #region Function

        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            try
            {
                oForm.DataSources.UserDataSources.Add("txemp", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150); // Hours Per Day
                txtEmployeeSelected = oForm.Items.Item("txemp").Specific;
                ItxtEmployeeSelected = oForm.Items.Item("txemp");
                txtEmployeeSelected.DataBind.SetBound(true, "", "txemp");

                oForm.DataSources.UserDataSources.Add("txcode", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150); // Hours Per Day
                txtEmployeeCodeSelected = oForm.Items.Item("txcode").Specific;
                ItxtEmployeeCodeSelected = oForm.Items.Item("txcode");
                txtEmployeeCodeSelected.DataBind.SetBound(true, "", "txcode");

                oForm.DataSources.UserDataSources.Add("txempP", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150); // Hours Per Day
                txtEmployeeSelectedP = oForm.Items.Item("txempP").Specific;
                ItxtEmployeeSelectedP = oForm.Items.Item("txempP");
                txtEmployeeSelectedP.DataBind.SetBound(true, "", "txempP");

                oForm.DataSources.UserDataSources.Add("txcodeP", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150); // Hours Per Day
                txtEmployeeCodeSelectedP = oForm.Items.Item("txcodeP").Specific;
                ItxtEmployeeCodeSelectedP = oForm.Items.Item("txcodeP");
                txtEmployeeCodeSelectedP.DataBind.SetBound(true, "", "txcodeP");

                oForm.DataSources.UserDataSources.Add("txnetpay", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtNetPay = oForm.Items.Item("txnetpay").Specific;
                ItxtNetPay = oForm.Items.Item("txnetpay");
                txtNetPay.DataBind.SetBound(true, "", "txnetpay");

                oForm.DataSources.UserDataSources.Add("txTotDeb", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtDebitTotal = oForm.Items.Item("txTotDeb").Specific;
                ItxtDebitTotal = oForm.Items.Item("txTotDeb");
                txtDebitTotal.DataBind.SetBound(true, "", "txTotDeb");
                ItxtDebitTotal.Enabled = false;

                oForm.DataSources.UserDataSources.Add("txTotCred", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtCreditTotal = oForm.Items.Item("txTotCred").Specific;
                ItxtCreditTotal = oForm.Items.Item("txTotCred");
                txtCreditTotal.DataBind.SetBound(true, "", "txTotCred");
                ItxtCreditTotal.Enabled = false;

                oForm.DataSources.UserDataSources.Add("cbLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cmbLocation = oForm.Items.Item("cbLoc").Specific;
                IcmbLocation = oForm.Items.Item("cbLoc");
                cmbLocation.DataBind.SetBound(true, "", "cbLoc");

                oForm.DataSources.UserDataSources.Add("cbDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cmbDepartment = oForm.Items.Item("cbDept").Specific;
                IcmbDepartment = oForm.Items.Item("cbDept");
                cmbDepartment.DataBind.SetBound(true, "", "cbDept");

                oForm.DataSources.UserDataSources.Add("cbPayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cmbPayroll = oForm.Items.Item("cbPayroll").Specific;
                IcmbPayroll = oForm.Items.Item("cbPayroll");
                cmbPayroll.DataBind.SetBound(true, "", "cbPayroll");

                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cmbPeriod = oForm.Items.Item("cbPeriod").Specific;
                IcmbPeriod = oForm.Items.Item("cbPeriod");
                cmbPeriod.DataBind.SetBound(true, "", "cbPeriod");

                //Not Process
                grdNPEmp = oForm.Items.Item("mtEmp").Specific;
                dtNPEmp = oForm.DataSources.DataTables.Item("dtEmployee");
                ceEmpID = grdNPEmp.Columns.Item("clempid");
                ceEmpID.TitleObject.Sortable = false;
                ceEmpName = grdNPEmp.Columns.Item("clEname");
                ceEmpName.TitleObject.Sortable = false;
                ceSelect = grdNPEmp.Columns.Item("clselect");
                ceSelect.TitleObject.Sortable = false;

                
                grdItems = oForm.Items.Item("mtItem").Specific;
                dtItems = oForm.DataSources.DataTables.Item("dtItems");
                ciItemCode = grdItems.Columns.Item("clIcode");
                ciItemName = grdItems.Columns.Item("clIname");
                ciRate = grdItems.Columns.Item("clPrice");
                ciQTY = grdItems.Columns.Item("clqty");

                //Processed

                grdPREmp = oForm.Items.Item("mtEmpPr").Specific;
                dtPREmp = oForm.DataSources.DataTables.Item("dtEmpsPr");
                cePEmpID = grdPREmp.Columns.Item("clempid");
                cePEmpID.TitleObject.Sortable = false;
                cePEmpName = grdPREmp.Columns.Item("clEname");
                cePEmpName.TitleObject.Sortable = false;
                cePSelect = grdPREmp.Columns.Item("clselect");
                cePSelect.TitleObject.Sortable = false;
                cePID = grdPREmp.Columns.Item("clid");
                cePID.TitleObject.Sortable = false;
                cePID.Visible = false;

                grdItemsPr = oForm.Items.Item("mtItemPr").Specific;
                dtItemsPr = oForm.DataSources.DataTables.Item("dtItemPr");
                ciPItemCode = grdItemsPr.Columns.Item("clIcode");
                ciPItemCode.TitleObject.Sortable = false;
                ciPItemName = grdItemsPr.Columns.Item("clIname");
                ciPItemName.TitleObject.Sortable = false;
                ciPRate = grdItemsPr.Columns.Item("clPrice");
                ciPRate.TitleObject.Sortable = false;
                ciPQty = grdItemsPr.Columns.Item("clqty");
                ciPQty.TitleObject.Sortable = false;
                ciPTotalAmount = grdItemsPr.Columns.Item("clamt");
                ciPTotalAmount.TitleObject.Sortable = false;

                //JE Detials

                grdJEHead = oForm.Items.Item("mtjedoc").Specific;
                dtJEHead = oForm.DataSources.DataTables.Item("dtJEDoc");
                chID = grdJEHead.Columns.Item("clid");
                chMemo = grdJEHead.Columns.Item("cldesc");
                chSBO = grdJEHead.Columns.Item("clsbje");
                chJENumber = grdJEHead.Columns.Item("cljenum");

                grdJEDetail = oForm.Items.Item("mtJeDet").Specific;
                dtJEDetail = oForm.DataSources.DataTables.Item("dtJeDet");
                cdAcctCode = grdJEDetail.Columns.Item("acctCode");
                cdAcctName = grdJEDetail.Columns.Item("acctName");
                cdDebit = grdJEDetail.Columns.Item("debit");
                cdCredit = grdJEDetail.Columns.Item("credit");

                fillCbs();

                btnMain = oForm.Items.Item("1").Specific;
                GetNotProcessedEmployee();
                GetProcessedEmployee();
                
                oForm.PaneLevel = 1;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("intialization exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void fillCbs()
        {
            try
            {
                IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                cmbPayroll.ValidValues.Add("0", "Select Payroll.");
                foreach (CfgPayrollDefination pr in prs)
                {
                    cmbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);
                }
                cmbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                IEnumerable<MstDepartment> depts = from p in dbHrPayroll.MstDepartment select p;
                cmbDepartment.ValidValues.Add("0", "All");
                foreach (MstDepartment dept in depts)
                {
                    cmbDepartment.ValidValues.Add(dept.ID.ToString(), dept.DeptName);
                }
                cmbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation select p;
                cmbLocation.ValidValues.Add("0", "All");
                foreach (MstLocation loc in locs)
                {
                    cmbLocation.ValidValues.Add(loc.Id.ToString(), loc.Description);

                }
                cmbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("fillCbs Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormFrom()
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
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormTo()
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
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetNotProcessedEmployee()
        {
            try
            {
                string Department, Location, Payroll,Period;
                
                Department = cmbDepartment.Value.Trim();
                Location = cmbLocation.Value.Trim();
                Payroll = cmbPayroll.Value.Trim();
                Period = cmbPeriod.Value.Trim();

                string strQuery = @"
                                    SELECT A1.EmpID, A1.FirstName + ' ' + A1.LastName AS EmpName, 'Y' AS [Select]
                                    FROM dbo.MstEmployee A1 INNER JOIN  dbo.TrnsEmployeePerPieceRate A2 ON A1.ID = A2.EmpID
                                    WHERE ISNULL(A1.flgActive,0) = 1 AND A1.ResignDate IS NULL 
	                                    AND A1.ID NOT IN (SELECT S1.EmpID FROM dbo.TrnsEmployeePerPieceProcessing S1 WHERE S1.PeriodID = '" + Period + @"' AND ISNULL(s1.flgProcessed,0) = 1)
                                    ORDER BY A1.SortOrder    ";

                DataTable dtRecords = ds.getDataTable(strQuery);

                if (dtRecords.Rows.Count > 0)
                {
                    dtNPEmp.Rows.Clear();
                    dtNPEmp.Rows.Add(dtRecords.Rows.Count);
                    Int32 i = 0;
                    foreach (DataRow drOne in dtRecords.Rows)
                    {
                        dtNPEmp.SetValue(ceEmpID.DataBind.Alias, i, drOne["EmpID"]);
                        dtNPEmp.SetValue(ceEmpName.DataBind.Alias, i, drOne["EmpName"]);
                        dtNPEmp.SetValue(ceSelect.DataBind.Alias, i, "Y");
                        i++;
                    }
                    grdNPEmp.LoadFromDataSource();
                }
                else
                {
                    dtNPEmp.Rows.Clear();
                    grdNPEmp.LoadFromDataSource();
                    btnMain.Caption = "Save";
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetSelectedNPEmployee Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetProcessedEmployee()
        {
            try
            {
                string Department, Location, Payroll, Period;

                Department = cmbDepartment.Value.Trim();
                Location = cmbLocation.Value.Trim();
                Payroll = cmbPayroll.Value.Trim();
                Period = cmbPeriod.Value.Trim();

                string strQuery = @"
                SELECT  
	                A1.EmpID, A1.FirstName + ' ' + A1.LastName AS EmpName, 'Y' AS [Select], A2.internalID AS ID
                FROM 
	                dbo.MstEmployee A1 INNER JOIN dbo.TrnsEmployeePerPieceProcessing A2 ON A1.ID = A2.EmpID
                WHERE 
	                ISNULL(A1.flgActive,0) = 1 AND A2.PeriodID = '"+ Period +@"' AND ISNULL(A2.flgProcessed,0) = 1                                    
                ORDER BY A1.SortOrder ";

                DataTable dtRecords = ds.getDataTable(strQuery);

                if (dtRecords.Rows.Count > 0)
                {
                    dtPREmp.Rows.Clear();
                    dtPREmp.Rows.Add(dtRecords.Rows.Count);
                    Int32 i = 0;
                    foreach (DataRow drOne in dtRecords.Rows)
                    {
                        dtPREmp.SetValue(ceEmpID.DataBind.Alias, i, drOne["EmpID"]);
                        dtPREmp.SetValue(ceEmpName.DataBind.Alias, i, drOne["EmpName"]);
                        dtPREmp.SetValue(cePID.DataBind.Alias, i, Convert.ToInt32(drOne["ID"]));
                        dtPREmp.SetValue(ceSelect.DataBind.Alias, i, "Y");
                        i++;
                    }
                    grdPREmp.LoadFromDataSource();
                }
                else
                {
                    dtPREmp.Rows.Clear();
                    grdPREmp.LoadFromDataSource();
                    btnMain.Caption = "Save";
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetProcessedEmployee Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetJEDocuments()
        {
            try
            {
                string Department, Location, Payroll, Period;

                Department = cmbDepartment.Value.Trim();
                Location = cmbLocation.Value.Trim();
                Payroll = cmbPayroll.Value.Trim();
                Period = cmbPeriod.Value.Trim();

                string strQuery = @"
                SELECT 
	                A1.ID, A1.Memo, ISNULL(A1.SBOJeNum,0) AS SBOJeNum 
                FROM 
	                dbo.trnsJE A1
                WHERE 
	                A1.periodId = '" + Period + @"' AND A1.Memo LIKE '%PP%'
                ";

                DataTable dtRecords = ds.getDataTable(strQuery);
                if (dtRecords.Rows.Count > 0)
                {
                    dtJEHead.Rows.Clear();
                    dtJEHead.Rows.Add(dtRecords.Rows.Count);
                    Int32 i = 0;
                    foreach (DataRow drOne in dtRecords.Rows)
                    {
                        dtJEHead.SetValue(chID.DataBind.Alias, i, i+1);
                        dtJEHead.SetValue(chMemo.DataBind.Alias, i, drOne["Memo"]);
                        dtJEHead.SetValue(chJENumber.DataBind.Alias, i, drOne["ID"]);
                        dtJEHead.SetValue(chSBO.DataBind.Alias, i, drOne["SBOJeNum"]);
                        i++;
                    }
                    grdJEHead.LoadFromDataSource();
                }
                else
                {
                    dtJEHead.Rows.Clear();
                    grdJEHead.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetJEDocuments Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private bool SaveRecords()
        {
            bool flgReturn = false;
            try
            {
                if (btnMain.Caption == "Save")
                {
                    List<string> lstEmpID = new List<string>();
                    List<ItemSet> lstItems = new List<ItemSet>();
                    grdNPEmp.FlushToDataSource();
                    for (int i = 0; i < dtNPEmp.Rows.Count; i++)
                    {
                        string empid = dtNPEmp.GetValue(ceEmpID.DataBind.Alias, i);
                        string selected = dtNPEmp.GetValue(ceSelect.DataBind.Alias, i);
                        if (!string.IsNullOrEmpty(empid) && selected == "Y")
                        {
                            lstEmpID.Add(empid);
                        }
                    }
                    grdItems.FlushToDataSource();
                    for (int i = 0; i < dtItems.Rows.Count; i++)
                    {
                        string itemcode, itemname, perpiecerate, Qty, lineTolal;
                        itemcode = dtItems.GetValue(ciItemCode.DataBind.Alias, i);
                        itemname = dtItems.GetValue(ciItemName.DataBind.Alias, i);
                        perpiecerate = string.Format("{0:0.00}", Convert.ToString(dtItems.GetValue(ciRate.DataBind.Alias, i)));
                        Qty = string.Format("{0:0.00}", Convert.ToString(dtItems.GetValue(ciQTY.DataBind.Alias, i)));
                        
                        //selected = dtItems.GetValue(ciActive.DataBind.Alias, i);
                        //if (!string.IsNullOrEmpty(itemcode) && selected == "Y")
                        if (!string.IsNullOrEmpty(itemcode))
                        {
                            var InList = new ItemSet();
                            InList.ItemCode = itemcode;
                            InList.ItemName = itemname;
                            InList.PerPieceRate = Convert.ToDecimal(perpiecerate);
                            InList.Qty = Convert.ToDecimal(Qty);
                            
                            lstItems.Add(InList);
                        }
                    }
                    if (lstEmpID.Count > 0)
                    {
                        if (lstItems.Count > 0)
                        {
                            foreach (var one in lstEmpID)
                            {   
                               MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == one select a).FirstOrDefault();
                               
                                if (oEmp == null) continue;
                                int Count = 0;
                                CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cmbPayroll.Value.ToString() select p).Single();
                                CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cmbPeriod.Value.ToString() select p).Single();
                               // Count = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.EmpID == oEmp.ID select a).Count();
                                Count = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.EmpID == oEmp.ID select a).Count();
                                if (Count == 0)
                                {
                                   
                                    TrnsEmployeePerPieceProcessing oDoc = new TrnsEmployeePerPieceProcessing();
                                    dbHrPayroll.TrnsEmployeePerPieceProcessing.InsertOnSubmit(oDoc);
                                    oDoc.MstEmployee = oEmp;
                                    oDoc.EmpName = oEmp.FirstName + " " + oEmp.LastName;
                                    oDoc.PayrollID = oEmp.PayrollID;
                                    oDoc.PayrollName = oEmp.PayrollName;
                                    oDoc.PeriodID = payrollperiod.ID;
                                    oDoc.PeriodName = payrollperiod.PeriodName;
                                    oDoc.FlgProcessed = false;
                                    oDoc.NetPayable = 0;
                                    oDoc.CreateDt = DateTime.Now;
                                    oDoc.UdateDt = DateTime.Now;
                                    oDoc.CreatedBy = oCompany.UserName;
                                    oDoc.UpdatedBy = oCompany.UserName;
                                        foreach (var oneitem in lstItems)
                                        {
                                            TrnsEmployeePerPieceProcessingDetail oDetail = new TrnsEmployeePerPieceProcessingDetail();
                                            oDetail.ItemCode = oneitem.ItemCode;
                                            oDetail.ItemName = oneitem.ItemName;
                                            oDetail.Rate = oneitem.PerPieceRate;
                                            oDetail.Qty = oneitem.Qty;
                                            


                                            oDoc.TrnsEmployeePerPieceProcessingDetail.Add(oDetail);
                                        }
                                    
                                    dbHrPayroll.SubmitChanges();
                                    oApplication.StatusBar.SetText("Added Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                }
                                else
                                {
                                    TrnsEmployeePerPieceProcessing oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.EmpID == oEmp.ID select a).FirstOrDefault();
                                    if (oDoc == null) continue;
                                    oDoc.UdateDt = DateTime.Now;
                                    oDoc.UpdatedBy = oCompany.UserName;
                                    foreach (var oneitem in lstItems)
                                    {
                                        int linecount = 0;
                                        linecount = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessingDetail where a.ItemCode == oneitem.ItemCode && a.FKID == oDoc.InternalID  select a).Count();
                                        if (linecount == 0)
                                        {
                                            TrnsEmployeePerPieceProcessingDetail oDetail = new TrnsEmployeePerPieceProcessingDetail();
                                            oDetail.ItemCode = oneitem.ItemCode;
                                            oDetail.ItemName = oneitem.ItemName;
                                            oDetail.Rate = oneitem.PerPieceRate;
                                            oDetail.Qty = oneitem.Qty;
                                            

                                            oDoc.TrnsEmployeePerPieceProcessingDetail.Add(oDetail);
                                        }
                                        else
                                        {
                                            TrnsEmployeePerPieceProcessingDetail oDetail = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessingDetail where a.ItemCode == oneitem.ItemCode && a.FKID == oDoc.InternalID  select a).FirstOrDefault();
                                            oDetail.ItemCode = oneitem.ItemCode;
                                            oDetail.ItemName = oneitem.ItemName;
                                            oDetail.Rate = oneitem.PerPieceRate;
                                            oDetail.Qty = oneitem.Qty;
                                        }
                                        dbHrPayroll.SubmitChanges();
                                    }
                                    oApplication.StatusBar.SetText("Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                                }
                                flgReturn = true;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("No items selected.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            flgReturn = false;
                        }
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("No employee selected.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        flgReturn = false;
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SaveRecords : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                flgReturn = false;
            }
            return flgReturn;
        }

        private bool SaveRecord()
        {
            try
            {
                if (string.IsNullOrEmpty(NPSelectedEmployee)) return false;
                string SelectEmployee;
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == NPSelectedEmployee select a).FirstOrDefault();
                if (oEmp == null) return false;
                TrnsEmployeePerPieceProcessing oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.EmpID == oEmp.ID && a.PeriodID.ToString() == cmbPeriod.Value.Trim() select a).FirstOrDefault();
                CfgPeriodDates oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.ID.ToString() == cmbPeriod.Value.Trim() select a).FirstOrDefault();
                if (oDoc == null)
                {
                    oDoc = new TrnsEmployeePerPieceProcessing();
                    dbHrPayroll.TrnsEmployeePerPieceProcessing.InsertOnSubmit(oDoc);
                    oDoc.MstEmployee = oEmp;
                    oDoc.EmpName = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                    oDoc.PayrollID = oEmp.PayrollID;
                    oDoc.PayrollName = oEmp.PayrollName;
                    oDoc.PeriodID = oPeriod.ID;
                    oDoc.PeriodName = oPeriod.PeriodName;
                    oDoc.FlgProcessed = false;
                    oDoc.NetPayable = 0;
                    oDoc.CreateDt = DateTime.Now;
                    oDoc.UdateDt = DateTime.Now;
                    oDoc.CreatedBy = oCompany.UserName;
                    oDoc.UpdatedBy = oCompany.UserName;
                    grdItems.FlushToDataSource();
                    for (int i = 0; i < dtItems.Rows.Count; i++)
                    {
                        string itemcode, itemname, perpiecerate, Qty, lineTolal;
                        itemcode = dtItems.GetValue(ciItemCode.DataBind.Alias, i);
                        itemname = dtItems.GetValue(ciItemName.DataBind.Alias, i);
                        perpiecerate = string.Format("{0:0.00}", Convert.ToString(dtItems.GetValue(ciRate.DataBind.Alias, i)));
                        Qty = string.Format("{0:0.00}", Convert.ToString(dtItems.GetValue(ciQTY.DataBind.Alias, i)));
                        if (Convert.ToDecimal(Qty) > 0)
                        {
                            TrnsEmployeePerPieceProcessingDetail oDetail = new TrnsEmployeePerPieceProcessingDetail();
                            oDetail.ItemCode = itemcode;
                            oDetail.ItemName = itemname;
                            oDetail.Rate = Convert.ToDecimal(perpiecerate);
                            oDetail.Qty = Convert.ToDecimal(Qty);
                            oDetail.LineTotal = Convert.ToDecimal(perpiecerate) * Convert.ToDecimal(Qty);
                            oDoc.TrnsEmployeePerPieceProcessingDetail.Add(oDetail);
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    return true;
                }
                else
                {
                    oDoc.FlgProcessed = false;
                    oDoc.NetPayable = 0;
                    oDoc.UdateDt = DateTime.Now;
                    oDoc.UpdatedBy = oCompany.UserName;
                    grdItems.FlushToDataSource();
                    for (int i = 0; i < dtItems.Rows.Count; i++)
                    {
                        string itemcode, itemname, perpiecerate, Qty, lineTolal;
                        itemcode = dtItems.GetValue(ciItemCode.DataBind.Alias, i);
                        itemname = dtItems.GetValue(ciItemName.DataBind.Alias, i);
                        perpiecerate = string.Format("{0:0.00}", Convert.ToString(dtItems.GetValue(ciRate.DataBind.Alias, i)));
                        Qty = string.Format("{0:0.00}", Convert.ToString(dtItems.GetValue(ciQTY.DataBind.Alias, i)));
                        if (Convert.ToDecimal(Qty) > 0)
                        {
                            int LineCheck = 0;
                            LineCheck = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessingDetail where a.FKID == oDoc.InternalID && a.ItemCode == itemcode select a).Count();
                            if (LineCheck == 0)
                            {
                                TrnsEmployeePerPieceProcessingDetail oDetail = new TrnsEmployeePerPieceProcessingDetail();
                                oDetail.ItemCode = itemcode;
                                oDetail.ItemName = itemname;
                                oDetail.Rate = Convert.ToDecimal(perpiecerate);
                                oDetail.Qty = Convert.ToDecimal(Qty);
                                oDetail.LineTotal = Convert.ToDecimal(perpiecerate) * Convert.ToDecimal(Qty);
                                oDoc.TrnsEmployeePerPieceProcessingDetail.Add(oDetail);
                            }
                            else
                            {
                                TrnsEmployeePerPieceProcessingDetail oDetail = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessingDetail where a.FKID == oDoc.InternalID && a.ItemCode == itemcode select a).FirstOrDefault(); 
                                oDetail.Rate = Convert.ToDecimal(perpiecerate);
                                oDetail.Qty = Convert.ToDecimal(Qty);
                                oDetail.LineTotal = Convert.ToDecimal(perpiecerate) * Convert.ToDecimal(Qty);
                            }
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SaveRecord Ex : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
        }

        private void ClearItemsNP()
        {
            try
            {
                dtItems.Rows.Clear();
                grdItems.LoadFromDataSource();
                NPSelectedEmployee = string.Empty;
                txtEmployeeSelected.Value = string.Empty;
                txtEmployeeCodeSelected.Value = string.Empty;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ClearItemsNP : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillSelectedEmployeeItemNP(string pempid)
        {
            try
            {
                string Period = string.Empty;
                Period = cmbPeriod.Value.Trim();
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == pempid select a).FirstOrDefault();
                if (oEmp == null) return;
                TrnsEmployeePerPieceRate oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.EmpID == oEmp.ID select a).FirstOrDefault();
                if (oDoc == null) return;
                //
                TrnsEmployeePerPieceProcessing oProcessing = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.EmpID == oDoc.EmpID && a.PeriodID.ToString() == Period select a).FirstOrDefault();
                dtItems.Rows.Clear();
                int i = 0;
                if (oProcessing == null)
                {
                    foreach (var OneItem in oDoc.TrnsEmployeePerPieceRateDetail)
                    {
                        if (OneItem.FlgActive == true)
                        {

                            dtItems.Rows.Add(1);
                            dtItems.SetValue(ciItemCode.DataBind.Alias, i, OneItem.ItemCode);
                            dtItems.SetValue(ciItemName.DataBind.Alias, i, OneItem.ItemName);
                            dtItems.SetValue(ciRate.DataBind.Alias, i, Convert.ToString(OneItem.Rate));
                            dtItems.SetValue(ciQTY.DataBind.Alias, i, "");
                           // dtItems.SetValue(ciTotalAmount.DataBind.Alias, i, "");
                            i++;

                        }
                    }
                }
                if (oProcessing != null)
                {
                    var oProcessingdetail = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessingDetail where a.FKID == oProcessing.InternalID select a).ToList();
                    if (oProcessingdetail != null)
                    {
                        foreach (var Processing in oProcessingdetail)
                        {
                            dtItems.Rows.Add(1);
                            dtItems.SetValue(ciItemCode.DataBind.Alias, i, Processing.ItemCode);
                            dtItems.SetValue(ciItemName.DataBind.Alias, i, Processing.ItemName);
                            dtItems.SetValue(ciRate.DataBind.Alias, i, Convert.ToString(Processing.Rate));
                            dtItems.SetValue(ciQTY.DataBind.Alias, i, Convert.ToString(Processing.Qty));
                           // dtItems.SetValue(ciTotalAmount.DataBind.Alias, i, "");
                            i++;
                        }
                    }
                }
                grdItems.LoadFromDataSource();
                //btnMain.Caption = "Save";
                oApplication.StatusBar.SetText("All Items Loaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillSelectedEmployeeItem : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillSelectedEmployeeItemProcessed(string pempid)
        {
            try
            {
                string Period = cmbPeriod.Value.Trim();
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == pempid select a).FirstOrDefault();
                if (oEmp == null) return;
                TrnsEmployeePerPieceProcessing oProcessing = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.EmpID == oEmp.ID && a.FlgProcessed == true && a.PeriodID.ToString() == Period select a).FirstOrDefault();
                if (oProcessing == null) return;
                
                dtItemsPr.Rows.Clear();
                int i = 0;
                decimal netAmount = 0M;
                if (oProcessing != null)
                {
                    foreach (var Detail in oProcessing.TrnsEmployeePerPieceProcessingDetail)
                    {
                        dtItemsPr.Rows.Add(1);
                        dtItemsPr.SetValue(ciPItemCode.DataBind.Alias, i, Detail.ItemCode);
                        dtItemsPr.SetValue(ciPItemName.DataBind.Alias, i, Detail.ItemName);
                        dtItemsPr.SetValue(ciPRate.DataBind.Alias, i, Convert.ToString(Detail.Rate));
                        dtItemsPr.SetValue(ciPQty.DataBind.Alias, i, Convert.ToString(Detail.Qty));
                        dtItemsPr.SetValue(ciPTotalAmount.DataBind.Alias, i, Convert.ToString(Detail.LineTotal));
                        netAmount += Convert.ToDecimal(Detail.LineTotal);
                        i++;
                    }
                }
                grdItemsPr.LoadFromDataSource();
                txtNetPay.Value = Convert.ToString(netAmount);
                oApplication.StatusBar.SetText("Items of EmpCode " + PRSelectedEmployee + " Loaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillSelectedEmployeeItem : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillSelectedJEDocDetail(string pJEDocID)
        {
            try
            {
                decimal debittotal = 0M;
                decimal credittotal = 0M;
                TrnsJE oDoc = (from a in dbHrPayroll.TrnsJE where a.ID.ToString() == pJEDocID select a).FirstOrDefault();
                if (oDoc == null) return;
                dtJEDetail.Rows.Clear();
                int i =0;
                foreach (var one in oDoc.TrnsJEDetail)
                {
                    dtJEDetail.Rows.Add(1);
                    dtJEDetail.SetValue(cdAcctCode.DataBind.Alias, i, one.AcctCode);
                    dtJEDetail.SetValue(cdAcctName.DataBind.Alias, i, one.AcctName);
                    dtJEDetail.SetValue(cdDebit.DataBind.Alias, i, Convert.ToString(one.Debit));
                    dtJEDetail.SetValue(cdCredit.DataBind.Alias, i, Convert.ToString(one.Credit));
                    debittotal += Convert.ToDecimal(one.Debit);
                    credittotal += Convert.ToDecimal(one.Credit);
                    i++;
                }
                grdJEDetail.LoadFromDataSource();
                txtCreditTotal.Value = Convert.ToString(credittotal);
                txtDebitTotal.Value = Convert.ToString(debittotal);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillSelectedJEDocDetail : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
      
        private void FillPeriod(string payroll)
        {
            try
            {
                if (cmbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cmbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cmbPeriod.ValidValues.Remove(cmbPeriod.ValidValues.Item(k).Value);
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
                        cmbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        count++;
                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();
                        //if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                        //{
                        //    selId = pd.ID.ToString();
                        //}
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
                        cmbPeriod.Select(selId);
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
        
        private void RefreshEmployeeDisplay()
        {
            oForm.Freeze(true);
            try
            {
                if (cmbPeriod.Value.ToString() != "")
                {
                    GetNotProcessedEmployee();
                    GetProcessedEmployee();
                    GetJEDocuments();
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("RefreshEmployeeDisplay :" + ex.Message);
            }
            oForm.Freeze(false);
        }       
        
        private void Processing()
        {
            try
            {
                List<string> SelectedEmployees = new List<string>();
                string Payroll, Period;
                Payroll = cmbPayroll.Value.Trim();
                Period = cmbPeriod.Value.Trim();
                grdNPEmp.FlushToDataSource();
                for (int i = 0; i < dtNPEmp.Rows.Count; i++)
                {
                    string empcode, check;
                    empcode = dtNPEmp.GetValue(ceEmpID.DataBind.Alias, i);
                    check = dtNPEmp.GetValue(ceSelect.DataBind.Alias, i);
                    if (check == "Y")
                    {
                        SelectedEmployees.Add(empcode);
                    }
                }
                if (SelectedEmployees.Count > 0)
                {
                    foreach (var One in SelectedEmployees)
                    {
                        decimal NetTotal = 0M;
                        TrnsEmployeePerPieceProcessing oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.MstEmployee.EmpID == One && a.PeriodID.ToString() == Period select a).FirstOrDefault();
                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == One select a).FirstOrDefault();
                        if (oDoc == null || oEmp == null)
                        {
                            oApplication.StatusBar.SetText(One +  "item qty not define.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        MstGLDetermination oEmpGL = ds.getEmpGl(oEmp);
                        oDoc.FlgProcessed = true;
                        oDoc.DebitAccount = oEmpGL.BasicSalary;
                        oDoc.DebitName = oEmpGL.BasicSalaryDesc;
                        oDoc.CreditAccount = oEmpGL.BSPayable;
                        oDoc.CreditName = oEmpGL.BSPayableDesc;
                        foreach (var Detail in oDoc.TrnsEmployeePerPieceProcessingDetail)
                        {
                            NetTotal += Convert.ToDecimal(Detail.LineTotal);
                        }
                        oDoc.NetPayable = NetTotal;
                        oApplication.StatusBar.SetText(One + " Successfull Posted", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                    dbHrPayroll.SubmitChanges();
                }
                RefreshEmployeeDisplay();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ValidatProcessing Ex : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void VoidProcessing()
        {
            try
            {
                List<string> SelectedEmployees = new List<string>();
                string Payroll, Period;
                Payroll = cmbPayroll.Value.Trim();
                Period = cmbPeriod.Value.Trim();
                grdPREmp.FlushToDataSource();
                for (int i = 0; i < dtPREmp.Rows.Count; i++)
                {
                    string empcode, check;
                    empcode = dtPREmp.GetValue(cePEmpID.DataBind.Alias, i);
                    check = dtPREmp.GetValue(cePSelect.DataBind.Alias, i);
                    if (check == "Y")
                    {
                        SelectedEmployees.Add(empcode);
                    }
                }
                if (SelectedEmployees.Count > 0)
                {
                    foreach (var One in SelectedEmployees)
                    {
                        decimal NetTotal = 0M;
                        TrnsEmployeePerPieceProcessing oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.MstEmployee.EmpID == One && a.PeriodID.ToString() == Period select a).FirstOrDefault();
                        if (oDoc == null)
                        {
                            oApplication.StatusBar.SetText(One + "item qty not define.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        oDoc.FlgProcessed = false;
                        oDoc.NetPayable = NetTotal;
                        oApplication.StatusBar.SetText(One + " Successfull Void", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                    dbHrPayroll.SubmitChanges();
                }
                RefreshEmployeeDisplay();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidProcessing Ex : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void VoidJEDoc()
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedJEDoc))
                {
                    TrnsJE oDoc = (from a in dbHrPayroll.TrnsJE where a.ID.ToString() == SelectedJEDoc select a).FirstOrDefault();
                    if (oDoc == null) return;
                    var oCollection = (from a in dbHrPayroll.TrnsEmployeePerPieceProcessing where a.JENum == oDoc.ID select a).ToList();
                    foreach (var One in oCollection)
                    {
                        One.FlgPosted = false;
                        One.JENum = 0;
                    }
                    dbHrPayroll.TrnsJE.DeleteOnSubmit(oDoc);
                    dbHrPayroll.SubmitChanges();
                    SelectedJEDoc = string.Empty;
                }
                RefreshEmployeeDisplay();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidJEDoc : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostProcessed()
        {
            try
            {
                int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                string Payroll, Period;
                Payroll = cmbPayroll.Value.Trim();
                Period = cmbPeriod.Value.Trim();
                CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == Period select p).FirstOrDefault();
                int totalCnt = 0;
                string spIds = "0";
                grdPREmp.FlushToDataSource();
                for (int i = 0; i < dtPREmp.Rows.Count; i++)
                {
                    string empcode, check;
                    empcode = dtPREmp.GetValue(cePEmpID.DataBind.Alias, i);
                    check = dtPREmp.GetValue(cePSelect.DataBind.Alias, i);
                    if (check == "Y")
                    {
                        if (totalCnt == 0)
                        {
                            spIds = Convert.ToString(dtPREmp.GetValue(cePID.DataBind.Alias, i));
                        }
                        else
                        {
                            spIds += ", " + Convert.ToString(dtPREmp.GetValue(cePID.DataBind.Alias, i));
                        }
                        totalCnt++;

                    }
                }
                if (spIds == "0")
                {
                    oApplication.SetStatusBarMessage("Select employees to post ");
                }
                SearchKeyVal.Clear();
                SearchKeyVal.Add("spIds", spIds);
                string JeSql = sqlString.getSql("JEQueryPP", SearchKeyVal);

                if (totalCnt > 0)
                {
                    DataTable dtJeDetail = ds.getDataTable(JeSql);
                    if (dtJeDetail.Rows.Count == 0) return;
                    string errMsg = "";
                    string strCode = "";
                    string strName = "";
                    foreach (DataRow dr in dtJeDetail.Rows)
                    {
                        strCode = dr["AcctCode"].ToString();
                        strName = dr["AcctName"].ToString();
                        if (strCode == "Not Found")
                        {
                            errMsg = "GL Missing. Please confirm that GL Determination complete.";
                        }
                    }
                    if (errMsg != "")
                    {
                        oApplication.SetStatusBarMessage(errMsg);
                        return;
                    }
                    TrnsJE je = new TrnsJE();
                    je.CreateDt = DateTime.Now;
                    je.FlgCanceled = false;
                    je.FlgPosted = false;
                    je.SBOJeNum = 0;
                    je.JEPostingDate = payrollperiod.EndDate;
                    je.PayrollID = payrollperiod.CfgPayrollDefination.ID;                                        
                    je.PeriodID = payrollperiod.ID;
                    je.Memo = " Payroll PP JE for period " + payrollperiod.PeriodName;

                    foreach (DataRow dr in dtJeDetail.Rows)
                    {
                        TrnsJEDetail jed = new TrnsJEDetail();
                        jed.AcctCode = dr["AcctCode"].ToString();
                        jed.AcctName = dr["AcctName"].ToString();
                        jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                        jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                        je.TrnsJEDetail.Add(jed);
                    }
                    dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                    dbHrPayroll.SubmitChanges();
                    int jeNum = je.ID;

                    for (int i = 0; i < dtPREmp.Rows.Count; i++)
                    {
                        string sel = dtPREmp.GetValue(cePSelect.DataBind.Alias, i);
                        string id = Convert.ToString(dtPREmp.GetValue(cePID.DataBind.Alias, i));
                        if (sel == "Y")
                        {
                            var oProcessRecord = (from p in dbHrPayroll.TrnsEmployeePerPieceProcessing where p.InternalID.ToString() == id select p).FirstOrDefault();
                            if (oProcessRecord == null) continue;
                            oProcessRecord.JENum = jeNum;
                            oProcessRecord.FlgPosted = true;
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    RefreshEmployeeDisplay();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostProcessed : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostInSBO()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedJEDoc))
                {
                    oApplication.StatusBar.SetText("Select a JE draft to Post.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                int confirm = oApplication.MessageBox("Are you sure you want to post draft? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3)
                {
                    return;
                }
                
                TrnsJE oDoc = (from p in dbHrPayroll.TrnsJE where p.ID.ToString() == SelectedJEDoc select p).FirstOrDefault();
                if (oDoc == null)
                {
                    oApplication.StatusBar.SetText("Je Not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (oDoc.SBOJeNum > 0)
                {
                    oApplication.StatusBar.SetText("Already Posted in SBO", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                string strResult = Program.objHrmsUI.postJe(oDoc.ID);
                if (strResult.Contains("Error"))
                {
                    oApplication.SetStatusBarMessage(strResult);
                }
                else
                {
                    oDoc.SBOJeNum = Convert.ToInt32(strResult);
                    dbHrPayroll.SubmitChanges();
                    RefreshEmployeeDisplay();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostInSBO : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);    
            }
        }

        #endregion

    }
}
