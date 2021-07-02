using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using DIHRMS.Custom;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    partial class rpt_Viewer : HRMSBaseForm
    {
        public rpt_Viewer(SAPbouiCOM.Application app, System.Data.DataTable dt)
        {
            oApplication = app;
           
        }
    }
}
