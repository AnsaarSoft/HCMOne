using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_ViewLeaveReq : HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.Button btnSearch,btnClear, btnCancel;
        SAPbouiCOM.EditText txtEmpCode, txtEmpName;
        SAPbouiCOM.DataTable dtLeaveReq;
        SAPbouiCOM.Matrix grdLeaveReq;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clDN, clLT, clDDate, clLFr, clLTo, clCount, clDocS, clAprS;

        #endregion

        #region "B1 Form Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            oForm.EnableMenu("1288", false);  // Next Record
            oForm.EnableMenu("1289", false);  // Pevious Record
            oForm.EnableMenu("1290", false);  // First Record
            oForm.EnableMenu("1291", false);  // Last record 
            InitiallizeForm();            
            oForm.Freeze(false);
        }       
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {                                        
                    case "2":
                        break;
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_LeaveRequest Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
            
                btnCancel = oForm.Items.Item("2").Specific;          
               
                // Initialize TextBoxes

                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;
                txtEmpCode.DataBind.SetBound(true, "", "txtEmpC");

                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 80);
                txtEmpName = oForm.Items.Item("txtEmpN").Specific;
                txtEmpName.DataBind.SetBound(true, "", "txtEmpN");               
               
                InitiallizegridMatrix();
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
                dtLeaveReq = oForm.DataSources.DataTables.Add("LeaveRequest");
                dtLeaveReq.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtLeaveReq.Columns.Add("DocNum", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLeaveReq.Columns.Add("LeaveType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLeaveReq.Columns.Add("DocDate", SAPbouiCOM.BoFieldsType.ft_Date);
                dtLeaveReq.Columns.Add("LeaveFrom", SAPbouiCOM.BoFieldsType.ft_Date);
                dtLeaveReq.Columns.Add("LeaveTo", SAPbouiCOM.BoFieldsType.ft_Date);
                dtLeaveReq.Columns.Add("Count", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLeaveReq.Columns.Add("DocStatus", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLeaveReq.Columns.Add("AprStatus", SAPbouiCOM.BoFieldsType.ft_Text);


                grdLeaveReq = (SAPbouiCOM.Matrix)oForm.Items.Item("grdLeaves").Specific;
                oColumns = (SAPbouiCOM.Columns)grdLeaveReq.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "No");

                oColumn = oColumns.Item("clDN");
                clDN = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "DocNum");

                oColumn = oColumns.Item("clLT");
                clLT = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "LeaveType");

                oColumn = oColumns.Item("clDDate");
                clDDate = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "DocDate");

                oColumn = oColumns.Item("clLFr");
                clLFr = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "LeaveFrom");

                oColumn = oColumns.Item("clLTo");
                clLTo = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "LeaveTo");

                oColumn = oColumns.Item("clCount");
                clCount = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "Count");

                oColumn = oColumns.Item("clDocS");
                clDocS = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "DocStatus");

                oColumn = oColumns.Item("clAprS");
                clAprS = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "AprStatus");

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
                    MstEmployee getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == pCode
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txtEmpName.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        LoadLeaveRequestes(getEmp.ID);
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ViewLeaveReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void LoadLeaveRequestes(int EmployeeID)
        {
            Int16 i = 0;
            try
            {
                var LeaveRequests = dbHrPayroll.TrnsLeavesRequest.Where(lr => lr.EmpID == EmployeeID).ToList();               
                if (LeaveRequests != null && LeaveRequests.Count > 0)
                {
                    dtLeaveReq.Rows.Clear();
                    dtLeaveReq.Rows.Add(LeaveRequests.Count());
                    foreach (var WD in LeaveRequests)
                    {
                        var LeaveType = dbHrPayroll.MstLeaveType.Where(a => a.ID == WD.LeaveType).FirstOrDefault();
                        dtLeaveReq.SetValue("No", i, i + 1);
                        dtLeaveReq.SetValue("DocNum", i, WD.DocNum);
                        dtLeaveReq.SetValue("LeaveType", i, LeaveType.Description);
                        dtLeaveReq.SetValue("DocDate", i, WD.DocDate);
                        dtLeaveReq.SetValue("LeaveFrom", i, WD.LeaveFrom);
                        dtLeaveReq.SetValue("LeaveTo", i, WD.LeaveTo);
                        dtLeaveReq.SetValue("Count", i, WD.TotalCount.ToString());
                        string docStatus = dbHrPayroll.MstLOVE.Where(l => l.Code == WD.DocStatus).Single().Value;
                        if (!string.IsNullOrEmpty(docStatus))
                        {
                            dtLeaveReq.SetValue("DocStatus", i, docStatus);
                        }
                        string approvalStatus = dbHrPayroll.MstLOVE.Where(l => l.Code == WD.DocAprStatus).Single().Value;
                        if (!string.IsNullOrEmpty(approvalStatus))
                        {
                            dtLeaveReq.SetValue("AprStatus", i, approvalStatus);
                        }
                        i++;
                    }
                    grdLeaveReq.LoadFromDataSource();
                }
                else
                {
                    dtLeaveReq.Rows.Clear();
                    grdLeaveReq.LoadFromDataSource();                   
                }
            }
            catch (Exception ex)
            {

                //MessageBox.Show("Error in ViewLeaveRequests Page: Error is " + ex.Message);
            }
        }
        
        private void picEmp()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empLeaveReq", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for View Leave");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpCode.Value = st.Rows[0][0].ToString();
                LoadSelectedData(txtEmpCode.Value);
            }
        }
        
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("EmpID", txtEmpCode.Value.ToString());
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

        #endregion
    }
}
