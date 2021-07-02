using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_ViewEmpPen : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.EditText txtEmpCode, txtReqBy, txtManager, txtdoj, txdesig, txtSalary;
        SAPbouiCOM.DataTable dtEmpPenalty;
        SAPbouiCOM.Matrix grdEmpPenaltyDetail;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo,clDocNum, PealtyType, clFromDate, clToDate, clDays, clPenaltyDays, cl_Active;
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
                        UpdateEmpPenaltyStatus();
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
                dtEmpPenalty = oForm.DataSources.DataTables.Add("EmpPenalty");
                dtEmpPenalty.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmpPenalty.Columns.Add("DocNo", SAPbouiCOM.BoFieldsType.ft_Integer); 
                dtEmpPenalty.Columns.Add("PenaltyType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpPenalty.Columns.Add("FromDate", SAPbouiCOM.BoFieldsType.ft_Date);
                dtEmpPenalty.Columns.Add("ToDate", SAPbouiCOM.BoFieldsType.ft_Date);
                dtEmpPenalty.Columns.Add("Days", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpPenalty.Columns.Add("PenaltyDays", SAPbouiCOM.BoFieldsType.ft_Text);        
                dtEmpPenalty.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);

                grdEmpPenaltyDetail = (SAPbouiCOM.Matrix)oForm.Items.Item("grdEmpP").Specific;
                oColumns = (SAPbouiCOM.Columns)grdEmpPenaltyDetail.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "No");

                oColumn = oColumns.Item("clDocNo");
                clDocNum = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "DocNo");


                oColumn = oColumns.Item("PenType");
                PealtyType = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "PenaltyType");

                oColumn = oColumns.Item("clFDate");
                clFromDate = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "FromDate");

                oColumn = oColumns.Item("clTDate");
                clToDate = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "ToDate");

                oColumn = oColumns.Item("clDays");
                clDays = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "Days");

                oColumn = oColumns.Item("clPDays");
                clPenaltyDays = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "PenaltyDays");

                oColumn = oColumns.Item("cl_Act");
                cl_Active = oColumn;
                oColumn.DataBind.Bind("EmpPenalty", "Active");

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
                string comName = "Search";
                Program.sqlString = "empAdvance";
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
                                  where a.EmpID.Contains(pCode)
                                  select a).FirstOrDefault();

                    if (getEmp != null)
                    {
                        txtReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                        txtdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                        txdesig.Value = getEmp.DesignationName;
                        txtSalary.Value = getEmp.BasicSalary != null ? String.Format("{0:0.00}", getEmp.BasicSalary) : "";
                        GetEmpPenaltyHistory(getEmp.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ViewAdvncReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetEmpPenaltyHistory(int intEmpID)
        {
            Int16 i = 0;
            try
            {
                var Data = dbHrPayroll.TrnsEmployeePenalty.Where(adv => adv.EmpId == intEmpID).ToList();
                if (Data.Count == 0)
                {
                    dtEmpPenalty.Rows.Clear();
                    grdEmpPenaltyDetail.LoadFromDataSource();
                    return;
                }
                else if (Data != null && Data.Count > 0)
                {                  
                    dtEmpPenalty.Rows.Clear();
                    dtEmpPenalty.Rows.Add(Data.Count());
                    foreach (var WD in Data)
                    {                       
                        var PenaltyType = dbHrPayroll.MstPenaltyRules.Where(a => a.ID == WD.PenaltyId).FirstOrDefault();
                        dtEmpPenalty.SetValue("No", i, i + 1);
                        dtEmpPenalty.SetValue("PenaltyType", i, PenaltyType.Description);
                        dtEmpPenalty.SetValue("DocNo", i, WD.ID);
                        dtEmpPenalty.SetValue("FromDate", i, WD.FromDate);
                        dtEmpPenalty.SetValue("ToDate", i, WD.ToDate);
                        dtEmpPenalty.SetValue("Days", i, String.Format("{0:0.00}", WD.Days));
                        dtEmpPenalty.SetValue("PenaltyDays", i, String.Format("{0:0.00}", WD.PenaltyDays));
                        //if (WD.FlgActive != null && WD.FlgActive.Value == true)
                        //{
                        //    //dtEmpPenalty.SetValue("Active", i, "Y");
                        //}
                        //else
                        //{
                        //    //dtEmpPenalty.SetValue("Active", i, "N");
                        //}                       
                        i++;
                    }
                    grdEmpPenaltyDetail.LoadFromDataSource();
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
        private void UpdateEmpPenaltyStatus()
        {
            string strDocNum = "";
            bool isStop = false;
            try
            {
                for (int i = 1; i < grdEmpPenaltyDetail.RowCount + 1; i++)
                {
                    strDocNum = (grdEmpPenaltyDetail.Columns.Item("clDocNo").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    isStop = (grdEmpPenaltyDetail.Columns.Item("cl_Act").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                    var AdvanceRecord = dbHrPayroll.TrnsEmployeePenalty.Where(a => a.ID == Convert.ToInt32(strDocNum)).FirstOrDefault();
                    if (isStop)
                    {
                        //AdvanceRecord.FlgActive = true;
                    }
                    else
                    {
                       // AdvanceRecord.FlgActive = false;
                    }
                }
                dbHrPayroll.SubmitChanges();
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
