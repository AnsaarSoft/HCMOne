using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_PerfPlan:HRMSBaseForm
    {

        #region "Variable"
        
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.UserDataSource oUDS_Employee, oUDS_PlanDate, oUDS_FromDate, oUDS_ToDate;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column oColumn, clNo, clID, clIsNew, clKPI,clTarget, clWeightage, clStartDate, clEndDate;
        private SAPbouiCOM.EditText txPlanNo,txPlanDate,txDesignation,txDepartment,txBranch,txFromDate,txToDate;
        private SAPbouiCOM.ComboBox cbEmpID;
        private SAPbouiCOM.Item ItxPlanNo, ItxPlanDate, ItxDesignation, ItxDepartment, ItxBranch, ItxFromDate, ItxToDate, IcbEmpID;
        private int CurrentPerfPlanID = 0, CurrentPerfPlanNo = 0;

        IEnumerable<TrnsPerformancePlan> oCollection = null;
        Boolean flgFormMode = false;

        #endregion

        #region "B1 Functions"
        
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            FillEmployeeCombo();
            FillKPIColumnCombo();
            txPlanNo.Value = GetNextDocnum().ToString();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            //GetDataFromDatasource();
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
                        //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                        MstEmployee Employee = (from v in dbHrPayroll.MstEmployee where v.ID.ToString() == cbEmpID.Value.Trim() select v).FirstOrDefault();
                        if (Employee != null)
                        {
                            txDesignation.Value = (Employee.DesignationName == null ? "" : Employee.DesignationName);
                            txDepartment.Value = (Employee.DepartmentName == null ? "" : Employee.DepartmentName);
                            txBranch.Value = (Employee.BranchName == null ? "" : Employee.BranchName);
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
                    case "t_PDate":
                        int CompanyDate = int.Parse(oCompany.GetCompanyDate().ToString("yyyyMMdd"));
                        int Date = int.Parse(txPlanDate.Value == "" ? "0" : txPlanDate.Value);
                        if (Date > CompanyDate)
                        {
                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DateCheck"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            BubbleEvent = false;
                            return;
                        }
                        break;
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
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {
                                case "cl_SDate":
                                case "cl_EDate":
                                    {
                                        string StartDate = (oMat.Columns.Item("cl_SDate").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value,
                                            EndDate = (oMat.Columns.Item("cl_EDate").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                                        if (StartDate.Equals("") || EndDate.Equals(""))
                                            return;
                                        else if (int.Parse(EndDate) < int.Parse(StartDate))
                                        {
                                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_DateComparison"), SAPbouiCOM.BoMessageTime.bmt_Short);
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
            try
            {
                switch (pVal.ItemUID)
                {
                    case "Mat":
                        if ((pVal.ColUID.Equals("cl_KPI") || pVal.ColUID.Equals("cl_Weigh") || pVal.ColUID.Equals("cl_SDate") || pVal.ColUID.Equals("cl_EDate")) && pVal.Row == oMat.RowCount)
                        {
                            var KPI = (oMat.Columns.Item("cl_KPI").Cells.Item(pVal.Row).Specific as SAPbouiCOM.ComboBox).Value;
                            var Weightage = (oMat.Columns.Item("cl_Weigh").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                            var StartDate = (oMat.Columns.Item("cl_SDate").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.ToString();
                            var EndDate = (oMat.Columns.Item("cl_EDate").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.ToString();
                            if (!KPI.Equals("") && !StartDate.Equals("") && !EndDate.Equals("") && !Weightage.Equals(""))
                            {
                                AddBlankRow();
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
            BubbleEvent = true;
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
                    
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            ChangeFormToAddMode();
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

        public override void fillFields()
        {
            base.fillFields();
            FillDocument();
        }
        
        #endregion 

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                oForm.AutoManaged = true;

                oUDS_Employee = oForm.DataSources.UserDataSources.Add("EmpID", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbEmpID = oForm.Items.Item("cb_EmpID").Specific;
                IcbEmpID = oForm.Items.Item("cb_EmpID");
                cbEmpID.DataBind.SetBound(true, "", "EmpID");

                oForm.DataSources.UserDataSources.Add("PlanNo", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txPlanNo = oForm.Items.Item("t_PNo").Specific;
                ItxPlanNo = oForm.Items.Item("t_PNo");
                txPlanNo.DataBind.SetBound(true, "", "PlanNo");

                oUDS_PlanDate = oForm.DataSources.UserDataSources.Add("PlanDate", SAPbouiCOM.BoDataType.dt_DATE);
                txPlanDate = oForm.Items.Item("t_PDate").Specific;
                ItxPlanDate = oForm.Items.Item("t_PDate");
                txPlanDate.DataBind.SetBound(true, "", "PlanDate");

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

                oUDS_FromDate = oForm.DataSources.UserDataSources.Add("FromDate", SAPbouiCOM.BoDataType.dt_DATE);
                txFromDate = oForm.Items.Item("t_FDate").Specific;
                ItxFromDate = oForm.Items.Item("t_FDate");
                txFromDate.DataBind.SetBound(true, "", "FromDate");

                oUDS_ToDate = oForm.DataSources.UserDataSources.Add("ToDate", SAPbouiCOM.BoDataType.dt_DATE);
                txToDate = oForm.Items.Item("t_TDate").Specific;
                ItxToDate = oForm.Items.Item("t_TDate");
                txToDate.DataBind.SetBound(true, "", "ToDate");

                oDBDataTable = oForm.DataSources.DataTables.Add("PerfPlanDetail");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("IsNew", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                oDBDataTable.Columns.Add("KPI", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("Weight", SAPbouiCOM.BoFieldsType.ft_Sum);
                oDBDataTable.Columns.Add("Target", SAPbouiCOM.BoFieldsType.ft_Sum);
                oDBDataTable.Columns.Add("SDate", SAPbouiCOM.BoFieldsType.ft_Date);
                oDBDataTable.Columns.Add("EDate", SAPbouiCOM.BoFieldsType.ft_Date);

                oMat = oForm.Items.Item("Mat").Specific;
                oColumns = oMat.Columns;

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "No");

                oColumn = oColumns.Item("cl_ID");
                clID = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "ID");

                oColumn = oColumns.Item("cl_IsNew");
                clIsNew = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "ISNew");

                oColumn = oColumns.Item("cl_KPI");
                clKPI = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "KPI");

                oColumn = oColumns.Item("cl_Weigh");
                clWeightage = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "Weight");

                oColumn = oColumns.Item("cl_Trgt");
                clTarget = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "Target");

                oColumn = oColumns.Item("cl_SDate");
                clStartDate = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "SDate");

                oColumn = oColumns.Item("cl_EDate");
                clEndDate = oColumn;
                oColumn.DataBind.Bind("PerfPlanDetail", "EDate");

                GetData();
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
                if (txPlanDate.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDocDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbEmpID.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullEmployee"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (txFromDate.Value.Equals("") || txToDate.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                string KPI, Weightage, StartDate, EndDate;
                for (int i = 1; i <= oMat.RowCount; i++)
                {
                    KPI = (oMat.Columns.Item("cl_KPI").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    Weightage = (oMat.Columns.Item("cl_Weigh").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Weightage = (Weightage == "0.0" ? "" : Weightage);
                    StartDate = (oMat.Columns.Item("cl_SDate").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    EndDate = (oMat.Columns.Item("cl_EDate").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (i != oMat.RowCount)
                    {
                        if (KPI.Equals("") && Weightage.Equals("") && StartDate.Equals("") && EndDate.Equals(""))
                        {
                            oApplication.StatusBar.SetText("Mmandatory Fields are Missing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            BubbleEvent = false;
                            return;
                        }
                    }
                    else
                    {
                        if ((KPI.Equals("") || Weightage.Equals("") || StartDate.Equals("") || EndDate.Equals(""))
                            && !(KPI.Equals("") && Weightage.Equals("") && StartDate.Equals("") && EndDate.Equals("")))
                        {
                            oApplication.StatusBar.SetText("Mmandatory Fields are Missing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            BubbleEvent = false;
                            return;
                        }
                    }
                }
                switch (oForm.Mode)
                {
                    case SAPbouiCOM.BoFormMode.fm_ADD_MODE:
                        AddDocument();
                        break;
                    case SAPbouiCOM.BoFormMode.fm_UPDATE_MODE:
                        oForm.Freeze(true);
                        UpdateDocument();
                        //FillDocument((from v in dbHrPayroll.TrnsPerformancePlan where v.Id == CurrentPerfPlanID select v).Single());
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

        private void AddDocument()
        {
            try
            {
                TrnsPerformancePlan PerfPlan = new TrnsPerformancePlan();
                PerfPlan.PlanNo = int.Parse(txPlanNo.Value);
                PerfPlan.PlanDate = DateTime.ParseExact(txPlanDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPlan.EmpID = int.Parse(cbEmpID.Value);
                PerfPlan.EmpBranch = txBranch.Value;
                PerfPlan.EmpDesignation = txDesignation.Value;
                PerfPlan.EmpDepartment = txDepartment.Value;
                PerfPlan.FromDate = DateTime.ParseExact(txFromDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPlan.ToDate = DateTime.ParseExact(txToDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPlan.CreateDate = DateTime.Now;
                PerfPlan.UserID = oCompany.UserSignature.ToString();
                PerfPlan.UpdatedBy = oCompany.UserName;
                oMat.FlushToDataSource();
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    TrnsPerformancePlanDetail PerfPlanDetail = new TrnsPerformancePlanDetail();
                    PerfPlanDetail.KPIID = oDBDataTable.GetValue("KPI", i);
                    string strWeight = Convert.ToString(oDBDataTable.GetValue("Weight", i));
                    PerfPlanDetail.WeightagePer = Convert.ToDecimal(strWeight.Replace(",",""));
                    string strTarget = Convert.ToString(oDBDataTable.GetValue("Target", i));
                    PerfPlanDetail.TargetPer = Convert.ToDecimal(strTarget.Replace(",",""));
                    PerfPlanDetail.StartDate = oDBDataTable.GetValue("SDate", i);
                    PerfPlanDetail.EndDate = oDBDataTable.GetValue("EDate", i);
                    PerfPlanDetail.CreateDate = DateTime.Now;
                    PerfPlanDetail.UserID = oCompany.UserSignature.ToString();
                    PerfPlanDetail.UpdatedBy = oCompany.UserName;
                    PerfPlan.TrnsPerformancePlanDetail.Add(PerfPlanDetail);
                }
                dbHrPayroll.TrnsPerformancePlan.InsertOnSubmit(PerfPlan);
                dbHrPayroll.SubmitChanges();
                CurrentPerfPlanID = PerfPlan.Id;
                CurrentPerfPlanNo = (int)PerfPlan.PlanNo;
                GetData();
                ChangeFormToAddMode();
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
       
        private void UpdateDocument()
        {
            try
            {
                TrnsPerformancePlan PerfPlan = (from v in dbHrPayroll.TrnsPerformancePlan where v.Id == CurrentPerfPlanID select v).Single();
                PerfPlan.PlanNo = int.Parse(txPlanNo.Value);
                PerfPlan.PlanDate = DateTime.ParseExact(txPlanDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPlan.EmpID = int.Parse(cbEmpID.Value);
                PerfPlan.EmpBranch = txBranch.Value;
                PerfPlan.EmpDesignation = txDesignation.Value;
                PerfPlan.EmpDepartment = txDepartment.Value;
                PerfPlan.FromDate = DateTime.ParseExact(txFromDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPlan.ToDate = DateTime.ParseExact(txToDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfPlan.UpdateDate = DateTime.Now;
                PerfPlan.UserID = oCompany.UserSignature.ToString();
                PerfPlan.UpdatedBy = oCompany.UserName;
                oMat.FlushToDataSource();
                int DetailId = 0;
                for (int i = 0; i < oDBDataTable.Rows.Count - 1; i++)
                {
                    if (oDBDataTable.GetValue("IsNew", i) == "Y")
                    {
                        TrnsPerformancePlanDetail PerfPlanDetail = new TrnsPerformancePlanDetail();
                        PerfPlanDetail.KPIID = oDBDataTable.GetValue("KPI", i);
                        PerfPlanDetail.WeightagePer = (decimal)oDBDataTable.GetValue("Weight", i);
                        PerfPlanDetail.TargetPer = (decimal)oDBDataTable.GetValue("Target", i);
                        PerfPlanDetail.StartDate = oDBDataTable.GetValue("SDate", i);
                        PerfPlanDetail.EndDate = oDBDataTable.GetValue("EDate", i);
                        PerfPlanDetail.CreateDate = DateTime.Now;
                        PerfPlanDetail.UserID = oCompany.UserSignature.ToString();
                        PerfPlanDetail.UpdatedBy = oCompany.UserName;
                        PerfPlan.TrnsPerformancePlanDetail.Add(PerfPlanDetail);
                    }
                    else
                    {
                        DetailId = oDBDataTable.GetValue("ID", i);
                        TrnsPerformancePlanDetail PerfPlanDetail = (from v in dbHrPayroll.TrnsPerformancePlanDetail where v.Id == DetailId select v).Single();
                        PerfPlanDetail.KPIID = oDBDataTable.GetValue("KPI", i);
                        PerfPlanDetail.WeightagePer = (decimal)oDBDataTable.GetValue("Weight", i);
                        PerfPlanDetail.TargetPer = (decimal)oDBDataTable.GetValue("Target", i);
                        PerfPlanDetail.StartDate = oDBDataTable.GetValue("SDate", i);
                        PerfPlanDetail.EndDate = oDBDataTable.GetValue("EDate", i);
                        PerfPlanDetail.UpdateDate = DateTime.Now;
                        PerfPlanDetail.UserID = oCompany.UserSignature.ToString();
                        PerfPlanDetail.UpdatedBy = oCompany.UserName;
                    }
                    dbHrPayroll.SubmitChanges();
                }
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
                TrnsPerformancePlan oDoc = oCollection.ElementAt<TrnsPerformancePlan>(currentRecord);
                
                txPlanNo.Value = oDoc.PlanNo.ToString();
                txPlanDate.Value = "";
                txPlanDate.Value = ((DateTime)oDoc.PlanDate).ToString("yyyyMMdd");
                oUDS_Employee.Value = oDoc.EmpID.ToString();
                txDesignation.Value = oDoc.EmpDesignation;
                txDepartment.Value = oDoc.EmpDepartment;
                txBranch.Value = oDoc.EmpBranch;
                txFromDate.Value = "";
                txToDate.Value = "";
                txFromDate.Value = ((DateTime)oDoc.FromDate).ToString("yyyyMMdd");
                txToDate.Value = ((DateTime)oDoc.ToDate).ToString("yyyyMMdd");

                //var Records = from v in oDoc.TrnsPerformancePlanDetail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (TrnsPerformancePlanDetail Line in oDoc.TrnsPerformancePlanDetail)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("ID", i, Line.Id);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("KPI", i, Line.KPIID);
                    oDBDataTable.SetValue("Weight", i, (double)Line.WeightagePer);
                    oDBDataTable.SetValue("Target", i, Convert.ToDouble(Line.TargetPer));
                    oDBDataTable.SetValue("SDate", i, Line.StartDate);
                    oDBDataTable.SetValue("EDate", i, Line.EndDate);
                    i++;
                }
                CurrentPerfPlanID = oDoc.Id;
                CurrentPerfPlanNo = (int)oDoc.PlanNo;
                oMat.LoadFromDataSource();
                if (oDoc != null)
                {
                    TrnsPromotionAdvice oPromotion = (from a in dbHrPayroll.TrnsPromotionAdvice where a.TrnsPerformancePlan.Id == oDoc.Id select a).FirstOrDefault();
                    if (oPromotion != null)
                    {
                        flgFormMode = true;
                    }
                    else
                    {
                        flgFormMode = false;
                    }
                }
                if (!flgFormMode)
                {
                    
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                }
                else
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.TrnsPerformancePlan select a;
            Int32 i = 0;
            foreach (TrnsPerformancePlan oDoc in oCollection)
            {
                CodeIndex.Add(oDoc.Id, i);
                i++;
            }
            totalRecord = i;
        }

        private void FillEmployeeCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                //var Employees = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;

                String querycheck = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.EmpID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName
                                      FROM dbo.MstEmployee
                                      ORDER BY dbo.MstEmployee.EmpID";
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(querycheck);
                foreach (var Employee in Records)
                {
                    cbEmpID.ValidValues.Add(Employee.ID.ToString(), Employee.EmpID + " : " + Employee.FirstName + " " + Employee.MiddleName + " " + Employee.LastName);
                    //cbEmpID.ValidValues.Add(Convert.ToString(Employee.EmpID), Convert.ToString(Employee.FirstName + " " + Employee.LastName));
                }
                IcbEmpID.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(" FillEmployeeCombo : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillKPIColumnCombo()
        {
            try
            {

                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsKPI);
                var KPIs = from v in dbHrPayroll.TrnsKPI select v;
                foreach (var KPI in KPIs)
                {
                    clKPI.ValidValues.Add(KPI.ID.ToString(), KPI.KeyObjectives);
                }
                clKPI.DisplayDesc = true;
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

        public int GetNextDocnum()
        {
            try
            {
                int MaxDocnum = Convert.ToInt32(dbHrPayroll.TrnsPerformancePlan.Max(x => x.PlanNo));
                return MaxDocnum + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private void ChangeFormToAddMode()
        {
            try
            {
                oForm.Freeze(true);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                CurrentPerfPlanID = 0;
                CurrentPerfPlanNo = 0;
                txPlanNo.Value = GetNextDocnum().ToString();
                txPlanDate.Value = "";
                oUDS_Employee.Value = "";
                txDesignation.Value = "";
                txDepartment.Value = "";
                txBranch.Value = "";
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

        #endregion
       
    }
}

