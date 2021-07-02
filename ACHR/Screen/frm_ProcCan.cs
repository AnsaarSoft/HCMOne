using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_ProcCan : HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.EditText txtFirstName, txtMiddleName, txtLastName, txtCandidateNo, txtValidFrom, txtValidTill;
        SAPbouiCOM.EditText txtOfficePhone, txtHomePhone, txtMobileNo, txtExtention, txtFax, txtPager, txtEmail;
        SAPbouiCOM.EditText txtwStreet, txtwStreetNo, txtwBlock, txtwBuilding, txtwZipCode, txtwCity;
        SAPbouiCOM.EditText txthStreet, txthStreetNo, txthBlock, txthBuilding, txthZipCode, txthCity;
        SAPbouiCOM.EditText txtDateOfBirth, txtNoOfChildrens, txtIdNo, txtPassportNo, txtPassportExpirationDate;
        SAPbouiCOM.EditText txtApplicationDate, txtAssignTo, txtLineManager, txtRemarks, txtJobRequisition;
        SAPbouiCOM.EditText txtCurrentSalary, txtExpectedSalary, txtRecommendedSalary, txtBank, txtAccountNo, txtBankBranch;
        SAPbouiCOM.EditText txtEmployeeID, txtUserCode, txtStaffingStatus;

        SAPbouiCOM.ComboBox cbDesignation, cbDepartment, cbBranch, cbLocation, cbAssignTo;
        //SAPbouiCOM.ComboBox cbStaffingStatus;
        SAPbouiCOM.ComboBox cbwState, cbwCountry, cbhState, cbhCountry;
        SAPbouiCOM.ComboBox cbGender, cbCountryOfBirth, cbMaritalStatus, cbCitizenShip;
        
        SAPbouiCOM.Button btnMain;

        SAPbouiCOM.Item itxtFirstName, itxtMiddleName, itxtLastName, itxtCandidateNo, itxtValidFrom, itxtValidTill;
        SAPbouiCOM.Item itxtOfficePhone, itxtHomePhone, itxtMobileNo, itxtExtention, itxtFax, itxtPager, itxtEmail;
        SAPbouiCOM.Item itxtwStreet, itxtwStreetNo, itxtwBlock, itxtwBuilding, itxtwZipCode, itxtwCity;
        SAPbouiCOM.Item itxthStreet, itxthStreetNo, itxthBlock, itxthBuilding, itxthZipCode, itxthCity;
        SAPbouiCOM.Item itxtDateOfBirth, itxtNoOfChildrens, itxtIdNo, itxtPassportNo, itxtPassportExpirationDate;
        SAPbouiCOM.Item itxtApplicationDate, itxtAssignTo, itxtLineManager, itxtRemarks, itxtJobRequisition;
        SAPbouiCOM.Item itxtCurrentSalary, itxtExpectedSalary, itxtRecommendedSalary, itxtBank, itxtAccountNo, itxtBankBranch;
        SAPbouiCOM.Item itxtEmployeeID, itxtUserCode;

        SAPbouiCOM.Item icbDesignation, icbDepartment, icbBranch, icbLocation, icbAssignTo;
        SAPbouiCOM.Item itxtStaffingStatus;
        SAPbouiCOM.Item icbwState, icbwCountry, icbhState, icbhCountry;
        SAPbouiCOM.Item icbGender, icbCountryOfBirth, icbMaritalStatus, icbCitizenShip;

        IEnumerable<MstCandidate> oCollection = null;

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
            }
            
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "txvacreq")
            {
                FillWithRequisitionSelection();
            }
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE && btnMain.Caption == "Ok")
            {
                btnMain.Caption = "Update";
            }
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            oForm.Freeze(true);
            InitiallizeDocument("New");
            btnMain.Caption = "Add";
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            oForm.Freeze(false);
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            
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
            oForm.Freeze(true);
            btnMain.Caption = "Find";
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE;
            InitiallizeDocument("Search");
            oForm.Freeze(false);
        }

        public override void fillFields()
        {
            base.fillFields();
            oForm.Freeze(true);
            FillFields();

            oForm.Freeze(false);
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            try
            {
                SearchKeyVal.Clear();
                if (txtFirstName.Value.Trim() != "" && txtFirstName.Value.Trim() != "*")
                {
                    SearchKeyVal.Add("FirstName", txtFirstName.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("FirstName", "%");
                }
                if (txtMiddleName.Value.Trim() != "" && txtMiddleName.Value.Trim() != "*")
                {
                    SearchKeyVal.Add("MiddleName", txtMiddleName.Value);
                }
                else
                {
                    SearchKeyVal.Add("MiddleName", "%");
                }
                if (txtLastName.Value.Trim() != "" && txtLastName.Value.Trim() != "*")
                {
                    SearchKeyVal.Add("LastName", txtLastName.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("LastName", "%");
                }
                if (txtCandidateNo.Value.Trim() != "" && txtCandidateNo.Value.Trim() != "*")
                {
                    SearchKeyVal.Add("CandidateNo", txtCandidateNo.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("CandidateNo", "%");
                }
                if (txtValidFrom.Value.Trim() != "")
                {
                    SearchKeyVal.Add("ValidFrom", txtValidFrom.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("ValidFrom", "19720101");
                }
                if (txtValidTill.Value.Trim() != "")
                {
                    SearchKeyVal.Add("ValidTill", txtValidTill.Value);
                }
                else
                {
                    SearchKeyVal.Add("ValidTill", "20250101");
                }

                if (!String.IsNullOrEmpty(txtJobRequisition.Value.Trim()))
                {
                    TrnsJobRequisition odoc = (from a in dbHrPayroll.TrnsJobRequisition where a.DocNum.ToString() == txtJobRequisition.Value.Trim() select a).FirstOrDefault();
                    if (odoc != null)
                    {
                        SearchKeyVal.Add("JobVacancy", odoc.ID.ToString());
                    }

                }
                else
                {
                    SearchKeyVal.Add("JobVacancy", "%");
                }
                if (cbDesignation.Selected.Value != "-1")
                {
                    SearchKeyVal.Add("Designation", cbDesignation.Selected.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("Position", "%");
                }
                if (cbDepartment.Selected.Value != "-1")
                {
                    SearchKeyVal.Add("Department", cbDepartment.Selected.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("Department", "%");
                }
                if (cbBranch.Selected.Value != "-1")
                {
                    SearchKeyVal.Add("Branch", cbBranch.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("Branch", "%");
                }
                if (cbLocation.Selected.Value != "-1")
                {
                    SearchKeyVal.Add("Location", cbLocation.Selected.Value.Trim());
                }
                else
                {
                    SearchKeyVal.Add("Location", "%");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                btnMain = oForm.Items.Item("btmain").Specific;
               
                //Header Area
                txtFirstName = oForm.Items.Item("txfname").Specific;
                itxtFirstName = oForm.Items.Item("txfname");
                oForm.DataSources.UserDataSources.Add("txfname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtFirstName.DataBind.SetBound(true, "", "txfname");

                txtMiddleName = oForm.Items.Item("txmname").Specific;
                itxtMiddleName = oForm.Items.Item("txmname");
                oForm.DataSources.UserDataSources.Add("txmname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtMiddleName.DataBind.SetBound(true, "", "txmname");

                txtLastName = oForm.Items.Item("txlname").Specific;
                itxtLastName = oForm.Items.Item("txlname");
                oForm.DataSources.UserDataSources.Add("txlname", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtLastName.DataBind.SetBound(true, "", "txlname");

                txtEmployeeID = oForm.Items.Item("txempid").Specific;
                itxtEmployeeID = oForm.Items.Item("txempid");
                oForm.DataSources.UserDataSources.Add("txempid", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtEmployeeID.DataBind.SetBound(true, "", "txempid");

                txtUserCode = oForm.Items.Item("txusercode").Specific;
                itxtUserCode = oForm.Items.Item("txusercode");
                oForm.DataSources.UserDataSources.Add("txusercode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtUserCode.DataBind.SetBound(true, "", "txusercode");

                txtCandidateNo = oForm.Items.Item("txcanno").Specific;
                itxtCandidateNo = oForm.Items.Item("txcanno");
                oForm.DataSources.UserDataSources.Add("txcanno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtCandidateNo.DataBind.SetBound(true, "", "txcanno");
                txtCandidateNo.Value = Convert.ToString(ds.GetNextCadidate());

                txtValidFrom = oForm.Items.Item("txvalidf").Specific;
                itxtValidFrom = oForm.Items.Item("txvalidf");
                oForm.DataSources.UserDataSources.Add("txvalidf", SAPbouiCOM.BoDataType.dt_DATE);
                txtValidFrom.DataBind.SetBound(true, "", "txvalidf");
                //txtValidFrom.Value = DateTime.Now.ToString("yyyyMMdd");

                txtValidTill = oForm.Items.Item("txvalidt").Specific;
                itxtValidTill = oForm.Items.Item("txvalidt");
                oForm.DataSources.UserDataSources.Add("txvalidt", SAPbouiCOM.BoDataType.dt_DATE);
                txtValidTill.DataBind.SetBound(true, "", "txvalidt");
                //txtValidTill.Value = DateTime.Now.ToString("yyyyMMdd");

                txtOfficePhone = oForm.Items.Item("txofficph").Specific;
                itxtOfficePhone = oForm.Items.Item("txofficph");
                oForm.DataSources.UserDataSources.Add("txofficph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,20);
                txtOfficePhone.DataBind.SetBound(true, "", "txofficph");

                txtHomePhone = oForm.Items.Item("txph").Specific;
                itxtHomePhone = oForm.Items.Item("txph");
                oForm.DataSources.UserDataSources.Add("txph", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,20);
                txtHomePhone.DataBind.SetBound(true, "", "txph");

                txtMobileNo = oForm.Items.Item("txmobile").Specific;
                itxtMobileNo = oForm.Items.Item("txmobile");
                oForm.DataSources.UserDataSources.Add("txmobile", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,20);
                txtMobileNo.DataBind.SetBound(true, "", "txmobile");

                txtExtention = oForm.Items.Item("txext").Specific;
                itxtExtention = oForm.Items.Item("txext");
                oForm.DataSources.UserDataSources.Add("txext", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,10);
                txtExtention.DataBind.SetBound(true, "", "txext");

                txtFax = oForm.Items.Item("txfax").Specific;
                itxtFax = oForm.Items.Item("txfax");
                oForm.DataSources.UserDataSources.Add("txfax", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,20);
                txtFax.DataBind.SetBound(true, "", "txfax");

                txtPager = oForm.Items.Item("txpager").Specific;
                itxtPager = oForm.Items.Item("txpager");
                oForm.DataSources.UserDataSources.Add("txpager", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,20);
                txtPager.DataBind.SetBound(true, "", "txpager");

                txtEmail = oForm.Items.Item("txemail").Specific;
                itxtEmail = oForm.Items.Item("txemail");
                oForm.DataSources.UserDataSources.Add("txemail", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,30);
                txtEmail.DataBind.SetBound(true, "", "txemail");

                cbDesignation = oForm.Items.Item("cbdesig").Specific;
                icbDesignation = oForm.Items.Item("cbdesig");
                oForm.DataSources.UserDataSources.Add("cbdesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDesignation.DataBind.SetBound(true, "", "cbdesig");

                txtJobRequisition = oForm.Items.Item("txvacreq").Specific;
                itxtJobRequisition = oForm.Items.Item("txvacreq");
                oForm.DataSources.UserDataSources.Add("txvacreq", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtJobRequisition.DataBind.SetBound(true, "", "txvacreq");

                cbDepartment = oForm.Items.Item("cbdept").Specific;
                icbDepartment = oForm.Items.Item("cbdept");
                oForm.DataSources.UserDataSources.Add("cbdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDepartment.DataBind.SetBound(true, "", "cbdept");

                cbBranch = oForm.Items.Item("cbbranch").Specific;
                icbBranch = oForm.Items.Item("cbbranch");
                oForm.DataSources.UserDataSources.Add("cbbranch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbBranch.DataBind.SetBound(true, "", "cbbranch");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                icbLocation = oForm.Items.Item("cbloc");
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbLocation.DataBind.SetBound(true, "", "cbloc");

                //Address Tab Working Address

                txtwStreet = oForm.Items.Item("txstreet").Specific;
                itxtwStreet = oForm.Items.Item("txstreet");
                oForm.DataSources.UserDataSources.Add("txstreet", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtwStreet.DataBind.SetBound(true, "", "txstreet");

                txtwStreetNo = oForm.Items.Item("txstrno").Specific;
                itxtwStreetNo = oForm.Items.Item("txstrno");
                oForm.DataSources.UserDataSources.Add("txstrno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtwStreetNo.DataBind.SetBound(true, "", "txstrno");

                txtwBlock = oForm.Items.Item("txblck").Specific;
                itxtwBlock = oForm.Items.Item("txblck");
                oForm.DataSources.UserDataSources.Add("txblck", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtwBlock.DataBind.SetBound(true, "", "txblck");

                txtwBuilding = oForm.Items.Item("txbuild").Specific;
                itxtwBuilding = oForm.Items.Item("txbuild");
                oForm.DataSources.UserDataSources.Add("txbuild", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtwBuilding.DataBind.SetBound(true, "", "txbuild");

                txtwZipCode = oForm.Items.Item("txzip").Specific;
                itxtwZipCode = oForm.Items.Item("txzip");
                oForm.DataSources.UserDataSources.Add("txzip", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtwZipCode.DataBind.SetBound(true, "", "txzip");

                txtwCity = oForm.Items.Item("txcity").Specific;
                itxtwCity = oForm.Items.Item("txcity");
                oForm.DataSources.UserDataSources.Add("txcity", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtwCity.DataBind.SetBound(true, "", "txcity");

                cbwState = oForm.Items.Item("cbstate").Specific;
                icbwState = oForm.Items.Item("cbstate");
                oForm.DataSources.UserDataSources.Add("cbstate", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbwState.DataBind.SetBound(true, "", "cbstate");

                cbwCountry = oForm.Items.Item("cbcntry").Specific;
                icbwCountry = oForm.Items.Item("cbcntry");
                oForm.DataSources.UserDataSources.Add("cbcntry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbwCountry.DataBind.SetBound(true, "", "cbcntry");

                // Address Tab Home Address

                txthStreet = oForm.Items.Item("txstreet1").Specific;
                itxthStreet = oForm.Items.Item("txstreet1");
                oForm.DataSources.UserDataSources.Add("txstreet1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txthStreet.DataBind.SetBound(true, "", "txstreet1");

                txthStreetNo = oForm.Items.Item("txstrno1").Specific;
                itxthStreetNo = oForm.Items.Item("txstrno1");
                oForm.DataSources.UserDataSources.Add("txstrno1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txthStreetNo.DataBind.SetBound(true, "", "txstrno1");

                txthBlock = oForm.Items.Item("txblck1").Specific;
                itxthBlock = oForm.Items.Item("txblck1");
                oForm.DataSources.UserDataSources.Add("txblck1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txthBlock.DataBind.SetBound(true, "", "txblck1");

                txthBuilding = oForm.Items.Item("txbuild1").Specific;
                itxthBuilding = oForm.Items.Item("txbuild1");
                oForm.DataSources.UserDataSources.Add("txbuild1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txthBuilding.DataBind.SetBound(true, "", "txbuild1");

                txthZipCode = oForm.Items.Item("txzip1").Specific;
                itxthZipCode = oForm.Items.Item("txzip1");
                oForm.DataSources.UserDataSources.Add("txzip1", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 10);
                txthZipCode.DataBind.SetBound(true, "", "txzip1");

                txthCity = oForm.Items.Item("txcity1").Specific;
                itxthCity = oForm.Items.Item("txcity1");
                oForm.DataSources.UserDataSources.Add("txcity1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txthCity.DataBind.SetBound(true, "", "txcity1");

                cbhState = oForm.Items.Item("cbstate1").Specific;
                icbhState = oForm.Items.Item("cbstate1");
                oForm.DataSources.UserDataSources.Add("cbstate1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbhState.DataBind.SetBound(true, "", "cbstate1");

                cbhCountry = oForm.Items.Item("cbcntry1").Specific;
                icbhCountry = oForm.Items.Item("cbcntry1");
                oForm.DataSources.UserDataSources.Add("cbcntry1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbhCountry.DataBind.SetBound(true, "", "cbcntry1");

                //Administration Tab

                txtApplicationDate = oForm.Items.Item("txappdt").Specific;
                itxtApplicationDate = oForm.Items.Item("txappdt");
                oForm.DataSources.UserDataSources.Add("txappdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtApplicationDate.DataBind.SetBound(true, "", "txappdt");

                txtAssignTo = oForm.Items.Item("txasgto").Specific;
                itxtAssignTo = oForm.Items.Item("txasgto");
                oForm.DataSources.UserDataSources.Add("txasgto", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtAssignTo.DataBind.SetBound(true, "", "txasgto");

                txtLineManager = oForm.Items.Item("txlinemng").Specific;
                itxtLineManager = oForm.Items.Item("txlinemng");
                oForm.DataSources.UserDataSources.Add("txlinemng", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtLineManager.DataBind.SetBound(true, "", "txlinemng");

                txtStaffingStatus = oForm.Items.Item("txstatus").Specific;
                itxtStaffingStatus = oForm.Items.Item("txstatus");
                oForm.DataSources.UserDataSources.Add("txstatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtStaffingStatus.DataBind.SetBound(true, "", "txstatus");
                
                oForm.PaneLevel = 1;

                //Personal Tab

                txtDateOfBirth = oForm.Items.Item("txdob").Specific;
                itxtDateOfBirth = oForm.Items.Item("txdob");
                oForm.DataSources.UserDataSources.Add("txdob", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtDateOfBirth.DataBind.SetBound(true, "", "txdob");

                txtNoOfChildrens = oForm.Items.Item("txchild").Specific;
                itxtNoOfChildrens = oForm.Items.Item("txchild");
                oForm.DataSources.UserDataSources.Add("txchild", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtNoOfChildrens.DataBind.SetBound(true, "", "txchild");

                txtIdNo = oForm.Items.Item("txidno").Specific;
                itxtIdNo = oForm.Items.Item("txidno");
                oForm.DataSources.UserDataSources.Add("txidno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtIdNo.DataBind.SetBound(true, "", "txidno");

                txtPassportNo = oForm.Items.Item("txpsno").Specific;
                itxtPassportNo = oForm.Items.Item("txpsno");
                oForm.DataSources.UserDataSources.Add("txpsno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtPassportNo.DataBind.SetBound(true, "", "txpsno");

                txtPassportExpirationDate = oForm.Items.Item("txpsexpdt").Specific;
                itxtPassportExpirationDate = oForm.Items.Item("txpsexpdt");
                oForm.DataSources.UserDataSources.Add("txpsexpdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtPassportExpirationDate.DataBind.SetBound(true, "", "txpsexpdt");

                cbGender = oForm.Items.Item("cbgender").Specific;
                icbGender = oForm.Items.Item("cbgender");
                oForm.DataSources.UserDataSources.Add("cbgender", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbGender.DataBind.SetBound(true, "", "cbgender");

                cbCountryOfBirth = oForm.Items.Item("cbcob").Specific;
                icbCountryOfBirth = oForm.Items.Item("cbcob");
                oForm.DataSources.UserDataSources.Add("cbcob", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbCountryOfBirth.DataBind.SetBound(true, "", "cbcob");

                cbMaritalStatus = oForm.Items.Item("cbmarital").Specific;
                icbMaritalStatus = oForm.Items.Item("cbmarital");
                oForm.DataSources.UserDataSources.Add("cbmarital", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbMaritalStatus.DataBind.SetBound(true, "", "cbmarital");

                cbCitizenShip = oForm.Items.Item("cbcitizen").Specific;
                icbCitizenShip = oForm.Items.Item("cbcitizen");
                oForm.DataSources.UserDataSources.Add("cbcitizen", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbCitizenShip.DataBind.SetBound(true, "", "cbcitizen");

                // Finance Tab

                txtCurrentSalary = oForm.Items.Item("txcslry").Specific;
                itxtCurrentSalary = oForm.Items.Item("txcslry");
                oForm.DataSources.UserDataSources.Add("txcslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtCurrentSalary.DataBind.SetBound(true, "", "txcslry");

                txtExpectedSalary = oForm.Items.Item("txeslry").Specific;
                itxtExpectedSalary = oForm.Items.Item("txeslry");
                oForm.DataSources.UserDataSources.Add("txeslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtExpectedSalary.DataBind.SetBound(true, "", "txeslry");

                txtRecommendedSalary = oForm.Items.Item("txrslry").Specific;
                itxtRecommendedSalary = oForm.Items.Item("txrslry");
                oForm.DataSources.UserDataSources.Add("txrslry", SAPbouiCOM.BoDataType.dt_SUM);
                txtRecommendedSalary.DataBind.SetBound(true, "", "txrslry");

                txtBank = oForm.Items.Item("txbank").Specific;
                itxtBank = oForm.Items.Item("txbank");
                oForm.DataSources.UserDataSources.Add("txbank", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtBank.DataBind.SetBound(true, "", "txbank");

                txtAccountNo = oForm.Items.Item("txacctno").Specific;
                itxtAccountNo = oForm.Items.Item("txacctno");
                oForm.DataSources.UserDataSources.Add("txacctno", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtAccountNo.DataBind.SetBound(true, "", "txacctno");

                txtBankBranch = oForm.Items.Item("txbnkbrnch").Specific;
                itxtBankBranch = oForm.Items.Item("txbnkbrnch");
                oForm.DataSources.UserDataSources.Add("txbnkbrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtBankBranch.DataBind.SetBound(true, "", "txbnkbrnch");

                //Remarks Tab

                txtRemarks = oForm.Items.Item("txremarks").Specific;
                itxtRemarks = oForm.Items.Item("txremarks");
                oForm.DataSources.UserDataSources.Add("txremarks", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtBankBranch.DataBind.SetBound(true, "", "txremarks");

                //Combo Fill
                FillDesignationCombo(cbDesignation);
                FillDepartmentCombo(cbDepartment);
                FillBranchCombo(cbBranch);
                FillLocationsCombo(cbLocation);

                //The FMS FOr selection
                Program.objHrmsUI.addFms("frm_ProcCan", "txvacreq", "-1", "SELECT DocNum, Department, Designation, Location FROM " + Program.objHrmsUI.HRMSDbName + ".dbo.TrnsJobRequisition WHERE DocAprStatus = ''LV0006''");
                Program.objHrmsUI.addFms("frm_ProcCan", "txlinemng", "-1", "SELECT EmpID AS [Employee Code], FirstName, MiddleName , LastName, DepartmentName, DesignationName, LocationName FROM " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee WHERE flgActive = 1");
                Program.objHrmsUI.addFms("frm_ProcCan", "txasgto", "-1", "SELECT EmpID AS [Employee Code], FirstName, MiddleName , LastName, DepartmentName, DesignationName, LocationName FROM " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee WHERE flgActive = 1");
                
                
                FillStatesCombo(cbwState);
                FillStatesCombo(cbhState);
                FillCountryCombo(cbwCountry);
                FillCountryCombo(cbhCountry);
                FillCountryCombo(cbCountryOfBirth);
                FillCountryCombo(cbCitizenShip);
                FillMartialCombo(cbMaritalStatus);
                FillGenderCombo(cbGender);
                //FillStaffingCombo(cbStaffingStatus);
                GetData();
                InitiallizeDocument("New");
                FormStatus();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception Form Initiallization Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument(String pCase)
        {
            oForm.Freeze(true);
            try
            {
                if (pCase == "New")
                {

                    txtFirstName.Value = "";
                    txtMiddleName.Value = "";
                    txtLastName.Value = "";
                    txtEmployeeID.Value = "";
                    txtUserCode.Value = "";
                    txtCandidateNo.Value = Convert.ToString(ds.GetNextCadidate());
                    txtJobRequisition.Value = "";
                    cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtValidFrom.Value = "";
                    txtValidTill.Value = "";
                    txtOfficePhone.Value = "";
                    txtExtention.Value = "";
                    txtMobileNo.Value = "";
                    txtPager.Value = "";
                    txtHomePhone.Value = "";
                    txtFax.Value = "";
                    txtEmail.Value = "";

                    // Address Work Area

                    txtwStreet.Value = "";
                    txtwStreetNo.Value = "";
                    txtwBlock.Value = "";
                    txtwBuilding.Value = "";
                    txtwZipCode.Value = "";
                    txtwCity.Value = "";
                    cbwState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbwCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    // Address Home Area

                    txthStreet.Value = "";
                    txthStreetNo.Value = "";
                    txthBlock.Value = "";
                    txthBuilding.Value = "";
                    txthZipCode.Value = "";
                    txthCity.Value = "";
                    cbhState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbhCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Administration

                    txtApplicationDate.Value = "";
                    txtLineManager.Value = "";
                    txtAssignTo.Value = "";
                    txtStaffingStatus.Value = "";
                    //cbStaffingStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Personal

                    cbGender.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtDateOfBirth.Value = "";
                    cbCountryOfBirth.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbMaritalStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtNoOfChildrens.Value = "";
                    txtIdNo.Value = "";
                    txtPassportNo.Value = "";
                    txtPassportExpirationDate.Value = "";
                    cbCitizenShip.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Finance

                    txtCurrentSalary.Value = "";
                    txtExpectedSalary.Value = "";
                    txtRecommendedSalary.Value = "";
                    txtBank.Value = "";
                    txtBankBranch.Value = "";
                    txtAccountNo.Value = "";
                    txtRemarks.Value = "";

                    btnMain.Caption = "Add";
                }

                if (pCase == "Search")
                {
                    txtFirstName.Value = "";
                    txtMiddleName.Value = "";
                    txtLastName.Value = "";
                    txtCandidateNo.Value = "";
                    txtEmployeeID.Value = "";
                    txtUserCode.Value = "";
                    //cbJobRequisition.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtJobRequisition.Value = "";
                    cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtValidFrom.Value = "";
                    txtValidTill.Value = "";
                    txtOfficePhone.Value = "";
                    txtExtention.Value = "";
                    txtMobileNo.Value = "";
                    txtPager.Value = "";
                    txtHomePhone.Value = "";
                    txtFax.Value = "";
                    txtEmail.Value = "";

                    // Address Work Area

                    txtwStreet.Value = "";
                    txtwStreetNo.Value = "";
                    txtwBlock.Value = "";
                    txtwBuilding.Value = "";
                    txtwZipCode.Value = "";
                    txtwCity.Value = "";
                    cbwState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbwCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    // Address Home Area

                    txthStreet.Value = "";
                    txthStreetNo.Value = "";
                    txthBlock.Value = "";
                    txthBuilding.Value = "";
                    txthZipCode.Value = "";
                    txthCity.Value = "";
                    cbhState.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbhCountry.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Administration

                    txtApplicationDate.Value = "";
                    //cbAssignTo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    //cbStaffingStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    //cbLineManager.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtStaffingStatus.Value = "";
                    txtAssignTo.Value = "";
                    txtLineManager.Value = "";

                    //Personal

                    cbGender.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtDateOfBirth.Value = "";
                    cbCountryOfBirth.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbMaritalStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtNoOfChildrens.Value = "";
                    txtIdNo.Value = "";
                    txtPassportNo.Value = "";
                    txtPassportExpirationDate.Value = "";
                    cbCitizenShip.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Finance

                    txtCurrentSalary.Value = "";
                    txtExpectedSalary.Value = "";
                    txtRecommendedSalary.Value = "";
                    txtBank.Value = "";
                    txtBankBranch.Value = "";
                    txtAccountNo.Value = "";
                    txtRemarks.Value = "";

                    btnMain.Caption = "Find";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}",ex.Message);
            }
            oForm.Freeze(false);
        }

        private void CheckMainButtonState()
        {
            switch (btnMain.Caption)
            {
                case "Add":
                    if (ValidateRecord())
                    {
                        if (AddCandidate())
                        {
                            InitiallizeDocument("New");
                        }
                        else
                        {
                        }
                    }
                    break;
                case "Update":
                    if (ValidateRecord())
                    {
                        if (UpdateCandidate())
                        {
                            InitiallizeDocument("New");
                        }
                        else
                        {
                        }
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

        private bool ValidateRecord()
        {
            bool retValue = true;
            String FirstName, LastName, tAssignTo, tLineManager, EmpCodeValue, UserCodeValue;
            Int32 Designation, Branch, Department, Location;
            String ValidFrom, ValidTo;

            FirstName = txtFirstName.Value.Trim();
            LastName = txtLastName.Value.Trim();
            Designation = Convert.ToInt32(cbDesignation.Value.Trim());
            Branch = Convert.ToInt32(cbBranch.Value.Trim());
            Department = Convert.ToInt32(cbDepartment.Value.Trim());
            Location = Convert.ToInt32(cbLocation.Value.Trim());
            ValidFrom = txtValidFrom.Value.Trim();
            ValidTo = txtValidTill.Value.Trim();
            tAssignTo = txtAssignTo.Value.Trim();
            tLineManager = txtLineManager.Value.Trim();
            EmpCodeValue = txtEmployeeID.Value.Trim();
            UserCodeValue = txtUserCode.Value.Trim();

            if (String.IsNullOrEmpty(FirstName))
            {
                retValue = false;
                oApplication.StatusBar.SetText("First Name is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (String.IsNullOrEmpty(LastName))
            {
                retValue = false;
                oApplication.StatusBar.SetText("Last Name is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (String.IsNullOrEmpty(ValidFrom))
            {
                retValue = false;
                oApplication.StatusBar.SetText("ValidFrom Date is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (String.IsNullOrEmpty(ValidTo))
            {
                retValue = false;
                oApplication.StatusBar.SetText("ValidTo Date is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (Designation == -1)
            {
                retValue = false;
                oApplication.StatusBar.SetText("Designation is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (Department == -1)
            {
                retValue = false;
                oApplication.StatusBar.SetText("Department is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }

            if (Branch == -1)
            {
                retValue = false;
                oApplication.StatusBar.SetText("Branch is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(tAssignTo))
            {
                retValue = false;
                oApplication.StatusBar.SetText("Assign To is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            if (String.IsNullOrEmpty(tLineManager))
            {
                retValue = false;
                oApplication.StatusBar.SetText("Line Manager is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            }
            //if (String.IsNullOrEmpty(EmpCodeValue))
            //{
            //    retValue = false;
            //    oApplication.StatusBar.SetText("Employee Code is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            //}
            //if (String.IsNullOrEmpty(UserCodeValue))
            //{
            //    retValue = false;
            //    oApplication.StatusBar.SetText("User Code is Mandatory", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            //}
            return retValue;
        }

        private Boolean AddCandidate()
        {
            Boolean retValue = true;
            try
            {
                MstCandidate oNew = new MstCandidate();
                oNew.FirstName = txtFirstName.Value.Trim();
                oNew.MiddleName = txtMiddleName.Value.Trim();
                oNew.LastName = txtLastName.Value.Trim();
                oNew.StaffingStatus = "OPEN"; //Set as open
                //oNew.EmpCode = txtEmployeeID.Value.Trim();
                oNew.UserCode = txtUserCode.Value.Trim();
                if (!String.IsNullOrEmpty(txtJobRequisition.Value))
                {
                    TrnsJobRequisition oJR = (from a in dbHrPayroll.TrnsJobRequisition where a.DocNum.ToString() == txtJobRequisition.Value.Trim() select a).FirstOrDefault();
                    if (oJR != null)
                    {
                        //oNew.JobRequisitionNo = oJR.ID;
                    }
                }
                else
                {
                    //oNew.JobRequisitionNo = null;
                }
                if (cbDepartment.Value.Trim() != "-1")
                {
                    oNew.Department = Convert.ToInt32(cbDepartment.Value.Trim());
                }
                else
                {
                    oNew.Department = null;
                }
                if (cbBranch.Value.Trim() != "-1")
                {
                    oNew.Branch = Convert.ToInt32(cbBranch.Value.Trim());
                }
                else
                {
                    oNew.Branch = null;
                }
                if (cbDesignation.Value.Trim() != "-1")
                {
                    oNew.Designation = Convert.ToInt32(cbDesignation.Value.Trim());
                }
                else
                {
                    oNew.Designation = null;
                }
                //oNew.Position = null;
                if (cbLocation.Value.Trim() != "-1")
                {
                    oNew.Location = Convert.ToInt32(cbLocation.Value.Trim());
                }
                else
                {
                    oNew.Location = null;
                }
                if (txtCandidateNo.Value.Trim() != "")
                {
                    oNew.CandidateNo = Convert.ToInt32(txtCandidateNo.Value.Trim());
                }
                if (txtValidFrom.Value.Trim() != "")
                {
                    //oNew.ValidFrom = DateTime.ParseExact(txtValidFrom.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                else
                {
                    //oNew.ValidFrom = null;
                }
                if (txtValidTill.Value.Trim() != "")
                {
                    //oNew.ValidTo = DateTime.ParseExact(txtValidTill.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                else
                {
                    //oNew.ValidTo = null;
                }
                oNew.OfficePhone = txtOfficePhone.Value.Trim();
                oNew.Extension = txtExtention.Value.Trim();
                oNew.MobilePhone = txtMobileNo.Value.Trim();
                oNew.Pager = txtPager.Value.Trim();
                oNew.HomePhone = txtHomePhone.Value.Trim();
                oNew.Fax = txtFax.Value.Trim();
                //oNew.Email = txtEmail.Value.Trim();

                //Address Tab

                oNew.WStreet = txtwStreet.Value.Trim();
                oNew.WStreetNo = txtwStreetNo.Value.Trim();
                oNew.WBuildingFloor = txtwBuilding.Value.Trim();
                oNew.WBlock = txtwBlock.Value.Trim();
                oNew.WCity = txtwCity.Value.Trim();
                oNew.WZipCode = txtwZipCode.Value.Trim();
                oNew.WState = cbwState.Value.Trim() != "-1" ? cbwState.Value.Trim() : null;
                oNew.WCountry = cbwCountry.Value.Trim() != "-1" ? cbwCountry.Value.Trim() : null;

                oNew.HStreet = txthStreet.Value.Trim();
                oNew.HStreetNo = txthStreetNo.Value.Trim();
                oNew.HBuildingFloor = txthBuilding.Value.Trim();
                oNew.HBlock = txthBlock.Value.Trim();
                oNew.HCity = txthCity.Value.Trim();
                oNew.HZipCode = txthZipCode.Value.Trim();
                oNew.HState = cbhState.Value.Trim() != "-1" ? cbhState.Value.Trim() : null;
                oNew.HCountry = cbhCountry.Value.Trim() != "-1" ? cbhCountry.Value.Trim() : null;

                //Administration Tab

                if (txtApplicationDate.Value.Trim() != "")
                {
                    oNew.ApplicationDate = DateTime.ParseExact(txtApplicationDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                else
                {
                    oNew.ApplicationDate = null;
                }
                if (!String.IsNullOrEmpty(txtAssignTo.Value))
                {
                    String uAssignedTo = txtAssignTo.Value.Trim();
                    oNew.AssignedTo = (from a in dbHrPayroll.MstEmployee where a.EmpID.Contains(uAssignedTo) select a.ID).FirstOrDefault();
                }
                else
                {
                    oNew.AssignedTo = null;
                }

                if (!String.IsNullOrEmpty(txtLineManager.Value))
                {
                    String uLineManager = txtLineManager.Value.Trim();
                    oNew.LineManager = (from a in dbHrPayroll.MstEmployee where a.EmpID.Contains(uLineManager) select a.ID).FirstOrDefault();
                }
                else
                {
                    oNew.LineManager = null;
                }
                //Personal Tab
                oNew.Gender = cbGender.Value != "-1" ? cbGender.Value.Trim() : null;
                if (txtDateOfBirth.Value.Trim() != "")
                    //{
                    //    oNew.DOB = DateTime.ParseExact(txtDateOfBirth.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    //}
                    //else
                    //{
                    //    oNew.DOB = null;
                    //}
                    //oNew.BornInCountry = cbCountryOfBirth.Value != "-1" ? cbCountryOfBirth.Value.Trim():null;
                    oNew.MartialStatus = cbMaritalStatus.Value != "-1" ? cbMaritalStatus.Value.Trim() : null;
                //oNew.NoOfChildren = txtNoOfChildrens.Value.Trim();
                //oNew.IDNo = txtIdNo.Value.Trim();
                //oNew.PassportNumber = txtPassportNo.Value.Trim();
                if (txtPassportExpirationDate.Value.Trim() != "")
                {
                    //oNew.PassportExpiration = DateTime.ParseExact(txtPassportExpirationDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                else
                {
                    //oNew.PassportExpiration = null;
                }
                //if (cbCitizenShip.Value.Trim() != "-1")
                //{
                //    //oNew.CitizenShip = Convert.ToString(cbCitizenShip.Value.Trim());
                //}
                //else
                //{
                //    //oNew.CitizenShip = null;
                //}

                //Finance Tab
                if (txtCurrentSalary.Value.Trim() != "")
                {
                    oNew.CurrentSalary = Convert.ToDecimal(txtCurrentSalary.Value.Trim());
                }
                else
                {
                    oNew.CurrentSalary = 0.0M;
                }
                if (txtExpectedSalary.Value.Trim() != "")
                {
                    oNew.ExpectedSalary = Convert.ToDecimal(txtExpectedSalary.Value.Trim());
                }
                else
                {
                    oNew.ExpectedSalary = null;
                }
                if (txtRecommendedSalary.Value.Trim() != "")
                {
                    oNew.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
                }
                else
                {
                    oNew.RecommendedSalary = null;
                }
                //oNew.Bank = txtBank.Value.Trim();
                //oNew.BankBranch = txtBankBranch.Value.Trim();
                //oNew.AccountNo = txtAccountNo.Value.Trim();
                oNew.Remarks = txtRemarks.Value.Trim();

                //Document Setup
                oNew.CreateDate = DateTime.Now;
                //oNew.UserID = oCompany.UserName;
                oNew.UpdateDate = DateTime.Now;
                oNew.UpdatedBy = oCompany.UserName;

                dbHrPayroll.MstCandidate.InsertOnSubmit(oNew);
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception AddCandidate Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private Boolean UpdateCandidate()
        {
            Boolean retValue = true;
            try
            {
                //MstCandidate oNew = null;

                //oNew = oCollection.ElementAt<MstCandidate>(currentRecord);
                
                //oNew.FirstName = txtFirstName.Value.Trim();
                //oNew.MiddleName = txtMiddleName.Value.Trim();
                //oNew.LastName = txtLastName.Value.Trim();
                //oNew.EmpCode = txtEmployeeID.Value.Trim();
                //oNew.UserCode = txtUserCode.Value.Trim();
                //if (!String.IsNullOrEmpty(txtJobRequisition.Value))
                //{
                //    TrnsJobRequisition oJR = (from a in dbHrPayroll.TrnsJobRequisition where a.DocNum.ToString() == txtJobRequisition.Value.Trim() select a).FirstOrDefault();
                //    if (oJR != null)
                //    {
                //        oNew.JobRequisitionNo = oJR.ID;
                //    }
                //}
                //else
                //{
                //    oNew.JobRequisitionNo = null;
                //}
                //if (cbDepartment.Value.Trim() != "-1")
                //{
                //    oNew.Department = Convert.ToInt32(cbDepartment.Value.Trim());
                //}
                //else
                //{
                //    oNew.Department = null;
                //}
                //if (cbBranch.Value.Trim() != "-1")
                //{
                //    oNew.Branch = Convert.ToInt32(cbBranch.Value.Trim());
                //}
                //else
                //{
                //    oNew.Branch = null;
                //}
                //if (cbDesignation.Value.Trim() != "-1")
                //{
                //    oNew.Designation = Convert.ToInt32(cbDesignation.Value.Trim());
                //}
                //else
                //{
                //    oNew.Designation = null;
                //}
                //oNew.Position = null;
                //if (cbLocation.Value.Trim() != "-1")
                //{
                //    oNew.Location = Convert.ToInt32(cbLocation.Value.Trim());
                //}
                //else
                //{
                //    oNew.Location = null;
                //}
                //if (txtCandidateNo.Value.Trim() != "")
                //{
                //    oNew.CandidateNo = Convert.ToInt32(txtCandidateNo.Value.Trim());
                //}
                //if (txtValidFrom.Value.Trim() != "")
                //{
                //    oNew.ValidFrom = DateTime.ParseExact(txtValidFrom.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //}
                //else
                //{
                //    oNew.ValidFrom = null;
                //}
                //if (txtValidTill.Value.Trim() != "")
                //{
                //    oNew.ValidTo = DateTime.ParseExact(txtValidTill.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //}
                //else
                //{
                //    oNew.ValidTo = null;
                //}
                //oNew.OfficePhone = txtOfficePhone.Value.Trim();
                //oNew.Extension = txtExtention.Value.Trim();
                //oNew.MobilePhone = txtMobileNo.Value.Trim();
                //oNew.Pager = txtPager.Value.Trim();
                //oNew.HomePhone = txtHomePhone.Value.Trim();
                //oNew.Fax = txtFax.Value.Trim();
                //oNew.Email = txtEmail.Value.Trim();

                ////Address Tab

                //oNew.WStreet = txtwStreet.Value.Trim();
                //oNew.WStreetNo = txtwStreetNo.Value.Trim();
                //oNew.WBuildingFloor = txtwBuilding.Value.Trim();
                //oNew.WBlock = txtwBlock.Value.Trim();
                //oNew.WCity = txtwCity.Value.Trim();
                //oNew.WZipCode = txtwZipCode.Value.Trim();
                //oNew.WState = cbwState.Value != "-1" ? cbwState.Value.Trim() : null;
                //oNew.WCountry = cbwCountry.Value != "-1" ? cbwCountry.Value.Trim() : null;

                //oNew.HStreet = txthStreet.Value.Trim();
                //oNew.HStreetNo = txthStreetNo.Value.Trim();
                //oNew.HBuildingFloor = txthBuilding.Value.Trim();
                //oNew.HBlock = txthBlock.Value.Trim();
                //oNew.HCity = txthCity.Value.Trim();
                //oNew.HZipCode = txthZipCode.Value.Trim();
                //oNew.HState = cbhState.Value != "-1" ? cbhState.Value.Trim() : null;
                //oNew.HCountry = cbhCountry.Value != "-1" ? cbhCountry.Value.Trim(): null;

                ////Administration Tab

                //if (txtApplicationDate.Value.Trim() != "")
                //{
                //    oNew.ApplicationDate = DateTime.ParseExact(txtApplicationDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //}
                //else
                //{
                //    oNew.ApplicationDate = null;
                //}
                //if (!String.IsNullOrEmpty(txtAssignTo.Value))
                //{
                //    String uAssignedTo = txtAssignTo.Value.Trim();
                //    oNew.AssignedTo = (from a in dbHrPayroll.MstEmployee where a.EmpID.Contains(uAssignedTo) select a.ID).FirstOrDefault();
                //}
                //else
                //{
                //    oNew.AssignedTo = null;
                //}
                
                //if (!String.IsNullOrEmpty(txtLineManager.Value))
                //{
                //    String uLineManager = txtLineManager.Value.Trim();
                //    oNew.LineManager = (from a in dbHrPayroll.MstEmployee where a.EmpID.Contains(uLineManager) select a.ID).FirstOrDefault();
                //}
                //else
                //{
                //    oNew.LineManager = null;
                //}
                ////Personal Tab
                //oNew.Gender = cbGender.Value != "-1" ? cbGender.Value.Trim() : null;
                ////if (txtDateOfBirth.Value.Trim() != "")
                ////{
                ////    oNew.DOB = DateTime.ParseExact(txtDateOfBirth.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                ////}
                ////else
                ////{
                ////    oNew.DOB = null;
                ////}
                ////oNew.BornInCountry = cbCountryOfBirth.Value != "-1" ? cbCountryOfBirth.Value.Trim() : null;
                //oNew.MartialStatus = cbMaritalStatus.Value != "-1" ? cbMaritalStatus.Value.Trim() : null;
                //oNew.NoOfChildren = txtNoOfChildrens.Value.Trim();
                //oNew.IDNo = txtIdNo.Value.Trim();
                //oNew.PassportNumber = txtPassportNo.Value.Trim();
                //if (txtPassportExpirationDate.Value.Trim() != "")
                //{
                //    oNew.PassportExpiration = DateTime.ParseExact(txtPassportExpirationDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //}
                //else
                //{
                //    oNew.PassportExpiration = null;
                //}
                ////if (cbCitizenShip.Value.Trim() != "-1")
                ////{
                ////    oNew.CitizenShip = Convert.ToString(cbCitizenShip.Value.Trim());
                ////}
                ////else
                ////{
                ////    oNew.CitizenShip = null;
                ////}

                ////Finance Tab
                //if (txtCurrentSalary.Value.Trim() != "")
                //{
                //    oNew.CurrentSalary = Convert.ToDecimal(txtCurrentSalary.Value.Trim());
                //}
                //else
                //{
                //    oNew.CurrentSalary = 0.0M;
                //}
                //if (txtExpectedSalary.Value.Trim() != "")
                //{
                //    oNew.ExpectedSalary = Convert.ToDecimal(txtExpectedSalary.Value.Trim());
                //}
                //else
                //{
                //    oNew.ExpectedSalary = null;
                //}
                //if (txtRecommendedSalary.Value.Trim() != "")
                //{
                //    oNew.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
                //}
                //else
                //{
                //    oNew.RecommendedSalary = null;
                //}
                ////oNew.Bank = txtBank.Value.Trim();
                ////oNew.BankBranch = txtBankBranch.Value.Trim();
                ////oNew.AccountNo = txtAccountNo.Value.Trim();
                //oNew.Remarks = txtRemarks.Value.Trim();
                ////Document Setup
                ////oNew.CreateDate = DateTime.Now;
                ////oNew.UserId = oCompany.UserName;
                //oNew.UpdateDate = DateTime.Now;
                //oNew.UpdatedBy = oCompany.UserName;

                ////return;
                //dbHrPayroll.SubmitChanges();
                //oApplication.StatusBar.SetText("Document Update Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception UpdateCandidate Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private void FormStatus()
        {
            itxtFirstName.AffectsFormMode = true;
            itxtMiddleName.AffectsFormMode = true;
            itxtLastName.AffectsFormMode = true;
            itxtEmployeeID.AffectsFormMode = true;
            itxtUserCode.AffectsFormMode = true;
            itxtCandidateNo.AffectsFormMode = true;
            itxtValidFrom.AffectsFormMode = true;
            itxtValidTill.AffectsFormMode = true;
            itxtOfficePhone.AffectsFormMode = true;
            itxtHomePhone.AffectsFormMode = true;
            itxtMobileNo.AffectsFormMode = true;
            itxtExtention.AffectsFormMode = true;
            itxtFax.AffectsFormMode = true;
            itxtPager.AffectsFormMode = true;
            itxtEmail.AffectsFormMode = true;
            itxtwStreet.AffectsFormMode = true;
            itxtwStreetNo.AffectsFormMode = true;
            itxtwBlock.AffectsFormMode = true;
            itxtwBuilding.AffectsFormMode = true;
            itxtwZipCode.AffectsFormMode = true;
            itxtwCity.AffectsFormMode = true;
            itxthStreet.AffectsFormMode = true;
            itxthStreetNo.AffectsFormMode = true;
            itxthBlock.AffectsFormMode = true;
            itxthBuilding.AffectsFormMode = true;
            itxthZipCode.AffectsFormMode = true;
            itxthCity.AffectsFormMode = true;
            itxtDateOfBirth.AffectsFormMode = true;
            itxtNoOfChildrens.AffectsFormMode = true;
            itxtIdNo.AffectsFormMode = true;
            itxtPassportNo.AffectsFormMode = true;
            itxtPassportExpirationDate.AffectsFormMode = true;
            itxtApplicationDate.AffectsFormMode = true;
            itxtAssignTo.AffectsFormMode = true;
            itxtLineManager.AffectsFormMode = true;
            itxtRemarks.AffectsFormMode = true;
            itxtJobRequisition.AffectsFormMode = true;
            itxtCurrentSalary.AffectsFormMode = true;
            itxtExpectedSalary.AffectsFormMode = true;
            itxtRecommendedSalary.AffectsFormMode = true;
            itxtBank.AffectsFormMode = true;
            itxtAccountNo.AffectsFormMode = true;
            itxtBankBranch.AffectsFormMode = true;
            icbDesignation.AffectsFormMode = true;
            icbDepartment.AffectsFormMode = true;
            icbBranch.AffectsFormMode = true;
            icbLocation.AffectsFormMode = true;
            //icbStaffingStatus.AffectsFormMode = true;
            itxtStaffingStatus.AffectsFormMode = true;
            icbwState.AffectsFormMode = true;
            icbwCountry.AffectsFormMode = true;
            icbhState.AffectsFormMode = true;
            icbhCountry.AffectsFormMode = true;
            icbGender.AffectsFormMode = true;
            icbCountryOfBirth.AffectsFormMode = true;
            icbMaritalStatus.AffectsFormMode = true;
            icbCitizenShip.AffectsFormMode = true;
        }

        private void FillFields()
        {
            try
            {
                MstCandidate oCan = oCollection.ElementAt<MstCandidate>(currentRecord);

                //Header Area
                txtFirstName.Value = oCan.FirstName;
                txtMiddleName.Value = oCan.MiddleName;
                txtLastName.Value = oCan.LastName;
                //txtEmployeeID.Value = oCan.EmpCode;
                txtUserCode.Value = oCan.UserCode;
                //cbJobRequisition.Select( oCan.JobRequisitionNo != null ? oCan.JobRequisitionNo.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                //txtJobRequisition.Value = Convert.ToString(oCan.JobRequisitionNo);
                cbDesignation.Select(oCan.Designation != null ? oCan.Designation.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbDepartment.Select(oCan.Department != null ? oCan.Department.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbBranch.Select(oCan.Branch != null ? oCan.Branch.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbLocation.Select(oCan.Location != null ? oCan.Location.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtCandidateNo.Value = oCan.CandidateNo.ToString();
                //if (oCan.ValidFrom != null)
                //{
                //    txtValidFrom.Value = Convert.ToDateTime(oCan.ValidFrom).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtValidFrom.Value = "";
                //}
                //if (oCan.ValidTo != null)
                //{
                //    txtValidTill.Value = Convert.ToDateTime(oCan.ValidTo).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtValidTill.Value = "";
                //}
                txtOfficePhone.Value = oCan.OfficePhone.ToString();
                txtExtention.Value = oCan.Extension.ToString();
                txtMobileNo.Value = oCan.MobilePhone.ToString();
                txtPager.Value = oCan.Pager.ToString();
                txtHomePhone.Value = oCan.HomePhone.ToString();
                txtFax.Value = oCan.Fax.ToString();
                //txtEmail.Value = oCan.Email;

                // Address Work Area

                txtwStreet.Value = oCan.WStreet;
                txtwStreetNo.Value = oCan.WStreetNo;
                txtwBlock.Value = oCan.WBlock;
                txtwBuilding.Value = oCan.WBuildingFloor;
                txtwZipCode.Value = oCan.WZipCode;
                txtwCity.Value = oCan.WCity;
                cbwState.Select(oCan.WState != null ? oCan.WState : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbwCountry.Select(oCan.WCountry != null ? oCan.WCountry : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

                // Address Home Area

                txthStreet.Value = oCan.HStreet;
                txthStreetNo.Value = oCan.HStreetNo;
                txthBlock.Value = oCan.HBlock;
                txthBuilding.Value = oCan.HBuildingFloor;
                txthZipCode.Value = oCan.HZipCode;
                txthCity.Value = oCan.HCity;
                cbhState.Select(oCan.HState != null ? oCan.HState : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbhCountry.Select(oCan.HCountry != null ? oCan.HCountry : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

                //Administration
                if (oCan.ApplicationDate != null)
                {
                    txtApplicationDate.Value = Convert.ToDateTime(oCan.ApplicationDate).ToString("yyyyMMdd");
                }
                else
                {
                    txtApplicationDate.Value = "";
                }
                if (oCan.AssignedTo != null)
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                        where a.ID == oCan.AssignedTo
                                        select a).FirstOrDefault();
                    //cbAssignTo.Select(oEmp.EmpID, SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtAssignTo.Value = oEmp.EmpID;
                }
                else
                {
                    //cbAssignTo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtAssignTo.Value = "";
                }
                //cbAssignTo.Select(oCan.AssignedTo != null ? oCan.AssignedTo.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_Index);
                if (oCan.StaffingStatus != null)
                {
                    MstLOVE oList = (from a in dbHrPayroll.MstLOVE
                                     where a.Code.Contains(oCan.StaffingStatus)
                                     select a).FirstOrDefault();
                    //cbStaffingStatus.Select(oList.Code, SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtStaffingStatus.Value = oCan.StaffingStatus;
                }
                else
                {
                    txtStaffingStatus.Value = "";
                }
                //cbStaffingStatus.Select(oCan.StaffingStatus != null ? oCan.StaffingStatus : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                if (oCan.LineManager != null)
                {
                    MstEmployee oLineManager = (from a in dbHrPayroll.MstEmployee
                                                where a.ID == oCan.LineManager
                                                select a).FirstOrDefault();
                    //cbLineManager.Select(oLineManager.EmpID, SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtLineManager.Value = oLineManager.EmpID;
                }
                else
                {
                    //cbLineManager.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtLineManager.Value = "";
                }
                //cbLineManager.Select( oCan.LineManager != null ? oCan.LineManager.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_Index);

                //Personal

                cbGender.Select(oCan.Gender != null ? oCan.Gender : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                //if (oCan.DOB != null)
                //{
                //    txtDateOfBirth.Value = Convert.ToDateTime(oCan.DOB).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtDateOfBirth.Value = "";
                //}
                //if (oCan.BornInCountry != null)
                //{
                //    cbCountryOfBirth.Select(oCan.BornInCountry, SAPbouiCOM.BoSearchKey.psk_ByValue);
                //}
                //else
                //{
                //    cbCountryOfBirth.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //}
                if (oCan.MartialStatus != null)
                {
                    cbMaritalStatus.Select(oCan.MartialStatus, SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                else
                {
                    cbMaritalStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                //txtNoOfChildrens.Value = oCan.NoOfChildren;
                //txtIdNo.Value = oCan.IDNo;
                //txtPassportNo.Value = oCan.PassportNumber;
                //if (oCan.PassportExpiration != null)
                //{
                //    txtPassportExpirationDate.Value = Convert.ToDateTime(oCan.PassportExpiration).ToString("yyyyMMdd");
                //}
                //else
                //{
                //    txtPassportExpirationDate.Value = "";
                //}
                //cbCitizenShip.Select(oCan.CitizenShip != null ? oCan.CitizenShip : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);

                //Finance

                txtCurrentSalary.Value = oCan.CurrentSalary.ToString();
                txtExpectedSalary.Value = oCan.ExpectedSalary.ToString();
                txtRecommendedSalary.Value = oCan.RecommendedSalary.ToString();
                //txtBank.Value = oCan.Bank;
                //txtBankBranch.Value = oCan.BankBranch;
                //txtAccountNo.Value = oCan.AccountNo;
                txtRemarks.Value = oCan.Remarks;

                btnMain.Caption = "Ok";

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Cadidate Didn't Load Succussfully Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDesignationCombo(SAPbouiCOM.ComboBox pCombo)
        {
            IEnumerable<MstDesignation> oDesignations = from a in dbHrPayroll.MstDesignation select a;
            pCombo.ValidValues.Add("-1", "");
            foreach (MstDesignation Designation in oDesignations)
            {
                pCombo.ValidValues.Add(Convert.ToString(Designation.Id), Convert.ToString(Designation.Name));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillDepartmentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            var AllDepartment = from a in dbHrPayroll.MstDepartment select a;
            pCombo.ValidValues.Add("-1", "");
            foreach (var Dept in AllDepartment)
            {
                pCombo.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillBranchCombo(SAPbouiCOM.ComboBox pCombo)
        {
            var AllBranches = from a in dbHrPayroll.MstBranches select a;
            pCombo.ValidValues.Add("-1", "");
            foreach (var Branch in AllBranches)
            {
                pCombo.ValidValues.Add(Convert.ToString(Branch.Id), Convert.ToString(Branch.Name));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillCountryCombo(SAPbouiCOM.ComboBox pCombo)
        {
            IEnumerable<MstCountry> Countries = from a in dbHrPayroll.MstCountry select a;
            pCombo.ValidValues.Add("-1", "");
            foreach (MstCountry Country in Countries)
            {
                pCombo.ValidValues.Add(Convert.ToString(Country.CountryCode), Convert.ToString(Country.CountryName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillStatesCombo(SAPbouiCOM.ComboBox pCombo)
        {
            IEnumerable<MstStates> States = from a in dbHrPayroll.MstStates select a;
            pCombo.ValidValues.Add("-1", "");
            foreach (MstStates State in States)
            {
                pCombo.ValidValues.Add(Convert.ToString(State.ID), Convert.ToString(State.StateName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
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
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillGenderCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Gender") select a;
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

        private void FillStaffingCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Staffing") select a;
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

        private void FillEmployeeCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstEmployee> Employees = from a in dbHrPayroll.MstEmployee select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstEmployee Employee in Employees)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Employee.EmpID), Convert.ToString(Employee.FirstName + Employee.LastName));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillJobRequisitionCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<TrnsJobRequisition> JobRequisitions = from a in dbHrPayroll.TrnsJobRequisition select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (TrnsJobRequisition JobRequisition in JobRequisitions)
                {
                    pCombo.ValidValues.Add(Convert.ToString(JobRequisition.ID), Convert.ToString(JobRequisition.DocNum));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch(Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillWithRequisitionSelection()
        {
            try
            {
                Int32 JobRequisition = 0;
                JobRequisition = Convert.ToInt32(txtJobRequisition.Value.Trim());
                TrnsJobRequisition oJR = (from a in dbHrPayroll.TrnsJobRequisition where a.DocNum == JobRequisition select a).FirstOrDefault();
                if (oJR != null)
                {
                    cbDesignation.Select(oJR.Designation != null ? oJR.Designation.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbDepartment.Select(oJR.Department != null ? oJR.Department.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbBranch.Select(oJR.Branch != null ? oJR.Branch.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbLocation.Select(oJR.Location != null ? oJR.Location.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oCollection = from a in dbHrPayroll.MstCandidate 
                          where a.StaffingStatus != "Selected" select a;
            Int32 i = 0;
            foreach (MstCandidate One in oCollection)
            {
                CodeIndex.Add(One.ID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void doFind()
        {
            try
            {
                PrepareSearchKeyHash();
                string strSql = sqlString.getSql("MstCandidate", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select Candidate", "Select  Candidate");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    currentObjId = st.Rows[0][0].ToString();
                    getRecord(currentObjId);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
