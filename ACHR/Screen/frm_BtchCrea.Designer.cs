using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_BtchCrea : HRMSBaseForm
    {
        SAPbouiCOM.EditText txDocNum, txBchName, txElCode, txElName, txEleType, txValue, txEmprCont, txEmpCont, txEff, txFilenam;
        SAPbouiCOM.ComboBox cbProll, cbPeriod, cbStatus, cbIfExst, cbCntValT, cbValType;
        SAPbouiCOM.Matrix mtEmp;

        SAPbouiCOM.Item ItxDocNum, ItxBchName, ItxElCode, ItxElName, ItxEleType, ItxValue, ItxEmprCont, ItxEmpCont, ItxEff, ItxFilenam;
        SAPbouiCOM.Item IcbProll, IcbPeriod, IcbStatus, IcbIfExst, IcbCntValT, IcbValType;
        SAPbouiCOM.Item ImtEmp, IbtProcess;

        SAPbouiCOM.DataTable dtEmps;
    
        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            IbtProcess = oForm.Items.Item("btProcess");

            oForm.DataSources.UserDataSources.Add("txDocNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txDocNum = oForm.Items.Item("txDocNum").Specific;
            ItxDocNum = oForm.Items.Item("txDocNum");
            txDocNum.DataBind.SetBound(true, "", "txDocNum");

            oForm.DataSources.UserDataSources.Add("txBchName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txBchName = oForm.Items.Item("txBchName").Specific;
            ItxBchName = oForm.Items.Item("txBchName");
            txBchName.DataBind.SetBound(true, "", "txBchName");

            oForm.DataSources.UserDataSources.Add("txElCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txElCode = oForm.Items.Item("txElCode").Specific;
            ItxElCode = oForm.Items.Item("txElCode");
            txElCode.DataBind.SetBound(true, "", "txElCode");

            oForm.DataSources.UserDataSources.Add("txElName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txElName = oForm.Items.Item("txElName").Specific;
            ItxElName = oForm.Items.Item("txElName");
            txElName.DataBind.SetBound(true, "", "txElName");

            oForm.DataSources.UserDataSources.Add("txEleType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txEleType = oForm.Items.Item("txEleType").Specific;
            ItxEleType = oForm.Items.Item("txEleType");
            txEleType.DataBind.SetBound(true, "", "txEleType");

            oForm.DataSources.UserDataSources.Add("txEff", SAPbouiCOM.BoDataType.dt_DATE); // Days of Month
            txEff = oForm.Items.Item("txEff").Specific;
            ItxEff = oForm.Items.Item("txEff");
            txEff.DataBind.SetBound(true, "", "txEff");

            oForm.DataSources.UserDataSources.Add("txValue", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txValue = oForm.Items.Item("txValue").Specific;
            ItxValue = oForm.Items.Item("txValue");
            txValue.DataBind.SetBound(true, "", "txValue");

            oForm.DataSources.UserDataSources.Add("txEmprCont", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txEmprCont = oForm.Items.Item("txEmprCont").Specific;
            ItxEmprCont = oForm.Items.Item("txEmprCont");
            txEmprCont.DataBind.SetBound(true, "", "txEmprCont");

            oForm.DataSources.UserDataSources.Add("txEmpCont", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txEmpCont = oForm.Items.Item("txEmpCont").Specific;
            ItxEmpCont = oForm.Items.Item("txEmpCont");
            txEmpCont.DataBind.SetBound(true, "", "txEmpCont");

            oForm.DataSources.UserDataSources.Add("txFilenam", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,254); // Days of Month
            txFilenam = oForm.Items.Item("txFilenam").Specific;
            ItxFilenam = oForm.Items.Item("txFilenam");
            txFilenam.DataBind.SetBound(true, "", "txFilenam");
            
            oForm.DataSources.UserDataSources.Add("cbProll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbProll = oForm.Items.Item("cbProll").Specific;
            IcbProll = oForm.Items.Item("cbProll");
            cbProll.DataBind.SetBound(true, "", "cbProll");

            oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbPeriod = oForm.Items.Item("cbPeriod").Specific;
            IcbPeriod = oForm.Items.Item("cbPeriod");
            cbPeriod.DataBind.SetBound(true, "", "cbPeriod");
            
            oForm.DataSources.UserDataSources.Add("cbStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbStatus = oForm.Items.Item("cbStatus").Specific;
            IcbStatus = oForm.Items.Item("cbStatus");
            cbStatus.DataBind.SetBound(true, "", "cbStatus");

            oForm.DataSources.UserDataSources.Add("cbIfExst", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbIfExst = oForm.Items.Item("cbIfExst").Specific;
            IcbIfExst = oForm.Items.Item("cbIfExst");
            cbIfExst.DataBind.SetBound(true, "", "cbIfExst");

            oForm.DataSources.UserDataSources.Add("cbValType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbValType = oForm.Items.Item("cbValType").Specific;
            IcbValType = oForm.Items.Item("cbValType");
            cbValType.DataBind.SetBound(true, "", "cbValType");

           
            mtEmp = oForm.Items.Item("mtEmp").Specific;
            ImtEmp = oForm.Items.Item("mtEmp");

            dtEmps = oForm.DataSources.DataTables.Item("dtEmps");
            oForm.Items.Item("btProcess").Enabled = false;
            oForm.Freeze(false);

        }

    }
}
