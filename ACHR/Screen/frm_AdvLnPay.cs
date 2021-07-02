using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Data.SqlClient;
using SAPbobsCOM;

namespace ACHR.Screen
{

    class frm_AdvLnPay : HRMSBaseForm
    {
        #region "Global Variable Area"


        SAPbouiCOM.Button btSave, btCancel, btPay;
        SAPbouiCOM.EditText txtReqBy, txtEmpCode, txtdocNum, txtManager, txtdoj, txtdesig, txtSalary, txtOriginator, txtRqAmnt, txtReqDt, txtdocStatus, txtappStatus, txtAprAmount, txtExpSalary, txtPreAd;
        private SAPbouiCOM.ComboBox cbPType;
        SAPbouiCOM.DataTable dtPrevAdvance;
        SAPbouiCOM.Matrix grdAdvncDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, AdvanceType, Amount, RecToDate, RemToDate, clDate;
        private Int32 CurrentRecord = 0, TotalRecords = 0;       
        SAPbouiCOM.Button btId, btPrint;
        SAPbouiCOM.CheckBox flgStop;
        SAPbouiCOM.PictureBox pctBox;
        DataTable dtError = new DataTable();

        #endregion

        #region "B1 Events"
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();

                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {                   
                    case "btId":
                        OpenNewSearchForm();
                        break;                                    
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {                
                btCancel = oForm.Items.Item("2").Specific;              
                btPay = oForm.Items.Item("btPay").Specific;
      



                //Initializing Textboxes
                txtReqBy = oForm.Items.Item("txtRby").Specific;
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;
   
                oForm.DataSources.UserDataSources.Add("txtDNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtdocNum = oForm.Items.Item("txtDNum").Specific;
                txtdocNum.DataBind.SetBound(true, "", "txtDNum");

                txtManager = oForm.Items.Item("txtManagr").Specific;

                oForm.DataSources.UserDataSources.Add("dtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtdoj = oForm.Items.Item("dtJoin").Specific;
                txtdoj.DataBind.SetBound(true, "", "dtJoin");

                txtdesig = oForm.Items.Item("txtDesig").Specific;
                txtSalary = oForm.Items.Item("txtSalry").Specific;
                txtExpSalary = oForm.Items.Item("txtExpS").Specific;

                cbPType = oForm.Items.Item("cbPTyp").Specific;
                FillPaymentTypeCombo();

                oForm.DataSources.UserDataSources.Add("txtRqAmnt", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtRqAmnt = oForm.Items.Item("txtRqAmnt").Specific;
                txtRqAmnt.DataBind.SetBound(true, "", "txtRqAmnt");                

                oForm.DataSources.UserDataSources.Add("txtApram", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtAprAmount = oForm.Items.Item("txtApram").Specific;
                txtAprAmount.DataBind.SetBound(true, "", "txtApram");
                txtPreAd = oForm.Items.Item("txtPreAd").Specific;              

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oForm.ActiveItem = "txtEmpC";
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillPaymentTypeCombo()
        {
            try
            {
                cbPType.ValidValues.Add("-1", "[Select One]");
                cbPType.ValidValues.Add("1", "Bank");
                cbPType.ValidValues.Add("2", "Cash");

                cbPType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "Search";
                Program.sqlString = "AdvLoanPayment";
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

        #endregion
    }
}
