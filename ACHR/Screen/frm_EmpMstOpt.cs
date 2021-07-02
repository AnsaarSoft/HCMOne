using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using DIHRMS;
using DIHRMS.Custom;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_EmpMstOpt : HRMSBaseForm
    {

        #region Global Variables

        SAPbouiCOM.Button btnMain, btnCancel, btnSyncTOSBO;
        SAPbouiCOM.Item ibtnMain, itxtEmployeeCode, itxtUserCode, itxtFirstName, ibtnSyncTOSBO, itbArabic, itxtManager, itxtReportTo, itxtBasicSalary, ilblBasicSalary, itxtGrossComputed;
        SAPbouiCOM.EditText txtFirstName, txtMiddleName, txtLastName, txtFatherName, txtMotherName, txtJobTitle, txGosi, txGosiV;
        SAPbouiCOM.EditText txtEmployeeCode, txtInitials, txtExtention, txtNamePrefix, txtOfficePhn, txtMobilePhn;
        SAPbouiCOM.EditText txtHomePhn, txtPager, txtFax, txtEmail, txtUserCode, txtDateOfJoining;
        SAPbouiCOM.EditText txtHomeStreet, txtHomeStreetNo, txtHomeBlock, txtHomeBuilding, txtHomeZip, txtHomeCity, txtHomeBranches;
        SAPbouiCOM.EditText txtWorkStreet, txtWorkStreetNo, txtWorkBlock, txtWorkBuilding, txtWorkZip, txtWorkCity;
        SAPbouiCOM.EditText txtPriCntName, txtPriCntRelation, txtPriCntNoLandLine, txtPriCntNoMobile, txtPriCntAddress, txtPriCntCity;
        SAPbouiCOM.EditText txtSecCntName, txtSecCntRelation, txtSecCntNoLandLine, txtSecCntNoMobile, txtSecCntAddress, txtSecCntCity;
        SAPbouiCOM.EditText txtSSNumber, txtUnionMemberShip, txtUnionMemberShipNo, txtNationality, txtIDCardNo, txtIDDtOfIssue;
        SAPbouiCOM.EditText txtBasicSalary, txtGrossComputed, txtEmpCalendar, txtEmpShift, txtWorkIM, txtPersonalIM, txtPersonalEmail, txtPersonalContact;
        SAPbouiCOM.EditText txtOrganizationUnit, txtReportTo, txtEmpContractType, txtHRCalendar, txtWindowsLogin, txtEmpGrade, txtPreviosEmpMonth;
        SAPbouiCOM.EditText txtWorkPermitRef, txtWorkPermitExpiry, txtContractExpiry, txtDOB, txtRemarks;
        SAPbouiCOM.EditText txtIDPlaceOfIssue, txtIDIssuedBy, txtIDExpiryDate, txtPassportNo, txtPassportDateofIssue, txtPassportExpiry, txtIncomeTax;
        SAPbouiCOM.EditText txtAccountTitle, txtAccountNo, txtBankName, txtBankBranch, txtEffectiveDate, txtPercentage, txtPassword, txtAttachments, txtTermination, txtResignation, txtManager, txtAllowedAdvance;
        SAPbouiCOM.EditText txtCompanyResidence;
        SAPbouiCOM.ComboBox cbHomeState, cbWorkState, cbHomeCountry, cbWorkCountry, cbPosition, cbDepartment, cbBranch, cbDesignation, cbJobTitle;
        SAPbouiCOM.ComboBox cbManager, cbPriCntState, cbPriCntCountry, cbSecCntState, cbSecCntCountry, cbMartial, cbReportingManager, cbCostCenter, cbProject;
        SAPbouiCOM.ComboBox cbReligion, cbPaymentMode, cbAccountType, cbPayroll, cbLocation, cbSalaryCurrency, cbBloodGroup, cbSBOLinkID, cbOhemUser, cbGender, cbGratuity, cmbOTSlabs;
        SAPbouiCOM.ComboBox cmbCategory, cmbEmpOffDay, cmbSubCategory, cbAttendanceAllowance;
        SAPbouiCOM.ComboBox cbDimension1, cbDimension2, cbDimension3, cbDimension4, cbDimension5;
        SAPbouiCOM.Folder tbAddress, tbEmergencyDetail, tbPersonal, tbSalary, tbAbsence, tbCommunication, tbClassification,
                            tbRelative, tbPastExperiance, tbEducation, tbQualification, tbArabic;
        SAPbouiCOM.Matrix mtAbsence, mtRelatives, mtCertification, mtPastExperiance, mtEducation;
        SAPbouiCOM.DataTable dtAbsence, dtRelatives, dtCertification, dtPastExperiance, dtEducation;
        SAPbouiCOM.Column Serial, aIsNew, aID, aDescription, aBalanceBF, aEntitled, aTotalAvailable, aUsed, aRequested, aApproved, aBalance,
                            rSerial, rIsNew, rId, rType, rFirstName, rLastName, rTelephone, rEmail, rDOB, rDepencdent, rMCNo,
                            rMCStartDate, rMCExpiryDate, cSerial, cIsNew, cId, cCertification, cAwardedBy, cAwardStatus, cDescription,
                            cNotes, cValidated, pSerial, pIsNew, pId, pCompany, pFromdt, pTodt, pPosition, pDuties, pNotes, pLastSalary,
                            eSerial, eIsNew, eId, eInstituteName, eFromDate, eToDate, eSubject, eQualification, eAwardedQlf, eMark, eNotes;
        SAPbouiCOM.CheckBox chkActiveEmployee, chkOTApplicable, flgTax, chkSup, chkPerPiece, chkEmailSalarySlip, chkSandwichLeaves, chkCompanyResidence, chkBlackListed, chkOffDay;
        SAPbouiCOM.ComboBox cbShift, cbcontractType, cmbProfitCenter, cmbTransport, cmbRecruitment, cmbInsuranceCategory, cbBonusSlabs;
        SAPbouiCOM.PictureBox pctBox;
        SAPbouiCOM.Button btnLoad, btnSave, btattch;
        SAPbouiCOM.StaticText lblDimension1, lblDimension2, lblDimension3, lblDimension4, lblDimension5;
        SAPbouiCOM.Item ilblDimension1, ilblDimension2, ilblDimension3, ilblDimension4, ilblDimension5, icmbCategory, icmbSubCategory, icmbEmpOffDay;
        SAPbouiCOM.Item icbDimension1, icbDimension2, icbDimension3, icbDimension4, icbDimension5, IcmbProfitCenter, IcmbOTSlabs, itxtAllowedAdvance, ichkSandwichLeaves;
        SAPbouiCOM.Item itxtCompanyResidence, ichkCompanyResidence, ichkBlackListed, icmbTransport, icmbRecruitment, icmbInsuranceCategory, icmbDeductionRule, ichkOffDay;
        ComboBox cmbShiftDaySlabs;
        Item icmbShiftDaySlabs;

        Boolean flgManager = false, flgReportTo = false, flgOnLoad = false, flgEmpFilter = false, flgFormMode = true;
        Boolean flgDim1, flgDim2, flgDim3, flgDim4, flgDim5;
        IEnumerable<MstEmployee> oEmployees = null;
        String FilePath, picPath;
        //Arabic Localization 

        SAPbouiCOM.EditText txtEnglishName, txtArabicName, txtPassportExpiryDtH, txtIDExpiryDtH;
        SAPbouiCOM.EditText txtMedicalCardExpirydtH, txtDrvLicCompletionDtH, txtDrvLicLastDtH, txtDrvLicReleaseDtH;
        SAPbouiCOM.EditText txtVisaNumber, txtIqamaProfessional, txtBankCardExpiryDtH;

        Int32 loadDocument = 0;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);

                InitiallizeForm();
                //Thread OThread = new Thread(fillCbs);
                //OThread.Start();
                //fillCbs();  
                AddValidValuesInCombos();
                itxtFirstName.Click();
                flgOnLoad = true;
                MsgSuccess("Employee master loaded successfully.");
                oForm.Freeze(false);
                flgEmpFilter = false;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        doSubmit();
                        break;
                    case "btSBO":
                        SyncToSBO();
                        UpdateSBO();
                        break;
                    case "btload":
                        LoadImageFile();
                        break;
                    case "btsave":
                        SaveImageFile();
                        break;
                    case "btmge":
                        flgManager = true;
                        OpenNewSearchForm();
                        break;
                    case "btreport":
                        flgReportTo = true;
                        OpenNewSearchForm();
                        break;
                    case "btrel":
                        OpenRelativeForm();
                        break;
                    case "btpexp":
                        OpenPastExpForm();
                        break;
                    case "btqual":
                        OpenQualificationForm();
                        break;
                    case "btedu":
                        OpenEducationForm();
                        break;
                    //Branch Assingment 05-May-15
                    case "btbrnch":
                        flgManager = true;
                        // OpenNewBranchesForm();
                        break;
                    case "btattch":
                        OpenNewAttachmentForm();
                        break;
                    case "tbsalary":
                        if (Program.objHrmsUI.isSuperUser == true && flgOnLoad)
                        {
                            ilblBasicSalary.Visible = true;
                            itxtBasicSalary.Visible = true;
                        }
                        else
                        {
                            ilblBasicSalary.Visible = false;
                            itxtBasicSalary.Visible = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == BoFormMode.fm_ADD_MODE || oForm.Mode == BoFormMode.fm_UPDATE_MODE)
                    {
                        if (!ValidateData())
                        {
                            BubbleEvent = false;
                        }
                    }
                    break;

            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmployeeCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmployeeCode.Value)
            {
                return;
            }
            if (Program.EmpID != txtEmployeeCode.Value.Trim() && !flgManager && !flgReportTo)
            {
                SetEmpValues();
                oForm.Items.Item("tbaddress").Click();
            }
            if (Program.EmpID != txtEmployeeCode.Value.Trim() && flgManager && !flgReportTo)
            {
                //Switching off
                txtManager.Value = Program.EmpID;
                Program.EmpID = string.Empty;
                flgManager = false;
            }
            if (Program.EmpID != txtEmployeeCode.Value.Trim() && flgReportTo && !flgManager)
            {
                //Switching off
                txtReportTo.Value = Program.EmpID;
                Program.EmpID = string.Empty;
                flgReportTo = false;
            }
            //base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            //SetEmpValues();
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            //switch (pVal.ItemUID)
            //{
            //    case "txthempcde":
            //        //LoadSelectedData(txtEmployeeCode.Value);
            //        break;
            //    default:
            //        break;
            //}

        }

        public override void etFormBeforClose(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormBeforClose(ref pVal, ref BubbleEvent);
            try
            {
                if (BubbleEvent == true)
                {
                    if (flgEmpFilter)
                        BubbleEvent = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            flgFormMode = true;
            InitiallizeDocument();
            itxtEmployeeCode.Enabled = true;
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            //btnMain.Caption = "Add";
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();

            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE;
            itxtEmployeeCode.Enabled = true;
            InitiallizeDocument();
            //txtEmployeeCode.Value = "";
            //btnMain.Caption = "Find";

            doSubmit();
            oForm.Items.Item("tbpm").Click();
            // oForm.Items.Item("tbaddress").Click();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        public bool ComboLoaded(SAPbouiCOM.ComboBox oCombo, string cases)
        {
            return oCombo.ValidValues.Count > 1;
        }

        public override void etAfterGetFocus(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterGetFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "cbjt":
                    if (!ComboLoaded(cbJobTitle, pVal.ItemUID)) FillJobTitleCombo(cbJobTitle);
                    break;
                case "cbloc":
                    if (!ComboLoaded(cbLocation, pVal.ItemUID)) FillLocationsCombo(cbLocation);
                    break;
                case "cbhposi":
                    if (!ComboLoaded(cbPosition, pVal.ItemUID)) FillPositionCombo(cbPosition);
                    break;
                case "cbhdept":
                    if (!ComboLoaded(cbDepartment, pVal.ItemUID)) FillDepartmentCombo(cbDepartment);
                    break;
                case "cbhbrnch":
                    if (!ComboLoaded(cbBranch, pVal.ItemUID)) FillBranchCombo(cbBranch);
                    break;
                case "cbdesig":
                    if (!ComboLoaded(cbDesignation, pVal.ItemUID)) FillDesignationCombo(cbDesignation);
                    break;
                case "cbsbo":
                    if (!ComboLoaded(cbSBOLinkID, pVal.ItemUID)) FillSboUsrCombo(cbSBOLinkID);
                    break;
                case "cbstate":
                    if (!ComboLoaded(cbHomeState, pVal.ItemUID)) FillStatesCombo();
                    break;
                case "cbstate1":
                    if (!ComboLoaded(cbWorkState, pVal.ItemUID)) FillStatesCombo();
                    break;
                case "cbpcstate":
                    if (!ComboLoaded(cbPriCntState, pVal.ItemUID)) FillStatesCombo();
                    break;
                case "cbscstate":
                    if (!ComboLoaded(cbSecCntState, pVal.ItemUID)) FillStatesCombo();
                    break;
                case "cbcountry":
                    if (!ComboLoaded(cbHomeCountry, pVal.ItemUID)) FillCountryCombo();
                    break;
                case "cbcountry1":
                    if (!ComboLoaded(cbWorkCountry, pVal.ItemUID)) FillCountryCombo();
                    break;
                case "cbpccoutry":
                    if (!ComboLoaded(cbPriCntCountry, pVal.ItemUID)) FillCountryCombo();
                    break;
                case "cbsccntry":
                    if (!ComboLoaded(cbSecCntCountry, pVal.ItemUID)) FillCountryCombo();
                    break;
                case "cbohem":
                    if (!ComboLoaded(cbOhemUser, pVal.ItemUID)) FillOHEMUserCombo(cbOhemUser); ;
                    break;
                case "cbgender":
                    if (!ComboLoaded(cbGender, pVal.ItemUID)) FillGenderCombo(cbGender);
                    break;
                case "cbcc":
                    if (!ComboLoaded(cbCostCenter, pVal.ItemUID)) FillCostCenterCombo(cbCostCenter); ;
                    break;
                case "cbProject":
                    if (!ComboLoaded(cbProject, pVal.ItemUID)) FillProjectCombo(cbProject);
                    break;
                case "cbreligion":
                    if (!ComboLoaded(cbReligion, pVal.ItemUID)) FillReligionCombo(cbReligion);
                    break;
                case "cbmartial":
                    if (!ComboLoaded(cbMartial, pVal.ItemUID)) FillMartialCombo(cbMartial);
                    break;
                case "cbblood":
                    if (!ComboLoaded(cbBloodGroup, pVal.ItemUID)) FillBloodGroupCombo(cbBloodGroup);
                    break;
                case "cbcat":
                    if (!ComboLoaded(cmbCategory, pVal.ItemUID)) FillComboCategory(cmbCategory);
                    break;
                case "cbsubcat":
                    if (!ComboLoaded(cmbSubCategory, pVal.ItemUID)) FillComboSubCategory(cmbSubCategory);
                    break;
                case "cbtran":
                    if (!ComboLoaded(cmbTransport, pVal.ItemUID)) FillLovList(cmbTransport, "TransMod");
                    break;
                case "cbrec":
                    if (!ComboLoaded(cmbRecruitment, pVal.ItemUID)) FillLovList(cmbRecruitment, "RecruitMod");
                    break;
                case "cbins":
                    if (!ComboLoaded(cmbInsuranceCategory, pVal.ItemUID)) FillLovList(cmbInsuranceCategory, "InsuranceMod");
                    break;
                case "cbslrycur":
                    if (!ComboLoaded(cbSalaryCurrency, pVal.ItemUID)) FillLovList(cbSalaryCurrency, "SalaryCurrency");
                    break;
                case "cbpaymod":
                    if (!ComboLoaded(cbPaymentMode, pVal.ItemUID)) FillLovList(cbPaymentMode, "PaymentMode");
                    break;
                case "cbacctype":
                    if (!ComboLoaded(cbAccountType, pVal.ItemUID)) FillLovList(cbAccountType, "AccountType");
                    break;
                case "cType":
                    if (!ComboLoaded(cbcontractType, pVal.ItemUID)) FillContractTypeCombo();
                    break;
                case "cbPC":
                    if (!ComboLoaded(cmbProfitCenter, pVal.ItemUID)) FillProfitCenterCombo(cmbProfitCenter);
                    break;
                case "cbgratuity":
                    if (!ComboLoaded(cbGratuity, pVal.ItemUID)) FillComboGratuity(cbGratuity);
                    break;
                case "cbots":
                    if (!ComboLoaded(cmbOTSlabs, pVal.ItemUID)) FillComboOTSlab(cmbOTSlabs);
                    break;
                case "cbsday":
                    if (!ComboLoaded(cmbShiftDaySlabs, pVal.ItemUID)) FillComboShiftDaysSlab(cmbShiftDaySlabs);
                    break;
                case "cbbonus":
                    if (!ComboLoaded(cbBonusSlabs, pVal.ItemUID)) FillBonusCombo(cbBonusSlabs);
                    break;
                case "cbpayroll":
                    if (!ComboLoaded(cbPayroll, pVal.ItemUID)) FillPayrollCombo(cbPayroll);
                    break;
                case "cbdim1":
                    if (!ComboLoaded(cbDimension1, pVal.ItemUID)) FillComboDimension1(cbDimension1);
                    break;
                case "cbdim2":
                    if (!ComboLoaded(cbDimension2, pVal.ItemUID)) FillComboDimension2(cbDimension2);
                    break;
                case "cbdim3":
                    if (!ComboLoaded(cbDimension3, pVal.ItemUID)) FillComboDimension3(cbDimension3);
                    break;
                case "cbdim4":
                    if (!ComboLoaded(cbDimension4, pVal.ItemUID)) FillComboDimension4(cbDimension4);
                    break;
                case "cbdim5":
                    if (!ComboLoaded(cbDimension5, pVal.ItemUID)) FillComboDimension5(cbDimension5);
                    break;
                case "cbdfd":
                    if (!ComboLoaded(cmbEmpOffDay, pVal.ItemUID)) FillLovList(cmbEmpOffDay, "OFFDAYS");
                    break;
                case "cbAttAlw":
                    if (!ComboLoaded(cbAttendanceAllowance, pVal.ItemUID)) FillAttendanceAllowanceCombo(cbAttendanceAllowance);
                    break;
            }
        }
        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            //Each Item should be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the control object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */
            try
            {
                oApplication.StatusBar.SetText("Please wait until Employee master loaded.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                #region Header
                flgEmpFilter = true;
                oForm.DefButton = "1";
                btnMain = oForm.Items.Item("1").Specific;
                ibtnMain = oForm.Items.Item("1");

                btnSyncTOSBO = oForm.Items.Item("btSBO").Specific;
                ibtnSyncTOSBO = oForm.Items.Item("btSBO");
                //btnMain.Caption = "Add";
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                //btnCancel = oForm.Items.Item("2").Specific;

                pctBox = oForm.Items.Item("picbox").Specific;


                tbAddress = oForm.Items.Item("tbaddress").Specific;
                tbEmergencyDetail = oForm.Items.Item("tbed").Specific;
                tbPersonal = oForm.Items.Item("tbpersonal").Specific;
                tbSalary = oForm.Items.Item("tbsalary").Specific;
                tbAbsence = oForm.Items.Item("tbabsence").Specific;
                tbCommunication = oForm.Items.Item("tbcom").Specific;
                tbClassification = oForm.Items.Item("tbclass").Specific;

                //tbRelative = oForm.Items.Item("tbrelative").Specific;
                //tbQualification = oForm.Items.Item("tbqlf").Specific;
                //tbPastExperiance = oForm.Items.Item("tbpstexp").Specific;
                //tbEducation = oForm.Items.Item("tbedu").Specific;
                tbArabic = oForm.Items.Item("flArabic").Specific;
                itbArabic = oForm.Items.Item("flArabic");
                //TODO: Arabic Check
                if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                {
                    itbArabic.Visible = true;
                }
                else
                {
                    itbArabic.Visible = false;
                }

                //Header Area
                oForm.DataSources.UserDataSources.Add("txthfname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtFirstName = oForm.Items.Item("txthfname").Specific;
                itxtFirstName = oForm.Items.Item("txthfname");
                txtFirstName.DataBind.SetBound(true, "", "txthfname");

                oForm.DataSources.UserDataSources.Add("txthmname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtMiddleName = oForm.Items.Item("txthmname").Specific;
                txtMiddleName.DataBind.SetBound(true, "", "txthmname");

                oForm.DataSources.UserDataSources.Add("txthlname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtLastName = oForm.Items.Item("txthlname").Specific;
                txtLastName.DataBind.SetBound(true, "", "txthlname");

                txtUserCode = oForm.Items.Item("txusercode").Specific;
                itxtUserCode = oForm.Items.Item("txusercode");
                oForm.DataSources.UserDataSources.Add("txusercode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtUserCode.DataBind.SetBound(true, "", "txusercode");

                oForm.DataSources.UserDataSources.Add("txpsw", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtPassword = oForm.Items.Item("txpsw").Specific;
                txtPassword.DataBind.SetBound(true, "", "txpsw");

                cbJobTitle = oForm.Items.Item("cbjt").Specific;
                oForm.DataSources.UserDataSources.Add("cbjt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbJobTitle.DataBind.SetBound(true, "", "cbjt");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLocation.DataBind.SetBound(true, "", "cbloc");

                cbPosition = oForm.Items.Item("cbhposi").Specific;
                oForm.DataSources.UserDataSources.Add("cbhposi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPosition.DataBind.SetBound(true, "", "cbhposi");

                cbDepartment = oForm.Items.Item("cbhdept").Specific;
                oForm.DataSources.UserDataSources.Add("cbhdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDepartment.DataBind.SetBound(true, "", "cbhdept");

                cbBranch = oForm.Items.Item("cbhbrnch").Specific;
                oForm.DataSources.UserDataSources.Add("cbhbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbBranch.DataBind.SetBound(true, "", "cbhbrnch");

                txtManager = oForm.Items.Item("txmanager").Specific;
                itxtManager = oForm.Items.Item("txmanager");
                oForm.DataSources.UserDataSources.Add("txmanager", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtManager.DataBind.SetBound(true, "", "txmanager");

                cbDesignation = oForm.Items.Item("cbdesig").Specific;
                oForm.DataSources.UserDataSources.Add("cbdesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDesignation.DataBind.SetBound(true, "", "cbdesig");

                oForm.DataSources.UserDataSources.Add("txthempcde", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmployeeCode = oForm.Items.Item("txthempcde").Specific;
                itxtEmployeeCode = oForm.Items.Item("txthempcde");
                txtEmployeeCode.DataBind.SetBound(true, "", "txthempcde");

                txtInitials = oForm.Items.Item("txhinitial").Specific;
                oForm.DataSources.UserDataSources.Add("txhinitial", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtInitials.DataBind.SetBound(true, "", "txhinitial");

                txtNamePrefix = oForm.Items.Item("txhnprefix").Specific;
                oForm.DataSources.UserDataSources.Add("txhnprefix", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtNamePrefix.DataBind.SetBound(true, "", "txhnprefix");

                txtOfficePhn = oForm.Items.Item("txhoph").Specific;
                oForm.DataSources.UserDataSources.Add("txhoph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtOfficePhn.DataBind.SetBound(true, "", "txhoph");

                txtExtention = oForm.Items.Item("txhext").Specific;
                oForm.DataSources.UserDataSources.Add("txhext", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtExtention.DataBind.SetBound(true, "", "txhext");

                txtMobilePhn = oForm.Items.Item("txhmph").Specific;
                oForm.DataSources.UserDataSources.Add("txhmph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtMobilePhn.DataBind.SetBound(true, "", "txhmph");

                txtHomePhn = oForm.Items.Item("txhhph").Specific;
                oForm.DataSources.UserDataSources.Add("txhhph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtHomePhn.DataBind.SetBound(true, "", "txhhph");

                txtPager = oForm.Items.Item("txhpager").Specific;
                oForm.DataSources.UserDataSources.Add("txhpager", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtPager.DataBind.SetBound(true, "", "txhpager");

                txtFax = oForm.Items.Item("txhfax").Specific;
                oForm.DataSources.UserDataSources.Add("txhfax", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 15);
                txtFax.DataBind.SetBound(true, "", "txhfax");

                txtEmail = oForm.Items.Item("txhemail").Specific;
                oForm.DataSources.UserDataSources.Add("txhemail", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmail.DataBind.SetBound(true, "", "txhemail");

                cbSBOLinkID = oForm.Items.Item("cbsbo").Specific;
                oForm.DataSources.UserDataSources.Add("cbsbo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbSBOLinkID.DataBind.SetBound(true, "", "cbsbo");

                chkActiveEmployee = oForm.Items.Item("chkhaemp").Specific;
                oForm.DataSources.UserDataSources.Add("chkhaemp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkActiveEmployee.DataBind.SetBound(true, "", "chkhaemp");
                chkActiveEmployee.Checked = true;

                chkOTApplicable = oForm.Items.Item("flgOT").Specific;
                oForm.DataSources.UserDataSources.Add("flgOT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkOTApplicable.DataBind.SetBound(true, "", "flgOT");
                chkOTApplicable.Checked = true;

                flgTax = oForm.Items.Item("flgTax").Specific;
                oForm.DataSources.UserDataSources.Add("flgTax", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgTax.DataBind.SetBound(true, "", "flgTax");
                flgTax.Checked = true;

                chkSup = oForm.Items.Item("chkSup").Specific;
                oForm.DataSources.UserDataSources.Add("chkSup", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkSup.DataBind.SetBound(true, "", "chkSup");
                chkSup.Checked = false;

                chkPerPiece = oForm.Items.Item("chkpp").Specific;
                oForm.DataSources.UserDataSources.Add("chkpp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkPerPiece.DataBind.SetBound(true, "", "chkpp");
                chkPerPiece.Checked = false;

                #endregion

                oForm.DataSources.UserDataSources.Add("txthftname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtFatherName = oForm.Items.Item("txthftname").Specific;
                txtFatherName.DataBind.SetBound(true, "", "txthftname");

                #region Address Tab
                oForm.PaneLevel = 1;

                txtHomeStreet = oForm.Items.Item("txstreet").Specific;
                oForm.DataSources.UserDataSources.Add("txstreet", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeStreet.DataBind.SetBound(true, "", "txstreet");

                txtHomeStreetNo = oForm.Items.Item("txstreetno").Specific;
                oForm.DataSources.UserDataSources.Add("txstreetno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeStreetNo.DataBind.SetBound(true, "", "txstreetno");

                txtHomeBlock = oForm.Items.Item("txblock").Specific;
                oForm.DataSources.UserDataSources.Add("txblock", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeBlock.DataBind.SetBound(true, "", "txblock");

                txtHomeBuilding = oForm.Items.Item("txbuilding").Specific;
                oForm.DataSources.UserDataSources.Add("txbuilding", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeBuilding.DataBind.SetBound(true, "", "txbuilding");

                txtHomeZip = oForm.Items.Item("txzipcode").Specific;
                oForm.DataSources.UserDataSources.Add("txzipcode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeZip.DataBind.SetBound(true, "", "txzipcode");

                txtHomeCity = oForm.Items.Item("txcity").Specific;
                oForm.DataSources.UserDataSources.Add("txcity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeCity.DataBind.SetBound(true, "", "txcity");

                cbHomeState = oForm.Items.Item("cbstate").Specific;
                oForm.DataSources.UserDataSources.Add("cbstate", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbHomeState.DataBind.SetBound(true, "", "cbstate");

                cbHomeCountry = oForm.Items.Item("cbcountry").Specific;
                oForm.DataSources.UserDataSources.Add("cbcountry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbHomeCountry.DataBind.SetBound(true, "", "cbcountry");

                txtWorkStreet = oForm.Items.Item("txstreet1").Specific;
                oForm.DataSources.UserDataSources.Add("txstreet1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkStreet.DataBind.SetBound(true, "", "txstreet1");

                txtWorkStreetNo = oForm.Items.Item("txstretno1").Specific;
                oForm.DataSources.UserDataSources.Add("txstretno1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkStreetNo.DataBind.SetBound(true, "", "txstretno1");

                txtWorkBlock = oForm.Items.Item("txblock1").Specific;
                oForm.DataSources.UserDataSources.Add("txblock1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkBlock.DataBind.SetBound(true, "", "txblock1");

                txtWorkBuilding = oForm.Items.Item("txbuildng1").Specific;
                oForm.DataSources.UserDataSources.Add("txbuildng1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkBuilding.DataBind.SetBound(true, "", "txbuildng1");

                txtWorkZip = oForm.Items.Item("txzipcode1").Specific;
                oForm.DataSources.UserDataSources.Add("txzipcode1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkZip.DataBind.SetBound(true, "", "txzipcode1");

                txtWorkCity = oForm.Items.Item("txcity1").Specific;
                oForm.DataSources.UserDataSources.Add("txcity1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkCity.DataBind.SetBound(true, "", "txcity1");

                cbWorkState = oForm.Items.Item("cbstate1").Specific;
                oForm.DataSources.UserDataSources.Add("cbstate1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbWorkState.DataBind.SetBound(true, "", "cbstate1");

                cbWorkCountry = oForm.Items.Item("cbcountry1").Specific;
                oForm.DataSources.UserDataSources.Add("cbcountry1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbWorkCountry.DataBind.SetBound(true, "", "cbcountry1");

                txtHomeBranches = oForm.Items.Item("txtbrnch").Specific;
                oForm.DataSources.UserDataSources.Add("txtbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeBranches.DataBind.SetBound(true, "", "txtbrnch");

                txtCompanyResidence = oForm.Items.Item("txcomres").Specific;
                itxtCompanyResidence = oForm.Items.Item("txcomres");
                oForm.DataSources.UserDataSources.Add("txcomres", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 200);
                txtCompanyResidence.DataBind.SetBound(true, "", "txcomres");

                chkCompanyResidence = oForm.Items.Item("chcomres").Specific;
                ichkCompanyResidence = oForm.Items.Item("chcomres");
                oForm.DataSources.UserDataSources.Add("chcomres", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkCompanyResidence.DataBind.SetBound(true, "", "chcomres");
                #endregion

                #region Emergency Contact

                txtPriCntName = oForm.Items.Item("txpcname").Specific;
                oForm.DataSources.UserDataSources.Add("txpcname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPriCntName.DataBind.SetBound(true, "", "txpcname");

                txtPriCntRelation = oForm.Items.Item("txpcrlt").Specific;
                oForm.DataSources.UserDataSources.Add("txpcrlt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPriCntRelation.DataBind.SetBound(true, "", "txpcrlt");

                txtPriCntNoLandLine = oForm.Items.Item("txpccnln").Specific;
                oForm.DataSources.UserDataSources.Add("txpccnln", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPriCntNoLandLine.DataBind.SetBound(true, "", "txpccnln");

                txtPriCntNoMobile = oForm.Items.Item("txpccnm").Specific;
                oForm.DataSources.UserDataSources.Add("txpccnm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPriCntNoMobile.DataBind.SetBound(true, "", "txpccnm");

                txtPriCntAddress = oForm.Items.Item("txpcadr").Specific;
                oForm.DataSources.UserDataSources.Add("txpcadr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPriCntAddress.DataBind.SetBound(true, "", "txpcadr");

                txtPriCntCity = oForm.Items.Item("txpccity").Specific;
                oForm.DataSources.UserDataSources.Add("txpccity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPriCntCity.DataBind.SetBound(true, "", "txpccity");

                cbPriCntState = oForm.Items.Item("cbpcstate").Specific;
                oForm.DataSources.UserDataSources.Add("cbpcstate", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbPriCntState.DataBind.SetBound(true, "", "cbpcstate");

                cbPriCntCountry = oForm.Items.Item("cbpcoutry").Specific;
                oForm.DataSources.UserDataSources.Add("cbpcoutry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbPriCntCountry.DataBind.SetBound(true, "", "cbpcoutry");

                txtSecCntName = oForm.Items.Item("txscname").Specific;
                oForm.DataSources.UserDataSources.Add("txscname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSecCntName.DataBind.SetBound(true, "", "txscname");

                txtSecCntRelation = oForm.Items.Item("txscrlt").Specific;
                oForm.DataSources.UserDataSources.Add("txscrlt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSecCntRelation.DataBind.SetBound(true, "", "txscrlt");

                txtSecCntNoLandLine = oForm.Items.Item("txsccnl").Specific;
                oForm.DataSources.UserDataSources.Add("txsccnl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSecCntNoLandLine.DataBind.SetBound(true, "", "txsccnl");

                txtSecCntNoMobile = oForm.Items.Item("txscnm").Specific;
                oForm.DataSources.UserDataSources.Add("txscnm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSecCntNoMobile.DataBind.SetBound(true, "", "txscnm");

                txtSecCntAddress = oForm.Items.Item("txscadr").Specific;
                oForm.DataSources.UserDataSources.Add("txscadr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSecCntAddress.DataBind.SetBound(true, "", "txscadr");

                txtSecCntCity = oForm.Items.Item("txsccity").Specific;
                oForm.DataSources.UserDataSources.Add("txsccity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSecCntCity.DataBind.SetBound(true, "", "txsccity");

                cbSecCntState = oForm.Items.Item("cbscstate").Specific;
                oForm.DataSources.UserDataSources.Add("cbscstate", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbSecCntState.DataBind.SetBound(true, "", "cbscstate");

                cbSecCntCountry = oForm.Items.Item("cbsccntry").Specific;
                oForm.DataSources.UserDataSources.Add("cbsccntry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbSecCntCountry.DataBind.SetBound(true, "", "cbsccntry");

                #endregion

                #region Personal

                cbMartial = oForm.Items.Item("cbmartial").Specific;
                oForm.DataSources.UserDataSources.Add("cbmartial", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbMartial.DataBind.SetBound(true, "", "cbmartial");

                cbReligion = oForm.Items.Item("cbreligion").Specific;
                oForm.DataSources.UserDataSources.Add("cbreligion", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbReligion.DataBind.SetBound(true, "", "cbreligion");

                txtMotherName = oForm.Items.Item("txmother").Specific;
                oForm.DataSources.UserDataSources.Add("txmother", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtMotherName.DataBind.SetBound(true, "", "txmother");

                txtSSNumber = oForm.Items.Item("txssno").Specific;
                oForm.DataSources.UserDataSources.Add("txssno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtSSNumber.DataBind.SetBound(true, "", "txssno");

                txtUnionMemberShip = oForm.Items.Item("txums").Specific;
                oForm.DataSources.UserDataSources.Add("txums", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtUnionMemberShip.DataBind.SetBound(true, "", "txums");

                txtUnionMemberShipNo = oForm.Items.Item("txumsno").Specific;
                oForm.DataSources.UserDataSources.Add("txumsno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtUnionMemberShipNo.DataBind.SetBound(true, "", "txumsno");

                txtNationality = oForm.Items.Item("txnation").Specific;
                oForm.DataSources.UserDataSources.Add("txnation", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtNationality.DataBind.SetBound(true, "", "txnation");

                cbOhemUser = oForm.Items.Item("cbohem").Specific;
                oForm.DataSources.UserDataSources.Add("cbohem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbOhemUser.DataBind.SetBound(true, "", "cbohem");

                txtIDCardNo = oForm.Items.Item("txidno").Specific;
                oForm.DataSources.UserDataSources.Add("txidno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtIDCardNo.DataBind.SetBound(true, "", "txidno");

                txtIDDtOfIssue = oForm.Items.Item("txisudt").Specific;
                oForm.DataSources.UserDataSources.Add("txisudt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtIDDtOfIssue.DataBind.SetBound(true, "", "txisudt");
                //txtIDDtOfIssue.Value = DateTime.Now.ToString("yyyyMMdd");

                txtIDPlaceOfIssue = oForm.Items.Item("txidplcisu").Specific;
                oForm.DataSources.UserDataSources.Add("txidplcisu", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtIDPlaceOfIssue.DataBind.SetBound(true, "", "txidplcisu");

                txtIDIssuedBy = oForm.Items.Item("txidisuby").Specific;
                oForm.DataSources.UserDataSources.Add("txidisuby", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtIDIssuedBy.DataBind.SetBound(true, "", "txidisuby");

                txtIDExpiryDate = oForm.Items.Item("txidexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txidexpdt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtIDExpiryDate.DataBind.SetBound(true, "", "txidexpdt");
                //txtIDExpiryDate.Value = DateTime.Now.ToString("yyyyMMdd");

                txtPassportNo = oForm.Items.Item("txpssno").Specific;
                oForm.DataSources.UserDataSources.Add("txpssno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPassportNo.DataBind.SetBound(true, "", "txpssno");

                txtPassportDateofIssue = oForm.Items.Item("txpssdt").Specific;
                oForm.DataSources.UserDataSources.Add("txpssdt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtPassportDateofIssue.DataBind.SetBound(true, "", "txpssdt");
                //txtPassportDateofIssue.Value = DateTime.Now.ToString("yyyyMMdd");

                txtPassportExpiry = oForm.Items.Item("txpssexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txpssexpdt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtPassportExpiry.DataBind.SetBound(true, "", "txpssexpdt");
                //txtPassportExpiry.Value = DateTime.Now.ToString("yyyyMMdd");

                txtIncomeTax = oForm.Items.Item("txicnno").Specific;
                oForm.DataSources.UserDataSources.Add("txicnno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtIncomeTax.DataBind.SetBound(true, "", "txicnno");

                cbCostCenter = oForm.Items.Item("cbcc").Specific;
                oForm.DataSources.UserDataSources.Add("cbcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbCostCenter.DataBind.SetBound(true, "", "cbcc");

                cbProject = oForm.Items.Item("cbProject").Specific;
                oForm.DataSources.UserDataSources.Add("cbProject", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbProject.DataBind.SetBound(true, "", "cbProject");

                cbGender = oForm.Items.Item("cbgender").Specific;
                oForm.DataSources.UserDataSources.Add("cbgender", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbGender.DataBind.SetBound(true, "", "cbgender");

                cbGratuity = oForm.Items.Item("cbgratuity").Specific;
                oForm.DataSources.UserDataSources.Add("cbgratuity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbGratuity.DataBind.SetBound(true, "", "cbgratuity");

                cmbOTSlabs = oForm.Items.Item("cbots").Specific;
                oForm.DataSources.UserDataSources.Add("cbots", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cmbOTSlabs.DataBind.SetBound(true, "", "cbots");

                cmbCategory = oForm.Items.Item("cbcat").Specific;
                icmbCategory = oForm.Items.Item("cbcat");
                oForm.DataSources.UserDataSources.Add("cbcat", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cmbCategory.DataBind.SetBound(true, "", "cbcat");

                cmbSubCategory = oForm.Items.Item("cbsubcat").Specific;
                icmbSubCategory = oForm.Items.Item("cbsubcat");
                oForm.DataSources.UserDataSources.Add("cbsubcat", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cmbSubCategory.DataBind.SetBound(true, "", "cbsubcat");

                txtDOB = oForm.Items.Item("txtdob").Specific;
                oForm.DataSources.UserDataSources.Add("txtdob", SAPbouiCOM.BoDataType.dt_DATE);
                txtDOB.DataBind.SetBound(true, "", "txtdob");

                txtTermination = oForm.Items.Item("txter").Specific;
                oForm.DataSources.UserDataSources.Add("txter", SAPbouiCOM.BoDataType.dt_DATE);
                txtTermination.DataBind.SetBound(true, "", "txter");

                txtResignation = oForm.Items.Item("txresign").Specific;
                oForm.DataSources.UserDataSources.Add("txresign", SAPbouiCOM.BoDataType.dt_DATE);
                txtResignation.DataBind.SetBound(true, "", "txresign");

                cmbTransport = oForm.Items.Item("cbtran").Specific;
                icmbTransport = oForm.Items.Item("cbtran");
                oForm.DataSources.UserDataSources.Add("cbtran", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                cmbTransport.DataBind.SetBound(true, "", "cbtran");

                cmbRecruitment = oForm.Items.Item("cbrec").Specific;
                icmbRecruitment = oForm.Items.Item("cbrec");
                oForm.DataSources.UserDataSources.Add("cbrec", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                cmbRecruitment.DataBind.SetBound(true, "", "cbrec");

                cmbInsuranceCategory = oForm.Items.Item("cbins").Specific;
                icmbInsuranceCategory = oForm.Items.Item("cbins");
                oForm.DataSources.UserDataSources.Add("cbins", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 50);
                cmbInsuranceCategory.DataBind.SetBound(true, "", "cbins");

                #endregion

                #region Salary

                ilblBasicSalary = oForm.Items.Item("174");

                txtBasicSalary = oForm.Items.Item("txbsslry").Specific;
                oForm.DataSources.UserDataSources.Add("txbsslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txbsslry");
                itxtBasicSalary = oForm.Items.Item("txbsslry");

                txtGrossComputed = oForm.Items.Item("txgsc").Specific;
                oForm.DataSources.UserDataSources.Add("txgsc", SAPbouiCOM.BoDataType.dt_SUM);
                txtGrossComputed.DataBind.SetBound(true, "", "txgsc");
                itxtGrossComputed = oForm.Items.Item("txgsc");

                txGosi = oForm.Items.Item("txGosi").Specific;
                oForm.DataSources.UserDataSources.Add("txGosi", SAPbouiCOM.BoDataType.dt_SUM);
                txGosi.DataBind.SetBound(true, "", "txGosi");

                txGosiV = oForm.Items.Item("txGosiV").Specific;
                oForm.DataSources.UserDataSources.Add("txGosiV", SAPbouiCOM.BoDataType.dt_SUM);
                txGosiV.DataBind.SetBound(true, "", "txGosiV");

                cbcontractType = oForm.Items.Item("cType").Specific;
                oForm.DataSources.UserDataSources.Add("cType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbcontractType.DataBind.SetBound(true, "", "cType");

                cmbProfitCenter = oForm.Items.Item("cbPC").Specific;
                oForm.DataSources.UserDataSources.Add("cbPC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cmbProfitCenter.DataBind.SetBound(true, "", "cbPC");

                txtEmpCalendar = oForm.Items.Item("txempcal").Specific;
                oForm.DataSources.UserDataSources.Add("txempcal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmpCalendar.DataBind.SetBound(true, "", "txempcal");

                //string query = "SELECT \"HldCode\" FROM \"OHLD\"";

                //Program.objHrmsUI.addFms("frm_EmpMst", "txempcal", "-1", query);

                cbSalaryCurrency = oForm.Items.Item("cbslrycur").Specific;
                oForm.DataSources.UserDataSources.Add("cbslrycur", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbSalaryCurrency.DataBind.SetBound(true, "", "cbslrycur");

                cbPaymentMode = oForm.Items.Item("cbpaymod").Specific;
                oForm.DataSources.UserDataSources.Add("cbpaymod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbPaymentMode.DataBind.SetBound(true, "", "cbpaymod");

                txtAccountTitle = oForm.Items.Item("txacctitle").Specific;
                oForm.DataSources.UserDataSources.Add("txacctitle", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtAccountTitle.DataBind.SetBound(true, "", "txacctitle");

                txtBankName = oForm.Items.Item("txbname").Specific;
                oForm.DataSources.UserDataSources.Add("txbname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtBankName.DataBind.SetBound(true, "", "txbname");

                //query = "SELECT \"BankCode\", \"BankName\" FROM \"ODSC\"";

                //Program.objHrmsUI.addFms("frm_EmpMst", "txbname", "-1", query);

                txtBankBranch = oForm.Items.Item("txbrnch").Specific;
                oForm.DataSources.UserDataSources.Add("txbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtBankBranch.DataBind.SetBound(true, "", "txbrnch");

                //query = "SELECT \"Branch\", \"Account\", \"BankCode\" FROM \"DSC1\"";

                //Program.objHrmsUI.addFms("frm_EmpMst", "txbrnch", "-1", query);

                txtAccountNo = oForm.Items.Item("txaccno").Specific;
                oForm.DataSources.UserDataSources.Add("txaccno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtAccountNo.DataBind.SetBound(true, "", "txaccno");

                cbAccountType = oForm.Items.Item("cbacctype").Specific;
                oForm.DataSources.UserDataSources.Add("cbacctype", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbAccountType.DataBind.SetBound(true, "", "cbacctype");

                txtEffectiveDate = oForm.Items.Item("txeffdt").Specific;
                oForm.DataSources.UserDataSources.Add("txeffdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtEffectiveDate.DataBind.SetBound(true, "", "txeffdt");
                //txtEffectiveDate.Value = DateTime.Now.ToString("yyyyMMdd");

                txtPercentage = oForm.Items.Item("txper").Specific;
                oForm.DataSources.UserDataSources.Add("txper", SAPbouiCOM.BoDataType.dt_SUM);
                txtPercentage.DataBind.SetBound(true, "", "txper");

                txtDateOfJoining = oForm.Items.Item("txdoj").Specific;
                oForm.DataSources.UserDataSources.Add("txdoj", SAPbouiCOM.BoDataType.dt_DATE);
                txtDateOfJoining.DataBind.SetBound(true, "", "txdoj");
                //txtDateOfJoining.Value = DateTime.Now.ToString("yyyyMMdd");

                cbBloodGroup = oForm.Items.Item("cbblood").Specific;
                oForm.DataSources.UserDataSources.Add("cbblood", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbBloodGroup.DataBind.SetBound(true, "", "cbblood");

                cbShift = oForm.Items.Item("cbShift").Specific;
                oForm.DataSources.UserDataSources.Add("cbShift", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbShift.DataBind.SetBound(true, "", "cbShift");

                cmbShiftDaySlabs = oForm.Items.Item("cbsday").Specific;
                oForm.DataSources.UserDataSources.Add("cbsday", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cmbShiftDaySlabs.DataBind.SetBound(true, "", "cbsday");

                cbBonusSlabs = oForm.Items.Item("cbbonus").Specific;
                oForm.DataSources.UserDataSources.Add("cbbonus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbBonusSlabs.DataBind.SetBound(true, "", "cbbonus");

                chkEmailSalarySlip = oForm.Items.Item("chkEmail").Specific;
                oForm.DataSources.UserDataSources.Add("chkEmail", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkEmailSalarySlip.DataBind.SetBound(true, "", "chkEmail");

                chkSandwichLeaves = oForm.Items.Item("chsl").Specific;
                oForm.DataSources.UserDataSources.Add("chsl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkSandwichLeaves.DataBind.SetBound(true, "", "chsl");

                txtAllowedAdvance = oForm.Items.Item("txaa").Specific;
                itxtAllowedAdvance = oForm.Items.Item("txaa");
                oForm.DataSources.UserDataSources.Add("txaa", SAPbouiCOM.BoDataType.dt_PERCENT);
                txtAllowedAdvance.DataBind.SetBound(true, "", "txaa");

                //chblklst
                chkBlackListed = oForm.Items.Item("chblklst").Specific;
                ichkBlackListed = oForm.Items.Item("chblklst");
                oForm.DataSources.UserDataSources.Add("chblklst", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkBlackListed.DataBind.SetBound(true, "", "chblklst");
                //chkEmailSalarySlip.Checked = true;
                HideBasicSalary();
                //oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                //strOut = oRecSet.Fields.Item("U_PayrollType").Value;

                #endregion

                #region Payroll

                cbPayroll = oForm.Items.Item("cbpayroll").Specific;
                oForm.DataSources.UserDataSources.Add("cbpayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                cbPayroll.DataBind.SetBound(true, "", "cbpayroll");

                cmbEmpOffDay = oForm.Items.Item("cbdfd").Specific;
                icmbEmpOffDay = oForm.Items.Item("cbdfd");
                oForm.DataSources.UserDataSources.Add("cbdfd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                cmbEmpOffDay.DataBind.SetBound(true, "", "cbdfd");

                cbDimension1 = oForm.Items.Item("cbdim1").Specific;
                icbDimension1 = oForm.Items.Item("cbdim1");
                oForm.DataSources.UserDataSources.Add("cbdim1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                cbDimension1.DataBind.SetBound(true, "", "cbdim1");


                cbDimension2 = oForm.Items.Item("cbdim2").Specific;
                icbDimension2 = oForm.Items.Item("cbdim2");
                oForm.DataSources.UserDataSources.Add("cbdim2", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension2.DataBind.SetBound(true, "", "cbdim2");

                cbDimension3 = oForm.Items.Item("cbdim3").Specific;
                icbDimension3 = oForm.Items.Item("cbdim3");
                oForm.DataSources.UserDataSources.Add("cbdim3", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension3.DataBind.SetBound(true, "", "cbdim3");

                cbDimension4 = oForm.Items.Item("cbdim4").Specific;
                icbDimension4 = oForm.Items.Item("cbdim4");
                oForm.DataSources.UserDataSources.Add("cbdim4", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension4.DataBind.SetBound(true, "", "cbdim4");

                cbDimension5 = oForm.Items.Item("cbdim5").Specific;
                icbDimension5 = oForm.Items.Item("cbdim5");
                oForm.DataSources.UserDataSources.Add("cbdim5", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension5.DataBind.SetBound(true, "", "cbdim5");

                lblDimension1 = oForm.Items.Item("283").Specific;
                ilblDimension1 = oForm.Items.Item("283");
                lblDimension2 = oForm.Items.Item("285").Specific;
                ilblDimension2 = oForm.Items.Item("285");
                lblDimension3 = oForm.Items.Item("287").Specific;
                ilblDimension3 = oForm.Items.Item("287");
                lblDimension4 = oForm.Items.Item("289").Specific;
                ilblDimension4 = oForm.Items.Item("289");
                lblDimension5 = oForm.Items.Item("291").Specific;
                ilblDimension5 = oForm.Items.Item("291");

                chkOffDay = oForm.Items.Item("choffday").Specific;
                ichkOffDay = oForm.Items.Item("choffday");
                oForm.DataSources.UserDataSources.Add("choffday", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkOffDay.DataBind.SetBound(true, "", "choffday");

                cbAttendanceAllowance = oForm.Items.Item("cbAttAlw").Specific;
                oForm.DataSources.UserDataSources.Add("cbAttAlw", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                cbAttendanceAllowance.DataBind.SetBound(true, "", "cbAttAlw");

                #endregion

                #region Communication

                txtWorkIM = oForm.Items.Item("txworkim").Specific;
                oForm.DataSources.UserDataSources.Add("txworkim", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkIM.DataBind.SetBound(true, "", "txworkim");

                txtPersonalIM = oForm.Items.Item("txperim").Specific;
                oForm.DataSources.UserDataSources.Add("txperim", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPersonalIM.DataBind.SetBound(true, "", "txperim");

                txtPersonalContact = oForm.Items.Item("txpercnt").Specific;
                oForm.DataSources.UserDataSources.Add("txpercnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPersonalContact.DataBind.SetBound(true, "", "txpercnt");

                txtPersonalEmail = oForm.Items.Item("txperemail").Specific;
                oForm.DataSources.UserDataSources.Add("txperemail", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPersonalEmail.DataBind.SetBound(true, "", "txperemail");

                #endregion

                #region Classification

                txtOrganizationUnit = oForm.Items.Item("txorgunit").Specific;
                oForm.DataSources.UserDataSources.Add("txorgunit", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtOrganizationUnit.DataBind.SetBound(true, "", "txorgunit");

                txtReportTo = oForm.Items.Item("txreport").Specific;
                itxtReportTo = oForm.Items.Item("txreport");
                oForm.DataSources.UserDataSources.Add("txreport", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtReportTo.DataBind.SetBound(true, "", "txreport");

                txtEmpContractType = oForm.Items.Item("txempct").Specific;
                oForm.DataSources.UserDataSources.Add("txempct", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtEmpContractType.DataBind.SetBound(true, "", "txempct");

                txtHRCalendar = oForm.Items.Item("txhrcal").Specific;
                oForm.DataSources.UserDataSources.Add("txhrcal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHRCalendar.DataBind.SetBound(true, "", "txhrcal");


                txtWindowsLogin = oForm.Items.Item("txwinlg").Specific;
                oForm.DataSources.UserDataSources.Add("txwinlg", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWindowsLogin.DataBind.SetBound(true, "", "txwinlg");

                txtEmpGrade = oForm.Items.Item("txempgrd").Specific;
                oForm.DataSources.UserDataSources.Add("txempgrd", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtEmpGrade.DataBind.SetBound(true, "", "txempgrd");


                txtPreviosEmpMonth = oForm.Items.Item("txprempmnt").Specific;
                oForm.DataSources.UserDataSources.Add("txprempmnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPreviosEmpMonth.DataBind.SetBound(true, "", "txprempmnt");

                txtWorkPermitRef = oForm.Items.Item("txwrkref").Specific;
                oForm.DataSources.UserDataSources.Add("txwrkref", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkPermitRef.DataBind.SetBound(true, "", "txwrkref");

                txtWorkPermitExpiry = oForm.Items.Item("txwrkpexp").Specific;
                oForm.DataSources.UserDataSources.Add("txwrkpexp", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtWorkPermitExpiry.DataBind.SetBound(true, "", "txwrkpexp");

                txtRemarks = oForm.Items.Item("txatch").Specific;
                oForm.DataSources.UserDataSources.Add("txatch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 200);
                txtRemarks.DataBind.SetBound(true, "", "txatch");

                txtContractExpiry = oForm.Items.Item("txconexp").Specific;
                oForm.DataSources.UserDataSources.Add("txconexp", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtContractExpiry.DataBind.SetBound(true, "", "txconexp");
                //txtContractExpiry.Value = DateTime.Now.ToString("yyyyMMdd");

                #endregion

                #region UAE

                txtEnglishName = oForm.Items.Item("txengname").Specific;
                oForm.DataSources.UserDataSources.Add("txengname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 35);
                txtEnglishName.DataBind.SetBound(true, "", "txengname");

                txtArabicName = oForm.Items.Item("txarname").Specific;
                oForm.DataSources.UserDataSources.Add("txarname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 35);
                txtArabicName.DataBind.SetBound(true, "", "txarname");

                txtPassportExpiryDtH = oForm.Items.Item("txpsexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txpsexpdt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtPassportExpiryDtH.DataBind.SetBound(true, "", "txpsexpdt");

                txtIDExpiryDtH = oForm.Items.Item("txidexph").Specific;
                oForm.DataSources.UserDataSources.Add("txidexph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtIDExpiryDtH.DataBind.SetBound(true, "", "txidexph");

                txtMedicalCardExpirydtH = oForm.Items.Item("txmedexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txmedexpdt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtMedicalCardExpirydtH.DataBind.SetBound(true, "", "txmedexpdt");

                txtDrvLicCompletionDtH = oForm.Items.Item("txdlcpldt").Specific;
                oForm.DataSources.UserDataSources.Add("txdlcpldt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtDrvLicCompletionDtH.DataBind.SetBound(true, "", "txdlcpldt");

                txtDrvLicLastDtH = oForm.Items.Item("txdldt").Specific;
                oForm.DataSources.UserDataSources.Add("txdldt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtDrvLicLastDtH.DataBind.SetBound(true, "", "txdldt");

                txtDrvLicReleaseDtH = oForm.Items.Item("txdlrdt").Specific;
                oForm.DataSources.UserDataSources.Add("txdlrdt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtDrvLicReleaseDtH.DataBind.SetBound(true, "", "txdlrdt");

                txtVisaNumber = oForm.Items.Item("txvisa").Specific;
                oForm.DataSources.UserDataSources.Add("txvisa", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtVisaNumber.DataBind.SetBound(true, "", "txvisa");

                txtIqamaProfessional = oForm.Items.Item("txiqama").Specific;
                oForm.DataSources.UserDataSources.Add("txiqama", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtIqamaProfessional.DataBind.SetBound(true, "", "txiqama");

                txtBankCardExpiryDtH = oForm.Items.Item("txbnkexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txbnkexpdt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtBankCardExpiryDtH.DataBind.SetBound(true, "", "txbnkexpdt");

                #endregion

                #region Absense

                mtAbsence = oForm.Items.Item("mtabsence").Specific;
                dtAbsence = oForm.DataSources.DataTables.Item("dtAbs");
                Serial = mtAbsence.Columns.Item("serial");
                aID = mtAbsence.Columns.Item("id");
                aID.Visible = false;
                aIsNew = mtAbsence.Columns.Item("isnew");
                aIsNew.Visible = false;
                aDescription = mtAbsence.Columns.Item("desc");
                aBalanceBF = mtAbsence.Columns.Item("bfbalance");
                aEntitled = mtAbsence.Columns.Item("entitled");
                aTotalAvailable = mtAbsence.Columns.Item("available");
                aUsed = mtAbsence.Columns.Item("used");
                aRequested = mtAbsence.Columns.Item("requested");
                aApproved = mtAbsence.Columns.Item("approved");
                aBalance = mtAbsence.Columns.Item("balance");
                //LeaveType = mtAbsence.Columns.Item("lt");

                #endregion

                itxtFirstName.Click();
                GetData();
                if (Program.systemInfo.SAPB1Integration == true)
                {
                    ibtnSyncTOSBO.Visible = true;
                }
                else
                {
                    ibtnSyncTOSBO.Visible = false;
                }
                oForm.PaneLevel = 1;
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oApplication.StatusBar.SetText("Employee master loaded successfully.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception Ex)
            {
                logger(Ex);
            }
        }

        private void LoadImageFile()
        {
            try
            {

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    pctBox.Picture = FilePath;

                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : LoadImageFile Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadPath1File()
        {
            try
            {

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    //picBoX2.Picture = FilePath;
                    //txtPath1.Value = FilePath;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : LoadImageFile Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SaveImageFile()
        {
            try
            {
                String EmpID;
                if (!String.IsNullOrEmpty(txtEmployeeCode.Value.Trim()))
                {
                    EmpID = txtEmployeeCode.Value.Trim();
                }
                else
                {
                    oApplication.StatusBar.SetText("No Employee Selected.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (String.IsNullOrEmpty(FilePath))
                {
                    oApplication.StatusBar.SetText("No Image Selected.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                picPath = oCompany.BitMapPath;
                if (!Directory.Exists(picPath))
                {
                    oApplication.StatusBar.SetText("Directory is Not Present.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    Directory.CreateDirectory(picPath);
                }
                if (String.IsNullOrEmpty(picPath))
                {
                    oApplication.StatusBar.SetText("Define Default Picture Location General Setting.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

                //picPath += Path.GetFileName(FilePath);

                picPath = Path.Combine(picPath, Path.GetFileName(FilePath));
                if (File.Exists(picPath))
                {
                    File.Delete(picPath);

                }
                File.Copy(FilePath, picPath, true);

                MstEmployee oImgEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpID select a).FirstOrDefault();

                if (oImgEmp != null)
                {
                    oImgEmp.ImgPath = picPath;
                }

                dbHrPayroll.SubmitChanges();


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : SaveImageFile Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void fillCbs()
        {
            oForm.Freeze(true);
            try
            {
                FillPositionCombo(cbPosition);
                FillDepartmentCombo(cbDepartment);
                FillBranchCombo(cbBranch);
                //FillManagerCombo(cbManager);
                FillLocationsCombo(cbLocation);
                FillComboGratuity(cbGratuity);
                FillComboOTSlab(cmbOTSlabs);
                FillComboShiftDaysSlab(cmbShiftDaySlabs);
                //FillComboBonusSlab(cmbBonusSlabs);
                FillDesignationCombo(cbDesignation);
                FillJobTitleCombo(cbJobTitle);
                FillMartialCombo(cbMartial);
                FillReligionCombo(cbReligion);
                FillGenderCombo(cbGender);
                FillCostCenterCombo(cbCostCenter);
                FillProjectCombo(cbProject);
                FillProfitCenterCombo(cmbProfitCenter);
                FillSboUsrCombo(cbSBOLinkID);
                FillOHEMUserCombo(cbOhemUser);
                FillLovList(cbSalaryCurrency, "SalaryCurrency");
                FillLovList(cbPaymentMode, "PaymentMode");
                FillLovList(cbAccountType, "AccountType");
                FillLovList(cmbTransport, "TransMod");
                FillLovList(cmbRecruitment, "RecruitMod");
                FillLovList(cmbInsuranceCategory, "InsuranceMod");
                FillLovList(cmbEmpOffDay, "OFFDAYS");
                FillBloodGroupCombo(cbBloodGroup);
                //FillManagerCombo(cbReportingManager);
                //FillRelationShipCombo(rType);
                FillCertificationCombo(cCertification);
                FillInstituteCombo(eInstituteName);
                FillQualificationCombo(eQualification);
                FillComboDimension1(cbDimension1);
                FillComboDimension2(cbDimension2);
                FillComboDimension3(cbDimension3);
                FillComboDimension4(cbDimension4);
                FillComboDimension5(cbDimension5);
                FillComboCategory(cmbCategory);
                FillComboSubCategory(cmbSubCategory);
                FillCountryCombo();
                FillStatesCombo();
                FillPayrollCombo(cbPayroll);
                FillAttendanceAllowanceCombo(cbAttendanceAllowance);
                FillBonusCombo(cbBonusSlabs);
                FillShiftCombo();
                FillContractTypeCombo();
                //FillComboDeductionRule(cmbDeductionRule);
                InitiallizeDocument();
            }
            catch
            {
            }
            flgEmpFilter = false;
            oForm.Freeze(false);

        }

        private void InitiallizeDocument()
        {
            oForm.Freeze(true);
            try
            {
                //System.Timers.Timer
                loadDocument = 0;

                #region Header Area
                txtFirstName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtFirstName.Item.UniqueID).ValueEx = "";
                txtMiddleName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtMiddleName.Item.UniqueID).ValueEx = "";
                txtLastName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtLastName.Item.UniqueID).ValueEx = "";
                //txtJobTitle.Value = "";
                //chkCreateUser.Checked = false;
                txtUserCode.Value = "";
                oForm.DataSources.UserDataSources.Item(txtUserCode.Item.UniqueID).ValueEx = "";
                txtPassword.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPassword.Item.UniqueID).ValueEx = "";
                if (Program.systemInfo.FlgAutoNumber == true)
                {
                    string empcode = GetNextNumber();
                    //txtEmployeeCode.Value = empcode;//GetNextNumber();
                    oForm.DataSources.UserDataSources.Item(txtEmployeeCode.Item.UniqueID).ValueEx = empcode;
                }
                else
                {
                    txtEmployeeCode.Value = "";
                    oForm.DataSources.UserDataSources.Item(txtEmployeeCode.Item.UniqueID).ValueEx = "";
                }
                txtInitials.Value = "";
                oForm.DataSources.UserDataSources.Item(txtInitials.Item.UniqueID).ValueEx = "";
                txtNamePrefix.Value = "";
                oForm.DataSources.UserDataSources.Item(txtNamePrefix.Item.UniqueID).ValueEx = "";
                txtOfficePhn.Value = "";
                oForm.DataSources.UserDataSources.Item(txtOfficePhn.Item.UniqueID).ValueEx = "";
                txtHomePhn.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomePhn.Item.UniqueID).ValueEx = "";
                txtExtention.Value = "";
                oForm.DataSources.UserDataSources.Item(txtExtention.Item.UniqueID).ValueEx = "";
                txtMobilePhn.Value = "";
                oForm.DataSources.UserDataSources.Item(txtMobilePhn.Item.UniqueID).ValueEx = "";
                txtPager.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPager.Item.UniqueID).ValueEx = "";
                txtFax.Value = "";
                oForm.DataSources.UserDataSources.Item(txtFax.Item.UniqueID).ValueEx = "";
                txtEmail.Value = "";
                oForm.DataSources.UserDataSources.Item(txtEmail.Item.UniqueID).ValueEx = "";
                cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbLocation.Item.UniqueID).ValueEx = "-1";
                cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDepartment.Item.UniqueID).ValueEx = "-1";
                cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDesignation.Item.UniqueID).ValueEx = "-1";
                cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbBranch.Item.UniqueID).ValueEx = "-1";
                cbPosition.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbPosition.Item.UniqueID).ValueEx = "-1";
                txtManager.Value = "";
                oForm.DataSources.UserDataSources.Item(txtManager.Item.UniqueID).ValueEx = "";
                cbJobTitle.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbJobTitle.Item.UniqueID).ValueEx = "-1";
                #endregion

                #region Address

                txtHomeStreet.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeStreet.Item.UniqueID).ValueEx = "";
                txtHomeStreetNo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeStreetNo.Item.UniqueID).ValueEx = "";
                txtHomeBlock.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeBlock.Item.UniqueID).ValueEx = "";
                txtHomeBuilding.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeBuilding.Item.UniqueID).ValueEx = "";
                txtHomeZip.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeZip.Item.UniqueID).ValueEx = "";
                txtHomeCity.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeCity.Item.UniqueID).ValueEx = "";
                cbHomeState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbHomeState.Item.UniqueID).ValueEx = "-1";
                cbHomeCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbHomeCountry.Item.UniqueID).ValueEx = "-1";

                txtWorkStreet.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkStreet.Item.UniqueID).ValueEx = "";
                txtWorkStreetNo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkStreetNo.Item.UniqueID).ValueEx = "";
                txtWorkBlock.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkBlock.Item.UniqueID).ValueEx = "";
                txtWorkBuilding.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkBuilding.Item.UniqueID).ValueEx = "";
                txtWorkZip.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkZip.Item.UniqueID).ValueEx = "";
                txtWorkCity.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkCity.Item.UniqueID).ValueEx = "";
                cbWorkState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbWorkState.Item.UniqueID).ValueEx = "-1";
                cbWorkCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbWorkCountry.Item.UniqueID).ValueEx = "-1";
                txtHomeBranches.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHomeBranches.Item.UniqueID).ValueEx = "";
                #endregion

                #region Emergency Detail

                txtPriCntName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPriCntName.Item.UniqueID).ValueEx = "";
                txtPriCntRelation.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPriCntRelation.Item.UniqueID).ValueEx = "";
                txtPriCntNoLandLine.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPriCntNoLandLine.Item.UniqueID).ValueEx = "";
                txtPriCntNoMobile.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPriCntNoMobile.Item.UniqueID).ValueEx = "";
                txtPriCntAddress.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPriCntAddress.Item.UniqueID).ValueEx = "";
                txtPriCntCity.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPriCntCity.Item.UniqueID).ValueEx = "";
                cbPriCntState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbPriCntState.Item.UniqueID).ValueEx = "-1";
                cbPriCntCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbPriCntCountry.Item.UniqueID).ValueEx = "-1";

                txtSecCntName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSecCntName.Item.UniqueID).ValueEx = "";
                txtSecCntRelation.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSecCntRelation.Item.UniqueID).ValueEx = "";
                txtSecCntNoLandLine.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSecCntNoLandLine.Item.UniqueID).ValueEx = "";
                txtSecCntNoMobile.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSecCntNoMobile.Item.UniqueID).ValueEx = "";
                txtSecCntAddress.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSecCntAddress.Item.UniqueID).ValueEx = "";
                txtSecCntCity.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSecCntCity.Item.UniqueID).ValueEx = "";
                cbSecCntState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbSecCntState.Item.UniqueID).ValueEx = "-1";
                cbSecCntCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbSecCntCountry.Item.UniqueID).ValueEx = "-1";
                #endregion

                #region Personal Tab

                txtFatherName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtFatherName.Item.UniqueID).ValueEx = "";
                txtMotherName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtMotherName.Item.UniqueID).ValueEx = "";
                cbMartial.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbMartial.Item.UniqueID).ValueEx = "-1";
                cbReligion.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbReligion.Item.UniqueID).ValueEx = "-1";
                cbGender.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbGender.Item.UniqueID).ValueEx = "-1";
                cbCostCenter.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbCostCenter.Item.UniqueID).ValueEx = "-1";
                cbProject.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbProject.Item.UniqueID).ValueEx = "-1";
                cbBloodGroup.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbBloodGroup.Item.UniqueID).ValueEx = "-1";
                txtSSNumber.Value = "";
                oForm.DataSources.UserDataSources.Item(txtSSNumber.Item.UniqueID).ValueEx = "";
                txtUnionMemberShip.Value = "";
                oForm.DataSources.UserDataSources.Item(txtUnionMemberShip.Item.UniqueID).ValueEx = "";
                txtUnionMemberShipNo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtUnionMemberShipNo.Item.UniqueID).ValueEx = "";
                txtNationality.Value = "";
                oForm.DataSources.UserDataSources.Item(txtNationality.Item.UniqueID).ValueEx = "";
                txtPassportNo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPassportNo.Item.UniqueID).ValueEx = "";
                txtPassportDateofIssue.Value = ""; // DateTime.Now.ToString("yyyyMMdd");
                oForm.DataSources.UserDataSources.Item(txtPassportDateofIssue.Item.UniqueID).ValueEx = "";
                txtPassportExpiry.Value = ""; // DateTime.Now.ToString("yyyyMMdd");
                oForm.DataSources.UserDataSources.Item(txtPassportExpiry.Item.UniqueID).ValueEx = "";
                txtIncomeTax.Value = "";
                oForm.DataSources.UserDataSources.Item(txtIncomeTax.Item.UniqueID).ValueEx = "";
                txtIDCardNo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtIDCardNo.Item.UniqueID).ValueEx = "";
                txtIDDtOfIssue.Value = ""; // DateTime.Now.ToString("yyyyMMdd");
                oForm.DataSources.UserDataSources.Item(txtIDDtOfIssue.Item.UniqueID).ValueEx = "";
                txtIDExpiryDate.Value = "";// DateTime.Now.ToString("yyyyMMdd");
                oForm.DataSources.UserDataSources.Item(txtIDExpiryDate.Item.UniqueID).ValueEx = "";
                txtIDPlaceOfIssue.Value = "";
                oForm.DataSources.UserDataSources.Item(txtIDPlaceOfIssue.Item.UniqueID).ValueEx = "";
                txtIDIssuedBy.Value = "";
                oForm.DataSources.UserDataSources.Item(txtIDIssuedBy.Item.UniqueID).ValueEx = "";

                #endregion

                #region Salary Tab

                txtBasicSalary.Value = "";
                oForm.DataSources.UserDataSources.Item(txtBasicSalary.Item.UniqueID).ValueEx = "0";
                cbSalaryCurrency.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbSalaryCurrency.Item.UniqueID).ValueEx = "-1";
                txtEmpCalendar.Value = "";
                oForm.DataSources.UserDataSources.Item(txtEmpCalendar.Item.UniqueID).ValueEx = "";
                cbPaymentMode.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbPaymentMode.Item.UniqueID).ValueEx = "-1";
                txtAccountNo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtAccountNo.Item.UniqueID).ValueEx = "";
                txtAccountTitle.Value = "";
                oForm.DataSources.UserDataSources.Item(txtAccountTitle.Item.UniqueID).ValueEx = "";
                txtBankName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtBankName.Item.UniqueID).ValueEx = "";
                txtBankBranch.Value = "";
                oForm.DataSources.UserDataSources.Item(txtBankBranch.Item.UniqueID).ValueEx = "";
                cbAccountType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbAccountType.Item.UniqueID).ValueEx = "-1";
                txtEffectiveDate.Value = ""; // DateTime.Now.ToString("yyyyMMdd");
                oForm.DataSources.UserDataSources.Item(txtEffectiveDate.Item.UniqueID).ValueEx = "";
                txtPercentage.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPercentage.Item.UniqueID).ValueEx = "0";
                txtDateOfJoining.Value = ""; // DateTime.Now.ToString("yyyyMMdd");
                oForm.DataSources.UserDataSources.Item(txtDateOfJoining.Item.UniqueID).ValueEx = "";
                cbBloodGroup.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbBloodGroup.Item.UniqueID).ValueEx = "-1";
                cbcontractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbcontractType.Item.UniqueID).ValueEx = "-1";
                cmbProfitCenter.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cmbProfitCenter.Item.UniqueID).ValueEx = "-1";
                cmbOTSlabs.Select(0, BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cmbOTSlabs.Item.UniqueID).ValueEx = "-1";
                cbGratuity.Select(0, BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbGratuity.Item.UniqueID).ValueEx = "-1";
                cmbShiftDaySlabs.Select(0, BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cmbShiftDaySlabs.Item.UniqueID).ValueEx = "-1";

                cbBonusSlabs.Select(0, BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbBonusSlabs.Item.UniqueID).ValueEx = "-1";
                #endregion

                #region Payroll

                cbAttendanceAllowance.Select(0, BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbAttendanceAllowance.Item.UniqueID).ValueEx = "-1";

                cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbPayroll.Item.UniqueID).ValueEx = "-1";
                cbDimension1.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension1.Item.UniqueID).ValueEx = "-1";
                cbDimension2.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension2.Item.UniqueID).ValueEx = "-1";
                cbDimension3.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension3.Item.UniqueID).ValueEx = "-1";
                cbDimension4.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension4.Item.UniqueID).ValueEx = "-1";
                cbDimension5.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension5.Item.UniqueID).ValueEx = "-1";

                #endregion

                #region Absence

                dtAbsence.Rows.Clear();
                mtAbsence.LoadFromDataSource();

                #endregion

                #region Communication

                txtWorkIM.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkIM.Item.UniqueID).ValueEx = "";
                txtPersonalIM.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPersonalIM.Item.UniqueID).ValueEx = "";
                txtPersonalContact.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPersonalContact.Item.UniqueID).ValueEx = "";
                txtPersonalEmail.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPersonalEmail.Item.UniqueID).ValueEx = "";

                #endregion

                #region Classification

                txtOrganizationUnit.Value = "";
                oForm.DataSources.UserDataSources.Item(txtOrganizationUnit.Item.UniqueID).ValueEx = "";
                txtReportTo.Value = "";
                oForm.DataSources.UserDataSources.Item(txtReportTo.Item.UniqueID).ValueEx = "";
                txtEmpContractType.Value = "";
                oForm.DataSources.UserDataSources.Item(txtEmpContractType.Item.UniqueID).ValueEx = "";
                txtHRCalendar.Value = "";
                oForm.DataSources.UserDataSources.Item(txtHRCalendar.Item.UniqueID).ValueEx = "";
                txtWindowsLogin.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWindowsLogin.Item.UniqueID).ValueEx = "";
                txtEmpGrade.Value = "";
                oForm.DataSources.UserDataSources.Item(txtEmpGrade.Item.UniqueID).ValueEx = "1";
                txtPreviosEmpMonth.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPreviosEmpMonth.Item.UniqueID).ValueEx = "";
                txtWorkPermitRef.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkPermitRef.Item.UniqueID).ValueEx = "";
                txtWorkPermitExpiry.Value = "";
                oForm.DataSources.UserDataSources.Item(txtWorkPermitExpiry.Item.UniqueID).ValueEx = "";
                txtContractExpiry.Value = "";
                oForm.DataSources.UserDataSources.Item(txtContractExpiry.Item.UniqueID).ValueEx = "";

                #endregion

                #region Arabic

                txtEnglishName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtEnglishName.Item.UniqueID).ValueEx = "";
                txtArabicName.Value = "";
                oForm.DataSources.UserDataSources.Item(txtArabicName.Item.UniqueID).ValueEx = "";
                txtPassportExpiryDtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtPassportExpiryDtH.Item.UniqueID).ValueEx = "";
                txtIDExpiryDtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtIDExpiryDtH.Item.UniqueID).ValueEx = "";
                txtMedicalCardExpirydtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtMedicalCardExpirydtH.Item.UniqueID).ValueEx = "";
                txtDrvLicCompletionDtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtDrvLicCompletionDtH.Item.UniqueID).ValueEx = "";
                txtDrvLicLastDtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtDrvLicLastDtH.Item.UniqueID).ValueEx = "";
                txtDrvLicReleaseDtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtDrvLicReleaseDtH.Item.UniqueID).ValueEx = "";
                txtVisaNumber.Value = "";
                oForm.DataSources.UserDataSources.Item(txtVisaNumber.Item.UniqueID).ValueEx = "";
                txtIqamaProfessional.Value = "";
                oForm.DataSources.UserDataSources.Item(txtIqamaProfessional.Item.UniqueID).ValueEx = "";
                txtBankCardExpiryDtH.Value = "";
                oForm.DataSources.UserDataSources.Item(txtBankCardExpiryDtH.Item.UniqueID).ValueEx = "";

                #endregion

                #region Attachment
                txtEmployeeCode.Active = true;
                //oForm.DataSources.UserDataSources.Item(txtEmployeeCode.Item.UniqueID).ValueEx = "";
                #endregion

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitiallizeDocument : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
            //txtEmployeeCode.Active = true;
        }

        private string GetNextNumber()
        {
            Int32 retValue = 0;
            try
            {
                retValue = Convert.ToInt32((from a in dbHrPayroll.MstEmployee select Convert.ToInt32(a.EmpID)).Max());
                if (retValue == 0)
                {
                    retValue = 1;
                }
                else
                {
                    retValue++;
                }
            }
            catch (Exception ex)
            {
                retValue = 1;
            }
            return Convert.ToString(retValue);
        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {
                    if (!string.IsNullOrEmpty(Program.objHrmsUI.EmployeeFilterValues))
                    {
                        string[] arr;
                        if (Program.objHrmsUI.EmployeeFilterValues.Contains(','))
                        {
                            arr = Program.objHrmsUI.EmployeeFilterValues.Split(',');
                            oEmployees = (from a in dbHrPayroll.MstEmployee
                                          where arr.Contains((a.PayrollID != null ? a.PayrollID.ToString() : "0"))
                                          select a).ToList();
                        }
                        else
                        {
                            oEmployees = (from a in dbHrPayroll.MstEmployee
                                          where a.PayrollID.ToString() == Program.objHrmsUI.EmployeeFilterValues
                                          select a).ToList();
                        }

                        Int32 i = 0;
                        foreach (var One in oEmployees)
                        {
                            CodeIndex.Add(One.ID.ToString(), i);
                            i++;
                        }
                        totalRecord = i;
                    }
                    else
                    {
                        oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
                        Int32 i = 0;
                        foreach (MstEmployee OEmp in oEmployees)
                        {
                            CodeIndex.Add(OEmp.ID.ToString(), i);
                            i++;
                        }
                        totalRecord = i;
                    }
                }
                else
                {
                    oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {
                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }

        public override void fillFields()
        {
            base.fillFields();
            InitiallizeDocument();
            oForm.Freeze(true);
            try
            {
                //IEnumerable<MstEmployee> oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
                MstEmployee oEmp = oEmployees.ElementAt<MstEmployee>(currentRecord);
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, oEmp);
                //var oEmp = (from a in dbHrPayroll.MstEmployee
                //            where a.ID == currentRecord
                //            select a).FirstOrDefault();
                if (oEmp == null)
                {
                    oApplication.StatusBar.SetText("Document didn't load successfully.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }
                //MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == Convert.ToString(CodeIndex.ContainsValue(currentRecord)) select a).FirstOrDefault();
                //Header Area
                //oForm.DataSources.UserDataSources.Item("txtFirstName").ValueEx = oEmp.FirstName;
                loadDocument = oEmp.ID;
                currentObjId = oEmp.ID.ToString();
                Program.ExtendendEmpID = oEmp.EmpID;
                txtFirstName.Value = oEmp.FirstName;
                txtFirstName.Active = true;
                txtMiddleName.Value = oEmp.MiddleName;
                txtLastName.Value = oEmp.LastName;
                //txtJobTitle.Value = oEmp.JobTitle;
                if (Convert.ToBoolean(oEmp.FlgUser))
                {
                    txtUserCode.Value = oEmp.MstUsers.ElementAt(0).UserCode;
                    txtPassword.Value = oEmp.MstUsers.ElementAt(0).PassCode;
                    //cbSBOLinkID.Select(oEmp.MstUsers.ElementAt(0).UserCode != oEmp.FirstName ? oEmp.MstUsers.ElementAt(0).UserCode : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue); 
                    cbSBOLinkID.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    txtUserCode.Value = "";
                    txtPassword.Value = "";
                    cbSBOLinkID.Select("0", SAPbouiCOM.BoSearchKey.psk_Index);
                }

                if (!String.IsNullOrEmpty(oEmp.ImgPath))
                {
                    pctBox.Picture = oEmp.ImgPath;
                }
                else
                {
                    pctBox.Picture = "";
                }
                chkActiveEmployee.Checked = Convert.ToBoolean(oEmp.FlgActive);
                chkPerPiece.Checked = Convert.ToBoolean(oEmp.FlgPerPiece);
                if (oEmp.FlgOTApplicable != null)
                {
                    chkOTApplicable.Checked = Convert.ToBoolean(oEmp.FlgOTApplicable);
                }
                else
                {
                    chkOTApplicable.Checked = false;
                }
                if (oEmp.FlgTax != null)
                {
                    flgTax.Checked = Convert.ToBoolean(oEmp.FlgTax);
                }
                else
                {
                    flgTax.Checked = false;
                }
                if (oEmp.FlgSuperVisor != null)
                {
                    chkSup.Checked = Convert.ToBoolean(oEmp.FlgSuperVisor);
                }
                else
                {
                    chkSup.Checked = false;
                }

                txtEmployeeCode.Value = oEmp.EmpID;
                txtInitials.Value = oEmp.Initials;
                txtNamePrefix.Value = oEmp.NamePrefix;
                txtOfficePhn.Value = oEmp.OfficePhone;
                txtHomePhn.Value = oEmp.HomePhone;
                txtExtention.Value = oEmp.OfficeExtension;
                txtMobilePhn.Value = oEmp.OfficeMobile;
                txtPager.Value = oEmp.Pager;
                txtFax.Value = oEmp.Fax;
                txtEmail.Value = oEmp.OfficeEmail;
                cbLocation.Select(oEmp.Location != null ? oEmp.Location.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbDepartment.Select(oEmp.DepartmentID != null ? oEmp.DepartmentID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbDesignation.Select(oEmp.DesignationID != null ? oEmp.DesignationID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbBranch.Select(oEmp.BranchID != null ? oEmp.BranchID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbPosition.Select(oEmp.PositionID != null ? oEmp.PositionID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                //cbManager.Select(oEmp.Manager != null ? oEmp.Manager.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                if (oEmp.Manager != null)
                {
                    if (oEmp.Manager != -1)
                    {
                        MstEmployee mngEmp = (from a in dbHrPayroll.MstEmployee where a.ID == oEmp.Manager select a).FirstOrDefault();
                        txtManager.Value = mngEmp.EmpID;
                    }
                }
                else
                {
                    txtManager.Value = "";
                }
                if (!String.IsNullOrEmpty(oEmp.SBOEmpCode))
                {
                    cbSBOLinkID.Select(oEmp.SboUserCode != null ? oEmp.SboUserCode.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbSBOLinkID.Select("-1", BoSearchKey.psk_ByValue);
                }
                if (!String.IsNullOrEmpty(oEmp.JobTitle))
                {
                    cbJobTitle.Select(oEmp.JobTitle.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbJobTitle.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                //Address Tab

                txtHomeStreet.Value = oEmp.WAStreet;
                txtHomeStreetNo.Value = oEmp.WAStreetNo;
                txtHomeBlock.Value = oEmp.WABlock;
                txtHomeBuilding.Value = oEmp.WAOther;
                txtHomeZip.Value = oEmp.WAZipCode;
                txtHomeCity.Value = oEmp.WACity;
                cbHomeState.Select(oEmp.WAState != null ? oEmp.WAState.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbHomeCountry.Select(oEmp.WACountry != null ? oEmp.WACountry.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                if (oEmp.FlgCompanyResidence != null)
                {
                    oForm.DataSources.UserDataSources.Item(chkCompanyResidence.DataBind.Alias).ValueEx = oEmp.FlgCompanyResidence == true ? "Y" : "N";
                }
                else
                {
                    chkCompanyResidence.Checked = false;
                }

                txtWorkStreet.Value = oEmp.HAStreet;
                txtWorkStreetNo.Value = oEmp.HAStreetNo;
                txtWorkBlock.Value = oEmp.HABlock;
                txtWorkBuilding.Value = oEmp.HAOther;
                txtWorkZip.Value = oEmp.HAZipCode;
                txtWorkCity.Value = oEmp.HACity;
                cbWorkState.Select(oEmp.HAState != null ? oEmp.HAState.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbWorkCountry.Select(oEmp.HACountry != null ? oEmp.HACountry.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtCompanyResidence.Value = string.IsNullOrEmpty(oEmp.CompanyAddress) ? "" : oEmp.CompanyAddress;

                //Emergency Detail Tab
                txtPriCntName.Value = oEmp.PriPersonName;
                txtPriCntRelation.Value = oEmp.PriRelationShip;
                txtPriCntNoLandLine.Value = oEmp.PriContactNoLandLine;
                txtPriCntNoMobile.Value = oEmp.PriContactNoMobile;
                txtPriCntAddress.Value = oEmp.PriAddress;
                txtPriCntCity.Value = oEmp.PriCity;
                cbPriCntState.Select(oEmp.PriState != null ? oEmp.PriState.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbPriCntCountry.Select(oEmp.PriCountry != null ? oEmp.PriCountry.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

                txtSecCntName.Value = oEmp.SecPersonName;
                txtSecCntRelation.Value = oEmp.SecRelationShip;
                txtSecCntNoLandLine.Value = oEmp.SecContactNoLandline;
                txtSecCntNoMobile.Value = oEmp.SecContactNoMobile;
                txtSecCntAddress.Value = oEmp.SecAddress;
                txtSecCntCity.Value = oEmp.SecCity;
                cbSecCntState.Select(oEmp.SecState != null ? oEmp.SecState.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbSecCntCountry.Select(oEmp.SecCountry != null ? oEmp.SecCountry.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

                //Administrator Tab Empty

                //Personal Tab

                txtFatherName.Value = oEmp.FatherName;
                txtMotherName.Value = oEmp.MotherName;
                cbMartial.Select(oEmp.MartialStatusID != null ? oEmp.MartialStatusID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbReligion.Select(oEmp.ReligionID != null ? oEmp.ReligionID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtSSNumber.Value = oEmp.SocialSecurityNo;
                txtUnionMemberShip.Value = oEmp.EmpUnion;
                txtUnionMemberShipNo.Value = oEmp.UnionMembershipNo;
                txtNationality.Value = oEmp.Nationality;
                txtPassportNo.Value = oEmp.PassportNo;
                cbGender.Select(oEmp.GenderID != null ? oEmp.GenderID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbCostCenter.Select(oEmp.CostCenter != null ? oEmp.CostCenter.ToString().Trim() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbProject.Select(oEmp.Project != null ? oEmp.Project.ToString().Trim() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cmbTransport.Select(oEmp.TransportMode == null ? "-1" : oEmp.TransportMode.Trim().ToString(), BoSearchKey.psk_ByValue);
                cmbRecruitment.Select(oEmp.RecruitmentMode == null ? "-1" : oEmp.RecruitmentMode.Trim().ToString(), BoSearchKey.psk_ByValue);
                cmbInsuranceCategory.Select(oEmp.InsuranceCategory == null ? "-1" : oEmp.InsuranceCategory.Trim().ToString(), BoSearchKey.psk_ByValue);
                if (oEmp.DOB != null)
                {
                    if (oEmp.DOB > DateTime.MinValue)
                    {
                        txtDOB.Value = Convert.ToDateTime(oEmp.DOB).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtDOB.Value = "";
                    }
                }
                else
                {
                    txtDOB.Value = "";
                }
                if (oEmp.TerminationDate != null)
                {
                    if (oEmp.TerminationDate > DateTime.MinValue)
                    {
                        txtTermination.Value = Convert.ToDateTime(oEmp.TerminationDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtTermination.Value = "";
                    }
                }
                else
                {
                    txtTermination.Value = "";
                }
                if (oEmp.ResignDate != null)
                {
                    if (oEmp.ResignDate > DateTime.MinValue)
                    {
                        txtResignation.Value = Convert.ToDateTime(oEmp.ResignDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtResignation.Value = "";
                    }
                }
                else
                {
                    txtResignation.Value = "";
                }
                if (oEmp.PassportDateofIssue != null)
                {
                    if (oEmp.PassportDateofIssue > DateTime.MinValue)
                    {
                        txtPassportDateofIssue.Value = Convert.ToDateTime(oEmp.PassportDateofIssue).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtPassportDateofIssue.Value = "";
                    }
                }
                else
                {
                    txtPassportDateofIssue.Value = "";
                }
                if (oEmp.PassportExpiryDate != null)
                {
                    if (oEmp.PassportExpiryDate > DateTime.MinValue)
                    {
                        txtPassportExpiry.Value = Convert.ToDateTime(oEmp.PassportExpiryDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtPassportExpiry.Value = "";
                    }
                }
                else
                {
                    txtPassportExpiry.Value = "";
                }

                txtIncomeTax.Value = oEmp.IncomeTaxNo;
                txtIDCardNo.Value = oEmp.IDNo;
                if (oEmp.IDDateofIssue != null)
                {
                    if (oEmp.IDDateofIssue > DateTime.MinValue)
                    {
                        txtIDDtOfIssue.Value = Convert.ToDateTime(oEmp.IDDateofIssue).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtIDDtOfIssue.Value = "";
                    }

                }
                else
                {
                    txtIDDtOfIssue.Value = "";
                }
                if (oEmp.IDExpiryDate != null)
                {
                    if (oEmp.IDExpiryDate > DateTime.MinValue)
                    {
                        txtIDExpiryDate.Value = Convert.ToDateTime(oEmp.IDExpiryDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtIDExpiryDate.Value = "";
                    }
                }
                else
                {
                    txtIDExpiryDate.Value = "";
                }
                if (oEmp.Category == null)
                {
                    cmbCategory.Select("-1", BoSearchKey.psk_ByValue);
                }
                else
                {
                    cmbCategory.Select(oEmp.Category.ToString(), BoSearchKey.psk_ByValue);
                }
                if (oEmp.SubCategory == null)
                {
                    cmbSubCategory.Select("-1", BoSearchKey.psk_ByValue);
                }
                else
                {
                    cmbSubCategory.Select(oEmp.SubCategory.ToString(), BoSearchKey.psk_ByValue);
                }
                txtIDPlaceOfIssue.Value = oEmp.IDPlaceofIssue;
                txtIDIssuedBy.Value = oEmp.IDIssuedBy;
                cbOhemUser.Select(oEmp.SBOEmpCode == null ? "-1" : oEmp.SBOEmpCode, SAPbouiCOM.BoSearchKey.psk_ByValue);

                //Salary Tab
                var shift = dbHrPayroll.TrnsAttendanceRegister.Where(tr => tr.EmpID == oEmp.ID && tr.Date == DateTime.Now.Date).FirstOrDefault();
                if (shift != null)
                {
                    //int shiftID = shift.ShiftID.Value;
                    string ShiftCode = shift.MstShifts.Description;
                    cbShift.Select(ShiftCode.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    var shiftTS = dbHrPayroll.TrnsAttendanceRegisterTS.Where(ab => ab.EmpID == oEmp.ID && ab.Date == DateTime.Now.Date).FirstOrDefault();
                    if (shiftTS != null)
                    {
                        //int shiftid = shiftTS.ShiftID.Value;
                        string ShiftCode = shiftTS.MstShifts.Description;
                        cbShift.Select(ShiftCode.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    }
                }
                txtBasicSalary.Value = oEmp.BasicSalary.ToString();
                if (oEmp.GrossSalary != null)
                {
                    txtGrossComputed.Value = oEmp.GrossSalary.ToString();
                }
                else
                {
                    txtGrossComputed.Value = "";
                }
                if (oEmp.GosiSalary != null)
                {
                    txGosi.Value = oEmp.GosiSalary.ToString();
                }
                if (oEmp.GosiSalaryV != null)
                {
                    txGosiV.Value = oEmp.GosiSalaryV.ToString();
                }
                if (!string.IsNullOrEmpty(oEmp.EmployeeContractType))
                {
                    cbcontractType.Select(oEmp.EmployeeContractType, SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbcontractType.Select("0", SAPbouiCOM.BoSearchKey.psk_Index);
                }
                if (string.IsNullOrEmpty(oEmp.ProfitCenter))
                {
                    cmbProfitCenter.Select("0", BoSearchKey.psk_Index);
                }
                else
                {
                    cmbProfitCenter.Select(oEmp.ProfitCenter, BoSearchKey.psk_ByValue);
                }
                if (oEmp.FlgEmail != null)
                {
                    oForm.DataSources.UserDataSources.Item("chkEmail").ValueEx = oEmp.FlgEmail == true ? "Y" : "N";
                }
                else
                {
                    chkEmailSalarySlip.Checked = false;
                }
                if (oEmp.FlgSandwich != null)
                {
                    oForm.DataSources.UserDataSources.Item("chsl").ValueEx = oEmp.FlgSandwich == true ? "Y" : "N";
                }
                else
                {
                    chkSandwichLeaves.Checked = false;
                }
                if (oEmp.FlgBlackListed != null)
                {
                    oForm.DataSources.UserDataSources.Item(chkBlackListed.DataBind.Alias).ValueEx = oEmp.FlgBlackListed == true ? "Y" : "N";
                }
                else
                {
                    chkBlackListed.Checked = false;
                }
                cbSalaryCurrency.Select(String.IsNullOrEmpty(oEmp.SalaryCurrency) ? "-1" : oEmp.SalaryCurrency, SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtEmpCalendar.Value = oEmp.EmpCalender != null ? oEmp.EmpCalender.ToString() : "";
                //txtEmpShift.Value = "";
                cbPaymentMode.Select(oEmp.PaymentMode != null ? oEmp.PaymentMode.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbBloodGroup.Select(oEmp.BloodGroupID != null ? oEmp.BloodGroupID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                //cmbDeductionRule.Select(oEmp.DeductionRules != null ? oEmp.DeductionRules.ToString() : "-1", BoSearchKey.psk_ByValue);
                txtAccountTitle.Value = oEmp.AccountTitle;
                txtBankName.Value = oEmp.BankName;
                txtBankBranch.Value = oEmp.BankBranch;
                txtAccountNo.Value = oEmp.AccountNo;
                txtAllowedAdvance.Value = oEmp.AllowedAdvance != null ? string.Format("{0:0.00}", oEmp.AllowedAdvance) : "0.00";
                cbAccountType.Select(oEmp.AccountType != null ? oEmp.AccountType.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                if (oEmp.EffectiveDate != null)
                {
                    if (oEmp.EffectiveDate > DateTime.MinValue)
                    {
                        txtEffectiveDate.Value = Convert.ToDateTime(oEmp.EffectiveDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtEffectiveDate.Value = "";
                    }
                }
                else
                {
                    txtEffectiveDate.Value = "";
                }

                txtPercentage.Value = oEmp.PercentagePaid.ToString();

                if (oEmp.JoiningDate != null)
                {
                    if (oEmp.JoiningDate > DateTime.MinValue)
                    {
                        txtDateOfJoining.Value = Convert.ToDateTime(oEmp.JoiningDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtDateOfJoining.Value = "";
                    }
                }
                else
                {
                    txtDateOfJoining.Value = "";
                }

                if (oEmp.GratuitySlabs == null)
                {
                    cbGratuity.Select(0, BoSearchKey.psk_Index);
                }
                else
                {
                    cbGratuity.Select(oEmp.GratuitySlabs != null ? oEmp.GratuitySlabs.ToString() : "-1", BoSearchKey.psk_ByValue);
                }

                if (oEmp.OTSlabs == null)
                {
                    cmbOTSlabs.Select(0, BoSearchKey.psk_Index);
                }
                else
                {
                    cmbOTSlabs.Select(oEmp.OTSlabs != null ? oEmp.OTSlabs.ToString() : "-1", BoSearchKey.psk_ByValue);
                }

                if (string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                {
                    cmbShiftDaySlabs.Select(0, BoSearchKey.psk_Index);
                }
                else
                {
                    cmbShiftDaySlabs.Select(string.IsNullOrEmpty(oEmp.ShiftDaysCode) ? "-1" : oEmp.ShiftDaysCode, BoSearchKey.psk_ByValue);
                }

                if (String.IsNullOrEmpty(oEmp.BonusCode))
                {
                    //cbBonusSlabs.Select(oEmp.BonusCode != null ? oEmp.BonusCode.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbBonusSlabs.Select(0, BoSearchKey.psk_Index);
                }
                else
                {
                    cbBonusSlabs.Select(string.IsNullOrEmpty(oEmp.BonusCode) ? "-1" : oEmp.BonusCode, BoSearchKey.psk_ByValue);
                }
                //Payroll Tab
                if (!String.IsNullOrEmpty(oEmp.PayrollID.ToString()))
                {
                    cbPayroll.Select(oEmp.PayrollID != null ? oEmp.PayrollID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbPayroll.Select(oEmp.PayrollID != null ? oEmp.PayrollID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                if (!String.IsNullOrEmpty(oEmp.AttendanceAllowance.ToString()))
                {
                    cbAttendanceAllowance.Select(oEmp.AttendanceAllowance != null ? oEmp.AttendanceAllowance.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbAttendanceAllowance.Select(oEmp.AttendanceAllowance != null ? oEmp.AttendanceAllowance.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                if (string.IsNullOrEmpty(oEmp.DefaultOffDay))
                {
                    cmbEmpOffDay.Select("-1", BoSearchKey.psk_ByValue);
                }
                else
                {
                    cmbEmpOffDay.Select(oEmp.DefaultOffDay != null ? oEmp.DefaultOffDay.ToString().Trim() : "-1", BoSearchKey.psk_ByValue);
                }
                try
                {
                    if (!string.IsNullOrEmpty(oEmp.Dimension1))
                    {
                        cbDimension1.Select(Convert.ToString(oEmp.Dimension1).Trim(), BoSearchKey.psk_ByValue);
                    }
                    else
                    {
                        cbDimension1.Select(0, BoSearchKey.psk_Index);
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    if (!string.IsNullOrEmpty(oEmp.Dimension2))
                    {
                        cbDimension2.Select(Convert.ToString(oEmp.Dimension2).Trim(), BoSearchKey.psk_ByValue);
                    }
                    else
                    {
                        cbDimension2.Select(0, BoSearchKey.psk_Index);
                    }
                }
                catch (Exception ex)
                {

                }
                try
                {
                    if (!string.IsNullOrEmpty(oEmp.Dimension3))
                    {
                        cbDimension3.Select(Convert.ToString(oEmp.Dimension3).Trim(), BoSearchKey.psk_ByValue);
                    }
                    else
                    {
                        cbDimension3.Select(0, BoSearchKey.psk_Index);
                    }
                }
                catch (Exception ex)
                {

                }
                try
                {
                    if (!string.IsNullOrEmpty(oEmp.Dimension4))
                    {
                        cbDimension4.Select(Convert.ToString(oEmp.Dimension4).Trim(), BoSearchKey.psk_ByValue);
                    }
                    else
                    {
                        cbDimension4.Select(0, BoSearchKey.psk_Index);
                    }
                }
                catch (Exception ex)
                {

                }
                try
                {
                    if (!string.IsNullOrEmpty(oEmp.Dimension5))
                    {
                        cbDimension5.Select(Convert.ToString(oEmp.Dimension5).Trim(), BoSearchKey.psk_ByValue);
                    }
                    else
                    {
                        cbDimension5.Select(0, BoSearchKey.psk_Index);
                    }
                }
                catch (Exception ex)
                {

                }
                if (oEmp.FlgOffDayApplicable != null)
                {
                    oForm.DataSources.UserDataSources.Item(chkOffDay.DataBind.Alias).ValueEx = oEmp.FlgOffDayApplicable == true ? "Y" : "N";
                }
                else
                {
                    chkOffDay.Checked = false;
                }


                //Absence Tab

                dtAbsence.Rows.Clear();
                FillLeaveDataGrid(oEmp.ID);
                mtAbsence.LoadFromDataSource();

                //Communication Tab

                txtWorkIM.Value = oEmp.WorkIM;
                txtPersonalIM.Value = oEmp.PersonalIM;
                txtPersonalContact.Value = oEmp.PersonalContactNo;
                txtPersonalEmail.Value = oEmp.PersonalEmail;

                //Classification Tab

                txtOrganizationUnit.Value = oEmp.OrganizationalUnit;
                //cbReportingManager.Select(oEmp.ReportToID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                if (oEmp.ReportToID != null)
                {
                    if (oEmp.ReportToID != -1)
                    {
                        MstEmployee rptMng = (from a in dbHrPayroll.MstEmployee where a.ID == oEmp.ReportToID select a).FirstOrDefault();
                        txtReportTo.Value = rptMng.EmpID;
                    }
                }
                txtEmpContractType.Value = oEmp.EmployeeContractType;
                txtHRCalendar.Value = oEmp.HrBaseCalendar;
                txtWindowsLogin.Value = oEmp.WindowsLogin;
                txtEmpGrade.Value = oEmp.EmployeeGrade.ToString();
                txtPreviosEmpMonth.Value = oEmp.PreEmpMonth;
                txtWorkPermitRef.Value = oEmp.WorkPermitRef;
                if (oEmp.WorkPermitExpiryDate != null)
                {
                    if (oEmp.WorkPermitExpiryDate > DateTime.MinValue)
                    {
                        txtWorkPermitExpiry.Value = Convert.ToDateTime(oEmp.WorkPermitExpiryDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtWorkPermitExpiry.Value = "";
                    }
                }
                else
                {
                    txtWorkPermitExpiry.Value = "";
                }
                if (oEmp.ContractExpiryDate != null)
                {
                    if (oEmp.ContractExpiryDate > DateTime.MinValue)
                    {
                        txtContractExpiry.Value = Convert.ToDateTime(oEmp.ContractExpiryDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                    }
                }
                else
                {
                    txtContractExpiry.Value = "";
                }

                //Arabic Tab Read Only Fields

                if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                {
                    txtEnglishName.Value = oEmp.EnglishName != null ? oEmp.EnglishName : "";
                    txtArabicName.Value = oEmp.ArabicName != null ? oEmp.ArabicName : "";
                    txtPassportExpiryDtH.Value = oEmp.PassportExpiryDt != null ? oEmp.PassportExpiryDt : "";
                    txtIDExpiryDtH.Value = oEmp.IDExpiryDt != null ? oEmp.IDExpiryDt : "";
                    txtMedicalCardExpirydtH.Value = oEmp.MedicalCardExpDt != null ? oEmp.MedicalCardExpDt : "";
                    txtDrvLicCompletionDtH.Value = oEmp.DrvLicCompletionDt != null ? oEmp.DrvLicCompletionDt : "";
                    txtDrvLicLastDtH.Value = oEmp.DrvLicLastDt != null ? oEmp.DrvLicLastDt : "";
                    txtDrvLicReleaseDtH.Value = oEmp.DrvLicReleaseDt != null ? oEmp.DrvLicReleaseDt : "";
                    txtVisaNumber.Value = oEmp.VisaNo != null ? oEmp.VisaNo : "";
                    txtIqamaProfessional.Value = oEmp.IqamaProfessional != null ? oEmp.IqamaProfessional : "";
                    txtBankCardExpiryDtH.Value = oEmp.BankCardExpiryDt != null ? oEmp.BankCardExpiryDt : "";
                }

                //Attachement Tab
                if (!String.IsNullOrEmpty(oEmp.Remarks))
                {
                    txtRemarks.Value = oEmp.Remarks;
                }
                else
                {
                    txtRemarks.Value = "";
                }
                //itxtEmployeeCode.Enabled = false;
                //btnMain.Caption = "Update";
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                flgFormMode = false;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Employee Doesn't load Successfully. : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //btnMain.Caption = "Update";
            }
            oForm.Freeze(false);
        }

        private bool ValidateData()
        {
            bool result = true;
            //Variable Section 
            String UserCode;
            String Password;
            String FirstName, LastName, EmpCode;
            String Department, Designation, Location, Payroll, ContractType, BasicSalary, DateOfJoining, AttendanceAllowance, BonusSlab;
            var oEmpDB = (from a in dbHrPayroll.MstEmployee where a.ID == loadDocument select a).FirstOrDefault();
            UserCode = txtUserCode.Value.Trim();
            Password = txtPassword.Value.Trim();
            FirstName = txtFirstName.Value.Trim();
            LastName = txtLastName.Value.Trim();
            Department = cbDepartment.Value.Trim();
            Designation = cbDesignation.Value.Trim();
            Payroll = cbPayroll.Value.Trim();
            AttendanceAllowance = cbAttendanceAllowance.Value.Trim();
            BonusSlab = cbBonusSlabs.Value.Trim();
            Location = cbLocation.Value.Trim();
            ContractType = cbcontractType.Value.Trim();
            BasicSalary = txtBasicSalary.Value.Trim();
            DateOfJoining = txtDateOfJoining.Value.Trim();
            EmpCode = txtEmployeeCode.Value.Trim();
            if (string.IsNullOrEmpty(EmpCode))
            {
                result = false;
                oApplication.StatusBar.SetText("Employee Code is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            else
            {
                if (oEmpDB != null)
                {
                    if (EmpCode != oEmpDB.EmpID)
                    {
                        result = false;
                        oApplication.StatusBar.SetText("You can't change employee code of existing employee.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    }
                }
            }
            if (String.IsNullOrEmpty(UserCode))
            {
                result = false;
                oApplication.StatusBar.SetText("UserCode is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(Password))
            {
                result = false;
                oApplication.StatusBar.SetText("Password is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(FirstName))
            {
                result = false;
                oApplication.StatusBar.SetText("FirstName is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(LastName))
            {
                result = false;
                oApplication.StatusBar.SetText("LastName is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(Department) || Department == "-1")
            {
                result = false;
                oApplication.StatusBar.SetText("Department is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(Designation) || Designation == "-1")
            {
                result = false;
                oApplication.StatusBar.SetText("Designation is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(Payroll) || Payroll == "-1")
            {
                result = false;
                oApplication.StatusBar.SetText("Payroll is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(Location) || Location == "-1")
            {
                result = false;
                oApplication.StatusBar.SetText("Location is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(ContractType) || ContractType == "-1")
            {
                result = false;
                oApplication.StatusBar.SetText("ContractType is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            //if (String.IsNullOrEmpty(BasicSalary) || Convert.ToDecimal(BasicSalary) == 0)
            //{
            //    result = false;
            //    oApplication.StatusBar.SetText("BasicSalary is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            //}
            if (Convert.ToDecimal(BasicSalary) < 0)
            {
                result = false;
                oApplication.StatusBar.SetText("BasicSalary can't be a negative value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(DateOfJoining))
            {
                result = false;
                oApplication.StatusBar.SetText("DateOfJoining is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (flgFormMode)
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeCode.Value.Trim() select a).FirstOrDefault();
                if (oEmp != null)
                {
                    result = false;
                    oApplication.StatusBar.SetText("Same Employee Code already exist.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
            }

            //Only Checks When Employee is Already Created

            if (oEmpDB != null && result)
            {
                //Check Contract Status

                if (oEmpDB.EmployeeContractType != ContractType)
                {
                    Int32 ConfirmAnswer = oApplication.MessageBox("Are you sure you want to change Contract Type.", 2, "Yes", "No");
                    if (ConfirmAnswer == 2)
                    {
                        result = false;
                        oApplication.StatusBar.SetText("Record not updated you cancelled changes.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    }
                    else
                    {
                        result = true;
                    }
                }

                //Payroll Change
                if (Convert.ToString(oEmpDB.PayrollID) != Payroll)
                {
                    Int32 ConfirmAnswer = oApplication.MessageBox("Are you sure! You want to change Employee Payroll.", 2, "Yes", "No");
                    if (ConfirmAnswer == 2)
                    {
                        result = false;
                        oApplication.StatusBar.SetText("Record Not Updated you cancelled changes.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    }
                    else
                    {
                        result = true;
                    }
                }

                //Check Activity Check
                if (oEmpDB.FlgActive != chkActiveEmployee.Checked)
                {
                    Int32 ConfirmAnswer = oApplication.MessageBox("Are you sure! You want to change Employee Active Status.", 2, "Yes", "No");
                    if (ConfirmAnswer == 2)
                    {
                        result = false;
                        oApplication.StatusBar.SetText("Record Not Updated you cancelled changes.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    }
                    else
                    {
                        result = true;
                    }
                }
                //Effective Date Check
                if (!string.IsNullOrEmpty(txtEffectiveDate.Value))
                {
                    try
                    {
                        DateTime testeffectivedate = DateTime.ParseExact(txtEffectiveDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime testjoiningdate = DateTime.ParseExact(txtDateOfJoining.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (testeffectivedate < testjoiningdate)
                        {
                            oApplication.StatusBar.SetText("Effective Date must be greater then joining date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            result = false;
                        }
                    }
                    catch
                    {
                        oApplication.StatusBar.SetText("Effective date is not valid.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        result = false;
                    }
                }
                //DOB Validation
                if (!string.IsNullOrEmpty(txtDOB.Value))
                {
                    try
                    {
                        DateTime testdob = DateTime.ParseExact(txtDOB.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime testjoiningdate = DateTime.ParseExact(txtDateOfJoining.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (testdob > testjoiningdate)
                        {
                            oApplication.StatusBar.SetText("Date of Birth should be lesser then joining date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            result = false;
                        }
                    }
                    catch
                    {
                        oApplication.StatusBar.SetText("Effective date is not valid.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        result = false;
                    }
                }
                //id card unique value
                if (!string.IsNullOrEmpty(txtIDCardNo.Value))
                {
                    string strNicNumber = txtIDCardNo.Value.Trim();
                    #region NIC Duplication Check

                    var objOEmployee = (from p in dbHrPayroll.MstEmployee
                                        where p.EmpID == EmpCode
                                        select p).FirstOrDefault();
                    if (objOEmployee != null)
                    {
                        if (objOEmployee.IDNo == null)
                        {
                            objOEmployee.IDNo = strNicNumber;//dtMat.GetValue("IDNo", i);
                        }
                        else if (objOEmployee.IDNo == strNicNumber.Trim())
                        {
                            objOEmployee.IDNo = Convert.ToString(objOEmployee.IDNo);
                        }
                        else if (objOEmployee.IDNo != strNicNumber.Trim())
                        {
                            int oNICcnt = (from p in dbHrPayroll.MstEmployee
                                           where p.IDNo == strNicNumber.Trim()
                                           select p).Count();
                            if (oNICcnt > 0)
                            {
                                oApplication.StatusBar.SetText("ID card number must be unique.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                result = false;
                            }
                            else
                            {
                                objOEmployee.IDNo = strNicNumber;//dtMat.GetValue("IDNo", i);
                            }
                        }
                    }
                    else
                    {
                        int oNICcnt = (from p in dbHrPayroll.MstEmployee
                                       where p.IDNo == strNicNumber.Trim()
                                       select p).Count();
                        if (oNICcnt > 0)
                        {
                            oApplication.StatusBar.SetText("ID card number must be unique.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            result = false;
                        }
                        else
                        {
                            objOEmployee.IDNo = strNicNumber;//dtMat.GetValue("IDNo", i);
                        }
                    }
                    #endregion
                    //string currentvalue = txtIDCardNo.Value.Trim();
                    //var oCheckUniqueID = (from a in dbHrPayroll.MstEmployee where a.IDNo == currentvalue select a).Count();
                    //if (oCheckUniqueID >= 1)
                    //{
                    //    oApplication.StatusBar.SetText("ID card number must be unique.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //    result = false;
                    //}
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(txtIDCardNo.Value))
                {
                    string currentvalue = txtIDCardNo.Value.Trim();
                    var oCheckUniqueID = (from a in dbHrPayroll.MstEmployee where a.IDNo == currentvalue select a).Count();
                    if (oCheckUniqueID >= 1)
                    {
                        oApplication.StatusBar.SetText("ID card number must be unique.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        result = false;
                    }
                }
            }

            return result;
        }

        private void FillLeaveDataGrid(int EmployeeID)
        {
            int i = 0;
            String iApprovedCode = "LV0006", iDraftCode = "LV0005";
            try
            {
                if (EmployeeID > 0)
                {
                    //var GQuery = (from bb in dbHrPayroll.MstEmployeeLeaves
                    //              where bb.EmpID == EmployeeID && bb.FlgActive == true
                    //              join cb in dbHrPayroll.MstLeaveType on bb.LeaveType equals cb.ID
                    //              select new
                    //              {
                    //                  ID = bb.ID,
                    //                  BalanceBF = Convert.ToString(bb.LeavesCarryForward) != null ? Convert.ToString(bb.LeavesCarryForward) : "0.00",
                    //                  Entitled = Convert.ToString(bb.LeavesEntitled) != null ? Convert.ToString(bb.LeavesEntitled) : "0.00",
                    //                  LeaveID = bb.LeaveType,
                    //                  CauseofLeave = cb.Description,
                    //                  TotalAvailable = Convert.ToString(bb.LeavesCarryForward + bb.LeavesEntitled),
                    //                  Used = Convert.ToString(bb.LeavesUsed) != null ? Convert.ToString(bb.LeavesUsed) : "0.00",
                    //                  RequestedLeaves = Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iDraftCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) != null ? Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iDraftCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) : "0.00",
                    //                  ApprovedLeaves = Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iApprovedCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) != null ? Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iApprovedCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) : "0.00"
                    //              }).ToList();

                    String strQuery = @"SELECT  
	                                        m2.ID AS ID,
	                                        m2.LeavesCarryForward AS BalanceBF, 
	                                        m2.LeavesEntitled AS Entitled, 
	                                        m2.LeaveType AS LeaveID, 
	                                        m3.Description AS CauseofLeave, 
	                                        m2.LeavesCarryForward + m2.LeavesEntitled AS TotalAvailable, 
	                                        m2.LeavesUsed AS USED,
	                                        (SELECT ISNULL(SUM(ISNULL(TotalCount ,0)),0) AS TotalCount FROM dbo.TrnsLeavesRequest AS S2 WHERE S2.EmpID = m1.Id 	AND S2.LeaveType = m2.LeaveType AND S2.DocAprStatus = 'LV0005' AND S2.LeaveFrom >= (SELECT s1.StartDate FROM dbo.MstCalendar AS s1 WHERE ISNULL(s1.flgActive,0) = 1) AND S2.LeaveTo <= (SELECT s1.EndDate FROM dbo.MstCalendar AS s1 WHERE ISNULL(s1.flgActive,0) = 1)) AS RequestedLeaves,
	                                        (SELECT ISNULL(SUM(ISNULL(TotalCount ,0)),0) AS TotalCount FROM dbo.TrnsLeavesRequest AS S2 WHERE S2.EmpID = m1.Id 	AND S2.LeaveType = m2.LeaveType AND S2.DocAprStatus = 'LV0006' AND S2.LeaveFrom >= (SELECT s1.StartDate FROM dbo.MstCalendar AS s1 WHERE ISNULL(s1.flgActive,0) = 1) AND S2.LeaveTo <= (SELECT s1.EndDate FROM dbo.MstCalendar AS s1 WHERE ISNULL(s1.flgActive,0) = 1)) AS ApprovedLeaves
                                        FROM 
	                                        dbo.MstEmployee AS m1 INNER JOIN dbo.MstEmployeeLeaves AS m2 ON m1.ID = m2.EmpID
	                                        INNER JOIN dbo.MstLeaveType AS m3 ON m2.LeaveType = m3.ID
                                        WHERE 
	                                        m1.ID = " + EmployeeID + @"
	                                        AND m2.FromDt = (SELECT s1.StartDate FROM dbo.MstCalendar AS s1 WHERE ISNULL(s1.flgActive,0) = 1)
	                                        AND m2.ToDt = (SELECT s1.EndDate FROM dbo.MstCalendar AS s1 WHERE ISNULL(s1.flgActive,0) = 1)";

                    System.Data.DataTable dtResult = ds.getDataTable(strQuery);

                    //foreach (var WD in GQuery)
                    //{
                    //    decimal TotalAvailable = Convert.ToDecimal(WD.TotalAvailable);
                    //    decimal TotalUsed = Convert.ToDecimal(WD.Used);
                    //    decimal TotalApproved = Convert.ToDecimal(WD.ApprovedLeaves);
                    //    decimal TotalRequested = Convert.ToDecimal(WD.RequestedLeaves);
                    //    decimal RemainingTotal = TotalAvailable - (TotalUsed + TotalApproved + TotalRequested);
                    //    dtAbsence.Rows.Add(1);
                    //    dtAbsence.SetValue(aIsNew.DataBind.Alias, dtAbsence.Rows.Count - 1, "N");
                    //    dtAbsence.SetValue(aID.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.ID);
                    //    dtAbsence.SetValue(aDescription.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.CauseofLeave);
                    //    dtAbsence.SetValue(aBalanceBF.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.BalanceBF.ToString());
                    //    dtAbsence.SetValue(aEntitled.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.Entitled.ToString());
                    //    dtAbsence.SetValue(aTotalAvailable.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.TotalAvailable.ToString());
                    //    dtAbsence.SetValue(aUsed.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.Used.ToString());
                    //    dtAbsence.SetValue(aRequested.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.RequestedLeaves.ToString());
                    //    dtAbsence.SetValue(aApproved.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.ApprovedLeaves.ToString());
                    //    dtAbsence.SetValue(aBalance.DataBind.Alias, dtAbsence.Rows.Count - 1, RemainingTotal.ToString());
                    //}
                    foreach (DataRow drRow in dtResult.Rows)
                    {
                        decimal TotalAvailable = Convert.ToDecimal(drRow["TotalAvailable"]);
                        decimal TotalUsed = Convert.ToDecimal(drRow["Used"]);
                        decimal TotalApproved = Convert.ToDecimal(drRow["ApprovedLeaves"]);
                        decimal TotalRequested = Convert.ToDecimal(drRow["RequestedLeaves"]);
                        decimal RemainingTotal = TotalAvailable - (TotalApproved + TotalRequested);
                        dtAbsence.Rows.Add(1);
                        dtAbsence.SetValue(aIsNew.DataBind.Alias, dtAbsence.Rows.Count - 1, "N");
                        dtAbsence.SetValue(aID.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["ID"]));
                        dtAbsence.SetValue(aDescription.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["CauseofLeave"]));
                        dtAbsence.SetValue(aBalanceBF.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["BalanceBF"]));
                        dtAbsence.SetValue(aEntitled.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["Entitled"]));
                        dtAbsence.SetValue(aTotalAvailable.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["TotalAvailable"]));
                        dtAbsence.SetValue(aUsed.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["Used"]));
                        dtAbsence.SetValue(aRequested.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["RequestedLeaves"]));
                        dtAbsence.SetValue(aApproved.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(drRow["ApprovedLeaves"]));
                        dtAbsence.SetValue(aBalance.DataBind.Alias, dtAbsence.Rows.Count - 1, Convert.ToString(RemainingTotal));
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: FillLeaveDataGrid Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("emp.EmpID", txtEmployeeCode.Value.Trim());
            SearchKeyVal.Add("emp.FirstName", txtFirstName.Value.Trim());
            SearchKeyVal.Add("emp.MiddleName", txtMiddleName.Value.Trim());
            SearchKeyVal.Add("emp.LastName", txtLastName.Value.Trim());
            SearchKeyVal.Add("emp.IDNo", txtIDCardNo.Value.Trim());
            if (cbJobTitle.Selected.Value != "-1") SearchKeyVal.Add("emp.JobTitle", cbJobTitle.Selected.Description);
            if (cbDepartment.Selected.Description.Trim() != "-1") SearchKeyVal.Add("emp.DepartmentName", cbDepartment.Selected.Description.Trim());
            if (cbLocation.Selected.Description.Trim() != "-1") SearchKeyVal.Add("emp.LocationName", cbLocation.Selected.Description.Trim());
            if (cbPayroll.Selected.Description.Trim() != "-1") SearchKeyVal.Add("emp.PayrollName", cbPayroll.Selected.Description.Trim());
            if (cbcontractType.Value.Trim() != "-1") SearchKeyVal.Add("emp.EmployeeContractType", cbcontractType.Value.Trim());
            //if (cbShift.Selected.Description.Trim() != "-1") SearchKeyVal.Add("emp.EmployeeContractType", cbShift.Selected.Description.Trim());
            SearchKeyVal.Add("emp.FlgActive", chkActiveEmployee.Checked == true ? "1" : "");

        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empMaster";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewAttachmentForm()
        {
            try
            {
                Program.EmpID = "";
                System.Windows.Forms.Application.DoEvents();
                if (!string.IsNullOrEmpty(txtEmployeeCode.Value))
                {
                    Program.AttachEmpID = txtEmployeeCode.Value.Trim();
                }
                string comName = "EmpAttch";

                Program.sqlString = "empMaster";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == Program.EmpID).FirstOrDefault();
                    if (EmpRecord != null)
                    {
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        currentObjId = Convert.ToString(EmpRecord.ID);
                        loadDocument = EmpRecord.ID;
                        getRecord(currentObjId);
                        Program.EmpID = string.Empty;

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void doFind()
        {

            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("mstEmployee", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select  Employee");
            pic = null;
            if (st.Rows.Count > 0)
            {
                currentObjId = st.Rows[0][0].ToString();
                getRecord(currentObjId);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            }
            // btnMain.Caption = "Ok"
        }

        private void doSubmit()
        {
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
            {
                //doFind();
                flgManager = false;
                flgReportTo = false;
                OpenNewSearchForm();
                HideBasicSalary();
            }
            //else if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE || oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
            else// if ( btnMain.Caption == "Update" || btnMain.Caption == "Add")
            {
                SubmitRecord();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
        }

        private void SubmitRecord()
        {
            try
            {
                using (dbHRMS oDBObject = new dbHRMS(Program.ConStrHRMS))
                //if(true)
                {
                    //Variable 
                    //oDBObject
                    //dbHrPayroll
                    Int32 SBOEmpCode = 0;
                    string pFirstName, pMiddleName, pLastName;
                    string cFirstName, cMiddleName, cLastName;
                    bool pStatus = false, cStatus = false;
                    int pDept = 0, pBranch = 0, cDept = 0, cBranch = 0, pPos = 0, cPos = 0;
                    Boolean flgUpdateCheck = true;
                    //Check Wheather That employ exist in db
                    //then create an object 
                    MstEmployee oEmp = null;
                    MstUsers oUsr = null;
                    TrnsSalaryProcessRegister ProcessedSalary = null;
                    string PayrollIdx = cbPayroll.Value.Trim();
                    string AttendanceAllowanceIdx = cbAttendanceAllowance.Value.Trim();
                    string BonusSlabIdx = cbBonusSlabs.Value.Trim();
                    decimal OldBasicSalary = 0, OldGrossSalary = 0;
                    int cnt = (from p in oDBObject.MstEmployee where p.EmpID == txtEmployeeCode.Value.Trim() select p).Count();
                    if (cnt == 0)
                    {
                        oEmp = new MstEmployee();
                        oUsr = new MstUsers();
                        oEmp.EmpID = txtEmployeeCode.Value.Trim();
                        oEmp.CreatedBy = oCompany.UserName;
                        oEmp.CreateDate = DateTime.Now;
                        oEmp.UpdatedBy = oCompany.UserName;
                        oEmp.UpdateDate = DateTime.Now;
                        oEmp.IntSboPublished = false;
                        oEmp.IntSboTransfered = false;
                        oUsr.CreateDate = DateTime.Now;
                        oUsr.CreatedBy = oCompany.UserName;
                        Int32 MaxOrder = 0;
                        if (PayrollIdx != "-1")
                        {
                            var oPayRollCollection = (from a in oDBObject.CfgPayrollDefination where a.PayrollWiseSortOrder == 0 select a.ID).ToList();

                            CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination where a.ID == Convert.ToInt32(PayrollIdx) select a).FirstOrDefault();

                            if (Payroll.PayrollWiseSortOrder != null)
                            {
                                if (Payroll.PayrollWiseSortOrder == 0 && oPayRollCollection != null)
                                {
                                    MaxOrder = Convert.ToInt32((from a in oDBObject.MstEmployee where oPayRollCollection.Contains(Convert.ToInt32(a.PayrollID)) select a.SortOrder).Max());
                                }
                                else
                                {
                                    MaxOrder = Convert.ToInt32((from a in oDBObject.MstEmployee where a.PayrollID == Payroll.ID select a.SortOrder).Max());
                                }
                                if (MaxOrder == 0)
                                {
                                    MaxOrder = Convert.ToInt32(Payroll.PayrollWiseSortOrder);
                                }
                                else
                                {
                                    MaxOrder++;
                                }
                            }
                            else
                            {
                                MaxOrder = Convert.ToInt32((from a in oDBObject.MstEmployee select a.SortOrder).Max());
                                if (MaxOrder == 0)
                                {
                                    MaxOrder = 1;
                                }
                                else
                                {
                                    MaxOrder++;
                                }
                            }
                        }
                        oEmp.SortOrder = MaxOrder;
                        oEmp.MstUsers.Add(oUsr);
                        oDBObject.MstEmployee.InsertOnSubmit(oEmp);
                        flgUpdateCheck = false;
                    }
                    else
                    {
                        // oEmp = oEmployees.ElementAt(currentRecord); 
                        oDBObject.SubmitChanges();
                        oEmp = (from p in oDBObject.MstEmployee where p.ID.ToString() == currentObjId select p).FirstOrDefault();
                        oUsr = (from a in oDBObject.MstUsers where a.Empid == oEmp.ID select a).FirstOrDefault();
                        if (oUsr != null)
                        {
                            oUsr = oEmp.MstUsers.ElementAt(0);
                        }
                        else
                        {
                            oUsr = new MstUsers();
                            oUsr.CreateDate = DateTime.Now;
                            oUsr.CreatedBy = oCompany.UserName;
                            oEmp.MstUsers.Add(oUsr);
                        }
                        //Re verify the check.
                        oEmp.IntSboPublished = true;
                    }
                    #region Get Employee Payroll And unlocked Period
                    var oPeriod = (from p in dbHrPayroll.CfgPeriodDates
                                   where p.PayrollId == oEmp.PayrollID
                                   && p.FlgLocked == false
                                   select p).FirstOrDefault();
                    if (oPeriod == null)
                    {

                    }
                    ProcessedSalary = new TrnsSalaryProcessRegister();
                    if (oEmp != null && oEmp.PayrollID > 0)
                    {

                        ProcessedSalary = (from s in dbHrPayroll.TrnsSalaryProcessRegister
                                           where s.EmpID == oEmp.ID
                                           && s.PayrollID == oEmp.PayrollID
                                           && s.PayrollPeriodID == oPeriod.ID
                                           select s).FirstOrDefault();
                    }
                    #endregion
                    MstLanguages oLan = (from a in oDBObject.MstLanguages where a.Name.Contains(Program.sboLanguage) select a).FirstOrDefault<MstLanguages>();
                    oUsr.UserCode = string.IsNullOrEmpty(txtEmployeeCode.Value) ? txtEmployeeCode.Value.Trim() : txtUserCode.Value.Trim();
                    oUsr.UserID = string.IsNullOrEmpty(txtEmployeeCode.Value) ? txtEmployeeCode.Value.Trim() : txtUserCode.Value.Trim();
                    oUsr.MstLanguages = oLan;
                    oUsr.PassCode = txtPassword.Value.Trim() != "" ? txtPassword.Value.Trim() : "12345";
                    oUsr.Language = oLan.Id;
                    oUsr.FlgActiveUser = true;
                    oUsr.FlgWebUser = true;
                    oUsr.UpdateDate = DateTime.Now;
                    oUsr.UpdatedBy = oCompany.UserName;

                    cFirstName = txtFirstName.Value.Trim();
                    pFirstName = oEmp.FirstName;
                    oEmp.FirstName = cFirstName;
                    cMiddleName = txtMiddleName.Value.Trim();
                    pMiddleName = oEmp.MiddleName;
                    oEmp.MiddleName = cMiddleName;
                    cLastName = txtLastName.Value.Trim();
                    pLastName = oEmp.LastName;
                    oEmp.LastName = cLastName;
                    oEmp.SBOEmpCode = null;
                    oEmp.SboUserCode = cbSBOLinkID.Value.Trim() == "-1" ? "-1" : cbSBOLinkID.Value.Trim();
                    oEmp.FatherName = txtFatherName.Value.Trim();
                    if (cbJobTitle.Value.Trim() != "-1" && cbJobTitle.Value.Trim() != "")
                    {
                        oEmp.JobTitle = cbJobTitle.Value.Trim();
                    }
                    else
                    {
                        oEmp.JobTitle = null;
                    }
                    if (flgTax.Checked)
                    {
                        oEmp.FlgTax = true;
                    }
                    else
                    {
                        oEmp.FlgTax = false;
                    }
                    if (chkSup.Checked)
                    {
                        oEmp.FlgSuperVisor = true;
                    }
                    else
                    {
                        oEmp.FlgSuperVisor = false;
                    }
                    if (chkPerPiece.Checked)
                    {
                        oEmp.FlgPerPiece = true;
                    }
                    else
                    {
                        oEmp.FlgPerPiece = false;
                    }
                    if (cbPosition.Value.Trim() != "-1" && cbPosition.Value.Trim() != "")
                    {
                        Int32 PositionID = Convert.ToInt32(cbPosition.Value.Trim());
                        MstPosition oPosition = (from a in oDBObject.MstPosition where a.Id == PositionID select a).FirstOrDefault();
                        pPos = oEmp.PositionID.GetValueOrDefault();
                        cPos = PositionID;
                        oEmp.PositionID = oPosition.Id;
                        oEmp.PositionName = oPosition.Name;
                    }
                    else
                    {
                        oEmp.PositionID = null;
                    }
                    #region Department  update validation
                    if (ProcessedSalary != null)
                    {
                        if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                        {
                            var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                where je.ID == ProcessedSalary.JENum
                                                select je).FirstOrDefault();
                            if (PostedSalary != null)
                            {
                                if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                {
                                    if (cbDepartment.Value.Trim() != Convert.ToString(oEmp.DepartmentID).Trim())
                                    {
                                        oApplication.StatusBar.SetText("Department can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (cbDepartment.Value.Trim() != "-1")
                                    {
                                        Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                                        MstDepartment oDept = (from a in oDBObject.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                                        pDept = DeptID;
                                        cDept = oEmp.DepartmentID.GetValueOrDefault();
                                        oEmp.DepartmentID = oDept.ID;
                                        oEmp.DepartmentName = oDept.DeptName;
                                    }
                                    else
                                    {
                                        oEmp.DepartmentID = null;
                                    }
                                }
                            }
                            //else if (ProcessedSalary != null && PostedSalary == null)
                            else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                            {
                                if (cbDepartment.Value.Trim() != Convert.ToString(oEmp.DepartmentID).Trim())
                                {
                                    oApplication.StatusBar.SetText("Department can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }

                            }
                            else
                            {
                                if (cbDepartment.Value.Trim() != "-1")
                                {
                                    Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                                    MstDepartment oDept = (from a in oDBObject.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                                    pDept = DeptID;
                                    cDept = oEmp.DepartmentID.GetValueOrDefault();
                                    oEmp.DepartmentID = oDept.ID;
                                    oEmp.DepartmentName = oDept.DeptName;
                                }
                                else
                                {
                                    oEmp.DepartmentID = null;
                                }
                            }
                        }
                        else
                        {
                            if (cbDepartment.Value.Trim() != "-1")
                            {
                                Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                                MstDepartment oDept = (from a in oDBObject.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                                pDept = DeptID;
                                cDept = oEmp.DepartmentID.GetValueOrDefault();
                                oEmp.DepartmentID = oDept.ID;
                                oEmp.DepartmentName = oDept.DeptName;
                            }
                            else
                            {
                                oEmp.DepartmentID = null;
                            }
                        }
                    }
                    else
                    {
                        if (cbDepartment.Value.Trim() != "-1")
                        {
                            Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                            MstDepartment oDept = (from a in oDBObject.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                            pDept = DeptID;
                            cDept = oEmp.DepartmentID.GetValueOrDefault();
                            oEmp.DepartmentID = oDept.ID;
                            oEmp.DepartmentName = oDept.DeptName;
                        }
                        else
                        {
                            oEmp.DepartmentID = null;
                        }
                    }
                    #endregion
                    //if (cbDepartment.Value.Trim() != "-1")
                    //{
                    //    Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                    //    MstDepartment oDept = (from a in oDBObject.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                    //    oEmp.DepartmentID = oDept.ID;
                    //    oEmp.DepartmentName = oDept.DeptName;
                    //}
                    //else
                    //{
                    //    oEmp.DepartmentID = null;
                    //}
                    if (cbDesignation.Value.Trim() != "-1")
                    {
                        Int32 DesigID = Convert.ToInt32(cbDesignation.Value.Trim());
                        MstDesignation oDesig = (from a in oDBObject.MstDesignation where a.Id == DesigID select a).FirstOrDefault();
                        oEmp.DesignationID = oDesig.Id;
                        oEmp.DesignationName = oDesig.Name;
                    }
                    else
                    {
                        oEmp.DesignationID = null;
                        oEmp.DesignationName = "";
                    }

                    if (cbBranch.Value.Trim() != "-1" && cbBranch.Value.Trim() != "")
                    {
                        Int32 BranchID = Convert.ToInt32(cbBranch.Value.Trim());
                        cBranch = BranchID;
                        pBranch = oEmp.BranchID.GetValueOrDefault();
                        MstBranches oBranch = (from a in oDBObject.MstBranches where a.Id == BranchID select a).FirstOrDefault();
                        oEmp.BranchID = oBranch.Id;
                        oEmp.BranchName = oBranch.Name;
                    }
                    else
                    {
                        oEmp.BranchID = null;
                    }
                    //if (cbManager.Value.Trim() != "-1")
                    //{
                    //    oEmp.Manager = Convert.ToInt32(cbManager.Value);
                    //}
                    //else
                    //{
                    //    oEmp.Manager = null;
                    //}
                    if (!String.IsNullOrEmpty(txtManager.Value.Trim()))
                    {
                        MstEmployee oMng = (from a in oDBObject.MstEmployee where a.EmpID == txtManager.Value.Trim() select a).FirstOrDefault();
                        oEmp.Manager = oMng.ID;
                        oMng = null;
                    }
                    else
                    {
                        oEmp.Manager = null;
                    }
                    oEmp.FlgUser = true;
                    oEmp.FlgOTApplicable = chkOTApplicable.Checked;
                    if (!String.IsNullOrEmpty(txtEmployeeCode.Value))
                    {
                        oEmp.EmpID = txtEmployeeCode.Value;
                    }
                    else
                    {
                        oApplication.MessageBox(Program.objHrmsUI.getStrMsg("Inf_EmpID"), 1, "Ok");
                        return;
                    }

                    #region Location  update validation
                    if (ProcessedSalary != null)
                    {
                        if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                        {
                            var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                where je.ID == ProcessedSalary.JENum
                                                select je).FirstOrDefault();
                            if (PostedSalary != null)
                            {
                                if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                {
                                    if (cbLocation.Value.Trim() != Convert.ToString(oEmp.Location).Trim())
                                    {
                                        oApplication.StatusBar.SetText("Location can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (cbLocation.Value.Trim() != "-1" && cbLocation.Value.Trim() != "")
                                    {
                                        Int32 LocId = Convert.ToInt32(cbLocation.Value);
                                        MstLocation Location = (from a in oDBObject.MstLocation where a.Id == LocId select a).FirstOrDefault();
                                        oEmp.Location = Location.Id;
                                        oEmp.LocationName = Location.Name;
                                    }
                                    else
                                    {
                                        oEmp.Location = null;
                                        oEmp.LocationName = "";
                                    }
                                }
                            }
                            //else if (ProcessedSalary != null && PostedSalary == null)
                            else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                            {
                                if (cbLocation.Value.Trim() != Convert.ToString(oEmp.Location).Trim())
                                {
                                    oApplication.StatusBar.SetText("Location can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            else
                            {
                                if (cbLocation.Value.Trim() != "-1" && cbLocation.Value.Trim() != "")
                                {
                                    Int32 LocId = Convert.ToInt32(cbLocation.Value);
                                    MstLocation Location = (from a in oDBObject.MstLocation where a.Id == LocId select a).FirstOrDefault();
                                    oEmp.Location = Location.Id;
                                    oEmp.LocationName = Location.Name;
                                }
                                else
                                {
                                    oEmp.Location = null;
                                    oEmp.LocationName = "";
                                }
                            }
                        }
                        else
                        {
                            if (cbLocation.Value.Trim() != "-1" && cbLocation.Value.Trim() != "")
                            {
                                Int32 LocId = Convert.ToInt32(cbLocation.Value);
                                MstLocation Location = (from a in oDBObject.MstLocation where a.Id == LocId select a).FirstOrDefault();
                                oEmp.Location = Location.Id;
                                oEmp.LocationName = Location.Name;
                            }
                            else
                            {
                                oEmp.Location = null;
                                oEmp.LocationName = "";
                            }
                        }
                    }
                    else
                    {
                        if (cbLocation.Value.Trim() != "-1" && cbLocation.Value.Trim() != "")
                        {
                            Int32 LocId = Convert.ToInt32(cbLocation.Value);
                            MstLocation Location = (from a in oDBObject.MstLocation where a.Id == LocId select a).FirstOrDefault();
                            oEmp.Location = Location.Id;
                            oEmp.LocationName = Location.Name;
                        }
                        else
                        {
                            oEmp.Location = null;
                            oEmp.LocationName = "";
                        }
                    }
                    #endregion
                    //if (cbLocation.Value.Trim() != "-1" && cbLocation.Value.Trim() != "")
                    //{
                    //    Int32 LocId = Convert.ToInt32(cbLocation.Value);
                    //    MstLocation Location = (from a in oDBObject.MstLocation where a.Id == LocId select a).FirstOrDefault();
                    //    oEmp.Location = Location.Id;
                    //    oEmp.LocationName = Location.Name;
                    //}
                    //else
                    //{
                    //    oEmp.Location = null;
                    //    oEmp.LocationName = "";
                    //}
                    oEmp.Initials = txtInitials.Value.Trim();
                    oEmp.NamePrefix = txtNamePrefix.Value.Trim();
                    oEmp.OfficePhone = txtOfficePhn.Value.Trim();
                    oEmp.OfficeExtension = txtExtention.Value.Trim();
                    oEmp.OfficeMobile = txtMobilePhn.Value.Trim();
                    oEmp.Pager = txtPager.Value.Trim();
                    oEmp.HomePhone = txtHomePhn.Value.Trim();
                    oEmp.Fax = txtFax.Value.Trim();
                    oEmp.OfficeEmail = txtEmail.Value.Trim();
                    cStatus = chkActiveEmployee.Checked;
                    pStatus = oEmp.FlgActive.GetValueOrDefault();
                    oEmp.FlgActive = chkActiveEmployee.Checked;
                    oEmp.FlgCompanyResidence = chkCompanyResidence.Checked;
                    oEmp.CompanyAddress = txtCompanyResidence.Value.Trim();
                    //oEmp.FlgOTApplicable = chkOTApplicable.Checked;


                    //Employee Address
                    oEmp.WAStreet = txtHomeStreet.Value.Trim();
                    oEmp.WAStreetNo = txtHomeStreetNo.Value.Trim();
                    oEmp.WABlock = txtHomeBlock.Value.Trim();
                    oEmp.WAOther = txtHomeBuilding.Value.Trim();
                    oEmp.WAZipCode = txtHomeZip.Value.Trim();
                    oEmp.WACity = txtHomeCity.Value.Trim();
                    oEmp.WACountry = cbHomeCountry.Value != "-1" ? cbHomeCountry.Value.Trim() : null;
                    oEmp.WAState = cbHomeState.Value != "-1" ? cbHomeState.Value.Trim() : null;

                    oEmp.HAStreet = txtWorkStreet.Value.Trim();
                    oEmp.HAStreetNo = txtWorkStreetNo.Value.Trim();
                    oEmp.HABlock = txtWorkBlock.Value.Trim();
                    oEmp.HAOther = txtWorkBuilding.Value.Trim();
                    oEmp.HAZipCode = txtWorkZip.Value.Trim();
                    oEmp.HACity = txtWorkCity.Value.Trim();
                    oEmp.HACountry = cbWorkCountry.Value != "-1" ? cbWorkCountry.Value.Trim() : null;
                    oEmp.HAState = cbWorkState.Value != "-1" ? cbWorkState.Value.Trim() : null;

                    //Relatives
                    oEmp.PriPersonName = txtPriCntName.Value.Trim();
                    oEmp.PriRelationShip = txtPriCntRelation.Value.Trim();
                    oEmp.PriContactNoLandLine = txtPriCntNoLandLine.Value.Trim();
                    oEmp.PriContactNoMobile = txtPriCntNoMobile.Value.Trim();
                    oEmp.PriAddress = txtPriCntAddress.Value.Trim();
                    oEmp.PriCity = txtPriCntCity.Value.Trim();
                    oEmp.PriCountry = cbPriCntCountry.Value != "-1" ? cbPriCntCountry.Value.Trim() : null;
                    oEmp.PriState = cbPriCntState.Value != "-1" ? cbPriCntState.Value.Trim() : null;

                    oEmp.SecPersonName = txtSecCntName.Value.Trim();
                    oEmp.SecRelationShip = txtSecCntRelation.Value.Trim();
                    oEmp.SecContactNoLandline = txtSecCntNoLandLine.Value.Trim();
                    oEmp.SecContactNoMobile = txtSecCntNoMobile.Value.Trim();
                    oEmp.SecAddress = txtSecCntAddress.Value.Trim();
                    oEmp.SecCity = txtSecCntCity.Value.Trim();
                    oEmp.SecCountry = cbSecCntCountry.Value != "-1" ? cbSecCntCountry.Value.Trim() : null;
                    oEmp.SecState = cbSecCntState.Value != "-1" ? cbSecCntState.Value.Trim() : null;

                    oEmp.MartialStatusID = cbMartial.Value != "-1" ? cbMartial.Value.Trim() : null;
                    oEmp.MartialStatusLOVType = "Marital";
                    oEmp.ReligionID = cbReligion.Value != "-1" ? cbReligion.Value.Trim() : null;
                    oEmp.ReligionLOVType = "Religion";
                    oEmp.SocialSecurityNo = txtSSNumber.Value.Trim();
                    oEmp.EmpUnion = txtUnionMemberShip.Value.Trim();
                    oEmp.UnionMembershipNo = txtUnionMemberShipNo.Value.Trim();
                    oEmp.IDNo = txtIDCardNo.Value.Trim();
                    oEmp.TransportMode = cmbTransport.Value != "-1" ? cmbTransport.Value.Trim() : null;
                    oEmp.RecruitmentMode = cmbRecruitment.Value != "-1" ? cmbRecruitment.Value.Trim() : null;
                    oEmp.InsuranceCategory = cmbInsuranceCategory.Value != "-1" ? cmbInsuranceCategory.Value.Trim() : null;
                    //:TODO Issue
                    if (txtIDDtOfIssue.Value != "")
                    {
                        oEmp.IDDateofIssue = DateTime.ParseExact(txtIDDtOfIssue.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.IDDateofIssue = null;
                    }
                    if (txtIDExpiryDate.Value != "")
                    {
                        oEmp.IDExpiryDate = DateTime.ParseExact(txtIDExpiryDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.IDExpiryDate = null;
                    }
                    oEmp.IDIssuedBy = txtIDIssuedBy.Value.Trim();
                    oEmp.IDPlaceofIssue = txtIDPlaceOfIssue.Value.Trim();
                    oEmp.Nationality = txtNationality.Value.Trim();
                    oEmp.MotherName = txtMotherName.Value.Trim();
                    oEmp.PassportNo = txtPassportNo.Value.Trim();
                    //Modified
                    if (!string.IsNullOrEmpty(cbCostCenter.Value))
                    {
                        if (cbCostCenter.Value.Trim() != "-1")
                        {
                            oEmp.CostCenter = cbCostCenter.Value;
                            //oEmp.CostCenter = cbCostCenter.Value != "-1" ? cbCostCenter.Value.Trim() : null;
                        }
                        else
                        {
                            oEmp.CostCenter = null;
                        }
                    }
                    if (!string.IsNullOrEmpty(cbProject.Value))
                    {
                        if (cbProject.Value.Trim() != "-1")
                        {
                            oEmp.Project = cbProject.Value;
                        }
                        else
                        {
                            oEmp.Project = null;
                        }
                    }
                    oEmp.GenderID = cbGender.Value.Trim() != "-1" ? cbGender.Value.Trim() : null;

                    if (txtDOB.Value != "")
                    {
                        oEmp.DOB = DateTime.ParseExact(txtDOB.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.DOB = null;
                    }

                    if (txtTermination.Value != "")
                    {
                        oEmp.TerminationDate = DateTime.ParseExact(txtTermination.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.TerminationDate = null;
                    }

                    if (txtResignation.Value != "")
                    {
                        oEmp.ResignDate = DateTime.ParseExact(txtResignation.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.ResignDate = null;
                    }

                    if (txtPassportDateofIssue.Value != "")
                    {
                        oEmp.PassportDateofIssue = DateTime.ParseExact(txtPassportDateofIssue.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.PassportDateofIssue = null;
                    }
                    if (txtPassportExpiry.Value != "")
                    {
                        oEmp.PassportExpiryDate = DateTime.ParseExact(txtPassportExpiry.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.PassportExpiryDate = null;
                    }
                    oEmp.IncomeTaxNo = txtIncomeTax.Value.Trim();
                    if (!string.IsNullOrEmpty(cmbCategory.Value))
                    {
                        oEmp.Category = Convert.ToInt32(cmbCategory.Value.Trim());
                    }
                    else
                    {
                        oEmp.Category = null;
                    }
                    if (!string.IsNullOrEmpty(cmbSubCategory.Value))
                    {
                        oEmp.SubCategory = Convert.ToInt32(cmbSubCategory.Value.Trim());
                    }
                    else
                    {
                        oEmp.SubCategory = null;
                    }
                    //Salary Tab
                    #region Basic Salary update validation
                    if (ProcessedSalary != null)
                    {
                        if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                        {
                            var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                where je.ID == ProcessedSalary.JENum
                                                select je).FirstOrDefault();
                            if (PostedSalary != null)
                            {
                                if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                {
                                    if (Convert.ToDecimal(txtBasicSalary.Value) != Convert.ToDecimal(oEmp.BasicSalary))
                                    {
                                        oApplication.StatusBar.SetText("Basic Salary can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (txtBasicSalary.Value != "")
                                    {
                                        OldBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                                        oEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                                    }
                                    else
                                    {
                                        oEmp.BasicSalary = 0.0M;
                                    }
                                }
                            }
                            //else if (ProcessedSalary != null && PostedSalary == null)
                            else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                            {
                                if (Convert.ToDecimal(txtBasicSalary.Value) != Convert.ToDecimal(oEmp.BasicSalary))
                                {
                                    oApplication.StatusBar.SetText("Basic Salary can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            else
                            {
                                if (txtBasicSalary.Value != "")
                                {
                                    OldBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                                    oEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                                }
                                else
                                {
                                    oEmp.BasicSalary = 0.0M;
                                }
                            }
                        }
                        else
                        {
                            if (txtBasicSalary.Value != "")
                            {
                                OldBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                                oEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                            }
                            else
                            {
                                oEmp.BasicSalary = 0.0M;
                            }
                        }
                    }
                    else
                    {
                        if (txtBasicSalary.Value != "")
                        {
                            OldBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                            oEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                        }
                        else
                        {
                            oEmp.BasicSalary = 0.0M;
                        }
                    }
                    #endregion
                    //if (txtBasicSalary.Value != "")
                    //{
                    //    OldBasicSalary = Convert.ToDecimal(oEmp.BasicSalary);
                    //    oEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                    //}
                    //else
                    //{
                    //    oEmp.BasicSalary = 0.0M;
                    //}
                    if (txtGrossComputed.Value != "")
                    {
                        OldGrossSalary = Convert.ToDecimal(oEmp.GrossSalary);
                        oEmp.GrossSalary = Convert.ToDecimal(txtGrossComputed.Value.Trim());
                    }
                    else
                    {
                        oEmp.GrossSalary = 0M;
                    }
                    if (!string.IsNullOrEmpty(txGosi.Value))
                    {
                        oEmp.GosiSalary = Convert.ToDecimal(txGosi.Value.Trim());
                    }
                    if (!string.IsNullOrEmpty(txGosiV.Value))
                    {
                        oEmp.GosiSalaryV = Convert.ToDecimal(txGosiV.Value.Trim());
                    }
                    else
                    {
                        oEmp.GosiSalary = 0.0M;
                    }
                    //if (!string.IsNullOrEmpty(cbShift.Value) && Convert.ToInt32(cbShift.Value.Trim()) > 0)
                    //{
                    //    string PayrollId = cbPayroll.Value.Trim();
                    //    if (PayrollId != "-1")
                    //    {
                    //        if (oEmp.ID > 0)
                    //        {
                    //            //CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination where a.ID == Convert.ToInt32(PayrollId) select a).FirstOrDefault();
                    //            //var oOldVal = oDBObject.TrnsAttendanceRegister.Where(tr => tr.EmpID == oEmp.ID && tr.Date == DateTime.Now.Date).FirstOrDefault();
                    //            //var PeriodId = oDBObject.CfgPeriodDates.Where(pd => pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate && pd.PayrollId == Payroll.ID).FirstOrDefault();
                    //            //if (oOldVal != null)
                    //            //{
                    //            //    oOldVal.PeriodID = PeriodId.ID;
                    //            //    oOldVal.ShiftID = Convert.ToInt32(cbShift.Value.Trim());
                    //            //    oOldVal.UpdateDate = DateTime.Now;
                    //            //}
                    //            //else
                    //            //{
                    //            //    TrnsAttendanceRegister attendance = new TrnsAttendanceRegister();
                    //            //    attendance.EmpID = oEmp.ID;
                    //            //    attendance.PeriodID = PeriodId.ID;
                    //            //    attendance.Date = DateTime.Now.Date;
                    //            //    attendance.ShiftID = Convert.ToInt32(cbShift.Value);
                    //            //    attendance.CreateDate = DateTime.Now;
                    //            //    attendance.UserId = oCompany.UserName;
                    //            //    attendance.Processed = false;
                    //            //    oDBObject.TrnsAttendanceRegister.InsertOnSubmit(attendance);
                    //            //}
                    //        }
                    //    }
                    //}
                    if (cmbProfitCenter.Value.Trim() == "-1")
                    {
                        oEmp.ProfitCenter = "";
                    }
                    else
                    {
                        oEmp.ProfitCenter = cmbProfitCenter.Value.Trim();
                    }
                    oEmp.FlgEmail = chkEmailSalarySlip.Checked;
                    oEmp.FlgSandwich = chkSandwichLeaves.Checked;
                    oEmp.FlgBlackListed = chkBlackListed.Checked;
                    oEmp.SalaryCurrency = cbSalaryCurrency.Value.Trim();//txtSalaryCurrency.Value.Trim();
                    oEmp.EmpCalender = txtEmpCalendar.Value.Trim();
                    oEmp.EmployeeContractType = cbcontractType.Value.Trim();
                    //UpdateEmp.shift Read Only Field in UI
                    oEmp.AccountTitle = txtAccountTitle.Value.Trim();
                    oEmp.AccountNo = txtAccountNo.Value.Trim();
                    oEmp.BankName = txtBankName.Value.Trim();
                    oEmp.BankBranch = txtBankBranch.Value.Trim();
                    if (UFFU.mFm.IsDecimal(txtAllowedAdvance.Value))
                    {
                        oEmp.AllowedAdvance = Convert.ToDecimal(txtAllowedAdvance.Value);
                    }
                    else
                    {
                        oEmp.AllowedAdvance = 0;
                    }
                    if (!string.IsNullOrEmpty(txtPercentage.Value))
                    {
                        oEmp.PercentagePaid = Convert.ToDecimal(txtPercentage.Value.Trim());
                    }
                    else
                    {
                        oEmp.PercentagePaid = 0M;
                    }
                    oEmp.AccountType = cbAccountType.Value.Trim();
                    if (txtEffectiveDate.Value != "")
                    {
                        oEmp.EffectiveDate = DateTime.ParseExact(txtEffectiveDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.EffectiveDate = null;
                    }
                    if (txtDateOfJoining.Value != "")
                    {
                        oEmp.JoiningDate = DateTime.ParseExact(txtDateOfJoining.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.JoiningDate = null;
                    }
                    oEmp.BloodGroupID = cbBloodGroup.Value.Trim() == "-1" ? null : cbBloodGroup.Value.Trim();
                    oEmp.BloodGroupLOVType = "BloodGroup";
                    oEmp.PaymentMode = cbPaymentMode.Value.Trim() == "-1" ? null : cbPaymentMode.Value.Trim();
                    //int dedrulecheck = cmbDeductionRule.Value.Trim() == "-1" ? 0 : Convert.ToInt32(cmbDeductionRule.Value.Trim());
                    //if (dedrulecheck == 0)
                    //{
                    //    oEmp.DeductionRules = null;
                    //}
                    //else
                    //{
                    //    oEmp.DeductionRules = dedrulecheck;
                    //}
                    string gratuityvalue = cbGratuity.Value;
                    if (!string.IsNullOrEmpty(gratuityvalue))
                    {
                        if (gratuityvalue != "-1")
                        {
                            var oGratuity = (from a in oDBObject.TrnsGratuitySlabs where a.InternalID.ToString() == gratuityvalue select a).FirstOrDefault();
                            if (oGratuity != null)
                            {
                                oEmp.TrnsGratuitySlabs = oGratuity;
                            }
                            else
                            {
                                oEmp.GratuitySlabs = null;
                            }
                        }
                        else
                        {
                            oEmp.GratuitySlabs = null;
                        }
                    }

                    string otslabvalue = cmbOTSlabs.Value;
                    if (!string.IsNullOrEmpty(otslabvalue))
                    {
                        if (otslabvalue != "-1")
                        {
                            var oOTSlab = (from a in oDBObject.TrnsOTSlab where a.InternalID.ToString() == otslabvalue select a).FirstOrDefault();
                            if (oOTSlab != null)
                            {
                                oEmp.TrnsOTSlab = oOTSlab;
                            }
                            else
                            {
                                oEmp.OTSlabs = null;
                            }
                        }
                        else
                        {
                            oEmp.OTSlabs = null;
                        }
                    }

                    string shiftdayvalue = cmbShiftDaySlabs.Value;
                    if (!string.IsNullOrEmpty(shiftdayvalue))
                    {
                        if (shiftdayvalue != "-1")
                        {
                            var oShiftDays = (from a in oDBObject.MstShiftDays where a.Code.ToString() == shiftdayvalue.Trim() select a).FirstOrDefault();
                            if (oShiftDays != null)
                            {
                                oEmp.ShiftDaysCode = oShiftDays.Code;
                            }
                            else
                            {
                                oEmp.ShiftDaysCode = "";
                            }
                        }
                        else
                        {
                            oEmp.ShiftDaysCode = "";
                        }
                    }

                    if (BonusSlabIdx != "-1" && BonusSlabIdx != "")
                    {
                        MstBonusYearly oBonus = (from a in oDBObject.MstBonusYearly
                                                 where a.DocNo == Convert.ToInt32(BonusSlabIdx)
                                                 && a.FlgActive == true
                                                 select a).FirstOrDefault();


                        oEmp.BonusCode = Convert.ToString(oBonus.DocCode);
                    }
                    else
                    {
                        oEmp.BonusCode = null;
                    }
                    //Absence Matrix Data
                    //its a report no data has been added from this screen.
                    //

                    //Communication Tab

                    oEmp.WorkIM = txtWorkIM.Value.Trim();
                    oEmp.PersonalIM = txtPersonalIM.Value.Trim();
                    oEmp.PersonalEmail = txtPersonalEmail.Value.Trim();
                    oEmp.PersonalContactNo = txtPersonalContact.Value.Trim();

                    //Classification Tab
                    oEmp.OrganizationalUnit = txtOrganizationUnit.Value.Trim();
                    //if (cbReportingManager.Value.Trim() != "")
                    //{
                    //    oEmp.ReportToID = Convert.ToInt32(cbReportingManager.Value.Trim());
                    //}
                    if (!String.IsNullOrEmpty(txtReportTo.Value.Trim()))
                    {
                        MstEmployee oRpt = (from a in oDBObject.MstEmployee where a.EmpID == txtReportTo.Value.Trim() select a).FirstOrDefault();
                        oEmp.ReportToID = oRpt.ID;
                        oRpt = null;
                    }
                    else
                    {
                        oEmp.ReportToID = null;
                    }
                    //oEmp.EmployeeContractType = txtEmpContractType.Value.Trim();
                    oEmp.HrBaseCalendar = txtHRCalendar.Value.Trim();
                    oEmp.WindowsLogin = txtWindowsLogin.Value.Trim();
                    if (txtEmpGrade.Value != "")
                    {
                        oEmp.EmployeeGrade = Convert.ToInt32(txtEmpGrade.Value.Trim());
                    }
                    else
                    {
                        oEmp.EmployeeGrade = null;
                    }
                    oEmp.PreEmpMonth = txtPreviosEmpMonth.Value.Trim();
                    oEmp.WorkPermitRef = txtWorkPermitRef.Value.Trim();
                    if (txtWorkPermitExpiry.Value != "")
                    {
                        oEmp.WorkPermitExpiryDate = DateTime.ParseExact(txtWorkPermitExpiry.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.WorkPermitExpiryDate = null;
                    }
                    if (txtContractExpiry.Value != "")
                    {
                        oEmp.ContractExpiryDate = DateTime.ParseExact(txtContractExpiry.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oEmp.ContractExpiryDate = null;
                    }

                    //Arabic Section 
                    //Only Executable or Visible when Arabic Language is Selection in SAP B1

                    if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                    {
                        oEmp.EnglishName = txtEnglishName.Value.Trim() != "" ? txtEnglishName.Value.Trim() : null;
                        oEmp.ArabicName = txtArabicName.Value.Trim() != "" ? txtArabicName.Value.Trim() : null;
                        oEmp.PassportExpiryDt = txtPassportExpiryDtH.Value.Trim() != "" ? txtPassportExpiryDtH.Value.Trim() : null;
                        oEmp.IDExpiryDt = txtIDExpiryDtH.Value.Trim() != "" ? txtIDExpiryDtH.Value.Trim() : null;
                        oEmp.MedicalCardExpDt = txtMedicalCardExpirydtH.Value.Trim() != "" ? txtMedicalCardExpirydtH.Value.Trim() : null;
                        oEmp.DrvLicCompletionDt = txtDrvLicCompletionDtH.Value.Trim() != "" ? txtDrvLicCompletionDtH.Value.Trim() : null;
                        oEmp.DrvLicLastDt = txtDrvLicLastDtH.Value.Trim() != "" ? txtDrvLicLastDtH.Value.Trim() : null;
                        oEmp.DrvLicReleaseDt = txtDrvLicReleaseDtH.Value.Trim() != "" ? txtDrvLicReleaseDtH.Value.Trim() : null;
                        oEmp.VisaNo = txtVisaNumber.Value.Trim() != "" ? txtVisaNumber.Value.Trim() : null;
                        oEmp.IqamaProfessional = txtIqamaProfessional.Value.Trim() != "" ? txtIqamaProfessional.Value.Trim() : null;
                        oEmp.BankCardExpiryDt = txtBankCardExpiryDtH.Value.Trim() != "" ? txtBankCardExpiryDtH.Value.Trim() : null;
                    }

                    //Payroll Assignment 

                    #region Payroll update validation
                    if (ProcessedSalary != null)
                    {
                        if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                        {
                            var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                where je.ID == ProcessedSalary.JENum
                                                select je).FirstOrDefault();
                            if (PostedSalary != null)
                            {
                                if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                {
                                    if (PayrollIdx != Convert.ToString(oEmp.PayrollID).Trim())
                                    {
                                        oApplication.StatusBar.SetText("Payroll can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (PayrollIdx != "-1")
                                    {
                                        CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination
                                                                        where a.ID == Convert.ToInt32(PayrollIdx)
                                                                        select a).FirstOrDefault();
                                        //oEmp.PayrollID = Payroll.ID;
                                        oEmp.CfgPayrollDefination = Payroll;
                                        oEmp.PayrollName = Payroll.PayrollName;
                                    }
                                    else
                                    {
                                        oEmp.PayrollID = null;
                                        oEmp.PayrollName = "";
                                    }
                                }
                            }
                            //else if (ProcessedSalary != null && PostedSalary == null)
                            else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                            {
                                if (PayrollIdx != Convert.ToString(oEmp.PayrollID).Trim())
                                {
                                    oApplication.StatusBar.SetText("Payroll can't be updated of Salary processed Employee '" + oEmp.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            else
                            {
                                if (PayrollIdx != "-1")
                                {
                                    CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination
                                                                    where a.ID == Convert.ToInt32(PayrollIdx)
                                                                    select a).FirstOrDefault();
                                    //oEmp.PayrollID = Payroll.ID;
                                    oEmp.CfgPayrollDefination = Payroll;
                                    oEmp.PayrollName = Payroll.PayrollName;
                                }
                                else
                                {
                                    oEmp.PayrollID = null;
                                    oEmp.PayrollName = "";
                                }
                            }
                        }
                        else
                        {
                            if (PayrollIdx != "-1")
                            {
                                CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination
                                                                where a.ID == Convert.ToInt32(PayrollIdx)
                                                                select a).FirstOrDefault();
                                //oEmp.PayrollID = Payroll.ID;
                                oEmp.CfgPayrollDefination = Payroll;
                                oEmp.PayrollName = Payroll.PayrollName;
                            }
                            else
                            {
                                oEmp.PayrollID = null;
                                oEmp.PayrollName = "";
                            }
                        }
                    }
                    else
                    {
                        if (PayrollIdx != "-1")
                        {
                            CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination
                                                            where a.ID == Convert.ToInt32(PayrollIdx)
                                                            select a).FirstOrDefault();
                            //oEmp.PayrollID = Payroll.ID;
                            oEmp.CfgPayrollDefination = Payroll;
                            oEmp.PayrollName = Payroll.PayrollName;
                        }
                        else
                        {
                            oEmp.PayrollID = null;
                            oEmp.PayrollName = "";
                        }
                    }
                    #endregion

                    //if (PayrollIdx != "-1")
                    //{
                    //    CfgPayrollDefination Payroll = (from a in oDBObject.CfgPayrollDefination 
                    //                                    where a.ID == Convert.ToInt32(PayrollIdx) 
                    //                                    select a).FirstOrDefault();
                    //    //oEmp.PayrollID = Payroll.ID;
                    //    oEmp.CfgPayrollDefination = Payroll;
                    //    oEmp.PayrollName = Payroll.PayrollName;
                    //}
                    //else
                    //{
                    //    oEmp.PayrollID = null;
                    //    oEmp.PayrollName = "";
                    //}
                    if (AttendanceAllowanceIdx != "-1" && AttendanceAllowanceIdx != "")
                    {
                        MstAttendanceAllowance Attendance = (from a in oDBObject.MstAttendanceAllowance
                                                             where a.DocNo == Convert.ToInt32(AttendanceAllowanceIdx)
                                                             select a).FirstOrDefault();

                        oEmp.AttendanceAllowance = Attendance.DocNo;
                    }
                    else
                    {
                        oEmp.AttendanceAllowance = null;
                    }
                    string defaultoffday = cmbEmpOffDay.Value.Trim();
                    if (!string.IsNullOrEmpty(defaultoffday))
                    {
                        oEmp.DefaultOffDay = defaultoffday;
                    }
                    else
                    {
                        oEmp.DefaultOffDay = "";
                    }
                    oEmp.FlgOffDayApplicable = chkOffDay.Checked;
                    string dim1 = cbDimension1.Value.Trim();
                    if (!string.IsNullOrEmpty(dim1) && dim1 != "-1")
                    {
                        oEmp.Dimension1 = dim1;
                    }
                    else
                    {
                        oEmp.Dimension1 = "";
                    }
                    string dim2 = cbDimension2.Value.Trim();
                    if (!string.IsNullOrEmpty(dim2) && dim2 != "-1")
                    {
                        oEmp.Dimension2 = dim2;
                    }
                    else
                    {
                        oEmp.Dimension2 = "";
                    }
                    string dim3 = cbDimension3.Value.Trim();
                    if (!string.IsNullOrEmpty(dim3) && dim3 != "-1")
                    {
                        oEmp.Dimension3 = dim3;
                    }
                    else
                    {
                        oEmp.Dimension3 = "";
                    }
                    string dim4 = cbDimension4.Value.Trim();
                    if (!string.IsNullOrEmpty(dim4) && dim4 != "-1")
                    {
                        oEmp.Dimension4 = dim4;
                    }
                    else
                    {
                        oEmp.Dimension4 = "";
                    }
                    string dim5 = cbDimension5.Value.Trim();
                    if (!string.IsNullOrEmpty(dim5) && dim5 != "-1")
                    {
                        oEmp.Dimension5 = dim5;
                    }
                    else
                    {
                        oEmp.Dimension5 = "";
                    }
                    oEmp.Remarks = txtRemarks.Value.Trim();

                    //oEmp.IntSboPublished = false;
                    oEmp.UpdateDate = DateTime.Now;
                    oEmp.UpdatedBy = oCompany.UserName;
                    if (pFirstName != cFirstName)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    if (pMiddleName != cMiddleName)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    if (pLastName != cLastName)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    if (pStatus != cStatus)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    if (pDept != cDept)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    if (pPos != cPos)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    if (pBranch != cBranch)
                    {
                        oEmp.IntSboPublished = false;
                    }
                    //object obj = oDBObject.GetChangeSet();

                    oDBObject.SubmitChanges();

                    if (!flgUpdateCheck)
                    {
                        ds.updateStandardElements(oEmp.EmpID, true, Program.ConStrHRMS);
                    }
                    else if (flgUpdateCheck)
                    {
                        if (oEmp.BasicSalary != OldBasicSalary || oEmp.GrossSalary != OldGrossSalary
                            || oEmp.PayrollID != Convert.ToInt32(PayrollIdx))
                        {
                            ds.updateStandardElements(oEmp.EmpID, true, Program.ConStrHRMS);
                        }
                    }
                    if (!string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                    {
                        VerifyAllEmployeeShiftDays(oEmp);
                    }
                }
                oApplication.StatusBar.SetText("Recorded Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                GetData();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: Add/Update Employee Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private Int32 CreateWebUser(String pUserCode)
        {
            try
            {
                MstUsers oNew;
                int cnt = (from p in dbHrPayroll.MstUsers where p.UserID == pUserCode || p.UserCode == pUserCode select p).Count();
                if (cnt > 0)
                {
                    oNew = (from p in dbHrPayroll.MstUsers where p.UserID == pUserCode || p.UserCode == pUserCode select p).Single();
                }
                else
                {
                    oNew = new MstUsers();
                    dbHrPayroll.MstUsers.InsertOnSubmit(oNew);


                }


                Int32 returnvalue = 0;
                oNew.UserCode = pUserCode;
                oNew.UserID = pUserCode;
                oNew.PassCode = "1234";
                MstLanguages oLan = (from a in dbHrPayroll.MstLanguages where a.Name.Contains(Program.sboLanguage) select a).FirstOrDefault<MstLanguages>();
                oNew.Language = oLan.Id;
                oNew.FlgActiveUser = true;

                oNew.FlgWebUser = true;
                oNew.CreateDate = DateTime.Now;
                oNew.CreatedBy = oCompany.UserName;
                oNew.UpdateDate = DateTime.Now;
                oNew.UpdatedBy = oCompany.UserName;


                if (cnt > 0)
                {
                    returnvalue = oNew.ID;
                }
                else
                {
                    returnvalue = (from a in dbHrPayroll.MstUsers select a.ID).Last();
                }
                try
                {

                    dbHrPayroll.SubmitChanges();
                }
                catch { }
                return returnvalue;

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            return 0;
        }

        private void UpdateEmployee(String pCode)
        {
            try
            {
                //get the emp
                var UpdateEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID.Contains(pCode) select a).FirstOrDefault();

                if (UpdateEmp == null) return;
                //assign the values into emp
                UpdateEmp.FirstName = txtFirstName.Value.Trim();
                UpdateEmp.MiddleName = txtMiddleName.Value.Trim();
                UpdateEmp.LastName = txtLastName.Value.Trim();
                UpdateEmp.FatherName = txtFatherName.Value.Trim();
                UpdateEmp.JobTitle = txtJobTitle.Value.Trim();
                Int32 PositionID = Convert.ToInt32(cbPosition.Value.Trim());
                MstPosition oPosition = (from a in dbHrPayroll.MstPosition where a.Id == PositionID select a).FirstOrDefault();
                UpdateEmp.PositionID = oPosition.Id;
                UpdateEmp.PositionName = oPosition.Name;
                Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                MstDepartment oDept = (from a in dbHrPayroll.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                UpdateEmp.DepartmentID = oDept.ID;
                UpdateEmp.DepartmentName = oDept.DeptName;
                Int32 DesigId = Convert.ToInt32(cbDesignation.Value.Trim());
                MstDesignation oDesig = (from a in dbHrPayroll.MstDesignation where a.Id == DesigId select a).FirstOrDefault();
                UpdateEmp.DesignationID = oDesig.Id;
                UpdateEmp.DesignationName = oDesig.Name;
                Int32 BranchID = Convert.ToInt32(cbBranch.Value.Trim());
                MstBranches oBranch = (from a in dbHrPayroll.MstBranches where a.Id == BranchID select a).FirstOrDefault();
                UpdateEmp.BranchID = oBranch.Id;
                UpdateEmp.BranchName = oBranch.Name;
                UpdateEmp.Manager = Convert.ToInt32(cbManager.Value);

                Int32 LocId = Convert.ToInt32(cbLocation.Value);
                MstLocation Location = (from a in dbHrPayroll.MstLocation where a.Id == LocId select a).FirstOrDefault();
                UpdateEmp.Location = Location.Id;
                UpdateEmp.LocationName = Location.Name;
                UpdateEmp.Initials = txtInitials.Value.Trim();
                UpdateEmp.NamePrefix = txtNamePrefix.Value.Trim();
                UpdateEmp.OfficePhone = txtOfficePhn.Value.Trim();
                UpdateEmp.OfficeExtension = txtExtention.Value.Trim();
                UpdateEmp.OfficeMobile = txtMobilePhn.Value.Trim();
                UpdateEmp.Pager = txtPager.Value.Trim();
                UpdateEmp.HomePhone = txtHomePhn.Value.Trim();
                UpdateEmp.Fax = txtFax.Value.Trim();
                UpdateEmp.OfficeEmail = txtEmail.Value.Trim();
                UpdateEmp.FlgActive = chkActiveEmployee.Checked;
                UpdateEmp.FlgOTApplicable = chkOTApplicable.Checked;
                UpdateEmp.FlgTax = flgTax.Checked;
                UpdateEmp.FlgSuperVisor = chkSup.Checked;
                //if (chkCreateUser.Checked)
                //{
                //    String UsrCode = "";
                //    UsrCode = txtUserCode.Value.Trim();
                //    if (String.IsNullOrEmpty(UsrCode))
                //    {
                //        oApplication.SetStatusBarMessage("Enter or Select Usercode",SAPbouiCOM.BoMessageTime.bmt_Short, true);
                //        return;
                //    }
                //    UpdateEmp.FlgUser = chkCreateUser.Checked;
                //    UpdateEmp.UserCode = CreateWebUser(UsrCode);

                //}
                //else
                //{
                //    UpdateEmp.FlgUser = chkCreateUser.Checked;
                //}
                //Personal 
                UpdateEmp.WAStreet = txtWorkStreet.Value.Trim();
                UpdateEmp.WAStreetNo = txtWorkStreetNo.Value.Trim();
                UpdateEmp.WABlock = txtWorkBlock.Value.Trim();
                UpdateEmp.WAOther = txtWorkBuilding.Value.Trim();
                UpdateEmp.WAZipCode = txtWorkZip.Value.Trim();
                UpdateEmp.WACity = txtWorkCity.Value.Trim();
                UpdateEmp.WACountry = cbWorkCountry.Value.Trim();
                UpdateEmp.WAState = cbWorkState.Value.Trim();

                UpdateEmp.HAStreet = txtHomeStreet.Value.Trim();
                UpdateEmp.HAStreetNo = txtHomeStreetNo.Value.Trim();
                UpdateEmp.HABlock = txtHomeBlock.Value.Trim();
                UpdateEmp.HAOther = txtHomeBuilding.Value.Trim();
                UpdateEmp.HAZipCode = txtHomeZip.Value.Trim();
                UpdateEmp.HACity = txtHomeCity.Value.Trim();
                UpdateEmp.HACountry = cbHomeCountry.Value.Trim();
                UpdateEmp.HAState = cbHomeState.Value.Trim();

                //Relatives
                UpdateEmp.PriPersonName = txtPriCntName.Value.Trim();
                UpdateEmp.PriRelationShip = txtPriCntRelation.Value.Trim();
                UpdateEmp.PriContactNoLandLine = txtPriCntNoLandLine.Value.Trim();
                UpdateEmp.PriContactNoMobile = txtPriCntNoMobile.Value.Trim();
                UpdateEmp.PriAddress = txtPriCntAddress.Value.Trim();
                UpdateEmp.PriCity = txtPriCntCity.Value.Trim();
                UpdateEmp.PriCountry = cbPriCntCountry.Value.Trim();
                UpdateEmp.PriState = cbPriCntState.Value.Trim();

                UpdateEmp.SecPersonName = txtSecCntName.Value.Trim();
                UpdateEmp.SecRelationShip = txtSecCntRelation.Value.Trim();
                UpdateEmp.SecContactNoLandline = txtSecCntNoLandLine.Value.Trim();
                UpdateEmp.SecContactNoMobile = txtSecCntNoMobile.Value.Trim();
                UpdateEmp.SecAddress = txtSecCntAddress.Value.Trim();
                UpdateEmp.SecCity = txtSecCntCity.Value.Trim();
                UpdateEmp.SecCountry = cbSecCntCountry.Value.Trim();
                UpdateEmp.SecState = cbSecCntState.Value.Trim();

                UpdateEmp.MartialStatusID = cbMartial.Value.Trim();
                UpdateEmp.MartialStatusLOVType = "Marital";
                UpdateEmp.ReligionID = cbReligion.Value.Trim();
                UpdateEmp.ReligionLOVType = "Religion";
                UpdateEmp.SocialSecurityNo = txtSSNumber.Value.Trim();
                UpdateEmp.EmpUnion = txtUnionMemberShip.Value.Trim();
                UpdateEmp.UnionMembershipNo = txtUnionMemberShipNo.Value.Trim();
                UpdateEmp.IDNo = txtIDCardNo.Value.Trim();
                UpdateEmp.IDDateofIssue = DateTime.ParseExact(txtIDDtOfIssue.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.IDExpiryDate = DateTime.ParseExact(txtIDExpiryDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.IDIssuedBy = txtIDIssuedBy.Value.Trim();
                UpdateEmp.IDPlaceofIssue = txtIDPlaceOfIssue.Value.Trim();
                UpdateEmp.Nationality = txtNationality.Value.Trim();
                UpdateEmp.MotherName = txtMotherName.Value.Trim();
                UpdateEmp.PassportNo = txtPassportNo.Value.Trim();
                UpdateEmp.PassportDateofIssue = DateTime.ParseExact(txtPassportDateofIssue.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.PassportExpiryDate = DateTime.ParseExact(txtPassportExpiry.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.IncomeTaxNo = txtIncomeTax.Value.Trim();

                //Payroll Tab

                if (String.IsNullOrEmpty(cbPayroll.Value.Trim()))
                {
                    UpdateEmp.PayrollID = null;
                    UpdateEmp.PayrollName = "";
                }
                else
                {
                    CfgPayrollDefination oPayroll = (from a in dbHrPayroll.CfgPayrollDefination where a.ID == Convert.ToInt32(cbPayroll.Value.Trim()) select a).FirstOrDefault();
                    UpdateEmp.PayrollID = oPayroll.ID;
                    UpdateEmp.PayrollName = oPayroll.PayrollName;
                }
                if (String.IsNullOrEmpty(cbAttendanceAllowance.Value.Trim()))
                {
                    UpdateEmp.AttendanceAllowance = null;

                }
                else
                {
                    MstAttendanceAllowance oAllowance = (from a in dbHrPayroll.MstAttendanceAllowance
                                                         where a.DocNo == Convert.ToInt32(cbAttendanceAllowance.Value.Trim())
                                                         select a).FirstOrDefault();
                    UpdateEmp.AttendanceAllowance = oAllowance.DocNo;

                }
                //Salary Tab
                UpdateEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                UpdateEmp.GosiSalary = Convert.ToDecimal(txGosi.Value.Trim());
                UpdateEmp.GosiSalaryV = Convert.ToDecimal(txGosiV.Value.Trim());
                UpdateEmp.SalaryCurrency = cbSalaryCurrency.Value.Trim();//txtSalaryCurrency.Value.Trim();
                UpdateEmp.EmpCalender = txtEmpCalendar.Value.Trim();
                //UpdateEmp.shift Read Only Field in UI
                UpdateEmp.AccountTitle = txtAccountTitle.Value.Trim();
                UpdateEmp.AccountNo = txtAccountNo.Value.Trim();
                UpdateEmp.BankName = txtBankName.Value.Trim();
                UpdateEmp.BankBranch = txtBankBranch.Value.Trim();
                UpdateEmp.PercentagePaid = Convert.ToDecimal(txtPercentage.Value.Trim());
                UpdateEmp.AccountType = cbAccountType.Value.Trim();
                UpdateEmp.EffectiveDate = DateTime.ParseExact(txtEffectiveDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.JoiningDate = DateTime.ParseExact(txtDateOfJoining.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.BloodGroupID = cbBloodGroup.Value.Trim();
                UpdateEmp.BloodGroupLOVType = "BloodGroup";
                UpdateEmp.PaymentMode = cbPaymentMode.Value.Trim();
                UpdateEmp.FlgEmail = chkEmailSalarySlip.Checked;

                if (String.IsNullOrEmpty(cbBonusSlabs.Value.Trim()))
                {
                    UpdateEmp.BonusCode = null;

                }
                else
                {
                    MstBonusYearly oBonus = (from a in dbHrPayroll.MstBonusYearly
                                             where a.DocNo == Convert.ToInt32(cbBonusSlabs.Value.Trim())
                                             && a.FlgActive == true
                                             select a).FirstOrDefault();
                    UpdateEmp.BonusCode = oBonus.DocCode;

                }
                //Absence Matrix Data
                //its a report no data has been added from this screen.
                //

                //Communication Tab

                UpdateEmp.WorkIM = txtWorkIM.Value.Trim();
                UpdateEmp.PersonalIM = txtPersonalIM.Value.Trim();
                UpdateEmp.PersonalEmail = txtPersonalEmail.Value.Trim();
                UpdateEmp.PersonalContactNo = txtPersonalContact.Value.Trim();

                //Classification Tab
                UpdateEmp.OrganizationalUnit = txtOrganizationUnit.Value.Trim();
                if (String.IsNullOrEmpty(cbReportingManager.Value.Trim()))
                {
                    oApplication.SetStatusBarMessage("Assign Reporting Manager in Classification Tab.", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                    return;
                }
                UpdateEmp.ReportToID = Convert.ToInt32(cbReportingManager.Value.Trim());

                UpdateEmp.EmployeeContractType = txtEmpContractType.Value.Trim();
                UpdateEmp.HrBaseCalendar = txtHRCalendar.Value.Trim();
                UpdateEmp.WindowsLogin = txtWindowsLogin.Value.Trim();
                UpdateEmp.EmployeeGrade = Convert.ToInt32(txtEmpGrade.Value.Trim());
                UpdateEmp.PreEmpMonth = txtPreviosEmpMonth.Value.Trim();
                UpdateEmp.WorkPermitRef = txtWorkPermitRef.Value.Trim();
                UpdateEmp.WorkPermitExpiryDate = DateTime.ParseExact(txtWorkPermitExpiry.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.ContractExpiryDate = DateTime.ParseExact(txtContractExpiry.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                //Setting
                UpdateEmp.UpdatedBy = oCompany.UserName;
                UpdateEmp.UpdateDate = DateTime.Now;


                dbHrPayroll.SubmitChanges();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                //btnMain.Caption = "Ok";
                oApplication.StatusBar.SetText("Record Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: UpdateEmployee Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //btnMain.Caption = "Update";
            }
        }

        private void AddValidValuesInCombos()
        {
            cbJobTitle.ValidValues.Add("-1", "");
            cbLocation.ValidValues.Add("-1", "");
            cbPosition.ValidValues.Add("-1", "");
            cbDepartment.ValidValues.Add("-1", "");
            cbBranch.ValidValues.Add("-1", "");
            cbDesignation.ValidValues.Add("-1", "");
            cbSBOLinkID.ValidValues.Add("-1", "");
            cbWorkState.ValidValues.Add("-1", "");
            cbHomeState.ValidValues.Add("-1", "");
            cbPriCntState.ValidValues.Add("-1", "");
            cbSecCntState.ValidValues.Add("-1", "");
            cbWorkCountry.ValidValues.Add("-1", "");
            cbHomeCountry.ValidValues.Add("-1", "");
            cbPriCntCountry.ValidValues.Add("-1", "");
            cbSecCntCountry.ValidValues.Add("-1", "");
            cbOhemUser.ValidValues.Add("-1", "");
            cbGender.ValidValues.Add("-1", "");

            cbCostCenter.ValidValues.Add("-1", "");
            cbProject.ValidValues.Add("-1", "");
            cbReligion.ValidValues.Add("-1", "");
            cbMartial.ValidValues.Add("-1", "");
            cbBloodGroup.ValidValues.Add("-1", "");
            cmbCategory.ValidValues.Add("-1", "");
            cmbSubCategory.ValidValues.Add("-1", "");
            cmbRecruitment.ValidValues.Add("-1", "");
            cmbInsuranceCategory.ValidValues.Add("-1", "");
            cbSalaryCurrency.ValidValues.Add("-1", "");
            cbPaymentMode.ValidValues.Add("-1", "");
            cbAccountType.ValidValues.Add("-1", "");
            cbcontractType.ValidValues.Add("-1", "");
            cmbProfitCenter.ValidValues.Add("-1", "");
            cbGratuity.ValidValues.Add("-1", "");
            cmbOTSlabs.ValidValues.Add("-1", "");
            cmbShiftDaySlabs.ValidValues.Add("-1", "");
            cbBonusSlabs.ValidValues.Add("-1", "");
            cbPayroll.ValidValues.Add("-1", "");
            cbDimension1.ValidValues.Add("-1", "Select Distribution");
            cbDimension2.ValidValues.Add("-1", "Select Distribution");
            cbDimension3.ValidValues.Add("-1", "Select Distribution");
            cbDimension4.ValidValues.Add("-1", "Select Distribution");
            cbDimension5.ValidValues.Add("-1", "Select Distribution");
            cmbEmpOffDay.ValidValues.Add("-1", "");
            cbAttendanceAllowance.ValidValues.Add("-1", "");

        }

        private void FillPositionCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllPositions = from a in dbHrPayroll.MstPosition orderby a.Description ascending select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (var Position in AllPositions)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Position.Id), Convert.ToString(Position.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillJobTitleCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var Collection = from a in dbHrPayroll.MstJobTitle orderby a.Name ascending select a;
                // pCombo.ValidValues.Add("-1", "");
                foreach (var One in Collection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillDepartmentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllDepartment = from a in dbHrPayroll.MstDepartment orderby a.DeptName ascending select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (var Dept in AllDepartment)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillDesignationCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation orderby a.Name select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstDesignation One in Designations)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillBranchCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllBranches = from a in dbHrPayroll.MstBranches orderby a.Description ascending select a;
                // pCombo.ValidValues.Add("-1", "");
                foreach (var Branch in AllBranches)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Branch.Id), Convert.ToString(Branch.Description));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillManagerCombo(SAPbouiCOM.ComboBox pCombo)
        {

            IEnumerable<MstEmployee> AllEmployee = from a in dbHrPayroll.MstEmployee select a;
            //pCombo.ValidValues.Add("-1", "");
            foreach (MstEmployee Emp in AllEmployee)
            {
                pCombo.ValidValues.Add(Convert.ToString(Emp.ID), Convert.ToString(Emp.FirstName + " " + Emp.MiddleName + " " + Emp.LastName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillUserCodeCombo(SAPbouiCOM.ComboBox pCombo)
        {
            var AllUsers = from a in dbHrPayroll.MstUsers orderby a.UserCode ascending select a;
            // pCombo.ValidValues.Add("-1", "");
            foreach (var Usr in AllUsers)
            {
                pCombo.ValidValues.Add(Convert.ToString(Usr.ID), Convert.ToString(Usr.UserCode));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillPayrollCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var PayrollAll = from a in dbHrPayroll.CfgPayrollDefination select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (var Prl in PayrollAll)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Prl.ID), Convert.ToString(Prl.PayrollName));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillAttendanceAllowanceCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var uniqueAllowance = (from att in dbHrPayroll.MstAttendanceAllowance
                                       where att.FlgActive == true
                                       select new
                                       {
                                           ID = att.ID,
                                           Code = att.Code,
                                           DocNumber = att.DocNo
                                       }).GroupBy(x => x.DocNumber).ToList();

                //pCombo.ValidValues.Add("-1", "");
                foreach (var Allw in uniqueAllowance)
                {
                    var DocumentNumber = Allw.Where(a => a.DocNumber == a.DocNumber).FirstOrDefault();
                    if (DocumentNumber != null)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(DocumentNumber.DocNumber), Convert.ToString(DocumentNumber.Code));
                    }
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillBonusCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oBonus = (from b in dbHrPayroll.MstBonusYearly
                              where b.FlgActive == true
                              select new
                              {
                                  ID = b.ID,
                                  DocCode = b.DocCode,
                                  DocNumber = b.DocNo
                              }).GroupBy(x => x.DocNumber).ToList();

                //pCombo.ValidValues.Add("-1", "");
                foreach (var BonusCode in oBonus)
                {
                    var DocumentNumber = BonusCode.Where(a => a.DocNumber == a.DocNumber).FirstOrDefault();
                    if (DocumentNumber != null)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(DocumentNumber.DocNumber), Convert.ToString(DocumentNumber.DocCode));
                    }
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillProfitCenterCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstProfitCenter select a).ToList();
                //pCombo.ValidValues.Add("-1", "Select Value.");
                foreach (var Line in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Line.Code), Convert.ToString(Line.Description));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillProfitCenterCombo Ex : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillShiftCombo()
        {
            try
            {
                var ShiftMaster = dbHrPayroll.MstShifts.Where(s => s.StatusShift == true).ToList();
                if (ShiftMaster != null && ShiftMaster.Count > 0)
                {
                    // cbShift.ValidValues.Add("-1", "");
                    foreach (var Prl in ShiftMaster)
                    {
                        cbShift.ValidValues.Add(Convert.ToString(Prl.Code), Convert.ToString(Prl.Description));
                    }
                    cbShift.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillContractTypeCombo()
        {
            try
            {
                var ContractType = dbHrPayroll.MstLOVE.Where(lv => lv.Type == "ContractType").ToList();
                if (ContractType != null && ContractType.Count > 0)
                {
                    //cbcontractType.ValidValues.Add("-1", "");
                    foreach (var Prl in ContractType)
                    {
                        cbcontractType.ValidValues.Add(Convert.ToString(Prl.Code), Convert.ToString(Prl.Value));
                    }
                    cbcontractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillCountryCombo()
        {
            IEnumerable<MstCountry> Countries = from a in dbHrPayroll.MstCountry orderby a.CountryName ascending select a;
            //cbWorkCountry.ValidValues.Add("-1", "");
            //cbHomeCountry.ValidValues.Add("-1", "");
            //cbPriCntCountry.ValidValues.Add("-1", "");
            //cbSecCntCountry.ValidValues.Add("-1", "");
            foreach (MstCountry Country in Countries)
            {
                cbWorkCountry.ValidValues.Add(Convert.ToString(Country.CountryCode), Convert.ToString(Country.CountryName));
                cbHomeCountry.ValidValues.Add(Convert.ToString(Country.CountryCode), Convert.ToString(Country.CountryName));
                cbPriCntCountry.ValidValues.Add(Convert.ToString(Country.CountryCode), Convert.ToString(Country.CountryName));
                cbSecCntCountry.ValidValues.Add(Convert.ToString(Country.CountryCode), Convert.ToString(Country.CountryName));
            }
            cbWorkCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbHomeCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbPriCntCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbSecCntCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillStatesCombo()
        {
            IEnumerable<MstStates> States = from a in dbHrPayroll.MstStates select a;
            //cbWorkState.ValidValues.Add("-1", "");
            //cbHomeState.ValidValues.Add("-1", "");
            //cbPriCntState.ValidValues.Add("-1", "");
            //cbSecCntState.ValidValues.Add("-1", "");
            foreach (MstStates State in States)
            {
                cbWorkState.ValidValues.Add(Convert.ToString(State.ID), Convert.ToString(State.StateName));
                cbHomeState.ValidValues.Add(Convert.ToString(State.ID), Convert.ToString(State.StateName));
                cbPriCntState.ValidValues.Add(Convert.ToString(State.ID), Convert.ToString(State.StateName));
                cbSecCntState.ValidValues.Add(Convert.ToString(State.ID), Convert.ToString(State.StateName));

                //pCombo.ValidValues.Add(Convert.ToString(State.ID), Convert.ToString(State.StateName));
            }
            // pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            cbWorkState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbHomeState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbPriCntState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbSecCntState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillMartialCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Marital") select a;
                // pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE One in MartialStatus)
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

        private void FillGenderCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> Gender = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Gender") select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE One in Gender)
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

        private void FillLovList(SAPbouiCOM.ComboBox pCombo, String TypeCode)
        {
            try
            {
                IEnumerable<MstLOVE> MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains(TypeCode) select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE One in MartialStatus)
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

        private void FillReligionCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> Religions = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Religion") orderby a.Value ascending select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE Religion in Religions)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Religion.Code), Convert.ToString(Religion.Value));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillBloodGroupCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> Religions = from a in dbHrPayroll.MstLOVE where a.Type.Contains("BloodGroup") select a;
                // pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE Religion in Religions)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Religion.Code), Convert.ToString(Religion.Value));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillLocationsCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLocation> Locations = from a in dbHrPayroll.MstLocation select a;
                //pCombo.ValidValues.Add("-1", "");
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

        private void FillInstituteCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstInstitute> Collection = from a in dbHrPayroll.MstInstitute select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstInstitute One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Name);
                }

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillCertificationCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstCertification> Collection = from a in dbHrPayroll.MstCertification select a;
                // pCombo.ValidValues.Add("-1", "");
                foreach (MstCertification One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Name);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillRelationShipCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstRelation> Collection = from a in dbHrPayroll.MstRelation select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstRelation One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Code);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillQualificationCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstQualification> Collection = from a in dbHrPayroll.MstQualification select a;
                //pCombo.ValidValues.Add("-1", "");
                foreach (MstQualification One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Code);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }

        private void FillSboUsrCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                //string strSql = "select \"user_code\" , \"U_NAME\" from " + oCompany.CompanyDB + ".dbo.ousr";
                string strSql = "select \"USER_CODE\" , \"U_NAME\" from \"OUSR\"";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                //System.Data.DataTable dtUsr = ds.getDataTable(strSql);
                //pCombo.ValidValues.Add("-1", "");
                while (oRecSet.EoF == false)
                {
                    pCombo.ValidValues.Add(Convert.ToString(oRecSet.Fields.Item("USER_CODE").Value), Convert.ToString(oRecSet.Fields.Item("U_NAME").Value));
                    oRecSet.MoveNext();
                }
                //foreach (DataRow dr in dtUsr.Rows)
                //{
                //    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                //}
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillOHEMUserCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                //string strSql = "Select \"empID\" , \"firstName\" From " + oCompany.CompanyDB + ".dbo.ohem";
                //string strSql = "select \"USER_CODE\" , \"U_NAME\" from \"OUSR\"";
                string strSql = "select \"USER_CODE\" , \"U_NAME\" from \"OUSR\" ORDER BY \"U_NAME\"";
                //System.Data.DataTable dtUsr = ds.getDataTable(strSql);
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);

                //pCombo.ValidValues.Add("-1", "");
                while (oRecSet.EoF == false)
                {
                    pCombo.ValidValues.Add(Convert.ToString(oRecSet.Fields.Item("USER_CODE").Value), Convert.ToString(oRecSet.Fields.Item("U_NAME").Value));
                    oRecSet.MoveNext();
                }
                //foreach (DataRow dr in dtUsr.Rows)
                //{
                //    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                //}
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillCostCenterCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                //string strSql = "Select \"PrcCode\", \"PrcName\" From " + oCompany.CompanyDB + ".dbo.OPRC";
                //string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\"";
                string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\" ORDER BY \"PrcName\"";
                //string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\" LEFT OUTER JOIN \"OOCR\" ON OPRC.PrcCode = OOCR.OcrCode";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                List<string> CostCentersCode = new List<string>();
                List<string> CostCentersName = new List<string>();
                //DataTable dtUsr = ds.getDataTable(strSql);

                //pCombo.ValidValues.Add("-1", "");
                while (oRecSet.EoF == false)
                {
                    //pCombo.ValidValues.Add(Convert.ToString(oRecSet.Fields.Item("PrcCode").Value), Convert.ToString(oRecSet.Fields.Item("PrcName").Value));
                    CostCentersCode.Add(Convert.ToString(oRecSet.Fields.Item("PrcCode").Value));
                    CostCentersName.Add(Convert.ToString(oRecSet.Fields.Item("PrcName").Value));
                    oRecSet.MoveNext();
                }

                string strSql1 = "Select \"OcrCode\", \"OcrName\" From \"OOCR\"";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql1);
                while (oRecSet1.EoF == false)
                {

                    if (!CostCentersCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        CostCentersCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        CostCentersName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                for (Int32 i = 0; i < CostCentersCode.Count; i++)
                {
                    pCombo.ValidValues.Add(CostCentersCode[i], CostCentersName[i]);
                }
                //foreach (DataRow dr in dtUsr.Rows)
                //{
                //    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                //}
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillProjectCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                //string strSql = "Select \"PrcCode\", \"PrcName\" From " + oCompany.CompanyDB + ".dbo.OPRC";
                //string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\"";
                string strSql = "Select \"PrjCode\", \"PrjName\" From \"OPRJ\" ORDER BY \"PrjName\"";
                //string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\" LEFT OUTER JOIN \"OOCR\" ON OPRC.PrcCode = OOCR.OcrCode";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                List<string> ProjectCode = new List<string>();
                List<string> ProjectName = new List<string>();
                //DataTable dtUsr = ds.getDataTable(strSql);

                //pCombo.ValidValues.Add("-1", "");
                while (oRecSet.EoF == false)
                {
                    //pCombo.ValidValues.Add(Convert.ToString(oRecSet.Fields.Item("PrcCode").Value), Convert.ToString(oRecSet.Fields.Item("PrcName").Value));
                    ProjectCode.Add(Convert.ToString(oRecSet.Fields.Item("PrjCode").Value));
                    ProjectName.Add(Convert.ToString(oRecSet.Fields.Item("PrjName").Value));
                    oRecSet.MoveNext();
                }

                //string strSql1 = "Select \"OcrCode\", \"OcrName\" From \"OOCR\"";
                //SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                //oRecSet1.DoQuery(strSql1);
                //while (oRecSet1.EoF == false)
                //{

                //    if (!ProjectCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                //    {
                //        ProjectCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                //        ProjectName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                //    }
                //    oRecSet1.MoveNext();
                //}
                for (Int32 i = 0; i < ProjectCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ProjectCode[i], ProjectName[i]);
                }
                //foreach (DataRow dr in dtUsr.Rows)
                //{
                //    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                //}
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillComboDimension1(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 1";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 1";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension1.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim1 = true;
                }
                else
                {
                    flgDim1 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '1' and \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {
                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value).Trim());
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value).Trim());
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i].ToString().Trim(), ccName[i].ToString().Trim());
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension2(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 2";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 2";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension2.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim2 = true;
                }
                else
                {
                    flgDim2 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '2' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension3(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 3";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 3";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension3.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim3 = true;
                }
                else
                {
                    flgDim3 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '3' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension4(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 4";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 4";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension4.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim4 = true;
                }
                else
                {
                    flgDim4 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '4' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension5(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 5";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 5";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension5.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim5 = true;
                }
                else
                {
                    flgDim5 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '5' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboGratuity(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.TrnsGratuitySlabs select a).ToList();
                //pCombo.ValidValues.Add("-1", "");
                foreach (var one in ocollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(one.InternalID), Convert.ToString(one.SlabCode));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillComboGratuity : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillComboOTSlab(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.TrnsOTSlab select a).ToList();
                // pCombo.ValidValues.Add("-1", "");
                foreach (var one in ocollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(one.InternalID), Convert.ToString(one.SlabCode));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillComboGratuity : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillComboShiftDaysSlab(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstShiftDays select a).ToList();
                //pCombo.ValidValues.Add("-1", "");
                foreach (var one in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(one.Code), Convert.ToString(one.Description));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillComboShiftDaysSlab : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillComboBonusSlab(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oUniqueNumber = (from a in dbHrPayroll.MstBonusYearly
                                     where a.FlgActive == true
                                     select new
                                     {
                                         ID = a.ID,
                                         Code = a.Code,
                                         DocCode = a.DocCode,
                                         DocNumber = a.DocNo
                                     }).GroupBy(x => x.DocNumber).ToList();

                //pCombo.ValidValues.Add("-1", "");
                foreach (var Allw in oUniqueNumber)
                {
                    var DocumentNumber = Allw.Where(a => a.DocNumber == a.DocNumber).FirstOrDefault();
                    if (DocumentNumber != null)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(DocumentNumber.DocNumber), Convert.ToString(DocumentNumber.DocCode));
                    }
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillComboDeductionRule(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.TrnsDeductionRules select a).ToList();
                //pCombo.ValidValues.Add("-1", "");
                foreach (var one in ocollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(one.ID), Convert.ToString(one.Code));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboCategory(ComboBox pCombo)
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.MstCategory select a).ToList();
                //pCombo.ValidValues.Add("-1", "");
                foreach (var one in ocollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(one.InternalID), Convert.ToString(one.Description));
                }
                pCombo.Select(0, BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboSubCategory(ComboBox pCombo)
        {
            try
            {
                var ocollection = (from a in dbHrPayroll.MstSubCategory select a).ToList();
                //pCombo.ValidValues.Add("-1", "");
                foreach (var one in ocollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(one.InternalID), Convert.ToString(one.Description));
                }
                pCombo.Select(0, BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void AddEmptyRowRelatives()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtRelatives.Rows.Count == 0)
                {
                    dtRelatives.Rows.Add(1);
                    RowValue = dtRelatives.Rows.Count;
                    dtRelatives.SetValue(rIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtRelatives.SetValue(rId.DataBind.Alias, RowValue - 1, "0");
                    dtRelatives.SetValue(rFirstName.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rLastName.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rTelephone.DataBind.Alias, RowValue - 1, 0);
                    dtRelatives.SetValue(rEmail.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rDOB.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtRelatives.SetValue(rDepencdent.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rMCNo.DataBind.Alias, RowValue - 1, 0);
                    dtRelatives.SetValue(rMCStartDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtRelatives.SetValue(rMCExpiryDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    mtRelatives.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtRelatives.GetValue(rType.DataBind.Alias, dtRelatives.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {

                        dtRelatives.Rows.Add(1);
                        RowValue = dtRelatives.Rows.Count;
                        dtRelatives.SetValue(rIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtRelatives.SetValue(rId.DataBind.Alias, RowValue - 1, "0");
                        dtRelatives.SetValue(rFirstName.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rLastName.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rTelephone.DataBind.Alias, RowValue - 1, 0);
                        dtRelatives.SetValue(rEmail.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rDOB.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtRelatives.SetValue(rDepencdent.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rMCNo.DataBind.Alias, RowValue - 1, 0);
                        dtRelatives.SetValue(rMCStartDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtRelatives.SetValue(rMCExpiryDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        mtRelatives.AddRow(1, mtRelatives.RowCount + 1);
                    }
                }
                mtRelatives.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: AddEmptyRowRelative Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void AddEmptyRowEducation()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtEducation.Rows.Count == 0)
                {
                    dtEducation.Rows.Add(1);
                    RowValue = dtEducation.Rows.Count;
                    dtEducation.SetValue(eIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtEducation.SetValue(eId.DataBind.Alias, RowValue - 1, 0);
                    dtEducation.SetValue(eInstituteName.DataBind.Alias, RowValue - 1, -1);
                    dtEducation.SetValue(eFromDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtEducation.SetValue(eToDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtEducation.SetValue(eSubject.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(eQualification.DataBind.Alias, RowValue - 1, -1);
                    dtEducation.SetValue(eAwardedQlf.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(eMark.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(eNotes.DataBind.Alias, RowValue - 1, "");
                    mtEducation.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtEducation.GetValue(eInstituteName.DataBind.Alias, dtEducation.Rows.Count - 1) == -1)
                    {
                    }
                    else
                    {
                        dtEducation.Rows.Add(1);
                        RowValue = dtEducation.Rows.Count;
                        dtEducation.SetValue(eIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtEducation.SetValue(eId.DataBind.Alias, RowValue - 1, 0);
                        dtEducation.SetValue(eInstituteName.DataBind.Alias, RowValue - 1, -1);
                        dtEducation.SetValue(eFromDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtEducation.SetValue(eToDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtEducation.SetValue(eSubject.DataBind.Alias, RowValue - 1, "");
                        dtEducation.SetValue(eQualification.DataBind.Alias, RowValue - 1, -1);
                        dtEducation.SetValue(eAwardedQlf.DataBind.Alias, RowValue - 1, "");
                        dtEducation.SetValue(eMark.DataBind.Alias, RowValue - 1, "");
                        dtEducation.SetValue(eNotes.DataBind.Alias, RowValue - 1, "");
                        mtEducation.AddRow(1, mtEducation.RowCount + 1);
                    }
                }
                mtEducation.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void AddEmptyRowCertification()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtCertification.Rows.Count == 0)
                {
                    dtCertification.Rows.Add(1);
                    RowValue = dtCertification.Rows.Count;
                    dtCertification.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtCertification.SetValue(cId.DataBind.Alias, RowValue - 1, "0");
                    dtCertification.SetValue(cCertification.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cAwardedBy.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cAwardStatus.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cNotes.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cValidated.DataBind.Alias, RowValue - 1, "");
                    mtCertification.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtCertification.GetValue(cCertification.DataBind.Alias, dtCertification.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtCertification.Rows.Add(1);
                        RowValue = dtCertification.Rows.Count;
                        dtCertification.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtCertification.SetValue(cId.DataBind.Alias, RowValue - 1, "0");
                        dtCertification.SetValue(cCertification.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cAwardedBy.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cAwardStatus.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cNotes.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cValidated.DataBind.Alias, RowValue - 1, "");
                        mtCertification.AddRow(1, mtCertification.RowCount + 1);
                    }
                }
                mtCertification.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddEmptyRowCertification : " + ex.Message);
            }
        }

        private void AddEmptyRowPastExperiance()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtPastExperiance.Rows.Count == 0)
                {
                    dtPastExperiance.Rows.Add(1);
                    RowValue = dtPastExperiance.Rows.Count;
                    dtPastExperiance.SetValue(pId.DataBind.Alias, RowValue - 1, 0);
                    dtPastExperiance.SetValue(pIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtPastExperiance.SetValue(pCompany.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pFromdt.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtPastExperiance.SetValue(pTodt.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtPastExperiance.SetValue(pPosition.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pDuties.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pNotes.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pLastSalary.DataBind.Alias, RowValue - 1, "");
                    mtPastExperiance.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtPastExperiance.GetValue(pCompany.DataBind.Alias, dtPastExperiance.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtPastExperiance.Rows.Add(1);
                        RowValue = dtPastExperiance.Rows.Count;
                        dtPastExperiance.SetValue(pId.DataBind.Alias, RowValue - 1, 0);
                        dtPastExperiance.SetValue(pIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtPastExperiance.SetValue(pCompany.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pFromdt.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtPastExperiance.SetValue(pTodt.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtPastExperiance.SetValue(pPosition.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pDuties.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pNotes.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pLastSalary.DataBind.Alias, RowValue - 1, "");
                        mtPastExperiance.AddRow(1, mtPastExperiance.RowCount + 1);
                    }
                }
                mtPastExperiance.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OpenRelativeForm()
        {
            try
            {

                string formName = "EmpMstR";
                Program.sqlString = "empMaster";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + formName).Select();
                }
                catch
                {
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + formName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + formName + ".xml", oCompany, "frm_" + formName);

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenPastExpForm()
        {
            try
            {

                string formName = "EmpMstP";
                Program.sqlString = "empMaster";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + formName).Select();
                }
                catch
                {
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + formName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + formName + ".xml", oCompany, "frm_" + formName);

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenQualificationForm()
        {
            try
            {

                string formName = "EmpMstQ";
                Program.sqlString = "empMaster";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + formName).Select();
                }
                catch
                {
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + formName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + formName + ".xml", oCompany, "frm_" + formName);

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenEducationForm()
        {
            try
            {

                string formName = "EmpMstE";
                Program.sqlString = "empMaster";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + formName).Select();
                }
                catch
                {
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + formName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + formName + ".xml", oCompany, "frm_" + formName);

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SyncToSBO()
        {
            int check = 0;
            try
            {
                MsgWarning("Please wait department synchronization started.");
                var oCollectionDept = dbHrPayroll.MstDepartment.Where(x => x.SAPDocEntry == null || x.SAPDocEntry == 0).ToList();
                foreach (MstDepartment oDept in oCollectionDept)
                {
                    String retValue = "";
                    if (oDept.Code.Length >= 20)
                    {
                        oApplication.StatusBar.SetText("Dept Code : " + oDept.Code + " Code shouldn't be longer then 19 characters.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    retValue = Convert.ToString(CreateDepartment(oDept.ID.ToString(), oDept.Code));
                    oDept.SAPDocEntry = Convert.ToInt32(retValue);
                    dbHrPayroll.SubmitChanges();

                }
                MsgWarning(oCollectionDept.Count.ToString() + " departments synchronized.");

                MsgWarning("Please wait branch synchronization started.");
                var BranchList = dbHrPayroll.MstBranches.Where(x => x.SAPDocEntry == null || x.SAPDocEntry == 0).ToList();
                foreach (MstBranches oBranch in BranchList)
                {
                    String retValue = "";
                    if (oBranch.Name.Length >= 20)
                    {
                        oApplication.StatusBar.SetText("Branch Code : " + oBranch.Name + " Code shouldn't be longer then 19 characters.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    retValue = Convert.ToString(CreateBranch(oBranch.Id.ToString(), oBranch.Name));
                    oBranch.SAPDocEntry = Convert.ToInt32(retValue);
                    dbHrPayroll.SubmitChanges();

                }
                MsgWarning(BranchList.Count.ToString() + " branches synchronized.");

                MsgWarning("Please wait Employee synchronization started.");
                var oCollectionEmp = (from a in dbHrPayroll.MstEmployee
                                      where a.IntSboTransfered == false
                                      select a).ToList();
                if (oCollectionEmp.Count > 0)
                {
                    Int32 SyncEmp = 0;
                    Int32 EmpStatus = 0;
                    foreach (MstEmployee oEmp in oCollectionEmp)
                    {
                        String retValue = "";
                        if (string.IsNullOrEmpty(oEmp.LastName))
                        {
                            oApplication.StatusBar.SetText("Employee Code : " + oEmp.EmpID + " Employee Last name is mandatory.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (string.IsNullOrEmpty(oEmp.PositionName))
                        {
                            oApplication.StatusBar.SetText("Employee Code : " + oEmp.EmpID + " Position name is mandatory.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        else
                        {
                            if (oEmp.PositionName.Length >= 20)
                            {
                                oApplication.StatusBar.SetText("Employee Code : " + oEmp.EmpID + " Position name shouldn't be longer then 19 characters.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        if (oEmp.MstDepartment.SAPDocEntry == null || oEmp.MstDepartment.SAPDocEntry == 0)
                        {
                            oApplication.StatusBar.SetText("Employee Code : " + oEmp.EmpID + ": Assign department didn't synchronize with SAPB1.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (oEmp.MstBranches.SAPDocEntry == null || oEmp.MstBranches.SAPDocEntry == 0)
                        {
                            oApplication.StatusBar.SetText("Employee Code : " + oEmp.EmpID + ": Assign branches didn't synchronize with SAPB1.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        check++;
                        retValue = Convert.ToString(CreateOHEM(oEmp.EmpID, oEmp.FirstName, oEmp.MiddleName, oEmp.LastName, oEmp.PositionName, oEmp.MstDepartment.SAPDocEntry.Value, oEmp.MstBranches.SAPDocEntry.Value));
                        oEmp.SBOEmpCode = retValue;
                        EmpStatus = Convert.ToInt32(retValue);
                        if (EmpStatus != 0)
                        {
                            oEmp.IntSboTransfered = true;
                            SyncEmp++;
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                    oApplication.StatusBar.SetText(SyncEmp.ToString() + " Employees Sync with SBO", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None);
                }
                MsgWarning("Please continue employee synchronization completed.");
            }
            catch (Exception ex)
            {
                int a = check;
                logger(ex);
            }
        }

        private void UpdateSBO()
        {
            try
            {
                MsgWarning("Please wait Employee Update synchronization started.");
                var oList = (from a in dbHrPayroll.MstEmployee
                             where a.IntSboPublished == false
                             && a.IntSboTransfered == true
                             && a.FlgActive == true
                             select a).ToList();
                if (oList.Count > 0)
                {
                    foreach (var One in oList)
                    {
                        string EmployeeCode = One.EmpID;
                        if (string.IsNullOrEmpty(One.DepartmentName))
                        {
                            MsgWarning("Department is mandatory. Employee Code: " + EmployeeCode);
                            continue;
                        }
                        if (string.IsNullOrEmpty(One.BranchName))
                        {
                            MsgWarning("Branch is mandatory. Employee Code: " + EmployeeCode);
                            continue;
                        }
                        if (string.IsNullOrEmpty(One.PositionName))
                        {
                            MsgWarning("Position is mandatory. Employee Code: " + EmployeeCode);
                            continue;
                        }
                        string retValue = Convert.ToString(UpdateOHEM(One.EmpID, One.FirstName, One.MiddleName, One.LastName, One.PositionName, One.MstDepartment.SAPDocEntry.Value, One.MstBranches.SAPDocEntry.Value));
                        One.SBOEmpCode = retValue;
                        if (retValue != "0")
                        {
                            One.IntSboPublished = true;
                        }
                        dbHrPayroll.SubmitChanges();
                        MsgSuccess("Employee updated successfully. Employee Code: " + EmployeeCode);
                    }
                }
                MsgWarning("Please continue Employee Update synchronization ended.");
            }
            catch (Exception ex)
            {
                logger(ex);
                MsgError("Something went wrong, Update synchronization failed.");
            }
        }

        private Int32 CreateOHEMAR(String EmpID, String pFirstName, String pMiddleName, String pLastName, String jobTitle, int departmentCode, int branchCode)
        {

            if (string.IsNullOrWhiteSpace(pFirstName))
            {
                oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " First Name is Madentory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            else if (string.IsNullOrWhiteSpace(pMiddleName))
            {
                oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Middle Name is Madentory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            else if (string.IsNullOrWhiteSpace(pLastName))
            {
                oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " last Name is Madentory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            else if (string.IsNullOrWhiteSpace(jobTitle))
            {
                oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Job Title is Madentory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            else if (departmentCode <= 0)
            {
                oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Department Code is Madentory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            else if (branchCode <= 0)
            {
                oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Branch Code is Madentory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            int retValue = 0;
            //if (Program.systemInfo.SAPB1Integration != true || oForm.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE) return;
            try
            {
                Boolean isUpdate = false;
                SAPbobsCOM.EmployeesInfo nEmp = (SAPbobsCOM.EmployeesInfo)Program.objHrmsUI.oDiCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oEmployeesInfo);
                int SAPEmpID = 0;
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT empID AS SAPEMPID FROM dbo.OHEM WHERE U_HrmsEmpId = '" + EmpID + "'");
                SAPEmpID = oRecSet.Fields.Item("SAPEMPID").Value;
                //SELECT empID AS SAPEMPID FROM dbo.OHEM WHERE U_HrmsEmpId='0017'
                if (SAPEmpID > 0)
                {
                    nEmp.GetByKey(SAPEmpID);
                    isUpdate = true;
                }


                String EmpCodeFromSAP = "";
                //oApplication.StatusBar.SetText("before create object EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                //oApplication.StatusBar.SetText("after create object EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.FirstName = pFirstName;
                //oApplication.StatusBar.SetText("after first name EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.MiddleName = pMiddleName;
                //oApplication.StatusBar.SetText("after middle name EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.LastName = pLastName;

                nEmp.JobTitle = jobTitle;
                nEmp.Department = departmentCode;
                nEmp.Branch = branchCode;

                //oApplication.StatusBar.SetText("after lastname EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                nEmp.UserFields.Fields.Item("U_HrmsEmpId").Value = EmpID;
                //oApplication.StatusBar.SetText("after custom field EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                if (isUpdate)
                {
                    if (nEmp.Update() == 0)
                    {
                        //oCompany.GetNewObjectCode( out EmpCodeFromSAP);
                        Program.objHrmsUI.oDiCompany.GetNewObjectCode(out EmpCodeFromSAP);
                        retValue = Convert.ToInt32(EmpCodeFromSAP);
                    }
                    else
                    {
                        //oApplication.SetStatusBarMessage("Error In Integrating with OHEM & EmpMaster.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                        Int32 errCode = 0;
                        String errName = "";
                        Program.objHrmsUI.oDiCompany.GetLastError(out errCode, out errName);
                        oApplication.StatusBar.SetText("Can't Sync Automatically Create EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Error Code : " + errCode + " Description : " + errName, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        retValue = 0;
                    }
                }
                else
                {
                    if (nEmp.Add() == 0)
                    {
                        //oCompany.GetNewObjectCode( out EmpCodeFromSAP);
                        Program.objHrmsUI.oDiCompany.GetNewObjectCode(out EmpCodeFromSAP);
                        retValue = Convert.ToInt32(EmpCodeFromSAP);
                    }
                    else
                    {
                        //oApplication.SetStatusBarMessage("Error In Integrating with OHEM & EmpMaster.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                        Int32 errCode = 0;
                        String errName = "";
                        Program.objHrmsUI.oDiCompany.GetLastError(out errCode, out errName);
                        oApplication.StatusBar.SetText("Can't Sync Automatically Create EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Error Code : " + errCode + " Description : " + errName, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        retValue = 0;
                    }
                }
                return retValue;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CreateOHEM Function Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            return retValue;
        }

        private Int32 CreateOHEM(String EmpID, String pFirstName, String pMiddleName, String pLastName, String jobTitle, int departmentCode, int branchCode)
        {
            int retValue = 0;
            //if (Program.systemInfo.SAPB1Integration != true || oForm.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE) return;
            try
            {

                String EmpCodeFromSAP = "";
                //oApplication.StatusBar.SetText("before create object EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                SAPbobsCOM.EmployeesInfo nEmp = (SAPbobsCOM.EmployeesInfo)Program.objHrmsUI.oDiCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oEmployeesInfo);
                //oApplication.StatusBar.SetText("after create object EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.FirstName = pFirstName.Trim();
                //oApplication.StatusBar.SetText("after first name EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.MiddleName = pMiddleName.Trim();
                //oApplication.StatusBar.SetText("after middle name EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.LastName = pLastName.Trim();

                nEmp.JobTitle = jobTitle.Trim();
                nEmp.Department = departmentCode;
                nEmp.Branch = branchCode;

                //oApplication.StatusBar.SetText("after lastname EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                nEmp.UserFields.Fields.Item("U_HrmsEmpId").Value = EmpID;
                //oApplication.StatusBar.SetText("after custom field EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                if (nEmp.Add() == 0)
                {
                    //oCompany.GetNewObjectCode( out EmpCodeFromSAP);
                    Program.objHrmsUI.oDiCompany.GetNewObjectCode(out EmpCodeFromSAP);
                    retValue = Convert.ToInt32(EmpCodeFromSAP);
                }
                else
                {
                    //oApplication.SetStatusBarMessage("Error In Integrating with OHEM & EmpMaster.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    Int32 errCode = 0;
                    String errName = "";
                    Program.objHrmsUI.oDiCompany.GetLastError(out errCode, out errName);
                    oApplication.StatusBar.SetText("Can't Sync Automatically Create EmpID :: " + EmpID + " Manualy.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    oApplication.StatusBar.SetText("SBO Internal Error For Employee :: " + EmpID + " Error Code : " + errCode + " Description : " + errName, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    retValue = 0;
                }
                return retValue;

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CreateOHEM Function Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            return retValue;
        }

        private Int32 UpdateOHEM(String EmpID, String pFirstName, String pMiddleName, String pLastName, String jobTitle, int departmentCode, int branchCode)
        {
            int retValue = 0;
            try
            {
                string QueryOHEM = "Select * From \"OHEM\" Where \"U_HrmsEmpId\" = '" + EmpID + "'";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(QueryOHEM);
                int EmpKey = Convert.ToInt32(oRecSet.Fields.Item("empID").Value);
                String EmpCodeFromSAP = "";
                SAPbobsCOM.EmployeesInfo nEmp = (SAPbobsCOM.EmployeesInfo)Program.objHrmsUI.oDiCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oEmployeesInfo);
                nEmp.GetByKey(EmpKey);

                nEmp.FirstName = pFirstName.Trim();
                nEmp.MiddleName = pMiddleName.Trim();
                nEmp.LastName = pLastName.Trim();

                nEmp.JobTitle = jobTitle.Trim();
                nEmp.Department = departmentCode;
                nEmp.Branch = branchCode;
                //nEmp.UserFields.Fields.Item("U_HrmsEmpId").Value = EmpID;

                if (nEmp.Update() == 0)
                {
                    Program.objHrmsUI.oDiCompany.GetNewObjectCode(out EmpCodeFromSAP);
                    retValue = Convert.ToInt32(EmpCodeFromSAP);
                }
                else
                {
                    Int32 errCode = 0;
                    String errName = "";
                    Program.objHrmsUI.oDiCompany.GetLastError(out errCode, out errName);
                    retValue = 0;
                }
                return retValue;

            }
            catch (Exception ex)
            {
                logger(ex);
            }
            return retValue;
        }

        private Int32 CreateDepartment(String depID, String depName)
        {
            int retValue = 0;
            try
            {
                string SAPDocEntry = "";
                DepartmentsService oDeptSrv;
                CompanyService cmpService = Program.objHrmsUI.oDiCompany.GetCompanyService();
                oDeptSrv = (DepartmentsService)(cmpService.GetBusinessService(ServiceTypes.DepartmentsService));
                Department addLine;
                addLine = (Department)oDeptSrv.GetDataInterface(DepartmentsServiceDataInterfaces.dsDepartment);
                addLine.Name = depName;
                DepartmentParams dp = oDeptSrv.AddDepartment(addLine);
                retValue = Convert.ToInt32(dp.Code);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CreateDepartment Function Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            return retValue;
        }

        private Int32 CreateBranch(String LocID, String LocName)
        {
            int retValue = 0;
            try
            {
                string SAPDocEntry = "";
                BranchesService oBranchSrv;

                CompanyService cmpService = Program.objHrmsUI.oDiCompany.GetCompanyService();
                oBranchSrv = (BranchesService)(cmpService.GetBusinessService(ServiceTypes.BranchesService));
                Branch addLine;
                addLine = (Branch)oBranchSrv.GetDataInterface(BranchesServiceDataInterfaces.bsBranch);
                addLine.Name = LocName;
                BranchParams bp = oBranchSrv.AddBranch(addLine);
                retValue = Convert.ToInt32(bp.Code);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("CreateBranch Function Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            return retValue;
        }

        private void HideBasicSalary()
        {
            try
            {
                if (!Program.objHrmsUI.isSuperUser)
                {
                    itxtBasicSalary.Visible = false;
                    ilblBasicSalary.Visible = false;
                }
                else
                {
                    itxtBasicSalary.Visible = true;
                    ilblBasicSalary.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void VerifyAllEmployeeShiftDays(MstEmployee One)
        {
            try
            {

                var oShiftDays = (from a in dbHrPayroll.MstShiftDays
                                  where a.Code == One.ShiftDaysCode
                                  select a).FirstOrDefault();
                var oCurrentFiscal = (from a in dbHrPayroll.MstCalendar
                                      where a.FlgActive == true
                                      select a).FirstOrDefault();
                DateTime StartDate, EndDate;
                StartDate = Convert.ToDateTime(One.JoiningDate);
                EndDate = Convert.ToDateTime(oCurrentFiscal.EndDate);
                var oOldRecord = (from a in dbHrPayroll.TrnsShiftsDaysRegister
                                  where a.RecordDate < StartDate
                                  && a.EmpCode == One.EmpID
                                  select a).ToList();
                if (oOldRecord.Count > 0)
                {
                    dbHrPayroll.TrnsShiftsDaysRegister.DeleteAllOnSubmit(oOldRecord);
                }
                bool flgOnOff = true;
                int BaseDocDays = Convert.ToInt32(oShiftDays.DaysCount);
                int RunningValue = 1;
                for (DateTime Running = StartDate; Running <= EndDate; Running = Running.AddDays(1))
                {
                    var oCount = (from a in dbHrPayroll.TrnsShiftsDaysRegister
                                  where a.RecordDate == Running
                                  && a.EmpCode == One.EmpID
                                  select a).Count();
                    if (oCount > 0)
                    {
                        TrnsShiftsDaysRegister oNew = (from a in dbHrPayroll.TrnsShiftsDaysRegister
                                                       where a.RecordDate == Running
                                                       && a.EmpCode == One.EmpID
                                                       select a).FirstOrDefault();
                        oNew.EmpCode = One.EmpID;
                        oNew.EmpName = One.FirstName + " " + One.MiddleName + " " + One.LastName;
                        oNew.ShiftName = oShiftDays.Code;
                        oNew.RecordDate = Running;
                        if (flgOnOff)
                        {
                            oNew.DayStatus = 1;
                        }
                        else
                        {
                            oNew.DayStatus = 0;
                        }
                        oNew.UpdatedBy = oCompany.UserName;
                        oNew.UpdateDate = DateTime.Now;
                    }
                    else
                    {
                        TrnsShiftsDaysRegister oNew = new TrnsShiftsDaysRegister();
                        dbHrPayroll.TrnsShiftsDaysRegister.InsertOnSubmit(oNew);
                        oNew.EmpCode = One.EmpID;
                        oNew.EmpName = One.FirstName + " " + One.MiddleName + " " + One.LastName;
                        oNew.ShiftName = oShiftDays.Code;
                        oNew.RecordDate = Running;
                        if (flgOnOff)
                        {
                            oNew.DayStatus = 1;
                        }
                        else
                        {
                            oNew.DayStatus = 0;
                        }
                        oNew.CreatedBy = oCompany.UserName;
                        oNew.UpdatedBy = oCompany.UserName;
                        oNew.CreateDate = DateTime.Now;
                        oNew.UpdateDate = DateTime.Now;
                    }
                    if (RunningValue < BaseDocDays)
                    {
                        RunningValue++;
                    }
                    else
                    {
                        RunningValue = 1;
                        if (flgOnOff)
                        {
                            flgOnOff = false;
                        }
                        else
                        {
                            flgOnOff = true;
                        }
                    }
                }

                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        #endregion

    }
}
