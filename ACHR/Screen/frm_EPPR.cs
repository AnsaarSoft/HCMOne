using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    class frm_EPPR : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.EditText txtEmpFrom, txtEmpTo, txtEmployeeSelected;
        SAPbouiCOM.ComboBox cmbPayroll, cmbLocation, cmbDepartment;
        SAPbouiCOM.Button btnEmpFrom, btnEmpTo, btnSearch, btnMain;
        SAPbouiCOM.Matrix grdEmployees, grdItems;
        SAPbouiCOM.DataTable dtEmployees, dtItems;
        SAPbouiCOM.Column ceEmpID, ceEmpName, ceSelect, ceIsNew;
        SAPbouiCOM.Column ciItemCode, ciItemName, ciPerPeiceRate, ciActive, ciIsNew;
        
        SAPbouiCOM.Item ItxtEmpFrom, ItxtEmpTo, ItxtEmployeeSelected;
        SAPbouiCOM.Item IcmbPayroll, IcmbLocation, IcmbDepartment;

        Boolean flgEmpFrom, flgEmpTo, flgItemRunOneTime;
        string EmployeeCodeSelected;

        public struct ItemSet
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public decimal PerPieceRate { get; set; }
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
            if (Program.oSapItems.Count > 0 && !flgEmpTo && !flgEmpFrom && flgItemRunOneTime)
            {
                flgItemRunOneTime = false;
                FillItemsFromOtherWindow();
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)           
            {
                case "1":
                    #region Main
                    if (SaveRecords())
                    {
                        ClearRecords();
                    }
                    #endregion  
                    break;
                case "btEmpFr":
                    #region Employee From
                    flgEmpTo = false;
                    flgEmpFrom = true;
                    OpenNewSearchFormFrom();
                    #endregion
                    break;
                case "btEmpTo":
                    #region Employee To
                    flgEmpTo = true;
                    flgEmpFrom = false;
                    OpenNewSearchFormTo();
                    #endregion
                    break;
                case "btSearch":
                    #region Search Button
                    GetSelectedEmployee();
                    #endregion
                    break;
                case "btItem":
                    #region Item Button
                    flgItemRunOneTime = true;
                    GetItemsWindow();
                    #endregion
                    break;
                case "mtEmp":
                    #region Employee Grid
                    if (pVal.Row >= 1 && pVal.Row <= grdEmployees.RowCount)
                    {
                        try
                        {
                            string empname = Convert.ToString(dtEmployees.GetValue(ceEmpName.DataBind.Alias, pVal.Row - 1));
                            string empcode = Convert.ToString(dtEmployees.GetValue(ceEmpID.DataBind.Alias, pVal.Row - 1));
                            txtEmployeeSelected.Value = empname;
                            EmployeeCodeSelected = empcode;
                            FillSelectedEmployeeItem(empcode);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        txtEmployeeSelected.Value = "";
                        FillSelectedEmployeeItem("");
                    }
                    if (pVal.ColUID == "clselect" && pVal.Row == 0)
                    {
                        SelectAllEmp();
                    }
                    #endregion
                    break;
                case "mtItem":
                    #region Item Grid
                    if (pVal.Row >= 1 && pVal.Row <= grdItems.RowCount)
                    {
                    }
                    else
                    {
                    }
                    if (pVal.ColUID == "clActive" && pVal.Row == 0)
                    {
                        SelectAllItems();
                    }
                    #endregion
                    break;
                case "btClear":
                    #region Clear Button
                    ClearRecords();
                    #endregion
                    break;
                case "btDelete":
                    #region Delete Button
                    DeleteRecord();
                    #endregion
                    break;
            }
        }
        
        #endregion

        #region Function
        
        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            try
            {
                oForm.DataSources.UserDataSources.Add("txEmpFrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpFrom = oForm.Items.Item("txEmpFrom").Specific;
                ItxtEmpFrom = oForm.Items.Item("txEmpFrom");
                txtEmpFrom.DataBind.SetBound(true, "", "txEmpFrom");

                oForm.DataSources.UserDataSources.Add("txEmpTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpTo = oForm.Items.Item("txEmpTo").Specific;
                ItxtEmpTo = oForm.Items.Item("txEmpTo");
                txtEmpTo.DataBind.SetBound(true, "", "txEmpTo");

                oForm.DataSources.UserDataSources.Add("txemp", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150); // Hours Per Day
                txtEmployeeSelected = oForm.Items.Item("txemp").Specific;
                ItxtEmployeeSelected = oForm.Items.Item("txemp");
                txtEmployeeSelected.DataBind.SetBound(true, "", "txemp");

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

                grdEmployees = oForm.Items.Item("mtEmp").Specific;
                dtEmployees = oForm.DataSources.DataTables.Item("dtEmployee");
                ceEmpID = grdEmployees.Columns.Item("clempid");
                ceEmpID.TitleObject.Sortable = false;
                ceEmpName = grdEmployees.Columns.Item("clEname");
                ceEmpName.TitleObject.Sortable = false;
                ceSelect = grdEmployees.Columns.Item("clselect");
                ceSelect.TitleObject.Sortable = false;

                grdItems = oForm.Items.Item("mtItem").Specific;
                dtItems = oForm.DataSources.DataTables.Item("dtItems");
                ciItemCode = grdItems.Columns.Item("clIcode");
                ciItemName = grdItems.Columns.Item("clIname");
                ciPerPeiceRate = grdItems.Columns.Item("clPrice");
                ciActive = grdItems.Columns.Item("clActive");

                btnEmpFrom = oForm.Items.Item("btEmpFr").Specific;
                btnEmpTo = oForm.Items.Item("btEmpTo").Specific;
                btnSearch = oForm.Items.Item("btSearch").Specific;
                btnMain = oForm.Items.Item("1").Specific;

                fillCbs();
            }
            catch(Exception ex)
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
                Program.sqlString = "empMasterPP";
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
                Program.sqlString = "empMasterPP";
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

        private void GetSelectedEmployee()
        {
            try
            {
                string FromEmp, ToEmp, Department, Location, Payroll;
                FromEmp = txtEmpFrom.Value.Trim();
                ToEmp = txtEmpTo.Value.Trim();
                Department = cmbDepartment.Value.Trim();
                Location = cmbLocation.Value.Trim();
                Payroll = cmbPayroll.Value.Trim();
                
                var oCollection = (from a in dbHrPayroll.MstEmployee where a.FlgActive == true && a.FlgPerPiece == true select a).ToList();

                if (!string.IsNullOrEmpty(FromEmp) && !string.IsNullOrEmpty(ToEmp))
                {
                    Int32? intFromEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == FromEmp select a.SortOrder).FirstOrDefault();
                    Int32? intToEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == ToEmp select a.SortOrder).FirstOrDefault();
                    if (intFromEmp == null) intFromEmp = 0;
                    if (intToEmp == null) intToEmp = 0;
                    oCollection = oCollection.Where(a => a.SortOrder >= intFromEmp && a.SortOrder <= intToEmp).ToList();
                }
                if (!string.IsNullOrEmpty(Department) && Department != "0")
                {
                    oCollection = oCollection.Where(a => a.DepartmentID == Convert.ToInt32(Department)).ToList();
                }
                if (!string.IsNullOrEmpty(Location) && Location != "0")
                {
                    oCollection = oCollection.Where(a => a.Location == Convert.ToInt32(Location)).ToList();
                }
                if (!string.IsNullOrEmpty(Payroll) && Payroll != "0")
                {
                    oCollection = oCollection.Where(a => a.PayrollID == Convert.ToInt32(Payroll)).ToList();
                }
                if (oCollection != null && oCollection.Count > 0)
                {
                    dtEmployees.Rows.Clear();
                    dtEmployees.Rows.Add(oCollection.Count);
                    Int32 i = 0;
                    foreach (var One in oCollection)
                    {
                        dtEmployees.SetValue(ceEmpID.DataBind.Alias,i, One.EmpID);
                        dtEmployees.SetValue(ceEmpName.DataBind.Alias,i, One.FirstName + " " + One.LastName);
                        dtEmployees.SetValue(ceSelect.DataBind.Alias,i, "Y");
                        i++;
                    }
                    grdEmployees.LoadFromDataSource();
                }
                else
                {
                    dtEmployees.Rows.Clear();
                    grdEmployees.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetSelectedEmployee Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetItemsWindow()
        {
            try
            {
                
                string comName = "ItemsSearch";
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
                oApplication.StatusBar.SetText("GetItemsWindow : " + ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillItemsFromOtherWindow()
        {
            try
            {
                int ListCount = Program.oSapItems.Count;
                int i = 0;
                grdItems.FlushToDataSource();
                dtItems.Rows.Add(ListCount -1);
                foreach(var One in Program.oSapItems)
                {
                    dtItems.SetValue(ciItemCode.DataBind.Alias, i, One.ItemCode);
                    dtItems.SetValue(ciItemName.DataBind.Alias, i, One.ItemName);
                    dtItems.SetValue(ciPerPeiceRate.DataBind.Alias, i, "0");
                    dtItems.SetValue(ciActive.DataBind.Alias, i, "Y");
                    //dtItems.SetValue(ciIsNew.DataBind.Alias, i, "Y");
                    i++;
                }
                grdItems.LoadFromDataSource();
                btnMain.Caption = "Save";
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillItemsFromOtherWindow : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    grdEmployees.FlushToDataSource();
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {
                        string empid = dtEmployees.GetValue(ceEmpID.DataBind.Alias, i);
                        string selected = dtEmployees.GetValue(ceSelect.DataBind.Alias, i);
                        if (!string.IsNullOrEmpty(empid) && selected=="Y")
                        {
                            lstEmpID.Add(empid);
                        }
                    }
                    grdItems.FlushToDataSource();
                    for (int i = 0; i < dtItems.Rows.Count; i++)
                    {
                        string itemcode, itemname, perpiecerate, selected;
                        itemcode = dtItems.GetValue(ciItemCode.DataBind.Alias, i);
                        itemname = dtItems.GetValue(ciItemName.DataBind.Alias, i);
                        perpiecerate = Convert.ToString(dtItems.GetValue(ciPerPeiceRate.DataBind.Alias, i));
                        selected = dtItems.GetValue(ciActive.DataBind.Alias, i);
                        if (!string.IsNullOrEmpty(itemcode) && selected == "Y")
                        {
                            var InList = new ItemSet();
                            InList.ItemCode = itemcode;
                            InList.ItemName = itemname;
                            InList.PerPieceRate = Convert.ToDecimal(perpiecerate);
                            lstItems.Add(InList);
                        }
                    }
                    if (lstEmpID.Count > 0)
                    {
                        if (lstItems.Count > 0)
                        {
                            foreach(var one in lstEmpID)
                            {
                                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == one select a).FirstOrDefault();
                                if (oEmp == null) continue;
                                int Count = 0;
                                Count = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.EmpID == oEmp.ID select a).Count();
                                if (Count == 0)
                                {
                                    TrnsEmployeePerPieceRate oDoc = new TrnsEmployeePerPieceRate();
                                    dbHrPayroll.TrnsEmployeePerPieceRate.InsertOnSubmit(oDoc);
                                    oDoc.MstEmployee = oEmp;
                                    oDoc.CreateDt = DateTime.Now;
                                    oDoc.UpdateDt = DateTime.Now;
                                    oDoc.CreatedBy = oCompany.UserName;
                                    oDoc.UpdatedBy = oCompany.UserName;
                                    foreach (var oneitem in lstItems)
                                    {
                                        TrnsEmployeePerPieceRateDetail oDetail = new TrnsEmployeePerPieceRateDetail();
                                        oDetail.ItemCode = oneitem.ItemCode;
                                        oDetail.ItemName = oneitem.ItemName;
                                        oDetail.Rate = oneitem.PerPieceRate;
                                        oDetail.FlgActive = true;
                                        oDetail.FlgDelete = false;
                                        oDetail.CreatedBy = oCompany.UserName;
                                        oDetail.UpdatedBy = oCompany.UserName;
                                        oDetail.CreatedDt = DateTime.Now;
                                        oDetail.UpdatedDt = DateTime.Now;
                                        oDoc.TrnsEmployeePerPieceRateDetail.Add(oDetail);
                                    }
                                    dbHrPayroll.SubmitChanges();
                                    oApplication.StatusBar.SetText("Added Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                }
                                else
                                {
                                    TrnsEmployeePerPieceRate oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.EmpID == oEmp.ID select a).FirstOrDefault();
                                    if (oDoc == null) continue;
                                    oDoc.UpdateDt = DateTime.Now;
                                    oDoc.UpdatedBy = oCompany.UserName;
                                    foreach (var oneitem in lstItems)
                                    {
                                        int linecount = 0;
                                        linecount = (from a in dbHrPayroll.TrnsEmployeePerPieceRateDetail where a.ItemCode == oneitem.ItemCode && a.FKID == oDoc.InternalID && a.FlgActive == true select a).Count();
                                        if (linecount == 0)
                                        {
                                            TrnsEmployeePerPieceRateDetail oDetail = new TrnsEmployeePerPieceRateDetail();
                                            oDetail.ItemCode = oneitem.ItemCode;
                                            oDetail.ItemName = oneitem.ItemName;
                                            oDetail.Rate = oneitem.PerPieceRate;
                                            oDetail.FlgActive = true;
                                            oDetail.FlgDelete = false;
                                            oDetail.CreatedBy = oCompany.UserName;
                                            oDetail.UpdatedBy = oCompany.UserName;
                                            oDetail.CreatedDt = DateTime.Now;
                                            oDetail.UpdatedDt = DateTime.Now;
                                            oDoc.TrnsEmployeePerPieceRateDetail.Add(oDetail);
                                        }
                                        else
                                        {
                                            TrnsEmployeePerPieceRateDetail oDetail = (from a in dbHrPayroll.TrnsEmployeePerPieceRateDetail where a.ItemCode == oneitem.ItemCode && a.FKID == oDoc.InternalID && a.FlgActive == true select a).FirstOrDefault();
                                            oDetail.ItemName = oneitem.ItemName;
                                            oDetail.Rate = oneitem.PerPieceRate;
                                            oDetail.FlgActive = true;
                                            oDetail.FlgDelete = false;
                                            oDetail.UpdatedBy = oCompany.UserName;
                                            oDetail.UpdatedDt = DateTime.Now;
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

        private void ClearRecords()
        {
            try
            {
                txtEmpFrom.Value = "";
                txtEmpTo.Value = "";
                cmbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtEmployees.Rows.Clear();
                grdEmployees.LoadFromDataSource();
                dtItems.Rows.Clear();
                grdItems.LoadFromDataSource();
                btnMain.Caption = "Ok";
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ClearRecords : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void DeleteRecord()
        {
            try
            {
                List<string> ItemListforDelete = new List<string>();
                grdItems.FlushToDataSource();
                for (int i = 0; i < dtItems.Rows.Count; i++)
                {
                    string itemcode, itemstatus;
                    itemcode = dtItems.GetValue(ciItemCode.DataBind.Alias, i);
                    itemstatus = dtItems.GetValue(ciActive.DataBind.Alias, i);
                    if (itemstatus == "Y")
                    {
                        //make sure it sure pop up
                        ItemListforDelete.Add(itemcode);
                    }
                    
                }
                if (ItemListforDelete.Count > 0)
                {
                    
                    TrnsEmployeePerPieceRate oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.MstEmployee.EmpID == EmployeeCodeSelected select a).FirstOrDefault();
                    if (oDoc == null) return;
                    foreach (var One in oDoc.TrnsEmployeePerPieceRateDetail)
                    {
                        foreach (var OneInList in ItemListforDelete)
                        {
                            if (OneInList == One.ItemCode && One.FlgActive == true)
                            {
                                One.FlgActive = false;
                                One.FlgDelete = true;
                            }
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Successfully delete item.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    FillSelectedEmployeeItem(EmployeeCodeSelected);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("DeleteRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillSelectedEmployeeItem(string pempid)
        {
            try
            {
                if (!string.IsNullOrEmpty(pempid))
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == pempid select a).FirstOrDefault();
                    if (oEmp == null) return;
                    TrnsEmployeePerPieceRate oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.EmpID == oEmp.ID select a).FirstOrDefault();
                    if (oDoc == null) return;
                    dtItems.Rows.Clear();
                    int i = 0;
                    foreach (var OneItem in oDoc.TrnsEmployeePerPieceRateDetail)
                    {
                        if (OneItem.FlgActive == true)
                        {
                            dtItems.Rows.Add(1);
                            dtItems.SetValue(ciItemCode.DataBind.Alias, i, OneItem.ItemCode);
                            dtItems.SetValue(ciItemName.DataBind.Alias, i, OneItem.ItemName);
                            dtItems.SetValue(ciPerPeiceRate.DataBind.Alias, i, Convert.ToString(OneItem.Rate));
                            dtItems.SetValue(ciActive.DataBind.Alias, i, "Y");
                            i++;
                        }
                    }
                    grdItems.LoadFromDataSource();
                    btnMain.Caption = "Save";
                    oApplication.StatusBar.SetText("All Items Loaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    dtItems.Rows.Clear();
                    grdItems.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillSelectedEmployeeItem : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SelectAllEmp()
        {
            try
            {

                oForm.Freeze(true);
                SAPbouiCOM.Column col = grdEmployees.Columns.Item("clselect");

                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {

                        dtEmployees.SetValue("select", i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmployees.Rows.Count; i++)
                    {
                        dtEmployees.SetValue("select", i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                grdEmployees.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SelectAllEmp : " +ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SelectAllItems()
        {
            try
            {
                oForm.Freeze(true);
                SAPbouiCOM.Column col = grdItems.Columns.Item("clActive");
                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtItems.Rows.Count; i++)
                    {

                        dtItems.SetValue("select", i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtItems.Rows.Count; i++)
                    {
                        dtItems.SetValue("select", i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                grdItems.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SelectAllEmp : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
      
        #endregion

    }

}
