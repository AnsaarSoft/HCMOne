using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_PrfCen : HRMSBaseForm
    {
        #region Variables

        private SAPbouiCOM.DataTable dtProfitCenter;
        private SAPbouiCOM.Matrix grdProfitCenter;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clName, clDesc, clActive;
        private SAPbouiCOM.EditText oCell;
        private SAPbouiCOM.CheckBox oCheck;
        private MstProfitCenter oDoc;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            GetData();
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
                                case "cl_Name":
                                    var Name = (grdProfitCenter.Columns.Item("cl_Name").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (Name.Equals("") && pVal.Row != grdProfitCenter.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullName"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        BubbleEvent = false;
                                        return;
                                    }

                                    for (int i = 1; i <= grdProfitCenter.RowCount; i++)
                                    {
                                        oCell = grdProfitCenter.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText;
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
                                        var Desc = (grdProfitCenter.Columns.Item("cl_Desc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                        if (Desc.Equals("") && pVal.Row != grdProfitCenter.RowCount)
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }
                                        for (int i = 1; i <= grdProfitCenter.RowCount; i++)
                                        {
                                            oCell = grdProfitCenter.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
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

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                dtProfitCenter = oForm.DataSources.DataTables.Add("ProfitCenter");
                dtProfitCenter.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtProfitCenter.Columns.Add("Id", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtProfitCenter.Columns.Add("Name", SAPbouiCOM.BoFieldsType.ft_Text, 20);
                dtProfitCenter.Columns.Add("Desc", SAPbouiCOM.BoFieldsType.ft_Text, 100);
                dtProfitCenter.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text, 1);

                grdProfitCenter = oForm.Items.Item("Mat").Specific;
                oColumns = grdProfitCenter.Columns;
                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("ProfitCenter", "No");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("ProfitCenter", "Id");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Name");
                clName = oColumn;
                oColumn.DataBind.Bind("ProfitCenter", "Name");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("ProfitCenter", "Desc");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("clactive");
                clActive = oColumn;
                oColumn.DataBind.Bind("ProfitCenter", "Active");
                oColumn.TitleObject.Sortable = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            try
            {
                grdProfitCenter.Clear();
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstProfitCenter);
                var Data = (from n in dbHrPayroll.MstProfitCenter select n).ToList();
                dtProfitCenter.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    dtProfitCenter.Rows.Add(1);
                    dtProfitCenter.SetValue("No", i, i + 1);
                    dtProfitCenter.SetValue("Id", i, v.InternalID);
                    dtProfitCenter.SetValue("Name", i, v.Code);
                    dtProfitCenter.SetValue("Desc", i, v.Description);
                    if(Convert.ToBoolean(v.FlgActive))
                    {
                        dtProfitCenter.SetValue("Active", i, "Y");
                    }
                    else
                    {
                        dtProfitCenter.SetValue("Active", i, "N");
                    }
                    i += 1;
                }
                grdProfitCenter.LoadFromDataSource();
                grdProfitCenter.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetData Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void AddBlankRow()
        {
            dtProfitCenter.Rows.Clear();
            grdProfitCenter.AddRow(1, grdProfitCenter.RowCount + 1);
            grdProfitCenter.FlushToDataSource();
            (grdProfitCenter.Columns.Item("cl_no").Cells.Item(grdProfitCenter.RowCount).Specific as SAPbouiCOM.EditText).Value = (grdProfitCenter.RowCount).ToString();
        }
        
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string Name = "", Desc = "", Active = "";
                bool flgActive = false;
                for (int i = 1; i < grdProfitCenter.RowCount; i++)
                {
                    oCell = grdProfitCenter.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Name = oCell.Value;
                    oCell = grdProfitCenter.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    Desc = oCell.Value;
                    oCheck = grdProfitCenter.Columns.Item("clactive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox;
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
                    if (string.IsNullOrEmpty(Name))
                    {
                        oApplication.StatusBar.SetText("Code Can't be empty", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (string.IsNullOrEmpty(Desc))
                    {
                        oApplication.StatusBar.SetText("desc can't be empty", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Name.Equals(dtProfitCenter.GetValue("Name", i - 1)) && Desc.Equals(dtProfitCenter.GetValue("Desc", i - 1)) && Active.Equals(dtProfitCenter.GetValue("Active", i -1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Name, Desc, int.Parse(dtProfitCenter.GetValue("Id", i - 1).ToString()), flgActive);
                    }
                }

                oCell = grdProfitCenter.Columns.Item("cl_Name").Cells.Item(grdProfitCenter.RowCount).Specific as SAPbouiCOM.EditText;
                Name = oCell.Value.ToLower().Trim();
                oCell = grdProfitCenter.Columns.Item("cl_Desc").Cells.Item(grdProfitCenter.RowCount).Specific as SAPbouiCOM.EditText;
                Desc = oCell.Value.ToLower().Trim();
                if (Name.Equals("") && Desc.Equals(""))
                {
                    return;
                }
                else if (!Name.Equals("") && Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText("desc can't be null", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (Name.Equals("") && !Desc.Equals(""))
                {
                    oApplication.StatusBar.SetText("code can't be null", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    grdProfitCenter.FlushToDataSource();
                    AddRowToDataBase(dtProfitCenter.GetValue("Name", grdProfitCenter.RowCount - 1).ToString(), dtProfitCenter.GetValue("Desc", grdProfitCenter.RowCount - 1).ToString(), flgActive);
                    GetData();
                    AddBlankRow();
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ValidateandSave Ex : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }
        
        private void AddRowToDataBase(string Name, string Desc, bool Active)
        {
            try
            {
                oDoc = new MstProfitCenter();
                oDoc.Code = Name;
                oDoc.Description = Desc;
                oDoc.FlgActive = Active;
                oDoc.FlgDelete = false;
                oDoc.CreateDt = DateTime.Now;
                oDoc.UpdateDt = DateTime.Now;
                oDoc.CreatedBy = oCompany.UserName;
                oDoc.UpdatedBy = oCompany.UserName;
                dbHrPayroll.MstProfitCenter.InsertOnSubmit(oDoc);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void UpdateExistingRecord(string Name, string Desc, int ID, bool Active)
        {
            try
            {
                //dbHrPayroll.ExecuteCommand("Update MstCertification set Name = {0},Description = {1},UpdateDate = {2} where ID = {3}", Name, Desc, DateTime.Now, ID);
                oDoc = (from v in dbHrPayroll.MstProfitCenter where v.InternalID == ID select v).FirstOrDefault();
                oDoc.Code = Name;
                oDoc.Description = Desc;
                oDoc.FlgActive = Active;
                oDoc.UpdateDt = DateTime.Now;
                oDoc.UpdatedBy = oCompany.UserName;
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
                for (int i = 1; i <= grdProfitCenter.RowCount; i++)
                {
                    oCell = grdProfitCenter.Columns.Item(ColName).Cells.Item(i).Specific as SAPbouiCOM.EditText;
                    value = oCell.Value;
                    for (int j = 1; j <= grdProfitCenter.RowCount; j++)
                    {
                        if (i == j)
                        { continue; }
                        else if (value.ToLower().Equals((grdProfitCenter.Columns.Item(ColName).Cells.Item(j).Specific as SAPbouiCOM.EditText).Value.ToLower()))
                        {
                            return (result = true);
                        }
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("CheckDuplicateRecords Ex : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return (result = true);
            }
        }

        #endregion
    }
}
