using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_InterviewCall:HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.Button btnMain, btnCancel;
        SAPbouiCOM.Button btnPrintLetter;
        SAPbouiCOM.EditText txtCandidateNo, txtFirstName, txtLastName, txtPhone, txtPosition, txtDepartment, txtBranch, txtAssignto;
        SAPbouiCOM.EditText txtLineManager, txtSubject, txtDocNo, txtCreatedOn, txtClosedOn, txtVacancyNo, txtValidTill;
        SAPbouiCOM.EditText txtStartTime, txtEndTime, txtDuration, txtReminderValue, txtRemarks;
        SAPbouiCOM.CheckBox chkReminder, chkDisplayCalendar;
        SAPbouiCOM.ComboBox cbLocation, cbDocStatus, cbReminderUnit;
        SAPbouiCOM.Matrix mtActivity, mtPanelist;
        SAPbouiCOM.DataTable dtActivity, dtPanelist;
        SAPbouiCOM.Column aEmpid, aActivityDt, aRecurrence, aContent, aId, aIsNew;
        SAPbouiCOM.Column pEmpId, pName, pPosition, pBranch, pDepartment, pId, pIsNew;

        SAPbouiCOM.Item itxtCandidateNo, itxtFirstName, itxtLastName, itxtPhone, itxtPosition, itxtDepartment, itxtBranch, itxtAssignto;
        SAPbouiCOM.Item itxtLineManager, itxtSubject, itxtDocNo, itxtCreatedOn, itxtClosedOn, itxtVacancyNo, itxtValidTill;
        SAPbouiCOM.Item itxtStartTime, itxtEndTime, itxtDuration, itxtReminderValue, itxtRemarks;
        SAPbouiCOM.Item ichkReminder, ichkDisplayCalendar;
        SAPbouiCOM.Item icbLocation, icbDocStatus, icbReminderUnit;
        SAPbouiCOM.Item imtActivity, imtPanelist; 


        //private Hashtable CodeIndex = new Hashtable();
        //private Int32 CurrentRecord = 0, TotalRecords;
        IEnumerable<TrnsInterviewCall> oCollection = null;

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
                    CheckMainButtonStatus();
                    break;
                case "btprint":
                    ShowPrintLetter();
                    break;
            }
        }

        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "mtpanelist":
                    Int32 TempEmpid, TempRowNumber;
                    TempRowNumber = pVal.Row;
                    SAPbouiCOM.ComboBox TempField = mtPanelist.GetCellSpecific(pEmpId.DataBind.Alias, TempRowNumber);
                    TempEmpid = Convert.ToInt32(TempField.Value.Trim());
                    if (TempEmpid != 1)
                    {
                        SetFieldsInPanelistGrid(TempEmpid, TempRowNumber - 1);
                    }
                    break;
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);

            if (pVal.ItemUID == "mtactivity" && pVal.ColUID == "content")
            {
                mtActivity.FlushToDataSource();
                AddEmptyRowActivity();
            }
            //if (pVal.ItemUID == "mtpanelist" && pVal.ColUID == "empid")
            //{
            //    mtPanelist.FlushToDataSource();
            //    AddEmptyRowPanelist();
            //}
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE && btnMain.Caption == "Ok")
            {
                btnMain.Caption = "Update";
            }
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            //GetNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            //GetPreviosRecord();
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            InitiallizeDocument("Find");
        }

        public override void fillFields()
        {
            base.fillFields();
            FillFields();
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            try
            {
                SearchKeyVal.Clear();
                String DocNumber, CandidateNo, DeptName, DesigName, BranchName;
                DateTime ScheduleDate;

                DocNumber = txtDocNo.Value.Trim();
                CandidateNo = txtCandidateNo.Value.Trim();
                DeptName = txtDepartment.Value.Trim();
                DesigName = txtPosition.Value.Trim();
                BranchName = txtBranch.Value.Trim();

                if (String.IsNullOrEmpty(txtDuration.Value))
                {
                    SearchKeyVal.Add("dtFrom", "01/01/2005");
                    SearchKeyVal.Add("dtTo", "12/31/2030");
                }
                else
                {
                    ScheduleDate = Convert.ToDateTime(txtDuration.Value.Trim());
                    SearchKeyVal.Add("dtFrom", ScheduleDate);
                    SearchKeyVal.Add("dtTo", ScheduleDate);
                }
                if (!String.IsNullOrEmpty(DocNumber))
                {
                    SearchKeyVal.Add("DocNum", DocNumber);
                }
                else
                {
                    SearchKeyVal.Add("DocNum","%");
                }
                if (!String.IsNullOrEmpty(CandidateNo))
                {
                    SearchKeyVal.Add("CanNo", CandidateNo);
                }
                else
                {
                    SearchKeyVal.Add("CanNo", "%");
                }
                if (!String.IsNullOrEmpty(DeptName))
                {
                    SearchKeyVal.Add("DeptName", DeptName);
                }
                else
                {
                    SearchKeyVal.Add("DeptName", "%");
                }
                if (!String.IsNullOrEmpty(DesigName))
                {
                    SearchKeyVal.Add("DesigName", DesigName);
                }
                else
                {
                    SearchKeyVal.Add("DesigName", "%");
                }
                if (!String.IsNullOrEmpty(BranchName))
                {
                    SearchKeyVal.Add("BranchName", BranchName);
                }
                else
                {
                    SearchKeyVal.Add("BranchName", "%");
                }
            }
            catch(Exception ex)
            {
            }
        }
        
        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                //Button Section

                btnMain = oForm.Items.Item("btmain").Specific;
                btnPrintLetter = oForm.Items.Item("btprint").Specific;
                //CheckBox Section 

                chkReminder = oForm.Items.Item("chkrem").Specific;
                ichkReminder = oForm.Items.Item("chkrem");
                oForm.DataSources.UserDataSources.Add("chkrem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkReminder.DataBind.SetBound(true, "", "chkrem");
                chkReminder.Checked = false;

                chkDisplayCalendar = oForm.Items.Item("chkcal").Specific;
                ichkDisplayCalendar = oForm.Items.Item("chkcal");
                oForm.DataSources.UserDataSources.Add("chkcal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkDisplayCalendar.DataBind.SetBound(true, "", "chkcal");
                chkDisplayCalendar.Checked = false;

                //TextBoxes

                txtCandidateNo = oForm.Items.Item("txcanno").Specific;
                itxtCandidateNo = oForm.Items.Item("txcanno");
                oForm.DataSources.UserDataSources.Add("txcanno", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtCandidateNo.DataBind.SetBound(true, "", "txcanno");

                txtFirstName = oForm.Items.Item("txfname").Specific;
                itxtFirstName = oForm.Items.Item("txfname");
                oForm.DataSources.UserDataSources.Add("txfname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtFirstName.DataBind.SetBound(true, "", "txfname");

                txtLastName = oForm.Items.Item("txlname").Specific;
                itxtLastName = oForm.Items.Item("txlname");
                oForm.DataSources.UserDataSources.Add("txlname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtLastName.DataBind.SetBound(true, "", "txlname");

                txtPhone = oForm.Items.Item("txtphone").Specific;
                itxtPhone = oForm.Items.Item("txtphone");
                oForm.DataSources.UserDataSources.Add("txtphone", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPhone.DataBind.SetBound(true, "", "txtphone");

                txtPosition = oForm.Items.Item("txposition").Specific;
                itxtPosition = oForm.Items.Item("txposition");
                oForm.DataSources.UserDataSources.Add("txposition", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPosition.DataBind.SetBound(true, "", "txposition");

                txtDepartment = oForm.Items.Item("txdept").Specific;
                itxtDepartment = oForm.Items.Item("txdept");
                oForm.DataSources.UserDataSources.Add("txdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDepartment.DataBind.SetBound(true, "", "txdept");

                txtBranch = oForm.Items.Item("txbranch").Specific;
                itxtBranch = oForm.Items.Item("txbranch");
                oForm.DataSources.UserDataSources.Add("txbranch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtBranch.DataBind.SetBound(true, "", "txbranch");

                txtAssignto = oForm.Items.Item("txass").Specific;
                itxtAssignto = oForm.Items.Item("txass");
                oForm.DataSources.UserDataSources.Add("txass", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtAssignto.DataBind.SetBound(true, "", "txass");

                txtLineManager = oForm.Items.Item("txlnman").Specific;
                itxtLineManager = oForm.Items.Item("txlnman");
                oForm.DataSources.UserDataSources.Add("txlnman", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtLineManager.DataBind.SetBound(true, "", "txlnman");

                txtDocNo = oForm.Items.Item("txdocno").Specific;
                itxtDocNo = oForm.Items.Item("txdocno");
                oForm.DataSources.UserDataSources.Add("txdocno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDocNo.DataBind.SetBound(true, "", "txdocno");

                txtCreatedOn = oForm.Items.Item("txcreated").Specific;
                itxtCreatedOn = oForm.Items.Item("txcreated");
                oForm.DataSources.UserDataSources.Add("txcreated", SAPbouiCOM.BoDataType.dt_DATE);
                txtCreatedOn.DataBind.SetBound(true, "", "txcreated");

                txtClosedOn = oForm.Items.Item("txclosed").Specific;
                itxtClosedOn = oForm.Items.Item("txclosed");
                oForm.DataSources.UserDataSources.Add("txclosed", SAPbouiCOM.BoDataType.dt_DATE);
                txtClosedOn.DataBind.SetBound(true, "", "txclosed");

                txtVacancyNo = oForm.Items.Item("txvacno").Specific;
                itxtVacancyNo = oForm.Items.Item("txvacno");
                oForm.DataSources.UserDataSources.Add("txvacno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtVacancyNo.DataBind.SetBound(true, "", "txvacno");

                txtValidTill = oForm.Items.Item("txvalid").Specific;
                itxtValidTill = oForm.Items.Item("txvalid");
                oForm.DataSources.UserDataSources.Add("txvalid", SAPbouiCOM.BoDataType.dt_DATE);
                txtValidTill.DataBind.SetBound(true, "", "txvalid");

                txtSubject = oForm.Items.Item("txsubject").Specific;
                itxtSubject = oForm.Items.Item("txsubject");
                oForm.DataSources.UserDataSources.Add("txsubject", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSubject.DataBind.SetBound(true, "", "txsubject");

                txtStartTime = oForm.Items.Item("txstartime").Specific;
                itxtStartTime = oForm.Items.Item("txstartime");
                oForm.DataSources.UserDataSources.Add("txstartime", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtStartTime.DataBind.SetBound(true, "", "txstartime");

                txtEndTime = oForm.Items.Item("txendtime").Specific;
                itxtEndTime = oForm.Items.Item("txendtime");
                oForm.DataSources.UserDataSources.Add("txendtime", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtEndTime.DataBind.SetBound(true, "", "txendtime");

                txtDuration = oForm.Items.Item("txdur").Specific;
                itxtDuration = oForm.Items.Item("txdur");
                oForm.DataSources.UserDataSources.Add("txdur", SAPbouiCOM.BoDataType.dt_DATE);
                txtDuration.DataBind.SetBound(true, "", "txdur");

                txtReminderValue = oForm.Items.Item("txremval").Specific;
                itxtReminderValue = oForm.Items.Item("txremval");
                oForm.DataSources.UserDataSources.Add("txremval", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 5);
                txtReminderValue.DataBind.SetBound(true, "", "txremval");

                txtRemarks = oForm.Items.Item("txremark").Specific;
                itxtRemarks = oForm.Items.Item("txremark");
                oForm.DataSources.UserDataSources.Add("txremark", SAPbouiCOM.BoDataType.dt_LONG_TEXT);
                txtRemarks.DataBind.SetBound(true, "", "txremark");

                //ComboBoxes

                cbDocStatus = oForm.Items.Item("cbstatus").Specific;
                icbDocStatus = oForm.Items.Item("cbstatus");
                oForm.DataSources.UserDataSources.Add("cbstatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDocStatus.DataBind.SetBound(true, "", "cbstatus");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                icbLocation = oForm.Items.Item("cbloc");
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbLocation.DataBind.SetBound(true, "", "cbloc");

                cbReminderUnit = oForm.Items.Item("cbremunit").Specific;
                icbReminderUnit = oForm.Items.Item("cbremunit");
                oForm.DataSources.UserDataSources.Add("cbremunit", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbReminderUnit.DataBind.SetBound(true, "", "cbremunit");

                //Activity Tab

                mtActivity = oForm.Items.Item("mtactivity").Specific;
                imtActivity = oForm.Items.Item("mtactivity");
                dtActivity = oForm.DataSources.DataTables.Item("dtactivity");
                aIsNew = mtActivity.Columns.Item("isnew");
                aId = mtActivity.Columns.Item("id");
                aEmpid = mtActivity.Columns.Item("empid");
                aActivityDt = mtActivity.Columns.Item("activitydt");
                aRecurrence = mtActivity.Columns.Item("recur");
                aContent = mtActivity.Columns.Item("content");
                aIsNew.Visible = false;
                aId.Visible = false;

                //Panelist Tab

                mtPanelist = oForm.Items.Item("mtpanelist").Specific;
                imtPanelist = oForm.Items.Item("mtpanelist");
                dtPanelist = oForm.DataSources.DataTables.Item("dtpanelist");
                pIsNew = mtPanelist.Columns.Item("isnew");
                pId = mtPanelist.Columns.Item("id");
                pEmpId = mtPanelist.Columns.Item("empid");
                pPosition = mtPanelist.Columns.Item("position");
                pDepartment = mtPanelist.Columns.Item("dept");
                pBranch = mtPanelist.Columns.Item("branch");
                pName = mtPanelist.Columns.Item("name");
                pIsNew.Visible = false;
                pId.Visible = false;

                //FillComboBoxes
                FillLocationsCombo(cbLocation);
                FillLOVListCombo(cbDocStatus, "DocStatus");
                FillLOVListCombo(cbReminderUnit, "EXPUnit");
                FillEmployeesColumn(pEmpId);
                FillEmployeesColumn(aEmpid);
                
                GetData();
                FormStatus();
                btnMain.Caption = "Ok";

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FormStatus()
        {
            itxtCandidateNo.AffectsFormMode = true;
            itxtFirstName.AffectsFormMode = true;
            itxtLastName.AffectsFormMode = true;
            itxtPhone.AffectsFormMode = true;
            itxtPosition.AffectsFormMode = true;
            itxtDepartment.AffectsFormMode = true;
            itxtBranch.AffectsFormMode = true;
            itxtAssignto.AffectsFormMode = true;
            itxtLineManager.AffectsFormMode = true; 
            itxtSubject.AffectsFormMode = true; 
            itxtDocNo.AffectsFormMode = true; 
            itxtCreatedOn.AffectsFormMode = true; 
            itxtClosedOn.AffectsFormMode = true; 
            itxtVacancyNo.AffectsFormMode = true; 
            itxtValidTill.AffectsFormMode = true;
            itxtStartTime.AffectsFormMode = true; 
            itxtEndTime.AffectsFormMode = true; 
            itxtDuration.AffectsFormMode = true; 
            itxtReminderValue.AffectsFormMode = true; 
            itxtRemarks.AffectsFormMode = true;
            ichkReminder.AffectsFormMode = true;
            ichkDisplayCalendar.AffectsFormMode = true;
            icbLocation.AffectsFormMode = true; 
            icbDocStatus.AffectsFormMode = true; 
            icbReminderUnit.AffectsFormMode = true;
            imtActivity.AffectsFormMode = true; 
            imtPanelist.AffectsFormMode = true;
        }

        private void InitiallizeDocument(String pCase)
        {
            oForm.Freeze(true);
            try
            {
                switch (pCase)
                {
                    case "LV0001":
                        //Headers
                        itxtCandidateNo.Enabled = false;
                        itxtFirstName.Enabled = false;
                        itxtLastName.Enabled = false;
                        itxtPhone.Enabled = false;
                        itxtPosition.Enabled = false;
                        itxtDepartment.Enabled = false;
                        itxtBranch.Enabled = false;
                        itxtAssignto.Enabled = false;
                        itxtLineManager.Enabled = false;
                        itxtSubject.Enabled = true;
                        itxtSubject.Click();
                        itxtDocNo.Enabled = false;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        itxtVacancyNo.Enabled = false;
                        itxtValidTill.Enabled = false;
                        //Schedule
                        itxtStartTime.Enabled = true;
                        itxtEndTime.Enabled = true;
                        itxtDuration.Enabled = true;
                        icbLocation.Enabled = true;
                        ichkDisplayCalendar.Enabled = true;
                        ichkReminder.Enabled = true;
                        itxtReminderValue.Enabled = true;
                        icbReminderUnit.Enabled = true;
                        //Remarks
                        itxtRemarks.Enabled = true;

                        //Matrix 
                        imtActivity.Enabled = true;
                        imtPanelist.Enabled = true;
                        itxtSubject.Click();
                        break;
                    case "LV0002":
                        itxtCandidateNo.Enabled = false;
                        itxtFirstName.Enabled = false;
                        itxtLastName.Enabled = false;
                        itxtPhone.Enabled = false;
                        itxtPosition.Enabled = false;
                        itxtDepartment.Enabled = false;
                        itxtBranch.Enabled = false;
                        itxtAssignto.Enabled = false;
                        itxtLineManager.Enabled = false;
                        itxtSubject.Enabled = true;
                        itxtSubject.Click();
                        itxtDocNo.Enabled = false;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        itxtVacancyNo.Enabled = false;
                        itxtValidTill.Enabled = false;
                        //Schedule
                        itxtStartTime.Enabled = false;
                        itxtEndTime.Enabled = false;
                        itxtDuration.Enabled = false;
                        icbLocation.Enabled = false;
                        ichkDisplayCalendar.Enabled = false;
                        ichkReminder.Enabled = false;
                        itxtReminderValue.Enabled = false;
                        icbReminderUnit.Enabled = false;
                        //Remarks
                        itxtRemarks.Enabled = true;

                        //Matrix 
                        imtActivity.Enabled = false;
                        imtPanelist.Enabled = false;
                        break;
                    case "LV0003":
                        itxtCandidateNo.Enabled = false;
                        itxtFirstName.Enabled = false;
                        itxtLastName.Enabled = false;
                        itxtPhone.Enabled = false;
                        itxtPosition.Enabled = false;
                        itxtDepartment.Enabled = false;
                        itxtBranch.Enabled = false;
                        itxtAssignto.Enabled = false;
                        itxtLineManager.Enabled = false;
                        itxtSubject.Enabled = true;
                        itxtSubject.Click();
                        itxtDocNo.Enabled = false;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = false;
                        itxtClosedOn.Enabled = false;
                        itxtVacancyNo.Enabled = false;
                        itxtValidTill.Enabled = false;
                        //Schedule
                        itxtStartTime.Enabled = false;
                        itxtEndTime.Enabled = false;
                        itxtDuration.Enabled = false;
                        icbLocation.Enabled = false;
                        ichkDisplayCalendar.Enabled = false;
                        ichkReminder.Enabled = false;
                        itxtReminderValue.Enabled = false;
                        icbReminderUnit.Enabled = false;
                        //Remarks
                        itxtRemarks.Enabled = true;

                        //Matrix 
                        imtActivity.Enabled = false;
                        imtPanelist.Enabled = false;
                        break;
                    case "New":
                        //Header Values
                        txtCandidateNo.Value = "";
                        txtFirstName.Value = "";
                        txtLastName.Value = "";
                        txtPhone.Value = "";
                        txtPosition.Value = "";
                        txtDepartment.Value = "";
                        txtBranch.Value = "";
                        txtAssignto.Value = "";
                        txtLineManager.Value = "";
                        txtSubject.Value = "";
                        txtDocNo.Value = "";
                        cbDocStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        txtCreatedOn.Value = "";
                        txtClosedOn.Value = "";
                        txtVacancyNo.Value = "";
                        txtValidTill.Value = "";
                        //Schedule
                        txtStartTime.Value = "";
                        txtEndTime.Value = "";
                        txtDuration.Value = "";
                        cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        chkDisplayCalendar.Checked = false;
                        chkReminder.Checked = false;
                        txtReminderValue.Value = "";
                        cbReminderUnit.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        //Remarks Value
                        txtRemarks.Value = "";
                        //Clear Matrix
                        dtActivity.Rows.Clear();
                        mtActivity.LoadFromDataSource();
                        dtPanelist.Rows.Clear();
                        mtPanelist.LoadFromDataSource();

                        //Headers
                        itxtCandidateNo.Enabled = true;
                        itxtFirstName.Enabled = true;
                        itxtLastName.Enabled = true;
                        itxtPhone.Enabled = true;
                        itxtPosition.Enabled = true;
                        itxtDepartment.Enabled = true;
                        itxtBranch.Enabled = true;
                        itxtAssignto.Enabled = true;
                        itxtLineManager.Enabled = true;
                        itxtSubject.Enabled = true;
                        itxtSubject.Click();
                        itxtDocNo.Enabled = true;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = true;
                        itxtClosedOn.Enabled = true;
                        itxtVacancyNo.Enabled = true;

                        itxtValidTill.Enabled = true;
                        //Schedule
                        itxtStartTime.Enabled = true;
                        itxtEndTime.Enabled = true;
                        itxtDuration.Enabled = true;
                        icbLocation.Enabled = true;
                        ichkDisplayCalendar.Enabled = true;
                        ichkReminder.Enabled = true;
                        itxtReminderValue.Enabled = true;
                        icbReminderUnit.Enabled = true;
                        //Remarks
                        itxtRemarks.Enabled = true;

                        //Matrix 
                        imtActivity.Enabled = true;
                        imtPanelist.Enabled = true;
                        itxtSubject.Click();
                        break;
                    case "Find":
                        //Header Values
                        txtCandidateNo.Value = "";
                        txtFirstName.Value = "";
                        txtLastName.Value = "";
                        txtPhone.Value = "";
                        txtPosition.Value = "";
                        txtDepartment.Value = "";
                        txtBranch.Value = "";
                        txtAssignto.Value = "";
                        txtLineManager.Value = "";
                        txtSubject.Value = "";
                        txtDocNo.Value = "";
                        cbDocStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        txtCreatedOn.Value = "";
                        txtClosedOn.Value = "";
                        txtVacancyNo.Value = "";
                        txtValidTill.Value = "";
                        //Schedule
                        txtStartTime.Value = "";
                        txtEndTime.Value = "";
                        txtDuration.Value = "";
                        cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        chkDisplayCalendar.Checked = false;
                        chkReminder.Checked = false;
                        txtReminderValue.Value = "";
                        cbReminderUnit.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        //Remarks Value
                        txtRemarks.Value = "";
                        //Clear Matrix
                        dtActivity.Rows.Clear();
                        mtActivity.LoadFromDataSource();
                        dtPanelist.Rows.Clear();
                        mtPanelist.LoadFromDataSource();

                        //Headers
                        btnMain.Caption = "Find";
                        itxtCandidateNo.Enabled = true;
                        itxtFirstName.Enabled = true;
                        itxtLastName.Enabled = true;
                        itxtPhone.Enabled = true;
                        itxtPosition.Enabled = true;
                        itxtDepartment.Enabled = true;
                        itxtBranch.Enabled = true;
                        itxtAssignto.Enabled = true;
                        itxtLineManager.Enabled = true;
                        itxtSubject.Enabled = true;
                        itxtSubject.Click();
                        itxtDocNo.Enabled = true;
                        icbDocStatus.Enabled = true;
                        itxtCreatedOn.Enabled = true;
                        itxtClosedOn.Enabled = true;
                        itxtVacancyNo.Enabled = true;

                        itxtValidTill.Enabled = true;
                        //Schedule
                        itxtStartTime.Enabled = true;
                        itxtEndTime.Enabled = true;
                        itxtDuration.Enabled = true;
                        icbLocation.Enabled = true;
                        ichkDisplayCalendar.Enabled = true;
                        ichkReminder.Enabled = true;
                        itxtReminderValue.Enabled = true;
                        icbReminderUnit.Enabled = true;
                        //Remarks
                        itxtRemarks.Enabled = true;

                        //Matrix 
                        imtActivity.Enabled = true;
                        imtPanelist.Enabled = true;
                        itxtSubject.Click();
                        break;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception InitiallizeDocument Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void CheckMainButtonStatus()
        {
            switch (btnMain.Caption)
            {
                case "Add":
                    break;
                case "Update":
                    if (UpdateDocument())
                    {
                        InitiallizeDocument("New");
                        btnMain.Caption = "Ok";
                    }
                    else
                    {
                        btnMain.Caption = "Update";
                    }
                    break;
                case "Ok":
                    oForm.Close();
                    break;
                case "Find":
                    doFind();
                    break;

            }

        }

        private Boolean UpdateDocument()
        {
            Boolean retValue = true;
            String DocumentStatus = cbDocStatus.Value.Trim();
            try
            {
                ////Int32 CurrentDoc = Convert.ToInt32(txtDocNo.Value.Trim());
                //TrnsInterviewCall oUpd = oCollection.ElementAt<TrnsInterviewCall>(currentRecord);

                //oUpd.Subject = txtSubject.Value.Trim();
                ////Step BY Step To closure
                //if (oUpd.DocStatus == "LV0001" && DocumentStatus == "LV0002")
                //{
                //    oUpd.DocStatus = "LV0002";
                //}
                //if (oUpd.DocStatus == "LV0002" && DocumentStatus == "LV0003")
                //{
                //    oUpd.DocStatus = "LV0003";
                //}
                ////Activity Tab
                //mtActivity.FlushToDataSource();
                //Int32 RowCount = 0;
                //RowCount = dtActivity.Rows.Count;
                //for (Int32 i = 0; i < RowCount; i++)
                //{
                //    Int32 id;
                //    DateTime ActivityDt;
                //    String Empid, Recurrences, Content, IsNew;
                //    IsNew = dtActivity.GetValue(aIsNew.DataBind.Alias, i);
                //    id = dtActivity.GetValue(aId.DataBind.Alias, i);
                //    Empid = dtActivity.GetValue(aEmpid.DataBind.Alias, i);
                //    ActivityDt = dtActivity.GetValue(aActivityDt.DataBind.Alias, i);
                //    Recurrences = dtActivity.GetValue(aRecurrence.DataBind.Alias, i);
                //    Content = dtActivity.GetValue(aContent.DataBind.Alias, i);
                //    if (IsNew == "Y")
                //    {
                //        if (!String.IsNullOrEmpty(Empid))
                //        {
                //            TrnsInterviewCallActivity One = new TrnsInterviewCallActivity();
                //            One.EmpID = Convert.ToInt32(Empid);
                //            One.ActivityDt = ActivityDt;
                //            One.FlgRecurrence = Recurrences == "Y" ? true : false;
                //            One.ActivityContent = Content;
                //            oUpd.TrnsInterviewCallActivity.Add(One);
                //        }
                //    }
                //    else
                //    {
                //        TrnsInterviewCallActivity One = (from a in dbHrPayroll.TrnsInterviewCallActivity where a.ID == id select a).FirstOrDefault();
                //        One.EmpID = Convert.ToInt32(Empid);
                //        One.ActivityDt = ActivityDt;
                //        One.FlgRecurrence = Recurrences == "Y" ? true : false;
                //        One.ActivityContent = Content;

                //    }
                //}

                ////Panelist Tab
                //mtPanelist.FlushToDataSource();
                //RowCount = dtPanelist.Rows.Count;
                //for (Int32 i = 0; i < RowCount; i++)
                //{
                //    Int32 id;
                //    String Empid, Name, Position, Branch, Department, IsNew;
                //    IsNew = dtPanelist.GetValue(pIsNew.DataBind.Alias, i);
                //    id = dtPanelist.GetValue(pId.DataBind.Alias, i);
                //    Empid = dtPanelist.GetValue(pEmpId.DataBind.Alias, i);
                //    Name = dtPanelist.GetValue(pName.DataBind.Alias, i);
                //    Position = dtPanelist.GetValue(pPosition.DataBind.Alias, i);
                //    Branch = dtPanelist.GetValue(pBranch.DataBind.Alias, i);
                //    Department = dtPanelist.GetValue(pDepartment.DataBind.Alias, i);
                //    if (IsNew == "Y")
                //    {
                //        if (!String.IsNullOrEmpty(Empid))
                //        {
                //            TrnsInterviewCallPanelList One = new TrnsInterviewCallPanelList();
                //            One.EmpID = Convert.ToInt32(Empid);
                //            oUpd.TrnsInterviewCallPanelList.Add(One);
                //        }
                //    }
                //    else
                //    {
                //        TrnsInterviewCallPanelList One = (from a in dbHrPayroll.TrnsInterviewCallPanelList where a.ID == id select a).FirstOrDefault();
                //        One.EmpID = Convert.ToInt32(Empid);
                //    }
                //}
                ////Schedule Tab
                //oUpd.StartTime = txtStartTime.Value.Trim();
                //oUpd.EndTime = txtEndTime.Value.Trim();
                //if (!string.IsNullOrEmpty(txtDuration.Value.Trim()))
                //{
                //    oUpd.ScheduleDate = DateTime.ParseExact(txtDuration.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //}
                //else
                //{
                //    oUpd.ScheduleDate = null;
                //}
                //if (cbLocation.Value.Trim() != "-1")
                //{
                //    oUpd.LocationID = Convert.ToInt32(cbLocation.Value.Trim());
                //}
                //else
                //{
                //    oUpd.LocationID = null;
                //}
                //if (txtReminderValue.Value != "")
                //{
                //    oUpd.ReminderValue = Convert.ToByte(txtReminderValue.Value.Trim());
                //}
                //else
                //{
                //    oUpd.ReminderValue = null;
                //}
                ////oUpd.FlgReminder = chkReminder.Checked;
                //oForm.DataSources.UserDataSources.Item("chkrem").ValueEx = chkReminder.Checked ? "Y" : "N";
                //if (chkReminder.Checked)
                //{
                //    oUpd.ReminderUnit = cbReminderUnit.Value.Trim() != "-1" ? cbReminderUnit.Value.Trim() : null;
                //    oUpd.FlgReminder = chkReminder.Checked;
                //}

                ////Remarks Tab
                //oUpd.Remarks = txtRemarks.Value.Trim();

                //dbHrPayroll.SubmitChanges();
                //oApplication.StatusBar.SetText("Document Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception UpdateInterviewCall Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private void FillFields()
        {
            try
            {
                ////InitiallizeDocument("New");

                //TrnsInterviewCall oDoc = oCollection.ElementAt<TrnsInterviewCall>(currentRecord);

                ////Header Area
                //txtDocNo.Value = oDoc.DocNum.ToString();
                //txtCandidateNo.Value = oDoc.MstCandidate.CandidateNo.ToString();
                //txtFirstName.Value = oDoc.MstCandidate.FirstName + " " + oDoc.MstCandidate.MiddleName;
                //txtLastName.Value = oDoc.MstCandidate.LastName;
                //txtPhone.Value = oDoc.MstCandidate.OfficePhone.ToString();
                //if (oDoc.MstCandidate.MstDesignation != null)
                //{
                //    txtPosition.Value = oDoc.MstCandidate.MstDesignation.Name;
                //}
                //if (oDoc.MstCandidate.MstDepartment != null)
                //{
                //    txtDepartment.Value = oDoc.MstCandidate.MstDepartment.Code;
                //}
                //if (oDoc.MstCandidate.MstBranches != null)
                //{
                //    txtBranch.Value = oDoc.MstCandidate.MstBranches.Name;
                //}
                //if (oDoc.MstCandidate.MstEmployee.EmpID != null)
                //{
                //    txtAssignto.Value = oDoc.MstCandidate.MstEmployee.EmpID;
                //}
                //if (oDoc.MstCandidate.LineManagerMstEmployee.EmpID != null)
                //{
                //    txtLineManager.Value = oDoc.MstCandidate.LineManagerMstEmployee.EmpID;
                //}
                //txtSubject.Value = oDoc.Subject;
                //if (!String.IsNullOrEmpty(oDoc.DocStatus))
                //{
                //    cbDocStatus.Select(oDoc.DocStatus, SAPbouiCOM.BoSearchKey.psk_ByValue);
                //}
                //if (oDoc.CreateDt != null)
                //{
                //    txtCreatedOn.Value = Convert.ToDateTime(oDoc.CreateDt).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtCreatedOn.Value = "";
                //}
                //if (oDoc.UpdateDt != null)
                //{
                //    txtClosedOn.Value = Convert.ToDateTime(oDoc.UpdateDt).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtClosedOn.Value = "";
                //}
                //txtVacancyNo.Value = oDoc.MstCandidate.JobRequisitionNo.ToString();
                //if (oDoc.MstCandidate.ValidTo != null)
                //{
                //    txtValidTill.Value = Convert.ToDateTime(oDoc.MstCandidate.ValidTo).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtValidTill.Value = "";
                //}
                ////Activity Area
                //dtActivity.Rows.Clear();
                //Int32 i = 0;
                //foreach (TrnsInterviewCallActivity One in oDoc.TrnsInterviewCallActivity)
                //{
                //    dtActivity.Rows.Add(1);
                //    dtActivity.SetValue(aIsNew.DataBind.Alias, i, "N");
                //    dtActivity.SetValue(aId.DataBind.Alias, i, One.ID.ToString());
                //    dtActivity.SetValue(aEmpid.DataBind.Alias, i, One.EmpID.ToString());
                //    dtActivity.SetValue(aActivityDt.DataBind.Alias, i, One.ActivityDt);
                //    dtActivity.SetValue(aRecurrence.DataBind.Alias, i, One.FlgRecurrence == true ? "Y" : "N");
                //    dtActivity.SetValue(aContent.DataBind.Alias, i, One.ActivityContent);
                //    i++;
                //}
                //AddEmptyRowActivity();
                ////mtActivity.LoadFromDataSource();
                
                ////Panelist Area
                //dtPanelist.Rows.Clear();
                //i = 0;
                //foreach (TrnsInterviewCallPanelList One in oDoc.TrnsInterviewCallPanelList)
                //{
                //    dtPanelist.Rows.Add(1);
                //    dtPanelist.SetValue(pIsNew.DataBind.Alias, i, "N");
                //    dtPanelist.SetValue(pId.DataBind.Alias, i, One.ID.ToString());
                //    dtPanelist.SetValue(pEmpId.DataBind.Alias, i, One.EmpID.ToString());
                //    dtPanelist.SetValue(pName.DataBind.Alias, i, One.MstEmployee.FirstName + " " + One.MstEmployee.LastName);
                //    dtPanelist.SetValue(pPosition.DataBind.Alias, i, One.MstEmployee.PositionName != null ? One.MstEmployee.PositionName : "");
                //    dtPanelist.SetValue(pBranch.DataBind.Alias, i, One.MstEmployee.BranchName != null ? One.MstEmployee.BranchName : "");
                //    dtPanelist.SetValue(pDepartment.DataBind.Alias, i, One.MstEmployee.DepartmentName != null ?One.MstEmployee.DepartmentName : "");
                //    i++;
                //}
                //AddEmptyRowPanelist();
                ////mtPanelist.LoadFromDataSource();
                
                ////Schedule Area
                //txtStartTime.Value = oDoc.StartTime;
                //txtEndTime.Value = oDoc.EndTime;
                //if (oDoc.ScheduleDate != null)
                //{
                //    txtDuration.Value = Convert.ToDateTime(oDoc.ScheduleDate).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtDuration.Value = "";
                //}
                //cbLocation.Select(oDoc.LocationID != null ? oDoc.LocationID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                ////chkReminder.Checked = Convert.ToBoolean(oDoc.FlgReminder);
                //oForm.DataSources.UserDataSources.Item("chkrem").ValueEx = oDoc.FlgReminder == true ? "Y" : "N";
                //if (Convert.ToBoolean(oDoc.FlgReminder))
                //{
                //    txtReminderValue.Value = oDoc.ReminderValue.ToString();
                //    cbReminderUnit.Select(oDoc.ReminderUnit != null ? oDoc.ReminderUnit : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                //}
                //else
                //{
                //    txtReminderValue.Value = "";
                //    cbReminderUnit.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                //}
                ////Remarks Area
                //txtRemarks.Value = oDoc.Remarks;
                //InitiallizeDocument(oDoc.DocStatus);
                //btnMain.Caption = "Update";
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception FillFields Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillLocationsCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLocation> Locations = from a in dbHrPayroll.MstLocation select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstLocation Location in Locations)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Location.Id), Convert.ToString(Location.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillLOVListCombo(SAPbouiCOM.ComboBox pCombo, String LOVType)
        {
            try
            {
                IEnumerable<MstLOVE> iLOVList = from a in dbHrPayroll.MstLOVE where a.Type.Contains(LOVType) select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE One in iLOVList)
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

        private void GetData()
        {
            //CodeIndex.Clear();
            //oCollection = from a in dbHrPayroll.TrnsInterviewCall select a;
            //Int32 i = 0;
            //foreach (TrnsInterviewCall oDoc in oCollection)
            //{
            //    CodeIndex.Add(oDoc.ID.ToString(), i);
            //    i++;
            //}
            //totalRecord = i;
        }
        
        private void AddEmptyRowActivity()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtActivity.Rows.Count == 0)
                {
                    dtActivity.Rows.Add(1);
                    RowValue = dtActivity.Rows.Count;
                    dtActivity.SetValue(aIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtActivity.SetValue(aId.DataBind.Alias, RowValue - 1, "0");
                    dtActivity.SetValue(aEmpid.DataBind.Alias, RowValue - 1, "");
                    dtActivity.SetValue(aActivityDt.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtActivity.SetValue(aRecurrence.DataBind.Alias, RowValue - 1, "");
                    dtActivity.SetValue(aContent.DataBind.Alias, RowValue - 1, "");
                    mtActivity.AddRow(1, 0);
                }
                else
                {
                    if (Convert.ToString(dtActivity.GetValue(aEmpid.DataBind.Alias, dtActivity.Rows.Count - 1)) == "")
                    {
                    }
                    else
                    {

                        dtActivity.Rows.Add(1);
                        RowValue = dtActivity.Rows.Count;
                        dtActivity.SetValue(aIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtActivity.SetValue(aId.DataBind.Alias, RowValue - 1, "0");
                        dtActivity.SetValue(aEmpid.DataBind.Alias, RowValue - 1, "");
                        dtActivity.SetValue(aActivityDt.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtActivity.SetValue(aRecurrence.DataBind.Alias, RowValue - 1, "");
                        dtActivity.SetValue(aContent.DataBind.Alias, RowValue - 1, "");
                        mtActivity.AddRow(1, mtActivity.RowCount);
                    }
                }
                mtActivity.LoadFromDataSource();
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
                Int32 RowValue = 0;
                if (dtPanelist.Rows.Count == 0)
                {
                    dtPanelist.Rows.Add(1);
                    RowValue = dtPanelist.Rows.Count;
                    dtPanelist.SetValue(pIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtPanelist.SetValue(pId.DataBind.Alias, RowValue - 1, "0");
                    dtPanelist.SetValue(pEmpId.DataBind.Alias, RowValue - 1, "");
                    dtPanelist.SetValue(pDepartment.DataBind.Alias, RowValue - 1, "");
                    dtPanelist.SetValue(pBranch.DataBind.Alias, RowValue - 1, "");
                    dtPanelist.SetValue(pPosition.DataBind.Alias, RowValue - 1, "");
                    mtPanelist.AddRow(1, 0);
                }
                else
                {
                    if (Convert.ToString(dtPanelist.GetValue(pEmpId.DataBind.Alias, dtPanelist.Rows.Count - 1)) == "")
                    {
                    }
                    else
                    {

                        dtPanelist.Rows.Add(1);
                        RowValue = dtPanelist.Rows.Count;
                        dtPanelist.SetValue(pIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtPanelist.SetValue(pId.DataBind.Alias, RowValue - 1, "0");
                        dtPanelist.SetValue(pEmpId.DataBind.Alias, RowValue - 1, "");
                        dtPanelist.SetValue(pDepartment.DataBind.Alias, RowValue - 1, "");
                        dtPanelist.SetValue(pBranch.DataBind.Alias, RowValue - 1, "");
                        dtPanelist.SetValue(pPosition.DataBind.Alias, RowValue - 1, "");
                        mtPanelist.AddRow(1, mtPanelist.RowCount);
                    }
                }
                mtPanelist.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillEmployeesColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstEmployee> Employees = from a in dbHrPayroll.MstEmployee select a;
                foreach (MstEmployee Emp in Employees)
                {
                    String Description = "";
                    Description = Emp.EmpID + " " + Emp.FirstName + " " + Emp.MiddleName + " " + Emp.LastName;
                    OneColumn.ValidValues.Add(Convert.ToString(Emp.ID), Description);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void SetFieldsInPanelistGrid(Int32 EmployeeId, Int32 RowNumber)
        {
            try
            {
                
                MstEmployee One = (from a in dbHrPayroll.MstEmployee where a.ID == EmployeeId select a).FirstOrDefault();
                dtPanelist.SetValue(pEmpId.DataBind.Alias, RowNumber, One.ID.ToString());
                dtPanelist.SetValue(pName.DataBind.Alias, RowNumber, One.FirstName + " " + One.LastName);
                dtPanelist.SetValue(pPosition.DataBind.Alias, RowNumber, One.PositionName != null ? One.PositionName : "");
                dtPanelist.SetValue(pBranch.DataBind.Alias, RowNumber, One.BranchName != null ? One.BranchName : "");
                dtPanelist.SetValue(pDepartment.DataBind.Alias, RowNumber, One.DepartmentName != null ? One.DepartmentName : "");
                mtPanelist.SetLineData(RowNumber + 1);
                AddEmptyRowPanelist();
                
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void ShowPrintLetter()
        {
            try
            {
                //TrnsInterviewCall oDoc = null;
                //oDoc = (from a in dbHrPayroll.TrnsInterviewCall where a.DocNum.ToString() == txtDocNo.Value.Trim() select a).FirstOrDefault();
                //if (oDoc != null)
                //{
                //    if (oDoc.DocStatus == "LV0002" || oDoc.DocStatus == "LV0003")
                //    {

                //        TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.RptCode.Contains("ICL") && a.FlgSystem == true select a).FirstOrDefault();
                //        if (oReport != null)
                //        {
                //            Program.objHrmsUI.printRpt("ICL", true, "WHERE     (dbo.TrnsInterviewCall.DocNum = " + oDoc.DocNum.ToString() + ")");
                //        }
                //        else
                //        {
                //            oApplication.StatusBar.SetText("Attach Interview Call Letter Reports.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //        }
                //    }
                //    else
                //    {
                //        oApplication.StatusBar.SetText("Only Open Interview Call Letters can be printed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    }
                //}
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception ShowPrintLetter : " +ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void doFind()
        {
            try
            {

                PrepareSearchKeyHash();
                string strSql = sqlString.getSql("InterviewCall", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Interview Call", "Select  Interview Call");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    currentObjId = st.Rows[0][0].ToString();
                    getRecord(currentObjId);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                }
            }
            catch(Exception ex)
            {
            }
        }

        #endregion
    }
}
