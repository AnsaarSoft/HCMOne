using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using DIHRMS.Custom;


namespace ACHR.Screen
{
    partial class frm_ApprDesc : HRMSBaseForm
    {
        IEnumerable<CfgApprovalDecisionRegister> pendingDocs;
        SAPbouiCOM.DataTable dtLeaveReq;
        SAPbouiCOM.Matrix grdLeaveReq;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clDN, clLT, clDDate, clLFr, clLTo, clCount, clCode, clName;
        SAPbouiCOM.Item IgrdLeaveReq;

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "1")
            {
                submitform();
            }
            if (pVal.ItemUID == "mtDecision")
            {

                if (pVal.Row >= 1 && pVal.Row <= mtDecision.RowCount)
                {
                    try
                    {
                        string id = Convert.ToString(dtAlerts.GetValue("docNum", pVal.Row - 1));
                        string docType = Convert.ToString(dtAlerts.GetValue("docType", pVal.Row - 1));
                        if (docType.Trim() == "LeaveRequest" && !string.IsNullOrEmpty(id))
                        {
                            IgrdLeaveReq.Visible = true;
                            LoadLeaveRequestes(Convert.ToInt32(id));                        
                        }
                        else
                        {
                            IgrdLeaveReq.Visible = false;
                        }
                        
                    }
                    catch
                    {
                        // iniSalaryDetail();
                    }
                }
            }
        }
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            fillPendingDocs();
        }
        private void fillPendingDocs()
        {
            dtAlerts.Rows.Clear();
            pendingDocs = from p in dbHrPayroll.CfgApprovalDecisionRegister where p.EmpID == oCompany.UserName &&  Convert.ToBoolean(p.FlgActive)  &&  p.LineStatusID=="LV0005"  select p;

            if (pendingDocs != null && pendingDocs.Count() <= 0)
            {
                pendingDocs = from p in dbHrPayroll.CfgApprovalDecisionRegister where p.EmpID == oCompany.UserSignature.ToString() && Convert.ToBoolean(p.FlgActive) && p.LineStatusID == "LV0005" select p;
            }

            int i=0;
            foreach (CfgApprovalDecisionRegister pendingDoc in pendingDocs)
            {

                dtAlerts.Rows.Add(1);
                dtAlerts.SetValue("id", i, pendingDoc.ID.ToString());
                dtAlerts.SetValue("docType", i, getObjectType( pendingDoc.DocType.ToString()));
                dtAlerts.SetValue("docNum", i, pendingDoc.DocNum.ToString());
                dtAlerts.SetValue("Status", i, pendingDoc.LineStatusID.ToString());
                try
                {
                    dtAlerts.SetValue("Remarks", i, pendingDoc.Remarks.ToString());
                }
                catch { }
                i++;
            }
            mtDecision.LoadFromDataSource();

        }
        private string getObjectType(string docId)
        {
           string outResult ="";
           int cnt = (from p in dbHrPayroll.CfgDocumentTypes where p.DocType.ToString() == docId select p).Count();

           if(cnt>0)
           {
               CfgDocumentTypes doctype=  (from p in dbHrPayroll.CfgDocumentTypes where p.DocType.ToString() == docId select p).Single();
               outResult = doctype.DocName; 
           }

            return outResult;

        }
        private void submitform()
        {
            mtDecision.FlushToDataSource();
            for (int i = 0; i < dtAlerts.Rows.Count; i++)
            {
                string id = dtAlerts.GetValue("id", i);
                CfgApprovalDecisionRegister dr = (from p in dbHrPayroll.CfgApprovalDecisionRegister where p.ID.ToString() == id.ToString() select p).Single();
                dr.LineStatusID = dtAlerts.GetValue("Status", i);
                dr.Remarks = dtAlerts.GetValue("Remarks", i);
            }
            dbHrPayroll.SubmitChanges();
            fillPendingDocs();
            

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
                dtLeaveReq.Columns.Add("EmpCode", SAPbouiCOM.BoFieldsType.ft_Text);
                dtLeaveReq.Columns.Add("EName", SAPbouiCOM.BoFieldsType.ft_Text);


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

                oColumn = oColumns.Item("ECode");
                clCode = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "EmpCode");

                oColumn = oColumns.Item("EName");
                clName = oColumn;
                oColumn.DataBind.Bind("LeaveRequest", "EName");

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void LoadLeaveRequestes(int DocNum)
        {
            Int16 i = 0;
            try
            {
                var LeaveRequests = dbHrPayroll.TrnsLeavesRequest.Where(lr => lr.DocNum == DocNum).ToList();
                if (LeaveRequests != null && LeaveRequests.Count > 0)
                {
                    dtLeaveReq.Rows.Clear();
                    dtLeaveReq.Rows.Add(LeaveRequests.Count());
                    foreach (var WD in LeaveRequests)
                    {
                        var LeaveType = dbHrPayroll.MstLeaveType.Where(a => a.ID == WD.LeaveType).FirstOrDefault();
                        dtLeaveReq.SetValue("No", i, i + 1);
                        dtLeaveReq.SetValue("EmpCode", i, WD.MstEmployee.EmpID);
                        dtLeaveReq.SetValue("EName", i, WD.MstEmployee.FirstName + " " + WD.MstEmployee.MiddleName + " " + WD.MstEmployee.LastName);
                        dtLeaveReq.SetValue("DocNum", i, WD.DocNum);
                        dtLeaveReq.SetValue("LeaveType", i, LeaveType.Description);
                        dtLeaveReq.SetValue("DocDate", i, WD.DocDate);
                        dtLeaveReq.SetValue("LeaveFrom", i, WD.LeaveFrom);
                        dtLeaveReq.SetValue("LeaveTo", i, WD.LeaveTo);
                        dtLeaveReq.SetValue("Count", i, WD.TotalCount.ToString());
                                                
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
    }
    

}
