using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_GLAcctDetLoc : HRMSBaseForm
    {

        #region Variable
        /* Form Items Objects */
        SAPbouiCOM.Matrix mtEarnings, mtDed, mtCon, mtLoan, mtAdv, mtOt, mtBonus, mtLD, mtEX;
        SAPbouiCOM.Column coAdvCode, coAdvDesc, coAdvAct, isNew, id;
        private SAPbouiCOM.DataTable dtAdv, dtEarnings, dtDeduct, dtCont, dtLoan, dtOt, dtBonus, dtEX, dtLD;
        SAPbouiCOM.EditText txBasicE, txArrE, txLevE, txEOSE, txGrtA, txITE, txBasicP, txArrP, txLvP, txEOSP, txGrtP, txITP, txDff, GLDID, txLocation;
        SAPbouiCOM.ComboBox cbGltype;
        SAPbouiCOM.Item IcbGltype, ImtAdv, IcoAdvCode, IcoAdvDesc, IcoAdvAct, ItxBasicE, ItxArrE, ItxLevE, ItxEOSE, ItxGrtA, ItxITE, ItxBasicP, ItxArrP, ItxLvP, ItxEOSP, ItxGrtP, ItxITP, ItxDff, IGLDID, ItxLocation;
        SAPbouiCOM.Folder tbEarning, tbDeduction, tbContribution, tbLoan, tbAdvance, tbOvertime, tbLeaveDeduction, tbBonus, tbExpense;
        SAPbouiCOM.Item itbEarning, itbDeduction, itbContribution, itbLoan, itbAdvance, itbOvertime, itbLeaveDeduction, itbBonus, itbExpense;
        //**********************************

        public IEnumerable<MstAdvance> advances;
        public IEnumerable<MstLoans> loans;
        public IEnumerable<MstOverTime> overtimes;
        public IEnumerable<MstBonus> bonuses;
        public IEnumerable<MstElementEarning> earnings;
        public IEnumerable<MstElementDeduction> deductions;
        public IEnumerable<MstElementContribution> contributions;
        public IEnumerable<MstExpense> expenses;
        public IEnumerable<MstLeaveDeduction> leavedeductions;

        public int SelId = 0;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.EnableMenu("1282", false); //Add Disable
            oForm.EnableMenu("1281", false); //Find Disable
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (pVal.Before_Action == false)
                    {
                        updateDbWithMat();
                    }
                    break;
                case "btDept":
                    pickDept();
                    break;
                case "btGlsCopy":
                    pickDeptforCopy();
                    break;
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "txLocation":
                    _fillFields();
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
            try
            {
                string acctcode = oDT.GetValue("AcctCode", 0);
            }
            catch
            {
                return;
            }
            if (cflItem.Type.ToString() == "it_EDIT")
            {
                SAPbouiCOM.EditText txt = oForm.Items.Item(itemId).Specific;
                oForm.DataSources.UserDataSources.Item(itemId).ValueEx = oDT.GetValue("AcctCode", 0);
                SAPbouiCOM.StaticText st = oForm.Items.Item("l" + itemId).Specific;
                st.Caption = oDT.GetValue("AcctName", 0);
            }
            if (cflItem.Type.ToString() == "it_MATRIX")
            {
                int i = 0;

                SAPbouiCOM.Matrix mat = oForm.Items.Item(itemId).Specific;
                // SAPbouiCOM.Column col;
                foreach (SAPbouiCOM.Column col in mat.Columns)
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

        #endregion

        #region Function

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

            tbEarning = oForm.Items.Item("6").Specific;
            itbEarning = oForm.Items.Item("6");
            tbDeduction = oForm.Items.Item("7").Specific;
            itbDeduction = oForm.Items.Item("7");
            tbContribution = oForm.Items.Item("8").Specific;
            itbContribution = oForm.Items.Item("8");
            tbLoan = oForm.Items.Item("9").Specific;
            itbLoan = oForm.Items.Item("9");
            tbAdvance = oForm.Items.Item("10").Specific;
            itbAdvance = oForm.Items.Item("10");
            tbOvertime = oForm.Items.Item("11").Specific;
            itbOvertime = oForm.Items.Item("11");
            tbLeaveDeduction = oForm.Items.Item("62").Specific;
            itbLeaveDeduction = oForm.Items.Item("62");
            //tbBonus = oForm.Items.Item("12").Specific;
            //itbBonus = oForm.Items.Item("12");
            //tbExpense = oForm.Items.Item("63").Specific;
            //itbExpense = oForm.Items.Item("63");

            //itbBonus.Visible = false;
            //itbExpense.Visible = false;

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

            oForm.DataSources.UserDataSources.Add("txLocation", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txLocation = oForm.Items.Item("txLocation").Specific;
            ItxLocation = oForm.Items.Item("txLocation");
            txLocation.DataBind.SetBound(true, "", "txLocation");


            oForm.DataSources.UserDataSources.Add("cbGltype", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            cbGltype = oForm.Items.Item("cbGltype").Specific;
            IcbGltype = oForm.Items.Item("cbGltype");
            cbGltype.DataBind.SetBound(true, "", "cbGltype");

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



            mtLD = oForm.Items.Item("mtLD").Specific;
            isNew = mtLD.Columns.Item("isNew");
            id = mtLD.Columns.Item("id");
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

            dtLD = oForm.DataSources.DataTables.Item("dtLD");
            dtLD.Rows.Clear();

            //, , , , , , 

            fillMat();
            //_fillFields();
            oForm.PaneLevel = 1;

            cbGltype.ValidValues.Add("COMP", "Company");
            cbGltype.ValidValues.Add("LOC", "Location");
            cbGltype.ValidValues.Add("DEPT", "Department");

            oForm.Freeze(false);

        }

        private void fillMat()
        {
            dtAdv.Rows.Clear();
            advances = from p in dbHrPayroll.MstAdvance  where p.FlgActive==true select p;
            dtAdv.Rows.Clear();
            dtAdv.Rows.Add(advances.Count());
            int i = 0;
            foreach (MstAdvance adv in advances)
            {
                dtAdv.SetValue("isNew", i, "N");
                dtAdv.SetValue("id", i, adv.Id);
                dtAdv.SetValue("Code", i, adv.AllowanceId.ToString());
                dtAdv.SetValue("descr", i, adv.Description.ToString());
                dtAdv.SetValue("indc", i, "");
                i++;

            }

            mtAdv.LoadFromDataSource();

            dtEarnings.Rows.Clear();
            earnings = from p in dbHrPayroll.MstElementEarning where p.MstElements.ElmtType == "Ear" select p;
            dtEarnings.Rows.Clear();
            dtEarnings.Rows.Add(earnings.Count());

            i = 0;
            foreach (MstElementEarning earning in earnings)
            {
                dtEarnings.SetValue("isNew", i, "N");
                dtEarnings.SetValue("id", i, earning.MstElements.Id.ToString());
                dtEarnings.SetValue("Code", i, earning.MstElements.ElementName);
                dtEarnings.SetValue("descr", i, earning.MstElements.Description);

                i++;

            }

            mtEarnings.LoadFromDataSource();

            dtDeduct.Rows.Clear();
            deductions = from p in dbHrPayroll.MstElementDeduction where p.MstElements.ElmtType == "Ded" select p;
            dtDeduct.Rows.Clear();
            dtDeduct.Rows.Add(deductions.Count());
            i = 0;
            foreach (MstElementDeduction deduct in deductions)
            {
                dtDeduct.SetValue("isNew", i, "N");
                dtDeduct.SetValue("id", i, deduct.MstElements.Id.ToString());
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
                dtLoan.SetValue("indc", i, "");
                i++;

            }

            mtLoan.LoadFromDataSource();

            dtCont.Rows.Clear();
            //contributions = from p in dbHrPayroll.MstElementContribution where p.Employee > 0 || p.Employer > 0 == true select p;
            contributions = from p in dbHrPayroll.MstElementContribution where p.MstElements.ElmtType == "Con" select p;
            dtCont.Rows.Clear();
            Int32 mfmtemp = contributions.Count();
            dtCont.Rows.Add(contributions.Count());
            i = 0;
            foreach (MstElementContribution cont in contributions)
            {
                dtCont.SetValue("isNew", i, "N");
                dtCont.SetValue("id", i, cont.MstElements.Id.ToString());
                dtCont.SetValue("Code", i, cont.MstElements.ElementName);
                dtCont.SetValue("descr", i, cont.MstElements.Description);

                i++;

            }

            mtCon.LoadFromDataSource();

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


            dtLD.Rows.Clear();
            leavedeductions = from p in dbHrPayroll.MstLeaveDeduction select p;
            dtLD.Rows.Clear();
            dtLD.Rows.Add(leavedeductions.Count());
            i = 0;
            foreach (MstLeaveDeduction ded in leavedeductions)
            {                
                dtLD.SetValue("isNew", i, "N");
                dtLD.SetValue("id", i, ded.Id.ToString());
                dtLD.SetValue("Code", i, ded.Code);
                dtLD.SetValue("descr", i, ded.Description);

                i++;

            }

            mtLD.LoadFromDataSource();

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
                dtAdv.SetValue("indc", 0, "");
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
                    dtAdv.SetValue("indc", dtAdv.Rows.Count - 1, "");
                    dtAdv.SetValue("Active", dtAdv.Rows.Count - 1, "N");
                    mtAdv.AddRow(1, mtAdv.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();

        }

        private void pickDeptforCopy()
        {
            SearchKeyVal.Clear();
            string picType = "";
            if (cbGltype.Value.Trim() == "COMP")
            {
                picType = "";
                // SelId = 0;
                // txLocation.Value = "Company";
            }

            if (cbGltype.Value.Trim() == "LOC")
            {
                picType = "glLoc";
            }
            if (cbGltype.Value.Trim() == "DEPT")
            {
                picType = "glDept";
            }
            if (picType != "")
            {
                string strSql = sqlString.getSql(picType, SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select", "Select");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    CopyGL(st.Rows[0][0].ToString());
                    // txLocation.Value = st.Rows[0][1].ToString();

                }
            }

        }

        private void pickDept()
        {
            SearchKeyVal.Clear();
            string picType = "";
            if (cbGltype.Value.Trim() == "COMP")
            {
                picType = "";
                SelId = 0;
                txLocation.Value = "Company";
            }

            if (cbGltype.Value.Trim() == "LOC")
            {
                picType = "glLoc";
            }
            if (cbGltype.Value.Trim() == "DEPT")
            {
                picType = "glDept";
            }
            if (picType != "")
            {
                string strSql = sqlString.getSql(picType, SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select", "Select");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    SelId = Convert.ToInt16(st.Rows[0][0].ToString());
                    txLocation.Value = st.Rows[0][1].ToString();

                }
            }
            _fillFields();
        }

        public void setAcct()
        {

        }

        private void inicontrolls()
        {
            SAPbouiCOM.StaticText st = oForm.Items.Item("ltxArrE").Specific;
            st.Caption = "";
            txArrE.Value = "";
            st = oForm.Items.Item("ltxArrP").Specific;
            st.Caption = "";
            txArrP.Value = "";
            st = oForm.Items.Item("ltxBasicE").Specific;
            st.Caption = "";
            txBasicE.Value = "";
            st = oForm.Items.Item("ltxBasicP").Specific;
            st.Caption = "";
            txBasicP.Value = "";
            st = oForm.Items.Item("ltxDff").Specific;
            st.Caption = "";
            txDff.Value = "";
            st = oForm.Items.Item("ltxEOSE").Specific;
            st.Caption = "";
            txEOSE.Value = "";
            st = oForm.Items.Item("ltxEOSP").Specific;
            st.Caption = "";
            txEOSP.Value = "";
            st = oForm.Items.Item("ltxGrtA").Specific;
            st.Caption = "";
            txGrtA.Value = "";
            st = oForm.Items.Item("ltxGrtP").Specific;
            st.Caption = "";
            txGrtP.Value = "";
            st = oForm.Items.Item("ltxITE").Specific;
            st.Caption = "";
            txITE.Value = "";
            st = oForm.Items.Item("ltxITP").Specific;
            st.Caption = "";
            txITP.Value = "";
            st = oForm.Items.Item("ltxLevE").Specific;
            st.Caption = "";
            txLevE.Value = "";
            st = oForm.Items.Item("ltxLvP").Specific;
            st.Caption = "";
            txLvP.Value = "";
            GLDID.Value = "";
            //SelId = 0;
            for (int i = 0; i < dtEarnings.Rows.Count; i++)
            {
                dtEarnings.SetValue("CostAcct", i, "");
                dtEarnings.SetValue("BalAcct", i, "");
                dtEarnings.SetValue("CostDesc", i, "");
                dtEarnings.SetValue("BalDesc", i, "");
            }
            mtEarnings.LoadFromDataSource();
            for (int i = 0; i < dtDeduct.Rows.Count; i++)
            {
                dtDeduct.SetValue("CostAcct", i, "");
                dtDeduct.SetValue("BalAcct", i, "");
                dtDeduct.SetValue("CostDesc", i, "");
                dtDeduct.SetValue("BalDesc", i, "");

            }
            mtDed.LoadFromDataSource();
            for (int i = 0; i < dtCont.Rows.Count; i++)
            {
                dtCont.SetValue("CostAcct", i, "");
                dtCont.SetValue("BalAcct", i, "");
                dtCont.SetValue("CostDesc", i, "");
                dtCont.SetValue("BalDesc", i, "");
                dtCont.SetValue("eCostAcct", i, "");
                dtCont.SetValue("eBalAcct", i, "");
                dtCont.SetValue("eCostDesc", i, "");
                dtCont.SetValue("eBalDesc", i, "");
            }
            mtCon.LoadFromDataSource();
            for (int i = 0; i < dtLoan.Rows.Count; i++)
            {
                dtLoan.SetValue("CostAcct", i, "");
                dtLoan.SetValue("BalAcct", i, "");
                dtLoan.SetValue("CostDesc", i, "");
                dtLoan.SetValue("BalDesc", i, "");
                dtLoan.SetValue("indc", i, "");
            }
            mtLoan.LoadFromDataSource();
            for (int i = 0; i < dtAdv.Rows.Count; i++)
            {
                dtAdv.SetValue("CostAcct", i, "");
                dtAdv.SetValue("BalAcct", i, "");
                dtAdv.SetValue("CostDesc", i, "");
                dtAdv.SetValue("BalDesc", i, "");
                dtAdv.SetValue("indc", i, "");
            }
            mtAdv.LoadFromDataSource();
            for (int i = 0; i < dtOt.Rows.Count; i++)
            {
                dtOt.SetValue("CostAcct", i, "");
                dtOt.SetValue("BalAcct", i, "");
                dtOt.SetValue("CostDesc", i, "");
                dtOt.SetValue("BalDesc", i, "");

            }
            mtOt.LoadFromDataSource();


            for (int i = 0; i < dtLD.Rows.Count; i++)
            {
                dtLD.SetValue("CostAcct", i, "");
                dtLD.SetValue("BalAcct", i, "");
                dtLD.SetValue("CostDesc", i, "");
                dtLD.SetValue("BalDesc", i, "");
            }
            mtLD.LoadFromDataSource();


        }

        public void _fillFields()
        {
            string LastAcctCode = string.Empty;
            if (txLocation.Value.ToString().Trim() == "") return;
            oForm.Freeze(true);
            try
            {
                MstGLDetermination gl;

                inicontrolls();
                int cnt = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == SelId.ToString() select p).Count();
                if (cnt == 0)
                {
                    //inicontrolls();
                }
                if (cnt == 0)
                {

                    //gl = new MstGLDetermination();
                    //gl.GLType = "DEPT";
                    //gl.GLValue = dept.ID;
                    //SelId = dept.ID;
                    //dbHrPayroll.MstGLDetermination.InsertOnSubmit(gl);
                    //dbHrPayroll.SubmitChanges();
                }
                else
                {

                    gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == SelId.ToString() select p).FirstOrDefault();

                    LastAcctCode = gl.ArrearsExpense == null ? "" : gl.ArrearsExpense.Trim();
                    txArrE.Value = gl.ArrearsExpense == null ? "" : gl.ArrearsExpense.Trim();
                    LastAcctCode = gl.ArrearsPayable == null ? "" : gl.ArrearsPayable.Trim();
                    txArrP.Value = gl.ArrearsPayable == null ? "" : gl.ArrearsPayable.Trim();
                    LastAcctCode = gl.BasicSalary == null ? "" : gl.BasicSalary.Trim();
                    txBasicE.Value = gl.BasicSalary == null ? "" : gl.BasicSalary.Trim();
                    LastAcctCode = gl.BSPayable == null ? "" : gl.BSPayable.Trim();
                    txBasicP.Value = gl.BSPayable == null ? "" : gl.BSPayable.Trim();
                    LastAcctCode = gl.DiffDRCR == null ? "" : gl.DiffDRCR.Trim();
                    txDff.Value = gl.DiffDRCR == null ? "" : gl.DiffDRCR.Trim();
                    LastAcctCode = gl.EOSExpese == null ? "" : gl.EOSExpese.Trim();
                    txEOSE.Value = gl.EOSExpese == null ? "" : gl.EOSExpese.Trim();
                    LastAcctCode = gl.EOSPayable == null ? "" : gl.EOSPayable.Trim();
                    txEOSP.Value = gl.EOSPayable == null ? "" : gl.EOSPayable.Trim();
                    LastAcctCode = gl.GratuityExpense == null ? "" : gl.GratuityExpense.Trim();
                    txGrtA.Value = gl.GratuityExpense == null ? "" : gl.GratuityExpense.Trim();
                    LastAcctCode = gl.GratuityPayable == null ? "" : gl.GratuityPayable.Trim();
                    txGrtP.Value = gl.GratuityPayable == null ? "" : gl.GratuityPayable.Trim();
                    LastAcctCode = gl.IncomeTaxExpense == null ? "" : gl.IncomeTaxExpense.Trim();
                    txITE.Value = gl.IncomeTaxExpense == null ? "" : gl.IncomeTaxExpense.Trim();
                    LastAcctCode = gl.IncomeTaxPayable == null ? "" : gl.IncomeTaxPayable.Trim();
                    txITP.Value = gl.IncomeTaxPayable == null ? "" : gl.IncomeTaxPayable.Trim();
                    LastAcctCode = gl.LeaveEncashmentExpense == null ? "" : gl.LeaveEncashmentExpense.Trim();
                    txLevE.Value = gl.LeaveEncashmentExpense == null ? "" : gl.LeaveEncashmentExpense.Trim();
                    LastAcctCode = gl.LeaveEncashmentPayable == null ? "" : gl.LeaveEncashmentPayable.Trim();
                    txLvP.Value = gl.LeaveEncashmentPayable == null ? "" : gl.LeaveEncashmentPayable.Trim();
                    //dtEarnings.Rows.Clear();
                    //GLDID.Value = gl.Id.ToString();
                    GLDID.Value = Convert.ToString(gl.Id != null ? gl.Id : 0);
                    //SelId = Convert.ToInt32(gl.GLValue != null ? gl.GLValue : 0);
                    int i = 0;
                    foreach (MstGLDEarningDetail edEtail in gl.MstGLDEarningDetail)
                    {
                        LastAcctCode = edEtail.CostAccout == null ? "" : edEtail.CostAccout.Trim();
                        dtEarnings.SetValue("CostAcct", i, edEtail.CostAccout == null ? "" : edEtail.CostAccout.Trim());
                        LastAcctCode = edEtail.BalancingAccount == null ? "" : edEtail.BalancingAccount.Trim();
                        dtEarnings.SetValue("BalAcct", i, edEtail.BalancingAccount == null ? "" : edEtail.BalancingAccount.Trim());
                        LastAcctCode = edEtail.CostAcctDisplay == null ? "" : edEtail.CostAcctDisplay.Trim();
                        dtEarnings.SetValue("CostDesc", i, edEtail.CostAcctDisplay == null ? "" : edEtail.CostAcctDisplay.Trim());
                        LastAcctCode = edEtail.BalancingAcctDisplay == null ? "" : edEtail.BalancingAcctDisplay.Trim();
                        dtEarnings.SetValue("BalDesc", i, edEtail.BalancingAcctDisplay == null ? "" : edEtail.BalancingAcctDisplay.Trim());
                        i++;
                    }
                    mtEarnings.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDDeductionDetail eDetail in gl.MstGLDDeductionDetail)
                    {
                        LastAcctCode = eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim();
                        dtDeduct.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim());
                        LastAcctCode = eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim();
                        dtDeduct.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim());
                        LastAcctCode = eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim();
                        dtDeduct.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim());
                        LastAcctCode = eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim();
                        dtDeduct.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim());
                        i++;
                    }
                    mtDed.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDContribution eDetail in gl.MstGLDContribution)
                    {
                        for (Int32 j = 0; j < gl.MstGLDContribution.Count; j++)
                        {
                            string idvalue = dtCont.GetValue("id", j);
                            if (idvalue == eDetail.ContributionId.ToString())
                            {
                                LastAcctCode = eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim();
                                dtCont.SetValue("CostAcct", j, eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim());
                                LastAcctCode = eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim();
                                dtCont.SetValue("BalAcct", j, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim());
                                LastAcctCode = eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim();
                                dtCont.SetValue("CostDesc", j, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim());
                                LastAcctCode = eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim();
                                dtCont.SetValue("BalDesc", j, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim());
                                LastAcctCode = eDetail.EmprCostAccount == null ? "" : eDetail.EmprCostAccount.Trim();
                                dtCont.SetValue("eCostAcct", j, eDetail.EmprCostAccount == null ? "" : eDetail.EmprCostAccount.Trim());
                                LastAcctCode = eDetail.EmprBalancingAccount == null ? "" : eDetail.EmprBalancingAccount.Trim();
                                dtCont.SetValue("eBalAcct", j, eDetail.EmprBalancingAccount == null ? "" : eDetail.EmprBalancingAccount.Trim());
                                LastAcctCode = eDetail.EmprCostAcctDisplay == null ? "" : eDetail.EmprCostAcctDisplay.Trim();
                                dtCont.SetValue("eCostDesc", j, eDetail.EmprCostAcctDisplay == null ? "" : eDetail.EmprCostAcctDisplay.Trim());
                                LastAcctCode = eDetail.EmprBalancingAcctDisplay == null ? "" : eDetail.EmprBalancingAcctDisplay.Trim();
                                dtCont.SetValue("eBalDesc", j, eDetail.EmprBalancingAcctDisplay == null ? "" : eDetail.EmprBalancingAcctDisplay.Trim());

                                i++;
                            }
                        }
                    }
                    mtCon.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDLoansDetails eDetail in gl.MstGLDLoansDetails)
                    {

                        int ID = eDetail.Id;
                        int LoanID = Convert.ToInt32(eDetail.LoanId);
                        var oLoan = (from a in dbHrPayroll.MstLoans where a.Id == LoanID select a).FirstOrDefault();
                        if (oLoan.FlgActive == false)
                        {
                            continue;
                        }
                        LastAcctCode = eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim();
                        dtLoan.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim());
                        LastAcctCode = eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim();
                        dtLoan.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim());
                        LastAcctCode = eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim();
                        dtLoan.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim());
                        LastAcctCode = eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim();
                        dtLoan.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim());
                        LastAcctCode = eDetail.A1Indicator == null ? "" : eDetail.A1Indicator.Trim();
                        dtLoan.SetValue("indc", i, eDetail.A1Indicator == null ? "" : eDetail.A1Indicator.Trim());
                        i++;
                    }
                    mtLoan.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDAdvanceDetail eDetail in gl.MstGLDAdvanceDetail)
                    {
                        int ID = eDetail.Id;
                        int AdvaneID = Convert.ToInt32(eDetail.AdvancesId);
                        var oAdvance = (from a in dbHrPayroll.MstAdvance where a.Id == AdvaneID select a).FirstOrDefault();
                        if (oAdvance.FlgActive == false)
                        {
                            continue;
                        }
                        LastAcctCode = eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim();
                        dtAdv.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim());
                        LastAcctCode = eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim();
                        dtAdv.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim());
                        LastAcctCode = eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim();
                        dtAdv.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim());
                        LastAcctCode = eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim();
                        dtAdv.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim());
                        LastAcctCode = eDetail.A1Indicator == null ? "" : eDetail.A1Indicator.Trim();
                        dtAdv.SetValue("indc", i, eDetail.A1Indicator == null ? "" : eDetail.A1Indicator.Trim());
                        i++;
                    }
                    mtAdv.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDOverTimeDetail eDetail in gl.MstGLDOverTimeDetail)
                    {
                        int ID = eDetail.Id;
                        int OverTimeID = Convert.ToInt32(eDetail.OvertimeId);
                        var oOverTime = (from a in dbHrPayroll.MstOverTime where a.ID == OverTimeID select a).FirstOrDefault();
                        if (oOverTime.FlgActive == false)
                        {
                            continue;
                        }
                        LastAcctCode = eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim();
                        dtOt.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim());
                        LastAcctCode = eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim();
                        dtOt.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim());
                        LastAcctCode = eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim();
                        dtOt.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim());
                        LastAcctCode = eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim();
                        dtOt.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim());
                        i++;
                    }
                    mtOt.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDLeaveDedDetails eDetail in gl.MstGLDLeaveDedDetails)
                    {
                        int ID = eDetail.Id;
                        int LeaveID = Convert.ToInt32(eDetail.LeaveDedId);
                        var oLeave = (from a in dbHrPayroll.MstLeaveType where a.ID == LeaveID select a).FirstOrDefault();
                        if (oLeave.Active == false)
                        {
                            continue;
                        }
                        LastAcctCode = eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim();
                        dtLD.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount.Trim());
                        LastAcctCode = eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim();
                        dtLD.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount.Trim());
                        LastAcctCode = eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim();
                        dtLD.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay.Trim());
                        LastAcctCode = eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim();
                        dtLD.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay.Trim());
                        i++;
                    }
                    mtLD.LoadFromDataSource();
                    i = 0;

                }
                ItxBasicE.Click();
            }
            catch (Exception ex)
            {
                int confirm = oApplication.MessageBox("This account code is missing / deleted from COA : " + LastAcctCode + "\nYou need to re-perform G/L determination for selected option.\nClick 'Yes' for Refresh, Click 'No' to proceed with error.", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3)
                {
                }
                else
                {

                    int cnt = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == SelId.ToString() select p).Count();
                    if (cnt > 0)
                    {
                        MstGLDetermination gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == SelId.ToString() select p).Single();
                        dbHrPayroll.MstGLDetermination.DeleteOnSubmit(gl);
                        dbHrPayroll.SubmitChanges();
                        SelId = 0;
                    }
                }

            }
            oForm.Freeze(false);
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
        }

        public string SaveAcctAssignment(string pValue)
        {
            try
            {
                return "";
            }
            catch
            {
                return "";
            }
        }

        private void updateDbWithMat()
        {

            try
            {
                MstGLDetermination gl;
                Boolean flgEarning, flgDeduction, flgContribution, flgLoan, flgAdvance, flgOvertime, flgLeaveDeduction;

                int cnt = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue == SelId select p).Count();

                if (cnt > 0)
                {
                    gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue == SelId select p).FirstOrDefault();
                }
                else
                {
                    gl = new MstGLDetermination();
                    gl.GLType = cbGltype.Value.Trim();
                    gl.GLValue = SelId;
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
                string tempgldvalue = GLDID.Value.Trim();
                mtEarnings.FlushToDataSource();
                for (int i = 0; i < dtEarnings.Rows.Count; i++)
                {
                    string id = dtEarnings.GetValue("id", i);
                    id = id.Trim();
                    MstGLDEarningDetail edEtail;
                    int erningcnt = (from p in dbHrPayroll.MstGLDEarningDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.ElementId.ToString() == id.ToString() select p).Count();
                    if (erningcnt > 0)
                    {
                        edEtail = (from p in dbHrPayroll.MstGLDEarningDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.ElementId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            edEtail = new MstGLDEarningDetail();
                            gl.MstGLDEarningDetail.Add(edEtail);
                            edEtail.ElementId = Convert.ToInt16(id);
                            edEtail.CreateDate = DateTime.Now;
                            edEtail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    edEtail.UpdatedBy = oCompany.UserName;
                    edEtail.UpdateDate = DateTime.Now;

                    edEtail.CostAccout = dtEarnings.GetValue("CostAcct", i);
                    edEtail.BalancingAccount = dtEarnings.GetValue("BalAcct", i);
                    edEtail.CostAcctDisplay = dtEarnings.GetValue("CostDesc", i);
                    edEtail.BalancingAcctDisplay = dtEarnings.GetValue("BalDesc", i);


                }

                mtDed.FlushToDataSource();
                for (int i = 0; i < dtDeduct.Rows.Count; i++)
                {
                    string id = dtDeduct.GetValue("id", i);
                    MstGLDDeductionDetail eDetail;
                    int dedCnt = (from p in dbHrPayroll.MstGLDDeductionDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.DeductionId.ToString() == id.ToString() select p).Count();
                    if (dedCnt > 0)
                    {
                        eDetail = (from p in dbHrPayroll.MstGLDDeductionDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.DeductionId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            eDetail = new MstGLDDeductionDetail();
                            eDetail.DeductionId = Convert.ToInt16(id);
                            gl.MstGLDDeductionDetail.Add(eDetail);
                            eDetail.CreateDate = DateTime.Now;
                            eDetail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    eDetail.UpdatedBy = oCompany.UserName;
                    eDetail.UpdateDate = DateTime.Now;

                    eDetail.CostAccount = dtDeduct.GetValue("CostAcct", i);
                    eDetail.BalancingAccount = dtDeduct.GetValue("BalAcct", i);
                    eDetail.CostAcctDisplay = dtDeduct.GetValue("CostDesc", i);
                    eDetail.BalancingAcctDisplay = dtDeduct.GetValue("BalDesc", i);

                }

                mtCon.FlushToDataSource();
                for (int i = 0; i < dtCont.Rows.Count; i++)
                {
                    string id = dtCont.GetValue("id", i);
                    MstGLDContribution eDetail;
                    int dedCnt = (from p in dbHrPayroll.MstGLDContribution where p.GLDId.ToString() == GLDID.Value.ToString() && p.ContributionId.ToString() == id.ToString() select p).Count();
                    if (dedCnt > 0)
                    {
                        eDetail = (from p in dbHrPayroll.MstGLDContribution where p.GLDId.ToString() == GLDID.Value.ToString() && p.ContributionId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            eDetail = new MstGLDContribution();
                            eDetail.ContributionId = Convert.ToInt16(id);
                            gl.MstGLDContribution.Add(eDetail);
                            eDetail.CreateDate = DateTime.Now;
                            eDetail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    eDetail.UpdatedBy = oCompany.UserName;
                    eDetail.UpdateDate = DateTime.Now;
                    string costaccountvalue = dtCont.GetValue("CostAcct", i);
                    eDetail.CostAccount = dtCont.GetValue("CostAcct", i);
                    eDetail.BalancingAccount = dtCont.GetValue("BalAcct", i);
                    eDetail.CostAcctDisplay = dtCont.GetValue("CostDesc", i);
                    eDetail.BalancingAcctDisplay = dtCont.GetValue("BalDesc", i);

                    eDetail.EmprCostAccount = dtCont.GetValue("eCostAcct", i);
                    eDetail.EmprBalancingAccount = dtCont.GetValue("eBalAcct", i);
                    eDetail.EmprCostAcctDisplay = dtCont.GetValue("eCostDesc", i);
                    eDetail.EmprBalancingAcctDisplay = dtCont.GetValue("eBalDesc", i);

                }

                mtLoan.FlushToDataSource();
                for (int i = 0; i < dtLoan.Rows.Count; i++)
                {
                    string id = dtLoan.GetValue("id", i);
                    MstGLDLoansDetails eDetail;
                    int dedCnt = (from p in dbHrPayroll.MstGLDLoansDetails where p.GLDId.ToString() == GLDID.Value.ToString() && p.LoanId.ToString() == id.ToString() select p).Count();
                    if (dedCnt > 0)
                    {
                        eDetail = (from p in dbHrPayroll.MstGLDLoansDetails where p.GLDId.ToString() == GLDID.Value.ToString() && p.LoanId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            eDetail = new MstGLDLoansDetails();
                            eDetail.LoanId = Convert.ToInt16(id);
                            gl.MstGLDLoansDetails.Add(eDetail);
                            eDetail.CreateDate = DateTime.Now;
                            eDetail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    eDetail.UpdatedBy = oCompany.UserName;
                    eDetail.UpdateDate = DateTime.Now;
                    eDetail.CostAccount = dtLoan.GetValue("CostAcct", i);
                    eDetail.BalancingAccount = dtLoan.GetValue("BalAcct", i);
                    eDetail.CostAcctDisplay = dtLoan.GetValue("CostDesc", i);
                    eDetail.BalancingAcctDisplay = dtLoan.GetValue("BalDesc", i);
                    eDetail.A1Indicator = dtLoan.GetValue("indc", i);
                }

                mtAdv.FlushToDataSource();
                for (int i = 0; i < dtAdv.Rows.Count; i++)
                {
                    string id = dtAdv.GetValue("id", i);
                    MstGLDAdvanceDetail eDetail;
                    int dedCnt = (from p in dbHrPayroll.MstGLDAdvanceDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.AdvancesId.ToString() == id.ToString() select p).Count();
                    if (dedCnt > 0)
                    {
                        eDetail = (from p in dbHrPayroll.MstGLDAdvanceDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.AdvancesId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            eDetail = new MstGLDAdvanceDetail();
                            eDetail.AdvancesId = Convert.ToInt16(id);
                            gl.MstGLDAdvanceDetail.Add(eDetail);
                            eDetail.CreateDate = DateTime.Now;
                            eDetail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    eDetail.UpdatedBy = oCompany.UserName;
                    eDetail.UpdateDate = DateTime.Now;
                    eDetail.CostAccount = dtAdv.GetValue("CostAcct", i);
                    eDetail.BalancingAccount = dtAdv.GetValue("BalAcct", i);
                    eDetail.CostAcctDisplay = dtAdv.GetValue("CostDesc", i);
                    eDetail.BalancingAcctDisplay = dtAdv.GetValue("BalDesc", i);
                    eDetail.A1Indicator = dtAdv.GetValue("indc", i);
                }

                mtOt.FlushToDataSource();
                for (int i = 0; i < dtOt.Rows.Count; i++)
                {
                    string id = dtOt.GetValue("id", i);
                    MstGLDOverTimeDetail eDetail;
                    int dedCnt = (from p in dbHrPayroll.MstGLDOverTimeDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.OvertimeId.ToString() == id.ToString() select p).Count();
                    if (dedCnt > 0)
                    {
                        eDetail = (from p in dbHrPayroll.MstGLDOverTimeDetail where p.GLDId.ToString() == GLDID.Value.ToString() && p.OvertimeId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            eDetail = new MstGLDOverTimeDetail();

                            eDetail.OvertimeId = Convert.ToInt16(id);
                            gl.MstGLDOverTimeDetail.Add(eDetail);
                            eDetail.CreateDate = DateTime.Now;
                            eDetail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    eDetail.UpdatedBy = oCompany.UserName;
                    eDetail.UpdateDate = DateTime.Now;

                    eDetail.CostAccount = dtOt.GetValue("CostAcct", i);
                    eDetail.BalancingAccount = dtOt.GetValue("BalAcct", i);
                    eDetail.CostAcctDisplay = dtOt.GetValue("CostDesc", i);
                    eDetail.BalancingAcctDisplay = dtOt.GetValue("BalDesc", i);

                }

                mtLD.FlushToDataSource();
                for (int i = 0; i < dtLD.Rows.Count; i++)
                {
                    string id = dtLD.GetValue("id", i);
                    MstGLDLeaveDedDetails eDetail;
                    int dedCnt = (from p in dbHrPayroll.MstGLDLeaveDedDetails where p.GLDId.ToString() == GLDID.Value.ToString() && p.LeaveDedId.ToString() == id.ToString() select p).Count();
                    if (dedCnt > 0)
                    {
                        eDetail = (from p in dbHrPayroll.MstGLDLeaveDedDetails where p.GLDId.ToString() == GLDID.Value.ToString() && p.LeaveDedId.ToString() == id.ToString() select p).FirstOrDefault();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            eDetail = new MstGLDLeaveDedDetails();
                            eDetail.LeaveDedId = Convert.ToInt16(id);
                            gl.MstGLDLeaveDedDetails.Add(eDetail);
                            eDetail.CreateDate = DateTime.Now;
                            eDetail.UserId = oCompany.UserName;
                        }
                        else
                        {
                            break;
                        }
                    }
                    eDetail.UpdatedBy = oCompany.UserName;
                    eDetail.UpdateDate = DateTime.Now;

                    eDetail.CostAccount = dtLD.GetValue("CostAcct", i);
                    eDetail.BalancingAccount = dtLD.GetValue("BalAcct", i);
                    eDetail.CostAcctDisplay = dtLD.GetValue("CostDesc", i);
                    eDetail.BalancingAcctDisplay = dtLD.GetValue("BalDesc", i);

                }

                dbHrPayroll.SubmitChanges();
                //SelId = Convert.ToInt32(gl.GLValue != null ? gl.GLValue : 0);
                GLDID.Value = Convert.ToString(gl.Id != null ? gl.Id : 0);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("updateDbWithMat Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void CopyGL(string GlID)
        {
            if (txLocation.Value.ToString().Trim() == "") return;
            oForm.Freeze(true);
            try
            {
                MstGLDetermination gl;

                inicontrolls();

                int cnt = (from p in dbHrPayroll.MstGLDetermination 
                           where p.GLType == cbGltype.Value.Trim() 
                           && p.GLValue.ToString() == GlID.ToString() 
                           select p).Count();

                if (cnt == 0)
                {
                    inicontrolls();
                }
                if (cnt == 0)
                {
                    //gl = new MstGLDetermination();
                    //gl.GLType = "DEPT";
                    //gl.GLValue = dept.ID;
                    //SelId = dept.ID;
                    //dbHrPayroll.MstGLDetermination.InsertOnSubmit(gl);
                    //dbHrPayroll.SubmitChanges();
                }
                else
                {

                    gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == GlID.ToString() select p).Single();
                    txArrE.Value = gl.ArrearsExpense == null ? "" : gl.ArrearsExpense;
                    txArrP.Value = gl.ArrearsPayable == null ? "" : gl.ArrearsPayable;
                    txBasicE.Value = gl.BasicSalary == null ? "" : gl.BasicSalary;
                    txBasicP.Value = gl.BSPayable == null ? "" : gl.BSPayable;
                    txDff.Value = gl.DiffDRCR == null ? "" : gl.DiffDRCR;
                    txEOSE.Value = gl.EOSExpese == null ? "" : gl.EOSExpese;
                    txEOSP.Value = gl.EOSPayable == null ? "" : gl.EOSPayable;
                    txGrtA.Value = gl.GratuityExpense == null ? "" : gl.GratuityExpense;
                    txGrtP.Value = gl.GratuityPayable == null ? "" : gl.GratuityPayable;
                    txITE.Value = gl.IncomeTaxExpense == null ? "" : gl.IncomeTaxExpense;
                    txITP.Value = gl.IncomeTaxPayable == null ? "" : gl.IncomeTaxPayable;
                    txLevE.Value = gl.LeaveEncashmentExpense == null ? "" : gl.LeaveEncashmentExpense;
                    txLvP.Value = gl.LeaveEncashmentPayable == null ? "" : gl.LeaveEncashmentPayable;
                    //dtEarnings.Rows.Clear();
                    //SelId = Convert.ToInt32(gl.GLValue != null ? gl.GLValue : 0);
                    //GLDID.Value = ;

                    GLDID.Value = Convert.ToString((from a in dbHrPayroll.MstGLDetermination where a.GLValue == SelId select a.Id).FirstOrDefault());

                    int i = 0;
                    foreach (MstGLDEarningDetail edEtail in gl.MstGLDEarningDetail)
                    {
                        // dtEarnings.SetValue("id", i, edEtail.Id.ToString());
                        dtEarnings.SetValue("CostAcct", i, edEtail.CostAccout == null ? "" : edEtail.CostAccout);
                        dtEarnings.SetValue("BalAcct", i, edEtail.BalancingAccount == null ? "" : edEtail.BalancingAccount);
                        dtEarnings.SetValue("CostDesc", i, edEtail.CostAcctDisplay == null ? "" : edEtail.CostAcctDisplay);
                        dtEarnings.SetValue("BalDesc", i, edEtail.BalancingAcctDisplay == null ? "" : edEtail.BalancingAcctDisplay);
                        i++;
                    }
                    mtEarnings.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDDeductionDetail eDetail in gl.MstGLDDeductionDetail)
                    {
                        dtDeduct.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount);
                        dtDeduct.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount);
                        dtDeduct.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay);
                        dtDeduct.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay);
                        i++;
                    }
                    mtDed.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDContribution eDetail in gl.MstGLDContribution)
                    {
                        dtCont.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount);
                        dtCont.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount);
                        dtCont.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay);
                        dtCont.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay);
                        dtCont.SetValue("eCostAcct", i, eDetail.EmprCostAccount == null ? "" : eDetail.EmprCostAccount);
                        dtCont.SetValue("eBalAcct", i, eDetail.EmprBalancingAccount == null ? "" : eDetail.EmprBalancingAccount);
                        dtCont.SetValue("eCostDesc", i, eDetail.EmprCostAcctDisplay == null ? "" : eDetail.EmprCostAcctDisplay);
                        dtCont.SetValue("eBalDesc", i, eDetail.EmprBalancingAcctDisplay == null ? "" : eDetail.EmprBalancingAcctDisplay);

                        i++;
                    }
                    mtCon.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDLoansDetails eDetail in gl.MstGLDLoansDetails)
                    {
                        dtLoan.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount);
                        dtLoan.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount);
                        dtLoan.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay);
                        dtLoan.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay);
                        i++;
                    }
                    mtLoan.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDAdvanceDetail eDetail in gl.MstGLDAdvanceDetail)
                    {
                        dtAdv.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount);
                        dtAdv.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount);
                        dtAdv.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay);
                        dtAdv.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay);

                        i++;
                    }
                    mtAdv.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDOverTimeDetail eDetail in gl.MstGLDOverTimeDetail)
                    {
                        dtOt.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount);
                        dtOt.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount);
                        dtOt.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay);
                        dtOt.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay);


                        i++;
                    }
                    mtOt.LoadFromDataSource();
                    i = 0;

                    foreach (MstGLDLeaveDedDetails eDetail in gl.MstGLDLeaveDedDetails)
                    {
                        dtLD.SetValue("CostAcct", i, eDetail.CostAccount == null ? "" : eDetail.CostAccount);
                        dtLD.SetValue("BalAcct", i, eDetail.BalancingAccount == null ? "" : eDetail.BalancingAccount);
                        dtLD.SetValue("CostDesc", i, eDetail.CostAcctDisplay == null ? "" : eDetail.CostAcctDisplay);
                        dtLD.SetValue("BalDesc", i, eDetail.BalancingAcctDisplay == null ? "" : eDetail.BalancingAcctDisplay);

                        i++;
                    }
                    mtLD.LoadFromDataSource();
                    i = 0;


                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
                //int confirm = oApplication.MessageBox("Error in loading the GL Determination Detail. \n\nIt may be because of GL having old structure. Do you want to delete this GL Determination \n\nCLose the form after deletion!", 3, "Yes", "No", "Cancel");
                //if (confirm == 2 || confirm == 3)
                //{
                //}
                //else
                //{

                //    int cnt = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == SelId.ToString() select p).Count();
                //    if (cnt > 0)
                //    {
                //        MstGLDetermination gl = (from p in dbHrPayroll.MstGLDetermination where p.GLType == cbGltype.Value.Trim() && p.GLValue.ToString() == SelId.ToString() select p).Single();
                //        dbHrPayroll.MstGLDetermination.DeleteOnSubmit(gl);
                //        dbHrPayroll.SubmitChanges();
                //        SelId = 0;
                //    }
                //}

            }
            oForm.Freeze(false);
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        #endregion

    }
}
