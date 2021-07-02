using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    partial class frm_TxSetup : HRMSBaseForm
    {

        #region Variables


        SAPbouiCOM.Matrix mtItax;
        SAPbouiCOM.EditText txMinSal, txSenCit, txMxSalDis, txDisc;
        SAPbouiCOM.ComboBox cbYear;
        SAPbouiCOM.Item ItxMinSal, ItxSenCit, ItxMxSalDis, ItxDisc, IcbYear;

        SAPbouiCOM.Column isNew, id;

        private SAPbouiCOM.DataTable dtTax;

        #endregion

        #region SAP B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);

        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);

            if (pVal.ItemUID == "cbYear")
            {
                string selYear = cbYear.Value.ToString().Trim();
                getYearTax(selYear);
            }


        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    submitForm();
                    break;
                case "btCopy":
                    pickTaxSlabforCopy();
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == BoFormMode.fm_ADD_MODE || oForm.Mode == BoFormMode.fm_UPDATE_MODE)
                    {
                        if (!AddValidation())
                        {
                            BubbleEvent = false;
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Function

        private void IniContrls()
        {
            txMinSal.Value = "";
            txSenCit.Value = "";
            txMxSalDis.Value = "";
            txDisc.Value = "";


            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            addEmptyRow();

        }

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            //EachItemshould be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the controll object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */
            mtItax = oForm.Items.Item("mtItax").Specific;
            isNew = mtItax.Columns.Item("isNew");
            id = mtItax.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            dtTax = oForm.DataSources.DataTables.Item("dtTax");
            dtTax.Rows.Clear();

            //, , , , 

            oForm.DataSources.UserDataSources.Add("txMinSal", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txMinSal = oForm.Items.Item("txMinSal").Specific;
            ItxMinSal = oForm.Items.Item("txMinSal");
            txMinSal.DataBind.SetBound(true, "", "txMinSal");
            //, , , 
            oForm.DataSources.UserDataSources.Add("txSenCit", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Days of Month
            txSenCit = oForm.Items.Item("txSenCit").Specific;
            ItxSenCit = oForm.Items.Item("txSenCit");
            txSenCit.DataBind.SetBound(true, "", "txSenCit");



            oForm.DataSources.UserDataSources.Add("txMxSalDis", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
            txMxSalDis = oForm.Items.Item("txMxSalDis").Specific;
            ItxMxSalDis = oForm.Items.Item("txMxSalDis");
            txMxSalDis.DataBind.SetBound(true, "", "txMxSalDis");

            oForm.DataSources.UserDataSources.Add("txDisc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txDisc = oForm.Items.Item("txDisc").Specific;
            ItxDisc = oForm.Items.Item("txDisc");
            txDisc.DataBind.SetBound(true, "", "txDisc");

            oForm.DataSources.UserDataSources.Add("cbYear", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            cbYear = oForm.Items.Item("cbYear").Specific;
            IcbYear = oForm.Items.Item("cbYear");
            cbYear.DataBind.SetBound(true, "", "cbYear");

            //getData();
            fillSalYear();
            addEmptyRow();

            IniContrls();
            oForm.Freeze(false);

        }

        private void addEmptyRow()
        {


            if (dtTax.Rows.Count == 0)
            {
                //id,isNew,Code,MinAmt,MaxAmt,fixTerm,Value,Descr
                dtTax.Rows.Add(1);

                dtTax.SetValue("id", 0, "0");
                dtTax.SetValue("isNew", 0, "Y");
                dtTax.SetValue("Code", 0, "");
                dtTax.SetValue("MinAmt", 0, "0.00");
                dtTax.SetValue("MaxAmt", 0, "0.00");
                dtTax.SetValue("fixTerm", 0, "0.00");
                dtTax.SetValue("Value", 0, "0.00");
                dtTax.SetValue("AddVal", 0, "0.00");
                dtTax.SetValue("Descr", 0, "");
                mtItax.AddRow(1, mtItax.RowCount + 1);
            }
            else
            {
                if (dtTax.GetValue("Code", dtTax.Rows.Count - 1) == "")
                {
                }
                else
                {
                    //id,isNew,Code,MinAmt,MaxAmt,fixTerm,Value,Descr
                    dtTax.Rows.Add(1);
                    dtTax.SetValue("id", dtTax.Rows.Count - 1, "0");
                    dtTax.SetValue("isNew", dtTax.Rows.Count - 1, "Y");
                    dtTax.SetValue("Code", dtTax.Rows.Count - 1, "");
                    dtTax.SetValue("MinAmt", dtTax.Rows.Count - 1, "0.00");
                    dtTax.SetValue("MaxAmt", dtTax.Rows.Count - 1, "0.00");
                    dtTax.SetValue("fixTerm", dtTax.Rows.Count - 1, "0.00");
                    dtTax.SetValue("Value", dtTax.Rows.Count - 1, "0.00");
                    dtTax.SetValue("AddVal", 0, "0.00");
                    dtTax.SetValue("Descr", dtTax.Rows.Count - 1, "");

                    mtItax.AddRow(1, mtItax.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();
            mtItax.LoadFromDataSource();

        }

        public void fillSalYear()
        {
            IEnumerable<MstCalendar> cals = from p in dbHrPayroll.MstCalendar select p;


            int i = 0;
            string selId = "0";
            foreach (MstCalendar cal in cals)
            {

                cbYear.ValidValues.Add(cal.Id.ToString(), cal.Description);
                selId = cal.Id.ToString();

                i++;
            }
            //  cbPeriod.Select(0,SAPbouiCOM.BoSearchKey.psk_Index);
            try
            {
                oForm.DataSources.UserDataSources.Item("cbYear").ValueEx = selId;
                getYearTax(selId);
            }
            catch { }


        }

        private bool isValid()
        {
            bool retVal = true;
            retVal = true;

            retVal = !overLapingRage();

            return retVal;
        }

        private void submitForm()
        {
            if (!isValid())
            {
                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("ValidationFailed"));
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                return;

            }
            // mtAuth.FlushToDataSource();
            string id = "";
            string code = "";
            string isnew = "";

            mtItax.FlushToDataSource();
            CfgTaxSetup yearTax;
            //:TODO Crash on Tax Slab
            int cnt = (from p in dbHrPayroll.CfgTaxSetup 
                       where p.SalaryYear.ToString() == cbYear.Value.ToString() 
                       select p).Count();
            if (cnt > 0)
            {
                yearTax = (from p in dbHrPayroll.CfgTaxSetup 
                           where p.SalaryYear.ToString() == cbYear.Value.ToString() 
                           select p).FirstOrDefault();
            }
            else
            {
                yearTax = new CfgTaxSetup();
                yearTax.MstCalendar = (from p in dbHrPayroll.MstCalendar 
                                       where p.Id.ToString() == cbYear.Value.ToString() 
                                       select p).FirstOrDefault(); // Convert.ToInt16(cbYear.Value.ToString());
                yearTax.UserId = oCompany.UserName;

                yearTax.CreateDate = DateTime.Now;
                dbHrPayroll.CfgTaxSetup.InsertOnSubmit(yearTax);
            }

            yearTax.UpdateDate = DateTime.Now;
            yearTax.UpdatedBy = oCompany.UserName;

            for (int i = 0; i < dtTax.Rows.Count; i++)
            {
                code = Convert.ToString(dtTax.GetValue("Code", i));
                id = Convert.ToString(dtTax.GetValue("id", i));
                isnew = Convert.ToString(dtTax.GetValue("isNew", i));
                isnew = isnew.Trim();
                code = code.Trim();
                if (code != "")
                {
                    CfgTaxDetail txDetail;
                    if (isnew == "Y")
                    {

                        txDetail = new CfgTaxDetail();
                        yearTax.CfgTaxDetail.Add(txDetail);
                    }
                    else
                    {
                        txDetail = (from p in dbHrPayroll.CfgTaxDetail where p.Id.ToString() == id select p).Single();
                    }
                    txDetail.TaxCode = dtTax.GetValue("Code", i);
                    txDetail.TaxValue = Convert.ToDecimal(dtTax.GetValue("Value", i));
                    txDetail.MinAmount = Convert.ToDecimal(dtTax.GetValue("MinAmt", i));
                    txDetail.MaxAmount = Convert.ToDecimal(dtTax.GetValue("MaxAmt", i));
                    txDetail.Description = Convert.ToString(dtTax.GetValue("Descr", i));
                    txDetail.AdditionalDisc = Convert.ToDecimal(dtTax.GetValue("AddVal", i));
                    txDetail.FixTerm = Convert.ToDecimal(dtTax.GetValue("fixTerm", i));

                    //id,isNew,Code,MinAmt,MaxAmt,fixTerm,Value,Descr
                }
            }

            dbHrPayroll.SubmitChanges();
            getYearTax(cbYear.Value);

        }

        private bool overLapingRage()
        {
            bool result = false;

            for (int i = 0; i < dtTax.Rows.Count - 1; i++)
            {
                string code = dtTax.GetValue("Code", i);
                if (code.Trim() != "")
                {
                    decimal minRange = Convert.ToDecimal(dtTax.GetValue("MinAmt", i));
                    decimal maxRange = Convert.ToDecimal(dtTax.GetValue("MaxAmt", i));
                    if (minRange >= maxRange)
                    {
                        result = true;
                    }
                    if (rangeCnt(Convert.ToInt32(dtTax.GetValue("MinAmt", i))) > 1 || rangeCnt(Convert.ToInt32(dtTax.GetValue("MaxAmt", i))) > 1)
                    {
                        result = true;
                    }
                }
            }

            return result;

        }

        private int rangeCnt(long number)
        {
            mtItax.FlushToDataSource();
            int result = 0;

            for (int i = 0; i < dtTax.Rows.Count - 1; i++)
            {
                string code = dtTax.GetValue("Code", i);
                if (code.Trim() != "")
                {
                    decimal minRange = Convert.ToDecimal(dtTax.GetValue("MinAmt", i));
                    decimal maxRange = Convert.ToDecimal(dtTax.GetValue("MaxAmt", i));
                    if (minRange <= number && maxRange >= number)
                    {
                        result += 1;
                    }
                }
            }

            return result;
        }

        public void getYearTax(string yearId)
        {
            //cbPeriod.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            dtTax.Rows.Clear();
            int cnt = (from p in dbHrPayroll.CfgTaxSetup where p.SalaryYear.ToString() == yearId.ToString() select p).Count();
            if (cnt > 0)
            {
                CfgTaxSetup yearTax = (from p in dbHrPayroll.CfgTaxSetup where p.SalaryYear.ToString() == yearId.ToString() select p).Single();
                int k = 0;
                foreach (CfgTaxDetail dt in yearTax.CfgTaxDetail)
                {
                    dtTax.Rows.Add(1);
                    dtTax.SetValue("id", dtTax.Rows.Count - 1, dt.Id.ToString());
                    dtTax.SetValue("isNew", dtTax.Rows.Count - 1, "N");
                    dtTax.SetValue("Code", dtTax.Rows.Count - 1, dt.TaxCode);
                    dtTax.SetValue("MinAmt", dtTax.Rows.Count - 1, dt.MinAmount.ToString());
                    dtTax.SetValue("MaxAmt", dtTax.Rows.Count - 1, dt.MaxAmount.ToString());
                    dtTax.SetValue("fixTerm", dtTax.Rows.Count - 1, dt.FixTerm.ToString());
                    dtTax.SetValue("Value", dtTax.Rows.Count - 1, dt.TaxValue.ToString());
                    dtTax.SetValue("AddVal", dtTax.Rows.Count - 1, dt.AdditionalDisc == null ? "0.00" : dt.AdditionalDisc.ToString());
                    dtTax.SetValue("Descr", dtTax.Rows.Count - 1, dt.Description);
                    mtItax.AddRow(1, mtItax.RowCount + 1);
                    k++;
                }
            }
            addEmptyRow();
            mtItax.LoadFromDataSource();
        }

        private Boolean AddValidation()
        {
            try
            {
                string code;
                string fiscalyear = cbYear.Value.Trim();
                decimal rowMinAmount = 0, rowMaxAmount = 0, prevMaxAmount = 0, rowFixValue = 0, rowValue = 0;
                List<string> oCodeList = new List<string>();
                if (!string.IsNullOrEmpty(fiscalyear))
                {
                    var oCal = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();
                    if (oCal == null)
                    {
                        oApplication.StatusBar.SetText("Active fiscal year not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    if (oCal.Id.ToString() != fiscalyear)
                    {
                        oApplication.StatusBar.SetText("Inactive fiscal are not allowed to modify.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                }
                mtItax.FlushToDataSource();
                for (int i = 0; i < dtTax.Rows.Count; i++)
                {
                    code = Convert.ToString(dtTax.GetValue("Code", i));
                    rowMinAmount = Convert.ToDecimal(dtTax.GetValue("MinAmt", i));
                    rowMaxAmount = Convert.ToDecimal(dtTax.GetValue("MaxAmt", i));
                    rowFixValue = Convert.ToDecimal(dtTax.GetValue("fixTerm", i));
                    rowValue = Convert.ToDecimal(dtTax.GetValue("Value", i));
                    if (i == 0)
                    {
                        prevMaxAmount = rowMaxAmount;
                    }
                    if (rowMinAmount < 0 || rowMaxAmount < 0 || rowFixValue < 0 || rowValue < 0)
                    {
                        oApplication.StatusBar.SetText("Negative Values not allowed. @ Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    if (rowMinAmount == prevMaxAmount)
                    {
                        oApplication.StatusBar.SetText("Previous maximum value can't be equal to Rows minimum value @ Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    if (rowMinAmount > rowMaxAmount)
                    {
                        oApplication.StatusBar.SetText("Minimum value can't be greater than maximum value. @ Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    prevMaxAmount = rowMaxAmount;
                    if (!string.IsNullOrEmpty(code))
                    {
                        oCodeList.Add(code);
                    }
                }
                if (oCodeList.Count != oCodeList.Distinct().Count())
                {
                    oApplication.StatusBar.SetText("Duplicate codes not allowed. ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return false;
                }
                int confirm = oApplication.MessageBox("Change in the tax slabs may impact salary tax deductions. Are you sure?", 1, "Yes", "No");
                if (confirm == 2) return false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void pickTaxSlabforCopy()
        {
            SearchKeyVal.Clear();
            string strSql = sqlString.getSql("TaxSlab", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select", "Select");
            pic = null;
            if (st.Rows.Count > 0)
            {
                CopyTaxSlab(st.Rows[0][0].ToString());
            }

        }

        public void CopyTaxSlab(string SlabID)
        {

            if (cbYear.Value.ToString().Trim() == "") return;
            oForm.Freeze(true);
            try
            {
                CfgTaxSetup tx;

                IniContrls();
                int cnt = (from tax in dbHrPayroll.CfgTaxSetup
                           where tax.Id == Convert.ToInt32(SlabID.Trim())
                           select tax).Count();
                if (cnt == 0)
                {
                    IniContrls();
                }
                if (cnt == 0)
                {

                }
                else
                {
                    tx = (from GetTx in dbHrPayroll.CfgTaxSetup
                          where GetTx.Id == Convert.ToInt32(SlabID.Trim())
                          select GetTx).FirstOrDefault();

                    var TaxDetails = (from txDT in dbHrPayroll.CfgTaxDetail
                                      where txDT.Pid == tx.Id
                                      select txDT).ToList();

                    txDisc.Value = Convert.ToString(tx.DiscountOnTotalTax.GetValueOrDefault());
                    txMinSal.Value =Convert.ToString( tx.MinTaxSalaryF.GetValueOrDefault());
                    txMxSalDis.Value = Convert.ToString(tx.MaxSalaryDisc.GetValueOrDefault());
                    txSenCit.Value =Convert.ToString(tx.SeniorCitizonAge.GetValueOrDefault());
                    
                    int i = 0;
                    foreach (CfgTaxDetail txdetail in TaxDetails)
                    {
                        if (txdetail.TaxCode != "")
                        {   
                            dtTax.SetValue("id", dtTax.Rows.Count - 1, txdetail.Id.ToString());
                            dtTax.SetValue("isNew", dtTax.Rows.Count - 1, "Y");
                            dtTax.SetValue("Code", dtTax.Rows.Count - 1, txdetail.TaxCode == null ? "" : txdetail.TaxCode);
                            dtTax.SetValue("MinAmt", dtTax.Rows.Count - 1, txdetail.MinAmount.GetValueOrDefault().ToString());
                            dtTax.SetValue("MaxAmt", dtTax.Rows.Count - 1, txdetail.MaxAmount.GetValueOrDefault().ToString());
                            dtTax.SetValue("fixTerm", dtTax.Rows.Count - 1, txdetail.FixTerm.GetValueOrDefault().ToString());
                            dtTax.SetValue("Value", dtTax.Rows.Count - 1, txdetail.TaxValue.GetValueOrDefault().ToString());
                            dtTax.SetValue("AddVal", dtTax.Rows.Count - 1, txdetail.AdditionalDisc.GetValueOrDefault().ToString());
                            dtTax.SetValue("Descr", dtTax.Rows.Count - 1, txdetail.Description);
                            mtItax.AddRow(1, mtItax.RowCount + 1);
                            i++;
                        }
                        dtTax.Rows.Add(1);
                    }
                    
                    mtItax.LoadFromDataSource();
                    i = 0;
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
            oForm.Freeze(false);
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }
        #endregion

    }
}
