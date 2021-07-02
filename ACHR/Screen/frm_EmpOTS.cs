using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.IO;

namespace ACHR.Screen
{
    class frm_EmpOTS : HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.Matrix mtOT;
        SAPbouiCOM.Button btnSrch, btnClr, btnProc, btId2, btId, btnsave, btncancel, btnOK, btnClearOT;
        SAPbouiCOM.EditText txtempfrm, txtempTo, txtdocNum, txFilenam;
        SAPbouiCOM.ComboBox cb_dpt, cb_loc, cmbPayroll, cmbPeriod, cmbOvertime, cbStatus;
        SAPbouiCOM.DataTable dtOverTime;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clId, clEmpId, clEmpName, clHours, clAmount, clActive;
        SAPbouiCOM.Item IbtnSrch, IbtnClr, IbtnProc, IbtId2, IbtId, Ibtnsave, Ibtncancel, IbtnOK, Icb_dpt, Icb_loc, IcbProll, IcbPeriod, Icb_OT, IcbStatus, Itxtempfrm, ItxtempTo, ItxtdocNum, ibtnClearOT, ItxFilenam;
        private decimal monthHours = Convert.ToDecimal(30.00 * 8.00);
        public IEnumerable<TrnsSingleEntryOTRequest> batchs;

        int docid = 0;
        private string SelectedEmp = "";
        private List<Program.ElementList> oListOfElementAmount = new List<Program.ElementList>();
        System.Data.DataTable DtFile = new System.Data.DataTable();
        #endregion

        #region SAP Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {

            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            //oForm.EnableMenu("1288", false);  // Next Record
            //oForm.EnableMenu("1289", false);  // Pevious Record
            //oForm.EnableMenu("1290", false);  // First Record
            //oForm.EnableMenu("1291", false);  // Last record 
            InitiallizeForm();
            FillDepartmentInCombo();
            FillEmpLocationInCombo();
            FillOvertimeTypeInCombo();
            FillPayrollInCombo();
            fillCbs();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            btnsave.Caption = "Add";
            oForm.Update();
            oForm.Refresh();
            getData();
            oForm.Freeze(false);
            btnOK.Caption = "OK";
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            SetEmpValues();
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "btId2":
                        OpenNewSearchFormTo();
                        break;
                    case "btnSrch":
                        PopulateGridWithFilterExpression();
                        break;
                    case "btnSave":
                        saveRecords();
                        break;
                    case "btnProc":
                        PostOverTimeRecords();
                        break;
                    case "2":
                        break;
                    case "btclearot":
                        ClearPreviosOT();
                        break;
                    case "btPick":
                        LoadEmployeeToGrid();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpOTS Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
                if (pVal.ItemUID == "cb_proll")
                {
                    FillPeriod(cmbPayroll.Value.Trim());
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            BubbleEvent = true;
            oForm.Freeze(true);
            try
            {
                if (pVal.ColUID == "cl_hrs")
                {
                    decimal TotalmonthHours = Convert.ToDecimal(30 * 24);
                    string OtHours = (mtOT.Columns.Item("cl_hrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    decimal EnteredHours = string.IsNullOrEmpty(OtHours) ? 0 : Convert.ToDecimal(OtHours);
                    if (EnteredHours > TotalmonthHours)
                    {
                        oApplication.StatusBar.SetText("Please enter Hours less than month hours", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        (mtOT.Columns.Item("cl_amount").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = string.Format("{0:0.00}", 0.00M);
                        oForm.Freeze(false);
                        return;
                    }

                    string empId = (mtOT.Columns.Item("cl_EmpCode").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;

                    if (!string.IsNullOrEmpty(empId) && !string.IsNullOrEmpty(OtHours))
                    {
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empId select p).FirstOrDefault();
                        if (emp != null)
                        {
                            Boolean flgFormula = false;
                            Boolean flgPerHour = false;
                            decimal decOTHours = 0.0M;
                            decimal TotalAmount = 0.0M;
                            string OtID = cmbOvertime.Value.Trim();
                            if (emp.EmployeeContractType != "DWGS")
                            {
                                var otType = dbHrPayroll.MstOverTime.Where(o => o.ID == Convert.ToInt32(OtID)).FirstOrDefault();
                                if (otType != null)
                                {
                                    flgFormula = otType.FlgFormula == null ? false : Convert.ToBoolean(otType.FlgFormula);
                                    flgPerHour = otType.FlgPerHour == null ? false : Convert.ToBoolean(otType.FlgPerHour);
                                }
                                if (flgPerHour == true)
                                {
                                    decOTHours = Convert.ToDecimal(OtHours);
                                    TotalAmount = setRowAmntPerHours(emp, decOTHours);
                                }
                                else
                                {
                                    decOTHours = Convert.ToDecimal(OtHours);
                                    TotalAmount = setRowAmnt(emp, decOTHours);
                                }

                            }
                            else
                            {
                                decOTHours = Convert.ToDecimal(OtHours);
                                TotalAmount = setRowAmntDailyWagers(emp, decOTHours);
                            }
                            (mtOT.Columns.Item("cl_amount").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = string.Format("{0:0.00}", TotalAmount);
                        }
                    }
                    mtOT.FlushToDataSource();
                    mtOT.LoadFromDataSource();

                }
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
                oForm.Freeze(false);
            }
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            LoadToNewRecord();
        }

        #endregion

        #region Functions

        public override void fillFields()
        {
            base.fillFields();
            _fillFields();
        }

        private void getData()
        {
            try
            {
                CodeIndex.Clear();
                batchs = from p in dbHrPayroll.TrnsSingleEntryOTRequest select p;
                int i = 0;
                foreach (TrnsSingleEntryOTRequest ele in batchs)
                {
                    CodeIndex.Add(ele.Id.ToString(), i);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function getData.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void _fillFields()
        {
            oForm.Freeze(true);
            string strProcessing = "";
            try
            {
                if (currentRecord >= 0)
                {

                    TrnsSingleEntryOTRequest record = batchs.ElementAt<TrnsSingleEntryOTRequest>(currentRecord);
                    if (record == null) return;
                    docid = record.Id;
                    txtempfrm.Value = string.Empty;
                    txtempTo.Value = string.Empty;
                    cb_dpt.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    cb_loc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    txtdocNum.Value = record.DocNum.ToString();
                    cmbPayroll.Select(record.PayrollId.ToString());
                    cmbPeriod.Select(record.PeriodId.ToString());
                    cmbOvertime.Select(record.OTType.ToString());
                    if (record.DocStatus.ToString().Trim() == "0")
                    {
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        IbtnProc.Enabled = true;
                        Ibtnsave.Enabled = true;
                        cbStatus.Select("1");
                        btnsave.Caption = "Update";
                        IbtnSrch.Enabled = false;

                    }
                    else if (record.DocStatus.ToString() == "2")
                    {
                        cbStatus.Select("2");
                        Ibtnsave.Enabled = false;
                        IbtnSrch.Enabled = false;
                        IbtnProc.Enabled = false;
                    }
                    else
                    {
                        IbtnProc.Enabled = false;
                    }
                    dtOverTime.Rows.Clear();
                    int rowNum = 0;
                    foreach (TrnsSingleEntryOTDetail btd in record.TrnsSingleEntryOTDetail)
                    {
                        cmbOvertime.Select(btd.OverTimeID.ToString());
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.ID == btd.EmpId select p).Single();
                        strProcessing = "Error in Setting Employee Record with Employee ID --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + "  ";

                        dtOverTime.Rows.Add(1);
                        dtOverTime.SetValue("Id", rowNum, btd.ID.ToString());
                        dtOverTime.SetValue("No", rowNum, (rowNum + 1).ToString());
                        dtOverTime.SetValue("EmpId", rowNum, emp.EmpID);
                        dtOverTime.SetValue("EmpName", rowNum, btd.EmpName);
                        //dtOverTime.SetValue("Overtimetype", rowNum, btd.ValueType);
                        dtOverTime.SetValue("Hours", rowNum, btd.Hours.ToString());
                        dtOverTime.SetValue("Amount", rowNum, btd.Amount.ToString());
                        dtOverTime.SetValue("Active", rowNum, btd.FlgActive == true ? "Y" : "N");
                        rowNum++;
                    }
                    mtOT.LoadFromDataSource();
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

        private void InitiallizeForm()
        {

            //Initializing Buttons
            btnsave = oForm.Items.Item("btnSave").Specific;
            Ibtnsave = oForm.Items.Item("btnSave");
            btnOK = oForm.Items.Item("1").Specific;
            IbtnOK = oForm.Items.Item("1");
            btncancel = oForm.Items.Item("2").Specific;
            Ibtncancel = oForm.Items.Item("2");
            btnSrch = oForm.Items.Item("btnSrch").Specific;
            IbtnSrch = oForm.Items.Item("btnSrch");
            btnClr = oForm.Items.Item("btnClr").Specific;
            IbtnClr = oForm.Items.Item("btnClr");
            IbtnClr.Visible = false;
            btId = oForm.Items.Item("btId").Specific;
            btId2 = oForm.Items.Item("btId2").Specific;
            btnProc = oForm.Items.Item("btnProc").Specific;
            IbtnProc = oForm.Items.Item("btnProc");
            IbtnProc.Enabled = false;
            btnClearOT = oForm.Items.Item("btclearot").Specific;
            ibtnClearOT = oForm.Items.Item("btclearot");

            //Initializing Text Boxes

            txtdocNum = oForm.Items.Item("docNum").Specific;
            oForm.DataSources.UserDataSources.Add("docNum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            txtdocNum.DataBind.SetBound(true, "", "docNum");
            ItxtdocNum = oForm.Items.Item("docNum");


            txtempTo = oForm.Items.Item("empTo").Specific;
            oForm.DataSources.UserDataSources.Add("empTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            txtempTo.DataBind.SetBound(true, "", "empTo");
            ItxtempTo = oForm.Items.Item("empTo");

            txtempfrm = oForm.Items.Item("empfrm").Specific;
            oForm.DataSources.UserDataSources.Add("empfrm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            txtempfrm.DataBind.SetBound(true, "", "empfrm");
            Itxtempfrm = oForm.Items.Item("empfrm");
            //Initializing ComboBoxes
            cb_dpt = oForm.Items.Item("cb_dpt").Specific;
            oForm.DataSources.UserDataSources.Add("cb_dpt", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            cb_dpt.DataBind.SetBound(true, "", "cb_dpt");
            Icb_dpt = oForm.Items.Item("cb_dpt");

            cb_loc = oForm.Items.Item("cb_loc").Specific;
            oForm.DataSources.UserDataSources.Add("cb_loc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            cb_loc.DataBind.SetBound(true, "", "cb_loc");
            Icb_loc = oForm.Items.Item("cb_loc");

            cmbPayroll = oForm.Items.Item("cb_proll").Specific;
            oForm.DataSources.UserDataSources.Add("cb_proll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            cmbPayroll.DataBind.SetBound(true, "", "cb_proll");
            IcbProll = oForm.Items.Item("cb_proll");

            cmbPeriod = oForm.Items.Item("cb_Prd").Specific;
            oForm.DataSources.UserDataSources.Add("cb_Prd", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            cmbPeriod.DataBind.SetBound(true, "", "cb_Prd");
            IcbPeriod = oForm.Items.Item("cb_Prd");

            cmbOvertime = oForm.Items.Item("cb_OT").Specific;
            oForm.DataSources.UserDataSources.Add("cb_OT", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            cmbOvertime.DataBind.SetBound(true, "", "cb_OT");
            Icb_OT = oForm.Items.Item("cb_OT");

            cbStatus = oForm.Items.Item("cbStatus").Specific;
            oForm.DataSources.UserDataSources.Add("cbStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
            cbStatus.DataBind.SetBound(true, "", "cbStatus");
            IcbStatus = oForm.Items.Item("cbStatus");

            oForm.DataSources.UserDataSources.Add("txFilenam", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 150);
            txFilenam = oForm.Items.Item("txFilenam").Specific;
            ItxFilenam = oForm.Items.Item("txFilenam");
            txFilenam.DataBind.SetBound(true, "", "txFilenam");

            InitiallizegridMatrix();

            long nextId = ds.getNextId("TrnsSingleEntryOTRequest", "ID");
            txtdocNum.Value = nextId.ToString();
            btnOK.Caption = "OK";

        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtOverTime = oForm.DataSources.DataTables.Add("OverTime");
                dtOverTime.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtOverTime.Columns.Add("Id", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtOverTime.Columns.Add("EmpId", SAPbouiCOM.BoFieldsType.ft_Text);
                dtOverTime.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtOverTime.Columns.Add("Overtimetype", SAPbouiCOM.BoFieldsType.ft_Text);
                dtOverTime.Columns.Add("Hours", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtOverTime.Columns.Add("Amount", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtOverTime.Columns.Add("Active", SAPbouiCOM.BoFieldsType.ft_Text);

                mtOT = (SAPbouiCOM.Matrix)oForm.Items.Item("mtOT").Specific;
                oColumns = (SAPbouiCOM.Columns)mtOT.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("OverTime", "No");

                oColumn = oColumns.Item("cl_id");
                clId = oColumn;
                oColumn.DataBind.Bind("OverTime", "Id");
                clId.Visible = false;

                oColumn = oColumns.Item("cl_EmpCode");
                clEmpId = oColumn;
                oColumn.DataBind.Bind("OverTime", "EmpId");

                oColumn = oColumns.Item("cl_EmpName");
                clEmpName = oColumn;
                oColumn.DataBind.Bind("OverTime", "EmpName");

                //oColumn = oColumns.Item("cl_grdOT");
                //clOvertime = oColumn;
                //oColumn.DataBind.Bind("OverTime", "Overtimetype");
                //clOvertime.Visible = false;

                oColumn = oColumns.Item("cl_hrs");
                clHours = oColumn;
                oColumn.DataBind.Bind("OverTime", "Hours");

                oColumn = oColumns.Item("cl_amount");
                clAmount = oColumn;
                oColumn.DataBind.Bind("OverTime", "Amount");

                oColumn = oColumns.Item("clAct");
                clActive = oColumn;
                oColumn.DataBind.Bind("OverTime", "Active");
                btnOK.Caption = "OK";
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
                    txtempfrm.Value = Program.FromEmpId;
                }
                if (!string.IsNullOrEmpty(Program.ToEmpId))
                {
                    txtempTo.Value = Program.ToEmpId;
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
                Program.FromEmpId = "";
                string comName = "fromSrch";
                //Program.sqlString = "empPick";
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
                Program.ToEmpId = "";
                string comName = "ToSearch";
                //Program.sqlString = "empPick";
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

        private void FillDepartmentInCombo()
        {
            try
            {
                var Departments = from a in dbHrPayroll.MstDepartment select a;
                cb_dpt.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstDepartment Dept in Departments)
                {
                    cb_dpt.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
                if (Departments != null)
                {
                    cb_dpt.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpOTS Function: FillDepartmentInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillEmpLocationInCombo()
        {
            try
            {
                var EmpLocation = from a in dbHrPayroll.MstLocation select a;
                cb_loc.ValidValues.Add(Convert.ToString(0), Convert.ToString("ALL"));
                foreach (MstLocation empLocation in EmpLocation)
                {
                    cb_loc.ValidValues.Add(Convert.ToString(empLocation.Id), Convert.ToString(empLocation.Name));
                }
                if (EmpLocation != null)
                {
                    cb_loc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpOTS Function: FillEmpLocationInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void FillOvertimeTypeInCombo()
        {
            try
            {
                var OverTime = (from a in dbHrPayroll.MstOverTime where a.FlgActive == true select a).ToList();
                cmbOvertime.ValidValues.Add("-1", "");
                foreach (MstOverTime empOvertimeType in OverTime)
                {
                    cmbOvertime.ValidValues.Add(Convert.ToString(empOvertimeType.ID), Convert.ToString(empOvertimeType.Description));

                }
                if (OverTime != null)
                {
                    cmbOvertime.Select(1, SAPbouiCOM.BoSearchKey.psk_Index);

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpOTS Function: FillOvertimeTypeInCombo Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillPayrollInCombo()
        {
            try
            {
                #region Fill Payroll
                int i = 0;
                string strOut = string.Empty;

                string strSql = "SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                //strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                strOut = Convert.ToString(oRecSet.Fields.Item("U_PayrollType").Value);
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    if (strOut != null && strOut != "")
                    {
                        string strSql2 = sqlString.getSql("GetPayrollName", SearchKeyVal);
                        strSql2 = strSql2 + " where ID in (" + strOut + ")";
                        strSql2 += " ORDER BY ID Asc ";
                        System.Data.DataTable dt = ds.getDataTable(strSql2);
                        System.Data.DataView dv = dt.DefaultView;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int l = 0; l < dt.Rows.Count; l++)
                            {
                                string strPayrollName = dt.Rows[l]["PayrollName"].ToString();
                                Int32 intPayrollID = Convert.ToInt32(dt.Rows[l]["ID"].ToString());
                                cmbPayroll.ValidValues.Add(intPayrollID.ToString(), strPayrollName);

                            }
                        }
                        cmbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cmbPayroll.Value);
                    }
                    else
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cmbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cmbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cmbPayroll.Value);
                    }
                }
                else
                {
                    IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                    foreach (CfgPayrollDefination pr in prs)
                    {
                        cmbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        i++;
                    }

                    cmbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    FillPeriod(cmbPayroll.Value);
                }
                //End Fill Payroll
                #endregion             
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillPayrollInCombo : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void FillPeriod(string payroll)
        {
            try
            {

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

        private void fillCbs()
        {
            try
            {

                fillCombo("btchStatus", cbStatus);
                cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function fillCbs.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }


        }

        private void PopulateGridWithFilterExpression()
        {
            Int16 i = 0;

            //var Data = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID > 0 && e.FlgOTApplicable == true).ToList();
            var Data = (from e in dbHrPayroll.MstEmployee where e.FlgActive == true && e.PayrollID > 0 && e.FlgOTApplicable == true orderby e.SortOrder ascending select e).ToList();

            if (txtempfrm.Value != string.Empty && txtempTo.Value != string.Empty)
            {
                int? intEmpIdFrom = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtempfrm.Value.Trim() select a.SortOrder).FirstOrDefault();
                int? intEmpIdTo = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtempTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                if (intEmpIdFrom == null) intEmpIdFrom = 0;
                if (intEmpIdTo == null) intEmpIdTo = 100000;
                if (intEmpIdFrom > intEmpIdTo)
                {
                    oApplication.StatusBar.SetText("Searching criteria is not valid for selected range.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (intEmpIdTo >= intEmpIdFrom)
                {
                    Data = Data.Where(e => e.SortOrder >= intEmpIdFrom && e.SortOrder <= intEmpIdTo).ToList();
                }

            }
            if (cb_loc.Value.Trim() != "0" && cb_loc.Value.Trim() != string.Empty)
            {
                Data = Data.Where(e => e.Location == Convert.ToInt32(cb_loc.Value)).ToList();
            }
            if (cb_dpt.Value.Trim() != "0" && cb_dpt.Value.Trim() != string.Empty)
            {
                Data = Data.Where(e => e.DepartmentID == Convert.ToInt32(cb_dpt.Value)).ToList();
            }
            if (cmbPayroll.Value.Trim() != "0" && cmbPayroll.Value.Trim() != string.Empty)
            {
                Data = Data.Where(e => e.PayrollID == Convert.ToInt32(cmbPayroll.Value.Trim())).ToList();
            }
            if (Data != null && Data.Count > 0)
            {

                dtOverTime.Rows.Clear();
                dtOverTime.Rows.Add(Data.Count());
                foreach (var EMP in Data)
                {
                    dtOverTime.SetValue("No", i, i + 1);
                    dtOverTime.SetValue("Id", i, "0");
                    dtOverTime.SetValue("EmpId", i, EMP.EmpID);
                    dtOverTime.SetValue("EmpName", i, EMP.FirstName + " " + EMP.MiddleName + " " + EMP.LastName);
                    dtOverTime.SetValue("Active", i, "Y");
                    //dtOverTime.SetValue("Designation", i, !String.IsNullOrEmpty(EMP.DesignationName) ? EMP.DesignationName.ToString() : "");
                    //dtOverTime.SetValue("Department", i, !String.IsNullOrEmpty(EMP.DepartmentName) ? EMP.DepartmentName.ToString() : "");
                    //dtOverTime.SetValue("Location", i, !String.IsNullOrEmpty(EMP.LocationName) ? EMP.LocationName.ToString() : "");
                    i++;
                }
                mtOT.LoadFromDataSource();
            }
            else
            {
                dtOverTime.Rows.Clear();
                mtOT.LoadFromDataSource();
            }
        }

        private decimal setRowAmnt(MstEmployee emp, decimal Overtimehours)
        {
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValueCola = 0.0M;
            decimal daysinYear = 0.0M;
            decimal amount = 0.0M, formulaAmount = 0;
            decimal baseValue = 0.00M;
            decimal value = 0.00M;
            Boolean flgFormula = false;
            int otLineID = 0;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            try
            {
                string code = cmbOvertime.Value.Trim(); //Convert.ToString(dtOT.GetValue("Code", rowNum));
                if (string.IsNullOrEmpty(code) || code == "-1")
                {
                    oApplication.StatusBar.SetText("Please select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return 0;
                }
                var OTTYpe = dbHrPayroll.MstOverTime.Where(o => o.ID.ToString() == code).FirstOrDefault();
                if (!string.IsNullOrEmpty(code))
                {
                    if (OTTYpe != null)
                    {
                        value = Convert.ToDecimal(OTTYpe.Value.Value);
                        daysOT = string.IsNullOrEmpty(OTTYpe.Days) ? Convert.ToInt16(0) : Convert.ToInt16(OTTYpe.Days);
                        HoursOT = string.IsNullOrEmpty(OTTYpe.Hours) ? 0 : Convert.ToDecimal(OTTYpe.Hours);
                        fixValueCola = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        flgFormula = OTTYpe.FlgFormula == null ? false : Convert.ToBoolean(OTTYpe.FlgFormula);
                        if (OTTYpe.ValueType == "POB")
                        {
                            baseValue = (decimal)emp.BasicSalary;
                        }
                        if (OTTYpe.ValueType == "POG")
                        {
                            baseValue = ds.getEmpGross(emp);
                        }
                        if (OTTYpe.ValueType == "FIX")
                        {
                            baseValue = OTTYpe.Value.Value;
                        }
                        otLineID = Convert.ToInt32(OTTYpe.ID);
                        SelectedEmp = emp.EmpID;
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
                    string PayrollPeriod = cmbPeriod.Value.Trim();
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
                decimal hours = Overtimehours; //Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
                decimal baseAmoun = baseValue;  //Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
                decimal Val = value; //Convert.ToDecimal(dtOT.GetValue("Value", rowNum));
                if (flgFormula)
                {
                    formulaAmount = ParseFormula(otLineID);
                    baseValue = formulaAmount;
                }
                if (fixValueCola > 0 && daysinYear > 0)
                {
                    baseAmoun = baseAmoun + fixValueCola;
                    baseAmoun = baseAmoun * 12;
                    baseAmoun = baseAmoun / daysinYear;
                    baseAmoun = baseAmoun / workhours;
                    decimal baseAmountFormula = 0;
                    baseAmountFormula = baseAmountFormula * 12;
                    baseAmountFormula = baseAmountFormula / daysinYear;
                    baseAmountFormula = baseAmountFormula / workhours;
                    amount = ((baseAmoun * Val / 100) + baseAmountFormula) * hours;
                    //baseAmoun = baseAmoun * 2;  //2 Tiem of Noraml Working Hours
                    //amount = ((baseAmoun) * Val / 100) * hours;
                    //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                }
                else
                {
                    if (OTTYpe.ValueType == "FIX")
                    {
                        amount = baseValue * hours;
                    }
                    else
                    {
                        //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                        amount = (((baseAmoun / monthHours) * Val / 100) + (formulaAmount / monthHours)) * hours;
                    }
                }
                //dtOT.SetValue("Amount", rowNum, amount.ToString());

                return amount;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function setRowAmnt.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }

        private decimal setRowAmntPerHours(MstEmployee emp, decimal Overtimehours)
        {
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValueCola = 0.0M;
            decimal daysinYear = 0.0M;
            decimal Weeks = 0.0M;
            decimal OTRatio = 0.0M;
            decimal amount = 0.0M, formulaAmount = 0;
            decimal baseValue = 0.00M;
            decimal value = 0.00M;
            Boolean flgFormula = false;
            Boolean flgPerHour = false;
            int otLineID = 0;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            try
            {
                string code = cmbOvertime.Value.Trim(); //Convert.ToString(dtOT.GetValue("Code", rowNum));
                if (string.IsNullOrEmpty(code) || code == "-1")
                {
                    oApplication.StatusBar.SetText("Please select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return 0;
                }
                if (!string.IsNullOrEmpty(code))
                {
                    var OTTYpe = dbHrPayroll.MstOverTime.Where(o => o.ID.ToString() == code).FirstOrDefault();
                    if (OTTYpe != null)
                    {
                        value = Convert.ToDecimal(OTTYpe.Value.Value);
                        daysOT = string.IsNullOrEmpty(OTTYpe.Days) ? Convert.ToInt16(0) : Convert.ToInt16(OTTYpe.Days);
                        HoursOT = string.IsNullOrEmpty(OTTYpe.Hours) ? 0 : Convert.ToDecimal(OTTYpe.Hours);
                        Weeks = string.IsNullOrEmpty(OTTYpe.Weeks) ? 0 : Convert.ToDecimal(OTTYpe.Weeks);
                        OTRatio = OTTYpe.Value == null ? 0 : Convert.ToDecimal(OTTYpe.Value);
                        fixValueCola = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        flgFormula = OTTYpe.FlgFormula == null ? false : Convert.ToBoolean(OTTYpe.FlgFormula);
                        flgPerHour = OTTYpe.FlgPerHour == null ? false : Convert.ToBoolean(OTTYpe.FlgPerHour);
                        if (OTTYpe.ValueType == "POB")
                        {
                            baseValue = (decimal)emp.BasicSalary;
                        }
                        if (OTTYpe.ValueType == "POG")
                        {
                            baseValue = ds.getEmpGross(emp);
                        }
                        if (OTTYpe.ValueType == "FIX")
                        {
                            baseValue = OTTYpe.Value.Value;
                        }
                        otLineID = Convert.ToInt32(OTTYpe.ID);
                        SelectedEmp = emp.EmpID;
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
                    string PayrollPeriod = cmbPeriod.Value.Trim();
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
                decimal hours = Overtimehours; //Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
                decimal baseAmoun = baseValue;  //Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
                decimal Val = value; //Convert.ToDecimal(dtOT.GetValue("Value", rowNum));

                formulaAmount = ParseFormula(otLineID);

                if (fixValueCola > 0 && daysinYear > 0)
                {
                    baseAmoun = baseAmoun + fixValueCola;
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
                    amount = (((((formulaAmount * 12) / Weeks) / daysOT) / HoursOT) * OTRatio) * hours;
                }
                //dtOT.SetValue("Amount", rowNum, amount.ToString());

                return amount;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function setRowAmnt.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
            }

        }

        private decimal setRowAmntDailyWagers(MstEmployee emp, decimal Overtimehours)
        {
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValueCola = 0.0M;
            decimal daysinYear = 0.0M;
            decimal amount = 0.0M;
            decimal baseValue = 0.00M;
            decimal value = 0.00M;
            Boolean flgFormula = false;
            int otLineID = 0;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            try
            {
                string code = cmbOvertime.Value.Trim();
                if (string.IsNullOrEmpty(code) || code == "-1")
                {
                    oApplication.StatusBar.SetText("Please select OverTime Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return 0;
                }
                var OTTYpe = dbHrPayroll.MstOverTime.Where(o => o.ID.ToString() == code).FirstOrDefault();
                if (!string.IsNullOrEmpty(code))
                {
                    if (OTTYpe != null)
                    {
                        value = Convert.ToDecimal(OTTYpe.Value.Value);
                        daysOT = string.IsNullOrEmpty(OTTYpe.Days) ? Convert.ToInt16(0) : Convert.ToInt16(OTTYpe.Days);
                        HoursOT = string.IsNullOrEmpty(OTTYpe.Hours) ? 0 : Convert.ToDecimal(OTTYpe.Hours);
                        fixValueCola = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        flgFormula = OTTYpe.FlgFormula == null ? false : Convert.ToBoolean(OTTYpe.FlgFormula);
                        if (OTTYpe.ValueType == "POB")
                        {
                            baseValue = (decimal)emp.BasicSalary;
                        }
                        if (OTTYpe.ValueType == "POG")
                        {
                            baseValue = ds.getEmpGross(emp);
                        }
                        if (OTTYpe.ValueType == "FIX")
                        {
                            baseValue = OTTYpe.Value.Value;
                        }
                        otLineID = Convert.ToInt32(OTTYpe.ID);
                        SelectedEmp = emp.EmpID;
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
                    string PayrollPeriod = cmbPeriod.Value.Trim();
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
                decimal hours = Overtimehours;
                decimal baseAmoun = baseValue;
                decimal Val = value;
                if (OTTYpe.ValueType == "FIX")
                {
                    amount = baseValue * hours;
                }
                else
                {
                    //amount = (baseAmoun * Val / 100) * hours;
                    amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                }
                //if (flgFormula)
                //{
                //    formulaAmount = ParseFormula(otLineID);
                //}
                //if (fixValueCola > 0 && daysinYear > 0)
                //{
                //    baseAmoun = baseAmoun + fixValueCola;
                //    baseAmoun = baseAmoun * 12;
                //    baseAmoun = baseAmoun / daysinYear;
                //    baseAmoun = baseAmoun / workhours;
                //    decimal baseAmountFormula = 0;
                //    baseAmountFormula = baseAmountFormula * 12;
                //    baseAmountFormula = baseAmountFormula / daysinYear;
                //    baseAmountFormula = baseAmountFormula / workhours;
                //    amount = ((baseAmoun * Val / 100) + baseAmountFormula) * hours;
                //    //baseAmoun = baseAmoun * 2;  //2 Tiem of Noraml Working Hours
                //    //amount = ((baseAmoun) * Val / 100) * hours;
                //    //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                //}
                //else
                //{
                //    if (OTTYpe.ValueType == "FIX")
                //    {
                //        amount = baseValue * hours;
                //    }
                //    else
                //    {
                //        //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                //        amount = (((baseAmoun / monthHours) * Val / 100) + (formulaAmount / monthHours)) * hours;
                //    }
                //}
                //dtOT.SetValue("Amount", rowNum, amount.ToString());

                return amount;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function setRowAmnt.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0;
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
                string PayrollPeriod = cmbPeriod.Value.Trim();
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

                            #region Recorining Elements
                            TrnsEmployeeElementDetail oEle_Rec = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                                              where a.TrnsEmployeeElement.EmployeeId == oEmp.ID
                                                              && a.MstElements.ElementName == OneComponent
                                                             && a.MstElements.Type== "Rec"
                                                              select a).FirstOrDefault();
                            if (oEle_Rec != null)
                            {
                                if (oEle_Rec.ElementType == "Ear")
                                {
                                    decimal elementamount = 0;
                                    decimal elementvalue = 0;
                                    decimal empGross = ds.getEmpGross(oEmp, 1, 0);
                                    string ValueType = oEle_Rec.ValueType.Trim();
                                    elementvalue = Convert.ToDecimal(oEle_Rec.Value);
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
                                else if (oEle_Rec.ElementType == "Ded")
                                {
                                    decimal elementamount = 0;
                                    decimal elementvalue = 0;
                                    decimal empGross = ds.getEmpGross(oEmp, 1, 0);
                                    string ValueType = oEle_Rec.ValueType.Trim();
                                    elementvalue = Convert.ToDecimal(oEle_Rec.Value);
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

                            #region Non-Recorring Eleents
                            TrnsEmployeeElementDetail oEle_NonRec = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                                                     where a.TrnsEmployeeElement.EmployeeId == oEmp.ID
                                                                     && a.MstElements.ElementName == OneComponent
                                                                    && a.MstElements.Type == "Non-Rec"
                                                                    && a.PeriodId == Convert.ToInt32(PayrollPeriod)
                                                                     select a).FirstOrDefault();
                            if (oEle_NonRec != null)
                            {
                                if (oEle_NonRec.ElementType == "Ear")
                                {
                                    decimal elementamount = 0;
                                    decimal elementvalue = 0;
                                    decimal empGross = ds.getEmpGross(oEmp, 1, 0);
                                    string ValueType = oEle_NonRec.ValueType.Trim();
                                    elementvalue = Convert.ToDecimal(oEle_NonRec.Value);
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
                                else if (oEle_NonRec.ElementType == "Ded")
                                {
                                    decimal elementamount = 0;
                                    decimal elementvalue = 0;
                                    decimal empGross = ds.getEmpGross(oEmp, 1, 0);
                                    string ValueType = oEle_NonRec.ValueType.Trim();
                                    elementvalue = Convert.ToDecimal(oEle_NonRec.Value);
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

        private void GetComponents1(string pexpression)
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
                            else
                            {
                                Program.ElementList oBj = new Program.ElementList();
                                oBj.ElementName = OneComponent;
                                oBj.ElementAmount = 0;
                                oListOfElementAmount.Add(oBj);
                                continue;
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

        private void InsertRecord()
        {
            try
            {
                mtOT.FlushToDataSource();
                string strpayrollId = cmbPayroll.Value.Trim();
                string periodId = cmbPeriod.Value.Trim();
                string overTimeid = cmbOvertime.Value.Trim();
                if (string.IsNullOrEmpty(strpayrollId) || strpayrollId == "-1")
                {
                    oApplication.StatusBar.SetText("Please select payroll type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (string.IsNullOrEmpty(periodId) || periodId == "-1")
                {
                    oApplication.StatusBar.SetText("Please select Period month", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                if (string.IsNullOrEmpty(overTimeid) || overTimeid == "-1")
                {
                    oApplication.StatusBar.SetText("Please select OverTime type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                TrnsSingleEntryOTRequest objOverTime = new TrnsSingleEntryOTRequest();
                dbHrPayroll.TrnsSingleEntryOTRequest.InsertOnSubmit(objOverTime);

                objOverTime.DocNum = Convert.ToInt32(txtdocNum.Value);
                objOverTime.DocStatus = cbStatus.Value;
                objOverTime.PayrollId = Convert.ToInt32(strpayrollId);
                objOverTime.PeriodId = Convert.ToInt32(periodId);
                objOverTime.OTType = Convert.ToInt32(overTimeid);
                objOverTime.CreatedBy = oCompany.UserName;
                objOverTime.UpdatedBy = oCompany.UserName;
                objOverTime.CreatedDate = DateTime.Now;
                objOverTime.UpdatedDate = DateTime.Now;
                for (int i = 0; i < dtOverTime.Rows.Count; i++)
                {
                    string empId = dtOverTime.GetValue("EmpId", i);
                    string Active = dtOverTime.GetValue("Active", i);
                    MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empId select p).FirstOrDefault();
                    if (emp != null)
                    {
                        TrnsSingleEntryOTDetail objDetail = new TrnsSingleEntryOTDetail();
                        objOverTime.TrnsSingleEntryOTDetail.Add(objDetail);

                        objDetail.OverTimeID = Convert.ToInt32(overTimeid);
                        objDetail.EmpId = emp.ID;
                        objDetail.EmpName = dtOverTime.GetValue("EmpName", i);
                        objDetail.Hours = Convert.ToDecimal(dtOverTime.GetValue("Hours", i));
                        objDetail.Amount = Convert.ToDecimal(dtOverTime.GetValue("Amount", i));
                        if (Active == "Y")
                        {
                            objDetail.FlgActive = true;
                        }
                        else
                        {
                            objDetail.FlgActive = false;
                        }
                        objDetail.CreatedBy = oCompany.UserName;
                        objDetail.UpdatedBy = oCompany.UserName;
                        objDetail.CreatedDate = DateTime.Now;
                        objDetail.UpdatedDate = DateTime.Now;
                    }
                }
                dbHrPayroll.SubmitChanges();
                ClearControls();
                getData();
                oApplication.StatusBar.SetText("Record Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function InsertRecord.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void saveRecords()
        {
            try
            {
                string strDocStatus = cbStatus.Value.Trim();
                switch (strDocStatus)
                {
                    case "0":
                        InsertRecord();
                        break;
                    case "1":
                        UpdateRecords();
                        break;
                    default:
                        break;
                }


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function saveRecords.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void UpdateRecords()
        {
            try
            {
                mtOT.FlushToDataSource();
                for (int i = 0; i < dtOverTime.Rows.Count; i++)
                {
                    string strrecordId = Convert.ToString(dtOverTime.GetValue("Id", i));
                    if (!string.IsNullOrEmpty(strrecordId))
                    {
                        TrnsSingleEntryOTDetail objDetailrecord = dbHrPayroll.TrnsSingleEntryOTDetail.Where(d => d.ID.ToString() == strrecordId).FirstOrDefault();
                        if (objDetailrecord != null)
                        {
                            objDetailrecord.Hours = Convert.ToDecimal(dtOverTime.GetValue("Hours", i));
                            objDetailrecord.Amount = Convert.ToDecimal(dtOverTime.GetValue("Amount", i));
                            string Active = dtOverTime.GetValue("Active", i);
                            if (Active == "Y")
                            {
                                objDetailrecord.FlgActive = true;
                            }
                            else
                            {
                                objDetailrecord.FlgActive = false;
                            }

                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Record Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function UpdateRecords.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostOverTimeRecords()
        {
            try
            {
                int confirm = oApplication.MessageBox("Posting Overtime is irr-reversable. Are you sure you want to post this Overtime? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                mtOT.FlushToDataSource();
                string periodId = cmbPeriod.Value;
                string overTimeid = cmbOvertime.Value;
                string strdocNum = txtdocNum.Value;
                if (string.IsNullOrEmpty(strdocNum))
                {
                    oApplication.StatusBar.SetText("Invalid document number", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(periodId) || periodId == "-1")
                {
                    oApplication.StatusBar.SetText("Please select Period month", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(overTimeid) || overTimeid == "-1")
                {
                    oApplication.StatusBar.SetText("Please select OverTime type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                CfgPeriodDates periodrecord = dbHrPayroll.CfgPeriodDates.Where(d => d.ID.ToString() == periodId).FirstOrDefault();
                MstOverTime OvertimeMaster = dbHrPayroll.MstOverTime.Where(e => e.ID.ToString() == overTimeid).FirstOrDefault();
                TrnsSingleEntryOTRequest objOtRequest = dbHrPayroll.TrnsSingleEntryOTRequest.Where(s => s.DocNum.ToString() == strdocNum).FirstOrDefault();

                for (int i = 0; i < dtOverTime.Rows.Count; i++)
                {
                    string empidError = "";
                    try
                    {
                        TrnsEmployeeOvertime objOverTime = null;
                        string empId = dtOverTime.GetValue("EmpId", i);
                        string Active = dtOverTime.GetValue("Active", i);
                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empId select p).FirstOrDefault();
                        empidError = empId;
                        if (emp != null && Active != "N")
                        {
                            objOverTime = dbHrPayroll.TrnsEmployeeOvertime.Where(d => d.EmployeeId == emp.ID && d.Period.ToString() == periodId).FirstOrDefault();
                            if (objOverTime == null)
                            {
                                objOverTime = new TrnsEmployeeOvertime();
                                dbHrPayroll.TrnsEmployeeOvertime.InsertOnSubmit(objOverTime);
                            }
                            objOverTime.Period = Convert.ToInt32(periodId);
                            objOverTime.EmployeeId = emp.ID;
                            //Create New Detail Record for this Employee
                            TrnsEmployeeOvertimeDetail objDetail = new TrnsEmployeeOvertimeDetail();
                            objOverTime.TrnsEmployeeOvertimeDetail.Add(objDetail);
                            //Filling data to detail Record
                            objDetail.OvertimeID = Convert.ToInt32(overTimeid);
                            objDetail.OTDate = periodrecord.StartDate;
                            //objDetail.OTDate = objOtRequest.CreatedDate;
                            objDetail.OTHours = Convert.ToDecimal(dtOverTime.GetValue("Hours", i));
                            objDetail.BasicSalary = emp.BasicSalary;
                            objDetail.ValueType = OvertimeMaster.ValueType;
                            objDetail.OTValue = OvertimeMaster.Value;
                            objDetail.FromTime = "";
                            objDetail.ToTime = "";
                            objDetail.Amount = Convert.ToDecimal(dtOverTime.GetValue("Amount", i));
                            if (Active == "Y")
                            {
                                objDetail.FlgActive = true;
                            }
                            else
                            {
                                objDetail.FlgActive = false;
                            }
                            objDetail.CreateDate = DateTime.Now;
                            objDetail.UpdateDate = DateTime.Now;
                            objDetail.UserId = oCompany.UserName;
                            dbHrPayroll.SubmitChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        oApplication.StatusBar.SetText("EmpID : " + empidError + " : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        continue;
                    }
                }
                if (objOtRequest != null)
                {
                    objOtRequest.DocStatus = "2";
                }
                dbHrPayroll.SubmitChanges();
                ClearControls();
                getData();
                oApplication.StatusBar.SetText("Record Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error in Function PostOverTimeRecords.Error is  " + ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadToNewRecord()
        {
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            ClearControls();
        }

        private void ClearControls()
        {
            try
            {
                txtempfrm.Value = string.Empty;
                txtempTo.Value = string.Empty;
                txtdocNum.Value = string.Empty;
                cb_dpt.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cb_loc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbOvertime.Select(1, SAPbouiCOM.BoSearchKey.psk_Index);
                cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                dtOverTime.Rows.Clear();
                mtOT.LoadFromDataSource();
                IbtnSrch.Enabled = true;
                Ibtnsave.Enabled = true;
                IbtnProc.Enabled = false;
                btnsave.Caption = "Add";
                long nextId = ds.getNextId("TrnsSingleEntryOTRequest", "ID");
                txtdocNum.Value = nextId.ToString();

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Function: ClearControls Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ClearPreviosOT()
        {
            try
            {
                if (docid != 0)
                {
                    var oDoc = (from a in dbHrPayroll.TrnsSingleEntryOTRequest where a.Id == docid select a).FirstOrDefault();
                    var oDocPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.ID == oDoc.PeriodId select a).FirstOrDefault();
                    mtOT.FlushToDataSource();
                    for (int i = 0; i < dtOverTime.Rows.Count; i++)
                    {
                        string strrecordId = Convert.ToString(dtOverTime.GetValue(clId.DataBind.Alias, i));
                        string flgActive = Convert.ToString(dtOverTime.GetValue(clActive.DataBind.Alias, i));
                        if (!string.IsNullOrEmpty(strrecordId))
                        {
                            if (string.IsNullOrEmpty(flgActive)) continue;
                            if (flgActive.ToLower().Trim() == "y")
                            {
                                TrnsSingleEntryOTDetail oOTDetail = (from a in dbHrPayroll.TrnsSingleEntryOTDetail where a.ID.ToString() == strrecordId select a).FirstOrDefault();
                                if (oOTDetail != null)
                                {
                                    var oOTList = (from a in dbHrPayroll.TrnsEmployeeOvertime
                                                   where a.MstEmployee.EmpID == oOTDetail.MstEmployee.EmpID
                                                   && a.CfgPeriodDates.ID == oDocPeriod.ID
                                                   select a).ToList();
                                    if (oOTList == null) continue;
                                    foreach (var One in oOTList)
                                    {
                                        foreach (var Line in One.TrnsEmployeeOvertimeDetail)
                                        {
                                            Line.FlgActive = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Successfully remove previos Overtime entries of selected employees.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ClearPreviosOT : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
                DtFile.Columns.Add("EmpId");
                string PayrollID = cmbPayroll.Selected.Value;
                var oPayroll = (from p in dbHrPayroll.CfgPayrollDefination
                                where p.ID == Convert.ToInt32(PayrollID)
                                select p).FirstOrDefault();
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

                    mtOT.Clear();
                    dtOverTime.Rows.Clear();
                    foreach (System.Data.DataRow dr1 in DtFile.Rows)
                    {
                        var oEmpID = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID == dr1["EmpId"].ToString()
                                      && a.PayrollID == oPayroll.ID
                                     && a.FlgOTApplicable==true
                                      select a).FirstOrDefault();
                        if (oEmpID != null)
                        {
                            dtOverTime.Rows.Add();
                            dtOverTime.SetValue("Id", LineNumber - 1, LineNumber);
                            dtOverTime.SetValue("EmpId", LineNumber - 1, dr1["EmpId"]);

                            string stremp = oEmpID.EmpID;
                            dtOverTime.SetValue("EmpName", LineNumber - 1, oEmpID.FirstName + ' ' + oEmpID.MiddleName + ' ' + oEmpID.LastName);
                            
                            LineNumber++;
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Employee ID: '" + dr1["EmpId"] + "' not found in payroll / Overtime not Applicable: '" + oPayroll.PayrollName + "' ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                    }
                    mtOT.LoadFromDataSource();
                    

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoadEmployeeToGrid : " + Ex.Message + counter + " : " + LineNumber.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

    }
}
