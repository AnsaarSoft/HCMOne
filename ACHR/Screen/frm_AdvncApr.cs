using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_AdvncApr:HRMSBaseForm
    {
        #region "Global Variable Area"
     
        SAPbouiCOM.Button btApproved, btReject, btCancel;
        SAPbouiCOM.EditText txtReqBy, txtEmpCode, txtdocNum, txtManager, txtdoj, txtdesig, txtSalary, txtOriginator, txtRqAmnt, txtReqDt, txtApAm;
        private SAPbouiCOM.ComboBox cbAdvT;
        SAPbouiCOM.DataTable dtPrevAdvance;     
        SAPbouiCOM.Matrix grdAdvncDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, AdvanceType, Amount, RecToDate, RemToDate;
        SAPbouiCOM.Button btId;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1288", false);  // Next Record
                oForm.EnableMenu("1289", false);  // Pevious Record
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();
                FillParentAdvnTypeCombo();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncApr Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
       
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btAppr":
                        ApproveAdvance();
                        break;
                    case "btId":
                        picDoc();
                        break;
                    case "btnRej":
                        RejectAdvance();
                        break;
                    case "2":

                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncApr Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btApproved = oForm.Items.Item("btAppr").Specific;
                btReject = oForm.Items.Item("btnRej").Specific;
                btCancel = oForm.Items.Item("2").Specific;

                //Initializing Textboxes
                txtReqBy = oForm.Items.Item("txtRby").Specific;
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;               
                txtdocNum = oForm.Items.Item("txtDNum").Specific;
                txtManager = oForm.Items.Item("txtManagr").Specific;

                oForm.DataSources.UserDataSources.Add("dtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtdoj = oForm.Items.Item("dtJoin").Specific;
                txtdoj.DataBind.SetBound(true, "", "dtJoin");

                txtdesig = oForm.Items.Item("txtDesig").Specific;
                txtSalary = oForm.Items.Item("txtSalry").Specific;
                txtOriginator = oForm.Items.Item("txtOrig").Specific;

                cbAdvT = oForm.Items.Item("cbAdvT").Specific;

                oForm.DataSources.UserDataSources.Add("txtRqAmnt", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtRqAmnt = oForm.Items.Item("txtRqAmnt").Specific;
                txtRqAmnt.DataBind.SetBound(true, "", "txtRqAmnt");

                oForm.DataSources.UserDataSources.Add("txtReqDt", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtReqDt = oForm.Items.Item("txtReqDt").Specific;
                txtReqDt.DataBind.SetBound(true, "", "txtReqDt");

                oForm.DataSources.UserDataSources.Add("txtApAm", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtApAm = oForm.Items.Item("txtApAm").Specific;
                txtRqAmnt.DataBind.SetBound(true, "", "txtApAm");

                InitiallizegridMatrix();
                
                
                string loginUserId = oCompany.UserName;            
             
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtPrevAdvance = oForm.DataSources.DataTables.Add("AdvanceRequest");
                dtPrevAdvance.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtPrevAdvance.Columns.Add("AdvanceType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("RecToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtPrevAdvance.Columns.Add("RemToDate", SAPbouiCOM.BoFieldsType.ft_Text);                

                grdAdvncDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grdAdvD").Specific;
                oColumns = (SAPbouiCOM.Columns)grdAdvncDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "No");

                oColumn = oColumns.Item("AdvType");
                AdvanceType = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "AdvanceType");

                oColumn = oColumns.Item("Amount");
                Amount = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "Amount");

                oColumn = oColumns.Item("cl_RecTD");
                RecToDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "RecToDate");

                oColumn = oColumns.Item("RemToDate");
                RemToDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "RemToDate");
                

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }      

        private void FillParentAdvnTypeCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstAdvance);
                var Data = from v in dbHrPayroll.MstAdvance select v;
                foreach (var v in Data)
                {
                    cbAdvT.ValidValues.Add(v.AllowanceId, v.Description);
                }
                oColumn.DisplayDesc = true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ApproveAdvance()
        {
            try
            {
                int intdocNum = Convert.ToInt32(txtdocNum.Value);
                if (!String.IsNullOrEmpty(txtEmpCode.Value))
                {
                    string loginEmpID = oCompany.UserName;                  
                    var aapAdvanceRecord = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == intdocNum && a.EmpID == loginEmpID && a.DocType == 20 && a.FlgActive == true).FirstOrDefault();
                    int empID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault().ID;
                    var AdvanceHeaderRecord = dbHrPayroll.TrnsAdvance.Where(a => a.DocNum == intdocNum && a.DocType == 20 && a.EmpID == empID).FirstOrDefault();
                    if (aapAdvanceRecord != null)
                    {
                        AdvanceHeaderRecord.ApprovedAmount = Convert.ToDecimal(txtApAm.Value);
                        AdvanceHeaderRecord.UpdateDate = DateTime.Now;
                        dbHrPayroll.SubmitChanges();
                        aapAdvanceRecord.LineStatusID = "LV0006"; //Advance Approved
                        aapAdvanceRecord.Remarks = "Advance Approved";
                        aapAdvanceRecord.UpdateDt = DateTime.Now;
                        dbHrPayroll.SubmitChanges();
                        ClearControls();
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    }
                    else
                    {
                        AdvanceHeaderRecord.ApprovedAmount = Convert.ToDecimal(txtApAm.Value);
                        AdvanceHeaderRecord.UpdateDate = DateTime.Now;
                        AdvanceHeaderRecord.DocAprStatus = "LV0006";
                        AdvanceHeaderRecord.DocStatus = "LV0002"; 
                        dbHrPayroll.SubmitChanges();
                        ClearControls();
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void RejectAdvance()
        {
            try
            {
                int intdocNum = Convert.ToInt32(txtdocNum.Value);
                if (!String.IsNullOrEmpty(txtEmpCode.Value))
                {
                    string loginEmpID = oCompany.UserName;
                    //EMID b Where Clause main Dalni hai.
                    int empID = dbHrPayroll.MstEmployee.Where(e => e.EmpID == txtEmpCode.Value).FirstOrDefault().ID;
                    var AdvanceHeaderRecord = dbHrPayroll.TrnsAdvance.Where(a => a.DocNum == intdocNum && a.DocType == 20 && a.EmpID == empID).FirstOrDefault();
                    var aapAdvanceRecord = dbHrPayroll.CfgApprovalDecisionRegister.Where(a => a.DocNum == intdocNum && a.EmpID == loginEmpID && a.DocType == 20 && a.FlgActive == true).FirstOrDefault();
                    if (aapAdvanceRecord != null)
                    {
                        aapAdvanceRecord.LineStatusID = "LV0007"; //Advance Rejected
                        aapAdvanceRecord.UpdateDt = DateTime.Now;
                        aapAdvanceRecord.Remarks = "Advance has been Rejected";
                        dbHrPayroll.SubmitChanges();
                        ClearControls();
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    }
                    else
                    {
                        AdvanceHeaderRecord.ApprovedAmount = Convert.ToDecimal(txtApAm.Value);
                        AdvanceHeaderRecord.UpdateDate = DateTime.Now;
                        AdvanceHeaderRecord.DocAprStatus = "LV0007";  //Advance Rejected
                        AdvanceHeaderRecord.DocStatus = "LV0002";
                        dbHrPayroll.SubmitChanges();
                        ClearControls();
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    }

                    
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadSelectedDatabyDocNum(string docNum)
        {
            try
            {
                string strDocStatus = "LV0001", strApprovalStatus = "LV0005";
                if (!string.IsNullOrEmpty(docNum))
                {
                    int docNumber = Convert.ToInt32(docNum);

                    var getEmpAdavce = dbHrPayroll.TrnsAdvance.Where(adv => adv.DocNum == docNumber && adv.DocStatus == strDocStatus && adv.DocAprStatus == strApprovalStatus).FirstOrDefault();                    
                    if (getEmpAdavce != null)
                    {
                        txtReqBy.Value = getEmpAdavce.EmpName;
                        txtEmpCode.Value = getEmpAdavce.MstEmployee.EmpID;
                        txtManager.Value = getEmpAdavce.ManagerName;
                        txtdoj.Value = getEmpAdavce.DateOfJoining == null ? "" : Convert.ToDateTime(getEmpAdavce.DateOfJoining).ToString("yyyyMMdd");
                        txtdesig.Value = getEmpAdavce.Designation;
                        txtSalary.Value = String.Format("{0:0.00}", getEmpAdavce.Salary);
                        txtOriginator.Value = Convert.ToString(getEmpAdavce.OriginatorName);
                        string strLoanTypeCode = Convert.ToString(dbHrPayroll.MstAdvance.Where(a => a.Id == getEmpAdavce.AdvanceType).FirstOrDefault().AllowanceId);
                        cbAdvT.Select(strLoanTypeCode, SAPbouiCOM.BoSearchKey.psk_ByValue);
                        txtRqAmnt.Value = Convert.ToString(getEmpAdavce.RequestedAmount);
                        txtApAm.Value = String.Format("{0:0.00}", getEmpAdavce.RequestedAmount);                       
                        txtReqDt.Value = Convert.ToDateTime(getEmpAdavce.RequiredDate).ToString("yyyyMMdd");
                        GetAdvanceHistory(getEmpAdavce.MstEmployee.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncApr Function: LoadSelectedDatabyDocNum Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetAdvanceHistory(int intEmpID)
        {
            string strDocStatus = "LV0003", strApprovalStatus = "LV0006";
            Int16 i = 0;
            try
            {
                var Data = dbHrPayroll.TrnsAdvance.Where(adv => adv.EmpID == intEmpID && adv.DocAprStatus == strApprovalStatus).ToList();
                if (Data.Count == 0)
                {
                    dtPrevAdvance.Rows.Clear();
                    grdAdvncDetail.LoadFromDataSource();
                    return;
                }
                else if (Data != null && Data.Count > 0)
                {
                    dtPrevAdvance.Rows.Clear();
                    dtPrevAdvance.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        var AdvanceType = dbHrPayroll.MstAdvance.Where(a => a.Id == WD.AdvanceType).FirstOrDefault();
                        dtPrevAdvance.SetValue("No", i, i + 1);
                        dtPrevAdvance.SetValue("AdvanceType", i, AdvanceType.Description);
                        dtPrevAdvance.SetValue("Amount", i, String.Format("{0:0.00}", WD.ApprovedAmount));
                        dtPrevAdvance.SetValue("RecToDate", i, "0.00");
                        dtPrevAdvance.SetValue("RemToDate", i, String.Format("{0:0.00}", WD.ApprovedAmount));
                        i++;
                    }
                    grdAdvncDetail.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }

        private void ClearControls()
        {
            try
            {
                txtEmpCode.Value = string.Empty;
                txtReqBy.Value = string.Empty;
                txtdocNum.Value = string.Empty;
                txtManager.Value = string.Empty;
                txtdoj.Value = string.Empty;
                txtdesig.Value = string.Empty;
                txtSalary.Value = string.Empty;
                txtOriginator.Value = string.Empty;
                txtRqAmnt.Value = string.Empty;
                txtReqDt.Value = string.Empty;              
                txtApAm.Value = string.Empty;
                cbAdvT.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtPrevAdvance.Rows.Clear();
                grdAdvncDetail.LoadFromDataSource();

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void picDoc()
        {
            string strSql = sqlString.getSql("AdvApprovalDoc", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select DocNumber", "Select Employee for AdvanceApproval");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtdocNum.Value = st.Rows[0][0].ToString();
                LoadSelectedDatabyDocNum(txtdocNum.Value);
            }
        }

        #endregion
    }
}
