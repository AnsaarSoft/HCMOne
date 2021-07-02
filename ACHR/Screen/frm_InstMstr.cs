using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_InstMstr : HRMSBaseForm
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn;
        private SAPbobsCOM.Recordset RecSet;
        private SAPbouiCOM.EditText oCell;
        private SAPbouiCOM.CheckBox oCheck;
        private MstInstitute InstMstr;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillCountryCombo();
            GetDataFromDataSource();
            AddBlankRow();
            oForm.Freeze(false);
        }
        private void InitiallizeForm()
        {
            try
            {
                oDBDataTable = oForm.DataSources.DataTables.Add("InstMstr");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("C", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                oDBDataTable.Columns.Add("N", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("CR", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 6);
                oDBDataTable.Columns.Add("CT", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 50);
                oDBDataTable.Columns.Add("County", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 50);
                oDBDataTable.Columns.Add("WR", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                //oDBDataTable.ExecuteQuery("select cast (0 as int) as No,cast(0 as int) as I,cast ('C' as nvarchar(20)) as C , cast ('N' as nvarchar(100)) as N,cast('CR' as nvarchar(6)) as CR,cast('CT' as nvarchar(50)) as CT,cast(0 as int) as WR");
                //oDBDataTable.Rows.Clear();

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;

                oColumn = oColumns.Item("cl_no");
                oColumn.DataBind.Bind("InstMstr", "No");

                oColumn = oColumns.Item("cl_ID");
                oColumn.DataBind.Bind("InstMstr", "I");

                oColumn = oColumns.Item("cl_Code");
                oColumn.DataBind.Bind("InstMstr", "C");

                oColumn = oColumns.Item("cl_Name");
                oColumn.DataBind.Bind("InstMstr", "N");

                oColumn = oColumns.Item("cl_Country");
                oColumn.DataBind.Bind("InstMstr", "CR");

                oColumn = oColumns.Item("cl_City");
                oColumn.DataBind.Bind("InstMstr", "CT");

                oColumn = oColumns.Item("cl_County");
                oColumn.DataBind.Bind("InstMstr", "County");

                oColumn = oColumns.Item("cl_WRank");
                oColumn.DataBind.Bind("InstMstr", "WR");

                oColumn = oColumns.Item("clactive");
                oColumn.DataBind.Bind("InstMstr", "Active");
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
        private void FillCountryCombo()
        {
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            try
            {
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;
                oColumn = oColumns.Item("cl_Country");
                //RecSet.DoQuery("Select Code,Name from OCRY"); // Old Normal
                RecSet.DoQuery("Select \"Code\",\"Name\" from OCRY"); // Hana & Normal
                while (!RecSet.EoF)
                {
                    oColumn.ValidValues.Add(RecSet.Fields.Item("Code").Value, RecSet.Fields.Item("Name").Value);
                    RecSet.MoveNext();
                }
                oColumn.DisplayDesc = true;
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
        private void GetDataFromDataSource()
        {
            try
            {
                oMat.Clear();
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstInstitute);
                var Data = (from n in dbHrPayroll.MstInstitute select n).ToList();
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("C", i, v.Code);
                    oDBDataTable.SetValue("N", i, v.Name);
                    oDBDataTable.SetValue("CR", i, v.CountryID.GetValueOrDefault());
                    oDBDataTable.SetValue("CT", i, v.CityID.GetValueOrDefault());
                    oDBDataTable.SetValue("County", i, v.County == null ? "" : v.County);
                    oDBDataTable.SetValue("WR", i, v.WorldRank.GetValueOrDefault());
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
                                    var Name = (oMat.Columns.Item("cl_Code").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (Name.Equals("") && pVal.Row != oMat.RowCount)
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
                                        else if (Name == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;

                                case "cl_Name":
                                    {
                                        var Desc = (oMat.Columns.Item("cl_Name").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                        if (Desc.Equals("") && pVal.Row != oMat.RowCount)
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
                                            else if (Desc == oCell.Value.Trim().ToLower())
                                            {
                                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    ValidateandSave(ref pVal, out BubbleEvent);
                    break;
            }
        }
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string Code = "", Name = "", Country = "", City = "", County = "", WorldRank = "", Active = "";
                bool flgActive = false;
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    Code = (oMat.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Name = (oMat.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Country = (oMat.Columns.Item("cl_Country").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    City = (oMat.Columns.Item("cl_City").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    County = (oMat.Columns.Item("cl_County").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    WorldRank = (oMat.Columns.Item("cl_WRank").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    flgActive = (oMat.Columns.Item("clactive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    if (flgActive)
                    {
                        Active = "Y";
                    }
                    else
                    {
                        Active = "N";
                    }
                    if (Code.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Name.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Country.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCountry"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (City.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCity"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }

                    else if (Code.Equals(oDBDataTable.GetValue("C", i - 1)) && Name.Equals(oDBDataTable.GetValue("N", i - 1)) &&
                        Country.Equals(oDBDataTable.GetValue("CR", i - 1)) && City.Equals(oDBDataTable.GetValue("CT", i - 1)) &&
                        County.Equals(oDBDataTable.GetValue("County", i - 1)) && int.Parse(WorldRank) == (oDBDataTable.GetValue("WR", i - 1)) && Active.Equals(oDBDataTable.GetValue("Active", i-1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Code, Name, Country, City, County, int.Parse(WorldRank), int.Parse(oDBDataTable.GetValue("I", i - 1).ToString()), flgActive);
                    }
                }

                Code = (oMat.Columns.Item("cl_Code").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                Name = (oMat.Columns.Item("cl_Name").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                Country = (oMat.Columns.Item("cl_Country").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                City = (oMat.Columns.Item("cl_City").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                County = (oMat.Columns.Item("cl_County").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                WorldRank = (oMat.Columns.Item("cl_WRank").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                flgActive = (oMat.Columns.Item("clactive").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.CheckBox).Checked;

                if (Code.Equals("") && Name.Equals(""))
                {
                    return;
                }
                else if (!Code.Equals("") && Name.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && Country.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCountry"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && City.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCity"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && WorldRank.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullWorldRank"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("C", oMat.RowCount - 1).ToString(), oDBDataTable.GetValue("N", oMat.RowCount - 1).ToString(),
                        oDBDataTable.GetValue("CR", oMat.RowCount - 1), oDBDataTable.GetValue("CT", oMat.RowCount - 1), oDBDataTable.GetValue("County", oMat.RowCount - 1), oDBDataTable.GetValue("WR", oMat.RowCount - 1), flgActive);
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
        private void AddRowToDataBase(string Code, string Name, string Country, string City, string County, int WorldRank, bool flgActive)
        {
            try
            {
                InstMstr = new MstInstitute();
                InstMstr.Code = Code;
                InstMstr.Name = Name;
                //InstMstr.Country = Country;
                //InstMstr.City = City;
                InstMstr.FlgActive = flgActive;
                InstMstr.County = County;
                InstMstr.WorldRank = WorldRank;
                InstMstr.CreateDate = DateTime.Now;
                InstMstr.UserID = oCompany.UserName;
                InstMstr.UserID = oCompany.UserSignature.ToString();
                dbHrPayroll.MstInstitute.InsertOnSubmit(InstMstr);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateExistingRecord(string Code, string Name, string Country, string City, string County, int WorldRank, int Id, bool flgActive)
        {
            try
            {
                //dbHrPayroll.ExecuteCommand("Update MstInstitue set Code = {0},Name = {1},Country = {2},City = {3},WorldRank = {4} where ID = {5}",Code, Name, 1, City ,WorldRank,Id);
                InstMstr = (from v in dbHrPayroll.MstInstitute where v.Id == Id select v).Single();
                InstMstr.Id = Id;
                InstMstr.Code = Code;
                InstMstr.Name = Name;
                //InstMstr.Country = Country;
                //InstMstr.City = City;
                InstMstr.FlgActive = flgActive;
                InstMstr.County = County;
                InstMstr.WorldRank = WorldRank;
                InstMstr.UpdateDate = DateTime.Now;
                InstMstr.UpdatedBy = oCompany.UserName;
                //InstMstr.UserID = oCompany.UserSignature.ToString();
                dbHrPayroll.SubmitChanges();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }
}
