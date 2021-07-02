using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_CancelLeaveReq : HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.Button btnCancel, btnRemove, btnSrch;
        SAPbouiCOM.EditText txtEmpCode, txtEmpName;
        
        SAPbouiCOM.ComboBox cbPayrollPeriod;
        SAPbouiCOM.Item IcbPayrollPeriod;
        SAPbouiCOM.DataTable dtLeaveReq;
        SAPbouiCOM.Matrix grdLeaveReq;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clDN, clLT, clDDate, clLFr, clLTo, clCount, clDocS, clAprS,clIsSel;

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
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "btnSrch":
                        LoadLeaveRequestes();
                        break;
                    case "btnRem":
                        RemoveSelectedLeaveReq();
                        break;
                    case "2":
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
                btnRemove = oForm.Items.Item("btnRem").Specific;
                btnSrch = oForm.Items.Item("btnSrch").Specific;
                // Initialize TextBoxes

               // cbPayrollPeriod = oForm.Items.Item("cbPeriod").Specific;
                cbPayrollPeriod = oForm.Items.Item("cbPeriod").Specific;
                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPayrollPeriod.DataBind.SetBound(true, "", "cbPeriod");
                IcbPayrollPeriod = oForm.Items.Item("cbPeriod");

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
                dtLeaveReq.Columns.Add("IsSel", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);

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

                oColumn = oColumns.Item("isSel");
                clAprS = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "IsSel");
                

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void LoadSelectedData()
        {
            string pCode = txtEmpCode.Value;           
            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == pCode
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txtEmpName.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        //FillPayrollPeriods(getEmp.PayrollID.Value);
                        FillPeriod(getEmp.PayrollID.Value);
                        dtLeaveReq.Rows.Clear();
                        grdLeaveReq.LoadFromDataSource();
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ViewLeaveReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void LoadLeaveRequestes()
        {
            Int16 i = 0;
            int EmployeeID = 0;
            int periodId=Convert.ToInt32(cbPayrollPeriod.Value);
            string pCode = txtEmpCode.Value;
            if (!String.IsNullOrEmpty(pCode))
            {
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == pCode
                              select a).FirstOrDefault();
                EmployeeID = getEmp.ID;
            }
            try
            {
                CfgPeriodDates LeavePeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == periodId).FirstOrDefault();
                if (LeavePeriod != null && LeavePeriod.FlgLocked == true)
                {
                    oApplication.StatusBar.SetText("Selected period is Locked.please unlock period and try again", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                else
                {
                    IEnumerable<TrnsLeavesRequest> LeaveRequests = from p in dbHrPayroll.TrnsLeavesRequest where p.EmpID.ToString() == EmployeeID.ToString() && ((p.LeaveFrom >= LeavePeriod.StartDate && p.LeaveFrom <= LeavePeriod.EndDate) || (p.LeaveTo >= LeavePeriod.StartDate && p.LeaveTo <= LeavePeriod.EndDate)) select p;
                    //var LeaveRequests = from p in dbHrPayroll.TrnsLeavesRequest where p.EmpID.ToString() == EmployeeID.ToString() && ((p.LeaveFrom >= LeavePeriod.StartDate && p.LeaveFrom <= LeavePeriod.EndDate) || (p.LeaveTo >= LeavePeriod.StartDate && p.LeaveTo <= LeavePeriod.EndDate))  select p;                    
                    if (LeaveRequests != null && LeaveRequests.Count() > 0)
                    {
                        dtLeaveReq.Rows.Clear();
                        dtLeaveReq.Rows.Add(LeaveRequests.Count());
                        foreach (var WD in LeaveRequests)
                        {
                            var LeaveType = dbHrPayroll.MstLeaveType.Where(a => a.ID == WD.LeaveType).FirstOrDefault();
                            dtLeaveReq.SetValue("No", i, i + 1);
                            dtLeaveReq.SetValue("IsSel", i, "N");
                            dtLeaveReq.SetValue("DocNum", i, WD.DocNum);
                            dtLeaveReq.SetValue("LeaveType", i, LeaveType.Description);
                            dtLeaveReq.SetValue("DocDate", i, WD.DocDate);
                            dtLeaveReq.SetValue("LeaveFrom", i, WD.LeaveFrom);
                            dtLeaveReq.SetValue("LeaveTo", i, WD.LeaveTo);
                            dtLeaveReq.SetValue("Count", i, WD.TotalCount.ToString());
                            string docStatus = dbHrPayroll.MstLOVE.Where(l => l.Code == WD.DocStatus).FirstOrDefault().Value;
                            if (!string.IsNullOrEmpty(docStatus))
                            {
                                dtLeaveReq.SetValue("DocStatus", i, docStatus);
                            }
                            string approvalStatus = dbHrPayroll.MstLOVE.Where(l => l.Code == WD.DocAprStatus).FirstOrDefault().Value;
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
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);  
            }
        }
        //private void picEmp()
        //{
        //    PrepareSearchKeyHash();
        //    string strSql = sqlString.getSql("empLeaveReq", SearchKeyVal);
        //    picker pic = new picker(oApplication, ds.getDataTable(strSql));
        //    System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for View Leave");
        //    pic = null;
        //    if (st.Rows.Count > 0)
        //    {
        //        txtEmpCode.Value = st.Rows[0][0].ToString();
        //        LoadSelectedData();
        //    }
        //}
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("EmpID", txtEmpCode.Value.ToString());
        }
        private void FillPeriod(int payrollID)
        {
            try
            {
                // dtPeriods.Rows.Clear();
                if (cbPayrollPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPayrollPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPayrollPeriod.ValidValues.Remove(cbPayrollPeriod.ValidValues.Item(k).Value);
                    }
                }
                int i = 0;
                string selId = "0";
                bool flgPrevios = false;
                bool flgHit = false;
                int count = 0;
                int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID == payrollID select p).Count();
                if (cnt > 0)
                {
                    CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID == payrollID select p).Single();
                    foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    {
                        if (pd.FlgVisible == null ? false : (bool)pd.FlgVisible && pd.FlgLocked != true)
                        {
                            cbPayrollPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        }
                        count++;

                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();

                        if (Convert.ToBoolean(pd.FlgLocked))
                        {
                            selId = "0";
                            flgPrevios = true;
                        }
                        else
                        {
                            if (flgPrevios)
                            {
                                selId = pd.ID.ToString();
                                flgPrevios = false;
                            }
                        }

                        i++;
                    }
                    try
                    {
                        cbPayrollPeriod.Select(selId);
                        //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }
        private void FillPayrollPeriods(int payrollID)
        {
            try
            {
                if (cbPayrollPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPayrollPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPayrollPeriod.ValidValues.Remove(cbPayrollPeriod.ValidValues.Item(k).Value);
                    }
                }
                cbPayrollPeriod.ValidValues.Add("-1", "[Select One]");
                var Data = from v in dbHrPayroll.CfgPeriodDates where v.PayrollId == payrollID && v.FlgLocked == false select v;
                foreach (var v in Data)
                {
                    cbPayrollPeriod.ValidValues.Add(v.ID.ToString(), v.PeriodName.ToString());                    
                }
                cbPayrollPeriod.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillPayrollPeriods Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void RemoveSelectedLeaveReq()
        {
            string strDocNum = "";
            try
            {
                  int confirm = oApplication.MessageBox("Are you sure you want to remove selected leave(s)? ",1, "Yes", "No");
                  if (confirm == 2) return;
                  if (dtLeaveReq != null && dtLeaveReq.Rows.Count > 0)
                  {
                      for (int i = 0; i < dtLeaveReq.Rows.Count; i++)
                      {
                          bool sel2 = (grdLeaveReq.Columns.Item("isSel").Cells.Item(i + 1).Specific as SAPbouiCOM.CheckBox).Checked;
                          if (sel2)
                          {
                              strDocNum = Convert.ToString(dtLeaveReq.GetValue("DocNum", i));
                              if (!string.IsNullOrEmpty(strDocNum))
                              {
                                  TrnsLeavesRequest LeaveDoc = dbHrPayroll.TrnsLeavesRequest.Where(lr => lr.DocNum == Convert.ToInt32(strDocNum)).FirstOrDefault();
                                  if (LeaveDoc != null)
                                  {
                                      dbHrPayroll.TrnsLeavesRequest.DeleteOnSubmit(LeaveDoc);
                                  }
                              }
                          }
                      }
                      dbHrPayroll.SubmitChanges();
                      dtLeaveReq.Rows.Clear();
                      grdLeaveReq.LoadFromDataSource();
                      oApplication.StatusBar.SetText("Record Submited Successfully",SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                  }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("RemoveSelectedLeaveReq Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                //string comName = "Search";
                //Program.sqlString = "empLeaveReq";
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
                    LoadSelectedData();
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
