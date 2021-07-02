using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_360DegPerf:HRMSBaseForm
    {

        #region "Global Variable"
        
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.UserDataSource oUDS_Employee, oUDS_Status, oUDS_Series, oUDS_ProfStatus;
        private SAPbouiCOM.UserDataSource oUDS_PerfPlanNo, oUDS_PlanFromDate, oUDS_PlanToDate, oUDS_CmptncyGrp, oUDS_LinMngr;
        private SAPbouiCOM.EditText txFirstName, txLastName, txDesignation, txDepartment, txBranch, txDocNum, txtStatus;
        private SAPbouiCOM.EditText txDocDate, txPlanFromDate, txPlanToDate, txTotalScore, txOverallRating, txRemarks;
        private SAPbouiCOM.ComboBox cbEmpID, cbStatus, cbSeries, cbProfStatus, cbPerfPlanNo, cbCmptncyGrp, cbLnMngr;
        private SAPbouiCOM.Item ItxFirstName, ItxLastName, ItxDesignation, ItxDepartment, ItxBranch, ItxDocNum, ItxDocDate, ItxPlanFromDate, itxtStatus;
        private SAPbouiCOM.Item ItxPlanToDate, IcbLnMngr, ItxTotalScore, ItxOverallRating, ItxRemarks, IcbEmpID, IcbStatus, IcbSeries, IcbProfStatus, IcbPerfPlanNo, IcbCmptncyGrp;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column clNo, clID, clIsNew, clCmptncy, clSelf, clMngr, clMngrScr, clPeer;
        private SAPbouiCOM.Column clPeerScr, clSubOrd, clSubOrdScr, clCust, clCustScr, clSupplier, clSupplierScore, clTotal;
        private int Current360DegPerfAprslID;
        
        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            IntitializeForm();
            FillEmployeeCombo();
            FillSeriesCombo();
            //FillLineManagerCombo();
            //FillCompetencyGrpCombo();
            FillCompetncyColumnCombo();
            //FillManagerColumnCombo();
            FillPeerColumnCombo();
            FillSubOrdColumnCombo();
            oUDS_Series.Value = "-1";
            txDocNum.Value = GetNextDocnum().ToString();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            //AddBlankRow();
            oForm.Freeze(false);
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
        
        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {
                                case "cl_Self":
                                case "cl_MngrScr":
                                case "cl_PeerScr":
                                case "cl_SOrdScr":
                                case "cl_CustScr":
                                case "cl_SuppScr":
                                    oForm.Freeze(true);
                                    oMat.FlushToDataSource();
                                    double TotalScore = oDBDataTable.GetValue("Self", pVal.Row - 1) + oDBDataTable.GetValue("MngrScr", pVal.Row - 1) +
                                        oDBDataTable.GetValue("PeerScr", pVal.Row - 1) + oDBDataTable.GetValue("SubOrdScr", pVal.Row - 1) +
                                        oDBDataTable.GetValue("CustScr", pVal.Row - 1) + oDBDataTable.GetValue("SuppScr", pVal.Row - 1);
                                    oDBDataTable.SetValue("Total", pVal.Row - 1, TotalScore.ToString());
                                    oMat.LoadFromDataSource();
                                    CalculateTotalScore();
                                    oForm.Freeze(false);
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
        
        public override void etBeforeCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "cb_CmpGrp":
                        if (cbEmpID.Value.Equals(""))
                        {
                            oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullEmployee"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
        
        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "cb_EmpID":
                        oForm.Freeze(true);
                        dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                        MstEmployee Employee = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbEmpID.Value.ToString()) select v).Single();
                        txFirstName.Value = (Employee.FirstName == null ? "" : Employee.FirstName);
                        txLastName.Value = (Employee.LastName == null ? "" : Employee.LastName);
                        txDesignation.Value = (Employee.DesignationName == null ? "" : Employee.DesignationName);
                        txDepartment.Value = (Employee.DepartmentName == null ? "" : Employee.DepartmentName);
                        txBranch.Value = (Employee.BranchName == null ? "" : Employee.BranchName);
                        FillPlanNoCombo(Employee.ID);
                        FillCompetencyGrpCombo(Employee.ID);
                        if (Employee.Manager != null)
                        {
                            Int32 ManagerID = Convert.ToInt32(Employee.Manager);
                            FillManagerColumnCombo(ManagerID);
                            FillLineManagerCombo(ManagerID);
                        }
                        txPlanFromDate.Value = "";
                        txPlanToDate.Value = "";
                        txTotalScore.Value = "";
                        txOverallRating.Value = "";
                        oUDS_CmptncyGrp.Value = "";
                        oDBDataTable.Rows.Clear();
                        oMat.LoadFromDataSource();
                        oForm.Freeze(false);
                        break;
                    case "cb_PPNo":
                        TrnsPerformancePlan Record = (from v in dbHrPayroll.TrnsPerformancePlan where v.PlanNo == int.Parse(cbPerfPlanNo.Value) select v).Single();
                        txPlanFromDate.Value = ((DateTime)Record.FromDate).ToString("yyyyMMdd");
                        txPlanToDate.Value = ((DateTime)Record.ToDate).ToString("yyyyMMdd");
                        break;
                    case "cb_CmpGrp":
                        oForm.Freeze(true);
                        oDBDataTable.Rows.Clear();
                        txTotalScore.Value = "0.00";
                        FillMatrixOnCmptncyGrpChange();
                        oMat.LoadFromDataSource();
                        oForm.Freeze(false);
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

        public override void getFirstRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 select v).Count();
                if (Count == 0)
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                else
                {
                    oForm.Freeze(true);
                    TrnsPerformanceAppraisal360 Head = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 orderby v.ID ascending select v).First();
                    Head = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 orderby v.ID ascending select v).First();
                    GetDataFromDatasource(Head);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void getPreviouRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 where v.ID < Current360DegPerfAprslID select v).Count();
                if (Count == 0)
                {
                    getFirstRecord();
                }
                else
                {
                    oForm.Freeze(true);
                    TrnsPerformanceAppraisal360 Head = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 where v.ID < Current360DegPerfAprslID orderby v.ID descending select v).First();
                    GetDataFromDatasource(Head);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void getNextRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 where v.ID > Current360DegPerfAprslID orderby v.ID ascending select v).Count();
                if (Count == 0)
                {
                    getLastRecord();
                }
                else
                {
                    oForm.Freeze(true);
                    TrnsPerformanceAppraisal360 Head = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 where v.ID > Current360DegPerfAprslID orderby v.ID ascending select v).First();
                    GetDataFromDatasource(Head);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void getLastRecord()
        {
            try
            {
                int Count = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 select v).Count();
                if (Count == 0)
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NoRecord"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                else
                {
                    oForm.Freeze(true);
                    TrnsPerformanceAppraisal360 Head = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 orderby v.ID descending select v).First();
                    GetDataFromDatasource(Head);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oForm.Freeze(false);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Events"

        private void IntitializeForm()
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

                oUDS_ProfStatus = oForm.DataSources.UserDataSources.Add("ProfStatus", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 10);
                cbProfStatus = oForm.Items.Item("cb_PStatus").Specific;
                IcbProfStatus = oForm.Items.Item("cb_PStatus");
                cbProfStatus.DataBind.SetBound(true, "", "ProfStatus");

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

                oUDS_CmptncyGrp = oForm.DataSources.UserDataSources.Add("CmptncyGrp", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbCmptncyGrp = oForm.Items.Item("cb_CmpGrp").Specific;
                IcbCmptncyGrp = oForm.Items.Item("cb_CmpGrp");
                cbCmptncyGrp.DataBind.SetBound(true, "", "CmptncyGrp");

                oUDS_LinMngr = oForm.DataSources.UserDataSources.Add("LnMngr", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbLnMngr = oForm.Items.Item("cb_LnMngr").Specific;
                IcbLnMngr = oForm.Items.Item("cb_LnMngr");
                cbLnMngr.DataBind.SetBound(true, "", "LnMngr");

                oForm.DataSources.UserDataSources.Add("TotalScore", SAPbouiCOM.BoDataType.dt_PERCENT);
                txTotalScore = oForm.Items.Item("t_TScore").Specific;
                ItxTotalScore = oForm.Items.Item("t_TScore");
                txTotalScore.DataBind.SetBound(true, "", "TotalScore");

                oForm.DataSources.UserDataSources.Add("OvrRtng", SAPbouiCOM.BoDataType.dt_PERCENT);
                txOverallRating = oForm.Items.Item("t_OvrRtng").Specific;
                ItxOverallRating = oForm.Items.Item("t_OvrRtng");
                txOverallRating.DataBind.SetBound(true, "", "OvrRtng");

                oForm.DataSources.UserDataSources.Add("Remarks", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 200);
                txRemarks = oForm.Items.Item("t_Remark").Specific;
                ItxRemarks = oForm.Items.Item("t_Remark");
                txRemarks.DataBind.SetBound(true, "", "Remarks");

                oDBDataTable = oForm.DataSources.DataTables.Add("360DegreeDetail");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("IsNew", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                oDBDataTable.Columns.Add("CmptncyGrp", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("Self", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Mngr", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("MngrScr", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Peer", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("PeerScr", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("SubOrd", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("SubOrdScr", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Cust", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 40);
                oDBDataTable.Columns.Add("CustScr", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Supp", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 40);
                oDBDataTable.Columns.Add("SuppScr", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_Percent);

                oMat = oForm.Items.Item("Mat").Specific;
                oColumns = oMat.Columns;

                clNo = oColumns.Item("cl_no");
                clNo.DataBind.Bind("360DegreeDetail", "No");

                clID = oColumns.Item("cl_ID");
                clID.DataBind.Bind("360DegreeDetail", "ID");

                clIsNew = oColumns.Item("cl_IsNew");
                clIsNew.DataBind.Bind("360DegreeDetail", "IsNew");

                clCmptncy = oColumns.Item("cl_Cmptncy");
                clCmptncy.DataBind.Bind("360DegreeDetail", "CmptncyGrp");

                clSelf = oColumns.Item("cl_Self");
                clSelf.DataBind.Bind("360DegreeDetail", "Self");

                clMngr = oColumns.Item("cl_Manager");
                clMngr.DataBind.Bind("360DegreeDetail", "Mngr");

                clMngrScr = oColumns.Item("cl_MngrScr");
                clMngrScr.DataBind.Bind("360DegreeDetail", "MngrScr");

                clPeer = oColumns.Item("cl_Peer");
                clPeer.DataBind.Bind("360DegreeDetail", "Peer");

                clPeerScr = oColumns.Item("cl_PeerScr");
                clPeerScr.DataBind.Bind("360DegreeDetail", "PeerScr");

                clSubOrd = oColumns.Item("cl_SubOrd");
                clSubOrd.DataBind.Bind("360DegreeDetail", "SubOrd");

                clSubOrdScr = oColumns.Item("cl_SOrdScr");
                clSubOrdScr.DataBind.Bind("360DegreeDetail", "SubOrdScr");

                clCust = oColumns.Item("cl_Cust");
                clCust.DataBind.Bind("360DegreeDetail", "Cust");

                clCustScr = oColumns.Item("cl_CustScr");
                clCustScr.DataBind.Bind("360DegreeDetail", "CustScr");

                clSupplier = oColumns.Item("cl_Supp");
                clSupplier.DataBind.Bind("360DegreeDetail", "Supp");

                clSupplierScore = oColumns.Item("cl_SuppScr");
                clSupplierScore.DataBind.Bind("360DegreeDetail", "SuppScr");

                clTotal = oColumns.Item("cl_Total");
                clTotal.DataBind.Bind("360DegreeDetail", "Total");

                base.fillCombo("PerApprsl_Status", cbProfStatus);
                IcbProfStatus.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetDataFromDatasource(TrnsPerformanceAppraisal360 Head)
        {
            try
            {
                oUDS_Employee.Value = Head.EmpID.ToString();
                txFirstName.Value = Head.EmpFirstName;
                txLastName.Value = Head.EmpLastName;
                txDesignation.Value = Head.Designation;
                txDepartment.Value = Head.Department;
                txBranch.Value = Head.Branch;
                oUDS_Status.Value = Head.DocStatus;
                oUDS_Series.Value = Head.Series.ToString();
                txDocNum.Value = Head.DocNum.ToString();
                txDocDate.Value = ((DateTime)Head.DocDate).ToString("yyyyMMdd");
                oUDS_ProfStatus.Value = Head.DocStatus;
                oUDS_PerfPlanNo.Value = Head.PlanNo.ToString();
                txPlanFromDate.Value = ((DateTime)Head.PerfPeriodFrom).ToString("yyyyMMdd");
                txPlanToDate.Value = ((DateTime)Head.PerfPeriodTo).ToString("yyyyMMdd");
                oUDS_CmptncyGrp.Value = Head.CompetencyGroupID.ToString();

                var Records = from v in Head.TrnsPerformanceAppraisal360Detail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (var Record in Records)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("ID", i, Record.ID);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("CmptncyGrp", i, Record.CompetencyID);
                    oDBDataTable.SetValue("Self", i, (double)Record.SelfScore);
                    oDBDataTable.SetValue("Mngr", i, Record.ManagerID);
                    oDBDataTable.SetValue("MngrScr", i, (double)Record.ScoreManager);
                    oDBDataTable.SetValue("Peer", i, Record.Peer);
                    oDBDataTable.SetValue("PeerScr", i, (double)Record.ScorePeer);
                    oDBDataTable.SetValue("SubOrd", i, Record.SubOrdinateID);
                    oDBDataTable.SetValue("SubOrdScr", i, (double)Record.ScoreSO);
                    oDBDataTable.SetValue("Cust", i, Record.Customer);
                    oDBDataTable.SetValue("CustScr", i, (double)Record.ScoreCustomer);
                    oDBDataTable.SetValue("Supp", i, Record.Supplier);
                    oDBDataTable.SetValue("SuppScr", i, (double)Record.ScoreSupplier);
                    oDBDataTable.SetValue("Total", i, (double)Record.LineTotal);
                    i++;
                }
                oMat.LoadFromDataSource();
                oUDS_LinMngr.Value = Head.LineManager.ToString();
                txTotalScore.Value = Head.TotalScore.ToString();
                txOverallRating.Value = Head.OverallRating.Value.ToString();
                txRemarks.Value = Head.Remarks;
                Current360DegPerfAprslID = Head.ID;
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
                //var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                String querycheck = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName
                                      FROM dbo.TrnsPerformancePlan INNER JOIN dbo.MstEmployee ON dbo.TrnsPerformancePlan.EmpID = dbo.MstEmployee.ID";
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(querycheck);
                foreach (var Record in Records)
                {
                    cbEmpID.ValidValues.Add(Record.ID.ToString(), Record.FirstName + " " + Record.LastName);
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
        
        private void FillLineManagerCombo(Int32 EMPID)
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                var Records = from v in dbHrPayroll.MstEmployee
                              where v.ID == EMPID 
                              select v;
                cbLnMngr.ValidValues.Add("-1", "Select Manager");
                foreach (var Record in Records)
                {
                    cbLnMngr.ValidValues.Add(Record.ID.ToString(), Record.FirstName + " " + Record.MiddleName + " " + Record.LastName);
                }
                IcbLnMngr.DisplayDesc = true;
                cbLnMngr.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillCompetencyGrpCombo(Int32 EMPID)
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstCompetencyGroup);
                //var Records = from v in dbHrPayroll.MstCompetencyGroup select v;
                String querycheck = @"
                                        SELECT 
	                                        DISTINCT dbo.MstCompetencyGroup.ID, dbo.MstCompetencyGroup.GroupID
                                        FROM   
	                                        dbo.TrnsCompetencyProfileDetail INNER JOIN
	                                        dbo.TrnsCompetencyProfile ON dbo.TrnsCompetencyProfileDetail.CPID = dbo.TrnsCompetencyProfile.ID INNER JOIN
	                                        dbo.MstCompetencyGroup ON dbo.TrnsCompetencyProfileDetail.CGID = dbo.MstCompetencyGroup.ID INNER JOIN
	                                        dbo.MstEmployee ON dbo.TrnsCompetencyProfile.EmpID = dbo.MstEmployee.ID
                                        WHERE     
	                                        dbo.MstEmployee.ID = " + EMPID.ToString() + "";
                var Records = dbHrPayroll.ExecuteQuery<MstCompetencyGroup>(querycheck);
                foreach (var Record in Records)
                {
                    cbCmptncyGrp.ValidValues.Add(Record.ID.ToString(), Record.GroupID);
                }
                IcbCmptncyGrp.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillCompetncyColumnCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsCompetencyProfileDetail);
                var Records = from v in dbHrPayroll.TrnsCompetencyProfileDetail select v;
                foreach (var Record in Records)
                {
                    clCmptncy.ValidValues.Add(Record.ID.ToString(), Record.Code);
                }
                clCmptncy.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillManagerColumnCombo( Int32 EmpID)
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                //String EMPID = cbEmpID.Value.Trim();
                if (EmpID > 0)
                {
                    var Records = from v in dbHrPayroll.MstEmployee
                                  where v.ID == EmpID && v.FlgActive == true 
                                  select v;
                    foreach (var Record in Records)
                    {
                        clMngr.ValidValues.Add(Record.ID.ToString(), Record.FirstName + " " + Record.LastName);
                    }
                    clMngr.DisplayDesc = true;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillPeerColumnCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                foreach (var Record in Records)
                {
                    clPeer.ValidValues.Add(Record.ID.ToString(), Record.FirstName + " " + Record.LastName);
                }
                clPeer.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillSubOrdColumnCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                foreach (var Record in Records)
                {
                    clSubOrd.ValidValues.Add(Record.ID.ToString(), Record.FirstName + " " + Record.LastName);
                }
                clSubOrd.DisplayDesc = true;
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
                IQueryable<TrnsPerformancePlan> Records;

                if (EmpID == -1)
                    Records = from v in dbHrPayroll.TrnsPerformancePlan select v;
                else
                    Records = from v in dbHrPayroll.TrnsPerformancePlan where v.EmpID == EmpID select v;

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
        
        public int GetNextDocnum()
        {
            try
            {
                int MaxDocnum = dbHrPayroll.TrnsPerformanceAppraisal360.Max(x => x.ID);
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
       
        private void ChangeFormToAddMode()
        {
            try
            {
                oForm.Freeze(true);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                Current360DegPerfAprslID = 0;
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
                oUDS_PerfPlanNo.Value = "";
                txPlanFromDate.Value = "";
                txPlanToDate.Value = "";
                oUDS_CmptncyGrp.Value = "";
                oUDS_LinMngr.Value = "";
                txTotalScore.Value = "";
                txOverallRating.Value = "";
                txRemarks.Value = "";
                oMat.Clear();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillMatrixOnCmptncyGrpChange()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsCompetencyProfileDetail);
                var Records = from v in dbHrPayroll.TrnsCompetencyProfileDetail where v.CGID == int.Parse(cbCmptncyGrp.Value) && v.TrnsCompetencyProfile.EmpID == int.Parse(cbEmpID.Value) select v;
                int i = 0;
                foreach (var Record in Records)
                {
                    oDBDataTable.Rows.Add(1); 
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("CmptncyGrp", i, Record.ID);
                    i++;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void CalculateTotalScore()
        {
            try
            {
                double TotalScore = 0.0;
                for (int i = 0; i < oDBDataTable.Rows .Count; i++)
                {
                    TotalScore += oDBDataTable.GetValue("Total", i);
                }
                txTotalScore.Value = TotalScore.ToString();
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
                //    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullSeries"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                else if (cbPerfPlanNo.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullPlanNo"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbCmptncyGrp.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullCmptncyGrp"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbLnMngr.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("Err_NullLinMngr"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }

                Int32 ExistsCount = 0;
                String PlanNumber, EmployeeID;
                PlanNumber = cbPerfPlanNo.Value.Trim();
                EmployeeID = cbEmpID.Value.Trim();
                ExistsCount = (from a in dbHrPayroll.TrnsPerformanceAppraisal360
                               where a.PlanNo.ToString() == PlanNumber && a.EmpID.ToString() == EmployeeID
                               select a).Count();
                if (ExistsCount > 0)
                {
                    oApplication.StatusBar.SetText("Duplicate Appraisal Not Allowed", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                string Manager, Peer, SubOrd, Customer, Supplier;
                for (int i = 1; i <= oMat.RowCount; i++)
                {
                    Manager = (oMat.Columns.Item("cl_Manager").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    Peer = (oMat.Columns.Item("cl_Peer").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    SubOrd = (oMat.Columns.Item("cl_SubOrd").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    Customer = (oMat.Columns.Item("cl_Cust").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    Supplier = (oMat.Columns.Item("cl_Supp").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (Manager.Equals("") || Peer.Equals("") || SubOrd.Equals("") || Customer.Equals("") || Supplier.Equals(""))
                    {
                        oApplication.StatusBar.SetText("Mandatory Fields are Missing", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                            GetDataFromDatasource((from v in dbHrPayroll.TrnsPerformanceAppraisal360 where v.ID == Current360DegPerfAprslID select v).Single());
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
                TrnsPerformanceAppraisal360  PerfAprsl360Head = new TrnsPerformanceAppraisal360();
                PerfAprsl360Head.EmpID = int.Parse(cbEmpID.Value);
                PerfAprsl360Head.EmpFirstName = txFirstName.Value;
                PerfAprsl360Head.EmpLastName = txLastName.Value;
                PerfAprsl360Head.Designation = txDesignation.Value;
                PerfAprsl360Head.Department = txDepartment.Value;
                PerfAprsl360Head.Branch = txBranch.Value;
                //PerfAprsl360Head.Status = int.Parse(cbStatus.Value);
                PerfAprsl360Head.Series = -1;
                PerfAprsl360Head.DocNum = int.Parse(txDocNum.Value);
                PerfAprsl360Head.DocDate = DateTime.ParseExact(txDocDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprsl360Head.DocStatus = cbProfStatus.Value;
                PerfAprsl360Head.PlanNo = int.Parse(cbPerfPlanNo.Value);
                PerfAprsl360Head.PerfPeriodFrom = DateTime.ParseExact(txPlanFromDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprsl360Head.PerfPeriodTo = DateTime.ParseExact(txPlanToDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprsl360Head.CompetencyGroupID = int.Parse(cbCmptncyGrp.Value);
                oMat.FlushToDataSource();

                for (int i = 0; i < oDBDataTable.Rows.Count; i++)
                {
                    TrnsPerformanceAppraisal360Detail PerfAprsl360Detail = new TrnsPerformanceAppraisal360Detail();
                    //string competencygroup = Convert.ToString(oDBDataTable.GetValue("CmptncyGrp", i));
                    //var compid = (from a in dbHrPayroll.TrnsCompetencyProfileDetail where a.TrnsCompetencyProfile.EmpID == Convert.ToInt32(cbPerfPlanNo.Value.Trim()) && a.Code == competencygroup select a).FirstOrDefault();
                    PerfAprsl360Detail.CompetencyID = oDBDataTable.GetValue("CmptncyGrp", i);
                    PerfAprsl360Detail.SelfScore = (decimal)oDBDataTable.GetValue("Self", i);
                    PerfAprsl360Detail.ManagerID = oDBDataTable.GetValue("Mngr", i);
                    PerfAprsl360Detail.ScoreManager = (decimal)oDBDataTable.GetValue("MngrScr", i);
                    PerfAprsl360Detail.Peer = oDBDataTable.GetValue("Peer", i);
                    PerfAprsl360Detail.ScorePeer = (decimal)oDBDataTable.GetValue("PeerScr", i);
                    PerfAprsl360Detail.SubOrdinateID = oDBDataTable.GetValue("SubOrd", i);
                    PerfAprsl360Detail.ScoreSO = (decimal)oDBDataTable.GetValue("SubOrdScr", i);
                    PerfAprsl360Detail.Customer = oDBDataTable.GetValue("Cust", i);
                    PerfAprsl360Detail.ScoreCustomer = (decimal)oDBDataTable.GetValue("CustScr", i);
                    PerfAprsl360Detail.Supplier = oDBDataTable.GetValue("Supp", i);
                    PerfAprsl360Detail.ScoreSupplier = (decimal)oDBDataTable.GetValue("SuppScr", i);
                    PerfAprsl360Detail.LineTotal = (decimal)oDBDataTable.GetValue("Total", i);
                    PerfAprsl360Detail.CreateDate = DateTime.Now;
                    PerfAprsl360Detail.UserId = oCompany.UserSignature.ToString();
                    PerfAprsl360Detail.UpdatedBy = oCompany.UserName;
                    PerfAprsl360Head.TrnsPerformanceAppraisal360Detail.Add(PerfAprsl360Detail);
                }
                PerfAprsl360Head.LineManager = int.Parse(cbLnMngr.Value);
                PerfAprsl360Head.TotalScore = decimal.Parse(txTotalScore.Value);
                PerfAprsl360Head.OverallRating = decimal.Parse(txOverallRating.Value);
                PerfAprsl360Head.Remarks = txRemarks.Value;
                PerfAprsl360Head.CreateDate = DateTime.Now;
                PerfAprsl360Head.UserId = oCompany.UserSignature.ToString();
                PerfAprsl360Head.UpdatedBy = oCompany.UserName;
                dbHrPayroll.TrnsPerformanceAppraisal360.InsertOnSubmit(PerfAprsl360Head);
                dbHrPayroll.SubmitChanges();
                Current360DegPerfAprslID = PerfAprsl360Head.ID;
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
                TrnsPerformanceAppraisal360 PerfAprsl360Head = (from v in dbHrPayroll.TrnsPerformanceAppraisal360 where v.ID == Current360DegPerfAprslID select v).Single();
                PerfAprsl360Head.EmpID = int.Parse(cbEmpID.Value);
                PerfAprsl360Head.EmpFirstName = txFirstName.Value;
                PerfAprsl360Head.EmpLastName = txLastName.Value;
                PerfAprsl360Head.Designation = txDesignation.Value;
                PerfAprsl360Head.Department = txDepartment.Value;
                PerfAprsl360Head.Branch = txBranch.Value;
                //PerfAprsl360Head.Status = int.Parse(cbStatus.Value);
                //PerfAprsl360Head.Series = int.Parse(cbSeries.Value);
                //PerfAprsl360Head.DocNum = int.Parse(txDocNum.Value);
                PerfAprsl360Head.DocDate = DateTime.ParseExact(txDocDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprsl360Head.DocStatus = cbProfStatus.Value;
                PerfAprsl360Head.PlanNo = int.Parse(cbPerfPlanNo.Value);
                PerfAprsl360Head.PerfPeriodFrom = DateTime.ParseExact(txPlanFromDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprsl360Head.PerfPeriodTo = DateTime.ParseExact(txPlanToDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprsl360Head.CompetencyGroupID = int.Parse(cbCmptncyGrp.Value);
                oMat.FlushToDataSource();

                int DetailID = 0;
                for (int i = 0; i < oDBDataTable.Rows.Count; i++)
                {
                    DetailID = oDBDataTable.GetValue("ID", i);
                    TrnsPerformanceAppraisal360Detail PerfAprsl360Detail = (from v in dbHrPayroll.TrnsPerformanceAppraisal360Detail where v.ID == DetailID select v).Single();
                    PerfAprsl360Detail.CompetencyID = oDBDataTable.GetValue("CmptncyGrp", i);
                    PerfAprsl360Detail.SelfScore = (decimal)oDBDataTable.GetValue("Self", i);
                    PerfAprsl360Detail.ManagerID = oDBDataTable.GetValue("Mngr", i);
                    PerfAprsl360Detail.ScoreManager = (decimal)oDBDataTable.GetValue("MngrScr", i);
                    PerfAprsl360Detail.Peer = oDBDataTable.GetValue("Peer", i);
                    PerfAprsl360Detail.ScorePeer = (decimal)oDBDataTable.GetValue("PeerScr", i);
                    PerfAprsl360Detail.SubOrdinateID = oDBDataTable.GetValue("SubOrd", i);
                    PerfAprsl360Detail.ScoreSO = (decimal)oDBDataTable.GetValue("SubOrdScr", i);
                    PerfAprsl360Detail.Customer = oDBDataTable.GetValue("Cust", i);
                    PerfAprsl360Detail.ScoreCustomer = (decimal)oDBDataTable.GetValue("CustScr", i);
                    PerfAprsl360Detail.Supplier = oDBDataTable.GetValue("Supp", i);
                    PerfAprsl360Detail.ScoreSupplier = (decimal)oDBDataTable.GetValue("SuppScr", i);
                    PerfAprsl360Detail.LineTotal = (decimal)oDBDataTable.GetValue("Total", i);
                    PerfAprsl360Detail.UpdateDate = DateTime.Now;
                    PerfAprsl360Detail.UserId = oCompany.UserSignature.ToString();
                    PerfAprsl360Detail.UpdatedBy = oCompany.UserName;
                }
                PerfAprsl360Head.LineManager = int.Parse(cbLnMngr.Value);
                PerfAprsl360Head.TotalScore = decimal.Parse(txTotalScore.Value);
                PerfAprsl360Head.OverallRating = decimal.Parse(txOverallRating.Value);
                PerfAprsl360Head.Remarks = txRemarks.Value;
                PerfAprsl360Head.CreateDate = DateTime.Now;
                PerfAprsl360Head.UserId = oCompany.UserSignature.ToString();
                PerfAprsl360Head.UpdatedBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        #endregion
    }
}
