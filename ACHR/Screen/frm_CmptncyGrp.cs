using System;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_CmptncyGrp:HRMSBaseForm 
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clCode, clDesc;
        private SAPbouiCOM.EditText oCell;

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
                oDBDataTable = oForm.DataSources.DataTables.Add("CompetencyGroup");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("C", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 4);
                oDBDataTable.Columns.Add("D", SAPbouiCOM.BoFieldsType.ft_Text, 100);

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("CompetencyGroup", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("CompetencyGroup", "I");

                oColumn = oColumns.Item("cl_Code");
                clCode = oColumn;
                oColumn.DataBind.Bind("CompetencyGroup", "C");

                oColumn = oColumns.Item("cl_Desc");
                clDesc = oColumn;
                oColumn.DataBind.Bind("CompetencyGroup", "D");

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
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstCompetencyGroup);
                var Data = from n in dbHrPayroll.MstCompetencyGroup select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.ID);
                    oDBDataTable.SetValue("C", i, v.GroupID);
                    oDBDataTable.SetValue("D", i, v.CompGroup);
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
                                    string Code = (oMat.Columns.Item("cl_Code").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (Code.Trim().Equals("") && pVal.Row != oMat.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullGroupID"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistGroupID"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;

                                case "cl_Desc":
                                    {
                                        var Desc = (oMat.Columns.Item("cl_Desc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                        if (Desc.Trim().Equals("") && pVal.Row != oMat.RowCount)
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
                    ValidateandSave(ref pVal, out BubbleEvent);
                    break;
            }
        }
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string Code = "", Desc = "";
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    Code = (oMat.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Desc = (oMat.Columns.Item("cl_Desc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (Code.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullGroupID"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Desc.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Code.Equals(oDBDataTable.GetValue("C", i - 1)) && Desc.Equals(oDBDataTable.GetValue("D", i - 1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Code, Desc, int.Parse(oDBDataTable.GetValue("I", i - 1).ToString()));
                    }
                }

                Code = (oMat.Columns.Item("cl_Code").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                Desc = (oMat.Columns.Item("cl_Desc").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
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
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullGroupID"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("C", oMat.RowCount - 1), oDBDataTable.GetValue("D", oMat.RowCount - 1));
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
        private void AddRowToDataBase(string Code, string Desc)
        {
            try
            {
                MstCompetencyGroup Competency = new MstCompetencyGroup();
                Competency.GroupID = Code;
                Competency.CompGroup = Desc;
                Competency.CreateDate = DateTime.Now;
                Competency.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.MstCompetencyGroup.InsertOnSubmit(Competency);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateExistingRecord(string Code, string Desc, int ID)
        {
            try
            {
                MstCompetencyGroup Competency = (from v in dbHrPayroll.MstCompetencyGroup where v.ID == ID select v).Single();
                Competency.GroupID = Code;
                Competency.CompGroup = Desc;
                Competency.UpdateDate = DateTime.Now;
                Competency.UserId = oCompany.UserSignature.ToString();
                Competency.UpdateBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }
}
