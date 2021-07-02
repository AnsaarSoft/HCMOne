using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    partial class rpt_Viewer : HRMSBaseForm
    {
        private SAPbouiCOM.Application oApplication;
        private string sInput = "";
        private string sTitle = "";
        private bool bLoadInputEvents;
        private System.Data.DataTable dtTable;
        private System.Data.DataTable dtOut = new System.Data.DataTable();
        private SAPbouiCOM.DataTable dtSearch;
        SAPbouiCOM.Matrix mtSearch;
        SAPbouiCOM.Form oform;
        SAPbouiCOM.Item IbtChoos;
        SAPbouiCOM.Button btChoos;
        private void InitiallizeForm()
        {
           

           

        }

    }
}
