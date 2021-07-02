using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;

namespace ACHR.Screen
{
    class frm_VacStatus:HRMSBaseForm 
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn,clNo ,clId, clCode, clDesc, clStDate, clEndDate, clInit, clStatus;
        private SAPbouiCOM.EditText oCell;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillVacancyCombo();
            GetDataFormDataSource();
            AddBlankRow();
            oForm.Freeze(false);
        }
        private void InitiallizeForm()
        {
            try 
            {
                oDBDataTable = oForm.DataSources.DataTables.Add("VacStatus");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("C", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                oDBDataTable.Columns.Add("D", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                //oDBDataTable.Columns.Add("T", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                oDBDataTable.Columns.Add("StDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("EndDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("Init", SAPbouiCOM.BoFieldsType.ft_Text, 50);
                oDBDataTable.Columns.Add("Status", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                oDBDataTable.Rows.Clear();

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oMat.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("VacStatus", "No");
                
                oColumn = oColumns.Item("cl_ID");
                clId = oColumn;
                oColumn.DataBind.Bind("VacStatus", "I");

                oColumn = oColumns.Item("cl_Code");
                clCode = oColumn;
                oColumn.DataBind.Bind("VacStatus", "C");

                oColumn = oColumns.Item("cl_Vncy");
                clDesc = oColumn;
                oColumn.DataBind.Bind("VacStatus", "D");

                oColumn = oColumns.Item("cl_St_Dt");
                clStDate = oColumn;
                oColumn.DataBind.Bind("VacStatus", "StDate");

                oColumn = oColumns.Item("cl_End_Dt");
                clEndDate = oColumn;
                oColumn.DataBind.Bind("VacStatus", "EndDate");

                oColumn = oColumns.Item("cl_Init");
                clInit = oColumn;
                oColumn.DataBind.Bind("VacStatus", "Init");
                clInit.Editable = false;

                oColumn = oColumns.Item("cl_Status");
                clStatus = oColumn;
                oColumn.DataBind.Bind("VacStatus", "Status");

                base.fillColumCombo("VacStatus_Status", clStatus);
                clStatus.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillVacancyCombo()
        {
            try
            {
                var Records = from v in dbHrPayroll.MstVacancyTypes where v.StatusVacancy == true select v;
                foreach (var Record in Records)
                {
                    clDesc.ValidValues.Add(Record.Code, Record.Description);
                }
                clDesc.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetDataFormDataSource()
        {
            try
            {
                oMat.Clear();
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsVacancyStatus);
                var Data = from n in dbHrPayroll.TrnsVacancyStatus select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("C", i, v.Code.ToString());
                    oDBDataTable.SetValue("D", i, v.Description);
                    oDBDataTable.SetValue("StDate", i, v.StartDate);
                    oDBDataTable.SetValue("EndDate", i, v.EndDate);
                    oDBDataTable.SetValue("Init", i, v.InitiatedBy);
                    oDBDataTable.SetValue("Status", i, v.StatusVacancy);
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
                (oMat.Columns.Item("cl_no").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
                (oMat.Columns.Item("cl_Init").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = oCompany.UserName;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                        return;
                    else
                        ValidateandSave(ref pVal, out BubbleEvent);
                    break;
                case "btn_del":
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
                    break;
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
                            
                            switch (pVal.ColUID)
                            {
                                case "cl_St_Dt":
                                case "cl_End_Dt":
                                    {
                                        string StartDate = (oMat.Columns.Item("cl_St_Dt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value,
                                            EndDate = (oMat.Columns.Item("cl_End_Dt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                        if (StartDate.Equals("") || EndDate.Equals(""))
                                            return;
                                        else if (int.Parse(EndDate) < int.Parse(StartDate))
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DateComparison"), SAPbouiCOM.BoMessageTime.bmt_Short);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;
                                case "cl_Code":
                                    {
                                        var Code = (oMat.Columns.Item("cl_Code").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                        if (Code.Equals("") && pVal.Row != oMat.RowCount)
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }

                                        for (int i = 1; i <= oMat.RowCount; i++)
                                        {
                                            oCell = oMat.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                            if (i == pVal.Row)
                                                continue;
                                            else if (Code == oCell.Value.Trim().ToLower())
                                            {
                                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                                BubbleEvent = false;
                                                return;
                                            }
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
            }
                
        }
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try 
            {
                string Code, Desc, InitiatedBy, Status, StartDate, EndDate;
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    Code = (oMat.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Desc = (oMat.Columns.Item("cl_Vncy").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    StartDate = (oMat.Columns.Item("cl_St_Dt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value.ToString();
                    EndDate = (oMat.Columns.Item("cl_End_Dt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value.ToString();
                    InitiatedBy = (oMat.Columns.Item("cl_Init").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Status = (oMat.Columns.Item("cl_Status").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (Code.Equals(oDBDataTable.GetValue("C", i - 1)) && Desc.Equals(oDBDataTable.GetValue("D", i - 1))
                         && StartDate == oDBDataTable.GetValue("StDate", i - 1).ToString("yyyyMMdd") && EndDate == oDBDataTable.GetValue("EndDate", i - 1).ToString("yyyyMMdd")
                         && InitiatedBy.Equals(oDBDataTable.GetValue("Init", i - 1)) && Status.Equals(oDBDataTable.GetValue("Status", i - 1)))
                    {
                        continue ;
                    }
                    else if (Code.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Desc.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (StartDate.Equals("") || EndDate.Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (InitiatedBy.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Status.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullStatus"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else
                    {
                        UpdateRecord(Code, Desc, StartDate, EndDate, InitiatedBy, Status, oDBDataTable.GetValue("I", i - 1));
                    }

                }
                Code = (oMat.Columns.Item("cl_Code").Cells.Item(oMat .RowCount).Specific as SAPbouiCOM.EditText).Value;
                Desc = (oMat.Columns.Item("cl_Vncy").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                StartDate = (oMat.Columns.Item("cl_St_Dt").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value.ToString();
                EndDate = (oMat.Columns.Item("cl_End_Dt").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value.ToString();
                InitiatedBy = (oMat.Columns.Item("cl_Init").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                Status = (oMat.Columns.Item("cl_Status").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                if (Code.Equals("") && Desc.Equals(""))
                {
                    return;
                }
                else if (Code.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (StartDate.Equals("") || EndDate.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (InitiatedBy.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (Status.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullStatus"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDatabase(oDBDataTable.GetValue("C", oMat.RowCount - 1), oDBDataTable.GetValue("D", oMat.RowCount - 1), oDBDataTable.GetValue("StDate", oMat.RowCount - 1),
                        oDBDataTable.GetValue("EndDate", oMat.RowCount - 1), oDBDataTable.GetValue("Init", oMat.RowCount - 1), oDBDataTable.GetValue("Status", oMat.RowCount - 1));
                    GetDataFormDataSource();
                    AddBlankRow();
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateRecord(string Code, string Desc, string StartDate, string EndDate, string IntitatedBy, string Status, int ID)
        {
            try
            {
                TrnsVacancyStatus VacancyStatus = (from v in dbHrPayroll.TrnsVacancyStatus where v.Id == ID select v).Single();
                VacancyStatus.Code = Code;
                VacancyStatus.Description = Desc;
                VacancyStatus.StartDate = DateTime.ParseExact(StartDate, "yyyymmdd", CultureInfo.InvariantCulture);
                VacancyStatus.EndDate = DateTime.ParseExact(EndDate, "yyyymmdd", CultureInfo.InvariantCulture);
                VacancyStatus.InitiatedBy = IntitatedBy;
                VacancyStatus.StatusVacancy = Status;
                VacancyStatus.UpdateDate = DateTime.Now;
                VacancyStatus.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void AddRowToDatabase(string Code, string Desc, DateTime StartDate, DateTime EndDate, string IntitatedBy, string Status)
        {
            try
            {
                TrnsVacancyStatus VacancyStatus = new TrnsVacancyStatus();
                VacancyStatus.Code = Code;
                VacancyStatus.Description = Desc;
                VacancyStatus.StartDate = StartDate;
                VacancyStatus.EndDate = EndDate;
                VacancyStatus.InitiatedBy = IntitatedBy;
                VacancyStatus.StatusVacancy = Status;
                VacancyStatus.CreateDate = DateTime.Now;
                VacancyStatus.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.TrnsVacancyStatus.InsertOnSubmit(VacancyStatus);
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
                int ID = oDBDataTable.GetValue("I", Row - 1);
                TrnsVacancyStatus Record = (from v in dbHrPayroll.TrnsVacancyStatus where v.Id == ID select v).Single();
                dbHrPayroll.TrnsVacancyStatus.DeleteOnSubmit(Record);
                dbHrPayroll.SubmitChanges();
                oDBDataTable.Rows.Remove(Row - 1);
                oMat.Clear();
                GetDataFormDataSource();
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
