using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_EmpMst:HRMSBaseForm
    {
        #region "Global Variable Section"

        SAPbouiCOM.Button btnMain, btnCancel, btnSyncTOSBO;
        SAPbouiCOM.Item ibtnMain, itxtEmployeeCode, itxtUserCode, itxtFirstName, ibtnSyncTOSBO, itbArabic, itxtManager, itxtReportTo;
        SAPbouiCOM.EditText txtFirstName, txtMiddleName, txtLastName, txtFatherName, txtMotherName, txtJobTitle;
        SAPbouiCOM.EditText txtEmployeeCode, txtInitials, txtExtention, txtNamePrefix, txtOfficePhn, txtMobilePhn;
        SAPbouiCOM.EditText txtHomePhn, txtPager, txtFax, txtEmail, txtUserCode, txtDateOfJoining;
        SAPbouiCOM.EditText txtHomeStreet, txtHomeStreetNo, txtHomeBlock, txtHomeBuilding, txtHomeZip, txtHomeCity;
        SAPbouiCOM.EditText txtWorkStreet, txtWorkStreetNo, txtWorkBlock, txtWorkBuilding, txtWorkZip, txtWorkCity, txtWorkBranches;
        SAPbouiCOM.EditText txtPriCntName, txtPriCntRelation, txtPriCntNoLandLine, txtPriCntNoMobile, txtPriCntAddress, txtPriCntCity;
        SAPbouiCOM.EditText txtSecCntName, txtSecCntRelation, txtSecCntNoLandLine, txtSecCntNoMobile, txtSecCntAddress, txtSecCntCity;
        SAPbouiCOM.EditText txtSSNumber, txtUnionMemberShip, txtUnionMemberShipNo, txtNationality, txtIDCardNo, txtIDDtOfIssue;
        SAPbouiCOM.EditText txtBasicSalary, txtEmpCalendar, txtEmpShift, txtWorkIM, txtPersonalIM, txtPersonalEmail, txtPersonalContact;
        SAPbouiCOM.EditText txtOrganizationUnit, txtReportTo, txtEmpContractType, txtHRCalendar, txtWindowsLogin, txtEmpGrade, txtPreviosEmpMonth;
        SAPbouiCOM.EditText txtWorkPermitRef, txtWorkPermitExpiry, txtContractExpiry, txtDOB, txtRemarks;
        SAPbouiCOM.EditText txtIDPlaceOfIssue, txtIDIssuedBy, txtIDExpiryDate, txtPassportNo, txtPassportDateofIssue, txtPassportExpiry, txtIncomeTax;
        SAPbouiCOM.EditText txtAccountTitle, txtAccountNo, txtBankName, txtBankBranch, txtEffectiveDate, txtPercentage, txtPassword, txtAttachments, txtTermination, txtResignation, txtManager;
        SAPbouiCOM.ComboBox cbHomeState, cbWorkState, cbHomeCountry, cbWorkCountry, cbPosition, cbDepartment, cbBranch, cbDesignation, cbJobTitle;
        SAPbouiCOM.ComboBox cbManager, cbPriCntState, cbPriCntCountry, cbSecCntState, cbSecCntCountry, cbMartial, cbReportingManager, cbCostCenter;
        SAPbouiCOM.ComboBox cbReligion, cbPaymentMode, cbAccountType, cbPayroll, cbLocation, cbSalaryCurrency, cbBloodGroup, cbSBOLinkID, cbOhemUser, cbGender;
        SAPbouiCOM.Folder tbAddress, tbEmergencyDetail, tbPersonal, tbSalary, tbAbsence, tbCommunication, tbClassification,
                            tbRelative, tbPastExperiance, tbEducation, tbQualification, tbArabic;
        SAPbouiCOM.Matrix mtAbsence, mtRelatives, mtCertification, mtPastExperiance, mtEducation;
        SAPbouiCOM.DataTable dtAbsence, dtRelatives, dtCertification, dtPastExperiance, dtEducation;
        SAPbouiCOM.Column Serial, aIsNew, aID, aDescription, aBalanceBF, aEntitled, aTotalAvailable, aUsed, aRequested, aApproved, aBalance, 
                            rSerial, rIsNew, rId, rType, rFirstName, rLastName, rTelephone, rEmail, rDOB, rDepencdent, rMCNo, 
                            rMCStartDate, rMCExpiryDate, cSerial, cIsNew, cId, cCertification, cAwardedBy, cAwardStatus, cDescription,
                            cNotes, cValidated, pSerial, pIsNew, pId, pCompany, pFromdt, pTodt, pPosition, pDuties, pNotes, pLastSalary,
                            eSerial, eIsNew, eId, eInstituteName, eFromDate, eToDate, eSubject, eQualification, eAwardedQlf, eMark, eNotes;
        SAPbouiCOM.CheckBox chkActiveEmployee;
        SAPbouiCOM.ComboBox cbShift;
        SAPbouiCOM.ComboBox cbcontractType;
        SAPbouiCOM.PictureBox pctBox;
        SAPbouiCOM.Button btnLoad, btnSave,btnBranches;


        Boolean flgManager= false, flgReportTo = false;
        IEnumerable<MstEmployee> oEmployees = null;
        String FilePath, picPath;

        //Arabic Localization 

        SAPbouiCOM.EditText txtEnglishName, txtArabicName, txtPassportExpiryDtH, txtIDExpiryDtH;
        SAPbouiCOM.EditText txtMedicalCardExpirydtH, txtDrvLicCompletionDtH, txtDrvLicLastDtH, txtDrvLicReleaseDtH;
        SAPbouiCOM.EditText txtVisaNumber, txtIqamaProfessional, txtBankCardExpiryDtH;
        
        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                fillCbs();
                itxtFirstName.Click();
                oForm.Freeze(false);
                
               
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
                    case "btbrnch":
                        flgReportTo = true;
                        OpenNewSearchForm();
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
            }
            if (Program.EmpID != txtEmployeeCode.Value.Trim() && flgManager && !flgReportTo)
            {
                txtManager.Value = Program.EmpID;
                flgManager = false;
            }
            if (Program.EmpID != txtEmployeeCode.Value.Trim() && flgReportTo && !flgManager)
            {
                txtReportTo.Value = Program.EmpID;
                flgReportTo = false;
            }
            //base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            //SetEmpValues();
        }
       

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            InitiallizeDocument();
            itxtEmployeeCode.Enabled = true;
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            //btnMain.Caption = "Add";
            
        }

        public override void FindRecordMode()
        {
           // Program.EmpID = string.Empty;
            base.FindRecordMode();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE;
            itxtEmployeeCode.Enabled = true;
            InitiallizeDocument();
            //btnMain.Caption = "Find";
            
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

        #endregion

        #region "Local Methods" 

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
                oApplication.StatusBar.SetText("Please wait Until Employee Master loaded.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                oForm.DefButton = "1";
                oForm.Freeze(true);
                btnMain = oForm.Items.Item("1").Specific;
                ibtnMain = oForm.Items.Item("1");

                btnSyncTOSBO = oForm.Items.Item("btSBO").Specific;
                ibtnSyncTOSBO = oForm.Items.Item("btSBO");
                //btnMain.Caption = "Add";
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                btnCancel = oForm.Items.Item("2").Specific;

                pctBox = oForm.Items.Item("picbox").Specific;


                tbAddress = oForm.Items.Item("tbaddress").Specific;
                tbAddress = oForm.Items.Item("tbaddress").Specific;

               
                tbEmergencyDetail = oForm.Items.Item("tbed").Specific;
                tbPersonal = oForm.Items.Item("tbpersonal").Specific;
                tbSalary = oForm.Items.Item("tbsalary").Specific;
                tbAbsence = oForm.Items.Item("tbabsence").Specific;
                tbCommunication = oForm.Items.Item("tbcom").Specific;
                tbClassification = oForm.Items.Item("tbclass").Specific;

                tbRelative = oForm.Items.Item("tbrelative").Specific;
                tbQualification = oForm.Items.Item("tbqlf").Specific;
                tbPastExperiance = oForm.Items.Item("tbpstexp").Specific;
                tbEducation = oForm.Items.Item("tbedu").Specific;
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
                objDataSources.Add("txtFirstName", "txthfname");

                oForm.DataSources.UserDataSources.Add("txthmname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtMiddleName = oForm.Items.Item("txthmname").Specific;
                txtMiddleName.DataBind.SetBound(true, "", "txthmname");
                objDataSources.Add("txtMiddleName", "txthmname");

                oForm.DataSources.UserDataSources.Add("txthlname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtLastName = oForm.Items.Item("txthlname").Specific;
                txtLastName.DataBind.SetBound(true, "", "txthlname");
                objDataSources.Add("txtLastName", "txthlname");

                oForm.DataSources.UserDataSources.Add("txthftname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtFatherName = oForm.Items.Item("txthftname").Specific;
                txtFatherName.DataBind.SetBound(true, "", "txthftname");
                objDataSources.Add("txtFatherName", "txthftname");


                oForm.DataSources.UserDataSources.Add("txpsw", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtPassword = oForm.Items.Item("txpsw").Specific;
                txtPassword.DataBind.SetBound(true, "", "txpsw");
                objDataSources.Add("txtPassword", "txpsw");


                oForm.DataSources.UserDataSources.Add("txthempcde", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtEmployeeCode = oForm.Items.Item("txthempcde").Specific;
                itxtEmployeeCode = oForm.Items.Item("txthempcde");
                txtEmployeeCode.DataBind.SetBound(true, "", "txthempcde");

                txtInitials = oForm.Items.Item("txhinitial").Specific;
                oForm.DataSources.UserDataSources.Add("txhinitial", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtInitials.DataBind.SetBound(true, "", "txhinitial");
                objDataSources.Add("txtInitials", "txhinitial");


                txtNamePrefix = oForm.Items.Item("txhnprefix").Specific;
                oForm.DataSources.UserDataSources.Add("txhnprefix", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtNamePrefix.DataBind.SetBound(true, "", "txhnprefix");
                objDataSources.Add("txtNamePrefix", "txhnprefix");


                txtOfficePhn = oForm.Items.Item("txhoph").Specific;
                oForm.DataSources.UserDataSources.Add("txhoph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtOfficePhn.DataBind.SetBound(true, "", "txhoph");
                objDataSources.Add("txtOfficePhn", "txhoph");

                txtExtention = oForm.Items.Item("txhext").Specific;
                oForm.DataSources.UserDataSources.Add("txhext", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtExtention.DataBind.SetBound(true, "", "txhext");
                objDataSources.Add("txtExtention", "txhext");


                txtMobilePhn = oForm.Items.Item("txhmph").Specific;
                oForm.DataSources.UserDataSources.Add("txhmph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtMobilePhn.DataBind.SetBound(true, "", "txhmph");
                objDataSources.Add("txtMobilePhn", "txhmph");


                txtHomePhn = oForm.Items.Item("txhhph").Specific;
                oForm.DataSources.UserDataSources.Add("txhhph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtHomePhn.DataBind.SetBound(true, "", "txhhph");
                objDataSources.Add("txtHomePhn", "txhhph");


                txtPager = oForm.Items.Item("txhpager").Specific;
                oForm.DataSources.UserDataSources.Add("txhpager", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtPager.DataBind.SetBound(true, "", "txhpager");
                objDataSources.Add("txtPager", "txhpager");


                txtFax = oForm.Items.Item("txhfax").Specific;
                oForm.DataSources.UserDataSources.Add("txhfax", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 15);
                txtFax.DataBind.SetBound(true, "", "txhfax");
                objDataSources.Add("txtFax", "txhfax");



                txtEmail = oForm.Items.Item("txhemail").Specific;
                oForm.DataSources.UserDataSources.Add("txhemail", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmail.DataBind.SetBound(true, "", "txhemail");
                objDataSources.Add("txtEmail", "txhemail");


                txtUserCode = oForm.Items.Item("txusercode").Specific;
                itxtUserCode = oForm.Items.Item("txusercode");
                oForm.DataSources.UserDataSources.Add("txusercode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtUserCode.DataBind.SetBound(true, "", "txusercode");
                //itxtUserCode.Enabled = false;
                objDataSources.Add("txtUserCode", "txusercode");

                cbPosition = oForm.Items.Item("cbhposi").Specific;
                oForm.DataSources.UserDataSources.Add("cbhposi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPosition.DataBind.SetBound(true, "", "cbhposi");
                objDataSources.Add("cbPosition", "cbhposi");

                cbShift = oForm.Items.Item("cbShift").Specific;
                oForm.DataSources.UserDataSources.Add("cbShift", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbShift.DataBind.SetBound(true, "", "cbShift");
                //objDataSources.Add("cbPosition", "cbhposi");

                cbJobTitle = oForm.Items.Item("cbjt").Specific;
                oForm.DataSources.UserDataSources.Add("cbjt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbJobTitle.DataBind.SetBound(true, "", "cbjt");
                objDataSources.Add("cbJobTitle", "cbjt");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLocation.DataBind.SetBound(true, "", "cbloc");
                objDataSources.Add("cbLocation", "cbhposi");

                cbDepartment = oForm.Items.Item("cbhdept").Specific;
                oForm.DataSources.UserDataSources.Add("cbhdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDepartment.DataBind.SetBound(true, "", "cbhdept");
                objDataSources.Add("cbDepartment", "cbhdept");

                cbDesignation = oForm.Items.Item("cbdesig").Specific;
                oForm.DataSources.UserDataSources.Add("cbdesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDesignation.DataBind.SetBound(true, "", "cbdesig");
                objDataSources.Add("cbDesignation", "cbdesig");

                cbBranch = oForm.Items.Item("cbhbrnch").Specific;
                oForm.DataSources.UserDataSources.Add("cbhbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbBranch.DataBind.SetBound(true, "", "cbhbrnch");
                objDataSources.Add("cbBranch", "cbhbrnch");

                //cbManager = oForm.Items.Item("cbhmng").Specific;
                //oForm.DataSources.UserDataSources.Add("cbhmng", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                //cbManager.DataBind.SetBound(true, "", "cbhmng");
                //objDataSources.Add("cbManager", "cbhmng");

                txtManager = oForm.Items.Item("txmanager").Specific;
                itxtManager = oForm.Items.Item("txmanager");
                oForm.DataSources.UserDataSources.Add("txmanager", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtManager.DataBind.SetBound(true, "", "txmanager");

                cbSBOLinkID = oForm.Items.Item("cbsbo").Specific;
                oForm.DataSources.UserDataSources.Add("cbsbo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbSBOLinkID.DataBind.SetBound(true, "", "cbsbo");
                objDataSources.Add("cbSBOLinkID", "cbsbo");

                chkActiveEmployee = oForm.Items.Item("chkhaemp").Specific;
                oForm.DataSources.UserDataSources.Add("chkhaemp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkActiveEmployee.DataBind.SetBound(true, "", "chkhaemp");
                chkActiveEmployee.Checked = true;
                objDataSources.Add("chkActiveEmployee", "chkhaemp");

                //chkCreateUser = oForm.Items.Item("chkUser").Specific;
                //oForm.DataSources.UserDataSources.Add("chkUser", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                //chkCreateUser.DataBind.SetBound(true, "", "chkUser");
                //objDataSources.Add("chkCreateUser", "chkUser");



                
               // Select the Pane
                oForm.PaneLevel = 1;

                //Set Query 
//                String query = @"SELECT EmpID, ISNULL(FirstName,'''') +  '' '' + ISNULL(MiddleName,'''')+ '' '' + ISNULL(LastName,'''') AS EmpName
//                                FROM "+ Program.objHrmsUI.HRMSDbName +".dbo.MstEmployee";
//                Program.objHrmsUI.addFms("frm_EmpMst", "txthempcde", "-1", query);

                 //Address Tab

                txtHomeStreet = oForm.Items.Item("txstreet").Specific;
                oForm.DataSources.UserDataSources.Add("txstreet", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeStreet.DataBind.SetBound(true, "", "txstreet");
                objDataSources.Add("txtHomeStreet", "txstreet");


                txtHomeStreetNo = oForm.Items.Item("txstreetno").Specific;
                oForm.DataSources.UserDataSources.Add("txstreetno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeStreetNo.DataBind.SetBound(true, "", "txstreetno");
                objDataSources.Add("txtHomeStreetNo", "txstreetno");
                
                txtHomeBlock = oForm.Items.Item("txblock").Specific;
                oForm.DataSources.UserDataSources.Add("txblock", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeBlock.DataBind.SetBound(true, "", "txblock");
                objDataSources.Add("txtHomeBlock", "txblock");


                txtHomeBuilding = oForm.Items.Item("txbuilding").Specific;
                oForm.DataSources.UserDataSources.Add("txbuilding", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeBuilding.DataBind.SetBound(true, "", "txbuilding");
                objDataSources.Add("txtHomeBuilding", "txbuilding");


                txtHomeZip = oForm.Items.Item("txzipcode").Specific;
                oForm.DataSources.UserDataSources.Add("txzipcode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeZip.DataBind.SetBound(true, "", "txzipcode");
                objDataSources.Add("txtHomeZip", "txzipcode");


                txtHomeCity = oForm.Items.Item("txcity").Specific;
                oForm.DataSources.UserDataSources.Add("txcity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtHomeCity.DataBind.SetBound(true, "", "txcity");
                objDataSources.Add("txtHomeCity", "txcity");
                

                cbHomeState = oForm.Items.Item("cbstate").Specific;
                oForm.DataSources.UserDataSources.Add("cbstate", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbHomeState.DataBind.SetBound(true, "", "cbstate");
                objDataSources.Add("cbHomeState", "cbstate");
                

                cbHomeCountry = oForm.Items.Item("cbcountry").Specific;
                oForm.DataSources.UserDataSources.Add("cbcountry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbHomeCountry.DataBind.SetBound(true, "", "cbcountry");
                objDataSources.Add("cbHomeCountry", "cbcountry");
                
                txtWorkStreet = oForm.Items.Item("txstreet1").Specific;
                oForm.DataSources.UserDataSources.Add("txstreet1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkStreet.DataBind.SetBound(true, "", "txstreet1");
                objDataSources.Add("txtWorkStreet", "txstreet1");
                

                txtWorkStreetNo = oForm.Items.Item("txstretno1").Specific;
                oForm.DataSources.UserDataSources.Add("txstretno1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkStreetNo.DataBind.SetBound(true, "", "txstretno1");
                objDataSources.Add("txtWorkStreetNo", "txstretno1");
                

                txtWorkBlock = oForm.Items.Item("txblock1").Specific;
                oForm.DataSources.UserDataSources.Add("txblock1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkBlock.DataBind.SetBound(true, "", "txblock1");
                objDataSources.Add("txtWorkBlock", "txblock1");
                
                txtWorkBuilding = oForm.Items.Item("txbuildng1").Specific;
                oForm.DataSources.UserDataSources.Add("txbuildng1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkBuilding.DataBind.SetBound(true, "", "txbuildng1");
                objDataSources.Add("txtWorkBuilding", "txbuildng1");
                

                txtWorkZip = oForm.Items.Item("txzipcode1").Specific;
                oForm.DataSources.UserDataSources.Add("txzipcode1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkZip.DataBind.SetBound(true, "", "txzipcode1");
                objDataSources.Add("txtWorkZip", "txzipcode1");
                
                txtWorkCity = oForm.Items.Item("txcity1").Specific;
                oForm.DataSources.UserDataSources.Add("txcity1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkCity.DataBind.SetBound(true, "", "txcity1");
                objDataSources.Add("txtWorkCity", "txcity1");
                
               
                cbWorkState = oForm.Items.Item("cbstate1").Specific;
                oForm.DataSources.UserDataSources.Add("cbstate1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbWorkState.DataBind.SetBound(true, "", "cbstate1");
                objDataSources.Add("cbWorkState", "cbstate1");


                txtWorkBranches = oForm.Items.Item("txtbrnch").Specific;
                oForm.DataSources.UserDataSources.Add("txtbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtWorkBranches.DataBind.SetBound(true, "", "txtbrnch");
                objDataSources.Add("cbWorkState", "txtbrnch");
                               
                //Emergency Detail

                txtPriCntName = oForm.Items.Item("txpcname").Specific;
                objDataSources.Add("txtPriCntName", "txpcname");

                txtPriCntRelation = oForm.Items.Item("txpcrlt").Specific;
                objDataSources.Add("txtPriCntRelation", "txpcrlt");

                txtPriCntNoLandLine = oForm.Items.Item("txpccnln").Specific;
                objDataSources.Add("txtPriCntNoLandLine", "txpccnln");

                txtPriCntNoMobile = oForm.Items.Item("txpccnm").Specific;
                objDataSources.Add("txtPriCntNoMobile", "txpccnm");


                txtPriCntAddress = oForm.Items.Item("txpcadr").Specific;
                objDataSources.Add("txtPriCntAddress", "txpcadr");

                txtPriCntCity = oForm.Items.Item("txpccity").Specific;
                objDataSources.Add("txtPriCntCity", "txpccity");

                cbPriCntState = oForm.Items.Item("cbpcstate").Specific;
                objDataSources.Add("cbPriCntState", "cbpcstate");

                cbPriCntCountry = oForm.Items.Item("cbpcoutry").Specific;
                objDataSources.Add("cbPriCntCountry", "cbpcoutry");

                txtSecCntName = oForm.Items.Item("txscname").Specific;
                objDataSources.Add("txtSecCntName", "txscname");

                txtSecCntRelation = oForm.Items.Item("txscrlt").Specific;
                objDataSources.Add("txtSecCntRelation", "txscrlt");

                txtSecCntNoLandLine = oForm.Items.Item("txsccnl").Specific;
                objDataSources.Add("txtSecCntNoLandLine", "txsccnl");

                txtSecCntNoMobile = oForm.Items.Item("txscnm").Specific;
                objDataSources.Add("txtSecCntNoMobile", "txscnm");

                txtSecCntAddress = oForm.Items.Item("txscadr").Specific;
                objDataSources.Add("txtSecCntAddress", "txscadr");


                txtSecCntCity = oForm.Items.Item("txsccity").Specific;
                objDataSources.Add("txtSecCntCity", "txsccity");

                cbSecCntState = oForm.Items.Item("cbscstate").Specific;
                objDataSources.Add("cbSecCntState", "cbscstate");

                cbSecCntCountry = oForm.Items.Item("cbsccntry").Specific;
                objDataSources.Add("cbSecCntCountry", "cbsccntry");


                //Administrator Tab Empty

                //Personal Detail Tab

                cbMartial = oForm.Items.Item("cbmartial").Specific;
                cbReligion = oForm.Items.Item("cbreligion").Specific;
                txtMotherName = oForm.Items.Item("txmother").Specific;
                txtSSNumber = oForm.Items.Item("txssno").Specific;
                txtUnionMemberShip = oForm.Items.Item("txums").Specific;
                txtUnionMemberShipNo = oForm.Items.Item("txumsno").Specific;
                txtNationality = oForm.Items.Item("txnation").Specific;

                cbOhemUser = oForm.Items.Item("cbohem").Specific;
                oForm.DataSources.UserDataSources.Add("cbohem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbOhemUser.DataBind.SetBound(true, "", "cbohem");

                txtIDCardNo = oForm.Items.Item("txidno").Specific;

                txtIDDtOfIssue = oForm.Items.Item("txisudt").Specific;
                oForm.DataSources.UserDataSources.Add("txisudt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtIDDtOfIssue.DataBind.SetBound(true, "", "txisudt");
                txtIDDtOfIssue.Value = DateTime.Now.ToString("yyyyMMdd");

                txtIDPlaceOfIssue = oForm.Items.Item("txidplcisu").Specific;
                txtIDIssuedBy = oForm.Items.Item("txidisuby").Specific;

                txtIDExpiryDate = oForm.Items.Item("txidexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txidexpdt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtIDExpiryDate.DataBind.SetBound(true, "", "txisudt");
                txtIDExpiryDate.Value = DateTime.Now.ToString("yyyyMMdd");

                txtPassportNo = oForm.Items.Item("txpssno").Specific;

                txtPassportDateofIssue = oForm.Items.Item("txpssdt").Specific;
                oForm.DataSources.UserDataSources.Add("txpssdt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtPassportDateofIssue.DataBind.SetBound(true, "", "txpssdt");
                txtPassportDateofIssue.Value = DateTime.Now.ToString("yyyyMMdd");

                txtPassportExpiry = oForm.Items.Item("txpssexpdt").Specific;
                oForm.DataSources.UserDataSources.Add("txpssexpdt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtPassportExpiry.DataBind.SetBound(true, "", "txpssexpdt");
                txtPassportExpiry.Value = DateTime.Now.ToString("yyyyMMdd");

                txtIncomeTax = oForm.Items.Item("txicnno").Specific;

                cbCostCenter = oForm.Items.Item("cbcc").Specific;
                oForm.DataSources.UserDataSources.Add("cbcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbCostCenter.DataBind.SetBound(true, "", "cbcc");

                cbGender = oForm.Items.Item("cbgender").Specific;
                oForm.DataSources.UserDataSources.Add("cbgender", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbGender.DataBind.SetBound(true, "", "cbgender");

                txtDOB = oForm.Items.Item("txtdob").Specific;
                oForm.DataSources.UserDataSources.Add("txtdob", SAPbouiCOM.BoDataType.dt_DATE);
                txtDOB.DataBind.SetBound(true, "", "txtdob");

                txtTermination = oForm.Items.Item("txter").Specific;
                oForm.DataSources.UserDataSources.Add("txter", SAPbouiCOM.BoDataType.dt_DATE);
                txtTermination.DataBind.SetBound(true, "", "txter");

                txtResignation = oForm.Items.Item("txresign").Specific;
                oForm.DataSources.UserDataSources.Add("txresign", SAPbouiCOM.BoDataType.dt_DATE);
                txtResignation.DataBind.SetBound(true, "", "txresign");

                //Salary Tab

                txtBasicSalary = oForm.Items.Item("txbsslry").Specific;
                oForm.DataSources.UserDataSources.Add("txbsslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtBasicSalary.DataBind.SetBound(true, "", "txbsslry");

                cbcontractType = oForm.Items.Item("cType").Specific;
                oForm.DataSources.UserDataSources.Add("cType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbcontractType.DataBind.SetBound(true, "", "cType");

                txtEmpCalendar = oForm.Items.Item("txempcal").Specific;
                oForm.DataSources.UserDataSources.Add("txempcal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmpCalendar.DataBind.SetBound(true, "", "txempcal");

                string query = @"SELECT HldCode FROM dbo.OHLD";

                Program.objHrmsUI.addFms("frm_EmpMst", "txempcal", "-1", query);

                cbSalaryCurrency = oForm.Items.Item("cbslrycur").Specific;
                oForm.DataSources.UserDataSources.Add("cbslrycur", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbSalaryCurrency.DataBind.SetBound(true, "", "cbslrycur");

                //txtEmpShift = oForm.Items.Item("txempshift").Specific;

                cbPaymentMode = oForm.Items.Item("cbpaymod").Specific;
                oForm.DataSources.UserDataSources.Add("cbpaymod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbPaymentMode.DataBind.SetBound(true, "", "cbpaymod");

                txtAccountTitle = oForm.Items.Item("txacctitle").Specific;
                oForm.DataSources.UserDataSources.Add("txacctitle", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtAccountTitle.DataBind.SetBound(true, "", "txacctitle");

                txtBankName = oForm.Items.Item("txbname").Specific;
                oForm.DataSources.UserDataSources.Add("txbname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtBankName.DataBind.SetBound(true, "", "txbname");

                query = @"SELECT BankCode, BankName FROM dbo.ODSC";

                Program.objHrmsUI.addFms("frm_EmpMst", "txbname", "-1", query);

                txtBankBranch = oForm.Items.Item("txbrnch").Specific;
                oForm.DataSources.UserDataSources.Add("txbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtBankBranch.DataBind.SetBound(true, "", "txbrnch");

                query = @"SELECT Branch, Account, BankCode FROM dbo.DSC1";

                Program.objHrmsUI.addFms("frm_EmpMst", "txbrnch", "-1", query);

                txtAccountNo = oForm.Items.Item("txaccno").Specific;
                oForm.DataSources.UserDataSources.Add("txaccno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtAccountNo.DataBind.SetBound(true, "", "txaccno");

                cbAccountType = oForm.Items.Item("cbacctype").Specific;
                oForm.DataSources.UserDataSources.Add("cbacctype", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbAccountType.DataBind.SetBound(true, "", "cbacctype");

                txtEffectiveDate = oForm.Items.Item("txeffdt").Specific;
                oForm.DataSources.UserDataSources.Add("txeffdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtEffectiveDate.DataBind.SetBound(true, "", "txeffdt");
                txtEffectiveDate.Value = DateTime.Now.ToString("yyyyMMdd");

                txtPercentage = oForm.Items.Item("txper").Specific;
                oForm.DataSources.UserDataSources.Add("txper", SAPbouiCOM.BoDataType.dt_SUM);
                txtPercentage.DataBind.SetBound(true, "", "txper");

                txtDateOfJoining = oForm.Items.Item("txdoj").Specific;
                oForm.DataSources.UserDataSources.Add("txdoj", SAPbouiCOM.BoDataType.dt_DATE);
                txtDateOfJoining.DataBind.SetBound(true, "", "txdoj");
                txtDateOfJoining.Value = DateTime.Now.ToString("yyyyMMdd");

                cbBloodGroup = oForm.Items.Item("cbblood").Specific;
                oForm.DataSources.UserDataSources.Add("cbblood", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbBloodGroup.DataBind.SetBound(true, "", "cbblood");

                //Payroll Tab

                cbPayroll = oForm.Items.Item("cbpayroll").Specific;
                oForm.DataSources.UserDataSources.Add("cbpayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbPayroll.DataBind.SetBound(true, "", "cbpayroll");

                //Communication  Tab

                txtWorkIM = oForm.Items.Item("txworkim").Specific;
                txtPersonalIM = oForm.Items.Item("txperim").Specific;
                txtPersonalContact = oForm.Items.Item("txpercnt").Specific;
                txtPersonalEmail = oForm.Items.Item("txperemail").Specific;

                //Classification 

                txtOrganizationUnit = oForm.Items.Item("txorgunit").Specific;

                //cbReportingManager = oForm.Items.Item("cbrman").Specific;
                //oForm.DataSources.UserDataSources.Add("cbrman", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                //cbReportingManager.DataBind.SetBound(true, "", "cbrman");

                txtReportTo = oForm.Items.Item("txreport").Specific;
                itxtReportTo = oForm.Items.Item("txreport");
                oForm.DataSources.UserDataSources.Add("txreport", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtReportTo.DataBind.SetBound(true, "", "txreport");

                txtEmpContractType = oForm.Items.Item("txempct").Specific;
                // txtEmpContractType.Active = false;
                txtHRCalendar = oForm.Items.Item("txhrcal").Specific;
                txtHRCalendar.Value = "1";
                txtWindowsLogin = oForm.Items.Item("txwinlg").Specific;

                txtEmpGrade = oForm.Items.Item("txempgrd").Specific;
                oForm.DataSources.UserDataSources.Add("txempgrd", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtEmpGrade.DataBind.SetBound(true, "", "txempgrd");
                txtEmpGrade.Value = "1";

                txtPreviosEmpMonth = oForm.Items.Item("txprempmnt").Specific;
                txtWorkPermitRef = oForm.Items.Item("txwrkref").Specific;

                txtWorkPermitExpiry = oForm.Items.Item("txwrkpexp").Specific;
                oForm.DataSources.UserDataSources.Add("txwrkpexp", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtWorkPermitExpiry.DataBind.SetBound(true, "", "txwrkpexp");
                txtWorkPermitExpiry.Value = DateTime.Now.ToString("yyyyMMdd");

                txtRemarks = oForm.Items.Item("txatch").Specific;
                oForm.DataSources.UserDataSources.Add("txatch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 200);
                txtRemarks.DataBind.SetBound(true, "", "txatch");


                txtContractExpiry = oForm.Items.Item("txconexp").Specific;
                oForm.DataSources.UserDataSources.Add("txconexp", SAPbouiCOM.BoDataType.dt_DATE, 20);
                txtContractExpiry.DataBind.SetBound(true, "", "txconexp");
                txtContractExpiry.Value = DateTime.Now.ToString("yyyyMMdd");

                //Arabic Localization Tab

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

                //Absence

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


                //Relatives

                mtRelatives = oForm.Items.Item("mtrelative").Specific;
                dtRelatives = oForm.DataSources.DataTables.Item("dtrelative");
                rIsNew = mtRelatives.Columns.Item("isnew");
                rIsNew.Visible = false;
                rId = mtRelatives.Columns.Item("id");
                rId.Visible = false;
                rSerial = mtRelatives.Columns.Item("serial");
                rType = mtRelatives.Columns.Item("type");
                rFirstName = mtRelatives.Columns.Item("firstname");
                rLastName = mtRelatives.Columns.Item("lastname");
                rTelephone = mtRelatives.Columns.Item("tele");
                rEmail = mtRelatives.Columns.Item("email");
                rDOB = mtRelatives.Columns.Item("dob");
                rDepencdent = mtRelatives.Columns.Item("depend");
                rMCNo = mtRelatives.Columns.Item("mcno");
                rMCStartDate = mtRelatives.Columns.Item("mcsdate");
                rMCExpiryDate = mtRelatives.Columns.Item("mcedate");

                AddEmptyRowRelatives();

                //Certification

                mtCertification = oForm.Items.Item("mtqlf").Specific;
                dtCertification = oForm.DataSources.DataTables.Item("dtcert");
                cSerial = mtCertification.Columns.Item("serial");
                cId = mtCertification.Columns.Item("id");
                cId.Visible = false;
                cIsNew = mtCertification.Columns.Item("isnew");
                cIsNew.Visible = true;
                cCertification = mtCertification.Columns.Item("cert");
                cAwardedBy = mtCertification.Columns.Item("awdby");
                cAwardStatus = mtCertification.Columns.Item("awdstatus");
                cDescription = mtCertification.Columns.Item("desc");
                cNotes = mtCertification.Columns.Item("notes");
                cValidated = mtCertification.Columns.Item("validated");

                AddEmptyRowCertification();

                //Past Experiance

                mtPastExperiance = oForm.Items.Item("mtpstexp").Specific;
                dtPastExperiance = oForm.DataSources.DataTables.Item("dtpstexp");
                pSerial = mtPastExperiance.Columns.Item("serial");
                pId = mtPastExperiance.Columns.Item("id");
                pId.Visible = false;
                pIsNew = mtPastExperiance.Columns.Item("isnew");
                pIsNew.Visible = false;
                pCompany = mtPastExperiance.Columns.Item("company");
                pFromdt = mtPastExperiance.Columns.Item("fromdt");
                pTodt = mtPastExperiance.Columns.Item("todt");
                pPosition = mtPastExperiance.Columns.Item("position");
                pDuties = mtPastExperiance.Columns.Item("duties");
                pNotes = mtPastExperiance.Columns.Item("note");
                pLastSalary = mtPastExperiance.Columns.Item("lsalary");

                AddEmptyRowPastExperiance();

                //Education
                mtEducation = oForm.Items.Item("mtedu").Specific;
                dtEducation = oForm.DataSources.DataTables.Item("dtins");
                eId = mtEducation.Columns.Item("id");
                eId.Visible = false;
                eIsNew = mtEducation.Columns.Item("isnew");
                eIsNew.Visible = false;
                eInstituteName = mtEducation.Columns.Item("insname");
                eFromDate = mtEducation.Columns.Item("fromdt");
                eToDate = mtEducation.Columns.Item("todt");
                eSubject = mtEducation.Columns.Item("subject");
                eQualification = mtEducation.Columns.Item("qlft");
                eAwardedQlf = mtEducation.Columns.Item("aqlft");
                eMark = mtEducation.Columns.Item("mark");
                eNotes = mtEducation.Columns.Item("notes");

                AddEmptyRowEducation();

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
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oForm.Freeze(false);
                
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: InitiallizeForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                File.Copy(FilePath, picPath,true);

                MstEmployee oImgEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpID select a).FirstOrDefault();

                if (oImgEmp != null)
                {
                    oImgEmp.ImgPath = picPath;
                }

                dbHrPayroll.SubmitChanges();
                

            }
            catch(Exception ex)
            {
                oApplication.StatusBar.SetText("Function : SaveImageFile Error : "+ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
       
        private void fillCbs()
        {
            FillPositionCombo(cbPosition);
            FillDepartmentCombo(cbDepartment);
            FillBranchCombo(cbBranch);
            //FillManagerCombo(cbManager);
            FillLocationsCombo(cbLocation);
            FillDesignationCombo(cbDesignation);
            FillJobTitleCombo(cbJobTitle);
            FillMartialCombo(cbMartial);
            FillReligionCombo(cbReligion);
            FillGenderCombo(cbGender);
            FillCostCenterCombo(cbCostCenter);
            FillSboUsrCombo(cbSBOLinkID);
            FillOHEMUserCombo(cbOhemUser);
            FillLovList(cbSalaryCurrency, "SalaryCurrency");
            FillLovList(cbPaymentMode, "PaymentMode");
            FillLovList(cbAccountType, "AccountType");
            FillBloodGroupCombo(cbBloodGroup);
            //FillManagerCombo(cbReportingManager);
            FillRelationShipCombo(rType);
            FillCertificationCombo(cCertification);          
            FillInstituteCombo(eInstituteName);
            FillQualificationCombo(eQualification);

            FillCountryCombo();
            FillStatesCombo();
            FillPayrollCombo(cbPayroll);
            FillShiftCombo();
            FillContractTypeCombo();
            oApplication.StatusBar.SetText("Employee Master Loaded Successfully.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        }
        
        private void InitiallizeDocument()
        {
            oForm.Freeze(true);
            try
            {
                //Header Area
                txtFirstName.Value = "";
                txtMiddleName.Value = "";
                txtLastName.Value = "";
                //txtJobTitle.Value = "";
                //chkCreateUser.Checked = false;
                txtUserCode.Value = "";
                txtPassword.Value = "";
                txtEmployeeCode.Value = "";
                txtInitials.Value = "";
                txtNamePrefix.Value = "";
                txtOfficePhn.Value = "";
                txtHomePhn.Value = "";
                txtExtention.Value = "";
                txtMobilePhn.Value = "";
                txtPager.Value = "";
                txtFax.Value = "";
                txtEmail.Value = "";
                cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbPosition.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbManager.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbJobTitle.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                //Address Tab

                txtHomeStreet.Value = "";
                txtHomeStreetNo.Value = "";
                txtHomeBlock.Value = "";
                txtHomeBuilding.Value = "";
                txtHomeZip.Value = "";
                txtHomeCity.Value = "";
                cbHomeState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbHomeCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtWorkStreet.Value = "";
                txtWorkStreetNo.Value = "";
                txtWorkBlock.Value = "";
                txtWorkBuilding.Value = "";
                txtWorkZip.Value = "";
                txtWorkCity.Value = "";
                cbWorkState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbWorkCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                //Emergency Detail Tab
                txtPriCntName.Value = "";
                txtPriCntRelation.Value = "";
                txtPriCntNoLandLine.Value = "";
                txtPriCntNoMobile.Value = "";
                txtPriCntAddress.Value = "";
                txtPriCntCity.Value = "";
                cbPriCntState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbPriCntCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                txtSecCntName.Value = "";
                txtSecCntRelation.Value = "";
                txtSecCntNoLandLine.Value = "";
                txtSecCntNoMobile.Value = "";
                txtSecCntAddress.Value = "";
                txtSecCntCity.Value = "";
                cbSecCntState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbSecCntCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                //Administrator Tab Empty

                //Personal Tab

                txtFatherName.Value = "";
                txtMotherName.Value = "";
                cbMartial.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbReligion.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbGender.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbCostCenter.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbBloodGroup.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtSSNumber.Value = "";
                txtUnionMemberShip.Value = "";
                txtUnionMemberShipNo.Value = "";
                txtNationality.Value = "";
                txtPassportNo.Value = "";
                txtPassportDateofIssue.Value = DateTime.Now.ToString("yyyyMMdd");
                txtPassportExpiry.Value = DateTime.Now.ToString("yyyyMMdd");
                txtIncomeTax.Value = "";
                txtIDCardNo.Value = "";
                txtIDDtOfIssue.Value = DateTime.Now.ToString("yyyyMMdd");
                txtIDExpiryDate.Value = DateTime.Now.ToString("yyyyMMdd");
                txtIDPlaceOfIssue.Value = "";
                txtIDIssuedBy.Value = "";

                //Salary Tab
                txtBasicSalary.Value = "";
                cbSalaryCurrency.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtEmpCalendar.Value = "";
                //txtEmpShift.Value = "";
                cbPaymentMode.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtAccountNo.Value = "";
                txtAccountTitle.Value = "";
                txtBankName.Value = "";
                txtBankBranch.Value = "";
                cbAccountType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtEffectiveDate.Value = DateTime.Now.ToString("yyyyMMdd");
                txtPercentage.Value = "";
                txtDateOfJoining.Value = DateTime.Now.ToString("yyyyMMdd");
                cbBloodGroup.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbcontractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //Payroll Tab
                cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                //Absence Tab

                dtAbsence.Rows.Clear();
                
                mtAbsence.LoadFromDataSource();

                //Communication Tab

                txtWorkIM.Value = "";
                txtPersonalIM.Value = "";
                txtPersonalContact.Value = "";
                txtPersonalEmail.Value = "";

                //Classification Tab

                txtOrganizationUnit.Value = "";
                txtReportTo.Value = "";
                txtEmpContractType.Value = "";
                txtHRCalendar.Value = "";
                txtWindowsLogin.Value = "";
                txtEmpGrade.Value = "";
                txtPreviosEmpMonth.Value = "";
                txtWorkPermitRef.Value = "";
                txtWorkPermitExpiry.Value = DateTime.Now.ToString("yyyyMMdd");
                txtContractExpiry.Value = DateTime.Now.ToString("yyyyMMdd");

                //Arabic Tab

                txtEnglishName.Value = "";
                txtArabicName.Value = "";
                txtPassportExpiryDtH.Value = "";
                txtIDExpiryDtH.Value = "";
                txtMedicalCardExpirydtH.Value = "";
                txtDrvLicCompletionDtH.Value = "";
                txtDrvLicLastDtH.Value = "";
                txtDrvLicReleaseDtH.Value = "";
                txtVisaNumber.Value = "";
                txtIqamaProfessional.Value = "";
                txtBankCardExpiryDtH.Value = "";


                //Relative Tab

                dtRelatives.Rows.Clear();
                AddEmptyRowRelatives();
                mtRelatives.LoadFromDataSource();

                //Qualification Tab

                dtCertification.Rows.Clear();
                AddEmptyRowCertification();
                mtCertification.LoadFromDataSource();

                //Past Experiance Tab

                dtPastExperiance.Rows.Clear();
                AddEmptyRowPastExperiance();
                mtPastExperiance.LoadFromDataSource();

                //Education Tab

                dtEducation.Rows.Clear();
                AddEmptyRowEducation();
                mtEducation.LoadFromDataSource();

                //Attachement Tab
                txtEmployeeCode.Active = true;
               
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
                
            }
            oForm.Freeze(false);
            txtEmployeeCode.Active = true;
        }

        private void GetData()
        {
            
            CodeIndex.Clear();
            oEmployees = from a in dbHrPayroll.MstEmployee select a;
            Int32 i = 0;
            foreach (MstEmployee oEmp in oEmployees)
            {
                CodeIndex.Add( oEmp.ID.ToString(),i);
                i++;
            }
            totalRecord = i;
        }

        public override void fillFields()
        {
            base.fillFields();
            
            oForm.Freeze(true);
            try
            {
                MstEmployee oEmp = oEmployees.ElementAt<MstEmployee>(currentRecord);

                //Header Area
              //oForm.DataSources.UserDataSources.Item("txtFirstName").ValueEx = oEmp.FirstName;

                currentObjId = oEmp.ID.ToString();
                txtFirstName.Value  = oEmp.FirstName;
                txtFirstName.Active = true;
                txtMiddleName.Value = oEmp .MiddleName;
                txtLastName.Value = oEmp.LastName;
                //txtJobTitle.Value = oEmp.JobTitle;
                if (Convert.ToBoolean(oEmp.FlgUser))
                {
                    txtUserCode.Value = oEmp.MstUsers.ElementAt(0).UserCode;
                    //cbSBOLinkID.Select(oEmp.MstUsers.ElementAt(0).UserCode != oEmp.FirstName ? oEmp.MstUsers.ElementAt(0).UserCode : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue); 
                    cbSBOLinkID.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    txtUserCode.Value = "";
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
                txtPassword.Value = oEmp.MstUsers.ElementAt(0).PassCode;
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
                    MstEmployee mngEmp = (from a in dbHrPayroll.MstEmployee where a.ID == oEmp.Manager select a).FirstOrDefault();
                    txtManager.Value = mngEmp.EmpID;
                }
                else
                {
                    txtManager.Value = "";
                }
                cbSBOLinkID.Select(oEmp.SboUserCode != null ? oEmp.SboUserCode.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
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
                cbHomeCountry.Select( oEmp.WACountry != null ? oEmp.WACountry.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                
                txtWorkStreet.Value = oEmp.HAStreet;
                txtWorkStreetNo.Value = oEmp.HAStreetNo;
                txtWorkBlock.Value = oEmp.HABlock;
                txtWorkBuilding.Value = oEmp.HAOther;
                txtWorkZip.Value = oEmp.HAZipCode;
                txtWorkCity.Value = oEmp.HACity;
                cbWorkState.Select(oEmp.HAState != null ? oEmp.HAState.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbWorkCountry.Select(oEmp.HACountry != null ? oEmp.HACountry.ToString() :"-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

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
                cbCostCenter.Select(oEmp.CostCenter != null ? oEmp.CostCenter.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
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
                
                txtIDPlaceOfIssue.Value = oEmp.IDPlaceofIssue;
                txtIDIssuedBy.Value = oEmp.IDIssuedBy;
                cbOhemUser.Select(oEmp.SBOEmpCode == null ? "-1" : oEmp.SBOEmpCode, SAPbouiCOM.BoSearchKey.psk_ByValue);

                //Salary Tab
                var shift = dbHrPayroll.TrnsAttendanceRegister.Where(tr => tr.EmpID == oEmp.ID && tr.Date == DateTime.Now.Date).FirstOrDefault();
                if (shift != null)
                {
                    int shiftID = shift.ShiftID.Value;
                    cbShift.Select(shiftID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                txtBasicSalary.Value = oEmp.BasicSalary.ToString();
                if (!String.IsNullOrEmpty(oEmp.PayrollID.ToString()))
                {
                    cbPayroll.Select(oEmp.PayrollID != null ? oEmp.PayrollID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

                }
                else
                {
                    cbPayroll.Select(oEmp.PayrollID != null ? oEmp.PayrollID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                if (!string.IsNullOrEmpty(oEmp.EmployeeContractType))
                {
                    cbcontractType.Select(oEmp.EmployeeContractType, SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbcontractType.Select("0", SAPbouiCOM.BoSearchKey.psk_Index);
                }
                cbSalaryCurrency.Select(String.IsNullOrEmpty(oEmp.SalaryCurrency) ? "-1" : oEmp.SalaryCurrency, SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtEmpCalendar.Value = oEmp.EmpCalender != null ? oEmp.EmpCalender.ToString() : "";
                //txtEmpShift.Value = "";
                cbPaymentMode.Select(oEmp.PaymentMode != null ? oEmp.PaymentMode.ToString(): "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbBloodGroup.Select(oEmp.BloodGroupID != null ? oEmp.BloodGroupID.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtAccountTitle.Value = oEmp.AccountTitle;
                txtBankName.Value = oEmp.BankName;
                txtBankBranch.Value = oEmp.BankBranch;
                txtAccountNo.Value = oEmp.AccountNo;
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

                //Payroll Tab
               
                //Absence Tab
                
                dtAbsence.Rows.Clear();
                FillLeaveDataGrid(oEmp.ID);

                //foreach (MstEmployeeLeaves One in oEmp.MstEmployeeLeaves)
                //{
                //    String iApprovedCode = "LV0006", iDraftCode = "LV0005";
                //    decimal Requested = 0, Approved = 0, Available = 0, Balance = 0, Used = 0;

                //    Requested = (from a in dbHrPayroll.TrnsLeavesRequest where a.EmpID == oEmp.ID && a.LeaveType == One.LeaveType && a.DocAprStatus == iDraftCode select a).Count();
                //    Approved = (from a in dbHrPayroll.TrnsLeavesRequest where a.EmpID == oEmp.ID && a.LeaveType == One.LeaveType && a.DocAprStatus == iApprovedCode select a).Count();
                //    Available = Convert.ToDecimal((One.LeavesCarryForward + One.LeavesEntitled));
                //    Used = Convert.ToDecimal(One.LeavesUsed);
                //    Balance = Convert.ToDecimal((One.LeavesCarryForward + One.LeavesEntitled)) - (Requested + Approved + Used);
                //    dtAbsence.Rows.Add(1);
                //    dtAbsence.SetValue(aIsNew.DataBind.Alias, dtAbsence.Rows.Count - 1 , "N");
                //    dtAbsence.SetValue(aID.DataBind.Alias, dtAbsence.Rows.Count - 1, One.ID);
                //    dtAbsence.SetValue(aDescription.DataBind.Alias, dtAbsence.Rows.Count - 1, One.MstLeaveType.Code);
                //    dtAbsence.SetValue(aBalance.DataBind.Alias, dtAbsence.Rows.Count - 1, Balance.ToString());
                //    dtAbsence.SetValue(aBalanceBF.DataBind.Alias, dtAbsence.Rows.Count - 1, One.LeavesCarryForward.ToString());
                //    dtAbsence.SetValue(aEntitled.DataBind.Alias, dtAbsence.Rows.Count - 1, One.LeavesEntitled.ToString());
                //    dtAbsence.SetValue(aTotalAvailable.DataBind.Alias, dtAbsence.Rows.Count - 1, Available.ToString());
                //    dtAbsence.SetValue(aUsed.DataBind.Alias, dtAbsence.Rows.Count - 1, One.LeavesUsed.ToString());
                //    dtAbsence.SetValue(aRequested.DataBind.Alias, dtAbsence.Rows.Count - 1, Requested.ToString());
                //    dtAbsence.SetValue(aApproved.DataBind.Alias, dtAbsence.Rows.Count - 1, Approved.ToString());
                    
                //}
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
                    MstEmployee rptMng = (from a in dbHrPayroll.MstEmployee where a.ReportToID == oEmp.ReportToID select a).FirstOrDefault();
                    txtReportTo.Value = rptMng.EmpID;
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
                if(oEmp.ContractExpiryDate != null)
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

                //Relative Tab

                dtRelatives.Rows.Clear();
                foreach (MstEmployeeRelatives One in oEmp.MstEmployeeRelatives)
                {
                    dtRelatives.Rows.Add(1);
                    dtRelatives.SetValue(rIsNew.DataBind.Alias, dtRelatives.Rows.Count - 1, "N");
                    dtRelatives.SetValue(rId.DataBind.Alias, dtRelatives.Rows.Count - 1, One.Id);
                    dtRelatives.SetValue(rType.DataBind.Alias, dtRelatives.Rows.Count - 1, One.RelativeID);
                    dtRelatives.SetValue(rFirstName.DataBind.Alias, dtRelatives.Rows.Count - 1, One.FirstName);
                    dtRelatives.SetValue(rLastName.DataBind.Alias, dtRelatives.Rows.Count - 1, One.LastName);
                    dtRelatives.SetValue(rTelephone.DataBind.Alias, dtRelatives.Rows.Count - 1, One.TelephoneNo);
                    dtRelatives.SetValue(rDOB.DataBind.Alias, dtRelatives.Rows.Count - 1, One.BOD);
                    dtRelatives.SetValue(rDepencdent.DataBind.Alias, dtRelatives.Rows.Count - 1, One.FlgDependent == true ? "Y" : "N");
                    dtRelatives.SetValue(rMCNo.DataBind.Alias, dtRelatives.Rows.Count - 1, One.MedicalCardNo);
                    dtRelatives.SetValue(rMCStartDate.DataBind.Alias, dtRelatives.Rows.Count - 1, One.MedicalCardStartDate);
                    dtRelatives.SetValue(rMCExpiryDate.DataBind.Alias, dtRelatives.Rows.Count - 1, One.MedicalCardExpiryDate);
                }
                mtRelatives.LoadFromDataSource();
                AddEmptyRowRelatives();
                //Qualification Tab

                dtCertification.Rows.Clear();
                foreach (MstEmployeeCertifications One in oEmp.MstEmployeeCertifications)
                {
                    dtCertification.Rows.Add(1);
                    dtCertification.SetValue(cIsNew.DataBind.Alias, dtCertification.Rows.Count - 1, "N");
                    dtCertification.SetValue(cId.DataBind.Alias, dtCertification.Rows.Count - 1, One.Id);
                    dtCertification.SetValue(cCertification.DataBind.Alias, dtCertification.Rows.Count - 1, One.CertificationID);
                    dtCertification.SetValue(cAwardedBy.DataBind.Alias, dtCertification.Rows.Count - 1, One.AwardedBy);
                    dtCertification.SetValue(cAwardStatus.DataBind.Alias, dtCertification.Rows.Count - 1, One.AwardStatus);
                    dtCertification.SetValue(cDescription.DataBind.Alias, dtCertification.Rows.Count - 1, One.Description);
                    dtCertification.SetValue(cNotes.DataBind.Alias, dtCertification.Rows.Count - 1, One.Notes);
                    dtCertification.SetValue(cValidated.DataBind.Alias, dtCertification.Rows.Count - 1, One.Validated);
                }
                mtCertification.LoadFromDataSource();
                AddEmptyRowCertification();
                
                //Past Experiance Tab

                dtPastExperiance.Rows.Clear();
                foreach (MstEmployeeExperience One in oEmp.MstEmployeeExperience)
                {
                    dtPastExperiance.Rows.Add(1);
                    dtPastExperiance.SetValue(pIsNew.DataBind.Alias, dtPastExperiance.Rows.Count - 1, "N");
                    dtPastExperiance.SetValue(pId.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Id);
                    dtPastExperiance.SetValue(pCompany.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.CompanyName);
                    dtPastExperiance.SetValue(pFromdt.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.FromDate);
                    dtPastExperiance.SetValue(pTodt.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.ToDate);
                    dtPastExperiance.SetValue(pPosition.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Position);
                    dtPastExperiance.SetValue(pDuties.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Duties);
                    dtPastExperiance.SetValue(pNotes.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Notes);
                    dtPastExperiance.SetValue(pLastSalary.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.LastSalary);
                }
                mtPastExperiance.LoadFromDataSource();
                AddEmptyRowPastExperiance();

                //Education Tab

                dtEducation.Rows.Clear();
                foreach (MstEmployeeEducation One in oEmp.MstEmployeeEducation)
                {
                    dtEducation.Rows.Add(1);
                    dtEducation.SetValue(eIsNew.DataBind.Alias, dtEducation.Rows.Count - 1, "N");
                    dtEducation.SetValue(eId.DataBind.Alias, dtEducation.Rows.Count - 1, One.Id);
                    dtEducation.SetValue(eInstituteName.DataBind.Alias, dtEducation.Rows.Count - 1, One.InstituteID);
                    dtEducation.SetValue(eFromDate.DataBind.Alias, dtEducation.Rows.Count - 1, One.FromDate);
                    dtEducation.SetValue(eToDate.DataBind.Alias, dtEducation.Rows.Count - 1, One.ToDate);
                    dtEducation.SetValue(eSubject.DataBind.Alias, dtEducation.Rows.Count - 1, One.Subject);
                    dtEducation.SetValue(eQualification.DataBind.Alias, dtEducation.Rows.Count - 1, One.QualificationID);
                    dtEducation.SetValue(eAwardedQlf.DataBind.Alias, dtEducation.Rows.Count - 1, One.AwardedQualification);
                    dtEducation.SetValue(eMark.DataBind.Alias, dtEducation.Rows.Count - 1, One.MarkGrade);
                    dtEducation.SetValue(eNotes.DataBind.Alias, dtEducation.Rows.Count - 1, One.Notes);

                }
                mtEducation.LoadFromDataSource();
                AddEmptyRowEducation();

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
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Employee Doesn't load Successfully. : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //btnMain.Caption = "Update";
            }
            oForm.Freeze(false);

        }
        
        private bool validateForm()
        {
            bool result = true;
            String UserCode;
            UserCode = txtUserCode.Value;

            if (String.IsNullOrEmpty(UserCode))
            {
                result = false;
                oApplication.StatusBar.SetText("UserCode is Empty", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
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
                    var GQuery = (from bb in dbHrPayroll.MstEmployeeLeaves
                                  where bb.EmpID == EmployeeID
                                  join cb in dbHrPayroll.MstLeaveType on bb.LeaveType equals cb.ID
                                  select new
                                  {
                                      ID = bb.ID,
                                      BalanceBF = Convert.ToString(bb.LeavesCarryForward) != null ? Convert.ToString(bb.LeavesCarryForward) : "0.00",
                                      Entitled = Convert.ToString(bb.LeavesEntitled) != null ? Convert.ToString(bb.LeavesEntitled) : "0.00",
                                      LeaveID = bb.LeaveType,
                                      CauseofLeave = cb.Description,
                                      TotalAvailable = Convert.ToString(bb.LeavesCarryForward + bb.LeavesEntitled),
                                      Used = Convert.ToString(bb.LeavesUsed) != null ? Convert.ToString(bb.LeavesUsed) : "0.00",
                                      RequestedLeaves = Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iDraftCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) != null ? Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iDraftCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) : "0.00",
                                      ApprovedLeaves = Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iApprovedCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) != null ? Convert.ToString(dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == EmployeeID && a.LeaveType == bb.LeaveType && a.DocAprStatus == iApprovedCode).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).FirstOrDefault().Amount) : "0.00"
                                  }).ToList();

                    foreach (var WD in GQuery)
                    {
                        decimal TotalAvailable = Convert.ToDecimal(WD.TotalAvailable);
                        decimal TotalUsed = Convert.ToDecimal(WD.Used);
                        decimal TotalApproved = Convert.ToDecimal(WD.ApprovedLeaves);
                        decimal TotalRequested = Convert.ToDecimal(WD.RequestedLeaves);
                        decimal RemainingTotal = TotalAvailable - (TotalUsed + TotalApproved + TotalRequested);
                        dtAbsence.Rows.Add(1);
                        dtAbsence.SetValue(aIsNew.DataBind.Alias, dtAbsence.Rows.Count - 1, "N");
                        dtAbsence.SetValue(aID.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.ID);
                        dtAbsence.SetValue(aDescription.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.CauseofLeave);                        
                        dtAbsence.SetValue(aBalanceBF.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.BalanceBF.ToString());
                        dtAbsence.SetValue(aEntitled.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.Entitled.ToString());
                        dtAbsence.SetValue(aTotalAvailable.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.TotalAvailable.ToString());
                        dtAbsence.SetValue(aUsed.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.Used.ToString());
                        dtAbsence.SetValue(aRequested.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.RequestedLeaves.ToString());
                        dtAbsence.SetValue(aApproved.DataBind.Alias, dtAbsence.Rows.Count - 1, WD.ApprovedLeaves.ToString());
                        dtAbsence.SetValue(aBalance.DataBind.Alias, dtAbsence.Rows.Count - 1, RemainingTotal.ToString());
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
            SearchKeyVal.Add("emp.EmpID", txtEmployeeCode.Value.Trim() );
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
            SearchKeyVal.Add("emp.FlgActive", chkActiveEmployee.Checked==true?"1":""  );
           
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
                        getRecord(currentObjId);
                       
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
            }
            //else if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE || oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
            else// if ( btnMain.Caption == "Update" || btnMain.Caption == "Add")
            {
                if (validateForm())
                {
                    submitForm();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
            }
        }
        
        private void submitForm()
        {
            try
            {
                //Variable 
                Int32 SBOEmpCode = 0;
                String iFirstName, iMiddleName, iLastName;
                //Check Wheather That employ exist in db
                //then create an object 

                MstEmployee oEmp;
                MstUsers oUsr;
                int cnt = ( from p in dbHrPayroll.MstEmployee where p.EmpID == txtEmployeeCode.Value.Trim() select p).Count();
                if(cnt==0)
                {
                     oEmp   = new MstEmployee();
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
                     oEmp.MstUsers.Add(oUsr);
                     dbHrPayroll.MstEmployee.InsertOnSubmit(oEmp);
                }
                else
                {
                    
                   // oEmp = oEmployees.ElementAt(currentRecord); 
                    oEmp = (from p in dbHrPayroll.MstEmployee where p.ID.ToString() == currentObjId  select p).Single();
                    oUsr = oEmp.MstUsers.ElementAt(0);
                }
                MstLanguages oLan = (from a in dbHrPayroll.MstLanguages where a.Name.Contains(Program.sboLanguage) select a).FirstOrDefault<MstLanguages>();
                oUsr.UserCode = cbSBOLinkID.Value.Trim() == "-1" ? txtUserCode.Value.Trim() : cbSBOLinkID.Value.Trim();
                oUsr.UserID = txtUserCode.Value.Trim();
                oUsr.MstLanguages = oLan;
                oUsr.PassCode = txtPassword.Value.Trim() != "" ? txtPassword.Value.Trim() : "12345";
                oUsr.Language = oLan.Id;
                oUsr.FlgActiveUser = true;
                oUsr.FlgWebUser = true;
                oUsr.UpdateDate = DateTime.Now;
                oUsr.UpdatedBy = oCompany.UserName;
                
                iFirstName = txtFirstName.Value.Trim();
                oEmp.FirstName = iFirstName;
                iMiddleName = txtMiddleName.Value.Trim();
                oEmp.MiddleName = iMiddleName;
                iLastName = txtLastName.Value.Trim();
                oEmp.LastName = iLastName;                
                oEmp.SBOEmpCode = null;
                oEmp.SboUserCode = cbSBOLinkID.Value.Trim() == "-1" ? "-1" : cbSBOLinkID.Value.Trim();
                oEmp.FatherName = txtFatherName.Value.Trim();               
                if (cbJobTitle.Value.Trim() != "-1")
                {
                    oEmp.JobTitle = cbJobTitle.Value.Trim();
                }
                else
                {
                    oEmp.JobTitle = null;
                }

                if ( cbPosition.Value.Trim() != "-1")
                {
                    Int32 PositionID = Convert.ToInt32( cbPosition.Value.Trim());
                    MstPosition oPosition = (from a in dbHrPayroll.MstPosition where a.Id == PositionID select a).FirstOrDefault();
                    oEmp.PositionID = oPosition.Id;
                    oEmp.PositionName = oPosition.Name;
                }
                else
                {
                    oEmp.PositionID = null;
                }
                if (cbDepartment.Value.Trim() != "-1")
                {
                    Int32 DeptID = Convert.ToInt32(cbDepartment.Value.Trim());
                    MstDepartment oDept = (from a in dbHrPayroll.MstDepartment where a.ID == DeptID select a ).FirstOrDefault();
                    oEmp.DepartmentID = oDept.ID;
                    oEmp.DepartmentName = oDept.DeptName;
                }
                else
                {
                    oEmp.DepartmentID = null;
                }
                if (cbDesignation.Value.Trim() != "-1")
                {
                    Int32 DesigID = Convert.ToInt32(cbDesignation.Value.Trim());
                    MstDesignation oDesig = (from a in dbHrPayroll.MstDesignation where a.Id == DesigID select a).FirstOrDefault();
                    oEmp.DesignationID = oDesig.Id;
                    oEmp.DesignationName = oDesig.Name;
                }
                else
                {
                    oEmp.DesignationID = null;
                    oEmp.DesignationName = "";
                }
                if (cbBranch.Value.Trim() != "-1")
                {
                    Int32 BranchID = Convert.ToInt32(cbBranch.Value.Trim());  
                    MstBranches oBranch = (from a in dbHrPayroll.MstBranches where a.Id == BranchID select a).FirstOrDefault();
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
                    MstEmployee oMng = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtManager.Value.Trim() select a).FirstOrDefault();
                    oEmp.Manager = oMng.ID;
                    oMng = null;
                }
                else
                {
                    oEmp.Manager = null;
                }
                oEmp.FlgUser = true;
                if ( !String.IsNullOrEmpty(txtEmployeeCode.Value))
                {
                    oEmp.EmpID = txtEmployeeCode.Value;
                }
                else
                {
                    oApplication.MessageBox( Program.objHrmsUI.getStrMsg("Inf_EmpID"),1,"Ok");                 
                    return;
                }

                if (cbLocation.Value.Trim() != "-1")
                {
                    Int32 LocId = Convert.ToInt32(cbLocation.Value);
                    MstLocation Location = (from a in dbHrPayroll.MstLocation where a.Id == LocId select a).FirstOrDefault();
                    oEmp.Location = Location.Id;
                    oEmp.LocationName = Location.Name;
                }
                else
                {
                    oEmp.Location = null;
                    oEmp.LocationName = "";
                }
                
                oEmp.Initials = txtInitials.Value.Trim();
                oEmp.NamePrefix = txtNamePrefix.Value.Trim();
                oEmp.OfficePhone = txtOfficePhn.Value.Trim();
                oEmp.OfficeExtension = txtExtention.Value.Trim();
                oEmp.OfficeMobile = txtMobilePhn.Value.Trim();
                oEmp.Pager = txtPager.Value.Trim();
                oEmp.HomePhone = txtHomePhn.Value.Trim();
                oEmp.Fax = txtFax.Value.Trim();
                oEmp.OfficeEmail = txtEmail.Value.Trim();
                oEmp.FlgActive = chkActiveEmployee.Checked;
                

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
                oEmp.HAState = cbWorkState.Value != "-1" ? cbWorkState.Value.Trim() :null;

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
                oEmp.SecCountry = cbSecCntCountry.Value != "-1" ? cbSecCntCountry.Value.Trim(): null;
                oEmp.SecState = cbSecCntState.Value != "-1" ? cbSecCntState.Value.Trim() : null;

                oEmp.MartialStatusID = cbMartial.Value != "-1" ? cbMartial.Value.Trim() : null;
                oEmp.MartialStatusLOVType = "Marital";
                oEmp.ReligionID = cbReligion.Value != "-1" ? cbReligion.Value.Trim() : null;
                oEmp.ReligionLOVType = "Religion";
                oEmp.SocialSecurityNo = txtSSNumber.Value.Trim();
                oEmp.EmpUnion = txtUnionMemberShip.Value.Trim();
                oEmp.UnionMembershipNo = txtUnionMemberShipNo.Value.Trim();
                oEmp.IDNo = txtIDCardNo.Value.Trim();
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

                //Salary Tab
                if (txtBasicSalary.Value != "")
                {
                    oEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                }
                else
                {
                    oEmp.BasicSalary = 0.0M;
                }
                if (!string.IsNullOrEmpty(cbShift.Value) && Convert.ToInt32(cbShift.Value.Trim()) > 0)
                {
                    string PayrollId = cbPayroll.Value.Trim();
                    if (PayrollId != "-1")
                    {
                        if (oEmp.ID > 0)
                        {
                            CfgPayrollDefination Payroll = (from a in dbHrPayroll.CfgPayrollDefination where a.ID == Convert.ToInt32(PayrollId) select a).FirstOrDefault();
                            var oOldVal = dbHrPayroll.TrnsAttendanceRegister.Where(tr => tr.EmpID == oEmp.ID && tr.Date == DateTime.Now.Date).FirstOrDefault();
                            var PeriodId = dbHrPayroll.CfgPeriodDates.Where(pd => pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate && pd.PayrollId == Payroll.ID).FirstOrDefault();
                            if (oOldVal != null)
                            {
                                oOldVal.PeriodID = PeriodId.ID;
                                oOldVal.ShiftID = Convert.ToInt32(cbShift.Value.Trim());
                                oOldVal.UpdateDate = DateTime.Now;
                            }
                            else
                            {
                                TrnsAttendanceRegister attendance = new TrnsAttendanceRegister();
                                attendance.EmpID = oEmp.ID;
                                attendance.PeriodID = PeriodId.ID;
                                attendance.Date = DateTime.Now.Date;
                                attendance.ShiftID = Convert.ToInt32(cbShift.Value);
                                attendance.CreateDate = DateTime.Now;
                                attendance.UserId = oCompany.UserName;
                                attendance.Processed = false;
                                dbHrPayroll.TrnsAttendanceRegister.InsertOnSubmit(attendance);
                            }
                        }
                    }
                }
                oEmp.SalaryCurrency = cbSalaryCurrency.Value.Trim();//txtSalaryCurrency.Value.Trim();
                oEmp.EmpCalender = txtEmpCalendar.Value.Trim();
                oEmp.EmployeeContractType = cbcontractType.Value.Trim();
                //UpdateEmp.shift Read Only Field in UI
                oEmp.AccountTitle = txtAccountTitle.Value.Trim();
                oEmp.AccountNo = txtAccountNo.Value.Trim();
                oEmp.BankName = txtBankName.Value.Trim();
                oEmp.BankBranch = txtBankBranch.Value.Trim();
                oEmp.PercentagePaid = Convert.ToDecimal(txtPercentage.Value.Trim());
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
                    MstEmployee oRpt = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtReportTo.Value.Trim() select a).FirstOrDefault();
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

                //Relative Tab
                mtRelatives.FlushToDataSource();
                
                if (dtRelatives.Rows.Count > 0 )
                {
                    for (Int32 i = 0; i < dtRelatives.Rows.Count; i++)
                    {
                        int rid;
                        string RelativeCode, risnew, fname, lname, telephone, email, mccardno;
                        DateTime DateOfBirth, MCExpiryDate, MCStartDate;
                        RelativeCode = dtRelatives.GetValue(rType.DataBind.Alias, i);
                        risnew = Convert.ToString(dtRelatives.GetValue(rIsNew.DataBind.Alias,i));
                        if (risnew == "Y" )
                        {

                            if (string.IsNullOrEmpty(RelativeCode)) continue;
                            rid = Convert.ToInt32(dtRelatives.GetValue(rId.DataBind.Alias, i));
                       
                            fname = dtRelatives.GetValue(rFirstName.DataBind.Alias, i);
                            lname = dtRelatives.GetValue(rLastName.DataBind.Alias, i);
                            telephone = dtRelatives.GetValue(rTelephone.DataBind.Alias, i);
                            email = dtRelatives.GetValue(rEmail.DataBind.Alias, i);
                            DateOfBirth = dtRelatives.GetValue(rDOB.DataBind.Alias, i);
                            MCStartDate = dtRelatives.GetValue(rMCStartDate.DataBind.Alias, i);
                            MCExpiryDate = dtRelatives.GetValue(rMCExpiryDate.DataBind.Alias, i);
                            mccardno = dtRelatives.GetValue(rMCNo.DataBind.Alias, i);
                            
                            MstEmployeeRelatives oRelative = new MstEmployeeRelatives();
                            oRelative.RelativeID = RelativeCode;
                            oRelative.RelativeLOVType = "Relative";
                            oRelative.FirstName = fname;
                            oRelative.LastName = lname;
                            oRelative.TelephoneNo = telephone;
                            oRelative.Email = email;
                            oRelative.MedicalCardNo = mccardno;
                            oRelative.MedicalCardStartDate = MCStartDate;
                            oRelative.MedicalCardExpiryDate = MCExpiryDate;
                            oRelative.BOD = DateOfBirth;
                            if (dtRelatives.GetValue(rDepencdent.DataBind.Alias, i) == "Y")
                            {
                                oRelative.FlgDependent = true;
                            }
                            else
                            {
                                oRelative.FlgDependent = false;
                            }
                            
                            oEmp.MstEmployeeRelatives.Add(oRelative);
                        }
                        else if( risnew == "N" )
                        {
                            rid = Convert.ToInt32(dtRelatives.GetValue(rId.DataBind.Alias, i));
                       
                            fname = dtRelatives.GetValue(rFirstName.DataBind.Alias, i);
                            lname = dtRelatives.GetValue(rLastName.DataBind.Alias, i);
                            telephone = dtRelatives.GetValue(rTelephone.DataBind.Alias, i);
                            email = dtRelatives.GetValue(rEmail.DataBind.Alias, i);
                            DateOfBirth = dtRelatives.GetValue(rDOB.DataBind.Alias, i);
                            MCStartDate = dtRelatives.GetValue(rMCStartDate.DataBind.Alias, i);
                            MCExpiryDate = dtRelatives.GetValue(rMCExpiryDate.DataBind.Alias, i);
                            mccardno = dtRelatives.GetValue(rMCNo.DataBind.Alias, i);
                            MstEmployeeRelatives oUpd = (from a in dbHrPayroll.MstEmployeeRelatives where a.Id == rid select a).FirstOrDefault();
                            oUpd.RelativeID = RelativeCode;
                            oUpd.RelativeLOVType = "Relative";
                            oUpd.FirstName = fname;
                            oUpd.LastName = lname;
                            oUpd.TelephoneNo = telephone;
                            oUpd.Email = email;
                            oUpd.MedicalCardNo = mccardno;
                            oUpd.MedicalCardStartDate = MCStartDate;
                            oUpd.MedicalCardExpiryDate = MCExpiryDate;
                            oUpd.BOD = DateOfBirth;
                            if (dtRelatives.GetValue(rDepencdent.DataBind.Alias, i) == "Y")
                            {
                                oUpd.FlgDependent = true;
                            }
                            else
                            {
                                oUpd.FlgDependent = false;
                            }
                        }
                    }
                }

                //Certification

                mtCertification.FlushToDataSource();
                if (dtCertification.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCertification.Rows.Count; i++)
                    {
                        int cid;
                        string certification, description, notes, awardstatus, awardby, validation, cisnew;
                        cid = dtCertification.GetValue(cId.DataBind.Alias, i);
                        cisnew = dtCertification.GetValue(cIsNew.DataBind.Alias, i);
                        certification = dtCertification.GetValue(cCertification.DataBind.Alias,i);
                        if ( cisnew == "Y")
                        {
                            if (string.IsNullOrEmpty(certification)) continue;
                            description = dtCertification.GetValue(cDescription.DataBind.Alias, i);
                            notes = dtCertification.GetValue(cNotes.DataBind.Alias, i);
                            awardby = dtCertification.GetValue(cAwardedBy.DataBind.Alias, i);
                            awardstatus = dtCertification.GetValue(cAwardStatus.DataBind.Alias, i);
                            validation = dtCertification.GetValue(cValidated.DataBind.Alias, i);
                            MstEmployeeCertifications oNew = new MstEmployeeCertifications();
                            oNew.CertificationID = Convert.ToInt32(certification);
                            oNew.Description = description;
                            oNew.AwardedBy = awardby;
                            oNew.AwardStatus = awardstatus;
                            oNew.Notes = notes;
                            oNew.Validated = validation;
                            oEmp.MstEmployeeCertifications.Add(oNew);
                        }
                        else if( cisnew == "N")
                        {
                            description = dtCertification.GetValue(cDescription.DataBind.Alias, i);
                            notes = dtCertification.GetValue(cNotes.DataBind.Alias, i);
                            awardby = dtCertification.GetValue(cAwardedBy.DataBind.Alias, i);
                            awardstatus = dtCertification.GetValue(cAwardStatus.DataBind.Alias, i);
                            validation = dtCertification.GetValue(cValidated.DataBind.Alias, i);
                            MstEmployeeCertifications oNew = (from a in dbHrPayroll.MstEmployeeCertifications where a.Id == cid select a).FirstOrDefault();
                            oNew.CertificationID = Convert.ToInt32(certification);
                            oNew.Description = description;
                            oNew.AwardedBy = awardby;
                            oNew.AwardStatus = awardstatus;
                            oNew.Notes = notes;
                            oNew.Validated = validation;
                        }
                    }
                }

                //Past Experiance
                mtPastExperiance.FlushToDataSource();
                if (dtPastExperiance.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPastExperiance.Rows.Count; i++)
                    {
                        int pid;
                        string companay, position, duties, notes, lastsalary, pisnew;
                        DateTime dtfrom, dtto;
                        pid = dtPastExperiance.GetValue(pId.DataBind.Alias, i);
                        pisnew = dtPastExperiance.GetValue(pIsNew.DataBind.Alias, i);
                        companay = dtPastExperiance.GetValue(pCompany.DataBind.Alias, i);
                        if (pisnew == "Y")
                        {
                            if (string.IsNullOrEmpty(companay)) continue;
                            position = dtPastExperiance.GetValue(pPosition.DataBind.Alias, i);
                            duties = dtPastExperiance.GetValue(pDuties.DataBind.Alias, i);
                            notes = dtPastExperiance.GetValue(pNotes.DataBind.Alias, i);
                            lastsalary = dtPastExperiance.GetValue(pLastSalary.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtPastExperiance.GetValue(pFromdt.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtPastExperiance.GetValue(pTodt.DataBind.Alias,i));
                            MstEmployeeExperience oNew = new MstEmployeeExperience();
                            oNew.CompanyName = companay;
                            oNew.Position = position;
                            oNew.Duties = duties;
                            oNew.Notes = notes;
                            oNew.LastSalary = lastsalary;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                            oEmp.MstEmployeeExperience.Add(oNew);
                        }
                        else if (pisnew == "N")
                        {
                            position = dtPastExperiance.GetValue(pPosition.DataBind.Alias, i);
                            duties = dtPastExperiance.GetValue(pDuties.DataBind.Alias, i);
                            notes = dtPastExperiance.GetValue(pNotes.DataBind.Alias, i);
                            lastsalary = dtPastExperiance.GetValue(pLastSalary.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtPastExperiance.GetValue(pFromdt.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtPastExperiance.GetValue(pTodt.DataBind.Alias, i));
                            MstEmployeeExperience oNew = (from a in dbHrPayroll.MstEmployeeExperience where a.Id == pid select a).FirstOrDefault();
                            oNew.CompanyName = companay;
                            oNew.Position = position;
                            oNew.Duties = duties;
                            oNew.Notes = notes;
                            oNew.LastSalary = lastsalary;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                        }
                    }
                }

                //Education

                
                mtEducation.FlushToDataSource();
                if (dtEducation.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEducation.Rows.Count; i++)
                    {
                        int eid = 0, institute = 0, qualification = 0;
                        string eisnew, subject, awardqlfy, marks, notes;
                        DateTime dtfrom, dtto;
                        eid = dtEducation.GetValue(eId.DataBind.Alias, i);
                        eisnew = dtEducation.GetValue(eIsNew.DataBind.Alias, i);
                        institute = dtEducation.GetValue(eInstituteName.DataBind.Alias, i);
                        qualification = dtEducation.GetValue(eQualification.DataBind.Alias, i);
                        if (eisnew == "Y")
                        {
                            if (institute == -1) continue;
                            if (qualification == -1) continue;
                            subject = dtEducation.GetValue(eSubject.DataBind.Alias, i);
                            awardqlfy = dtEducation.GetValue(eAwardedQlf.DataBind.Alias, i);
                            marks = dtEducation.GetValue(eMark.DataBind.Alias, i);
                            notes = dtEducation.GetValue(eNotes.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtEducation.GetValue(eFromDate.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtEducation.GetValue(eToDate.DataBind.Alias,i));
                            MstEmployeeEducation oNew = new MstEmployeeEducation();
                            oNew.InstituteID = institute;
                            oNew.QualificationID = qualification;
                            oNew.Subject = subject;
                            oNew.AwardedQualification = awardqlfy;
                            oNew.MarkGrade = marks;
                            oNew.Notes = notes;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            oNew.UserId = oCompany.UserName;
                            oNew.UpdatedBy = oCompany.UserName;
                            oEmp.MstEmployeeEducation.Add(oNew);
                        }
                        else if (eisnew == "N")
                        {
                            subject = dtEducation.GetValue(eSubject.DataBind.Alias, i);
                            awardqlfy = dtEducation.GetValue(eAwardedQlf.DataBind.Alias, i);
                            marks = dtEducation.GetValue(eMark.DataBind.Alias, i);
                            notes = dtEducation.GetValue(eNotes.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtEducation.GetValue(eFromDate.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtEducation.GetValue(eToDate.DataBind.Alias, i));
                            MstEmployeeEducation oNew = (from a in dbHrPayroll.MstEmployeeEducation where a.Id == eid select a).FirstOrDefault();
                            oNew.InstituteID = institute;
                            oNew.QualificationID = qualification;
                            oNew.Subject = subject;
                            oNew.AwardedQualification = awardqlfy;
                            oNew.MarkGrade = marks;
                            oNew.Notes = notes;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            oNew.UserId = oCompany.UserName;
                            oNew.UpdatedBy = oCompany.UserName;
                        }
                    }
                }

                
                //Payroll Assignment 

                string PayrollIdx = cbPayroll.Value.Trim();
                if (PayrollIdx != "-1")
                {
                    CfgPayrollDefination Payroll = (from a in dbHrPayroll.CfgPayrollDefination where a.ID == Convert.ToInt32(PayrollIdx) select a).FirstOrDefault();
                    oEmp.PayrollID = Payroll.ID;
                    oEmp.PayrollName = Payroll.PayrollName;
                }
                else
                {
                    oEmp.PayrollID = null;
                    oEmp.PayrollName = "";
                }


                oEmp.Remarks = txtRemarks.Value.Trim();

                oEmp.IntSboPublished = false;
                oEmp.UpdateDate = DateTime.Now;
                oEmp.UpdatedBy = oCompany.UserName;
                
                dbHrPayroll.SubmitChanges();
                ds.updateStandardElements(oEmp,true);               
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
                //Salary Tab
                UpdateEmp.BasicSalary = Convert.ToDecimal(txtBasicSalary.Value.Trim());
                UpdateEmp.SalaryCurrency = cbSalaryCurrency.Value.Trim();//txtSalaryCurrency.Value.Trim();
                UpdateEmp.EmpCalender = txtEmpCalendar.Value.Trim();
                //UpdateEmp.shift Read Only Field in UI
                UpdateEmp.AccountTitle = txtAccountTitle.Value.Trim();
                UpdateEmp.AccountNo = txtAccountNo.Value.Trim();
                UpdateEmp.BankName = txtBankName.Value.Trim();
                UpdateEmp.BankBranch = txtBankBranch.Value.Trim();
                UpdateEmp.PercentagePaid = Convert.ToDecimal( txtPercentage.Value.Trim());
                UpdateEmp.AccountType = cbAccountType.Value.Trim();
                UpdateEmp.EffectiveDate = DateTime.ParseExact(txtEffectiveDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.JoiningDate = DateTime.ParseExact(txtDateOfJoining.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                UpdateEmp.BloodGroupID = cbBloodGroup.Value.Trim();
                UpdateEmp.BloodGroupLOVType = "BloodGroup";
                UpdateEmp.PaymentMode = cbPaymentMode.Value.Trim();

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

        private void FillPositionCombo( SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllPositions = from a in dbHrPayroll.MstPosition select a;
                pCombo.ValidValues.Add("-1", "");
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
                var Collection = from a in dbHrPayroll.MstJobTitle select a;
                pCombo.ValidValues.Add("-1", "");
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

        private void FillDepartmentCombo( SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllDepartment = from a in dbHrPayroll.MstDepartment select a;
                pCombo.ValidValues.Add("-1", "");
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
                IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation select a;
                pCombo.ValidValues.Add("-1", "");
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

        private void FillBranchCombo( SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllBranches = from a in dbHrPayroll.MstBranches select a;
                pCombo.ValidValues.Add("-1", "");
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
            pCombo.ValidValues.Add("-1", "");
            foreach (MstEmployee Emp in AllEmployee)
            {
                pCombo.ValidValues.Add(Convert.ToString(Emp.ID), Convert.ToString(Emp.FirstName + " " + Emp.MiddleName + " " + Emp.LastName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                
        }

        private void FillUserCodeCombo(SAPbouiCOM.ComboBox pCombo)
        {
            var AllUsers = from a in dbHrPayroll.MstUsers select a;
            pCombo.ValidValues.Add("-1", "");
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
                pCombo.ValidValues.Add("-1", "");
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

        private void FillShiftCombo()
        {
            try
            {
                var ShiftMaster = dbHrPayroll.MstShifts.Where(s => s.StatusShift == true).ToList();
                if (ShiftMaster != null && ShiftMaster.Count > 0)
                {
                    cbShift.ValidValues.Add("-1", "");
                    foreach (var Prl in ShiftMaster)
                    {
                        cbShift.ValidValues.Add(Convert.ToString(Prl.Id), Convert.ToString(Prl.Description));
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
                    cbcontractType.ValidValues.Add("-1", "");
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
            IEnumerable<MstCountry> Countries = from a in dbHrPayroll.MstCountry select a;
            cbWorkCountry.ValidValues.Add("-1","");
            cbHomeCountry.ValidValues.Add("-1", "");
            cbPriCntCountry.ValidValues.Add("-1", "");
            cbSecCntCountry.ValidValues.Add("-1","");
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
            cbWorkState.ValidValues.Add("-1", "");
            cbHomeState.ValidValues.Add("-1","");
            cbPriCntState.ValidValues.Add("-1","");
            cbSecCntState.ValidValues.Add("-1","");
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
                pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE One in MartialStatus)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Code), Convert.ToString(One.Value));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}",Ex.Message);
            }
        }

        private void FillGenderCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> Gender = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Gender") select a;
                pCombo.ValidValues.Add("-1", "");
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
                pCombo.ValidValues.Add("-1", "");
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
                IEnumerable<MstLOVE> Religions = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Religion") select a;
                pCombo.ValidValues.Add("-1", "");
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
                pCombo.ValidValues.Add("-1", "");
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

        private void FillInstituteCombo( SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstInstitute> Collection = from a in dbHrPayroll.MstInstitute select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstInstitute One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Code);
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
                pCombo.ValidValues.Add("-1", "");
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
                pCombo.ValidValues.Add("-1", "");
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
                pCombo.ValidValues.Add("-1", "");
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
                string strSql = "select user_code , U_NAME from " + oCompany.CompanyDB + ".dbo.ousr";
                DataTable dtUsr = ds.getDataTable(strSql);
                pCombo.ValidValues.Add("-1", "");
                foreach (DataRow dr in dtUsr.Rows)
                {
                    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                }
                //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillOHEMUserCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strSql = "select empID , firstName from " + oCompany.CompanyDB + ".dbo.ohem";
                DataTable dtUsr = ds.getDataTable(strSql);

                pCombo.ValidValues.Add("-1", "");
                foreach (DataRow dr in dtUsr.Rows)
                {
                    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                }
                //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillCostCenterCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strSql = "select PrcCode, PrcName from " + oCompany.CompanyDB + ".dbo.oprc";
                DataTable dtUsr = ds.getDataTable(strSql);

                pCombo.ValidValues.Add("-1", "");
                foreach (DataRow dr in dtUsr.Rows)
                {
                    pCombo.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                }
                //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

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

        private Int32 CreateOHEM(String EmpID, String pFirstName, String pMiddleName, String pLastName)
        {
            int retValue = 0;
            //if (Program.systemInfo.SAPB1Integration != true || oForm.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE) return;
            try
            {
                String EmpCodeFromSAP = "";
                SAPbobsCOM.EmployeesInfo nEmp = (SAPbobsCOM.EmployeesInfo)Program.objHrmsUI.oDiCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oEmployeesInfo);
                nEmp.FirstName = pFirstName;
                nEmp.MiddleName = pMiddleName;
                nEmp.LastName = pLastName;
                nEmp.UserFields.Fields.Item("U_HrmsEmpId").Value = EmpID;
                if (nEmp.Add() == 0)
                {
                    //oCompany.GetNewObjectCode( out EmpCodeFromSAP);
                    Program.objHrmsUI.oDiCompany.GetNewObjectCode(out EmpCodeFromSAP);
                    retValue = Convert.ToInt32(EmpCodeFromSAP);
                }
                else
                {
                    oApplication.SetStatusBarMessage("Error In Integrating with OHEM & EmpMaster.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
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

        private void SyncToSBO()
        {
            try
            {
                IEnumerable<MstEmployee> oEmpCollection = from a in dbHrPayroll.MstEmployee
                                                          where a.IntSboTransfered == false && a.FlgActive == true
                                                          select a;
                Int32 CountDB = (from a in dbHrPayroll.MstEmployee
                                where a.IntSboTransfered != true && a.FlgActive == true
                                select a).Count();

                if (CountDB > 0)
                {
                    foreach (MstEmployee EMP in oEmpCollection)
                    {

                        String retValue;
                        retValue = Convert.ToString(CreateOHEM(EMP.EmpID, EMP.FirstName, EMP.MiddleName, EMP.LastName));
                        EMP.SBOEmpCode = retValue;
                        if (!String.IsNullOrEmpty(retValue))
                        {
                            EMP.IntSboTransfered = true;
                        }
                        else
                        {
                            EMP.IntSboTransfered = false;
                        }
                    }

                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText( CountDB.ToString() + " Employees Sync with SBO", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None);
                }
                else
                {
                    oApplication.StatusBar.SetText("All Synced", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None);
                }
                                                          
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SBO Sync Function Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
