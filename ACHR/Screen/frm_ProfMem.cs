using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_ProfMem:HRMSBaseForm
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clName, clDesc;
        private SAPbouiCOM.EditText oCell;
        private MstMemberShip ProfMemmbership;

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

                oDBDataTable = oForm.DataSources.DataTables.Add("ProfMem");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("N", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                oDBDataTable.Columns.Add("D", SAPbouiCOM.BoFieldsType.ft_Text, 100);

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;
                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("ProfMem", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("ProfMem", "I");

                oColumn = oColumns.Item("cl_Name");
                clName = oColumn;
                oColumn.DataBind.Bind("ProfMem", "N");

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("ProfMem", "D");
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
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstMemberShip);
                var Data = from n in dbHrPayroll.MstMemberShip select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("N", i, v.Name);
                    oDBDataTable.SetValue("D", i, v.Description);
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
                                case "cl_Name":
                                    var Name = (oMat.Columns.Item("cl_Name").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (Name.Equals("") && pVal.Row != oMat.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        BubbleEvent = false;
                                        return;
                                    }

                                    for (int i = 1; i <= oMat.RowCount; i++)
                                    {
                                        oCell = oMat.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                        if (i == pVal.Row)
                                            continue;
                                        else if (Name == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;

                                case "cl_Desc":
                                    {
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
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                return;
            switch (pVal.ItemUID)
            {
                case "1":
                    //if (!CheckDuplicateRecords("cl_Name") && !CheckDuplicateRecords("cl_Desc"))
                    //{
                        ValidateandSave(ref pVal, out BubbleEvent);
                    //}
                    //else
                    //{
                    //    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DupRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    //    BubbleEvent = false;
                    //}
                    break;
            }
        }
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string Name = "", Desc = "";
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    oCell = oMat.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Name = oCell.Value;
                    oCell = oMat.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Desc = oCell.Value;
                    if (Name.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Desc.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Name.Equals(oDBDataTable.GetValue("N", i - 1)) && Desc.Equals(oDBDataTable.GetValue("D", i - 1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Name, Desc, int.Parse(oDBDataTable.GetValue("I", i - 1).ToString()));
                    }
                }

                oCell = oMat.Columns.Item("cl_Name").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                Name = oCell.Value.ToLower().Trim();
                oCell = oMat.Columns.Item("cl_Desc").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                Desc = oCell.Value.ToLower().Trim();
                if (Name.Equals("") && Desc.Equals(""))
                {
                    return;
                }
                else if (!Name.Equals("") && Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("N", oMat.RowCount - 1).ToString(), oDBDataTable.GetValue("D", oMat.RowCount - 1).ToString());
                    GetDataFromDataSource();
                    AddBlankRow();
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        private void AddRowToDataBase(string Name, string Desc)
        {
            try
            {
                ProfMemmbership = new MstMemberShip();
                ProfMemmbership.Name = Name;
                ProfMemmbership.Description = Desc;
                ProfMemmbership.CreateDate = DateTime.Now;
                dbHrPayroll.MstMemberShip.InsertOnSubmit(ProfMemmbership);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateExistingRecord(string Name, string Desc, int ID)
        {
            try
            {
                //dbHrPayroll.ExecuteCommand("Update MstMemberShip set Name = {0},Description = {1},UpdateDate = {2} where ID = {3}", Name, Desc, DateTime.Now, ID);
                ProfMemmbership = (from v in dbHrPayroll.MstMemberShip where v.Id == ID select v).Single();
                ProfMemmbership.Name = Name;
                ProfMemmbership.Description = Desc;
                ProfMemmbership.UpdateDate = DateTime.Now;
                dbHrPayroll.SubmitChanges();
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
