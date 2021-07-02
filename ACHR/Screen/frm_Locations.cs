using System;
using System.Linq;
using DIHRMS;
namespace ACHR.Screen
{
    class frm_Locations:HRMSBaseForm
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clName, clDesc, clCostCenter, clAttID, clTradeTLicenseNo,clBankCode, clActive;
        private SAPbouiCOM.EditText oCell;
        private SAPbouiCOM.CheckBox oCheck;
        private MstLocation Location;
        
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
                oDBDataTable = oForm.DataSources.DataTables.Add("Locations");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("N", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                oDBDataTable.Columns.Add("D", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                oDBDataTable.Columns.Add("C", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                oDBDataTable.Columns.Add("A", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                oDBDataTable.Columns.Add("TL", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                oDBDataTable.Columns.Add("BC", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                oDBDataTable.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text, 1);

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;
                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("Locations", "No");
                oColumn.TitleObject.Sortable = false;
               

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("Locations", "I");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Name");
                clName = oColumn;
                oColumn.DataBind.Bind("Locations", "N");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("Locations", "D");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("clactive");
                clActive = oColumn;
                oColumn.DataBind.Bind("Locations", "Active");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("V_1");
                clCostCenter = oColumn;
                oColumn.DataBind.Bind("Locations", "C");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("V_0");
                clAttID = oColumn;
                oColumn.DataBind.Bind("Locations", "A");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("V_2");
                clTradeTLicenseNo = oColumn;
                oColumn.DataBind.Bind("Locations", "TL");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("V_3");
                clBankCode = oColumn;
                oColumn.DataBind.Bind("Locations", "BC");
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
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstLocation);
                var Data = (from n in dbHrPayroll.MstLocation select n).ToList();
                oDBDataTable.Rows.Clear();
                int i = 0;                
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("N", i, v.Name);
                    oDBDataTable.SetValue("D", i, v.Description);
                    string ActiveValue = "N";
                    if (Convert.ToBoolean(v.FlgActive))
                    {
                        ActiveValue = "Y";
                    }
                    else
                    {
                        ActiveValue = "N";
                    }
                    oDBDataTable.SetValue("Active", i, ActiveValue);
                    oDBDataTable.SetValue("C", i, string.IsNullOrEmpty(v.CostCenter) ? "" : v.CostCenter);
                    oDBDataTable.SetValue("A", i, !string.IsNullOrEmpty(v.AttandanceID) ? v.AttandanceID : "");
                    oDBDataTable.SetValue("TL", i, string.IsNullOrEmpty(v.TradeLicenseNo) ? "" : v.TradeLicenseNo);
                    oDBDataTable.SetValue("BC", i, string.IsNullOrEmpty(v.BankCode) ? "" : v.BankCode);
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
            try
            {
                oDBDataTable.Rows.Clear();
                oMat.AddRow(1, oMat.RowCount + 1);
                oMat.FlushToDataSource();
                (oMat.Columns.Item("cl_no").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
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
            switch (pVal .ItemUID )
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
                string Name = "", Desc = "", CostCenter = "", AttID = "", TradeLicenseNo = "", BankCode = "", Active = "";
                bool flgActive = false;
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    oCell = oMat.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Name = oCell.Value;
                    oCell = oMat.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Desc = oCell.Value;
                    oCheck = oMat.Columns.Item("clactive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox;
                    flgActive = oCheck.Checked;
                    if (oCheck.Checked)
                    {
                        Active = "Y";
                    }
                    else
                    {
                        Active = "N";
                    }
                    oCell = oMat.Columns.Item("V_1").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    CostCenter = oCell.Value.Trim();
                    oCell = oMat.Columns.Item("V_0").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    AttID = oCell.Value.Trim();

                    oCell = oMat.Columns.Item("V_2").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    TradeLicenseNo = oCell.Value.Trim();
                    oCell = oMat.Columns.Item("V_3").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    BankCode = oCell.Value.Trim();

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
                    else if (Name.Equals(oDBDataTable.GetValue("N", i - 1)) && Desc.Equals(oDBDataTable.GetValue("D", i - 1)) && CostCenter.Equals(oDBDataTable.GetValue("C", i - 1)) && AttID.Equals(oDBDataTable.GetValue("A", i - 1)) && TradeLicenseNo.Equals(oDBDataTable.GetValue("TL", i - 1)) && BankCode.Equals(oDBDataTable.GetValue("BC", i - 1)) && Active.Equals(oDBDataTable.GetValue("Active", i-1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Name, Desc, CostCenter, AttID, TradeLicenseNo, BankCode, int.Parse(oDBDataTable.GetValue("I", i - 1).ToString()), flgActive);
                    }
                }

                oCell = oMat.Columns.Item("cl_Name").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                Name = oCell.Value.ToLower().Trim();
                oCell = oMat.Columns.Item("cl_Desc").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                oCheck = oMat.Columns.Item("clactive").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.CheckBox;
                if (oCheck.Checked)
                {
                    Active = "Y";
                    flgActive = true;
                }
                else
                {
                    Active = "N";
                    flgActive = false;
                }
                Desc = oCell.Value.ToLower().Trim();
                oCell = oMat.Columns.Item(clCostCenter.UniqueID).Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                CostCenter = oCell.Value.ToLower().Trim();
                oCell = oMat.Columns.Item(clAttID.UniqueID).Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                AttID = oCell.Value.ToLower().Trim();

                oCell = oMat.Columns.Item("V_2").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                TradeLicenseNo = oCell.Value.ToLower().Trim();
                oCell = oMat.Columns.Item("V_3").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText;
                BankCode = oCell.Value.ToLower().Trim();

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
                    AddRowToDataBase(
                        oDBDataTable.GetValue("N", oMat.RowCount - 1).ToString(), 
                        oDBDataTable.GetValue("D", oMat.RowCount - 1).ToString(), 
                        oDBDataTable.GetValue("C", oMat.RowCount - 1).ToString(), 
                        oDBDataTable.GetValue("A", oMat.RowCount - 1).ToString(), 
                        oDBDataTable.GetValue("TL", oMat.RowCount - 1).ToString(), 
                        oDBDataTable.GetValue("BC", oMat.RowCount - 1).ToString(),
                        oDBDataTable.GetValue("Active", oMat.RowCount - 1).ToString()
                        );
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

        private void AddRowToDataBase(string Name, string Desc, string costcenter, string attid, string TradeLicenseNo,string BankCode, string Active)
        {
            try
            {
                Location = new MstLocation();
                Location.Name = Name;
                Location.Description = Desc;
                if (Active == "Y")
                {
                    Location.FlgActive = true;
                }
                else
                {
                    Location.FlgActive = false;
                }
                Location.CostCenter = costcenter;
                Location.AttandanceID = attid;
                Location.TradeLicenseNo = TradeLicenseNo;
                Location.BankCode = BankCode;
                Location.CreateDate = DateTime.Now;
                //AR
                Location.UserId = oCompany.UserName;
                dbHrPayroll.MstLocation.InsertOnSubmit(Location);
                dbHrPayroll.SubmitChanges();
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void UpdateExistingRecord(string Name, string Desc, string costcenter, string attid, string TradeLicenseNo, string BankCode, int ID, bool flgActive)
        {
            try
            {

                Location = (from v in dbHrPayroll.MstLocation where v.Id == ID select v).FirstOrDefault();
                var EmpWithLocationID = dbHrPayroll.MstEmployee.Where(e => e.Location == ID && e.FlgActive == true).ToList();
                if (EmpWithLocationID != null && EmpWithLocationID.Count > 0 && Location.Name != Name)
                {
                    oApplication.MessageBox(Name + " Location Can't be updated.It is attached with Employee(s)");
                    oApplication.StatusBar.SetText(Name + " Location Can't be updated.It is attached with Employee(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                Location.Name = Name;
                Location.Description = Desc;
                Location.FlgActive = flgActive;
                Location.CostCenter = costcenter;
                Location.AttandanceID = attid;
                Location.TradeLicenseNo = TradeLicenseNo;
                Location.BankCode = BankCode;
                Location.UpdateDate = DateTime.Now;
                //AR
                Location.UpdatedBy = oCompany.UserName;
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
