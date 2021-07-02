using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;

namespace ACHR.Screen
{
    class frm_HolDates:HRMSBaseForm 
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.UserDataSource oUDS_FromWeek, oUDS_ToWeek;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clIsNew, clS_Date, clE_Date, clRem;
        private SAPbouiCOM.EditText txHoliday;
        private SAPbouiCOM.OptionBtn objan1, ob4dweek, obffweek;
        private SAPbouiCOM.ComboBox cbfrmweek, cbtoweek;
        private SAPbouiCOM.CheckBox ck1yr, ckwk_as_w;
        private SAPbouiCOM.Item ItxHoliday, Iobjan1, Iob4dweek, Iobffweek, Icbfrmweek, Icbtoweek, Ich1yr, Ickwk_as_w;
        private int CurrentHldId = 0;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            AddBlankRow();
            oForm.Freeze(false);
        }
        private void InitiallizeForm()
        {
            try 
            {
                oForm.DataSources.UserDataSources.Add("txHoliday", SAPbouiCOM.BoDataType.dt_SHORT_TEXT ,20); // Holidays
                txHoliday = oForm.Items.Item("t_Holiday").Specific;
                ItxHoliday = oForm.Items.Item("t_Holiday");
                txHoliday.DataBind.SetBound(true, "", "txHoliday");

                oForm.DataSources.UserDataSources.Add("WeekNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT , 1); // Jan 1 Option Button
                objan1  = oForm.Items.Item("r_jan1").Specific;
                Iobjan1 = oForm.Items.Item("r_jan1");
                objan1.DataBind.SetBound(true, "", "WeekNum");

                ob4dweek = oForm.Items.Item("r_4dweek").Specific; // 4-Day Week Oprion Button
                ob4dweek.GroupWith("r_jan1");
                Iob4dweek = oForm.Items.Item("r_4dweek");
                ob4dweek.DataBind.SetBound(true, "", "WeekNum");

                obffweek = oForm.Items.Item("r_ffweek").Specific; // Full Week Option Button
                obffweek.GroupWith("r_jan1");
                Iobffweek = oForm.Items.Item("r_ffweek");
                obffweek.DataBind.SetBound(true, "", "WeekNum");

                oForm.DataSources.UserDataSources.Add("frmweek", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,10); //From Week Combo Box
                oUDS_FromWeek = oForm.DataSources.UserDataSources.Item("frmweek");
                cbfrmweek = oForm.Items.Item("cb_frmweek").Specific;
                Icbfrmweek = oForm.Items.Item("cb_frmweek");
                cbfrmweek.DataBind.SetBound(true, "", "frmweek");

                oForm.DataSources.UserDataSources.Add("toweek", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,10); //To Week ComboBox
                oUDS_ToWeek = oForm.DataSources.UserDataSources.Item("toweek");
                cbtoweek = oForm.Items.Item("cb_toweek").Specific;
                Icbtoweek = oForm.Items.Item("cb_toweek");
                cbtoweek.DataBind.SetBound(true, "", "toweek");

                oForm.DataSources.UserDataSources.Add("1yr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT ,1); //Valid For One Year Check Box
                ck1yr = oForm.Items.Item("ch_1yr").Specific;
                Ich1yr = oForm.Items.Item("ch_1yr");
                ck1yr.DataBind.SetBound(true, "", "1yr");

                oForm.DataSources.UserDataSources.Add("wk_as_w", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); //Set Weekend as WorkDays Check Box
                ckwk_as_w = oForm.Items.Item("ch_wk_as_w").Specific;
                Ickwk_as_w = oForm.Items.Item("ch_wk_as_w");
                ckwk_as_w.DataBind.SetBound(true, "", "wk_as_w");

                oDBDataTable = oForm.DataSources.DataTables.Add("HolDetails");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("IsNew", SAPbouiCOM.BoFieldsType.ft_Text, 1);
                oDBDataTable.Columns.Add("StDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("EdDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("Rem", SAPbouiCOM.BoFieldsType.ft_Text, 200);
                
                oMat = (SAPbouiCOM .Matrix)oForm.Items.Item("Mat").Specific;
                oMat.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = oMat.Columns;

                oColumn = oColumns.Item("cl_No");
                clNo = oColumn;
                oColumn.DataBind.Bind("HolDetails", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("HolDetails", "I");

                oColumn = oColumns.Item("cl_IsNew");
                clIsNew = oColumn;
                oColumn.DataBind.Bind("HolDetails", "IsNew");

                oColumn = oColumns.Item("cl_S_Date");
                clS_Date = oColumn;
                oColumn.DataBind.Bind("HolDetails", "StDate");

                oColumn = oColumns.Item("cl_E_Date");
                clE_Date = oColumn;
                oColumn.DataBind.Bind("HolDetails", "EdDate");

                oColumn = oColumns.Item("cl_Rem");
                clRem = oColumn;
                oColumn.DataBind.Bind("HolDetails", "Rem");

                base.fillCombo("HolDate_Days", cbfrmweek); Icbfrmweek.DisplayDesc = true;
                base.fillCombo("HolDate_Days", cbtoweek); Icbtoweek.DisplayDesc = true;

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                objan1.Selected = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetDataFromDataSource()
        {
            try
            {
                MstHolidays HolHead = (from H in dbHrPayroll.MstHolidays select H).First();

                CurrentHldId = HolHead.Id;
                txHoliday.Value = HolHead.Holiday;
                switch (HolHead.WeekNumbering)
                {
                    case "1": objan1.Selected = true; break;
                    case "2": ob4dweek.Selected = true; break;
                    case "3": obffweek.Selected = true; break;
                }

                oUDS_FromWeek.Value = HolHead.WeekendFrom;
                oUDS_ToWeek.Value = HolHead.WeekendTo;

                ckwk_as_w.Checked = (bool)HolHead.WeekendAtWork;
                ck1yr.Checked = (bool)HolHead.Validity;

                var Detail = from v in HolHead.MstHolidayDetail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Detail)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("StDate", i, v.StartDate);
                    oDBDataTable.SetValue("EdDate", i, v.EndDate);
                    oDBDataTable.SetValue("Rem", i, v.Remarks == null ? "" : v.Remarks);
                    i += 1;
                }
                oMat.LoadFromDataSource();
                //ItxHoliday.Enabled = false;
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;

            }
            catch (Exception ex)
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                //oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetDataFromDataSource(MstHolidays HolHead)
        {
            try
            {
                CurrentHldId = HolHead.Id;
                txHoliday.Value = HolHead.Holiday;
                switch (HolHead.WeekNumbering)
                {
                    case "1": objan1.Selected = true; break;
                    case "2": ob4dweek.Selected = true; break;
                    case "3": obffweek.Selected = true; break;
                }

                oUDS_FromWeek.Value = HolHead.WeekendFrom;
                oUDS_ToWeek.Value = HolHead.WeekendTo;

                ckwk_as_w.Checked = (bool)HolHead.WeekendAtWork;
                ck1yr.Checked = (bool)HolHead.Validity;

                var Detail = from v in HolHead.MstHolidayDetail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Detail)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("StDate", i, v.StartDate);
                    oDBDataTable.SetValue("EdDate", i, v.EndDate);
                    oDBDataTable.SetValue("Rem", i, v.Remarks == null ? "" : v.Remarks);
                    i += 1;
                }
                oMat.LoadFromDataSource();
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
                oDBDataTable.Rows.Clear();
                oMat.AddRow(1, oMat.RowCount + 1);
                oMat.FlushToDataSource();
                (oMat.Columns.Item("cl_No").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
                (oMat.Columns.Item("cl_IsNew").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = "Y";
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
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
                            if (pVal .ColUID .Equals ("cl_S_Date") || pVal .ColUID .Equals ("cl_E_Date"))
                            {
                                string StartDate = ((oMat.Columns.Item("cl_S_Date").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value).ToString ();
                                string EndDate = ((oMat.Columns.Item("cl_E_Date").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value).ToString();
                                for (int i = 1; i <= oMat.RowCount; i++)
                                {
                                    if (i == pVal.Row)
                                        continue;
                                    else if (StartDate.Equals(((oMat.Columns.Item("cl_S_Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value).ToString())
                                        && EndDate.Equals(((oMat.Columns.Item("cl_E_Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value).ToString ()))
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistDtRange"));
                                        BubbleEvent = false;
                                        return;
                                    }
                                    
                                }
                            }    
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            try 
            {
                switch (pVal.ItemUID)
                {
                    case "Mat":
                        {
                            if ((pVal.ColUID.Equals("cl_S_Date") || pVal.ColUID.Equals("cl_E_Date"))&& pVal.Row == oMat .RowCount)
                            {
                                string StartDate = ((oMat.Columns.Item("cl_S_Date").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value).ToString();
                                string EndDate = ((oMat.Columns.Item("cl_E_Date").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value).ToString();
                                if (!StartDate.Equals("") && !EndDate.Equals(""))
                                {
                                    AddBlankRow();
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            switch (pVal .ItemUID)
            {
                case "1":
                    if (oForm.Mode.Equals(SAPbouiCOM.BoFormMode.fm_OK_MODE))
                        return;
                    else 
                        ValidateAndSave(ref pVal, out BubbleEvent);
                    break;
                case "btn_first":
                    getFirstRecord();
                    break;
                case "btn_prev":
                    getPreviouRecord();
                    break;
                case "btn_next":
                    getNextRecord();
                    break;
                case "btn_last":
                    getLastRecord();
                    break;
                case "btn_new":
                    ChangeFormToAddMode();
                    break;
                case "btn_del":
                    {
                        int Count = 0;
                        for (int i = 1; i < oMat.RowCount; i++)
                        {
                            if (oMat.IsRowSelected(i))
                            {
                                int Res = oApplication.MessageBox(Program.objHrmsUI.getStrMsg("War_DelRow"), 1, "Yes", "No", "");
                                if (Res == 1)
                                {
                                    DeleteRecord(i);
                                }
                                else
                                {
                                    oMat.SelectRow(i, false, false);
                                }
                                Count += 1;
                            }

                        }
                        if (Count == 0)
                        {
                            oApplication.MessageBox(Program.objHrmsUI.getStrMsg("War_SelectRow"), 1, "OK", "", "");
                        }
                    }
                    break;
            }
        }
        public override void getFirstRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.MstHolidays select v).Count();
                if (Count == 0)
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                else
                {
                    oForm.Freeze(true);
                    MstHolidays HolHead = (from v in dbHrPayroll.MstHolidays orderby v.Id ascending select v).First();
                    GetDataFromDataSource(HolHead);
                    AddBlankRow();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void getPreviouRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.MstHolidays where v.Id < CurrentHldId select v).Count();
                if (Count == 0)
                {
                    getFirstRecord();
                } 
                else
                {
                    oForm.Freeze(true);
                    MstHolidays HolHead = (from v in dbHrPayroll.MstHolidays where v.Id < CurrentHldId orderby v.Id descending select v).First();
                    GetDataFromDataSource(HolHead);
                    AddBlankRow();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void getNextRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.MstHolidays where v.Id > CurrentHldId orderby v.Id ascending select v).Count();
                if (Count == 0)
                {
                    getLastRecord();
                }
                else
                {
                    oForm.Freeze(true);
                    MstHolidays HolHead = (from v in dbHrPayroll.MstHolidays where v.Id > CurrentHldId orderby v.Id ascending select v).First();
                    GetDataFromDataSource(HolHead);
                    AddBlankRow();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void getLastRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.MstHolidays select v).Count();
                if (Count == 0)
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                else
                {
                    oForm.Freeze(true);
                    MstHolidays HolHead = (from v in dbHrPayroll.MstHolidays orderby v.Id descending select v).First();
                    GetDataFromDataSource(HolHead);
                    AddBlankRow();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void ChangeFormToAddMode()
        {
            try
            {
                oForm.Freeze(true);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                CurrentHldId = 0;
                objan1.Selected = true;
                txHoliday.Value = "";
                oUDS_FromWeek.Value = "";
                oUDS_ToWeek.Value = "";
                ckwk_as_w.Checked = false;
                ck1yr.Checked = false;
                oMat.Clear();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void ValidateAndSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (txHoliday.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullHol"));
                    BubbleEvent = false;
                    return;
                }
                else if (cbfrmweek.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullFrmWeek"));
                    BubbleEvent = false;
                    return;
                }
                else if (cbtoweek.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullToWeek"));
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    string StartDate = "";
                    string EndDate = "";
                    for (int i = 1; i <= oMat.RowCount; i++)
                    {
                        StartDate = ((oMat.Columns.Item("cl_S_Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value).ToString();
                        EndDate = ((oMat.Columns.Item("cl_E_Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value).ToString();

                        if (oMat.RowCount == i)
                        {
                            if ((StartDate.Equals("") && !EndDate.Equals("")) || (!StartDate.Equals("") && EndDate.Equals("")) || oMat .RowCount == 1)
                            {
                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDate"));
                                BubbleEvent = false;
                                return;
                            }
                        }
                        else if (StartDate.Equals("") || EndDate.Equals(""))
                        {
                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDate"));
                            BubbleEvent = false;
                            return;
                        }
                    }
                    switch (oForm .Mode)
                    {
                        case SAPbouiCOM.BoFormMode.fm_ADD_MODE :
                            AddRecordToDatabase();
                            ChangeFormToAddMode();
                            break;
                        case SAPbouiCOM.BoFormMode.fm_UPDATE_MODE :
                            UpdateRecords();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }
        private void AddRecordToDatabase()
        {
            try
            {
                string WeekNumbering = "", Remarks;
                DateTime StartDate, EndDate;

                if (objan1.Selected == true)
                {
                    WeekNumbering = "1";
                }
                else if (ob4dweek.Selected == true)
                {
                    WeekNumbering = "2";
                }
                else if (obffweek.Selected == true)
                {
                    WeekNumbering = "3";
                }
                MstHolidays HolHead = new MstHolidays();
                HolHead.Holiday = txHoliday.Value;
                HolHead.WeekNumbering = WeekNumbering;
                HolHead.WeekendFrom = cbfrmweek.Value;
                HolHead.WeekendTo = cbtoweek.Value;
                HolHead.Validity = ck1yr.Checked;
                HolHead.WeekendAtWork = ckwk_as_w.Checked;

                oMat.FlushToDataSource();
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    StartDate = oDBDataTable.GetValue("StDate", i);
                    EndDate = oDBDataTable.GetValue("EdDate", i);
                    Remarks = oDBDataTable.GetValue("Rem", i);

                    MstHolidayDetail HolDetail = new MstHolidayDetail();
                    HolDetail.StartDate = StartDate;
                    HolDetail.EndDate = EndDate;
                    HolDetail.Remarks = Remarks;
                    HolDetail.CreateDate = DateTime.Now;
                    HolHead.MstHolidayDetail.Add(HolDetail);
                }

                dbHrPayroll.MstHolidays.InsertOnSubmit(HolHead);
                dbHrPayroll.SubmitChanges();
                CurrentHldId = HolHead.Id;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateRecords()
        {
            try
            {
                string WeekNumbering = "", Remarks,IsNew,ID;
                DateTime StartDate, EndDate;

                if (objan1.Selected == true)
                {
                    WeekNumbering = "1";
                }
                else if (ob4dweek.Selected == true)
                {
                    WeekNumbering = "2";
                }
                else if (obffweek.Selected == true)
                {
                    WeekNumbering = "3";
                }
                MstHolidays HolHead = (from v in dbHrPayroll.MstHolidays where v.Id == CurrentHldId select v).Single();
                HolHead.Holiday = txHoliday.Value;
                HolHead.WeekNumbering = WeekNumbering;
                HolHead.WeekendFrom = cbfrmweek.Value;
                HolHead.WeekendTo = cbtoweek.Value;
                HolHead.Validity = ck1yr.Checked;
                HolHead.WeekendAtWork = ckwk_as_w.Checked;

                for (int i = 1; i < oMat.RowCount; i++)
                {
                    ID = (oMat.Columns.Item("cl_ID").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    IsNew = (oMat.Columns.Item("cl_IsNew").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    StartDate = DateTime.ParseExact((oMat.Columns.Item("cl_S_Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value, "yyyyMMdd", CultureInfo.InvariantCulture);
                    EndDate = DateTime.ParseExact((oMat.Columns.Item("cl_E_Date").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value, "yyyyMMdd", CultureInfo.InvariantCulture);
                    Remarks = (oMat.Columns.Item("cl_Rem").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (IsNew == "N")
                    {
                        MstHolidayDetail HolDetail = (from v in HolHead.MstHolidayDetail where v.Id == int.Parse(ID) select v).Single();
                        HolDetail.StartDate = StartDate;
                        HolDetail.EndDate = EndDate;
                        HolDetail.Remarks = Remarks;
                        HolDetail.CreateDate = DateTime.Now;
                    }
                    else
                    {
                        MstHolidayDetail HolDetail = new MstHolidayDetail();
                        HolDetail.StartDate = StartDate;
                        HolDetail.EndDate = EndDate;
                        HolDetail.Remarks = Remarks;
                        HolDetail.CreateDate = DateTime.Now;
                        HolHead.MstHolidayDetail.Add(HolDetail);
                        (oMat.Columns.Item("cl_IsNew").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = "N";
                    }
                }
                dbHrPayroll.SubmitChanges();
                GetDataFromDataSource(HolHead);
                AddBlankRow();
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
                int ID = oDBDataTable.GetValue("I", Row - 1);
                MstHolidayDetail Record = (from v in dbHrPayroll.MstHolidayDetail where v.Id == ID select v).Single();
                dbHrPayroll.MstHolidayDetail.DeleteOnSubmit(Record);
                dbHrPayroll.SubmitChanges();
                oDBDataTable.Rows.Remove(Row - 1);
                oMat.Clear();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }
}
