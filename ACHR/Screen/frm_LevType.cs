using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_LevType:HRMSBaseForm
    {
        #region Variables

        private SAPbouiCOM.DataTable dtMain;
        private SAPbouiCOM.Matrix grdMain;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clCode, clDesc, clLevType, clDedCode, clPDedCod, clAccural, clEncash, clEncashElement, clActive,clConDeduction, clDefault,clDaysInYear,clMonth, clLeaveCap, clCarryForward, clProRate;
        private SAPbouiCOM.EditText oCell;
        private MstLeaveType LeaveType;
        Boolean flgLeavesStatusChange, flgUserTrigger;

        #endregion

        #region Form B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillDedCodeCombo();
            FillEncashElementCombo();
            getData();
            AddBlankRow();
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
        
        #endregion

        #region Functions
        
        private void InitiallizeForm()
        {
            try 
            {
                dtMain = oForm.DataSources.DataTables.Add("LevType");
                dtMain.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("C", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("D", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                dtMain.Columns.Add("LevType", SAPbouiCOM.BoFieldsType.ft_Text, 20);                
                dtMain.Columns.Add("DedCode", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("PDedCod", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtMain.Columns.Add("AccCnt", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("cl_DinY", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("cl_Mnth", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtMain.Columns.Add("Encash", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                dtMain.Columns.Add("EncashElement", SAPbouiCOM.BoFieldsType.ft_Text, 50);
                dtMain.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                dtMain.Columns.Add("ConPro", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                dtMain.Columns.Add("LeaveCap", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtMain.Columns.Add("CarryFor", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                dtMain.Columns.Add("ProRate", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                //ConditionalProcessing
                dtMain.Columns.Add("cl_Def", SAPbouiCOM.BoFieldsType.ft_Text, 1);

                grdMain = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                grdMain.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)grdMain.Columns;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("LevType", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("LevType", "I");

                oColumn = oColumns.Item("cl_Code");
                clCode = oColumn;
                oColumn.DataBind.Bind("LevType", "C");

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("LevType", "D");

                oColumn = oColumns.Item("cl_LevType");
                clLevType = oColumn;
                oColumn.DataBind.Bind("LevType", "LevType");

                oColumn = oColumns.Item("cl_DedCode");
                clDedCode = oColumn;
                oColumn.DataBind.Bind("LevType", "DedCode");

                oColumn = oColumns.Item("cl_PDedCod");
                clPDedCod = oColumn;
                oColumn.DataBind.Bind("LevType", "PDedCod");
                clPDedCod.Visible = false;

                oColumn = oColumns.Item("cl_AccCnt");
                clAccural = oColumn;
                oColumn.DataBind.Bind("LevType", "AccCnt");
                clAccural.Visible = false;

                oColumn = oColumns.Item("cl_Encash");
                clEncash = oColumn;
                oColumn.DataBind.Bind("LevType", "Encash");

                oColumn = oColumns.Item("cl_elmnt");
                clEncashElement = oColumn;
                oColumn.DataBind.Bind("LevType", "EncashElement");

                oColumn = oColumns.Item("cl_Active");
                clActive = oColumn;
                oColumn.DataBind.Bind("LevType", "Active");
                //====
                oColumn = oColumns.Item("cl_Pro");
                clConDeduction = oColumn;
                oColumn.DataBind.Bind("LevType", "ConPro");

                oColumn = oColumns.Item("cl_Def");
                clDefault = oColumn;
                oColumn.DataBind.Bind("LevType", "cl_Def");
                clDefault.Visible = false;

                oColumn = oColumns.Item("cl_DinY");
                clDaysInYear = oColumn;
                oColumn.DataBind.Bind("LevType", "cl_DinY");
                clDaysInYear.Visible = false;

                oColumn = oColumns.Item("cl_Mnth");
                clMonth = oColumn;
                oColumn.DataBind.Bind("LevType", "cl_Mnth");
                clMonth.Visible = false;

                oColumn = oColumns.Item("cllcap");
                clLeaveCap = oColumn;
                oColumn.DataBind.Bind("LevType", "LeaveCap");

                oColumn = oColumns.Item("clcf");
                clCarryForward = oColumn;
                oColumn.DataBind.Bind("LevType", "CarryFor");

                oColumn = oColumns.Item("clpr");
                clProRate = oColumn;
                oColumn.DataBind.Bind("LevType", "ProRate");

                base.fillColumCombo("LevTyp_LevType", clLevType);
                clLevType.DisplayDesc = true;

                grdMain.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillDedCodeCombo()
        {
            try 
            {
                clDedCode.ValidValues.Add("-1","");
                clPDedCod.ValidValues.Add("-1", "");
                var Records = from v in dbHrPayroll.MstLeaveDeduction where v.DeductionStatus == true select v;
                foreach (var Record in Records)
                {
                    clDedCode.ValidValues.Add(Record.Code, Record.Description);
                    clPDedCod.ValidValues.Add(Record.Code, Record.Description);
                }
                clDedCode.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillEncashElementCombo()
        {
            try
            {
                clEncashElement.ValidValues.Add("-1", "");

                var Elements=dbHrPayroll.MstElements.Where(e=>e.ElmtType=="Ear").ToList();


                foreach (var Record in Elements)
                {
                    if (Record.MstElementEarning.ElementAt(0).FlgLeaveEncashment == true)
                    {
                        clEncashElement.ValidValues.Add(Record.ElementName, Record.Description);
                    }                    
                }
                clEncashElement.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void getData()
        {
            try
            {
                grdMain.Clear();
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLeaveType);
                var Data = from n in dbHrPayroll.MstLeaveType select n;
                dtMain.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    dtMain.Rows.Add(1);
                    dtMain.SetValue("No", i, i + 1);
                    dtMain.SetValue("I", i, v.ID);
                    dtMain.SetValue("C", i, v.Code);
                    dtMain.SetValue("D", i, v.Description);
                    dtMain.SetValue("LevType", i, v.LeaveType);
                    dtMain.SetValue("DedCode", i, v.DeductionCode == null ? "-1" :  v.DeductionCode);
                    dtMain.SetValue("PDedCod", i, v.PendingApprDedCode == null ? "-1" : v.PendingApprDedCode);
                    dtMain.SetValue("EncashElement", i, v.EncashElement == null ? "-1" : v.EncashElement);
                    dtMain.SetValue("cl_DinY", i, v.DaysinYear == null ? "0" : Convert.ToString(v.DaysinYear));
                    dtMain.SetValue("cl_Mnth", i, v.Months == null ? "0" : Convert.ToString(v.Months));
                    dtMain.SetValue("AccCnt", i, v.AccumulativeCount);
                    dtMain.SetValue("Encash", i, v.Encash == true ? "Y" : "N");
                    dtMain.SetValue("Active", i, v.Active == true ? "Y" : "N");
                    dtMain.SetValue("ConPro", i, v.FlgConditionalProcessing == true ? "Y" : "N");
                    dtMain.SetValue("LeaveCap", i, Convert.ToDouble(v.LeaveCap));
                    dtMain.SetValue("CarryFor", i, v.FlgCarryForward == true ? "Y" : "N");
                    dtMain.SetValue("ProRate", i, v.FlgProRate == true ? "Y" : "N");
                    if (v.FlgDefault != null)
                    {
                        dtMain.SetValue("cl_Def", i, v.FlgDefault == true ? "Y" : "N");
                    }
                    else
                    {
                        dtMain.SetValue("cl_Def", i, "N");
                    }
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
                (grdMain.Columns.Item(clAccural.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value = "0";
                (grdMain.Columns.Item(clMonth.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value = "0";
                (grdMain.Columns.Item(clDaysInYear.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value = "0";
                (grdMain.Columns.Item(clLeaveCap.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value = "0";
                (grdMain.Columns.Item(clCarryForward.UniqueID).Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.CheckBox).Checked = false;
                grdMain.FlushToDataSource();
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
                for (int i = 1; i <= grdMain.RowCount; i++)
                {
                    string code, desc, leavetype, dedcode, accural, daysinyear, month, elementencash, leavecap ="0";
                    int lineid;
                    Boolean flgEncash, flgActive, flgDefault, flgConditionalDeduction, flgProRate, flgCarryForward;
                    lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    code = (grdMain.Columns.Item(clCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    desc = (grdMain.Columns.Item(clDesc.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    leavetype = (grdMain.Columns.Item(clLevType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    dedcode = (grdMain.Columns.Item(clDedCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    accural = (grdMain.Columns.Item(clAccural.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    daysinyear = (grdMain.Columns.Item(clDaysInYear.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    month = (grdMain.Columns.Item(clMonth.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    elementencash = (grdMain.Columns.Item(clEncashElement.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    leavecap = (grdMain.Columns.Item(clLeaveCap.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    flgEncash = (grdMain.Columns.Item(clEncash.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgConditionalDeduction = (grdMain.Columns.Item(clConDeduction.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgDefault = (grdMain.Columns.Item(clDefault.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgProRate = (grdMain.Columns.Item(clProRate.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgCarryForward = (grdMain.Columns.Item(clCarryForward.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                    if (lineid == 0)
                    {
                        if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(desc)) continue;
                        MstLeaveType oNew = new MstLeaveType();
                        dbHrPayroll.MstLeaveType.InsertOnSubmit(oNew);
                        oNew.Code = code;
                        oNew.Description = desc;
                        oNew.LeaveType = leavetype;
                        oNew.LTLovType = "";
                        oNew.DeductionCode = dedcode;
                        oNew.PendingApprDedCode = "-1";
                        oNew.AccumulativeCount = Convert.ToInt32(accural);
                        oNew.Encash = flgEncash;
                        oNew.EncashElement = elementencash;
                        oNew.Active = flgActive;
                        oNew.FlgVL = false;
                        oNew.SubType = "";
                        oNew.FlgDefault = flgDefault;
                        oNew.Months = Convert.ToInt32(month);
                        oNew.DaysinYear = Convert.ToInt32(daysinyear);
                        oNew.FlgConditionalProcessing = flgConditionalDeduction;
                        oNew.LeaveCap = Convert.ToDecimal(leavecap);
                        oNew.FlgProRate = flgProRate;
                        oNew.FlgCarryForward = flgCarryForward;
                        oNew.CreateDate = DateTime.Now;
                        oNew.UserId = oCompany.UserName;
                        oNew.UpdateDate = DateTime.Now;
                        oNew.UpdateBy = oCompany.UserName;

                    }
                    else
                    {
                        MstLeaveType oDoc = (from a in dbHrPayroll.MstLeaveType where a.ID == lineid select a).FirstOrDefault();
                        if (oDoc == null) continue;
                        
                        oDoc.Description = desc;
                        oDoc.LeaveType = leavetype;
                        oDoc.LTLovType = "";
                        oDoc.DeductionCode = dedcode;
                        oDoc.PendingApprDedCode = "-1";
                        oDoc.AccumulativeCount = Convert.ToInt32(accural);
                        oDoc.Encash = flgEncash;
                        oDoc.EncashElement = elementencash;
                        if (flgLeavesStatusChange)
                        {
                            oDoc.Active = flgActive;
                        }
                        oDoc.FlgVL = false;
                        oDoc.SubType = "";
                        oDoc.FlgDefault = flgDefault;
                        oDoc.Months = Convert.ToInt32(month);
                        oDoc.DaysinYear = Convert.ToInt32(daysinyear);
                        oDoc.FlgConditionalProcessing = flgConditionalDeduction;
                        oDoc.LeaveCap = Convert.ToDecimal(leavecap);
                        oDoc.FlgProRate = flgProRate;
                        oDoc.FlgCarryForward = flgCarryForward;
                        oDoc.UpdateDate = DateTime.Now;
                        oDoc.UpdateBy = oCompany.UserName;
                    }
                }
                dbHrPayroll.SubmitChanges();
                getData();
                AddBlankRow();
            }
            catch (Exception ex)
            {
            }
        }

        private Boolean ValidateRecords()
        {
            try
            {
                for (int i = 1; i <= grdMain.RowCount; i++)
                {
                    string code, desc, leavetype, dedcode, accural, daysinyear, month, elementencash;
                    int lineid;
                    Boolean flgEncash, flgActive, flgDefault, flgConditionalDeduction;
                    lineid = Convert.ToInt32((grdMain.Columns.Item(clID.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    code = (grdMain.Columns.Item(clCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    desc = (grdMain.Columns.Item(clDesc.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    leavetype = (grdMain.Columns.Item(clLevType.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    dedcode = (grdMain.Columns.Item(clDedCode.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    accural = (grdMain.Columns.Item(clAccural.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    daysinyear = (grdMain.Columns.Item(clDaysInYear.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    month = (grdMain.Columns.Item(clMonth.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    elementencash = (grdMain.Columns.Item(clEncashElement.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    flgEncash = (grdMain.Columns.Item(clEncash.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgActive = (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgConditionalDeduction = (grdMain.Columns.Item(clConDeduction.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgDefault = (grdMain.Columns.Item(clDefault.UniqueID).Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                    if (lineid == 0)//new record
                    {
                        if (string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc))
                        {
                            oApplication.StatusBar.SetText("Leave code is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(code) && string.IsNullOrEmpty(desc))
                        {
                            oApplication.StatusBar.SetText("Leave description is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(leavetype))
                        {
                            if (leavetype == "Ded" && flgEncash)
                            {
                                oApplication.StatusBar.SetText("Leave encashment can't be deductable. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (leavetype == "Ded" && !string.IsNullOrEmpty(elementencash))
                            {
                                if (elementencash != "-1")
                                {
                                    oApplication.StatusBar.SetText("Leave encashment can't be deductable. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return false;
                                }
                            }
                            if (leavetype == "Ded" && (string.IsNullOrEmpty(dedcode) || dedcode == "-1"))
                            {
                                oApplication.StatusBar.SetText("Deduction code is mandatory for deductable leave types. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (leavetype == "NonDed" && !string.IsNullOrEmpty(dedcode))
                            {
                                if (dedcode != "-1")
                                {
                                    oApplication.StatusBar.SetText("Non deductable leaves doesn't need deduction code. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return false;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(elementencash))
                        {
                            if (elementencash == "-1" && flgEncash)
                            {
                                oApplication.StatusBar.SetText("Leave encashment element is mandatory if leave was encashment. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (elementencash != "-1" && !flgEncash)
                            {
                                oApplication.StatusBar.SetText("Leave encashment check is mandatory if leave was encashment. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                        }
                        var CountCheck = (from a in dbHrPayroll.MstLeaveType where a.Code == code select a).Count();
                        if (CountCheck > 0)
                        {
                            oApplication.StatusBar.SetText("Leave code already exist. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                    else //old record
                    {
                        if (string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc))
                        {
                            oApplication.StatusBar.SetText("Leave code is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(code) && string.IsNullOrEmpty(desc))
                        {
                            oApplication.StatusBar.SetText("Leave description is madatory. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (!string.IsNullOrEmpty(leavetype))
                        {
                            if (leavetype == "Ded" && flgEncash)
                            {
                                oApplication.StatusBar.SetText("Leave encashment can't be deductable. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (leavetype == "Ded" && !string.IsNullOrEmpty(elementencash) )
                            {
                                if (elementencash != "-1")
                                {
                                    oApplication.StatusBar.SetText("Leave encashment can't be deductable. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return false;
                                }
                            }
                            if (leavetype == "Ded" && (string.IsNullOrEmpty(dedcode) || dedcode == "-1"))
                            {
                                oApplication.StatusBar.SetText("Deduction code is mandatory for deductable leave types. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (leavetype == "NonDed" && !string.IsNullOrEmpty(dedcode))
                            {
                                if (dedcode != "-1")
                                {
                                    oApplication.StatusBar.SetText("Non deductable leaves doesn't need deduction code. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return false;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(elementencash))
                        {
                            if (elementencash == "-1" && flgEncash)
                            {
                                oApplication.StatusBar.SetText("Leave encashment element is mandatory if leave was encashment. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (elementencash != "-1" && !flgEncash)
                            {
                                oApplication.StatusBar.SetText("Leave encashment check is mandatory if leave was encashment. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                        }

                        var oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.ID == lineid select a).FirstOrDefault();
                        if (oLeaveType == null) continue;
                        if (oLeaveType.Code != code)
                        {
                            oApplication.StatusBar.SetText("You can't change leave code. at Line # " + i.ToString() , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (Convert.ToBoolean(oLeaveType.Active) != flgActive)
                        {
                            int confirm = oApplication.MessageBox("Are you sure you want to change leaves active status ? at line # " + i.ToString(), 2, "Yes", "No");
                            if (confirm == 1)
                            {
                                var oAssignLeaveCount = (from a in dbHrPayroll.MstEmployeeLeaves where a.MstLeaveType.Code == oLeaveType.Code select a).Count();
                                if (oAssignLeaveCount > 0)
                                {
                                    oApplication.StatusBar.SetText("You can't change leave active status as it was already assigned to employees. at Line # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    flgLeavesStatusChange = false;
                                    return false;
                                }
                                else
                                {
                                    flgLeavesStatusChange = true;
                                }
                            }
                            else
                            {
                                flgLeavesStatusChange = false;
                            }
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
        
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try 
            {
                string Code, Desc, LevType, DedCode,pDedCode,encashElem;
                int AccCnt, dayInYear, Months, LineID;
                bool Encash, Active, ConditionalProcessing, flgDefault;
                for (int i = 1; i < grdMain.RowCount; i++)
                {
                    LineID = Convert.ToInt32((grdMain.Columns.Item("cl_ID").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    Code = (grdMain.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Desc = (grdMain.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    LevType = (grdMain.Columns.Item("cl_LevType").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    DedCode = (grdMain.Columns.Item("cl_DedCode").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    encashElem = (grdMain.Columns.Item("cl_elmnt").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    pDedCode = (grdMain.Columns.Item("cl_PDedCod").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    AccCnt = int .Parse ((grdMain.Columns.Item("cl_AccCnt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    dayInYear = int.Parse((grdMain.Columns.Item("cl_DinY").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    Months = int.Parse((grdMain.Columns.Item("cl_Mnth").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                    Encash = (grdMain.Columns.Item("cl_Encash").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    Active = (grdMain.Columns.Item("cl_Active").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    ConditionalProcessing = (grdMain.Columns.Item("cl_Pro").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    flgDefault = (grdMain.Columns.Item("cl_Def").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;

                    if (Code.Equals(dtMain.GetValue("C", i - 1)) && Desc.Equals(dtMain.GetValue("D", i - 1)) && encashElem.Equals(dtMain.GetValue("EncashElement", i - 1)) &&
                        LevType.Equals(dtMain.GetValue("LevType", i - 1)) && DedCode.Equals(dtMain.GetValue("DedCode", i - 1)) && pDedCode.Equals(dtMain.GetValue("PDedCod", i - 1)) && 
                        AccCnt == dtMain .GetValue ("AccCnt",i-1) && (Encash == true ? "Y": "N") == dtMain .GetValue ("Encash",i-1) &&
                        (Active == true ? "Y" : "N") == dtMain.GetValue("Active", i - 1) && (ConditionalProcessing == true ? "Y" : "N") == dtMain.GetValue("ConPro", i - 1) && Months == dtMain.GetValue("cl_Mnth", i - 1))
                    {
                        continue ;
                    }
                    else if (Code.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Desc.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }
                    else if (!Code.Equals("") && LevType.Trim() == "Ded" && encashElem.Trim() == "LVENC")
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLeaveEnc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }
                    else if (!Code.Equals("") && LevType.Trim() == "Ded" && Encash == true)
                    {
                        oApplication.StatusBar.SetText("Deductable leave Type could not checked as leave encashment.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }
                    else if (!Code.Equals("") && LevType.Trim() == "Ded" && DedCode == "-1")
                    {
                        oApplication.StatusBar.SetText("Deductable leave type must have deduction code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }
                    else if (!Code.Equals("") && encashElem.Trim() == "LVENC" && Encash == false)
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullEncashActive"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }

                    else if (!LevType.Equals("") && LevType.Trim() == "NonDed" && DedCode != "-1")
                    {
                        oApplication.StatusBar.SetText("Deduction not applicable on non deductable leaves", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                        return;
                    }
                    var EmpLeaves = dbHrPayroll.MstLeaveType.Where(e => e.ID == LineID).FirstOrDefault();
                    if (EmpLeaves != null)
                    {
                        if (EmpLeaves.Code != Code)
                        {
                            oApplication.StatusBar.SetText("You can not change leave type code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            BubbleEvent = false;
                            return;
                        }
                        if (EmpLeaves.Active != Active)
                        {
                            
                            int confirm = oApplication.MessageBox("Are you sure you want to Active / deactive leave? ", 3, "Yes", "No", "Cancel");
                            if (confirm == 2 || confirm == 3)
                            {
                                BubbleEvent = false;
                                return;
                            }
                            else
                            {
                                var leaveTye = (from a in dbHrPayroll.MstLeaveType where a.Code == Code select a).FirstOrDefault();
                                var LeaveTypeInUsed = dbHrPayroll.MstEmployeeLeaves.Where(l => l.LeaveType == leaveTye.ID).ToList();
                                if (LeaveTypeInUsed != null && LeaveTypeInUsed.Count > 0 && Active == false)
                                {
                                    //oApplication.MessageBox("System can not inactive '" + Desc + "' because it is attached with employee(s). Line @" + (i+1).ToString());

                                    oApplication.StatusBar.SetText("System can not inactive '" + Desc + "' because it is attached with employee(s).Line @" + (i).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }

                                var LeaveTypeInUsed2 = dbHrPayroll.MstEmployeeLeaves.Where(l => l.LeaveType == leaveTye.ID).ToList();
                                if (LeaveTypeInUsed2 != null && LeaveTypeInUsed2.Count > 0 && Active == true)
                                {
                                    oApplication.StatusBar.SetText("System can not change '" + Desc + "' because it is attached with employee(s).Line @" + (i).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                UpdateRecord(Code, Desc, LevType, DedCode, pDedCode, encashElem, AccCnt, Encash, Active, ConditionalProcessing, (int)dtMain.GetValue("I", i - 1), flgDefault, dayInYear, Months);
                            }
                        }

                        else
                        {
                            UpdateRecord(Code, Desc, LevType, DedCode, pDedCode, encashElem, AccCnt, Encash, Active, ConditionalProcessing, (int)dtMain.GetValue("I", i - 1), flgDefault, dayInYear, Months);
                        }
                    }
                    else
                    {
                        //oApplication.StatusBar.SetText("Entered code not exist in database.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                }
                Code = (grdMain.Columns.Item("cl_Code").Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value;
                Desc = (grdMain.Columns.Item("cl_Desc").Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.EditText).Value;
                LevType = (grdMain.Columns.Item("cl_LevType").Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                DedCode = (grdMain.Columns.Item("cl_DedCode").Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                pDedCode = (grdMain.Columns.Item("cl_PDedCod").Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                encashElem = (grdMain.Columns.Item("cl_elmnt").Cells.Item(grdMain.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                if (Code.Equals("") && Desc.Equals(""))
                {
                    return;
                }
                if (Code.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLeaveCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    BubbleEvent = false;
                    return;
                }
                //if (LeaveType.Equals(""))
                //{
                //    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLeaveType"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    BubbleEvent = false;
                //    return;
                //}
                else if (!Code.Equals("") && Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && LevType.Equals(""))
                {   
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLevType"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && DedCode.Equals("") && LevType.Trim() == "Ded")
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDedCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && LevType.Trim() == "Ded" && encashElem.Trim() == "LVENC")
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLeaveEnc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    grdMain.FlushToDataSource();
                    AddRowToDataBase(dtMain.GetValue("C", grdMain.RowCount - 1), dtMain.GetValue("D", grdMain.RowCount - 1), dtMain.GetValue("LevType", grdMain.RowCount - 1),
                    dtMain.GetValue("DedCode", grdMain.RowCount - 1), dtMain.GetValue("EncashElement", grdMain.RowCount - 1), dtMain.GetValue("AccCnt", grdMain.RowCount - 1), dtMain.GetValue("Encash", grdMain.RowCount - 1), dtMain.GetValue("Active", grdMain.RowCount - 1), dtMain.GetValue("ConPro", grdMain.RowCount - 1), dtMain.GetValue("cl_Def", grdMain.RowCount - 1), dtMain.GetValue("cl_DinY", grdMain.RowCount - 1), dtMain.GetValue("cl_Mnth", grdMain.RowCount - 1));
                    getData();
                    AddBlankRow();
                    oForm.Freeze(false);
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }
        
        private void UpdateRecord(string Code, string Desc, string LevType, string DedCode, string pDedCode, string elemcode, int AccCnt, bool Encash, bool Active,bool ConditionalProcessing, int ID, bool flgDefault, int intDinY, int intMnth)
        {
            try 
            {
                //var LeaveTypeInUsed = dbHrPayroll.MstEmployeeLeaves.Where(l => l.LeaveType == ID).ToList();
                //if (LeaveTypeInUsed != null && LeaveTypeInUsed.Count > 0 && Active == false)
                //{
                //    oApplication.MessageBox("System can not inactive '" + Desc + "' because it is attached with employee(s).");

                //    oApplication.StatusBar.SetText("System can not inactive '" + Desc + "' because it is attached with employee(s).", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return;
                //}

                //var LeaveTypeInUsed2 = dbHrPayroll.MstEmployeeLeaves.Where(l => l.LeaveType == ID).ToList();
                //if (LeaveTypeInUsed2 != null && LeaveTypeInUsed2.Count > 0 && Active == true)
                //{
                //    oApplication.StatusBar.SetText("System can not change '" + Desc + "' because it is attached with employee(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return;
                //}

                LeaveType = (from v in dbHrPayroll.MstLeaveType where v.ID == ID select v).FirstOrDefault();
                LeaveType.Code = Code;
                LeaveType.EncashElement = string.IsNullOrEmpty(elemcode) ? "" : elemcode;
                LeaveType.Description = Desc;
                LeaveType.LeaveType = LevType;
                LeaveType.DeductionCode = DedCode;
                LeaveType.AccumulativeCount = AccCnt;
                LeaveType.Encash = Encash;
                LeaveType.Active = Active;
                LeaveType.FlgConditionalProcessing = ConditionalProcessing;
                LeaveType.DaysinYear = intDinY;
                LeaveType.Months = intMnth;
                LeaveType.FlgDefault = flgDefault;
                LeaveType.PendingApprDedCode = pDedCode;
                LeaveType.UpdateDate = DateTime.Now;
                LeaveType.UpdateBy = oCompany.UserName;
                //LeaveType.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void AddRowToDataBase(string Code, string Desc, string LevType, string DedCode, string elemcode, int AccCnt, string Encash, string Active,string ConditionalProcessing, string flgDefault, int intDinY, int intMnth)
        {
            try
            {
                LeaveType = new MstLeaveType();
                LeaveType.Code = Code;
                LeaveType.Description = Desc;
                LeaveType.LeaveType = LevType;
                LeaveType.DeductionCode = DedCode;
                LeaveType.AccumulativeCount = AccCnt;
                LeaveType.DaysinYear = intDinY;
                LeaveType.Months = intMnth;
                LeaveType.EncashElement = string.IsNullOrEmpty(elemcode) ? "" : elemcode;
                LeaveType.Encash = (Encash == "Y" ? true : false);
                LeaveType.Active = (Active == "Y" ? true : false);
                LeaveType.FlgConditionalProcessing = (ConditionalProcessing == "Y" ? true : false);
                LeaveType.FlgDefault = (flgDefault == "Y" ? true : false);
                LeaveType.CreateDate = DateTime.Now;
                LeaveType.UserId = oCompany.UserName;
                //LeaveType.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.MstLeaveType.InsertOnSubmit(LeaveType);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            } 
        }
        
        private void DeleteRecord(int Row)
        {
            try
            {
                oForm.Freeze(true);
                int ID = dtMain.GetValue("I", Row - 1);
                MstLeaveType Record = (from v in dbHrPayroll.MstLeaveType where v.ID == ID select v).Single();
                dbHrPayroll.MstLeaveType.DeleteOnSubmit(Record);
                dbHrPayroll.SubmitChanges();
                dtMain.Rows.Remove(Row - 1);
                grdMain.Clear();
                getData();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }      
        
        private bool CheckDuplicateRecords(string ColName)
        {
            bool result = false;
            try
            {
                string value = "";
                for (int i = 1; i <= grdMain.RowCount; i++)
                {
                    oCell = grdMain.Columns.Item(ColName).Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    value = oCell.Value;
                    for (int j = 1; j <= grdMain.RowCount; j++)
                    {
                        if (i == j)
                        { continue; }
                        else if (value.ToLower().Equals((grdMain.Columns.Item(ColName).Cells.Item(j).Specific as SAPbouiCOM.EditText).Value.ToLower()))
                        {
                            return (result = true);
                        }
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return (result = true);
            }
        }
     
        #endregion
    }
}
