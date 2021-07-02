using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    class frm_RetroPay : HRMSBaseForm
    {
        SAPbouiCOM.EditText txDocNum, txEmpFrom, txEmpTo;
        SAPbouiCOM.ComboBox cbProll, cbDept, cbLoc, cbRetEle, cbPFrom, cbPTo;
        SAPbouiCOM.Button btVoid;

        SAPbouiCOM.Item ItxDocNum, ItxEmpFrom, ItxEmpTo;
        SAPbouiCOM.Item IcbProll, IcbDept, IcbLoc, IcbRetEle, IcbPFrom, IcbPTo;
        SAPbouiCOM.Item IbtVoid;

        SAPbouiCOM.DataTable dtEmps;
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            btVoid = oForm.Items.Item("btVoid").Specific;
            IbtVoid = oForm.Items.Item("btVoid");

            oForm.DataSources.UserDataSources.Add("txDocNum", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 30); // Days of Month
            txDocNum = oForm.Items.Item("txDocNum").Specific;
            ItxDocNum = oForm.Items.Item("txDocNum");
            txDocNum.DataBind.SetBound(true, "", "txDocNum");

            oForm.DataSources.UserDataSources.Add("txEmpFrom", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 30); // Days of Month
            txEmpFrom = oForm.Items.Item("txEmpFrom").Specific;
            ItxEmpFrom = oForm.Items.Item("txEmpFrom");
            txEmpFrom.DataBind.SetBound(true, "", "txEmpFrom");

            oForm.DataSources.UserDataSources.Add("txEmpTo", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 30); // Days of Month
            txEmpTo = oForm.Items.Item("txEmpTo").Specific;
            ItxEmpTo = oForm.Items.Item("txEmpTo");
            txEmpTo.DataBind.SetBound(true, "", "txEmpTo");

            oForm.DataSources.UserDataSources.Add("cbProll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbProll = oForm.Items.Item("cbProll").Specific;
            IcbProll = oForm.Items.Item("cbProll");
            cbProll.DataBind.SetBound(true, "", "cbProll");

             oForm.DataSources.UserDataSources.Add("cbDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbDept = oForm.Items.Item("cbDept").Specific;
            IcbDept = oForm.Items.Item("cbDept");
            cbDept.DataBind.SetBound(true, "", "cbDept");

             oForm.DataSources.UserDataSources.Add("cbLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbLoc = oForm.Items.Item("cbLoc").Specific;
            IcbLoc = oForm.Items.Item("cbLoc");
            cbLoc.DataBind.SetBound(true, "", "cbLoc");

              oForm.DataSources.UserDataSources.Add("cbRetEle", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbRetEle = oForm.Items.Item("cbRetEle").Specific;
            IcbRetEle = oForm.Items.Item("cbRetEle");
            cbRetEle.DataBind.SetBound(true, "", "cbRetEle");

             oForm.DataSources.UserDataSources.Add("cbPFrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbPFrom = oForm.Items.Item("cbPFrom").Specific;
            IcbPFrom = oForm.Items.Item("cbPFrom");
            cbPFrom.DataBind.SetBound(true, "", "cbPFrom");

             oForm.DataSources.UserDataSources.Add("cbPTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            cbPTo = oForm.Items.Item("cbPTo").Specific;
            IcbPTo = oForm.Items.Item("cbPTo");
            cbPFrom.DataBind.SetBound(true, "", "cbPTo");


            oForm.Freeze(false);
            fillCbs();

        }
        private void fillCbs()
        {
            int i = 0;
            string selId = "0";
            IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
            foreach (CfgPayrollDefination pr in prs)
            {
                cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                i++;
            }
            cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            FillPeriod(cbProll.Value);

            IEnumerable<MstDepartment> depts = from p in dbHrPayroll.MstDepartment select p;
            cbDept.ValidValues.Add("0", "All");
            foreach (MstDepartment dept in depts)
            {
                cbDept.ValidValues.Add(dept.ID.ToString(), dept.DeptName);

            }
            cbDept.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            cbLoc.ValidValues.Add("0", "All");
            IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation select p;

            foreach (MstLocation loc in locs)
            {
                cbLoc.ValidValues.Add(loc.Id.ToString(), loc.Description);

            }
            cbLoc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            IEnumerable<MstRetroElementSet> rtEls = from p in dbHrPayroll.MstRetroElementSet   select p ;

            foreach (MstRetroElementSet rtEl in rtEls)
            {
                cbRetEle.ValidValues.Add(rtEl.Id.ToString(), rtEl.RetroSetName);

            }
            cbRetEle.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }
        private void FillPeriod(string payroll)
        {
            if (cbPFrom.ValidValues.Count > 0)
            {
                int vcnt = cbPFrom.ValidValues.Count;
                for (int k = vcnt - 1; k >= 0; k--)
                {
                    cbPFrom.ValidValues.Remove(cbPFrom.ValidValues.Item(k).Value);
                    cbPTo.ValidValues.Remove(cbPFrom.ValidValues.Item(k).Value);
                    
                }
            }
            int i = 0;
            string selId = "0";
            int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Count();
            if (cnt > 0)
            {
                CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Single();
                foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                {
                    cbPFrom.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                    cbPTo.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());


                    if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                    {
                        selId = pd.ID.ToString();
                    }

                    i++;
                }
                try
                {
                    cbPFrom.Select(selId);
                    cbPTo.Select(selId);
                 
                }
                catch { }
            }

        }


    }
       
}
