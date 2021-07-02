
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Gratuity:HRMSBaseForm
    {
        /* Form Items Objects */
        SAPbouiCOM.Matrix  mtGratuity;
        SAPbouiCOM.Column Code, Descr, GratType, Basis, Active, BasedOn, YearFrom, YearTo, Factor, FormOn,isNew,id;
        private SAPbouiCOM.DataTable dtGratuity;

        //**********************************

        public IEnumerable<MstGratuity> gratuity;



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


            mtGratuity = oForm.Items.Item("mtGratuity").Specific;
            isNew = mtGratuity.Columns.Item("isNew");
            id = mtGratuity.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

          
            Code = mtGratuity.Columns.Item("Code");
            Code.Visible = false;
            Descr = mtGratuity.Columns.Item("Descr");
            GratType = mtGratuity.Columns.Item("GratType");
            Basis = mtGratuity.Columns.Item("Basis");
            Active = mtGratuity.Columns.Item("Active");
            BasedOn = mtGratuity.Columns.Item("BasedOn");
            YearFrom = mtGratuity.Columns.Item("YearFrom");
            YearTo = mtGratuity.Columns.Item("YearTo");
            Factor = mtGratuity.Columns.Item("Factor");
            FormOn = mtGratuity.Columns.Item("FormOn");

            dtGratuity = oForm.DataSources.DataTables.Item("dtGratuity");
            dtGratuity.Rows.Clear();
            fillColumCombo("gratType", mtGratuity.Columns.Item("GratType"));
            fillColumCombo("gratBasis", mtGratuity.Columns.Item("Basis"));
            fillColumCombo("gratBasedOn", mtGratuity.Columns.Item("BasedOn"));

            fillColumCombo("gratBaseOn", mtGratuity.Columns.Item("FormOn"));

            

            fillMat();

           
            oForm.Freeze(false);

        }
        private void fillMat()
        {
            dtGratuity.Rows.Clear();
            gratuity = from p in dbHrPayroll.MstGratuity select p;
            dtGratuity.Rows.Clear();
            dtGratuity.Rows.Add(gratuity.Count());
            int i = 0;
            foreach (MstGratuity grat in gratuity)
            {
                try
                {
                    dtGratuity.SetValue("isNew", i, "N");
                    dtGratuity.SetValue("id", i, grat.Id);
                    dtGratuity.SetValue("Code", i, grat.Id.ToString());
                    dtGratuity.SetValue("Descr", i, grat.GratuityName.ToString());
                    dtGratuity.SetValue("GratType", i, grat.GratuityType.ToString());
                    dtGratuity.SetValue("Basis", i, grat.GratuityBasis.ToString());
                    dtGratuity.SetValue("Active", i, grat.FlgActive == true ? "Y" : "N");
                    dtGratuity.SetValue("BasedOn", i, grat.GratuityBasis.ToString());
                    dtGratuity.SetValue("YearFrom", i, grat.YearFrom.ToString());
                    dtGratuity.SetValue("YearTo", i, grat.YearTo.ToString());
                    dtGratuity.SetValue("Factor", i, grat.Factor.ToString());
                    dtGratuity.SetValue("FormOn", i, grat.SalaryType.ToString());

                }
                catch { }
                i++;

            }
            addEmptyRow();

            mtGratuity.LoadFromDataSource();

        }
        private void addEmptyRow()
        {


            if (dtGratuity.Rows.Count == 0)
            {
                dtGratuity.Rows.Add(1);

              


                dtGratuity.SetValue("isNew", 0, "Y");
                dtGratuity.SetValue("id", 0, 0);
                dtGratuity.SetValue("Code", 0, "");
                dtGratuity.SetValue("Descr", 0, "");
                dtGratuity.SetValue("GratType", 0, "");
                dtGratuity.SetValue("Basis", 0, "");
                dtGratuity.SetValue("Active", 0, "N");
                dtGratuity.SetValue("BasedOn", 0, "");
                dtGratuity.SetValue("YearFrom", 0, "0");
                dtGratuity.SetValue("YearTo", 0, "0");
                dtGratuity.SetValue("Factor", 0, "0.00");
                dtGratuity.SetValue("FormOn", 0, "");



                mtGratuity.AddRow(1, mtGratuity.RowCount + 1);
            }
            else
            {
                if (dtGratuity.GetValue("Code", dtGratuity.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtGratuity.Rows.Add(1);
                    dtGratuity.SetValue("isNew", dtGratuity.Rows.Count - 1, "Y");
                    dtGratuity.SetValue("id", dtGratuity.Rows.Count - 1, 0);
                    dtGratuity.SetValue("Code", dtGratuity.Rows.Count - 1, "");
                    dtGratuity.SetValue("Descr", dtGratuity.Rows.Count - 1, "");
                    dtGratuity.SetValue("GratType", dtGratuity.Rows.Count - 1, "0");
                    dtGratuity.SetValue("Basis", dtGratuity.Rows.Count - 1, "0");
                    dtGratuity.SetValue("Active", dtGratuity.Rows.Count - 1, "N");
                    dtGratuity.SetValue("BasedOn", dtGratuity.Rows.Count - 1, "0");
                    dtGratuity.SetValue("YearFrom", dtGratuity.Rows.Count - 1, "0");
                    dtGratuity.SetValue("YearTo", dtGratuity.Rows.Count - 1, "0");
                    dtGratuity.SetValue("Factor", dtGratuity.Rows.Count - 1, "0.00");
                    dtGratuity.SetValue("FormOn", dtGratuity.Rows.Count - 1, "");

                    mtGratuity.AddRow(1, mtGratuity.RowCount + 1);
                }

            }
          

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
                mtGratuity.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                for (int i = 0; i < dtGratuity.Rows.Count; i++)
                {
                    code = Convert.ToString(dtGratuity.GetValue("Descr", i));
                    isnew = Convert.ToString(dtGratuity.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "")
                    {
                        MstGratuity objGra;
                        id = Convert.ToString(dtGratuity.GetValue("id", i));
                        if (isnew == "Y")
                        {
                            objGra = new MstGratuity();
                            dbHrPayroll.MstGratuity.InsertOnSubmit(objGra);
                        }
                        else
                        {
                            objGra = (from p in dbHrPayroll.MstGratuity where p.Id.ToString() == id.Trim() select p).Single();
                            //objGra.Code = Convert.ToInt16( dtGratuity.GetValue("Code", i));

                        }



                        objGra.GratuityName = dtGratuity.GetValue("Descr", i);
                        objGra.GratuityType = dtGratuity.GetValue("GratType", i);
                        objGra.FlgActive = Convert.ToString(dtGratuity.GetValue("Active", i)) == "Y" ? true : false;
                        objGra.BasedOn = dtGratuity.GetValue("BasedOn", i);

                        objGra.GratuityBasis = Convert.ToInt16(dtGratuity.GetValue("Basis", i));
                        objGra.YearFrom = Convert.ToString(dtGratuity.GetValue("YearFrom", i));
                        objGra.YearTo = Convert.ToString(dtGratuity.GetValue("YearTo", i));
                        string va = Convert.ToString(dtGratuity.GetValue("Factor", i));
                        objGra.Factor = Convert.ToDecimal(va);
                        objGra.SalaryType = dtGratuity.GetValue("FormOn", i);
                        objGra.CreateDate = DateTime.Now;
                        objGra.UserId = oCompany.UserName; //to be changed;

                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch(Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }
       
    }
}
