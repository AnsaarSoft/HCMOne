using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Qualif:HRMSBaseForm
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clName, clDesc, clActive;
        private SAPbouiCOM.EditText oCell;
        private SAPbouiCOM.CheckBox oCheck;
        private SAPbobsCOM.Recordset RecSet;
        private string Query;
        private MstQualification Qualification;

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

                oDBDataTable = oForm.DataSources.DataTables.Add("Qualif");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("N", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                oDBDataTable.Columns.Add("D", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                oDBDataTable.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text, 1);

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;
                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("Qualif", "No");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("Qualif", "I");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Name");
                clName = oColumn;
                oColumn.DataBind.Bind("Qualif", "N");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("Qualif", "D");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("clactive");
                clActive = oColumn;
                oColumn.DataBind.Bind("Qualif", "Active");
                oColumn.TitleObject.Sortable = false;
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
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstQualification);
                var Data = from n in dbHrPayroll.MstQualification select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("N", i, v.Code);
                    oDBDataTable.SetValue("D", i, v.Name);
                    if(Convert.ToBoolean(v.FlgActive))
                    {
                        oDBDataTable.SetValue("Active", i, "Y");
                    }
                    else
                    {
                        oDBDataTable.SetValue("Active", i, "N");
                    }
                    i += 1;
                }
                oMat.LoadFromDataSource();
                oMat.AutoResizeColumns();
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
                string Name = "", Desc = "", Active = "";
                bool flgActive = false;
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    oCell = oMat.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Name = oCell.Value;
                    oCell = oMat.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Desc = oCell.Value;
                    oCheck = oMat.Columns.Item("clactive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox;
                    if(oCheck.Checked)
                    {
                        Active = "Y";
                        flgActive = true;
                    }
                    else
                    {
                        Active = "N";
                        flgActive = false;
                    }

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
                    else if (Name.Equals(oDBDataTable.GetValue("N", i - 1)) && Desc.Equals(oDBDataTable.GetValue("D", i - 1)) && Active.Equals(oDBDataTable.GetValue("Active", i-1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Name, Desc, int.Parse(oDBDataTable.GetValue("I", i - 1).ToString()), flgActive);
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
                else if (Name.Equals("") && !Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("N", oMat.RowCount - 1).ToString(), oDBDataTable.GetValue("D", oMat.RowCount - 1).ToString(), flgActive);
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
        private void AddRowToDataBase(string Name, string Desc, bool Active)
        {
            RecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            try
            {
                Qualification = new MstQualification();
                Qualification.Code = Name;
                Qualification.Name = Desc;
                Qualification.FlgActive = Active;
                Qualification.CreateDate = DateTime.Now;
                Qualification.UserId = oCompany.UserName;
                dbHrPayroll.MstQualification.InsertOnSubmit(Qualification);
                dbHrPayroll.SubmitChanges();
                try
                {
                    Query = string.Format("insert into OHED (edType ,Name ,descriptio ) values ({0},'{1}','{2}')", Qualification.Id, Qualification.Code, Qualification.Name);
                    RecSet.DoQuery(Query);
                }
                catch (Exception)
                {
                    oApplication.StatusBar.SetText("Code Already Exist in Table OHED : Unable to add row in OHED", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
            }
        }
        private void UpdateExistingRecord(string Name, string Desc, int ID, bool Active)
        {
            RecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            try
            {
                //dbHrPayroll.ExecuteCommand("Update MstQualification set Name = {0},Description = {1},UpdateDate = {2} where ID = {3}", Name, Desc, DateTime.Now, ID);
                Qualification = (from v in dbHrPayroll.MstQualification where v.Id == ID select v).Single();
                Qualification.Code = Name;
                Qualification.Name = Desc;
                Qualification.FlgActive = Active;
                Qualification.UpdateDate = DateTime.Now;
                Qualification.UpdatedBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();
                Query = string.Format("Update \"OHED\" set \"name\" = '{0}',\"descriptio\" = '{1}'where \"edType\" = {2}", Name, Desc, ID);
                RecSet.DoQuery(Query);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
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
