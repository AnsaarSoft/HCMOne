using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_AttAdj : HRMSBaseForm
    {
        SAPbouiCOM.EditText txDocNum, txDocDate, txFilenam,txSourceId;
        SAPbouiCOM.ComboBox cbProll, cbPeriod, cbStatus;
        SAPbouiCOM.Matrix mtEmp;

        SAPbouiCOM.Item ItxDocNum, ItxDocDate, ItxFilenam, ItxSourceId;
        SAPbouiCOM.Item IcbProll, IcbPeriod, IcbStatus;
        SAPbouiCOM.Item ImtEmp;

        SAPbouiCOM.DataTable dtEmps;
    
        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            oForm.DataSources.UserDataSources.Add("txDocNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txDocNum = oForm.Items.Item("txDocNum").Specific;
            ItxDocNum = oForm.Items.Item("txDocNum");
            txDocNum.DataBind.SetBound(true, "", "txDocNum");


            oForm.DataSources.UserDataSources.Add("txDocDate", SAPbouiCOM.BoDataType.dt_DATE, 30); // Days of Month
            txDocDate = oForm.Items.Item("txDocDate").Specific;
            ItxDocDate = oForm.Items.Item("txDocDate");
            txDocDate.DataBind.SetBound(true, "", "txDocDate");
           
            

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


            oForm.DataSources.UserDataSources.Add("txSourceId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txSourceId = oForm.Items.Item("txSourceId").Specific;
            ItxSourceId = oForm.Items.Item("txSourceId");
            txSourceId.DataBind.SetBound(true, "", "txSourceId");


          
             
            mtEmp = oForm.Items.Item("mtEmp").Specific;
            ImtEmp = oForm.Items.Item("mtEmp");

            dtEmps = oForm.DataSources.DataTables.Item("dtEmps");
            oForm.Freeze(false);

        }

    }
}
