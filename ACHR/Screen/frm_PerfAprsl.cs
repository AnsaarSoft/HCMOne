using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_PerfAprsl : HRMSBaseForm
    {

        #region "Global Variable"
        private SAPbouiCOM.Button btnMain, btnCancel;
        private SAPbouiCOM.Item ibtnMain, ibtnCancel;
        private SAPbouiCOM.DataTable oDBDataTable;
        private SAPbouiCOM.UserDataSource oUDS_Employee, oUDS_Grade, oUDS_DOJ, oUDS_AppraiserCode;
        private SAPbouiCOM.EditText txEmpName, txDateOfJoin, txPosition, txDocNum, txDocDate;
        private SAPbouiCOM.EditText txAppraiserName, txAppraiserPosition, txAppraiserDept, txRemarks, txTotalScore;
        private SAPbouiCOM.ComboBox cbEmpID, cbGrade, cbAppraiserCode, cbPerformancePlan;
        private SAPbouiCOM.Item ItxEmpName, ItxDateOfJoin, ItxPosition, IcbGrade, ItxDocNum, ItxDocDate;
        private SAPbouiCOM.Item ItxAppraiserName, ItxAppraiserPosition, ItxAppraiserDept, ItxRemarks, ItxTotalScore, IcbEmpID, IcbAppraiserCode, IcbPerformancePlan;
        private SAPbouiCOM.Matrix oMat;
        private SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.Column clNo, clID, clIsNew, clKeyResAreas, clSelf, clSelfRemarks, clRepMngr, clMngrRemarks, clTarget, clWeightage, clScore;
        private int CurrentPerfArslID = 0;

        Boolean flgNewDocLoad = true;
        Boolean flgFormMode = false;

        IEnumerable<TrnsPerformanceAppraisal> oCollection = null;
        #endregion

        #region "B1 Form Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            IntitializeForm();
            FillEmployeeCombo();
            FillGradeCombo();
            FillAppraiserCombo();
            FillKPIColumnCombo();
            FillSelfColumnCombo();
            FillReportingManagerColumnCombo();
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
                        oForm.Freeze(true);
                        dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                        MstEmployee Employee = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbEmpID.Value.ToString()) select v).FirstOrDefault();
                        txEmpName.Value = (Employee.FirstName == null ? "" : Employee.FirstName + " " + Employee.LastName);
                        txPosition.Value = (Employee.PositionName == null ? "" : Employee.PositionName);
                        txDateOfJoin.Value = (Employee.JoiningDate == null ? "" : ((DateTime)Employee.JoiningDate).ToString("yyyyMMdd"));
                        txTotalScore.Value = "";
                        oDBDataTable.Rows.Clear();
                        FillPerformancePlanCombo(cbPerformancePlan, Employee.EmpID);
                        oForm.Freeze(false);
                        break;
                    case "cb_PerPlan":
                        if (flgNewDocLoad)
                        {
                            MstEmployee oEmployee = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbEmpID.Value.ToString()) select v).FirstOrDefault();
                            FillMatrixOnEmoloyeeChange(oEmployee.ID);
                            oMat.LoadFromDataSource();
                        }
                        break;

                    case "cb_AprCode":
                        //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                        MstEmployee Appraiser = (from v in dbHrPayroll.MstEmployee where v.ID == int.Parse(cbAppraiserCode.Value.ToString()) select v).FirstOrDefault();
                        txAppraiserName.Value = (Appraiser.FirstName == null ? "" : Appraiser.FirstName + " " + Appraiser.LastName);
                        txAppraiserPosition.Value = (Appraiser.PositionName == null ? "" : Appraiser.PositionName);
                        txAppraiserDept.Value = (Appraiser.DepartmentName == null ? "" : Appraiser.DepartmentName);
                        break;
                    case "Mat":
                        {
                            switch (pVal.ColUID)
                            {
                                case "cl_Self":
                                case "cl_RepMngr":
                                    oForm.Freeze(true);
                                    oMat.FlushToDataSource();
                                    string Id = oDBDataTable.GetValue("Self", pVal.Row - 1);
                                    int Score = 0;
                                    if (!Id.Equals(""))
                                        Score += (int)((from v in dbHrPayroll.MstPerformanceAssessmentCriteria where v.Id == int.Parse(Id) select new { Score = v.Points }).Single()).Score;
                                    Id = oDBDataTable.GetValue("RepMngr", pVal.Row - 1);
                                    if (!Id.Equals(""))
                                        Score += (int)((from v in dbHrPayroll.MstPerformanceAssessmentCriteria where v.Id == int.Parse(Id) select new { Score = v.Points }).Single()).Score;
                                    oDBDataTable.SetValue("Score", pVal.Row - 1, (double)(Score / 2));
                                    CalculateTotalScore();
                                    oMat.LoadFromDataSource();
                                    oForm.Freeze(false);
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                oForm.Freeze(false);
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
                    //case "btn_first":
                    //    getFirstRecord();
                    //    break;
                    //case "btn_prev":
                    //    getPreviouRecord();
                    //    break;
                    //case "btn_next":
                    //    getNextRecord();
                    //    break;
                    //case "btn_last":
                    //    getLastRecord();
                    //    break;
                    //case "btn_new":
                    //    ChangeFormToAddMode();
                    //    break;
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
            flgNewDocLoad = true;
            ChangeFormToAddMode();
        }

        public override void fillFields()
        {
            base.fillFields();
            FillDocument();
        }

        #endregion

        #region "Local Method"

        private void IntitializeForm()
        {
            try
            {
                oForm.AutoManaged = true;

                oUDS_Employee = oForm.DataSources.UserDataSources.Add("EmpID", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbEmpID = oForm.Items.Item("cb_EmpID").Specific;
                IcbEmpID = oForm.Items.Item("cb_EmpID");
                cbEmpID.DataBind.SetBound(true, "", "EmpID");
                IcbEmpID.DisplayDesc = true;

                oForm.DataSources.UserDataSources.Add("EmpName", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 20);
                txEmpName = oForm.Items.Item("t_EmpName").Specific;
                ItxEmpName = oForm.Items.Item("t_EmpName");
                txEmpName.DataBind.SetBound(true, "", "EmpName");

                oUDS_DOJ = oForm.DataSources.UserDataSources.Add("DOJ", SAPbouiCOM.BoDataType.dt_DATE);
                txDateOfJoin = oForm.Items.Item("t_DOJ").Specific;
                ItxDateOfJoin = oForm.Items.Item("t_DOJ");
                txDateOfJoin.DataBind.SetBound(true, "", "DOJ");

                oForm.DataSources.UserDataSources.Add("Position", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                txPosition = oForm.Items.Item("t_Pos").Specific;
                ItxPosition = oForm.Items.Item("t_Pos");
                txPosition.DataBind.SetBound(true, "", "Position");

                oUDS_Grade = oForm.DataSources.UserDataSources.Add("Grade", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                cbGrade = oForm.Items.Item("cb_Grade").Specific;
                IcbGrade = oForm.Items.Item("cb_Grade");
                cbGrade.DataBind.SetBound(true, "", "Grade");

                oForm.DataSources.UserDataSources.Add("DocNum", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txDocNum = oForm.Items.Item("t_DocNum").Specific;
                ItxDocNum = oForm.Items.Item("t_DocNum");
                txDocNum.DataBind.SetBound(true, "", "DocNum");

                oForm.DataSources.UserDataSources.Add("DocDate", SAPbouiCOM.BoDataType.dt_DATE);
                txDocDate = oForm.Items.Item("t_DocDate").Specific;
                ItxDocDate = oForm.Items.Item("t_DocDate");
                txDocDate.DataBind.SetBound(true, "", "DocDate");

                oUDS_AppraiserCode = oForm.DataSources.UserDataSources.Add("AprCode", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                cbAppraiserCode = oForm.Items.Item("cb_AprCode").Specific;
                IcbAppraiserCode = oForm.Items.Item("cb_AprCode");
                cbAppraiserCode.DataBind.SetBound(true, "", "AprCode");
                IcbAppraiserCode.DisplayDesc = true;

                oForm.DataSources.UserDataSources.Add("AprName", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 20);
                txAppraiserName = oForm.Items.Item("t_AprName").Specific;
                ItxAppraiserName = oForm.Items.Item("t_AprName");
                txAppraiserName.DataBind.SetBound(true, "", "AprName");

                oForm.DataSources.UserDataSources.Add("AprPos", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                txAppraiserPosition = oForm.Items.Item("t_AprPos").Specific;
                ItxAppraiserPosition = oForm.Items.Item("t_AprPos");
                txAppraiserPosition.DataBind.SetBound(true, "", "AprPos");

                oForm.DataSources.UserDataSources.Add("AprDept", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                txAppraiserDept = oForm.Items.Item("t_AprDept").Specific;
                ItxAppraiserDept = oForm.Items.Item("t_AprDept");
                txAppraiserDept.DataBind.SetBound(true, "", "AprDept");

                oForm.DataSources.UserDataSources.Add("TotalScore", SAPbouiCOM.BoDataType.dt_PERCENT);
                txTotalScore = oForm.Items.Item("t_TScore").Specific;
                ItxTotalScore = oForm.Items.Item("t_TScore");
                txTotalScore.DataBind.SetBound(true, "", "TotalScore");

                oForm.DataSources.UserDataSources.Add("Remark", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txRemarks = oForm.Items.Item("t_Remark").Specific;
                ItxRemarks = oForm.Items.Item("t_Remark");
                txRemarks.DataBind.SetBound(true, "", "Remark");

                oForm.DataSources.UserDataSources.Add("PerPlan", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                cbPerformancePlan = oForm.Items.Item("cb_PerPlan").Specific;
                IcbPerformancePlan = oForm.Items.Item("cb_PerPlan");
                txRemarks.DataBind.SetBound(true, "", "PerPlan");


                oDBDataTable = oForm.DataSources.DataTables.Add("PerfAprslDetail");
                oDBDataTable.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("IsNew", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
                oDBDataTable.Columns.Add("KRA", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDBDataTable.Columns.Add("Self", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                oDBDataTable.Columns.Add("SelfRemark", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("RepMngr", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                oDBDataTable.Columns.Add("MngrRemark", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                oDBDataTable.Columns.Add("Target", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Weigh", SAPbouiCOM.BoFieldsType.ft_Percent);
                oDBDataTable.Columns.Add("Score", SAPbouiCOM.BoFieldsType.ft_Percent);

                oMat = oForm.Items.Item("Mat").Specific;
                oColumns = oMat.Columns;

                clNo = oColumns.Item("cl_no");
                clNo.DataBind.Bind("PerfAprslDetail", "No");

                clID = oColumns.Item("cl_ID");
                clID.DataBind.Bind("PerfAprslDetail", "ID");

                clIsNew = oColumns.Item("cl_IsNew");
                clIsNew.DataBind.Bind("PerfAprslDetail", "IsNew");

                clKeyResAreas = oColumns.Item("cl_KRA");
                clKeyResAreas.DataBind.Bind("PerfAprslDetail", "KRA");

                clSelf = oColumns.Item("cl_Self");
                clSelf.DataBind.Bind("PerfAprslDetail", "Self");

                clSelfRemarks = oColumns.Item("cl_SelfRem");
                clSelfRemarks.DataBind.Bind("PerfAprslDetail", "SelfRemark");

                clRepMngr = oColumns.Item("cl_RepMngr");
                clRepMngr.DataBind.Bind("PerfAprslDetail", "RepMngr");

                clMngrRemarks = oColumns.Item("cl_MngrRem");
                clMngrRemarks.DataBind.Bind("PerfAprslDetail", "MngrRemark");

                clTarget = oColumns.Item("cl_Trgt");
                clTarget.DataBind.Bind("PerfAprslDetail", "Target");

                clWeightage = oColumns.Item("cl_Weigh");
                clWeightage.DataBind.Bind("PerfAprslDetail", "Weigh");

                clScore = oColumns.Item("cl_Score");
                clScore.DataBind.Bind("PerfAprslDetail", "Score");

                GetData();
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitializeDocument()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }

        }

        private void FillEmployeeCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                //var Records = from v in dbHrPayroll.MstEmployee where v.FlgActive == true select v;
                String querycheck = @"SELECT DISTINCT dbo.MstEmployee.ID, dbo.MstEmployee.EmpID, dbo.MstEmployee.FirstName, dbo.MstEmployee.MiddleName, dbo.MstEmployee.LastName
                                      FROM dbo.TrnsPerformancePlan INNER JOIN dbo.MstEmployee ON dbo.TrnsPerformancePlan.EmpID = dbo.MstEmployee.ID
                                      ORDER BY dbo.MstEmployee.EmpID";
                var Records = dbHrPayroll.ExecuteQuery<MstEmployee>(querycheck);
                foreach (var Record in Records)
                {
                    cbEmpID.ValidValues.Add(Record.ID.ToString(), Convert.ToString(Record.EmpID.ToString() + " : " + Record.FirstName + " " + Record.MiddleName + " " + Record.LastName));
                }
                //IcbEmpID.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillGradeCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstGrading);
                var Records = from v in dbHrPayroll.MstGrading select v;
                foreach (var Record in Records)
                {
                    cbGrade.ValidValues.Add(Record.ID.ToString(), Record.Code);
                }
                IcbGrade.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillAppraiserCombo()
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
                    cbAppraiserCode.ValidValues.Add(Record.ID.ToString(), Convert.ToString( Record.EmpID + " : " + Record.FirstName + " " + Record.MiddleName + " " + Record.LastName));
                }
                //IcbAppraiserCode.DisplayDesc = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillKPIColumnCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsKPI);
                var Records = from v in dbHrPayroll.TrnsKPI select v;
                foreach (var Record in Records)
                {
                    clKeyResAreas.ValidValues.Add(Record.ID.ToString(), Record.KeyObjectives);
                }
                clKeyResAreas.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillSelfColumnCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstPerformanceAssessmentCriteria);
                var Records = from v in dbHrPayroll.MstPerformanceAssessmentCriteria select v;
                foreach (var Record in Records)
                {
                    clSelf.ValidValues.Add(Record.Id.ToString(), Record.Description);
                }
                clSelf.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillPerformancePlanCombo(SAPbouiCOM.ComboBox pCombo, String pEmpID)
        {
            try
            {
                IEnumerable<TrnsPerformancePlan> oDocuments = from a in dbHrPayroll.TrnsPerformancePlan
                                                              where a.MstEmployee.EmpID == pEmpID
                                                              select a;

                if (pCombo.ValidValues.Count > 0)
                {
                    Int32 ComboCount = pCombo.ValidValues.Count;
                    for (Int32 i = ComboCount - 1; i >= 0; i--)
                    {
                        pCombo.ValidValues.Remove(pCombo.ValidValues.Item(i).Value);
                    }
                    foreach (TrnsPerformancePlan One in oDocuments)
                    {
                        pCombo.ValidValues.Add(One.Id.ToString(), One.PlanNo.ToString());

                    }
                }
                else
                {
                    foreach (TrnsPerformancePlan One in oDocuments)
                    {
                        pCombo.ValidValues.Add(One.Id.ToString(), One.PlanNo.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception FillPerformancePlan Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillReportingManagerColumnCombo()
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstPerformanceAssessmentCriteria);
                var Records = from v in dbHrPayroll.MstPerformanceAssessmentCriteria select v;
                foreach (var Record in Records)
                {
                    clRepMngr.ValidValues.Add(Record.Id.ToString(), Record.Description);
                }
                clRepMngr.DisplayDesc = true;
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
                int MaxDocnum = Convert.ToInt32(dbHrPayroll.TrnsPerformanceAppraisal.Max(x => x.DocNum));
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
                CurrentPerfArslID = 0;
                oUDS_Employee.Value = "";
                txEmpName.Value = "";
                txDateOfJoin.Value = "";
                txPosition.Value = "";
                oUDS_Grade.Value = "";
                txDocNum.Value = GetNextDocnum().ToString();
                txDocDate.Value = "";
                oUDS_AppraiserCode.Value = "";
                txAppraiserName.Value = "";
                txAppraiserPosition.Value = "";
                txAppraiserDept.Value = "";
                txTotalScore.Value = "";
                txRemarks.Value = "";
                oMat.Clear();
                AddBlankRow();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillMatrixOnEmoloyeeChange(int EmpId)
        {
            try
            {
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.TrnsPerformancePlan);
                var Heads = from v in dbHrPayroll.TrnsPerformancePlan where v.EmpID == EmpId && v.Id.ToString() == cbPerformancePlan.Value select v;
                int i = 0;
                foreach (var Head in Heads)
                {
                    var Records = from v in Head.TrnsPerformancePlanDetail select v;
                    foreach (var Record in Records)
                    {
                        oDBDataTable.Rows.Add(1);
                        oDBDataTable.SetValue("No", i, i + 1);
                        oDBDataTable.SetValue("IsNew", i, "N");
                        oDBDataTable.SetValue("KRA", i, Record.KPIID);
                        oDBDataTable.SetValue("Weigh", i, (double)Record.WeightagePer);
                        oDBDataTable.SetValue("Target", i, (double)Record.TargetPer);
                        i++;
                    }
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
                for (int i = 0; i < oDBDataTable.Rows.Count; i++)
                {
                    TotalScore += oDBDataTable.GetValue("Score", i); ;
                }
                txTotalScore.Value = TotalScore.ToString();
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
                flgNewDocLoad = false;
                TrnsPerformanceAppraisal oDoc = oCollection.ElementAt<TrnsPerformanceAppraisal>(currentRecord);
                if (oDoc != null)
                {
                    FillPerformancePlanCombo(cbPerformancePlan, oDoc.MstEmployee.EmpID);
                }
                cbPerformancePlan.Select(Convert.ToString(oDoc.PlanNumber), SAPbouiCOM.BoSearchKey.psk_ByValue);
                oUDS_Employee.Value = (oDoc.EmpID.ToString() == null ? "" : oDoc.EmpID.ToString());
                txEmpName.Value = (oDoc.EmpName == null ? "" : oDoc.EmpName);
                txDateOfJoin.Value = ((DateTime)oDoc.DateOfJoining).ToString("yyyyMMdd");
                txPosition.Value = (oDoc.Position == null ? "" : oDoc.Position);
                oUDS_Grade.Value = (oDoc.Grade == null ? "" : oDoc.Grade);
                txDocNum.Value = (oDoc.DocNum.ToString() == null ? "" : oDoc.DocNum.ToString());
                txDocDate.Value = ((DateTime)oDoc.DocDate).ToString("yyyyMMdd");
                oUDS_AppraiserCode.Value = (oDoc.AppraiserID.ToString() == null ? "" : oDoc.AppraiserID.ToString());
                txAppraiserName.Value = (oDoc.AppraiserName == null ? "" : oDoc.AppraiserName);
                txAppraiserPosition.Value = (oDoc.AppraiserPosition == null ? "" : oDoc.AppraiserPosition);
                txAppraiserDept.Value = (oDoc.AppraiserDepartment == null ? "" : oDoc.AppraiserDepartment);
                txTotalScore.Value = oDoc.TotalScore.ToString();
                txRemarks.Value = (oDoc.Remarks == null ? "" : oDoc.Remarks);

                //var Records = from v in oDoc.TrnsPerformanceAppraisalDetail select v;
                oDBDataTable.Rows.Clear();
                int i = 0;
                foreach (TrnsPerformanceAppraisalDetail Line in oDoc.TrnsPerformanceAppraisalDetail)
                {
                    oDBDataTable.Rows.Add(1);
                    oDBDataTable.SetValue("No", i, i + 1);
                    oDBDataTable.SetValue("ID", i, Line.ID);
                    oDBDataTable.SetValue("IsNew", i, "N");
                    oDBDataTable.SetValue("KRA", i, Line.KPIID);
                    oDBDataTable.SetValue("Self", i, Line.SelfAppraisal);
                    oDBDataTable.SetValue("SelfRemark", i, Line.SelfRemarks);
                    oDBDataTable.SetValue("RepMngr", i, Line.ReportingManager);
                    oDBDataTable.SetValue("MngrRemark", i, Line.ManagerRemarks);
                    oDBDataTable.SetValue("Weigh", i, (double)Line.Weightage);
                    oDBDataTable.SetValue("Target", i, (double)Line.TargetPer);
                    oDBDataTable.SetValue("Score", i, Line.Score);
                    i++;
                }
                oMat.LoadFromDataSource();
                CurrentPerfArslID = oDoc.ID;

                if (oDoc != null)
                {
                    TrnsPromotionAdvice oPromotionAdv = (from a in dbHrPayroll.TrnsPromotionAdvice where a.TrnsPerformancePlan.Id == oDoc.TrnsPerformancePlan.Id select a).FirstOrDefault();
                    if (oPromotionAdv != null)
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
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
                else
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
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
                    oApplication.StatusBar.SetText("employee not selected", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (txDocDate.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText("DocDate not entered", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (cbAppraiserCode.Value.Equals(""))
                {
                    oApplication.StatusBar.SetText("Appraiser Not Selected", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    BubbleEvent = false;
                    return;
                }
                else if (String.IsNullOrEmpty(cbPerformancePlan.Value))
                {
                    oApplication.StatusBar.SetText("Performance Plan Not Selected", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                Int32 ExistCount = 0;
                String PlanNumber, EmployeeID;
                PlanNumber = cbPerformancePlan.Value.Trim();
                EmployeeID = cbEmpID.Value.Trim();
                ExistCount = (from a in dbHrPayroll.TrnsPerformanceAppraisal
                             where a.PlanNumber.ToString() == PlanNumber && a.EmpID.ToString() == EmployeeID
                             select a).Count();
                if (ExistCount > 0)
                {
                    oApplication.StatusBar.SetText("Two Appraisals Can't be added on Single Plan.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                string Self, SelfRemarks, RepManager, RepManagerRemarks;
                for (int i = 1; i <= oMat.RowCount; i++)
                {
                    Self = (oMat.Columns.Item("cl_Self").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    SelfRemarks = (oMat.Columns.Item("cl_SelfRem").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    RepManager = (oMat.Columns.Item("cl_RepMngr").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    RepManagerRemarks = (oMat.Columns.Item("cl_MngrRem").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    if (Self.Equals("") || SelfRemarks.Equals("") || RepManager.Equals("") || RepManagerRemarks.Equals(""))
                    {
                        oApplication.StatusBar.SetText("Manadatory Fields Are Empty", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                        return;
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
                        //FillDocument((from v in dbHrPayroll.TrnsPerformanceAppraisal where v.ID == CurrentPerfArslID select v).Single());
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
                TrnsPerformanceAppraisal PerfAprslHead = new TrnsPerformanceAppraisal();
                PerfAprslHead.EmpID = int.Parse(cbEmpID.Value.Trim());
                PerfAprslHead.EmpName = txEmpName.Value.Trim();
                PerfAprslHead.DateOfJoining = DateTime.ParseExact(txDateOfJoin.Value.ToString().Trim(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprslHead.Position = txPosition.Value.Trim();
                PerfAprslHead.Grade = cbGrade.Value.Trim();
                PerfAprslHead.DocNum = int.Parse(txDocNum.Value.Trim());
                PerfAprslHead.DocDate = DateTime.ParseExact(txDocDate.Value.ToString().Trim(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprslHead.AppraiserID = int.Parse(cbAppraiserCode.Value.Trim());
                PerfAprslHead.AppraiserName = txAppraiserName.Value.Trim();
                PerfAprslHead.AppraiserPosition = txAppraiserPosition.Value.Trim();
                PerfAprslHead.AppraiserDepartment = txAppraiserDept.Value.Trim();
                Int32 PerformancePlanID = int.Parse(cbPerformancePlan.Value.Trim());
                //PerfAprslHead.PlanNumber = int.Parse(cbPerformancePlan.Value.Trim());
                TrnsPerformancePlan oPP = (from a in dbHrPayroll.TrnsPerformancePlan where a.Id == PerformancePlanID select a).FirstOrDefault();
                if (oPP != null)
                    PerfAprslHead.TrnsPerformancePlan = oPP;

                oMat.FlushToDataSource();
                for (int i = 0; i < oDBDataTable.Rows.Count; i++)
                {
                    TrnsPerformanceAppraisalDetail PerfAprslDetail = new TrnsPerformanceAppraisalDetail();
                    //PerfAprslDetail.KPIID = oDBDataTable.GetValue("KRA", i);
                    Int32 PlanID = oDBDataTable.GetValue("KRA", i);
                    //TrnsPerformancePlan oPPD = (from a in dbHrPayroll.TrnsPerformancePlan where a.Id == PlanID select a).FirstOrDefault();
                    TrnsKPI oKPI = (from a in dbHrPayroll.TrnsKPI where a.ID == PlanID select a).FirstOrDefault();
                    PerfAprslDetail.TrnsKPI = oKPI;
                    PerfAprslDetail.SelfAppraisal = oDBDataTable.GetValue("Self", i);
                    PerfAprslDetail.SelfRemarks = oDBDataTable.GetValue("SelfRemark", i);
                    PerfAprslDetail.ReportingManager = oDBDataTable.GetValue("RepMngr", i);
                    PerfAprslDetail.ManagerRemarks = oDBDataTable.GetValue("MngrRemark", i);
                    PerfAprslDetail.Weightage = (decimal)oDBDataTable.GetValue("Weigh", i);
                    PerfAprslDetail.TargetPer = (decimal)oDBDataTable.GetValue("Target", i);
                    PerfAprslDetail.Score = Convert.ToString(oDBDataTable.GetValue("Score", i));
                    PerfAprslDetail.CreateDate = DateTime.Now;
                    PerfAprslDetail.UserId = oCompany.UserSignature.ToString();
                    PerfAprslDetail.UpdatedBy = oCompany.UserName;
                    PerfAprslHead.TrnsPerformanceAppraisalDetail.Add(PerfAprslDetail);
                }
                PerfAprslHead.TotalScore = decimal.Parse(txTotalScore.Value.ToString());
                PerfAprslHead.Remarks = txRemarks.Value;
                PerfAprslHead.CreateDate = DateTime.Now;
                PerfAprslHead.UserId = oCompany.UserName.Trim();
                PerfAprslHead.UpdatedBy = oCompany.UserName.Trim();
                dbHrPayroll.TrnsPerformanceAppraisal.InsertOnSubmit(PerfAprslHead);
                dbHrPayroll.SubmitChanges();
                CurrentPerfArslID = PerfAprslHead.ID;
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                ChangeFormToAddMode();
                GetData();
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
                TrnsPerformanceAppraisal PerfAprslHead = (from v in dbHrPayroll.TrnsPerformanceAppraisal where v.ID == CurrentPerfArslID select v).Single();
                PerfAprslHead.EmpID = int.Parse(cbEmpID.Value);
                PerfAprslHead.EmpName = txEmpName.Value;
                PerfAprslHead.DateOfJoining = DateTime.ParseExact(txDateOfJoin.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprslHead.Position = txPosition.Value;
                PerfAprslHead.Grade = cbGrade.Value;
                PerfAprslHead.DocNum = int.Parse(txDocNum.Value);
                PerfAprslHead.DocDate = DateTime.ParseExact(txDocDate.Value.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                PerfAprslHead.AppraiserID = int.Parse(cbAppraiserCode.Value);
                PerfAprslHead.AppraiserName = txAppraiserName.Value;
                PerfAprslHead.AppraiserPosition = txAppraiserPosition.Value;
                PerfAprslHead.AppraiserDepartment = txAppraiserDept.Value;
                oMat.FlushToDataSource();

                int DetailID = 0;
                for (int i = 0; i < oDBDataTable.Rows.Count; i++)
                {
                    DetailID = oDBDataTable.GetValue("ID", i);
                    TrnsPerformanceAppraisalDetail PerfAprslDetail = (from v in dbHrPayroll.TrnsPerformanceAppraisalDetail where v.ID == DetailID select v).Single();
                    //PerfAprslDetail.KPIID = oDBDataTable.GetValue("KRA", i);
                    PerfAprslDetail.SelfAppraisal = oDBDataTable.GetValue("Self", i);
                    PerfAprslDetail.SelfRemarks = oDBDataTable.GetValue("SelfRemark", i);
                    PerfAprslDetail.ReportingManager = oDBDataTable.GetValue("RepMngr", i);
                    PerfAprslDetail.ManagerRemarks = oDBDataTable.GetValue("MngrRemark", i);
                    PerfAprslDetail.Weightage = (decimal)oDBDataTable.GetValue("Weigh", i);
                    PerfAprslDetail.TargetPer = (decimal)oDBDataTable.GetValue("Target", i);
                    PerfAprslDetail.Score = Convert.ToString(oDBDataTable.GetValue("Score", i));
                    PerfAprslDetail.UpdateDate = DateTime.Now;
                    PerfAprslDetail.UserId = oCompany.UserSignature.ToString();
                    PerfAprslDetail.UpdatedBy = oCompany.UserName;
                    PerfAprslHead.TrnsPerformanceAppraisalDetail.Add(PerfAprslDetail);
                }
                PerfAprslHead.TotalScore = decimal.Parse(txTotalScore.Value.ToString());
                PerfAprslHead.Remarks = txRemarks.Value;
                PerfAprslHead.UpdateDate = DateTime.Now;
                PerfAprslHead.UpdatedBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Document Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.TrnsPerformanceAppraisal select a;
            Int32 i = 0;
            foreach (TrnsPerformanceAppraisal oDoc in oCollection)
            {
                CodeIndex.Add(oDoc.ID, oDoc.DocNum);
                i++;
            }
            totalRecord = i;
        }

        #endregion
    }
}
