using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_IntEvlSelection:HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.Button btnMain, btnCancel, btnCreateEmployee;
        SAPbouiCOM.IButtonCombo btnPrint, btnPrint2;
        SAPbouiCOM.EditText txtInterviewCall, txtInterviewDt, txtCandidateNo, txtCandidateName;
        SAPbouiCOM.EditText txtDocNum, txtDocStatus, txtCreatedOn, txtClosedOn;
        SAPbouiCOM.EditText txtOverallScore, txtKnowledgeSkill, txtTotalScore;
        SAPbouiCOM.EditText txtBudgetedSalary, txtRecommendedSalary, txtApprovedSalary, txtProbationValue;
        SAPbouiCOM.ComboBox cbAssestmentArea, cbPanelist, cbResult, cbProbationUnit, cbContractType, cbDocStatus;
        SAPbouiCOM.CheckBox chkSelected, chkCandidateAccepted, chkEmployeeAccepted;
        
        SAPbouiCOM.Matrix mtAssestment, mtPanelist, mtScoreBoard, mtElements;
        SAPbouiCOM.DataTable dtAssestment, dtPanelist, dtScoreBoard, dtElements;
        SAPbouiCOM.Column aId, aIsNew, aCriteria, aDescription, aMarks, aMarksObtain, aPanelist, aRequiredScore, aRemarks;
        SAPbouiCOM.Column pId, pIsNew, pAssestmentCode, pDescription, pMarks, pMarksObtain, pRemarks;
        SAPbouiCOM.Column sId, sIsNew, sAssesstmentArea, sAverageMarks, sRemarks;
        SAPbouiCOM.Column eId, eIsNew, eElementName, eElementType;

        SAPbouiCOM.Item ibtnMain, ibtnCancel, ibtnCreateEmployee;
        SAPbouiCOM.Item ibtnPrint, ibtnPrint2;
        SAPbouiCOM.Item itxtInterviewCall, itxtInterviewDt, itxtCandidateNo, itxtCandidateName;
        SAPbouiCOM.Item itxtDocNum, itxtDocStatus, itxtCreatedOn, itxtClosedOn;
        SAPbouiCOM.Item itxtOverallScore, itxtKnowledgeSkill, itxtTotalScore;
        SAPbouiCOM.Item itxtBudgetedSalary, itxtRecommendedSalary, itxtApprovedSalary, itxtProbationValue;
        SAPbouiCOM.Item icbAssestmentArea, icbPanelist, icbResult, icbProbationUnit, icbContractType, icbDocStatus;
        SAPbouiCOM.Item ichkSelected, ichkCandidateAccepted, ichkEmployeeAccepted;
        SAPbouiCOM.Item imtAssestment, imtPanelist, imtScoreBoard, imtElements;
        //private Hashtable CodeIndex = new Hashtable();
        private Int32 CurrentRecord = 0, TotalRecords = 0;
        private Boolean DocLoad = false;
        IEnumerable<TrnsInterviewEAS> oDocCollections = null;

        #endregion

        #region "B1 Form Events"
        
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btmain":
                    CheckMainButtonState();
                    break;
                case "btemp":
                    CreateEmployee();
                    break;
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "txintcall":
                    SetInterviewCallData();
                    //GetPanelistFromInterviewCall(aPanelist);
                    //FillComboColumnPanelist(aPanelist);
                    break;
                case "mtassest":
                    if (pVal.ColUID == "panelist")
                    {
                        mtAssestment.FlushToDataSource();
                        AddEmptyRowAssestment();
                    }
                    break;
                case "mtpanel":
                    if (pVal.ColUID == "remark")
                    {
                        AddEmptyRowPanelist();
                    }
                    break;
            }
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "cbasset":

                    FillComboCriteriaSelectAssestment(cbAssestmentArea.Selected.Value, aCriteria);
                    break;
                case "cbpanelist":
                    FillPanelistDataOnSelection();
                    break;
                case "mtassest":
                    if (pVal.ColUID == "criteria")
                    {
                        Int32 CriteriaField;
                        Int32 RowNumber;
                        RowNumber = pVal.Row;
                        SAPbouiCOM.ComboBox One = mtAssestment.GetCellSpecific(aCriteria.DataBind.Alias, RowNumber);
                        if (!String.IsNullOrEmpty(One.Value.Trim()))
                        {
                            CriteriaField = Convert.ToInt32(One.Value.Trim());
                            SetAssestmentLine(RowNumber - 1, CriteriaField);
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
            }
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            //GetNext();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            //GetPrevious();
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void fillFields()
        {
            base.fillFields();
            FillFields();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            InitiallizeDocument("Find");
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            try
            {
                String InterviewCallDocNum, InterviewEASDocNum, CandidateNo, DocStatus;
                DateTime ScheduleTime;
                InterviewCallDocNum = txtInterviewCall.Value.Trim();
                InterviewEASDocNum = txtDocNum.Value.Trim();
                CandidateNo = txtCandidateNo.Value.Trim();
                DocStatus = cbDocStatus.Selected.Value;
                
                if (String.IsNullOrEmpty(txtInterviewDt.Value))
                {
                    SearchKeyVal.Add("dtFrom", "01/01/2005");
                    SearchKeyVal.Add("dtTo", "12/31/2030");
                }
                else
                {
                    ScheduleTime = Convert.ToDateTime(txtInterviewDt.Value.Trim());
                    SearchKeyVal.Add("dtFrom", ScheduleTime);
                    SearchKeyVal.Add("dtTo", ScheduleTime);
                }
                if (!String.IsNullOrEmpty(InterviewEASDocNum))
                {
                    SearchKeyVal.Add("IntEAS", InterviewEASDocNum);
                }
                else
                {
                    SearchKeyVal.Add("IntEAS", "%");
                }
                if (!String.IsNullOrEmpty(CandidateNo))
                {
                    SearchKeyVal.Add("CanNo", CandidateNo);
                }
                else
                {
                    SearchKeyVal.Add("CanNo", "%");
                }
                if (!String.IsNullOrEmpty(InterviewCallDocNum))
                {
                    SearchKeyVal.Add("DocNum", InterviewCallDocNum);
                }
                else
                {
                    SearchKeyVal.Add("DocNum", "%");
                }
                if (!String.IsNullOrEmpty(DocStatus) && DocStatus != "0")
                {
                    SearchKeyVal.Add("DocStatus", DocStatus);
                }
                else
                {
                    SearchKeyVal.Add("DocStatus", "%");
                }
                
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {

                //Buttons
                btnMain = oForm.Items.Item("btmain").Specific;
                ibtnMain = oForm.Items.Item("btmain");
                btnCancel = oForm.Items.Item("2").Specific;
                ibtnCancel = oForm.Items.Item("2");
                btnPrint = oForm.Items.Item("btprint").Specific;
                ibtnPrint = oForm.Items.Item("btprint");
                btnPrint2 = oForm.Items.Item("btprint2").Specific;
                ibtnPrint2 = oForm.Items.Item("btprint2");
                btnCreateEmployee = oForm.Items.Item("btemp").Specific;
                ibtnCreateEmployee = oForm.Items.Item("btemp");

                //Header Section

                txtInterviewCall = oForm.Items.Item("txintcall").Specific;
                itxtInterviewCall = oForm.Items.Item("txintcall");
                oForm.DataSources.UserDataSources.Add("txintcall", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtInterviewCall.DataBind.SetBound(true, "", "txintcall");

                String Query;

                Query = @"SELECT A.DocNum, B.CandidateNo, B.FirstName, B.LastName
                          FROM " + Program.objHrmsUI.HRMSDbName + @".dbo.TrnsInterviewCall AS A INNER JOIN " + Program.objHrmsUI.HRMSDbName + @".dbo.MstCandidate AS B ON A.CandidateID = B.ID";
                Program.objHrmsUI.addFms("frm_IntEvlSelection", "txintcall", "-1", Query);

                txtInterviewDt = oForm.Items.Item("txintdt").Specific;
                itxtInterviewDt = oForm.Items.Item("txintdt");
                oForm.DataSources.UserDataSources.Add("txintdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtInterviewDt.DataBind.SetBound(true, "", "txintdt");

                txtCandidateNo = oForm.Items.Item("txcanno").Specific;
                itxtCandidateNo = oForm.Items.Item("txcanno");
                oForm.DataSources.UserDataSources.Add("txcanno", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtCandidateNo.DataBind.SetBound(true, "", "txcanno");

                txtCandidateName = oForm.Items.Item("txcanname").Specific;
                itxtCandidateName = oForm.Items.Item("txcanname");
                oForm.DataSources.UserDataSources.Add("txcanname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtCandidateName.DataBind.SetBound(true, "", "txcanname");

                txtDocNum = oForm.Items.Item("txdocno").Specific;
                itxtDocNum = oForm.Items.Item("txdocno");
                oForm.DataSources.UserDataSources.Add("txdocno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDocNum.DataBind.SetBound(true, "", "txdocno");
                

                cbDocStatus = oForm.Items.Item("cbstatus").Specific;
                icbDocStatus = oForm.Items.Item("cbstatus");
                oForm.DataSources.UserDataSources.Add("cbstatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDocStatus.DataBind.SetBound(true, "", "cbstatus");
                FillLovList(cbDocStatus, "DocStatus");

                txtCreatedOn = oForm.Items.Item("txcrton").Specific;
                itxtCreatedOn = oForm.Items.Item("txcrton");
                oForm.DataSources.UserDataSources.Add("txcrton", SAPbouiCOM.BoDataType.dt_DATE);
                txtCreatedOn.DataBind.SetBound(true, "", "txcrton");
                itxtCreatedOn.Enabled = false;

                txtClosedOn = oForm.Items.Item("txcldon").Specific;
                itxtClosedOn = oForm.Items.Item("txcldon");
                oForm.DataSources.UserDataSources.Add("txcldon", SAPbouiCOM.BoDataType.dt_DATE);
                txtClosedOn.DataBind.SetBound(true, "", "txcldon");
                itxtClosedOn.Enabled = false;

                // Assesstment Tab

                cbAssestmentArea = oForm.Items.Item("cbasset").Specific;
                icbAssestmentArea = oForm.Items.Item("cbasset");
                oForm.DataSources.UserDataSources.Add("cbasset", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbAssestmentArea.DataBind.SetBound(true, "", "cbasset");
                FillComboAssestment(cbAssestmentArea);

                mtAssestment = oForm.Items.Item("mtassest").Specific;
                
                imtAssestment = oForm.Items.Item("mtassest");
                dtAssestment = oForm.DataSources.DataTables.Item("dtassest");
                aIsNew = mtAssestment.Columns.Item("isnew");
                aIsNew.Visible = false;
                aId = mtAssestment.Columns.Item("id");
                aId.Visible = false;
                aCriteria = mtAssestment.Columns.Item("criteria");
                aCriteria.TitleObject.Sortable = false;
                aDescription = mtAssestment.Columns.Item("desc");
                aDescription.TitleObject.Sortable = false;
                aMarks = mtAssestment.Columns.Item("mark");
                aMarks.TitleObject.Sortable = false;
                aMarksObtain = mtAssestment.Columns.Item("obtain");
                aMarksObtain.TitleObject.Sortable = false;
                aPanelist = mtAssestment.Columns.Item("panelist");
                aPanelist.TitleObject.Sortable = false;
                aRequiredScore = mtAssestment.Columns.Item("reqscore");
                aRequiredScore.TitleObject.Sortable = false;
                aRemarks = mtAssestment.Columns.Item("remark");
                aRemarks.TitleObject.Sortable = false;
                
                
                // Panelist Tab

                cbPanelist = oForm.Items.Item("cbpanelist").Specific;
                icbPanelist = oForm.Items.Item("cbpanelist");
                oForm.DataSources.UserDataSources.Add("cbpanelist", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbPanelist.DataBind.SetBound(true, "", "cbpanelist");
                //TODO: fill the combo{Load when document opens}
                
                mtPanelist = oForm.Items.Item("mtpanel").Specific;
                imtPanelist = oForm.Items.Item("mtpanel");
                dtPanelist = oForm.DataSources.DataTables.Item("dtpanel");
                pId = mtPanelist.Columns.Item("id");
                pId.Visible = false;
                pIsNew = mtPanelist.Columns.Item("isnew");
                pIsNew.Visible = false;
                pAssestmentCode = mtPanelist.Columns.Item("assest");
                pDescription = mtPanelist.Columns.Item("desc");
                pMarks = mtPanelist.Columns.Item("marks");
                pMarksObtain = mtPanelist.Columns.Item("obtain");
                pRemarks = mtPanelist.Columns.Item("remark");
                

                // ScroreBoard Tab

                txtOverallScore = oForm.Items.Item("txscover").Specific;
                itxtOverallScore = oForm.Items.Item("txscover");
                oForm.DataSources.UserDataSources.Add("txscover", SAPbouiCOM.BoDataType.dt_SUM);
                txtOverallScore.DataBind.SetBound(true, "", "txscover");

                txtKnowledgeSkill = oForm.Items.Item("txknow").Specific;
                itxtKnowledgeSkill = oForm.Items.Item("txknow");
                oForm.DataSources.UserDataSources.Add("txknow", SAPbouiCOM.BoDataType.dt_SUM);
                txtKnowledgeSkill.DataBind.SetBound(true, "", "txknow");

                txtTotalScore = oForm.Items.Item("txtscore").Specific;
                itxtTotalScore = oForm.Items.Item("txtscore");
                oForm.DataSources.UserDataSources.Add("txtscore", SAPbouiCOM.BoDataType.dt_SUM);
                txtTotalScore.DataBind.SetBound(true, "", "txtscore");

                cbResult = oForm.Items.Item("cbresult").Specific;
                icbResult = oForm.Items.Item("cbresult");
                oForm.DataSources.UserDataSources.Add("cbresult", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbResult.DataBind.SetBound(true, "", "cbresult");
                FillLovList(cbResult, "IntSel");

                chkSelected = oForm.Items.Item("chkselect").Specific;
                ichkSelected = oForm.Items.Item("chkselect");
                oForm.DataSources.UserDataSources.Add("chkselect", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                chkSelected.DataBind.SetBound(true, "", "chkselect");

                mtScoreBoard = oForm.Items.Item("mtsb").Specific;
                imtScoreBoard = oForm.Items.Item("mtsb");
                dtScoreBoard = oForm.DataSources.DataTables.Item("dtscb");
                sId = mtScoreBoard.Columns.Item("id");
                sId.Visible = false;
                sIsNew = mtScoreBoard.Columns.Item("isnew");
                sIsNew.Visible = false;
                sAssesstmentArea = mtScoreBoard.Columns.Item("assest");
                sAssesstmentArea.TitleObject.Sortable = false;
                sAverageMarks = mtScoreBoard.Columns.Item("avg");
                sAverageMarks.TitleObject.Sortable = false;
                sRemarks = mtScoreBoard.Columns.Item("remark");
                sRemarks.TitleObject.Sortable = false;

                //Compensation & Benefits Tab

                txtBudgetedSalary = oForm.Items.Item("txbudslry").Specific;
                itxtBudgetedSalary = oForm.Items.Item("txbudslry");
                oForm.DataSources.UserDataSources.Add("txbudslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtBudgetedSalary.DataBind.SetBound(true, "", "txbudslry");

                txtRecommendedSalary = oForm.Items.Item("txrecslry").Specific;
                itxtRecommendedSalary = oForm.Items.Item("txrecslry");
                oForm.DataSources.UserDataSources.Add("txrecslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtRecommendedSalary.DataBind.SetBound(true, "", "txrecslry");

                txtApprovedSalary = oForm.Items.Item("txappslry").Specific;
                itxtApprovedSalary = oForm.Items.Item("txappslry");
                oForm.DataSources.UserDataSources.Add("txappslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtApprovedSalary.DataBind.SetBound(true, "", "txappslry");

                cbContractType = oForm.Items.Item("cbcontype").Specific;
                icbContractType = oForm.Items.Item("cbcontype");
                oForm.DataSources.UserDataSources.Add("cbcontype", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbContractType.DataBind.SetBound(true, "", "cbcontype");
                FillLovList(cbContractType, "ContractType");

                txtProbationValue = oForm.Items.Item("txproval").Specific;
                itxtProbationValue = oForm.Items.Item("txproval");
                oForm.DataSources.UserDataSources.Add("txproval", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtProbationValue.DataBind.SetBound(true, "", "txproval");
                txtProbationValue.Value = "1";

                cbProbationUnit = oForm.Items.Item("cbprounit").Specific;
                icbProbationUnit = oForm.Items.Item("cbprounit");
                oForm.DataSources.UserDataSources.Add("cbprounit", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbProbationUnit.DataBind.SetBound(true, "", "cbprounit");
                FillLovList(cbProbationUnit, "EXPUnit");

                chkCandidateAccepted = oForm.Items.Item("chkcan").Specific;
                ichkCandidateAccepted = oForm.Items.Item("chkcan");
                oForm.DataSources.UserDataSources.Add("chkcan", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                chkCandidateAccepted.DataBind.SetBound(true, "", "chkcan");

                chkEmployeeAccepted = oForm.Items.Item("chkemp").Specific;
                ichkEmployeeAccepted = oForm.Items.Item("chkemp");
                oForm.DataSources.UserDataSources.Add("chkemp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                chkEmployeeAccepted.DataBind.SetBound(true, "", "chkemp");

                mtElements = oForm.Items.Item("mtelement").Specific;
                imtElements = oForm.Items.Item("mtelement");
                dtElements = oForm.DataSources.DataTables.Item("dtele");
                eId = mtElements.Columns.Item("id");
                eId.Visible = false;
                eIsNew = mtElements.Columns.Item("isnew");
                eIsNew.Visible = false;
                eElementName = mtElements.Columns.Item("eleid");
                eElementName.TitleObject.Sortable = false;
                eElementType = mtElements.Columns.Item("eletype");
                eElementType.TitleObject.Sortable = false;


                //LastSet
                GetData();
                oForm.PaneLevel = 1;
                FormStatus();
                InitiallizeDocument("New");

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument(String pCase)
        {
            try
            {
                switch (pCase)
                {
                    case "New":
                        btnMain.Caption = "Add";
                        itxtInterviewCall.Enabled = true;
                        itxtInterviewCall.Click();
                        itxtInterviewDt.Enabled = false;
                        itxtCandidateNo.Enabled = false;
                        itxtCandidateName.Enabled = false;
                        itxtDocNum.Enabled = false;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        //Assestment Tab
                        icbAssestmentArea.Enabled = true;
                        imtAssestment.Enabled = true;
                        //Overall Assestment 
                        icbPanelist.Enabled = true;
                        imtPanelist.Enabled = true;
                        //ScoreBoard
                        itxtOverallScore.Enabled = true;
                        itxtKnowledgeSkill.Enabled = true;
                        itxtTotalScore.Enabled = true;
                        icbResult.Enabled = true;
                        ichkSelected.Enabled = true;
                        ibtnPrint.Enabled = false;
                        //Compansation
                        itxtBudgetedSalary.Enabled = true;
                        itxtRecommendedSalary.Enabled = true;
                        itxtApprovedSalary.Enabled = true;
                        icbContractType.Enabled = true;
                        itxtProbationValue.Enabled = true;
                        icbProbationUnit.Enabled = true;
                        ichkCandidateAccepted.Enabled = true;
                        ichkEmployeeAccepted.Enabled = true;
                        ibtnCreateEmployee.Enabled = true;
                        ibtnPrint2.Enabled = true;

                        //Now Values Clear

                        txtDocNum.Value = Convert.ToString(ds.GetDocumentNumber(-1, 21));
                        txtInterviewCall.Value = "";
                        txtInterviewDt.Value = "";
                        txtCandidateNo.Value = "";
                        txtCandidateName.Value = "";
                        cbDocStatus.Select(1, SAPbouiCOM.BoSearchKey.psk_Index);
                        txtCreatedOn.Value = "";
                        txtClosedOn.Value = "";
                        //Assestment Tab
                        //cbAssestmentArea.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        mtAssestment.Clear();
                        dtAssestment.Rows.Clear();
                        AddEmptyRowAssestment();

                        //Overall Assestment 
                        //cbPanelist.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        mtPanelist.Clear();
                        dtPanelist.Rows.Clear();
                        AddEmptyRowPanelist();

                        //ScoreBoard
                        txtOverallScore.Value = "";
                        txtKnowledgeSkill.Value = "";
                        txtTotalScore.Value = "";
                        cbResult.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        chkSelected.Checked = false;
                        
                        //Compansation
                        txtBudgetedSalary.Value = "";
                        txtRecommendedSalary.Value = "";
                        txtApprovedSalary.Value = "";
                        cbContractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        txtProbationValue.Value = "";
                        cbProbationUnit.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        chkCandidateAccepted.Checked = false;
                        chkEmployeeAccepted.Checked = false;

                        break;
                    case "LoadDoc":
                        btnMain.Caption = "Update";
                        break;
                    case "LV0001":
                        btnMain.Caption = "Update";
                        itxtInterviewCall.Enabled = true;
                        itxtInterviewCall.Click();
                        itxtInterviewDt.Enabled = false;
                        itxtCandidateNo.Enabled = false;
                        itxtCandidateName.Enabled = false;
                        itxtDocNum.Enabled = false;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        //Assestment Tab
                        icbAssestmentArea.Enabled = true;
                        imtAssestment.Enabled = true;
                        //Overall Assestment 
                        icbPanelist.Enabled = true;
                        imtPanelist.Enabled = true;
                        //ScoreBoard
                        itxtOverallScore.Enabled = true;
                        itxtKnowledgeSkill.Enabled = true;
                        itxtTotalScore.Enabled = true;
                        icbResult.Enabled = true;
                        ichkSelected.Enabled = true;
                        ibtnPrint.Enabled = false;
                        //Compansation
                        itxtBudgetedSalary.Enabled = true;
                        itxtRecommendedSalary.Enabled = true;
                        itxtApprovedSalary.Enabled = true;
                        icbContractType.Enabled = true;
                        itxtProbationValue.Enabled = true;
                        icbProbationUnit.Enabled = true;
                        ichkCandidateAccepted.Enabled = true;
                        ichkEmployeeAccepted.Enabled = true;
                        ibtnCreateEmployee.Enabled = true;
                        ibtnPrint2.Enabled = true;
                        break;
                    case "LV0002":
                        btnMain.Caption = "Update";
                        itxtInterviewCall.Enabled = false;
                        //itxtInterviewCall.Click();
                        itxtInterviewDt.Enabled = false;
                        itxtCandidateNo.Enabled = false;
                        itxtCandidateName.Enabled = false;
                        itxtDocNum.Enabled = false;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        //Assestment Tab
                        icbAssestmentArea.Enabled = true;
                        imtAssestment.Enabled = false;
                        //Overall Assestment 
                        icbPanelist.Enabled = true;
                        imtPanelist.Enabled = true;
                        //ScoreBoard
                        itxtOverallScore.Enabled = true;
                        itxtKnowledgeSkill.Enabled = true;
                        itxtTotalScore.Enabled = true;
                        icbResult.Enabled = true;
                        ichkSelected.Enabled = true;
                        ibtnPrint.Enabled = false;
                        //Compansation
                        itxtBudgetedSalary.Enabled = true;
                        itxtRecommendedSalary.Enabled = true;
                        itxtApprovedSalary.Enabled = true;
                        icbContractType.Enabled = true;
                        itxtProbationValue.Enabled = true;
                        icbProbationUnit.Enabled = true;
                        ichkCandidateAccepted.Enabled = true;
                        ichkEmployeeAccepted.Enabled = true;
                        ibtnCreateEmployee.Enabled = true;
                        ibtnPrint2.Enabled = true;
                        break;
                    case "LV0003":
                        break;
                    case "Find":
                        btnMain.Caption = "Find";
                        itxtInterviewCall.Enabled = true;
                        itxtInterviewCall.Click();
                        itxtInterviewDt.Enabled = true;
                        itxtCandidateNo.Enabled = true;
                        itxtCandidateName.Enabled = false;
                        itxtDocNum.Enabled = true;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        //Assestment Tab
                        icbAssestmentArea.Enabled = true;
                        imtAssestment.Enabled = true;
                        //Overall Assestment 
                        icbPanelist.Enabled = true;
                        imtPanelist.Enabled = true;
                        //ScoreBoard
                        itxtOverallScore.Enabled = true;
                        itxtKnowledgeSkill.Enabled = true;
                        itxtTotalScore.Enabled = true;
                        icbResult.Enabled = true;
                        ichkSelected.Enabled = true;
                        ibtnPrint.Enabled = false;
                        //Compansation
                        itxtBudgetedSalary.Enabled = true;
                        itxtRecommendedSalary.Enabled = true;
                        itxtApprovedSalary.Enabled = true;
                        icbContractType.Enabled = true;
                        itxtProbationValue.Enabled = true;
                        icbProbationUnit.Enabled = true;
                        ichkCandidateAccepted.Enabled = true;
                        ichkEmployeeAccepted.Enabled = true;
                        ibtnCreateEmployee.Enabled = true;
                        ibtnPrint2.Enabled = true;

                        //Now Values Clear

                        txtDocNum.Value = "";
                        txtInterviewCall.Value = "";
                        txtInterviewDt.Value = "";
                        txtCandidateNo.Value = "";
                        txtCandidateName.Value = "";
                        cbDocStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        txtCreatedOn.Value = "";
                        txtClosedOn.Value = "";
                        //Assestment Tab
                        //cbAssestmentArea.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        mtAssestment.Clear();
                        dtAssestment.Rows.Clear();
                        AddEmptyRowAssestment();

                        //Overall Assestment 
                        //cbPanelist.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        mtPanelist.Clear();
                        dtPanelist.Rows.Clear();
                        AddEmptyRowPanelist();

                        //ScoreBoard
                        txtOverallScore.Value = "";
                        txtKnowledgeSkill.Value = "";
                        txtTotalScore.Value = "";
                        cbResult.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        chkSelected.Checked = false;
                        
                        //Compansation
                        txtBudgetedSalary.Value = "";
                        txtRecommendedSalary.Value = "";
                        txtApprovedSalary.Value = "";
                        cbContractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        txtProbationValue.Value = "";
                        cbProbationUnit.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        chkCandidateAccepted.Checked = false;
                        chkEmployeeAccepted.Checked = false;
                        break;

                }
            }
            catch (Exception)
            {
            }
        }

        private void FormStatus()
        {
            try
            {
            }
            catch
            {
            }
        }

        private void CheckMainButtonState()
        {
            switch (btnMain.Caption)
            {
                case "Add":
                    if (ValidateForm())
                    {
                        if (AddDocument())
                        {
                            InitiallizeDocument("New");
                        }
                    }
                    break;
                case "Update":
                    if (ValidateForm())
                    {
                        if (UpdateDocument())
                        {
                            InitiallizeDocument("New");
                        }
                    }
                    break;
                case "OK":
                    oForm.Close();
                    break;
                case "Find":
                    doFind();
                    break;
            }
        }

        private Boolean ValidateForm()
        {
            Boolean retValue = true;
            
            try 
            {
                //Check Probation Value
                if (string.IsNullOrEmpty(txtProbationValue.Value.Trim()))
                {
                    txtProbationValue.Value = "0";
                }
            }
            catch (Exception)
            {
                retValue = false;
            }

            return retValue;
        }

        private Boolean AddDocument()
        {
            Boolean retValue = true;
            try
            {
                //Int32 InterviewCall;
                //TrnsInterviewEAS oDoc = new TrnsInterviewEAS();

                //oDoc.DocNum = Convert.ToInt32(txtDocNum.Value.Trim());
                //oDoc.DocType = 21;
                //oDoc.Series = -1;
                //oDoc.DocStatus = cbDocStatus.Value.Trim();
                //InterviewCall = Convert.ToInt32(txtInterviewCall.Value.Trim());
                //TrnsInterviewCall One = (from a in dbHrPayroll.TrnsInterviewCall where a.DocNum == InterviewCall select a).FirstOrDefault();
                //if ( One == null)
                //{
                //    oApplication.StatusBar.SetText("Select Interview Document", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    retValue = false;
                //    return retValue;
                //}
                //oDoc.InterviewID = One.ID;
                //oDoc.AssetmentScore = 0.0M;
                //oDoc.KnowledgeSkillScore = 0.0M;
                //oDoc.Result = "";
                //oDoc.FlgSelected = chkSelected.Checked;
                //oDoc.BudgetSalary = Convert.ToDecimal(txtBudgetedSalary.Value.Trim());
                //oDoc.ApprovedSalary = Convert.ToDecimal(txtApprovedSalary.Value.Trim());
                //oDoc.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
                //oDoc.ContractType = cbContractType.Value.Trim();
                //oDoc.ContractTypeLOV = "ContractType";
                //oDoc.ProbationValue = Convert.ToByte(txtProbationValue.Value.Trim());
                //oDoc.ProbationUnit = cbProbationUnit.Value.Trim();
                //oDoc.FlgAccepted = chkCandidateAccepted.Checked;
                //oDoc.FlgContract = chkEmployeeAccepted.Checked;

                ////Assesment
                //mtAssestment.FlushToDataSource();
                //String Criteria, Description, Marks, RequiredScore, Panelist, Obtain, Remarks;
                //Criteria = dtAssestment.GetValue(aCriteria.DataBind.Alias, 0);

                //if (dtAssestment.Rows.Count > 0 && !String.IsNullOrEmpty(Criteria))
                //{
                //    for (Int32 i = 0; i < dtAssestment.Rows.Count; i++)
                //    {
                //        TrnsInterviewEASAssetment OneLine = new TrnsInterviewEASAssetment();
                //        Criteria = Convert.ToString(dtAssestment.GetValue(aCriteria.DataBind.Alias, i));
                //        Description = Convert.ToString(dtAssestment.GetValue(aDescription.DataBind.Alias, i));
                //        Marks = Convert.ToString(dtAssestment.GetValue(aMarks.DataBind.Alias, i));
                //        RequiredScore = Convert.ToString(dtAssestment.GetValue(aRequiredScore.DataBind.Alias, i));
                //        Panelist = Convert.ToString(dtAssestment.GetValue(aPanelist.DataBind.Alias, i));
                //        Obtain = Convert.ToString(dtAssestment.GetValue(aMarksObtain.DataBind.Alias, i));
                //        Remarks = Convert.ToString(dtAssestment.GetValue(aRemarks.DataBind.Alias, i));
                //        if (!String.IsNullOrEmpty(Criteria))
                //        {
                //            OneLine.CriteriaId = Convert.ToInt32(Criteria);
                //            OneLine.Description = Description;
                //            OneLine.Marks = Convert.ToDecimal(Marks);
                //            OneLine.Required = Convert.ToDecimal(RequiredScore);
                //            OneLine.PanelistId = Convert.ToInt32(Panelist);
                //            OneLine.Obtain = Convert.ToDecimal(Obtain);
                //            OneLine.Remarks = Remarks;
                //            oDoc.TrnsInterviewEASAssetment.Add(OneLine);
                //        }
                //    }
                //}

                //oDoc.CreateDt = DateTime.Now;
                //oDoc.UpdateDt = DateTime.Now;
                //oDoc.UserId = oCompany.UserName;
                //oDoc.UpdatedBy = oCompany.UserName;

                //dbHrPayroll.TrnsInterviewEAS.InsertOnSubmit(oDoc);
                //dbHrPayroll.SubmitChanges();
                //oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception AddDocument Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private Boolean UpdateDocument()
        {
            Boolean retValue = true;
            string DocumentStatus = "";
            DocumentStatus = cbDocStatus.Value.Trim();
            try
            {

                TrnsInterviewEAS oDoc = oDocCollections.ElementAt<TrnsInterviewEAS>(currentRecord);
                if (oDoc == null)
                {
                    retValue = false;
                    return retValue;
                }

                if (oDoc.DocStatus == "LV0001" && DocumentStatus == "LV0002")
                {
                    oDoc.DocStatus = "LV0002";
                }
                if (oDoc.DocStatus == "LV0002" && DocumentStatus == "LV0003")
                {
                    oDoc.DocStatus = "LV0003";
                }
                //Assestment Tab
                if (oDoc.DocStatus == "LV0001")
                {
                    mtAssestment.FlushToDataSource();
                    Int32 RowCount = 0;
                    RowCount = dtAssestment.Rows.Count;
                    for (Int32 i = 0; i < RowCount; i++)
                    {
                        String vIsNew, vId, vCriteria, vDescription, vMarks, vMarksObtain, vPanelist, vRemarks, vRequiredMarks;
                        vIsNew = dtAssestment.GetValue(aIsNew.DataBind.Alias, i);
                        vId = dtAssestment.GetValue(aId.DataBind.Alias, i);
                        vCriteria = dtAssestment.GetValue(aCriteria.DataBind.Alias, i);
                        vDescription = dtAssestment.GetValue(aDescription.DataBind.Alias, i);
                        vMarks = Convert.ToString(dtAssestment.GetValue(aMarks.DataBind.Alias, i));
                        vRequiredMarks = Convert.ToString(dtAssestment.GetValue(aRequiredScore.DataBind.Alias, i));
                        vMarksObtain = Convert.ToString(dtAssestment.GetValue(aMarksObtain.DataBind.Alias, i));
                        vPanelist = Convert.ToString(dtAssestment.GetValue(aPanelist.DataBind.Alias, i));
                        vRemarks = dtAssestment.GetValue(aRemarks.DataBind.Alias, i);
                        if (!String.IsNullOrEmpty(vCriteria))
                        {
                            if (vIsNew == "Y")
                            {
                                TrnsInterviewEASAssetment OneLine = new TrnsInterviewEASAssetment();
                                OneLine.CriteriaId = Convert.ToInt32(vCriteria);
                                OneLine.Description = vDescription;
                                OneLine.Marks = Convert.ToDecimal(vMarks);
                                OneLine.Required = Convert.ToDecimal(vRequiredMarks);
                                OneLine.PanelistId = Convert.ToInt32(vPanelist);
                                OneLine.Obtain = Convert.ToDecimal(vMarksObtain);
                                OneLine.Remarks = vRemarks;
                                oDoc.TrnsInterviewEASAssetment.Add(OneLine);
                            }
                            else
                            {
                                TrnsInterviewEASAssetment OneLine = (from a in dbHrPayroll.TrnsInterviewEASAssetment where a.ID.ToString() == vId select a).FirstOrDefault();
                                OneLine.CriteriaId = Convert.ToInt32(vCriteria);
                                //OneLine.Description = vDescription;
                                OneLine.Marks = Convert.ToDecimal(vMarks);
                                OneLine.Required = Convert.ToDecimal(vRequiredMarks);
                                OneLine.PanelistId = Convert.ToInt32(vPanelist);
                                //OneLine.Obtain = Convert.ToDecimal(vMarksObtain);
                                //OneLine.Remarks = vRemarks;
                            }
                        }
                    }
                }
                //Panelist Tab
                if (oDoc.DocStatus == "LV0002")
                {
                    mtPanelist.FlushToDataSource();
                    Int32 RowCount = 0;
                    RowCount = dtPanelist.Rows.Count;
                    for (Int32 i = 0; i < RowCount; i++)
                    {
                        String vIsNew, vId, vCriteria, vDescription, vMarks, vMarksObtain, vRemarks;
                        vIsNew = dtPanelist.GetValue(pIsNew.DataBind.Alias, i);
                        vId = dtPanelist.GetValue(pId.DataBind.Alias, i);
                        vCriteria = Convert.ToString(dtPanelist.GetValue(pAssestmentCode.DataBind.Alias, i));
                        vDescription = dtPanelist.GetValue(pDescription.DataBind.Alias, i);
                        vMarks = Convert.ToString(dtPanelist.GetValue(pMarks.DataBind.Alias, i));
                        vMarksObtain = Convert.ToString(dtPanelist.GetValue(pMarksObtain.DataBind.Alias, i));
                        vRemarks = dtPanelist.GetValue(pRemarks.DataBind.Alias, i);
                        if (!String.IsNullOrEmpty(vCriteria))
                        {
                            TrnsInterviewEASAssetment OneLine = (from a in dbHrPayroll.TrnsInterviewEASAssetment where a.ID.ToString() == vId select a).FirstOrDefault();
                            //OneLine.CriteriaId = Convert.ToInt32(vCriteria);
                            //OneLine.Description = vDescription;
                            //OneLine.Marks = Convert.ToDecimal(vMarks);
                            //OneLine.Required = Convert.ToDecimal(vRequiredMarks);
                            //OneLine.PanelistId = Convert.ToInt32(vPanelist);
                            OneLine.Obtain = Convert.ToDecimal(vMarksObtain);
                            OneLine.Remarks = vRemarks;
                        }
                    }
                }

                //Scoreboard Tab

                Decimal overallscore, knowledgeskillscore;
                overallscore = Convert.ToDecimal(txtOverallScore.Value.Trim());
                oDoc.AssetmentScore = overallscore;
                knowledgeskillscore = Convert.ToDecimal(txtKnowledgeSkill.Value.Trim());
                oDoc.KnowledgeSkillScore = knowledgeskillscore;
                //Total Score Field Needed
                oDoc.Result = cbResult.Value.Trim();
                oDoc.FlgSelected = chkSelected.Checked;

                Decimal AssestmentScore = 0.0M;

                foreach (TrnsInterviewEASAssetment One in oDoc.TrnsInterviewEASAssetment)
                {
                    AssestmentScore += Convert.ToDecimal(One.Obtain);
                }
                txtOverallScore.Value = AssestmentScore.ToString();
                oDoc.AssetmentScore = AssestmentScore;
                txtTotalScore.Value = Convert.ToString(overallscore + knowledgeskillscore);

                //Compensation & Beneifit
                oDoc.BudgetSalary = Convert.ToDecimal(txtBudgetedSalary.Value.Trim());
                oDoc.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
                oDoc.ApprovedSalary = Convert.ToDecimal(txtApprovedSalary.Value.Trim());
                oDoc.ContractType = cbContractType.Value.Trim();
                oDoc.ProbationValue = Convert.ToByte(txtProbationValue.Value.Trim());
                oDoc.ProbationUnit = cbProbationUnit.Value.Trim();
                oDoc.FlgAccepted = chkCandidateAccepted.Checked; 
                oDoc.FlgContract =  chkEmployeeAccepted.Checked; 

                dbHrPayroll.SubmitChanges();
                
                //btnMain.Caption = "Ok";
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

//                //setting the matrix
//                String query = @"SELECT     dbo.MstAssestment.Code, AVG(dbo.TrnsInterviewEASAssetment.Obtain) AS MarksObtain
//                                FROM         dbo.MstAssestment INNER JOIN
//                                                        dbo.MstAssestmentCriteria ON dbo.MstAssestment.ID = dbo.MstAssestmentCriteria.AssestmentID INNER JOIN
//                                                        dbo.TrnsInterviewEASAssetment ON dbo.MstAssestmentCriteria.ID = dbo.TrnsInterviewEASAssetment.CriteriaId
//                                GROUP BY dbo.MstAssestment.Code";
//                DataTable scoreboard = new DataTable();
//                scoreboard = ds.getDataTable(query);
//                dtScoreBoard.Rows.Clear();
//                Int32 rowcount = 0;
//                foreach (DataRow a in scoreboard.Rows)
//                {
//                    String AssestmentCode, MarksObtain;
//                    AssestmentCode = a["Code"].ToString();
//                    MarksObtain = a["MarksObtain"].ToString();
//                    if (AssestmentCode != "")
//                    {
//                        dtScoreBoard.Rows.Add(1);
//                        dtScoreBoard.SetValue(sAssesstmentArea.DataBind.Alias, rowcount, AssestmentCode);
//                        dtScoreBoard.SetValue(sAverageMarks.DataBind.Alias, rowcount, MarksObtain);
//                        dtScoreBoard.SetValue(sRemarks.DataBind.Alias, rowcount, "");
//                    }
//                    rowcount++;
//                }
//                mtScoreBoard.LoadFromDataSource();
                

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception UpdateDocument Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private Boolean SubmitChanges()
        {
            Boolean retValue = false;
            try
            {

//                Int32 DocNumFromField, InterviewCall;
//                TrnsInterviewEAS oDoc = null;
//                DocNumFromField = Convert.ToInt32(txtDocNum.Value.Trim());
//                Int32 checkDoc = (from a in dbHrPayroll.TrnsInterviewEAS where a.DocNum == DocNumFromField select a.DocNum).Count();
//                if (checkDoc == 0)
//                {
//                    oDoc = new TrnsInterviewEAS();
//                    oDoc.DocNum = DocNumFromField;
//                    oDoc.DocType = 21;
//                    oDoc.Series = -1;
//                    InterviewCall = Convert.ToInt32(txtInterviewCall.Value.Trim());
//                    TrnsInterviewCall One = (from a in dbHrPayroll.TrnsInterviewCall where a.DocNum == InterviewCall select a).FirstOrDefault();
//                    if (One == null)
//                    {
//                        oApplication.StatusBar.SetText("Select Interview Document", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
//                        retValue = false;
//                        return retValue;
//                    }
//                    oDoc.InterviewID = One.ID;
//                    oDoc.UserId = oCompany.UserName;
//                    oDoc.CreateDt = DateTime.Now;
//                    dbHrPayroll.TrnsInterviewEAS.InsertOnSubmit(oDoc);
//                }
//                else
//                {
//                    oDoc = oDocCollections.ElementAt<TrnsInterviewEAS>(currentRecord);
//                    oDoc.UpdatedBy = oCompany.UserName;
//                    oDoc.UpdateDt = DateTime.Now;
//                    if (oDoc == null)
//                    {
//                        retValue = false;
//                        return retValue;
//                    }
//                }

//                if (cbDocStatus.Value.Trim() != "-1")
//                {
//                    oDoc.DocStatus = cbDocStatus.Value.Trim();
//                }
//                else
//                {
//                    oDoc.DocStatus = null;
//                }
                
//                //Assestment Tab
//                mtAssestment.FlushToDataSource();
//                Int32 RowCount = 0;
//                RowCount = dtAssestment.Rows.Count;
//                for (Int32 i = 0; i < RowCount; i++)
//                {
//                    String vIsNew, vId, vCriteria, vDescription, vMarks, vMarksObtain, vPanelist, vRemarks, vRequiredMarks;
//                    vIsNew = dtAssestment.GetValue(aIsNew.DataBind.Alias, i);
//                    vId = dtAssestment.GetValue(aId.DataBind.Alias, i);
//                    vCriteria = dtAssestment.GetValue(aCriteria.DataBind.Alias, i);
//                    vDescription = dtAssestment.GetValue(aDescription.DataBind.Alias, i);
//                    vMarks = Convert.ToString(dtAssestment.GetValue(aMarks.DataBind.Alias, i));
//                    vRequiredMarks = Convert.ToString(dtAssestment.GetValue(aRequiredScore.DataBind.Alias, i));
//                    vMarksObtain = Convert.ToString(dtAssestment.GetValue(aMarksObtain.DataBind.Alias, i));
//                    vPanelist = Convert.ToString(dtAssestment.GetValue(aPanelist.DataBind.Alias, i));
//                    vRemarks = dtAssestment.GetValue(aRemarks.DataBind.Alias, i);
//                    if (!String.IsNullOrEmpty(vCriteria))
//                    {
//                        if (vIsNew == "Y")
//                        {
//                            TrnsInterviewEASAssetment OneLine = new TrnsInterviewEASAssetment();
//                            OneLine.CriteriaId = Convert.ToInt32(vCriteria);
//                            OneLine.Description = vDescription;
//                            OneLine.Marks = Convert.ToDecimal(vMarks);
//                            OneLine.Required = Convert.ToDecimal(vRequiredMarks);
//                            OneLine.PanelistId = Convert.ToInt32(vPanelist);
//                            OneLine.Obtain = Convert.ToDecimal(vMarksObtain);
//                            OneLine.Remarks = vRemarks;
//                            oDoc.TrnsInterviewEASAssetment.Add(OneLine);
//                        }
//                        else
//                        {
//                            TrnsInterviewEASAssetment OneLine = (from a in dbHrPayroll.TrnsInterviewEASAssetment where a.ID.ToString() == vId select a).FirstOrDefault();
//                            OneLine.CriteriaId = Convert.ToInt32(vCriteria);
//                            //OneLine.Description = vDescription;
//                            OneLine.Marks = Convert.ToDecimal(vMarks);
//                            OneLine.Required = Convert.ToDecimal(vRequiredMarks);
//                            OneLine.PanelistId = Convert.ToInt32(vPanelist);
//                            //OneLine.Obtain = Convert.ToDecimal(vMarksObtain);
//                            //OneLine.Remarks = vRemarks;
//                        }
//                    }
//                }

//                //Panelist Tab
//                mtPanelist.FlushToDataSource();
//                RowCount = 0;
//                RowCount = dtPanelist.Rows.Count;
//                for (Int32 i = 0; i < RowCount; i++)
//                {
//                    String vIsNew, vId, vCriteria, vDescription, vMarks, vMarksObtain, vRemarks;
//                    vIsNew = dtPanelist.GetValue(pIsNew.DataBind.Alias, i);
//                    vId = dtPanelist.GetValue(pId.DataBind.Alias, i);
//                    vCriteria = Convert.ToString(dtPanelist.GetValue(pAssestmentCode.DataBind.Alias, i));
//                    vDescription = dtPanelist.GetValue(pDescription.DataBind.Alias, i);
//                    vMarks = Convert.ToString(dtPanelist.GetValue(pMarks.DataBind.Alias, i));
//                    vMarksObtain = Convert.ToString(dtPanelist.GetValue(pMarksObtain.DataBind.Alias, i));
//                    vRemarks = dtPanelist.GetValue(pRemarks.DataBind.Alias, i);
//                    if (!String.IsNullOrEmpty(vCriteria))
//                    {
//                        TrnsInterviewEASAssetment OneLine = (from a in dbHrPayroll.TrnsInterviewEASAssetment where a.ID.ToString() == vId select a).FirstOrDefault();
//                        //OneLine.CriteriaId = Convert.ToInt32(vCriteria);
//                        //OneLine.Description = vDescription;
//                        //OneLine.Marks = Convert.ToDecimal(vMarks);
//                        //OneLine.Required = Convert.ToDecimal(vRequiredMarks);
//                        //OneLine.PanelistId = Convert.ToInt32(vPanelist);
//                        OneLine.Obtain = Convert.ToDecimal(vMarksObtain);
//                        OneLine.Remarks = vRemarks;
//                    }
//                }


//                //Scoreboard Tab

//                Decimal overallscore, knowledgeskillscore;
//                overallscore = Convert.ToDecimal(txtOverallScore.Value.Trim());
//                oDoc.AssetmentScore = overallscore;
//                knowledgeskillscore = Convert.ToDecimal(txtKnowledgeSkill.Value.Trim());
//                oDoc.KnowledgeSkillScore = knowledgeskillscore;
//                //Total Score Field Needed
//                oDoc.Result = cbResult.Value.Trim();
//                oDoc.FlgSelected = chkSelected.Checked;

//                Decimal AssestmentScore = 0.0M;

//                foreach (TrnsInterviewEASAssetment One in oDoc.TrnsInterviewEASAssetment)
//                {
//                    AssestmentScore += Convert.ToDecimal(One.Obtain);
//                }
//                txtOverallScore.Value = AssestmentScore.ToString();
//                oDoc.AssetmentScore = AssestmentScore;
//                txtTotalScore.Value = Convert.ToString(overallscore + knowledgeskillscore);

//                //Compensation & Beneifit
//                oDoc.BudgetSalary = Convert.ToDecimal(txtBudgetedSalary.Value.Trim());
//                oDoc.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
//                oDoc.ApprovedSalary = Convert.ToDecimal(txtApprovedSalary.Value.Trim());
//                oDoc.ContractType = cbContractType.Value.Trim();
//                oDoc.ProbationValue = Convert.ToByte(txtProbationValue.Value.Trim());
//                oDoc.ProbationUnit = cbProbationUnit.Value.Trim();
//                oDoc.FlgAccepted = chkCandidateAccepted.Checked;
//                oDoc.FlgContract = chkEmployeeAccepted.Checked;

//                dbHrPayroll.SubmitChanges();
//                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
//                //btnMain.Caption = "Ok";
//                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

//                //setting the matrix
//                String query = @"SELECT     dbo.MstAssestment.Code, AVG(dbo.TrnsInterviewEASAssetment.Obtain) AS MarksObtain
//                                FROM         dbo.MstAssestment INNER JOIN
//                                                        dbo.MstAssestmentCriteria ON dbo.MstAssestment.ID = dbo.MstAssestmentCriteria.AssestmentID INNER JOIN
//                                                        dbo.TrnsInterviewEASAssetment ON dbo.MstAssestmentCriteria.ID = dbo.TrnsInterviewEASAssetment.CriteriaId
//                                GROUP BY dbo.MstAssestment.Code";
//                DataTable scoreboard = new DataTable();
//                scoreboard = ds.getDataTable(query);
//                dtScoreBoard.Rows.Clear();
//                Int32 rowcount = 0;
//                foreach (DataRow a in scoreboard.Rows)
//                {
//                    String AssestmentCode, MarksObtain;
//                    AssestmentCode = a["Code"].ToString();
//                    MarksObtain = a["MarksObtain"].ToString();
//                    if (AssestmentCode != "")
//                    {
//                        dtScoreBoard.Rows.Add(1);
//                        dtScoreBoard.SetValue(sAssesstmentArea.DataBind.Alias, rowcount, AssestmentCode);
//                        dtScoreBoard.SetValue(sAverageMarks.DataBind.Alias, rowcount, MarksObtain);
//                        dtScoreBoard.SetValue(sRemarks.DataBind.Alias, rowcount, "");
//                    }
//                    rowcount++;
//                }
//                mtScoreBoard.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Updating. : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private void FillFields()
        {
            try
            {
//                TrnsInterviewEAS oDoc = oDocCollections.ElementAt<TrnsInterviewEAS>(currentRecord);

//                txtInterviewCall.Value = oDoc.TrnsInterviewCall.DocNum.ToString();
//                txtInterviewDt.Value = Convert.ToDateTime(oDoc.TrnsInterviewCall.ScheduleDate).ToString("yyyyMMdd");
//                txtCandidateNo.Value = oDoc.TrnsInterviewCall.MstCandidate.CandidateNo.ToString();
//                txtCandidateName.Value = oDoc.TrnsInterviewCall.MstCandidate.FirstName;
//                txtDocNum.Value = oDoc.DocNum.ToString();
//                cbDocStatus.Select(String.IsNullOrEmpty(oDoc.DocStatus) ? "LV0001" : oDoc.DocStatus, SAPbouiCOM.BoSearchKey.psk_ByValue);
//                //docstatus field needed
//                //cbDocStatus.Select(oDoc.
//                txtCreatedOn.Value = Convert.ToDateTime(oDoc.CreateDt).ToString("yyyyMMdd");
//                txtClosedOn.Value = Convert.ToDateTime(oDoc.CreateDt).ToString("yyyyMMdd");

//                //Assestment Tab

//                //Panelist Tab

//                //Scoreboard Tab
//                //setting the matrix
//                String query = @"SELECT     dbo.MstAssestment.Code, AVG(dbo.TrnsInterviewEASAssetment.Obtain) AS MarksObtain
//                                FROM         dbo.MstAssestment INNER JOIN
//                                                        dbo.MstAssestmentCriteria ON dbo.MstAssestment.ID = dbo.MstAssestmentCriteria.AssestmentID INNER JOIN
//                                                        dbo.TrnsInterviewEASAssetment ON dbo.MstAssestmentCriteria.ID = dbo.TrnsInterviewEASAssetment.CriteriaId
//                                WHERE dbo.TrnsInterviewEASAssetment.IEASId = '"+ oDoc.ID + @"'
//                                GROUP BY dbo.MstAssestment.Code";
//                DataTable scoreboard = new DataTable();
//                scoreboard = ds.getDataTable(query);
//                if (scoreboard.Rows.Count > 0)
//                {
//                    dtScoreBoard.Rows.Clear();
//                    Int32 rowcount = 0;
//                    foreach (DataRow a in scoreboard.Rows)
//                    {
//                        String AssestmentCode, MarksObtain;
//                        AssestmentCode = a["Code"].ToString();
//                        MarksObtain = a["MarksObtain"].ToString();
//                        if (AssestmentCode != "")
//                        {
//                            dtScoreBoard.Rows.Add(1);
//                            dtScoreBoard.SetValue(sAssesstmentArea.DataBind.Alias, rowcount, AssestmentCode);
//                            dtScoreBoard.SetValue(sAverageMarks.DataBind.Alias, rowcount, MarksObtain);
//                            dtScoreBoard.SetValue(sRemarks.DataBind.Alias, rowcount, "");
//                        }
//                        rowcount++;
//                    }
//                    mtScoreBoard.LoadFromDataSource();
//                }
//                txtOverallScore.Value = oDoc.AssetmentScore.ToString();
//                txtKnowledgeSkill.Value = oDoc.KnowledgeSkillScore.ToString();
//                //Total Score Field Needed
//                if (string.IsNullOrEmpty(oDoc.Result))
//                {
//                    cbResult.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
//                }
//                else
//                {
//                    //cbResult.Select(oDoc.Result.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
//                    Console.WriteLine(oDoc.Result);
//                }
//                chkSelected.Checked = Convert.ToBoolean(oDoc.FlgSelected);

//                //Compensation & Beneifit
//                txtBudgetedSalary.Value = oDoc.BudgetSalary.ToString();
//                txtRecommendedSalary.Value = oDoc.RecommendedSalary.ToString();
//                txtApprovedSalary.Value = oDoc.ApprovedSalary.ToString();
//                cbContractType.Select(oDoc.ContractType.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
//                txtProbationValue.Value = oDoc.ProbationValue.ToString();
//                cbProbationUnit.Select(oDoc.ProbationUnit, SAPbouiCOM.BoSearchKey.psk_ByValue);
//                chkCandidateAccepted.Checked = Convert.ToBoolean( oDoc.FlgAccepted);
//                chkEmployeeAccepted.Checked = Convert.ToBoolean( oDoc.FlgContract);

//                //dtElements.Rows.Clear();
//                //Int32 i = 0;
//                //foreach (TrnsInterviewEASPanelist One in oDoc.TrnsInterviewEASPanelist)
//                //{

//                //}
//                FillComboPanelist(cbPanelist);
//                FillComboColumnPanelist(aPanelist);
//                DocLoad = true;
//                InitiallizeDocument(oDoc.DocStatus);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception FillFields Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oDocCollections = from a in dbHrPayroll.TrnsInterviewEAS select a;
            Int32 i = 0;
            foreach(TrnsInterviewEAS One in oDocCollections)
            {
                CodeIndex.Add(One.ID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void SetInterviewCallData()
        {
            try
            {
                //if (btnMain.Caption == "Add")
                //{
                //    String InterviewID;
                //    if (!String.IsNullOrEmpty(txtInterviewCall.Value))
                //    {
                //        InterviewID = txtInterviewCall.Value.Trim();
                //    }
                //    else
                //    {
                //        return;
                //    }
                //    TrnsInterviewCall One = null;
                //    One = (from a in dbHrPayroll.TrnsInterviewCall where a.DocNum == Convert.ToInt32(InterviewID) select a).FirstOrDefault();

                //    if (One != null)
                //    {
                //        txtInterviewDt.Value = Convert.ToDateTime(One.CreateDt).ToString("yyyyMMdd");
                //        txtCandidateNo.Value = One.DocNum.ToString();
                //        txtCandidateName.Value = One.MstCandidate.FirstName + " " + One.MstCandidate.LastName;
                //    }
                //}
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillLovList(SAPbouiCOM.ComboBox pCombo, String TypeCode)
        {
            try
            {
                IEnumerable<MstLOVE> Collection = from a in dbHrPayroll.MstLOVE where a.Type.Contains(TypeCode) select a;
                pCombo.ValidValues.Add("0", "Select Value");
                foreach (MstLOVE One in Collection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Code), Convert.ToString(One.Value));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillComboAssestment(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstAssestment> oCollection = from a in dbHrPayroll.MstAssestment select a;
                foreach (MstAssestment One in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.ID), Convert.ToString(One.Assestment));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillComboCriteriaSelectAssestment(String AssestmentCode, SAPbouiCOM.Column pCombo)
        {
            try
            {
                if (String.IsNullOrEmpty(AssestmentCode))
                {
                    return;
                }
                if (pCombo.ValidValues.Count > 0)
                {
                    Int32 ComboCount = pCombo.ValidValues.Count;
                    for (Int32 i = ComboCount - 1; i >= 0; i--)
                    {
                        pCombo.ValidValues.Remove(pCombo.ValidValues.Item(i).Value);
                    }

                    IEnumerable<MstAssestmentCriteria> Collection = from a in dbHrPayroll.MstAssestmentCriteria where a.AssestmentID.ToString() == AssestmentCode select a;
                    foreach (MstAssestmentCriteria One in Collection)
                    {
                        pCombo.ValidValues.Add(One.ID.ToString(), One.Criteria);

                    }
                }
                else
                {
                    IEnumerable<MstAssestmentCriteria> Collection = from a in dbHrPayroll.MstAssestmentCriteria where a.AssestmentID.ToString() == AssestmentCode select a;
                    foreach (MstAssestmentCriteria One in Collection)
                    {
                        pCombo.ValidValues.Add(One.ID.ToString(), One.Criteria);
                    }
                }
                FillDataIfDocLoadTrue(DocLoad);

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillDataIfDocLoadTrue(Boolean pflag)
        {
            try
            {
                if (pflag)
                {
                    Int32 DocNum = Convert.ToInt32(txtDocNum.Value.Trim());
                    Int32 AssestmentSelected = Convert.ToInt32(cbAssestmentArea.Value.Trim());
                    IEnumerable<TrnsInterviewEASAssetment> One = from a in dbHrPayroll.TrnsInterviewEASAssetment where a.TrnsInterviewEAS.DocNum == DocNum && a.MstAssestmentCriteria.MstAssestment.ID == AssestmentSelected select a;
                    dtAssestment.Rows.Clear();
                    Int32 i = 0;
                    foreach (TrnsInterviewEASAssetment Oneline in One)
                    {
                        dtAssestment.Rows.Add(1);
                        dtAssestment.SetValue(aIsNew.DataBind.Alias, i, "N");
                        dtAssestment.SetValue(aId.DataBind.Alias, i, Oneline.ID);
                        dtAssestment.SetValue(aCriteria.DataBind.Alias, i, Oneline.CriteriaId.ToString());
                        dtAssestment.SetValue(aDescription.DataBind.Alias, i, Oneline.Description);
                        dtAssestment.SetValue(aMarks.DataBind.Alias, i, Oneline.Marks.ToString());
                        dtAssestment.SetValue(aMarksObtain.DataBind.Alias, i, Oneline.Obtain.ToString());
                        dtAssestment.SetValue(aRequiredScore.DataBind.Alias, i, Oneline.Required.ToString());
                        dtAssestment.SetValue(aPanelist.DataBind.Alias, i , Oneline.PanelistId.ToString());
                        dtAssestment.SetValue(aRemarks.DataBind.Alias, i, Oneline.Remarks);
                        i++;
                    }
                    AddEmptyRowAssestment();
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillComboPanelist(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                //String InterviewCall;
                //if (!String.IsNullOrEmpty(txtInterviewCall.Value))
                //{
                //    InterviewCall = txtInterviewCall.Value;
                //}
                //else
                //{
                //    return;
                //}
                //if (pCombo.ValidValues.Count > 0)
                //{
                //    Int32 ComboCount = pCombo.ValidValues.Count;
                //    for (Int32 i = ComboCount - 1; i >= 0; i--)
                //    {
                //        pCombo.ValidValues.Remove(pCombo.ValidValues.Item(i).Value);
                //    }

                //    IEnumerable<TrnsInterviewCallPanelList> Collection = from a in dbHrPayroll.TrnsInterviewCallPanelList where a.TrnsInterviewCall.DocNum.ToString() == InterviewCall select a;
                //    foreach (TrnsInterviewCallPanelList One in Collection)
                //    {
                //        String Description = One.MstEmployee.EmpID + " : " + One.MstEmployee.FirstName + " " + One.MstEmployee.MiddleName + " " + One.MstEmployee.LastName;
                //        pCombo.ValidValues.Add(One.ID.ToString(), One.MstEmployee.EmpID);
                //    }
                //}
                //else
                //{
                //    IEnumerable<TrnsInterviewCallPanelList> Collection = from a in dbHrPayroll.TrnsInterviewCallPanelList where a.TrnsInterviewCall.DocNum.ToString() == InterviewCall select a;
                //    foreach (TrnsInterviewCallPanelList One in Collection)
                //    {
                //        String Description = One.MstEmployee.EmpID + " : " + One.MstEmployee.FirstName + " " + One.MstEmployee.MiddleName + " " + One.MstEmployee.LastName;
                //        pCombo.ValidValues.Add(One.ID.ToString(), One.MstEmployee.EmpID);
                //    }
                //}
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void AddEmptyRowAssestment()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtAssestment.Rows.Count == 0)
                {
                    dtAssestment.Rows.Add(1);
                    RowValue = dtAssestment.Rows.Count;
                    dtAssestment.SetValue(aIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtAssestment.SetValue(aId.DataBind.Alias, RowValue - 1, "0");
                    dtAssestment.SetValue(aCriteria.DataBind.Alias, RowValue - 1, "");
                    dtAssestment.SetValue(aDescription.DataBind.Alias, RowValue - 1, "");
                    dtAssestment.SetValue(aMarks.DataBind.Alias, RowValue - 1, "0");
                    dtAssestment.SetValue(aMarksObtain.DataBind.Alias, RowValue - 1, "0");
                    dtAssestment.SetValue(aPanelist.DataBind.Alias, RowValue - 1, "");
                    dtAssestment.SetValue(aRequiredScore.DataBind.Alias, RowValue - 1, "0");
                    dtAssestment.SetValue(aRemarks.DataBind.Alias, RowValue - 1, "");
                    mtAssestment.AddRow(1, 0);
                }
                else
                {
                    if (String.IsNullOrEmpty(dtAssestment.GetValue(aCriteria.DataBind.Alias, dtAssestment.Rows.Count - 1)) )
                    {
                    }
                    else
                    {

                        dtAssestment.Rows.Add(1);
                        RowValue = dtAssestment.Rows.Count;
                        dtAssestment.SetValue(aIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtAssestment.SetValue(aId.DataBind.Alias, RowValue - 1, "0");
                        dtAssestment.SetValue(aCriteria.DataBind.Alias, RowValue - 1, "");
                        dtAssestment.SetValue(aDescription.DataBind.Alias, RowValue - 1, "");
                        dtAssestment.SetValue(aMarks.DataBind.Alias, RowValue - 1, "0");
                        dtAssestment.SetValue(aMarksObtain.DataBind.Alias, RowValue - 1, "0");
                        dtAssestment.SetValue(aPanelist.DataBind.Alias, RowValue - 1, "");
                        dtAssestment.SetValue(aRequiredScore.DataBind.Alias, RowValue - 1, "0");
                        dtAssestment.SetValue(aRemarks.DataBind.Alias, RowValue - 1, "");
                        mtAssestment.AddRow(1, mtAssestment.RowCount);
                    }
                }
                mtAssestment.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void AddEmptyRowPanelist()
        {
            try
            {
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillComboColumnPanelist(SAPbouiCOM.Column pCombo)
        {
            try
            {
                //String InterviewCall;
                //if (!String.IsNullOrEmpty(txtInterviewCall.Value))
                //{
                //    InterviewCall = txtInterviewCall.Value;
                //}
                //else
                //{
                //    return;
                //}
                //if (pCombo.ValidValues.Count > 0)
                //{
                //    Int32 ComboCount = pCombo.ValidValues.Count;
                //    for (Int32 i = ComboCount - 1; i >= 0; i--)
                //    {
                //        pCombo.ValidValues.Remove(pCombo.ValidValues.Item(i).Value);
                //    }

                //    IEnumerable<TrnsInterviewCallPanelList> Collection = from a in dbHrPayroll.TrnsInterviewCallPanelList where a.TrnsInterviewCall.DocNum.ToString() == InterviewCall select a;
                //    foreach (TrnsInterviewCallPanelList One in Collection)
                //    {
                //        String Description = One.MstEmployee.EmpID + " : " +  One.MstEmployee.FirstName + " " + One.MstEmployee.MiddleName + " " + One.MstEmployee.LastName;
                //        pCombo.ValidValues.Add(One.MstEmployee.ID.ToString(), Description);
                //    }
                //}
                //else
                //{
                //    IEnumerable<TrnsInterviewCallPanelList> Collection = from a in dbHrPayroll.TrnsInterviewCallPanelList where a.TrnsInterviewCall.DocNum.ToString() == InterviewCall select a;
                //    foreach (TrnsInterviewCallPanelList One in Collection)
                //    {
                //        String Description = One.MstEmployee.EmpID + " : " + One.MstEmployee.FirstName + " " + One.MstEmployee.MiddleName + " " + One.MstEmployee.LastName;
                //        pCombo.ValidValues.Add(One.MstEmployee.ID.ToString(), Description);
                //    }
                //}
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void SetAssestmentLine(Int32 RowToSet, Int32 CriteriaSelection)
        {
            try
            {
                MstAssestmentCriteria One = (from a in dbHrPayroll.MstAssestmentCriteria where a.ID == CriteriaSelection select a).FirstOrDefault();
                dtAssestment.SetValue(aCriteria.DataBind.Alias, RowToSet, CriteriaSelection);
                dtAssestment.SetValue(aDescription.DataBind.Alias, RowToSet, One.Description);
                dtAssestment.SetValue(aMarks.DataBind.Alias, RowToSet, One.Marks.ToString());
                dtAssestment.SetValue(aRequiredScore.DataBind.Alias, RowToSet, One.MinMarks.ToString());
                dtAssestment.SetValue(aPanelist.DataBind.Alias, RowToSet, "");
                dtAssestment.SetValue(aMarksObtain.DataBind.Alias, RowToSet, "0.0");
                dtAssestment.SetValue(aRemarks.DataBind.Alias, RowToSet, "");
                mtAssestment.SetLineData(RowToSet + 1);
                //AddEmptyRowAssestment();

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillPanelistDataOnSelection()
        {
            try
            {
                if (DocLoad)
                {
                    Int32 DocNum = Convert.ToInt32(txtDocNum.Value.Trim());
                    String SelectedPanelist = cbPanelist.Selected.Description.Trim();
                    
                    IEnumerable<TrnsInterviewEASAssetment> One = from a in dbHrPayroll.TrnsInterviewEASAssetment where a.TrnsInterviewEAS.DocNum == DocNum && a.MstEmployee.EmpID == SelectedPanelist select a;
                    dtPanelist.Rows.Clear();
                    Int32 i = 0;
                    foreach (TrnsInterviewEASAssetment OneLine in One)
                    {
                        dtPanelist.Rows.Add(1);
                        dtPanelist.SetValue(pIsNew.DataBind.Alias, i, "N");
                        dtPanelist.SetValue(pId.DataBind.Alias, i, OneLine.ID);
                        dtPanelist.SetValue(pAssestmentCode.DataBind.Alias, i, OneLine.CriteriaId.ToString());
                        dtPanelist.SetValue(pDescription.DataBind.Alias, i, OneLine.Description);
                        dtPanelist.SetValue(pMarks.DataBind.Alias, i, OneLine.Marks.ToString());
                        dtPanelist.SetValue(pMarksObtain.DataBind.Alias, i, OneLine.Obtain.ToString());
                        dtPanelist.SetValue(pRemarks.DataBind.Alias, i, OneLine.Remarks);
                        i++;
                    }
                    mtPanelist.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void CreateEmployee()
        {
            try
            {
                //if (DocLoad)
                //{
                //    Int32 InterviewEASDocNum = 0, CandidateID = 0;

                //    InterviewEASDocNum = Convert.ToInt32(txtDocNum.Value.Trim());

                //    CandidateID = (from a in dbHrPayroll.TrnsInterviewEAS where a.DocNum == InterviewEASDocNum select a.TrnsInterviewCall.MstCandidate.ID).FirstOrDefault();
                //    MstCandidate oCan = (from a in dbHrPayroll.MstCandidate where a.ID == CandidateID select a).FirstOrDefault();
                //    MstEmployee oEmp = new MstEmployee();
                //    MstUsers oUsr = new MstUsers();
                //    if (String.IsNullOrEmpty(oCan.EmpCode))
                //    {
                //        oApplication.StatusBar.SetText("Update EMPCode in Candidate Master, Employee not generated", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //        return;
                //    }
                //    if (String.IsNullOrEmpty(oCan.UserCode))
                //    {
                //        oApplication.StatusBar.SetText("Update UserCode in Candidate Master, Employee not generated", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //        return;
                //    }
                //    oEmp.EmpID = oCan.EmpCode;
                //    oEmp.FirstName = oCan.FirstName;
                //    oEmp.MiddleName = oCan.MiddleName != null ? oCan.MiddleName : null;
                //    oEmp.LastName = oCan.LastName;
                //    oEmp.BasicSalary = 0.0M;
                //    oEmp.FlgActive = true;
                //    oEmp.FlgUser = true;
                //    oEmp.IntSboPublished = false;
                //    oEmp.IntSboTransfered = false;
                //    if (oCan.Position != null)
                //    {
                //        oEmp.PositionID = oCan.MstPosition.Id;
                //    }
                //    else
                //    {
                //        oEmp.PositionID = null;
                //    }
                //    if (oCan.Branch != null)
                //    {
                //        oEmp.BranchID = oCan.MstBranches.Id;
                //    }
                //    else
                //    {
                //        oEmp.BranchID = null;
                //    }
                //    if (oCan.Department != null)
                //    {
                //        oEmp.DepartmentID = oCan.MstDepartment.ID;
                //    }
                //    else
                //    {
                //        oEmp.DepartmentID = null;
                //    }
                //    if (oCan.Location != null)
                //    {
                //        oEmp.Location = oCan.MstLocation.Id;
                //    }
                //    else
                //    {
                //        oEmp.Location = null;
                //    }
                //    oEmp.OfficePhone = oCan.OfficePhone != null ? oCan.OfficePhone : null;
                //    oEmp.HomePhone = oCan.HomePhone != null ? oCan.HomePhone : null;
                //    oEmp.OfficeMobile = oCan.MobilePhone != null ? oCan.MobilePhone : null;
                //    oEmp.OfficeExtension = oCan.Extension != null ? oCan.Extension : null;
                //    oEmp.Pager = oCan.Pager != null ? oCan.Pager : null;
                //    oEmp.Fax = oCan.Fax != null ? oCan.Fax :null;
                //    oEmp.OfficeEmail = oCan.Email != null ? oCan.Email : null;

                //    oEmp.CreateDate = DateTime.Now;
                //    oEmp.UpdateDate = DateTime.Now;
                //    oEmp.UpdatedBy = oCompany.UserName;

                //    oUsr.UserID = oCan.UserCode;
                //    oUsr.UserCode = oCan.UserCode;
                //    oUsr.PassCode = "12345";
                //    oUsr.CreateDate = DateTime.Now;
                //    oUsr.UpdateDate = DateTime.Now;
                //    oUsr.CreatedBy = oCompany.UserName;
                //    oUsr.UpdatedBy = oCompany.UserName;

                //    oEmp.MstUsers.Add(oUsr);
                //    dbHrPayroll.MstEmployee.InsertOnSubmit(oEmp);
                //    dbHrPayroll.SubmitChanges();
                //    oApplication.StatusBar.SetText("Employee Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                //}
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void doFind()
        {
            try
            {

                PrepareSearchKeyHash();
                string strSql = sqlString.getSql("InterviewEvaluation", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Interview Evaluation", "Select Interview Evaluation");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    currentObjId = st.Rows[0][0].ToString();
                    getRecord(currentObjId);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
