using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_ResAprv : HRMSBaseForm
    {

        #region "Global Variable Area"
        
        SAPbouiCOM.Button btnAprvd, btnRejc, btCancel;      
        SAPbouiCOM.EditText txtEmpName, txtEmpCode, txtdocNum, txtManager, txtdoj, txtDocdt, txtdesig, txtSalary, txtOriginator, txtResgdt, txtAprby, txtAprdt, txtTerdt;
        SAPbouiCOM.EditText txtResigReason, txtAprComents;
        SAPbouiCOM.CheckBox flgOption1, flgOption2, flgOption3, flgOption4, flgOption5, flgOption6, flgOption7;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ResReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "txtEmpC":
                    LoadSelectedData(txtEmpCode.Value);
                    break;
                default:
                    break;
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btnAprvd":
                        ApproveResig();
                        break;
                    case "btnRejc":
                        RejectEmpResign();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ResReq Function: AfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btnAprvd = oForm.Items.Item("btnAprvd").Specific;
                btnRejc = oForm.Items.Item("btnRejc").Specific;
                btCancel = oForm.Items.Item("2").Specific;

                //Initializing Textboxes
                txtEmpName = oForm.Items.Item("txtEmpN").Specific;
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;                
                txtdocNum = oForm.Items.Item("txtDocN").Specific;
                txtManager = oForm.Items.Item("txtMang").Specific;
                txtdoj = oForm.Items.Item("txtdoj").Specific;
                txtDocdt = oForm.Items.Item("txtDocD").Specific;
                txtdesig = oForm.Items.Item("txtDeig").Specific;
                txtSalary = oForm.Items.Item("txtSal").Specific;
                txtOriginator = oForm.Items.Item("txtOrig").Specific;
                txtResgdt = oForm.Items.Item("txtRoD").Specific;
                txtResigReason = oForm.Items.Item("txtResR").Specific;
                txtAprComents = oForm.Items.Item("txtAprC").Specific;
                txtAprby = oForm.Items.Item("txtAprby").Specific;

                oForm.DataSources.UserDataSources.Add("txtAprdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtAprdt = oForm.Items.Item("txtAprdt").Specific;
                txtAprdt.DataBind.SetBound(true, "", "txtAprdt");

                oForm.DataSources.UserDataSources.Add("txtTerdt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtTerdt = oForm.Items.Item("txtTerdt").Specific;
                txtTerdt.DataBind.SetBound(true, "", "txtTerdt");

                oForm.DataSources.UserDataSources.Add("flgOption1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption1 = oForm.Items.Item("flgOption1").Specific;
                flgOption1.DataBind.SetBound(true, "", "flgOption1");

                oForm.DataSources.UserDataSources.Add("flgOption2", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption2 = oForm.Items.Item("flgOption2").Specific;
                flgOption2.DataBind.SetBound(true, "", "flgOption2");

                oForm.DataSources.UserDataSources.Add("flgOption3", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption3 = oForm.Items.Item("flgOption3").Specific;
                flgOption3.DataBind.SetBound(true, "", "flgOption3");

                oForm.DataSources.UserDataSources.Add("flgOption4", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption4 = oForm.Items.Item("flgOption4").Specific;
                flgOption4.DataBind.SetBound(true, "", "flgOption4");


                oForm.DataSources.UserDataSources.Add("flgOption5", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption5 = oForm.Items.Item("flgOption5").Specific;
                flgOption5.DataBind.SetBound(true, "", "flgOption5");


                oForm.DataSources.UserDataSources.Add("flgOption6", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption6 = oForm.Items.Item("flgOption6").Specific;
                flgOption6.DataBind.SetBound(true, "", "flgOption6");

                oForm.DataSources.UserDataSources.Add("flgOption7", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption7 = oForm.Items.Item("flgOption7").Specific;
                flgOption7.DataBind.SetBound(true, "", "flgOption7");                

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadSelectedData(String pCode)
        {
            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    var empRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == pCode).FirstOrDefault();
                    if (empRecord != null)
                    {
                        var getEmpResRecord = (from a in dbHrPayroll.TrnsResignation
                                               where a.EmpID == empRecord.ID
                                               select a).FirstOrDefault();
                        if (getEmpResRecord != null)
                        {
                            txtEmpName.Value = empRecord.FirstName + " " + empRecord.MiddleName + " " + empRecord.LastName;
                            txtdocNum.Value = Convert.ToString(getEmpResRecord.DocNum);
                            txtManager.Value = Convert.ToString(getEmpResRecord.ManagerID);
                            txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmpResRecord.ManagerID select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                            txtdoj.Value = Convert.ToString(getEmpResRecord.DateOfJoining);
                            txtDocdt.Value = Convert.ToString(getEmpResRecord.CreateDate);
                            txtSalary.Value = String.Format("{0:0.00}", empRecord.BasicSalary);
                            txtOriginator.Value = Convert.ToString(getEmpResRecord.OriginatorID);
                            txtdesig.Value = Convert.ToString(getEmpResRecord.DesignationID);
                            txtResgdt.Value = Convert.ToString(getEmpResRecord.ResignDate);
                            //txtAprdt.Value = Convert.ToString(DateTime.Now.Date);
                            txtResigReason.Value = Convert.ToString(getEmpResRecord.ResignationReason);
                            if (getEmpResRecord.FlgOption1 == true)
                            {
                                flgOption1.Checked = true;
                            }
                            if (getEmpResRecord.FlgOption2 == true)
                            {
                                flgOption2.Checked = true;
                            }
                            if (getEmpResRecord.FlgOption3 == true)
                            {
                                flgOption3.Checked = true;
                            }
                            if (getEmpResRecord.FlgOption4 == true)
                            {
                                flgOption4.Checked = true;
                            }
                            if (getEmpResRecord.FlgOption5 == true)
                            {
                                flgOption5.Checked = true;
                            }
                            if (getEmpResRecord.FlgOption6 == true)
                            {
                                flgOption6.Checked = true;
                            }
                            if (getEmpResRecord.FlgOption7 == true)
                            {
                                flgOption7.Checked = true;
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ResReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ApproveResig()
        {
            try
            {
                int intdocNum = Convert.ToInt32(txtdocNum.Value);
                if (!String.IsNullOrEmpty(txtEmpCode.Value))
                {
                    string loginEmpID = oCompany.UserName;
                    //EMID b Where Clause main Dalni hai.
                    var appdesRegRecord = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == intdocNum && a.EmpID == loginEmpID && a.DocType == 12 && a.FlgActive == true).FirstOrDefault();
                    if (appdesRegRecord != null)
                    {
                        appdesRegRecord.LineStatusID = "LV0006"; //Resign Approved
                        appdesRegRecord.UpdateDt = DateTime.Now;
                        appdesRegRecord.Remarks = txtAprComents.Value;
                    }

                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void RejectEmpResign()
        {
            int intdocNum = Convert.ToInt32(txtdocNum.Value);
            if (!String.IsNullOrEmpty(txtEmpCode.Value))
            {
                //EMID b Where Clause main Dalni hai.
                var appdesRegRecord = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == intdocNum && a.DocType == 12 && a.FlgActive == true).FirstOrDefault();
                if (appdesRegRecord != null)
                {
                    appdesRegRecord.LineStatusID = "LV0007"; //Resign Rejected
                    appdesRegRecord.UpdateDt = DateTime.Now;
                    appdesRegRecord.Remarks = txtAprComents.Value;                    
                }
                dbHrPayroll.SubmitChanges();
            }
        }

        #endregion
    }
}
