using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_DT : HRMSBaseForm
    {
        SAPbouiCOM.EditText txDb, txFilenam, txAccrDate, txHrmsId;
        SAPbouiCOM.ComboBox cbImpObj;
        SAPbouiCOM.Matrix mtObj;
        SAPbouiCOM.Button btPick, btPull, btPrg, btImport;
        SAPbouiCOM.OptionBtn optNew;
        SAPbouiCOM.Item ItxDb, ItxFilenam, ItxAccrDate, ItxHrmsId;
        SAPbouiCOM.Item IcbImpObj;
        SAPbouiCOM.Item ImtObj;
        SAPbouiCOM.Item IbtPick, IbtPull, IbtPrg, IbtImport;


        SAPbouiCOM.DataTable dtHead, dtMat;
    
        private void InitiallizeForm()
        {
            try
            {
                oForm.Freeze(true);

                cbImpObj = oForm.Items.Item("cbImpObj").Specific;
                oForm.DataSources.UserDataSources.Add("cbImpObj", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbImpObj.DataBind.SetBound(true, "", "cbImpObj");
                IcbImpObj = oForm.Items.Item("cbImpObj");

                dtHead = oForm.DataSources.DataTables.Item("dtHead");
                dtMat = oForm.DataSources.DataTables.Item("dtMat");
                mtObj = oForm.Items.Item("mtObj").Specific;
                ImtObj = oForm.Items.Item("mtObj");

                dtHead.Rows.Add(1);
                //optNew = oForm.Items.Item("optNew").Specific;
                //optOlddb = oForm.Items.Item("optOlddb").Specific;
                //optOlddb.GroupWith("optNew");
                //optOlddb.Selected = true;
                //dtHead.SetValue("optold", 0, "Y");
                //IbtPrg = oForm.Items.Item("btPrg");
                //txDb = oForm.Items.Item("txDb").Specific;
                btImport = oForm.Items.Item("btImport").Specific;
                IbtImport = oForm.Items.Item("btImport");
                txFilenam = oForm.Items.Item("txFilenam").Specific;
                ItxFilenam = oForm.Items.Item("txFilenam");
                //ItxFilenam.Enabled = false;
                //IbtPrg.Width = 0;
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {                
            }

        }

    }
}
