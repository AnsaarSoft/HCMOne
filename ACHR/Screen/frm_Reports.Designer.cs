using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_Reports : HRMSBaseForm
    {
        SAPbouiCOM.EditText txCode, txName, txMenu, txFilenam;
        SAPbouiCOM.Matrix mtReports;
        SAPbouiCOM.Column mCode, mId, mName, mMenu, mEmployee, mDepartment, mLocation, mDateFrom, mDateTo, mPreviousPeriod, mPeriod, mCritaria;
        SAPbouiCOM.Button btPick;
        SAPbouiCOM.Item ItxCode, ItxName, ItxMenu, ItxFilenam;
        SAPbouiCOM.Item ImtReports;
        SAPbouiCOM.Item IbtPick;
        SAPbouiCOM.CheckBox chkEmployee, chkDepartment, chkLocation, chkDateFrom, chkDateTo, chkPreviousPeriod, chkPeriod, chkCritaria;

        SAPbouiCOM.DataTable  dtMat;

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            dtMat = oForm.DataSources.DataTables.Item("dtMat");
            mtReports = oForm.Items.Item("mtReports").Specific;
            mCode = mtReports.Columns.Item("Code");
            mId = mtReports.Columns.Item("ID");
            mName = mtReports.Columns.Item("Name");
            mMenu = mtReports.Columns.Item("Menu");
            mEmployee = mtReports.Columns.Item("emp");
            mDepartment = mtReports.Columns.Item("dept");
            mLocation = mtReports.Columns.Item("loc");
            mDateFrom = mtReports.Columns.Item("dtfrom");
            mDateTo = mtReports.Columns.Item("dtto");

            mPeriod = mtReports.Columns.Item("prd");
            mPreviousPeriod = mtReports.Columns.Item("pprd");
            mCritaria = mtReports.Columns.Item("Crite");

            mMenu.Visible = false;

            oForm.DataSources.UserDataSources.Add("txFilenam", SAPbouiCOM.BoDataType.dt_SHORT_TEXT , 200); // Hours Per Day
            txFilenam = oForm.Items.Item("txFilenam").Specific;
            ItxFilenam = oForm.Items.Item("txFilenam");
            txFilenam.DataBind.SetBound(true, "", "txFilenam");
            ItxFilenam.Enabled = false;

            oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
            txCode = oForm.Items.Item("txCode").Specific;
            ItxCode = oForm.Items.Item("txCode");
            txCode.DataBind.SetBound(true, "", "txCode");

            oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
            txName = oForm.Items.Item("txName").Specific;
            ItxName = oForm.Items.Item("txName");
            txName.DataBind.SetBound(true, "", "txName");

            oForm.DataSources.UserDataSources.Add("txMenu", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
            txMenu = oForm.Items.Item("txMenu").Specific;
            ItxMenu = oForm.Items.Item("txMenu");
            txMenu.DataBind.SetBound(true, "", "txMenu");

            chkEmployee = oForm.Items.Item("chkemp").Specific;
            oForm.DataSources.UserDataSources.Add("chkemp", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkEmployee.DataBind.SetBound(true, "", "chkemp");

            chkDepartment = oForm.Items.Item("chkdept").Specific;
            oForm.DataSources.UserDataSources.Add("chkdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkDepartment.DataBind.SetBound(true, "", "chkdept");

            chkLocation = oForm.Items.Item("chkloc").Specific;
            oForm.DataSources.UserDataSources.Add("chkloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkLocation.DataBind.SetBound(true, "", "chkloc");

            chkDateFrom = oForm.Items.Item("chkdtfrom").Specific;
            oForm.DataSources.UserDataSources.Add("chkdtfrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkDateFrom.DataBind.SetBound(true, "", "chkdtfrom");

            chkDateTo = oForm.Items.Item("chkdtto").Specific;
            oForm.DataSources.UserDataSources.Add("chkdtto", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkDateTo.DataBind.SetBound(true, "", "chkdtto");

            chkPreviousPeriod = oForm.Items.Item("chkPPrd").Specific;
            oForm.DataSources.UserDataSources.Add("chkPPrd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkPreviousPeriod.DataBind.SetBound(true, "", "chkPPrd");

            chkPeriod = oForm.Items.Item("chkPrd").Specific;
            oForm.DataSources.UserDataSources.Add("chkPrd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkPeriod.DataBind.SetBound(true, "", "chkPrd");

            chkCritaria = oForm.Items.Item("chkCrt").Specific;
            oForm.DataSources.UserDataSources.Add("chkCrt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
            chkCritaria.DataBind.SetBound(true, "", "chkCrt");

            mtReports.AutoResizeColumns();
            oForm.Freeze(false);

        }

    }
}
