using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_PerfPrdConf:HRMSBaseForm
    {
        #region Variable
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.UserDataSource oUDS_Employee, oUDS_FromDate, oUDS_ToDate;
        private SAPbouiCOM.EditText txEmpName, txLocation, txDept, txFromDate, txToDate;
        private SAPbouiCOM.ComboBox cbEmpID;
        private SAPbouiCOM.CheckBox chAllEmplyee;
        private SAPbouiCOM.Item ItxEmpName, ItxLocation, ItxDept, ItxFromDate, ItxToDate, IcbEmpID, IchAllEmplyee;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column clNo, clID, clIsNew, clPlanNo, clPlanDate, clFromDate, clToDate, clBranch, clName, clDesg, clDept;
        private int CurrentCfgPerfPrdID = 0;

        IEnumerable<CfgPerformancePeriod> oCollection = null;
        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitializeForm();
            FillEmployeeCombo();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            AddBlankRow();
            oForm.Freeze(false);
        }
        
        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "cb_EmpID":
                        {
                            oForm.Freeze(true);
                            dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                            MstEmployee Employee = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbEmpID.Value.ToString()) select v).Single();
                            txEmpName.Value = (Employee.FirstName == null ? "" : Employee.FirstName + " " + Employee.LastName);
                            txLocation.Value = (Employee.LocationName == null ? "" : Employee.LocationName);
                            txDept.Value = (Employee.DepartmentName == null ? "" : Employee.DepartmentName);
                            oMat.Clear();
                            AddBlankRow();
                            FillPlanNoColumnCombo(Employee.ID);
                            oForm.Freeze(false);
                        }
                        break;
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {
                                case "cl_PNo":
                                    {
                                        var PlanNo = (oMat.Columns.Item("cl_PNo").Cells.Item(pVal.Row).Specific as SAPbouiCOM.ComboBox).Value;
                                        if (!PlanNo.Equals(""))
                                        {
                                            oForm.Freeze(true);
                                            if (pVal.Row == oMat.RowCount)
                                            {
                                                AddBlankRow();
                                            }
                                            dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformancePlan);
                                            TrnsPerformancePlan PerfPlan = (from v in dbHrPayroll.TrnsPerformancePlan where v.PlanNo == int.Parse(PlanNo) select v).Single();
                                            oDBDataTable.SetValue("PNo", pVal.Row - 1, PerfPlan.PlanNo);
                                            oDBDataTable.SetValue("PDate", pVal.Row - 1, PerfPlan.PlanDate);
                                            oDBDataTable.SetValue("FromDate", pVal.Row - 1, PerfPlan.FromDate);
                                            oDBDataTable.SetValue("ToDate", pVal.Row - 1, PerfPlan.ToDate);
                                            oDBDataTable.SetValue("Branch", pVal.Row - 1, PerfPlan.EmpBranch);
                                            oDBDataTable.SetValue("Name", pVal.Row - 1, PerfPlan.MstEmployee.FirstName + " " + PerfPlan.MstEmployee.LastName);
                                            oDBDataTable.SetValue("Desg", pVal.Row - 1, PerfPlan.EmpDesignation);
                                            oDBDataTable.SetValue("Dept", pVal.Row - 1, PerfPlan.EmpDepartment);
                                            oMat.LoadFromDataSource();
                                            oForm.Freeze(false);
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
        
        public override void etBeforeValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "t_FDate":
                    case "t_TDate":
                        {
                            string FromDate = txFromDate.Value, ToDate = txToDate.Value;
                            if (FromDate.Equals("") || ToDate.Equals(""))
                                return;
                            else if (int.Parse(ToDate) < int.Parse(FromDate))
                            {
                                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DateComparison"), SAPbouiCOM.BoMessageTime.bmt_Short);
                                BubbleEvent = false;
                                return;
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
        
        public override void etAfterGetFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {
                                case "cl_PNo":
                                    {
                                        var PNo = (oMat.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row).Specific as SAPbouiCOM.ComboBox).Value;
                                        for (int i = 1; i <= oMat.RowCount; i++)
                                        {
                                            var value = (oMat.Columns.Item(pVal.ColUID).Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                                            if (pVal.Row == i)
                                                continue;
                                            else if (PNo.Equals(value))
                                            {
                                                oApplication.StatusBar.SetText("Plan No Already Exist", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "ch_AllEmp":
                        {
                            bool AllEmployee = chAllEmplyee.Checked;
                            oForm.Freeze(true);
                            if (AllEmployee)
                            {
                                oUDS_Employee.Value = "";
                                txEmpName.Value = "";
                                txDept.Value = "";
                                oForm.Items.Item("t_FDate").Click();
                                IcbEmpID.Enabled = false;
                                FillPlanNoColumnCombo(-1);
                                oMat.Clear();
                                AddBlankRow();
                            }
                            else
                            {
                                IcbEmpID.Enabled = true;
                                int Count = clPlanNo.ValidValues.Count;
                                for (int i = 1; i <= Count; i++)
                                {
                                    clPlanNo.ValidValues.Remove(Count - i, SAPbouiCOM.BoSearchKey.psk_Index);
                                }
                                oMat.Clear();
                                AddBlankRow();
                            }
                            oForm.Freeze(false);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void fillFields()
        {
            base.fillFields();
            FillDocument();
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

        #endregion

        #region User Functions
        
        private void InitializeForm()
        {
            try
            {
                oUDS_Employee = oForm.DataSources.UserDataSources.Add("EmpID", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbEmpID = oForm.Items.Item("cb_EmpID").Specific;
                IcbEmpID = oForm.Items.Item("cb_EmpID");
                cbEmpID.DataBind.SetBound(true, "", "EmpID");

                oForm.DataSources.UserDataSources.Add("EmpName", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 40);
                txEmpName = oForm.Items.Item("t_EmpName").Specific;
                ItxEmpName = oForm.Items.Item("t_EmpName");
                txEmpName.DataBind.SetBound(true, "", "EmpName");

                oForm.DataSources.UserDataSources.Add("Location", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txLocation = oForm.Items.Item("t_Lct").Specific;
                ItxLocation = oForm.Items.Item("t_Lct");
                txLocation.DataBind.SetBound(true, "", "Location");

                oForm.DataSources.UserDataSources.Add("Dept", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txDept = oForm.Items.Item("t_Dept").Specific;
                ItxDept = oForm.Items.Item("t_Dept");
                txDept.DataBind.SetBound(true, "", "Dept");

                oForm.DataSources.UserDataSources.Add("AllEmp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chAllEmplyee = oForm.Items.Item("ch_AllEmp").Specific;
                IchAllEmplyee = oForm.Items.Item("ch_AllEmp");
                chAllEmplyee.DataBind.SetBound(true, "", "AllEmp");

                oUDS_FromDate = oForm.DataSources.UserDataSources.Add("FDate", SAPbouiCOM.BoDataType.dt_DATE);
                txFromDate = oForm.Items.Item("t_FDate").Specific;
                ItxFromDate = oForm.Items.Item("t_FDate");
                txFromDate.DataBind.SetBound(true, "", "FDate");

                oUDS_ToDate = oForm.DataSources.UserDataSources.Add("TDate", SAPbouiCOM.BoDataType.dt_DATE);
                txToDate = oForm.Items.Item("t_TDate").Specific;
                ItxToDate = oForm.Items.Item("t_TDate");
                txToDate.DataBind.SetBound(true, "", "TDate");

                oDBDataTable = oForm.DataSources.DataTables.Add("PerfAprslDetail");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("IsNew", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                oDBDataTable.Columns.Add("PNo", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("PDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("FromDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("ToDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("Branch", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("Name", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("Desg", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("Dept", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);

                oMat = oForm.Items.Item("Mat").Specific;
                oColumns = oMat.Columns;

                clNo = oColumns.Item("cl_no");
                clNo.DataBind.Bind("PerfAprslDetail", "No");

                clID = oColumns.Item("cl_ID");
                clID.DataBind.Bind("PerfAprslDetail", "ID");

                clIsNew = oColumns.Item("cl_IsNew");
                clIsNew.DataBind.Bind("PerfAprslDetail", "IsNew");

                clPlanNo = oColumns.Item("cl_PNo");
                clPlanNo.DataBind.Bind("PerfAprslDetail", "PNo");

                clPlanDate = oColumns.Item("cl_PDate");
                clPlanDate.DataBind.Bind("PerfAprslDetail", "PDate");

                clFromDate = oColumns.Item("cl_FDate");
                clFromDate.DataBind.Bind("PerfAprslDetail", "FromDate");

                clToDate = oColumns.Item("cl_TDate");
                clToDate.DataBind.Bind("PerfAprslDetail", "ToDate");

                clBranch = oColumns.Item("cl_Branch");
                clBranch.DataBind.Bind("PerfAprslDetail", "Branch");

                clName = oColumns.Item("cl_Name");
                clName.DataBind.Bind("PerfAprslDetail", "Name");

                clDesg = oColumns.Item("cl_Desg");
                clDesg.DataBind.Bind("PerfAprslDetail", "Desg");

                clDept = oColumns.Item("cl_Dept");
                clDept.DataBind.Bind("PerfAprslDetail", "Dept");

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
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                String querycheck = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.EmpID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName
                                      FROM dbo.TrnsPerformancePlan INNER JOIN dbo.MstEmployee ON dbo.TrnsPerformancePlan.EmpID = dbo.MstEmployee.ID
                                      ORDER BY dbo.MstEmployee.EmpID";
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(querycheck);
                //var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                foreach (var Record in Records)
                {
                    String EmpName = Record.EmpID + " : " + Record.FirstName + " " + Record.MiddleName + " " + Record.LastName;
                    cbEmpID.ValidValues.Add(Record.ID.ToString(),  EmpName);
                }
                IcbEmpID.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception Function : FillEmployeeCombo Error: " + ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillPlanNoColumnCombo(int EmpID)
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformancePlan);
                IQueryable<TrnsPerformancePlan> Records;

                if (EmpID == -1)
                    Records = from v in dbHrPayroll.TrnsPerformancePlan select v;
                else
                    Records = from v in dbHrPayroll.TrnsPerformancePlan where v.EmpID == EmpID select v;

                int Count = clPlanNo.ValidValues.Count;
                for (int i = 1; i <= Count; i++)
                {
                    clPlanNo.ValidValues.Remove(Count - i, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                foreach (var Record in Records)
                {
                    clPlanNo.ValidValues.Add(Record.PlanNo.ToString(), Record.MstEmployee.FirstName + " " + Record.MstEmployee.LastName);
                }
                clPlanNo.DisplayDesc = false;
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
                (oMat.Columns.Item("cl_no").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = (oMat.RowCount).ToString();
                (oMat.Columns.Item("cl_IsNew").Cells.Item(oMat.RowCount).Specific as SAPbouiCOM.EditText).Value = "Y";
                oMat.FlushToDataSource();
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        ////public override void getFirstRecord()
        ////{
        ////    try
        ////    {
        ////        int Count = (from v in dbHrPayroll.CfgPerformancePeriod select v).Count();
        ////        if (Count == 0)
        ////        {
        ////            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        ////        }
        ////        else
        ////        {
        ////            oForm.Freeze(true);
        ////            CfgPerformancePeriod Head = (from v in dbHrPayroll.CfgPerformancePeriod orderby v.Id ascending select v).First();
        ////            Head = (from v in dbHrPayroll.CfgPerformancePeriod orderby v.Id ascending select v).First();
        ////            GetDataFromDataSource(Head);
        ////            AddBlankRow();
        ////            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        ////            oForm.Freeze(false);
        ////        }

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        ////    }
        ////}
        
        //public override void getPreviouRecord()
        //{
        //    try
        //    {
        //        int Count = (from v in dbHrPayroll.CfgPerformancePeriod where v.Id < CurrentCfgPerfPrdID select v).Count();
        //        if (Count == 0)
        //        {
        //            getFirstRecord();
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            CfgPerformancePeriod Head = (from v in dbHrPayroll.CfgPerformancePeriod where v.Id < CurrentCfgPerfPrdID orderby v.Id descending select v).First();
        //            GetDataFromDataSource(Head);
        //            AddBlankRow();
        //            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
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
        //        int Count = (from v in dbHrPayroll.CfgPerformancePeriod where v.Id > CurrentCfgPerfPrdID orderby v.Id ascending select v).Count();
        //        if (Count == 0)
        //        {
        //            getLastRecord();
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            CfgPerformancePeriod Head = (from v in dbHrPayroll.CfgPerformancePeriod where v.Id > CurrentCfgPerfPrdID orderby v.Id ascending select v).First();
        //            GetDataFromDataSource(Head);
        //            AddBlankRow();
        //            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
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
        //        int Count = (from v in dbHrPayroll.CfgPerformancePeriod select v).Count();
        //        if (Count == 0)
        //        {
        //            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        //        }
        //        else
        //        {
        //            oForm.Freeze(true);
        //            CfgPerformancePeriod Head = (from v in dbHrPayroll.CfgPerformancePeriod orderby v.Id descending select v).First();
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
                CurrentCfgPerfPrdID = 0;
                oUDS_Employee.Value = "";
                txEmpName.Value = "";
                txLocation.Value = "";
                txDept.Value = "";
                chAllEmplyee.Checked = false;
                txFromDate.Value = "";
                txToDate.Value = "";
                oMat.Clear();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void GetDataFromDataSource(CfgPerformancePeriod PerfPrdHead)
        {
            try
            {
                FillPlanNoColumnCombo((int)PerfPrdHead.EmpID);
                oUDS_Employee.Value = (PerfPrdHead.EmpID.ToString() == null ? "" : PerfPrdHead.EmpID.ToString());
                var Employee = (from v in dbHrPayroll.MstEmployee where v.ID == PerfPrdHead.EmpID select v).Single();
                txEmpName.Value = Employee.FirstName + " " + Employee.LastName;
                txLocation.Value = (PerfPrdHead.Location == null ? "" : PerfPrdHead.Location);
                txDept.Value = (PerfPrdHead.Department == null ? "" : PerfPrdHead.Department);
                txFromDate.Value = "";
                txToDate.Value = "";
                txFromDate.Value = ((DateTime)PerfPrdHead.FromDate).ToString("yyyyMMdd");
                txToDate.Value = ((DateTime)PerfPrdHead.ToDate).ToString("yyyyMMdd");

                var Records = from v in PerfPrdHead.CfgPerformancePeriodDetail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var Record in Records)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("ID", i, Record.Id);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("PNo", i, Record.PlanNo);
                    oDBDataTable.SetValue("PDate", i, Record.PlanDate);
                    oDBDataTable.SetValue("FromDate", i, Record.FromDate);
                    oDBDataTable.SetValue("ToDate", i, Record.ToDate);
                    oDBDataTable.SetValue("Branch", i, Record.Branch);
                    oDBDataTable.SetValue("Name", i, Record.Name);
                    oDBDataTable.SetValue("Desg", i, Record.Designation);
                    oDBDataTable.SetValue("Dept", i, Record.Department);
                    i++;
                }
                oMat.LoadFromDataSource();
                CurrentCfgPerfPrdID = PerfPrdHead.Id;
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
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
                else if (((from v in dbHrPayroll .CfgPerformancePeriod where v.EmpID == int .Parse (cbEmpID.Value) select v ).Count () == 1 && oForm .Mode ==  SAPbouiCOM.BoFormMode.fm_ADD_MODE) ||
                    ((from v in dbHrPayroll.CfgPerformancePeriod where v.EmpID == int.Parse(cbEmpID.Value) && v.Id != CurrentCfgPerfPrdID select v).Count() == 1 && oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_ExistEmployee"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (txFromDate.Value.Equals("") || txToDate.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (oMat.RowCount == 1)
                {
                    if ((oMat.Columns.Item("cl_PNo").Cells.Item(1).Specific as SAPbouiCOM.ComboBox).Value.Equals(""))
                    {
                        oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullPlanNo"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
                    }
                }
                switch (oForm.Mode)
                {
                    case SAPbouiCOM.BoFormMode.fm_ADD_MODE:
                        AddRecord();
                        break;
                    case SAPbouiCOM.BoFormMode.fm_UPDATE_MODE:
                        oForm.Freeze(true);
                        UpdateRecord();
                        GetDataFromDataSource((from v in dbHrPayroll.CfgPerformancePeriod where v.Id == CurrentCfgPerfPrdID select v).Single());
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
        
        private void AddRecord()
        {
            try 
            {
                CfgPerformancePeriod PerfPrdHead = new CfgPerformancePeriod();
                PerfPrdHead.EmpID = int.Parse(cbEmpID.Value);
                PerfPrdHead.Location = txLocation.Value;
                PerfPrdHead.Department = txDept.Value;
                PerfPrdHead.FromDate = DateTime.ParseExact(txFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPrdHead.ToDate = DateTime.ParseExact(txToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                oMat .FlushToDataSource ();
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    CfgPerformancePeriodDetail PerfPrdDetail = new CfgPerformancePeriodDetail();
                    string checkvalue = Convert.ToString(oDBDataTable.GetValue("PNo", i));
                    TrnsPerformancePlan oPlan = (from a in dbHrPayroll.TrnsPerformancePlan where a.PlanNo.ToString() == checkvalue select a).FirstOrDefault();
                    PerfPrdDetail.TrnsPerformancePlan = oPlan;
                    PerfPrdDetail.PlanDate = oDBDataTable.GetValue("PDate", i);
                    PerfPrdDetail.FromDate = oDBDataTable.GetValue("FromDate", i);
                    PerfPrdDetail.ToDate = oDBDataTable.GetValue("ToDate", i);
                    PerfPrdDetail.Branch = oDBDataTable.GetValue("Branch", i);
                    PerfPrdDetail.Name = oDBDataTable.GetValue("Name", i);
                    PerfPrdDetail.Designation = oDBDataTable.GetValue("Desg", i);
                    PerfPrdDetail.Department = oDBDataTable.GetValue("Dept", i);
                    PerfPrdDetail.CreateDate = DateTime.Now;
                    PerfPrdDetail.UserId = oCompany.UserSignature.ToString();
                    PerfPrdDetail.UpdatedBy = oCompany.UserName;
                    PerfPrdHead.CfgPerformancePeriodDetail.Add(PerfPrdDetail);
                }
                PerfPrdHead.CreateDate = DateTime.Now;
                PerfPrdHead.UserId = oCompany.UserSignature.ToString();
                PerfPrdHead.UpdatedBy = oCompany.UserName;
                dbHrPayroll.CfgPerformancePeriod.InsertOnSubmit(PerfPrdHead);
                dbHrPayroll.SubmitChanges();
                CurrentCfgPerfPrdID = PerfPrdHead.Id;
                ChangeFormToAddMode();
                GetData();
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
                CfgPerformancePeriod PerfPrdHead = (from v in dbHrPayroll.CfgPerformancePeriod where v.Id == CurrentCfgPerfPrdID select v).Single();
                PerfPrdHead.EmpID = int.Parse(cbEmpID.Value);
                PerfPrdHead.Location = txLocation.Value;
                PerfPrdHead.Department = txDept.Value;
                PerfPrdHead.FromDate = DateTime.ParseExact(txFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPrdHead.ToDate = DateTime.ParseExact(txToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                oMat.FlushToDataSource();

                int DetailId = 0;
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    if (oDBDataTable.GetValue("IsNew", i) == "N")
                    {
                        DetailId = oDBDataTable.GetValue("ID", i);
                        CfgPerformancePeriodDetail PerfPrdDetail = (from v in dbHrPayroll.CfgPerformancePeriodDetail where v.Id == DetailId select v).Single();
                        PerfPrdDetail.PlanNo = oDBDataTable.GetValue("PNo", i);
                        PerfPrdDetail.PlanDate = oDBDataTable.GetValue("PDate", i);
                        PerfPrdDetail.FromDate = oDBDataTable.GetValue("FromDate", i);
                        PerfPrdDetail.ToDate = oDBDataTable.GetValue("ToDate", i);
                        PerfPrdDetail.Branch = oDBDataTable.GetValue("Branch", i);
                        PerfPrdDetail.Name = oDBDataTable.GetValue("Name", i);
                        PerfPrdDetail.Designation = oDBDataTable.GetValue("Desg", i);
                        PerfPrdDetail.Department = oDBDataTable.GetValue("Dept", i);
                        PerfPrdDetail.UpdateDate = DateTime.Now;
                        PerfPrdDetail.UserId = oCompany.UserSignature.ToString();
                        PerfPrdDetail.UpdatedBy = oCompany.UserName;
                    }
                    else
                    {
                        CfgPerformancePeriodDetail PerfPrdDetail = new CfgPerformancePeriodDetail();
                        PerfPrdDetail.PlanNo = oDBDataTable.GetValue("PNo", i);
                        PerfPrdDetail.PlanDate = oDBDataTable.GetValue("PDate", i);
                        PerfPrdDetail.FromDate = oDBDataTable.GetValue("FromDate", i);
                        PerfPrdDetail.ToDate = oDBDataTable.GetValue("ToDate", i);
                        PerfPrdDetail.Branch = oDBDataTable.GetValue("Branch", i);
                        PerfPrdDetail.Name = oDBDataTable.GetValue("Name", i);
                        PerfPrdDetail.Designation = oDBDataTable.GetValue("Desg", i);
                        PerfPrdDetail.Department = oDBDataTable.GetValue("Dept", i);
                        PerfPrdDetail.CreateDate = DateTime.Now;
                        PerfPrdDetail.UserId = oCompany.UserSignature.ToString();
                        PerfPrdDetail.UpdatedBy = oCompany.UserName;
                        PerfPrdHead.CfgPerformancePeriodDetail.Add(PerfPrdDetail);
                    }
                }
                PerfPrdHead.CreateDate = DateTime.Now;
                PerfPrdHead.UserId = oCompany.UserSignature.ToString();
                PerfPrdHead.UpdatedBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();
                CurrentCfgPerfPrdID = PerfPrdHead.Id;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDocument()
        {
            try
            {
                oForm.Freeze(true);
                CfgPerformancePeriod Head = null;
                Head = oCollection.ElementAt<CfgPerformancePeriod>(currentRecord);
                GetDataFromDataSource(Head);
                AddBlankRow();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                oForm.Freeze(false);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception Fill Document Error : " + ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.CfgPerformancePeriod select a;
            Int32 i = 0;
            foreach (CfgPerformancePeriod oDoc in oCollection)
            {
                CodeIndex.Add(oDoc.Id, i);
                i++;
            }
            totalRecord = i;
        }
        
        #endregion
    }
}
