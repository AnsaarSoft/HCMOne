using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_VacReq : HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.EditText txtNoOfVacancies, txtDocumentNumber, txtPostingDate, txtValidTill, txtStatus;
        SAPbouiCOM.EditText txtAllocatedBudget, txtVacantPosition, txtApprovedForOccupency, txtRejectFromOccupency, txtStartDt, txtEndDt, txtRemarks, txtCostCenter;
        SAPbouiCOM.EditText txtExperianceFrom, txtExperianceTo;
        SAPbouiCOM.EditText txtBudgetSalaryFrom, txtBudgetSalaryTo, txtRecommendedSalary, txtCompsationRemarks, txtRecommendSalaryApprovedBy;
        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.ComboBox cbLocation, cbDepartment, cbBranch, cbDesignation, cbContractType;
        SAPbouiCOM.ComboBox cbExperianceUnit, cbRecommendSalaryApprovedBy, cbBudgetDocument;
        SAPbouiCOM.CheckBox chkTemporaryBasis;
        SAPbouiCOM.Matrix mtCompetency, mtSkills, mtEducation, mtCertification;
        SAPbouiCOM.Column cmIsnew, cmId, cmSerial, cmCompetency, cmDescription, cmRemarks;
        SAPbouiCOM.Column skIsnew, skId, skSerial, skSkill, skDescription, skRemarks, skPriorty;
        SAPbouiCOM.Column edIsnew, edId, edSerial, edEducation, edMajor, edDiploma;
        SAPbouiCOM.Column ctIsnew, ctId, ctSerial, ctCertification, ctModule;
        SAPbouiCOM.DataTable dtEducation, dtSkills, dtCompetency, dtCertification;
        SAPbouiCOM.Item itxtNoOfVacancies, itxtDocumentNumber, itxtPostingDate, itxtValidTill, itxtStatus;
        SAPbouiCOM.Item itxtAllocatedBudget, itxtVacantPosition, itxtApprovedForOccupency, itxtRejectFromOccupency, itxtStartDt, itxtEndDt, itxtRemarks, itxtCostCenter;
        SAPbouiCOM.Item itxtExperianceFrom, itxtExperianceTo;
        SAPbouiCOM.Item itxtBudgetSalaryFrom, itxtBudgetSalaryTo, itxtRecommendedSalary, itxtCompsationRemarks, ichkTemporaryBasis, itxtRecommendSalaryApprovedBy;

        IEnumerable<TrnsJobRequisition> oDocuments = null;

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
                    CheckState();
                    break;

            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);

            if (pVal.ItemUID == "mtskill" && pVal.ColUID == "priorty")
            {
                mtSkills.FlushToDataSource();
                AddEmptyRowSkill();
            }
            if (pVal.ItemUID == "mtcom" && pVal.ColUID == "remarks")
            {
                mtCompetency.FlushToDataSource();
                AddEmptyRowCompetency();
            }
            if (pVal.ItemUID == "mtedu" && pVal.ColUID == "dip")
            {
                mtEducation.FlushToDataSource();
                AddEmptyRowEducation();
            }
            if (pVal.ItemUID == "mtcert" && pVal.ColUID == "module")
            {
                mtCertification.FlushToDataSource();
                AddEmptyRowCertification();
            }
            if (pVal.ItemUID == "cbctype")
            {
                if (cbContractType.Value.Trim() == "Probation" || cbContractType.Value.Trim() == "Contract")
                {
                    itxtEndDt.Enabled = true;
                }
                else
                {
                    itxtEndDt.Enabled = false;
                }

            }
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE && btnMain.Caption == "Ok")
            {
                btnMain.Caption = "Update";
            }
                        
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
            oForm.Freeze(true);
            InitiallizeDocument("New");
            btnMain.Caption = "Add";
            oForm.Freeze(false);
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
            oForm.Freeze(true);
            FillDocument();
            oForm.Freeze(false);
        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                //oForm.Items.Item("").Specific;
                btnMain = oForm.Items.Item("btmain").Specific;
                //btnNew = oForm.Items.Item("btnew").Specific;
                //btnForward = oForm.Items.Item("btforw").Specific;
                //btnPrevios = oForm.Items.Item("btprev").Specific;

                //Header Area

                txtDocumentNumber = oForm.Items.Item("txdocno").Specific;
                itxtDocumentNumber = oForm.Items.Item("txdocno");
                oForm.DataSources.UserDataSources.Add("txdocno", SAPbouiCOM.BoDataType.dt_LONG_NUMBER, 10);
                txtDocumentNumber.DataBind.SetBound(true, "", "txdocno");
                txtDocumentNumber.Value = Convert.ToString(ds.GetDocumentNumber(-1, 15));

                txtNoOfVacancies = oForm.Items.Item("txvacno").Specific;
                itxtNoOfVacancies = oForm.Items.Item("txvacno");
                oForm.DataSources.UserDataSources.Add("txvacno", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 10);
                txtNoOfVacancies.DataBind.SetBound(true, "", "txvacno");
                txtNoOfVacancies.Value = "1";
                

                txtStatus = oForm.Items.Item("txstatus").Specific;
                itxtStatus = oForm.Items.Item("txstatus");
                oForm.DataSources.UserDataSources.Add("txstatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtStatus.DataBind.SetBound(true, "", "txstatus");
                

                txtPostingDate = oForm.Items.Item("txpostdt").Specific;
                itxtPostingDate = oForm.Items.Item("txpostdt");
                oForm.DataSources.UserDataSources.Add("txpostdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtPostingDate.DataBind.SetBound(true, "", "txpostdt");
                //txtPostingDate.Value = DateTime.Now.ToString("yyyyMMdd");

                txtValidTill = oForm.Items.Item("txvaliddt").Specific;
                itxtValidTill = oForm.Items.Item("txvaliddt");
                oForm.DataSources.UserDataSources.Add("txvaliddt", SAPbouiCOM.BoDataType.dt_DATE);
                txtValidTill.DataBind.SetBound(true, "", "txvaliddt");
                //txtValidTill.Value = DateTime.Now.ToString("yyyyMMdd");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbLocation.DataBind.SetBound(true, "", "cbloc");

                cbDepartment = oForm.Items.Item("cbdept").Specific;
                oForm.DataSources.UserDataSources.Add("cbdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbDepartment.DataBind.SetBound(true, "", "cbdept");

                cbBranch = oForm.Items.Item("cbbranch").Specific;
                oForm.DataSources.UserDataSources.Add("cbbranch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbBranch.DataBind.SetBound(true, "", "cbbranch");

                cbDesignation = oForm.Items.Item("cbdesig").Specific;
                oForm.DataSources.UserDataSources.Add("cbdesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbDesignation.DataBind.SetBound(true, "", "cbdesig");

                cbContractType = oForm.Items.Item("cbctype").Specific;
                oForm.DataSources.UserDataSources.Add("cbctype", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbContractType.DataBind.SetBound(true, "", "cbctype");

                cbBudgetDocument = oForm.Items.Item("cbbh").Specific;
                oForm.DataSources.UserDataSources.Add("cbbh", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbBudgetDocument.DataBind.SetBound(true, "", "cbbh");

                //Details Area

                txtAllocatedBudget = oForm.Items.Item("txallbug").Specific;
                itxtAllocatedBudget = oForm.Items.Item("txallbug");
                oForm.DataSources.UserDataSources.Add("txallbug", SAPbouiCOM.BoDataType.dt_SUM);
                txtAllocatedBudget.DataBind.SetBound(true, "", "txallbug");

                txtVacantPosition = oForm.Items.Item("txvacpos").Specific;
                itxtVacantPosition = oForm.Items.Item("txvacpos");
                oForm.DataSources.UserDataSources.Add("txvacpos", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtVacantPosition.DataBind.SetBound(true, "", "txvacpos");

                txtApprovedForOccupency = oForm.Items.Item("txappocu").Specific;
                itxtApprovedForOccupency = oForm.Items.Item("txappocu");
                oForm.DataSources.UserDataSources.Add("txappocu", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtApprovedForOccupency.DataBind.SetBound(true, "", "txappocu");

                txtRejectFromOccupency = oForm.Items.Item("txappocc2").Specific;
                itxtRejectFromOccupency = oForm.Items.Item("txappocc2");
                oForm.DataSources.UserDataSources.Add("txappocc2", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtRejectFromOccupency.DataBind.SetBound(true, "", "txappocc2");

                txtStartDt = oForm.Items.Item("txstartdt").Specific;
                itxtStartDt = oForm.Items.Item("txstartdt");
                oForm.DataSources.UserDataSources.Add("txstartdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtStartDt.DataBind.SetBound(true, "", "txstartdt");

                txtEndDt = oForm.Items.Item("txenddt").Specific;
                itxtEndDt = oForm.Items.Item("txenddt");
                oForm.DataSources.UserDataSources.Add("txenddt", SAPbouiCOM.BoDataType.dt_DATE);
                txtEndDt.DataBind.SetBound(true, "", "txenddt");

                txtCostCenter = oForm.Items.Item("txcc").Specific;
                itxtCostCenter = oForm.Items.Item("txcc");
                oForm.DataSources.UserDataSources.Add("txcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                txtCostCenter.DataBind.SetBound(true, "", "txcc");
                Program.objHrmsUI.addFms("frm_VacReq", "txcc", "-1", "SELECT PrcCode As [CostCenter Code], PrcName As [CostCenter Name] FROM dbo.OPRC");

                txtRemarks = oForm.Items.Item("txdetrem").Specific;
                itxtRemarks = oForm.Items.Item("txdetrem");
                oForm.DataSources.UserDataSources.Add("txdetrem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtRemarks.DataBind.SetBound(true, "", "txdetrem");

                chkTemporaryBasis = oForm.Items.Item("chktemp").Specific;
                ichkTemporaryBasis = oForm.Items.Item("chktemp");
                oForm.DataSources.UserDataSources.Add("chktemp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 2);
                chkTemporaryBasis.DataBind.SetBound(true, "", "chktemp");
                chkTemporaryBasis.Checked = false;

                //Skills Tab

                txtExperianceFrom = oForm.Items.Item("txexfrom").Specific;
                itxtExperianceFrom = oForm.Items.Item("txexfrom");
                oForm.DataSources.UserDataSources.Add("txexfrom", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 5);
                txtExperianceFrom.DataBind.SetBound(true, "", "txexfrom");
                txtExperianceFrom.Value = "0";

                txtExperianceTo = oForm.Items.Item("txexto").Specific;
                itxtExperianceTo = oForm.Items.Item("txexto");
                oForm.DataSources.UserDataSources.Add("txexto", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 5);
                txtExperianceTo.DataBind.SetBound(true, "", "txexto");
                txtExperianceTo.Value = "0";

                cbExperianceUnit = oForm.Items.Item("txexpunit").Specific;
                oForm.DataSources.UserDataSources.Add("txexpunit", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10);
                cbExperianceUnit.DataBind.SetBound(true, "", "txexpunit");

                mtCompetency = oForm.Items.Item("mtcom").Specific;
                dtCompetency = oForm.DataSources.DataTables.Item("dtcom");
                cmIsnew = mtCompetency.Columns.Item("isnew");
                cmIsnew.Visible = false;
                cmId = mtCompetency.Columns.Item("id");
                cmId.Visible = false;
                cmSerial = mtCompetency.Columns.Item("serial");
                cmCompetency = mtCompetency.Columns.Item("comid");
                cmDescription = mtCompetency.Columns.Item("desc");
                cmRemarks = mtCompetency.Columns.Item("remarks");


                mtSkills = oForm.Items.Item("mtskill").Specific;
                dtSkills = oForm.DataSources.DataTables.Item("dtskill");
                skIsnew = mtSkills.Columns.Item("isnew");
                skIsnew.Visible = false;
                skId = mtSkills.Columns.Item("id");
                skId.Visible = false;
                skSerial = mtSkills.Columns.Item("serial");
                skSkill = mtSkills.Columns.Item("skillid");
                skDescription = mtSkills.Columns.Item("desc");
                skRemarks = mtSkills.Columns.Item("remarks");
                skPriorty = mtSkills.Columns.Item("priorty");

                //Qualification

                mtEducation = oForm.Items.Item("mtedu").Specific;
                dtEducation = oForm.DataSources.DataTables.Item("dtedu");
                edIsnew = mtEducation.Columns.Item("isnew");
                edIsnew.Visible = false;
                edId = mtEducation.Columns.Item("id");
                edId.Visible = false;
                edSerial = mtEducation.Columns.Item("serial");
                edEducation = mtEducation.Columns.Item("edu");
                edMajor = mtEducation.Columns.Item("major");
                edDiploma = mtEducation.Columns.Item("dip");

                mtCertification = oForm.Items.Item("mtcert").Specific;
                dtCertification = oForm.DataSources.DataTables.Item("dtcert");
                ctIsnew = mtCertification.Columns.Item("isnew");
                ctIsnew.Visible = false;
                ctId = mtCertification.Columns.Item("id");
                ctId.Visible = false;
                ctSerial = mtCertification.Columns.Item("serial");
                ctCertification = mtCertification.Columns.Item("cert");
                ctModule = mtCertification.Columns.Item("module");

                //Compensation Tab

                txtBudgetSalaryFrom = oForm.Items.Item("txbsalfrom").Specific;
                itxtBudgetSalaryFrom = oForm.Items.Item("txbsalfrom");
                oForm.DataSources.UserDataSources.Add("txbsalfrom", SAPbouiCOM.BoDataType.dt_SUM);
                txtBudgetSalaryFrom.DataBind.SetBound(true, "", "txbsalfrom");

                txtBudgetSalaryTo = oForm.Items.Item("txbsalto").Specific;
                itxtBudgetSalaryTo = oForm.Items.Item("txbsalto");
                oForm.DataSources.UserDataSources.Add("txbsalto", SAPbouiCOM.BoDataType.dt_SUM);
                txtBudgetSalaryTo.DataBind.SetBound(true, "", "txbsalto");

                txtRecommendedSalary = oForm.Items.Item("txrecsal").Specific;
                itxtRecommendedSalary = oForm.Items.Item("txrecsal");
                oForm.DataSources.UserDataSources.Add("txrecsal", SAPbouiCOM.BoDataType.dt_SUM);
                txtRecommendedSalary.DataBind.SetBound(true, "", "txrecsal");

                txtRecommendSalaryApprovedBy = oForm.Items.Item("txapp").Specific;
                itxtRecommendSalaryApprovedBy = oForm.Items.Item("txapp");
                oForm.DataSources.UserDataSources.Add("txapp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtRecommendSalaryApprovedBy.DataBind.SetBound(true, "", "txapp");

                String strQuery = "SELECT EmpID AS [Employee Code], FirstName, MiddleName , LastName, DepartmentName, DesignationName, LocationName FROM " +  Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee WHERE flgActive = 1";
                Program.objHrmsUI.addFms("frm_VacReq", "txapp", "-1", strQuery);

                txtCompsationRemarks = oForm.Items.Item("txcomrem").Specific;
                itxtCompsationRemarks = oForm.Items.Item("txcomrem");
                oForm.DataSources.UserDataSources.Add("txcomrem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtCompsationRemarks.DataBind.SetBound(true, "", "txcomrem");

                //Initialize The Document
                FillDepartmentCombo(cbDepartment);
                FillDesignationCombo(cbDesignation);
                FillLocationsCombo(cbLocation);
                FillBranchCombo(cbBranch);
                FillContractType(cbContractType);
                FillExperianceUnit(cbExperianceUnit);
                FillCompetencyInColumn(cmCompetency);
                FillSkillInColumn(skSkill);
                FillEducationInColumn(edEducation);
                FillCertificationInColumn(ctCertification);
                //FillEmployeeCombo(cbRecommendSalaryApprovedBy);
                FillBudgetDocumentCombo(cbBudgetDocument);
                oForm.PaneLevel = 1;
                InitializeDocument();
                FormStatus();
                GetData();
                itxtPostingDate.Click();
                itxtDocumentNumber.Enabled = false;
                itxtStatus.Enabled = false;
                itxtNoOfVacancies.Enabled = false;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FormStatus()
        {
            try
            {
                itxtPostingDate.AffectsFormMode = true;
                itxtValidTill.AffectsFormMode = true;
                itxtAllocatedBudget.AffectsFormMode = true;
                itxtVacantPosition.AffectsFormMode = true;
                itxtApprovedForOccupency.AffectsFormMode = true;
                itxtRejectFromOccupency.AffectsFormMode = true;
                itxtStartDt.AffectsFormMode = true;
                itxtEndDt.AffectsFormMode = true;
                itxtCostCenter.AffectsFormMode = true;
                itxtRemarks.AffectsFormMode = true;

            }
            catch (Exception ex)
            {
            }
        }

        private void CheckState()
        {
            switch (btnMain.Caption)
            {
                case "Add":
                    if (ValidateForm())
                    {
                        if (AddDocument())
                        {
                            AddNewRecord();
                        }
                        //btnMain.Caption = "Add";
                    }
                    break;
                case "Update":
                    if (ValidateForm())
                    {
                        if (UpdateDocument())
                        {
                            AddNewRecord();
                        }
                        //btnMain.Caption = "Add";
                    }
                    break;
                case "Ok":
                    oForm.Close();
                    break;
            }
        }

        private void InitiallizeDocument(String pCase)
        {
            try
            {
                if (pCase == "New")
                {
                    txtDocumentNumber.Value = Convert.ToString(ds.GetDocumentNumber(-1, 15));
                    txtNoOfVacancies.Value = "1";
                    txtPostingDate.Value = "";
                    txtValidTill.Value = "";
                    txtStatus.Value = "";

                    cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbContractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Detail Tab

                    txtAllocatedBudget.Value = "";
                    txtVacantPosition.Value = "";
                    txtApprovedForOccupency.Value = "";
                    txtRejectFromOccupency.Value = "";
                    txtStartDt.Value = "";
                    txtEndDt.Value = "";
                    txtCostCenter.Value = "";
                    txtRemarks.Value = "";
                    chkTemporaryBasis.Checked = false; ;

                    //Skill & Competancy Tab

                    txtExperianceFrom.Value = "0";
                    txtExperianceTo.Value = "1";
                    cbExperianceUnit.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    dtSkills.Rows.Clear();
                    AddEmptyRowSkill();
                    mtSkills.LoadFromDataSource();

                    dtCompetency.Rows.Clear();
                    AddEmptyRowCompetency();
                    mtCompetency.LoadFromDataSource();

                    //Qualification Education & Certification Tab

                    dtEducation.Rows.Clear();
                    AddEmptyRowEducation();
                    mtEducation.LoadFromDataSource();

                    dtCertification.Rows.Clear();
                    AddEmptyRowCertification();
                    mtCertification.LoadFromDataSource();

                    //Compensation Tab

                    txtBudgetSalaryFrom.Value = "";
                    txtBudgetSalaryTo.Value = "";
                    txtRecommendedSalary.Value = "";
                    //cbRecommendSalaryApprovedBy.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtRecommendSalaryApprovedBy.Value = "";
                    txtCompsationRemarks.Value = "";

                    itxtNoOfVacancies.Enabled = false;
                    itxtStatus.Enabled = false;
                    itxtDocumentNumber.Enabled = false;
                    //Setting the environment
                    //sbtnMain.Caption = "Add";
                }
                if (pCase == "Search")
                {
                    txtDocumentNumber.Value = Convert.ToString(ds.GetDocumentNumber(-1, 15));
                    txtNoOfVacancies.Value = "";
                    txtPostingDate.Value = "";
                    txtValidTill.Value = "";
                    txtStatus.Value = "";
                    cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cbContractType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    //Detail Tab

                    txtAllocatedBudget.Value = "";
                    txtVacantPosition.Value = "";
                    txtApprovedForOccupency.Value = "";
                    txtRejectFromOccupency.Value = "";
                    txtStartDt.Value = "";
                    txtEndDt.Value = "";
                    txtCostCenter.Value = "";
                    txtRemarks.Value = "";
                    chkTemporaryBasis.Checked = false; ;

                    //Skill & Competancy Tab

                    txtExperianceFrom.Value = "0";
                    txtExperianceTo.Value = "1";
                    cbExperianceUnit.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    dtSkills.Rows.Clear();
                    AddEmptyRowSkill();
                    mtSkills.LoadFromDataSource();

                    dtCompetency.Rows.Clear();
                    AddEmptyRowCompetency();
                    mtCompetency.LoadFromDataSource();

                    //Qualification Education & Certification Tab

                    dtEducation.Rows.Clear();
                    AddEmptyRowEducation();
                    mtEducation.LoadFromDataSource();

                    dtCertification.Rows.Clear();
                    AddEmptyRowCertification();
                    mtCertification.LoadFromDataSource();

                    //Compensation Tab

                    txtBudgetSalaryFrom.Value = "";
                    txtBudgetSalaryTo.Value = "";
                    txtRecommendedSalary.Value = "";
                    //cbRecommendSalaryApprovedBy.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtRecommendSalaryApprovedBy.Value = "";
                    txtCompsationRemarks.Value = "";

                    //Setting the environment
                    //sbtnMain.Caption = "Add";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }

        }

        private Boolean ValidateForm()
        {
            Boolean retValue = false;
            DateTime ValidTillDate, PostingDate;
            Double BudgetAmount = 0.0, RecommendedAmount = 0.0;
            Int32 Location = 0, Department = 0, Branch = 0, Designation = 0, BudgetDoc = 0;
            try
            {
                retValue = true;
                //Retrive the Budget Document

                
                BudgetDoc = Convert.ToInt32(cbBudgetDocument.Value.Trim());
                TrnsHeadBudget oDoc = (from a in dbHrPayroll.TrnsHeadBudget where a.ID == BudgetDoc select a).FirstOrDefault();
                if (oDoc == null)
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Select Budget Document", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                //Checking Valid Till Date of Vacancy Requisition.
                ValidTillDate = DateTime.ParseExact(txtValidTill.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                if (!(oDoc.FromDt <= ValidTillDate && oDoc.ToDt >= ValidTillDate))
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Valid Till Date is Out of Range.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }

                //Posting Date Check
                PostingDate = DateTime.ParseExact(txtPostingDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                if (!( oDoc.FromDt <= PostingDate && oDoc.ToDt >= PostingDate ))
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Posting Date is Out of Range.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                //Budget Amount checking.
                BudgetAmount = Convert.ToDouble(txtBudgetSalaryTo.Value);
                RecommendedAmount = Convert.ToDouble(txtRecommendedSalary.Value);

                if (BudgetAmount < RecommendedAmount)
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Recommended Salary can't be greater than Budgeted Amount", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }

                //Checking for no of vacancy
                if (String.IsNullOrEmpty(txtNoOfVacancies.Value.Trim()))
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Enter No of Vacancies", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }

                //Checking location
                Location = Convert.ToInt32(cbLocation.Value.Trim());
                if (oDoc.Location != Location)
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Location must be same as on Budget Document", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }

                //validation for collection set of branch department and designation
                foreach (TrnsHeadBudgetDetail OneRow in oDoc.TrnsHeadBudgetDetail)
                {
                    Branch = Convert.ToInt32(cbBranch.Value.Trim());
                    Department = Convert.ToInt32(cbDepartment.Value.Trim());
                    Designation = Convert.ToInt32(cbDesignation.Value.Trim());

                    if (!(Convert.ToInt32(OneRow.BranchID) == Branch && Convert.ToInt32(OneRow.DepartmentID) == Department && Convert.ToInt32(OneRow.DesignationID) == Designation))
                    {
                        retValue = false;
                    }
                    else
                    {
                        retValue = true;
                        break;
                    }
                }
                if (retValue == false)
                {
                    oApplication.StatusBar.SetText("Branch, Department & Designation was not define in Budget", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                
            }
            catch (Exception ex)
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
                TrnsJobRequisition oDoc = new TrnsJobRequisition();
                Int32 LocId = 0, DeptId = 0, DesigId = 0, BrnchId = 0, BudgetDocument = 0;
                String CntrType;

                //Header Area
                oDoc.DocNum = Convert.ToInt32(txtDocumentNumber.Value.Trim());
                oDoc.Series = -1;
                oDoc.DocStatus = "LV0001";
                oDoc.PostingDate = DateTime.ParseExact(txtPostingDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                oDoc.ValidUpto = DateTime.ParseExact(txtValidTill.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                oDoc.NoOfVacancy = Convert.ToByte(txtNoOfVacancies.Value.Trim());
                LocId = Convert.ToInt32(cbLocation.Value.Trim());
                oDoc.LocationID = LocId;
                oDoc.Location = (from a in dbHrPayroll.MstLocation where a.Id == LocId select a.Name).FirstOrDefault();
                DeptId = Convert.ToInt32(cbDepartment.Value.Trim());
                oDoc.DeptID = DeptId;
                oDoc.Department = (from a in dbHrPayroll.MstDepartment where a.ID == DeptId select a.DeptName).FirstOrDefault();
                DesigId = Convert.ToInt32(cbDesignation.Value.Trim());
                oDoc.DesignationID = DesigId;
                oDoc.Designation = (from a in dbHrPayroll.MstDesignation where a.Id == DesigId select a.Name).FirstOrDefault();
                BrnchId = Convert.ToInt32(cbBranch.Value.Trim());
                oDoc.BranchID = BrnchId;
                oDoc.Branch = (from a in dbHrPayroll.MstBranches where a.Id == BrnchId select a.Name).FirstOrDefault();
                CntrType = cbContractType.Value.Trim();
                oDoc.ContractType = CntrType;
                oDoc.ContractTypeLOV = "ContractType";
                BudgetDocument = cbBudgetDocument.Value.Trim() != "-1" ? Convert.ToInt32(cbBudgetDocument.Value.Trim()) : 0;
                oDoc.BaseDoc = BudgetDocument;

                //Detail Section

                oDoc.AllocatedBudget = Convert.ToDecimal(txtAllocatedBudget.Value.Trim());
                oDoc.VacantPosition = txtVacantPosition.Value.Trim();
                oDoc.AppOccupancy = txtApprovedForOccupency.Value.Trim();
                oDoc.RejOccupancy = txtRejectFromOccupency.Value.Trim();
                if (!String.IsNullOrEmpty(txtStartDt.Value))
                {
                    oDoc.StartDate = DateTime.ParseExact(txtStartDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                if (!String.IsNullOrEmpty(txtEndDt.Value))
                {
                    oDoc.EndDate = DateTime.ParseExact(txtEndDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                oDoc.CostCenter = txtCostCenter.Value.Trim();
                oDoc.Remarks = txtRemarks.Value.Trim();
                oDoc.FlgTempBasis = chkTemporaryBasis.Checked;
                oDoc.FlgAdvertised = false;
                //Skill Connection

                mtSkills.FlushToDataSource();
                Int32 SkillRows = dtSkills.Rows.Count;
                for (Int32 i = 0; i < SkillRows; i++)
                {
                    
                    String SkillID, Desc, Remarks, Priorty, isnew, id;
                    SkillID = dtSkills.GetValue(skSkill.DataBind.Alias, i);
                    Desc = dtSkills.GetValue(skDescription.DataBind.Alias, i);
                    Remarks = dtSkills.GetValue(skRemarks.DataBind.Alias, i);
                    Priorty = Convert.ToString(dtSkills.GetValue(skPriorty.DataBind.Alias, i));
                    isnew = dtSkills.GetValue(skIsnew.DataBind.Alias, i);
                    id = Convert.ToString(dtSkills.GetValue(skId.DataBind.Alias, i));
                    if (!String.IsNullOrEmpty(SkillID))
                    {
                        TrnsJRDetailSkills SkillDetail = new TrnsJRDetailSkills();
                        SkillDetail.SkillID = Convert.ToInt32(SkillID);
                        SkillDetail.Description = Desc;
                        SkillDetail.Remarks = Remarks;
                        SkillDetail.Priorty = Convert.ToByte(Priorty);
                        oDoc.TrnsJRDetailSkills.Add(SkillDetail);
                    }
                }

                //Competancy Connection
                mtCompetency.FlushToDataSource();
                Int32 CompetancyRows = dtCompetency.Rows.Count;
                for (Int32 i = 0; i < CompetancyRows; i++)
                {
                    
                    String CompetancyID, Desc, Remarks, isnew, id;
                    CompetancyID = dtCompetency.GetValue(cmCompetency.DataBind.Alias, i);
                    Desc = dtCompetency.GetValue(cmDescription.DataBind.Alias, i);
                    Remarks = dtCompetency.GetValue(cmRemarks.DataBind.Alias, i);
                    isnew = dtCompetency.GetValue(cmIsnew.DataBind.Alias, i);
                    id = Convert.ToString(dtCompetency.GetValue(cmId.DataBind.Alias, i));
                    if (!String.IsNullOrEmpty(CompetancyID))
                    {
                        TrnsJRDetailCompetancy CompetancyDetail = new TrnsJRDetailCompetancy();
                        CompetancyDetail.CompetancyID = Convert.ToInt32(CompetancyID);
                        CompetancyDetail.Description = Desc;
                        CompetancyDetail.Remarks = Remarks;
                        oDoc.TrnsJRDetailCompetancy.Add(CompetancyDetail);
                    }
                }
                oDoc.ExperianceFrom = Convert.ToByte(txtExperianceFrom.Value.Trim());
                oDoc.ExperianceTo = Convert.ToByte(txtExperianceTo.Value.Trim());
                oDoc.ExperianceUnit = Convert.ToString(cbExperianceUnit.Selected.Description.Trim());
                
                //Education Connection
                mtEducation.FlushToDataSource();
                Int32 EducationRows = dtEducation.Rows.Count;
                for (Int32 i = 0; i < EducationRows; i++)
                {

                    String EducationID, Major, Diploma, isnew, id;
                    EducationID = dtEducation.GetValue(edEducation.DataBind.Alias, i);
                    Major = dtEducation.GetValue(edMajor.DataBind.Alias, i);
                    Diploma = dtEducation.GetValue(edDiploma.DataBind.Alias, i);
                    isnew = dtEducation.GetValue(edIsnew.DataBind.Alias, i);
                    id = Convert.ToString(dtEducation.GetValue(edId.DataBind.Alias, i));
                    if ( !String.IsNullOrEmpty(EducationID))
                    {
                        TrnsJRDetailEducation EducationDetail = new TrnsJRDetailEducation();
                        EducationDetail.EducationType = Convert.ToInt32(EducationID);
                        EducationDetail.Major = Major;
                        EducationDetail.Diploma = Diploma;
                        oDoc.TrnsJRDetailEducation.Add(EducationDetail);
                    }
                }

                //Certification Connection
                mtCertification.FlushToDataSource();
                Int32 CertificationRows = dtCertification.Rows.Count;
                for (Int32 i = 0; i < CertificationRows; i++)
                {

                    String CertID, Module, isnew, id;
                    CertID = dtCertification.GetValue(ctCertification.DataBind.Alias, i);
                    Module = dtCertification.GetValue(ctModule.DataBind.Alias, i);
                    isnew = dtCertification.GetValue(ctIsnew.DataBind.Alias, i);
                    id = Convert.ToString(dtCertification.GetValue(ctId.DataBind.Alias, i));
                    if ( !String.IsNullOrEmpty(CertID))
                    {
                        TrnsJRDetailCertification CertificationDetail = new TrnsJRDetailCertification();
                        CertificationDetail.CertificationType = Convert.ToInt32(CertID);
                        CertificationDetail.Module = Module;
                        oDoc.TrnsJRDetailCertification.Add(CertificationDetail);
                    }
                }

                //Compensation Area

                oDoc.BudgetSalaryFrom = Convert.ToDecimal(txtBudgetSalaryFrom.Value.Trim());
                oDoc.BudgetSalaryTo = Convert.ToDecimal(txtBudgetSalaryTo.Value.Trim());
                oDoc.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
                //oDoc.ApprovedBy = cbRecommendSalaryApprovedBy.Value.Trim();
                oDoc.ApprovedBy = txtRecommendSalaryApprovedBy.Value.Trim();
                oDoc.CompensationRemarks = Convert.ToString(txtCompsationRemarks.Value.Trim());

                //DocIDs
                oDoc.CreateDate = DateTime.Now;
                oDoc.UserID = oCompany.UserName;

                dbHrPayroll.TrnsJobRequisition.InsertOnSubmit(oDoc);
                dbHrPayroll.SubmitChanges();
                GetData();
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Function : AddDocument Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private Boolean UpdateDocument()
        {
            Boolean retValue = true;
            try
            {
                TrnsJobRequisition oDoc = null;
                Int32 LocId = 0, DeptId = 0, DesigId = 0, BrnchId = 0, uGetDoc = 0, uDocType = 0, uSeries = 0, BudgetDocument = 0;
                String CntrType;
                uGetDoc = Convert.ToInt32(txtDocumentNumber.Value.Trim());
                uDocType = 15;
                uSeries = -1;
                oDoc = (from a in dbHrPayroll.TrnsJobRequisition where a.DocNum == uGetDoc && a.DocType == uDocType && a.Series == uSeries select a).FirstOrDefault();

                //Header Area
                if ( oDoc != null )
                {
                    oDoc.PostingDate = DateTime.ParseExact(txtPostingDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.ValidUpto = DateTime.ParseExact(txtValidTill.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.NoOfVacancy = Convert.ToByte(txtNoOfVacancies.Value);
                    LocId = Convert.ToInt32(cbLocation.Value);
                    oDoc.LocationID = LocId;
                    oDoc.Location = (from a in dbHrPayroll.MstLocation where a.Id == LocId select a.Name).FirstOrDefault();
                    DeptId = Convert.ToInt32(cbDepartment.Value);
                    oDoc.DeptID = DeptId;
                    oDoc.Department = (from a in dbHrPayroll.MstDepartment where a.ID == DeptId select a.DeptName).FirstOrDefault();
                    DesigId = Convert.ToInt32(cbDesignation.Value);
                    oDoc.DesignationID = DesigId;
                    oDoc.Designation = (from a in dbHrPayroll.MstDesignation where a.Id == DesigId select a.Name).FirstOrDefault();
                    BrnchId = Convert.ToInt32(cbBranch.Value);
                    oDoc.BranchID = BrnchId;
                    oDoc.Branch = (from a in dbHrPayroll.MstBranches where a.Id == BrnchId select a.Name).FirstOrDefault();
                    CntrType = cbContractType.Value;
                    oDoc.ContractType = CntrType;
                    oDoc.ContractTypeLOV = "ContractType";
                    BudgetDocument = cbBudgetDocument.Value.Trim() != "-1" ? Convert.ToInt32(cbBudgetDocument.Value.Trim()) : 0;
                    oDoc.BaseDoc = BudgetDocument;

                    //Detail Section

                    oDoc.AllocatedBudget = Convert.ToDecimal(txtAllocatedBudget.Value);
                    oDoc.VacantPosition = txtVacantPosition.Value;
                    oDoc.AppOccupancy = txtApprovedForOccupency.Value;
                    oDoc.RejOccupancy = txtRejectFromOccupency.Value;
                    if (!String.IsNullOrEmpty(txtStartDt.Value))
                    {
                        oDoc.StartDate = DateTime.ParseExact(txtStartDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    if (!String.IsNullOrEmpty(txtEndDt.Value))
                    {
                        oDoc.EndDate = DateTime.ParseExact(txtEndDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    oDoc.CostCenter = txtCostCenter.Value;
                    oDoc.Remarks = txtRemarks.Value;
                    oDoc.FlgTempBasis = chkTemporaryBasis.Checked;
                    oDoc.FlgAdvertised = false;
                    //Skill Connection

                    mtSkills.FlushToDataSource();
                    Int32 SkillRows = dtSkills.Rows.Count;
                    for (Int32 i = 0; i < SkillRows; i++)
                    {

                        String SkillID, Desc, Remarks, Priorty, isnew, id;
                        SkillID = dtSkills.GetValue(skSkill.DataBind.Alias, i);
                        Desc = dtSkills.GetValue(skDescription.DataBind.Alias, i);
                        Remarks = dtSkills.GetValue(skRemarks.DataBind.Alias, i);
                        Priorty = Convert.ToString(dtSkills.GetValue(skPriorty.DataBind.Alias, i));
                        isnew = dtSkills.GetValue(skIsnew.DataBind.Alias, i);
                        id = Convert.ToString(dtSkills.GetValue(skId.DataBind.Alias, i));
                        if (!String.IsNullOrEmpty(SkillID))
                        {
                            if (isnew == "Y")
                            {
                                TrnsJRDetailSkills SkillDetail = new TrnsJRDetailSkills();
                                SkillDetail.SkillID = Convert.ToInt32(SkillID);
                                SkillDetail.Description = Desc;
                                SkillDetail.Remarks = Remarks;
                                SkillDetail.Priorty = Convert.ToByte(Priorty);
                                oDoc.TrnsJRDetailSkills.Add(SkillDetail);
                            }
                            else
                            {
                                TrnsJRDetailSkills SkillDetail = null;
                                SkillDetail = (from a in dbHrPayroll.TrnsJRDetailSkills where a.Id == Convert.ToInt32(id) select a).FirstOrDefault();
                                if (SkillDetail != null)
                                {
                                    SkillDetail.Description = Desc;
                                    SkillDetail.Remarks = Remarks;
                                    SkillDetail.Priorty = Convert.ToByte(Priorty);
                                }
                            }
                        }
                    }

                    //Competancy Connection
                    mtCompetency.FlushToDataSource();
                    Int32 CompetancyRows = dtCompetency.Rows.Count;
                    for (Int32 i = 0; i < CompetancyRows; i++)
                    {

                        String CompetancyID, Desc, Remarks, isnew, id;
                        CompetancyID = dtCompetency.GetValue(cmCompetency.DataBind.Alias, i);
                        Desc = dtCompetency.GetValue(cmDescription.DataBind.Alias, i);
                        Remarks = dtCompetency.GetValue(cmRemarks.DataBind.Alias, i);
                        isnew = dtCompetency.GetValue(cmIsnew.DataBind.Alias, i);
                        id = Convert.ToString(dtCompetency.GetValue(cmId.DataBind.Alias, i));
                        if (!String.IsNullOrEmpty(CompetancyID))
                        {
                            if (isnew == "Y")
                            {
                                TrnsJRDetailCompetancy CompetancyDetail = new TrnsJRDetailCompetancy();
                                CompetancyDetail.CompetancyID = Convert.ToInt32(CompetancyID);
                                CompetancyDetail.Description = Desc;
                                CompetancyDetail.Remarks = Remarks;
                                oDoc.TrnsJRDetailCompetancy.Add(CompetancyDetail);
                            }
                            else
                            {
                                TrnsJRDetailCompetancy CompetancyDetail = null;
                                CompetancyDetail = (from a in dbHrPayroll.TrnsJRDetailCompetancy where a.ID == Convert.ToInt32(id) select a).FirstOrDefault();
                                if (CompetancyDetail != null)
                                {
                                    CompetancyDetail.Description = Desc;
                                    CompetancyDetail.Remarks = Remarks;
                                }
                            }
                        }
                    }
                    oDoc.ExperianceFrom = Convert.ToByte(txtExperianceFrom.Value.Trim());
                    oDoc.ExperianceTo = Convert.ToByte(txtExperianceTo.Value.Trim());
                    oDoc.ExperianceUnit = Convert.ToString(cbExperianceUnit.Selected.Description.Trim());

                    //Education Connection
                    mtEducation.FlushToDataSource();
                    Int32 EducationRows = dtEducation.Rows.Count;
                    for (Int32 i = 0; i < EducationRows; i++)
                    {

                        String EducationID, Major, Diploma, isnew, id;
                        EducationID = dtEducation.GetValue(edEducation.DataBind.Alias, i);
                        Major = dtEducation.GetValue(edMajor.DataBind.Alias, i);
                        Diploma = dtEducation.GetValue(edDiploma.DataBind.Alias, i);
                        isnew = dtEducation.GetValue(edIsnew.DataBind.Alias, i);
                        id = Convert.ToString(dtEducation.GetValue(edId.DataBind.Alias, i));
                        if (!String.IsNullOrEmpty(EducationID))
                        {
                            if (isnew == "Y")
                            {
                                TrnsJRDetailEducation EducationDetail = new TrnsJRDetailEducation();
                                EducationDetail.EducationType = Convert.ToInt32(EducationID);
                                EducationDetail.Major = Major;
                                EducationDetail.Diploma = Diploma;
                                oDoc.TrnsJRDetailEducation.Add(EducationDetail);
                            }
                            else
                            {
                                TrnsJRDetailEducation EducationDetail = null;
                                EducationDetail = (from a in dbHrPayroll.TrnsJRDetailEducation where a.ID == Convert.ToInt32(id) select a ).FirstOrDefault();
                                if (EducationDetail != null)
                                {
                                    EducationDetail.Major = Major;
                                    EducationDetail.Diploma = Diploma;
                                }
                            }
                        }
                    }

                    //Certification Connection
                    mtCertification.FlushToDataSource();
                    Int32 CertificationRows = dtCertification.Rows.Count;
                    for (Int32 i = 0; i < CertificationRows; i++)
                    {

                        String CertID, Module, isnew, id;
                        CertID = dtCertification.GetValue(ctCertification.DataBind.Alias, i);
                        Module = dtCertification.GetValue(ctModule.DataBind.Alias, i);
                        isnew = dtCertification.GetValue(ctIsnew.DataBind.Alias, i);
                        id = Convert.ToString(dtCertification.GetValue(ctId.DataBind.Alias, i));
                        if (!String.IsNullOrEmpty(CertID))
                        {
                            if (isnew == "Y")
                            {
                                TrnsJRDetailCertification CertificationDetail = new TrnsJRDetailCertification();
                                CertificationDetail.CertificationType = Convert.ToInt32(CertID);
                                CertificationDetail.Module = Module;
                                oDoc.TrnsJRDetailCertification.Add(CertificationDetail);
                            }
                            else
                            {
                                TrnsJRDetailCertification CertificationDetail = null;
                                CertificationDetail = (from a in dbHrPayroll.TrnsJRDetailCertification where a.ID == Convert.ToInt32(id) select a ).FirstOrDefault();
                                if (CertificationDetail != null)
                                {
                                    CertificationDetail.Module = Module;
                                }
                            }
                        }
                    }

                    //Compensation Area

                    oDoc.BudgetSalaryFrom = Convert.ToDecimal(txtBudgetSalaryFrom.Value.Trim());
                    oDoc.BudgetSalaryTo = Convert.ToDecimal(txtBudgetSalaryTo.Value.Trim());
                    oDoc.RecommendedSalary = Convert.ToDecimal(txtRecommendedSalary.Value.Trim());
                    //oDoc.ApprovedBy = cbRecommendSalaryApprovedBy.Value.Trim();
                    oDoc.ApprovedBy = txtRecommendSalaryApprovedBy.Value.Trim();
                    oDoc.CompensationRemarks = Convert.ToString(txtCompsationRemarks.Value.Trim());

                    //DocIDs
                    oDoc.UpdateDate = DateTime.Now;
                    oDoc.UpdatedBy = "manager";

                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Document Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception UpdateDocument Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private void FillDocument()
        {
            try
            {
                Int32 fDocNum = 0, fDocType = 15, fSeries = -1;
                TrnsJobRequisition oDoc = null;
                oDoc = oDocuments.ElementAt<TrnsJobRequisition>(currentRecord);
                if (oDoc != null)
                {
                    //Header Budget
                    txtDocumentNumber.Value = oDoc.DocNum.ToString();
                    txtNoOfVacancies.Value = oDoc.NoOfVacancy.ToString();
                    txtPostingDate.Value = Convert.ToDateTime(oDoc.PostingDate).ToString("yyyyMMdd");
                    txtValidTill.Value = Convert.ToDateTime(oDoc.ValidUpto).ToString("yyyyMMdd");
                    MstLOVE oValue = (from a in dbHrPayroll.MstLOVE where a.Code == oDoc.DocStatus.Trim() select a).FirstOrDefault();
                    if (oValue != null)
                    {
                        txtStatus.Value = oValue.Value;
                    }
                    cbLocation.Select(oDoc.LocationID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbBranch.Select(oDoc.BranchID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbDesignation.Select(oDoc.DesignationID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbDepartment.Select(oDoc.DeptID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbContractType.Select(oDoc.ContractType.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbBudgetDocument.Select(oDoc.BaseDoc.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                    //Detail Tab

                    txtAllocatedBudget.Value = oDoc.AllocatedBudget.ToString();
                    txtVacantPosition.Value = oDoc.VacantPosition.ToString();
                    txtApprovedForOccupency.Value = oDoc.AppOccupancy.ToString();
                    txtRejectFromOccupency.Value = oDoc.RejOccupancy.ToString();
                    txtStartDt.Value = Convert.ToDateTime( oDoc.StartDate).ToString("yyyyMMdd");
                    txtEndDt.Value = Convert.ToDateTime(oDoc.EndDate).ToString("yyyyMMdd");
                    txtCostCenter.Value = oDoc.CostCenter.ToString();
                    txtRemarks.Value = oDoc.Remarks;
                    chkTemporaryBasis.Checked = oDoc.FlgTempBasis;

                    //Skill & Competancy Tab

                    txtExperianceFrom.Value = oDoc.ExperianceFrom.ToString();
                    txtExperianceTo.Value = oDoc.ExperianceTo.ToString();
                    cbExperianceUnit.Select(oDoc.ExperianceUnit, SAPbouiCOM.BoSearchKey.psk_ByDescription);

                    dtSkills.Rows.Clear();
                    Int32 i = 0;
                    foreach( TrnsJRDetailSkills OneLine in oDoc.TrnsJRDetailSkills )
                    {
                        dtSkills.Rows.Add(1);
                        dtSkills.SetValue(skSkill.DataBind.Alias, i, OneLine.SkillID.ToString());
                        dtSkills.SetValue(skDescription.DataBind.Alias, i , OneLine.Description);
                        dtSkills.SetValue(skPriorty.DataBind.Alias, i , OneLine.Priorty.ToString());
                        dtSkills.SetValue(skIsnew.DataBind.Alias, i, "N");
                        dtSkills.SetValue(skId.DataBind.Alias, i, OneLine.Id.ToString());
                        i++;
                    }
                    AddEmptyRowSkill();
                    mtSkills.LoadFromDataSource();

                    dtCompetency.Rows.Clear();
                    i = 0;
                    foreach (TrnsJRDetailCompetancy OneLine in oDoc.TrnsJRDetailCompetancy)
                    {
                        dtCompetency.Rows.Add(1);
                        dtCompetency.SetValue(cmCompetency.DataBind.Alias, i, OneLine.CompetancyID.ToString());
                        dtCompetency.SetValue(cmDescription.DataBind.Alias, i, OneLine.Description.ToString());
                        dtCompetency.SetValue(cmRemarks.DataBind.Alias, i, OneLine.Remarks);
                        dtCompetency.SetValue(cmIsnew.DataBind.Alias, i, "N");
                        dtCompetency.SetValue(cmId.DataBind.Alias, i, OneLine.ID.ToString());
                        i++;
                    }
                    AddEmptyRowCompetency();
                    mtCompetency.LoadFromDataSource();

                    //Qualification Education & Certification Tab

                    dtEducation.Rows.Clear();
                    i = 0;
                    foreach (TrnsJRDetailEducation OneLine in oDoc.TrnsJRDetailEducation)
                    {
                        dtEducation.Rows.Add(1);
                        dtEducation.SetValue(edEducation.DataBind.Alias, i,OneLine.EducationType.ToString());
                        dtEducation.SetValue(edDiploma.DataBind.Alias, i, OneLine.Diploma);
                        dtEducation.SetValue(edMajor.DataBind.Alias, i, OneLine.Major);
                        dtEducation.SetValue(edIsnew.DataBind.Alias, i, "N");
                        dtEducation.SetValue(edId.DataBind.Alias, i, OneLine.ID.ToString());
                    }
                    AddEmptyRowEducation();
                    mtEducation.LoadFromDataSource();

                    dtCertification.Rows.Clear();
                    i = 0;
                    foreach (TrnsJRDetailCertification OneLine in oDoc.TrnsJRDetailCertification)
                    {
                        dtCertification.Rows.Add(1);
                        dtCertification.SetValue(ctCertification.DataBind.Alias, i, OneLine.CertificationType.ToString());
                        dtCertification.SetValue(ctModule.DataBind.Alias, i, OneLine.Module);
                        dtCertification.SetValue(ctIsnew.DataBind.Alias, i, "N");
                        dtCertification.SetValue(ctId.DataBind.Alias, i, OneLine.ID.ToString());
                    }
                    AddEmptyRowCertification();
                    mtCertification.LoadFromDataSource();
                    
                    //Compensation Tab

                    txtBudgetSalaryFrom.Value = oDoc.BudgetSalaryFrom.ToString();
                    txtBudgetSalaryTo.Value = oDoc.BudgetSalaryTo.ToString();
                    txtRecommendedSalary.Value = oDoc.RecommendedSalary.ToString();
                    //cbRecommendSalaryApprovedBy.Select(oDoc.ApprovedBy, SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtRecommendSalaryApprovedBy.Value = oDoc.ApprovedBy;
                    txtCompsationRemarks.Value = oDoc.CompensationRemarks.ToString();

                    //Setting the environment
                    btnMain.Caption = "Ok";
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Document didn't load Successfully Error : " +Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oDocuments = from a in dbHrPayroll.TrnsJobRequisition select a;
            Int32 i = 0;
            foreach (TrnsJobRequisition oDoc in oDocuments)
            {
                CodeIndex.Add(oDoc.ID, oDoc.DocNum);
                i++;
            }
            totalRecord = i;
        }

        private void InitializeDocument()
        {
            try
            {
                btnMain.Caption = "Add";
                txtDocumentNumber.Value = Convert.ToString(ds.GetDocumentNumber(-1, 15));
                AddEmptyRowCompetency();
                AddEmptyRowSkill();
                AddEmptyRowEducation();
                AddEmptyRowCertification();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void AddEmptyRowEducation()
        {
            Int32 RowValue = 0;
            if (dtEducation.Rows.Count == 0)
            {
                dtEducation.Rows.Add(1);
                RowValue = dtEducation.Rows.Count;
                dtEducation.SetValue(edIsnew.DataBind.Alias, RowValue - 1, "Y");
                dtEducation.SetValue(edId.DataBind.Alias, RowValue - 1, "0");
                dtEducation.SetValue(edEducation.DataBind.Alias, RowValue - 1, "");
                dtEducation.SetValue(edMajor.DataBind.Alias, RowValue - 1, "");
                dtEducation.SetValue(edDiploma.DataBind.Alias, RowValue - 1, "");
                //dtEducation.SetValue(edSerial.DataBind.Alias, RowValue - 1, RowValue);
                mtEducation.AddRow(1, 0);
            }
            else
            {
                if (dtEducation.GetValue(edEducation.DataBind.Alias, dtEducation.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtEducation.Rows.Add(1);
                    RowValue = dtEducation.Rows.Count;
                    dtEducation.SetValue(edIsnew.DataBind.Alias, RowValue - 1, "Y");
                    dtEducation.SetValue(edId.DataBind.Alias, RowValue - 1, "0");
                    dtEducation.SetValue(edEducation.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(edMajor.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(edDiploma.DataBind.Alias, RowValue - 1, "");
                    //dtEducation.SetValue(edSerial.DataBind.Alias, RowValue - 1, RowValue);
                    mtEducation.AddRow(1, RowValue);
                }
            }
            mtEducation.LoadFromDataSource();
        }

        private void AddEmptyRowCertification()
        {
            Int32 RowValue = 0;
            if (dtCertification.Rows.Count == 0)
            {
                dtCertification.Rows.Add(1);
                RowValue = dtCertification.Rows.Count;
                dtCertification.SetValue(ctIsnew.DataBind.Alias, RowValue - 1, "Y");
                dtCertification.SetValue(ctId.DataBind.Alias, RowValue - 1, "0");
                dtCertification.SetValue(ctCertification.DataBind.Alias, RowValue - 1, "");
                dtCertification.SetValue(ctModule.DataBind.Alias, RowValue - 1, "");
                //dtCertification.SetValue(ctSerial.DataBind.Alias, RowValue - 1, RowValue);
                mtCertification.AddRow(1, 0);
            }
            else
            {
                if (dtCertification.GetValue(ctCertification.DataBind.Alias, dtCertification.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtCertification.Rows.Add(1);
                    RowValue = dtCertification.Rows.Count;
                    dtCertification.SetValue(ctIsnew.DataBind.Alias, RowValue - 1, "Y");
                    dtCertification.SetValue(ctId.DataBind.Alias, RowValue - 1, "0");
                    dtCertification.SetValue(ctCertification.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(ctModule.DataBind.Alias, RowValue - 1, "");
                    //dtCertification.SetValue(ctSerial.DataBind.Alias, RowValue - 1, RowValue);
                    mtCertification.AddRow(1, RowValue);
                }
            }
            mtCertification.LoadFromDataSource();
        }

        private void AddEmptyRowCompetency()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtCompetency.Rows.Count == 0)
                {
                    dtCompetency.Rows.Add(1);
                    RowValue = dtCompetency.Rows.Count;
                    dtCompetency.SetValue(cmIsnew.DataBind.Alias, RowValue - 1, "Y");
                    dtCompetency.SetValue(cmId.DataBind.Alias, RowValue - 1, "0");
                    dtCompetency.SetValue(cmCompetency.DataBind.Alias, RowValue - 1, "");
                    dtCompetency.SetValue(cmDescription.DataBind.Alias, RowValue - 1, "");
                    dtCompetency.SetValue(cmRemarks.DataBind.Alias, RowValue - 1, "");
                    //dtCompetency.SetValue(cmSerial.DataBind.Alias, RowValue - 1, RowValue);
                    mtCompetency.AddRow(1, 0);
                }
                else
                {
                    if (dtCompetency.GetValue(cmCompetency.DataBind.Alias, dtCompetency.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtCompetency.Rows.Add(1);
                        RowValue = dtCompetency.Rows.Count;
                        dtCompetency.SetValue(cmIsnew.DataBind.Alias, RowValue - 1, "Y");
                        dtCompetency.SetValue(cmId.DataBind.Alias, RowValue - 1, "0");
                        dtCompetency.SetValue(cmCompetency.DataBind.Alias, RowValue - 1, "");
                        dtCompetency.SetValue(cmDescription.DataBind.Alias, RowValue - 1, "");
                        dtCompetency.SetValue(cmRemarks.DataBind.Alias, RowValue - 1, "");
                        //dtCompetency.SetValue(cmSerial.DataBind.Alias, RowValue - 1, RowValue);
                        mtCompetency.AddRow(1, RowValue);
                    }
                }
                mtCompetency.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("AddEmptyRow Competency: {0}", Ex.Message);
            }
        }

        private void AddEmptyRowSkill()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtSkills.Rows.Count == 0)
                {
                    dtSkills.Rows.Add(1);
                    RowValue = dtSkills.Rows.Count;
                    dtSkills.SetValue(skIsnew.DataBind.Alias, RowValue - 1, "Y");
                    dtSkills.SetValue(skId.DataBind.Alias, RowValue - 1, "0");
                    dtSkills.SetValue(skSkill.DataBind.Alias, RowValue - 1, "");
                    dtSkills.SetValue(skDescription.DataBind.Alias, RowValue - 1, "");
                    dtSkills.SetValue(skRemarks.DataBind.Alias, RowValue - 1, "");
                    dtSkills.SetValue(skPriorty.DataBind.Alias, RowValue - 1, RowValue);
                    //dtSkills.SetValue(cmSerial.DataBind.Alias, RowValue - 1, RowValue);
                    mtSkills.AddRow(1, 0);
                }
                else
                {
                    if (dtSkills.GetValue(skSkill.DataBind.Alias, dtSkills.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtSkills.Rows.Add(1);
                        RowValue = dtSkills.Rows.Count;
                        dtSkills.SetValue(skIsnew.DataBind.Alias, RowValue - 1, "Y");
                        dtSkills.SetValue(skId.DataBind.Alias, RowValue - 1, "0");
                        dtSkills.SetValue(skSkill.DataBind.Alias, RowValue - 1, "");
                        dtSkills.SetValue(skDescription.DataBind.Alias, RowValue - 1, "");
                        dtSkills.SetValue(skRemarks.DataBind.Alias, RowValue - 1, "");
                        dtSkills.SetValue(skPriorty.DataBind.Alias, RowValue - 1, RowValue);
                        //dtSkills.SetValue(cmSerial.DataBind.Alias, RowValue - 1, RowValue);
                        mtSkills.AddRow(1, RowValue);
                    }
                }
                mtSkills.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("AddEmptyRow Skill: {0}", Ex.Message);
            }
        }

        private void FillDepartmentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            IEnumerable<MstDepartment> AllDepartment = from a in dbHrPayroll.MstDepartment select a;
            pCombo.ValidValues.Add("-1", ""); 
            foreach (MstDepartment Dept in AllDepartment)
            {
                pCombo.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void FillBranchCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstBranches> AllBranches = from a in dbHrPayroll.MstBranches select a;
                pCombo.ValidValues.Add("-1", ""); 
                foreach (MstBranches Branch in AllBranches)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Branch.Id), Convert.ToString(Branch.Name));
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

        private void FillDesignationCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation select a;
                pCombo.ValidValues.Add("-1", ""); 
                foreach (MstDesignation Desig in Designations)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Desig.Id), Convert.ToString(Desig.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillContractType(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> LOVS = from a in dbHrPayroll.MstLOVE where a.Type.Contains("HREMPType") select a;
                pCombo.ValidValues.Add("-1", ""); 
                foreach (MstLOVE Lov in LOVS)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Lov.Code), Convert.ToString(Lov.Value));
                    pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillExperianceUnit(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> LOVS = from a in dbHrPayroll.MstLOVE where a.Type.Contains("EXPUnit") select a;
                pCombo.ValidValues.Add("-1", ""); 
                foreach (MstLOVE Lov in LOVS)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Lov.Code), Convert.ToString(Lov.Value));
                    pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillCompetencyInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                //IEnumerable<MstCompetancy> Competencies = from a in dbHrPayroll.MstCompetancy select a;
                //OneColumn.ValidValues.Add("-1", ""); 
                //foreach (MstCompetancy Competency in Competencies)
                //{
                //    OneColumn.ValidValues.Add(Convert.ToString(Competency.ID), Convert.ToString(Competency.Code));
                //}
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillSkillInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstSkills> Skills = from a in dbHrPayroll.MstSkills select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (MstSkills Skill in Skills)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Skill.ID), Convert.ToString(Skill.Code));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillEducationInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstQualification> Qualifications = from a in dbHrPayroll.MstQualification select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (MstQualification Qualification in Qualifications)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Qualification.Id), Convert.ToString(Qualification.Code));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillCertificationInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstCertification> Certifications = from a in dbHrPayroll.MstCertification select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (MstCertification Certification in Certifications)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Certification.Id), Convert.ToString(Certification.Name));
                }
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

        private void FillBudgetDocumentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<TrnsHeadBudget> DocCollection = from a in dbHrPayroll.TrnsHeadBudget select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (TrnsHeadBudget Document in DocCollection)
                {
                    String Description = "";
                    Description = Document.DocNum.ToString() + " : " + Document.Description; 
                    pCombo.ValidValues.Add(Convert.ToString(Document.ID), Description);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        #endregion
    }
}
