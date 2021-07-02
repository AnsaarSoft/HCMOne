using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_MstEmployeeBonus : HRMSBaseForm
    {
        #region Variables

        private SAPbouiCOM.DataTable dtMain;
        private SAPbouiCOM.Matrix grdMain;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clCode, clValueType, clSalaryFrom, clSalaryTo, clScaleFrom, clScaleTo, clValue, clMinimumMonthsDuration, clElementType, clActive;
        private SAPbouiCOM.EditText oCell, txtDocNum, txtCode;

        SAPbouiCOM.Item ItxtDocNum, ItxtCode;
        private MstBonusYearly oEmployeeBonus;
        Boolean flgUserTrigger;

        #endregion

        #region Form B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillEmployeeElement();

            InitiallizeDocument();

            oForm.Freeze(false);
        }

        public override void etBeforeValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                switch (pVal.ItemUID)
                {
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {

                                case "cl_Code":
                                    var Code = (grdMain.Columns.Item("cl_Code").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (Code.Equals("") && pVal.Row != grdMain.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        BubbleEvent = false;
                                        return;
                                    }

                                    for (int i = 1; i <= grdMain.RowCount; i++)
                                    {
                                        oCell = grdMain.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                        if (i == pVal.Row)
                                            continue;
                                        else if (Code == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;

                                case "cl_Desc":
                                    var Desc = (grdMain.Columns.Item("cl_Desc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();

                                    if (Desc.Equals("") && pVal.Row != grdMain.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        BubbleEvent = false;
                                        return;
                                    }
                                    for (int i = 1; i <= grdMain.RowCount; i++)
                                    {
                                        oCell = grdMain.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                        if (i == pVal.Row)
                                            continue;
                                        else if (Desc == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;
                            }

                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE || oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        if (!ValidateRecords())
                        {
                            BubbleEvent = false;
                        }
                        else
                        {
                            flgUserTrigger = true;
                        }
                    }
                    break;
                case "btn_del":
                    int Count = 0;
                    for (int i = 1; i < grdMain.RowCount; i++)
                    {
                        if (grdMain.IsRowSelected(i))
                        {
                            int Res = oApplication.MessageBox(Program.objHrmsUI.getStrMsg("War_DelRow"), 1, "Yes", "No", "");
                            if (Res == 1)
                            {
                                DeleteRecord(i);
                            }
                            else
                            {
                                grdMain.SelectRow(i, false, false);
                            }
                            Count += 1;
                        }

                    }
                    if (Count == 0)
                    {
                        oApplication.MessageBox(Program.objHrmsUI.getStrMsg("War_SelectRow"), 1, "OK", "", "");
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
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE && !flgUserTrigger)
                            return;
                        else
                            SubmitRecords();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public override void fillFields()
        {
            base.fillFields();
            oForm.Freeze(true);
            InitiallizeDocument();
            FillRecord();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            oForm.Freeze(false);
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            InitiallizeDocument();
        }
        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                oForm.DataSources.UserDataSources.Add("txtDocNo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txtDocNum = oForm.Items.Item("txtDocNo").Specific;
                ItxtDocNum = oForm.Items.Item("txtDocNo");
                txtDocNum.DataBind.SetBound(true, "", "txtDocNo");

                oForm.DataSources.UserDataSources.Add("txtCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txtCode = oForm.Items.Item("txtCode").Specific;
                ItxtCode = oForm.Items.Item("txtCode");
                txtCode.DataBind.SetBound(true, "", "txtCode");

                dtMain = oForm.DataSources.DataTables.Add("dtRecords");
                dtMain.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("code", SAPbouiCOM.BoFieldsType.ft_Text, 50);
                dtMain.Columns.Add("ValueType", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                dtMain.Columns.Add("SalaryFrom", SAPbouiCOM.BoFieldsType.ft_Float);
                dtMain.Columns.Add("SalaryTo", SAPbouiCOM.BoFieldsType.ft_Float);
                dtMain.Columns.Add("ScaleFrom", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("ScaleTo", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("Value", SAPbouiCOM.BoFieldsType.ft_Float);
                dtMain.Columns.Add("MonthDur", SAPbouiCOM.BoFieldsType.ft_Float);
                dtMain.Columns.Add("ElementType", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                dtMain.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                //clValType
                grdMain = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                grdMain.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)grdMain.Columns;

                oColumn = oColumns.Item("clno");
                clNo = oColumn;
                oColumn.DataBind.Bind("dtRecords", "No");

                oColumn = oColumns.Item("clID");
                clID = oColumn;
                oColumn.DataBind.Bind("dtRecords", "I");

                oColumn = oColumns.Item("clCode");
                clCode = oColumn;
                oColumn.DataBind.Bind("dtRecords", "code");

                oColumn = oColumns.Item("clValType");
                clValueType = oColumn;
                oColumn.DataBind.Bind("dtRecords", "ValueType");

                oColumn = oColumns.Item("clSlryFrm");
                clSalaryFrom = oColumn;
                oColumn.DataBind.Bind("dtRecords", "SalaryFrom");

                oColumn = oColumns.Item("clSlryTo");
                clSalaryTo = oColumn;
                oColumn.DataBind.Bind("dtRecords", "SalaryTo");

                oColumn = oColumns.Item("clSclFrm");
                clScaleFrom = oColumn;
                oColumn.DataBind.Bind("dtRecords", "ScaleFrom");

                oColumn = oColumns.Item("clSclTo");
                clScaleTo = oColumn;
                oColumn.DataBind.Bind("dtRecords", "ScaleTo");

                oColumn = oColumns.Item("clValue");
                clValue = oColumn;
                oColumn.DataBind.Bind("dtRecords", "Value");

                oColumn = oColumns.Item("clMonthDur");
                clMinimumMonthsDuration = oColumn;
                oColumn.DataBind.Bind("dtRecords", "MonthDur");

                oColumn = oColumns.Item("clElmnt");
                clElementType = oColumn;
                oColumn.DataBind.Bind("dtRecords", "ElementType");

                oColumn = oColumns.Item("clActive");
                clActive = oColumn;
                oColumn.DataBind.Bind("dtRecords", "Active");

                //fillColumCombo("Val_Type", clValueType);

                fillColumComboBonus();

                grdMain.AutoResizeColumns();
                GetData();
                InitiallizeDocument();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                txtCode.Value = "";
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                GetDocumentNo();
                AddBlankRow();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeDocument : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetDocumentNo()
        {
            long nextId = 0;
            nextId = ds.getNextId("MstBonusYearly", "DocNo");

            txtDocNum.Value = Convert.ToString(nextId);

        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                var oDocuments = (from a in dbHrPayroll.MstBonusYearly select a).ToList();
                Int32 i = 0;
                foreach (var oDoc in oDocuments)
                {
                    CodeIndex.Add(i, oDoc.DocNo);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
            }
        }

        private void FillRecord()
        {
            Int16 i = 0;
            try
            {
                if (CodeIndex.Count == 0) return;
                string value = CodeIndex[currentRecord].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var oDoc = (from a in dbHrPayroll.MstBonusYearly
                                where a.DocNo.ToString() == value
                                select a).FirstOrDefault();
                    if (oDoc == null)
                    {
                        dtMain.Rows.Clear();
                        grdMain.LoadFromDataSource();
                        return;
                    }
                    txtDocNum.Value = Convert.ToString(oDoc.DocNo);
                    txtCode.Value = oDoc.DocCode;
                    var GetAllData = (from n in dbHrPayroll.MstBonusYearly select n).ToList();
                    var Data = GetAllData.Where(a => a.DocCode == oDoc.DocCode).ToList();

                    if (Data != null && Data.Count > 0)
                    {
                        dtMain.Rows.Clear();
                        foreach (var v in Data)
                        {
                            dtMain.Rows.Add(1);
                            dtMain.SetValue("No", i, i + 1);
                            dtMain.SetValue("I", i, v.ID);
                            dtMain.SetValue("code", i, v.Code);
                            dtMain.SetValue("ValueType", i, v.ValueType);
                            dtMain.SetValue("SalaryFrom", i, Convert.ToDouble(Math.Round(Convert.ToDecimal((float)v.SalaryFrom), 2)));
                            dtMain.SetValue("SalaryTo", i, Convert.ToDouble(Math.Round(Convert.ToDecimal((float)v.SalaryTo), 2)));
                            dtMain.SetValue("ScaleFrom", i, Convert.ToInt32(v.ScaleFrom.GetValueOrDefault()));
                            dtMain.SetValue("ScaleTo", i, Convert.ToInt32(v.ScaleTo.GetValueOrDefault()));
                            dtMain.SetValue("Value", i, Convert.ToDouble(Math.Round(Convert.ToDecimal((float)v.BonusPercentage), 2)));
                            dtMain.SetValue("MonthDur", i, Convert.ToDouble(Math.Round(Convert.ToDecimal((float)v.MinimumMonthsDuration), 2)));
                            var oElements = (from e in dbHrPayroll.MstElements where e.Id == v.ElementType select e).FirstOrDefault();
                            if (oElements != null)
                            {
                                dtMain.SetValue("ElementType", i, oElements.ElementName);
                            }
                            dtMain.SetValue("Active", i, v.FlgActive == true ? "Y" : "N");

                            i += 1;
                        }
                        grdMain.LoadFromDataSource();
                    }
                }
                AddBlankRow();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void getDataOld()
        {
            try
            {
                grdMain.FlushToDataSource();
                grdMain.Clear();
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstAttendanceAllowance);
                var Data = from n in dbHrPayroll.MstAttendanceAllowance select n;
                dtMain.Rows.Clear();
                int i = 0;


                foreach (var v in Data)
                {
                    //txtDocNum.Value = Convert.ToString(v.DocNo);
                    txtCode.Value = v.Code;
                    dtMain.Rows.Add(1);
                    dtMain.SetValue("No", i, i + 1);
                    dtMain.SetValue("I", i, v.ID);
                    dtMain.SetValue("LevType", i, v.LeaveType);
                    dtMain.SetValue("LveCnt", i, v.LeaveCount);
                    dtMain.SetValue("ElementType", i, v.ElementType == null ? "-1" : v.ElementType);
                    //dtMain.SetValue("ElementType", i, v.ElementType);
                    dtMain.SetValue("AllAmt", i, v.Value.ToString());
                    dtMain.SetValue("Active", i, v.FlgActive == true ? "Y" : "N");

                    i += 1;
                }
                grdMain.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        protected virtual void fillColumComboBonus()
        {
            var Types = dbHrPayroll.MstLOVE.Where(x => x.Type == "Val_Type" && x.Code != "FIX").ToList();
            foreach (var type in Types)
            {
                clValueType.ValidValues.Add(type.Code, type.Value);
            }
        }

        private void AddBlankRow()
        {
            try
            {
                dtMain.Rows.Clear();
                grdMain.AddRow(1, grdMain.RowCount + 1);
                (grdMain.Columns.Item(clNo.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value = (grdMain.RowCount).ToString();
                (grdMain.Columns.Item(clID.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value = "0";
                grdMain.FlushToDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillEmployeeElement()
        {
            try
            {
                clElementType.ValidValues.Add("-1", "");
                var Records = from v in dbHrPayroll.MstElements where v.FlgEmployeeBonus == true select v;
                foreach (var Record in Records)
                {
                    clElementType.ValidValues.Add(Record.ElementName, Record.Description);
                }
                clElementType.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SubmitRecords()
        {
            try
            {
                int DocNum;
                DocNum = Convert.ToInt32(txtDocNum.Value.Trim());
                var oDoc = (from a in dbHrPayroll.MstBonusYearly
                            where a.DocNo == DocNum
                            select a).FirstOrDefault();
                if (oDoc == null)
                {
                    for (int i = 1; i <= grdMain.RowCount; i++)
                    {
                        string Code, SalaryFrom, SalaryTo, Value, MinimumMonthsDuration, ElementType, ScaleFrom, ScaleTo, ValueType;
                        int lineid;
                        Boolean flgActive;
                        lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                        Code = (grdMain.Columns.Item(clCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ValueType = (grdMain.Columns.Item(clValueType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        SalaryFrom = (grdMain.Columns.Item(clSalaryFrom.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        SalaryTo = (grdMain.Columns.Item(clSalaryTo.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ScaleFrom = (grdMain.Columns.Item(clScaleFrom.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ScaleTo = (grdMain.Columns.Item(clScaleTo.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        Value = (grdMain.Columns.Item(clValue.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        MinimumMonthsDuration = (grdMain.Columns.Item(clMinimumMonthsDuration.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ElementType = (grdMain.Columns.Item(clElementType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                        if (lineid == 0)
                        {
                            if (Code != "")
                            {
                                if (string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(ElementType)) continue;
                                var oElement = (from a in dbHrPayroll.MstElements where a.ElementName == ElementType select a).FirstOrDefault();

                                MstBonusYearly oNew = new MstBonusYearly();
                                oNew.DocNo = Convert.ToInt32(txtDocNum.Value.Trim());
                                oNew.DocCode = txtCode.Value.Trim();
                                oNew.Code = Code;
                                oNew.ValueType = ValueType;
                                oNew.SalaryFrom = Convert.ToDecimal(SalaryFrom);
                                oNew.SalaryTo = Convert.ToDecimal(SalaryTo);
                                oNew.ScaleFrom = Convert.ToInt32(ScaleFrom);
                                oNew.ScaleTo = Convert.ToInt32(ScaleTo);
                                oNew.BonusPercentage = Convert.ToDecimal(Value);
                                oNew.MinimumMonthsDuration = Convert.ToDecimal(MinimumMonthsDuration);
                                oNew.ElementType = Convert.ToInt32(oElement.Id);

                                oNew.FlgActive = flgActive;

                                oNew.CreatedDate = DateTime.Now;
                                oNew.CreatedBy = oCompany.UserName;
                                dbHrPayroll.MstBonusYearly.InsertOnSubmit(oNew);
                            }

                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= grdMain.RowCount; i++)
                    {
                        string Code, SalaryFrom, SalaryTo, Value, MinimumMonthsDuration, ElementType, ScaleFrom, ScaleTo, ValueType;
                        int lineid;
                        Boolean flgActive;
                        if ((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value == "")
                        {
                            lineid = 0;
                        }
                        else
                        {
                            lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                        }
                        Code = (grdMain.Columns.Item(clCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ValueType = (grdMain.Columns.Item(clValueType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        SalaryFrom = (grdMain.Columns.Item(clSalaryFrom.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        SalaryTo = (grdMain.Columns.Item(clSalaryTo.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ScaleFrom = (grdMain.Columns.Item(clScaleFrom.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ScaleTo = (grdMain.Columns.Item(clScaleTo.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        Value = (grdMain.Columns.Item(clValue.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        MinimumMonthsDuration = (grdMain.Columns.Item(clMinimumMonthsDuration.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        ElementType = (grdMain.Columns.Item(clElementType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        if (string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(ElementType)) continue;
                        var oElement = (from a in dbHrPayroll.MstElements where a.ElementName == ElementType select a).FirstOrDefault();
                        if (oDoc == null) continue;
                        if (lineid == 0)
                        {


                            MstBonusYearly oNew = new MstBonusYearly();


                            oNew.DocNo = Convert.ToInt32(txtDocNum.Value.Trim());
                            oNew.DocCode = txtCode.Value.Trim();
                            oNew.Code = Code;
                            oNew.ValueType = ValueType;
                            oNew.SalaryFrom = Convert.ToDecimal(SalaryFrom);
                            oNew.SalaryTo = Convert.ToDecimal(SalaryTo);
                            oNew.ScaleFrom = Convert.ToInt32(ScaleFrom);
                            oNew.ScaleTo = Convert.ToInt32(ScaleTo);
                            oNew.BonusPercentage = Convert.ToDecimal(Value);
                            oNew.MinimumMonthsDuration = Convert.ToDecimal(MinimumMonthsDuration);
                            oNew.ElementType = Convert.ToInt32(oElement.Id);

                            oNew.FlgActive = flgActive;

                            oNew.CreatedDate = DateTime.Now;
                            oNew.CreatedBy = oCompany.UserName;
                            dbHrPayroll.MstBonusYearly.InsertOnSubmit(oNew);
                        }
                        else
                        {
                            if (Code != "")
                            {
                                var oValue = (from a in dbHrPayroll.MstBonusYearly
                                              where a.ID == lineid
                                              select a).FirstOrDefault();

                                oValue.DocNo = Convert.ToInt32(txtDocNum.Value.Trim());
                                //oValue.DocCode = txtCode.Value.Trim();

                                oValue.Code = Code;
                                oValue.ValueType = ValueType;
                                oValue.SalaryFrom = Convert.ToDecimal(SalaryFrom);
                                oValue.SalaryTo = Convert.ToDecimal(SalaryTo);
                                oValue.ScaleFrom = Convert.ToInt32(ScaleFrom);
                                oValue.ScaleTo = Convert.ToInt32(ScaleTo);
                                oValue.BonusPercentage = Convert.ToDecimal(Value);
                                oValue.MinimumMonthsDuration = Convert.ToDecimal(MinimumMonthsDuration);
                                if (oElement == null)
                                {
                                    oValue.ElementType = 0;
                                }
                                else
                                {
                                    oValue.ElementType = Convert.ToInt32(oElement.Id);
                                }

                                oValue.FlgActive = flgActive;

                                oValue.UpdatedDate = DateTime.Now;
                                oValue.UpdatedBy = oCompany.UserName;
                                oValue.FlgActive = flgActive;
                            }
                        }
                        //dbHrPayroll.SubmitChanges();
                    }
                }
                dbHrPayroll.SubmitChanges();
                GetData();
                InitiallizeDocument();
                dtMain.Rows.Clear();
                grdMain.LoadFromDataSource();
                //AddBlankRow();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function: SubmitRecords Msg: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                oForm.Freeze(false);
            }
        }

        private Boolean ValidateRecords()
        {
            try
            {
                if (txtCode.Value.Trim().Equals(""))
                {
                    oApplication.StatusBar.SetText("Doc code is madatory:", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                for (int i = 1; i <= grdMain.RowCount; i++)
                {
                    string Code, SalaryFrom, SalaryTo, Value, MinimumMonthsDuration, ElementType;
                    int lineid;
                    Boolean flgActive;
                    lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    if ((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value == "")
                    {
                        lineid = 0;
                    }
                    else
                    {
                        lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    }
                    Code = (grdMain.Columns.Item(clCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    SalaryFrom = (grdMain.Columns.Item(clSalaryFrom.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    SalaryTo = (grdMain.Columns.Item(clSalaryTo.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Value = (grdMain.Columns.Item(clValue.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    MinimumMonthsDuration = (grdMain.Columns.Item(clMinimumMonthsDuration.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    ElementType = (grdMain.Columns.Item(clElementType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                    if (lineid == 0)//new record
                    {
                        if (string.IsNullOrEmpty(Code) && !string.IsNullOrEmpty(ElementType))
                        {
                            oApplication.StatusBar.SetText("Code is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(ElementType))
                        {
                            oApplication.StatusBar.SetText("Element Type is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }

                    }
                    else //old record
                    {
                        if (string.IsNullOrEmpty(Code) && !string.IsNullOrEmpty(ElementType))
                        {
                            oApplication.StatusBar.SetText("Code is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(ElementType))
                        {
                            oApplication.StatusBar.SetText("Element Type is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void DeleteRecord(int Row)
        {
            try
            {
                oForm.Freeze(true);
                int ID = dtMain.GetValue("I", Row - 1);
                MstBonusYearly Record = (from v in dbHrPayroll.MstBonusYearly where v.ID == ID select v).Single();
                dbHrPayroll.MstBonusYearly.DeleteOnSubmit(Record);
                dbHrPayroll.SubmitChanges();
                dtMain.Rows.Remove(Row - 1);
                grdMain.Clear();
                FillRecord();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        #endregion
    }
}
