using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_ViewAdvncReq : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.EditText txtEmpCode,txtReqBy, txtManager, txtdoj, txdesig, txtSalary;
        SAPbouiCOM.DataTable dtAdvanceReq;
        SAPbouiCOM.Matrix grdAdvanceDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clDocNum, AdvanceType, clDate, Amount, aprAmnt, RecToDate, RemToDate, cl_status, cl_Stop;
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
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ViewAdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                    case "1":
                        //if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                        UpdateAdvanceStatus();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ViewAdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmpCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmpCode.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }     
        }
        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "txtEmpC")
            {
                if (string.IsNullOrEmpty(txtEmpCode.Value))
                {
                    //OpenNewSearchForm();
                }
            }
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();
            txtEmpCode.Value = "";
            OpenNewSearchForm();

            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btId = oForm.Items.Item("btId").Specific;
                //Initializing Textboxes
                oForm.DataSources.UserDataSources.Add("txtRby", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80);
                txtReqBy = oForm.Items.Item("txtRby").Specific;
                txtReqBy.DataBind.SetBound(true, "", "txtRby");

                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;

                oForm.DataSources.UserDataSources.Add("txtManagr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80);
                txtManager = oForm.Items.Item("txtManagr").Specific;
                txtManager.DataBind.SetBound(true, "", "txtManagr");

                oForm.DataSources.UserDataSources.Add("dtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtdoj = oForm.Items.Item("dtJoin").Specific;
                txtdoj.DataBind.SetBound(true, "", "dtJoin");

                oForm.DataSources.UserDataSources.Add("txtDesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txdesig = oForm.Items.Item("txtDesig").Specific;
                txdesig.DataBind.SetBound(true, "", "txtDesig");

                oForm.DataSources.UserDataSources.Add("txtSalry", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtSalary = oForm.Items.Item("txtSalry").Specific;
                txtSalary.DataBind.SetBound(true, "", "txtSalry");

                InitiallizegridMatrix();

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE;
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
                dtAdvanceReq = oForm.DataSources.DataTables.Add("AdvanceRequest");
                dtAdvanceReq.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAdvanceReq.Columns.Add("DocNo", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAdvanceReq.Columns.Add("AdvanceType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAdvanceReq.Columns.Add("Date", SAPbouiCOM.BoFieldsType.ft_Date);
                dtAdvanceReq.Columns.Add("aprAmnt", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAdvanceReq.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Text);                
                dtAdvanceReq.Columns.Add("RecToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAdvanceReq.Columns.Add("RemToDate", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAdvanceReq.Columns.Add("Status", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAdvanceReq.Columns.Add("Stop", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);

                grdAdvanceDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grdAdvD").Specific;
                oColumns = (SAPbouiCOM.Columns)grdAdvanceDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "No");

                oColumn = oColumns.Item("DocNum");
                clDocNum = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "DocNo");

                oColumn = oColumns.Item("AdvType");
                AdvanceType = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "AdvanceType");

                oColumn = oColumns.Item("clDate");
                clDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "Date");

                oColumn = oColumns.Item("aprAmnt");
                aprAmnt = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "aprAmnt");

                oColumn = oColumns.Item("Amount");
                Amount = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "Amount");

                oColumn = oColumns.Item("cl_RecTD");
                RecToDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "RecToDate");

                oColumn = oColumns.Item("RemToDate");
                RemToDate = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "RemToDate");

                oColumn = oColumns.Item("cl_Status");
                cl_status = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "Status");

                oColumn = oColumns.Item("cl_Stop");
                cl_Stop = oColumn;
                oColumn.DataBind.Bind("AdvanceRequest", "Stop");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void picEmp()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empAdvance", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Loan");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpCode.Value = st.Rows[0][0].ToString();
                LoadSelectedData(txtEmpCode.Value);
            }
        }
        
        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empPick";                
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
        
        private void LoadSelectedData(String pCode)
        {
            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == pCode
                                  select a).FirstOrDefault();

                    if (getEmp != null)
                    {
                        txtReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                        txtdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                        txdesig.Value = getEmp.DesignationName;
                        txtSalary.Value = getEmp.BasicSalary != null ? String.Format("{0:0.00}", getEmp.BasicSalary) : "";
                        GetAdvanceHistory(getEmp.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ViewAdvncReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void GetAdvanceHistory(int intEmpID)
        {            
            Int16 i = 0;
            try
            {
                var Data = dbHrPayroll.TrnsAdvance.Where(adv => adv.EmpID == intEmpID).ToList();               
                if (Data.Count == 0)
                {
                    dtAdvanceReq.Rows.Clear();
                    grdAdvanceDetail.LoadFromDataSource();
                    return;
                }
                else if (Data != null && Data.Count > 0)
                {
                    decimal ReceiveAmount = 0.0M;
                    dtAdvanceReq.Rows.Clear();
                    dtAdvanceReq.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {
                        var ApprovalStatusDetail = dbHrPayroll.MstLOVE.Where(LV => LV.Code == WD.DocAprStatus).FirstOrDefault();
                        var AdvanceType = dbHrPayroll.MstAdvance.Where(a => a.Id == WD.AdvanceType).FirstOrDefault();
                        dtAdvanceReq.SetValue("No", i, i + 1);
                        dtAdvanceReq.SetValue("DocNo", i, WD.DocNum);
                        dtAdvanceReq.SetValue("Date", i, WD.RequiredDate);
                        dtAdvanceReq.SetValue("AdvanceType", i, AdvanceType.Description);
                        dtAdvanceReq.SetValue("Amount", i, String.Format("{0:0.00}", WD.RequestedAmount));
                        dtAdvanceReq.SetValue("aprAmnt", i, String.Format("{0:0.00}", WD.ApprovedAmount));
                        if (WD.ApprovedAmount > 0 && WD.RemainingAmount >= 0)
                        {
                            ReceiveAmount = WD.ApprovedAmount.Value - WD.RemainingAmount.Value;
                        }
                        if (ApprovalStatusDetail != null)
                        {
                            dtAdvanceReq.SetValue("Status", i, ApprovalStatusDetail.Value);
                        }
                        if (WD.FlgStop!=null && WD.FlgStop.Value==true)
                        {
                            dtAdvanceReq.SetValue("Stop", i, "Y");
                        }
                        else
                        {
                            dtAdvanceReq.SetValue("Stop", i, "N");
                        }
                        dtAdvanceReq.SetValue("RecToDate", i, String.Format("{0:0.00}", ReceiveAmount));
                        dtAdvanceReq.SetValue("RemToDate", i, String.Format("{0:0.00}", WD.RemainingAmount));
                        i++;
                    }
                    grdAdvanceDetail.LoadFromDataSource();
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }
        
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            if (!string.IsNullOrEmpty(txtEmpCode.Value))
            {
                SearchKeyVal.Add("EmpID", txtEmpCode.Value.ToString());
            }
        }
        
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmpCode.Value = Program.EmpID;
                    LoadSelectedData(txtEmpCode.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        private void UpdateAdvanceStatus()
        {
            string strDocNum = "";           
            bool isStop = false;
            try
            {
                for (int i = 1; i < grdAdvanceDetail.RowCount + 1; i++)
                {
                    strDocNum = (grdAdvanceDetail.Columns.Item("DocNum").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    isStop = (grdAdvanceDetail.Columns.Item("cl_Stop").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    var AdvanceRecord = dbHrPayroll.TrnsAdvance.Where(a => a.DocNum == Convert.ToInt32(strDocNum) && a.MstEmployee.EmpID == txtEmpCode.Value.Trim() ).FirstOrDefault();
                    if (isStop)
                    {
                        AdvanceRecord.FlgStop = true;
                    }
                    else
                    {
                        AdvanceRecord.FlgStop = false;
                    }
                    dbHrPayroll.SubmitChanges();
                }
                //dbHrPayroll.SubmitChanges();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: UpdateAdvanceStatus Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
