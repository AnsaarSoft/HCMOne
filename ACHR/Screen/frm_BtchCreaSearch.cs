using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace ACHR.Screen
{
    class frm_BtchCreaSearch : HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.EditText txtSearch;
        SAPbouiCOM.Button btnSearch;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column DocumentNo, PayrollName, code, PeriodName;


        #endregion

        #region Form Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            //oForm.ActiveItem = "txsearch";
            oForm.Freeze(false);

        }

        public override void etAfterDoubleClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterDoubleClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "mtmain")
            {
                if (pVal.Row >= 1 && pVal.Row <= grdMain.RowCount)
                {
                    string internalid = Convert.ToString(dtMain.GetValue(DocumentNo.DataBind.Alias, pVal.Row - 1));
                    Program.EmpID = internalid;
                    this.Dispose();
                    this.oForm.Close();
                }
            }
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btsearch":
                        //FilterRecord();
                        //GetBatchByFilterExpresion();
                        break;
                    default:
                        break;
                }
                if (pVal.ItemUID == "mtmain")
                {
                    //if (pVal.Row >= 1 && pVal.Row <= grdMain.RowCount)
                    //{
                    //    string internalid = Convert.ToString(dtMain.GetValue(DocumentNo.DataBind.Alias, pVal.Row - 1));
                    //    Program.EmpID = internalid;
                    //    this.Dispose();
                    //    this.oForm.Close();
                    //}
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_MstShift Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region Functions 

        private void InitiallizeForm()
        {
            try
            {
                //btnSearch = oForm.Items.Item("btsearch").Specific;
                //txtSearch = oForm.Items.Item("txsearch").Specific;
                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");

                DocumentNo = grdMain.Columns.Item("id");
                code = grdMain.Columns.Item("code");
                PeriodName = grdMain.Columns.Item("prd");
                PayrollName = grdMain.Columns.Item("prol");

                //DocumentNo.Visible = false;

                GetData();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData1()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.TrnsBatches where a.CfgPeriodDates.FlgLocked == false select a).ToList().OrderBy(a => a.CreateDate);
                if (oCollection.Count() > 0)
                {
                    int i = 0;
                    dtMain.Rows.Clear();
                    foreach (var Line in oCollection)
                    {
                        dtMain.Rows.Add(1);

                        dtMain.SetValue(DocumentNo.DataBind.Alias, i, Line.Id);
                        dtMain.SetValue(code.DataBind.Alias, i, Line.BatchName);
                        dtMain.SetValue(PayrollName.DataBind.Alias, i, Line.PayrollName);
                        dtMain.SetValue(PeriodName.DataBind.Alias, i, Line.PayrollPeriod);
                        i++;
                    }
                    grdMain.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetData()
        {
            try
            {
                string strOut = string.Empty;
                var oCollection = (from a in dbHrPayroll.TrnsBatches where a.CfgPeriodDates.FlgLocked == false select a).ToList().OrderBy(a => a.CreateDate);
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                    strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                    if (!string.IsNullOrEmpty(strOut))
                    {
                        string[] ConfigurePayroll = strOut.Split(',');
                        dtMain.Rows.Clear();
                        int i = 0;
                        foreach (string onePayroll in ConfigurePayroll)
                        {
                            var oPayroll = (from a in dbHrPayroll.CfgPayrollDefination
                                            where a.ID.ToString() == onePayroll
                                            select a).FirstOrDefault();
                            var oCollection1 = oCollection.Where(p => p.PayrollID == oPayroll.ID).ToList().OrderBy(e => e.CreateDate);
                            if (oCollection.Count() > 0)
                            {
                                foreach (var Line in oCollection1)
                                {
                                    dtMain.Rows.Add(1);
                                    dtMain.SetValue(DocumentNo.DataBind.Alias, i, Line.Id);
                                    dtMain.SetValue(code.DataBind.Alias, i, Line.BatchName);
                                    dtMain.SetValue(PayrollName.DataBind.Alias, i, Line.PayrollName);
                                    dtMain.SetValue(PeriodName.DataBind.Alias, i, Line.PayrollPeriod);
                                    i++;
                                }
                            }
                        }
                        grdMain.LoadFromDataSource();
                    }
                    else
                    {
                        if (oCollection.Count() > 0)
                        {
                            int i = 0;
                            dtMain.Rows.Clear();
                            foreach (var Line in oCollection)
                            {
                                dtMain.Rows.Add(1);
                                dtMain.SetValue(DocumentNo.DataBind.Alias, i, Line.Id);
                                dtMain.SetValue(code.DataBind.Alias, i, Line.BatchName);
                                dtMain.SetValue(PayrollName.DataBind.Alias, i, Line.PayrollName);
                                dtMain.SetValue(PeriodName.DataBind.Alias, i, Line.PayrollPeriod);
                                i++;
                            }
                            grdMain.LoadFromDataSource();
                        }
                    }
                }
                else
                {
                    if (oCollection.Count() > 0)
                    {
                        int i = 0;
                        dtMain.Rows.Clear();
                        foreach (var Line in oCollection)
                        {
                            dtMain.Rows.Add(1);
                            dtMain.SetValue(DocumentNo.DataBind.Alias, i, Line.Id);
                            dtMain.SetValue(code.DataBind.Alias, i, Line.BatchName);
                            dtMain.SetValue(PayrollName.DataBind.Alias, i, Line.PayrollName);
                            dtMain.SetValue(PeriodName.DataBind.Alias, i, Line.PayrollPeriod);
                            i++;
                        }
                        grdMain.LoadFromDataSource();
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GetData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FilterRecord()
        {
            try
            {
                DataTable dt = new DataTable();

                string strValue = txtSearch.Value.ToLower();
                string strSql = sqlString.getSql(Program.sqlString, SearchKeyVal);
                dt = ds.getDataTable(strSql);

                DataView dv = dt.DefaultView;

                if (dt != null && dt.Rows.Count > 0)
                {

                    dt = dv.ToTable();
                    dtMain.Rows.Clear();
                    dtMain.Rows.Add(dt.Rows.Count);
                    for (int K = 0; K < dt.Rows.Count; K++)
                    {
                        dtMain.SetValue("No", K, K + 1);
                        dtMain.SetValue("txsearch", K, string.IsNullOrEmpty(strValue) ? "" : strValue);

                    }
                    grdMain.LoadFromDataSource();
                    //for set focus on matrix




                }



            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FilterRecord Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void GetBatchByFilterExpresion()
        {

            try
            {
                string BatchName = txtSearch.Value.Trim();
                //var BatchID = dbHrPayroll.TrnsBatches.Where(s => s.BatchName == BatchName).FirstOrDefault();
                if (BatchName != null)
                {
                    var oCollection = (from a in dbHrPayroll.TrnsBatches where a.BatchName == BatchName select a).ToList().OrderBy(a => a.CreateDate);
                    if (oCollection != null)
                    {
                        int i = 0;
                        dtMain.Rows.Clear();
                        foreach (var Line in oCollection)
                        {
                            dtMain.Rows.Add(1);

                            dtMain.SetValue(DocumentNo.DataBind.Alias, i, Line.Id);
                            dtMain.SetValue(code.DataBind.Alias, i, Line.BatchName);
                            dtMain.SetValue(PayrollName.DataBind.Alias, i, Line.PayrollName);
                            dtMain.SetValue(PeriodName.DataBind.Alias, i, Line.PayrollPeriod);
                            i++;
                        }
                        grdMain.LoadFromDataSource();


                    }
                }
                else
                {
                    GetData();
                }

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }

            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }


        #endregion

    }
}
