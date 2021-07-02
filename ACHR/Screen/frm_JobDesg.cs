using System;
using System.Linq;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_JobDesg:HRMSBaseForm
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clCode, clName, clMinExp, clPrntDesg, clJobPos;
        private SAPbouiCOM.EditText oCell;
        private MstJobDesignation JobDesg;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            
            oForm.Freeze(true);
            InitiallizeForm();
            FillParentDesignationCombo();
            FillJobPostionCombo();
            GetDataFromDataSource();
            AddBlankRow();
            oForm.Freeze(false);
        }
        private void InitiallizeForm()
        {
            try
            {
                oDBDataTable = oForm.DataSources.DataTables.Add("JobDesg");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("I", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("C", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                oDBDataTable.Columns.Add("N", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("ME", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("PD", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                oDBDataTable.Columns.Add("JP", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                oDBDataTable.Rows.Clear();

                oMat = (SAPbouiCOM.Matrix)oForm.Items.Item("Mat").Specific;
                oMat.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
                oColumns = (SAPbouiCOM.Columns)oMat.Columns;

                oColumn = oColumns.Item("cl_No");
                clNo = oColumn;
                oColumn.DataBind.Bind("JobDesg", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("JobDesg", "I");

                oColumn = oColumns.Item("cl_Code");
                clCode = oColumn;
                oColumn.DataBind.Bind("JobDesg", "C");

                oColumn = oColumns.Item("cl_Name");
                clName = oColumn;
                oColumn.DataBind.Bind("JobDesg", "N");

                oColumn = oColumns.Item("cl_Min_Exp");
                clMinExp = oColumn;
                oColumn.DataBind.Bind("JobDesg", "ME");

                oColumn = oColumns.Item("cl_Pr_Desg");
                clPrntDesg = oColumn;
                oColumn.DataBind.Bind("JobDesg", "PD");

                oColumn = oColumns.Item("cl_Job_Pos");
                clJobPos = oColumn;
                oColumn.DataBind.Bind("JobDesg", "JP");
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
            (oMat.Columns.Item("cl_No").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
        }
        private void FillParentDesignationCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstDesignation);
                var Data = from v in dbHrPayroll.MstDesignation select v;
                foreach (var v in Data)
                {
                    clPrntDesg.ValidValues.Add(v.Name, v.Description);
                }
                oColumn.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillJobPostionCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstPosition);
                var Data = from v in dbHrPayroll.MstPosition select v;
                foreach (var v in Data)
                {
                    clJobPos.ValidValues.Add(v.Name, v.Description);
                }
                oColumn.DisplayDesc = true;
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
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstJobDesignation);
                var Data = from n in dbHrPayroll.MstJobDesignation select n;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var v in Data)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("I", i, v.Id);
                    oDBDataTable.SetValue("C", i, v.Code);
                    oDBDataTable.SetValue("N", i, v.Name);
                    oDBDataTable.SetValue("ME", i, v.MinExperiance);
                    oDBDataTable.SetValue("PD", i,  v.ParentDesignation);
                    oDBDataTable.SetValue("JP", i,  v.JobPosition);
                    i += 1;
                }
                oMat.LoadFromDataSource();
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
        private void ValidateandSave(ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                string Code = "", Name = "", MinExp = "", PrntDesg = "", JobPos = "";
                for (int i = 1; i < oMat.RowCount; i++)
                {
                    Code = (oMat.Columns.Item("cl_Code").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Name = (oMat.Columns.Item("cl_Name").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    MinExp = (oMat.Columns.Item("cl_Min_Exp").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    PrntDesg = (oMat.Columns.Item("cl_Pr_Desg").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    JobPos = (oMat.Columns.Item("cl_Job_Pos").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;

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
                    else if (MinExp.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullMinExp"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (PrntDesg.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullPrntDesg"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (JobPos.Trim().Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullJobPos"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                    else if (Code.Equals(oDBDataTable.GetValue("C", i - 1)) && Name.Equals(oDBDataTable.GetValue("N", i - 1)) &&
                        int.Parse(MinExp) == oDBDataTable.GetValue("ME", i - 1) && PrntDesg.Equals(oDBDataTable.GetValue("PD", i - 1)) &&
                        JobPos.Equals(oDBDataTable.GetValue("JP", i - 1)))
                    {
                        continue;
                    }
                    else
                    {
                        UpdateExistingRecord(Code, Name, int.Parse(MinExp), PrntDesg, JobPos, oDBDataTable.GetValue("I", i - 1));
                    }
                }

                Code = (oMat.Columns.Item("cl_Code").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                Name = (oMat.Columns.Item("cl_Name").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                MinExp = (oMat.Columns.Item("cl_Min_Exp").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value;
                PrntDesg = (oMat.Columns.Item("cl_Pr_Desg").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.ComboBox).Value;
                JobPos = (oMat.Columns.Item("cl_Job_Pos").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.ComboBox).Value;

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
                else if (!Code.Equals("") && MinExp.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullMinExp"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && PrntDesg.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullPrntDesg"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (!Code.Equals("") && JobPos.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullJobPos"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else
                {
                    oForm.Freeze(true);
                    oMat.FlushToDataSource();
                    AddRowToDataBase(oDBDataTable.GetValue("C", oMat.RowCount - 1).ToString(), oDBDataTable.GetValue("N", oMat.RowCount - 1).ToString(),
                       oDBDataTable.GetValue("ME", oMat.RowCount - 1), oDBDataTable.GetValue("PD", oMat.RowCount - 1), oDBDataTable.GetValue("JP", oMat.RowCount - 1));
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
        private void AddRowToDataBase(string Code, string Name, int MinExp, string PrntDesg, string JobPos)
        {
            try
            {
                JobDesg = new MstJobDesignation();
                JobDesg.Code = Code;
                JobDesg.Name = Name;
                JobDesg.MinExperiance = MinExp;
                JobDesg.ParentDesignation = PrntDesg;
                JobDesg.JobPosition = JobPos;
                JobDesg.CreateDate = DateTime.Now;
                JobDesg.UserId = oCompany.UserSignature.ToString();
                dbHrPayroll.MstJobDesignation.InsertOnSubmit(JobDesg);
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateExistingRecord(string Code, string Name, int MinExp, string PrntDesg, string JobPos, int Id)
        {
            try
            {
                JobDesg = (from v in dbHrPayroll.MstJobDesignation where v.Id == Id select v).Single();
                JobDesg.Code = Code;
                JobDesg.Name = Name;
                JobDesg.MinExperiance = MinExp;
                JobDesg.ParentDesignation = PrntDesg;
                JobDesg.JobPosition = JobPos;
                JobDesg.UpdateDate = DateTime.Now;
                JobDesg.UserId = oCompany.UserSignature.ToString();
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
                MstJobDesignation Record = (from v in dbHrPayroll.MstJobDesignation where v.Id == ID select v).Single();
                dbHrPayroll.MstJobDesignation.DeleteOnSubmit(Record);
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
    }
}
