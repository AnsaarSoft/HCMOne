using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_CmptncyProf:HRMSBaseForm
    {
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.UserDataSource oUDS_Employee, oUDS_Status, oUDS_Series, oUDS_ProfStatus;
        private SAPbouiCOM.EditText txFirstName, txLastName, txDesignation, txDepartment, txBranch, txDocNum, txDocDate, txtStatus;
        private SAPbouiCOM.ComboBox cbEmpID, cbStatus, cbSeries, cbProfStatus;
        private SAPbouiCOM.Item ItxFirstName, ItxLastName, ItxDesignation, ItxDepartment, ItxBranch,
            ItxDocNum, ItxDocDate, IcbEmpID, IcbStatus, IcbSeries, IcbProfStatus;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.EditText oCell;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column clNo, clID, clIsNew, clCmtncyID, clCmptncyDesc, clCmptncyGrpID;
        private int CurrentCmptncyProfID = 0;

        IEnumerable<TrnsCompetencyProfile> oCollection = null;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            IntitalizeForm();
            FillEmployeeCombo();
            FillCompetencyGroupColumnCombo();
            FillSeriesCombo();
            oUDS_Series.Value = "-1";
            txDocNum.Value = GetNextDocnum().ToString();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            AddBlankRow();
            oForm.Freeze(false);
        }
        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }
        public override void getLastRecord()
        {
            base.getLastRecord();
        }
        public override void getNextRecord()
        {
            base.getNextRecord();
        }
        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }
        public override void AddNewRecord()
        {
            base.AddNewRecord();
            ChangeFormToAddMode();
        }
        public override void fillFields()
        {
            base.fillFields();
            FillDocument();
        }
        private void IntitalizeForm()
        { 
            try
            {
                oUDS_Employee = oForm.DataSources.UserDataSources.Add("EmpID", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbEmpID = oForm.Items.Item("cb_EmpID").Specific;
                IcbEmpID = oForm.Items.Item("cb_EmpID");
                cbEmpID.DataBind.SetBound(true, "", "EmpID");

                oForm.DataSources.UserDataSources.Add("FirstName", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 40);
                txFirstName = oForm.Items.Item("t_FName").Specific;
                ItxFirstName = oForm.Items.Item("t_FName");
                txFirstName.DataBind.SetBound(true, "", "FirstName");

                oForm.DataSources.UserDataSources.Add("LastName", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 40);
                txLastName = oForm.Items.Item("t_LName").Specific;
                ItxLastName = oForm.Items.Item("t_LName");
                txLastName.DataBind.SetBound(true, "", "LastName");

                oForm.DataSources.UserDataSources.Add("Desg", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txDesignation = oForm.Items.Item("t_Desg").Specific;
                ItxDesignation = oForm.Items.Item("t_Desg");
                txDesignation.DataBind.SetBound(true, "", "Desg");

                oForm.DataSources.UserDataSources.Add("Dept", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txDepartment = oForm.Items.Item("t_Dept").Specific;
                ItxDepartment = oForm.Items.Item("t_Dept");
                txDepartment.DataBind.SetBound(true, "", "Dept");

                oForm.DataSources.UserDataSources.Add("Branch", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txBranch = oForm.Items.Item("t_Branch").Specific;
                ItxBranch = oForm.Items.Item("t_Branch");
                txBranch.DataBind.SetBound(true, "", "Branch");

                oUDS_Status = oForm.DataSources.UserDataSources.Add("Status", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtStatus = oForm.Items.Item("tx_Status").Specific;
                IcbStatus = oForm.Items.Item("tx_Status");
                txtStatus.DataBind.SetBound(true, "", "Status");

                oUDS_Series = oForm.DataSources.UserDataSources.Add("Series", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbSeries = oForm.Items.Item("cb_Series").Specific;
                IcbSeries = oForm.Items.Item("cb_Series");
                cbSeries.DataBind.SetBound(true, "", "Series");

                oForm.DataSources.UserDataSources.Add("DocNum", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txDocNum = oForm.Items.Item("t_DocNum").Specific;
                ItxDocNum = oForm.Items.Item("t_DocNum");
                txDocNum.DataBind.SetBound(true, "", "DocNum");

                oForm.DataSources.UserDataSources.Add("DocDate", SAPbouiCOM.BoDataType.dt_DATE);
                txDocDate = oForm.Items.Item("t_DocDate").Specific;
                ItxDocDate = oForm.Items.Item("t_DocDate");
                txDocDate.DataBind.SetBound(true, "", "DocDate");

                oUDS_ProfStatus = oForm.DataSources.UserDataSources.Add("ProfStatus", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 10);
                cbProfStatus = oForm.Items.Item("cb_PStatus").Specific;
                IcbProfStatus = oForm.Items.Item("cb_PStatus");
                cbProfStatus.DataBind.SetBound(true, "", "ProfStatus");

                oDBDataTable = oForm.DataSources.DataTables.Add("CmptncyProfDetail");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("IsNew", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                oDBDataTable.Columns.Add("CmptncyId", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric ,6);
                oDBDataTable.Columns.Add("CmptncyDesc", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 250);
                oDBDataTable.Columns.Add("CmptncyGrpId", SAPbouiCOM.BoFieldsType.ft_Integer);

                oMat = oForm.Items.Item("Mat").Specific;
                oColumns = oMat.Columns;

                clNo = oColumns.Item("cl_no");
                clNo.DataBind.Bind("CmptncyProfDetail", "No");

                clID = oColumns.Item("cl_ID");
                clID.DataBind.Bind("CmptncyProfDetail", "ID");

                clIsNew = oColumns.Item("cl_IsNew");
                clIsNew.DataBind.Bind("CmptncyProfDetail", "IsNew");

                clCmtncyID = oColumns.Item("cl_CmpID");
                clCmtncyID.DataBind.Bind("CmptncyProfDetail", "CmptncyId");

                clCmptncyDesc = oColumns.Item("cl_CmpDesc");
                clCmptncyDesc.DataBind.Bind("CmptncyProfDetail", "CmptncyDesc");

                clCmptncyGrpID = oColumns.Item("cl_GrpID");
                clCmptncyGrpID.DataBind.Bind("CmptncyProfDetail", "CmptncyGrpId");

                base.fillCombo("PerApprsl_Status", cbProfStatus);
                IcbProfStatus.DisplayDesc = true;
                GetData();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillEmployeeCombo()
        {
            try
            {
                String strQuery = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName FROM dbo.CfgPerformancePeriod INNER JOIN dbo.MstEmployee ON dbo.CfgPerformancePeriod.EmpID = dbo.MstEmployee.ID";
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(strQuery);
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                //var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                foreach (var Record in Records)
                {
                    String EMPNAME = Record.FirstName + " " + Record.MiddleName + " " + Record.LastName;
                    cbEmpID.ValidValues.Add(Record.ID.ToString(), EMPNAME);
                }
                IcbEmpID.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillEmployeeCombo Error : "+ ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillCompetencyGroupColumnCombo()
        {
            try
            {
                var Records = from v in dbHrPayroll.MstCompetencyGroup select v;
                foreach (var Record in Records)
                {
                    clCmptncyGrpID.ValidValues.Add(Record.ID.ToString(), Record.GroupID);
                }
                clCmptncyGrpID.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillSeriesCombo()
        {
            try
            {
                cbSeries.ValidValues.Add("-1", "Primary");
                IcbSeries.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public int GetNextDocnum()
        {
            try
            {
                int MaxDocnum = dbHrPayroll.TrnsCompetencyProfile.Max(x => x.ID);
                return MaxDocnum + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }
        private void AddBlankRow()
        {
            try
            {
                oDBDataTable.Rows.Clear();
                oMat.AddRow(1, oMat.RowCount + 1);
                (oMat.Columns.Item("cl_no").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
                (oMat.Columns.Item("cl_IsNew").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = "Y";
                oMat.FlushToDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        //public override void getFirstRecord()
        //{
        //    try
        //    {
        //        int Count = (from v in dbHrPayroll.TrnsCompetencyProfile select v).Count();
        //        if (Count == 0)
        //        {
        //            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            TrnsCompetencyProfile Head = (from v in dbHrPayroll.TrnsCompetencyProfile orderby v.ID ascending select v).First();
        //            GetDataFromDataSource(Head);
        //            AddBlankRow();
        //            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        //            oForm.Freeze(false);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //    }
        //}
        //public override void getPreviouRecord()
        //{
        //    try
        //    {
        //        int Count = (from v in dbHrPayroll.TrnsCompetencyProfile where v.ID < CurrentCmptncyProfID select v).Count();
        //        if (Count == 0)
        //        {
        //            getFirstRecord();
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            TrnsCompetencyProfile Head = (from v in dbHrPayroll.TrnsCompetencyProfile where v.ID < CurrentCmptncyProfID orderby v.ID descending select v).First();
        //            GetDataFromDataSource(Head);
        //            AddBlankRow();
        //            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        //            oForm.Freeze(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //    }
        //}
        //public override void getNextRecord()
        //{
        //    try
        //    {
        //        int Count = (from v in dbHrPayroll.TrnsCompetencyProfile where v.ID > CurrentCmptncyProfID orderby v.ID ascending select v).Count();
        //        if (Count == 0)
        //        {
        //            getLastRecord();
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            TrnsCompetencyProfile Head = (from v in dbHrPayroll.TrnsCompetencyProfile where v.ID > CurrentCmptncyProfID orderby v.ID ascending select v).First();
        //            GetDataFromDataSource(Head);
        //            AddBlankRow();
        //            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        //            oForm.Freeze(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //    }
        //}
        //public override void getLastRecord()
        //{
        //    try
        //    {
        //        int Count = (from v in dbHrPayroll.TrnsCompetencyProfile select v).Count();
        //        if (Count == 0)
        //        {
        //            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            TrnsCompetencyProfile Head = (from v in dbHrPayroll.TrnsCompetencyProfile orderby v.ID descending select v).First();
        //            GetDataFromDataSource(Head);
        //            AddBlankRow();
        //            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        //            oForm.Freeze(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //    }
        //}
        private void ChangeFormToAddMode()
        {
            try
            {
                oForm.Freeze(true);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                CurrentCmptncyProfID = 0;
                oUDS_Employee.Value = "";
                txFirstName.Value = "";
                txLastName.Value = "";
                txDesignation.Value = "";
                txDepartment.Value = "";
                txBranch.Value = "";
                oUDS_Status.Value = "";
                oUDS_Series.Value = "-1";
                txDocNum.Value = GetNextDocnum().ToString();
                txDocDate.Value = "";
                oUDS_ProfStatus.Value = "";
                oMat.Clear();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetDataFromDataSource(TrnsCompetencyProfile CmptncyProfHead)
        {
            try
            {
                oUDS_Employee.Value = (CmptncyProfHead.EmpID.ToString() == null ? "" : CmptncyProfHead.EmpID.ToString());
                txFirstName.Value = (CmptncyProfHead.FirstName == null ? "" : CmptncyProfHead.FirstName);
                txLastName.Value = (CmptncyProfHead.LastName == null ? "" : CmptncyProfHead.LastName);
                txDesignation.Value = (CmptncyProfHead.Designation == null ? "" : CmptncyProfHead.Designation);
                txDepartment.Value = (CmptncyProfHead.Department == null ? "" : CmptncyProfHead.Department);
                txBranch.Value = (CmptncyProfHead.Branch == null ? "" : CmptncyProfHead.Branch);
                
                oUDS_Series.Value = (CmptncyProfHead.Series == null ? "-1" : CmptncyProfHead.Series.ToString());
                txDocNum.Value = (CmptncyProfHead.DocNum.ToString() == null ? "" : CmptncyProfHead.DocNum.ToString());
                txDocDate.Value = "";
                txDocDate.Value = ((DateTime)CmptncyProfHead.DocDate).ToString("yyyyMMdd");
                oUDS_ProfStatus.Value = (CmptncyProfHead.DocStatusID == null ? "" : CmptncyProfHead.DocStatusID);
                oUDS_Status.Value = CmptncyProfHead.DocStatusID == null ? "Draft" : CmptncyProfHead.DocStatusID;
                var Records = from v in CmptncyProfHead.TrnsCompetencyProfileDetail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var Record in Records)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("ID", i, Record.ID);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("CmptncyId", i, Record.Code);
                    oDBDataTable.SetValue("CmptncyDesc", i, Record.Description);
                    oDBDataTable.SetValue("CmptncyGrpId", i, Record.CGID);
                    i++;
                }                
                oMat.LoadFromDataSource();
                CurrentCmptncyProfID = CmptncyProfHead.ID;
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "cb_EmpID":
                        dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                        MstEmployee Employee = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbEmpID.Value.ToString()) select v).Single();
                        txFirstName.Value = (Employee.FirstName == null ? "" : Employee.FirstName);
                        txLastName.Value = (Employee.LastName == null ? "" : Employee.LastName);
                        txDesignation.Value = (Employee.DesignationName == null ? "" : Employee.DesignationName);
                        txDepartment.Value = (Employee.DepartmentName == null ? "" : Employee.DepartmentName);
                        txBranch.Value = (Employee.BranchName == null ? "" : Employee.BranchName);
                        break;
                    case "cb_Series":
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etBeforeValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "t_DocDate":
                        int CompanyDate = int.Parse(oCompany.GetCompanyDate().ToString("yyyyMMdd"));
                        int Date = int.Parse(txDocDate.Value == "" ? "0" : txDocDate.Value);
                        if (Date > CompanyDate)
                        {
                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DateCheck"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            BubbleEvent = false;
                            return;
                        }
                        break;
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {
                                case "cl_CmpID":
                                    var ID = (oMat.Columns.Item("cl_CmpID").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (ID.Equals("") && pVal.Row != oMat.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        BubbleEvent = false;
                                        return;
                                    }

                                    for (int i = 1; i <= oMat.RowCount; i++)
                                    {
                                        oCell = oMat.Columns.Item("cl_CmpID").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                        if (i == pVal.Row)
                                            continue;
                                        else if (ID == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program .objHrmsUI .getStrMsg ("Err_ExistCode"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                            BubbleEvent = false;
                                            return;
                                        }
                                    }
                                    break;
                                case "cl_CmpDesc":
                                    var Desc = (oMat.Columns.Item("cl_CmpDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Trim().ToLower();
                                    if (Desc.Equals("") && pVal.Row != oMat.RowCount)
                                    {
                                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        BubbleEvent = false;
                                        return;
                                    }

                                    for (int i = 1; i <= oMat.RowCount; i++)
                                    {
                                        oCell = oMat.Columns.Item("cl_CmpDesc").Cells.Item(i).Specific as SAPbouiCOM.EditText;
                                        if (i == pVal.Row)
                                            continue;
                                        else if (Desc == oCell.Value.Trim().ToLower())
                                        {
                                            oApplication.StatusBar.SetText(Program .objHrmsUI .getStrMsg ("Err_ExistDesc"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                switch (pVal.ItemUID)
                {
                    case "Mat":
                        {
                            if ((pVal.ColUID.Equals("cl_CmpID") || pVal.ColUID.Equals("cl_CmpDesc") || pVal.ColUID.Equals("cl_GrpID")) && pVal.Row == oMat.RowCount)
                            {
                                string CmtncyID = ((oMat.Columns.Item("cl_CmpID").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value).ToString();
                                string CpmtncyDesc = ((oMat.Columns.Item("cl_CmpDesc").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value).ToString();
                                string CpmtncyGrp = ((oMat.Columns.Item("cl_GrpID").Cells.Item(pVal.Row).Specific as SAPbouiCOM.ComboBox).Value).ToString();
                                if (!CmtncyID.Equals("") && !CpmtncyDesc.Equals("") && !CpmtncyGrp.Equals(""))
                                {
                                    AddBlankRow();
                                }
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
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                            return;
                        else
                            ValidateAndSave(ref pVal, ref BubbleEvent);
                        break;
                    case "btn_first":
                        getFirstRecord();
                        break;
                    case "btn_prev":
                        getPreviouRecord();
                        break;
                    case "btn_next":
                        getNextRecord();
                        break;
                    case "btn_last":
                        getLastRecord();
                        break;
                    case "btn_new":
                        ChangeFormToAddMode();
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void ValidateAndSave(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                if (cbEmpID.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullEmployee"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                //else if (cbSeries.Value.Equals(""))
                //{
                //    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg(" Err_NullSeries"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    BubbleEvent = false;
                //    return;
                //}
                else if (txDocDate.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDocDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbProfStatus.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullProfileStatus"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                string CmptncyID, CmptncyDesc, CmptncyGrpId;
                for (int i = 1; i <= oMat.RowCount; i++)
                {
                    CmptncyID = (oMat.Columns.Item("cl_CmpID").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    CmptncyDesc = (oMat.Columns.Item("cl_CmpDesc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    CmptncyGrpId = (oMat.Columns.Item("cl_GrpID").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (i != oMat.RowCount)
                    {
                        if (CmptncyID.Equals("") && CmptncyDesc.Equals("") && CmptncyGrpId.Equals(""))
                        {
                            oApplication.StatusBar.SetText("Mmandatory Fields are Missing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            BubbleEvent = false;
                            return;
                        }
                    }
                    else
                    {
                        if ((CmptncyID.Equals("") || CmptncyDesc.Equals("") || CmptncyGrpId.Equals(""))
                            && !(CmptncyID.Equals("") && CmptncyDesc.Equals("") && CmptncyGrpId.Equals("")))
                        {
                            oApplication.StatusBar.SetText("Mmandatory Fields are Missing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            BubbleEvent = false;
                            return;
                        }
                    }
                }
                switch (oForm.Mode)
                {
                    case SAPbouiCOM.BoFormMode.fm_ADD_MODE :
                        AddRecordToDatabase();
                        break;
                    case SAPbouiCOM.BoFormMode.fm_UPDATE_MODE :
                        oForm.Freeze(true);
                        UpdateRecord();
                        GetDataFromDataSource((from v in dbHrPayroll.TrnsCompetencyProfile where v.ID == CurrentCmptncyProfID select v).Single());
                        AddBlankRow();
                        oForm.Freeze(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void AddRecordToDatabase()
        {
            try
            {
                TrnsCompetencyProfile Head = new TrnsCompetencyProfile();
                Head.EmpID = int.Parse(cbEmpID.Value);
                Head.FirstName = txFirstName.Value;
                Head.LastName = txLastName.Value;
                Head.Designation = txDesignation.Value;
                Head.Department = txDepartment.Value;
                Head.Branch = txBranch.Value;
                Head.DocType = 16;

                //Head.Series = int.Parse(cbSeries.Value);
                Head.DocNum = int.Parse(txDocNum.Value);
                Head.DocDate = DateTime.ParseExact(txDocDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                Head.DocStatusID = cbProfStatus.Value;
                oMat.FlushToDataSource();
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    TrnsCompetencyProfileDetail Detail = new TrnsCompetencyProfileDetail();
                    Detail.Code = oDBDataTable.GetValue("CmptncyId", i);
                    Detail.Description = oDBDataTable.GetValue("CmptncyDesc", i);
                    Detail.CGID = oDBDataTable.GetValue("CmptncyGrpId", i);
                    Detail.CreateDate = DateTime.Now;
                    Detail.UserId = oCompany.UserSignature.ToString();
                    Detail.UpdatedBy = oCompany.UserName;
                    Head.TrnsCompetencyProfileDetail.Add(Detail);
                }
                Head.CreateDate = DateTime.Now;
                Head.UserId = oCompany.UserSignature.ToString();
                Head.UpdatedBy = oCompany.UserName;
                dbHrPayroll.TrnsCompetencyProfile.InsertOnSubmit(Head);
                dbHrPayroll.SubmitChanges();
                GetData();
                ChangeFormToAddMode();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void UpdateRecord()
        {
            try
            {
                TrnsCompetencyProfile Head = (from v in dbHrPayroll.TrnsCompetencyProfile where v.ID == CurrentCmptncyProfID select v).Single();
                Head.EmpID = int.Parse(cbEmpID.Value);
                Head.FirstName = txFirstName.Value;
                Head.LastName = txLastName.Value;
                Head.Designation = txDesignation.Value;
                Head.Department = txDepartment.Value;
                Head.Branch = txBranch.Value;
                //Head.Series = int.Parse(cbSeries.Value);
                //Head.DocNum = int.Parse(txDocNum.Value);
                Head.DocDate = DateTime.ParseExact(txDocDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                Head.DocStatusID = cbProfStatus.Value;
                oMat.FlushToDataSource();
                int DetailId = 0;
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    if (oDBDataTable.GetValue("IsNew", i) == "Y")
                    {
                        TrnsCompetencyProfileDetail Detail = new TrnsCompetencyProfileDetail();
                        Detail.Code = oDBDataTable.GetValue("CmptncyId", i);
                        Detail.Description = oDBDataTable.GetValue("CmptncyDesc", i);
                        Detail.CGID = oDBDataTable.GetValue("CmptncyGrpId", i);
                        Detail.CreateDate = DateTime.Now;
                        Detail.UserId = oCompany.UserSignature.ToString();
                        Detail.UpdatedBy = oCompany.UserName;
                        Head.TrnsCompetencyProfileDetail.Add(Detail);
                    }
                    else
                    {
                        DetailId = oDBDataTable.GetValue("ID", i);
                        TrnsCompetencyProfileDetail Detail = (from v in dbHrPayroll.TrnsCompetencyProfileDetail where v.ID == DetailId select v).Single();
                        Detail.Code = oDBDataTable.GetValue("CmptncyId", i);
                        Detail.Description = oDBDataTable.GetValue("CmptncyDesc", i);
                        Detail.CGID = oDBDataTable.GetValue("CmptncyGrpId", i);
                        Detail.UpdateDate = DateTime.Now;
                        Detail.UserId = oCompany.UserSignature.ToString();
                        Detail.UpdatedBy = oCompany.UserName;
                    }
                }
                Head.UpdateDate = DateTime.Now;
                Head.UserId = oCompany.UserSignature.ToString();
                Head.UpdatedBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetData()
        {
            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.TrnsCompetencyProfile select a;
            Int32 i = 0;
            foreach (TrnsCompetencyProfile oDoc in oCollection)
            {
                CodeIndex.Add(oDoc.ID, i);
                i++;
            }
            totalRecord = i;
        }
        private void FillDocument()
        {
            try
            {
                oForm.Freeze(true);
                TrnsCompetencyProfile Head = null;
                Head = oCollection.ElementAt<TrnsCompetencyProfile>(currentRecord);
                GetDataFromDataSource(Head);
                AddBlankRow();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillDocument Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }
}
