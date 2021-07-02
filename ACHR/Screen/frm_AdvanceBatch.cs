using System;
using System.Data;
using System.IO;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbouiCOM;

namespace ACHR.Screen
{
    partial class frm_AdvanceBatch : HRMSBaseForm
    {
        #region Local Variable Area

        public IEnumerable<TrnsAdvancePaymentBatch> AdvancePaymentBatch;
        public int advanceId = 0;
        SAPbouiCOM.DataTable dtPeriods;
        Boolean flgValidCall = false, flgDocMode = false, flgCalculateAdvance = false;
        decimal BasicTillDay = 0, getAdvanceDeduction = 0, getLoanDeduction = 0, getLeaveDeduction = 0, getOT = 0, getElemnts = 0;
        string selEmpId = "";

        SAPbouiCOM.EditText txDocNum, txDocDate, txApidAccount, txDurationFrom, txDurationTo, txFilenam;
        SAPbouiCOM.ComboBox cbProll, cbPeriod, cbStatus, cbAdvanceType;
        SAPbouiCOM.Matrix mtEmp;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clID, clEmployeeID, clEmployeeName, clBranch, clBasicSalary, clBasicSalaryEarned, clAllowance, clAbsentDeduction;
        SAPbouiCOM.Column clEarltLateMinutes, clOverTimeMinutes, clOTDeduction, clLoanInstallment, clNeAmount, clAdvance, clAdvancePercentage, clMaxAdvanceAllowed, clAdvanceApproved, clActive;
        SAPbouiCOM.Item ItxDocNum, ItxDocDate, ItxApidAccount, ItxDurationFrom, ItxDurationTo, ItxFilenam;
        SAPbouiCOM.Item IcbProll, IcbPeriod, IcbStatus, IcbAdvanceType;
        SAPbouiCOM.Button btSave, btCalculateAdvance;
        SAPbouiCOM.Item ImtEmp, IbtProcess, IbtSave, IbtCalculateAdvance;
        SAPbouiCOM.PictureBox pctBox;

        SAPbouiCOM.DataTable dtEmps;
        System.Data.DataTable DtFile = new System.Data.DataTable();

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            InitiallizegridMatrix();
            fillCbs();
            IniContrls();
            IbtCalculateAdvance.Enabled = false;
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btProcess":
                        if (!ValidateRecord())
                        {
                            BubbleEvent = false;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btnSave":
                    submitForm();
                    break;
                case "btPick":
                    LoadEmployeeToGrid();
                    break;
                case "btncalc":
                    CalculateAdvance();
                    break;
                case "btProcess":
                    var DocumentStatus = (from a in dbHrPayroll.TrnsAdvancePaymentBatch where a.DocumentNo.ToString() == txDocNum.Value.Trim() select a).FirstOrDefault();
                    if (DocumentStatus != null)
                    {
                        if (DocumentStatus.Status == "0" || DocumentStatus.Status == "1")
                        {
                            PostAdvancePayment();
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Selected document already Posted", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                    }

                    break;

            }
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
                if (pVal.ItemUID == "cbProll")
                {
                    FillPeriod(cbProll.Value.Trim());
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);

            try
            {
                if (pVal.ColUID == "clAdApr")
                {
                    decimal intMaximumAdvanceAllowed = 0.0M, intAdvanceApproved = 0.0M, intAdvancePaid = 0.0M, intNetSalartTillDate = 0.0M;
                    string strNetSalartTillDate = (mtEmp.Columns.Item("clNamt").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    string strMaximumAdvanceAllowed = (mtEmp.Columns.Item("clAdAlw").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    string strAdvanceApproved = (mtEmp.Columns.Item("clAdApr").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    intNetSalartTillDate = Convert.ToDecimal(strNetSalartTillDate);
                    intMaximumAdvanceAllowed = Convert.ToDecimal(strMaximumAdvanceAllowed);
                    intAdvanceApproved = Convert.ToDecimal(strAdvanceApproved);

                    intAdvancePaid = (intNetSalartTillDate / 100) * intMaximumAdvanceAllowed;
                    if (intAdvanceApproved > intMaximumAdvanceAllowed)
                    {
                        oApplication.StatusBar.SetText("Maximum advance allowed is greater than advance to be given.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtEmp.Columns.Item("clAdApr").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = string.Format("{0:0.000}", "0.00");
                        return;
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void fillFields()
        {
            base.fillFields();
            _fillFields();
        }

        #endregion

        #region Local Functionsm

        private void InitiallizeForm()
        {
            try
            {
                oForm.Freeze(true);

                oForm.DataSources.UserDataSources.Add("txDocNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txDocNum = oForm.Items.Item("txDocNum").Specific;
                ItxDocNum = oForm.Items.Item("txDocNum");
                txDocNum.DataBind.SetBound(true, "", "txDocNum");

                oForm.DataSources.UserDataSources.Add("txDocdate", SAPbouiCOM.BoDataType.dt_DATE);
                txDocDate = oForm.Items.Item("txDocdate").Specific;
                ItxDocDate = oForm.Items.Item("txDocdate");
                txDocDate.DataBind.SetBound(true, "", "txDocdate");


                oForm.DataSources.UserDataSources.Add("txPaidAcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
                txApidAccount = oForm.Items.Item("txPaidAcc").Specific;
                ItxApidAccount = oForm.Items.Item("txPaidAcc");
                txApidAccount.DataBind.SetBound(true, "", "txPaidAcc");
                ItxApidAccount.Enabled = false;

                oForm.DataSources.UserDataSources.Add("txDTTo", SAPbouiCOM.BoDataType.dt_DATE);
                txDurationTo = oForm.Items.Item("txDTTo").Specific;
                ItxDurationTo = oForm.Items.Item("txDTTo");
                txDurationTo.DataBind.SetBound(true, "", "txDTTo");

                oForm.DataSources.UserDataSources.Add("txFilenam", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 150);
                txFilenam = oForm.Items.Item("txFilenam").Specific;
                ItxFilenam = oForm.Items.Item("txFilenam");
                txFilenam.DataBind.SetBound(true, "", "txFilenam");

                oForm.Items.Item("btProcess").Enabled = false;
                IbtProcess = oForm.Items.Item("btProcess");

                oForm.Items.Item("btnSave").Enabled = true;
                btSave = oForm.Items.Item("btnSave").Specific;
                IbtSave = oForm.Items.Item("btnSave");

                oForm.Items.Item("btncalc").Enabled = true;
                btCalculateAdvance = oForm.Items.Item("btncalc").Specific;
                IbtCalculateAdvance = oForm.Items.Item("btncalc");

                #region Matrix
                //mtEmp = oForm.Items.Item("mtEmp").Specific;
                //dtEmps = oForm.DataSources.DataTables.Item("dtEmps");
                //clID = mtEmp.Columns.Item("id");
                //clID.Visible = false;
                //clEmployeeID = mtEmp.Columns.Item("clEId");
                //clEmployeeName = mtEmp.Columns.Item("clEName");
                //clBranch = mtEmp.Columns.Item("clBrnch");
                //clBasicSalary = mtEmp.Columns.Item("clBSal");
                //clBasicSalaryEarned = mtEmp.Columns.Item("clBSEar");
                //clAllowance = mtEmp.Columns.Item("clallw");
                ////clAllowance = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "allw");

                //clAbsentDeduction = mtEmp.Columns.Item("clabsded");
                ////clAbsentDeduction = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "absded");

                //clEarltLateMinutes = mtEmp.Columns.Item("clELT");
                ////clEarltLateMinutes = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "EarlyLT");

                //clOverTimeMinutes = mtEmp.Columns.Item("clOT");
                ////clOverTimeMinutes = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "OT");

                //clOTDeduction = mtEmp.Columns.Item("clOTD");
                ////clOTDeduction = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "OTded");

                //clLoanInstallment = mtEmp.Columns.Item("clLoan");
                ////clLoanInstallment = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "Loan");

                //clNeAmount = mtEmp.Columns.Item("clNamt");
                ////clNeAmount = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "Netamt");

                //clAdvance = mtEmp.Columns.Item("clAdv");
                ////clAdvance = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "Adv");

                //clMaxAdvanceAllowed = mtEmp.Columns.Item("clAdAlw");
                ////clMaxAdvanceAllowed = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "AdvAllw");

                //clAdvanceApproved = mtEmp.Columns.Item("clAdApr");
                ////clAdvanceApproved = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "AdvAppr");

                //clActive = mtEmp.Columns.Item("vlActive");
                ////clActive = oColumn;
                ////oColumn.DataBind.Bind("mtEmp", "flgActive");

                #endregion

                #region CBS
                cbProll = oForm.Items.Item("cbProll").Specific;
                oForm.DataSources.UserDataSources.Add("cbProll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbProll.DataBind.SetBound(true, "", "cbProll");

                cbPeriod = oForm.Items.Item("cbPeriod").Specific;
                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbPeriod.DataBind.SetBound(true, "", "cbPeriod");

                cbStatus = oForm.Items.Item("cbStatus").Specific;
                oForm.DataSources.UserDataSources.Add("cbStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbStatus.DataBind.SetBound(true, "", "cbStatus");

                cbAdvanceType = oForm.Items.Item("cbadva").Specific;
                oForm.DataSources.UserDataSources.Add("cbadva", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbAdvanceType.DataBind.SetBound(true, "", "cbadva");
                #endregion

                oForm.Freeze(false);
            }
            catch (Exception ex)
            {

            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtEmps = oForm.DataSources.DataTables.Add("mtEmp");
                dtEmps.Columns.Add("id", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmps.Columns.Add("EId", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("EName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("Brnch", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmps.Columns.Add("BSal", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("BSEar", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("allw", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("absded", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("ELT", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("OT", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("OTD", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("Adv", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("Loan", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("Namt", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("AdPrc", SAPbouiCOM.BoFieldsType.ft_Percent);
                dtEmps.Columns.Add("AdAlw", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("AdApr", SAPbouiCOM.BoFieldsType.ft_Price);
                dtEmps.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text);


                mtEmp = (SAPbouiCOM.Matrix)oForm.Items.Item("mtEmp").Specific;
                oColumns = (SAPbouiCOM.Columns)mtEmp.Columns;


                oColumn = oColumns.Item("id");
                clID = oColumn;
                oColumn.DataBind.Bind("mtEmp", "id");
                clID.Visible = false;
                oColumn = oColumns.Item("clEId");
                clEmployeeID = oColumn;
                oColumn.DataBind.Bind("mtEmp", "EId");

                oColumn = oColumns.Item("clEName");
                clEmployeeName = oColumn;
                oColumn.DataBind.Bind("mtEmp", "EName");

                oColumn = oColumns.Item("clBrnch");
                clBranch = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Brnch");

                oColumn = oColumns.Item("clBSal");
                clBasicSalary = oColumn;
                oColumn.DataBind.Bind("mtEmp", "BSal");

                oColumn = oColumns.Item("clBSEar");
                clBasicSalaryEarned = oColumn;
                oColumn.DataBind.Bind("mtEmp", "BSEar");

                oColumn = oColumns.Item("clallw");
                clAllowance = oColumn;
                oColumn.DataBind.Bind("mtEmp", "allw");

                oColumn = oColumns.Item("clabsded");
                clAbsentDeduction = oColumn;
                oColumn.DataBind.Bind("mtEmp", "absded");

                oColumn = oColumns.Item("clELT");
                clEarltLateMinutes = oColumn;
                oColumn.DataBind.Bind("mtEmp", "ELT");

                oColumn = oColumns.Item("clOT");
                clOverTimeMinutes = oColumn;
                oColumn.DataBind.Bind("mtEmp", "OT");

                oColumn = oColumns.Item("clOTD");
                clOTDeduction = oColumn;
                oColumn.DataBind.Bind("mtEmp", "OTD");

                oColumn = oColumns.Item("clLoan");
                clLoanInstallment = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Loan");

                oColumn = oColumns.Item("clNamt");
                clNeAmount = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Namt");

                oColumn = oColumns.Item("clAdv");
                clAdvance = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Adv");

                oColumn = oColumns.Item("clAdPrc");
                clAdvancePercentage = oColumn;
                oColumn.DataBind.Bind("mtEmp", "AdPrc");

                oColumn = oColumns.Item("clAdAlw");
                clMaxAdvanceAllowed = oColumn;
                oColumn.DataBind.Bind("mtEmp", "AdAlw");

                oColumn = oColumns.Item("clAdApr");
                clAdvanceApproved = oColumn;
                oColumn.DataBind.Bind("mtEmp", "AdApr");

                oColumn = oColumns.Item("clActive");
                clActive = oColumn;
                oColumn.DataBind.Bind("mtEmp", "Active");



            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void fillCbs()
        {
            try
            {
                int i = 0;
                string selId = "0";

                #region Fill Payroll
                string strOut = string.Empty;
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'");
                strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    if (strOut != null && strOut != "")
                    {
                        //IEnumerable<CfgPayrollDefination> prs = 
                        //    from p in dbHrPayroll.CfgPayrollDefination 
                        //    where p.ID.ToString() == strOut.Trim() select p;

                        string strSql = sqlString.getSql("GetPayrollName", SearchKeyVal);
                        strSql = strSql + " where ID in (" + strOut + ")";
                        strSql += " ORDER BY ID Asc ";
                        System.Data.DataTable dt = ds.getDataTable(strSql);
                        DataView dv = dt.DefaultView;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int l = 0; l < dt.Rows.Count; l++)
                            {
                                string strPayrollName = dt.Rows[l]["PayrollName"].ToString();
                                Int32 intPayrollID = Convert.ToInt32(dt.Rows[l]["ID"].ToString());
                                cbProll.ValidValues.Add(intPayrollID.ToString(), strPayrollName);

                            }
                        }
                        //foreach (CfgPayrollDefination pr in prs)
                        //{
                        //    cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        //    i++;
                        //}

                        cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbProll.Value);
                    }
                    else
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbProll.Value);
                    }

                }
                else
                {
                    IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                    foreach (CfgPayrollDefination pr in prs)
                    {
                        cbProll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        i++;
                    }

                    cbProll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    FillPeriod(cbProll.Value);
                }
                //End Fill Payroll
                #endregion

                FillPeriod(cbProll.Value);
                fillCombo("btchStatus", cbStatus);
                FillAdvanceTypeCombo();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function fillCbs.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillAdvanceTypeCombo()
        {
            try
            {
                cbAdvanceType.ValidValues.Add("-1", "[Select One]");
                var Data = from v in dbHrPayroll.MstAdvance where v.FlgActive == true select v;
                foreach (var v in Data)
                {
                    cbAdvanceType.ValidValues.Add(v.Id.ToString(), v.Description);
                }
                cbAdvanceType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                // dtPeriods.Rows.Clear();
                if (cbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPeriod.ValidValues.Remove(cbPeriod.ValidValues.Item(k).Value);
                    }
                }
                int i = 0;
                string selId = "0";
                bool flgPrevios = false;
                bool flgHit = false;
                int count = 0;
                int cnt = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Count();
                if (cnt > 0)
                {
                    CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).Single();
                    foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    {
                        //if (Convert.ToBoolean(pd.FlgVisible))
                        //{
                        cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        //}
                        count++;

                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();

                        if (Convert.ToBoolean(pd.FlgLocked))
                        {
                            selId = "0";
                            flgPrevios = true;
                        }
                        else
                        {
                            if (flgPrevios)
                            {
                                selId = pd.ID.ToString();
                                flgPrevios = false;
                            }
                        }

                        i++;
                    }
                    try
                    {
                        cbPeriod.Select(selId);
                        //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void IniContrls()
        {

            try
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oForm.Update();
                oForm.Refresh();
                getData();
                //GetDocumentNo();
                long nextId = ds.getNextId("TrnsAdvancePaymentBatch", "ID");
                txDocNum.Value = nextId.ToString();
                cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                dtEmps.Rows.Clear();
                AddEmptyRow();
                btSave.Caption = "Save";

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function IniContrls.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void _fillFields()
        {
            oForm.Freeze(true);
            string strProcessing = "";
            btSave.Caption = "Update";
            try
            {
                if (cbStatus.Value.Trim() == "0")
                {
                    IbtCalculateAdvance.Enabled = true;
                }
                else
                {
                    IbtCalculateAdvance.Enabled = false;
                }
                if (currentRecord >= 0)
                {
                    TrnsAdvancePaymentBatch record = AdvancePaymentBatch.ElementAt<TrnsAdvancePaymentBatch>(currentRecord);

                    txDocNum.Value = record.DocumentNo.ToString();
                    txDocDate.Value = Convert.ToDateTime(record.DocumentDate).ToString("yyyyMMdd");
                    txDurationTo.Value = Convert.ToDateTime(record.DurationTo).ToString("yyyyMMdd");

                    cbProll.Select(record.PayrollID.ToString());
                    cbPeriod.Select(record.PeriodID.ToString());
                    cbAdvanceType.Select(record.AdvanceType.ToString());
                    cbStatus.Select(record.Status.ToString());
                    if (record.Status.ToString().Trim() == "0")
                    {
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        btSave.Caption = "Update";
                        IbtProcess.Enabled = true;
                    }
                    else
                    {
                        IbtProcess.Enabled = false;

                    }

                    dtEmps.Rows.Clear();
                    int rowNum = 0;
                    foreach (TrnsAdvancePaymentBatchDetail btd in record.TrnsAdvancePaymentBatchDetail)
                    {
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.ID == btd.EmployeeID select p).FirstOrDefault();
                        //strProcessing = "Error in Setting Employee Record with Employee ID --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + "  ";

                        dtEmps.Rows.Add(1);
                        dtEmps.SetValue("id", rowNum, btd.ID.ToString());
                        dtEmps.SetValue("EId", rowNum, emp.EmpID);
                        dtEmps.SetValue("EName", rowNum, btd.EmployeeName);
                        dtEmps.SetValue("Brnch", rowNum, btd.BranchID.ToString());
                        dtEmps.SetValue("BSal", rowNum, btd.BasicSalary.ToString());
                        dtEmps.SetValue("BSEar", rowNum, btd.BasicSalaryEarned.ToString());
                        dtEmps.SetValue("allw", rowNum, btd.Allowance.ToString());
                        dtEmps.SetValue("ELT", rowNum, btd.EarlyLateInMinutes.ToString());
                        dtEmps.SetValue("OT", rowNum, btd.OverTimeInMinutes.ToString());
                        dtEmps.SetValue("OTD", rowNum, btd.OTDeduction.ToString());
                        dtEmps.SetValue("Adv", rowNum, btd.AdvanceDeduction.ToString());
                        dtEmps.SetValue("Loan", rowNum, btd.LoanInstallment.ToString());
                        dtEmps.SetValue("Namt", rowNum, btd.NetAmount.ToString());
                        dtEmps.SetValue("AdPrc", rowNum, btd.AdvancePercentage.ToString());
                        dtEmps.SetValue("AdAlw", rowNum, btd.MaxAdvanceAllowed.ToString());
                        dtEmps.SetValue("AdApr", rowNum, btd.AdvanceApproved.ToString());
                        dtEmps.SetValue("Active", rowNum, btd.FlgActive == true ? "Y" : "N");
                        rowNum++;

                    }

                    AddEmptyRow();
                    mtEmp.LoadFromDataSource();
                    oForm.Items.Item("btProcess").Enabled = true;
                }

                oForm.Freeze(false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(strProcessing + "Error in loading Record!" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                oForm.Freeze(false);

            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtEmps.Rows.Count == 0)
            {
                dtEmps.Rows.Add(1);
                RowValue = dtEmps.Rows.Count;
                dtEmps.SetValue(clEmployeeID.DataBind.Alias, RowValue - 1, "");
                dtEmps.SetValue(clEmployeeName.DataBind.Alias, RowValue - 1, "");
                dtEmps.SetValue(clBranch.DataBind.Alias, RowValue - 1, "");
                dtEmps.SetValue(clBasicSalary.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clBasicSalaryEarned.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clAllowance.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clAbsentDeduction.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clEarltLateMinutes.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clOverTimeMinutes.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clOTDeduction.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clAdvance.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clLoanInstallment.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clNeAmount.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clMaxAdvanceAllowed.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clAdvanceApproved.DataBind.Alias, RowValue - 1, "0");
                dtEmps.SetValue(clActive.DataBind.Alias, RowValue - 1, "N");
                //grdMain.AddRow(1, RowValue + 1);
                mtEmp.AddRow(1, 0);
            }
            else
            {
                if (dtEmps.GetValue(clEmployeeName.DataBind.Alias, dtEmps.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtEmps.Rows.Add(1);
                    RowValue = dtEmps.Rows.Count;
                    dtEmps.SetValue(clEmployeeID.DataBind.Alias, RowValue - 1, "");
                    dtEmps.SetValue(clEmployeeName.DataBind.Alias, RowValue - 1, "");
                    dtEmps.SetValue(clBranch.DataBind.Alias, RowValue - 1, "");
                    dtEmps.SetValue(clBasicSalary.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clBasicSalaryEarned.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clAllowance.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clAbsentDeduction.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clEarltLateMinutes.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clOverTimeMinutes.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clOTDeduction.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clAdvance.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clLoanInstallment.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clNeAmount.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clMaxAdvanceAllowed.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clAdvanceApproved.DataBind.Alias, RowValue - 1, "0");
                    dtEmps.SetValue(clActive.DataBind.Alias, RowValue - 1, "N");
                    mtEmp.AddRow(1, mtEmp.RowCount + 1);
                }
            }
            mtEmp.LoadFromDataSource();
        }

        private void getData()
        {
            try
            {
                CodeIndex.Clear();
                AdvancePaymentBatch = from p in dbHrPayroll.TrnsAdvancePaymentBatch select p;
                int i = 0;
                foreach (TrnsAdvancePaymentBatch ele in AdvancePaymentBatch)
                {
                    CodeIndex.Add(ele.ID.ToString(), i);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function getData.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadEmployeeToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[2];
            Int16 counter = 0;
            Int16 LineNumber = 1;
            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("No");
                DtFile.Columns.Add("EmpID");
                string PayrollName = cbProll.Selected.Value;
                var oPayroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID == Convert.ToInt32(PayrollName) select p).FirstOrDefault();
                FilePath = Program.objHrmsUI.FindFile();
                if (String.IsNullOrEmpty(FilePath))
                {
                    oApplication.SetStatusBarMessage("Select a template file");
                    return;
                }
                txFilenam.Value = Convert.ToString(FilePath);
                if (!string.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();

                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {

                    mtEmp.Clear();
                    dtEmps.Rows.Clear();
                    foreach (DataRow dr1 in DtFile.Rows)
                    {
                        var oEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == dr1["EmpID"].ToString() && a.PayrollID == oPayroll.ID select a).FirstOrDefault();
                        if (oEmpID != null)
                        {
                            dtEmps.Rows.Add();
                            dtEmps.SetValue("id", LineNumber - 1, LineNumber);
                            dtEmps.SetValue("EId", LineNumber - 1, dr1["EmpID"]);

                            #region Variables
                            BasicTillDay = 0.0M;
                            getElemnts = 0.0M;
                            getAdvanceDeduction = 0.0M;
                            getLoanDeduction = 0.0M;
                            getLeaveDeduction = 0.0M;
                            getOT = 0.0M;
                            #endregion

                            string stremp = oEmpID.EmpID;
                            dtEmps.SetValue("EName", LineNumber - 1, oEmpID.FirstName + ' ' + oEmpID.MiddleName + ' ' + oEmpID.LastName);
                            dtEmps.SetValue("Brnch", LineNumber - 1, oEmpID.BranchName);
                            dtEmps.SetValue("BSal", LineNumber - 1, string.Format("{0:0.00}", ds.getEmpGross(oEmpID)));
                            LineNumber++;
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Employee ID: '" + dr1["EmpID"] + "' not found in payroll: '" + oPayroll.PayrollName + "' ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }


                    }
                    mtEmp.LoadFromDataSource();
                    IbtCalculateAdvance.Enabled = true;

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoanAdvanceToGrid : " + Ex.Message + counter + " : " + LineNumber.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void CalculateAdvance()
        {
            string spIds = "0";
            string code = "";
            List<string> oSelectedEmployee = new List<string>();
            mtEmp.FlushToDataSource();
            try
            {
                if (cbStatus.Value == "Processed")
                {
                    oApplication.StatusBar.SetText("Advance already calculated and Processed", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {
                    #region Variables
                    BasicTillDay = 0.0M;
                    decimal leaveCnt = 0.00M;
                    decimal payDays = 0.00M;
                    decimal leaveDays = 0.00M;
                    decimal monthDays = 0.00M;

                    getElemnts = 0.0M;
                    getAdvanceDeduction = 0.0M;
                    getLoanDeduction = 0.0M;
                    getLeaveDeduction = 0.0M;
                    decimal payRatio = 1.00M;
                    getOT = 0.0M;
                    decimal TotalSalary = 0.0M;
                    decimal decNetAmount = 0.0M;
                    decimal decMaxAdvanceAllowed = 0.0M;
                    decimal decAdvanceApproved = 0.0M;

                    #endregion
                    code = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    code = code.Trim();
                    if (code != "")
                    {
                        var oEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == code.ToString() select a).FirstOrDefault();
                        if (oEmpID != null)
                        {
                            #region Check GL Determination
                            MstGLDetermination glDetr = null;
                            try
                            {
                                glDetr = ds.getEmpGl(oEmpID);
                                if (glDetr == null)
                                {
                                    oApplication.StatusBar.SetText("EmpCode : " + oEmpID + " Doesn't have GL determination defined in respected Location or Deparment.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                            }
                            catch
                            {
                                MsgWarning("GL Determination for employee not found.");
                                return;
                            }
                            #endregion

                            #region Calculate
                            DateTime DurationToDate = DateTime.MinValue;
                            if (!string.IsNullOrEmpty(txDurationTo.Value))
                            {
                                DurationToDate = DateTime.ParseExact(txDurationTo.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Please set Duration to Date.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                                return;
                            }
                            try
                            {
                                var GetPeriodFromRequestDate = (from a in dbHrPayroll.CfgPeriodDates
                                                                where a.StartDate <= DurationToDate
                                                                && a.EndDate >= DurationToDate
                                                                && a.PayrollId == oEmpID.PayrollID
                                                                select a).FirstOrDefault();
                                if (GetPeriodFromRequestDate != null)
                                {
                                    DateTime startDate = GetPeriodFromRequestDate.StartDate.Value;
                                    if (oEmpID.JoiningDate > GetPeriodFromRequestDate.StartDate.Value)
                                    {
                                        startDate = Convert.ToDateTime(oEmpID.JoiningDate);
                                    }
                                    DateTime EndDate = GetPeriodFromRequestDate.EndDate.Value;
                                    DateTime TodayDate = DurationToDate;
                                    int CountTotalPeriodDays = (EndDate - startDate).Days + 1;
                                    int CountTillDate = (TodayDate - startDate).Days + 1;
                                    if (oEmpID.JoiningDate > GetPeriodFromRequestDate.StartDate.Value)
                                    {
                                        payRatio = CountTillDate / CountTotalPeriodDays;
                                    }
                                    else
                                    {
                                        payRatio = 1;
                                    }
                                    decimal getBasic = oEmpID.BasicSalary.Value;
                                    //Calculate Basic Salary
                                    if (getBasic > 0)
                                    {
                                        BasicTillDay = (getBasic / CountTotalPeriodDays) * CountTillDate;
                                    }
                                    //////Absents ////
                                    System.Data.DataTable dtAbsentDeduction = ds.DynamicLeavesProcessing(oEmpID, GetPeriodFromRequestDate, (decimal)ds.getEmpGross(oEmpID), out leaveCnt, DurationToDate);
                                    foreach (DataRow dr in dtAbsentDeduction.Rows)
                                    {
                                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                        getLeaveDeduction = getLeaveDeduction + RecordAmount;
                                    }
                                    //* Payroll elements assigned to employee ***Employee Elements ****** 
                                    System.Data.DataTable dtSalPrlElements = ds.salaryProcessingElements(oEmpID, GetPeriodFromRequestDate, CountTillDate, (decimal)ds.getEmpGross(oEmpID), glDetr, payRatio, 0, 0, 0);
                                    foreach (DataRow dr in dtSalPrlElements.Rows)
                                    {
                                        if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                        {
                                            decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                            getElemnts = getElemnts + RecordAmount;
                                        }
                                    }
                                    //////Over time ////
                                    Int32 otminute = 0;
                                    //System.Data.DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(oEmp, GetPeriodFromRequestDate, (decimal)ds.getEmpGross(oEmp), out otminute);
                                    System.Data.DataTable dtSalOverTimes = ds.DynamicOTProcessing(oEmpID, GetPeriodFromRequestDate, (decimal)ds.getEmpGross(oEmpID), out otminute, DurationToDate);
                                    foreach (DataRow dr in dtSalOverTimes.Rows)
                                    {
                                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                        getOT = getOT + RecordAmount;
                                    }
                                    // * ************Advance Recovery Processing **************
                                    System.Data.DataTable dtAdvance = ds.salaryProcessingAdvance(oEmpID, (decimal)ds.getEmpGross(oEmpID), GetPeriodFromRequestDate);
                                    foreach (DataRow dr in dtAdvance.Rows)
                                    {
                                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                        getAdvanceDeduction = getAdvanceDeduction + RecordAmount;
                                    }
                                    // txtPreAd.Value = string.Format("{0:0.00}", getAdvanceDeduction);
                                    string PreviousAdvance = string.Format("{0:0.00}", getAdvanceDeduction);
                                    // * ************Loan Recovery Processing **************
                                    System.Data.DataTable dtLoands = ds.salaryProcessingLoans(oEmpID, (decimal)ds.getEmpGross(oEmpID), GetPeriodFromRequestDate);
                                    foreach (DataRow dr in dtLoands.Rows)
                                    {
                                        decimal RecordAmount = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                        getLoanDeduction = getLoanDeduction + RecordAmount;

                                    }
                                    var oEarlyInAndOTMinutes = dbHrPayroll.GetEaryLateInAndOT(startDate, DurationToDate, oEmpID.EmpID).FirstOrDefault();
                                    if (oEarlyInAndOTMinutes != null)
                                    {
                                        (mtEmp.Columns.Item("clELT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = oEarlyInAndOTMinutes.EarlyIn_Miutes.GetValueOrDefault().ToString();
                                        (mtEmp.Columns.Item("clOT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = oEarlyInAndOTMinutes.OverTime_Miutes.GetValueOrDefault().ToString();
                                        //dtEmps.SetValue("ELT", LineNumber - 1, string.Format("{0:0.00}", oEarlyInAndOTMinutes.EarlyIn_Miutes.GetValueOrDefault().ToString()));
                                        //dtEmps.SetValue("OT", LineNumber - 1, string.Format("{0:0.00}", oEarlyInAndOTMinutes.OverTime_Miutes.GetValueOrDefault().ToString()));
                                    }
                                    else
                                    {
                                        //oApplication.StatusBar.SetText("EmpCode : " + oEmpID.EmpID + "Over Time Nhe Mila.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        (mtEmp.Columns.Item("clELT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = "0.00";
                                        (mtEmp.Columns.Item("clOT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = "0.00";
                                        //dtEmps.SetValue("ELT", LineNumber - 1, string.Format("{0:0.00}", "0.00"));
                                        //dtEmps.SetValue("OT", LineNumber - 1, string.Format("{0:0.00}", "0.00"));
                                    }
                                    #region Set Value on Grid
                                    (mtEmp.Columns.Item("clBSEar").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = BasicTillDay.ToString();
                                    //dtEmps.SetValue("BSEar", LineNumber - 1, string.Format("{0:0.00}", BasicTillDay.ToString()));
                                    (mtEmp.Columns.Item("clallw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = getElemnts.ToString();
                                    //dtEmps.SetValue("allw", LineNumber - 1, string.Format("{0:0.00}", getElemnts.ToString()));
                                    (mtEmp.Columns.Item("clabsded").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = getLeaveDeduction.ToString();
                                    //dtEmps.SetValue("absded", LineNumber - 1, string.Format("{0:0.00}", getLeaveDeduction.ToString()));        
                                    (mtEmp.Columns.Item("clOTD").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = getOT.ToString();
                                    //dtEmps.SetValue("OTD", LineNumber - 1, string.Format("{0:0.00}", getOT.ToString()));
                                    (mtEmp.Columns.Item("clAdv").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = getAdvanceDeduction.ToString();
                                    //dtEmps.SetValue("adv", LineNumber - 1, string.Format("{0:0.00}", getAdvanceDeduction.ToString()));
                                    (mtEmp.Columns.Item("clLoan").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = getLoanDeduction.ToString();
                                    //dtEmps.SetValue("Loan", LineNumber - 1, string.Format("{0:0.00}", getLoanDeduction.ToString()));
                                    (mtEmp.Columns.Item("clAdPrc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = oEmpID.AllowedAdvance.GetValueOrDefault().ToString();
                                    //dtEmps.SetValue("AdPrc", LineNumber - 1, string.Format("{0:0.00}", oEmpID.AllowedAdvance.GetValueOrDefault().ToString()));

                                    //decNetAmount = (BasicTillDay + getElemnts + getOT) + (getLeaveDeduction + getAdvanceDeduction + getLoanDeduction);
                                    decNetAmount = (BasicTillDay + getElemnts) + (getLeaveDeduction + getAdvanceDeduction + getLoanDeduction);
                                    (mtEmp.Columns.Item("clNamt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = decNetAmount.ToString();
                                    //dtEmps.SetValue("Namt", LineNumber - 1, string.Format("{0:0.00}", decNetAmount.ToString()));
                                    decMaxAdvanceAllowed = (decNetAmount / 100) * oEmpID.AllowedAdvance.GetValueOrDefault();
                                    (mtEmp.Columns.Item("clAdAlw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value = decMaxAdvanceAllowed.ToString();
                                    //dtEmps.SetValue("AdAlw", LineNumber - 1, string.Format("{0:0.00}", decMaxAdvanceAllowed.ToString()));
                                    //dtEmps.SetValue(clActive.DataBind.Alias, LineNumber - 1, "Y");
                                    Boolean flgActiveAdvance = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked = true;
                                    #endregion
                                }
                            }
                            catch (Exception ex)
                            {
                                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                            #endregion
                        }

                    }
                }
                IbtCalculateAdvance.Enabled = false;
            }
            catch (Exception ex)
            {

            }
            //totalCnt = oSelectedEmployee.Count;
            //prog = oApplication.StatusBar.CreateProgressBar("Processing Salary", totalCnt, false);
            //prog.Value = 0;

        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            IniContrls();
        }

        private void submitForm()
        {
            string strProcessing = "";
            DateTime DocDate = DateTime.MinValue;
            DateTime DurationDate = DateTime.MinValue;
            DocDate = DateTime.ParseExact(txDocDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            DurationDate = DateTime.ParseExact(txDurationTo.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            try
            {
                if (string.IsNullOrEmpty(cbPeriod.Value.Trim()) || cbPeriod.Selected.Value.Trim() == "0")
                {
                    oApplication.StatusBar.SetText("Please select valid Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(cbAdvanceType.Value.Trim()) || cbAdvanceType.Value.Trim() == "-1")
                {
                    oApplication.StatusBar.SetText("Please select Advance Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                mtEmp.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                CfgPayrollDefination oPayroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cbProll.Value.ToString() select p).FirstOrDefault();
                CfgPeriodDates oPayrollPeriod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).FirstOrDefault();
                TrnsAdvancePaymentBatch AdvancePayment;
                int cnt = (from p in dbHrPayroll.TrnsAdvancePaymentBatch where p.DocumentNo.ToString() == txDocNum.Value.ToString() select p).Count();
                if (cnt == 0)
                {
                    AdvancePayment = new TrnsAdvancePaymentBatch();
                    long nextId = 0;

                    nextId = ds.getNextId("TrnsAdvancePaymentBatch", "ID");
                    AdvancePayment.DocumentNo = Convert.ToInt32(nextId);
                    AdvancePayment.AdvanceType = Convert.ToInt32(cbAdvanceType.Value.Trim());
                    AdvancePayment.PayrollID = Convert.ToInt32(oPayroll.ID);
                    AdvancePayment.PayrollName = oPayroll.PayrollName;
                    AdvancePayment.PeriodID = Convert.ToInt32(oPayrollPeriod.ID);
                    AdvancePayment.PeriodName = oPayrollPeriod.PeriodName;
                    AdvancePayment.DocumentDate = DocDate;
                    AdvancePayment.DurationTo = DurationDate;
                    AdvancePayment.Status = cbStatus.Value.ToString().Trim();
                    AdvancePayment.PaidAccount = "";
                    AdvancePayment.CreatedDate = DateTime.Now;
                    AdvancePayment.CreatedBy = oCompany.UserName;

                    dbHrPayroll.TrnsAdvancePaymentBatch.InsertOnSubmit(AdvancePayment);

                }
                else
                {
                    AdvancePayment = (from p in dbHrPayroll.TrnsAdvancePaymentBatch where p.ID.ToString() == txDocNum.Value.ToString() select p).FirstOrDefault();
                    //AdvancePayment.AdvanceType = Convert.ToInt32(cbAdvanceType.Value.Trim());
                    //AdvancePayment.PayrollID = Convert.ToInt32(cbProll.Value.Trim());
                    //AdvancePayment.PeriodID = Convert.ToInt32(cbPeriod.Value.Trim());
                    AdvancePayment.UpdatedDate = DateTime.Now;
                    AdvancePayment.UpdatedBy = oCompany.UserName;
                }

                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {

                    code = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    code = code.Trim();
                    if (code != "")
                    {

                        Boolean flgActiveAdvance = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                        decimal approvedAdvance = Convert.ToDecimal((mtEmp.Columns.Item("clAdApr").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                        if (flgActiveAdvance == true && approvedAdvance > 0)
                        {

                            TrnsAdvancePaymentBatchDetail AdvancePaymentDettail;
                            string strdetailId = (mtEmp.Columns.Item("id").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            int detailId = Convert.ToInt32(strdetailId);
                            MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == code && p.FlgActive == true && p.PayrollID == oPayroll.ID select p).FirstOrDefault();
                            if (emp == null)
                            {
                                oApplication.StatusBar.SetText("Employee with EmpId " + code + " not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                continue;
                            }
                            if (cnt == 0)
                            {
                                AdvancePaymentDettail = new TrnsAdvancePaymentBatchDetail();
                                AdvancePayment.TrnsAdvancePaymentBatchDetail.Add(AdvancePaymentDettail);
                                AdvancePaymentDettail.EmployeeID = emp.ID;
                                AdvancePaymentDettail.EmployeeName = (mtEmp.Columns.Item("clEName").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value; //dtEmps.GetValue("EName", i);
                                AdvancePaymentDettail.BranchID = emp.BranchID;
                                AdvancePaymentDettail.BasicSalary = emp.BasicSalary;
                                AdvancePaymentDettail.BasicSalaryEarned = Convert.ToDecimal((mtEmp.Columns.Item("clBSEar").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.Allowance = Convert.ToDecimal((mtEmp.Columns.Item("clallw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.AbsentDeduction = Convert.ToDecimal((mtEmp.Columns.Item("clabsded").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.EarlyLateInMinutes = (int)Convert.ToDecimal((mtEmp.Columns.Item("clELT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.OverTimeInMinutes = (int)Convert.ToDecimal((mtEmp.Columns.Item("clOT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.OTDeduction = Convert.ToDecimal((mtEmp.Columns.Item("clOTD").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.AdvanceDeduction = Convert.ToDecimal((mtEmp.Columns.Item("clAdv").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.LoanInstallment = Convert.ToDecimal((mtEmp.Columns.Item("clLoan").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.NetAmount = Convert.ToDecimal((mtEmp.Columns.Item("clNamt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.AdvancePercentage = Convert.ToDecimal((mtEmp.Columns.Item("clAdPrc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.MaxAdvanceAllowed = Convert.ToDecimal((mtEmp.Columns.Item("clAdAlw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.AdvanceApproved = Convert.ToDecimal((mtEmp.Columns.Item("clAdApr").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                AdvancePaymentDettail.FlgActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                                AdvancePaymentDettail.CreatedDate = DateTime.Now;
                                AdvancePaymentDettail.CreatedBy = oCompany.UserName;
                            }
                            else
                            {
                                AdvancePaymentDettail = (from p in dbHrPayroll.TrnsAdvancePaymentBatchDetail where p.FKID == AdvancePayment.ID && p.EmployeeID == emp.ID && AdvancePayment.PeriodID == Convert.ToInt32(cbPeriod.Value.Trim()) select p).FirstOrDefault();

                                if (AdvancePaymentDettail != null)
                                {
                                    AdvancePaymentDettail.EmployeeID = emp.ID;
                                    AdvancePaymentDettail.EmployeeName = (mtEmp.Columns.Item("clEName").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value; //dtEmps.GetValue("EName", i);
                                    AdvancePaymentDettail.BranchID = emp.BranchID;
                                    AdvancePaymentDettail.BasicSalary = emp.BasicSalary;
                                    AdvancePaymentDettail.BasicSalaryEarned = Convert.ToDecimal((mtEmp.Columns.Item("clBSEar").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.Allowance = Convert.ToDecimal((mtEmp.Columns.Item("clallw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.AbsentDeduction = Convert.ToDecimal((mtEmp.Columns.Item("clabsded").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.EarlyLateInMinutes = (int)Convert.ToDecimal((mtEmp.Columns.Item("clELT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.OverTimeInMinutes = (int)Convert.ToDecimal((mtEmp.Columns.Item("clOT").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.OTDeduction = Convert.ToDecimal((mtEmp.Columns.Item("clOTD").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.AdvanceDeduction = Convert.ToDecimal((mtEmp.Columns.Item("clAdv").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.LoanInstallment = Convert.ToDecimal((mtEmp.Columns.Item("clLoan").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.NetAmount = Convert.ToDecimal((mtEmp.Columns.Item("clNamt").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.AdvancePercentage = Convert.ToDecimal((mtEmp.Columns.Item("clAdPrc").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.MaxAdvanceAllowed = Convert.ToDecimal((mtEmp.Columns.Item("clAdAlw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    AdvancePaymentDettail.AdvanceApproved = Convert.ToDecimal((mtEmp.Columns.Item("clAdApr").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                    bool flgcheckActive = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                                    AdvancePaymentDettail.FlgActive = flgcheckActive;
                                    AdvancePaymentDettail.UpdatedDate = DateTime.Now;
                                    AdvancePaymentDettail.UpdatedBy = oCompany.UserName;
                                }
                                //dbHrPayroll.SubmitChanges();
                            }
                        }
                    }
                }

                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                dbHrPayroll.SubmitChanges();
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    AddNewRecord();
                }
                else
                {
                    _fillFields();
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(strProcessing + "Error in Function submitForm.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void PostAdvancePayment()
        {
            DateTime DocDate = DateTime.MinValue;
            DateTime DurationDate = DateTime.MinValue;
            DocDate = DateTime.ParseExact(txDocDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            DurationDate = DateTime.ParseExact(txDurationTo.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


            int confirm = oApplication.MessageBox("Do you want to Post selected employees?", 1, "Yes", "No");
            if (confirm == 2)
            {
                oApplication.StatusBar.SetText("Posting canceled", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return;
            }

            try
            {
                for (int i = 1; i < mtEmp.RowCount + 1; i++)
                {
                    using (dbHRMS oDBPrivate = new dbHRMS(Program.ConStrHRMS))
                    {
                        string strEmpID = (mtEmp.Columns.Item("clEId").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        if (strEmpID.Trim() != "")
                        {
                            MstEmployee emp = (from p in oDBPrivate.MstEmployee where p.EmpID == strEmpID && p.FlgActive == true select p).FirstOrDefault();
                            if (emp == null)
                            {
                                oApplication.StatusBar.SetText("Employee with EmpId " + strEmpID + " not Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                continue;
                            }
                            var getUser = emp.MstUsers.FirstOrDefault();
                            int MasterAdv = (from p in oDBPrivate.MstAdvance 
                                             where p.FlgActive == true 
                                             select p).Count();
                            if (MasterAdv != 0)
                            {
                                var TragetedAdvanceDoc = (from a in oDBPrivate.TrnsAdvancePaymentBatch 
                                                          where a.DocumentNo == Convert.ToInt32(txDocNum.Value.Trim()) 
                                                          select a).FirstOrDefault();

                                var AdvancePaymentDettail = (from p in oDBPrivate.TrnsAdvancePaymentBatchDetail 
                                                             where p.FKID == TragetedAdvanceDoc.ID 
                                                             && p.EmployeeID == emp.ID 
                                                             && TragetedAdvanceDoc.PeriodID == Convert.ToInt32(cbPeriod.Value.Trim()) 
                                                             select p).FirstOrDefault();

                                Boolean flgActiveAdvance = (mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                                decimal approvedAdvance = Convert.ToDecimal((mtEmp.Columns.Item("clAdApr").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                if (AdvancePaymentDettail.FlgActive == true && approvedAdvance > 0)
                                {
                                    if (AdvancePaymentDettail.TargetAdvanceRefDoc == null)
                                    {
                                        TrnsAdvance oNewAdvnc = new TrnsAdvance();
                                        long nextId = 0;

                                        nextId = ds.getNextId("TrnsAdvance", "ID");

                                        oDBPrivate.TrnsAdvance.InsertOnSubmit(oNewAdvnc);
                                        oNewAdvnc.DocNum = Convert.ToInt32(nextId);
                                        oNewAdvnc.DocType = Convert.ToByte("20");
                                        oNewAdvnc.EmpID = emp.ID;
                                        oNewAdvnc.EmpName = emp.FirstName + " " + emp.MiddleName + " " + emp.LastName;
                                        oNewAdvnc.OriginatorID = emp.ID;
                                        oNewAdvnc.DateOfJoining = emp.JoiningDate;

                                        if (getUser != null)
                                        {
                                            oNewAdvnc.OriginatorID = emp.ID;
                                            oNewAdvnc.OriginatorName = getUser.UserID;
                                        }
                                        else
                                        {
                                            oNewAdvnc.OriginatorID = emp.ID;
                                            oNewAdvnc.OriginatorName = Convert.ToString(emp.ID);
                                        }
                                        oNewAdvnc.DesignationID = emp.DepartmentID;
                                        oNewAdvnc.Designation = emp.DepartmentName;
                                        oNewAdvnc.Salary = AdvancePaymentDettail.BasicSalary; //Convert.ToDecimal((mtEmp.Columns.Item("clBSal").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                        oNewAdvnc.AdvanceType = Convert.ToInt32(cbAdvanceType.Value.Trim());
                                        oNewAdvnc.RequestedAmount = AdvancePaymentDettail.AdvanceApproved; //Convert.ToDecimal((mtEmp.Columns.Item("clAdAlw").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                        oNewAdvnc.ApprovedAmount = AdvancePaymentDettail.AdvanceApproved; //Convert.ToDecimal((mtEmp.Columns.Item("clAdApr").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                        oNewAdvnc.RemainingAmount = AdvancePaymentDettail.AdvanceApproved;//Convert.ToDecimal((mtEmp.Columns.Item("clAdApr").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value);
                                        oNewAdvnc.RequiredDate = DurationDate;
                                        oNewAdvnc.MaturityDate = DurationDate;
                                        oNewAdvnc.FlgActive = AdvancePaymentDettail.FlgActive; //(mtEmp.Columns.Item("clActive").Cells.Item(i).Specific as SAPbouiCOM.CheckBox).Checked;
                                        oNewAdvnc.FlgStop = false;
                                        oNewAdvnc.CreateDate = DateTime.Now;
                                        oNewAdvnc.UserId = oCompany.UserName;
                                        oNewAdvnc.UpdateDate = DateTime.Now;
                                        oNewAdvnc.UpdateBy = oCompany.UserName;
                                        AdvancePaymentDettail.TargetAdvanceRefDoc = Convert.ToInt32(nextId);
                                        oNewAdvnc.TransID = Convert.ToInt32(AdvancePaymentDettail.ID);
                                        oDBPrivate.SubmitChanges();
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Advnace of Employee Code: " + emp.EmpID + " Already Posted on Advance Document No: " + AdvancePaymentDettail.TargetAdvanceRefDoc + "", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                TrnsAdvancePaymentBatch record = AdvancePaymentBatch.ElementAt<TrnsAdvancePaymentBatch>(currentRecord);
                record.Status = "2";
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function processBatch.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            _fillFields();
        }

        private Boolean ValidateRecord()
        {
            try
            {
                for (int i = 0; i < dtEmps.Rows.Count; i++)
                {
                    string strEmpCode;
                    decimal RequestedAmount = 0M, GrossSalary = 0M, TillDateSalary = 0M;
                    DateTime dtRequestDate = DateTime.ParseExact(txDurationTo.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    strEmpCode = dtEmps.GetValue(clEmployeeID.DataBind.Alias, i);
                    RequestedAmount = Convert.ToDecimal(dtEmps.GetValue(clBasicSalaryEarned.DataBind.Alias, i));
                    GrossSalary = Convert.ToDecimal(dtEmps.GetValue(clBasicSalary.DataBind.Alias, i));
                    TillDateSalary = Convert.ToDecimal(dtEmps.GetValue(clBasicSalaryEarned.DataBind.Alias, i));

                    var oEmp = (from a in dbHrPayroll.MstEmployee
                                where a.EmpID == strEmpCode
                                select a).FirstOrDefault();
                    if (oEmp != null)
                    {
                        var oPeriod = (from a in dbHrPayroll.CfgPeriodDates
                                       where a.StartDate <= dtRequestDate
                                       && a.EndDate >= dtRequestDate
                                       && a.PayrollId == oEmp.PayrollID
                                       select a).FirstOrDefault();
                        Int32 DayCount = 0;
                        if (oEmp.JoiningDate > oPeriod.StartDate)
                        {
                            DayCount = Convert.ToInt32((dtRequestDate - Convert.ToDateTime(oEmp.JoiningDate)).TotalDays + 1d);
                        }
                        else
                        {
                            DayCount = Convert.ToInt32((dtRequestDate - Convert.ToDateTime(oPeriod.StartDate)).TotalDays + 1d);
                        }
                        Int32 SavedAttendanceCount = (from a in dbHrPayroll.TrnsAttendanceRegister
                                                      where a.EmpID == oEmp.ID
                                                      && a.PeriodID == oPeriod.ID
                                                      && (a.Processed == null ? false : Convert.ToBoolean(a.Processed)) == true
                                                      select a).Count();
                        if (SavedAttendanceCount < DayCount)
                        {
                            MsgWarning("Attendance was not saved, You're not allowed to post Advance Batch. EmpCode : " + oEmp.EmpID);
                            return false;
                        }
                    }
                }
                //if (Program.systemInfo.FlgRetailRules1 == true)
                //{
                //    var oEmp = (from a in dbHrPayroll.MstEmployee
                //                where a.EmpID == txtEmpCode.Value.Trim()
                //                select a).FirstOrDefault();
                //    decimal RequestedAmount = string.IsNullOrEmpty(txtRequestedAmount.Value) ? 0 : Convert.ToDecimal(txtRequestedAmount.Value);
                //    decimal Salary = string.IsNullOrEmpty(txtSalary.Value) ? 0 : Convert.ToDecimal(txtSalary.Value);
                //    decimal ExpectedSalary = string.IsNullOrEmpty(txtExpSalary.Value) ? 0 : Convert.ToDecimal(txtExpSalary.Value);
                //    DateTime dtRequestDate = DateTime.ParseExact(txtReqDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                //    //decimal salaryBalance = Salary + ExpectedSalary;
                //    if (oEmp != null)
                //    {
                //        var oPeriod = (from a in dbHrPayroll.CfgPeriodDates
                //                       where a.StartDate <= dtRequestDate
                //                       && a.EndDate >= dtRequestDate
                //                       && a.PayrollId == oEmp.PayrollID
                //                       select a).FirstOrDefault();
                //        Int32 DayCount = Convert.ToInt32((dtRequestDate - Convert.ToDateTime(oPeriod.StartDate)).TotalDays + 1d);
                //        Int32 SavedAttendanceCount = (from a in dbHrPayroll.TrnsAttendanceRegister
                //                                      where a.EmpID == oEmp.ID
                //                                      && a.PeriodID == oPeriod.ID
                //                                      && (a.Processed == null ? false : Convert.ToBoolean(a.Processed)) == true
                //                                      select a).Count();
                //        if (SavedAttendanceCount < DayCount)
                //        {
                //            MsgWarning("Attendance was not saved, You're not allowed to enter advance request.");
                //            return false;
                //        }
                //        if (oEmp.AllowedAdvance == null)
                //        {
                //            MsgWarning("You must define Allowed percentage for advance on employee master.");
                //            return false;
                //        }
                //        else
                //        {
                //            decimal percentagevalue = Convert.ToDecimal(oEmp.AllowedAdvance);
                //            if (RequestedAmount > (ExpectedSalary * (percentagevalue / 100)))
                //            {
                //                MsgWarning("Requested amount can't be higher than allowed advance range " + oEmp.EmpID + " : " + string.Format("{0:0.00}", oEmp.AllowedAdvance) + " Percent.");
                //                return false;
                //            }
                //        }
                //    }

                //}
                //else
                //{
                //    decimal RequestedAmount = string.IsNullOrEmpty(txtRequestedAmount.Value) ? 0 : Convert.ToDecimal(txtRequestedAmount.Value);
                //    decimal Salary = string.IsNullOrEmpty(txtSalary.Value) ? 0 : Convert.ToDecimal(txtSalary.Value);
                //    decimal PrevAdvance = string.IsNullOrEmpty(txtPreAd.Value) ? 0 : Convert.ToDecimal(txtPreAd.Value);
                //    decimal salaryBalance = Salary + PrevAdvance;
                //    if (RequestedAmount > salaryBalance)
                //    {
                //        oApplication.StatusBar.SetText("Requested amount can't be greater than Salary", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //        return false;
                //    }
                //}
                //var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpCode.Value.Trim() select a).FirstOrDefault();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

}
