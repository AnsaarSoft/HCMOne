using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_PromAdvc:HRMSBaseForm
    {
        #region "Global Variable"
        
        private SAPbouiCOM.UserDataSource oUDS_Employee, oUDS_Status, oUDS_Series, oUDS_AdvcStatus, oUDS_PerfPlanNo, oUDS_PlanFromDate;
        private SAPbouiCOM.UserDataSource oUDS_PlanToDate, oUDS_AprslNo, oUDS_AprslDate, oUDS_LastPromDate, oUDS_NewStatus, oUDS_NewDesg, oUDS_NewDept, oUDS_NewLinMngr, oUDS_TotalScore, oUDS_Remarks;
        private SAPbouiCOM.EditText txFirstName, txLastName, txDesignation, txDepartment, txBranch, txDocNum, txtStatus;
        private SAPbouiCOM.EditText txDocDate, txPlanFromDate, txPlanToDate, txAprslDate, txLastPromDate, txtRemarks, txIncrement, txtTotalScore;
        private SAPbouiCOM.ComboBox cbEmpID, cbStatus, cbSeries, cbAdvcStatus, cbPerfPlanNo, cbAprslNo;
        private SAPbouiCOM.ComboBox cbNewStatus, cbNewDesg, cbNewDept, cbNewLinMngr;
        private SAPbouiCOM.CheckBox chPromGranted;
        private SAPbouiCOM.Item ItxFirstName, ItxLastName, ItxDesignation, ItxDepartment, ItxBranch, ItxDocNum, ItxDocDate, itxtStatus;
        private SAPbouiCOM.Item ItxPlanFromDate, ItxPlanToDate, ItxAprslDate, ItxLastPromDate, ItxtRemarks, ItxIncrement, ItxtTotalScore, IcbEmpID, IcbStatus, IcbSeries;
        private SAPbouiCOM.Item IcbAdvcStatus, IcbPerfPlanNo, IcbAprslNo, IcbNewStatus, IcbNewDesg, IcbNewDept, IcbNewLinMngr, IchPromGranted;
        private int CurrentPromAdvcID = 0;

        IEnumerable<TrnsPromotionAdvice> oCollection = null;
        
        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            IntitalizeForm();
            FillEmployeeCombo();
            FillSeriesCombo();
            FillNewDesignationCombo();
            FillNewDepartmentCombo();
            FillNewLineManagerCombo();
            oUDS_Series.Value = "-1";
            txDocNum.Value = GetNextDocnum().ToString();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
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
                        MstEmployee Employee = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbEmpID.Value.ToString()) select v).Single();
                        txFirstName.Value = (Employee.FirstName == null ? "" : Employee.FirstName);
                        txLastName.Value = (Employee.LastName == null ? "" : Employee.LastName);
                        txDesignation.Value = (Employee.DesignationName == null ? "" : Employee.DesignationName);
                        txDepartment.Value = (Employee.DepartmentName == null ? "" : Employee.DepartmentName);
                        txBranch.Value = (Employee.BranchName == null ? "" : Employee.BranchName);
                        txPlanFromDate.Value = "";
                        txPlanToDate.Value = "";
                        txAprslDate.Value = "";
                        txLastPromDate.Value = "";
                        FillPlanNoCombo(Employee.ID);
                        FillAprslNoCombo(Employee.ID);
                        break;
                    case "cb_PPNo":
                        //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformancePlan);
                        var PerfPlan = (from v in dbHrPayroll.TrnsPerformancePlan where v.PlanNo == int.Parse(cbPerfPlanNo.Value) select v).Single();
                        txPlanFromDate.Value = "";
                        txPlanFromDate.Value = ((DateTime)PerfPlan.FromDate).ToString("yyyyMMdd");
                        txPlanToDate.Value = "";
                        txPlanToDate.Value = ((DateTime)PerfPlan.ToDate).ToString("yyyyMMdd");
                        break;
                    case "cb_AprslNo":
                        //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformanceAppraisal);
                        var PerfAprsl = (from v in dbHrPayroll.TrnsPerformanceAppraisal where v.DocNum == int.Parse(cbAprslNo.Value) select v).FirstOrDefault();
                        if (PerfAprsl != null)
                        {
                            txAprslDate.Value = "";
                            txAprslDate.Value = ((DateTime)PerfAprsl.DocDate).ToString("yyyyMMdd");
                            txtTotalScore.Value = Convert.ToString(PerfAprsl.TotalScore);
                        }
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
                        InitializeControls();
                        break;
                    case "btPrint":
                        PrintPromotionAdvice();
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
        }

        public override void  fillFields()
        {
 	         base.fillFields();
            FillDocuments();
        }

        #endregion

        #region "Local Methods"

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
                //cbStatus = oForm.Items.Item("cb_Status").Specific;
                //IcbStatus = oForm.Items.Item("cb_Status");
                //cbStatus.DataBind.SetBound(true, "", "Status");
                txtStatus = oForm.Items.Item("txStatus").Specific;
                itxtStatus = oForm.Items.Item("txStatus");
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

                oUDS_AdvcStatus = oForm.DataSources.UserDataSources.Add("AdvcStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbAdvcStatus = oForm.Items.Item("cb_AdvcSts").Specific;
                IcbAdvcStatus = oForm.Items.Item("cb_AdvcSts");
                cbAdvcStatus.DataBind.SetBound(true, "", "AdvcStatus");

                oUDS_PerfPlanNo = oForm.DataSources.UserDataSources.Add("PerfPlanNo", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbPerfPlanNo = oForm.Items.Item("cb_PPNo").Specific;
                IcbPerfPlanNo = oForm.Items.Item("cb_PPNo");
                cbPerfPlanNo.DataBind.SetBound(true, "", "PerfPlanNo");

                oUDS_PlanFromDate = oForm.DataSources.UserDataSources.Add("PlnFDate", SAPbouiCOM.BoDataType.dt_DATE);
                txPlanFromDate = oForm.Items.Item("t_PlnFDt").Specific;
                ItxPlanFromDate = oForm.Items.Item("t_PlnFDt");
                txPlanFromDate.DataBind.SetBound(true, "", "PlnFDate");

                oUDS_PlanToDate = oForm.DataSources.UserDataSources.Add("PlnTDate", SAPbouiCOM.BoDataType.dt_DATE);
                txPlanToDate = oForm.Items.Item("t_PlnTDt").Specific;
                ItxPlanToDate = oForm.Items.Item("t_PlnTDt");
                txPlanToDate.DataBind.SetBound(true, "", "PlnTDate");

                oUDS_AprslNo = oForm.DataSources.UserDataSources.Add("AprslNo", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbAprslNo = oForm.Items.Item("cb_AprslNo").Specific;
                IcbAprslNo = oForm.Items.Item("cb_AprslNo");
                cbAprslNo.DataBind.SetBound(true, "", "AprslNo");

                oUDS_AprslDate = oForm.DataSources.UserDataSources.Add("AprslDate", SAPbouiCOM.BoDataType.dt_DATE);
                txAprslDate = oForm.Items.Item("t_AprslDt").Specific;
                ItxAprslDate = oForm.Items.Item("t_AprslDt");
                txAprslDate.DataBind.SetBound(true, "", "AprslDate");

                oUDS_LastPromDate = oForm.DataSources.UserDataSources.Add("LstPrmDt", SAPbouiCOM.BoDataType.dt_DATE);
                txLastPromDate = oForm.Items.Item("t_LstPrmDt").Specific;
                ItxLastPromDate = oForm.Items.Item("t_LstPrmDt");
                txLastPromDate.DataBind.SetBound(true, "", "LstPrmDt");

                oForm.DataSources.UserDataSources.Add("Remarks", SAPbouiCOM.BoDataType.dt_LONG_TEXT);
                txtRemarks = oForm.Items.Item("t_PrmAdvc").Specific;
                ItxtRemarks = oForm.Items.Item("t_PrmAdvc");
                txtRemarks.DataBind.SetBound(true, "", "Remarks");

                oForm.DataSources.UserDataSources.Add("IncPer", SAPbouiCOM.BoDataType.dt_PERCENT);
                txIncrement = oForm.Items.Item("t_IncPer").Specific;
                ItxIncrement = oForm.Items.Item("t_IncPer");
                txIncrement.DataBind.SetBound(true, "", "IncPer");

                oForm.DataSources.UserDataSources.Add("TotalScore", SAPbouiCOM.BoDataType.dt_PERCENT);
                txtTotalScore = oForm.Items.Item("txTScr").Specific;
                ItxtTotalScore = oForm.Items.Item("txTScr");
                txtTotalScore.DataBind.SetBound(true, "", "TotalScore");

                oUDS_NewStatus = oForm.DataSources.UserDataSources.Add("NewStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbNewStatus = oForm.Items.Item("cb_NStatus").Specific;
                IcbNewStatus = oForm.Items.Item("cb_NStatus");
                cbNewStatus.DataBind.SetBound(true, "", "NewStatus");

                oUDS_NewDesg = oForm.DataSources.UserDataSources.Add("NewDesg", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbNewDesg = oForm.Items.Item("cb_NDesg").Specific;
                IcbNewDesg = oForm.Items.Item("cb_NDesg");
                cbNewDesg.DataBind.SetBound(true, "", "NewDesg");

                oUDS_NewDept = oForm.DataSources.UserDataSources.Add("NewDept", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbNewDept = oForm.Items.Item("cb_NDept").Specific;
                IcbNewDept = oForm.Items.Item("cb_NDept");
                cbNewDept.DataBind.SetBound(true, "", "NewDept");

                oUDS_NewLinMngr = oForm.DataSources.UserDataSources.Add("NewLinMngr", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbNewLinMngr = oForm.Items.Item("cb_NLnMngr").Specific;
                IcbNewLinMngr = oForm.Items.Item("cb_NLnMngr");
                cbNewLinMngr.DataBind.SetBound(true, "", "NewLinMngr");

                oForm.DataSources.UserDataSources.Add("PromGrant", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chPromGranted = oForm.Items.Item("ch_PrmGrnt").Specific;
                IchPromGranted = oForm.Items.Item("ch_PrmGrnt");
                chPromGranted.DataBind.SetBound(true, "", "PromGrant");

                //oUDS_Remarks = oForm.DataSources.UserDataSources.Add("Remarks", SAPbouiCOM.BoDataType.dt_LONG_TEXT);
                //txtRemarks = oForm.Items.Item("").Specific;
                //ItxtRemarks = oForm.Items.Item("");
                //txtRemarks.DataBind.SetBound(true, "", "TotalScore");

                base.fillCombo("PerApprsl_Status", cbAdvcStatus);
                IcbAdvcStatus.DisplayDesc = true;

                base.fillCombo("ContractType", cbNewStatus); ;
                IcbNewStatus.DisplayDesc = true;

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
                string strQuery = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.EmpID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName
                                    FROM dbo.MstEmployee INNER JOIN dbo.TrnsPerformanceAppraisal ON dbo.MstEmployee.ID = dbo.TrnsPerformanceAppraisal.EmpID
                                    ORDER BY dbo.MstEmployee.EmpID";

                //var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(strQuery);
                foreach (var Record in Records)
                {
                    cbEmpID.ValidValues.Add(Record.ID.ToString(), Convert.ToString(Record.EmpID + " : " + Record.FirstName + " " + Record.MiddleName + " " + Record.LastName));
                }
                IcbEmpID.DisplayDesc = true;
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
        
        private void FillPlanNoCombo(int EmpID)
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformancePlan);
                var Records = from v in dbHrPayroll.TrnsPerformancePlan where v.EmpID == EmpID select v;

                int Count = cbPerfPlanNo.ValidValues.Count;
                for (int i = 1; i <= Count; i++)
                {
                    cbPerfPlanNo.ValidValues.Remove(Count - i, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                foreach (var Record in Records)
                {
                    cbPerfPlanNo.ValidValues.Add(Record.PlanNo.ToString(), Record.MstEmployee.FirstName + " " + Record.MstEmployee.LastName);
                }
                IcbPerfPlanNo.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillAprslNoCombo(int EmpID)
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformanceAppraisal);
                var Records = from v in dbHrPayroll.TrnsPerformanceAppraisal where v.EmpID == EmpID select v;

                int Count = cbAprslNo.ValidValues.Count;
                for (int i = 1; i <= Count; i++)
                {
                    cbAprslNo.ValidValues.Remove(Count - i, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                foreach (var Record in Records)
                {
                    cbAprslNo.ValidValues.Add(Record.DocNum.ToString(), Record.MstEmployee.FirstName + " " + Record.MstEmployee.LastName);
                }
                IcbAprslNo.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillNewDesignationCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstDesignation);
                var Records = from v in dbHrPayroll.MstDesignation select v;

                foreach (var Record in Records)
                {
                    cbNewDesg.ValidValues.Add(Record.Id.ToString(), Record.Name);
                }
                IcbNewDesg.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillNewDepartmentCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstDepartment);
                var Records = from v in dbHrPayroll.MstDepartment select v;

                foreach (var Record in Records)
                {
                    cbNewDept.ValidValues.Add(Record.ID.ToString(), Record.DeptName);
                }
                IcbNewDept.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillNewLineManagerCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                //var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                String querycheck = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.EmpID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName
                                      FROM dbo.MstEmployee
                                      ORDER BY dbo.MstEmployee.EmpID";
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(querycheck);
                foreach (var Record in Records)
                {
                    cbNewLinMngr.ValidValues.Add(Record.ID.ToString(), Record.EmpID + " : "  + Record.FirstName + " " + Record.MiddleName + " " + Record.LastName);
                }
                IcbNewLinMngr.DisplayDesc = true;
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
                else if (cbSeries.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullSeries"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (txDocDate.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullDocDate"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbAdvcStatus.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText("Select Advice Status", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbPerfPlanNo.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullPlanNo"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbAprslNo.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullAprslNo"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (txtRemarks.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText("Enter Promotion Advice", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbNewStatus.Value.Equals("") || cbNewDesg.Value.Equals("") || cbNewDept.Value.Equals("") || cbNewLinMngr.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_MandatoryFields"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }

                switch (oForm.Mode)
                {
                    case SAPbouiCOM.BoFormMode.fm_ADD_MODE:
                        AddDocument();
                        break;
                    case SAPbouiCOM.BoFormMode.fm_UPDATE_MODE:
                        UpdateDocument();
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitializeControls()
        {
            try
            {
                oForm.Freeze(true);
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
                oUDS_AdvcStatus.Value = "";
                oUDS_PerfPlanNo.Value = "";
                txPlanFromDate.Value = "";
                txPlanToDate.Value = "";
                oUDS_AprslNo.Value = "";
                txAprslDate.Value = "";
                txLastPromDate.Value = "";
                txtRemarks.Value = "";
                oUDS_NewStatus.Value = "";
                oUDS_NewDesg.Value = "";
                oUDS_NewDept.Value = "";
                oUDS_NewLinMngr.Value = "";
                txIncrement.Value = "0.00";
                txtTotalScore.Value = "0.00";
                chPromGranted.Checked = false;
                CurrentPromAdvcID = 0;
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oForm.Freeze(false);
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
                TrnsPromotionAdvice oNew = new TrnsPromotionAdvice();
                
                oNew.EmpID = int.Parse(cbEmpID.Value.Trim());
                oNew.FirstName = txFirstName.Value.Trim();
                oNew.LastName = txLastName.Value.Trim();
                oNew.Designation = txDesignation.Value.Trim();
                oNew.Department = txDepartment.Value.Trim();
                oNew.Branch = txBranch.Value.Trim();
                //PromAdvc.Status = int.Parse(cbStatus.Value);
                oNew.AdviceStatus = "Draft";
                oNew.Series = int.Parse(cbSeries.Value.Trim());
                oNew.DocNum = int.Parse(txDocNum.Value.Trim());
                oNew.DocDate = DateTime.ParseExact(txDocDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                oNew.AdviceStatus = cbAdvcStatus.Value.Trim();
                string checkvalue = cbPerfPlanNo.Value.Trim();
                TrnsPerformancePlan oPlan = (from a in dbHrPayroll.TrnsPerformancePlan where a.PlanNo.ToString() == checkvalue select a).FirstOrDefault();
                //oNew.PlanNo = int.Parse(cbPerfPlanNo.Value.Trim());
                oNew.TrnsPerformancePlan = oPlan;
                oNew.PerfPeriodFrom = DateTime.ParseExact(txPlanFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                oNew.PerfPeriodTo = DateTime.ParseExact(txPlanToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                oNew.AppraisalNo = int.Parse(cbAprslNo.Value.Trim());
                oNew.AppraisalDate = DateTime.ParseExact(txAprslDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                oNew.TotalScorce = Convert.ToDecimal(txtTotalScore.Value);
                //PromAdvc.LastPromotionDate = DateTime.ParseExact(txLastPromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);

                oNew.NewStatus = String.IsNullOrEmpty(cbNewStatus.Value.Trim()) ? "" : cbNewStatus.Value.Trim();
                oNew.NewDesignation = int.Parse(cbNewDesg.Value.Trim());
                oNew.NewDepartment = int.Parse(cbNewDept.Value.Trim());
                oNew.NewLineManager = int.Parse(cbNewLinMngr.Value.Trim());
                oNew.IncrementPer = decimal.Parse(txIncrement.Value.Trim());
                oNew.FltPromotion = chPromGranted.Checked;
                oNew.Remarks = txtRemarks.Value.Trim();
                oNew.CreateDate = DateTime.Now;
                oNew.UserId = oCompany.UserName;
                oNew.UpdatedBy = oCompany.UserName;
                dbHrPayroll.TrnsPromotionAdvice.InsertOnSubmit(oNew);
                dbHrPayroll.SubmitChanges();
                CurrentPromAdvcID = oNew.ID;
                InitializeControls();
                GetData();
                oApplication.StatusBar.SetText("Document Added Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
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
                TrnsPromotionAdvice PromAdvc = (from v in dbHrPayroll.TrnsPromotionAdvice where v.ID == CurrentPromAdvcID select v).Single();
                PromAdvc.EmpID = int.Parse(cbEmpID.Value);
                PromAdvc.FirstName = txFirstName.Value;
                PromAdvc.LastName = txLastName.Value;
                PromAdvc.Designation = txDesignation.Value;
                PromAdvc.Department = txDepartment.Value;
                PromAdvc.Branch = txBranch.Value;
                //PromAdvc.Status = int.Parse(cbStatus.Value);
                //PromAdvc.Series = int.Parse(cbSeries.Value);
                //PromAdvc.DocNum = int.Parse(txDocNum.Value);
                PromAdvc.DocDate = DateTime.ParseExact(txDocDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                //PromAdvc.PlanNo = int.Parse(cbPerfPlanNo.Value);
                PromAdvc.PerfPeriodFrom = DateTime.ParseExact(txPlanFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PromAdvc.PerfPeriodTo = DateTime.ParseExact(txPlanToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PromAdvc.AppraisalNo = int.Parse(cbAprslNo.Value);
                PromAdvc.AppraisalDate = DateTime.ParseExact(txAprslDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PromAdvc.TotalScorce = Convert.ToDecimal(txtTotalScore.Value);
                //PromAdvc.LastPromotionDate = DateTime.ParseExact(txLastPromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                if (cbAdvcStatus.Value.Trim() != "-1")
                {
                    PromAdvc.AdviceStatus = cbAdvcStatus.Value.Trim();
                }
                PromAdvc.NewStatus = cbNewStatus.Value;
                PromAdvc.NewDesignation = int.Parse(cbNewDesg.Value);
                PromAdvc.NewDepartment = int.Parse(cbNewDept.Value);
                PromAdvc.NewLineManager = int.Parse(cbNewLinMngr.Value);
                PromAdvc.IncrementPer = decimal.Parse(txIncrement.Value);
                PromAdvc.FltPromotion = chPromGranted.Checked;
                PromAdvc.Remarks = txtRemarks.Value.Trim();
                PromAdvc.UpdateDate = DateTime.Now;
                PromAdvc.UserId = oCompany.UserSignature.ToString();
                PromAdvc.UpdatedBy = oCompany.UserName;

                dbHrPayroll.SubmitChanges();
                CurrentPromAdvcID = PromAdvc.ID;
                oApplication.StatusBar.SetText("Document Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDocuments()
        {
            try
            {
                TrnsPromotionAdvice oDoc = oCollection.ElementAt<TrnsPromotionAdvice>(currentRecord);
                
                txDocNum.Value = oDoc.DocNum.ToString();
                cbSeries.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                oUDS_Employee.Value = oDoc.EmpID.ToString();
                txFirstName.Value = oDoc.FirstName; 
                txLastName.Value = oDoc.LastName;
                txDesignation.Value = oDoc.Designation;
                txDepartment.Value = oDoc.Department;
                txBranch.Value = oDoc.Branch;
                oUDS_Status.Value = oDoc.AdviceStatus;
                oUDS_Series.Value = oDoc.Series.ToString();
                txDocNum.Value = oDoc.DocNum.ToString();
                txDocDate.Value = "";
                txDocDate.Value = ((DateTime)oDoc.DocDate).ToString("yyyyMMdd");
                oUDS_AdvcStatus.Value = oDoc.AdviceStatus;
                oUDS_PerfPlanNo.Value = oDoc.PlanNo.ToString();
                txPlanFromDate.Value = ((DateTime)oDoc.PerfPeriodFrom).ToString("yyyyMMdd");
                txPlanToDate.Value = ((DateTime)oDoc.PerfPeriodTo).ToString("yyyyMMdd");
                oUDS_AprslNo.Value = oDoc.AppraisalNo.ToString();
                txAprslDate.Value = ((DateTime)oDoc.AppraisalDate).ToString("yyyyMMdd");
                txtTotalScore.Value = Convert.ToString(oDoc.TotalScorce);
                if (!oDoc.LastPromotionDate.Equals(null))
                    txLastPromDate.Value = ((DateTime)oDoc.LastPromotionDate).ToString("yyyyMMdd");
                else
                    txLastPromDate.Value = "";
                txtRemarks.Value = oDoc.Remarks;
                oUDS_NewStatus.Value = oDoc.NewStatus;
                oUDS_NewDesg.Value = oDoc.NewDesignation.ToString();
                oUDS_NewDept.Value = oDoc.NewDepartment.ToString();
                oUDS_NewLinMngr.Value = oDoc.NewLineManager.ToString();
                txIncrement.Value = oDoc.IncrementPer.ToString();
                chPromGranted.Checked = (bool)oDoc.FltPromotion;
                
                CurrentPromAdvcID = oDoc.ID;
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
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
                int MaxDocnum = Convert.ToInt32(dbHrPayroll.TrnsPromotionAdvice.Max(x => x.DocNum));
                return MaxDocnum + 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.TrnsPromotionAdvice select a;
            Int32 i = 0;
            foreach (TrnsPromotionAdvice OneLine in oCollection)
            {
                CodeIndex.Add(OneLine.ID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void PrintPromotionAdvice()
        {
            try
            {
                TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.RptCode.Contains("ProAdv") select a).FirstOrDefault();
                if (oReport != null)
                {
                    if (!String.IsNullOrEmpty(txDocNum.Value))
                    {
                        Int32 oDoc = (from a in dbHrPayroll.TrnsPromotionAdvice
                                                   where a.DocNum.ToString() == txDocNum.Value
                                                   select a).Count();
                        if (oDoc > 0)
                        {
                            Program.objHrmsUI.printRpt("ProAdv", true, "WHERE dbo.TrnsPromotionAdvice.DocNum = '" + txDocNum.Value.Trim() + "'", "");
                        }
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Attach Promotion Advice Reports First.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception @ PrintPromotionAdvice : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
        
        
    }
}

