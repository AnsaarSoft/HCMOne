using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Collections;

namespace ACHR.Screen
{
    class frm_Increment : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.EditText txDocNum, txtEffectiveFrom, txIncValue, txtEmpFrom, txtEmpTo;
        SAPbouiCOM.ComboBox cbPayroll, cmbPeriod, cmbAppliedOn, cbIncType, cbStatus, cbLoc, cbDept, cbDes, cbJob, cbElement;
        SAPbouiCOM.Button btEmpFr, btEmpTo, btGetEmp;
        SAPbouiCOM.Matrix mtEmps;
        SAPbouiCOM.DataTable empDetail, dtPeriods;
        SAPbouiCOM.DataTable dtEmployees;

        SAPbouiCOM.Item ItxDocNum, ItxDateApp, ItxIncValue, ItxEmpFrom, ItxEmpTo;
        SAPbouiCOM.Item IcbPayroll, IcbPeriod, IcbApplOn, IcbIncType, IcbStatus, IcbLoc, IcbDept, IcbDes, IcbJob, IcbElement;
        SAPbouiCOM.Item ImtEmps;

        public IEnumerable<TrnsIncrementPromotion> increment;
        public string SelectedEmp = "";
        Boolean flgEmpFrom, flgEmpTo;
        IEnumerable<MstEmployee> oEmployees = null;

        #endregion

        #region SAP B1 Events

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            ClearControls();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            oForm.Refresh();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            //if (currentRecord + 1 == totalRecord)
            //{
            //    currentRecord = 0;
            //    oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Nev_Rec_Last"), SAPbouiCOM.BoMessageTime.bmt_Short, false);

            //}
            //else
            //{
            //    currentRecord = currentRecord + 1;
            //}
            //_fillFields();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            //if (currentRecord <= 0)
            //{
            //    currentRecord = totalRecord - 1;
            //}
            //else
            //{
            //    currentRecord = currentRecord - 1;
            //}
            //_fillFields();
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            //_fillFields();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            //_fillFields();
        }

        public override void fillFields()
        {
            base.fillFields();
            //oForm.Freeze(true);

            _fillFields();
            //oForm.Freeze(false);
        }

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (flgEmpTo && !flgEmpFrom)
            {
                txtEmpTo.Value = Program.EmpID;
                flgEmpFrom = false;
                flgEmpTo = false;
            }
            if (!flgEmpTo && flgEmpFrom)
            {
                txtEmpFrom.Value = Program.EmpID;
                flgEmpFrom = false;
                flgEmpTo = false;
            }
            //SetEmpValues();
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "txIncValue")
            {
                // getNewSalary();
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
                case "btEmpFr":
                    flgEmpTo = false;
                    flgEmpFrom = true;
                    OpenNewSearchFormFrom();
                    break;
                case "btEmpTo":
                    flgEmpTo = true;
                    flgEmpFrom = false;
                    OpenNewSearchFormTo();
                    break;
                case "btGetEmp":
                    getEmployees();
                    break;
                case "btCalc":
                    calcIncrement();
                    break;
                case "40":
                    getEmployees();
                    break;
            }
        }

        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCfl(ref pVal, ref BubbleEvent);
            SAPbouiCOM.ChooseFromListEvent ocfl = (SAPbouiCOM.ChooseFromListEvent)pVal;

            string itemId = pVal.ItemUID;

            if (itemId == "txEmpCode")
            {
                SAPbouiCOM.Item cflItem = oForm.Items.Item(itemId);
                SAPbouiCOM.DataTable oDT = ocfl.SelectedObjects;
                if (cflItem.Type.ToString() == "it_EDIT")
                {
                    SAPbouiCOM.EditText txt = oForm.Items.Item(itemId).Specific;
                    SelectedEmp = Convert.ToString(oDT.GetValue("empID", 0));
                    oForm.DataSources.UserDataSources.Item(itemId).ValueEx = Convert.ToString(oDT.GetValue("empID", 0));
                    oForm.DataSources.UserDataSources.Item("txHRMSId").ValueEx = Convert.ToString(oDT.GetValue("U_HrmsEmpId", 0));
                    oForm.DataSources.UserDataSources.Item("txEmpName").ValueEx = Convert.ToString(oDT.GetValue("firstName", 0)) + " " + Convert.ToString(oDT.GetValue("lastName", 0));
                    setEmpDetail(Convert.ToString(oDT.GetValue("U_HrmsEmpId", 0)));

                }
            }

        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "cbApplOn" || pVal.ItemUID == "cbIncType")
            {
                if (cbStatus.Value.Trim() == "" || cbStatus.Value.Trim() == "0")
                {
                    // getNewSalary();
                }
            }
            if (pVal.ItemUID == "cbPayroll")
            {

                FillPeriod(cbPayroll.Value);
            }

        }

        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
        }


        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            try
            {
                oForm.DataSources.UserDataSources.Add("txDocNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txDocNum = oForm.Items.Item("txDocNum").Specific;
                ItxDocNum = oForm.Items.Item("txDocNum");
                txDocNum.DataBind.SetBound(true, "", "txDocNum");

                oForm.DataSources.UserDataSources.Add("txDateApp", SAPbouiCOM.BoDataType.dt_DATE); // Hours Per Day
                txtEffectiveFrom = oForm.Items.Item("txDateApp").Specific;
                ItxDateApp = oForm.Items.Item("txDateApp");
                txtEffectiveFrom.DataBind.SetBound(true, "", "txDateApp");

                oForm.DataSources.UserDataSources.Add("txIncValue", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txIncValue = oForm.Items.Item("txIncValue").Specific;
                ItxIncValue = oForm.Items.Item("txIncValue");
                txIncValue.DataBind.SetBound(true, "", "txIncValue");

                oForm.DataSources.UserDataSources.Add("txEmpFrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpFrom = oForm.Items.Item("txEmpFrom").Specific;
                ItxEmpFrom = oForm.Items.Item("txEmpFrom");
                txtEmpFrom.DataBind.SetBound(true, "", "txEmpFrom");

                oForm.DataSources.UserDataSources.Add("txEmpTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpTo = oForm.Items.Item("txEmpTo").Specific;
                ItxEmpTo = oForm.Items.Item("txEmpTo");
                txtEmpTo.DataBind.SetBound(true, "", "txEmpTo");

                oForm.DataSources.UserDataSources.Add("cbPayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbPayroll = oForm.Items.Item("cbPayroll").Specific;
                IcbPayroll = oForm.Items.Item("cbPayroll");
                cbPayroll.DataBind.SetBound(true, "", "cbPayroll");

                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cmbPeriod = oForm.Items.Item("cbPeriod").Specific;
                IcbPeriod = oForm.Items.Item("cbPeriod");
                cmbPeriod.DataBind.SetBound(true, "", "cbPeriod");

                oForm.DataSources.UserDataSources.Add("cbIncType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbIncType = oForm.Items.Item("cbIncType").Specific;
                IcbIncType = oForm.Items.Item("cbIncType");
                cbIncType.DataBind.SetBound(true, "", "cbIncType");

                oForm.DataSources.UserDataSources.Add("cbApplOn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cmbAppliedOn = oForm.Items.Item("cbApplOn").Specific;
                IcbApplOn = oForm.Items.Item("cbApplOn");
                cmbAppliedOn.DataBind.SetBound(true, "", "cbApplOn");

                oForm.DataSources.UserDataSources.Add("cbStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbStatus = oForm.Items.Item("cbStatus").Specific;
                IcbStatus = oForm.Items.Item("cbStatus");
                cbStatus.DataBind.SetBound(true, "", "cbStatus");

                oForm.DataSources.UserDataSources.Add("cbLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbLoc = oForm.Items.Item("cbLoc").Specific;
                IcbLoc = oForm.Items.Item("cbLoc");
                cbLoc.DataBind.SetBound(true, "", "cbLoc");

                oForm.DataSources.UserDataSources.Add("cbDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbDept = oForm.Items.Item("cbDept").Specific;
                IcbDept = oForm.Items.Item("cbDept");
                cbDept.DataBind.SetBound(true, "", "cbDept");

                oForm.DataSources.UserDataSources.Add("cbDes", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbDes = oForm.Items.Item("cbDes").Specific;
                IcbDes = oForm.Items.Item("cbDes");
                cbDes.DataBind.SetBound(true, "", "cbDes");

                oForm.DataSources.UserDataSources.Add("cbJob", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbJob = oForm.Items.Item("cbJob").Specific;
                IcbJob = oForm.Items.Item("cbJob");
                cbJob.DataBind.SetBound(true, "", "cbJob");


                oForm.DataSources.UserDataSources.Add("cbElement", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbElement = oForm.Items.Item("cbElement").Specific;
                IcbElement = oForm.Items.Item("cbElement");
                cbElement.DataBind.SetBound(true, "", "cbElement");

                dtPeriods = oForm.DataSources.DataTables.Item("dtPeriods");
                empDetail = oForm.DataSources.DataTables.Item("empDetail");
                btEmpFr = oForm.Items.Item("btEmpFr").Specific;
                btEmpTo = oForm.Items.Item("btEmpTo").Specific;
                btGetEmp = oForm.Items.Item("btGetEmp").Specific;
                mtEmps = oForm.Items.Item("mtEmps").Specific;



                fillCombo("ValType", cbIncType);
                fillColumCombo("ValType", mtEmps.Columns.Item("incType"));

                fillCombo("incStatus", cbStatus);
                fillCombo("incAppl", cmbAppliedOn);
                fillColumCombo("incAppl", mtEmps.Columns.Item("applyOn"));
                fillCbs();
                oForm.PaneLevel = 1;
                AddNewRecord();
                //GetDataFilterData();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void picEmp(string Tx)
        {
            string EmpFrom, EmpTo;
            EmpFrom = txtEmpFrom.Value.Trim();
            EmpTo = txtEmpTo.Value.Trim();
            string strSql = sqlString.getSql("empElement", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Increment");
            pic = null;
            if (st.Rows.Count > 0)
            {
                if (Tx == "From")
                {
                    //txtEmpFrom.Value.Trim() = st.Rows[0][0].ToString();
                    EmpFrom = st.Rows[0][0].ToString();
                }
                else
                {
                    //txtEmpTo.Value = st.Rows[0][0].ToString();
                    EmpTo = st.Rows[0][0].ToString();
                }
                // setEmpDetail(Convert.ToString(txHRMSId.Value));


            }
        }

        private void setEmpDetail(string empid)
        {
            try
            {
                /*
                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p).Single();
                txEmpName.Value = emp.FirstName + " " + emp.LastName;
                txBasic.Value = emp.BasicSalary.ToString();
                txGross.Value = ds.getEmpGross(emp).ToString();
                txPayroll.Value = emp.CfgPayrollDefination.PayrollName;
                */
            }
            catch { oApplication.SetStatusBarMessage("Invalid Employee"); }


        }

        private void getNewSalaryOriginal(decimal basicSalary, decimal grossSalary, MstEmployee cEmpID, decimal incValue, string incType, string applyOn, out decimal newBasic, out decimal newGross, out decimal Arears)
        {


            decimal currentBasic = basicSalary;
            decimal currentGross = grossSalary;
            decimal FixValue = mfmGetFixElementValue(cEmpID);
            newBasic = 0.00M;
            newGross = 0.00M;
            Arears = 0.00M;
            if (cmbAppliedOn == null) return;
            if (cmbAppliedOn.Value == "" || cbIncType.Value == "" || txIncValue.Value == "" || currentGross == 0 || currentBasic == 0)
            {
                newBasic = 0.00M;
                newBasic = 0.00M;
            }
            else
            {
                decimal incAmount = 0.00M;
                if (applyOn == "0")
                {
                    if (incType == "Per")
                    {
                        incAmount = currentBasic * incValue / 100;
                        newBasic = currentBasic + incAmount;
                        newGross = currentGross + incAmount;
                    }
                    if (incType == "Amnt")
                    {
                        newBasic = currentBasic + incValue;
                        newGross = currentGross + incValue;
                    }

                }
                else
                {
                    if (incType == "Per")
                    {
                        incAmount = currentGross * incValue / 100;
                        newGross = currentGross + incAmount;
                        //newBasic = currentBasic + currentBasic * incValue / 100;
                        newBasic = (currentBasic / (currentGross - FixValue)) * (newGross - FixValue);

                    }
                    if (incType == "Amnt")
                    {

                        newGross = currentGross + incValue;
                        //newBasic = newGross / currentGross * currentBasic;
                        newBasic = (currentBasic / (currentGross - FixValue)) * (newGross - FixValue);
                    }
                }
            }


        }

        private void getNewSalary(decimal basicSalary, decimal grossSalary, MstEmployee cEmpID, decimal incValue, string incType, string applyOn, out decimal newBasic, out decimal newGross, out decimal Arears)
        {


            decimal currentBasic = basicSalary;
            decimal currentGross = grossSalary;
            decimal FixValue = mfmGetFixElementValue(cEmpID);
            newBasic = 0.00M;
            newGross = 0.00M;
            Arears = 0.00M;
            if (cmbAppliedOn == null) return;
            if (cmbAppliedOn.Value == "" || cbIncType.Value == "" || txIncValue.Value == "" || currentGross == 0 || currentBasic == 0)
            {
                newBasic = 0.00M;
                newBasic = 0.00M;
            }
            else
            {
                decimal incAmount = 0.00M;
                decimal incAmountBasic = 0.00M;
                if (applyOn == "0")
                {
                    if (incType == "Per")
                    {
                        currentGross = currentGross - FixValue;
                        incAmount = currentBasic * incValue / 100;
                        newBasic = currentBasic + incAmount;
                        #region New Increment Gross
                        decimal TotalArear = 0.00M;
                        foreach (TrnsEmployeeElementDetail ele in cEmpID.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                        {
                            if (((bool)ele.MstElements.FlgEffectOnGross))
                            {
                                string elementName = "";
                                decimal ElementOldAmount = 0.0M;
                                decimal ElementArear = 0.0M;
                                var EarningElement = (from a in dbHrPayroll.MstElementEarning where a.ElementID == ele.MstElements.Id && a.ValueType != "FIX" && ele.MstElements.Type != "Non-Rec" select a).FirstOrDefault();
                                if (ele.MstElements.ElmtType == "Ear" && EarningElement != null)
                                {
                                    elementName = ele.MstElements.Description;
                                    ElementOldAmount = Convert.ToDecimal(ele.Amount);
                                    if (incType == "Amnt")
                                    {
                                        ElementArear = Convert.ToDecimal(incValue) * Convert.ToDecimal(ele.Value) / 100;
                                    }
                                    else
                                    {
                                        ElementArear = Convert.ToDecimal(incValue) * Convert.ToDecimal(ele.Value) / 100;
                                    }
                                    if (ElementArear > 0)
                                    {
                                        TotalArear = TotalArear + ElementArear;
                                    }
                                }
                            }
                        }
                        #endregion
                        //newGross = currentGross + incAmount + FixValue;
                        newGross = currentGross + TotalArear + FixValue;
                    }
                    if (incType == "Amnt")
                    {
                        newBasic = currentBasic + incValue;
                        #region New Increment Gross
                        decimal TotalArear = 0.00M;
                        foreach (TrnsEmployeeElementDetail ele in cEmpID.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                        {
                            if (((bool)ele.MstElements.FlgEffectOnGross))
                            {
                                string elementName = "";
                                decimal ElementOldAmount = 0.0M;
                                decimal ElementArear = 0.0M;
                                var EarningElement = (from a in dbHrPayroll.MstElementEarning where a.ElementID == ele.MstElements.Id && a.ValueType != "FIX" && ele.MstElements.Type != "Non-Rec" select a).FirstOrDefault();
                                if (ele.MstElements.ElmtType == "Ear" && EarningElement != null)
                                {
                                    elementName = ele.MstElements.Description;
                                    ElementOldAmount = Convert.ToDecimal(ele.Amount);
                                    if (incType == "Amnt")
                                    {
                                        ElementArear = Convert.ToDecimal(incValue) * Convert.ToDecimal(ele.Value) / 100;
                                    }
                                    else
                                    {
                                        ElementArear = Convert.ToDecimal(incValue) * Convert.ToDecimal(ele.Value) / 100;
                                    }
                                    if (ElementArear > 0)
                                    {
                                        TotalArear = TotalArear + ElementArear;
                                    }
                                }
                            }
                        }
                        #endregion
                        // newGross = currentGross + incValue;
                        newGross = currentGross + TotalArear + incValue;
                    }

                }
                else
                {
                    if (incType == "Per")
                    {
                        currentGross = currentGross - FixValue;
                        incAmount = currentGross * incValue / 100;

                        newGross = currentGross + incAmount + FixValue;
                        //newBasic = currentBasic + currentBasic * incValue / 100;
                        //newBasic = (currentBasic / (currentGross - FixValue)) * (newGross - FixValue);
                        incAmountBasic = currentBasic * incValue / 100;
                        newBasic = currentBasic + incAmountBasic;
                    }
                    if (incType == "Amnt")
                    {

                        newGross = currentGross + incValue;
                        //newBasic = newGross / currentGross * currentBasic;
                        newBasic = (currentBasic / (currentGross - FixValue)) * (newGross - FixValue);
                    }
                }
            }


        }

        private void getData()
        {
            CodeIndex.Clear();
            increment = from p in dbHrPayroll.TrnsIncrementPromotion select p;
            int i = 0;
            foreach (TrnsIncrementPromotion ele in increment)
            {
                CodeIndex.Add(ele.Id.ToString(), i);
                i++;
            }
            totalRecord = i;
        }

        private void ClearControls()
        {
            getData();
            //GetDataFilterData();
            long nextId = ds.getNextId("TrnsIncrementPromotion", "ID");
            txDocNum.Value = nextId.ToString();
            empDetail.Rows.Clear();
            txtEmpFrom.Value = "";
            txtEmpTo.Value = "";
            txIncValue.Value = "0.00";
            empDetail.Rows.Clear();
            cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            //txEmpCode.Active = true;
            oForm.Items.Item("1").Enabled = true;
            txtEffectiveFrom.Active = true;
            IcbStatus.Enabled = false;


        }

        private void assignIncrement(TrnsIncrementPromotion inc)
        {

            if (cbStatus.Value.Trim() == "1")
            {
                MstEmployee emp;
                int empCnt = empDetail.Rows.Count;
                for (int i = 0; i < empCnt; i++)
                {

                    decimal newBasicSalary = Convert.ToDecimal(empDetail.GetValue("nBasic", i));
                    string empCode = Convert.ToString(empDetail.GetValue("Code", i));
                    decimal newGross = Convert.ToDecimal(empDetail.GetValue("nGross", i));
                    decimal oldGross = Convert.ToDecimal(empDetail.GetValue("Grs", i));
                    decimal incrementValue = Convert.ToDecimal(empDetail.GetValue("incValue", i));
                    decimal arear = Convert.ToDecimal(empDetail.GetValue("arear", i));
                    emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empCode select p).Single();
                    TrnsEmployeeElementDetail arearElement = new TrnsEmployeeElementDetail();
                    arearElement.ValueType = "FIX";
                    arearElement.Value = arear;
                    arearElement.FlgOneTimeConsumed = false;
                    arearElement.FlgTaxable = true;
                    arearElement.PeriodId = (long)inc.PayIn;
                    arearElement.FlgActive = true;
                    arearElement.MstElements = (from p in dbHrPayroll.MstElements where p.Id.ToString() == inc.ArearElementId.ToString() select p).Single();
                    emp.TrnsEmployeeElement[0].TrnsEmployeeElementDetail.Add(arearElement);
                    string applyOn = Convert.ToString(empDetail.GetValue("applyOn", i));

                    emp.BasicSalary = Convert.ToDecimal(newBasicSalary);
                    emp.GrossSalary = Convert.ToDecimal(newGross);
                    if (applyOn == "0")
                    {
                        foreach (TrnsEmployeeElementDetail ele in emp.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                        {
                            if (((bool)ele.MstElements.FlgEffectOnGross))
                            {
                                string elementName = "";
                                if (ele.ValueType.Trim() == "POB")
                                {
                                    elementName = ele.MstElements.Description;
                                    ele.Amount = emp.BasicSalary * ele.Value / 100;
                                }

                            }
                        }
                    }

                    if (applyOn == "1")
                    {
                        foreach (TrnsEmployeeElementDetail ele in emp.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                        {
                            if (((bool)ele.MstElements.FlgEffectOnGross))
                            {
                                if (false)
                                {
                                    if (ele.MstElements.MstElementEarning[0].ValueType.Trim() == "FIX")
                                    {
                                        decimal eleoldValue = (decimal)ele.Value;
                                        if (cbIncType.Value.ToString().Trim() == "Per")
                                        {
                                            ele.Amount = eleoldValue + eleoldValue * Convert.ToDecimal(incrementValue) / 100;
                                            ele.Value = ele.Amount;
                                        }
                                        else
                                        {
                                            ele.Amount = eleoldValue * Convert.ToDecimal(newGross) / Convert.ToDecimal(oldGross);
                                            ele.Value = ele.Amount;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                inc.StatusRec = 2;
                dbHrPayroll.SubmitChanges();
                _fillFields();
            }
        }

        private bool submitForm()
        {
            bool submitResult = true;
            bool assigning = false;
            if (cbStatus.Value.Trim() == "1")
            {
                int confirm = oApplication.MessageBox("Assigning Increment will update the salaries of listed employees. Are you sure to assign?", 1, "Yes", "No");
                if (confirm != 1) return false;
                assigning = true;
            }
            try
            {

                TrnsIncrementPromotion increment;


                int cnt = (from p in dbHrPayroll.TrnsIncrementPromotion where p.Id.ToString() == txDocNum.Value select p).Count();
                if (cnt > 0)
                {
                    increment = (from p in dbHrPayroll.TrnsIncrementPromotion where p.Id == Convert.ToInt16(txDocNum.Value) select p).Single();
                    if (assigning)
                    {

                        assignIncrement(increment);
                        return false;
                    }

                }
                else
                {

                    increment = new TrnsIncrementPromotion();
                    increment.CreateDate = DateTime.Now;
                    increment.UserId = oCompany.UserName;

                }

                increment.UpdateDate = DateTime.Now;
                increment.UpdatedBy = oCompany.UserName;
                CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cbPayroll.Value.ToString() select p).Single();
                increment.CfgPayrollDefination = pr;
                increment.IncreamentValue = Convert.ToDecimal(txIncValue.Value);
                increment.IncreamentType = cbIncType.Value;
                increment.ApplicableDate = DateTime.ParseExact(txtEffectiveFrom.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                increment.ApplyOn = Convert.ToInt32(cmbAppliedOn.Value);
                increment.StatusRec = Convert.ToInt32(cbStatus.Value);
                increment.PayIn = Convert.ToInt32(cmbPeriod.Value);
                increment.ArearElementId = Convert.ToInt16(cbElement.Value);
                MstEmployee IncEmp;
                int empCnt = empDetail.Rows.Count;
                if (empCnt == 0) return false;
                for (int i = 0; i < empCnt; i++)
                {
                    string empCode = empDetail.GetValue("Code", i);

                    IncEmp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empCode select p).Single();
                    TrnsIncDetail empIncrement;
                    int incEmp = (from p in dbHrPayroll.TrnsIncDetail where p.IncrId.ToString() == txDocNum.Value.ToString().Trim() && p.EmpCode == empCode select p).Count();
                    if (incEmp > 0)
                    {
                        empIncrement = (from p in dbHrPayroll.TrnsIncDetail where p.IncrId.ToString() == txDocNum.Value.ToString().Trim() && p.EmpCode == empCode select p).Single();
                    }
                    else
                    {
                        empIncrement = new TrnsIncDetail();
                        empIncrement.EmpCode = empCode;
                        empIncrement.MstEmployee = IncEmp;
                        increment.TrnsIncDetail.Add(empIncrement);

                    }
                    empIncrement.EmpName = empDetail.GetValue("Name", i);
                    empIncrement.IncType = empDetail.GetValue("incType", i);
                    empIncrement.IncValue = Convert.ToDecimal(empDetail.GetValue("incValue", i));
                    empIncrement.ApplOn = empDetail.GetValue("applyOn", i);
                    empIncrement.CBasic = Convert.ToDecimal(empDetail.GetValue("cBasic", i));
                    empIncrement.CGross = Convert.ToDecimal(empDetail.GetValue("Grs", i));
                    empIncrement.NewBasic = Convert.ToDecimal(empDetail.GetValue("nBasic", i));
                    empIncrement.NewGross = Convert.ToDecimal(empDetail.GetValue("nGross", i));
                    empIncrement.Arear = Convert.ToDecimal(empDetail.GetValue("arear", i));
                }



                dbHrPayroll.SubmitChanges();
                //GetDataFilterData();
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    ClearControls();
                }
                else
                {
                    _fillFields();
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);

                submitResult = false;
            }


            return submitResult;
        }

        private void _fillFields()
        {
            oForm.Freeze(true);

            try
            {
                if (currentRecord >= 0)
                {

                    TrnsIncrementPromotion record;
                    record = increment.ElementAt<TrnsIncrementPromotion>(currentRecord);
                    txDocNum.Value = record.Id.ToString();


                    txIncValue.Value = record.IncreamentValue.ToString();
                    oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = record.PayIn.ToString();
                    oForm.DataSources.UserDataSources.Item("cbApplOn").ValueEx = record.ApplyOn.ToString();
                    oForm.DataSources.UserDataSources.Item("cbStatus").ValueEx = record.StatusRec.ToString();
                    oForm.DataSources.UserDataSources.Item("cbPayroll").ValueEx = record.PayrollID.ToString();
                    oForm.DataSources.UserDataSources.Item("cbElement").ValueEx = record.ArearElementId.ToString();
                    oForm.DataSources.UserDataSources.Item("cbIncType").ValueEx = record.IncreamentType.ToString();


                    if (record.ApplicableDate != null)
                    {
                        oForm.DataSources.UserDataSources.Item("txDateApp").ValueEx = Convert.ToDateTime(record.ApplicableDate).ToString("yyyyMMdd");
                    }
                    else
                    {
                        txtEffectiveFrom.Value = "";
                    }

                    empDetail.Rows.Clear();
                    int i = 0;
                    foreach (TrnsIncDetail incDetail in record.TrnsIncDetail)
                    {
                        empDetail.Rows.Add(1);
                        empDetail.SetValue("Code", i, incDetail.EmpCode.ToString());
                        empDetail.SetValue("Name", i, incDetail.EmpName.ToString());
                        empDetail.SetValue("cBasic", i, incDetail.CBasic.ToString());
                        empDetail.SetValue("Grs", i, incDetail.CGross.ToString());
                        empDetail.SetValue("applyOn", i, incDetail.ApplOn.ToString());
                        empDetail.SetValue("incType", i, incDetail.IncType.ToString());
                        empDetail.SetValue("incValue", i, incDetail.IncValue.ToString());

                        empDetail.SetValue("nBasic", i, incDetail.NewBasic.ToString());
                        empDetail.SetValue("nGross", i, incDetail.NewGross.ToString());
                        empDetail.SetValue("arear", i, incDetail.Arear.ToString());


                        i++;
                    }
                    mtEmps.LoadFromDataSource();
                    if (record.StatusRec.ToString() == "0")
                    {
                        IcbStatus.Enabled = true;
                        oForm.Items.Item("1").Enabled = true;
                        oForm.Items.Item("40").Enabled = true;
                        oForm.Items.Item("btGetEmp").Enabled = true;
                        oForm.Items.Item("btCalc").Enabled = true;
                        //oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;

                    }
                    else
                    {
                        IcbStatus.Enabled = false;
                        oForm.Items.Item("1").Enabled = false;
                        oForm.Items.Item("40").Enabled = false;
                        oForm.Items.Item("btGetEmp").Enabled = false;
                        oForm.Items.Item("btCalc").Enabled = false;
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    }


                }
            }
            catch (Exception ex)
            {
                oForm.Freeze(false);
            }

            oForm.Freeze(false);
        }

        private void fillCbs()
        {
            int i = 0;
            string selId = "0";
            IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
            foreach (CfgPayrollDefination pr in prs)
            {
                cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);
                i++;
            }

            cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            FillPeriod(cbPayroll.Value);

            IEnumerable<MstDepartment> depts = (from p in dbHrPayroll.MstDepartment orderby p.DeptName ascending select p);
            cbDept.ValidValues.Add("0", "All");
            foreach (MstDepartment dept in depts)
            {
                cbDept.ValidValues.Add(dept.ID.ToString(), dept.DeptName);

            }
            cbDept.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            cbLoc.ValidValues.Add("0", "All");
            IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation orderby p.Description ascending select p;

            foreach (MstLocation loc in locs)
            {
                cbLoc.ValidValues.Add(loc.Id.ToString(), loc.Description);

            }
            cbLoc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            cbElement.ValidValues.Add("-1", "");
            IEnumerable<MstElements> eles = from p in dbHrPayroll.MstElements where p.Type == "Non-Rec" select p;

            foreach (MstElements ele in eles)
            {
                cbElement.ValidValues.Add(ele.Id.ToString(), ele.Description);

            }
            cbElement.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);


            cbDes.ValidValues.Add("0", "All");
            IEnumerable<MstDesignation> designations = from p in dbHrPayroll.MstDesignation orderby p.Description ascending select p;

            foreach (MstDesignation des in designations)
            {
                cbDes.ValidValues.Add(des.Id.ToString(), des.Description);

            }
            cbDes.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);


            cbJob.ValidValues.Add("0", "All");
            IEnumerable<MstJobTitle> jobtitles = from p in dbHrPayroll.MstJobTitle orderby p.Description ascending select p;

            foreach (MstJobTitle jt in jobtitles)
            {
                cbJob.ValidValues.Add(jt.Id.ToString(), jt.Description);

            }
            cbJob.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);


        }

        private void FillPeriodOld(string payroll)
        {
            dtPeriods.Rows.Clear();
            if (cmbPeriod.ValidValues.Count > 0)
            {
                int vcnt = cmbPeriod.ValidValues.Count;
                for (int k = vcnt - 1; k >= 0; k--)
                {
                    cmbPeriod.ValidValues.Remove(cmbPeriod.ValidValues.Item(k).Value);
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
                    cmbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());

                    if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                    {
                        selId = pd.ID.ToString();
                    }

                    i++;
                }
                try
                {
                    cmbPeriod.Select(selId);
                    //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                }
                catch { }
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                dtPeriods.Rows.Clear();
                if (cmbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cmbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cmbPeriod.ValidValues.Remove(cmbPeriod.ValidValues.Item(k).Value);
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
                        if (pd.FlgVisible == null ? false : (bool)pd.FlgVisible && pd.FlgLocked != true)
                        {
                            cmbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        }
                        count++;
                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();
                        //if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                        //{
                        //    selId = pd.ID.ToString();
                        //}
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
                        cmbPeriod.Select(selId);
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

        private void calcIncrementOriginal()
        {
            try
            {
                mtEmps.FlushToDataSource();
                decimal newGross, newBasic, Arear = 0.00M;
                decimal cGross, cBasic = 0.00M;
                int periodDays = 0;
                int ApplyOn = 0;

                CfgPeriodDates payIn = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cmbPeriod.Value.ToString() select p).FirstOrDefault();
                if (payIn == null)
                {
                    oApplication.StatusBar.SetText("Select PayIn period.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtEffectiveFrom.Value))
                {
                    oApplication.StatusBar.SetText("Effective from date field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                DateTime effectiveFrom = DateTime.ParseExact(txtEffectiveFrom.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DateTime effectiveTo = Convert.ToDateTime(payIn.StartDate);
                int daystoPay = (effectiveTo - effectiveFrom).Days;
                if (daystoPay < 0)
                {
                    oApplication.StatusBar.SetText("Effective From can't be after PayIn Period Start date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                else
                {
                    //daystoPay++;
                }
                if (string.IsNullOrEmpty(cmbAppliedOn.Value) || cmbAppliedOn.Value == "-1")
                {
                    oApplication.StatusBar.SetText("Applied On field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                ApplyOn = Convert.ToInt32(cmbAppliedOn.Value);
                MstEmployee emp;
                decimal arerar = 0.00M;

                int empCnt = empDetail.Rows.Count;
                //if (empCnt == 1)
                //{
                //    oApplication.StatusBar.SetText("Select employee before calculation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return;
                //}
                for (int i = 0; i < empCnt; i++)
                {
                    string empid = empDetail.GetValue("Code", i);

                    if (string.IsNullOrEmpty(empid))
                    {
                        oApplication.StatusBar.SetText("Select employee before calculation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p).FirstOrDefault();
                    cBasic = Convert.ToDecimal(empDetail.GetValue("cBasic", i));
                    cGross = Convert.ToDecimal(empDetail.GetValue("Grs", i));
                    periodDays = (int)emp.CfgPayrollDefination.WorkDays;
                    if (periodDays == 0)
                    {
                        periodDays = (Convert.ToDateTime(payIn.EndDate) - Convert.ToDateTime(payIn.StartDate)).Days;
                        periodDays = periodDays + 1;
                    }
                    getNewSalary(cBasic, cGross, emp,
                        Convert.ToDecimal(empDetail.GetValue("incValue", i)), Convert.ToString(empDetail.GetValue("incType", i)),
                        Convert.ToString(empDetail.GetValue("applyOn", i)), out newBasic,
                        out newGross, out Arear);

                    if (periodDays > 0)
                    {
                        if (ApplyOn == 1)
                        {
                            Arear = ((newGross - cGross) / periodDays) * daystoPay;
                        }
                        else
                        {
                            Arear = ((newBasic - cBasic) / periodDays) * daystoPay;
                        }
                    }

                    empDetail.SetValue("nBasic", i, newBasic.ToString());
                    empDetail.SetValue("nGross", i, newGross.ToString());
                    empDetail.SetValue("arear", i, Arear.ToString());



                }
                mtEmps.LoadFromDataSource();
            }
            catch
            {
                oApplication.StatusBar.SetText("Error in Calculation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void calcIncrement()
        {
            try
            {
                mtEmps.FlushToDataSource();
                decimal newGross, newBasic, Arear = 0.00M, TotalArear = 0.00M;
                decimal cGross, cBasic = 0.00M;
                int periodDays = 0;
                int ApplyOn = 0;

                decimal OneMonthArear = 0.0M;
                CfgPeriodDates payIn = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cmbPeriod.Value.ToString() select p).FirstOrDefault();
                if (payIn == null)
                {
                    oApplication.StatusBar.SetText("Select PayIn period.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtEffectiveFrom.Value))
                {
                    oApplication.StatusBar.SetText("Effective from date field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                DateTime effectiveFrom = DateTime.ParseExact(txtEffectiveFrom.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DateTime effectiveTo = Convert.ToDateTime(payIn.StartDate);
                int daystoPay = (effectiveTo - effectiveFrom).Days;

                decimal EffectiveMonths = 0.0M;
                EffectiveMonths = ((effectiveTo.Year - effectiveFrom.Year) * 12) + effectiveTo.Month - effectiveFrom.Month;
                int month = effectiveFrom.Month;
                int year = effectiveFrom.Year;

                decimal GetPartialMonth = 0.0M;
                int intdaysIneffectiveMonth = System.DateTime.DaysInMonth(year, month);
                DateTime monthEndDate = new DateTime(effectiveFrom.Year, effectiveFrom.Month, intdaysIneffectiveMonth);
                GetPartialMonth = (monthEndDate - effectiveFrom).Days + 1;
                decimal decMonthCount = GetPartialMonth / intdaysIneffectiveMonth;
                decimal daystoPayforDaysCount = (effectiveTo - effectiveFrom).Days;
                EffectiveMonths = EffectiveMonths - 1;
                EffectiveMonths = EffectiveMonths + decMonthCount;

                if (daystoPay < 0)
                {
                    oApplication.StatusBar.SetText("Effective From can't be after PayIn Period Start date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                else
                {
                    //daystoPay++;
                }
                if (string.IsNullOrEmpty(cmbAppliedOn.Value) || cmbAppliedOn.Value == "-1")
                {
                    oApplication.StatusBar.SetText("Applied On field is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                ApplyOn = Convert.ToInt32(cmbAppliedOn.Value);
                MstEmployee emp;

                int empCnt = empDetail.Rows.Count;
                //if (empCnt == 1)
                //{
                //    oApplication.StatusBar.SetText("Select employee before calculation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return;
                //}
                for (int i = 0; i < empCnt; i++)
                {
                    string empid = empDetail.GetValue("Code", i);

                    if (string.IsNullOrEmpty(empid))
                    {
                        oApplication.StatusBar.SetText("Select employee before calculation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p).FirstOrDefault();
                    cBasic = Convert.ToDecimal(empDetail.GetValue("cBasic", i));
                    cGross = Convert.ToDecimal(empDetail.GetValue("Grs", i));
                    string IncrementType = Convert.ToString(empDetail.GetValue("incType", i));
                    decimal IncrementValue = Convert.ToDecimal(empDetail.GetValue("incValue", i));
                    periodDays = (int)emp.CfgPayrollDefination.WorkDays;
                    decimal FixValue = mfmGetFixElementValue((from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p).FirstOrDefault());
                    if (periodDays == 0)
                    {
                        periodDays = (Convert.ToDateTime(payIn.EndDate) - Convert.ToDateTime(payIn.StartDate)).Days;

                        periodDays = periodDays + 1;
                    }
                    getNewSalary(cBasic, cGross, emp,
                        Convert.ToDecimal(empDetail.GetValue("incValue", i)), Convert.ToString(empDetail.GetValue("incType", i)),
                        Convert.ToString(empDetail.GetValue("applyOn", i)), out newBasic,
                        out newGross, out Arear);
                    Arear = 0;
                    TotalArear = 0;
                    OneMonthArear = 0;
                    if (periodDays > 0)
                    {
                        Arear = (newBasic - cBasic);
                        TotalArear = TotalArear + Arear;
                        OneMonthArear = Arear;
                        foreach (TrnsEmployeeElementDetail ele in emp.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                        {
                            if (((bool)ele.MstElements.FlgEffectOnGross))
                            {
                                string elementName = "";
                                decimal ElementOldAmount = 0.0M;
                                decimal ElementArear = 0.0M;
                                var EarningElement = (from a in dbHrPayroll.MstElementEarning
                                                      where a.ElementID == ele.MstElements.Id
                                                      && a.ValueType != "FIX"
                                                      && ele.MstElements.Type != "Non-Rec"
                                                      && ele.FlgActive == true
                                                      select a).FirstOrDefault();
                                if (ele.MstElements.ElmtType == "Ear" && EarningElement != null)
                                {
                                    elementName = ele.MstElements.Description;
                                    ElementOldAmount = Convert.ToDecimal(ele.Amount);
                                    if (IncrementType == "Amnt")
                                    {
                                        ElementArear = Convert.ToDecimal(IncrementValue) * Convert.ToDecimal(ele.Value) / 100;
                                    }
                                    else
                                    {
                                        ElementArear = Convert.ToDecimal(Arear) * Convert.ToDecimal(ele.Value) / 100;
                                    }
                                    if (ElementArear > 0)
                                    {
                                        TotalArear = TotalArear + ElementArear;
                                    }
                                }
                            }
                        }
                    }

                    if (EffectiveMonths > 0)
                    {
                        TotalArear = (TotalArear * EffectiveMonths);
                        empDetail.SetValue("nBasic", i, newBasic.ToString());
                        empDetail.SetValue("nGross", i, newGross.ToString());
                        empDetail.SetValue("arear", i, TotalArear.ToString());
                    }
                    else
                    {
                        empDetail.SetValue("nBasic", i, newBasic.ToString());
                        empDetail.SetValue("nGross", i, newGross.ToString());
                        empDetail.SetValue("arear", i, "0.00");
                    }



                }
                mtEmps.LoadFromDataSource();
            }
            catch
            {
                oApplication.StatusBar.SetText("Error in Calculation.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void getEmployees()
        {
            // SAPbouiCOM.Column col = mtEmpPr.Columns.Item("isSel");
            //col.TitleObject.Caption = "";

            if (cbIncType.Value.Trim() == "-1" || txIncValue.Value.Trim() == "0.00" || cbElement.Value.Trim() == "-1" || txtEffectiveFrom.Value.Trim() == "1")
            {
                oApplication.SetStatusBarMessage("Provide the increment parameter before fetching employees.");
                return;
            }

            DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName, Program.objHrmsUI.logger);
            //var Data = (from e in dbHrPayroll.MstEmployee where e.FlgActive == true && e.PayrollID > 0 orderby e.SortOrder ascending select e).ToList();
            string strSql = "SELECT EmpID, SBOEmpCode, ID, FirstName + ' ' + ISNULL(MiddleName, '') AS empName ,  DepartmentName, LocationName FROM         " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee where payrollId = " + cbPayroll.Value.ToString().Trim() + " and ResignDate IS NULL AND ISNULL(flgActive,'1') = 1 ";
            if (cbDept.Value.ToString().Trim() != "0")
            {
                strSql += " and departmentId = " + cbDept.Value.ToString();

            }
            if (cbLoc.Value.ToString().Trim() != "0")
            {
                strSql += " and location = " + cbLoc.Value.ToString().Trim();
            }

            if (cbDes.Value.ToString().Trim() != "0")
            {
                strSql += " and DesignationID = '" + cbDes.Value.ToString().Trim() + "'";
            }

            if (cbJob.Value.ToString().Trim() != "0")
            {
                strSql += " and JobTitle = '" + cbJob.Value.ToString().Trim() + "'";
            }

            if (!String.IsNullOrEmpty(txtEmpFrom.Value.Trim()) && !String.IsNullOrEmpty(txtEmpTo.Value.Trim()))
            {
                //String FromEmpID, ToEmpID;
                //FromEmpID = Convert.ToString((from p in dbHrPayroll.MstEmployee where p.EmpID.Contains(txtEmpFrom.Value.Trim()) select p.ID).FirstOrDefault());
                //ToEmpID = Convert.ToString((from p in dbHrPayroll.MstEmployee where p.EmpID.Contains(txtEmpTo.Value.Trim()) select p.ID).FirstOrDefault());
                //strSql += " and ID BETWEEN " + FromEmpID + " AND "+ ToEmpID +"";
                Int32? FromEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault();
                Int32? ToEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                if (FromEmpID == null) FromEmpID = 0;
                if (ToEmpID == null) ToEmpID = 100000000;
                strSql += " and ISNULL(sortorder,0) between " + FromEmpID + " and " + ToEmpID + "";
            }
            System.Data.DataTable dtEmp = ds.getDataTable(strSql);
            empDetail.Rows.Clear();
            int i = 0;
            MstEmployee emp;
            decimal empGross = 0.00M;
            foreach (DataRow dr in dtEmp.Rows)
            {
                emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == dr["EmpID"].ToString() select p).Single();
                empDetail.Rows.Add(1);
                // empDetail.SetValue("isSel", i, "N");
                empDetail.SetValue("Code", i, dr["EmpID"].ToString());
                empDetail.SetValue("Name", i, dr["empName"].ToString());
                empGross = ds.getEmpGross(emp);
                empDetail.SetValue("cBasic", i, emp.BasicSalary.ToString());
                empDetail.SetValue("Grs", i, empGross.ToString());
                if (cmbAppliedOn.Value != "") empDetail.SetValue("applyOn", i, cmbAppliedOn.Value.ToString());
                if (cbIncType.Value != "") empDetail.SetValue("incType", i, cbIncType.Value.ToString());
                empDetail.SetValue("incValue", i, txIncValue.Value.ToString());

                i++;
            }

            mtEmps.LoadFromDataSource();
        }

        private void GetEmployees()
        {
            try
            {
                string location = "", department = "", designation = "", jobtitle = "", fromid = "", toid = "";
                location = cbLoc.Value.Trim();
                department = cbDept.Value.Trim();
                designation = cbDes.Value.Trim();
                jobtitle = cbJob.Value.Trim();
                fromid = txtEmpFrom.Value.Trim();
                toid = txtEmpTo.Value.Trim();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("GetEmployees : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private decimal mfmGetFixElementValue(MstEmployee oEmp)
        {
            decimal retValue = 0.0M;
            try
            {

                foreach (TrnsEmployeeElementDetail ele in oEmp.TrnsEmployeeElement.ElementAt(0).TrnsEmployeeElementDetail)
                {
                    if (((bool)ele.MstElements.FlgEffectOnGross))
                    {
                        if (ele.ValueType.Trim() == "FIX" && ele.MstElements.Type.Trim() == "Rec" && ele.MstElements.ElmtType.Trim() == "Ear")
                        {
                            //ele.Amount = emp.BasicSalary * ele.Value / 100;
                            retValue += Convert.ToDecimal(ele.Value);
                        }

                    }
                }
            }
            catch (Exception Ex)
            {
                retValue = 0.0M;
            }
            return retValue;
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.FromEmpId = "";
                string comName = "fromSrch";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormTo_Old()
        {
            try
            {
                Program.ToEmpId = "";
                string comName = "ToSearch";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.FromEmpId))
                {
                    txtEmpFrom.Value = Program.FromEmpId;
                }
                if (!string.IsNullOrEmpty(Program.ToEmpId))
                {
                    txtEmpTo.Value = Program.ToEmpId;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void OpenNewSearchFormFrom()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empPick";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormTo()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empPick";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetDataFilterData()
        {
            try
            {
                CodeIndex.Clear();
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {



                    string strOut = string.Empty;
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                    strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                    //IEnumerable<MstEmployee> oEmployees =(from e in dbHrPayroll.MstEmployee where Convert.ToString(e.PayrollID) == strOut  select e);
                    oEmployees = (from e in dbHrPayroll.MstEmployee where Convert.ToString(e.PayrollID) == strOut select e);
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {
                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;

                }
                else
                {
                    oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
                    Int32 i = 0;
                    foreach (MstEmployee OEmp in oEmployees)
                    {

                        CodeIndex.Add(OEmp.ID.ToString(), i);
                        i++;
                    }
                    totalRecord = i;
                }

            }


            //    IEnumerable<MstEmployee> oEmployees = (from a in dbHrPayroll.MstEmployee select a).ToList();
            //    Int32 i = 0;
            //    foreach (MstEmployee oEmp in oEmployees)
            //    {
            //        CodeIndex.Add(oEmp.ID.ToString(), i);
            //        i++;
            //    }
            //    totalRecord = i;
            //}
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }
        #endregion

    }
}
