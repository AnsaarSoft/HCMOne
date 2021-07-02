using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_KPI:HRMSBaseForm
    {
        #region Variable Section
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clKeyObj, clLagKPI, clLedKPI;
        private SAPbouiCOM.EditText oCell;
        private TrnsKPI KPI;
        #endregion

        #region B1 Events
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            GetDataFromDataSource();
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
                                case "cl_Key":
                                    var KeyObj = (oMat.Columns.Item("cl_Key").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (KeyObj.Equals("") && pVal.Row != oMat.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullKeyObj"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        BubbleEvent = false;
                                        return;
                                    }

                                    for (int i = 1; i <= oMat.RowCount; i++)
                                    {
                                        oCell = oMat.Columns.Item("cl_Key").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                        if (i == pVal.Row)
                                            continue;
                                        else if (KeyObj == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistKeyObj"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    //if (!CheckDuplicateRecords("cl_Key"))
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
                    //int Count = 0;
                    //for (int i = 1; i < oMat.RowCount; i++)
                    //{
                    //    if (oMat.IsRowSelected(i))
                    //    {
                    //        int Res = oApplication.MessageBox(Program.objHrmsUI.getStrMsg("War_DelRow"), 1, "Yes", "No", "");
                    //        if (Res == 1)
                    //        {
                    //            DeleteRecord(i);
                    //        }
                    //        else
                    //        {
                    //            oMat.SelectRow(i, false, false);
                    //        }
                    //        Count += 1;
                    //    }

                    //}
                    //if (Count == 0)
                    //{
                    //    oApplication.MessageBox(Program.objHrmsUI.getStrMsg("War_SelectRow"), 1, "OK", "", "");
                    //}
                    break;
            }
        }
        #endregion

        #region Functions
        void InitiallizeForm()
        {
            try
            {
                oDBDataTable = oForm.DataSources.DataTables.Add("KPI");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("Key", SAPbouiCOM.BoFieldsType.ft_Text, 60);
                oDBDataTable.Columns.Add("LagKPI", SAPbouiCOM.BoFieldsType.ft_Text, 60);
                oDBDataTable.Columns.Add("LedKPI", SAPbouiCOM.BoFieldsType.ft_Text, 60);

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oMat.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;
                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("KPI", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("KPI", "I");

                oColumn = oColumns.Item("cl_Key");
                clKeyObj = oColumn;
                oColumn.DataBind.Bind("KPI", "Key");

                oColumn = oColumns.Item("cl_lagKPI");
                clLagKPI = oColumn;
                oColumn.DataBind.Bind("KPI", "LagKPI");

                oColumn = oColumns.Item("cl_ledKPI");
                clLedKPI = oColumn;
                oColumn.DataBind.Bind("KPI", "LedKPI");
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
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsKPI);
                var Data = from n in dbHrPayroll.TrnsKPI select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.ID);
                    oDBDataTable.SetValue("Key", i, v.KeyObjectives);
                    oDBDataTable.SetValue("LagKPI", i, v.LaggingKPI);
                    oDBDataTable.SetValue("LedKPI", i, v.LeadingKPI);
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
        
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string KPIobj = "", LagKPI = "", LedKPI = "";
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    KPIobj = (oMat.Columns.Item("cl_Key").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    LagKPI = (oMat.Columns.Item("cl_lagKPI").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    LedKPI = (oMat.Columns.Item("cl_ledKPI").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (KPIobj.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullKeyObj"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (LagKPI.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLagKPI"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (LedKPI.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLedKPI"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (KPIobj.Equals(oDBDataTable.GetValue("Key", i - 1)) && LagKPI.Equals(oDBDataTable.GetValue("LagKPI", i - 1)) &&
                        LedKPI.Equals(oDBDataTable.GetValue("LedKPI", i - 1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(KPIobj, LagKPI, LedKPI, int.Parse(oDBDataTable.GetValue("I", i - 1).ToString()));
                    }
                }

                KPIobj = (oMat.Columns.Item("cl_Key").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                LagKPI = (oMat.Columns.Item("cl_lagKPI").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                LedKPI = (oMat.Columns.Item("cl_ledKPI").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                if (KPIobj.Equals("") && LagKPI.Equals("") && LedKPI.Equals(""))
                {
                    return;
                }
                else if (!KPIobj.Equals("") && LagKPI.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLagKPI"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (!KPIobj.Equals("") && LedKPI.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLedKPI"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }

                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("Key", oMat.RowCount - 1), oDBDataTable.GetValue("LagKPI", oMat.RowCount - 1), oDBDataTable.GetValue("LedKPI", oMat.RowCount - 1).ToString());
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
        
        private void AddRowToDataBase(string KeyObj, string LagKPI,string LedKPI)
        {
            try
            {
                KPI = new TrnsKPI();
                KPI.KeyObjectives = KeyObj;
                KPI.LaggingKPI = LagKPI;
                KPI.LeadingKPI = LedKPI;
                KPI.CreateDate = DateTime.Now;
                KPI.UserID = oCompany.UserSignature.ToString();
                dbHrPayroll.TrnsKPI.InsertOnSubmit(KPI);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void UpdateExistingRecord(string KPIObj, string LagKPI,string LedKPI, int ID)
        {
            try
            {
                KPI = (from v in dbHrPayroll.TrnsKPI where v.ID == ID select v).Single();
                KPI.KeyObjectives = KPIObj;
                KPI.LaggingKPI = LagKPI;
                KPI.LeadingKPI = LedKPI;
                KPI.UpdateDate = DateTime.Now;
                KPI.UserID = oCompany.UserSignature.ToString();
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
                TrnsKPI Record = (from v in dbHrPayroll.TrnsKPI where v.ID == ID select v).Single();
                dbHrPayroll.TrnsKPI.DeleteOnSubmit(Record);
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
                oForm.Freeze(false);
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
        #endregion
    }
}
