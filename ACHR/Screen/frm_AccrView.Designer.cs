using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_AccrView : HRMSBaseForm
    {
        SAPbouiCOM.EditText txEmpId, txEmpName, txAccrDate, txHrmsId;
        SAPbouiCOM.ComboBox cbLType;
        SAPbouiCOM.Matrix mtAccrual;

        SAPbouiCOM.Item ItxEmpId, ItxEmpName, ItxAccrDate, ItxHrmsId;
        SAPbouiCOM.Item IcbLType;
        SAPbouiCOM.Item ImtAccrual;


        SAPbouiCOM.DataTable dtAccrual;
    
        private void InitiallizeForm()
        {
            oForm.Freeze(true);


            oForm.DataSources.UserDataSources.Add("txEmpId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txEmpId = oForm.Items.Item("txEmpId").Specific;
            ItxEmpId = oForm.Items.Item("txEmpId");
            txEmpId.DataBind.SetBound(true, "", "txEmpId");

            oForm.DataSources.UserDataSources.Add("txHrmsId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txHrmsId = oForm.Items.Item("txHrmsId").Specific;
            ItxHrmsId = oForm.Items.Item("txHrmsId");
            txHrmsId.DataBind.SetBound(true, "", "txHrmsId");

            oForm.DataSources.UserDataSources.Add("txEmpName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txEmpName = oForm.Items.Item("txEmpName").Specific;
            ItxEmpName = oForm.Items.Item("txEmpName");
            txEmpName.DataBind.SetBound(true, "", "txEmpName");

            oForm.DataSources.UserDataSources.Add("txAccrDate", SAPbouiCOM.BoDataType.dt_DATE); // Days of Month
            txAccrDate = oForm.Items.Item("txAccrDate").Specific;
            ItxAccrDate = oForm.Items.Item("txAccrDate");
            txAccrDate.DataBind.SetBound(true, "", "txAccrDate");


            oForm.DataSources.UserDataSources.Add("cbLType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbLType = oForm.Items.Item("cbLType").Specific;
            IcbLType = oForm.Items.Item("cbLType");
            cbLType.DataBind.SetBound(true, "", "cbLType");

            mtAccrual = oForm.Items.Item("mtAccrual").Specific;

            dtAccrual = oForm.DataSources.DataTables.Item("dtAccrual");
            oForm.Freeze(false);

        }

    }
}
