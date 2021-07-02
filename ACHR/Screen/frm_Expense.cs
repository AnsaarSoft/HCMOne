using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;


namespace ACHR.Screen
{
    class frm_Expense:HRMSBaseForm
    {
        /* Form Items Objects */
        SAPbouiCOM.Matrix mtExpense;
        SAPbouiCOM.Column coCode, coDesc, covos, coActive, isNew, id;
        private SAPbouiCOM.DataTable dtExpense;

        //**********************************

        public IEnumerable<MstExpense> expense;



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

            mtExpense = oForm.Items.Item("mtExpense").Specific;
            isNew = mtExpense.Columns.Item("isNew");
            id = mtExpense.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;
            coCode = mtExpense.Columns.Item("coCode");
            coDesc = mtExpense.Columns.Item("coDescr");
            coActive = mtExpense.Columns.Item("coActive");
            covos = mtExpense.Columns.Item("covos");
           
            dtExpense = oForm.DataSources.DataTables.Item("dtExpense");
            dtExpense.Rows.Clear();
            fillMat();


            oForm.Freeze(false);

        }
        private void fillMat()
        {
            dtExpense.Rows.Clear();
            expense = from p in dbHrPayroll.MstExpense select p;
            dtExpense.Rows.Clear();
            dtExpense.Rows.Add(expense.Count());
            int i = 0;
            foreach (MstExpense exp in expense)
            {
                dtExpense.SetValue("isNew", i, "N");
                dtExpense.SetValue("id", i, exp.Id);
                dtExpense.SetValue("expCode", i, exp.ExpenseId.ToString());
                dtExpense.SetValue("Descr", i, exp.Description.ToString());
                dtExpense.SetValue("vos", i, exp.FlgVoss == true ? "Y" : "N");
                dtExpense.SetValue("Active", i, exp.FlgActive == true ? "Y" : "N");

                i++;

            }
            addEmptyRow();

            mtExpense.LoadFromDataSource();

        }
        private void addEmptyRow()
        {


            if (dtExpense.Rows.Count == 0)
            {
                dtExpense.Rows.Add(1);

                dtExpense.SetValue("isNew", 0, "Y");
                dtExpense.SetValue("id", 0, 0);
                dtExpense.SetValue("expCode", 0, "");
                dtExpense.SetValue("Descr", 0, "");
                dtExpense.SetValue("vos", 0, "0");
                dtExpense.SetValue("Active", 0, "N");



                mtExpense.AddRow(1, mtExpense.RowCount + 1);
            }
            else
            {
                if (dtExpense.GetValue("expCode", dtExpense.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtExpense.Rows.Add(1);
                    dtExpense.SetValue("isNew", dtExpense.Rows.Count - 1, "Y");
                    dtExpense.SetValue("id", dtExpense.Rows.Count - 1, 0);
                    dtExpense.SetValue("expCode", dtExpense.Rows.Count - 1, "");
                    dtExpense.SetValue("Descr", dtExpense.Rows.Count - 1, "");
                    dtExpense.SetValue("Active", dtExpense.Rows.Count - 1, "N");
                    dtExpense.SetValue("vos", dtExpense.Rows.Count - 1, "N");
                    mtExpense.AddRow(1, mtExpense.RowCount + 1);
                }

            }
            mtExpense.LoadFromDataSource();

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
            mtExpense.FlushToDataSource();
            string id = "";
            string code = "";
            string isnew = "";
            for (int i = 0; i < dtExpense.Rows.Count; i++)
            {
                code = Convert.ToString(dtExpense.GetValue("expCode", i));
                isnew = Convert.ToString(dtExpense.GetValue("isNew", i));
                isnew = isnew.Trim();
                code = code.Trim();
                if (code != "")
                {
                    MstExpense objUp;
                    id = Convert.ToString(dtExpense.GetValue("id", i));
                    if (isnew == "Y")
                    {
                        objUp = new MstExpense();
                        dbHrPayroll.MstExpense.InsertOnSubmit(objUp);
                    }
                    else
                    {
                        objUp = (from p in dbHrPayroll.MstExpense where p.Id.ToString() == id.Trim() select p).Single();
                    }
                    objUp.ExpenseId = dtExpense.GetValue("expCode", i);
                    objUp.Description = dtExpense.GetValue("Descr", i);

                    objUp.FlgActive = Convert.ToString(dtExpense.GetValue("Active", i)) == "Y" ? true : false;
                    objUp.FlgVoss = Convert.ToString(dtExpense.GetValue("vos", i)) == "Y" ? true : false;

                    objUp.CreateDate = DateTime.Now;
                    objUp.UserId = "Manager"; //to be changed;

                }
            }
            dbHrPayroll.SubmitChanges();
            addEmptyRow();
        }
    }
}
