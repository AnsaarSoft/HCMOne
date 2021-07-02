using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_GLAcctDetComp : HRMSBaseForm
    {

        /* Form Items Objects */
        SAPbouiCOM.Matrix mtEarnings, mtDed, mtCon, mtLoan, mtAdv, mtOt, mtBonus;
        SAPbouiCOM.Column coAdvCode, coAdvDesc, coAdvAct,isNew,id;
        private SAPbouiCOM.DataTable dtAdv, dtEarnings, dtDeduct, dtCont, dtLoan, dtOt, dtBonus;
        SAPbouiCOM.EditText txBasicE, txArrE, txLevE, txEOSE, txGrtA, txITE, txBasicP, txArrP, txLvP, txEOSP, txGrtP, txITP,txDff,GLDID;
        SAPbouiCOM.Item ImtAdv, IcoAdvCode, IcoAdvDesc, IcoAdvAct, ItxBasicE, ItxArrE, ItxLevE, ItxEOSE, ItxGrtA, ItxITE, ItxBasicP, ItxArrP, ItxLvP, ItxEOSP, ItxGrtP, ItxITP, ItxDff, IGLDID;
        //**********************************

        public IEnumerable<MstAdvance> advances;
        public IEnumerable<MstLoans> loans;
        public IEnumerable<MstOverTime> overtimes;
        public IEnumerable<MstBonus> bonuses;
        public IEnumerable<MstElementEarning> earnings;
        public IEnumerable<MstElementDeduction> deductions;
        public IEnumerable<MstElementContribution> contributions;
      
       
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
         //   oForm.Freeze(true);
           
           InitiallizeForm();
        //    oForm.Freeze(false);

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

            //, , , , txGrtA, txITE, txBasicP, txArrP, txLvP, txEOSP, txGrtP, txITP;

            oForm.DataSources.UserDataSources.Add("txBasicE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txBasicE = oForm.Items.Item("txBasicE").Specific;
            ItxBasicE = oForm.Items.Item("txBasicE");
            txBasicE.DataBind.SetBound(true, "", "txBasicE");
            
            oForm.DataSources.UserDataSources.Add("txArrE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txArrE = oForm.Items.Item("txArrE").Specific;
            ItxArrE = oForm.Items.Item("txArrE");
            txArrE.DataBind.SetBound(true, "", "txArrE");
           
            oForm.DataSources.UserDataSources.Add("txLevE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txLevE = oForm.Items.Item("txLevE").Specific;
            ItxLevE = oForm.Items.Item("txLevE");
            txLevE.DataBind.SetBound(true, "", "txLevE");
           
            oForm.DataSources.UserDataSources.Add("txEOSE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txEOSE = oForm.Items.Item("txEOSE").Specific;
            ItxEOSE = oForm.Items.Item("txEOSE");
            txEOSE.DataBind.SetBound(true, "", "txEOSE");


            oForm.DataSources.UserDataSources.Add("txGrtA", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txGrtA = oForm.Items.Item("txGrtA").Specific;
            ItxGrtA = oForm.Items.Item("txGrtA");
            txGrtA.DataBind.SetBound(true, "", "txGrtA");

            oForm.DataSources.UserDataSources.Add("txITE", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txITE = oForm.Items.Item("txITE").Specific;
            ItxITE = oForm.Items.Item("txITE");
            txITE.DataBind.SetBound(true, "", "txITE");

            oForm.DataSources.UserDataSources.Add("txBasicP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txBasicP = oForm.Items.Item("txBasicP").Specific;
            ItxBasicP = oForm.Items.Item("txBasicP");
            txBasicP.DataBind.SetBound(true, "", "txBasicP");

            oForm.DataSources.UserDataSources.Add("txArrP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txArrP = oForm.Items.Item("txArrP").Specific;
            ItxArrP = oForm.Items.Item("txArrP");
            txArrP.DataBind.SetBound(true, "", "txArrP");

            oForm.DataSources.UserDataSources.Add("txLvP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txLvP = oForm.Items.Item("txLvP").Specific;
            ItxLvP = oForm.Items.Item("txLvP");
            txLvP.DataBind.SetBound(true, "", "txLvP");

            oForm.DataSources.UserDataSources.Add("txEOSP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txEOSP = oForm.Items.Item("txEOSP").Specific;
            ItxEOSP = oForm.Items.Item("txEOSP");
            txEOSP.DataBind.SetBound(true, "", "txEOSP");

            oForm.DataSources.UserDataSources.Add("txGrtP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txGrtP = oForm.Items.Item("txGrtP").Specific;
            ItxGrtP = oForm.Items.Item("txGrtP");
            txGrtP.DataBind.SetBound(true, "", "txGrtP");

            oForm.DataSources.UserDataSources.Add("txITP", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txITP = oForm.Items.Item("txITP").Specific;
            ItxITP = oForm.Items.Item("txITP");
            txITP.DataBind.SetBound(true, "", "txITP");
            
            oForm.DataSources.UserDataSources.Add("txDff", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txDff = oForm.Items.Item("txDff").Specific;
            ItxDff = oForm.Items.Item("txDff");
            txDff.DataBind.SetBound(true, "", "txDff");


            oForm.DataSources.UserDataSources.Add("GLDID", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            GLDID = oForm.Items.Item("GLDID").Specific;
            IGLDID = oForm.Items.Item("GLDID");
            GLDID.DataBind.SetBound(true, "", "GLDID");


            mtAdv = oForm.Items.Item("mtAdv").Specific;
            isNew = mtAdv.Columns.Item("isNew");
            id = mtAdv.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtEarnings = oForm.Items.Item("mtEarnings").Specific;
            isNew = mtEarnings.Columns.Item("isNew");
            id = mtEarnings.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtDed = oForm.Items.Item("mtDed").Specific;
            isNew = mtDed.Columns.Item("isNew");
            id = mtDed.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtCon = oForm.Items.Item("mtCon").Specific;
            isNew = mtCon.Columns.Item("isNew");
            id = mtCon.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtLoan = oForm.Items.Item("mtLoan").Specific;
            isNew = mtLoan.Columns.Item("isNew");
            id = mtLoan.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtOt = oForm.Items.Item("mtOt").Specific;
            isNew = mtOt.Columns.Item("isNew");
            id = mtOt.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            mtBonus = oForm.Items.Item("mtBonus").Specific;
            isNew = mtBonus.Columns.Item("isNew");
            id = mtBonus.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;


            //, , , , mtAdv, , ;
            dtAdv = oForm.DataSources.DataTables.Item("dtAdv");
            dtAdv.Rows.Clear();

            dtEarnings = oForm.DataSources.DataTables.Item("dtEarnings");
            dtEarnings.Rows.Clear();

            dtDeduct = oForm.DataSources.DataTables.Item("dtDeduct");
            dtDeduct.Rows.Clear();
            
            dtCont = oForm.DataSources.DataTables.Item("dtCont");
            dtCont.Rows.Clear();

            dtLoan = oForm.DataSources.DataTables.Item("dtLoan");
            dtLoan.Rows.Clear();

            dtOt = oForm.DataSources.DataTables.Item("dtOt");
            dtOt.Rows.Clear();

            dtBonus = oForm.DataSources.DataTables.Item("dtBonus");
            dtBonus.Rows.Clear();
            //, , , , , , 

            fillMat();
            _fillFields();
            oForm.PaneLevel =1;
            oForm.Freeze(false);

        }
        private void fillMat()
        {
            dtAdv.Rows.Clear();
            advances = from p in dbHrPayroll.MstAdvance select p;
            dtAdv.Rows.Clear();
            dtAdv.Rows.Add(advances.Count());
            int i = 0;
            foreach (MstAdvance adv in advances)
            {
                dtAdv.SetValue("isNew", i, "N");
                dtAdv.SetValue("id", i, adv.Id);
                dtAdv.SetValue("Code", i, adv.AllowanceId.ToString());
                dtAdv.SetValue("descr", i, adv.Description.ToString());
               
                i++;

            }
          
            mtAdv.LoadFromDataSource();

             dtEarnings.Rows.Clear();
             earnings = from p in dbHrPayroll.MstElementEarning where p.Value>0 select p;
             dtEarnings.Rows.Clear();
             dtEarnings.Rows.Add(earnings.Count());
             i = 0;
            foreach (MstElementEarning earning in earnings)
            {
                dtEarnings.SetValue("isNew", i, "N");
                dtEarnings.SetValue("id", i, earning.Id.ToString());
                dtEarnings.SetValue("Code", i, earning.MstElements.ElementName);
                dtEarnings.SetValue("descr", i, earning.MstElements.Description);

                i++;

            }

            mtEarnings.LoadFromDataSource();

            dtDeduct.Rows.Clear();
            deductions = from p in dbHrPayroll.MstElementDeduction where p.Value>0 select p;
            dtDeduct.Rows.Clear();
            dtDeduct.Rows.Add(deductions.Count());
            i = 0;
            foreach (MstElementDeduction deduct in deductions)
            {
                dtDeduct.SetValue("isNew", i, "N");
                dtDeduct.SetValue("id", i, deduct.Id.ToString());
                dtDeduct.SetValue("Code", i, deduct.MstElements.ElementName);
                dtDeduct.SetValue("descr", i, deduct.MstElements.Description);

                i++;

            }

            mtDed.LoadFromDataSource();

            dtLoan.Rows.Clear();
            loans = from p in dbHrPayroll.MstLoans where p.FlgActive == true select p;
            dtLoan.Rows.Clear();
            dtLoan.Rows.Add(loans.Count());
            i = 0;
            foreach (MstLoans lon in loans)
            {
                dtLoan.SetValue("isNew", i, "N");
                dtLoan.SetValue("id", i, lon.Id.ToString());
                dtLoan.SetValue("Code", i, lon.Code);
                dtLoan.SetValue("descr", i, lon.Description);

                i++;

            }

            mtLoan.LoadFromDataSource();

            dtCont.Rows.Clear();
            contributions = from p in dbHrPayroll.MstElementContribution where p.Employee>0 || p.Employer>0 == true select p;
            dtCont.Rows.Clear();
            dtCont.Rows.Add(contributions.Count());
            i = 0;
            foreach (MstElementContribution cont in contributions)
            {
                dtCont.SetValue("isNew", i, "N");
                dtCont.SetValue("id", i, cont.Id.ToString());
                dtCont.SetValue("Code", i, cont.MstElements.ElementName);
                dtCont.SetValue("descr", i, cont.MstElements.Description);

                i++;

            }

            mtCon.LoadFromDataSource();


            dtBonus.Rows.Clear();
            bonuses = from p in dbHrPayroll.MstBonus where p.FlgActive  == true select p;
            dtBonus.Rows.Clear();
            dtBonus.Rows.Add(bonuses.Count());
            i = 0;
            foreach (MstBonus bon in bonuses)
            {
                dtBonus.SetValue("isNew", i, "N");
                dtBonus.SetValue("id", i, bon.Id.ToString());
                dtBonus.SetValue("Code", i, bon.Code);
                dtBonus.SetValue("descr", i, bon.Description);

                i++;

            }

            mtBonus.LoadFromDataSource();

            dtOt.Rows.Clear();
            overtimes = from p in dbHrPayroll.MstOverTime where p.FlgActive == true select p;
            dtOt.Rows.Clear();
            dtOt.Rows.Add(overtimes.Count());
            i = 0;
            foreach (MstOverTime ot in overtimes)
            {
                dtOt.SetValue("isNew", i, "N");
                dtOt.SetValue("id", i, ot.ID.ToString());
                dtOt.SetValue("Code", i, ot.Code);
                dtOt.SetValue("descr", i, ot.Description);

                i++;

            }

            mtOt.LoadFromDataSource();
           
        }
        private void addEmptyRow()
        {


            if (dtAdv.Rows.Count == 0)
            {
                dtAdv.Rows.Add(1);
                dtAdv.SetValue("isNew", 0, "Y");
                dtAdv.SetValue("id", 0, 0);
                dtAdv.SetValue("advCode", 0, "");
                dtAdv.SetValue("Desc", 0, "");
                dtAdv.SetValue("Active", 0, "N");
                mtAdv.AddRow(1, mtAdv.RowCount + 1);
            }
            else
            {
                if (dtAdv.GetValue("advCode", dtAdv.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtAdv.Rows.Add(1);
                    dtAdv.SetValue("isNew", dtAdv.Rows.Count - 1, "Y");
                    dtAdv.SetValue("advCode", dtAdv.Rows.Count - 1, "");
                    dtAdv.SetValue("Desc", dtAdv.Rows.Count - 1, "");
                    dtAdv.SetValue("Active", dtAdv.Rows.Count - 1, "N");
                    mtAdv.AddRow(1, mtAdv.RowCount + 1);
                }

            }
           // mtAdv.FlushToDataSource();
           
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

        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCfl(ref pVal, ref BubbleEvent);

           

            string itemId = pVal.ItemUID;
            SAPbouiCOM.ChooseFromListEvent ocfl = (SAPbouiCOM.ChooseFromListEvent)pVal;
            SAPbouiCOM.Item cflItem = oForm.Items.Item(itemId);
            SAPbouiCOM.DataTable oDT = ocfl.SelectedObjects;
            if (cflItem.Type.ToString() == "it_EDIT")
            {
                SAPbouiCOM.EditText txt = oForm.Items.Item(itemId).Specific ;
                oForm.DataSources.UserDataSources.Item(itemId).ValueEx = oDT.GetValue("AcctCode", 0);
                SAPbouiCOM.StaticText st = oForm.Items.Item("l" + itemId).Specific;
                st.Caption = oDT.GetValue("AcctName", 0);
            }
            if (cflItem.Type.ToString() == "it_MATRIX")
            {
                int i = 0;
                    
                SAPbouiCOM.Matrix mat=oForm.Items.Item(itemId).Specific;
               // SAPbouiCOM.Column col;
                foreach(SAPbouiCOM.Column col in mat.Columns)
                {
                    if (col.UniqueID != "V_-1")
                    {
                        if (col.ChooseFromListUID == ocfl.ChooseFromListUID)
                        {
                            string tablename = col.DataBind.TableName;
                            string fieldname = col.DataBind.Alias;
                            string code = oDT.GetValue("AcctCode", 0);
                            string descr = oDT.GetValue("AcctName", 0);
                            SAPbouiCOM.DataTable dt = oForm.DataSources.DataTables.Item(col.DataBind.TableName);


                            dt.SetValue(fieldname, pVal.Row - 1, code);
                            dt.SetValue(i, pVal.Row - 1, descr);

                            // mat.FlushToDataSource();
                            mat.SetLineData(pVal.Row);
                            oForm.Freeze(true);
                            mat.LoadFromDataSource();
                            oForm.Freeze(false);
                        }
                    }
                    i++;
                }
            }
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                           
            //oApplication.MessageBox(cflItem.Type.ToString());


        }
        public void setAcct()
        {

        }
        public void _fillFields()
        {
            MstGLDetermination gl;


            int cnt = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "COMP" && p.GLValue == 0 select p).Count();

            if (cnt > 0)
            {
                
                gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "COMP" && p.GLValue == 0 select p).Single();
                txArrE.Value = gl.ArrearsExpense;
                txArrP.Value = gl.ArrearsPayable;
                txBasicE.Value = gl.BasicSalary;
                txBasicP.Value = gl.BSPayable;
                txDff.Value = gl.DiffDRCR;
                txEOSE.Value = gl.EOSExpese;
                txEOSP.Value = gl.EOSPayable;
                txGrtA.Value = gl.GratuityExpense;
                txGrtP.Value = gl.GratuityPayable;
                txITE.Value = gl.IncomeTaxExpense;
                txITP.Value = gl.IncomeTaxPayable;
                txLevE.Value = gl.LeaveEncashmentExpense;
                txLvP.Value = gl.LeaveEncashmentPayable;
                //dtEarnings.Rows.Clear();
                GLDID.Value = gl.Id.ToString();
                int i = 0;
                foreach (MstGLDEarningDetail edEtail in gl.MstGLDEarningDetail)
                {
                   // dtEarnings.SetValue("id", i, edEtail.Id.ToString());
                    dtEarnings.SetValue("CostAcct", i, edEtail.CostAccout);
                    dtEarnings.SetValue("BalAcct", i, edEtail.BalancingAccount);
                    dtEarnings.SetValue("CostDesc", i, edEtail.CostAcctDisplay);
                    dtEarnings.SetValue("BalDesc", i, edEtail.BalancingAcctDisplay);
                    i++;
                }
                mtEarnings.LoadFromDataSource();
                i = 0;

                foreach (MstGLDDeductionDetail eDetail in gl.MstGLDDeductionDetail)
                {
                    dtDeduct.SetValue("CostAcct", i, eDetail.CostAccount);
                    dtDeduct.SetValue("BalAcct", i, eDetail.BalancingAccount);
                    dtDeduct.SetValue("CostDesc", i, eDetail.CostAcctDisplay);
                    dtDeduct.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay);
                    i++;
                }
                mtDed.LoadFromDataSource();
                i = 0;

                foreach (MstGLDContribution eDetail in gl.MstGLDContribution)
                {
                    dtCont.SetValue("CostAcct", i, eDetail.CostAccount);
                    dtCont.SetValue("BalAcct", i, eDetail.BalancingAccount);
                    dtCont.SetValue("CostDesc", i, eDetail.CostAcctDisplay);
                    dtCont.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay);
                    i++;
                }
                mtCon.LoadFromDataSource();
                i = 0;

                foreach (MstGLDLoansDetails eDetail in gl.MstGLDLoansDetails)
                {
                    dtLoan.SetValue("CostAcct", i, eDetail.CostAccount);
                    dtLoan.SetValue("BalAcct", i, eDetail.BalancingAccount);
                    dtLoan.SetValue("CostDesc", i, eDetail.CostAcctDisplay);
                    dtLoan.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay);
                    i++;
                }
                mtLoan.LoadFromDataSource();
                i = 0;

                foreach (MstGLDAdvanceDetail eDetail in gl.MstGLDAdvanceDetail)
                {
                    dtAdv.SetValue("CostAcct", i, eDetail.CostAccount);
                    dtAdv.SetValue("BalAcct", i, eDetail.BalancingAccount);
                    dtAdv.SetValue("CostDesc", i, eDetail.CostAcctDisplay);
                    dtAdv.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay);
                    i++;
                }
                mtAdv.LoadFromDataSource();
                i = 0;

                foreach (MstGLDOverTimeDetail eDetail in gl.MstGLDOverTimeDetail)
                {
                    dtOt.SetValue("CostAcct", i, eDetail.CostAccount);
                    dtOt.SetValue("BalAcct", i, eDetail.BalancingAccount);
                    dtOt.SetValue("CostDesc", i, eDetail.CostAcctDisplay);
                    dtOt.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay);
                    i++;
                }
                mtOt.LoadFromDataSource();
                i = 0;

                foreach (MstGLDBonusDetail eDetail in gl.MstGLDBonusDetail)
                {
                    dtBonus.SetValue("CostAcct", i, eDetail.CostAccount);
                    dtBonus.SetValue("BalAcct", i, eDetail.BalancingAccount);
                    dtBonus.SetValue("CostDesc", i, eDetail.CostAcctDisplay);
                    dtBonus.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay);
                    i++;
                }
                mtBonus.LoadFromDataSource();
                i = 0;


            }
        }
        private void updateDbWithMat()
        {

            MstGLDetermination gl;


            int cnt = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "COMP" && p.GLValue == 0 select p).Count();

            if (cnt > 0)
            {
                gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == "COMP" && p.GLValue == 0 select p).Single();
            }
            else
            {
                gl = new MstGLDetermination();
                gl.GLType = "COMP";
                gl.CreateDate = DateTime.Now;
                gl.UserId = oCompany.UserName;
                dbHrPayroll.MstGLDetermination.InsertOnSubmit(gl);
            }

            gl.UpdatedBy = oCompany.UserName;
            gl.UpdateDate = DateTime.Now;

            gl.BasicSalary = txBasicE.Value;
            gl.BSPayable = txBasicP.Value;
            gl.ArrearsExpense = txArrE.Value;
            gl.ArrearsPayable = txArrP.Value;
            gl.LeaveEncashmentExpense = txLevE.Value;
            gl.LeaveEncashmentPayable = txLvP.Value;
            gl.EOSExpese = txEOSE.Value;
            gl.EOSPayable = txEOSP.Value;
            gl.GratuityPayable = txGrtP.Value;
            gl.GratuityExpense = txGrtA.Value;
            gl.IncomeTaxPayable = txITP.Value;
            gl.IncomeTaxExpense = txITE.Value;
            gl.DiffDRCR = txDff.Value;
            gl.GLType = "COMP";
            gl.GLValue = 0;

            for (int i = 0; i < dtEarnings.Rows.Count; i++)
            {
                string id = dtEarnings.GetValue("id", i);
                MstGLDEarningDetail edEtail;
                int erningcnt = (from p in dbHrPayroll.MstGLDEarningDetail where p.GLDId.ToString()==GLDID.Value.ToString() && p.ElementId.ToString()==id.ToString() select p).Count();
                if (erningcnt > 0)
                {
                    edEtail = (from p in dbHrPayroll.MstGLDEarningDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.ElementId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    edEtail = new MstGLDEarningDetail();
                    gl.MstGLDEarningDetail.Add(edEtail);
                    edEtail.ElementId = Convert.ToInt16(id);
                    edEtail.CreateDate = DateTime.Now;
                    edEtail.UserId = oCompany.UserName;

                }
                edEtail.UpdatedBy = oCompany.UserName;
                edEtail.UpdateDate = DateTime.Now;
                
                edEtail.CostAccout = dtEarnings.GetValue("CostAcct", i);
                edEtail.BalancingAccount = dtEarnings.GetValue("BalAcct", i);
                edEtail.CostAcctDisplay = dtEarnings.GetValue("CostDesc", i);
                edEtail.BalancingAcctDisplay = dtEarnings.GetValue("BalDesc", i);

                
            }

            for (int i = 0; i < dtDeduct.Rows.Count; i++)
            {
                string id = dtDeduct.GetValue("id", i);
                MstGLDDeductionDetail eDetail;
                int dedCnt = (from p in dbHrPayroll.MstGLDDeductionDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.DeductionId.ToString() == id.ToString() select p).Count();
                if (dedCnt > 0)
                {
                    eDetail = (from p in dbHrPayroll.MstGLDDeductionDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.DeductionId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    eDetail = new MstGLDDeductionDetail();

                    eDetail.DeductionId = Convert.ToInt16(id);
                    gl.MstGLDDeductionDetail.Add(eDetail);
                    eDetail.CreateDate = DateTime.Now;
                    eDetail.UserId = oCompany.UserName;

                }
                eDetail.UpdatedBy = oCompany.UserName;
                eDetail.UpdateDate = DateTime.Now;

                eDetail.CostAccount = dtDeduct.GetValue("CostAcct", i);
                eDetail.BalancingAccount = dtDeduct.GetValue("BalAcct", i);
                eDetail.CostAcctDisplay = dtDeduct.GetValue("CostDesc", i);
                eDetail.BalancingAcctDisplay = dtDeduct.GetValue("BalDesc", i);

            }

            for (int i = 0; i < dtCont.Rows.Count; i++)
            {
                string id = dtCont.GetValue("id", i);
                MstGLDContribution eDetail;
                int dedCnt = (from p in dbHrPayroll.MstGLDContribution where p.GLDId.ToString() == GLDID.Value.ToString() && p.ContributionId.ToString() == id.ToString() select p).Count();
                if (dedCnt > 0)
                {
                    eDetail = (from p in dbHrPayroll.MstGLDContribution where p.GLDId.ToString() == GLDID.Value.ToString() && p.ContributionId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    eDetail = new MstGLDContribution();

                    eDetail.ContributionId = Convert.ToInt16(id);
                    gl.MstGLDContribution.Add(eDetail);
                    eDetail.CreateDate = DateTime.Now;
                    eDetail.UserId = oCompany.UserName;

                }
                eDetail.UpdatedBy = oCompany.UserName;
                eDetail.UpdateDate = DateTime.Now;
                eDetail.CostAccount = dtCont.GetValue("CostAcct", i);
                eDetail.BalancingAccount = dtCont.GetValue("BalAcct", i);
                eDetail.CostAcctDisplay = dtCont.GetValue("CostDesc", i);
                eDetail.BalancingAcctDisplay = dtCont.GetValue("BalDesc", i);

            }

            for (int i = 0; i < dtLoan.Rows.Count; i++)
            {
                string id = dtLoan.GetValue("id", i);
                MstGLDLoansDetails eDetail;
                int dedCnt = (from p in dbHrPayroll.MstGLDLoansDetails where p.GLDId.ToString() == GLDID.Value.ToString() && p.LoanId.ToString() == id.ToString() select p).Count();
                if (dedCnt > 0)
                {
                    eDetail = (from p in dbHrPayroll.MstGLDLoansDetails where p.GLDId.ToString() == GLDID.Value.ToString() && p.LoanId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    eDetail = new MstGLDLoansDetails();

                    eDetail.LoanId = Convert.ToInt16(id);
                    gl.MstGLDLoansDetails.Add(eDetail);
                    eDetail.CreateDate = DateTime.Now;
                    eDetail.UserId = oCompany.UserName;

                }
                eDetail.UpdatedBy = oCompany.UserName;
                eDetail.UpdateDate = DateTime.Now;
                eDetail.CostAccount = dtLoan.GetValue("CostAcct", i);
                eDetail.BalancingAccount = dtLoan.GetValue("BalAcct", i);
                eDetail.CostAcctDisplay = dtLoan.GetValue("CostDesc", i);
                eDetail.BalancingAcctDisplay = dtLoan.GetValue("BalDesc", i);

            }

            for (int i = 0; i < dtAdv.Rows.Count; i++)
            {
                string id = dtAdv.GetValue("id", i);
                MstGLDAdvanceDetail eDetail;
                int dedCnt = (from p in dbHrPayroll.MstGLDAdvanceDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.AdvancesId.ToString() == id.ToString() select p).Count();
                if (dedCnt > 0)
                {
                    eDetail = (from p in dbHrPayroll.MstGLDAdvanceDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.AdvancesId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    eDetail = new MstGLDAdvanceDetail();

                    eDetail.AdvancesId = Convert.ToInt16(id);
                    gl.MstGLDAdvanceDetail.Add(eDetail);
                    eDetail.CreateDate = DateTime.Now;
                    eDetail.UserId = oCompany.UserName;

                }
                eDetail.UpdatedBy = oCompany.UserName;
                eDetail.UpdateDate = DateTime.Now;
                eDetail.CostAccount = dtAdv.GetValue("CostAcct", i);
                eDetail.BalancingAccount = dtAdv.GetValue("BalAcct", i);
                eDetail.CostAcctDisplay = dtAdv.GetValue("CostDesc", i);
                eDetail.BalancingAcctDisplay = dtAdv.GetValue("BalDesc", i);

            }

            for (int i = 0; i < dtOt.Rows.Count; i++)
            {
                string id = dtOt.GetValue("id", i);
                MstGLDOverTimeDetail eDetail;
                int dedCnt = (from p in dbHrPayroll.MstGLDOverTimeDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.OvertimeId.ToString() == id.ToString() select p).Count();
                if (dedCnt > 0)
                {
                    eDetail = (from p in dbHrPayroll.MstGLDOverTimeDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.OvertimeId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    eDetail = new MstGLDOverTimeDetail();

                    eDetail.OvertimeId = Convert.ToInt16(id);
                    gl.MstGLDOverTimeDetail.Add(eDetail);
                    eDetail.CreateDate = DateTime.Now;
                    eDetail.UserId = oCompany.UserName;

                }
                eDetail.UpdatedBy = oCompany.UserName;
                eDetail.UpdateDate = DateTime.Now;

                eDetail.CostAccount = dtOt.GetValue("CostAcct", i);
                eDetail.BalancingAccount = dtOt.GetValue("BalAcct", i);
                eDetail.CostAcctDisplay = dtOt.GetValue("CostDesc", i);
                eDetail.BalancingAcctDisplay = dtOt.GetValue("BalDesc", i);

            }

           
            for (int i = 0; i < dtBonus.Rows.Count; i++)
            {
                string id = dtBonus.GetValue("id", i);
                MstGLDBonusDetail eDetail;
                int dedCnt = (from p in dbHrPayroll.MstGLDBonusDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.BonusId.ToString() == id.ToString() select p).Count();
                if (dedCnt > 0)
                {
                    eDetail = (from p in dbHrPayroll.MstGLDBonusDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.BonusId.ToString() == id.ToString() select p).Single();
                }
                else
                {
                    eDetail = new MstGLDBonusDetail();
                    eDetail.BonusId = Convert.ToInt16(id);
                    gl.MstGLDBonusDetail.Add(eDetail);
                    eDetail.CreateDate = DateTime.Now;
                    eDetail.UserId = oCompany.UserName;

                }
                eDetail.UpdatedBy = oCompany.UserName;
                eDetail.UpdateDate = DateTime.Now;

                eDetail.CostAccount = dtBonus.GetValue("CostAcct", i);
                eDetail.BalancingAccount = dtBonus.GetValue("BalAcct", i);
                eDetail.CostAcctDisplay = dtBonus.GetValue("CostDesc", i);
                eDetail.BalancingAcctDisplay = dtBonus.GetValue("BalDesc", i);

            }
           
            dbHrPayroll.SubmitChanges();
        }
       
    }
}
