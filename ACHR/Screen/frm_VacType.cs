using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_VacType:HRMSBaseForm 
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clCode, clDesc, clActive;
        private SAPbouiCOM.EditText oCell;
        private MstVacancyTypes VacTyp;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            GetDataFromDataSource();
            AddBlankRow();
            oForm.Freeze(false);
        }
        private void InitiallizeForm()
        {
            try
            {
                oDBDataTable = oForm.DataSources.DataTables.Add("VacTyp");
                oDBDataTable.ExecuteQuery("select cast(0 as int) as No,cast(0 as int) as I,cast ('C' as nvarchar(20)) as C, cast ('D' as nvarchar(100)) as D,cast (0 as bit) as A");
                oDBDataTable.Rows.Clear();

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oMat.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("VacTyp", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("VacTyp", "I");

                oColumn = oColumns.Item("cl_Code");
                clCode = oColumn;
                oColumn.DataBind.Bind("VacTyp", "C");

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("VacTyp", "D");

                oColumn = oColumns.Item("cl_Active");
                clActive = oColumn;
                oColumn.DataBind.Bind("VacTyp", "A");
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
                oMat.Clear();
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstVacancyTypes);
                var Data = from n in dbHrPayroll.MstVacancyTypes select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("C", i, v.Code);
                    oDBDataTable.SetValue("D", i, v.Description);
                    oDBDataTable.SetValue("A", i, v.StatusVacancy == true ? "Y" : "N");
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
            oDBDataTable.Rows.Clear();
            oMat.AddRow(1, oMat.RowCount + 1);
            oMat.FlushToDataSource();
            (oMat.Columns.Item("cl_no").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
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
                                    break;

                                case "cl_Desc":
                                        var Desc = (oMat.Columns.Item("cl_Desc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                        if (Desc.Equals("") && pVal.Row != oMat.RowCount)
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }
                                        for (int i = 1; i <= oMat.RowCount; i++)
                                        {
                                            oCell = oMat.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                            if (i == pVal.Row)
                                                continue;
                                            else if (Desc == oCell.Value.Trim().ToLower())
                                            {
                                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
            }
        }
        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            switch (pVal.ItemUID)
            {
                case "1":
                    //if (!CheckDuplicateRecords("cl_Code") && !CheckDuplicateRecords("cl_Desc"))
                    //{
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                        return;
                    else 
                        ValidateandSave(ref pVal, out BubbleEvent);
                    //}
                    //else
                    //{
                    //    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DupRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    //    BubbleEvent = false;
                    //}
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
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string Code = "", Desc = "";
                bool Active;
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    Code = (oMat.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Desc = (oMat.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Active = (oMat.Columns.Item("cl_Active").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
    
                    if (Code.Trim().Equals(""))
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
                    else if (Code.Equals(oDBDataTable.GetValue("C", i - 1)) && Desc.Equals(oDBDataTable.GetValue("D", i - 1)) && Active.Equals(oDBDataTable.GetValue("A", i - 1) == "Y" ? true : false))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Code, Desc, Active,oDBDataTable.GetValue("I", i - 1));
                    }
                }

                Code = (oMat.Columns.Item("cl_Code").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value.ToLower().Trim();
                Desc = (oMat.Columns.Item("cl_Desc").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value.ToLower().Trim();
                if (Code.Equals("") && Desc.Equals(""))
                {
                    return;
                }
                else if (!Code.Equals("") && Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (Code.Equals("") && !Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("C", oMat.RowCount - 1), oDBDataTable.GetValue("D", oMat.RowCount - 1), oDBDataTable.GetValue("A", oMat.RowCount - 1));
                    GetDataFromDataSource();
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
        private void AddRowToDataBase(string Code, string Desc,string Active)
        {
            try
            {
                VacTyp = new MstVacancyTypes();
                VacTyp.Code = Code;
                VacTyp.Description = Desc;
                VacTyp.StatusVacancy = (Active == "Y" ? true : false);
                VacTyp.CreateDate = DateTime.Now;
                VacTyp.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.MstVacancyTypes.InsertOnSubmit(VacTyp);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateExistingRecord(string Code, string Desc,bool Active, int ID)
        {
            try
            {
                VacTyp = (from v in dbHrPayroll.MstVacancyTypes where v.Id == ID select v).Single();
                VacTyp.Code = Code;
                VacTyp.Description = Desc;
                VacTyp.StatusVacancy = Active;
                VacTyp.UpdateDate = DateTime.Now;
                VacTyp.UserId = oCompany.UserSignature.ToString();
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
                MstVacancyTypes Record = (from v in dbHrPayroll.MstVacancyTypes where v.Id == ID select v).Single();
                dbHrPayroll.MstVacancyTypes.DeleteOnSubmit(Record);
                dbHrPayroll.SubmitChanges();
                oDBDataTable.Rows.Remove(Row - 1);
                oMat.Clear();
                GetDataFromDataSource();
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
                for (int i = 1; i <= oMat.RowCount; i++)
                {
                    oCell = oMat.Columns.Item(ColName).Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    value = oCell.Value;
                    for (int j = 1; j <= oMat.RowCount; j++)
                    {
                        if (i == j)
                        { continue; }
                        else if (value.ToLower().Equals((oMat.Columns.Item(ColName).Cells.Item(j).Specific as SAPbouiCOM.EditText).Value.ToLower()))
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
    }
}
