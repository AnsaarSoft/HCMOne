using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_PayrollIni : HRMSBaseForm
    {

        #region Variables

        SAPbouiCOM.EditText txDOM, txHPD, txPFVal, txSSVal, txSSMC, txSboUID, txSboPwd, txPatchNum;
        SAPbouiCOM.CheckBox chBI, chAtt, chPBAtt, chETS, chkSSLAttendance, chPF, chSS, chkArabicContent, chkCCWiseGL, chkAbs, chkmb, chkempf, chkAutoNumber, chkMultiDimension, chkA1Integration, chkUnitProcessing, chkProjectBaseJE, chkRetail, chkSalaryProcessingOnAttendance, chkSalaryClassification, chkFCJE, chkLateInEarlyOutLeaveRules, chkEnableLeaveCalendar;
        SAPbouiCOM.ComboBox cbPFPer, cbSSB;
        SAPbouiCOM.Item ItxDOM, IchkAbs, ItxHPD, ItxPFVal, ItxSSVal, ItxSSMC, IchBI, IchAtt, IchPBAtt, IchETS, IchEGra, IchPF, IchSS, IcbPFPer, IcbSSB, ItxSboUID, ItxSboPwd, ItxPatchNum, IchkArabicContent, IchkCCWiseGL, Ichkmb, Ichkempf, IchkAutoNumber, IchkMultiDimension, IchkA1Integration, IchkUnitProcessing, IchkSSLAttendance, IchkProjectBaseJE, ichkRetail, ichkSalaryProcessingOnAttendance, IchkSalaryClassification, ichkFCJE, IchkLateInEarlyOutLeaveRules, IchkEnableLeaveCalendar;

        public IEnumerable<MstCompany> payrollInfo;

        #endregion 

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            oForm.Freeze(true);
            _fillFields();
            oForm.Freeze(false);

        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    submitForm();
                    BubbleEvent = false;
                    break;
                case "btPatch":
                    //Program.objHrmsUI.applyPatch(txPatchNum.Value.Trim());
                    break;
            }
        }

        #endregion 

        #region Functions

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            //EachItemshould be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the controll object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */

            oForm.DefButton = "1";
            oForm.DataSources.UserDataSources.Add("txDOM", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Days of Month
            txDOM = oForm.Items.Item("txDOM").Specific;
            ItxDOM = oForm.Items.Item("txDOM");
            txDOM.DataBind.SetBound(true, "", "txDOM");

            oForm.DataSources.UserDataSources.Add("txHPD", SAPbouiCOM.BoDataType.dt_SUM); // Hours Per Day
            txHPD = oForm.Items.Item("txHPD").Specific;
            ItxHPD = oForm.Items.Item("txHPD");
            txHPD.DataBind.SetBound(true, "", "txHPD");

            oForm.DataSources.UserDataSources.Add("chkAbs", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
            chkAbs = oForm.Items.Item("chkAbs").Specific;
            IchkAbs = oForm.Items.Item("chkAbs");
            chkAbs.DataBind.SetBound(true, "", "chkAbs");

            oForm.DataSources.UserDataSources.Add("chBI", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
            chBI = oForm.Items.Item("chBI").Specific;
            IchBI = oForm.Items.Item("chBI");
            chBI.DataBind.SetBound(true, "", "chBI");

            oForm.DataSources.UserDataSources.Add("chAtt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Attandance
            chAtt = oForm.Items.Item("chAtt").Specific;
            IchAtt = oForm.Items.Item("chAtt");
            chAtt.DataBind.SetBound(true, "", "chAtt");

            oForm.DataSources.UserDataSources.Add("chPBAtt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Project Based Attandance
            chPBAtt = oForm.Items.Item("chPBAtt").Specific;
            IchPBAtt = oForm.Items.Item("chPBAtt");
            chPBAtt.DataBind.SetBound(true, "", "chPBAtt");

            oForm.DataSources.UserDataSources.Add("chETS", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chETS = oForm.Items.Item("chETS").Specific;
            IchETS = oForm.Items.Item("chETS");
            chETS.DataBind.SetBound(true, "", "chETS");

            //oForm.DataSources.UserDataSources.Add("chEGra", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable Gratuity
            //chEGra = oForm.Items.Item("chEGra").Specific;
            //IchEGra = oForm.Items.Item("chEGra");
            //chEGra.DataBind.SetBound(true, "", "chEGra");

            oForm.DataSources.UserDataSources.Add("chkarabic", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable Arabic Content
            chkArabicContent = oForm.Items.Item("chkarabic").Specific;
            IchkArabicContent = oForm.Items.Item("chkarabic");
            chkArabicContent.DataBind.SetBound(true, "", "chkarabic");

            oForm.DataSources.UserDataSources.Add("chkcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable costcenter Content
            chkCCWiseGL = oForm.Items.Item("chkcc").Specific;
            IchkCCWiseGL = oForm.Items.Item("chkcc");
            chkCCWiseGL.DataBind.SetBound(true, "", "chkcc");

            //Branches Integeration (AR)
            oForm.DataSources.UserDataSources.Add("chkmb", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable branches Content
            chkmb = oForm.Items.Item("chkmb").Specific;
            Ichkmb = oForm.Items.Item("chkmb");
            chkmb.DataBind.SetBound(true, "", "chkmb");

            //Employee Filter User Wise (AR)
            oForm.DataSources.UserDataSources.Add("chkempf", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable employee filrer Content
            chkempf = oForm.Items.Item("chkempf").Specific;
            Ichkempf = oForm.Items.Item("chkempf");
            chkempf.DataBind.SetBound(true, "", "chkempf");

            oForm.DataSources.UserDataSources.Add("chAuto", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkAutoNumber = oForm.Items.Item("chAuto").Specific;
            IchkAutoNumber = oForm.Items.Item("chAuto");
            chkAutoNumber.DataBind.SetBound(true, "", "chAuto");

            oForm.DataSources.UserDataSources.Add("chmd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkMultiDimension = oForm.Items.Item("chmd").Specific;
            IchkMultiDimension = oForm.Items.Item("chmd");
            chkMultiDimension.DataBind.SetBound(true, "", "chmd");

            oForm.DataSources.UserDataSources.Add("cha1int", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkA1Integration = oForm.Items.Item("cha1int").Specific;
            IchkA1Integration = oForm.Items.Item("cha1int");
            chkA1Integration.DataBind.SetBound(true, "", "cha1int");

            oForm.DataSources.UserDataSources.Add("chunits", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkUnitProcessing = oForm.Items.Item("chunits").Specific;
            IchkUnitProcessing = oForm.Items.Item("chunits");
            chkUnitProcessing.DataBind.SetBound(true, "", "chunits");

            oForm.DataSources.UserDataSources.Add("chsslatt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkSSLAttendance = oForm.Items.Item("chsslatt").Specific;
            IchkSSLAttendance = oForm.Items.Item("chsslatt");
            chkSSLAttendance.DataBind.SetBound(true, "", "chsslatt");


            oForm.DataSources.UserDataSources.Add("chJEP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkProjectBaseJE = oForm.Items.Item("chJEP").Specific;
            IchkProjectBaseJE = oForm.Items.Item("chJEP");
            chkProjectBaseJE.DataBind.SetBound(true, "", "chJEP");

            //oForm.DataSources.UserDataSources.Add("chretail", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            //chkRetail = oForm.Items.Item("chretail").Specific;
            //ichkRetail = oForm.Items.Item("chretail");
            //chkRetail.DataBind.SetBound(true, "", "chretail");

            oForm.DataSources.UserDataSources.Add("chlateLves", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkLateInEarlyOutLeaveRules = oForm.Items.Item("chlateLves").Specific;
            IchkLateInEarlyOutLeaveRules = oForm.Items.Item("chlateLves");
            chkLateInEarlyOutLeaveRules.DataBind.SetBound(true, "", "chlateLves");

            oForm.DataSources.UserDataSources.Add("chspa", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkSalaryProcessingOnAttendance = oForm.Items.Item("chspa").Specific;
            ichkSalaryProcessingOnAttendance = oForm.Items.Item("chspa");
            chkSalaryProcessingOnAttendance.DataBind.SetBound(true, "", "chspa");

            oForm.DataSources.UserDataSources.Add("chfcje", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkFCJE = oForm.Items.Item("chfcje").Specific;
            ichkFCJE = oForm.Items.Item("chfcje");
            chkFCJE.DataBind.SetBound(true, "", "chfcje");

            oForm.DataSources.UserDataSources.Add("28", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkSalaryClassification = oForm.Items.Item("28").Specific;
            IchkSalaryClassification = oForm.Items.Item("28");
            chkSalaryClassification.DataBind.SetBound(true, "", "28");

            oForm.DataSources.UserDataSources.Add("chLvecal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Enable auto number Content
            chkEnableLeaveCalendar = oForm.Items.Item("chLvecal").Specific;
            IchkEnableLeaveCalendar = oForm.Items.Item("chLvecal");
            chkEnableLeaveCalendar.DataBind.SetBound(true, "", "chLvecal");

            //oForm.DataSources.UserDataSources.Add("txPFVal", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Hours Per Day
            //txPFVal = oForm.Items.Item("txPFVal").Specific;
            //ItxPFVal = oForm.Items.Item("txPFVal");
            //txPFVal.DataBind.SetBound(true, "", "txPFVal");

            //oForm.DataSources.UserDataSources.Add("cbPFPer", SAPbouiCOM.BoDataType.dt_SHORT_TEXT); // PF Period
            //cbPFPer = oForm.Items.Item("cbPFPer").Specific;
            //IcbPFPer = oForm.Items.Item("cbPFPer");
            //cbPFPer.DataBind.SetBound(true, "", "cbPFPer");

            //oForm.DataSources.UserDataSources.Add("chSS", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); //Enable Social Security
            //chSS = oForm.Items.Item("chSS").Specific;
            //IchSS = oForm.Items.Item("chSS");
            //chSS.DataBind.SetBound(true, "", "chSS");

            //oForm.DataSources.UserDataSources.Add("cbSSB", SAPbouiCOM.BoDataType.dt_SHORT_TEXT); // Days of Month
            //cbSSB = oForm.Items.Item("cbSSB").Specific;
            //IcbSSB = oForm.Items.Item("cbSSB");
            //cbSSB.DataBind.SetBound(true, "", "cbSSB");

            oForm.DataSources.UserDataSources.Add("txSboUID", SAPbouiCOM.BoDataType.dt_SHORT_TEXT); // Days of Month
            txSboUID = oForm.Items.Item("txSboUID").Specific;
            ItxSboUID = oForm.Items.Item("txSboUID");
            txSboUID.DataBind.SetBound(true, "", "txSboUID");

            oForm.DataSources.UserDataSources.Add("txSboPwd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT); // Days of Month
            txSboPwd = oForm.Items.Item("txSboPwd").Specific;
            ItxSboPwd = oForm.Items.Item("txSboPwd");
            txSboPwd.DataBind.SetBound(true, "", "txSboPwd");

            //oForm.DataSources.UserDataSources.Add("txSSVal", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            //txSSVal = oForm.Items.Item("txSSVal").Specific;
            //ItxSSVal = oForm.Items.Item("txSSVal");
            //txSSVal.DataBind.SetBound(true, "", "txSSVal");

            //oForm.DataSources.UserDataSources.Add("txSSMC", SAPbouiCOM.BoDataType.dt_SUM); // Hours Per Day
            //txSSMC = oForm.Items.Item("txSSMC").Specific;
            //ItxSSMC = oForm.Items.Item("txSSMC");
            //txSSMC.DataBind.SetBound(true, "", "txSSMC");

            //oForm.DataSources.UserDataSources.Add("txPatchNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 10); // Hours Per Day
            //txPatchNum = oForm.Items.Item("txPatchNum").Specific;
            //ItxPatchNum = oForm.Items.Item("txPatchNum");
            //txPatchNum.DataBind.SetBound(true, "", "txPatchNum");

            //fillCombo("Val_Type", cbSSB);


            oForm.Freeze(false);


        }

        private bool submitForm()
        {
            bool submitResult = true;

            try
            {
                int confirm = oApplication.MessageBox("Changes in the Payroll Initialization may impact payroll processing. Are you sure you want to make changes?", 1, "Yes", "No");
                if (confirm == 2) return false;
                CfgPayrollBasicInitialization payrollIni;
                int cnt = (from p in dbHrPayroll.CfgPayrollBasicInitialization where p.Id == 1 select p).Count();
                if (cnt > 0)
                {
                    payrollIni = (from p in dbHrPayroll.CfgPayrollBasicInitialization where p.Id == 1 select p).Single();
                }
                else
                {
                    payrollIni = new CfgPayrollBasicInitialization();
                    dbHrPayroll.CfgPayrollBasicInitialization.InsertOnSubmit(payrollIni);

                }
                payrollIni.AttendanceSystem = chAtt.Checked;
                payrollIni.SAPB1Integration = chBI.Checked;
                payrollIni.FlgAbsent = chkAbs.Checked;
                //payrollIni.Gratuity = chEGra.Checked;
                payrollIni.FlgArabic = chkArabicContent.Checked;
                payrollIni.FlgCostCenterGL = chkCCWiseGL.Checked;
                payrollIni.TaxSetup = chETS.Checked;
                payrollIni.ProjectBased = chPBAtt.Checked;
                //payrollIni.ProvidentFund = chPF.Checked;
                payrollIni.FlgAutoNumber = chkAutoNumber.Checked;
                payrollIni.FlgBranches = chkmb.Checked;
                payrollIni.FlgEmployeeFilter = chkempf.Checked;
                payrollIni.FlgMultipleDimension = chkMultiDimension.Checked;
                payrollIni.FlgA1Integration = chkA1Integration.Checked;
                payrollIni.FlgUnitFeature = chkUnitProcessing.Checked;
                payrollIni.FlgSSL = chkSSLAttendance.Checked;
                payrollIni.FlgProject = chkProjectBaseJE.Checked;
                payrollIni.FlgProcessingOnAttendance = chkSalaryProcessingOnAttendance.Checked;
                //payrollIni.FlgRetailRules1 = chkRetail.Checked;
                payrollIni.FlgLateInEarlyOutLeaveRules = chkLateInEarlyOutLeaveRules.Checked;
                payrollIni.FlgJELocationWise = chkSalaryClassification.Checked;
                payrollIni.FlgLeaveCalendar = chkEnableLeaveCalendar.Checked;
                payrollIni.FlgJECurrency = chkFCJE.Checked;
                payrollIni.WorkingDays = txDOM.Value == "" ? (short)0 : Convert.ToInt16(txDOM.Value);
                payrollIni.WorkingHours = txHPD.Value == "" ? (decimal)0 : Convert.ToDecimal(txHPD.Value);
                //payrollIni.EligibilityValue = txPFVal.Value == "" ? (short)0 : Convert.ToInt16(txPFVal.Value);
                //payrollIni.SSMaxContribution = txSSMC.Value == "" ? (decimal)0 : Convert.ToDecimal(txSSMC.Value);
                //payrollIni.SSValue = txSSVal.Value == "" ? (decimal)0 : Convert.ToDecimal(txSSVal.Value);
                //payrollIni.SSBasis = cbSSB.Value.Trim();
                //payrollIni.CompanyName = "";
                payrollIni.SboUID = txSboUID.Value.Trim();
                payrollIni.SboPwd = txSboPwd.Value.Trim();
                dbHrPayroll.SubmitChanges();

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
                oForm.AutoManaged = false;
                submitResult = false;
            }
            oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Medium, false);
            return submitResult;
        }

        private void _fillFields()
        {
            try
            {
                int cnt = (from p in dbHrPayroll.CfgPayrollBasicInitialization where p.Id == 1 select p).Count();
                if (cnt > 0)
                {
                    CfgPayrollBasicInitialization payrollIni = (from p in dbHrPayroll.CfgPayrollBasicInitialization where p.Id == 1 select p).Single();
                    chAtt.Checked = payrollIni.AttendanceSystem == null ? false : (bool)payrollIni.AttendanceSystem;
                    chBI.Checked = payrollIni.SAPB1Integration == null ? false : (bool)payrollIni.SAPB1Integration;
                    //chEGra.Checked = payrollIni.Gratuity == null ? false : (bool)payrollIni.Gratuity;
                    chkArabicContent.Checked = payrollIni.FlgArabic == null ? false : (bool)payrollIni.FlgArabic;
                    chkCCWiseGL.Checked = payrollIni.FlgCostCenterGL == null ? false : (bool)payrollIni.FlgCostCenterGL;
                    chETS.Checked = payrollIni.TaxSetup == null ? false : (bool)payrollIni.TaxSetup;
                    chPBAtt.Checked = payrollIni.ProjectBased == null ? false : (bool)payrollIni.ProjectBased;
                    //chPF.Checked = payrollIni.ProvidentFund == null ? false : (bool)payrollIni.ProvidentFund;
                    chkAbs.Checked = payrollIni.FlgAbsent == null ? false : (bool)payrollIni.FlgAbsent;
                    chkAutoNumber.Checked = payrollIni.FlgAutoNumber == null ? false : (bool)payrollIni.FlgAutoNumber;
                    chkMultiDimension.Checked = payrollIni.FlgMultipleDimension == null ? false : (bool)payrollIni.FlgMultipleDimension;
                    chkA1Integration.Checked = payrollIni.FlgA1Integration == null ? false : (bool)payrollIni.FlgA1Integration;
                    chkUnitProcessing.Checked = payrollIni.FlgUnitFeature == null ? false : (bool)payrollIni.FlgUnitFeature;
                    chkSSLAttendance.Checked = payrollIni.FlgSSL == null ? false : (bool)payrollIni.FlgSSL;
                   // chkRetail.Checked = payrollIni.FlgRetailRules1 == null ? false : (bool)payrollIni.FlgRetailRules1;
                    chkLateInEarlyOutLeaveRules.Checked = payrollIni.FlgLateInEarlyOutLeaveRules == null ? false : (bool)payrollIni.FlgLateInEarlyOutLeaveRules;
                    chkSalaryProcessingOnAttendance.Checked = payrollIni.FlgProcessingOnAttendance == null ? false : (bool)payrollIni.FlgProcessingOnAttendance;
                    chkSalaryClassification.Checked = payrollIni.FlgJELocationWise == null ? false : Convert.ToBoolean(payrollIni.FlgJELocationWise);
                    chkEnableLeaveCalendar.Checked = payrollIni.FlgLeaveCalendar == null ? false : Convert.ToBoolean(payrollIni.FlgLeaveCalendar);
                    chkFCJE.Checked = payrollIni.FlgJECurrency == null ? false : Convert.ToBoolean(payrollIni.FlgJECurrency);
                    //Branches Integeration (AR)

                    chkmb.Checked = payrollIni.FlgBranches == null ? false : (bool)payrollIni.FlgBranches;

                    //Employee Filter User Wise (AR)

                    chkempf.Checked = payrollIni.FlgEmployeeFilter == null ? false : (bool)payrollIni.FlgEmployeeFilter;
                    chkProjectBaseJE.Checked = payrollIni.FlgProject == null ? false : (bool)payrollIni.FlgProject;


                    //chSS.Checked = payrollIni.SSCompany == null ? false : (bool)payrollIni.SSCompany;
                    //txDOM.Value = payrollIni.WorkingDays == null ? "" : payrollIni.WorkingDays.ToString();
                    //txHPD.Value = payrollIni.WorkingHours == null ? "" : payrollIni.WorkingHours.ToString();
                    //txPFVal.Value = payrollIni.EligibilityValue == null ? "" : payrollIni.EligibilityValue.ToString();
                    //txSSMC.Value = payrollIni.SSMaxContribution == null ? "" : payrollIni.SSMaxContribution.ToString();
                    //txSSVal.Value = payrollIni.SSValue == null ? "" : payrollIni.SSValue.ToString();
                    txSboUID.Value = payrollIni.SboUID == null ? "" : payrollIni.SboUID.ToString();
                    txSboPwd.Value = payrollIni.SboPwd == null ? "" : payrollIni.SboPwd.ToString();
                    //try
                    //{
                    //    cbSSB.Select(payrollIni.SSBasis.ToString());
                    //}
                    //catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }

            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        }

        #endregion

    }

}
