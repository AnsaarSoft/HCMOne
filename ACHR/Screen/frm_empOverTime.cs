
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    partial class frm_empOverTime : HRMSBaseForm
    {

        #region variables

        /* Form Items Objects */
        SAPbouiCOM.Matrix mtOT;
        SAPbouiCOM.EditText txCode, txHrmsCode, txName, txHours, txAmount;
        SAPbouiCOM.ComboBox cbPayroll, cbPeriod;
        SAPbouiCOM.Column isNew, id;
        private SAPbouiCOM.DataTable dtOT;
        private decimal monthHours = Convert.ToDecimal(30.00 * 8.00);
        SAPbouiCOM.ChooseFromList oCfl;
        SAPbouiCOM.Item ItxCode, ItxHrmsCode, ItxName, ItxHours, ItxAmount, IcbPayroll, IcbPeriod;

        //**********************************
        private string SelectedEmp = "";
        private List<Program.ElementList> oListOfElementAmount = new List<Program.ElementList>();

        #endregion

        #region Form B1 events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {

            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
            //oForm.DefButton = "1";
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);

            switch (pVal.ItemUID)
            {
                case "cbPeriod":
                    getEmpOts(txHrmsCode.Value, cbPeriod.Value);
                    break;
            }
        }

        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {

                case "mtOT":

                    if (pVal.ColUID == "Hours")
                    {
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txHrmsCode.Value select p).Single();
                        mtOT.FlushToDataSource();
                        int rowNum = pVal.Row;
                        // setRowAmnt(rowNum - 1);
                        setRowAmnt(rowNum - 1, emp);
                        mtOT.LoadFromDataSource();
                    }



                    break;

            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            //oForm.Freeze(true);
            string OtHours = "00:00";
            BubbleEvent = true;
            try
            {
                if (pVal.ColUID == "fTime" || pVal.ColUID == "eTime")
                {
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txHrmsCode.Value select p).Single();
                    string[] StartDate = (mtOT.Columns.Item("fTime").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
                    string[] EndDate = (mtOT.Columns.Item("eTime").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
                    string RangeFrom = (mtOT.Columns.Item("fTime").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    string RangeTo = (mtOT.Columns.Item("eTime").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;

                    if (!string.IsNullOrEmpty(RangeFrom) && (!string.IsNullOrEmpty(RangeTo)))
                    {
                        double checkRangeFrom = TimeSpan.Parse(RangeFrom).TotalHours;
                        double checkRangeTo = TimeSpan.Parse(RangeTo).TotalHours;
                        if (checkRangeFrom > checkRangeTo)
                        {
                            oApplication.StatusBar.SetText("Range from is greater then range to", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                    }
                    if (StartDate.Length != 2 || EndDate.Length != 2)
                    {
                        //oApplication.StatusBar.SetText("Please enter valid time formate (00:00)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }

                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1])) - ((int.Parse(StartDate[0]) * 60) + int.Parse(StartDate[1]));
                        if (DurinMin < 0)
                            DurinMin += 1440;
                        int HrsDur = DurinMin / 60;
                        int MinDur = DurinMin % 60;
                        OtHours = HrsDur.ToString().PadLeft(2, '0') + ":" + MinDur.ToString().PadLeft(2, '0');
                        double decPunchTimeOUT = TimeSpan.Parse(OtHours).TotalHours;
                        // decimal decOtHours = CalculateHourTimeCount(OtHours);
                        string strOtHours = string.Format("{0:0.00}", decPunchTimeOUT);
                        if (!string.IsNullOrEmpty(strOtHours) && OtHours != "00:00")
                        {
                            (mtOT.Columns.Item("Hours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = strOtHours;
                        }
                        mtOT.FlushToDataSource();
                        int rowNum = pVal.Row;
                        //setRowAmnt(rowNum - 1);
                        //Per Hour Condition
                        Boolean flgFormula = false;
                        Boolean flgPerHour = false;
                        string code = (mtOT.Columns.Item("Code").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                        // string code = Convert.ToString(dtOT.GetValue("Code", rowNum));
                        if (!string.IsNullOrEmpty(code))
                        {
                            var otType = dbHrPayroll.MstOverTime.Where(o => o.Code == code).FirstOrDefault();
                            if (otType != null)
                            {
                                flgFormula = otType.FlgFormula == null ? false : Convert.ToBoolean(otType.FlgFormula);
                                flgPerHour = otType.FlgPerHour == null ? false : Convert.ToBoolean(otType.FlgPerHour);
                            }
                        }
                        if (flgPerHour == true)
                        {
                            setRowAmntPerHour(rowNum - 1, emp);
                        }
                        else
                        {
                            setRowAmnt(rowNum - 1, emp);
                        }

                        mtOT.LoadFromDataSource();

                    }
                }
                if (pVal.ColUID == "OTDate")
                {
                    DateTime OTDateX = DateTime.MinValue;
                    string OTDate = (mtOT.Columns.Item("OTDate").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (string.IsNullOrEmpty(OTDate)) return;
                    OTDateX = DateTime.ParseExact(OTDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime x = Convert.ToDateTime(OTDateX);
                    var CurrentPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.FlgLocked == true && a.StartDate <= Convert.ToDateTime(x) && a.EndDate >= Convert.ToDateTime(x) select a).FirstOrDefault();
                    if (CurrentPeriod != null)
                    {
                        oApplication.StatusBar.SetText("Overtime on locked periods are not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
            // oForm.Freeze(false);
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txHrmsCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txHrmsCode.Value)
            {
            }
            else
            {
                SetEmpValues();
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    doSubmit();
                    break;
                case "btPick":
                    //picEmp();
                    OpenNewSearchForm();
                    break;
                case "mtOT":
                    if (pVal.ColUID == "pick")
                    {
                        picOt(pVal.Row - 1);
                        //picOt(pVal.Row);
                    }
                    break;


            }
        }

        public override void etBeforeValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeValidate(ref pVal, ref BubbleEvent);

        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE;
            IniContrls();
            ItxHrmsCode.Enabled = true;
            ItxName.Enabled = true;
            txHrmsCode.Active = true;
            dtOT.Rows.Clear();
            OpenNewSearchForm();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }
        //public override void FindRecordMode()
        //{
        //    base.FindRecordMode();

        //    OpenNewSearchForm();

        //    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
        //}
        #endregion

        #region Function

        private void IniContrls()
        {
            try
            {
                dtOT.Rows.Clear();
                txCode.Value = "";
                txName.Value = "";
                txHrmsCode.Value = "";
                txHours.Value = "";
                txAmount.Value = "";

                txName.Active = true;
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void getData()
        {

        }

        private void InitiallizeForm()
        {
            try
            {
                mtOT = oForm.Items.Item("mtOT").Specific;
                isNew = mtOT.Columns.Item("isNew");
                id = mtOT.Columns.Item("id");
                isNew.Visible = false;
                id.Visible = false;
                mtOT.Columns.Item("ValType").Visible = false;
                mtOT.Columns.Item("Value").Visible = false;
                mtOT.Columns.Item("BaseVal").Visible = false;

                dtOT = oForm.DataSources.DataTables.Item("dtOT");
                dtOT.Rows.Clear();

                oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                txCode = oForm.Items.Item("txCode").Specific;
                ItxCode = oForm.Items.Item("txCode");
                txCode.DataBind.SetBound(true, "", "txCode");

                oForm.DataSources.UserDataSources.Add("txHrmsCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
                txHrmsCode = oForm.Items.Item("txHrmsCode").Specific;
                ItxHrmsCode = oForm.Items.Item("txHrmsCode");
                txHrmsCode.DataBind.SetBound(true, "", "txHrmsCode");

                oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
                txName = oForm.Items.Item("txName").Specific;
                ItxName = oForm.Items.Item("txName");
                txName.DataBind.SetBound(true, "", "txName");

                oForm.DataSources.UserDataSources.Add("txHours", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txHours = oForm.Items.Item("txHours").Specific;
                ItxHours = oForm.Items.Item("txHours");
                txHours.DataBind.SetBound(true, "", "txHours");

                oForm.DataSources.UserDataSources.Add("txAmount", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txAmount = oForm.Items.Item("txAmount").Specific;
                ItxAmount = oForm.Items.Item("txAmount");
                txAmount.DataBind.SetBound(true, "", "txAmount");

                oForm.DataSources.UserDataSources.Add("cbPayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbPayroll = oForm.Items.Item("cbPayroll").Specific;
                IcbPayroll = oForm.Items.Item("cbPayroll");
                cbPayroll.DataBind.SetBound(true, "", "cbPayroll");


                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbPeriod = oForm.Items.Item("cbPeriod").Specific;
                IcbPeriod = oForm.Items.Item("cbPeriod");
                cbPeriod.DataBind.SetBound(true, "", "cbPeriod");

                oCfl = oForm.ChooseFromLists.Item("OHEM");

                Program.objHrmsUI.loadHrmsEmps(oCfl);

                //getData();
                // fillMat();
                addEmptyRow();
                IniContrls();

                mtOT.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                //dtOT.Rows.Clear();
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
                    CfgPayrollDefination pr = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == payroll.Trim() select p).FirstOrDefault();
                    foreach (CfgPeriodDates pd in pr.CfgPeriodDates)
                    {
                        cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
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

        public void setEmpDetail(string empHrmsCode)
        {

            int cnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == empHrmsCode select p).Count();
            if (cnt > 0)
            {

                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empHrmsCode select p).FirstOrDefault();
                SelectedEmp = emp.EmpID;
                IniContrls();
                txHrmsCode.Value = emp.EmpID.ToString();
                txName.Value = emp.FirstName + " " + emp.LastName;

                oForm.DataSources.UserDataSources.Item("cbPayroll").ValueEx = emp.CfgPayrollDefination.PayrollName;
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
                IEnumerable<CfgPeriodDates> perioddateList = emp.CfgPayrollDefination.CfgPeriodDates.Where(d => d.FlgLocked == false).ToList();
                foreach (CfgPeriodDates pd in perioddateList)
                {
                    if (pd.FlgLocked == false)
                    {
                        cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());

                        if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                        {
                            selId = pd.ID.ToString();
                        }

                        i++;
                    }
                }
                try
                {
                    if (!string.IsNullOrEmpty(selId) && selId != "0")
                    {
                        cbPeriod.Select(selId, SAPbouiCOM.BoSearchKey.psk_ByValue);
                    }
                    else
                    {
                        cbPeriod.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    }
                }
                catch (Exception ex)
                {
                }
                //foreach (CfgPeriodDates pd in emp.CfgPayrollDefination.CfgPeriodDates)
                //{

                //    cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName);

                //    if (pd.StartDate < DateTime.Now && DateTime.Now < pd.EndDate)
                //    {
                //        selId = pd.ID.ToString();
                //    }
                //    i++;
                //}
                //  cbPeriod.Select(0,SAPbouiCOM.BoSearchKey.psk_Index);
                try
                {
                    oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                }
                catch { }
                getEmpOts(empHrmsCode, selId);
            }
            cbPeriod.Active = true;
            ItxHrmsCode.Enabled = true;
            ItxName.Enabled = false;
        }

        private void setOtInfo(string otCode, int rowNum)
        {
            int cnt = (from p in dbHrPayroll.MstOverTime where p.Code == otCode select p).Count();
            if (cnt > 0)
            {
                if (dtOT != null && dtOT.Rows.Count > 0)
                {
                    //MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txHrmsCode.Value select p).Single();
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txHrmsCode.Value select p).FirstOrDefault();
                    //MstOverTime ot = (from p in dbHrPayroll.MstOverTime where p.Code == otCode select p).Single();
                    MstOverTime ot = (from p in dbHrPayroll.MstOverTime where p.Code == otCode select p).FirstOrDefault();
                    dtOT.SetValue("Code", rowNum, ot.Code);
                    dtOT.SetValue("Descr", rowNum, ot.Description);
                    decimal baseValue = 0.00M;
                    if (ot.ValueType == "POB")
                    {
                        baseValue = (decimal)emp.BasicSalary;
                    }

                    if (ot.ValueType == "POG")
                    {

                        baseValue = ds.getEmpGross(emp);
                    }
                    if (ot.ValueType == "FIX")
                    {
                        baseValue = ot.Value.Value;
                    }

                    dtOT.SetValue("ValType", rowNum, ot.ValueType);
                    dtOT.SetValue("Value", rowNum, ot.Value.ToString());
                    dtOT.SetValue("BaseVal", rowNum, baseValue.ToString());
                    //setRowAmnt(rowNum);
                    setRowAmnt(rowNum, emp);
                    mtOT.LoadFromDataSource();

                    // mtOT.SetLineData(rowNum + 1);

                    // addEmptyElement();
                }

            }

        }

        public void getEmpOts(string empHrmsCode, string Period)
        {
            try
            {
                dtOT.Rows.Clear();
                int cnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == empHrmsCode select p).Count();
                if (cnt > 0)
                {
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empHrmsCode select p).Single();
                    CfgPeriodDates payrollPeriod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == Period select p).Single();
                    if (emp.CfgPayrollDefination.WorkDays > 0)
                    {
                        monthHours = Convert.ToDecimal(emp.CfgPayrollDefination.WorkDays * emp.CfgPayrollDefination.WorkHours);
                    }
                    TrnsEmployeeOvertime pot;
                    decimal totalHours = 0.0M;
                    decimal totalAmount = 0.0M;
                    cnt = (from p in dbHrPayroll.TrnsEmployeeOvertime where p.EmployeeId == emp.ID && p.Period == payrollPeriod.ID select p).Count();
                    if (cnt > 0)
                    {
                        pot = (from p in dbHrPayroll.TrnsEmployeeOvertime where p.EmployeeId == emp.ID && p.Period.ToString() == Period.ToString() select p).Single();
                        int k = 0;
                        IEnumerable<TrnsEmployeeOvertimeDetail> OTD = pot.TrnsEmployeeOvertimeDetail.Where(otd => otd.FlgActive == true).ToList();


                        foreach (TrnsEmployeeOvertimeDetail ot in OTD)
                        {
                            dtOT.Rows.Add(1);
                            dtOT.SetValue("id", k, ot.Id.ToString());
                            dtOT.SetValue("isNew", k, "N");
                            dtOT.SetValue("Code", k, ot.MstOverTime.Code);
                            dtOT.SetValue("Descr", k, ot.MstOverTime.Description);
                            dtOT.SetValue("pick", k, strCfl);

                            dtOT.SetValue("OTDate", k, Convert.ToDateTime(ot.OTDate).ToString("yyyyMMdd"));
                            dtOT.SetValue("fTime", k, ot.FromTime);
                            dtOT.SetValue("eTime", k, ot.ToTime);
                            dtOT.SetValue("Amount", k, ot.Amount.ToString());
                            dtOT.SetValue("BaseVal", k, ot.BasicSalary.ToString());
                            dtOT.SetValue("ValType", k, ot.ValueType);
                            if (ot.OTValue != null)
                            {
                                dtOT.SetValue("Value", k, ot.OTValue.ToString());
                            }
                            dtOT.SetValue("Hours", k, ot.OTHours.ToString());
                            dtOT.SetValue("Active", k, ot.FlgActive == true ? "Y" : "N");
                            if ((bool)ot.FlgActive)
                            {
                                totalHours += Convert.ToDecimal(ot.OTHours.ToString());
                                totalAmount += Convert.ToDecimal(ot.Amount.ToString());
                            }
                            k++;
                        }
                        oForm.DataSources.UserDataSources.Item("txAmount").ValueEx = totalAmount.ToString();
                        oForm.DataSources.UserDataSources.Item("txHours").ValueEx = totalHours.ToString();
                        cbPeriod.Active = true;
                    }
                    addEmptyRow();
                    mtOT.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void addEmptyRow()
        {


            if (dtOT.Rows.Count == 0)
            {
                dtOT.Rows.Add(1);

                dtOT.SetValue("id", 0, "0");
                dtOT.SetValue("isNew", 0, "Y");
                dtOT.SetValue("Code", 0, "");
                dtOT.SetValue("pick", 0, strCfl);

                dtOT.SetValue("Descr", 0, "");
                dtOT.SetValue("OTDate", 0, "");
                dtOT.SetValue("fTime", 0, "");
                dtOT.SetValue("eTime", 0, "");
                dtOT.SetValue("Hours", 0, "0.00");
                dtOT.SetValue("Amount", 0, "0.00");
                dtOT.SetValue("Active", 0, "N");
                mtOT.AddRow(1, mtOT.RowCount + 1);
            }
            else
            {
                if (dtOT.GetValue("Code", dtOT.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtOT.Rows.Add(1);
                    dtOT.SetValue("id", dtOT.Rows.Count - 1, "0");
                    dtOT.SetValue("isNew", dtOT.Rows.Count - 1, "Y");
                    dtOT.SetValue("Code", dtOT.Rows.Count - 1, "");
                    dtOT.SetValue("Descr", dtOT.Rows.Count - 1, "");
                    dtOT.SetValue("pick", dtOT.Rows.Count - 1, strCfl);

                    dtOT.SetValue("OTDate", dtOT.Rows.Count - 1, "");
                    dtOT.SetValue("fTime", dtOT.Rows.Count - 1, "");
                    dtOT.SetValue("eTime", dtOT.Rows.Count - 1, "");
                    dtOT.SetValue("Hours", dtOT.Rows.Count - 1, "0.00");
                    dtOT.SetValue("Amount", dtOT.Rows.Count - 1, "0.00");
                    dtOT.SetValue("Active", dtOT.Rows.Count - 1, "N");

                    mtOT.AddRow(1, mtOT.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();
            mtOT.LoadFromDataSource();

        }

        private void addNew()
        {
            ItxName.Enabled = true;

            IniContrls();
        }

        private void picOt(int rownum)
        {
            string strSql = sqlString.getSql("otmst", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select OT", "Select over time");
            pic = null;
            if (st.Rows.Count > 0)
            {
                string strCode = st.Rows[0][0].ToString();

                setOtInfo(strCode, rownum);

            }
        }

        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            if (!string.IsNullOrEmpty(txHrmsCode.Value))
            {
                SearchKeyVal.Add("EmpID", txHrmsCode.Value.ToString());
            }
        }

        private void picEmp()
        {

            //SearchKeyVal.Clear();
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empElementOvertime", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for elements");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txHrmsCode.Value = st.Rows[0][0].ToString();
                setEmpDetail(Convert.ToString(txHrmsCode.Value.ToString()));
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txHrmsCode.Value = Program.EmpID;
                    setEmpDetail(Convert.ToString(txHrmsCode.Value.ToString()));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                //Program.sqlString = "empMaster";
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

        private decimal CalculateHourTimeCount(string ActualOTHours)
        {
            decimal OtHours = 0;
            try
            {
                if (!string.IsNullOrEmpty(ActualOTHours))
                {
                    string[] EndDate = ActualOTHours.Split(':');
                    if (EndDate.Length != 2)
                    {
                        return 0;
                    }
                    else
                    {
                        int DurinMin = ((int.Parse(EndDate[0]) * 60) + int.Parse(EndDate[1]));
                        OtHours = DurinMin / 60;
                        decimal min = DurinMin % 60;
                        min = decimal.Multiply(0.01M, min);
                        OtHours = OtHours + min;
                    }
                }
                return OtHours;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }

        private void setRowAmnt(int rowNum)
        {

            decimal hours = Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
            decimal baseAmoun = Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
            decimal Val = Convert.ToDecimal(dtOT.GetValue("Value", rowNum));
            decimal amount = ((baseAmoun / monthHours) * Val / 100) * hours;
            dtOT.SetValue("Amount", rowNum, amount.ToString());

        }

        private void setRowAmnt(int rowNum, MstEmployee emp)
        {
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValue = 0.0M;
            decimal daysinYear = 0.0M;
            decimal amount = 0.0M, formulaAmount = 0, PerDayCap =0, PerMonthCap =0;
            Boolean flgFormula = false;
            Boolean flgPerHour = false;
            int otLineID = 0;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            try
            {
                string code = Convert.ToString(dtOT.GetValue("Code", rowNum));
                var otType = dbHrPayroll.MstOverTime.Where(o => o.Code == code).FirstOrDefault();
                if (!string.IsNullOrEmpty(code))
                {
                    if (otType != null)
                    {
                        //daysOT =Convert.ToInt16( OTTYpe.Days);
                        //HoursOT = Convert.ToDecimal(OTTYpe.Hours);
                        //fixValue = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        //daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        daysOT = string.IsNullOrEmpty(otType.Days) ? Convert.ToInt16(0) : Convert.ToInt16(otType.Days);
                        HoursOT = string.IsNullOrEmpty(otType.Hours) ? 0 : Convert.ToDecimal(otType.Hours);
                        fixValue = otType.FixValue == null ? 0 : Convert.ToDecimal(otType.FixValue);
                        daysinYear = otType.DaysinYear == null ? 0 : Convert.ToDecimal(otType.DaysinYear);
                        flgFormula = otType.FlgFormula == null ? false : Convert.ToBoolean(otType.FlgFormula);
                        flgPerHour = otType.FlgPerHour == null ? false : Convert.ToBoolean(otType.FlgPerHour);
                        PerDayCap = otType.PerDayCap == null ? 0 : Convert.ToDecimal(otType.PerDayCap);
                        PerMonthCap = otType.PerMonthCap == null ? 0 : Convert.ToDecimal(otType.PerMonthCap);
                        otLineID = Convert.ToInt32(otType.ID);
                    }
                }
                if (HoursOT > 0)
                {
                    workhours = HoursOT;
                }
                if (daysOT > 0)
                {
                    days = daysOT;
                }
                if (daysOT <= 0)
                {
                    string PayrollPeriod = cbPeriod.Value.Trim();
                    if (!string.IsNullOrEmpty(PayrollPeriod))
                    {
                        CfgPeriodDates LeaveFromPeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                        if (LeaveFromPeriod != null)
                        {
                            if (days < 1)
                            {
                                days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                            }
                            else if (days < 1)
                            {
                                days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                            }
                        }
                    }

                    monthHours = Convert.ToDecimal(days * workhours);
                    decimal hours = Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
                    decimal baseAmoun = Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
                    decimal Val = Convert.ToDecimal(dtOT.GetValue("Value", rowNum));
                    if (PerDayCap > 0)
                    {
                        if (hours > PerDayCap)
                        {
                            hours = PerDayCap;
                            MsgWarning("Per Day Cap was hit during OT calculations");
                        }
                    }
                    if (flgFormula)
                    {
                        formulaAmount = ParseFormula(otLineID);
                    }
                    //
                    if (flgPerHour)
                    {
                        formulaAmount = ParseFormula(otLineID);
                    }
                    else
                    {

                    }

                    //
                    if (fixValue > 0 && daysinYear > 0)
                    {
                        baseAmoun = baseAmoun + fixValue;
                        baseAmoun = baseAmoun * 12;
                        baseAmoun = baseAmoun / daysinYear;
                        baseAmoun = baseAmoun / workhours;
                        decimal baseAmountFormula = 0;
                        baseAmountFormula = baseAmountFormula * 12;
                        baseAmountFormula = baseAmountFormula / daysinYear;
                        baseAmountFormula = baseAmountFormula / workhours;
                        //baseAmoun = baseAmoun * 2;  //2 Tiem of Noraml Working Hours
                        amount = ((baseAmoun * Val / 100) + baseAmountFormula) * hours;
                        //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                    }
                    else
                    {
                        if (flgFormula)
                        {
                            amount = (((baseAmoun / monthHours) * Val / 100) + (formulaAmount / monthHours)) * hours;
                        }
                        else
                        {
                            if (otType.ValueType == "FIX")
                            {
                                amount = baseAmoun * hours;
                            }
                            else
                            {
                                amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                            }
                        }
                    }
                    dtOT.SetValue("Amount", rowNum, amount.ToString());
                }
            }
            catch (Exception ex)
            {
                //Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
                logger(ex);
            }

        }

        private void setRowAmntPerHour(int rowNum, MstEmployee emp)
        {
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValue = 0.0M;
            decimal daysinYear = 0.0M;
            decimal Weeks = 0.0M;
            decimal OTRatio = 0.0M;
            decimal amount = 0.0M, formulaAmount = 0;
            Boolean flgFormula = false;
            Boolean flgPerHour = false;
            int otLineID = 0;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            try
            {
                string code = Convert.ToString(dtOT.GetValue("Code", rowNum));
                var otType = dbHrPayroll.MstOverTime.Where(o => o.Code == code).FirstOrDefault();
                if (!string.IsNullOrEmpty(code))
                {

                    if (otType != null)
                    {
                        //daysOT =Convert.ToInt16( OTTYpe.Days);
                        //HoursOT = Convert.ToDecimal(OTTYpe.Hours);
                        //fixValue = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        //daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        daysOT = string.IsNullOrEmpty(otType.Days) ? Convert.ToInt16(0) : Convert.ToInt16(otType.Days);
                        HoursOT = string.IsNullOrEmpty(otType.Hours) ? 0 : Convert.ToDecimal(otType.Hours);
                        Weeks = string.IsNullOrEmpty(otType.Weeks) ? 0 : Convert.ToDecimal(otType.Weeks);
                        OTRatio = otType.Value == null ? 0 : Convert.ToDecimal(otType.Value);
                        fixValue = otType.FixValue == null ? 0 : Convert.ToDecimal(otType.FixValue);
                        daysinYear = otType.DaysinYear == null ? 0 : Convert.ToDecimal(otType.DaysinYear);
                        flgFormula = otType.FlgFormula == null ? false : Convert.ToBoolean(otType.FlgFormula);
                        flgPerHour = otType.FlgPerHour == null ? false : Convert.ToBoolean(otType.FlgPerHour);
                        otLineID = Convert.ToInt32(otType.ID);
                    }
                }
                if (HoursOT > 0)
                {
                    workhours = HoursOT;
                }
                if (daysOT > 0)
                {
                    days = daysOT;
                }
                if (daysOT <= 0)
                {
                    string PayrollPeriod = cbPeriod.Value.Trim();
                    if (!string.IsNullOrEmpty(PayrollPeriod))
                    {
                        CfgPeriodDates LeaveFromPeriod = dbHrPayroll.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                        if (LeaveFromPeriod != null)
                        {
                            if (days < 1)
                            {
                                days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                            }
                            else if (days < 1)
                            {
                                days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                            }
                        }
                    }
                }
                monthHours = Convert.ToDecimal(days * workhours);
                decimal hours = Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
                decimal baseAmoun = Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
                decimal Val = Convert.ToDecimal(dtOT.GetValue("Value", rowNum));

                formulaAmount = ParseFormula(otLineID);

                if (fixValue > 0 && daysinYear > 0)
                {
                    baseAmoun = baseAmoun + fixValue;
                    baseAmoun = baseAmoun * 12;
                    baseAmoun = baseAmoun / daysinYear;
                    baseAmoun = baseAmoun / workhours;
                    decimal baseAmountFormula = 0;
                    baseAmountFormula = baseAmountFormula * 12;
                    baseAmountFormula = baseAmountFormula / daysinYear;
                    baseAmountFormula = baseAmountFormula / workhours;
                    amount = ((baseAmoun * Val / 100) + baseAmountFormula) * hours;
                }
                else
                {
                    if (flgFormula)
                    {
                        amount = (((((formulaAmount * 12) / Weeks) / daysOT) / HoursOT) * OTRatio) * hours;
                    }
                    else
                    {
                        if (otType.ValueType == "FIX")
                        {
                            amount = baseAmoun * hours;
                        }
                        else
                        {
                            amount = (((((formulaAmount * 12) / Weeks) / daysOT) / HoursOT) * OTRatio) * hours;
                        }
                    }
                }
                dtOT.SetValue("Amount", rowNum, amount.ToString());
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }

        }

        private decimal ParseFormula(int OTMasterID)
        {
            decimal retValue = 0;
            try
            {
                MstOverTime otMaster = (from a in dbHrPayroll.MstOverTime where a.ID == OTMasterID select a).FirstOrDefault();
                if (otMaster == null) return 0;
                string otExpression = otMaster.Expression;
                oListOfElementAmount.Clear();
                GetComponents(otExpression);
                if (oListOfElementAmount.Count > 0)
                {
                    foreach (var OneElement in oListOfElementAmount)
                    {
                        otExpression = otExpression.Replace(OneElement.ElementName, OneElement.ElementAmount.ToString());
                    }
                    //oApplication.StatusBar.SetText("Expresion : " + otExpression, SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    Program.objHrmsUI.logger.LogEntry(Program.objHrmsUI.AppVersion, "EmpID : " + SelectedEmp + " Expression : " + otExpression);
                    System.Data.DataTable dt = new System.Data.DataTable();
                    retValue = Convert.ToDecimal(dt.Compute(otExpression, ""));
                }
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
            return retValue;
        }

        private void GetComponents(string pexpression)
        {
            try
            {
                int charCount = 0;
                string pString = "";
                List<string> oElementList = new List<string>();
                foreach (char OneChar in pexpression)
                {
                    if ((OneChar >= 65 && OneChar <= 90) || (OneChar >= 97 && OneChar <= 122) || (OneChar >= 48 && OneChar <= 57))
                    {
                        pString += Convert.ToString(OneChar);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(pString))
                        {
                            oElementList.Add(pString);
                            pString = "";
                        }
                    }
                    charCount++;
                }
                Program.objHrmsUI.logger.LogEntry(Program.objHrmsUI.AppVersion, "Paramet List : " + oElementList.Count.ToString());
                if (oElementList.Count > 0)
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == SelectedEmp select a).FirstOrDefault();
                    foreach (string OneComponent in oElementList)
                    {
                        if (OneComponent == "BS" || OneComponent == "GS")
                        {
                            Program.ElementList oBj = new Program.ElementList();
                            oBj.ElementName = OneComponent;
                            if (OneComponent == "BS")
                            {
                                oBj.ElementAmount = Convert.ToDecimal(oEmp.BasicSalary);
                            }
                            else if (OneComponent == "GS")
                            {
                                oBj.ElementAmount = ds.getEmpGross(oEmp, 1, 0);
                            }
                            oListOfElementAmount.Add(oBj);
                            continue;
                        }
                        else
                        {
                            #region Element Calculations
                            TrnsEmployeeElementDetail oEle = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                                              where a.TrnsEmployeeElement.EmployeeId == oEmp.ID && a.MstElements.ElementName == OneComponent
                                                              select a).FirstOrDefault();
                            if (oEle != null)
                            {
                                if (oEle.ElementType == "Ear")
                                {
                                    decimal elementamount = 0;
                                    decimal elementvalue = 0;
                                    decimal empGross = ds.getEmpGross(oEmp, 1, 0);
                                    string ValueType = oEle.ValueType.Trim();
                                    elementvalue = Convert.ToDecimal(oEle.Value);
                                    if (ValueType == "POB")
                                    {
                                        elementamount = Convert.ToDecimal(elementvalue) / 100 * (decimal)oEmp.BasicSalary;
                                    }
                                    if (ValueType == "POG")
                                    {
                                        elementamount = Convert.ToDecimal(elementvalue) / 100 * empGross;
                                    }
                                    if (ValueType == "FIX")
                                    {
                                        elementamount = elementvalue;
                                    }
                                    Program.ElementList oBj = new Program.ElementList();
                                    oBj.ElementName = OneComponent;
                                    oBj.ElementAmount = elementamount;
                                    oListOfElementAmount.Add(oBj);
                                    continue;
                                }
                                else if (oEle.ElementType == "Ded")
                                {
                                    decimal elementamount = 0;
                                    decimal elementvalue = 0;
                                    decimal empGross = ds.getEmpGross(oEmp, 1, 0);
                                    string ValueType = oEle.ValueType.Trim();
                                    elementvalue = Convert.ToDecimal(oEle.Value);
                                    if (ValueType == "POB")
                                    {
                                        elementamount = Convert.ToDecimal(elementvalue) / 100 * (decimal)oEmp.BasicSalary;
                                    }
                                    if (ValueType == "POG")
                                    {
                                        elementamount = Convert.ToDecimal(elementvalue) / 100 * empGross;
                                    }
                                    if (ValueType == "FIX")
                                    {
                                        elementamount = elementvalue;
                                    }
                                    Program.ElementList oBj = new Program.ElementList();
                                    oBj.ElementName = OneComponent;
                                    oBj.ElementAmount = (-1) * elementamount;
                                    oListOfElementAmount.Add(oBj);
                                    continue;
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void doFind()
        {

            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empForOvertime", SearchKeyVal);


            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select  Employee");
            pic = null;
            if (st.Rows.Count > 0)
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                currentObjId = st.Rows[0]["EmpID"].ToString();
                oForm.DataSources.UserDataSources.Item("txHrmsCode").ValueEx = st.Rows[0]["EmpID"].ToString();
                oForm.DataSources.UserDataSources.Item("txName").ValueEx = st.Rows[0]["FirstName"].ToString() + " " + st.Rows[0]["LastName"].ToString();

                setEmpDetail(currentObjId);


            }


        }

        private bool validateForm()
        {
            bool booResult = true;

            return booResult;
        }

        private void doSubmit()
        {
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
            {
                doFind();
            }
            else
            {
                if (validateForm())
                {
                    submitForm();
                }
            }
        }

        private void submitForm()
        {
            mtOT.FlushToDataSource();
            Byte Doctype = 24;
            int docNumber = 0;
            string id = "";
            string code = "";
            string isnew = "";

            //int confirm = oApplication.MessageBox("Are you sure you want to Add Overtime for Selected Employee? ", 3, "Yes", "No", "Cancel");
            //if (confirm == 2 || confirm == 3)
            //{
            //    return;
            //} 
            TrnsEmployeeOvertime empOverTime;
            int cnt = (from p in dbHrPayroll.TrnsEmployeeOvertime where p.Period.ToString() == cbPeriod.Value && p.MstEmployee.EmpID.ToString() == txHrmsCode.Value select p).Count();
            if (cnt > 0)
            {
                empOverTime = (from p in dbHrPayroll.TrnsEmployeeOvertime where p.Period.ToString() == cbPeriod.Value && p.MstEmployee.EmpID.ToString() == txHrmsCode.Value select p).FirstOrDefault();

            }
            else
            {
                empOverTime = new TrnsEmployeeOvertime();
                empOverTime.MstEmployee = (from p in dbHrPayroll.MstEmployee where p.EmpID == txHrmsCode.Value select p).FirstOrDefault();
                empOverTime.CfgPeriodDates = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value select p).FirstOrDefault();
                empOverTime.CreateDate = DateTime.Now;
                empOverTime.UserId = oCompany.UserName;
                dbHrPayroll.TrnsEmployeeOvertime.InsertOnSubmit(empOverTime);
            }

            empOverTime.UpdateDate = DateTime.Now;
            empOverTime.UpdatedBy = oCompany.UserName;
            for (int i = 0; i < dtOT.Rows.Count; i++)
            {
                code = Convert.ToString(dtOT.GetValue("Code", i));
                id = Convert.ToString(dtOT.GetValue("id", i));
                isnew = Convert.ToString(dtOT.GetValue("isNew", i));
                isnew = isnew.Trim();
                code = code.Trim();
                if (code != "")
                {
                    TrnsEmployeeOvertimeDetail otDetail;
                    if (isnew == "Y")
                    {
                        otDetail = new TrnsEmployeeOvertimeDetail();
                        empOverTime.TrnsEmployeeOvertimeDetail.Add(otDetail);
                        otDetail.DocType = Doctype;
                        docNumber = dbHrPayroll.TrnsEmployeeOvertimeDetail.Count();
                        docNumber = docNumber + 1;
                        otDetail.DocNum = docNumber;
                    }
                    else
                    {
                        //otDetail = (from p in dbHrPayroll.TrnsEmployeeOvertimeDetail where p.Id.ToString() == id select p).Single();
                        otDetail = (from p in dbHrPayroll.TrnsEmployeeOvertimeDetail where p.Id.ToString() == id select p).FirstOrDefault();
                    }
                    //otDetail.MstOverTime = (from p in dbHrPayroll.MstOverTime where p.Code == code select p).Single();
                    otDetail.MstOverTime = (from p in dbHrPayroll.MstOverTime where p.Code == code select p).FirstOrDefault();
                    otDetail.OTDate = dtOT.GetValue("OTDate", i);
                    string OVerTimeHrs = Convert.ToString(dtOT.GetValue("Hours", i));
                    if (!string.IsNullOrEmpty(OVerTimeHrs))
                    {
                        otDetail.OTHours = Convert.ToDecimal(OVerTimeHrs);
                    }
                    otDetail.FromTime = dtOT.GetValue("fTime", i);
                    otDetail.ToTime = dtOT.GetValue("eTime", i);
                    otDetail.Amount = Convert.ToDecimal(dtOT.GetValue("Amount", i));
                    otDetail.ValueType = dtOT.GetValue("ValType", i);
                    otDetail.OTValue = Convert.ToDecimal(dtOT.GetValue("Value", i));
                    otDetail.BasicSalary = Convert.ToDecimal(dtOT.GetValue("BaseVal", i));
                    otDetail.FlgActive = Convert.ToString(dtOT.GetValue("Active", i)) == "Y" ? true : false;
                }
            }

            dbHrPayroll.SubmitChanges();
            getEmpOts(txHrmsCode.Value, cbPeriod.Value);
        }

        #endregion

    }
}
