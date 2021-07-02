
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Bonus:HRMSBaseForm
    {
        /* Form Items Objects */
        SAPbouiCOM.Matrix  mtBonus;
        SAPbouiCOM.Column coCode, coDesc, coVT,coVal, coActive, isNew, id;
        private SAPbouiCOM.DataTable dtBonus;

        //**********************************b

        public IEnumerable<MstBonus> bonus;



        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);

            InitiallizeForm();
            oForm.Freeze(false);

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

            mtBonus = oForm.Items.Item("mtBonus").Specific;
            isNew = mtBonus.Columns.Item("isNew");
            id = mtBonus.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;
            coCode = mtBonus.Columns.Item("coCode");
            coDesc = mtBonus.Columns.Item("coDesc");
            coActive = mtBonus.Columns.Item("coActive");
            coVal = mtBonus.Columns.Item("coVal");
            coVT = mtBonus.Columns.Item("coVT");
            dtBonus = oForm.DataSources.DataTables.Item("dtBonus");
            dtBonus.Rows.Clear();
            fillMat();
            fillColumCombo("Val_Type", coVT);

            oForm.Freeze(false);

        }
        private void fillMat()
        {
            dtBonus.Rows.Clear();
            bonus = from p in dbHrPayroll.MstBonus select p;
            dtBonus.Rows.Clear();
            dtBonus.Rows.Add(bonus.Count());
            int i = 0;
            foreach (MstBonus bon in bonus)
            {
                dtBonus.SetValue("isNew", i, "N");
                dtBonus.SetValue("id", i, bon.Id);
                dtBonus.SetValue("BonusCode", i, bon.Code.ToString());
                dtBonus.SetValue("Descr", i, bon.Description.ToString());
                dtBonus.SetValue("ValType", i, bon.ValueType.ToString());
                dtBonus.SetValue("Val", i, bon.Value.ToString());
                dtBonus.SetValue("Active", i, bon.FlgActive == true ? "Y" : "N");

                i++;

            }
            addEmptyRow();

            mtBonus.LoadFromDataSource();

        }
        private void addEmptyRow()
        {


            if (dtBonus.Rows.Count == 0)
            {
                dtBonus.Rows.Add(1);

                dtBonus.SetValue("isNew", 0, "Y");
                dtBonus.SetValue("id", 0, 0);
                dtBonus.SetValue("BonusCode", 0, "");
                dtBonus.SetValue("Descr", 0, "");
                dtBonus.SetValue("ValType", 0, "");
                dtBonus.SetValue("Val", 0, "0.00");
                dtBonus.SetValue("Active", 0, "N");



                mtBonus.AddRow(1, mtBonus.RowCount + 1);
            }
            else
            {
                if (dtBonus.GetValue("BonusCode", dtBonus.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtBonus.Rows.Add(1);
                    dtBonus.SetValue("isNew", dtBonus.Rows.Count - 1, "Y");
                    dtBonus.SetValue("id", dtBonus.Rows.Count - 1, 0);
                    dtBonus.SetValue("BonusCode", dtBonus.Rows.Count - 1, "");
                    dtBonus.SetValue("Descr", dtBonus.Rows.Count - 1, "");
                    dtBonus.SetValue("ValType", dtBonus.Rows.Count - 1, "");
                    dtBonus.SetValue("Val", dtBonus.Rows.Count - 1, "0.00");
                    dtBonus.SetValue("Active", dtBonus.Rows.Count - 1, "N");
                    mtBonus.AddRow(1, mtBonus.RowCount + 1);
                }

            }
            mtBonus.LoadFromDataSource();

        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    updateDbWithMat();
                    break;
            }
        }

        private void updateDbWithMat()
        {
            try
            {
                mtBonus.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                for (int i = 0; i < dtBonus.Rows.Count; i++)
                {
                    code = Convert.ToString(dtBonus.GetValue("BonusCode", i));
                    isnew = Convert.ToString(dtBonus.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "")
                    {
                        MstBonus objBonus;
                        id = Convert.ToString(dtBonus.GetValue("id", i));
                        if (isnew == "Y")
                        {
                            objBonus = new MstBonus();
                            dbHrPayroll.MstBonus.InsertOnSubmit(objBonus);
                        }
                        else
                        {
                            objBonus = (from p in dbHrPayroll.MstBonus where p.Id.ToString() == id.Trim() select p).Single();
                        }
                        objBonus.Code = Convert.ToString(dtBonus.GetValue("BonusCode", i));
                        objBonus.Description = dtBonus.GetValue("Descr", i);
                        objBonus.ValueType = Convert.ToString(dtBonus.GetValue("ValType", i));
                        string va = dtBonus.GetValue("Val", i);
                        objBonus.Value = Convert.ToDecimal(va);

                        objBonus.FlgActive = Convert.ToString(dtBonus.GetValue("Active", i)) == "Y" ? true : false;
                        //objBonus.CreateDate = DateTime.Now;
                       // objBonus.CreatedBy = "Manager"; //to be changed;

                    }
                }
                dbHrPayroll.SubmitChanges();
                addEmptyRow();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }
       
    }
}
