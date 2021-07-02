using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_EleSetup : HRMSBaseForm
    {
        //chdntax
        #region Variables

        SAPbouiCOM.EditText txtCode, txtName, txtDesc, txtEffectiveDate, txEndDate, txErVal, txDeVal, txCnEmprCo, txCnEmpCo, txInVal, txCnNTA, txmxant, txMxEmly, txMxEmlr, txtSalaryRangeFrom, txtSalaryRangeTo, txtDaysFrom, txtDaysTo;
        SAPbouiCOM.CheckBox chPrInPrl, chStdEle, flgGosi, chErLE, chErEos, chErNT, chErVar, chErMltp, chErAddEnt, chDeMulEn, chDeAddEn, chInTxble, chOnGross, chkDeductionEos, chkContributionEos, chkProbation, chkVCo, chkProportionEar, chkProportionDed, chkVGross, chBtch, chkAttendanceAllowance, chkRemainingAmount, chkEmployeeBonus, chkShiftDays, chkGradeDependent, chkDedNonTax;
        SAPbouiCOM.ComboBox cbRecType, cbPriClass, cbErVT, cbDeVt, cbCnType, cmbValidOn;
        SAPbouiCOM.Button btDeFb, btCnFB;

        SAPbouiCOM.Item ItxCode, ItxName, ItxDescr, ItxEfctDate, ItxEndDate, ItxErVal, ItxDeVal, ItxCnEmprCo, ItxCnEmpCo, ItxInVal, ItxCnNTA, Itxmxant, ItxMxEmlr, ItxMxEmly, ItxtSalaryRangeFrom, ItxtSalaryRangeTo, ItxtDaysFrom, ItxtDaysTo, ichkShiftDays, ichkGradeDependent, IchkAttendanceAllowance, IchkRemainingAmount, IchkEmployeeBonus;
        SAPbouiCOM.Item IchPrInPrl, IchStdEle, IchErLE, IchErEos, IchErNT, IchErVar, IchErMltp, IchErAddEnt, IchDeMulEn, IchDeAddEn, IchInTxble, IchkDeductionEos, IchkContributionEos, IchkProbation, IchkVCo, IchkProportionEar, IchkProportionDed, IchkVGross, ichkDedNonTax;
        SAPbouiCOM.Item IcbRecType, IcbPriClass, IcbErVT, IcbDeVt, IcbCnType, IchOnGross, IchBtch, IcmbValidOn;
        SAPbouiCOM.Item IbtDeFb, IbtCnFB, IflgGosi;

        public IEnumerable<MstElements> PayrollElement;

        Int32 loadedElement = 0;

        #endregion 

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":

                    doSubmit();


                    break;
                case "cmdNew":
                    addNew();
                    break;
                case "cmdNext":
                    getNextRecord();
                    break;
                case "cmdPrev":
                    getPreviouRecord();
                    break;

            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        if (!ValidateRecord())
                        {
                            BubbleEvent = false;
                        }
                    }
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                    {
                        if (!ValidateRecordUpdate())
                        {
                            BubbleEvent = false;
                        }
                    }
                    break;

            }
        }
        
        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "cbPriClass")
            {
                switch (cbPriClass.Value.Trim())
                {
                    case "Ear":
                        oForm.PaneLevel = 1;
                        break;
                    case "Ded":
                        oForm.PaneLevel = 2;
                        break;
                    case "Con":
                        oForm.PaneLevel = 3;
                        break;
                    case "Inf":
                        oForm.PaneLevel = 4;
                        break;
                }
            }

        }

        public override void fillFields()
        {
            base.fillFields();
            _fillFields();
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            addNew();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            iniControlls();
            oForm.Items.Item("fdEarn").Visible = false;
            oForm.Items.Item("fdDed").Visible = false;
            oForm.Items.Item("fdCont").Visible = false;
            oForm.Items.Item("fdInfo").Visible = false;
            doSubmit();
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {

                oForm.DefButton = "1";

                oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txtCode = oForm.Items.Item("txCode").Specific;
                ItxCode = oForm.Items.Item("txCode");
                txtCode.DataBind.SetBound(true, "", "txCode");

                oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
                txtName = oForm.Items.Item("txName").Specific;
                ItxName = oForm.Items.Item("txName");
                txtName.DataBind.SetBound(true, "", "txName");

                oForm.DataSources.UserDataSources.Add("txDescr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // SBO Intigration
                txtDesc = oForm.Items.Item("txDescr").Specific;
                ItxDescr = oForm.Items.Item("txDescr");
                txtDesc.DataBind.SetBound(true, "", "txDescr");

                oForm.DataSources.UserDataSources.Add("txEfctDate", SAPbouiCOM.BoDataType.dt_DATE); // Attandance
                txtEffectiveDate = oForm.Items.Item("txEfctDate").Specific;
                ItxEfctDate = oForm.Items.Item("txEfctDate");
                txtEffectiveDate.DataBind.SetBound(true, "", "txEfctDate");

                oForm.DataSources.UserDataSources.Add("txEndDate", SAPbouiCOM.BoDataType.dt_DATE); // Project Based Attandance
                txEndDate = oForm.Items.Item("txEndDate").Specific;
                ItxEndDate = oForm.Items.Item("txEndDate");
                txEndDate.DataBind.SetBound(true, "", "txEndDate");

                oForm.DataSources.UserDataSources.Add("txErVal", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txErVal = oForm.Items.Item("txErVal").Specific;
                ItxErVal = oForm.Items.Item("txErVal");
                txErVal.DataBind.SetBound(true, "", "txErVal");

                oForm.DataSources.UserDataSources.Add("txDeVal", SAPbouiCOM.BoDataType.dt_SUM); // Enable Gratuity
                txDeVal = oForm.Items.Item("txDeVal").Specific;
                ItxDeVal = oForm.Items.Item("txDeVal");
                txDeVal.DataBind.SetBound(true, "", "txDeVal");

                oForm.DataSources.UserDataSources.Add("txCnEmpCo", SAPbouiCOM.BoDataType.dt_SUM); // Enable PF
                txCnEmpCo = oForm.Items.Item("txCnEmpCo").Specific;
                ItxCnEmpCo = oForm.Items.Item("txCnEmpCo");
                txCnEmpCo.DataBind.SetBound(true, "", "txCnEmpCo");

                oForm.DataSources.UserDataSources.Add("txCnEmprCo", SAPbouiCOM.BoDataType.dt_SUM); // Enable PF
                txCnEmprCo = oForm.Items.Item("txCnEmprCo").Specific;
                ItxCnEmprCo = oForm.Items.Item("txCnEmprCo");
                txCnEmprCo.DataBind.SetBound(true, "", "txCnEmprCo");

                oForm.DataSources.UserDataSources.Add("txmxant", SAPbouiCOM.BoDataType.dt_SUM);
                txmxant = oForm.Items.Item("txmxant").Specific;
                Itxmxant = oForm.Items.Item("txmxant");
                txmxant.DataBind.SetBound(true, "", "txmxant");

                oForm.DataSources.UserDataSources.Add("txMxEmly", SAPbouiCOM.BoDataType.dt_SUM);
                txMxEmly = oForm.Items.Item("txMxEmly").Specific;
                ItxMxEmly = oForm.Items.Item("txMxEmly");
                txMxEmly.DataBind.SetBound(true, "", "txMxEmly");

                oForm.DataSources.UserDataSources.Add("txMxEmlr", SAPbouiCOM.BoDataType.dt_SUM);
                txMxEmlr = oForm.Items.Item("txMxEmlr").Specific;
                ItxMxEmlr = oForm.Items.Item("txMxEmlr");
                txMxEmlr.DataBind.SetBound(true, "", "txMxEmlr");


                oForm.DataSources.UserDataSources.Add("txInVal", SAPbouiCOM.BoDataType.dt_SUM); // Hours Per Day
                txInVal = oForm.Items.Item("txInVal").Specific;
                ItxInVal = oForm.Items.Item("txInVal");
                txInVal.DataBind.SetBound(true, "", "txInVal");

                oForm.DataSources.UserDataSources.Add("txCnNTA", SAPbouiCOM.BoDataType.dt_SUM); // Hours Per Day
                txCnNTA = oForm.Items.Item("txCnNTA").Specific;
                ItxCnNTA = oForm.Items.Item("txCnNTA");
                txCnNTA.DataBind.SetBound(true, "", "txCnNTA");

                oForm.DataSources.UserDataSources.Add("txsfrom", SAPbouiCOM.BoDataType.dt_SUM);
                txtSalaryRangeFrom = oForm.Items.Item("txsfrom").Specific;
                ItxtSalaryRangeFrom = oForm.Items.Item("txsfrom");
                txtSalaryRangeFrom.DataBind.SetBound(true, "", "txsfrom");

                oForm.DataSources.UserDataSources.Add("txsto", SAPbouiCOM.BoDataType.dt_SUM);
                txtSalaryRangeTo = oForm.Items.Item("txsto").Specific;
                ItxtSalaryRangeTo = oForm.Items.Item("txsto");
                txtSalaryRangeTo.DataBind.SetBound(true, "", "txsto");

                oForm.DataSources.UserDataSources.Add("txdfrom", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtDaysFrom = oForm.Items.Item("txdfrom").Specific;
                ItxtDaysFrom = oForm.Items.Item("txdfrom");
                txtDaysFrom.DataBind.SetBound(true, "", "txdfrom");

                oForm.DataSources.UserDataSources.Add("txdto", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER);
                txtDaysTo = oForm.Items.Item("txdto").Specific;
                ItxtDaysTo = oForm.Items.Item("txdto");
                txtDaysTo.DataBind.SetBound(true, "", "txdto");

                oForm.DataSources.UserDataSources.Add("chPrInPrl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // PF Period
                chPrInPrl = oForm.Items.Item("chPrInPrl").Specific;
                IchPrInPrl = oForm.Items.Item("chPrInPrl");
                chPrInPrl.DataBind.SetBound(true, "", "chPrInPrl");

                oForm.DataSources.UserDataSources.Add("chStdEle", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); //Enable Social Security
                chStdEle = oForm.Items.Item("chStdEle").Specific;
                IchStdEle = oForm.Items.Item("chStdEle");
                chStdEle.DataBind.SetBound(true, "", "chStdEle");

                oForm.DataSources.UserDataSources.Add("chErLE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chErLE = oForm.Items.Item("chErLE").Specific;
                IchErLE = oForm.Items.Item("chErLE");
                chErLE.DataBind.SetBound(true, "", "chErLE");

                oForm.DataSources.UserDataSources.Add("flgProb", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkProbation = oForm.Items.Item("flgProb").Specific;
                IchkProbation = oForm.Items.Item("flgProb");
                chkProbation.DataBind.SetBound(true, "", "flgProb");

                oForm.DataSources.UserDataSources.Add("chErEos", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chErEos = oForm.Items.Item("chErEos").Specific;
                IchErEos = oForm.Items.Item("chErEos");
                chErEos.DataBind.SetBound(true, "", "chErEos");

                oForm.DataSources.UserDataSources.Add("chkcEOS", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkContributionEos = oForm.Items.Item("chkcEOS").Specific;
                IchkContributionEos = oForm.Items.Item("chkcEOS");
                chkContributionEos.DataBind.SetBound(true, "", "chkcEOS");

                oForm.DataSources.UserDataSources.Add("chkdEOS", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkDeductionEos = oForm.Items.Item("chkdEOS").Specific;
                IchkDeductionEos = oForm.Items.Item("chkdEOS");
                chkDeductionEos.DataBind.SetBound(true, "", "chkdEOS");

                oForm.DataSources.UserDataSources.Add("chErNT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chErNT = oForm.Items.Item("chErNT").Specific;
                IchErNT = oForm.Items.Item("chErNT");
                chErNT.DataBind.SetBound(true, "", "chErNT");

                oForm.DataSources.UserDataSources.Add("chErVar", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chErVar = oForm.Items.Item("chErVar").Specific;
                IchErVar = oForm.Items.Item("chErVar");
                chErVar.DataBind.SetBound(true, "", "chErVar");

                oForm.DataSources.UserDataSources.Add("chBtch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chBtch = oForm.Items.Item("chBtch").Specific;
                IchBtch = oForm.Items.Item("chBtch");
                chBtch.DataBind.SetBound(true, "", "chBtch");

                oForm.DataSources.UserDataSources.Add("chErMltp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chErMltp = oForm.Items.Item("chErMltp").Specific;
                IchErMltp = oForm.Items.Item("chErMltp");
                chErMltp.DataBind.SetBound(true, "", "chErMltp");

                oForm.DataSources.UserDataSources.Add("chErAddEnt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chErAddEnt = oForm.Items.Item("chErAddEnt").Specific;
                IchErAddEnt = oForm.Items.Item("chErAddEnt");
                chErAddEnt.DataBind.SetBound(true, "", "chErAddEnt");

                oForm.DataSources.UserDataSources.Add("chDeMulEn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chDeMulEn = oForm.Items.Item("chDeMulEn").Specific;
                IchDeMulEn = oForm.Items.Item("chDeMulEn");
                chDeMulEn.DataBind.SetBound(true, "", "chDeMulEn");

                oForm.DataSources.UserDataSources.Add("chDeAddEn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chDeAddEn = oForm.Items.Item("chDeAddEn").Specific;
                IchDeAddEn = oForm.Items.Item("chDeAddEn");
                chDeAddEn.DataBind.SetBound(true, "", "chDeAddEn");

                oForm.DataSources.UserDataSources.Add("chInTxble", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chInTxble = oForm.Items.Item("chInTxble").Specific;
                IchInTxble = oForm.Items.Item("chInTxble");
                chInTxble.DataBind.SetBound(true, "", "chInTxble");

                oForm.DataSources.UserDataSources.Add("chdntax", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkDedNonTax = oForm.Items.Item("chdntax").Specific;
                ichkDedNonTax = oForm.Items.Item("chdntax");
                chkDedNonTax.DataBind.SetBound(true, "", "chdntax");

                oForm.DataSources.UserDataSources.Add("flgGosi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                flgGosi = oForm.Items.Item("flgGosi").Specific;
                IflgGosi = oForm.Items.Item("flgGosi");
                flgGosi.DataBind.SetBound(true, "", "flgGosi");

                oForm.DataSources.UserDataSources.Add("chOnGross", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chOnGross = oForm.Items.Item("chOnGross").Specific;
                IchOnGross = oForm.Items.Item("chOnGross");
                chOnGross.DataBind.SetBound(true, "", "chOnGross");

                oForm.DataSources.UserDataSources.Add("chkproEar", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkProportionEar = oForm.Items.Item("chkproEar").Specific;
                IchkProportionEar = oForm.Items.Item("chkproEar");
                chkProportionEar.DataBind.SetBound(true, "", "chkproEar");

                oForm.DataSources.UserDataSources.Add("chkProDed", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkProportionDed = oForm.Items.Item("chkProDed").Specific;
                IchkProportionDed = oForm.Items.Item("chkProDed");
                chkProportionDed.DataBind.SetBound(true, "", "chkProDed");

                oForm.DataSources.UserDataSources.Add("chkVGross", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
                chkVGross = oForm.Items.Item("chkVGross").Specific;
                IchkVGross = oForm.Items.Item("chkVGross");
                chkVGross.DataBind.SetBound(true, "", "chkVGross");

                oForm.DataSources.UserDataSources.Add("cbRecType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cbRecType = oForm.Items.Item("cbRecType").Specific;
                IcbRecType = oForm.Items.Item("cbRecType");
                cbRecType.DataBind.SetBound(true, "", "cbRecType");

                oForm.DataSources.UserDataSources.Add("cbPriClass", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cbPriClass = oForm.Items.Item("cbPriClass").Specific;
                IcbPriClass = oForm.Items.Item("cbPriClass");
                cbPriClass.DataBind.SetBound(true, "", "cbPriClass");

                oForm.DataSources.UserDataSources.Add("cbErVT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cbErVT = oForm.Items.Item("cbErVT").Specific;
                IcbErVT = oForm.Items.Item("cbErVT");
                cbErVT.DataBind.SetBound(true, "", "cbErVT");

                oForm.DataSources.UserDataSources.Add("cbDeVt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cbDeVt = oForm.Items.Item("cbDeVt").Specific;
                IcbDeVt = oForm.Items.Item("cbDeVt");
                cbDeVt.DataBind.SetBound(true, "", "cbDeVt");

                oForm.DataSources.UserDataSources.Add("cbvalidon", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cmbValidOn = oForm.Items.Item("cbvalidon").Specific;
                IcmbValidOn = oForm.Items.Item("cbvalidon");
                cmbValidOn.DataBind.SetBound(true, "", "cbvalidon");

                oForm.DataSources.UserDataSources.Add("chkVCo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkVCo = oForm.Items.Item("chkVCo").Specific;
                IchkVCo = oForm.Items.Item("chkVCo");
                chkVCo.DataBind.SetBound(true, "", "chkVCo");

                oForm.DataSources.UserDataSources.Add("chsd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkShiftDays = oForm.Items.Item("chsd").Specific;
                ichkShiftDays = oForm.Items.Item("chsd");
                chkShiftDays.DataBind.SetBound(true, "", "chsd");

                oForm.DataSources.UserDataSources.Add("chgd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkGradeDependent = oForm.Items.Item("chgd").Specific;
                ichkGradeDependent = oForm.Items.Item("chgd");
                chkGradeDependent.DataBind.SetBound(true, "", "chgd");

                oForm.DataSources.UserDataSources.Add("cbCnType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // SBO Intigration
                cbCnType = oForm.Items.Item("cbCnType").Specific;
                IcbCnType = oForm.Items.Item("cbCnType");
                cbCnType.DataBind.SetBound(true, "", "cbCnType");

                oForm.DataSources.UserDataSources.Add("chAttAllow", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkAttendanceAllowance = oForm.Items.Item("chAttAllow").Specific;
                IchkAttendanceAllowance = oForm.Items.Item("chAttAllow");
                chkAttendanceAllowance.DataBind.SetBound(true, "", "chAttAllow");

                oForm.DataSources.UserDataSources.Add("chRemnAmt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkRemainingAmount = oForm.Items.Item("chRemnAmt").Specific;
                IchkRemainingAmount = oForm.Items.Item("chRemnAmt");
                chkRemainingAmount.DataBind.SetBound(true, "", "chRemnAmt");

                oForm.DataSources.UserDataSources.Add("chEmpBonus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // SBO Intigration
                chkEmployeeBonus = oForm.Items.Item("chEmpBonus").Specific;
                IchkEmployeeBonus = oForm.Items.Item("chEmpBonus");
                chkEmployeeBonus.DataBind.SetBound(true, "", "chEmpBonus");
                //chEmpBonus
                btDeFb = oForm.Items.Item("btDeFb").Specific;
                IbtDeFb = oForm.Items.Item("btDeFb");

                btCnFB = oForm.Items.Item("btCnFB").Specific;
                IbtCnFB = oForm.Items.Item("btCnFB");

                fillCombo("Val_Type", cbErVT);
                fillCombo("Val_Type", cbDeVt);
                fillCombo("Val_Type", cbCnType);
                fillCombo("Ele_Type", cbPriClass);
                fillCombo("Ele_Cat", cbRecType);
                fillCombo("gratBasedOn", cmbValidOn);

                getData();

                oForm.PaneLevel = 1;
            }
            catch(Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void getData()
        {
            CodeIndex.Clear();
            PayrollElement = from a in dbHrPayroll.MstElements select a;
            var oCollection = (from a in dbHrPayroll.MstElements select a).ToList();
            int i = 0;
            foreach (var One in oCollection)
            {
                CodeIndex.Add(One.Id.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void addNew()
        {
            iniControlls();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
        }
        
        private void iniControlls()
        {
            loadedElement = 0;
            ItxName.Enabled = true;
            txtCode.Value = "";
            txtName.Value = "";
            txtDesc.Value = "";
            cbPriClass.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            cbRecType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            oForm.DataSources.UserDataSources.Item("chPrInPrl").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chStdEle").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chErAddEnt").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chErLE").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chErVar").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chErEos").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chkdEOS").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chkcEOS").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chErMltp").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chErNT").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chkVCo").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("flgGosi").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chBtch").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chsd").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chgd").ValueEx = "N";
            txErVal.Value = "0.00";
            oForm.DataSources.UserDataSources.Item("chDeAddEn").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chDeMulEn").ValueEx = "N";
            txDeVal.Value = "0.00";
            txCnEmpCo.Value = "0.00";
            txCnEmprCo.Value = "0.00";
            txInVal.Value = "0.00";
            txtName.Active = true;
            oForm.DataSources.UserDataSources.Item("chInTxble").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chAttAllow").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chRemnAmt").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chEmpBonus").ValueEx = "N";


        }
        
        private bool ValidateData1()
        {
            bool retValue = true;
            string errMessage = "";
            if (txtName.Value == "")
            {
                retValue = false;
                errMessage += "Name of Element Required !";
            }
            if (txtEffectiveDate.Value == "")
            {
                retValue = false;
                errMessage += "Effective Date Required !";
            }
            if (cbPriClass.Value.Trim() == "Ear")
            {
                if (cbErVT.Value.Trim() == "")
                {
                    retValue = false;
                    errMessage += "Earning Value Type Required !";
                }

            }
            if (cbPriClass.Value.Trim() == "Ded")
            {
                if (cbDeVt.Value.Trim() == "")
                {
                    retValue = false;
                    errMessage += "Deduction Value Type Required !";
                }

            }
            if (!retValue) oApplication.SetStatusBarMessage(errMessage, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
            return retValue;
        }

        private Boolean ValidateRecord()
        {
            try
            {
                //check code
                string checkcode = txtName.Value.Trim();
                if (!string.IsNullOrEmpty(checkcode))
                {
                    var checkcodecount = (from a in dbHrPayroll.MstElements where a.ElementName == checkcode select a).Count();
                    if (checkcodecount > 0)
                    {
                        oApplication.StatusBar.SetText("Duplicate codes not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Code is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check name
                string checkdesc = txtDesc.Value.Trim();
                if (string.IsNullOrEmpty(checkdesc))
                {
                    oApplication.StatusBar.SetText("Description is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check element type
                string checkeletype = cbPriClass.Value.Trim();
                if (string.IsNullOrEmpty(checkeletype))
                {
                    oApplication.StatusBar.SetText("Element Classification is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check element type rec/nonrec
                string checkeletype1 = cbRecType.Value.Trim();
                if (string.IsNullOrEmpty(checkeletype1))
                {
                    oApplication.StatusBar.SetText("Element Type is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //effective date is mandatory
                string checkeffective = txtEffectiveDate.Value.Trim();
                if (string.IsNullOrEmpty(checkeffective))
                {
                    oApplication.StatusBar.SetText("Effective date is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                var oTempElementRemainingAmount = (from a in dbHrPayroll.MstElements where a.FlgRemainingAmount == true select a).Count();

                if (oTempElementRemainingAmount > 0 && chkRemainingAmount.Checked == true)
                {
                    oApplication.StatusBar.SetText("Duplicate FlgBalanceAmount not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                var oTempElementBonus = (from a in dbHrPayroll.MstElements where a.FlgRemainingAmount == true select a).Count();

                if (oTempElementBonus > 0 && chkEmployeeBonus.Checked == true)
                {
                    oApplication.StatusBar.SetText("Duplicate Employee Bonus not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Boolean ValidateRecordUpdate()
        {
            try
            {
                //check code
                string checkcode = txtName.Value.Trim();
                if (!string.IsNullOrEmpty(checkcode))
                {
                    var checkcodecount = (from a in dbHrPayroll.MstElements where a.ElementName == checkcode select a).Count();
                    if (checkcodecount > 1)
                    {
                        oApplication.StatusBar.SetText("Duplicate codes not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Code is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check name
                string checkdesc = txtDesc.Value.Trim();
                if (string.IsNullOrEmpty(checkdesc))
                {
                    oApplication.StatusBar.SetText("Description is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check element type
                string checkeletype = cbPriClass.Value.Trim();
                if (string.IsNullOrEmpty(checkeletype))
                {
                    oApplication.StatusBar.SetText("Element Classification is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check element type rec/nonrec
                string checkeletype1 = cbRecType.Value.Trim();
                if (string.IsNullOrEmpty(checkeletype1))
                {
                    oApplication.StatusBar.SetText("Element Type is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //effective date is mandatory
                string checkeffective = txtEffectiveDate.Value.Trim();
                if (string.IsNullOrEmpty(checkeffective))
                {
                    oApplication.StatusBar.SetText("Effective date is mandatory field.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                var oTempElementRemainingAmount = (from a in dbHrPayroll.MstElements where a.FlgRemainingAmount == true select a).Count();
                if (oTempElementRemainingAmount > 0 && chkRemainingAmount.Checked == true)
                {
                    oApplication.StatusBar.SetText("Duplicate FlgBalanceAmount not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                var oTempElementBonus = (from a in dbHrPayroll.MstElements where a.FlgRemainingAmount == true select a).Count();
                if (oTempElementBonus > 0 && chkEmployeeBonus.Checked == true)
                {
                    oApplication.StatusBar.SetText("Duplicate Employee Bonus not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                return true;
                
            }
            catch
            {
                return false;
            }
        }
        
        private void doSubmit()
        {
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
            {
                doFind();
            }
            else
            {
                submitForm();
            }

        }
        
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            // , , StartDate, EndDate, ElmtType, flgProcessInPayroll, flgStandardElement
            SearchKeyVal.Add("ElementName", txtName.Value.Trim().ToString());
            SearchKeyVal.Add("Description", txtDesc.Value.Trim().ToString());
            SearchKeyVal.Add("ElmtType", cbPriClass.Value.Trim().ToString());
            SearchKeyVal.Add("flgProcessInPayroll", chPrInPrl.Checked == true ? "1" : "");
            SearchKeyVal.Add("flgStandardElement", chStdEle.Checked == true ? "1" : "");



        }
        
        private void doFind()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("elementSetup", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Element", "Select  elements");
            pic = null;
            if (st.Rows.Count > 0)
            {
                currentObjId = st.Rows[0][0].ToString();
                getRecord(currentObjId);
            }
        }

        private bool submitForm()
        {
            bool submitResult = true;
            try
            {
                MstElements payEle;
                MstElementEarning payEarn;
                MstElementDeduction payDed;
                MstElementContribution payContr;
                MstElementInformation payInfo;
                //int cnt = (from p in dbHrPayroll.MstElements where p.ElementName.ToString() == txtCode.Value select p).Count();
                 string checkcode = txtName.Value.Trim();
                 if (!string.IsNullOrEmpty(checkcode))
                 {
                     var checkcodecount = (from a in dbHrPayroll.MstElements where a.ElementName == checkcode select a).Count();
                     if (checkcodecount > 1)
                     {
                         //oApplication.StatusBar.SetText("Duplicate codes not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                         return false;
                     }
                 }
                int cnt = (from p in dbHrPayroll.MstElements where p.Id == loadedElement select p).Count();
                if (cnt > 0)
                {
                    payEle = (from p in dbHrPayroll.MstElements where p.Id == loadedElement select p).FirstOrDefault();
                    payEarn = payEle.MstElementEarning.ElementAt(0);
                    payDed = payEle.MstElementDeduction.ElementAt(0);
                    payContr = payEle.MstElementContribution.ElementAt(0);
                    payInfo = payEle.MstElementInformation.ElementAt(0);
                }
                else
                {
                    payEle = new MstElements();
                    payEarn = new MstElementEarning();
                    payDed = new MstElementDeduction();
                    payContr = new MstElementContribution();
                    payInfo = new MstElementInformation();
                    payEle.CreateDate = DateTime.Now;
                    payEle.CreatedBy = oCompany.UserName;
                    dbHrPayroll.MstElements.InsertOnSubmit(payEle);
                    payEle.MstElementEarning.Add(payEarn);
                    payEle.MstElementDeduction.Add(payDed);
                    payEle.MstElementContribution.Add(payContr);
                    payEle.MstElementInformation.Add(payInfo);
                }
                payEle.ElementName = txtName.Value;
                payEle.Description = txtDesc.Value;
                payEle.StartDate = DateTime.ParseExact(txtEffectiveDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                if (txEndDate.Value != "")
                {
                    payEle.EndDate = DateTime.ParseExact(txEndDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                else
                {
                    payEle.EndDate = null;
                }
                payEle.FlgProcessInPayroll = chPrInPrl.Checked;
                payEle.FlgStandardElement = chStdEle.Checked;
                payEle.FlgEffectOnGross = chOnGross.Checked;
                payEle.FlgProbationApplicable = chkProbation.Checked;
                payEle.FlgVGross = chkVGross.Checked;
                payEle.FlgShiftDays = chkShiftDays.Checked;
                payEle.FlgGradeDep = chkGradeDependent.Checked;
                payEle.ElmtType = cbPriClass.Value.Trim();
                payEle.ElmtTypeLovType = "Ele_Type";
                payEle.Type = cbRecType.Value.Trim();
                payEle.TypeLovType = "Ele_Cat";
                payEle.FlgConBatch = chBtch.Checked;


                // payEarn.ValueType = cbErVT.Value;
                payEarn.Value = Convert.ToDecimal(txErVal.Value);
                payEarn.ValueType = cbErVT.Value.Trim();
                payEarn.FlgAdditionalEntryAllowed = chErAddEnt.Checked;
                payEarn.FlgLeaveEncashment = chErLE.Checked;
                payEarn.FlgEOS = chErEos.Checked;
                payEle.FlgGosi = flgGosi.Checked;
                payEarn.FlgMultipleEntryAllowed = chErMltp.Checked;
                payEarn.FlgNotTaxable = chErNT.Checked;
                payEarn.FlgVariableValue = chErVar.Checked;
                payEarn.FlgPropotionate = chkProportionEar.Checked;

                //Deduction Value
                payDed.FlgAdditionalEntryAllowed = chDeAddEn.Checked;
                payDed.FlgMultipleEntryAllowed = chDeMulEn.Checked;
                payDed.ValueType = cbDeVt.Value.Trim();
                payDed.Value = Convert.ToDecimal(txDeVal.Value);
                payDed.FlgEOS = chkDeductionEos.Checked;
                payDed.FlgPropotionate = chkProportionDed.Checked;
                payDed.FlgNonTaxable = chkDedNonTax.Checked;
                payEle.FlgAttendanceAllowance = chkAttendanceAllowance.Checked;
                payEle.FlgRemainingAmount = chkRemainingAmount.Checked;
                payEle.FlgEmployeeBonus = chkEmployeeBonus.Checked;

                // payContr
                payContr.ContributionLOVType = cbCnType.Value.Trim();
                payContr.ContributionID = cbCnType.Value.Trim();
                payContr.Employee = Convert.ToDecimal(txCnEmpCo.Value);
                payContr.Employer = Convert.ToDecimal(txCnEmprCo.Value);
                payContr.ContTaxDiscount = Convert.ToDecimal(txCnNTA.Value);
                payContr.MaxAppAmount = string.IsNullOrEmpty(txmxant.Value) ? 0.0M : Convert.ToDecimal(txmxant.Value);
                payContr.MaxEmployeeContribution = string.IsNullOrEmpty(txMxEmly.Value) ? 0.0M : Convert.ToDecimal(txMxEmly.Value);
                payContr.MaxEmployerContribution = string.IsNullOrEmpty(txMxEmlr.Value) ? 0.0M : Convert.ToDecimal(txMxEmlr.Value);
                payContr.SalaryRangeFrom = string.IsNullOrEmpty(txtSalaryRangeFrom.Value) ? 0.0M : Convert.ToDecimal(txtSalaryRangeFrom.Value);
                payContr.SalaryRangeTo = string.IsNullOrEmpty(txtSalaryRangeTo.Value) ? 0.0M : Convert.ToDecimal(txtSalaryRangeTo.Value);
                payContr.ValidOnSalary = string.IsNullOrEmpty(cmbValidOn.Value) ? "" : Convert.ToString(cmbValidOn.Value);
                payContr.DaysRangeFrom = string.IsNullOrEmpty(txtDaysFrom.Value) ? 0 : Convert.ToInt32(txtDaysFrom.Value);
                payContr.DaysRangeTo = string.IsNullOrEmpty(txtDaysTo.Value) ? 0 : Convert.ToInt32(txtDaysTo.Value);
                payContr.FlgEOS = chkContributionEos.Checked;
                payContr.FlgVariableValue = chkVCo.Checked;


                // PayInfo
                payInfo.FlgTaxable = chInTxble.Checked;
                payInfo.Value = Convert.ToDecimal(txInVal.Value);
                //payInfo.

                payEle.UpdateDate = DateTime.Now;
                payEle.UpdatedBy = oCompany.UserName;
                payEle.CreatedBy = oCompany.UserName;
                dbHrPayroll.SubmitChanges();

                dbHrPayroll.SubmitChanges();
                if (cnt == 0)
                {
                    getData();
                    addNew();
                }

                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, false);

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
                oForm.AutoManaged = false;
                submitResult = false;
            }
            return submitResult;
        }
        
        private void _fillFields()
        {
            oForm.Freeze(true);
            try
            {
                if (currentRecord >= 0)
                {
                    txtDesc.Active = true;
                    ItxName.Enabled = false;
                    MstElements record = PayrollElement.ElementAt<MstElements>(currentRecord);
                    //var record = dbHrPayroll.MstElements.ElementAt<MstElements>(currentRecord);
                    loadedElement = record.Id;
                    switch (record.ElmtType.Trim())
                    {
                        case "Ear":
                            oForm.PaneLevel = 1;
                            break;
                        case "Ded":
                            oForm.PaneLevel = 2;
                            break;
                        case "Con":
                            oForm.PaneLevel = 3;
                            break;
                        case "Inf":
                            oForm.PaneLevel = 4;
                            break;
                    }
                    oForm.DataSources.UserDataSources.Item("txCode").ValueEx = record.Id.ToString();
                    oForm.DataSources.UserDataSources.Item("txName").ValueEx = record.ElementName.ToString();
                    oForm.DataSources.UserDataSources.Item("txDescr").ValueEx = record.Description;
                    //txtCode.Value = record.Id.ToString();
                    //txtName.Value = record.ElementName;
                    //txtDesc.Value = record.Description;
                    
                    //cbPriClass.Value = record.FlgProcessInPayroll;
                    //cbRecType.Value = record.FlgRecurrin
                    oForm.DataSources.UserDataSources.Item("chPrInPrl").ValueEx = record.FlgProcessInPayroll == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chStdEle").ValueEx = record.FlgStandardElement == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chOnGross").ValueEx = record.FlgEffectOnGross == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("flgProb").ValueEx = record.FlgProbationApplicable == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chkVGross").ValueEx = record.FlgVGross == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("flgGosi").ValueEx = record.FlgGosi == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("cbRecType").ValueEx = record.Type;
                    oForm.DataSources.UserDataSources.Item("cbPriClass").ValueEx = record.ElmtType;
                    oForm.DataSources.UserDataSources.Item("chBtch").ValueEx = record.FlgConBatch == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chsd").ValueEx = record.FlgShiftDays == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chgd").ValueEx = record.FlgGradeDep == true ? "Y" : "N";
                    try
                    {
                        oForm.DataSources.UserDataSources.Item("txEfctDate").ValueEx = Convert.ToDateTime(record.StartDate).ToString("yyyyMMdd");
                        if (record.EndDate != null)
                        {
                            oForm.DataSources.UserDataSources.Item("txEndDate").ValueEx = Convert.ToDateTime(record.EndDate).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txEndDate.Value = "";
                        }
                    }

                    catch { }
                    if (record.MstElementEarning.Count > 0)
                    {
                        // oForm.DataSources.UserDataSources.Item("chErAddEnt").ValueEx = "Y";

                        oForm.DataSources.UserDataSources.Item("cbErVT").ValueEx = record.MstElementEarning.ElementAt(0).ValueType;

                        oForm.DataSources.UserDataSources.Item("chErAddEnt").ValueEx = record.MstElementEarning.ElementAt(0).FlgAdditionalEntryAllowed == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chErLE").ValueEx = record.MstElementEarning.ElementAt(0).FlgLeaveEncashment == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chErVar").ValueEx = record.MstElementEarning.ElementAt(0).FlgVariableValue == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chkproEar").ValueEx = record.MstElementEarning.ElementAt(0).FlgPropotionate == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chErEos").ValueEx = record.MstElementEarning.ElementAt(0).FlgEOS == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chErMltp").ValueEx = record.MstElementEarning.ElementAt(0).FlgMultipleEntryAllowed == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chErNT").ValueEx = record.MstElementEarning.ElementAt(0).FlgNotTaxable == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("txErVal").ValueEx = record.MstElementEarning.ElementAt(0).Value.ToString();
                        oForm.DataSources.UserDataSources.Item("chAttAllow").ValueEx = record.FlgAttendanceAllowance == true ? "Y" : "N";
                        oForm.DataSources.UserDataSources.Item("chRemnAmt").ValueEx = record.FlgRemainingAmount == true ? "Y" : "N";
                        oForm.DataSources.UserDataSources.Item("chEmpBonus").ValueEx = record.FlgEmployeeBonus == true ? "Y" : "N";


                    }
                    if (record.MstElementDeduction.Count > 0)
                    {
                        oForm.DataSources.UserDataSources.Item("cbDeVt").ValueEx = record.MstElementDeduction.ElementAt(0).ValueType;
                        oForm.DataSources.UserDataSources.Item("chDeAddEn").ValueEx = record.MstElementDeduction.ElementAt(0).FlgAdditionalEntryAllowed == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chDeMulEn").ValueEx = record.MstElementDeduction.ElementAt(0).FlgMultipleEntryAllowed == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chkdEOS").ValueEx = record.MstElementDeduction.ElementAt(0).FlgEOS == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("txDeVal").ValueEx = record.MstElementDeduction.ElementAt(0).Value.ToString();
                        oForm.DataSources.UserDataSources.Item("chkProDed").ValueEx = record.MstElementDeduction.ElementAt(0).FlgPropotionate == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chdntax").ValueEx = record.MstElementDeduction.ElementAt(0).FlgNonTaxable == false ? "N" : "Y";

                    }
                    if (record.MstElementContribution.Count > 0)
                    {
                        oForm.DataSources.UserDataSources.Item("cbCnType").ValueEx = record.MstElementContribution.ElementAt(0).ContributionLOVType;

                        oForm.DataSources.UserDataSources.Item("txCnEmpCo").ValueEx = record.MstElementContribution.ElementAt(0).Employee.ToString();
                        oForm.DataSources.UserDataSources.Item("txCnEmprCo").ValueEx = record.MstElementContribution.ElementAt(0).Employer.ToString();
                        if (!string.IsNullOrEmpty(record.MstElementContribution.ElementAt(0).MaxAppAmount.ToString()))
                        {
                            oForm.DataSources.UserDataSources.Item("txmxant").ValueEx = record.MstElementContribution.ElementAt(0).MaxAppAmount.ToString();
                        }
                        if (!string.IsNullOrEmpty(record.MstElementContribution.ElementAt(0).MaxEmployeeContribution.ToString()))
                        {
                            oForm.DataSources.UserDataSources.Item("txMxEmly").ValueEx = record.MstElementContribution.ElementAt(0).MaxEmployeeContribution.ToString();
                        }
                        if (!string.IsNullOrEmpty(record.MstElementContribution.ElementAt(0).MaxEmployerContribution.ToString()))
                        {
                            oForm.DataSources.UserDataSources.Item("txMxEmlr").ValueEx = record.MstElementContribution.ElementAt(0).MaxEmployerContribution.ToString();
                        }
                        oForm.DataSources.UserDataSources.Item("chkcEOS").ValueEx = record.MstElementContribution.ElementAt(0).FlgEOS == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("chkVCo").ValueEx = record.MstElementContribution.ElementAt(0).FlgVariableValue == false ? "N" : "Y";
                        oForm.DataSources.UserDataSources.Item("txCnNTA").ValueEx = record.MstElementContribution.ElementAt(0).ContTaxDiscount == null ? "0" : record.MstElementContribution.ElementAt(0).ContTaxDiscount.ToString();
                        oForm.DataSources.UserDataSources.Item("txsfrom").ValueEx = record.MstElementContribution.ElementAt(0).SalaryRangeFrom == null ? "0" : record.MstElementContribution.ElementAt(0).SalaryRangeFrom.ToString();
                        oForm.DataSources.UserDataSources.Item("txsto").ValueEx = record.MstElementContribution.ElementAt(0).SalaryRangeTo == null ? "0" : record.MstElementContribution.ElementAt(0).SalaryRangeTo.ToString();
                        oForm.DataSources.UserDataSources.Item("txdfrom").ValueEx = record.MstElementContribution.ElementAt(0).DaysRangeFrom == null ? "0" : record.MstElementContribution.ElementAt(0).DaysRangeFrom.ToString();
                        oForm.DataSources.UserDataSources.Item("txdto").ValueEx = record.MstElementContribution.ElementAt(0).DaysRangeTo == null ? "0" : record.MstElementContribution.ElementAt(0).DaysRangeTo.ToString();
                        oForm.DataSources.UserDataSources.Item("cbvalidon").ValueEx = record.MstElementContribution.ElementAt(0).ValidOnSalary == null ? "-1" : record.MstElementContribution.ElementAt(0).ValidOnSalary;
                    }
                    if (record.MstElementInformation.Count > 0)
                    {
                        oForm.DataSources.UserDataSources.Item("txInVal").ValueEx = record.MstElementInformation.ElementAt(0).Value.ToString();
                        oForm.DataSources.UserDataSources.Item("chInTxble").ValueEx = record.MstElementInformation.ElementAt(0).FlgTaxable == false ? "N" : "Y";

                    }

                }

                oForm.Freeze(false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch
            {
                oApplication.SetStatusBarMessage("Element Document didn't load successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                oForm.Freeze(false);
            }
        }

        #endregion
        
    }
}
