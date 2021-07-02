using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_ApprDesc : HRMSBaseForm
    {
        SAPbouiCOM.Matrix mtDecision;

        SAPbouiCOM.Item ImtDecision;

        private SAPbouiCOM.DataTable dtAlerts;
        private SAPbouiCOM.DataTable dtInfo;


        
        private void InitiallizeForm()
        {
            oForm.Freeze(true);



            dtAlerts = oForm.DataSources.DataTables.Item("dtAlerts");
            //dtInfo = oForm.DataSources.DataTables.Item("dtInfo");
            mtDecision = oForm.Items.Item("mtDecision").Specific;
            IgrdLeaveReq = oForm.Items.Item("grdLeaves");
            //mtDocInfo = oForm.Items.Item("mtDocInfo").Specific;
            oForm.Freeze(false);
            mtDecision.Columns.Item("id").Visible = false;
            fillColumCombo("ApprovalStatus", mtDecision.Columns.Item("cbStatus"));
            InitiallizegridMatrix();
            IgrdLeaveReq.Visible = false;
        }

    }
}
