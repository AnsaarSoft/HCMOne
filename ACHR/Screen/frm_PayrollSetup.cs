using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using SAPbouiCOM;

namespace ACHR.Screen
{

    class frm_PayrollSetup : HRMSBaseForm
    {

        #region Variable

        public IEnumerable<CfgPayrollDefination> Payroll;

        SAPbouiCOM.EditText txCode, txName, txFEndDate, txCost, txWD, txWH, txPayrolWiseSortOrder;
        SAPbouiCOM.CheckBox chGratuity, chCheck, chCash, chBT, chYear, chUpdate, flgDflt, chkOT;
        SAPbouiCOM.ComboBox cbType, cbGrt, cbGLType, cmbOT;
        SAPbouiCOM.Matrix mtElement, mtPeriod, grdShiftSlabs;
        SAPbouiCOM.Column ElemId, ElemIsNew, Element, PrClass, EffDate, EndDate, PerIsNew, Period, StartDate, PerEndDate, PerCntr, PerVisible;
        Column clsSerial, clsId, clsPick, clsShiftCode, clsShiftname, clsDefault, clsActive, clsPriority;
        SAPbouiCOM.Button cmdPrev, cmdNext, cmdNew, btVarStdEl;

        SAPbouiCOM.Item ItxCode, ItxName, ItxFEndDate, ItxCost, ItxPayrolWiseSortOrder;
        SAPbouiCOM.Item IchGratuity, IchCheck, IchCash, IchBT, ItxWD, ItxWH, IchYear, IcbGLType, IchUpdate, IflgDflt, IchkOT, IcmbOT;
        SAPbouiCOM.Item IcbType, IcbGrt;
        SAPbouiCOM.Item ImtElement, ImtPeriod;
        SAPbouiCOM.Item IcmdPrev, IcmdNext, IcmdNew;

        SAPbouiCOM.DataTable dtElement, dtPeriod, dtShifts;

        
        

        int loadedPayroll = 0;

        #endregion

        #region B1 Events


        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.DefButton = "1";
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
            AddNewRecord();
            AddEmptyRowElements();
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (!validateForm())
                    {
                        BubbleEvent = false;
                    }
                    break;
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {

                    case "cmdNext":
                        getNextRecord();
                        break;
                    case "cmdPrev":
                        getPreviouRecord();
                        break;
                    case "chOT":
                        enableOT();
                        break;
                    case "cmdNew":
                        // addNew();
                        AddEmptyRowElements();
                        break;
                    case "1":
                        doSubmit();
                        break;
                    case "btVarStdEl":
                        updateStdElements();
                        break;
                    case "btEupdate":
                        Add_AND_UpdateElements();
                        //updateElementsAttachedElement();
                        break;
                    case "mtElement":
                        if (pVal.ColUID == "pick")
                        {
                            if (pVal.Row <= dtElement.Rows.Count)
                            {
                                selectElement(pVal.Row);
                            }
                        }
                        break;
                    case "mtshifts":
                        if(pVal.ColUID == "clpick")
                        {
                            if (pVal.Row <= dtShifts.Rows.Count)
                            {
                                SelectShifts(pVal.Row);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void fillGrat()
        {
            try
            {
                var grat = dbHrPayroll.MstGratuity.ToList();
                foreach (MstGratuity gr in grat)
                {

                    cbGrt.ValidValues.Add(gr.Id.ToString(), gr.GratuityName);
                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        public override void fillFields()
        {
            base.fillFields();
            FillRecords();
        }

        private void getData()
        {
            CodeIndex.Clear();
            Payroll = from p in dbHrPayroll.CfgPayrollDefination select p;
            int i = 0;
            foreach (CfgPayrollDefination ele in Payroll)
            {
                CodeIndex.Add(ele.ID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            DocumentInitiallize();
        }

        

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txCode = oForm.Items.Item("txCode").Specific;
                ItxCode = oForm.Items.Item("txCode");
                txCode.DataBind.SetBound(true, "", "txCode");

                oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
                txName = oForm.Items.Item("txName").Specific;
                ItxName = oForm.Items.Item("txName");
                txName.DataBind.SetBound(true, "", "txName");


                oForm.DataSources.UserDataSources.Add("txFEndDate", SAPbouiCOM.BoDataType.dt_DATE); // Project Based Attandance
                txFEndDate = oForm.Items.Item("txFEndDate").Specific;
                ItxFEndDate = oForm.Items.Item("txFEndDate");
                txFEndDate.DataBind.SetBound(true, "", "txFEndDate");

                oForm.DataSources.UserDataSources.Add("txSortOrdr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Project Based Attandance
                txPayrolWiseSortOrder = oForm.Items.Item("txSortOrdr").Specific;
                ItxPayrolWiseSortOrder = oForm.Items.Item("txSortOrdr");
                txPayrolWiseSortOrder.DataBind.SetBound(true, "", "txSortOrdr");

                oForm.DataSources.UserDataSources.Add("txCost", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Project Based Attandance
                txCost = oForm.Items.Item("txCost").Specific;
                ItxCost = oForm.Items.Item("txCost");
                txCost.DataBind.SetBound(true, "", "txCost");

                oForm.DataSources.UserDataSources.Add("txWD", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Project Based Attandance
                txWD = oForm.Items.Item("txWD").Specific;
                ItxWD = oForm.Items.Item("txWD");
                txWD.DataBind.SetBound(true, "", "txWD");

                oForm.DataSources.UserDataSources.Add("txWH", SAPbouiCOM.BoDataType.dt_SUM); // Project Based Attandance
                txWH = oForm.Items.Item("txWH").Specific;
                ItxWH = oForm.Items.Item("txWH");
                txWH.DataBind.SetBound(true, "", "txWH");


                //oForm.DataSources.UserDataSources.Add("chGratuity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // PF Period
                //chGratuity = oForm.Items.Item("chGratuity").Specific;
                //IchGratuity = oForm.Items.Item("chGratuity");
                //chGratuity.DataBind.SetBound(true, "", "chGratuity");

                oForm.DataSources.UserDataSources.Add("flgDflt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // PF Period
                flgDflt = oForm.Items.Item("flgDflt").Specific;
                IflgDflt = oForm.Items.Item("flgDflt");
                flgDflt.DataBind.SetBound(true, "", "flgDflt");

                oForm.DataSources.UserDataSources.Add("chCheck", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); //Enable Social Security
                chCheck = oForm.Items.Item("chCheck").Specific;
                IchCheck = oForm.Items.Item("chCheck");
                chCheck.DataBind.SetBound(true, "", "chCheck");

                oForm.DataSources.UserDataSources.Add("chCash", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chCash = oForm.Items.Item("chCash").Specific;
                IchCash = oForm.Items.Item("chCash");
                chCash.DataBind.SetBound(true, "", "chCash");

                oForm.DataSources.UserDataSources.Add("chBT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chBT = oForm.Items.Item("chBT").Specific;
                IchBT = oForm.Items.Item("chBT");
                chBT.DataBind.SetBound(true, "", "chBT");

                oForm.DataSources.UserDataSources.Add("chYear", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chYear = oForm.Items.Item("chYear").Specific;
                IchYear = oForm.Items.Item("chYear");
                chYear.DataBind.SetBound(true, "", "chYear");

                oForm.DataSources.UserDataSources.Add("chUpdate", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chUpdate = oForm.Items.Item("chUpdate").Specific;
                IchUpdate = oForm.Items.Item("chUpdate");
                chUpdate.DataBind.SetBound(true, "", "chUpdate");

                oForm.DataSources.UserDataSources.Add("chOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkOT = oForm.Items.Item("chOT").Specific;
                IchkOT = oForm.Items.Item("chOT");
                chkOT.DataBind.SetBound(true, "", "chOT");

                oForm.DataSources.UserDataSources.Add("cbType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cbType = oForm.Items.Item("cbType").Specific;
                IcbType = oForm.Items.Item("cbType");
                cbType.DataBind.SetBound(true, "", "cbType");

                //oForm.DataSources.UserDataSources.Add("cbGrt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                //cbGrt = oForm.Items.Item("cbGrt").Specific;
                //IcbGrt = oForm.Items.Item("cbGrt");
                //cbGrt.DataBind.SetBound(true, "", "cbGrt");

                oForm.DataSources.UserDataSources.Add("cbOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cmbOT = oForm.Items.Item("cbOT").Specific;
                IcmbOT = oForm.Items.Item("cbOT");
                cmbOT.DataBind.SetBound(true, "", "cbOT");

                cmdPrev = oForm.Items.Item("cmdPrev").Specific;
                IcmdPrev = oForm.Items.Item("cmdPrev");

                cmdNext = oForm.Items.Item("cmdNext").Specific;
                IcmdNext = oForm.Items.Item("cmdNext");

                cmdNew = oForm.Items.Item("cmdNew").Specific;
                IcmdNew = oForm.Items.Item("cmdNew");

                cbGLType = oForm.Items.Item("cbGLType").Specific;
                oForm.DataSources.UserDataSources.Add("cbGLType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                IcbGLType = oForm.Items.Item("cbGLType");
                cbGLType.DataBind.SetBound(true, "", "cbGLType");

                mtElement = oForm.Items.Item("mtElement").Specific;
                ElemIsNew = mtElement.Columns.Item("isNew");
                ElemId = mtElement.Columns.Item("ElemId");
                ElemIsNew.Visible = false;
                ElemId.Visible = false;
                Element = mtElement.Columns.Item("Element");
                PrClass = mtElement.Columns.Item("PrClass");
                EffDate = mtElement.Columns.Item("EffDate");
                EndDate = mtElement.Columns.Item("EndDate");
                dtElement = oForm.DataSources.DataTables.Item("dtElement");
                dtElement.Rows.Clear();

                mtPeriod = oForm.Items.Item("mtPeriod").Specific;
                PerIsNew = mtPeriod.Columns.Item("PerIsNew");
                PerIsNew.Visible = false;
                Period = mtPeriod.Columns.Item("Period");
                StartDate = mtPeriod.Columns.Item("StartDate");
                PerEndDate = mtPeriod.Columns.Item("PerEndDate");
                PerCntr = mtPeriod.Columns.Item("flgLocked");
                PerVisible = mtPeriod.Columns.Item("flgVisible");
                dtPeriod = oForm.DataSources.DataTables.Item("dtPeriod");

                grdShiftSlabs = oForm.Items.Item("mtshifts").Specific;
                dtShifts = oForm.DataSources.DataTables.Item("dtShifts");
                clsSerial = grdShiftSlabs.Columns.Item("clser");
                clsId = grdShiftSlabs.Columns.Item("clid");
                clsPick = grdShiftSlabs.Columns.Item("clpick");
                clsShiftCode = grdShiftSlabs.Columns.Item("clcode");
                clsShiftname = grdShiftSlabs.Columns.Item("clname");
                clsDefault = grdShiftSlabs.Columns.Item("cldefault");
                clsPriority = grdShiftSlabs.Columns.Item("clprty");
                clsActive = grdShiftSlabs.Columns.Item("clactive");
                
                IcmbOT.Enabled = false;
                fillCombo("Prd_Type", cbType);
                fillCombo("GLType", cbGLType);
                FillComboOverTime();
                //cbGLType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //fillGrat();
                getData();
                oForm.PaneLevel = 2;
            }
            catch (Exception ex)
            {
                logger(ex);
            }
            
        }

        private void DocumentInitiallize()
        {
            try
            {
                loadedPayroll = 0;
                ItxCode.Visible = false;
                ItxName.Enabled = true;
                txCode.Value = "0";
                txFEndDate.Value = "";
                txName.Value = "";
                chBT.Checked = false;
                chCash.Checked = false;
                chCheck.Checked = false;
                txName.Active = true;
                txCost.Value = "";
                txCost.Active = true;
                txPayrolWiseSortOrder.Value = "0";
                txPayrolWiseSortOrder.Active = true;
                txWD.Value = "0";
                txWH.Value = "0";
                chYear.Checked = false;
                dtElement.Rows.Clear();
                dtPeriod.Rows.Clear();
                dtShifts.Rows.Clear();
                txName.Active = true;

                AddEmptyRowElements();
                AddEmptyRowShifts();
                oForm.PaneLevel = 2;
                oForm.Mode = BoFormMode.fm_ADD_MODE;
            }
            catch(Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboOverTime()
        {
            try
            {
                var Data = dbHrPayroll.MstOverTime.Where(o => o.FlgActive == true).ToList();
                cmbOT.ValidValues.Add("-1", "Select one...");
                if (Data != null && Data.Count > 0)
                {
                    foreach (MstOverTime Overtime in Data)
                    {
                        cmbOT.ValidValues.Add(Convert.ToString(Overtime.ID), Convert.ToString(Overtime.Description));
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillOvertimeType Exception Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void enableGr()
        {
            if (chGratuity.Checked)
            {
                IcbGrt.Enabled = true;
            }
            else
            {
                txFEndDate.Active = true;
                IcbGrt.Enabled = false;
            }
        }

        private void enableOT()
        {
            if (chkOT.Checked)
            {
                IcmbOT.Enabled = true;
            }
            else
            {
                IcmbOT.Enabled = false;
            }
        }

        private void setElementInfo(string ele, int rowNum)
        {
            try
            {
                int cnt = (from p in dbHrPayroll.MstElements where p.Id.ToString() == ele select p).Count();
                if (cnt > 0)
                {
                    MstElements element = (from p in dbHrPayroll.MstElements where p.Id.ToString() == ele select p).FirstOrDefault();
                    //Check Database 
                    int oLink = (from a in dbHrPayroll.MstElementLink where a.PayrollID == loadedPayroll && a.ElementID == element.Id select a).Count();
                    if (oLink > 0)
                    {
                        oApplication.StatusBar.SetText("Element already attach with payroll, Multiple attachments not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    //Check Grid before add.
                    string ElementID = string.Empty;
                    for (int i = 0; i < mtElement.RowCount; i++)
                    {
                        ElementID = (mtElement.Columns.Item("ElemId").Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value;
                        if (!string.IsNullOrEmpty(ElementID))
                        {
                            if (ElementID == element.Id.ToString())
                            {
                                oApplication.StatusBar.SetText("Element already attach with payroll, Multiple attachments not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                        }
                    }
                    dtElement.SetValue("ElemId", rowNum, element.Id);
                    dtElement.SetValue("EffDate", rowNum, element.StartDate);
                    dtElement.SetValue("Element", rowNum, element.Description);
                    dtElement.SetValue("PrClass", rowNum, element.ElmtType);
                    if (element.EndDate != null)
                    {
                        dtElement.SetValue("EndDate", rowNum, element.EndDate);
                    }
                    mtElement.SetLineData(rowNum + 1);
                    AddEmptyRowElements();

                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void SetShiftInfo(string shiftcode, int rowNum)
        {
            try
            {
                
                int cnt = (from a in dbHrPayroll.MstShifts where a.Code == shiftcode select a).Count();
                if (cnt > 0)
                {
                    MstShifts oShift = (from a in dbHrPayroll.MstShifts where a.Code == shiftcode select a).FirstOrDefault();
                    //Check Database 
                    int oLink = (from a in dbHrPayroll.CfgPayrollShifts where a.PayrollId == loadedPayroll && a.ShiftId == oShift.Id select a).Count();
                    if (oLink > 0)
                    {
                        MsgWarning("Shift already attach with payroll, Multiple attachments not allowed.");
                        return;
                    }
                    //Check Grid before add.
                    string ShiftCode = string.Empty;
                    for (int i = 0; i < grdShiftSlabs.RowCount; i++)
                    {
                        ShiftCode = (grdShiftSlabs.Columns.Item(clsShiftCode.UniqueID).Cells.Item(i + 1).Specific as SAPbouiCOM.EditText).Value;
                        if (!string.IsNullOrEmpty(ShiftCode))
                        {
                            if (ShiftCode == oShift.Code)
                            {
                                MsgWarning("Shift already attach with payroll, Multiple attachements are not allowed.");
                                return;
                            }
                        }
                    }
                    grdShiftSlabs.FlushToDataSource();
                    dtShifts.Rows.Add(1);
                    dtShifts.SetValue(clsId.DataBind.Alias, rowNum, 0);
                    dtShifts.SetValue(clsShiftCode.DataBind.Alias, rowNum, oShift.Code);
                    dtShifts.SetValue(clsShiftname.DataBind.Alias, rowNum, oShift.Description);
                    dtShifts.SetValue(clsDefault.DataBind.Alias, rowNum, "N");
                    dtShifts.SetValue(clsPriority.DataBind.Alias, rowNum, 1);
                    dtShifts.SetValue(clsActive.DataBind.Alias, rowNum, "Y");
                    //grdShiftSlabs.SetLineData(rowNum + 1);
                    AddEmptyRowShifts();

                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("PayrollName", txName.Value.Trim());
            SearchKeyVal.Add("pr.PayrollType", cbType.Value.Trim());
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            DocumentInitiallize();
            ItxName.Enabled = true;
            txName.Active = true;
        }

        private void selectElement(int rownum)
        {
            try
            {
                SearchKeyVal.Clear();
                string strSql = sqlString.getSql("elementSetup", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Element", "Select  Element");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    currentObjId = st.Rows[0][0].ToString();
                    setElementInfo(currentObjId, rownum - 1);
                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void SelectShifts(int rownum)
        {
            try
            {
                SearchKeyVal.Clear();
                string strSql = sqlString.getSql("PayrollShifts", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Shifts", "Select  Shifts");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    currentObjId = st.Rows[0][0].ToString();
                    SetShiftInfo(currentObjId, rownum - 1);
                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void doFind()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("PayrollSetup", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Payroll", "Select  Payroll");
            pic = null;
            if (st.Rows.Count > 0)
            {
                currentObjId = st.Rows[0][0].ToString();
                getRecord(currentObjId);
            }
        }

        private bool validateForm()
        {
            bool result = true;
            return result;
        }

        private void doSubmit()
        {
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
            {
                doFind();
            }
            else
            {
                submitForm();
            }
        }

        private bool submitForm()
        {
            //int confirm = oApplication.MessageBox("Are you sure you want to Update selected Payroll? ", 3, "Yes", "No", "Cancel");
            //if (confirm == 2 || confirm == 3)
            //{
            //    return false;
            //} 
            mtElement.FlushToDataSource();
            bool submitResult = true;
            try
            {
                CfgPayrollDefination Payroll;
                MstElementLink PayrollElements;
                CfgPeriodDates PeriodDates;
                if (txName.Value.Trim() == "")
                {
                    oApplication.SetStatusBarMessage("Give Name of payroll");
                    return false;
                }
                int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == txCode.Value.Trim() select p).Count();
                if (cnt > 0)
                {
                    Payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == txCode.Value.Trim() select p).FirstOrDefault();
                    var EmpList = Payroll.MstEmployee.ToList();
                    if (EmpList != null && EmpList.Count > 0 && Payroll.PayrollType.Trim() != cbType.Value.Trim())
                    {
                        oApplication.StatusBar.SetText("Can't update payroll type.Payroll is Attached with Employee(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    if (EmpList != null && EmpList.Count > 0 && Payroll.GLType.Trim() != cbGLType.Value.ToString().Trim())
                    {
                        oApplication.StatusBar.SetText("Can't update payroll GL Type.Payroll is Attached with Employee(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }

                }
                else
                {

                    Payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName.Trim() == txName.Value.Trim() select p).FirstOrDefault();
                    if (Payroll != null)
                    {
                        oApplication.StatusBar.SetText("payroll With Same Name Already Exist", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    if (Payroll == null)
                    {
                        Payroll = new CfgPayrollDefination();
                        dbHrPayroll.CfgPayrollDefination.InsertOnSubmit(Payroll);
                    }
                }
                for (int i = 0; i < dtElement.Rows.Count; i++)
                {
                    string isNew = Convert.ToString(dtElement.GetValue("isNew", i));
                    string elId = Convert.ToString(dtElement.GetValue("ElemId", i));
                    string id = Convert.ToString(dtElement.GetValue("id", i));
                    if (elId != "0")
                    {
                        MstElements mstEle = (from p in dbHrPayroll.MstElements where p.Id.ToString() == elId select p).Single();

                        if (isNew == "Y")
                        {
                            PayrollElements = new MstElementLink();
                            Payroll.MstElementLink.Add(PayrollElements);
                        }
                        else
                        {
                            PayrollElements = (from p in dbHrPayroll.MstElementLink where p.ID.ToString() == id select p).Single();
                        }
                        PayrollElements.MstElements = mstEle;
                        PayrollElements.FlgActive = Convert.ToString(dtElement.GetValue("Active", i)) == "Y" ? true : false;

                    }
                }

                CfgPeriodDates period;
                mtPeriod.FlushToDataSource();
                for (int i = 0; i < dtPeriod.Rows.Count; i++)
                {
                    string periodid = Convert.ToString(dtPeriod.GetValue("id", i));
                    period = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == periodid select p).FirstOrDefault();
                    if (period != null)
                    {
                        period.FlgLocked = Convert.ToString(dtPeriod.GetValue("flgLocked", i)) == "Y" ? true : false;
                        period.FlgVisible = Convert.ToString(dtPeriod.GetValue("flgVisible", i)) == "Y" ? true : false;
                    }
                }

                grdShiftSlabs.FlushToDataSource();
                for(int i = 0; i < dtShifts.Rows.Count; i++)
                {
                    int lineid = 0, priorityvalue =0;
                    string defaultvalue, activevalue, shiftcode;
                    lineid = Convert.ToInt32(dtShifts.GetValue(clsId.DataBind.Alias, i));
                    shiftcode = Convert.ToString(dtShifts.GetValue(clsShiftCode.DataBind.Alias, i));
                    defaultvalue = Convert.ToString(dtShifts.GetValue(clsDefault.DataBind.Alias, i));
                    activevalue = Convert.ToString(dtShifts.GetValue(clsActive.DataBind.Alias, i));
                    priorityvalue = Convert.ToInt32(dtShifts.GetValue(clsPriority.DataBind.Alias, i));

                    if (string.IsNullOrEmpty(shiftcode)) continue;
                    if (lineid != 0)
                    {
                        var LineValue = (from a in dbHrPayroll.CfgPayrollShifts
                                         where a.Id == lineid
                                         select a).FirstOrDefault();
                        if (LineValue != null)
                        {

                            LineValue.Priority = priorityvalue;
                            if (defaultvalue.Trim().ToUpper() == "Y")
                            {
                                LineValue.FlgDefault = true;
                            }
                            else
                            {
                                LineValue.FlgDefault = false;
                            }
                            if (activevalue.Trim().ToUpper() == "Y")
                            {
                                LineValue.FlgActive = true;
                            }
                            else
                            {
                                LineValue.FlgActive = false;
                            }
                        }
                    }
                    else
                    {
                        var oShiftMaster = (from a in dbHrPayroll.MstShifts
                                            where a.Code == shiftcode
                                            select a).FirstOrDefault();
                        CfgPayrollShifts oNew = new CfgPayrollShifts();
                        oNew.CfgPayrollDefination = Payroll;
                        oNew.ShiftId = oShiftMaster.Id;
                        oNew.Priority = priorityvalue;
                        if (defaultvalue.Trim().ToUpper() == "Y")
                        {
                            oNew.FlgDefault = true;
                        }
                        else
                        {
                            oNew.FlgDefault = false;
                        }
                        if (activevalue.Trim().ToUpper() == "Y")
                        {
                            oNew.FlgActive = true;
                        }
                        else
                        {
                            oNew.FlgActive = false;
                        }
                        oNew.CreatedBy = oCompany.UserName;
                        oNew.UpdatedBy = oCompany.UserName;
                        oNew.CreateDate = DateTime.Now;
                        oNew.UpdateDate = DateTime.Now;
                        Payroll.CfgPayrollShifts.Add(oNew);
                    }
                }

                Payroll.CreateDate = DateTime.Now;
                Payroll.UserId = oCompany.UserName;
                Payroll.GLType = cbGLType.Value.ToString();
                Payroll.PayrollName = txName.Value;
                Payroll.CostCenter = txCost.Value;
                if (!string.IsNullOrEmpty(txPayrolWiseSortOrder.Value))
                {
                    Payroll.PayrollWiseSortOrder = Convert.ToInt32(txPayrolWiseSortOrder.Value);
                }
                Payroll.WorkDays = Convert.ToInt16(txWD.Value);
                Payroll.WorkHours = Convert.ToDecimal(txWH.Value);
                Payroll.FirstPeriodEndDt = DateTime.ParseExact(txFEndDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                Payroll.PayrollType = cbType.Value;
                Payroll.FlgIsDefault = flgDflt.Checked;
                Payroll.PayrollTypeLOVType = "Prd_Type";
                //Payroll.FlgGratuity = chGratuity.Checked;
                Payroll.FlgGratuity = false;
                Payroll.FlgPMBankTransfer = chBT.Checked;
                Payroll.FlgPMCash = chCash.Checked;
                Payroll.FlgPMCheque = chCheck.Checked;
                Payroll.WorkDaysType = chYear.Checked;
                if (chkOT.Checked)
                {
                    Payroll.FlgOT = true;
                    Payroll.OTValue = Convert.ToInt32(cmbOT.Value.Trim());
                }
                else
                {
                    Payroll.FlgOT = false;
                    Payroll.OTValue = Convert.ToInt32("0");
                }

                //if (chGratuity.Checked)
                //{
                //    Payroll.GratuityID = Convert.ToInt16(cbGrt.Value.Trim());
                //}
                Payroll.GratuityID = null;
                Payroll.UpdateDate = DateTime.Now;
                Payroll.UpdatedBy = oCompany.UserName;

                dbHrPayroll.SubmitChanges();

                getData();

                //oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    AddNewRecord();
                }
                else
                {
                    FillRecords();
                }

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
                oForm.AutoManaged = false;
                submitResult = false;
                logger(ex);
            }
            return submitResult;
        }

        private void FillRecords()
        {
            oForm.Freeze(true);
            try
            {
                if (currentRecord >= 0)
                {
                    cbType.Active = true;
                    ItxName.Enabled = false;
                    CfgPayrollDefination record;
                    MstElementLink lPayrollElements;
                    CfgPeriodDates lPeriodDates;


                    record = Payroll.ElementAt<CfgPayrollDefination>(currentRecord);
                    loadedPayroll = record.ID;
                    oForm.DataSources.UserDataSources.Item("txCode").ValueEx = record.ID.ToString();
                    txCode.Value = record.ID.ToString();
                    oForm.DataSources.UserDataSources.Item("txName").ValueEx = record.PayrollName;
                    oForm.DataSources.UserDataSources.Item("txCost").ValueEx = record.CostCenter;
                    oForm.DataSources.UserDataSources.Item("txFEndDate").ValueEx = Convert.ToDateTime(record.FirstPeriodEndDt).ToString("yyyyMMdd");
                    oForm.DataSources.UserDataSources.Item("cbType").ValueEx = record.PayrollType;
                    oForm.DataSources.UserDataSources.Item("cbOT").ValueEx = Convert.ToString(record.OTValue);
                    oForm.DataSources.UserDataSources.Item("cbGLType").ValueEx = record.GLType;
                    if (!string.IsNullOrEmpty(record.GLType))
                    {
                        cbGLType.Select(record.GLType.Trim(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    }
                    oForm.DataSources.UserDataSources.Item("txWD").ValueEx = record.WorkDays.ToString();
                    oForm.DataSources.UserDataSources.Item("txWH").ValueEx = record.WorkHours.ToString();
                    //oAttendanceRegisterSaved.LeaveType == null ? "" : Convert.ToString(oAttendanceRegisterSaved.LeaveType);
                    oForm.DataSources.UserDataSources.Item("txSortOrdr").ValueEx = record.PayrollWiseSortOrder == null ? "0" : record.PayrollWiseSortOrder.ToString();



                    if (record.FlgIsDefault != null)
                    {
                        oForm.DataSources.UserDataSources.Item("flgDflt").ValueEx = (bool)record.FlgIsDefault == true ? "Y" : "N";
                    }
                    oForm.DataSources.UserDataSources.Item("chBT").ValueEx = (bool)record.FlgPMBankTransfer == true ? "Y" : "N";
                    //oForm.DataSources.UserDataSources.Item("chGratuity").ValueEx = (bool)record.FlgGratuity == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chCash").ValueEx = (bool)record.FlgPMCash == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chCheck").ValueEx = (bool)record.FlgPMCheque == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chYear").ValueEx = (bool)record.WorkDaysType == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chOT").ValueEx = Convert.ToBoolean(record.FlgOT) == true ? "Y" : "N";

                    dtElement.Rows.Clear();
                    int i = 0;
                    foreach (MstElementLink pe in record.MstElementLink)
                    {
                        dtElement.Rows.Add(1);
                        dtElement.SetValue("isNew", dtElement.Rows.Count - 1, "N");
                        dtElement.SetValue("id", dtElement.Rows.Count - 1, pe.ID.ToString());
                        dtElement.SetValue("ElemId", dtElement.Rows.Count - 1, pe.ElementID.ToString());
                        dtElement.SetValue("Element", dtElement.Rows.Count - 1, pe.MstElements.ElementName);
                        dtElement.SetValue("PrClass", dtElement.Rows.Count - 1, pe.MstElements.ElmtType);
                        dtElement.SetValue("EffDate", dtElement.Rows.Count - 1, pe.MstElements.StartDate);
                        dtElement.SetValue("pick", dtElement.Rows.Count - 1, strCfl);
                        dtElement.SetValue("eValue", dtElement.Rows.Count - 1, "0");
                        dtElement.SetValue("eUpdate", dtElement.Rows.Count - 1, "N");
                        dtElement.SetValue("Active", dtElement.Rows.Count - 1, pe.FlgActive == true ? "Y" : "N");


                        if (pe.MstElements.EndDate != null)
                        {
                            dtElement.SetValue("EndDate", i, pe.MstElements.EndDate);
                        }
                        i++;

                    }
                    i = 0;
                    dtPeriod.Rows.Clear();

                    foreach (CfgPeriodDates pd in record.CfgPeriodDates)
                    {
                        dtPeriod.Rows.Add(1);
                        dtPeriod.SetValue("id", dtPeriod.Rows.Count - 1, pd.ID.ToString());
                        dtPeriod.SetValue("Period", dtPeriod.Rows.Count - 1, pd.PeriodName.ToString());
                        dtPeriod.SetValue("StartDate", dtPeriod.Rows.Count - 1, Convert.ToDateTime(pd.StartDate).ToString("yyyy-MM-dd"));
                        dtPeriod.SetValue("EndDate", dtPeriod.Rows.Count - 1, Convert.ToDateTime(pd.EndDate).ToString("yyyy-MM-dd"));
                        dtPeriod.SetValue("flgLocked", dtPeriod.Rows.Count - 1, pd.FlgLocked == true ? "Y" : "N");
                        dtPeriod.SetValue("flgVisible", dtPeriod.Rows.Count - 1, pd.FlgVisible == true ? "Y" : "N");

                        i++;
                    }

                    dtShifts.Rows.Clear();
                    i = 0;
                    foreach (var One in record.CfgPayrollShifts)
                    {
                        string shiftcode, shiftdesc;
                        shiftcode = One.MstShifts.Code;
                        shiftdesc = One.MstShifts.Description;
                        dtShifts.Rows.Add(1);
                        //dtShifts.SetValue(clsSerial.DataBind.Alias, i, i + 1);
                        dtShifts.SetValue(clsId.DataBind.Alias, i, One.Id);
                        dtShifts.SetValue(clsPick.DataBind.Alias, i, strCfl);
                        dtShifts.SetValue(clsShiftCode.DataBind.Alias, i, shiftcode);
                        dtShifts.SetValue(clsShiftname.DataBind.Alias, i, shiftdesc);
                        dtShifts.SetValue(clsPriority.DataBind.Alias, i, One.Priority);
                        if (One.FlgDefault != null)
                        {
                            if (Convert.ToBoolean(One.FlgDefault))
                            {
                                dtShifts.SetValue(clsDefault.DataBind.Alias, i, "Y");
                            }
                            else
                            {
                                dtShifts.SetValue(clsDefault.DataBind.Alias, i, "N");
                            }
                        }
                        else
                        {
                            dtShifts.SetValue(clsDefault.DataBind.Alias, i, "N");
                        }
                        if (One.FlgActive != null)
                        {
                            if (Convert.ToBoolean(One.FlgActive))
                            {
                                dtShifts.SetValue(clsActive.DataBind.Alias, i, "Y");
                            }
                            else
                            {
                                dtShifts.SetValue(clsActive.DataBind.Alias, i, "N");
                            }
                        }
                        else
                        {
                            dtShifts.SetValue(clsActive.DataBind.Alias, i, "N");
                        }
                        i++;
                    }

                    mtPeriod.LoadFromDataSource();
                    mtElement.LoadFromDataSource();
                    grdShiftSlabs.LoadFromDataSource();
                    AddEmptyRowElements();
                    AddEmptyRowShifts();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
            }
            catch (Exception ex)
            {
                oForm.Freeze(false);
                logger(ex);
            }
            // oForm.Refresh();
            oForm.Freeze(false);
        }

        private void AddEmptyRowElements()
        {
            try
            {
                // mtElement.FlushToDataSource();
                if (dtElement.Rows.Count == 0)
                {
                    dtElement.Rows.Add(1);
                    dtElement.SetValue("isNew", 0, "Y");
                    dtElement.SetValue("id", 0, "0");

                    dtElement.SetValue("ElemId", 0, 0);
                    dtElement.SetValue("pick", 0, strCfl);

                    dtElement.SetValue("Element", 0, "");
                    dtElement.SetValue("PrClass", 0, "");
                    dtElement.SetValue("EndDate", 0, "");
                    mtElement.AddRow(1, 0);
                    // mtElement.SetLineData(1);
                }
                else
                {
                    if (dtElement.GetValue("Element", dtElement.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtElement.Rows.Add(1);
                        dtElement.SetValue("isNew", dtElement.Rows.Count - 1, "Y");
                        dtElement.SetValue("id", dtElement.Rows.Count - 1, 0);
                        dtElement.SetValue("ElemId", dtElement.Rows.Count - 1, 0);
                        dtElement.SetValue("Element", dtElement.Rows.Count - 1, "");
                        dtElement.SetValue("PrClass", dtElement.Rows.Count - 1, "");
                        dtElement.SetValue("EndDate", dtElement.Rows.Count - 1, "");
                        dtElement.SetValue("pick", dtElement.Rows.Count - 1, strCfl);
                        mtElement.AddRow(1, mtElement.RowCount);


                    }

                }

                mtElement.LoadFromDataSourceEx();
                // mtElement.LoadFromDataSource();

            }
            catch (Exception ex)
            {
                string errmsg = ex.Message;

            }
        }

        private void AddEmptyRowShifts()
        {
            try
            {
                //grdShiftSlabs.FlushToDataSource();
                if (dtShifts.Rows.Count == 0)
                {
                    dtShifts.Rows.Add(1);
                    //dtShifts.SetValue(clsSerial.DataBind.Alias, 0, 0);
                    dtShifts.SetValue(clsId.DataBind.Alias, 0, 0);
                    dtShifts.SetValue(clsPick.DataBind.Alias, 0, strCfl);
                    dtShifts.SetValue(clsShiftCode.DataBind.Alias, 0, "");
                    dtShifts.SetValue(clsShiftname.DataBind.Alias, 0, "");
                    dtShifts.SetValue(clsDefault.DataBind.Alias, 0, "N");
                    dtShifts.SetValue(clsPriority.DataBind.Alias, 0, 0);
                    dtShifts.SetValue(clsActive.DataBind.Alias, 0, "Y");
                    grdShiftSlabs.AddRow(1, 0);
                }
                else
                {
                    int LineCount = dtShifts.Rows.Count - 1;
                    if (dtShifts.GetValue(clsShiftCode.DataBind.Alias, LineCount) == "")
                    {
                    }
                    else
                    {
                        dtShifts.Rows.Add(1);
                        LineCount = dtShifts.Rows.Count - 1;
                        //dtShifts.SetValue(clsSerial.DataBind.Alias, LineCount, 0);
                        dtShifts.SetValue(clsId.DataBind.Alias, LineCount, 0);
                        dtShifts.SetValue(clsPick.DataBind.Alias, LineCount, strCfl);
                        dtShifts.SetValue(clsShiftCode.DataBind.Alias, LineCount, "");
                        dtShifts.SetValue(clsShiftname.DataBind.Alias, LineCount, "");
                        dtShifts.SetValue(clsDefault.DataBind.Alias, LineCount, "Y");
                        dtShifts.SetValue(clsPriority.DataBind.Alias, LineCount, 0);
                        dtShifts.SetValue(clsActive.DataBind.Alias, LineCount, "Y");
                        grdShiftSlabs.AddRow(1, grdShiftSlabs.RowCount);
                    }
                }
                grdShiftSlabs.LoadFromDataSourceEx();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public void updateStdElements()
        {
            int confirm = oApplication.MessageBox("Are you sure you want to Update Element(s)? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3)
            {
                return;
            }
            SAPbouiCOM.ProgressBar prog = null;
            try
            {
                var emps = (from p in dbHrPayroll.MstEmployee where p.PayrollID.ToString() == txCode.Value.ToString().Trim() && p.FlgActive == true select p).ToList();
                int totalEmps = emps.Count();

                prog = oApplication.StatusBar.CreateProgressBar("Updating Employee Elements", totalEmps, false);
                prog.Value = 0;

                foreach (MstEmployee emp in emps)
                {
                    System.Windows.Forms.Application.DoEvents();
                    ds.updateStandardElements(emp, chUpdate.Checked);
                    prog.Value += 1;
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
            finally
            {
                if (prog != null)
                {
                    prog.Stop();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                }
                prog = null;
            }
        }
        //add and update elements on the basis of payroll
        public void Add_AND_UpdateElements()
        {
            int inc = 0;
            string selId = "0";
            bool flgPrevios = false;
            bool flgHit = false;
            int count = 0;
            #region CheckActivePeriod
            int Pcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == txName.Value.Trim() select p).Count();
            if (Pcnt > 0)
            {
                CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == txName.Value.Trim() select p).FirstOrDefault();
                foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                {
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
                            string pickId = selId;
                        }
                    }

                    inc++;
                }
            }
            #endregion

            #region Validation
            int confirm = oApplication.MessageBox("Are you sure you want to Update Element(s)? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3)
            {
                return;
            }
            SAPbouiCOM.ProgressBar prog = null;
            var emps = (from p in dbHrPayroll.MstEmployee where p.PayrollID.ToString() == txCode.Value.ToString().Trim() && p.FlgActive == true select p).ToList();

            if (emps.Count == 0)
            {
                oApplication.StatusBar.SetText("Payroll not attached with any employee.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return;
            }
            #endregion
            for (int i = 0; i < dtElement.Rows.Count; i++)
            {
                mtElement.FlushToDataSource();
                string update = Convert.ToString(dtElement.GetValue("eUpdate", i));
                string Active = dtElement.GetValue("Active", i);
                string ElementUpdate = dtElement.GetValue("eValue", i);
                string ElementName = dtElement.GetValue("Element", i);
                int totalEmps = emps.Count();
                if (update == "Y")
                {

                    int ID = dtElement.GetValue("ElemId", i);

                    string ElementType = dtElement.GetValue("PrClass", i);

                    string periodName = txName.Value.Trim();

                    if (ElementUpdate == "0" || ElementUpdate == "")
                    {
                        oApplication.StatusBar.SetText(ElementName + " Element update Needs Greater than Zero Value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }

                    var oElement = (from a in dbHrPayroll.MstElements where a.Id == ID select a).FirstOrDefault();

                    var prds = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == txName.Value.Trim() select p).FirstOrDefault();
                    //var linkedElement = (from a in dbHrPayroll.MstElementLink where a.ElementID == oElement.Id && a.FlgActive == true && a.PayrollID == prds.ID select a).FirstOrDefault();
                    var linkedElement = (from a in dbHrPayroll.MstElementLink where a.ElementID == oElement.Id && a.PayrollID == prds.ID select a).FirstOrDefault();
                    if (oElement != null)
                    {
                        if (linkedElement.FlgActive == false)
                        {
                            oApplication.StatusBar.SetText("Please Active " + oElement.Description + " element .", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        if (ElementUpdate == "0" && ElementUpdate == null)
                        {
                            oApplication.StatusBar.SetText(ElementName + " Element update Needs Greater than Zero Value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        #region Earning
                        if (ElementType.Trim() == "Ear")
                        {
                            decimal outValue = 0M;
                            foreach (MstEmployee emp in emps)
                            {
                                TrnsEmployeeElement empEle;

                                var oElementEar = (from a in dbHrPayroll.MstElementEarning where a.ElementID == oElement.Id select a).FirstOrDefault();
                                if (oElementEar == null)
                                {
                                    oApplication.StatusBar.SetText("Element not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                int cnt = (from p in dbHrPayroll.TrnsEmployeeElement where p.EmployeeId == emp.ID select p).Count();
                                if (cnt == 0)
                                {
                                    empEle = new TrnsEmployeeElement();
                                    empEle.CreateDate = DateTime.Now;
                                    empEle.UserId = emp.CreatedBy;
                                    empEle.EmployeeId = oElement.Id;
                                    empEle.FlgActive = true; //instance.FlgActive;
                                    dbHrPayroll.TrnsEmployeeElement.InsertOnSubmit(empEle);
                                }
                                else
                                {
                                    empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == emp.EmpID select p).FirstOrDefault();
                                }
                                empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == emp.EmpID select p).FirstOrDefault();
                                int linkedCnt = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID select p).Count();


                                if (oElement.Type == "Non-Rec")
                                {
                                    if (linkedCnt == 0)
                                    {
                                        TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();

                                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                        trntEle.ElementId = linkedElement.ElementID;
                                        trntEle.FlgRetro = false;
                                        trntEle.StartDate = oElement.StartDate;
                                        trntEle.EndDate = oElement.EndDate;
                                        trntEle.ElementType = oElement.ElmtType;
                                        trntEle.ValueType = oElementEar.ValueType;
                                        //POB,POG,FIX

                                        CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, trntEle);
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        trntEle.PeriodId = Convert.ToInt32(selId);
                                        empEle.TrnsEmployeeElementDetail.Add(trntEle);

                                    }
                                    else
                                    {
                                        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, trntEle);
                                        //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                        trntEle.PeriodId = Convert.ToInt32(selId);
                                        trntEle.UpdateDate = DateTime.Now;
                                        trntEle.UpdatedBy = oCompany.UserName;
                                    }
                                    //dbHrPayroll.SubmitChanges();
                                    //oApplication.StatusBar.SetText("Element amount updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                    //return;
                                }
                                else
                                {
                                    if (linkedCnt == 0)
                                    {
                                        TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();

                                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                        trntEle.ElementId = linkedElement.ElementID;
                                        trntEle.FlgRetro = false;
                                        trntEle.StartDate = oElement.StartDate;
                                        trntEle.EndDate = oElement.EndDate;
                                        trntEle.ElementType = oElement.ElmtType;
                                        trntEle.ValueType = oElementEar.ValueType;
                                        CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, trntEle);
                                        //trntEle.Value = oElementEar.Value;
                                        //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        empEle.TrnsEmployeeElementDetail.Add(trntEle);
                                    }
                                    else
                                    {
                                        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, trntEle);
                                        trntEle.UpdateDate = DateTime.Now;
                                        trntEle.UpdatedBy = oCompany.UserName;
                                    }

                                    //oApplication.StatusBar.SetText("Element amount updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                    //return;
                                }

                            }
                            dbHrPayroll.SubmitChanges();
                        }

                        #endregion

                        #region Deductions
                        if (ElementType.Trim() == "Ded")
                        {
                            foreach (MstEmployee empded in emps)
                            {
                                TrnsEmployeeElement empEleDed;

                                //var oElementEar = (from a in dbHrPayroll.MstElementEarning where a.ElementID == oElement.Id select a).FirstOrDefault();
                                var oElementDed = (from a in dbHrPayroll.MstElementDeduction where a.ElementID == oElement.Id select a).FirstOrDefault();
                                //var oElementCon = (from a in dbHrPayroll.MstElementContribution where a.ElementId == oElement.Id select a).FirstOrDefault();
                                if (oElementDed == null)
                                {
                                    oApplication.StatusBar.SetText("Element not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                int cntded = (from p in dbHrPayroll.TrnsEmployeeElement where p.EmployeeId == empded.ID select p).Count();
                                if (cntded == 0)
                                {
                                    empEleDed = new TrnsEmployeeElement();
                                    empEleDed.CreateDate = DateTime.Now;
                                    empEleDed.UserId = empded.CreatedBy;
                                    empEleDed.EmployeeId = oElement.Id;
                                    empEleDed.FlgActive = true; //instance.FlgActive;
                                    dbHrPayroll.TrnsEmployeeElement.InsertOnSubmit(empEleDed);
                                }
                                else
                                {
                                    empEleDed = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empded.EmpID select p).FirstOrDefault();
                                }
                                empEleDed = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empded.EmpID select p).FirstOrDefault();
                                int linkedCntded = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID select p).Count();


                                if (oElement.Type == "Non-Rec")
                                {
                                    if (linkedCntded == 0)
                                    {
                                        TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();

                                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                        trntEle.ElementId = linkedElement.ElementID;
                                        trntEle.FlgRetro = false;
                                        trntEle.StartDate = oElement.StartDate;
                                        trntEle.EndDate = oElement.EndDate;
                                        trntEle.ElementType = oElement.ElmtType;
                                        trntEle.ValueType = oElementDed.ValueType;
                                        CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, trntEle);
                                        //trntEle.Value = oElementDed.Value;
                                        //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        trntEle.PeriodId = Convert.ToInt32(selId);
                                        empEleDed.TrnsEmployeeElementDetail.Add(trntEle);

                                    }
                                    else
                                    {
                                        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, trntEle);
                                        trntEle.PeriodId = Convert.ToInt32(selId);
                                        trntEle.UpdateDate = DateTime.Now;
                                        trntEle.UpdatedBy = oCompany.UserName;
                                    }

                                }
                                else
                                {
                                    if (linkedCntded == 0)
                                    {
                                        TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();

                                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                        trntEle.ElementId = linkedElement.ElementID;
                                        trntEle.FlgRetro = false;
                                        trntEle.StartDate = oElement.StartDate;
                                        trntEle.EndDate = oElement.EndDate;
                                        trntEle.ElementType = oElement.ElmtType;
                                        trntEle.ValueType = oElementDed.ValueType;
                                        CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, trntEle);
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        empEleDed.TrnsEmployeeElementDetail.Add(trntEle);
                                    }
                                    else
                                    {
                                        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, trntEle);
                                        trntEle.UpdateDate = DateTime.Now;
                                        trntEle.UpdatedBy = oCompany.UserName;
                                    }

                                    //oApplication.StatusBar.SetText("Element amount updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                    //return;
                                }
                            }
                            dbHrPayroll.SubmitChanges();
                        }
                        #endregion

                        #region Contribution
                        if (ElementType.Trim() == "Con")
                        {
                            foreach (MstEmployee empCon in emps)
                            {
                                TrnsEmployeeElement empEleCon;
                                var oElementCon = (from a in dbHrPayroll.MstElementContribution where a.ElementId == oElement.Id select a).FirstOrDefault();
                                if (oElementCon == null)
                                {
                                    oApplication.StatusBar.SetText("Element not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                int cntded = (from p in dbHrPayroll.TrnsEmployeeElement where p.EmployeeId == empCon.ID select p).Count();
                                if (cntded == 0)
                                {
                                    empEleCon = new TrnsEmployeeElement();
                                    empEleCon.CreateDate = DateTime.Now;
                                    empEleCon.UserId = empCon.CreatedBy;
                                    empEleCon.EmployeeId = oElement.Id;
                                    empEleCon.FlgActive = true; //instance.FlgActive;
                                    dbHrPayroll.TrnsEmployeeElement.InsertOnSubmit(empEleCon);
                                }
                                else
                                {
                                    empEleCon = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empCon.EmpID select p).FirstOrDefault();
                                }
                                empEleCon = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empCon.EmpID select p).FirstOrDefault();
                                int linkedCntded = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.ElementId == linkedElement.ElementID select p).Count();


                                if (linkedCntded == 0)
                                {
                                    TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();

                                    trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                    trntEle.ElementId = linkedElement.ElementID;
                                    trntEle.FlgRetro = false;
                                    trntEle.StartDate = oElement.StartDate;
                                    trntEle.EndDate = oElement.EndDate;
                                    trntEle.ElementType = oElement.ElmtType;
                                    trntEle.ValueType = oElementCon.ContributionID;
                                    CalculateElementValues(ElementUpdate, empCon, trntEle.ValueType, trntEle);
                                    trntEle.FlgActive = linkedElement.FlgActive;
                                    empEleCon.TrnsEmployeeElementDetail.Add(trntEle);
                                }
                                else
                                {
                                    TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                    trntEle.MstElements = linkedElement.MstElements;
                                    trntEle.FlgActive = linkedElement.FlgActive;
                                    //trntEle.ValueType = oElementCon.ContributionID;
                                    CalculateElementValues(ElementUpdate, empCon, trntEle.ValueType, trntEle);
                                    //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                    trntEle.UpdateDate = DateTime.Now;
                                    trntEle.UpdatedBy = oCompany.UserName;
                                }
                                //dbHrPayroll.SubmitChanges();
                                //oApplication.StatusBar.SetText("Element amount updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                //return;
                            }
                            dbHrPayroll.SubmitChanges();
                        }
                        #endregion
                        //dbHrPayroll.SubmitChanges();
                    }
                }

                //else
                //{
                //    oApplication.StatusBar.SetText( ElementName + " Element update Needs Greater than Zero Value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return;
                //}
            }
            FillRecords();
            if (confirm == 1)
            {
                oApplication.StatusBar.SetText("Selected elements updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
        }
        //only update attached element with employees
        public void updateElementsAttachedElement()
        {
            int inc = 0;
            string selId = "0";
            bool flgPrevios = false;
            bool flgHit = false;
            int count = 0;
            #region CheckActivePeriod
            int Pcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == txName.Value.Trim() select p).Count();
            if (Pcnt > 0)
            {
                CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == txName.Value.Trim() select p).FirstOrDefault();
                foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                {
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
                            string pickId = selId;
                        }
                    }

                    inc++;
                }
            }
            #endregion

            #region Validation
            int confirm = oApplication.MessageBox("Are you sure you want to Update Element(s)? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3)
            {
                return;
            }
            else
            {
                oApplication.StatusBar.SetText("Payroll element update in progress please wait......", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            SAPbouiCOM.ProgressBar prog = null;
            var emps = (from p in dbHrPayroll.MstEmployee where p.PayrollID.ToString() == txCode.Value.ToString().Trim() && p.FlgActive == true select p).ToList();

            if (emps.Count == 0)
            {
                oApplication.StatusBar.SetText("Payroll not attached with any employee.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return;
            }
            #endregion
            for (int i = 0; i < dtElement.Rows.Count; i++)
            {
                mtElement.FlushToDataSource();
                string update = Convert.ToString(dtElement.GetValue("eUpdate", i));
                string Active = dtElement.GetValue("Active", i);
                string ElementUpdate = dtElement.GetValue("eValue", i);
                string ElementName = dtElement.GetValue("Element", i);
                int totalEmps = emps.Count();
                if (update == "Y")
                {

                    int ID = dtElement.GetValue("ElemId", i);

                    string ElementType = dtElement.GetValue("PrClass", i);

                    string periodName = txName.Value.Trim();

                    if (ElementUpdate == "0" || ElementUpdate == "")
                    {
                        oApplication.StatusBar.SetText(ElementName + " Element update Needs Greater than Zero Value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }

                    var oElement = (from a in dbHrPayroll.MstElements where a.Id == ID select a).FirstOrDefault();

                    var prds = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == txName.Value.Trim() select p).FirstOrDefault();
                    //var linkedElement = (from a in dbHrPayroll.MstElementLink where a.ElementID == oElement.Id && a.FlgActive == true && a.PayrollID == prds.ID select a).FirstOrDefault();
                    var linkedElement = (from a in dbHrPayroll.MstElementLink where a.ElementID == oElement.Id && a.PayrollID == prds.ID select a).FirstOrDefault();
                    if (oElement != null)
                    {
                        if (linkedElement.FlgActive == false)
                        {
                            oApplication.StatusBar.SetText("Please Active " + oElement.Description + " element .", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        if (ElementUpdate == "0" && ElementUpdate == null)
                        {
                            oApplication.StatusBar.SetText(ElementName + " Element update Needs Greater than Zero Value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        #region Earning
                        if (ElementType.Trim() == "Ear")
                        {
                            decimal outValue = 0M;
                            foreach (MstEmployee emp in emps)
                            {
                                TrnsEmployeeElement empEle;

                                var oElementEar = (from a in dbHrPayroll.MstElementEarning where a.ElementID == oElement.Id select a).FirstOrDefault();
                                if (oElementEar == null)
                                {
                                    oApplication.StatusBar.SetText("Element not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                                int cnt = (from p in dbHrPayroll.TrnsEmployeeElement where p.EmployeeId == emp.ID && p.FlgActive == true select p).Count();
                                if (cnt == 0)
                                {

                                }
                                else
                                {
                                    empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == emp.EmpID && p.FlgActive == true select p).FirstOrDefault();
                                }
                                empEle = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == emp.EmpID && p.FlgActive == true select p).FirstOrDefault();
                                //int linkedCnt = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && p.FlgOneTimeConsumed == false && p.FlgActive == true select p).Count();
                                int linkedCnt = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && p.FlgActive == true select p).Count();

                                if (oElement.Type == "Non-Rec")
                                {
                                    if (linkedCnt != 0)
                                    {
                                        //int CheckElementExist = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed == false || p.FlgOneTimeConsumed == null) && p.PeriodId != null && p.FlgActive == true select p).Count();
                                        //int CheckElementExist = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) != Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).Count();
                                        int CheckElementExist = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && (p.FlgActive != null ? p.FlgActive : false) == true select p).Count();
                                        if (CheckElementExist > 0)
                                        {
                                            TrnsEmployeeElementDetail ChecktrntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                            if (ChecktrntEle == null)
                                            {
                                                TrnsEmployeeElementDetail AddtrntEle = new TrnsEmployeeElementDetail();
                                                AddtrntEle.RetroAmount = Convert.ToDecimal(0.00);
                                                AddtrntEle.ElementId = linkedElement.ElementID;
                                                AddtrntEle.FlgRetro = false;
                                                AddtrntEle.StartDate = oElement.StartDate;
                                                AddtrntEle.EndDate = oElement.EndDate;
                                                AddtrntEle.ElementType = oElement.ElmtType;
                                                AddtrntEle.ValueType = oElementEar.ValueType;
                                                //POB,POG,FIX

                                                CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, AddtrntEle);
                                                AddtrntEle.FlgActive = linkedElement.FlgActive;
                                                AddtrntEle.PeriodId = Convert.ToInt32(selId);
                                                empEle.TrnsEmployeeElementDetail.Add(AddtrntEle);
                                            }
                                            else
                                            {
                                                //int CheckElementCountOne = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).Count();
                                                //if (CheckElementCountOne == 1)
                                                //{
                                                TrnsEmployeeElementDetail UpdatetrntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                                if (UpdatetrntEle != null)
                                                {
                                                    UpdatetrntEle.MstElements = linkedElement.MstElements;
                                                    UpdatetrntEle.FlgActive = linkedElement.FlgActive;
                                                    CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, UpdatetrntEle);
                                                    //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                                    UpdatetrntEle.PeriodId = Convert.ToInt32(selId);
                                                    UpdatetrntEle.UpdateDate = DateTime.Now;
                                                    UpdatetrntEle.UpdatedBy = oCompany.UserName;
                                                }
                                                //}
                                            }
                                        }
                                        //else
                                        //{
                                        //    int CheckElementCountOne = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).Count();
                                        //    if (CheckElementCountOne == 1)
                                        //    {
                                        //        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                        //        if (trntEle != null)
                                        //        {
                                        //            trntEle.MstElements = linkedElement.MstElements;
                                        //            trntEle.FlgActive = linkedElement.FlgActive;
                                        //            CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, trntEle);
                                        //            //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                        //            trntEle.PeriodId = Convert.ToInt32(selId);
                                        //            trntEle.UpdateDate = DateTime.Now;
                                        //            trntEle.UpdatedBy = oCompany.UserName;
                                        //        }
                                        //    }
                                        //}

                                    }
                                }
                                else
                                {
                                    if (linkedCnt != 0)
                                    {

                                        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && p.FlgActive == true select p).FirstOrDefault();
                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                        CalculateElementValues(ElementUpdate, emp, oElementEar.ValueType, trntEle);
                                        trntEle.UpdateDate = DateTime.Now;
                                        trntEle.UpdatedBy = oCompany.UserName;
                                    }
                                }

                            }
                            dbHrPayroll.SubmitChanges();
                        }

                        #endregion

                        #region Deductions
                        if (ElementType.Trim() == "Ded")
                        {
                            foreach (MstEmployee empded in emps)
                            {
                                TrnsEmployeeElement empEleDed;

                                //var oElementEar = (from a in dbHrPayroll.MstElementEarning where a.ElementID == oElement.Id select a).FirstOrDefault();
                                var oElementDed = (from a in dbHrPayroll.MstElementDeduction where a.ElementID == oElement.Id select a).FirstOrDefault();
                                //var oElementCon = (from a in dbHrPayroll.MstElementContribution where a.ElementId == oElement.Id select a).FirstOrDefault();
                                if (oElementDed == null)
                                {
                                    oApplication.StatusBar.SetText("Element not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                int cntded = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.FlgActive == true select p).Count();
                                if (cntded == 0)
                                {
                                }
                                else
                                {
                                    empEleDed = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empded.EmpID && p.FlgActive == true select p).FirstOrDefault();
                                }
                                empEleDed = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empded.EmpID && p.FlgActive == true select p).FirstOrDefault();
                                int linkedCntded = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID && p.FlgOneTimeConsumed == false && p.FlgActive == true select p).Count();


                                if (oElement.Type == "Non-Rec")
                                {
                                    if (linkedCntded != 0)
                                    {
                                        //int CheckElementExist = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed == false || p.FlgOneTimeConsumed == null) && p.PeriodId != null && p.FlgActive == true select p).Count();
                                        //int CheckElementExist = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) != Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).Count();
                                        int CheckElementExist = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && (p.FlgActive != null ? p.FlgActive : false) == true select p).Count();
                                        if (CheckElementExist > 0)
                                        {
                                            TrnsEmployeeElementDetail ChecktrntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                            if (ChecktrntEle == null)
                                            {
                                                TrnsEmployeeElementDetail AddtrntEle = new TrnsEmployeeElementDetail();
                                                AddtrntEle.RetroAmount = Convert.ToDecimal(0.00);
                                                AddtrntEle.ElementId = linkedElement.ElementID;
                                                AddtrntEle.FlgRetro = false;
                                                AddtrntEle.StartDate = oElement.StartDate;
                                                AddtrntEle.EndDate = oElement.EndDate;
                                                AddtrntEle.ElementType = oElement.ElmtType;
                                                AddtrntEle.ValueType = oElementDed.ValueType;
                                                //POB,POG,FIX

                                                CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, AddtrntEle);
                                                AddtrntEle.FlgActive = linkedElement.FlgActive;
                                                AddtrntEle.PeriodId = Convert.ToInt32(selId);
                                                empEleDed.TrnsEmployeeElementDetail.Add(AddtrntEle);
                                            }
                                            else
                                            {
                                                //int CheckElementCountOne = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).Count();
                                                //if (CheckElementCountOne == 1)
                                                //{
                                                TrnsEmployeeElementDetail UpdatetrntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                                if (UpdatetrntEle != null)
                                                {
                                                    UpdatetrntEle.MstElements = linkedElement.MstElements;
                                                    UpdatetrntEle.FlgActive = linkedElement.FlgActive;
                                                    CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, UpdatetrntEle);
                                                    //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                                    UpdatetrntEle.PeriodId = Convert.ToInt32(selId);
                                                    UpdatetrntEle.UpdateDate = DateTime.Now;
                                                    UpdatetrntEle.UpdatedBy = oCompany.UserName;
                                                }
                                                //}
                                            }
                                        }
                                        //else
                                        //{
                                        //    int CheckElementCountOne = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).Count();
                                        //    if (CheckElementCountOne == 1)
                                        //    {
                                        //        TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                        //        if (trntEle != null)
                                        //        {
                                        //            trntEle.MstElements = linkedElement.MstElements;
                                        //            trntEle.FlgActive = linkedElement.FlgActive;
                                        //            CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, trntEle);
                                        //            //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                        //            trntEle.PeriodId = Convert.ToInt32(selId);
                                        //            trntEle.UpdateDate = DateTime.Now;
                                        //            trntEle.UpdatedBy = oCompany.UserName;
                                        //        }
                                        //    }
                                        //}
                                    }
                                }
                                else
                                {
                                    if (linkedCntded != 0)
                                    {
                                        TrnsEmployeeElementDetail UpdatetrntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                        //TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empded.ID && p.ElementId == linkedElement.ElementID && p.FlgActive == true select p).FirstOrDefault();
                                        UpdatetrntEle.MstElements = linkedElement.MstElements;
                                        UpdatetrntEle.FlgActive = linkedElement.FlgActive;
                                        CalculateElementValues(ElementUpdate, empded, oElementDed.ValueType, UpdatetrntEle);
                                        UpdatetrntEle.UpdateDate = DateTime.Now;
                                        UpdatetrntEle.UpdatedBy = oCompany.UserName;
                                    }

                                }
                            }
                            dbHrPayroll.SubmitChanges();
                        }
                        #endregion

                        #region Contribution
                        if (ElementType.Trim() == "Con")
                        {
                            foreach (MstEmployee empCon in emps)
                            {
                                TrnsEmployeeElement empEleCon;
                                var oElementCon = (from a in dbHrPayroll.MstElementContribution where a.ElementId == oElement.Id select a).FirstOrDefault();
                                if (oElementCon == null)
                                {
                                    oApplication.StatusBar.SetText("Element not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                int cntded = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.FlgActive == true select p).Count();
                                if (cntded == 0)
                                {
                                    //empEleCon = new TrnsEmployeeElement();
                                    //empEleCon.CreateDate = DateTime.Now;
                                    //empEleCon.UserId = empCon.CreatedBy;
                                    //empEleCon.EmployeeId = oElement.Id;
                                    //empEleCon.FlgActive = true; //instance.FlgActive;
                                    //dbHrPayroll.TrnsEmployeeElement.InsertOnSubmit(empEleCon);
                                }
                                else
                                {
                                    empEleCon = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empCon.EmpID && p.FlgActive == true select p).FirstOrDefault();
                                }
                                empEleCon = (from p in dbHrPayroll.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == empCon.EmpID && p.FlgActive == true select p).FirstOrDefault();
                                int linkedCntded = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.ElementId == linkedElement.ElementID && p.FlgOneTimeConsumed == false && p.FlgActive == true select p).Count();


                                if (linkedCntded != 0)
                                {

                                    //TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();

                                    //trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                    //trntEle.ElementId = linkedElement.ElementID;
                                    //trntEle.FlgRetro = false;
                                    //trntEle.StartDate = oElement.StartDate;
                                    //trntEle.EndDate = oElement.EndDate;
                                    //trntEle.ElementType = oElement.ElmtType;
                                    //trntEle.ValueType = oElementCon.ContributionID;
                                    //CalculateElementValues(ElementUpdate, empCon, trntEle.ValueType, trntEle);
                                    //trntEle.FlgActive = linkedElement.FlgActive;
                                    //empEleCon.TrnsEmployeeElementDetail.Add(trntEle);
                                    //TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                    TrnsEmployeeElementDetail UpdatetrntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.ElementId == linkedElement.ElementID && (p.PeriodId != null ? p.PeriodId : 0) == Convert.ToInt32(selId) && (p.FlgOneTimeConsumed != null ? p.FlgOneTimeConsumed : false) == false && p.FlgActive == true select p).FirstOrDefault();
                                    UpdatetrntEle.MstElements = linkedElement.MstElements;
                                    UpdatetrntEle.FlgActive = linkedElement.FlgActive;
                                    //trntEle.ValueType = oElementCon.ContributionID;
                                    CalculateElementValues(ElementUpdate, empCon, UpdatetrntEle.ValueType, UpdatetrntEle);
                                    //trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                    UpdatetrntEle.UpdateDate = DateTime.Now;
                                    UpdatetrntEle.UpdatedBy = oCompany.UserName;

                                }
                                else
                                {
                                    //TrnsEmployeeElementDetail trntEle = (from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == empCon.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                    //trntEle.MstElements = linkedElement.MstElements;
                                    //trntEle.FlgActive = linkedElement.FlgActive;
                                    ////trntEle.ValueType = oElementCon.ContributionID;
                                    //CalculateElementValues(ElementUpdate, empCon, trntEle.ValueType, trntEle);
                                    ////trntEle.Amount = Convert.ToDecimal(ElementUpdate);
                                    //trntEle.UpdateDate = DateTime.Now;
                                    //trntEle.UpdatedBy = oCompany.UserName;
                                }
                                //dbHrPayroll.SubmitChanges();
                                //oApplication.StatusBar.SetText("Element amount updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                //return;
                            }
                            dbHrPayroll.SubmitChanges();
                        }
                        #endregion
                        //dbHrPayroll.SubmitChanges();
                    }
                }

                //else
                //{
                //    oApplication.StatusBar.SetText( ElementName + " Element update Needs Greater than Zero Value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return;
                //}
            }
            FillRecords();
            if (confirm == 1)
            {
                oApplication.StatusBar.SetText("Selected elements updated successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
        }

        private void CalculateElementValues(string ElementUpdate, MstEmployee emp, string elementValueType, TrnsEmployeeElementDetail trntEle)
        {
            decimal outValue = 0M;
            if (elementValueType == "POB")
            {
                trntEle.Value = Convert.ToDecimal(ElementUpdate);
                outValue = Convert.ToDecimal(ElementUpdate) / 100 * (decimal)emp.BasicSalary;
                trntEle.Amount = outValue;
            }
            if (elementValueType == "POG")
            {
                trntEle.Value = Convert.ToDecimal(ElementUpdate);
                outValue = Convert.ToDecimal(ElementUpdate) / 100 * (decimal)ds.getEmpGross(emp);
                //outValue = Convert.ToDecimal(ElementUpdate) / 100 * (decimal)emp.GrossSalary;
                trntEle.Amount = outValue;
            }
            if (elementValueType == "FIX")
            {
                trntEle.Value = Convert.ToDecimal(ElementUpdate);
                trntEle.Amount = Convert.ToDecimal(ElementUpdate);
            }

        }

        #endregion

    }
}
