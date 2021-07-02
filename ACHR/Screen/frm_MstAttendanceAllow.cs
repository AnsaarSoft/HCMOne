using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_MstAttendanceAllow : HRMSBaseForm
    {
        #region Variables

        private SAPbouiCOM.DataTable dtMain;
        private SAPbouiCOM.Matrix grdMain;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clLevType, clLeaveCount, clAllowanceElement, clAllowanceAmount, clActive;
        private SAPbouiCOM.EditText oCell, txtDocNum, txtAllowanceCode;

        SAPbouiCOM.Item ItxtDocNum, ItxtAllowanceCode;
        private MstAttendanceAllowance oAttendanceAllowance;
        Boolean flgAllowanceStatusChange, flgUserTrigger;

        #endregion

        #region Form B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillAllowanceElement();

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
                txtAllowanceCode = oForm.Items.Item("txtCode").Specific;
                ItxtAllowanceCode = oForm.Items.Item("txtCode");
                txtAllowanceCode.DataBind.SetBound(true, "", "txtCode");

                dtMain = oForm.DataSources.DataTables.Add("LevTyp");
                dtMain.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("LevType", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("LveCnt", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("ElementType", SAPbouiCOM.BoFieldsType.ft_Text, 50);
                dtMain.Columns.Add("AllAmt", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text, 1);

                grdMain = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                grdMain.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)grdMain.Columns;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("LevTyp", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("LevTyp", "I");

                oColumn = oColumns.Item("cl_LevType");
                clLevType = oColumn;
                oColumn.DataBind.Bind("LevTyp", "LevType");

                oColumn = oColumns.Item("cl_LveCnt");
                clLeaveCount = oColumn;
                oColumn.DataBind.Bind("LevTyp", "LveCnt");

                oColumn = oColumns.Item("cl_elmnt");
                clAllowanceElement = oColumn;
                oColumn.DataBind.Bind("LevTyp", "ElementType");

                oColumn = oColumns.Item("cl_AllAmt");
                clAllowanceAmount = oColumn;
                oColumn.DataBind.Bind("LevTyp", "AllAmt");

                oColumn = oColumns.Item("cl_Active");
                clActive = oColumn;
                oColumn.DataBind.Bind("LevTyp", "Active");

                base.fillColumCombo("LevTyp_LevType", clLevType);
                clLevType.DisplayDesc = true;

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
                txtAllowanceCode.Value = "";

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
            int? intIdt = dbHrPayroll.MstAttendanceAllowance.Max(u => (int?)u.DocNo);
            //int DocCount = dbHrPayroll.TrnsLeavesRequest.Count() + 1;
            intIdt = intIdt == null ? 1 : intIdt + 1;
            txtDocNum.Value = Convert.ToString(intIdt);

        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                var oDocuments = (from a in dbHrPayroll.MstAttendanceAllowance select a).ToList();
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
                    var oDoc = (from a in dbHrPayroll.MstAttendanceAllowance
                                where a.DocNo.ToString() == value
                                select a).FirstOrDefault();
                    if (oDoc == null)
                    {
                        dtMain.Rows.Clear();
                        grdMain.LoadFromDataSource();
                        return;
                    }
                    txtDocNum.Value = Convert.ToString(oDoc.DocNo);
                    txtAllowanceCode.Value = oDoc.Code;
                    var GetAllData = (from n in dbHrPayroll.MstAttendanceAllowance select n).ToList();
                    var Data = GetAllData.Where(a => a.DocNo == oDoc.DocNo).ToList();

                    if (Data != null && Data.Count > 0)
                    {
                        dtMain.Rows.Clear();
                        foreach (var v in Data)
                        {
                            dtMain.Rows.Add(1);
                            dtMain.SetValue("No", i, i + 1);
                            dtMain.SetValue("I", i, v.ID);
                            dtMain.SetValue("LevType", i, v.LeaveType);
                            dtMain.SetValue("LveCnt", i, v.LeaveCount);
                            dtMain.SetValue("ElementType", i, v.ElementType == null ? "-1" : v.ElementType);
                            dtMain.SetValue("AllAmt", i, v.Value.ToString());
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
                    txtAllowanceCode.Value = v.Code;
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

        private void FillAllowanceElement()
        {
            try
            {
                clAllowanceElement.ValidValues.Add("-1", "");
                var Records = from v in dbHrPayroll.MstElements where v.FlgAttendanceAllowance == true select v;
                foreach (var Record in Records)
                {
                    clAllowanceElement.ValidValues.Add(Record.ElementName, Record.Description);
                }
                clAllowanceElement.DisplayDesc = true;
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
                var oDoc = (from a in dbHrPayroll.MstAttendanceAllowance
                            where a.DocNo == DocNum
                            select a).FirstOrDefault();
                if (oDoc == null)
                {
                    for (int i = 1; i <= grdMain.RowCount; i++)
                    {
                        string leavetype, LeaveCount, AllowanceElement, AllowanceAmount;
                        int lineid;
                        Boolean flgActive;
                        lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                        leavetype = (grdMain.Columns.Item(clLevType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        LeaveCount = (grdMain.Columns.Item(clLeaveCount.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        AllowanceElement = (grdMain.Columns.Item(clAllowanceElement.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        AllowanceAmount = (grdMain.Columns.Item(clAllowanceAmount.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                        if (lineid == 0)
                        {
                            if (leavetype != "")
                            {
                                if (string.IsNullOrEmpty(leavetype) && string.IsNullOrEmpty(AllowanceElement)) continue;
                                MstAttendanceAllowance oNew = new MstAttendanceAllowance();
                                oNew.DocNo = Convert.ToInt32(txtDocNum.Value.Trim());
                                oNew.Code = txtAllowanceCode.Value.Trim();

                                oNew.LeaveType = leavetype;
                                oNew.LeaveCount = Convert.ToInt32(LeaveCount);
                                oNew.ElementType = AllowanceElement;
                                oNew.Value = Convert.ToDecimal(AllowanceAmount);
                                oNew.FlgActive = flgActive;

                                oNew.CreatedDate = DateTime.Now;
                                oNew.CreatedBy = oCompany.UserName;
                                dbHrPayroll.MstAttendanceAllowance.InsertOnSubmit(oNew);
                            }

                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= grdMain.RowCount; i++)
                    {
                        string leavetype, LeaveCount, AllowanceElement, AllowanceAmount;
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
                        leavetype = (grdMain.Columns.Item(clLevType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        LeaveCount = (grdMain.Columns.Item(clLeaveCount.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        AllowanceElement = (grdMain.Columns.Item(clAllowanceElement.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        AllowanceAmount = (grdMain.Columns.Item(clAllowanceAmount.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                        //MstAttendanceAllowance oDoc = (from a in dbHrPayroll.MstAttendanceAllowance where a.ID == lineid select a).FirstOrDefault();
                        if (oDoc == null) continue;
                        if (lineid == 0)
                        {
                            if (string.IsNullOrEmpty(leavetype) && string.IsNullOrEmpty(AllowanceElement)) continue;
                            MstAttendanceAllowance oNew = new MstAttendanceAllowance();
                            oNew.DocNo = Convert.ToInt32(txtDocNum.Value.Trim());
                            oNew.Code = txtAllowanceCode.Value.Trim();

                            oNew.LeaveType = leavetype;
                            oNew.LeaveCount = Convert.ToInt32(LeaveCount);
                            oNew.ElementType = AllowanceElement;
                            oNew.Value = Convert.ToDecimal(AllowanceAmount);
                            oNew.FlgActive = flgActive;

                            oNew.CreatedDate = DateTime.Now;
                            oNew.CreatedBy = oCompany.UserName;
                            dbHrPayroll.MstAttendanceAllowance.InsertOnSubmit(oNew);
                        }
                        else
                        {
                            if (leavetype != "")
                            {
                                var oValue = (from a in dbHrPayroll.MstAttendanceAllowance
                                              where a.ID == lineid
                                              select a).FirstOrDefault();

                                oValue.DocNo = Convert.ToInt32(txtDocNum.Value.Trim());
                                oValue.Code = txtAllowanceCode.Value.Trim();
                                oValue.LeaveType = leavetype;
                                oValue.LeaveCount = Convert.ToInt32(LeaveCount);
                                oValue.ElementType = AllowanceElement;
                                oValue.Value = Convert.ToDecimal(AllowanceAmount);



                                oValue.UpdatedDate = DateTime.Now;
                                oValue.UpdatedBy = oCompany.UserName;
                                oValue.FlgActive = flgActive;

                            }
                        }

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
                if (txtAllowanceCode.Value.Trim().Equals(""))
                {
                    oApplication.StatusBar.SetText("Allowance code is madatory:", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                for (int i = 1; i <= grdMain.RowCount; i++)
                {
                    string leavetype, LeaveCount, AllowanceElement, AllowanceAmount;
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
                    leavetype = (grdMain.Columns.Item(clLevType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    LeaveCount = (grdMain.Columns.Item(clLeaveCount.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    AllowanceElement = (grdMain.Columns.Item(clAllowanceElement.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    AllowanceAmount = (grdMain.Columns.Item(clAllowanceAmount.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                    if (lineid == 0)//new record
                    {
                        if (string.IsNullOrEmpty(leavetype) && !string.IsNullOrEmpty(AllowanceElement))
                        {
                            oApplication.StatusBar.SetText("Leave Type is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(leavetype) && string.IsNullOrEmpty(AllowanceElement))
                        {
                            oApplication.StatusBar.SetText("Element Type is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        //if (string.IsNullOrEmpty(LeaveCount))
                        //{
                        //    oApplication.StatusBar.SetText("Leave Count is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return false;
                        //}
                        //if (string.IsNullOrEmpty(AllowanceAmount))
                        //{
                        //    oApplication.StatusBar.SetText("Allowance Value is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return false;
                        //}

                    }
                    else //old record
                    {
                        if (string.IsNullOrEmpty(leavetype) && !string.IsNullOrEmpty(AllowanceElement))
                        {
                            oApplication.StatusBar.SetText("Leave Type is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(leavetype) && string.IsNullOrEmpty(AllowanceElement))
                        {
                            oApplication.StatusBar.SetText("Element Type is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        //if (string.IsNullOrEmpty(LeaveCount))
                        //{
                        //    oApplication.StatusBar.SetText("Leave Count is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return false;
                        //}
                        //if (string.IsNullOrEmpty(AllowanceAmount))
                        //{
                        //    oApplication.StatusBar.SetText("Allowance Value is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return false;
                        //}

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
                MstAttendanceAllowance Record = (from v in dbHrPayroll.MstAttendanceAllowance where v.ID == ID select v).Single();
                dbHrPayroll.MstAttendanceAllowance.DeleteOnSubmit(Record);
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
