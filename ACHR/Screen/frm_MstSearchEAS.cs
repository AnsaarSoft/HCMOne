using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace ACHR.Screen
{
    class frm_MstSearchEAS : HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.EditText txtSearch;
        SAPbouiCOM.Button btnSearch;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column id, serial, code, createdate;

        #endregion

        #region Form Events

        public override void  CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
 	        base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.ActiveItem = "txsearch";
            oForm.Freeze(false);

        }

        public override void etAfterDoubleClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterDoubleClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "mtmain")
            {
                if (pVal.Row >= 1 && pVal.Row <= grdMain.RowCount)
                {
                    string internalid = Convert.ToString(dtMain.GetValue(id.DataBind.Alias, pVal.Row - 1));
                    Program.EmpID = internalid;
                    this.Dispose();
                    this.oForm.Close();
                }
            }

        }

        #endregion

        #region Functions 

        private void InitiallizeForm()
        {
            try
            {
                btnSearch = oForm.Items.Item("btsearch").Specific;
                txtSearch = oForm.Items.Item("txsearch").Specific;
                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                id = grdMain.Columns.Item("id");
                serial = grdMain.Columns.Item("serial");
                code = grdMain.Columns.Item("code");
                createdate = grdMain.Columns.Item("cd");
                id.Visible = false;
                GetData();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance select a).ToList().OrderBy(a => a.CreateDate);
                if (oCollection.Count() > 0)
                {
                    int i = 0;
                    dtMain.Rows.Clear();
                    foreach (var Line in oCollection)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(id.DataBind.Alias, i, Line.InternalID);
                        dtMain.SetValue(serial.DataBind.Alias, i, i + 1);
                        dtMain.SetValue(code.DataBind.Alias, i, Line.DocNum.ToString());
                        dtMain.SetValue(createdate.DataBind.Alias, i, Line.CreateDate);
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

        #endregion

    }
}
